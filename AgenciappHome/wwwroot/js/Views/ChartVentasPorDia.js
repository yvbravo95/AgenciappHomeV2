/*=========================================================================================
    File Name: basic-area.js
    Description: echarts basic area chart
    ----------------------------------------------------------------------------------------
    Item Name: Stack - Responsive Admin Theme
    Version: 1.0
    Author: PIXINVENT
    Author URL: http://www.themeforest.net/user/pixinvent
==========================================================================================*/

// Basic Area chart
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
            var myChart = ec.init(document.getElementById('ventasPorDia'));

            $.ajax({
                type: "POST",
                url: "/Analytics/getDataVentasDia/",
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                async: true,
                success: function (response) {
                    var datos = JSON.parse(response);
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
                            data: ['Reservas', 'Envios', 'Recargas', 'Remesas','Envios Maritimos']
                        },

                        // Add custom colors
                        color: ['#FF7588', '#40C7CA', '#FFA87D', '#18e000','#d200e0'],

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
                        series: [
                            {
                                name: 'Reservas',
                                type: 'line',
                                smooth: true,
                                itemStyle: { normal: { areaStyle: { type: 'default' } } },
                                data: datos[3]["data"]
                            },
                            {
                                name: 'Envios',
                                type: 'line',
                                smooth: true,
                                itemStyle: { normal: { areaStyle: { type: 'default' } } },
                                data: datos[0]["data"]
                            },
                            {
                                name: 'Recargas',
                                type: 'line',
                                smooth: true,
                                itemStyle: { normal: { areaStyle: { type: 'default' } } },
                                data: datos[1]["data"]
                            },
                            {
                                name: 'Remesas',
                                type: 'line',
                                smooth: true,
                                itemStyle: { normal: { areaStyle: { type: 'default' } } },
                                data: datos[2]["data"]
                            },
                            {
                                name: 'Envios Maritimos',
                                type: 'line',
                                smooth: true,
                                itemStyle: { normal: { areaStyle: { type: 'default' } } },
                                data: datos[4]["data"]
                            }
                        ]
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