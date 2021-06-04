/**
 * @fileoverview JOTシステム共通JavaScript処理
 */
/* ドキュメントロード完了後イベントバインド */
window.addEventListener('load', function () {
    /* テキストボックスフォーカスがあった時点で選択 */
    var texboxObjList = document.querySelectorAll("input[type='text']");
    for (let i = 0; i < texboxObjList.length; i++) {
        texboxObjList[i].addEventListener('focus', function () {
            this.select();
        });
    }

    /* ブラウザ戻るボタンの禁止(無反応化) */
    if (window.history && window.history.pushState) {
        if (window == window.parent) {
            window.history.pushState(null, null, null);
            window.addEventListener('popstate', function (e) {
                if (!e.state) {
                    //. もう一度履歴を操作して終了
                    window.history.pushState(null, null, null);
                    window.history.pushState(null, null, null);
                    return false;
                }
            });
        } else {
            window.parent.history.pushState(null, null, null);
            window.parent.addEventListener('popstate', function (e) {
                if (!e.state) {
                    //. もう一度履歴を操作して終了
                    window.history.pushState(null, null, null);
                    window.parent.history.pushState(null, null, null);
                    return false;
                }
            });
            window.history.pushState(null, null, null);
            window.addEventListener('popstate', function (e) {
                if (!e.state) {
                    //. もう一度履歴を操作して終了
                    window.history.pushState(null, null, null);
                    window.parent.history.pushState(null, null, null);
                    return false;
                }
            });
        }
    }

    // ポストバック時のスクロール崩れ補正
    var contensBox = document.getElementById("divContensbox");
    var detailBox = document.getElementById("detailbox");
    var headerBox = document.getElementById("headerbox");
    if (contensBox !== null && detailBox !== null && headerBox !== null) {
        contensBox.scrollLeft = 0;
        detailBox.scrollLeft = 0;
        headerBox.scrollLeft = 0;
    }
    //commonCloseModal();
    //ポップアップ背面を使用不可に変更
    var popUpObj = document.getElementById('pnlCommonMessageWrapper');
    if (popUpObj !== null) {
        if (popUpObj.style.display !== 'none') {
            // 現在のフォーカスをポップアップに移動
            if (document.activeElement !== null) {
                document.activeElement.blur();
            }
            commonDisableModalBg(popUpObj.id);
            popUpObj.focus();
        }
    }

    // ICPリンク追加
    addIcpLink("lblFooterMessage");
});

/**
 * ICPリンクの配置
 * @param {target} ターゲットオブジェクト
 * @return {undefined} なし
 */
function addIcpLink(target) {
    var targetObj = document.getElementById(target);
    if (targetObj === null) {
        return;
    }
    // 中国proxyサイト経由時に追加
    if (location.hostname === "jotthomas.cn") {
        var addPosition = 'afterend';
        var addLink = '<div id="divIcpLink"><a href="https://beian.miit.gov.cn/" target="_blank" rel="noopener noreferrer">辽ICP备14000691号</a></div>';
        targetObj.insertAdjacentHTML(addPosition, addLink);
    }
    return;
 }

/**
 * ボタンクリックイベントをバインド
 * @param {object} targetButtonObjects クリックイベントを紐づけるボタンIDの配列
 * @return {undefined} なし
 */
function bindButtonClickEvent(targetButtonObjects) {
    // 引数未指定や配列がない場合は終了
    if (targetButtonObjects === null) {
        return;
    }
    if (targetButtonObjects.length === 0) {
        return;
    }
    //ボタンID配列のループ 
    for (let i = 0; i < targetButtonObjects.length; i++) {
        /* オブジェクトの存在チェック(存在しない場合はスキップ) */
        if (document.getElementById(targetButtonObjects[i]) === null) {
            continue;
        }
        var buttonId = targetButtonObjects[i];
        document.getElementById(buttonId).onmousedown = (function (buttonId) {
            return function () {
                var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                if (hdnObj === null) {
                    hdnObj = document.createElement('input');
                    hdnObj.type = 'hidden';
                    hdnObj.id = 'hdnClickButtonIdBeforeBlur';
                    document.forms[0].appendChild(hdnObj);
                    hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                }
                hdnObj.value = buttonId;
            };
        })(buttonId);
        /* クリックイベントに紐づけ */
        document.getElementById(buttonId).onclick = (function (buttonId) {
            return function () {
                buttonClick(buttonId);
            };
        })(buttonId);
    }
}
/**
 * ボタンクリックイベント
 * @param {string} buttonId クリックしたボタンID
 * @return {undefined} なし
 */
function buttonClick(buttonId) {
    /* 引数が未指定の場合そのまま終了 */
    if (buttonId === null || buttonId === '') {
        return; 
    }
    /* クリックしたIDを格納する隠しフィールド、およびサブミット隠しフィールドが存在しない場合終了 */
    var clickedButtonId = document.getElementById('hdnButtonClick');
    var submitObj = document.getElementById('hdnSubmit');
    if (clickedButtonId === null || submitObj === null) {
        return;
    }
    clickedButtonId.value = "";

    if (submitObj.value === 'FALSE') {
        submitObj.value = 'TRUE';
        clickedButtonId.value = buttonId;
        commonDispWait();
        document.forms[0].submit();
    } else {
        return;
    }

}

/**
 * 左ボックス表示処理のダブルクリックイベントバインド
 * @param {object} targetDblClickObjects オブジェクトのID,ダブルクリックで開くListViewのIDの二次元配列
 * @return {undefined} なし
 */
function bindLeftBoxShowEvent(targetDblClickObjects) {
    // 引数未指定や配列がない場合は終了
    if (targetDblClickObjects === null) {
        return;
    }
    if (targetDblClickObjects.length === 0) {
        return;
    }
    for (let i = 0; i < targetDblClickObjects.length; i++) {
        /* オブジェクトの存在チェック(存在しない場合はスキップ) */
        if (document.getElementById(targetDblClickObjects[i][0]) === null) {
            continue;
        }
        var inputObjectId = targetDblClickObjects[i][0];
        var inputTargetViewId = targetDblClickObjects[i][1];
        /* ダブルクリックイベントに紐づけ */
        var inputObject = document.getElementById(inputObjectId);
        var canEventBind = false;
        inputObject.autocomplete = 'off'; /* オートコンプリートをOFF */
        inputObject.placeholder = '';
        if (inputObject.disabled !== null) {
            if (inputObject.disabled !== 'disabled' && inputObject.disabled !== 'true' && inputObject.disabled !== true) {
                inputObject.placeholder = 'DoubleClick to select';
                canEventBind = true;
            }
        }
        if (canEventBind === true) {
            // フォーカスを合わさないとテキストボックスはダブルクリックに反応しないため、
            // 前後をspanタグで括りそちらにダブルクリックイベントを記載
            var wrapper = document.createElement('span'); 
            wrapper.appendChild(inputObject.cloneNode(true)); 
            inputObject.parentNode.replaceChild(wrapper, inputObject);

            wrapper.ondblclick = (function (inputObjectId, inputTargetViewId) {
                return function () {
                    fieldDBclick(inputObjectId, inputTargetViewId);

                };
            })(inputObjectId, inputTargetViewId);
        }
    }

}
/**
 * フィールドダブルクリックイベント
 * @param {string} fieldId=ダブルクリックを行ったフィールドID
 * @param {string} viewId=表示するビューのID
 * @return {undefined} なし
 */
function fieldDBclick(fieldId, viewId) {
    var submitObj = document.getElementById('hdnSubmit');
    var dblClickObject = document.getElementById('hdnTextDbClickField');
    var viewIdObject = document.getElementById('hdnLeftboxActiveViewId');
    var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
    if (submitObj === null || dblClickObject === null || viewIdObject === null || leftBoxOpen === null) {
        return;
    }

    // サブミットフラグが立っていない場合のみ実行
    if (submitObj.value === 'FALSE') {
        submitObj.value = 'TRUE';
        dblClickObject.value = fieldId;
        viewIdObject.value = viewId;
        leftBoxOpen.value = "Open";
        commonDispWait();
        document.forms[0].submit();
    }

}
/**
 * 右ボックス表示/非表示処理
 * @return {undefined} なし
 */
function displayLeftBox() {
    var leftBoxObj = document.getElementById('divLeftbox');
    var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
    if (leftBoxObj === null || leftBoxOpen === null) {
        return;
    }
    // 全部消す
    leftBoxObj.style.display = 'none';
    if (leftBoxOpen.value === 'Open') {
        leftBoxObj.style.display = 'block';
    }
}
/**
 * 右ボックス表示/非表示処理のダブルクリックイベントバインド
 * @return {undefined} なし
 */
function bindDiplayRightBoxEvent() {
    /* ************************ */
    /* 右ボックス表示関連
    /* ************************ */
    // 右ボックス表示処理のダブルクリックするオブジェクト 
    var rightBoxDblClickBindObjectsId = ['divShowRightBox',
                                         'lblFooterMessage'];
    //for (var i = 0, len = rightBoxDblClickBindObjectsId.length; i < len; ++i) {
    //    showRightBoxObj = document.getElementById(rightBoxDblClickBindObjectsId[i]);
    //    showRightBoxObj.ondblclick = function () {
    //            displayRightBox();
    //        };
    //}
    var showRightBoxObj = document.getElementById('divShowRightBox');
    showRightBoxObj.onclick = function () {
        displayRightBox();
    };
    showRightBoxObj = document.getElementById('lblFooterMessage');
    showRightBoxObj.ondblclick = function () {
        displayRightBox();
    };
}
/**
 * 右ボックス表示/非表示切替処理
 * @return {undefined} なし
 */
function displayRightBox() {
    var rightBoxOpenObj = document.getElementById('hdnRightboxOpen');
    var rightBoxObj = document.getElementById('divRightbox');
    /* 左ボックス及び左ボックスの状態を記録するHiddenFieldの存在チェック 
       存在しない場合は空振り 
    */
    if (rightBoxOpenObj === null || rightBoxObj === null) {
        return;
    }

    /* 表示非表示切替 */
    if (rightBoxObj.style.display !== 'block' || rightBoxObj.style.display === '') {
        rightBoxObj.style.display = 'block';
        rightBoxOpenObj.value = 'Open';
    } else {
        rightBoxObj.style.display = 'none';
        rightBoxOpenObj.value = '';
    }

}
/**
 * テキストボックス変更イベント紐づけ
 * @param {object} targetOnchangeObjects オブジェクトのID,ダブルクリックで開くListViewのIDの二次元配列
 * @return {undefined} なし
 */
function bindTextOnchangeEvent(targetOnchangeObjects) {
    var onchangeItemId = document.getElementById('hdnOnchangeField');
    var prvValueObj = document.getElementById('hdnOnchangeFieldPrevValue');
    var submitObj = document.getElementById('hdnSubmit');
    /* ************************ */
    /* 変更イベントをサーバーで受け取る
    /* ************************ */
    // 必須オブジェクトの条件がそろっていない場合は終了
    if (targetOnchangeObjects === null) {
        return;
    }
    if (targetOnchangeObjects.length === 0) {
        return;
    }
    if (onchangeItemId === null) {
        return;
    }
    if (prvValueObj === null) {
        return;
    }
    if (submitObj === null) {
        return;
    }
    // 変更時イベントを拾うオブジェクトのループ
    for (var i = 0, len = targetOnchangeObjects.length; i < len; ++i) {
        onchangeObj = document.getElementById(targetOnchangeObjects[i]);
        if (onchangeObj === null) {
            continue;
        }
        // 使用不可の場合はイベントバインドしない
        if (onchangeObj.disabled !== null) {
            if (onchangeObj.disabled === 'disabled') {
                continue;
            }
        }
        //
        onchangeObj.onfocus = (function (onchangeObj, prvValueObj) {
            return function () {
                var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                if (hdnObj !== null) {
                    hdnObj.value = '';
                }
                prvValueObj.value = onchangeObj.value; // 変更したフィールドのIDを記録
            };
        })(onchangeObj, prvValueObj);

        // イベントバインド
        onchangeObj.onblur = (function (onchangeObj, onchangeItemId) {
            return function () {
                var prevValueObj = document.getElementById('hdnOnchangeFieldPrevValue');
                var submitObj = document.getElementById('hdnSubmit');
                var footerMessageObj = document.getElementById('lblFooterMessage');
                if (footerMessageObj !== null) {
                    footerMessageObj.innerText = '';
                }
                if (prevValueObj.value !== onchangeObj.value) {
                    if (submitObj.value === 'FALSE') {
                        submitObj.value = 'TRUE';
                        onchangeItemId.value = onchangeObj.id; // 変更したフィールドのIDを記録
                        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
                        if (activeElem !== null || document.activeElement.id !== null) {
                            activeElem.value = document.activeElement.id;
                        }
                        prevValueObj.value = '';
                        let hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                        if (hdnObj !== null) {
                            let clickedButtonId = document.getElementById('hdnButtonClick');
                            clickedButtonId.value = hdnObj.value;
                            hdnObj.value = '';
                        }
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
                                    commonDispWait();
                                    document.forms[0].submit();  // サブミット
                                }
                            }
                        }
                    }
                }
            };

        })(onchangeObj, onchangeItemId);
    }
}
/**
 * 変更イベントを拾いポストバックをするとフォーカスを失うため保管
* @return {undefined} なし
 */
function focusAfterChange() {
    var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
    if (activeElem !== null) {
        if (activeElem.value !== '') {
            var targetForcusElm = document.getElementById(activeElem.value);
            activeElem.value = '';
            if (targetForcusElm !== null) {
                targetForcusElm.focus();
                if (targetForcusElm.type === 'text') {
                    targetForcusElm.select();
                }
            }
        }
    }
}
/**
 * 
 * 左ボックスのリストボックスオブジェクトダブルクリックイベントバインド
 * 左ボックス内(ID=divLeftbox)のリストボックス(selectタグ)を自動検索
 * リストボックスのダブルクリックイベントが
 * 左ボックス選択ボタンクリックイベントを呼び出す
 * 汎用的にしているため未存在チェックを精緻に行っています。
 * @return {undefined} なし
 */
function bindLeftListBoxDblClickEvent() {
    /* 選択ボタン */
    var btnSelectObj = document.getElementById('btnLeftBoxButtonSel');
    /* 選択ボタンオブジェクトが未存在またはdisabled=falseの場合はイベントをバインドしない */
    if (btnSelectObj === null) {
        return;
    }
    if (btnSelectObj.disabled !== null) {
        if (btnSelectObj.disabled === '' || btnSelectObj.disabled === 'disabled' || btnSelectObj.disabled === 'disabled') {
            return;
        }
    }
    /* 左ボックスエレメント */
    var leftBoxOjb = document.getElementById('divLeftbox');
    /* そもそも左ボックスが存在しない場合はそのまま終了 */
    if (leftBoxOjb === null) {
        return; 
    }
    /* 左ボックスエレメントよりselect(リストボックス)を検索 */
    var selectBoxList = leftBoxOjb.getElementsByTagName('select');
    /* selectタグが左ボックスに未存在（レンダリングされていない場合はスルー) */
    if (selectBoxList === null) {
        return;
    }
    if (selectBoxList.length === 0) {
        return;
    }
    var targetSelectBox = selectBoxList[0]; /* 一旦最初に見つかったselectタグ */
    for (var i = 0, len = selectBoxList.length; i < len; ++i) {
        /* IDのprefixがlbとついたリストボックスを対象とする */
        if (selectBoxList[i].id.indexOf('lb') === 0) {
            targetSelectBox = selectBoxList[i];
        }
    }
    targetSelectBox.ondblclick = function () {
        /* 選択ボタン */
        var btnSelectObj = document.getElementById('btnLeftBoxButtonSel');
        /* 選択ボタンオブジェクトが未存在またはdisabled=falseの場合はイベントをバインドしない */
        if (btnSelectObj === null) {
            return;
        }
        /* 選択ボタンクリックイベント発火 */
        btnSelectObj.click();
    };
}
/**
 * 
 * フッターヘルプアイコンのダブルクリックイベントバインド
 * @return {undefined} なし
 */
function bindFooterShowHelpEvent() {
    // アイコンオブジェクトの取得
    var helpIconObj = document.getElementById('divShowHelp');
    // 存在しない場合はそのまま終了
    if (helpIconObj === null) {
        return;
    }
    // 存在する場合はダブルクリックイベントに処理をバインド
    //helpIconObj.ondblclick = function () {
    //        displayHelp();
    //};
    helpIconObj.onclick = function () {
        displayHelp();
    };
}
/**
 * ヘルプ表示のためのイベント
 * hdnHelpChangeに値を設定しPOSTし別途HELPページをサーバー処理後に表示
 * @return {undefined} なし
 */
function displayHelp() {
    var helpFlgOjb = document.getElementById('hdnHelpChange');
    // 存在しない場合はそのまま終了
    if (helpFlgOjb === null) {
        return true;
    }
    helpFlgOjb.value = 'HELP';
    commonDispWait();
    document.forms[0].submit(); // サブミットを行いサーバーにポストバック(asp.netは1ページ1に1フォームしか入れられないためこれでOK
                                // 別基盤に移行する場合などは注意)
}
/**
 * 別ウィンドウを立ち上げヘルプページを表示する
 * @return {undefined} なし
 */
function openHelpPage() {
    var canHelpOpenObj = document.getElementById('hdnCanHelpOpen');
    if (canHelpOpenObj === null) {
        return true;
    }
    if (canHelpOpenObj.value === '1') {
        canHelpOpenObj.value = '';
        // サイトルートパスの取得（当スクリプトパスの１階層上）
        var root = getDir(getThisScriptRoot(), 1);
        // HelpUrl
        var helpUrl = root + 'COM00003HELP.aspx';
        // 別ウィンドウでヘルプページを開く
        window.open(helpUrl, '_blank', 'menubar=1, location=1, status=1, scrollbars=1, resizable=1');
    }


}
/**
 * 当スクリプトを配置しているパスを返却
 * @return {undefined} なし
 */
function getThisScriptRoot() {
    var root;
    var scripts = document.getElementsByTagName("script");
    var i = scripts.length;
    while (i--) {
        var match = scripts[i].src.match(/(^|.*\/)common\.js$/);
        if (match) {
            root = match[1];
            break;
        }
    }

    return root;
}
/**
 * 所定したURLのn階層上を取得
 * @param {string} place = 対象URL , n = 何階層上か
 * @param {number} n = 何階層上か
 * @return {string} 所定したURLのn階層上のURL
 */
function getDir(place, n) {
    return place.replace(new RegExp('(?:\\\/+[^\\\/]*){0,' + ((n || 0) + 1) + '}$'), '/');
}
var timerUnlock;
/**
 * 画面をロックする
 * @return {undefined} なし
 */
function screenLock() {

    // ロック用のdivを生成
    var wholeDiv = document.createElement('div');
    wholeDiv.id = "divScreenLock";
    // ロック用のスタイル
    wholeDiv.style.height = '100%';
    wholeDiv.style.left = '0px';
    wholeDiv.style.position = 'fixed';
    wholeDiv.style.top = '0px';
    wholeDiv.style.width = '100%';
    wholeDiv.style.zIndex = '9999';
    wholeDiv.style.opacity = '0';

    var objBody = document.getElementsByTagName("body").item(0);

    objBody.appendChild(wholeDiv);
    // そのまま放置した場合も考慮し一定時間でロック解除
    timerUnlock = setTimeout(function () {
        // ロック画面の削除
        var divScreenLock = document.getElementById('divScreenLock');
        if (divScreenLock !== null) {
            var parentBodyObj = divScreenLock.parentNode;
            parentBodyObj.removeChild(divScreenLock);
        }
    }, 3000);
}
/**
 * 画面ロック解除
 * @return {undefined} なし
 */
function screenUnlock(){
    if (timerUnlock !== null) {
        clearTimeout(timerUnlock);
        timerUnlock = null;
    }
    var divScreenLock = document.getElementById('divScreenLock');

    if (divScreenLock !== null) {
        var parentBodyObj = divScreenLock.parentNode;
        parentBodyObj.removeChild(divScreenLock);
        parentBodyObj.style.visibility = 'visible';
    }
    var scriptTags = document.getElementsByTagName("script");

    if (scriptTags !== null) {
        focusScriptTag = scriptTags[scriptTags.length - 1];
        if (focusScriptTag.innerText.match(/WebForm_AutoFocus/)) {
            (function focusScript() {
                var script = document.createElement('script');
                var src = focusScriptTag.innerHTML.match(/WebForm_AutoFocus.+;/);
                script.innerHTML = src;
                document.body.appendChild(script);
                script.addEventListener('DOMContentLoaded', focusScript, false);
            })();
        }
    }
    
}
/* コード＋名称の名称部分をポップアップさせる */
/* ラベルのidが"lbl"で始まり"Text"で終わることが前提 */
function setDisplayNameTip() {
    var contentsBox = document.getElementById('divContensbox');
    if (divContensbox === null) {
        return;
    }
    var textLabelList = contentsBox.querySelectorAll("span[id^='lbl'][id$='Text']");
    for (let i = 0; i < textLabelList.length; i++) {
        let spnObj = textLabelList[i];
        var spninnerVal = spnObj.innerText;
        if (spninnerVal === '') {
            spninnerVal = spnObj.textContent;
        }
        textLabelList[i].title = spninnerVal;
    }
}
/**
 * 左リストボックスの拡張機能（ソート、フィルタ）を追加
 * @param {targetList} 以下のデータを配列としてもつ{リストボックスのID,ソート機能フラグ,フィルタ機能フラグ}
 *                      ※ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方)
 *                      ※フィルタ機能フラグ(0,無し,1:設定)
 * @return {undefined} なし
 */
function addLeftBoxExtention(targetListBoxes) {
    //引数未指定や配列がない場合は終了
    if (targetListBoxes === null) {
        return;
    }
    if (targetListBoxes.length === 0) {
        return;
    }
    /* 左ボックスがない場合はそのまま終了 */
    var leftBoxObj = document.getElementById('divLeftbox');
    if (leftBoxObj === null) {
        return;
    }
    /* 対象一覧のループ */
    for (let i = 0; i < targetListBoxes.length; i++) {
        /* オブジェクトの存在チェック(存在しない場合はスキップ) */
        if (document.getElementById(targetListBoxes[i][0]) === null) {
            continue;
        }
        /* リストボックスの取得、および拡張機能のフラグを取得 */
        var targetListBox = document.getElementById(targetListBoxes[i][0]);
        var sortFlag = targetListBoxes[i][1];
        var filterFlag = targetListBoxes[i][2];
        // フラグが両方解除の場合意味がないので終了
        if (sortFlag === '0' && filterFlag === '0') {
            return;
        }
        // ソート拡張機能を追加
        if (sortFlag === '1' || sortFlag === '2' || sortFlag === '3') {
            addLeftBoxSort(targetListBox, sortFlag);
        }
        // フィルタ拡張機能を追加
        if (filterFlag === '1') {
            addLeftBoxFilter(targetListBox, sortFlag);
        }
        // ソートデフォルトを名称検索状態にする
        if (sortFlag === '1' || sortFlag === '3') {
            var nameSortChkObj = document.getElementById('rbLeftListOrderNameASC');
            if (nameSortChkObj !== null) {
                nameSortChkObj.click();
            }
        }
        return; // １リストしか存在しえないので見つかったら処理終了
    }

}
/**
 * ソート拡張機能のHTMLを生成及び生成したタグにイベントを紐づけ
 * @param {obj} リストボックスオブジェクト（上に拡張機能を付与します）
 * @return {flg} ソート機能フラグ(0,無し,1:名称のみ,2:名称コード(コロン区切り),3:両方)
 */
function addLeftBoxSort(obj, flg) {
    if (obj === null || obj === undefined) {
        return;
    }
    // ソートラジオボタンオブジェクトをクライアントサイドで生成するタグ
    var orderChooseTable = '<table id="tblLeftListSortType">\n';
    /* コード検索用ラジオボタン追加 */
    if (flg === '2' || flg === '3') {
        orderChooseTable = orderChooseTable + '  <tr>\n' +
            '    <td><input name="rbLeftListOrder" id="rbLeftListOrderCodeASC"  type="radio" value="CodeASC" checked="checked" />\n' +
            '        <label for="rbLeftListOrderCodeASC">Code AtoZ</label>\n' +
            '    </td>\n' +
            '    <td><input name="rbLeftListOrder" id="rbLeftListOrderCodeDESC" type="radio" value="CodeDesc" />\n' +
            '        <label for="rbLeftListOrderCodeDESC">Code ZtoA</label>\n' +
            '    </td>\n' +
            '  </tr>\n';
    }
    /* 名称検索用ラジオボタン追加 */
    if (flg === '1' || flg === '3') {
        let checkVal = ''; /* 名称検索のみの場合はNameAscにデフォルトチェックをあてる */
        if (flg === '1') {
            checkVal = 'checked="checked"';
        }
        orderChooseTable = orderChooseTable + '  <tr>\n' +
            '    <td><input name="rbLeftListOrder" id="rbLeftListOrderNameASC"  type="radio" value="NameASC" ' + checkVal + ' />\n' +
            '        <label for="rbLeftListOrderNameASC">Name AtoZ</label>\n' +
            '    </td>\n' +
            '    <td><input name="rbLeftListOrder" id="rbLeftListOrderNameDESC" type="radio" value="NameDesc" />\n' +
            '        <label for="rbLeftListOrderNameDESC">Name ZtoA</label>\n' +
            '    </td>\n' +
            '  </tr>\n';
    }
    orderChooseTable = orderChooseTable + '</table>\n';

    /* 上記で作成したタグをリストボックス前に挿入 */
    obj.insertAdjacentHTML('beforebegin', orderChooseTable);
    var objId = obj.id;
    // ラジオボタンのイベントバインド(挿入したラジオボタンすべて)
    var rbLists = document.getElementsByName('rbLeftListOrder');
    for (let i = 0; i < rbLists.length; i++) {
        var rbObj = rbLists[i];
        rbObj.onclick = (function (objId, rbObj) {
            return function () {
                leftListBoxSort(objId, rbObj);
            };
        })(objId, rbObj);
    }
}
/**
 * フィルタ拡張機能のHTMLを生成及び生成したタグにイベントを紐づけ
 * @param {obj} リストボックスオブジェクト（上に拡張機能を付与します）
 * @return {flg} フィルタ機能フラグ(0,無し,1:設定)
 */
function addLeftBoxFilter(obj, flg) {
    if (obj === null || obj === undefined) {
        return;
    }
    // フィルタテキスト及びフィルタ実行ボタンを生成するタグ
    var filterTable = '<table id="tblLeftFilter">\n' +
        '  <tr>\n' +
        '    <td><input id="txtLeftListFilter" type="text" value="" title="Filter Condition" />\n' +
        '    </td>\n' +
        '    <td><input id="btnLeftListFilter" type="button" value="Filter!" />\n' +
        '    </td>\n' +
        '  </tr>\n' +
        '</table>\n';

    /* サーバーより取得したリストボックスでの選択肢の表示非表示をCSSでOnOffできないので
       隠して、リストボックスのクローンを生成しクローンで選択肢の追加削除を行う準備
    */
    // サーバーより取得したリストボックスをspanタグで括り隠す
    let wrapper = document.createElement('span');
    wrapper.style.display = 'none';
    obj.parentNode.appendChild(wrapper);
    // サーバーより取得したリストボックスのクローンをID=lbLeftCloneとして生成
    var additionalAttr = '';
    if (obj.multiple) {
        additionalAttr = 'multiple="multiple"';
    }
    var listClone = '<select id="lbLeftClone" size="4" ' + additionalAttr + '>' + obj.innerHTML + '</select>';
    wrapper.appendChild(obj);
    wrapper.insertAdjacentHTML('beforebegin', filterTable);
    wrapper.insertAdjacentHTML('beforebegin', listClone);
    // フィルタボタンのイベントの紐づけ
    var leftFilterButton = document.getElementById('btnLeftListFilter');
    var leftListClone = document.getElementById('lbLeftClone');
    leftFilterButton.onclick = (function (leftListClone, listBoxObj) {
        return function () {
            leftListBoxFilter(leftListClone, listBoxObj.id);
        };
    })(leftListClone, obj);

    // リストボックスのクローンにて選択されてイベントをバインド
    // クローンリストが選択されていたら、隠している本物のリストの選択肢も同じ状態にする。
    leftListClone.onchange = (function (leftListClone, obj) {
        return function () {
            var baseList = document.getElementById(obj.id);
            if (baseList.multiple) {
                var baseCheckedList = baseList.querySelectorAll(':checked');
                var currentCheckedList = leftListClone.querySelectorAll(':checked');
                for (let i = baseCheckedList.length - 1; i >= 0; i--) {
                    baseCheckedList[i].selected = false;
                }
                for (let i = currentCheckedList.length - 1; i >= 0; i--) {
                    let baseOptionForSelect = baseList.querySelectorAll('option[value="' + currentCheckedList[i].value + '"]');
                    if (baseOptionForSelect[0] !== null) {
                        baseOptionForSelect[0].selected = true;
                    }
                }
            } else {
                baseList.value = leftListClone.value;
            }
        };
    })(leftListClone, obj);
    // リストボックスのクローンのダブルクリックイベントバインド
    // 本物のリストのダブルクリックイベントを発火させる
    leftListClone.ondblclick = (function (obj) {
        return function () {
            obj.ondblclick();
        };
    })(obj);
}
/**
 * リストボックスソートイベント
 * @param {listBoxObj} リストボックスオブジェクト
 * @return {rbObj} 検索のラジオボタン
 */
function leftListBoxSort(listBoxObjId, rbObj) {
    var sortBaseNode = document.getElementById(listBoxObjId);
    if (sortBaseNode === null) {
        return;
    }
    // 1件のみ0件はソートの意味がないのでそのまま終了
    if (sortBaseNode.length <= 1) {
        return;
    }
    var sortClone = sortBaseNode.cloneNode(true);
    if (sortBaseNode.multiple) {
        var selectedOpt = sortBaseNode.querySelectorAll(':checked');
        for (let i = 0; i < selectedOpt.length; i++) {
            let cloneOptionForSelect = sortClone.querySelectorAll('option[value="' + selectedOpt[i].value + '"]');
            if (cloneOptionForSelect[0] !== null) {
                cloneOptionForSelect[0].selected = true;
            }
        }

    } else {
        sortClone.value = sortBaseNode.value;
    }
    // リストボックスの選択肢ループ
    var optionArray = Array.prototype.slice.call(sortClone.options);
    // 値ソートメソッド
    function compareValueAsc(a, b) {
        if (a.value > b.value) {
            return 1;
        } else if (a.value < b.value) {
            return -1
        } else {
            return 0
        }
    }
    // 表示値ソートメソッド
    function compareTextAsc(a, b) {
        var displayStringAPart = a.textContent.substring(a.textContent.indexOf(':'));
        var displayStringBPart = b.textContent.substring(b.textContent.indexOf(':'));
        if (displayStringAPart > displayStringBPart) {
            return 1;
        } else if (displayStringAPart < displayStringBPart) {
            return -1
        } else {
            return 0
        }
    }
    // 値ソートメソッド
    function compareValueDesc(a, b) {
        if (a.value < b.value) {
            return 1;
        } else if (a.value > b.value) {
            return -1
        } else {
            return 0
        }
    }
    // 表示値ソートメソッド
    function compareTextDesc(a, b) {
        var displayStringAPart = a.textContent.substring(a.textContent.indexOf(':'));
        var displayStringBPart = b.textContent.substring(b.textContent.indexOf(':'));
        if (displayStringAPart < displayStringBPart) {
            return 1;
        } else if (displayStringAPart > displayStringBPart) {
            return -1
        } else {
            return 0
        }
    }
    // チェックボックスの値によって上記定義のソートメソッドを実行
    switch (rbObj.value) {
        case 'CodeASC':
            optionArray.sort(compareValueAsc);
            break;
        case 'CodeDesc':
            optionArray.sort(compareValueDesc);
            break;
        case 'NameASC':
            optionArray.sort(compareTextAsc);
            break;
        case 'NameDesc':
            optionArray.sort(compareTextDesc);
            break;
    }
    for (let i = 0; i < optionArray.length; i++) {
        sortClone.appendChild(sortClone.removeChild(optionArray[i]));
    }
    sortBaseNode.parentNode.replaceChild(sortClone, sortBaseNode);
    // フィルタ機能が有効な場合、画面で見えているクローンにも反映させる
    var cloneList = document.getElementById('lbLeftClone');
    if (cloneList !== null) {
        leftListBoxFilter(cloneList, listBoxObjId);
    }
}
/**
 * リストボックスフィルタ機能
 * @param {listBoxObj} リストボックスオブジェクト
 * @return {rbObj} 検索のラジオボタン
 */
function leftListBoxFilter(leftListClone, listBoxObjId) {
    var filterCond = document.getElementById('txtLeftListFilter').value.trim();
    if (filterCond === "") {
        filterCond = '.*';
    } else {
        filterCond = '.*' + filterCond.replace(/[\\^$.*+?()[\]{}|]/g, '\\$&') + '.*';
    }
    var listBoxObjBase = document.getElementById(listBoxObjId);
    var listBoxObjClone = listBoxObjBase.cloneNode(true);
    var selectedOpt = listBoxObjBase.querySelectorAll(':checked');
    if (listBoxObjBase.multiple) {
        for (let i = 0; i < selectedOpt.length; i++) {
            let cloneOptionForSelect = listBoxObjClone.querySelectorAll('option[value="' + selectedOpt[i].value + '"]');
            if (cloneOptionForSelect[0] !== null) {
                cloneOptionForSelect[0].selected = true;
            }
        }
    } else {
        listBoxObjClone.value = listBoxObjBase.value;
    }
    // 一旦画面表示上の選択ボックスクリア 
    for (let i = leftListClone.options.length - 1; i >= 0; i--) {
        leftListClone.remove(i);
    }
    // 検索条件にて絞り込み
    var reg = new RegExp(filterCond, "i");
    for (let i = 0; i < listBoxObjClone.length; i++) {
        var optionElm = listBoxObjClone.options[i];
        // 検索条件が未設定の場合はすべて対象、それ以外は検索条件に一致すること
        var targetText = optionElm.textContent;
        if (reg.test(targetText)) {
            optionClone = optionElm.cloneNode(true);
            leftListClone.appendChild(optionClone);
            if (optionElm.selected) {
                optionClone.selected = true;
            }
        }
    }
    listBoxObjBase.parentNode.replaceChild(listBoxObjClone, listBoxObjBase);
}
/**
 * アップロードボタンの配置
 * @param {listBoxObj} リストボックスオブジェクト
 * @return {rbObj} 検索のラジオボタン
 */
function addUploadExtention(targetButtonId, position, isMulti, dropBoxId, uploadButtonName) {
    var updBtnName = uploadButtonName || 'Upload';
    var targetButtonObj = document.getElementById(targetButtonId);
    if (targetButtonObj === null) {
        return;
    }
    var dropBoxObj = document.getElementById(dropBoxId);
    if (dropBoxObj === null) {
        return;
    }
    var addAttribute = '';
    if (isMulti === true) {
        addAttribute = 'multiple = "multiple"';
    }
    var addPosition = 'beforebegin';
    var buttonStyle = 'margin-right:3px;';
    if (position === 'AFTER') {
        addPosition = 'afterend';
        buttonStyle = 'margin-left:3px;';
    }

    var elmSuffix = 0;
    var existCheck = document.getElementById('btnFileUploadExtention');

    while (existCheck !== null) {
        elmSuffix = elmSuffix + 1;
        existCheck = document.getElementById('btnFileUploadExtention' + elmSuffix.toString());
    }
    var elmSuffixChar = '';
    if (elmSuffix > 0) {
        elmSuffixChar = elmSuffix.toString();
    }
    var uploadButton = '<input id="btnFileUploadExtention' + elmSuffixChar + '" type="button" value="' + updBtnName + '" style="' + buttonStyle + '" /><input id="filFileUploadExtention' + elmSuffixChar + '" type="file" ' + addAttribute + ' style="display: none;" />';
    targetButtonObj.insertAdjacentHTML(addPosition, uploadButton);

    var uploadButtonObj = document.getElementById('btnFileUploadExtention' + elmSuffixChar);
    if (targetButtonObj.disabled === true) {
        uploadButtonObj.disabled = true;
        uploadButtonObj.classList.add('aspNetDisabled');
    }
    var uploadFileObj = document.getElementById('filFileUploadExtention' + elmSuffixChar);
    uploadButtonObj.onclick = (function (dropObj, uploadFileObj) {
        return function () {
            uploadFileObj.click();
        };
    })(dropBoxId, uploadFileObj);

    uploadFileObj.onchange = (function (dropObj, uploadFileObj) {
        return function () {
            if (uploadFileObj.files.length > 0) {
                var dropObj = document.getElementById(dropBoxId);
                if (dropObj !== null) {
                    //対象のドロップイベントを選択したファイルをもとに発火
                    // file = uploadFileObj.files[0];  
                    var rect = dropObj.getBoundingClientRect(),
                        x = rect.left + (rect.width >> 1),
                        y = rect.top + (rect.height >> 1);
                    var data = { files: uploadFileObj.files };

                    ['dragenter', 'dragover', 'drop'].forEach(function (name) {
                        var event = document.createEvent('MouseEvent');
                        event.initMouseEvent(name, !0, !0, window, 0, 0, 0, x, y, !1, !1, !1, !1, 0, null);
                        event.dataTransfer = data;
                        dropObj.dispatchEvent(event);
                    });
                    uploadFileObj.outerHTML = uploadFileObj.outerHTML;
                }
            }
        };
    })(dropBoxId, uploadFileObj);
}
/**
 * 列名(cellfiedlname)及び、対象のパネルIDを元に対象の列ID、テーブルオブジェクトを返却
 * @param {string}colName カラム名称(ヘッダーのcellfiedlnameの設定値)
 * @param {string}listId 対象パネルオブジェクトのID
 * @return {object} 戻りオブジェクト.ColumnNo=対象カラム番号,戻りオブジェクト.TargetTable=対象のデータテーブル
 * @example 使用方法 呼出し側で 
 * var [ご自由な変数] = getTargetColumnNoTable('USDBR', 'WF_LISTAREA');
 * var [ご自由なcellObj] = [ご自由な変数].TargetTable.rows[ご自由な行No].cells[[ご自由な変数].ColumnNo];
 * →[ご自由なcellObj].textContent とするとセルの文字が取り出せたりします
 */
function getTargetColumnNoTable(colName, listId) {
    var listArea = document.getElementById(listId);
    // 表エリアの描画なし
    if (listArea === null) {
        return null; // そのまま終了
    }
    var leftHeaderDiv = document.getElementById(listArea.id + "_HL");
    var rightHeaderDiv = document.getElementById(listArea.id + "_HR");
    var leftDataDiv = document.getElementById(listArea.id + "_DL");
    var rightDataDiv = document.getElementById(listArea.id + "_DR");
    if (leftHeaderDiv === null && rightHeaderDiv === null) {
        return null; // そのまま終了
    }
    // 左固定列のカラム名検索
    if (leftHeaderDiv !== null && leftHeaderDiv.getElementsByTagName("table") !== null) {
        let leftHeaderTable = leftHeaderDiv.getElementsByTagName("table")[0];
        let leftHeaderRow = leftHeaderTable.rows[0]
        for (let i = 0; i < leftHeaderRow.cells.length; i++) {
            let targetCell = leftHeaderRow.cells[i];
            if (targetCell.getAttribute("cellfiedlname") === colName) {
                let retDataTable = leftDataDiv.getElementsByTagName("table")[0];
                let retVal = { ColumnNo: i, TargetTable: retDataTable };
                return retVal;
            }
        }
    }
    // 右動的列のカラム名検索
    if (rightHeaderDiv !== null && rightHeaderDiv.getElementsByTagName("table") !== null) {
        let rightHeaderTable = rightHeaderDiv.getElementsByTagName("table")[0];
        let rightHeaderRow = rightHeaderTable.rows[0]
        for (let i = 0; i < rightHeaderRow.cells.length; i++) {
            let targetCell = rightHeaderRow.cells[i];
            if (targetCell.getAttribute("cellfiedlname") === colName) {
                let retDataTable = rightDataDiv.getElementsByTagName("table")[0];
                let retVal = { ColumnNo: i, TargetTable: retDataTable };
                return retVal;
            }
        }
    }
    // ここまで来た場合は検索結果なしnull返却
    return null;
}
/**
 * リストの共通イベント(ホイール、横スクロール)をバインド
 * @param {string}listObjId リストオブジェクトのID
 * @param {string}isPostBack 各ページで'<%= if(IsPostBack = True, "1", "0") %>'を指定（外部スクリプトではサーバータグが使用できない為)
 * @param {boolean}adjustHeight 高さを調整するか
 * @param {boolean}keepHScrollWhenPostBack 省略可 ポストバック時に横スクロールを保持するか(True:保持(デフォルト),Fase:保持しない)
 * @param {boolean}resetXposFirstLoad 省略可 初回ロード時にスクロールバー位置の記憶をリセットするか(True:リセット(デフォルト),Fase:保持))
 * @return {undefined} なし
 * @example 使用方法  
 * bindListCommonEvents('<%= Me.WF_LISTAREA.ClientId %>','<%= if(IsPostBack = True, "1", "0") %>');
 */
function bindListCommonEvents(listObjId, isPostBack, adjustHeight, keepHScrollWhenPostBack, resetXposFirstLoad) {
    // 第3引数が未指定の場合
    if (adjustHeight === undefined) {
        adjustHeight = false;
    }
    // 第4引数が未指定の場合
    if (keepHScrollWhenPostBack === undefined) {
        keepHScrollWhenPostBack = true;
    }
    // 第5引数が未指定の場合
    if (resetXposFirstLoad === undefined) {
        resetXposFirstLoad = true;
    }

    var listObj = document.getElementById(listObjId);
    // そもそもリストがレンダリングされていなければ終了
    if (listObj === null) {
        return;
    }
    // Mouseホイールイベントのバインド
    var mousewheelevent = 'onwheel' in listObj ? 'wheel' : 'onmousewheel' in listObj ? 'mousewheel' : 'DOMMouseScroll';
    listObj.addEventListener(mousewheelevent, commonListMouseWheel, true);
    // 横スクロールイベントのバインド
    // 可変列ヘッダーテーブル、可変列データテーブルのオブジェクトを取得
    var headerTableObj = document.getElementById(listObjId + '_HR');
    var dataTableObj = document.getElementById(listObjId + '_DR');
    // 可変列の描画がない場合はそのまま終了
    if (headerTableObj === null || dataTableObj === null) {
        return;
    }
    // スクロールイベントのバインド
    dataTableObj.addEventListener('scroll', (function (listObj) {
        return function () {
            commonListScroll(listObj);
        };
    })(listObj), false);

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
                    objSubmit.value = 'TRUE';
                    objMouseWheel.value = '-';
                    commonDispWait();
                    document.forms[0].submit();  //aspx起動
                    return false;
                };
            };
            // ↓キー押下時
            if (window.event.keyCode === 40) {
                if (objSubmit.value === 'FALSE') {
                    objSubmit.value = 'TRUE';
                    objMouseWheel.value = '+';
                    commonDispWait();
                    document.forms[0].submit();  //aspx起動
                    return false;
                };
            };
        };
    })(), false);

    // スクロールを保持する場合
    if (isPostBack === '0' && keepHScrollWhenPostBack && resetXposFirstLoad) {
        // 初回ロード時は左スクロール位置を0とる
        setCommonListScrollXpos(listObj.id, '0');
    }
    // ポストバック時は保持したスクロール位置に戻す
    if (isPostBack === '1' && keepHScrollWhenPostBack) {
        var xpos = getCommonListScrollXpos(listObj.id);
        dataTableObj.scrollLeft = xpos;
        var e = document.createEvent("UIEvents");
        e.initUIEvent("scroll", true, true, window, 1);
        dataTableObj.dispatchEvent(e);
    }
    //高さ調整
    if (adjustHeight === true) {
        /* 現在の表示を調整 */
        commonListAdjustHeight(listObj.id);
        /* リサイズイベントにバインド */
        window.addEventListener('resize', function () {
            commonListAdjustHeight(listObj.id);
        }, false);
    }
    bindCommonListHighlight(listObj.id);
}
/* 共通リストのハイライトイベント */
function bindCommonListHighlight(listObjId) {
    // 可変列ヘッダーテーブル、可変列データテーブルのオブジェクトを取得
    var leftDataDivObj = document.getElementById(listObjId + '_DL');
    var rightDataDivObj = document.getElementById(listObjId + '_DR');
    if (leftDataDivObj === null || rightDataDivObj === null) {
        return;
    }
    var leftTrList = leftDataDivObj.getElementsByTagName('tr');
    var rightTrList = rightDataDivObj.getElementsByTagName('tr');
    for (let i = 0; i < leftTrList.length; i++) {
        var leftTr = leftTrList[i];
        var rightTr = null;
        if (rightTrList !== null) {
            rightTr = rightTrList[i];
        }
        // 左のEventListener設定
        leftTr.addEventListener('mouseover', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.add("hover");
                rightTr.classList.add("hover");
            };
        })(leftTr, rightTr), false);
        // 左のEventListener設定
        leftTr.addEventListener('mouseout', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.remove("hover");
                rightTr.classList.remove("hover");
            };
        })(leftTr, rightTr), false);
        // 右のEventListener設定
        rightTr.addEventListener('mouseover', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.add("hover");
                rightTr.classList.add("hover");
            };
        })(leftTr, rightTr), false);
        // 右のEventListener設定
        rightTr.addEventListener('mouseout', (function (leftTr, rightTr) {
            return function () {
                leftTr.classList.remove("hover");
                rightTr.classList.remove("hover");
            };
        })(leftTr, rightTr), false);
    }
}
/**
 * リストデータ部スクロール共通処理（ヘッダー部のスクロールを連動させる)
 * @param {object}listObj リスト全体のオブジェクト
 * @return {undefined} なし
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function commonListScroll(listObj) {
    var rightHeaderTableObj = document.getElementById(listObj.id + '_HR');
    var rightDataTableObj = document.getElementById(listObj.id + '_DR');
    var leftDataTableObj = document.getElementById(listObj.id + '_DL');

    setCommonListScrollXpos(listObj.id, rightDataTableObj.scrollLeft);
    rightHeaderTableObj.scrollLeft = rightDataTableObj.scrollLeft; // 左右連動させる
    leftDataTableObj.scrollTop = rightDataTableObj.scrollTop; // 上下連動させる
}
/**
 * リストの高さを調節する
 * @param {string}listId リスト全体のオブジェクトID
 * @return {string} リスト設定文字
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function commonListAdjustHeight(listId) {
    var userAgent = window.navigator.userAgent.toLowerCase();
    var browserAjust = -1;
    if (userAgent.indexOf('msie') !== -1 ||
        userAgent.indexOf('trident') !== -1) {
        //IE
    } else if (userAgent.indexOf('edge') !== -1) {
        //Edge
    } else if (userAgent.indexOf('chrome') != -1) {
        //Chrome
        //browserAjust = -10;

    } else if (userAgent.indexOf('safari') != -1) {
        //Safari
    } else if (userAgent.indexOf('firefox') != -1) {
        //FireFox
    } else if (userAgent.indexOf('opera') != -1) {
        //Opera
    }

    var listObj = document.getElementById(listId);
    var listObjParent = listObj.parentNode;
    var parentRect = listObjParent.getBoundingClientRect();
    var listRect = listObj.getBoundingClientRect();

    var listHeight = parentRect.top + listObjParent.clientHeight - listRect.top;

    //alert(parentBottom);
    listObj.style.height = (listHeight + browserAjust) + 'px';
}
/**
 * リストの横スクロール位置をwebStrage(セッションストレージ)に保持した値より取得する
 * @param {string}listId リスト全体のオブジェクトID
 * @return {string} リスト設定文字
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function getCommonListScrollXpos(listId) {
    var saveKey = document.forms[0].id + listId + "xScrollPos";
    var retValue = sessionStorage.getItem(saveKey);
    if (retValue === null) {
        retValue = '';
    }
    return retValue;
}
/**
 * リストの横スクロール位置をwebStrage(セッションストレージ)に保持する
 * @param {string}listId リスト全体のオブジェクトID
 * @param {string}val リストに保持する値
 * @return {undefined} なし
 * @example 個別ページからの使用想定はなし(bindListCommonEventsから設定)
 */
function setCommonListScrollXpos(listId, val) {
    var saveKey = document.forms[0].id + listId + "xScrollPos";
    sessionStorage.setItem(saveKey, val);
}

/**
 * 一覧表のマウスホイールイベント
 * @param {Event}event 未使用
 * @example サーバーにポストしスクロール分の一覧データを表示
 */
function commonListMouseWheel(event) {
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
        objSubmit.value = "TRUE";
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    } else {
        return false;
    }
}
/**
 * リストのソートイベント
 * @param {string}listId 対象リストのID
 * @param {string}fieldId ソート対象のフィールド
 * @example ソート設定を記載しサーバーへサブミット
 */
function commonListSortClick(listId, fieldId) {
    var objSubmit = document.getElementById('hdnSubmit');
    var formId = document.forms[0].id;
    var sortOrderObj = document.getElementById('hdnListSortValue' + formId + listId);
    var listPosition = document.getElementById('hdnListPosition');
    if (objSubmit === null || sortOrderObj === null) {
        return false;
    }

    var sortOrderValue = sortOrderObj.value;
    if (sortOrderValue === '') {
        sortOrderValue = fieldId + ' ASC';
    } else {
        var sortOrderValueArray = [];
        if (sortOrderValue !== '') {
            sortOrderValueArray = sortOrderValue.split(',');
        }
        var keyValueSort = {};
        for (var i = 0; i < sortOrderValueArray.length; i++) {
            var sortOrder = sortOrderValueArray[i];
            keyValueSort[sortOrder.split(' ')[0]] = { sort: i, value: sortOrder.split(' ')[1] };
        }

        if (keyValueSort[fieldId]) {
            if (keyValueSort[fieldId].value === "ASC") {
                keyValueSort[fieldId].value = "DESC";
            } else if ((keyValueSort[fieldId].value === "DESC")) {
                delete keyValueSort[fieldId];
            }
        } else {
            keyValueSort[fieldId] = { sort: 9999, value: "ASC" };
        }
        var retArray = [];
        for (key in keyValueSort) {
            retArray.push({ field: key, sort: keyValueSort[key].sort, value: keyValueSort[key].value });
        }
        retArray.sort(function (a, b) {
            if (a.sort < b.sort) return -1;
            if (a.sort > b.sort) return 1;
            return 0;
        });
        sortOrderValue = '';
        for (var i = 0; i < retArray.length; i++) {
            if (sortOrderValue === '') {
                sortOrderValue = retArray[i].field + ' ' + retArray[i].value;
            } else {
                sortOrderValue = sortOrderValue + ',' + retArray[i].field + ' ' + retArray[i].value;
            }
        }
    }
    sortOrderObj.value = sortOrderValue;
    objSubmit.value = "TRUE";
    commonDispWait();
    document.forms[0].submit();                            //aspx起動
}
/**
 * ポップアップの背面操作禁止を解除
 * @param {string} modalWapperId ポップアップのID
 * @return {undefined} なし
 */
function commonCloseModal(modalWapperId) {
    var disableElemType = 'select,input:not([type="hidden"]),textarea,button';
    var popUpInnerObjects = null;
    var popUpInnerObjectsId = new Array();
    if (modalWapperId !== '') {
        var keepElemType = '{0} select,{0} input:not([type="hidden"]),{0} textarea,{0} button';
        keepElemType = keepElemType.split('{0}').join("#" + modalWapperId);
        popUpInnerObjects = document.forms[0].querySelectorAll(keepElemType);
        if (popUpInnerObjects !== null) {
            for (let i = 0, len = popUpInnerObjects.length; i < len; ++i) {
                popUpInnerObjectsId.push(popUpInnerObjects[i].id);
            }
        }
    }
    document.forms[0].removeAttribute('data-showmodal');
    var inputItems = document.forms[0].querySelectorAll(disableElemType);
    for (let i = 0, len = inputItems.length; i < len; ++i) {
        let inputItem = inputItems[i];
        if (popUpInnerObjectsId.indexOf(inputItem.id) >= 0) {
            continue;
        }
        inputItem.tabIndex = null;
        inputItem.removeAttribute('tabIndex');
        let indexVal = inputItem.getAttribute('data-orgtabindex');
        if (indexVal !== null) {
            inputItem.tabIndex = indexVal;
            inputItem.removeAttribute('data-orgtabindex');
        }
    }
}
/**
 * ポップアップの背面操作を禁止
  * @param {string} modalWapperId ポップアップのID
 * @return {undefined} なし
 */
function commonDisableModalBg(modalWapperId) {
    var disableElemType = 'select,input:not([type="hidden"]),textarea,button,div.firstPage,div.lastPage';
    var popUpInnerObjects = null;
    var popUpInnerObjectsId = new Array();
    if (modalWapperId !== '') {
        var keepElemType = '{0} select,{0} input:not([type="hidden"]),{0} textarea,{0} button';
        keepElemType = keepElemType.split('{0}').join("#" + modalWapperId);
        popUpInnerObjects = document.forms[0].querySelectorAll(keepElemType);
        if (popUpInnerObjects !== null) {
            for (let i = 0, len = popUpInnerObjects.length; i < len; ++i) {
                popUpInnerObjectsId.push(popUpInnerObjects[i].id);
            }
        }
    }
    var inputItems = document.forms[0].querySelectorAll(disableElemType);
    for (let i = 0, len = inputItems.length; i < len; ++i) {
        let inputItem = inputItems[i];
        if (popUpInnerObjectsId.indexOf(inputItem.id) >= 0) {
            continue;
        }
        let indexVal = inputItem.tabIndex;
        if (inputItem.hasAttribute('tabIndex')) {
            inputItem.dataset.orgtabindex = indexVal; //('data-orgtabindex', indexVal);
        }
        inputItem.tabIndex = '-1';
    }
    // keydownイベントの無効化
    if (modalWapperId !== '') {
        var modalWapperObj = document.getElementById(modalWapperId);
        if (modalWapperObj !== null) {
            modalWapperObj.tabIndex = '-1';
            modalWapperObj.style.outline = 'none';
            // 画面キーダウンイベントのバインド
            modalWapperObj.addEventListener('keydown', (function (event) {
                return function (event) {
                    // ↑キー押下時
                    if (window.event.keyCode === 38) {
                        window.event.stopPropagation(); //フォームのキーダウンイベントに↑キー伝達抑止
                    };
                    // ↓キー押下時
                    if (window.event.keyCode === 40) {
                        window.event.stopPropagation(); //フォームのキーダウンイベントに↓キー伝達抑止
                    };
                };
            })(event), false);
        }
    }
}
/**
 * 検索条件ボックス生成
 * @param {string}createInsideObjId 検索ボックスを作りこむタグ（div等のid)
 * @param {int}一行の表示するフィールド数(省略時2フィールド)
 * @example 
 */
function commonCreateSearchArea(createInsideObjId, rowPerField) {
    rowPerField = rowPerField || 2;
	var createArea = document.getElementById(createInsideObjId);
	//描画エリアが無い場合は終了
	if (createArea === null) {
		return;
	}
	//転記対象オブジェクト取得
	var searchCondObjArea = document.getElementById('divSearchConditionBox');
	if (searchCondObjArea === null) {
		return;
    }
    if (searchCondObjArea.childElementCount === 1) {
        return;
    }
    //ポストバック時の開閉状態を記憶
    var openItem = document.getElementById('hdnSearchConditionDetailOpenFlg');
    var isOpen = false;
    if (openItem !== null) {
        if (openItem.value === '1') {
            isOpen = true;
        }
    }
    var serachBox = document.createElement('ul');
    serachBox.classList.add('commonSearchCond');
    var liField = document.createElement('li');
    liField.id = 'liRowView';
    liField.classList.add('rowView');
    var ulField = document.createElement('ul');
    var liChildField = document.createElement('li');
    liChildField.classList.add('func');
    var searchCondObjAreaSpnCnt = searchCondObjArea.querySelectorAll('#divSearchConditionBox > span');
    if (rowPerField < searchCondObjAreaSpnCnt.length) {
        liChildField.innerText = '+';
        if (isOpen) {
            liChildField.innerText = '-';
        }
        liChildField.setAttribute('onclick', 'commonDispSearchCondRow(this);');
    } else {
        liChildField.innerText = '-';
        liChildField.classList.add('noDetail');
    }
    ulField.appendChild(liChildField);
    var fieldCount = 0;
    // 転記対象の直下span要素のループ
    var className = ['label', 'input', 'text'];
    for (let i = 0; i < searchCondObjArea.childElementCount; i++) {
        // 直下の要素がspanではない場合はスキップ
        if (searchCondObjArea.children[i].tagName.toLowerCase() !== 'span') {
            continue;
        }
        // span要素直下のオブジェクトを取得
        var spanObjCnt = searchCondObjArea.children[i].children;
        var isValueOnly = false;
        // span項目直下の要素数が3以外は値、入力の状態
        if (spanObjCnt.length !== 3) {
            isValueOnly = true;
        }
        var objCnt = 0;
        // 検索条件表示エリアに転記していく
        while (spanObjCnt.length !== 0) {
            liChildField = document.createElement('li');
            liChildField.classList.add(className[objCnt]);
            liChildField.appendChild(spanObjCnt[0]);
            if (objCnt === 1 && isValueOnly) {
                liChildField.classList.add('valueOnly');
            } else if (objCnt !== 1) {
                // 文言をポップアップさせるためのイベント追加
                liChildField.addEventListener('mouseover', (function (liChildField) {
                    return function () {
                        commonDispSearchFieldPopUp(liChildField);
                    };
                })(liChildField), false);
            }
            objCnt = objCnt + 1;            
            ulField.appendChild(liChildField);
        }

        //searchCondObjArea.children[i].textContent = null;
        fieldCount = fieldCount + 1;
        if (rowPerField === fieldCount) {
            if (liField.id === 'liRowView') {
                if (isOpen) {
                    liField.classList.add('isOpen');
                }
                liField.appendChild(ulField);
                serachBox.appendChild(liField);
                liField = document.createElement('li');
                liField.id = 'liRowOC';
                if (isOpen) {
                    liField.classList.add('isOpen');
                }
                liField.classList.add('rowOC');
            } else {
                liField.appendChild(ulField);
            }
            ulField = document.createElement('ul');
            liChildField = document.createElement('li');
            liChildField.classList.add('func');
            ulField.appendChild(liChildField);
            fieldCount = 0;
        }
    }
    if (rowPerField !== fieldCount && liField.id !== 'liRowOC') {
        liField.appendChild(ulField);
    }
    if (fieldCount !== 0 && rowPerField !== fieldCount && liField.id === 'liRowOC') {
        for (fieldCount; fieldCount < rowPerField; fieldCount++) {
            liChildField = document.createElement('li');
            liChildField.classList.add('empty');
            ulField.appendChild(liChildField);
        }
        liField.appendChild(ulField);
    }
    serachBox.appendChild(liField);
    // 同席に作成した検索ボックスを展開
    createArea.appendChild(serachBox);
    // 展開したのち、隠しボックスの位置を調整
    var rowViewObj = document.getElementById('liRowView');
    var rowOcObj = document.getElementById('liRowOC');
    if (rowOcObj === null) {
        return;
    }
    rowOcObj.style.top = rowViewObj.offsetTop + rowViewObj.offsetHeight;
    // 値が入っているマーク付け
    commonSetSearchFieldHasInputValueMark();
}
/**
 * 検索条件開閉イベント
 * @param {string}listId 対象リストのID
 * @param {string}fieldId ソート対象のフィールド
 * @example ソート設定を記載しサーバーへサブミット
 */
function commonDispSearchCondRow(callerObj) {
	var rowOcObj = document.getElementById('liRowOC');
    var rowViewObj = document.getElementById('liRowView');
    var openItem = document.getElementById('hdnSearchConditionDetailOpenFlg');
	if (callerObj.innerText === '+') {
		rowOcObj.classList.add('isOpen');
		rowViewObj.classList.add('isOpen');
        callerObj.innerText = '-';
        if (openItem !== null) {
            openItem.value = '1';
        }
	} else {
		rowOcObj.classList.remove('isOpen');
		rowViewObj.classList.remove('isOpen');
        callerObj.innerText = '+';
        if (openItem !== null) {
            openItem.value = '0';
        }
    }
    commonSetSearchFieldHasInputValueMark();
}
/**
 * 検索条件開閉イベント
 * @param {string}listId 対象リストのID
 * @param {string}fieldId ソート対象のフィールド
 * @example ソート設定を記載しサーバーへサブミット
 */
function commonDispSearchFieldPopUp(obj) {
    obj.title = obj.innerText;
}
function commonSetSearchFieldHasInputValueMark() {
    var hasValue = false;
    var liOcObj = document.getElementById('liRowOC');
    var liViewObj = document.getElementById('liRowView');
    var inputedShowHideAreaItem = liOcObj.querySelectorAll('input[type="text"]');
    for (let i = 0; i < inputedShowHideAreaItem.length; i++) {
        if (inputedShowHideAreaItem[i].value !== '') {
            hasValue = true;
            break;
        }
    }
    if (hasValue === false) {
        inputedShowHideAreaItem = liOcObj.querySelectorAll('input[type="radio"]');
        if (inputedShowHideAreaItem.length !== 0) {
            hasValue = true;
        }
    }

    if (hasValue) {
        liViewObj.classList.add('hasValue');
    } else {
        liViewObj.classList.remove('hasValue');
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
 *  ダウンロード処理(Pdf)
 * @return {undefined} なし
 * @description 
 */
function f_PDFPrint() {
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
function bindCommonDragDropEvents(dragDropAreaObjectsList, handlerUrl) {
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
        objDDArea.addEventListener('dragstart', function (event) { commonDragEventCancel(event); }, false);
        objDDArea.addEventListener('dragenter', function (event) { commonDragEventCancel(event); }, false);
        objDDArea.addEventListener('dragover', function (event) { commonDragOverEvent(event); }, false);
        objDDArea.addEventListener('dragleave', function (event) { commonDragEventCancel(event); }, false);
        objDDArea.addEventListener('drag', function (event) { commonDragEventCancel(event); }, false);
        objDDArea.addEventListener('drop', function (bindObjInfo, handlerUrl) {
            return function (event) {
                commonDropEvent(event, bindObjInfo.kbn, bindObjInfo.acceptExtentions, handlerUrl);
            };
        }(bindObjInfo, handlerUrl), false);

        objDDArea.addEventListener('dragend', function (event) { commonDragEventCancel(event); }, false);
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
function commonDropEvent(e, kbn, acceptExtentions, handlerUrl) {
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
function commonDragOverEvent(event) {
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
function commonDragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
}
/**
 *  ウェイト画面表示
 * @return {undefined} なし
 * @description 
 */
function commonDispWait() {
    var hasElm = document.getElementById('comloading');
    if (hasElm !== null) {
        document.body.removeChild(hasElm);
    }
    // ウエイトスクリーン用半透明の大枠オブジェクト
    var lodingObj;
    lodingObj = document.createElement('div');
    lodingObj.id = 'comloading';
    lodingObj.classList.add('comloading');
    // ウエイトスクリーン用のフォーカス移動抑止のオブジェクト
    var forsubObj;
    forsubObj = document.createElement('input');
    forsubObj.id = 'comlodingtextbox';
    forsubObj.type = 'text';
    forsubObj.classList.add('comlodingtextbox');
    forsubObj.tabindex = '1';
    lodingObj.appendChild(forsubObj);
    // ウェイトスクリーン用のアニメーション枠
    var lodingMsgObj = document.createElement('div');
    lodingMsgObj.classList.add('comloadingmsg');
    // 子要素追加
    var lodingMsgChild1Obj = document.createElement('div');
    var lodingMsgChild2Obj = document.createElement('div');
    var lodingMsgChild3Obj = document.createElement('div');
    lodingMsgObj.appendChild(lodingMsgChild1Obj);
    lodingMsgObj.appendChild(lodingMsgChild2Obj);
    lodingMsgObj.appendChild(lodingMsgChild3Obj);
    //lodingMsgObj.innerText = 'Loading.....';
    lodingObj.appendChild(lodingMsgObj);
    document.body.appendChild(lodingObj);
    // テキストボックスにフォーカスを合わせておく
    forsubObj = document.getElementById('comlodingtextbox');
    forsubObj.select();
    forsubObj.onblur = (function (forsubObj) {
        return function () {
            forsubObj.select();
        }
    }(forsubObj));
    commonDisableModalBg('comloading');
}
/**
 *  ウェイト画面非表示
 * @return {undefined} なし
 * @description 
 */
function commonHideWait() {
    var hasElm = document.getElementById('comloading');
    if (hasElm !== null) {
        commonCloseModal('');
        document.body.removeChild(hasElm);
    }
}
