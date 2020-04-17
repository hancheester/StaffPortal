(function () {

    "use strict";

    angular.module("admin-app")
        .controller("businessRolesCtrl", businessRolesCtrl);

    function businessRolesCtrl($scope, $http, $log, notify, $timeout) {
        $scope.businessRolesHierarchy = [];
        $scope.businessRoles = [];
        $scope.newBusinessRole = {};
        var role_permissions = [];

        var getHierarchy = function () {
            $http.get("/webapi/getbusinessroleshierarchy")
                .then(function (response) {
                    $scope.businessRolesHierarchy = angular.copy(response.data.value);
                    $scope.selectedRole = $scope.businessRolesHierarchy[0];
                    getAppPermissions();
                    console.log("Business Roles Hierarchy: ", $scope.businessRolesHierarchy)
                }, function (err) {
                    console.error(err);
                });
        }

        getHierarchy();

        var getBusinessRoles = function () {
            $http.get("/webapi/getbusinessroles")
                .then(function (response) {
                    $scope.businessRoles = angular.copy(response.data.value);
                    $scope.selectedRole = $scope.businessRoles[0];
                    getAppPermissions();
                }, function (err) {
                    console.error(err);
                });
        }

        getBusinessRoles();

        var getAppPermissions = function () {
            $http.get("/webapi/getpermissions")
                .then(function (response) {
                    $scope.selectedRole.appPermissions = [];
                    console.log("selected role: ", $scope.selectedRole);
                    role_permissions = [];
                    $scope.appPermissions = angular.copy(response.data);

                    if ($scope.selectedRole.permissions !== null) {
                        for (var i = 0; i < $scope.selectedRole.permissions.length; i++) {
                            var permission = $scope.selectedRole.permissions[i];
                            var index = $scope.appPermissions.findIndex(arrVal => permission.id === arrVal.id);
                            if (index !== undefined)
                                $scope.appPermissions[index].isAllowed = true;
                        }
                    }
                }, function (err) {
                    console.error(err);
                });
        };

        $scope.newItemSelected = function (item, $event) {
            $http.get("/webapi/getbusinessrole/" + item.id)
                .then(function (response) {
                    $scope.selectedRole = angular.copy(response.data);
                    console.log("$scope.businessRoles: ", $scope.businessRoles);

                    $scope.selectedRole.parentBusinessRole = $scope.businessRoles.find(arrVal => arrVal.id === $scope.selectedRole.parentBusinessRoleId);
                    console.log("$scope.selectedRole.parentBusinessRole: ", $scope.selectedRole.parentBusinessRole);
                    getAppPermissions();
                }, function (err) {
                    console.error(err);
                });
            var li = angular.element('side-menu li.selected');
            console.log("Selected li: ", li);
            if (li !== undefined)
                li.removeClass("selected");

            var el = angular.element($event.target).parent();
            el.addClass("selected");
        };

        $scope.updatePermission = function (permission) {
            // Reproduces the APIModel => BusinessRole_PermissionAPIModel
            var role_permission = {
                businessRoleId: $scope.selectedRole.id,
                permissionId: permission.id,
                isAllowed: permission.isAllowed
            };

            var index = role_permissions.findIndex(arrVal => arrVal.permissionId === role_permission.permissionId);
            if (index === -1)
                role_permissions.push(role_permission);
            else
                role_permissions.splice(index, 1);
        };

        var updatePermissions = function () {
            $http.post("/webapi/businessRole/updatepermissions", role_permissions)
                .then(function (response) {
                    var rsp = angular.copy(response.data.value);
                    console.log("Permissions updated", rsp);
                }, function (err) {
                    console.error(err);
                });
        }

        $scope.updateBusinessRole = function () {
            console.log("selected business role: ", $scope.selectedRole);
            $scope.selectedRole.parentBusinessRoleId = $scope.selectedRole.parentBusinessRole.id;

            $http.post("/webapi/updatebusinessrole", $scope.selectedRole)
                .then(function (response) {
                    var role = angular.copy(response.data.value);
                    console.log("Update Business Role: ", role);
                    updatePermissions();
                    getHierarchy();
                    notify({
                        message: 'Role Updated Successfully',
                        classes: 'full-width success'
                    });
                }, function (err) {
                    notify({
                        message: 'Error! Could not Update Business Role',
                        classes: 'full-width danger',
                        position: 'left'
                    });
                    $scope.errorMessages = angular.copy(err.data.value);
                    console.log("Error Messages: ", { ErrorMessages: $scope.errorMessages });
                    $timeout(function () {
                        $scope.errorMessages = [];
                    }, TIMEOUT_DURATION);
                });
        };

        $scope.createNewRole = function () {

            $http.post("/webapi/createbusinessrole", $scope.newBusinessRole)
                .then(function (response) {
                    $scope.newBusinessRole = {};

                    notify({
                        message: 'New Role Created Successfully',
                        classes: 'full-width success'
                    });
                    $scope.businessRoleForm.$dirty = false;
                    $scope.businessRoleForm.name.$dirty = false;
                    $scope.businessRoleForm.businessRoleSelect.$dirty = false;
                    getHierarchy();
                    getBusinessRoles();
                }, function () {

                    notify({
                        message: 'Error! Could not Create New Role',
                        classes: 'full-width danger',
                        position: 'left'
                    });

                })
        }
    }
})();