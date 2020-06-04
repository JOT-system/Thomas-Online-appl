<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="GLM00001TEST.aspx.vb" Inherits="OFFICE.GLM00001TEST" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>GLM00001</title>

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
            overflow:auto;}

        /* ------------------------------
         擬似フレーム　スタイル定義
        ------------------------------ */
        #titlebox {
            height:32px; 
            width:100%;
            
            position:fixed; 
            top:0px;
            
            overflow:hidden;
            text-align:center;
       	    background-color: rgb(0,100,0); }

        #footerbox {
            height:18px; 
            width:100% ;

            position:fixed; 
            bottom:0px;

            overflow:hidden;
            margin:1px 1px 1px 1px; 
            background-color:darksalmon;
        }

        #leftbox {
            width:0em; 

            position:fixed;
            top:34px;
            bottom:20px; 
            left:0; 

            overflow:hidden;
           	background-color: gray; 
            z-index:20;
        }

        #rightbox {
            width:0em; 

            position:fixed;
            top:34px;
            bottom:20px; 
            right:0; 

            overflow:auto;
          	background-color: gray;
            z-index:21;}

        #headerbox {
            position:fixed;
            top:34px; 
            bottom:20.4em; 
            left:0px; 
            right:0px; 

            overflow:hidden;
       	    background-color: rgb(220,230,240); }
        
        #detailbox {
            height:19em; 

            position:fixed; 
            bottom:20px; 
            left:0px; 
            right:0px; 

            overflow:hidden;
  	        background-color:rgb(148,138,84); 
        }



 

    


        /* ------------------------------
         画面項目　スタイル定義
        ------------------------------ */
        /* タイトル情報 */
        #WF_TITLEID{
            height:25px; 

            overflow:hidden;
            position:fixed; 
            left:1em;

            color: white;
            font-size: small;
            vertical-align:middle;
            text-align:left;
        }

        #WF_TITLETEXT{
            height:30px; 
            width:45em;

            overflow:hidden;
            position:fixed; 
            top:3px;
            left:3em;

            color: white ;
            font-size:x-large;
            vertical-align:middle;
        }


        #WF_TITLECAMP{
            height:15px; 
            width:12em;

            overflow:hidden;
            position:fixed; 
            top:2px;
            left:83em; 

            color:white; 
            font-size:small;
            vertical-align:middle;
            text-align:left; 
        }


        #WF_TITLEDATE{
            height:15px; 
            width:12em;

            overflow:hidden;
            position:fixed; 
            top:18px;
            left:83em; 

            color: white ;
            font-size:small;
            vertical-align:middle;
            text-align:left;

        }

        .WF_leftboxSW{
            height:28px; 
            width:300px;

            overflow:hidden;
            position:fixed; 
            top:0px;
            left:0px;}

        .WF_rightboxSW{
            height:28px; 
            width:300px;

            overflow:hidden;
            position:fixed; 
            top:0px;
            right:0px;}


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

        /* ヘッダー情報 */
        .WF_GRID{
            position:fixed;
            top:356px;
            bottom:20px; 
            right:1.5em;

            overflow:auto;
            font-size: small;
            vertical-align:middle;
        }

        .WF_HEADUSER{
            height:15px; 
            width:12em;

            overflow:hidden;
            position:fixed; 
            right:1em; 
            margin: 0px 0px 0px 0px;
            color:white; 
            font-size:x-small;
            vertical-align:middle;
            text-align:left; 
        }




        /* 詳細情報 */
        .WF_TEXTBOX_CSS{
            color:black; 
            font-size:small;
            vertical-align:middle;
            text-align:left; 
            overflow:hidden;
            ime-mode:inactive;
            border:1px solid black;
        }

        .WF_TEXTBOX_RONLY{
            color:black;

            font-size:small;
            vertical-align:middle;
            text-align:left; 
            overflow:hidden;
            ime-mode:inactive;
            border:1px solid black;
        }

        .WF_TEXT_LEFT{
            color:black; 
            font-size:small;
            vertical-align:middle;
            text-align:left; 
            overflow:hidden;
            ime-mode:inactive;
        }

        .WF_TEXT_LEFT_K{
            color:black; 
            font-size:small;
            vertical-align:middle;
            text-align:left; 
            overflow:hidden;
            ime-mode:active;
        }

        .WF_TEXT_LEFT_LABEL{
            color:blue; 
            font-size:small;
            vertical-align:middle;
            text-align:left; 
            overflow:hidden;
            ime-mode:active;
        }

        .WF_TEXT_RIGHT{
            color:black; 
            vertical-align:middle;
            text-align:right; 
            overflow:hidden;
        }

        .WF_TEXT_CENTER{
            color:black; 
            font-size:small;
            vertical-align:middle;
            text-align:center; 
            overflow:hidden;
        }

        .WF_LABEL_LEFT{
            color:black; 
            font-size:small;
            vertical-align:middle;
            text-align:left; 
            overflow:hidden;
            ime-mode:inactive;
            background-color:rgb(148,138,84); 
        }

        .WF_Dtab{
            text-align:center;
            vertical-align:-0.5em;
        }
        .WF_DViewRep{
            color:black; 
            margin-top:0.5em;
            margin-top:0.5em;
        }

        .WF_MEMO{
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
            color:black;
            z-index:30;
            border: 1px black;
            background-color:white;
            border: 2px solid blue; background-color: #ccffff;
        }


        

        /* ------------------------------
         個別スタイル定義
        ------------------------------ */


          #lime {color: lime}
          #textblue    {color: blue}
          #textblack   {color: black}
          #textgray    {color: gray}
          #textsilver  {color: silver}
          #textwhite   {color: white}
          #textmaroon  {color: maroon}
          #textgreen   {color: green}
          #textnavy    {color: navy}
          #textpurple  {color: purple}
          #textolive   {color: olive}
          #textteal    {color: teal}
          #textyellow  {color: yellow}
          #textfuchsia {color: fuchsia}
          #textaqua    {color: aqua}

        .WF_Repeater thead{

        }
        .WF_Repeater tbody{

        }

        </style>


    <script type="text/javascript" src="calendar.js"></script>

    <script  type="text/javascript">

        // ○OnLoad用処理（左右Box非表示）
        function InitDisplay() {

            // 全部消す
            document.getElementById("leftbox").style.width = "0em";
            document.getElementById("rightbox").style.width = "0em";

            if (document.getElementById('WF_LeftboxOpen').value == "Open") {
                document.getElementById("leftbox").style.width = "26em";
            };

            if (document.getElementById('WF_RightboxOpen').value == "Open") {
                document.getElementById("rightbox").style.width = "26em";
            };
            //更新ボタン活性／非活性
            if (document.getElementById('WF_MAPpermitcode').value == "TRUE") {
                //活性
                document.getElementById("WF_ButtonUPDATE").disabled = "";
            } else {
                //非活性 
                document.getElementById("WF_ButtonUPDATE").disabled = "disabled";
            };
        };

        // ○左Box用処理（左Box表示/非表示切り替え）
        function Field_DBclick(repfield, fieldNM, tabNo) {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_FIELD_rep').value = repfield;
                document.getElementById('WF_FIELD').value = fieldNM;
                document.getElementById('WF_LeftMViewChange').value = tabNo;
                document.getElementById('WF_LeftboxOpen').value = "Open";
                MC0006.submit();                            //aspx起動
            };
        };

        // ○左BOX用処理（DBクリック選択+値反映）
        function ListboxDBclick() {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_LeftboxOpen').value = "";
                document.getElementById("WF_ListboxDBclick").value = "DBclick";
                MC0006.submit();                            //aspx起動
            }
        }

        // ○右Box用処理（右Box表示/非表示切り替え）
        function r_boxDisplay() {
            if (document.getElementById("rightbox").style.width == "0em") {
                document.getElementById("rightbox").style.width = "26em";
                document.getElementById('WF_RightboxOpen').value = "Open";
            } else {
                document.getElementById("rightbox").style.width = "0em";
                document.getElementById('WF_RightboxOpen').value = "";
            };
        };

        // ○右BOX用処理（ラジオボタン）
        function rightboxChange(tabNo) {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_RightViewChange').value = tabNo;
                document.getElementById('WF_RightboxOpen').value = "Open";
                MC0006.submit();                            //aspx起動
            }
        }

        // ○右BOX用処理（メモ変更）
        function MEMOChange() {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById("WF_MEMOChange").value = "MEMOChange";
                document.getElementById('WF_RightboxOpen').value = "Open";
                MC0006.submit();                            //aspx起動
            }
        }

        // ○GridView一覧用処理
        function GridDbClick(obj, LineCnt) {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById("WF_GridDBclick").value = LineCnt;
                MC0006.submit();                             //aspx起動
            };
        };

        // ○GridView処理（矢印処理）
        document.onkeydown = function (event) {
            if (window.event.keyCode == 38) {
                if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                    document.getElementById("WF_SUBMIT").value = "TRUE"
                    document.getElementById("WF_MouseWheel").value = "-";
                    MC0006.submit();                            //aspx起動
                };
            };
            if (window.event.keyCode == 40) {
                if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                    document.getElementById("WF_SUBMIT").value = "TRUE"
                    document.getElementById("WF_MouseWheel").value = "+";
                    MC0006.submit();                            //aspx起動
                };
            };
        };

        // ○ディテール(タブ切替)処理
        function DtabChange(tabNo) {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_DTABChange').value = tabNo;
                MC0006.submit();                            //aspx起動
            }
        }

        // ○ディテール(開始年月日変更)処理
        function STYMDChange() {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_STYMDChange').value = "Change";
                MC0006.submit();                            //aspx起動
            }
        }

        // ○ディテール(PDF内容表示)処理
        function DtabPDFdisplay(filename) {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_DTABPDFEXCELdisplay').value = filename;
                MC0006.submit();                            //aspx起動
            }
        }

        // ○ディテール(PDF表示選択切替)処理
        function PDFselectChange() {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                document.getElementById('WF_DTABPDFEXCELchange').value = "Change";
                MC0006.submit();                            //aspx起動
            }
        }

        // ○ドロップ処理（ドラッグドロップ入力）
        function f_dragEvent(e, kbn) {
            document.getElementById("WF_MESSAGE").textContent = "ファイルアップロード開始";
            document.getElementById("WF_MESSAGE").style.color = "blue";
            document.getElementById("WF_MESSAGE").style.fontWeight = "bold";

            // ドラッグされたファイル情報を取得
            var files = e.dataTransfer.files;

            // 送信用FormData オブジェクトを用意
            var fd = new FormData();

            // ファイル情報を追加する
            for (var i = 0; i < files.length; i++) {
                fd.append("files", files[i]);
            }

            // XMLHttpRequest オブジェクトを作成
            var xhr = new XMLHttpRequest();

            // ドロップファイルによりURL変更
            if (files[0].type == "application/pdf") {
                // 「POST メソッド」「接続先 URL」を指定
                xhr.open("POST", "CO0101PDFUP.ashx", false)

                // イベント設定
                // ⇒XHR 送信正常で実行されるイベント
                xhr.onload = function (e) {
                    if (e.currentTarget.status == 200) {
                        document.getElementById("WF_EXCEL_UPLOAD").value = "PDF_LOADED";
                        MC0006.submit();                             //aspx起動
                    } else {
                        document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
                        document.getElementById("WF_MESSAGE").style.color = "red";
                        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
                    }
                };

                // ⇒XHR 送信ERRで実行されるイベント
                xhr.onerror = function (e) {
                    document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
                    document.getElementById("WF_MESSAGE").style.color = "red";
                    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
                };

                // ⇒XHR 通信中止すると実行されるイベント
                xhr.onabort = function (e) {
                    document.getElementById("WF_MESSAGE").textContent = "通信を中止しました。";
                    document.getElementById("WF_MESSAGE").style.color = "red";
                    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
                };

                // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
                xhr.ontimeout = function (e) {
                    document.getElementById("WF_MESSAGE").textContent = "タイムアウトエラーが発生しました。";
                    document.getElementById("WF_MESSAGE").style.color = "red";
                    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
                };

                // 「送信データ」を指定、XHR 通信を開始する
                xhr.send(fd);
            } else {
                // 「POST メソッド」「接続先 URL」を指定
                xhr.open("POST", "CO0100XLSUP.ashx", false)

                // イベント設定
                // ⇒XHR 送信正常で実行されるイベント
                xhr.onload = function (e) {
                    if (e.currentTarget.status == 200) {

                        if (kbn == "FILE_UP") {
                            document.getElementById("WF_EXCEL_UPLOAD").value = "XLS_SAVE";
                        } else {
                            document.getElementById("WF_EXCEL_UPLOAD").value = "XLS_LOADED";
                        }
                        MC0006.submit();                             //aspx起動
                    } else {
                        document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
                        document.getElementById("WF_MESSAGE").style.color = "red";
                        document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
                    }
                };

                // ⇒XHR 送信ERRで実行されるイベント
                xhr.onerror = function (e) {
                    document.getElementById("WF_MESSAGE").textContent = "ファイルアップロードが失敗しました。";
                    document.getElementById("WF_MESSAGE").style.color = "red";
                    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
                };

                // ⇒XHR 通信中止すると実行されるイベント
                xhr.onabort = function (e) {
                    document.getElementById("WF_MESSAGE").textContent = "通信を中止しました。";
                    document.getElementById("WF_MESSAGE").style.color = "red";
                    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
                };

                // ⇒送信中にタイムアウトエラーが発生すると実行されるイベント
                xhr.ontimeout = function (e) {
                    document.getElementById("WF_MESSAGE").textContent = "タイムアウトエラーが発生しました。";
                    document.getElementById("WF_MESSAGE").style.color = "red";
                    document.getElementById("WF_MESSAGE").style.fontWeight = "bold";
                };

                // 「送信データ」を指定、XHR 通信を開始する
                xhr.send(fd);
            };



        }

        // ○ドロップ処理（処理抑止）
        function f_dragEventCancel(event) {
            event.preventDefault();  //イベントをキャンセル
        };

        // ○GridView処理（マウスホイール処理）
        function f_MouseWheel(event) {
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                    if (window.event.wheelDelta < 0) {
                    document.getElementById("WF_MouseWheel").value = "+";
                } else {
                    document.getElementById("WF_MouseWheel").value = "-";
                };
                MC0006.submit();                            //aspx起動
            } else {
                return false;
            };
        };

        // ○ダウンロード処理
        function f_ExcelPrint() {
            // リンク参照
            window.open(document.getElementById("WF_PrintURL").value, "view", "_blank");
        };

        function f_PDFPrint() {
            // リンク参照
            window.open(document.getElementById("WF_PrintURL").value, "view", "_blank");
        };

        // ○各ボタン押下処理
        function ButtonClick(btn) {
            //押下されたボタンを設定
            document.getElementById("WF_ButtonClick").value = btn;

            //サーバー未処理（WF_SUBMIT="FALSE"）のときのみ、SUBMIT
            if (document.getElementById("WF_SUBMIT").value == "FALSE") {
                document.getElementById("WF_SUBMIT").value = "TRUE"
                MC0006.submit();                            //aspx起動
            } else {
                return false;
            }
        };
    </script>

</head>
<body onload="InitDisplay()">
    <form id="MC0006" runat="server"> 

    <div ondragstart="f_dragEventCancel(event)"
            ondrag="f_dragEventCancel(event)"
            ondragend="f_dragEventCancel(event)" 
            ondragenter="f_dragEventCancel(event)"
            ondragleave="f_dragEventCancel(event)" 
            ondragover="f_dragEventCancel(event)"  
            ondrop="f_dragEvent(event,'DATA_IN')">    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->

        <!-- 全体レイアウト　titlebox -->
        <div class="titlebox" id="titlebox">
            <asp:Label ID="WF_TITLEID" runat="server" Text="" CssClass="WF_TITLEID" ></asp:Label>
            <asp:Label ID="WF_TITLETEXT" runat="server" Text="" CssClass="WF_TITLETEXT"></asp:Label>
            <asp:Label ID="WF_TITLECAMP" runat="server" CssClass="WF_TITLECAMP"></asp:Label>
            <asp:Label ID="WF_TITLEDATE" runat="server" Text="" CssClass="WF_TITLEDATE"></asp:Label>
            <img class="WF_rightboxSW" src="透明R.png" style="z-index:30" ondblclick="r_boxDisplay()" alt=""/>
        </div>

        <!-- 全体レイアウト　headerbox -->
        <div  class="headerbox" id="headerbox">
            <div class="Operation" style="margin-left:3em;margin-top:0.5em;">
                <!-- ■　選択　■ -->
                <a style="position:fixed;top:2.9em;left:3em;">
                    <asp:Label ID="WF_TORINAME_LABEL" runat="server" Text="取引先名称" Height="1.5em" Font-Bold="True"></asp:Label>
                </a>
                <a style="position:fixed;top:2.8em;left:8.5em;">
                    <asp:TextBox ID="WF_TORINAME" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <a style="position:fixed;top:2.9em;left:17em;">
                    <asp:Label ID="WF_TODOKENAME_LABEL" runat="server" Text="届先名称" Height="1.5em" Font-Bold="True"></asp:Label>
                </a>
                <a style="position:fixed;top:2.8em;left:21.5em;">
                    <asp:TextBox ID="WF_TODOKENAME" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                </a>

                <a style="position:fixed;top:2.9em;left:30em;">
                    <asp:Label ID="WF_CLASS_LABEL" runat="server" Text="分類" Height="1.5em" Font-Bold="True" Font-Underline="True"></asp:Label>
                </a>
                <a style="position:fixed;top:2.8em;left:32.5em;"ondblclick="Field_DBclick('' , 'WF_CLASS', 6)">
                    <asp:TextBox ID="WF_CLASS" runat="server" Height="1.1em" Width="7em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    <asp:Label ID="WF_CLASS_TEXT" runat="server" Height="1.1em" Width="10em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <a>　　</a>
                <!-- ■　ボタン　■ -->
                <a style="position:fixed;top:2.8em;left:49em;">
                    <input type="button" id="WF_ButtonExtract" value="絞り込み"  style="Width:5em" onclick="ButtonClick('WF_ButtonExtract');" />
                </a>
                <a style="position:fixed;top:2.8em;left:53.5em;">
                    <input type="button" id="WF_ButtonUPDATE" value="DB更新"  style="Width:5em" onclick="ButtonClick('WF_ButtonUPDATE');" />
                </a>
                <a style="position:fixed;top:2.8em;left:58em;">
                    <input type="button" id="WF_ButtonCSV" value="ﾀﾞｳﾝﾛｰﾄﾞ"  style="Width:5em" onclick="ButtonClick('WF_ButtonCSV');" />
                </a>
                <a style="position:fixed;top:2.8em;left:62.5em;">
                    <input type="button" id="WF_ButtonPrint" value="一覧印刷"  style="Width:5em" onclick="ButtonClick('WF_ButtonPrint');" />
                </a>
                <a style="position:fixed;top:2.8em;left:67em;">
                    <input type="button" id="WF_ButtonEND" value="終了"  style="Width:5em" onclick="ButtonClick('WF_ButtonEND');" />
                </a>
                <a style="position:fixed;top:3.2em;left:75em;">
                    <asp:Image ID="WF_ButtonFIRST2" runat="server" ImageUrl="先頭頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonFIRST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
                <a style="position:fixed;top:3.2em;left:77em;">
                    <asp:Image ID="WF_ButtonLAST2" runat="server" ImageUrl="最終頁.png" Width="1.5em" onclick="ButtonClick('WF_ButtonLAST');" Height="1em" ImageAlign="AbsMiddle" />
                </a>
            </div>
            <div style="overflow-x:auto;overflow-y:hidden;position:fixed;top:4.5em;bottom:21em;left:1em;right:1em;" onmousewheel="f_MouseWheel()">
                <!--
                <asp:GridView ID="WF_GRID" runat="server">
                </asp:GridView>
                -->
                <asp:table ID="GridView1" runat="server">
                </asp:table>
                <!-- GridViewスクロールはさせない(Header固定・Bodyスクロールは、スクロールバー分だけレイアウトがずれる)。 -->
            </div>
        </div>
    </div>


        <!-- 全体レイアウト　detailbox -->
        <div  class="detailbox" id="detailbox">
            <div ondragstart="f_dragEventCancel(event)"
                    ondrag="f_dragEventCancel(event)"
                    ondragend="f_dragEventCancel(event)" 
                    ondragenter="f_dragEventCancel(event)"
                    ondragleave="f_dragEventCancel(event)" 
                    ondragover="f_dragEventCancel(event)"  
                    ondrop="f_dragEvent(event,'DATA_IN')">    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->

                <a style="position:relative;top:0.5em;left:49em;">
                    <input type="button" id="WF_UPDATE" value="表更新"  style="Width:5em" onclick="ButtonClick('WF_UPDATE');" />
                </a>
                <a style="position:relative;top:0.5em;left:49em;margin: 0em 0em 0em 0.2em;">
                    <input type="button" id="WF_CLEAR" value="クリア"  style="Width:5em" onclick="ButtonClick('WF_CLEAR');" />
                </a>
                <a style="position:relative;top:0.5em;left:49em;margin: 0em 0em 0em 0.2em;">
                    <input type="button" id="WF_MAP" value="地図表示"  style="Width:5em" onclick="ButtonClick('WF_MAP');" />
                </a>
                <a style="position:relative;top:0.5em;left:49em;margin: 0em 0em 0em 0.2em;">
                    <input type="button" id="WF_COORDINATE" value="緯度経度"  style="Width:5em" onclick="ButtonClick('WF_COORDINATE');" />
                </a>
                <a style="position:relative;top:0.5em;left:49em;margin: 0em 0em 0em 0.2em;">
                </a><br />

                <!-- ■　選択No　■ -->
                <a style="position:fixed;bottom:18.3em;left:3em; width:32em;">
                    <asp:Label ID="Label2" runat="server" Text="選択No" Height="1.1em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True"></asp:Label>
                    <asp:Label ID="WF_Sel_LINECNT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT"></asp:Label>
                </a>

                <!-- ■　会社　■ -->
                <a style="position:fixed;bottom:16.8em;left:3em; width:32em;" >
                    <asp:Label ID="WF_CAMPCODE_L" runat="server" Text="会社CD" Height="1.1em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_CAMPCODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" ondblclick="Field_DBclick('' , 'WF_CAMPCODE', 0)"></asp:TextBox>
                    <asp:Label ID="WF_CAMPCODE_TEXT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　取引先　■ -->
                <a style="position:fixed;bottom:16.8em;left:36.5em; width:32em;">
                    <asp:Label ID="WF_TORICODE_L" runat="server" Text="取引先CD" Height="1.1em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TORICODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" ondblclick="Field_DBclick('' , 'WF_TORICODE', 2)"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_TORICODE_TEXT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>
            
                <!-- ■　届先　■ -->
                <a style="position:fixed;bottom:15.3em;left:36.5em; width:32em;">
                    <asp:Label ID="WF_TODOKECODE_L" runat="server" Text="届先CD" Height="1.1em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <b>
                    <asp:TextBox ID="WF_TODOKECODE" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS" ondblclick="Field_DBclick('' , 'WF_TODOKECODE', 3)"></asp:TextBox>
                    </b>
                    <asp:Label ID="WF_TODOKECODE_TEXT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　有効年月日　■ -->
                <a style="position:fixed;bottom:15.3em;left:3em; width:40em;" >
                    <asp:Label ID="WF_YMD_L" runat="server" Text="有効年月日" Height="1.1em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <b  ondblclick="Field_DBclick('' , 'WF_STYMD', 7)">
                        <asp:TextBox ID="WF_STYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                    <asp:Label ID="Label1" runat="server" Text=" ～ " CssClass="WF_TEXT_LEFT"></asp:Label>
                    <b  ondblclick="Field_DBclick('' , 'WF_ENDYMD', 7)">
                        <asp:TextBox ID="WF_ENDYMD" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"></asp:TextBox>
                    </b>
                </a>

                <!-- ■　削除フラグ　■ -->
                <a style="position:fixed;bottom:16.8em;left:62.5em; width:32em;"  ondblclick="Field_DBclick('' , 'WF_DELFLG' ,  1)">
                    <asp:Label ID="WF_DELFLG_L" runat="server" Text="削除" Height="1.1em" Width="7em" CssClass="WF_TEXT_LEFT" Font-Bold="True" Font-Underline="True"></asp:Label>
                    <asp:TextBox ID="WF_DELFLG" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXTBOX_CSS"  ondblclick="Field_DBclick('' , 'WF_DELFLG', 1)"></asp:TextBox>
                    <asp:Label ID="WF_DELFLG_TEXT" runat="server" Height="1.1em" Width="15em" CssClass="WF_TEXT_LEFT_LABEL"></asp:Label>
                </a>

                <!-- ■　Dタブ　■ -->
                <a style="position:fixed;bottom:13.21em;left:2em;" onclick="DtabChange('0')">
                    <asp:Label ID="WF_Dtab01" runat="server" Text="届先情報" Height="1.3em" Width="5.9em" CssClass="WF_Dtab" Font-Size="Medium"></asp:Label>
                </a>

                <a style="position:fixed;bottom:13.21em;left:8.2em;"  onclick="DtabChange('1')">
                    <asp:Label ID="WF_Dtab02" runat="server" Text="書類（PDF or EXCEL）" Height="1.3em" Width="11em" CssClass="WF_Dtab" Font-Size="Medium"></asp:Label>
                </a>
            </div>

            <!-- ■ DITAIL画面　■ -->        
            <asp:MultiView ID="WF_DetailMView" runat="server">
                <asp:View ID="WF_DView1" runat="server" >

                    <span class="WF_DViewRep1_Area" style="position:fixed;height:11em;bottom:2em;left:1.5em;right:1.5em;overflow-x:hidden;overflow-y:auto;background-color:white;border: 2px solid blue;background-color: rgb(220,230,240);table-layout: fixed"
                        ondragstart="f_dragEventCancel(event)"
                        ondrag="f_dragEventCancel(event)"
                        ondragend="f_dragEventCancel(event)" 
                        ondragenter="f_dragEventCancel(event)"
                        ondragleave="f_dragEventCancel(event)" 
                        ondragover="f_dragEventCancel(event)"  
                        ondrop="f_dragEvent(event,'DATA_IN')">    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->

                       <asp:Repeater ID="WF_DViewRep1" runat="server"  >
                            <HeaderTemplate>
                            </HeaderTemplate>

                            <ItemTemplate>
                                <table style="border-width:1px;margin:0.3em 0em 0em 1.3em;">
                                <tr style="">

                                <td style="height:1.1em;">
                                    <!-- 項目(名称)　左Side -->
                                    <asp:Label ID="WF_Rep1_FIELDNM_L" runat="server" Text="" Height="1.1em" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <asp:Label ID="Label6" runat="server" Text="" Height="1.1em" Width="0.5em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;" hidden="hidden">
                                    <!-- 項目(記号名)　左Side -->
                                    <asp:Label ID="WF_Rep1_FIELD_L" runat="server" Text="" Height="1.1em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <!-- 値　左Side -->
                                    <asp:TextBox ID="WF_Rep1_VALUE_L" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox>
                                </td>

                                <td style="height:1.1em;">
                                    <asp:Label ID="Label7" runat="server" Text="" Height="1.1em" Width="0.5em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;color:blue;">
                                    <!-- 値（名称）　左Side -->
                                    <asp:Label ID="WF_Rep1_VALUE_TEXT_L" runat="server" Height="1.1em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>
 
                                <td style="height:1.1em;">
                                    <!-- スペース -->
                                    <asp:Label ID="Label5" runat="server" Text="" Height="1.1em" Width="1em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <!-- 項目(名称)　中央 -->
                                    <asp:Label ID="WF_Rep1_FIELDNM_M" runat="server" Text="" Height="1.1em" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <asp:Label ID="Label8" runat="server" Text="" Height="1.1em" Width="0.5em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;" hidden="hidden">
                                    <!-- 項目(記号名)　中央 -->
                                    <asp:Label ID="WF_Rep1_FIELD_M" runat="server" Text="" Height="1.1em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <!-- 値　中央 -->
                                    <asp:TextBox ID="WF_Rep1_VALUE_M" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox>
                                </td>

                                <td style="height:1.1em;">
                                    <asp:Label ID="Label9" runat="server" Text="" Height="1.1em" Width="0.5em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;color:blue;">
                                    <!-- 値（名称）　中央 -->
                                    <asp:Label ID="WF_Rep1_VALUE_TEXT_M" runat="server" Height="1.1em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <!-- スペース -->
                                    <asp:Label ID="Label4" runat="server" Text="" Height="1.1em" Width="1em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <!-- 項目(名称)　右Side -->
                                    <asp:Label ID="WF_Rep1_FIELDNM_R" runat="server" Text="" Height="1.1em" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <asp:Label ID="Label10" runat="server" Text="" Height="1.1em" Width="0.5em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;" hidden="hidden">
                                    <!-- 項目(記号名)　右Side -->
                                    <asp:Label ID="WF_Rep1_FIELD_R" runat="server" Text="" Height="1.1em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;">
                                    <!-- 値　右Side -->
                                    <asp:TextBox ID="WF_Rep1_VALUE_R" runat="server" Height="1.1em" Width="8em" CssClass="WF_TEXTBOX_repCSS"></asp:TextBox>
                                </td>

                                <td style="height:1.1em;">
                                    <asp:Label ID="Label11" runat="server" Text="" Height="1.1em" Width="0.5em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.1em;color:blue;">
                                    <!-- 値（名称）　右Side -->
                                    <asp:Label ID="WF_Rep1_VALUE_TEXT_R" runat="server" Height="1.1em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                </tr>
                                </table>
                            </ItemTemplate>

                            <FooterTemplate>
                            </FooterTemplate>
             
                        </asp:Repeater>
                    </span>
                </asp:View>

            <!-- ■ PDF選択　■ -->

                <asp:View ID="WF_DView2" runat="server">

                    <span class="WF_DViewRep2_Area" style="position:fixed;height:11em;bottom:2em;left:1.5em;right:1.5em;overflow-x:hidden;overflow-y:auto;background-color:white;border: 2px solid blue;background-color: rgb(220,230,240);table-layout: fixed" 
                        ondragstart="f_dragEventCancel(event)"
                        ondrag="f_dragEventCancel(event)"
                        ondragend="f_dragEventCancel(event)" 
                        ondragenter="f_dragEventCancel(event)"
                        ondragleave="f_dragEventCancel(event)" 
                        ondragover="f_dragEventCancel(event)"  
                        ondrop="f_dragEvent(event,'FILE_UP')">    <!-- draggable="true"を指定するとTEXTBoxのマウス操作に影響 -->
                        
                        <!-- PDF表示選択 -->
                        <span style="position:relative;top:0.5em;left:1.3em;">
                            <asp:Label ID="Label12" runat="server" Text="表示選択" Height="1.1em" Width="6em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>

                        <span style="position:relative;top:0.5em;left:0.5em;" onchange="PDFselectChange()">
                            <asp:ListBox ID="WF_Rep2_PDFselect" runat="server" Height="1.5em" Width="13em"></asp:ListBox>
                        </span>

                        <span style="position:relative;top:0.5em;left:3.0em;">
                            <asp:Label ID="Label3" runat="server" Text="添付書類(届先台帳EXCEL)を登録する場合は、ここにドロップすること" Height="1.1em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Size="Medium"></asp:Label>
                       </span>
                        <span style="position:absolute;top:1.6em;left:30.5em;">
                            <asp:Label ID="Label15" runat="server" Text="↓↓↓" Height="1.1em" CssClass="WF_TEXT_LEFT" Font-Bold="true" Font-Size="Medium"></asp:Label>
                       </span>
                       <br />

                        <!-- PDF明細ヘッダー -->
                        <span style="position:relative;top:0.7em;left:5.0em;">
                            <asp:Label ID="Label13" runat="server" Text="ファイル名" Height="1.1em" Width="8em" CssClass="WF_TEXT_LEFT"></asp:Label>
                        </span>

                        <span style="position:relative;top:0.7em;left:34.3em;">
                            <asp:Label ID="Label14" runat="server" Text="削 除" Height="1.1em" Width="8em" CssClass="WF_TEXT_CENTER"></asp:Label>
                        </span>
                        <br />

                        <span style="position:absolute;top:3.2em;left:1.3em;height:7.3em;width:50em;overflow-x:hidden;overflow-y:auto;background-color:white;border:1px solid black;">
                        <asp:Repeater ID="WF_DViewRepPDF" runat="server" >
                            <HeaderTemplate>
                            </HeaderTemplate>

                            <ItemTemplate>
                                <table style="">
                                <tr style="">

                                <td style="height:1.0em;width:40em;color:blue;">
                                <!-- ■　ファイル記号名称　■ -->
                                <a>　</a>
                                <asp:Label ID="WF_Rep_FILENAME" runat="server" Text="" Height="1.0em" Width="30em" CssClass="WF_TEXT_LEFT"></asp:Label>
                                </td>

                                <td style="height:1.0em;width:10em;">
                                <!-- ■　削除　■ -->
                                <asp:TextBox ID="WF_Rep_DELFLG" runat="server" Height="1.0em" Width="10em" CssClass="WF_TEXT_CENTER"></asp:TextBox>
                                </td>

                                <td style="height:1.0em;width:10em;" hidden="hidden">
                                <!-- ■　FILEPATH　■ -->
                                <asp:Label ID="WF_Rep_FILEPATH" runat="server" Height="1.0em" Width="10em" CssClass="WF_TEXT_LEFT"></asp:Label>
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
        </div>

        <!-- 全体レイアウト　rightbox -->
        <div class="rightbox" id="rightbox">
            <a>
                <span style="position:relative;left:1.5em;right:1em;top:1.2em;" >
                        <asp:RadioButton ID="WF_right_SW1" runat="server" GroupName="rightbox" Text=" メモ表示" Width="8em"   Onclick="rightboxChange('0')" Checked="True" />
                        <asp:RadioButton ID="WF_right_SW2" runat="server" GroupName="rightbox" Text=" エラー詳細表示" Width="8em" Onclick="rightboxChange('1')"/>
                </span>
            </a><br />

            <asp:MultiView ID="WF_RightMView" runat="server">
                <!-- 　メモ　 -->
                <asp:View id="RightView1" runat="server" >
                    <a id="rightbox_memo">
                        <span id="memo" style="position:absolute;left:1em;right:1em;top:3em;" onchange="MEMOChange()">
                        <asp:TextBox ID="WF_MEMO" runat="server" Width="28.4em" Height="16.9em" CssClass="WF_MEMO" TextMode="MultiLine"></asp:TextBox>
                        </span><br />
                    </a>
                </asp:View>

                <asp:View id="RightView2" runat="server" >
                    <a id="rightbox_errreport">
                        <span id="errreport" style="position:absolute;left:1em;right:1em;top:3em;" >
                        <asp:TextBox ID="WF_ERR_REPORT" runat="server" Width="28.4em" Height="16.9em" TextMode="MultiLine"></asp:TextBox>
                        </span><br />
                    </a>
                </asp:View>
            </asp:MultiView>
            
            <span style="position:absolute;left:1em;top:17.5em;">印刷・インポート設定</span><br />

            <span style="position:absolute;left:1em;right:1em;top:19.0em;">
                <asp:ListBox ID="WF_REPORTID" runat="server" Width="28.4em" Height="15em" style="border: 2px solid blue;background-color: #ccffff;"></asp:ListBox>
            </span><br />

        </div>


        <!-- 全体レイアウト　footerbox -->
        <div class="footerbox" id="footerbox">
            <asp:Label ID="WF_MESSAGE" runat="server" Text="" CssClass="WF_MESSAGE" ondblclick="r_boxDisplay()"></asp:Label><br />
        </div>


        <!-- 全体レイアウト　leftbox -->
        <div class="leftbox" id="leftbox">
            <div class="button" id="button" style="position:relative;left:0.5em;top:0.8em;">
                <input type="button" id="WF_ButtonSel" value="　選　択　"  onclick="ButtonClick('WF_ButtonSel');" />
                <input type="button" id="WF_ButtonCan" value="キャンセル"  onclick="ButtonClick('WF_ButtonCan');" />
            </div><br />
            
            <asp:MultiView ID="WF_LeftMView" runat="server">
                <!-- 　会社コード　 -->
                <asp:View id="LeftView1" runat="server" >
                    <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                    <asp:ListBox ID="WF_ListBoxCAMPCODE" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                    </a>
                </asp:View>

                <!-- 　削除フラグ　 -->
                <asp:View id="LeftView2" runat="server" >
                    <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                    <asp:ListBox ID="WF_ListBoxDELFLG" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                    </a>
                </asp:View>

                <!-- 　取引先　 -->
                <asp:View id="LeftView3" runat="server" >
                    <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                    <asp:ListBox ID="WF_ListBoxTORICODE" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                    </a>
                </asp:View>

                <!-- 　届先　 -->
                <asp:View id="LeftView4" runat="server" >
                    <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                    <asp:ListBox ID="WF_ListBoxTODOKECODE" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                    </a>
                </asp:View>
                
                <!-- 　市町村コード　 -->
                <asp:View id="LeftView5" runat="server" >
                    <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                    <asp:ListBox ID="WF_ListBoxCITIES" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                    </a>
                </asp:View>

                <!-- 　管理部署　 -->
                <asp:View id="LeftView6" runat="server" >
                    <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                    <asp:ListBox ID="WF_ListBoxMORG" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                    </a>
                </asp:View>

                <!-- 　分類　 -->
                <asp:View id="LeftView7" runat="server" >
                    <a  style="position:relative;height: 30.5em; width:24.7em;overflow: hidden;" ondblclick="ListboxDBclick()">
                    <asp:ListBox ID="WF_ListBoxCLASS" runat="server" CssClass="WF_ListBoxArea"></asp:ListBox>
                    </a>
                </asp:View>

                <!-- 　カレンダー　 -->
                <asp:View id="LeftView8" runat="server" >
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

            </asp:MultiView>

        </div>

        <div hidden="hidden">
                <asp:TextBox ID="WF_GridDBclick" Text="" runat="server" ></asp:TextBox>   <!-- GridViewダブルクリック -->
                <asp:TextBox ID="WF_GridPosition" Text="" runat="server" ></asp:TextBox>  <!-- GridView表示位置フィールド -->
            
                <input id="WF_DTABChange" runat="server" value="" type="text"/>           <!-- DetailBox Mview切替 -->
                <input id="WF_DTABPDFEXCELdisplay" runat="server" value="" type="text"/>  <!-- DetailBox PDF内容表示 -->
                <input id="WF_DTABPDFEXCELchange" runat="server" value="" type="text"/>   <!-- DetailBox PDF表示内容切替 -->
                <input id="WF_STYMDChange" runat="server" value="" type="text"/>          <!-- DetailBox 有効年月日変更 -->

                <input id="WF_FIELD"  runat="server" value=""  type="text" />             <!-- Textbox DBクリックフィールド -->
                <input id="WF_FIELD_rep"  runat="server" value=""  type="text" />         <!-- Textbox(Repeater) DBクリックフィールド -->

                <input id="WF_LeftMViewChange" runat="server" value="" type="text"/>      <!-- Leftbox Mview切替 -->
                <input id="WF_LeftboxOpen"  runat="server" value=""  type="text" />       <!-- Leftbox 開閉 -->
                <input id="WF_ListboxDBclick" runat="server" value="" type="text"/>       <!-- Leftbox ダブルクリック -->

                <input id="WF_RightViewChange" runat="server" value="" type="text"/>      <!-- Rightbox Mview切替 -->
                <input id="WF_RightboxOpen" runat="server" value=""  type="text" />       <!-- Rightbox 開閉 -->

                <input id="WF_SelectedIndex"  runat="server" value=""  type="text" />     <!-- Textbox DBクリックフィールド -->

                <input id="WF_MEMOChange" runat="server" value="" type="text"/>           <!-- MEMO変更フィールド -->

                <input id="WF_MouseWheel" runat="server" value="" type="text"/>           <!-- GridViewマウス操作フィールド -->

                <input id="WF_EXCEL_UPLOAD"  runat="server" value=""  type="text" />      <!-- Excel アップロードフィールド -->
            
                <asp:ListBox ID="WF_ListBoxPDF" runat="server"></asp:ListBox>             <!-- PDF アップロード一覧 -->

                <input id="WF_PrintURL" runat="server" value=""  type="text" />           <!-- Textbox Print URL -->

                <input id="WF_SUBMIT" runat="server" value=""  type="text" />             <!-- サーバー処理中（TRUE:実行中、FALSE:未実行） -->
                <input id="WF_ButtonClick" runat="server" value=""  type="text" />        <!-- ボタン押下 -->
                <input id="WF_MAPpermitcode" runat="server" value=""  type="text" />      <!-- 権限 -->
        </div>

    </form>
</body>
</html>
