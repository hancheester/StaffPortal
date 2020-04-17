(function () {
    "use strict";

    angular.module("app")
        .component("card", {
            templateUrl: "/js/dashboard/card.component.html",
            bindings: {
                title: "<",
                body: "<"
            },
            controller: CardController,
            controllerAs: "vm"
        });

    function CardController() {
        var vm = this;
    }
})();