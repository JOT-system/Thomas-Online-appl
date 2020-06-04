Imports System.Data
Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' メニュー画面クラス
''' </summary>
Public Class COM00002MENU
    Inherits GbPageCommon '共通ページはGB用の為一旦継承させず.net標準

    ' 定数定義
    Const CONST_MAPID = "COM00002"
    Const CONST_NAMESPACE = "COM00002MENU"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0005TermInfo As New BASEDLL.COA0005TermInfo            'サーバ情報取得
        Dim COA0007CompanyInfo As New BASEDLL.COA0007CompanyInfo      '会社情報取得


        '★★★ 全画面共通チェック ★★★
        '○ セッション情報（ユーザ）未設定時の処理(ログオンへ画面遷移)  ★必須処理
        '  ※直接URL指定で起動した場合、ログオン画面へ遷移
        If COA0019Session.USERID = "" Then
            Server.Transfer(C_LOGIN_URL)
            Return
        End If

        '★★★ オンラインサービス判定  ★★★
        '○画面UserIDの会社からDB(T0001_ONLINESTAT)検索
        COA0005TermInfo.TERMid = Convert.ToString(HttpContext.Current.Session("APSRVname"))
        COA0005TermInfo.COA0005GetTermInfo()
        If COA0005TermInfo.ERR = C_MESSAGENO.NORMAL Then
            '■■■　オンラインサービス判定　■■■
            If COA0005TermInfo.ONLINESW = 0 Then
                Server.Transfer(C_LOGIN_URL)
                Return
            End If
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0005TermInfo.ERR & ""})
            Return
        End If

        If IsPostBack Then
            '■■■ ヘルプ表示処理 ■■■
            If Me.hdnHelpChange.Value = Nothing Or Me.hdnHelpChange.Value = "" Then
            Else
                WF_HELPDisplay(sender, e)
                Me.hdnHelpChange.Value = ""
            End If
        Else

            '★★★ セッション変数設定 ★★★

            '○ 固定項目設定  ★必須処理
            Session("Namespace") = CONST_NAMESPACE
            Session("Class") = "Page_Load"

            '★★★ 初期画面表示 ★★★
            '○ ヘッダー表示
            'ID、表題設定
            Me.lblTitleId.Text = CONST_MAPID
            Try
                '検索SQL文
                Dim selColumn As String = "NAMES"
                If COA0019Session.LANGDISP <> C_LANG.JA Then
                    selColumn = selColumn & "_" & COA0019Session.LANGDISP
                End If

                Dim SQLStr As String =
                     "SELECT rtrim(isnull(B." & selColumn & ",A." & selColumn & ")) as NAMES " _
                   & " FROM  COS0009_PROFMAP A " _
                   & " LEFT OUTER JOIN COS0009_PROFMAP B " _
                   & "   ON  B.PROFID    = @PROFID " _
                   & "   and B.MAPIDP    = A.MAPIDP " _
                   & "   and B.VARIANTP  = B.VARIANT " _
                   & "   and B.TITLEKBN  = 'H' " _
                   & "   and B.STYMD    <= @STYMD " _
                   & "   and B.ENDYMD   >= @ENDYMD " _
                   & "   and B.DELFLG   <> @DELFLG " _
                   & " Where A.PROFID    = 'Default' " _
                   & "   and A.MAPIDP    = @MAPIDP " _
                   & "   and A.VARIANTP  = @VARIANTP " _
                   & "   and A.TITLEKBN  = 'H' " _
                   & "   and A.STYMD    <= @STYMD " _
                   & "   and A.ENDYMD   >= @ENDYMD " _
                   & "   and A.DELFLG   <> @DELFLG " _
                   & " ORDER BY A.SEQ "

                'DataBase接続文字
                Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                      SQLcmd As New SqlCommand(SQLStr, SQLcon)

                    SQLcon.Open() 'DataBase接続(Open)
                    With SQLcmd.Parameters
                        .Add("@PROFID", System.Data.SqlDbType.Char, 20).Value = COA0019Session.USERID
                        .Add("@MAPIDP", System.Data.SqlDbType.Char, 50).Value = CONST_MAPID
                        .Add("@VARIANTP", System.Data.SqlDbType.Char, 50).Value = HttpContext.Current.Session("MAPvariant").ToString
                        .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                        .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                        .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_YES
                    End With

                    Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                        While SQLdr.Read
                            Me.lblTitleText.Text = Convert.ToString(SQLdr("NAMES"))
                            Exit While
                        End While
                    End Using
                End Using

            Catch ex As Exception
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.SYSTEMADM80003)})

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE                       '
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL                            '
                COA0003LogFile.TEXT = ex.ToString()
                COA0003LogFile.MESSAGENO = C_MESSAGENO.SYSTEMADM80003
                COA0003LogFile.COA0003WriteLog()                             'ログ出力
                Return
            End Try

            COA0007CompanyInfo.COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
            COA0007CompanyInfo.STYMD = Date.Now
            COA0007CompanyInfo.ENDYMD = Date.Now
            COA0007CompanyInfo.COA0007getCompanyInfo()
            If COA0007CompanyInfo.ERR = C_MESSAGENO.NORMAL Then
                If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                    Me.lblTitleCompany.Text = COA0007CompanyInfo.NAMES_EN
                Else
                    Me.lblTitleCompany.Text = COA0007CompanyInfo.NAMES
                End If
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0005TermInfo.ERR & ""})
                Return
            End If

            '****************************************
            'ヘッダー日付設定
            '****************************************
            Dim timeFormat As String = " HH:mm:ss" 'これも国別にあるなら要設定
            Dim dateFormat As String = "yyyy/MM/dd" '空白も加味して初期の日付フォーマットを準備
            Dim GBA00003UserSetting As New GBA00003UserSetting With {
                    .USERID = COA0019Session.USERID
                }
            GBA00003UserSetting.GBA00003GetUserSetting()
            If GBA00003UserSetting.DATEFORMAT IsNot Nothing AndAlso
               GBA00003UserSetting.DATEFORMAT <> "" Then
                dateFormat = GBA00003UserSetting.DATEFORMAT
            End If
            Dim datetimeFormat As String = dateFormat & timeFormat
            Me.lblTitleDate.Text = ""
            Me.lblTitleDate.Text = Date.Now.ToString(datetimeFormat)

            '****************************************
            'ユーザー名設定
            '****************************************
            Me.lblTitleOffice.Text = COA0019Session.USERID

            '****************************************
            'メッセージクリア
            '****************************************
            Me.lblFooterMessage.Text = ""

            '************************************
            'メニュー文言の設定
            '************************************
            Try
                Dim selColumn1 As String = "TITLE"
                Dim selColumn2 As String = "NAMES"
                Dim selColumn3 As String = "NAMEL"
                If COA0019Session.LANGDISP <> C_LANG.JA Then
                    selColumn1 = selColumn1 & "_" & COA0019Session.LANGDISP
                    selColumn2 = selColumn2 & "_" & COA0019Session.LANGDISP
                    selColumn3 = selColumn3 & "_" & COA0019Session.LANGDISP
                End If

                '検索SQL文
                Dim SQLStr As String =
                     "SELECT rtrim(A.SEQ) as SEQ , rtrim(A.MAPID) as MAPID , rtrim(A.VARIANT) as VARIANT , " _
                   & " rtrim(A." & selColumn1 & ") as TITLE , rtrim(A." & selColumn2 & ") as NAMES , rtrim(A." & selColumn3 & ") as NAMEL , rtrim(B.URL) as URL " _
                   & " FROM  COS0009_PROFMAP A " _
                   & " LEFT JOIN COS0008_URL B " _
                   & "   ON  B.MAPID    = A.MAPID " _
                   & "   and B.STYMD   <= @STYMD " _
                   & "   and B.ENDYMD  >= @ENDYMD " _
                   & "   and B.DELFLG  <> @DELFLG " _
                   & " Where A.PROFID   = @PROFID " _
                   & "   and A.MAPIDP   = @MAPIDP " _
                   & "   and A.VARIANTP = @VARIANTP " _
                   & "   and A.TITLEKBN = 'I' " _
                   & "   and A.POSI     = @POSI " _
                   & "   and A.STYMD   <= @STYMD " _
                   & "   and A.ENDYMD  >= @ENDYMD " _
                   & "   and A.DELFLG  <> @DELFLG " _
                   & " ORDER BY A.SEQ "
                Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                      SQLcmd As New SqlCommand(SQLStr, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)
                    'ループ内で変動しないSQLパラメータ設定
                    With SQLcmd.Parameters
                        .Add("@MAPIDP", System.Data.SqlDbType.Char, 50).Value = CONST_MAPID
                        .Add("@VARIANTP", System.Data.SqlDbType.Char, 50).Value = HttpContext.Current.Session("MAPvariant").ToString
                        .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                        .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                        .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_YES

                    End With
                    'ループ内で変動するSQLパラメータオブジェクトを変数に格納
                    Dim paramProfId As SqlParameter = SQLcmd.Parameters.Add("@PROFID", System.Data.SqlDbType.Char, 20)
                    Dim paramPosi As SqlParameter = SQLcmd.Parameters.Add("@POSI", System.Data.SqlDbType.Char, 1)

                    'ポジション、PROFID分ループを回す
                    For Each posiInfo In {New With {.Posi = "L", .RepObj = Repeater_Menu_L},
                                          New With {.Posi = "R", .RepObj = Repeater_Menu_R}}
                        paramPosi.Value = posiInfo.Posi
                        'For Each profId As String In {COA0019Session.USERID, "Default"}
                        For Each profId As String In {COA0019Session.PROFID, "Default"}
                            paramProfId.Value = profId
                            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                                If SQLdr.HasRows = True Then
                                    posiInfo.RepObj.DataSource = SQLdr
                                    posiInfo.RepObj.DataBind()
                                    Exit For
                                Else
                                    Continue For
                                End If
                            End Using
                        Next profId
                    Next posiInfo
                End Using
            Catch ex As Exception
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.SYSTEMADM80003)})

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = ex.ToString()
                COA0003LogFile.MESSAGENO = C_MESSAGENO.SYSTEMADM80003
                COA0003LogFile.COA0003WriteLog()
                Return
            End Try

            '■■■ パスワード有効期限の警告表示 ■■■
            '○パスワード有効期限の警告表示
            Dim WW_ENDYMD As Date = Date.Now

            Try
                'S0014_USER検索SQL文
                Dim SQL_Str As String =
                     "SELECT PASSENDYMD " _
                   & " FROM  COS0006_USERPASS " _
                   & " Where USERID = @USERID " _
                   & "   and DELFLG <> @DELFLG "

                'DataBase接続文字
                Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                      USERcmd As New SqlCommand(SQL_Str, SQLcon)
                    SQLcon.Open() 'DataBase接続(Open)
                    With USERcmd.Parameters
                        .Add("@USERID", System.Data.SqlDbType.Char, 20).Value = COA0019Session.USERID
                        .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = BaseDllCommon.CONST_FLAG_YES
                    End With

                    Using SQLdr As SqlDataReader = USERcmd.ExecuteReader()
                        While SQLdr.Read
                            WW_ENDYMD = CDate(SQLdr("PASSENDYMD"))
                            Exit While
                        End While
                    End Using
                End Using
            Catch ex As Exception
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.SYSTEMADM80003)})

                COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
                COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
                COA0003LogFile.TEXT = ex.ToString()
                COA0003LogFile.MESSAGENO = C_MESSAGENO.SYSTEMADM80003
                COA0003LogFile.COA0003WriteLog()                             'ログ出力
                Return
            End Try

            If DateDiff("d", Date.Now, WW_ENDYMD) < 31 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.PASSEXPIRESOON, Me.lblFooterMessage)
            End If

        End If

    End Sub
    ' ******************************************************************************
    ' ***  Repeater_Menu_R バインド時 編集（左）（右）                           ***
    ' ******************************************************************************
    Protected Sub rptInfo_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles Repeater_Menu_L.ItemDataBound, Repeater_Menu_R.ItemDataBound

        '★★★ Repeater_Menu_Rバインド時 編集（右） ★★★
        '○ヘッダー編集
        If (e.Item.ItemType = ListItemType.Header) Then
        End If

        '○アイテム編集
        If ((e.Item.ItemType = ListItemType.Item) Or (e.Item.ItemType = ListItemType.AlternatingItem)) Then
            Dim whitchRep As String = If(DirectCast(sender, Repeater).ID = Me.Repeater_Menu_L.ID, "L", "R")

            Dim repLabel As Label = DirectCast(e.Item.FindControl("WF_MenuLabe_" & whitchRep), Label)
            Dim repVari As Label = DirectCast(e.Item.FindControl("WF_MenuVARI_" & whitchRep), Label)
            Dim repUrl As Label = DirectCast(e.Item.FindControl("WF_MenuURL_" & whitchRep), Label)
            Dim repMap As Label = DirectCast(e.Item.FindControl("WF_MenuMAP_" & whitchRep), Label)
            Dim repButton As Button = DirectCast(e.Item.FindControl("WF_MenuButton_" & whitchRep), Button)

            repLabel.Text = HttpUtility.HtmlEncode(Convert.ToString(DataBinder.Eval(e.Item.DataItem, "TITLE")))
            repVari.Text = Convert.ToString(DataBinder.Eval(e.Item.DataItem, "VARIANT"))
            If IsDBNull(DataBinder.Eval(e.Item.DataItem, "URL")) Then
                repUrl.Text = ""
            Else
                repUrl.Text = Convert.ToString(DataBinder.Eval(e.Item.DataItem, "URL"))
            End If
            repMap.Text = Convert.ToString(DataBinder.Eval(e.Item.DataItem, "MAPID"))
            repButton.Text = "  " & Convert.ToString(DataBinder.Eval(e.Item.DataItem, "NAMES"))

            If Convert.ToString(DataBinder.Eval(e.Item.DataItem, "TITLE")) = "" Then
                If Convert.ToString(DataBinder.Eval(e.Item.DataItem, "NAMES")) = "" Then
                    repLabel.Text = "　　"
                    repLabel.Visible = True
                    repVari.Visible = False
                    repButton.Visible = False
                    repUrl.Visible = False
                    repMap.Visible = False
                Else
                    repLabel.Visible = False
                    repVari.Visible = False
                    repButton.Visible = True
                    repUrl.Visible = False
                    repMap.Visible = False
                End If
            Else
                repLabel.Visible = True
                repVari.Visible = False
                repButton.Visible = False
                repUrl.Visible = False
                repMap.Visible = False
            End If
        End If

        '○フッター編集
        If e.Item.ItemType = ListItemType.Footer Then
        End If

    End Sub
    ' ******************************************************************************
    ' ***  Repeater_Menu (右)(左) ボタン押下処理                                 ***
    ' ******************************************************************************
    Protected Sub Repeater_Menu_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles Repeater_Menu_L.ItemCommand,
                                                                                                     Repeater_Menu_R.ItemCommand

        Dim targerRep As Repeater = DirectCast(source, Repeater)
        Dim whitchRep As String = If(targerRep.ID = Me.Repeater_Menu_L.ID, "L", "R")
        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0010AUTHORmap As New BASEDLL.COA0010AUTHORmap          'メッセージ取得

        '★★★ ボタン押下時、画面遷移（左） ★★★
        '○ボタン押下時、画面遷移情報取得
        Dim WW_COUNT As Integer = e.Item.ItemIndex
        Dim WW_URL As Label = DirectCast(targerRep.Items(WW_COUNT).FindControl("WF_MenuURL_" & whitchRep), Label)
        Dim WW_VARI As Label = DirectCast(targerRep.Items(WW_COUNT).FindControl("WF_MenuVARI_" & whitchRep), Label)
        Dim WW_MAPID As Label = DirectCast(targerRep.Items(WW_COUNT).FindControl("WF_MenuMAP_" & whitchRep), Label)

        '○画面遷移権限チェック（左）
        COA0010AUTHORmap.MAPID = WW_MAPID.Text
        COA0010AUTHORmap.COA0010GetAUTHORmap()
        If COA0010AUTHORmap.ERR = C_MESSAGENO.NORMAL Then
            If COA0010AUTHORmap.MAPPERMITCODE = "1" OrElse COA0010AUTHORmap.MAPPERMITCODE = "2" Then
                HttpContext.Current.Session("MAPpermitcode") = COA0010AUTHORmap.MAPPERMITCODE
                HttpContext.Current.Session("MAPvariant") = WW_VARI.Text
                HttpContext.Current.Session("MAPetc") = ""
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.ACCESSDENIED, Me.lblFooterMessage)
                Return
            End If
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0010AUTHORmap.ERR & ""})
            Return
        End If

        'ボタン押下時、画面遷移
        Server.Transfer(WW_URL.Text)

    End Sub
    ' ******************************************************************************
    ' ***  ヘルプボタン処理                                                      ***
    ' ******************************************************************************
    Protected Sub WF_HELPDisplay(sender As Object, e As EventArgs)
        '■■■ セッション変数設定 ■■■
        '○ 固定項目設定  ■必須処理
        Session("Class") = "WF_HELPDisplay"

        '■■■ 画面遷移実行 ■■■
        HttpContext.Current.Session("HELPid") = CONST_MAPID
        'ClientScript.RegisterStartupScript(Me.GetType, "OpenNewWindow", "<script language=""javascript"">window.open('COM00003HELP.aspx', '_blank', 'menubar=1, location=1, status=1, scrollbars=1, resizable=1');</script>")
        Me.hdnCanHelpOpen.Value = "1"
    End Sub


End Class