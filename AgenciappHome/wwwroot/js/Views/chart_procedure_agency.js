/*=========================================================================================
    File Name: basic-column.js
    Description: echarts column chart
    ----------------------------------------------------------------------------------------
    Item Name: Stack - Responsive Admin Theme
    Version: 1.0
    Author: PIXINVENT
    Author URL: http://www.themeforest.net/user/pixinvent
==========================================================================================*/

// Basic column chart
// ------------------------------

$(window).on("load", function () {

    // Set paths
    // ------------------------------

    require.config({
        paths: {
            echarts: '~/app-assets/vendors/js/charts/echarts'
        }
    });


    // Configuration
    // ------------------------------

    require(
        [
            'echarts',
            'echarts/chart/bar',
            'echarts/chart/line'
        ],


        // Charts setup
        function (ec) {

            // Initialize chart
            // ------------------------------
            var myChart = ec.init(document.getElementById('chartprocedure'));

            $.ajax({
                type: "POST",
                url: "/home/getchartprocedureagency/",
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                async: true,
                success: function (response) {
                    var datos = response;
                    var days = '[';
                    for (var i = 0; i < datos[0]["data"].length; i++) {
                        if (days == '[') {
                            days = days + '"' + i + '"';
                        }
                        else {
                            days = days + ',"' + i + '"';
                        }

                    }
                    days = days + "]";

                    var leyendData = new Array;
                    var leyendColor = new Array;
                    var seriesData = new Array;
                    for (var i = 0; i < datos.length; i++) {
                        const item = datos[i];
                        const randomColor = Math.floor(Math.random() * 16777215).toString(16);
                        leyendData.push(item.nombreServicio);
                        leyendColor.push("#" + randomColor);
                        seriesData.push({
                            name: item.nombreServicio,
                            type: 'line',
                            smooth: true,
                            itemStyle: { normal: { areaStyle: { type: 'default' } } },
                            data: datos[i]["data"]
                        });
                    }

                    // Chart Options
                    // ------------------------------
                    chartOptions = {

                        // Setup grid
                        grid: {
                            x: 40,
                            x2: 20,
                            y: 35,
                            y2: 25
                        },

                        // Add tooltip
                        tooltip: {
                            trigger: 'axis'
                        },

                        // Add legend
                        legend: {
                            data: leyendData
                        },

                        // Add custom colors
                        color: leyendColor,

                        // Enable drag recalculate
                        calculable: true,

                        // Horizontal axis
                        xAxis: [{
                            type: 'category',
                            boundaryGap: false,
                            data: JSON.parse(days)
                        }],

                        // Vertical axis
                        yAxis: [{
                            type: 'value'
                        }],

                        // Add series
                        series: seriesData
                    };

                    // Apply options
                    // ------------------------------

                    myChart.setOption(chartOptions);

                    // Resize chart
                    // ------------------------------

                    $(function () {

                        // Resize chart on menu width change and window resize
                        $(window).on('resize', resize);
                        $(".menu-toggle").on('click', resize);

                        // Resize function
                        function resize() {
                            setTimeout(function () {

                                // Resize chart
                                myChart.resize();
                            }, 200);
                        }
                    });
                },
                failure: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                },
                error: function (response) {
                    showErrorMessage("ERROR", response.responseText);
                }
            });
        }
    );
});