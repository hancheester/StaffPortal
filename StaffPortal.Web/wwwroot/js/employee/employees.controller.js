(function() {
    "use strict";

    angular.module("app")
        .controller("EmployeesController", 
        ["$scope", "$http", "notify", EmployeesController]);

    function EmployeesController($scope, $http, notify) {
        var vm = this;

        vm.employees = [];
        vm.currentPage = 1;
        vm.pageSize = 10;
        vm.totalItems = 0;
        vm.query = {};
        vm.isBusy = false;

        vm.$onInit = init;
        vm.searchEmployees = searchEmployees;
        vm.submitOnEnter = submitOnEnter;
        vm.clearFilters = clearFilters;
        vm.remove = remove;

        function init() {
            $scope.$watch("vm.currentPage + vm.pageSize", function () {
                vm.searchEmployees();
            });
        }

        function searchEmployees() {
            let params = {
                pageNumber: vm.currentPage,
                pageSize: vm.pageSize,
                id: vm.query.id ? vm.query.id : undefined,
                firstName: vm.query.firstName ? vm.query.firstName : undefined,
                lastName: vm.query.lastName ? vm.query.lastName : undefined,
                email: vm.query.email ? vm.query.email : undefined
            };

            $http.get("/api/user/v1/employees", {
                params
            }).then(function (response) {
                vm.employees = angular.copy(response.data.value.employees);
                vm.totalItems = response.data.value.total;
            }, function (err) {
                console.error(err);
            });
        }

        function clearFilters() {
            vm.query = {};
            vm.searchEmployees();
        }

        function submitOnEnter($event) {
            var keyCode = $event.which || $event.keyCode;
            if (keyCode === 13) {
                vm.searchEmployees();
            }
        }

        function remove(index) {
            let id = vm.employees[index].id;
            $http.delete("/api/user/v1/employee/" + id)
                .then(function (response) {
                    notify({
                        message: "Employee deleted successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                    vm.employees.splice(index, 1);
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator.",
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                });
        }
    }
})();