<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00012REPAIR.aspx.vb" Inherits="OFFICE.GBT00012REPAIR"  %>
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
    <link href="~/GB/css/GBT00012REPAIR.css" rel="stylesheet" type="text/css" />
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
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00012REPAIR.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">

        // ドロップ処理（ドラッグドロップ入力）
        function f_dragEvent(e,kbn) {
            e.preventDefault();
            e.stopPropagation();
            commonDispWait();
            var footerMsg = document.getElementById("lblFooterMessage");
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
                    if (kbn == "FILE_UP") {
                        //var ExcelReg = new RegExp("^.*\.xlsx$");
                        //var WordReg = new RegExp("^.*\.docx$");
                        //var JpgReg = new RegExp("^.*\.jpg$");
                        //var PngReg = new RegExp("^.*\.png");
                        //var GifReg = new RegExp("^.*\.gif$");
                        //var GmpReg = new RegExp("^.*\.gmp$");
                        //var PdfReg = new RegExp("^.*\.pdf$");
                        //var arrList = [ExcelReg, WordReg, JpgReg, PngReg, GifReg, GmpReg, PdfReg];
                        var gFlg = "0"
                        //for (var j = 0; j < arrList.length ; j++) {
                        //    if (files[i].name.toLowerCase().match(arrList[j])) {
                                fd.append("files", files[i]);
                        //        gFlg = "0"
                        //        break;
                        //    } else {
                        //        gFlg = "1"
                        //    }
                        //}
                        if (gFlg == "1") {
                            footerMsg.textContent = '<%= Me.hdnUploadError05.Value %>';
                            footerMsg.style.color = "red";
                            footerMsg.style.fontWeight = "bold";
                            footerMsg.value = '<%= Me.hdnUploadError05.Value %>';
                            commonHideWait();
                            return;
                        }
                    } else {
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
               }

                // XMLHttpRequest オブジェクトを作成
                var xhr = new XMLHttpRequest();

                // ドロップファイルによりURL変更
                // 「POST メソッド」「接続先 URL」を指定
                xhr.open("POST",'<%= ResolveUrl("~/COH0001FILEUP.ashx") %>' , false)

                // イベント設定
                // ⇒XHR 送信正常で実行されるイベント
                xhr.onload = function (e) {
                    if (e.currentTarget.status == 200) {

                        if (kbn == "FILE_UP") {
                            document.getElementById("hdnListUpload").value = "FILE_LOADED";
                        } else {
                            document.getElementById("hdnListUpload").value = "XLS_LOADED";
                        }
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


        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            changeCommonInfoArea();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>', '<%= Me.btnOutputExcel.ClientId %>',
                                       '<%= Me.btnOutputFile.ClientId  %>',
                                       '<%= Me.btnSave.ClientId  %>',
                                       '<%= Me.btnRemarkInputOk.ClientId  %>',
                                       '<%= Me.btnRemarkInputCancel.ClientId  %>',
                                       '<%= Me.btnApply.ClientID %>',
                                       '<%= Me.btnApproval.ClientId  %>',
                                       '<%= Me.btnReject.ClientId  %>',
                                       '<%= Me.btnApplyMsgYes.ClientId  %>',
                                       '<%= Me.btnApplyMsgNo.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* タブクリックイベントのバインド */
            var targetTabObjects = ['<%= Me.tabRepair.ClientID %>','<%= Me.tabFileUp.ClientID %>','<%= Me.tabDoneFileUp.ClientID %>']

            bindTabClickEvent(targetTabObjects);
            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';
            var viewTankNo = '<%= Me.vLeftTankNo.ClientID %>';
            var viewDepot = '<%= Me.vLeftDepot.ClientID %>';
            var viewProduct = '<%= Me.vLeftProduct.ClientID %>';
            var viewDelFlg = '<%= Me.vLeftDelFlg.ClientID %>';
            var dblClickObjects = [['<%= Me.txtAppRequestYmd.ClientID %>', viewCalId],
                                   ['<%= Me.txtApprovedYmd.ClientID %>', viewCalId],
                                   ['<%= Me.txtTankNo.ClientID %>', viewTankNo],
                                   ['<%= Me.txtDepoCode.ClientID %>', viewDepot],
                                   ['<%= Me.txtDepoInDate.ClientID %>', viewCalId],
                                   ['<%= Me.txtLastProduct.ClientID %>', viewProduct],
                                   ['<%= Me.txtTwoAgoProduct.ClientID %>', viewProduct],
                                   ['<%= Me.txtDeleteFlag.ClientID %>', viewDelFlg]
            ];
            var txtAttachmentDelFlgObjects = document.querySelectorAll('input[id^="dViewRep_txtRepDelFlg_"');
            for (let i = 0; i < txtAttachmentDelFlgObjects.length; i++) {
                dblClickObjects.push([txtAttachmentDelFlgObjects[i].id, viewDelFlg]);
            }
            var txtAfterAttachmentDelFlgObjects = document.querySelectorAll('input[id^="dDoneViewRep_txtRepDelFlg_"');
            for (let i = 0; i < txtAfterAttachmentDelFlgObjects.length; i++) {
                dblClickObjects.push([txtAfterAttachmentDelFlgObjects[i].id, viewDelFlg]);
            }
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
            var leftListExtentionTarget = [['<%= Me.lbTankNo.ClientID %>', '3', '1'],
                                           ['<%= Me.lbDelFlg.ClientID %>', '3', '1'],
                                           ['<%= Me.lbTankUsage.ClientID %>', '3', '1'],
                                           ['<%= Me.lbCost.ClientID %>', '3', '1'],
                                           ['<%= Me.lbDepot.ClientID %>', '3', '1'],
                                           ['<%= Me.lbProduct.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = ['<%= Me.txtTankNo.ClientID %>',
                                         '<%= Me.txtDepoCode.ClientID %>',
                                         '<%= Me.txtDeleteFlag.ClientID %>',
                                         '<%= Me.txtTankUsage.ClientID %>',
                                         '<%= Me.txtLastProduct.ClientID %>',
                                         '<%= Me.txtTwoAgoProduct.ClientID %>'
            ];
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
            /* Applyボタンの選択ポップアップ表示イベント関係の紐付け */
            bindApplyOnClick();
            /* 費用項目グリッドのイベントバインド */
            bindCostRowEvents();
            /* 左ボックスの備考ダブルクリックイベント */
            bindSpnRightRemarksDbClick();
            /* 備考欄のダブルクリックイベントバインド */
            bindRemarkDblClick();
            /* 各種計算項目のイベントバインド */
            bindHireageCommercialfactorOnBlur();
            /* ファイルドラッグ＆ドロップのイベントバインド */
            /* 費目入力 */
            var hdnUplExcelObj = document.getElementById('hdnInputExcel');
            var dragDropObj = document.getElementById('divContainer');
            if (dragDropObj !== null) {
                dragDropObj.addEventListener("dragstart", f_dragEventCancel, false);
                dragDropObj.addEventListener("drag", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragend", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragenter", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragleave", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragover", f_dragEventCancel, false);
            }
            if (hdnUplExcelObj !== null) {
            var dragDropObj = document.getElementById('divContainer');
                if (dragDropObj !== null) {
                    dragDropObj.addEventListener("drop", f_dragEvent, false);
                }
            }
            /* 添付ファイル */
            var hdnInputFileObj = document.getElementById('hdnInputFile');
            if (hdnInputFileObj !== null) {
                var ddFileObjs = ['dViewRepArea', 'dDoneViewRepArea'];
                for (let i = 0; i < ddFileObjs.length; i++) {
                    var dragDropFileObj = document.getElementById(ddFileObjs[i]);
                    if (dragDropFileObj !== null) {
                        dragDropFileObj.addEventListener("dragstart", f_dragEventCancel, false);
                        dragDropFileObj.addEventListener("drag", f_dragEventCancel, false);
                        dragDropFileObj.addEventListener("dragend", f_dragEventCancel, false);
                        dragDropFileObj.addEventListener("dragenter", f_dragEventCancel, false);
                        dragDropFileObj.addEventListener("dragleave", f_dragEventCancel, false);
                        dragDropFileObj.addEventListener("dragover", f_dragEventCancel, false);
                        dragDropFileObj.addEventListener("drop",function(){f_dragEvent(event,'FILE_UP')} , false);
                    }
                }
            }
            //ダブルクリックEvent紐づけ
            bindDoubleClickEvent('<%= Me.txtTankUsage.ClientID %>')

            var scrollTop = document.getElementById("hdnBodyScrollTop");
            if (scrollTop.value !== "") {
                document.getElementById("divContensbox").scrollTop = scrollTop.value;
                scrollTop.value = "";
            }
            /* テキストポップアップ表示設定 */
            setDisplayNameTip();
            var brRemark = document.getElementById('lblBrRemarkText');
            brRemark.removeAttribute('title');
            /* アップロードボタンの設定(Repair-Infoタブ) */
            addUploadExtention('<%= Me.hdnInputExcel.ClientID %>', 'AFTER', false, 'divContainer');
            /* アップロードボタンの設定(File-Infoタブ)複数選択可能 */
            addUploadExtention('<%= Me.hdnInputFile.ClientID %>', 'AFTER', true, 'dViewRepArea', 'Upload');
            /* アップロードボタンの設定(File-Infoタブ)複数選択可能 */
            addUploadExtention('<%= Me.hdnInputFile.ClientID %>', 'AFTER', true, 'dDoneViewRepArea','Upload');
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
    <form id="GBT00012R" runat="server">
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
                                <asp:Label ID="lblBrNoTitle" runat="server" Text=""></asp:Label>
                            </td>
                            <td colspan="3">
                                <asp:Label ID="lblBrNo" runat="server" Text=""></asp:Label>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblBrRemark" runat="server" Text="BR注記" Font-Underline="true"></asp:Label>
                            </td>
                            <td colspan="7">
                                <span id="spnBrRemark" <%= If(Me.lblBrRemarkText.Enabled, "", "class=""aspNetDisabled""") %>>
                                <%= If(Me.lblBrRemarkText.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                <asp:label ID="lblBrRemarkText" runat="server" Text=""></asp:label>
                                </span>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow2" runat="server">
                            <td>
                              &nbsp;
                            </td>
                            <td colspan="13">
                                &nbsp;
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow3" runat="server">
                            <td colspan="3">
                                &nbsp;
                            </td>
                            <td>
                                <asp:Label ID="lblApploveDate" runat="server" Text="Date"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblAgent" runat="server" Text="Agent"></asp:Label>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblPic" runat="server" Text="Pic"></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblAppRemarks" runat="server" Text="Remarks" Font-Underline="true"></asp:Label>
                            </td>
                            <td colspan="6">
                                &nbsp;
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow4" runat="server">
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
                        <tr id="trBrInfoRow5" runat="server">
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
                        <tr id="trBrInfoRow6" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblTankNo" runat="server" Text="Tank No." Font-Underline="true" CssClass="requiredMark2"></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtTankNo" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblLeaseCheck" runat="server" Text="Lease Check"></asp:Label>
                            </td>
                            <td >
                                <span onclick ="f_checkLeaseEvent(event)" >
                                    <asp:CheckBox ID="chkLeaseCheck" runat="server" Text="" ></asp:CheckBox>
                                </span>
                            </td>
                            <td colspan="8">
                                &nbsp;
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
                                <asp:Label ID="lblDepoInDate" runat="server" Text="DepoIn Date"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtDepoInDate" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblDepoCode" runat="server" Text="Depo Code"></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtDepoCode" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblDepoCodeText" runat="server" Text=""></asp:Label>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblLocation" runat="server" Text="Location"></asp:Label>
                            </td>
                            <td colspan="5">
                                <asp:TextBox ID="txtLocation" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow11" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblBreakerNo" runat="server" Text="Breaker No"></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtBreakerNo" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblLastOrderNo" runat="server" Text="Last Order No" ></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtLastOrderNo" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td colspan="8">
                                &nbsp;
                            </td>
                            <td >
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow8" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblLastProduct" runat="server" Text="Last Product" ></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtLastProduct" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblLastProductText" runat="server" Text="" ></asp:Label>
                            </td>
                            <td class="textRightCell">
                                <asp:Label ID="lblTwoAgoProduct" runat="server" Text="Two Ago Product" ></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtTwoAgoProduct" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblTwoAgoProductText" runat="server" Text="" ></asp:Label>
                            </td>
                            <td colspan="4">
                                &nbsp;
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow9" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2" class="textRightCell">
                                <asp:Label ID="lblDeleteFlag" runat="server" Text="Delete" Font-Underline="true" CssClass="requiredMark2"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtDeleteFlag" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblDeleteFlagText" runat="server" Text="" ></asp:Label>
                            </td>
                            <td  class="textRightCell">
                                <asp:Label ID="lblTankUsage" runat="server" Text="Tank Usage" ></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtTankUsage" runat="server" Text="" ></asp:TextBox>
                            </td>
                            <td>
                                <asp:Label ID="lblTankUsageText" runat="server" Text="" ></asp:Label>
                            </td>
                            <td colspan="5">
                                &nbsp;
                            </td>
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBrInfoRow10" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblSettlementOffice" runat="server" Text="Settlement Office"></asp:Label>
                            </td>
                            <td >
                                <asp:TextBox ID="txtSettlementOffice" runat="server" Text="" ></asp:TextBox>
                            </td>
                            <td colspan="2">
                                <asp:Label ID="lblSettlementOfficeText" runat="server" Text=""></asp:Label>
                            </td>
                            <td >
                                <asp:Label ID="lblRemark" runat="server" Text="Special Instructions" Font-Underline="true" ></asp:Label>
                            </td>
                            <td colspan="7">
                                <span id="spnRemarks" <%= If(Me.lblRemarks.Enabled, "", "class=""aspNetDisabled""") %>>
                                    <%= If(Me.lblRemarks.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                    <asp:Label ID="lblRemarks" runat="server" Text="" Enabled="False"></asp:Label>
                                </span>
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
                        <div id="tabRepair" runat="server" >Repair-Info</div>
                        <div id="tabFileUp" runat="server">File-Info</div>
                        <div id="tabDoneFileUp" runat="server">Done-File-Info</div>
                    </div>
                    <div class="tabRightSide">
                        <span id="spnShowCommonInfo" >Show CommonInfo</span>
                        <!-- 共通情報を表示するか(0=表示しない,1(Default)=表示する) -->
                        <asp:HiddenField ID="hdnIsShowCommonInfo" runat="server" Value="1" />
                    </div>
                </div>
                <div id="actionButtonsBox" runat="server">
                    <span id="spnActButtonBox" runat="server" visible="true">
                        <input id="btnOutputExcel" type="button" value="エクセル出力" runat="server" />
                        <input id="hdnInputExcel" type="hidden" runat="server" />
                        <input id="btnSave" type="button" value="保存" runat="server" />
                        <%--<input id="btnInputRequest" type="button" value="登録"  runat="server" />--%>
                        <input id="btnApply" type="button" value="申請"  runat="server" />
                    </span>
                    <span id="spnAppButtonBox" runat="server" visible="true">
                        <input id="btnApproval" type="button" value="承認"  runat="server" />
                        <input id="btnReject" type="button" value="否認"  runat="server" />
                    </span>
                    <input id="btnOutputFile" type="button" value="ﾀﾞｳﾝﾛｰﾄﾞ"  runat="server"  />
                    <input id="hdnInputFile" type="hidden" runat="server" />
                    <input id="btnBack" type="button" value="戻る"  runat="server"  />
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
                            <asp:Label ID="lblCostRemarkCanEntry" runat="server" Text="" Visible="false"></asp:Label></td>
                        <td colspan="5">
                            <input id="btnAddCost" type="button" value="追加"  runat="server"/>
                        </td>
                        <td class="textCenter">
                            <asp:Label ID="lblEstimatedSummary" runat="server" Text="推定合計"></asp:Label>
                        </td>
                        <td class="textCenter">
                            <asp:Label ID="lblBulkCheck" runat="server" Text="Bulk Check"></asp:Label>
                        </td>
                        <td class="textCenter">
                            <asp:Label ID="lblApprovedSummary" runat="server" Text="承認済み合計"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" class="textRightCell">
                            <asp:Label ID="lblLocalRateRef" runat="server" Text="Loc.Cur Rate"></asp:Label>
                        </td>
                        <td colspan="5">
                            <asp:TextBox ID="txtLocalCurrencyRef" runat="server" Text="" ></asp:TextBox>
                            <asp:TextBox ID="txtLocalRateRef" runat="server" Text="" CssClass="textRight"></asp:TextBox>
                        </td>
                        <td >
                            <input id="iptEstimatedSummary" type="text" disabled="true" class="aspNetDisabled textRight" runat="server" />
                        </td>
                        <td >
                            <span onclick ="f_checkEvent(event)" >
                                <asp:CheckBox ID="chkBulkCheck" runat="server" Text="" ></asp:CheckBox>
                            </span>
                        </td>
                        <td>
                            <input id="iptApprovedSummary" type="text" disabled="true" class="aspNetDisabled textRight" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td>
                            &nbsp;
                        </td>
                        <td colspan="14">
                            <asp:GridView ID="gvDetailInfo" runat="server" AutoGenerateColumns="False" ShowFooter="False" GridLines="None"  CellSpacing="-1" ShowHeaderWhenEmpty="True" >
                                <Columns >
                                    <asp:TemplateField HeaderText="action">
                                        <ItemTemplate>
                                            <asp:Panel ID="pnlDeleteButtonArea" runat="server" Visible='<%# IIf(Eval("IsAddedCost") = "1", "True", "False") %>'>
                                                <input id='btnDeleteCostItem_<%# Container.DataItemIndex %>' type="button" value='<%= Me.hdnDispDeleteBtnText.Value %>' data-uniqueindex='<%#Eval("UniqueIndex") %>' <%= If(Me.gvDetailInfo.Enabled = False OrElse Me.hdnApprovalFlg.Value = "1", "class=""aspNetDisabled"" disabled=""disabled""", "") %> />
                                            </asp:Panel>
                                            <asp:HiddenField ID="hdnCostCode" runat="server"  Value='<%# Bind("CostCode") %>' />
                                            <asp:HiddenField ID="hdnItem1" runat="server"  Value='<%# Bind("Item1") %>' />
                                            <%--<asp:HiddenField ID="hdnItem2" runat="server"  Value='<%# Bind("Item2") %>' />--%>
                                            <asp:HiddenField ID="hdnRemarks" runat="server"  Value='<%# Bind("Remarks") %>' />
                                            <asp:HiddenField ID="hdnClass2" runat="server"  Value='<%# Bind("Class2") %>' />
                                            <asp:HiddenField ID="hdnClass4" runat="server"  Value='<%# Bind("Class4") %>' />
                                            <asp:HiddenField ID="hdnClass8" runat="server"  Value='<%# Bind("Class8") %>' />
                                            <asp:HiddenField ID="hdnSortOrder" runat="server"  Value='<%# Bind("SortOrder") %>' />
                                            <asp:HiddenField ID="hdnIsAddedCost" runat="server"  Value='<%# Bind("IsAddedCost") %>' />
                                            <asp:HiddenField ID="hdnItemGroup" runat="server"  Value='<%# Bind("ItemGroup") %>' />
                                            <asp:HiddenField ID="hdnUniqueIndex" runat="server"  Value='<%# Bind("UniqueIndex") %>' />
                                            <asp:HiddenField ID="hdnLocalRate" runat="server"  Value='<%# Bind("LocalCurrncyRate") %>' />
                                            <asp:HiddenField ID="hdnLocalCurrncy" runat="server"  Value='<%# Bind("LocalCurrncy") %>' />
                                            <asp:HiddenField ID="hdnCountryCode" runat="server"  Value='<%# Bind("CountryCode") %>' />
                                            <asp:HiddenField ID="hdnInvoicedBy" runat="server"  Value='<%# Bind("InvoicedBy") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField HeaderText='' DataField="CostCode" />
                                    <asp:BoundField HeaderText="" DataField="Item1" />
                                    <%--<asp:BoundField HeaderText="" DataField="Item2" />--%>
                                    <asp:TemplateField HeaderText="">
                                        <ItemTemplate>
                                            <span id='spnCostRemarks_<%# Container.DataItemIndex %>'  data-uniqueindex='<%#Eval("UniqueIndex") %>' data-disableflg="<%= If(Me.lblCostRemarkCanEntry.Enabled = False, "Y", "") %>" >
                                                <%# If(Eval("Remarks") = "" AndAlso Me.lblCostRemarkCanEntry.Enabled = False, "<span class=""aspNetDisabled remarksMessage"" disabled=""disabled""  title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                                <%# IF(Eval("Remarks") = "" AndAlso Me.lblCostRemarkCanEntry.Enabled, "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                                <asp:Label ID="lblCostRemarks" runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("Remarks")) %>' Enabled="false" ></asp:Label>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtLocal" runat="server" text='<%# Bind("Local") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' onclick="this.select(0,this.value.length)"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtUsd" runat="server" text='<%# Bind("USD") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' onclick="this.select(0,this.value.length)"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="chkApp" runat="server" text='<%# Bind("RepairFlg") %>' data-uniqueindex='<%#Eval("UniqueIndex") %>' onclick ="f_checkAppEvent(this)" Enabled='<%# IF (Me.chkBulkCheck.Enabled, "True", "False") %>'></asp:CheckBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtApprovedUsd" runat="server" text='<%# Bind("ApprovedUsd") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' onclick="this.select(0,this.value.length)" Enabled="false"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </td>                        
                    </tr>
                </table>
                </div>
                <div id="divFileUpInfo" runat="server">
                <table class="infoTable fileup" >
                    <colgroup>
                        <col /><col /><col />
                    </colgroup>
                    <tbody>
                    <tr>
                        <td>&nbsp;</td>
                        <td>
                            <asp:MultiView ID="mltvFileUp" runat="server" ActiveViewIndex="0">
                                <asp:View ID="vFileUp" runat="server">
                                    <div id="dViewRepArea">
                                        <div id="divAttachmentHeaderArea">
                                            <asp:Label ID="lblFileName" runat="server" Text="File Name" CssClass="textLeft" ></asp:Label>
                                            <asp:Label ID="lblDropDesc" runat="server" Text="To register attached documents, drop it here" CssClass="textLeft" ></asp:Label>
                                            <asp:Label ID="lblUnder" runat="server" Text="↓↓↓" CssClass="textLeft" visible="false"></asp:Label>
                                            <asp:Label ID="lblDelete" runat="server" Text="Delete" CssClass="textCenter" ></asp:Label>
                                        </div>
                                        <div id="divRepAttachments">
                                        <asp:Repeater ID="dViewRep" runat="server" >
                                            <HeaderTemplate>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <table>
                                                    <tr>
                                                    <td ondblclick='dispAttachmentFile("<%# Eval("FILENAME") %>");'>
                                                        <%-- ファイル記号名称 --%>
                                                        <asp:Label ID="lblRepFileName" runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("FILENAME")) %>' Title='<%# Eval("FILENAME") %>' CssClass="textLeft"></asp:Label>
                                                    </td>
                                                    <td>
                                                        <%-- 削除 --%>
                                                        <asp:TextBox ID="txtRepDelFlg" runat="server" CssClass="textCenter" Text='<%# Eval("DELFLG") %>' Enabled='<%# If(Me.hdnInputFile.Visible, "True", "False") %>'></asp:TextBox>
                                                    </td>
                                                    <td>
                                                        <%-- FILEPATH --%>
                                                        <asp:Label ID="lblRepFilePath" runat="server" CssClass="textLeft"></asp:Label>
                                                    </td>
                                                </tr>
                                                </table>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                        </div>
                                    </div>
                                </asp:View>
                            </asp:MultiView>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    </tbody>
                </table>
                </div>
                <div id="divDoneFileUpInfo" runat="server">
                <table class="infoTable fileup" >
                    <colgroup>
                        <col /><col /><col /><col /><col />
                        <col /><col /><col /><col /><col />
                        <col /><col /><col /><col /><col />
                    </colgroup>
                    <tbody>
                    <tr>
                        <td colspan="14">
                            <asp:MultiView ID="mltvDoneFileUp" runat="server" ActiveViewIndex="0">
                                <asp:View ID="vDoneFileUp" runat="server">
                                    <div id="dDoneViewRepArea">
                                        <div id="divDoneAttachmentHeaderArea">
                                            <asp:Label ID="lblDoneFileName" runat="server" Text="File Name" ></asp:Label>
                                            <asp:Label ID="lblDoneDropDesc" runat="server" Text="To register attached documents, drop it here" CssClass="textLeft" ></asp:Label>
                                            <asp:Label ID="lblDoneUnder" runat="server" Text="↓↓↓" CssClass="textLeft" visible="false"></asp:Label>
                                            <asp:Label ID="lblDoneDelete" runat="server" Text="Delete" CssClass="textCenter"></asp:Label>
                                        </div>
                                        <div id="divDoneRepAttachments">
                                        <asp:Repeater ID="dDoneViewRep" runat="server" >
                                            <HeaderTemplate>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <table>
                                                    <tr>
                                                        <td ondblclick='dispAttachmentFile("<%# Eval("FILENAME") %>");'>
                                                            <%-- ファイル記号名称 --%>
                                                            <asp:Label ID="lblRepFileName" runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("FILENAME")) %>' Title='<%# Eval("FILENAME") %>' CssClass="textLeft"></asp:Label>
                                                        </td>
                                                        <td>
                                                            <%-- 削除 --%>
                                                            <asp:TextBox ID="txtRepDelFlg" runat="server" CssClass="textCenter" Text='<%# Eval("DELFLG") %>' Enabled='<%# If(Me.hdnInputFile.Visible, "True", "False") %>'></asp:TextBox>
                                                        </td>
                                                        <td>
                                                            <%-- FILEPATH --%>
                                                            <asp:Label ID="lblRepFilePath" runat="server" CssClass="textLeft"></asp:Label>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                        </div>
                                    </div>
                                </asp:View>
                            </asp:MultiView>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    </tbody>
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
                    <%-- Download用 --%>
                    <asp:HiddenField ID="hdnZipURL" runat="server" />
                    <%-- フッターヘルプ関連処理で使用 --%>
                    <asp:HiddenField ID="hdnHelpChange" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCanHelpOpen" runat="server" Value="" />
                    <%-- 画面固有 --%>
                    <asp:HiddenField ID="hdnDispDeleteBtnText" value="Delete" runat="server" />
                    <asp:HiddenField ID="hdnDispLeftBoxCostCode" value="コード" runat="server" />
                    <asp:HiddenField ID="hdnDispLeftBoxItem1" value="項目1" runat="server" />
                    <%--<asp:HiddenField ID="hdnDispLeftBoxItem2" value="項目2" runat="server" />--%>
                    <asp:HiddenField ID="hdnIsTrilateral" runat="server" /> <%-- 三国間か？ "1"=三国間 それ以外=二国 --%>
                    <asp:HiddenField ID="hdnSelectedTabId" runat="server" Value="" /> <%-- 選択中のタブ --%>
                    <asp:HiddenField ID="hdnBackUrl" value="" runat="server" />
                    <asp:HiddenField ID="hdnBodyScrollTop" value="" runat="server" />
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
                    <asp:HiddenField ID="hdnStYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnEndYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnShipper" runat="server" Value="" />
                    <asp:HiddenField ID="hdnConsignee" runat="server" Value="" />
                    <asp:HiddenField ID="hdnPort" runat="server" Value="" />
                    <asp:HiddenField ID="hdnApproval" runat="server" Value="" />
                    <asp:HiddenField ID="hdnOffice" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCorrection" runat="server" Value="" />
                    <asp:HiddenField ID="hdnDenial" runat="server" Value="" />
                    <asp:HiddenField ID="hdnStep" runat="server" Value="" />
                    <asp:HiddenField ID="hdnMsgId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnApprovalObj" runat="server" Value="" />
                    <asp:HiddenField ID="hdnStatus" runat="server" Value="" />
                    <asp:HiddenField ID="hdnTankNo" runat="server" Value="" />
                    <asp:HiddenField ID="hdnDepot" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSubId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnLinkId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCountry" runat="server" Value="" />
                    <asp:HiddenField ID="hdnReportVariant" runat="server" Value="" />
                    <%-- アップロード一覧 --%> 
                    <asp:ListBox ID="hdnListBox" runat="server"></asp:ListBox>
                    <%-- 前画面XMLファイル保持用 --%>
                    <asp:HiddenField ID="hdnXMLsaveFileRet" runat="server" Value="" />
                    <%-- 承認制御用 --%>
                    <asp:HiddenField ID="hdnReject" runat="server" Value="" />
                    <asp:HiddenField ID="hdnAppJotRemarks" runat="server" Value="" />
                    <asp:HiddenField ID="hdnRejBtn" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCostSelectedTabId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnRemarkFlg" runat="server" Value="" />
                    <asp:HiddenField ID="hdnRemarkInitFlg" runat="server" Value="" />
                    <asp:HiddenField ID="hdnApprovalFlg" runat="server" Value="" />
                    <asp:HiddenField ID="hdnAppTranFlg" runat="server" Value="" />
                    <asp:HiddenField ID="hdnDelFlg" runat="server" Value="" />
                    <asp:HiddenField ID="hdnAlreadyFlg" runat="server" Value="" />
                    <asp:HiddenField ID="hdnHistoryFlg" runat="server" Value="" />
                    <%-- DetailBox File内容表示 --%>
                    <asp:HiddenField ID="hdnFileDisplay" runat="server" Value="" />
                    <%-- 一括チェック用 --%>
                    <asp:HiddenField ID="hdnBulkCheckChange" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCheckAppChange" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCheckUniqueNumber" runat="server" Value="" />
                    <%-- Leaseチェック用 --%>
                    <asp:HiddenField ID="hdnLeaseCheckChange" runat="server" Value="" />
                    <%-- 前画面(承認画面)保持用 --%>
                    <asp:HiddenField ID="hdnPrevViewID" runat="server" Value="" />
                    <asp:HiddenField ID="hdnApplyId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnLastStep" runat="server" Value="" />
                    <asp:HiddenField ID="hdnBrId" runat="server" Value="" />
                    <%-- 前々画面(承認検索条件画面)保持用 --%>
                    <asp:HiddenField ID="hdnGBT00012STankNo" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnLastCargo" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnLocation" runat="server" Value="" Visible="false" />
                    <%-- オーガナイザー情報保持 --%>
                    <asp:HiddenField ID="hdnCountryOrganizer" runat="server" Value="" Visible="false" />
                    <%-- ダブルクリックしたフィールド値を格納 --%>
                    <asp:HiddenField ID="hdnDbClickField" runat="server" Value="" /> 
                    <%-- 終了(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnMsgboxShowFlg" Value="0" runat="server" />
                    <asp:HiddenField ID="hdnMsgboxAppChangeFlg" Value="" runat="server" />
                    <asp:HiddenField ID="hdnMsgboxFieldName" Value="" runat="server" />
                    <%-- タンクマスタ制御用 --%>
                    <asp:HiddenField ID="hdnSelectedTankNo" Value="" runat="server" />
                    <%-- 登録情報 --%>
                    <asp:HiddenField ID="hdnInitYmd" Value="" runat="server" />
                    <asp:HiddenField ID="hdnInitUser" Value="" runat="server" />
                    <%-- MapVariant退避 --%>
                    <asp:HiddenField ID="hdnThisMapVariant" runat="server" Value="" Visible="false" />
                    <%-- BRオーガナイザ国コード --%>
                    <asp:HiddenField ID="hdnCountryOrg" runat="server" Value="" Visible="false" />
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
                    <%-- 費用項目 VIEW　 --%>
                    <asp:View ID="vLeftCost" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCost" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 費用 VIEW　 --%>
                    <%--  　削除フラグ　 --%>
                    <asp:View id="vLeftDelFlg" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDelFlg" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 削除フラグ VIEW　 --%>
                    <%-- タンク番号 VIEW --%>
                    <asp:View ID="vLeftTankNo" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTankNo" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END タンク番号 VIEW　 --%>
                    <%-- デポ VIEW --%>
                    <asp:View ID="vLeftDepot" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDepot" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END デポ VIEW　 --%>
                    <%-- タンク使用法 VIEW --%>
                    <asp:View ID="vLeftTankUsage" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTankUsage" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END タンク使用法 VIEW　 --%>
                    <%-- Product VIEW --%>
                    <asp:View ID="vLeftProduct" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbProduct" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Product VIEW　 --%>
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
            <%-- 申請確認ポップアップ --%>
            <div id="divApplyMsgBoxWrapper" runat="server">
                <div id="divApplyMsgBox">
                    <div id="divApplyMsgtitle">
                        <%= Me.hdnMsgboxFieldName.Value %>
                    </div>
                    <div id="divMsgButtons">
                        <input id="btnApplyMsgYes" type="button" value="YES" runat="server" />
                        <input id="btnApplyMsgNo" type="button" value="NO" runat="server" />
                        <input id="btnApplyMsgCancel" type="button" value="CANCEL" runat="server"  />
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
