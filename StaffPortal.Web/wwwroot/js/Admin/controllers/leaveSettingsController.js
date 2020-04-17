(function () {

    "use strict";

    angular.module('admin-app')
        .controller('leaveSettingsController', leaveSettingsController);

    function leaveSettingsController($http, $scope, notify) {

        // #region SCOPE PARAMETERS INITIALIZATION
        $scope.leaveSettings = {};
        $scope.leaveTypes = [];
        // #endregion END SCOPE PARAMETERS INITIALIZATION

        $scope.checkImpactOnAllowance = function (leaveType) {
            if (leaveType.accruable)
                leaveType.impactOnAllowance = true;
        }

        // #region API CALLS

        $scope.saveLeaveSettings = function () {
            var jobject = {
                leaveSettings: $scope.leaveSettings,
                defaultLeaveType: $scope.leaveTypes[0]
            };

            console.log(jobject);

            $http.post("/webapi/editleavesettings", jobject)
                .then(function (response) {
                    var toUpdate = $scope.leaveTypes.slice(1);
                    console.log(toUpdate);
                    $http.post("/webapi/editleavetypes", toUpdate)
                    notify({
                        message: 'Leave Settings Updated Successfully',
                        classes: 'full-width success'
                    });

                }, function (err) {
                    notify({
                        message: 'Error! Could not update Leave Settings',
                        classes: 'full-width danger',
                        position: 'left'
                    });
                });
        }

        $http.get("/webapi/getleavesettings")
            .then(function (response) {
                angular.copy(response.data, $scope.leaveSettings);
            }, function (err) {
                console.log(err.statusText);
            });

        $http.get("/api/leave/v1/leave-types")
            .then(function (response) {
                var types = angular.copy(response.data.value);
                $scope.leaveTypes = $scope.leaveTypes.concat(types);
            }, function (err) {
                console.log(err.statusText);
            });

        $scope.createNewLeaveType = function (newLeaveType, form) {
            $http.post("/webapi/createleavetype", newLeaveType)
                .then(function (response) {
                    notify({
                        message: 'New Leave Type Created Successfully',
                        classes: 'full-width success'
                    });
                    var newLT = angular.copy(response.data);
                    console.log(newLT);
                    $scope.leaveTypes.push(newLT);
                    $scope.newLeaveType = {};
                    angular.forEach(form.$$controls, function (value, key) {
                        value.$touched = false;
                        value.$dirty = false;
                    });
                }, function (err) {
                    notify({
                        message: 'Error! Could not Create New Leave Type',
                        classes: 'full-width danger',
                        position: 'left'
                    });
                });
        }

        $scope.deleteLeaveType = function (leaveType, index) {
            console.log(leaveType.id);
            $http.delete("/webapi/deleteleavetype?leaveTypeId=" + leaveType.id)
                .then(function (response) {
                    $scope.leaveTypes.splice(index, 1);
                    notify({
                        message: 'Leave Type Deleted Successfully',
                        classes: 'full-width success'
                    });

                }, function (err) {
                    notify({
                        message: 'Error! Could not Delete Leave Type',
                        classes: 'full-width danger',
                        position: 'left'
                    });

                });
        }

        // #endregion END API CALLS
    }
})();