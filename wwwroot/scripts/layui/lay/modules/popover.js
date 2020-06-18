/*通用popup/popover封装 @author Ahri 2018.7.23 */
; layui.define(['jquery', 'layer', 'common'], function (exports) {
    "use strict";
    var $ = layui.$, layer = layui.layer, common = layui.common;
    var popover = {
        /**
         * 选择教研组-教师pop窗
         * @author Ahri 2018.7.26
         * @param {Object}   parent              window 对象（必传）
         * @param {Boolean}  isSingle            是否单选（必传）
         * @param {String}   checkedValue        原始选中的Id值(必传，如果没有，传空值)
         * @param {Boolean}  isTeachClass        是否过滤任教设置(可选参数)
         * @param {String}   courseExampleId     排课方案id(isTeachClass为true时有效)
         * @param {String}   gcStr               年级ID（Sys_Grade）+课程信息的ID（Sys_Course）的组合，用逗号隔开，如(xx,xx)
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name，如果isSingle为false，返回的是json数组(必传)
         */
        teachers: function (parent, isSingle, checkedValue, isTeachClass, courseExampleId, gcStr, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            if (isSingle) {
                /*单选*/
                layer.open({
                    id: 'POP_TEACHERS_' + common.getRandom()
                    , title: '<b class="fs-18">设置教师</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['850px', '570px']
                    , content: ['/Popover/SingleGroupTeacherSelector?checkedValue=' + checkedValue + '&isTeachClass=' + isTeachClass + '&courseExampleId=' + courseExampleId + '&gcStr=' + gcStr, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.__SINGLEGROUPTEACHERSELECTOR.lstTeacher;
                        if (typeof (callback) !== "undefined") {
                            callback(((data != null) ? data[0] : { "id": '', "name": '' }));
                        }
                        layer.close(index);
                    }
                });
            }
            else {
                /*多选*/
            }
        },
        /**
         * 选择教师pop窗
         * @author Ahri 2018.8.7
         * @param {Object}   parent                           window 对象（必传）
         * @param {Boolean}  isFilterCheckedTeacher           是否过滤掉已选择的教师（这是后台代码过滤）
         * @param {Boolean}  isSingle                         是否单选（必传，false多选，true单选）
         * @param {Number}   tableType                        过滤表枚举类型（1：None，不过滤；2：TecherGroup，过滤教研组成员；3：User，过滤用户信息表）
         * @param {String}   teacherGroupId                   教研组ID，当tableType为2的时候，传此参数，其他类型传递过来也不起作用
         * @param {String}   checkedValue                     已选中的老师
         * @param {Function} successCallBack                  成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        workers: function (parent, isFilterCheckedTeacher, isSingle, tableType, teacherGroupId, checkedValue, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            if (isSingle) {
                /*单选*/
                layer.open({
                    id: 'POP_WORKERS_' + common.getRandom()
                    , title: '<b class="fs-18">选择教师</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['850px', '570px']
                    , content: ['/Popover/SingleWorkersSelector?isFilterCheckedTeacher=' + isFilterCheckedTeacher + '&tableType=' + tableType + '&teacherGroupId=' + teacherGroupId + "&checkedValue=" + checkedValue, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.__SINGLEWORKERSSELECTOR.lstTeacher;
                        //alert(typeof (callback) !== "undefined");
                        if (data != null && typeof (callback) !== "undefined") {
                            console.log("---");
                            console.log(data[0]);
                            callback(data[0]);
                        }
                        layer.close(index);
                    }
                });
            }
            else {
                /*多选*/
                layer.open({
                    id: 'POP_WORKERS_' + common.getRandom()
                    , title: '<b class="fs-18">选择教师</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['850px', '570px']
                    , content: ['/Popover/DoubleWorkersSelector?isFilterCheckedTeacher=' + isFilterCheckedTeacher + '&tableType=' + tableType + '&teacherGroupId=' + teacherGroupId, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.__DOUBLEWORKERSSELECTOR.lstTeacher();

                        if (typeof (callback) !== "undefined") {
                            callback(data);
                        }
                        layer.close(index);
                    }
                });
            }
        },
        /**
         * 选择用户信息POP窗
         * @param {Object}    parent window 对象（必传）
         * @param {String}    roleType 角色类型
         * @param {Function}  callback 成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        users: function (parent, roleType, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_USER_' + common.getRandom()
                , title: '<b class="fs-18">选择用户</b>'
                , type: 2
                , closeBtn: 1
                , area: ['850px', '570px']
                , content: ['/Popover/DoubleUsersSelector?roleType=' + roleType, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.__DOUBLEUSERSSELECTOR.lstUser();

                    if (typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
            });
        },
        /**
         * 建筑楼栋教室pop窗
         * @author Ahri 2018.7.27
         * @param  {String}   courseExampleId     排课方案id，如果不传则不过滤分配为教室的场所
         * @param  {Function} successCallBack     成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        places: function (courseExampleId, callback) {
            parent.layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">设置教室</b>'
                , type: 2
                , closeBtn: 1
                , area: ['850px', '570px']
                , content: ['/Popover/DoublePlacesSelector?ceid=' + courseExampleId, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.K.lstPlace;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    parent.layer.close(index);
                }
            });
        },
        /**
         * 建筑楼栋教室pop窗 单选
         * @param {Function} successCallBack   成功回调,回调参数data，包含了id和name(必传)
         */
        /**
         * 选择场所POP窗
         * @param {Object}    parent       window 对象（必传）
         * @param {Number}    filter       一个表示过滤已选场所的枚举（见：TreeResult 中的 FilterRoomEnum）
         * @param {String}    checkedValue 已选中的场所（选传，没有的时候，传空）
         * @param {Function}  callback     成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        singlePlaces: function (parent, filter, checkedValue, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">设置教室</b>'
                , type: 2
                , closeBtn: 1
                , area: ['850px', '570px']
                , content: ['/Popover/SinglePlacesSelector?filter=' + filter + "&checkedValue=" + checkedValue, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.K.lstPlace;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
            });
        },
        /**
         * 选择学生POP窗
         * @param {Object}    parent       window 对象（必传）
         * @param {Number}    filter       一个表示过滤已选场所的枚举（见：TreeResult 中的 FilterRoomEnum）
         * @param {String}    checkedValue 已选中的班级（选传，没有的时候，传空）
         * @param {Function}  callback     成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         * @param {String}  studentsScope  学生ID范围，（不过滤可 null,或者不传）
         */
        students: function (parent, isSingle, gradeId, checkedValue, callback, studentsScope) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            // parent.studentsScope = (studentsScope == undefined || studentsScope == null) ? null : studentsScope; 
            parent.studentsScope = studentsScope;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            if (isSingle) {
                /*单选*/
                layer.open({
                    id: 'POP_STUDENT' + common.getRandom()
                    , title: '<b class="fs-18">选择学生</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['950px', '570px']
                    , content: ['/Popover/SingleStudentSelector?gradeId=' + gradeId + '&checkedValue=' + checkedValue, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.K.lstStudent;
                        if (data != null && typeof (callback) !== "undefined") {
                            callback(data[0]);
                        }
                        layer.close(index);
                    }
                });
            }
            else {
                /*多选*/
                layer.open({
                    id: 'POP_STUDENT' + common.getRandom()
                    , title: '<b class="fs-18">选择学生</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['950px', '570px']
                    , content: ['/Popover/DoubleStudentSelector?gradeId=' + gradeId + '&checkedValue=' + checkedValue, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.K.lstStudent;
                        if (data != null && typeof (callback) !== "undefined") {
                            callback(data);
                        }
                        layer.close(index);
                    }
                });
            }
        },

        /**
         * 指定机构选择学生POP窗
         * @param {Object}    parent       window 对象（必传）
         * @param {Number}    filter       一个表示过滤已选场所的枚举（见：TreeResult 中的 FilterRoomEnum）
         * @param {String}    checkedValue 已选中的班级（选传，没有的时候，传空）
         * @param {Function}  callback     成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         * @param {String}  studentsScope  学生ID范围，（不过滤可 null,或者不传）
         */
        orgStudents: function (parent, isSingle, orgId, gradeId, checkedValue, callback, studentsScope) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            // parent.studentsScope = (studentsScope == undefined || studentsScope == null) ? null : studentsScope; 
            parent.studentsScope = studentsScope;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            if (isSingle) {
                /*单选*/
                layer.open({
                    id: 'POP_STUDENT' + common.getRandom()
                    , title: '<b class="fs-18">选择学生</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['950px', '570px']
                    , content: ['/Popover/SingleStudentSelector?orgId=' + orgId + '&gradeId=' + gradeId + '&checkedValue=' + checkedValue, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.K.lstStudent;
                        if (data != null && typeof (callback) !== "undefined") {
                            callback(data[0]);
                        }
                        layer.close(index);
                    }
                });
            }
            else {
                /*多选*/
                layer.open({
                    id: 'POP_STUDENT' + common.getRandom()
                    , title: '<b class="fs-18">选择学生</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['950px', '570px']
                    , content: ['/Popover/DoubleStudentSelector?orgId=' + orgId + '&gradeId=' + gradeId + '&checkedValue=' + checkedValue, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.K.lstStudent;
                        if (data != null && typeof (callback) !== "undefined") {
                            callback(data);
                        }
                        layer.close(index);
                    }
                });
            }
        },
        /**
         * 选择分区班级信息POP窗
         * @param {Object}    parent       window 对象（必传）
         * @param {Boolean}   isSingle     是否单选（必传，false多选，true单选）
         * @param {String}    checkedValue 已选中的班级（选传，没有的时候，传空）
         * @param {Function}  callback     成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        class: function (parent, isSingle, checkedValue, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            if (isSingle) {
                layer.open({
                    id: 'POP_USER_' + common.getRandom()
                    , title: '<b class="fs-18">选择班级</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['850px', '570px']
                    , content: ['/Popover/SingleClassSelector?checkedValue=' + checkedValue, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.K.lstClass;
                        if (data != null && typeof (callback) !== "undefined") {
                            callback(data[0]);
                        }
                        layer.close(index);
                    }
                });
            } else {
                //多选
                layer.open({
                    id: 'POP_USER_' + common.getRandom()
                    , title: '<b class="fs-18">选择班级</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['850px', '570px']
                    , content: ['/Popover/DoubleClassSelector?checkedValue=' + checkedValue, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                            , data = iframeWin.K.lstClass;
                        if (data != null && typeof (callback) !== "undefined") {
                            callback(data);
                        }
                        layer.close(index);
                    }
                });
            }
        },
        /**
         * 校本选课创建活动选择节次模版pop窗
         * @param {Function} successCallBack   成功回调,回调参数data，包含了id和name(必传)
        /**
         * 选择场所POP窗
         * @param {Object}    parent       window 对象（必传）
         * @param {Function}  callback     成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        ChooseSchedulePop: function (parent, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">选择作息模版</b>'
                , type: 2
                , closeBtn: 1
                , area: ['900px', '570px']
                , content: ['/Popover/ChooseSchedulePop', 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.K.lstSchedule;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
            });
        },
        /**
         * 校本选课创建活动选择上课节次pop窗
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name(必传)
        /**
         * 选择场所POP窗
         * @param {Object}    parent             window 对象（必传）
         * @param {String}    scheduleId         作息模版ID（必传）
         * @param {String}    selecteData       已经选择了的对象（必传）
         * @param {Function}  callback           成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        ChooseScheduleItemPop: function (parent, scheduleId, selecteData, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">选择上课节次</b>'
                , type: 2
                , closeBtn: 1
                , area: ['900px', '640px']
                , content: ['/Popover/ChooseScheduleItemPop?scheduleId=' + scheduleId, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.cache.point;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
                , success: function (layero, index) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]; //得到iframe页的窗口对象，执行iframe页的方法：iframeWin.method();
                    iframeWin.cache.point = selecteData;
                }
            });
        },
        /**
         * 校本选课创建活动选择课程pop窗
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name(必传)
        /**
         * 选择场所POP窗
         * @param {Object}    parent             window 对象（必传）
         * @param {String}    operate            操作的对象 是分区选课 还是 活动添加课程
         * @param {Object}    selecteData        已选数据
         * @param {Function}  callback           成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        ChooseCoursePop: function (parent, operate, selecteData, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">选择课程</b>'
                , type: 2
                , closeBtn: 1
                , area: ['960px', '570px']
                , content: ['/Popover/ChooseCoursePop?operate=' + operate, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.cache.course;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
                , success: function (layero, index) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]; //得到iframe页的窗口对象，执行iframe页的方法：iframeWin.method();
                    iframeWin.cache.course = selecteData;
                }
            });
        },
        /**
         * 校本选课创建活动选择学生pop窗
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name(必传)
        /**
         * 选择场所POP窗
         * @param {Object}    parent             window 对象（必传）
         * @param {Function}  callback           成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        ChooseStudentPop: function (parent, selecteData, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">选择学员</b>'
                , type: 2
                , closeBtn: 1
                , area: ['960px', '570px']
                , content: ['/Popover/ChooseStudentPop', 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.cache.student;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
                , success: function (layero, index) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]; //得到iframe页的窗口对象，执行iframe页的方法：iframeWin.method();
                    iframeWin.cache.student = selecteData;
                }
            });
        },
        /**
         * 校本选课 设置课程规则pop窗
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name(必传)
        /**
         * @param {Object}    parent             window 对象（必传）
         * @param {String}    SchoolCourseGroupId         活动ID（必传）
         * @param {Object}    selecteData        已选数据（必传）
         * @param {Function}  callback           成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        SettingCourseRulePop: function (parent, schoolCourseGroupId, selecteData, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">选择课程</b>'
                , type: 2
                , closeBtn: 1
                , area: ['70%', '640px']
                , content: ['/Popover/SettingCourseRulePop?schoolCourseGroupId=' + schoolCourseGroupId, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.cache.course;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
                , success: function (layero, index) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]; //得到iframe页的窗口对象，执行iframe页的方法：iframeWin.method();
                    iframeWin.cache.course = selecteData;
                }
            });
        },
        /**
         * 校本选课 设置课程规则 选择老师 pop窗
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name(必传)
        /**
         * @param {Object}    parent             window 对象（必传）
         * @param {String}    checkedValue       选中id（必传）
         * @param {Function}  callback           成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        SettingTearcherPop: function (parent, checkedValue, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">选择老师</b>'
                , type: 2
                , closeBtn: 1
                , area: ['850px', '570px']
                , content: ['/Popover/SettingTearcherPop?checkedValue=' + checkedValue, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.K.lstTeacher;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
            });
        },
        /**
         * 校本选课 区域 选择学生 pop窗
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name(必传)
        /**
         * @param {Object}    parent             window 对象（必传）
         * @param {String}    selecteData       选中id（必传）
         * @param {Function}  callback           成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        AreaStudentPop: function (parent, schoolCourseGroupId, selStudentData, areaId, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">选择学生</b>'
                , type: 2
                , closeBtn: 1
                , area: ['950px', '600px']
                , content: ['/Popover/AreaStudentPop?schoolCourseGroupId=' + schoolCourseGroupId + '&areaId=' + areaId, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.cache.student
                        , classData = iframeWin.cache.classArr;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data, classData);
                    }
                    layer.close(index);
                }
                , success: function (layero, index) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]; //得到iframe页的窗口对象，执行iframe页的方法：iframeWin.method();
                    iframeWin.cache.student = selStudentData;
                    //iframeWin.cache.classArr = selClassData;
                }
            });
        },
        /**
         * 考勤插入学生选择学生 pop窗
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name(必传)
        /**
         * @param {Object}    parent             window 对象（必传）
         * @param {String}    selecteData        选中id（必传）
         * @param {Function}  callback           成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        ClassAttStudentPop: function (parent, classId, ceid, weekid, pointid, courseid, teacherid, placeid, callback) {

            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            //string classId, string ceid, int weekid, int pointid, string courseid
            layer.open({
                id: 'POP_STUDENT' + common.getRandom()
                , title: '<b class="fs-18">选择学生</b>'
                , type: 2
                , closeBtn: 1
                , area: ['650px', '570px']
                , content: ['/Popover/SingleDcClassStudent?classId=' + classId + '&ceid=' + ceid + '&weekid=' + weekid + '&pointid=' + pointid + '&courseid=' + courseid + '&teacherid=' + teacherid + '&placeid=' + placeid, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.K.lstStudent;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data[0]);
                    }
                    layer.close(index);
                }
            });

        },
        /**
         * 校本选课创建活动选择课程pop窗
         * @param {Function} successCallBack     成功回调,回调参数data，包含了id和name(必传)
        /**
         * 选择场所POP窗
         * @param {Object}    parent             window 对象（必传）
         * @param {Object}    selecteData        已选数据
         * @param {Function}  callback           成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
         */
        ChooseAreaCoursePop: function (parent, schoolCourseGroupId, selecteData, callback) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            layer.open({
                id: 'POP_' + common.getRandom()
                , title: '<b class="fs-18">选择课程</b>'
                , type: 2
                , closeBtn: 1
                , area: ['900px', '570px']
                , content: ['/Popover/ChooseAreaCoursePop?schoolCourseGroupId=' + schoolCourseGroupId, 'no']
                , btn: ["确定", "取消"]
                , btnAlign: 'c'
                , yes: function (index, layero) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]
                        , data = iframeWin.cache.course;
                    if (data != null && typeof (callback) !== "undefined") {
                        callback(data);
                    }
                    layer.close(index);
                }
                , success: function (layero, index) {
                    var iframeWin = parent.window[layero.find('iframe')[0]['name']]; //得到iframe页的窗口对象，执行iframe页的方法：iframeWin.method();
                    iframeWin.cache.course = selecteData;
                }
            });
        },
        /**
        * 选择学生POP窗  班级课程
        * @param {Object}    parent       window 对象（必传）
        * @param {Number}    filter       一个表示过滤已选场所的枚举（见：TreeResult 中的 FilterRoomEnum）
        * @param {String}    checkedValue 已选中的班级（选传，没有的时候，传空）
        * @param {Function}  callback     成功回调,回调参数data，包含了id和name，返回的是json数组(必传)
        * @param {String}  classId     班级Id 根据班级Id查找课程
        * @param {String}  studentsScope  学生ID范围，（不过滤可 null,或者不传）
        */
        studentsAndCourse: function (parent, isSingle, gradeId, checkedValue, callback, classId, courseExampleId, studentsScope) {
            parent = (parent == undefined || parent == null) ? window.self : parent;
            // parent.studentsScope = (studentsScope == undefined || studentsScope == null) ? null : studentsScope; 
            parent.studentsScope = studentsScope;
            layer = parent.layer === undefined ? layui.layer : parent.layer;
            if (isSingle) {
                /*单选*/
                layer.open({
                    id: 'POP_STUDENT' + common.getRandom()
                    , title: '<b class="fs-18">选择学生</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['950px', '570px']
                    , content: ['/Popover/SingleStudentAndCourseSelector?gradeId=' + gradeId + '&checkedValue=' + checkedValue + '&classId=' + classId + '&courseExampleId=' + courseExampleId, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']], data = { Students: iframeWin.K.lstStudent, CourseIds: iframeWin.K.course };
                        if (data != null && typeof (callback) !== "undefined") {
                            callback(data);
                        }
                        layer.close(index);
                    }
                });
            }
            else {
                /*多选*/
                layer.open({
                    id: 'POP_STUDENT' + common.getRandom()
                    , title: '<b class="fs-18">选择学生</b>'
                    , type: 2
                    , closeBtn: 1
                    , area: ['950px', '570px']
                    , content: ['/Popover/DoubleStudentAndCourseSelector?gradeId=' + gradeId + '&checkedValue=' + checkedValue + '&classId=' + classId + '&courseExampleId=' + courseExampleId, 'no']
                    , btn: ["确定", "取消"]
                    , btnAlign: 'c'
                    , yes: function (index, layero) {
                        var iframeWin = parent.window[layero.find('iframe')[0]['name']], data = { Students: iframeWin.K.lstStudent, CourseIds: iframeWin.K.course };
                        if (data != null && typeof (callback) !== "undefined") {
                            callback(data);
                        }
                        layer.close(index);
                    }
                });
            }
        }
    };
    exports("popover", popover);
});
