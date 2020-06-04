<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBM00014CUSTOMERAGENT.aspx.vb" Inherits="OFFICE.GBM00014CUSTOMERAGENT" %>
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
    <link href="css/GBM00014CUSTOMERAGENT.css" rel="stylesheet" type="text/css" />
    <style>
    </style>
    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8"></script>
    <%-- 左ボックスカレンダー使用の場合のスクリプト --%>
    <script type="text/javascript" src='<%= ResolveUrl("~/script/calendar.js") %>'  charset="utf-8"></script>
    <%--個別のスクリプトは以下に記載 --%>
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加

        // 画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            /* ボタンクリックイベントのバインド(適宜追加) */
            /* 絞り込み */
            var targetButtonObjects = [['<%= Me.btnDbUpdate.ClientId  %>'],
                ['<%= Me.btnBack.ClientId  %>'],
                ['<%= Me.btnLeftBoxButtonSel.ClientId  %>'],
                ['<%= Me.btnLeftBoxButtonCan.ClientId  %>']];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            //var viewCalId = '';                      
            //var dblClickObjects = [];
            //bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */
            
            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            //var targetOnchangeObjects = [];
            //bindTextOnchangeEvent(targetOnchangeObjects);
            //focusAfterChange();

            /* 右ボックスの開閉ダブルクリックイベントバインド
               右上透明ボックス、下のメッセージ欄、他がある場合は個別で　*/
            bindDiplayRightBoxEvent();
            /* ヘルプボタン表示のダブルクリックイベントバインド */
            bindFooterShowHelpEvent();

            /* ヘルプ表示処理 */
            openHelpPage(); /* hdnCanHelpOpenに"1"が立たない限り開きません。 */

        screenUnlock();
        });

        // OnLoad用処理（左右Box非表示）
        function InitDisplay() {

            //更新ボタン活性／非活性
            if (document.getElementById('hdnMAPpermitCode').value == "TRUE") {
                //活性
                if (document.getElementById("btnDbUpdate")) {
                    document.getElementById("btnDbUpdate").disabled = "";
                }
                if (document.getElementById("btnDbUpdate")) {
                    document.getElementById("btnDbUpdate").disabled = "";
                }
            } else {
                //非活性 
                if (document.getElementById("btnDbUpdate")) {
                    document.getElementById("btnDbUpdate").disabled = "disabled";
                }
                if (document.getElementById("btnListUpdate")) {
                    document.getElementById("btnListUpdate").disabled = "disabled";
                }
            };
        };

        // // GridView処理（矢印処理）
        //document.onkeydown = function (event) {
        //    if (window.event.keyCode == 38) {
        //        if (document.getElementById("hdnSubmit").value == "FALSE") {
        //            document.getElementById("hdnSubmit").value = "TRUE"
        //            document.getElementById("hdnMouseWheel").value = "-";
        //            document.forms[0].submit();                            //aspx起動
        //        };
        //    };
        //    if (window.event.keyCode == 40) {
        //        if (document.getElementById("hdnSubmit").value == "FALSE") {
        //            document.getElementById("hdnSubmit").value = "TRUE"
        //            document.getElementById("hdnMouseWheel").value = "+";
        //            document.forms[0].submit();                            //aspx起動
        //        };
        //    };
        //};

        //// ダブルクリック処理
        //function Field_DBclick(ActiveViewId, DbClickField) {
        //    if (document.getElementById("hdnSubmit").value == "FALSE") {
        //        document.getElementById("hdnSubmit").value = "TRUE"
        //        document.getElementById('hdnLeftboxActiveViewId').value = ActiveViewId;
        //        document.getElementById('hdnTextDbClickField').value = DbClickField;
        //        document.getElementById('hdnIsLeftBoxOpen').value = "Open";
        //        document.forms[0].submit();                            //aspx起動
        //    };
        //};

        //// テキスト変更処理
        //function TextChange(TextField) {
        //    if (document.getElementById("hdnSubmit").value == "FALSE") {
        //        document.getElementById("hdnSubmit").value = "TRUE"
        //        document.getElementById('hdnOnchangeField').value = TextField;
        //        document.forms[0].submit();                            //aspx起動
        //    };
        //};

        // ドロップ処理（処理抑止）
        function f_dragEventCancel(event) {
            event.preventDefault();  //イベントをキャンセル
        };

        //// GridView処理（マウスホイール処理）
        //function f_MouseWheel(event) {
        //    if (document.getElementById("hdnSubmit").value == "FALSE") {
        //        document.getElementById("hdnSubmit").value = "TRUE"
        //        if (window.event.wheelDelta < 0) {
        //            document.getElementById("hdnMouseWheel").value = "+";
        //        } else {
        //            document.getElementById("hdnMouseWheel").value = "-";
        //        };
        //        document.forms[0].submit();                            //aspx起動
        //    } else {
        //        return false;
        //    };
        //};

        // Repeater行情報取得処理（ラジオボタン切り替え時のON、OFF設定）
        function Rep_ButtonChange(CheckItem, Position) {
            if (document.getElementById("hdnSubmit").value == "FALSE") {
                document.getElementById("hdnSubmit").value = "TRUE"
                document.getElementById("hdnRepUse").value = CheckItem;
                document.getElementById("hdnRepPosition").value = Position;
                document.forms[0].submit();                            //aspx起動
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
    <form id="GBM00014" runat="server">
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
                <%-- ヘッダー部 --%>
                <div  class="headerbox" id="headerbox"
                    ondragstart="f_dragEventCancel(event)"
                    ondrag="f_dragEventCancel(event)"
                    ondragend="f_dragEventCancel(event)" 
                    ondragenter="f_dragEventCancel(event)"
                    ondragleave="f_dragEventCancel(event)" 
                    ondragover="f_dragEventCancel(event)"  
                    ondrop="f_dragEventCancel(event)">
                    <div id="actionButtonsBox" >
                        <a id ="Operation">
                            <asp:Label ID="lblAgent"  runat="server" Text="代理店:" Height="1.1em" Width="5em" CssClass="textLeft"></asp:Label>
                            <asp:Label ID="lblAgentName"  runat="server"  Height="1.1em" Width="10em" CssClass="textLeft"></asp:Label>
                        </a>
                        <a>
                            <input id="btnDbUpdate" type="button" value="DB更新"  runat="server"  />
                            <input id="btnBack" type="button" value="終了"  runat="server"  />
                        </a>
                    </div>             
                </div>
                <%-- 明細部 --%>
                <div  class="detailbox" id="detailbox">
                    <div ondragstart="f_dragEventCancel(event)"
                            ondrag="f_dragEventCancel(event)"
                            ondragend="f_dragEventCancel(event)" 
                            ondragenter="f_dragEventCancel(event)"
                            ondragleave="f_dragEventCancel(event)" 
                            ondragover="f_dragEventCancel(event)"  
                            ondrop="f_dragEventCancel(event)">    

                    <%-- ヘッダ --%>
                    <table id="headerTitle">
                        <tr>
                            <td>
                                <%-- 顧客 --%>
                                <asp:Label ID="lblCustomer"  runat="server" Text="顧客" CssClass="textCenter"></asp:Label>
                            </td>

                            <td>
                                <%-- 使用有無 --%>
                                <asp:Label ID="lblUse"  runat="server" Text="使用有無"  CssClass="textCenter"></asp:Label>
                            </td>

                            <td>
                                <%-- 請求先 --%>
                                <asp:Label ID="lblBill"  runat="server" Text="請求先" CssClass="textCenter" ></asp:Label>
                            </td>

                            <td>
                                <%-- タイプ01 --%>
                                <asp:Label ID="lblType1"  runat="server" Text="タイプ01" CssClass="textCenter" ></asp:Label>
                            </td>

                            <td>
                                <%-- タイプ02 --%>
                                <asp:Label ID="lblType2"  runat="server" Text="タイプ02" CssClass="textCenter" ></asp:Label>
                            </td>

                            <td>
                                <%-- タイプ03 --%>
                                <asp:Label ID="lblType3"  runat="server" Text="タイプ03" CssClass="textCenter" ></asp:Label>
                            </td>

                            <td>
                                <%-- タイプ04 --%>
                                <asp:Label ID="lblType4"  runat="server" Text="タイプ04" CssClass="textCenter" ></asp:Label>
                            </td>

                            <td>
                                <%-- タイプ05 --%>
                                <asp:Label ID="lblType5"  runat="server" Text="タイプ05" CssClass="textCenter" ></asp:Label>
                            </td>
                        </tr>
                    </table>

                   <%-- 明細 --%>
                   <span class="rpDetail" >

                        <asp:Repeater ID="rpDetail" runat="server" >
                            <HeaderTemplate>
                            </HeaderTemplate>

                            <ItemTemplate>
                                <table id="detailTable">
                                    <tr>
                                        <%-- 非表示項目 --%>
                                        <td hidden="hidden">
                                            <asp:TextBox     ID="hdnOrgCode"  runat="server"></asp:TextBox>   <!-- 組織コード　-->
                                            <asp:TextBox     ID="hdnAgentCode"  runat="server"></asp:TextBox>   <!-- 代理店コード　-->
                                            <asp:TextBox     ID="hdnDelFlg"     runat="server"></asp:TextBox>   <!-- 削除フラグ　-->
                                        </td>

                                        <%-- 顧客 --%>
                                        <td>
                                            <asp:label       ID="lblRpCustCode"   runat="server" Height="1.1em" Width="5.5em"  CssClass="textLeft"></asp:label>
                                            <asp:label       ID="lblRpCustName"   runat="server" Height="1.1em" Width="12.0em" CssClass="textLeft"></asp:label>
                                        </td>

                                        <%-- 使用有無 --%>
                                        <td>
                                            <asp:RadioButton ID="rdoRpUseOn"     runat="server" GroupName="rdoRpRepUse"  Width="4em" />
                                            <asp:RadioButton ID="rdoRpUseOff"    runat="server" GroupName="rdoRpRepUse"  Width="5em" />
                                        </td>

                                        <%-- 請求先 --%>
                                        <td>
                                            <asp:TextBox     ID="txtRpBillCode"  runat="server" Height="1.1em" Width="6.5em" CssClass="textLeft"></asp:TextBox>
                                            <asp:Label       ID="lblRpBillName"  runat="server" Height="1.1em" Width="8.0em" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <%-- 取引タイプ01 --%>
                                        <td>
                                            <asp:TextBox     ID="txtRpType01" runat="server" Height="1.1em" Width="4.5em"  CssClass="textLeft"></asp:TextBox>
                                            <asp:Label       ID="lblRpType01" runat="server" Height="1.1em" Width="6em"  CssClass="textLeft"></asp:Label>
                                        </td>

                                        <%-- 取引タイプ02 --%>
                                        <td>
                                            <asp:TextBox     ID="txtRpType02" runat="server" Height="1.1em" Width="4.5em"  CssClass="textLeft"></asp:TextBox>
                                            <asp:Label       ID="lblRpType02" runat="server" Height="1.1em" Width="6em"  CssClass="textLeft"></asp:Label>
                                        </td>

                                        <%-- 取引タイプ03 --%>
                                        <td>
                                            <asp:TextBox     ID="txtRpType03" runat="server" Height="1.1em" Width="4.5em"  CssClass="textLeft"></asp:TextBox>
                                            <asp:Label       ID="lblRpType03" runat="server" Height="1.1em" Width="6em"  CssClass="textLeft"></asp:Label>
                                       </td>

                                        <%-- 取引タイプ04 --%>
                                        <td>
                                            <asp:TextBox     ID="txtRpType04" runat="server" Height="1.1em" Width="4.5em"  CssClass="textLeft"></asp:TextBox>
                                            <asp:Label       ID="lblRpType04" runat="server" Height="1.1em" Width="6em"  CssClass="textLeft"></asp:Label>
                                        </td>

                                        <%-- 取引タイプ05 --%>
                                        <td>
                                            <asp:TextBox     ID="txtRpType05" runat="server" Height="1.1em" Width="4.5em"  CssClass="textLeft"></asp:TextBox>
                                            <asp:Label       ID="lblRpType05" runat="server" Height="1.1em" Width="6em"  CssClass="textLeft"></asp:Label>
                                        </td>
                                    </tr>
                                </table>
                            </ItemTemplate>
                        </asp:Repeater>
                    </span>
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
                    <%-- GridViewマウス操作フィールド --%>
                    <asp:HiddenField ID="hdnMouseWheel" runat="server" Value="" />
                    <%-- List表示位置フィールド --%>
                    <asp:HiddenField ID="hdnListPosition" runat="server" Value="" />
                    <%-- 権限 --%>
                    <asp:HiddenField id="hdnMAPpermitCode" runat="server" Value="" />
                    <%-- Listダブルクリック --%> 
                    <asp:HiddenField id="hdnListDbClick" runat="server" Value="" />                    
                    <%-- Repeater 行位置 --%> 
                    <asp:HiddenField id="hdnRepPosition" runat="server" Value="" />                    
                    <%-- Repeater ラジオボタン --%> 
                    <asp:HiddenField id="hdnRepUse" runat="server" Value="" />      
                    <%-- 一覧情報保存先のファイル名 --%> 
                    <asp:HiddenField id="hdnXMLsaveFile" runat="server" Value="" Visible="false" />
                    <%-- 一覧表用Variant --%> 
                    <asp:HiddenField id="hdnViewVariant" runat="server" Value="" Visible="false" />
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
