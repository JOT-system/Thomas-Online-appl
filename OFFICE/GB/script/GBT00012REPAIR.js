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

// Demurrageの自動計算を日付ToBlurイベントに紐づけ 
function bindDemurrageDayOnBlur() {
    var daysTo = document.getElementById('txtDemurdayT1');
    if (daysTo === null) {
        return;
    }
    /* ブラーイベントに紐づけ */
    daysTo.addEventListener('focus', function (daysTo) {
        return function () {
            daysTo._oldvalue = daysTo.value;
        };
    }(daysTo), false);

    daysTo.addEventListener('blur', function (daysTo) {
        return function () {
            if (daysTo._oldvalue !== daysTo.value) {
                calcDemurrageDay();
            }
        };
    }(daysTo), false);

}
// Demurrage翌日を自動計算
function calcDemurrageDay() {
    var daysFrom = document.getElementById('txtDemurdayF1');
    var daysTo = document.getElementById('txtDemurdayT1');
    var thereafter = document.getElementById('txtDemurday2');

    /* すべてのオブジェクトがそろっているか確認 */
    if (daysFrom === null || daysTo === null || thereafter === null) {
        return;
    }
    daysFrom.value = '1';
    thereafter.value = '';
    if (daysTo.value === '') {
        return;
    }
    if (isNaN(daysTo.value)) {
        return;
    }
    var hdnSubmitObj = document.getElementById('hdnSubmit');
    if (hdnSubmitObj.value === 'FALSE') {
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;

        hdnSubmitObj.value = 'TRUE';
        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcDemurrageDay';
        commonDispWait();
        document.forms[0].submit();
    }
    ///* 計算 */
    //var daysToNum = 0;
    //var resultthereafter = null;
    //try {
    //    daysToNum = parseInt(daysTo.value.replace(/,/g, ''));
    //    daysTo.value = daysToNum;

    //    thereafter.value = daysToNum + 1;
    //} catch(e) {
    //    thereafter.value = '';
    //}
}
/* Blurイベントの紐づけ */
function bindTotalDaysOnBlur() {
    var loading = document.getElementById('txtLoading');
    var steaming = document.getElementById('txtSteaming');
    var tip = document.getElementById('txtTip');
    var extra = document.getElementById('txtExtra');
    /* 必要項目がない場合そのまま終了 */
    if (loading === null || steaming === null || tip === null || extra === null) {
        return;
    }
    varBindObjects = [loading, steaming, tip, extra];
    for (let i = 0; i < varBindObjects.length; i++) {
        var obj = varBindObjects[i];
        /* フォーカスイベント紐づけ */
        varBindObjects[i].addEventListener('focus', function (obj) {
            return function () {
                obj._oldvalue = obj.value;
            };
        }(obj),false);

        /* ブラーイベントに紐づけ */
        varBindObjects[i].addEventListener('blur', function (obj) {
            return function () {
                if (obj._oldvalue !== obj.value) {
                    calcTotalDays();
                }
            };
        }(obj),false);
    }
}

// HirageInfo 期間合計計算 発側期間、船上期間、着側期間、追加期間の合計を算出
function calcTotalDays() {
    var totalDays = document.getElementById('txtTotal');
    var loading = document.getElementById('txtLoading');
    var steaming = document.getElementById('txtSteaming');
    var tip = document.getElementById('txtTip');
    var extra = document.getElementById('txtExtra');
    /* 必要項目がない場合そのまま終了 */
    if (totalDays === null || loading === null || steaming === null || tip === null || extra === null) {
        return;
    }
    /* 合計欄初期化 */
    totalDays.value = '';
    /* 必要項目がすべて空白の場合はそのまま終了 */
    if (loading.value === '' && steaming.value === '' && tip.value === '' && extra.value === '') {
        return;
    }
    /* 必要項目が数字かチェック */
    if (isNaN(loading.value.replace(/,/g, '')) || isNaN(steaming.value.replace(/,/g, '')) || isNaN(tip.value.replace(/,/g, '')) || isNaN(extra.value.replace(/,/g, ''))) {
        return;
    }
    var hdnSubmitObj = document.getElementById('hdnSubmit');
    if (hdnSubmitObj.value === 'FALSE') {
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;
        
        hdnSubmitObj.value = 'TRUE';
        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcTotalDays';
        commonDispWait();
        document.forms[0].submit();
    }

    ///* 計算実行 */
    //var loadingNum = !loading.value ? 0 : parseInt(loading.value.replace(/,/g, ''));
    //if (loading.value !== '') {
    //    loading.value = loadingNum;
    //}
    //var steamingNum = !steaming.value ? 0 : parseInt(steaming.value.replace(/,/g, ''));
    //if (steaming.value !== '') {
    //    steaming.value = steamingNum;
    //}
    //var tipNum = !tip.value ? 0 : parseInt(tip.value.replace(/,/g, ''));
    //if (tip.value !== '') {
    //    tip.value = tipNum;
    //}
    //var extraNum = !extra.value ? 0 : parseInt(extra.value.replace(/,/g, ''));
    //if (extra.value !== '') {
    //    extra.value = extraNum;
    //}
    //totalDays.value = loadingNum + steamingNum + tipNum + extraNum;
}
/* 重量のブラーイベントに紐づけ */
function bindFillingRateCheckOnBlur() {
    var weight = document.getElementById('txtWeight');
    if (weight === null) {
        return;
    }

    /* フォーカスイベント紐づけ */
    weight.addEventListener('focus', function (weight) {
        return function () {
            weight._oldvalue = weight.value;
        };
    }(weight), false);

    /* ブラーイベントに紐づけ */
    weight.addEventListener('blur', function (weight) {
        return function () {
            if (weight._oldvalue !== weight.value) {
                calcFillingRate();
            }
        };
    }(weight), false);
}
/* タンク積載%及びチェック */
function calcFillingRate() {
    var isHazard = document.getElementById('hdnProductIsHazard');
    var weight = document.getElementById('txtWeight');
    var gravity = document.getElementById('txtSGravity');
    var capacity = document.getElementById('txtTankCapacity');
    var fillingRate = document.getElementById('txtTankFillingRate');
    var fillingRateCheck = document.getElementById('txtTankFillingCheck');

    /* 必要項目がない場合そのまま終了 */
    if (isHazard === null || weight === null || gravity === null ||
        capacity === null || fillingRate === null || fillingRateCheck === null) {
        return;
    }
    /* 計算結果・チェック結果クリア */
    fillingRate.value = '';
    fillingRateCheck.value = '';
    fillingRateCheck.className = 'aspNetDisabled';
    /* 必要項目が数字かチェック */
    if (isNaN(weight.value.replace(/,/g, '')) || isNaN(gravity.value.replace(/,/g, ''))) {
        return;
    }
    /* 必須項目が未入力かチェック */
    if (weight.value === '' || gravity.value === '') {
        return;
    }
    var hdnSubmitObj = document.getElementById('hdnSubmit');
    if (hdnSubmitObj.value === 'FALSE') {
        hdnSubmitObj.value = 'TRUE';
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;

        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcFillingRate';
        commonDispWait();
        document.forms[0].submit();
    }

    ///* 計算実行 */
    //weightNum = parseFloat(weight.value.replace(/,/g, ''));
    //weight.value = formatNumber(weightNum,0);
    //gravityNum = parseFloat(gravity.value.replace(/,/g, ''));
    //capacityNum = parseFloat(capacity.value.replace(/,/g, ''));
    //try {
    //    var fillingRateNum = weightNum / (capacityNum * gravityNum) * 100;
    //    fillingRate.value = formatNumber(Math.floor(fillingRateNum * 100) / 100,2);
    //    fillingRate.value = fillingRate.value + "%";
    //    var highValue = 95;
    //    var lowValue = 70;
    //    /* 危険物の場合は閾値を変更 */
    //    if (isHazard === '1') {
    //        highValue = 95;
    //        lowValue = 80;
    //    }
    //    if (fillingRateNum < lowValue || fillingRateNum > highValue) {
    //        fillingRateCheck.value = "ERROR";
    //        fillingRateCheck.className = 'aspNetDisabled error';
    //    } else {
    //        fillingRateCheck.value = "CLEAR!";
    //        fillingRateCheck.className = 'aspNetDisabled clear';
    //    }
    //} catch (e) {
    //    return;
    //}
}
/* JOTHIREAGE COMMERCIALFACTOR変更時イベント */
function bindInvoiceTotalOnBlur() {
    var jotHireage = document.getElementById('txtJOTHireage');
    var commercialFactor = document.getElementById('txtCommercialFactor');
    /* 必要項目がレンダリングされていない場合は実行しない */
    if (jotHireage === null || commercialFactor === null) {
        return;
    }
    varBindObjects = [jotHireage, commercialFactor];
    for (let i = 0; i < varBindObjects.length; i++) {
        var obj = varBindObjects[i];
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
                    calcInvoiceTotal();
                }
            };
        }(obj), false);
    }
}
/* JOT売上情報・総額算出
   JOTHIREAGE COMMERCIALFACTOR変更時に反映  */
function calcInvoiceTotal() {
    var totalCost = document.getElementById('txtTotalCost');
    var jotHireage = document.getElementById('txtJOTHireage');
    var commercialFactor = document.getElementById('txtCommercialFactor');
    var invoiceTotal = document.getElementById('txtInvoicedTotal');
    var totalSpan = document.getElementById('txtTotal');
    var parDay = document.getElementById('txtPerDay');
    /* 必要項目がレンダリングされていない場合は実行しない */
    if (totalCost === null || jotHireage === null || commercialFactor === null || invoiceTotal === null || totalSpan === null) {
        return;
    }

    /* InvoiceTotalを初期化 */
    invoiceTotal.value = "";
    /* 必要項目に入力がない場合はそのまま終了 */
    if (totalCost.value === '' || jotHireage.value === '' || commercialFactor.value === '' || totalSpan.value === '') {
        return;
    }
    /* 必要項目が数字かチェック */
    if (isNaN(totalCost.value.replace(/,/g, '')) || isNaN(jotHireage.value.replace(/,/g, '')) || isNaN(commercialFactor.value.replace(/,/g, '')) || isNaN(totalSpan.value.replace(/,/g, ''))) {
        return;
    }
    var hdnSubmitObj = document.getElementById('hdnSubmit');
    if (hdnSubmitObj.value === 'FALSE') {
        hdnSubmitObj.value = 'TRUE';
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;

        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcInvoiceTotal';
        commonDispWait();
        document.forms[0].submit();
    }
}
/* 総額変更時のイベントバインド */
function bindHireageCommercialfactorOnBlur() {
    var invoiceTotal = document.getElementById('txtInvoicedTotal');
    /* 必要項目がレンダリングされていない場合は実行しない */
    if (invoiceTotal === null) {
        return;
    }
    /* フォーカスイベント紐づけ */
    invoiceTotal.addEventListener('focus', function (invoiceTotal) {
        return function () {
            invoiceTotal._oldvalue = invoiceTotal.value;
        };
    }(invoiceTotal), false);

    /* ブラーイベントに紐づけ */
    invoiceTotal.addEventListener('blur', function (invoiceTotal) {
        return function () {
            if (invoiceTotal._oldvalue !== invoiceTotal.value) {
                calcHireageCommercialfactor();
            }
        };
    }(invoiceTotal), false);
}
/* 総額よりJOT総額、調整、総額 自動計算をする */
function calcHireageCommercialfactor() {
    var totalCost = document.getElementById('txtTotalCost');
    var jotHireage = document.getElementById('txtJOTHireage');
    var commercialFactor = document.getElementById('txtCommercialFactor');
    var invoiceTotal = document.getElementById('txtInvoicedTotal');
    var totalSpan = document.getElementById('txtTotal');
    var hireagePerDay = document.getElementById('txtPerDay');

    /* 必要項目がレンダリングされていない場合は実行しない */
    if (totalCost === null || jotHireage === null || commercialFactor === null || invoiceTotal === null || totalSpan === null || hireagePerDay === null) {
        return;
    }
    /* 自動算出項目を初期化 */
    jotHireage.value = "";
    commercialFactor.value = "";
    hireagePerDay.value = "";
    /* 必要項目に入力がない場合はそのまま終了 */
    if (totalCost.value === '' || invoiceTotal.value === '' || totalSpan.value === '') {
        return;
    }
    /* 必要項目が数字かチェック */
    if (isNaN(totalCost.value.replace(/,/g, '')) || isNaN(invoiceTotal.value.replace(/,/g, '')) || isNaN(totalSpan.value.replace(/,/g, '')) ) {
        return;
    }
    var hdnSubmitObj = document.getElementById('hdnSubmit');
    if (hdnSubmitObj.value === 'FALSE') {
        hdnSubmitObj.value = 'TRUE';
        var scrollTop = document.getElementById("hdnBodyScrollTop");
        scrollTop.value = document.getElementById("divContensbox").scrollTop;

        var calcFunctionName = document.getElementById('hdnCalcFunctionName');
        calcFunctionName.value = 'CalcHireageCommercialfactor';
        commonDispWait();
        document.forms[0].submit();
    }

}

/* 申請ボタン及びポップアップのボタンにイベントを紐づける */
function bindApplyOnClick() {
    var changeFlg = document.getElementById('hdnMsgboxAppChangeFlg');
    var showFlg = document.getElementById('hdnMsgboxShowFlg');
    var buttonObj = document.getElementById('btnApply');
    var popAreaBox = document.getElementById('divApplyMsgBoxWrapper');
    var buttonCancel = document.getElementById('btnApplyMsgCancel');
    var buttonYes = document.getElementById('btnApplyMsgYes');
    var buttonNo = document.getElementById('btnApplyMsgNo');

    if (changeFlg.value !== '') {
        if (showFlg.value === '1') {
            popAreaBox.style.display = 'block';
        }
    }

    if (buttonObj !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonObj.addEventListener('click', (function (popAreaBox) {
            return function () {
                if (changeFlg.value === '') {
                    return;
                }
                popAreaBox.style.display = 'block';
            };
        })(popAreaBox), false);
    }

    if (buttonCancel !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonCancel.addEventListener('click', (function (popAreaBox, changeFlg) {
            return function () {
                popAreaBox.style.display = 'none';
                changeFlg.value = '';
            };
        })(popAreaBox, changeFlg), false);
    }

    if (buttonYes !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonYes.addEventListener('click', (function (popAreaBox) {
            return function () {

                var hdnSubmitObj = document.getElementById('hdnSubmit');
                var hdnButtonClickId = document.getElementById('hdnButtonClick');
                if (hdnSubmitObj.value === 'FALSE') {
                    hdnSubmitObj.value = 'TRUE';
                    popAreaBox.style.display = 'none';
                    hdnButtonClickId.value = 'btnApplyMsgCancel';
                    commonDispWait();
                    document.forms[0].submit();
                }
            };
        })(popAreaBox), false);
    }

    if (buttonNo !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonNo.addEventListener('click', (function (popAreaBox) {
            return function () {

                var hdnSubmitObj = document.getElementById('hdnSubmit');
                var hdnButtonClickId = document.getElementById('hdnButtonClick');
                if (hdnSubmitObj.value === 'FALSE') {
                    hdnSubmitObj.value = 'TRUE';
                    popAreaBox.style.display = 'none';
                    hdnButtonClickId.value = 'btnApplyMsgCancel';
                    commonDispWait();
                    document.forms[0].submit();
                }
            };
        })(popAreaBox), false);
    }
}

/* 費用項目を開く */
function bindAddCostOnClick() {
    var btnCostAdd = document.getElementById('btnAddCost');
    /* 費用追加ボタンが存在しない場合はそのまま終了 */
    if (btnCostAdd === null) {
        return;
    }
    /* 左ボックスエレメント */
    var leftBoxOjb = document.getElementById('divLeftbox');
    /* そもそも左ボックスが存在しない場合はそのまま終了 */
    if (leftBoxOjb === null) {
        return;
    }
    /* 追加ボタンにイベントをバインド */
    btnCostAdd.addEventListener('click', (function () {
        var submitObj = document.getElementById('hdnSubmit');
        var viewId = 'vLeftCost';
        var dblClickObject = document.getElementById('hdnTextDbClickField');
        var viewIdObject = document.getElementById('hdnLeftboxActiveViewId');
        var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
        if (submitObj.value === 'FALSE') {
            submitObj.value = 'TRUE';
            dblClickObject.value = 'gvCostList';
            viewIdObject.value = viewId;
            leftBoxOpen.value = "Open";
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
    var divBrDetailInfo = document.getElementById('divBrDetailInfo')
    /* 費用詳細が開かれていない場合はそのまま終了 */
    if (divBrDetailInfo === null) {
        return;
    }

    /* divBrDetailInfo内の削除ボタンを取得 */
    var deleteCostButtonObjects = divBrDetailInfo.querySelectorAll('input[id^="btnDeleteCostItem_"]');
    if (deleteCostButtonObjects !== null) {
        if (deleteCostButtonObjects.length > 0) {
            for (var i = 0, len = deleteCostButtonObjects.length; i < len; ++i) {
                var obj = deleteCostButtonObjects[i];
                /* クリックイベントに紐づけ */
                obj.addEventListener('click', (function (obj) {
                    return function () {
                        deleteCostClick(obj);
                    };
                })(obj), false);
            }
        }
    }
    /* divBrDetailInfo内の業者テキストボックスを取得 */
    var contractorTextObjects = divBrDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtContractor_"]');
    if (contractorTextObjects !== null) {
        if (contractorTextObjects.length > 0) {
            for (var i = 0, len = contractorTextObjects.length; i < len; ++i) {
                var obj = contractorTextObjects[i];
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
                            contractorDbClick(obj);
                        };
                    })(obj), false);
                }

                obj = document.getElementById(contractorTextObjects[i].id);
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
                            calcContractor(obj);
                        }
                    };
                }(obj), false);
            }
        }
    }
    /* divBrDetailInfo内のUSDテキストボックスを取得 */
    var usdTextObjects = divBrDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtUsd_"]');
    if (usdTextObjects !== null) {
        if (usdTextObjects.length > 0) {
            for (var i = 0, len = usdTextObjects.length; i < len; ++i) {
                var obj = usdTextObjects[i]
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
                            calcSummaryCostUsd(obj);
                        }
                    };
                }(obj), false);
            }
        }
    }
    /* divBrDetailInfo内のApprovedUSDテキストボックスを取得 */
    var appUsdTextObjects = divBrDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtApprovedUsd_"]');
    if (appUsdTextObjects !== null) {
        if (appUsdTextObjects.length > 0) {
            for (var i = 0, len = appUsdTextObjects.length; i < len; ++i) {
                var obj = appUsdTextObjects[i]
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
                            calcSummaryCostAppUsd(obj);
                        }
                    };
                }(obj), false);
            }
        }
    }
    /* divBrDetailInfo内のLOCALテキストボックスを取得 */
    var localTextObjects = divBrDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtLocal_"]');
    if (localTextObjects !== null) {
        if (localTextObjects.length > 0) {
            for (var i = 0, len = localTextObjects.length; i < len; ++i) {
                var obj = localTextObjects[i]
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
                            calcSummaryCostLocal(obj);
                        }
                    };
                }(obj), false);
            }
        }
    }
    /* 費用項目の所見エリアダブルクリック時イベントバインド */
    /* divBrDetailInfo内のLOCALテキストボックスを取得 */
    var remarksSpanObjects = divBrDetailInfo.querySelectorAll('span[id^="spnCostRemarks_"]');
    if (remarksSpanObjects !== null) {
        if (remarksSpanObjects.length > 0) {
            /* グリッド自体のEnabledが切られていた場合は使用不可 */
            var gridItem = document.getElementById("gvDetailInfo");
            var canBind = true;
            if (gridItem.class !== null) {
                if (gridItem.class === 'aspNetDisabled') {
                    canBind = false;
                }
            }

            if (canBind === true) {
                for (var i = 0, len = remarksSpanObjects.length; i < len; ++i) {
                    var obj = remarksSpanObjects[i]
                    /* クリックイベントに紐づけ */
                    obj.addEventListener('dblclick', (function (obj) {
                        return function () {
                            displayCostRemarkbox(obj);
                        };
                    })(obj), false);
                }
            }
        } 
    }
}
///* 編集ボタン押下処理 */
//function bindEditOnClick() {
//    var btnEdit = document.getElementById('btnRemarkInputEdit');
//    /* 編集ボタンが存在しない場合はそのまま終了 */
//    if (btnEdit === null) {
//        return;
//    }
//    /* 編集ボタンにイベントをバインド */
//    btnCostAdd.addEventListener('click', (function () {
//        var submitObj = document.getElementById('hdnSubmit');
//        var viewId = 'vLeftCost';
//        var dblClickObject = document.getElementById('hdnTextDbClickField');
//        var viewIdObject = document.getElementById('hdnLeftboxActiveViewId');
//        var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
//        if (submitObj.value === 'FALSE') {
//            submitObj.value = 'TRUE';
//            dblClickObject.value = 'gvCostList';
//            viewIdObject.value = viewId;
//            leftBoxOpen.value = "Open";
//            document.forms[0].submit();
//        }
//    }), false);

//}
/* 削除ボタン押下時処理 */
function deleteCostClick(obj) {
    var uniIndex = document.getElementById('hdnDelteCostUniqueIndex'); 
    if (obj === null) {
        return;
    }
    if (uniIndex === null) {
        return;
    }
    var submitobj = document.getElementById('hdnSubmit'); 
    if (submitobj.value === 'FALSE') {
        submitobj.value = 'TRUE';
        uniIndex.value = obj.dataset.uniqueindex;
        commonDispWait();
        document.forms[0].submit();
    }

}
/* 業者コードダブルクリックイベント */
/* 引数:objダブルクリックしたテキストボックスオブジェクト */
function contractorDbClick(obj) {
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
        dbClickField.value = "txtContractor";
        leftboxActiveViewId.value = "vLeftContractor";
        currentUnieuqIndexObj.value = obj.dataset.uniqueindex;
        isLeftBoxOpen.value = "Open";
        commonDispWait();
        document.forms[0].submit();
    }
}
/* 費用タブでのUSDコスト計算 */
function calcSummaryCostUsd(obj) {
    var divBrDetailInfo = document.getElementById('divBrDetailInfo')
    if (divBrDetailInfo === null) {
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
        calcFunctionName.value = 'CalcSummaryCostUsd';
        commonDispWait();
        document.forms[0].submit();
    }
} 
/* 費用タブでの承認USDコスト計算 */
function calcSummaryCostAppUsd(obj) {
    var divBrDetailInfo = document.getElementById('divBrDetailInfo')
    if (divBrDetailInfo === null) {
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
        calcFunctionName.value = 'CalcSummaryCostAppUsd';
        commonDispWait();
        document.forms[0].submit();
    }
} 
/* 費用タブでのUSDコスト計算 */
function calcSummaryCostLocal(obj) {
    var divBrDetailInfo = document.getElementById('divBrDetailInfo')
    if (divBrDetailInfo === null) {
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
        calcFunctionName.value = 'CalcSummaryCostLocal';
        commonDispWait();
        document.forms[0].submit();
    }
} 
/* 費用名変更
   費用コード変更時に反映  */
function calcContractor(obj) {
    var divBrDetailInfo = document.getElementById('divBrDetailInfo')
    if (divBrDetailInfo === null) {
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

///* 発着タブについて費用の合計を計算する */
///* 引数：costColumn:'USD'または'LOCAL'を設定 */
//function calcSummaryCost(costColumn) {
//    var divBrDetailInfo = document.getElementById('divBrDetailInfo')
//    if (divBrDetailInfo === null) {
//        return;
//    }
//    var summary;
//    var costElements;
//    if (costColumn === "USD") {
//        summary = document.getElementById("iptAgencySummaryUsd");
//        costElements = divBrDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtUsd_"]');
//    } else {
//        summary = document.getElementById("iptAgencySummaryLocal");
//        costElements = divBrDetailInfo.querySelectorAll('input[id^="gvDetailInfo_txtLocal_"]');
//    }
//    /* 必要項目がレンダリングされていない場合はそのまま終了 */
//    if (costElements === null) {
//        return true;
//    }
//    if (costElements.length === 0) {
//        return;
//    }
//    if (summary === null) {
//        return;
//    }

//    summary.value = ''; //合計値を一旦ブランク

//    var summaryValue = 0;
//    for (var i = 0, len = costElements.length; i < len; ++i) {
//        var numString = costElements[i].value.replace(/,/g, "");
//        var num = parseFloat(numString, 10);
//        if (isNaN(num) == false) {
//            summaryValue = summaryValue + num;
//            costElements[i].value = formatNumber(num,2);
//        }
//    }
    
//    summary.value = formatNumber(summaryValue,2);
//}
/* コメント表示時の左ボックス表示イベント */
function displayCostRemarkbox(obj) {
    var remarkBoxOpenObj = document.getElementById('hdnRemarkboxOpen');
    var submitObj = document.getElementById('hdnSubmit');
    var remarkboxField = document.getElementById('hdnRemarkboxField');
    var currentUnieuqIndexObj = document.getElementById('hdnCurrentUnieuqIndex'); 
    var leftBoxOpen = document.getElementById('hdnIsLeftBoxOpen');
    var fieldDisplayNameObj = document.getElementById('hdnRemarkboxFieldName');
    var parentRowIndex = obj.parentElement.parentElement.rowIndex;
    var grid = document.getElementById('gvDetailInfo');
    var costCodeField = grid.rows[parentRowIndex].cells[1];
    var costNameField = grid.rows[parentRowIndex].cells[2];
    var fieldName = parentRowIndex + ':' + costCodeField.innerText + '(' + costNameField.innerText + ') ' + grid.rows[0].cells[3].innerText;
    /* 表示切替 */
    if (submitObj !== 'FALSE' || remarkBoxOpenObj.value !== 'Open') {
        submitObj.value = 'TRUE';
        currentUnieuqIndexObj.value = obj.dataset.uniqueindex;
        remarkboxField.value = 'lblCostRemarks';
        remarkBoxOpenObj.value = 'Open';
        leftBoxOpen.value = ''; /* 右ボックスとの共存不可 */
        fieldDisplayNameObj.value = fieldName;
        commonDispWait();
        document.forms[0].submit();

    }

}
/* ブレーカー備考欄 */
function bindRemarkDblClick() {
    var dblClickRemarkObjects = [['spnBrRemark', 'lblBrRemarkText'],
                                 ['spnApplyRemarks', 'lblApplyRemarks'],
                                 ['spnAppJotRemarks', 'lblAppJotRemarks'],
                                 ['spnRemarks', 'lblRemarks']
                                ];
    for (let i = 0; i < dblClickRemarkObjects.length; i++) {
        /* ダブルクリックオブジェクト */
        var obj = document.getElementById(dblClickRemarkObjects[i][0]);
        var lblObj = document.getElementById(dblClickRemarkObjects[i][1]);
        /* オブジェクトの存在チェック(存在しない場合はスキップ) */
        if (obj === null || lblObj === null) {
            continue;
        }
        ///* 使用不可の場合もスキップ */
        //if (lblObj.class !== null) {
        //    if (lblObj.class === 'aspNetDisabled') {
        //        continue;
        //    }
        //}
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
            case "lblBrRemarkText":
                fieldDisplayName = document.getElementById('lblBrRemark').innerText;
                break;
            case "lblApplyRemarks":
                fieldDisplayName = document.getElementById('lblApproval').innerText;
                fieldDisplayName = fieldDisplayName + ' ' + document.getElementById('lblAppRemarks').innerText;
                break;
            case "lblAppJotRemarks":
                fieldDisplayName = document.getElementById('lblApproved').innerText;
                fieldDisplayName = fieldDisplayName + ' ' + document.getElementById('lblAppRemarks').innerText;
                break;
            case "lblRemarks":
                fieldDisplayName = document.getElementById('lblRemark').innerText;
                break;
        }
        fieldDisplayNameObj.value = fieldDisplayName;
        remarkBoxOpenObj.value = 'Open';
        leftBoxOpen.value = ''; /* 右ボックスとの共存不可 */
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

function bindDoubleClickEvent(targetId) {
    // 引数未指定や配列がない場合は終了
    if (targetId === '') {
        return;
    }

    /* オブジェクトの存在チェック(存在しない場合はスキップ) */
    if (document.getElementById(targetId) === null) {
        return;
    }
    /* ダブルクリックイベントに紐づけ */
    var inputObject = document.getElementById(targetId);

    // フォーカスを合わさないとテキストボックスはダブルクリックに反応しないため、
    // 前後をspanタグで括りそちらにダブルクリックイベントを記載
    var wrapper = document.createElement('span');
    wrapper.appendChild(inputObject.cloneNode(true));
    inputObject.parentNode.replaceChild(wrapper, inputObject);

    wrapper.ondblclick = (function (targetId) {
        return function () {
            var submitObj = document.getElementById('hdnSubmit');
            if (submitObj.value === 'FALSE') {
                submitObj.value = 'TRUE';
                document.getElementById("hdnDbClickField").value = targetId;
                commonDispWait();
                document.forms[0].submit();
            }
        };
    })(targetId);
}


// 必要な場合適宜関数、処理を追加
// ダウンロード処理
function f_ExcelPrint() {
    // リンク参照
    var printUrlObj = document.getElementById("hdnPrintURL");
    if (printUrlObj === null) {
        return;
    }
    window.open(printUrlObj.value + '?date=' + new Date().getTime() + '', "view", "scrollbars=yes,resizable=yes,status=yes");
    printUrlObj.value = '';
};
function f_DownLoad() {
    // リンク参照
    var dwnUrlObj = document.getElementById("hdnZipURL");
    if (dwnUrlObj === null) {
        return;
    }
    window.open(dwnUrlObj.value + '?date=' + new Date().getTime() + '' , "view", "scrollbars=yes,resizable=yes,status=yes");
    dwnUrlObj.value = '';
}; 

// ドロップ処理（処理抑止）
function f_dragEventCancel(event) {
    event.preventDefault();  //イベントをキャンセル
};

// チェック変更
function f_checkEvent(event) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById('hdnBulkCheckChange').value = event.target.checked;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
};

// Leaseチェック変更
function f_checkLeaseEvent(event) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById('hdnLeaseCheckChange').value = event.target.checked;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
};

// 費用チェック変更
function f_checkAppEvent(obj) {
    if (document.getElementById("hdnSubmit").value == "FALSE") {
        document.getElementById("hdnSubmit").value = "TRUE"
        document.getElementById('hdnCheckAppChange').value = obj.checked;
        document.getElementById('hdnCheckUniqueNumber').value = obj.parentNode.dataset.uniqueindex;
        commonDispWait();
        document.forms[0].submit();                            //aspx起動
    }
};

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