(function() {
    "use strict";

    angular.module("app")
        .directive("equal", function () {
            return {
                require: "ngModel",
                link: function (scope, element, attrs, ctrl) {

                    let validate = function (viewValue) {
                        var comparisonModel = attrs.equal;

                        if (!viewValue || !comparisonModel) {
                            ctrl.$setValidity("equal", true);
                        } else {
                            ctrl.$setValidity("equal", viewValue === comparisonModel);
                        }

                        return viewValue;
                    };

                    ctrl.$parsers.unshift(validate);
                    ctrl.$formatters.push(validate);

                    attrs.$observe("equal", function (comparisonModel) {
                        return validate(ctrl.$viewValue);
                    });
                }
            };
        });
})();