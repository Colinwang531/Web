; layui.define(['jquery', 'layer', 'form', 'common'], function (exports) {
    "use strict";
    var $ = layui.$, layer = layui.layer, form = layui.form, common = layui.common,
        that, P = {};
    var Transfer = function () {
        /*得到最右边的所有值*/
        this.getRightAll = function () {
            var _inputs = $(".transfer-panel-right .transfer-div").find("dd input[type='checkbox']");
            var obj = null;
            if (_inputs != null && _inputs.length > 0) {
                obj = new Array();
                $.each(_inputs, function (idx, item) {
                    var _value = $(item).parents("dd").attr("lay-value"),
                        _name = $(item).parents("dd").attr("lay-title"),
                        _sort = $(item).parents("dd").attr("lay-sort");
                    /*注意：这里用的是首字母大写开头的，用于传递对象数据，自动与Model匹配*/
                    obj.push({ Id: _value, Name: _name, Sort: _sort });
                });
            }
            return obj;
        };
    };
    var FS = {
        __leftSelectedAry: null,
        __rightSelectedAry: null,
        listen: function () {

            form.on('checkbox(transferLeftAllChoose)', function (data) {
                var chkAll = data.elem.checked, btn_right = $(".transfer-to-right button"), $inputs = $(':checkbox[lay-filter="transferLeftChecked"]').not(':disabled').prop('checked', chkAll);
                if (chkAll && $inputs.length > 0) {
                    btn_right.removeClass("layui-btn-disabled");
                    FS.__leftSelectedAry = new Array();
                    $.each($inputs, function (idx, item) {
                        var _value = $(item).parents("dd").attr("lay-value"),
                            _name = $(item).parents("dd").attr("lay-title"),
                            _sort = $(item).parents("dd").attr("lay-sort");
                        FS.__leftSelectedAry.push({ id: _value, name: _name, sort: _sort });
                    });
                }
                else {
                    FS.__leftSelectedAry = null;
                    btn_right.addClass("layui-btn-disabled");
                }
                form.render('checkbox');
            });
            /*左边checkbox监听*/
            form.on('checkbox(transferLeftChecked)', function (data) {
                var $this = $(data.othis),
                    $parent = $this.parents(".transfer-content"),
                    $inputs = $this.parents(".transfer-div").find("dd input[type='checkbox']:checked");
                if ($inputs.length > 0) {
                    $parent.find(".transfer-to-right button").removeClass("layui-btn-disabled");
                    FS.__leftSelectedAry = new Array();
                    $.each($inputs, function (idx, item) {
                        var _value = $(item).parents("dd").attr("lay-value"),
                            _name = $(item).parents("dd").attr("lay-title"),
                            _sort = $(item).parents("dd").attr("lay-sort");
                        FS.__leftSelectedAry.push({ id: _value, name: _name, sort: _sort });
                    });
                }
                else {
                    $parent.find(".transfer-to-right button").addClass("layui-btn-disabled");
                }
            });
            /*右边checkbox监听*/
            form.on('checkbox(transferRightChecked)', function (data) {
                var $this = $(data.othis),
                    $parent = $this.parents(".transfer-content"),
                    $inputs = $this.parents(".transfer-div").find("dd input[type='checkbox']:checked");

                if ($inputs.length > 0) {
                    $parent.find(".transfer-to-left button").removeClass("layui-btn-disabled");
                    FS.__rightSelectedAry = new Array();
                    $.each($inputs, function (idx, item) {
                        var _value = $(item).parents("dd").attr("lay-value"),
                            _name = $(item).parents("dd").attr("lay-title"),
                            _sort = $(item).parents("dd").attr("lay-sort");
                        FS.__rightSelectedAry.push({ id: _value, name: _name, sort: _sort });
                    });
                }
                else {
                    $parent.find(".transfer-to-left button").addClass("layui-btn-disabled");
                }
            });
            /*右移*/
            $('.transfer-to-right').off('click').on('click', function () {
                var $this = $(this),
                    $parent = $this.parents(".transfer-content"),
                    _name = $this.attr("lay-name") || "",
                    $right = $parent.find(".transfer-panel-right .transfer-div"),
                    $left = $parent.find(".transfer-panel-left .transfer-div");
                $("#transferLeftAllChoose").prop('checked', false);
                /*追加右边值*/
                if (FS.__leftSelectedAry != null) {
                    $.each(FS.__leftSelectedAry, function (idx, item) {
                        $right.append(['<dd lay-value="' + item.id + '" lay-sort="' + item.sort + '" lay-title="' + item.name + '"><input type="hidden" name="' + _name + '" value="' + item.id + '">',
                            '<input lay-filter="transferRightChecked"  ',
                        ' type="checkbox" title="' + item.name + '" lay-skin="primary"></dd>'
                        ].join(""));
                    });
                }
                /*移除左边值*/
                var inputs = $left.find("dd input[type='checkbox']");
                for (var i = 0; i < inputs.length; i++) {
                    if ($(inputs[i]).is(':checked')) {
                        $(inputs[i]).parents("dd").remove();
                    }
                }
                /*重置‘右移’按钮的状态*/
                $parent.find(".transfer-to-right button").addClass("layui-btn-disabled");
                /*重置左边选中项*/
                FS.__leftSelectedAry = null;
                /*重新渲染*/
                form.render('checkbox');
                /*如果不加这个会页面自动刷新，暂时还不知道原因*/
                return false;
            });
            /*左移*/
            $('.transfer-to-left').off('click').on('click', function () {
                var $this = $(this),
                    $parent = $this.parents(".transfer-content"),
                    _name = $this.attr("lay-name") || "",
                    $right = $parent.find(".transfer-panel-right .transfer-div"),
                    $left = $parent.find(".transfer-panel-left .transfer-div");
                /*追加左边值*/
                if (FS.__rightSelectedAry != null) {
                    $.each(FS.__rightSelectedAry, function (idx, item) {
                        $left.append(['<dd lay-value="' + item.id + '" lay-sort="' + item.sort + '" lay-title="' + item.name + '"><input type="hidden" name="' + _name + '" value="' + item.id + '">',
                            '<input lay-filter="transferLeftChecked"  ',
                        ' type="checkbox"  title="' + item.name + '" lay-skin="primary"></dd>'
                        ].join(""));
                    });
                }
                /*移除右边值*/
                var inputs = $right.find("dd input[type='checkbox']");
                for (var i = 0; i < inputs.length; i++) {
                    if ($(inputs[i]).is(':checked')) {
                        $(inputs[i]).parents("dd").remove();
                    }
                }
                /*重置‘右移’按钮的状态*/
                $parent.find(".transfer-to-left button").addClass("layui-btn-disabled");
                /*重置左边选中项*/
                FS.__rightSelectedAry = null;
                /*重新渲染*/
                form.render('checkbox');
                /*如果不加这个会页面自动刷新，暂时还不知道原因*/
                return false;
            });
            /*搜索*/
            $(document).on('keypress', ".transfer-search-div .search_condition", function (e) {
                e.stopPropagation();
                if (/^13$/.test(e.keyCode)) {
                    FS.search($(this));
                }
            });
            /*清空*/
            $(document).on("click", ".transfer-search-div .search-clear-btn", function (event) {
                $(this).prev().val("");
                FS.search($(this));
            });
        },
        search: function ($t) {
            var _value = $t.val(),
                _regExp = new RegExp(_value),
                $parent = $t.parents(".transfer-content"),
                lstDD = $parent.find(".transfer-panel-left .transfer-div").find("dd");
            for (var i = 0, t = lstDD.length; i < t; i++) {
                if (_value == "") {
                    $(lstDD[i]).show();
                }
                else if (_regExp.test($(lstDD[i]).attr("lay-title"))) {
                    $(lstDD[i]).show();
                }
                else {
                    $(lstDD[i]).hide();
                }
            }
        },
        redner: function (data, $t) {
            var _name = $t.attr("name");
            if (_name.indexOf('[') == -1) { name += "[]"; }
            var _values = ($t.attr("value") || "").split(',');

            $t.removeAttr("name");
            $t.removeAttr("value");

            /*读取监听标识*/
            var filter = P.filter || "";
            /*读取复选框禁用的值*/
            var _disableds = (P.disabled || "").split(",");
            /*是否开启级联*/
            var _cascade = $t.attr("cascade") || "false";
            var _searchHtml = "";

            if (_cascade === "false") {
                _searchHtml = [
                    '<div lay-value="" class="transfer-search-div">',
                    '<div class="checkall-right" ><input id="transferLeftAllChoose" type="checkbox" lay-skin="primary" lay-filter="transferLeftAllChoose" /></div>',
                    '<i class="layui-icon  drop-search-btn"></i>',
                    '<input class="layui-input search_condition" placeholder="搜索（按Enter键）">',
                    '<i class="layui-icon  clear-btn search-clear-btn" title="清除">&#x1006;</i>',
                    '</div>'
                ].join("");
            }

            var lstLeft = "", lstRight = "";
            if (data != null && data.length > 0) {
                $.each(data, function (idx, item) {
                    if (_values.indexOf(item.id) == -1) {
                        /*左边装载数据*/
                        var _input = '<dd lay-value="' + item.id + '" lay-sort="' + item.sort + '" lay-title="' + item.name + '"><input type="checkbox" lay-filter="transferLeftChecked" title="' + item.name + '" lay-skin="primary"></dd>';
                        if (_disableds.indexOf(item.id) != -1 || item.disabled) {
                            _input = _input.replace("<input", "<input disabled ");
                        }
                        lstLeft += _input;
                    }
                    else {
                        /*右边装载数据*/
                        var _input = '<dd lay-value="' + item.id + '" lay-sort="' + item.sort + '"  lay-title="' + item.name + '"><input type="hidden" name="' + _name + '" value="' + item.id + '"><input lay-filter="transferRightChecked"   type="checkbox"  title="' + item.name + '" lay-skin="primary"></dd>';
                        /*设置禁用：1参数中带了禁用值的，2返回数据中disabled为true的*/
                        if ((_disableds.indexOf(item.id) != -1)) {
                            _input = _input.replace("<input", "<input disabled ");
                        }
                        lstRight += _input;
                    }
                    $t.append(_input);
                });
            }
            var outHtml =
                $t.html([
                    '<div class="transfer-content">',
                    '<div class="transfer-panel transfer-panel-left">',
                    _searchHtml,
                    '<dl class="transfer-div">',
                    lstLeft,
                    '</dl>',
                    '</div>',
                    '<div class="transfer-btn transfer-to-right">',
                    '<button title="右移" lay-name="' + _name + '" class="layui-btn layui-btn-normal layui-btn-sm layui-btn-disabled"><i class="layui-icon">&#xe65b;</i></button>',
                    '</div>',
                    '<div class="transfer-btn  transfer-to-left">',
                    '<button title="左移" lay-name="' + _name + '"  class="layui-btn layui-btn-normal layui-btn-sm layui-btn-disabled"><i class="layui-icon">&#xe65a;</i></button>',
                    '</div>',
                    '<div class="transfer-panel transfer-panel-right">',
                    '<div lay-value="" class="transfer-search-div">',
                    '<span  class="transfer-title" >',
                    ' 已选列表',
                    '</span>',
                    '</div>',
                    '<dl class="transfer-div">',
                    lstRight,
                    '</dl>',
                    '</div>',
                    '</div>'
                ].join(""));
            $t.append(outHtml);
        },
        getData: function (url) {
            var data;
            common.GET(url, {}, false, function (result) {
                if (result.success) {
                    data = result.data;
                } else {
                    data = {};
                    layer.msg(result.msg);
                }
            }, function (error) {
                data = {};
                layer.msg(error.msg);
            }, false);
            return data;
        }
    };
    Transfer.prototype.render = function () {
        that = this;
        var $t = $(".transfer-tool"), props = $t.attr("lxdProps");
        if (!props) {
            return;
        }
        P = eval("(" + props + ")");
        FS.redner(FS.getData(P.url), $t);
        FS.listen();
    }

    var transfer = new Transfer();
    transfer.render();
    exports('transfer', transfer);
});