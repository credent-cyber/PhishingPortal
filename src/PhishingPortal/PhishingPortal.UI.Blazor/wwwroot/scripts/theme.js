
function setLayout(layoutTargetValue, headerColor) {
    //for Layout
    var layoutLinks = document.querySelectorAll('.theme-color.layout-type a.active').forEach(function (link) {
        link.classList.remove('active');
    });

    // Add the "active" class to the specific <a> element based on data-value
    var layoutTargetLink = document.querySelector('.theme-color.layout-type a[data-value="' + layoutTargetValue + '"]');
    if (layoutTargetLink) {
        layoutTargetLink.classList.add('active');
    }

    //for header
    document.querySelectorAll('.theme-color.header-color a.active').forEach(function (element) {
        element.classList.remove('active');
    });


    // Add the "active" class to the specific <a> element based on data-value
    var targetLink = document.querySelector('.theme-color a[data-value="' + headerColor + '"]');
    if (targetLink) {
        targetLink.classList.add('active');
    }
}