Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' REPAIRBREAKER一覧画面クラス
''' </summary>
Public Class GBT00011RESULT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00011" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44             '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8           'マウススクロール時の増分

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
            COA0003LogFile = New COA0003LogFile              'ログ出力

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))

                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = "RepairSearch"
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                                messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})
                    Return
                End If

                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetListDataTable()
                    'チェックボックス設定
                    If Me.hdnXMLsaveFileRet.Value <> "" Then

                        Dim chkdt As DataTable = CreateDataTable()
                        Dim COA0021ListTable As New COA0021ListTable

                        COA0021ListTable.FILEdir = Me.hdnXMLsaveFileRet.Value
                        COA0021ListTable.TBLDATA = chkdt
                        COA0021ListTable.COA0021recoverListTable()
                        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                            chkdt = COA0021ListTable.OUTTBL
                        Else
                            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
                            Return
                        End If

                        For i As Integer = 0 To chkdt.Rows.Count - 1
                            For j As Integer = 0 To dt.Rows.Count - 1

                                If Convert.ToString(chkdt.Rows(i)("BRID")) = Convert.ToString(dt.Rows(j)("BRID")) Then

                                    dt.Rows(j)("CHECK") = chkdt.Rows(i)("CHECK")
                                End If
                            Next
                        Next

                    End If

                    'グリッド用データをファイルに退避
                    With Nothing
                        Dim COA0021ListTable As New COA0021ListTable
                        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
                        COA0021ListTable.TBLDATA = dt
                        COA0021ListTable.COA0021saveListTable()
                        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                            Return
                        End If
                    End With

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = "Default"
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
                        .LEVENT = "ondblclick"
                        .LFUNC = "ListDbClick"
                        .TITLEOPT = True
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .NOCOLUMNWIDTHOPT = 50
                        .USERSORTOPT = 1
                    End With
                    COA0013TableObject.COA0013SetTableObject()

                    Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
                    Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
                    Dim checkedValue As Boolean
                    For Each dr As DataRow In listData.Rows
                        If Convert.ToString(dr.Item("CHECK")) = "on" Then
                            checkedValue = True
                        Else
                            checkedValue = False
                        End If
                        Dim chkId As String = "chkWF_LISTAREACHECK" & Convert.ToString(dr.Item("LINECNT"))
                        Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
                        If chk IsNot Nothing Then
                            chk.Checked = checkedValue
                        End If

                        If {C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE, C_APP_STATUS.APPLYING}.Contains(Trim(Convert.ToString(dr.Item("STATUS")))) Then
                            chk.Enabled = False
                        Else
                            chk.Enabled = True
                        End If

                        If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
                            chk.Enabled = False
                            Me.WF_LISTAREA.Attributes.Add("data-hidedelete", "1")
                        End If
                    Next
                    '画面表示しているLineCntを保持
                    If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
                        Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                                  Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
                        ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
                    Else
                        ViewState("DISPLAY_LINECNT_LIST") = Nothing
                    End If
                End Using 'DataTable

                'メッセージ設定
                If hdnMsgId.Value <> "" Then
                    CommonFunctions.ShowMessage(hdnMsgId.Value, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                End If
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                '画面の入力情報を保持
                Dim messageNo As String = FileSaveDisplayInput()
                If messageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                                messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})
                    Return
                End If
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
                '**********************
                ' 一覧表の行ダブルクリック判定
                '**********************
                If Me.hdnListDBclick.Value <> "" Then
                    ListRowDbClick()
                    Me.hdnListDBclick.Value = ""
                    Return '単票ページにリダイレクトするため念のため処理は終わらせる
                End If
                '**********************
                ' Help表示
                '**********************
                If Me.hdnHelpChange.Value IsNot Nothing AndAlso Me.hdnHelpChange.Value <> "" Then
                    DivShowHelp_DoubleClick(CONST_MAPID)
                    Me.hdnHelpChange.Value = ""
                End If
                '**********************
                ' スクロール処理 
                '**********************
                ListScrole()
                hdnMouseWheel.Value = ""

            End If
            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        Catch ex As Threading.ThreadAbortException
            'キャンセルやServerTransferにて後続の処理が打ち切られた場合のエラーは発生させない
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.SYSTEMADM 'ここは適宜変えてください
            Dim NORMAL As String = ""
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = messageNo
            COA0003LogFile.COA0003WriteLog()

            Return

        End Try
    End Sub
    ''' <summary>
    ''' 左ビュー表示処理
    ''' </summary>
    Private Sub DisplayLeftView()
        Dim targetObject As Control = Nothing
        'ビューの存在チェック
        Dim changeViewObj As View = DirectCast(Me.mvLeft.FindControl(Me.hdnLeftboxActiveViewId.Value), View)
        If changeViewObj IsNot Nothing Then
            Me.mvLeft.SetActiveView(changeViewObj)
            Select Case changeViewObj.ID
                '他のビューが存在する場合はViewIdでCaseを追加

                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Me.hdnCalendarValue.Value = txtobj.Text

                        Me.mvLeft.Focus()
                    End If
                'タンク番号ビュー表示切替
                Case Me.vLeftTank.ID
                    SetTankListItem(Me.txtTankNo.Text)
                    '承認ビュー表示切替
                Case Me.vLeftApproval.ID
                    SetApprovalListItem(Me.txtApproval.Text)
            End Select
        End If

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = "GBT00011R"
        COA0011ReturnUrl.VARI = "GB_Default"
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
            'WF_TITLETEXT.Text = COA0011ReturnUrl.NAMES
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '次画面の変数セット
        HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL
        '画面遷移実行()
        Server.Transfer(COA0011ReturnUrl.URL)
    End Sub
    ''' <summary>
    ''' リペア新規作成ボタン押下時
    ''' </summary>
    Public Sub btnCreateRepair_Click()
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl

        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New COA0021ListTable

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If
        Me.hdnXMLsaveFileRet.Value = hdnXMLsaveFile.Value

        '■■■ 画面遷移先URL取得 ■■■
        COA0012DoUrl.MAPIDP = CONST_MAPID & "R"
        COA0012DoUrl.VARIP = "GB_RepairNew"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Me.hdnSelectedBrId.Value = ""
        Me.hdnSelectedStatus.Value = ""

        Session("MAPmapid") = CONST_MAPID & "R"
        Session("MAPvariant") = "GB_RepairNew"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 申請ボタン押下時
    ''' </summary>
    Public Sub btnApply_Click()
        Dim COA0021ListTable As New COA0021ListTable
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        Dim dt As DataTable = CreateDataTable()

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        Dim procDateTime As DateTime = DateTime.Now
        Dim applyId As String = Nothing
        Dim errIdList(2) As String
        Dim lastStep As String = Nothing
        'CHECKチェックボックスがチェック済の全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("CHECK")) = "on")
        Dim applyDt As DataTable = Nothing
        If q.Any = True Then
            applyDt = q.CopyToDataTable
        Else
            applyDt = dt.Clone
        End If
        For Each applyDr As DataRow In applyDt.Rows 'For i As Integer = 0 To dt.Rows.Count - 1

            '申請ID取得
            Dim GBA00002MasterApplyID As New GBA00002MasterApplyID
            GBA00002MasterApplyID.COMPCODE = COA0019Session.APSRVCamp
            GBA00002MasterApplyID.SYSCODE = C_SYSCODE_GB
            GBA00002MasterApplyID.KEYCODE = COA0019Session.APSRVname
            GBA00002MasterApplyID.MAPID = "GBT00012"
            GBA00002MasterApplyID.EVENTCODE = C_BRREVENT.APPLY
            GBA00002MasterApplyID.SUBCODE = ""
            GBA00002MasterApplyID.COA0032getgApplyID()
            If GBA00002MasterApplyID.ERR = C_MESSAGENO.NORMAL Then
                applyId = GBA00002MasterApplyID.APPLYID
            Else
                'CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                '                            messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00002MasterApplyID.ERR)})
                'Return
                If errIdList(0) = "" Then
                    errIdList(0) = GBA00002MasterApplyID.ERR
                End If
                Continue For
            End If

            Dim subCode As String = Convert.ToString(applyDr.Item("AGENTORGANIZER"))

            '申請登録
            COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
            COA0032Apploval.I_APPLYID = applyId
            COA0032Apploval.I_MAPID = "GBT00012"
            COA0032Apploval.I_EVENTCODE = C_BRREVENT.APPLY
            COA0032Apploval.I_SUBCODE = subCode
            COA0032Apploval.COA0032setApply()
            If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
                lastStep = COA0032Apploval.O_LASTSTEP
            Else
                'CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
                'Return
                If errIdList(1) = "" Then
                    errIdList(1) = COA0032Apploval.O_ERR
                End If
                Continue For
            End If

            Dim userEmail As String = ""

            'ブレーカー更新
            Dim sqlStat As New StringBuilder
            sqlStat.Clear()
            sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
            sqlStat.AppendLine("   SET APPLYID   = @APPLYID")
            sqlStat.AppendLine("      ,LASTSTEP  = @LASTSTEP")
            sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE BRID      = @BRID")
            sqlStat.AppendLine("   AND TYPE      = @TYPE")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
                  sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン
                With sqlCmd.Parameters
                    'パラメータ設定
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = applyDr.Item("BRID")
                    .Add("@TYPE", SqlDbType.NVarChar, 20).Value = "INFO"
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = applyId
                    .Add("@LASTSTEP", SqlDbType.NVarChar, 20).Value = lastStep
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.ExecuteNonQuery()
            End Using

            'メール
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = C_BRREVENT.APPLY
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = Convert.ToString(applyDr.Item("BRID"))
            GBA00009MailSendSet.BRSUBID = Convert.ToString(applyDr.Item("SUBID"))
            GBA00009MailSendSet.BRBASEID = Convert.ToString(applyDr.Item("BRBASEID"))
            GBA00009MailSendSet.BRROUND = ""
            GBA00009MailSendSet.APPLYID = applyId
            GBA00009MailSendSet.LASTSTEP = lastStep
            GBA00009MailSendSet.GBA00009setMailToRepBR()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
                'Return
                If errIdList(2) = "" Then
                    errIdList(2) = GBA00009MailSendSet.ERR
                End If
                Continue For
            End If

        Next

        If errIdList(0) <> "" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", errIdList(0))})
            Return
        ElseIf errIdList(1) <> "" Then
            CommonFunctions.ShowMessage(errIdList(1), Me.lblFooterMessage, pageObject:=Me)
            Return
        ElseIf errIdList(2) <> "" Then
            CommonFunctions.ShowMessage(errIdList(2), Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.APPLYSUCCESS

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub
    '''' <summary>
    '''' 承認取り消しボタン押下時
    '''' </summary>
    'Public Sub btnApplyCancel_Click()

    'End Sub
    ''' <summary>
    ''' Excelダウンロードボタン押下時
    ''' </summary>
    Public Sub btnExcelDownload_Click()

        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        '一覧表示データ復元 
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If
        '帳票出力
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            Dim reportId As String = "FullColumnList" 'Me.hdnReportVariant.Value
            Dim reportMapId As String = CONST_MAPID
            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If

            '別画面でExcelを表示
            hdnPrintURL.Value = COA0027ReportTable.URL
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

        End With
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
                Case Me.vLeftCal.ID 'アクティブなビューがカレンダー
                    'カレンダー選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        txtobj.Text = Me.hdnCalendarValue.Value
                        txtobj.Focus()
                    End If
                Case Me.vLeftTank.ID 'アクティブなビューがタンク番号コード
                    'タンク番号選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbTank.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbTank.SelectedItem.Value
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftApproval.ID 'アクティブなビューが承認
                    '承認選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbApproval.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbApproval.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
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
    ''' 先頭頁ボタン押下時
    ''' </summary>
    Public Sub btnFIRST_Click()

        'ポジション設定
        hdnListPosition.Value = "1"

    End Sub
    ''' <summary>
    ''' 最終頁ボタン押下時
    ''' </summary>
    Public Sub btnLAST_Click()
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable

        '一覧表示データ復元 
        Dim dt As DataTable = CreateDataTable()

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        'ソート
        Dim dvTBLview As DataView
        dvTBLview = New DataView(dt)
        dvTBLview.RowFilter = "HIDDEN= '0'"

        'ポジションを設定するのみ
        If dvTBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            hdnListPosition.Value = Convert.ToString(dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT))
        Else
            hdnListPosition.Value = Convert.ToString(dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT) + 1)
        End If

        dvTBLview.Dispose()
        dvTBLview = Nothing

    End Sub
    ''' <summary>
    ''' 一覧削除ボタン押下時
    ''' </summary>
    Public Sub btnListDelete_Click()

        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New COA0021ListTable
        Dim sqlStat As New Text.StringBuilder
        Dim tran As SqlTransaction = Nothing
        Dim procDateTime As DateTime = DateTime.Now

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        Dim dr As DataRow = dt.Rows((CInt(hdnListCurrentRownum.Value) - 1))
        Dim brId As String = Convert.ToString(dr.Item("BRID"))



        'ステータスチェック
        If {C_APP_STATUS.APPLYING, C_APP_STATUS.REVISE, C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE}.Contains(Trim(Convert.ToString(dr.Item("STATUS")))) Then
            Dim MsgBrId As String = ""
            If COA0019Session.LANGDISP = C_LANG.JA Then
                MsgBrId = "ブレーカーID"
            Else
                MsgBrId = "Breaker ID"
            End If
            CommonFunctions.ShowMessage(C_MESSAGENO.UNSELECTABLEERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {MsgBrId & "：" & brId & ""})
            Return
        End If

        Try
            '削除処理
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                tran = sqlCon.BeginTransaction() 'トランザクション開始

                '******************************
                ' 紐づけ情報削除
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
                sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE BRID      = @BRID")
                sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    Dim paramBrId As SqlParameter = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar, 20)
                    Dim paramDelflg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                    Dim paramUpdymd As SqlParameter = sqlCmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)
                    Dim paramUpduser As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)
                    Dim paramReceiveymd As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)
                    'パラメータ設定
                    paramBrId.Value = brId
                    paramDelflg.Value = CONST_FLAG_YES
                    paramUpdymd.Value = procDateTime
                    paramUpduser.Value = COA0019Session.USERID
                    paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD
                    sqlCmd.ExecuteNonQuery()
                End Using

                '******************************
                ' organizer情報（ブレーカー基本）削除
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBT0002_BR_BASE")
                sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE BRID      = @BRID")
                sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    Dim paramBrId As SqlParameter = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar, 20)
                    Dim paramDelflg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                    Dim paramUpdymd As SqlParameter = sqlCmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)
                    Dim paramUpduser As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)
                    Dim paramReceiveymd As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)
                    'パラメータ設定
                    paramBrId.Value = brId
                    paramDelflg.Value = CONST_FLAG_YES
                    paramUpdymd.Value = procDateTime
                    paramUpduser.Value = COA0019Session.USERID
                    paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD
                    sqlCmd.ExecuteNonQuery()
                End Using

                '******************************
                ' 費用情報削除
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBT0003_BR_VALUE")
                sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE BRID      = @BRID")
                sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    Dim paramBrId As SqlParameter = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar, 20)
                    Dim paramDelflg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                    Dim paramUpdymd As SqlParameter = sqlCmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)
                    Dim paramUpduser As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)
                    Dim paramReceiveymd As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)
                    'パラメータ設定
                    paramBrId.Value = brId
                    paramDelflg.Value = CONST_FLAG_YES
                    paramUpdymd.Value = procDateTime
                    paramUpduser.Value = COA0019Session.USERID
                    paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD
                    sqlCmd.ExecuteNonQuery()
                End Using

                tran.Commit()

            End Using

            'メッセージ出力
            hdnMsgId.Value = C_MESSAGENO.NORMALENTRY

            Dim thisPageUrl As String = Request.Url.ToString
            Server.Transfer(Request.Url.LocalPath)

        Catch ex As Exception
            Throw
        Finally
            If tran IsNot Nothing Then
                tran.Dispose()
            End If
        End Try

    End Sub
    ''' <summary>
    ''' ブレーカー番号取得
    ''' </summary>
    ''' <param name="sqlCon">オプション 項目</param>
    ''' <returns>新規ブレーカー番号</returns>
    Private Function GetNewBreakerNo(Optional sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim brNo As String = ""
        Dim sqlStat As New Text.StringBuilder
        '生きているブレーカーは基本情報＋発地着地(最大4)の5レコード想定
        sqlStat.AppendLine("Select  'BT' ")
        sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
        sqlStat.AppendLine("      + '_'")
        sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR " & C_SQLSEQ.BREAKER & ")),4)")
        sqlStat.AppendLine("      + '_'")
        sqlStat.AppendLine("      + (SELECT VALUE1")
        sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
        sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
        sqlStat.AppendLine("            AND KEYCODE = @KEYCODE)")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                Dim paramClass As SqlParameter = sqlCmd.Parameters.Add("@CLASS", SqlDbType.NVarChar, 20)
                Dim paramKeyCode As SqlParameter = sqlCmd.Parameters.Add("@KEYCODE", SqlDbType.NVarChar, 20)
                'SQLパラメータ値セット
                paramClass.Value = C_SERVERSEQ
                paramKeyCode.Value = COA0019Session.APSRVname
                'paramKeyCode.Value = "DESKTOP-D5IC4N5" '本当は動作させるホスト名
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get new BreakerNo error")
                    End If

                    brNo = Convert.ToString(dt.Rows(0).Item(0))
                End Using

            End Using
            Return brNo
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try
    End Function
    ''' <summary>
    ''' 画面グリッドのデータを取得しファイルに保存する。
    ''' </summary>
    Private Function FileSaveDisplayInput() As String
        'そもそも画面表示データがない状態の場合はそのまま終了
        If ViewState("DISPLAY_LINECNT_LIST") Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If
        Dim displayLineCnt = DirectCast(ViewState("DISPLAY_LINECNT_LIST"), List(Of Integer))
        Dim COA0021ListTable As New COA0021ListTable
        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        dt = CreateDataTable()
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            Return COA0021ListTable.ERR
        End If
        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        'Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String)
        '入力値保持用のフィールド名設定
        fieldIdList.Add("CHECK", objChkPrifix)

        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    '                    formToPost.Remove(dispObjId)
                End If
                Dim dr As DataRow = dt.Rows(i - 1)
                dr.Item(fieldId.Key) = displayValue
            Next
        Next

        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()

        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0021ListTable.ERR
        End If
        Return C_MESSAGENO.NORMAL
    End Function
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
        AddLangSetting(dicDisplayText, Me.btnExtract, "絞り込み", "Search")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnCreateRepair, "リペア新規作成", "Repair New")
        AddLangSetting(dicDisplayText, Me.btnApply, "申請", "Apply")
        'AddLangSetting(dicDisplayText, Me.btnApplyCancel, "申請取消", "APPLY CANCEL")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblTankNoLabel, "タンク", "TANK")
        AddLangSetting(dicDisplayText, Me.lblApprovalLabel, "承認", "STATUS")

        AddLangSetting(dicDisplayText, Me.hdnConfirmTitle, "削除しますよろしいですか？", "Are you sure you want to delete?")
        AddLangSetting(dicDisplayText, Me.lblConfirmBrNoName, "BR NO", "BR NO")

        SetDisplayLangObjects(dicDisplayText, lang)

        '****************************************
        ' グリッドヘッダーの表示文言設定(GrivViewだけは個別制御が必要)
        '****************************************
        '適宜追加を
    End Sub
    ''' <summary>
    ''' 一覧表のデータテーブルを取得する関数
    ''' </summary>
    ''' <returns></returns>
    Private Function GetListDataTable() As DataTable
        Dim mapId As String = "GBT00011"
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        'ソート順取得
        COA0020ProfViewSort.MAPID = mapId
        COA0020ProfViewSort.VARI = "Default"
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder
        Dim retDt As New DataTable
        '一旦生きているブレーカはすべて取得
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(BS.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,BS.BRID AS BRID")
        sqlStat.AppendLine("      ,BS.BRBASEID AS BRBASEID")
        sqlStat.AppendLine("      ,BIIF.SUBID AS SUBID")
        sqlStat.AppendLine("      ,FORMAT(BS.STYMD,'yyyy/MM/dd') AS STYMD")
        'sqlStat.AppendLine("      ,FORMAT(BS.ENDYMD,'yyyy/MM/dd') AS ENDYMD")
        sqlStat.AppendLine("      ,BS.TANKNO AS TANKNO")
        sqlStat.AppendLine("      ,ISNULL(BS.DEPOTCODE,'') AS DEPOTCODE")
        sqlStat.AppendLine("      ,ISNULL(DP.NAMES,'') AS DEPOTNAME")
        sqlStat.AppendLine("      ,ISNULL(DP.LOCATION,'') AS LOCATION")
        sqlStat.AppendLine("      ,CASE WHEN BS.REMARK<>'' THEN '〇' ELSE '' END AS HASREMARK")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FVIF.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FVIF.VALUE2,'') END AS APPLYORDENY")
        sqlStat.AppendLine("      ,'' AS ""CHECK""")
        sqlStat.AppendLine("      ,BIIF.APPLYID AS APPLYID")
        sqlStat.AppendLine("      ,BIIF.LASTSTEP AS STEP")
        sqlStat.AppendLine("      ,ISNULL(AHIF.STATUS,'') AS STATUS")
        sqlStat.AppendLine("      ,BS.AGENTORGANIZER")
        sqlStat.AppendLine("  FROM GBT0002_BR_BASE BS ")
        sqlStat.AppendLine("  INNER JOIN GBT0001_BR_INFO BIL1") 'ブレーカー(関連付け)
        sqlStat.AppendLine("    ON  BIL1.BRID         = BS.BRID")
        sqlStat.AppendLine("   AND  BIL1.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  BIL1.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  BIL1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  BIL1.TYPE         = 'POL1'")
        sqlStat.AppendLine("   AND  BIL1.BRTYPE       = '" & C_BRTYPE.REPAIR & "'")
        sqlStat.AppendLine("  INNER JOIN GBT0001_BR_INFO BIIF") 'ブレーカー(関連付け)
        sqlStat.AppendLine("    ON  BIIF.BRID         = BS.BRID")
        sqlStat.AppendLine("   AND  BIIF.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  BIIF.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  BIIF.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  BIIF.TYPE         = 'INFO'")
        sqlStat.AppendLine("   AND  BIIF.BRTYPE       = '" & C_BRTYPE.REPAIR & "'")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AHIF") '承認履歴
        sqlStat.AppendLine("    ON  AHIF.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AHIF.APPLYID      = BIIF.APPLYID")
        sqlStat.AppendLine("   AND  AHIF.STEP         = BIIF.LASTSTEP")
        sqlStat.AppendLine("   AND  AHIF.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FVIF") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FVIF.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FVIF.KEYCODE      = AHIF.STATUS")
        sqlStat.AppendLine("   AND  FVIF.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0003_DEPOT DP") 'DEPOT名称用JOIN
        sqlStat.AppendLine("    ON  DP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  DP.DEPOTCODE    = BS.DEPOTCODE")
        sqlStat.AppendLine("   AND  DP.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  DP.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  DP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine(" WHERE BS.DELFLG   <> @DELFLG")
        '動的検索条件のSQL付与
        If Me.hdnEndYMD.Value <> "" Then
            'StYmd
            sqlStat.AppendLine("   AND BS.STYMD   <= @STYMD")
        End If

        If Me.hdnStYMD.Value <> "" Then
            'End
            sqlStat.AppendLine("   AND BS.STYMD   >= @ENDYMD")
        End If

        If Me.hdnTankNo.Value <> "" Then
            'TankNo
            sqlStat.AppendLine("   AND BS.TANKNO   = @TANKNO")
        End If

        If Me.hdnDepot.Value <> "" Then
            'Depot
            sqlStat.AppendLine("   AND DP.DEPOTCODE   = @DEPOTCODE")
        End If
        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            Dim paramLangDisp As SqlParameter = sqlCmd.Parameters.Add("@LANGDISP", SqlDbType.NVarChar, 20)
            'SQLパラメータ(動的変化あり)
            Dim paramStYmd As SqlParameter = Nothing
            Dim paramEndYmd As SqlParameter = Nothing
            Dim paramTankNo As SqlParameter = Nothing
            Dim paramDepotCode As SqlParameter = Nothing
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramDelFlg.Value = CONST_FLAG_YES
            paramLangDisp.Value = COA0019Session.LANGDISP

            If Me.hdnEndYMD.Value <> "" Then '検索条件のTOをFROMと突き合わせ
                'StYmd
                paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
                paramStYmd.Value = Date.ParseExact(Me.hdnEndYMD.Value, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
            End If

            If Me.hdnStYMD.Value <> "" Then '検索条件のFROMをTOと突き合わせ
                'EndYmd
                paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
                paramEndYmd.Value = Date.ParseExact(Me.hdnStYMD.Value, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
            End If

            If Me.hdnTankNo.Value <> "" Then
                'TANKNO
                paramTankNo = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar, 20)
                paramTankNo.Value = Me.hdnTankNo.Value
            End If

            If Me.hdnDepot.Value <> "" Then
                'DEPOTCODE
                paramDepotCode = sqlCmd.Parameters.Add("@DEPOTCODE", SqlDbType.NVarChar, 20)
                paramDepotCode.Value = Me.hdnDepot.Value
            End If

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return retDt
    End Function

    ''' <summary>
    ''' 一覧 マウスホイール時処理 (一覧スクロール)
    ''' </summary>
    Protected Sub ListScrole()
        'If hdnMouseWheel.Value = "" Then
        '    Return
        'End If
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル

        '表示データ件数取得
        Dim DataCnt As Integer = 0                  '(絞り込み後)有効Data数

        '一覧表示データ復元
        Dim dt As DataTable = CreateDataTable()

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To dt.Rows.Count - 1
            If Convert.ToString(dt.Rows(i)(4)) = "0" Then
                DataCnt = DataCnt + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                dt.Rows(i)("SELECT") = DataCnt
            End If
        Next

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If

        '現在表示位置取得
        Dim ListPosition As Integer = 0
        If Me.hdnListPosition.Value = "" Then
            ListPosition = 1
        Else
            Try
                Integer.TryParse(Me.hdnListPosition.Value, ListPosition)
            Catch ex As Exception
                ListPosition = 1
            End Try
        End If

        Dim ScrollInt As Integer = CONST_SCROLLROWCOUNT
        '表示位置決定(次頁スクロール)
        If hdnMouseWheel.Value = "+" And
        (ListPosition + ScrollInt) < DataCnt Then
            ListPosition = ListPosition + ScrollInt
        End If

        '表示位置決定(前頁スクロール)
        If hdnMouseWheel.Value = "-" And
        (ListPosition - ScrollInt) >= 0 Then
            ListPosition = ListPosition - ScrollInt
        End If

        'ソート
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, ListPosition, hdnListPosition)
        '一覧作成
        COA0013TableObject.MAPID = CONST_MAPID
        COA0013TableObject.VARI = "Default"
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = Me.WF_LISTAREA
        COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.LEVENT = "ondblclick"
        COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.APPLYチェックがついているチェックボックスオブジェクトをチェック状態にする
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            For Each targetCheckBoxId As String In {"CHECK"} '複数チェックボックスを配置している場合は配列に追加

                '申請チェックボックスの加工
                Dim targetCheckBoxLineCnt = (From dr As DataRow In listData
                                             Where Convert.ToString(dr.Item(targetCheckBoxId)) <> ""
                                             Select Convert.ToInt32(dr.Item("LINECNT")))
                For Each lineCnt As Integer In targetCheckBoxLineCnt
                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & targetCheckBoxId & lineCnt.ToString
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        chkObj.Checked = True
                    End If
                Next

            Next
            '申請チェックの使用可否制御
            Dim disableCheckBoxLineCnt As EnumerableRowCollection(Of Integer)
            If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
                disableCheckBoxLineCnt = (From dr As DataRow In listData Select Convert.ToInt32(dr.Item("LINECNT")))
                Me.WF_LISTAREA.Attributes.Add("data-hidedelete", "1")
            Else
                disableCheckBoxLineCnt = (From dr As DataRow In listData
                                          Where {C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE, C_APP_STATUS.APPLYING}.Contains(Trim(Convert.ToString(dr.Item("STATUS"))))
                                          Select Convert.ToInt32(dr.Item("LINECNT")))
            End If
            For Each lineCnt As Integer In disableCheckBoxLineCnt
                Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & "CHECK" & lineCnt.ToString
                Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                If tmpObj IsNot Nothing Then
                    Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                    chkObj.Enabled = False
                End If
            Next

        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

    End Sub
    ''' <summary>
    ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateDataTable() As DataTable
        Dim retDt As New DataTable
        '共通項目
        retDt.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        retDt.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        retDt.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        retDt.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        retDt.Columns.Add("HIDDEN", GetType(Integer))
        '個別項目
        retDt.Columns.Add("ACTION", GetType(String))
        retDt.Columns.Add("BRID", GetType(String))
        retDt.Columns.Add("BRBASEID", GetType(String))
        retDt.Columns.Add("SUBID", GetType(String))
        retDt.Columns.Add("STYMD", GetType(String))
        retDt.Columns.Add("ENDYMD", GetType(String))
        retDt.Columns.Add("TANKNO", GetType(String))
        retDt.Columns.Add("DEPOTCODE", GetType(String))
        retDt.Columns.Add("DEPOTNAME", GetType(String))
        retDt.Columns.Add("LOCATION", GetType(String))
        retDt.Columns.Add("HASREMARK", GetType(String))
        retDt.Columns.Add("APPLYORDENY", GetType(String))
        retDt.Columns.Add("CHECK", GetType(String))
        retDt.Columns.Add("DELETEFLAG", GetType(String))

        retDt.Columns.Add("APPLYID", GetType(String))
        retDt.Columns.Add("STEP", GetType(String))
        retDt.Columns.Add("STATUS", GetType(String))
        retDt.Columns.Add("AGENTORGANIZER", GetType(String))

        retDt.Columns("CHECK").DefaultValue = ""
        '削除
        retDt.Columns.Add("DELETEBTN", GetType(String))

        Return retDt
    End Function
    ''' <summary>
    ''' リスト行ダブルクリック時イベント
    ''' </summary>
    Private Sub ListRowDbClick()
        Dim rowIdString As String = Me.hdnListDBclick.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(rowIdString, rowId) = True Then
            rowId = rowId - 1
        Else
            Return
        End If

        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New COA0021ListTable

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If
        Me.hdnXMLsaveFileRet.Value = hdnXMLsaveFile.Value

        Dim selectedRow As DataRow = dt.Rows(rowId)
        Me.hdnSelectedBrId.Value = Convert.ToString(selectedRow.Item("BRID"))
        Me.hdnSelectedStatus.Value = Convert.ToString(selectedRow.Item("STATUS"))
        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID & "R"
        COA0012DoUrl.VARIP = "GB_ShowDetail"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        Session("MAPmapid") = CONST_MAPID & "R"
        Session("MAPvariant") = "GB_ShowDetail"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    Public Sub btnExtract_Click()
        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        '一覧表示データ復元 
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If

        'フィルタでの絞り込みを利用するか確認
        Dim isFillterOff As Boolean = True
        If Me.txtTankNo.Text.Trim <> "" OrElse Me.txtApproval.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtTankNo.Text.Trim = "" OrElse Convert.ToString(dr("TANKNO")) = Me.txtTankNo.Text.Trim) _
                  AndAlso (Me.txtApproval.Text.Trim = "" OrElse Convert.ToString(dr("APPLYORDENY")) = Me.txtApproval.Text.Trim)) Then
                    dr.Item("HIDDEN") = 1
                End If
            End If
        Next
        '画面先頭を表示
        hdnListPosition.Value = "1"

        '一覧表示データ保存
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
        Else
            'メッセージ表示
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALEXTRUCT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        End If

        'カーソル設定
        Me.txtTankNo.Focus()

    End Sub
    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00011SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00011SELECT = DirectCast(Page.PreviousPage, GBT00011SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtStYMD", Me.hdnStYMD},
                                                                           {"txtEndYMD", Me.hdnEndYMD},
                                                                           {"txtTankNo", Me.hdnTankNo},
                                                                           {"txtDepot", Me.hdnDepot}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmpText As TextBox = DirectCast(tmpCont, TextBox)
                    item.Value.Value = tmpText.Text
                End If
            Next
        ElseIf TypeOf Page.PreviousPage Is GBT00012REPAIR Then
            '単票画面の場合
            Dim prevObj As GBT00012REPAIR = DirectCast(Page.PreviousPage, GBT00012REPAIR)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnTankNo", Me.hdnTankNo},
                                                                        {"hdnDepot", Me.hdnDepot}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmphdn.Value
                End If
            Next

            Dim tmpXMLFile As Control = prevObj.FindControl("hdnXMLsaveFileRet")

            If tmpXMLFile IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpXMLFile, HiddenField)
                Me.hdnXMLsaveFileRet.Value = tmphdn.Value
            End If

            'Dim tmpMsg As Control = prevObj.FindControl("hdnMsgId")

            'If tmpMsg IsNot Nothing Then
            '    Dim tmphdn As HiddenField = DirectCast(tmpMsg, HiddenField)
            '    Me.hdnMsgId.Value = tmphdn.Value
            'End If

        ElseIf TypeOf Page.PreviousPage Is GBT00011RESULT Then
            '同画面の場合
            Dim prevObj As GBT00011RESULT = DirectCast(Page.PreviousPage, GBT00011RESULT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnTankNo", Me.hdnTankNo},
                                                                        {"hdnDepot", Me.hdnDepot}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmphdn.Value
                End If
            Next

            Dim tmpMsg As Control = prevObj.FindControl("hdnMsgId")

            If tmpMsg IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpMsg, HiddenField)
                Me.hdnMsgId.Value = tmphdn.Value
            End If

        End If
    End Sub

    ''' <summary>
    ''' タンク番号リストアイテムを設定
    ''' </summary>
    Private Function SetTankListItem(selectedValue As String) As String

        Dim GBA00012TankInfo As New GBA00012TankInfo

        Try
            'リストクリア
            Me.lbTank.Items.Clear()

            GBA00012TankInfo.LISTBOX_TANK = Me.lbTank
            GBA00012TankInfo.GBA00012getLeftListTank()
            If GBA00012TankInfo.ERR = C_MESSAGENO.NORMAL OrElse GBA00012TankInfo.ERR = C_MESSAGENO.NODATA Then
                Me.lbTank = DirectCast(GBA00012TankInfo.LISTBOX_TANK, ListBox)
            Else
                Return GBA00012TankInfo.ERR
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbTank.Items.Count > 0 Then
                Dim findListItem = Me.lbTank.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            Return C_MESSAGENO.NORMAL

        Catch ex As Exception
            Dim retCode As String = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = retCode
            COA0003LogFile.COA0003WriteLog()
            Return retCode
        End Try
    End Function
    ''' <summary>
    ''' 承認リストアイテムを設定
    ''' </summary>
    Private Function SetApprovalListItem(selectedValue As String) As String
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Dim retCode As String = C_MESSAGENO.NORMAL

        Try
            'リストクリア
            Me.lbApproval.Items.Clear()

            'ユーザＩＤListBox設定
            COA0017FixValue.COMPCODE = GBC_COMPCODE_D
            COA0017FixValue.CLAS = "APPROVAL"
            If COA0019Session.LANGDISP = C_LANG.JA Then
                COA0017FixValue.LISTBOX1 = Me.lbApproval
            Else
                COA0017FixValue.LISTBOX2 = Me.lbApproval
            End If
            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                If COA0019Session.LANGDISP = C_LANG.JA Then
                    Me.lbApproval = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
                Else
                    Me.lbApproval = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
                End If
            Else
                retCode = COA0017FixValue.ERR
            End If
            Return retCode

        Catch ex As Exception
            retCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = retCode
            COA0003LogFile.COA0003WriteLog()
            Return retCode
        End Try
    End Function
End Class