(function () {

    "use strict";

    angular.module("training-app", ['ngRoute', 'ui.bootstrap', 'ngAnimate', 'controls', 'cgNotify'])
        //.config(function ($routeProvider) {
        //    $routeProvider.when("/", {
        //        controller: "leaveApprovalCtrl",
        //        templateUrl: "/js/Manage/views/leaveApproval.html"
        //    });

        //    $routeProvider.when("/rejectedRequests", {
        //        controller: "rejectedRequestsCtrl",
        //        templateUrl: "/js/Manage/views/rejectedRequests.html"
        //    });

        //    $routeProvider.otherwise({ redirectTo: "/" });
        //})
        .filter('startFrom', function () {
            return function (data, start) {
                return data.slice(start)
            }
        });
})();