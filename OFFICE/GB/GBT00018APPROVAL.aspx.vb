Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' SOA承認画面クラス
''' </summary>
Public Class GBT00018APPROVAL
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00018A"   '自身のMAPID
    Private Const CONST_ORD_MAPID As String = "GBT00004"
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分

    Private Const CONST_VS_FILECNTDATA As String = "VSFILECNT" 'ファイル数保持用ビューステートデータ
    Private Const CONST_VS_ATTA_UNIQUEID As String = "ATTA_UNIQUEID"
    Private Const CONST_VS_PREV_ATTACHMENTINFO As String = "PREV_ATTACHMENTINFO"
    Private Const CONST_VS_CURR_ATTACHMENTINFO As String = "CURR_ATTACHMENTINFO"

    'アップロードファイルルート
    Private Const CONST_DIRNAME_SOAREP_UPROOT As String = "SOAREPORT" 'SOAふぁるアップロードルート

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 添付情報保持データテーブル
    ''' </summary>
    Private dtCurAttachment As DataTable
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

                'レポート設定
                Dim retMessageNo As String = RightboxInit()
                If retMessageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(retMessageNo, Me.lblFooterMessage)
                    Return
                End If
                '****************************************
                '表示非表示制御
                '****************************************
                DisplayControl()
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                If Me.hdnPrintFlg.Value = "1" Then
                    COA0031ProfMap.VARIANTP = Me.hdnPrevViewID.Value
                Else
                    COA0031ProfMap.VARIANTP = "GB_Default"
                End If

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
                If Me.hdnPrintFlg.Value = "1" Then
                    If COA0019Session.LANGDISP = C_LANG.JA Then
                        Me.txtApprovalObj.Text = "全て"
                    Else
                        Me.txtApprovalObj.Text = "All"
                    End If
                Else
                    If Me.hdnExtractApp.Value = "" Then
                        If COA0019Session.LANGDISP = C_LANG.JA Then
                            Me.txtApprovalObj.Text = "承認者"
                        Else
                            Me.txtApprovalObj.Text = "Approver"
                        End If
                    Else
                        Me.txtApprovalObj.Text = Me.hdnExtractApp.Value
                    End If
                End If

                'BillingMonth初期設定
                Me.lblBillingMonth.Text = Me.hdnBillingYmd.Value

                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetListDataTable()

                    For Each dr As DataRow In dt.Rows
                        GetAttachmentCnt(dr)
                        dr.Item("PRINTMONTHLOAD") = dr.Item("PRINTMONTH")
                        'フィルタ使用時の場合
                        If Convert.ToString(dr.Item("HIDDEN")) = "0" AndAlso Not (Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All") Then

                            ''条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                            'If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Then
                            '    dr.Item("HIDDEN") = 1
                            'Else
                            '    dr.Item("HIDDEN") = 0
                            'End If

                            If Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim OrElse
                                Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.EDITING OrElse
                                (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.REJECT AndAlso Trim(Convert.ToString(dr.Item("STEP"))) = Trim(Convert.ToString(dr.Item("LASTSTEP")))) Or
                                (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.APPROVED AndAlso Trim(Convert.ToString(dr.Item("STEP"))) = Trim(Convert.ToString(dr.Item("LASTSTEP"))) Or
                                (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.APPLYING AndAlso Trim(Convert.ToString(dr.Item("STEP"))) = Trim(Convert.ToString(dr.Item("CURSTEP")))) Or
                                (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.REVISE AndAlso Trim(Convert.ToString(dr.Item("STEP"))) = Trim(Convert.ToString(dr.Item("CURSTEP"))))) Then

                                dr.Item("HIDDEN") = 0
                            Else
                                dr.Item("HIDDEN") = 1

                            End If

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
                    Dim divDlCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DL")
                    Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
                    Dim tblDlCont As Table = DirectCast(divDlCont.Controls(0), Table)
                    Dim checkedValue As Boolean
                    If Me.hdnPrintFlg.Value <> "1" Then
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

                            If Not ((Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.APPLYING OrElse Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.REVISE) AndAlso (Trim(Convert.ToString(dr.Item("APPROVALTYPE"))) <> "")) Then
                                chk.Enabled = False
                            Else
                                chk.Enabled = True
                            End If
                            '直近承認済み解除は許可それ以外はボタンを使用不可
                            If Not ({C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE}.Contains((Trim(Convert.ToString(dr.Item("STATUS"))))) _
                               AndAlso Convert.ToString(dr.Item("HAS_FUTURE_CLOSINGREC")) = "0" _
                               AndAlso (Trim(Convert.ToString(dr.Item("APPROVALTYPE"))) <> "")) Then
                                Dim btnId As String = "btnWF_LISTAREAUNLOCKAPPROVE" & Convert.ToString(dr.Item("LINECNT"))
                                Dim btnObj As HtmlButton = DirectCast(tblDlCont.FindControl(btnId), HtmlButton)
                                If btnObj IsNot Nothing Then
                                    btnObj.Disabled = True
                                End If
                            End If
                        Next
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
                If Me.hdnPrintFlg.Value = "1" Then
                    Me.dtCurAttachment = CollectDispAttachmentInfo()
                End If

                If messageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                                messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})
                    Return
                End If
                '**********************
                ' テキストボックス変更判定
                '**********************
                If Me.hdnOnchangeField IsNot Nothing AndAlso Me.hdnOnchangeField.Value <> "" Then
                    Dim fieldName As String = Me.hdnOnchangeField.Value
                    If Me.hdnOnchangeField.Value.StartsWith("txtWF_LISTAREAPRINTMONTH") Then
                        fieldName = "txtWF_LISTAREAPRINTMONTH"
                    End If
                    'テキストID + "_Change"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)
                    Dim btnEventName As String = fieldName & "_Change"
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
                ''**********************
                '' 一覧表の行ダブルクリック判定
                ''**********************
                'If Me.hdnListDBclick.Value <> "" Then
                '    ListRowDbClick()
                '    Me.hdnListDBclick.Value = ""
                'End If
                '**********************
                ' 承認理由入力ボックス表示
                '**********************
                If Me.hdnRemarkboxField.Value <> "" Then
                    DisplayApplyReason(True)
                    Me.divRemarkInputBoxWrapper.Style("display") = "block"
                End If
                If Me.hdnPrintFlg.Value = "1" Then
                    '**********************
                    ' ファイルアップロード処理
                    '**********************
                    If Me.hdnListUpload.Value IsNot Nothing AndAlso Me.hdnListUpload.Value <> "" Then
                        UploadAttachment()
                        Me.hdnListUpload.Value = ""
                    End If
                    '**********************
                    ' 添付ファイル内容表示処理
                    '**********************
                    If Me.hdnFileDisplay.Value IsNot Nothing AndAlso Me.hdnFileDisplay.Value <> "" Then
                        AttachmentFileNameDblClick()
                        hdnFileDisplay.Value = ""
                    End If

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
            If Me.hdnPrintFlg.Value = "1" Then
                ViewState(CONST_VS_CURR_ATTACHMENTINFO) = Me.dtCurAttachment
            End If

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
                '承認ビュー表示切替
                Case Me.vLeftApprovalObj.ID
                    SetApprovalObjListItem(Me.txtApprovalObj.Text)

                '出力年月
                Case Me.vLeftPrintMonth.ID
                    SetPrintMonth(True)
                Case Else
                    SetFixvalueListItem("GENERALFLG", Me.lbYesNo)
                    Dim dicListId As New Dictionary(Of String, ListBox) _
                        From {{Me.vLeftYesNo.ID, Me.lbYesNo}}

                    If dicListId.ContainsKey(changeViewObj.ID) = False Then
                        Return
                    End If
                    Dim targetListObj = dicListId(changeViewObj.ID)
                    targetListObj.SelectedIndex = -1
                    targetListObj.Focus()

                    If Me.hdnTextDbClickField.Value.StartsWith("repAttachment_txtDeleteFlg_") Then
                        Dim drIndex As Integer = CInt(Me.hdnTextDbClickField.Value.Replace("repAttachment_txtDeleteFlg_", ""))
                        Dim dtAttachment As DataTable = Me.dtCurAttachment
                        Dim drTargetAttachmentRow = dtAttachment.Rows(drIndex)
                        Dim findLbValue As ListItem = targetListObj.Items.FindByValue(Convert.ToString(drTargetAttachmentRow("DELFLG")))
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
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
        COA0011ReturnUrl.VARI = Me.hdnPrevViewID.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
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
                        Where Trim(Convert.ToString(item("STATUS"))).Trim = C_APP_STATUS.REVISE _
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
        For Each checkedDr As DataRow In checkedDt.Rows

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

            EntryACWork(Convert.ToString(checkedDr.Item("COUNTRYCODE")), Convert.ToString(checkedDr.Item("BILLINGYMD")))


            If Convert.ToString(checkedDr.Item("LASTSTEP")) = Convert.ToString(checkedDr.Item("STEP")) Then

                ' 最終承認の場合メール送信
                Dim GBA00009MailSendSet As New GBA00009MailSendSet
                GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                GBA00009MailSendSet.MAILSUBCODE = ""
                GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
                GBA00009MailSendSet.APPLYSTEP = Convert.ToString(checkedDr.Item("STEP"))
                GBA00009MailSendSet.EVENTCODE = C_SCLOSEEVENT.APPROVALOK
                GBA00009MailSendSet.GBA00009setMailToBliingClose()

                If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                    'CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
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
            'If COA0019Session.LANGDISP = C_LANG.JA Then
            '    Me.txtApprovalObj.Text = "承認者"
            'Else
            '    Me.txtApprovalObj.Text = "Approver"
            'End If
        End If
        Me.hdnExtractApp.Value = Me.txtApprovalObj.Text

        If errNo <> "" Then
            CommonFunctions.ShowMessage(errNo, Me.lblFooterMessage)
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
                Case Me.vLeftPrintMonth.ID
                    '業者選択時
                    If Me.lbPrintMonth.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        SetPrintMonth(False, Me.lbPrintMonth.SelectedValue)
                    End If
                Case Else
                    If Me.hdnTextDbClickField.Value.StartsWith("repAttachment_txtDeleteFlg_") Then
                        Dim drIndex As Integer = CInt(Me.hdnTextDbClickField.Value.Replace("repAttachment_txtDeleteFlg_", ""))
                        Dim dtAttachment As DataTable = Me.dtCurAttachment
                        Dim drTargetAttachmentRow = dtAttachment.Rows(drIndex)

                        If Me.lbYesNo.SelectedItem IsNot Nothing Then
                            drTargetAttachmentRow("DELFLG") = Me.lbYesNo.SelectedValue
                        Else
                            drTargetAttachmentRow("DELFLG") = ""
                        End If
                        Me.repAttachment.DataSource = dtAttachment
                        Me.repAttachment.DataBind()
                        Exit Select
                    End If

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
    ''' 添付ファイルボックスのキャンセルボタン押下時イベント
    ''' </summary>
    Public Sub btnAttachmentUploadOk_Click()
        '添付ファイルに動きがあったかチェック
        If HasModifiedAttachmentFile() Then
            Dim attaUniqueIdx As String = Convert.ToString(ViewState(CONST_VS_ATTA_UNIQUEID))
            '動きがある場合添付ファイルを正式フォルダに転送
            CommonFunctions.SaveAttachmentFilesList(Me.dtCurAttachment, attaUniqueIdx, CONST_DIRNAME_SOAREP_UPROOT)
        End If

        Me.hdnRemarkboxOpen.Value = ""
        Me.hdnRemarkboxField.Value = ""
        Me.hdnCurrentUnieuqIndex.Value = ""
        'マルチライン入力ボックスの非表示
        Me.divAttachmentInputAreaWapper.Style("display") = "none"

    End Sub

    ''' <summary>
    ''' 添付ファイルボックスのキャンセルボタン押下時イベント
    ''' </summary>
    Public Sub btnAttachmentUploadCancel_Click()

        Me.hdnRemarkboxOpen.Value = ""
        Me.hdnRemarkboxField.Value = ""
        Me.hdnCurrentUnieuqIndex.Value = ""
        'マルチライン入力ボックスの非表示
        Me.divAttachmentInputAreaWapper.Style("display") = "none"

    End Sub
    ''' <summary>
    ''' 添付ファイルポップアップ-ダウンロードボタン押下時
    ''' </summary>
    Public Sub btnDownloadFiles_Click()
        Dim dtAttachment As DataTable = Me.dtCurAttachment
        Dim aTTauniqueId As String = Convert.ToString(ViewState(CONST_VS_ATTA_UNIQUEID)).Replace("\", "")
        'ダウンロード対象有無
        If dtAttachment Is Nothing OrElse dtAttachment.Rows.Count = 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.FILENOTEXISTS, Me.lblFooterMessage, pageObject:=Me)
        End If
        Dim dlUrl As String = CommonFunctions.GetAttachmentCompressedFileUrl(dtAttachment, aTTauniqueId)
        If dlUrl <> "" Then
            Me.hdnPrintURL.Value = dlUrl
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
        End If
        '終了メッセージ
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDOWNLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
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
    ''' 一覧表年月変更時イベント
    ''' </summary>
    Public Sub txtWF_LISTAREAPRINTMONTH_Change()

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

        AddLangSetting(dicDisplayText, Me.lblApprovalObjLabel, "承認種別", "Approval Type")
        AddLangSetting(dicDisplayText, Me.lblBillingMonthLabel, "請求月", "Billing Month")

        AddLangSetting(dicDisplayText, Me.lblAttachCounryTitle, "国", "Country")
        AddLangSetting(dicDisplayText, Me.lblAttachMonthTitle, "対象月", "Month")
        '****************************************
        ' 添付ファイルヘッダー部
        '****************************************
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderText, "添付書類を登録する場合は、ここにドロップすること", "To register attached documents, drop it here")
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderFileName, "ファイル名", "FileName")
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderDelete, "削 除", "Delete")
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

        'ソート順取得
        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = Me.hdnPrevViewID.Value
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
        'sqlStat.AppendLine("      ,TIMSTP = cast(CT.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,CT.COUNTRYCODE AS COUNTRYCODE ")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(CT.NAMESJP,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(CT.NAMES,'') END As COUNTRYNAME")
        sqlStat.AppendLine("      ,ISNULL(CL.REPORTMONTH,'') AS BILLINGYMD ")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1, FV3.VALUE1) ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2, FV3.VALUE2) END As APPROVALORREJECT")
        sqlStat.AppendLine("      ,'' AS OUTPUT ")
        sqlStat.AppendLine("      ,'' AS ""CHECK""")
        sqlStat.AppendLine("      ,ISNULL(AH.APPROVEDTEXT,'') AS APPROVEDTEXT")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN CASE WHEN ISNULL(US.STAFFNAMES,'') = '' THEN ISNULL(AH4.APPROVERID,'') ELSE US.STAFFNAMES END ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN CASE WHEN ISNULL(US.STAFFNAMES_EN,'') = '' THEN ISNULL(AH4.APPROVERID,'') ELSE US.STAFFNAMES_EN END END AS APPROVERID")
        sqlStat.AppendLine("      ,ISNULL(CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN CASE WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN ISNULL(FV1.VALUE1,'') + '+' ")
        sqlStat.AppendLine("                                            ELSE CASE WHEN AH.STEP > ISNULL(AH2.STEP,'" & C_APP_FIRSTSTEP & "') THEN ISNULL(FV1.VALUE1,'') + '*' ")
        sqlStat.AppendLine("                                            ELSE ISNULL(FV1.VALUE1,'') END END ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN CASE WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN ISNULL(FV1.VALUE2,'') + '+'  ")
        sqlStat.AppendLine("                                            ELSE CASE WHEN AH.STEP > ISNULL(AH2.STEP,'" & C_APP_FIRSTSTEP & "') THEN ISNULL(FV1.VALUE2,'') + '*' ")
        sqlStat.AppendLine("                                            ELSE ISNULL(FV1.VALUE2,'') END END END ,'') AS APPROVALOBJECT ")
        sqlStat.AppendLine("      ,ISNULL(AH.STEP,'') As STEP")
        sqlStat.AppendLine("      ,CASE WHEN (AH3.STEP = CL.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
        sqlStat.AppendLine("            WHEN (AH3.STEP = CL.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
        sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH3.STEP,'00'))))) + '/' + trim(convert(char,convert(int,CL.LASTSTEP))) END as STEPSTATE")
        sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
        sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
        sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END As CURSTEP")
        sqlStat.AppendLine("      ,ISNULL(AH.APPLYID,'') AS APPLYID ")
        sqlStat.AppendLine("      ,ISNULL(AH.STATUS,'" & C_APP_STATUS.EDITING & "') AS STATUS")

        sqlStat.AppendLine("      ,CL.APPLYOFFICE AS APPLYOFFICE")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(TR.NAMELJP,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(TR.NAMEL,'') END As OFFICENAME")

        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(USN.STAFFNAMES,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(USN.STAFFNAMES_EN,'') END As APPLYUSER")

        sqlStat.AppendLine("      ,EX.CURRENCYCODE AS CURRENCYCODE")
        sqlStat.AppendLine("      ,EX.EXRATE AS LOCALRATE")

        sqlStat.AppendLine("      ,ISNULL(AH.SUBCODE,'') AS SUBCODE")
        sqlStat.AppendLine("      ,ISNULL(AP.APPROVALTYPE,'') AS APPROVALTYPE")

        sqlStat.AppendLine("      ,ISNULL(CL.LASTSTEP,'') AS LASTSTEP")

        sqlStat.AppendLine("      ,CASE WHEN ISNULL(CL.APPLYID,'') = '' THEN '' ELSE FORMAT(CL.UPDYMD,'yyyy/MM/dd HH:mm') END AS CLOSEDATE")

        sqlStat.AppendLine("      ,(SELECT  MAX(REPORTMONTH) FROM GBT0006_CLOSINGDAY  PCL ")
        sqlStat.AppendLine("        INNER JOIN COT0002_APPROVALHIST APL ")
        sqlStat.AppendLine("                ON PCL.APPLYID = APL.APPLYID")
        sqlStat.AppendLine("               AND PCL.LASTSTEP = APL.STEP")
        sqlStat.AppendLine("               AND APL.STATUS IN ('" & C_APP_STATUS.APPROVED & "','" & C_APP_STATUS.COMPLETE & "')")
        sqlStat.AppendLine("               AND CT.COUNTRYCODE = PCL.COUNTRYCODE")
        sqlStat.AppendLine("         WHERE PCL.DELFLG <> @DELFLG ) as PRINTMONTH")
        sqlStat.AppendLine("      ,'' AS PRINTMONTHLOAD ")
        sqlStat.AppendLine("      ,'' AS [PRINT] ")
        If Me.hdnBillingYmd.Value <> "" Then
            sqlStat.AppendLine("      ,(SELECT CASE WHEN COUNT(CLSS.REPORTMONTH)=0 THEN '0' ELSE '1' END ") '未来の締め月を持っているか？（持っている場合戻させない）
            sqlStat.AppendLine("          FROM GBT0006_CLOSINGDAY CLSS")
            sqlStat.AppendLine("         WHERE CLSS.COUNTRYCODE = CT.COUNTRYCODE ")
            sqlStat.AppendLine("           AND CLSS.REPORTMONTH > @BILLINGYMD ) AS HAS_FUTURE_CLOSINGREC")
        Else
            sqlStat.AppendLine("      ,'' AS HAS_FUTURE_CLOSINGREC ") 'メニューSOA Reportからの遷移時想定
        End If
        sqlStat.AppendLine("      ,'' AS  ATTACHMENT")
        'sqlStat.AppendLine("  FROM GBM0001_COUNTRY CT") '国マスタ
        sqlStat.AppendLine("  FROM ( SELECT COMPCODE,COUNTRYCODE,NAMESJP,NAMES,STYMD,ENDYMD,DELFLG FROM GBM0001_COUNTRY")
        sqlStat.AppendLine("        UNION ALL SELECT @COMPCODE,'" & GBC_JOT_SOA_COUNTRY & "' AS COUNTRYCODE,'" & GBC_JOT_SOA_COUNTRY & "','" & GBC_JOT_SOA_COUNTRY & "',@STYMD AS STYMD,@ENDYMD AS ENDYMD,'" & CONST_FLAG_NO & "' AS DELFLG ) CT")

        sqlStat.AppendLine("  LEFT JOIN GBT0006_CLOSINGDAY CL")
        sqlStat.AppendLine("    ON  CT.COUNTRYCODE  = CL.COUNTRYCODE ")
        sqlStat.AppendLine("   AND  CL.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  CL.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  CL.DELFLG      <> @DELFLG")

        If Me.hdnBillingYmd.Value <> "" Then
            sqlStat.AppendLine("   AND CL.REPORTMONTH = @BILLINGYMD")
            'sqlStat.AppendLine("   AND FORMAT(DATEADD(month,1,CL.BILLINGYMD),'yyyy/MM') = @BILLINGYMD")
        End If
        If Me.hdnPrintFlg.Value = "1" Then
            sqlStat.AppendLine("   AND CL.REPORTMONTH = (SELECT MAX(CLS2.REPORTMONTH)")
            sqlStat.AppendLine("                           FROM GBT0006_CLOSINGDAY CLS2")
            sqlStat.AppendLine("                          WHERE CLS2.COUNTRYCODE  = CT.COUNTRYCODE")
            sqlStat.AppendLine("                            AND CLS2.STYMD       <= @STYMD")
            sqlStat.AppendLine("                            AND CLS2.ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("                            AND CLS2.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("                        )")
        End If
        sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH ")
        sqlStat.AppendLine("    ON  AH.APPLYID   = CL.APPLYID ")
        sqlStat.AppendLine("   And  AH.COMPCODE  = @COMPCODE ")
        sqlStat.AppendLine("   And  AH.DELFLG   <> @DELFLG")

        If Me.hdnPrintFlg.Value = "1" Then
            sqlStat.AppendLine("   And CL.LASTSTEP  = AH.STEP")
        End If

        sqlStat.AppendLine("  LEFT JOIN COS0022_APPROVAL AP") '承認設定マスタ
        sqlStat.AppendLine("    On  AP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   And  AP.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   And  AP.EVENTCODE    = AH.EVENTCODE")
        sqlStat.AppendLine("   And  AP.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("   And  AP.STEP         = AH.STEP")
        sqlStat.AppendLine("   And  AP.USERID       = @USERID")
        sqlStat.AppendLine("   And  AP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And  AP.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And  AP.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) As STEP")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) As AH2 ")
        sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MAX(STEP) As STEP ")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS  > '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> @DELFLG ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) As AH3 ")
        sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH3.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH3.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH4 ")
        sqlStat.AppendLine("    ON AH3.APPLYID = AH4.APPLYID ")
        sqlStat.AppendLine("   AND AH3.STEP    = AH4.STEP ")
        sqlStat.AppendLine("   AND AH4.DELFLG <> @DELFLG")
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

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR") '業者名称用JOIN
        sqlStat.AppendLine("    ON  TR.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  TR.CARRIERCODE  = CL.APPLYOFFICE")
        sqlStat.AppendLine("   AND  TR.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  TR.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  TR.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE EX") '通貨用JOIN
        sqlStat.AppendLine("    ON  EX.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  EX.COUNTRYCODE  = CT.COUNTRYCODE")
        sqlStat.AppendLine("   AND  EX.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  EX.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  EX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  EX.TARGETYM     = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")

        sqlStat.AppendLine("  LEFT JOIN COS0005_USER USN") 'ユーザー名用JOIN
        sqlStat.AppendLine("    ON  USN.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  USN.USERID       = CL.APPLYUSER")
        sqlStat.AppendLine("   AND  USN.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  USN.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  USN.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV3") '承認否認名称用JOIN
        sqlStat.AppendLine("    ON  FV3.COMPCODE     = '" & GBC_COMPCODE_D & "'")
        sqlStat.AppendLine("   AND  FV3.SYSCODE      = '" & C_SYSCODE_GB & "'")
        sqlStat.AppendLine("   AND  FV3.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FV3.KEYCODE      = '" & C_APP_STATUS.EDITING & "'")
        sqlStat.AppendLine("   AND  FV3.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  FV3.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  FV3.DELFLG      <> @DELFLG")

        sqlStat.AppendLine(" WHERE CT.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("   AND CT.STYMD  <= @STYMD")
        sqlStat.AppendLine("   AND CT.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("   AND CT.DELFLG <> @DELFLG ")

        If Me.hdnPrintFlg.Value = "1" Then
            'If Not GBA00003UserSetting.IS_JOTUSER Then
            If Not GBA00003UserSetting.IS_JOTUSER AndAlso Not GBA00003UserSetting.IS_AGENTTOPUSER Then
                sqlStat.AppendLine("   AND CT.COUNTRYCODE = '" & GBA00003UserSetting.COUNTRYCODE & "'")
            ElseIf GBA00003UserSetting.IS_AGENTTOPUSER Then
                ' Agent Topユーザの場合、JOTを除く
                sqlStat.AppendLine("   AND CT.COUNTRYCODE <> '" & GBC_JOT_SOA_COUNTRY & "'")
            End If
        End If

        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@USERID", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                .Add("@LANGDISP", SqlDbType.NVarChar, 20).Value = COA0019Session.LANGDISP
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@TARGETYM", SqlDbType.Date).Value = Date.Now

                If Me.hdnBillingYmd.Value <> "" Then
                    .Add("@BILLINGYMD", System.Data.SqlDbType.NVarChar).Value = FormatDateContrySettings(FormatDateYMD(Me.hdnBillingYmd.Value, GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
                End If
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return retDt
    End Function

    ''' <summary>
    ''' 帳票出力用のデータテーブルを取得する関数
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOutputListDataTable(ByVal selectedRow As DataRow) As DataTable
        'Dim mapId As String = CONST_ORD_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力

        Dim GBA00013SoaInfo As New GBA00013SoaInfo
        GBA00013SoaInfo.INVOICEDBYTYPE = "EJ"
        GBA00013SoaInfo.COUNTRYCODE = Convert.ToString(selectedRow.Item("COUNTRYCODE"))
        If Convert.ToString(selectedRow.Item("COUNTRYCODE")) = GBC_JOT_SOA_COUNTRY Then
            GBA00013SoaInfo.INVOICEDBYTYPE = "OJ"
            GBA00013SoaInfo.COUNTRYCODE = ""
        End If
        'GBA00013SoaInfo.REPORTMONTH = FormatDateContrySettings(FormatDateYMD(Me.lblBillingMonth.Text, GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
        If Me.hdnPrintFlg.Value = "1" Then
            GBA00013SoaInfo.REPORTMONTH = FormatDateContrySettings(FormatDateYMD(Convert.ToString(selectedRow.Item("PRINTMONTH")), GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
        Else
            GBA00013SoaInfo.REPORTMONTH = FormatDateContrySettings(FormatDateYMD(Me.lblBillingMonth.Text, GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
        End If
        GBA00013SoaInfo.SHOULDGETALLCOST = "1"
        GBA00013SoaInfo.GBA00013getSoaDataTable()
        If Not {C_MESSAGENO.NORMAL, C_MESSAGENO.NODATA}.Contains(GBA00013SoaInfo.ERR) Then
            Throw New Exception("GBA00013getSoaDataTable Error")
        End If

        Dim dtDbResult As DataTable = GBA00013SoaInfo.SOADATATABLE

        Dim retDt As DataTable = CreateOrderListTable()
        If dtDbResult Is Nothing OrElse dtDbResult.Rows Is Nothing OrElse dtDbResult.Rows.Count = 0 Then
            Dim writeDr As DataRow = retDt.NewRow
            SetHeaderValue(writeDr, selectedRow, True)
            retDt.Rows.Add(writeDr)
            Return retDt
        End If
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next
        Dim actyNo As Integer = 0
        Dim orderNo As String = Convert.ToString(dtDbResult.Rows(0).Item("ORDERNO"))
        Dim tankSeq As String = Convert.ToString(dtDbResult.Rows(0).Item("TANKSEQ"))
        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                If colName = "DISPSEQ" Then
                    writeDr.Item(colName) = Convert.ToString(readDr.Item(colName))
                Else
                    writeDr.Item(colName) = readDr.Item(colName)
                End If
            Next
            If Not (tankSeq.Equals(readDr.Item("TANKSEQ")) _
                    AndAlso orderNo.Equals(readDr.Item("ORDERNO"))) Then
                actyNo = 0
                orderNo = Convert.ToString(readDr.Item("ORDERNO"))
                tankSeq = Convert.ToString(readDr.Item("TANKSEQ"))
            End If
            actyNo = actyNo + 1
            writeDr.Item("ACTYNO") = actyNo.ToString("000")

            SetHeaderValue(writeDr, selectedRow, True)

            retDt.Rows.Add(writeDr)
        Next

        Return retDt

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
            GetAttachmentCnt(dt.Rows(i))
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
        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnPrevViewID.Value
            .SRCDATA = listData
            .TBLOBJ = Me.WF_LISTAREA
            .SCROLLTYPE = "2"
            .LEVENT = "ondblclick"
            .LFUNC = "ListDbClick"
            .TITLEOPT = True
            .NOCOLUMNWIDTHOPT = 50
            .OPERATIONCOLUMNWIDTHOPT = -1
            .USERSORTOPT = 1
        End With
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

        Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
        Dim divDlCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DL")
        Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
        Dim tblDlCont As Table = DirectCast(divDlCont.Controls(0), Table)
        'Dim checkedValue As Boolean
        For Each dr As DataRow In listData.Rows
            Dim chkId As String = "chkWF_LISTAREACHECK" & Convert.ToString(dr.Item("LINECNT"))
            Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
            If Not ((Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.APPLYING OrElse Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.REVISE) AndAlso (Trim(Convert.ToString(dr.Item("APPROVALTYPE"))) <> "")) Then
                chk.Enabled = False
            Else
                chk.Enabled = True
            End If
            '直近承認済み解除は許可それ以外はボタンを使用不可
            If Not ({C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE}.Contains((Trim(Convert.ToString(dr.Item("STATUS"))))) _
                  AndAlso Convert.ToString(dr.Item("HAS_FUTURE_CLOSINGREC")) = "0" _
                  AndAlso (Trim(Convert.ToString(dr.Item("APPROVALTYPE"))) <> "")) Then
                Dim btnId As String = "btnWF_LISTAREAUNLOCKAPPROVE" & Convert.ToString(dr.Item("LINECNT"))
                Dim btnObj As HtmlButton = DirectCast(tblDlCont.FindControl(btnId), HtmlButton)
                If btnObj IsNot Nothing Then
                    btnObj.Disabled = True
                End If
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
        With retDt.Columns
            '共通項目
            .Add("LINECNT", GetType(Integer))              'DBの固定フィールド
            .Add("OPERATION", GetType(String))             'DBの固定フィールド
            .Add("TIMSTP", GetType(String))                'DBの固定フィールド
            .Add("SELECT", GetType(Integer))               'DBの固定フィールド
            .Add("HIDDEN", GetType(Integer))

            '個別項目
            .Add("DATAID", GetType(String))                'データID
            .Add("ORDERNO", GetType(String))               '受注番号
            .Add("STYMD", GetType(String))                 '有効開始日
            .Add("ENDYMD", GetType(String))                '有効終了日
            .Add("TANKSEQ", GetType(String))               '作業番号(タンクSEQ)
            .Add("DTLPOLPOD", GetType(String))             '発地着地区分
            .Add("DTLOFFICE", GetType(String))             '代理店
            .Add("TANKNO", GetType(String))                'タンク番号
            .Add("COSTCODE", GetType(String))              '費用コード
            .Add("ACTIONID", GetType(String))              'アクションコード
            .Add("DISPSEQ", GetType(String))               '表示順番
            .Add("LASTACT", GetType(String))               '輸送完了作業
            .Add("REQUIREDACT", GetType(String))           '必須作業
            .Add("ORIGINDESTINATION", GetType(String))     '起点終点
            .Add("COUNTRYCODE", GetType(String))           '国コード
            .Add("CURRENCYCODE", GetType(String))          '通貨換算コード
            .Add("TAXATION", GetType(String))              '課税フラグ
            .Add("AMOUNTBR", GetType(String))              '金額(BR)
            .Add("AMOUNTORD", GetType(String))             '金額(ORD)
            .Add("AMOUNTFIX", GetType(String))             '金額(FIX)
            .Add("CONTRACTORBR", GetType(String))          '業者コード(BR)
            .Add("CONTRACTORODR", GetType(String))         '業者コード(ORD)
            .Add("CONTRACTORFIX", GetType(String))         '業者コード(FIX)
            .Add("SCHEDELDATEBR", GetType(String))         '作業日(BR) 
            .Add("SCHEDELDATE", GetType(String))           '作業日(ORD)
            .Add("ACTUALDATE", GetType(String))            '作業日(FIX)
            .Add("LOCALBR", GetType(String))               '現地金額(BR)
            .Add("LOCALRATE", GetType(String))             '現地通貨換算レート
            .Add("TAXBR", GetType(String))                 '税(BR)
            .Add("AMOUNTPAY", GetType(String))             '金額(PAY)
            .Add("LOCALPAY", GetType(String))              '現地金額(PAY)
            .Add("TAXPAY", GetType(String))                '税(PAY)
            .Add("INVOICEDBY", GetType(String))            '船荷証券発行コード
            .Add("APPLYID", GetType(String))               '費用変更申請ID
            .Add("APPLYTEXT", GetType(String))             '申請コメント
            .Add("LASTSTEP", GetType(String))              '最終承認STEP
            .Add("SOAAPPDATE", GetType(String))            'SOA締日付
            .Add("REMARK", GetType(String))                '所見
            .Add("BLID", GetType(String))                  'BL番号
            .Add("BLAPPDATE", GetType(String))             'BL承認日
            .Add("BRID", GetType(String))                  'ブレーカーID
            .Add("BRCOST", GetType(String))                'ブレーカー起因費用
            .Add("DATEFIELD", GetType(String))             '予定日付参照
            .Add("DATEINTERVAL", GetType(String))          '予定日付加減算日数
            .Add("BRADDEDCOST", GetType(String))           'ブレーカーコスト追加フラグ
            .Add("AGENTORGANIZER", GetType(String))        'オーガナイザーエージェント
            .Add("DELFLG", GetType(String))                '削除フラグ
            .Add("REPORTMONTH", GetType(String))            '出力月
            .Add("SOACODE", GetType(String))                'SOAコード
            .Add("COUNTRYNAME", GetType(String))           '国名
            .Add("APPROVALOBJECT", GetType(String))        '承認対象(通常、代行、SKIP)
            .Add("APPROVALORREJECT", GetType(String))      '承認or否認
            .Add("CHECK", GetType(String))                 'チェック
            .Add("STEP", GetType(String))                  'ステップ
            .Add("STATUS", GetType(String))                'ステータス
            .Add("CURSTEP", GetType(String))               '承認ステップ
            .Add("STEPSTATE", GetType(String))             'ステップ状況
            .Add("APPROVALTYPE", GetType(String))          '承認区分
            .Add("APPROVERID", GetType(String))
            .Add("APPLYOFFICE", GetType(String))
            .Add("OFFICENAME", GetType(String))
            .Add("APPLYUSER", GetType(String))
            .Add("EVENTCODE", GetType(String))
            .Add("APPLYDATE", GetType(String))
            .Add("SUBCODE", GetType(String))
            .Add("CLOSEDATE", GetType(String))

            .Add("PRINTMONTH", GetType(String))
            .Add("PRINTMONTHLOAD", GetType(String))
            .Add("PRINT", GetType(String))
        End With

        Return retDt
    End Function

    ''' <summary>
    ''' 一覧表示用のデータテーブルを作成
    ''' </summary>
    ''' <returns>TODOまだイマジネーションのため揉む必要あり</returns>
    Private Function CreateOrderListTable() As DataTable
        Dim retDt As New DataTable
        With retDt.Columns
            '固定部分は追加しておく
            .Add("LINECNT", GetType(Integer))            'DBの固定フィールド
            .Add("OPERATION", GetType(String)).DefaultValue = ""           'DBの固定フィールド
            .Add("TIMSTP", GetType(String)).DefaultValue = ""              'DBの固定フィールド
            .Add("SELECT", GetType(Integer))             'DBの固定フィールド
            .Add("HIDDEN", GetType(Integer))
            .Add("DATAID", GetType(String)).DefaultValue = ""
            .Add("SYSKEY", GetType(String)).DefaultValue = ""
            .Add("REPORTMONTH", GetType(String)).DefaultValue = ""         '出力月
            .Add("SOACODE", GetType(String)).DefaultValue = ""             'SOAコード
            Dim colList As New List(Of String) From {"ORDERNO", "BRTYPE", "TANKSEQ", "DTLPOLPOD", "DTLOFFICE", "TANKNO", "COSTCODE", "COSTNAME", "ACTIONID", "DISPSEQ", "LASTACT",
                                                 "AMOUNTBR", "AMOUNTORD", "AMOUNTFIX", "CONTRACTORBR", "CONTRACTORODR", "CONTRACTORFIX", "SCHEDELDATEBR", "SCHEDELDATE", "ACTUALDATE",
                                                 "APPLYID", "APPLYTEXT", "LASTSTEP", "STATUS", "BRID", "BRCOST", "ACTYNO", "AGENTKBNSORT", "USETYPE", "DISPSEQISEMPTY", "APPLY",
                                                 "INVOICEDBY", "AGENTORGANIZER", "DELFLG",
                                                 "IS_ODR_CHANGECOST", "IS_FIX_CHANGECOST",
                                                 "IS_CALC_DEMURRAGE", "TIP", "DEMURTO", "DEMURUSRATE1", "DEMURUSRATE2",
                                                 "CHARGE_CLASS1", "CHARGE_CLASS4", "LOCALRATE", "CURRENCYCODE",
                                                 "AGENT", "ORGOFFICE", "OTHEROFFICE", "COUNTRYCODE",
                                                 "EXRATE", "REFAMOUNT", "AMOUNTPAY", "LOCALPAY", "SOAAPPDATE",
                                                 "IS_UPDATE_SHIPDATE", "ORIGINDESTINATION", "COMMAMOUNT",
                                                 "CONTRACTORNAMEBR", "CONTRACTORNAMEODR", "CONTRACTORNAMEFIX",
                                                 "BILLINGYMD", "ISBILLINGCLOSED", "USDAMOUNT", "LOCALAMOUNT", "REPORTYMD",
                                                 "JOT", "ISAUTOCLOSE", "ISAUTOCLOSELONG", "DISPLAYCURRANCYCODE", "TAXATION", "TAXRATE", "TAXRATE_L", "SOARATE", "EXSHIPRATE_1", "EXSHIPRATE_2",
                                                 "REPORTMONTHH", "COUNTRYNAMEH", "OFFICENAMEH", "APPLYUSERH", "CURRENCYCODEH", "LOCALRATEH",
                                                 "REPORTMONTHORG", "DATA", "JOTCODE", "ACCODE", "LOCALRATESOA", "AMOUNTPAYODR", "LOCALPAYODR",
                                                 "UAG_USD", "UAG_LOCAL", "USD_USD", "USD_LOCAL", "LOCAL_USD", "LOCAL_LOCAL",
                                                 "FINALREPORTNOH", "CLOSEDATEH", "PRINTDATEH", "REMARK"}

            For Each colName As String In colList
                .Add(colName, GetType(String)).DefaultValue = ""
            Next
        End With
        Return retDt
    End Function
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

        'リストに存在しない場合、エラー
        SetApprovalObjListItem(Me.txtApprovalObj.Text)
        If Not CheckList(txtApprovalObj.Text, lbApprovalObj) Then
            CommonFunctions.ShowMessage(C_MESSAGENO.INVALIDINPUT, Me.lblFooterMessage, pageObject:=Me)
            Me.txtApprovalObj.Focus()
            Return
        End If

        'フィルタでの絞り込みを利用するか確認
        Dim isFillterOffApp As Boolean = True
        If Me.txtApprovalObj.Text.Trim <> "" Then
            isFillterOffApp = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOffApp = False AndAlso Not (Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All") Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                'If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Then
                '    dr.Item("HIDDEN") = 1
                'End If

                If Not (Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Or
                                Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.EDITING Or
                                (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.REJECT AndAlso Trim(Convert.ToString(dr.Item("STEP"))) = Trim(Convert.ToString(dr.Item("LASTSTEP")))) Or
                                (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.APPROVED AndAlso Trim(Convert.ToString(dr.Item("STEP"))) = Trim(Convert.ToString(dr.Item("LASTSTEP")))) Or
                                (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.APPLYING AndAlso Trim(Convert.ToString(dr.Item("STEP"))) = Trim(Convert.ToString(dr.Item("CURSTEP")))) Or
                                (Trim(Convert.ToString(dr.Item("STATUS"))) = C_APP_STATUS.REVISE AndAlso Trim(Convert.ToString(dr.Item("STEP"))) = Trim(Convert.ToString(dr.Item("CURSTEP"))))) Then
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

            '請求日を戻す処理実行
            BackBillingDate(Convert.ToString(checkedDr.Item("COUNTRYCODE")), Convert.ToString(checkedDr.Item("BILLINGYMD")))

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
            End If

            '承認コメント更新処理
            UpdateApprovedText(Convert.ToString(HttpContext.Current.Session("APSRVCamp")), Convert.ToString(checkedDr.Item("APPLYID")),
                               Convert.ToString(checkedDr.Item("STEP")), Convert.ToString(checkedDr.Item("APPROVEDTEXT")))


            ' 最終承認の場合メール送信
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            GBA00009MailSendSet.APPLYSTEP = Convert.ToString(checkedDr.Item("STEP"))
            GBA00009MailSendSet.EVENTCODE = C_SCLOSEEVENT.APPROVALNG
            GBA00009MailSendSet.GBA00009setMailToBliingClose()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
                'Return
                If errNo = "" Then
                    errNo = GBA00009MailSendSet.ERR
                End If
            End If
        Next

        '絞り込み
        If Me.txtApprovalObj.Text = "" Then
            'If COA0019Session.LANGDISP = C_LANG.JA Then
            '    Me.txtApprovalObj.Text = "承認者"
            'Else
            '    Me.txtApprovalObj.Text = "Approver"
            'End If
        End If
        Me.hdnExtractApp.Value = Me.txtApprovalObj.Text

        If errNo <> "" Then
            CommonFunctions.ShowMessage(errNo, Me.lblFooterMessage)
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

        If TypeOf Page.PreviousPage Is GBT00018APPROVAL Then

            Dim prevPage As GBT00018APPROVAL = DirectCast(Page.PreviousPage, GBT00018APPROVAL)
            '実行画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnMsgId", Me.hdnMsgId},
                                                                    {"hdnBillingYmd", Me.hdnBillingYmd},
                                                                    {"hdnPrevViewID", Me.hdnPrevViewID},
                                                                    {"hdnExtractApp", Me.hdnExtractApp}}

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    dicObj.Value.Value = tmphdnObj.Value
                End If
            Next

        ElseIf TypeOf Page.PreviousPage Is GBT00018SELECT Then

            Dim prevObj As GBT00018SELECT = DirectCast(Page.PreviousPage, GBT00018SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtBillingYmd", Me.hdnBillingYmd}
                                                                        }

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    ElseIf TypeOf tmpCont Is RadioButtonList Then
                        Dim tmpRbl As RadioButtonList = DirectCast(tmpCont, RadioButtonList)
                        item.Value.Value = tmpRbl.SelectedValue
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

            'メニューから遷移
        ElseIf Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00018SELECT Then
            'メニューからの画面遷移

            Me.hdnPrintFlg.Value = "1"
            Me.hdnBillingYmd.Value = ""
            Me.hdnPrevViewID.Value = "GB_PRINT"

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
                .Add("@APPROVEDTEXT", SqlDbType.NVarChar, 1024).Value = parmApprovedText
                .Add("@UPDYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = parmCompCode
                .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = parmApplyId
                .Add("@STEP", SqlDbType.NVarChar, 20).Value = parmStep
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
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
            'sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
            sqlStat.AppendLine("  FROM GBT0008_JOTSOA_VALUE")
            sqlStat.AppendLine(" WHERE APPLYID   = @APPLYID")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = applyId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
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
        Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String) From {{"CHECK", objChkPrifix}, {"PRINTMONTH", objTxtPrifix}}

        Dim formToPost = New NameValueCollection(Request.Form)
        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    formToPost.Remove(dispObjId)
                End If
                If fieldId.Key = "PRINTMONTH" Then
                    displayValue = BaseDllCommon.FormatDateYMD(displayValue, GBA00003UserSetting.DATEYMFORMAT)
                    displayValue = BaseDllCommon.FormatDateContrySettings(displayValue, "yyyy/MM")
                    If displayValue = "" Then
                        displayValue = Convert.ToString(dt.Rows(i - 1).Item(fieldId.Key))
                    End If
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
    ''' 一覧出力ボタン押下時
    ''' </summary>
    Public Sub btnListOutput_Click()

        '帳票出力
        Dim rowIdString As String = Me.hdnListCurrentRownum.Value
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
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim outputDt As DataTable = New DataTable

        If Convert.ToString(selectedRow.Item("PRINTMONTH")) = "" Then
            CommonFunctions.ShowMessage("30001", Me.lblFooterMessage, pageObject:=Me)
            Return
        End If


        If Trim(Convert.ToString(selectedRow.Item("STATUS"))) = C_APP_STATUS.EDITING OrElse
            Trim(Convert.ToString(selectedRow.Item("STATUS"))) = C_APP_STATUS.REJECT OrElse
            Convert.ToString(selectedRow.Item("PRINTMONTH")) > Convert.ToString(selectedRow.Item("PRINTMONTHLOAD")) Then
            outputDt = GetOutputListDataTable(selectedRow)
        Else
            outputDt = GetOutputJOTSOAListDataTable(selectedRow)
        End If

        Using outputDt

            'If outputDt IsNot Nothing AndAlso outputDt.Rows.Count = 0 Then
            '    CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage, pageObject:=Me)
            '    Return
            'End If

            '帳票出力
            With Nothing
                Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
                Dim reportId As String = Me.lbRightList.SelectedValue
                Dim reportMapId As String = CONST_MAPID
                COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
                COA0027ReportTable.TBLDATA = outputDt                              'PARAM04:データ参照tabledata
                COA0027ReportTable.COA0027ReportTable()

                If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                Else
                    CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If

                '別画面でExcelを表示
                hdnPrintURL.Value = COA0027ReportTable.URL
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            End With
        End Using

    End Sub

    ''' <summary>
    ''' 一覧PDF出力ボタン押下時
    ''' </summary>
    Public Sub btnListPrint_Click()

        '帳票出力
        Dim rowIdString As String = Me.hdnListCurrentRownum.Value
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
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim outputDt As DataTable = New DataTable

        If Convert.ToString(selectedRow.Item("PRINTMONTH")) = "" Then
            CommonFunctions.ShowMessage("30001", Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Dim clDt As DataTable = GetClosingDay(Convert.ToString(selectedRow.Item("COUNTRYCODE")))
        With Me.lbPrintMonth
            .DataSource = clDt
            .DataTextField = "REPORTMONTH"
            .DataValueField = "REPORTMONTH"
            .DataBind()
            .Focus()
        End With

        If Me.lbPrintMonth.Items.Count > 0 Then
            Dim findListItem = Me.lbPrintMonth.Items.FindByValue(Convert.ToString(selectedRow.Item("PRINTMONTH")))
            If findListItem Is Nothing Then
                CommonFunctions.ShowMessage(C_MESSAGENO.INVALIDINPUT, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        End If

        'If Trim(Convert.ToString(selectedRow.Item("STATUS"))) = C_APP_STATUS.EDITING OrElse
        '    Trim(Convert.ToString(selectedRow.Item("STATUS"))) = C_APP_STATUS.REJECT Then
        '    outputDt = GetOutputListDataTable(selectedRow)
        'Else
        outputDt = GetOutputJOTSOAListDataTable(selectedRow)
        'End If

        Using outputDt

            'If outputDt IsNot Nothing AndAlso outputDt.Rows.Count = 0 Then
            'CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage, pageObject:=Me)
            'Return
            'End If

            '帳票出力
            With Nothing
                Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
                Dim reportId As String = Me.lbRightList.SelectedValue
                Dim reportMapId As String = CONST_MAPID
                COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                COA0027ReportTable.FILETYPE = "pdf"                                'PARAM03:出力ファイル形式
                COA0027ReportTable.TBLDATA = outputDt                              'PARAM04:データ参照tabledata
                COA0027ReportTable.COA0027ReportTable()

                If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                Else
                    CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If

                '別画面でExcelを表示
                hdnPrintURL.Value = COA0027ReportTable.URL
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
            End With
        End Using

    End Sub
    ''' <summary>
    ''' 承認解除ボタン押下時
    ''' </summary>
    Public Sub btnListUnlockApprove_Click()
        '帳票出力
        Dim rowIdString As String = Me.hdnListCurrentRownum.Value
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
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        Dim selectedRow As DataRow = dt.Rows(rowId)

        BackBillingDate(Convert.ToString(selectedRow.Item("COUNTRYCODE")), Convert.ToString(selectedRow.Item("BILLINGYMD")), True)

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
    ''' 国、月でのアップロード済ファイル数を取得
    ''' </summary>
    Private Sub GetAttachmentCnt(dr As DataRow)
        'SOA Reportでの呼出し以外は無意味なのでスキップ
        If Me.hdnPrintFlg.Value <> "1" Then
            Return
        End If
        '一旦添付ファイル情報フィールドをクリア
        dr("ATTACHMENT") = ""
        'コピー元のディレクトリ取得
        Dim reportMonth As String = Convert.ToString(dr("PRINTMONTH")).Replace("/", "")
        Dim countryCode As String = Convert.ToString(dr("COUNTRYCODE"))
        If reportMonth = "" Then
            Return
        End If
        '対象のファイル有無取得
        Dim upBaseDir As String = COA0019Session.UPLOADFILESDir '承認機能がある際はこちら考慮 COA0019Session.BEFOREAPPROVALDir
        Dim uploadPath As String = IO.Path.Combine(upBaseDir, CONST_DIRNAME_SOAREP_UPROOT, countryCode, reportMonth)
        'フォルダ自体未存在
        If IO.Directory.Exists(uploadPath) = False Then
            Return
        End If
        '対象ディレクトリのファイル情報取得
        Dim filesObj = IO.Directory.GetFiles(uploadPath)
        If filesObj Is Nothing OrElse filesObj.Count = 0 Then
            Return
        End If
        'ここまで来た場合はファイル存在あり
        dr("ATTACHMENT") = String.Format("{0} File", filesObj.Count)
    End Sub
    ''' <summary>
    ''' 添付ファイル保持用のクラス
    ''' </summary>
    ''' <remarks>国、月でのアップロード済ファイル数を保持</remarks>
    Public Class AttachmentFileCount
        ''' <summary>
        ''' 国コード
        ''' </summary>
        ''' <returns></returns>
        Public Property CountryCode As String
        ''' <summary>
        ''' 月
        ''' </summary>
        ''' <returns></returns>
        Public Property Month As String
        ''' <summary>
        ''' ファイル数
        ''' </summary>
        ''' <returns></returns>
        Public Property FileCount As Integer

    End Class

    ''' <summary>
    ''' 添付ファイルの変更有無チェック
    ''' </summary>
    ''' <returns>True:変更あり,False:変更なし</returns>
    Private Function HasModifiedAttachmentFile() As Boolean
        '添付ファイルの個数判定
        Dim prevAttachDt As DataTable = DirectCast(ViewState(CONST_VS_PREV_ATTACHMENTINFO), DataTable)
        Dim dispAttachDt = Me.dtCurAttachment

        With Nothing
            Dim dispAttachFileCnt As Integer = 0
            Dim prevAttachFileCnt As Integer = 0
            If dispAttachDt IsNot Nothing Then
                dispAttachFileCnt = dispAttachDt.Rows.Count
            End If
            If prevAttachDt IsNot Nothing Then
                prevAttachFileCnt = prevAttachDt.Rows.Count
            End If
            If prevAttachFileCnt <> dispAttachFileCnt Then
                '添付ファイルの数値が合わない場合は変更あり
                Return True
            End If
        End With
        'フィールド変更チェック
        Dim chkAttachFields As New List(Of String) From {"FILENAME", "DELFLG", "ISMODIFIED"}
        Dim maxRowIdx As Integer = dispAttachDt.Rows.Count - 1
        For rowIdx = 0 To maxRowIdx Step 1
            Dim dispDr As DataRow = dispAttachDt.Rows(rowIdx)
            Dim prevDr As DataRow = prevAttachDt.Rows(rowIdx)
            For Each fieldName In chkAttachFields
                If Not dispDr(fieldName).Equals(prevDr(fieldName)) Then
                    '対象フィールドの値に変更があった場合
                    Return True
                End If
            Next fieldName 'フィールドループ
        Next 'データテーブル行ループ
        'ここまでくれば変更なし
        Return False
    End Function
    ''' <summary>
    ''' 画面入力情報を取得しデータセットに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function CollectDispAttachmentInfo() As DataTable
        Dim dt As DataTable = DirectCast(ViewState(CONST_VS_CURR_ATTACHMENTINFO), DataTable)
        If dt Is Nothing Then
            Return Nothing
        End If
        '添付ファイルの収集
        Dim dtAttachment As DataTable = CommonFunctions.DeepCopy(dt)
        For Each repItem As RepeaterItem In Me.repAttachment.Items
            Dim fileName As Label = DirectCast(repItem.FindControl("lblFileName"), Label)
            Dim deleteFlg As TextBox = DirectCast(repItem.FindControl("txtDeleteFlg"), TextBox)
            If fileName Is Nothing OrElse deleteFlg Is Nothing Then
                Continue For
            End If
            Dim qAttachment = From attachmentItem In dtAttachment Where attachmentItem("FILENAME").Equals(fileName.Text)
            If qAttachment.Any Then
                qAttachment.FirstOrDefault.Item("DELFLG") = deleteFlg.Text
                'qAttachment.FirstOrDefault.Item("ISMODIFIED") = CONST_FLAG_YES
            End If
        Next

        Return dtAttachment
    End Function

    ''' <summary>
    ''' 国、年月一覧の添付(Attachment)フィールドダブルクリック時
    ''' </summary>
    Public Sub ShowAttachmentArea_Click()
        '*********************************
        '添付ファイル情報のリセット
        '*********************************
        ViewState.Remove(CONST_VS_PREV_ATTACHMENTINFO)
        ViewState.Remove(CONST_VS_CURR_ATTACHMENTINFO)
        ViewState.Remove(CONST_VS_ATTA_UNIQUEID)
        '*********************************
        '国リストを復元し選択行のレコード取得
        '*********************************
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
        Dim rowIdString As String = Me.hdnListCurrentRownum.Value

        Dim targetDr As DataRow = (From item In dt Where Convert.ToString(item("LINECNT")) = rowIdString).FirstOrDefault
        Dim countryCode As String = Convert.ToString(targetDr("COUNTRYCODE"))
        Dim countryName As String = Convert.ToString(targetDr("COUNTRYNAME"))
        Dim month As String = Convert.ToString(targetDr("PRINTMONTH"))
        Dim attrUniqueId As String = String.Format("{0}\{1}", countryCode, month.Replace("/", ""))
        '*********************************
        '有効なREPORTMONTHが選択されているかチェック
        '*********************************
        Dim tmpDtm As Date = Now
        If month = "" OrElse Date.TryParseExact(month, "yyyy/MM", Nothing, Nothing, tmpDtm) = False Then
            Return
        End If
        Dim clDt As DataTable = GetClosingDay(countryCode)
        With Me.lbPrintMonth
            .DataSource = clDt
            .DataTextField = "REPORTMONTH"
            .DataValueField = "REPORTMONTH"
            .DataBind()
            .Focus()
        End With
        If Me.lbPrintMonth.Items.Count > 0 Then
            Dim findListItem = Me.lbPrintMonth.Items.FindByValue(Convert.ToString(month))
            If findListItem Is Nothing Then
                CommonFunctions.ShowMessage(C_MESSAGENO.INVALIDINPUT, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        End If
        '*********************************
        '添付ファイルユーザー作業領域のクリア
        '*********************************
        CommonFunctions.CleanUserTempDirectory(CONST_MAPID)
        '*********************************
        '保存済みの添付ファイル一覧の取得、画面設定
        '*********************************
        Dim dtAttachment As DataTable = CommonFunctions.GetInitAttachmentFileList(attrUniqueId, CONST_DIRNAME_SOAREP_UPROOT, CONST_MAPID)
        Me.dtCurAttachment = dtAttachment
        ViewState(CONST_VS_PREV_ATTACHMENTINFO) = dtAttachment
        ViewState(CONST_VS_CURR_ATTACHMENTINFO) = CommonFunctions.DeepCopy(dtAttachment)
        ViewState(CONST_VS_ATTA_UNIQUEID) = attrUniqueId
        'リピーターに一覧を設定
        repAttachment.DataSource = dtAttachment
        repAttachment.DataBind()
        '*********************************
        '添付ファイルポップアップの表示
        '*********************************
        'ヘッダー部分に国、年月を転送
        Me.lblAttachCounry.Text = countryName
        Me.lblAttachMonth.Text = BASEDLL.FormatDateContrySettings(month & "/01", GBA00003UserSetting.DATEYMFORMAT)
        '表示スタイル設定
        Me.divAttachmentInputAreaWapper.Style.Remove("display")
        Me.divAttachmentInputAreaWapper.Style.Add("display", "block")
    End Sub
    ''' <summary>
    ''' 添付ファイルアップロード処理
    ''' </summary>
    Private Sub UploadAttachment()
        Dim attrUniqueId As String = Convert.ToString(ViewState(CONST_VS_ATTA_UNIQUEID))
        Dim dtAttachment As DataTable = Me.dtCurAttachment
        Dim chkMsgNo = CommonFunctions.CheckUploadAttachmentFile(dtAttachment)
        If chkMsgNo <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(chkMsgNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        dtAttachment = CommonFunctions.UploadAttachmentFile(dtAttachment, attrUniqueId, CONST_MAPID)
        repAttachment.DataSource = dtAttachment
        repAttachment.DataBind()
        Me.dtCurAttachment = dtAttachment
    End Sub
    ''' <summary>
    ''' 添付ファイル欄の添付ファイル名ダブルクリック時処理
    ''' </summary>
    Private Sub AttachmentFileNameDblClick()
        Dim fileName As String = Me.hdnFileDisplay.Value
        If fileName = "" Then
            Return
        End If
        Dim dtAttachment As DataTable = Me.dtCurAttachment
        Dim dlUrl As String = CommonFunctions.GetAttachfileDownloadUrl(dtAttachment, fileName)
        Me.hdnPrintURL.Value = dlUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
    End Sub
    ''' <summary>
    ''' Fixvalueを元にリストボックスを作成
    ''' </summary>
    ''' <param name="className"></param>
    ''' <param name="targetList"></param>
    ''' <remarks></remarks>
    Private Sub SetFixvalueListItem(className As String, targetList As ListBox)
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Dim jpList As New ListBox
        Dim engList As New ListBox
        targetList.Items.Clear()
        'Term選択肢
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = className
        COA0017FixValue.LISTBOX1 = jpList
        COA0017FixValue.LISTBOX2 = engList
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                targetList.Items.AddRange(jpList.Items.Cast(Of ListItem).ToArray)
            Else
                targetList.Items.AddRange(engList.Items.Cast(Of ListItem).ToArray)
            End If
        Else
            Throw New Exception("Fix value getError")
        End If
    End Sub

    ''' <summary>
    ''' 右ボックス出力帳票選択肢設定
    ''' </summary>
    ''' <returns>メッセージNo</returns>
    Private Function RightboxInit() As String
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim excelMapId As String = CONST_MAPID

        'RightBOX情報設定
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        retVal = C_MESSAGENO.NORMAL

        '初期化
        Me.txtRightErrorMessage.Text = ""

        'レポートID情報
        COA0022ProfXls.MAPID = excelMapId
        COA0022ProfXls.COA0022getReportId()
        Me.lbRightList.Items.Clear() '一旦選択肢をクリア
        If COA0022ProfXls.ERR = C_MESSAGENO.NORMAL Then
            Try
                Dim listBoxObj As ListBox = DirectCast(COA0022ProfXls.REPORTOBJ, ListBox)
                For Each listItem As ListItem In listBoxObj.Items
                    Me.lbRightList.Items.Add(listItem)
                Next
            Catch ex As Exception
            End Try
        Else
            retVal = COA0022ProfXls.ERR
            Return retVal
        End If

        'レポートID変数検索
        COA0016VARIget.MAPID = excelMapId
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        If Me.hdnPrintFlg.Value = "1" Then
            COA0016VARIget.VARI = "Print"
        Else
            COA0016VARIget.VARI = "Approval"
        End If
        COA0016VARIget.FIELD = "REPORTID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR <> C_MESSAGENO.NORMAL Then
            retVal = COA0016VARIget.ERR
            Return retVal
        End If

        'ListBox選択
        Me.lbRightList.SelectedIndex = -1     '選択無しの場合、デフォルト
        Dim targetListItem = lbRightList.Items.FindByValue(COA0016VARIget.VALUE)
        If targetListItem IsNot Nothing Then
            targetListItem.Selected = True
        Else
            If Me.lbRightList.Items.Count > 0 Then
                Me.lbRightList.SelectedIndex = 0
            End If
        End If

        Return retVal
    End Function

    '''' <summary>
    '''' 月締め日を取得
    '''' </summary>
    '''' <param name="countryCode"></param>
    '''' <returns></returns>
    '''' <remarks>デッドロジックの可能性あり2019/06/07 三宅：コメントアウトし様子見、使わないなら削除</remarks>
    'Private Function GetClosingDate(countryCode As String) As String
    '    Dim sqlStat As New StringBuilder
    '    sqlStat.AppendLine("SELECT FORMAT(DATEADD(month,1,BILLINGYMD),'yyyy/MM')")
    '    sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY")
    '    sqlStat.AppendLine(" WHERE COUNTRYCODE = @COUNTRYCODE ")
    '    sqlStat.AppendLine("   AND DELFLG <> @DELFLG")

    '    Dim dtDbResult As New DataTable
    '    Dim retStr As String = ""
    '    'DB接続
    '    Using sqlCon As New SqlConnection(COA0019Session.DBcon),
    '          sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
    '        sqlCon.Open() '接続オープン
    '        'SQLパラメータ設定
    '        With sqlCmd.Parameters
    '            .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
    '            .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
    '        End With
    '        '取得結果をDataTableに転送
    '        Using sqlDa As New SqlDataAdapter(sqlCmd)
    '            sqlDa.Fill(dtDbResult)
    '        End Using 'sqlDa
    '    End Using 'sqlCon sqlCmd
    '    If dtDbResult IsNot Nothing AndAlso dtDbResult.Rows.Count > 0 Then
    '        With dtDbResult.Rows(0)
    '            retStr = Convert.ToString(.Item(0))
    '        End With
    '    End If
    '    Return retStr
    'End Function

    ''' <summary>
    ''' 請求日戻し処理
    ''' </summary>
    Public Sub BackBillingDate(ByVal countryCode As String, ByVal billingYmd As String, Optional isUnclockProc As Boolean = False)

        '請求日更新
        Dim sqlStat As StringBuilder = Nothing
        Dim sqlStats As New List(Of StringBuilder)
        '******************************
        'ClosingDayを戻す
        '******************************
        sqlStat = New StringBuilder
        sqlStat.AppendLine(" UPDATE GBT0006_CLOSINGDAY ")
        sqlStat.AppendLine("    SET  DELFLG     = @DELFLG")
        sqlStat.AppendLine("       , UPDYMD     = @ENTYMD")
        sqlStat.AppendLine("       , UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("       , UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("    AND COUNTRYCODE = @COUNTRYCODE")
        sqlStat.AppendLine("    AND REPORTMONTH = @REPORTMONTH")
        sqlStats.Add(sqlStat)
        '******************************
        'OrderValueの強制〆を金額及びSOAAPDATEを元の状態に戻す
        '******************************
        'OrderValue履歴積み上げ
        sqlStat = New StringBuilder
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
        sqlStat.AppendLine("     ,CURRENCYSEGMENT")
        sqlStat.AppendLine("     ,ACCCRERATE")
        sqlStat.AppendLine("     ,ACCCREYEN")
        sqlStat.AppendLine("     ,ACCCREFOREIGN")
        sqlStat.AppendLine("     ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("     ,FORCECLOSED")
        sqlStat.AppendLine("     ,AMOUNTFIXBFC")
        sqlStat.AppendLine("     ,ACCCREYENBFC")
        sqlStat.AppendLine("     ,ACCCREFOREIGNBFC")
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
        sqlStat.AppendLine("         ,CURRENCYSEGMENT")
        sqlStat.AppendLine("         ,ACCCRERATE")
        sqlStat.AppendLine("         ,ACCCREYEN")
        sqlStat.AppendLine("         ,ACCCREFOREIGN")
        sqlStat.AppendLine("         ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("         ,FORCECLOSED")
        sqlStat.AppendLine("         ,AMOUNTFIXBFC")
        sqlStat.AppendLine("         ,ACCCREYENBFC")
        sqlStat.AppendLine("         ,ACCCREFOREIGNBFC")
        sqlStat.AppendLine("         ,'" & CONST_FLAG_YES & "'             AS DELFLG")
        sqlStat.AppendLine("         ,INITYMD")
        sqlStat.AppendLine("         ,INITUSER")
        sqlStat.AppendLine("         ,@ENTYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE OV")
        sqlStat.AppendLine(" WHERE OV.DELFLG      = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("   AND OV.FORCECLOSED = '1'")
        sqlStat.AppendLine("   AND EXISTS (SELECT 1 ")
        sqlStat.AppendLine("                 FROM GBT0008_JOTSOA_VALUE JV")
        sqlStat.AppendLine("                WHERE JV.DATAIDODR    = OV.DATAID")
        sqlStat.AppendLine("                  AND JV.DELFLG       = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("                  AND JV.CLOSINGGROUP = @COUNTRYCODE")
        sqlStat.AppendLine("                  AND JV.CLOSINGMONTH = @REPORTMONTH")
        sqlStat.AppendLine("              )")
        'sqlStat.AppendLine(";")
        sqlStats.Add(sqlStat)
        '金額戻し
        sqlStat = New StringBuilder
        sqlStat.AppendLine(" UPDATE OV")
        sqlStat.AppendLine("    SET SOAAPPDATE       = '1900/01/01'")      '強制クローズされるデータはSOA日付初期値の為戻す
        sqlStat.AppendLine("       ,AMOUNTFIX        = AMOUNTFIXBFC")      '退避した金額を戻す
        sqlStat.AppendLine("       ,ACCCREYEN        = ACCCREYENBFC")      '退避した金額を戻す
        sqlStat.AppendLine("       ,ACCCREFOREIGN    = ACCCREFOREIGNBFC")  '退避した金額を戻す

        sqlStat.AppendLine("       ,FORCECLOSED      = ''")                '強制〆のフラグはなくす
        sqlStat.AppendLine("       ,AMOUNTFIXBFC     = @AMOUNTFIXBFC")     '退避前の金額は0
        sqlStat.AppendLine("       ,ACCCREYENBFC     = @ACCCREYENBFC")     '退避前の金額は0
        sqlStat.AppendLine("       ,ACCCREFOREIGNBFC = @ACCCREFOREIGNBFC") '退避前の金額は0

        sqlStat.AppendLine("       ,UPDYMD        = @ENTYMD")
        sqlStat.AppendLine("       ,UPDUSER       = @UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID     = @UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD    = @RECEIVEYMD ")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE OV")
        sqlStat.AppendLine(" WHERE OV.DELFLG      = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("   AND OV.FORCECLOSED = '1'")
        sqlStat.AppendLine("   AND EXISTS (SELECT 1 ")
        sqlStat.AppendLine("                 FROM GBT0008_JOTSOA_VALUE JV")
        sqlStat.AppendLine("                WHERE JV.DATAIDODR    = OV.DATAID")
        sqlStat.AppendLine("                  AND JV.DELFLG       = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("                  AND JV.CLOSINGGROUP = @COUNTRYCODE")
        sqlStat.AppendLine("                  AND JV.CLOSINGMONTH = @REPORTMONTH")
        sqlStat.AppendLine("              )")
        'sqlStat.AppendLine(";")
        sqlStats.Add(sqlStat)
        '******************************
        'JOTSOAテーブルの情報も論理削除する
        '******************************
        sqlStat = New StringBuilder
        sqlStat.AppendLine(" UPDATE GBT0008_JOTSOA_VALUE ")
        sqlStat.AppendLine("    SET  DELFLG     = @DELFLG")
        sqlStat.AppendLine("       , UPDYMD     = @ENTYMD")
        sqlStat.AppendLine("       , UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("       , UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("    AND CLOSINGGROUP = @COUNTRYCODE")
        sqlStat.AppendLine("    AND CLOSINGMONTH = @REPORTMONTH")
        sqlStats.Add(sqlStat)
        If isUnclockProc Then
            '******************************
            'アンロックでの戻し作業の場合経理連携関連の2テーブルにも削除フラグを立てる
            '******************************
            sqlStat = New StringBuilder
            sqlStat.AppendLine(" UPDATE GBT0015_AC_WORK ")
            sqlStat.AppendLine("    SET  DELFLG     = @DELFLG")
            sqlStat.AppendLine("       , UPDYMD     = @ENTYMD")
            sqlStat.AppendLine("       , UPDUSER    = @UPDUSER")
            sqlStat.AppendLine("       , UPDTERMID  = @UPDTERMID")
            sqlStat.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
            sqlStat.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
            sqlStat.AppendLine("    AND CLOSINGGROUP = @COUNTRYCODE")
            sqlStat.AppendLine("    AND CLOSINGMONTH = @REPORTMONTH")
            sqlStats.Add(sqlStat)
            sqlStat = New StringBuilder
            sqlStat.AppendLine(" UPDATE GBT0014_AC_VALUE ")
            sqlStat.AppendLine("    SET  DELFLG     = @DELFLG")
            sqlStat.AppendLine("       , UPDYMD     = @ENTYMD")
            sqlStat.AppendLine("       , UPDUSER    = @UPDUSER")
            sqlStat.AppendLine("       , UPDTERMID  = @UPDTERMID")
            sqlStat.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
            sqlStat.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
            sqlStat.AppendLine("    AND CLOSINGGROUP = @COUNTRYCODE")
            sqlStat.AppendLine("    AND CLOSINGMONTH = @REPORTMONTH")
            sqlStats.Add(sqlStat)
        End If
#Region "否認時承認日戻し 2019/06/07 全てコメントアウトし新たなSQLを↑に定義、問題なければRegion内削除"
        'sqlStat.AppendLine("INSERT INTO GBT0006_CLOSINGDAY")
        'sqlStat.AppendLine("     (")
        'sqlStat.AppendLine("     COUNTRYCODE")
        'sqlStat.AppendLine("   , STYMD")
        'sqlStat.AppendLine("   , BILLINGYMD")
        'sqlStat.AppendLine("   , REPORTMONTH")
        'sqlStat.AppendLine("   , APPLYID")
        'sqlStat.AppendLine("   , LASTSTEP")
        'sqlStat.AppendLine("   , APPLYUSER")
        'sqlStat.AppendLine("   , APPLYOFFICE")

        'sqlStat.AppendLine("   , DELFLG")
        'sqlStat.AppendLine("   , INITYMD")
        'sqlStat.AppendLine("   , UPDYMD")
        'sqlStat.AppendLine("   , UPDUSER")
        'sqlStat.AppendLine("   , UPDTERMID")
        'sqlStat.AppendLine("   , RECEIVEYMD")
        'sqlStat.AppendLine("     )")
        'sqlStat.AppendLine(" SELECT COUNTRYCODE")
        'sqlStat.AppendLine("       ,@STYMD")
        'sqlStat.AppendLine("       ,DATEADD(month,-1,BILLINGYMD)")
        'sqlStat.AppendLine("       ,REPORTMONTH")
        'sqlStat.AppendLine("       ,APPLYID")
        'sqlStat.AppendLine("       ,LASTSTEP")
        'sqlStat.AppendLine("       ,APPLYUSER")
        'sqlStat.AppendLine("       ,APPLYOFFICE")

        'sqlStat.AppendLine("       ,'" & CONST_FLAG_NO & "'")
        'sqlStat.AppendLine("       ,@ENTYMD")
        'sqlStat.AppendLine("       ,@ENTYMD")
        'sqlStat.AppendLine("       ,@UPDUSER")
        'sqlStat.AppendLine("       ,@UPDTERMID")
        'sqlStat.AppendLine("       ,@RECEIVEYMD")
        'sqlStat.AppendLine("   FROM GBT0006_CLOSINGDAY")
        'sqlStat.AppendLine("  WHERE DELFLG <> @DELFLG ")
        'sqlStat.AppendLine("    AND COUNTRYCODE = @COUNTRYCODE; ")
        'sqlStat.AppendLine(" UPDATE GBT0006_CLOSINGDAY ")
        'sqlStat.AppendLine("    SET  DELFLG     = @DELFLG")
        'sqlStat.AppendLine("       , UPDYMD     = @ENTYMD")
        'sqlStat.AppendLine("       , UPDUSER    = @UPDUSER")
        'sqlStat.AppendLine("       , UPDTERMID  = @UPDTERMID")
        'sqlStat.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
        'sqlStat.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
        'sqlStat.AppendLine("    AND COUNTRYCODE = @COUNTRYCODE")
        'sqlStat.AppendLine("    AND INITYMD     < @ENTYMD;")
#End Region
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open() '接続オープン
            Using tran = sqlCon.BeginTransaction()

                Using sqlCmd As New SqlCommand()
                    sqlCmd.Connection = sqlCon
                    sqlCmd.Transaction = tran
                    With sqlCmd.Parameters
                        Dim procDate As Date = Date.Now
                        'パラメータ設定
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                        .Add("@STYMD", SqlDbType.Date).Value = procDate
                        .Add("@ENTYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")

                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
                        .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = billingYmd

                        .Add("@AMOUNTFIXBFC", SqlDbType.Float).Value = 0
                        .Add("@ACCCREYENBFC", SqlDbType.Float).Value = 0
                        .Add("@ACCCREFOREIGNBFC", SqlDbType.Float).Value = 0
                    End With
                    Try
                        For Each sqlStatItem In sqlStats
                            sqlCmd.CommandText = sqlStatItem.ToString
                            sqlCmd.ExecuteNonQuery()
                        Next
                        tran.Commit()
                    Catch ex As Exception
                        tran.Rollback()
                        Throw
                    End Try
                End Using ' cmd
            End Using 'tran 
        End Using 'con 

    End Sub

    ''' <summary>
    ''' 帳票出力用のデータテーブルを取得する関数
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOutputJOTSOAListDataTable(ByVal selectedRow As DataRow) As DataTable
        Dim mapId As String = CONST_ORD_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力

        Dim GBA00013SoaInfo As New GBA00013SoaInfo
        GBA00013SoaInfo.INVOICEDBYTYPE = "EJ"
        GBA00013SoaInfo.COUNTRYCODE = Convert.ToString(selectedRow.Item("COUNTRYCODE"))
        If Convert.ToString(selectedRow.Item("COUNTRYCODE")) = GBC_JOT_SOA_COUNTRY Then
            GBA00013SoaInfo.INVOICEDBYTYPE = "OJ"
            GBA00013SoaInfo.COUNTRYCODE = ""
        End If

        If Me.hdnPrintFlg.Value = "1" Then
            GBA00013SoaInfo.REPORTMONTH = FormatDateContrySettings(FormatDateYMD(Convert.ToString(selectedRow.Item("PRINTMONTH")), GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
        Else
            GBA00013SoaInfo.REPORTMONTH = FormatDateContrySettings(FormatDateYMD(Me.lblBillingMonth.Text, GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
        End If

        GBA00013SoaInfo.GBA00013getJOTSoaDataTable()
        If Not {C_MESSAGENO.NORMAL, C_MESSAGENO.NODATA}.Contains(GBA00013SoaInfo.ERR) Then
            Throw New Exception("GBA00013getJotSoaDataTable Error")
        End If

        Dim dtDbResult As DataTable = GBA00013SoaInfo.SOADATATABLE

        Dim retDt As DataTable = CreateOrderListTable()
        If dtDbResult Is Nothing OrElse dtDbResult.Rows Is Nothing OrElse dtDbResult.Rows.Count = 0 Then
            Dim writeDr As DataRow = retDt.NewRow
            SetHeaderValue(writeDr, selectedRow, True)
            retDt.Rows.Add(writeDr)
            Return retDt
        End If
        Dim loopEnd As Integer = 1
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next
        Dim actyNo As Integer = 0
        Dim orderNo As String = Convert.ToString(dtDbResult.Rows(0).Item("ORDERNO"))
        Dim tankSeq As String = Convert.ToString(dtDbResult.Rows(0).Item("TANKSEQ"))
        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                If colName = "DISPSEQ" Then
                    writeDr.Item(colName) = Convert.ToString(readDr.Item(colName))
                Else
                    writeDr.Item(colName) = readDr.Item(colName)
                End If
            Next
            If Not (tankSeq.Equals(readDr.Item("TANKSEQ")) _
                    AndAlso orderNo.Equals(readDr.Item("ORDERNO"))) Then
                actyNo = 0
                orderNo = Convert.ToString(readDr.Item("ORDERNO"))
                tankSeq = Convert.ToString(readDr.Item("TANKSEQ"))
            End If
            actyNo = actyNo + 1
            writeDr.Item("ACTYNO") = actyNo.ToString("000")

            SetHeaderValue(writeDr, selectedRow)

            retDt.Rows.Add(writeDr)
        Next

        Return retDt

    End Function

    ''' <summary>
    ''' LIST登録チェック
    ''' </summary>
    ''' <param name="inText"></param>
    ''' <param name="inList"></param>
    Protected Function CheckList(ByVal inText As String, ByVal inList As ListBox) As Boolean

        Dim flag As Boolean = False

        If inText <> "" Then

            For i As Integer = 0 To inList.Items.Count - 1
                If inList.Items(i).Text = inText Then
                    flag = True
                    Exit For
                End If
            Next
        Else
            flag = True

        End If

        Return flag
    End Function

    ''' <summary>
    ''' 表示非表示制御
    ''' </summary>
    ''' <remarks>初回ロード時（非ポストバック時）に実行する想定</remarks>
    Private Sub DisplayControl()

        If Me.hdnPrintFlg.Value = "1" Then

            'Me.divSearchConditionBox.visible = False

            Me.btnExtract.Visible = False
            Me.btnApproval.Visible = False
            Me.btnReject.Visible = False
            Me.searchCondition.Visible = False

        End If

    End Sub

    ''' <summary>
    ''' 出力年月設定
    ''' </summary>
    ''' <param name="isOpen"></param>
    Private Sub SetPrintMonth(isOpen As Boolean, Optional selectVal As String = "")
        Me.lbPrintMonth.Items.Clear()

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

        Dim uniqueIndex As String = Me.hdnListCurrentRownum.Value
        Dim targetRow = (From dr In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = uniqueIndex)

        Dim printMonth As String = ""
        Dim countryCode As String = ""
        If targetRow IsNot Nothing AndAlso targetRow.Count > 0 Then

            If isOpen = True Then

                printMonth = Convert.ToString(targetRow(0).Item("PRINTMONTH"))
                countryCode = Convert.ToString(targetRow(0).Item("COUNTRYCODE"))

                Dim clDt As DataTable = GetClosingDay(countryCode)
                With Me.lbPrintMonth
                    .DataSource = clDt
                    .DataTextField = "FORMATEDDREPORTMONTH"
                    .DataValueField = "REPORTMONTH"
                    .DataBind()
                    .Focus()
                End With

                '一応現在入力しているテキストと一致するものを選択状態
                If Me.lbPrintMonth.Items.Count > 0 Then
                    Dim findListItem = Me.lbPrintMonth.Items.FindByValue(printMonth)
                    If findListItem IsNot Nothing Then
                        findListItem.Selected = True
                    End If
                End If

                Me.mvLeft.Focus()

            Else
                targetRow(0).Item("PRINTMONTH") = selectVal
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
    ''' 年月のリストを取得する
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <returns></returns>
    Private Function GetClosingDay(countryCode As String) As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        With retDt.Columns
            .Add("REPORTMONTH", GetType(String))
            .Add("FORMATEDDREPORTMONTH", GetType(String))
        End With

        'SQL文作成
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT distinct CD.REPORTMONTH AS REPORTMONTH")
        sqlStat.AppendLine("               ,''             AS FORMATEDDREPORTMONTH")
        sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CD")
        sqlStat.AppendLine("  INNER JOIN COT0002_APPROVALHIST AP")
        sqlStat.AppendLine("          ON CD.APPLYID  = AP.APPLYID ")
        sqlStat.AppendLine("         AND CD.LASTSTEP = AP.STEP")
        sqlStat.AppendLine("         AND AP.STATUS IN ('" & C_APP_STATUS.APPROVED & "','" & C_APP_STATUS.COMPLETE & "')")
        sqlStat.AppendLine(" WHERE CD.COUNTRYCODE = @COUNTRYCODE")
        sqlStat.AppendLine("   AND CD.REPORTMONTH <> ''")
        sqlStat.AppendLine("   AND CD.DELFLG <> @DELFLG")
        sqlStat.AppendLine("ORDER BY CD.REPORTMONTH DESC ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        If retDt IsNot Nothing AndAlso retDt.Rows.Count > 0 Then
            For Each dr As DataRow In retDt.Rows
                Dim reportMonth As String = Convert.ToString(dr("REPORTMONTH")) & "/01"
                Dim formattedReportMonthg As String = BaseDllCommon.FormatDateContrySettings(reportMonth, GBA00003UserSetting.DATEYMFORMAT)
                dr("FORMATEDDREPORTMONTH") = formattedReportMonthg
            Next
        End If
        Return retDt
    End Function
    ''' <summary>
    ''' ヘッダー設定
    ''' </summary>
    ''' <param name="writeDr"></param>
    ''' <param name="selectedRow"></param>
    Private Sub SetHeaderValue(ByRef writeDr As DataRow, ByVal selectedRow As DataRow, Optional JOTSOAflg As Boolean = False)

        Dim reportDate As String = FormatDateContrySettings(FormatDateYMD(Convert.ToString(selectedRow.Item("PRINTMONTH")), GBA00003UserSetting.DATEFORMAT), "yyyy/MM") & "/01"

        'ヘッダ情報設定
        writeDr.Item("COUNTRYNAMEH") = Convert.ToString(selectedRow.Item("COUNTRYNAME"))
        writeDr.Item("OFFICENAMEH") = Convert.ToString(selectedRow.Item("OFFICENAME"))
        writeDr.Item("APPLYUSERH") = Convert.ToString(selectedRow.Item("APPLYUSER"))
        writeDr.Item("CURRENCYCODEH") = Convert.ToString(selectedRow.Item("CURRENCYCODE"))
        writeDr.Item("LOCALRATEH") = Convert.ToString(selectedRow.Item("LOCALRATE"))

        writeDr.Item("FINALREPORTNOH") = ""
        writeDr.Item("CLOSEDATEH") = Convert.ToString(selectedRow.Item("CLOSEDATE"))
        writeDr.Item("PRINTDATEH") = Date.Now.ToString("yyyy/MM/dd HH:mm")

        If JOTSOAflg Then

            If Me.hdnBillingYmd.Value.Trim = "" Then
                'ヘッダ情報設定
                writeDr.Item("REPORTMONTHH") = reportDate
                '明細情報設定
                writeDr.Item("REPORTMONTH") = Convert.ToString(selectedRow.Item("PRINTMONTH"))
            Else
                'ヘッダ情報設定
                writeDr.Item("REPORTMONTHH") = FormatDateContrySettings(FormatDateYMD(Me.hdnBillingYmd.Value, GBA00003UserSetting.DATEFORMAT), "yyyy/MM") & "/01"
                '明細情報設定
                writeDr.Item("REPORTMONTH") = Me.lblBillingMonth.Text
            End If
            'writeDr.Item("REPORTMONTHORG") = ""

        End If

        If Me.hdnPrintFlg.Value = "1" Then

            Dim clData As DataTable = GetPrintClosingDate(Convert.ToString(selectedRow.Item("COUNTRYCODE")), reportDate)

            If clData.Rows.Count > 0 Then

                writeDr.Item("OFFICENAMEH") = Convert.ToString(clData.Rows(0).Item("OFFICENAME"))
                writeDr.Item("APPLYUSERH") = Convert.ToString(clData.Rows(0).Item("APPLYUSER"))
                writeDr.Item("CURRENCYCODEH") = Convert.ToString(clData.Rows(0).Item("CURRENCYCODE"))
                writeDr.Item("LOCALRATEH") = Convert.ToString(clData.Rows(0).Item("LOCALRATE"))
                writeDr.Item("CLOSEDATEH") = Convert.ToString(clData.Rows(0).Item("CLOSEDATE"))

            End If
        End If

    End Sub

    ''' <summary>
    ''' 印刷時に締め日情報を再取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <returns></returns>
    Private Function GetPrintClosingDate(ByVal countryCode As String, ByVal reportDate As String) As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        Static retResult As Dictionary(Of String, DataTable)
        If retResult Is Nothing Then
            retResult = New Dictionary(Of String, DataTable)
        End If
        Dim keyString As String = countryCode & "@" & reportDate
        If retResult.ContainsKey(keyString) Then
            retDt = retResult(keyString)
            Return retDt
        End If
        'SQL文作成
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(TR.NAMELJP,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(TR.NAMEL,'') END As OFFICENAME")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(USN.STAFFNAMES,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(USN.STAFFNAMES_EN,'') END As APPLYUSER")
        sqlStat.AppendLine("      ,ISNULL(EX.CURRENCYCODE,'') AS CURRENCYCODE")
        sqlStat.AppendLine("      ,ISNULL(EX.EXRATE,'') AS LOCALRATE")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(CL.APPLYID,'') = '' THEN '' ELSE FORMAT(CL.UPDYMD,'yyyy/MM/dd HH:mm') END AS CLOSEDATE")

        sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CL")
        sqlStat.AppendLine("  INNER JOIN COT0002_APPROVALHIST AP")
        sqlStat.AppendLine("          ON CL.APPLYID  = AP.APPLYID ")
        sqlStat.AppendLine("         AND CL.LASTSTEP = AP.STEP")
        sqlStat.AppendLine("         AND AP.STATUS IN ('" & C_APP_STATUS.APPROVED & "','" & C_APP_STATUS.COMPLETE & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR") '業者名称用JOIN
        sqlStat.AppendLine("    ON  TR.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  TR.CARRIERCODE  = CL.APPLYOFFICE")
        sqlStat.AppendLine("   AND  TR.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  TR.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  TR.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE EX") '通貨用JOIN
        sqlStat.AppendLine("    ON  EX.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  EX.COUNTRYCODE  = CL.COUNTRYCODE")
        sqlStat.AppendLine("   AND  EX.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  EX.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  EX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  EX.TARGETYM     = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")

        sqlStat.AppendLine("  LEFT JOIN COS0005_USER USN") 'ユーザー名用JOIN
        sqlStat.AppendLine("    ON  USN.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  USN.USERID       = CL.APPLYUSER")
        sqlStat.AppendLine("   AND  USN.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  USN.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  USN.DELFLG      <> @DELFLG")

        sqlStat.AppendLine(" WHERE CL.COUNTRYCODE = @COUNTRYCODE")
        sqlStat.AppendLine("   AND CL.REPORTMONTH = @REPORTMONTH")
        sqlStat.AppendLine("   AND CL.DELFLG     <> @DELFLG")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = FormatDateContrySettings(FormatDateYMD(reportDate, GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@TARGETYM", SqlDbType.Date).Value = Date.Parse(reportDate)
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        retResult.Add(keyString, retDt)
        Return retDt
    End Function
    ''' <summary>
    ''' 経理連携（作業用） 登録
    ''' </summary>
    ''' <param name="parmCountry"></param>
    ''' <param name="parmBillingYm"></param>
    Public Sub EntryACWork(ByVal parmCountry As String, ByVal parmBillingYm As String)

        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("UPDATE GBT0015_AC_WORK")
        sqlStat.AppendLine("    SET  DELFLG     = @DELFLG")
        sqlStat.AppendLine("       , UPDYMD     = @ENTYMD")
        sqlStat.AppendLine("       , UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("       , UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("    AND CLOSINGMONTH = @CLOSINGMONTH")
        sqlStat.AppendLine("    AND CLOSINGGROUP = @CLOSINGGROUP;")

        sqlStat.AppendLine("INSERT INTO GBT0015_AC_WORK")
        sqlStat.AppendLine(" (")
        sqlStat.AppendLine("   CLOSINGMONTH")
        sqlStat.AppendLine("  ,CLOSINGGROUP")
        sqlStat.AppendLine("  ,INVOICEDBY")
        sqlStat.AppendLine("  ,CONTRACTORFIX")
        sqlStat.AppendLine("  ,ORDERNO")
        sqlStat.AppendLine("  ,TANKNO")
        sqlStat.AppendLine("  ,COSTCODE")
        sqlStat.AppendLine("  ,COSTTYPE")
        sqlStat.AppendLine("  ,CRACCOUNT")
        sqlStat.AppendLine("  ,DBACCOUNT")
        sqlStat.AppendLine("  ,CRACCOUNTFORIGN")
        sqlStat.AppendLine("  ,DBACCOUNTFORIGN")
        sqlStat.AppendLine("  ,OFFCRACCOUNT")
        sqlStat.AppendLine("  ,OFFDBACCOUNT")
        sqlStat.AppendLine("  ,OFFCRACCOUNTFORIGN")
        sqlStat.AppendLine("  ,OFFDBACCOUNTFORIGN")
        sqlStat.AppendLine("  ,CRSEGMENT1")
        sqlStat.AppendLine("  ,DBSEGMENT1")
        sqlStat.AppendLine("  ,CRGENERALPURPOSE")
        sqlStat.AppendLine("  ,CREGENPURPOSE")
        sqlStat.AppendLine("  ,DBGENERALPURPOSE")
        sqlStat.AppendLine("  ,DEBGENPURPOSE")
        sqlStat.AppendLine("  ,COUNTRYCODE")
        sqlStat.AppendLine("  ,CURRENCYCODE")
        sqlStat.AppendLine("  ,TAXATION")
        sqlStat.AppendLine("  ,AMOUNTFIX")
        sqlStat.AppendLine("  ,LOCALBR")
        sqlStat.AppendLine("  ,LOCALRATE")
        sqlStat.AppendLine("  ,AMOUNTPAYODR")
        sqlStat.AppendLine("  ,LOCALPAYODR")
        sqlStat.AppendLine("  ,TAXBR")
        sqlStat.AppendLine("  ,LOCALRATESOA")
        sqlStat.AppendLine("  ,AMOUNTPAY")
        sqlStat.AppendLine("  ,LOCALPAY")
        sqlStat.AppendLine("  ,TAXPAY")
        sqlStat.AppendLine("  ,UAG_USD")
        sqlStat.AppendLine("  ,UAG_LOCAL")
        sqlStat.AppendLine("  ,UAG_JPY")
        sqlStat.AppendLine("  ,UAG_USD_SHIP")
        sqlStat.AppendLine("  ,UAG_JPY_SHIP")
        sqlStat.AppendLine("  ,USD_USD")
        sqlStat.AppendLine("  ,USD_LOCAL")
        sqlStat.AppendLine("  ,LOCAL_USD")
        sqlStat.AppendLine("  ,LOCAL_LOCAL")
        sqlStat.AppendLine("  ,ACTUALDATE")
        sqlStat.AppendLine("  ,SOAAPPDATE")
        sqlStat.AppendLine("  ,REMARK")
        sqlStat.AppendLine("  ,BRID")
        sqlStat.AppendLine("  ,APPLYID")
        sqlStat.AppendLine("  ,SOACODE")
        sqlStat.AppendLine("  ,SOASHORTCODE")
        sqlStat.AppendLine("  ,REPORTMONTH")
        sqlStat.AppendLine("  ,REPORTMONTHORG")
        sqlStat.AppendLine("  ,REPORTRATEJPY")
        sqlStat.AppendLine("  ,SHIPDATE")
        sqlStat.AppendLine("  ,DOUTDATE")
        sqlStat.AppendLine("  ,LOADING")
        sqlStat.AppendLine("  ,STEAMING")
        sqlStat.AppendLine("  ,TIP")
        sqlStat.AppendLine("  ,EXTRA")
        sqlStat.AppendLine("  ,ROUTEDAYS")
        sqlStat.AppendLine("  ,DATAIDODR")
        sqlStat.AppendLine("  ,ACCRECRATE")
        sqlStat.AppendLine("  ,ACCRECYEN")
        sqlStat.AppendLine("  ,ACCRECFOREIGN")
        sqlStat.AppendLine("  ,EXSHIPRATE1")
        sqlStat.AppendLine("  ,INSHIPRATE1")
        sqlStat.AppendLine("  ,EXSHIPRATE2")
        sqlStat.AppendLine("  ,INSHIPRATE2")
        sqlStat.AppendLine("  ,TANKSEQ")
        sqlStat.AppendLine("  ,DTLPOLPOD")
        sqlStat.AppendLine("  ,DTLOFFICE")
        sqlStat.AppendLine("  ,DELFLG")
        sqlStat.AppendLine("  ,INITYMD")
        sqlStat.AppendLine("  ,UPDYMD")
        sqlStat.AppendLine("  ,UPDUSER")
        sqlStat.AppendLine("  ,UPDTERMID")
        sqlStat.AppendLine("  ,RECEIVEYMD")
        sqlStat.AppendLine(" )")

        sqlStat.AppendLine(" SELECT ")

        sqlStat.AppendLine("    @CLOSINGMONTH as CLOSINGMONTH,")
        sqlStat.AppendLine("    @CLOSINGGROUP as CLOSINGGROUP,")
        sqlStat.AppendLine("    jv.INVOICEDBY as INVOICEDBY,")
        sqlStat.AppendLine("    jv.CONTRACTORFIX as CONTRACTORFIX,")
        sqlStat.AppendLine("    jv.ORDERNO as ORDERNO,")
        sqlStat.AppendLine("    jv.TANKNO as TANKNO,")
        sqlStat.AppendLine("    jv.COSTCODE as COSTCODE,")
        sqlStat.AppendLine("    cc.COSTTYPE as COSTTYPE,")
        sqlStat.AppendLine("    cc.CRACCOUNT as CRACCOUNT,")
        sqlStat.AppendLine("    cc.DBACCOUNT as DBACCOUNT,")
        sqlStat.AppendLine("    cc.CRACCOUNTFORIGN as CRACCOUNTFORIGN,")
        sqlStat.AppendLine("    cc.DBACCOUNTFORIGN as DBACCOUNTFORIGN,")
        sqlStat.AppendLine("    cc.OFFCRACCOUNT as OFFCRACCOUNT,")
        sqlStat.AppendLine("    cc.OFFDBACCOUNT as OFFDBACCOUNT,")
        sqlStat.AppendLine("    cc.OFFCRACCOUNTFORIGN as OFFCRACCOUNTFORIGN,")
        sqlStat.AppendLine("    cc.OFFDBACCOUNTFORIGN as OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("    cc.CRSEGMENT1 as CRSEGMENT1,")
        sqlStat.AppendLine("    cc.DBSEGMENT1 as DBSEGMENT1,")
        sqlStat.AppendLine("    cc.CRGENERALPURPOSE as CRGENERALPURPOSE,")
        'sqlStat.AppendLine("    case when cc.CRGENERALPURPOSE = '1' and (jv.UAG_USD * rt.EXRATE) < 100000.0 then '1'")
        'sqlStat.AppendLine("            when cc.CRGENERALPURPOSE = '1' and (jv.UAG_USD * rt.EXRATE) between 100000.0 and 199999.0 then '2'")
        'sqlStat.AppendLine("            when cc.CRGENERALPURPOSE = '1' then '9'")
        sqlStat.AppendLine("    case when cc.CRGENERALPURPOSE = '1' then ov.ACCCURRENCYSEGMENT ")
        sqlStat.AppendLine("            else '0'")
        sqlStat.AppendLine("    end as 'CREGENPURPOSE',")
        sqlStat.AppendLine("    cc.DBGENERALPURPOSE as DBGENERALPURPOSE,")
        'sqlStat.AppendLine("    case when cc.DBGENERALPURPOSE = '1' and (jv.UAG_USD * rt.EXRATE) < 100000.0 then '1'")
        'sqlStat.AppendLine("            when cc.DBGENERALPURPOSE = '1' and (jv.UAG_USD * rt.EXRATE) between 100000.0 and 199999.0 then '2'")
        'sqlStat.AppendLine("            when cc.DBGENERALPURPOSE = '1' then '9'")
        sqlStat.AppendLine("    case when cc.DBGENERALPURPOSE = '1' then ov.ACCCURRENCYSEGMENT ")
        sqlStat.AppendLine("            else '0'")
        sqlStat.AppendLine("    end as 'DEBGENPURPOSE',")
        sqlStat.AppendLine("    jv.COUNTRYCODE as COUNTRYCODE,")
        sqlStat.AppendLine("    jv.CURRENCYCODE as CURRENCYCODE,")
        sqlStat.AppendLine("    jv.TAXATION as TAXATION,")
        sqlStat.AppendLine("    jv.AMOUNTFIX as AMOUNTFIX,")
        sqlStat.AppendLine("    jv.LOCALBR as LOCALBR,")
        sqlStat.AppendLine("    jv.LOCALRATE as LOCALRATE,")
        sqlStat.AppendLine("    jv.AMOUNTPAYODR as AMOUNTPAYODR,")
        sqlStat.AppendLine("    jv.LOCALPAYODR as LOCALPAYODR,")
        sqlStat.AppendLine("    jv.TAXBR as TAXBR,")
        sqlStat.AppendLine("    jv.LOCALRATESOA as LOCALRATESOA,")
        sqlStat.AppendLine("    jv.AMOUNTPAY as AMOUNTPAY,")
        sqlStat.AppendLine("    jv.LOCALPAY as LOCALPAY,")
        sqlStat.AppendLine("    jv.TAXPAY as TAXPAY,")
        sqlStat.AppendLine("    jv.UAG_USD as UAG_USD,")
        sqlStat.AppendLine("    jv.UAG_LOCAL as UAG_LOCAL,")
        'sqlStat.AppendLine("    jv.UAG_USD * rt.EXRATE as UAG_JPY,")
        sqlStat.AppendLine("    case when jv.CURRENCYCODE = 'JPY' then jv.AMOUNTFIX ")
        sqlStat.AppendLine("         else round(jv.UAG_USD * rt.EXRATE,0) ")
        sqlStat.AppendLine("    end as UAG_JPY,")
        'sqlStat.AppendLine("    case when COSTTYPE = '1' and isnull(jb.EXSHIPRATE1,0.0) > 0.0 then jv.AMOUNTFIX ")
        sqlStat.AppendLine("    case when COSTTYPE = '1' and isnull(jb.EXSHIPRATE1,0.0) > 0.0 and jv.CURRENCYCODE <> 'JPY' then jv.AMOUNTFIX ")
        sqlStat.AppendLine("         when COSTTYPE = '1' then jv.UAG_USD ")
        sqlStat.AppendLine("         when COSTTYPE = '2' then jv.UAG_USD ")
        sqlStat.AppendLine("    end as UAG_USD_SHIP,")
        'sqlStat.AppendLine("    case when COSTTYPE = '1' and isnull(jb.EXSHIPRATE1,0.0) > 0.0 then round(jv.AMOUNTFIX * jb.EXSHIPRATE1,0) ")
        sqlStat.AppendLine("    case when COSTTYPE = '1' and isnull(jb.EXSHIPRATE1,0.0) > 0.0 and jv.CURRENCYCODE <> 'JPY' then round(jv.AMOUNTFIX * jb.EXSHIPRATE1,0) ")
        sqlStat.AppendLine("         when COSTTYPE = '1' and jv.CURRENCYCODE = 'JPY' then jv.AMOUNTFIX ")
        sqlStat.AppendLine("         when COSTTYPE = '1' then round(jv.UAG_USD * rt.EXRATE,0) ")
        sqlStat.AppendLine("         when COSTTYPE = '2' and jv.CURRENCYCODE = 'JPY' then jv.AMOUNTFIX ")
        sqlStat.AppendLine("         when COSTTYPE = '2' then round(jv.UAG_USD * rt.EXRATE,0) ")
        sqlStat.AppendLine("    end as UAG_JPY_SHIP,")
        sqlStat.AppendLine("    jv.USD_USD as USD_USD,")
        sqlStat.AppendLine("    jv.USD_LOCAL as USD_LOCAL,")
        sqlStat.AppendLine("    jv.LOCAL_USD as LOCAL_USD,")
        sqlStat.AppendLine("    jv.LOCAL_LOCAL as LOCAL_LOCAL,")
        sqlStat.AppendLine("    jv.ACTUALDATE as ACTUALDATE,")
        sqlStat.AppendLine("    jv.SOAAPPDATE as SOAAPPDATE,")
        sqlStat.AppendLine("    jv.REMARK as REMARK,")
        sqlStat.AppendLine("    jv.BRID as BRID,")
        sqlStat.AppendLine("    jv.APPLYID as APPLYID,")
        sqlStat.AppendLine("    jv.SOACODE as SOACODE,")
        sqlStat.AppendLine("    jv.SOASHORTCODE as SOASHORTCODE,")
        sqlStat.AppendLine("    jv.REPORTMONTH as REPORTMONTH,")
        sqlStat.AppendLine("    jv.REPORTMONTHORG as REPORTMONTHORG,")
        sqlStat.AppendLine("    rt.EXRATE as REPORTRATEJPY,")
        sqlStat.AppendLine("    jv.SHIPDATE as SHIPDATE,")
        sqlStat.AppendLine("    jv.DOUTDATE as DOUTDATE,")
        sqlStat.AppendLine("    isnull(jb.LOADING,0) as LOADING,")
        sqlStat.AppendLine("    isnull(jb.STEAMING,0) as STEAMING,")
        sqlStat.AppendLine("    isnull(jb.TIP,0) as TIP,")
        sqlStat.AppendLine("    isnull(jb.EXTRA,0) as EXTRA,")
        sqlStat.AppendLine("    isnull(jb.LOADING,0) + isnull(jb.STEAMING,0) + isnull(jb.TIP,0) + isnull(jb.EXTRA,0) as ROOTDAYS,")
        sqlStat.AppendLine("    jv.DATAIDODR as DATAIDODR,")
        sqlStat.AppendLine("    jv.ACCRECRATE as ACCRECRATE,")
        sqlStat.AppendLine("    jv.ACCRECYEN as ACCRECYEN,")
        sqlStat.AppendLine("    jv.ACCRECFOREIGN as ACCRECFOREIGN,")
        sqlStat.AppendLine("    isnull(jb.EXSHIPRATE1,0.0) as EXSHIPRATE1,")
        sqlStat.AppendLine("    isnull(jb.INSHIPRATE1,0.0) as INSHIPRATE1,")
        sqlStat.AppendLine("    isnull(jb.EXSHIPRATE2,0.0) as EXSHIPRATE2,")
        sqlStat.AppendLine("    isnull(jb.INSHIPRATE2,0.0) as INSHIPRATE2,")
        sqlStat.AppendLine("    jv.TANKSEQ as TANKSEQ,")
        sqlStat.AppendLine("    jv.DTLPOLPOD as DTLPOLPOD,")
        sqlStat.AppendLine("    jv.DTLOFFICE as DTLOFFICE,")
        sqlStat.AppendLine("    '" & CONST_FLAG_NO & "' as DELFLG,")
        sqlStat.AppendLine("    @ENTYMD as INITYMD,")
        sqlStat.AppendLine("    @ENTYMD as UPDYMD,")
        sqlStat.AppendLine("    @UPDUSER as UPDUSER,")
        sqlStat.AppendLine("    @UPDTERMID as UPDTERMID,")
        sqlStat.AppendLine("    @RECEIVEYMD as RECEIVEYMD")

        sqlStat.AppendLine("from GBT0008_JOTSOA_VALUE jv")
        sqlStat.AppendLine("inner join ( select cc.COSTCODE,cc.CRACCOUNT,cc.DBACCOUNT,cc.CRACCOUNTFORIGN,cc.DBACCOUNTFORIGN,")
        sqlStat.AppendLine("                    cc.OFFCRACCOUNT,cc.OFFDBACCOUNT,cc.OFFCRACCOUNTFORIGN,cc.OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("                    cc.CRGENERALPURPOSE, cc.DBGENERALPURPOSE, cc.CRSEGMENT1, cc.DBSEGMENT1,")
        sqlStat.AppendLine("                    case when cc.CLASS2 <> '' then '1' else '2' end as 'COSTTYPE'")
        sqlStat.AppendLine("                from GBM0010_CHARGECODE cc")
        sqlStat.AppendLine("                where cc.DELFLG <> 'Y'")
        sqlStat.AppendLine("                and   cc.CRACCOUNT <> ''")
        sqlStat.AppendLine("                group by cc.COSTCODE,cc.CRACCOUNT,cc.DBACCOUNT,cc.CRACCOUNTFORIGN,cc.DBACCOUNTFORIGN,")
        sqlStat.AppendLine("                        cc.OFFCRACCOUNT,cc.OFFDBACCOUNT,cc.OFFCRACCOUNTFORIGN,cc.OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("                        cc.CRGENERALPURPOSE, cc.DBGENERALPURPOSE, cc.CRSEGMENT1, cc.DBSEGMENT1,")
        sqlStat.AppendLine("                        case when cc.CLASS2 <> '' then '1' else '2' end ) cc")
        sqlStat.AppendLine("    on cc.COSTCODE = jv.COSTCODE")
        sqlStat.AppendLine("left outer join GBT0013_JOTSOA_BASE jb")
        sqlStat.AppendLine("    on jb.ORDERNO = jv.ORDERNO")
        sqlStat.AppendLine("    and jb.REPORTMONTH = jv.CLOSINGMONTH")
        sqlStat.AppendLine("    and jb.DELFLG <> @DELFLG")
        sqlStat.AppendLine("inner join GBT0005_ODR_VALUE ov")
        sqlStat.AppendLine("    on ov.DATAID = jv.DATAIDODR")
        sqlStat.AppendLine("    and ov.DELFLG <> @DELFLG")
        sqlStat.AppendLine("inner join GBM0020_EXRATE rt")
        sqlStat.AppendLine("    on rt.COUNTRYCODE = 'JP'")
        sqlStat.AppendLine("    and rt.CURRENCYCODE = 'JPY'")
        sqlStat.AppendLine("    and rt.TARGETYM = @TARGETYM")
        sqlStat.AppendLine("    and rt.DELFLG <> @DELFLG")
        sqlStat.AppendLine("where jv.CLOSINGMONTH = @CLOSINGMONTH")
        sqlStat.AppendLine("and   jv.CLOSINGGROUP = @CLOSINGGROUP")
        sqlStat.AppendLine("and   jv.DELFLG <> @DELFLG")
        ' 前月案分計上済み、当月未計上
        sqlStat.AppendLine("UNION")
        sqlStat.AppendLine(" SELECT ")
        sqlStat.AppendLine("    @CLOSINGMONTH as CLOSINGMONTH,")
        sqlStat.AppendLine("    @CLOSINGGROUP as CLOSINGGROUP,")
        sqlStat.AppendLine("    aw.INVOICEDBY as INVOICEDBY,")
        sqlStat.AppendLine("    aw.CONTRACTORFIX as CONTRACTORFIX,")
        sqlStat.AppendLine("    aw.ORDERNO as ORDERNO,")
        sqlStat.AppendLine("    aw.TANKNO as TANKNO,")
        sqlStat.AppendLine("    aw.COSTCODE as COSTCODE,")
        sqlStat.AppendLine("    aw.COSTTYPE as COSTTYPE,")
        sqlStat.AppendLine("    aw.CRACCOUNT as CRACCOUNT,")
        sqlStat.AppendLine("    aw.DBACCOUNT as DBACCOUNT,")
        sqlStat.AppendLine("    aw.CRACCOUNTFORIGN as CRACCOUNTFORIGN,")
        sqlStat.AppendLine("    aw.DBACCOUNTFORIGN as DBACCOUNTFORIGN,")
        sqlStat.AppendLine("    aw.OFFCRACCOUNT as OFFCRACCOUNT,")
        sqlStat.AppendLine("    aw.OFFDBACCOUNT as OFFDBACCOUNT,")
        sqlStat.AppendLine("    aw.OFFCRACCOUNTFORIGN as OFFCRACCOUNTFORIGN,")
        sqlStat.AppendLine("    aw.OFFDBACCOUNTFORIGN as OFFDBACCOUNTFORIGN,")
        sqlStat.AppendLine("    aw.CRSEGMENT1 as CRSEGMENT1,")
        sqlStat.AppendLine("    aw.DBSEGMENT1 as DBSEGMENT1,")
        sqlStat.AppendLine("    aw.CRGENERALPURPOSE as CRGENERALPURPOSE,")
        sqlStat.AppendLine("    aw.DEBGENPURPOSE as DEBGENPURPOSE,")
        sqlStat.AppendLine("    aw.DBGENERALPURPOSE as DBGENERALPURPOSE,")
        sqlStat.AppendLine("    aw.CREGENPURPOSE as CREGENPURPOSE,")
        sqlStat.AppendLine("    aw.COUNTRYCODE as COUNTRYCODE,")
        sqlStat.AppendLine("    aw.CURRENCYCODE as CURRENCYCODE,")
        sqlStat.AppendLine("    aw.TAXATION as TAXATION,")
        sqlStat.AppendLine("    aw.AMOUNTFIX as AMOUNTFIX,")
        sqlStat.AppendLine("    aw.LOCALBR as LOCALBR,")
        sqlStat.AppendLine("    aw.LOCALRATE as LOCALRATE,")
        sqlStat.AppendLine("    aw.AMOUNTPAYODR as AMOUNTPAYODR,")
        sqlStat.AppendLine("    aw.LOCALPAYODR as LOCALPAYODR,")
        sqlStat.AppendLine("    aw.TAXBR as TAXBR,")
        sqlStat.AppendLine("    aw.LOCALRATESOA as LOCALRATESOA,")
        sqlStat.AppendLine("    aw.AMOUNTPAY as AMOUNTPAY,")
        sqlStat.AppendLine("    aw.LOCALPAY as LOCALPAY,")
        sqlStat.AppendLine("    aw.TAXPAY as TAXPAY,")
        sqlStat.AppendLine("    aw.UAG_USD as UAG_USD,")
        sqlStat.AppendLine("    aw.UAG_LOCAL as UAG_LOCAL,")
        sqlStat.AppendLine("    aw.UAG_USD * rt.EXRATE as UAG_JPY,")
        sqlStat.AppendLine("    aw.UAG_USD_SHIP as UAG_USD_SHIP,")
        sqlStat.AppendLine("    case when aw.COSTTYPE = '2' and aw.CURRENCYCODE <> 'JPY' then aw.UAG_USD_SHIP * rt.EXRATE ")
        sqlStat.AppendLine("         else aw.UAG_JPY_SHIP end as UAG_JPY_SHIP,")
        sqlStat.AppendLine("    aw.USD_USD as USD_USD,")
        sqlStat.AppendLine("    aw.USD_LOCAL as USD_LOCAL,")
        sqlStat.AppendLine("    aw.LOCAL_USD as LOCAL_USD,")
        sqlStat.AppendLine("    aw.LOCAL_LOCAL as LOCAL_LOCAL,")
        sqlStat.AppendLine("    aw.ACTUALDATE as ACTUALDATE,")
        sqlStat.AppendLine("    aw.SOAAPPDATE as SOAAPPDATE,")
        sqlStat.AppendLine("    aw.REMARK as REMARK,")
        sqlStat.AppendLine("    aw.BRID as BRID,")
        sqlStat.AppendLine("    aw.APPLYID as APPLYID,")
        sqlStat.AppendLine("    aw.SOACODE as SOACODE,")
        sqlStat.AppendLine("    aw.SOASHORTCODE as SOASHORTCODE,")
        sqlStat.AppendLine("    aw.REPORTMONTH as REPORTMONTH,")
        sqlStat.AppendLine("    aw.REPORTMONTHORG as REPORTMONTHORG,")
        sqlStat.AppendLine("    rt.EXRATE as REPORTRATEJPY,")
        sqlStat.AppendLine("    aw.SHIPDATE as SHIPDATE,")
        sqlStat.AppendLine("    aw.DOUTDATE as DOUTDATE,")
        sqlStat.AppendLine("    aw.LOADING as LOADING,")
        sqlStat.AppendLine("    aw.STEAMING as STEAMING,")
        sqlStat.AppendLine("    aw.TIP as TIP,")
        sqlStat.AppendLine("    aw.EXTRA as EXTRA,")
        sqlStat.AppendLine("    aw.ROUTEDAYS as ROUTEDAYS,")
        sqlStat.AppendLine("    aw.DATAIDODR as DATAIDODR,")
        sqlStat.AppendLine("    aw.ACCRECRATE as ACCRECRATE,")
        sqlStat.AppendLine("    aw.ACCRECYEN as ACCRECYEN,")
        sqlStat.AppendLine("    aw.ACCRECFOREIGN as ACCRECFOREIGN,")
        sqlStat.AppendLine("    aw.EXSHIPRATE1 as EXSHIPRATE1,")
        sqlStat.AppendLine("    aw.INSHIPRATE1 as INSHIPRATE1,")
        sqlStat.AppendLine("    aw.EXSHIPRATE2 as EXSHIPRATE2,")
        sqlStat.AppendLine("    aw.INSHIPRATE2 as INSHIPRATE2,")
        sqlStat.AppendLine("    aw.TANKSEQ as TANKSEQ,")
        sqlStat.AppendLine("    aw.DTLPOLPOD as DTLPOLPOD,")
        sqlStat.AppendLine("    aw.DTLOFFICE as DTLOFFICE,")
        sqlStat.AppendLine("    aw.DELFLG as DELFLG,")
        sqlStat.AppendLine("    @ENTYMD as INITYMD,")
        sqlStat.AppendLine("    @ENTYMD as UPDYMD,")
        sqlStat.AppendLine("    @UPDUSER as UPDUSER,")
        sqlStat.AppendLine("    @UPDTERMID as UPDTERMID,")
        sqlStat.AppendLine("    @RECEIVEYMD as RECEIVEYMD")
        sqlStat.AppendLine("from GBT0015_AC_WORK aw")
        sqlStat.AppendLine("inner join GBM0020_EXRATE rt")
        sqlStat.AppendLine("    on rt.COUNTRYCODE = 'JP'")
        sqlStat.AppendLine("    and rt.CURRENCYCODE = 'JPY'")
        sqlStat.AppendLine("    and rt.TARGETYM = @TARGETYM")
        sqlStat.AppendLine("    and rt.DELFLG <> @DELFLG")
        sqlStat.AppendLine("where aw.CLOSINGMONTH = @BFCLOSINGMONTH")
        sqlStat.AppendLine("and   aw.CLOSINGGROUP = @CLOSINGGROUP")
        sqlStat.AppendLine("and   aw.DELFLG <> @DELFLG")
        sqlStat.AppendLine("and  ( aw.ROUTEDAYS - (DATEDIFF(day,aw.DOUTDATE, EOMONTH(@CLOSINGMONTH + '/01'))+1)) > 0")
        sqlStat.AppendLine("and   aw.REPORTMONTH <= @BFCLOSINGMONTH")
        sqlStat.AppendLine("and  not exists ( ")
        sqlStat.AppendLine("                  select * from GBT0008_JOTSOA_VALUE jv")
        sqlStat.AppendLine("                  where jv.DATAIDODR = aw.DATAIDODR ")
        sqlStat.AppendLine("                  and jv.CLOSINGMONTH = @CLOSINGMONTH")
        sqlStat.AppendLine("                  and jv.CLOSINGGROUP = aw.CLOSINGGROUP")
        sqlStat.AppendLine("                  and jv.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                )")

        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
             sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = Date.Now
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                .Add("@NOWDATE", SqlDbType.DateTime).Value = Now

                .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = parmBillingYm
                .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = parmCountry
                .Add("@TARGETYM", SqlDbType.DateTime).Value = CDate(parmBillingYm & "/01")

                ' 前月
                .Add("@BFCLOSINGMONTH", SqlDbType.NVarChar).Value = CDate(parmBillingYm & "/01").AddMonths(-1).ToString("yyyy/MM")
                ' 当月末


            End With
            sqlCmd.ExecuteNonQuery()
        End Using

        'Return C_MESSAGENO.NORMAL 'ここまでくれば正常
    End Sub

End Class

