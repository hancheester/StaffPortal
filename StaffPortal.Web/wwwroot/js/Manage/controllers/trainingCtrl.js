(function () {

    "use strict";

    angular.module('training-app')
        .controller('trainingCtrl', trainingCtrl);

    function trainingCtrl($http, $scope, notify) {
        $http.get("/webapi/currentuser")
            .then(function (response) {
                $scope.user = angular.copy(response.data.value);
                $http.get("/webapi/trainingmodules/" + $scope.user.id)
                    .then(function (response) {
                        $scope.invitations = angular.copy(response.data.value);
                    }, function (err) {
                        console.error("Get training modules for current user - ERROR: ", err);
                    });

                $http.get("/webapi/pendingtrainingmodules/" + $scope.user.id)
                    .then(function (response) {
                        $scope.invitations = angular.copy(response.data.value);
                    }, function (err) {
                        console.error("Get Invitations for current user - ERROR: ", err);
                    });

            }, function (err) {
                console.error("Get Current Employee - ERROR: ", err);
            });
    }
})();