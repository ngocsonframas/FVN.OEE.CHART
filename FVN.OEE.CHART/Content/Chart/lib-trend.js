function renderTrend(data) {
  Highcharts.chart('trend', {
    chart: {
      type: 'spline'
    },

    accessibility: {
      description: 'Show chart OEE.'
    },

    legend: {
      symbolWidth: 10
    },

    title: {
      text: 'Trend of Histories'
    },

    yAxis: {
      title: {
        text: 'Trend of Factories'
      }
    },

    xAxis: {
      title: {
        text: 'Time'
      },
      accessibility: {
        description: 'Trend of Factories'
      },
      categories: data.Time
    },

    tooltip: {
      split: true
    },

    plotOptions: {
      series: {
        point: {
          
        },
        cursor: 'pointer'
      }
    },

    series: [
      {
        name: 'Availability',
        data: 86.4
      }, {
        name: 'Performance',
        data: 92.1,
        dashStyle: 'ShortDashDot',
        color: Highcharts.getOptions().colors[4]
      }, {
        name: 'Quality',
        data: 97.1,
        dashStyle: 'ShortDot',
        color: Highcharts.getOptions().colors[7]
      },
    ],

    responsive: {
      rules: [{
        condition: {
          maxWidth: 500
        },
        chartOptions: {
          legend: {
            itemWidth: 250
          }
        }
      }]
    }
  });
}


