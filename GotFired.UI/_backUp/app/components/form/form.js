(function () {
    "use strict";
    var app = angular.module("GotFired");

    app.component('formAll', {
        templateUrl: '../app/components/form/form-pending.html',
        controllerAs: "vm",
        controller: function (dismissalResource, $routeParams, toastr) {
            var vm = this;
            vm.$onInit = function () {
                vm.data = {};
                dismissalResource.all(
                  function (data) {
                      console.log(data);
                      vm.data = data;
                      vm.data.header = 'Tüm Vakalar';
                  },
                 function (response) {
                     console.log(response);
                 });
            };

        }
    });


    app.component('formPending', {
        templateUrl: '../app/components/form/form-pending.html',
        controllerAs: "vm",
        controller: function (dismissalResource, $routeParams, toastr) {
            var vm = this;
            vm.$onInit = function () {
                vm.data = {};
                dismissalResource.pending(
                  function (data) {
                      console.log(data);
                      vm.data = data;
                      vm.data.header = 'Bekleyen Vakalar';
                  },
                 function (response) {
                     console.log(response);
                 });
            };

        }
    });

    app.component('formAnswered', {
        templateUrl: '../app/components/form/form-pending.html',
        controllerAs: "vm",
        controller: function (dismissalResource, $routeParams) {
            var vm = this;
            vm.$onInit = function () {
                vm.listformAnswered(1);
            };

            vm.listformAnswered = function (page) {
                dismissalResource.answered( { page: page || 1 },
             function (result) {
                 console.log(result);
                 vm.data = result.data;
                 vm.pagingData = result.paging;
                 vm.data.header = 'Cevaplanan Vakalar';
             },
            function (response) {
                console.log(response);
            });
            }
        }
    });
    app.component('view', {
        templateUrl: '../app/components/form/view.html',
        controllerAs: "vm",
        controller: function (dismissalViewResource, answerResource, $routeParams, $uibModal, $http, $rootScope) {
            var vm = this;
            vm.$onInit = function () {
                vm.data = dismissalViewResource.view({ id: $routeParams.id }, function (result) {
                    console.log(result);
                });

            };
            vm.openModal = function () {
                console.log("open");
                var modalInstance = $uibModal.open({
                    //template: '<answer></answer>'
                    backdrop: 'static',
                    templateUrl: '../app/components/form/answer.html',
                    controllerAs: 'subModel',
                    controller: function () {

                        var subModel = this;
                        subModel.data = {};
                        subModel.$onInit = function () {

                            console.log($rootScope.currentUser);
                            subModel.data.answer = { DismissalCaseId: $routeParams.id, EmailAddress: vm.data.EmailAddress, Guid: vm.data.Guid, UserName: $rootScope.currentUser.userName, CreatedDate: new Date() };
                        }
                        subModel.answer = function () {
                            answerResource.post(subModel.data.answer, function successCallback(result) {

                                modalInstance.close();
                                vm.data.Comments.push(subModel.data.answer);
                                console.log(vm.data.Comments);
                                // this callback will be called asynchronously
                                // when the response is available
                            }, function errorCallback(response) {
                                // called asynchronously if an error occurs
                                // or server returns response with an error status.
                            });
                        };

                        subModel.cancel = function () {
                            console.log("close");
                            modalInstance.dismiss('close');
                        };
                    }
                });
            }
        }
    });
})();