(function () {
    "use strict";

    angular.module("app")
        .component("cardMyCalendar", {
            templateUrl: "/js/dashboard/card-my-calendar.component.html",
            bindings: {
                title: "<"
            },
            controller: ["$scope", "$http", CardMyCalendarController],
            controllerAs: "vm"
        });

    function CardMyCalendarController($scope, $http) {
        var vm = this;

        vm.holidays = {};
        vm.totalMonths = 36; // Total months for 3 years
        vm.counter = 0;
        vm.progress = null;
        vm.options = {};

        vm.$onInit = init;        
        vm.getDayClass = getDayClass;
        vm.getHolidayByMonth = getHolidayByMonth;

        function init() {
            let currentDate = new Date();
            let currentMonth = currentDate.getMonth() + 1;
            let currentYear = currentDate.getFullYear();

            let years = [currentYear - 1, currentYear, currentYear + 1];

            [].forEach.call(years, function (year) {
                let months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
                [].forEach.call(months, function (month) {
                    vm.getHolidayByMonth(month, year)
                        .then(function () {
                            vm.counter = vm.counter + 1;
                            vm.progress = Math.min(100, parseInt(100.0 * vm.counter / vm.totalMonths));
                            if (month === currentMonth && year === currentYear) {
                                $scope.$broadcast("refreshDatepickers");
                            }

                        });
                });
            });            

            vm.options = {
                customClass: vm.getDayClass
            };            
        }

        function getHolidayByMonth(month, year) {
            return $http.get("/api/leave/v1/holiday/month/" + month + "/year/" + year)
                .then(function (response) {
                    let data = angular.copy(response.data.value);
                    vm.holidays[month.toString() + "/" + year.toString()] = data;                    
                }, function (err) {
                    console.error(err);
                });
        }

        function getDayClass(data) {
            var date = data.date,
                mode = data.mode;
            if (mode === "day") {
                let year = date.getFullYear();
                let month = date.getMonth() + 1;                
                let day = date.getDate();
                let key = month.toString() + "/" + year.toString();
                if (vm.holidays[key] && vm.holidays[key].includes(day)) {
                    return "holiday";
                }                
            }

            return "";
        }
    }
})();