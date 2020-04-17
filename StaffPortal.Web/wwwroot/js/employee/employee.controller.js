(function() {
    "use strict";

    angular.module("app")
        .controller("EmployeeController",
            ["$http", "notify", EmployeeController]);

    function EmployeeController($http, notify) {
        var vm = this;

        vm.employee = {
            workingDays: [],
            primaryBusinessRole: {}
        };
        vm.departments = [];
        vm.roles = [];
        vm.isBusy = false;
        vm.isDobOpen = false;
        vm.isStartDateOpen = false;
        vm.isEndDateOpen = false;

        vm.$onInit = init;
        vm.getDepartments = getDepartments;
        vm.getRoles = getRoles;
        vm.create = create;
        vm.toTimeSpan = toTimeSpan;

        function init() {
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

            vm.getDepartments();
            vm.getRoles();
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
                            name: element.name,
                            showOnRota: false,
                            minimumRequired: 0
                        });
                    });

                }, function (err) {
                    console.error(err);
                });
        }

        function create() {
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

            $http.post("/api/user/v1/employee", data)
                .then(function (response) {
                    notify({
                        message: "Employee created successfully.",
                        duration: -1,
                        position: "center",
                        classes: "alert-success"
                    });
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator. " + err.data.value.message,
                        duration: -1,
                        position: "center",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.isBusy = false;
                });
        }

        function toTimeSpan(date) {
            if (date instanceof Date)
                return date.getHours() + ":" + date.getMinutes() + ":00";
            return date;
        }
    }
})();