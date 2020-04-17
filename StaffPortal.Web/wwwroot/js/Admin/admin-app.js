(function () {

    "use strict";

    angular.module("admin-app", ['ngRoute', 'ui.bootstrap', 'controls', 'cgNotify', 'ngFileUpload'])
        .config(function ($routeProvider) {
            $routeProvider.when("/", {
                controller: "departmentsCtrl",
                templateUrl: "/js/Admin/views/departmentsView.html"
            });

            $routeProvider.when("/editor/:departmentId", {
                controller: "editDepartmentCtrl",
                templateUrl: "/js/Admin/views/departmentEditorView.html"
            });

            $routeProvider.otherwise({ redirectTo: "/" });
        })
        .filter('startFrom', function () {
            return function (data, start) {
                return data.slice(start)
            }
        });
})();