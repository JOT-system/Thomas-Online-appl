<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00001BREAKER.aspx.vb" Inherits="OFFICE.GBT00001BREAKER"  %>
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
    <link href="~/GB/css/GBT00001BREAKER.css?rd=20190527" rel="stylesheet" type="text/css" />
    <!-- Global site tag (gtag.js) - Google Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=UA-162522994-1"></script>
    <script>
      window.dataLayer = window.dataLayer || [];
      function gtag(){dataLayer.push(arguments);}
      gtag('js', new Date());

      gtag('config', 'UA-162522994-1');
    </script>
    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8"></script>
    <%-- 左ボックスカレンダー使用の場合のスクリプト --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/script/calendar.js") %>'  charset="utf-8"></script>
    <%--個別のスクリプトは以下に記載 --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00001BREAKER.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加
        // ダウンロード処理
        function f_ExcelPrint() {
            // リンク参照
            var printUrlObj = document.getElementById("hdnPrintURL");
            if (printUrlObj === null) {
                return;
            }
            window.open(printUrlObj.value, "view", "scrollbars=yes,resizable=yes,status=yes");
            printUrlObj.value = '';
        };

        function f_PDFPrint() {
            var objPrintUrl = document.getElementById("hdnPrintURL");
            if (objPrintUrl === null) {
                return;
            }
            // リンク参照
            window.open(objPrintUrl.value, "view", "_blank");
        }

        // ドロップ処理（ドラッグドロップ入力）
        function f_dragEvent(e) {
            e.preventDefault();
            commonDispWait();
            var footerMsg = document.getElementById("lblFooterMessage");
            this.style.cursor = "default";
            if (document.getElementById('hdnMAPpermitCode').value == "TRUE") {
                footerMsg.textContent = '<%= Me.hdnUploadMessage01.Value %>';
                footerMsg.style.color = "blue";
                footerMsg.style.fontWeight = "bold";

                // ドラッグされたファイル情報を取得
                var files = e.dataTransfer.files;

                // 送信用FormData オブジェクトを用意
                var fd = new FormData();

                // ファイル情報を追加する
                
                for (var i = 0; i < files.length; i++) {
                    /* 拡張子xlsxの場合 */
                    var reg = new RegExp("^.*\.xlsx$");
                    if (files[i].name.toLowerCase().match(reg)) {
                        fd.append("files", files[i]);
                    } else {
                        footerMsg.textContent = '<%= Me.hdnUploadError05.Value %>';
                        footerMsg.style.color = "red";
                        footerMsg.style.fontWeight = "bold";
                        commonHideWait();
                        return;
                    }
               }

                // XMLHttpRequest オブジェクトを作成
                var xhr = new XMLHttpRequest();

                // ドロップファイルによりURL変更
                // 「POST メソッド」「接続先 URL」を指定
                xhr.open("POST",'<%= ResolveUrl(OFFICE.CommonConst.C_UPLOAD_HANDLER_URL) %>' , false)

                // イベント設定
                // ⇒XHR 送信正常で実行されるイベント
                xhr.onload = function (e) {
                    if (e.currentTarget.status == 200) {


                        document.getElementById("hdnListUpload").value = "XLS_LOADED";
                        document.forms[0].submit();                             //aspx起動
                    } else {
                        footerMsg.textContent = '<%= Me.hdnUploadError01.Value %>';
                        footerMsg.style.color = "red";
                        footerMsg.style.fontWeight = "bold";
                        commonHideWait();
                    }
                };

                // ⇒XHR 送信ERRで実行されるイベント
                xhr.onerror = function (e) {
                    footerMsg.textContent = '<%= Me.hdnUploadError01.Value %>';
                    footerMsg.style.color = "red";
                    footerMsg.style.fontWeight = "bold";
                    commonHideWait();
                };

                // ⇒XHR 通信中止すると実行されるイベント
                xhr.onabort = function (e) {
                    footerMsg.textContent = '<%= Me.hdnUploadError02.Value %>';
                    footerMsg.style.color = "red";
                    footerMsg.style.fontWeight = "bold";
                    commonHideWait();
                };

                // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
                xhr.ontimeout = function (e) {
                    footerMsg.textContent = '<%= Me.hdnUploadError03.Value %>';
                    footerMsg.style.color = "red";
                    footerMsg.style.fontWeight = "bold";
                    commonHideWait();
                };

                // 「送信データ」を指定、XHR 通信を開始する
                xhr.send(fd);
            } else {
                footerMsg.textContent = '<%= Me.hdnUploadError04.Value %>';
                footerMsg.style.color = "red";
                footerMsg.style.fontWeight = "bold";
                commonHideWait();
            }
                
        }

        // ドロップ処理（処理抑止）
        function f_dragEventCancel(event) {
            event.preventDefault();  //イベントをキャンセル
        };

        // 〇チェックボックス処理
        function inputRequestChk(chkObjId,chkMailObjId) {

            var chkObj = document.getElementById(chkObjId);
            var chkMailObj = document.getElementById(chkMailObjId);
            if (chkObj === null || chkMailObj === null) {
                return;
            }

            if (chkObj.checked === false) {
                chkMailObj.checked = false;
            }
        }

        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            changeCommonInfoArea();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>', '<%= Me.btnOutputExcel.ClientId %>',
                                       '<%= Me.btnPrint.ClientId %>',
                                       '<%= Me.btnSave.ClientId  %>', '<%= Me.btnSelectMailOk.ClientID %>', 
                                       '<%= Me.btnEntryCostSelectMailOk.ClientID  %>',
                                       '<%= Me.btnEntryCostSelectMailYes.ClientID  %>',
                                       '<%= Me.btnEntryCostSelectMailNo.ClientID  %>',
                                       '<%= Me.btnApply.ClientID %>',
                                       '<%= Me.btnReject.ClientId  %>',
                                       '<%= Me.btnApproval.ClientId  %>',
                                       '<%= Me.btnAppReject.ClientId  %>',
                                       '<%= Me.btnRemarkInputOk.ClientId  %>',
                                       '<%= Me.btnRemarkInputCancel.ClientId  %>',
                                       '<%= Me.btnRemarkInputEdit.ClientId  %>',
                                       '<%= Me.btnApplyMsgYes.ClientId  %>',
                                       '<%= Me.btnApplyMsgNo.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* タブクリックイベントのバインド */
            var targetTabObjects = ['<%= Me.tabOrganizer.ClientID %>', 
                                    '<%= Me.tabExport1.ClientID %>', '<%= Me.tabInport1.ClientID %>',
                                    '<%= Me.tabExport2.ClientID %>', '<%= Me.tabInport2.ClientID %>']

            bindTabClickEvent(targetTabObjects);
            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';
            var viewShipper = '<%= Me.vLeftShipper.ClientID %>';
            var viewConsignee = '<%= Me.vLeftConsignee.ClientID %>';
            var viewCarrier = '<%= Me.vLeftCarrier.ClientID %>';
            var viewProduct = '<%= Me.vLeftProduct.ClientID %>';
            var viewCountry = '<%= Me.vLeftCountry.ClientID %>';
            var viewPort = '<%= Me.vLeftPort.ClientID %>';
            var viewTerm = '<%= Me.vLeftTerm.ClientID %>';
            var viewAgent = '<%= Me.vLeftAgent.ClientID %>';
            var viewMSDS = '<%= Me.vLeftMSDS.ClientID %>';
            var viewBilCategory = '<%= Me.vLeftBillingCategory.ClientID %>';
            var dblClickObjects = [['<%= Me.txtBrStYmd.ClientID %>', viewCalId],
                                   ['<%= Me.txtBrEndYmd.ClientID %>', viewCalId],
                                   ['<%= Me.txtAppRequestYmd.ClientID %>', viewCalId],
                                   ['<%= Me.txtApprovedYmd.ClientID %>', viewCalId],
                                   ['<%= Me.txtEtd1.ClientID %>', viewCalId],
                                   ['<%= Me.txtEta1.ClientID %>', viewCalId],
                                   ['<%= Me.txtEtd2.ClientID %>', viewCalId],
                                   ['<%= Me.txtEta2.ClientID %>', viewCalId],
                                   ['<%= Me.txtShipper.ClientID %>', viewShipper],
                                   ['<%= Me.txtConsignee.ClientID %>', viewConsignee],
                                   ['<%= Me.txtCarrier1.ClientID %>', viewCarrier],
                                   ['<%= Me.txtCarrier2.ClientID %>', viewCarrier],
                                   ['<%= Me.txtProduct.ClientID %>',viewProduct],
                                   ['<%= Me.txtRecieptCountry1.ClientID %>',viewCountry],
                                   ['<%= Me.txtDischargeCountry1.ClientID %>',viewCountry],
                                   ['<%= Me.txtLoadCountry1.ClientID %>',viewCountry],
                                   ['<%= Me.txtDeliveryCountry1.ClientID %>',viewCountry],
                                   ['<%= Me.txtRecieptCountry2.ClientID %>',viewCountry],
                                   ['<%= Me.txtDischargeCountry2.ClientID %>',viewCountry],
                                   ['<%= Me.txtLoadCountry2.ClientID %>',viewCountry],
                                   ['<%= Me.txtDeliveryCountry2.ClientID %>',viewCountry],
                                   ['<%= Me.txtRecieptPort1.ClientID %>',viewPort],
                                   ['<%= Me.txtDischargePort1.ClientID %>',viewPort],
                                   ['<%= Me.txtLoadPort1.ClientID %>',viewPort],
                                   ['<%= Me.txtDeliveryPort1.ClientID %>',viewPort],
                                   ['<%= Me.txtRecieptPort2.ClientID %>',viewPort],
                                   ['<%= Me.txtDischargePort2.ClientID %>',viewPort],
                                   ['<%= Me.txtLoadPort2.ClientID %>',viewPort],
                                   ['<%= Me.txtDeliveryPort2.ClientID %>', viewPort],
                                   ['<%= Me.txtBrTerm.ClientID %>',viewTerm],
                                   ['<%= Me.txtAgentPol1.ClientID %>',viewAgent],
                                   ['<%= Me.txtAgentPod1.ClientID %>',viewAgent],
                                   ['<%= Me.txtAgentPol2.ClientID %>',viewAgent],
                                   ['<%= Me.txtAgentPod2.ClientID %>', viewAgent],
                                   ['<%= Me.lblMSDS.ClientID %>', viewMSDS],
                                   ['<%= Me.txtInvoiced.ClientID %>', viewAgent],
                                   ['<%= Me.txtBillingCategory.ClientID %>', viewBilCategory]
            ];
            
            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();
            /* 費用追加ボタンのイベントバインド */
            bindAddCostOnClick();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbShipper.ClientID %>', '3', '1'],
                                           ['<%= Me.lbConsignee.ClientID %>', '3', '1'],
                                           ['<%= Me.lbCarrier.ClientID %>', '3', '1'],
                                           ['<%= Me.lbProduct.ClientID %>', '3', '1'],
                                           ['<%= Me.lbCountry.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPort.ClientID %>', '3', '1'],
                                           ['<%= Me.lbTerm.ClientID %>', '3', '1'],
                                           ['<%= Me.lbAgent.ClientID %>', '3', '1'],
                                           ['<%= Me.lbContractor.ClientID %>', '3', '1'],
                                           ['<%= Me.lbMSDS.ClientID %>', '3', '1'],
                                           ['<%= Me.lbCost.ClientID %>', '3', '1'],
                                           ['<%= Me.lbBillingCategory.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = ['<%= Me.txtBrTerm.ClientID %>', '<%= Me.txtConsignee.ClientID %>',
                                         '<%= Me.txtCarrier1.ClientID %>', '<%= Me.txtCarrier2.ClientID %>',
                                         '<%= Me.txtProduct.ClientID %>',
                                         '<%= Me.txtLocalRateRef.ClientID %>',
                                         '<%= Me.txtAmtRequest.ClientID %>',
                                         '<%= Me.txtAmtPrincipal.ClientID %>',
                                         '<%= Me.txtInvoiced.ClientID %>',
                                         '<%= Me.txtBillingCategory.ClientID %>',
                                         '<%= Me.txtFee.ClientID %>', 
                                         '<%= Me.txtRecieptPort1.ClientID %>','<%= Me.txtRecieptPort2.ClientID %>',
                                         '<%= Me.txtDischargePort1.ClientID %>','<%= Me.txtDischargePort2.ClientID %>'];
            bindTextOnchangeEvent(targetOnchangeObjects);

            /* 右ボックスの開閉ダブルクリックイベントバインド
               右上透明ボックス、下のメッセージ欄、他がある場合は個別で　*/
            bindDiplayRightBoxEvent();
            /* ヘルプボタン表示のダブルクリックイベントバインド */
            bindFooterShowHelpEvent();

            /* ヘルプ表示処理 */
            openHelpPage(); /* hdnCanHelpOpenに"1"が立たない限り開きません。 */

            /* カレンダー描画処理 */
            var calValueObj = document.getElementById('<%= Me.hdnCalendarValue.ClientID %>');
            if (calValueObj !== null) {
                /* 日付格納隠し項目がレンダリングされている場合のみ実行 */
                carenda(0);
                setAltMsg(firstAltYMD, firstAltMsg);
            }

            /* 共通オーナー情報の開閉イベントの紐づけ */
            document.getElementById("spnShowCommonInfo").onclick = (
                function () {
                    commonInfoAreaClick();
                    changeCommonInfoArea();
                }
            );
            /* InputRequestボタンの宛先選択ポップアップ表示イベント関係の紐付け */
            bindInputRequestOnClick();
            /* EntryCostボタンの送信有無選択ポップアップ表示イベント関係の紐付け */
            bindEntryCostOnClick();
            /* Applyボタンの選択ポップアップ表示イベント関係の紐付け */
            bindApplyOnClick();
            /* 費用項目グリッドのイベントバインド */
            bindCostRowEvents();
            /* 左ボックスの備考ダブルクリックイベント */
            bindSpnRightRemarksDbClick();
            /* 備考欄のダブルクリックイベントバインド */
            bindRemarkDblClick();
            ///* 編集ボタンクリックイベントバインド */
            //bindEditOnClick();
            /* 各種計算項目のイベントバインド */
            bindDemurrageDayOnBlur();
            bindTotalDaysOnBlur();
            bindFillingRateCheckOnBlur();
            bindInvoiceTotalOnBlur(); /* 変更時イベント時のみ発火 ロード時には計算しない */
            bindHireageCommercialfactorOnBlur();

            /* ファイルドラッグ＆ドロップのイベントバインド */
            var dragDropObj = document.getElementById('divContainer');
            if (dragDropObj !== null) {
                dragDropObj.addEventListener("dragstart", f_dragEventCancel, false);
                dragDropObj.addEventListener("drag", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragend", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragenter", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragleave", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragover", f_dragEventCancel, false);
                dragDropObj.addEventListener("drop", f_dragEvent, false);
            }
            var scrollTop = document.getElementById("hdnBodyScrollTop");
            if (scrollTop.value !== "") {
                document.getElementById("divContensbox").scrollTop = scrollTop.value;
                scrollTop.value = "";
            }

            /* テキストポップアップ表示設定 */
            setDisplayNameTip();
            var brRemark = document.getElementById('lblBrRemarkText');
            brRemark.removeAttribute('title');
            /* アップロードボタンの設定 */
            addUploadExtention('<%= Me.btnOutputExcel.ClientID %>', 'AFTER', false, 'divContainer');

            /* 画面ロック解除 */
            screenUnlock();
            focusAfterChange();
        });
    </script>
</head>
<%-- 基本的にタグ内でのクライアントサイドのJavaScriptのイベント記述はせず、
    ヘッダーにあるwindow.onloadでイベントバインドをします。
    スタイルなども直接記述は極力行わないように
    ※%付きのコメントはHTMLソース表示でもレンダリングされないものです --%>
<body>
    <%--FormIDは適宜変更ください。 --%>
    <form id="GBT00001B" runat="server">
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

                <div id="commonInfo" runat="server">
                    <table class="infoTable common">
                        <colgroup>
                            <col /><col /><col /><col /><col />
                            <col /><col /><col /><col /><col />
                            <col /><col /><col /><col /><col />
                        </colgroup>
                        <tr id="trBrInfoRow1" runat="server">
                            <td class="headerCell" colspan="2" >
                                <asp:Label ID="lblBrInfoHeader" runat="server" Text="BR-Info"></asp:Label>
                            </td>
                            <td>
                                NO:
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblBrNo" runat="server" Text="XXXXXXXXXX"></asp:Label>
                            </td>
                            <td  class="textRightCell">
                                <asp:Label ID="lblDisabled" runat="server" Text="Disabled"></asp:Label>
                            </td>
                            <td colspan="8">
                                <asp:CheckBox ID="chkDisabled" runat="server" />
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow2" runat="server">
                            <td>
                              &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblBrType" runat="server" Text="種類"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtBrType" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblBrTypeText" runat="server" Text=""></asp:Label>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblBrStYmd" runat="server" Text="有効期限" CssClass="requiredMark2"></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtBrStYmd" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                ～
                                  <asp:TextBox ID="txtBrEndYmd" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblBrRemark" runat="server" Text="BR注記" Font-Underline="true"></asp:Label>
                            </td>
                            <%--<td colspan="5">--%>
                            <td>
                                <span id="spnBrRemark" <%= If(Me.lblBrRemarkText.Enabled, "", "class=""aspNetDisabled""") %>>
                                <%= If(Me.lblBrRemarkText.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                <asp:label ID="lblBrRemarkText" runat="server" Text=""></asp:label>
                                </span>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblCopied" runat="server" Text=""></asp:Label>
                                <%= IIf(Me.hdnOriginalCopyBrid.Value = "", "<span>(New)</span>", "<span>(Copy)</span>") %>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblCopiedFrom" runat="server" Text=""></asp:Label>
                                <%= IIf(Me.hdnOriginalCopyBrid.Value = "", "", Me.hdnOriginalCopyBrid.Value) %>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow3" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblBrTerm" runat="server" Text="輸送形態" CssClass="requiredMark2"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtBrTerm" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblBrTermText" runat="server" Text=""></asp:Label>
                            </td>
                            <td class="textRightCell">
                                <%--<asp:Label ID="lblNoOfTanks" runat="server" Text="タンク本数" CssClass="requiredMark2"></asp:Label>--%>
                                <asp:Label  ID="lblNoOfTanks" runat="server" Text="タンク本数" ></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtNoOfTanks" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblInvoiced" runat="server" Text="船荷証券発行者" Font-Underline="true" CssClass="requiredMark2"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtInvoiced" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblInvoicedText" runat="server" Text=""></asp:Label>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblBillingCategory" runat="server" Text="請求先" Font-Underline="true" CssClass="requiredMark2"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtBillingCategory" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblBillingCategoryText" runat="server" Text=""></asp:Label>
                            </td>
                            <td >
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow5" runat="server">
                            <td colspan="3">
                                &nbsp;
                            </td>
                            <td>
                                <asp:Label ID="lblApploveDate" runat="server" Text="DATE"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblAgent" runat="server" Text="AGENT"></asp:Label>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblPic" runat="server" Text="PIC"></asp:Label>
                            </td>
                            <td colspan="7">
                                <asp:Label ID="lblAppRemarks" runat="server" Text="REMARKS" Font-Underline="true"></asp:Label>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow6" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblApproval" runat="server" Text="Approval"></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtAppRequestYmd" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:TextBox ID="txtAppOffice" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:TextBox ID="txtAppSalesPic" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblAppSalesPicText" runat="server" Text=""></asp:Label>
                            </td>
                            <td colspan="7">
                                <span id="spnApplyRemarks" <%= If(Me.lblApplyRemarks.Enabled, "", "class=""aspNetDisabled""") %>>
                                    <%= If(Me.lblApplyRemarks.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                    <asp:Label ID="lblApplyRemarks" runat="server" Text=""></asp:Label>
                                </span>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow7" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblApproved" runat="server" Text="Approved"></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtApprovedYmd" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                -
                            </td>
                            <td >
                                <asp:TextBox ID="txtAppJotPic" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblAppJotPicText" runat="server" Text="" ></asp:Label>
                            </td>
                            <td colspan="7">
                                <span id="spnAppJotRemarks" <%= If(Me.lblAppJotRemarks.Enabled, "", "class=""aspNetDisabled""") %>>
                                    <%= If(Me.lblAppJotRemarks.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                    <asp:Label ID="lblAppJotRemarks" runat="server" Text="" Enabled="False"></asp:Label>
                                </span>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trShipperInfoRow1" runat="server">
                            <td colspan="5" class="headerCell" >
                                <asp:Label ID="lblShipperConsigneeinfoHeader" runat="server" Text="Shipper/Consignee/Carrier-Info"></asp:Label>
                            </td>
                            <td colspan="2"  >
                                <asp:Label ID="lblConsignee" runat="server" Text="荷受人"></asp:Label>
                            </td>
                            <td colspan="2"  >
                                <asp:Label ID="lblCarrier1" runat="server" Text="船会社1"></asp:Label>
                            </td>
                            <td colspan="5"  >
                                <asp:Label ID="lblCarrier2" runat="server" Text="船会社2"></asp:Label>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trShipperInfoRow2" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblShipper" runat="server" Text="荷主" CssClass="requiredMark2"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtShipper" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblShipperText" runat="server" Text=""></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtConsignee" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblConsigneeText" runat="server" Text=""></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtCarrier1" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblCarrier1Text" runat="server" Text=""></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtCarrier2" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td colspan="4">
                                <asp:Label ID="lblCarrier2Text" runat="server" Text="-"></asp:Label>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <!-- Product/Tank-Info -->
                        <tr id="tr2" runat="server">
                            <td class="headerCell" colspan="3">
                                <asp:Label ID="lblProductTankInfoHeader" runat="server" Text="Product/Tank-Info"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblMSDS" runat="server" Text="[MSDS]"  Font-Underline="True"></asp:Label>
                            </td>
                            <td colspan="10"></td>
                            <td></td>
                        </tr>
                        <tr id="tr3" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblProduct" runat="server" Text="積載品" CssClass="requiredMark2"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtProduct" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td >
                                <asp:Label ID="lblProductText" runat="server" Text=""></asp:Label>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblImdg" runat="server" Text="危険品等級"></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtImdg" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblUNNo" runat="server" Text="国連番号"></asp:Label>
                            </td>
                            <td colspan="6">
                                <asp:TextBox ID="txtUNNo" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>

                    </table>
                </div>
                <!-- タブ表示エリア -->
                <div id="tabsBox">
                    <div class="tabLeftSide">
                        <div id="tabOrganizer" runat="server">Organizer-Info</div>
                        <div id="tabExport1" runat="server" >Export1-Info</div>
                        <div id="tabInport1" runat="server">Import1-Info</div>
                        <div id="tabExport2" runat="server">Export2-Info</div>
                        <div id="tabInport2" runat="server">Import2-Info</div>
                    </div>
                    <div class="tabRightSide">
                        <span id="spnShowCommonInfo" >Show CommonInfo</span>
                        <!-- 共通情報を表示するか(0=表示しない,1(Default)=表示する) -->
                        <asp:HiddenField ID="hdnIsShowCommonInfo" runat="server" Value="1" />
                    </div>
                </div>
                <div id="actionButtonsBox" runat="server">
                    <input id="btnApproval" type="button" value="承認"  runat="server" visible="false"/>
                    <input id="btnAppReject" type="button" value="否認" runat="server" visible="false"  />
                    <input id="btnReject" type="button" value="編集" runat="server" visible="false"  />

                    <input id="btnOutputExcel" type="button" value="エクセル出力" runat="server" />
                    <input id="btnSave" type="button" value="保存" runat="server" />
                    <input id="btnApply" type="button" value="申請"  runat="server" />
                    <input id="btnInputRequest" type="button" value="登録"  runat="server" />
                    <input id="btnEntryCost" type="button" value="費用登録" runat="server" />

                    <input id="btnPrint" type="button" value="Print" runat="server" />
                    <input id="btnBack" type="button" value="戻る"  runat="server"  />
                </div>
                <table class="infoTable main">
                    <colgroup>
                        <col /><col /><col /><col /><col />
                        <col /><col /><col /><col /><col />
                        <col /><col /><col /><col /><col />
                    </colgroup>

                    <tr id="trPortInfoRow1" runat="server">
                        <td class="headerCell"  colspan="3">
                            <asp:Label ID="lblPortPlaceInfoHeader" runat="server" Text="Port/Place-Info"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblCountry1" runat="server" Text="COUNTRY"></asp:Label>
                        </td>
                        <td colspan="3">
                            <asp:Label ID="lblPort1" runat="server" Text="PORT" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblCountry2" runat="server" Text="COUNTRY"></asp:Label>
                        </td>
                        <td colspan="2">
                            <asp:Label ID="lblPort2" runat="server" Text="PORT"></asp:Label>
                        </td>
                        <td colspan="2">
                            <asp:Label ID="lblAgentHeader" runat="server" Text="AGENT" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td colspan="2">
                            <asp:Label ID="lblRemark" runat="server" Text="SPECIAL INSTRUCTIONS" Font-Underline="true" ></asp:Label>
                        </td>
                        <td></td>
                    </tr>
                    <tr id="trPortInfoRow2" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblExport1Row" runat="server" Text="輸出1(Export)"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtRecieptCountry1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:TextBox ID="txtRecieptPort1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td colspan="2">
                            <asp:Label ID="lblRecieptPort1Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtLoadCountry1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:TextBox ID="txtLoadPort1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:Label ID="lblLoadPort1Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtAgentPol1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:Label ID="lblAgentPol1Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td colspan="2">
                            <span id="spnRemarks" <%= If(Me.lblRemarks.Enabled, "", "class=""aspNetDisabled""") %>>
                                <%= If(Me.lblRemarks.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                <asp:Label ID="lblRemarks" runat="server" Text=""></asp:Label>
                            </span>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr id="trPortInfoRow3" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblInport1Row" runat="server" Text="輸入1(Import)"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtDischargeCountry1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:TextBox ID="txtDischargePort1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td colspan="2">
                            <asp:Label ID="lblDischargePort1Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtDeliveryCountry1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:TextBox ID="txtDeliveryPort1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:Label ID="lblDeliveryPort1Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtAgentPod1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td colspan="3">
                            <asp:Label ID="lblAgentPod1Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trPortInfoRow4" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblExport2Row" runat="server" Text="輸出2(Export)"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtRecieptCountry2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:TextBox ID="txtRecieptPort2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td colspan="2">
                            <asp:Label ID="lblRecieptPort2Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtLoadCountry2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:TextBox ID="txtLoadPort2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:Label ID="lblLoadPort2Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtAgentPol2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:Label ID="lblAgentPol2Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td colspan="3">
                            <span id="spnRemarks2" <%= If(Me.lblRemarks2.Enabled, "", "class=""aspNetDisabled""") %>>
                                <%= If(Me.lblRemarks2.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                <asp:Label ID="lblRemarks2" runat="server" Text=""></asp:Label>
                            </span>
                        </td>
                    </tr>
                    <tr id="trPortInfoRow5" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblInport2Row" runat="server" Text="輸入2(Import)"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtDischargeCountry2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:TextBox ID="txtDischargePort2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td colspan="2">
                            <asp:Label ID="lblDischargePort2Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtDeliveryCountry2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td >
                            <asp:TextBox ID="txtDeliveryPort2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            <asp:Label ID="lblDeliveryPort2Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtAgentPod2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td colspan="3">
                            <asp:Label ID="lblAgentPod2Text" runat="server" Text=""></asp:Label>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trCarrierSubInfoRow1" runat="server">
                        <td class="headerCell" colspan="14">
                            <asp:Label ID="lblCarrierInfoHeader" runat="server" Text="Carrier-SubInfo"></asp:Label>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trCarrierSubInfoRow2" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblVsl1" runat="server" Text="船名1"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtVsl1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblVoy1" runat="server" Text="航海番号1"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtVoy1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblEtd1" runat="server" Text="出発日1"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtEtd1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblEta1" runat="server" Text="到着日1"></asp:Label>
                        </td>
                        <td colspan="5">
                            <asp:TextBox ID="txtEta1" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trCarrierSubInfoRow3" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblVsl2" runat="server" Text="船名2"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtVsl2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblVoy2" runat="server" Text="航海番号2"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtVoy2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblEtd2" runat="server" Text="出発日2"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtEtd2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblEta2" runat="server" Text="到着日2"></asp:Label>
                        </td>
                        <td colspan="5">
                            <asp:TextBox ID="txtEta2" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>

                    <!-- Product/Tank-Info -->
                    <tr id="trProductTankSubInfoRow1" runat="server">
                        <td class="headerCell" colspan="14">
                            <asp:Label ID="lblProductTankSubinfoHeader" runat="server" Text="Product/Tank-SubInfo"></asp:Label>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trProductTankSubInfoRow2" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblWeight" runat="server" Text="積載重量" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtWeight" runat="server" Text="" CssClass="textRight" ></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblSGravity" runat="server" Text="比重"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtSGravity" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblTankCapacity" runat="server" Text="タンク容量"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtTankCapacity" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblTankFillingRate" runat="server" Text="タンク積載％"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtTankFillingRate" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblTankFillingCheck" runat="server" Text="ﾁｪｯｸ結果"></asp:Label>
                        </td>
                        <td colspan="3">
                            <asp:TextBox ID="txtTankFillingCheck" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <!-- ここから Hireage-Info -->
                    <tr id="trHireageInfoRow1" runat="server">
                        <td class="headerCell" colspan="14">
                            <asp:Label ID="lblHireageInfoHeader" runat="server" Text="Hireage-Info"></asp:Label>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trHireageInfoRow2" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblTotal" runat="server" Text="期間合計"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtTotal" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblLoading" runat="server" Text="発側期間" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtLoading" runat="server" Text="0" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblSteaming" runat="server" Text="船上期間" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtSteaming" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblTip" runat="server" Text="着側期間" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td >
                             <asp:TextBox ID="txtTip" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblExtra" runat="server" Text="追加期間" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td colspan="3">
                             <asp:TextBox ID="txtExtra" runat="server" Text="" CssClass="textRight" ></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trHireageInfoRow3" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblTotalCost" runat="server" Text="TOTAL COST"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtTotalCost" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblJOTHireage" runat="server" Text="JOT総額" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtJOTHireage" runat="server" Text="" CssClass="textRight" ></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblCommercialFactor" runat="server" Text="調整" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtCommercialFactor" runat="server" Text="" CssClass="textRight" ></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblInvoicedTotal" runat="server" Text="総額" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtInvoicedTotal" runat="server" Text="" CssClass="textRight" ></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblPerDay" runat="server" Text="PerDay"></asp:Label>
                        </td>
                        <td colspan="3">
                             <asp:TextBox ID="txtPerDay" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trHireageInfoRow4" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblAmount" runat="server" Text="総額変更"></asp:Label>
                        </td>
                        <td >
                            <asp:Label ID="lblAmtRequest" runat="server" Text="要求"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtAmtRequest" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblAmtPrincipal" runat="server" Text="確認"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtAmtPrincipal" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblAmtDiscount" runat="server" Text="差額"></asp:Label>
                        </td>
                        <td colspan="7">
                            <asp:TextBox ID="txtAmtDiscount" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <!-- ここから Hireage-Info(JPY) -->
                    <tr id="trHireageJPYInfoRow1" runat="server">
                        <td class="headerCell" colspan="14">
                            <asp:Label ID="lblHireageJPYInfoHeader" runat="server" Text="Hireage-Info"></asp:Label>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trHireageJPYInfoRow2" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblTotalCostJPY" runat="server" Text="TOTAL COST"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtTotalCostJPY" runat="server" Text="" CssClass="textRight" Enabled="false"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblJOTHireageJPY" runat="server" Text="JOT総額" ></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtJOTHireageJPY" runat="server" Text="" CssClass="textRight" Enabled="false" ></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblCommercialFactorJPY" runat="server" Text="調整"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtCommercialFactorJPY" runat="server" Text="" CssClass="textRight" Enabled="false" ></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblInvoicedTotalJPY" runat="server" Text="総額"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtInvoicedTotalJPY" runat="server" Text="" CssClass="textRight" Enabled="false" ></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblPerDayJPY" runat="server" Text="PerDay"></asp:Label>
                        </td>
                        <td colspan="3">
                             <asp:TextBox ID="txtPerDayJPY" runat="server" Text="" CssClass="textRight" Enabled="false"></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trHireageJPYInfoRow3" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblAmountJPY" runat="server" Text="総額変更"></asp:Label>
                        </td>
                        <td >
                            <asp:Label ID="lblAmtRequestJPY" runat="server" Text="要求"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtAmtRequestJPY" runat="server" Text="" CssClass="textRight" Enabled="false"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblAmtPrincipalJPY" runat="server" Text="確認"></asp:Label>
                        </td>
                        <td >
                            <asp:TextBox ID="txtAmtPrincipalJPY" runat="server" Text="" CssClass="textRight" Enabled="false"></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblAmtDiscountJPY" runat="server" Text="差額"></asp:Label>
                        </td>
                        <td colspan="7">
                            <asp:TextBox ID="txtAmtDiscountJPY" runat="server" Text="" CssClass="textRight" Enabled="false"></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <!-- ここから Cost-Info -->
                    <tr id="trCostInfoRow1" runat="server">
                        <td class="headerCell" colspan="14">
                            <asp:Label ID="lblCostInfoHeader" runat="server" Text="Cost-Info"></asp:Label>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr id="trCostInfoRow2" runat="server">
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblFee" runat="server" Text="手数料"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtFee" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td colspan="10">
                            &nbsp;
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                </table>
                <div id="divDemurrage" runat="server">
                <table class="infoTable demurrage" >
                    <colgroup>
                        <col /><col /><col /><col /><col />
                        <col /><col /><col /><col />
                    </colgroup>
                    <tbody>

                    <tr>
                        <td class="headerCell" colspan="2">
                            <asp:Label ID="lblDemurrageInfoHeader" runat="server" Text="Demurrage-Info"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblDemurrageDateFrom" runat="server" Text="From"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblDemurrageDateTo" runat="server" Text="To" CssClass="requiredMark2"></asp:Label>
                        </td>

                        <td colspan="2">
                            <asp:Label ID="lblDemurrageRate" runat="server" Text="US$/DAY" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblDemurrageThereafterDate" runat="server" Text="Date"></asp:Label>
                        </td>
                        <td>
                            <asp:Label ID="lblDemurrageThereafterRate" runat="server" Text="US$/DAY" CssClass="requiredMark2"></asp:Label>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td>
                            &nbsp;
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblDemurday1" runat="server" Text="一次期間"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtDemurdayF1" runat="server" Text=""></asp:TextBox>
                            ~
                        </td>
                        <td>
                            <asp:TextBox ID="txtDemurdayT1" runat="server" Text="" ></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtDemurUSRate1" runat="server" Text="" CssClass="textRight" ></asp:TextBox>
                        </td>
                        <td class="textRightCell">
                            <asp:Label ID="lblDemurday2" runat="server" Text="二次期間"></asp:Label>
                        </td>
                        <td>
                            <asp:TextBox ID="txtDemurday2" runat="server" Text="" ></asp:TextBox>
                        </td>
                        <td>
                            <asp:TextBox ID="txtDemurUSRate2" runat="server" Text="" CssClass="textRight" ></asp:TextBox>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    </tbody>
                </table>
                </div>
                <div id="divBrDetailInfo" runat="server">
                <table class="infoTable brDetail" >
                    <colgroup>
                        <col /><col /><col />
                        <col /><col /><col />
                        <col /><col /><col />
                        <col /><col />
                    </colgroup>
                    <tr>
                        <td class="headerCell" colspan="2">
                            <asp:Label ID="lblDetailInfoHeadedr" runat="server" Text="BRdetail-Info"></asp:Label>
                        </td>
                        <td>
                            <input id="btnAddCost" type="button" value="追加"  runat="server"/>
                        </td>
                        <td>
                            <asp:Label ID="lblAgencySummary" runat="server" Text="各代理店合計"></asp:Label>
                        </td>
                        <%--<td>
                            <input id="iptAgencySummaryLocal" type="text" disabled="true" class="aspNetDisabled textRight" runat="server" />
                        </td>--%>
                        <td colspan="2">
                            <input id="iptAgencySummaryUsd" type="text" disabled="true" class="aspNetDisabled textRight" runat="server" />
                        </td>
                        <td>
                            <asp:Label ID="lblLocalRateRef" runat="server" Text="Loc.Cur Rate"></asp:Label>
                        </td>
                        <td colspan="2">
                            <asp:TextBox ID="txtLocalRateRef" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                            <%--<input id="iptLocalRate" type="text" class="aspNetDisabled textRight" runat="server" />--%>
                        </td>
                        <td>
                            <%--<asp:Label ID="lblUSDRateRef" runat="server" Text="TTM Rate"></asp:Label>--%>
                        </td>
                        <%--<td>--%>
                            <%--<asp:TextBox ID="txtUSDRateRef" runat="server" Text="" CssClass="textRight"></asp:TextBox>--%>
                            <%--<input id="iptUSDRate" type="text" class="aspNetDisabled textRight" runat="server" />--%>
                        <%--</td>--%>
                    </tr>
                    <tr>
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="11">
                            <asp:GridView ID="gvDetailInfo" runat="server" AutoGenerateColumns="False" ShowFooter="False" GridLines="None"  CellSpacing="-1" >
                                <Columns >
                                    <asp:TemplateField HeaderText="action" ItemStyle-CssClass="ActionCell" HeaderStyle-CssClass="ActionCell">
                                        <ItemTemplate>
                                            <asp:Panel ID="pnlDeleteButtonArea" runat="server" Visible='<%# IIf(Eval("IsAddedCost") = "1", "True", "False") %>'>
                                                <input id='btnDeleteCostItem_<%# Container.DataItemIndex %>' type="button" value='<%= Me.hdnDispDeleteBtnText.Value %>' data-uniqueindex='<%#Eval("UniqueIndex") %>' <%= If(Me.gvDetailInfo.Enabled, "", "class=""aspNetDisabled"" disabled=""disabled""") %> />
                                            </asp:Panel>
                                            <asp:HiddenField ID="hdnCostCode" runat="server"  Value='<%# Bind("CostCode") %>' />
                                            <asp:HiddenField ID="hdnCostName" runat="server"  Value='<%# Bind("CostName") %>' />
                                            <asp:HiddenField ID="hdnRemarks" runat="server"  Value='<%# Bind("Remarks") %>' />
                                            <asp:HiddenField ID="hdnChargeClass4" runat="server"  Value='<%# Bind("ChargeClass4") %>' />
                                            <asp:HiddenField ID="hdnChargeClass8" runat="server"  Value='<%# Bind("ChargeClass8") %>' />
                                            <asp:HiddenField ID="hdnSortOrder" runat="server"  Value='<%# Bind("SortOrder") %>' />
                                            <asp:HiddenField ID="hdnIsAddedCost" runat="server"  Value='<%# Bind("IsAddedCost") %>' />
                                            <asp:HiddenField ID="hdnItemGroup" runat="server"  Value='<%# Bind("ItemGroup") %>' />
                                            <asp:HiddenField ID="hdnUniqueIndex" runat="server"  Value='<%# Bind("UniqueIndex") %>' />
                                            <asp:HiddenField ID="hdnActionId" runat="server"  Value='<%# Bind("ActionId") %>' />
                                            <asp:HiddenField ID="hdnClass1" runat="server"  Value='<%# Bind("Class1") %>' />
                                            <asp:HiddenField ID="hdnClass2" runat="server"  Value='<%# Bind("Class2") %>' />
                                            <asp:HiddenField ID="hdnClass3" runat="server"  Value='<%# Bind("Class3") %>' />
                                            <asp:HiddenField ID="hdnClass4" runat="server"  Value='<%# Bind("Class4") %>' />
                                            <asp:HiddenField ID="hdnClass5" runat="server"  Value='<%# Bind("Class5") %>' />
                                            <asp:HiddenField ID="hdnClass6" runat="server"  Value='<%# Bind("Class6") %>' />
                                            <asp:HiddenField ID="hdnClass7" runat="server"  Value='<%# Bind("Class7") %>' />
                                            <asp:HiddenField ID="hdnClass8" runat="server"  Value='<%# Bind("Class8") %>' />
                                            <asp:HiddenField ID="hdnCountryCode" runat="server"  Value='<%# Bind("CountryCode") %>' />                                            
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField HeaderText='' DataField="CostCode" ItemStyle-CssClass="CostCodeCell" HeaderStyle-CssClass="CostCodeCell" />
                                    <asp:BoundField HeaderText="" DataField="CostName" ItemStyle-CssClass="CostNameCell" HeaderStyle-CssClass="CostNameCell" />
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="BlCell" HeaderStyle-CssClass="BlCell">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkBl" runat="server" text='<%# Eval("Class9") %>'></asp:CheckBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="JOTCell" HeaderStyle-CssClass="JOTCell">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkJOT" runat="server" text='<%# Eval("InvoicedBy") %>'  Enabled='<%# if(Eval("CountryCode").Equals("JP"), True, False) %>'></asp:CheckBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="SCCell" HeaderStyle-CssClass="SCCell">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkSC" runat="server" text='<%# Eval("Billing") %>'></asp:CheckBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="BaseOnCell" HeaderStyle-CssClass="BaseOnCell">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtBaseOn" runat="server" text='<%# Bind("BasedOn") %>' CssClass="textRight"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField> 
<%--                                    <asp:TemplateField HeaderText="">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtTax" runat="server" text='<%# Bind("Tax") %>' CssClass="textRight"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField> --%>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="LocalCell" HeaderStyle-CssClass="LocalCell">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtLocal" runat="server" text='<%# Bind("Local") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="TaxationCell" HeaderStyle-CssClass="TaxationCell" >
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkTaxation" runat="server" text='<%# Eval("Taxation") %>' Enabled='<%# if(Eval("CountryCode").Equals("JP"), True, False) %>'></asp:CheckBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="USDCell" HeaderStyle-CssClass="USDCell">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtUsd" runat="server" text='<%# Bind("USD") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>                                     
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="ContractorCell" HeaderStyle-CssClass="ContractorCell">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtContractor" runat="server" text='<%# Bind("ConstractorCode") %>' data-uniqueindex='<%#Eval("UniqueIndex") %>'></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="ContractorTextCell" HeaderStyle-CssClass="ContractorTextCell">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtContractorText" runat="server" text='<%# Bind("Constractor") %>' Enabled="false"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="LocalRateCell" HeaderStyle-CssClass="LocalRateCell">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtLocalRate" runat="server" text='<%# Bind("LocalCurrncyRate") %>' Enabled="false" CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>'></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
<%--                                    <asp:TemplateField HeaderText="">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtUSDRate" runat="server" text='<%# Bind("USDRate") %>' Enabled="false" CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>'></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>--%>
                                    <asp:TemplateField HeaderText="" ItemStyle-CssClass="RemarksCell" HeaderStyle-CssClass="RemarksCell">
                                        <ItemTemplate>
                                            <span id='spnCostRemarks_<%# Container.DataItemIndex %>'  data-uniqueindex='<%#Eval("UniqueIndex") %>'>
                                                <%# IF(Eval("Remarks") = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                                <asp:Label ID="lblCostRemarks" runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("Remarks")) %>'></asp:Label>
                                            </span>
                                            
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </td>
                        
                    </tr>

                </table>
                </div>
                <div class="emptybox"></div>

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
                    <%-- 画面固有 --%>
                    <asp:HiddenField ID="hdnDispDeleteBtnText" value="Delete" runat="server" />
                    <asp:HiddenField ID="hdnDispLeftBoxCostCode" value="コード" runat="server" />
                    <asp:HiddenField ID="hdnDispLeftBoxCostName" value="費用名用" runat="server" />
                    <asp:HiddenField ID="hdnIsTrilateral" runat="server" Visible="false" /> <%-- 三国間か？ "1"=三国間 それ以外=二国 --%>

                    <asp:HiddenField ID="hdnSelectedTabId" runat="server" Value="" /> <%-- 選択中のタブ --%>
                    <asp:HiddenField ID="hdnIsViewOnlyPopup" runat="server" Value="0" Visible="false" /> <%-- 参照のみのポップアップ表示か？ "1":ポップアップ表示,"0":それ以外 --%>
                    <asp:HiddenField ID="hdnIsViewFromApprove" runat="server" Value="0" Visible="false" /> <%-- 承認画面からの遷移か "1":承認画面遷移,"0":それ以外 --%>

                    <asp:HiddenField ID="hdnProductIsHazard" runat="server" Value="" /> <%-- 積載品は危険物か？ "1"=危険物 それ以外=非危険品 --%>
                    <asp:HiddenField ID="hdnPrpvisions" runat="server" Value="" /> <%-- 追加規定 --%>
                    <asp:HiddenField ID="hdnCanCalcHireageCommercialFactor" runat="server" Value="" Visible ="false" /> <%-- 売上総額よりJOT総額算出の自動計算を行うか1=行う それいがい=行わない、費用変更時→オーナータブ移動時に使用 --%>
                    <asp:HiddenField ID="hdnPrevTotalInvoicedValue" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnBackUrl" value="" runat="server" Visible="false" />
                    <asp:HiddenField ID="hdnBodyScrollTop" value="" runat="server" />
                    <asp:HiddenField ID="hdnCallerMapId" value="" runat="server" Visible ="False" />
                    <%-- 費用用 --%>
                    <asp:HiddenField ID="hdnDelteCostUniqueIndex" value="" runat="server" />
                    <asp:HiddenField ID="hdnCurrentUnieuqIndex" value="" runat="server" />
                    <%-- 備考欄ボックス --%>
                    <asp:HiddenField ID="hdnRemarkboxOpen" value="" runat="server" />
                    <asp:HiddenField ID="hdnRemarkboxField" value="" runat="server" />
                    <asp:HiddenField ID="hdnRemarkboxFieldName" value="" runat="server" />
                    <%-- RemarkEmptyMessage --%>
                    <asp:HiddenField ID="hdnRemarkEmptyMessage" value="" runat="server" />
                    <asp:HiddenField ID="hdnRightBoxRemarkField" value="" runat="server" />
                    <asp:HiddenField ID="hdnRightBoxClose" value="" runat="server" />
                    <%-- ドラッグアンドドロップ --%>
                    <asp:HiddenField ID="hdnMAPpermitCode" Value="TRUE" runat="server" />
                    <asp:HiddenField ID="hdnListUpload" Value="" runat="server" />
                    <%-- ドラッグアンドドロップ(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnUploadMessage01" Value="ファイルアップロード開始" runat="server" />
                    <asp:HiddenField ID="hdnUploadError01" Value="ファイルアップロードが失敗しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError02" Value="通信を中止しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError03" Value="タイムアウトエラーが発生しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError04" Value="更新権限がありません。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError05" Value="対応外のファイル形式です。" runat="server" />
                    <%-- 当画面の計算処理POST(設定した名称の関数を実行) --%>
                    <asp:HiddenField ID="hdnCalcFunctionName" Value="" runat="server" />
                    <%-- 前画面(検索画面)検索条件保持用 --%>
                    <asp:HiddenField ID="hdnStYMD" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnEndYMD" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnShipper" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnConsignee" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnPort" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnApproval" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnOffice" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnCorrection" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnExtract" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnDenial" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnStep" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnMsgId" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnBrType" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnSearchBreakerType" runat="server" Value="" />
                    <asp:HiddenField ID="hdnPol1Status" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnPol2Status" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnPod1Status" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnPod2Status" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnPOLPort" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnPODPort" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnProduct" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnBrId" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnStatus" runat="server" Value="" visible="false"/>  <%-- オーガナイザの申請ステータス(直近ステータス(例外として：承認遷移→Editボタン押下時は03(EDIT)となる)) --%>
                    <%-- 前画面XMLファイル保持用 --%>
                    <asp:HiddenField ID="hdnXMLsaveFileRet" runat="server" Value="" visible="false" />
                    <%-- 前画面(承認画面)保持用 --%>
                    <asp:HiddenField ID="hdnPrevViewID" runat="server" Value="" Visible="false" />
                    <%-- 承認制御用 --%>
                    <asp:HiddenField ID="hdnReject" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnAppJotRemarks" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnCostSelectedTabId" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnNewBreaker" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnRemarkFlg" runat="server" Value="" visible="false"/>
                    <asp:HiddenField ID="hdnRemarkInitFlg" runat="server" Value="" visible="false" />
                    <asp:HiddenField ID="hdnApply" runat="server" Value="" visible="false" />
                    <asp:HiddenField ID="hdnEntryCost" runat="server" Value="" visible="false" />
                    <asp:HiddenField ID="hdnDisableAll" runat="server" Value="" visible="false" />
                    <asp:HiddenField ID="hdnInputReq" runat="server" Value="" visible="false" />
                    <asp:HiddenField ID="hdnCountryControl" runat="server" Value="" visible="false" />                    
                    <asp:HiddenField ID="hdnEnableControl" runat="server" Value="" visible="false" />                    
                    <%-- オーガナイザー情報保持 --%>
                    <asp:HiddenField ID="hdnCountryOrganizer" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnAgentOrganizer" runat="server" Value="" Visible="false" />
                    <%-- 終了(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnMsgboxFieldName" Value="" runat="server" />
                    <asp:HiddenField ID="hdnExitMsgboxFieldName" Value="" runat="server" />
                    <asp:HiddenField ID="hdnEntryCostFieldName" Value="" runat="server" />
                    <asp:HiddenField ID="hdnMsgboxShowFlg" Value="0" runat="server" />
                    <asp:HiddenField ID="hdnMsgboxChangeFlg" Value="" runat="server" />
                    <asp:HiddenField ID="hdnMsgboxAppChangeFlg" Value="" runat="server" />
                    <%-- 登録情報 --%>
                    <asp:HiddenField ID="hdnInitYmd" Value="" runat="server" Visible="false" />
                    <asp:HiddenField ID="hdnInitUser" Value="" runat="server" Visible="false" />
                    <asp:HiddenField ID="hdnInitUserName" Value="" runat="server" Visible="false" />
                    <%-- JPY Exrate保持 --%>
                    <asp:HiddenField ID="hdnJpyExRate" Value="" runat="server" Visible="false" />
                    <%-- 登録済のコピー元BRID保持用--%>
                    <asp:HiddenField ID="hdnOriginalCopyBrid" Value="" runat="server" Visible="false" />
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
                    <%-- TERM選択 VIEW　 --%>
                    <asp:View ID="vLeftTerm" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTerm" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END TERM選択 VIEW　 --%>
                    <%-- 国選択 VIEW　 --%>
                    <asp:View ID="vLeftCountry" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCountry" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 国選択 VIEW　 --%>
                    <%-- 港選択 VIEW　 --%>
                    <asp:View ID="vLeftPort" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbPort" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 港選択 VIEW　 --%>
                    <%-- 荷主選択 VIEW　 --%>
                    <asp:View ID="vLeftShipper" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbShipper" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 荷主選択 VIEW　 --%>
                    <%-- 荷受人選択 VIEW　 --%>
                    <asp:View ID="vLeftConsignee" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbConsignee" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 荷受人選択 VIEW　 --%>
                    <%-- 船会社 VIEW　 --%>
                    <asp:View ID="vLeftCarrier" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCarrier" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 船会社 VIEW　 --%>
                    <%-- 積載品 VIEW　 --%>
                    <asp:View ID="vLeftProduct" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbProduct" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 積載品 VIEW　 --%>
                    <%-- 費用項目 VIEW　 --%>
                    <asp:View ID="vLeftCost" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCost" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 費用 VIEW　 --%>
                    <%-- 業者 VIEW　 --%>
                    <asp:View ID="vLeftContractor" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbContractor" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 業者 VIEW　 --%>
                    <%-- エージェント VIEW　 --%>
                    <asp:View ID="vLeftAgent" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbAgent" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END エージェント VIEW　 --%>
                    <%-- MSDS VIEW --%>
                    <asp:View ID="vLeftMSDS" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbMSDS" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END MSDS VIEW　 --%>
                    <%-- 請求先 VIEW --%>
                    <asp:View ID="vLeftBillingCategory" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbBillingCategory" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 請求先 VIEW　 --%>
                </asp:MultiView>
            </div> <%-- END 左ボックス --%>
            <%-- 右ボックス --%>
            <div id="divRightbox" runat="server">
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
                                                If(mvRightMessage.ActiveViewIndex = "1", Me.rbShowError.Text, Me.rbShowMemo.Text),
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
                                <span id="spnRightRemarks">
                                    <asp:TextBox ID="txtRightRemarks" runat="server" TextMode="MultiLine"></asp:TextBox>
                                </span>
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
            <%-- 送信先設定ポップアップ --%>
            <div id="divSendConfirmBoxWrapper">
                <div id="divSendConfirmBox">
                    <div id="divSendConfirmTitle">
                        <asp:Label ID="lblSendTargetMessage" runat="server" Text="Select mail recipient"></asp:Label>
                    </div>
                    <div id="divSendConfirmButtons">
                        <input id="btnSelectMailOk" type="button" value="OK" runat="server" />
                        <input id="btnSelectMailCancel" type="button" value="CANCEL" runat="server" />
                    </div>
                    <div id="divSendConfirmCheckBoxes">
                        <ul>
                            <li>
                                <asp:Label ID="lblchkExport1" runat="server" Text="Export1："></asp:Label>
                                &nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="chkInputRequestExport1" runat="server" Text="Input Request" />
                                &nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="chkMailExport1" runat="server" Text="Mail"  />
                            </li>
                            <li>
                                <asp:Label ID="lblchkImport1" runat="server" Text="Import1："></asp:Label>
                                &nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="chkInputRequestImport1" runat="server" Text="Input Request" />
                                &nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="chkMailInport1" runat="server" Text="Mail"  />
                            </li>
                            <li>
                                <asp:Label ID="lblchkExport2" runat="server" Text="Export2："></asp:Label>
                                &nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="chkInputRequestExport2" runat="server" Text="Input Request" />
                                &nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="chkMailExport2" runat="server" Text="Mail"  />
                            </li>
                            <li>
                                <asp:Label ID="lblchkImport2" runat="server" Text="Import2："></asp:Label>
                                &nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="chkInputRequestImport2" runat="server" Text="Input Request" />
                                &nbsp;&nbsp;&nbsp;
                                <asp:CheckBox ID="chkMailInport2" runat="server" Text="Mail"  />
                            </li>
                        </ul>
                    </div>
                    <asp:HiddenField ID="hdnSelectMailTarger" runat="server" Value="" />
                </div>
            </div>
            <%-- EntryCostメール有無設定ポップアップ --%>
            <div id="divEntryCostSendConfirmBoxWrapper">
                <div id="divEntryCostSendConfirmBox">
                    <div id="divEntryCostSendConfirmTitle">
                        <%= Me.hdnEntryCostFieldName.Value %>
                        <%--<asp:Label ID="lblEntryCostSendTargetMessage" runat="server" Text="Select whether to send mail"></asp:Label>--%>
                    </div>
                    <div id="divEntryCostSendConfirmButtons">
                        <input id="btnEntryCostSelectMailOk" type="button" value="OK" runat="server" />
                        <input id="btnEntryCostSelectMailYes" type="button" value="YES" runat="server" />
                        <input id="btnEntryCostSelectMailNo" type="button" value="NO" runat="server" />
                        <input id="btnEntryCostSelectMailCancel" type="button" value="CANCEL" runat="server" />
                    </div>
                    <div id="divEntryCostSendConfirmCheckBoxes">
                        <ul>
                            <li><asp:CheckBox ID="chkMailSend" runat="server" Text="Send" Checked="true"/></li>
                        </ul>
                    </div>
                    <asp:HiddenField ID="hdnEntryCostSelectMailTarger" runat="server" Value="" />
                </div>
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
                        <input id="btnRemarkInputEdit" type="button" value="EDIT" runat="server" />
                    </div>
                    <div id="divRemarkTextArea">
                        <asp:TextBox ID="txtRemarkInput" runat="server" TextMode="MultiLine"></asp:TextBox>
                    </div>
                </div>
            </div>
            <%-- 申請確認ポップアップ --%>
            <div id="divApplyMsgBoxWrapper" runat="server">
                <div id="divApplyMsgBox">
                    <div id="divApplyMsgtitle">
                        <%= Me.hdnMsgboxFieldName.Value %>
                    </div>
                    <div id="divMsgButtons">
                        <input id="btnApplyMsgYes" type="button" value="YES" runat="server" />
                        <input id="btnApplyMsgNo" type="button" value="NO" runat="server" visible="false" />
                        <input id="btnApplyMsgCancel" type="button" value="CANCEL" runat="server"  />
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
