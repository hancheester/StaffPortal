(function () {

    "use strict";

    angular.module("manage-app")
        .controller("editAccountCtrl", editAccountCtrl);

    function editAccountCtrl($http, $scope, notify) {
        $scope.departments = [];

        $http.get("/webapi/getdepartments")
            .then(function (response) {
                $scope.departments = angular.copy(response.data.value);
            }, function (err) {
                console.error(err);
            })

        $http.get("/webapi/GetCurrentEmployee")
            .then(function (response) {
                $scope.isBusy = false;
                $scope.employee = angular.copy(response.data.value);
                $scope.employee.startDate = new Date($scope.employee.startDate);
                $scope.employee.endDate = new Date($scope.employee.endDate);
                $scope.employee.dob = new Date($scope.employee.dob);

                for (var i = 0; i < $scope.employee.daysWorking.length; i++) {
                    $scope.employee.daysWorking[i].startTime = toDate($scope.employee.daysWorking[i].startTime);
                    $scope.employee.daysWorking[i].endTime = toDate($scope.employee.daysWorking[i].endTime);
                    $scope.employee.daysWorking[i].selectedOption = {
                        id: $scope.employee.daysWorking[i].departmentId,
                        name: $scope.employee.daysWorking[i].departmentName
                    };
                }
            }, function (err) {
                $scope.isBusy = false;
                console.log(err);
            });

        $scope.startTimeChanged = function (index, workingDay) {
            if ($scope.employee.daysWorking[index].endTime < workingDay.startTime)
                $scope.employee.daysWorking[index].endTime = workingDay.startTime;
        }

        $scope.saveChanges = function () {
            var m = angular.copy($scope.employee.user);
            console.log("m: ", m);
            var model = {
                firstName: m.firstName,
                lastName: m.lastName,
                gender: m.gender,
                phoneNumber: m.phoneNumber,
                email: m.email,
                password: "",
                repeatPassword: ""
            };

            if ($scope.employee.password && $scope.employee.repeatPassword) {
                model.password = $scope.employee.password;
                model.repeatPassword = $scope.employee.repeatPassword;
            }

            console.log("model: ", model);

            $http.post("/webapi/user-manage/edituser", model)
                .then(function (response) {
                    console.log(response);
                    notify({
                        message: 'Your personal information has been updated successfully',
                        classes: 'full-width success'
                    });

                }, function (err) {
                    notify({
                        message: 'Error! Could not update your personal information',
                        classes: 'full-width danger',
                        position: 'left'
                    });
                });
        }

        $scope.mstep = 5;
        $scope.hstep = 1;

        var toDate = function (string) {
            var hh = string.substring(0, 2);
            var mm = string.substring(3, 5);
            var time = new Date();
            time.setHours(hh, mm, "00");
            return time;
        }

        var toTimeSpan = function (date) {
            if (date instanceof Date)
                return date.getHours() + ":" + date.getMinutes() + ":" + date.getSeconds();
            return date;
        }

        // #region DATEPICKER POPUP

        $scope.dateOptions = {
            formatYear: 'yy',
            minDate: new Date(1950, 1, 1)
        };

        $scope.datePickerOptions = {
            minDate: new Date(1950, 1, 1)
        };

        $scope.formats = ['dd-MM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];
        $scope.altInputFormats = ['M!/d!/yyyy'];

        $scope.openDOB = function () {
            $scope.popupDOB.opened = true;
        };

        $scope.openStartDate = function () {
            $scope.popupStartDate.opened = true;
        };

        $scope.openEndDate = function () {
            $scope.popupEndDate.opened = true;
        };

        $scope.popupDOB = {
            opened: false
        };

        $scope.popupStartDate = {
            opened: false
        };

        $scope.popupEndDate = {
            opened: false
        };

        $scope.momentFormat = function (date, format) {
            var m = moment(date);
            return m.format(format);
        }

        $scope.toDepName = function (depId) {
            var dep = $scope.departments.find(function (arrVal) {
                if (arrVal.id === depId)
                    return arrVal;
            });
            if (dep !== undefined)
                return dep.name;
            else
                return "Unassigned";
        }

        // #endregion DATEPICKER POPUP
    }
})();