
// 一覧表の入力フィールドダブルクリック時左ボックスを表示するイベント
function listTextFieldDBclick(fieldId, viewId) {
    var submitObj = document.getElementById('hdnSubmit');
    var dblClickObject = document.getElementById('hdnTextDbClickField');
    var viewIdObject = document.getElementById('hdnLeftboxActiveViewId');
    var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
    var currentRownum = document.getElementById('hdnListCurrentRownum');
    var txtObj = document.getElementById(fieldId);
    if (submitObj === null || dblClickObject === null || viewIdObject === null || leftBoxOpen === null) {
        return;
    }

    // サブミットフラグが立っていない場合のみ実行
    if (submitObj.value === 'FALSE') {
        submitObj.value = 'TRUE';
        dblClickObject.value = fieldId;
        viewIdObject.value = viewId;
        leftBoxOpen.value = "Open";
        currentRownum.value = txtObj.getAttribute("rownum");
        commonDispWait();
        document.forms[0].submit();
    }
}   
function bindPrevScroll(listObjId) {
    var listObj = document.getElementById(listObjId);
    // そもそもリストがレンダリングされていなければ終了
    if (listObj === null) {
        return;
    }
    // Mouseホイールイベントのバインド
    var mousewheelevent = 'onwheel' in listObj ? 'wheel' : 'onmousewheel' in listObj ? 'mousewheel' : 'DOMMouseScroll';
    listObj.addEventListener(mousewheelevent, (function () {
        var objSubmit = document.getElementById("hdnSubmit");
        var objMouseWheel = document.getElementById("hdnMouseWheel");
        if (objSubmit.value === "FALSE") {
            if (window.event.wheelDelta < 0) {
                objMouseWheel.value = "+";
            } else {
                // リストの現在見えている位置が最上部の場合はポストバックせず終了
                var listPosition = document.getElementById("hdnListPosition");
                if (listPosition !== null) {
                    if (listPosition.value === '' || listPosition.value === '1') {
                        return false;
                    }
                }
                objMouseWheel.value = "-";

            }

            var activeElm = document.activeElement;
            if (activeElm !== null) {
                if (activeElm.id.indexOf('txtWF_LISTAREA') === 0) {
                    if (activeElm.onblur !== null) {
                        activeElm.onblur();
                    }
                }
            }
            objSubmit.value = "TRUE";
            commonDispWait();
            document.forms[0].submit();                            //aspx起動
        } else {
            return false;
        }

    }), true);

    // 画面キーダウンイベントのバインド
    // GridView処理（矢印処理）
    document.addEventListener('keydown', (function () {
        return function () {
            var objSubmit = document.getElementById('hdnSubmit');
            var objMouseWheel = document.getElementById('hdnMouseWheel');
            // ↑キー押下時
            if (window.event.keyCode === 38) {
                if (objSubmit.value === 'FALSE') {
                    // リストの現在見えている位置が最上部の場合はポストバックせず終了
                    var listPosition = document.getElementById("hdnListPosition");
                    if (listPosition !== null) {
                        if (listPosition.value === '' || listPosition.value === '1') {
                            return false;
                        }
                    }
                    objMouseWheel.value = '-';
                    var activeElm = document.activeElement;
                    if (activeElm !== null) {
                        if (activeElm.id.indexOf('txtWF_LISTAREA') === 0) {
                            if (activeElm.onblur !== null) {
                                activeElm.onblur();
                            }
                        }
                    }
                    objSubmit.value = "TRUE";
                    commonDispWait();
                    document.forms[0].submit();  //aspx起動
                    return false;
                };
            };
            // ↓キー押下時
            if (window.event.keyCode === 40) {
                if (objSubmit.value === 'FALSE') {
                    objMouseWheel.value = '+';
                    var activeElm = document.activeElement;
                    if (activeElm !== null) {
                        if (activeElm.id.indexOf('txtWF_LISTAREA') === 0) {
                            if (activeElm.onblur !== null) {
                                activeElm.onblur();
                            }

                        }
                    }
                    objSubmit.value = "TRUE";
                    commonDispWait();
                    document.forms[0].submit();  //aspx起動
                    return false;
                };
            };
        };
    })(), false);

}

// 一覧表の日付項目にイベントバインド
//function bindListDateTextbox() {
//    /* 表右明細行オブジェクトを取得 */
//    var listRightData = document.getElementById('WF_LISTAREA_DR');


//    if (listRightData !== null) {
//        listRightData.style.display = "none"; // 一旦画面描画を行わせない為左データエリアを非表示
//        /* 右明細オブジェクトのカレンダーイベントを紐づけるテキストボックスを取得 */
//        var dispCalEventObjcts = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREADPIN']," +
//            "input[type=text][id^='txtWF_LISTAREAETYD'], input[type=text][id^='txtWF_LISTAREADOUT'], input[type=text][id^='txtWF_LISTAREACYIN']");
//        for (let i = 0; i < dispCalEventObjcts.length; i++) {
//            var canEventBind = false;
//            var inputObject = dispCalEventObjcts[i];
//            var inputTargetViewId = 'vLeftCal';
//            inputObject.autocomplete = 'off'; /* オートコンプリートをOFF */
//            inputObject.placeholder = '';
//            if (inputObject.disabled !== null) {
//                if (inputObject.disabled !== 'disabled' && inputObject.disabled !== 'true' && inputObject.disabled !== true) {
//                    inputObject.placeholder = 'DoubleClick to select';
//                    canEventBind = true;
//                }
//            }
//            if (canEventBind === true) {
//                // ダブルクリックイベントバインド
//                // フォーカスを合わさないとテキストボックスはダブルクリックに反応しないため、
//                // 前後をspanタグで括りそちらにダブルクリックイベントを記載
//                var wrapper = document.createElement('span');
//                wrapper.appendChild(inputObject.cloneNode(true));
//                inputObject.parentNode.replaceChild(wrapper, inputObject);
//                let inputObjectId = inputObject.id;
//                wrapper.ondblclick = (function (inputObjectId, inputTargetViewId) {
//                    return function () {
//                        listTextFieldDBclick(inputObjectId, inputTargetViewId);

//                    };
//                })(inputObjectId, inputTargetViewId);

//                // 手入力イベントバインド
//                // フォーカス
//                inputObject = document.getElementById(inputObjectId);
//                inputObject.onfocus = (function (inputObject) {
//                    return function () {
//                        var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
//                        if (hdnObj !== null) {
//                            hdnObj.value = '';
//                        }
//                        inputObject.dataset.prev = inputObject.value; // 変更したフィールドのIDを記録
//                    };
//                })(inputObject);
//                // フォーカス喪失後
//                var onchangeItemId = document.getElementById('hdnOnchangeField');
//                inputObject.onblur = (function (inputObject, onchangeItemId) {
//                    return function () {
//                        var submitObj = document.getElementById('hdnSubmit');
//                        var footerMessageObj = document.getElementById('lblFooterMessage');
//                        if (footerMessageObj !== null) {
//                            footerMessageObj.innerText = '';
//                        }
//                        if (inputObject.dataset.prev !== inputObject.value) {
//                            if (submitObj.value === 'FALSE') {
//                                submitObj.value = 'TRUE';
//                                onchangeItemId.value = inputObject.id; // 変更したフィールドのIDを記録
//                                var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
//                                if (activeElem !== null || document.activeElement.id !== null) {
//                                    activeElem.value = document.activeElement.id;
//                                }
//                                inputObject.dataset.prev = '';
//                                let hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
//                                if (hdnObj !== null) {
//                                    let clickedButtonId = document.getElementById('hdnButtonClick');
//                                    clickedButtonId.value = hdnObj.value;
//                                    hdnObj.value = '';
//                                }
//                                let currentRownum = document.getElementById('hdnListCurrentRownum');
//                                currentRownum.value = inputObject.getAttribute("rownum");
//                                commonDispWait();
//                                document.forms[0].submit();  // サブミット
//                            } else {
//                                // ボタンクリックでフォーカスが移された場合を考慮
//                                let hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
//                                if (hdnObj !== null) {
//                                    if (hdnObj.value !== '') {
//                                        let clickedButtonId = document.getElementById('hdnButtonClick');
//                                        clickedButtonId.value = hdnObj.value;
//                                        hdnObj.value = '';
//                                        if (submitObj.value === 'FALSE') {
//                                            submitObj.value = 'TRUE';
//                                            let currentRownum = document.getElementById('hdnListCurrentRownum');
//                                            currentRownum.value = inputObject.getAttribute("rownum");
//                                            commonDispWait();
//                                            document.forms[0].submit();  // サブミット
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    };

//                })(inputObject, onchangeItemId);
//            }

//        }
//        listRightData.style.display = ""; // 左データエリアを表示
//    }

//}
/* 左ボックス表示イベントバインド */
function bindListTextboxEvents() {
    /* 表右明細行オブジェクトを取得 */
    var listRightData = document.getElementById('WF_LISTAREA_DR');
    var fldNames = ['DPIN', 'ETYD', 'DOUT', 'CYIN', 'CHECK_DPIN', 'CHECK_ETYD', 'CHECK_DOUT', 'CHECK_CYIN'];
    var leftViewNames = ['vLeftCal', 'vLeftCal', 'vLeftCal', 'vLeftCal', 'vLeftCheck', 'vLeftCheck', 'vLeftCheck', 'vLeftCheck'];

    if (listRightData !== null) {
        for (let fildIdx = 0; fildIdx < fldNames.length; fildIdx++) {
            var fieldName = fldNames[fildIdx];
            listRightData.style.display = "none"; // 一旦画面描画を行わせない為左データエリアを非表示
            /* 右明細オブジェクトのカレンダーイベントを紐づけるテキストボックスを取得 */
            var dispCalEventObjcts = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREA" + fieldName + "']," +
                "input[type=text][id^='txtWF_LISTAREA" + fieldName + "']");
            for (let i = 0; i < dispCalEventObjcts.length; i++) {
                var canEventBind = false;
                var inputObject = dispCalEventObjcts[i];
                var inputTargetViewId = leftViewNames[fildIdx];
                inputObject.autocomplete = 'off'; /* オートコンプリートをOFF */
                inputObject.placeholder = '';
                if (inputObject.disabled !== null) {
                    if (inputObject.disabled !== 'disabled' && inputObject.disabled !== 'true' && inputObject.disabled !== true) {
                        inputObject.placeholder = 'DoubleClick to select';
                        canEventBind = true;
                    }
                }
                if (canEventBind === true) {
                    // ダブルクリックイベントバインド
                    // フォーカスを合わさないとテキストボックスはダブルクリックに反応しないため、
                    // 前後をspanタグで括りそちらにダブルクリックイベントを記載
                    var wrapper = document.createElement('span');
                    wrapper.appendChild(inputObject.cloneNode(true));
                    inputObject.parentNode.replaceChild(wrapper, inputObject);
                    let inputObjectId = inputObject.id;
                    wrapper.ondblclick = (function (inputObjectId, inputTargetViewId) {
                        return function () {
                            listTextFieldDBclick(inputObjectId, inputTargetViewId);

                        };
                    })(inputObjectId, inputTargetViewId);

                    // 手入力イベントバインド
                    // フォーカス
                    inputObject = document.getElementById(inputObjectId);
                    inputObject.onfocus = (function (inputObject) {
                        return function () {
                            var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                            if (hdnObj !== null) {
                                hdnObj.value = '';
                            }
                            inputObject.dataset.prev = inputObject.value; // 変更したフィールドのIDを記録
                        };
                    })(inputObject);
                    // フォーカス喪失後
                    var onchangeItemId = document.getElementById('hdnOnchangeField');
                    inputObject.onblur = (function (inputObject, onchangeItemId) {
                        return function () {
                            var submitObj = document.getElementById('hdnSubmit');
                            var footerMessageObj = document.getElementById('lblFooterMessage');
                            if (footerMessageObj !== null) {
                                footerMessageObj.innerText = '';
                            }
                            if (inputObject.dataset.prev !== inputObject.value) {
                                if (submitObj.value === 'FALSE') {
                                    submitObj.value = 'TRUE';
                                    onchangeItemId.value = inputObject.id; // 変更したフィールドのIDを記録
                                    var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
                                    if (activeElem !== null || document.activeElement.id !== null) {
                                        activeElem.value = document.activeElement.id;
                                    }
                                    inputObject.dataset.prev = '';
                                    let hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                                    if (hdnObj !== null) {
                                        let clickedButtonId = document.getElementById('hdnButtonClick');
                                        clickedButtonId.value = hdnObj.value;
                                        hdnObj.value = '';
                                    }
                                    let currentRownum = document.getElementById('hdnListCurrentRownum');
                                    currentRownum.value = inputObject.getAttribute("rownum");
                                    commonDispWait();
                                    document.forms[0].submit();  // サブミット
                                } else {
                                    // ボタンクリックでフォーカスが移された場合を考慮
                                    let hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                                    if (hdnObj !== null) {
                                        if (hdnObj.value !== '') {
                                            let clickedButtonId = document.getElementById('hdnButtonClick');
                                            clickedButtonId.value = hdnObj.value;
                                            hdnObj.value = '';
                                            if (submitObj.value === 'FALSE') {
                                                submitObj.value = 'TRUE';
                                                let currentRownum = document.getElementById('hdnListCurrentRownum');
                                                currentRownum.value = inputObject.getAttribute("rownum");
                                                commonDispWait();
                                                document.forms[0].submit();  // サブミット
                                            }
                                        }
                                    }
                                }
                            }
                        };

                    })(inputObject, onchangeItemId);
                }

            }
            listRightData.style.display = ""; // 左データエリアを表示
        }
    }

}

/* 一覧表の表示制御 */
function gridDispControl(delButtonName) {
    /* ボタンオブジェクト名称設定 */
    var listLeftData = document.getElementById('WF_LISTAREA_DL');
    /* 右明細テキスト */
    var listRightData = document.getElementById('WF_LISTAREA_DR');
    if (listRightData !== null) {

    }
}

// 一覧表添付ファイルセルダブルクリック時イベント
function showAttachmentArea(obj, lineCnt, fieldName) {
    var currentRowNum = obj.getAttribute('rownum');
    var objCurrentRowNum = document.getElementById('hdnListCurrentRownum');
    var objButtonClick = document.getElementById('hdnButtonClick');
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE'
        objCurrentRowNum.value = lineCnt;
        objButtonClick.value = 'ShowAttachmentArea';
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    };
    return false;
}
// 添付ファイル一覧、添付ファイル名ダブルクリック時
function dispAttachmentFile(filename) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById('hdnFileDisplay').value = filename;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
}
