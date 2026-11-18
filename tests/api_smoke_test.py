#!/usr/bin/env python3
"""
End-to-end smoke test of the HTTP contract the AngularJS/jQuery front end depends on.
Calls every endpoint the way the browser pages do (login.html posts /token as text/plain,
apply.html and userView.html post jQuery-serialized forms, the admin SPA posts JSON).

Usage:  python3 tests/api_smoke_test.py [base_url] [admin_user] [admin_password] [plain_user] [plain_password]
Defaults match db/tests/legacy-ef6-schema.sql. Optional: MAILPIT_URL=http://localhost:8025 checks sent mail.
Only standard library. Writes test data; run it against a disposable database.
"""
import json, os, sys, time, urllib.parse, urllib.request, urllib.error

BASE = (sys.argv[1] if len(sys.argv) > 1 else "http://localhost:8080").rstrip("/")
ADMIN, ADMIN_PW = (sys.argv[2:4] + ["legacy-admin", "legacy-pass"][len(sys.argv[2:4]):])[:2]
USER, USER_PW = (sys.argv[4:6] + ["legacy-user", "legacy-pass"][len(sys.argv[4:6]):])[:2]
MAILPIT = os.environ.get("MAILPIT_URL")
failures = []
RUN = str(int(time.time()))  # unique suffix so the test can be rerun on the same database
TEMPLATE, NEW_USER = "Smoke " + RUN, "smoke-" + RUN


def call(method, path, body=None, token=None, content_type=None):
    data = None
    headers = {}
    if isinstance(body, (dict, list)) and content_type is None:
        data, content_type = json.dumps(body).encode(), "application/json"
    elif body is not None:
        data = body.encode() if isinstance(body, str) else body
    if content_type:
        headers["Content-Type"] = content_type
    if token:
        headers["Authorization"] = "Bearer " + token
    req = urllib.request.Request(BASE + path, data=data, method=method, headers=headers)
    try:
        with urllib.request.urlopen(req) as r:
            raw = r.read().decode()
            return r.status, (json.loads(raw) if raw else None)
    except urllib.error.HTTPError as e:
        raw = e.read().decode()
        try:
            return e.code, json.loads(raw) if raw else None
        except ValueError:
            return e.code, raw


def check(name, cond, detail=""):
    print(("PASS " if cond else "FAIL ") + name + ("" if cond else f"  -> {detail}"))
    if not cond:
        failures.append(name)


def login(user, pw):
    # login.html: XMLHttpRequest.send(string) without setRequestHeader -> text/plain
    return call("POST", "/token", f"grant_type=password&username={user}&password={pw}", content_type="text/plain;charset=UTF-8")


# --- /token ------------------------------------------------------------------------------------
s, r = login(ADMIN, ADMIN_PW)
check("token: legacy v2 password accepted", s == 200 and "access_token" in r, (s, r))
check("token: response shape (access_token, token_type, expires_in, userName)",
      s == 200 and r.get("token_type") == "bearer" and r.get("expires_in") == 86400 and r.get("userName") == ADMIN, r)
admin = r.get("access_token") if s == 200 else None
s, r = login(ADMIN, "wrong")
check("token: wrong password -> 400 invalid_grant", s == 400 and r.get("error") == "invalid_grant", (s, r))
s, r = login("no-such-user", "x")
check("token: unknown user -> 400 (was a NullReferenceException before)", s == 400, (s, r))
s, r = login(USER, USER_PW)
user = r.get("access_token") if s == 200 else None
check("token: second legacy user", user is not None, (s, r))

# --- authorization -----------------------------------------------------------------------------
s, _ = call("GET", "/api/v1/dismissalcase/index/1")
check("no token -> 401", s == 401, s)
s, _ = call("POST", "/api/v1/auth/create", {"UserName": "x", "Password": "xxxxxx"}, token=user)
check("'user' role on admin-only endpoint -> 403", s == 403, s)

# --- applicant (anonymous) ---------------------------------------------------------------------
s, r = call("GET", "/api/v1/dismissalcase/apply")
check("GET apply: lookups", s == 200 and {"guid", "cities", "supportedBys", "ageIntervals", "sgkTerminationReasons"} <= set(r), (s, r))
check("GET apply: entity JSON is PascalCase", s == 200 and r["cities"] and "Name" in r["cities"][0] and "ID" in r["cities"][0], r.get("cities") if s == 200 else r)
check("GET apply: enum lookups have display names", s == 200 and r["genders"][1]["Name"] == "Kadın", r.get("genders") if s == 200 else r)
guid = r["guid"]
city = r["cities"][0]["ID"]
form = urllib.parse.urlencode([
    ("Guid", guid), ("ApplicantFullName", "Smoke Test"), ("ApplicantEmail", "smoke@example.com"),
    ("ApplicantPhoneNumber", "(555) 555 55 55"), ("ApplicantGender", "2"), ("ApplicantAgeInterval", "2"),
    ("ApplicantEducationalState", ""), ("CityID", str(city)), ("EmploymentTerminationDate", "2026-10-01T00:00:00.000Z"),
    ("EmploymentDurationSince", "3"), ("EmployeeCount", "1"), ("DismissalState", "2"), ("WantShareHisExperience", "true"),
    ("DetailedExperience", "deneyim"), ("AppealState", "1"), ("CompanySectorName", "Yazılım"), ("SGKTerminationReasonID", ""),
])
s, r = call("POST", "/api/v1/dismissalcase/apply", form, content_type="application/x-www-form-urlencoded; charset=UTF-8")
check("POST apply (jQuery form post)", s == 200, (s, r))
s, r = call("POST", "/api/v1/dismissalcase/apply", "Guid=x&ApplicantEmail=", content_type="application/x-www-form-urlencoded; charset=UTF-8")
check("POST apply invalid -> 400 with Web API 2 ModelState shape", s == 400 and isinstance(r, dict) and "ModelState" in r, (s, r))

s, case = call("GET", f"/api/v1/dismissalcase/userview/{guid}")
check("GET userview/{guid}", s == 200 and case.get("FullName") == "Smoke Test" and case.get("GenderName") == "Kadın", (s, case))
case_id = case.get("Id") if s == 200 else None
s, r = call("POST", "/api/v1/dismissalcase/useranswer",
            urllib.parse.urlencode({"DismissalCaseId": case_id, "Text": "<p>soru</p>", "UserName": "Smoke Test", "Guid": guid}),
            content_type="application/x-www-form-urlencoded; charset=UTF-8")
check("POST useranswer (form) returns the comment", s == 200 and r.get("Text") == "<p>soru</p>" and r.get("ID", 0) > 0, (s, r))

# --- admin: cases --------------------------------------------------------------------------------
for path in ["index/1", "pending/1", "answered/1"]:
    s, r = call("GET", f"/api/v1/dismissalcase/{path}", token=admin)
    check(f"GET {path}: data + paging", s == 200 and "data" in r and r["paging"]["ItemsPerPage"] == 20, (s, r))
s, r = call("GET", "/api/v1/dismissalcase/pending/1", token=admin)
check("pending list contains the new case", s == 200 and any(c["Id"] == case_id for c in r["data"]), r)
existing = []
for p in (1, 2):
    s, r = call("GET", f"/api/v1/dismissalcase/index/{p}", token=admin)
    existing += [c["Id"] for c in r["data"]] if s == 200 else []
existing = existing[::-1]  # oldest first: older cases are more likely to have supporters and comments
views = [call("GET", f"/api/v1/dismissalcase/view/{i}", token=admin) for i in existing[:20]]
rich = [v for st, v in views if st == 200 and v.get("SupportedByNames") and v.get("Comments")]
check("GET view/{id} on existing rows (city, supporters, comments via includes)",
      all(st == 200 for st, _ in views) and rich and all(v.get("CityName") for v in rich), [st for st, _ in views])
s, r = call("GET", "/api/v1/dismissalcase/view/999999", token=admin)
check("GET view/missing -> 400 {Message}", s == 400 and r.get("Message") == "model cannot be null", (s, r))

s, r = call("GET", "/api/v1/dismissalcase/getAnswerTemplates", token=admin)
check("getAnswerTemplates", s == 200 and len(r) >= 1 and "Name" in r[0], (s, r))
s, r = call("GET", f"/api/v1/dismissalcase/getAnswerTemplateContent/{r[0]['ID']}", token=admin)
check("getAnswerTemplateContent", s == 200 and "Content" in r, (s, r))
s, r = call("GET", "/api/v1/dismissalcase/getCategories", token=admin)
check("getCategories", s == 200 and isinstance(r, list), (s, r))
s, r = call("GET", "/api/v1/dismissalcase/getTags/taz", token=admin)
check("getTags/{query}", s == 200 and any(t["Name"] == "tazminat" for t in r), (s, r))
tag_id = r[0]["ID"] if s == 200 and r else None

s, _ = call("POST", "/api/v1/dismissalcase/evaluate", {"DismissalCaseId": case_id, "CategoryId": 0, "Tags": [{"ID": tag_id, "Name": "tazminat"}]}, token=admin)
check("POST evaluate", s == 200, s)
s, r = call("GET", f"/api/v1/dismissalcase/view/{case_id}", token=admin)
check("evaluate stored the tag", s == 200 and [t["ID"] for t in r["DismissalCaseTags"]] == [tag_id], r)
s, _ = call("POST", "/api/v1/dismissalcase/removeTag", {"DismissalCaseID": case_id, "TagID": tag_id}, token=admin)
check("POST removeTag", s == 200, s)
s, _ = call("POST", "/api/v1/dismissalcase/addTag", {"DismissalCaseID": case_id, "TagID": tag_id}, token=admin)
check("POST addTag", s == 200, s)
s, _ = call("POST", "/api/v1/dismissalcase/postNotes", {"notes": "not", "dismissalCaseId": str(case_id)}, token=admin)
check("POST postNotes (camelCase keys, string id as the SPA sends)", s == 200, s)
s, _ = call("POST", "/api/v1/dismissalcase/answer",
            {"DismissalCaseId": str(case_id), "EmailAddress": "smoke@example.com", "Guid": guid, "UserName": ADMIN,
             "CreatedDate": "2026-11-18T10:00:00.000Z", "Text": "<p>cevap</p>"}, token=admin)
check("POST answer (sends applicant email)", s == 200, s)
s, r = call("GET", f"/api/v1/dismissalcase/view/{case_id}", token=admin)
check("answer stored comment + note", s == 200 and len(r["Comments"]) == 2 and r["Notes"] == "not", r)
s, r = call("GET", "/api/v1/dismissalcase/answered/1", token=admin)
check("case moved to answered", s == 200 and any(c["Id"] == case_id for c in r["data"]), r)
s, _ = call("POST", "/api/v1/dismissalcase/makeRead", {"Id": case_id}, token=admin)
check("POST makeRead", s == 200, s)

# --- admin: answer templates ---------------------------------------------------------------------
s, _ = call("POST", "/api/v1/answerTemplate/create", {"Name": TEMPLATE, "Content": "<p>x</p>"}, token=admin)
check("answerTemplate create", s == 200, s)
s, r = call("GET", "/api/v1/answerTemplate/list/1", token=admin)
check("answerTemplate list", s == 200 and any(t["Name"] == TEMPLATE for t in r["data"]), (s, r))
tid = next(t["ID"] for t in r["data"] if t["Name"] == TEMPLATE)
s, t = call("GET", f"/api/v1/answerTemplate/getById/{tid}", token=admin)
check("answerTemplate getById", s == 200 and t["Content"] == "<p>x</p>", (s, t))
t["Content"] = "<p>y</p>"
s, _ = call("POST", "/api/v1/answerTemplate/update", t, token=admin)
check("answerTemplate update (RowVersion check)", s == 200, s)
s, _ = call("POST", "/api/v1/answerTemplate/update", t, token=admin)
check("answerTemplate stale update rejected (RowVersion changed)", s == 500, s)

# --- admin: users ---------------------------------------------------------------------------------
s, r = call("GET", "/api/v1/auth/index/1", token=admin)
check("auth index", s == 200 and r["paging"]["TotalItems"] >= 2 and any(u["UserName"] == ADMIN and u["RoleNames"] == ["admin"] for u in r["data"]), (s, r))
s, r = call("GET", "/api/v1/auth/getAllRoles", token=admin)
check("auth getAllRoles", s == 200 and set(r) >= {"admin", "editor", "user"}, (s, r))
s, _ = call("POST", "/api/v1/auth/create", {"UserName": NEW_USER, "Password": "abcdef", "FriendlyName": "Smoke"}, token=admin)
check("auth create (6-char password, v2 rules)", s == 200, s)
new_id, page = None, 1
while new_id is None:
    s, r = call("GET", f"/api/v1/auth/index/{page}", token=admin)
    if s != 200 or not r["data"]:
        break
    new_id = next((u["UserId"] for u in r["data"] if u["UserName"] == NEW_USER), None)
    page += 1
check("auth index paging finds the new user", new_id is not None, page)
s, _ = call("POST", "/api/v1/auth/updateUserRoles", {"UserName": NEW_USER, "FriendlyName": "Smoke 2", "UserRoles": ["editor"]}, token=admin)
check("auth updateUserRoles", s == 200, s)
s, r = call("GET", f"/api/v1/auth/getUserRoles/{new_id}", token=admin)
check("auth getUserRoles", s == 200 and r["UserRoles"] == ["editor"] and r["FriendlyName"] == "Smoke 2", (s, r))
s, _ = call("POST", "/api/v1/auth/changePassword", {"UserName": NEW_USER, "Password": "newpass"}, token=admin)
check("auth changePassword", s == 200, s)
s, r = login(NEW_USER, "newpass")
check("new user logs in with changed password, editor role works", s == 200 and call("GET", "/api/v1/dismissalcase/index/1", token=r["access_token"])[0] == 200, (s, r))

# --- unhandled exception ----------------------------------------------------------------------------
s, _ = call("POST", "/api/v1/dismissalcase/makeRead", {"Id": 999999}, token=admin)
check("unhandled exception -> 500", s == 500, s)

if MAILPIT:
    with urllib.request.urlopen(MAILPIT.rstrip("/") + "/api/v1/messages") as resp:
        subjects = [m["Subject"] for m in json.load(resp)["messages"]]
    check("mail: applicant link email sent", "İşten Atıldım Başvurunuz Hk." in subjects, subjects)
    check("mail: exception email sent", any(sj.startswith("Exception:") for sj in subjects), subjects)

print(f"\n{len(failures)} failure(s)" if failures else "\nALL PASSED")
sys.exit(1 if failures else 0)
