<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00001NEWBREAKER.aspx.vb" Inherits="OFFICE.GBT00001NEWBREAKER" %>
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
    <style>
        #divInputArea
        {
            padding:10px;
            min-width:100px;
        }
        #tblInputArea{
            table-layout: fixed;
            border-collapse: separate;
            border-spacing: 1px;
            border: 1px solid #999999;
            min-width:805px;
        } 

        #tblInputArea th
        ,#tblInputArea td
        {
            font-size:small;
            text-align:left;
            vertical-align:middle;
            border:1px solid #999999;
            padding:5px;
            text-overflow:clip;
            overflow:hidden;
        }
        #tblInputArea th
        {
            background-color: #8BACCD;
        }
        /* 1カラム目幅 見出し */
        #tblInputArea col:nth-child(1)
        {
            width:200px;
        }
        /* 2カラム目幅 入力列(コード) */
        #tblInputArea col:nth-child(2)
        {
            width:219px;
        }
        /* 3カラム目幅 名称 */
        #tblInputArea col:nth-child(3)
        {
            min-width:380px;
            width:auto;
        }
        /* 標準のテキストボックス(TDの幅いっぱいにする) */
        #tblInputArea td input[type=text]{
            width:calc(100% - 5px);
        }
        /* 発地、着地のテキストボックス */
        /*#tblInputArea tr:nth-child(4) input[type=text],
        #tblInputArea tr:nth-child(6) input[type=text],
        #tblInputArea tr:nth-child(7) input[type=text],
        #tblInputArea tr:nth-child(8) input[type=text]{
            width:calc(50% - 11.75px);
        }*/
        #lblTransferPattern,#lblPol1,#lblShipper,
        #lblPod1,#lblPol2,#lblPod2
        {
            text-decoration:underline;
        }
        #tblInputArea span[id^="lbl"][id$="Text"] {
            color: rgb(0,0,255);
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
            var targetButtonObjects = ['<%= Me.btnCreate.ClientId  %>','<%= Me.btnBack.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonSel.ClientId  %>','<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewBreakerType = '<%= Me.vLeftBreakerType.ClientID %>';
            var viewTransferPattern = '<%= Me.vLeftTransferPattern.ClientID %>';
            var viewPort = '<%= Me.vLeftPort.ClientID %>';
            var viewShipper = '<%= Me.vLeftShipper.ClientID %>';

            var dblClickObjects = [['<%= Me.txtBreakerType.ClientID %>',viewBreakerType],
                                   ['<%= Me.txtTransferPattern.ClientID %>', viewTransferPattern],
                                   ['<%= Me.txtPolPort1.ClientID %>', viewPort],
                                   ['<%= Me.txtShipper.ClientID %>', viewShipper],
                                   ['<%= Me.txtPodPort1.ClientID %>', viewPort],
                                   ['<%= Me.txtPolPort2.ClientID %>', viewPort],
                                   ['<%= Me.txtPodPort2.ClientID %>', viewPort]];
            bindLeftBoxShowEvent(dblClickObjects);

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbBreakerType.ClientID %>', '3', '1'],
                                           ['<%= Me.lbTransferPattern.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPort.ClientID %>', '3', '1'],
                                           ['<%= Me.lbShipper.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド */
            var targetOnchangeObjects = ['<%= Me.txtBreakerType.ClientID %>',
                                         '<%= Me.txtTransferPattern.ClientID %>',
                                         '<%= Me.txtPolPort1.ClientID %>',
                                         '<%= Me.txtShipper.ClientID %>',
                                         '<%= Me.txtPodPort1.ClientID %>',
                                         '<%= Me.txtPolPort2.ClientID %>',
                                         '<%= Me.txtPodPort2.ClientID %>'];

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
    <form id="GBT00001N" runat="server">
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
            <%--コンテンツボックス --%>
            <div id="divContensbox">
                <div id="actionButtonsBox">
                    <input id="btnCreate" type="button" value="作成" runat="server" />
                    <input id="btnBack" type="button" value="戻る"  runat="server"  />
                    <div id="btnFIRST" class="firstPage" runat="server" style="visibility:hidden;"></div>
                    <div id="btnLAST" class="lastPage" runat="server" style="visibility:hidden;"></div>
                </div>
                <div id="divInputArea">
                    <table id="tblInputArea">
                        <colgroup>
                            <col /><col />
                            <col />
                        </colgroup>
                        <tr>
                            <th>
                                <asp:Label ID="lblBreakerType" runat="server" Text="ブレーカータイプ" CssClass=""></asp:Label>
                            </th>
                            <td >
                                    <asp:TextBox ID="txtBreakerType" runat="server" Text="" Enabled ="false" ></asp:TextBox>
                            </td>
                            <td >
                                    <asp:Label ID="lblBreakerTypeText" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr id="trLeaseTankUse" runat="server">
                            <th>
                                <asp:Label ID="lblLeaseTankUse" runat="server" Text="リースタンク使用" CssClass=""></asp:Label>
                            </th>
                            <td colspan="2">
                                <asp:CheckBox ID="chkLeaseTankUse" runat="server" />
                            </td>
                        </tr>
                        <tr>
                            <th>
                                <asp:Label ID="lblTransferPattern" runat="server" Text="輸送パターン" CssClass="requiredMark2"></asp:Label>
                            </th>
                            <td >
                                <asp:TextBox ID="txtTransferPattern" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td>
                                <asp:Label ID="lblTransferPatternText" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <th>
                                <asp:Label ID="lblPol1" runat="server" Text="発地" CssClass="requiredMark2"></asp:Label>
                            </th>
                            <td >
                                <%--<asp:TextBox ID="txtPolCountry1" runat="server" Text=""></asp:TextBox>
                                /--%>
                                <asp:TextBox ID="txtPolPort1" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td>
                                <%--<asp:Label ID="lblPolCountry1Text" runat="server" Text=""></asp:Label>
                                <%= If(Convert.ToString(Me.lblPolCountry1Text.Text) = "" And Convert.ToString(Me.lblPolPort1Text.Text) = "", "", "/") %>--%>
                                <asp:Label ID="lblPolPort1Text" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <th>
                                <asp:Label ID="lblShipper" runat="server" Text="荷主" CssClass="requiredMark2"></asp:Label>
                            </th>
                            <td>
                                <asp:TextBox ID="txtShipper" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td>
                                <asp:Label ID="lblShipperText" runat="server" Text=""></asp:Label>
                            </td>

                        </tr>
                        <tr id="trPod1" runat="server">
                            <th>
                                <asp:Label ID="lblPod1" runat="server" Text="着地" CssClass="requiredMark2"></asp:Label>
                            </th>
                            <td >
                                <%--<asp:TextBox ID="txtPodCountry1" runat="server" Text=""></asp:TextBox>
                                /--%>
                                <asp:TextBox ID="txtPodPort1" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td>
                                <%--<asp:Label ID="lblPodCountry1Text" runat="server" Text=""></asp:Label>
                                <%= If(Convert.ToString(Me.lblPodCountry1Text.Text) = "" And Convert.ToString(Me.lblPodPort1Text.Text) = "", "", "/") %>--%>
                                <asp:Label ID="lblPodPort1Text" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr id="trPol2" runat="server">
                            <th>
                                <asp:Label ID="lblPol2" runat="server" Text="発地" CssClass="requiredMark2"></asp:Label>
                            </th>
                            <td >
                                <%--<asp:TextBox ID="txtPolCountry2" runat="server" Text=""></asp:TextBox>
                                /--%>
                                <asp:TextBox ID="txtPolPort2" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td>
                                <%--<asp:Label ID="lblPolCountry2Text" runat="server" Text=""></asp:Label>
                                <%= If(Convert.ToString(Me.lblPolCountry2Text.Text) = "" And Convert.ToString(Me.lblPolPort2Text.Text) = "", "", "/") %>--%>
                                <asp:Label ID="lblPolPort2Text" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                        <tr  id="trPod2" runat="server">
                            <th>
                                <asp:Label ID="lblPod2" runat="server" Text="着地" CssClass="requiredMark2"></asp:Label>
                            </th>
                            <td >
                                <%--<asp:TextBox ID="txtPodCountry2" runat="server" Text=""></asp:TextBox>
                                /--%>
                                <asp:TextBox ID="txtPodPort2" runat="server" Text=""></asp:TextBox>
                            </td>
                            <td>
                                <%--<asp:Label ID="lblPodCountry2Text" runat="server" Text=""></asp:Label>
                                <%= If(Convert.ToString(Me.lblPodCountry2Text.Text) = "" And Convert.ToString(Me.lblPodPort2Text.Text) = "", "", "/") %>--%>
                                <asp:Label ID="lblPodPort2Text" runat="server" Text=""></asp:Label>
                            </td>
                        </tr>
                    </table>
                </div>
                <div id="divHidden">
                    <%-- 必要な隠し要素はこちらに(共通で使用しそうなものは定義済) --%>
                    <asp:HiddenField ID="hdnSubmit" runat="server" Value="" />      <%-- サーバー処理中（TRUE:実行中、FALSE:未実行）--%>
                    <asp:HiddenField ID="hdnButtonClick" runat="server" Value="" /> <%-- ボタン押下(押下したボタンIDを格納) --%>
                    <%-- フィールド変更イベントをサーバー処理させるための定義 --%>
                    <asp:HiddenField ID="hdnOnchangeFieldPrevValue" runat="server" Value="" /> <%-- フォーカスが入った瞬間の値を保持 --%>
                    <asp:HiddenField ID="hdnOnchangeField" runat="server" Value="" />   <%-- テキスト項目変更値格納用 --%>
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
                    <%-- 当画面固有 --%>
                    <asp:HiddenField ID="hdnIsTrilateral" runat="server" /> <%-- 三国間か？ "1"=三国間 それ以外=二国 --%>
                    <asp:HiddenField ID="hdnPolCountry1" runat="server" />
                    <asp:HiddenField ID="hdnPolCountry2" runat="server" />
                    <asp:HiddenField ID="hdnPodCountry1" runat="server" />
                    <asp:HiddenField ID="hdnPodCountry2" runat="server" />
                    <asp:HiddenField ID="hdnPolPort1" runat="server" />
                    <asp:HiddenField ID="hdnPolPort2" runat="server" />
                    <asp:HiddenField ID="hdnPodPort1" runat="server" />
                    <asp:HiddenField ID="hdnPodPort2" runat="server" />
                    <%-- コピー元のBrId  --%>
                    <asp:HiddenField ID="hdnCopyBaseBrId" Visible="false" runat="server" />
                    <%-- 前画面(検索画面)検索条件保持用 --%>
                    <asp:HiddenField ID="hdnStYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnEndYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnShipper" runat="server" Value="" />
                    <asp:HiddenField ID="hdnConsignee" runat="server" Value="" />
                    <asp:HiddenField ID="hdnPort" runat="server" Value="" />
                    <asp:HiddenField ID="hdnApproval" runat="server" Value="" />
                    <asp:HiddenField ID="hdnOffice" runat="server" Value="" /> 
                    <asp:HiddenField ID="hdnBreakerType" runat="server" Value="" /> 
                    <asp:HiddenField ID="hdnSearchBreakerType" runat="server" Value="" /> 
                    <%-- 当画面Vari保持  --%>
                    <asp:HiddenField ID="hdnThisMapVariant" runat="server" Visible="false" Value="" />
                    <%-- 輸送パターン別Breaker作成時の初期値  --%>
                    <asp:HiddenField ID="hdnInitInvoicedBy" runat="server" Visible="false" Value="" />
                    <asp:HiddenField ID="hdnInitBillingCategory" runat="server" Visible="false" Value="" />
                    <asp:HiddenField ID="hdnInitConsignee" runat="server" Visible="false" Value="" />
                    <asp:HiddenField ID="hdnInitProductCode" runat="server" Visible="false" Value="" />
                    <asp:HiddenField ID="hdnInitAgentPol1" runat="server" Visible="false" Value="" />
                    <asp:HiddenField ID="hdnInitAgentPod1" runat="server" Visible="false" Value="" />
                    <asp:HiddenField ID="hdnInitAgentPol2" runat="server" Visible="false" Value="" />
                    <asp:HiddenField ID="hdnInitAgentPod2" runat="server" Visible="false" Value="" />

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
                    <%-- ブレーカー種類選択 VIEW --%>
                    <asp:View ID="vLeftBreakerType" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbBreakerType" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 輸送パターン選択 VIEW　 --%>
                    <%-- 輸送パターン選択 VIEW　 --%>
                    <asp:View ID="vLeftTransferPattern" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTransferPattern" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 輸送パターン選択 VIEW　 --%>
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
