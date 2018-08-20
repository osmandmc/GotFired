(function () {
    "use strict";
    var app = angular.module("GotFired");


    app.component('header', {
        templateUrl: '../app/components/theme/header.html',
        controllerAs: "vm",
        controller: function ($http,authService) { //$routeParams, productResource) {
            var vm = this;
            vm.$onInit = function () {
                Layout.initHeader(); // init header
                vm.userName = authService.getUser().userName;
            };

            vm.logout = function () {
                authService.logOut();
            };
        }
    });

    app.component('sidebar', {
        templateUrl: '../app/components/theme/sidebar.html',
        controllerAs: "model",
        controller: function ($http) { //$routeParams, productResource) {
            var model = this;
            model.$onInit = function () {
                Layout.initSidebar(); // init sidebar
            };
        }
    });

    //app.component('pageHead', {
    //    templateUrl: '../app/components/theme/page-head.html',
    //    controllerAs: "model",
    //    controller: function ($http) { //$routeParams, productResource) {
    //        var model = this;
    //        model.$onInit = function () {
    //            Demo.init(); // init theme panel
    //        };
    //    }
    //});

    app.component('quickSidebar', {
        templateUrl: '../app/components/theme/quick-sidebar.html',
        controllerAs: "model",
        controller: function ($http) { //$routeParams, productResource) {
            var model = this;
            model.$onInit = function () {
                setTimeout(function () {
                    QuickSidebar.init(); // init quick sidebar        
                }, 2000)

                //$http.get("http://localhost:4400/api/product/1").then(function (res) {
                //    console.log(res);
                //    console.log("heyy");
                //    model.data = res.data
                //})
            };
        }
    });


    app.component('footer', {
        templateUrl: '../app/components/theme/footer.html',
        controllerAs: "model",
        controller: function ($http ) { //$routeParams, productResource) {
            var model = this;
            model.$onInit = function () {
                Layout.initFooter(); // init footer
            };
        }
    });
 
   


 



})();