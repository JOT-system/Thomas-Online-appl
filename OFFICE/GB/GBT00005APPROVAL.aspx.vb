Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' ブレーカー承認画面クラス
''' </summary>
Public Class GBT00005APPROVAL
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00005A"   '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分

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
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '初回絞り込み設定
                '****************************************
                If Me.hdnExtract.Value = "" Then
                    If COA0019Session.LANGDISP = C_LANG.JA Then
                        Me.txtApprovalObj.Text = "承認者"
                    Else
                        Me.txtApprovalObj.Text = "Approver"
                    End If
                Else
                    Me.txtApprovalObj.Text = Me.hdnExtract.Value
                End If
                Me.txtApplicantObj.Text = Me.hdnExtractApplicant.Value
                Me.txtBreakerIdObj.Text = Me.hdnExtractBreakerId.Value

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
                        'If Not (Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All") Then
                        '    '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                        '    If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Then
                        '        dr.Item("HIDDEN") = 1
                        '    Else
                        '        dr.Item("HIDDEN") = 0
                        '    End If
                        'Else
                        '    dr.Item("HIDDEN") = 0
                        'End If
                        dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
                        'フィルタ使用時の場合
                        '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                        If Not ((Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All" OrElse
                            Me.txtApprovalObj.Text.Trim = "" OrElse Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim) _
                            AndAlso (Me.txtApplicantObj.Text.Trim = "" OrElse Convert.ToString(dr("APPLICANTID")).Trim.Equals(Me.txtApplicantObj.Text.Trim)) _
                            AndAlso (Me.txtBreakerIdObj.Text.Trim = "" OrElse Convert.ToString(dr("BRID")).Trim.Equals(Me.txtBreakerIdObj.Text.Trim))
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
                    Dim divDrContL As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DL")
                    Dim tblContL As Table = DirectCast(divDrContL.Controls(0), Table)
                    Dim checkedValue As Boolean
                    For i As Integer = 0 To listData.Rows.Count - 1
                        If Convert.ToString(listData.Rows(i).Item("CHECK")) = "on" Then
                            checkedValue = True
                        Else
                            checkedValue = False
                        End If
                        Dim chkId As String = "chkWF_LISTAREACHECK" & Convert.ToString(listData.Rows(i).Item("LINECNT"))
                        Dim chk As CheckBox = DirectCast(tblContL.FindControl(chkId), CheckBox)
                        If chk IsNot Nothing Then
                            chk.Checked = checkedValue
                        End If

                        If Not {C_APP_STATUS.APPLYING, C_APP_STATUS.REVISE}.Contains(Trim(Convert.ToString(listData.Rows(i).Item("STATUS")))) Then
                            chk.Enabled = False
                        Else
                            chk.Enabled = True
                        End If

                        Dim celCls As String = ""
                        If CDec(listData.Rows(i).Item("AMTDISCOUNT")) < 0 Then
                            celCls = "minusDiscount"
                        Else
                            celCls = ""
                        End If
                        tblCont.Rows(i).CssClass = celCls
                        tblContL.Rows(i).CssClass = celCls

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
                        Me.hdnCalendarValue.Value = FormatDateYMD(txtobj.Text, GBA00003UserSetting.DATEFORMAT)

                        Me.mvLeft.Focus()
                    End If
                    '承認ビュー表示切替
                Case Me.vLeftApprovalObj.ID
                    SetApprovalObjListItem(Me.txtApprovalObj.Text)
            End Select
        End If

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID
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
        For Each checkedDr As DataRow In checkedDt.Rows ' For i As Integer = 0 To dt.Rows.Count - 1

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

            ' 最終承認の場合メール送信
            If Convert.ToString(checkedDr.Item("LASTSTEP")) = Convert.ToString(checkedDr.Item("STEP")) Then
                'メール
                Dim GBA00009MailSendSet As New GBA00009MailSendSet
                GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                GBA00009MailSendSet.EVENTCODE = "BRS_Approved"
                'GBA00009MailSendSet.MAILSUBCODE = subCode
                GBA00009MailSendSet.MAILSUBCODE = ""
                GBA00009MailSendSet.BRID = Convert.ToString(checkedDr.Item("BRID"))
                GBA00009MailSendSet.BRSUBID = Convert.ToString(checkedDr.Item("SUBID"))
                GBA00009MailSendSet.BRBASEID = Convert.ToString(checkedDr.Item("BRBASEID"))
                GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
                GBA00009MailSendSet.LASTSTEP = Convert.ToString(checkedDr.Item("LASTSTEP"))
                GBA00009MailSendSet.GBA00009setMailToBR()
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

        If errNo <> "" Then
            CommonFunctions.ShowMessage(errNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '絞り込み
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = "全て"
            Else
                Me.txtApprovalObj.Text = "All"
            End If
        End If

        Me.hdnExtract.Value = Me.txtApprovalObj.Text
        Me.hdnExtractApplicant.Value = Me.txtApplicantObj.Text
        Me.hdnExtractBreakerId.Value = Me.txtBreakerIdObj.Text

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
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblApprovalObjLabel, "種別", "Type")
        AddLangSetting(dicDisplayText, Me.lblApplicantLabel, "申請者", "Applicant")
        AddLangSetting(dicDisplayText, Me.lblBreakerIdLabel, "ブレーカーＩＤ", "Breaker ID")

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
        Dim textProductTblField As String = "PRODUCTNAME"
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
        '承認情報取得
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(BS.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,BS.BRID")
        sqlStat.AppendLine("      ,BS.BRBASEID")
        sqlStat.AppendLine("      ,convert(nvarchar, BS.STYMD , 111) as STYMD")
        sqlStat.AppendLine("      ,convert(nvarchar, BS.ENDYMD , 111) as ENDYMD")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYFROM WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYFROM,'yyyy/MM/dd') END AS VALIDITYFROM")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYTO,'yyyy/MM/dd')   END AS VALIDITYTO")
        sqlStat.AppendLine("      ,BI.BRTYPE")
        sqlStat.AppendLine("      ,BS.TERMTYPE")
        sqlStat.AppendLine("      ,BS.NOOFTANKS")
        sqlStat.AppendFormat("      ,ISNULL(SP.{0},'') AS SHIPPER", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CN.{0},'') AS CONSIGNEE", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,BS.CARRIER1")
        sqlStat.AppendLine("      ,BS.CARRIER2")
        sqlStat.AppendFormat("      ,ISNULL(PD.{0},'') AS PRODUCTCODE", textProductTblField).AppendLine()
        sqlStat.AppendLine("      ,BS.PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,BS.RECIEPTCOUNTRY1")
        sqlStat.AppendLine("      ,BS.RECIEPTPORT1")
        sqlStat.AppendLine("      ,BS.RECIEPTCOUNTRY2")
        sqlStat.AppendLine("      ,BS.RECIEPTPORT2")
        sqlStat.AppendLine("      ,BS.LOADCOUNTRY1")
        sqlStat.AppendLine("      ,BS.LOADPORT1 AS POL")
        sqlStat.AppendLine("      ,ISNULL(PTL.AREANAME,'')  AS POLNAME")
        sqlStat.AppendLine("      ,BS.LOADCOUNTRY2")
        sqlStat.AppendLine("      ,BS.LOADPORT2")
        sqlStat.AppendLine("      ,BS.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("      ,BS.DISCHARGEPORT1 AS POD")
        sqlStat.AppendLine("      ,ISNULL(PTD.AREANAME,'')  AS PODNAME")
        sqlStat.AppendLine("      ,BS.DISCHARGECOUNTRY2")
        sqlStat.AppendLine("      ,BS.DISCHARGEPORT2")
        sqlStat.AppendLine("      ,BS.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,BS.DELIVERYPORT1")
        sqlStat.AppendLine("      ,BS.DELIVERYCOUNTRY2")
        sqlStat.AppendLine("      ,BS.DELIVERYPORT2")
        sqlStat.AppendLine("      ,BS.VSL1")
        sqlStat.AppendLine("      ,BS.VOY1")
        sqlStat.AppendLine("      ,BS.ETD1")
        sqlStat.AppendLine("      ,BS.ETA1")
        sqlStat.AppendLine("      ,BS.VSL2")
        sqlStat.AppendLine("      ,BS.VOY2")
        sqlStat.AppendLine("      ,BS.ETD2")
        sqlStat.AppendLine("      ,BS.ETA2")
        sqlStat.AppendLine("      ,BS.LOADING")
        sqlStat.AppendLine("      ,BS.STEAMING")
        sqlStat.AppendLine("      ,BS.TIP")
        sqlStat.AppendLine("      ,BS.EXTRA")
        sqlStat.AppendLine("      ,BS.JOTHIREAGE")
        sqlStat.AppendLine("      ,BS.COMMERCIALFACTOR")
        sqlStat.AppendLine("      ,BS.AMTREQUEST")
        sqlStat.AppendLine("      ,BS.AMTPRINCIPAL")
        sqlStat.AppendLine("      ,BS.AMTDISCOUNT")
        sqlStat.AppendLine("      ,BS.DEMURTO")
        sqlStat.AppendLine("      ,BS.DEMURUSRATE1")
        sqlStat.AppendLine("      ,BS.DEMURUSRATE2")
        sqlStat.AppendLine("      ,BS.AGENTORGANIZER")
        sqlStat.AppendLine("      ,BS.AGENTPOL1")
        sqlStat.AppendLine("      ,BS.AGENTPOL2")
        sqlStat.AppendLine("      ,BS.AGENTPOD1")
        sqlStat.AppendLine("      ,BS.AGENTPOD2")
        sqlStat.AppendLine("      ,CASE WHEN BS.APPLYTEXT<>'' THEN '〇' ELSE '' END AS HASREMARK")
        sqlStat.AppendLine("      ,BS.DELFLG")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN CASE WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN ISNULL(FV1.VALUE1,'') + '+' ")
        sqlStat.AppendLine("                                            ELSE CASE WHEN AH.STEP > ISNULL(AH2.STEP,'" & C_APP_FIRSTSTEP & "') THEN ISNULL(FV1.VALUE1,'') + '*' ")
        sqlStat.AppendLine("                                            ELSE ISNULL(FV1.VALUE1,'') END END ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN CASE WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN ISNULL(FV1.VALUE2,'') + '+'  ")
        sqlStat.AppendLine("                                            ELSE CASE WHEN AH.STEP > ISNULL(AH2.STEP,'" & C_APP_FIRSTSTEP & "') THEN ISNULL(FV1.VALUE2,'') + '*' ")
        sqlStat.AppendLine("                                            ELSE ISNULL(FV1.VALUE2,'') END END END AS APPROVALOBJECT ")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
        sqlStat.AppendLine("      ,'' AS ""CHECK""")
        sqlStat.AppendLine("      ,AH.APPLYID")
        sqlStat.AppendLine("      ,AH.APPLICANTID")
        sqlStat.AppendLine("      ,AH.STEP")
        sqlStat.AppendLine("      ,AH.STATUS")
        sqlStat.AppendLine("      ,CASE WHEN (AH3.STEP = BI.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
        sqlStat.AppendLine("            WHEN (AH3.STEP = BI.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
        sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH3.STEP,'00'))))) + '/' + trim(convert(char,convert(int,BI.LASTSTEP))) END as STEPSTATE")
        'sqlStat.AppendLine("      ,convert(char,(convert(int,isnull(AH2.STEP,'01')) - 1)) + '/' + convert(char,convert(int,BI.LASTSTEP)) as STEPSTATE")
        sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
        sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
        sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
        sqlStat.AppendLine("      ,BI.LASTSTEP")
        sqlStat.AppendLine("      ,AP.APPROVALTYPE")
        'sqlStat.AppendLine("      ,ISNULL(TRIM(AH4.APPROVERID) + '(' + ISNULL(AH4.STEP,'--') + ')','') AS APPROVERID")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(US.STAFFNAMES,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(US.STAFFNAMES_EN,'') END AS APPROVERID")
        sqlStat.AppendLine("      ,TI.TOTALINVOICED AS TOTALINVOICED")
        sqlStat.AppendLine("      ,TI.TOTALCOST AS TOTALCOST")
        sqlStat.AppendLine("      ,BI.SUBID AS SUBID")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
        sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
        sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
        sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
        sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
        sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  INNER JOIN GBT0001_BR_INFO BI") 'ブレーカー(関連付け)
        sqlStat.AppendLine("    ON  BI.APPLYID      = AH.APPLYID")
        'sqlStat.AppendLine("   AND  BI.LASTSTEP     = AH.STEP")
        sqlStat.AppendLine("   AND  BI.TYPE         = 'INFO'")
        sqlStat.AppendLine("   AND  BI.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  BI.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  BI.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  INNER JOIN GBT0002_BR_BASE BS") 'ブレーカー(基本)
        sqlStat.AppendLine("    ON  BS.BRID         = BI.BRID")
        sqlStat.AppendLine("   AND  BS.BRBASEID     = BI.LINKID")
        sqlStat.AppendLine("   AND  BS.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  BS.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  BS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> '" & CONST_FLAG_YES & "' ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
        sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MAX(STEP) AS STEP ")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS  > '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> '" & CONST_FLAG_YES & "' ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH3 ")
        sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH3.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH3.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH4 ")
        sqlStat.AppendLine("    ON AH3.APPLYID = AH4.APPLYID ")
        sqlStat.AppendLine("   AND AH3.STEP    = AH4.STEP ")
        sqlStat.AppendLine("   AND AH4.DELFLG <> '" & CONST_FLAG_YES & "'")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US") 'APPROVER名称用JOIN
        sqlStat.AppendLine("    ON  US.USERID       = AH4.APPROVERID")
        sqlStat.AppendLine("   AND  US.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  US.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  US.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'SHIPPER名称用JOIN
        sqlStat.AppendLine("    ON  SP.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  SP.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  SP.CUSTOMERCODE = BS.SHIPPER")
        sqlStat.AppendLine("   AND  SP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  SP.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  SP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CN") 'CONSIGNEE名称用JOIN
        sqlStat.AppendLine("    ON  CN.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  CN.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  CN.CUSTOMERCODE = BS.CONSIGNEE")
        sqlStat.AppendLine("   AND  CN.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  CN.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  CN.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  CN.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  PD.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PD.PRODUCTCODE  = BS.PRODUCTCODE")
        sqlStat.AppendLine("   AND  PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  PD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD.ENABLED      =  @ENABLED")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTL") 'PORT名称用JOIN
        sqlStat.AppendLine("    ON  PTL.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PTL.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PTL.PORTCODE     = BS.LOADPORT1")
        sqlStat.AppendLine("   AND  PTL.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  PTL.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  PTL.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTD") 'PORT名称用JOIN
        sqlStat.AppendLine("    ON  PTD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PTD.COUNTRYCODE  = BS.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   AND  PTD.PORTCODE     = BS.DISCHARGEPORT1")
        sqlStat.AppendLine("   AND  PTD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  PTD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  PTD.DELFLG      <> @DELFLG")
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
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT BIS.BRID AS BRID, ISNULL(SUM(BVS.USD),0) AS TOTALCOST, ISNULL(SUM(BVS.USD),0) + BSS.JOTHIREAGE + BSS.COMMERCIALFACTOR + ISNULL(BSS.FEE,0) AS TOTALINVOICED")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO AS BIS ")
        sqlStat.AppendLine("  LEFT JOIN GBT0002_BR_BASE AS BSS ")
        sqlStat.AppendLine("    ON  BIS.BRID    = BSS.BRID ")
        sqlStat.AppendLine("   AND  BIS.LINKID  = BSS.BRBASEID ")
        sqlStat.AppendLine("   AND  BSS.DELFLG <> '" & CONST_FLAG_YES & "' ")
        sqlStat.AppendLine("  LEFT JOIN GBT0003_BR_VALUE AS BVS ")
        sqlStat.AppendLine("    ON  BIS.BRID    = BVS.BRID ")
        sqlStat.AppendLine("   AND  BVS.DELFLG <> '" & CONST_FLAG_YES & "'  ")
        sqlStat.AppendLine(" WHERE  BIS.DELFLG <> '" & CONST_FLAG_YES & "'  ")
        sqlStat.AppendLine(" GROUP  BY  BIS.BRID, BSS.JOTHIREAGE, BSS.COMMERCIALFACTOR, BSS.FEE ")
        sqlStat.AppendLine(" HAVING BSS.JOTHIREAGE IS NOT NULL AND BSS.COMMERCIALFACTOR IS NOT NULL ) AS TI ")
        sqlStat.AppendLine("    ON  TI.BRID     = BS.BRID ")
        sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
        sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
        sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
        sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

        If Me.hdnStYMD.Value <> "" AndAlso Me.hdnEndYMD.Value <> "" Then
            sqlStat.AppendLine("   AND (convert(nvarchar, AH.APPLYDATE , 111)  BETWEEN  @APPLYFROM  AND  @APPLYTO )")
        End If

        If Me.hdnShipper.Value <> "" Then
            'SHIPPER
            sqlStat.AppendLine("   AND BS.SHIPPER   = @SHIPPER")
        End If

        If Me.hdnConsignee.Value <> "" Then
            'CONSIGNEE
            sqlStat.AppendLine("   AND BS.CONSIGNEE   = @CONSIGNEE")
        End If

        If Me.hdnPOLPort.Value <> "" Then
            'POLPORT
            sqlStat.AppendLine("   AND BS.LOADPORT1   = @POLPORT")
        End If

        If Me.hdnPODPort.Value <> "" Then
            'PODPORT
            sqlStat.AppendLine("   AND BS.DISCHARGEPORT1   = @PODPORT")
        End If

        If Me.hdnProduct.Value <> "" Then
            '積載品
            sqlStat.AppendLine("   AND BS.PRODUCTCODE   = @PRODUCT")
        End If

        If Me.hdnBrId.Value <> "" Then
            'ブレーカーID
            sqlStat.AppendLine("   AND BS.BRID   = @BREAKERID")
        End If

        If Me.hdnSearchBreakerType.Value <> "" Then
            'ブレーカータイプ
            sqlStat.AppendLine("   AND BI.BRTYPE   = @BREAKERTYPE")
        End If

        If Me.hdnApproval.Value <> "" Then
            'ステータス
            sqlStat.AppendLine("   AND AH.STATUS   = @APPROVAL")
        End If

        'OFFICE
        Dim countryCode As String = ""
        Dim officeCode As String = ""
        If Me.hdnOffice.Value <> "" Then
            officeCode = Me.hdnOffice.Value
            sqlStat.AppendLine("   AND (    BS.AGENTORGANIZER = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOL1      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOL2      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOD1      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOD2      = @OFFICECODE")
            sqlStat.AppendLine("       )")
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
            Dim paramEnabled As SqlParameter = sqlCmd.Parameters.Add("@ENABLED", SqlDbType.NVarChar)
            Dim paramUserID As SqlParameter = sqlCmd.Parameters.Add("@USERID", SqlDbType.NVarChar)
            Dim paramMapID As SqlParameter = sqlCmd.Parameters.Add("@MAPID", SqlDbType.NVarChar)
            Dim paramLangDisp As SqlParameter = sqlCmd.Parameters.Add("@LANGDISP", SqlDbType.NVarChar)
            Dim paramStYMD As SqlParameter = sqlCmd.Parameters.Add("@STYMD", System.Data.SqlDbType.Date)
            Dim paramEndYMD As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", System.Data.SqlDbType.Date)
            Dim paramEventCode As SqlParameter = sqlCmd.Parameters.Add("@EVENTCODE", SqlDbType.NVarChar)

            'SQLパラメータ(動的変化あり)
            Dim paramApplyFrom As SqlParameter = Nothing
            Dim paramApplyTo As SqlParameter = Nothing
            Dim paramShipper As SqlParameter = Nothing
            Dim paramConsignee As SqlParameter = Nothing
            Dim paramPOLPort As SqlParameter = Nothing
            Dim paramPODPort As SqlParameter = Nothing
            Dim paramProduct As SqlParameter = Nothing
            Dim paramBrId As SqlParameter = Nothing
            Dim paramApproval As SqlParameter = Nothing
            Dim paramOffice As SqlParameter = Nothing
            Dim paramBreakerType As SqlParameter = Nothing

            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramDelFlg.Value = CONST_FLAG_YES
            paramEnabled.Value = CONST_FLAG_YES
            paramUserID.Value = COA0019Session.USERID
            paramMapID.Value = "GBT00001"
            paramLangDisp.Value = COA0019Session.LANGDISP
            paramStYMD.Value = Date.Now
            paramEndYMD.Value = Date.Now
            paramEventCode.Value = C_BRSEVENT.APPLY

            If Me.hdnStYMD.Value <> "" AndAlso Me.hdnEndYMD.Value <> "" Then
                paramApplyFrom = sqlCmd.Parameters.Add("@APPLYFROM", SqlDbType.Date)
                paramApplyFrom.Value = Me.hdnStYMD.Value
                paramApplyTo = sqlCmd.Parameters.Add("@APPLYTO", SqlDbType.Date)
                paramApplyTo.Value = Me.hdnEndYMD.Value
            End If

            If Me.hdnShipper.Value <> "" Then
                'SHIPPER
                paramShipper = sqlCmd.Parameters.Add("@SHIPPER", SqlDbType.NVarChar)
                paramShipper.Value = Me.hdnShipper.Value
            End If

            If Me.hdnConsignee.Value <> "" Then
                'CONSIGNEE
                paramConsignee = sqlCmd.Parameters.Add("@CONSIGNEE", SqlDbType.NVarChar)
                paramConsignee.Value = Me.hdnConsignee.Value
            End If

            If Me.hdnPOLPort.Value <> "" Then
                'POLPORT
                paramPOLPort = sqlCmd.Parameters.Add("@POLPORT", SqlDbType.NVarChar)
                paramPOLPort.Value = Me.hdnPOLPort.Value
            End If

            If Me.hdnPODPort.Value <> "" Then
                'POLPORT
                paramPODPort = sqlCmd.Parameters.Add("@PODPORT", SqlDbType.NVarChar)
                paramPODPort.Value = Me.hdnPODPort.Value
            End If

            If Me.hdnProduct.Value <> "" Then
                'PRODUCT
                paramProduct = sqlCmd.Parameters.Add("@PRODUCT", SqlDbType.NVarChar)
                paramProduct.Value = Me.hdnProduct.Value
            End If

            If Me.hdnBrId.Value <> "" Then
                'BRID
                paramBrId = sqlCmd.Parameters.Add("@BREAKERID", SqlDbType.NVarChar)
                paramBrId.Value = Me.hdnBrId.Value
            End If

            If Me.hdnSearchBreakerType.Value <> "" Then
                Dim brType As String = ""
                'ブレーカータイプ
                paramBreakerType = sqlCmd.Parameters.Add("@BREAKERTYPE", SqlDbType.NVarChar)
                Select Case Me.hdnSearchBreakerType.Value
                    Case "01SALES"
                        brType = C_BRTYPE.SALES

                    Case "02OPE"
                        brType = C_BRTYPE.OPERATION

                End Select

                paramBreakerType.Value = brType
            End If

            If Me.hdnApproval.Value <> "" Then
                'Approval
                paramApproval = sqlCmd.Parameters.Add("@APPROVAL", SqlDbType.NVarChar)
                paramApproval.Value = Me.hdnApproval.Value
            End If

            If Me.hdnOffice.Value <> "" Then
                'Office
                paramOffice = sqlCmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar)
                paramOffice.Value = Me.hdnOffice.Value
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
        COA0013TableObject.VARI = Me.hdnPrevViewID.Value
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

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.APPLYチェックがついているチェックボックスオブジェクトをチェック状態にする
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            '申請チェックボックスの加工
            For Each targetCheckBoxId As String In {"CHECK"} '複数チェックボックスを配置している場合は配列に追加


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
            'チェックボックス使用可否制御
            Dim targetDisableCheck = (From dr As DataRow In listData
                                      Where Not {C_APP_STATUS.APPLYING, C_APP_STATUS.REVISE}.Contains(Convert.ToString(dr.Item("STATUS")).Trim)
                                      Select Convert.ToInt32(dr.Item("LINECNT")))
            If targetDisableCheck.Any Then
                For Each lineCnt As Integer In targetDisableCheck
                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & "CHECK" & lineCnt.ToString
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        chkObj.Enabled = False
                    End If
                Next
            End If

            'マイナス金額の強調表示
            Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
            Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
            Dim divDrContL As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DL")
            Dim tblContL As Table = DirectCast(divDrContL.Controls(0), Table)
            Dim minusAmountRows = (From dr As DataRow In listData
                                   Where CDec(dr.Item("AMTDISCOUNT")) < 0)
            If minusAmountRows.Any Then
                For Each minusAmountRow In minusAmountRows
                    Dim rowIndex As Integer = listData.Rows.IndexOf(minusAmountRow)
                    tblCont.Rows(rowIndex).CssClass = "minusDiscount"
                    tblContL.Rows(rowIndex).CssClass = "minusDiscount"
                Next
            End If

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
        retDt.Columns.Add("BRID", GetType(String))                  'ブレーカーID
        retDt.Columns.Add("BRBASEID", GetType(String))              'ブレーカー基本情報ID
        retDt.Columns.Add("STYMD", GetType(String))                 '有効開始日
        retDt.Columns.Add("ENDYMD", GetType(String))                '有効終了日
        retDt.Columns.Add("VALIDITYFROM", GetType(String))          'ブレーカー有効期限FROM
        retDt.Columns.Add("VALIDITYTO", GetType(String))            'ブレーカー有効期限TO
        retDt.Columns.Add("BRTYPE", GetType(String))                'ブレーカータイプ
        retDt.Columns.Add("TERMTYPE", GetType(String))              'ＴＥＲＭタイプ
        retDt.Columns.Add("NOOFTANKS", GetType(String))             'タンク本数
        retDt.Columns.Add("SHIPPER", GetType(String))               '荷主コード
        retDt.Columns.Add("CONSIGNEE", GetType(String))             '荷受人コード
        retDt.Columns.Add("CARRIER1", GetType(String))              '船社コード１
        retDt.Columns.Add("CARRIER2", GetType(String))              '船社コード２
        retDt.Columns.Add("PRODUCTCODE", GetType(String))           '積載品コード
        retDt.Columns.Add("PRODUCTWEIGHT", GetType(String))         '積載品重量
        retDt.Columns.Add("RECIEPTCOUNTRY1", GetType(String))       '荷受国コード1
        retDt.Columns.Add("RECIEPTPORT1", GetType(String))          '荷受港コード1
        retDt.Columns.Add("RECIEPTCOUNTRY2", GetType(String))       '荷受国コード2
        retDt.Columns.Add("RECIEPTPORT2", GetType(String))          '荷受港コード2
        retDt.Columns.Add("LOADCOUNTRY1", GetType(String))          '積荷国コード1 
        retDt.Columns.Add("POL", GetType(String))                   '積荷港コード1
        retDt.Columns.Add("POLNAME", GetType(String))               '積荷港名1
        retDt.Columns.Add("LOADCOUNTRY2", GetType(String))          '積荷国コード2
        retDt.Columns.Add("LOADPORT2", GetType(String))             '積荷港コード2
        retDt.Columns.Add("DISCHARGECOUNTRY1", GetType(String))     '荷揚国コード1
        retDt.Columns.Add("POD", GetType(String))                   '荷揚港コード1
        retDt.Columns.Add("PODNAME", GetType(String))               '荷揚港名1
        retDt.Columns.Add("DISCHARGECOUNTRY2", GetType(String))     '荷揚国コード2
        retDt.Columns.Add("DISCHARGEPORT2", GetType(String))        '荷揚港コード2
        retDt.Columns.Add("DELIVERYCOUNTRY1", GetType(String))      '荷渡国コード1
        retDt.Columns.Add("DELIVERYPORT1", GetType(String))         '荷渡港コード1
        retDt.Columns.Add("DELIVERYCOUNTRY2", GetType(String))      '荷渡国コード2
        retDt.Columns.Add("DELIVERYPORT2", GetType(String))         '荷渡港コード2
        retDt.Columns.Add("VSL1", GetType(String))                  '船名1
        retDt.Columns.Add("VOY1", GetType(String))                  '航海番号1
        retDt.Columns.Add("ETD1", GetType(String))                  '出港日1
        retDt.Columns.Add("ETA1", GetType(String))                  '着港日1
        retDt.Columns.Add("VSL2", GetType(String))                  '船名2
        retDt.Columns.Add("VOY2", GetType(String))                  '航海番号2
        retDt.Columns.Add("ETD2", GetType(String))                  '出港日2
        retDt.Columns.Add("ETA2", GetType(String))                  '着港日2
        retDt.Columns.Add("LOADING", GetType(String))               '発側期間
        retDt.Columns.Add("STEAMING", GetType(String))              '船上期間
        retDt.Columns.Add("TIP", GetType(String))                   '着側期間
        retDt.Columns.Add("EXTRA", GetType(String))                 '追加期間
        retDt.Columns.Add("JOTHIREAGE", GetType(String))            'JOT総額
        retDt.Columns.Add("COMMERCIALFACTOR", GetType(String))      '調整
        retDt.Columns.Add("AMTREQUEST", GetType(String))            '総額変更(要求)
        retDt.Columns.Add("AMTPRINCIPAL", GetType(String))          '総額変更(確認)
        retDt.Columns.Add("AMTDISCOUNT", GetType(String))           '総額変更(差額)
        retDt.Columns.Add("DEMURTO", GetType(String))               '一次期間To
        retDt.Columns.Add("DEMURUSRATE1", GetType(String))          '一次期間単価
        retDt.Columns.Add("DEMURUSRATE2", GetType(String))          '二次期間単価
        retDt.Columns.Add("AGENTORGANIZER", GetType(String))        'オーガナイザーエージェント
        retDt.Columns.Add("AGENTPOL1", GetType(String))             '発１エージェント
        retDt.Columns.Add("AGENTPOL2", GetType(String))             '発２エージェント
        retDt.Columns.Add("AGENTPOD1", GetType(String))             '着１エージェント
        retDt.Columns.Add("AGENTPOD2", GetType(String))             '着２エージェント
        retDt.Columns.Add("HASREMARK", GetType(String))             '備考
        retDt.Columns.Add("DELFLG", GetType(String))                '削除フラグ
        retDt.Columns.Add("APPROVALOBJECT", GetType(String))        '承認対象(通常、代行、SKIP)
        retDt.Columns.Add("APPROVALORREJECT", GetType(String))      '承認or否認
        retDt.Columns.Add("CHECK", GetType(String))                 'チェック
        retDt.Columns.Add("APPLYID", GetType(String))               '申請ID
        retDt.Columns.Add("APPLICANTID", GetType(String))             '申請者
        retDt.Columns.Add("STEP", GetType(String))                  'ステップ
        retDt.Columns.Add("STATUS", GetType(String))                'ステータス
        retDt.Columns.Add("CURSTEP", GetType(String))               '承認ステップ
        retDt.Columns.Add("LASTSTEP", GetType(String))              'ラストステップ
        retDt.Columns.Add("STEPSTATE", GetType(String))             'ステップ状況
        retDt.Columns.Add("APPROVALTYPE", GetType(String))          '承認区分

        retDt.Columns.Add("APPROVERID", GetType(String))
        retDt.Columns.Add("TOTALINVOICED", GetType(String))
        retDt.Columns.Add("TOTALCOST", GetType(String))

        retDt.Columns.Add("SUBID", GetType(String))

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

        Dim selectedRow As DataRow = dt.Rows(rowId)
        Me.hdnSelectedBrId.Value = Convert.ToString(selectedRow.Item("BRID"))
        Me.hdnSelectedStYMD.Value = Convert.ToString(selectedRow.Item("STYMD"))
        Me.hdnSelectedEndYMD.Value = Convert.ToString(selectedRow.Item("ENDYMD"))
        Me.hdnSelectedShipper.Value = Convert.ToString(selectedRow.Item("SHIPPER"))
        Me.hdnSelectedConsignee.Value = Convert.ToString(selectedRow.Item("CONSIGNEE"))
        Me.hdnSelectedPort.Value = Convert.ToString(selectedRow.Item("POL"))
        Me.hdnSelectedApproval.Value = ""
        Me.hdnSelectedOffice.Value = ""
        Me.hdnXMLsaveFileRet.Value = hdnXMLsaveFile.Value
        If Convert.ToString(Trim(Convert.ToString(selectedRow.Item("STATUS")))) = C_APP_STATUS.REVISE Then
            Me.hdnCorrection.Value = "1"
            Me.hdnDenial.Value = ""
        ElseIf Convert.ToString(Trim(Convert.ToString(selectedRow.Item("STATUS")))) = C_APP_STATUS.REJECT Then
            Me.hdnCorrection.Value = ""
            Me.hdnDenial.Value = "1"
        Else
            Me.hdnCorrection.Value = ""
            Me.hdnDenial.Value = ""
        End If
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = "全て"
            Else
                Me.txtApprovalObj.Text = "All"
            End If
        End If
        Me.hdnExtract.Value = txtApprovalObj.Text
        Me.hdnExtractApplicant.Value = txtApplicantObj.Text
        Me.hdnExtractBreakerId.Value = txtBreakerIdObj.Text
        Me.hdnSelectedStep.Value = Convert.ToString(selectedRow.Item("STEP"))
        Me.hdnStatus.Value = Convert.ToString(Trim(Convert.ToString(selectedRow.Item("STATUS"))))

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = "GB_ShowBrDetail"
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
        If Me.txtApprovalObj.Text.Trim <> "" OrElse Me.txtApplicantObj.Text.Trim <> "" OrElse Me.txtBreakerIdObj.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            'If isFillterOff = False AndAlso (Not (Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All") OrElse Me.txtApplicantObj.Text.Trim <> "") Then
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All" OrElse Me.txtApprovalObj.Text.Trim = "" OrElse Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim) _
                    AndAlso (Me.txtApplicantObj.Text.Trim = "" OrElse Convert.ToString(dr("APPLICANTID")).Trim.Equals(Me.txtApplicantObj.Text.Trim)) _
                    AndAlso (Me.txtBreakerIdObj.Text.Trim = "" OrElse Convert.ToString(dr("BRID")).Trim.Equals(Me.txtBreakerIdObj.Text.Trim))
                    ) Then
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
        Me.txtApprovalObj.Focus()

    End Sub
    ''' <summary>
    ''' 否認ボタン押下時処理
    ''' </summary>
    Public Sub btnReject_Click()
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

            'メール
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = "BRS_Rejected"
            'GBA00009MailSendSet.MAILSUBCODE = subCode
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = Convert.ToString(checkedDr.Item("BRID"))
            GBA00009MailSendSet.BRSUBID = Convert.ToString(checkedDr.Item("SUBID"))
            GBA00009MailSendSet.BRBASEID = Convert.ToString(checkedDr.Item("BRBASEID"))
            GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            GBA00009MailSendSet.LASTSTEP = Convert.ToString(checkedDr.Item("LASTSTEP"))
            GBA00009MailSendSet.GBA00009setMailToBR()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                'Return
                If errNo = "" Then
                    errNo = GBA00009MailSendSet.ERR
                End If
                Continue For
            End If

        Next

        If errNo <> "" Then
            CommonFunctions.ShowMessage(errNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '絞り込み
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = "全て"
            Else
                Me.txtApprovalObj.Text = "All"
            End If
        End If
        Me.hdnExtract.Value = Me.txtApprovalObj.Text
        Me.hdnExtractApplicant.Value = Me.txtApplicantObj.Text
        Me.hdnExtractBreakerId.Value = Me.txtBreakerIdObj.Text

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
        If TypeOf Page.PreviousPage Is GBT00001BREAKER Then
            '単票画面の場合
            Dim prevObj As GBT00001BREAKER = DirectCast(Page.PreviousPage, GBT00001BREAKER)

            '前画面と当画面のテキストボックス関連ディクショナリ
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                    {"hdnEndYMD", Me.hdnEndYMD},
                                                                    {"hdnShipper", Me.hdnShipper},
                                                                    {"hdnConsignee", Me.hdnConsignee},
                                                                    {"hdnPOLPort", Me.hdnPOLPort},
                                                                    {"hdnPODPort", Me.hdnPODPort},
                                                                    {"hdnProduct", Me.hdnProduct},
                                                                    {"hdnBrId", Me.hdnBrId},
                                                                    {"hdnApproval", Me.hdnApproval},
                                                                    {"hdnOffice", Me.hdnOffice},
                                                                    {"hdnSearchBreakerType", Me.hdnSearchBreakerType},
                                                                    {"hdnXMLsaveFileRet", Me.hdnXMLsaveFileRet},
                                                                    {"hdnExtract", Me.hdnExtract},
                                                                    {"hdnExtractApplicant", Me.hdnExtractApplicant},
                                                                    {"hdnExtractBreakerId", Me.hdnExtractBreakerId},
                                                                    {"hdnPrevViewID", Me.hdnPrevViewID}
            }

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    If dicObj.Key = "hdnStYMD" OrElse dicObj.Key = "hdnEndYMD" Then
                        dicObj.Value.Value = FormatDateYMD(tmphdnObj.Value, GBA00003UserSetting.DATEFORMAT)
                    Else
                        dicObj.Value.Value = tmphdnObj.Value
                    End If
                End If
            Next

            'Dim tmpXML As Control = prevObj.FindControl("hdnXMLsaveFileRet")

            'If tmpXML IsNot Nothing Then
            '    Dim tmphdn As HiddenField = DirectCast(tmpXML, HiddenField)
            '    Me.hdnXMLsaveFileRet.Value = tmphdn.Value
            'End If

            'Dim tmpExt As Control = prevObj.FindControl("hdnExtract")

            'If tmpExt IsNot Nothing Then
            '    Dim tmphdn As HiddenField = DirectCast(tmpExt, HiddenField)
            '    Me.hdnExtract.Value = tmphdn.Value
            'End If
            '画面ビューID保持
            'Dim tmpPrevViewIDObj As HiddenField = DirectCast(prevObj.FindControl("hdnPrevViewID"), HiddenField)
            'If tmpPrevViewIDObj IsNot Nothing Then
            '    Me.hdnPrevViewID.Value = tmpPrevViewIDObj.Value
            'End If
        ElseIf TypeOf Page.PreviousPage Is GBT00005SELECT Then 'TODO 検索画面（現状メニューより遷移の為固定で設定)
            '検索画面遷移
            Dim prevObj As GBT00005SELECT = DirectCast(Page.PreviousPage, GBT00005SELECT)
            '前画面と当画面のテキストボックス関連ディクショナリ
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtStYMD", Me.hdnStYMD},
                                                                    {"txtEndYMD", Me.hdnEndYMD},
                                                                    {"txtShipper", Me.hdnShipper},
                                                                    {"txtConsignee", Me.hdnConsignee},
                                                                    {"txtPOLPort", Me.hdnPOLPort},
                                                                    {"txtPODPort", Me.hdnPODPort},
                                                                    {"txtProduct", Me.hdnProduct},
                                                                    {"txtBrId", Me.hdnBrId},
                                                                    {"txtApproval", Me.hdnApproval},
                                                                    {"txtOffice", Me.hdnOffice},
                                                                    {"rblBreakerType", Me.hdnSearchBreakerType}}

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is TextBox Then
                        Dim tmpText As TextBox = DirectCast(tmpCont, TextBox)
                        If dicObj.Key = "txtStYMD" OrElse dicObj.Key = "txtEndYMD" Then
                            dicObj.Value.Value = FormatDateYMD(tmpText.Text, GBA00003UserSetting.DATEFORMAT)
                        Else
                            dicObj.Value.Value = tmpText.Text
                        End If
                    ElseIf TypeOf tmpCont Is RadioButtonList Then
                        Dim tmpRbl As RadioButtonList = DirectCast(tmpCont, RadioButtonList)
                        dicObj.Value.Value = tmpRbl.SelectedValue
                    ElseIf TypeOf tmpCont Is ListBox Then
                        Dim tmplist As ListBox = DirectCast(tmpCont, ListBox)
                        dicObj.Value.Value = tmplist.SelectedValue
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
        ElseIf TypeOf Page.PreviousPage Is GBT00005APPROVAL Then

            Dim prevObj As GBT00005APPROVAL = DirectCast(Page.PreviousPage, GBT00005APPROVAL)

            '前画面と当画面のテキストボックス関連ディクショナリ
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                    {"hdnEndYMD", Me.hdnEndYMD},
                                                                    {"hdnShipper", Me.hdnShipper},
                                                                    {"hdnConsignee", Me.hdnConsignee},
                                                                    {"hdnPOLPort", Me.hdnPOLPort},
                                                                    {"hdnPODPort", Me.hdnPODPort},
                                                                    {"hdnProduct", Me.hdnProduct},
                                                                    {"hdnBrId", Me.hdnBrId},
                                                                    {"hdnApproval", Me.hdnApproval},
                                                                    {"hdnOffice", Me.hdnOffice},
                                                                    {"hdnSearchBreakerType", Me.hdnSearchBreakerType},
                                                                    {"hdnMsgId", Me.hdnMsgId},
                                                                    {"hdnExtract", Me.hdnExtract},
                                                                    {"hdnExtractApplicant", Me.hdnExtractApplicant},
                                                                    {"hdnExtractBreakerId", Me.hdnExtractBreakerId},
                                                                    {"hdnPrevViewID", Me.hdnPrevViewID}
                                                                    }

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    If dicObj.Key = "hdnStYMD" OrElse dicObj.Key = "hdnEndYMD" Then
                        dicObj.Value.Value = FormatDateYMD(tmphdnObj.Value, GBA00003UserSetting.DATEFORMAT)
                    Else
                        dicObj.Value.Value = tmphdnObj.Value
                    End If
                End If
            Next

            'Dim tmpMsg As Control = prevObj.FindControl("hdnMsgId")

            'If tmpMsg IsNot Nothing Then
            '    Dim tmphdn As HiddenField = DirectCast(tmpMsg, HiddenField)
            '    Me.hdnMsgId.Value = tmphdn.Value
            'End If

            'Dim tmpExt As Control = prevObj.FindControl("hdnExtract")

            'If tmpExt IsNot Nothing Then
            '    Dim tmphdn As HiddenField = DirectCast(tmpExt, HiddenField)
            '    Me.hdnExtract.Value = tmphdn.Value
            'End If
            '画面ビューID保持
            'Dim tmpPrevViewIDObj As HiddenField = DirectCast(prevObj.FindControl("hdnPrevViewID"), HiddenField)
            'If tmpPrevViewIDObj IsNot Nothing Then
            '    Me.hdnPrevViewID.Value = tmpPrevViewIDObj.Value
            'End If
        End If
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
End Class