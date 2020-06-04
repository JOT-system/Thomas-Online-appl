<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GBT00011SELECT.aspx.vb" Inherits="OFFICE.GBT00011SELECT" %>

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
    <style>
       /* テキスト表示情報 */
       #itemTable{
           table-layout:fixed; 
           margin-top :5px;
           margin-left: 65px;
           width:785px;
	   }

       /* 共通セル設定 */
       #itemTable td{
           padding:10px;
	   }
	   
       /* 1列目幅 */
	   #itemTable td:nth-child(1){
           width:170px;
           height:22.4px;
           font-weight:bold;
           overflow:hidden;
           text-decoration:underline;
           color: black ;
           font-size: small;
           vertical-align:middle;
           text-align:left;
	   }
	   	   
       /* 2列名幅 */
	   #itemTable td:nth-child(2){
           width:50px;
           height:22.4px;
           overflow:hidden;
           color: black ;
           font-size: small;
           vertical-align:middle;
           text-align:left;
	   }
	   
       /* 3列名幅 */
	   #itemTable td:nth-child(3){
		   width:200px;
           height:22.4px;
           overflow:hidden;
           color: blue ;
           font-size: small;
           vertical-align:middle;
	    }
	   
       /* 4列名幅 */
	   #itemTable td:nth-child(4){
           text-align:right;
		   width:100px;
           height:22.4px;
           overflow:hidden;
           color: black ;
           font-size: small;
           vertical-align:middle;
	    }

	    /* 5列名幅 */
	   #itemTable td:nth-child(5){
           width:200px;
           height:22.4px;
	    }

        /* 開始日 */
	   #txtStYMD{
           height:22.4px;
	    }

        /* 終了日 */
	   #txtEndYMD{
           height:22.4px;
	    }

        /* タンク番号 */
	   #txtTankNo{
           height:22.4px;
	    }

       /* デポ */
	   #txtDepot{
           height:22.4px;
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
            /* 実行 */
            var targetButtonObjects = ['<%= Me.btnEnter.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);
            /* 終了 */
            var targetButtonObjects = ['<%= Me.btnBack.ClientId  %>', '<%= Me.btnLeftBoxButtonSel.ClientId  %>',
                                       '<%= Me.btnLeftBoxButtonCan.ClientId  %>'];
            bindButtonClickEvent(targetButtonObjects);

            /* 左ボックス表示/非表示制御(hdnIsLeftBoxOpenが'Open'の場合表示) */
            displayLeftBox();

            /* 左ボックス表示ダブルクリックイベントのバインド */
            var viewCalId = '<%= Me.vLeftCal.ClientID %>';                      /* 年月日 */
            var viewTankNo = '<%= Me.vLeftTank.ClientID %>';                    /* タンク番号 */
            var viewDepot = '<%= Me.vLeftDepot.ClientID %>';                    /* デポ */
            var dblClickObjects = [['<%= Me.txtStYMD.ClientID %>', viewCalId],
                                   ['<%= Me.txtEndYMD.ClientID %>', viewCalId],
                                   ['<%= Me.txtTankNo.ClientID %>', viewTankNo],
                                   ['<%= Me.txtDepot.ClientID %>', viewDepot]];
            bindLeftBoxShowEvent(dblClickObjects);

            /* 手入力変更時のイベント */

            /* 左ボックスのリストボックスダブルクリックイベントバインド */
            bindLeftListBoxDblClickEvent();

            /* 左ボックスの拡張機能 */
            /* 拡張機能を紐づけるリスト及び機能のフラグの配列 
             * 2階層 1次元:コントロールのID,二次元:ソート機能フラグ(0,無し,1:名称のみ,2:コードのみ,3:両方),フィルタ機能フラグ(0,無し,1:設定)
             */ 
            var leftListExtentionTarget = [['<%= Me.lbTank.ClientID %>', '3', '1'],['<%= Me.lbDepot.ClientID %>', '3', '1']];
            addLeftBoxExtention(leftListExtentionTarget);

            /* 画面テキストボックス変更イベントのバインド(変更検知したいテキストボックスIDを指定 */
            var targetOnchangeObjects = [['<%= Me.txtTankNo.ClientID %>'],['<%= Me.txtDepot.ClientID %>']];
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
    <%--FormIDは適宜変更ください。 --%>
    <form id="GBT00011S" runat="server">
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
                    <input id="btnEnter" type="button" value="実行"  runat="server"  />
                    <input id="btnBack" type="button" value="終了"  runat="server"  />
                    <div id="btnFIRST" class="firstPage" runat="server" style="visibility:hidden;"></div>
                    <div id="btnLAST" class="lastPage" runat="server" style="visibility:hidden;"></div>
                </div>

                <table id="itemTable">
                    <%-- 年度 --%>
	                <tr>
		                <td>
			                <a><asp:Label ID="lblYMD1" runat="server" CssClass="requiredMark"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:Label ID="lblYMD2" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtStYMD" runat="server"></asp:TextBox></a>
		                </td>
		                <td>
			                <a><asp:Label ID="lblTilde" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtEndYMD" runat="server"></asp:TextBox></a>
		                </td>
	                </tr>
                    <%-- デポ --%>
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblDepot" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtDepot" runat="server"></asp:TextBox></a>
		                </td>
                        <td colspan="2">
			                <a><asp:Label ID="lblDepotText" runat="server" ></asp:Label></a>
		                </td>
	                </tr>
                    <%-- タンク番号 --%>
                    <tr>
		                <td colspan="2">
			                <a><asp:Label ID="lblTankNo" runat="server"></asp:Label></a>
		                </td>
		                <td>
			                <a><asp:TextBox ID="txtTankNo" runat="server"></asp:TextBox></a>
		                </td>
	                </tr>
                </table>

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
                    <%-- MapVariant退避 --%>
                    <asp:HiddenField ID="hdnThisMapVariant" runat="server" Value="" Visible="false" />
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
                    <%--  　タンク番号　 --%>
                    <asp:View id="vLeftTank" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbTank" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END タンク番号 VIEW　 --%>
                    <%--  　デポ　 --%>
                    <asp:View id="vLeftDepot" runat="server" >
                        <div class="leftViewContents">
                            <asp:ListBox ID="lbDepot" runat="server" CssClass="leftViewContents"></asp:ListBox>                           
                        </div>
                    </asp:View> <%-- END デポ VIEW　 --%>
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
