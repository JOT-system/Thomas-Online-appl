<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00005SELECT.aspx.vb" Inherits="OFFICE.GBT00005SELECT" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <asp:PlaceHolder ID="phCommonHeader" runat="server"></asp:PlaceHolder>
    <%--フォームのID以外でタイトルを設定する場合は適宜変更--%>
    <title><%= Me.Form.ClientId %></title>
    <%--全画面共通のスタイルシート --%>
    <link href="~/css/commonStyle.css" rel="stylesheet" type="text/css" />
    <%--個別のスタイルは以下に記載 OR 外部ファイルに逃す --%>
    <style>
       /* テキスト表示情報 */
       #itemTable{
           table-layout:fixed; 
           margin-top:10px;
           margin-left: 65px;
           width:816px;
	   }

       /* 共通セル設定 */
       #itemTable td{
           padding:10px;
           vertical-align:middle;
	   }
	   #itemTable col:nth-child(1) {
           width:175px;
	   }
	   #itemTable col:nth-child(2) {
           width:80px;
	   }
	   #itemTable col:nth-child(3) {
           width:180px;
	   }
	   #itemTable col:nth-child(4) {
           width:120px;
	   }
	   #itemTable col:nth-child(5) {
           width:200px;
	   }
       /* 検索条件テーブルの行の高さ */
       #itemTable tr {
           height:22.4px;
       }
       /* 1列目をアンダーライン */
	   #itemTable td:nth-child(1){
           font-weight:bold;
           text-decoration:underline;
	   }
       /* 予実績選択・船名は左選択がないためアンダーラインなし */
	   #itemTable tr:nth-child(1) td:nth-child(1){
           text-decoration:none;
	   }
       /* 予定実績 */
	   #itemTable ul li{
           display:inline-block;
           margin-right:5px;
           vertical-align:middle;
	   }
	   #itemTable ul li input[type="radio"]{
           margin-top:1px;
           display:inline-block;
           vertical-align:middle;
	   }
	   #itemTable ul li label{
           display:inline-block;
           margin-top:1px;
           margin-left:2px;
           margin-right:10px;
           vertical-align:middle;
           
	   }
	   /* From To文言のセル設定 */
       #itemTable tr:nth-child(2) td:nth-child(2),
       #itemTable tr:nth-child(2) td:nth-child(4)
       {
           text-align:right;
       }

       /* 検索条件の入力テキスト */
       #itemTable input[type="text"] {
           height:22.4px;
       }
       /* コード検索の名称表示部 */
       #itemTable span[id^="lbl"][id$="Text"]
       {
           color:blue;
       }
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
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加

        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            /* ボタンクリックイベントのバインド(適宜追加) */
            /* 実行 */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnEnter.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';                      /* 年月日 */
            var viewShipperId = '<%= Me.vLeftShipper.ClientID %>';              /* 荷主コード */
            var viewConsigneeId = '<%= Me.vLeftConsignee.ClientID %>';          /* 荷受人コード */
            var viewPOLPortId = '<%= Me.vLeftPOLPort.ClientID %>';              /* POL港コード */
            var viewPODPortId = '<%= Me.vLeftPODPort.ClientID %>';              /* POD港コード */
            var viewProductId = '<%= Me.vLeftProduct.ClientID %>';              /* 積載品コード */
            var viewBrId = '<%= Me.vLeftBreakerId.ClientID %>';                 /* ブレーカーID */
            var viewApprovalId = '<%= Me.vLeftApproval.ClientID %>';            /* 承認 */
            var viewOfficeId = '<%= Me.vLeftOffice.ClientID %>';                /* 代理店コード */
            var dblClickObjects = [['<%= Me.txtStYMD.ClientID %>', viewCalId],
                                   ['<%= Me.txtEndYMD.ClientID %>', viewCalId],
                                   ['<%= Me.txtShipper.ClientID %>', viewShipperId],
                                   ['<%= Me.txtConsignee.ClientID %>', viewConsigneeId],
                                   ['<%= Me.txtPOLPort.ClientID %>', viewPOLPortId],
                                   ['<%= Me.txtPODPort.ClientID %>', viewPODPortId],
                                   ['<%= Me.txtProduct.ClientID %>', viewProductId],
                                   ['<%= Me.txtBrId.ClientID %>', viewBrId],
                                   ['<%= Me.txtApproval.ClientID %>', viewApprovalId],
                                   ['<%= Me.txtOffice.ClientID %>', viewOfficeId]];
            bindLeftBoxShowEvent(dblClickObjects);

            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbShipper.ClientID %>', '3', '1'],
                                           ['<%= Me.lbConsignee.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPOLPort.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPODPort.ClientID %>', '3', '1'],
                                           ['<%= Me.lbBreakerId.ClientID %>', '3', '1'],
                                           ['<%= Me.lbProduct.ClientID %>', '3', '1'],
                                           ['<%= Me.lbApproval.ClientID %>', '3', '1'],
                                           ['<%= Me.lbOffice.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = [['<%= Me.txtShipper.ClientID %>'],
                                         ['<%= Me.txtConsignee.ClientID %>'],
                                         ['<%= Me.txtPOLPort.ClientID %>'],
                                         ['<%= Me.txtPODPort.ClientID %>'],
                                         ['<%= Me.txtProduct.ClientID %>'],
                                         ['<%= Me.txtApproval.ClientID %>'],
                                         ['<%= Me.txtOffice.ClientID %>']];
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
    <form id="GBT00005S" runat="server">
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
                <div id="actionButtonsBox">
                    <input id="btnEnter" type="button" value="実行"  runat="server" />
                    <input id="btnBack" type="button" value="終了"  runat="server"  />
                    <div id="btnFIRST" class="firstPage" runat="server" style="visibility:hidden;"></div>
                    <div id="btnLAST" class="lastPage" runat="server" style="visibility:hidden;"></div>
                </div>

                <table id="itemTable">
                    <colgroup>
                        <col /><col /><col /><col /><col /><col />
                    </colgroup>
                    <%-- ブレーカータイプ区分 --%> 
                    <tr>
                        <td colspan="2">
                            <asp:Label ID="lblBreakerType" runat="server" Text="ブレーカータイプ"></asp:Label>
                        </td>
                        <td colspan="3">
                            <asp:RadioButtonList ID="rblBreakerType" runat="server" RepeatLayout="UnorderedList">
                                <asp:ListItem Text="SALES" Value="SALES"></asp:ListItem>
                                <asp:ListItem Text="OPERATION" Value="OPERATION"></asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    <%-- 年度 --%>
	                <tr>
		                <td>
			                <a><asp:Label ID="lblYMD1" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:Label ID="lblYMD2" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtStYMD" runat="server"></asp:TextBox></a>
		                </td>
		                <td>
			                <a><asp:Label ID="lblTilde" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtEndYMD" runat="server"></asp:TextBox></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                    <%-- 荷主コード --%>
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblShipper" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtShipper" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="2">
			                <a><asp:Label ID="lblShipperText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                    <%-- 荷受人コード --%> 
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblConsignee" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtConsignee" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="2">
			                <a><asp:Label ID="lblConsigneeText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                    <%-- 積載品コード --%> 
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblProduct" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtProduct" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="2">
			                <a><asp:Label ID="lblProductText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                    <%-- POL港コード --%> 
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblPOLPort" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtPOLPort" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="2">
			                <a><asp:Label ID="lblPOLPortText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                    <%-- POD港コード --%> 
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblPODPort" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtPODPort" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="2">
			                <a><asp:Label ID="lblPODPortText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                    <%-- Breaker ID --%> 
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblBrId" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtBrId" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="3">
                            &nbsp;
		                </td>
	                </tr>
                    <%-- 承認コード --%> 
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblApproval" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtApproval" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="2">
			                <a><asp:Label ID="lblApprovalText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                    <%-- 代理店コード --%> 
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblOffice" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtOffice" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="2">
			                <a><asp:Label ID="lblOfficeText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                </table>

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
                    <%-- 遷移時のMAPVariant保持 --%>
                    <asp:HiddenField ID="hdnMapVariant" runat="server" Value="" Visible="false" />
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
                    <%--  　荷主コード　 --%>
                    <asp:View id="vLeftShipper" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbShipper" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 荷主コード VIEW　 --%>
                    <%--  　荷受人コード　 --%>
                    <asp:View id="vLeftConsignee" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbConsignee" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 荷受人コード VIEW　 --%>
                    <%--  　POL港コード　 --%>
                    <asp:View id="vLeftPOLPort" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbPOLPort" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END POL港コード VIEW　 --%>
                    <%--  　POD港コード　 --%>
                    <asp:View id="vLeftPODPort" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbPODPort" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END POD港コード VIEW　 --%>
                    <%--  　積載品コード　 --%>
                    <asp:View id="vLeftProduct" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbProduct" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 積載品コード VIEW　 --%>
                    <%--  　ブレーカーID　 --%>
                    <asp:View id="vLeftBreakerId" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbBreakerId" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END ブレーカーID VIEW　 --%>
                    <%--  　承認　 --%>
                    <asp:View id="vLeftApproval" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbApproval" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 承認 VIEW　 --%>
                    <%--  　代理店コード　 --%>
                    <asp:View id="vLeftOffice" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbOffice" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 代理店コード VIEW　 --%>
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

        </div>
    </form>
</body>
</html>