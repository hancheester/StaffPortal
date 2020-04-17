(function () {
    "use strict";

    angular.module("app")
        .component("shiftCalendarRoleStaff", {
            templateUrl: "/js/shift/shift-calendar-role-staff.component.html",
            bindings: {
                departmentId: "<",
                employeeId: "<",
                name: "<",
                roleId: "<",
                role: "<",
                shifts: "<",
                dates: "<",
                getEmployeeRoles: "&onGetEmployeeRoles",
                changeTime: "&onChangeTime",
                changeRole: "&onChangeRole"
            },
            controller: ["$http", "$uibModal", ShiftCalendarRoleStaffController],
            controllerAs: "vm"
        });

    function ShiftCalendarRoleStaffController($http, $uibModal) {
        let vm = this;

        vm.showPencil = false;
        vm.days = [
            "Sunday", "Monday", "Tuesday",
            "Wednesday", "Thursday", "Friday",
            "Saturday"
        ];

        vm.$onInit = init;

        vm.formatTimes = formatTimes;
        vm.getTimes = getTimes;
        vm.setTimes = setTimes;
        vm.isOnLeave = isOnLeave;
        vm.editUser = editUser;
        vm.editTime = editTime;
        vm.formatDate = formatDate;
        vm.toDate = toDate;
        vm.toTimeSpan = toTimeSpan;
        vm.getDateByDay = getDateByDay;

        function init() {
        }

        function formatTimes(day) {
            var times = vm.getTimes(day);

            if (times) {
                return times.startTime + " - " + times.endTime;
            }

            return "";
        }

        function getTimes(day) {
            let found = vm.shifts.find(function (shift) {
                return shift.day === day;
            });

            if (found)
                return {
                    startTime: found.startTime,
                    endTime: found.endTime
                };

            return null;
        }

        function setTimes(day, startTime, endTime) {
            let found = vm.shifts.find(function (shift) {
                return shift.day === day;
            });

            if (found) {
                found.startTime = startTime;
                found.endTime = endTime;
            }

            vm.shifts.push({
                day,
                startTime,
                endTime
            });
        }

        function isOnLeave(day) {
            let found = vm.shifts.find(function (shift) {
                return shift.day === day;
            });

            if (found)
                return found.onLeave;

            return false;
        }

        function editUser() {
            let name = vm.name;
            let employeeId = vm.employeeId;
            let getEmployeeRoles = vm.getEmployeeRoles;

            let modal = $uibModal.open({
                animation: true,
                templateUrl: "/js/shift/shift-edit-role.component.html",
                controller: function ($uibModalInstance) {
                    let vm = this;

                    vm.name = name;
                    vm.roles = [];
                    vm.selectedRole = {};
                    vm.getEmployeeRoles = getEmployeeRoles();

                    vm.$onInit = init;
                    vm.update = update;
                    vm.cancel = cancel;

                    function init() {
                        vm.getEmployeeRoles(employeeId)
                            .then(function (response) {
                                let data = angular.copy(response.data.value);
                                vm.roles = data;
                                if (vm.roles && vm.roles.length > 0) {
                                    vm.selectedRole = vm.roles[0];
                                }
                            }, function (err) {
                                console.error(err);
                            });
                    }

                    function update() {
                        $uibModalInstance.close(vm.selectedRole);
                    }

                    function cancel() {
                        $uibModalInstance.dismiss();
                    }
                },
                controllerAs: "vm"
            });
            
            modal.result.then(function (selectedRole) {
                if (selectedRole) {
                    let employeeId = vm.employeeId;
                    let businessRoleId = selectedRole.id;
                    let departmentId = vm.departmentId;
                    
                    let data = [];

                    vm.days.forEach(function (day) {
                        let shiftDay = {
                            employeeId,
                            businessRoleId,
                            departmentId,
                            day,
                            startDate: new Date()
                        };

                        let times = vm.getTimes(day);
                        if (times) {
                            shiftDay.startTime = vm.toTimeSpan(times.startTime);
                            shiftDay.endTime = vm.toTimeSpan(times.endTime);
                        }

                        data.push(shiftDay);
                    });

                    vm.changeRole()(departmentId, employeeId, vm.roleId, businessRoleId, data);
                }                
            }, function () { });
        }

        function editTime(day) {
            let name = vm.name;
            let role = vm.role;
            let formatDate = vm.formatDate;
            let toDate = vm.toDate;

            let foundShift = vm.shifts.find(function (shift) {
                return shift.day === day;
            });

            if (!foundShift) {
                let date = vm.getDateByDay(day);
                foundShift = {
                    date: date,
                    startTime: "00:00",
                    endTime: "00:00"
                };
            }

            let modal = $uibModal.open({
                animation: true,
                templateUrl: "/js/shift/shift-edit-time.component.html",
                controller: function ($uibModalInstance) {
                    let vm = this;

                    vm.name = name;
                    vm.role = role;
                    vm.date = formatDate(foundShift.date, "/");
                    vm.startTime = {};
                    vm.endTime = {};

                    vm.$onInit = init;
                    vm.update = update;
                    vm.cancel = cancel;

                    function init() {
                        vm.startTime = toDate(foundShift.startTime);
                        vm.endTime = toDate(foundShift.endTime);
                    }

                    function update() {
                        $uibModalInstance.close({ startTime: vm.startTime, endTime: vm.endTime });
                    }

                    function cancel() {
                        $uibModalInstance.dismiss();
                    }
                },
                controllerAs: "vm"
            });

            modal.result.then(function (times) {
                if (times.startTime && times.endTime) {
                    let employeeId = vm.employeeId;
                    let startTime = vm.toTimeSpan(times.startTime);
                    let endTime = vm.toTimeSpan(times.endTime);

                    let data = {
                        employeeId,
                        businessRoleId: vm.roleId,
                        departmentId: vm.departmentId,
                        day,
                        startDate: new Date(),
                        startTime,
                        endTime
                    };

                    vm.changeTime()(data)
                        .then(function () {
                            vm.setTimes(day, startTime, endTime);
                        });
                    
                }
            }, function () { });
        }

        function formatDate(date, separator) {
            let day = date.getDate();
            if (day < 9) day = "0" + day;
            let targetDate = [day, date.getMonth() + 1, date.getFullYear()].join(separator);

            return targetDate;
        }

        function getDateByDay(day) {
            return vm.dates[vm.days.indexOf(day)];
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
            if (date instanceof Date) {
                let hour = date.getHours();
                let min = date.getMinutes();

                if (hour < 10) {
                    hour = "0" + hour;
                }

                if (min < 10) {
                    min = "0" + min;
                }

                return hour + ":" + min + ":00";
            }
            return date;
        }
    }
})();