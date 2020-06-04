window.addEventListener('load', function () {
    var listArea = document.getElementById('WF_LISTAREA');
    if (listArea === null) {
        return;
    }
    
    var chkItemList = listArea.querySelectorAll("input[type='checkbox'][disabled]:not([id$='Clone'])");
    if (chkItemList !== null) {
        for (let i = 0; i < chkItemList.length; i++) {
            var chkItem = chkItemList[i];
            var clChk = chkItem.cloneNode(true);
            clChk.id = clChk.id + "Clone";
            clChk.name = clChk.name + "Clone";
            chkItem.parentNode.appendChild(clChk);
            chkItem.disabled = false;
            chkItem.style.display = "none";
        }
    }
    if (document.forms[0].dataset.disabled === '1') {
        var dataAreaObj = listArea.querySelectorAll("input[type='checkbox']");
        if (dataAreaObj !== null) {
            for (let i = 0; i < dataAreaObj.length; i++) {
                var tableItem = dataAreaObj[i];
                tableItem.setAttribute('disabled', 'disabled');
            }
        }

    }
    

    
});

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

// フォームGBT0004ORDERで利用するJavaScript
// ダウンロード処理
function f_ExcelPrint() {
    // リンク参照
    var printUrlObj = document.getElementById("hdnPrintURL");
    if (printUrlObj === null) {
        return;
    }
    window.open(printUrlObj.value, "view", "_blank");
    printUrlObj.value = '';
}
// ドロップ処理（処理抑止）
function f_dragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
}
// ドロップ処理（ドラッグドロップ入力）
function f_dragEvent(e) {
    e.preventDefault();
    commonDispWait();
    var footerMsg = document.getElementById("lblFooterMessage");
    footerMsg.className = "";

    this.style.cursor = "default";
    if (document.getElementById('hdnMAPpermitCode').value === "TRUE") {
        footerMsg.textContent = document.getElementById('hdnUploadMessage01').value;
        footerMsg.classList.add('INFORMATION');

        // ドラッグされたファイル情報を取得
        var files = e.dataTransfer.files;

        // 送信用FormData オブジェクトを用意
        var fd = new FormData();

        // ファイル情報を追加する

        for (var i = 0; i < files.length; i++) {
            /* 拡張子xlsxの場合 */
            var reg = new RegExp("^.*\.xlsx$");
            if (files[i].name.toLowerCase().match(reg)) {
                fd.append("files", files[i]);
            } else {
                footerMsg.textContent = document.getElementById('hdnUploadError05').value;
                footerMsg.classList.add('ABNORMAL');
                commonHideWait();
                return;
            }
        }

        // XMLHttpRequest オブジェクトを作成
        var xhr = new XMLHttpRequest();

        // ドロップファイルによりURL変更
        // 「POST メソッド」「接続先 URL」を指定
        xhr.open("POST", document.getElementById('hdnFileUpUrl').value, false);

        // イベント設定
        // ⇒XHR 送信正常で実行されるイベント
        xhr.onload = function (e) {
            if (e.currentTarget.status === 200) {


                document.getElementById("hdnListUpload").value = "XLS_LOADED";
                document.forms[0].submit();                             //aspx起動
            } else {
                footerMsg.textContent = document.getElementById('hdnUploadError01').value;
                footerMsg.classList.add('ABNORMAL');
                commonHideWait();
            }
        };

        // ⇒XHR 送信ERRで実行されるイベント
        xhr.onerror = function (e) {
            footerMsg.textContent = document.getElementById('hdnUploadError01').value;
            footerMsg.classList.add('ABNORMAL');
            commonHideWait();
        };

        // ⇒XHR 通信中止すると実行されるイベント
        xhr.onabort = function (e) {
            footerMsg.textContent = document.getElementById('hdnUploadError02').value;
            footerMsg.classList.add('ABNORMAL');
            commonHideWait();
        };

        // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
        xhr.ontimeout = function (e) {
            footerMsg.textContent = document.getElementById('hdnUploadError03').value;
            footerMsg.classList.add('ABNORMAL');
            commonHideWait();
        };

        // 「送信データ」を指定、XHR 通信を開始する
        xhr.send(fd);
    } else {
        footerMsg.textContent = document.getElementById('hdnUploadError04').value;
        footerMsg.classList.add('ABNORMAL');
        commonHideWait();
    }

}
// ○一覧用処理
function ListDbClick(obj, LineCnt) {
    return false; /* 一旦行全体のダブルクリックをつぶす */
    if (document.getElementById("hdnSubmit").value === "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE";
        document.getElementById("hdnListDBclick").value = LineCnt;
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    }
}
// 〇一覧ボタンイベントバインド
function bindGridButtonClickEvent() {
    rowHeaderObj = document.getElementById('WF_LISTAREA_DL');
    if (rowHeaderObj === null) {
        return; /* レンダリングされていない場合はそのまま終了 */
    }
    var buttonList = rowHeaderObj.querySelectorAll("button[id^='btnWF_LISTAREAACTION']");
    /* 対象のボタンが1件もない場合はそのまま終了 */
    if (buttonList === null) {
        return;
    }
    if (buttonList.length === 0) {
        return;
    }

    for (let i = 0; i < buttonList.length; i++) {
        var buttonObj = buttonList[i];
        var tdNode = buttonObj.parentNode;
        var trNode = tdNode.parentNode;
        var brOdFlgCell = trNode.cells[3];

        var rightNode = document.getElementById('WF_LISTAREA_DR').getElementsByTagName('table')[0];
        var rightTrNode = rightNode.rows[trNode.rowIndex];

        if (brOdFlgCell.innerText === '1') {
            /* ブレーカー情報の場合 */
            /* クリックイベントに紐づけ */
            buttonObj.onclick = (function (buttonObj) {
                return function () {
                    listButtonClick(buttonObj);
                };
            })(buttonObj);
            buttonObj.onmouseover = (function () {
                return function () {
                    window.status = "";
                    return true;
                };
            })();

        } else {
            /* オーダー情報の場合 */
            tdNode.removeChild(buttonObj); /* ノードよりボタンを削除 */
            trNode.dataset.orderRow = '1';
            rightTrNode.dataset.orderRow = '1';
            /* IEの場合動的に変更したAttributeを使用してCSSが反映されないため再代入 */
            trNode.innerHTML = trNode.innerHTML;
            rightTrNode.innerHTML = rightTrNode.innerHTML;
        }
    }
}
// 〇一覧削除ボタンクリックイベント
function listButtonClick(obj) {
    var currentRowNum = obj.getAttribute('rownum');
    var objCurrentRowNum = document.getElementById('hdnListCurrentRownum');
    var objButtonClick = document.getElementById('hdnButtonClick');
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE';
        objCurrentRowNum.value = currentRowNum;
        objButtonClick.value = 'btnListDelete';
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    }
    return false;
}
/* 費用項目を開く */
function bindAddCostOnClick() {
    var btnCostAdd = document.getElementById('btnAddCost');
    /* 費用追加ボタンが存在しない場合はそのまま終了 */
    if (btnCostAdd === null) {
        return;
    }
    /* 左ボックスエレメント */
    var leftBoxOjb = document.getElementById('divLeftbox');
    /* そもそも左ボックスが存在しない場合はそのまま終了 */
    if (leftBoxOjb === null) {
        return;
    }
    /* 追加ボタンにイベントをバインド */
    btnCostAdd.addEventListener('click', (function () {
        var submitObj = document.getElementById('hdnSubmit');
        var mapvariantObj = document.getElementById('hdnListMapVariant');
        var viewId = 'vLeftAddCost';
        if (mapvariantObj.value === 'GB_NonBreaker') {
            viewId = 'vLeftAddNbCost';
        }
        var dblClickObject = document.getElementById('hdnTextDbClickField');
        var viewIdObject = document.getElementById('hdnLeftboxActiveViewId');
        var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
        if (submitObj.value === 'FALSE') {
            submitObj.value = 'TRUE';
            dblClickObject.value = 'gvCostList';
            viewIdObject.value = viewId;
            leftBoxOpen.value = "Open";
            commonDispWait();
            document.forms[0].submit();
        }
    }), false);

    var rblPolPodObj = document.getElementById('rblPolPod');
    if (rblPolPodObj !== null) {
        rblPolPodObj.addEventListener("change", (function () {
            var submitObj = document.getElementById('hdnSubmit');
            var mapvariantObj = document.getElementById('hdnListMapVariant');
            var viewId = 'vLeftAddCost';
            if (mapvariantObj.value === 'GB_NonBreaker') {
                viewId = 'vLeftAddNbCost';
            }
            var dblClickObject = document.getElementById('hdnTextDbClickField');
            var viewIdObject = document.getElementById('hdnLeftboxActiveViewId');
            var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
            if (submitObj.value === 'FALSE') {
                submitObj.value = 'TRUE';
                dblClickObject.value = 'gvCostList';
                viewIdObject.value = viewId;
                leftBoxOpen.value = "Open";
                commonDispWait();
                document.forms[0].submit();
            }
        }), false);


    }
}
/* 一覧表の表示制御 */
function gridDispControl(delButtonName) {
    /* ボタンオブジェクト名称設定 */
    var listLeftData = document.getElementById('WF_LISTAREA_DL');
    /* 右明細テキスト */
    var listRightData = document.getElementById('WF_LISTAREA_DR');
    if (listRightData !== null) {

        /* 承認備考欄 */
        var applyReasonTextList = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREAAPPLYTEXT']");
        if (applyReasonTextList === null) {
            applyReasonTextList = listLeftData.querySelectorAll("input[type=text][id^='txtWF_LISTAREAAPPLYTEXT']");
        }
        if (applyReasonTextList !== null) {
            var wrapperMain = document.createElement('div');
            for (let i = 0; i < applyReasonTextList.length; i++) {
                let applyReasonTextObj = applyReasonTextList[i];
                applyReasonTextObj.readOnly = 'true';
                applyReasonTextObj.tabIndex = '-1';
                /* ダブルクリックイベントバインド */
                // フォーカスを合わさないとテキストボックスはダブルクリックに反応しないため、
                // 前後をspanタグで括りそちらにダブルクリックイベントを記載
                let wrapper = wrapperMain.cloneNode(true);
                //wrapper.id = "spn" + applyReasonTextObj.id;
                wrapper.appendChild(applyReasonTextObj.cloneNode(true));
                applyReasonTextObj.parentNode.replaceChild(wrapper, applyReasonTextObj);
                /* ダブルクリックイベントに紐づけ */
                wrapper.addEventListener('dblclick', (function (applyReasonTextObj) {
                    return function () {
                        //applyReasonTextObj.value = "test";
                        displayApplyReasonbox(applyReasonTextObj);
                    };
                })(applyReasonTextObj), false);
            }
        }

        /* 備考欄 */
        var remarkList = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREAREMARK']");
        if (remarkList === null) {
            remarkList = listLeftData.querySelectorAll("input[type=text][id^='txtWF_LISTAREAREMARK']");
        }
        if (remarkList !== null) {
            var wrapperMain = document.createElement('div');
            for (let i = 0; i < remarkList.length; i++) {
                let remarkObj = remarkList[i];
                remarkObj.readOnly = 'true';
                remarkObj.tabIndex = '-1';
                /* ダブルクリックイベントバインド */
                // フォーカスを合わさないとテキストボックスはダブルクリックに反応しないため、
                // 前後をspanタグで括りそちらにダブルクリックイベントを記載
                let wrapper = wrapperMain.cloneNode(true);
                wrapper.appendChild(remarkObj.cloneNode(true));
                remarkObj.parentNode.replaceChild(wrapper, remarkObj);
                /* ダブルクリックイベントに紐づけ */
                wrapper.addEventListener('dblclick', (function (remarkObj) {
                    return function () {
                        displayRemarkbox(remarkObj);
                    };
                })(remarkObj), false);
            }
        }
    }
}

/* 費用一覧削除ボタンイベントバインド及び削除ボタンの表示制御 */
function bindCostListDeleteOnClick() {
    let rowHeaderObj = document.getElementById('WF_LISTAREA_DL');
    if (rowHeaderObj === null) {
        return; /* レンダリングされていない場合はそのまま終了 */
    }
    let listAreaObj = document.getElementById('WF_LISTAREA');
    if (listAreaObj.classList.contains('GB_SOA') === true ||
        listAreaObj.classList.contains('GB_Demurrage') === true) {
        return;
    }
    var buttonList = rowHeaderObj.querySelectorAll("input[type=button][id^='btnWF_LISTAREAACTION']");
    /* 対象のボタンが1件もない場合はそのまま終了 */
    if (buttonList === null) {
        return;
    }
    if (buttonList.length === 0) {
        return;
    }
    rowHeaderObj.style.display = "none";
    for (let i = 0; i < buttonList.length; i++) {
        let buttonObj = buttonList[i];
        /* クリックイベントに紐づけ */
        buttonObj.onclick = (function (buttonObj) {
            return function () {
                listButtonClick(buttonObj);
            };
        })(buttonObj);

    }
    rowHeaderObj.style.display = "";
}
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
        remarkboxField.value = 'txtWF_LISTAREAAPPLYTEXT';
        remarkBoxOpenObj.value = 'Open';
        leftBoxOpen.value = ''; /* 右ボックスとの共存不可 */
        fieldDisplayNameObj.value = fieldName;
        commonDispWait();
        document.forms[0].submit();

    }
}
/* 備考ボックス表示JavaScript(ダブルクリックされた一覧備考) */
function displayRemarkbox(obj) {
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
        remarkboxField.value = 'txtWF_LISTAREAREMARK';
        remarkBoxOpenObj.value = 'Open';
        leftBoxOpen.value = ''; /* 右ボックスとの共存不可 */
        fieldDisplayNameObj.value = fieldName;
        commonDispWait();
        document.forms[0].submit();

    }
}
// 一覧表の日付項目にイベントバインド
function bindListDateTextbox() {
    /* 表右明細行オブジェクトを取得 */
    var listRightData = document.getElementById('WF_LISTAREA_DR');

    
    if (listRightData !== null) {
        listRightData.style.display = "none"; // 一旦画面描画を行わせない為左データエリアを非表示
        /* 右明細オブジェクトのカレンダーイベントを紐づけるテキストボックスを取得 */
        var dispCalEventObjcts = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREASCHEDELDATE']:not([id^='txtWF_LISTAREASCHEDELDATEBR'])," +
                                                                "input[type=text][id^='txtWF_LISTAREAACTUALDATE'], input[type=text][id^='txtWF_LISTAREASOAAPPDATE']");
        for (let i = 0; i < dispCalEventObjcts.length; i++) {
            var canEventBind = false;
            var inputObject = dispCalEventObjcts[i];
            var inputTargetViewId = 'vLeftCal';
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
/* 通貨コード左ボックス表示イベントバインド */
function bindListCurrencyTextbox() {
    /* 表右明細行オブジェクトを取得 */
    var listRightData = document.getElementById('WF_LISTAREA_DR');

    if (listRightData !== null) {
        listRightData.style.display = "none"; // 一旦画面描画を行わせない為左データエリアを非表示
        /* 右明細オブジェクトのカレンダーイベントを紐づけるテキストボックスを取得 */
        var dispCalEventObjcts = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREACURRENCYCODE']");
        for (let i = 0; i < dispCalEventObjcts.length; i++) {
            var canEventBind = false;
            var inputObject = dispCalEventObjcts[i];
            var inputTargetViewId = 'vLeftCurrencyCode';
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
/* 通貨コード左ボックス表示イベントバインド */
function bindListContractorTextbox() {
    /* 表右明細行オブジェクトを取得 */
    var listRightData = document.getElementById('WF_LISTAREA_DR');

    if (listRightData !== null) {
        listRightData.style.display = "none"; // 一旦画面描画を行わせない為左データエリアを非表示
        /* 右明細オブジェクトのカレンダーイベントを紐づけるテキストボックスを取得 */
        var dispCalEventObjcts = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREACONTRACTORODR']," +
                                                                "input[type=text][id^='txtWF_LISTAREACONTRACTORFIX']");
        for (let i = 0; i < dispCalEventObjcts.length; i++) {
            var canEventBind = false;
            var inputObject = dispCalEventObjcts[i];
            var inputTargetViewId = 'vLeftContractor';
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
/* 汎用補助区分左ボックス表示イベントバインド */
function bindListAccCurrencySegmentTextbox() {
    /* 表右明細行オブジェクトを取得 */
    var listRightData = document.getElementById('WF_LISTAREA_DR');

    if (listRightData !== null) {
        listRightData.style.display = "none"; // 一旦画面描画を行わせない為左データエリアを非表示
        /* 右明細オブジェクトのカレンダーイベントを紐づけるテキストボックスを取得 */
        var dispCalEventObjcts = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREAACCCURRENCYSEGMENT']");
        for (let i = 0; i < dispCalEventObjcts.length; i++) {
            var canEventBind = false;
            var inputObject = dispCalEventObjcts[i];
            var inputTargetViewId = 'vLeftAccCurrencySegment';
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
                //inputObject = document.getElementById(inputObjectId);
                //inputObject.onfocus = (function (inputObject) {
                //    return function () {
                //        var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                //        if (hdnObj !== null) {
                //            hdnObj.value = '';
                //        }
                //        inputObject.dataset.prev = inputObject.value; // 変更したフィールドのIDを記録
                //    };
                //})(inputObject);
                // フォーカス喪失後
                //var onchangeItemId = document.getElementById('hdnOnchangeField');
                //inputObject.onblur = (function (inputObject, onchangeItemId) {
                //    return function () {
                //        var submitObj = document.getElementById('hdnSubmit');
                //        var footerMessageObj = document.getElementById('lblFooterMessage');
                //        if (footerMessageObj !== null) {
                //            footerMessageObj.innerText = '';
                //        }
                //        if (inputObject.dataset.prev !== inputObject.value) {
                //            if (submitObj.value === 'FALSE') {
                //                submitObj.value = 'TRUE';
                //                onchangeItemId.value = inputObject.id; // 変更したフィールドのIDを記録
                //                var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
                //                if (activeElem !== null || document.activeElement.id !== null) {
                //                    activeElem.value = document.activeElement.id;
                //                }
                //                inputObject.dataset.prev = '';
                //                let hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                //                if (hdnObj !== null) {
                //                    let clickedButtonId = document.getElementById('hdnButtonClick');
                //                    clickedButtonId.value = hdnObj.value;
                //                    hdnObj.value = '';
                //                }
                //                let currentRownum = document.getElementById('hdnListCurrentRownum');
                //                currentRownum.value = inputObject.getAttribute("rownum");
                //                document.forms[0].submit();  // サブミット
                //            } else {
                //                // ボタンクリックでフォーカスが移された場合を考慮
                //                let hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                //                if (hdnObj !== null) {
                //                    if (hdnObj.value !== '') {
                //                        let clickedButtonId = document.getElementById('hdnButtonClick');
                //                        clickedButtonId.value = hdnObj.value;
                //                        hdnObj.value = '';
                //                        if (submitObj.value === 'FALSE') {
                //                            submitObj.value = 'TRUE';
                //                            let currentRownum = document.getElementById('hdnListCurrentRownum');
                //                            currentRownum.value = inputObject.getAttribute("rownum");
                //                            document.forms[0].submit();  // サブミット
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    };

                //})(inputObject, onchangeItemId);
            }

        }
        listRightData.style.display = ""; // 左データエリアを表示
    }

}
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
// 一覧表の日付項目に左カレンダー選択ボックスを表示するイベント
function leftMonthViewOpen(lineCnt, closingMonth, reportMonth,txtObjId) {
    var submitObj = document.getElementById('hdnSubmit');
    var dblClickObject = document.getElementById('hdnTextDbClickField');
    var viewIdObject = document.getElementById('hdnLeftboxActiveViewId');
    var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
    var currentRownum = document.getElementById('hdnListCurrentRownum');
    var viewId = 'vLeftReportMonth';
    if (submitObj === null || dblClickObject === null || viewIdObject === null || leftBoxOpen === null) {
        return;
    }

    // サブミットフラグが立っていない場合のみ実行
    if (submitObj.value === 'FALSE') {
        submitObj.value = 'TRUE';
        dblClickObject.value = txtObjId;
        viewIdObject.value = viewId;
        leftBoxOpen.value = "Open";
        currentRownum.value = lineCnt;
        commonDispWait();
        document.forms[0].submit();
    }

}
/* タンク一覧に遷移するため所定の項目に値を格納しサブミット */
function browseTankList(orderNo, tankSeq, dataId) {
    var submitObj = document.getElementById('hdnSubmit');
    var selectedOrderNo = document.getElementById('hdnSelectedOrderId');
    var selectedTankSeq = document.getElementById('hdnSelectedTankSeq');
    var selectedDataId = document.getElementById('hdnSelectedDataId');
    var openTankProc = document.getElementById('hdnTankProc');
    if (submitObj === null || selectedOrderNo === null || selectedTankSeq === null || selectedDataId === null) {
        return;
    } 
    selectedOrderNo.value = '';
    selectedTankSeq.value = '';
    selectedDataId.value = '';
    if (submitObj.value === 'FALSE') {
        submitObj.value = 'TRUE';
        openTankProc.value = 'OPEN';
        selectedOrderNo.value = orderNo;
        selectedTankSeq.value = tankSeq;
        selectedDataId.value = dataId;
        commonDispWait();
        document.forms[0].submit();
    }
}
/* タンク一覧に遷移するため所定の項目に値を格納しサブミット */
function deleteTankNo(orderNo, tankSeq, dataId) {
    var submitObj = document.getElementById('hdnSubmit');
    var selectedOrderNo = document.getElementById('hdnSelectedOrderId');
    var selectedTankSeq = document.getElementById('hdnSelectedTankSeq');
    var selectedDataId = document.getElementById('hdnSelectedDataId');
    var openTankProc = document.getElementById('hdnTankProc');
    if (submitObj === null || selectedOrderNo === null || selectedTankSeq === null) {
        return;
    }
    selectedOrderNo.value = '';
    selectedTankSeq.value = '';
    selectedDataId.value = '';
    if (submitObj.value === 'FALSE') {
        submitObj.value = 'TRUE';
        openTankProc.value = 'DELETE';
        selectedOrderNo.value = orderNo;
        selectedTankSeq.value = tankSeq;
        selectedDataId.value = dataId;
        commonDispWait();
        document.forms[0].submit();
    }
}
/* 金額オーバーの一覧に色付け（ロード時） */
function setCostPriceBackGroundColor() {
    /* BR予定額 vs オーダー予定額 */
    var brCostColumnObj = getTargetColumnNoTable('AMOUNTFIX', 'WF_LISTAREA');
    var ordCostColumnObj = getTargetColumnNoTable('AMOUNTORD', 'WF_LISTAREA');
    //比較対象のカラムが存在していない場合は実行不可能
    if (brCostColumnObj !== null && ordCostColumnObj !== null) {
        let brColumnNo = brCostColumnObj.ColumnNo;
        let brTable = brCostColumnObj.TargetTable;
        let ordColumnNo = ordCostColumnObj.ColumnNo;
        let ordTable = ordCostColumnObj.TargetTable;
        let ordIsTextObj = false;
        if (ordTable.rows.length !== 0) {
            let checkCell = ordTable.rows[0].cells[ordColumnNo];
            if (checkCell.querySelectorAll('input[type=text]').length === 1) {
                ordIsTextObj = true;
            }
            for (let i = 0; i < brTable.rows.length; i++) {
                let brValueObj = brTable.rows[i].cells[brColumnNo];
                let odrValueObj;
                let ordValue;
                if (ordIsTextObj) {
                    odrValueObj = ordTable.rows[i].cells[ordColumnNo].querySelectorAll('input[type=text]')[0];
                    ordValue = odrValueObj.value;
                    odrValueObj.onblur = (function (odrValueObj, compareValue) {
                        return function () {
                            costValueChange(odrValueObj, compareValue);
                        };

                    })(odrValueObj, brValueObj.textContent);
                } else {
                    odrValueObj = ordTable.rows[i].cells[ordColumnNo];
                    ordValue = odrValueObj.textContent;
                }
                var brValue = brValueObj.textContent;
                styleClass = compareCostValue(brValue, ordValue);
                if (styleClass !== '') {
                    odrValueObj.classList.add(styleClass);
                } 
                reportMonthControl(styleClass, odrValueObj.getAttribute('rownum'));
                applyEnableControl(styleClass, odrValueObj.getAttribute('rownum'));
                var soaChkObjId = 'chkWF_LISTAREASOACHECK' + odrValueObj.getAttribute('rownum');
                soaChange(soaChkObjId, odrValueObj.getAttribute('rownum'), styleClass);
            }
        }

    }

}
/* 文字列の2つの費用値を検索 */
function compareCostValue(firstVal, secondVal) {
    let firstValWOComma = firstVal.replace(/,/g, '');
    let secondValWOComma = secondVal.replace(/,/g, '');
    if (firstValWOComma === '') {
        firstValWOComma = '0';
    }
    if (secondValWOComma === '') {
        secondValWOComma = '0';
    }

    if (isNaN(firstValWOComma) || isNaN(firstValWOComma)) {
        return '';
    }
    let firstNum = new Number(firstValWOComma);
    let secondNum = new Number(secondValWOComma);
    if (firstNum > secondNum) {
        return 'greatherThan';
    } else if (firstNum < secondNum) {
        return 'lessThan';
    } else {
        return '';
    }
}
/* 費用項目変更時イベント */
/* 引数：targetObject 費用項目テキストボックス */
/*       compareValue 比較対象の値 */
function costValueChange(targetObject, compareValue) {
    var objVal = targetObject.value;
    var styleClass = compareCostValue(compareValue, objVal);
    targetObject.classList.remove('greatherThan');
    targetObject.classList.remove('lessThan');
    if (styleClass !== '') {
        targetObject.classList.add(styleClass);
    }
    reportMonthControl(styleClass, targetObject.getAttribute('rownum'));
    applyEnableControl(styleClass, targetObject.getAttribute('rownum'));
}

function reportMonthControl(styleClass, rowNum) {
    var reportMonthId = 'txtWF_LISTAREA' + 'DEMREPORTMONTH' + rowNum;
    var reportManthSpanId = 'lblWF_LISTAREA' + 'DEMREPORTMONTH' + rowNum;
    var reportMonthObj = document.getElementById(reportMonthId);
    var reportMonthSpanObj = document.getElementById(reportManthSpanId);
    if (reportMonthObj === null) {
        return;
    }
    if (reportMonthSpanObj === null) {
        return;
    }

    if (styleClass !== '') {
        reportMonthObj.readOnly = 'true';
        reportMonthObj.classList.add('aspNetDisabled');
        reportMonthObj.value = reportMonthObj.dataset.reportmonth;
        reportMonthSpanObj.classList.add("aspNetDisabled");
    } else {
        reportMonthObj.readOnly = '';
        reportMonthObj.classList.remove('aspNetDisabled');
        reportMonthSpanObj.classList.remove("aspNetDisabled");
    }
}
/* 申請ボックスの使用可否制御 */
function applyEnableControl(styleClass, rowNum) {
    var applyCheckId = 'chkWF_LISTAREA' + 'APPLY' + rowNum;

    var applyTextId = 'txtWF_LISTAREA' + 'APPLYTEXT' + rowNum;
    var soaCheckId = 'chkWF_LISTAREA' + 'SOACHECK' + rowNum;
    var applyCheckObj = document.getElementById(applyCheckId);
    var applyTextObj = document.getElementById(applyTextId);
    var soaCheckObj = document.getElementById(soaCheckId);

    if (applyCheckObj === null || applyTextObj === null) {
        return;
    }
    var applyTextParent = applyTextObj.parentNode;
    if (styleClass !== '') {
        applyCheckObj.disabled = '';
        applyCheckObj.classList.remove('aspNetDisabled');
        let clId = applyCheckObj.id + "Clone";
        let clObj = document.getElementById(clId);
        if (clObj !== null) {
            applyCheckObj.parentNode.removeChild(clObj);
            applyCheckObj.style.display = "inline-block";
        }
        applyTextParent.readOnly = '';
        applyTextParent.classList.remove('aspNetDisabled');
        if (soaCheckObj !== null) {
            soaCheckObj.disabled = 'true';
            soaCheckObj.classList.add('aspNetDisabled');
        }
    } else {
        applyCheckObj.checked = false;
        applyCheckObj.disabled = 'true';
        applyCheckObj.classList.add('aspNetDisabled');
        applyTextParent.readOnly = 'true';
        applyTextParent.classList.add('aspNetDisabled');
        applyTextObj.classList.remove('needsInput');
        if (soaCheckObj !== null) {
            //soaCheckObj.disabled = '';
            //soaCheckObj.classList.remove('aspNetDisabled');
        }
    }
}

/* オフィス付け替え処理 */
/* 引数：対象行 */
/* compareValue 比較対象の値 */
function swapOffice(lineCnt) {
    var submitObj = document.getElementById('hdnSubmit');
    var dblClickObject = document.getElementById('hdnTextDbClickField');
    var currentRownum = document.getElementById('hdnListCurrentRownum');
    if (submitObj === null || dblClickObject === null || currentRownum === null) {
        return;
    }

    if (submitObj.value !== "FALSE") {
        return;
    } 
    dblClickObject.value = 'DTLOFFICE';
    currentRownum.value = lineCnt;
    submitObj.value = 'TRUE';
    commonDispWait();
    document.forms[0].submit();

}
/* デマレッジ更新イベントバインド */
/* 引数：対象行 */
/* compareValue 比較対象の値 */
function bindCalcDemAgentComm() {
    var listObj = document.getElementById('WF_LISTAREA');
    // 一覧表がレンダリングされていない場合は終了
    if (listObj === null) {
        return;
    }
    // classにGB_Demurrageがない場合はほか機能の為そのまま終了
    if (listObj.classList.contains('GB_Demurrage') === false) {
        return;
    }
    // イベントの紐づけ
    var calcDemAgentCommObjcts = listObj.querySelectorAll("input[type=text][id^='txtWF_LISTAREAAMOUNTFIX']");
    if (calcDemAgentCommObjcts === null) {
        return;
    }
    for (let i = 0; i < calcDemAgentCommObjcts.length; i++) {
        var txtObj = calcDemAgentCommObjcts[i];
        txtObj._oldvalue = txtObj.value;

        /* ブラーイベントに紐づけ */
        txtObj.addEventListener('focus', function (txtObj) {
            return function () {
                var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                if (hdnObj !== null) {
                    hdnObj.value = '';
                }
                txtObj._oldvalue = txtObj.value;
            };
        }(txtObj), false);

        txtObj.addEventListener('blur', function (txtObj) {
            return function () {
                if (txtObj._oldvalue !== txtObj.value) {
                    var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
                    if (activeElem !== null || document.activeElement.id !== null) {
                        activeElem.value = document.activeElement.id;
                    }
                    var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                    if (hdnObj !== null) {
                        var clickedButtonId = document.getElementById('hdnButtonClick');
                        clickedButtonId.value = hdnObj.value;
                        hdnObj.value = '';
                    }
                    var rownum = txtObj.getAttribute("rownum");
                    calcDemAgentComm(rownum);
                }
            };
        }(txtObj), false);

    }
}
/* デマレッジ更新イベントコール */
/* 引数：対象行 */
function calcDemAgentComm(lineCnt) {
    var submitObj = document.getElementById('hdnSubmit');
    var dblClickObject = document.getElementById('hdnTextDbClickField');
    var currentRownum = document.getElementById('hdnListCurrentRownum');
    if (submitObj === null || dblClickObject === null || currentRownum === null) {
        return;
    }

    if (submitObj.value !== "FALSE") {
        return;
    }
    dblClickObject.value = 'AMOUNTFIX';
    currentRownum.value = lineCnt;
    submitObj.value = 'TRUE';
    commonDispWait();
    document.forms[0].submit();

}
/* デマレッジ更新イベントコール */
/* 引数：対象チェックボックスID,申請コメント入力ID */
function applyChange(checkObjId, applyTextObjId) {
    var checkObj = document.getElementById(checkObjId);
    var applyTextObj = document.getElementById(applyTextObjId);
    if (checkObj === null || applyTextObj === null) {
        return;
    }
    if (checkObj.checked === true) {
        applyTextObj.classList.add('needsInput');
    } else {
        applyTextObj.classList.remove('needsInput');
    }
}
/* SOAチェック変更時イベント */
function soaChange(checkObjId, lineCnt, styleClass) {
    var soaChkObj = document.getElementById(checkObjId);
    if (soaChkObj === null) {
        return;
    }
    var actualAmtObj = document.getElementById('txtWF_LISTAREAAMOUNTORD' + lineCnt);
    var jotChkObj = document.getElementById('chkWF_LISTAREAJOT' + lineCnt);
    var applyChkObj = document.getElementById('chkWF_LISTAREAAPPLY' + lineCnt);
    var applyTextObj = document.getElementById('txtWF_LISTAREAAPPLYTEXT' + lineCnt);
    var actualDateObj = document.getElementById('txtWF_LISTAREAACTUALDATE' + lineCnt);
    if (styleClass === null || styleClass === undefined) {
        styleClass = '';
        if (actualAmtObj !== null) {
            if (actualAmtObj.classList.contains('lessThan')) {
                styleClass = 'lessThan';
            } else if (actualAmtObj.classList.contains('greatherThan')) {
                styleClass = 'greatherThan';
            }
        }
    }

    if (applyChkObj === null) {
        //SOAでの申請チェック非表示は申請中の為他の入力項目は使用不可能
        if (soaChkObj !== null) {
            soaChkObj.disabled = 'true';
            soaChkObj.classList.add('aspNetDisabled');
        }
        if (jotChkObj !== null) {
            jotChkObj.disabled = 'true';
            jotChkObj.classList.add('aspNetDisabled');
            let chkItem = jotChkObj;
            let clChk = chkItem.cloneNode(true);
            clChk.id = clChk.id + "Clone";
            clChk.name = clChk.name + "Clone";
            chkItem.parentNode.appendChild(clChk);
            chkItem.disabled = false;
            chkItem.style.display = "none";
        }
        if (actualAmtObj !== null) {
            actualAmtObj.readOnly = 'true';
            actualAmtObj.classList.add('aspNetDisabled');
        }
        if (actualDateObj !== null) {
            actualDateObj.readOnly = 'true';
            actualDateObj.classList.add('aspNetDisabled');
        }
        return;
    }

    if (soaChkObj.checked === true) {
        if (applyChkObj !== null) {
            applyChkObj.checked = false;
            applyChkObj.disabled = 'true';
            applyChkObj.classList.add('aspNetDisabled');
            applyTextObj.classList.remove('needsInput');
        } 

        if (jotChkObj !== null) {
            jotChkObj.disabled = 'true';
            jotChkObj.classList.add('aspNetDisabled');
            let chkItem = jotChkObj;
            let clChk = chkItem.cloneNode(true);
            clChk.id = clChk.id + "Clone";
            clChk.name = clChk.name + "Clone";
            chkItem.parentNode.appendChild(clChk);
            chkItem.disabled = false;
            chkItem.style.display = "none";
        }

        if (actualAmtObj !== null) {
            actualAmtObj.readOnly = 'true';
            actualAmtObj.classList.add('aspNetDisabled');
        }
        if (actualDateObj !== null) {
            actualDateObj.readOnly = 'true';
            actualDateObj.classList.add('aspNetDisabled');
        }
    } else {

        if (applyChkObj !== null && styleClass !== '') {
            applyChkObj.disabled = '';
            applyChkObj.classList.remove('aspNetDisabled');
            let clId = applyChkObj.id + "Clone";
            let clObj = document.getElementById(clId);
            if (clObj !== null) {
                applyChkObj.parentNode.removeChild(clObj);
                applyChkObj.style.display = "inline-block";
            }
        }

        if (jotChkObj !== null) {
            jotChkObj.disabled = '';
            jotChkObj.classList.remove('aspNetDisabled');
            let clId = jotChkObj.id + "Clone";
            let clObj = document.getElementById(clId);
            if (clObj !== null) {
                jotChkObj.parentNode.removeChild(clObj);
                jotChkObj.style.display = "inline-block";
            }
        }

        if (actualAmtObj !== null) {
            actualAmtObj.readOnly = '';
            actualAmtObj.classList.remove('aspNetDisabled');
            let clId = actualAmtObj.id + "Clone";
            let clObj = document.getElementById(clId);
            if (clObj !== null) {
                actualAmtObj.parentNode.removeChild(clObj);
                actualAmtObj.style.display = "inline-block";
            }
        }
        if (actualDateObj !== null) {
            if (!actualDateObj.dataset.everydisable) {
                actualDateObj.readOnly = '';
                actualDateObj.classList.remove('aspNetDisabled');
            }
        }
    }
    
}
/* 強制サブミットを行う */
function doSubmit() {
    var submitObj = document.getElementById('hdnSubmit');
    if (submitObj.value !== "FALSE") {
        return;
    }
    submitObj.value = 'TRUE';
    commonDispWait();
    document.forms[0].submit();
}
/* ASP.NETにてチェックボックスのチェックON OFFをコード側で変えても戻される為の回避 */
function listChkBoxControl(listId) {
    var listArea = document.getElementById('WF_LISTAREA');
    if (listArea === null) {
        return;
    }
    var chkCheckedModList = listArea.querySelectorAll("span[data-listchkid]");
    if (chkCheckedModList !== null) {
        for (let i = 0; i < chkCheckedModList.length; i++) {
            var chkParentSpan = chkCheckedModList[i];
            var chkObjId = chkParentSpan.dataset.listchkid;
            var chkObj = document.getElementById(chkObjId);
            if (chkObj !== null) {
                if (chkParentSpan.dataset.checkedval === 'true') {
                    chkObj.checked = true;
                } else {
                    chkObj.checked = false;
                }
            }
        }
    }
}
/* チェックボックスOn/Offでの合計金額の加減算 */
function calcSummaryAmount(chkId, thisRowAmount) {
    // 合計欄表示オブジェクトの取得
    var summaryObj = document.getElementById('lblUsdAmountSummary');
    if (summaryObj === null) {
        return;
    }
    // チェックボックスオブジェクトの取得
    var chkObj = document.getElementById(chkId);
    if (chkObj === null) {
        return;
    }
    // 表示内容を数字変換
    var summaryNum = 0;
    if (summaryObj.innerText !== '') {
        summaryNum = Number(summaryObj.innerText.replace(/,/, ''));
    }
    var thisRowAmountNum = 0;
    if (thisRowAmount !== '') {
        thisRowAmountNum = Number(thisRowAmount.replace(/,/, ''));
    }
    // チェックの状態に応じ加減算
    if (chkObj.checked === true) {
        summaryNum = summaryNum + thisRowAmountNum;
    } else {
        summaryNum = summaryNum - thisRowAmountNum;
    }
    // 合計欄に表示を戻す
    summaryObj.innerText = formatCurrency(summaryNum,2);

}
function formatCurrency(num, scale) {
    var re = /(\d)(?=(\d\d\d)+(?!\d))/g; //正規表現
    return Number(num).toFixed(scale).replace(re, '$1,');
}