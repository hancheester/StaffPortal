// Write your JavaScript code.
(function () {

    $('li.sub-dropdown').hover(function () {
        var li = $(this);
        li.find('.dropdown-menu').stop(true, true).fadeIn(300);
        
        setTimeout(function () {
            li.addClass('minimize');
        }, 150)
        
    }, function () {
        var li = $(this);

        li.find('.dropdown-menu').stop(true, true).fadeOut(100);        

        setTimeout(function () {
            li.removeClass('minimize');
        }, 150)
    });
})();