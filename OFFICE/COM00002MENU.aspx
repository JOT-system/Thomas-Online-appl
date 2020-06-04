<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="COM00002MENU.aspx.vb" Inherits="OFFICE.COM00002MENU" %>

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
        #divMenuBox {
            vertical-align:top;
            text-align:center;
            white-space: nowrap;
            letter-spacing: -.4em;
            padding-top:35px;
        }
        #divMenu_L,#divMenu_R {
            vertical-align:top;
            display:inline-block;
            width:528px;
            letter-spacing:normal;
        }
        #divMenu_R {
            width:auto;
        }
        #divMenuBox input {
            margin-left:35px;
            width:400px;
            height:26.5px;
            text-align:left;
            vertical-align:middle;
            background-image:none;
            background-color:gray;
            overflow:hidden;
            border:none;
        }
        #divMenuBox input:hover {
            background-color:blue;
            color:white;
            cursor:pointer;
        }
        span.WF_MenuLabel_L,
        span.WF_MenuLabel_R{
            font-size:large !important;
            font-weight:bold;
        }
        #divMenuBox td {
            text-align:left;
            vertical-align:middle;
            padding-top:2px;
        }
        #divMenuBox tr {
            height:27px;
        }
    </style>
    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8"></script>
    <%--個別のスクリプトは以下に記載 --%>
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加

        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            /* ボタンクリックイベントのバインド(適宜追加) */
            /* 実行 */
<%--            var targetButtonObjects = ['<%= Me.btnEnter.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* 終了 */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);--%>

            /* 右ボックスの開閉ダブルクリックイベントバインド
               右上透明ボックス、下のメッセージ欄、他がある場合は個別で　*/
            bindDiplayRightBoxEvent();
            /* ヘルプボタン表示のダブルクリックイベントバインド */
            bindFooterShowHelpEvent();

            /* ヘルプ表示処理 */
            openHelpPage(); /* hdnCanHelpOpenに"1"が立たない限り開きません。 */

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
    <form id="COM00002" runat="server">
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
                <div id="actionButtonsBox" style="display:none;">
                    <%-- ここにログオフボタンでも？ ↑のstyle属性全消しで出ます --%>
                    <input id="btnLogoff" type="button" value="LOGOFF"  runat="server"  />
                </div>

                <div  class="Menuheaderbox" id="divMenuBox">
                    <div  class="Menu_L" id="divMenu_L"  >
                    <asp:Repeater ID="Repeater_Menu_L" runat="server" >
                        <HeaderTemplate>
                            <table>
                        </HeaderTemplate>

                        <ItemTemplate>
                            <tr>
                            <td >
                                <asp:Label ID="WF_MenuLabe_L" runat="server" CssClass="WF_MenuLabel_L"></asp:Label>
                                <asp:Label ID="WF_MenuURL_L" runat="server" Visible="False"></asp:Label>
                                <asp:Label ID="WF_MenuVARI_L" runat="server" Visible="False"></asp:Label>
                                <asp:Label ID="WF_MenuMAP_L" runat="server" Visible="False"></asp:Label>
                                <asp:Button ID="WF_MenuButton_L" runat="server" CssClass="WF_MenuButton_L" OnClientClick="commonDispWait();" /> 
                            </td>
                            </tr>
                        </ItemTemplate>

                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    </div>

                    <div class="Menu_R" id="divMenu_R" >
                    <asp:Repeater ID="Repeater_Menu_R" runat="server" >
                        <HeaderTemplate>
                            <table>
                        </HeaderTemplate>

                        <ItemTemplate>
                            <tr>
                            <td >
                                <asp:Label ID="WF_MenuLabe_R" runat="server" CssClass="WF_MenuLabel_R"></asp:Label>
                                <asp:Label ID="WF_MenuURL_R" runat="server"  Visible="False" ></asp:Label>
                                <asp:Label ID="WF_MenuVARI_R" runat="server"  Visible="False" ></asp:Label>
                                <asp:Label ID="WF_MenuMAP_R" runat="server" Visible="False"></asp:Label>
                                <asp:Button ID="WF_MenuButton_R" runat="server" CssClass="WF_MenuButton_R" OnClientClick="commonDispWait();" /> 
                            </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </table>
                        </FooterTemplate>
             
                    </asp:Repeater>
                    </div>

                </div>
                <div id="divHidden">
                    <%-- 必要な隠し要素はこちらに(共通で使用しそうなものは定義済) --%>
                    <asp:HiddenField ID="hdnSubmit" runat="server" Value="" />      <%-- サーバー処理中（TRUE:実行中、FALSE:未実行）--%>
                    <asp:HiddenField ID="hdnButtonClick" runat="server" Value="" /> <%-- ボタン押下(押下したボタンIDを格納) --%>
                    <%-- フィールド変更イベントをサーバー処理させるための定義 --%>
                    <asp:HiddenField ID="hdnOnchangeField" runat="server" Value="" />   <%-- テキスト項目変更値格納用 --%>
                    <asp:HiddenField ID="hdnOnchangeFieldPrevValue" runat="server" Value="" /> <%-- フォーカスが入った瞬間の値を保持 --%>
                    <asp:HiddenField ID="hdnActiveElementAfterOnChange" runat="server" Value="" /> <%-- 変更後イベント直後のフォーカスオブジェクト --%>
                    <%-- フッターヘルプ関連処理で使用 --%>
                    <asp:HiddenField ID="hdnHelpChange" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCanHelpOpen" runat="server" Value="" />
                </div>
            </div>
            <%--フッターボックス --%>
            <div id="divFooterbox" >
                <div><asp:Label ID="lblFooterMessage" runat="server" Text=""></asp:Label></div>
                <div id="divShowHelp" ></div>
            </div>

        </div>
    </form>
</body>
</html>
