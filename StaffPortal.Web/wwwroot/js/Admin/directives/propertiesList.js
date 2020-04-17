(function () {

    "use strict";

    angular.module('admin-app').directive('propertiesList', function () {
        return {
            templateUrl: "/js/Admin/views/propertiesList.html",
            scope: {
                properties: '='
            },
            controller: function ($scope) {
                $scope.updateProperty = function (property) {
                    console.log($scope);
                    $scope.$parent.propertyUpdated(property);
                }
            }
        }
    });
})();