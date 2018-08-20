(function () {
    "use strict";
    var app = angular.module("GotFired");

    app.component('answertemplateList', {
        templateUrl: '../app/components/answerTemplate/list.html',
        controllerAs: "vm",
        controller: function (answerTemplateResource, $routeParams) {
            var vm = this;
            vm.$onInit = function () {
                console.log("cevap");
                vm.listAnswerTemplate(1);
            };

            vm.listAnswerTemplate = function (page) {
                answerTemplateResource.list({ page: page || 1 },
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
    app.component('answertemplateCreate', {
        templateUrl: '../app/components/answerTemplate/create.html',
        controllerAs: "vm",
        controller: function (answerTemplateResource, $rootScope) {
            var vm = this;
            vm.data = {};
            vm.create = function () {
                console.log("data : " + vm.data);
                console.log(vm.data);

                answerTemplateResource.create(vm.data, function (result) {
                    console.log(result);
                    window.location.href = '#/answertemplate/list';
                }, function (error) {
                    console.log(error);
                })
            }
        }
    });
    app.component('answertemplateEdit', {
        templateUrl: '../app/components/answerTemplate/edit.html',
        controllerAs: "vm",
        controller: function (answerTemplateResource, $rootScope, $routeParams) {
            var vm = this;
            vm.$onInit = function () {
                vm.model = answerTemplateResource.getById({ id: $routeParams.id });
            };

            vm.update = function () {
                console.log("data : " + vm.model);
                console.log(vm.model);

                answerTemplateResource.update(vm.model, function (result) {
                    console.log(result);
                    window.location.href = '#/answertemplate/list';
                }, function (error) {
                    console.log(error);
                })
            }
        }
    });
   

})();

