﻿<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00030TANKLIST.aspx.vb" Inherits="OFFICE.GBT00030TANKLIST" %>
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
    <link href="~/GB/css/GBT00030TANKLIST.css" rel="stylesheet" type="text/css" />
    <style>
    </style>
    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8"></script>
    <%-- 左ボックスカレンダー使用の場合のスクリプト --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/script/calendar.js") %>'  charset="utf-8"></script>
    <%--個別のスクリプトは以下に記載 --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00030LIST.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">
        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>',
                                       '<%= Me.btnExtract.ClientID %>',
                                       '<%= Me.btnExcelDownload.ClientID %>',
                                       '<%= Me.btnLeftBoxButtonSel.ClientId  %>','<%= Me.btnLeftBoxButtonCan.ClientId  %>',
                                       '<%= Me.btnFIRST.ClientId  %>','<%= Me.btnLAST.ClientId  %>'
                                       ];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewTankNo = '<%= Me.vLeftTankNo.ClientID  %>';
            var dblClickObjects = [['<%= Me.txtTankNo.ClientId %>', viewTankNo]]
            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            var leftListExtentionTarget = [['<%= Me.lbTankNo.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */

            /* 右ボックスの開閉ダブルクリックイベントバインド
               右上透明ボックス、下のメッセージ欄、他がある場合は個別で　*/
            bindDiplayRightBoxEvent();
            /* ヘルプボタン表示のダブルクリックイベントバインド */
            bindFooterShowHelpEvent();
            /* ヘルプ表示処理 */
            openHelpPage(); /* hdnCanHelpOpenに"1"が立たない限り開きません。 */

            /* 共通一覧のスクロールイベント紐づけ */
            bindListCommonEvents('<%= Me.WF_LISTAREA.ClientId %>', '<%= if(IsPostBack = True, "1", "0") %>',true);

            /* 検索ボックス生成 */
            commonCreateSearchArea('selectHeaderBox');

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
    <form id="GBT00030T" runat="server" >
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
                    <input id="btnExtract" type="button" value="絞り込み"  runat="server"  />
                    <input id="btnExcelDownload" type="button" value="Excelダウンロード"  runat="server" />
                    <input id="btnBack" type="button" value="戻る"  runat="server"  />
                    <div id="btnFIRST" class="firstPage" runat="server"></div>
                    <div id="btnLAST" class="lastPage" runat="server"></div>
                </div>

                <div id="selectHeaderBox" runat="server">
                    <div id="divSearchConditionBox">
                        <asp:HiddenField ID="hdnSearchConditionDetailOpenFlg" runat="server" Value="" />
                        <span>
                            <asp:Label ID="lblTankNoLabel" runat="server" Text=""></asp:Label>
                            <asp:TextBox ID="txtTankNo" runat="server" Text=""></asp:TextBox>
                            <%--<asp:Label ID="lblTankNoText" runat="server" Text=""></asp:Label>--%>
                        </span>
                    </div>
                </div>

                <!-- タンク動静、タンク一覧 -->
                <asp:panel id="WF_LISTAREA" runat="server">
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

                    <%-- ダブルクリックしたタンクId --%>
                    <asp:HiddenField ID="hdnSelectedTankNo" runat="server" Value="" />
                    <%-- 前画面情報の選択モード --%>
                    <asp:HiddenField ID="hdnSelectedPort" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnSelectedActy" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnSelectedMode" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnSelectedOrderNo" runat="server" Value="" />
                    <%-- 前画面情報(オーダー情報)を保持しているファイルパス --%>
                    <asp:HiddenField ID="hdnOrderXMLsaveFile" runat="server" Value="" Visible="False" />
                    <%-- 前画面情報（オーダー情報）のスクロール位置 --%>
                    <asp:HiddenField ID="hdnOrderDispListPosition" runat="server" Value="" Visible="False" />
                    <%-- 当画面情報 --%>
                    <asp:HiddenField ID="hdnThisMapVariant" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnThisViewVariant" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnListEvent" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnListFunc" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnListScrollXPos" runat="server" Value="" />

                    <asp:HiddenField ID="hdnConfirmTitle" runat="server" Value="" Visible="False" />
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
                    <%-- TANKNO VIEW --%>
                    <asp:View ID="vLeftTankNo" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTankNo" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END TANKNO VIEW　 --%>
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
            <div id="divConfirmBoxWrapper" runat="server" enableviewstate="false">
                <div id="divConfirmBox">
                    <div id="divConfirmtitle">
                        <%= Me.hdnConfirmTitle.Value %>
                    </div>
                    <div id="divConfirmBoxButtons">
                        <input id="btnConfirmOk" type="button" value="OK" runat="server" />
                        <input id="btnConfirmCancel" type="button" value="CANCEL" runat="server" onclick="document.getElementById('divConfirmBoxWrapper').style.display = 'none';" />
                    </div>
                    <div id="divConfirmBoxMessageArea">
                        <div><asp:Label ID="lblConfirmMessage" runat="server" Text=""></asp:Label></div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
