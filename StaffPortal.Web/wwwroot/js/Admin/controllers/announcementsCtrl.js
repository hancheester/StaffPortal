(function () {

    "use strict";

    angular.module('admin_announcements-app')
        .controller('announcementsCtrl', announcementsCtrl);

    function announcementsCtrl($http, $scope, $timeout, notify, Upload) {
        $scope.announcements = [];
        $scope.pageSize = 10;
        $scope.currentPage = 1;
        
        $http.get("/webapi/getannouncements")
            .then(function (response) {
                $scope.announcements = angular.copy(response.data.value);
            }, function (err) {
                console.error(err);
            });

        $scope.propertyName = 'name';
        $scope.reverse = true;

        $scope.sortBy = function (propertyName, $element) {
            var th = angular.element($element.currentTarget);            
            $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
            $scope.propertyName = propertyName;
        };
    }
})();