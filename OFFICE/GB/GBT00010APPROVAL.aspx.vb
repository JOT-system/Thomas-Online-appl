Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' 費用承認画面クラス
''' </summary>
Public Class GBT00010APPROVAL
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00010A"   '自身のMAPID
    'Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    'Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_DSPROWCOUNT = 99                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 100            'マウススクロール時の増分

    'EventCode
    Private Const CONST_EVENT_ORD = "ODR_ApplyDefault"          'オーダー
    Private Const CONST_EVENT_DEM = "ODR_ApplyGB_Demurrage"     'デマレージ
    Private Const CONST_EVENT_NON = "ODR_ApplyGB_NonBreaker"    'ノンブレーカー
    Private Const CONST_EVENT_SOA = "ODR_ApplyGB_SOA"           'SOA
    Private Const CONST_EVENT_TNK = "ODR_ApplyGB_TankActivity"  'タンク動静
    Private Const CONST_EVENT_COS = "ODR_ApplyGB_CostUp"        'COSTUP
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
                If Me.hdnExtractCost.Value = "" Then
                    Me.txtCostType.Text = "ALL"
                Else
                    Me.txtCostType.Text = Me.hdnExtractCost.Value
                End If
                If Me.hdnExtractApp.Value = "" Then
                    If COA0019Session.LANGDISP = C_LANG.JA Then
                        Me.txtApprovalObj.Text = "承認者"
                    Else
                        Me.txtApprovalObj.Text = "Approver"
                    End If
                Else
                    Me.txtApprovalObj.Text = Me.hdnExtractApp.Value
                End If
                Me.txtOrderId.Text = Me.hdnExtractOrderId.Value
                Me.txtTankNo.Text = Me.hdnExtractTankNo.Value
                Me.txtApplicant.Text = Me.hdnExtractApplicant.Value
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
                        If Not (Me.txtCostType.Text.Trim = "ALL") Then
                            '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                            If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtCostType.Text.Trim Then
                                dr.Item("HIDDEN") = 1
                            Else
                                dr.Item("HIDDEN") = 0
                            End If
                        Else
                            dr.Item("HIDDEN") = 0
                        End If

                        If Convert.ToString(dr.Item("HIDDEN")) = "0" AndAlso Not (Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All") Then
                            '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                            If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Then
                                dr.Item("HIDDEN") = 1
                            Else
                                dr.Item("HIDDEN") = 0
                            End If
                        End If

                        If Convert.ToString(dr.Item("HIDDEN")) = "0" AndAlso
                            Not ((Me.txtOrderId.Text.Trim = "" OrElse Convert.ToString(dr("ORDERNO")).Contains(Me.txtOrderId.Text.Trim)) _
                            AndAlso (Me.txtTankNo.Text.Trim = "" OrElse Convert.ToString(dr("TANKNO")).Contains(Me.txtTankNo.Text.Trim)) _
                            AndAlso (Me.txtApplicant.Text.Trim = "" OrElse Convert.ToString(dr("APPLICANTID")).Contains(Me.txtApplicant.Text.Trim))) Then
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
                        '.SCROLLTYPE = "2"
                        .SCROLLTYPE = "3"
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

                    Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DL")
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
                Me.hdnOrderViewUrl.Value = GetOderUrl()
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
                '費用種別ビュー表示切替
                Case Me.vLeftCostType.ID
                    SetCostTypeListItem(Me.txtCostType.Text)
                '承認ビュー表示切替
                Case Me.vLeftApprovalObj.ID
                    SetApprovalObjListItem(Me.txtApprovalObj.Text)
                    ''オーダーIDビュー表示切替
                    'Case Me.vLeftApprovalObj.ID
                    '    SetOrderIdListItem(Me.txtOrderId.Text)
                    ''タンクNoビュー表示切替
                    'Case Me.vLeftApprovalObj.ID
                    '    SetTankNoListItem(Me.txtTankNo.Text)
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
                'DATAID取得
                Dim dataId As String = ""
                'dataId = GetDATAID(Convert.ToString(checkedDr.Item("APPLYID")))
                Dim tergetDr As DataRow = GetTargetData(Convert.ToString(checkedDr.Item("APPLYID")))
                dataId = Convert.ToString(tergetDr("DATAID"))
                'AMOUNTFIX更新
                'UpdateAmountFix(Convert.ToString(checkedDr.Item("APPLYID")), Convert.ToString(checkedDr.Item("ORDERNO")))
                UpdateAmountFix(dataId, tergetDr)

                ' 最終承認の場合メール送信
                Dim GBA00009MailSendSet As New GBA00009MailSendSet
                GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                GBA00009MailSendSet.MAILSUBCODE = ""
                GBA00009MailSendSet.ODRDATAID = dataId
                GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
                GBA00009MailSendSet.APPLYSTEP = Convert.ToString(checkedDr.Item("STEP"))

                Dim eventCode As String = ""
                Select Case Convert.ToString(checkedDr.Item("EVENTCODE"))
                    Case "DEMURRAGE"
                        GBA00009MailSendSet.EVENTCODE = "ODR_Approved_Demurrage"
                        GBA00009MailSendSet.GBA00009setMailToOdr()

                    Case "NONBREAKER"
                        GBA00009MailSendSet.EVENTCODE = "ODR_Approved_NonBreaker"
                        GBA00009MailSendSet.GBA00009setMailToNonBR()

                    Case "COSTUP"
                        GBA00009MailSendSet.EVENTCODE = "ODR_Approved"
                        GBA00009MailSendSet.GBA00009setMailToOdr()
                    Case Else
                        Continue For

                End Select

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
        If Me.txtCostType.Text = "" Then
            Me.txtCostType.Text = "ALL"
        End If
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = "承認者"
            Else
                Me.txtApprovalObj.Text = "Approver"
            End If
        End If
        Me.hdnExtractCost.Value = Me.txtCostType.Text
        Me.hdnExtractApp.Value = Me.txtApprovalObj.Text
        Me.hdnExtractOrderId.Value = Me.txtOrderId.Text
        Me.hdnExtractTankNo.Value = Me.txtTankNo.Text
        Me.hdnExtractApplicant.Value = Me.txtApplicant.Text

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
                Case Me.vLeftCostType.ID 'アクティブなビューが費用種別
                    '費用種別選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCostType.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbCostType.SelectedItem.Text
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
    ''' 前頁ボタン押下時
    ''' </summary>
    Public Sub btnPREV_Click()

        'ポジションを設定するのみ
        hdnMouseWheel.Value = "-"

    End Sub
    ''' <summary>
    ''' 次頁ボタン押下時
    ''' </summary>
    Public Sub btnNEXT_Click()

        'ポジションを設定するのみ
        hdnMouseWheel.Value = "+"

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

        AddLangSetting(dicDisplayText, Me.lblCostTypeLabel, "費用種別", "Cost Type")
        AddLangSetting(dicDisplayText, Me.lblApprovalObjLabel, "承認種別", "Approval Type")
        AddLangSetting(dicDisplayText, Me.lblOrderIdLabel, "オーダーID", "Order ID")
        AddLangSetting(dicDisplayText, Me.lblTankNoLabel, "タンクNo", "Tank No")
        AddLangSetting(dicDisplayText, Me.lblApplicantLabel, "申請者", "Applicant")

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
        'If COA0019Session.LANGDISP <> C_LANG.JA Then
        '    textCustomerTblField = "NAMES"
        'End If
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
        '承認情報取得
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(OV.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,OV.DATAID As DATAID")
        sqlStat.AppendLine("      ,OV.ORDERNO As ORDERNO")
        sqlStat.AppendLine("      ,convert(nvarchar, OV.STYMD , 111) As STYMD")
        sqlStat.AppendLine("      ,convert(nvarchar, OV.ENDYMD , 111) As ENDYMD")
        sqlStat.AppendLine("      ,OV.TANKSEQ As TANKSEQ")
        sqlStat.AppendLine("      ,OV.DTLPOLPOD As DTLPOLPOD")
        sqlStat.AppendLine("      ,isnull(TR.NAMES,'') As DTLOFFICE")
        sqlStat.AppendLine("      ,OV.TANKNO As TANKNO")
        sqlStat.AppendLine("      ,OV.COSTCODE As COSTCODE")
        sqlStat.AppendLine("      ,OV.ACTIONID As ACTIONID")
        sqlStat.AppendLine("      ,OV.DISPSEQ As DISPSEQ")
        sqlStat.AppendLine("      ,OV.LASTACT As LASTACT")
        sqlStat.AppendLine("      ,OV.CURRENCYCODE As CURRENCYCODE")
        sqlStat.AppendLine("      ,OV.AMOUNTBR As AMOUNTBR")
        sqlStat.AppendLine("      ,OV.AMOUNTORD As AMOUNTORD")
        sqlStat.AppendLine("      ,OV.AMOUNTFIX As AMOUNTFIX")
        sqlStat.AppendLine("      ,OV.CONTRACTORBR As CONTRACTORBR")
        sqlStat.AppendLine("      ,OV.CONTRACTORODR As CONTRACTORODR")
        sqlStat.AppendLine("      ,OV.CONTRACTORFIX As CONTRACTORFIX")
        sqlStat.AppendLine("      ,ISNULL(DPFIX.NAMES, CASE WHEN ISNULL(CST.CLASS2,'') <> '' THEN CTFIX.NAMESEN ELSE TRFIX.NAMES END) As CONTRACTORFIXNAME")
        sqlStat.AppendLine("      ,OV.SCHEDELDATEBR As SCHEDELDATEBR")
        sqlStat.AppendLine("      ,OV.SCHEDELDATE As SCHEDELDATE")
        sqlStat.AppendLine("      ,OV.ACTUALDATE As ACTUALDATE")
        sqlStat.AppendLine("      ,OV.LOCALBR As LOCALBR")
        sqlStat.AppendLine("      ,OV.LOCALRATE As LOCALRATE")
        sqlStat.AppendLine("      ,OV.TAXBR As TAXBR")
        sqlStat.AppendLine("      ,OV.AMOUNTPAY As AMOUNTPAY")
        sqlStat.AppendLine("      ,OV.LOCALPAY As LOCALPAY")
        sqlStat.AppendLine("      ,OV.TAXPAY As TAXPAY")
        sqlStat.AppendLine("      ,OV.INVOICEDBY As INVOICEDBY")
        sqlStat.AppendLine("      ,OV.APPLYID As APPLYID")
        sqlStat.AppendLine("      ,OV.APPLYTEXT As APPLYTEXT")
        sqlStat.AppendLine("      ,OV.LASTSTEP As LASTSTEP")
        'sqlStat.AppendLine("      ,OV.BLID As BLID")
        sqlStat.AppendLine("      ,'' As BLID")
        'sqlStat.AppendLine("      ,OV.BLAPPDATE As BLAPPDATE")
        sqlStat.AppendLine("      ,'' As BLAPPDATE")
        sqlStat.AppendLine("      ,OV.SOAAPPDATE As SOAAPPDATE")
        sqlStat.AppendLine("      ,OV.REMARK As REMARK")
        sqlStat.AppendLine("      ,OV.BRID As BRID")
        sqlStat.AppendLine("      ,OV.BRCOST As BRCOST")
        sqlStat.AppendLine("      ,OV.AGENTORGANIZER As AGENTORGANIZER")
        sqlStat.AppendLine("      ,OV.DELFLG As DELFLG")
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
        'sqlStat.AppendLine("      ,AH.APPLYID")
        sqlStat.AppendLine("      ,AH.APPLICANTID As APPLICANTID")
        sqlStat.AppendLine("      ,AH.STEP As STEP")
        sqlStat.AppendLine("      ,AH.STATUS As STATUS")
        sqlStat.AppendLine("      ,CASE WHEN (AH3.STEP = OV.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
        sqlStat.AppendLine("            WHEN (AH3.STEP = OV.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
        sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH3.STEP,'00'))))) + '/' + trim(convert(char,convert(int,OV.LASTSTEP))) END as STEPSTATE")
        sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
        sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
        sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END As CURSTEP")
        sqlStat.AppendLine("      ,AP.APPROVALTYPE As APPROVALTYPE")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(US.STAFFNAMES,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(US.STAFFNAMES_EN,'') END As APPROVERID")
        sqlStat.AppendLine("      ,CASE WHEN AH.EVENTCODE = '" & CONST_EVENT_ORD & "' THEN 'ORDER' ")
        sqlStat.AppendLine("            WHEN AH.EVENTCODE = '" & CONST_EVENT_DEM & "' THEN 'DEMURRAGE' ")
        sqlStat.AppendLine("            WHEN AH.EVENTCODE = '" & CONST_EVENT_NON & "' THEN 'NONBREAKER' ")
        sqlStat.AppendLine("            WHEN AH.EVENTCODE = '" & CONST_EVENT_SOA & "' THEN 'SOA' ")
        sqlStat.AppendLine("            WHEN AH.EVENTCODE = '" & CONST_EVENT_TNK & "' THEN 'TANKACTIVITY'")
        sqlStat.AppendLine("            WHEN AH.EVENTCODE = '" & CONST_EVENT_COS & "' THEN 'COSTUP' END AS EVENTCODE")
        sqlStat.AppendLine("      ,convert(nvarchar, AH.APPLYDATE , 111) As APPLYDATE")
        sqlStat.AppendLine("      ,CST.NAMES as COSTNAME")
        sqlStat.AppendLine("      ,CST.NAMESJP as COSTNAMEJP")
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
        sqlStat.AppendLine("  INNER JOIN GBT0005_ODR_VALUE OV") 'オーダー(明細)
        sqlStat.AppendLine("    On  OV.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   And  OV.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And  OV.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And  OV.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   And  OV.APPLYID      <> ''")
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) As STEP")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> '" & CONST_FLAG_YES & "' ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) As AH2 ")
        sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MAX(STEP) As STEP ")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS  > '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> '" & CONST_FLAG_YES & "' ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) As AH3 ")
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
        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR") '代理店名称用JOIN
        sqlStat.AppendLine("    ON  TR.CARRIERCODE  = OV.DTLOFFICE")
        sqlStat.AppendLine("   AND  TR.CLASS        = '" & C_TRADER.CLASS.AGENT & "'")
        sqlStat.AppendLine("   AND  TR.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  TR.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  TR.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN ( SELECT COSTCODE, NAMES, NAMESJP, CLASS2 FROM GBM0010_CHARGECODE")
        sqlStat.AppendLine("              WHERE COMPCODE  = @COMPCODE")
        sqlStat.AppendLine("              AND STYMD      <= @STYMD")
        sqlStat.AppendLine("              AND ENDYMD     >= @ENDYMD")
        sqlStat.AppendLine("              AND DELFLG     <> @DELFLG ")
        sqlStat.AppendLine("              GROUP BY COSTCODE, NAMES, NAMESJP, CLASS2 ) CST")
        sqlStat.AppendLine("    ON CST.COSTCODE = OV.COSTCODE ")

        '*FIX_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX")
        sqlStat.AppendLine("        ON  OV.CONTRACTORFIX = TRFIX.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = @COMPCODE ")
        sqlStat.AppendLine("       AND  TRFIX.STYMD       <= OV.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= OV.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX")
        sqlStat.AppendLine("        ON  OV.CONTRACTORFIX = DPFIX.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = @COMPCODE ")
        sqlStat.AppendLine("       AND  DPFIX.STYMD       <= OV.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= OV.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CTFIX")
        sqlStat.AppendLine("        ON  OV.CONTRACTORFIX = CTFIX.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CTFIX.COMPCODE     = @COMPCODE ")
        sqlStat.AppendLine("       AND  CTFIX.STYMD       <= OV.STYMD")
        sqlStat.AppendLine("       AND  CTFIX.ENDYMD      >= OV.STYMD")
        sqlStat.AppendLine("       AND  CTFIX.DELFLG      <> @DELFLG")
        '*FIX_CONTRACTOR名取得JOIN END

        sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
        sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
        'sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")

        If Me.hdnSelectedStYMD.Value <> "" AndAlso Me.hdnSelectedEndYMD.Value <> "" Then
            sqlStat.AppendLine("   AND (convert(nvarchar, AH.APPLYDATE , 111)  BETWEEN  @APSTYMD  AND  @APENDYMD )")
        End If

        If Me.hdnSelectedCostType.Value <> "" AndAlso Me.hdnSelectedCostType.Value <> "ALL" Then
            sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")
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

            'SQLパラメータ(動的変化あり)
            Dim paramValidityfrom As SqlParameter = Nothing
            Dim paramValidityto As SqlParameter = Nothing
            Dim paramShipper As SqlParameter = Nothing
            Dim paramConsignee As SqlParameter = Nothing
            Dim paramPort As SqlParameter = Nothing
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramDelFlg.Value = CONST_FLAG_YES
            paramUserID.Value = COA0019Session.USERID
            'paramMapID.Value = "GBT00004"
            paramLangDisp.Value = COA0019Session.LANGDISP
            paramStYMD.Value = Date.Now
            paramEndYMD.Value = Date.Now

            If Me.hdnSelectedCostType.Value <> "" AndAlso Me.hdnSelectedCostType.Value <> "ALL" Then
                Dim paramEventCode As SqlParameter = sqlCmd.Parameters.Add("@EVENTCODE", SqlDbType.NVarChar)

                Select Case Me.hdnSelectedCostType.Value
                'オーダー
                    Case "ORDER"
                        paramEventCode.Value = CONST_EVENT_ORD
                'デマレージ
                    Case "DEMURRAGE"
                        paramEventCode.Value = CONST_EVENT_DEM
                'ノンブレーカー
                    Case "NONBREAKER"
                        paramEventCode.Value = CONST_EVENT_NON
                'SOA
                    Case "SOA"
                        paramEventCode.Value = CONST_EVENT_SOA
                'タンク動静
                    Case "TANKACTIVITY"
                        paramEventCode.Value = CONST_EVENT_TNK
                'コストアップ
                    Case "COSTUP"
                        paramEventCode.Value = CONST_EVENT_COS
                End Select
            End If

            If Me.hdnSelectedStYMD.Value <> "" AndAlso Me.hdnSelectedEndYMD.Value <> "" Then

                Dim paramApStYMD As SqlParameter = sqlCmd.Parameters.Add("@APSTYMD", System.Data.SqlDbType.Date)
                Dim paramApEndYMD As SqlParameter = sqlCmd.Parameters.Add("@APENDYMD", System.Data.SqlDbType.Date)
                paramApStYMD.Value = Me.hdnSelectedStYMD.Value
                paramApEndYMD.Value = Me.hdnSelectedEndYMD.Value

            End If

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return retDt
    End Function
    ''' <summary>
    ''' オーダーURL取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOderUrl() As String
        Dim mstUrl As String = ""
        '■■■ 画面遷移先URL取得 ■■■]
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = "GB_ShowOrDetail"
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
        'COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.SCROLLTYPE = "3"
        COA0013TableObject.LEVENT = "ondblclick"
        COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

        Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DL")
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
        retDt.Columns.Add("DATAID", GetType(String))                'データID
        retDt.Columns.Add("ORDERNO", GetType(String))               '受注番号
        retDt.Columns.Add("STYMD", GetType(String))                 '有効開始日
        retDt.Columns.Add("ENDYMD", GetType(String))                '有効終了日
        retDt.Columns.Add("TANKSEQ", GetType(String))               '作業番号(タンクSEQ)
        retDt.Columns.Add("DTLPOLPOD", GetType(String))             '発地着地区分
        retDt.Columns.Add("DTLOFFICE", GetType(String))             '代理店
        retDt.Columns.Add("TANKNO", GetType(String))                'タンク番号
        retDt.Columns.Add("COSTCODE", GetType(String))              '費用コード
        retDt.Columns.Add("ACTIONID", GetType(String))              'アクションコード
        retDt.Columns.Add("DISPSEQ", GetType(String))               '表示順番
        retDt.Columns.Add("LASTACT", GetType(String))               '輸送完了作業
        retDt.Columns.Add("CURRENCYCODE", GetType(String))          '通貨換算コード
        retDt.Columns.Add("AMOUNTBR", GetType(String))              '金額(BR)
        retDt.Columns.Add("AMOUNTORD", GetType(String))             '金額(ORD)
        retDt.Columns.Add("AMOUNTFIX", GetType(String))             '金額(FIX)
        retDt.Columns.Add("CONTRACTORBR", GetType(String))          '業者コード(BR)
        retDt.Columns.Add("CONTRACTORODR", GetType(String))         '業者コード(ORD)
        retDt.Columns.Add("CONTRACTORFIX", GetType(String))         '業者コード(FIX)
        retDt.Columns.Add("SCHEDELDATEBR", GetType(String))         '作業日(BR) 
        retDt.Columns.Add("SCHEDELDATE", GetType(String))           '作業日(ORD)
        retDt.Columns.Add("ACTUALDATE", GetType(String))            '作業日(FIX)
        retDt.Columns.Add("LOCALBR", GetType(String))               '現地金額(BR)
        retDt.Columns.Add("LOCALRATE", GetType(String))             '現地通貨換算レート
        retDt.Columns.Add("TAXBR", GetType(String))                 '税(BR)
        retDt.Columns.Add("AMOUNTPAY", GetType(String))             '金額(PAY)
        retDt.Columns.Add("LOCALPAY", GetType(String))              '現地金額(PAY)
        retDt.Columns.Add("TAXPAY", GetType(String))                '税(PAY)
        retDt.Columns.Add("INVOICEDBY", GetType(String))            '船荷証券発行コード
        retDt.Columns.Add("APPLYID", GetType(String))               '費用変更申請ID
        retDt.Columns.Add("APPLYTEXT", GetType(String))             '申請コメント
        retDt.Columns.Add("LASTSTEP", GetType(String))              '最終承認STEP
        retDt.Columns.Add("BLID", GetType(String))                  'BL番号
        retDt.Columns.Add("BLAPPDATE", GetType(String))             'BL承認日
        retDt.Columns.Add("SOAAPPDATE", GetType(String))            'SOA締日付
        retDt.Columns.Add("REMARK", GetType(String))                '所見
        retDt.Columns.Add("BRID", GetType(String))                  'ブレーカーID
        retDt.Columns.Add("BRCOST", GetType(String))                'ブレーカー起因費用
        retDt.Columns.Add("AGENTORGANIZER", GetType(String))        'オーガナイザーエージェント
        retDt.Columns.Add("DELFLG", GetType(String))                '削除フラグ

        retDt.Columns.Add("APPROVALOBJECT", GetType(String))        '承認対象(通常、代行、SKIP)
        retDt.Columns.Add("APPROVALORREJECT", GetType(String))      '承認or否認
        retDt.Columns.Add("CHECK", GetType(String))                 'チェック
        'retDt.Columns.Add("APPLYID", GetType(String))               '申請ID
        retDt.Columns.Add("APPLICANTID", GetType(String))           '申請者
        retDt.Columns.Add("STEP", GetType(String))                  'ステップ
        retDt.Columns.Add("STATUS", GetType(String))                'ステータス
        retDt.Columns.Add("CURSTEP", GetType(String))               '承認ステップ
        retDt.Columns.Add("STEPSTATE", GetType(String))             'ステップ状況
        retDt.Columns.Add("APPROVALTYPE", GetType(String))          '承認区分

        retDt.Columns.Add("APPROVERID", GetType(String))

        retDt.Columns.Add("EVENTCODE", GetType(String))
        retDt.Columns.Add("APPLYDATE", GetType(String))

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

        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim eventCode As String = ""
        Select Case Convert.ToString(selectedRow.Item("EVENTCODE"))
            Case "ORDER"
                eventCode = CONST_EVENT_ORD
            Case "DEMURRAGE"
                eventCode = CONST_EVENT_DEM
            Case "NONBREAKER"
                eventCode = CONST_EVENT_NON
            Case "SOA"
                eventCode = CONST_EVENT_SOA
            Case "TANKACTIVITY"
                eventCode = CONST_EVENT_TNK
            Case "COSTUP"
                eventCode = CONST_EVENT_COS
            Case Else
                eventCode = ""
        End Select

        Me.hdnEventCode.Value = eventCode
        Me.hdnApplyID.Value = Convert.ToString(selectedRow.Item("APPLYID"))

        'JavaScriptにて別タブ表示を実行するフラグを立てる
        Me.hdnOrderViewOpen.Value = "1"

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
        If Me.txtCostType.Text.Trim <> "" Then
            isFillterOff = False
        End If
        Dim isFillterOffApp As Boolean = True
        If Me.txtApprovalObj.Text.Trim <> "" Then
            isFillterOffApp = False
        End If
        Dim isFillterOffOther As Boolean = True
        'If Me.txtOrderId.Text.Trim <> "" OrElse Me.txtTankNo.Text.Trim <> "" Then
        If Me.txtOrderId.Text.Trim <> "" OrElse Me.txtTankNo.Text.Trim <> "" OrElse Me.txtApplicant.Text.Trim <> "" Then
            isFillterOffOther = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False AndAlso Not (Me.txtCostType.Text.Trim = "ALL") Then

                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not Convert.ToString(dr.Item("EVENTCODE")) = Me.txtCostType.Text.Trim Then
                    dr.Item("HIDDEN") = 1
                End If

            End If

            If isFillterOffApp = False AndAlso Not (Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All") Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Then
                    dr.Item("HIDDEN") = 1
                End If

            End If

            If (isFillterOffOther = False AndAlso
                Not ((Me.txtOrderId.Text.Trim = "" OrElse Convert.ToString(dr.Item("ORDERNO")).Contains(Me.txtOrderId.Text.Trim)) _
              AndAlso (Me.txtTankNo.Text.Trim = "" OrElse Convert.ToString(dr.Item("TANKNO")).Contains(Me.txtTankNo.Text.Trim)) _
              AndAlso (Me.txtApplicant.Text.Trim = "" OrElse Convert.ToString(dr.Item("APPLICANTID")).Contains(Me.txtApplicant.Text.Trim)))) Then
                dr.Item("HIDDEN") = 1

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
        Me.txtCostType.Focus()

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

            '承認コメント更新処理
            UpdateApprovedText(Convert.ToString(HttpContext.Current.Session("APSRVCamp")), Convert.ToString(checkedDr.Item("APPLYID")),
                               Convert.ToString(checkedDr.Item("STEP")), Convert.ToString(checkedDr.Item("APPROVEDTEXT")))


            'DATAID取得
            Dim dataId As String = ""
            dataId = GetDATAID(Convert.ToString(checkedDr.Item("APPLYID")))

            ' 最終承認の場合メール送信
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.ODRDATAID = dataId
            GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            GBA00009MailSendSet.APPLYSTEP = Convert.ToString(checkedDr.Item("STEP"))

            Dim eventCode As String = ""
            Select Case Convert.ToString(checkedDr.Item("EVENTCODE"))
                Case "DEMURRAGE"
                    GBA00009MailSendSet.EVENTCODE = "ODR_Rejected_Demurrage"
                    GBA00009MailSendSet.GBA00009setMailToOdr()

                Case "NONBREAKER"
                    GBA00009MailSendSet.EVENTCODE = "ODR_Rejected_NonBreaker"
                    GBA00009MailSendSet.GBA00009setMailToNonBR()

                Case "COSTUP"
                    GBA00009MailSendSet.EVENTCODE = "ODR_Rejected"
                    GBA00009MailSendSet.GBA00009setMailToOdr()

                Case Else
                    Continue For
            End Select
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
        If Me.txtCostType.Text = "" Then
            Me.txtCostType.Text = "ALL"
        End If
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = "承認者"
            Else
                Me.txtApprovalObj.Text = "Approver"
            End If
        End If
        Me.hdnExtractCost.Value = Me.txtCostType.Text
        Me.hdnExtractApp.Value = Me.txtApprovalObj.Text
        Me.hdnExtractOrderId.Value = Me.txtOrderId.Text
        Me.hdnExtractTankNo.Value = Me.txtTankNo.Text
        Me.hdnExtractApplicant.Value = Me.txtApplicant.Text

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

        If TypeOf Page.PreviousPage Is GBT00010APPROVAL Then

            Dim prevObj As GBT00010APPROVAL = DirectCast(Page.PreviousPage, GBT00010APPROVAL)
            Dim tmpCont As Control = prevObj.FindControl("hdnMsgId")

            If tmpCont IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpCont, HiddenField)
                Me.hdnMsgId.Value = tmphdn.Value
            End If

            Dim tmpExt As Control = prevObj.FindControl("hdnExtractCost")

            If tmpExt IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpExt, HiddenField)
                Me.hdnExtractCost.Value = tmphdn.Value
            End If

            Dim tmpExtApp As Control = prevObj.FindControl("hdnExtractApp")

            If tmpExtApp IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpExtApp, HiddenField)
                Me.hdnExtractApp.Value = tmphdn.Value
            End If

            Dim tmpStYMD As Control = prevObj.FindControl("hdnSelectedStYMD")

            If tmpStYMD IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpStYMD, HiddenField)
                Me.hdnSelectedStYMD.Value = FormatDateYMD(tmphdn.Value, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpEndYMD As Control = prevObj.FindControl("hdnSelectedEndYMD")

            If tmpEndYMD IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpEndYMD, HiddenField)
                Me.hdnSelectedEndYMD.Value = FormatDateYMD(tmphdn.Value, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpCostType As Control = prevObj.FindControl("hdnSelectedCostType")

            If tmpCostType IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpCostType, HiddenField)
                Me.hdnSelectedCostType.Value = tmphdn.Value
            End If

            '画面ビューID保持
            Dim tmpPrevViewIDObj As HiddenField = DirectCast(prevObj.FindControl("hdnPrevViewID"), HiddenField)
            If tmpPrevViewIDObj IsNot Nothing Then
                Me.hdnPrevViewID.Value = tmpPrevViewIDObj.Value
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00010SELECT Then

            Dim prevObj As GBT00010SELECT = DirectCast(Page.PreviousPage, GBT00010SELECT)
            Dim tmpStYMD As Control = prevObj.FindControl("txtStYMD")

            If tmpStYMD IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpStYMD, TextBox)
                Me.hdnSelectedStYMD.Value = FormatDateYMD(tmphdn.Text, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpEndYMD As Control = prevObj.FindControl("txtEndYMD")

            If tmpEndYMD IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpEndYMD, TextBox)
                Me.hdnSelectedEndYMD.Value = FormatDateYMD(tmphdn.Text, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpCostType As Control = prevObj.FindControl("txtCostType")

            If tmpCostType IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpCostType, TextBox)
                Me.hdnSelectedCostType.Value = tmphdn.Text
            End If

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

        End If
    End Sub
    ''' <summary>
    ''' リストアイテムを設定
    ''' </summary>
    Private Function SetCostTypeListItem(selectedValue As String) As String
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Dim retCode As String = C_MESSAGENO.NORMAL

        'リストクリア
        Me.lbCostType.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "COSTTYPE"
        COA0017FixValue.LISTBOX1 = Me.lbCostType
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbCostType = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            retCode = COA0017FixValue.ERR
        End If
        Return retCode
    End Function
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
    ''' 金額(FIX)更新処理
    ''' </summary>
    Private Sub UpdateAmountFix(ByVal dataId As String, tergetDr As DataRow)
        Dim needsUpdateDemurrageComm As Boolean = False
        If tergetDr IsNot Nothing AndAlso tergetDr("COSTCODE").Equals(GBC_COSTCODE_DEMURRAGE) Then
            needsUpdateDemurrageComm = True
        End If
        'オーダー明細削除
        Dim sqlStat As New StringBuilder
        sqlStat.Clear()
        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("      ORDERNO")
        sqlStat.AppendLine("     ,STYMD")
        sqlStat.AppendLine("     ,ENDYMD")
        sqlStat.AppendLine("     ,TANKSEQ")
        sqlStat.AppendLine("     ,DTLPOLPOD")
        sqlStat.AppendLine("     ,DTLOFFICE")
        sqlStat.AppendLine("     ,TANKNO")
        sqlStat.AppendLine("     ,COSTCODE")
        sqlStat.AppendLine("     ,ACTIONID")
        sqlStat.AppendLine("     ,DISPSEQ")
        sqlStat.AppendLine("     ,LASTACT")
        sqlStat.AppendLine("     ,REQUIREDACT")
        sqlStat.AppendLine("     ,ORIGINDESTINATION")
        sqlStat.AppendLine("     ,COUNTRYCODE")
        sqlStat.AppendLine("     ,CURRENCYCODE")
        sqlStat.AppendLine("     ,TAXATION")
        sqlStat.AppendLine("     ,AMOUNTBR")
        sqlStat.AppendLine("     ,AMOUNTORD")
        sqlStat.AppendLine("     ,AMOUNTFIX")
        sqlStat.AppendLine("     ,CONTRACTORBR")
        sqlStat.AppendLine("     ,CONTRACTORODR")
        sqlStat.AppendLine("     ,CONTRACTORFIX")
        sqlStat.AppendLine("     ,SCHEDELDATEBR")
        sqlStat.AppendLine("     ,SCHEDELDATE")
        sqlStat.AppendLine("     ,ACTUALDATE")
        sqlStat.AppendLine("     ,LOCALBR")
        sqlStat.AppendLine("     ,LOCALRATE")
        sqlStat.AppendLine("     ,TAXBR")
        sqlStat.AppendLine("     ,AMOUNTPAY")
        sqlStat.AppendLine("     ,LOCALPAY")
        sqlStat.AppendLine("     ,TAXPAY")
        sqlStat.AppendLine("     ,INVOICEDBY")
        sqlStat.AppendLine("     ,APPLYID")
        sqlStat.AppendLine("     ,APPLYTEXT")
        sqlStat.AppendLine("     ,LASTSTEP")
        sqlStat.AppendLine("     ,SOAAPPDATE")
        sqlStat.AppendLine("     ,REMARK")
        sqlStat.AppendLine("     ,BRID")
        sqlStat.AppendLine("     ,BRCOST")
        sqlStat.AppendLine("     ,DATEFIELD")
        sqlStat.AppendLine("     ,DATEINTERVAL")
        sqlStat.AppendLine("     ,BRADDEDCOST")
        sqlStat.AppendLine("     ,AGENTORGANIZER")
        sqlStat.AppendLine("     ,DELFLG")
        sqlStat.AppendLine("     ,INITYMD")
        sqlStat.AppendLine("     ,INITUSER")
        sqlStat.AppendLine("     ,UPDYMD")
        sqlStat.AppendLine("     ,UPDUSER")
        sqlStat.AppendLine("     ,UPDTERMID")
        sqlStat.AppendLine("     ,RECEIVEYMD")
        sqlStat.AppendLine(" ) SELECT ORDERNO")
        sqlStat.AppendLine("         ,STYMD")
        sqlStat.AppendLine("         ,ENDYMD")
        sqlStat.AppendLine("         ,TANKSEQ")
        sqlStat.AppendLine("         ,DTLPOLPOD")
        sqlStat.AppendLine("         ,DTLOFFICE")
        sqlStat.AppendLine("         ,TANKNO")
        sqlStat.AppendLine("         ,COSTCODE")
        sqlStat.AppendLine("         ,ACTIONID")
        sqlStat.AppendLine("         ,DISPSEQ")
        sqlStat.AppendLine("         ,LASTACT")
        sqlStat.AppendLine("         ,REQUIREDACT")
        sqlStat.AppendLine("         ,ORIGINDESTINATION")
        sqlStat.AppendLine("         ,COUNTRYCODE")
        sqlStat.AppendLine("         ,CURRENCYCODE")
        sqlStat.AppendLine("         ,TAXATION")
        sqlStat.AppendLine("         ,AMOUNTBR")
        sqlStat.AppendLine("         ,AMOUNTORD")
        sqlStat.AppendLine("         ,AMOUNTFIX")
        sqlStat.AppendLine("         ,CONTRACTORBR")
        sqlStat.AppendLine("         ,CONTRACTORODR")
        sqlStat.AppendLine("         ,CONTRACTORFIX")
        sqlStat.AppendLine("         ,SCHEDELDATEBR")
        sqlStat.AppendLine("         ,SCHEDELDATE")
        sqlStat.AppendLine("         ,ACTUALDATE")
        sqlStat.AppendLine("         ,LOCALBR")
        sqlStat.AppendLine("         ,LOCALRATE")
        sqlStat.AppendLine("         ,TAXBR")
        sqlStat.AppendLine("         ,AMOUNTPAY")
        sqlStat.AppendLine("         ,LOCALPAY")
        sqlStat.AppendLine("         ,TAXPAY")
        sqlStat.AppendLine("         ,INVOICEDBY")
        sqlStat.AppendLine("         ,APPLYID       AS APPLYID")
        sqlStat.AppendLine("         ,APPLYTEXT     AS APPLYTEXT")
        sqlStat.AppendLine("         ,LASTSTEP      AS LASTSTEP")
        sqlStat.AppendLine("         ,SOAAPPDATE")
        sqlStat.AppendLine("         ,REMARK")
        sqlStat.AppendLine("         ,BRID")
        sqlStat.AppendLine("         ,BRCOST")
        sqlStat.AppendLine("         ,DATEFIELD")
        sqlStat.AppendLine("         ,DATEINTERVAL")
        sqlStat.AppendLine("         ,BRADDEDCOST")
        sqlStat.AppendLine("         ,AGENTORGANIZER")
        sqlStat.AppendLine("         ,'" & CONST_FLAG_YES & "'             AS DELFLG")
        sqlStat.AppendLine("         ,INITYMD")
        sqlStat.AppendLine("         ,INITUSER")
        sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

        sqlStat.AppendLine(" UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("    SET AMOUNTFIX     = AMOUNTORD")
        sqlStat.AppendLine("       ,UPDYMD        = @UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER       = @UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID     = @UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD    = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
             sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            Dim procDate As Date = Date.Now
            With sqlCmd.Parameters
                'パラメータ設定
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                .Add("@DATAID", SqlDbType.NVarChar).Value = dataId
                sqlCmd.ExecuteNonQuery()
            End With
            'デマレージレコードの承認につき手数料レコードも承認された金額で更新をする
            If needsUpdateDemurrageComm Then
                AddAgentCommRecord(tergetDr, sqlCon, procDate:=procDate)
            End If
        End Using
    End Sub
    ''' <summary>
    ''' デマレッジ確定時にエージェントコミッションを同発着に追加する処理
    ''' </summary>
    ''' <param name="drFixDemurrage">対象デマレッジレコード</param>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="sqlTran">[オプション]トランザクション(未指定時はトランザクションなし)</param>
    ''' <returns></returns>
    Private Function AddAgentCommRecord(drFixDemurrage As DataRow, sqlCon As SqlConnection, Optional sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#) As String

        'ありえないがそもそもレコードがない場合は何もしない
        If drFixDemurrage Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If
        If procDate.Equals(#1900/01/01#) Then
            procDate = Now
        End If
        'デマレッジ確定後の増幅定義の取得(キー：発着、リスト(0:発着、1:費用コード、2:率、3:Remarks記載文言)
        Static dicFixDemurrage As Dictionary(Of String, List(Of String)) = Nothing
        If dicFixDemurrage Is Nothing Then
            Dim COA0017FixValue As New COA0017FixValue With {
                .COMPCODE = GBC_COMPCODE_D, .CLAS = "FIXDEMURRAGE"
                }
            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                dicFixDemurrage = COA0017FixValue.VALUEDIC
            Else
                Throw New Exception("Fix value getError")
            End If
        End If
        Dim sqlStat As New StringBuilder
        Dim amountFix As Decimal = If(Convert.ToString(drFixDemurrage.Item("AMOUNTORD")) = "", 0, Decimal.Parse(Convert.ToString(drFixDemurrage.Item("AMOUNTORD"))))
        Dim orderNo As String = Convert.ToString(drFixDemurrage.Item("ORDERNO"))
        Dim tankSeq As String = Convert.ToString(drFixDemurrage.Item("TANKSEQ"))
        Dim dtlPolPod As String = Convert.ToString(drFixDemurrage.Item("DTLPOLPOD"))
        Dim listFixDemurrage As List(Of String) = dicFixDemurrage(dtlPolPod)
        Dim costCode As String = listFixDemurrage(1)
        Dim commRate As Decimal = If(listFixDemurrage(2) = "", 0, Decimal.Parse(listFixDemurrage(2)))
        Dim commAmount As Decimal = amountFix * commRate
        Dim commRemark As String = listFixDemurrage(3)
        '既にあるデマレッジ増幅のAgentComレコードに削除フラグを立てる
        sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("   SET DELFLG     = @DELFLG")
        sqlStat.AppendLine("     , UPDYMD     = @UPDYMD")
        sqlStat.AppendLine("     , UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("     , UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("     , RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND DTLPOLPOD = @DTLPOLPOD")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        sqlStat.AppendLine("   AND COSTCODE  = @COSTCODE")
        sqlStat.AppendLine("   AND REMARK    = @REMARK")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG;")
        If Convert.ToString(drFixDemurrage.Item("SOAAPPDATE")).Trim <> "" _
           AndAlso Convert.ToString(drFixDemurrage.Item("JOT")) <> "on" Then
            sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
            sqlStat.AppendLine("           ORDERNO ")
            sqlStat.AppendLine("         , STYMD ")
            sqlStat.AppendLine("         , ENDYMD")
            sqlStat.AppendLine("         , TANKSEQ")
            sqlStat.AppendLine("         , DTLPOLPOD")
            sqlStat.AppendLine("         , DTLOFFICE")
            sqlStat.AppendLine("         , TANKNO")
            sqlStat.AppendLine("         , COSTCODE")
            sqlStat.AppendLine("         , ACTIONID")
            sqlStat.AppendLine("         , DISPSEQ")
            sqlStat.AppendLine("         , LASTACT")
            sqlStat.AppendLine("         , REQUIREDACT")
            sqlStat.AppendLine("         , ORIGINDESTINATION")
            sqlStat.AppendLine("         , COUNTRYCODE")
            sqlStat.AppendLine("         , CURRENCYCODE")
            sqlStat.AppendLine("         , TAXATION")
            sqlStat.AppendLine("         , AMOUNTBR")
            sqlStat.AppendLine("         , AMOUNTORD")
            sqlStat.AppendLine("         , AMOUNTFIX")
            sqlStat.AppendLine("         , CONTRACTORBR")
            sqlStat.AppendLine("         , CONTRACTORODR")
            sqlStat.AppendLine("         , CONTRACTORFIX")
            sqlStat.AppendLine("         , SCHEDELDATEBR")
            sqlStat.AppendLine("         , SCHEDELDATE")
            sqlStat.AppendLine("         , ACTUALDATE")
            sqlStat.AppendLine("         , LOCALBR")
            sqlStat.AppendLine("         , LOCALRATE")
            sqlStat.AppendLine("         , TAXBR")
            sqlStat.AppendLine("         , AMOUNTPAY")
            sqlStat.AppendLine("         , LOCALPAY")
            sqlStat.AppendLine("         , TAXPAY")
            sqlStat.AppendLine("         , INVOICEDBY")
            sqlStat.AppendLine("         , APPLYID")
            sqlStat.AppendLine("         , APPLYTEXT")
            sqlStat.AppendLine("         , LASTSTEP")
            sqlStat.AppendLine("         , SOAAPPDATE")
            sqlStat.AppendLine("         , REMARK")
            sqlStat.AppendLine("         , BRID")
            sqlStat.AppendLine("         , BRCOST")
            sqlStat.AppendLine("         , DATEFIELD")
            sqlStat.AppendLine("         , DATEINTERVAL")
            sqlStat.AppendLine("         , BRADDEDCOST")
            sqlStat.AppendLine("         , AGENTORGANIZER")
            sqlStat.AppendLine("         , DELFLG")
            sqlStat.AppendLine("         , INITYMD ")
            sqlStat.AppendLine("         , INITUSER")
            sqlStat.AppendLine("         , UPDYMD")
            sqlStat.AppendLine("         , UPDUSER")
            sqlStat.AppendLine("         , UPDTERMID")
            sqlStat.AppendLine("         , RECEIVEYMD")
            sqlStat.AppendLine("   ) ")
            sqlStat.AppendLine("SELECT TOP 1")
            sqlStat.AppendLine("           OV.ORDERNO ")
            sqlStat.AppendLine("         , OV.STYMD ")
            sqlStat.AppendLine("         , OV.ENDYMD")
            sqlStat.AppendLine("         , OV.TANKSEQ")
            sqlStat.AppendLine("         , OV.DTLPOLPOD")
            sqlStat.AppendLine("         , OV.DTLOFFICE")
            sqlStat.AppendLine("         , OV.TANKNO")
            sqlStat.AppendLine("         , @COSTCODE")
            sqlStat.AppendLine("         , ''") 'ACTIONID
            sqlStat.AppendLine("         , ''") 'DISPSREQ
            sqlStat.AppendLine("         , ''") 'LASTACT
            sqlStat.AppendLine("         , ''") 'REQUIREDACT
            sqlStat.AppendLine("         , ''") 'ORIGINDESTINATION
            sqlStat.AppendLine("         , OV.COUNTRYCODE")
            sqlStat.AppendLine("         , OV.CURRENCYCODE")
            sqlStat.AppendLine("         , @TAXATION")
            sqlStat.AppendLine("         , @COMMAMOUNT")
            sqlStat.AppendLine("         , @COMMAMOUNT")
            sqlStat.AppendLine("         , @COMMAMOUNT")
            sqlStat.AppendLine("         , ''") 'CONTRACTORBR
            sqlStat.AppendLine("         , ''") 'CONTRACTORODR
            sqlStat.AppendLine("         , CASE WHEN OV.DTLPOLPOD = 'POL1' THEN OBS.AGENTPOL1")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POL2' THEN OBS.AGENTPOL2")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POD1' THEN OBS.AGENTPOD1")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POD2' THEN OBS.AGENTPOD2")
            sqlStat.AppendLine("                ELSE '1'")
            sqlStat.AppendLine("            END") 'CONTRACTORFIX
            sqlStat.AppendLine("         , @DEMDATE") 'SCHEDELDATEBR
            sqlStat.AppendLine("         , @DEMDATE") 'SCHEDELDATE
            sqlStat.AppendLine("         , @DEMDATE") 'ACTUALDATE
            sqlStat.AppendLine("         , 0") 'LOCALBR
            sqlStat.AppendLine("         , OV.LOCALRATE") 'LOCALRATE
            sqlStat.AppendLine("         , 0") 'TAXBR
            sqlStat.AppendLine("         , 0") 'AMOUNTPAY
            sqlStat.AppendLine("         , 0") 'LOCALPAY
            sqlStat.AppendLine("         , 0") 'TAXPAY

            sqlStat.AppendLine("         , CASE WHEN OV.DTLPOLPOD = 'POL1' THEN OBS.AGENTPOL1")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POL2' THEN OBS.AGENTPOL2")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POD1' THEN OBS.AGENTPOD1")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POD2' THEN OBS.AGENTPOD2")
            sqlStat.AppendLine("                ELSE '1'")
            sqlStat.AppendLine("            END") 'INVOICEDBY
            'sqlStat.AppendLine("         , OV.INVOICEDBY") 'INVOICEDBY
            sqlStat.AppendLine("         , ''") 'APPLYID
            sqlStat.AppendLine("         , ''") 'APPLYTEXT
            sqlStat.AppendLine("         , ''") 'LASTSTEP
            sqlStat.AppendLine("         , @DEMSOADATE") 'SOAAPPDATE
            sqlStat.AppendLine("         , @REMARK")
            sqlStat.AppendLine("         , OV.BRID")
            sqlStat.AppendLine("         , OV.BRCOST")
            sqlStat.AppendLine("         , OV.DATEFIELD")
            sqlStat.AppendLine("         , OV.DATEINTERVAL")
            sqlStat.AppendLine("         , OV.BRADDEDCOST")
            sqlStat.AppendLine("         , OV.AGENTORGANIZER")
            sqlStat.AppendLine("         , '" & CONST_FLAG_NO & "'")
            sqlStat.AppendLine("         , OV.INITYMD")
            sqlStat.AppendLine("         , OV.INITUSER")
            sqlStat.AppendLine("         , @UPDYMD")
            sqlStat.AppendLine("         , @UPDUSER")
            sqlStat.AppendLine("         , @UPDTERMID")
            sqlStat.AppendLine("         , @RECEIVEYMD")
            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE OV")
            sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE OVS")
            sqlStat.AppendLine("         ON OVS.ORDERNO   = OV.ORDERNO")
            sqlStat.AppendLine("        AND OVS.TANKSEQ   = OV.TANKSEQ")
            sqlStat.AppendLine("        AND OVS.DTLPOLPOD = OV.DTLPOLPOD")
            sqlStat.AppendLine("        AND OVS.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("        AND OVS.CONTRACTORFIX <> ''")
            sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OBS")
            sqlStat.AppendLine("         ON OBS.ORDERNO   = OV.ORDERNO")
            sqlStat.AppendLine("        AND OBS.DELFLG   <> @DELFLG")
            'sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CC")
            'sqlStat.AppendLine("         ON CC.COSTCODE = OVS.COSTCODE")
            'sqlStat.AppendLine("        AND '1' = CASE WHEN OVS.DTLPOLPOD LIKE 'POL%' AND CC.LDKBN IN ('B','L') THEN '1' ")
            'sqlStat.AppendLine("                       WHEN OVS.DTLPOLPOD LIKE 'POD%' AND CC.LDKBN IN ('B','D') THEN '1' ")
            'sqlStat.AppendLine("                       WHEN OVS.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            'sqlStat.AppendLine("                       ELSE '1'")
            'sqlStat.AppendLine("                  END")
            'sqlStat.AppendLine("        AND CC.DELFLG  <> @DELFLG")
            'sqlStat.AppendLine("        AND CC.CLASS4 IN (SELECT CCS.CLASS4")
            'sqlStat.AppendLine("                            FROM GBM0010_CHARGECODE CCS")
            'sqlStat.AppendLine("                           WHERE CCS.COSTCODE = OV.COSTCODE")
            'sqlStat.AppendLine("                             AND '1' = CASE WHEN OV.DTLPOLPOD LIKE 'POL%' AND CCS.LDKBN IN ('B','L') THEN '1' ")
            'sqlStat.AppendLine("                                            WHEN OV.DTLPOLPOD LIKE 'POD%' AND CCS.LDKBN IN ('B','D') THEN '1' ")
            'sqlStat.AppendLine("                                            WHEN OV.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            'sqlStat.AppendLine("                                            ELSE '1'")
            'sqlStat.AppendLine("                                       END")
            'sqlStat.AppendLine("                             AND CCS.DELFLG  <> @DELFLG)")
            sqlStat.AppendLine(" WHERE OV.DATAID=@DATAID;")
        End If
        Try

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@DATAID", SqlDbType.NVarChar).Value = Convert.ToString(drFixDemurrage.Item("DATAID"))
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankSeq
                    .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = dtlPolPod
                    .Add("@COSTCODE", SqlDbType.NVarChar).Value = costCode 'FIXVALUEで取得したデマレッジ手数料費目
                    .Add("@REMARK", SqlDbType.NVarChar).Value = commRemark
                    .Add("@TAXATION", SqlDbType.NVarChar).Value = If(GetDefaultTaxation(Convert.ToString(drFixDemurrage.Item("COUNTRYCODE"))) = "on", "1", "0")
                    .Add("@COMMAMOUNT", SqlDbType.NVarChar).Value = commAmount
                    .Add("@DEMDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(drFixDemurrage.Item("ACTUALDATE")))
                    .Add("@DEMSOADATE", SqlDbType.Date).Value = procDate
                    .Add("@RECEIVEYMD", SqlDbType.NVarChar).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.ExecuteNonQuery()
            End Using
            Return orderNo
        Catch ex As Exception
            Throw
        End Try
    End Function
    '''' <summary>
    '''' 金額(FIX)更新処理
    '''' </summary>
    '''' <remarks>n倍加してしまうロジック20190807一旦コメント</remarks>
    'Private Sub UpdateAmountFix(ByVal parmApplyId As String, ByVal parmOrder As String)

    '    'オーダー明細削除
    '    Dim sqlStat As New StringBuilder
    '    sqlStat.Clear()
    '    sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
    '    sqlStat.AppendLine("   SET DELFLG     = @DELFLG ")
    '    sqlStat.AppendLine("      ,UPDYMD     = @UPDYMD ")
    '    sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER ")
    '    sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
    '    sqlStat.AppendLine(" WHERE ORDERNO    = @ORDERNO ")
    '    sqlStat.AppendLine("   AND APPLYID    = @APPLYID ")
    '    'DB接続
    '    Using sqlCon As New SqlConnection(COA0019Session.DBcon),
    '         sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
    '        sqlCon.Open() '接続オープン
    '        With sqlCmd.Parameters
    '            'パラメータ設定
    '            .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
    '            .Add("@UPDYMD", SqlDbType.DateTime).Value = Date.Now
    '            .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
    '            .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
    '            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
    '            .Add("@ORDERNO", SqlDbType.NVarChar).Value = parmOrder
    '            .Add("@APPLYID", SqlDbType.NVarChar).Value = parmApplyId
    '            sqlCmd.ExecuteNonQuery()
    '        End With

    '    End Using

    '    '金額(FIX)更新
    '    sqlStat.Clear()
    '    sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
    '    sqlStat.AppendLine("              ORDERNO")
    '    sqlStat.AppendLine("             ,STYMD")
    '    sqlStat.AppendLine("             ,ENDYMD")
    '    sqlStat.AppendLine("             ,TANKSEQ")
    '    sqlStat.AppendLine("             ,DTLPOLPOD")
    '    sqlStat.AppendLine("             ,DTLOFFICE")
    '    sqlStat.AppendLine("             ,TANKNO")
    '    sqlStat.AppendLine("             ,COSTCODE")
    '    sqlStat.AppendLine("             ,ACTIONID")
    '    sqlStat.AppendLine("             ,DISPSEQ")
    '    sqlStat.AppendLine("             ,LASTACT")
    '    sqlStat.AppendLine("             ,REQUIREDACT")
    '    sqlStat.AppendLine("             ,ORIGINDESTINATION")
    '    sqlStat.AppendLine("             ,COUNTRYCODE")
    '    sqlStat.AppendLine("             ,CURRENCYCODE")
    '    sqlStat.AppendLine("             ,TAXATION")
    '    sqlStat.AppendLine("             ,AMOUNTBR")
    '    sqlStat.AppendLine("             ,AMOUNTORD")
    '    sqlStat.AppendLine("             ,AMOUNTFIX")
    '    sqlStat.AppendLine("             ,CONTRACTORBR")
    '    sqlStat.AppendLine("             ,CONTRACTORODR")
    '    sqlStat.AppendLine("             ,CONTRACTORFIX")
    '    sqlStat.AppendLine("             ,SCHEDELDATEBR")
    '    sqlStat.AppendLine("             ,SCHEDELDATE")
    '    sqlStat.AppendLine("             ,ACTUALDATE")
    '    sqlStat.AppendLine("             ,LOCALBR")
    '    sqlStat.AppendLine("             ,LOCALRATE")
    '    sqlStat.AppendLine("             ,TAXBR")
    '    sqlStat.AppendLine("             ,AMOUNTPAY")
    '    sqlStat.AppendLine("             ,LOCALPAY")
    '    sqlStat.AppendLine("             ,TAXPAY")
    '    sqlStat.AppendLine("             ,INVOICEDBY")
    '    sqlStat.AppendLine("             ,APPLYID")
    '    sqlStat.AppendLine("             ,APPLYTEXT")
    '    sqlStat.AppendLine("             ,LASTSTEP")
    '    sqlStat.AppendLine("             ,SOAAPPDATE")
    '    sqlStat.AppendLine("             ,REMARK")
    '    sqlStat.AppendLine("             ,BRID")
    '    sqlStat.AppendLine("             ,BRCOST")
    '    sqlStat.AppendLine("             ,DATEFIELD")
    '    sqlStat.AppendLine("             ,DATEINTERVAL")
    '    sqlStat.AppendLine("             ,BRADDEDCOST")
    '    sqlStat.AppendLine("             ,AGENTORGANIZER")
    '    sqlStat.AppendLine("             ,CURRENCYSEGMENT")
    '    sqlStat.AppendLine("             ,ACCCRERATE")
    '    sqlStat.AppendLine("             ,ACCCREYEN")
    '    sqlStat.AppendLine("             ,ACCCREFOREIGN")
    '    sqlStat.AppendLine("             ,ACCCURRENCYSEGMENT")
    '    sqlStat.AppendLine("             ,DELFLG")
    '    sqlStat.AppendLine("             ,INITYMD ")
    '    sqlStat.AppendLine("             ,INITUSER ")
    '    sqlStat.AppendLine("             ,UPDYMD ")
    '    sqlStat.AppendLine("             ,UPDUSER ")
    '    sqlStat.AppendLine("             ,UPDTERMID ")
    '    sqlStat.AppendLine("             ,RECEIVEYMD ")
    '    sqlStat.AppendLine("   ) SELECT ")
    '    sqlStat.AppendLine("              ORDERNO")
    '    sqlStat.AppendLine("             ,STYMD")
    '    sqlStat.AppendLine("             ,ENDYMD")
    '    sqlStat.AppendLine("             ,TANKSEQ")
    '    sqlStat.AppendLine("             ,DTLPOLPOD")
    '    sqlStat.AppendLine("             ,DTLOFFICE")
    '    sqlStat.AppendLine("             ,TANKNO")
    '    sqlStat.AppendLine("             ,COSTCODE")
    '    sqlStat.AppendLine("             ,ACTIONID")
    '    sqlStat.AppendLine("             ,DISPSEQ")
    '    sqlStat.AppendLine("             ,LASTACT")
    '    sqlStat.AppendLine("             ,REQUIREDACT")
    '    sqlStat.AppendLine("             ,ORIGINDESTINATION")
    '    sqlStat.AppendLine("             ,COUNTRYCODE")
    '    sqlStat.AppendLine("             ,CURRENCYCODE")
    '    sqlStat.AppendLine("             ,TAXATION")
    '    sqlStat.AppendLine("             ,AMOUNTBR")
    '    sqlStat.AppendLine("             ,AMOUNTORD")
    '    sqlStat.AppendLine("             ,AMOUNTORD")
    '    sqlStat.AppendLine("             ,CONTRACTORBR")
    '    sqlStat.AppendLine("             ,CONTRACTORODR")
    '    sqlStat.AppendLine("             ,CONTRACTORFIX")
    '    sqlStat.AppendLine("             ,SCHEDELDATEBR")
    '    sqlStat.AppendLine("             ,SCHEDELDATE")
    '    sqlStat.AppendLine("             ,ACTUALDATE")
    '    sqlStat.AppendLine("             ,LOCALBR")
    '    sqlStat.AppendLine("             ,LOCALRATE")
    '    sqlStat.AppendLine("             ,TAXBR")
    '    sqlStat.AppendLine("             ,AMOUNTPAY")
    '    sqlStat.AppendLine("             ,LOCALPAY")
    '    sqlStat.AppendLine("             ,TAXPAY")
    '    sqlStat.AppendLine("             ,INVOICEDBY")
    '    sqlStat.AppendLine("             ,APPLYID")
    '    sqlStat.AppendLine("             ,APPLYTEXT")
    '    sqlStat.AppendLine("             ,LASTSTEP")
    '    sqlStat.AppendLine("             ,SOAAPPDATE")
    '    sqlStat.AppendLine("             ,REMARK")
    '    sqlStat.AppendLine("             ,BRID")
    '    sqlStat.AppendLine("             ,BRCOST")
    '    sqlStat.AppendLine("             ,DATEFIELD")
    '    sqlStat.AppendLine("             ,DATEINTERVAL")
    '    sqlStat.AppendLine("             ,BRADDEDCOST")
    '    sqlStat.AppendLine("             ,AGENTORGANIZER")
    '    sqlStat.AppendLine("             ,CURRENCYSEGMENT")
    '    sqlStat.AppendLine("             ,ACCCRERATE")
    '    sqlStat.AppendLine("             ,ACCCREYEN")
    '    sqlStat.AppendLine("             ,ACCCREFOREIGN")
    '    sqlStat.AppendLine("             ,ACCCURRENCYSEGMENT")
    '    sqlStat.AppendLine("             ,@DELFLG")
    '    sqlStat.AppendLine("             ,INITYMD ")
    '    sqlStat.AppendLine("             ,INITUSER ")
    '    sqlStat.AppendLine("             ,@UPDYMD ")
    '    sqlStat.AppendLine("             ,@UPDUSER ")
    '    sqlStat.AppendLine("             ,@UPDTERMID ")
    '    sqlStat.AppendLine("             ,@RECEIVEYMD ")
    '    sqlStat.AppendLine("      FROM  GBT0005_ODR_VALUE    ")
    '    sqlStat.AppendLine("     WHERE ORDERNO  = @ORDERNO   ")
    '    sqlStat.AppendLine("       AND APPLYID  = @APPLYID ")
    '    Using sqlCon As New SqlConnection(COA0019Session.DBcon),
    '        sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
    '        sqlCon.Open() '接続オープン
    '        With sqlCmd.Parameters
    '            'パラメータ設定
    '            .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
    '            '.Add("@INITYMD", SqlDbType.DateTime).Value = Date.Now
    '            .Add("@UPDYMD", SqlDbType.DateTime).Value = Date.Now
    '            .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
    '            .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
    '            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
    '            .Add("@ORDERNO", SqlDbType.NVarChar).Value = parmOrder
    '            .Add("@APPLYID", SqlDbType.NVarChar).Value = parmApplyId

    '            sqlCmd.ExecuteNonQuery()

    '        End With
    '    End Using
    'End Sub

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
    ''' 最新のデマ用のレコード抽出
    ''' </summary>
    ''' <returns></returns>
    Private Function GetTargetData(ByVal applyId As String, Optional ByRef sqlCon As SqlConnection = Nothing) As DataRow
        Dim canCloseConnect As Boolean = False
        Dim dataID As String = ""
        Dim retDr As DataRow
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT VL.DATAID ")
            sqlStat.AppendLine("     , VL.ORDERNO     AS ORDERNO")
            sqlStat.AppendLine("     , VL.DTLOFFICE   AS DTLOFFICE ")
            sqlStat.AppendLine("     , VL.DTLPOLPOD   AS DTLPOLPOD ")
            sqlStat.AppendLine("     , VL.TANKNO      AS TANKNO ")
            sqlStat.AppendLine("     , VL.TANKSEQ     AS TANKSEQ ")
            sqlStat.AppendLine("     , VL.COUNTRYCODE AS COUNTRYCODE ")
            sqlStat.AppendLine("     , VL.COSTCODE    AS COSTCODE")
            sqlStat.AppendLine("     , VL.AMOUNTBR    AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.AMOUNTORD   AS AMOUNTORD")
            sqlStat.AppendLine("     , VL.AMOUNTFIX   AS AMOUNTFIX")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")

            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
            sqlStat.AppendLine("     , VL.INVOICEDBY AS INVOICEDBY")
            sqlStat.AppendLine("     , CASE WHEN VL.INVOICEDBY = @JOTAGENT THEN 'on' ELSE '' END AS JOT")
            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")
            sqlStat.AppendLine(" WHERE VL.APPLYID   = @APPLYID")
            sqlStat.AppendLine("   AND VL.DELFLG   <> @DELFLG")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Order Value error")
                    End If

                    retDr = dt.Rows(0)
                End Using
            End Using
            Return retDr
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
    ''' 課税フラグのデフォルト値取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>仮作成にて変動の可能性がある為、デフォルト値取得関数化</remarks>
    Private Function GetDefaultTaxation(countryCode As String) As String
        Return If(GBA00003UserSetting.IS_JPOPERATOR AndAlso countryCode = "JP", "on", "")
    End Function
    ''' <summary>
    ''' 日付を変換
    ''' </summary>
    ''' <param name="dateString"></param>
    ''' <returns>変換できない場合はMinValue</returns>
    Private Function DateStringToDateTime(dateString As String) As DateTime
        Dim dateTimeDefault As DateTime = DateTime.Parse("1900/01/01 00:00:00")
        Dim tmpDateTime As DateTime
        If DateTime.TryParse(dateString, tmpDateTime) Then
            Return tmpDateTime
        Else
            Return dateTimeDefault
        End If
    End Function
End Class

