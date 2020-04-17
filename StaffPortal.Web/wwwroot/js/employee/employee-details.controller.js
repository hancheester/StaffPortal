(function() {
    "use strict";

    angular.module("app")
        .controller("EmployeeDetailsController",
            ["$scope","$http", "notify", EmployeeDetailsController]);

    function EmployeeDetailsController($scope, $http, notify) {
        $scope.init = function (id) {
            vm.id = id;
        };

        var vm = this;
        vm.id = 0;
        vm.employee = {};
        vm.departments = [];
        vm.roles = [];
        vm.isBusy = false;

        vm.$onInit = init;
        vm.get = get;
        vm.update = update;
        vm.getDepartments = getDepartments;
        vm.getRoles = getRoles;
        vm.toDate = toDate;
        vm.toTimeSpan = toTimeSpan;

        function init() {
            $scope.$watch("vm.id", function () {
                vm.get();
            });

            vm.getDepartments();
            vm.getRoles();
        }

        function get() {
            $http.get("/api/user/v1/employee/" + vm.id)
                .then(function (response) {
                    let data = angular.copy(response.data.value);
                    vm.employee = data;

                    vm.employee.startDate = new Date(vm.employee.startDate);
                    if (vm.employee.endDate) {
                        vm.employee.endDate = new Date(vm.employee.endDate);
                    }
                    vm.employee.dob = new Date(vm.employee.dob);

                    if (vm.employee.workingDays) {
                        vm.employee.workingDays = vm.employee.workingDays.map(function (element) {
                            if (element.startTime) {
                                element.startTime = vm.toDate(element.startTime);
                            }

                            if (element.endTime) {
                                element.endTime = vm.toDate(element.endTime);
                            }

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

                    vm.employee.primaryBusinessRole = vm.roles.find(function (element) {
                        return element.roleId === vm.employee.primaryBusinessRoleId;
                    });

                    vm.roles.forEach(function (current, index) {
                        if (vm.employee.secondaryBusinessRoleIds.includes(current.roleId)) {
                            vm.roles[index].isChecked = true;
                        }                        
                    });

                }, function (err) {
                    console.error(err);
                });
        }

        function update() {
            vm.isBusy = true;
            let data = {
                firstName: vm.employee.firstName,
                lastName: vm.employee.lastName,
                phoneNumber: vm.employee.phoneNumber,
                gender: vm.employee.gender,
                email: vm.employee.email,
                userName: vm.employee.userName,
                password: vm.employee.password,
                repeatPassword: vm.employee.repeatPassword,
                nin: vm.employee.nin,
                dob: vm.employee.dob,
                startDate: vm.employee.startDate,
                endDate: vm.employee.endDate,
                holidayAllowance: vm.employee.holidayAllowance,
                barcode: vm.employee.barcode,
                hoursRequired: vm.employee.hoursRequired,
                workingDays: vm.employee.workingDays
                    .filter(function (element) {
                        return element.isAssigned;
                    })
                    .map(function (element) {
                        return {
                            day: element.day,
                            departmentId: element.departmentId,
                            isAssigned: true,
                            startTime: vm.toTimeSpan(element.startTime),
                            endTime: vm.toTimeSpan(element.endTime)
                        };
                    }),
                primaryBusinessRoleId: vm.employee.primaryBusinessRole.roleId,
                secondaryBusinessRoleIds: vm.roles
                    .filter(function (element) {
                        return element.isChecked;
                    })
                    .map(function (element) {
                        return element.roleId;
                    })
            };

            $http.put("/api/user/v1/employee/" + vm.id, data)
                .then(function (response) {
                    notify({
                        message: "Employee updated successfully.",
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

        function getDepartments() {
            // TODO: We need to check if the department is open on this day or not.
            $http.get("/api/department/v1/departments")
                .then(function (response) {
                    let departments = angular.copy(response.data.value);
                    vm.departments = departments;
                }, function (error) {
                    console.error(error);
                });
        }

        function getRoles() {
            $http.get("/api/role/v1/roles")
                .then(function (response) {
                    let roles = angular.copy(response.data.value);

                    roles.forEach(function (element) {
                        vm.roles.push({
                            roleId: element.id,
                            name: element.name
                        });
                    });

                }, function (err) {
                    console.error(err);
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

        function toTimeSpan(date) {
            if (date instanceof Date)
                return date.getHours() + ":" + date.getMinutes() + ":00";
            return date;
        }
    }

})();