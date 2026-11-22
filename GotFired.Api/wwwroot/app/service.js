(function () {
    "use strict";
    angular.module("service", ['ngResource'])
    .constant("settings", {
        //iDserverPath: 'http://istenatildimdestek.tk',
        //idServerSslPath: 'https://localhost:44394',
        //apiServerPath: 'http://istenatildimdestek.tk',
        //uiServerPath: 'http://localhost:54483',
        grantType: 'password'
    })
    //.factory("pagingResource", ["$resource", "settings", pagingResource])
    .factory("authService", ["settings", "$rootScope", authService])
    .factory("dismissalResource", ["$resource", "settings", "authService", dismissalResource])
    .factory("dismissalViewResource", ["$resource", "settings", "authService", dismissalViewResource])
    .factory("answerResource", ["$resource", "settings", "authService", answerResource])
    .factory("userResource", ["$resource", "settings", "authService", userResource])
    .factory("answerTemplateResource", ["$resource", "settings", "authService", answerTemplateResource]);



    function authService(settings, $rootScope, $resource) {
        var authObj = {};

        authObj.getUser = function () {
            console.log($rootScope.currentUser);
            return $rootScope.currentUser;
        };
        authObj.logOut = function () {
            localStorage.removeItem("tempData");
            $rootScope.currentUser = null;
            window.location.href = window.location.origin + "/login.html";
            console.log($rootScope);
        };
        //authObj.isInRole = function (role) {
        //    if (authObj.isAuthenticated) {

        //    }
        //};
        authObj.startUp = function () {
            if (localStorage.getItem("tempData") === null) {
                window.location.href = window.location.origin + "/login.html";
            };
            var temp = angular.fromJson(localStorage.getItem("tempData"));
            $rootScope.currentUser = { userName: temp.userName }
        };

        authObj.isAuthenticated = function () {
            console.log("user: " + $rootScope.currentUser);
            if ($rootScope.currentUser === undefined || $rootScope.currentUser === null) {
                //console.log("login: " + $rootScope.currentUser);
                window.location.href = window.location.origin + "/login.html";
            }
            return true;
        };


        authObj.getAccessToken = function () {
            if (localStorage.getItem("tempData") === null) {
                window.location.href = window.location.origin + "/login.html";
            }
            else {
                return angular.fromJson(localStorage.getItem("tempData")).access_token;
            }
        };

        return authObj;
    };

    // REST API (see GotFired.Api/Features/Endpoints.cs). Each function keeps the action names the components use.
    function auth(authService) {
        return { 'Authorization': 'Bearer ' + authService.getAccessToken() };
    }

    function dismissalResource($resource, settings, authService) {
        return $resource("/api/v1/cases", null,
            {
                all: { method: 'GET', headers: auth(authService), params: { page: '@page' } },
                pending: { method: 'GET', headers: auth(authService), params: { page: '@page', state: 'pending' } },
                answered: { method: 'GET', headers: auth(authService), params: { page: '@page', state: 'answered' } }
            });
    };

    function dismissalViewResource($resource, settings, authService) {
        return $resource("/api/v1/cases/:id", { id: '@id' },
            {
                view: { method: 'GET', headers: auth(authService) },
                // body: { TagIds: [..] }
                setTags: { method: 'PUT', url: "/api/v1/cases/:id/tags", headers: auth(authService) },
                addTag: { method: 'PUT', url: "/api/v1/cases/:id/tags/:tagId", params: { tagId: '@tagId' }, headers: auth(authService) },
                removeTag: { method: 'DELETE', url: "/api/v1/cases/:id/tags/:tagId", params: { tagId: '@tagId' }, headers: auth(authService) },
                // body: { Notes }
                saveNotes: { method: 'PUT', url: "/api/v1/cases/:id/notes", headers: auth(authService) },
                // body: { AppealState: 2 }
                setAppealState: { method: 'PUT', url: "/api/v1/cases/:id/appeal-state", headers: auth(authService) },
                getCategories: { method: 'GET', url: "/api/v1/categories", isArray: true, headers: auth(authService) },
                getTags: { method: 'GET', url: "/api/v1/tags", isArray: true, params: { search: '@query' }, headers: auth(authService) },
                getAnswerTemplates: { method: 'GET', url: "/api/v1/answer-templates", params: { pageSize: 100 }, headers: auth(authService) },
                getAnswerTemplateContent: { method: 'GET', url: "/api/v1/answer-templates/:id", headers: auth(authService) }
            });
    };

    function answerResource($resource, settings, authService) {
        // body: { Text }
        return $resource("/api/v1/cases/:id/comments", { id: '@id' },
            { post: { method: 'POST', headers: auth(authService) } });
    }

    function userResource($resource, settings, authService) {
        return $resource("/api/v1/users/:id", { id: '@Id' },
            {
                list: { method: 'GET', url: "/api/v1/users", params: { page: '@page' }, headers: auth(authService) },
                getAllRoles: { method: 'GET', url: "/api/v1/roles", isArray: true, headers: auth(authService) },
                // GET /users/:id returns { Id, UserName, FriendlyName, UserRoles }
                get: { method: 'GET', headers: auth(authService) },
                // body: { FriendlyName, UserRoles }
                update: { method: 'PUT', headers: auth(authService) },
                post: { method: 'POST', url: "/api/v1/users", headers: auth(authService) },
                // body: { Password }
                changePassword: { method: 'PUT', url: "/api/v1/users/:id/password", headers: auth(authService) }
            });
    };

    function answerTemplateResource($resource, settings, authService) {
        // Updates need the ETag from GET in If-Match: see answerTemplateEdit (uses $http).
        return $resource("/api/v1/answer-templates/:id", { id: '@ID' },
            {
                list: { method: 'GET', url: "/api/v1/answer-templates", params: { page: '@page' }, headers: auth(authService) },
                getById: { method: 'GET', headers: auth(authService) },
                create: { method: 'POST', url: "/api/v1/answer-templates", headers: auth(authService) }
            });
    };
})();
