angular
    .module('admin-app')
    .directive('laterThan', laterThan);

function laterThan($http, $q) {
    return {
        require: 'ngModel',
        scope: {
            otherModelValue: "=laterThan"
        },
        link: function (scope, element, attr, ngModel) {
            console.log(ngModel);

            console.log(ngModel.$viewValue[0]);
            ngModel.$validators.laterThan = function (modelValue) {
                return modelValue > scope.otherModelValue;
            };

            scope.$watch("otherModelValue", function () {
                ngModel.$validate();
            });
        }
    }
}