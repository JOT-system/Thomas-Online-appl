<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBM00022ORG.aspx.vb" Inherits="OFFICE.GBM00022ORG" %>
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
    <link href="css/GBM00022ORG.css" rel="stylesheet" type="text/css" />
    <style>
        .UnderLine {
            text-decoration :none !important;
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
                ['<%= Me.btnDownload.ClientId  %>'],          /* ﾀﾞｳﾝﾛｰﾄﾞ */
                ['<%= Me.btnPrint.ClientId  %>'],             /* 一覧印刷 */
                ['<%= Me.btnBack.ClientId  %>'],              /* 終了 */
                ['<%= Me.btnListUpdate.ClientId  %>'],        /* 表更新 */
                ['<%= Me.btnClear.ClientId  %>'],             /* クリア */
                ['<%= Me.btnFIRST.ClientId  %>'],             /* 先頭 */
                ['<%= Me.btnLAST.ClientId  %>']               /* 最終 */
                ];             

            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewOperationId = '<%= Me.vLeftOperation.ClientID %>';          /* オペレーション */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';                      /* 年月日 */
            var viewDelFlgId = '<%= Me.vLeftDelFlg.ClientID %>';                /* 削除フラグ */
            var viewOrgLevelId = '<%= Me.vLeftOrgLevel.ClientID %>';            /* 組織レベル */
            var dblClickObjects = [['<%= Me.txtOperationEx.ClientID %>', viewOperationId],
            ['<%= Me.txtOrgLevelEx.ClientID %>', viewOrgLevelId],
            ['<%= Me.txtStYMD.ClientID %>', viewCalId],
            ['<%= Me.txtEndYMD.ClientID %>', viewCalId],
            ['<%= Me.txtOrgLevel.ClientID %>', viewOrgLevelId],
            ['<%= Me.txtDelFlg.ClientID %>', viewDelFlgId],
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
                                           ['<%= Me.lbOrgLevel.ClientID %>', '3', '1'],
                                           ['<%= Me.lbDelFlg.ClientID %>', '3', '1']
                                           ];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = [
            ['<%= Me.txtOrgLevel.ClientID %>'],
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
            bindListCommonEvents('<%= Me.pnlListArea.ClientId %>','<%= if(IsPostBack = True, "1", "0") %>');

            /* 検索ボックス生成 */
            commonCreateSearchArea('searchCondition');

            screenUnlock();
            // Mouseホイールイベントの除去
            var listObj = document.getElementById('<%= Me.pnlListArea.ClientId %>');
            var mousewheelevent = 'onwheel' in listObj ? 'wheel' : 'onmousewheel' in listObj ? 'mousewheel' : 'DOMMouseScroll';
            listObj.removeEventListener(mousewheelevent, commonListMouseWheel, true);

            focusAfterChange();
            // マスタ活性非活性制御
            masterDisableObjects();
            // D&Dイベント紐づけリスト(id:対象のオブジェクトID,kbn,許可拡張子配列(未指定時はすべて))
            var dragDropAreaObjectsList = [
                { id: 'headerbox', kbn: 'DATA_IN', acceptExtentions: ['xlsx'] },
                { id: 'detailStaticbox', kbn: 'DATA_IN', acceptExtentions: ['xlsx'] },
                { id: 'WF_DViewRep1_Area', kbn: 'DATA_IN', acceptExtentions: ['xlsx'] },
                { id: 'WF_DViewRep2_Area', kbn: 'DATA_IN', acceptExtentions: ['xlsx'] }
            ];
            bindMasterDragDropEvents(dragDropAreaObjectsList, '<%= ResolveUrl(OFFICE.CommonConst.C_UPLOAD_HANDLER_URL)  %>');
            /* アップロードボタンの設定 */
            addUploadExtention('<%= Me.btnDownload.ClientID %>', 'AFTER', false, 'headerbox', 'Upload');
        });
    </script>
</head>
<%-- 基本的にタグ内でのクライアントサイドのJavaScriptのイベント記述はせず、
    ヘッダーにあるwindow.onloadでイベントバインドをします。
    スタイルなども直接記述は極力行わないように
    ※%付きのコメントはHTMLソース表示でもレンダリングされないものです --%>
<body>
    <%--FormIDは適宜変更ください。 --%>
    <form id="GBM00022" runat="server">
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
                        <div id="buttonBox">
                            <input id="btnExtract" type="button" value="絞り込み" runat="server" tabindex="2"  />
                            <input id="btnDbUpdate" type="button" value="DB更新" runat="server" tabindex="2"  />
                            <input id="btnDownload" type="button" value="ﾀﾞｳﾝﾛｰﾄﾞ" runat="server" tabindex="2"  />
                            <input id="btnPrint" type="button" value="一覧印刷" runat="server" tabindex="2"  />
                            <input id="btnBack" type="button" value="終了"  runat="server" tabindex="2"  />
                            <div id="btnFIRST" class="firstPage" runat="server" tabindex="2" ></div>
                            <div id="btnLAST" class="lastPage" runat="server" tabindex="2" ></div>
                        </div>
                        <div id="searchCondition">
                        </div>
                        <div id="divSearchConditionBox">
                            <asp:HiddenField ID="hdnSearchConditionDetailOpenFlg" runat="server" Value="" />
                            <span>
                                <asp:Label ID="lblOperationEx" runat="server" Text=""></asp:Label>
                                <asp:TextBox ID="txtOperationEx" runat="server" Text="" TabIndex="1"></asp:TextBox>
                            </span>
                            <span>
                                <asp:Label ID="lblMOrgCodeEx" runat="server" Text=""></asp:Label>
                                <asp:TextBox ID="txtMOrgCodeEx" runat="server" Text="" TabIndex="1"></asp:TextBox>
                            </span>
                            <span>
                                <asp:Label ID="lblOrgLevelEx" runat="server" Text=""></asp:Label>
                                <asp:TextBox ID="txtOrgLevelEx" runat="server" Text="" TabIndex="1"></asp:TextBox>
                            </span>
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

                        </div>
                        <div class="detailInputRow">
                            <%-- 有効年月日 --%>
                            <a id="stYMD">
                                <asp:Label ID="lblYMD" runat="server" Text="有効年月日" CssClass="textLeft requiredMark" ></asp:Label>
                                <asp:TextBox ID="txtStYMD" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblTilde" runat="server" Text=" ～ " CssClass="textLeft"></asp:Label>
                                <asp:TextBox ID="txtEndYMD" runat="server" CssClass="textCss"></asp:TextBox>
                            </a>
                        </div>
                        <div class="detailInputRow">
                            <%-- 組織コード --%>
                            <a id="stOrgCode">
                                <asp:Label ID="lblOrgCode" runat="server" Text="組織コード" CssClass="textLeft UnderLine requiredMark"></asp:Label>
                                <asp:TextBox ID="txtOrgCode" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblOrgCodeText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>
                            <%-- 組織レベル --%>
                            <a id="stOrgLevel">
                                <asp:Label ID="lblOrgLevel" runat="server" Text="組織レベル" CssClass="textLeft requiredMark"></asp:Label>
                                <asp:TextBox ID="txtOrgLevel" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblOrgLevelText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>
                        </div>
                        <div class="detailInputRow">
                            <%-- 管理組織コード --%>
                            <a id="stMOrgCode">
                                <asp:Label ID="lblMOrgCode" runat="server" Text="管理組織コード" CssClass="textLeft UnderLine requiredMark" ></asp:Label>
                                <asp:TextBox ID="txtMOrgCode" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblMOrgCodeText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>
                            <%-- 削除フラグ --%>
                            <a id="stDelFlg">
                                <asp:Label ID="lblDelFlg" runat="server" Text="削除フラグ" CssClass="textLeft requiredMark"></asp:Label>
                                <asp:TextBox ID="txtDelFlg" runat="server" CssClass="textCss"></asp:TextBox>
                                <asp:Label ID="lblDelFlgText" runat="server" CssClass="textLeftLabel"></asp:Label>
                            </a>
                        </div>

                        <%-- Dタブ --%>
                        <a id="stDtabTrader" onclick="masterDtabChange('0')">
                            <asp:Label ID="lblDtabOrganization" runat="server" Text="組織情報" ></asp:Label>
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

                                        <td>
                                            <%-- 値　左Side --%>
                                            <asp:TextBox ID="WF_Rep1_VALUE_1" runat="server" CssClass="WF_TEXTBOX_repCSS" style="visibility:hidden;"></asp:TextBox>
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
                                            <asp:TextBox ID="WF_Rep1_VALUE_2" runat="server" CssClass="WF_TEXTBOX_repCSS" style="visibility:hidden;"></asp:TextBox>
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
                                            <asp:TextBox ID="WF_Rep1_VALUE_3" runat="server" CssClass="WF_TEXTBOX_repCSS" style="visibility:hidden;"></asp:TextBox>
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
                    <%-- Excel アップロードフィールド --%>
                    <asp:HiddenField ID="hdnListUpload" runat="server" Value="" />
                    <%-- List表示位置フィールド --%>
                    <asp:HiddenField ID="hdnListPosition" runat="server" Value="" />
                    <%-- 権限 --%>
                    <asp:HiddenField id="hdnMAPpermitCode" runat="server" Value="" />
                    <%-- Listダブルクリック --%> 
                    <asp:HiddenField id="hdnListDbClick" runat="server" Value="" />
                    <%-- 一覧情報保存先のファイル名 --%> 
                    <asp:HiddenField id="hdnXMLsaveFile" runat="server" Value="" />
                    <%-- 前画面選択条件 --%>
                    <asp:HiddenField ID="hdnSelectedStYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedEndYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedOrgCountry" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedOrgOffice" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedOrgPort" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedOrgDepot" runat="server" Value="" />
                    <asp:HiddenField ID="hdnViewId" runat="server" Value="" />
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
                    <%--  　オペレーション　 --%>
                    <asp:View id="vLeftOperation" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbOperation" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END オペレーション VIEW　 --%>
                    <%--  　組織コード　 --%>
                    <asp:View id="vLeftOrgCode" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbOrgCode" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 組織コード VIEW　 --%>
                    <%--  　組織レベル　 --%>
                    <asp:View id="vLeftOrgLevel" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbOrgLevel" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 組織レベル VIEW　 --%>
                    <%--  　管理組織コード　 --%>
                    <asp:View id="vLeftMOrgCode" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbMOrgCode" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 管理組織コード VIEW　 --%>
                    <%--  削除フラグ --%>
                    <asp:View id="vLeftDelFlg" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDelFlg" runat="server" CssClass="leftViewContents"></asp:ListBox>
                        </div>
                    </asp:View> <%-- END 削除フラグ VIEW --%>
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
