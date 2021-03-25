window.getDimensions = function () {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};

GetDivInformation = function (element) {
    return document.getElementById(element).getBoundingClientRect();
};