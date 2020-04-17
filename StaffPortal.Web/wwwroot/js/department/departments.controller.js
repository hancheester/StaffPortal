(function () {

    "use strict";

    angular.module("app")
        .controller("DepartmentsController",
            ["$http", "notify", DepartmentsController]);

    function DepartmentsController($http, notify) {
        var vm = this;
                
        vm.departments = [];
        vm.department = null;

        vm.$onInit = init;
        vm.get = get;
        vm.remove = remove;

        function init() {
            vm.get();
        }

        function get() {
            $http.get("/api/department/v1/departments")
                .then(function (response) {
                    vm.departments = angular.copy(response.data.value);
                }, function (err) {
                    console.error(err);
                });
        }

        function remove(index) {
            let id = vm.departments[index].id;
            $http.delete("/api/department/v1/department/" + id)
                .then(function (response) {
                    notify({
                        message: "Department deleted successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                    vm.departments.splice(index, 1);
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