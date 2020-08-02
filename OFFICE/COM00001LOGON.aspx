<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="COM00001LOGON.aspx.vb" Inherits="OFFICE.COM00001LOGON" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>    <%--フォームのID以外でタイトルを設定する場合は適宜変更--%>
    <asp:PlaceHolder ID="phCommonHeader" runat="server"></asp:PlaceHolder>
    <title><%= Me.Form.ClientId %></title>
    <%--全画面共通のスタイルシート --%>
    <link href="~/css/commonStyle.css" rel="stylesheet" type="text/css" />
    <%--個別のスタイルは以下に記載 OR 外部ファイルに逃す --%>
    <style>
        #loginInputArea {
            margin-left: 115px;
            margin-top: 40px;
        }
        #loginInputArea table {
            table-layout:fixed;
        }
        #loginInputArea td {
            padding:5px;
            min-height :25px;
            vertical-align:middle;
            text-align:left;
            white-space:nowrap;
        }
        #loginInputArea span{
            font-size:15px !important;
            font-weight: bold;
        }
        #loginInputArea input[type=text],
        #txtPassword
        {
            width:160px;
            height:20px;
        }
        #loginInputArea col:nth-child(1),
        #loginInputArea col:nth-child(3){
            min-width:150px;
            width:150px;
        } 
        #loginInputArea col:nth-child(2){
            min-width: 200px;
            width: 200px;
        } 
        #loginInputArea col:nth-child(4){
            min-width: 110px;
            width: 110px;
        } 
        #loginInputArea col:nth-child(5){
            width:auto;
            min-width: 400px;
        } 
        #loginInputArea col:nth-child(5){
            width:auto;
        } 
        #loginInputArea tr:nth-child(-n +2) td:nth-child(1) {
            padding-left:20px;
        }
        #loginInputArea tr:nth-child(3) td:nth-child(1) {
            height:auto;
        }
        #loginInputArea tr:nth-child(3) td:nth-child(1) span {
            min-width:1072px;
            width:1072px;
            min-height:375px;
            height:375px;
            margin-top:15px;
            display:block;
            border: inset currentColor;
            background-color :white;
            font-weight: normal !important;
            font-size:16px !important;
            padding:2px;
            resize:both;
            overflow-y:auto;
        }
        #btnLogin {
            padding-left:15px;
            padding-right:15px;
        }
        #lblTermIdText {
            font-weight:normal !important;
            color:blue;
        }
        #lblTermId {
            text-decoration:underline;
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
            var targetButtonObjects = ['<%= Me.btnLogin.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientID %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientID %>'];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewTermId = '<%= Me.vLeftTermId.ClientID %>';                      /* 年月日 */
            var dblClickObjects = [['<%= Me.txtTermId.ClientID %>', viewTermId]];
            bindLeftBoxShowEvent(dblClickObjects);

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = [['<%= Me.txtTermId.ClientID %>']];
            bindTextOnchangeEvent(targetOnchangeObjects);

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
    <form id="COM00001" runat="server">
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
                <asp:Button ID="btnSubmitDummy" runat="server" Text="" OnClientClick="document.getElementById('btnLogin').onclick();return false;" style="width:0px !important;height:0px !important;border:0px !important;position:fixed;top:0;left:0;z-index:1;" TabIndex="-1" />

                <div id="actionButtonsBox" style="display:none;">
                </div>
                <div id="loginInputArea">
                    <table>
                        <colgroup>
                            <col /><col /><col /><col /><col /><col />
                        </colgroup>
                        <tr>
                            <td>
                                <asp:Label ID="lblUserId" runat="server" Text="User Account"></asp:Label></td>
                            <td>
                                <asp:TextBox ID="txtUserId" runat="server" TabIndex="1" spellcheck="false"></asp:TextBox>
                            </td>
                            <td colspan="3"> 
                                <input id="btnLogin" type="button" value="Logon" runat="server" tabindex="4"  /></td>
                            <td></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="lblPassword" runat="server" Text="Password"></asp:Label></td>
                            <td>
                                <asp:TextBox ID="txtPassword" runat="server" TabIndex="2" TextMode="Password"></asp:TextBox>
                            </td>
                            <td>
                                <asp:Label ID="lblTermId" runat="server" Text="代行端末ＩＤ"></asp:Label>
                            </td>
                            <td>
                                <asp:TextBox ID="txtTermId" runat="server" TabIndex="3"></asp:TextBox>
                            </td>
                            <td>
                                <asp:Label ID="lblTermIdText" runat="server" Text=""></asp:Label>
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td colspan="6">
                                <asp:Label ID="lblGuidance" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                    </table>
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

                    <asp:HiddenField ID="hdnLoginFlg" runat="server" Value="" /> 
                </div>
            <%-- 左ボックス --%>
            <div id="divLeftbox">
                <div id="divLeftBoxButtonsBox">
                    <input type="button" id="btnLeftBoxButtonSel" value="　選　択　" runat="server" />
                    <input type="button" id="btnLeftBoxButtonCan" value="キャンセル" runat="server"  />
                </div>
                <%--  　マルチビュー　 --%>
                <asp:MultiView ID="mvLeft" runat="server">
                    <%--  　端末ＩＤ VIEW　 --%>
                    <asp:View id="vLeftTermId" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTermId" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 端末ＩＤ VIEW　 --%>
                </asp:MultiView>
            </div> <%-- END 左ボックス --%>
            <%--フッターボックス --%>
            <div id="divFooterbox" >
                <div><asp:Label ID="lblFooterMessage" runat="server" Text=""></asp:Label></div>
                <div id="divShowHelp" ></div>
            </div>

        </div>
       </div>
    </form>
</body>
</html>
