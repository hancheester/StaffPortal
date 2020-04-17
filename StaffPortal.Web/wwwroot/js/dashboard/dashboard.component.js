(function () {
    "use strict";

    angular.module("app")
        .component("dashboard", {
            templateUrl: "/js/dashboard/dashboard.component.html",
            controller: ["$scope", "$compile", "$element", DashboardController],
            controllerAs: "vm"
        });

    function DashboardController($scope, $compile, $element) {
        var vm = this;

        vm.$onInit = init;
        vm.createGapBox = createGapBox;
        vm.initDrapDropHandlers = initDrapDropHandlers;
        
        function init() {
            let cards = [
                {
                    position: 1,
                    type: "card-leave-quota",
                    title: "Leave Quota"
                },
                {
                    position: 2,
                    type: "card-my-leave-request",
                    title: "Upcoming Leave Requests"
                },
                {
                    position: 3,
                    type: "card-my-calendar",
                    title: "My Calendar"
                },
                {
                    position: 4,
                    title: "Approved / Pending Hours"
                },
                {
                    position: 5,
                    title: "Announcements"
                },
                {
                    position: 6,
                    title: "Today\\'s Shifts"
                },
                {
                    position: 7,
                    title: "Notifications"
                },
                {
                    position: 9,
                    title: "Last Clock In/Out"
                },
                {
                    position: 12,
                    title: "Upcoming Trainings"
                },
                {
                    position: 15,
                    title: "Quick Links"
                },
                {
                    position: 18,
                    title: "Latest Resources"
                }
            ];

            var columns = angular.element(".column");

            [].forEach.call(cards, function (card) {

                if (!card.type) return;

                let template = "<" + card.type + " class='box' title=\"'" + card.title + "'\"></" + card.type + ">";
                let newScope = $scope.$new(true, $scope);
                newScope = angular.merge(newScope, card);

                let compiled = $compile(template)(newScope);
                let remainder = card.position % 3;
                let position = remainder !== 0 ? remainder - 1 : 2;
                angular.element(columns[position]).append(compiled).append(vm.createGapBox());
            });

            [].forEach.call(columns, function (column) {
                if (column.children.length === 0) {
                    angular.element(column).append(vm.createGapBox());
                }
            });

            angular.element(function () {
                initDrapDropHandlers();
            });
        }

        function createGapBox() {
            let template = "<div class='box gap'></div>";
            return $compile(template)($scope);
        }

        function initDrapDropHandlers() {
            var dragSource = null;

            function handleDragStart(e) {
                this.classList.add('source');
                var boxes = document.querySelectorAll('.box:not(.source)');
                [].forEach.call(boxes, function (box) {
                    box.classList.add('drag');
                });

                dragSource = this;

                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData('text/html', this.innerHTML);
            }

            function handleDragOver(e) {
                if (e.preventDefault) {
                    e.preventDefault();
                }

                e.dataTransfer.dropEffect = 'move';

                return false;
            }

            function handleDragEnter(e) {
                if (!this.nextElementSibling) {
                    this.classList.add('empty');
                }

                this.classList.add('over');
            }

            function handleDragLeave(e) {
                if (!this.nextElementSibling) {
                    this.classList.remove('empty');
                }

                this.classList.remove('over');
            }

            function handleDragEnd(e) {
                cleanBoxes();
            }

            function handleDrop(e) {
                if (e.stopPropagation) {
                    e.stopPropagation();
                }

                if (dragSource !== this) {
                    dragSource.innerHTML = this.innerHTML;

                    let data = e.dataTransfer.getData('text/html');
                    let isGapBox = this.classList.contains('gap');

                    if (isGapBox) {
                        if (this.previousElementSibling) {
                            let gapBox = createBox('box gap');
                            this.parentNode.insertBefore(gapBox, this);
                        }

                        let box = createBox('box');
                        box.innerHTML = data;
                        this.parentNode.insertBefore(box, this);

                        if (dragSource.nextElementSibling && dragSource.nextElementSibling.classList.contains('gap')) {
                            let item = dragSource.nextElementSibling;
                            if (item.nextElementSibling) {
                                dragSource.parentNode.removeChild(dragSource.nextElementSibling);
                            } else {
                                if (dragSource.previousElementSibling && dragSource.previousElementSibling.classList.contains('gap')) {
                                    dragSource.parentNode.removeChild(dragSource.previousElementSibling);
                                }
                            }
                        }

                        dragSource.parentNode.removeChild(dragSource);

                    } else {
                        this.innerHTML = data;
                    }

                    cleanBoxes();
                }

                return false;
            }

            function bindDnDEvents(box) {
                box.addEventListener('dragstart', handleDragStart, false);
                box.addEventListener('dragenter', handleDragEnter, false);
                box.addEventListener('dragover', handleDragOver, false);
                box.addEventListener('dragleave', handleDragLeave, false);
                box.addEventListener('dragend', handleDragEnd, false);
                box.addEventListener('drop', handleDrop, false);
            }

            function createBox(className) {
                let box = document.createElement('div');
                box.className = className;
                bindDnDEvents(box);

                return box;
            }

            function cleanBoxes() {
                var boxes = document.querySelectorAll('.box');
                [].forEach.call(boxes, function (box) {
                    box.classList.remove('over');
                    box.classList.remove('drag');
                    box.classList.remove('source');
                    box.classList.remove('empty');
                });
            }

            function init() {
                var boxes = document.querySelectorAll('.box');
                [].forEach.call(boxes, function (box) {
                    bindDnDEvents(box);
                });
            }

            init();
        }
    }
})();