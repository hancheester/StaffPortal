(function() {
    "use strict";

    angular.module("app")
        .controller("DepartmentController",
            ["$http", "notify", DepartmentController]);

    function DepartmentController($http, notify) {
        var vm = this;

        vm.department = {
            openingHours: []
        };
        vm.roles = [];
        vm.isBusy = false;

        vm.$onInit = init;
        vm.getRoles = getRoles;
        vm.create = create;
        vm.toTimeSpan = toTimeSpan;

        function init() {
            let week = [
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday",
                "Sunday"
            ];

            for (var i = 0; i < week.length; i++) {
                vm.department.openingHours.push({
                    day: week[i],
                    isOpen: false,
                    openingTime: "",
                    closingTime: ""
                });
            }

            vm.getRoles();
        }

        function getRoles() {
            $http.get("/api/role/v1/roles")
                .then(function (response) {
                    let roles = angular.copy(response.data.value);

                    roles.forEach(function (element) {
                        vm.roles.push({
                            roleId: element.id,
                            name: element.name,
                            showOnRota: false,
                            minimumRequired: 0
                        });
                    });

                }, function (err) {
                    console.error(err);
                });
        }

        function create() {
            vm.isBusy = true;
            let data = {
                name: vm.department.name,
                minimumRequired: vm.department.minimumRequired,
                openingHours: vm.department.openingHours
                    .filter(function (element) {
                        return element.isOpen;
                    })
                    .map(function (element) {
                    return {
                        day: element.day,
                        isOpen: element.isOpen,
                        openingTime: vm.toTimeSpan(element.openingTime),
                        closingTime: vm.toTimeSpan(element.closingTime)
                    };
                }),
                roles: vm.roles.filter(function (element) {
                    return element.showOnRota;
                })
            };
            
            $http.post("/api/department/v1/department", data)
                .then(function (response) {
                    notify({
                        message: "Department created successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                }, function (err) {
                    console.error(err);
                    notify({
                        message: "Sorry, there is an error. Please contact administrator. " + err.data.message,
                        duration: -1,
                        position: "right",
                        classes: "alert-danger"
                    });
                })
                .then(function () {
                    vm.isBusy = false;
                });
        }

        function toTimeSpan(date) {
            if (date instanceof Date)
                return date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds();
            return date;            
        }
    }
})();