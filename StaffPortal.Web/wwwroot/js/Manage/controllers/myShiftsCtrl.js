(function () {

    "use strict";

    angular.module("manage-app")
        .controller("myShiftsCtrl", myShiftsCtrl);

    function myShiftsCtrl($scope, $http, notify) {
        $scope.isBusy = true;

        function createCalendarRequest(employeeId, startDate, endDate) {
            var request = {
                employeeId: employeeId,
                fromdate: startDate.toDate(),
                toDate: endDate.toDate()
            };

            return request;
        }

        //#region API CALLS

        $http.get("/webapi/GetCurrentEmployee")
            .then(function (response) {
                $scope.employee = angular.copy(response.data.value);

                var from = moment();
                var to = from.clone().add(7, 'days');
                var request = createCalendarRequest($scope.employee.id, from, to);

                console.log("request: ", request);
                $http.post("/webapi/getcalendarcoded", request)
                    .then(function (response) {
                        $scope.calendarCoded = angular.copy(response.data.value)
                        console.log("Calendar Coded: ", $scope.calendarCoded);
                    }, function (err) {
                        console.error("Get calendar - ERROR: ", err);
                    });

                // Get Departments for the logged in user
                $http.get("/webapi/manage-admin/getdepatmentsonemployee")
                    .then(function (response) {
                        $scope.departments = angular.copy(response.data.value);
                        $scope.selectedDepartment = $scope.departments[0];
                        console.log("$scope.departments:", $scope.departments);
                        if ($scope.selectedDepartment !== undefined) {
                            getRota($scope.selectedDepartment.id, new Date());
                        }
                    }, function (err) {
                        console.error("Get Department on employee - ERROR: ", err);
                    });

                // Get BusinessRoles for the logged in user
                $http.get("/webapi/getuserbusinessrole")
                    .then(function (response) {
                        $scope.businessRoles = [{ id: 0, name: "All Roles" }];
                        $scope.selectedRole = $scope.businessRoles[0];
                        $scope.businessRoles = $scope.businessRoles.concat(response.data);
                    }, function (err) {
                        console.error("getuserbusinessrole - ERORR: ", err);
                    });

            }, function (err) {
                console.error("GetCurrentEmployee - ERROR: ", err);
            });

        $scope.filterRota = function () {
            var date = $scope.calendarDate;
            var startDate = new Date(date.year(), date.month(), date.date());

            getRota($scope.selectedDepartment.id, startDate, $scope.selectedRole.id);
        }

        // GET WEEKLY SHIFT
        var getRota = function (depId, fromDate, roleId) {
            $scope.isBusy = true;
            if (roleId === undefined)
                roleId = 0;

            fromDate = toMomentDate(fromDate);
            var request = {
                departmentId: depId,
                fromDate: fromDate.toDate(),
                businessRoleId: roleId,
                employeeId: $scope.employee.id
            };
            console.log("REQUEST: ", request);
            $http.post("/webapi/shift-view/userweeklyshift/", request)
                .then(function (response) {
                    $scope.rota = angular.copy(response.data.value);
                    console.log("$scope.rota - RESPONSE: ", $scope.rota);
                    $scope.isBusy = false;
                }, function (err) {
                    console.error("Weekly Shift - ERROR: ", err);
                    $scope.isBusy = false;
                })
        }

        $scope.selectDepartment = function () {
            console.log("Selected Department: ", $scope.selectedDepartment);
        }

        $scope.selectRole = function () {
            var date = $scope.calendarDate;
            var startDate = new Date(date.year(), date.month(), date.date());

            getRota($scope.selectedDepartment.id, startDate, $scope.selectedRole.id);
        }

        $scope.nextWeek = function (date) {
            var startDate = new Date(date.year(), date.month(), date.date());
            getRota($scope.selectedDepartment.id, startDate);
        }

        $scope.previousWeek = function (date) {
            var startDate = new Date(date.year(), date.month(), date.date());
            getRota($scope.selectedDepartment.id, startDate);
        }

        $scope.saveAssignment = function (assignment, $modal) {
            var model = {
                businessRoleId: assignment.businessRole.id,
                employeeId: assignment.employeeId,
                startTime: toTimeSpan(assignment.startTime),
                endTime: toTimeSpan(assignment.endTime),
            };

            console.log("$modal: ", $modal);

            $http.post("/webapi/shift-edit/insertassignment", model)
                .then(function (response) {
                    var data = angular.copy(response.data.value);
                    console.log("New Assignment - RESPONSE: ", data);
                    $modal.modal('hide');
                }, function (err) {
                    $scope.errorMessages = angular.copy(err.data.value);
                    console.error("New Assignment - ERROR: ", err);
                });
        }

        //#endregion API CALLS

        var toTimeSpan = function (date) {
            if (date instanceof Date)
                return date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds();
            if (moment.isMoment(date))
                console.log("moment.isMoment(date)", moment.isMoment(date));
            return "error";
        }

        function toDateTime(arr) {
            angular.forEach(arr, function (value, key) {
                value.date = value.date.toDate().toUTCString();
            });
        }

        $scope.hstep = 1;
        $scope.mstep = 5;

        function toMomentDate(date) {
            var m = moment(date);
            return m;
        }
    }
})();