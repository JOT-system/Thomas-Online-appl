Imports BASEDLL

Public Class COM00003HELP
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not IsPostBack Then
            HELP_Display()
        Else
            '■■■ Detail PFD内容表示処理 ■■■
            If WF_FileDisplay.Value = Nothing Or WF_FileDisplay.Value = "" Then
            Else
                FileDisplay()
                WF_FileDisplay.Value = ""
            End If
        End If

    End Sub

    ' ******************************************************************************
    ' ***  ヘルプ一覧表示                                                        ***
    ' ******************************************************************************
    Protected Sub HELP_Display()
        '■■■ セッション変数設定 ■■■
        '○ 固定項目設定  ★必須処理
        Session("Class") = "HELP_Display"

        '■■■ 初期設定 ■■■
        Dim WW_Dir As String = ""

        Dim COM00003tbl As New System.Data.DataTable
        Dim COM00003row As DataRow

        'COM00003tblテンポラリDB準備
        COM00003tbl.Clear()
        COM00003tbl.Columns.Clear()
        COM00003tbl.Clear()
        COM00003tbl.Columns.Add("WF_Rep_FILENAME", GetType(String))
        COM00003tbl.Columns.Add("WF_Rep_FILEPATH", GetType(String))

        '■■■ 画面編集 ■■■
        '○PDF格納ディレクトリ編集    
        WW_Dir = ""
        WW_Dir = WW_Dir & COA0019Session.HELPDir & "\" & Convert.ToString(HttpContext.Current.Session("HELPid"))

        '○指定HELPフォルダ内ファイル取得
        Dim WW_Files_dir As New List(Of String)     'Dir + FileName
        Dim WW_Files_name As New List(Of String)    'FileName
        Dim WW_HELPfiles As String()

        If System.IO.Directory.Exists(WW_Dir) Then
            WW_HELPfiles = System.IO.Directory.GetFiles(WW_Dir, "*", System.IO.SearchOption.AllDirectories)
            For Each tempFile As String In WW_HELPfiles
                Dim WW_tempFile As String = System.IO.Path.GetFileName(tempFile)
                If WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                    COM00003row = COM00003tbl.NewRow

                    'ファイル名格納
                    COM00003row("WF_Rep_FILENAME") = WW_tempFile
                    'ファイルパス格納
                    COM00003row("WF_Rep_FILEPATH") = tempFile
                    COM00003tbl.Rows.Add(COM00003row)
                End If
                'Do
                '    If InStr(WW_tempFile, "\") > 0 Then
                '        'ファイル名編集
                '        WW_tempFile = Mid(WW_tempFile, InStr(WW_tempFile, "\") + 1, 100)
                '    End If

                '    If InStr(WW_tempFile, "\") = 0 And WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                '        COM00003row = COM00003tbl.NewRow

                '        'ファイル名格納
                '        COM00003row("WF_Rep_FILENAME") = WW_tempFile
                '        'ファイルパス格納
                '        COM00003row("WF_Rep_FILEPATH") = tempFile
                '        COM00003tbl.Rows.Add(COM00003row)
                '        Exit Do
                '    End If

                'Loop Until InStr(WW_tempFile, "\") = 0
            Next
        End If

        'Repeaterバインド
        WF_DViewRepPDF.DataSource = COM00003tbl
        WF_DViewRepPDF.DataBind()

        '■■■ データ設定 ■■■
        'Repeaterへデータをセット
        For i As Integer = 0 To COM00003tbl.Rows.Count - 1

            'ファイル記号名称
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text = Convert.ToString(COM00003tbl.Rows(i)("WF_Rep_FILENAME"))
            'FILEPATH
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), System.Web.UI.WebControls.Label).Text = Convert.ToString(COM00003tbl.Rows(i)("WF_Rep_FILEPATH"))

        Next

        '■■■ イベント設定 ■■■
        Dim WW_ATTR As String = ""
        For i As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "FileDisplay('" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text & "')"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Attributes.Add("ondblclick", WW_ATTR)
        Next
    End Sub

    ' ***  DetailPDF内容表示（Detail・PDFダブルクリック時（内容照会））
    Protected Sub FileDisplay()

        Dim WW_TERM As String = ""
        Dim WW_TERMIP As String = ""

        WW_TERM = Convert.ToString(HttpContext.Current.Session("APSRVname"))
        WW_TERMIP = Convert.ToString(HttpContext.Current.Session("APSRVIp"))

        'Dim WW_Dir As String = COA0019Session.PRINTWORKDir & WW_TERM
        Dim WW_Dir As String = COA0019Session.PRINTWORKDir & "/" & COA0019Session.USERID

        '■■■ セッション変数設定 ■■■
        '○ 固定項目設定  ★必須処理
        Session("Class") = "FileDisplay"

        For i As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
            'ダブルクリック時コード検索イベント追加
            If DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text = WF_FileDisplay.Value Then

                'ディレクトリが存在しない場合、作成する
                If System.IO.Directory.Exists(WW_Dir) = False Then
                    System.IO.Directory.CreateDirectory(WW_Dir)
                End If

                'ダウンロードファイル送信準備
                System.IO.File.Copy(DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), System.Web.UI.WebControls.Label).Text,
                                    WW_Dir & "\" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text,
                                    True)

                'ダウンロード処理へ遷移
                'WF_HELPURL.Value = "http://" & WW_TERMIP & "/print/" & WW_TERM & "/" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text
                WF_HELPURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & COA0019Session.PRINTROOTUrl & "/" & COA0019Session.USERID & "/" &
                                   Uri.EscapeUriString(DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text)
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_DownLoad()", True)

                Exit For
            End If
        Next

    End Sub

End Class