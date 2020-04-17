(function () {

    "use strict";

    angular.module('admin-app').directive('subMenu', function () {
        return {
            templateUrl: "/js/Admin/views/subMenu.html",
            restrict: "E",
            scope: {
                items: '='
            },
            controller: function ($scope) {
                $scope.selectItem = function (item, $event) {
                    $scope.$parent.selectItem(item, $event);
                };
            }
        };
    });
})();