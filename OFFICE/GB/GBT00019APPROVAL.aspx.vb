Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' タンク引当承認画面クラス
''' </summary>
Public Class GBT00019APPROVAL
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00019A"   '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_APP_ALL_JP = "全て"
    Private Const CONST_APP_ALL_EN = "All"
    Private Const CONST_APP_APR_JP = "承認者"
    Private Const CONST_APP_APR_EN = "Approver"

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
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = "GB_Default"
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})
                    Return
                End If
                '****************************************
                '初回絞り込み設定
                '****************************************
                If Me.hdnExtractTankNo.Value <> "" Then
                    Me.txtTankNo.Text = Me.hdnExtractTankNo.Value
                End If
                If Me.hdnExtractApp.Value = "" Then
                    If COA0019Session.LANGDISP = C_LANG.JA Then
                        Me.txtApprovalObj.Text = CONST_APP_APR_JP
                    Else
                        Me.txtApprovalObj.Text = CONST_APP_APR_EN
                    End If
                Else
                    Me.txtApprovalObj.Text = Me.hdnExtractApp.Value
                End If
                Me.txtApplicantId.Text = Me.hdnExtractApplicant.Value
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

                                If Convert.ToString(chkdt.Rows(i)("APPLYID")) = Convert.ToString(dt.Rows(j)("APPLYID")) AndAlso
                                    Convert.ToString(chkdt.Rows(i)("STEP")) = Convert.ToString(dt.Rows(j)("STEP")) Then

                                    dt.Rows(j)("CHECK") = chkdt.Rows(i)("CHECK")
                                End If
                            Next
                        Next

                    End If

                    For Each dr As DataRow In dt.Rows
                        'フィルタ使用時の場合
                        'If Not (Me.txtTankNo.Text.Trim = "") Then
                        '    '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                        '    If Not Convert.ToString(dr.Item("TANKNO")) = Me.txtTankNo.Text.Trim Then
                        '        dr.Item("HIDDEN") = 1
                        '    Else
                        '        dr.Item("HIDDEN") = 0
                        '    End If
                        'Else
                        '    dr.Item("HIDDEN") = 0
                        'End If

                        'If Convert.ToString(dr.Item("HIDDEN")) = "0" AndAlso Not (Me.txtApprovalObj.Text.Trim = CONST_APP_ALL_JP OrElse Me.txtApprovalObj.Text.Trim = CONST_APP_ALL_EN) Then
                        '    '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                        '    If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Then
                        '        dr.Item("HIDDEN") = 1
                        '    Else
                        '        dr.Item("HIDDEN") = 0
                        '    End If
                        'End If
                        dr.Item("HIDDEN") = 0
                        If Not (
                            (Me.txtTankNo.Text.Trim = "" OrElse Convert.ToString(dr("TANKNO")).Trim.Equals(Me.txtTankNo.Text.Trim)) _
                            AndAlso (Me.txtApprovalObj.Text.Trim = CONST_APP_ALL_JP OrElse Me.txtApprovalObj.Text.Trim = CONST_APP_ALL_EN OrElse Me.txtApprovalObj.Text.Trim = "" OrElse Convert.ToString(dr("APPROVALOBJECT")).Trim.Equals(Me.txtApprovalObj.Text.Trim)) _
                            AndAlso (Me.txtApplicantId.Text.Trim = "" OrElse Convert.ToString(dr("APPLICANTID")).Trim.Equals(Me.txtApplicantId.Text.Trim))
                            ) Then
                            dr.Item("HIDDEN") = 1
                        End If

                    Next

                    'グリッド用データをファイルに退避
                    With Nothing
                        Dim COA0021ListTable As New COA0021ListTable
                        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
                        COA0021ListTable.TBLDATA = dt
                        COA0021ListTable.COA0021saveListTable()
                        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {COA0021ListTable.ERR})
                            Return
                        End If
                    End With

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = Me.hdnPrevViewID.Value
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
                        .LEVENT = "ondblclick"
                        .LFUNC = "ListDbClick"
                        .TITLEOPT = True
                        .NOCOLUMNWIDTHOPT = 50
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .USERSORTOPT = 1
                    End With
                    COA0013TableObject.COA0013SetTableObject()

                    If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
                        Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                                  Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
                        ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
                    Else
                        ViewState("DISPLAY_LINECNT_LIST") = Nothing
                    End If

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

                        If Not (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.APPLYING OrElse Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.REVISE) Then
                            chk.Enabled = False
                        Else
                            chk.Enabled = True
                        End If
                    Next

                End Using 'DataTable

                'メッセージ設定
                If hdnMsgId.Value <> "" Then
                    CommonFunctions.ShowMessage(hdnMsgId.Value, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                End If

                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'URL設定
                '****************************************
                Me.hdnTankViewUrl.Value = GetTankUrl()
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
                    'Return '単票ページにリダイレクトするため念のため処理は終わらせる
                End If
                '**********************
                ' 承認理由入力ボックス表示
                '**********************
                If Me.hdnRemarkboxField.Value <> "" Then
                    DisplayApplyReason(True)
                    Me.divRemarkInputBoxWrapper.Style("display") = "block"
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
            DisplayListObjEdit() 'リストオブジェクトの編集
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
                        Me.hdnCalendarValue.Value = FormatDateYMD(txtobj.Text, GBA00003UserSetting.DATEFORMAT)

                        Me.mvLeft.Focus()
                    End If
                'タンク番号ビュー表示切替
                Case Me.vLeftTankNo.ID
                    SetTankNoListItem(Me.txtTankNo.Text)
                '承認ビュー表示切替
                Case Me.vLeftApprovalObj.ID
                    SetApprovalObjListItem(Me.txtApprovalObj.Text)
            End Select
        End If

    End Sub
    ''' <summary>
    ''' 申請理由表示処理
    ''' </summary>
    ''' <param name="isOpen"></param>
    Private Sub DisplayApplyReason(isOpen As Boolean)
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        '一覧表示データ復元 
        Dim dt As DataTable = CreateDataTable()

        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        Dim uniqueIndex As String = Me.hdnCurrentUnieuqIndex.Value
        Dim targetRow = (From dr In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = uniqueIndex)

        If targetRow IsNot Nothing AndAlso targetRow.Count > 0 Then
            If isOpen = True Then
                Me.txtRemarkInput.Text = Convert.ToString(targetRow(0).Item("APPROVEDTEXT"))
                Me.txtRemarkInput.Focus()
            Else
                targetRow(0).Item("APPROVEDTEXT") = Me.txtRemarkInput.Text
                '一覧表データの保存
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    Throw New Exception("Update Approved Text Failed")
                End If
            End If
        End If

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = "Default"
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
    ''' Excelダウンロードボタン押下時
    ''' </summary>
    Public Sub btnExcelDownload_Click()

        Dim COA0021ListTable As New COA0021ListTable
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
    ''' 承認ボタン押下時
    ''' </summary>
    Public Sub btnApproval_Click()
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
        'ステータス否認かつチェック付のレコードを取得
        Dim rejectRow = From item As DataRow In dt
                        Where Convert.ToString(item("STATUS")).Trim = C_APP_STATUS.REVISE _
                      AndAlso Convert.ToString(item("CHECK")) = "on"
        If rejectRow.Any Then
            '否認レコードありの場合処理終了
            'メッセージ出力
            CommonFunctions.ShowMessage(C_MESSAGENO.REVISING, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Dim errNo As String = ""
        'CHECKチェックボックスがチェック済の全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("CHECK")) = "on")
        Dim checkedDt As DataTable = Nothing
        If q.Any = True Then
            checkedDt = q.CopyToDataTable
        Else
            checkedDt = dt.Clone
        End If
        For Each checkedDr As DataRow In checkedDt.Rows 'For i As Integer = 0 To dt.Rows.Count - 1

            '承認登録
            COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
            COA0032Apploval.I_APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            COA0032Apploval.I_STEP = Convert.ToString(checkedDr.Item("STEP"))
            COA0032Apploval.COA0032setApproval()
            If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
                'Return
                If errNo = "" Then
                    errNo = COA0032Apploval.O_ERR
                End If
                Continue For
            End If

            '承認コメント更新処理
            UpdateApprovedText(Convert.ToString(HttpContext.Current.Session("APSRVCamp")), Convert.ToString(checkedDr.Item("APPLYID")),
                               Convert.ToString(checkedDr.Item("STEP")), Convert.ToString(checkedDr.Item("APPROVEDTEXT")))

            If Convert.ToString(checkedDr.Item("LASTSTEP")) = Convert.ToString(checkedDr.Item("STEP")) Then

                'ActualDate更新
                UpdateActualDate(Convert.ToString(checkedDr.Item("TANKSEQ")), Convert.ToString(checkedDr.Item("ORDERNO")))

                ' 最終承認の場合メール送信
                Dim GBA00009MailSendSet As New GBA00009MailSendSet
                GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                GBA00009MailSendSet.EVENTCODE = "ODR_Approved_Tank"
                GBA00009MailSendSet.MAILSUBCODE = ""
                GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
                GBA00009MailSendSet.APPLYSTEP = Convert.ToString(checkedDr.Item("STEP"))
                GBA00009MailSendSet.ORDERNO = Convert.ToString(checkedDr.Item("ORDERNO"))
                GBA00009MailSendSet.GBA00009setMailToTank()
                If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                    'CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                    'Return
                    If errNo = "" Then
                        errNo = GBA00009MailSendSet.ERR
                    End If
                    Continue For
                End If

            End If

        Next

        '絞り込み
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = CONST_APP_ALL_JP
            Else
                Me.txtApprovalObj.Text = CONST_APP_ALL_EN
            End If
        End If
        Me.hdnExtractTankNo.Value = Me.txtTankNo.Text
        Me.hdnExtractApp.Value = Me.txtApprovalObj.Text
        Me.hdnExtractApplicant.Value = Me.txtApplicantId.Text

        If errNo <> "" Then
            CommonFunctions.ShowMessage(errNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.APPROVALSUCCESS

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        HttpContext.Current.Session("MAPmapid") = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "GB_Default"
        COA0012DoUrl.MAPIDP = Convert.ToString(HttpContext.Current.Session("MAPmapid"))
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

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
                Case Me.vLeftTankNo.ID 'アクティブなビューがタンク番号
                    'タンク番号選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbTankNo.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbTankNo.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftApprovalObj.ID 'アクティブなビューが承認対象
                    '承認対象選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbApprovalObj.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbApprovalObj.SelectedItem.Text
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
    ''' 備考入力ボックスのOKボタン押下時イベント
    ''' </summary>
    Public Sub btnRemarkInputOk_Click()
        DisplayApplyReason(False)

        Me.hdnRemarkboxOpen.Value = ""
        Me.hdnRemarkboxField.Value = ""
        Me.hdnCurrentUnieuqIndex.Value = ""
        'マルチライン入力ボックスの非表示
        Me.divRemarkInputBoxWrapper.Style("display") = "none"
    End Sub
    ''' <summary>
    ''' 備考入力ボックスのキャンセルボタン押下時イベント
    ''' </summary>
    Public Sub btnRemarkInputCancel_Click()

        Me.hdnRemarkboxOpen.Value = ""
        Me.hdnRemarkboxField.Value = ""
        Me.hdnCurrentUnieuqIndex.Value = ""
        'マルチライン入力ボックスの非表示
        Me.divRemarkInputBoxWrapper.Style("display") = "none"
    End Sub
    ''' <summary>
    ''' 先頭頁ボタン押下時
    ''' </summary>
    Public Sub btnFIRST_Click()

        'ポジションを設定するのみ
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
        AddLangSetting(dicDisplayText, Me.btnReject, "否認", "Reject")
        AddLangSetting(dicDisplayText, Me.btnApproval, "承認", "Approval")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.lblTankNoLabel, "タンク番号", "Tank No")
        AddLangSetting(dicDisplayText, Me.lblApprovalObjLabel, "承認種別", "Approval Type")
        AddLangSetting(dicDisplayText, Me.lblApplicantIdLabel, "申請者", "Applicant")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)

    End Sub


    ''' <summary>
    ''' 一覧表のデータテーブルを取得する関数
    ''' </summary>
    ''' <returns></returns>
    Private Function GetListDataTable() As DataTable
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得
        '文言フィールド（開発中のためいったん固定
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If
        Dim textProductTblField As String = "NAMES"
        'If COA0019Session.LANGDISP <> C_LANG.JA Then
        '    textProductTblField = "NAMES"
        'End If

        'ソート順取得
        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = "Default"
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder
        Dim retDt As New DataTable
        '共通テーブル定義START
        sqlStat.AppendLine("with")
        '直近３積載品
        sqlStat.AppendLine("WITH_P3HIST as (")
        sqlStat.AppendLine("    select TANKNO,[1] as HIST1,[2] as HIST2,[3] as HIST3")
        sqlStat.AppendLine("    from")
        sqlStat.AppendLine("    (")
        sqlStat.AppendLine("        select RANK() OVER(PARTITION BY OV.TANKNO ORDER BY OV.ACTUALDATE desc) as RECENT,")
        sqlStat.AppendLine("               OV.TANKNO, TRIM(P.PRODUCTNAME) as PRODUCTNAME")
        sqlStat.AppendLine("        from GBT0005_ODR_VALUE OV")
        sqlStat.AppendLine("        inner join GBT0004_ODR_BASE OB")
        sqlStat.AppendLine("        on OB.ORDERNO = OV.ORDERNO")
        sqlStat.AppendLine("        and OB.DELFLG <> @DELFLG")
        sqlStat.AppendLine("        inner join GBM0008_PRODUCT P")
        sqlStat.AppendLine("        on P.PRODUCTCODE = OB.PRODUCTCODE")
        sqlStat.AppendLine("        and P.STYMD  <= OB.STYMD")
        sqlStat.AppendLine("        and P.ENDYMD >= OB.STYMD")
        sqlStat.AppendLine("        and P.DELFLG <> @DELFLG")
        sqlStat.AppendLine("        where OV.ACTIONID = 'LOAD'")
        sqlStat.AppendLine("        and   OV.DELFLG <> @DELFLG")
        sqlStat.AppendLine("        and   OV.ACTUALDATE <> @INITDATE")
        sqlStat.AppendLine("    ) as RECNT_LOAD")
        sqlStat.AppendLine("    PIVOT (")
        sqlStat.AppendLine("        max(PRODUCTNAME) for RECENT in ([1],[2],[3])")
        sqlStat.AppendLine("    ) as PivotTable")
        sqlStat.AppendLine(")")

        '承認情報取得
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(AH.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")

        sqlStat.AppendLine("      ,OV2.APPLYID As APPLYID")
        sqlStat.AppendLine("      ,OV2.ORDERNO As ORDERNO")
        sqlStat.AppendLine("      ,OV2.LASTSTEP As LASTSTEP")
        sqlStat.AppendLine("      ,OV.TANKNO As TANKNO")
        sqlStat.AppendLine("      ,OV2.TANKSEQ As TANKSEQ")

        sqlStat.AppendLine("      ,TM.REPAIRSTAT As REPAIRSTAT")
        sqlStat.AppendLine("      ,'' As REPAIRDATE")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, TM.INSPECTDATE5 , 111),'') As INSPECTDATE5")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, TM.INSPECTDATE2P5 , 111),'') As INSPECTDATE2P5")
        sqlStat.AppendLine("      ,TM.NEXTINSPECTTYPE As NEXTINSPECTTYPE")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, TM.NEXTINSPECTDATE , 111),'') As NEXTINSPECTDATE")

        sqlStat.AppendLine("      ,isnull(convert(nvarchar, POL1.SCHEDELDATE , 111),'') As POL1SCHEDELDATE")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, POD1.SCHEDELDATE , 111),'') As POD1SCHEDELDATE")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, POL2.SCHEDELDATE , 111),'') As POL2SCHEDELDATE")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, POD2.SCHEDELDATE , 111),'') As POD2SCHEDELDATE")

        sqlStat.AppendLine("      ,OB.LOADING As LOADING")
        sqlStat.AppendLine("      ,OB.STEAMING As STEAMING")
        sqlStat.AppendLine("      ,OB.TIP As TIP")
        sqlStat.AppendLine("      ,OB.EXTRA As EXTRA")
        sqlStat.AppendLine("      ,isnull(LPORT1.AREANAME,'') As LOADPORT1")
        sqlStat.AppendLine("      ,isnull(LPORT2.AREANAME,'') As LOADPORT2")
        sqlStat.AppendLine("      ,isnull(DPORT1.AREANAME,'') As DELIVERYPORT1")
        sqlStat.AppendLine("      ,isnull(DPORT2.AREANAME,'') As DELIVERYPORT2")

        sqlStat.AppendLine("      ,isnull(TRIM(PD.PRODUCTNAME),'') As PURODUCT")

        sqlStat.AppendLine("      ,isnull(P3HIST.HIST1,'') as PD_HIST1")
        sqlStat.AppendLine("      ,isnull(P3HIST.HIST2,'') as PD_HIST2")
        sqlStat.AppendLine("      ,isnull(P3HIST.HIST3,'') as PD_HIST3")

        sqlStat.AppendFormat("      ,isnull(SP.{0},'') AS SHIPPER", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,isnull(CN.{0},'') AS CONSIGNEE", textCustomerTblField).AppendLine()

        sqlStat.AppendLine("      ,isnull(convert(nvarchar, LOAD.ACTUALDATE , 111),'')  As LOADDATE")

        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN CASE WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN ISNULL(FV1.VALUE1,'') + '+' ")
        sqlStat.AppendLine("                                            ELSE CASE WHEN AH.STEP > ISNULL(AH2.STEP,'" & C_APP_FIRSTSTEP & "') THEN ISNULL(FV1.VALUE1,'') + '*' ")
        sqlStat.AppendLine("                                            ELSE ISNULL(FV1.VALUE1,'') END END ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN CASE WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN ISNULL(FV1.VALUE2,'') + '+'  ")
        sqlStat.AppendLine("                                            ELSE CASE WHEN AH.STEP > ISNULL(AH2.STEP,'" & C_APP_FIRSTSTEP & "') THEN ISNULL(FV1.VALUE2,'') + '*' ")
        sqlStat.AppendLine("                                            ELSE ISNULL(FV1.VALUE2,'') END END END AS APPROVALOBJECT ")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END As APPROVALORREJECT")
        sqlStat.AppendLine("      ,AH.APPROVEDTEXT As APPROVEDTEXT")
        sqlStat.AppendLine("      ,'' As ""CHECK""")
        sqlStat.AppendLine("      ,AH.STEP As STEP")
        sqlStat.AppendLine("      ,AH.STATUS As STATUS")
        sqlStat.AppendLine("      ,AH.APPLICANTID As APPLICANTID")
        sqlStat.AppendLine("      ,CASE WHEN (AH3.STEP = OV2.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
        sqlStat.AppendLine("            WHEN (AH3.STEP = OV2.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
        sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH3.STEP,'00'))))) + '/' + trim(convert(char,convert(int,OV2.LASTSTEP))) END as STEPSTATE")
        sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
        sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
        sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END As CURSTEP")
        sqlStat.AppendLine("      ,AP.APPROVALTYPE As APPROVALTYPE")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(TRIM(US.STAFFNAMES),'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(TRIM(US.STAFFNAMES_EN),'') END As APPROVERID")
        sqlStat.AppendLine("      ,convert(nvarchar, AH.APPLYDATE , 111) As APPLYDATE")

        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
        sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
        sqlStat.AppendLine("    On  AP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   And  AP.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   And  AP.EVENTCODE    = AH.EVENTCODE")
        sqlStat.AppendLine("   And  AP.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("   And  AP.STEP         = AH.STEP")
        sqlStat.AppendLine("   And  AP.USERID       = @USERID")
        sqlStat.AppendLine("   And  AP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And  AP.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And  AP.DELFLG      <> @DELFLG")

        'オーダー(明細)２
        sqlStat.AppendLine("  INNER JOIN ( ")
        sqlStat.AppendLine("  SELECT ORDERNO,TANKSEQ,APPLYID,LASTSTEP")
        sqlStat.AppendLine("  FROM GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("  WHERE DELFLG <> @DELFLG ")
        sqlStat.AppendLine("    AND STYMD  <= @STYMD")
        sqlStat.AppendLine("    AND ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("  GROUP BY ORDERNO,TANKSEQ,APPLYID,LASTSTEP ) AS OV2 ")
        sqlStat.AppendLine("    ON  OV2.APPLYID      = AH.APPLYID")

        'オーダー(明細)
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT ORDERNO,TANKSEQ,TANKNO ")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE ")
        sqlStat.AppendLine("  WHERE DELFLG <> @DELFLG ")
        sqlStat.AppendLine("    AND STYMD  <= @STYMD")
        sqlStat.AppendLine("    AND ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("    AND TANKNO <> ''")
        sqlStat.AppendLine("  GROUP BY ORDERNO,TANKSEQ,TANKNO ) AS OV ")
        sqlStat.AppendLine("    ON  OV.ORDERNO      = OV2.ORDERNO")
        sqlStat.AppendLine("   AND  OV.TANKSEQ      = OV2.TANKSEQ")

        'オーダー(基本)
        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OB")
        sqlStat.AppendLine("    On  OB.ORDERNO      = OV2.ORDERNO")
        sqlStat.AppendLine("   And  OB.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And  OB.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And  OB.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
        sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MAX(STEP) AS STEP ")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS  > '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH3 ")
        sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH3.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH3.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH4 ")
        sqlStat.AppendLine("    ON AH3.APPLYID = AH4.APPLYID ")
        sqlStat.AppendLine("   AND AH3.STEP    = AH4.STEP ")
        sqlStat.AppendLine("   AND AH4.DELFLG <> @DELFLG ")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US") 'APPROVER名称用JOIN
        sqlStat.AppendLine("    ON  US.USERID       = AH4.APPROVERID")
        sqlStat.AppendLine("   AND  US.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  US.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  US.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
        sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
        sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
        sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
        sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
        sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
        sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
        sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
        sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
        sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")

        'タンクマスタ
        sqlStat.AppendLine("  LEFT JOIN GBM0006_TANK TM")
        sqlStat.AppendLine("    ON  TM.TANKNO       = OV.TANKNO")
        sqlStat.AppendLine("   AND  TM.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  TM.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  TM.DELFLG      <> @DELFLG")

        'POL1VALUE
        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE POL1")
        sqlStat.AppendLine("    ON  POL1.ORDERNO      = OV2.ORDERNO")
        sqlStat.AppendLine("   AND  POL1.DATEFIELD   IN ('ETD','ETD1')")
        sqlStat.AppendLine("   AND  POL1.TANKSEQ      = OV2.TANKSEQ")
        sqlStat.AppendLine("   AND  POL1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  POL1.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  POL1.DELFLG      <> @DELFLG")

        'POD1VALUE
        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE POD1")
        sqlStat.AppendLine("    ON  POD1.ORDERNO      = OV2.ORDERNO")
        sqlStat.AppendLine("   AND  POD1.DATEFIELD   IN ('ETA','ETA1')")
        sqlStat.AppendLine("   AND  POD1.TANKSEQ      = OV2.TANKSEQ")
        sqlStat.AppendLine("   AND  POD1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  POD1.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  POD1.DELFLG      <> @DELFLG")

        'POL2VALUE
        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE POL2")
        sqlStat.AppendLine("    ON  POL2.ORDERNO      = OV2.ORDERNO")
        sqlStat.AppendLine("   AND  POL2.DATEFIELD    = 'ETD2'")
        sqlStat.AppendLine("   AND  POL2.TANKSEQ      = OV2.TANKSEQ")
        sqlStat.AppendLine("   AND  POL2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  POL2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  POL2.DELFLG      <> @DELFLG")

        'POD2VALUE
        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE POD2")
        sqlStat.AppendLine("    ON  POD2.ORDERNO      = OV2.ORDERNO")
        sqlStat.AppendLine("   AND  POD2.DATEFIELD    = 'ETA2'")
        sqlStat.AppendLine("   AND  POD2.TANKSEQ      = OV2.TANKSEQ")
        sqlStat.AppendLine("   AND  POD2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  POD2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  POD2.DELFLG      <> @DELFLG")

        'LOAD
        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE LOAD")
        sqlStat.AppendLine("    ON  LOAD.ORDERNO      = OV2.ORDERNO")
        sqlStat.AppendLine("   AND  LOAD.ACTIONID     = 'LOAD'    ")
        sqlStat.AppendLine("   AND  LOAD.TANKSEQ      = OV2.TANKSEQ")
        sqlStat.AppendLine("   AND  LOAD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  LOAD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  LOAD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  LOAD.ACTUALDATE  <> @INITDATE")

        'LOADPORT1
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT LPORT1")
        sqlStat.AppendLine("    ON  LPORT1.PORTCODE     = OB.LOADPORT1")
        sqlStat.AppendLine("   AND  LPORT1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  LPORT1.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  LPORT1.DELFLG      <> @DELFLG")

        'LOADPORT2
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT LPORT2")
        sqlStat.AppendLine("    ON  LPORT2.PORTCODE     = OB.LOADPORT2")
        sqlStat.AppendLine("   AND  LPORT2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  LPORT2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  LPORT2.DELFLG      <> @DELFLG")

        'DELIVERYPORT1
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT DPORT1")
        sqlStat.AppendLine("    ON  DPORT1.PORTCODE     = OB.DELIVERYPORT1")
        sqlStat.AppendLine("   AND  DPORT1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  DPORT1.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  DPORT1.DELFLG      <> @DELFLG")

        'DELIVERYPORT2
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT DPORT2")
        sqlStat.AppendLine("    ON  DPORT2.PORTCODE     = OB.DELIVERYPORT2")
        sqlStat.AppendLine("   AND  DPORT2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  DPORT2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  DPORT2.DELFLG      <> @DELFLG")

        'Shipper名
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP ")
        sqlStat.AppendLine("    ON SP.CUSTOMERCODE   = OB.SHIPPER")
        sqlStat.AppendLine("   AND SP.STYMD         <= @STYMD")
        sqlStat.AppendLine("   AND SP.ENDYMD        >= @ENDYMD")
        sqlStat.AppendLine("   AND SP.DELFLG        <> @DELFLG")

        'Consignee名
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CN ")
        sqlStat.AppendLine("    ON CN.CUSTOMERCODE   = OB.CONSIGNEE")
        sqlStat.AppendLine("   AND CN.STYMD         <= @STYMD")
        sqlStat.AppendLine("   AND CN.ENDYMD        >= @ENDYMD")
        sqlStat.AppendLine("   AND CN.DELFLG        <> @DELFLG")

        'Product名
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD ")
        sqlStat.AppendLine("    ON PD.PRODUCTCODE   = OB.PRODUCTCODE")
        sqlStat.AppendLine("   AND PD.STYMD        <= @STYMD")
        sqlStat.AppendLine("   AND PD.ENDYMD       >= @ENDYMD")
        sqlStat.AppendLine("   AND PD.DELFLG       <> @DELFLG")

        '直近３積載品
        sqlStat.AppendLine("  LEFT JOIN WITH_P3HIST P3HIST")
        sqlStat.AppendLine("    ON P3HIST.TANKNO = OV.TANKNO")
        'ETD予定日最大値
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT ORDERNO, TANKNO, MAX(SCHEDELDATE) As SCHEDELDATE ")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE ")
        sqlStat.AppendLine("  WHERE STYMD       <= @STYMD ")
        sqlStat.AppendLine("    AND ENDYMD      >= @ENDYMD ")
        sqlStat.AppendLine("    AND DELFLG      <> @DELFLG ")
        sqlStat.AppendLine("    AND DATEFIELD   IN ('ETD','ETD1','ETD2') ")
        sqlStat.AppendLine("  GROUP BY ORDERNO, TANKNO ) As OVMX ")
        sqlStat.AppendLine("    ON  OVMX.ORDERNO = OV.ORDERNO ")
        sqlStat.AppendLine("   AND  OVMX.TANKNO  = OV.TANKNO ")

        sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
        sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
        sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")
        sqlStat.AppendLine("   AND AH.STATUS        <> '" & C_APP_STATUS.COMPLETE & "'")

        If Me.hdnStYMD.Value <> "" AndAlso Me.hdnEndYMD.Value <> "" Then
            sqlStat.AppendLine("   AND (convert(nvarchar, AH.APPLYDATE , 111)  BETWEEN  @APPLYDATEFROM  AND  @APPLYDATETO )")
        End If

        'If Me.hdnEndYMD.Value <> "" Then
        '    'VALIDITY FROM
        '    sqlStat.AppendLine("   AND AH.APPLYDATE   <= @APPLYDATEFROM")
        'End If

        'If Me.hdnStYMD.Value <> "" Then
        '    'VALIDITY TO
        '    sqlStat.AppendLine("   AND AH.APPLYDATE   >= @APPLYDATETO")
        'End If

        If Me.hdnOrderNo.Value <> "" Then
            sqlStat.AppendLine("   AND OV.ORDERNO      = @ORDERNO")
        End If

        If Me.hdnTankNo.Value <> "" Then
            sqlStat.AppendLine("   AND OV.TANKNO      = @TANKNO")
        End If

        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)
            Dim paramUserID As SqlParameter = sqlCmd.Parameters.Add("@USERID", SqlDbType.NVarChar)
            Dim paramLangDisp As SqlParameter = sqlCmd.Parameters.Add("@LANGDISP", SqlDbType.NVarChar)
            Dim paramStYMD As SqlParameter = sqlCmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
            Dim paramEndYMD As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
            Dim paramEventCode As SqlParameter = sqlCmd.Parameters.Add("@EVENTCODE", SqlDbType.NVarChar)
            Dim paramInitDate As SqlParameter = sqlCmd.Parameters.Add("@INITDATE", SqlDbType.DateTime)

            'SQLパラメータ(動的変化あり)
            Dim paramOrderNo As SqlParameter = Nothing
            Dim paramTankNo As SqlParameter = Nothing
            Dim paramApplyDateFrom As SqlParameter = Nothing
            Dim paramApplyDateTo As SqlParameter = Nothing
            Dim paramShipper As SqlParameter = Nothing
            Dim paramConsignee As SqlParameter = Nothing
            Dim paramPort As SqlParameter = Nothing
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramDelFlg.Value = CONST_FLAG_YES
            paramUserID.Value = COA0019Session.USERID
            paramLangDisp.Value = COA0019Session.LANGDISP
            paramStYMD.Value = Date.Now
            paramEndYMD.Value = Date.Now
            paramEventCode.Value = C_TKAEVENT.APPLY
            paramInitDate.Value = "1900/01/01"

            If Me.hdnStYMD.Value <> "" AndAlso Me.hdnEndYMD.Value <> "" Then

                'APPLYDATE FROM
                paramApplyDateFrom = sqlCmd.Parameters.Add("@APPLYDATEFROM", SqlDbType.Date)
                paramApplyDateFrom.Value = Me.hdnStYMD.Value
                'APPLYDATE TO
                paramApplyDateTo = sqlCmd.Parameters.Add("@APPLYDATETO", SqlDbType.Date)
                paramApplyDateTo.Value = Me.hdnEndYMD.Value

            End If

            'If Me.hdnEndYMD.Value <> "" Then '検索条件のTOをFROMと突き合わせ
            '    'APPLYDATE FROM
            '    paramApplyDateFrom = sqlCmd.Parameters.Add("@APPLYDATEFROM", SqlDbType.Date)
            '    paramApplyDateFrom.Value = Me.hdnEndYMD.Value
            'End If

            'If Me.hdnStYMD.Value <> "" Then '検索条件のFROMをTOと突き合わせ
            '    'APPLYDATE TO
            '    paramApplyDateTo = sqlCmd.Parameters.Add("@APPLYDATETO", SqlDbType.Date)
            '    paramApplyDateTo.Value = Me.hdnStYMD.Value
            'End If

            If Me.hdnOrderNo.Value <> "" Then
                paramOrderNo = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar)
                paramOrderNo.Value = Me.hdnOrderNo.Value
            End If

            If Me.hdnTankNo.Value <> "" Then
                paramTankNo = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar)
                paramTankNo.Value = Me.hdnTankNo.Value
            End If

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return retDt
    End Function
    ''' <summary>
    ''' タンクURL取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetTankUrl() As String
        Dim mstUrl As String = ""
        '■■■ 画面遷移先URL取得 ■■■]
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = "GB_ShowTankDetail"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return ""
        End If
        HttpContext.Current.Session("MAPmapid") = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "Default"
        '画面遷移実行
        mstUrl = COA0012DoUrl.URL
        mstUrl = VirtualPathUtility.ToAbsolute(mstUrl) 'チルダURLから相対URLに変換
        Dim brUriObj As New Uri(Request.Url, mstUrl) 'アプリルートURL+相対URL
        Return brUriObj.AbsoluteUri 'フルURLを返却(相対URLだとCHROMEではワークしない)
    End Function
    ''' <summary>
    ''' 一覧 マウスホイール時処理 (一覧スクロール)
    ''' </summary>
    Protected Sub ListScrole()
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
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

        Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
        Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
        'Dim checkedValue As Boolean
        For Each dr As DataRow In listData.Rows
            Dim chkId As String = "chkWF_LISTAREACHECK" & Convert.ToString(dr.Item("LINECNT"))
            Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
            If Not (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.APPLYING OrElse Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.REVISE) Then
                chk.Enabled = False
            Else
                chk.Enabled = True
            End If
        Next

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.APPLYチェックがついているチェックボックスオブジェクトをチェック状態にする
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            Dim targetCheckBoxLineCnt = (From dr As DataRow In listData
                                         Where Convert.ToString(dr.Item("CHECK")) <> ""
                                         Select Convert.ToInt32(dr.Item("LINECNT")))
            For Each lineCnt As Integer In targetCheckBoxLineCnt
                Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & "CHECK" & lineCnt.ToString
                Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                If tmpObj IsNot Nothing Then
                    Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                    chkObj.Checked = True
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
        retDt.Columns.Add("LINECNT", GetType(Integer))              'DBの固定フィールド
        retDt.Columns.Add("OPERATION", GetType(String))             'DBの固定フィールド
        retDt.Columns.Add("TIMSTP", GetType(String))                'DBの固定フィールド
        retDt.Columns.Add("SELECT", GetType(Integer))               'DBの固定フィールド
        retDt.Columns.Add("HIDDEN", GetType(Integer))
        '個別項目
        retDt.Columns.Add("APPLYID", GetType(String))
        retDt.Columns.Add("ORDERNO", GetType(String))
        retDt.Columns.Add("LASTSTEP", GetType(String))
        retDt.Columns.Add("TANKNO", GetType(String))
        retDt.Columns.Add("TANKSEQ", GetType(String))
        retDt.Columns.Add("REPAIRSTAT", GetType(String))
        retDt.Columns.Add("REPAIRDATE", GetType(String))
        retDt.Columns.Add("INSPECTDATE5", GetType(String))
        retDt.Columns.Add("INSPECTDATE2P5", GetType(String))
        retDt.Columns.Add("NEXTINSPECTTYPE", GetType(String))
        retDt.Columns.Add("NEXTINSPECTDATE", GetType(String))
        retDt.Columns.Add("POL1SCHEDELDATE", GetType(String))
        retDt.Columns.Add("POD1SCHEDELDATE", GetType(String))
        retDt.Columns.Add("POL2SCHEDELDATE", GetType(String))
        retDt.Columns.Add("POD2SCHEDELDATE", GetType(String))
        retDt.Columns.Add("LOADING", GetType(String))
        retDt.Columns.Add("STEAMING", GetType(String))
        retDt.Columns.Add("TIP", GetType(String))
        retDt.Columns.Add("EXTRA", GetType(String))
        retDt.Columns.Add("LOADPORT1", GetType(String))
        retDt.Columns.Add("LOADPORT2", GetType(String))
        retDt.Columns.Add("DELIVERYPORT1", GetType(String))
        retDt.Columns.Add("DELIVERYPORT2", GetType(String))
        retDt.Columns.Add("PURODUCT", GetType(String))
        retDt.Columns.Add("PD_HIST1", GetType(String))
        retDt.Columns.Add("PD_HIST2", GetType(String))
        retDt.Columns.Add("PD_HIST3", GetType(String))
        retDt.Columns.Add("SHIPPER", GetType(String))
        retDt.Columns.Add("CONSIGNEE", GetType(String))
        retDt.Columns.Add("LOADDATE", GetType(String))
        retDt.Columns.Add("APPROVALOBJECT ", GetType(String))
        retDt.Columns.Add("APPROVALORREJECT", GetType(String))
        retDt.Columns.Add("APPROVEDTEXT", GetType(String))
        retDt.Columns.Add("CHECK", GetType(String))
        retDt.Columns.Add("STEP", GetType(String))
        retDt.Columns.Add("STATUS", GetType(String))
        retDt.Columns.Add("STEPSTATE", GetType(String))
        retDt.Columns.Add("CURSTEP", GetType(String))
        retDt.Columns.Add("APPROVALTYPE", GetType(String))
        retDt.Columns.Add("APPROVERID", GetType(String))
        retDt.Columns.Add("APPLYDATE", GetType(String))
        retDt.Columns.Add("APPLICANTID", GetType(String))

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
                                        messageParams:=New List(Of String) From {C_MESSAGENO.SYSTEMADM})
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

        Dim selectedRow As DataRow = dt.Rows(rowId)

        Me.hdnSelectTankNo.Value = Convert.ToString(selectedRow.Item("TANKNO"))

        '絞り込み
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = CONST_APP_ALL_JP
            Else
                Me.txtApprovalObj.Text = CONST_APP_ALL_EN
            End If
        End If
        Me.hdnExtractTankNo.Value = Me.txtTankNo.Text
        Me.hdnExtractApp.Value = Me.txtApprovalObj.Text
        Me.hdnExtractApplicant.Value = Me.txtApplicantId.Text

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = "GB_ShowTankDetail"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        HttpContext.Current.Session("MAPmapid") = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "GB_Default"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

        ''JavaScriptにて別タブ表示を実行するフラグを立てる
        'Me.hdnTankViewOpen.Value = "1"

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
        If Me.txtTankNo.Text.Trim <> "" Then
            isFillterOff = False
        End If
        Dim isFillterOffApp As Boolean = True
        If Me.txtApprovalObj.Text.Trim <> "" Then
            isFillterOffApp = False
        End If
        Dim isFillterOffApplicant As Boolean = True
        If Me.txtApplicantId.Text.Trim <> "" Then
            isFillterOffApplicant = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False AndAlso Not (Me.txtTankNo.Text.Trim = "") Then

                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                'If Not Convert.ToString(dr.Item("TANKNO")) = Me.txtTankNo.Text.Trim Then
                If Not Convert.ToString(dr("TANKNO")).Trim.Equals(Me.txtTankNo.Text.Trim) Then
                    dr.Item("HIDDEN") = 1
                End If

            End If

            If isFillterOffApp = False AndAlso Not (Me.txtApprovalObj.Text.Trim = CONST_APP_ALL_JP OrElse Me.txtApprovalObj.Text.Trim = CONST_APP_ALL_EN) Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                'If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Then
                If Not Convert.ToString(dr("APPROVALOBJECT")).Trim.Equals(Me.txtApprovalObj.Text.Trim) Then
                    dr.Item("HIDDEN") = 1
                End If

            End If

            If isFillterOffApplicant = False AndAlso Not (Me.txtApplicantId.Text.Trim = "") Then

                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not Convert.ToString(dr("APPLICANTID")).Trim.Equals(Me.txtApplicantId.Text.Trim) Then
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
    ''' 否認ボタン押下時処理
    ''' </summary>
    Public Sub btnReject_Click()
        Dim COA0004LableMessage As New BASEDLL.COA0004LableMessage    'メッセージ取得
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

        Dim errNo As String = ""
        'CHECKチェックボックスがチェック済の全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("CHECK")) = "on")
        Dim checkedDt As DataTable = Nothing
        If q.Any = True Then
            checkedDt = q.CopyToDataTable
        Else
            checkedDt = dt.Clone
        End If
        For Each checkedDr As DataRow In checkedDt.Rows 'For i As Integer = 0 To dt.Rows.Count - 1

            '否認登録
            COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
            COA0032Apploval.I_APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            COA0032Apploval.I_STEP = Convert.ToString(checkedDr.Item("STEP"))
            COA0032Apploval.COA0032setDenial()
            If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
                'Return
                If errNo = "" Then
                    errNo = COA0032Apploval.O_ERR
                End If
                Continue For
            End If

            '承認コメント更新処理
            UpdateApprovedText(Convert.ToString(HttpContext.Current.Session("APSRVCamp")), Convert.ToString(checkedDr.Item("APPLYID")),
                               Convert.ToString(checkedDr.Item("STEP")), Convert.ToString(checkedDr.Item("APPROVEDTEXT")))


            ' 最終承認の場合メール送信
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = "ODR_Rejected_Tank"
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            GBA00009MailSendSet.APPLYSTEP = Convert.ToString(checkedDr.Item("STEP"))
            GBA00009MailSendSet.ORDERNO = Convert.ToString(checkedDr.Item("ORDERNO"))
            GBA00009MailSendSet.GBA00009setMailToTank()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                'Return
                If errNo = "" Then
                    errNo = GBA00009MailSendSet.ERR
                End If
                Continue For
            End If

        Next

        '絞り込み
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = CONST_APP_ALL_JP
            Else
                Me.txtApprovalObj.Text = CONST_APP_ALL_EN
            End If
        End If
        Me.hdnExtractTankNo.Value = Me.txtTankNo.Text
        Me.hdnExtractApp.Value = Me.txtApprovalObj.Text
        Me.hdnExtractApplicant.Value = Me.txtApplicantId.Text

        If errNo <> "" Then
            CommonFunctions.ShowMessage(errNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.REJECTSUCCESS

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        HttpContext.Current.Session("MAPmapid") = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "GB_Default"
        COA0012DoUrl.MAPIDP = Convert.ToString(HttpContext.Current.Session("MAPmapid"))
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

    End Sub
    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()

        If TypeOf Page.PreviousPage Is GBT00019APPROVAL Then

            Dim prevObj As GBT00019APPROVAL = DirectCast(Page.PreviousPage, GBT00019APPROVAL)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnMsgId", Me.hdnMsgId},
                                                                        {"hdnExtractTankNo", Me.hdnExtractTankNo},
                                                                        {"hdnExtractApp", Me.hdnExtractApp},
                                                                        {"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnOrderNo", Me.hdnOrderNo},
                                                                        {"hdnTankNo", Me.hdnTankNo},
                                                                        {"hdnPrevViewID", Me.hdnPrevViewID}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next

        ElseIf TypeOf Page.PreviousPage Is GBT00019SELECT Then

            Dim prevObj As GBT00019SELECT = DirectCast(Page.PreviousPage, GBT00019SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtStYMD", Me.hdnStYMD},
                                                                        {"txtEndYMD", Me.hdnEndYMD},
                                                                        {"txtOrderNo", Me.hdnOrderNo},
                                                                        {"txtTankNo", Me.hdnTankNo}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        If item.Key = "txtStYMD" OrElse item.Key = "txtEndYMD" Then
                            item.Value.Value = FormatDateYMD(tmpHdn.Value, GBA00003UserSetting.DATEFORMAT)
                        Else
                            item.Value.Value = tmpHdn.Value
                        End If
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        If item.Key = "txtStYMD" OrElse item.Key = "txtEndYMD" Then
                            item.Value.Value = FormatDateYMD(tmpTxtObj.Text, GBA00003UserSetting.DATEFORMAT)
                        Else
                            item.Value.Value = tmpTxtObj.Text
                        End If
                    End If
                End If
            Next

            Dim tmplst As ListBox = DirectCast(prevObj.FindControl("lbRightList"), ListBox)
            Dim selectedViewId As String = "Default"
            If tmplst IsNot Nothing Then
                If tmplst.SelectedItem Is Nothing AndAlso
                      tmplst.Items.Count > 0 Then
                    selectedViewId = tmplst.Items(0).Value
                ElseIf tmplst.SelectedItem IsNot Nothing Then
                    selectedViewId = tmplst.SelectedItem.Value
                End If
            End If
            Me.hdnPrevViewID.Value = selectedViewId '前画面より選択した画面レイアウト

        ElseIf TypeOf Page.PreviousPage Is GBM00006TANK Then

            Dim prevObj As GBM00006TANK = DirectCast(Page.PreviousPage, GBM00006TANK)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnMsgId", Me.hdnMsgId},
                                                                        {"hdnExtractTankNo", Me.hdnExtractTankNo},
                                                                        {"hdnExtractApp", Me.hdnExtractApp},
                                                                        {"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnOrderNo", Me.hdnOrderNo},
                                                                        {"hdnTankNo", Me.hdnTankNo},
                                                                        {"hdnPrevViewID", Me.hdnPrevViewID}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next

        End If
    End Sub
    ''' <summary>
    ''' リストアイテムを設定
    ''' </summary>
    Private Sub SetTankNoListItem(selectedValue As String)
        Dim GBA00012TankInfo As New GBA00012TankInfo

        Try

            'リストクリア
            Me.lbTankNo.Items.Clear()

            GBA00012TankInfo.LISTBOX_TANK = Me.lbTankNo
            GBA00012TankInfo.GBA00012getLeftListTank()
            If GBA00012TankInfo.ERR = C_MESSAGENO.NORMAL OrElse GBA00012TankInfo.ERR = C_MESSAGENO.NODATA Then
                Me.lbTankNo = DirectCast(GBA00012TankInfo.LISTBOX_TANK, ListBox)
            Else
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbTankNo.Items.Count > 0 Then
                Dim findListItem = Me.lbTankNo.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try

    End Sub
    ''' <summary>
    ''' 承認リストアイテムを設定
    ''' </summary>
    Private Function SetApprovalObjListItem(selectedValue As String) As String
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Dim retCode As String = C_MESSAGENO.NORMAL

        'リストクリア
        Me.lbApprovalObj.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "APPROVALDISPTYPE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbApprovalObj
        Else
            COA0017FixValue.LISTBOX2 = Me.lbApprovalObj
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbApprovalObj = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbApprovalObj = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If
        Else
            retCode = COA0017FixValue.ERR
        End If
        Return retCode
    End Function
    ''' <summary>
    ''' 承認コメント更新処理
    ''' </summary>
    Private Sub UpdateApprovedText(ByVal parmCompCode As String, ByVal parmApplyId As String, ByVal parmStep As String, ByVal parmApprovedText As String)

        '承認コメント更新
        Dim sqlStat As New StringBuilder
        sqlStat.Clear()
        sqlStat.AppendLine("UPDATE COT0002_APPROVALHIST")
        sqlStat.AppendLine("   SET APPROVEDTEXT = @APPROVEDTEXT")
        sqlStat.AppendLine("      ,UPDYMD       = @UPDYMD ")
        sqlStat.AppendLine("      ,UPDUSER      = @UPDUSER ")
        sqlStat.AppendLine("      ,RECEIVEYMD   = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND APPLYID      = @APPLYID")
        sqlStat.AppendLine("   AND STEP         = @STEP")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
             sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'パラメータ設定
                .Add("@APPROVEDTEXT", SqlDbType.NVarChar).Value = parmApprovedText
                .Add("@UPDYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = parmCompCode
                .Add("@APPLYID", SqlDbType.NVarChar).Value = parmApplyId
                .Add("@STEP", SqlDbType.NVarChar).Value = parmStep
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                sqlCmd.ExecuteNonQuery()

            End With
        End Using
    End Sub
    ''' <summary>
    ''' 作業日更新処理
    ''' </summary>
    Private Sub UpdateActualDate(ByVal parmTankSeq As String, ByVal parmOrder As String)

        'オーダー明細削除
        Dim sqlStat As New StringBuilder

        '作業日更新
        sqlStat.Clear()
        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("              ORDERNO")
        sqlStat.AppendLine("             ,STYMD")
        sqlStat.AppendLine("             ,ENDYMD")
        sqlStat.AppendLine("             ,TANKSEQ")
        sqlStat.AppendLine("             ,DTLPOLPOD")
        sqlStat.AppendLine("             ,DTLOFFICE")
        sqlStat.AppendLine("             ,TANKNO")
        sqlStat.AppendLine("             ,COSTCODE")
        sqlStat.AppendLine("             ,ACTIONID")
        sqlStat.AppendLine("             ,DISPSEQ")
        sqlStat.AppendLine("             ,LASTACT")
        sqlStat.AppendLine("             ,REQUIREDACT")
        sqlStat.AppendLine("             ,ORIGINDESTINATION")
        sqlStat.AppendLine("             ,COUNTRYCODE")
        sqlStat.AppendLine("             ,CURRENCYCODE")
        sqlStat.AppendLine("             ,AMOUNTBR")
        sqlStat.AppendLine("             ,AMOUNTORD")
        sqlStat.AppendLine("             ,AMOUNTFIX")
        sqlStat.AppendLine("             ,CONTRACTORBR")
        sqlStat.AppendLine("             ,CONTRACTORODR")
        sqlStat.AppendLine("             ,CONTRACTORFIX")
        sqlStat.AppendLine("             ,SCHEDELDATEBR")
        sqlStat.AppendLine("             ,SCHEDELDATE")
        sqlStat.AppendLine("             ,ACTUALDATE")
        sqlStat.AppendLine("             ,LOCALBR")
        sqlStat.AppendLine("             ,LOCALRATE")
        sqlStat.AppendLine("             ,TAXBR")
        sqlStat.AppendLine("             ,AMOUNTPAY")
        sqlStat.AppendLine("             ,LOCALPAY")
        sqlStat.AppendLine("             ,TAXPAY")
        sqlStat.AppendLine("             ,INVOICEDBY")
        sqlStat.AppendLine("             ,APPLYID")
        sqlStat.AppendLine("             ,APPLYTEXT")
        sqlStat.AppendLine("             ,LASTSTEP")
        sqlStat.AppendLine("             ,SOAAPPDATE")
        sqlStat.AppendLine("             ,REMARK")
        sqlStat.AppendLine("             ,BRID")
        sqlStat.AppendLine("             ,BRCOST")
        sqlStat.AppendLine("             ,DATEFIELD")
        sqlStat.AppendLine("             ,DATEINTERVAL")
        sqlStat.AppendLine("             ,BRADDEDCOST")
        sqlStat.AppendLine("             ,AGENTORGANIZER")
        sqlStat.AppendLine("             ,DELFLG")
        sqlStat.AppendLine("             ,INITYMD ")
        sqlStat.AppendLine("             ,INITUSER ")
        sqlStat.AppendLine("             ,UPDYMD ")
        sqlStat.AppendLine("             ,UPDUSER ")
        sqlStat.AppendLine("             ,UPDTERMID ")
        sqlStat.AppendLine("             ,RECEIVEYMD ")
        sqlStat.AppendLine("   ) SELECT ")
        sqlStat.AppendLine("              ORDERNO")
        sqlStat.AppendLine("             ,STYMD")
        sqlStat.AppendLine("             ,ENDYMD")
        sqlStat.AppendLine("             ,TANKSEQ")
        sqlStat.AppendLine("             ,DTLPOLPOD")
        sqlStat.AppendLine("             ,DTLOFFICE")
        sqlStat.AppendLine("             ,TANKNO")
        sqlStat.AppendLine("             ,COSTCODE")
        sqlStat.AppendLine("             ,ACTIONID")
        sqlStat.AppendLine("             ,DISPSEQ")
        sqlStat.AppendLine("             ,LASTACT")
        sqlStat.AppendLine("             ,REQUIREDACT")
        sqlStat.AppendLine("             ,ORIGINDESTINATION")
        sqlStat.AppendLine("             ,COUNTRYCODE")
        sqlStat.AppendLine("             ,CURRENCYCODE")
        sqlStat.AppendLine("             ,AMOUNTBR")
        sqlStat.AppendLine("             ,AMOUNTORD")
        sqlStat.AppendLine("             ,AMOUNTFIX")
        sqlStat.AppendLine("             ,CONTRACTORBR")
        sqlStat.AppendLine("             ,CONTRACTORODR")
        sqlStat.AppendLine("             ,CONTRACTORFIX")
        sqlStat.AppendLine("             ,SCHEDELDATEBR")
        sqlStat.AppendLine("             ,SCHEDELDATE")
        sqlStat.AppendLine("             ,ACTUALDATE")
        sqlStat.AppendLine("             ,LOCALBR")
        sqlStat.AppendLine("             ,LOCALRATE")
        sqlStat.AppendLine("             ,TAXBR")
        sqlStat.AppendLine("             ,AMOUNTPAY")
        sqlStat.AppendLine("             ,LOCALPAY")
        sqlStat.AppendLine("             ,TAXPAY")
        sqlStat.AppendLine("             ,INVOICEDBY")
        sqlStat.AppendLine("             ,APPLYID")
        sqlStat.AppendLine("             ,APPLYTEXT")
        sqlStat.AppendLine("             ,LASTSTEP")
        sqlStat.AppendLine("             ,SOAAPPDATE")
        sqlStat.AppendLine("             ,REMARK")
        sqlStat.AppendLine("             ,BRID")
        sqlStat.AppendLine("             ,BRCOST")
        sqlStat.AppendLine("             ,DATEFIELD")
        sqlStat.AppendLine("             ,DATEINTERVAL")
        sqlStat.AppendLine("             ,BRADDEDCOST")
        sqlStat.AppendLine("             ,AGENTORGANIZER")
        sqlStat.AppendLine("             ,@DELFLG")
        sqlStat.AppendLine("             ,INITYMD ")
        sqlStat.AppendLine("             ,INITUSER ")
        sqlStat.AppendLine("             ,@UPDYMD ")
        sqlStat.AppendLine("             ,@UPDUSER ")
        sqlStat.AppendLine("             ,@UPDTERMID ")
        sqlStat.AppendLine("             ,@RECEIVEYMD ")
        sqlStat.AppendLine("      FROM  GBT0005_ODR_VALUE    ")
        sqlStat.AppendLine("     WHERE ORDERNO  = @ORDERNO   ")
        sqlStat.AppendLine("       AND TANKSEQ  = @TANKSEQ ")
        sqlStat.AppendLine("       AND ACTIONID  IN ('TKAL','TAED','TAEC')")
        sqlStat.AppendLine("       AND ACTUALDATE = '1900/01/01'")
        sqlStat.AppendLine("       AND DELFLG   <> @DELFLG")

        sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("   SET  ")
        sqlStat.AppendLine("       UPDYMD     = @UPDYMD ")
        sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER ")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE ORDERNO    = @ORDERNO ")
        sqlStat.AppendLine("   AND TANKSEQ    = @TANKSEQ ")
        sqlStat.AppendLine("   AND ACTIONID  IN ('TKAL','TAED','TAEC')")
        sqlStat.AppendLine("   AND ACTUALDATE = '1900/01/01'")
        sqlStat.AppendLine("   AND DELFLG    <> @DELFLG")
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
            sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'パラメータ設定
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@INITYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = parmOrder
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = parmTankSeq
                sqlCmd.ExecuteNonQuery()

            End With
        End Using

        'オーダー明細
        sqlStat.Clear()
        sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("   SET ACTUALDATE = @ACTUALDATE ")
        sqlStat.AppendLine(" WHERE ORDERNO    = @ORDERNO ")
        sqlStat.AppendLine("   AND TANKSEQ    = @TANKSEQ ")
        sqlStat.AppendLine("   AND ACTIONID  IN ('TKAL','TAED','TAEC')")
        sqlStat.AppendLine("   AND ACTUALDATE = '1900/01/01'")
        sqlStat.AppendLine("   AND DELFLG    <> @DELFLG ")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
             sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'パラメータ設定
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ACTUALDATE", SqlDbType.Date).Value = Date.Now
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = parmOrder
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = parmTankSeq
                sqlCmd.ExecuteNonQuery()
            End With

        End Using
    End Sub

    ''' <summary>
    ''' 最新のDATAID取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDATAID(ByVal applyId As String, Optional ByRef sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim dataID As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT  DATAID ")
            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
            sqlStat.AppendLine(" WHERE APPLYID   = @APPLYID")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Order Value error")
                    End If

                    dataID = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return dataID
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Close()
                sqlCon.Dispose()
                sqlCon = Nothing
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
        Dim dt As DataTable = CreateDataTable()

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            Return C_MESSAGENO.SYSTEMADM
        End If

        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If

        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String) From {{"CHECK", objChkPrifix}}

        Dim formToPost = New NameValueCollection(Request.Form)
        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    formToPost.Remove(dispObjId)
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
    ''' 画面表示のテーブルを制御する
    ''' </summary>
    Private Sub DisplayListObjEdit()
        '引当不可にするレコードを色付けする判定
        Dim dicTestTypeMinusMonth As New Dictionary(Of String, Integer) From {{"2.5", -1}, {"5", -3}}

        Dim targetPanel As Panel = Me.WF_LISTAREA

        Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
        If rightDataDiv.HasControls = False _
           OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
           OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
            Return
        End If
        Dim rightHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HR"), Panel)
        Dim rightHeaderTable As Table = DirectCast(rightHeaderDiv.Controls(0), Table)
        Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"REPAIRSTAT", ""},
                                                                         {"NEXTINSPECTTYPE", ""},
                                                                         {"NEXTINSPECTDATE", ""},
                                                                         {"MAXETD", ""}}
        With Nothing '右ヘッダーの列名より対象の列番号を取得
            Dim headerTableRow = rightHeaderTable.Rows(0)
            If headerTableRow.Cells.Count = 0 Then
                Return 'ヘッダー列に列が存在しない場合は終了
            End If
            'セル名称より列番号を取得
            Dim maxCellIndex = headerTableRow.Cells.Count - 1
            For cellIndex = 0 To maxCellIndex
                Dim targetCell As TableCell = headerTableRow.Cells(cellIndex)
                If targetCell.Attributes("cellfiedlname") IsNot Nothing AndAlso
               dicColumnNameToNo.ContainsKey(targetCell.Attributes("cellfiedlname")) Then
                    dicColumnNameToNo(targetCell.Attributes("cellfiedlname")) = cellIndex.ToString
                End If
            Next
        End With '列番号取得完了

        Dim leftHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HL"), Panel)
        Dim leftHeaderTable As Table = DirectCast(leftHeaderDiv.Controls(0), Table)
        Dim dicLeftColumnNameToNo As New Dictionary(Of String, String) From {{"TANKNO", ""}}

        With Nothing '右ヘッダーの列名より対象の列番号を取得
            Dim headerTableRow = leftHeaderTable.Rows(0)
            If headerTableRow.Cells.Count = 0 Then
                Return 'ヘッダー列に列が存在しない場合は終了
            End If
            'セル名称より列番号を取得
            Dim maxCellIndex = headerTableRow.Cells.Count - 1
            For cellIndex = 0 To maxCellIndex
                Dim targetCell As TableCell = headerTableRow.Cells(cellIndex)
                If targetCell.Attributes("cellfiedlname") IsNot Nothing AndAlso
               dicLeftColumnNameToNo.ContainsKey(targetCell.Attributes("cellfiedlname")) Then
                    dicLeftColumnNameToNo(targetCell.Attributes("cellfiedlname")) = cellIndex.ToString
                End If
            Next
        End With '列番号取得完了

        Dim rightDataTable As Table = DirectCast(rightDataDiv.Controls(0), Table)
        Dim leftDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DL"), Panel)
        Dim leftDataTable As Table = DirectCast(leftDataDiv.Controls(0), Table) '1列目LINECNT 、3列目のSHOW DELETEカラム取得用
        '******************************
        'レンダリング行のループ
        '******************************
        '点検日付格納
        Dim targetDate As Date = Now
        If Date.TryParse(dicColumnNameToNo("MAXETD"), targetDate) = True Then
            targetDate = Date.Parse(dicColumnNameToNo("MAXETD"))
        Else
            targetDate = Date.Now
        End If
        Dim repairAttr As String = "data-repair"
        Dim inspectionSoonAttr As String = "data-inspectionsoon"
        Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        For i = 0 To rowCnt
            Dim tbrRight As TableRow = rightDataTable.Rows(i)
            Dim tbrLeft As TableRow = leftDataTable.Rows(i)

            Dim lineCnt As String = tbrLeft.Cells(0).Text

            ''各行の編集ボタンを加工
            'If dicLeftColumnNameToNo("EDIT") <> "" AndAlso
            '   dicLeftColumnNameToNo("TANKNO") <> "" Then
            '    Dim tankNo As String = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO"))).Text
            '    With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("EDIT")))
            '        If .HasControls = True AndAlso TypeOf .Controls(0) Is HtmlButton Then
            '            Dim tmpBtn As HtmlButton = DirectCast(.Controls(0), HtmlButton)
            '            Dim tmpInpBtn As New HtmlInputButton("button") With {.ViewStateMode = ViewStateMode.Disabled,
            '                                                                 .ID = tmpBtn.ID, .Name = tmpBtn.ID,
            '                                                                 .Value = "EDIT"}
            '            tmpInpBtn.Attributes.Add("onclick", String.Format("showTankMaster('{0}'); return false;", tankNo))
            '            .Controls.Clear()
            '            .Controls.Add(tmpInpBtn)

            '        End If
            '    End With
            'End If
            'リペアステータス判定
            If dicColumnNameToNo("REPAIRSTAT") <> "" AndAlso
               dicLeftColumnNameToNo("TANKNO") <> "" Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("REPAIRSTAT")))
                    If .Text.Trim <> CONST_FLAG_NO Then
                        'リペアステータスが0の場合修理中の為、行に属性を追加
                        tbrRight.Attributes.Add(repairAttr, "1")
                        tbrLeft.Attributes.Add(repairAttr, "1")
                        With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO")))
                            .Attributes.Add(repairAttr, "1")
                        End With
                    End If
                End With
            End If
            '定期点検日付チェック
            Dim nextDate As Date
            If dicColumnNameToNo("NEXTINSPECTTYPE") <> "" AndAlso
               dicColumnNameToNo("NEXTINSPECTDATE") <> "" AndAlso
               dicLeftColumnNameToNo("TANKNO") <> "" AndAlso
               Date.TryParse(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("NEXTINSPECTDATE"))).Text, nextDate) = True AndAlso
               dicTestTypeMinusMonth.ContainsKey(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("NEXTINSPECTTYPE"))).Text.Trim) Then

                nextDate = Date.Parse(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("NEXTINSPECTDATE"))).Text)
                Dim appendMonth As Integer = dicTestTypeMinusMonth(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("NEXTINSPECTTYPE"))).Text.Trim)
                Dim checkDate As Date = nextDate.AddMonths(appendMonth)

                If checkDate <= targetDate Then
                    tbrRight.Attributes.Add(inspectionSoonAttr, "1")
                    tbrLeft.Attributes.Add(inspectionSoonAttr, "1")
                    With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO")))
                        .Attributes.Add(inspectionSoonAttr, "1")
                    End With
                End If
            End If

        Next 'END ROWCOUNT
    End Sub
End Class

