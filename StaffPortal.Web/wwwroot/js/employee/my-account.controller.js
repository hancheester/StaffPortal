(function() {
    "use strict";

    angular.module("app")
        .controller("MyAccountController",
        ["$scope", "$http", "notify", MyAccountController]);

    function MyAccountController($scope, $http, notify) {
        $scope.init = function (id) {
            vm.id = id;
        };

        var vm = this;
        vm.id = 0;
        vm.employee = {};
        vm.departments = [];
        vm.isBusy = false;

        vm.$onInit = init;
        vm.update = update;
        vm.get = get;
        vm.getDepartments = getDepartments;
        vm.toDate = toDate;

        function init() {
            $scope.$watch("vm.id", function () {
                vm.get();
            });

            vm.getDepartments();
        }

        function update() {
            vm.isBusy = true;
            let data = {
                firstName: vm.employee.firstName,
                lastName: vm.employee.lastName,
                phoneNumber: vm.employee.phoneNumber,
                gender: vm.employee.gender,
                email: vm.employee.email,                
                password: vm.employee.password,
                repeatPassword: vm.employee.repeatPassword,
                nin: vm.employee.nin                
            };

            $http.put("/api/user/v1/my-account/" + vm.id, data)
                .then(function (response) {
                    notify({
                        message: "Account updated successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator. " + err.data.value.message,
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.isBusy = false;
                });
        }

        function get() {
            vm.isBusy = true;

            $http.get("/api/user/v1/employee/" + vm.id)
                .then(function (response) {
                    let data = angular.copy(response.data.value);
                    vm.employee = data;

                    if (vm.employee.workingDays) {
                        vm.employee.workingDays = vm.employee.workingDays.map(function (element) {
                            
                            if (element.departmentId) {
                                element.department = vm.departments.find(function (department) {
                                    return department.id === element.departmentId;
                                });
                            }

                            return element;
                        });
                    } else {
                        let week = [
                            "Monday",
                            "Tuesday",
                            "Wednesday",
                            "Thursday",
                            "Friday",
                            "Saturday",
                            "Sunday"
                        ];

                        for (var i = 0; i < week.length; i++) {
                            vm.employee.workingDays.push({
                                day: week[i],
                                isAssigned: false,
                                stateTime: "",
                                endTime: "",
                                departmentId: 0
                            });
                        }
                    }

                }, function (err) {
                    console.error(err);
                })
                .then(function () {
                    vm.isBusy = false;
                });
        }

        function getDepartments() {
            $http.get("/api/department/v1/departments")
                .then(function (response) {
                    let departments = angular.copy(response.data.value);
                    vm.departments = departments;
                }, function (error) {
                    console.error(error);
                });
        }

        function toDate(string) {
            var hh = string.substring(0, 2);
            var mm = string.substring(3, 5);

            if (hh === "00" && mm === "00")
                return null;

            var time = new Date();
            time.setHours(hh, mm, "00");
            return time;
        }
    }
})();