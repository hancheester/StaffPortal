(function () {

    angular.module("manage-app")
        .directive("editShift", function ($timeout) {
            return {
                restrict: "E",
                templateUrl: "/js/Manage/views/editShifts.html",
                transclude: true,
                scope: {
                    data: "=",
                    leaveDays: "=",
                    daysWorking: "=",
                    week: "=",
                    month: "=date",
                    selectedDepartment: "=",
                    isBusy: '=',
                    nextClick: '&',
                    previousClick: '&'
                },
                link: function (scope) {
                    scope.selected = _removeTime(scope.selected || moment().utc());
                    scope.month = scope.selected.clone();
                    const MODE_ASSIGNMENT = "assignment";
                    const MODE_WEEK = "week";
                    const MODE_UPDATE_ASSIGNMENT = "update_assignment";
                    var assingment_edit_mode = undefined;

                    var start = scope.selected.clone();

                    scope.$watch('isBusy', function () {
                        console.log("isBusy: ", scope.isBusy);
                    }, true);

                    scope.$watch('data', function () {
                        _removeTime(start.day(0));
                        scope.weeks = [];
                        let businessRole_Employees = null;

                        if (scope.data && scope.data.businessRole_Employees) {
                            businessRole_Employees = scope.data.businessRole_Employees;
                        }

                        scope.weeks = _buildWeek(scope.month.clone(), scope.month, scope.leaveDays, scope.week,
                            scope.daysWorking, businessRole_Employees, scope.selectedDepartment);
                        console.log("scope.weeks: ", scope.weeks);
                    }, true);

                    // TIMEPICKER CONFIG
                    scope.hstep = 1;
                    scope.mstep = 5;
                    scope.ismeridian = false;

                    scope.select = function (day) {
                        if (!day.isDisabled) {
                            scope.selected = day.date.utc();
                            var i = scope.daysSelected.indexOf(day);
                            if (scope.daysSelected.length > 0 && i !== -1) {
                                scope.daysSelected.splice(i, 1);
                            }
                            else {
                                day.isFullDay = true;
                                scope.daysSelected.push(day);
                            }
                        }
                    };

                    scope.editShift = function (employee, date) {
                        scope.selectedEmployee = employee;
                        var time = new Date();
                        time.setHours("00", "00", "00");
                        assingment_edit_mode = MODE_ASSIGNMENT;

                        scope.assignment = {
                            employeeId: employee.employeeId,
                            startTime: time,
                            endTime: time,
                            startDate: new Date(date.date)
                        };
                    }

                    scope.editWeek = function (employee, date) {
                        scope.selectedEmployee = employee;

                        scope.weekAssignment = {
                            employeeId: employee.employeeId,
                            startDate: new Date(date.date),
                            recurringWeeks: 0,
                            businessRoleId: 0
                        };
                    }

                    scope.startTimeChanged = function () {
                        if (scope.assignment.endTime < scope.assignment.startTime)
                            scope.assignment.endTime = scope.assignment.startTime;
                    }

                    scope.saveAssignment = function (form) {
                        var $modal = angular.element("#editShift");
                        if (form.$valid)
                            scope.$parent.saveAssignment(scope.assignment, $modal);
                        else {

                            angular.forEach(form.$$controls, function (value, key) {
                                value.$touched = true;
                                value.$dirty = true;
                            });
                        }
                    }

                    scope.saveWeek = function (form) {
                        var $modal = angular.element("#editWeek");
                        if (form.$valid)
                            scope.$parent.saveWeek(scope.weekAssignment, $modal);
                        else {

                            angular.forEach(form.$$controls, function (value, key) {
                                value.$touched = true;
                                value.$dirty = true;
                            });
                        }
                    }

                    scope.isDate = function (date) {
                        console.log("isDate(): ", date instanceof (Date));
                        return date instanceof (Date);
                    }

                    scope.next = function () {
                        var next = scope.month.clone();
                        _removeTime(next.month(next.week() + 1).day(0));
                        scope.month.week(scope.month.week() + 1);
                        scope.nextClick({ date: scope.month });
                        //$timeout(scope.weeks = _buildWeek(scope.month.clone(), scope.month, scope.leaveDays, scope.week,
                        //    scope.daysWorking, scope.data.businessRole_Employees, scope.selectedDepartment),
                        //    4000);
                    };

                    scope.previous = function () {
                        var previous = scope.month.clone();
                        _removeTime(previous.month(previous.week() - 1).day(0));
                        scope.month.week(scope.month.week() - 1);
                        scope.previousClick({ date: scope.month });
                        //$timeout(scope.weeks = _buildWeek(scope.month.clone(), scope.month, scope.leaveDays, scope.week,
                        //    scope.daysWorking, scope.data.businessRole_Employees, scope.selectedDepartment),
                        //    4000);
                    };

                    scope.countDaysAssigned = function (array) {
                        var count = array.filter(function (day) { return day.isAssigned === true }).length;

                        return count;
                    }

                }
            };

            function _removeTime(date) {
                var d = date.day(1).hour(0).minute(0).second(0).millisecond(0);
                return date.day(1).hour(0).minute(0).second(0).millisecond(0);
            }

            function _buildWeek(date, month, leaveDays, week, daysWorking, employeesOnShift, selectedDepartment) {
                var days = [];
                var assignments = [];

                for (var i = 0; i < 7; i++) {
                    days.push(
                        {
                            day: {
                                name: date.format("ddd"),
                                number: date.date(),
                                isCurrentMonth: date.month() === month.month(),
                                isToday: date.isSame(new Date(), "day"),
                                date: date,
                                isDisabled: week ? week[i].isDisabled : true,
                                status: week ? StaffLevelStatus[week[i].statusCode] : '',
                                departmentId: week ? week[i].departmentId : 0,
                                notification: week ? week[i].departmentName : ''
                            }
                        });

                    date = date.clone();
                    date.add(1, "d");
                }

                var rota = {
                    selectedDepartment: selectedDepartment,
                    days: days,
                    assignments: assignments
                };
                return rota;
            }

            function checkIsLeave(arr, val) {
                alert();
                return arr.find(arrVal => val.isSame(arrVal.date));
            }

            function checkDisabled(arr, val) {
                return arr.some(function (arrVal) {
                    return arrVal.isSame(val, "day");
                });
            }

        });
})();