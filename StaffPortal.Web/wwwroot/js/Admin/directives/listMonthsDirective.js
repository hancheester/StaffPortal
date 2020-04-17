(function () {

    "use strict";

    angular.module('admin-app').directive('listMonths', function () {
        return {
            templateUrl: "/js/Admin/views/listMonths.html",
            restrict: "E",
            scope: {
                months: '='
            },
            controller: function ($scope) {
                $scope.selecedtMonths = function (months) {
                    $scope.$parent.selecedtMonths(months);
                };
            }
        };
    });
})();