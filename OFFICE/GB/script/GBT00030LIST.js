//// ○一覧用処理
//function ListDbClick(obj, LineCnt) {
//    if (document.getElementById('hdnSubmit').value == 'FALSE') {
//        document.getElementById('hdnSubmit').value = 'TRUE'
//        document.getElementById('hdnListDBclick').value = LineCnt;
//        commonDispWait();
//        document.forms[0].submit();                             //aspx起動
//    };
//};
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

    // 多段ヘッダースクロール同期
    var hlistObj = document.getElementById(WF_LISTAREA_H.id);
    if (hlistObj === null) {
        return;
    }
    var hrightHeaderTableObj = document.getElementById(hlistObj.id + '_HR');
    hrightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft; // 左右連動させる
    var h1listObj = document.getElementById(WF_LISTAREA_H1.id);
    if (h1listObj === null) {
        return;
    }
    var h1rightHeaderTableObj = document.getElementById(h1listObj.id + '_HR');
    h1rightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft; // 左右連動させる

};
