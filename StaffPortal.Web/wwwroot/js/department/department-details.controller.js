(function() {
    "use strict";

    angular.module("app")
        .controller("DepartmentDetailsController",
            ["$scope", "$http", "notify", DepartmentDetailsController]);

    function DepartmentDetailsController($scope, $http, notify) {
        $scope.init = function (id) {
            vm.id = id;
        };

        var vm = this;

        vm.id = 0;
        vm.department = {};
        vm.isBusyUpdating = false;
        vm.isBusyDeleting = false;

        vm.$onInit = init;
        vm.get = get;
        vm.update = update;
        vm.remove = remove;
        vm.toDate = toDate;
        vm.toTimeSpan = toTimeSpan;
        
        function init() {
            $scope.$watch("vm.id", function () {
                vm.get();
            });
        }

        function get() {
            $http.get("/api/department/v1/department/" + vm.id)
                .then(function (response) {                    
                    let data = angular.copy(response.data.value);
                    vm.department = data;

                    if (vm.department.openingHours) {
                        vm.department.openingHours = vm.department.openingHours.map(function (element) {
                            element.openingTime = vm.toDate(element.openingTime);
                            element.closingTime = vm.toDate(element.closingTime);                            
                            return element;
                        });
                    } else {
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
                    }

                    vm.department.roles = vm.department.roles.map(function (element) {
                        if (element.minimumRequired === 0) {
                            element.minimumRequired = null;
                        }

                        return element;
                    });

                }, function (err) {
                    console.error(err);
                });
        }

        function update() {
            vm.isBusyUpdating = true;

            let data = {
                id: vm.department.id,
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
                roles: vm.department.roles.filter(function (element) {
                    return element.showOnRota;
                })
            };
            
            $http.put("/api/department/v1/department/" + vm.id, data)
                .then(function (response) {
                    notify({
                        message: "Department updated successfully.",
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
                    vm.isBusyUpdating = false;
                });
        }

        function remove() {
            vm.isBusyDeleting = true;

            $http.delete("/api/department/v1/department/" + id)
                .then(function (response) {
                    notify({
                        message: "Department deleted successfully.",
                        duration: -1,
                        position: "right",
                        classes: "alert-success"
                    });
                    vm.departments.splice(index, 1);
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
                    vm.isBusyDeleting = false;
                });
        }

        function toDate(string) {
            var hh = string.substring(0, 2);
            var mm = string.substring(3, 5);
            
            if (hh === "00" && mm === "00")
                return null;

            var time = new Date();
            time.setHours(hh, mm, "00");
            return time;
        }

        function toTimeSpan(date) {
            if (date instanceof Date)
                return date.getHours() + ":" + date.getMinutes() + ":00";
            return date;
        }
    }

})();