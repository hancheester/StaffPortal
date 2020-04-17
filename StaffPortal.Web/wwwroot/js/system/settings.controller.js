(function () {
    "use strict";

    angular.module("app")
        .controller("SettingsController",
        ["$scope", "$http", "notify", "Upload", SettingsController]);

    function SettingsController($scope, $http, notify, Upload) {
        var vm = this;

        vm.company = {};
        vm.email = {};
        vm.leave = {};
        vm.leaveTypes = [];
        vm.leaveType = {};
        vm.logo = null;
        vm.isBusyUpdatingCompany = false;
        vm.isBusyCreatingLeaveType = false;
        vm.isBusyUpdatingEmail = false;
        vm.isCompanyYearOpen = false;

        vm.$onInit = init;
        vm.toDate = toDate;
        vm.isValidDate = isValidDate;
        vm.formatDateDDMM = formatDateDDMM;
        vm.getCompany = getCompany;
        vm.getLeave = getLeave;
        vm.getLeaveType = getLeaveType;
        vm.getEmail = getEmail;
        vm.checkImpactOnAllowance = checkImpactOnAllowance;
        vm.deleteLeaveType = deleteLeaveType;
        vm.createLeaveType = createLeaveType;
        vm.updateCompany = updateCompany;
        vm.updateLeave = updateLeave;
        vm.updateEmail = updateEmail;

        function init() {
            vm.getCompany();
            vm.getLeave();
            vm.getLeaveType();
            vm.getEmail();

            $scope.$watch("vm.company.financialYearStart", function () {
                if (vm.company.financialYearStart instanceof Date) {
                    var m = moment(vm.company.financialYearStart);
                    vm.company.financialYearEnd = m.add(1, "year").add(-1, "days").toDate();
                }
            });
        }

        function getCompany() {
            $http.get("/api/config/v1/company")
                .then(function (response) {
                    let data = angular.copy(response.data.value);
                    vm.company = data;
                    vm.company.financialYearStart = toDate(vm.company.financialYearStart);
                    vm.company.financialYearEnd = toDate(vm.company.financialYearEnd);
                }, function (err) {
                    console.error(err);
                });
        }

        function getLeave() {
            $http.get("/api/config/v1/leave")
                .then(function (response) {
                    let data = angular.copy(response.data.value);
                    vm.leave = data;

                    var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

                    let monthsToAccrue = angular.copy(vm.leave.monthsToAccrue);
                    vm.leave.monthsToAccrue = [];

                    [].forEach.call(months, function (month) {
                        if (monthsToAccrue.indexOf(month) >= 0) {
                            vm.leave.monthsToAccrue.push({
                                name: month,
                                isAccruable: true
                            });
                        } else {
                            vm.leave.monthsToAccrue.push({
                                name: month,
                                isAccruable: false
                            });
                        }                        
                    });
                }, function (err) {
                    console.error(err);
                });
        }

        function getLeaveType() {
            $http.get("/api/leave/v1/leave-types")
                .then(function (response) {
                    let data = angular.copy(response.data.value);
                    vm.leaveTypes = data;
                }, function (err) {
                    console.error(err);
                });
        }

        function getEmail() {
            $http.get("/api/config/v1/email")
                .then(function (response) {
                    let data = angular.copy(response.data.value);
                    vm.email = data;
                }, function (err) {
                    console.error(err);
                });
        }

        function checkImpactOnAllowance (leaveType) {
            if (leaveType.accruable)
                leaveType.impactOnAllowance = true;
        }

        function deleteLeaveType(index) {
            let id = vm.leaveTypes[index].id;
            $http.delete("/api/leave/v1/leave-type/" + id)
                .then(function (response) {
                    notify({
                        message: "Leave type deleted successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                    vm.leaveTypes.splice(index, 1);
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator.",
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                });
        }

        function createLeaveType() {
            vm.isBusyCreatingLeaveType = true;

            let data = vm.leaveType;

            $http.post("/api/leave/v1/leave-type", data)
                .then(function (response) {
                    notify({
                        message: "Leave type created successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                }, function (err) {
                    console.error(err);
                    let message = err.data.message ? err.data.message : "";
                    notify({
                        message: "Sorry, there is an error. Please contact administrator. " + message,
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.getLeaveType();
                    vm.isBusyCreatingLeaveType = false;
                });
        }

        function updateCompany() {
            vm.isBusyUpdatingCompany = true;

            vm.company.financialYearStart = vm.formatDateDDMM(vm.company.financialYearStart);
            vm.company.financialYearEnd = vm.formatDateDDMM(vm.company.financialYearEnd);

            let data = {
                logo: vm.logo,
                model: vm.company
            };

            Upload.upload({
                method: "PUT",
                url: "/api/config/v1/company/0",
                data: data
            }).then(function () {
                notify({
                    message: "Company updated successfully.",
                    duration: -1,
                    position: "right",
                    classes: "alert-success"
                });
            }, function (err) {
                console.error(err);
                notify({
                    message: "Sorry, there is an error. Please contact administrator. " + err.data.value.message,
                    duration: -1,
                    position: "right",
                    classes: "alert-danger"
                });
            }, function (evt) {
                if (vm.logo) {
                    // Math.min is to fix IE which reports 200% sometimes
                    vm.logo.progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                }                
            })
            .then(function () {
                vm.getCompany();

                vm.isBusyUpdatingCompany = false;

                if (vm.logo) {
                    vm.logo.progress = -1;
                }                
            });
        }

        function updateLeave() {
            vm.isBusyUpdatingLeave = true;

            let data = angular.copy(vm.leave);

            data.monthsToAccrue = vm.leave.monthsToAccrue.filter(function (element) {
                return element.isAccruable;
            }).map(function (element) {
                return element.name;
            });

            $http.put("/api/config/v1/leave/0", data)
                .then(function (response) {
                    notify({
                        message: "Leave settings updated successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });  
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator.",
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.getLeave();
                    vm.isBusyUpdatingLeave = false;
                });
        }

        function updateEmail() {
            vm.isBusyUpdatingEmail = true;

            let data = angular.copy(vm.email);

            $http.put("/api/config/v1/email/0", data)
                .then(function (response) {
                    notify({
                        message: "Email settings updated successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator.",
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.getEmail();
                    vm.isBusyUpdatingEmail = false;
                });
        }

        function toDate(value) {
            let year = (new Date()).getFullYear();

            if (vm.isValidDate(value + "/" + year)) {
                var dateParts = value.split("/");
                return new Date(year, dateParts[1] - 1, dateParts[0]);
            }
            
            return new Date();
        }

        function isValidDate(value) {
            let d = moment(value, "D/M/YYYY");
            if (d === null || !d.isValid()) return false;

            return value.indexOf(d.format("D/M/YYYY")) >= 0
                || value.indexOf(d.format("DD/MM/YYYY")) >= 0
                || value.indexOf(d.format("D/M/YY")) >= 0
                || value.indexOf(d.format("DD/MM/YY")) >= 0;
        }

        function formatDateDDMM(value) {
            return value.getDate() + "/" + (value.getMonth() + 1);
        }
    }
})();