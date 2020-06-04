<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBM00000APPROVAL.aspx.vb" Inherits="OFFICE.GBM00000APPROVAL" %>
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
    <link href="~/GB/css/GBM00000APPROVAL.css" rel="stylesheet" />
    <style>
        #divApprovalInfo {
            height:100%;
            width:100%;
            margin:0px;
            position:fixed;
            z-index:50;
            left:0;
            top:0;
        }
        #ifraApprovalInfo {
            width:100%;
            height:100%;
        }
    </style>
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
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>',
                                       '<%= Me.btnExtract.ClientID %>',
                                       '<%= Me.btnExcelDownload.ClientID %>',
                                       '<%= Me.btnReject.ClientID %>',
                                       '<%= Me.btnApproval.ClientID %>',
                                       '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>',
                                       '<%= Me.btnFIRST.ClientID %>',
                                       '<%= Me.btnLAST.ClientID %>'
            ];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewApprovalObjId = '<%= Me.vLeftApprovalObj.ClientID %>';            /* 承認対象 */
            var dblClickObjects = [['<%= Me.txtApprovalObj.ClientID %>', viewApprovalObjId]];
            bindLeftBoxShowEvent(dblClickObjects);

            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();
            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbApprovalObj.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

<%--            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = [['<%= Me.txtPort.ClientID %>'],
                                         ['<%= Me.txtApproval.ClientID %>']];
            bindTextOnchangeEvent(targetOnchangeObjects);
            focusAfterChange();--%>

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
             /* グリッドのクリックイベント紐づけ */
            //bindGridButtonClickEvent();
            /* ブレーカー単票画面の表示 */
            var masterOpenFlg = document.getElementById('hdnMasterViewOpen');
            if (masterOpenFlg !== null) {
                if (masterOpenFlg.value === '1') {
                    masterOpenFlg.value = '0';
                    openMasterWindow();
                }
            }
            /* 共通一覧のスクロールイベント紐づけ */
            bindListCommonEvents('<%= Me.WF_LISTAREA.ClientId %>', '<%= if(IsPostBack = True, "1", "0") %>', true);
            /* 検索ボックス生成 */
            commonCreateSearchArea('searchCondition');
            screenUnlock();
        });
        // ○一覧用処理
        function ListDbClick(obj, LineCnt) {
            if (document.getElementById("hdnSubmit").value == "FALSE") {
                document.getElementById("hdnSubmit").value = "TRUE"
                document.getElementById("hdnListDBclick").value = LineCnt;
                commonDispWait();
                document.forms[0].submit();                             //aspx起動
            };
        };

        // 〇ブレーカー単票オープン処理
        function openMasterWindow() {
            var wholeDiv = document.createElement("div");
            wholeDiv.id = 'divApprovalInfo';
            var iFrameMaster = document.createElement("iframe");
            iFrameMaster.id = 'ifraApprovalInfo';
            iFrameMaster.setAttribute('frameborder','0');
            iFrameMaster.src = 'about:blank';
            wholeDiv.appendChild(iFrameMaster);
            //一旦div及び空のiframeを生成（生成しないと後述の処理が機能しない)
            document.body.appendChild(wholeDiv);

            // iframe内に生成した空ページにFormを作成しブレーカー単票画面にポスト
            iFrameMaster = document.getElementById('ifraApprovalInfo');
            // iframeのロード時イベント設定(発着タブの切り替えなどでポストバック後ロードも担保)
            iFrameMaster.onload = (function (iFrameMaster) {
                return function () {
                    var backBtn = iFrameMaster.contentWindow.document.getElementById('btnBack');
                    if (backBtn === null) {
                        return;
                    }
                    // ブレーカー単票戻るボタンイベントをクリア
                    backBtn.outerHTML = backBtn.outerHTML;
                    backBtn = iFrameMaster.contentWindow.document.getElementById('btnBack');
                    // 戻るボタンを生成したIFrameを削除する用クリックイベント変更
                    backBtn.onclick = (function (iFrameMaster) {
                        return function () {
                            var parentDiv = iFrameMaster.parentNode;
                            parentDiv.parentNode.removeChild(parentDiv);
                        };
                    })(iFrameMaster);

                };
             })(iFrameMaster);

            var mstUrl = document.getElementById('hdnMasterViewUrl').value; // ポストするURL
            var mstValue1 = document.getElementById('hdnSelectedValue1').value; // ポストするコード1
            var mstValue2 = document.getElementById('hdnSelectedValue2').value; // ポストするコード2
            var mstValue3 = document.getElementById('hdnSelectedValue3').value; // ポストするコード3
            var mstValue4 = document.getElementById('hdnSelectedValue4').value; // ポストするコード4
            var mstValue5 = document.getElementById('hdnSelectedValue5').value; // ポストするコード5
            var mstValue6 = document.getElementById('hdnSelectedValue6').value; // ポストするコード6
            var mstValue7 = document.getElementById('hdnSelectedValue7').value; // ポストするコード7

            var frmMst = iFrameMaster.contentWindow.document.createElement("form");
            frmMst.action = mstUrl;
            frmMst.target = "_self";
            frmMst.method = 'post';
            // POSTする引き渡し情報を生成
            var qs = [{ type: 'hidden', name: 'hdnSender', value: '<%= Me.Form.ID %>' },
                      { type: 'hidden', name: 'hdnSelectedValue1', value: mstValue1 },
                      { type: 'hidden', name: 'hdnSelectedValue2', value: mstValue2 },
                      { type: 'hidden', name: 'hdnSelectedValue3', value: mstValue3 },
                      { type: 'hidden', name: 'hdnSelectedValue4', value: mstValue4 },
                      { type: 'hidden', name: 'hdnSelectedValue5', value: mstValue5 },
                      { type: 'hidden', name: 'hdnSelectedValue6', value: mstValue6 },
                      { type: 'hidden', name: 'hdnSelectedValue7', value: mstValue7 }];
            for (var i = 0; i < qs.length; i++) {
                var ol = qs[i];
                var brinput = iFrameMaster.contentWindow.document.createElement("input");

                for (var p in ol) {
                    brinput.setAttribute(p, ol[p]);
                }
                frmMst.appendChild(brinput);
            }
            // 空ウィンドウに作成したformをbodyに追加して、サブミットする。その後、formを削除
            var brbody = iFrameMaster.contentWindow.document.getElementsByTagName("body")[0];
            brbody.appendChild(frmMst);

            iFrameMaster.contentWindow.document.forms[0].submit();
        }

    </script>
</head>
<%-- 基本的にタグ内でのクライアントサイドのJavaScriptのイベント記述はせず、
    ヘッダーにあるwindow.onloadでイベントバインドをします。
    スタイルなども直接記述は極力行わないように
    ※%付きのコメントはHTMLソース表示でもレンダリングされないものです --%>
<body>
    <%--FormIDは適宜変更ください。 --%>
    <form id="GBM00000A" runat="server">
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
                    <input id="btnExtract" type="button" value="絞り込み"  runat="server" tabindex="2" />
                    <input id="btnApproval" type="button" value="承認"  runat="server" tabindex="2" /> 
                    <input id="btnReject" type="button" value="否認"  runat="server" tabindex="2" />
                    <input id="btnExcelDownload" type="button" value="Excelダウンロード"  runat="server" tabindex="2" />
                    <input id="btnBack" type="button" value="終了"  runat="server" tabindex="2" />
                    <div id="btnFIRST" class="firstPage" runat="server"  tabindex="2" ></div>
                    <div id="btnLAST" class="lastPage" runat="server"  tabindex="2" ></div>
                </div>
                <div id="searchCondition">
                </div>
                <div id="divSearchConditionBox">
                    <asp:HiddenField ID="hdnSearchConditionDetailOpenFlg" runat="server" Value="" />
                    <span>
                        <asp:Label ID="lblApprovalObjLabel" runat="server" Text=""></asp:Label>
                        <asp:TextBox ID="txtApprovalObj" runat="server" Text="" TabIndex="1"></asp:TextBox>
                    </span>
                </div>
                <asp:panel id="WF_LISTAREA" runat="server" >
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
                    <asp:HiddenField ID="hdnXMLsaveFile" runat="server" Value="" />  <%--  退避した一覧データのファイル保存先 --%>
                    <asp:HiddenField ID="hdnMouseWheel" runat="server" Value="" />   <%--  マウスホイールのUPorDownを記憶 --%>
                    <asp:HiddenField ID="hdnListPosition" runat="server" Value="" /> <%--  縦スクロールポジション --%>
                    <asp:HiddenField ID="hdnListDBclick" runat="server" Value="" />  <%--  ダブルクリックした行番号を記録 --%>   
                    <%-- 前画面(マスタ検索画面)受け渡し情報 --%>
                    <asp:HiddenField ID="hdnStYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnEndYMD" runat="server" Value="" />
                    <asp:HiddenField ID="hdnMASTERTYPE" runat="server" Value="" />  
                    <asp:HiddenField ID="hdnMASTERTYPEText" runat="server" Value="" />  
                    <asp:HiddenField ID="hdnViewId" runat="server" Value="" />  
                    <%-- 次画面(マスタ編集画面)引き渡し情報 --%>
                    <asp:HiddenField ID="hdnSelectedValue1" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedValue2" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedValue3" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedValue4" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedValue5" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedValue6" runat="server" Value="" />
                    <asp:HiddenField ID="hdnSelectedValue7" runat="server" Value="" /> 
                    <asp:HiddenField ID="hdnSelectedValue8" runat="server" Value="" /> 
                    <asp:HiddenField ID="hdnSelectedValue9" runat="server" Value="" /> 
                    <asp:HiddenField ID="hdnSelectedValue10" runat="server" Value="" /> 
                    <asp:HiddenField ID="hdnXMLsaveFileRet" runat="server" Value="" /> 
                    <asp:HiddenField ID="hdnExtract" runat="server" Value="" />
                    <%-- 画面遷移用メッセージ --%>
                    <asp:HiddenField ID="hdnMsgId" runat="server" Value="" />
                    <%-- マスタ参照画面設定 --%>
                    <asp:HiddenField ID="hdnMasterViewOpen" runat="server" Value="0" EnableViewState="false" />
                    <asp:HiddenField ID="hdnMasterViewUrl"  runat="server" Value="" />
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
                    <%--  　承認対象　 --%>
                    <asp:View id="vLeftApprovalObj" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbApprovalObj" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END 承認 VIEW　 --%>
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
