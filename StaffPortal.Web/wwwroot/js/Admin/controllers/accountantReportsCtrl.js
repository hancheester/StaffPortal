(function () {

    "use strict";

    angular.module('admin-app')
        .controller('accountantReportsCtrl', accountantReportsCtrl);

    function accountantReportsCtrl($http, $scope, $timeout, notify) {
        const API_DATEFORMAT = "YYYYMMDD";

        $scope.isBusy = false;
        $scope.timesheet = [];
        $scope.pageSize = 5;
        $scope.currentPage = 1;
        $scope.toDate = new Date();
        $scope.fromDate = moment($scope.toDate).add(-6, 'day').toDate();
        console.log("To Date: ", $scope.toDate);

        $scope.getAccountantReports = function (fromDate, toDate) {
            var mFromDate = moment(fromDate);
            var mToDate = moment(toDate);
            $scope.isBusy = true;

            $http.get("/webapi/accountantreport/" + 0 +
                "/start_date/" + mFromDate.format(API_DATEFORMAT) +
                "/end_date/" + mToDate.format(API_DATEFORMAT) +
                "/pageindex/" + $scope.currentPage +
                "/pagesize/" + $scope.pageSize
            )
                .then(function (response) {
                    var pagedResult = angular.copy(response.data.value);
                    $scope.accountantReports = pagedResult.items;
                    $scope.totalItems = pagedResult.totalItems;
                    console.log("Accountant Reports: ", {
                        Reports: $scope.accountantReports,
                        totalItems: $scope.totalItems
                    });
                }, function (err) {
                    $scope.errorMessages = angular.copy(err.data.value);
                    console.error({ errorMessages: $scope.errorMessages });

                    $timeout(function () {
                        $scope.errorMessages = [];
                    }, TIMEOUT_DURATION);
                })
                .then(function () {
                    $scope.isBusy = false;
                });
        }
                

        $http.get("/webapi/getdepartments")
            .then(function (response) {
                $scope.departments = angular.copy(response.data.value);
                $scope.selectedDepartment = $scope.departments[0];
            }, function (err) {
                console.error(err);
            });

        $http.get("/webapi/getbusinessroles")
            .then(function (response) {
                $scope.businessRoles = [{ id: 0, name: "All Roles" }];
                $scope.selectedRole = $scope.businessRoles[0];
                $scope.businessRoles = $scope.businessRoles.concat(response.data.value);
            }, function (err) {
                console.error(err);
            })

        $scope.getAccountantReports($scope.fromDate, $scope.toDate);

        $scope.formatTimespan = function (time) {
            var dotIndex = time.indexOf('.');
            var ddotIndex = time.indexOf(':');

            var days = 0;
            var hours = 0;
            var minutes = 0;

            if (dotIndex > 0 && dotIndex < ddotIndex) {
                try {
                    days = parseInt(time.substring(0, dotIndex));
                    var daysInHours = days * 24;
                    var conHours = parseInt(time.substring(dotIndex + 1, dotIndex + 3));
                    hours = (conHours + daysInHours).toString();
                    minutes = time.substring(dotIndex + 4, dotIndex + 6);
                }
                catch (err) {
                    console.error("Timespan Conversion Error!");
                }
            }
            else {
                hours = time.substring(0, 2);
                minutes = time.substring(3, 5);
            }

            return hours + "." + minutes;
        }

        $scope.clockToString = function (isClockIn) {
            if (isClockIn)
                return "Clock In";
            else
                return "Clock Out";
        }

        $scope.diff_minutes = function (date1, date2) {
            if (typeof (date1) === 'string')
                date1 = new Date(date1);
            if (typeof (date2) === 'string')
                date2 = new Date(date2);
            var msPerDay = 8.64e7;

            var diff = (date1 - date2);

            var hours = Math.abs(Math.round(diff / 3600000));
            var minutes = Math.abs(Math.round((diff % 3600000) / 60000));
            var duration = moment().hour(hours).minutes(minutes);

            return duration.format('H.mm');
        }

        // #region DATEPICKER POPUP

        $scope.mstep = 5;
        $scope.hstep = 1;

        $scope.today = function () {
            $scope.dt = new Date();
        };

        $scope.today();

        $scope.clear = function () {
            $scope.dt = null;
        };

        var lastYear = moment().add(-1, 'year').toDate();
        $scope.options = {
            customClass: getDayClass,
            minDate: lastYear,
            maxDate: new Date(),
            showWeeks: false
        };

        $scope.fromDatePopup = {
            opened: false
        };

        $scope.toDatePopup = {
            opened: false
        };

        $scope.openDate = function (popup) {
            popup.opened = true;
        };

        // Disable weekend selection
        function disabled(data) {
            var date = data.date,
                mode = data.mode;
            return mode === 'day' && (date.getDay() === 0 || date.getDay() === 6);
        }

        //$scope.toggleMin = function () {
        //    $scope.options.minDate = $scope.options.minDate ? null : new Date();
        //};

        //$scope.toggleMin();

        $scope.setDate = function (year, month, day) {
            $scope.dt = new Date(year, month, day);
        };

        var tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        var afterTomorrow = new Date(tomorrow);
        afterTomorrow.setDate(tomorrow.getDate() + 1);
        $scope.events = [
            {
                date: tomorrow,
                status: 'full'
            },
            {
                date: afterTomorrow,
                status: 'partially'
            }
        ];

        function getDayClass(data) {
            var date = data.date,
                mode = data.mode;
            if (mode === 'day') {
                var dayToCheck = new Date(date).setHours(0, 0, 0, 0);

                for (var i = 0; i < $scope.events.length; i++) {
                    var currentDay = new Date($scope.events[i].date).setHours(0, 0, 0, 0);

                    if (dayToCheck === currentDay) {
                        return $scope.events[i].status;
                    }
                }
            }

            return '';
        }
        // #endregion DATEPICKER POPUP

    }
})();