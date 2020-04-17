(function () {

    "use strict";

    angular.module("app")
        .controller("LeaveApprovalController",
            ["$http", "notify", LeaveApprovalController]);

    function LeaveApprovalController($http, notify) {
        var vm = this;

        vm.pendingLeaveRequests = [];
        vm.calendarData = [];
        vm.departments = [];
        vm.selectedDepartment = {};
        vm.currentDate = null;
        vm.hasNoPendingRequest = false;

        vm.$onInit = init;
        vm.getPendingLeaveRequests = getPendingLeaveRequests;
        vm.getCalendarData = getCalendarData;
        vm.refreshCalendar = refreshCalendar;
        vm.approve = approve;
        vm.reject = reject;

        function init() {
            vm.getPendingLeaveRequests()
                .then(function () {
                    if (vm.pendingLeaveRequests && vm.pendingLeaveRequests.length > 0) {
                        vm.refreshCalendar();
                    }
                });

            $http.get("/api/department/v1/my-departments")
                .then(function (response) {
                    vm.departments = angular.copy(response.data.value);
                    vm.selectedDepartment = vm.departments[0];
                }, function (err) {
                    console.error(err);
                });
        }

        function getPendingLeaveRequests() {
            return $http.get("/api/leave/v1/pending-leave-requests")
                .then(function (response) {
                    vm.pendingLeaveRequests = angular.copy(response.data.value);
                    vm.hasNoPendingRequest = vm.pendingLeaveRequests.length === 0;
                }, function (err) {
                    console.error(err);
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

            $http.get("/api/leave/v1/departmental-calendar/" + vm.selectedDepartment.id, {
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

        function refreshCalendar() {
            if (vm.currentDate === null) {
                vm.currentDate = moment().utc();
            }
            vm.getCalendarData(vm.currentDate);
        }

        function approve(index) {
            let leaveRequest = vm.pendingLeaveRequests[index];
            leaveRequest.isApproving = true;

            $http.put("/api/leave/v1/leave-request/" + leaveRequest.id + "/approve")            
                .then(function (response) {
                    vm.getPendingLeaveRequests();
                    notify({
                        message: "Leave request approved successfully.",
                        duration: -1,
                        classes: "alert-success",
                        position: "right"
                    });
                }, function (err) {
                    leaveRequest.isApproving = false;
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator.",
                        duration: -1,
                        classes: "alert-danger",
                        position: "right"
                    });
                });
        }

        function reject(index) {
            let leaveRequest = vm.pendingLeaveRequests[index];
            leaveRequest.isRejecting = true;

            let data = leaveRequest.rejectionReason;

            if (data === null) {
                leaveRequest.isRequired = true;
                return;
            }

            $http.put("/api/leave/v1/leave-request/" + leaveRequest.id + "/reject", data)
                .then(function (response) {
                    vm.getPendingLeaveRequests();
                    notify({
                        message: "Leave request rejected successfully.",
                        duration: -1,
                        classes: "alert-success",
                        position: "right"
                    });
                }, function (err) {
                    leaveRequest.isRejecting = false;
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator.",
                        duration: -1,
                        classes: "alert-danger",
                        position: "right"
                    });
                });
        }
    }
})();