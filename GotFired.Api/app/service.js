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

    function dismissalResource($resource, settings, authService) {
        //console.log(authService.getToken());

        return $resource("/api/v1/ ", null,
            {
                all: {
                    method: 'GET', url: "api/v1/dismissalcase/index/:page",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { page: '@page' }
                },
                pending: {
                    method: 'GET', url: "/api/v1/dismissalcase/pending/:page",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { page: '@page' }
                },
                answered: {
                    method: 'GET', url: "/api/v1/dismissalcase/answered/:page",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { page: '@page' }
                }
            });
    };

    function dismissalViewResource($resource, settings, authService) {
        return $resource("/api/v1/dismissalcase/view/:id", null,
            {
                view: {
                    method: 'GET',
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { id: '@id' }
                }, 
                evaluate: {
                    method: 'POST', url: "api/v1/dismissalCase/evaluate",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                addTag: {
                    method: 'POST', url: "api/v1/dismissalCase/addTag",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                removeTag: {
                    method: 'POST', url: "api/v1/dismissalCase/removeTag",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                postNotes: {
                    method: 'POST', url: "api/v1/dismissalCase/postNotes",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                makeRead: {
                    method: 'POST', url: "api/v1/dismissalCase/makeRead",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                    
                }, 
                getCategories: {
                    method: 'GET', url: "api/v1/dismissalCase/getCategories", isArray: true,
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }                   
                },
                getTags: {
                    method: 'GET', url: "api/v1/dismissalCase/getTags", isArray: true,
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                },
                getTagsByQuery: {
                    method: 'GET', url: "api/v1/dismissalCase/getTags/:query", isArray: true,
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { query: '@query' }
                }
                ,
                getAnswerTemplates: {
                    method: 'GET', url: "api/v1/dismissalCase/getAnswerTemplates", isArray: true,
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                },
                getAnswerTemplateContent: {
                    method: 'GET', url: "api/v1/dismissalCase/getAnswerTemplateContent/:id", 
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { id: '@id' }
                }
            });
    };

    function answerResource($resource, settings, authService) {
        return $resource("/api/v1/dismissalcase/answer/", null,
                    {
                        post:
                            {
                                headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                                method: 'POST'
                            }

                    });
    }

    function userResource($resource, settings, authService) {
        return $resource("/api/v1/auth/", null,
            {
                list: {
                    method: 'GET', url: "api/v1/auth/index/:page",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { page: '@page' }
                },
                getAllRoles: {
                    method: 'GET', url: "api/v1/auth/getAllRoles", isArray: true,
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                },
                getUserRoles: {
                    method: 'GET', url: "api/v1/auth/getUserRoles/:id",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { id: '@id' }
                },
                updateUserRoles: {
                    method: 'POST', url: "api/v1/auth/updateUserRoles",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                post: {
                    method: 'POST', url: "api/v1/auth/create",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                changePassword: {
                    method: 'POST', url: "api/v1/auth/changePassword",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                getUserById: {
                    method: 'GET', url: "api/v1/auth/getUserById/:id",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { id: '@id' }
                },


            });
    };

    function answerTemplateResource($resource, settings, authService) {
        return $resource("/api/v1/auth/", null,
            {
                list: {
                    method: 'GET', url: "api/v1/answerTemplate/list/:page",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { page: '@page' }
                },
                getById: {
                    method: 'GET', url: "api/v1/answerTemplate/getById/:id",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                },
                update: {
                    method: 'POST', url: "api/v1/answerTemplate/update",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                create: {
                    method: 'POST', url: "api/v1/answerTemplate/create",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                }
                
            });
    };

})();