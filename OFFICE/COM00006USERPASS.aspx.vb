Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net
Imports BASEDLL

''' <summary>
''' ユーザーパスワード変更画面クラス
''' </summary>
Public Class COM00006USERPASS
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "COM00006"    '自身のMAPID
    Private Const CONST_TBLMASTER = "COS0006_USERPASS"

    Private COM00006ds As DataSet                         'Jnl格納ＤＳ
    Private COM00006tbl As DataTable                      'Jnl格納用テーブル
    Private Const CONST_DATATABLE = "COM00006TBL"

    Private returnCode As String = String.Empty         'サブ用リターンコード

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' ページロード時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            COA0003LogFile = New COA0003LogFile                             'ログ出力
            Dim COA0031ProfMap As New COA0031ProfMap

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            '画面モード（更新・参照）設定
            If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) = "2" Then
                hdnMAPpermitCode.Value = "TRUE"
            Else
                hdnMAPpermitCode.Value = "FALSE"
            End If

            'リターンコード設定
            returnCode = C_MESSAGENO.NORMAL

            '****************************************
            'メッセージ初期化
            '****************************************
            Me.lblFooterMessage.Text = ""
            'Me.lblFooterMessage.ForeColor = Color.Black
            'Me.lblFooterMessage.Font.Bold = False

            '作業用データベース設定
            COM00006ds = New DataSet()                                      '初期化
            COM00006tbl = COM00006ds.Tables.Add(CONST_DATATABLE)

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'タイトル設定
                '****************************************
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = "Default"
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                End If

                '****************************************
                '有効期限表示・戻るボタン制御
                '****************************************
                If Me.hdnThisMapVariant.Value <> "PassExpired" Then
                    SetEffectiveDate()
                    If returnCode <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
                        Return
                    End If
                Else
                    'ログイン画面からパスワード期限切れで遷移の場合
                    CommonFunctions.ShowMessage("70002", Me.lblFooterMessage, pageObject:=Me)
                    Me.btnBack.Visible = False
                End If

                '****************************************
                'フォーカス設定
                '****************************************
                txtNewPass.Focus()

            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
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
                ' Help表示
                '**********************
                If Me.hdnHelpChange.Value IsNot Nothing AndAlso Me.hdnHelpChange.Value <> "" Then
                    DivShowHelp_DoubleClick(CONST_MAPID)
                    Me.hdnHelpChange.Value = ""
                End If
            End If
            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定

        Catch ex As Threading.ThreadAbortException
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.EXCEPTION
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = messageNo
            COA0003LogFile.COA0003WriteLog()

            Return
        End Try
    End Sub
    ''' <summary>
    ''' 有効期限表示
    ''' </summary>
    Protected Sub SetEffectiveDate()
        COA0003LogFile = New COA0003LogFile                         'ログ出力
        returnCode = C_MESSAGENO.NORMAL

        'DataBase接続文字
        Dim SQLStr As String = Nothing

        '画面表示用データ取得
        Try
            '検索SQL文
            SQLStr =
                 "SELECT * " _
               & " FROM  COS0006_USERPASS " _
               & " Where USERID  = @USERID " _
               & "   and DELFLG <> @DELFLG "

            'DataBase接続文字
            Using sqlCon = New SqlConnection(COA0019Session.DBcon),
                  sqlCmd = New SqlCommand(SQLStr, sqlCon)
                'DataBase接続(Open)
                sqlCon.Open()
                With sqlCmd.Parameters
                    .Add("@USERID", System.Data.SqlDbType.Char, 20).Value = COA0019Session.USERID
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_YES
                End With
                Using sqlDr = sqlCmd.ExecuteReader()
                    While sqlDr.Read
                        Dim EffectiveDate As Date = Nothing
                        Dim ConvEffectiveDate As String = Nothing
                        If Date.TryParse(Convert.ToString(sqlDr("PASSENDYMD")), EffectiveDate) Then
                            ConvEffectiveDate = EffectiveDate.ToString("yyyy年MM月dd日")

                            If GBA00003UserSetting.DATEFORMAT IsNot Nothing AndAlso GBA00003UserSetting.DATEFORMAT <> "" Then
                                ConvEffectiveDate = EffectiveDate.ToString(GBA00003UserSetting.DATEFORMAT)
                            ElseIf COA0019Session.LANGDISP = C_LANG.JA Then
                                ConvEffectiveDate = EffectiveDate.ToString("yyyy年MM月dd日")
                            Else
                                ConvEffectiveDate = EffectiveDate.ToString("MMM/dd/yyyy")
                            End If
                            CommonFunctions.ShowMessage(C_MESSAGENO.PASSEXPIREINFO, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL,
                                                messageParams:=New List(Of String) From {ConvEffectiveDate})
                        End If
                        Exit While
                    End While
                End Using 'sqlDr
            End Using 'sqlCon,sqlCmd

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
            Return
        End Try

    End Sub
    ''' <summary>
    ''' 更新ボタン押下時
    ''' </summary>
    Public Sub btnUpdate_Click()
        COA0003LogFile = New COA0003LogFile                         'ログ出力
        Dim COA0030Journal As New COA0030Journal            'Journal Out
        Dim COA0009Encryption As New COA0009Encryption      '文字列復号化

        'DataBase接続文字
        Dim SQLStr As String = Nothing
        Try

            '入力内容チェック
            InputDataCheck()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

            '更新SQL文･･･マスタへ更新
            Dim nowDate As DateTime = Date.Now

            SQLStr =
                    "  UPDATE " & CONST_TBLMASTER _
                  & "  SET PASSWORD   = @PASSWORD , " _
                  & "      MISSCNT    = @MISSCNT , " _
                  & "      PASSENDYMD = @PASSENDYMD , " _
                  & "      DELFLG     = @DELFLG , " _
                  & "      UPDYMD     = @UPDYMD , " _
                  & "      UPDUSER    = @UPDUSER , " _
                  & "      UPDTERMID  = @UPDTERMID , " _
                  & "      RECEIVEYMD = @RECEIVEYMD " _
                  & "  WHERE USERID   = @USERID ; "
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
                  sqlCmd = New SqlCommand(SQLStr, sqlCon)
                sqlCon.Open() 'DataBase接続(Open)
                With sqlCmd.Parameters
                    .Add("@USERID", System.Data.SqlDbType.Char, 20).Value = COA0019Session.USERID
                    '暗号化
                    COA0009Encryption.TargetStr = Trim(txtNewPass.Text)
                    COA0009Encryption.COA0009EncryptStr()
                    .Add("@PASSWORD", System.Data.SqlDbType.Char, 200).Value = COA0009Encryption.ConvStr
                    .Add("@MISSCNT", System.Data.SqlDbType.Int).Value = 0
                    .Add("@PASSENDYMD", System.Data.SqlDbType.Date).Value = nowDate.AddMonths(3)
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_NO
                    .Add("@UPDYMD", System.Data.SqlDbType.DateTime).Value = nowDate
                    .Add("@UPDUSER", System.Data.SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", System.Data.SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", System.Data.SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                sqlCmd.ExecuteNonQuery()
            End Using

            '更新ジャーナル追加
            COM00006tbl.Clear()
            COM00006tbl.Columns.Add("USERID", GetType(String))
            COM00006tbl.Columns.Add("PASSWORD", GetType(String))
            COM00006tbl.Columns.Add("MISSCNT", GetType(Integer))
            COM00006tbl.Columns.Add("PASSENDYMD", GetType(Date))
            COM00006tbl.Columns.Add("DELFLG", GetType(String))
            'COM00006tbl.Columns.Add("INITYMD", GetType(Date))
            COM00006tbl.Columns.Add("UPDYMD", GetType(DateTime))
            COM00006tbl.Columns.Add("UPDUSER", GetType(String))
            COM00006tbl.Columns.Add("UPDTERMID", GetType(String))
            COM00006tbl.Columns.Add("RECEIVEYMD", GetType(DateTime))
            COM00006ds.EnforceConstraints = False
            COA0030Journal.ROW = COM00006tbl.NewRow

            COA0030Journal.TABLENM = CONST_TBLMASTER
            COA0030Journal.ACTION = "UPDATE"

            COA0030Journal.ROW("USERID") = COA0019Session.USERID
            '暗号化
            COA0009Encryption.TargetStr = Trim(txtNewPass.Text)
            COA0009Encryption.COA0009EncryptStr()
            COA0030Journal.ROW("PASSWORD") = COA0009Encryption.ConvStr
            COA0030Journal.ROW("MISSCNT") = 0
            COA0030Journal.ROW("PASSENDYMD") = nowDate.AddMonths(3)
            COA0030Journal.ROW("DELFLG") = BaseDllCommon.CONST_FLAG_NO
            'COA0030Journal.ROW("INITYMD") = nowDate
            COA0030Journal.ROW("UPDYMD") = nowDate
            COA0030Journal.ROW("UPDUSER") = COA0019Session.USERID
            COA0030Journal.ROW("UPDTERMID") = HttpContext.Current.Session("APSRVname")
            COA0030Journal.ROW("RECEIVEYMD") = CONST_DEFAULT_RECEIVEYMD
            COA0030Journal.COA0030SaveJournal()

            If COA0030Journal.ERR = C_MESSAGENO.NORMAL Then
            Else
                CommonFunctions.ShowMessage(COA0030Journal.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
            'メッセージ表示
            If returnCode = C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            End If

            '項目クリア
            txtNewPass.Text = ""
            txtReNewPass.Text = ""
            'カーソル設定
            txtNewPass.Focus()

            'ログイン画面からの遷移（パスワード期限切れ）の場合更新後にログイン画面へ遷移
            If Me.hdnThisMapVariant.Value = "PassExpired" Then
                '本来ログイン直後に遷移する画面へ遷移
                Dim dr As DataRow = GetUserMapInfo()
                Dim mapId As String = Convert.ToString(dr("MAPID"))
                Dim url As String = GetMenuUrl(mapId)
                '次画面の変数セット
                HttpContext.Current.Session("MAPmapid") = mapId
                HttpContext.Current.Session("MAPvariant") = Convert.ToString(dr("VARIANT"))
                HttpContext.Current.Session("MAPpermitcode") = ""
                HttpContext.Current.Session("MAPetc") = ""
                Server.Transfer(url)
            End If
        Catch ex As Threading.ThreadAbortException

        Catch ex As Exception

            returnCode = C_MESSAGENO.EXCEPTION
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()                             'ログ出力
        End Try

    End Sub
    ''' <summary>
    ''' 終了ボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New COA0011ReturnUrl

        '自画面MAPIDより親MAP・URLを取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '次画面の変数セット
        HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL

        '画面遷移実行
        Server.Transfer(COA0011ReturnUrl.URL)

    End Sub
    ''' <summary>
    ''' 入力データチェック
    ''' </summary>
    Protected Sub InputDataCheck()
        Dim COA0008InvalidChar As New COA0008InvalidChar

        'インターフェイス初期値設定
        returnCode = C_MESSAGENO.NORMAL

        'パスワード 禁則チェック
        COA0008InvalidChar.CHARin = txtNewPass.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin <> COA0008InvalidChar.CHARout Then
            CommonFunctions.ShowMessage(C_MESSAGENO.INPUTERROR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)

            txtNewPass.Focus()
            Return
        End If

        'パスワード 単項目チェック
        CheckSingle("NEWPASS", txtNewPass.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtNewPass.Focus()
            Return
        End If

        '再入力パスワード 禁則チェック
        COA0008InvalidChar.CHARin = txtReNewPass.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin <> COA0008InvalidChar.CHARout Then
            returnCode = C_MESSAGENO.INPUTERROR
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)

            txtNewPass.Focus()
            Return
        End If

        '再入力パスワード 単項目チェック
        CheckSingle("RENEWPASS", txtReNewPass.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtNewPass.Focus()
            Return
        End If

        '整合性チェック
        If txtNewPass.Text <> txtReNewPass.Text Then

            returnCode = C_MESSAGENO.REINPUTVALUE
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)

            txtNewPass.Focus()
            Return
        End If

        '申請済チェック
        ApplyCompCheck()
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtNewPass.Focus()
            Return
        End If

    End Sub
    ''' <summary>
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="inColName"></param>
    ''' <param name="inText"></param>
    Protected Sub CheckSingle(ByVal inColName As String, ByVal inText As String)

        Dim COA0026FieldCheck As New COA0026FieldCheck      '項目チェック
        '〇単項目チェック
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELD = inColName
        COA0026FieldCheck.VALUE = inText
        COA0026FieldCheck.COA0026FieldCheck()
        If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            returnCode = COA0026FieldCheck.ERR
        End If

    End Sub
    ''' <summary>
    ''' 申請済チェック
    ''' </summary>
    Protected Sub ApplyCompCheck()
        'DataBase接続文字
        Dim SQLStr As String = Nothing

        Try

            '検索SQL文
            SQLStr =
                    "SELECT * " _
                & " FROM  COS0020_USERAPPLY " _
                & " Where USERID        = @USERID " _
                & "   and STYMD        <= @STYMD " _
                & "   and ENDYMD       >= @ENDYMD " _
                & "   and DELFLG       <> @DELFLG "

            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
                  sqlCmd = New SqlCommand(SQLStr, sqlCon)
                sqlCon.Open()
                With sqlCmd.Parameters
                    .Add("@USERID", System.Data.SqlDbType.Char, 20).Value = COA0019Session.USERID
                    .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_YES
                End With
                Using SQLdr = sqlCmd.ExecuteReader()
                    If SQLdr.Read Then
                        returnCode = C_MESSAGENO.HASAPPLYINGRECORD
                        CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
                    Else
                        returnCode = C_MESSAGENO.NORMAL
                    End If
                End Using
            End Using

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try

    End Sub
    ''' <summary>
    ''' 表示言語設定
    ''' </summary>
    ''' <param name="lang">JA or EN</param>
    Private Sub LangSetting(ByVal lang As String)
        If lang <> C_LANG.JA Then
            lang = C_LANG.EN 'JA以外でEN以外が来た場合でも強制的にEN
        End If
        '****************************************
        ' オブジェクトの文言設定
        '****************************************
        '1階層(キー:オブジェクト、各言語での表示文言)
        '2階層(キー:言語での表示文言、表示文言)
        Dim dicDisplayText As New Dictionary(Of Control, Dictionary(Of String, String))

        'ラベル等やグリッドを除くの文言設定(適宜追加) リピーターの表ヘッダーもこの方式で可能ですので
        '作成者に聞いてください。
        AddLangSetting(dicDisplayText, Me.btnUpdate, "更新", "Update")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.lblNewPass, "新しいパスワード", "New Password")
        AddLangSetting(dicDisplayText, Me.lblReNewPass, "(再入力)新しいパスワード", "(Re-enter) New Password")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub
    ''' <summary>
    ''' ユーザーMAP情報取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetUserMapInfo() As DataRow
        Dim dt As New DataTable
        Dim retDr As DataRow = Nothing
        'S0004_USER検索SQL文
        Dim SQL_Str As String =
                 "SELECT rtrim(A.MAPID) as MAPID , rtrim(A.VARIANT) as VARIANT , " _
               & " rtrim(A.PROFID) as PROFID " _
               & " FROM  COS0005_USER A " _
               & " Where A.USERID      = @USERID " _
               & "   and A.STYMD      <= @STYMD " _
               & "   and A.ENDYMD     >= @ENDYMD " _
               & "   and A.DELFLG <> @DELFLG "
        'DataBase接続文字
        Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                SQLcmd As New SqlCommand(SQL_Str, SQLcon)
            SQLcon.Open() 'DataBase接続(Open)
            With SQLcmd.Parameters
                .Add("@USERID", System.Data.SqlDbType.Char, 20).Value = COA0019Session.USERID
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_YES
            End With

            Using sqlDa As New SqlDataAdapter(SQLcmd)
                sqlDa.Fill(dt)
            End Using
            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                retDr = dt.Rows(0)
            End If
        End Using 'SQLcon,SQLcmd
        Return retDr
    End Function
    ''' <summary>
    ''' メニュー画面へのURLを取得
    ''' </summary>
    ''' <param name="mapId">MAPID</param>
    ''' <returns></returns>
    ''' <remarks>本来ログイン直後に遷移するURLを取得</remarks>
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