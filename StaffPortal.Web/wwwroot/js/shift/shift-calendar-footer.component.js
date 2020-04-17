(function () {
    "use strict";

    angular.module("app")
        .component("shiftCalendarFooter", {
            templateUrl: "/js/shift/shift-calendar-footer.component.html",
            bindings: {
                reports: "<"
            },
            controller: ShiftCalenderFooterController,
            controllerAs: "vm"
        });

    function ShiftCalenderFooterController() {

    }
})();