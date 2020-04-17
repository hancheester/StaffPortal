(function () {

    "use strict";

    angular.module('admin_training-app')
        .controller('editModuleCtrl', editModuleCtrl);

    function editModuleCtrl($http, $scope, $timeout, $location, $routeParams, notify, Upload) {

        // #region SCOPE PARAMETERS INITIALIZATION
        $scope.pageSize = 10;
        $scope.currentPage = 1;

        $scope.companyInfo = {};
        $scope.file = {};
        $scope.trainingModule = {
            trainingMaterialsFileNames: [],
            invitedEmployees: []
        };

        $scope.employees = [];
        $scope.departments = [];
        $scope.businessRoles = [];
        $scope.selectedDepartment = {};
        $scope.selectedRole = {};
        $scope.reminder = {};
        $scope.reminder.duration = 0;
        $scope.query = {
            departmentId: 0,
            businessRoleId: 0
        };
        // #endregion END SCOPE PARAMETERS INITIALIZATION

        $scope.trainingModule.id = $routeParams.moduleId;

        $scope.logModule = function () {
            $scope.trainingModule.reminder = $scope.reminder.duration * $scope.reminder.POT;
            console.log("Training Module: ", $scope.trainingModule);
        }

        if ($scope.trainingModule.id === MODE_CREATE) {
            $scope.selectedDepartment.id = 0;
            $scope.trainingModule.id = 0;
            $scope.selectedRole.id = 0;
            $scope.mode = MODE_CREATE;
        }
        else {
            getModule($scope.trainingModule.id);
            $scope.mode = MODE_EDIT;
        }

        // #region API CALLS
        var getModule = function (moduleId) {
            $http.get("/webapi/gettrainingmodule/" + moduleId)
                .then(function (response) {
                    $scope.trainingModule = angular.copy(response.data.value);

                    $http.get("/webapi/getinvitedemployees/" + moduleId)
                        .then(function (response) {
                            $scope.trainingModule.invitedEmployees = response.data.value;
                            console.log("$scope.trainingModule: ", $scope.trainingModule);
                        }, function (err) {
                            console.error("ERROR: ", err);
                        });

                }, function (err) {
                    console.error("Get Module - ERROR: ", err);
                });
        }


        $scope.getEmployees = function () {
            var query = {
                departmentId: $scope.selectedDepartment.id,
                businessRoleId: $scope.selectedRole.id
            };

            $http.get("/webapi/getinvitableemployees/" + $scope.selectedDepartment.id + "/"
                + $scope.selectedRole.id + "/" + $scope.trainingModule.id)
                .then(function (response) {
                    $scope.employees = angular.copy(response.data.value);
                    console.log("Invitable Employees: ", $scope.employees);
                    $scope.totalItems = $scope.employees.length;
                }, function (err) {
                    console.error(err);
                });
        }

        $scope.getEmployees($scope.query);

        $scope.removeSession = function () {
            $http.post("/webapi/removesession")
                .then(function (response) {
                }, function (err) {
                });
        }

        $scope.removeSession();

        $http.get("/webapi/getbusinessroles")
            .then(function (response) {
                $scope.businessRoles = [{ id: 0, name: "All Roles" }];
                $scope.selectedRole = $scope.businessRoles[0];
                $scope.businessRoles = $scope.businessRoles.concat(response.data.value);
            }, function (err) {
                console.error(err);
            })

        $scope.saveTrainingModule = function (form, model) {
            model.reminder = $scope.reminder.duration * $scope.reminder.POT;
            console.log("Sending model: ", model);

            if ($scope.mode === MODE_CREATE) {
                $http.post("/webapi/inserttrainingmodule", model)
                    .then(function (response) {
                        $location.path("/");
                        notify({
                            message: 'Training Module created successfully!',
                            classes: 'full-width success',
                            position: 'left'
                        });
                        console.log("Insert Training Module: ", response.data.value);
                    }, function (err) {
                        $scope.errMessages = [];
                        notify({
                            message: 'Error! Could not create new Training Module',
                            classes: 'full-width danger',
                            position: 'left'
                        });
                        $scope.errMessages = angular.copy(err.data);
                        console.error("Insert Training Module: ", $scope.errMessages);
                        if (!form.$valid) {
                            angular.forEach(form.$$controls, function (value, key) {
                                value.$touched = true;
                                value.$dirty = true;
                            });
                        }
                        $timeout(function () {
                            $scope.errMessages = [];
                        }, TIMEOUT_DURATION);
                    });
            }
            else if ($scope.mode === MODE_EDIT) {
                $http.post("/webapi/edittrainingmodule", model)
                    .then(function (response) {
                        $location.path("/");
                        notify({
                            message: 'Training Module edited successfully!',
                            classes: 'full-width success',
                            position: 'left'
                        });
                        console.log("Edit Training Module:", response.data.value);
                    }, function (err) {
                        $scope.errMessages = [];
                        notify({
                            message: 'Error! Could not Edit new Training Module',
                            classes: 'full-width danger',
                            position: 'left'
                        });
                        $scope.errMessages = angular.copy(err.data);
                        console.error("Edit Training Module:", $scope.errMessages);
                        if (!form.$valid) {
                            angular.forEach(form.$$controls, function (value, key) {
                                value.$touched = true;
                                value.$dirty = true;
                            });
                        }
                        $timeout(function () {
                            $scope.errMessages = [];
                        }, TIMEOUT_DURATION);
                    });
            };
        }

        $scope.uploadPic = function (file) {
            if (typeof file !== "undefined") {
                file.upload = Upload.upload({
                    url: '/webapi/trainingmaterialstemp',
                    data: { file: file },
                });

                file.upload.then(function (response) {
                    file.result = response.data;
                    $scope.errorMsg = response.status + ': ' + response.data;
                    notify({
                        message: 'Document Uploaded Successfully',
                        classes: 'full-width success'
                    });
                    console.log("file.name: ", file.name);
                    $scope.trainingModule.trainingMaterialsFileNames.push(file.name);
                }, function (err) {
                    var message = err.data;
                    notify({
                        message: 'Error! Could not upload the document ' + message.errorMessage,
                        classes: 'full-width danger',
                        position: 'left'
                    });

                    console.error("ERROR - upload document: ", err.data);
                    $scope.errMessages = angular.copy(err.data);
                    $timeout(function () {
                        $scope.errMessages = [];
                    }, TIMEOUT_DURATION);

                }, function (evt) {
                    // Math.min is to fix IE which reports 200% sometimes
                    file.progress = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
                });
            }
            else
                $scope.updateCompanyInfo();
        }

        $scope.removeImage = function (filename, $index) {
            var filepath = {
                folderPath: $scope.trainingModule.trainingMaterialsFolderPath,
                filename: filename
            };

            $http.post("/webapi/deleteimage", filepath)
                .then(function (response) {
                    console.log("delete image: ", response.data.value);
                    $scope.trainingModule.trainingMaterialsFileNames.splice($index, 1);
                }, function (err) {
                    console.error(err);
                });
        }

        $scope.retrieveTempFiles = function () {
            $http.get("/webapi/session_trainingMaterials")
                .then(function (response) {
                    console.log(response.data.value);
                }, function (err) {
                    console.error(err);
                });
        }

        $scope.insertInvitation = function (employee, $index) {
            $scope.employees.splice($index, 1);
            $scope.trainingModule.invitedEmployees.push(employee);
            var invitation = {
                employeeId: employee.id,
                trainingModuleId: $scope.trainingModule.id
            };

            console.log("employeeId, moduleId: ", { employeeId: employee.id, moduleId: $scope.trainingModule.id });

            $http.post("/webapi/insertinvitation", invitation)
                .then(function (response) {
                    console.log("Insert Invitation: ", response.data.value);
                }, function (err) {
                    console.log("Insert Invitation: ", err);
                });
        }

        $scope.removeInvitation = function (employee, $index) {
            $scope.trainingModule.invitedEmployees.splice($index, 1);
            $scope.employees.push(employee);
            var invitation = {
                employeeId: employee.id,
                trainingModuleId: $scope.trainingModule.id
            };

            console.log("employeeId, moduleId: ", { employeeId: employee.id, moduleId: $scope.trainingModule.id });

            $http.post("/webapi/deleteinvitation", invitation)
                .then(function (response) {
                    console.log("Delete Invitation: ", response.data.value);
                }, function (err) {
                    console.log("Delete Invitation: ", err);
                });
        }

        // #endregion END API CALLS
        
        $scope.inlineOptions = {
            customClass: getDayClass,
            minDate: new Date()
        };

        $scope.dateOptions = {
            formatYear: 'yy',
            minDate: new Date()
        };

        $scope.toggleMin = function () {
            $scope.inlineOptions.minDate = $scope.inlineOptions.minDate ? null : new Date();
            $scope.dateOptions.minDate = $scope.inlineOptions.minDate;
        };

        $scope.toggleMin();

        $scope.open1 = function () {
            $scope.popup1.opened = true;
        };

        $scope.open2 = function () {
            $scope.popup2.opened = true;
        };

        $scope.formats = ['dd-MM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];
        $scope.altInputFormats = ['M!/d!/yyyy'];

        $scope.popup1 = {
            opened: false
        };

        $scope.popup2 = {
            opened: false
        };

        function getDayClass(data) {
            var date = data.date,
                mode = data.mode;
            if (mode === 'day') {
                var dayToCheck = new Date(date).setHours(0, 0, 0, 0);

                for (var i = 0; i < $scope.events.length; i++) {
                    var currentDay = new Date($scope.events[i].date).setHours(0, 0, 0, 0);

                    if (dayToCheck === currentDay) {
                        return $scope.events[i].status;
                    }
                }
            }
            return '';
        }

        $scope.mockTrainingModule = function () {
            $scope.trainingModule.name = generateStringField(5);
            $scope.trainingModule.description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do " +
                "eiusmod tempor incididunt ut labore et dolore magna aliqua.";
            $scope.trainingModule.location = generateStringField(5);
            $scope.trainingModule.frequency = 5;
            $scope.trainingModule.reminder = 5;
        }

        function generateStringField(length) {
            var text = "";
            var charScope = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            for (var i = 0; i < length; i++)
                text += charScope.charAt(Math.floor(Math.random() * charScope.length));

            return text;
        }
    }
})();