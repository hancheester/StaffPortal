(function () {

    "use strict";

    angular.module('admin_announcements-app')
        .controller('editAnnouncementCtrl', editAnnouncementCtrl);

    function editAnnouncementCtrl($http, $scope, $timeout, $location, $routeParams, notify, Upload) {


        // #region SCOPE PARAMETERS INITIALIZATION

        $scope.announcement = {
            recipients: []
        };

        $scope.departments = [];
        $scope.businessRoles = [];
        $scope.selectedDepartments = [];
        $scope.selectedRoles = [];
        $scope.query = {
            departmentId: 0,
            businessRoleId: 0
        };
        // #endregion SCOPE PARAMETERS INITIALIZATION


        $scope.announcement.id = $routeParams.announcementId;

        $http.post("/webapi/removesession")
            .then(function (response) {
            }, function (err) {
            });

        var getAnnouncement = function (announcementId) {
            $http.get("/webapi/getannouncement/" + announcementId)
                .then(function (response) {
                    $scope.announcement = angular.copy(response.data.value.announcement);
                    $scope.announcement.date = new Date($scope.announcement.date);
                    console.log("GET Announcement - $scope.announcement: ", $scope.announcement);
                }, function (err) {
                    console.error("Get Module - ERROR: ", err);
                });
        }

        if ($scope.announcement.id !== "create") {
            getAnnouncement($scope.announcement.id);
            $scope.mode = MODE_EDIT;
        }
        else {
            $scope.announcement.id = 0;
            $scope.mode = MODE_CREATE;
        }

        $http.get("/webapi/getdepartments")
            .then(function (response) {
                $scope.departments = [{ id: 0, name: "All Departments" }];
                $scope.selectedDepartment = $scope.departments[0];
                $scope.departments = $scope.departments.concat(response.data.value);
            }, function (err) {
                $scope.businessRoles = angular.copy(response.data.value);
            });

        $http.get("/webapi/getbusinessroles")
            .then(function (response) {
                $scope.businessRoles = [{ id: 0, name: "All Roles" }];
                $scope.selectedRole = $scope.businessRoles[0];
                $scope.businessRoles = $scope.businessRoles.concat(response.data.value);
            }, function (err) {
                console.error(err);
            })

        $scope.saveAnnouncement = function (form, model) {
            console.log("model: ", model);

            if ($scope.mode === MODE_CREATE) {
                $http.post("/webapi/insertannouncement", model)
                    .then(function (response) {
                        $location.path("/");
                        notify({
                            message: 'Announcement created successfully!',
                            classes: 'full-width success',
                            position: 'left'
                        });
                        console.log("Insert Training Module: ", response.data.value);
                    }, function (err) {
                        $scope.errMessages = [];
                        notify({
                            message: 'Error! Could not create new Announcement',
                            classes: 'full-width danger',
                            position: 'left'
                        });
                        $scope.errMessages = angular.copy(err.data);
                        console.error("Insert Announcement: ", $scope.errMessages);
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
                $http.post("/webapi/editannouncement", model)
                    .then(function (response) {
                        $location.path("/");
                        notify({
                            message: 'Announcement edited successfully!',
                            classes: 'full-width success',
                            position: 'left'
                        });
                        console.log("Edit Training Module:", response.data.value);
                    }, function (err) {
                        $scope.errMessages = [];
                        notify({
                            message: 'Error! Could edit Announcement',
                            classes: 'full-width danger',
                            position: 'left'
                        });
                        $scope.errMessages = angular.copy(err.data);
                        console.error("Edit Announcement:", $scope.errMessages);
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

        $scope.toggleRole = function ($index, $event) {
            var isSelected = !$scope.businessRoles[$index].isSelected;
            var item = $scope.businessRoles[$index];

            if (isSelected) {
                $scope.selectedRoles.push(item.id);
            }
            else {
                console.log("item to delete: ", item);

                var i = $scope.selectedRoles.findIndex(arrVal => item.id === arrVal);
                $scope.selectedRoles.splice(i, 1);
                console.log("selectedRoles: ", $scope.selectedRoles);
            }

            $scope.businessRoles[$index].isSelected = isSelected;
        }

        $scope.toggleDepartment = function ($index, $event) {
            var isSelected = !$scope.departments[$index].isSelected;
            var item = $scope.departments[$index];

            if (isSelected) {
                $scope.selectedDepartments.push(item.id);
            }
            else {
                console.log("item to delete: ", item);
                var i = $scope.selectedDepartments.findIndex(arrVal => item.id === arrVal);
                $scope.selectedDepartments.splice(i, 1);
                console.log("selectedDepartments: ", $scope.selectedDepartments);
            }

            $scope.departments[$index].isSelected = isSelected;
        }

        $scope.addRecipients = function () {

            var query = { businessRolesIds: $scope.selectedRoles, departmentsIds: $scope.selectedDepartments };

            var businessRolesIds = $scope.selectedRoles;
            var departmentsIds = $scope.selectedDepartments;

            console.log("Add Recipients: ", query);

            $http.post("/webapi/addrecipients", query)
                .then(function (response) {

                    $scope.announcement.recipients = angular.copy(response.data.value);
                    console.log("Get Recipients - RESPONSE", $scope.announcement);
                }, function (err) {
                    console.error("Get Recipients - ERROR", err);
                });
        }

        //$scope.retrieveTempFiles = function () {
        //    $http.get("/webapi/session_trainingMaterials")
        //        .then(function (response) {
        //            console.log(response.data.value);
        //        }, function (err) {
        //            console.error(err);
        //        });
        //}

        //$scope.removeSession = function () {
        //    $http.post("/webapi/removesession")
        //        .then(function (response) {
        //        }, function (err) {
        //        });
        //}

        $scope.inlineOptions = {
            customClass: getDayClass,
            minDate: new Date()
        };

        $scope.insertInvitation = function (employee, $index) {
            $scope.employees.splice($index, 1);
            $scope.announcement.invitedEmployees.push(employee);
            var invitation = {
                employeeId: employee.id,
                trainingannouncementId: $scope.announcement.id
            };

            console.log("employeeId, announcementId: ", { employeeId: employee.id, announcementId: $scope.announcement.id });

            $http.post("/webapi/insertinvitation", invitation)
                .then(function (response) {
                    console.log("Insert Invitation: ", response.data.value);
                }, function (err) {
                    console.log("Insert Invitation: ", err);
                });
        }

        $scope.removeInvitation = function (employee, $index) {
            $scope.announcement.invitedEmployees.splice($index, 1);
            $scope.employees.push(employee);
            var invitation = {
                employeeId: employee.id,
                trainingannouncementId: $scope.announcement.id
            };

            console.log("employeeId, announcementId: ", { employeeId: employee.id, announcementId: $scope.announcement.id });

            $http.post("/webapi/deleteinvitation", invitation)
                .then(function (response) {
                    console.log("Delete Invitation: ", response.data.value);
                }, function (err) {
                    console.log("Delete Invitation: ", err);
                });
        }

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

        $scope.formats = ['dd-MM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
        $scope.format = $scope.formats[0];
        $scope.altInputFormats = ['M!/d!/yyyy'];

        $scope.popup1 = {
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

        $scope.mockAnnouncement = function () {
            $scope.announcement.title = generateStringField(5);
            $scope.announcement.date = randomDate(new Date(), new Date(moment().year() + 1, 0, 1));
            $scope.announcement.body = generateStringField(25);
            console.log("$scope.announcement: ", $scope.announcement);
        }

        function generateStringField(length) {
            var text = "";
            var charScope = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            for (var i = 0; i < length; i++)
                text += charScope.charAt(Math.floor(Math.random() * charScope.length));

            return text;
        }

        function randomDate(start, end) {
            return new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
        }
    }
})();