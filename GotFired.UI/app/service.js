(function () {
    "use strict";
    angular.module("service", ['ngResource'])
    .constant("settings", {
        iDserverPath: 'http://istenatildimdestek.tk',
        //idServerSslPath: 'https://localhost:44394',
        apiServerPath: 'http://istenatildimdestek.tk',
        uiServerPath: 'http://localhost:54483',
        grantType: 'password'
    })
    //.factory("pagingResource", ["$resource", "settings", pagingResource])
    .factory("authService", ["settings", "$rootScope", authService])
    .factory("dismissalResource", ["$resource", "settings", "authService", dismissalResource])
    .factory("dismissalViewResource", ["$resource", "settings", "authService", dismissalViewResource])
    .factory("answerResource", ["$resource", "settings", "authService", answerResource]);



    function authService(settings, $rootScope, $resource) {
        var authObj = {};

        authObj.getUser = function () {
            
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
            if ($rootScope.currentUser === undefined || $rootScope.currentUser === null) {
                window.location.href = window.location.origin + "/login.html";
            }
            return true;
        };

        //authObj.setToken = function (token) {
        //    authObj.token = token;
        //};

        //var getToken = function (userName, password) {
        //    debugger;
        //    if (authObj.token === "") {
        //        if (localStorage.getItem("access_token") === null) {
        //            var url = settings.iDserverPath + "/token?" +
        //                "grant_type=" + settings.grantType +
        //                "&username=" + userName +
        //                "&password=" + password
        //        }
        //        else {
        //            authObj.setToken(localStorage["access_token"]);
        //        }
        //    }
        //    return container;
        //};

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

    //function pagingResource($resource,) {
    //    return $resource(settings.apiServerPath + "/api/v1/dismissalcase/view/:id", null,
    //      {
    //          view: {
    //              method: 'GET',
    //              headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
    //              params: { id: '@id' }
    //          }
    //      });
    //};

    function dismissalResource($resource, settings, authService) {
        //console.log(authService.getToken());

        return $resource(settings.apiServerPath + "/api/v1/dismissalcase", null,
            {
                all:{
                    method: 'GET', url: settings.apiServerPath + "/api/v1/dismissalcase/index", isArray: true,
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                pending: {
                    method: 'GET', url: settings.apiServerPath + "/api/v1/dismissalcase/pending", isArray: true,
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() }
                },
                answered: {
                    method: 'GET', url: settings.apiServerPath + "/api/v1/dismissalcase/answered/:page",
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { page: '@page' },
                }
            });
    };

    function dismissalViewResource($resource, settings, authService) {
        return $resource(settings.apiServerPath + "/api/v1/dismissalcase/view/:id", null,
            {
                view: {
                    method: 'GET',
                    headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                    params: { id: '@id' }
                }
            });
    };

    function answerResource($resource, settings, authService) {
        return $resource(settings.apiServerPath + "/api/v1/dismissalcase/answer/", null,
                    {
                        post:
                            {
                                headers: { 'Authorization': 'Bearer ' + authService.getAccessToken() },
                                method: 'POST',
                            }

                    })
    };








})();