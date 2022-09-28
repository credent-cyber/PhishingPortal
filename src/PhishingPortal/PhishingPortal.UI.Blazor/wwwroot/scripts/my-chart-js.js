/// <reference path="../chart.js" />
/// <reference path="../chart.js" />
//window.setup = (id, config) => {
//    var ctx = document.getElementById(id).getContext('2d');
//    new Chart(ctx, config);
//}

window.setup = (id, config, obj) => {



    var ctx = document.getElementById(id).getContext('2d');
    var chart = new Chart(ctx, config);  
    var dotnetInstance = obj;
    chart.options.onClick = function(event, array ) {
        var rtn;

        if (array !== undefined && array.length > 0)

           // console.log(config.data.labels[array[0].index]);
           // console.log(array[0].index);
           // console.log(config);
           // rtn = id;
            rtn = config.data.labels[array[0].index];
        //DotNet.invokeMethodAsync('PhishingPortal.UI.Blazor', 'ChartClick', rtn, id);
        dotnetInstance.invokeMethodAsync('ChartClick', rtn, id);
    };

    chart.options.onHover = function () {
        DotNet.invokeMethodAsync('PhishingPortal.UI.Blazor', 'ChartHover');
    };

}

//function clickHandler(evt) {
//    const points = myChart.getElementsAtEventForMode(evt, 'nearest', { intersect: true }, true);

//    if (points.length) {
//        const firstPoint = points[0];
//        const label = bar.data.labels[firstPoint.index];
//        const value = bar.data.datasets[firstPoint.datasetIndex].data[firstPoint.index];
//        console.log(value);
//        alert(value);

//    }
//}

