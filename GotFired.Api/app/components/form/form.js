(function () {
    "use strict";
    var app = angular.module("GotFired");

    app.component('formAll', {
        templateUrl: '../app/components/form/form-pending.html',
        controllerAs: "vm",
        controller: function (dismissalResource, $routeParams) {
            var vm = this;
            vm.$onInit = function () {
                vm.listformAnswered(1);
            };

            vm.listformAnswered = function (page) {
                dismissalResource.all({ page: page || 1 },
                    function (result) {
                        console.log(result);
                        vm.data = result.data;
                        vm.pagingData = result.paging;
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
        controller: function (dismissalResource, $routeParams) {
            var vm = this;
            vm.$onInit = function () {
                vm.listformAnswered(1);
            };
            vm.viewCase = function (id) {
                window.location ="#/form/view/" + id
            }
            vm.listformAnswered = function (page) {
                dismissalResource.pending({ page: page || 1 },
                    function (result) {
                        console.log(result);
                        vm.data = result.data;
                        vm.pagingData = result.paging;
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
                dismissalResource.answered({ page: page || 1 },
                    function (result) {
                        console.log(result);
                        vm.data = result.data;
                        vm.pagingData = result.paging;
                        vm.data.header = 'Cevaplanan Vakalar';
                    },
                    function (response) {
                        console.log(response);
                    });
            };
        }
    });
    app.component('view', {
        templateUrl: '../app/components/form/view.html',
        controllerAs: "vm",
        controller: function (dismissalViewResource, answerResource, $routeParams, $uibModal, $http, $rootScope, $q, toastr) {
            var vm = this;
            vm.$onInit = function () {
                vm.data = dismissalViewResource.view({ id: $routeParams.id }, function (result) {
                    vm.data.answer = { DismissalCaseId: $routeParams.id, EmailAddress: vm.data.EmailAddress, Guid: vm.data.Guid, UserName: $rootScope.currentUser.userName, CreatedDate: new Date() };

                });
                console.log(vm.data);
                toastr.info("test");
                //dismissalViewResource.getCategories(function (result) {
                //    vm.categories = result;
                //    //vm.categories.unshift({ ID: '', Name: "seçiniz..." });
                //});


                dismissalViewResource.getTags(function (result) {
                    vm.Tags = result;
                    console.log(result);
                });

            };
            vm.showAnswerEditor = function () {
                vm.showAnswer = true;
                dismissalViewResource.getAnswerTemplates(function (result) {
                    vm.answerTemplates = result;
                    vm.answerTemplates.unshift({ ID: -1, Name: "Buradan cevap seçebilirsin.." });
                });

                vm.selectedAnswerTemplate = -1;
            };
            vm.addTag = function (item) {

                if (!_.some(vm.data.DismissalCaseTags, function (b) {
                    return b.ID === item.ID;
                })) {
                    dismissalViewResource.addTag({ DismissalCaseID: $routeParams.id, TagID: item.ID }, function successCallback(result) {
                        vm.data.DismissalCaseTags.push(item);
                    });

                }
            };
            vm.removeTag = function (item) {
                console.log(item);
                dismissalViewResource.removeTag({ DismissalCaseID: $routeParams.id, TagID: item.ID }, function successCallback(result) {
                    var index = vm.data.DismissalCaseTags.indexOf(item);
                    vm.data.DismissalCaseTags.splice(index, 1);
                    console.log(vm.data.DismissalCaseTags);
                });

            };
            vm.getAnswerTemplate = function () {
                console.log("selected: " + vm.selectedAnswerTemplate);
                if (vm.selectedAnswerTemplate != -1) {
                    dismissalViewResource.getAnswerTemplateContent({ id: vm.selectedAnswerTemplate }, function successCallback(result) {
                        var res = result.Content;
                        console.log(vm.data.answer.Text);
                        if (vm.data.answer.Text != undefined)
                            vm.data.answer.Text += result.Content;
                        else
                            vm.data.answer.Text = result.Content;

                    });


                }
            };
            vm.answer = function () {
                vm.isDisabled = true;
                console.log(vm.data.answer);
                answerResource.post(vm.data.answer, function successCallback(result) {
                    vm.data.Comments.push(vm.data.answer);
                    console.log(vm.data.Comments);
                    vm.showAnswer = false;
                    vm.isDisabled = false;
                    // this callback will be called asynchronously
                    // when the response is available
                }, function errorCallback(response) {
                    // called asynchronously if an error occurs
                    // or server returns response with an error status.
                });
            };
            vm.cancel = function () {
                console.log("close");
                vm.showAnswer = false;
            };
            vm.loadTags = function (query) {
                var deferred = $q.defer();
                var reqGetShops = dismissalViewResource.getTags({ query: query }, function (result) {
                    deferred.resolve(result);
                });
                return deferred.promise;
            };

            vm.evaluate = function () {
                console.log(vm.data.CategoryId);
                vm.data.EvaluationModel = { DismissalCaseId: $routeParams.id, CategoryId: vm.data.CategoryId, Tags: vm.data.DismissalCaseTags };
                dismissalViewResource.evaluate(vm.data.EvaluationModel, function successCallback(result) {
                    alert("Değerlendirmeniz başarıyla kaydedilmiştir.");
                });
            };

            vm.postNotes = function () {
                vm.data.NoteModel = { notes: vm.data.Notes, dismissalCaseId: $routeParams.id }
                dismissalViewResource.postNotes(vm.data.NoteModel, function successCallback(result) {
                    alert('Notunuz başarıyla kaydedilmiştir.');
                }, function errorCallback(response) {

                });
            };
            vm.makeRead = function () {
                console.log("makereadid:" + $routeParams.id);
                var dismissalCaseObject = { id: $routeParams.id };
                dismissalViewResource.makeRead(dismissalCaseObject, function successCallback(result) {
                    alert('Vaka okundu olarak işaretlenmiştir.');
                    window.location.href = '#/form/all';
                }, function errorCallback(response) {

                });

            };
        }
    });
})();