(function () {
    "use strict";

    angular.module("app")
        .component("shiftCalendarBody", {
            templateUrl: "/js/shift/shift-calendar-body.component.html",
            bindings: {
                departmentId: "<",
                roles: "<",
                getEmployeeRoles: "&onGetEmployeeRoles",
                changeTime: "&onChangeTime",
                changeRole: "&onChangeRole"
            },
            controller: ShiftCalenderBodyController,
            controllerAs: "vm"
        });

    function ShiftCalenderBodyController() {
        var vm = this;
    }
})();