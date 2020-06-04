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
        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
        if (activeElem !== null || document.activeElement.id !== null) {
            activeElem.value = document.activeElement.id;
        }
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
        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
        if (activeElem !== null || document.activeElement.id !== null) {
            activeElem.value = document.activeElement.id;
        }
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

    var capacity = document.getElementById('txtTankCapacity');
    if (capacity === null) {
        return;
    }

    /* フォーカスイベント紐づけ */
    capacity.addEventListener('focus', function (capacity) {
        return function () {
            capacity._oldvalue = capacity.value;
        };
    }(capacity), false);

    /* ブラーイベントに紐づけ */
    capacity.addEventListener('blur', function (capacity) {
        return function () {
            if (capacity._oldvalue !== capacity.value) {
                calcFillingRate();
            }
        };
    }(capacity), false);
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
        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
        if (activeElem !== null || document.activeElement.id !== null) {
            activeElem.value = document.activeElement.id;
        }
        commonDispWait();
        document.forms[0].submit();
    }

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
        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
        if (activeElem !== null || document.activeElement.id !== null) {
            activeElem.value = document.activeElement.id;
        }
        commonDispWait();
        document.forms[0].submit();
    }
    
    ///* 数値変換を行い演算 */
    //var totalCostNum = parseFloat(totalCost.value.replace(/,/g, ''));
    //var jotHireageNum = Math.floor(parseFloat(jotHireage.value.replace(/,/g, '')) * 100) / 100;
    //jotHireage.value = formatNumber(jotHireageNum, 2);
    //var commercialFactorNum = Math.floor(parseFloat(commercialFactor.value.replace(/,/g, '')) * 100) / 100;
    //var totalSpanNum = parseFloat(totalSpan.value.replace(/,/g, ''));
    //commercialFactor.value = formatNumber(commercialFactorNum, 2);
    //var invoicedTotalNum = jotHireageNum * totalSpanNum + commercialFactorNum + totalCostNum;
    
    //invoiceTotal.value = formatNumber(invoicedTotalNum * 100 / 100, 2);
    //parDay.value = formatNumber((invoicedTotalNum - totalCostNum) / totalSpanNum,2);
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
        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
        if (activeElem !== null || document.activeElement.id !== null) {
            activeElem.value = document.activeElement.id;
        }
        commonDispWait();
        document.forms[0].submit();
    }
    ///* 数値変換を行い演算 */
    //var totalCostNum = parseFloat(totalCost.value.replace(/,/g, ''));
    //var invoicedTotalNum = Math.floor(parseFloat(invoiceTotal.value.replace(/,/g, '')) * 100) / 100;
    //invoiceTotal.value = formatNumber(invoicedTotalNum, 2);
    //var totalSpanNum = parseFloat(totalSpan.value.replace(/,/g, ''));

    //var hireagePerDayNum = Math.floor(((invoicedTotalNum - totalCostNum) / totalSpanNum) * 100) / 100;
    //hireagePerDay.value = formatNumber(hireagePerDayNum, 2);

    //var jotHireageNum = totalSpanNum * hireagePerDayNum;
    //jotHireage.value = formatNumber(jotHireageNum, 2);

    //var commercialFactorNum = Math.floor((invoicedTotalNum - totalCostNum - jotHireageNum) * 100) / 100;
    //commercialFactor.value = formatNumber(commercialFactorNum, 2);

}

/* inputリクエストボタン及びポップアップのボタンにイベントを紐づける */
function bindInputRequestOnClick() {
    var buttonObj = document.getElementById('btnInputRequest');
    var popAreaBox = document.getElementById('divSendConfirmBoxWrapper');
    var buttonCancel = document.getElementById('btnSelectMailCancel');
    var buttonOk = document.getElementById('btnSelectMailOk');

    if (buttonObj !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonObj.addEventListener('click', (function (popAreaBox) {
            return function () {
                popAreaBox.style.display = 'block';
            };
        })(popAreaBox), false);
    }

    if (buttonCancel !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonCancel.addEventListener('click', (function (popAreaBox) {
            return function () {
                popAreaBox.style.display = 'none';

            };
        })(popAreaBox), false);
    }
    if (buttonOk !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonOk.addEventListener('click', (function (popAreaBox) {
            return function () {

                var hdnSubmitObj = document.getElementById('hdnSubmit');
                var hdnButtonClickId = document.getElementById('hdnButtonClick');
                if (hdnSubmitObj.value === 'FALSE') {
                    hdnSubmitObj.value = 'TRUE';
                    popAreaBox.style.display = 'none';
                    hdnButtonClickId.value = 'btnSelectMailCancel';
                    commonDispWait();
                    document.forms[0].submit();
                }
            };
        })(popAreaBox), false);
    }
}

/* EntryCostボタン及びポップアップのボタンにイベントを紐づける */
function bindEntryCostOnClick() {
    var changeFlg = document.getElementById('hdnMsgboxChangeFlg');
    var showFlg = document.getElementById('hdnMsgboxShowFlg');
    var buttonObj = document.getElementById('btnEntryCost');
    var popAreaBox = document.getElementById('divEntryCostSendConfirmBoxWrapper');
    var buttonCancel = document.getElementById('btnEntryCostSelectMailCancel');
    var buttonOk = document.getElementById('btnEntryCostSelectMailOk');
    var buttonYes = document.getElementById('btnEntryCostSelectMailYes');
    var buttonNo = document.getElementById('btnEntryCostSelectMailNo');

    if (changeFlg.value !== '') {

        popAreaBox.style.display = 'block';
        if (showFlg.value === '1') {
            buttonOk.style.display = 'none';
        } else if (showFlg.value === '0') {
            buttonYes.style.display = 'none';
            buttonNo.style.display = 'none';
        }
    }

    if (buttonObj !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonObj.addEventListener('click', (function (popAreaBox) {
            return function () {

                var hdnSubmitObj = document.getElementById('hdnSubmit');
                var hdnButtonClickId = document.getElementById('hdnButtonClick');
                hdnSubmitObj.value = 'TRUE';
                popAreaBox.style.display = 'none';
                hdnButtonClickId.value = 'btnEntryCost';
                commonDispWait();
                document.forms[0].submit();

            };
        })(popAreaBox), false);
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
    if (buttonOk !== null && popAreaBox !== null) {
        /* クリックイベントに紐づけ */
        buttonOk.addEventListener('click', (function (popAreaBox, changeFlg) {
            return function () {
                changeFlg.value = '';
                var hdnSubmitObj = document.getElementById('hdnSubmit');
                var hdnButtonClickId = document.getElementById('hdnButtonClick');
                if (hdnSubmitObj.value === 'FALSE') {
                    hdnSubmitObj.value = 'TRUE';
                    popAreaBox.style.display = 'none';
                    hdnButtonClickId.value = 'btnEntryCostSelectMailOk';
                    commonDispWait();
                    document.forms[0].submit();
                }
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
                    hdnButtonClickId.value = 'btnEntryCostSelectMailYes';
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
                    hdnButtonClickId.value = 'btnEntryCostSelectMailNo';
                    commonDispWait();
                    document.forms[0].submit();
                }
            };
        })(popAreaBox), false);
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
                    hdnButtonClickId.value = 'btnApplyMsgYes';
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
                    hdnButtonClickId.value = 'btnApplyMsgNo';
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
        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
        if (activeElem !== null || document.activeElement.id !== null) {
            activeElem.value = document.activeElement.id;
        }
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
        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
        if (activeElem !== null || document.activeElement.id !== null) {
            activeElem.value = document.activeElement.id;
        }
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
        var activeElem = document.getElementById('hdnActiveElementAfterOnChange');
        if (activeElem !== null || document.activeElement.id !== null) {
            activeElem.value = document.activeElement.id;
        }
        commonDispWait();
        document.forms[0].submit();
    }
}

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
    var fieldName = parentRowIndex + ':' + costCodeField.innerText + '(' + costNameField.innerText + ') ' + grid.rows[0].cells[11].innerText;
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
                                 ['spnRemarks', 'lblRemarks'],
                                 ['spnRemarks2', 'lblRemarks2']
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
            case "lblRemarks2":
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
