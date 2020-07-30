<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00020LEASE.aspx.vb" Inherits="OFFICE.GBT00020LEASE" %>

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
    <link href="~/GB/css/GBT00020LEASE.css" rel="stylesheet" type="text/css" />
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
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00020LEASE.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">
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
        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            //changeCommonInfoArea();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>', '<%= Me.btnOutputExcel.ClientId %>',
                                       '<%= Me.btnSave.ClientId  %>','<%= Me.btnDownloadFiles.ClientId %>'];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';
            var viewShipper = '<%= Me.vLeftShipper.ClientID %>';
            var viewLeasePaymentType = '<%= me.vLeftLeasePaymentType.ClientID %>';
            var viewLeasePaymentKind = '<%= me.vLeftLeasePaymentKind.ClientID %>';
            var viewLeaseAccount = '<%= me.vLeftLeaseAccount.ClientID %>';
            var viewTax = '<%= me.vLeftTax.ClientID %>';
            var viewYesNo = '<%= me.vLeftYesNo.ClientID %>';
            var dblClickObjects = [['<%= Me.txtShipper.ClientID %>', viewShipper],
                                   ['<%= Me.txtLeaseFrom.ClientID %>', viewCalId],
                                   ['<%= Me.txtLeasePaymentType.ClientID %>', viewLeasePaymentType],
                                   ['<%= Me.txtLeasePaymentKind.ClientID %>', viewLeasePaymentKind],
                                   ['<%= Me.txtLeaseAccount.ClientID %>', viewLeaseAccount],
                                   ['<%= Me.txtTax.ClientID %>', viewTax],
                                   ['<%= Me.txtAutoExtend.ClientID %>', viewYesNo]
                                  ];
            var txtAttachmentDelFlgObjects = document.querySelectorAll('input[id^="repAttachment_txtDeleteFlg_"');
            for (let i = 0; i < txtAttachmentDelFlgObjects.length; i++) {
                dblClickObjects.push([txtAttachmentDelFlgObjects[i].id, viewYesNo]);
            }
            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();
            /* 費用追加ボタンのイベントバインド */
            //bindAddCostOnClick();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbShipper.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPort.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = ['<%= Me.txtShipper.ClientID %>',  '<%= Me.txtLeasePaymentType.ClientID %>',
                                         '<%= Me.txtLeasePaymentKind.ClientID %>','<%= Me.txtAutoExtend.ClientID %>',
                                         '<%= Me.txtLeaseAccount.ClientID %>','<%= Me.txtTax.ClientID %>'];
            
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

            var scrollTop = document.getElementById("hdnBodyScrollTop");
            if (scrollTop.value !== "") {
                document.getElementById("divContensbox").scrollTop = scrollTop.value;
                scrollTop.value = "";
            }
            /* テキストポップアップ表示設定 */
            setDisplayNameTip();
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
    <form id="GBT00020L" runat="server">
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
                    <span id="spnActButtonBox" runat="server" visible="true">
                        <input id="btnOutputExcel" type="button" value="エクセル出力" runat="server" visible="false"  />
                        <input id="btnSave" type="button" value="保存" runat="server" />
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
                                    <col /><col /><col /><col />
                                </colgroup>
							    <tr>
								    <th class="rowHeader"><asp:Label ID="lblBrInfoHeader" runat="server" Text="BR-info" CssClass="areaTitle"></asp:Label></th>
								    <td class="numHeader">NO</td>
								    <td><asp:Label ID="lblBrNo" runat="server" Text=""></asp:Label>
                                        <%= If(Me.lblBrNo.Text = "", "<span>(New)</span>", "") %>
								    </td>
								    <th><asp:Label ID="lblContractPerson" runat="server" Text="Contract Person" CssClass="contractPerson"></asp:Label></th>
								    <td><asp:Label ID="lblContractPersonName" runat="server" Text="" CssClass="contractPersonName"></asp:Label></td>
								    <td colspan="3"></td>
                                    <td class="auto"></td>
							    </tr>
					    	    <tr>
					    		    <th class="requiredMark2"><asp:Label ID="lblShipper" runat="server" Text="SHIPPER"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtShipper" runat="server" Text=""></asp:TextBox></td>
					    		    <td><asp:Label ID="lblShipperText" runat="server" Text=""></asp:Label></td>
					    		    <th><asp:Label ID="lblShipperTel" runat="server" Text="TEL"></asp:Label></th>
								    <td><asp:Label ID="lblShipperTelText" runat="server" Text=""></asp:Label></td>
                                    <th><asp:Label ID="lblShipperAddress" runat="server" Text="ADDRESS"></asp:Label></th>
								    <td><asp:Label ID="lblShipperAddressText" runat="server" Text=""></asp:Label></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th class="requiredMark2"><asp:Label ID="lblLeasePriod" runat="server" Text="LEASE PERIOD"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtLeaseFrom" runat="server" Text=""></asp:TextBox></td>
					    		    <th><asp:Label ID="lblEnabled" runat="server" Text="Enabled"></asp:Label></th>
					    		    <td><asp:CheckBox ID="chkEnabled" runat="server" /></td>
                                    <td></td>
                                    <th><asp:Label ID="lblAccSegment" runat="server" Text="Segment"></asp:Label></th>
                                    <td><asp:TextBox ID="txtAccSegment" runat="server" Text=""></asp:TextBox></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
                                    <th><asp:Label ID="lblLeasePaymentType" runat="server" Text="Payment Month"></asp:Label></th>
                                    <td><asp:TextBox ID="txtLeasePaymentType" runat="server"></asp:TextBox></td>
                                    <td><asp:Label ID="lblLeasePaymentTypeText" runat="server" Text=""></asp:Label></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th><asp:Label ID="lblLeasePaymentKind" runat="server" Text="Kind"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtLeasePaymentKind" runat="server"></asp:TextBox></td>
								    <td><asp:Label ID="lblLeasePaymentKindText" runat="server" Text=""></asp:Label></td>
                                    <th><asp:Label ID="lblAutoExtend" runat="server" Text="Auto Extend"></asp:Label></th>
                                    <td><asp:TextBox ID="txtAutoExtend" runat="server"></asp:TextBox></td>
                                    <td><asp:Label ID="lblAutoExtendText" runat="server" Text=""></asp:Label></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th class="requiredMark2"><asp:Label ID="lblLeaseAccount" runat="server" Text="Account"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtLeaseAccount" runat="server"></asp:TextBox></td>
								    <td><asp:Label ID="lblLeaseAccountText" runat="server" Text=""></asp:Label></td>
                                    <th><asp:Label ID="lblTax" runat="server" Text="Tax Kind"></asp:Label></th>
                                    <td><asp:TextBox ID="txtTax" runat="server"></asp:TextBox></td>
                                    <td><asp:Label ID="lblTaxText" runat="server" Text=""></asp:Label></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
                                    <th class="requiredMark2"><asp:Label ID="lblNoOfAgreement" runat="server" Text="No Of Agreement"></asp:Label></th>
                                    <td><asp:TextBox ID="txtNoOfAgreement" runat="server"></asp:TextBox></td>
								    <td></td>
                                    <td></td>
								    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th class="varticalTop"><asp:Label ID="lblRemarks" runat="server" Text="Remarks"></asp:Label></th>
					    		    <td colspan="7"><asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine"></asp:TextBox></td>
                                    <td></td>
					    	    </tr>
					    	    <tr>
					    		    <th class="varticalTop"><asp:Label ID="lblAttachment" runat="server" Text="Attachment"></asp:Label></th>
					    		    <td colspan="7">
                                        <div>
                                            <asp:Button ID="hdnUpload" runat="server" Text="" />
                                            <input id="btnDownloadFiles" type="button" value="File Download"  runat="server"  />
                                        </div>
                                        <div id="divAttachmentArea" runat="server">
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
                                                    <table class="tblAttachment">
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr class="trAttachment" >
                                                        <td ondblclick='dispAttachmentFile("<%# Eval("FILENAME") %>");'><asp:Label ID="lblFileName" runat="server" Text='<%# HttpUtility.HtmlEncode(Eval("FILENAME")) %>' CssClass="textLeft" Title='<%# Eval("FILENAME") %>'></asp:Label></td>
                                                        <td><asp:TextBox ID="txtDeleteFlg" runat="server" CssClass="textCenter" Text='<%# Eval("DELFLG") %>' Enabled='<%# IF(Me.hdnUpload.Enabled, "True", "False") %>'></asp:TextBox>
                                                        </td>
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
                                <tr style="display:none;">
					    		    <th></th>
					    		    <th><asp:Label ID="lblAppDate" runat="server" Text="DATE"></asp:Label></th>
								    <th><asp:Label ID="lblAppAgent" runat="server" Text="AGENT"></asp:Label></th>
								    <th><asp:Label ID="lblAppPic" runat="server" Text="PIC"></asp:Label></th>
								    <th></th>
								    <th style="text-decoration:underline;"><asp:Label ID="lblAppRemarks" runat="server" Text="REMARKS"></asp:Label></th>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
					    	    <tr style="display:none;">
					    		    <th><asp:Label ID="lblApply" runat="server" Text="Apply"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtApplyDate" runat="server" Text=""></asp:TextBox></td>
								    <td><asp:TextBox ID="txtApplyAgent" runat="server" Text=""></asp:TextBox></td>
								    <td><asp:TextBox ID="txtApplyPic" runat="server" Text=""></asp:TextBox></td>
								    <td><asp:Label ID="lblApplyPicText" runat="server" Text=""></asp:Label></td>
								    <td><span class="remarksMessage" >&nbsp;</span></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>		
					    	    <tr style="display:none;">
					    		    <th><asp:Label ID="lblApproved" runat="server" Text=""></asp:Label></th>
					    		    <td><asp:TextBox ID="txtApprovedDate" runat="server" Text=""></asp:TextBox></td>
								    <td>-</td>
								    <td><asp:TextBox ID="txtApprovedPic" runat="server" Text=""></asp:TextBox></td>
								    <td><asp:Label ID="lblApprovedPicText" runat="server" Text=""></asp:Label></td>
								    <td><span class="remarksMessage" >&nbsp;</span></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
					    	    </tr>
                            </table>
                        </li>
                    </ul>
                </div>
                <div class="emptybox"></div>
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
                    <%-- ドラッグアンドドロップ(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnUploadMessage01" Value="ファイルアップロード開始" runat="server" />
                    <asp:HiddenField ID="hdnUploadError01" Value="ファイルアップロードが失敗しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError02" Value="通信を中止しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError03" Value="タイムアウトエラーが発生しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError04" Value="更新権限がありません。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError05" Value="対応外のファイル形式です。" runat="server" />
                    <%-- 添付ファイル一覧のファイルダブルクリック時のファイル名保持 --%>
                    <asp:HiddenField ID="hdnFileDisplay" Value="" runat="server" />
                    
                    <%-- 当画面の計算処理POST(設定した名称の関数を実行) --%>
                    <asp:HiddenField ID="hdnThisMapVariant" Value="" runat="server" />
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
                    <%-- リース支払月 VIEW　 --%>
                    <asp:View ID="vLeftLeasePaymentType" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLeasePaymentType" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リース支払月 VIEW　 --%>
                    <%-- リース支払種類 VIEW　 --%>
                    <asp:View ID="vLeftLeasePaymentKind" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLeasePaymentKind" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リース支払種類 VIEW　 --%>
                    <%-- 口座 VIEW　 --%>
                    <asp:View ID="vLeftLeaseAccount" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLeaseAccount" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 口座 VIEW　 --%>
                    <%-- 税区分 VIEW　 --%>
                    <asp:View ID="vLeftTax" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTax" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 税区分 VIEW　 --%>
                    <%-- Yes/No(自動延長) VIEW　 --%>
                    <asp:View ID="vLeftYesNo" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbYesNo" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Yes/No(自動延長) VIEW　 --%>
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
        </div>
    </form>
</body>
</html>

