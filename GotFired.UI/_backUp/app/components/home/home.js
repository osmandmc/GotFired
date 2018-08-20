(function () {
    "use strict";
    var app = angular.module("GotFired");


    app.component('home', {
        templateUrl: '../app/components/home/home.html',
        controllerAs: "vm",
        controller: function ($http, $routeParams) {
            var vm = this;
            vm.$onInit = function () {
                vm.message = "Home !";
            };

        }
    });

})();