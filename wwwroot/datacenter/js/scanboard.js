/*by suzong 202009*/
$(function () {
    //页面淡入效果
    $(".animsition").animsition({
        inClass: 'fade-in',
        outClass: 'fade-out',
        inDuration: 300,
        outDuration: 1000,
        // e.g. linkElement   :   'a:not([target="_blank"]):not([href^=#])'
        loading: false,
        loadingParentElement: 'body', //animsition wrapper element
        loadingClass: 'animsition-loading',
        unSupportCss: ['animation-duration',
            '-webkit-animation-duration',
            '-o-animation-duration'
        ],
        //"unSupportCss" option allows you to disable the "animsition" in case the css property in the array is not supported by your browser.
        //The default setting is to disable the "animsition" in a browser that does not support "animation-duration".
        overlay: false,
        overlayClass: 'animsition-overlay-slide',
        overlayParentElement: 'body'
    });
    document.onreadystatechange = subSomething;
    function subSomething() {
        if (document.readyState == "complete") {
            $('#loader').hide();
        }
    }

    //顶部时间
    function getTime() {
        var myDate = new Date();
        var myYear = myDate.getFullYear(); //获取完整的年份(4位,1970-????)
        var myMonth = myDate.getMonth() + 1; //获取当前月份(0-11,0代表1月)
        var myToday = myDate.getDate(); //获取当前日(1-31)
        var myDay = myDate.getDay(); //获取当前星期X(0-6,0代表星期天)
        var myHour = myDate.getHours(); //获取当前小时数(0-23)
        var myMinute = myDate.getMinutes(); //获取当前分钟数(0-59)
        var mySecond = myDate.getSeconds(); //获取当前秒数(0-59)
        var week = ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'];
        var nowTime;

        nowTime = myYear + '-' + fillZero(myMonth) + '-' + fillZero(myToday) + '&nbsp;&nbsp;' + week[myDay] + '&nbsp;&nbsp;' + fillZero(myHour) + ':' + fillZero(myMinute) + ':' + fillZero(mySecond);
        $('.topTime').html(nowTime);
    };
    function fillZero(str) {
        var realNum;
        if (str < 10) {
            realNum = '0' + str;
        } else {
            realNum = str;
        }
        return realNum;
    }
    setInterval(getTime, 1000);

    //地图上的搜索
    $('.searchBtn').on('click', function () {
        $(this).hide();
        $('.searchInner').addClass('open');
        setTimeout(function () {
            $('.searchInner').find('form').show();
        }, 400);
    });
    $('.search').on('click', function (event) {
        event.stopPropagation();
    });
    $('body').on('click', function () {
        $('.searchInner').find('form').hide();
        $('.searchInner').removeClass('open');
        setTimeout(function () {
            $('.searchBtn').show();
        }, 400);
    });
    $("#subSearch").click(function () {
        var text = $("#subText").val().trim();
        if (text == "") return;
        SetShipList(text);
    })


    function totalNum(obj, speed) {
        var singalNum = 0;
        var timer;
        var totalNum = obj.attr('total');

        if (totalNum) {
            timer = setInterval(function () {
                singalNum += speed;
                if (singalNum >= totalNum) {
                    singalNum = totalNum;
                    clearInterval(timer);
                }
                obj.html(singalNum);
            }, 1);
        }
    }
    setTimeout(function () {
        $('.progress').each(function (i, ele) {
            var PG = $(ele).attr('progress');
            var PGNum = parseInt(PG);
            var zero = 0;
            var speed = 50;
            var timer;

            $(ele).find('h4').html(zero + '%');
            if (PGNum < 10) {
                $(ele).find('.progressBar span').addClass('bg-red');
                $(ele).find('h3 i').addClass('color-red');
            } else if (PGNum >= 10 && PGNum < 50) {
                $(ele).find('.progressBar span').addClass('bg-yellow');
                $(ele).find('h3 i').addClass('color-yellow');
            } else if (PGNum >= 50 && PGNum < 100) {
                $(ele).find('.progressBar span').addClass('bg-blue');
                $(ele).find('h3 i').addClass('color-blue');
            } else {
                $(ele).find('.progressBar span').addClass('bg-green');
                $(ele).find('h3 i').addClass('color-green');
            }
            $(ele).find('.progressBar span').animate({ width: PG }, PGNum * speed);
            timer = setInterval(function () {
                zero++;
                $(ele).find('h4').html(zero + '%');
                if (zero == PGNum) {
                    clearInterval(timer);
                }
            }, speed);
        });

        //总计运单数
        totalNum($('#totalNum'), 1000);
        //基本信息
        totalNum($('#indicator1'), 1);
        totalNum($('#indicator2'), 1);
        totalNum($('#indicator3'), 1);

    }, 500);

    var summaryPie1, summaryPie2, summaryPie3, summaryBar, summaryLine;
    var pieData;
    function setSummary() {
        summaryPie1 = echarts.init(document.getElementById('summaryPie1'));
        summaryPie2 = echarts.init(document.getElementById('summaryPie2'));
        summaryPie3 = echarts.init(document.getElementById('summaryPie3'));

        var ww = $(window).width();
        var pieData;
        if (ww > 1600) {
            pieData = {
                pieTop: '40%',
                pieTop2: '36%',
                titleSize: 20,
                pieRadius: [80, 85],
                itemSize: 32
            }
        } else {
            pieData = {
                pieTop: '30%',
                pieTop2: '26%',
                titleSize: 16,
                pieRadius: [60, 64],
                itemSize: 28
            }
        };
        //弹出框调用ECharts饼图
        var pieOption1 = {
            title: {
                x: 'center',
                y: pieData.pieTop,
                text: '司机',
                textStyle: {
                    fontWeight: 'normal',
                    color: '#ffd325',
                    fontSize: pieData.titleSize,
                },
                subtext: '总数：100人\n今日工作：25人',
                subtextStyle: {
                    color: '#fff',
                }
            },
            tooltip: {
                show: false,
            },
            toolbox: {
                show: false,
            },

            series: [{
                type: 'pie',
                clockWise: false,
                radius: pieData.pieRadius,
                hoverAnimation: false,
                center: ['50%', '50%'],
                data: [{
                    value: 25,
                    label: {
                        normal: {
                            formatter: '{d}%',
                            position: 'outside',
                            show: true,
                            textStyle: {
                                fontSize: pieData.itemSize,
                                fontWeight: 'normal',
                                color: '#ffd325'
                            }
                        }
                    },
                    itemStyle: {
                        normal: {
                            color: '#ffd325',
                            shadowColor: '#ffd325',
                            shadowBlur: 10
                        }
                    }
                }, {
                    value: 75,
                    name: '未工作',
                    itemStyle: {
                        normal: {
                            color: 'rgba(44,59,70,1)', // 未完成的圆环的颜色
                            label: {
                                show: false
                            },
                            labelLine: {
                                show: false
                            }
                        },
                        emphasis: {
                            color: 'rgba(44,59,70,1)' // 未完成的圆环的颜色
                        }
                    },
                    itemStyle: {
                        normal: {
                            color: '#11284e',
                            shadowColor: '#11284e',
                        }
                    },
                }]
            }]
        }
        var pieOption2 = {
            title: {
                x: 'center',
                y: pieData.pieTop,
                text: '车辆',
                textStyle: {
                    fontWeight: 'normal',
                    color: '#32ffc7',
                    fontSize: pieData.titleSize
                },
                subtext: '总数：100辆\n今日工作：75辆人',
                subtextStyle: {
                    color: '#fff',
                }
            },
            tooltip: {
                show: false,
            },
            toolbox: {
                show: false,
            },

            series: [{
                type: 'pie',
                clockWise: false,
                radius: pieData.pieRadius,
                hoverAnimation: false,
                center: ['50%', '50%'],
                data: [{
                    value: 75,
                    label: {
                        normal: {
                            formatter: '{d}%',
                            position: 'outside',
                            show: true,
                            textStyle: {
                                fontSize: pieData.itemSize,
                                fontWeight: 'normal',
                                color: '#32ffc7'
                            }
                        }
                    },
                    itemStyle: {
                        normal: {
                            color: '#32ffc7',
                            shadowColor: '#32ffc7',
                            shadowBlur: 10
                        }
                    }
                }, {
                    value: 25,
                    name: '未工作',
                    itemStyle: {
                        normal: {
                            color: 'rgba(44,59,70,1)', // 未完成的圆环的颜色
                            label: {
                                show: false
                            },
                            labelLine: {
                                show: false
                            }
                        },
                        emphasis: {
                            color: 'rgba(44,59,70,1)' // 未完成的圆环的颜色
                        }
                    },
                    itemStyle: {
                        normal: {
                            color: '#11284e',
                            shadowColor: '#11284e',
                        }
                    },
                }]
            }]
        }
        var pieOption3 = {
            title: {
                x: 'center',
                y: pieData.pieTop2,
                text: '运单',
                textStyle: {
                    fontWeight: 'normal',
                    color: '#1eb6fe',
                    fontSize: pieData.titleSize
                },
                subtext: '总数：100单\n正常单：50单\n异常单：50单',
                subtextStyle: {
                    color: '#fff',
                }
            },
            tooltip: {
                show: false,
            },
            toolbox: {
                show: false,
            },

            series: [{
                type: 'pie',
                clockWise: false,
                radius: pieData.pieRadius,
                hoverAnimation: false,
                center: ['50%', '50%'],
                data: [{
                    value: 50,
                    label: {
                        normal: {
                            formatter: '{d}%',
                            position: 'outside',
                            show: true,
                            textStyle: {
                                fontSize: pieData.itemSize,
                                fontWeight: 'normal',
                                color: '#1eb6fe'
                            }
                        }
                    },
                    itemStyle: {
                        normal: {
                            color: '#1eb6fe',
                            shadowColor: '#1eb6fe',
                            shadowBlur: 10
                        }
                    }
                }, {
                    value: 50,
                    name: '未工作',
                    itemStyle: {
                        normal: {
                            color: 'rgba(44,59,70,1)', // 未完成的圆环的颜色
                            label: {
                                show: false
                            },
                            labelLine: {
                                show: false
                            }
                        },
                        emphasis: {
                            color: 'rgba(44,59,70,1)' // 未完成的圆环的颜色
                        }
                    },
                    itemStyle: {
                        normal: {
                            color: '#11284e',
                            shadowColor: '#11284e',
                        }
                    },
                }]
            }]
        }

        //弹出框调用ECharts柱状图
        summaryBar = echarts.init(document.getElementById('summaryBar'));
        var barOption = {

            tooltip: {
                trigger: 'item',
                formatter: function (params) {
                    var res = '本月' + params.name + '号运单数：' + params.data;
                    return res;
                }
            },
            grid: {
                top: '20%',
                left: '15%',
                width: '80%',
                height: '80%',
                containLabel: true
            },
            xAxis: {
                data: ['美的南沙分厂', '美的商业空调事业部', '佛山信华'],
                axisLabel: {
                    show: true,
                    textStyle: {
                        fontSize: '12px',
                        color: '#fff',
                    }
                },
                axisLine: {
                    lineStyle: {
                        color: '#fff',
                        width: 1,
                    }
                }
            },

            yAxis: {
                axisLabel: {
                    show: true,
                    textStyle: {
                        fontSize: '12px',
                        color: '#fff',
                    }
                },
                axisLine: {
                    lineStyle: {
                        color: '#fff',
                        width: 1,
                    }
                },
                splitLine: {
                    show: false,
                }
            },

            series: {
                name: '',
                type: 'bar',
                barWidth: 20,
                data: ['15', '13', '17'],
                itemStyle: {
                    normal: {
                        color: new echarts.graphic.LinearGradient(
                            0, 0, 0, 1,
                            [
                                { offset: 0, color: '#3876cd' },
                                { offset: 0.5, color: '#45b4e7' },
                                { offset: 1, color: '#54ffff' }
                            ]
                        ),
                    },
                },
            },
        }

        //弹出框调用ECharts折线图
        summaryLine = echarts.init(document.getElementById('summaryLine'));
        var lineOption = {

            tooltip: {
                trigger: 'item',
                formatter: function (params) {
                    var res = '本月' + params.name + '号运单数：' + params.data;
                    return res;
                }
            },
            grid: {
                top: '20%',
                left: '0%',
                width: '100%',
                height: '80%',
                containLabel: true
            },
            xAxis: {
                data: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12', '13', '14', '15', '16', '17', '18', '19', '20', '21', '22', '23', '24', '25', '26', '27', '28', '29', '30', '31'],
                axisLabel: {
                    show: true,
                    textStyle: {
                        fontSize: '12px',
                        color: '#3e70b0',
                    }
                },
                axisLine: {
                    lineStyle: {
                        color: '#0e2c52',
                        width: 1,
                    }
                }
            },

            yAxis: {
                axisLabel: {
                    show: true,
                    textStyle: {
                        fontSize: '12px',
                        color: '#3e70b0',
                    }
                },
                axisLine: {
                    lineStyle: {
                        color: '#0e2c52',
                        width: 1,
                    }
                },
                splitLine: {
                    show: true,
                    lineStyle: {
                        color: '#0e2c52',
                        width: 1,
                    }
                }
            },

            series: {
                name: '',
                type: 'line',
                data: ['5', '14', '3', '6', '8', '18', '11', '4', '8', '7', '16', '13', '6', '10', '11', '9', '19', '13', '4', '20', '12', '7', '13', '15', '8', '3', '9', '16', '11', '16', '8'],
                areaStyle: {
                    normal: {
                        color: 'rgba(79,237,247,0.3)',
                    }
                },
                itemStyle: {
                    normal: {
                        lineStyle: {
                            color: '#00dafb',
                            width: 1,
                        },
                        color: '#00dafb',
                    },
                },
            },
        }

        summaryPie1.setOption(pieOption1);
        summaryPie2.setOption(pieOption2);
        summaryPie3.setOption(pieOption3);
        summaryBar.setOption(barOption);
        summaryLine.setOption(lineOption);
    }

    //弹窗
    $('.summaryBtn').on('click', function () {
        $('.filterbg').show();
        $('.popup').show();
        $('.popup').width('3px');
        $('.popup').animate({ height: '76%' }, 400, function () {
            $('.popup').animate({ width: '82%' }, 400);
        });
        setTimeout(summaryShow, 800);
    });
    $('.popupClose').on('click', function () {
        $('.popupClose').css('display', 'none');
        $('.summary').hide();
        summaryPie1.clear();
        summaryPie2.clear();
        summaryPie3.clear();
        summaryBar.clear();
        summaryLine.clear();
        $('.popup').animate({ width: '3px' }, 400, function () {
            $('.popup').animate({ height: 0 }, 400);
        });
        setTimeout(summaryHide, 800);
    });
    function summaryShow() {
        $('.popupClose').css('display', 'block');
        $('.summary').show();
        setSummary();

    };
    function summaryHide() {
        $('.filterbg').hide();
        $('.popup').hide();
        $('.popup').width(0);
    };

    $(window).resize(function () {
        myChartMonthAlarm.resize();
        try {
            summaryPie1.resize();
            summaryPie2.resize();
            summaryPie3.resize();
            summaryBar.resize();
            summaryLine.resize();
        } catch (err) {
            return false;
        }
    });


    //加载后端数据
    SetAlarmType();
    SetDataStatis();
    SetShipList(null);
    SetMonthAlarmStatis();
    SetCountInfo();
    SetAttendance();
});


/**************************************调用业务数据 Start************************************/
//报警类型分析
function SetAlarmType(month) {
    var myChartAlarm = echarts.init(document.getElementById('myChartAlarm'));
    $.ajax({
        type: "get",
        url: "/Home/GetAlarmType",
        data: { month: month },
        success: function (res) {
            //长度为6 的数组填充默认值0
            let dataArray = Array.apply(null, Array(6)).map(() => 0);
            res.forEach(function (k, v) {
                dataArray[k.type - 1] = k.num;
            });
            var optionAlarm = {
                color: ['#339899'],
                tooltip: {
                    trigger: 'axis',
                    axisPointer: {            // 坐标轴指示器，坐标轴触发有效
                        type: 'shadow'        // 默认为直线，可选为：'line' | 'shadow'
                    }
                },
                grid: {
                    left: '3%',
                    right: '4%',
                    bottom: '3%',
                    containLabel: true
                },
                xAxis: [
                    {
                        name: '类型',
                        nameTextStyle: {
                            color: '#fff'
                        },
                        type: 'category',
                        data: ['安全帽', '打电话', '睡觉', '打架', '考勤入', '考勤出'],//type 1 2 3 4 5 6
                        axisTick: {
                            alignWithLabel: true
                        },
                        axisLabel: {
                            color: '#fff',
                            fontWeight: 'bold'
                        }
                    }
                ],
                yAxis: [
                    {
                        name: '次数',
                        nameTextStyle: {
                            color: '#fff'
                        },
                        type: 'value',
                        axisLabel: {
                            color: '#fff'
                        }
                    }
                ],
                series: [
                    {
                        name: '报警次数',
                        type: 'bar',
                        barWidth: '50%',
                        data: dataArray
                    }
                ]
            };
            myChartAlarm.setOption(optionAlarm);
        }
    })
}

//设备数据统计
function SetDataStatis() {
    var myCountStatis = echarts.init(document.getElementById('myCountStatis'));
    $.ajax({
        type: "get",
        url: "/Home/GetDataStatis",
        success: function (res) {
            var optionCountStatis = {
                legend: {},
                tooltip: {},
                dataset: {
                    source: [
                        ['product', 'Device', 'Camera'],
                        ['启用', res.enableDeviceCount, res.enableCameraCount],
                        ['未启用', res.stopDeviceCount, res.stopCameraCount]
                    ]
                },
                series: [{
                    name: 'NVR设备',
                    type: 'pie',
                    radius: 45,
                    center: ['32%', '60%'],
                    encode: {
                        itemName: 'product',
                        value: 'Device'
                    }
                }, {
                    name: '摄像机',
                    type: 'pie',
                    radius: 45,
                    center: ['70%', '60%'],
                    encode: {
                        itemName: 'product',
                        value: 'Camera'
                    }
                }],
                color: [/*'#c23531', '#2f4554',*/ '#61a0a8', '#d48265', '#91c7ae', '#749f83', '#ca8622', '#bda29a', '#6e7074', '#546570', '#c4ccd3']
            };
            myCountStatis.setOption(optionCountStatis);
        }
    });
}

//船舶信息集合 高德地图展示
function SetShipList(shipName) {
    var map = new AMap.Map("myMap", {
        resizeEnable: true,
        zoom: 7,
        mapStyle: 'amap://styles/darkblue',
        //center: [120.394045, 35.988562]
    });
    $.ajax({
        type: "get",
        url: "/Home/GetShipList",
        data: { shipName: shipName },
        success: function (res) {
            res.forEach(function (item, index) {
                if (item.coordinate == null || item.coordinate == "") return;
                var marker, lineArr = handleArrayStr(item.coordinate.split(","));

                marker = new AMap.Marker({
                    map: map,
                    position: lineArr[lineArr.length - 1],
                    icon: "/datacenter/images/s_ico4.png",
                    offset: new AMap.Pixel(-26, -13),
                    autoRotation: true,
                    angle: -90,
                });
                marker.content = '<p>' + item.name + '</p>' +
                    '<p>是否在港：' + (item.flag == true ? "航行" : "停港") + '</p>' +
                    '<p>船员数量：' + (item.crewNum == 0 ? "未知" : (item.crewNum + "人")) + '</p>';
                marker.on('click', markerClick);

                // 绘制轨迹
                var polyline = new AMap.Polyline({
                    map: map,
                    path: lineArr,
                    showDir: true,
                    strokeColor: "#28F",  //线颜色
                    strokeOpacity: 0.5,     //线透明度
                    strokeWeight: 2,      //线宽
                    strokeStyle: "dashed"  //线样式solid
                });
                var passedPolyline = new AMap.Polyline({
                    map: map,
                    //path: lineArr,
                    strokeColor: getRandomColor(),
                    strokeOpacity: 0.5,
                    strokeWeight: 2,
                    strokeStyle: "dashed"
                });
                marker.on('moving', function (e) {
                    passedPolyline.setPath(e.passedPath);
                });
                //地图渲染 开始动画
                map.setFitView();
                marker.moveAlong(lineArr, 3000);
                //信息弹窗
                var infoWindow = new AMap.InfoWindow({
                    offset: new AMap.Pixel(-5, -8)
                });
                function markerClick(e) {
                    infoWindow.setContent(e.target.content);
                    infoWindow.open(map, e.target.getPosition());
                }
                map.on('click', function () {
                    infoWindow.close();
                });
            });
        }
    });
}
//月报警量统计
function SetMonthAlarmStatis() {
    //月运单量统计图
    var myChartMonthAlarm = echarts.init(document.getElementById('myChartMonthAlarm'));
    var optionMonthAlarm = {
        tooltip: {
            trigger: 'item',
            formatter: function (params) {
                var res = '本月' + params.name + '号运单数：' + params.data;
                return res;
            }
        },
        grid: {
            top: '5%',
            left: '0%',
            width: '100%',
            height: '95%',
            containLabel: true
        },
        xAxis: {
            data: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12', '13', '14', '15', '16', '17', '18', '19', '20', '21', '22', '23', '24', '25', '26', '27', '28', '29', '30', '31'],
            axisLabel: {
                show: true,
                textStyle: {
                    fontSize: '12px',
                    color: '#fff',
                }
            },
            axisLine: {
                lineStyle: {
                    color: '#fff',
                    width: 1,
                }
            }
        },

        yAxis: {
            axisLabel: {
                show: true,
                textStyle: {
                    fontSize: '12px',
                    color: '#fff',
                }
            },
            axisLine: {
                lineStyle: {
                    color: '#fff',
                    width: 1,
                }
            },
            splitLine: {
                show: false,
            }
        },

        series: {
            name: '',
            type: 'bar',
            barWidth: 10,
            data: ['5', '14', '3', '6', '8', '18', '11', '4', '8', '7', '16', '13', '6', '10', '11', '9', '19', '13', '4', '20', '12', '7', '13', '15', '8', '3', '9', '16', '11', '16', '8'],
            itemStyle: {
                normal: {
                    barBorderRadius: [5, 5, 5, 5],
                    color: new echarts.graphic.LinearGradient(
                        0, 0, 0, 1,
                        [
                            { offset: 0, color: '#3876cd' },
                            { offset: 0.5, color: '#45b4e7' },
                            { offset: 1, color: '#54ffff' }
                        ]
                    ),
                },
            },
        },
    }
    $.get("/Home/GetMonthAlarmStatis", function (res) {
        debugger;
    });
    setTimeout(function () {
        myChartMonthAlarm.setOption(optionMonthAlarm);
    }, 500);
}

//基本信息
function SetCountInfo() {
    $.get("/Home/GetCountInfo", function (res) {
        $("#indicator1").attr("total", res[0].sailCount);
        $("#indicator2").attr("total", res[0].portCount);
        $("#indicator3").attr("total", res[0].crewCount);
    });
}
//考勤状态
function SetAttendance() {
    $.get("/Home/GetAttendance", function (res) {
        if (res != null) {
            let temp = "";
            res.forEach(function (item) {
                let behavior = "";
                if (item.behavior == 0)
                    behavior = '<span style="background-color: #40CE48;">签入</span>';
                else
                    behavior = '<span style="background-color: #8A2bE2;">签出</span>';


                temp += '<li><div class="fontInner clearfix">' +
                    '<span>' + item.shipName + '</span>' +
                    '<span>' + item.crewName + '</span>' +
                    '<span>' + dateFtt("yyyy-MM-dd hh:mm:ss", item.time) + '</span>' + behavior +
                    '<span><div class="progress" progress="' + (item.rate == 0 ? 1 : Math.round(item.rate)) + '%">' +
                    '<div class="progressBar"><span></span></div>' +
                    '<h3><i><h4></h4></i></h3>' +
                    '</div></span>' +
                    '</div></li>';
            });
            $("#FontScroll ul").empty();
            $("#FontScroll ul").append(temp);
            //考勤状态文字滚动
            $('#FontScroll').FontScroll({ time: 2000, num: 1 });
        }
    })
}


//处理数据库里返回的经纬度字符串数组，转为二维Float类型数组
function handleArrayStr(lineArr) {
    var arr = new Array();
    var num = 0;
    let len = lineArr.length / 2;
    for (var x = 0; x < len; x++) {
        arr[x] = new Array();
        for (var y = 0; y < 2; y++) {
            arr[x][y] = parseFloat(lineArr[num].replace("[", "").replace("]", ""));
            num++;
        }
    }
    return arr;
}
//生成十六进制的颜色值
function getRandomColor() {
    return '#' + (function (color) {
        return (color += '0123456789abcdef'[Math.floor(Math.random() * 16)])
            && (color.length == 6) ? color : arguments.callee(color);
    })('');
}
//时间格式化处理
function dateFtt(fmt, date) {
    date = new Date(date);
    var o = {
        "M+": date.getMonth() + 1,                 //月份   
        "d+": date.getDate(),                    //日   
        "h+": date.getHours(),                   //小时   
        "m+": date.getMinutes(),                 //分   
        "s+": date.getSeconds(),                 //秒   
        "q+": Math.floor((date.getMonth() + 3) / 3), //季度   
        "S": date.getMilliseconds()             //毫秒   
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (date.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
}
/**************************************调用业务数据 End************************************/