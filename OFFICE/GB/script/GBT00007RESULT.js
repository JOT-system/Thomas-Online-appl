// 必要な場合適宜関数、処理を追加
function f_ExcelPrint() {
    // リンク参照
    var printUrlObj = document.getElementById("hdnPrintURL");
    if (printUrlObj === null) {
        return;
    }
    window.open(printUrlObj.value, "view", "_blank");
    printUrlObj.value = '';
}
// ○GridView処理（マウスホイール処理）
function f_MouseWheel(event) {
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE';
        if (window.event.wheelDelta < 0) {
            document.getElementById('hdnMouseWheel').value = "+";
        } else {
            document.getElementById('hdnMouseWheel').value = '-';
        }
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    }
}
function $E(id) { return document.getElementById(id); }
function scroll() {
    $E('WF_LISTAREA_HR').scrollLeft = $E('WF_LISTAREA_DR').scrollLeft;// 左右連動させる
    $E('WF_LISTAREA_DL').scrollTop = $E('WF_LISTAREA_DR').scrollTop;// 上下連動させる
}
// ○一覧用処理
function ListDbClick(obj, LineCnt) {
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE';
        document.getElementById('hdnListDBclick').value = LineCnt;
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    }
}
// 〇一覧ボタンイベントバインド
function bindGridButtonClickEvent() {
    var rowHeaderObj = document.getElementById('WF_LISTAREA_DL');
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
    var colCond = "th[cellfiedlname='BLISSUE'],th[cellfiedlname='ODID'],th[cellfiedlname^='ETD'],th[cellfiedlname^='ETA'],th[cellfiedlname='CONSIGNEE']";
    var rightHeaderNode = document.getElementById('WF_LISTAREA_HR').getElementsByTagName('table')[0];
    var showDisplayFieldHeaderNode = rightHeaderNode.querySelectorAll(colCond);
    var showDisplayFieldCellIndexList = [];
    if (showDisplayFieldHeaderNode !== null) {
        for (let i = 0; i < showDisplayFieldHeaderNode.length; i++) {
            showDisplayFieldCellIndexList.push(showDisplayFieldHeaderNode[i].cellIndex);
        }
    }
    var leftHeaderNode = document.getElementById('WF_LISTAREA_HL').getElementsByTagName('table')[0];
    showDisplayFieldHeaderNode = leftHeaderNode.querySelectorAll(colCond);
    var showDisplayLeftFieldCellIndexList = [];
    if (showDisplayFieldHeaderNode !== null) {
        for (let i = 0; i < showDisplayFieldHeaderNode.length; i++) {
            showDisplayLeftFieldCellIndexList.push(showDisplayFieldHeaderNode[i].cellIndex);
        }
    }
    var rightNode = document.getElementById('WF_LISTAREA_DR').getElementsByTagName('table')[0];
    var leftDataTable = rowHeaderObj.getElementsByTagName('table')[0];
    for (let i = 0; i < buttonList.length; i++) {
        var buttonObj = buttonList[i];
        var tdNode = buttonObj.parentNode;
        var trNode = tdNode.parentNode;

        var rightTrNode = rightNode.rows[trNode.rowIndex];
        var leftTrNode = leftDataTable.rows[trNode.rowIndex];
        /* オーダー情報の場合 */
        //tdNode.removeChild(buttonObj); /* ノードよりボタンを削除 */
        trNode.dataset.orderRow = '1';
        rightTrNode.dataset.orderRow = '1';
        if (showDisplayFieldCellIndexList.length > 0) {
            for (let cellIdx = 0; cellIdx < showDisplayFieldCellIndexList.length; cellIdx++) {
                rightTrNode.cells[showDisplayFieldCellIndexList[cellIdx]].dataset.showcell = '1';
            }

        }
        if (showDisplayLeftFieldCellIndexList.length > 0) {
            for (let cellIdx = 0; cellIdx < showDisplayLeftFieldCellIndexList.length; cellIdx++) {
                leftTrNode.cells[showDisplayLeftFieldCellIndexList[cellIdx]].dataset.showcell = '1';
            }
        }
        /* IEの場合動的に変更したAttributeを使用してCSSが反映されないため再代入 */
        trNode.innerHTML = trNode.innerHTML;
        rightTrNode.innerHTML = rightTrNode.innerHTML;

        /* クリックイベントに紐づけ */
        let buttonId = buttonObj.id;
        buttonObj = document.getElementById(buttonId);
        buttonObj.dataset.orderRow = '1';
        buttonObj.onclick = (function (buttonObj) {
            return function () {
                listButtonClick(buttonObj);
                return false;
            };
        })(buttonObj);
        buttonObj.onmouseover = (function () {
            return function () {
                window.status = "";
                return true;
            };
        })();



    }
}
// 〇一覧ボタンクリックイベント
function listButtonClick(obj) {
    var currentRowNum = obj.getAttribute('rownum');
    var isOrderRow = '1';
    var clickButtonName = 'btnListDelete';



    /* オーダー削除確認ボックスの表示 */
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        /* クリック行のノンブレーカID(オーダーNo)を取得 */
        var colCond = "th[cellfiedlname='NONBRID']";
        var leftHeaderNode = document.getElementById('WF_LISTAREA_HL').getElementsByTagName('table')[0];
        var targetHeaderNode = leftHeaderNode.querySelectorAll(colCond);

        var cellIndex = targetHeaderNode[0].cellIndex;
        var leftDataNode = document.getElementById('WF_LISTAREA_DL').getElementsByTagName('table')[0];
        var clickTableRow = obj.parentNode.parentNode.rowIndex;
        var selectedOrderId = leftDataNode.rows[clickTableRow].cells[cellIndex].textContent;

        /* 削除確認ポップアップ表示 */
        var confirmObj = document.getElementById('divConfirmBoxWrapper');
        confirmObj.style.display = 'block';
        var confirmOkButton = document.getElementById('btnConfirmOk');
        confirmOkButton.dataset.rowNum = currentRowNum;
        confirmOkButton.dataset.buttonName = clickButtonName;
        var confirmOrderNoObj = document.getElementById('lblConfirmOrderNo');
        confirmOrderNoObj.textContent = selectedOrderId;
        /* 確認メッセージクリックボタン押下時イベントバインド */
        confirmOkButton.onclick = (function (confirmOrderNoObj) {
            return function () {
                document.getElementById('hdnSubmit').value = 'TRUE';
                var objCurrentRowNum = document.getElementById('hdnListCurrentRownum');
                var objButtonClick = document.getElementById('hdnButtonClick');
                objCurrentRowNum.value = this.dataset.rowNum;
                objButtonClick.value = this.dataset.buttonName;
                commonDispWait();
                document.forms[0].submit();                             //aspx起動
                return false;
            };
        })(confirmOrderNoObj);
    }

    return false;
}
function bindApplyComment() {
    let listRightData = document.getElementById('WF_LISTAREA_DR');
    if (listRightData !== null) {
        /* 承認備考欄 */
        var applyReasonTextList = listRightData.querySelectorAll("input[type=text][id^='txtWF_LISTAREAAPPLYTEXT']");
        if (applyReasonTextList !== null) {
            var wrapperMain = document.createElement('div');
            for (let i = 0; i < applyReasonTextList.length; i++) {
                let applyReasonTextObj = applyReasonTextList[i];
                applyReasonTextObj.readOnly = 'true';
                /* ダブルクリックイベントバインド */
                // フォーカスを合わさないとテキストボックスはダブルクリックに反応しないため、
                // 前後をspanタグで括りそちらにダブルクリックイベントを記載
                let wrapper = wrapperMain.cloneNode(true);
                //wrapper.id = "spn" + applyReasonTextObj.id;
                wrapper.appendChild(applyReasonTextObj.cloneNode(true));
                applyReasonTextObj.parentNode.replaceChild(wrapper, applyReasonTextObj);
                /* ダブルクリックイベントに紐づけ */
                wrapper.addEventListener('dblclick', (function (applyReasonTextObj) {
                    return function (e) {
                        //applyReasonTextObj.value = "test";
                        e.stopPropagation();
                        displayApplyReasonbox(applyReasonTextObj);
                    };
                })(applyReasonTextObj), false);
            }
        }
    }
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