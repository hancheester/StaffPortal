(function () {
    "use strict";

    angular.module("app")
        .component("cardLeaveQuota", {
            templateUrl: "/js/dashboard/card-leave-quota.component.html",
            bindings: {
                title: "<"
            },
            controller: ["$http", CardLeaveQuotaController],
            controllerAs: "vm"
        });

    function CardLeaveQuotaController($http) {
        var vm = this;

        vm.isBusy = false;
        vm.leaveQuota = {};
        vm.leaveQuotaTotalChart = {};
        vm.leaveQuotaChart = {};

        vm.$onInit = init;
        vm.getLeaveQuota = getLeaveQuota;

        function init() {
            vm.getLeaveQuota();
        }

        function getLeaveQuota() {
            vm.isBusy = true;

            $http.get("/api/leave/v1/leave-quota")
                .then(function (response) {
                    vm.leaveQuota = angular.copy(response.data.value);

                    var initial = vm.leaveQuota.total - vm.leaveQuota.accruedAsDay;

                    vm.leaveQuotaTotalChart = {
                        labels: ["Holiday Allowance", "Accrued As Day"],
                        colors: ["rgba(151,187,205,0.7)", "rgba(45,255,150,0.6)"],
                        data: [initial, vm.leaveQuota.accruedAsDay],
                        options: {
                            cutoutPercentage: 60
                        }
                    };

                    vm.leaveQuotaChart = {
                        labels: ["Used",
                            "Pending",
                            "Available",
                            "No Impact On Allowance",
                            "Accrued As Pay"],

                        colors: ["rgba(255,45,45,0.7)",
                            "rgba(255,255,214,0.4)",
                            "rgba(45,255,150,0.2)",
                            "rgba(155,115,150,0.2)",
                            "rgba(155,165,150,0.2)"],

                        data: [vm.leaveQuota.used,
                        vm.leaveQuota.pending,
                        vm.leaveQuota.remaining,
                        vm.leaveQuota.noImpact,
                        vm.leaveQuota.accruedAsPay],

                        options: {
                            cutoutPercentage: 60
                        }
                    };

                }, function (err) {                    
                    console.error(err);
                })
                .then(function () {
                    vm.isBusy = false;
                });
        }
    }
})();