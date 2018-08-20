(function () {
    "use strict";
    var app = angular.module("GotFired", ['ngRoute', 'ui.bootstrap', 'toastr', "service"]);

    app.factory('myInterceptor', function ($injector, $q) {
        var myInterceptor = {
            responseError: function (response) {
                
                console.log(response);
                switch (response.status) {
                    case 401:
                        window.location.href = window.location.origin + "/login.html"; break; //return $q.reject(response);
                    case 500:
                        $injector.get('toastr').error('Beklenmedik bir hata oluştu.');
                        $injector.get('$location').path('/dashboard'); break;
                    case 412:
                        $injector.get('toastr').warning(response.data.StatusMessage, response.data.StatusText); break;
                    default:
                        $injector.get('toastr').error('Beklenmedik bir hata oluştu.', "Default Error");
                        $injector.get('$location').path('/dashboard'); break;
                }
                return $q.reject(response);
            }
        };
        return myInterceptor;
    });


    app.config(function ($httpProvider, $routeProvider, $locationProvider, toastrConfig) {
        //$httpProvider.interceptors['myInterceptor'];
        $httpProvider.defaults.headers.common['Cache-Control'] = 'no-cache';
        $httpProvider.defaults.headers.common['If-Modified-Since'] = 'Mon, 01 Jan 1990 05:00:00 GMT';
        $httpProvider.defaults.headers.common['Pragma'] = 'no-cache';
        $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
        $httpProvider.interceptors.push('myInterceptor');
        angular.extend(toastrConfig, {
            //allowHtml: true,
            closeButton: true,
            extendedTimeOut: 3000,
            autoDismiss: false,
            containerId: 'toast-container',
            maxOpened: 0,
            newestOnTop: true,
            positionClass: 'toast-bottom-center',
            preventDuplicates: false,
            preventOpenDuplicates: false,
            target: 'body',
            showEasing: "swing",
            hideEasing: "linear",
            showMethod: "fadeIn",
            hideMethod: "fadeOut"
        });


        //$locationProvider.html5Mode(true).hashPrefix('!');
        //if (window.history && history.pushState) {
        //    $locationProvider.html5Mode({
        //        enabled: true,
        //        requireBase: false
        //    });
        //};
        $routeProvider
        //.when('/', {
        //    templateUrl: "index.html",
        //    //controller: ' ',
        //})
        //.when('/layout', {
        //    template: "<layout></layout>",
        //})
        .when('/dashboard', {
            template: "<dashboard></dashboard>",
            resolve: {
                auth: function (authService) {
                    authService.isAuthenticated();
                }
            }
        })
        .when('/form/all', {
            template: '<form-all></form-all>',
            resolve: {
                auth: function (authService) {
                    authService.isAuthenticated();
                }
            }
        })
        .when('/form/pending', {
            template: '<form-pending></form-pending>',
            resolve: {
                auth: function (authService) {
                    authService.isAuthenticated();
                }
            }
        })
        .when('/form/answered', {
            template: '<form-answered></form-answered>',
            resolve: {
                auth: function (authService) {
                    authService.isAuthenticated();
                }
            }
        })
        .when('/form/view/:id', {
            template: '<view></view>',
            resolve: {
                auth: function (authService) {
                    authService.isAuthenticated();
                }
            }
        })

        .otherwise({ redirectTo: '/form/answered' });

    });


    app.run(function ($rootScope, authService) {
        authService.startUp();
        //$rootScope.$on("$routeChangeStart", function (event, next, current) {
            //console.log(current);
            //console.log(next);

            //console.log($rootScope);
            //if ($rootScope.currentUser == null) {
            //    // no logged user, redirect to /login
            //    if (next.templateUrl === "/login.html") {
            //    } else {
            //        $location.path("/login.html");
            //    }
            //}
        //});
    });

    //app.component('layout', {
    //    templateUrl: 'layout.html',
    //    controllerAs: "model",
    //    controller: function ($http, $routeParams) {
    //        var model = this;
    //        model.$onInit = function () {
    //            alert("");
    //        };

    //    }
    //});



})();


