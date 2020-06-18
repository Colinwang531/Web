/*通用JS封装 @author Ahri 2018.7.17*/
; layui.define(['jquery', 'layer'], function (exports) {
    "use strict";
    var $ = layui.$, layer = layui.layer, msgIndex = 0;
    var common = {
        /**
         * 计算分页页码
         * @param {Number} count 总条数
         * @param {Number} limit 每页显示条数
         * @param {Number} len 被减掉数据的条数
         * @param {Number} cpage 当前页码
         * @returns {Number} 最大页码
         */
        page: function (count, limit, len, cpage) {
            var clast = Math.ceil((count - len) / limit) || 1;
            return (clast >= cpage ? cpage : clast);
        },
        /**
         * 获取一个随机数
         * @returns {number} 随机数
         */
        getRandom: function () {
            return (Math.random() * 1000 | 0);
        },
        /*身份证隐藏月日*/
        cardHide: function (str) {
            return str.replace(/^(.{10})(?:\d+)(.{4})$/, "$1****$2");
        },
        /**
         * 隐藏电话号码中间四位
         * @param {String} str 电话号码
         */
        phoneHide: function (str) {
            return str.replace(/^(.{3})(?:\d+)(.{4})$/, "$1****$2");
        },
        /**
         * 字符串为空等判断
         * @param {String} str 字符串
         */
        isNull: function (str) {
            return str == "" || str == null || str == undefined || typeof (str) == "undefined";
        },
        /**
         * 从对象数组中删除属性为objPropery，值为objValue元素的对象
         * @param {Array} arrPerson  数组对象
         * @param {String} objPropery  对象的属性
         * @param {String} objPropery  对象的值
         * @return {Array} 过滤后数组
         */
        _REMOVE: function (arrPerson, objPropery, objValue) {
            return $.grep(arrPerson, function (cur, i) {
                return cur[objPropery] != objValue;
            });
        },
        /**
         * 从对象数组中获取属性为objPropery，值为objValue元素的对象
         * @param {Array} arrPerson  数组对象
         * @param {String} objPropery  对象的属性
         * @param {String} objPropery  对象的值
         * @return {Array} 过滤后的数组
         */
        _FIND: function (arrPerson, objPropery, objValue) {
            return $.grep(arrPerson, function (cur, i) {
                return cur[objPropery] == objValue;
            });
        },
        formatMoney: function (places, symbol, thousand, decimal) {
            places = !isNaN(places = Math.abs(places)) ? places : 2;
            symbol = symbol !== undefined ? symbol : "$";
            thousand = thousand || ",";
            decimal = decimal || ".";
            var number = this,
                negative = number < 0 ? "-" : "",
                i = parseInt(number = Math.abs(+number || 0).toFixed(places), 10) + "",
                j = (j = i.length) > 3 ? j % 3 : 0;
            return symbol + negative + (j ? i.substr(0, j) + thousand : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousand) + (places ? decimal + Math.abs(number - i).toFixed(places).slice(2) : "");
        },
        /**
         * 身份证验证
         * 
         * @param {String} idcard  身份证号
         * @param {String} elem    错位焦点定位的元素ID
         * @return {Boolean}
         */
        checkIDCard: function (idcard, elem) {
            idcard = idcard.toLocaleLowerCase();
            var arrVerifyCode = [1, 0, "x", 9, 8, 7, 6, 5, 4, 3, 2];
            var Wi = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2];
            var Checker = [1, 9, 8, 7, 6, 5, 4, 3, 2, 1, 1];

            var Ai = idcard.length == 18 ? idcard.substring(0, 17) : idcard.slice(0, 6) + "19" + idcard.slice(6, 16);
            if (!/^\d+$/.test(Ai)) {
                $("#" + elem + "").focus();
                return false;
            }
            var yyyy = Ai.slice(6, 10), mm = Ai.slice(10, 12) - 1, dd = Ai.slice(12, 14);
            var d = new Date(yyyy, mm, dd), now = new Date();
            var year = d.getFullYear(), mon = d.getMonth(), day = d.getDate();

            if (year != yyyy || mon != mm || day != dd || d > now || year < 1900) {
                $("#" + elem + "").focus();
                return false;
            }
            for (var i = 0, ret = 0; i < 17; i++) {
                ret += Ai.charAt(i) * Wi[i];
            }
            Ai += arrVerifyCode[ret %= 11];
            if (idcard.length == 18 && idcard != Ai) {
                $("#" + elem + "").focus();
                return false;
            }
            return true;
        },
        /**
         * 根据name从url获取参数
         * @param {string} name  参数名
         * @return {string} 参数值
         */
        getUrlParam: function (name) {
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
            var r = window.location.search.substr(1).match(reg);
            if (r != null)
                return decodeURI(r[2]);
            return null;
        },
        /**
         * remark 封装的ajax.post请求
         * @author Ahri 2018.6.4
         * @param {String} url                 请求地址(必要)
         * @param {Object} data                请求参数(必要)
         * @param {Boolean} async              同步异步(必要)(true:异步，false：同步)
         * @param {Function} successCallBack   成功回调(必要)
         * @param {Function} errorCallBack     错误回调(必要)
         * @param {Boolean} loading            是否显示ajax请求提示信息，默认为：true
         * @param {String} message             请求自定义提示信息，默认为：努力加载中...
         * @param {String} dataType            数据类型，默认为：json
         */
        GET: function (url, data, async, successFunc, errorFunc, loading, message, dataType) {

            async = async == undefined ? true : async;
            dataType = this.isNull(dataType) ? "json" : dataType;
            url += ((url.indexOf("?") > 0) ? '&' : '?') + '__dt__for__ie11_nocache=' + new Date();
            message = this.isNull(message) ? "正在读取数据，请稍后..." : message;
            loading = loading == undefined ? true : loading;
            $.ajax({
                url: url
                , type: "get"
                , async: async
                , data: data
                , dataType: dataType
                , beforeSend: function (xhr) {
                    if (loading) {
                        try { msgIndex = layer.msg(message, { icon: 16, time: false, shade: 0.5 }); } catch (e) { }
                    }
                }
                , success: function (data) {
                    if (!data.success && data.code == 2002) {
                        try {
                            debugger
                            layer.msg(data.msg, { icon: 7, time: 2000 }, function () { window.opener = null; window.top.location.href = data.redirectUrl; });
                        }
                        catch (e) { }
                    }
                    else {
                        if (typeof (successFunc) !== "undefined") { successFunc(data); }
                    }
                }
                , error: function (e) {
                    if (typeof (errorFunc) !== "undefined") { var data = { code: 400, success: false, msg: "请求错误", count: 0 }; errorFunc(data); } else {
                        try { layer.msg("请求错误", { icon: 5, time: 2000 }); } catch (e) { }
                    }
                }
                , complete: function () {
                    if (loading) { try { layer.close(msgIndex); } catch (e) { } }
                }
            });
        },
        /**
         * remark 封装的ajax.post请求
         * @author Ahri 2018.6.4
         * @param {String} url                 请求地址(必要)
         * @param {Object} data                请求参数(必要)
         * @param {Boolean} async              同步异步(必要)(true:异步，false：同步)
         * @param {Function} successCallBack   成功回调(必要)
         * @param {Function} errorCallBack     错误回调(必要)
         * @param {Boolean} loading            是否显示ajax请求提示信息，默认为：true
         * @param {String} message             请求自定义提示信息，默认为：努力加载中...
         * @param {String} dataType            数据类型，默认为：json
         */
        POST: function (url, data, async, successFunc, errorFunc, loading, message, dataType, token, isGetToken) {

            async = async == undefined ? true : async;
            dataType = this.isNull(dataType) ? "json" : dataType;
            token = this.isNull(token) ? "" : token;
            message = this.isNull(message) ? "正在处理数据，请稍后..." : message;
            loading = loading == undefined ? true : loading;
            isGetToken = isGetToken == undefined ? false : isGetToken;

            $.ajax({
                url: url
                , type: "post"
                , async: async
                , data: data
                , crossDomain: true
                , dataType: dataType
                , beforeSend: function (xhr) {
                    xhr.setRequestHeader('Authorization', 'Bearer ' + token);
                    if (isGetToken) {
                        xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                    }
                    if (loading) {
                        try { msgIndex = layer.msg(message, { icon: 16, time: false, shade: 0.5 }); } catch (e) { }
                    }
                }
                , success: function (data) {
                    if (!data.success && data.code == 2002) {
                        try {
                            layer.msg(data.msg, { icon: 7, time: 2000 }, function () { window.opener = null; window.top.location.href = data.redirectUrl; });
                        }
                        catch (e) { }
                    }
                    else {
                        if (typeof (successFunc) !== "undefined") { successFunc(data); }
                    }
                }
                , error: function (e) {
                    if (typeof (errorFunc) !== "undefined") {
                        var data = { code: 400, success: false, msg: "请求错误", count: 0 };
                        errorFunc(data);
                    }
                    else {
                        try { layer.msg("请求错误", { icon: 5, time: 2000 }); } catch (e) { }
                    }
                }
                , complete: function () {
                    if (loading) { try { layer.close(msgIndex); } catch (e) { } }
                }
            });
        },

        /**
         * 封装的ajax.post请求
         * @author huangchuan 2017.7.10
         * @param {String} url               请求地址
         * @param {Object} postData          请求参数（JSON对象）
         * @param {Function} successCallback 成功回调（函数）
         * @param {Function} errorCallBack   错误回调（函数）
         * @param {String} layerMsg          ajax请求自定义提示信息，默认为：努力加载中...
         * @param {Boolean} loadAsync        同步异步（布尔值），默认为：false
         * @param {Boolean} isShowLayer      是否显示ajax请求提示信息，默认为：true
         * @param {Boolean} tipType          提示方式（Bool），默认为：false
         */
        PostArrange: function (url, postData, successCallback, errorCallBack, layerMsg, loadAsync, isShowLayer, tipType, btnName) {


            var disableCtrlS = function (e) {
                //屏蔽ctrl+s快捷键;可以判断是不是mac，如果是mac,ctrl变为花键
                if (e.keyCode == 83 && (navigator.platform.match("Mac") ? e.metaKey : e.ctrlKey)) {
                    e.preventDefault();
                }
            };

            //屏蔽ctrl+s,不然排课窗口会被关闭
            document.addEventListener("keydown", disableCtrlS, false);
            parent.document.addEventListener("keydown", disableCtrlS, false);

            if (btnName === undefined || btnName === "") {
                btnName = "正在排课中： ";
            }
            var intertime;
            var whentime = 1;
            var dataType = "json";
            loadAsync = loadAsync || false;
            tipType = tipType || false;
            isShowLayer = isShowLayer == undefined ? true : isShowLayer;
            $.ajax({
                url: url,
                type: "post",
                data: postData,
                async: loadAsync,
                dataType: dataType,
                beforeSend: function (xhr) {
                    layui.use(['layer'], function () {
                        if (tipType) {
                            $(".returntime").html(btnName + whentime + ' s');
                            intertime = setInterval(function () {
                                whentime++;
                                $(".returntime").html(btnName + whentime + ' s');
                            }, 1000);
                        }
                        else {
                            Utility.Config.msgIndex = layer.msg('<span class="returntime">' + layerMsg + btnName + whentime + ' 秒</span>', { icon: 16, time: false, shade: 0.5 });
                            intertime = setInterval(function () {
                                whentime++;
                                $(".returntime").html(layerMsg + btnName + whentime + " 秒");
                            }, 1000);
                        }
                    });
                },
                success: function (data) {
                    if (!data.Success && data.Status == "2002") {
                        //ajax请求发现登录状态丢失
                        layer.msg(data.Message, {
                            icon: 7,
                            time: 2000
                        }, function () {
                            window.opener = null;
                            window.top.location.href = data.Result.Url;
                        });
                    }
                    else {
                        if (typeof (successCallback) !== 'undefined') { successCallback(data); }
                    }
                },
                error: function (e) {
                    clearInterval(intertime);
                    var data = { Success: false, Status: "-1", Message: "请求错误" };
                    if (typeof (errorCallBack) !== 'undefined') { errorCallBack(data); }
                },
                complete: function (xhr, status) {
                    clearInterval(intertime);
                    $(".returntime").html("已完成,用时 " + whentime + " 秒");

                    //移除屏蔽
                    document.removeEventListener("keydown", disableCtrlS, false);
                    parent.document.removeEventListener("keydown", disableCtrlS, false);
                }
            });

        }
    };

    exports('common', common);
});