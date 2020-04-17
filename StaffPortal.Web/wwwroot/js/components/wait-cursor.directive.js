(function () {

    "use strict";

    angular.module("app")
        .directive("waitCursor", function () {
            return {
                restrict: "E",
                templateUrl: "/js/components/wait-cursor.directive.html"
            };
        });
})();