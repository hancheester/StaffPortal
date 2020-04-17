(function () {
    "use strict";

    angular.module("app")
        .component("shiftCalendarHeader", {
            templateUrl: "/js/shift/shift-calendar-header.component.html",
            bindings: {
                dates: "<",
                next: "&onNext",
                prev: "&onPrev"
            },
            controller: ShiftCalenderHeadarController,
            controllerAs: "vm"
        });

    function ShiftCalenderHeadarController() {
        var vm = this;

        vm.months = [
            "January", "February", "March",
            "April", "May", "June", "July",
            "August", "September", "October",
            "November", "December"
        ];

        vm.days = [
            "Sunday", "Monday", "Tuesday",
            "Wednesday", "Thursday", "Friday",
            "Saturday"
        ];

    }
})();