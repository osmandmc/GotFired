(function () {
    "use strict";
    var app = angular.module("GotFired");

    app.component('userList', {
        templateUrl: '../app/components/user/list.html',
        controllerAs: "vm",
        controller: function (userResource, $routeParams) {
            var vm = this;
            vm.$onInit = function () {
                vm.listUser(1);
            };

            vm.listUser = function (page) {
                userResource.list({ page: page || 1 },
             function (result) {
                 console.log(result.data);
                 vm.data = result.data;
                 vm.pagingData = result.paging;
             },
            function (response) {
                console.log(response);
            });
            };
            
        }
    });
    app.component('userCreate', {
        templateUrl: '../app/components/user/user-create.html',
        controllerAs: "vm",
        controller: function (userResource, $rootScope) {
            var vm = this;
            vm.data = {};
            vm.create = function () {
                console.log("data : " + vm.data);
                console.log(vm.data);

                userResource.post(vm.data, function (result) {
                    console.log(result);
                    window.location.href = '#/user/list';
                }, function (error) {
                    console.log(error);
                })
            }
        }
    });
    app.component('userEdit', {
        templateUrl: '../app/components/user/user-edit.html',
        controllerAs: "vm",
        controller: function (userResource, $rootScope, $routeParams) {
            var vm = this;
            vm.userRoles = {};
            vm.roles = [];
            vm.$onInit = function () {
                vm.roles = userResource.getAllRoles();
                console.log(vm.roles);
                vm.userRoleModel = userResource.getUserRoles({ id: $routeParams.id });
            };
            
            vm.update = function () {
                console.log("data : " + vm.userRoleModel);
                console.log(vm.userRoleModel);

                userResource.updateUserRoles(vm.userRoleModel, function (result) {
                    console.log(result);
                    window.location.href = '#/user/list';
                }, function (error) {
                    console.log(error);
                })
            }
        }
    });
    app.component('userPassword', {
        templateUrl: '../app/components/user/user-password.html',
        controllerAs: "vm",
        controller: function (userResource, $rootScope, $routeParams) {
            var vm = this;
            vm.data = {};
            vm.$onInit = function () {
                vm.data = userResource.getUserById({ id: $routeParams.id });
                
            };
            vm.changePassword = function () {
                console.log("data : " + vm.data);
                console.log(vm.data);

                userResource.changePassword(vm.data, function (result) {
                    console.log(result);
                    window.location.href = '#/user/list';
                }, function (error) {
                    console.log(error);
                })
            }
        }
    });

})();

