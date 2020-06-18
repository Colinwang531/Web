/*通用Combobox封装 @author Ahri 2018.7.19*/
; layui.define(['jquery', 'form', 'common'], function (exports) {
    "use strict";
    var $ = layui.$, form = layui.form, common = layui.common;
    var combobox = {
        /*HTML元素append*/
        stitching: function (data, elem, defaultValue, isCheckedFirst, emptyText) {
            isCheckedFirst = isCheckedFirst == undefined ? true : isCheckedFirst;
            if (common.isNull(defaultValue) && !isCheckedFirst) {
                $(elem).append('<option value="">' + (common.isNull(emptyText) ? "--请选择--" : emptyText) + '</option>');
            }
            $.each(data, function (idx, item) {
                var strChecked = '';
                if (defaultValue && item.id == defaultValue) {
                    item.checked = true;
                } else if (defaultValue && item.id != defaultValue) {
                    item.checked = false;
                }
                if ((item.id == defaultValue) || (common.isNull(defaultValue) && isCheckedFirst && idx == 0 && !item.checked) || item.checked) {
                    strChecked = 'selected="selected"';
                }
                $(elem).append('<option tag="' + item.tag + '" value="' + item.id + '" ' + strChecked + '>' + item.name + '</option>');
            });
            form.render('select');
        },
        /*HTML元素append*/
        stitchingIsfirst: function (data, elem, defaultValue, isCheckedFirst, emptyText) {
            isCheckedFirst = isCheckedFirst == undefined ? true : isCheckedFirst;
            $.each(data, function (idx, item) {
                var strChecked = '';
                if ((item.id == defaultValue) || (common.isNull(defaultValue) && isCheckedFirst && idx == 0 && !item.checked) || item.checked) {
                    strChecked = 'selected="selected"';
                }
                $(elem).append('<option tag="' + item.tag + '" value="' + item.id + '" ' + strChecked + '>' + item.name + '</option>');
            });
            form.render('select');
        },
        /*设置默认值*/
        setDefault: function (element, defaultval) {
            var options = $(element + " option");
            $.each(options, function (index, item) {
                if (item.value == defaultval) {
                    item.selected = true;
                }
            })
        },
    };
    exports('combobox', combobox);
});
