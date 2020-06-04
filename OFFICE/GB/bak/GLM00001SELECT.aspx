<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GLM00001SELECT.aspx.vb" Inherits="OFFICE.GLM0001SELECT" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge"/>
    <title>GLM00001S</title>

    <style>
        /* ------------------------------
         全体スタイル初期値定義
        ------------------------------ */

        /* ブラウザのデフォルト初期化 */
        * {margin:0; padding:0;}
        
        body {
                /* ブラウザのデフォルト初期化 */
                margin:0; 
                padding:0; 
                overflow:auto;
        }

        /* ------------------------------
           擬似フレーム　スタイル定義
        ------------------------------ */
        #titlebox {
            height:32px; 

            position:fixed; 
            top:0px;
            left:0px;
            right:0px;
            background-color: rgb(0,100,0);
            overflow:hidden;
            text-align:center;}

        #footerbox {
            height:18px; 
            width:100% ;

            position:fixed; 
            bottom:0px;

            overflow:hidden;
            margin:1px 1px 1px 1px; 
            background-color:darksalmon;
        }

        #headerbox {
            width:auto; 

            position:fixed;
            top:34px; 
            bottom:20px; 
            left:0px; 
            right:0px; 

            overflow:hidden;
           	background-color: rgb(220,230,240);}

        #leftbox {
            width: 0em;
            position: fixed;
            top: 34px;
            bottom: 20px;
            left: 0;
            overflow: hidden;
            background-color: gray;
            z-index: 20;}

        #rightbox {
            width:0em; 

            position:fixed;
            top:34px;
            bottom:20px; 
            right:0; 

            overflow:auto;
      	    background-color: gray;
            z-index:21;}


/* ------------------------------
 画面項目　スタイル定義
------------------------------ */
/* タイトル情報 */
        #WF_TITLEID{
            height:1em; 
            width:18em;

            overflow:hidden;
            position:fixed; 
            left:1em;

            color: white;
            font-size: small;
            vertical-align:middle;
            text-align:left;}

        #WF_TITLETEXT{
            height:30px; 
            width:45em;

            position:fixed; 
            top:3px;
            left:3em;

            color: white;
            overflow:hidden;
            font-size:x-large;
            vertical-align:middle;
            text-align:center;}

        #WF_TITLECAMP{
            height:30px; 
            width:18em;

            overflow:hidden;
            position:fixed; 
            top:2px;
            left:83em; 

            color:white; 
            font-size: small;
            vertical-align:middle;
            text-align:left;}

        #WF_TITLEDATE{
            height:15px; 
            width:13em;

            overflow:hidden;
            position:fixed; 
            top:18px;
            left:83em; 

            color: white ;
            font-size:small;
            vertical-align:middle;
            text-align:left;}

        .WF_rightboxSW{
            height:15px; 
            width:300px;

            overflow:hidden;
            position:fixed; 
            top:17px;
            right:0px;

            color: white ;
            font-size:small;
            vertical-align:middle;}

        /* メッセージ情報 */
        #WF_MESSAGE{
            height:16px; 

            overflow:hidden;
            position:fixed; 
            left:1.5em;
            right:1.5em;

            color: blue ;
            font-size: small;
            vertical-align:middle;
        }

        /* テキスト表示情報 */
        .WF_TEXT{
            height:1.3em;
            overflow:hidden;
            color: blue ;
            font-size: small;
            vertical-align:middle;
            text-align:left;}

        .WF_ERRORREPORT{
            font-size: small;
            vertical-align:top;
            text-align:left;}


    /* LeftBox領域 */
    .WF_ListBoxArea{
            position:relative;
            top:1.0em;
            left:1.4em;
            height:34em;
            width:27em;
            overflow:hidden;
            overflow-y:auto;
            border: 1px black;
            color:black;
            z-index:30;
            background-color:white;
            border: 2px solid blue; background-color: #ccffff;
        }

    </style>

    <script type="text/javascript" src="calendar.js"></script>

    <script type="text/javascript">

        // ○OnLoad用処理（左右Box非表示）
        function InitDisplay() {

            // 全部消す
            document.getElementById("leftbox").style.width = "0em";
            document.getElementById("rightbox").style.width = "0em";

            if (document.getElementById('WF_LeftboxOpen').value == "Open") {
                document.getElementById("leftbox").style.width = "26em";
            };
        };

        // ○左Box用処理（左Box表示/非表示切り替え）
        function Field_DBclick(fieldNM, tabNo) {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_FIELD').value = fieldNM;
                document.getElementById('WF_MViewChange').value = tabNo;
                document.getElementById('WF_LeftboxOpen').value = "Open";
                GLM00001S.submit();                            //aspx起動
            };
        };

        // ○左BOX用処理（DBクリック選択+値反映）
        function ListboxDBclick() {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_LeftboxOpen').value = "";
                document.getElementById("WF_ListboxDBclick").value = "DBclick";
                GLM00001S.submit();                            //aspx起動
            }
        }

        // ○左BOX用処理（TextBox変更時、名称取得）
        function TextBox_change(fieldNM) {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_FIELD').value = fieldNM;
                document.getElementById('WF_TextBoxchange').value = "TextBoxchange";
                GLM00001S.submit();                            //aspx起動
            }
        }

        // ○右Box用処理（右Box表示/非表示切り替え）
        function r_boxDisplay() {
            if (document.getElementById("rightbox").style.width == "0em") {
                document.getElementById("rightbox").style.width = "26em";
            } else {
                document.getElementById("rightbox").style.width = "0em";
            };
        };

        // ○ドロップ処理（処理抑止）
        function f_dragEventCancel(event) {
            event.preventDefault();  //イベントをキャンセル
        };

        // ○メッセージクリア
        function MsgClear() {
            document.getElementById("WF_MESSAGE").value = "";
        }

        // ○ヘルプBox用処理
        function HelpDisplay() {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById("WF_HelpChange").value = "HELP";
                GLM00001S.submit();                            //aspx起動
            };
        };

        function ButtonClick(btn) {
            //押下されたボタンを設定
            document.getElementById("WF_ButtonClick").value = btn;

            //サーバー未処理（WF_SUBMIT="FALSE"）のときのみ、SUBMIT
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                GLM00001S.submit();                            //aspx起動
            } else {
                return false;
            }
        };
       </script>

</head>

<body onload="InitDisplay()">
    <form id="GLM00001S" runat="server" 
                ondragstart="f_dragEventCancel(event)"
                ondrag="f_dragEventCancel(event)"
                ondragend="f_dragEventCancel(event)" 
                ondragenter="f_dragEventCancel(event)"
                ondragleave="f_dragEventCancel(event)" 
                ondragover="f_dragEventCancel(event)"  
                ondrop="f_dragEventCancel(event)">    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->

        <!-- 全体レイアウト　titlebox -->
        <div class="titlebox" id="titlebox">
            <asp:Label ID="WF_TITLEID" runat="server" Text=""></asp:Label>
            <asp:Label ID="WF_TITLETEXT" runat="server" Text=""></asp:Label>
            <asp:Label ID="WF_TITLECAMP" runat="server" Text=""></asp:Label>
            <asp:Label ID="WF_TITLEDATE" runat="server" Text=""></asp:Label>
            <img class="WF_rightboxSW" src="透明R.png" style="z-index:30" ondblclick="r_boxDisplay()" alt=""/>
        </div>

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerbox" id="headerbox" >
            <!-- ○ 固定項目 ○ -->
            <a style="position:fixed;top:2.8em;left:62.5em;">
                <asp:Button ID="WF_ButtonDO" runat="server" Text="実行" style="Width:5em" OnClientClick="ButtonClick('WF_ButtonDO')"></asp:Button>
            </a>
            <a style="position:fixed;top:2.8em;left:67em;">
                <asp:Button ID="WF_ButtonEND" runat="server" Text="終了" style="Width:5em" OnClientClick="ButtonClick('WF_ButtonEND')"></asp:Button>
            </a>

            <!-- ○ 変動項目 ○ -->
            <!-- 　年度　 -->
            <a style="position:fixed;top:7.7em;left:4em;font-weight:bold;text-decoration:underline">
                <asp:Label ID="WF_LABEL_STYMD1" runat="server" Text="有効年月日" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:7.7em;left:11.5em;">
                <asp:Label ID="WF_LABEL_STYMD2" runat="server" Text="範囲指定" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:7.7em;left:42.5em;">～</a>

            <a style="position:fixed;top:7.5em;left:18em;" ondblclick="Field_DBclick('WF_STYMD', 5)">
                <asp:TextBox ID="WF_STYMD" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:7.5em;left:44em;" ondblclick="Field_DBclick('WF_ENDYMD', 5)">
                <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            
            <!-- 　国連番号　 -->
            <a style="position:fixed;top:9.9em;left:4em;font-weight:bold;text-decoration:underline">
                <asp:Label ID="WF_LABEL_UNNO1" runat="server" Text="国連番号" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:9.9em;left:11.5em;">
                <asp:Label ID="WF_LABEL_UNNO2" runat="server" Text="範囲指定" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:9.9em;left:42.5em;">～</a>

            <a style="position:fixed;top:9.7em;left:18em;" ondblclick="Field_DBclick('WF_UNNOF' ,  0)" onchange="TextBox_change('WF_UNNOF')">
                <asp:TextBox ID="WF_UNNOF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:9.9em;left:27em;">
                <asp:Label ID="WF_UNNOF_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <a style="position:fixed;top:9.7em;left:44em;" ondblclick="Field_DBclick('WF_UNNOT' ,  0)" onchange="TextBox_change('WF_UNNOT')">
                <asp:TextBox ID="WF_UNNOT" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:9.9em;left:53em;">
                <asp:Label ID="WF_UNNOT_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>


            <!-- 　等級　 -->
            <a style="position:fixed;top:12.1em;left:4em;font-weight:bold;text-decoration:underline">
                <asp:Label ID="WF_LABEL_HAZSRDCLASS" runat="server" Text="等級" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:12.1em;left:11.5em;">
                <asp:Label ID="Label1" runat="server" Text="範囲指定" CssClass="WF_TEXT"></asp:Label>
            </a>
            <a style="position:fixed;top:12.1em;left:42.5em;">～</a>

            <a style="position:fixed;top:11.9em;left:18em;" ondblclick="Field_DBclick('WF_HAZSRDCLASSF' ,  1)" onchange="TextBox_change('WF_HAZSRDCLASSF')">
                <asp:TextBox ID="WF_HAZSRDCLASSF" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:12.1em;left:27em;">
                <asp:Label ID="WF_HAZSRDCLASSF_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <a style="position:fixed;top:11.9em;left:44em;" ondblclick="Field_DBclick('WF_HAZSRDCLASST' ,  1)" onchange="TextBox_change('WF_HAZSRDCLASST')">
                <asp:TextBox ID="WF_HAZSRDCLASST" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:12.1em;left:53em;">
                <asp:Label ID="WF_HAZSRDCLASST_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>

            <!-- 　容器等級　 -->
            <a style="position:fixed;top:14.3em;left:4em;font-weight:bold;text-decoration:underline">
                <asp:Label ID="WF_LABEL_PACKINGGROUP" runat="server" Text="容器等級" CssClass="WF_TEXT"></asp:Label>
            </a>

            <a style="position:fixed;top:14.1em;left:18em;" ondblclick="Field_DBclick('WF_PACKINGGROUP' ,  2)" onchange="TextBox_change('WF_PACKINGGROUP')">
                <asp:TextBox ID="WF_PACKINGGROUP" runat="server" Height="1.4em" Width="10em" onblur="MsgClear()"></asp:TextBox>
            </a>
            <a style="position:fixed;top:14.3em;left:27em;">
                <asp:Label ID="WF_PACKINGGROUP_Text" runat="server" Text="" Width="17em" CssClass="WF_TEXT"></asp:Label>
            </a>


            <a hidden="hidden">
                <asp:TextBox ID="WF_FIELD" runat="server" text=""></asp:TextBox>
                <asp:TextBox ID="WF_ListboxDBclick" runat="server" text=""></asp:TextBox>
                <asp:TextBox ID="WF_LeftboxOpen" runat="server" text=""></asp:TextBox>
                <asp:TextBox ID="WF_MViewChange" runat="server" text=""></asp:TextBox>
                <asp:TextBox ID="WF_SelectedIndex" runat="server" text=""></asp:TextBox>
                <asp:TextBox ID="WF_TextBoxchange" runat="server" text=""></asp:TextBox>
                <asp:TextBox ID="WF_HelpChange" runat="server" text=""></asp:TextBox>
                <asp:TextBox ID="WF_SUBMIT" runat="server" text=""></asp:TextBox>
                <asp:TextBox ID="WF_ButtonClick" runat="server" text=""></asp:TextBox>
            </a>

        </div>

        <!-- 全体レイアウト　footerbox -->
        <div class="footerbox" id="footerbox">
            <asp:TextBox ID="WF_MESSAGE" runat="server" Height="18px" Width="100%" ReadOnly="True" Enabled="False" style="background-color:darksalmon" BorderStyle="None"></asp:TextBox>
<%--            <asp:Label ID="WF_MESSAGE" runat="server" Text="" CssClass="WF_MESSAGE"></asp:Label><br />--%>
            <a style="position:fixed;right:0.2em;">
            <img class="WF_HelpSW" src="ヘルプ.jpg" style="z-index:30" ondblclick="HelpDisplay()" alt=""/>
            </a>
        </div>

        <!-- 全体レイアウト　rightbox -->
        <div class="rightbox" id="rightbox">
            <span style="position:relative;left:1em;top:1em;">
                <a >エラー詳細表示</a>
            </span><br />

            <span style="position:relative;left:1em;right:1em;top:1.2em;">
                <asp:TextBox ID="WF_ERRORREPORT" runat="server" Width="28.4em" Height="16.9em" CssClass="WF_ERRORREPORT" TextMode="MultiLine"></asp:TextBox>
            </span><br />

            <span style="position:relative;left:1em;top:2em;">
                <a >画面レイアウト設定</a>
            </span><br />

            <span style="position:relative;left:1em;right:1em;top:2.3em;">
                <asp:ListBox ID="WF_VIEW" runat="server" Width="28.4em" Height="15em" style="border: 2px solid blue;background-color: rgb(220,230,240);" ></asp:ListBox>
            </span><br />

        </div>

        <!-- 全体レイアウト　leftbox -->
        <div class="leftbox" id="leftbox" >
            <div class="button" id="button" style="position:relative;left:0.5em;top:0.8em;">
                <asp:Button ID="WF_ButtonSel" runat="server" Text="　選　択　" OnClientClick="ButtonClick('WF_ButtonSel')"></asp:Button>
                <asp:Button ID="WF_ButtonCan" runat="server" Text="キャンセル" OnClientClick="ButtonClick('WF_ButtonCan')"></asp:Button>
            </div><br />
            
            <div class="tabbox" id="tabbox" style="margin: 0px; padding: 0px; width: 30em;position:relative;left:0.5em;top:0em;">
                <asp:MultiView ID="WF_LEFTMView" runat="server">

                    <!-- 　カレンダー　 -->
                    <asp:View id="tab1" runat="server" >
                    <a  style="position:relative;top:1em; left: 3em; height: 30.5em; width:24.7em;overflow: hidden;">
                        <asp:textbox ID="WF_Calendar" runat="server" type="hidden"></asp:textbox>
                        <div id="dValue" style="position:absolute; visibility:hidden"></div>
                        <table border="0">
                            <tr><td colspan="3">
                            <table border="1" >
                                <tr><td>
                                    <div id="carenda">
                                        <script type="text/JavaScript">
                                        <!--
                                        carenda(0);
                                        //-->
                                        </script>
                                    </div>
                                </td></tr>
                                <tr>
                                    <td id="altMsg" style="background:white">
                                        <script type="text/JavaScript">
                                        <!--
                                        setAltMsg(firstAltYMD, firstAltMsg);
                                        //-->
                                        </script>
                                    </td>
                                </tr>
                            </table>
                            </td></tr>
                        </table>
                    </a>
                    </asp:View>

                    <!-- 　国連番号　 -->
                    <asp:View id="tab2" runat="server" >
                        <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                        <asp:ListBox ID="WF_ListBoxUNNO" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                        </a>
                    </asp:View>

                    <!-- 　等級　 -->
                    <asp:View id="tab3" runat="server" >
                        <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                        <asp:ListBox ID="WF_ListBoxHAZSRDCLASS" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                        </a>
                    </asp:View>

                    <!-- 　容器等級　 -->
                    <asp:View id="tab4" runat="server" >
                        <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                        <asp:ListBox ID="WF_ListBoxPACKINGGROUP" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                        </a>
                    </asp:View>

                </asp:MultiView>
            </div>

        </div>

        <!-- Work レイアウト -->

    </form>
</body>
    
</html>
