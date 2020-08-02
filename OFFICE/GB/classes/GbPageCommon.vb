Option Strict On
Imports System
Imports System.Collections
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports BASEDLL
''' <summary>
''' ページ共通処理クラス
''' </summary>
''' <remarks>各ページで継承して利用する想定
''' こちらの処理が各ページの処理より先に実行されます。
''' </remarks>
Public Class GbPageCommon
    Inherits System.Web.UI.Page
    ''' <summary>
    ''' ロード時のメッセージNoを押さえるセッション変数名
    ''' </summary>
    Public Const CONST_SESSION_COM_LOAD_MESSAGENO As String = "COM_LOAD_MESSAGENO"
    Protected Overrides Sub SavePageStateToPersistenceMedium(ByVal viewState As Object)
        Dim lofF As New LosFormatter
        Using sw As New IO.StringWriter
            lofF.Serialize(sw, viewState)
            Dim viewStateString = sw.ToString()
            Dim bytes = Convert.FromBase64String(viewStateString)
            bytes = CompressByte(bytes)
            ClientScript.RegisterHiddenField("__VSTATE", Convert.ToBase64String(bytes))
        End Using
    End Sub
    Protected Overrides Function LoadPageStateFromPersistenceMedium() As Object
        Dim viewState As String = Request.Form("__VSTATE")
        Dim bytes = Convert.FromBase64String(viewState)
        bytes = DeCompressByte(bytes)
        Dim lofF = New LosFormatter()
        Return lofF.Deserialize(Convert.ToBase64String(bytes))
    End Function
    ''' <summary>
    ''' ページロード処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim comMessageNo As String = C_MESSAGENO.NORMAL '全て正常時は正常をセッション変数に格納
        Dim isBackLogin As Boolean = False
        If IsPostBack = False AndAlso IsAccessbleBrowser(Me.Page.Request.UserAgent) = False Then
            Try
                Response.Redirect("~/BrowserWorning.html", True)
            Catch ex As Exception
            End Try
        End If

        Dim phCont As PlaceHolder = DirectCast(Me.Page.Header.FindControl("phCommonHeader"), PlaceHolder)
        If phCont IsNot Nothing Then
            'Chrome翻訳対策
            Dim metaHeader As New HtmlMeta With {.Name = "google", .Content = "notranslate"}
            phCont.Controls.Add(metaHeader)

            'Webコンフィグに上帯またはタイトル文言が設定されている場合、動的スタイルをヘッダーに追加
            Dim bgColor As String = Convert.ToString(ConfigurationManager.AppSettings("HtmlHeaderBgColor"))
            Dim prefixText As String = Convert.ToString(ConfigurationManager.AppSettings("HtmlHeaderTextPrefix"))
            Dim bgColorC As String = Convert.ToString(ConfigurationManager.AppSettings("HtmlContensBgColor"))
            If bgColor <> "" OrElse prefixText <> "" OrElse bgColorC <> "" Then
                Dim styleHeader As New HtmlGenericControl("STYLE")
                styleHeader.Attributes.Add("type", "text/css")
                Dim writeStyle As New StringBuilder
                If bgColor <> "" Then
                    writeStyle.AppendLine("#divTitlebox {")
                    writeStyle.AppendFormat("    background-color : {0} !important;", bgColor).AppendLine()
                    writeStyle.AppendLine("}")
                End If
                If prefixText <> "" Then
                    writeStyle.AppendLine("#lblTitleText:before {")
                    writeStyle.AppendFormat("    content : ""{0} "";", prefixText).AppendLine()
                    writeStyle.AppendLine("    color:red;")
                    writeStyle.AppendLine("    font-weight:bolder;")
                    writeStyle.AppendLine("}")
                End If
                If bgColorC <> "" Then
                    writeStyle.AppendLine("#divContensbox {")
                    writeStyle.AppendFormat("    background-color : {0} !important;", bgColorC).AppendLine()
                    writeStyle.AppendLine("}")
                    writeStyle.AppendLine("#headerbox {")
                    writeStyle.AppendFormat("    background-color : {0} !important;", bgColorC).AppendLine()
                    writeStyle.AppendLine("}")
                End If
                styleHeader.InnerHtml = writeStyle.ToString
                phCont.Controls.Add(styleHeader)
            End If

        End If
        Me.Page.Form.Attributes.Add("translate", "no") '念のためFormに翻訳させないAttributeも追加
        'ログオンから呼ばれた場合はすべて無視
        If TypeOf Me.Page Is COM00001LOGON OrElse TypeOf Me.Page Is COM00002MENU OrElse
           TypeOf Me.Page Is COM00003HELP Then
            Return
        End If
        Try
            Session(CONST_SESSION_COM_LOAD_MESSAGENO) = comMessageNo
            '************************************
            'セッション変数死活
            '************************************
            If COA0019Session.USERID Is Nothing OrElse COA0019Session.USERID = "" OrElse COA0019Session.USERID = "INIT" Then
                isBackLogin = True
                comMessageNo = C_MESSAGENO.SESSIONEXPIRED
                Session(CONST_SESSION_COM_LOAD_MESSAGENO) = comMessageNo
                Server.Transfer(C_LOGIN_URL, False) 'ログイン画面に遷移
                Return
            End If
            '************************************
            'オンラインサービス判定
            '************************************
            'オンラインサービス判定
            Dim COA0005TermInfo As New BASEDLL.COA0005TermInfo With {.TERMid = Convert.ToString(HttpContext.Current.Session("APSRVname"))}
            COA0005TermInfo.COA0005GetTermInfo()
            If COA0005TermInfo.ERR <> C_MESSAGENO.NORMAL Then
                isBackLogin = True
                'BASEDLL処理異常 画面にメッセージを表示
                comMessageNo = COA0005TermInfo.ERR
                Session(CONST_SESSION_COM_LOAD_MESSAGENO) = comMessageNo
                Server.Transfer(C_LOGIN_URL, False) 'ログイン画面に遷移 Server.TransferやRedirectはFinallyを通らない
                Return
            End If
            '****************************************
            'メッセージ初期化
            '****************************************
            Dim lblFooterMessageObj As Label = DirectCast(Page.Form.FindControl("lblFooterMessage"), Label)
            lblFooterMessageObj.Text = ""
            lblFooterMessageObj.ForeColor = Drawing.Color.Black
            lblFooterMessageObj.Font.Bold = False
            '****************************************
            'ページオブジェクトへの日付フィールド埋込
            '****************************************
            Dim hdnDateFormatObj As New HtmlControls.HtmlInputHidden With {
                .ID = "hdnPageCommonDateFormat",
                .Name = .ID,
                .Value = GBA00003UserSetting.DATEFORMAT
            }
            Page.Form.Parent.Controls.Add(hdnDateFormatObj)
            '************************************
            '初回ロード時の共通処理
            '************************************
            If IsPostBack = False Then
                '****************************************
                'ユーザー共通情報取得
                '****************************************
                Dim GBA00003UserSetting As New GBA00003UserSetting With {
                    .USERID = COA0019Session.USERID
                }
                GBA00003UserSetting.GBA00003GetUserSetting()
                If GBA00003UserSetting.ERR <> C_MESSAGENO.NORMAL Then
                    'BASEDLL処理異常 画面にメッセージを表示
                    comMessageNo = COA0005TermInfo.ERR
                    Return
                End If
                '****************************************
                'ヘッダー日付設定
                '****************************************
                Dim lblHeaderDateObj As Label = DirectCast(Page.Form.FindControl("lblTitleDate"), Label)
                If lblHeaderDateObj IsNot Nothing Then
                    Dim timeFormat As String = " HH:mm:ss" 'これも国別にあるなら要設定
                    Dim dateFormat As String = "yyyy/MM/dd" '空白も加味して初期の日付フォーマットを準備
                    If GBA00003UserSetting.DATEFORMAT IsNot Nothing AndAlso
                       GBA00003UserSetting.DATEFORMAT <> "" Then
                        dateFormat = GBA00003UserSetting.DATEFORMAT
                    End If
                    Dim datetimeFormat As String = dateFormat & timeFormat
                    lblHeaderDateObj.Text = ""
                    lblHeaderDateObj.Text = Date.Now.ToString(datetimeFormat)
                End If
                '****************************************
                '画面ID
                '****************************************
                Dim lblTitleIdObj As Label = DirectCast(Page.Form.FindControl("lblTitleId"), Label)
                If lblTitleIdObj IsNot Nothing Then
                    lblTitleIdObj.Text = Page.Form.ID
                End If
                '****************************************
                'ヘッダー会社設定
                '****************************************
                Dim lblTitleCompayObj As Label = DirectCast(Page.Form.FindControl("lblTitleCompany"), Label)
                If lblTitleCompayObj IsNot Nothing Then
                    Dim COA0007getCompanyInfo As New BASEDLL.COA0007CompanyInfo With {
                        .COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp")),
                        .STYMD = Date.Now,
                        .ENDYMD = Date.Now}
                    COA0007getCompanyInfo.COA0007getCompanyInfo()
                    If COA0007getCompanyInfo.ERR = C_MESSAGENO.NORMAL Then
                        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                            lblTitleCompayObj.Text = COA0007getCompanyInfo.NAMES_EN
                        Else
                            lblTitleCompayObj.Text = COA0007getCompanyInfo.NAMES
                        End If
                    Else
                        'BASEDLL処理異常 画面にメッセージを表示
                        comMessageNo = COA0007getCompanyInfo.ERR
                        Return
                    End If
                End If

                '****************************************
                'オフィスユーザー名設定
                '****************************************
                Dim lblOfficeObj As Label = DirectCast(Page.Form.FindControl("lblTitleOffice"), Label)
                If lblOfficeObj IsNot Nothing Then
                    lblOfficeObj.Text = String.Format("{0}({1})", GBA00003UserSetting.OFFICENAME, GBA00003UserSetting.USERID)
                End If
            End If

        Catch ex As Threading.ThreadAbortException
            'ThreadAbortExceptionは無視
        Catch ex As Exception
            comMessageNo = C_MESSAGENO.SYSTEMADM
            Dim COA0003LogFile As New COA0003LogFile With {
                .RUNKBN = C_RUNKBN.ONLINE,
                .NIWEA = C_NAEIW.ABNORMAL,
                .TEXT = Page.Form.ID & ControlChars.CrLf & ex.ToString(),
                .MESSAGENO = comMessageNo
            }
            COA0003LogFile.COA0003WriteLog()
        Finally
            Session(CONST_SESSION_COM_LOAD_MESSAGENO) = comMessageNo
            'ログイン強制遷移しない限りは当画面に留まるのでエラーの場合はメッセージ設定
            Dim lblFooterMessageObj As Label = DirectCast(Page.Form.FindControl("lblFooterMessage"), Label)
            If comMessageNo <> C_MESSAGENO.NORMAL AndAlso
               lblFooterMessageObj IsNot Nothing AndAlso
               isBackLogin = False Then
                Dim COA0004LableMessage As New COA0004LableMessage
                COA0004LableMessage.MESSAGENO = comMessageNo
                COA0004LableMessage.NAEIW = C_NAEIW.ABNORMAL
                COA0004LableMessage.PARA01 = comMessageNo
                COA0004LableMessage.MESSAGEBOX = lblFooterMessageObj
                COA0004LableMessage.COA0004getMessage()
                If COA0004LableMessage.ERR = C_MESSAGENO.NORMAL Then
                    lblFooterMessageObj = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                End If
            End If

        End Try
    End Sub
    ''' <summary>
    ''' 「?」ボタンダブルクリック時イベント
    ''' </summary>
    ''' <remarks>共通処理として記載</remarks>
    Public Sub DivShowHelp_DoubleClick(mapId As String)
        Try
            Dim hdnCanHelpObj As HiddenField = DirectCast(Page.Form.FindControl("hdnCanHelpOpen"), HiddenField)
            Session("Class") = "WF_HELPDisplay"
            '画面遷移実行
            If hdnCanHelpObj Is Nothing Then
                Return
            End If
            'TODO COA0019Sessionに差し替えるかも
            HttpContext.Current.Session("HELPid") = mapId
            hdnCanHelpObj.Value = "1"
        Catch ex As Exception
            Dim lblFooterMessageObj As Label = DirectCast(Page.Form.FindControl("lblFooterMessage"), Label)
            If lblFooterMessageObj IsNot Nothing Then
                Dim COA0004LableMessage As New COA0004LableMessage With {
                            .MESSAGENO = C_MESSAGENO.EXCEPTION,
                            .NAEIW = C_NAEIW.ABNORMAL,
                            .MESSAGEBOX = lblFooterMessageObj
                }
                COA0004LableMessage.COA0004getMessage()
                If COA0004LableMessage.ERR = C_MESSAGENO.NORMAL Then
                    lblFooterMessageObj = DirectCast(COA0004LableMessage.MESSAGEBOX, Label)
                End If

            End If

            Dim COA0003LogFile As New COA0003LogFile With {
                            .RUNKBN = C_RUNKBN.ONLINE,
                            .NIWEA = C_NAEIW.ABNORMAL,
                            .TEXT = ex.ToString(),
                            .MESSAGENO = C_MESSAGENO.EXCEPTION}
            COA0003LogFile.COA0003WriteLog()

            Return
        End Try
    End Sub

    ''' <summary>
    ''' LangSetting関数で利用する文言設定ディクショナリ作成関数
    ''' </summary>
    ''' <param name="dicDisplayText">対象ディクショナリオブジェクト</param>
    ''' <param name="obj">オブジェクト</param>
    ''' <param name="jaText">日本語文言</param>
    ''' <param name="enText">英語文言</param>
    Public Sub AddLangSetting(ByRef dicDisplayText As Dictionary(Of Control, Dictionary(Of String, String)),
                               ByVal obj As Control, ByVal jaText As String, enText As String)
        dicDisplayText.Add(obj,
                           New Dictionary(Of String, String) _
                           From {{C_LANG.JA, jaText}, {C_LANG.EN, enText}})
    End Sub
    ''' <summary>
    ''' 画面文言設定
    ''' </summary>
    ''' <param name="dicDisplayText"></param>
    Public Sub SetDisplayLangObjects(dicDisplayText As Dictionary(Of Control, Dictionary(Of String, String)), lang As String)
        '上記で設定したオブジェクトの文言を変更
        For Each displayTextItem In dicDisplayText
            '足りないかもしれないので適宜追加
            Dim bufItem As Control = displayTextItem.Key
            If TypeOf bufItem Is Label Then
                'ラベルの場合
                Dim bufLabel As Label = DirectCast(bufItem, Label)
                bufLabel.Text = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is Button Then
                'ボタンの場合
                Dim bufButton As Button = DirectCast(bufItem, Button)
                bufButton.Text = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HiddenField Then
                '隠しフィールドの場合
                Dim bufHdf As HiddenField = DirectCast(bufItem, HiddenField)
                bufHdf.Value = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is RadioButton Then
                'ラジオボタン文言
                Dim bufRadio As RadioButton = DirectCast(bufItem, RadioButton)
                bufRadio.Text = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HtmlInputButton Then
                'Input[Type=button]
                Dim bufhtmlInputButton As HtmlInputButton = DirectCast(bufItem, HtmlInputButton)
                bufhtmlInputButton.Value = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HtmlInputHidden Then
                'Input[Type=Hidden]
                Dim bufhtmlInputHidden As HtmlInputHidden = DirectCast(bufItem, HtmlInputHidden)
                bufhtmlInputHidden.Value = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HtmlTableCell Then
                'テーブルセル<td>は<td></td>のすべての文字を設定
                Dim bufhtmlTableCell As HtmlTableCell = DirectCast(bufItem, HtmlTableCell)
                bufhtmlTableCell.InnerHtml = displayTextItem.Value(lang)
            ElseIf TypeOf bufItem Is HtmlControl Then
                'ここは今のところ不明オブジェクトなので何もしない
                Dim bufhtmlCont = DirectCast(bufItem, HtmlControl)
            End If
        Next
    End Sub
    ''' <summary>
    ''' アクセス可能なブラウザか判定
    ''' </summary>
    ''' <returns>True:許可、False:不可</returns>
    Private Function IsAccessbleBrowser(ua As String) As Boolean
        Dim retVal As Boolean = False
        Try
            ua = ua.ToLower 'すべて小文字に置換
            '許可するUAに含まれる文字("msie", "trident"→IE,"edge"→EDGE)
            'その他許可する必要があれば("safari"→safari(MAC OSの標準ブラウザ,"firefox"→FIREFOX,
            '                           "opera"→OPERA)などを↓のリストに適宜追加
            Dim acceptList As New List(Of String) From {"msie", "trident", "edg"}

            For Each accept In acceptList
                If ua.IndexOf(accept) <> -1 Then
                    retVal = True
                    Exit For
                End If
            Next
            Return retVal
        Catch ex As Exception
            Return False
        End Try
    End Function
    ''' <summary>
    ''' ByteDetaを圧縮
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function CompressByte(data As Byte()) As Byte()
        Using ms As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionLevel.Fastest)
            ds.Write(data, 0, data.Length)
            ds.Close()
            Return ms.ToArray
        End Using
    End Function
    ''' <summary>
    ''' Byteデータを解凍
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function DeCompressByte(data As Byte()) As Byte()
        Using inpMs As New IO.MemoryStream(data),
              outMs As New IO.MemoryStream,
              ds As New IO.Compression.DeflateStream(inpMs, IO.Compression.CompressionMode.Decompress)
            ds.CopyTo(outMs)
            Return outMs.ToArray
        End Using

    End Function
End Class
