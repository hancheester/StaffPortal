(function () {
    "use strict";

    angular.module("app")
        .directive("calendar", function () {
            return {
                restrict: "E",
                templateUrl: "/js/components/calendar.directive.html",
                scope: {
                    selectedDates: "=ngModel",
                    selected: "=",
                    disabled: "=?",
                    calendarData: "=",
                    calendarLeaveSettings : "=",
                    emergency: "=?",
                    requestable: "=?",
                    nextClick: "&",
                    previousClick: "&",
                    showBusy: "=",
                    currentDate: "=?"
                },
                controller: ["$scope", "$sce", CalendarController],
                controllerAs: "vm"
            };

            function CalendarController($scope, $sce) {
                var vm = this;

                vm.firstDayOfRequestable = {};
                vm.lastDayOfEmergency = {};
                vm.currentDate = {};
                vm.firstDay = {};
                vm.weeks = [];
                vm.selectedDates = [];

                vm.$onInit = onInit;
                vm.next = next;
                vm.previous = previous;
                vm.buildMonth = buildMonth;
                vm.buildWeek = buildWeek;
                vm.select = select;
                vm.extractEmployeeNamesInHtml = extractEmployeeNamesInHtml;
                vm.checkDisabledDates = checkDisabledDates;

                function buildMonth() {
                    vm.weeks = [];
                    let firstDayInWeek = vm.firstDay.clone();
                    let lastDate = vm.currentDate.clone().add(1, "months").date(0);
                    while (firstDayInWeek.isAfter(lastDate) === false) {
                        vm.weeks.push({
                            days: vm.buildWeek(firstDayInWeek)
                        });
                        firstDayInWeek.add(1, "weeks");
                    }
                }

                function buildWeek(firstDayInWeek) {                   
                    let days = [];

                    let calendar = $scope.calendarData;
                    if (calendar === undefined) return days;

                    let currentDay = firstDayInWeek;
                    let today = moment().hour(0).minute(0).second(0).millisecond(0);
                    let yesterday = today.clone().subtract(1, "seconds");
                    let selectedDates = vm.selectedDates;
                    let requestableModeOnly = $scope.requestableModeOnly;
                    let emergencyModeOnly = $scope.emergencyModeOnly;

                    for (var i = 0; i < 7; i++) {
                        var calendarDay;
                        
                        calendarDay = calendar.find(function (element) {
                            var elementDate = moment(element.date);
                            return elementDate.date() === currentDay.date()
                                && elementDate.month() === currentDay.month()
                                && elementDate.year() === currentDay.year();
                        });

                        if (calendarDay) {
                            var employeesOnHoliday = "";
                            if (calendarDay && calendarDay.allocation !== null && calendarDay.employeesOnHolidays !== null)
                                employeesOnHoliday = vm.extractEmployeeNamesInHtml(calendarDay.employeesOnHolidays);

                            days.push({
                                name: currentDay.format("dd").substring(0, 1),
                                number: currentDay.date(),
                                isCurrentMonth: currentDay.month() === vm.currentDate.month(),
                                isToday: currentDay.isSame(today, "day"),
                                isRequestable: currentDay.isSameOrAfter(vm.firstDayOfRequestable),
                                isEmergencyRequestable: currentDay.isBetween(yesterday, vm.lastDayOfEmergency),
                                date: currentDay.clone(),
                                selected: selectedDates.some(function (element) {
                                    return currentDay.isSame(element.date, "day");
                                }),
                                isDisabled: calendarDay.isDisabled
                                    || emergencyModeOnly && !currentDay.isBetween(yesterday, vm.lastDayOfEmergency)
                                    || requestableModeOnly && !currentDay.isSameOrAfter(vm.firstDayOfRequestable),
                                status: StaffLevelStatus[calendarDay.statusCode],
                                departmentId: calendarDay.departmentId,
                                notification: calendarDay.departmentName,
                                employeesOnHolidaysHTML: employeesOnHoliday,
                                isFullDay: true
                            });
                        }

                        currentDay = currentDay.clone();
                        currentDay.add(1, "days");
                    }

                    return days;
                }

                function next() {
                    vm.currentDate.add(1, "months");
                    $scope.currentDate = vm.currentDate;
                    vm.firstDay = vm.currentDate.clone().date(0).day(1).hour(0).minute(0).second(0).millisecond(0);
                    $scope.nextClick({ date: vm.currentDate });
                }

                function previous() {
                    vm.currentDate.subtract(1, "months");
                    vm.firstDay = vm.currentDate.clone().date(0).day(1).hour(0).minute(0).second(0).millisecond(0);
                    $scope.previousClick({ date: vm.currentDate });
                }

                function select(day) {
                    if (!$scope.disabled) {
                        if (!day.isDisabled && (day.isRequestable || day.isEmergencyRequestable)) {
                            var i = vm.selectedDates.findIndex(arrVal => day.date.isSame(arrVal.date));
                            if (vm.selectedDates.length > 0 && i !== -1) {
                                vm.selectedDates.splice(i, 1);
                            }
                            else {
                                vm.selectedDates.push(day);
                            }
                            $scope.selectedDates = vm.selectedDates;
                            vm.checkDisabledDates();
                            vm.buildMonth();
                        }
                    }
                }

                function checkDisabledDates() {
                    let datesSelected = vm.selectedDates;
                    if (datesSelected.length > 0) {
                        if (vm.firstDayOfRequestable.isAfter(vm.lastDayOfEmergency)) {
                            if (datesSelected.every(element => element.isEmergencyRequestable)) {
                                $scope.emergencyModeOnly = true;
                                $scope.emergency = true;
                                $scope.requestable = false;
                            }
                            else if (datesSelected.every(element => element.isRequestable)) {
                                $scope.requestableModeOnly = true;
                                $scope.emergency = false;
                                $scope.requestable = true;
                            }
                        }
                        if (vm.firstDayOfRequestable.isSameOrBefore(vm.lastDayOfEmergency)) {
                            if (datesSelected.some(element => element.isEmergencyRequestable && !element.isRequestable)) {
                                $scope.emergencyModeOnly = true;
                                $scope.emergency = true;
                                $scope.requestable = false;
                            }
                            else if (datesSelected.some(element => element.isRequestable && !element.isEmergencyRequestable)) {
                                $scope.requestableModeOnly = true;
                                $scope.emergency = false;
                                $scope.requestable = true;
                            }
                            else {
                                $scope.requestableModeOnly = false;
                                $scope.emergencyModeOnly = false;
                                $scope.emergency = false;
                                $scope.requestable = false;
                            }
                        }
                    }
                    else {
                        $scope.requestableModeOnly = false;
                        $scope.emergencyModeOnly = false;
                        $scope.emergency = false;
                        $scope.requestable = false;
                    }
                }

                function extractEmployeeNamesInHtml(array) {
                    var html = "";

                    if (array !== undefined && array)
                        for (var i = 0; i < array.length; i++) {
                            html += array[i].name + "<br/>";
                        }

                    return $sce.trustAsHtml(html);
                }

                function onInit() {                    
                    let today = moment().hour(0).minute(0).second(0).millisecond(0);

                    if ($scope.calendarLeaveSettings) {
                        vm.firstDayOfRequestable = today.clone().add($scope.calendarLeaveSettings.leaveNoticePeriod, "days");
                        vm.lastDayOfEmergency = today.clone().add($scope.calendarLeaveSettings.emergencyAllowance, "days");
                    }

                    vm.currentDate = moment().utc().clone().hour(0).minute(0).second(0).millisecond(0);
                    vm.firstDay = moment().utc().clone().date(0).day(1).hour(0).minute(0).second(0).millisecond(0);

                    $scope.$watch("calendarData", function () {
                        vm.buildMonth();
                    }, true);

                    $scope.$watch("calendarLeaveSettings", function () {
                        if ($scope.calendarLeaveSettings) {
                            vm.firstDayOfRequestable = today.clone().add($scope.calendarLeaveSettings.leaveNoticePeriod, "days");
                            vm.lastDayOfEmergency = today.clone().add($scope.calendarLeaveSettings.emergencyAllowance, "days");
                            vm.buildMonth();
                        }
                    }, true);
                }
            }
        });
})();