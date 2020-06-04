/* 第１カラムと第２カラムの項目値を数値比較し、文字色を変更（主に金額） */
/* 別途cssにlessThan(1>2)、greatherThan(1<2)の色定義が必要 */
function setCompareNumBackGroundColor(firstCol, secondCol, ColerCol, PanelID) {
    var fCostColumnObj = getTargetColumnNoTable(firstCol, PanelID);
    var sCostColumnObj = getTargetColumnNoTable(secondCol, PanelID);
    //比較対象のカラムが存在していない場合は実行不可能
    if (fCostColumnObj !== null && sCostColumnObj !== null) {
        let fColumnNo = fCostColumnObj.ColumnNo;
        let fTable = fCostColumnObj.TargetTable;
        let sColumnNo = sCostColumnObj.ColumnNo;
        let sTable = sCostColumnObj.TargetTable;
        // 第２項目のみテキストボックスに対応 //
        let sIsTextObj = false;
        if (sTable.rows.length !== 0) {
            let checkCell = sTable.rows[0].cells[sColumnNo];
            if (checkCell.querySelectorAll('input[type=text]').length === 1) {
                sIsTextObj = true;
            }
            for (let i = 0; i < fTable.rows.length; i++) {
                let fValueObj = fTable.rows[i].cells[fColumnNo];
                let sValueObj;
                let sValue;
                if (sIsTextObj) {
                    sValueObj = sTable.rows[i].cells[sColumnNo].querySelectorAll('input[type=text]')[0];
                    sValue = sValueObj.value;
                    sValueObj.onblur = (function (sValueObj, compareValue) {
                        return function () {
                            costValueChange(sValueObj, compareValue);
                        };

                    })(sValueObj, fValueObj.textContent);
                } else {
                    sValueObj = sTable.rows[i].cells[sColumnNo];
                    sValue = sValueObj.textContent;
                }
                var fValue = fValueObj.textContent;
                styleClass = compareCostValue(fValue, sValue);
                if (styleClass !== '') {
                    if (ColerCol === 'F' || ColerCol === 'B') {
                        fValueObj.classList.add(styleClass);
                    }
                    if (ColerCol === 'S' || ColerCol === 'B') {
                        sValueObj.classList.add(styleClass);
                    }
                }
            }
        }
    }
}
/* 文字列の2つの費用値を検索 */
function compareCostValue(firstVal, secondVal) {
    let firstValWOComma = firstVal.replace(/,/g, '');
    let secondValWOComma = secondVal.replace(/,/g, '');
    if (firstValWOComma === '') {
        firstValWOComma = '0';
    }
    if (secondValWOComma === '') {
        secondValWOComma = '0';
    }

    if (isNaN(firstValWOComma) || isNaN(firstValWOComma)) {
        return '';
    }
    let firstNum = new Number(firstValWOComma);
    let secondNum = new Number(secondValWOComma);
    if (firstNum > secondNum) {
        return 'greatherThan';
    } else if (firstNum < secondNum) {
        return 'lessThan';
    } else {
        return '';
    }
}
/* 費用項目変更時イベント */
/* 引数：targetObject 費用項目テキストボックス */
/*       compareValue 比較対象の値 */
function costValueChange(targetObject, compareValue) {
    var objVal = targetObject.value;
    var styleClass = compareCostValue(compareValue, objVal);
    targetObject.classList.remove('greatherThan');
    targetObject.classList.remove('lessThan');
    if (styleClass !== '') {
        targetObject.classList.add(styleClass);
    }
    reportMonthControl(styleClass, targetObject.getAttribute('rownum'));
    applyEnableControl(styleClass, targetObject.getAttribute('rownum'));
}