"""
Browser end-to-end test of the real pages (AngularJS admin SPA, apply.html, userView.html) against a running
stack. It drives the UI the way a person does and fails on any JavaScript error or failed /api request.

Run against the docker compose stack (WRITES DATA: use a dev or restored copy, never production):

    docker run --rm --network gotfired_default -v "$PWD/tests/e2e":/e2e -w /e2e \
      -e BASE_URL=http://api:8080 -e MAILPIT_URL=http://mailpit:8025 \
      mcr.microsoft.com/playwright/python:v1.65.2-noble python ui_test.py

Defaults: dev-admin / admin123 (seeded by the compose stack).
"""
import json
import os
import re
import sys
import urllib.request

from playwright.sync_api import expect, sync_playwright

BASE = os.environ.get("BASE_URL", "http://localhost:8080").rstrip("/")
MAILPIT = os.environ.get("MAILPIT_URL", "http://localhost:8025").rstrip("/")
USER = os.environ.get("ADMIN_USER", "dev-admin")
PASSWORD = os.environ.get("ADMIN_PASSWORD", "admin123")

results = []


def step(name):
    def wrap(fn):
        def run(*args):
            try:
                fn(*args)
                results.append((True, name, ""))
                print("PASS", name)
            except Exception as e:  # noqa: BLE001 - report every failure and continue
                results.append((False, name, str(e).splitlines()[0][:300]))
                print("FAIL", name, "->", str(e).splitlines()[0][:300])
        return run
    return wrap


def latest_link(to):
    msgs = json.load(urllib.request.urlopen(f"{MAILPIT}/api/v1/messages"))["messages"]
    msg = next(m for m in msgs if any(t["Address"] == to for t in m["To"]))
    html = json.load(urllib.request.urlopen(f"{MAILPIT}/api/v1/message/{msg['ID']}"))["HTML"]
    return re.search(r"href\s*=\s*'([^']+)'", html).group(1)


def watch(page, problems):
    page.on("pageerror", lambda e: problems.append(f"JS error: {e}"))
    page.on("response", lambda r: problems.append(f"{r.status} {r.request.method} {r.url}")
            if "/api/" in r.url and r.status >= 400 and not getattr(page, "expect_api_error", False) else None)
    page.on("dialog", lambda d: (page.dialogs.append(d.message), d.accept()))
    page.dialogs = []


with sync_playwright() as p:
    browser = p.chromium.launch()
    context = browser.new_context(base_url=BASE, viewport={"width": 1400, "height": 900})
    # apply.html redirects to the organisation's site after submitting: do not leave the test environment.
    context.route(re.compile(r"https?://(www\.)?plazaeylem\.org.*"), lambda route: route.fulfill(status=200, body="redirected"))
    problems = []
    page = context.new_page()
    watch(page, problems)
    state = {}

    @step("staff log in through login.html")
    def login():
        page.goto("/login.html")
        # login.html's own script reads the first username/password inputs.
        page.locator("input[name=username]").first.fill(USER)
        page.locator("input[name=password]").first.fill(PASSWORD)
        page.locator("button", has_text="Giriş Yap").click()
        page.wait_for_url(re.compile(r"/(index\.html)?(#.*)?$"))
        page.wait_for_function("localStorage.getItem('tempData') !== null")

    @step("case list shows cases")
    def case_list():
        page.goto("/#/form/all")
        rows = page.locator("a[href*='#/form/view/']")
        expect(rows.first).to_be_visible(timeout=15000)
        state["case_href"] = rows.first.get_attribute("href")

    @step("pending and answered lists load")
    def filtered_lists():
        for route in ("#/form/pending", "#/form/answered"):
            page.goto("/" + route)
            page.wait_for_load_state("networkidle")

    @step("case view shows the applicant's details")
    def case_view():
        page.goto("/" + state["case_href"].lstrip("/"))
        page.wait_for_load_state("networkidle")
        state["case_id"] = re.search(r"view/(\d+)", state["case_href"]).group(1)
        detail = page.evaluate("angular.element(document.querySelector('[ng-click=\"vm.postNotes()\"]')).scope().vm.data")
        assert detail and detail.get("FullName") is not None, detail
        state["applicant_email"] = detail.get("EmailAddress")
        state["comments_before"] = len(detail.get("Comments") or [])

    @step("no leftover 'test' toast on the case page")
    def no_test_toast():
        assert page.locator("#toast-container :text-is('test')").count() == 0

    @step("staff save notes")
    def notes():
        page.locator("[ng-model='vm.data.Notes']").fill("ui test note")
        page.locator("[ng-click='vm.postNotes()']").click()
        page.wait_for_load_state("networkidle")
        assert any("Notunuz" in d for d in page.dialogs), page.dialogs

    @step("staff add and remove a tag")
    def tags():
        add = page.locator("[ng-click='vm.addTag(item)']").first
        add.wait_for(timeout=10000)
        name = add.inner_text().strip()
        add.click()
        page.wait_for_load_state("networkidle")
        remove = page.locator("[ng-click='vm.removeTag(item)']", has_text=name)
        expect(remove).to_have_count(1)
        remove.click()
        page.wait_for_load_state("networkidle")
        expect(page.locator("[ng-click='vm.removeTag(item)']", has_text=name)).to_have_count(0)

    @step("staff evaluate (save tags)")
    def evaluate():
        page.locator("[ng-click='vm.evaluate()']").click()
        page.wait_for_load_state("networkidle")
        assert any("Değerlendirmeniz" in d for d in page.dialogs), page.dialogs

    @step("staff answer using a template; the comment appears")
    def answer():
        page.locator("[ng-click='vm.showAnswerEditor()']").click()
        select = page.locator("select[ng-model='vm.selectedAnswerTemplate']")
        expect(select.locator("option")).not_to_have_count(1, timeout=10000)
        select.select_option(index=1)
        page.locator("[ng-click='vm.getAnswerTemplate()']").click()
        page.wait_for_load_state("networkidle")
        page.evaluate("""() => {
            const vm = angular.element(document.querySelector('[ng-click="vm.answer()"]')).scope().vm;
            vm.data.answer.Text = (vm.data.answer.Text || '') + '<p>ui test answer</p>';
            angular.element(document.body).injector().get('$rootScope').$apply();
        }""")
        page.locator("[ng-click='vm.answer()']").click()
        page.wait_for_load_state("networkidle")
        comments = page.evaluate("angular.element(document.querySelector('[ng-click=\"vm.postNotes()\"]')).scope().vm.data.Comments")
        assert len(comments) == state["comments_before"] + 1 and "ui test answer" in comments[-1]["Text"], comments[-1:]

    @step("applicant opens the emailed link and sees the answer")
    def applicant_view():
        assert state.get("applicant_email"), "case has no applicant email"
        link = latest_link(state["applicant_email"])
        assert "/userView.html#token=" in link, link
        applicant = context.new_page()
        watch(applicant, problems)
        applicant.goto(BASE + link[link.index("/userView.html"):])
        expect(applicant.locator("#FullName")).not_to_be_empty(timeout=15000)
        expect(applicant.locator("#timeline")).to_contain_text("ui test answer")
        state["applicant"] = applicant

    @step("applicant replies through the page; the reply appears")
    def applicant_reply():
        applicant = state["applicant"]
        applicant.locator("#answerBtn").click()
        applicant.evaluate("$('#answerTextEditor').summernote('code', '<p>ui test reply</p>')")
        applicant.locator("#myForm #submitBtn, #submitBtn").first.click()
        expect(applicant.locator("#timeline")).to_contain_text("ui test reply", timeout=10000)
        applicant.close()

    @step("the reply reopened the case (pending list)")
    def reopened():
        page.goto("/#/form/pending")
        expect(page.locator(f"a[href*='#/form/view/{state['case_id']}']").first).to_be_visible(timeout=15000)

    @step("staff mark the case as read")
    def make_read():
        page.goto("/" + state["case_href"].lstrip("/"))
        page.wait_for_load_state("networkidle")
        page.locator("[ng-click='vm.makeRead()']").click()
        page.wait_for_url(re.compile(r"#/form/all"))
        assert any("okundu" in d for d in page.dialogs), page.dialogs

    @step("answer template list and edit (If-Match) save")
    def templates():
        page.goto("/#/answertemplate/list")
        edit = page.locator("a[href*='#/answertemplate/edit/']").first
        expect(edit).to_be_visible(timeout=15000)
        edit.click()
        page.wait_for_load_state("networkidle")
        page.locator("input[ng-model='vm.model.Name']").fill(page.locator("input[ng-model='vm.model.Name']").input_value())
        page.locator("[ng-click='vm.update()']").click()
        page.wait_for_url(re.compile(r"#/answertemplate/list"), timeout=10000)

    @step("user list, role editing and password pages load")
    def users():
        page.goto("/#/user/list")
        edit = page.locator("a[href*='#/user/edit/']").first
        expect(edit).to_be_visible(timeout=15000)
        user_id = re.search(r"edit/(.+)$", edit.get_attribute("href")).group(1)
        edit.click()
        page.wait_for_load_state("networkidle")
        model = page.evaluate("angular.element(document.querySelector('[ng-click=\"vm.update()\"]')).scope().vm.userRoleModel")
        assert model.get("UserName"), model
        page.goto(f"/#/user/password/{user_id}")
        page.wait_for_load_state("networkidle")

    @step("public form: a validation error is shown")
    def apply_invalid():
        form = context.new_page()
        watch(form, problems)
        form.expect_api_error = True   # the 400 is the expected outcome
        form.goto("/apply.html")
        form.wait_for_load_state("networkidle")
        # Bypass the browser's own required-field check to reach the API validation.
        form.evaluate("document.getElementById('applyForm').setAttribute('novalidate', '')")
        form.locator("#btnSubmit").click()
        form.wait_for_timeout(1500)
        assert form.dialogs and "e-posta" in form.dialogs[-1].lower(), form.dialogs
        form.close()

    @step("public form: an application is submitted")
    def apply_valid():
        form = context.new_page()
        watch(form, problems)
        form.goto("/apply.html")
        form.wait_for_load_state("networkidle")
        form.evaluate("""() => {
            for (const el of document.querySelectorAll('#applyForm [required]')) {
                if (el.tagName === 'SELECT') { el.selectedIndex = el.options.length > 1 ? 1 : 0; }
                else if (el.type === 'radio' || el.type === 'checkbox') { el.checked = true; }
                else if (el.type === 'email' || el.name === 'ApplicantEmail') { el.value = 'ui.test@example.com'; }
                else { el.value = el.value || 'UI Test'; }
            }
        }""")
        form.locator("#btnSubmit").click()
        form.wait_for_timeout(2000)
        assert any("başarıyla" in d for d in form.dialogs), form.dialogs
        form.close()

    for test in (login, case_list, filtered_lists, case_view, no_test_toast, notes, tags, evaluate, answer,
                 applicant_view, applicant_reply, reopened, make_read, templates, users, apply_invalid, apply_valid):
        test()

    @step("no JavaScript errors or failed API calls")
    def clean():
        assert not problems, problems[:5]
    clean()
    browser.close()

failed = [r for r in results if not r[0]]
print(f"\n{len(results) - len(failed)}/{len(results)} passed")
sys.exit(1 if failed else 0)
