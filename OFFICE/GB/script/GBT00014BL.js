/* show/hide commonInfoリンクボタンクリック時イベント */
function commonInfoAreaClick() {
    var isShowComInfo = document.getElementById("hdnIsShowCommonInfo");
    if (isShowComInfo.value === "1") {
        isShowComInfo.value = "0"; /* PostBackしても保持するHidden項目に設定 */
    } else {
        isShowComInfo.value = "1"; /* PostBackしても保持するHidden項目に設定 */
    }
}
/* commonInfoエリアの表示/非表示スタイル切り替え */
function changeCommonInfoArea() {
    var isShowComInfo = document.getElementById("hdnIsShowCommonInfo");
    var divCommonInfo = document.getElementById("commonInfo");
    var spnShowCommon = document.getElementById("spnShowCommonInfo");
    if (isShowComInfo.value === "1") {
        spnShowCommon.innerText = "Hide CommonInfo";
        divCommonInfo.style.display = "block";
    } else {
        spnShowCommon.innerText = "Show CommonInfo";
        divCommonInfo.style.display = "none";
    }
}
// タブクリックイベント
function tabClick(tabId) {
    var selectedTabId = document.getElementById('hdnSelectedTabId');
    var submitObj = document.getElementById('hdnSubmit');
    var targetObj = document.getElementById(tabId);
    /* 対象オブジェクトが存在しない場合はそのまま終了 */
    if (selectedTabId === null || submitObj === null || targetObj === null) {
        return;
    }
    /* 選択中のオブジェクトの場合はそのまま終了 */
    if (selectedTabId.value === targetObj.id) {
        return;
    }
    /* hidden submitがFALSEではない場合は動作させない */
    if (submitObj.value === 'FALSE') {
        submitObj.value = 'TRUE';
        selectedTabId.value = tabId;
        commonDispWait();
        document.forms[0].submit();
    }
}
// タブクリックイベントバインド
function bindTabClickEvent(targetTabObjects) {
    // 引数未指定や配列がない場合は終了
    if (targetTabObjects === null) {
        return;
    }
    if (targetTabObjects.length === 0) {
        return;
    }
    //ボタンID配列のループ 
    for (let i = 0; i < targetTabObjects.length; i++) {
        if (targetTabObjects[i] === '') {
            continue;
        }
        /* オブジェクトの存在チェック(存在しない場合はスキップ) */
        if (document.getElementById(targetTabObjects[i]) === null) {
            continue;
        }
        var tabId = targetTabObjects[i];
        
        /* クリックイベントに紐づけ */
        document.getElementById(tabId).addEventListener('click', (function (tabId) {
            return function () {
                tabClick(tabId);
        
            };
        })(tabId), false);

        //document.getElementById(tabId).onclick = (function (tabId) {
        //    return function () {
        //        tabClick(tabId);
        //
        //    };
        //  })(tabId);
    }
}
/* 左備考・初見エリアのダブルクリックイベント */
function bindSpnRightRemarksDbClick() {
    var spnRightRemarksObj = document.getElementById('spnRightRemarks');
    var hdnRightBoxCloseObj = document.getElementById('hdnRightBoxClose');
    if (spnRightRemarksObj === null || hdnRightBoxCloseObj === null) {
        return;
    }
    /* クリックイベントに紐づけ */
    spnRightRemarksObj.addEventListener('dblclick', (function () {
        var submitObj = document.getElementById('hdnSubmit');
        if (submitObj.value === 'FALSE') {
            submitObj.value = 'TRUE';
            hdnRightBoxCloseObj.value = 'CLOSE';
            commonDispWait();
            document.forms[0].submit();
        }
    }), false);

}

/* ********************************************** */
/* 費用関連 
/* ********************************************** */
/* 費用項目の各イベントをバインド */
function bindCostRowEvents() {
    var divTankDetailInfo = document.getElementById('divTankDetailInfo')
    /* 費用詳細が開かれていない場合はそのまま終了 */
    if (divTankDetailInfo === null) {
        return;
    }

    /* divTankDetailInfo内のGrossWeightテキストボックスを取得 */
    var gWeightTextObjects = divTankDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtGrossWeight_"]');
    if (gWeightTextObjects !== null) {
        if (gWeightTextObjects.length > 0) {
            for (var i = 0, len = gWeightTextObjects.length; i < len; ++i) {
                var obj = gWeightTextObjects[i]
                /* フォーカスイベント紐づけ */
                obj.addEventListener('focus', function (obj) {
                    return function () {
                        obj._oldvalue = obj.value;
                    };
                }(obj), false);

                /* ブラーイベントに紐づけ */
                obj.addEventListener('blur', function (obj) {
                    return function () {
                        if (obj._oldvalue !== obj.value) {
                            calcSummaryGrossWeight(obj);
                        }
                    };
                }(obj), false);
            }
        }
    }
    /* divTankDetailInfo内のNetWeightテキストボックスを取得 */
    var netWeightObjects = divTankDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtNetWeight_"]');
    if (netWeightObjects !== null) {
        if (netWeightObjects.length > 0) {
            for (var i = 0, len = netWeightObjects.length; i < len; ++i) {
                var obj = netWeightObjects[i]
                /* フォーカスイベント紐づけ */
                obj.addEventListener('focus', function (obj) {
                    return function () {
                        obj._oldvalue = obj.value;
                    };
                }(obj), false);

                /* ブラーイベントに紐づけ */
                obj.addEventListener('blur', function (obj) {
                    return function () {
                        if (obj._oldvalue !== obj.value) {
                            calcSummaryNetWeight(obj);
                        }
                    };
                }(obj), false);
            }
        }
    }

    /* divTankDetailInfo内のNoofPackageテキストボックスを取得 */
    var noOfPackageObjects = divTankDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtNoOfPackage_"]');
    if (noOfPackageObjects !== null) {
        if (noOfPackageObjects.length > 0) {
            for (var i = 0, len = noOfPackageObjects.length; i < len; ++i) {
                var obj = noOfPackageObjects[i]
                /* フォーカスイベント紐づけ */
                obj.addEventListener('focus', function (obj) {
                    return function () {
                        obj._oldvalue = obj.value;
                    };
                }(obj), false);

                /* ブラーイベントに紐づけ */
                obj.addEventListener('blur', function (obj) {
                    return function () {
                        if (obj._oldvalue !== obj.value) {
                            calcSummaryNoOfPackage(obj);
                        }
                    };
                }(obj), false);
            }
        }
    }
    /* divTankDetailInfo内のNoofPackageテキストボックスを取得 */
    var emptyOrFullObjects = divTankDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtEmptyOrFull_"]');
    if (emptyOrFullObjects !== null) {
        if (emptyOrFullObjects.length > 0) {
            for (var i = 0, len = emptyOrFullObjects.length; i < len; ++i) {
                var obj = emptyOrFullObjects[i];
                var canEventBind = false;
                obj.autocomplete = 'off'; /* オートコンプリートをOFF */
                obj.placeholder = '';
                if (obj.disabled !== null) {
                    if (obj.disabled !== 'disabled' && obj.disabled !== 'true' && obj.disabled !== true) {
                        obj.placeholder = 'DoubleClick to select';
                        canEventBind = true;
                    }
                }
                if (canEventBind === true) {
                    // フォーカスを合わさないとテキストボックスはダブルクリックに反応しないため、
                    // 前後をspanタグで括りそちらにダブルクリックイベントを記載
                    var wrapper = document.createElement('span');
                    wrapper.appendChild(obj.cloneNode(true));
                    obj.parentNode.replaceChild(wrapper, obj);

                    /* クリックイベントに紐づけ */
                    wrapper.addEventListener('dblclick', (function (obj) {
                        return function () {
                            emptyOrFullDbClick(obj);
                        };
                    })(obj), false);
                }

            }
        }
    }
}

/* GrossWeight計算 */
function calcSummaryGrossWeight(obj) {
    var divTankDetailInfo = document.getElementById('divTankDetailInfo')
    if (divTankDetailInfo === null) {
        return;
    }
    var submitObj = document.getElementById('hdnSubmit');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex'); 

    /* 表示切替 */
    if (submitObj !== 'FALSE' || rightBoxOpenObj.value !== 'Open') {
        submitObj.value = 'TRUE';
        currentUnieuqIndexObj.value = obj.dataset.uniqueindex;
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;
        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcSummaryGrossWeight';
        commonDispWait();
        document.forms[0].submit();
    }
} 
/* NetWeight計算 */
function calcSummaryNetWeight(obj) {
    var divTankDetailInfo = document.getElementById('divTankDetailInfo')
    if (divTankDetailInfo === null) {
        return;
    }
    var submitObj = document.getElementById('hdnSubmit');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex');

    /* 表示切替 */
    if (submitObj !== 'FALSE' || rightBoxOpenObj.value !== 'Open') {
        submitObj.value = 'TRUE';
        currentUnieuqIndexObj.value = obj.dataset.uniqueindex;
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;
        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcSummaryNetWeight';
        commonDispWait();
        document.forms[0].submit();
    }
} 
/* NoOfPackage計算 */
function calcSummaryNoOfPackage(obj) {
    var divTankDetailInfo = document.getElementById('divTankDetailInfo')
    if (divTankDetailInfo === null) {
        return;
    }
    var submitObj = document.getElementById('hdnSubmit');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex');

    /* 表示切替 */
    if (submitObj !== 'FALSE' || rightBoxOpenObj.value !== 'Open') {
        submitObj.value = 'TRUE';
        currentUnieuqIndexObj.value = obj.dataset.uniqueindex;
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;
        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcSummaryNoOfPackage';
        commonDispWait();
        document.forms[0].submit();
    }
} 
/* 費用名変更
   費用コード変更時に反映  */
function calcContractor(obj) {
    var divTankDetailInfo = document.getElementById('divTankDetailInfo')
    if (divTankDetailInfo === null) {
        return;
    }

    var hdnSubmitObj = document.getElementById('hdnSubmit');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex');

    if (hdnSubmitObj.value === 'FALSE') {
        hdnSubmitObj.value = 'TRUE';
        currentUnieuqIndexObj.value = obj.dataset.uniqueindex;
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;

        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcContractor';
        commonDispWait();
        document.forms[0].submit();
    }
}

/* EorFダブルクリックイベント */
/* 引数:objダブルクリックしたテキストボックスオブジェクト */
function emptyOrFullDbClick(obj) {
    // 対象のオブジェクトが存在しない場合は終了
    if (obj === null) {
        return;
    }

    var submitobj = document.getElementById('hdnSubmit');
    var dbClickField = document.getElementById('hdnTextDbClickField');
    var isLeftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
    var leftboxActiveViewId = document.getElementById('hdnLeftboxActiveViewId');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex');

    // サブミットフラグが立っていない場合のみ実行
    if (submitobj.value === 'FALSE') {
        submitobj.value = 'TRUE';
        dbClickField.value = "txtEmptyOrFull";
        leftboxActiveViewId.value = "vLeftEorF";
        currentUnieuqIndexObj.value = obj.dataset.uniqueindex;
        isLeftBoxOpen.value = "Open";
        commonDispWait();
        document.forms[0].submit();
    }
}

/* ********************************************** */
/* ブレーカー全般で使用
/* ********************************************** */
/* 数字カンマ区切り変換 */
function formatNumber(num, scale) {
    var re = /(\d)(?=(\d\d\d)+(?!\d))/g; //正規表現
    return Number(num).toFixed(scale).replace(re, '$1,');
}
/* 左ボックス表示の書き換え */
function displayRightBox() {
    var rightBoxOpenObj = document.getElementById('hdnRightboxOpen');
    var rightBoxObj = document.getElementById('divRightbox');
    var rightBoxRemarkField = document.getElementById('hdnRightBoxRemarkField');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex');

    rightBoxRemarkField.value = '';
    currentUnieuqIndexObj.value = '';

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
// ディテール(内容表示)処理
function FileDisplay(filename) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById('hdnFileDisplay').value = filename;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
}
// ダブルクリック処理
function Field_DBclick(ActiveViewId, DbClickField) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById('hdnLeftboxActiveViewId').value = ActiveViewId;
        document.getElementById('hdnTextDbClickField').value = DbClickField;
        document.getElementById('hdnIsLeftBoxOpen').value = "Open";
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    };
};