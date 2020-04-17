(function () {

    "use strict";

    angular.module("manage-app", ['ngRoute', 'ui.bootstrap', 'ngAnimate', 'chart.js', 'controls', 'cgNotify'])
        .config(function ($routeProvider) {
            $routeProvider.when("/", {
                controller: "LeaveApprovalController",
                templateUrl: "/js/leave/leave-approval.html"
            });

            $routeProvider.when("/rejectedRequests", {
                controller: "rejectedRequestsCtrl",
                templateUrl: "/js/Manage/views/rejectedRequests.html"
            });

            $routeProvider.otherwise({ redirectTo: "/" });
        })
        .filter('startFrom', function () {
            return function (data, start) {
                return data.slice(start);
            };
        });
})();