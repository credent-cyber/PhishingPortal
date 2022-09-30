/// <reference path="../chart.js" />
/// <reference path="../chart.js" />


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
        dotnetInstance.invokeMethodAsync('ChartClick', rtn, id);
    };

    chart.options.onHover = function () {
        DotNet.invokeMethodAsync('PhishingPortal.UI.Blazor', 'ChartHover');
    };

}

