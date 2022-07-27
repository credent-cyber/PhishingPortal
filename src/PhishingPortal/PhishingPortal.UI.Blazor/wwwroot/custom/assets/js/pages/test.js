'use strict';
$(document).ready(function () {
    var bar = document.getElementbyId("chart-bar-1").getcontext('2d');
    var data1 = {
        labels: ["campaign 1", "campaign 2", "campaign 3", "campaign 4"],
        datasets: [{
            label: "no of emails",
            data: [50, 45, 92, 72],
            bordercolor: '#0e9e4a',
            backgroundcolor: '#0e9e4a',
            hoverbordercolor: '#0e9e4a',
            hoverbackgroundcolor: '#0e9e4a',
        }, {
            label: "no of clicks",
            data: [50, 32, 35, 40],
            bordercolor: '#4099ff',
            backgroundcolor: '#4099ff',
            hoverbordercolor: '#4099ff',
            hoverbackgroundcolor: '#4099ff',
        }]
    };
    var mybarchart = new chart(bar, {
        type: 'bar',
        data: data1,
        options: {
            barvaluespacing: 20
        }
    });

//    var bar = document.getelementbyid("chart-bar-a").getcontext('2d');
//    var data1 = {
//        labels: ["campaign 11", "campaign 2", "campaign 3", "campaign 4"],
//        datasets: [{
//            label: "no of sms",
//            data: [59, 43, 79, 63],
//            bordercolor: '#ffb64d',
//            backgroundcolor: '#ffb64d',
//            hoverbordercolor: '#ffb64d',
//            hoverbackgroundcolor: '#ffb64d',
//        }, {
//            label: "no of clicks",
//            data: [23, 32, 28, 38],
//            bordercolor: '#4099ff',
//            backgroundcolor: '#4099ff',
//            hoverbordercolor: '#4099ff',
//            hoverbackgroundcolor: '#4099ff',
//        }]
//    };
//    var mybarchart = new chart(bar, {
//        type: 'bar',
//        data: data1,
//        options: {
//            barvaluespacing: 20
//        }
//    });

//    var bar = document.getelementbyid("chart-bar-2").getcontext('2d');
//    var data1 = {
//        labels: ["campaign 1", "campaign 2", "campaign 3", "campaign 4"],
//        datasets: [{
//            label: "no of clicks",
//            data: [25, 45, 74, 85],
//            bordercolor: '#4099ff',
//            backgroundcolor: '#4099ff',
//            hoverbordercolor: '#4099ff',
//            hoverbackgroundcolor: '#4099ff',
//        }, {
//            label: "no of sms",
//            data: [30, 52, 65, 65],
//            bordercolor: '#ffb64d',
//            backgroundcolor: '#ffb64d',
//            hoverbordercolor: '#ffb64d',
//            hoverbackgroundcolor: '#ffb64d',
//        }]
//    };
//    var mybarchart = new chart(bar, {
//        type: 'bar',
//        data: data1,
//        options: {
//            barvaluespacing: 20,
//            scales: {
//                xaxes: [{
//                    stacked: true,
//                }],
//                yaxes: [{
//                    stacked: true
//                }]
//            }
//        },
//    });

//    var bar = document.getelementbyid("chart-bar-3").getcontext('2d');
//    var theme_g1 = bar.createlineargradient(0, 300, 0, 0);
//    var data1 = {
//        labels: [0, 1, 2, 3],
//        datasets: [{
//            label: "data 1",
//            data: [25, 45, 74, 85],
//            bordercolor: '#4099ff',
//            backgroundcolor: '#4099ff',
//            hoverbordercolor: '#4099ff',
//            hoverbackgroundcolor: '#4099ff',
//        }, {
//            label: "data 2",
//            data: [30, 52, 65, 65],
//            bordercolor: '#ff5370',
//            backgroundcolor: '#ff5370',
//            hoverbordercolor: '#ff5370',
//            hoverbackgroundcolor: '#ff5370',
//        }]
//    };
//    var mybarchart = new chart(bar, {
//        type: 'horizontalbar',
//        data: data1,
//        options: {
//            barvaluespacing: 20
//        }
//    });
//    var bar = document.getelementbyid("chart-line-1").getcontext('2d');
//    var data1 = {
//        labels: [0, 1, 2, 3, 4, 5, 6],
//        datasets: [{
//            label: "d1",
//            data: [55, 70, 62, 81, 56, 70, 90],
//            fill: false,
//            borderwidth: 4,
//            linetension: 0,
//            borderdash: [15, 10],
//            bordercolor: '#0e9e4a',
//            backgroundcolor: '#0e9e4a',
//            hoverbordercolor: '#0e9e4a',
//            hoverbackgroundcolor: '#0e9e4a',
//        }, {
//            label: "d2",
//            data: [85, 55, 70, 50, 75, 45, 60],
//            fill: true,
//            cubicinterpolationmode: 'monotone',
//            borderwidth: 0,
//            bordercolor: '#4099ff',
//            backgroundcolor: '#4099ff',
//            hoverbordercolor: '#4099ff',
//            hoverbackgroundcolor: '#4099ff',
//        }, {
//            label: "d3",
//            data: [50, 75, 80, 70, 85, 80, 70],
//            fill: false,
//            borderwidth: 4,
//            bordercolor: '#2ed8b6',
//            backgroundcolor: '#2ed8b6',
//            hoverbordercolor: '#2ed8b6',
//            hoverbackgroundcolor: '#2ed8b6',
//        }]
//    };
//    var mybarchart = new chart(bar, {
//        type: 'line',
//        data: data1,
//        responsive: true,
//        options: {
//            barvaluespacing: 20,
//            maintainaspectratio: false,
//        }
//    });
//    var bar = document.getelementbyid("chart-area-2").getcontext('2d');
//    var data1 = {
//        labels: [0, 1, 2, 3, 4, 5, 6],
//        datasets: [{
//            label: "d1",
//            data: [85, 55, 70, 50, 75, 45, 60],
//            borderwidth: 1,
//            bordercolor: '#00bcd4',
//            backgroundcolor: '#00bcd4',
//            hoverbordercolor: '#00bcd4',
//            hoverbackgroundcolor: '#00bcd4',
//            fill: 'origin',
//        }]
//    };
//    var mybarchart = new chart(bar, {
//        type: 'line',
//        data: data1,
//        responsive: true,
//        options: {
//            barvaluespacing: 20,
//            maintainaspectratio: false,
//        }
//    });
//    var bar = document.getelementbyid("chart-area-3").getcontext('2d');
//    var data1 = {
//        labels: [0, 1, 2, 3, 4, 5, 6],
//        datasets: [{
//            label: "d1",
//            data: [85, 55, 70, 50, 75, 45, 60],
//            borderwidth: 1,
//            bordercolor: '#2ed8b6',
//            backgroundcolor: '#2ed8b6',
//            hoverbordercolor: '#2ed8b6',
//            hoverbackgroundcolor: '#2ed8b6',
//            fill: 'end',
//        }]
//    };
//    var mybarchart = new chart(bar, {
//        type: 'line',
//        data: data1,
//        responsive: true,
//        options: {
//            barvaluespacing: 20,
//            maintainaspectratio: false,
//        }
//    });
//    var bar = document.getelementbyid("chart-area-1").getcontext('2d');
//    var data1 = {
//        labels: [0, 1, 2, 3, 4, 5, 6],
//        datasets: [{
//            label: "d1",
//            data: [45, 60, 45, 80, 60, 80, 45],
//            fill: true,
//            borderwidth: 4,
//            bordercolor: '#4099ff',
//            backgroundcolor: '#4099ff',
//            hoverbordercolor: '#4099ff',
//            hoverbackgroundcolor: '#4099ff',
//        }, {
//            label: "d2",
//            data: [45, 80, 45, 45, 60, 45, 80],
//            fill: true,
//            cubicinterpolationmode: 'monotone',
//            borderwidth: 0,
//            bordercolor: '#0e9e4a',
//            backgroundcolor: '#0e9e4a',
//            hoverbordercolor: '#0e9e4a',
//            hoverbackgroundcolor: '#0e9e4a',
//        }, {
//            label: "d3",
//            data: [83, 45, 60, 45, 45, 55, 45],
//            fill: true,
//            borderwidth: 4,
//            bordercolor: '#2ed8b6',
//            backgroundcolor: '#2ed8b6',
//            hoverbordercolor: '#2ed8b6',
//            hoverbackgroundcolor: '#2ed8b6',
//        }]
//    };
//    var mybarchart = new chart(bar, {
//        type: 'line',
//        data: data1,
//        responsive: true,
//        options: {
//            barvaluespacing: 20,
//            maintainaspectratio: false,
//        }
//    });
//    var bar = document.getelementbyid("chart-radar-1").getcontext('2d');
//    var data1 = {
//        labels: [0, 1, 2, 3, 4, 5, 6],
//        datasets: [{
//            label: "d1",
//            data: [60, 60, 45, 80, 60, 80, 45],
//            fill: true,
//            borderwidth: 4,
//            bordercolor: '#4099ff',
//            backgroundcolor: '#4099ff',
//            hoverbordercolor: '#4099ff',
//            hoverbackgroundcolor: '#4099ff',
//        }, {
//            label: "d2",
//            data: [40, 80, 40, 65, 60, 50, 95],
//            fill: true,
//            cubicinterpolationmode: 'monotone',
//            borderwidth: 0,
//            bordercolor: '#0e9e4a',
//            backgroundcolor: '#0e9e4a',
//            hoverbordercolor: '#0e9e4a',
//            hoverbackgroundcolor: '#0e9e4a',
//        }, {
//            label: "d3",
//            data: [20, 40, 80, 60, 85, 60, 20],
//            fill: true,
//            borderwidth: 4,
//            bordercolor: '#2ed8b6',
//            backgroundcolor: '#2ed8b6',
//            hoverbordercolor: '#2ed8b6',
//            hoverbackgroundcolor: '#2ed8b6',
//        }]
//    };
//    var mybarchart = new chart(bar, {
//        type: 'radar',
//        data: data1,
//        responsive: true,
//        options: {
//            barvaluespacing: 20,
//            maintainaspectratio: false,
//        }
//    });
//    var bar = document.getelementbyid("chart-radar-2").getcontext('2d');
//    var data1 = {
//        labels: [0, 1, 2, 3, 4, 5, 6],
//        datasets: [{
//            label: "d1",
//            data: [60, 60, 45, 80, 60, 80, 45],
//            fill: true,
//            borderwidth: 4,
//            bordercolor: '#2ed8b6',
//            backgroundcolor: '#2ed8b6',
//            hoverbordercolor: '#2ed8b6',
//            hoverbackgroundcolor: '#2ed8b6',
//        }, {
//            label: "d2",
//            data: [40, 80, 40, 65, 60, 50, 95],
//            fill: true,
//            cubicinterpolationmode: 'monotone',
//            borderwidth: 0,
//            bordercolor: '#ff5370',
//            backgroundcolor: '#ff5370',
//            hoverbordercolor: '#ff5370',
//            hoverbackgroundcolor: '#ff5370',
//        }, {
//            label: "d3",
//            data: [20, 40, 80, 60, 85, 60, 20],
//            fill: true,
//            borderwidth: 4,
//            bordercolor: '#ffb64d',
//            backgroundcolor: '#ffb64d',
//            hoverbordercolor: '#ffb64d',
//            hoverbackgroundcolor: '#ffb64d',
//        }]
//    };
//    var barchart = new chart(bar, {
//        type: 'radar',
//        data: data1,
//        responsive: true,
//        options: {
//            barvaluespacing: 20,
//            maintainaspectratio: false,
//        }
//    });
//    var bar = document.getelementbyid("chart-radar-3").getcontext('2d');
//    var data1 = {
//        labels: [0, 1, 2, 3, 4, 5, 6],
//        datasets: [{
//            label: "d1",
//            data: [60, 60, 45, 80, 60, 80, 45],
//            fill: true,
//            borderwidth: 4,
//            bordercolor: '#4099ff',
//            backgroundcolor: "transparent",
//            hoverbordercolor: '#4099ff',
//            hoverbackgroundcolor: "transparent",
//        }, {
//            label: "d2",
//            data: [40, 80, 40, 65, 60, 50, 95],
//            fill: true,
//            cubicinterpolationmode: 'monotone',
//            borderwidth: 0,
//            bordercolor: '#0e9e4a',
//            backgroundcolor: "transparent",
//            hoverbordercolor: '#0e9e4a',
//            hoverbackgroundcolor: "transparent",
//        }, {
//            label: "d3",
//            data: [20, 40, 80, 60, 85, 60, 20],
//            fill: true,
//            borderwidth: 4,
//            bordercolor: '#2ed8b6',
//            backgroundcolor: "transparent",
//            hoverbordercolor: '#2ed8b6',
//            hoverbackgroundcolor: "transparent",
//        }]
//    };
//    var barchart = new chart(bar, {
//        type: 'radar',
//        data: data1,
//        responsive: true,
//        options: {
//            barvaluespacing: 20,
//            maintainaspectratio: false,
//        }
//    });
//    var bar = document.getelementbyid("chart-pie-1").getcontext('2d');
//    var data4 = {
//        labels: ["data 1", "data 2", "data 3"],
//        datasets: [{
//            data: [30, 30, 40],
//            backgroundcolor: ['#2ed8b6', '#0e9e4a', '#4099ff'],
//            hoverbackgroundcolor: ['#2ed8b6', '#0e9e4a', '#4099ff']
//        }]
//    };
//    var mypiechart = new chart(bar, {
//        type: 'pie',
//        data: data4,
//        responsive: true,
//        options: {
//            maintainaspectratio: false,
//        }
//    });
//    var bar = document.getelementbyid("chart-donut-1").getcontext('2d');
//    var data4 = {
//        labels: ["data 1", "data 2", "data 3"],
//        datasets: [{
//            data: [30, 30, 40],
//            backgroundcolor: ['#2ed8b6', '#ffb64d', '#ff5370'],
//            hoverbackgroundcolor: ['#2ed8b6', '#ffb64d', '#ff5370']
//        }]
//    };
//    var mypiechart = new chart(bar, {
//        type: 'doughnut',
//        data: data4,
//        responsive: true,
//        options: {
//            maintainaspectratio: false,
//        }
//    });
});