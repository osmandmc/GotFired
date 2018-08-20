(function () {
    "use strict";
    var app = angular.module("GotFired");
    app.component('myPaging', {
        templateUrl: '../app/helper/pagination.html',
        controllerAs: "vm",
        bindings: {
            pagingData: '<',
            pagingFunc: '<'
        },
        //require: {
        //    parent: '^^'
        //},
        controller: function ($routeParams) {
            var vm = this;
            vm.$onChanges = function (changes) {
                if (changes.pagingData.currentValue !== undefined) {
                    vm.pagingData = changes.pagingData.currentValue;
                    console.log(vm.pagingData);
                    vm.numbers = new Array();
                    var ceil = Math.ceil(vm.pagingData.TotalItems / vm.pagingData.ItemsPerPage);
                    for (var i = 1; i <= ceil; i++) {
                        vm.numbers.push({ value: i });
                    }
                    console.log(vm.numbers);
                }

            };

            vm.getList = function (page) {

                vm.pagingFunc(page.value);
            }
        }

    });

    // helper for myPaging component above
    function PaginateData(data, page, pageSize) {
        var dataPaginated = [];
        var length = Math.min((page * pageSize), data.length);
        var skip = (page - 1) * pageSize;
        for (var i = skip ; i < length ; i++) {

            dataPaginated.push(data[i]);
        }

        return dataPaginated;
    };
})();

