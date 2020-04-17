(function () {

    "use strict";

    angular.module("app")
        .directive('monthList', function () {
            return {
                templateUrl: "/js/components/month-list.directive.html",
                restrict: "E",
                scope: {
                    months: "="
                },
                controllerAs: "vm",
                controller: ["$scope", MonthListController]
            };

            function MonthListController($scope) {
                var vm = this;                
            }
    });
})();