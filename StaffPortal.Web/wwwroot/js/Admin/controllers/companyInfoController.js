(function () {

    "use strict";

    angular.module('admin-app')
        .controller('companyInfoController', companyInfoController);

    function companyInfoController($http, $scope, $timeout, notify, Upload) {
        $scope.companyInfo = {};
        $scope.file = {};


        $http.get("/webapi/getcompanyinfo")
            .then(function (response) {
                angular.copy(response.data, $scope.companyInfo);
                $scope.companyInfo.financialYearStart = new Date($scope.companyInfo.financialYearStart);
                $scope.companyInfo.financialYearEnd = new Date($scope.companyInfo.financialYearEnd);
            }, function (err) {
                console.log(err);
            });

        $scope.$watch('companyInfo.financialYearStart', function () {
            var m = moment($scope.companyInfo.financialYearStart);
            $scope.companyInfo.financialYearEnd = m.add(1, 'year').add(-1, 'days');            
        });

        $scope.updateCompanyInfo = function () {
            //checkDate
            $http.post("/webapi/editcompanyinfo", $scope.companyInfo)
                .then(function (response) {
                    notify({
                        message: 'Company Information Updated Successfully',
                        classes: 'full-width success'
                    });
                }, function (err) {
                    notify({
                        message: 'Error! Could not update Company Information',
                        classes: 'full-width danger',
                        position: 'left'
                    });
                    console.log("ERROR - edit company info: ", err.data.value);
                    $scope.errMessages = angular.copy(err.data.value);
                    $timeout(function () {
                        $scope.errMessages = [];
                    }, TIMEOUT_DURATION);
                });
        };

        $scope.uploadPic = function (file) {
            if (typeof file !== "undefined") {
                file.upload = Upload.upload({
                    url: '/webapi/editcompanyinfofiles',
                    data: { file: file, model: $scope.companyInfo },
                });

                file.upload.then(function (response) {
                    $timeout(function () {
                        file.result = response.data;
                        $scope.errorMsg = response.status + ': ' + response.data;
                        notify({
                            message: 'Company Information Updated Successfully',
                            classes: 'full-width success'
                        });
                    });
                }, function (err) {
                    if (response.status > 0) {
                        notify({
                            message: 'Error! Could not update Company Information',
                            classes: 'full-width danger',
                            position: 'left'
                        });
                    }
                    console.error("ERROR - edit company info: ", err.data.value);
                    $scope.errMessages = angular.copy(err.data.value);
                    $timeout(function () {
                        $scope.errMessages = [];
                    }, TIMEOUT_DURATION);

                }, function (evt) {
                    // Math.min is to fix IE which reports 200% sometimes
                    file.progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                });
            }
            else
                $scope.updateCompanyInfo();
        }

        $scope.inlineOptions = {
            customClass: getDayClass,
            minDate: new Date()
        };

        $scope.dateOptions = {
            maxMode: 'day',            
            minDate: new Date()
        };

        $scope.toggleMin = function () {
            $scope.inlineOptions.minDate = $scope.inlineOptions.minDate ? null : new Date();
            $scope.dateOptions.minDate = $scope.inlineOptions.minDate;
        };

        $scope.toggleMin();

        $scope.open1 = function () {
            $scope.popup1.opened = true;
        };

        $scope.open2 = function () {
            $scope.popup2.opened = true;
        };

        $scope.formats = ['dd-MM', 'MM/dd', 'dd.MM', 'shortDate'];
        $scope.format = $scope.formats[0];
        $scope.altInputFormats = ['M!/d!'];

        $scope.popup1 = {
            opened: false
        };

        $scope.popup2 = {
            opened: false
        };

        function getDayClass(data) {
            var date = data.date,
                mode = data.mode;
            if (mode === 'day') {
                var dayToCheck = new Date(date).setHours(0, 0, 0, 0);

                for (var i = 0; i < $scope.events.length; i++) {
                    var currentDay = new Date($scope.events[i].date).setHours(0, 0, 0, 0);

                    if (dayToCheck === currentDay) {
                        return $scope.events[i].status;
                    }
                }
            }

            return '';
        }
    }
})();