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
function ListDbClick(obj, OrderNo) {
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE';
        document.getElementById('hdnListDBclick').value = OrderNo;
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