Public Class GLM0001SELECT
    Inherits System.Web.UI.Page

    Const CONST_MAPID = "GLM00001S"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Dim CA0004LableMessage As New BASEDLL.CA0004LableMessage    'メッセージ取得
        Dim CA0005TermInfo As New BASEDLL.CA0005TermInfo            'サーバ情報取得
        Dim GLM00001SELECT As New GLDLL.GLM00001SELECT

        '■■■ 全画面共通チェック ■■■　　　■付きコメントは必須処理
        '○ セッション情報（ユーザ）未設定時の処理(ログオンへ画面遷移) 
        '  ※直接URL指定で起動した場合、ログオン画面へ遷移
        If Session("Userid") = "" Then
            Server.Transfer("/COM00001LOGON.aspx")
        End If

        ''■■■ オンラインサービス判定  ■■■
        ''○画面UserIDの会社からDB(T0001_ONLINESTAT)検索
        CA0005TermInfo.TERMid = HttpContext.Current.Session("APSRVname")
        CA0005TermInfo.CA0005GetTermInfo()
        If CA0005TermInfo.ERR = "00000" Then
            '■■■　オンラインサービス判定　■■■
            If CA0005TermInfo.ONLINESW = 0 Then
                'オンラインサービス停止時、ログオン画面へ遷移
                Server.Transfer("/COM00001LOGON.aspx")
                Exit Sub
            End If
        Else
            CA0004LableMessage.MESSAGENO = "20001"
            CA0004LableMessage.PARA01 = "CODE:" & CA0005TermInfo.ERR & ""
            CA0004LableMessage.NAEIW = "A"
            CA0004LableMessage.MESSAGEBOX = WF_MESSAGE
            CA0004LableMessage.CA0004getMessage()
            WF_MESSAGE = CA0004LableMessage.MESSAGEBOX
            Exit Sub
        End If

        '■■■ ヘルプ表示処理 ■■■
        If WF_HelpChange.Text = Nothing Or WF_HelpChange.Text = "" Then
        Else
            HttpContext.Current.Session("HELPid") = CONST_MAPID
            ClientScript.RegisterStartupScript(Me.GetType, "OpenNewWindow", "<script language=""javascript"">window.open('../COM00003HELP.aspx', '_blank', 'menubar=1, location=1, status=1, scrollbars=1, resizable=1');</script>")
            WF_HelpChange.Text = ""
            WF_SUBMIT.Text = "FALSE"
            Exit Sub
        End If

        HttpContext.Current.Session("MAPurl") = ""

        If IsPostBack Then
            GLM00001SELECT.Page_Load_IsPostBack(sender, e)
            If HttpContext.Current.Session("MAPurl") <> "" Then
                Server.Transfer(HttpContext.Current.Session("MAPurl"))
            End If
        Else
            GLM00001SELECT.Page_Load_NoPostBack(sender, e)
        End If

        'サーバー処理終了を設定
        WF_SUBMIT.Text = "FALSE"

    End Sub

End Class