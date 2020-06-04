/* 通貨コード左ボックス表示イベントバインド */
function bindListTextboxEvents() {
    /* 表右明細行オブジェクトを取得 */
    var listRightData = document.getElementById('WF_LISTAREA_DR');
    var fldNames = ['ACCCURRENCYSEGMENT', 'BOTHCLASS', 'INCTORICODE', 'EXPTORICODE', 'DEPOSITDAY','OVERDRAWDAY', 'HOLIDAYFLG'];
    var leftViewNames = ['vLeftAccCurrencySegment', 'vLeftBothClass', 'vLeftToriCode', 'vLeftToriCode', 'vLeftPayDay','vLeftPayDay', 'vLeftHolidayFlg'];

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