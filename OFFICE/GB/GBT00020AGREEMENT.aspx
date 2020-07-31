<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00020AGREEMENT.aspx.vb" Inherits="OFFICE.GBT00020AGREEMENT" %>

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
    <link href="~/GB/css/GBT00020AGREEMENT.css" rel="stylesheet" type="text/css" />
    <style>
    </style>
    <!-- Global site tag (gtag.js) - Google Analytics -->
<%--    <script async src="https://www.googletagmanager.com/gtag/js?id=UA-162522994-1"></script>
    <script>
      window.dataLayer = window.dataLayer || [];
      function gtag(){dataLayer.push(arguments);}
      gtag('js', new Date());

      gtag('config', 'UA-162522994-1');
    </script>--%>
    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8"></script>
    <%-- 左ボックスカレンダー使用の場合のスクリプト --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/script/calendar.js") %>'  charset="utf-8"></script>
    <%--個別のスクリプトは以下に記載 --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00020AGREEMENT.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加
        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            //changeCommonInfoArea();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>', '<%= Me.btnOutputExcel.ClientId %>',
                                       '<%= Me.btnSave.ClientId  %>',
                                       '<%= Me.btnReject.ClientId  %>',
                                       '<%= Me.btnAddNewTank.ClientID %>',
                                       '<%= Me.btnTankInputOk.ClientID %>',
                                       '<%= Me.btnCreateLeaseOrder.ClientID %>','<%= Me.btnDownloadFiles.ClientId %>',
                                       '<%= Me.btnApply.ClientID %>',
                                       '<%= Me.btnRemarkInputOk.ClientId  %>',
                                       '<%= Me.btnRemarkInputCancel.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewProduct = '<%= Me.vLeftProduct.ClientID %>';
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';
            var viewDepot = '<%= Me.vLeftDepot.ClientID %>';
            var viewAutoExtendKind = '<%= Me.vLeftAutoExtendKind.ClientID %>';
            var viewLeaseCurrency = '<%= Me.vLeftLeaseCurrency.ClientID %>';
            var viewYesNo = '<%= me.vLeftYesNo.ClientID %>';
            var viewLeaseTerm = '<%= Me.vLeftLeaseTerm.ClientID %>';
            var viewLeasePayment = '<%= me.vLeftLeaseType.ClientID %>';
            var viewPaymentMonth = '<%= me.vLeftPaymentMonth.ClientID %>';
            var viewLeasePaymentKind = '<%= me.vLeftLeasePaymentKind.ClientID %>';
            var viewTax = '<%= me.vLeftTax.ClientID %>';
            var viewYesNo = '<%= me.vLeftYesNo.ClientID %>';

            var dblClickObjects = [['<%= Me.txtLeaseTerm.ClientID %>', viewLeaseTerm],
                                   ['<%= Me.txtProduct.ClientID %>', viewProduct],
                                   ['<%= Me.txtStartDate.ClientID %>', viewCalId],
                                   ['<%= Me.txtEndDateSche.ClientID %>', viewCalId],
                                   ['<%= Me.txtEndDate.ClientID %>',viewCalId],
                                   ['<%= Me.txtDepoIn.ClientID %>',viewDepot], 
                                   ['<%= Me.txtSegSwStartDate.ClientID %>', viewCalId],
                                   ['<%= Me.txtSegSwEndDate.ClientID %>', viewCalId],
                                   ['<%= Me.txtAutoExtendKind.ClientID %>',viewAutoExtendKind],
                                   ['<%= Me.txtCurrency.ClientID %>', viewLeaseCurrency],
                                   ['<%= Me.txtLeaseType.ClientID %>', viewLeasePayment],
                                   ['<%= Me.txtPaymentMonth.ClientID %>', viewPaymentMonth],
                                   ['<%= Me.txtLeasePaymentKind.ClientID %>', viewLeasePaymentKind],
                                   ['<%= Me.txtTax.ClientID %>', viewTax],
                                   ['<%= Me.txtAutoExtend.ClientID %>', viewYesNo]];

            var txtAttachmentDelFlgObjects = document.querySelectorAll('input[id^="repAttachment_txtDeleteFlg_"');
            for (let i = 0; i < txtAttachmentDelFlgObjects.length; i++) {
                dblClickObjects.push([txtAttachmentDelFlgObjects[i].id, viewYesNo]);
            }
            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbProduct.ClientID %>', '3', '1'],
                                           ['<%= Me.lbLeaseTerm.ClientID %>', '3', '1'],
                                           ['<%= Me.lbLeaseType.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPaymentMonth.ClientID %>', '3', '1'],
                                           ['<%= Me.lbYesNo.ClientID %>', '3', '1'],
                                           ['<%= Me.lbAutoExtendKind.ClientID %>', '3', '1'],
                                           ['<%= Me.lbLeasePaymentKind.ClientID %>', '3', '1'],
                                           ['<%= Me.lbLeaseCurrency.ClientID %>', '3', '1'],
                                           ['<%= Me.lbTax.ClientID %>', '3', '1'],
                                           ['<%= Me.lbDepot.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = ['<%= Me.txtLeaseTerm.ClientID %>','<%= Me.txtLeaseType.ClientID %>',
                                         '<%= Me.txtProduct.ClientID %>',
                                         '<%= Me.txtPaymentMonth.ClientID %>', '<%= Me.txtAutoExtend.ClientID %>',
                                         '<%= Me.txtLeasePaymentKind.ClientID %>','<%= Me.txtTax.ClientID %>',
                                         '<%= Me.txtDepoIn.ClientID %>',
                                         '<%= Me.txtAutoExtendKind.ClientID %>']
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
            /* 備考欄のダブルクリックイベントバインド */
            bindRemarkDblClick();
            // D&Dイベント紐づけリスト(id:対象のオブジェクトID,kbn,許可拡張子配列(未指定時はすべて))
            var dragDropAreaObjectsList = [
                { id: 'divAttachmentArea', kbn: 'FILE_UP'}
            ];
            var enableUpload = document.getElementById('<%= Me.hdnUpload.ClientID  %>');
            if (enableUpload !== null) {
                if (enableUpload.disabled) {
                    dragDropAreaObjectsList = null;
                }
            }
            bindCommonDragDropEvents(dragDropAreaObjectsList, '<%= ResolveUrl(OFFICE.CommonConst.C_UPLOAD_HANDLER_URL)  %>');
            /* アップロードボタンの設定 */
            addUploadExtention('<%= Me.hdnUpload.ClientID %>', 'AFTER', true, 'divAttachmentArea','Upload');



            var scrollTop = document.getElementById("hdnBodyScrollTop");
            if (scrollTop.value !== "") {
                document.getElementById("divContensbox").scrollTop = scrollTop.value;
                scrollTop.value = "";
            }
            /* テキストポップアップ表示設定 */
            setDisplayNameTip();

            /* タンク入力項目の制御 */
            var hdnTankInputAreaDisplayObj = document.getElementById('hdnTankInputAreaDisplay');
            var divTankInputBoxWrapperObj = document.getElementById('divTankInputBoxWrapper');
            if (hdnTankInputAreaDisplayObj !== null && divTankInputBoxWrapperObj !== null) {
                divTankInputBoxWrapperObj.style.display = hdnTankInputAreaDisplayObj.value;
                if (hdnTankInputAreaDisplayObj.value === 'block') {
                    commonDisableModalBg(divTankInputBoxWrapperObj.id);
                }
            }
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
    <form id="GBT00020A" runat="server">
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
                <div id="headerBox">
                <div id="actionButtonsBox" runat="server">
                    <asp:Label ID="lblOrderStart" runat="server" Text=""></asp:Label>
                    <asp:DropDownList ID="ddlOrderStart" runat="server"></asp:DropDownList>
                    <input id="btnCreateLeaseOrder" type="button" value="Create Lease Order" runat="server" />
                    <input id="btnReject" type="button" value="否認" runat="server" visible="false"  />
                    <span id="spnActButtonBox" runat="server" visible="true">
                        <input id="btnOutputExcel" type="button" value="エクセル出力" runat="server" visible="false" />
                        <input id="btnApply" type="button" value="申請" runat="server" />
                        <input id="btnSave" type="button" value="保存" runat="server" />
                        <input id="btnInputRequest" type="button" value="登録"  runat="server" visible="false" />
                        <input id="btnEntryCost" type="button" value="費用登録" runat="server" visible="false" />
                    </span>
                    <input id="btnBack" type="button" value="戻る"  runat="server"  />
                    <div id="btnFIRST" class="firstPage" runat="server" style="visibility:hidden;"></div>
                    <div id="btnLAST" class="lastPage" runat="server" style="visibility:hidden;"></div>
                </div>
                <div id="commonInfo" runat="server">
                    <ul style="margin-left:10px;list-style:none;">
                        <li>
                            <table class="itemTable">
                                <colgroup>
                                    <col /><col /><col /><col /><col />
                                    <col /><col /><col /><col /><col />
                                </colgroup>

							    <tr>
								    <th class="rowHeader"><asp:Label ID="lblBrInfoHeader" runat="server" Text="BR-info" CssClass="areaTitle"></asp:Label></th>
								    <td class="numHeader">NO</td>
								    <td><asp:Label ID="lblAgreementNo" runat="server" Text=""></asp:Label>
                                        <%= If(Me.lblAgreementNo.Text = "", "<span>(New)</span>", "") %>
								    </td>
								    <th></th>
								    <td></td>
								    <td colspan="4"></td>
                                    <td class="auto"></td>
							    </tr>
                                <tr>
					    		    <th></th>
					    		    <th><asp:Label ID="lblAppDate" runat="server" Text="DATE"></asp:Label></th>
								    <th><asp:Label ID="lblAppAgent" runat="server" Text="AGENT"></asp:Label></th>
								    <th><asp:Label ID="lblAppPic" runat="server" Text="PIC"></asp:Label></th>
								    <th></th>
								    <th style="text-decoration:underline;"><asp:Label ID="lblAppRemarksH" runat="server" Text="REMARKS"></asp:Label></th>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
					    	    <tr>
					    		    <th><asp:Label ID="lblApply" runat="server" Text="Apply"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtApplyDate" runat="server" Text="" Enabled="false"></asp:TextBox></td>
								    <td><asp:TextBox ID="txtApplyAgent" runat="server" Text="" Enabled="false"></asp:TextBox></td>
								    <td><asp:TextBox ID="txtApplyPic" runat="server" Text="" Enabled="false"></asp:TextBox></td>
								    <td><asp:Label ID="lblApplyPicText" runat="server" Text=""></asp:Label></td>
								    <td><span id="spnApplyRemarks" <%= If(Me.lblApplyRemarks.Enabled, "", "class=""aspNetDisabled""") %>>
                                                                   <%= If(Me.lblApplyRemarks.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                        <asp:Label ID="lblApplyRemarks" runat="server" Text=""></asp:Label>
                                        </span>
								    </td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>		
					    	    <tr>
					    		    <th><asp:Label ID="lblApproved" runat="server" Text="Approved"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtApprovedDate" runat="server" Text="" Enabled="false"></asp:TextBox></td>
								    <td>-</td>
								    <td><asp:TextBox ID="txtApprovedPic" runat="server" Text="" Enabled="false"></asp:TextBox></td>
								    <td><asp:Label ID="lblApprovedPicText" runat="server" Text=""></asp:Label></td>
								    <td><span id="spnAppJotRemarks" <%= If(Me.lblAppJotRemarks.Enabled, "", "class=""aspNetDisabled""") %>>
                                        <%= If(Me.lblAppJotRemarks.Text = "", "<span class=""remarksMessage"" title=""" & Me.hdnRemarkEmptyMessage.Value & """>&nbsp;</span>", "") %>
                                        <asp:Label ID="lblAppJotRemarks" runat="server" Text="" Enabled="False"></asp:Label>
                                        </span>
								    </td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th style="text-decoration:underline;" class="requiredMark2"><asp:Label ID="lblLeaseTerm" runat="server" Text="Lease Term"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtLeaseTerm" runat="server" Text=""></asp:TextBox></td>
					    		    <td><asp:Label ID="lblLeaseTermText" runat="server" Text=""></asp:Label></td>
								    <th class="textRight requiredMark2"><asp:Label ID="lblLeaseType" runat="server" CssClass="textRight" Text="Lease Type"></asp:Label></th>
                                    <td><asp:TextBox ID="txtLeaseType" runat="server" Text="" ></asp:TextBox></td>
                                    <td><asp:Label ID="lblLeaseTypeText" runat="server" Text=""></asp:Label></td>
								    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th style="text-decoration:underline;" class="requiredMark2"><asp:Label ID="lblProduct" runat="server" Text="PRODUCT"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtProduct" runat="server" Text=""></asp:TextBox></td>
					    		    <td><asp:Label ID="lblProductText" runat="server" Text=""></asp:Label></td>
								    <th class="textRight"><asp:Label ID="lblProductImdg" runat="server" Text="IMDG"></asp:Label></th>
                                    <td><asp:TextBox ID="txtImdg" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th class="textRight"><asp:Label ID="lblProductUnNo" runat="server" Text="UN No."></asp:Label></th>
								    <td><asp:TextBox ID="txtUnNo" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
								    <th class="requiredMark2"><asp:Label ID="lblPaymentMonth" runat="server" Text="Payment Month"></asp:Label></th>
                                    <td><asp:TextBox ID="txtPaymentMonth" runat="server" Text=""></asp:TextBox></td>
                                    <td><asp:Label ID="lblPaymentMonthText" runat="server" Text=""></asp:Label></td>
								    <th class="textRight requiredMark2"><asp:Label ID="lblAutoExtend" runat="server" Text="Auto Extend"></asp:Label></th>
                                    <td ><asp:TextBox ID="txtAutoExtend" runat="server" Text=""></asp:TextBox></td>
                                    <td><asp:Label ID="lblAutoExtendText" runat="server" Text=""></asp:Label></td>
					    		    <th class="textRight requiredMark2"><asp:Label ID="lblAutoExtendKind" runat="server" Text="Auto Extend Kind"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtAutoExtendKind" runat="server" Text=""></asp:TextBox></td>
					    		    <td><asp:Label ID="lblAutoExtendKindText" runat="server" Text=""></asp:Label></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
                                    <th><asp:Label ID="lblLeasePaymentKind" runat="server" Text="Kind" class="requiredMark2"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtLeasePaymentKind" runat="server" Text=""></asp:TextBox></td>
                                    <td><asp:Label ID="lblLeasePaymentKindText" runat="server" Text="" ></asp:Label></td>
					    		    <th class="textRight requiredMark2"><asp:Label ID="lblLeasePayments" runat="server" Text="Lease Payments"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtLeasePayments" runat="server" Text=""></asp:TextBox></td>
								    <th class="textRight"><asp:Label ID="lblReLease" runat="server" Text="Re-Lease"></asp:Label></th>
                                    <td><asp:TextBox ID="txtReLease" runat="server" Text=""></asp:TextBox></td>
                                    <th class="textRight requiredMark2"><asp:Label ID="lblCurrency" runat="server" Text="Currency"></asp:Label></th>
								    <td><asp:TextBox ID="txtCurrency" runat="server" Text="" class="requiredMark2"></asp:TextBox></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
								    <th><asp:Label ID="lblTax" runat="server" CssClass="requiredMark2" Text="Tax Kind"></asp:Label></th>
                                    <td><asp:TextBox ID="txtTax" runat="server"></asp:TextBox></td>
                                    <td><asp:Label ID="lblTaxText" runat="server" Text=""></asp:Label></td>
					    		    <th class="textRight"><asp:Label ID="lblTaxRate" runat="server" Text="Tax Rate"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtTaxRate" runat="server"></asp:TextBox></td>
								    <td></td>
                                    <td></td>
					    		    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th class="varticalTop"><asp:Label ID="lblRemarks" runat="server" Text="Remarks"></asp:Label></th>
					    		    <td colspan="8"><asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine"></asp:TextBox></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th class="varticalTop"><asp:Label ID="lblAttachment" runat="server" Text="Attachment"></asp:Label></th>
					    		    <td colspan="8">
                                        <div>
                                            <asp:Button ID="hdnUpload" runat="server" Text="Button" />
                                            <input id="btnDownloadFiles" type="button" value="File Download"  runat="server"  />
                                        </div>
                                        <div id="divAttachmentArea">
                                            <asp:HiddenField ID="hdnAttachmentHeaderFileName" runat="server" Value="FileName" />
                                            <asp:HiddenField ID="hdnAttachmentHeaderText" runat="server" Value="To register attached documents, drop it here" />
                                            <asp:HiddenField ID="hdnAttachmentHeaderDelete" runat="server" Value="Delete" />

                                            <table class="tblAttachmentHeader">
                                                <tr>
                                                    <th rowspan="2"><%= Me.hdnAttachmentHeaderFileName.Value %></th>
                                                    <th><%= Me.hdnAttachmentHeaderText.Value %></th>
                                                    <th rowspan="2"><%= Me.hdnAttachmentHeaderDelete.Value %></th>
                                                </tr>
                                                <tr>
                                                    <th>↓↓↓</th>
                                                </tr>
                                            </table>

                                            <asp:Repeater ID="repAttachment" runat="server">
                                                <HeaderTemplate>
                                                    <table  class="tblAttachment">
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr class="trAttachment" >
                                                        <td ondblclick='dispAttachmentFile("<%# Eval("FILENAME") %>");'><asp:Label ID="lblFileName" runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("FILENAME")) %>' CssClass="textLeft" Title='<%# Eval("FILENAME") %>'></asp:Label></td>
                                                        <td><asp:TextBox ID="txtDeleteFlg" runat="server" CssClass="textCenter" Text='<%# Eval("DELFLG") %>' Enabled='<%# IF(Me.hdnUpload.Enabled, "True", "False") %>'></asp:TextBox></td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </div>
					    		    </td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th class="varticalTop"><asp:Label ID="lblTankList" runat="server" Text="TankList"></asp:Label></th>
					    		    <td colspan="9">
                                        <div><input id="btnAddNewTank" type="button" value="Allocate" runat="server" /></div>
                                        <div id="divTankListArea">
                                            <asp:HiddenField ID="hdnListHeaderTank" runat="server" Value="Order" />
                                            <asp:HiddenField ID="hdnListHeaderDelButton" runat="server" Value="Delete" />
                                            <asp:HiddenField ID="hdnListHeaderTankNo" runat="server" Value="Tank No" />
                                            <asp:HiddenField ID="hdnListHeaderStatus" runat="server" Value="Status" />
                                            <asp:HiddenField ID="hdnListHeaderDepoOut" runat="server" Value="Depo Out" />
                                            <asp:HiddenField ID="hdnListHeaderStartDate" runat="server" Value="Start Date" />
                                            <asp:HiddenField ID="hdnListHeaderEndDateScr" runat="server" Value="End Date(Sche)" />
                                            <asp:HiddenField ID="hdnListHeaderCancel" runat="server" Value="Cancel" />
                                            <asp:HiddenField ID="hdnListHeaderEndDate" runat="server" Value="End Date" />
                                            <asp:HiddenField ID="hdnListHeaderDepoIn" runat="server" Value="Depo in" />
                                            <asp:HiddenField ID="hdnListHeaderSegSwStartDate" runat="server" Value="Seg Sw From" />
                                            <asp:HiddenField ID="hdnListHeaderSegSwEndDate" runat="server" Value="Seg Sw To" />
                                            <asp:HiddenField ID="hdnListHeaderRemarks" runat="server" Value="Remarks" />
                                            <asp:Repeater ID="repTankInfo" runat="server">
                                                <HeaderTemplate>
                                                    <table id="tblTankInfoList">
                                                        <tr>
                                                            <th class="toOrder"><%= Me.hdnListHeaderTank.Value %></th>
                                                            <th class="delButtonCol"><%= Me.hdnListHeaderDelButton.Value %></th>
                                                            <th class="tankNoCol"><%= Me.hdnListHeaderTankNo.Value %></th>
                                                            <th class="status"><%= Me.hdnListHeaderStatus.Value %></th>
                                                            <th class="depoNameCol"><%= Me.hdnListHeaderDepoOut.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderStartDate.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderEndDateScr.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderCancel.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderEndDate.Value %></th>
                                                            <th class="depoNameCol"><%= Me.hdnListHeaderDepoIn.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderSegSwStartDate.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderSegSwEndDate.Value %></th>
                                                            <th class="remarks"><%= Me.hdnListHeaderRemarks.Value %></th>
                                                            <th class="lineCnt">No.</th>
                                                        </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr ondblclick="ListDbClick(this,'<%# Eval("LINECNT") %>');">
                                                        <td><asp:CheckBox ID="chkToOrder" Checked='<%# If(Convert.ToString(Eval("TOORDER")) = "1", True, False) %>' runat="server" />
                                                        </td>
                                                        <td class="delButtonCol"><input id="btnDeleteRow<%# Eval("LINECNT") %>" type="button" value="Delete" onclick="deleteTank('<%# Eval("LINECNT") %>')" /></td>
                                                        <td title='<%# Eval("TANKNO") %>'        ><%# Eval("TANKNO") %></td>
                                                        <td title=''>&nbsp;</td>
                                                        <td title='<%# Eval("DEPOTOUTNAME") %>'  ><%# Eval("DEPOTOUTNAME") %></td>
                                                        <td title='<%# Eval("LEASESTYMD") %>'     ><%# BASEDLL.FormatDateContrySettings(Eval("LEASESTYMD"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("LEASEENDYMDSCR") %>'><%# BASEDLL.FormatDateContrySettings(Eval("LEASEENDYMDSCR"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("CANCELFLG") %>'     ><%# if(Convert.ToString(Eval("CANCELFLG")) = "1", "&#10003;", "") %></td>
                                                        <td title='<%# Eval("LEASEENDYMD") %>'    ><%# BASEDLL.FormatDateContrySettings(Eval("LEASEENDYMD"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("DEPOTINNAME") %>'   ><%# Eval("DEPOTINNAME") %></td>
                                                        <td title='<%# Eval("SEGSWSTYMD") %>'     ><%# BASEDLL.FormatDateContrySettings(Eval("SEGSWSTYMD"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("SEGSWENDYMD") %>'     ><%# BASEDLL.FormatDateContrySettings(Eval("SEGSWENDYMD"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("REMARK") %>'   ><%# Eval("REMARK") %></td>
                                                        <td class="lineCnt"><asp:Label ID="lblLineCnt" runat="server" Text='<%# Eval("LINECNT") %>'></asp:Label></td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </div>
					    		    </td>
                                    
					    	    </tr>
                            </table>
                        </li>
                    </ul>
                </div>
                <div class="emptybox"></div>
                </div>

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
                    <asp:HiddenField ID="hdnListDBclick" runat="server" Value="" />  <%--  ダブルクリックした行番号を記録 --%>   
                    <asp:HiddenField ID="hdnListCurrentRownum" runat="server" Value="" /> <%-- 一覧でボタンクリックイベントを発生させたRowNumを保持 --%>

                    <%-- 画面固有 --%>
                    <asp:HiddenField ID="hdnSelectedTabId" runat="server" Value="" /> <%-- 選択中のタブ --%>
                    <asp:HiddenField ID="hdnIsViewOnlyPopup" runat="server" Value="0" /> <%-- 参照のみのポップアップ表示か？ "1":ポップアップ表示,"0":それ以外 --%>

                    <asp:HiddenField ID="hdnProductIsHazard" runat="server" Value="" /> <%-- 積載品は危険物か？ "1"=危険物 それ以外=非危険品 --%>
                    <asp:HiddenField ID="hdnCanCalcHireageCommercialFactor" runat="server" Value="" Visible ="false" /> <%-- 売上総額よりJOT総額算出の自動計算を行うか1=行う それいがい=行わない、費用変更時→オーナータブ移動時に使用 --%>
                    <asp:HiddenField ID="hdnPrevTotalInvoicedValue" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnBackUrl" value="" runat="server" Visible="false" />
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
                    <%-- 添付ファイル一覧のファイルダブルクリック時のファイル名保持 --%>
                    <asp:HiddenField ID="hdnFileDisplay" Value="" runat="server" />
                    <%-- ドラッグアンドドロップ(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnUploadMessage01" Value="ファイルアップロード開始" runat="server" />
                    <asp:HiddenField ID="hdnUploadError01" Value="ファイルアップロードが失敗しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError02" Value="通信を中止しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError03" Value="タイムアウトエラーが発生しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError04" Value="更新権限がありません。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError05" Value="対応外のファイル形式です。" runat="server" />
                    <%-- 当画面の計算処理POST(設定した名称の関数を実行) --%>
                    <asp:HiddenField ID="hdnCalcFunctionName" Value="" runat="server" />
                    <asp:HiddenField ID="hdnThisMapVariant" Value="" runat="server" />
                    <asp:HiddenField ID="hdnTankInputAreaDisplay" Value="none" runat="server" />
                    <%-- 当画面の申請ステータス状況 --%>
                    <asp:HiddenField ID="hdnApplyStatus" Value="" runat="server" />

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
                    <%-- リースTERM選択 VIEW　 --%>
                    <asp:View ID="vLeftLeaseTerm" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLeaseTerm" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リースTERM選択 VIEW　 --%>
                    <%-- リース会計 VIEW　 --%>
                    <asp:View ID="vLeftLeaseType" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLeaseType" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リース会計 VIEW　 --%>
                    <%-- リース支払月 VIEW　 --%>
                    <asp:View ID="vLeftPaymentMonth" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbPaymentMonth" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リース支払月 VIEW　 --%>
                    <%-- リース支払種類 VIEW　 --%>
                    <asp:View ID="vLeftLeasePaymentKind" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLeasePaymentKind" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リース支払種類 VIEW　 --%>
                    <%-- リース通貨 VIEW　 --%>
                    <asp:View ID="vLeftLeaseCurrency" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLeaseCurrency" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リース通貨 VIEW　 --%>
                    <%-- 税区分 VIEW　 --%>
                    <asp:View ID="vLeftTax" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTax" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 税区分 VIEW　 --%>
                    <%-- Yes/No(自動延長) VIEW　 --%>
                    <asp:View ID="View1" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="ListBox1" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Yes/No(自動延長) VIEW　 --%>
                    <%-- 自動延長種別 VIEW　 --%>
                    <asp:View ID="vLeftAutoExtendKind" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbAutoExtendKind" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 自動延長種別 VIEW　 --%>
                    <%-- 積載品 VIEW　 --%>
                    <asp:View ID="vLeftProduct" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbProduct" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 積載品 VIEW　 --%>
                    <%-- デポ VIEW　 --%>
                    <asp:View ID="vLeftDepot" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDepot" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END デポ VIEW　 --%>
                    <%-- Yes/No(自動延長) VIEW　 --%>
                    <asp:View ID="vLeftYesNo" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbYesNo" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Yes/No(自動延長) VIEW　 --%>
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
            <%-- タンク入力 --%>
            <div id="divTankInputBoxWrapper" runat="server">
                <div id="divTankInputBox">
                    <div id="divTankInputBoxInputitle">
                        <div id="divTankInputBoxButtons">
                            <input id="btnTankInputOk" type="button" value="OK" runat="server" />
                            <input id="btnTankInputCancel" type="button" value="CANCEL" runat="server" onclick="commonCloseModal('divTankInputBoxWrapper');document.getElementById('divTankInputBoxWrapper').style.display='none';document.getElementById('hdnTankInputAreaDisplay').value='none';" />
                        </div>
                        <span>Tank Info</span>
                        <asp:HiddenField ID="hdnPopUpLineCnt" runat="server" />
                    </div>

                    <div id="divTankInputArea">
                        <table class="tankInput">
                            <colgroup>
                                <col /><col /><col /><col /><col />
                            </colgroup>
                            <tr>
                                <th>
                                    <asp:Label ID="lblTankNo" runat="server" Text="Tank No"></asp:Label>
                                </th>
                                <td colspan="3">
                                    <asp:TextBox ID="txtTankNo" runat="server" Enabled="false"></asp:TextBox>
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <th>
                                    <asp:Label ID="lblDepoOut" runat="server" Text="Depo Out"></asp:Label>
                                </th>
                                <td>
                                    <asp:TextBox ID="txtDepoOut" runat="server" Enabled="false"></asp:TextBox>
                                </td>
                                <td colspan="2" title="<%= Me.lblDepoOutText.Text %>">
                                    <asp:Label ID="lblDepoOutText" runat="server" Text=""></asp:Label>
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <th><asp:Label ID="lblPayStDaily" runat="server" Text="Daily Payment(Start)"></asp:Label></th>
                                <td class="chkArea">
                                    <asp:CheckBox ID="chkPayStDaily" runat="server" />
                                </td>
                                <th><asp:Label ID="lblPayEndDaily" runat="server" Text="(End)"></asp:Label></th>
                                <td class="chkArea">
                                    <asp:CheckBox ID="chkPayEndDaily" runat="server" />
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <th>
                                    <asp:Label ID="lblStartDate" runat="server" Text="Start Date"></asp:Label>
                                </th>
                                <td>
                                    <asp:TextBox ID="txtStartDate" runat="server"></asp:TextBox>
                                </td>
                                <th>
                                    <asp:Label ID="lblEndDateSche" runat="server" Text="End Date(Sche)"></asp:Label>
                                </th>
                                <td>
                                    <asp:TextBox ID="txtEndDateSche" runat="server"></asp:TextBox>
                                </td>
                                <td>

                                </td>
                            </tr>
                            <tr>
                                <th class="varticalTop">
                                    <asp:Label ID="lblTankRemarks" runat="server" Text="Remarks"></asp:Label>
                                </th>
                                <td colspan="3">
                                    <asp:TextBox ID="txtTankRemarks" runat="server" TextMode="MultiLine"></asp:TextBox>
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <th>
                                    <asp:Label ID="lblCancelDate" runat="server" Text="Cancel"></asp:Label>
                                </th>
                                <td class="chkArea">
                                    <asp:TextBox ID="txtCancelDate" runat="server" Visible="false"></asp:TextBox>
                                    <asp:CheckBox ID="chkCancel" runat="server" />
                                </td>
                                <td colspan="2"></td>
                                <td></td>
                            </tr>
                            <tr>
                                <th>
                                    <asp:Label ID="lblEndDate" runat="server" Text="End Date"></asp:Label>
                                </th>
                                <td>
                                    <asp:TextBox ID="txtEndDate" runat="server"></asp:TextBox>
                                </td>
                                <td colspan="2"></td>
                                <td></td>
                            </tr>
                            <tr>
                                <th>
                                    <asp:Label ID="lblDepoIn" runat="server" Text="Depo In"></asp:Label>
                                </th>
                                <td>
                                    <asp:TextBox ID="txtDepoIn" runat="server"></asp:TextBox>
                                </td>
                                <td colspan="2" title="<%= Me.lblDepoInText.Text %>">
                                    <asp:Label ID="lblDepoInText" runat="server" Text=""></asp:Label>
                                </td>
                                <td></td>
                            </tr>
                            <tr>
                                <th>
                                    <asp:Label ID="lblSegSwStartDate" runat="server" Text="Seg Switching Date"></asp:Label>
                                </th>
                                <td>
                                    <asp:TextBox ID="txtSegSwStartDate" runat="server"></asp:TextBox>
                                </td>
                                <th>
                                    <asp:Label ID="lblSegSwEndDate" runat="server" Text="～"></asp:Label>
                                </th>
                                <td>
                                    <asp:TextBox ID="txtSegSwEndDate" runat="server"></asp:TextBox>
                                </td>
                                <td>

                                </td>
                            </tr>

                        </table>
                    </div>
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

