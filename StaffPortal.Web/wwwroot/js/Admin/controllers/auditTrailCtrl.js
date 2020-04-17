(function () {

    "use strict";

    angular.module("admin-app")
        .controller("auditTrailCtrl", auditTrailCtrl);

    function auditTrailCtrl($http, $scope, notify) {
        $scope.auditTrails = [];
        $scope.pageSize = 10;
        $scope.currentPage = 1;
        $scope.totalItems = 0;
        $scope.query = {
            userName: "",
            event: "",
            details: "",
            date: ""
        };

        var getPagedList = function (currentPage, pageSize) {
            var req = {
                pageIndex: currentPage,
                pageSize: pageSize
            };
            $http.post("/webapi/audittrail", req)
                .then(function (response) {
                    $scope.auditTrails = [];
                    var pagedResult = angular.copy(response.data.value);
                    $scope.auditTrails = angular.copy(pagedResult.list);
                    $scope.totalItems = pagedResult.totalItems;
                }, function (err) {
                    console.error("Audit Trail - ERROR: ", err);
                })
        }

        $scope.pageChanged = function () {
            getPagedList($scope.currentPage, $scope.pageSize);
        }

        getPagedList($scope.currentPage, $scope.pageSize);

        $scope.clearSearchFields = function () {
            $scope.query = {
                userName: "",
                event: "",
                details: "",
                date: ""
            };
        }
    }
})();