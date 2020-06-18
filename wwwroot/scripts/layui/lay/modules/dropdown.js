/*通用Dropdown下拉按钮封装 @author Ahri 2018.7.21*/
; layui.define(['jquery'], function (exports) {
    "use strict";
    var that, $ = layui.$, device = layui.device(),
        /*区分移动端和电脑端触发下拉按钮组的方式*/
        event = device.android || device.ios ? 'click' : 'mouseover',
        /*是否允许隐藏*/
        isHideable = function (elem) {
            return elem && elem.className.indexOf('layui-btn-dropdown') == -1 && elem.className.indexOf('layui-dropdown-menu') == -1;
        },
        /*隐藏Dropdown实例*/
        hideDropdwon = function (e) {
            if (isHideable(e.target) && isHideable(e.target.parentElement) && isHideable(e.target.parentElement.parentElement)) { that.hide(); }
        };

    /*(1)构建Dropdown当前实例*/
    var Dropdown = function () {
        this.inst = null;
        this.currReElem = null;
    };
    /*(2)修正Dropdown显示位置*/
    Dropdown.prototype.hide = function () {
        if (that && that.inst && that.inst.is(':visible')) {
            that.inst.css('display', 'none');
            $('body').off(event, hideDropdwon);
        }
    };
    /*(3)渲染Dropdown*/
    Dropdown.prototype.render = function () {
        that = this;
        $('.layui-btn-dropdown').each(function (index, elem) {
            var reElem = $(elem);
            reElem.data('id', 'dropdown-' + index);
            event = (device.android || device.ios) ? 'click' : 'mouseover';

            reElem[event](function () {
                if (!that.inst
                    || that.currReElem.data('id') != reElem.data('id')
                    || (that.currReElem.data('id') == reElem.data('id') && !that.inst.is(':visible'))) {
                    that.hide();
                    var dropElem = reElem.find('.layui-dropdown-menu'),
                        left = reElem.offset().left - $(window).scrollLeft(),
                        top = reElem.offset().top + reElem.height() - $(window).scrollTop() - 2,
                        containerWidth = reElem.width(),
                        dropWidth = dropElem.width(),
                        offsetRight = left + containerWidth,
                        overflow = (left + dropWidth) > $(window).width(),
                        strCss = { 'display': 'block', 'position': 'fixed', 'top': top + 'px', 'left': left + 'px' };

                    overflow && $.extend(true, strCss, { 'left': (offsetRight - dropWidth) + 'px' });
                    dropElem.css(strCss).on('click', 'li', function () {
                        dropElem.css('display', 'none');
                    });
                    that.inst = dropElem;
                    that.currReElem = reElem;
                    $('body').on(event, hideDropdwon);
                }
            });
        });
    };

    /*实例化Dropdown，并自动渲染*/
    var dropdown = new Dropdown();
    dropdown.render();
    $(window).scroll(function () {
        dropdown.hide();
    });
    exports('dropdown', dropdown);
});