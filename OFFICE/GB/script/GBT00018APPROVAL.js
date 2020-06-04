// ○一覧用処理
function ListDbClick(obj, LineCnt) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById("hdnListDBclick").value = LineCnt;
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    };
};

/* 備考ボックス表示JavaScript(ダブルクリックされた一覧備考) */
function displayApplyReasonbox(obj) {
    var thisObj = document.getElementById(obj.id);
    var remarkBoxOpenObj = document.getElementById('hdnRemarkboxOpen');
    var submitObj = document.getElementById('hdnSubmit');
    var remarkboxField = document.getElementById('hdnRemarkboxField');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex');
    var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
    var fieldDisplayNameObj = document.getElementById('hdnRemarkboxFieldName');
    /* ダブルクリックした列のヘッダー文言を取得 */
    var parentColIndex = thisObj.parentElement.parentElement.cellIndex; /* 列Index取得 */
    var headerRightTable = document.getElementById("WF_LISTAREA_HR").getElementsByTagName("table")[0]; /* ヘッダー部のテーブルオブジェクト取得 */
    var headerName = headerRightTable.rows[0].cells[parentColIndex].innerText; /* 表示文言取得 */

    var fieldName = "No." + thisObj.getAttribute('rownum') + ':' + headerName;
    /* 表示切替 */
    if (submitObj !== 'FALSE' || remarkBoxOpenObj.value !== 'Open') {
        submitObj.value = 'TRUE';
        currentUnieuqIndexObj.value = thisObj.getAttribute('rownum');
        remarkboxField.value = 'txtWF_LISTAPPROVEDTEXT';
        remarkBoxOpenObj.value = 'Open';
        leftBoxOpen.value = ''; /* 右ボックスとの共存不可 */
        fieldDisplayNameObj.value = fieldName;
        commonDispWait();
        document.forms[0].submit();

    }
}

// 〇一覧ボタンイベントバインド
function bindGridButtonClickEvent() {
    rowHeaderObj = document.getElementById('WF_LISTAREA_DL');
    rowHeaderRObj = document.getElementById('WF_LISTAREA_DR');
    if (rowHeaderObj === null) {
        return; /* レンダリングされていない場合はそのまま終了 */
    }

    if (rowHeaderRObj === null) {
        return; /* レンダリングされていない場合はそのまま終了 */
    }

    var buttonList = rowHeaderObj.querySelectorAll("button[id^='btnWF_LISTAREAOUTPUT']");
    /* 対象のボタンが1件もない場合はそのまま終了 */
    var bindOutputEvent = true;
    if (buttonList === null) {
        bindOutputEvent = false;
    }
    if (buttonList.length === 0) {
        bindOutputEvent = false;
    }
    if (bindOutputEvent) {
        for (let i = 0; i < buttonList.length; i++) {
            var buttonObj = buttonList[i];
            var tdNode = buttonObj.parentNode;
            var trNode = tdNode.parentNode;

            /* クリックイベントに紐づけ */
            buttonObj.onclick = (function (buttonObj) {
                return function () {
                    listButtonClick(buttonObj);
                    return false;
                };
            })(buttonObj);
        }
    }

    var buttonPrintList = rowHeaderRObj.querySelectorAll("button[id^='btnWF_LISTAREAPRINT']");
    /* 対象のボタンが1件もない場合はそのまま終了 */
    var bindPrintEvent = true;
    if (buttonPrintList === null) {
        bindPrintEvent = false;
    }
    if (buttonPrintList.length === 0) {
        bindPrintEvent = false;
    }
    if (bindPrintEvent) {
        for (let i = 0; i < buttonPrintList.length; i++) {
            var buttonPrintObj = buttonPrintList[i];
            var tdNodePrint = buttonPrintObj.parentNode;
            var trNodePrint = tdNodePrint.parentNode;

            /* クリックイベントに紐づけ */
            buttonPrintObj.onclick = (function (buttonPrintObj) {
                return function () {
                    listPrintButtonClick(buttonPrintObj);
                    return false;
                };
            })(buttonPrintObj);
        }
    }

    var buttonUnlockAList = rowHeaderRObj.querySelectorAll("button[id^='btnWF_LISTAREAUNLOCKAPPROVE']");
    /* 対象のボタンが1件もない場合はそのまま終了 */
    if (buttonUnlockAList === null) {
        return;
    }
    if (buttonUnlockAList.length === 0) {
        return;
    }

    for (let i = 0; i < buttonUnlockAList.length; i++) {
        var buttonUnlockAObj = buttonUnlockAList[i];
        var tdNodePrint = buttonUnlockAObj.parentNode;
        var trNodePrint = tdNodePrint.parentNode;

        /* クリックイベントに紐づけ */
        buttonUnlockAObj.onclick = (function (buttonUnlockAObj) {
            return function () {
                listUnlockAButtonClick(buttonUnlockAObj);
                return false;
            };
        })(buttonUnlockAObj);
    }

}

// 〇一覧ボタンクリックイベント
function listButtonClick(obj) {
    var currentRowNum = obj.getAttribute('rownum');
    var objCurrentRowNum = document.getElementById('hdnListCurrentRownum');
    var objButtonClick = document.getElementById('hdnButtonClick');
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE'
        objCurrentRowNum.value = currentRowNum;
        objButtonClick.value = 'btnListOutput';
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    };
    return false;
}

function listPrintButtonClick(obj) {
    var currentRowNum = obj.getAttribute('rownum');
    var objCurrentRowNum = document.getElementById('hdnListCurrentRownum');
    var objButtonClick = document.getElementById('hdnButtonClick');
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE'
        objCurrentRowNum.value = currentRowNum;
        objButtonClick.value = 'btnListPrint';
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    };
    return false;
}

function listUnlockAButtonClick(obj) {
    var currentRowNum = obj.getAttribute('rownum');
    var objCurrentRowNum = document.getElementById('hdnListCurrentRownum');
    var objButtonClick = document.getElementById('hdnButtonClick');
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE'
        objCurrentRowNum.value = currentRowNum;
        objButtonClick.value = 'btnListUnlockApprove';
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    };
    return false;
}

function f_ExcelPrint() {
    // リンク参照
    var printUrlObj = document.getElementById("hdnPrintURL");
    if (printUrlObj === null) {
        return;
    }
    window.open(printUrlObj.value + "?dtm=" + (new Date).getTime(), "view", "_blank");
    printUrlObj.value = '';
}

function f_PDFPrint() {
    var objPrintUrl = document.getElementById("hdnPrintURL");
    if (objPrintUrl === null) {
        return;
    }
    // リンク参照
    window.open(objPrintUrl.value, "view", "_blank");
    objPrintUrl.value = '';
}

///* 出力年月左ボックス表示イベントバインド */
//function bindListPrintMonthTextbox() {
//    /* 表右明細行オブジェクトを取得 */
//    var listLeftData = document.getElementById('WF_LISTAREA_DR');

//    if (listLeftData !== null) {
//        listLeftData.style.display = "none"; // 一旦画面描画を行わせない為左データエリアを非表示
//        /* 右明細オブジェクトのカレンダーイベントを紐づけるテキストボックスを取得 */
//        var dispPrintEventObjcts = listLeftData.querySelectorAll("input[type=text][id^='txtWF_LISTAREAPRINTMONTH']");
//        for (let i = 0; i < dispPrintEventObjcts.length; i++) {
//            var canEventBind = false;
//            var inputObject = dispPrintEventObjcts[i];
//            var inputTargetViewId = 'vLeftPrintMonth';
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
//            }
//        }
//        listLeftData.style.display = ""; // 左データエリアを表示
//    }
//}
// 一覧表の日付項目に左カレンダー選択ボックスを表示するイベント
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
function bindListTextboxEvents() {
    /* 表右明細行オブジェクトを取得 */
    var listRightData = document.getElementById('WF_LISTAREA_DR');
    var fldNames = ['PRINTMONTH'];
    var leftViewNames = ['vLeftPrintMonth'];

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