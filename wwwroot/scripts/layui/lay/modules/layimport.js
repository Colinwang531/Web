/*通用import封装 */
; layui.define(['jquery', 'layer', 'common'], function (exports) {
    "use strict";
    var $ = layui.$, layer = layui.layer, common = layui.common;
    var layimport = {
        /*
         * remark 导入Excel
         * @params String title        标题(必传，如果没有，传空值)
         * @params String filepath        下载excel模版地址(如果有下载模版就传没有就传空值)
         * @params String controllerUrl     导入数据路由地址
         * @params Function successCallBack   回调方法
         */
        excel: function (title, filepath, controllerUrl, callback) {
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">' + title + '</b>'
                , type: 2
                , shade: 0.8
                , area: ['650px', '360px']
                , content: ['/Import/Import?excelurl=' + filepath + '&controllerUrl=' + controllerUrl + '&callback=' + callback, 'no']
            });
        }
    };
    exports("layimport", layimport);
});
