<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00014BL.aspx.vb" Inherits="OFFICE.GBT00014BL"  %>
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
    <link href="~/GB/css/GBT00014BL.css" rel="stylesheet" type="text/css" />
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
    <script type="text/javascript" src='<%= ResolveUrl("~/GB/script/GBT00014BL.js") %>'  charset="utf-8"></script>
    <script type="text/javascript">
        // 必要な場合適宜関数、処理を追加
        // ダウンロード処理
        function f_ExcelPrint() {
            // リンク参照
            var printUrlObj = document.getElementById("hdnPrintURL");
            if (printUrlObj === null) {
                return;
            }
            window.open(printUrlObj.value, "view", "scrollbars=yes,resizable=yes,status=yes");
            printUrlObj.value = '';
        };
        function f_PDFPrint() {
            var objPrintUrl = document.getElementById("hdnPrintURL");
            if (objPrintUrl === null) {
                return;
            }
            // リンク参照
            window.open(objPrintUrl.value, "view", "_blank");
        }

        function f_DownLoad() {
            // リンク参照
            var dwnUrlObj = document.getElementById("hdnZipURL");
            if (dwnUrlObj === null) {
                return;
            }
            window.open(dwnUrlObj.value, "view", "scrollbars=yes,resizable=yes,status=yes");
            dwnUrlObj.value = '';
        }; 
        // ドロップ処理（ドラッグドロップ入力）
        function f_dragEvent(e,kbn) {
            e.preventDefault();
            e.stopPropagation();
            commonDispWait();
            var footerMsg = document.getElementById("lblFooterMessage");
            if (document.getElementById('hdnMAPpermitCode').value == "TRUE") {
                footerMsg.textContent = '<%= Me.hdnUploadMessage01.Value %>';
                footerMsg.style.color = "blue";
                footerMsg.style.fontWeight = "bold";

                // ドラッグされたファイル情報を取得
                var files = e.dataTransfer.files;

                // 送信用FormData オブジェクトを用意
                var fd = new FormData();

                // ファイル情報を追加する
                
                for (var i = 0; i < files.length; i++) {
                    if (kbn == "FILE_UP") {
                        fd.append("files", files[i]);

                    } else {

                        /* 拡張子xlsxの場合 */
                        var reg = new RegExp("^.*\.xlsx$");
                        if (files[i].name.toLowerCase().match(reg)) {
                            fd.append("files", files[i]);
                        } else {
                            footerMsg.textContent = '<%= Me.hdnUploadError05.Value %>';
                            footerMsg.style.color = "red";
                            footerMsg.style.fontWeight = "bold";
                            commonDispWait();
                            return;
                        }
                    }
               }

                // XMLHttpRequest オブジェクトを作成
                var xhr = new XMLHttpRequest();

                // ドロップファイルによりURL変更
                // 「POST メソッド」「接続先 URL」を指定
                xhr.open("POST",'<%= ResolveUrl("~/COH0001FILEUP.ashx") %>' , false)

                // イベント設定
                // ⇒XHR 送信正常で実行されるイベント
                xhr.onload = function (e) {
                    if (e.currentTarget.status == 200) {

                        if (kbn == "FILE_UP") {
                            document.getElementById("hdnListUpload").value = "FILE_LOADED";
                        } else {
                            document.getElementById("hdnListUpload").value = "XLS_LOADED";
                        }
                        document.forms[0].submit();                             //aspx起動
                    } else {
                        footerMsg.textContent = '<%= Me.hdnUploadError01.Value %>';
                        footerMsg.style.color = "red";
                        footerMsg.style.fontWeight = "bold";
                        commonDispWait();
                    }
                };

                // ⇒XHR 送信ERRで実行されるイベント
                xhr.onerror = function (e) {
                    footerMsg.textContent = '<%= Me.hdnUploadError01.Value %>';
                    footerMsg.style.color = "red";
                    footerMsg.style.fontWeight = "bold";
                    commonDispWait();
                };

                // ⇒XHR 通信中止すると実行されるイベント
                xhr.onabort = function (e) {
                    footerMsg.textContent = '<%= Me.hdnUploadError02.Value %>';
                    footerMsg.style.color = "red";
                    footerMsg.style.fontWeight = "bold";
                    commonDispWait();
                };

                // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
                xhr.ontimeout = function (e) {
                    footerMsg.textContent = '<%= Me.hdnUploadError03.Value %>';
                    footerMsg.style.color = "red";
                    footerMsg.style.fontWeight = "bold";
                    commonDispWait();
                };

                // 「送信データ」を指定、XHR 通信を開始する
                xhr.send(fd);
            } else {
                footerMsg.textContent = '<%= Me.hdnUploadError04.Value %>';
                footerMsg.style.color = "red";
                footerMsg.style.fontWeight = "bold";
                commonDispWait();
            }
                
        }

        // ドロップ処理（処理抑止）
        function f_dragEventCancel(event) {
            event.preventDefault();  //イベントをキャンセル
        };

        // ○画面ロード時処理(すべてのレンダリングが終了後実行されます。)
        window.addEventListener('DOMContentLoaded', function () {
            screenLock();
            //changeCommonInfoArea();
            /* ボタンクリックイベントのバインド(適宜追加) */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>', '<%= Me.btnOutputExcel.ClientId %>',
                                       '<%= Me.btnPrint.ClientId %>',
                                       '<%= Me.btnPDFPrint.ClientId %>',
                                       '<%= Me.btnSave.ClientId  %>',
                                       '<%= Me.btnOutputFile.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* タブクリックイベントのバインド */
            var targetTabObjects = ['<%= Me.tabBL.ClientID %>','<%= Me.tabTank.ClientID %>','<%= Me.tabOther.ClientID %>','<%= Me.tabFileUp.ClientID %>']

            bindTabClickEvent(targetTabObjects);
            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';
            <%--var viewFrtAndCrg = '<%= Me.vLeftFrtAndCrg.ClientID %>';--%>
            var viewCountry = '<%= Me.vLeftCountry.ClientID %>';
            var viewBlType = '<%= Me.vLeftBlType.ClientID %>';
            var viewCarBlType = '<%= Me.vLeftCarBlType.ClientID %>';
            var viewDemAcct = '<%= Me.vLeftDemAcct.ClientID %>';
            var viewCarrier = '<%= Me.vLeftCarrier.ClientID %>';
            var dblClickObjects = [['<%= Me.txtPaymentPlace.ClientID %>', viewCountry],
                                   ['<%= Me.txtBlIssuePlace.ClientID %>', viewCountry],
                                   ['<%= Me.txtAnIssuePlace.ClientID %>', viewCountry],
                                   ['<%= Me.txtBlType.ClientID %>', viewBlType],
                                   ['<%= Me.txtCarBlType.ClientID %>', viewCarBlType],
                                   ['<%= Me.txtDemAcct.ClientID %>', viewDemAcct],
                                   ['<%= Me.txtLdnDate.ClientID %>', viewCalId],
                                   ['<%= Me.txtCarrier.ClientID %>', viewCarrier]];
            
            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbFrtAndCrg.ClientID %>', '3', '1'],
                                           ['<%= Me.lbBlType.ClientID %>', '3', '1'],
                                           ['<%= Me.lbCarBlType.ClientID %>', '3', '1'],
                                           ['<%= Me.lbDemAcct.ClientID %>', '3', '1'],
                                           ['<%= Me.lbCountry.ClientID %>', '3', '1'],
                                           ['<%= Me.lbTerm.ClientID %>', '3', '1'],
                                           ['<%= Me.lbEorF.ClientID %>', '3', '1'],
                                           ['<%= Me.lbCarrier.ClientID %>', '3', '1']
                                           ];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = ['<%= Me.txtShipRateEx.ClientID %>',
                                         '<%= Me.txtShipRateIn.ClientID %>',
                                         '<%= Me.txtPaymentPlace.ClientID %>',
                                         '<%= Me.txtBlIssuePlace.ClientID %>',
                                         '<%= Me.txtAnIssuePlace.ClientID %>',
                                         '<%= Me.txtBlType.ClientID %>',
                                         '<%= Me.txtCarBlType.ClientID %>',
                                         '<%= Me.txtNoOfBl.ClientID %>',
                                         '<%= Me.txtCarrier.ClientID %>'];
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
            ///* 共通オーナー情報の開閉イベントの紐づけ */
            //document.getElementById("spnShowCommonInfo").onclick = (
            //    function () {
            //        commonInfoAreaClick();
            //        changeCommonInfoArea();
            //    }
            //);
            /* 費用項目グリッドのイベントバインド */
            bindCostRowEvents();
            /* 左ボックスの備考ダブルクリックイベント */
            bindSpnRightRemarksDbClick();
            /* ファイルドラッグ＆ドロップのイベントバインド */
            var dragDropObj = document.getElementById('divContainer');
            if (dragDropObj !== null) {
                dragDropObj.addEventListener("dragstart", f_dragEventCancel, false);
                dragDropObj.addEventListener("drag", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragend", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragenter", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragleave", f_dragEventCancel, false);
                dragDropObj.addEventListener("dragover", f_dragEventCancel, false);
                dragDropObj.addEventListener("drop", f_dragEvent, false);
            }

            var scrollTop = document.getElementById("hdnBodyScrollTop");
            if (scrollTop.value !== "") {
                document.getElementById("divContensbox").scrollTop = scrollTop.value;
                scrollTop.value = "";
            }

            /* アップロードボタンの設定(File-Infoタブ)複数選択可能 */
            addUploadExtention('<%= Me.btnOutputFile.ClientID %>', 'AFTER', true, 'dViewRepArea', 'Upload');
            /* アップロードボタンの設定 */
            addUploadExtention('<%= Me.btnOutputExcel.ClientID %>', 'AFTER', false, 'divContainer');
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
    <form id="GBT00014" runat="server">
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

                <div id="commonInfo" runat="server">
                    <table class="infoTable common">
                        <colgroup>
                            <col /><col /><col /><col /><col />
                            <col /><col /><col /><col /><col />
                            <col /><col /><col /><col /><col />
                        </colgroup>
                        <tr id="trBlHeadInfoRow1" runat="server">
                            <td class="headerCell" colspan="2" >
                                <asp:Label ID="lblBlInfoHeader" runat="server" Text="B/L-Info"></asp:Label>
                            </td>
                        </tr>
                        <tr id="trBlHeadInfoRow2" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td  class="textRightCell">
                                <asp:Label ID="lblOrderNoTitle" runat="server" Text=""></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblOrderNo" runat="server" Text=""></asp:Label>
                            </td>
                            <td colspan="5">
                                &nbsp;
                            </td>
                        </tr>
                        <tr id="trBlHeadInfoRow3" runat="server">
                            <td>
                                &nbsp;
                            </td>
                            <td  class="textRightCell">
                                <asp:Label ID="lblBlNoTitle" runat="server" Text=""></asp:Label>
                            </td>
                            <td>
                                <asp:Label ID="lblBlNo" runat="server" Text=""></asp:Label>
                            </td>
                            <td colspan="5">
                                &nbsp;
                            </td>
                        </tr>
                    </table>
                </div>
                <!-- タブ表示エリア -->
                <div id="tabsBox">
                    <div class="tabLeftSide">
                        <div id="tabBL" runat="server" >BL-Info</div>
                        <div id="tabTank" runat="server">Tank-Info</div>
                        <div id="tabOther" runat="server">Other</div>
                        <div id="tabFileUp" runat="server">File-Info</div>
                    </div>
                    <%--<div class="tabRightSide">
                        <span id="spnShowCommonInfo" >Show CommonInfo</span>
                        <!-- 共通情報を表示するか(0=表示しない,1(Default)=表示する) -->
                        <asp:HiddenField ID="hdnIsShowCommonInfo" runat="server" Value="1" />
                    </div>--%>
                </div>
                <div id="actionButtonsBox" runat="server">
                    <span id="spnActButtonBox" runat="server" visible="true">
                        <input id="btnOutputExcel" type="button" value="エクセル出力" runat="server" />
                        <input id="btnPrint" type="button" value="帳票出力" runat="server" />
                        <input id="btnPDFPrint" type="button" value="帳票出力" runat="server" />
                        <input id="btnSave" type="button" value="保存" runat="server" />
                    </span>
                    <input id="btnOutputFile" type="button" value="ﾀﾞｳﾝﾛｰﾄﾞ"  runat="server"  />
                    <input id="btnBack" type="button" value="戻る"  runat="server"  />
                </div>
                <div id="divBlDetailInfo" runat="server">
                <table>
                    <colgroup>
                        <col /><col /><col /><col /><col /><col />
                        <col /><col /><col /><col /><col /><col />
                    </colgroup>
                    <tr>
                        <th>
                            <asp:Label ID="lblShipper" runat="server" Text="Shipper" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtShipperText" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblConsignee" runat="server" Text="Consignee" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtConsigneeText" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblNotifyParty" runat="server" Text="NotifyParty" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtNotifyPartyText" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblCargoRelease" runat="server" Text="Contact for cargo release" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtCargoReleaseText" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblPlaceOfReceipt" runat="server" Text="Place of Receipt" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtPlaceOfReceipt" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblPortOfLoading" runat="server" Text="Port of Loading" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtPortOfLoading" runat="server" Text=""></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblPortOfDischarge" runat="server" Text="Port of Discharge" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtPortOfDischarge" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblPlaceOfDelivery" runat="server" Text="Place of Delivery" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtPlaceOfDelivery" runat="server" Text=""></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblPreCarriageBy" runat="server" Text="Pre-carriage by" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtPreCarriageBy" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblVessel" runat="server" Text="Vessel" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtVessel" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblVoyNo" runat="server" Text="Voy No" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtVoyNo" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblFnlDest" runat="server" Text="Final Destination" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtFnlDest" runat="server" Text=""></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblShipRateEx" runat="server" Text="" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtShipRateEx" runat="server" Text="" class="textRight"></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblShipRateIn" runat="server" Text="" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtShipRateIn" runat="server" Text="" class="textRight"></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblBookingNo" runat="server" Text="Booking No" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtBookingNo" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblNoOfBl" runat="server" Text="No of B/L" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtNoOfBl" runat="server" Text=""></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblBlType" runat="server" Text="BL Type" ></asp:Label>
                        </th>
                        <td>
                            <asp:TextBox ID="txtBlType" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            <asp:Label ID="lblBlTypeText" runat="server" Text="" ></asp:Label>
                        </td>
                        <%--<th>
                            <asp:Label ID="lblBlNo" runat="server" Text="B/L No" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtBlNo" runat="server" Text=""></asp:TextBox>
                        </td>--%>
                        <th>
                            <asp:Label ID="lblCarBlType" runat="server" Text="CarrierBL Type" ></asp:Label>
                        </th>
                        <td>
                            <asp:TextBox ID="txtCarBlType" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            <asp:Label ID="lblCarBlTypeText" runat="server" Text="" ></asp:Label>
                        </td>
                        <th>
                            <asp:Label ID="lblCarrierBlNo" runat="server" Text="Carrier B/L No" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtCarrierBlNo" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblCarrier" runat="server" Text="Carrier" ></asp:Label>
                        </th>
                        <td>
                            <asp:TextBox ID="txtCarrier" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            <asp:Label ID="lblCarrierText" runat="server" Text="" ></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblDemAcct" runat="server" Text="Demu For The Acct Of" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtDemAcct" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblBlPlaceDateIssue" runat="server" Text="Place and Date of issue" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtBlPlaceDateIssue" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td colspan="6">
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblFreightCharges" runat="server" Text="Freight and Charges" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtFreightCharges" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblDecOfGd" runat="server" Text="Description Of Goods" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtDecOfGdText" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblMarksNumbers" runat="server" Text="Marks And Numbers" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtMarksNumbers" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                        <td colspan="6">
                        </td>
                    </tr>
                    <%--経由地１、２の船社・船名記入領域--%>
                    <tr>
                        <th>
                            <asp:Label ID="lblVsl2nd" runat="server" Text="2nd Vessel" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtVsl2nd" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblVoy2nd" runat="server" Text="2nd Voyage" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtVoy2nd" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblVsl3rd" runat="server" Text="3rd Vessel" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtVsl3rd" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblVoy3rd" runat="server" Text="3rd Voyage" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtVoy3rd" runat="server" Text=""></asp:TextBox>
                        </td>
                    </tr>

                </table>
                </div>
                <div id="divTankDetailInfo" runat="server">
                <table>
                    <colgroup>
                        <col /><col /><col /><col /><col /><col />
                        <col /><col /><col /><col /><col /><col />
                    </colgroup>
                    <tr>
                        <th>
                            <asp:Label ID="lblGrossSummary" runat="server" Text="総合計"></asp:Label>
                        </th>
                        <td colspan="2">
                            <input id="iptGrossSummary" type="text" disabled="true" class="aspNetDisabled textRight" runat="server" />
                        </td>
                        <th>
                            <asp:Label ID="lblNetSummary" runat="server" Text="正味合計"></asp:Label>
                        </th>
                        <td colspan="2">
                            <input id="iptNetSummary" type="text" disabled="true" class="aspNetDisabled textRight" runat="server" />
                        </td>
                        <th>
                            <asp:Label ID="lblMeasurement" runat="server" Text="Measurement"></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtMeasurement" runat="server" Text="" class="textRight" ></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblNoOfPackage" runat="server" Text="No of Package" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <input id="iptNoOfPackage" type="text" disabled="true" class="aspNetDisabled textRight" runat="server" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="12">
                            <asp:GridView ID="gvDetailInfo" runat="server" AutoGenerateColumns="False" ShowFooter="False" GridLines="None"  CellSpacing="-1" ShowHeaderWhenEmpty="True" >
                                <Columns >
                                    <%--<asp:TemplateField HeaderText="">
                                        <ItemTemplate>--%>
                                            <%--<asp:Panel ID="pnlDeleteButtonArea" runat="server" Visible='<%# IIf(Eval("IsAddedCost") = "1", "True", "False") %>'>
                                                <input id='btnDeleteCostItem_<%# Container.DataItemIndex %>' type="button" value='<%= Me.hdnDispDeleteBtnText.Value %>' data-uniqueindex='<%#Eval("UniqueIndex") %>' <%= If(Me.gvDetailInfo.Enabled = False, "class=""aspNetDisabled"" disabled=""disabled""", "") %> />
                                            </asp:Panel>--%>
                                            <%--<asp:HiddenField ID="hdnUniqueIndex" runat="server"  Value='<%# Bind("UniqueIndex") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>--%>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="TankNo">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtTankNo" runat="server" text='<%# Bind("TankNo") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' enabled="false"></asp:TextBox>
                                            <asp:HiddenField ID="hdnUniqueIndex" runat="server"  Value='<%# Bind("UniqueIndex") %>' />
                                            <asp:HiddenField ID="hdnOrderNo" runat="server"  Value='<%# Bind("OrderNo") %>' />
                                            <asp:HiddenField ID="hdnTankSeq" runat="server"  Value='<%# Bind("TankSeq") %>' />
                                            <asp:HiddenField ID="hdnShipRateEx" runat="server"  Value='<%# Bind("ShipRateEx") %>' />
                                            <asp:HiddenField ID="hdnShipRateIn" runat="server"  Value='<%# Bind("ShipRateIn") %>' />
                                            <asp:HiddenField ID="hdnTareWeight" runat="server"  Value='<%# Bind("TareWeight") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="Seq">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtDispSeq" runat="server" text='<%# Bind("DispSeq") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="TankType">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtTankType" runat="server" text='<%# Bind("TankType") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="SealNo1">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtSealNo1" runat="server" text='<%# Bind("SealNo1") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="SealNo2">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtSealNo2" runat="server" text='<%# Bind("SealNo2") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="SealNo3">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtSealNo3" runat="server" text='<%# Bind("SealNo3") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="SealNo4">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtSealNo4" runat="server" text='<%# Bind("SealNo4") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="GrossWeight">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtGrossWeight" runat="server" text='<%# Bind("GrossWeight") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' enabled="false" ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="NetWeight">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtNetWeight" runat="server" text='<%# Bind("NetWeight") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="EmptyOrFull">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtEmptyOrFull" runat="server" text='<%# Bind("EmptyOrFull") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="" HeaderStyle-CssClass="NoOfPackage">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtNoOfPackage" runat="server" text='<%# Bind("NoOfPackage") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <%--<asp:TemplateField HeaderText="">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtMeasurement" runat="server" text='<%# Bind("Measurement") %>' CssClass="textRight" data-uniqueindex='<%#Eval("UniqueIndex") %>' ></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>--%>
                                </Columns>
                            </asp:GridView>
                        </td>                        
                    </tr>
                </table>
                </div>
                <div id="divOtherDetailInfo" runat="server">
                <table>
                    <colgroup>
                        <col /><col /><col /><col /><col /><col />
                        <col /><col /><col /><col /><col /><col />
                    </colgroup>
                    <tr>
                        <th>
                            <asp:Label ID="lblRevenueTons" runat="server" Text="Revenue Tons" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtRevenueTons" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblRate" runat="server" Text="Rate" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtRate" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblPer" runat="server" Text="Per" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtPer" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblPrepaid" runat="server" Text="Prepaid" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtPrepaid" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblCollect" runat="server" Text="Collect" ></asp:Label>
                        </th>
                        <td colspan="5">
                            <asp:TextBox ID="txtCollect" runat="server" Text="" TextMode="MultiLine"></asp:TextBox>
                        </td>
                        <td colspan="6">
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblMerDecValue" runat="server" Text="Merchant's Declared" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtMerDecValue" runat="server" Text="" class="textRight" ></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblPaymentPlace" runat="server" Text="Payment Place" ></asp:Label>
                        </th>
                        <td>
                            <asp:TextBox ID="txtPaymentPlace" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            <asp:Label ID="lblPaymentPlaceText" runat="server" Text="" ></asp:Label>
                        </td>
                        <th>
                            <asp:Label ID="lblBlIssuePlace" runat="server" Text="B/L Issue Place" ></asp:Label>
                        </th>
                        <td>
                            <asp:TextBox ID="txtBlIssuePlace" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            <asp:Label ID="lblBlIssuePlaceText" runat="server" Text="" ></asp:Label>
                        </td>
                        <th>
                            <asp:Label ID="lblAnIssuePlace" runat="server" Text="A/N Issue Place" ></asp:Label>
                        </th>
                        <td >
                            <asp:TextBox ID="txtAnIssuePlace" runat="server" Text=""></asp:TextBox>
                        </td>
                        <td>
                            <asp:Label ID="lblAnIssuePlaceText" runat="server" Text="" ></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <th>
                            <asp:Label ID="lblLdnVessel" runat="server" Text="Vessel" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtLdnVessel" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblLdnPol" runat="server" Text="Port of Loading" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtLdnPol" runat="server" Text=""></asp:TextBox>
                        </td>
                        <%--<td>
                            <asp:Label ID="lblLdnPolText" runat="server" Text="" ></asp:Label>
                        </td>--%>
                        <th>
                            <asp:Label ID="lblLdnDate" runat="server" Text="Date" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtLdnDate" runat="server" Text=""></asp:TextBox>
                        </td>
                        <th>
                            <asp:Label ID="lblLdnBy" runat="server" Text="By" ></asp:Label>
                        </th>
                        <td colspan="2">
                            <asp:TextBox ID="txtLdnBy" runat="server" Text=""></asp:TextBox>
                        </td>
                    </tr>
                </table>
                </div>
                <div id="divFileUpInfo" runat="server">
                <table class="infoTable fileup" >
                    <colgroup>
                        <col /><col /><col /><col /><col /><col />
                        <col /><col /><col /><col /><col /><col />
                    </colgroup>
                    <tbody>
                    <tr>
                        <td colspan="12">
                            <asp:MultiView ID="mltvFileUp" runat="server" ActiveViewIndex="0">
                                <asp:View ID="vFileUp" runat="server">
                                    <span id="dViewRepArea" style="position:absolute;min-height:33em;left:1.5em;right:1.5em;overflow-x:hidden;overflow-y:auto;background-color:white;background-color: rgb(220,230,240);table-layout: auto" 
                                        ondragstart="f_dragEventCancel(event)"
                                        ondrag="f_dragEventCancel(event)"
                                        ondragend="f_dragEventCancel(event)" 
                                        ondragenter="f_dragEventCancel(event)"
                                        ondragleave="f_dragEventCancel(event)" 
                                        ondragover="f_dragEventCancel(event)"  
                                        ondrop="f_dragEvent(event,'FILE_UP')">

                                        <asp:Label ID="lblDropDesc" runat="server" Text="To register attached documents, drop it here" Height="1.1em" CssClass="textLeft" Font-Bold="true" Font-Size="Medium" style="position:relative;top:0.5em;left:30.5em;"></asp:Label>
                                        <asp:Label ID="lblUnder" runat="server" Text="↓↓↓" Height="1.1em" CssClass="textLeft" Font-Bold="true" Font-Size="Medium" style="position:absolute;top:1.6em;left:40.5em;"></asp:Label>
                                       <br />

                                        <asp:Label ID="lblFileName" runat="server" Text="File Name" Height="1.1em" Width="8em" CssClass="textLeft" style="position:relative;top:0.7em;left:5.0em;"></asp:Label>

                                        <asp:Label ID="lblDelete" runat="server" Text="Delete" Height="1.1em" Width="8em" CssClass="textCenter" style="position:relative;top:0.7em;left:72.5em;"></asp:Label>
                                        <br />

                                        <span style="position:absolute;top:3.5em;left:1.3em;height:390px;min-width:90em;overflow-x:hidden;overflow-y:auto;background-color:white;border:1px solid black;">
                                        <asp:Repeater ID="dViewRep" runat="server" >
                                            <HeaderTemplate>
                                            </HeaderTemplate>

                                            <ItemTemplate>
                                                <table style="">
                                                <tr style="">

                                                <td style="height:1.0em;width:40em;">
                                                <%-- ファイル記号名称 --%>
                                                <a>　</a>
                                                <asp:Label ID="lblRepFileName" runat="server" Text="" Height="1.0em" Width="77em" CssClass="textLeft"></asp:Label>
                                                </td>

                                                <td style="height:1.0em;width:10em;">
                                                <%-- 削除 --%>
                                                <asp:TextBox ID="txtRepDelFlg" runat="server" Height="1.0em" Width="10em" CssClass="textCenter"></asp:TextBox>
                                                </td>

                                                <td style="height:1.0em;width:10em;" hidden="hidden">
                                                <%-- FILEPATH --%>
                                                <asp:Label ID="lblRepFilePath" runat="server" Height="1.0em" Width="10em" CssClass="textLeft"></asp:Label>
                                                </td>

                                                </tr>
                                                </table>
                                            </ItemTemplate>

                                            <FooterTemplate>
                                            </FooterTemplate>
             
                                        </asp:Repeater>
                                        </span>
                                    </span>
                                </asp:View>
                            </asp:MultiView>
                        </td>
                        <td>
                            &nbsp;
                        </td>
                    </tr>
                    </tbody>
                </table>
                </div>

                <div class="emptybox"></div>

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
                    <%-- Download用 --%>
                    <asp:HiddenField ID="hdnZipURL" runat="server" />
                    <%-- フッターヘルプ関連処理で使用 --%>
                    <asp:HiddenField ID="hdnHelpChange" runat="server" Value="" />
                    <asp:HiddenField ID="hdnCanHelpOpen" runat="server" Value="" />
                    <%-- 画面固有 --%>
                    <asp:HiddenField ID="hdnDispDeleteBtnText" value="Delete" runat="server" />
                    <asp:HiddenField ID="hdnDispLeftBoxCostCode" value="コード" runat="server" />
                    <asp:HiddenField ID="hdnDispLeftBoxItem1" value="項目1" runat="server" />
                    <%--<asp:HiddenField ID="hdnDispLeftBoxItem2" value="項目2" runat="server" />--%>
                    <asp:HiddenField ID="hdnIsTrilateral" runat="server" /> <%-- 三国間か？ "1"=三国間 それ以外=二国 --%>
                    <asp:HiddenField ID="hdnSelectedTabId" runat="server" Value="" /> <%-- 選択中のタブ --%>
                    <asp:HiddenField ID="hdnIsViewOnlyPopup" runat="server" Value="0" /> <%-- 参照のみのポップアップ表示か？ "1":ポップアップ表示,"0":それ以外 --%>
                    <asp:HiddenField ID="hdnBackUrl" value="" runat="server" />
                    <asp:HiddenField ID="hdnBodyScrollTop" value="" runat="server" />
                    <asp:HiddenField ID="hdnLoadCountry" value="" runat="server" />
                    <%-- 費用用 --%>
                    <asp:HiddenField ID="hdnDelteCostUniqueIndex" value="" runat="server" />
                    <asp:HiddenField ID="hdnCurrentUnieuqIndex" value="" runat="server" />
                    <%-- 備考欄ボックス --%>
                    <asp:HiddenField ID="hdnRemarkboxOpen" value="" runat="server" />
                    <asp:HiddenField ID="hdnRemarkboxField" value="" runat="server" />
                    <asp:HiddenField ID="hdnRemarkboxFieldName" value="" runat="server" />
                    <%-- RemarkEmptyMessage --%>
                    <asp:HiddenField ID="hdnRemarkEmptyMessage" value="" runat="server" />
                    <asp:HiddenField ID="hdnRightBoxRemarkField" value="" runat="server" />
                    <asp:HiddenField ID="hdnRightBoxClose" value="" runat="server" />
                    <%-- ドラッグアンドドロップ --%>
                    <asp:HiddenField ID="hdnMAPpermitCode" Value="TRUE" runat="server" />
                    <asp:HiddenField ID="hdnListUpload" Value="" runat="server" />
                    <%-- ドラッグアンドドロップ(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnUploadMessage01" Value="ファイルアップロード開始" runat="server" />
                    <asp:HiddenField ID="hdnUploadError01" Value="ファイルアップロードが失敗しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError02" Value="通信を中止しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError03" Value="タイムアウトエラーが発生しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError04" Value="更新権限がありません。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError05" Value="対応外のファイル形式です。" runat="server" />
                    <%-- 当画面の計算処理POST(設定した名称の関数を実行) --%>
                    <asp:HiddenField ID="hdnCalcFunctionName" Value="" runat="server" />
                    <%-- 前画面情報保持用 --%>
                    <asp:HiddenField ID="hdnOrderNo" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnWhichTrans" runat="server" Value="" Visible="False" /> 
                    <asp:HiddenField ID="hdnMsgId" runat="server" Value="" Visible="False" /> 
                    <asp:HiddenField ID="hdnSearchType" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnBlIssued" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETDStYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETDEndYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnShipper" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnConsignee" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnPort" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnProduct" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnCarrier" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnVsl" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnOffice" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnCountry" runat="server" Value=""  Visible="False" />
                    <asp:HiddenField ID="hdnDepartureArrival" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETAStYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnETAEndYMD" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnPortOfLoading" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnPortOfDischarge" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnReportVariant" runat="server" Value="" Visible="False" />

                    <%-- アップロード一覧 --%> 
                    <asp:ListBox ID="hdnListBox" runat="server"></asp:ListBox>
                    <%-- 前画面XMLファイル保持用 --%>
                    <asp:HiddenField ID="hdnXMLsaveFileRet" runat="server" Value="" />
                    <%-- DetailBox File内容表示 --%>
                    <asp:HiddenField ID="hdnFileDisplay" runat="server" Value="" />
                    <%-- 前画面(承認画面)保持用 --%>
                    <asp:HiddenField ID="hdnPrevViewID" runat="server" Value="" />
                    <%-- 更新保持用 --%>
                    <asp:HiddenField ID="hdnTmstmp" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnUpdYmd" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnUpdUser" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnUpdTermId" runat="server" Value="" Visible="False" />
                    <asp:HiddenField ID="hdnProductName" runat="server" Value="" Visible="False" />
                    <%-- 遷移したMAPVARI保持 --%>
                    <asp:HiddenField ID="hdnThisMapVariant" runat="server" Value="" Visible="false" />
                    <%-- BLNO --%>
                    <%--<asp:HiddenField ID="hdnBLNo" runat="server" Value="" Visible="false" />--%>
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
                    <%--  　Freight and Charges　 --%>
                    <asp:View id="vLeftFrtAndCrg" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbFrtAndCrg" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END Freight and Charges VIEW　 --%>
                    <%-- TERM選択 VIEW　 --%>
                    <asp:View ID="vLeftTerm" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTerm" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END TERM選択 VIEW　 --%>
                    <%-- EorF --%>
                    <asp:View ID="vLeftEorF" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbEorF" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END EorF　 --%>
                    <%-- Country --%>
                    <asp:View ID="vLeftCountry" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCountry" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Country　 --%>
                    <%-- B/L Type --%>
                    <asp:View ID="vLeftBlType" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbBlType" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END B/L Type　 --%>
                    <%-- Carrier B/L Type --%>
                    <asp:View ID="vLeftCarBlType" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCarBlType" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Carrier B/L Type　 --%>
                    <%-- Carrier --%>
                    <asp:View ID="vLeftCarrier" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCarrier" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Carrier　 --%>
                    <%-- Dem Acct --%>
                    <asp:View ID="vLeftDemAcct" runat="server">
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDemAcct" runat="server"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END Dem Acct　 --%>
                    <%--  　削除フラグ　 --%>
                    <asp:View id="vLeftDelFlg" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDelFlg" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 削除フラグ VIEW　 --%>
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
                        <%-- 右テキスト表示内容選択(メモ or エラー詳細) --%>
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
                    <div>
                        <br/>
                    </div>
                    <div>
                        <%-- 右リストの帳票用説明文 --%>
                        <asp:Label ID="lblRightListPrintDiscription" runat="server" Text=""></asp:Label>
                    </div>
                    <div>
                        <%-- 右リスト帳票用 --%>
                        <asp:ListBox ID="lbRightListPrint" runat="server">
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
