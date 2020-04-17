(function () {

    "use strict";

    angular.module("admin_announcements-app", ['ngRoute', 'ui.bootstrap', 'cgNotify', 'ngFileUpload'])
        .config(function ($routeProvider) {
            $routeProvider.when("/", {
                controller: "announcementsCtrl",
                templateUrl: "/js/Admin/views/announcements.html"
            });

            $routeProvider.when("/editor/:announcementId", {
                controller: "editAnnouncementCtrl",
                templateUrl: "/js/Admin/views/editAnnouncement.html"
            });

            $routeProvider.otherwise({ redirectTo: "/" });
        })
        .filter('startFrom', function () {
            return function (data, start) {
                return data.slice(start)
            }
        });
})();