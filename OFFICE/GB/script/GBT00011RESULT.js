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
    var colCond = "th[cellfiedlname='BLISSUE'],th[cellfiedlname='ODID'],th[cellfiedlname^='ETD'],th[cellfiedlname^='ETA'],th[cellfiedlname='CONSIGNEE'],th[cellfiedlname='NOOFTANKS']";
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
        var brOdFlgCell = trNode.cells[3];

        var rightTrNode = rightNode.rows[trNode.rowIndex];
        var leftTrNode = leftDataTable.rows[trNode.rowIndex];
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