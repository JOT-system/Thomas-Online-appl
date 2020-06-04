Imports System.Data.SqlClient
Public Class THOMAS_MailTest2
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click



        HttpContext.Current.Session("APSRVname") = "APSRVname"
        HttpContext.Current.Session("USERID") = "TEST USER"
        HttpContext.Current.Session("SYSCODE") = C_SYSCODE_GB
        Dim sendexe As String

        If Me.RadioButtonList2.SelectedValue = "test" Then
            HttpContext.Current.Session("DBcon") = "Data Source=DESKTOP-D5IC4N5\JOT;Initial Catalog=APPLDB;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;;UID='sa';Password='123456789'"
            sendexe = "C:\APPL_JOT\SYS\BATCH\COB00001SendMail\COB00001SendMail.exe"
        Else
            HttpContext.Current.Session("DBcon") = "Data Source=DESKTOP-D5IC4N5\JOT;Initial Catalog=APPLDB(demo);Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;;UID='sa';Password='123456789'"
            sendexe = "C:\APPL_JOT(demo)\SYS\BATCH\COB00001SendMail\COB00001SendMail.exe"
        End If


        Dim GBA00009MailSendSet As New GBA00009MailSendSet
        GBA00009MailSendSet.BRSUBID = ""
        GBA00009MailSendSet.BRBASEID = ""
        Label2.Text = ""

        Dim sqlStat As New System.Text.StringBuilder
        sqlStat.AppendLine("   SELECT ")
        sqlStat.AppendLine("     trim(SUBID) as SUBID, trim(LINKID) as LINKID FROM GBT0001_BR_INFO ")
        sqlStat.AppendFormat("   WHERE BRID = '{0}' ", TextBox3.Text)
        sqlStat.AppendLine("     AND TYPE = 'INFO' ")
        sqlStat.AppendLine("     AND DELFLG <> '1' ")

        Using sqlConn As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon"))) _
            , sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn)
            sqlConn.Open()

            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                While sqlDr.Read
                    GBA00009MailSendSet.BRSUBID = Convert.ToString(sqlDr("SUBID"))
                    GBA00009MailSendSet.BRBASEID = Convert.ToString(sqlDr("LINKID"))
                End While
            End Using

        End Using

        If GBA00009MailSendSet.BRSUBID = "" Then
            Label2.Text = "Breaker ID：" & TextBox3.Text & " 未存在"
        Else
            GBA00009MailSendSet.EVENTCODE = Me.RadioButtonList1.SelectedValue
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = TextBox3.Text
            GBA00009MailSendSet.GBA00009setMailToBR()

            Dim p As System.Diagnostics.Process =
                    System.Diagnostics.Process.Start(sendexe)
            p.WaitForExit()


        End If

    End Sub
End Class