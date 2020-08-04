<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00004NEWORDER.aspx.vb" Inherits="OFFICE.GBT00004NEWORDER" %>

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
        #divInputArea {
            padding:10px;
        }
        #divInputArea table {
            border-spacing:1px;
            border:0;
            border-collapse:separate;
            table-layout:fixed;
            width:804px;
        }
        #divInputArea table th {
            background-color: #8BACCD;
            text-align:left;
        }
        #divInputArea table tr:nth-child(1) th:nth-child(n+1) {
            text-align:center;
        }
        #divInputArea table td,
        #divInputArea table th{
            padding:5px;
            white-space:nowrap;
            text-overflow:ellipsis;
            border:1px solid rgb(100,100,100);
            border-spacing:0px;

        }
        #divInputArea table td.empty {
            border-color:transparent;
        }
        #divInputArea table col:nth-child(1) {
            width:100px;
        }
        #divInputArea table col:nth-child(2) {
            width:90px;
        }
        #divInputArea table col:nth-child(3) {
            width:0;
        }
        #divInputArea table col:nth-child(4) {
            width:80px;
        }
        #divInputArea table col:nth-child(5) {
            width:90px;
        }
        #divInputArea table col:nth-child(6) {
            width:0;
        }
        #divInputArea table col:nth-child(7) {
            width:150px;
        }
        #divInputArea table col:nth-child(8) {
            width:120px;
        }
        #divInputArea table col:nth-child(9) {
            width:120px;
        }
        #divInputArea table col:nth-child(10) {
            width:0;
        }
        #divInputArea input[type="text"] {
            height:22px;
            width:calc(100% - 5px);
        }
        #divInputArea #txtTotalInvoiced {
            font-size:20px !important;
            height:50px;
        }
        .textRight{
            text-align:right;
        }
        #lblFillingDate,
        #lblEtd1,#lblEta1,#lblEtd2,#lblEta2 {
            text-decoration:underline;
        }

       /* コード検索の名称表示部 */
       #lblConsigneeText
       {
           color:blue;
           height:22.4px;
       }
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
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加

        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnSave.ClientId %>','<%= Me.btnBack.ClientId  %>', 
                                       '<%= Me.btnLeftBoxButtonSel.ClientId  %>','<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';
            var viewConsignee = '<%= Me.vLeftConsignee.ClientID %>';
            var dblClickObjects = [['<%= Me.txtFillingDate.ClientID %>',viewCalId],
                                   ['<%= Me.txtEta1.ClientID %>', viewCalId],
                                   ['<%= Me.txtEtd1.ClientID %>', viewCalId],
                                   ['<%= Me.txtEta2.ClientID %>',viewCalId],
                                   ['<%= Me.txtEtd2.ClientID %>', viewCalId],
                                   ['<%= Me.txtConsignee.ClientID %>', viewConsignee]];

            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbConsignee.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);
            
            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = ['<%= Me.txtConsignee.ClientID %>']; 
            bindTextOnchangeEvent(targetOnchangeObjects);
            //focusAfterChange();

            /* 右ボックスの開閉ダブルクリックイベントバインド
               右上透明ボックス、下のメッセージ欄、他がある場合は個別で　*/
            bindDiplayRightBoxEvent();
            /* ヘルプボタン表示のダブルクリックイベントバインド */
            bindFooterShowHelpEvent();

            /* ヘルプ表示処理 */
            openHelpPage(); /* hdnCanHelpOpenに"1"が立たない限り開きません。 */
            /* コピーテキスト変更時イベントのバインド */
            bindCalcOrderTotalInvoiced();
            /* カレンダー描画処理 */
            var calValueObj = document.getElementById('<%= Me.hdnCalendarValue.ClientID %>');
            if (calValueObj !== null) {
                /* 日付格納隠し項目がレンダリングされている場合のみ実行 */
                carenda(0);
                setAltMsg(firstAltYMD, firstAltMsg);
            }
            screenUnlock();
        });
        // OrderのTotalInvoiceの計算イベントをCOPYテクキスとボックスのイベントに紐づけ
        function bindCalcOrderTotalInvoiced() {
            var copyObj = document.getElementById('<%= Me.txtCopy.Clientid %>');
            if (copyObj === null) {
                return;
            }
            copyObj._oldvalue = copyObj.value;
            /* ブラーイベントに紐づけ */
            copyObj.addEventListener('focus', function (copyObj) {
                return function () {
                    var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                    if (hdnObj !== null) {
                        hdnObj.value = '';
                    }
                    copyObj._oldvalue = copyObj.value;
                };
            }(copyObj), false);

            copyObj.addEventListener('blur', function (copyObj) {
                return function () {
                    if (copyObj._oldvalue !== copyObj.value) {
                        var hdnObj = document.getElementById('hdnClickButtonIdBeforeBlur');
                        if (hdnObj !== null) {
                            var clickedButtonId = document.getElementById('hdnButtonClick');
                            clickedButtonId.value = hdnObj.value;
                            hdnObj.value = '';
                        }
                        calcOrderTotalInvoiced();
                    }
                };
            }(copyObj), false);

        }
        //  OrderのTotalInvoiceの自動計算
        function calcOrderTotalInvoiced() {
            var copyObj = document.getElementById('<%= Me.txtCopy.Clientid %>');
            var totalTanks = document.getElementById('<%= Me.txtTotalTanks.Clientid %>');
            var totalInvoiced = document.getElementById('<%= Me.txtTotalInvoiced.Clientid %>');

            /* すべてのオブジェクトがそろっているか確認 */
            if (copyObj === null || totalTanks === null || totalInvoiced === null) {
                return;
            }
            totalTanks.value = "";
            totalInvoiced.value = "";
            if (copyObj.value === '') {
                return;
            }
            if (isNaN(copyObj.value)) {
                return;
            }
            var hdnSubmitObj = document.getElementById('hdnSubmit');
            if (hdnSubmitObj.value === 'FALSE') {
                var scrollTop = document.getElementById("hdnBodyScrollTop");
                scrollTop.value = document.getElementById("divContensbox").scrollTop;

                hdnSubmitObj.value = 'TRUE';
                var calcFunctionName = document.getElementById('hdnCalcFunctionName');
                calcFunctionName.value = 'CalcOrderTotalInvoiced';
                document.getElementById("hdnDoublePostCheck").value = document.getElementById("hdnDoublePostCheck").value + 2;
                commonDispWait();
                document.forms[0].submit();
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
    <form id="GBT00004N" runat="server">
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
                    <input id="btnSave" type="button" value="保存"  runat="server"  />
                    <input id="btnBack" type="button" value="戻る"  runat="server"  />
                    <div id="btnFIRST" class="firstPage" runat="server" style="visibility:hidden;"></div>
                    <div id="btnLAST" class="lastPage" runat="server" style="visibility:hidden;"></div>
                </div>
                <div id="divInputArea">
                    <%-- 10列 --%>
                    <table>
                        <colgroup>
                            <col /><col /><col /><col /><col />
                            <col /><col /><col /><col /><col />
                        </colgroup>
                        <tr>
                            <th><asp:Label ID="lblBrId" runat="server" Text="BREAKER No"></asp:Label></th>
                            <td colspan="3"><asp:TextBox ID="txtBrId" runat="server" Enabled="false" Text=""></asp:TextBox></td>
                            <td class="empty" colspan="3"></td>
                            <th><asp:Label ID="lblOffice" runat="server" Text="OFFICE"></asp:Label></th>
                            <th><asp:Label ID="lblSalesPic" runat="server" Text="SALES.PIC"></asp:Label></th>
                            <td class="empty"></td>
                        </tr>
                        <tr>
                            <th><asp:Label ID="lblCopy" runat="server" Text="Copy" CssClass="requiredMark2"></asp:Label></th>
                            <td><asp:TextBox ID="txtCopy" runat="server" CssClass="textRight"></asp:TextBox></td>
                            <td class="empty" colspan="5"></td>
                            <td><asp:TextBox ID="txtOffice" runat="server" Enabled="false"></asp:TextBox></td>
                            <td><asp:TextBox ID="txtSalesPic" runat="server" Enabled="false"></asp:TextBox></td>
                            <td class="empty"></td>
                        </tr>
                        <tr style="display:none;">
                            <td class="empty" colspan="9"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr style="display:none;">
                            <th><asp:Label ID="lblNoOfTanks" runat="server" Text="No Tank OF"></asp:Label></th>
                            <td><asp:TextBox ID="txtNoOfTanks" runat="server" Enabled="false" CssClass="textRight"></asp:TextBox></td>
                            <td class="empty"></td>
                            <th><asp:Label ID="lblTotalTanks" runat="server" Text="Total Tanks"></asp:Label></th>
                            <td><asp:TextBox ID="txtTotalTanks" runat="server" Enabled="false" CssClass="textRight"></asp:TextBox></td>
                            <td colspan="4" class="empty"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr>
                            <td colspan="9" class="empty"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr>
                            <th><asp:Label ID="lblConsignee" runat="server" Text="Consignee"></asp:Label></th>
                            <td><asp:TextBox ID="txtConsignee" runat="server"></asp:TextBox></td>
                            <td><asp:Label ID="lblConsigneeText" runat="server" Text=""></asp:Label></td>
                            <td colspan="6" class="empty"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr>
                            <th><asp:Label ID="lblFillingDate" runat="server" Text="FillingDate"></asp:Label></th>
                            <td><asp:TextBox ID="txtFillingDate" runat="server"></asp:TextBox></td>
                            <td colspan="7" class="empty"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr>
                            <th><asp:Label ID="lblEtd1" runat="server" Text="ETD1" CssClass="requiredMark2"></asp:Label></th>
                            <td><asp:TextBox ID="txtEtd1" runat="server"></asp:TextBox></td>
                            <td colspan="4" class="empty"></td>
                            <th rowspan="2"><asp:Label ID="lblTotalInvoiced" runat="server" Text="TOTAL INVOINCED"></asp:Label></th>
                            <td colspan="2" rowspan="2"><asp:TextBox ID="txtTotalInvoiced" runat="server"  Enabled="false" CssClass="textRight"></asp:TextBox></td>
                            <td rowspan="2" class="empty"></td>
                        </tr>
                        <tr>
                            <th><asp:Label ID="lblEta1" runat="server" Text="ETA1" CssClass="requiredMark2"></asp:Label></th>
                            <td><asp:TextBox ID="txtEta1" runat="server"></asp:TextBox></td>
                            <td colspan="4" class="empty"></td>
                        </tr>
                        <tr>
                            <th><asp:Label ID="lblBookingNo" runat="server" Text="Booking No."></asp:Label></th>
                            <td><asp:TextBox ID="txtBookingNo" runat="server"></asp:TextBox></td>
                            <td colspan="7" class="empty"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr>
                            <th><asp:Label ID="lblVesselName" runat="server" Text="Vessel Name"></asp:Label></th>
                            <td><asp:TextBox ID="txtVesselName" runat="server"></asp:TextBox></td>
                            <td colspan="7" class="empty"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr>
                            <th><asp:Label ID="lblVoyageNo" runat="server" Text="Voyage No."></asp:Label></th>
                            <td><asp:TextBox ID="txtVoyageNo" runat="server"></asp:TextBox></td>
                            <td colspan="7" class="empty"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr id="trEtd2" runat="server">
                            <th><asp:Label ID="lblEtd2" runat="server" Text="ETD2" ></asp:Label></th>
                            <td><asp:TextBox ID="txtEtd2" runat="server"></asp:TextBox></td>
                            <td colspan="7" class="empty"></td>
                            <td class="empty"></td>
                        </tr>
                        <tr id="trEta2" runat="server">
                            <th><asp:Label ID="lblEta2" runat="server" Text="ETA2" ></asp:Label></th>
                            <td><asp:TextBox ID="txtEta2" runat="server"></asp:TextBox></td>
                            <td colspan="7" class="empty"></td>
                            <td class="empty"></td>
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
                    <%-- 右ボックス用情報 --%>                    
                    <asp:HiddenField ID="hdnRightboxOpen" runat="server" Value="" /> <%-- Rightbox 開閉 --%>
                    <asp:HiddenField ID="hdnPrintURL" runat="server" />             <%-- Textbox Print URL --%>
                    <%-- フッターヘルプ関連処理で使用 --%>
                    <asp:HiddenField ID="hdnHelpChange" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCanHelpOpen" runat="server" Value="" />
                    <%-- 当画面固有 --%>
                    <asp:HiddenField ID="hdnCalcFunctionName" runat="server" Value="" />
                    <asp:HiddenField ID="hdnBodyScrollTop" runat="server" Value="" />
                    <asp:HiddenField ID="hdnAgentOrganizer" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnSalesPic" runat="server" Value="" Visible="false" />
                    <%-- 検索結果画面の選択したブレーカー番号 --%>
                    <asp:HiddenField ID="hdnBrId" runat="server" Value="" />
                    <%-- 当画面の保存時に生成したオーダーNo（次画面に引継ぎ） --%>
                    <asp:hiddenfield ID="hdnOrderNo" runat="server" Value="" />
                    <%-- 検索画面（前々画面）の条件保持用フィールド --%>
                    <asp:HiddenField ID="hdnSearchType" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnETDStYMD" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnETDEndYMD" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnETAStYMD" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnETAEndYMD" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnShipper" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnConsignee" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnPortOfLoading" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnPortOfDischarge" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnOffice" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnListId" runat="server" Value="" Visible="false" />
                    <asp:HiddenField ID="hdnReportVariant" runat="server" Value="" Visible="false" />
                    <%-- hdnDebug --%>
                    <asp:HiddenField ID="hdnDoublePostCheck" runat="server" Value="0" EnableViewState="false" ViewStateMode="Disabled" />
                    <%-- 検索結果画面の選択したブレーカータイプ --%>
                    <asp:HiddenField ID="hdnBrType" runat="server" Value="" />
                    <asp:HiddenField ID="hdnDeliveryCountry1" runat="server" Value="" />
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
                    <%-- 【サンプル！】ブレーカー種類選択 VIEW --%>
                    <asp:View ID="vLeftBreakerType" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbBreakerType" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 【サンプル！】ブレーカー種類選択 VIEW　 --%>
                    <%-- 荷受人選択 VIEW　 --%>
                    <asp:View ID="vLeftConsignee" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbConsignee" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 荷受人選択 VIEW　 --%>
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