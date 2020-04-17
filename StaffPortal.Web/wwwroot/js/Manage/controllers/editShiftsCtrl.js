(function () {

    "use strict";

    angular.module("manage-app")
        .controller("editShiftsCtrl", editShiftsCtrl);

    function editShiftsCtrl($scope, $http, notify) {
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

                console.log("request calendar: ", request);
                $http.post("/webapi/getcalendarcoded", request)
                    .then(function (response) {
                        $scope.calendarCoded = angular.copy(response.data.value)
                    }, function (err) {
                        console.error("Get calendar - ERROR: ", err);
                    });

                // Get Departments for the logged in user
                $http.get("/webapi/manage-admin/getdepatmentsonemployee")
                    .then(function (response) {
                        $scope.departments = angular.copy(response.data.value);
                        $scope.selectedDepartment = $scope.departments[0];
                        console.log("$scope.departments:", $scope.departments);
                        let departmentId = 0;
                        if ($scope.selectedDepartment) {
                            departmentId = $scope.selectedDepartment.id;
                        }
                        getRota(departmentId, new Date());
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

        // GET WEEKLY SHIFT
        var getRota = function (depId, fromDate, roleId) {
            $scope.isBusy = true;
            if (roleId === undefined)
                roleId = 0;

            var date = moment(fromDate);
            var request = {
                departmentId: depId,
                fromDate: date.toJSON(),
                businessRoleId: roleId
            };

            $http.post("/webapi/shift-edit/weeklyshift", request)
                .then(function (response) {
                    $scope.rota = angular.copy(response.data.value);
                    console.log("$scope.rota - RESPONSE: ", $scope.rota);
                }, function (err) {
                    console.error("Weekly Shift - ERROR: ", err);
                })
                .then(function () {
                    $scope.isBusy = false;
                })
        }

        $scope.filterRota = function () {
            var date = $scope.calendarDate;
            var startDate = new Date(date.year(), date.month(), date.date());

            getRota($scope.selectedDepartment.id, startDate, $scope.selectedRole.id);
        }

        $scope.getWeek = function (date) {
            var startDate = new Date(date.year(), date.month(), date.date());
            getRota($scope.selectedDepartment.id, startDate);
        }

        $scope.saveAssignment = function (assignment, $modal) {
            var date = new Date();

            var model = {
                businessRoleId: assignment.businessRole.id,
                employeeId: assignment.employeeId,
                startTime: toTimeSpan(assignment.startTime),
                endTime: toTimeSpan(assignment.endTime),
                departmentId: $scope.selectedDepartment.id,
                startDate: assignment.startDate,
                recurringWeeks: assignment.recurringWeeks
            };

            console.log("Insert Assignment : ", model);

            $http.post("/webapi/shift-edit/insertassignment", model)
                .then(function (response) {
                    var data = angular.copy(response.data.value);
                    console.log("New Assignment - RESPONSE: ", data);
                    $modal.modal('hide');
                    updateRota();
                }, function (err) {
                    $scope.errorMessages = angular.copy(err.data.value);
                    console.error("New Assignment - ERROR: ", err);
                });
        }

        $scope.saveWeek = function (weekAssignment, $modal) {
            var model = {
                businessRoleId: weekAssignment.businessRole.id,
                employeeId: weekAssignment.employeeId,
                departmentId: $scope.selectedDepartment.id,
                startDate: weekAssignment.startDate,
                recurringWeeks: weekAssignment.recurringWeeks
            };

            $http.post("/webapi/shit-edit/editweek", model)
                .then(function (response) {
                    var data = angular.copy(response.data.value);
                    console.log("Edit week: ", response.data.value);
                    $modal.modal('hide');
                    updateRota();
                }, function (err) {
                    console.error("Edit week: ", err);
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

        var updateRota = function () {
            var date = new Date();

            getRota($scope.selectedDepartment.id, date);
        }

        
        $scope.hstep = 1;
        $scope.mstep = 5;
    }
})();