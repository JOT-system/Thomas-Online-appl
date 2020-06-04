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
// 〇一覧★ボタンクリックイベント
function listButtonClick(obj) {
    var currentRowNum = obj.getAttribute('rownum');
    var actType = obj.getAttribute('actType');
    var clickButtonName = obj.id;

    var objCurrentRowNum = document.getElementById('hdnListCurrentRownum');
    var objButtonClick = document.getElementById('hdnButtonClick');
    if (document.getElementById('hdnSubmit').value === 'FALSE') {
        document.getElementById('hdnSubmit').value = 'TRUE';
        objCurrentRowNum.value = currentRowNum;
        objButtonClick.value = clickButtonName;
        commonDispWait();
        document.forms[0].submit();                             //aspx起動
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