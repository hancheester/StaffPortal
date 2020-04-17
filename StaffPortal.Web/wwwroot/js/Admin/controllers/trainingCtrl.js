(function () {

    "use strict";

    angular.module('admin_training-app')
        .controller('trainingCtrl', trainingCtrl);

    function trainingCtrl($http, $scope, $timeout, notify, Upload) {

        // #region SCOPE PARAMETERS INITIALIZATION
        $scope.trainingModules = [];
        $scope.pageSize = 10;
        $scope.currentPage = 1;
        // #endregion SCOPE PARAMETERS INITIALIZATION

        // #region API CALLS
        $http.get("/webapi/gettrainingmodules")
            .then(function (response) {
                $scope.trainingModules = angular.copy(response.data.value);
            }, function (err) {
                console.error(err);
            });

        // #endregion END API CALLS

        $scope.propertyName = 'name';
        $scope.reverse = true;

        $scope.sortBy = function (propertyName, $element) {
            var th = angular.element($element.currentTarget);            
            $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
            $scope.propertyName = propertyName;
        };
    }
})();