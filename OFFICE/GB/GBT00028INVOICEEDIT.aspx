<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00028INVOICEEDIT.aspx.vb" Inherits="OFFICE.GBT00028INVOICEEDIT" %>

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
    <link href="~/GB/css/GBT00028INVOICEEDIT.css" rel="stylesheet" type="text/css" />
    <style>
    </style>
<%--    <!-- Global site tag (gtag.js) - Google Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=UA-162522994-1"></script>
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
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00028INVOICEEDIT.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加
        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            //changeCommonInfoArea();
            /* ボタンクリックイベントのバインド(適宜追加) */
<%--                                       '<%= Me.btnDownloadFiles.ClientId %>',--%>
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>', '<%= Me.btnOutputExcel.ClientId %>',
                                       '<%= Me.btnOutput.ClientId %>', '<%= Me.btnSave.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';                      /* 年月日 */
            var viewPOLId = '<%= Me.vLeftPOL.ClientID %>';                      /* POL */
            var viewPODId = '<%= Me.vLeftPOD.ClientID %>';                      /* POD */
            var viewProductId = '<%= Me.vLeftProduct.ClientID %>';              /* PRODUCT */
            var viewLanguageId = '<%= Me.vLeftLanguage.ClientID %>';            /* Language */

            var dblClickObjects = [
                                   ['<%= Me.txtPOL.ClientID %>', viewPOLId],
                                   ['<%= Me.txtPOD.ClientID %>', viewPODId],
                                   ['<%= Me.txtProduct.ClientID %>', viewProductId],
                                   ['<%= Me.txtlang.ClientID %>', viewLanguageId],
                                   ['<%= Me.txtIssueDate.ClientID %>', viewCalId]
            ];

            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [
                                           ['<%= Me.lbPOL.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPOD.ClientID %>', '3', '1'],
                                           ['<%= Me.lbProduct.ClientID %>', '3', '1'],
                                           ['<%= Me.lbLanguage.ClientID %>', '3', '1'],
                                          ];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = []
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

            /* 検索ボックス生成 */
            //commonCreateSearchArea('selectHeaderBox',3);
            commonCreateSearchArea('searchCondition',3);

            /* テキストポップアップ表示設定 */
            setDisplayNameTip();

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
    <form id="GBT00028L" runat="server">
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
                        <input id="btnExtract" type="button" value="絞り込み"  runat="server"  />
                        <input id="btnOutputExcel" type="button" value="Excel出力" runat="server" />
                        <input id="btnOutput" type="button" value="出力" runat="server" />
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
                                    <col /><col /><col /><col /><col />
                                </colgroup>

                                <tr id="trInvoiceInfoRow1" runat="server">
                                    <th><asp:Label ID="lblInvoiceNo" runat="server" Text="請求書番号"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtInvoiceNo" runat="server" Text="" Enabled="false"></asp:TextBox></td>
					    		    <td><asp:TextBox ID="txtInvoiceNoSub" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th class="textRight"><asp:Label ID="lblConditionsInfo" runat="server" Text="お支払い条件"></asp:Label></th>
								    <th class="textRight"><asp:Label ID="lblPaymentDate" runat="server" Text="お支払い日"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtPaymentDate" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th><asp:Label ID="lbllang" runat="server" Text="言語"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtlang" runat="server" Text="" Enabled="true"></asp:TextBox></td>
                                    <td><asp:Label ID="lbllangText" runat="server" Text=""></asp:Label></td>
                                    <td></td>
                                </tr>
                                <tr id="trInvoiceInfoRow2" runat="server">
                                    <th><asp:Label ID="lblInvoicePostNo" runat="server" Text="請求先郵便番号"></asp:Label></th>
					    		    <td colspan="2"><asp:TextBox ID="txtInvoicePostNo" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th colspan="2" class="textRight"><asp:Label ID="lblPaymentType" runat="server" Text="お支払方法"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtPaymentType" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th><asp:Label ID="lblIssueDate" runat="server" Text="請求書発行年月日"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtIssueDate" runat="server" Text="" Enabled="true"></asp:TextBox></td>
                                    <td></td>
                                    <td></td>
                                </tr>
                                <tr id="trInvoiceInfoRow3" runat="server">
                                    <th><asp:Label ID="lblInvoiceAddress" runat="server" Text="請求先住所"></asp:Label></th>
					    		    <td colspan="2"><asp:TextBox ID="txtInvoiceAddress1" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th colspan="2" class="textRight"><asp:Label ID="lblBank" runat="server" Text="振込銀行"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtBank" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th><asp:Label ID="lblInvoiceType" runat="server" Text="出力タイプ"></asp:Label></th>

					    		    <td colspan="3"><asp:RadioButtonList ID="rblInvoiceTyp" runat="server" RepeatLayout="UnorderedList">
                                            <asp:ListItem Text="DRAFT" Value="DRAFT"></asp:ListItem>
                                            <asp:ListItem Text="ORIGINAL" Value="ORIGINAL"></asp:ListItem>
					    		        </asp:RadioButtonList></td>
<%--                                    <td></td>
                                    <td></td>--%>
                                    <td></td>
                                    <td></td>
                                </tr>
                                <tr id="trInvoiceInfoRow4" runat="server">
                                    <td></td>
					    		    <td colspan="2"><asp:TextBox ID="txtInvoiceAddress2" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th colspan="2" class="textRight"><asp:Label ID="lblDepositItem" runat="server" Text="預金種目"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtDepositItem" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th class="textRight"><asp:Label ID="lblOutCntDraft" runat="server" Text="ドラフト版出力数"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtOutCntDraft" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
                                <tr id="trInvoiceInfoRow5" runat="server">
                                    <th><asp:Label ID="lblInvoiceName" runat="server" Text="請求先名称"></asp:Label></th>
					    		    <td colspan="2"><asp:TextBox ID="txtInvoiceName1" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th colspan="2" class="textRight"><asp:Label ID="lblAccountNo" runat="server" Text="口座番号"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtAccountNo" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th class="textRight"><asp:Label ID="lblOutCntOriginal" runat="server" Text="本紙版出力数"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtOutCntOriginal" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
                                <tr id="trInvoiceInfoRow6" runat="server">
                                    <td></td>
					    		    <td colspan="2"><asp:TextBox ID="txtInvoiceName2" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th colspan="2" class="textRight"><asp:Label ID="lblAccountName" runat="server" Text="口座名"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtAccountName" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
                                <tr id="trInvoiceInfoRow7" runat="server">
                                    <td></td>
					    		    <td colspan="2"><asp:TextBox ID="txtInvoiceName3" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <th colspan="2" class="textRight"><asp:Label ID="lblCurrency" runat="server" Text="通貨"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtCurrency" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
					    	    <tr id="trInvoiceRemarks" runat="server">
					    		    <th class="varticalTop"><asp:Label ID="lblRemarks" runat="server" Text="Remarks"></asp:Label></th>
					    		    <td colspan="8"><asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine"></asp:TextBox></td>
                                    <td></td>
					    	    </tr>
					    	    <tr id="trBlank1" runat="server">
                                   <td>&nbsp;</td>
 					    	    </tr>
                                <tr id="trInvoiceTankInfo" runat="server">
					    		    <th class="varticalTop"><asp:Label ID="lblTankList" runat="server" Text="TankList"></asp:Label></th>
                                    <td></td>
                                    <td></td>
                                    <th colspan="2" class="textRight"><asp:Label ID="lblTotal" runat="server" Text="請求額"></asp:Label></th>
					    		    <td><asp:TextBox ID="txtTotal" runat="server" Text="" Enabled="false"></asp:TextBox></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
					    	    <tr id="trSearch" runat="server">
                                   <td></td>
                                    <td colspan="8">
                                        <div id="searchCondition" runat="server">
                                        </div>
                                        <div id="divSearchConditionBox">
                                            <asp:HiddenField ID="hdnSearchConditionDetailOpenFlg" runat="server" Value="" /> 
                                            <span>
                                                <asp:Label ID="lblPOL" runat="server" Text="POL"></asp:Label>
                                                <asp:TextBox ID="txtPOL" runat="server" Text=""></asp:TextBox>
                                                <asp:Label ID="lblPOLText" runat="server" Text=""></asp:Label>
                                            </span> 
                                            <span>
                                                <asp:Label ID="lblPOD" runat="server" Text="POD"></asp:Label>
                                                <asp:TextBox ID="txtPOD" runat="server" Text=""></asp:TextBox>
                                                <asp:Label ID="lblPODText" runat="server" Text=""></asp:Label>
                                            </span>
                                            <span>
                                                <asp:Label ID="lblProduct" runat="server" Text="Product"></asp:Label>
                                                <asp:TextBox ID="txtProduct" runat="server" Text=""></asp:TextBox>
                                                <asp:Label ID="lblProductText" runat="server" Text=""></asp:Label>
                                            </span>
                                        </div>
                                   </td>
                                   <td></td>
 					    	    </tr>
					    	    <tr id="trInvoiceTankList" runat="server" >
					    		    <%--<th class="varticalTop"><asp:Label ID="lblTankList" runat="server" Text="TankList"></asp:Label></th>--%>
                                    <td></td>
 					    		    <td colspan="9" id="tdInvoiceTankList">
                                        <div id="divTankListArea">
                                            <asp:HiddenField ID="hdnListHeaderCheck" runat="server" Value="発行有無" />
                                            <asp:HiddenField ID="hdnListHeaderNo" runat="server" Value="No" />
                                            <asp:HiddenField ID="hdnListHeaderOrder" runat="server" Value="ORDERNO" />
                                            <asp:HiddenField ID="hdnListHeaderTankNo" runat="server" Value="TANKNO" />
                                            <asp:HiddenField ID="hdnListHeaderBlId" runat="server" Value="BLID" />
                                            <asp:HiddenField ID="hdnListHeaderTermType" runat="server" Value="TERM TYPE" />
                                            <asp:HiddenField ID="hdnListHeaderPOL" runat="server" Value="POL" />
                                            <asp:HiddenField ID="hdnListHeaderPOD" runat="server" Value="POD" />
                                            <asp:HiddenField ID="hdnListHeaderProduct" runat="server" Value="PRODUCT NAME" />
                                            <asp:HiddenField ID="hdnListHeaderLoadDate" runat="server" Value="LOAD" />
                                            <asp:HiddenField ID="hdnListHeaderETD" runat="server" Value="ETD" />
                                            <asp:HiddenField ID="hdnListHeaderETA" runat="server" Value="ETA" />
                                            <asp:HiddenField ID="hdnListHeaderShipDate" runat="server" Value="SHIP" />
                                            <asp:HiddenField ID="hdnListHeaderArvdDate" runat="server" Value="ARVD" />
                                            <asp:HiddenField ID="hdnListHeaderAmount" runat="server" Value="AMOUNT FIX" />
                                            <asp:HiddenField ID="hdnListHeaderBRID" runat="server" Value="BRID" />
                                            <asp:HiddenField ID="hdnListHeaderCustomer" runat="server" Value="CUSTOMER" />
                                            <asp:Repeater ID="repTankInfo" runat="server">
                                                <HeaderTemplate>
                                                    <table id="tblTankInfoList">
                                                        <tr>
                                                            <th class="shortCol"><%= Me.hdnListHeaderCheck.Value %>
                                                                <asp:CheckBox ID="chkAllSelect" Checked='<%# If(Convert.ToString(Me.hdnAllSelectCheckValue.Value) = "TRUE", True, False) %>' runat="server" Enabled='<%# If(Convert.ToString(Me.txtInvoiceNo.Text) = "", True, False) %>' onclick="f_checkAllSelectEvent(event)"/>
                                                            </th>
                                                            <th class="shortCol"><%= Me.hdnListHeaderNo.Value %></th>
                                                            <th class="idCol"><%= Me.hdnListHeaderOrder.Value %></th>
                                                            <th class="idCol"><%= Me.hdnListHeaderTankNo.Value %></th>
                                                            <th class="idCol"><%= Me.hdnListHeaderBlId.Value %></th>
                                                            <th class="idCol"><%= Me.hdnListHeaderTermType.Value %></th>
                                                            <th class="shortCol"><%= Me.hdnListHeaderPOL.Value %></th>
                                                            <th class="shortCol"><%= Me.hdnListHeaderPOD.Value %></th>
                                                            <th class="nameCol"><%= Me.hdnListHeaderProduct.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderLoadDate.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderETD.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderETA.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderShipDate.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderArvdDate.Value %></th>
                                                            <th class="dateCol"><%= Me.hdnListHeaderAmount.Value %></th>
                                                            <th class="idCol"><%= Me.hdnListHeaderBRID.Value %></th>
                                                            <th class="idCol"><%= Me.hdnListHeaderCustomer.Value %></th>
                                                            <th class="lineCnt">No.</th>
                                                        </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr ondblclick="ListDbClick(this,'<%# Eval("ORDERNO") %>');">
                                                        <td><asp:CheckBox ID="chkToInvoice" Checked='<%# If(Convert.ToString(Eval("TOINVOICE")) = "1", True, False) %>' runat="server" Enabled='<%# If(Convert.ToString(Eval("INVOICENO")) = "", True, False) %>' onclick="f_checkEvent(this)"/>
                                                        </td>
                                                        <%--<td title=''>&nbsp;</td>--%>
                                                        <td title='<%# Eval("LINECNT") %>'  ><%# Eval("LINECNT") %></td>
                                                        <td title='<%# Eval("ORDERNO") %>'  ><%# Eval("ORDERNO") %></td>
                                                        <td title='<%# Eval("TANKNO") %>'  ><%# Eval("TANKNO") %></td>
                                                        <td title='<%# Eval("BLID") %>'  ><%# Eval("BLID") %></td>
                                                        <td title='<%# Eval("TERMTYPE") %>'  ><%# Eval("TERMTYPE") %></td>
                                                        <td title='<%# Eval("POL") %>'  ><%# Eval("POL") %></td>
                                                        <td title='<%# Eval("POD") %>'  ><%# Eval("POD") %></td>
                                                        <td title='<%# Eval("PRODUCTNAME") %>'  ><%# Eval("PRODUCTNAME") %></td>
                                                        <td title='<%# Eval("LOADDATE") %>'     ><%# BASEDLL.FormatDateContrySettings(Eval("LOADDATE"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("ETD") %>'     ><%# BASEDLL.FormatDateContrySettings(Eval("ETD"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("ETA") %>'     ><%# BASEDLL.FormatDateContrySettings(Eval("ETA"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("SHIPDATE") %>'     ><%# BASEDLL.FormatDateContrySettings(Eval("SHIPDATE"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("ARVDDATE") %>'     ><%# BASEDLL.FormatDateContrySettings(Eval("ARVDDATE"), OFFICE.GBA00003UserSetting.DATEFORMAT) %></td>
                                                        <td title='<%# Eval("AMOUNT") %>'   ><%# Eval("AMOUNT") %></td>
                                                        <td title='<%# Eval("BRID") %>'  ><%# Eval("BRID") %></td>
                                                        <td title='<%# Eval("CUSTOMER") %>'  ><%# Eval("CUSTOMER") %></td>
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
                    <asp:HiddenField ID="hdnAllSelectCheckValue" runat="server" Value="FALSE" /> <%-- 全チェック欄の値を保持 --%>
                    <asp:HiddenField ID="hdnAllSelectCheckChange" runat="server" Value="FALSE" /> <%-- 全チェックのイベントを保持 --%>

                    <%-- 画面固有 --%>
                    <asp:HiddenField ID="hdnPrintType" value="PDF" runat="server" />
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
                    <%--  POL --%>
                    <asp:View id="vLeftPOL" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbPOL" runat="server" CssProduct="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END POL VIEW --%>
                    <%--  POD --%>
                    <asp:View id="vLeftPOD" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbPOD" runat="server" CssProduct="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END POD VIEW --%>
                    <%--  Product --%>
                    <asp:View id="vLeftProduct" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbProduct" runat="server" CssProduct="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Product VIEW --%>
                    <%--  Language --%>
                    <asp:View id="vLeftLanguage" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLanguage" runat="server" CssProduct="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Language VIEW --%>
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

