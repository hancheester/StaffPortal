(function () {

    "use strict";

    angular.module('admin-app').directive('sideMenu', function () {
        return {
            templateUrl: "/js/Admin/views/sideMenu.html",
            restrict: "E",
            scope: {
                items: '='
            },
            controller: function ($scope) {
                $scope.selectItem = function (item, $event) {
                    $scope.$parent.newItemSelected(item, $event);
                };
            }
        };
    });
})();