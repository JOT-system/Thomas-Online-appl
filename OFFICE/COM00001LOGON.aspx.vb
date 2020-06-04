Imports System.Drawing
Imports System.Data.SqlClient
Imports System.Net
Imports System.Data
'Imports System.Windows.Forms
Imports BASEDLL
''' <summary>
''' ログイン画面クラス
''' </summary>
Public Class COM00001LOGON
    Inherits GbPageCommon

    ' 定数定義
    Private Const CONST_WEBCONFSTR_APLINI = "InifilePath"               ' WebConfig検索文字列(アプリINIフルパス)
    Private Const CONST_WEBCONFSTR_LANGDISP = "DefLangModeDisp"         ' WebConfig検索文字列(表示言語)
    Private Const CONST_WEBCONFSTR_LANGLOG = "DefLangModeLog"           ' WebConfig検索文字列(ログ出力言
    Private Const CONST_WEBCONFSTR_CHKMODE = "DefConnectCheckMode"      ' WebConfig検索文字列(接続チェックモード)
    Private Const CONST_SYSTEM_TITLE = "Welcome to Global Business Dept. FrontEnd Support System"

    Private Const CONST_MAPID As String = "COM00001"
    Private Const CONST_MAPVARI As String = "Default"
    Private Const CONST_PASSINIT_MAPVARI As String = "PassExpired"

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile

#Region "共通セッション情報"
    '共通セッション情報
    '   Namespace     : 名称空間(プロジェクト名)
    '   Class         : クラス(プロジェクト直下のクラス)
    '   Userid        : ユーザID
    '   APSRVname     : APサーバー名称
    '   APSRVCamp     : APサーバー設置会社(全社サーバー："＊"、個別設置サーバー：会社)
    '   APSRVOrg      : APサーバー設置部署(全社サーバー："＊"、個別設置サーバー：部署)
    '   MOrg          : 管理部署(営業部、支店レベル)
    '   Term          : 操作端末(端末操作情報として利用)
    '   TERMCOMP      : 操作端末会社(端末操作情報として利用)
    '   TermORG       : 操作端末部署(端末操作情報として利用)
    '   Selected_COMPCODE   : 画面選択会社コード
    '   Selected_STYMD      : 画面選択
    '   Selected_ENDYMD     : 画面選択
    '   Selected_USERIDFrom : 画面選択
    '   Selected_USERIDTo   : 画面選択
    '   Selected_USERIDG1   : 画面選択
    '   Selected_USERIDG2   : 画面選択
    '   Selected_USERIDG3   : 画面選択
    '   Selected_USERIDG4   : 画面選択
    '   Selected_USERIDG5   : 画面選択
    '   Selected_MAPIDPFrom : 画面選択
    '   Selected_MAPIDPTo   : 画面選択
    '   Selected_MAPIDPG1   : 画面選択
    '   Selected_MAPIDPG2   : 画面選択
    '   Selected_MAPIDPG3   : 画面選択
    '   Selected_MAPIDPG4   : 画面選択
    '   Selected_MAPIDPG5   : 画面選択
    '   Selected_MAPIDFrom  : 画面選択
    '   Selected_MAPIDTo    : 画面選択
    '   Selected_MAPIDG1    : 画面選択
    '   Selected_MAPIDG2    : 画面選択
    '   Selected_MAPIDG3    : 画面選択
    '   Selected_MAPIDG4    : 画面選択
    '   Selected_MAPIDG5    : 画面選択

    '   DBcon         : DB接続文字列 
    '   LOGdir        : ログ出力ディレクトリ 
    '   PDFdir        : PDF用ワークのディレクトリ
    '   FILEdir       : FILE格納ディレクトリ
    '   JNLdir        : 更新ジャーナル格納ディレクトリ

    '   MAPmapid      : 画面間IF(MAPID)
    '   MAPvariant    : 画面間IF(変数)
    '   MAPpermitcode : 画面間IF(権限)
    '   MAPetc        : 画面間IF(各PRGで利用)
    '   DRIVERS       : 事務用URL：初期URL(=htt://xxxx/OFFICE)、乗務員用URL：初期URL(=htt://xxxx/DRIVERS)
#End Region

    Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        '■■■　初期処理　■■■

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim COA0000DllMessage As New BASEDLL.COA0000DllMessage        'ライブラリ用メッセージ
        Dim COA0001WebconfStr As New BASEDLL.COA0001WebconfStr        'Web.config設定取得
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0005TermInfo As New BASEDLL.COA0005TermInfo            'サーバ情報取得
        Dim COA0007CompanyInfo As New BASEDLL.COA0007CompanyInfo      '会社情報取得
        Dim COA0009Encryption As New BASEDLL.COA0009Encryption        '文字列復号化

        If IsPostBack Then
            '************************************
            'セッション変数死活
            '************************************
            If COA0019Session.DBcon Is Nothing OrElse COA0019Session.DBcon = "" Then
                Dim comMessageNo = C_MESSAGENO.SESSIONEXPIRED
                Session(GbPageCommon.CONST_SESSION_COM_LOAD_MESSAGENO) = comMessageNo
                Server.Transfer(C_LOGIN_URL, False) '自身をリロードに遷移
                Return
            End If

            Me.txtPassword.Attributes.Add("value", Me.txtPassword.Text)
            '**********************
            ' テキストボックス変更判定
            '**********************
            If Me.hdnOnchangeField IsNot Nothing AndAlso Me.hdnOnchangeField.Value <> "" Then
                'テキストID + "_Change"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)
                Dim btnEventName As String = Me.hdnOnchangeField.Value & "_Change"
                Me.hdnOnchangeField.Value = ""
                '変更イベントが存在する場合は実行存在しない場合はスキップ
                Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(btnEventName)
                If mi IsNot Nothing Then
                    CallByName(Me, btnEventName, CallType.Method, Nothing)
                End If
            End If
            '**********************
            ' ボタンクリック判定
            '**********************
            'hdnButtonClickに文字列が設定されていたら実行する
            If Me.hdnButtonClick IsNot Nothing AndAlso Me.hdnButtonClick.Value <> "" Then
                'ボタンID + "_Click"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)
                Dim btnEventName As String = Me.hdnButtonClick.Value & "_Click"
                Me.hdnButtonClick.Value = ""
                CallByName(Me, btnEventName, CallType.Method, Nothing)
            End If
            '**********************
            ' ダブルクリック判定
            '**********************
            If Me.hdnLeftboxActiveViewId IsNot Nothing AndAlso Me.hdnLeftboxActiveViewId.Value <> "" Then
                '左ビュー表示
                DisplayLeftView()
                '隠し項目の表示ViewId保持項目をクリア
                Me.hdnLeftboxActiveViewId.Value = ""
            End If
        Else
            '■■■　セッション変数設定　■■■

            '○ 固定項目設定
            If String.IsNullOrEmpty(COA0019Session.USERID) Then

                COA0019Session.LANGDISP = C_LANG.EN
                COA0019Session.LANGLOG = C_LANG.JA
                COA0019Session.USERID = "INIT"
                COA0019Session.TERM = "INIT"
                COA0019Session.TERMCOMP = "INIT"
                COA0019Session.TERMORG = "INIT"
                COA0019Session.PROFID = "INIT"
                COA0019Session.SELECTEDCOMP = "INIT"
                COA0019Session.DRIVERS = ""

            End If

            '○ 表示言語設定
            langSetting()
            Me.btnLogin.Disabled = True

            '○ システムエラーメッセージ取得(システム管理者に連絡)
            Dim errMessage As String
            COA0000DllMessage.MessageCode = C_MESSAGENO.SYSTEMADM
            COA0000DllMessage.COA0000GetMesssage()
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errMessage = COA0000DllMessage.MessageStrEN
            Else
                errMessage = COA0000DllMessage.MessageStrJA
            End If

            '○ Web.config設定取得(InParm無し)
            '　表示文字コード未設定はデフォルトで英語
            COA0001WebconfStr.KeyStr = Trim(CONST_WEBCONFSTR_LANGDISP)
            COA0001WebconfStr.COA0001getWebconfStr()
            If COA0001WebconfStr.ERR = C_MESSAGENO.NORMAL Then
                COA0019Session.LANGDISP = COA0001WebconfStr.ConfStr
                langSetting()
            End If

            '　ログ出力文字コード未設定はデフォルトで日本語
            COA0001WebconfStr.KeyStr = Trim(CONST_WEBCONFSTR_LANGLOG)
            COA0001WebconfStr.COA0001getWebconfStr()
            If COA0001WebconfStr.ERR = C_MESSAGENO.NORMAL Then
                COA0019Session.LANGLOG = COA0001WebconfStr.ConfStr
            End If

            '　アプリINIファイルフルパス未設定は異常終了
            COA0001WebconfStr.KeyStr = Trim(CONST_WEBCONFSTR_APLINI)
            COA0001WebconfStr.COA0001getWebconfStr()
            If COA0001WebconfStr.ERR = C_MESSAGENO.NORMAL Then
                HttpContext.Current.Session("INIFILE") = COA0001WebconfStr.ConfStr
            Else
                Me.lblFooterMessage.CssClass = "ABNORMAL"
                Me.lblFooterMessage.Text = errMessage & "(ERROR:" & COA0001WebconfStr.ERR & ")"
                'サーバー処理終了を設定
                Me.hdnSubmit.Value = "FALSE"
                Return
            End If
            '********************************************************
            'INIファイルより各種値を設定しセッション変数に格納
            '********************************************************
            Try
                '○ APサーバー名称取得(InParm無し)
                HttpContext.Current.Session("APSRVname") = GetInifileValue("ap server", "name string", "value")
                '○ DB接続文字取得(InParm無し)
                COA0019Session.DBcon = GetInifileValue("sql server", "connection string", "value")
                '○ SQLサーバー接続パスワード取得(InParm無し)
                '復号化
                COA0009Encryption.TargetStr = GetInifileValue("sql password", "connection string", "value")
                COA0009Encryption.COA0009DecryptStr()
                'DB接続文字列
                COA0019Session.DBcon = COA0019Session.DBcon & ";UID=""sa"";Password=""" & COA0009Encryption.ConvStr & """"
                '○ FILESディレクトリ取得(InParm無し)
                COA0019Session.SYSTEMROOTDir = GetInifileValue("SYSTEMROOT directory", "directory string", "path")
                '○ FILESディレクトリ取得(InParm無し)
                COA0019Session.FILESDir = GetInifileValue("FILES directory", "directory string", "path")
                '○ HELPディレクトリ取得(InParm無し)
                COA0019Session.HELPDir = GetInifileValue("HELP directory", "directory string", "path")
                '○ PRINTFORMATディレクトリ取得(InParm無し)
                COA0019Session.PRINTFORMATDir = GetInifileValue("PRINTFORMAT directory", "directory string", "path")
                '○ ログ格納ディレクトリ取得(InParm無し)
                COA0019Session.LOGDir = GetInifileValue("LOG directory", "directory string", "path")
                '○ UPLOADFILESディレクトリ取得(InParm無し)
                COA0019Session.UPLOADFILESDir = GetInifileValue("UPLOADFILES directory", "directory string", "path")
                '○ SYSTEMディレクトリ取得(InParm無し)
                COA0019Session.USERTEMPDir = GetInifileValue("USERTEMP directory", "directory string", "path")
                '○ XMLディレクトリ取得(InParm無し)
                COA0019Session.XMLDir = GetInifileValue("XML directory", "directory string", "path")
                '○ PRINTディレクトリ取得(InParm無し)
                COA0019Session.PRINTWORKDir = GetInifileValue("PRINT directory", "directory string", "path")
                '○ UPLOADディレクトリ取得(InParm無し)
                COA0019Session.UPLOADDir = GetInifileValue("UPLOAD directory", "directory string", "path")
                '○ SENDディレクトリ取得(InParm無し)
                COA0019Session.SENDDir = GetInifileValue("SEND directory", "directory string", "path")
                '○ RECEIVEディレクトリ取得(InParm無し)
                COA0019Session.RECEIVEDir = GetInifileValue("RECEIVE directory", "directory string", "path")
                '○ BEFORE APPROVALディレクトリ取得(InParm無し)
                COA0019Session.BEFOREAPPROVALDir = GetInifileValue("BEFORE APPROVAL directory", "directory string", "path")
                '○ PRINTルートURL(開発などと分ける為)
                COA0019Session.PRINTROOTUrl = GetInifileValue("PRINTROOT url", "name string", "value", "print")
            Catch ex As Exception
                '取得した場合はGetInifileValueより共通COA0002IniFile.ERRをex.messageに設定しスロー
                Me.lblFooterMessage.CssClass = "ABNORMAL"
                Me.lblFooterMessage.Text = errMessage & "(ERROR:" & ex.Message & ")"
                'サーバー処理終了を設定
                Me.hdnSubmit.Value = "FALSE"
                Return
            End Try

            '○ APサーバー情報からAPサーバー設置会社(APSRVCamp)、APサーバー設置部署(APSRVOrg)取得
            COA0005TermInfo.TERMid = Convert.ToString(HttpContext.Current.Session("APSRVname"))
            COA0005TermInfo.COA0005GetTermInfo()
            If COA0005TermInfo.ERR = C_MESSAGENO.NORMAL Then
                '■■■　オンラインサービス判定　■■■
                If COA0005TermInfo.ONLINESW = 0 Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.ONLINESTOP, Me.lblFooterMessage)
                    'サーバー処理終了を設定
                    Me.hdnSubmit.Value = "FALSE"
                    Return
                Else
                    HttpContext.Current.Session("APSRVCamp") = COA0005TermInfo.TERMCOMP
                    HttpContext.Current.Session("APSRVOrg") = COA0005TermInfo.TERMORG
                    HttpContext.Current.Session("APSRVIp") = COA0005TermInfo.IPADDR
                    HttpContext.Current.Session("MOrg") = COA0005TermInfo.MORG
                    '■■■　運用ガイダンス表示　■■■
                    Me.lblGuidance.Text = COA0005TermInfo.TEXT.Replace(vbCrLf, "<br />")
                    If COA0005TermInfo.SYSTEMTITLE <> "" Then
                        Me.lblTitleText.Text = Trim(COA0005TermInfo.SYSTEMTITLE)
                    End If
                End If
            Else
                Me.lblFooterMessage.CssClass = "ABNORMAL"
                Me.lblFooterMessage.Text = errMessage & "(ERROR:" & COA0005TermInfo.ERR & ")"
                'サーバー処理終了を設定
                Me.hdnSubmit.Value = "FALSE"
                Return
            End If

            '■■■ 初期画面表示 ■■■
            '○ ヘッダー表示
            'ID、表題設定
            Me.lblTitleId.Text = "Logon"

            '会社設定
            COA0007CompanyInfo.COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
            COA0007CompanyInfo.STYMD = Date.Now
            COA0007CompanyInfo.ENDYMD = Date.Now
            COA0007CompanyInfo.COA0007getCompanyInfo()
            If COA0007CompanyInfo.ERR = C_MESSAGENO.NORMAL Then
                If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                    Me.lblTitleCompany.Text = COA0007CompanyInfo.NAMES_EN
                    HttpContext.Current.Session("APSRVCampName") = COA0007CompanyInfo.NAMES_EN
                Else
                    Me.lblTitleCompany.Text = COA0007CompanyInfo.NAMES
                    HttpContext.Current.Session("APSRVCampName") = COA0007CompanyInfo.NAMES
                End If
            Else
                Me.lblFooterMessage.CssClass = "ABNORMAL"
                Me.lblFooterMessage.Text = errMessage & "(" & COA0007CompanyInfo.ERR & ")"
                'サーバー処理終了を設定
                Me.hdnSubmit.Value = "FALSE"
                Return
            End If

            '現在日付設定
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                Me.lblTitleDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            Else
                Me.lblTitleDate.Text = DateTime.Now.ToString("yyyy年MM月dd日 HH時mm分")
            End If

            '■■■　初期メッセージ表示　■■■
            Dim messageNo As String = C_MESSAGENO.INPUTIDPASS 'ユーザID、パスワードを入力して下さい。
            Dim naeiw As String = C_NAEIW.INFORMATION
            If Session(GbPageCommon.CONST_SESSION_COM_LOAD_MESSAGENO) IsNot Nothing AndAlso
               (Convert.ToString(Session(GbPageCommon.CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL AndAlso
                Convert.ToString(Session(GbPageCommon.CONST_SESSION_COM_LOAD_MESSAGENO)) <> "") Then

                messageNo = Convert.ToString(Session(GbPageCommon.CONST_SESSION_COM_LOAD_MESSAGENO))
                naeiw = C_NAEIW.ABNORMAL
                Session(GbPageCommon.CONST_SESSION_COM_LOAD_MESSAGENO) = ""
            End If
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, naeiw:=naeiw)

            'C:\APPL\APPLFILES\XML_TMPディレクトリの不要データを掃除
            Dim WW_File As String

            For Each tempFile As String In System.IO.Directory.GetFiles(COA0019Session.XMLDir, "*", System.IO.SearchOption.AllDirectories)
                ' ファイルパスからファイル名を取得
                WW_File = System.IO.Path.GetFileName(tempFile)
                '本日作成以外のファイルは削除
                If Not WW_File.StartsWith(Date.Now.ToString("yyyyMMdd")) Then
                    System.IO.File.Delete(tempFile)
                End If
            Next tempFile

            '全社サーバーの場合、端末ＩＤのLISTBOXを表示する
            '■■■ 選択情報　設定処理 ■■■
            Dim WW_TermClass = COA0005TermInfo.TERMCLASS
            Dim LocalIp As String = COA0005TermInfo.IPADDR

            Dim RemoteIp As String = ""
            Try
                RemoteIp = Request.UserHostAddress

            Catch ex As Exception
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                            messageParams:=New List(Of String) From {"ERROR:89001"})
                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = ex.ToString()
                COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
                COA0003LogFile.COA0003WriteLog()                             'ログ出力
                'サーバー処理終了を設定
                Me.hdnSubmit.Value = "FALSE"
                Return
            End Try

            Dim LocalIp3 As String = Mid(LocalIp, 1, LocalIp.LastIndexOf("."))

            Dim RemoteIp3 As String = ""
            If RemoteIp.LastIndexOf(".") < 0 Then
                RemoteIp3 = LocalIp3
            Else
                RemoteIp3 = Mid(RemoteIp, 1, RemoteIp.LastIndexOf("."))
            End If

            If WW_TermClass = "2" AndAlso LocalIp3 <> RemoteIp3 Then
                '○左Boxへの値設定
                leftBox_init()
                Me.txtTermId.Visible = True
                Me.lblTermId.Visible = True
            Else
                '端末IDを非表示
                Me.txtTermId.Visible = False
                Me.lblTermId.Visible = False
            End If

            Me.txtUserId.Focus()
            Me.btnLogin.Disabled = False

        End If

        'サーバー処理終了を設定
        Me.hdnSubmit.Value = "FALSE"
    End Sub
    ''' <summary>
    ''' IniFileのキー3種を引数に値を取得
    ''' </summary>
    ''' <param name="keyStr1"></param>
    ''' <param name="keyStr2"></param>
    ''' <param name="keyStr3"></param>
    ''' <returns></returns>
    Private Function GetInifileValue(keyStr1 As String, keyStr2 As String, keyStr3 As String, Optional defaultValue As String = "") As String
        Try
            Dim COA0002IniFile As New BASEDLL.COA0002IniStr               'INIファイル設定取得
            With COA0002IniFile
                .KeyStr1 = keyStr1
                .KeyStr2 = keyStr2
                .KeyStr3 = keyStr3
                .COA0002getInistr()
                If .ERR = C_MESSAGENO.NORMAL Then
                    Return Trim(COA0002IniFile.IniStr)
                ElseIf defaultValue <> "" AndAlso .ERR = "90003" Then
                    Return Trim(defaultValue)
                Else
                    Throw New Exception(COA0002IniFile.ERR)
                End If
            End With
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function
    ''' <summary>
    ''' 左ビュー表示処理
    ''' </summary>
    Private Sub DisplayLeftView()
        Dim targetObject As Control = Nothing
        'ビューの存在チェック
        Dim changeViewObj As System.Web.UI.WebControls.View = DirectCast(Me.mvLeft.FindControl(Me.hdnLeftboxActiveViewId.Value), System.Web.UI.WebControls.View)
        If changeViewObj IsNot Nothing Then
            Me.mvLeft.SetActiveView(changeViewObj)
            Select Case changeViewObj.ID
                '他のビューが存在する場合はViewIdでCaseを追加
                '会社コードビュー表示切替
                Case Me.vLeftTermId.ID
                    Dim findItem = Me.lbTermId.Items.FindByValue(Me.txtTermId.Text)
                    If findItem IsNot Nothing Then
                        findItem.Selected = True
                    End If
                    Me.mvLeft.Focus()
            End Select
        End If

    End Sub
    ''' <summary>
    ''' LOGONボタン押下時処理
    ''' </summary>
    Public Sub btnLogin_Click()

        '■■■　初期処理　■■■

        '○共通宣言
        '*共通関数宣言(APPLDLL)
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0005TermInfo As New BASEDLL.COA0005TermInfo            'サーバ情報取得
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar      '文字列禁則文字除去
        Dim COA0009Encryption As New BASEDLL.COA0009Encryption        '文字列復号化

        '○オンラインサービス判定
        COA0005TermInfo.TERMid = Convert.ToString(HttpContext.Current.Session("APSRVname"))
        COA0005TermInfo.COA0005GetTermInfo()
        If COA0005TermInfo.ERR = C_MESSAGENO.NORMAL Then
            '■■■　オンラインサービス判定　■■■
            If COA0005TermInfo.ONLINESW = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.ONLINESTOP, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
                'サーバー処理終了を設定
                Me.hdnSubmit.Value = "FALSE"
                Return
            End If
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"ERROR:" & COA0005TermInfo.ERR})
            'サーバー処理終了を設定
            Me.hdnSubmit.Value = "FALSE"
            Return
        End If

        '○セッション変数設定
        HttpContext.Current.Session("Class") = "WF_ButtonOK_Click"

        '■■■　メイン処理　■■■

        '○ 入力文字内の禁止文字排除
        '   画面UserID内の使用禁止文字排除
        COA0008InvalidChar.CHARin = Me.txtUserId.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin <> COA0008InvalidChar.CHARout Then
            Me.txtUserId.Text = COA0008InvalidChar.CHARout
        End If

        '   画面PassWord内の使用禁止文字排除
        COA0008InvalidChar.CHARin = Me.txtPassword.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin <> COA0008InvalidChar.CHARout Then
            Me.txtPassword.Text = COA0008InvalidChar.CHARout
        End If

        '○ 画面UserIDのDB(S0004_USER)存在チェック
        Dim WW_USERID As String = ""
        Dim WW_USERNAME As String = ""
        Dim WW_SYSCODE As String = ""
        Dim WW_PASSWORD As String = ""
        Dim WW_STYMD As Date = Date.Now
        Dim WW_ENDYMD As Date = Date.Now
        Dim WW_PASSENDYMD As Date
        Dim WW_MISSCNT As Integer = 0
        Dim WW_ORG As String = ""
        Dim WW_DEFAULTSRV As String = ""
        Dim WW_LOGINFLG As String = ""
        Dim WW_MAPID As String = ""
        Dim WW_VARIANT As String = ""
        Dim WW_LANGDISP As String = ""
        Dim WW_PROFID As String = ""
        Dim WW_UPDYMD As Date
        Dim WW_UPDTIMSTP As Byte()
        Dim WW_err As String = ""

        Try
            'S0004_USER検索SQL文
            Dim SQL_Str As String =
                 "SELECT rtrim(A.USERID) as USERID , A.STYMD , A.ENDYMD , rtrim(B.PASSWORD) as PASSWORD , B.MISSCNT , C.SYSCODE , rtrim(A.ORG) as ORG ," _
               & " rtrim(A.STAFFNAMES) as STAFFNAMES , rtrim(A.STAFFNAMES_EN) as STAFFNAMES_EN , " _
               & " rtrim(A.DEFAULTSRV) as DEFAULTSRV , rtrim(A.LOGINFLG) as LOGINFLG , rtrim(A.MAPID) as MAPID , rtrim(A.VARIANT) as VARIANT , " _
               & " rtrim(A.PROFID) as PROFID , rtrim(A.LANGDISP) as LANGDISP , B.PASSENDYMD , A.INITYMD , A.UPDYMD , A.UPDTIMSTP " _
               & " FROM  COS0005_USER A " _
               & " INNER JOIN COS0006_USERPASS B " _
               & "   ON  B.USERID      = A.USERID " _
               & "   and B.DELFLG     <> @DELFLG " _
               & " INNER JOIN COS0021_ORG C " _
               & "   ON   C.STYMD      <= @STYMD " _
               & "   and  C.ENDYMD     >= @ENDYMD " _
               & "   and  C.DELFLG     <> @DELFLG " _
               & "   and  C.ORGCODE    = A.ORG " _
               & " INNER JOIN COS0021_ORG D " _
               & "   ON   D.STYMD      <= @STYMD " _
               & "   and  D.ENDYMD     >= @ENDYMD " _
               & "   and  D.DELFLG     <> @DELFLG " _
               & "   and  D.ORGCODE    = C.MORGCODE " _
               & "   and  D.ORGLEVEL   = '" & GBC_ORGLEVEL.COUNTRY & "' " _
               & " Where A.USERID      = @USERID " _
               & "   and A.STYMD      <= @STYMD " _
               & "   and A.ENDYMD     >= @ENDYMD " _
               & "   and A.DELFLG <> @DELFLG "
            'DataBase接続文字
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                With SQLcmd.Parameters
                    .Add("@USERID", System.Data.SqlDbType.Char, 20).Value = Me.txtUserId.Text
                    .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_YES
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    WW_err = C_MESSAGENO.WRONGIDPASS

                    While SQLdr.Read
                        WW_USERID = Convert.ToString(SQLdr("USERID"))
                        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                            WW_USERNAME = Convert.ToString(SQLdr("STAFFNAMES_EN"))
                        Else
                            WW_USERNAME = Convert.ToString(SQLdr("STAFFNAMES"))
                        End If
                        WW_SYSCODE = Convert.ToString(SQLdr("SYSCODE"))
                        WW_PASSWORD = Convert.ToString(SQLdr("PASSWORD"))
                        '復号化
                        COA0009Encryption.TargetStr = Trim(WW_PASSWORD)
                        COA0009Encryption.COA0009DecryptStr()
                        WW_PASSWORD = COA0009Encryption.ConvStr
                        WW_STYMD = CDate(SQLdr("STYMD"))
                        WW_ENDYMD = CDate(SQLdr("ENDYMD"))
                        WW_MISSCNT = CInt(SQLdr("MISSCNT"))
                        WW_PASSENDYMD = CDate(SQLdr("PASSENDYMD"))
                        WW_ORG = Convert.ToString(SQLdr("ORG"))
                        WW_DEFAULTSRV = Convert.ToString(SQLdr("DEFAULTSRV"))
                        WW_LOGINFLG = Convert.ToString(SQLdr("LOGINFLG"))
                        WW_MAPID = Convert.ToString(SQLdr("MAPID"))
                        WW_VARIANT = Convert.ToString(SQLdr("VARIANT"))
                        WW_LANGDISP = Convert.ToString(SQLdr("LANGDISP"))
                        WW_PROFID = Convert.ToString(SQLdr("PROFID"))
                        WW_UPDYMD = CDate(SQLdr("UPDYMD"))
                        WW_UPDTIMSTP = CType(SQLdr("UPDTIMSTP"), Byte())
                        WW_err = C_MESSAGENO.NORMAL
                        Exit While
                    End While
                End Using 'SQLdr
            End Using 'SQLcon,SQLcmd
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"ERROR:89001"})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()                             'ログ出力
            Return
        End Try
#Region "しばらくコメントで残すがRegionない削除、必要なセッション情報を格納していないためこのタイミングでチェック→パス変更画面遷移へは行わず処理下部に移動"
        ''パスワード期限切れ
        'If WW_err = C_MESSAGENO.NORMAL AndAlso WW_PASSENDYMD < Date.Now Then
        '    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
        '                                messageParams:=New List(Of String) From {"ERROR:70002"})

        '    'サーバー処理終了を設定
        '    Me.hdnSubmit.Value = "FALSE"
        '    Return
        'End If

#End Region

        'ユーザID誤り
        If (WW_err <> C_MESSAGENO.NORMAL OrElse Me.txtUserId.Text = "Default" OrElse Me.txtUserId.Text = "INIT") Then
            CommonFunctions.ShowMessage(C_MESSAGENO.WRONGIDPASS, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            Me.txtUserId.Focus()
            Return
        End If

        '○ パスワードチェック
        If (Me.txtPassword.Text <> WW_PASSWORD) Then
            CommonFunctions.ShowMessage(C_MESSAGENO.WRONGIDPASS, Me.lblFooterMessage, naeiw:=C_NAEIW.ABNORMAL, pageObject:=Me)
            'パスワードエラー回数のカウントUP
            Try
                Dim SQL_Str As String =
                     "Update COS0006_USERPASS " _
                   & "Set    MISSCNT = @MISSCNT , UPDYMD = @UPDYMD , UPDUSER = @USERID , RECEIVEYMD = @RECEIVEYMD " _
                   & "Where  USERID  = @USERID "

                'DataBase接続文字
                Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                      SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)
                    With SQLcmd.Parameters
                        If WW_MISSCNT = 999 Then
                            .Add("@MISSCNT", SqlDbType.Int).Value = WW_MISSCNT
                        Else
                            .Add("@MISSCNT", SqlDbType.Int).Value = WW_MISSCNT + 1
                        End If
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = Date.Now
                        .Add("@USERID", SqlDbType.Char, 20).Value = Me.txtUserId.Text
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With
                    SQLcmd.ExecuteNonQuery()
                End Using
            Catch ex As Exception
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"ERROR:89001"})

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = ex.ToString()
                COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
                COA0003LogFile.COA0003WriteLog()                             'ログ出力
            End Try
            Me.txtUserId.Focus()
            Return

        End If

        '○ アカウントロックチェック
        '最大ミスカウント取得
        Dim maxMissCnt As Integer = GetMaxMissCount()

        If (WW_MISSCNT >= maxMissCnt) Then
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"ERROR:{0}", C_MESSAGENO.ACCOUNTLOCKED})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = String.Format("パスワードロック(USERID={0}、PASS={1}、MISSCNT={2})", Me.txtUserId.Text, Me.txtPassword.Text, WW_MISSCNT)
            COA0003LogFile.MESSAGENO = C_MESSAGENO.ACCOUNTLOCKED
            COA0003LogFile.COA0003WriteLog()                             'ログ出力
            CommonFunctions.ShowMessage("70001", lblFooterMessage, pageObject:=Me)
            Return
        End If

        '○ パスワードチェックＯＫ時処理
        'セッション情報（ユーザＩＤ）設定
        COA0019Session.USERID = Me.txtUserId.Text.Trim
        COA0019Session.USERNAME = WW_USERNAME
        COA0019Session.SYSCODE = WW_SYSCODE
        COA0019Session.USERORG = WW_ORG
        COA0019Session.LANGDISP = WW_LANGDISP
        COA0019Session.PROFID = WW_PROFID

        'ミスカウントクリア
        Try
            'S0014_USER更新SQL文
            Dim SQL_Str As String =
                 "Update COS0006_USERPASS " _
               & "Set    MISSCNT = @MISSCNT , UPDYMD = @UPDYMD , UPDUSER = @USERID , RECEIVEYMD = @RECEIVEYMD " _
               & "Where  USERID  = @USERID "
            'DataBase接続文字
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                With SQLcmd.Parameters
                    .Add("@MISSCNT", System.Data.SqlDbType.Int).Value = 0
                    .Add("@UPDYMD", System.Data.SqlDbType.DateTime).Value = Date.Now
                    .Add("@USERID", System.Data.SqlDbType.Char, 20).Value = Me.txtUserId.Text
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {"ERROR:89001"})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()                             'ログ出力
            Return
        End Try

        '代行
        '端末ＩＤチェック＆セッション変数の設定
        '存在チェック(LeftBox存在しない場合エラー)
        For i As Integer = 0 To lbTermId.Items.Count - 1
            If lbTermId.Items(i).Value = Me.txtTermId.Text Then
                HttpContext.Current.Session("APSRVname") = txtTermId.Text
                COA0005TermInfo.TERMid = Convert.ToString(HttpContext.Current.Session("APSRVname"))
                COA0005TermInfo.COA0005GetTermInfo()
                If COA0005TermInfo.ERR = C_MESSAGENO.NORMAL Then
                    HttpContext.Current.Session("APSRVCamp") = COA0005TermInfo.TERMCOMP
                    HttpContext.Current.Session("APSRVOrg") = COA0005TermInfo.TERMORG
                    HttpContext.Current.Session("MOrg") = COA0005TermInfo.MORG
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.WRONGTERMID, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If

                Exit For
            End If

            If (i >= (Me.lbTermId.Items.Count - 1)) Then
                CommonFunctions.ShowMessage(C_MESSAGENO.WRONGTERMID, Me.lblFooterMessage, pageObject:=Me)

                Me.txtTermId.Focus()
                Return
            End If
        Next

        'デフォルトサーバ接続チェック
        If ((WW_LOGINFLG = BaseDllCommon.CONST_FLAG_YES) AndAlso (Me.hdnLoginFlg.Value <> Me.txtUserId.Text) AndAlso (WW_DEFAULTSRV <> Convert.ToString(HttpContext.Current.Session("APSRVname")))) Then
            'メッセージボックスを表示する
            CommonFunctions.ShowMessage(C_MESSAGENO.CONNECTOTHERSERVER, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            Me.hdnLoginFlg.Value = Me.txtUserId.Text
            Me.btnLogin.Focus()
            Return
        Else
            Me.hdnLoginFlg.Value = ""
        End If
        'パスワード有効期限チェック
        Dim IsPasswordExpired As Boolean = False
        If WW_PASSENDYMD < Date.Now Then
            IsPasswordExpired = True
        End If
        '■■■　終了処理　■■■

        '○ パスワードチェックＯＫ時、指定画面へ遷移
        'ユーザマスタより、MAPIDおよびVARIANTを取得
        Dim WW_URL As String = ""

        Try
            If IsPasswordExpired Then
                'パスワード期限切れの場合は変更画面の情報を取得
                Dim COA0012DoUrl As New COA0012DoUrl With {
                    .MAPIDP = CONST_MAPID, .VARIP = CONST_MAPVARI
                }
                COA0012DoUrl.COA0012GetDoUrl()
                If COA0012DoUrl.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                            messageParams:=New List(Of String) From {"ERROR:89001"})
                    Return
                End If
                WW_URL = COA0012DoUrl.URL
                WW_VARIANT = CONST_PASSINIT_MAPVARI
            Else
                'パスワード期限内の場合はメニューURL取得
                WW_URL = GetMenuUrl(WW_MAPID)
            End If
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"ERROR:89001"})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()                  'ログ出力

            Return
        End Try

        Dim WW_DATENOW As Date = Date.Now
        Try
            '日報ＤＢ更新
            Dim SQLStr As String =
                          " DECLARE @hensuu as bigint ; " _
                        & " set @hensuu = 0 ; " _
                        & " DECLARE hensuu CURSOR FOR " _
                        & "   SELECT CAST(UPDTIMSTP as bigint) as hensuu  " _
                        & "     FROM COS0007_LOGONYMD " _
                        & "WHERE TERMID    = @TERMID " _
                        & " OPEN hensuu ; " _
                        & " FETCH NEXT FROM hensuu INTO @hensuu ; " _
                        & " IF ( @@FETCH_STATUS = 0 ) " _
                        & "    UPDATE COS0007_LOGONYMD " _
                        & "    SET LOGONYMD    = @LOGONYMD " _
                        & "      , UPDYMD      = @UPDYMD " _
                        & "      , UPDUSER     = @UPDUSER " _
                        & "      , UPDTERMID   = @UPDTERMID " _
                        & "      , RECEIVEYMD  = @RECEIVEYMD  " _
                        & "    WHERE TERMID    = @TERMID ; " _
                        & " IF ( @@FETCH_STATUS <> 0 ) " _
                        & "    INSERT INTO COS0007_LOGONYMD " _
                        & "             (TERMID , " _
                        & "              LOGONYMD , " _
                        & "              INITYMD , " _
                        & "              UPDYMD , " _
                        & "              UPDUSER ,  " _
                        & "              UPDTERMID , " _
                        & "              RECEIVEYMD ) " _
                        & "      VALUES (@TERMID,@LOGONYMD,@INITYMD,@UPDYMD,@UPDUSER,@UPDTERMID,@RECEIVEYMD); " _
                        & " CLOSE hensuu ; " _
                        & " DEALLOCATE hensuu ; "
            'DataBase接続文字
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd As SqlCommand = New SqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                With SQLcmd.Parameters
                    .Add("@TERMID", System.Data.SqlDbType.Char, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@LOGONYMD", System.Data.SqlDbType.Char, 20).Value = WW_DATENOW
                    .Add("@INITYMD", System.Data.SqlDbType.SmallDateTime).Value = WW_DATENOW
                    .Add("@UPDYMD", System.Data.SqlDbType.DateTime).Value = WW_DATENOW
                    .Add("@UPDUSER", System.Data.SqlDbType.Char, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", System.Data.SqlDbType.Char, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"ERROR:89001"})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()                             'ログ出力
            Return

        End Try

        Dim WW_LOGONYMD As String = Date.Now.ToString("yyyy/MM/dd")
        Try
            'S0020_LOGONYMD検索SQL文
            Dim SQL_Str As String =
                 "SELECT isnull(convert(char,LOGONYMD,111), '') as LOGONYMD " _
               & " FROM  COS0007_LOGONYMD " _
               & " Where TERMID   = @TERMID "
            'DataBase接続文字
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd As New SqlCommand(SQL_Str, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                With SQLcmd.Parameters
                    .Add("@TERMID", System.Data.SqlDbType.Char, 30).Value = HttpContext.Current.Session("APSRVname")
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        Try
                            WW_LOGONYMD = Convert.ToString(SQLdr("LOGONYMD"))
                        Catch ex As Exception
                            WW_LOGONYMD = Date.Now.ToString("yyyy/MM/dd")
                        End Try
                        Exit While
                    End While
                End Using 'SQLdr
            End Using 'SQLcon SQLcmd

        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"ERROR:89001"})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()                             'ログ出力
            Return
        End Try

        'GBシステム固有設定
        If COA0019Session.SYSCODE = C_SYSCODE_GB Then
            Dim GBA00003UserSetting As New GBA00003UserSetting
            GBA00003UserSetting.USERID = Me.txtUserId.Text
            GBA00003UserSetting.GBA00003GetUserSetting()
        End If

        '次画面の変数セット
        HttpContext.Current.Session("MAPmapid") = WW_MAPID
        HttpContext.Current.Session("MAPvariant") = WW_VARIANT
        HttpContext.Current.Session("MAPpermitcode") = ""
        HttpContext.Current.Session("MAPetc") = ""

        HttpContext.Current.Session("LogonYMD") = WW_LOGONYMD

        '画面遷移実行
        If COA0019Session.USERID = "INIT" Then
        Else
            Server.Transfer(WW_URL)
        End If

    End Sub

    ' ******************************************************************************
    ' ***  leftBOXのListBox値設定                                                ***     '2015/12/10 ADD
    ' ******************************************************************************
    Private Sub leftBox_init()

        Dim COA0000DllMessage As New BASEDLL.COA0000DllMessage
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力

        '■■■ セッション変数設定 ■■■
        '○ 固定項目設定  ★必須処理
        Session("Class") = "leftBox_init"

        '○ 端末ID
        Try
            Dim getColumn As String = "TERMNAME"
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                getColumn = getColumn & "_" & Trim(COA0019Session.LANGDISP)
            End If

            Dim SQLStr As String =
                    " SELECT TERMID, rtrim(" & getColumn & ") as TERMNAME " &
                    " FROM COS0001_TERM " &
                    " WHERE TERMCLASS     =  '1' " &
                    " AND   STYMD        <= getdate() " &
                    " AND   ENDYMD       >= getdate() " &
                    " AND   DELFLG       <> " & "'" & BaseDllCommon.CONST_FLAG_YES & "'"
            'DataBase接続文字
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd As New SqlCommand(SQLStr, SQLcon)
                SQLcon.Open() 'DataBase接続(Open)
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        Me.lbTermId.Items.Add(New ListItem(Convert.ToString(SQLdr("TERMNAME")), Convert.ToString(SQLdr("TERMID"))))
                    End While
                End Using
            End Using
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {"ERROR:89001"})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()                             'ログ出力

            'サーバー処理終了を設定
            Me.hdnSubmit.Value = "FALSE"
            Return
        End Try

    End Sub

    ''' <summary>
    ''' 左ボックス選択ボタン押下時
    ''' </summary>
    Public Sub btnLeftBoxButtonSel_Click()
        Dim targetObject As Control = Nothing
        '現在表示している左ビューを取得
        Dim activeViewObj As View = Me.mvLeft.GetActiveView
        If activeViewObj IsNot Nothing Then
            Select Case activeViewObj.ID
                'ビューごとの処理はケースを追加で実現
                Case Me.vLeftTermId.ID 'アクティブなビューが会社コード
                    '会社コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbTermId.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbTermId.SelectedItem.Value
                            Me.lblTermIdText.Text = Me.lbTermId.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblTermIdText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Else
                    '何もしない
            End Select
        End If
        '○ 画面左サイドボックス非表示は、画面JavaScriptで実行
        Me.hdnTextDbClickField.Value = ""
        Me.hdnIsLeftBoxOpen.Value = ""
    End Sub
    ''' <summary>
    ''' 左ボックスキャンセルボタン押下時
    ''' </summary>
    Public Sub btnLeftBoxButtonCan_Click()
        'フォーカスセット
        Dim dblClickField As Control
        dblClickField = Me.FindControl(Me.hdnTextDbClickField.Value)
        If dblClickField IsNot Nothing Then
            'この規則性ではない場合は適宜個別に設定
            dblClickField.Focus()
        End If
        '○ 画面左サイドボックス非表示は、画面JavaScriptで実行
        Me.hdnTextDbClickField.Value = ""
        Me.hdnIsLeftBoxOpen.Value = ""
    End Sub
    ''' <summary>
    ''' TermId変更時イベント
    ''' </summary>
    Public Sub txtTermId_Change()
        Dim termId As String = Me.txtTermId.Text.Trim
        Me.lblTermIdText.Text = ""
        If termId = "" Then
            Return
        End If
        Dim findTermId = Me.lbTermId.Items.FindByValue(termId)
        If findTermId IsNot Nothing Then
            Me.txtTermId.Text = termId
            Me.lblTermIdText.Text = findTermId.Text
        End If

    End Sub

    ''' <summary>
    ''' 初期表示言語設定     
    ''' </summary>
    Protected Sub langSetting()

        '■■■ セッション変数設定 ■■■
        '○ 固定項目設定  ★必須処理
        Session("Class") = "langSetting"

        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
            '英語表示設定
            Me.lblUserId.Text = "User Account"
            Me.lblPassword.Text = "Password"
            Me.lblTermId.Text = "Substitute terminal"
            Me.btnLogin.Value = "Logon"
            Me.btnLeftBoxButtonSel.Value = "Select"
            Me.btnLeftBoxButtonCan.Value = "Cancel"
            '端末IDを非表示
            Me.txtTermId.Visible = False
            Me.lblTermId.Visible = False
        Else
            '日本語表示設定
            Me.lblUserId.Text = "ユーザＩＤ"
            Me.lblPassword.Text = "パスワード"
            Me.lblTermId.Text = "代行端末ＩＤ"
            Me.btnLogin.Value = "実行"
            Me.btnLeftBoxButtonSel.Value = "　選　択　"
            Me.btnLeftBoxButtonCan.Value = "キャンセル"
            Me.txtTermId.Visible = False
            Me.lblTermId.Visible = False
        End If

    End Sub
    ''' <summary>
    ''' FixValueより最大ミスカウントを取得
    ''' </summary>
    ''' <returns>最大ミスすう</returns>
    Private Function GetMaxMissCount() As Integer
        '取得できない場合6を返却
        Dim retVal As Integer = 6
        Dim COA0017FixValue As New COA0017FixValue With
            {.COMPCODE = GBC_COMPCODE_D, .CLAS = "LOGIN_SETTINGS"}
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR <> C_MESSAGENO.NORMAL Then
            Dim retMaxCnt = COA0017FixValue.VALUEDIC("MAXMISSCOUNT")
            retVal = CInt(retMaxCnt(1))
        End If
        Return retVal

    End Function
    ''' <summary>
    ''' メニュー画面へのURLを取得
    ''' </summary>
    ''' <param name="mapId">MAPID</param>
    ''' <returns></returns>
    Private Function GetMenuUrl(mapId As String) As String

        Dim retUrl As String = ""
        'S0009_URL検索SQL文
        Dim SQL_Str As String =
             "SELECT rtrim(URL) as URL " _
           & " FROM  COS0008_URL " _
           & " Where MAPID    = @MAPID " _
           & "   and STYMD   <= @STYMD " _
           & "   and ENDYMD  >= @ENDYMD " _
           & "   and DELFLG  <> @DELFLG "

        'DataBase接続文字
        Using SQLcon As New SqlConnection(COA0019Session.DBcon),
              SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            SQLcon.Open() 'DataBase接続(Open)
            With SQLcmd.Parameters
                .Add("@MAPID", System.Data.SqlDbType.Char, 50).Value = mapId
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_YES
            End With

            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                While SQLdr.Read
                    retUrl = Convert.ToString(SQLdr("URL"))
                    Exit While
                End While
            End Using
        End Using
        Return retUrl
    End Function
End Class