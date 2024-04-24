window.initializeEditor = async function (design) {
    var editor = unlayer.init({
        id: 'editor-container',
        projectId: 1234,
        displayMode: 'email'
    });

    unlayer.addEventListener('editor:ready', function () {
        //window.DotNet.invokeMethodAsync('PhishingPortal.UI.Blazor', 'EditorReady', design);
        var originalData = JSON.parse(design);
        loadDesign(originalData);
    });
}


window.exportHtml = async function () {
    return new Promise((resolve, reject) => {
        unlayer.exportHtml(function (data) {
            var html = data.html;
            resolve(html);
        }, {
            cleanup: true
        });
    });
}
window.exportDesign = async function () {
    return new Promise((resolve, reject) => {
        unlayer.exportHtml(function (data) {
            var json = JSON.stringify(data.design); // Convert the design object to a JSON string
            resolve(json);
        }, {
            cleanup: true
        });
    });
}
window.exportPlainText = async function () {
    return new Promise((resolve, reject) => {
        unlayer.exportHtml(function (data) {
            var text = data.text; // final text
            resolve(text);
        }, {
            cleanup: true
        });
    });
}
window.loadDesign = async function (design) {
    unlayer.loadDesign(design);
}