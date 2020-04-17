(function () {
    "use strict";

    angular.module("app")
        .directive("roleTree", function () {
            return {
                restrict: "E",
                templateUrl: "/js/components/role-tree.directive.html",
                scope: {
                    roles: "=",
                    select: "&onSelect"
                },
                controller: ["$scope", RoleTreeController],
                controllerAs: "vm"
            };

            function RoleTreeController($scope) {
                var vm = this;

                vm.selectRole = selectRole;

                function selectRole(id) {
                    $scope.select()(id);
                }
            }
        });
})();