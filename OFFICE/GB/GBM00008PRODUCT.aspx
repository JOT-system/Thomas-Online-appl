<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBM00008PRODUCT.aspx.vb" Inherits="OFFICE.GBM00008PRODUCT" %>
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
    <link href="~/css/masterCommon.css" rel="stylesheet" type="text/css" />
    <link href="css/GBM00008PRODUCT.css" rel="stylesheet" type="text/css" />
    <%--共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/common.js") %>' type="text/javascript" charset="utf-8"></script>
    <%--マスタ登録で共通利用するJavaScript --%>
    <script src='<%= ResolveUrl("~/script/masterCommon.js") %>' type="text/javascript" charset="utf-8"></script>
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
            var targetButtonObjects = [['<%= Me.btnExtract.ClientId  %>'],
                ['<%= Me.btnLeftBoxButtonSel.ClientId  %>'],
                ['<%= Me.btnLeftBoxButtonCan.ClientId  %>'],
                ['<%= Me.btnDbUpdate.ClientId  %>'],
                ['<%= Me.btnDownload.ClientId  %>'],
                ['<%= Me.btnPrint.ClientId  %>'],
                ['<%= Me.btnBack.ClientId  %>'],
                ['<%= Me.btnListUpdate.ClientId  %>'],
                ['<%= Me.btnClear.ClientId  %>'],
                ['<%= Me.btnFIRST.ClientId  %>'],
                ['<%= Me.btnLAST.ClientId  %>']
                ];

            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewOperationId = '<%= Me.vLeftOperation.ClientID %>';          /* オペレーション */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';                      /* 年月日 */
            var viewDelFlgId = '<%= Me.vLeftDelFlg.ClientID %>';                /* 削除フラグ */
            var dblClickObjects = [['<%= Me.txtOperationEx.ClientID %>', viewOperationId],
            ['<%= Me.txtStYMD.ClientID %>', viewCalId],
            ['<%= Me.txtEndYMD.ClientID %>', viewCalId],
            ['<%= Me.txtDelFlg.ClientID %>', viewDelFlgId]];
            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */
            
            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbOperation.ClientID %>', '3', '1'],
                                           ['<%= Me.lbDelFlg.ClientID %>', '3', '1'],
                                           ['<%= Me.lbUNNO.ClientID %>', '3', '1'],
                                           ['<%= Me.lbEnabled.ClientID %>', '3', '1'],
                                           ['<%= Me.lbHazardClass.ClientID %>', '3', '1'],
                                           ['<%= Me.lbPackingGroup.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);


            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = [['<%= Me.txtDelFlg.ClientID %>']];
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

            setDisplayNameTip();
            /* 共通一覧のスクロールイベント紐づけ */
            bindListCommonEvents('<%= Me.pnlListArea.ClientId %>', '<%= if(IsPostBack = True, "1", "0") %>');

            screenUnlock();
            focusAfterChange();
            // マスタ活性非活性制御
            masterDisableObjects();
            // D&Dイベント紐づけリスト(id:対象のオブジェクトID,kbn,許可拡張子配列(未指定時はすべて))
            var dragDropAreaObjectsList = [
                { id: 'headerbox', kbn: 'DATA_IN', acceptExtentions: ['xlsx'] },
                { id: 'detailStaticbox', kbn: 'DATA_IN', acceptExtentions: ['xlsx'] },
                { id: 'WF_DViewRep1_Area', kbn: 'DATA_IN', acceptExtentions: ['xlsx'] },
                { id: 'WF_DViewRep2_Area', kbn: 'FILE_UP'}
            ];
            bindMasterDragDropEvents(dragDropAreaObjectsList, '<%= ResolveUrl(OFFICE.CommonConst.C_UPLOAD_HANDLER_URL)  %>');
            /* アップロードボタンの設定 */
            addUploadExtention('<%= Me.btnDownload.ClientID %>', 'AFTER', false, 'headerbox', 'Upload');
            /* アップロードボタンの設定 */
            addUploadExtention('<%= Me.btnClear.ClientID %>', 'BEFORE', false, 'WF_DViewRep2_Area', 'File Upload');
            var divContensboxObj = document.getElementById('divContensbox');
            var txtProductObj = document.getElementById('txtProduct');
            var txtProductCodeExObj = document.getElementById('txtProductCodeEx');
            txtProductObj.addEventListener('focus', function () {
                var divContensboxObj = document.getElementById('divContensbox');
                divContensboxObj.scrollLeft = 0;
            });
            txtProductCodeExObj.addEventListener('focus', function () {
                var divContensboxObj = document.getElementById('divContensbox');
                divContensboxObj.scrollLeft = 0;
            });

        });
    </script>
</head>
<%-- 基本的にタグ内でのクライアントサイドのJavaScriptのイベント記述はせず、
    ヘッダーにあるwindow.onloadでイベントバインドをします。
    スタイルなども直接記述は極力行わないように
    ※%付きのコメントはHTMLソース表示でもレンダリングされないものです --%>
<body>
    <%--FormIDは適宜変更ください。 --%>
    <form id="GBM00008" runat="server">
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
                <div  class="headerbox" id="headerbox">
                    <div id="actionButtonsBox" >
                        <div id="extractItem">
                            <asp:Label ID="lblOperationEx" runat="server" Text="操作" ></asp:Label>
                            <asp:TextBox ID="txtOperationEx" runat="server" ></asp:TextBox>
                            <asp:Label ID="lblProductCodeEx" runat="server" Text="積載品コード"></asp:Label>
                            <asp:TextBox ID="txtProductCodeEx" runat="server" ></asp:TextBox>
                            <asp:Label ID="lblProductNameEx" runat="server" Text="積載品名称"></asp:Label>
                            <asp:TextBox ID="txtProductNameEx" runat="server" ></asp:TextBox>
                            <asp:Label ID="lblCasNoEx" runat="server" Text="CAS No."></asp:Label>
                            <asp:TextBox ID="txtCasNoEx" runat="server" ></asp:TextBox>
                        </div>
                        <div id="buttonBox">
                            <input id="btnExtract" type="button" value="絞り込み"  runat="server"  />
                            <input id="btnDbUpdate" type="button" value="DB更新"  runat="server"  />
                            <input id="btnDownload" type="button" value="ﾀﾞｳﾝﾛｰﾄﾞ"  runat="server"  />
                            <input id="btnPrint" type="button" value="一覧印刷"  runat="server"  />
                            <input id="btnBack" type="button" value="終了"  runat="server"  />
                            <div id="btnFIRST" class="firstPage" runat="server"></div>
                            <div id="btnLAST" class="lastPage" runat="server"></div>
                        </div>
                    </div>      
                    <div id="divListArea">
                        <asp:panel id="pnlListArea" runat="server">
                        </asp:panel>
                    </div>
                </div>
                <%-- 明細部 --%>
                <div  class="detailbox" id="detailbox">
                    <div  id="detailStaticbox">
                        <div id="divDetailActionBox">
                            <a><input type="button" id="btnListUpdate" value="表更新" runat="server"/></a>
                            <a><input type="button" id="btnClear" value="クリア" runat="server"/></a>
                            <%-- 選択No --%>
                            <a id="stLineCnt">
                                <asp:Label ID="lblLineCnt" runat="server" Text="選択No" CssClass="textLeft" ></asp:Label>
                                <asp:Label ID="lblLineCntText" runat="server" CssClass="textLeft"></asp:Label>
                            </a>

                            <%-- 申請ID --%>
                            <a id="stApplyID">
                                <asp:Label ID="lblApplyID" runat="server" Text="申請ID" CssClass="textLeft" Font-Bold="True"></asp:Label>
                                <asp:Label ID="lblApplyIDText" runat="server" CssClass="textLeft"></asp:Label>
                            </a>
                            <%-- 警告メッセージ --%>
                            <a id="stWarMsg">
                                <asp:TextBox ID="txtWarMsg" runat="server" ReadOnly="true" TabIndex="-1" BackColor="#cccccc"></asp:TextBox>
                            </a>
                        </div>

                        <div class="detailInputRow">
                            <%-- 積載品コード --%>
                            <a id="stProductCode">
                                <asp:Label ID="lblProduct" runat="server" Text="積載品コード" CssClass="textLeft requiredMark" ></asp:Label>
                                <asp:TextBox ID="txtProduct" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblProductText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>
                        </div>
                        <div class="detailInputRow">
                            <%-- 国連番号 --%>
                            <%--<a id="stUNNO">
                                <asp:Label ID="lblUNNO" runat="server" Text="国連番号" CssClass="textLeft" ></asp:Label>
                                <asp:TextBox ID="txtUNNO" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblUNNOText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>--%>
                            <%-- 会社コード --%>
                            <%--<a id="stCompCode">
                                <asp:Label ID="lblCompCode" runat="server" Text="会社コード" CssClass="textLeft requiredMark"></asp:Label>
                                <asp:TextBox ID="txtCompCode" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblCompCodeText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>--%>
                        </div>
                        <%-- 国コード --%>
                        <%--<a id="stCountryCode">
                            <asp:Label ID="lblCountry" runat="server" Text="国コード" Height="1.1em" Width="10em" CssClass="textLeft requiredMark" Font-Bold="True" Font-Underline="True"></asp:Label>
                            <asp:TextBox ID="txtCountry" runat="server" Height="1.1em" Width="15em" CssClass="textCss"></asp:TextBox>
                            <asp:Label ID="lblCountryText" runat="server" Height="1.1em" Width="15em" CssClass="textLeftLabel"></asp:Label>
                        </a>--%>

                        <%-- 顧客コード --%>
                        <%--<a id="stShipperCode">
                            <asp:Label ID="lblShipper" runat="server" Text="顧客コード" Height="1.1em" Width="10em" CssClass="textLeft requiredMark" Font-Bold="True" Font-Underline="True"></asp:Label>
                            <asp:TextBox ID="txtShipper" runat="server" Height="1.1em" Width="15em" CssClass="textCss"></asp:TextBox>
                            <asp:Label ID="lblShipperText" runat="server" Height="1.1em" Width="15em" CssClass="textLeftLabel"></asp:Label>
                        </a>--%>


                        <div class="detailInputRow">
                            <%-- 有効年月日 --%>
                            <a id="stYMD">
                                <asp:Label ID="lblYMD" runat="server" Text="有効年月日" CssClass="textLeft requiredMark" ></asp:Label>
                                <asp:TextBox ID="txtStYMD" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblTilde" runat="server" Text=" ～ " CssClass="textLeft"></asp:Label>
                                <asp:TextBox ID="txtEndYMD" runat="server" CssClass="textCss"></asp:TextBox>
                            </a>

                            <%-- 削除フラグ --%>
                            <a id="stDelFlg">
                                <asp:Label ID="lblDelFlg" runat="server" Text="削除" CssClass="textLeft requiredMark" ></asp:Label>
                                <asp:TextBox ID="txtDelFlg" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblDelFlgText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>
                        </div>
                        <%-- Dタブ --%>
                        <a id="stDtabProduct" onclick="masterDtabChange('0')">
                            <asp:Label ID="lblDtabProduct" runat="server" Text="積載品情報" ></asp:Label>
                        </a>
                        <a id="stDtabDocument" onclick="masterDtabChange('1')">
                            <asp:Label ID="lblDtabDocument" runat="server" Text="書類（PDF）" ></asp:Label>
                        </a>
                    </div>

                    <%-- DITAIL画面 --%>     
                    <asp:MultiView ID="WF_DetailMView" runat="server">
                        <asp:View ID="WF_DView1" runat="server" >

                            <span class="WF_DViewRep1_Area" id="WF_DViewRep1_Area">
                               <asp:Repeater ID="WF_DViewRep1" runat="server"  >
                                    <HeaderTemplate>
                                        <table>
                                    </HeaderTemplate>

                                    <ItemTemplate>
                                        <tr>

                                        <%-- 非表示項目(左Box処理用・Repeater内行位置) --%>
                                        <td>
                                            <asp:TextBox ID="WF_Rep1_MEISAINO" runat="server"></asp:TextBox>  
                                            <asp:TextBox ID="WF_Rep1_LINEPOSITION" runat="server"></asp:TextBox>  
                                        </td>

                                        <td>
                                            <%-- 項目(名称)　左Side --%>
                                            <asp:Label ID="WF_Rep1_FIELDNM_1" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <asp:Label ID="WF_Rep1_Label1_1" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 項目(記号名)　左Side --%>
                                            <asp:Label ID="WF_Rep1_FIELD_1" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td style="height:1.1em;">
                                            <%-- 値　左Side --%>
                                            <asp:TextBox ID="WF_Rep1_VALUE_1" runat="server" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox>
                                        </td>

                                        <td>
                                            <asp:Label ID="WF_Rep1_Label2_1" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 値（名称）　左Side --%>
                                            <asp:Label ID="WF_Rep1_VALUE_TEXT_1" runat="server" CssClass="textLeftLabel"></asp:Label>
                                        </td>
 
                                        <td>
                                            <%-- スペース --%>
                                            <asp:Label ID="WF_Rep1_Label3_1" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 項目(名称)　中央 --%>
                                            <asp:Label ID="WF_Rep1_FIELDNM_2" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <asp:Label ID="WF_Rep1_Label1_2" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 項目(記号名)　中央 --%>
                                            <asp:Label ID="WF_Rep1_FIELD_2" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 値　中央 --%>
                                            <asp:TextBox ID="WF_Rep1_VALUE_2" runat="server" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox>
                                        </td>

                                        <td>
                                            <asp:Label ID="WF_Rep1_Label2_2" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 値（名称）　中央 --%>
                                            <asp:Label ID="WF_Rep1_VALUE_TEXT_2" runat="server" CssClass="textLeftLabel"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- スペース --%>
                                            <asp:Label ID="WF_Rep1_Label3_2" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 項目(名称)　右Side --%>
                                            <asp:Label ID="WF_Rep1_FIELDNM_3" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <asp:Label ID="WF_Rep1_Label1_3" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 項目(記号名)　右Side --%>
                                            <asp:Label ID="WF_Rep1_FIELD_3" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 値　右Side --%>
                                            <asp:TextBox ID="WF_Rep1_VALUE_3" runat="server" CssClass="WF_TEXTBOX_repCSS" ></asp:TextBox>
                                        </td>

                                        <td>
                                            <asp:Label ID="WF_Rep1_Label2_3" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>

                                        <td>
                                            <%-- 値（名称）　右Side --%>
                                            <asp:Label ID="WF_Rep1_VALUE_TEXT_3" runat="server" CssClass="textLeftLabel"></asp:Label>
                                        </td>

                                        </tr>
                                        
<%--                                        <asp:Label ID="WF_Rep1_LINE" runat="server" Height="1px" Width="100%" style="display:none; border-bottom:solid; border-width:2px; border-color:blue;"></asp:Label>--%>
                                    </ItemTemplate>

                                    <FooterTemplate>
                                        </table>
                                    </FooterTemplate>
             
                                </asp:Repeater>
                            </span>
                        </asp:View>

                        <%-- PDF選択 --%>
                        <asp:View ID="WF_DView2" runat="server">

                            <span class="WF_DViewRep2_Area" id="WF_DViewRep2_Area">
                        
                                <%-- PDF表示選択 --%>
                                <span class="WF_DViewRep2HeaderRow">
                                    <asp:Label ID="WF_Rep2_DispSelect" runat="server" Text="表示選択" CssClass="textLeft"></asp:Label>
                                    <span onchange="PDFselectChange()">
                                        <asp:ListBox ID="WF_Rep2_PDFselect" runat="server"></asp:ListBox>
                                    </span>
                                    <asp:Label ID="WF_Rep2_Desc" runat="server" Text="添付書類を登録する場合は、ここにドロップすること" CssClass="textLeft"></asp:Label>
                                </span>
                                <%-- PDF明細ヘッダー --%>
                                <span class="WF_DViewRep2HeaderRow">
                                    <asp:Label ID="WF_Rep2_PDFfileName" runat="server" Text="ファイル名" CssClass="textLeft"></asp:Label>
                                    <asp:Label ID="WF_Rep2_Under" runat="server" Text="↓↓↓" CssClass="textLeft"></asp:Label>
                                    <asp:Label ID="WF_Rep2_Delete" runat="server" Text="削 除" CssClass="textCenter"></asp:Label>
                                </span>
                                <span class="WF_DViewRep2DataRow">
                                <asp:Repeater ID="WF_DViewRepPDF" runat="server" >
                                    <HeaderTemplate>
                                        <table>
                                    </HeaderTemplate>

                                    <ItemTemplate>
                                        <tr>
                                            <td>
                                                <%-- ファイル記号名称 --%>
                                                <asp:Label ID="WF_Rep_FILENAME" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                            </td>
                                            <td>
                                                <%-- 削除 --%>
                                                <asp:TextBox ID="WF_Rep_DELFLG" runat="server" CssClass="textCenter"></asp:TextBox>
                                            </td>
                                            <td>
                                                <%-- FILEPATH --%>
                                                <asp:Label ID="WF_Rep_FILEPATH" runat="server" CssClass="textLeft" Visible="false"></asp:Label>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </table>
                                    </FooterTemplate>
             
                                </asp:Repeater>
                                </span>
                            </span>
                        </asp:View>
                    </asp:MultiView>
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
                    <%-- DetailBox Mview切替 --%>
                    <asp:HiddenField ID="hdnDTABChange" runat="server" Value="" />
                    <%-- DetailBox PDF内容表示 --%>
                    <asp:HiddenField ID="hdnDTABPDFEXCELdisplay" runat="server" Value="" />
                    <%-- DetailBox PDF表示内容切替 --%>
                    <asp:HiddenField ID="hdnDTABPDFEXCELchange" runat="server" Value="" />
                    <%-- Excel アップロードフィールド --%>
                    <asp:HiddenField ID="hdnListUpload" runat="server" Value="" />
                    <%-- List表示位置フィールド --%>
                    <asp:HiddenField ID="hdnListPosition" runat="server" Value="" />
                    <%-- 権限 --%>
                    <asp:HiddenField id="hdnMAPpermitCode" runat="server" Value="" />
                    <%-- Listダブルクリック --%> 
                    <asp:HiddenField id="hdnListDbClick" runat="server" Value="" />                    
                    <%-- PDF アップロード一覧 --%> 
                    <asp:ListBox ID="hdnListBoxPDF" runat="server"></asp:ListBox>
                    <%-- 一覧情報保存先のファイル名 --%> 
                    <asp:HiddenField id="hdnXMLsaveFile" runat="server" Value="" />
                    <%-- ドラッグアンドドロップ(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnUploadMessage01" Value="ファイルアップロード開始" runat="server" />
                    <asp:HiddenField ID="hdnUploadError01" Value="ファイルアップロードが失敗しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError02" Value="通信を中止しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError03" Value="タイムアウトエラーが発生しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError04" Value="更新権限がありません。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError05" Value="対応外のファイル形式です。" runat="server" />
                    <%-- 前画面引き渡し情報 --%>
                    <asp:HiddenField ID="hdnSelectedCompCode" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedCountryCode" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedCustomerCode" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedProductCode" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedStYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedEndYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnUnNo" runat="server" Value="" />
                    <asp:HiddenField ID="hdnEnabled" runat="server" Value="" />
                    <asp:HiddenField ID="hdnXMLsaveFileRet" runat="server" Value="" /> 
                    <asp:HiddenField ID="hdnExtract" runat="server" Value="" />
                    <asp:HiddenField ID="hdnViewId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedApplyID" runat="server" Value="" />
                    <%-- 詳細ボックス開閉情報保持用 --%>
                    <asp:HiddenField ID="hdnIsHideDetailBox" Value="0" runat="server" />
                    <%-- MAPVARIANT保持用 --%>
                    <asp:HiddenField ID="hdnThisMapVariant" Value="" runat="server" Visible="false" />
                    <%-- 会社コード --%>
                    <asp:HiddenField ID="hdnCompCode" runat="server" Value="" />
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
                    <%--  　オペレーション　 --%>
                    <asp:View id="vLeftOperation" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbOperation" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END オペレーション VIEW　 --%>
                    <%--  　会社コード　 --%>
                    <%--<asp:View id="vLeftCompCode" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCompCode" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View>--%> <%-- END 会社コード VIEW　 --%>
                    <%--  　国コード　 --%>
                    <%--<asp:View id="vLeftCountry" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCountry" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View>--%> <%-- END 国コード VIEW　 --%>
                     <%--  　顧客コード　 --%>
                    <%--<asp:View id="vLeftShipper" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbShipper" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View>--%> <%-- END 顧客コード VIEW　 --%>
                     <%--  　国連番号コード　 --%>
                    <asp:View id="vLeftUNNO" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbUNNO" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 国連番号コード VIEW　 --%>
                     <%--  　削除フラグ　 --%>
                    <asp:View id="vLeftDelFlg" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDelFlg" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 削除フラグ VIEW　 --%>
                    <%--  　有効フラグ　 --%>
                    <asp:View id="vLeftEnabled" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbEnabled" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 有効フラグ VIEW　 --%>
                    <%--  　等級名フラグ　 --%>
                    <asp:View id="vLeftHazardClass" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbHazardClass" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 等級名フラグ VIEW　 --%>
                    <%--  　容器等級名フラグ　 --%>
                    <asp:View id="vLeftPackingGroup" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbPackingGroup" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 容器等級名フラグ VIEW　 --%>
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
