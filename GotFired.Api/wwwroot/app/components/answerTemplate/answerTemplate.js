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
        controller: function (answerTemplateResource, authService, $http, $routeParams) {
            var vm = this;
            vm.$onInit = function () {
                // The ETag identifies the version being edited; it goes back in If-Match so a concurrent
                // edit is detected (412) instead of silently overwritten.
                vm.model = answerTemplateResource.getById({ id: $routeParams.id }, function (value, headers) {
                    vm.etag = headers('ETag');
                });
            };

            vm.update = function () {
                $http.put('/api/v1/answer-templates/' + vm.model.ID,
                    { Name: vm.model.Name, Content: vm.model.Content },
                    { headers: { 'Authorization': 'Bearer ' + authService.getAccessToken(), 'If-Match': vm.etag } })
                    .then(function () {
                        window.location.href = '#/answertemplate/list';
                    }, function (error) {
                        console.log(error);
                    });
            }
        }
    });
   

})();

