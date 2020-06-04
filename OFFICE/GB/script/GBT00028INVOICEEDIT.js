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
// ○一覧用処理
function ListDbClick(obj, LineCnt) {
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE';
        document.getElementById('hdnListDBclick').value = LineCnt;
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    }
}
// 〇一覧★ボタンクリックイベント
function listButtonClick(obj) {
    var currentRowNum = obj.getAttribute('rownum');
    var isOrderRow = obj.dataset.orderRow;
    var clickButtonName = 'btnListAction';
    if (isOrderRow === '1') {
        clickButtonName = 'btnListDelete';
    } else {
        isOrderRow = '0';
    }

    if (isOrderRow === '0') {
        /* ブレーカー単票表示 */
        var objCurrentRowNum = document.getElementById('hdnListCurrentRownum');
        var objButtonClick = document.getElementById('hdnButtonClick');
        if (document.getElementById('hdnSubmit').value === 'FALSE') {
            document.getElementById('hdnSubmit').value = 'TRUE';
            objCurrentRowNum.value = currentRowNum;
            objButtonClick.value = clickButtonName;
            commonDispWait();
            document.forms[0].submit();                             //aspx起動
        }
    } else {
        /* オーダー削除確認ボックスの表示 */
        if (document.getElementById('hdnSubmit').value === 'FALSE') {
            /* クリック行のオーダーNoを取得 */
            var colCond = "th[cellfiedlname='ODID']";
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
    }
    return false;
}
// 〇ブレーカー単票オープン処理
function openBreakerWindow() {
    var wholeDiv = document.createElement("div");
    wholeDiv.id = 'divBreakerInfo';
    var iFrameBreaker = document.createElement("iframe");
    iFrameBreaker.id = 'ifraBreakerInfo';
    iFrameBreaker.setAttribute('frameborder', '0');
    iFrameBreaker.src = 'about:blank';
    wholeDiv.appendChild(iFrameBreaker);
    //一旦div及び空のiframeを生成（生成しないと後述の処理が機能しない)
    document.body.appendChild(wholeDiv);

    // iframe内に生成した空ページにFormを作成しブレーカー単票画面にポスト
    iFrameBreaker = document.getElementById('ifraBreakerInfo');
    // iframeのロード時イベント設定(発着タブの切り替えなどでポストバック後ロードも担保)
    iFrameBreaker.onload = (function (iFrameBreaker) {
        return function () {
            var backBtn = iFrameBreaker.contentWindow.document.getElementById('btnBack');
            if (backBtn === null) {
                return;
            }
            // ブレーカー単票戻るボタンイベントをクリア
            backBtn.outerHTML = backBtn.outerHTML;
            backBtn = iFrameBreaker.contentWindow.document.getElementById('btnBack');
            // 戻るボタンを生成したIFrameを削除する用クリックイベント変更
            backBtn.onclick = (function (iFrameBreaker) {
                return function () {
                    var parentDiv = iFrameBreaker.parentNode;
                    parentDiv.parentNode.removeChild(parentDiv);
                };
            })(iFrameBreaker);

        };
    })(iFrameBreaker);

    var brUrl = document.getElementById('hdnBreakerViewUrl').value; // ポストするURL
    var brId = document.getElementById('hdnSelectedBrId').value; // ポストするブレーカーID

    var frmBr = iFrameBreaker.contentWindow.document.createElement("form");
    frmBr.action = brUrl;
    frmBr.target = "_self";
    frmBr.method = 'post';
    // POSTする引き渡し情報を生成
    var frmId = document.forms[0].id;
    var qs = [{ type: 'hidden', name: 'hdnSender', value: frmId }, { type: 'hidden', name: 'hdnBrIdFromOrderList', value: brId }];
    for (var i = 0; i < qs.length; i++) {
        var ol = qs[i];
        var brinput = iFrameBreaker.contentWindow.document.createElement("input");

        for (var p in ol) {
            brinput.setAttribute(p, ol[p]);
        }
        frmBr.appendChild(brinput);
    }
    // 空ウィンドウに作成したformをbodyに追加して、サブミットする。その後、formを削除
    var brbody = iFrameBreaker.contentWindow.document.getElementsByTagName("body")[0];
    brbody.appendChild(frmBr);

    iFrameBreaker.contentWindow.document.forms[0].submit();
}
// 必要な場合適宜関数、処理を追加
// 添付ファイル一覧、添付ファイル名ダブルクリック時
function dispAttachmentFile(filename) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById('hdnFileDisplay').value = filename;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
}
/* ブレーカー備考欄 */
function bindRemarkDblClick() {
    var dblClickRemarkObjects = [
    ['spnApplyRemarks', 'lblApplyRemarks'],
    ['spnAppJotRemarks', 'lblAppJotRemarks']
    ];
    for (let i = 0; i < dblClickRemarkObjects.length; i++) {
        /* ダブルクリックオブジェクト */
        var obj = document.getElementById(dblClickRemarkObjects[i][0]);
        var lblObj = document.getElementById(dblClickRemarkObjects[i][1]);
        /* オブジェクトの存在チェック(存在しない場合はスキップ) */
        if (obj === null || lblObj === null) {
            continue;
        }

        /* ダブルクリックイベントにバインド */
        obj.addEventListener('dblclick', (function (lblObj) {
            return function () {
                displayRemarkbox(lblObj);
            };
        })(lblObj), false);
    }

}
/* 備考欄ダブルクリックイベント */
function displayRemarkbox(obj) {
    var remarkBoxOpenObj = document.getElementById('hdnRemarkboxOpen');
    var submitObj = document.getElementById('hdnSubmit');
    var remarkBoxRemarkField = document.getElementById('hdnRemarkboxField');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex');
    var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');

    /* 表示切替 */
    if (submitObj !== 'FALSE' || remarkBoxOpenObj.value !== 'Open') {
        submitObj.value = 'TRUE';
        var fieldDisplayNameObj = document.getElementById('hdnRemarkboxFieldName');
        currentUnieuqIndexObj.value = '';
        remarkBoxRemarkField.value = obj.id;
        var fieldDisplayName = '';
        switch (remarkBoxRemarkField.value) {
            case "lblApplyRemarks":
                fieldDisplayName = document.getElementById('lblApply').innerText;
                fieldDisplayName = fieldDisplayName + ' ' + document.getElementById('lblAppRemarksH').innerText;
                break;
            case "lblAppJotRemarks":
                fieldDisplayName = document.getElementById('lblApproved').innerText;
                fieldDisplayName = fieldDisplayName + ' ' + document.getElementById('lblAppRemarksH').innerText;
                break;
        }
        fieldDisplayNameObj.value = fieldDisplayName;
        remarkBoxOpenObj.value = 'Open';
        leftBoxOpen.value = ''; /* 右ボックスとの共存不可 */
        commonDispWait();
        document.forms[0].submit();

    }
}

/* 一覧チェックボックスイベント */
function f_checkEvent(obj) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        //document.getElementById('hdnCheckChange').value = obj.checked;
        //document.getElementById('hdnCheckUniqueIndex').value = obj.parentNode.dataset.uniqueindex;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
};