(function () {
    "use strict";

    angular.module("app")
        .component("cardMyLeaveRequest", {
            templateUrl: "/js/dashboard/card-my-leave-request.component.html",
            bindings: {
                title: "<"
            },
            controller: ["$http", CardMyLeaveRequestController],
            controllerAs: "vm"
        });

    function CardMyLeaveRequestController($http) {
        var vm = this;

        vm.isBusy = false;
        vm.leaveRequests = [];

        vm.$onInit = init;
        vm.getLeaveRequests = getLeaveRequests;

        function init() {
            vm.getLeaveRequests();
        }

        function getLeaveRequests() {
            vm.isBusy = true;
            let params = {
                pageNumber: 1,
                pageSize: 3
            };

            $http.get("/api/leave/v1/leave-requests", {
                params
            }).then(function (response) {
                vm.leaveRequests = angular.copy(response.data.value.requests);                
            }, function (err) {
                console.error(err);
            }).then(function () {
                vm.isBusy = false;
            });
        }
    }
})();