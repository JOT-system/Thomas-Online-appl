<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00004ORDER.aspx.vb" Inherits="OFFICE.GBT00004ORDER" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <asp:PlaceHolder ID="phCommonHeader" runat="server"></asp:PlaceHolder>
    <link rel="icon" type="image/png" href="~/images/favicon.png" />
    <%--フォームのID以外でタイトルを設定する場合は適宜変更--%>
    <title><%= Me.Form.ClientId %></title>
    <%--全画面共通のスタイルシート --%>
    <link href="~/css/commonStyle.css" rel="stylesheet" type="text/css" />
    <%--個別のスタイルは以下に記載 OR 外部ファイルに逃す --%>
    <link href="~/GB/css/GBT00004ORDER.css" rel="stylesheet" type="text/css" />
    <style>
        /* サーバータグが使えるページ内に内報グリッドボタンのボタン名 */
        #WF_LISTAREA_DL td:nth-child(4) button:after{
            content:"<%= Me.hdnListDeleteName.Value %>";
        }
    </style>
    <!-- Global site tag (gtag.js) - Google Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=UA-162522994-1"></script>
    <script>
      window.dataLayer = window.dataLayer || [];
      function gtag(){dataLayer.push(arguments);}
      gtag('js', new Date());

      gtag('config', 'UA-162522994-1');
    </script>
    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8" ></script>
    <%-- 左ボックスカレンダー使用の場合のスクリプト --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/script/calendar.js") %>'  charset="utf-8"></script>
    <%--個別のスクリプトは以下に記載 --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00004ORDER.js?dtm=20190422A") %>'  charset="utf-8"></script>
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加
        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>', '<%= Me.btnExtract.ClientID %>',
                                       '<%= Me.btnExcelDownload.ClientID %>', '<%= Me.btnSave.ClientID %>',
                                       '<%= Me.btnFIRST.ClientID %>', '<%= Me.btnLAST.ClientID %>',
                                       '<%= Me.btnApply.ClientID %>',
                                       '<%= Me.btnRemarkInputOk.ClientID %>', '<%= Me.btnRemarkInputCancel.ClientID %>',
                                       '<%= Me.btnBliingClose.ClientID %>' 
                                      ];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';
            var viewCostId = '<%= Me.vLeftCost.Clientid %>';
            var viewAddCostId = '<%= Me.vLeftAddCost.Clientid %>';
            var viewActyId = '<%= Me.vLeftActy.ClientID  %>';
            var viewVender = '<%= Me.vLeftVender.ClientID  %>';
            var viewBreakerType = '';
            var dblClickObjects = [['<%= Me.txtCostItem.Clientid  %>', viewCostId], ['<%= Me.txtActy.ClientId %>', viewActyId],
                                   ['<%= Me.txtVender.ClientId %>', viewVender],
                                   ['<%= Me.txtBrVender.ClientId %>', viewVender],
                                   ['<%= Me.txtEstimatedVender.ClientId %>',viewVender]
                                  ];

            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();
            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbCost.ClientID %>', '3', '1'],
                                           ['<%= Me.lbAddCost.ClientID %>', '3', '1'],
                                           ['<%= Me.lbAddNbCost.ClientID %>', '3', '1'],
                                           ['<%= Me.lbActy.ClientID %>', '3', '1'],
                                           ['<%= Me.lbContractor.ClientID %>','3','1'],
                                           ['<%= Me.lbVender.ClientID %>','3','1'],
                                          ];
            addLeftBoxExtention(leftListExtentionTarget);
            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = ['<%= Me.txtCostItem.ClientID %>', '<%= Me.txtActy.ClientId %>',
                                         '<%= Me.txtVender.ClientId %>', '<%= Me.txtBrVender.ClientId %>',
                                         '<%= Me.txtEstimatedVender.ClientId %>'];
            bindTextOnchangeEvent(targetOnchangeObjects);

            /* 右ボックスの開閉ダブルクリックイベントバインド
               右上透明ボックス、下のメッセージ欄、他がある場合は個別で　*/
            bindDiplayRightBoxEvent();
            /* ヘルプボタン表示のダブルクリックイベントバインド */
            bindFooterShowHelpEvent();
            /**
             * 一覧表制御 
             */
            /* チェックボックスのチェック値復元 */
            listChkBoxControl('<%= Me.WF_LISTAREA.ClientID %>');
            /* ブレーカー情報のテキストなど使用可否制御 */
            gridDispControl("<%= Me.hdnListDeleteName.Value %>");
            /* 一覧表日付項目カレンダー表示イベントバインド */
            bindListDateTextbox();
            /* 一覧表通貨コード項目選択ボックス表示イベントバインド */
            bindListCurrencyTextbox();
            /* 一覧表通貨コード項目選択ボックス表示イベントバインド */
            bindListContractorTextbox();
            /* 汎用補助区分　項目選択ボックス表示イベントバインド */
            bindListAccCurrencySegmentTextbox();
            /* 費用追加ボタンイベントバインド */
            bindAddCostOnClick();
            /* 費用削除ボタンイベントバインド */
            bindCostListDeleteOnClick();
            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();
            /* ヘルプ表示処理 */
            openHelpPage(); /* hdnCanHelpOpenに"1"が立たない限り開きません。 */

            /* カレンダー描画処理 */
            var calValueObj = document.getElementById('<%= Me.hdnCalendarValue.ClientID %>');
            if (calValueObj !== null) {
                var billingYmdObj = document.getElementById('<%= Me.hdnBillingYmd.ClientID %>');
                var billingYmdValue = '';
                if (billingYmdObj !== null) {
                    billingYmdValue = billingYmdObj.value
                }
                /* 日付格納隠し項目がレンダリングされている場合のみ実行 */
                if (billingYmdValue === '') {
                    carenda(0);
                } else {
                    carenda(0,null,billingYmdValue);
                }
                setAltMsg(firstAltYMD, firstAltMsg);
            }
            /* ADDコスト時の高さ調整 */
            var findObj = document.getElementById('<%= me.ddlNo.Clientid %>'); //390px
            if (findObj !== null) {
                let leftCont = document.getElementById('divLeftbox');
                let listElements = leftCont.querySelectorAll("select[id^='lb']");
                if (listElements === null) {
                    return;
                }
                if (listElements.length === 0) {
                    return;
                }
                let listElement = listElements[0];
                listElement.style.minHeight = "335px";
            }
            /* ファイルドラッグ＆ドロップのイベントバインド */
            var dragDropObj = document.getElementById('divContainer');
            var listObj = document.getElementById('WF_LISTAREA');
            if (dragDropObj !== null && listObj.classList.contains('aspNetDisabled') === false) {
                dragDropObj.addEventListener("dragstart", f_dragEventCancel, false);
                dragDropObj.addEventListener("drag", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragend", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragenter", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragleave", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragover", f_dragEventCancel, false);
                dragDropObj.addEventListener("drop", f_dragEvent, false);
            }
            /* 費用入力項目の同行の金額と比較し色を付ける*/
            setCompareNumBackGroundColor('AMOUNTBR','AMOUNTFIX','S'); // OriginalとEstimated差異
            setCostPriceBackGroundColor(); // 変更時のイベントも同様に紐づけ
            bindCalcDemAgentComm();
            /* アップロードボタンの設定 */
            addUploadExtention('<%= Me.btnExcelDownload.ClientID %>', 'AFTER', false, 'divContainer');

            /* 共通一覧のスクロールイベント紐づけ */
            bindPrevScroll('<%= Me.WF_LISTAREA.ClientId %>');
            bindListCommonEvents('<%= Me.WF_LISTAREA.ClientId %>','<%= if(IsPostBack = True, "1", "0") %>',true);
            /* 検索ボックス生成 */
            commonCreateSearchArea('orderHeaderBox');

            /* Remarkボックスの前画面操作抑止 */
            var divRemarkInputBoxWrapperObj = document.getElementById('divRemarkInputBoxWrapper');
            if (divRemarkInputBoxWrapperObj !== null) {
                if (divRemarkInputBoxWrapperObj.style.display !== 'none') {
                    commonDisableModalBg(divRemarkInputBoxWrapperObj.id);
                }
            }

            screenUnlock();
            var rblPolPodObj = document.getElementById('rblPolPod');
            if (rblPolPodObj === null) {
                focusAfterChange();
            } else {
                var selectedChk = rblPolPodObj.querySelectorAll("input[type=radio][checked='checked']");
                if (selectedChk !== null) {
                    if (selectedChk[0] !== null) {
                        selectedChk[0].focus();
                    }
                }
            }
        });

    </script>
</head>
<%-- 基本的にタグ内でのクライアントサイドのJavaScriptのイベント記述はせず、
    ヘッダーにあるwindow.onloadでイベントバインドをします。
    スタイルなども直接記述は極力行わないように
    ※%付きのコメントはHTMLソース表示でもレンダリングされないものです --%>
<body style="visibility:visible;">
    <%--FormIDは適宜変更ください。 --%>
    <form id="GBT00004" runat="server">
        <%--ヘッダーボックス --%>
        <div id="divContainer">
            <div id="divTitlebox">
                <table id="tblTitlebox">
                    <tr>
                        <td>
                            <%= IIf(Me.lblTitleId.Text <> "", "ID:", "") %>
                            <asp:Label ID="lblTitleId"   runat ="server" Text=""></asp:Label>
                        </td>
                        <td rowspan="2">
                            <asp:Label ID="lblTitleText" runat="server" Text=""></asp:Label>
                        </td>
                        <td >
                            <asp:Label ID="lblTitleCompany" runat="server" Text=""></asp:Label>
                        </td>
                        <td rowspan="2">
                            <div id="divShowRightBoxBg"><div id="divShowRightBox" ></div></div>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:Label ID="lblTitleOffice" runat="server" Text=""></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblTitleDate" runat="server" Text=""></asp:Label>
                        </td>
                    </tr>
                </table>
            </div>
            <%--コンテンツボックス(このdiv内に適宜追加お願いします) --%>
            <div id="divContensbox">
                <%-- ************************************** --%>
                <%--ご自由に！！(このコメントは消してください) --%>
                <%-- ************************************** --%>
                <div id="actionButtonsBox">
                    <input id="btnExtract" type="button" value="絞り込み"  runat="server"  />
                    <input id="btnApply" type="button" value="申請"  runat="server"  />
                    <input id="btnBliingClose" type="button" value="精算締め"  runat="server" />
                    <input id="btnExcelDownload" type="button" value="Excelダウンロード"  runat="server"  />
                    <input id="btnSave" type="button" value="保存"  runat="server"  />
                    <input id="btnBack" type="button" value="戻る"  runat="server"  />
                    <div id="btnFIRST" class="firstPage" runat="server"></div>
                    <div id="btnLAST" class="lastPage" runat="server"></div>
                </div>
                <div id="orderHeaderBox" runat="server"> <%-- 機能によって見え隠れする可能性がある為一旦runat serverにしておく --%>
                </div>
                <div id="divSearchConditionBox">
                    <asp:HiddenField ID="hdnSearchConditionDetailOpenFlg" runat="server" Value="" />
                    <span id="spnClosingDate" runat="server">
                        <asp:Label ID="lblClosingDateLabel" runat="server" Text=""></asp:Label>
                        <asp:Label ID="lblClosingDate" runat="server" Text=""></asp:Label>
                    </span>
                    <span id="spnUsdAmountSummary"  runat="server">
                        <asp:Label ID="lblUsdAmountSummaryLabel" runat="server" Text=""></asp:Label>
                        <asp:Label ID="lblUsdAmountSummary" runat="server" Text=""></asp:Label>
                    </span>
                    <span id="spnOrderNo" runat="server">
                        <asp:Label ID="lblOrderNoLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtOrderNo" runat="server"></asp:TextBox>
                    </span>
                    <span id="spnTankNo" runat="server">
                        <asp:Label ID="lblTankNoLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtTankNo" runat="server" Text=""></asp:TextBox>
                    </span>
                    <span id="spnActy" runat="server">
                        <asp:Label ID="lblActy" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtActy" runat="server" Text=""></asp:TextBox>
                        <asp:Label ID="lblActyText" runat="server" Text=""></asp:Label>
                    </span>
                    <span id="spnCostItem" runat="server">
                        <asp:Label ID="lblCostItemLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtCostItem" runat="server" Text=""></asp:TextBox>
                        <asp:Label ID="lblCostItemText" runat="server" Text=""></asp:Label>
                    </span>

                    <span id="spnVender" runat="server">
                        <asp:Label ID="lblVenderLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtVender" runat="server" Text=""></asp:TextBox>
                        <asp:Label ID="lblVenderText" runat="server" Text=""></asp:Label>
                    </span>
                    
                    <span id="spnBrVender" runat="server">
                        <asp:Label ID="lblBrVenderLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtBrVender" runat="server" Text=""></asp:TextBox>
                        <asp:Label ID="lblBrVenderText" runat="server" Text=""></asp:Label>
                    </span>

                    <span id="spnEstimatedVender" runat="server">
                        <asp:Label ID="lblEstimatedVenderLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtEstimatedVender" runat="server" Text=""></asp:TextBox>
                        <asp:Label ID="lblEstimatedVenderText" runat="server" Text=""></asp:Label>
                    </span>


                    <span id="spnAlocTankInfo"  runat="server">
                        <asp:Label ID="lblAllocateTankCount" runat="server" Text=""></asp:Label>
                        <span><asp:Label ID="lblAllocateTankSelectedCount" runat="server" Text=""></asp:Label>/<asp:Label ID="lblAllocateTankMaxCount" runat="server" Text=""></asp:Label></span>
                    </span>
                    <span id="spnShowTotalInvoiceRelatedCost"  runat="server" visible="false" data-comment="SOAのTOTALINVOICEを表示するかのチェックボックス、一旦常に非表示(当条件項目の表示制御PGは入れていない、ある程度修正しやすいように画面項目として置いている)">
                        <asp:Label ID="lblShowTotalInvoiceRelatedCost" runat="server" Text=""></asp:Label>
                        <span><asp:CheckBox ID="ckhShowTotalInvoiceRelatedCost" runat="server" Checked="false" /></span>
                    </span>
                  　<span id="spnHideNoAmount"  runat="server" visible="false" data-comment="金額０を非表示とする">
                        <asp:Label ID="lblHideNoAmount" runat="server" Text="Hide $0"></asp:Label>
                        <span><asp:CheckBox ID="chkHideNoAmount" runat="server" Checked="true" /></span>
                    </span>
                </div>
                <!-- 追加ボタン -->
                <div id="addCostArea">
                    <input id="btnAddCost" type="button" runat="server" value="ADD COST" />
                </div>
                <!-- タンク動静、引き当て一覧 -->
                <asp:panel id="WF_LISTAREA" runat="server" EnableViewState="false">
                </asp:panel>
                <div id="divHidden">
                    <%-- 必要な隠し要素はこちらに(共通で使用しそうなものは定義済) --%>
                    <asp:HiddenField ID="hdnSubmit" runat="server" Value="" />      <%-- サーバー処理中（TRUE:実行中、FALSE:未実行）--%>
                    <asp:HiddenField ID="hdnButtonClick" runat="server" Value="" /> <%-- ボタン押下(押下したボタンIDを格納) --%>
                    <%-- フィールド変更イベントをサーバー処理させるための定義 --%>
                    <asp:HiddenField ID="hdnOnchangeField" runat="server" Value="" />   <%-- テキスト項目変更値格納用 --%>
                    <asp:HiddenField ID="hdnOnchangeFieldPrevValue" runat="server" Value="" /> <%-- フォーカスが入った瞬間の値を保持 --%>
                    <asp:HiddenField ID="hdnActiveElementAfterOnChange" runat="server" Value="" /> <%-- 変更後イベント直後のフォーカスオブジェクト --%>
                    <%-- 左ボックス用情報 --%>
                    <asp:HiddenField ID="hdnIsLeftBoxOpen" runat="server" Value="" />    <%-- 左ボックスオープン --%>
                    <asp:HiddenField ID="hdnTextDbClickField" runat="server" Value="" /> <%-- ダブルクリックしたフィールド値を格納 --%>
                    <asp:HiddenField ID="hdnLeftboxActiveViewId" runat="server" Value="" /> <%-- 左ボックスのアクティブなビュー --%>
                    <%-- 右ボックス用情報 --%>                    
                    <asp:HiddenField ID="hdnRightboxOpen" runat="server" Value="" /> <%-- Rightbox 開閉 --%>
                    <asp:HiddenField ID="hdnPrintURL" runat="server" />             <%-- Textbox Print URL --%>
                    <%-- フッターヘルプ関連処理で使用 --%>
                    <asp:HiddenField ID="hdnHelpChange" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCanHelpOpen" runat="server" Value="" />
                    <%-- 一覧表制御用 --%>
                    <asp:HiddenField ID="hdnXMLsaveFile" runat="server" Value="" Visible="False" />  <%--  退避した一覧データのファイル保存先 --%>
                    <asp:HiddenField ID="hdnMouseWheel" runat="server" Value="" />   <%--  マウスホイールのUPorDownを記憶 --%>
                    <asp:HiddenField ID="hdnListPosition" runat="server" Value="" /> <%--  縦スクロールポジション --%>
                    <asp:HiddenField ID="hdnListDBclick" runat="server" Value="" />  <%--  ダブルクリックした行番号を記録 --%>   
                    <asp:HiddenField ID="hdnListCurrentRownum" runat="server" Value="" /> <%-- 一覧でボタンクリックイベントを発生させたRowNumを保持 --%>
                    
                    <asp:HiddenField ID="hdnOrgXMLsaveFile" runat="server" Value="" Visible="False" />  <%--  初回ロード時に取得したデータ、「hdnXMLsaveFile」と比較し更新可否制御 --%> 
                    <asp:HiddenField ID="hdnListMapVariant" runat="server" Value="" />  
                    <asp:HiddenField ID="hdnListId" runat="server" Value="" /> <%-- 前画面から引き継がれたListId --%>
                    <%-- 一覧表現在表示しているIDを保持 --%>
<%--                    <asp:HiddenField ID="hdnListFirstKey" runat="server" Value="" />
                    <asp:HiddenField ID="hdnListLastKey" runat="server" Value="" />--%>
                    <%-- 一覧表文言用 --%>
                    <asp:HiddenField ID="hdnListDeleteName" runat="server" Value="DELETE" />
                    <%-- 検索画面（前々画面）の条件保持用フィールド --%>
                    <asp:HiddenField ID="hdnSearchType" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETDStYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETDEndYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETAStYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETAEndYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnShipper" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnConsignee" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnPortOfLoading" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnPortOfDischarge" runat="server" Value="" Visible="False"  />
                    <asp:HiddenField ID="hdnOffice" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnReportVariant" runat="server" Value="" Visible="False" /> <%-- 前々画面の右ボックス指定の前画面オーダー一覧の表示ID --%>
                    <asp:HiddenField ID="hdnBlIssued" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnPort" runat="server" Value="" Visible="False" /> 
                    <asp:HiddenField ID="hdnProduct" runat="server" Value="" Visible="False" /> 
                    <asp:HiddenField ID="hdnCarrier" runat="server" Value="" Visible="False" /> 
                    <asp:HiddenField ID="hdnVsl" runat="server" Value="" Visible="False" /> 
                    <asp:HiddenField ID="hdnDepartureArrival" runat="server" Value="" Visible="False" /> 
                    <%-- 新規作成画面情報引継ぎ --%>
                    <asp:HiddenField ID="hdnOrderNo" runat="server" Value="" Visible="False" />                    
                    <asp:HiddenField ID="hdnIsNewData" runat="server" Value="0" Visible="False" />
                    <asp:HiddenField ID="hdnFillingDate" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnEtd1" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnEta1" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnEtd2" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnEta2" runat="server" Value="" Visible="False" />
                    <%-- ノンブレーカー画面情報引継ぎ --%>
                    <asp:HiddenField ID="hdnDateTermStYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnDateTermEndYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnApproval" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnSettleType" runat="server" Value=""  Visible="False" />
                    <%-- デマレージ検索条件画面情報引継ぎ --%>
                    <asp:HiddenField ID="hdnTankNo" runat="server" Value=""  Visible="False" />
                    <%-- SOA検索条件画面情報引継ぎ --%>
                    <asp:HiddenField ID="hdnInvoicedBy" runat="server" Value=""  Visible="False" />
                    <asp:HiddenField ID="hdnVender" runat="server" Value=""  Visible="False" />
                    <asp:HiddenField ID="hdnAgentSoa" runat="server" Value=""  Visible="False" />
                    <asp:HiddenField ID="hdnCountry" runat="server" Value=""  Visible="False" />
                    <asp:HiddenField ID="hdnReportMonth" runat="server" Value=""  Visible="False" />
                    <asp:HiddenField ID="hdnActualDateStYMD" runat="server" Value=""  Visible="False" />
                    <asp:HiddenField ID="hdnActualDateEndYMD" runat="server" Value=""  Visible="False" />
                    <%-- タンク動静検索画面情報引継ぎ --%>
                    <asp:HiddenField ID="hdnActy" runat="server" Value=""  Visible="False" />
                    <%-- 前画面情報仮置き（本来使わない） --%>
                    <%--<asp:HiddenField ID="hdnBrId" runat="server" Value="" />--%>
                    <asp:HiddenField ID="hdnTrans" runat="server" Value=""  Visible="False" />
                    <asp:HiddenField ID="hdnCopy" runat="server" Value="" />
                    <%-- 当画面フィルタ条件を保持(現在フィルタがかかっている条件) --%>
                    <asp:HiddenField ID="hdnFilterCostItem" runat="server" Value="" />
                    <asp:HiddenField ID="hdnFilterActy" runat="server" Value="" />
                    <%-- タンク一覧引き渡し情報(ダブルクリックしたオーダーNo --%>
                    <asp:HiddenField ID="hdnSelectedOrderId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedTankSeq" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedDataId" runat="server" Value="" /> <%-- ノンブレ時に引き渡し --%>
                    <%-- タンク一覧DELETE=上記選択情報をもとにタンクをクリア,OPEN=タンク一覧をオープン --%>
                    <asp:HiddenField ID="hdnTankProc" runat="server" Value="" />
                    <%-- タンク一覧より選択されたタンクIDを格納 --%>
                    <asp:HiddenField ID="hdnSelectedTankId" runat="server" Value="" />
                    <%-- 備考欄ボックス --%>
                    <asp:HiddenField ID="hdnRemarkboxOpen" value="" runat="server" />
                    <asp:HiddenField ID="hdnRemarkboxField" value="" runat="server" />
                    <asp:HiddenField ID="hdnRemarkboxFieldName" value="" runat="server" />
                    <%-- 一覧表制御用 --%>
                    <asp:HiddenField ID="hdnCurrentUnieuqIndex" value="" runat="server" />
                    <%-- 自身リフレッシュ時のメッセージ(NO)を保持 --%>
                    <asp:HiddenField ID="hdnRefreshMessageNo" runat="server" Value="" Visible="False" />
                    <%-- ドラッグアンドドロップ(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnUploadMessage01" Value="ファイルアップロード開始" runat="server" />
                    <asp:HiddenField ID="hdnUploadError01" Value="ファイルアップロードが失敗しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError02" Value="通信を中止しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError03" Value="タイムアウトエラーが発生しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError04" Value="更新権限がありません。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError05" Value="対応外のファイル形式です。" runat="server" />
                    <%-- アップロード先のURL --%>
                    <asp:HiddenField ID="hdnMAPpermitCode" Value="TRUE" runat="server" />
                    <asp:HiddenField ID="hdnListUpload" Value="" runat="server" />

                    <asp:HiddenField ID="hdnListScrollXPos" Value="" runat="server" />
                    <%= "<input type=""hidden"" id=""hdnFileUpUrl"" value='" & ResolveUrl("~/COH0001FILEUP.ashx") & "' />" %>
                    <%-- ユーザー情報 --%>
                    <asp:HiddenField ID="hdnUserCurrency" Value="" runat="server" Visible="False" />
                    <asp:HiddenField ID="hdnUserOffice" Value="" runat="server" Visible="False" />
                    <asp:HiddenField ID="hdnUserCountry" Value="" runat="server" Visible="False" />
                    <%-- SOAカレンダー制限値保持用 --%>
                    <asp:HiddenField ID="hdnBillingYmd" Value="" runat="server" />
                    <asp:HiddenField ID="hdnCurrentCloseYm" Value="" runat="server" Visible="false" />  
                    <%-- 当画面のみで保持すればいいもの --%>
                    <asp:HiddenField ID="hdnUsdDecimalPlaces" Value="" runat="server" Visible="false" />

                </div>
            </div>
            <%-- 左ボックス --%>
            <div id="divLeftbox">
                <div id="divLeftBoxButtonsBox">
                    <input type="button" id="btnLeftBoxButtonSel" value="　選　択　" runat="server" />
                    <input type="button" id="btnLeftBoxButtonCan" value="キャンセル" runat="server"  />
                </div>
                <%--  　マルチビュー　 --%>
                <asp:MultiView ID="mvLeft" runat="server">
                    <%-- 費用項目 VIEW　 --%>
                    <asp:View ID="vLeftCost" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCost" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 費用 VIEW　 --%>
                    <%-- 費用追加用 VIEW　 --%>
                    <asp:View ID="vLeftAddCost" runat="server">
                        <div class="leftViewContents">
                            <asp:DropDownList ID="ddlNo" runat="server">
                            </asp:DropDownList>
                            <asp:RadioButtonList ID="rblPolPod" runat="server" RepeatColumns="3" RepeatDirection="Horizontal"></asp:RadioButtonList>
                            <asp:ListBox ID="lbAddCost" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 費用 VIEW　 --%>
                    <%-- ノンブレーカー費用追加用 VIEW　 --%>
                    <asp:View ID="vLeftAddNbCost" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbAddNbCost" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END ノンブレーカー費用 VIEW　 --%>
                    <%-- ACTY VIEW　 --%>
                    <asp:View ID="vLeftActy" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbActy" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END ACTY VIEW　 --%>
                    <%-- CURRENCYCODE VIEW　 --%>
                    <asp:View ID="vLeftCurrencyCode" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCurrencyCode" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END CURRENCYCODE VIEW　 --%>
                    <%-- 業者 VIEW　 --%>
                    <asp:View ID="vLeftContractor" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbContractor" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 業者 VIEW　 --%>
                    <%-- ベンダー VIEW(絞り込み条件用)　 --%>
                    <asp:View ID="vLeftVender" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbVender" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END ベンダー VIEW(絞り込み条件用)　 --%>
                    <%--  　計上月種別コード　 --%>
                    <asp:View id="vLeftReportMonth" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbReportMonth" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 計上月種別 VIEW　 --%>
                    <%--  　汎用補助区分 VIEW　 --%>
                    <asp:View id="vLeftAccCurrencySegment" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbAccCurrencySegment" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 汎用補助区分 VIEW　 --%>
                    <%--  　カレンダー　 --%>
                    <asp:View id="vLeftCal" runat="server" >
                        <div class="leftViewContents">
                            <asp:HiddenField ID="hdnCalendarValue" runat="server" />
                            <input id="hdnDateValue" type="hidden" value="" />
                            <table border="0">
                                <tr>
                                    <td>
                                        <table border="1" >
                                            <tr>
                                                <td>
                                                    <div id="carenda">
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td id="altMsg" style="background:white">
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </asp:View> <%-- END カレンダー VIEW　 --%>
                </asp:MultiView>
            </div> <%-- END 左ボックス --%>
            <%-- 右ボックス --%>
            <div id="divRightbox">
                <%-- ****************************
                     右マルチラインテキスト表示エリア
                     **************************** --%>
                <div id="divRightMessageBox">
                    <%-- 殆どの画面は"メモ"、"備考"の入力がないためエラーメッセージのみ
                        当選択項目を非表示及びエラーメッセージ表示を基準とするため
                        こちらはあまり意識する必要なし --%>
                    <div id="divMessageType" runat="server" >
                        <%-- 左テキスト表示内容選択(メモ or エラー詳細) --%>
                        <div id="divMessageChooseArea" runat="server" visible="false">
                            <asp:RadioButton ID="rbShowMemo" runat="server" GroupName="MessageTypeChoose" Text="memo" />
                            <asp:RadioButton ID="rbShowError" runat="server" GroupName="MessageTypeChoose" Text="Error Information" Checked="True" />
                        </div>

                        <div id="divMessageTypeName" runat="server" visible="false">
                            <%-- こちらに編集しているマルチラインテキストの項目名を表示 --%>
                            <asp:Label ID="lblMessageType" runat="server" Text=""></asp:Label>
                        </div>
                        
                        <%-- エラー詳細のみ表示の場合はrbShowErrorの文言のみ表示 --%>
                        <%= IIf(Me.divMessageChooseArea.Visible = False And
                                                                                Me.divMessageTypeName.Visible = False,
                                                                                Me.rbShowError.Text,
                                                                                "") %>
                    </div>
                <%-- ****************************
                     右マルチラインテキストボックス
                     **************************** --%>
                    <div id="divRightMessageTextBox">
                        <asp:MultiView ID="mvRightMessage" runat="server" ActiveViewIndex="1">
                            <asp:View ID="vRightMemo" runat="server">
                                <ul>
                                    <li>
                                        <asp:Label ID="lblRightInfo1" runat="server" Text="ダブルクリックを行い入力を確定してください。"></asp:Label>
                                    </li>
                                </ul>
                                <asp:TextBox ID="txtRightMemo" runat="server" TextMode="MultiLine"></asp:TextBox>
                            </asp:View>
                            <asp:View ID="vRightErrorMessage" runat="server">
                                <asp:TextBox ID="txtRightErrorMessage" text="" runat="server" TextMode="MultiLine" ReadOnly="true"></asp:TextBox>
                            </asp:View>
                            <asp:View ID="vRightRemarks" runat="server">
                                <ul>
                                    <li>
                                        <asp:Label ID="lblRightInfo2" runat="server" Text="ダブルクリックを行い入力を確定してください。"></asp:Label>
                                    </li>
                                </ul>
                                <asp:TextBox ID="txtRightRemarks" runat="server" TextMode="MultiLine"></asp:TextBox>
                            </asp:View>                            
                        </asp:MultiView>
                    </div>
                </div>　<%-- END 右メッセージ表示エリア --%>
                <%-- ****************************
                     右マルチラインリストボックス表示エリア
                    この機能が不要な場合は
                    divRightListBox.visibleをFalseに
                ********************************* --%>
                <div id="divRightListBox" runat="server">
                    <div>
                        <%-- 右リストの説明文 --%>
                        <asp:Label ID="lblRightListDiscription" runat="server" Text=""></asp:Label>
                    </div>
                    <div>
                        <%-- 右リスト本体 --%>
                        <asp:ListBox ID="lbRightList" runat="server">
                        </asp:ListBox>
                    </div>
                </div>
            </div>  <%-- END 右ボックス --%>
            <%--フッターボックス --%>
            <div id="divFooterbox" >
                <div><asp:Label ID="lblFooterMessage" runat="server" Text=""></asp:Label></div>
                <div id="divShowHelp" ></div>
            </div>
            <%-- マルチラインテキスト入力ポップアップ --%>
            <div id="divRemarkInputBoxWrapper" runat="server">
                <div id="divRemarkInputBox">
                    <div id="divRemarkInputitle">
                        <%= Me.hdnRemarkboxFieldName.Value %>
                    </div>
                    <div id="divRemarkInputButtons">
                        <input id="btnRemarkInputOk" type="button" value="OK" runat="server" />
                        <input id="btnRemarkInputCancel" type="button" value="CANCEL" runat="server" />
                    </div>
                    <div id="divRemarkTextArea">
                        <asp:TextBox ID="txtRemarkInput" runat="server" TextMode="MultiLine"></asp:TextBox>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
