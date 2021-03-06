﻿<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBM00006TANK.aspx.vb" Inherits="OFFICE.GBM00006TANK" %>
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
    <link href="css/GBM00006TANK.css" rel="stylesheet" type="text/css" />
    <style>
        #lblTankNo {
            text-decoration: none;
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
                ['<%= Me.btnLeftBoxButtonSel.ClientId  %>'],  /* 選択 */
                ['<%= Me.btnLeftBoxButtonCan.ClientId  %>'],  /* キャンセル */
                ['<%= Me.btnDbUpdate.ClientId  %>'],          /* DB更新 */
                ['<%= Me.btnDownload.ClientId  %>'],          /*  ﾀﾞｳﾝﾛｰﾄﾞ */
                ['<%= Me.btnPrint.ClientId  %>'],             /* 一覧印刷 */
                ['<%= Me.btnBack.ClientId  %>'],              /* 終了 */
                ['<%= Me.btnListUpdate.ClientId  %>'],        /* 表更新 */
                ['<%= Me.btnClear.ClientId  %>'],             /* クリア */
                ['<%= Me.btnFIRST.ClientId  %>'],             /* 先頭 */
                ['<%= Me.btnLAST.ClientId  %>']              /* 最終 */
                ];             

            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewOperationId = '<%= Me.vLeftOperation.ClientID %>';          /* オペレーション */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';                      /* 年月日 */
            var viewCompCodeId = '<%= Me.vLeftCompCode.ClientID %>';            /* 会社コード */
            var viewProperty = '<%= Me.vLeftProperty.ClientID %>';              /* 所属 */
            var viewDelFlgId = '<%= Me.vLeftDelFlg.ClientID %>';                /* 削除フラグ */
            var dblClickObjects = [['<%= Me.txtOperationEx.ClientID %>', viewOperationId],
            ['<%= Me.txtStYMD.ClientID %>', viewCalId],
            ['<%= Me.txtEndYMD.ClientID %>', viewCalId],
            ['<%= Me.txtCompCode.ClientID %>', viewCompCodeId],
            ['<%= Me.txtProperty.ClientID %>', viewProperty],
            ['<%= Me.txtDelFlg.ClientID %>', viewDelFlgId]
            ];
            bindLeftBoxShowEvent(dblClickObjects);
            /* 手入力変更時のイベント */
            
            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbOperation.ClientID %>', '3', '1'],
                                           ['<%= Me.lbCompCode.ClientID %>', '3', '1'],
                                           ['<%= Me.lbProperty.ClientID %>', '3', '1'], 
                                           ['<%= Me.lbDelFlg.ClientID %>', '3', '1'],
                                           ['<%= Me.lbLMOF.ClientID %>', '3', '1'],
                                           ['<%= Me.lbInspectType.ClientID %>', '3', '1'],
                                           ['<%= Me.lbLeaseStat.ClientID %>', '3', '1'],
                                           ['<%= Me.lbRepairStat.ClientID %>', '3', '1'],
                                           ['<%= Me.lbJapFireApproved.ClientID %>', '3', '1'],
                                           ['<%= Me.lbManufacture.ClientID %>', '3', '1'],
                                           ['<%= Me.lbStruct.ClientID %>', '3', '1'],
                                           ['<%= Me.lbUsDotApproved.ClientID %>', '3', '1'],
                                           ['<%= Me.lbDischarge.ClientID %>', '3', '1'],
                                           ['<%= Me.lbFootValue.ClientID %>', '3', '1'],
                                           ['<%= Me.lbBottomOutlet.ClientID %>', '3', '1'],
                                           ['<%= Me.lbTopOutlet.ClientID %>', '3', '1'],
                                           ['<%= Me.lbAirInlet.ClientID %>', '3', '1'],
                                           ['<%= Me.lbBaffles.ClientID %>', '3', '1'],
                                           ['<%= Me.lbBurstDisc.ClientID %>', '3', '1'],
                                           ['<%= Me.lbTherm.ClientID %>', '3', '1'],
                                           ['<%= Me.lbMlSeal.ClientID %>', '3', '1'],
                                           ['<%= Me.lbNewTankPort.ClientID %>', '3', '1'],
                                           ['<%= Me.lbNewTankActy.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = [
            ['<%= Me.txtCompCode.ClientID %>'],
            ['<%= Me.txtProperty.ClientID %>'],
            ['<%= Me.txtDelFlg.ClientID %>']
            ];
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
        });
    </script>
</head>
<%-- 基本的にタグ内でのクライアントサイドのJavaScriptのイベント記述はせず、
    ヘッダーにあるwindow.onloadでイベントバインドをします。
    スタイルなども直接記述は極力行わないように
    ※%付きのコメントはHTMLソース表示でもレンダリングされないものです --%>
<body>
    <%--FormIDは適宜変更ください。 --%>
    <form id="GBM00006" runat="server">
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
                            <asp:Label ID="lblTankNoEx" runat="server" Text="タンク番号" ></asp:Label>
                            <asp:TextBox ID="txtTankNoEx" runat="server" ></asp:TextBox>
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
                        <asp:panel id="pnlListArea" runat="server" >
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
                                <asp:Label ID="lblApplyID" runat="server" Text="申請ID" CssClass="textLeft" ></asp:Label>
                                <asp:Label ID="lblApplyIDText" runat="server" CssClass="textLeft"></asp:Label>
                            </a>

                            <%-- 会社コード --%>
                            <a id="stCompCode">
                                <asp:Label ID="lblCompCode" runat="server" Text="会社コード" CssClass="textLeft requiredMark" Visible="false"></asp:Label>
                                <asp:TextBox ID="txtCompCode" runat="server" CssClass="textCss" Visible="false"></asp:TextBox>
                                <asp:Label ID="lblCompCodeText" runat="server" CssClass="textLeftLabel" Visible="false"></asp:Label>
                            </a>
                        </div>
                        <div class="detailInputRow">
                            <%-- 有効年月日 --%>
                            <a id="stYMD">
                                <asp:Label ID="lblYMD" runat="server" Text="有効年月日" CssClass="textLeft requiredMark"></asp:Label>
                                <asp:TextBox ID="txtStYMD" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblTilde" runat="server" Text=" ～ " CssClass="textLeft"></asp:Label>
                                <asp:TextBox ID="txtEndYMD" runat="server" CssClass="textCss"></asp:TextBox>
                            </a>
                        </div>
                        <div class="detailInputRow">
                            <%-- 所属 --%>
                            <a id="stProperty">
                                <asp:Label ID="lblProperty" runat="server" Text="所属" CssClass="textLeft requiredMark"></asp:Label>
                                <asp:TextBox ID="txtProperty" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblPropertyText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>
                            <%-- 削除フラグ --%>
                            <a id="stDelFlg">
                                <asp:Label ID="lblDelFlg" runat="server" Text="削除フラグ" CssClass="textLeft requiredMark"></asp:Label>
                                <asp:TextBox ID="txtDelFlg" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblDelFlgText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>
                        </div>
                        <div class="detailInputRow">
                            <%-- タンク番号 --%>
                            <a id="stTankNo">
                                <asp:Label ID="lblTankNo" runat="server" Text="タンク番号" CssClass="textLeft requiredMark" ></asp:Label>
                                <asp:TextBox ID="txtTankNo" runat="server" CssClass="textCss"></asp:TextBox>
                                <%--<asp:Label ID="lblTankNoText" runat="server" Height="1.1em" Width="15em" CssClass="textLeftLabel"></asp:Label>--%>
                            </a>
                        </div>
                        <%-- Dタブ --%>
                        <a id="stDtabTank" onclick="masterDtabChange('0')">
                            <asp:Label ID="lblDtabTank" runat="server" Text="タンク情報"></asp:Label>
                        </a>
                        <a id="stDtabDocument" onclick="masterDtabChange('1')">
                            <asp:Label ID="lblDtabDocument" runat="server" Text="書類（PDF）" ></asp:Label>
                        </a>
                    </div>

                    <%-- DITAIL画面 --%>     
                    <asp:MultiView ID="WF_DetailMView" runat="server">
                        <asp:View ID="WF_DView1" runat="server" >

                            <span class="WF_DViewRep1_Area" id="WF_DViewRep1_Area">
                               <asp:Repeater ID="WF_DViewRep1" runat="server">
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
                                        <td>
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
                                        <td style="height:1.1em;">
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
                                            <asp:TextBox ID="WF_Rep1_VALUE_3" runat="server" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:Label ID="WF_Rep1_Label2_3" runat="server" Text="" CssClass="textLeft"></asp:Label>
                                        </td>
                                        <td>
                                            <%-- 値（名称）　右Side --%>
                                            <asp:Label ID="WF_Rep1_VALUE_TEXT_3" runat="server" CssClass="textLeftLabel"></asp:Label>
                                        </td>
                                        </tr>
                                        
                                        <%--<asp:Label ID="WF_Rep1_LINE" runat="server" Height="1px" Width="100%" style="display:none; border-bottom:solid; border-width:2px; border-color:blue;"></asp:Label>--%>
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
                                    <%--<asp:Label ID="WF_Rep2_DispSelect" runat="server" Text="表示選択" CssClass="textLeft"></asp:Label>
                                    <span onchange="PDFselectChange()">
                                        <asp:ListBox ID="WF_Rep2_PDFselect" runat="server"></asp:ListBox>
                                    </span>--%>
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
                    <asp:HiddenField id="hdnXMLsaveFile" runat="server" Value="" Visible="false" />
                    <%-- 前画面選択条件 --%>
                    <asp:HiddenField ID="hdnSelectedTankNo" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedStYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedEndYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnViewId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedApplyID" runat="server" Value="" />                    
                    <asp:HiddenField ID="hdnMsgId" runat="server" Value="" />
                    <asp:HiddenField ID="hdnExtractTankNo" runat="server" Value="" />
                    <asp:HiddenField ID="hdnExtractApp" runat="server" Value="" />
                    <asp:HiddenField ID="hdnStYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnEndYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnOrderNo" runat="server" Value="" />
                    <asp:HiddenField ID="hdnTankNo" runat="server" Value="" />
                    <asp:HiddenField ID="hdnPrevViewID" runat="server" Value="" />
                    <%-- 前画面(タンクステータス)情報保持 --%>
                    <asp:HiddenField ID="hdnListSortValueGBT00006RWF_LISTAREA" runat="server" Value="" />
                    <%-- ドラッグアンドドロップ(メッセージ 英語/日本語切替対応用) --%>
                    <asp:HiddenField ID="hdnUploadMessage01" Value="ファイルアップロード開始" runat="server" />
                    <asp:HiddenField ID="hdnUploadError01" Value="ファイルアップロードが失敗しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError02" Value="通信を中止しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError03" Value="タイムアウトエラーが発生しました。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError04" Value="更新権限がありません。" runat="server" />
                    <asp:HiddenField ID="hdnUploadError05" Value="対応外のファイル形式です。" runat="server" />
                    <%-- 詳細ボックス開閉情報保持用 --%>
                    <asp:HiddenField ID="hdnIsHideDetailBox" Value="0" runat="server" />
                    <%-- MAPVARIANT保持用 --%>
                    <asp:HiddenField ID="hdnThisMapVariant" Value="0" runat="server" Visible="false" />
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
                    <%--  会社コード --%>
                    <asp:View id="vLeftCompCode" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbCompCode" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 会社コード VIEW --%>
                    <%--  所属 --%>
                    <asp:View id="vLeftProperty" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbProperty" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 所属 VIEW --%>
                    <%--  所有形態（自社、リース他） --%>
                    <asp:View id="vLeftLMOF" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLMOF" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 所有形態（自社、リース他） VIEW --%>
                    
                    <%--  検査日種別 --%>
                    <asp:View id="vLeftInspectType" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbInspectType" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END フラグ全般 VIEW --%>
                    <%--  削除フラグ --%>
                    <asp:View id="vLeftDelFlg" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDelFlg" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 削除フラグ VIEW --%>
                    
                    <%--  　リース　 --%>
                    <asp:View id="vLeftLeaseStat" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbLeaseStat" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リース  VIEW　 --%>
                    <%--  　リペア　 --%>
                    <asp:View id="vLeftRepairStat" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbRepairStat" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END リペア  VIEW　 --%>

                    <%--  　JP消防検査有無　 --%>
                    <asp:View id="vLeftJapFireApproved" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbJapFireApproved" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END JP消防検査有無  VIEW　 --%>

                    <%--  　製造メーカー　 --%>
                    <asp:View id="vLeftManufacture" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbManufacture" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 製造メーカー  VIEW　 --%>

                    <%--  　追加構造　 --%>
                    <asp:View id="vLeftStruct" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbStruct" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 追加構造  VIEW　 --%>

                    <%--  　荷重試験実施の有無　 --%>
                    <asp:View id="vLeftUsDotApproved" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbUsDotApproved" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 荷重試験実施の有無  VIEW　 --%>

                    <%--  　液出し口の位置　 --%>
                    <asp:View id="vLeftDischarge" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDischarge" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 液出し口の位置  VIEW　 --%>

                    <%--  　フート弁の仕様　 --%>
                    <asp:View id="vLeftFootValue" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbFootValue" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END フート弁の仕様  VIEW　 --%>

                    <%--  　下部液出し口の仕様　 --%>
                    <asp:View id="vLeftBottomOutlet" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbBottomOutlet" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 下部液出し口の仕様  VIEW　 --%>

                    <%--  　上部積込口の仕様　 --%>
                    <asp:View id="vLeftTopOutlet" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTopOutlet" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 上部積込口の仕様  VIEW　 --%>

                    <%--  　エアラインのバルブの仕様　 --%>
                    <asp:View id="vLeftAirInlet" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbAirInlet" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END エアラインのバルブの仕様  VIEW　 --%>

                    <%--  　防波板の有無　 --%>
                    <asp:View id="vLeftBaffles" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbBaffles" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 防波板の有無  VIEW　 --%>

                    <%--  　破裂板の有無　 --%>
                    <asp:View id="vLeftBurstDisc" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbBurstDisc" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 破裂板の有無  VIEW　 --%>

                    <%--  　温度計の種類　 --%>
                    <asp:View id="vLeftTherm" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTherm" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 温度計の種類  VIEW　 --%>

                    <%--  　マンホールパッキンの種類　 --%>
                    <asp:View id="vLeftMlSeal" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbMlSeal" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END マンホールパッキンの種類  VIEW　 --%>

                    <%--  　マル関ステッカー貼付　 --%>
                    <asp:View id="vLeftMarukanSticker" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbMarukanSticker" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END マンホールパッキンの種類  VIEW　 --%>

                    <%--  　New Tank 港　 --%>
                    <asp:View id="vLeftNewTankPort" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbNewTankPort" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END New Tank 港  VIEW　 --%>

                    <%--  　New Tank Acty　 --%>
                    <asp:View id="vLeftNewTankActy" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbNewTankActy" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END New Tank Acty  VIEW　 --%>

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
