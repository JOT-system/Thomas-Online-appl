// ○一覧用処理
function ListCellClick(obj, LineCnt, Column) {
    if (document.getElementById('hdnSubmit').value == 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE'
        document.getElementById('hdnListDBclick').value = LineCnt;
        document.getElementById('hdnListCellclick').value = Column;
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    };
};

// ○一覧スクロール処理
function commonListScroll(listObj) {
    var rightHeaderTableObj = document.getElementById(listObj.id + '_HR');
    var rightDataTableObj = document.getElementById(listObj.id + '_DR');
    var leftDataTableObj = document.getElementById(listObj.id + '_DL');

    setCommonListScrollXpos(listObj.id, rightDataTableObj.scrollLeft);
    rightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft; // 左右連動させる
    leftDataTableObj.scrollTop = rightDataTableObj.scrollTop; // 上下連動させる

    // ２段ヘッダースクロール同期
    var hlistObj = document.getElementById(WF_LISTAREA_H.id);
    var hrightHeaderTableObj = document.getElementById(hlistObj.id + '_HR');
    hrightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft; // 左右連動させる

};
// 〇一覧ボタンイベントバインド
function bindGridButtonClickEvent() {
    var rightHeaderObj = document.getElementById('WF_LISTAREA_HR');
    if (rightHeaderObj === null) {
        return; /* レンダリングされていない場合はそのまま終了 */
    }
    var buttonList = rightHeaderObj.querySelectorAll("button[id^='btnAll']");
    /* 対象のボタンが1件もない場合はそのまま終了 */
    if (buttonList === null) {
        return;
    }
    if (buttonList.length === 0) {
        return;
    }
    for (let i = 0; i < buttonList.length; i++) {
        var buttonObj = buttonList[0];

        /* クリックイベントに紐づけ */
        buttonObj.onclick = (function (buttonObj) {
            return function () {
                allButtonClick(buttonObj);
                return false;
            };
        })(buttonObj);
    }

};

// 〇一覧ヘッダALLボタンクリックイベント
function allButtonClick(obj) {
    var objButtonColumn = document.getElementById('hdnListCellclick');
    var objButtonClick = document.getElementById('hdnButtonClick');
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE'
        objButtonColumn.value = obj.getAttribute('cellfiedlname');
        objButtonClick.value = 'btnAllAction';
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
    };
    return false;
}
