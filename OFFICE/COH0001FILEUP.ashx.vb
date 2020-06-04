Imports System.IO.Path
Imports System.Web
Imports System.Web.Services
Imports BASEDLL

Public Class COH0001FILEUP
    Implements System.Web.IHttpHandler, System.Web.SessionState.IRequiresSessionState

    Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        '共通宣言
        '*共通関数宣言(BASEDLL)
        Dim COA0005TermInfo As New BASEDLL.COA0005TermInfo            'サーバ情報取得

        Dim session As System.Web.SessionState.HttpSessionState = HttpContext.Current.Session

        '  ※直接URL指定で起動した場合、異常終了
        If COA0019Session.USERID Is Nothing OrElse COA0019Session.USERID = "" Then
            context.Response.StatusCode = 300                           'エラーリターン(textStatus:errorとなる)
            Return
        End If

        '★★★ オンラインサービス判定  ★★★
        '○画面UserIDの会社からDB(T0001_ONLINESTAT)検索
        COA0005TermInfo.TERMid = Convert.ToString(HttpContext.Current.Session("APSRVname"))
        COA0005TermInfo.COA0005GetTermInfo()
        If COA0005TermInfo.ERR = C_MESSAGENO.NORMAL Then
            '■■■　オンラインサービス判定　■■■
            If COA0005TermInfo.ONLINESW = 0 Then
                context.Response.StatusCode = 300                       'エラーリターン(textStatus:errorとなる)
                Return
            End If
        Else
            context.Response.StatusCode = 300                           'エラーリターン(textStatus:errorとなる)
            Return
        End If


        '■アップロードFILE格納ディレクトリ取得
        Try
            '　アップロードFILE格納フォルダ作成
            Dim WW_Dir As String = ""
            WW_Dir = COA0019Session.UPLOADDir
            '　格納フォルダ存在確認＆作成(C:\apple\files\UPLOAD_TMP)
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '　アップロードFILE格納フォルダ存在確認＆作成(C:\apple\files\UPLOAD_TMP\ユーザID)
            WW_Dir = COA0019Session.UPLOADDir & "\" & COA0019Session.USERID
            If System.IO.Directory.Exists(WW_Dir) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir)
            End If

            '　アップロードFILE格納フォルダ内不要ファイル削除(すべて削除)
            WW_Dir = COA0019Session.UPLOADDir & "\" & COA0019Session.USERID
            For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*.*")
                ' ファイルパスからファイル名を取得
                System.IO.File.Delete(tempFile)
            Next
        Catch ex As Exception
            context.Response.StatusCode = 300                           'エラーリターン(textStatus:errorとなる)
            Return
        End Try

        ''■アップロードFILEチェック
        ''アップロードは１ファイルのみ
        'If context.Request.Files.Count <> 1 Then
        '    context.Response.StatusCode = 300                           'エラーリターン(textStatus:errorとなる)
        '    Exit Sub
        'End If

        For i As Integer = 0 To context.Request.Files.Count - 1
            Dim WW_FILEname As String = GetExtension(context.Request.Files(0).FileName)
            If Mid(WW_FILEname.ToLower, 1, 3) = "xls" Then
                '■EXCELプロセスのお掃除
                Dim ps As System.Diagnostics.Process() = System.Diagnostics.Process.GetProcesses()
                For Each p As System.Diagnostics.Process In ps
                    Try '拒否エラーのためのtry
                        If Mid(p.ProcessName, 1, 5) = "EXCEL" Then
                            Dim WW_START As Long = CInt((DateTime.Parse(Convert.ToString(p.StartTime))).ToString("HHmmss"))
                            Dim WW_NOW As Long = CInt(DateTime.Now.ToString("HHmmss"))
                            'If (WW_NOW - WW_START) > 10 Then   '10秒
                            '    p.Kill()
                            'End If

                            'p.Kill()
                        End If
                    Catch ex As Exception
                    End Try
                Next
                Exit For
            End If
        Next

        '■アップロードFILE格納
        For i As Integer = 0 To context.Request.Files.Count - 1
            Try
                Dim WW_FILEname = System.IO.Path.GetFileName(context.Request.Files(i).FileName)

                Dim WW_PostedFile As HttpPostedFile = context.Request.Files(i)
                WW_PostedFile.SaveAs(COA0019Session.UPLOADDir & "\" & COA0019Session.USERID & "\" & WW_FILEname)
            Catch ex As Exception
                context.Response.StatusCode = 300     'エラーリターン(textStatus:errorとなる)
            End Try
        Next

    End Sub

    ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class