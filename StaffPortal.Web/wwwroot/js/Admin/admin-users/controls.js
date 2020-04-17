(function () {

    "use strict";

    angular.module("controls", [])
        .directive("waitCursor", waitCursor);

    function waitCursor() {
        return {
            restrict: 'E',
            templateUrl: "/js/Admin/views/waitCursor.html"
        };
    }    
})();