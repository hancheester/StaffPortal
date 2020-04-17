(function () {

    "use strict";

    angular.module('admin-app')
        .controller('timesheetCtrl', timesheetCtrl);

    function timesheetCtrl($http, $scope, $timeout, notify) {

        // #region SCOPE PARAMETERS INITIALIZATION
        $scope.timesheet = [];
        $scope.timesheetToApprove = [];
        $scope.pageSize = 10;
        $scope.currentPage = 1;
        $scope.requestedDate = new Date();
        $scope.selectedDepartment = {
            id: 0
        };
        // #endregion SCOPE PARAMETERS INITIALIZATION

        $scope.selectedRole = {
            id: 0
        };

        $scope.getTimesheet = function () {
            
            var request = {
                departmentId: $scope.selectedDepartment.id,
                fromDate: $scope.requestedDate.toJSON(),
                businessRoleId: $scope.selectedRole.id
            };

            $http.post("/webapi/gettimesheet", request)
                .then(function (response) {
                    $scope.timesheet = angular.copy(response.data.value);
                    angular.forEach($scope.timesheet, function (value, key) {
                        value.item1.timestamp = new Date(value.item1.timestamp);
                        value.item2.timestamp = new Date(value.item2.timestamp);
                    });
                }, function (err) {
                    console.error("get timesheet - ERROR: ", err);
                });
        }

        $scope.getTimesheet();

        $scope.filterTimesheet = function () {
            var date = moment();
            console.log("Requested Date.toJSON: ", date.toJSON());

            $scope.getTimesheet($scope.selectedDepartment.id, $scope.selectedRole.id, date.toJSON());
        }
        
        $scope.shiftPopover = {
            content: 'Hello, World!',
            templateUrl: 'shiftTemplate.html',
            title: $scope.timesheet
        };

        $scope.clockToString = function (isClockIn) {
            if (isClockIn)
                return "Clock In";
            else
                return "Clock Out";
        }

        $scope.selectTimesheetDetails = function (timesheetDetail) {
            $scope.selectedtsd = angular.copy(timesheetDetail);
            console.log("Selected Timesheet Details: ", $scope.selectedtsd);
            $scope.selectedtsd.date = new Date(timesheetDetail.timestamp);
            $scope.selectedtsd.time = new Date(timesheetDetail.timestamp);
        }

        $scope.diff_minutes = function (date1, date2) {

            date1 = moment(date1);
            date2 = moment(date2);

            var momentDiff = moment.duration(date2.diff(date1));
            var duration = momentDiff.hours() + "." + momentDiff.minutes();

            return duration;
        }
        
        $scope.timeclockToApprove = function (timeclock) {
            console.log({ timeclock: timeclock });
            var index = $scope.timesheetToApprove.findIndex(arrVal => arrVal.timeclockId === timeclock.timeclockId);

            if (index === -1)
                $scope.timesheetToApprove.push(timeclock);
            else
                $scope.timesheetToApprove.splice(index, 1);

            console.log({ timesheetToApprove: $scope.timesheetToApprove });
        }

        $scope.print = function (obj) {
            console.log("PRINT: ", obj);
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

        $scope.popupTsdDate = {
            opened: false
        };

        $scope.popupRequestedDate = {
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

        $scope.toggleMin = function () {
            $scope.options.minDate = $scope.options.minDate ? null : new Date();
        };

        $scope.toggleMin();

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

        // #region API CALLS

        $http.get("/webapi/GetCurrentEmployee")
            .then(function (response) {
                $scope.employee = angular.copy(response.data.value);

                // Get Departments for the logged in user
                $http.get("/webapi/manage-admin/getdepatmentsonemployee")
                    .then(function (response) {
                        $scope.departments = angular.copy(response.data.value);
                        $scope.selectedDepartment = $scope.departments[0];
                        console.log("$scope.departments:", $scope.departments);

                    }, function (err) {
                        console.error("Get Department on employee - ERROR: ", err);
                    });

                // Get All BusinessRoles
                $http.get("/webapi/getbusinessroles")
                    .then(function (response) {
                        $scope.businessRoles = [{ id: 0, name: "All Roles" }];
                        $scope.selectedRole = $scope.businessRoles[0];
                        $scope.businessRoles = $scope.businessRoles.concat(response.data.value);
                    }, function (err) {
                        console.error("getuserbusinessrole - ERORR: ", err);
                    });

            }, function (err) {
                console.error("GetCurrentEmployee - ERROR: ", err);
            });


        $scope.approveTimesheet = function () {
            $http.post("/webapi/approvetimesheet", $scope.timesheetToApprove)
                .then(function (response) {
                    notify({
                        message: 'Timesheet Approved Successfully!',
                        classes: 'full-width success'
                    });
                }, function (err) {
                    console.error("Timesheet approver - ERROR: ", err);
                    notify({
                        message: 'Error! Could not approve timesheet',
                        classes: 'full-width danger',
                        position: 'left'
                    });
                });
        }

        $scope.editTimesheet = function (selectedtsd) {
            console.log("selectedtsd: ", selectedtsd);
            var date = moment();

            var model = {
                id: selectedtsd.timeclockId,
                timestamp: selectedtsd.timestamp.toJSON(),
                employeeId: selectedtsd.employeeId,
                isClockIn: selectedtsd.isClockIn,
                isApproved: selectedtsd.isApproved
            };

            $http.post("/webapi/edittimestamp", model)
                .then(function (response) {
                    console.log("Edit timestamp - RESPONSE: ", response.data.value);
                    $('#editTimesheet').modal('hide');
                    $scope.getTimesheet(0, 0, date.toJSON());
                }, function (err) {
                    console.error(err);
                });
        }

        // #endregion END API CALLS
    }
})();