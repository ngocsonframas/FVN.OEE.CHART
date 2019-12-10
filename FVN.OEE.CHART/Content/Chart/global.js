var d = new Date();

var currDate = d.getDate("DD");
var currMonth = d.getMonth() + 1;
var currYear = d.getFullYear();

var dateNow = moment().format("dddd, MMMM, DD, YYYY");
function formatDate(date) {
  return date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + date.getDate()
}

function clearMainChart() {
  $('#main-chart').empty();
}

function renderHtml(container, degital, machine, downtime, colorOff, colorOn) {
  let html = '<div class="col-md-6 bg-degital-num">'
    + '<div class="row">'
    + '<div class="col-8" style="padding:0;font-weight:700;">'
    + '<div id="' + container + '" ></div>'
    + '</div>'
    + '<div class="col-4" style="padding:0;">'
    + '<h4 class="badge badge-primary" style="font-size:1.2rem;">' + machine + '%</h4>'
    + '<div class="exampleContainer">'
    + '<div id="' + degital + '" class="med-sevenSegArray5"></div>'
    + '</div>'
    + '</div>'
    //+ '<div class="downtime">'
    //+ '<h5 class="el-downtime">' + downtime + '</h4>'
    //+ '</div>'
    + '</div >'
    + '</div >';
  $('#main-chart').append(html);
  $("#" + degital).sevenSeg({
    digits: 3,
    value: 0,
    colorOff: colorOff,
    colorOn: colorOn,
  });
}
var Chart = function renderChart(container, degital, newVal, machine) {

  Highcharts.chart(container, {

    chart: {
      type: 'gauge',
      plotBackgroundColor: null,
      plotBackgroundImage: null,
      plotBorderWidth: 0,
      plotShadow: false
    },
    pane: {
      startAngle: -150,
      endAngle: 150,
      background: [{
        backgroundColor: {
          linearGradient: {
            x1: 0,
            y1: 0,
            x2: 0,
            y2: 1
          },
          stops: [
            [0, '#FFF'],
            [1, '#333']
          ]
        },
        borderWidth: 1,
        outerRadius: '109%'
      }, {
        backgroundColor: {
          linearGradient: {
            x1: 0,
            y1: 0,
            x2: 0,
            y2: 1
          },
          stops: [
            [0, '#333'],
            [1, '#FFF']
          ]
        },
        borderWidth: 1,
        outerRadius: '107%'
      }, {
        // default background
          backgroundColor: '#DDD',
      }, {
        backgroundColor: '#DDD',
        borderWidth: 0,
        outerRadius: '105%',
        innerRadius: '103%'
      }]
    },

    // the value axis
    yAxis: {
      min: 0,
      max: 100,

      minorTickInterval: 'auto',
      minorTickWidth: 1,
      minorTickLength: 10,
      minorTickPosition: 'inside',
      minorTickColor: '#000',

      tickPixelInterval: 30,
      tickWidth: 2,
      tickPosition: 'inside',
      tickLength: 10,
      tickColor: '#000',
      labels: {
        step: 2,
        rotation: 'auto'
      },
      title: {
        text: machine + '(%)'
      },
      plotBands: [{
        from: 0,
        to: 60,
        color: '#ED0000' // green
      }, {
        from: 60,
        to: 85,
        color: '#fff202' // yellow
      }, {
        from: 85,
        to: 100,
        color: '#55BF3B' // red
      }]
    },

    series: [{
      name: 'Availability',
      data: [newVal],
      tooltip: {
        valueSuffix: ' %'
      }
    }]

  },
    // Add some life
    function (chart) {
      if (!chart.renderer.forExport) {
        $(degital).sevenSeg({
          digits: 3,
          value: newVal
        });

      }
    });
}

function renderCompareChart(data) {
  // Create the chart
  Highcharts.chart('comparison', {
    chart: {
      type: 'column'
    },
    xAxis: {
      type: 'category'
    },
    yAxis: {
      title: {
        text: 'Comparison chart'
      }

    },
    legend: {
      enabled: false
    },
    plotOptions: {
      series: {
        borderWidth: 0,
        dataLabels: {
          enabled: true,
          format: '{point.y}%'
        }
      }
    },

    tooltip: {
      headerFormat: '<span style="font-size:11px">{series.name}</span><br>',
      pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y}%</b><br/>'
    },

    "series": [
      {
        "name": "Avaiability",
        "colorByPoint": true,
        "data": data
      }
    ]
  });
}

function getQuarter(d) {
  d = d || new Date();
  var m = Math.floor(d.getMonth() / 3) + 2;
  m -= m > 4 ? 4 : 0;
  var y = d.getFullYear() + (m == 1 ? 1 : 0);
  return [y, m];
}

function notifySuccess(message) {
  var e = { message: message };
  var t = $.notify(e, {
    type: "success",
    allow_dismiss: true,
    newest_on_top: true,
    timer: 2000,
    placement: { from: "top", align: "right" },
    offset: { x: "30", y: "30" },
    delay: 1000,
    z_index: 10000,
    animate: { enter: "animated bounce", exit: "animated bounce" }
  });
}

function notifyError(message) {
  var e = { message: message };
  var t = $.notify(e, {
    type: "danger",
    allow_dismiss: true,
    newest_on_top: true,
    timer: 2000,
    placement: { from: "top", align: "right" },
    offset: { x: "30", y: "30" },
    delay: 1000,
    z_index: 10000,
    animate: { enter: "animated swing", exit: "animated bounce" }
  });
}




