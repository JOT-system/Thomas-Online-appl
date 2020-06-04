/**
 * @fileoverview JOTシステムマスタ関連共通JavaScript処理
 */
///**
// * 引数に指定したオブジェクトを無効化
// * @param {string} objectIdList クリックイベントを紐づけるボタンIDの配列
// * @return {undefined} なし
// * @description マスターロード時に呼び出す
// */
//function masterDisableObjects(objectIdList) {
//    // 制御するオブジェクトがない場合はそのまま終了
//    if (objectIdList === null || objectIdList === undefined) {
//        return;
//    }
//    // 活性非活性の管理オブジェクトを取得
//    var permitCodeObj = document.getElementById('hdnMAPpermitCode');
//    if (permitCodeObj === null) {
//        return; //存在しない場合はそのまま終了
//    }
//    var disableValue = '';
//    if (permitCodeObj.value !== 'TRUE') {
//        var disableValue = 'disabled';
//    }
//    // 制御対象オブジェクトのループ
//    for (let i = 0; i < objectIdList.length; i++) {
//        var targetObj = document.getElementById(objectIdList[i]);
//        if (targetObj !== null) {
//            //targetObj.disabled = disableValue;
//        }
//    }
//}
/**
 * マスタ共通ロード
 */
window.addEventListener('load', function () {
    /* **********************************
     * テキストボックス変更イベントにつき 
     * 次フォーカスを保持するよう変更 
    ********************************** */
    var changeEventTextObjects = document.querySelectorAll('span[class^="WF_DViewRep"] table input[type=text][onchange^="TextChange("]');
    for (let i = 0; i < changeEventTextObjects.length; i++) {
        var objText = changeEventTextObjects[i];
        // 編集前直前情報の保持
        objText._oldvalue = objText.value;
        var orgFunc = objText.getAttribute('onchange');
        var fieldName = orgFunc.replace('TextChange(', '').split(',')[0].replace(/'/g, '');

        objText.addEventListener('focus', function (objText) {
            return function () {
                var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                if (hdnObj !== null) {
                    hdnObj.value = '';
                }
                objText._oldvalue = objText.value;
            };
        }(objText),false);

        objText.addEventListener('blur', function (objText, fieldName) {
            return function () {
                if (objText._oldvalue !== objText.value) {
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
                    textChange(fieldName);
                }
            };
        }(objText, fieldName), false);

        objText.removeAttribute('onchange');
    }
    /* **********************************
     * 詳細（入力ボックス）開閉オプション処理 
    ********************************** */
    // 必要オブジェクトの取得
    var objIsHideDetailBox = document.getElementById('hdnIsHideDetailBox');
    var objListNoHeaderTdFindResult = document.querySelectorAll('div[id="pnlListArea_HL"] table tr:nth-child(1) th:nth-child(1)');
    if (objIsHideDetailBox === null || objListNoHeaderTdFindResult.length === 0) {
        return;
    }
    // 表示・非表示ボックスの追加
    var objListNoHeaderTd = objListNoHeaderTdFindResult[0];
    var divShowHideControl = document.createElement('div');
    //divShowHideControl.textContent = 'HideDetail';
    divShowHideControl.id = 'divPnlListArea_HLShowHideDetailBox';
    objListNoHeaderTd.insertAdjacentHTML('afterbegin', divShowHideControl.outerHTML);
    // 表示・非表示ボックスのイベントバインド
    divShowHideControl = document.getElementById('divPnlListArea_HLShowHideDetailBox');
    divShowHideControl.addEventListener('click', function () { masterShowHideDetailClick(this); },false);
    masterShowHideDetail(objIsHideDetailBox.value, divShowHideControl);
    // 位置調整
    var detailRepObjects = document.querySelectorAll('span[class^="WF_DViewRep"]');
    for (let i = 0; i < detailRepObjects.length; i++) {
        detailRepObjects[i].scrollLeft = 0;
    }
});
/**
 * オブジェクトを無効化
 * @return {undefined} なし
 * @description マスターロード時に呼び出す(本来は↑の関数でコールしたいが修正前のロジックが変なため
 * 単純に移行)
 */
function masterDisableObjects() {
    // 活性非活性の管理オブジェクトを取得
    var permitCodeObj = document.getElementById('hdnMAPpermitCode');
    if (permitCodeObj === null) {
        return; //存在しない場合はそのまま終了
    }
    var dbUpdBtnObj = document.getElementById('btnDbUpdate');
    var lstUpdBtnObj = document.getElementById('btnListUpdate');
    if (dbUpdBtnObj === null || lstUpdBtnObj === null) {
        return;
    }

    if (permitCodeObj.value === 'TRUE') {
        dbUpdBtnObj.disabled = "";
    } else {
        dbUpdBtnObj.disabled = "disabled";
        lstUpdBtnObj.disabled = "disabled";
    }
}
/**
 *  詳細エリアのタブ変更時イベント
 * @param {string} tabNo タブNo
 * @return {undefined} なし
 * @description 詳細エリアのタブ変更時イベント
 */
function masterDtabChange(tabNo) {
    var objSubmit = document.getElementById('hdnSubmit');
    var objDtabChenge = document.getElementById('hdnDTABChange');
    // 対象のオブジェクトが存在していない場合は終了
    if (objSubmit === null || objDtabChenge === null) {
        return;
    }
    // SUBMITフラグを見て処理実行
    if (objSubmit.value === 'FALSE') {
        objSubmit.value = 'TRUE';
        objDtabChenge.value = tabNo;
        commonDispWait();
        document.forms[0].submit();
    }
}
/**
 *  上部一覧表のリストダブルクリックイベント
 * @param {object} obj TR(行)オブジェクト
 * @param {string} lineCnt 行No
 * @return {undefined} なし
 * @description 詳細エリアのタブ変更時イベント
 */
function ListDbClick(obj, lineCnt) {
    var objSubmit = document.getElementById('hdnSubmit');
    var objListDbClick = document.getElementById('hdnListDbClick');
    // 対象のオブジェクトが存在していない場合は終了
    if (objSubmit === null || objListDbClick === null) {
        return;
    }
    // SUBMITフラグを見て処理実行
    if (objSubmit.value === 'FALSE') {
        objSubmit.value = 'TRUE';
        objListDbClick.value = lineCnt;
        var objIsHideDetailBox = document.getElementById('hdnIsHideDetailBox');
        if (objIsHideDetailBox !== null) {
            objIsHideDetailBox.value = '0';
        }
        commonDispWait();
        document.forms[0].submit();
    }
}
/**
 *  下部詳細のテキストフィールドダブルクリックイベント
 * @param {string} activeViewId オープンする左ボックスのVIEWID
 * @param {string} dbClickField 行No
 * @return {undefined} なし
 * @description 
 */
function Field_DBclick(activeViewId, dbClickField) {
    var objSubmit = document.getElementById('hdnSubmit');
    var objActiveViewId = document.getElementById('hdnLeftboxActiveViewId');
    var objDbClickField = document.getElementById('hdnTextDbClickField');
    var objIsLeftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
    // 対象のオブジェクトが存在していない場合は終了
    if (objSubmit === null || objActiveViewId === null || objDbClickField === null || objIsLeftBoxOpen === null) {
        return;
    }

    if (objSubmit.value === 'FALSE') {
        objSubmit.value = 'TRUE';
        objActiveViewId.value = activeViewId;
        objDbClickField.value = dbClickField;
        objIsLeftBoxOpen.value = 'Open';
        commonDispWait();
        document.forms[0].submit();
    }
}
/**
 *  下部詳細のテキストフィールドダブルクリックイベント
 * @param {string} textField 対象のフィールド名
 * @return {undefined} なし
 * @description 
 */
function textChange(textField) {
    var objSubmit = document.getElementById('hdnSubmit');
    var objOnchangeField = document.getElementById('hdnOnchangeField');
    if (objSubmit.value === "FALSE") {
        objSubmit.value = "TRUE";
        objOnchangeField.value = textField;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
}
/**
 *  ダウンロード処理(Excel)
 * @return {undefined} なし
 * @description 
 */
function f_ExcelPrint() {
    var objPrintUrl = document.getElementById("hdnPrintURL");
    if (objPrintUrl === null) {
        return;
    }
    // リンク参照
    window.open(objPrintUrl.value, "view", "_blank");
}
/**
 * マスタ関連D&Dイベントバインド
 * @param {Array} dragDropAreaObjectsList イベントバインドするドラッグエリアの情報
 * @param {string} handlerUrl ashxのURL
 * @return {undefined} なし
 * @description 
 */
function bindMasterDragDropEvents(dragDropAreaObjectsList, handlerUrl) {
    // バインドオブジェクトが存在しない場合はそのまま終了
    if (dragDropAreaObjectsList === null) {
        return;
    }
    // バインドオブジェクト情報のループ
    for (let i = 0; i < dragDropAreaObjectsList.length; i++) {
        var bindObjInfo = dragDropAreaObjectsList[i];
        var objDDArea = document.getElementById(bindObjInfo.id);
        if (objDDArea === null) {
            continue;
        }
        // 画面内の文字を選択しドラッグ＆ドロップを抑止しつつ、ファイルのドラッグ＆ドロップを可能にする。
        objDDArea.addEventListener('dragstart', function (event) { mastertDragEventCancel(event); }, false);
        objDDArea.addEventListener('dragenter', function (event) { mastertDragEventCancel(event); }, false);
        objDDArea.addEventListener('dragover', function (event) { mastertDragOverEvent(event); }, false);
        objDDArea.addEventListener('dragleave', function (event) { mastertDragEventCancel(event); }, false);
        objDDArea.addEventListener('drag', function (event)  { mastertDragEventCancel(event); }, false);
        objDDArea.addEventListener('drop', function (bindObjInfo, handlerUrl) {
            return function (event) {
                masterDropEvent(event, bindObjInfo.kbn, bindObjInfo.acceptExtentions, handlerUrl);
            };
        }(bindObjInfo, handlerUrl), false);

        objDDArea.addEventListener('dragend', function (event) { mastertDragEventCancel(event); }, false);
    }
}

/**
 * ドロップ処理（ドラッグドロップ入力）
 * @param {Event} e ドラッグイベントオブジェクト
 * @param {string} kbn ドラッグイベントオブジェクト
 * @param {Array} acceptExtentions 許可拡張子配列(未設定時は全対象)
 * @param {string} handlerUrl ashxのURL
 * @return {undefined} なし
 * @description
 */
function masterDropEvent(e, kbn, acceptExtentions, handlerUrl) {
    e.preventDefault();
    commonDispWait();
    // ********************************
    // フッターボックスのオブジェクト取得
    // ********************************
    var footerMsg = document.getElementById("lblFooterMessage");
    // ********************************
    // メッセージの取得
    // ********************************
    var messageList = new Array(6);
    var stMsgObj = document.getElementById('hdnUploadMessage01');
    messageList[0] = '';
    if (stMsgObj !== null) {
        messageList[0] = stMsgObj.value;
    }
    for (let i = 1; i < 6; i++) {
        var tmpObj = document.getElementById('hdnUploadError0' + i);
        if (tmpObj !== null) {
            messageList[i] = tmpObj.value;
        } else {
            messageList[i] = '';
        }
    }
    if (document.getElementById('hdnMAPpermitCode').value === "TRUE") {
        footerMsg.textContent = messageList[0];
        footerMsg.removeAttribute("class");
        footerMsg.classList.add('INFORMATION');
        // ドラッグされたファイル情報を取得
        var files = e.dataTransfer.files;

        // 送信用FormData オブジェクトを用意
        var fd = new FormData();
        // 許可拡張子の正規表現文字生成
        var regString = "";
        if (acceptExtentions !== null) {
            // acceptExtentionsがない場合は拡張子制限なし
            regString = "^.*$"; 
        } else {
            // 許可拡張子を元に正規表現文字を生成
            for (var i = 0; i < acceptExtentions.length; i++) {
                if (regString === '') {
                    regString = '^.*\.' + acceptExtentions[i] + '$';
                } else {
                    regString = regString + '|' + '^.*\.' + acceptExtentions[i] + '$';
                }
            }
        }
        // 正規表現オブジェクトの生成
        var reg = new RegExp(regString);

        for (let i = 0; i < files.length; i++) {
            if (files[i].name.toLowerCase().match(reg)) {
                fd.append("files", files[i]);
            } else {
                footerMsg.textContent = messageList[5];
                footerMsg.removeAttribute("class");
                footerMsg.classList.add('ABNORMAL');
                commonHideWait();
                return;
            }
        }

        // XMLHttpRequest オブジェクトを作成
        var xhr = new XMLHttpRequest();

        // ドロップファイルによりURL変更
        // 「POST メソッド」「接続先 URL」を指定
        xhr.open("POST", handlerUrl, false);

        // イベント設定
        // ⇒XHR 送信正常で実行されるイベント
        xhr.onload = function (e) {
            if (e.currentTarget.status === 200) {

                if (kbn === "FILE_UP") {
                    document.getElementById("hdnListUpload").value = "PDF_LOADED";
                } else {
                    document.getElementById("hdnListUpload").value = "XLS_LOADED";
                }
                document.forms[0].submit();                             //aspx起動
            } else {
                footerMsg.textContent = messageList[1];
                footerMsg.removeAttribute("class");
                footerMsg.classList.add('ABNORMAL');
                commonHideWait();
            }
        };

        // ⇒XHR 送信ERRで実行されるイベント
        xhr.onerror = function (e) {
            footerMsg.textContent = messageList[1];
            footerMsg.removeAttribute("class");
            footerMsg.classList.add('ABNORMAL');
            commonHideWait();
        };

        // ⇒XHR 通信中止すると実行されるイベント
        xhr.onabort = function (e) {
            footerMsg.textContent = messageList[2];
            footerMsg.removeAttribute("class");
            footerMsg.classList.add('ABNORMAL');
            commonHideWait();
        };

        // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
        xhr.ontimeout = function (e) {
            footerMsg.textContent = messageList[3];
            footerMsg.removeAttribute("class");
            footerMsg.classList.add('ABNORMAL');
            commonHideWait();
        };

        // 「送信データ」を指定、XHR 通信を開始する
        xhr.send(fd);
    } else {
        footerMsg.textContent = messageList[4];
        footerMsg.removeAttribute("class");
        footerMsg.classList.add('ABNORMAL');
        commonHideWait();
    }

}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function mastertDragOverEvent(event) {
    //event.preventDefault();  //イベントをキャンセル
    event.preventDefault();
    event.dataTransfer.dropEffect = 'copy'; //ドラッグする文言を変更 CHROMEのみワーク
}
/**
 * ドロップ処理（処理抑止）
 * @param {Event} event ドラッグイベントオブジェクト
 * @return {undefined} なし
 * @description
 */
function mastertDragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
}
/**
 * ディテール(PDF内容表示)処理
 * @param {string} filename ファイル名
 * @return {undefined} なし
 * @description
 */
function DtabPDFdisplay(filename) {
    var objSubmit = document.getElementById('hdnSubmit');
    var objDtabPdfExcelDisp = document.getElementById('hdnDTABPDFEXCELdisplay');
    if (objSubmit === null || objDtabPdfExcelDisp === null) {
        return;
    }

    if (objSubmit.value === "FALSE") {
        objSubmit.value = "TRUE";
        objDtabPdfExcelDisp.value = filename;
        commonHideWait();
        document.forms[0].submit();                            //aspx起動
    }
}

/**
 * ディテール(PDF内容表示)処理
 * @return {undefined} なし
 * @description
 */
function PDFselectChange() {
    var objSubmit = document.getElementById('hdnSubmit');
    var objDtabPdfExcelChange = document.getElementById('hdnDTABPDFEXCELchange');
    if (objSubmit === null || objDtabPdfExcelChange === null) {
        return;
    }

    if (objSubmit.value === "FALSE") {
        objSubmit.value = "TRUE";
        objDtabPdfExcelChange.value = "Change";
        commonHideWait();
        document.forms[0].submit();                            //aspx起動
    }
}
/**
 * 詳細開閉処理
 * @param {object} callObj 呼出し元
 * @return {undefined} なし
 * @description
 */
function masterShowHideDetailClick(callObj) {
    var objIsHideDetailBox = document.getElementById('hdnIsHideDetailBox');
    if (objIsHideDetailBox === null) {
        return;
    }

    if (objIsHideDetailBox.value === '1') {
        objIsHideDetailBox.value = '0';
    } else {
        objIsHideDetailBox.value = '1';
    }
    masterShowHideDetail(objIsHideDetailBox.value,callObj);
}
function masterShowHideDetail(isHide, callObj) {
    var objHeaderBox = document.getElementById('headerbox');
    var objDetailBox = document.getElementById('detailbox');
    if (isHide === '1') {
        callObj.classList.remove("hideDetail");
        callObj.classList.add("showDetail");
        objHeaderBox.style.height = '100%';
        objDetailBox.style.display = 'none';
    } else {
        callObj.classList.remove("showDetail");
        callObj.classList.add("hideDetail");
        objHeaderBox.style.height = null;
        objDetailBox.style.display = 'block';
    }
}