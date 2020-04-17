(function () {

    "use strict";

    angular.module("manage-app")
        .controller("rejectedRequestsCtrl", rejectedRequestsCtrl);

    function rejectedRequestsCtrl($scope, $http, $compile, notify) {
        var date = new Date();
        $scope.leaveRequestStatus = LeaveRequestStatus;
        $scope.pageSize = 10;
        $scope.currentPage = 1;
        $scope.rejectedRequests = [];
        $scope.approveIsCollapsed = true;
        var departmentId = 0;

        $scope.updateRequest = function (request, index, isAccepted) {

            angular.forEach(request.requestedDates, function (value, key) {
                value.statusCode = $scope.leaveRequestStatus.rejected;
                if (isAccepted)
                    value.statusCode = $scope.leaveRequestStatus.accepted;
            });
            request.rejectionReason = request.approveReason;

            console.log("POST - Update Request: ", request);
            $http.post("/webapi/approver-manage/updateLeaveRequest", request)
                .then(function (response) {
                    console.log("RESPONSE - Update Request: ", response);
                    if (isAccepted) {
                        $scope.rejectedRequests.splice(index, 1);
                        console.log("Request Accepted: ", request);
                        notify({
                            message: "The request has been acceppted",
                            classes: 'full-width success',
                            position: 'left',
                        });
                    }
                    else {
                        $scope.rejectedRequests.splice(index, 1);
                        console.warn("Request Rejeceted: ", request);
                        notify({
                            message: "The request has been rejected",
                            classes: 'full-width success',
                            position: 'left',
                        });
                    }
                }, function (err) {
                    console.error("ERROR - Update Request: ", err);

                    var errorMessages = angular.copy(err.data.value.errorMessages);
                    var errMsg = "";
                    angular.forEach(errorMessages, function (value, key) {
                        notify({
                            message: value,
                            classes: 'full-width danger',
                            position: 'left',
                            duration: 15000
                        });
                    });
                });
        }

        //#region API CALLS

        $http.get("/webapi/approver-manage/GetRejectedLeaveRequests/departmentId=" + departmentId)
            .then(function (response) {
                console.log("GetRejectedLeaveRequests Response: ", response.data.value);
                $scope.rejectedRequests = angular.copy(response.data.value);
                $scope.totalItems = $scope.rejectedRequests.length;
            }, function (err) {
                console.error("GetgetRejectedLeaveRequests - Error: ", err);
            })


        $http.get("/webapi/manage-admin/getdepatmentsonemployee")
            .then(function (response) {
                $scope.departments = [{ id: 0, name: "All Departments" }]
                var departments = angular.copy(response.data.value);                
                $scope.departments = $scope.departments.concat(departments);
                $scope.selectedDepartment = $scope.departments[0];

                console.log("$scope.departments: ", $scope.departments);
            }, function (err) {
                console.error(err);
            });

        $scope.selectDepartment = function (departmentId) {
            retrieveRejectedRequests(departmentId);
        }

        var retrieveRejectedRequests = function (departmentId) {
            $http.get("/webapi/approver-manage/GetRejectedLeaveRequests/" + departmentId)
                .then(function (response) {
                    console.log("GetRejectedLeaveRequests Over Department Response: ", response.data.value);
                    $scope.rejectedRequests = angular.copy(response.data.value);
                    $scope.totalItems = $scope.rejectedRequests.length;
                }, function (err) {
                    console.error("GetgetRejectedLeaveRequests - Error: ", err);
                })
        }

        $http.get("/webapi/getchildrenroles")
            .then(function(response) {
                var roles = angular.copy(response.data.value);
                console.log("RESPONSE - Children Roles: ", roles);
            }, function(err) {
                console.error("ERROR - Children Roles: ", roles);
            });

        retrieveRejectedRequests(departmentId);

        //#endregion API CALLS
    }
})();