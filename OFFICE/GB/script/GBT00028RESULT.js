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

/* 一覧チェックボックスイベント */
function f_checkEvent(obj) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
};

// 全選択チェック変更
function f_checkAllSelectEvent(event) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById('hdnAllSelectCheckChange').value = "TRUE";
        document.getElementById('hdnAllSelectCheckValue').value = event.target.checked;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
};


/* InvoiceNewボタン処理 */
var dispInvoiceNewTimeOut = 100;
var mbuttonAreaObj;
var mInvoiceNewMouseOverObj;
var setTimeToHideID;

function bindDisplayInvoiceNewBtn() {
    var spnMouseOverObj = document.getElementById('lblInvoiceNew');
    var divButtonItemAreaObj = document.getElementById('divInvoiceItems');
    if (spnMouseOverObj === null || divButtonItemAreaObj === null) {
        return;
    }
    spnMouseOverObj.onmouseover = function () { displayInvoiceNewBtn(divButtonItemAreaObj.id); };
    spnMouseOverObj.onmouseout = function () { hideInvoiceNewBtnTimer(); }
    divButtonItemAreaObj.onmouseover = function () { resetHideInvoiceNewBtnTimer(); };
    divButtonItemAreaObj.onmouseout = function () { hideInvoiceNewBtnTimer(); }
}

function displayInvoiceNewBtn(objId) {
    if (mbuttonAreaObj) {
        mbuttonAreaObj.style.display = 'none';
        mInvoiceNewMouseOverObj.style.backgroundColor = "";
    }
    mbuttonAreaObj = document.getElementById(objId);
    mbuttonAreaObj.style.display = 'block';
    mInvoiceNewMouseOverObj = document.getElementById('lblInvoiceNew');
    mInvoiceNewMouseOverObj.style.backgroundColor = "#DE9292";
}

function hideInvoiceNewBtnTimer() {
    setTimeToHideID = window.setTimeout(hideInvoiceNewBtn, dispInvoiceNewTimeOut);
}

function hideInvoiceNewBtn() {
    mbuttonAreaObj.style.display = 'none';
    mInvoiceNewMouseOverObj.style.backgroundColor = "";
}

function resetHideInvoiceNewBtnTimer() {
    if (setTimeToHideID) {
        window.clearTimeout(setTimeToHideID);
        setTimeToHideID = 0;
    }
}
/* 金額項目の通貨編集（ロード時） */
function formatAmount() {
    var cColumnObj = getTargetColumnNoTable('ACCCURRENCYSEGMENT', 'WF_LISTAREA');
    var aColumnObj = getTargetColumnNoTable('INVOICEAMOUNT', 'WF_LISTAREA');
    var tColumnObj = getTargetColumnNoTable('TAXAMT', 'WF_LISTAREA');
    var nColumnObj = getTargetColumnNoTable('NONTAXAMT', 'WF_LISTAREA');
    //対象のカラムが存在していない場合は実行不可能
    if (cColumnObj !== null && aColumnObj !== null && tColumnObj !== null && nColumnObj !== null) {
        let cColumnNo = cColumnObj.ColumnNo;
        let cTable = cColumnObj.TargetTable;
        let aColumnNo = aColumnObj.ColumnNo;
        let tColumnNo = tColumnObj.ColumnNo;
        let nColumnNo = nColumnObj.ColumnNo;
        let aTable = aColumnObj.TargetTable;
        if (cTable.rows.length !== 0) {
            for (let i = 0; i < cTable.rows.length; i++) {
                let cValueObj = cTable.rows[i].cells[cColumnNo];
                var cValue = cValueObj.textContent;
                let aValueObj = aTable.rows[i].cells[aColumnNo].querySelectorAll('input[type=text]')[0];
                let aValue = aValueObj.value;
                let tValueObj = aTable.rows[i].cells[tColumnNo].querySelectorAll('input[type=text]')[0];
                let tValue = tValueObj.value;
                let nValueObj = aTable.rows[i].cells[nColumnNo].querySelectorAll('input[type=text]')[0];
                let nValue = nValueObj.value;

                aValueObj.value = changeCurrency(cValue, aValue)
                tValueObj.value = changeCurrency(cValue, tValue)
                nValueObj.value = changeCurrency(cValue, nValue)
            }
        }
    }
}
function changeCurrency(currency, num) {
    var num = Number(num.replace(/[^0-9.-]+/g, ""));

    if (currency == 'JPY') {
        return Number(num).toLocaleString('ja-JP', { style: 'currency', currency: 'JPY' });
    } else if (currency == 'USD') {
        return Number(num).toLocaleString('en-US', { style: 'currency', currency: 'USD' });
    } else {
        num = ''
        return num
    }
}
