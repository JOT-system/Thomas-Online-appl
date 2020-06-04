<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00026BLNOTRANSFER.aspx.vb" Inherits="OFFICE.GBT00026BLNOTRANSFER" %>

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
           width:80px;
	   }
	   #itemTable col:nth-child(2) {
           width:200px;
	   }
	   #itemTable col:nth-child(3) {
           width:200px;
	   }
	   #itemTable col:nth-child(4) {
           width:200px;
	   }
	   /*#itemTable col:nth-child(5) {
           width:200px;
	   }*/
       /* 検索条件テーブルの行の高さ */
       #itemTable tr {
           height:22.4px;
       }
       /* 1列目を太字 */
	   #itemTable td:nth-child(1){
           font-weight:bold;
	   }
       /* BrNoをアンダーライン */
	   #lblTransfererBrText,#lblTransfereeBrText {
           text-decoration:underline; 
           text-decoration-color:blue;
	   }
       #lblTransfererBrText:hover,#lblTransfereeBrText:hover {
           cursor :pointer ;
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
       /* 検索条件の入力テキスト */
       #itemTable input[type="text"] {
           height:22.4px;
       }
       /* コード検索の名称表示部 */
       #itemTable span[id^="lbl"][id$="Text"]
       {
           color:blue;
       }
       #itemTable span.colorRed[id^='lbl'][id$='Text'] {
           color:red;
           text-decoration :none;
       }
       #itemTable span.colorRed[id^='lbl'][id$='Text']:hover {
           cursor:default;
       }
       #divBreakerInfo {
            height:100%;
            width:100%;
            margin:0px;
            position:fixed;
            z-index:50;
            left:0;
            top:0;
       }
       #ifraBreakerInfo {
            width:100%;
            height:100%;
       }
    </style>
    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8"></script>
    <%-- 左ボックスカレンダー使用の場合のスクリプト --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/script/calendar.js") %>'  charset="utf-8"></script>
    <%--個別のスクリプトは以下に記載 --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00026BLNOTRANSFER.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加

        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            /* ボタンクリックイベントのバインド(適宜追加) */
            /* チェック */
            var targetButtonObjects = ['<%= Me.btnCheck.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* 実行 */
            var targetButtonObjects = ['<%= Me.btnEnter.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* 終了 */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
<%--            var viewTransfererId = '<%= Me.vLeftTransferer.ClientID %>';        /* 移行元 */
            var viewTransfereeId = '<%= Me.vLeftTransferee.ClientID %>';        /* 移行先 */--%>
            var dblClickObjects = [
<%--                                   ['<%= Me.txtTransferer.ClientID %>', viewTransfererId],
                                   ['<%= Me.txtTransferee.ClientID %>', viewTransfereeId]--%>
                                  ];
            bindLeftBoxShowEvent(dblClickObjects);

            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = [
                                            ['<%= Me.txtTransferer.ClientID %>'],
                                            ['<%= Me.txtTransferee.ClientID %>']];
            bindTextOnchangeEvent(targetOnchangeObjects);

            /* 右ボックスの開閉ダブルクリックイベントバインド
               右上透明ボックス、下のメッセージ欄、他がある場合は個別で　*/
            bindDiplayRightBoxEvent();
            /* ヘルプボタン表示のダブルクリックイベントバインド */
            bindFooterShowHelpEvent();

            /* ヘルプ表示処理 */
            openHelpPage(); /* hdnCanHelpOpenに"1"が立たない限り開きません。 */

            /* カレンダー描画処理 */
<%--            var calValueObj = document.getElementById('<%= Me.hdnCalendarValue.ClientID %>');
            if (calValueObj !== null) {
                /* 日付格納隠し項目がレンダリングされている場合のみ実行 */
                carenda(0);
                setAltMsg(firstAltYMD, firstAltMsg);
            }--%>

            screenUnlock();
            focusAfterChange();
        });
        function brIdDoubleClick(bridObj) {
            if (bridObj.innerText !== "none") {
                var selectedBridObj = document.getElementById('hdnSelectedBrId');
                selectedBridObj.value = bridObj.innerText;
                openBreakerWindow();
            }
        }
    </script>
</head>
<%-- 基本的にタグ内でのクライアントサイドのJavaScriptのイベント記述はせず、
    ヘッダーにあるwindow.onloadでイベントバインドをします。
    スタイルなども直接記述は極力行わないように
    ※%付きのコメントはHTMLソース表示でもレンダリングされないものです --%>
<body>
    <%--FormIDは適宜変更ください。 --%>
    <form id="GBT00026T" runat="server">
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
                    <input id="btnCheck" type="button" value="チェック"  runat="server"  />
                    <input id="btnEnter" type="button" value="付け替え"  runat="server"  />
                    <input id="btnBack" type="button" value="終了"  runat="server"  />
                    <div id="btnFIRST" class="firstPage" runat="server" style="visibility:hidden;"></div>
                    <div id="btnLAST" class="lastPage" runat="server" style="visibility:hidden;"></div>
                </div>

                <table id="itemTable">
                    <colgroup>
                        <col /><col /><col /><col /><col />
                    </colgroup>
                    <%-- 移行元 --%>
                    <tr>
		                <td>
			                <a><asp:Label ID="lblTransferer" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtTransferer" runat="server"></asp:TextBox></a>
		                </td>
                        <td>
			                <a><asp:Label ID="lblTransfererOdrText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
			                <a><asp:Label ID="lblTransfererBrText" runat="server" ondblclick="brIdDoubleClick(this);" ></asp:Label></a>
		                </td>
                        <td>
                            &nbsp;
                        </td>
	                </tr>
                    <%-- 移行先 --%> 
                    <tr>
		                <td>
			                <a><asp:Label ID="lblTransferee" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtTransferee" runat="server"></asp:TextBox></a>
		                </td>
                        <td>
			                <a><asp:Label ID="lblTransfereeOdrText" runat="server" ></asp:Label></a>
		                </td>
                        <td>
			                <a><asp:Label ID="lblTransfereeBrText" runat="server" ondblclick="brIdDoubleClick(this);"></asp:Label></a>
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
                    <%-- MAPVARIANT保持 --%>
                    <asp:HiddenField ID="hdnMapVariant" runat="server" Value="" Visible="false" />

                    <%-- CHECK OK B/L No. --%>
                    <asp:HiddenField ID="hdnChkTransferer" runat="server" Value=""  />
                    <asp:HiddenField ID="hdnChkTransferee" runat="server" Value=""  />
                    <%-- 次画面引継用 --%>
                    <asp:HiddenField ID="hdnSelectedBrId" runat="server" Value="" />  <%--  行ダブルクリック(★ボタンクリック)したブレーカーNo --%>
                    <%-- Breaker単票画面設定 --%>
                    <asp:HiddenField ID="hdnBreakerViewUrl"  runat="server" Value="" />

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
                    <%--  　移行元　 --%>
                    <asp:View id="vLeftTransferer" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTransferer" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 移行元 VIEW　 --%>
                    <%--  　移行先　 --%>
                    <asp:View id="vLeftTransferee" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTransferee" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 移行先 VIEW　 --%>
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
