(function () {

    angular.module("manage-app")
        .directive("shift", function ($timeout) {
            return {
                restrict: "E",
                templateUrl: "/js/Manage/views/shift.html",
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
                    scope.selected = _removeTime(scope.selected || moment());
                    scope.month = scope.selected.clone();

                    var start = scope.selected.clone();

                    scope.$watch('isBusy', function () {
                        console.log("isBusy: ", scope.isBusy);
                    }, true);
                    
                    scope.$watch('data', function () {
                        _removeTime(start.day(0));
                        scope.weeks = [];

                        let businessRolesEmployees = scope.data !== undefined ? scope.data.businessRole_Employees : null;

                        if (scope.week !== undefined) {
                            scope.weeks = _buildWeek(scope.month.clone(), scope.month, scope.leaveDays, scope.week,
                                scope.daysWorking, businessRolesEmployees, scope.selectedDepartment);
                        }
                        
                    }, true);

                    // TIMEPICKER CONFIG
                    scope.hstep = 1;
                    scope.mstep = 5;
                    scope.ismeridian = false;

                    scope.select = function (day) {
                        if (!day.isDisabled) {
                            scope.selected = day.date;
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

                    scope.selectEmployee = function (employee) {
                        scope.selectedEmployee = employee;
                        var time = new Date();
                        time.setHours("00", "00", "00");
                        scope.assignment = {
                            employeeId: employee.employeeId,
                            startTime: time,
                            endTime: time
                        };
                    }

                    scope.startTimeChanged = function () {
                        if (scope.assignment.endTime < scope.assignment.startTime)
                            scope.assignment.endTime = scope.assignment.startTime;
                    }

                    scope.saveAssignment = function (form, $element) {
                        var $modal = angular.element("#editEmployee");
                        if (form.$valid)
                            scope.$parent.saveAssignment(scope.assignment, $modal);
                        else {
                            console.log("form.$$controls: ", form.$$controls);

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
                        $timeout(scope.weeks = _buildWeek(scope.month.clone(), scope.month, scope.leaveDays, scope.week,
                            scope.daysWorking, scope.data.businessRole_Employees, scope.selectedDepartment),
                            4000);
                    };

                    scope.previous = function () {
                        var previous = scope.month.clone();
                        _removeTime(previous.month(previous.week() - 1).day(0));
                        scope.month.week(scope.month.week() - 1);
                        scope.previousClick({ date: scope.month });
                        $timeout(scope.weeks = _buildWeek(scope.month.clone(), scope.month, scope.leaveDays, scope.week,
                            scope.daysWorking, scope.data.businessRole_Employees, scope.selectedDepartment),
                            4000);
                    };
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
                                isDisabled: week[i].isDisabled,
                                status: StaffLevelStatus[week[i].statusCode],
                                departmentId: week[i].departmentId,
                                notification: week[i].departmentName
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