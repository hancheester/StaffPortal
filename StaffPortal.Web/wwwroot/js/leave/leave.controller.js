(function () {

    "use strict";

    angular.module("app")
        .controller("LeaveController",
            ["$scope", "$http", "notify", LeaveController]);

    function LeaveController($scope, $http, notify) {

        var vm = this;

        vm.history = {
            currentPage: 1,
            pageSize: 5,
            leaveRequests: [],
            totalRequests: 0,
            query: {},
            isBusy: false
        };

        vm.leaveRequest = {
            leaveType: undefined,
            isEmergency: false,
            note: "",
            requestedDates: [],
            isBusy: false
        };

        vm.calendarIsBusy = false;
        vm.calendarLeaveSettings = null;
        vm.quotaIsBusy = false;
        vm.calendarData = [];
        vm.leaveTypes = [];
        vm.leaveQuota = {};
        vm.leaveQuotaChart = {};
        vm.leaveQuotaTotalChart = {};
        
        vm.$onInit = init;
        vm.searchLeaveRequests = searchLeaveRequests;
        vm.submitLeaveRequest = submitLeaveRequest;
        vm.getCalendarData = getCalendarData;
        vm.getLeaveQuota = getLeaveQuota;
        vm.removeRequestedDate = removeRequestedDate;

        function init() {
            let today = moment().utc();
            vm.getCalendarData(today);

            vm.getLeaveQuota();

            $http.get("/api/leave/v1/requestable-leave-types")
                .then(function (response) {
                    vm.leaveTypes = angular.copy(response.data.value);
                }, function (err) {
                    console.error(err);
                });

            $http.get("/api/leave/v1/calendar-leave-settings")
                .then(function (response) {
                    vm.calendarLeaveSettings = angular.copy(response.data.value);
                }, function (err) {
                    console.error(err);
                });

            $scope.$watch("vm.history.currentPage + vm.history.pageSize", function () {
                vm.searchLeaveRequests();
            });
        }

        function searchLeaveRequests() {
            vm.history.isBusy = true;
            let params = {
                pageNumber: vm.history.currentPage,
                pageSize: vm.history.pageSize
            };

            $http.get("/api/leave/v1/leave-requests", {
                params
            }).then(function (response) {
                vm.history.leaveRequests = angular.copy(response.data.value.requests);
                vm.history.totalRequests = response.data.value.total;
            }, function (err) {
                console.error(err);
            }).then(function () {
                vm.history.isBusy = false;
            });
        }

        function submitLeaveRequest() {
            vm.leaveRequest.isBusy = true;
            let data = {
                requestedDates: vm.leaveRequest.requestedDates,
                leaveTypeId: vm.leaveRequest.leaveType.id,
                isEmergency: vm.leaveRequest.isEmergency,
                note: vm.leaveRequest.note
            };

            $http.post("/api/leave/v1/leave-request", data)
                .then(function (response) {
                    notify({
                        message: "Leave request submitted successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });

                    vm.getLeaveQuota();
                    vm.searchLeaveRequests();
                    vm.leaveRequest.requestedDates = [];
                    vm.leaveRequest.leaveType = undefined;
                    vm.leaveRequest.isEmergency = false;
                    vm.leaveRequest.note = "";                    
                    vm.leaveRequest.isBusy = false;

                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator.",
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                    vm.leaveRequest.isBusy = false;
                });
        }

        function getCalendarData(date) {
            vm.calendarIsBusy = true;
            let startDate = date.clone().date(0).day(1);
            let endDate = date.clone().add(1, "months").date(0).day(7);

            let params = {
                fromDate: startDate.format("YYYYMMDD[000000]"),
                toDate: endDate.format("YYYYMMDD[000000]")
            };

            $http.get("/api/leave/v1/personal-calendar", {
                params
            })
            .then(function (response) {
                vm.calendarData = angular.copy(response.data.value);                
                vm.calendarIsBusy = false;
            }, function (err) {
                console.error(err);
                vm.calendarIsBusy = false;
            });
        }

        function getLeaveQuota() {
            vm.quotaIsBusy = true;

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
                    vm.quotaIsBusy = false;
                });
        }

        function removeRequestedDate(position) {
            vm.leaveRequest.requestedDates.splice(position, 1);
        }        
    }
})();