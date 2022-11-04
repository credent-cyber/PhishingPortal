/// <reference path="../chart.js" />
/// <reference path="../chart.js" />

let charts = [];

window.destroyChart = (id) => {
    charts.forEach((ele, index) => {
        if (ele.id == id) {
            ele.instance.destroy();
        }
    });
}

window.setup = (id, config, obj) => {

    var ctx = document.getElementById(id).getContext('2d');

    destroyChart(id);

    var chart = new Chart(ctx, config);

    charts.push({ id: id, instance: chart });

    var dotnetInstance = obj;
    chart.options.onClick = function(event, array ) {
        var rtn;

        if (array !== undefined && array.length > 0)

            rtn = config.data.labels[array[0].index];
        dotnetInstance.invokeMethodAsync('ChartClick', rtn, id);
    };

    chart.options.onHover = function () {
        DotNet.invokeMethodAsync('PhishingPortal.UI.Blazor', 'ChartHover');
    };

}
