(function () {

    "use strict";

    angular.module("admin_training-app", ['ngRoute', 'ui.bootstrap', 'cgNotify', 'ngFileUpload'])
        .config(function ($routeProvider) {
            $routeProvider.when("/", {
                controller: "trainingCtrl",
                templateUrl: "/js/Admin/views/training.html"
            });

            $routeProvider.when("/editor/:moduleId", {
                controller: "editModuleCtrl",
                templateUrl: "/js/Admin/views/editModule.html"
            });

            $routeProvider.otherwise({ redirectTo: "/" });
        })
        .filter('startFrom', function () {
            return function (data, start) {
                return data.slice(start)
            }
        });
})();