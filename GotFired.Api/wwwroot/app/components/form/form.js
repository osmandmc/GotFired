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
                    vm.data.answer = { Text: undefined };
                });
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
                    vm.answerTemplates = result.data;
                    vm.answerTemplates.unshift({ ID: -1, Name: "Buradan cevap seçebilirsin.." });
                });

                vm.selectedAnswerTemplate = -1;
            };
            vm.addTag = function (item) {

                if (!_.some(vm.data.DismissalCaseTags, function (b) {
                    return b.ID === item.ID;
                })) {
                    dismissalViewResource.addTag({ id: $routeParams.id, tagId: item.ID }, {}, function successCallback(result) {
                        vm.data.DismissalCaseTags.push(item);
                    });

                }
            };
            vm.removeTag = function (item) {
                console.log(item);
                dismissalViewResource.removeTag({ id: $routeParams.id, tagId: item.ID }, function successCallback(result) {
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
                answerResource.post({ id: $routeParams.id, Text: vm.data.answer.Text }, function successCallback(result) {
                    vm.data.Comments.push(result);   // the stored comment (201 Created)
                    vm.data.answer = { Text: undefined };
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
                var reqGetShops = dismissalViewResource.getTags({ search: query }, function (result) {
                    deferred.resolve(result);
                });
                return deferred.promise;
            };

            vm.evaluate = function () {
                console.log(vm.data.CategoryId);
                var tagIds = vm.data.DismissalCaseTags.map(function (t) { return t.ID; });
                dismissalViewResource.setTags({ id: $routeParams.id, TagIds: tagIds }, function successCallback(result) {
                    alert("Değerlendirmeniz başarıyla kaydedilmiştir.");
                });
            };

            vm.postNotes = function () {
                dismissalViewResource.saveNotes({ id: $routeParams.id, Notes: vm.data.Notes }, function successCallback(result) {
                    alert('Notunuz başarıyla kaydedilmiştir.');
                }, function errorCallback(response) {

                });
            };
            vm.makeRead = function () {
                console.log("makereadid:" + $routeParams.id);
                dismissalViewResource.setAppealState({ id: $routeParams.id, AppealState: 2 }, function successCallback(result) {
                    alert('Vaka okundu olarak işaretlenmiştir.');
                    window.location.href = '#/form/all';
                }, function errorCallback(response) {

                });

            };
        }
    });
})();