<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00003RESULT.aspx.vb" Inherits="OFFICE.GBT00003RESULT" %>

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
    <link href="~/GB/css/GBT00003RESULT.css" rel="stylesheet" />
    <style>
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
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00003RESULT.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">
        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>', '<%= Me.btnExtract.ClientID %>',
                                       '<%= Me.btnFIRST.ClientId  %>','<%= Me.btnLAST.ClientId  %>',
                                       '<%= Me.btnExcelDownload.ClientID %>'];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewProductId = '<%= Me.vLeftProduct.ClientID %>';
            var viewPortId = '<%= Me.vLeftPort.ClientID %>';
            var dblClickObjects = [['<%= Me.txtProduct.ClientID %>',viewProductId],
                                   ['<%= Me.txtPol.ClientID %>', viewPortId],
                                   ['<%= Me.txtPod.ClientID %>', viewPortId]];

            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbProduct.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPort.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = ['<%= Me.txtProduct.ClientID %>',
                                         '<%= Me.txtPol.ClientID %>',
                                         '<%= Me.txtPod.ClientID %>'];
            bindTextOnchangeEvent(targetOnchangeObjects);


            /* 右ボックスの開閉ダブルクリックイベントバインド
               右上透明ボックス、下のメッセージ欄、他がある場合は個別で　*/
            bindDiplayRightBoxEvent();
            /* ヘルプボタン表示のダブルクリックイベントバインド */
            bindFooterShowHelpEvent();

            /* ヘルプ表示処理 */
            openHelpPage(); /* hdnCanHelpOpenに"1"が立たない限り開きません。 */

            /* カレンダー描画処理 */
            //var calValueObj = document.getElementById('');
            //if (calValueObj !== null) {
            //    /* 日付格納隠し項目がレンダリングされている場合のみ実行 */
            //    carenda(0);
            //    setAltMsg(firstAltYMD, firstAltMsg);
            //}

            /* グリッドのクリックイベント紐づけ */
            bindGridButtonClickEvent();
            /* ブレーカー単票画面の表示 */
            var breakerOpenFlg = document.getElementById('hdnBreakerViewOpen');
            if (breakerOpenFlg !== null) {
                if (breakerOpenFlg.value === '1') {
                    breakerOpenFlg.value = '0';
                    openBreakerWindow();
                }
            }
            /* 共通一覧のスクロールイベント紐づけ */
            bindListCommonEvents('<%= Me.WF_LISTAREA.ClientId %>','<%= if(IsPostBack = True, "1", "0") %>',true);
            commonCreateSearchArea('searchCondition');
            chkShowDeleteChanged();
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
    <form id="GBT00003R" runat="server">
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
                <div id="actionButtonsBox">
                    <input id="btnExtract" type="button" value="絞り込み"  runat="server"  />
                    <input id="btnExcelDownload" type="button" value="Excelダウンロード"  runat="server" />   
                    <input id="btnBack" type="button" value="戻る"  runat="server" />
                    <div id="btnFIRST" class="firstPage" runat="server"></div>
                    <div id="btnLAST" class="lastPage" runat="server"></div>
                </div>
                <div id="searchCondition">
                </div>
                <div id="divSearchConditionBox">
                    <asp:HiddenField ID="hdnSearchConditionDetailOpenFlg" runat="server" Value="" />
                    <span>
                        <asp:Label ID="lblShipperLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtShipper" runat="server" Text=""></asp:TextBox>
                    </span>
                    <span>
                        <asp:Label ID="lblConsigneeLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtConsignee" runat="server"></asp:TextBox>
                    </span>
                    <span>
                        <asp:Label ID="lblProductLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtProduct" runat="server"></asp:TextBox>
                        <asp:Label ID="lblProductText" runat="server" Text=""></asp:Label>
                    </span>
                    <span>
                        <asp:Label ID="lblPolLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtPol" runat="server"></asp:TextBox>
                        <asp:Label ID="lblPolText" runat="server" Text=""></asp:Label>
                    </span>
                    <span>
                        <asp:Label ID="lblPodLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtPod" runat="server"></asp:TextBox>
                        <asp:Label ID="lblPodText" runat="server" Text=""></asp:Label>
                    </span>
                    <span>
                        <asp:Label ID="lblBreakerIdLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtBreaker" runat="server"></asp:TextBox>
                    </span>
                    <span>
                        <asp:Label ID="lblOrderIdLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtOrderId" runat="server"></asp:TextBox>
                    </span>
                    <span>
                        <asp:Label ID="lblListViewTypeLabel" runat="server" Text="VIEW"></asp:Label>
                        <asp:RadioButtonList ID="rblListViewType" runat="server" RepeatLayout="UnorderedList">
                        </asp:RadioButtonList>
                    </span>
                    <span>
                        <asp:Label ID="lblDeleteMode" runat="server" Text="Delete Mode"></asp:Label>
                        <span onchange="chkShowDeleteChanged();"><asp:CheckBox ID="chkShowDelete" runat="server" Checked="false" /></span>
                    </span>
                </div>
                <asp:panel id="WF_LISTAREA" runat="server" data-delmode='<%# If(Me.chkShowDelete.Checked, "1", "0")  %>' >
                </asp:panel>
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
                    <asp:HiddenField ID="hdnMouseWheel" runat="server" Value="" />   <%--  マウスホイールのUPorDownを記憶 --%>
                    <asp:HiddenField ID="hdnListPosition" runat="server" Value="" /> <%--  縦スクロールポジション --%>
                    <asp:HiddenField ID="hdnListDBclick" runat="server" Value="" />  <%--  ダブルクリックした行番号を記録 --%>   
                    <asp:HiddenField ID="hdnListCurrentRownum" runat="server" Value="" /> <%-- 一覧でボタンクリックイベントを発生させたRowNumを保持 --%>
                    <%-- 前画面(検索条件)引継用 --%>
                    <asp:HiddenField ID="hdnSearchType" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETDStYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETDEndYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETAStYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETAEndYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnShipper" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnConsignee" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnPortOfLoading" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnPortOfDischarge" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnOffice" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnReportVariant" runat="server" Value="" Visible="False" />
                    <%-- 次画面引継用 --%>
                    <asp:HiddenField ID="hdnSelectedBrId" runat="server" Value="" />  <%--  行ダブルクリック(★ボタンクリック)したブレーカーNo --%>
                    <asp:HiddenField ID="hdnSelectedOdId" runat="server" Value="" />  <%--  行ダブルクリック(★ボタンクリック)したオーダーNo --%>
                    <%-- Breaker単票画面設定 --%>
                    <asp:HiddenField ID="hdnBreakerViewOpen" runat="server" Value="0" EnableViewState="false" />
                    <asp:HiddenField ID="hdnBreakerViewUrl"  runat="server" Value="" />
                    <%-- 削除確認メッセージ --%>
                    <asp:HiddenField ID="hdnConfirmTitle"  runat="server" Value="" />
                    <asp:HiddenField ID="hdnMsgboShowFlg"  runat="server" Value="0" />
                    <%-- MAPVARIANT保持用 --%>
                    <asp:HiddenField ID="hdnThisMapVariant" Value="" runat="server" Visible="false" />
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
                    <%-- 積載品 VIEW　 --%>
                    <asp:View ID="vLeftProduct" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbProduct" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 積載品 VIEW　 --%>
                    <%-- 港選択 VIEW　 --%>
                    <asp:View ID="vLeftPort" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbPort" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 港選択 VIEW　 --%>

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
            <div id="divConfirmBoxWrapper" runat="server">
                <div id="divConfirmBox">
                    <div id="divConfirmtitle">
                        <%= Me.hdnConfirmTitle.Value %>
                    </div>
                    <div id="divConfirmBoxButtons">
                        <input id="btnConfirmOk" type="button" value="OK" runat="server" />
                        <input id="btnConfirmCancel" type="button" value="CANCEL" runat="server" onclick="document.getElementById('divConfirmBoxWrapper').style.display = 'none';" />
                    </div>
                    <div id="divConfirmBoxMessageArea">
                        <div><asp:Label ID="lblConfirmOrderNoName" runat="server" Text=""></asp:Label>:<asp:Label ID="lblConfirmOrderNo" runat="server" Text=""></asp:Label></div>
                        <div><%= Me.hdnConfirmTitle.Value %></div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
