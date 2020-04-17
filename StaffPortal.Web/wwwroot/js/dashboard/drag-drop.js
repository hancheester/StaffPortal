(function () {
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
})();