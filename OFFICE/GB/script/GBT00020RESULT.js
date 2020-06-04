var dispTemplateTimeOut = 100;
var mbuttonAreaObj;
var mTemplateMouseOverObj;
var setTimeToHideID;

function bindDisplayTemplateBtn() {
    var spnMouseOverObj = document.getElementById('lblTemplateDownload');
    var divButtonItemAreaObj = document.getElementById('divTemplateItems');
    if (spnMouseOverObj === null || divButtonItemAreaObj === null) {
        return;
    }
    spnMouseOverObj.onmouseover = function () { displayTemplateBtn(divButtonItemAreaObj.id); };
    spnMouseOverObj.onmouseout = function () { hideTemplateBtnTimer(); }
    divButtonItemAreaObj.onmouseover = function () { resetHideTemplateBtnTimer(); };
    divButtonItemAreaObj.onmouseout = function () { hideTemplateBtnTimer(); }
}

function displayTemplateBtn(objId) {
    if (mbuttonAreaObj) {
        mbuttonAreaObj.style.display = 'none';
        mTemplateMouseOverObj.style.backgroundColor = "";
    }
    mbuttonAreaObj = document.getElementById(objId);
    mbuttonAreaObj.style.display = 'block';
    mTemplateMouseOverObj = document.getElementById('lblTemplateDownload');
    mTemplateMouseOverObj.style.backgroundColor = "#DE9292";
}

function hideTemplateBtnTimer() {
    setTimeToHideID = window.setTimeout(hideTemplateBtn, dispTemplateTimeOut); 
}

function hideTemplateBtn() {
    mbuttonAreaObj.style.display = 'none';
    mTemplateMouseOverObj.style.backgroundColor = "";
}

function resetHideTemplateBtnTimer() {
    if (setTimeToHideID) {
        window.clearTimeout(setTimeToHideID);
        setTimeToHideID = 0; 
    }
}
function addAgreement(brid) {
    var hdnSelectedBrIdObj = document.getElementById('hdnSelectedBrId');
    if (hdnSelectedBrIdObj === null) {
        return;
    }
    hdnSelectedBrIdObj.value = brid;
    buttonClick('btnAddAgreement');
}