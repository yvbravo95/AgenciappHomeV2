//Grafico de pastel
$(window).on("load", function () {

    // Set paths
    // ------------------------------

    require.config({
        paths: {
            echarts: '../../../app-assets/vendors/js/charts/echarts'
        }
    });


    // Configuration
    // ------------------------------

    require(
        [
            'echarts',
            'echarts/chart/pie',
            'echarts/chart/funnel'
        ],


        // Charts setup
        function (ec) {
            // Initialize chart
            // ------------------------------
            var myChart = ec.init(document.getElementById('basic-pie'));
            var myChartService = ec.init(document.getElementById('basic-pieServices'));

            $.get("/Home/getTransactionCombos", function (response) {
                if (response.success) {
                    // Chart Options
                    // ------------------------------
                    var colors = [];
                    var data = [];
                    for (var i = 0; i < response.data.values.length; i++) {
                        colors.push(generarNuevoColor());
                        data.push({
                            value: response.data.values[i], name: response.data.names[i]
                        })
                    }
                    chartOptions = {

                        // Add title
                        title: {
                            text: 'Cantidad Combos',
                            subtext: '',
                            x: 'center'
                        },

                        // Add tooltip
                        tooltip: {
                            trigger: 'item',
                            formatter: "{a} <br/>{b}: {c} ({d}%)"
                        },

                        // Add legend
                        legend: {
                            orient: 'vertical',
                            x: 'left',
                            data: response.data.names
                        },

                        // Add custom colors
                        color: colors,


                        // Display toolbox
                        toolbox: {
                            show: true,
                            orient: 'vertical',
                            feature: {
                                mark: {
                                    show: true,
                                    title: {
                                        mark: 'Markline switch',
                                        markUndo: 'Undo markline',
                                        markClear: 'Clear markline'
                                    }
                                },
                                dataView: {
                                    show: true,
                                    readOnly: false,
                                    title: 'View data',
                                    lang: ['View chart data', 'Close', 'Update']
                                },
                                magicType: {
                                    show: true,
                                    title: {
                                        pie: 'Switch to pies',
                                        funnel: 'Switch to funnel',
                                    },
                                    type: ['pie', 'funnel'],
                                    option: {
                                        funnel: {
                                            x: '25%',
                                            y: '20%',
                                            width: '50%',
                                            height: '70%',
                                            funnelAlign: 'left',
                                            max: 1548
                                        }
                                    }
                                },
                                restore: {
                                    show: true,
                                    title: 'Restore'
                                },
                                saveAsImage: {
                                    show: true,
                                    title: 'Same as image',
                                    lang: ['Save']
                                }
                            }
                        },

                        // Enable drag recalculate
                        calculable: true,

                        // Add series
                        series: [{
                            name: 'Browsers',
                            type: 'pie',
                            radius: '70%',
                            center: ['50%', '57.5%'],
                            data: data
                        }]
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
                }
            });
                
            $.get("/Home/getTransactionServicios", function (response) {
                if (response.success) {
                    // Chart Options
                    // ------------------------------
                    var colors = [];
                    var data = [];
                    for (var i = 0; i < response.data.values.length; i++) {
                        colors.push(generarNuevoColor());
                        data.push({
                            value: response.data.values[i], name: response.data.names[i]
                        })
                    }
                    chartOptions = {

                        // Add title
                        title: {
                            text: 'Cantidad Servicios',
                            subtext: '',
                            x: 'center'
                        },

                        // Add tooltip
                        tooltip: {
                            trigger: 'item',
                            formatter: "{a} <br/>{b}: {c} ({d}%)"
                        },

                        // Add legend
                        legend: {
                            orient: 'vertical',
                            x: 'left',
                            data: response.data.names
                        },

                        // Add custom colors
                        color: colors,


                        // Display toolbox
                        toolbox: {
                            show: true,
                            orient: 'vertical',
                            feature: {
                                mark: {
                                    show: true,
                                    title: {
                                        mark: 'Markline switch',
                                        markUndo: 'Undo markline',
                                        markClear: 'Clear markline'
                                    }
                                },
                                dataView: {
                                    show: true,
                                    readOnly: false,
                                    title: 'View data',
                                    lang: ['View chart data', 'Close', 'Update']
                                },
                                magicType: {
                                    show: true,
                                    title: {
                                        pie: 'Switch to pies',
                                        funnel: 'Switch to funnel',
                                    },
                                    type: ['pie', 'funnel'],
                                    option: {
                                        funnel: {
                                            x: '25%',
                                            y: '20%',
                                            width: '50%',
                                            height: '70%',
                                            funnelAlign: 'left',
                                            max: 1548
                                        }
                                    }
                                },
                                restore: {
                                    show: true,
                                    title: 'Restore'
                                },
                                saveAsImage: {
                                    show: true,
                                    title: 'Same as image',
                                    lang: ['Save']
                                }
                            }
                        },

                        // Enable drag recalculate
                        calculable: true,

                        // Add series
                        series: [{
                            name: 'Browsers',
                            type: 'pie',
                            radius: '70%',
                            center: ['50%', '57.5%'],
                            data: data
                        }]
                    };

                    // Apply options
                    // ------------------------------

                    myChartService.setOption(chartOptions);


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
                                myChartService.resize();
                            }, 200);
                        }
                    });
                }
            });
        }
    );
});

function generarNuevoColor() {
    var simbolos, color;
    simbolos = "0123456789ABCDEF";
    color = "#";

    for (var i = 0; i < 6; i++) {
        color = color + simbolos[Math.floor(Math.random() * 16)];
    }

    return color;
}