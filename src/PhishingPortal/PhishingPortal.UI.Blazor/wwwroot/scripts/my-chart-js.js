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

    chart.options.onClick = function (event, array) {
        var rtn, lbl;

        if (array !== undefined && array.length > 0) {

            if (config.data.ids == undefined) {
                rtn = config.data.labels[array[0].index];
                dotnetInstance.invokeMethodAsync('ChartClick', rtn, id);
            }
            else {
                rtn = config.data.ids[array[0].index];
                var datasetIndex = array[0].datasetIndex;
                var dataIndex = array[0].index;
                var label = config.data.labels[dataIndex];
                var datasetLabel = config.data.datasets[datasetIndex].label;
                lbl = datasetLabel;
                dotnetInstance.invokeMethodAsync('ChartClick', rtn, lbl, id);
            }


        }

    };

    chart.options.onHover = function () {
        DotNet.invokeMethodAsync('PhishingPortal.UI.Blazor', 'ChartHover');
    };

}


/***********************************/
//window.mychart = {};

//window.chartClickHandler = (evt, x, y, z) => {
//    let chart = evt.chart;
//    const points = chart.getElementsAtEventForMode(evt, 'nearest', { intersect: true }, true);
//    console.log('chart click detected');
//    console.log(points);
//    if (points.length) {
//        const firstPoint = points[0];
//        var label = chart.data.labels[firstPoint.index];
//        var Ids = chart;
//        var value = chart.data.datasets[firstPoint.datasetIndex].data[firstPoint.index];
//        console.log('Point Clicked : (Id/label/value)'+ Ids + label + "/" + value);
//        //alert("You will be redirected to this link: " + chart.data.datasets[firstPoint.datasetIndex].urls[firstPoint.index])
//       // window.location.href = "https://phishsims.com/report/"+Ids;
//    }
//}

//window.setup = (id, config) => {

//    var ctx = document.getElementById(id).getContext('2d');
//    config.options.onClick = chartClickHandler;
//    new Chart(ctx, config);

//}