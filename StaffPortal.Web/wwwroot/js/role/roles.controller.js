(function() {
    "use strict";

    angular.module("app")
        .controller("RolesController", 
        ["$http", "notify", RolesController]);

    function RolesController($http, notify) {
        var vm = this;

        vm.roleTree = {};
        vm.role = {};
        vm.roles = [];
        vm.permissions = [];
        vm.isBusyCreating = false;
        vm.isBusyUpdating = false;
        vm.isBusyDeleting = false;

        vm.$onInit = init;
        vm.getTree = getTree;
        vm.selectRole = selectRole;
        vm.getRoles = getRoles;
        vm.getPermissions = getPermissions;
        vm.updatePermission = updatePermission;
        vm.create = create;
        vm.update = update;
        vm.remove = remove;

        function init() {
            vm.getTree();
            vm.getRoles();
            vm.getPermissions();
        }

        function getTree() {
            $http.get("/api/role/v1/role-tree")
                .then(function (response) {
                    let tree = angular.copy(response.data.value);
                    vm.roleTree = tree;
                }, function (err) {
                    console.error(err);
                });
        }

        function getRoles() {
            $http.get("/api/role/v1/roles")
                .then(function (response) {
                    let roles = angular.copy(response.data.value);
                    vm.roles = roles;
                }, function (err) {
                    console.error(err);
                });
        }

        function getPermissions() {
            $http.get("/api/permission/v1/permissions")
                .then(function (response) {
                    let permissions = angular.copy(response.data.value);
                    vm.permissions = permissions;
                }, function (err) {
                    console.error(err);
                });
        }

        function updatePermission(permission) {
            permission.isBusyUpdating = true;
            let isAllowed = permission.isAllowed;
            let permissionId = permission.id;
            let handler = "";

            if (isAllowed) {
                handler = "allow-permission";
            } else {
                handler = "disallow-permission";
            }

            $http.put("/api/role/v1/role/" + vm.role.id + "/" + handler, permissionId)
                .then(function (response) {
                    notify({
                        message: "Permission updated successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator. " + err.data.message,
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    permission.isBusyUpdating = false;
                });
        }

        function selectRole(id) {
            vm.getPermissions();

            $http.get("/api/role/v1/role/" + id)
                .then(function (response) {
                    let role = angular.copy(response.data.value);
                    vm.role = role;
                    vm.role.parentBusinessRole = vm.roles.find(function (element) {
                        return element.id === role.parentBusinessRoleId;
                    });
                    let permissionIds = vm.role.permissions.map(function (current) {
                        return current.id;
                    });
                    vm.permissions.forEach(function (current, index) {
                        if (permissionIds.includes(current.id)) {
                            vm.permissions[index].isAllowed = true;
                        }
                    });
                }, function (err) {
                    console.error(err);
                });
        }

        function create() {
            vm.isBusyCreating = true;
            let data = {
                name: vm.role.name,
                parentBusinessRoleId: vm.role.parentBusinessRole.id
            };

            $http.post("/api/role/v1/role", data)
                .then(function (response) {
                    let data = angular.copy(response.data);
                    vm.role.id = data.id;
                    notify({
                        message: "Business role created successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                    vm.getTree();
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator. " + err.data.message,
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.isBusyCreating = false;
                });
        }

        function update() {
            vm.isBusyUpdating = true;
            let data = {
                id: vm.role.id,
                name: vm.role.name,
                parentBusinessRoleId: vm.role.parentBusinessRole.id
            };

            $http.put("/api/role/v1/role/" + data.id, data)
                .then(function (response) {
                    notify({
                        message: "Business role updated successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                    vm.getTree();
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator. " + err.data.message,
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.isBusyUpdating = false;
                });
        }

        function remove() {
            vm.isBusyDeleting = true;

            $http.delete("/api/role/v1/role/" + vm.role.id)
                .then(function (response) {
                    vm.role = {};
                    notify({
                        message: "Role deleted successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                    vm.getTree();
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator.",
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.isBusyDeleting = false;
                });
        }
    }
})();