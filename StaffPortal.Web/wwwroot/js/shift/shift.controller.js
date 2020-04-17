(function() {
    "use strict";

    angular.module("app")
        .controller("ShiftController", ["$scope", "$http", "notify", ShiftController]);

    function ShiftController($scope, $http, notify) {
        var vm = this;

        vm.selectedDepartment = null;
        vm.selectedRole = null;
        vm.departments = [];
        vm.roles = [];
        vm.dates = [];
        vm.today = {};
        vm.departmentalRoles = [];
        vm.statusReports = [];

        vm.$onInit = init;
        vm.load = load;
        vm.getDepartments = getDepartments;
        vm.loadRota = loadRota;
        vm.loadEmployeeShifts = loadEmployeeShifts;
        vm.getRoles = getRoles;
        vm.calculateDates = calculateDates;
        vm.next = next;
        vm.prev = prev;
        vm.getShifts = getShifts;
        vm.getDepartmentalRoles = getDepartmentalRoles;
        vm.getEmployeeRoles = getEmployeeRoles;
        vm.formatDate = formatDate;
        vm.changeTime = changeTime;
        vm.changeRole = changeRole;

        function init() {
            vm.today = moment();
            vm.calculateDates();            
            vm.getRoles();      
            vm.getDepartments()
                .then(function () {
                    vm.load();
                });
        }

        function load() {
            if (vm.selectedDepartment && vm.selectedDepartment.id) {
                vm.getDepartmentalRoles()
                    .then(function () {
                        vm.loadRota();
                    });

                vm.statusReports = [];

                vm.dates.forEach(function (date) {
                    let targetDate = vm.formatDate(date);
                    let report = {
                        isBusy: true,
                        min: 0,
                        actual: 0
                    };

                    vm.statusReports.push(report);

                    $http.get("/api/shift/v1/minimum-staff-status/department/" + vm.selectedDepartment.id + "/date/" + targetDate)
                        .then(function (response) {
                            let data = angular.copy(response.data.value);
                            if (data) {
                                report.min = data.item1;
                                report.actual = data.item2;
                            }
                        }, function (err) {
                            console.error(err);
                        })
                        .then(function () {
                            report.isBusy = false;
                        });
                });
            }
        }

        function getDepartments() {
            return $http.get("/api/department/v1/my-departments")
                .then(function (response) {
                    let data = angular.copy(response.data.value);
                    vm.departments = data;

                    if (vm.departments.length > 0) {
                        vm.selectedDepartment = vm.departments[0];
                    }
                }, function (err) {
                    console.error(err);
                });
        }

        function getRoles() {
            $http.get("/api/role/v1/my-roles")
                .then(function (response) {
                    let data = angular.copy(response.data.value);

                    data.sort(function (a, b) {
                        let nameA = a.name.toUpperCase();
                        let nameB = b.name.toUpperCase();

                        if (nameA < nameB) {
                            return -1;
                        }
                        if (nameA > nameB) {
                            return 1;
                        }
                        return 0;
                    });

                    vm.roles = data;
                }, function (err) {
                    console.error(err);
                });
        }

        function calculateDates() {
            vm.dates = [];
            [0, 1, 2, 3, 4, 5, 6].forEach(function (value) {
                vm.dates.push(vm.today.day(value).hour(0).minute(0).second(0).millisecond(0).toDate());
            });
        }

        function next() {
            vm.today.add(1, "weeks");
            vm.calculateDates();
            vm.load();
        }

        function prev() {
            vm.today.subtract(1, "weeks");
            vm.calculateDates();
            vm.load();
        }

        function getShifts(departmentId, roleId, date) {
            let targetDate = vm.formatDate(date);

            return $http.get("/api/shift/v1/rota/department/" + departmentId + "/role/" + roleId + "/date/" + targetDate);
        }

        function getDepartmentalRoles() {
            let from = vm.dates[0];
            let fromDay = from.getDate();
            if (fromDay < 9) fromDay = "0" + fromDay;
            let fromMonth = from.getMonth() + 1;
            if (fromMonth < 9) fromMonth = "0" + fromMonth;

            let to = vm.dates[6];
            let toDay = to.getDate();
            if (toDay < 9) toDay = "0" + toDay;
            let toMonth = to.getMonth() + 1;
            if (toMonth < 9) toMonth = "0" + toMonth;
            
            var fromDate = [fromDay, fromMonth, from.getFullYear()].join("");
            var toDate = [toDay, toMonth, to.getFullYear()].join("");

            if (vm.selectedRole) {
                return $http.get("/api/shift/v1/roles/" + vm.selectedRole.id + "/department/" + vm.selectedDepartment.id + "/from-date/" + fromDate + "/to-date/" + toDate)
                    .then(function (response) {
                        let data = angular.copy(response.data.value);
                        vm.departmentalRoles = data;
                    }, function (err) {
                        console.error(err);
                    });                
            }
            else {
                vm.departmentalRoles = [];

                return $http.get("/api/shift/v1/roles/department/" + vm.selectedDepartment.id + "/from-date/" + fromDate + "/to-date/" + toDate)
                    .then(function (response) {
                        let data = angular.copy(response.data.value);
                        vm.departmentalRoles = data;
                    }, function (err) {
                        console.error(err);
                    });
            }
        }

        function loadRota() {
            vm.departmentalRoles.forEach(function (role) {
                vm.loadEmployeeShifts(vm.selectedDepartment.id, role)
                    .then(function () {
                        vm.departmentalRoles.sort(function (a, b) {
                            let nameA = a.name.toUpperCase();
                            let nameB = b.name.toUpperCase();

                            if (nameA < nameB) {
                                return -1;
                            }
                            if (nameA > nameB) {
                                return 1;
                            }
                            return 0;
                        });
                    });
            });
        }

        function loadEmployeeShifts(departmentId, role) {            
            return new Promise(function (resolve, reject) {
                role.employees = [];
                role.isBusy = true;

                let employees = role.employees;
                let dateCounter = 0;

                vm.dates.forEach(function (date) {
                    vm.getShifts(departmentId, role.id, date)
                        .then(function (response) {
                            let rotas = response.data.value;

                            rotas.forEach(function (rota) {
                                let foundEmployee = employees.find(function (employee) {
                                    return employee.id === rota.employeeId;
                                });

                                if (foundEmployee) {
                                    foundEmployee.shifts.push({
                                        date: date,
                                        day: rota.day,
                                        startTime: rota.shift.item1,
                                        endTime: rota.shift.item2,
                                        onLeave: rota.shift.item3
                                    });
                                } else {
                                    employees.push({
                                        id: rota.employeeId,
                                        name: rota.name,
                                        shifts: [{
                                            date: date,
                                            day: rota.day,
                                            startTime: rota.shift.item1,
                                            endTime: rota.shift.item2,
                                            onLeave: rota.shift.item3
                                        }]
                                    });

                                    employees.sort(function (a, b) {
                                        let nameA = a.name.toUpperCase();
                                        let nameB = b.name.toUpperCase();

                                        if (nameA < nameB) {
                                            return -1;
                                        }
                                        if (nameA > nameB) {
                                            return 1;
                                        }
                                        return 0;
                                    });
                                }
                            });
                        }, function (err) {
                            console.error(err);
                        })
                        .then(function () {
                            dateCounter = dateCounter + 1;
                            role.isBusy = !(dateCounter === 7);
                        });
                });

                resolve();
            });
        }

        function getEmployeeRoles(id) {
            return $http.get("/api/role/v1/roles/employee/" + id);
        }

        function changeTime(newTime) {
            return $http.put("/api/shift/v1/times/employee/" + newTime.employeeId, newTime)
                .then(function (response) {
                    notify({
                        message: "Time updated successfully.",
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
                });
        }

        function changeRole(departmentId, employeeId, oldRoleId, newRoleId, newRole) {
            $http.put("/api/shift/v1/role/department/" + departmentId + "/employee/" + employeeId, newRole)
                .then(function (response) {
                    let oldRole = vm.departmentalRoles.find(function (role) {
                        return role.id === oldRoleId;
                    });

                    if (oldRole) {
                        oldRole.employees.splice(oldRole.employees.findIndex(function (employee) {
                            return employee.id === employeeId;
                        }), 1);
                    }

                    let foundRole = vm.departmentalRoles.find(function(role) {
                        return role.id === newRoleId;
                    });

                    if (foundRole) {
                        vm.loadEmployeeShifts(departmentId, foundRole);
                    }

                    notify({
                        message: "Role updated successfully.",
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
                });
        }

        function formatDate(date, separator) {
            if (!separator) {
                separator = "";
            }

            let day = date.getDate();
            if (day < 10) day = "0" + day;

            let month = date.getMonth() + 1;
            if (month < 10) month = "0" + month;

            let targetDate = [day, month, date.getFullYear()].join(separator);

            return targetDate;
        }
    }
})();