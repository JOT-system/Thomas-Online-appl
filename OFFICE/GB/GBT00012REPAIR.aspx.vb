Imports System.Data.SqlClient
Imports System.Net
Imports BASEDLL

''' <summary>
''' リペアブレーカー単票画面クラス
''' </summary>
Public Class GBT00012REPAIR
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00012" '自身のMAPID
    Private Before As String = "BeforeRepair"
    Private After As String = "AfterRepair"

    Private Const BEFORE_SAVE_MSG As String = "Do you want to execute it after saving?"

    'VIEWSTATE名
    Private Const CONST_VS_NAME_BEFORE_PREV_VAL As String = "BEFOREPREVVAL"
    Private Const CONST_VS_NAME_BEFORE_CURRENT_VAL As String = "BEFORECURRENTVAL"
    Private Const CONST_VS_NAME_AFTER_PREV_VAL As String = "AFTERPREVVAL"
    Private Const CONST_VS_NAME_AFTER_CURRENT_VAL As String = "AFTERCURRENTVAL"
    Private Const CONST_DIRNAME_REPAIR As String = "REPAIR"

    Private Const CONST_VS_NAME_DICBRINFO As String = "DICBRINFO"
    Private Const CONST_VS_NAME_COSTLIST As String = "COSTLIST"

    '初期情報保持用ViewState
    Private Const CONST_VS_NAME_INIT_ORGINFO As String = "INITORGANIZERINFO"
    Private Const CONST_VS_NAME_INIT_BRINFO As String = "INITDICBRINFO"
    Private Const CONST_VS_NAME_INIT_COSTLIST As String = "INITCOSTLIST"
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 画面退避用アイテム
    ''' </summary>
    ''' <returns></returns>
    Public Property DisplayItems As GBT00012RITEMS
    ''' <summary>
    ''' ポストバック時画面上の情報を保持
    ''' </summary>
    Private AfterRepairAttachment As DataTable
    Private BeforeRepairAttachment As DataTable

    Public isBeforeApploveFlg As Boolean

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
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '遷移元よりデータ取得
                '****************************************
                Dim ds As DataSet = New DataSet
                If Me.GetPrevDisplayInfo(ds) <> C_MESSAGENO.NORMAL Then
                    Return
                End If

                Dim apSt = GetStatus(Me.hdnBrId.Value)

                If Me.hdnStatus.Value = C_APP_STATUS.APPROVED AndAlso apSt = C_APP_STATUS.APPROVED Then
                    isBeforeApploveFlg = False
                Else
                    isBeforeApploveFlg = True
                End If

                BeforeRepairAttachment = CommonFunctions.GetInitAttachmentFileList(Me.hdnBrId.Value, CONST_DIRNAME_REPAIR, CONST_MAPID, isBeforeApploveFlg, Before)
                AfterRepairAttachment = CommonFunctions.GetInitAttachmentFileList(Me.hdnBrId.Value, CONST_DIRNAME_REPAIR, CONST_MAPID, isBeforeApploveFlg, After)

                ViewState(CONST_VS_NAME_BEFORE_PREV_VAL) = BeforeRepairAttachment '保存前の情報
                ViewState(CONST_VS_NAME_BEFORE_CURRENT_VAL) = BeforeRepairAttachment '編集中の情報保持用
                ViewState(CONST_VS_NAME_AFTER_PREV_VAL) = AfterRepairAttachment '保存前の情報
                ViewState(CONST_VS_NAME_AFTER_CURRENT_VAL) = AfterRepairAttachment '編集中の情報保持用

                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = "RepairBreaker"
                COA0031ProfMap.COA0031GetDisplayTitle()

                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})

                    Return
                End If
                '****************************************
                '取得データを画面展開
                '****************************************
                '保持項目設定
                SetInitData(ds.Tables("ORGANIZER_INFO"))
                'オーナー情報
                SetDisplayOrganizerInfo(ds.Tables("ORGANIZER_INFO"))
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'タブによる表示切替
                '****************************************
                Me.tabRepair.Attributes.Add("class", "selected")
                Me.hdnSelectedTabId.Value = Me.tabRepair.ClientID
                visibleControl(True, Me.tabRepair.ClientID)

                enabledControls(True)
                '右ボックス帳票タブ
                Dim errMsg As String = ""
                errMsg = Me.RightboxInit(True, COSTITEM.CostItemGroup.Repair)
                If errMsg <> "" Then
                    'Me.lblFooterMessage.Text = errMsg
                End If

                SetCostGridItem(Me.tabFileUp.ClientID, Me.hdnSelectedTabId.Value)

                Dim initFlg As String = "1"
                '初回自動計算
                CalcSummaryCostLocal(initFlg)
                CalcSummaryCostUsd(initFlg)
                CalcSummaryCostAppUsd(initFlg)
                CostEnabledControls()
                FileUppEnabledControls()

                txtTankUsage_Change()
                txtDeleteFlag_Change()
                txtDepoCode_Change()

                '****************************************
                'Fileタブ初期処理
                '****************************************
                '添付ファイル
                Me.dViewRep.DataSource = BeforeRepairAttachment
                Me.dViewRep.DataBind()

                Me.dDoneViewRep.DataSource = AfterRepairAttachment
                Me.dDoneViewRep.DataBind()

                'メッセージ設定
                If hdnMsgId.Value <> "" Then
                    CommonFunctions.ShowMessage(hdnMsgId.Value, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                End If
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                Dim currentTab = GetCurrentTab()
                If currentTab = COSTITEM.CostItemGroup.Repair Then
                    SaveGridItem(currentTab)
                End If

                Me.BeforeRepairAttachment = CollectDispValues(Before)
                Me.AfterRepairAttachment = CollectDispValues(After)
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
                ' タブクリック判定
                '**********************
                Dim clickedTabCont As Control = Me.FindControl(Me.hdnSelectedTabId.Value)
                Dim clickedTabObj As HtmlControls.HtmlControl = Nothing
                If clickedTabCont IsNot Nothing Then
                    clickedTabObj = DirectCast(clickedTabCont, HtmlControls.HtmlControl)
                End If
                If clickedTabObj IsNot Nothing AndAlso clickedTabObj.Attributes("class") <> "selected" Then
                    TabClick(Me.hdnSelectedTabId.Value)
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
                ' 費用グリッド削除ボタン押下時イベント
                '**********************
                If Me.hdnDelteCostUniqueIndex.Value <> "" Then
                    btnListDelete_Click()
                End If
                '**********************
                ' 備考・初見入力ボックス表示
                '**********************
                If Me.hdnRemarkboxField.Value <> "" Then
                    'その他備考
                    Dim targetControl As Label = DirectCast(Me.FindControl(Me.hdnRemarkboxField.Value), Label)
                    If Me.hdnRemarkboxField.Value = "lblCostRemarks" Then
                        '費用項目 備考
                        DisplayCostRemarks(True)
                    Else
                        If targetControl.Enabled = False Then
                            Me.btnRemarkInputOk.Disabled = True
                            Me.txtRemarkInput.ReadOnly = True
                        Else
                            Me.btnRemarkInputOk.Disabled = False
                            Me.txtRemarkInput.ReadOnly = False
                        End If
                        Me.txtRemarkInput.Text = HttpUtility.HtmlDecode(targetControl.Text)
                    End If
                    'マルチライン入力ボックスの表示
                    Me.divRemarkInputBoxWrapper.Style("display") = "block"
                End If

                '**********************
                ' ダブルクリック個別処理
                '**********************
                If Me.hdnDbClickField.Value <> "" Then
                    Dim fldName As String = Me.hdnDbClickField.Value
                    Dim dbEventName As String = Me.hdnDbClickField.Value & "_DbClick"
                    Me.hdnDbClickField.Value = ""
                    'イベントが存在する場合は実行存在しない場合はスキップ
                    Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(dbEventName)
                    If mi IsNot Nothing Then
                        CallByName(Me, dbEventName, CallType.Method, Nothing)
                    End If
                End If

                '**********************
                ' 自動計算処理
                '**********************
                If Me.hdnCalcFunctionName.Value <> "" Then
                    Dim funcName As String = Me.hdnCalcFunctionName.Value
                    Me.hdnCalcFunctionName.Value = ""
                    Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(funcName)
                    If mi IsNot Nothing Then
                        CallByName(Me, funcName, CallType.Method, Nothing)
                    End If
                    '費用項目非活性制御
                    CostEnabledControls()
                End If
                '**********************
                ' ファイルアップロード処理
                '**********************
                If Me.hdnListUpload.Value IsNot Nothing AndAlso Me.hdnListUpload.Value <> "" Then
                    If Me.hdnListUpload.Value = "XLS_LOADED" Then
                        UploadExcel()
                    ElseIf Me.hdnListUpload.Value = "FILE_LOADED" Then
                        UploadFile()
                    End If

                    Me.hdnListUpload.Value = ""
                End If
                '**********************
                ' Detail File内容表示処理
                '**********************
                If Me.hdnFileDisplay.Value IsNot Nothing AndAlso Me.hdnFileDisplay.Value <> "" Then
                    FileDisplay()
                    hdnFileDisplay.Value = ""
                End If
                '**********************
                ' 一括チェック処理
                '**********************
                If Me.hdnBulkCheckChange.Value IsNot Nothing AndAlso Me.hdnBulkCheckChange.Value <> "" Then
                    If Me.hdnBulkCheckChange.Value = "true" Then
                        BulkCheck()
                    End If
                    hdnBulkCheckChange.Value = ""
                End If
                '**********************
                ' Leaseチェック処理
                '**********************
                If Me.hdnLeaseCheckChange.Value IsNot Nothing AndAlso Me.hdnLeaseCheckChange.Value <> "" Then
                    LeaseCheck()
                    hdnLeaseCheckChange.Value = ""
                End If
                '**********************
                ' チェック変更処理
                '**********************
                If Me.hdnCheckAppChange.Value IsNot Nothing AndAlso Me.hdnCheckAppChange.Value <> "" Then
                    CheckApp()
                    hdnCheckAppChange.Value = ""
                    hdnCheckUniqueNumber.Value = ""
                End If

            End If

            '**********************
            ' Help表示
            '**********************
            If Me.hdnHelpChange.Value IsNot Nothing AndAlso Me.hdnHelpChange.Value <> "" Then
                DivShowHelp_DoubleClick(CONST_MAPID)
                Me.hdnHelpChange.Value = ""
            End If
            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            ViewState(CONST_VS_NAME_BEFORE_CURRENT_VAL) = BeforeRepairAttachment '編集中の情報保持用
            ViewState(CONST_VS_NAME_AFTER_CURRENT_VAL) = AfterRepairAttachment '編集中の情報保持用

        Catch ex As Threading.ThreadAbortException
            Return
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.SYSTEMADM)})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.SYSTEMADM
            COA0003LogFile.COA0003WriteLog()

            Return
        Finally
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        End Try
    End Sub
    ''' <summary>
    ''' 画面上のデータを取得し設定
    ''' </summary>
    ''' <returns>画面情報より取得したDataTable</returns>
    Private Function CollectDispValues(ByVal suffix As String) As DataTable
        Dim retDt As New DataTable
        Dim dtTarget As DataTable = Nothing
        Dim gvTarget As Repeater = Nothing
        If suffix = Before Then
            dtTarget = DirectCast(ViewState(CONST_VS_NAME_BEFORE_CURRENT_VAL), DataTable)
            gvTarget = dViewRep
        Else
            dtTarget = DirectCast(ViewState(CONST_VS_NAME_AFTER_CURRENT_VAL), DataTable)
            gvTarget = dDoneViewRep
        End If
        '添付ファイルグリッド
        Dim dtAttachment As DataTable = CommonFunctions.DeepCopy(dtTarget)
        For Each repItem As RepeaterItem In gvTarget.Items
            Dim fileName As Label = DirectCast(repItem.FindControl("lblRepFileName"), Label)
            Dim deleteFlg As TextBox = DirectCast(repItem.FindControl("txtRepDelFlg"), TextBox)
            If fileName Is Nothing OrElse deleteFlg Is Nothing Then
                Continue For
            End If
            Dim qAttachment = From attachmentItem In dtAttachment Where attachmentItem("FILENAME").Equals(fileName.Text)
            If qAttachment.Any Then
                qAttachment.FirstOrDefault.Item("DELFLG") = deleteFlg.Text
            End If
        Next
        retDt = dtAttachment
        Return retDt
    End Function
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
                        Dim wkDate As Date = Nothing
                        If Date.TryParseExact(txtobj.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, wkDate) Then
                            Me.hdnCalendarValue.Value = wkDate.ToString("yyyy/MM/dd")
                        Else
                            Me.hdnCalendarValue.Value = txtobj.Text
                        End If

                        Me.mvLeft.Focus()
                    End If
                Case vLeftCost.ID
                    Dim dt As DataTable = GetCost()
                    With Me.lbCost
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With
                Case Me.vLeftDelFlg.ID
                    Dim drIndex As Integer = 0
                    Dim dtAttachment As DataTable = Nothing
                    If Me.hdnTextDbClickField.Value.StartsWith("dViewRep_txtRepDelFlg_") Then
                        drIndex = CInt(Me.hdnTextDbClickField.Value.Replace("dViewRep_txtRepDelFlg_", ""))
                        dtAttachment = Me.BeforeRepairAttachment
                    ElseIf Me.hdnTextDbClickField.Value.StartsWith("dDoneViewRep_txtRepDelFlg_") Then
                        drIndex = CInt(Me.hdnTextDbClickField.Value.Replace("dDoneViewRep_txtRepDelFlg_", ""))
                        dtAttachment = Me.AfterRepairAttachment
                    Else
                        SetDelFlgListItem()
                        Return
                    End If

                    Dim drTargetAttachmentRow = dtAttachment.Rows(drIndex)
                    Dim findLbValue As ListItem = lbDelFlg.Items.FindByValue(Convert.ToString(drTargetAttachmentRow("DELFLG")))
                    If findLbValue IsNot Nothing Then
                        findLbValue.Selected = True
                    End If

                'タンク番号表示切替
                Case Me.vLeftTankNo.ID
                    SetTankNoListItem(Me.chkLeaseCheck.Checked)
                    'タンク表示切替
                Case Me.vLeftTankUsage.ID
                    SetTankUsageListItem()

                Case vLeftDepot.ID
                    Dim dt As DataTable = GetDepot()
                    With Me.lbDepot
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With
                Case vLeftProduct.ID
                    Dim dt As DataTable = GetProduct()
                    With Me.lbProduct
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With

            End Select
        End If

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()

        TextChangeCheck()
        If Me.hdnMsgboxShowFlg.Value = "1" Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, Me, submitButtonId:="btnExitMsgOk")
            Return
        End If

        exitProc()

    End Sub
    ''' <summary>
    ''' 終了OKボタン押下時
    ''' </summary>
    Public Sub btnExitMsgOk_Click()

        exitProc()

    End Sub

    ''' <summary>
    ''' 終了処理
    ''' </summary>
    Public Sub exitProc()

        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl
        '■■■ 画面遷移先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        '次画面の変数セット
        HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL
        '画面遷移実行
        Server.Transfer(COA0011ReturnUrl.URL)
    End Sub
    ''' <summary>
    ''' Excel出力ボタン押下時
    ''' </summary>
    Public Sub btnOutputExcel_Click()

        Dim ds As New DataSet
        Dim currentTab = GetCurrentTab()

        If currentTab <> COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        Dim reportMapId As String = ""

        '帳票出力
        Dim outUrl As String = ""

        Dim dt As DataTable = Nothing

        '画面情報を取得しデータテーブルに格納
        dt = CollectDisplayCostInfo()
        reportMapId = CONST_MAPID

        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDOWNLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                Return
            End If

            outUrl = COA0027ReportTable.URL

        End With

        '別画面でExcelを表示
        hdnPrintURL.Value = outUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

    End Sub
    ''' <summary>
    ''' FileDownloadボタン押下時
    ''' </summary>
    Public Sub btnOutputFile_Click()

        Dim currentTab = GetCurrentTab(Nothing)

        If currentTab = COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        Dim dtAttachment As DataTable = Nothing

        '修理前ファイル
        If currentTab = COSTITEM.CostItemGroup.FileUp Then
            dtAttachment = Me.BeforeRepairAttachment
        ElseIf currentTab = COSTITEM.CostItemGroup.DoneFileUp Then
            dtAttachment = Me.AfterRepairAttachment
        End If

        'ダウンロード対象有無
        If dtAttachment Is Nothing OrElse dtAttachment.Rows.Count = 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.FILENOTEXISTS, Me.lblFooterMessage, pageObject:=Me)
        End If
        Dim dlUrl As String = CommonFunctions.GetAttachmentCompressedFileUrl(dtAttachment, Me.lblBrNo.Text)

        If dlUrl <> "" Then
            Me.hdnPrintURL.Value = dlUrl
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
        End If
        '終了メッセージ
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDOWNLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

    End Sub
    ''' <summary>
    ''' 保存ボタン押下時イベント
    ''' </summary>
    Public Sub btnSave_Click()

        '保存処理
        saveProc()

        If Not hdnMsgId.Value = C_MESSAGENO.NORMALDBENTRY Then
            Return
        End If

        Dim thisPageUrl As String = Request.Url.ToString
        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
        Server.Transfer(Request.Url.LocalPath)

    End Sub

    ''' <summary>
    ''' 保存処理
    ''' </summary>
    Public Sub saveProc()
        Me.hdnMsgId.Value = ""
        Dim ds As New DataSet
        'オーナー基本情報のテキストボックス禁則文字変換
        Dim changeInvalidTextObjects As New List(Of TextBox) From
            {Me.txtTankNo, Me.txtDeleteFlag, Me.txtTankUsage}
        ChangeInvalidChar(changeInvalidTextObjects)
        '画面情報をデータテーブルに格納
        Dim orgDt As DataTable = CollectDisplayOrganizerInfo()
        Dim costDt As DataTable = CollectDisplayCostInfo()
        '費用項目の禁則文字置換
        ChangeInvalidChar(costDt, New List(Of String) From {"COSTCODE", "ITEM1"})

        '各種データテーブルをデータセットに格納
        ds.Tables.AddRange({orgDt, costDt})
        '入力チェック
        If CheckInput(ds, True, True) = False Then
            Return
        End If

        'リペアフラグを保持
        RepairFlagRetention(COSTITEM.CostItemGroup.Repair)

        'DB登録処理実行
        Dim errFlg As Boolean = True
        EntryData(ds, COSTITEM.CostItemGroup.Repair, errFlg)
        If Not errFlg Then
            Return
        End If

        Me.hdnMsgId.Value = C_MESSAGENO.NORMALDBENTRY

        If txtDeleteFlag.Text = CONST_FLAG_YES Then

            '削除の場合一覧に戻る
            exitProc()
        End If

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
                Case vLeftCost.ID
                    '費用選択時
                    If Me.lbCost.SelectedItem IsNot Nothing Then
                        Dim costCode As String = Me.lbCost.SelectedItem.Value
                        AddNewCostItem(costCode)
                    End If
                Case Me.vLeftDelFlg.ID 'アクティブなビューが削除フラグ
                    '削除フラグ選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbDelFlg.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbDelFlg.SelectedItem.Value
                            Me.lblDeleteFlagText.Text = Me.lbDelFlg.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblDeleteFlagText.Text = ""
                            txtobj.Focus()
                        End If
                    Else
                        Dim drIndex As Integer = 0
                        Dim dtAttachment As DataTable = Nothing
                        If Me.hdnTextDbClickField.Value.StartsWith("dViewRep_txtRepDelFlg_") Then
                            drIndex = CInt(Me.hdnTextDbClickField.Value.Replace("dViewRep_txtRepDelFlg_", ""))
                            dtAttachment = Me.BeforeRepairAttachment
                            Dim drTargetAttachmentRow = dtAttachment.Rows(drIndex)

                            If Me.lbDelFlg.SelectedItem IsNot Nothing Then
                                drTargetAttachmentRow("DELFLG") = Me.lbDelFlg.SelectedValue
                            Else
                                drTargetAttachmentRow("DELFLG") = ""
                            End If

                            dViewRep.DataSource = dtAttachment
                            dViewRep.DataBind()
                            Exit Select
                        ElseIf Me.hdnTextDbClickField.Value.StartsWith("dDoneViewRep_txtRepDelFlg_") Then
                            drIndex = CInt(Me.hdnTextDbClickField.Value.Replace("dDoneViewRep_txtRepDelFlg_", ""))
                            dtAttachment = Me.AfterRepairAttachment
                            Dim drTargetAttachmentRow = dtAttachment.Rows(drIndex)

                            If Me.lbDelFlg.SelectedItem IsNot Nothing Then
                                drTargetAttachmentRow("DELFLG") = Me.lbDelFlg.SelectedValue
                            Else
                                drTargetAttachmentRow("DELFLG") = ""
                            End If
                            dDoneViewRep.DataSource = dtAttachment
                            dDoneViewRep.DataBind()
                            Exit Select
                        End If

                    End If
                Case Me.vLeftTankNo.ID 'アクティブなビューがタンク番号
                    'タンク番号選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim tankNo As String = ""
                        If Me.lbTankNo.SelectedItem IsNot Nothing Then
                            tankNo = Me.lbTankNo.SelectedItem.Value
                            targetTextBox.Text = Me.lbTankNo.SelectedItem.Value
                            SetDisplayTankNo(tankNo)
                        End If
                    End If

                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If
                Case Me.vLeftTankUsage.ID
                    'タンク使用法選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbTankUsage.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbTankUsage.SelectedItem.Value
                            Me.lblTankUsageText.Text = Me.lbTankUsage.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblTankUsageText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                    Me.lbTankUsage.Items.Clear()
                Case Me.vLeftDepot.ID
                    'デポコード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbDepot.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbDepot.SelectedItem.Value
                            If Me.lbDepot.SelectedItem.Text.Contains(":") Then
                                Dim parts As String()
                                parts = Split(Me.lbDepot.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblDepoCodeText.Text = parts(1)
                            Else
                                Me.lblDepoCodeText.Text = Me.lbDepot.SelectedItem.Text
                            End If
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblDepoCodeText.Text = ""
                            txtobj.Focus()
                        End If

                        Dim dt As DataTable = GetDepot()
                        Dim findResult = (From item In dt
                                          Where Convert.ToString(item("CODE")) = Me.txtDepoCode.Text).FirstOrDefault

                        If findResult IsNot Nothing Then
                            Me.txtLocation.Text = findResult.Item("LOCATION").ToString
                        Else
                            Me.txtLocation.Text = ""
                        End If

                    End If

                Case Me.vLeftProduct.ID
                    '積載品コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If txtobj.ID = "txtLastProduct" Then
                            If Me.lbProduct.SelectedItem IsNot Nothing Then
                                txtobj.Text = Me.lbProduct.SelectedItem.Value
                                If Me.lbProduct.SelectedItem.Text.Contains(":") Then
                                    Dim parts As String()
                                    parts = Split(Me.lbProduct.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                    Me.lblLastProductText.Text = parts(1)
                                Else
                                    Me.lblLastProductText.Text = Me.lbProduct.SelectedItem.Text
                                End If
                                txtobj.Focus()
                            Else
                                txtobj.Text = ""
                                Me.lblLastProductText.Text = ""
                                txtobj.Focus()
                            End If

                        ElseIf txtobj.ID = "txtTwoAgoProduct" Then
                            If Me.lbProduct.SelectedItem IsNot Nothing Then
                                txtobj.Text = Me.lbProduct.SelectedItem.Value
                                If Me.lbProduct.SelectedItem.Text.Contains(":") Then
                                    Dim parts As String()
                                    parts = Split(Me.lbProduct.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                    Me.lblTwoAgoProductText.Text = parts(1)
                                Else
                                    Me.lblTwoAgoProductText.Text = Me.lbProduct.SelectedItem.Text
                                End If
                                txtobj.Focus()
                            Else
                                txtobj.Text = ""
                                Me.lblTwoAgoProductText.Text = ""
                                txtobj.Focus()
                            End If
                        End If

                    End If

                Case Else
                    '何もしない
            End Select
        End If
        '○ 画面左サイドボックス非表示は、画面JavaScriptで実行
        Me.hdnTextDbClickField.Value = ""
        Me.hdnIsLeftBoxOpen.Value = ""
        ClearLeftListData()
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
        ClearLeftListData()
    End Sub
    ''' <summary>
    ''' 備考入力ボックスのOKボタン押下時イベント
    ''' </summary>
    Public Sub btnRemarkInputOk_Click()
        If Me.hdnRemarkboxField.Value = "lblCostRemarks" Then
            DisplayCostRemarks(False)

        ElseIf Me.hdnRemarkboxField.Value = "lblRemarks" Then

            Dim targetControl As Label = DirectCast(Me.FindControl(Me.hdnRemarkboxField.Value), Label)
            targetControl.Text = HttpUtility.HtmlEncode(Me.txtRemarkInput.Text)

            Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
            brInfo = DirectCast(ViewState(CONST_VS_NAME_DICBRINFO), Dictionary(Of String, BreakerInfo))

            brInfo("INFO").Remark = HttpUtility.HtmlEncode(Me.txtRemarkInput.Text)

            ViewState(CONST_VS_NAME_DICBRINFO) = brInfo
        Else
            Dim targetControl As Label = DirectCast(Me.FindControl(Me.hdnRemarkboxField.Value), Label)
            targetControl.Text = HttpUtility.HtmlEncode(Me.txtRemarkInput.Text)
        End If

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

        Me.hdnRemarkInitFlg.Value = ""

        Me.hdnRemarkboxOpen.Value = ""
        Me.hdnRemarkboxField.Value = ""
        Me.hdnCurrentUnieuqIndex.Value = ""
        'マルチライン入力ボックスの非表示
        Me.divRemarkInputBoxWrapper.Style("display") = "none"
    End Sub

    ''' <summary>
    ''' タブクリックイベント
    ''' </summary>
    ''' <param name="tabObjId">クリックしたタブオブジェクトのID</param>
    Protected Sub TabClick(tabObjId As String)
        Dim isOwner As Boolean = False
        '一旦選択されたタブがリペアの場合はオーナーとする
        If tabObjId = Me.tabRepair.ClientID Then
            isOwner = True
        End If

        Dim beforeTab As String = ""
        Dim intBeforeTab As Integer = Nothing
        Dim selectedTab As String = ""
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)
        tabObjects.Add(COSTITEM.CostItemGroup.Repair, Me.tabRepair)
        tabObjects.Add(COSTITEM.CostItemGroup.FileUp, Me.tabFileUp)
        tabObjects.Add(COSTITEM.CostItemGroup.DoneFileUp, Me.tabDoneFileUp)

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected") Then
                beforeTab = tabObject.Value.ClientID
                intBeforeTab = tabObject.Key
            End If
            tabObject.Value.Attributes.Remove("class")
            If tabObjId = tabObject.Value.ClientID Then
                tabObject.Value.Attributes.Add("class", "selected")
                selectedTab = tabObject.Value.ClientID
            End If

        Next

        visibleControl(isOwner, selectedTab)

        If selectedTab = Me.tabRepair.ID Then

            SetCostGridItem(beforeTab, selectedTab)

            RightboxInit(isOwner, COSTITEM.CostItemGroup.Repair)
            enabledControls(isOwner)

            Dim inFlg As String = "1"
            CalcSummaryCostLocal(inFlg)
            CalcSummaryCostUsd(inFlg)
            CalcSummaryCostAppUsd(inFlg)
            CostEnabledControls()

        ElseIf selectedTab = Me.tabFileUp.ID Then

            FileUppEnabledControls()

        End If

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
        '****************************************
        '右ボックス
        '****************************************
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Remark")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")
        AddLangSetting(dicDisplayText, Me.lblRightInfo1, "ダブルクリックを行い入力を確定してください。", "Double click To confirm input.")
        AddLangSetting(dicDisplayText, Me.lblRightInfo2, "ダブルクリックを行い入力を確定してください。", "Double click To confirm input.")
        '****************************************
        ' 共通情報部分
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblBrInfoHeader, "Repair-Info", "Repair-Info")
        AddLangSetting(dicDisplayText, Me.lblBrNoTitle, "No：", "No：")
        AddLangSetting(dicDisplayText, Me.lblBrRemark, "BR注記", "Note")

        AddLangSetting(dicDisplayText, Me.lblApploveDate, "Date", "Date")
        AddLangSetting(dicDisplayText, Me.lblAgent, "Agent", "Agent")
        AddLangSetting(dicDisplayText, Me.lblPic, "Pic", "Pic")
        AddLangSetting(dicDisplayText, Me.lblAppRemarks, "Remarks", "Remarks")
        AddLangSetting(dicDisplayText, Me.lblApproval, "Apply", "Apply")
        AddLangSetting(dicDisplayText, Me.lblApproved, "Approved", "Approved")

        '****************************************
        ' オーナーのみ情報部分
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblTankNo, "Tank No.", "Tank No.")
        AddLangSetting(dicDisplayText, Me.lblDepoCode, "Depot Code", "Depot Code")
        AddLangSetting(dicDisplayText, Me.lblDepoInDate, "DepoIn Date", "DepoIn Date")
        AddLangSetting(dicDisplayText, Me.lblBreakerNo, "ブレーカー番号", "Breaker No")
        AddLangSetting(dicDisplayText, Me.lblLastProduct, "Last Product", "Last Product")
        AddLangSetting(dicDisplayText, Me.lblTwoAgoProduct, "Two Ago Product", "Two Ago Product")
        AddLangSetting(dicDisplayText, Me.lblLastOrderNo, "Last Order ID", "Last Order ID")
        AddLangSetting(dicDisplayText, Me.lblDeleteFlag, "Delete", "Delete")
        AddLangSetting(dicDisplayText, Me.lblTankUsage, "Tank Failure", "Tank Failure")
        AddLangSetting(dicDisplayText, Me.lblLocation, "Location", "Location")

        AddLangSetting(dicDisplayText, Me.lblDetailInfoHeadedr, "BRdetail-Info", "BRdetail-Info")
        '****************************************
        ' 発・着要素
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblSettlementOffice, "Settlement Office", "Settlement Office")
        AddLangSetting(dicDisplayText, Me.lblLocalRateRef, "Cur/Rate", "Cur/Rate")

        AddLangSetting(dicDisplayText, Me.lblEstimatedSummary, "Total Estimated", "Total Estimated")
        AddLangSetting(dicDisplayText, Me.lblApprovedSummary, "Total Approved", "Total Approved")

        '****************************************
        ' 各種ボタン
        '****************************************
        AddLangSetting(dicDisplayText, Me.btnAddCost, "費用追加", "Add Cost")
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnOutputExcel, "ﾃﾞｰﾀﾀﾞｳﾝﾛｰﾄﾞ", "Data Download")
        AddLangSetting(dicDisplayText, Me.btnOutputFile, "ﾌｧｲﾙﾀﾞｳﾝﾛｰﾄﾞ", "File Download")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.btnApply, "申請", "Apply")
        AddLangSetting(dicDisplayText, Me.btnReject, "否認", "Reject")
        AddLangSetting(dicDisplayText, Me.btnApproval, "承認", "Approval")

        '****************************************
        ' 申請ポップアップ
        '****************************************
        Me.hdnMsgboxFieldName.Value = BEFORE_SAVE_MSG

        '****************************************
        '左ボックス
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblRightListDiscription, "印刷・インポート設定", "Print/Import Settings")

        '****************************************
        ' 隠しフィールド
        '****************************************
        AddLangSetting(dicDisplayText, Me.hdnDispDeleteBtnText, "削除", "Delete")
        AddLangSetting(dicDisplayText, Me.hdnDispLeftBoxCostCode, "コード", "Code")
        AddLangSetting(dicDisplayText, Me.hdnDispLeftBoxItem1, "項目1", "Item1")
        'AddLangSetting(dicDisplayText, Me.hdnDispLeftBoxItem2, "項目2", "Item2")
        AddLangSetting(dicDisplayText, Me.hdnRemarkEmptyMessage, "DoubleClick to input", "DoubleClick to input")
        'ファイルアップロードメッセージ
        AddLangSetting(dicDisplayText, Me.hdnUploadMessage01, "ファイルアップロード開始", "Start uploading files")
        AddLangSetting(dicDisplayText, Me.hdnUploadError01, "ファイルアップロードが失敗しました。", "File upload failed.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError02, "通信を中止しました。", "Communication was canceled.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError03, "タイムアウトエラーが発生しました。", "A timeout error occurred.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError04, "更新権限がありません。", "do not have update permission.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError05, "対応外のファイル形式です。", "It is an incompatible file format.")

        SetDisplayLangObjects(dicDisplayText, lang)

        '****************************************
        ' グリッドヘッダーの表示文言設定(GrivViewだけは個別制御が必要)
        '****************************************
        Dim dicGridDisplayText As New Dictionary(Of Integer, Dictionary(Of String, String))
        dicGridDisplayText.Add(1,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "コード"}, {C_LANG.EN, "Code"}})
        dicGridDisplayText.Add(2,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "名称1"}, {C_LANG.EN, "Item1"}})
        dicGridDisplayText.Add(3,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "所見"}, {C_LANG.EN, "Remark"}})
        dicGridDisplayText.Add(4,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "現地金額"}, {C_LANG.EN, "Local"}})
        dicGridDisplayText.Add(5,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "USD金額"}, {C_LANG.EN, "USD"}})
        dicGridDisplayText.Add(6,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "リペア"}, {C_LANG.EN, "Repair"}})
        dicGridDisplayText.Add(7,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "承認USD金額"}, {C_LANG.EN, "ApprovedUSD"}})

        If gvDetailInfo.Columns.Count > 0 Then
            '最大列数取得
            Dim colMaxIndex As Integer = gvDetailInfo.Columns.Count - 1
            '列のループ
            For i = 0 To colMaxIndex
                Dim fldObj As DataControlField = gvDetailInfo.Columns(i)
                '変換ディクショナリに対象カラム名を置換が設定されている場合文言変更
                If dicGridDisplayText.ContainsKey(i) = True Then
                    fldObj.HeaderText = dicGridDisplayText(i)(lang)
                End If
            Next
        End If
    End Sub
    ''' <summary>
    ''' 遷移元（前画面）の情報を取得
    ''' </summary>
    Private Function GetPrevDisplayInfo(ByRef retDataSet As DataSet) As String

        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim costList As List(Of COSTITEM) = Nothing
        If TypeOf Page.PreviousPage Is GBT00012REPAIR Then
            '自身からの遷移(Save時に反応)
            Dim brNo As String = ""
            Dim prevPage As GBT00012REPAIR = DirectCast(Page.PreviousPage, GBT00012REPAIR)
            brNo = prevPage.lblBrNo.Text
            Me.hdnBrId.Value = brNo
            Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo)
            ViewState(CONST_VS_NAME_DICBRINFO) = dicBrInfo

            'メイン情報取得
            Dim dt As DataTable = GetBreakerBase(dicBrInfo)
            Me.hdnCountryOrg.Value = Convert.ToString(dt.Rows(0)("COUNTRYORGANIZER"))
            'メイン情報格納
            retDataSet.Tables.Add(dt)
            '費用情報取得
            Dim costDt As DataTable = GetBreakerValue(dicBrInfo)
            retDataSet.Tables.Add(costDt)
            '検索条件
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                            {"hdnEndYMD", Me.hdnEndYMD},
                                                                            {"hdnShipper", Me.hdnShipper},
                                                                            {"hdnConsignee", Me.hdnConsignee},
                                                                            {"hdnPort", Me.hdnPort},
                                                                            {"hdnApproval", Me.hdnApproval},
                                                                            {"hdnOffice", Me.hdnOffice},
                                                                            {"hdnMsgId", Me.hdnMsgId},
                                                                            {"hdnApprovalFlg", Me.hdnApprovalFlg},
                                                                            {"hdnAppTranFlg", Me.hdnAppTranFlg},
                                                                            {"hdnApprovalObj", Me.hdnApprovalObj},
                                                                            {"hdnStatus", Me.hdnStatus},
                                                                            {"hdnTankNo", Me.hdnTankNo},
                                                                            {"hdnDepot", Me.hdnDepot},
                                                                            {"hdnPrevViewID", Me.hdnPrevViewID},
                                                                            {"hdnXMLsaveFileRet", Me.hdnXMLsaveFileRet},
                                                                            {"hdnAlreadyFlg", Me.hdnAlreadyFlg},
                                                                            {"hdnDelFlg", Me.hdnDelFlg},
                                                                            {"hdnBrId", Me.hdnBrId},
                                                                            {"hdnSubId", Me.hdnSubId},
                                                                            {"hdnApplyId", Me.hdnApplyId},
                                                                            {"hdnStep", Me.hdnStep},
                                                                            {"hdnLastStep", Me.hdnLastStep},
                                                                            {"hdnHistoryFlg", Me.hdnHistoryFlg},
                                                                            {"hdnLocation", Me.hdnLocation},
                                                                            {"hdnLastCargo", Me.hdnLastCargo},
                                                                            {"hdnGBT00012STankNo", Me.hdnGBT00012STankNo}}


            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            If Me.hdnApprovalFlg.Value = "1" Then '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
                '承認
                If Me.hdnStatus.Value = C_APP_STATUS.APPROVED OrElse Me.hdnStatus.Value = C_APP_STATUS.REJECT Then
                    Me.hdnApprovalFlg.Value = "1" '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
                End If
            Else

                '申請
                If Me.hdnStatus.Value = C_APP_STATUS.APPROVED OrElse Me.hdnStatus.Value = C_APP_STATUS.APPLYING Then
                    Me.hdnApprovalFlg.Value = "1" '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
                End If

            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00011RESULT Then

            '一覧からの遷移
            Dim brNo As String = ""
            Dim prevPage As GBT00011RESULT = DirectCast(Page.PreviousPage, GBT00011RESULT)
            Dim hdnBrIdObj As HiddenField = Nothing
            hdnBrIdObj = DirectCast(prevPage.FindControl("hdnSelectedBrId"), HiddenField)
            brNo = hdnBrIdObj.Value
            Me.hdnBrId.Value = brNo

            Me.hdnApprovalFlg.Value = "0" '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)

            If brNo = "" Then

                '新規
                Dim newDt As DataTable = CreateOrganizerInfoTable()
                Dim hasError As Boolean = False
                Dim newDr As DataRow = newDt.Rows(0)
                newDr.Item("BRTYPE") = ""
                newDr.Item("USETYPE") = ""

                newDr.Item("AGENTORGANIZER") = GBA00003UserSetting.OFFICECODE

                newDr.Item("OFFICENAME") = GBA00003UserSetting.OFFICENAME

                newDr.Item("COUNTRYORGANIZER") = GBA00003UserSetting.COUNTRYCODE

                newDr.Item("TERMTYPE") = "CC" '一旦CY-CY
                'メイン情報取得
                retDataSet.Tables.Add(newDt)
                '費用情報取得
                Dim newCostDt As DataTable = CreateCostData(newDt)

                Dim rate As String = Nothing
                Dim currency As String = Nothing
                Dim countryCode As String = Nothing
                'レート設定
                For i As Integer = 0 To newCostDt.Rows.Count - 1
                    rate = "0"
                    countryCode = GBA00003UserSetting.COUNTRYCODE

                    '為替レート取得
                    Dim exRtDt As DataTable = Nothing
                    Dim GBA00010ExRate As New GBA00010ExRate
                    GBA00010ExRate.COUNTRYCODE = countryCode
                    GBA00010ExRate.TARGETYM = Date.Now.ToString("yyyy/MM")
                    GBA00010ExRate.getExRateInfo()
                    If GBA00010ExRate.ERR = C_MESSAGENO.NORMAL Then
                        exRtDt = GBA00010ExRate.EXRATE_TABLE
                        Dim exRtDr As DataRow = exRtDt.Rows(0)
                        rate = Convert.ToString(exRtDr.Item("EXRATE"))
                        currency = Convert.ToString(exRtDr.Item("CURRENCYCODE"))
                    Else
                        rate = "0"
                        currency = "USD"
                    End If
                    newCostDt.Rows(i).Item("LOCALRATE") = rate
                    newCostDt.Rows(i).Item("CURRENCYCODE") = currency
                    newCostDt.Rows(i).Item("COUNTRYCODE") = countryCode

                Next

                retDataSet.Tables.Add(newCostDt)

                Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
                'ブレーカー紐づけ情報作成
                brInfo = SetBreakerInfo("", newDt)
                ViewState(CONST_VS_NAME_DICBRINFO) = brInfo

                Me.hdnCountryOrg.Value = GBA00003UserSetting.COUNTRYCODE
            Else

                '更新
                Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo)
                ViewState(CONST_VS_NAME_DICBRINFO) = dicBrInfo

                'メイン情報取得
                Dim dt As DataTable = GetBreakerBase(dicBrInfo)
                'メイン情報格納
                retDataSet.Tables.Add(dt)
                '費用情報取得
                Dim costDt As DataTable = GetBreakerValue(dicBrInfo)
                retDataSet.Tables.Add(costDt)
                Me.hdnCountryOrg.Value = Convert.ToString(dt.Rows(0)("COUNTRYORGANIZER"))
            End If

            Dim tmpStYmd As Control = prevPage.FindControl("hdnStYMD")

            If tmpStYmd IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpStYmd, HiddenField)
                Me.hdnStYMD.Value = tmphdn.Value
            End If

            Dim tmpEndYmd As Control = prevPage.FindControl("hdnEndYMD")

            If tmpEndYmd IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpEndYmd, HiddenField)
                Me.hdnEndYMD.Value = tmphdn.Value
            End If

            Dim hdnXMLObj As HiddenField = Nothing
            hdnXMLObj = DirectCast(prevPage.FindControl("hdnXMLsaveFileRet"), HiddenField)
            If hdnXMLObj IsNot Nothing Then
                Me.hdnXMLsaveFileRet.Value = hdnXMLObj.Value
            End If

            Dim tmpStatus As Control = prevPage.FindControl("hdnSelectedStatus")

            If tmpStatus IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpStatus, HiddenField)
                Me.hdnStatus.Value = Convert.ToString(tmphdn.Value).Trim
            End If

            Dim tmpTankNo As Control = prevPage.FindControl("hdnTankNo")

            If tmpTankNo IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpTankNo, HiddenField)
                Me.hdnTankNo.Value = tmphdn.Value
            End If

            Dim tmpDepot As Control = prevPage.FindControl("hdnDepot")

            If tmpDepot IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpDepot, HiddenField)
                Me.hdnDepot.Value = tmphdn.Value
            End If

            If Me.hdnStatus.Value = C_APP_STATUS.APPROVED OrElse Me.hdnStatus.Value = C_APP_STATUS.APPLYING Then
                Me.hdnApprovalFlg.Value = "1" '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
                'Me.hdnDelFlg.Value = CONST_FLAG_YES '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00012APPROVAL Then
            '承認画面からの遷移
            Me.hdnApprovalFlg.Value = "1"　'承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
            Me.hdnAppTranFlg.Value = "1" '承認済画面から来たフラグ(1:承認画面からの遷移)

            Dim brNo As String = ""
            Dim prevPage As GBT00012APPROVAL = DirectCast(Page.PreviousPage, GBT00012APPROVAL)
            Dim hdnBrIdObj As HiddenField = Nothing
            hdnBrIdObj = DirectCast(prevPage.FindControl("hdnSelectedBrId"), HiddenField)
            Me.hdnBrId.Value = hdnBrIdObj.Value
            brNo = hdnBrIdObj.Value
            Me.hdnBrId.Value = brNo

            Dim tmpDelFlg As Control = prevPage.FindControl("hdnSelectedDelFlg")
            If tmpDelFlg IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpDelFlg, HiddenField)
                Me.hdnDelFlg.Value = tmphdn.Value '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
            End If

            Dim tmpSubId As Control = prevPage.FindControl("hdnSubId")
            If tmpSubId IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpSubId, HiddenField)
                Me.hdnSubId.Value = tmphdn.Value
            End If

            Dim tmpLinkId As Control = prevPage.FindControl("hdnLinkId")
            If tmpLinkId IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpLinkId, HiddenField)
                Me.hdnLinkId.Value = tmphdn.Value
            End If

            Dim hdnViewId = DirectCast(prevPage.FindControl("hdnPrevViewID"), HiddenField)
            Me.hdnPrevViewID.Value = hdnViewId.Value

            Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo)
            ViewState(CONST_VS_NAME_DICBRINFO) = dicBrInfo

            'メイン情報取得
            Dim dt As DataTable = GetBreakerBase(dicBrInfo)
            Me.hdnCountryOrg.Value = Convert.ToString(dt.Rows(0)("COUNTRYORGANIZER"))
            'メイン情報格納
            retDataSet.Tables.Add(dt)
            '費用情報取得
            Dim costDt As DataTable = GetBreakerValue(dicBrInfo)
            retDataSet.Tables.Add(costDt)

            Me.hdnStYMD.Value = Date.Now.ToString("yyyy/MM/dd")
            Me.hdnEndYMD.Value = "2099/12/31"

            Dim hdnXMLObj As HiddenField = Nothing
            hdnXMLObj = DirectCast(prevPage.FindControl("hdnXMLsaveFileRet"), HiddenField)
            If hdnXMLObj IsNot Nothing Then
                Me.hdnXMLsaveFileRet.Value = hdnXMLObj.Value
            End If

            Dim tmpStYmd As Control = prevPage.FindControl("hdnStYMD")

            If tmpStYmd IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpStYmd, HiddenField)
                Me.hdnStYMD.Value = tmphdn.Value
            End If

            Dim tmpEndYmd As Control = prevPage.FindControl("hdnEndYMD")

            If tmpEndYmd IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpEndYmd, HiddenField)
                Me.hdnEndYMD.Value = tmphdn.Value
            End If

            Dim tmpApprovalObj As Control = prevPage.FindControl("txtApprovalObj")

            If tmpEndYmd IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpApprovalObj, TextBox)
                Me.hdnApprovalObj.Value = tmphdn.Text
            End If

            Dim tmpStatus As Control = prevPage.FindControl("hdnStatus")

            If tmpStatus IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpStatus, HiddenField)
                Me.hdnStatus.Value = Convert.ToString(tmphdn.Value).Trim
            End If

            Dim tmpStep As Control = prevPage.FindControl("hdnStep")
            If tmpStep IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpStep, HiddenField)
                Me.hdnStep.Value = tmphdn.Value
            End If

            Dim tmpLastStep As Control = prevPage.FindControl("hdnLastStep")
            If tmpLastStep IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpLastStep, HiddenField)
                Me.hdnLastStep.Value = tmphdn.Value
            End If

            Dim tmpApplyId As Control = prevPage.FindControl("hdnApplyId")
            If tmpApplyId IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpApplyId, HiddenField)
                Me.hdnApplyId.Value = tmphdn.Value
            End If

            Dim tmpGBT00012STankNo As Control = prevPage.FindControl("hdnTankNo")
            If tmpGBT00012STankNo IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpGBT00012STankNo, HiddenField)
                Me.hdnGBT00012STankNo.Value = tmphdn.Value
            End If

            Dim tmpLocation As Control = prevPage.FindControl("hdnLocation")
            If tmpLocation IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpLocation, HiddenField)
                Me.hdnLocation.Value = tmphdn.Value
            End If

            Dim tmpLastCargo As Control = prevPage.FindControl("hdnLastCargo")
            If tmpLastCargo IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpLastCargo, HiddenField)
                Me.hdnLastCargo.Value = tmphdn.Value
            End If

            If Me.hdnStatus.Value = C_APP_STATUS.APPROVED Then
                Me.hdnApprovalFlg.Value = "1"　'承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
                'Me.hdnDelFlg.Value = CONST_FLAG_YES '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
                Me.hdnAlreadyFlg.Value = "1" '承認済フラグ(承認画面の遷移時のみ設定 1:承認済)
            ElseIf Me.hdnStatus.Value = C_APP_STATUS.REJECT Then
                Me.hdnApprovalFlg.Value = "1"　'承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
                'Me.hdnDelFlg.Value = CONST_FLAG_YES '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
            End If

        ElseIf TypeOf Page.PreviousPage Is GBM00006TANK Then
            'タンクマスタからの遷移
            Dim prevObj As GBM00006TANK = DirectCast(Page.PreviousPage, GBM00006TANK)
            SetGbt00012items(prevObj.Gbt00012items)
            retDataSet.Tables.Add(prevObj.Gbt00012items.dicCurrentDatatables)
            costList = prevObj.Gbt00012items.lstCurrentCostList
            'Dim brNo As String = ""
            'brNo = Me.hdnBrId.Value

            'If brNo = "" Then

            '    '新規
            '    Dim newDt As DataTable = CreateOrganizerInfoTable()
            '    Dim hasError As Boolean = False
            '    Dim newDr As DataRow = newDt.Rows(0)
            '    newDr.Item("BRTYPE") = ""
            '    newDr.Item("USETYPE") = ""

            '    newDr.Item("AGENTORGANIZER") = GBA00003UserSetting.OFFICECODE

            '    newDr.Item("OFFICENAME") = GBA00003UserSetting.OFFICENAME

            '    newDr.Item("COUNTRYORGANIZER") = GBA00003UserSetting.COUNTRYCODE

            '    newDr.Item("TERMTYPE") = "CC" '一旦CY-CY

            '    Dim tmpTankNo As Control = prevObj.FindControl("hdnSelectedTankNo")
            '    If tmpTankNo IsNot Nothing Then
            '        Dim tmphdn As HiddenField = DirectCast(tmpTankNo, HiddenField)
            '        newDr.Item("TANKNO") = tmphdn.Value
            '    End If

            '    'メイン情報取得
            '    retDataSet.Tables.Add(newDt)
            '    '費用情報取得
            '    Dim newCostDt As DataTable = CreateCostData(newDt)

            '    Dim rate As String = Nothing
            '    Dim currency As String = Nothing
            '    Dim countryCode As String = Nothing
            '    'レート設定
            '    For i As Integer = 0 To newCostDt.Rows.Count - 1
            '        rate = "0"
            '        countryCode = GBA00003UserSetting.COUNTRYCODE

            '        '為替レート取得
            '        Dim exRtDt As DataTable = Nothing
            '        Dim GBA00010ExRate As New GBA00010ExRate
            '        GBA00010ExRate.COUNTRYCODE = countryCode
            '        GBA00010ExRate.TARGETYM = Date.Now.ToString("yyyy/MM")
            '        GBA00010ExRate.getExRateInfo()
            '        If GBA00010ExRate.ERR = C_MESSAGENO.NORMAL Then
            '            exRtDt = GBA00010ExRate.EXRATE_TABLE
            '            Dim exRtDr As DataRow = exRtDt.Rows(0)
            '            rate = Convert.ToString(exRtDr.Item("EXRATE"))
            '            currency = Convert.ToString(exRtDr.Item("CURRENCYCODE"))
            '        Else
            '            rate = "0"
            '            currency = "USD"
            '        End If
            '        newCostDt.Rows(i).Item("LOCALRATE") = rate
            '        newCostDt.Rows(i).Item("CURRENCYCODE") = currency
            '        newCostDt.Rows(i).Item("COUNTRYCODE") = countryCode

            '    Next

            '    retDataSet.Tables.Add(newCostDt)

            '    Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
            '    'ブレーカー紐づけ情報作成
            '    brInfo = SetBreakerInfo("", newDt)
            '    ViewState(CONST_VS_NAME_DICBRINFO) = brInfo

            '    Me.hdnCountryOrg.Value = Convert.ToString(newDr("COUNTRYORGANIZER"))

            'Else

            '    Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo)
            '    ViewState(CONST_VS_NAME_DICBRINFO) = dicBrInfo

            '    'メイン情報取得
            '    Dim dt As DataTable = GetBreakerBase(dicBrInfo)
            '    Me.hdnCountryOrg.Value = Convert.ToString(dt.Rows(0)("COUNTRYORGANIZER"))
            '    Dim tmpTankNo As Control = prevObj.FindControl("hdnSelectedTankNo")
            '    If tmpTankNo IsNot Nothing Then
            '        Dim tmphdn As HiddenField = DirectCast(tmpTankNo, HiddenField)
            '        Me.hdnSelectedTankNo.Value = tmphdn.Value
            '    End If

            '    'メイン情報格納
            '    retDataSet.Tables.Add(dt)
            '    '費用情報取得
            '    Dim costDt As DataTable = GetBreakerValue(dicBrInfo)
            '    retDataSet.Tables.Add(costDt)

            'End If

        ElseIf TypeOf Page.PreviousPage Is GBT00022RESULT Then
            'リペア履歴からの遷移
            Dim brNo As String = ""
            Dim prevPage As GBT00022RESULT = DirectCast(Page.PreviousPage, GBT00022RESULT)
            Dim hdnBrIdObj As HiddenField = Nothing
            hdnBrIdObj = DirectCast(prevPage.FindControl("hdnSelectedBrId"), HiddenField)
            brNo = hdnBrIdObj.Value
            Me.hdnBrId.Value = brNo

            Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo)
            ViewState(CONST_VS_NAME_DICBRINFO) = dicBrInfo
            Me.hdnSubId.Value = dicBrInfo("INFO").SubId
            Me.hdnLinkId.Value = dicBrInfo("INFO").LinkId
            'メイン情報取得
            Dim dt As DataTable = GetBreakerBase(dicBrInfo)

            Me.hdnCountryOrg.Value = Convert.ToString(dt.Rows(0)("COUNTRYORGANIZER"))
            'メイン情報格納
            retDataSet.Tables.Add(dt)
            '費用情報取得
            Dim costDt As DataTable = GetBreakerValue(dicBrInfo)
            retDataSet.Tables.Add(costDt)

            '検索条件
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStApproveDate", Me.hdnStYMD},
                                                                            {"hdnEndApproveDate", Me.hdnEndYMD},
                                                                            {"hdnCountry", Me.hdnCountry},
                                                                            {"hdnTankNo", Me.hdnTankNo},
                                                                            {"hdnReportVariant", Me.hdnReportVariant}}


            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            Me.hdnApprovalFlg.Value = "1"　'承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
            Me.hdnDelFlg.Value = CONST_FLAG_YES '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
            'Me.hdnHistoryFlg.Value = "1" '外す '履歴画面から遷移時のみ(1に設定)

        End If
        '費用一覧を変更可能な一時リスト変数に可能

        If costList Is Nothing Then
            costList = Me.CreateTemporaryCostList(retDataSet.Tables("COST_INFO"), retDataSet.Tables("ORGANIZER_INFO"))
        End If
        'VIEWSTATEにコスト情報を保存
        ViewState(CONST_VS_NAME_COSTLIST) = costList
        '初期情報保持
        If ViewState("INITORGANIZERINFO") Is Nothing Then
            ViewState("INITORGANIZERINFO") = retDataSet.Tables("ORGANIZER_INFO")
            ViewState("INITDICBRINFO") = ViewState(CONST_VS_NAME_DICBRINFO)
            ViewState("INITCOSTLIST") = ViewState(CONST_VS_NAME_COSTLIST)
        End If
        Return retVal
    End Function
    ''' <summary>
    ''' オーナー情報を格納する空のデータテーブルを作成する
    ''' </summary>
    ''' <returns>Organizer情報のデータテーブルを作成</returns>
    ''' <remarks>複数レコードはありえないので１レコード作り返却</remarks>
    Private Function CreateOrganizerInfoTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = "ORGANIZER_INFO"
        retDt.Columns.Add("BRID", GetType(String))
        retDt.Columns.Add("BRBASEID", GetType(String))
        retDt.Columns.Add("STYMD", GetType(String))
        retDt.Columns.Add("USETYPE", GetType(String))
        retDt.Columns.Add("VALIDITYFROM", GetType(String))
        retDt.Columns.Add("VALIDITYTO", GetType(String))
        retDt.Columns.Add("TERMTYPE", GetType(String))
        retDt.Columns.Add("NOOFTANKS", GetType(String))
        retDt.Columns.Add("SHIPPER", GetType(String))
        retDt.Columns.Add("CONSIGNEE", GetType(String))
        retDt.Columns.Add("CARRIER1", GetType(String))
        retDt.Columns.Add("CARRIER2", GetType(String))
        'retDt.Columns.Add("PRODUCTCODE", GetType(String))
        retDt.Columns.Add("IMDGCODE", GetType(String))
        retDt.Columns.Add("UNNO", GetType(String))
        retDt.Columns.Add("RECIEPTCOUNTRY1", GetType(String))
        retDt.Columns.Add("RECIEPTPORT1", GetType(String))
        retDt.Columns.Add("LOADCOUNTRY1", GetType(String))
        retDt.Columns.Add("LOADPORT1", GetType(String))
        retDt.Columns.Add("DISCHARGECOUNTRY1", GetType(String))
        retDt.Columns.Add("DISCHARGEPORT1", GetType(String))
        retDt.Columns.Add("DELIVERYCOUNTRY1", GetType(String))
        retDt.Columns.Add("DELIVERYPORT1", GetType(String))

        retDt.Columns.Add("RECIEPTCOUNTRY2", GetType(String))
        retDt.Columns.Add("RECIEPTPORT2", GetType(String))
        retDt.Columns.Add("LOADCOUNTRY2", GetType(String))
        retDt.Columns.Add("LOADPORT2", GetType(String))
        retDt.Columns.Add("DISCHARGECOUNTRY2", GetType(String))
        retDt.Columns.Add("DISCHARGEPORT2", GetType(String))
        retDt.Columns.Add("DELIVERYCOUNTRY2", GetType(String))
        retDt.Columns.Add("DELIVERYPORT2", GetType(String))

        retDt.Columns.Add("VSL1", GetType(String))
        retDt.Columns.Add("VOY1", GetType(String))
        retDt.Columns.Add("ETD1", GetType(String))
        retDt.Columns.Add("ETA1", GetType(String))

        retDt.Columns.Add("VSL2", GetType(String))
        retDt.Columns.Add("VOY2", GetType(String))
        retDt.Columns.Add("ETD2", GetType(String))
        retDt.Columns.Add("ETA2", GetType(String))
        retDt.Columns.Add("INVOICEDBY", GetType(String))
        retDt.Columns.Add("PRODUCTWEIGHT", GetType(String))
        retDt.Columns.Add("GRAVITY", GetType(String))
        retDt.Columns.Add("LOADING", GetType(String))
        retDt.Columns.Add("STEAMING", GetType(String))
        retDt.Columns.Add("TIP", GetType(String))
        retDt.Columns.Add("EXTRA", GetType(String))
        retDt.Columns.Add("JOTHIREAGE", GetType(String))
        retDt.Columns.Add("COMMERCIALFACTOR", GetType(String))
        retDt.Columns.Add("AMTREQUEST", GetType(String))
        retDt.Columns.Add("AMTPRINCIPAL", GetType(String))
        retDt.Columns.Add("AMTDISCOUNT", GetType(String))
        retDt.Columns.Add("DEMURTO", GetType(String))
        retDt.Columns.Add("DEMURUSRATE1", GetType(String))
        retDt.Columns.Add("DEMURUSRATE2", GetType(String))

        '承認情報
        retDt.Columns.Add("APPLYDATE", GetType(String))
        retDt.Columns.Add("APPLICANTID", GetType(String))
        retDt.Columns.Add("APPLICANTNAME", GetType(String))
        retDt.Columns.Add("APPROVEDATE", GetType(String))
        retDt.Columns.Add("APPROVERID", GetType(String))
        retDt.Columns.Add("APPROVERNAME", GetType(String))

        '念のため
        retDt.Columns.Add("REMARK", GetType(String))
        retDt.Columns.Add("APPLYTEXT", GetType(String))
        retDt.Columns.Add("APPROVEDTEXT", GetType(String))
        retDt.Columns.Add("BRTYPE", GetType(String)) 'ブレーカータイプ
        retDt.Columns.Add("ISTRILATERAL", GetType(String)) '3国間輸送か "1.三国,その他.通常
        retDt.Columns.Add("TANKCAPACITY", GetType(String))
        retDt.Columns.Add("DAYSTOTAL", GetType(String))
        retDt.Columns.Add("PERDAY", GetType(String))
        retDt.Columns.Add("TOTALINVOICED", GetType(String))

        '検討中
        retDt.Columns.Add("DUMMY", GetType(String))
        retDt.Columns.Add("DUMMY2", GetType(String))
        'エージェント関係
        retDt.Columns.Add("AGENTORGANIZER", GetType(String))
        retDt.Columns.Add("AGENTPOL1", GetType(String))
        retDt.Columns.Add("AGENTPOL2", GetType(String))
        retDt.Columns.Add("AGENTPOD1", GetType(String))
        retDt.Columns.Add("AGENTPOD2", GetType(String))

        'リペアブレーカー
        retDt.Columns.Add("BREAKERID", GetType(String))
        retDt.Columns.Add("TANKNO", GetType(String))
        retDt.Columns.Add("DEPOTCODE", GetType(String))
        retDt.Columns.Add("DEPOTNAME", GetType(String))
        retDt.Columns.Add("LOCATION", GetType(String))
        retDt.Columns.Add("REPAIRDEPOINDATE", GetType(String))
        retDt.Columns.Add("LASTPRODUCT", GetType(String))
        retDt.Columns.Add("PRODUCTNAME", GetType(String))
        retDt.Columns.Add("TWOAGOPRODUCT", GetType(String))
        retDt.Columns.Add("TWOAGOPRODUCTNAME", GetType(String))
        retDt.Columns.Add("LASTORDERNO", GetType(String))
        retDt.Columns.Add("DELFLG", GetType(String))
        retDt.Columns.Add("TANKUSAGE", GetType(String))
        'retDt.Columns.Add("SETTLEMENTOFFICE", GetType(String))
        retDt.Columns.Add("OFFICENAME", GetType(String))

        retDt.Columns.Add("SPECIALINS", GetType(String))

        retDt.Columns.Add("COUNTRYORGANIZER", GetType(String))

        retDt.Columns.Add("INITYMD", GetType(String))
        retDt.Columns.Add("INITUSER", GetType(String))

        retDt.Columns.Add("FEE", GetType(String))
        retDt.Columns.Add("BILLINGCATEGORY", GetType(String))
        retDt.Columns.Add("USINGLEASETANK", GetType(String))
        retDt.Columns.Add("REPAIRBRID", GetType(String))

        '初期値設定
        retDt.Columns("NOOFTANKS").DefaultValue = "1"

        retDt.Columns("PRODUCTWEIGHT").DefaultValue = "0"
        retDt.Columns("LOADING").DefaultValue = "0"
        retDt.Columns("STEAMING").DefaultValue = "0"
        retDt.Columns("TIP").DefaultValue = "0"
        retDt.Columns("EXTRA").DefaultValue = "0"
        retDt.Columns("AMTREQUEST").DefaultValue = "0"
        retDt.Columns("AMTPRINCIPAL").DefaultValue = "0"
        retDt.Columns("AMTDISCOUNT").DefaultValue = "0"
        retDt.Columns("DEMURTO").DefaultValue = "0"
        retDt.Columns("DEMURUSRATE1").DefaultValue = "0"
        retDt.Columns("DEMURUSRATE2").DefaultValue = "0"

        retDt.Columns("DELFLG").DefaultValue = CONST_FLAG_NO

        Dim dr As DataRow = retDt.NewRow
        dr.Item("DUMMY") = "　"
        retDt.Rows.Add(dr)
        Return retDt
    End Function
    ''' <summary>
    ''' 費用項目用のデータテーブル作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateCostInfoTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = "COST_INFO"
        retDt.Columns.Add("BRID", GetType(String))
        retDt.Columns.Add("BRVALUEID", GetType(String))
        retDt.Columns.Add("STYMD", GetType(String))
        retDt.Columns.Add("DTLPOLPOD", GetType(String))
        retDt.Columns.Add("DTLOFFICE", GetType(String))
        retDt.Columns.Add("COSTCODE", GetType(String))
        retDt.Columns.Add("COSTNAME", GetType(String))
        retDt.Columns.Add("BASEON", GetType(String))
        retDt.Columns.Add("TAX", GetType(String))
        retDt.Columns.Add("USD", GetType(String))
        retDt.Columns.Add("LOCAL", GetType(String))
        retDt.Columns.Add("CONTRACTOR", GetType(String))
        retDt.Columns.Add("CONTRACTORNAME", GetType(String))
        retDt.Columns.Add("CURRENCYCODE", GetType(String))
        retDt.Columns.Add("LOCALRATE", GetType(String))
        retDt.Columns.Add("USDRATE", GetType(String))
        retDt.Columns.Add("REMARK", GetType(String))
        retDt.Columns.Add("CLASS4", GetType(String))
        retDt.Columns.Add("CLASS8", GetType(String))
        retDt.Columns.Add("CAN_DELETE", GetType(String))
        retDt.Columns.Add("SORT_ORDER", GetType(String))
        retDt.Columns.Add("CLASS2", GetType(String))
        retDt.Columns.Add("AGENT", GetType(String))
        retDt.Columns.Add("INVOICEDBY", GetType(String))

        'リペアブレーカー
        retDt.Columns.Add("ITEM1", GetType(String))
        'retDt.Columns.Add("ITEM2", GetType(String))
        retDt.Columns.Add("REPAIRFLG", GetType(String))
        retDt.Columns.Add("APPROVEDUSD", GetType(String))
        retDt.Columns.Add("COUNTRYCODE", GetType(String))

        '初期値設定
        retDt.Columns("BASEON").DefaultValue = "1"
        retDt.Columns("TAX").DefaultValue = "0"
        retDt.Columns("LOCAL").DefaultValue = "0"
        Return retDt

    End Function
    ''' <summary>
    ''' 初回保持項目
    ''' </summary>
    Private Sub SetInitData(dt As DataTable)
        Dim dr As DataRow = dt.Rows(0)

        Me.hdnInitYmd.Value = Convert.ToString(dr.Item("INITYMD"))
        Me.hdnInitUser.Value = Convert.ToString(dr.Item("INITUSER"))

    End Sub
    ''' <summary>
    ''' オーナー情報をデータテーブルより画面に貼り付け
    ''' </summary>
    ''' <param name="dt"></param>
    Private Sub SetDisplayOrganizerInfo(dt As DataTable, Optional isExcelInport As Boolean = False)
        Dim dr As DataRow = dt.Rows(0)

        Me.lblBrNo.Text = Convert.ToString(dr.Item("BRID"))

        Me.lblApplyRemarks.Text = Convert.ToString(dr.Item("APPLYTEXT")) 'Apply Remarks(ラベルなのでHTMLエンコード)
        Me.lblAppJotRemarks.Text = Convert.ToString(dr.Item("APPROVEDTEXT")) 'Approved Remarks(ラベルなのでHTMLエンコード)

        Me.lblBrRemarkText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("REMARK")))

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        If ViewState(CONST_VS_NAME_DICBRINFO) IsNot Nothing Then
            brInfo = DirectCast(ViewState(CONST_VS_NAME_DICBRINFO), Dictionary(Of String, BreakerInfo))

            Me.lblRemarks.Text = brInfo("INFO").Remark

        End If

        Me.txtTankNo.Text = Convert.ToString(dr.Item("TANKNO"))

        Me.txtDepoCode.Text = Convert.ToString(dr.Item("DEPOTCODE"))

        Me.lblDepoCodeText.Text = Convert.ToString(dr.Item("DEPOTNAME"))

        Me.txtLocation.Text = Convert.ToString(dr.Item("LOCATION"))

        If Convert.ToString(dr.Item("REPAIRDEPOINDATE")) <> "" Then
            Me.txtDepoInDate.Text = Date.Parse(Convert.ToString(dr.Item("REPAIRDEPOINDATE"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtDepoInDate.Text = Convert.ToString(dr.Item("REPAIRDEPOINDATE"))
        End If

        Me.txtBreakerNo.Text = Convert.ToString(dr.Item("REPAIRBRID"))

        Me.txtLastProduct.Text = Convert.ToString(dr.Item("LASTPRODUCT"))

        Me.lblLastProductText.Text = Convert.ToString(dr.Item("PRODUCTNAME"))

        Me.txtTwoAgoProduct.Text = Convert.ToString(dr.Item("TWOAGOPRODUCT"))

        Me.lblTwoAgoProductText.Text = Convert.ToString(dr.Item("TWOAGOPRODUCTNAME"))

        Me.txtLastOrderNo.Text = Convert.ToString(dr.Item("LASTORDERNO"))

        Me.txtDeleteFlag.Text = Convert.ToString(dr.Item("DELFLG"))

        If Me.txtDeleteFlag.Text <> "" Then

            Me.lblDeleteFlagText.Text = ""
            Dim deleteFlagItem As ListItem = Me.lbDelFlg.Items.FindByValue(Me.txtDeleteFlag.Text)
            If deleteFlagItem IsNot Nothing Then

                Me.lblDeleteFlagText.Text = deleteFlagItem.Text
            End If
        End If

        Me.txtTankUsage.Text = Convert.ToString(dr.Item("TANKUSAGE"))

        If Me.txtTankUsage.Text <> "" Then

            Me.lblTankUsageText.Text = ""
            Dim tankUsageItem As ListItem = Me.lbTankUsage.Items.FindByValue(Me.txtTankUsage.Text)
            If tankUsageItem IsNot Nothing Then

                Me.lblTankUsageText.Text = tankUsageItem.Text
            End If
        End If

        Me.txtSettlementOffice.Text = Convert.ToString(dr.Item("AGENTORGANIZER"))

        'オーガナイザー情報保持用
        Me.hdnCountryOrganizer.Value = Convert.ToString(dr.Item("COUNTRYORGANIZER"))

        Dim country As String = ""
        Dim agentDt As DataTable = GetAgent("", Convert.ToString(dr.Item("AGENTORGANIZER")))
        If agentDt IsNot Nothing AndAlso agentDt.Rows.Count > 0 Then
            country = Convert.ToString(agentDt.Rows(0).Item("COUNTRYCODE"))

            '為替レート取得
            Dim rate As String = Nothing
            Dim crncy As String = Nothing
            Dim exRtDt As DataTable = Nothing
            Dim GBA00010ExRate As New GBA00010ExRate
            GBA00010ExRate.COUNTRYCODE = country
            If Me.lblBrNo.Text = "" Then
                GBA00010ExRate.TARGETYM = Date.Now.ToString("yyyy/MM")
            Else
                GBA00010ExRate.TARGETYM = Date.Parse(Convert.ToString(dr.Item("STYMD")).ToString).ToString("yyyy/MM")
            End If
            GBA00010ExRate.getExRateInfo()
                If GBA00010ExRate.ERR = C_MESSAGENO.NORMAL Then
                    exRtDt = GBA00010ExRate.EXRATE_TABLE
                    Dim exRtDr As DataRow = exRtDt.Rows(0)
                    rate = Convert.ToString(exRtDr.Item("EXRATE"))
                    crncy = Convert.ToString(exRtDr.Item("CURRENCYCODE"))
                Else
                    rate = "0"
                    crncy = "USD"
                End If

                '初期値設定
                Me.txtLocalCurrencyRef.Text = Convert.ToString(crncy)
                Me.txtLocalRateRef.Text = NumberFormat(Convert.ToString(rate), country, "", "1")

            End If

            Me.lblSettlementOfficeText.Text = Convert.ToString(dr.Item("OFFICENAME"))

        '承認関係項目
        If Convert.ToString(dr.Item("APPLYDATE")) <> "" Then
            Me.txtAppRequestYmd.Text = Date.Parse(Convert.ToString(dr.Item("APPLYDATE"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtAppRequestYmd.Text = Convert.ToString(dr.Item("APPLYDATE")) 'Apply Date
        End If

        If Me.txtAppRequestYmd.Text <> "" Then
            Me.txtAppOffice.Text = Convert.ToString(dr.Item("AGENTORGANIZER")) 'Apply Office
        Else
            Me.txtAppOffice.Text = ""
        End If
        Me.txtAppSalesPic.Text = Convert.ToString(dr.Item("APPLICANTID")) 'Apply PIC
        Me.lblAppSalesPicText.Text = Convert.ToString(dr.Item("APPLICANTNAME")) 'Apply PIC NAME(ラベルなのでHTMLエンコード)
        Me.lblApplyRemarks.Text = Convert.ToString(dr.Item("APPLYTEXT")) 'Apply Remarks(ラベルなのでHTMLエンコード)

        If Convert.ToString(dr.Item("APPROVEDATE")) <> "" Then
            Me.txtApprovedYmd.Text = Date.Parse(Convert.ToString(dr.Item("APPROVEDATE"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtApprovedYmd.Text = Convert.ToString(dr.Item("APPROVEDATE")) 'Approved Date
        End If

        Me.txtAppJotPic.Text = Convert.ToString(dr.Item("APPROVERID"))   'Approved PIC
        Me.lblAppJotPicText.Text = Convert.ToString(dr.Item("APPROVERNAME")) 'Approved PIC NAME(ラベルなのでHTMLエンコード)
        Me.lblAppJotRemarks.Text = Convert.ToString(dr.Item("APPROVEDTEXT")) 'Approved Remarks(ラベルなのでHTMLエンコード)

        If Convert.ToString(dr.Item("USINGLEASETANK")) = "1" Then

            Me.chkLeaseCheck.Checked = True

        End If

    End Sub

    ''' <summary>
    ''' データテーブルをもとに初期コスト一覧を取得
    ''' </summary>
    ''' <param name="dt">ブレーカー基本情報データテーブル</param>
    ''' <returns>USETYPEをもとにGBM0009_TRPATTERN,GBM0010_CHARGECODE
    ''' より費用の一覧を取得</returns>
    Private Function CreateCostData(dt As DataTable) As DataTable

        Dim sqlStat As New StringBuilder
        Dim displayNameField As String = "NAMES"
        Dim retDt As New DataTable
        sqlStat.AppendLine("SELECT TR.AGENTKBN")
        sqlStat.AppendLine("      , CH.COSTCODE As CODE")
        sqlStat.AppendFormat("     , CH.{0} AS NAMES", displayNameField).AppendLine()
        sqlStat.AppendLine("      , TR.CLASS2 As CLASS2")
        sqlStat.AppendLine("      , CH.CLASS4 As CLASS4")
        sqlStat.AppendLine("      , CH.CLASS8 As CLASS8")
        sqlStat.AppendLine("  FROM GBM0009_TRPATTERN TR")
        sqlStat.AppendLine(" INNER JOIN GBM0010_CHARGECODE CH")
        sqlStat.AppendLine("    ON TR.COMPCODE = CH.COMPCODE")
        sqlStat.AppendLine("   AND TR.COSTCODE = CH.COSTCODE")
        sqlStat.AppendLine("   AND CH.LDKBN    = 'B' ")
        sqlStat.AppendLine("   AND CH.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CH.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND CH.DELFLG   <> @DELFLG")
        sqlStat.AppendLine(" WHERE TR.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("   AND TR.ORG      = @ORG")
        sqlStat.AppendLine("   AND TR.BRTYPE   = @BREAKERTYPE")
        sqlStat.AppendLine("   AND TR.USETYPE  = @USETYPE")
        sqlStat.AppendLine("   AND TR.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND TR.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND TR.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("   AND TR.AGENTKBN <> 'Organizer'") 'オーナーを除く費用
        sqlStat.AppendLine(" ORDER BY TR.AGENTKBN,TR.COSTCODE")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                Dim dr As DataRow = dt.Rows(0)
                Dim breakerTypeDev As String = C_BRTYPE.REPAIR
                Dim useType As String = Convert.ToString(dr.Item("USETYPE"))
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@ORG", SqlDbType.NVarChar, 20).Value = "GB_Default" '輸送パターンマスタのORG項目 一旦GB_Default固定
                .Add("@BREAKERTYPE", SqlDbType.NVarChar, 20).Value = breakerTypeDev
                .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = useType
                .Add("@STYMD", SqlDbType.Date).Value = Date.Today
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Today
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Dim sqlResultDt As New DataTable
            Using sqlDa As New SqlDataAdapter(sqlCmd)

                sqlDa.Fill(sqlResultDt)
            End Using
            retDt = CreateCostInfoTable()
            If sqlResultDt IsNot Nothing AndAlso sqlResultDt.Rows.Count > 0 Then
                retDt = CreateCostInfoTable()
                Dim sortOrder As New Dictionary(Of String, Integer) From {{"POL1", 0}}
                For Each sqlResultDr As DataRow In sqlResultDt.Rows
                    Dim writeDr As DataRow
                    writeDr = retDt.NewRow
                    Dim dtlPolPod As String = Convert.ToString(sqlResultDr.Item("AGENTKBN"))
                    writeDr.Item("DTLPOLPOD") = dtlPolPod
                    writeDr.Item("COSTCODE") = sqlResultDr.Item("CODE")
                    writeDr.Item("ITEM1") = sqlResultDr.Item("NAMES")
                    'writeDr.Item("ITEM2") = ""
                    writeDr.Item("CAN_DELETE") = "0"
                    writeDr.Item("SORT_ORDER") = sortOrder(dtlPolPod)
                    writeDr.Item("CLASS2") = sortOrder(dtlPolPod) + 1
                    writeDr.Item("CLASS4") = sqlResultDr.Item("CLASS4")
                    writeDr.Item("CLASS8") = sqlResultDr.Item("CLASS8")
                    retDt.Rows.Add(writeDr)
                    sortOrder(dtlPolPod) = sortOrder(dtlPolPod) + 1
                Next
            End If
        End Using
        retDt.TableName = "COST_INFO"
        Return retDt

    End Function
    ''' <summary>
    ''' 費用用のリストデータ作成
    ''' </summary>
    ''' <param name="dt">コストデータテーブル</param>
    ''' <returns>タブ切り替え等での途中入力を保持する</returns>
    Function CreateTemporaryCostList(dt As DataTable, orgDt As DataTable) As List(Of COSTITEM)

        'デマレージリスト取得
        Dim demList As List(Of String) = GetDemurrageList()
        Dim retList As New List(Of COSTITEM)

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim sortOrder As New Dictionary(Of COSTITEM.CostItemGroup, Integer) From {{COSTITEM.CostItemGroup.Repair, 0}}
            Dim uniqueIndex As Integer = 0

            For Each costDr As DataRow In dt.Rows

                Dim item As New COSTITEM
                item.SortOrder = sortOrder(item.ItemGroup).ToString
                sortOrder(item.ItemGroup) = sortOrder(item.ItemGroup) + 1
                item.CostCode = Convert.ToString(costDr.Item("COSTCODE"))
                item.USD = Convert.ToString(costDr.Item("USD"))
                item.Local = Convert.ToString(costDr.Item("LOCAL"))

                item.Item1 = Convert.ToString(costDr.Item("ITEM1"))
                item.RepairFlg = Convert.ToString(costDr.Item("REPAIRFLG"))
                item.ApprovedUsd = Convert.ToString(costDr.Item("APPROVEDUSD"))

                item.Remarks = Convert.ToString(costDr.Item("REMARK"))
                item.Class4 = Convert.ToString(costDr.Item("CLASS4"))
                item.Class8 = Convert.ToString(costDr.Item("CLASS8"))
                item.Class2 = Convert.ToString(costDr.Item("CLASS2"))
                item.IsAddedCost = Convert.ToString(costDr.Item("CAN_DELETE"))
                item.SortOrder = Convert.ToString(costDr.Item("SORT_ORDER"))

                item.LocalCurrncyRate = Convert.ToString(costDr.Item("LOCALRATE"))
                item.LocalCurrncy = Convert.ToString(costDr.Item("CURRENCYCODE"))

                item.CountryCode = Convert.ToString(costDr.Item("COUNTRYCODE"))
                item.InvoicedBy = Convert.ToString(costDr.Item("INVOICEDBY"))

                If demList.IndexOf(item.CostCode) <> -1 Then
                    Continue For
                End If

                retList.Add(item)

                item.UniqueIndex = uniqueIndex

                uniqueIndex = uniqueIndex + 1

            Next

        End If
        If retList IsNot Nothing AndAlso retList.Count > 0 Then
            Dim currentCostItemGroup = COSTITEM.CostItemGroup.Repair
            Dim showCostList = (From allCostItem In retList
                                Where allCostItem.ItemGroup = currentCostItemGroup
                                Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
            retList = showCostList
        End If
        Return retList
    End Function

    ''' <summary>
    ''' 選択前の費用一覧の入力値を保持し、選択したタブに一致する費用情報を表示
    ''' </summary>
    ''' <param name="beforeTab">切替前のタブ</param>
    ''' <param name="selectedTab">切替後のタブ</param>
    Private Sub SetCostGridItem(ByVal beforeTab As String, ByVal selectedTab As String)
        'If beforeTab = Me.tabRepair.ClientID Then
        '    Dim beforeCostItemGroup As COSTITEM.CostItemGroup
        '    beforeCostItemGroup = COSTITEM.CostItemGroup.Repair

        '    SaveGridItem(beforeCostItemGroup)
        'End If
        If selectedTab = Me.tabRepair.ID Then
            Dim currentCostItemGroup = COSTITEM.CostItemGroup.Repair
            Dim countryCode As String = Me.hdnCountryOrg.Value
            Dim allCostList = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
            Dim showCostList = (From allCostItem In allCostList
                                Where allCostItem.ItemGroup = currentCostItemGroup
                                Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
            Me.gvDetailInfo.DataSource = showCostList
            Me.gvDetailInfo.DataBind()

        End If

    End Sub
    ''' <summary>
    ''' 費用項目一覧にコードを追加
    ''' </summary>
    ''' <param name="costCode"></param>
    Private Sub AddNewCostItem(costCode As String)
        Dim tabObjects As New List(Of HtmlControl) From {Me.tabRepair}
        Dim costGroup As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Repair

        '費用項目を取得
        Dim dt As DataTable = GetCost(costCode)
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        If allCostList Is Nothing Then
            allCostList = New List(Of COSTITEM)
        End If
        Dim item As New COSTITEM
        item.ItemGroup = costGroup
        item.CostCode = costCode
        item.Item1 = Convert.ToString(dr.Item("NAME"))

        Dim addedCostList = (From allCostItem In allCostList
                             Where allCostItem.ItemGroup = costGroup _
                              And allCostItem.IsAddedCost = "1").ToList
        Dim maxSortNo As Integer = 1
        If addedCostList IsNot Nothing AndAlso addedCostList.Count > 0 Then
            maxSortNo = addedCostList.Count + 1
        End If
        item.USD = "0.00"
        item.Local = NumberFormat(0, Me.hdnCountryOrg.Value)
        item.RepairFlg = "0"
        item.ApprovedUsd = "0.00"
        item.LocalCurrncy = Me.txtLocalCurrencyRef.Text
        item.LocalCurrncyRate = Me.txtLocalRateRef.Text
        item.Remarks = ""
        item.Class4 = Convert.ToString(dr.Item("CLASS4"))
        item.Class8 = Convert.ToString(dr.Item("CLASS8"))
        item.Class2 = Convert.ToString(maxSortNo)
        item.SortOrder = Convert.ToString(maxSortNo)
        item.IsAddedCost = "1"
        item.CountryCode = Me.hdnCountryOrg.Value
        Dim maxUniqueIndex As Integer = 0
        Dim maxOrderdUniqueIndex = (From allCostItem In allCostList
                                    Order By allCostItem.UniqueIndex Descending).ToList
        If maxOrderdUniqueIndex IsNot Nothing AndAlso maxOrderdUniqueIndex.Count > 0 Then
            maxUniqueIndex = maxOrderdUniqueIndex(0).UniqueIndex + 1
        End If
        item.UniqueIndex = maxUniqueIndex
        allCostList.Add(item)
        ViewState(CONST_VS_NAME_COSTLIST) = allCostList

        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.ItemGroup = costGroup
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub
    ''' <summary>
    ''' 削除ボタン押下時イベント
    ''' </summary>
    Public Sub btnListDelete_Click()
        CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMDELETE, pageObject:=Me, submitButtonId:="btnListDeleteOK")
        ViewState("DELUNIQUEINDEX") = Me.hdnDelteCostUniqueIndex.Value
        Me.hdnDelteCostUniqueIndex.Value = ""
    End Sub
    ''' <summary>
    ''' 削除ボタン押下時イベント
    ''' </summary>
    Public Sub btnListDeleteOK_Click()
        Dim uniqueIndex As Integer = 0
        Dim uniqueIndexString As String = Convert.ToString(ViewState("DELUNIQUEINDEX"))
        If Integer.TryParse(uniqueIndexString, uniqueIndex) Then
            DeleteCostItem(uniqueIndex)
        End If
        ViewState("DELUNIQUEINDEX") = ""
    End Sub
    ''' <summary>
    ''' 費用一覧のアイテムを削除
    ''' </summary>
    ''' <param name="uniqueIndex">内部保持しているuniqueインデックス</param>
    Private Sub DeleteCostItem(uniqueIndex As Integer)
        Dim tabObjects As New List(Of HtmlControl) From {Me.tabRepair}
        Dim costGroup As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Repair

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))

        Dim removedCostList = (From allCostItem In allCostList
                               Where allCostItem.UniqueIndex <> uniqueIndex).ToList
        ViewState(CONST_VS_NAME_COSTLIST) = removedCostList
        Dim showCostList = (From allCostItem In removedCostList
                            Where allCostItem.ItemGroup = costGroup
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '合計値計算
        CalcSummaryCostUsd()
        CalcSummaryCostAppUsd()

    End Sub
    ''' <summary>
    ''' 画面上の費目データをVIEWSTATE("COSTLIST")に保存
    ''' </summary>
    ''' <param name="currentTab"></param>
    Private Sub SaveGridItem(currentTab As COSTITEM.CostItemGroup)
        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        Dim retCostList = (From allCostItem In allCostList
                           Where allCostItem.ItemGroup <> currentTab).ToList

        Dim correctDispCostList As New List(Of COSTITEM)
        For Each gridItem As GridViewRow In Me.gvDetailInfo.Rows
            Dim item As New COSTITEM
            item.ItemGroup = currentTab
            item.CostCode = DirectCast(gridItem.FindControl("hdnCostCode"), HiddenField).Value
            item.USD = DirectCast(gridItem.FindControl("txtUsd"), TextBox).Text
            item.Local = DirectCast(gridItem.FindControl("txtLocal"), TextBox).Text
            item.LocalCurrncy = DirectCast(gridItem.FindControl("hdnLocalCurrncy"), HiddenField).Value
            item.LocalCurrncyRate = DirectCast(gridItem.FindControl("hdnLocalRate"), HiddenField).Value
            item.Item1 = DirectCast(gridItem.FindControl("hdnItem1"), HiddenField).Value

            If DirectCast(gridItem.FindControl("chkApp"), CheckBox).Text = "1" Then
                item.RepairFlg = "1"
            Else
                item.RepairFlg = "0"
            End If

            item.ApprovedUsd = DirectCast(gridItem.FindControl("txtApprovedUsd"), TextBox).Text

            item.Remarks = DirectCast(gridItem.FindControl("hdnRemarks"), HiddenField).Value
            item.Class2 = DirectCast(gridItem.FindControl("hdnClass2"), HiddenField).Value
            item.Class4 = DirectCast(gridItem.FindControl("hdnClass4"), HiddenField).Value
            item.Class8 = DirectCast(gridItem.FindControl("hdnClass8"), HiddenField).Value
            item.SortOrder = DirectCast(gridItem.FindControl("hdnSortOrder"), HiddenField).Value
            item.IsAddedCost = DirectCast(gridItem.FindControl("hdnIsAddedCost"), HiddenField).Value
            item.CountryCode = DirectCast(gridItem.FindControl("hdnCountryCode"), HiddenField).Value
            item.InvoicedBy = DirectCast(gridItem.FindControl("hdnInvoicedBy"), HiddenField).Value
            Dim uniqueIndexString = DirectCast(gridItem.FindControl("hdnUniqueIndex"), HiddenField).Value
            Dim uniqueIndex = 0
            Integer.TryParse(uniqueIndexString, uniqueIndex)
            item.UniqueIndex = uniqueIndex

            retCostList.Add(item)
        Next
        ViewState(CONST_VS_NAME_COSTLIST) = retCostList

    End Sub
    ''' <summary>
    ''' 表示非表示制御
    ''' </summary>
    ''' <param name="isOwner"></param>
    ''' <param name="selectedTab"></param>
    Private Sub visibleControl(ByVal isOwner As Boolean, ByVal selectedTab As String)

        '一旦入力項目の表示を非表示にする
        Dim allVisibleControls As New List(Of HtmlControl)
        allVisibleControls.AddRange({Me.divBrDetailInfo,
                                     Me.btnOutputExcel,
                                     Me.hdnInputExcel,
                                     Me.btnOutputFile,
                                     Me.hdnInputFile,
                                     Me.btnSave,
                                     Me.btnApply,
                                     Me.btnApproval,
                                     Me.btnReject,
                                     Me.divFileUpInfo,
                                     Me.divDoneFileUpInfo})
        For Each item In allVisibleControls
            item.Visible = False
        Next
        Dim visibleControls As New List(Of HtmlControl)
        If selectedTab = Me.tabFileUp.ClientID Then

            visibleControls.AddRange({Me.divFileUpInfo, Me.btnOutputFile, Me.hdnInputFile})

        ElseIf selectedTab = Me.tabDoneFileUp.ClientID Then

            visibleControls.AddRange({Me.divDoneFileUpInfo, Me.btnOutputFile, Me.hdnInputFile})

        Else

            If Me.hdnAppTranFlg.Value = "1" Then '承認済画面から来たフラグ(1:承認画面からの遷移)
                visibleControls.AddRange({Me.divBrDetailInfo, Me.btnOutputExcel, Me.btnSave, Me.btnApproval, Me.btnReject})
                ''申請画面からの当画面は申請中のみアップロード可能(申請チェックボックス,コメントのアップロードが可能になったら。現状使用不可)
                'If Me.hdnStatus.Value = C_APP_STATUS.APPLYING Then
                '    visibleControls.Add(Me.hdnInputExcel)
                'End If
            Else
                visibleControls.AddRange({Me.divBrDetailInfo, Me.btnOutputExcel, Me.btnSave, Me.btnApply})
                '承認済みの場合、費目はアップロードさせない
                If Not {C_APP_STATUS.APPLYING, C_APP_STATUS.APPROVED}.Contains(Me.hdnStatus.Value) Then
                    visibleControls.Add(Me.hdnInputExcel)
                End If
            End If

        End If

        '対象のアイテムを表示
        For Each item In visibleControls
            item.Visible = True
        Next

        '参照モードは保存にかかわる機能ボタンすべて消す
        If Me.hdnDelFlg.Value = CONST_FLAG_YES Then
            Me.btnSave.Visible = False
            Me.btnApply.Visible = False
            Me.btnApproval.Visible = False
            Me.btnReject.Visible = False
            Me.hdnInputExcel.Visible = False
            Me.hdnInputFile.Visible = False
        End If

    End Sub
    ''' <summary>
    ''' 使用可否制御
    ''' </summary>
    ''' <param name="isOwner"></param>
    Private Sub enabledControls(isOwner As Boolean)

        Dim controlObjects As New List(Of TextBox) _
                             From {Me.txtAppRequestYmd, Me.txtAppOffice,
                                   Me.txtAppSalesPic, Me.txtApprovedYmd, Me.txtAppJotPic,
                                   Me.txtTankNo, Me.txtDepoCode, Me.txtDepoInDate, Me.txtBreakerNo, Me.txtLocation,
                                   Me.txtLastProduct, Me.txtTwoAgoProduct, Me.txtLastOrderNo, Me.txtDeleteFlag, Me.txtTankUsage}
        For Each controlObj In controlObjects
            controlObj.Enabled = isOwner
        Next

        '入力不可能を続けるオブジェクト(txtAmtPrincipalはJOT否認時に使用可能にする)
        Dim disablecontrolObjects As New List(Of TextBox) _
                             From {Me.txtAppRequestYmd, Me.txtAppOffice,
                                   Me.txtAppSalesPic, Me.txtApprovedYmd, Me.txtAppJotPic,
                                   Me.txtSettlementOffice,
                                   Me.txtLocalCurrencyRef, Me.txtLocalRateRef,
                                   Me.txtTankUsage, Me.txtLocation}
        For Each disablecontrolObj In disablecontrolObjects
            disablecontrolObj.Enabled = False
        Next

        'チェックボックス
        LeaseCheck()

        '以下のコントロールはasp.net上でEnabledを行うと値変更を反映できないためReadOnlyとする
        Dim changeDisableControls As New List(Of TextBox) From {Me.txtTankUsage}
        For Each changeDisableControl In changeDisableControls
            If changeDisableControl.Enabled = False Then
                changeDisableControl.Enabled = True
                changeDisableControl.Attributes.Remove("readonly")
                changeDisableControl.Attributes.Add("readonly", "readonly")
                Dim classAttrs As String = changeDisableControl.CssClass
                Dim resultCss As String = "aspNetDisabled"
                For Each classAttr In classAttrs.Split(" "c)
                    If classAttr = "aspNetDisabled" Then
                        Continue For
                    End If
                    If resultCss = "aspNetDisabled" Then
                        resultCss = resultCss & " " & classAttr
                    End If
                Next
                changeDisableControl.CssClass = resultCss
            Else
                changeDisableControl.Attributes.Remove("readonly")
                Dim classAttrs As String = changeDisableControl.CssClass
                Dim resultCss As String = ""
                For Each classAttr In classAttrs.Split(" "c)
                    If classAttr = "aspNetDisabled" Then
                        Continue For
                    End If
                    If resultCss = "" Then
                        resultCss = resultCss & " " & classAttr
                    End If
                Next
                changeDisableControl.CssClass = resultCss
            End If
        Next
        Me.chkBulkCheck.Enabled = False
        Me.btnApproval.Disabled = True
        Me.btnReject.Disabled = True
        '参照のみ
        If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
            Me.lblBrRemarkText.Enabled = False
            Me.lblRemarks.Enabled = False
            Me.lblApplyRemarks.Enabled = False
            Me.lblAppJotRemarks.Enabled = False

            Me.btnRemarkInputOk.Disabled = True

            Me.txtTankNo.Enabled = False
            Me.chkLeaseCheck.Enabled = False
            Me.txtDeleteFlag.Enabled = False
            Me.btnSave.Disabled = True
            Me.btnApply.Disabled = True
            Me.btnAddCost.Disabled = True

            Me.lblCostRemarkCanEntry.Enabled = False '明細のRemarkもこのチェックに連動
            Me.gvDetailInfo.Enabled = False
            Return '読み取りモードの場合これ以降の制御はさせない
        End If
        '読取専用(添付ファイルすら何もさせない)-- 承認画面の過去履歴ダブルクリック時及びリペアヒストリーから来た際の制御
        If Me.hdnDelFlg.Value = CONST_FLAG_YES Then
            Me.lblBrRemarkText.Enabled = False
            Me.lblRemarks.Enabled = False
            Me.lblApplyRemarks.Enabled = False
            Me.lblAppJotRemarks.Enabled = False

            Me.btnRemarkInputOk.Disabled = True

            Me.txtTankNo.Enabled = False
            Me.chkLeaseCheck.Enabled = False
            Me.txtDeleteFlag.Enabled = False
            Me.btnSave.Disabled = True
            Me.btnApply.Disabled = True
            Me.btnAddCost.Disabled = True

            Me.lblCostRemarkCanEntry.Enabled = False '明細のRemarkもこのチェックに連動
            Return '読み取りモードの場合これ以降の制御はさせない
        End If
        If Me.hdnApprovalFlg.Value = "1" Then '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
            Me.lblBrRemarkText.Enabled = False
            Me.lblRemarks.Enabled = False
            Me.lblApplyRemarks.Enabled = False
            Me.lblAppJotRemarks.Enabled = False

            Me.btnRemarkInputOk.Disabled = True

            Me.txtTankNo.Enabled = False
            Me.chkLeaseCheck.Enabled = False
            Me.txtDeleteFlag.Enabled = False
            Me.btnSave.Disabled = False
            Me.btnApply.Disabled = True
            Me.btnAddCost.Disabled = True

            Me.lblCostRemarkCanEntry.Enabled = False
        End If
        If Me.hdnAppTranFlg.Value = "1" AndAlso
           Me.hdnStatus.Value = C_APP_STATUS.APPLYING Then '承認済画面から来たフラグ(1:承認画面からの遷移)
            '承認画面から遷移かつステータスが申請中の場合のみ
            'NOTE,承認者コメント,リペアチェックを活性化
            Me.btnApproval.Disabled = False
            Me.btnReject.Disabled = False

            Me.btnRemarkInputOk.Disabled = False
            Me.lblBrRemarkText.Enabled = True
            Me.lblAppJotRemarks.Enabled = True
            Me.lblRemarks.Enabled = False
            Me.lblCostRemarkCanEntry.Enabled = True
            Me.chkBulkCheck.Enabled = True
        End If
        '通常入力時の未申請、否認戻り
        If Me.hdnAppTranFlg.Value <> "1" AndAlso
           {"", C_APP_STATUS.REJECT, C_APP_STATUS.APPAGAIN, C_APP_STATUS.EDITING}.Contains(Me.hdnStatus.Value) Then
            Me.lblRemarks.Enabled = True
        End If
        'If Me.hdnAlreadyFlg.Value = "1" Then '承認済フラグ(承認画面の遷移時のみ設定 1:承認済)
        '    'ここでの動作は不要
        'End If

        'If Me.hdnHistoryFlg.Value = "1" Then '履歴画面から遷移時のみ(1に設定) 'hdnHistoryFlgのフラグすらきれいにしたら不要
        '    'ここでの動作は不要
        'End If

        '新規作成時は未セーブ(BRNO未確定)申請は不可
        If Me.lblBrNo.Text = "" Then
            Me.btnApply.Disabled = True
        End If

    End Sub
    ''' <summary>
    ''' 左ボックスのリストデータをクリア
    ''' </summary>
    ''' <remarks>viewstateのデータ量軽減</remarks>
    Private Sub ClearLeftListData()
        Me.lbTankUsage.Items.Clear()
        Me.lbCost.Items.Clear()
        'Me.lbDelFlg.Items.Clear()
        Me.lbDepot.Items.Clear()
        Me.lbProduct.Items.Clear()
        Me.lbTankNo.Items.Clear()
        Me.mvLeft.SetActiveView(Me.vLeftCal)
    End Sub
    ''' <summary>
    ''' 費用一覧取得
    ''' </summary>
    ''' <param name="costCode">費用コード(未指定時は全件)</param>
    ''' <returns></returns>
    Private Function GetCost(Optional costCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim textField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMES"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT COSTCODE AS CODE")
        sqlStat.AppendFormat("     , COSTCODE + ':' + {0} As LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("       , CLASS4 As CLASS4")
        sqlStat.AppendLine("       , CLASS8 As CLASS8")
        sqlStat.AppendLine("  FROM GBM0010_CHARGECODE")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND REPAIRBR    = @REPAIRBR")
        If costCode <> "" Then
            sqlStat.AppendLine("   AND COSTCODE    = @COSTCODE")
        End If
        sqlStat.AppendLine("   AND LDKBN        = 'B'")
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        'sqlStat.AppendLine("ORDER BY COSTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 10).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@REPAIRBR", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@COSTCODE", SqlDbType.NVarChar, 20).Value = costCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' デポ一覧取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDepot() As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim textField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMES"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT DEPOTCODE AS CODE")
        sqlStat.AppendFormat("     , DEPOTCODE + ':' + {0} As LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("  , LOCATION AS LOCATION")
        sqlStat.AppendLine("  FROM GBM0003_DEPOT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        'sqlStat.AppendLine("ORDER BY DEPOTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 10).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 積載品検索
    ''' </summary>
    ''' <param name="productCode">積載品コード（省略時は全件）</param>
    ''' <returns></returns>
    Private Function GetProduct(Optional productCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "PRODUCTNAME"

        'SQL文作成(TODO:ORGもキーだが今のところ未設定)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT rtrim(PRODUCTCODE) AS CODE")
        sqlStat.AppendFormat("      ,rtrim({0}) AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,rtrim(PRODUCTCODE) + ':' + rtrim({0})  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("      ,rtrim(IMDGCODE) AS IMDGCODE")
        sqlStat.AppendLine("      ,rtrim(UNNO) AS UNNO")
        sqlStat.AppendLine("      ,rtrim(GRAVITY) AS GRAVITY")
        sqlStat.AppendLine("      ,rtrim(HAZARDCLASS) AS HAZARDCLASS")
        sqlStat.AppendLine("  FROM GBM0008_PRODUCT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If productCode <> "" Then
            sqlStat.AppendLine("   AND PRODUCTCODE    = @PRODUCTCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND ENABLED      = @ENABLED")
        'sqlStat.AppendLine("ORDER BY PRODUCTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@PRODUCTCODE", SqlDbType.NVarChar, 20).Value = productCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 対象Agent一覧を取得
    ''' </summary>
    ''' <param name="countryCode"></param>
    ''' <param name="carrierCode">業者コード</param>
    ''' <returns></returns>
    ''' <remarks>GBM0005_TRADERより引数国コードをもとにCLASS='AGENT'の一覧を取得</remarks>
    Private Function GetAgent(countryCode As String, Optional carrierCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim textField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMES"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("Select CARRIERCODE As CODE")
        sqlStat.AppendFormat("     , CARRIERCODE + ':' + {0} As LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("       , COUNTRYCODE AS COUNTRYCODE")
        sqlStat.AppendLine("  FROM GBM0005_TRADER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If countryCode <> "" Then
            sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   AND CLASS       = @TRCLASS")
        If carrierCode <> "" Then
            sqlStat.AppendLine("   And CARRIERCODE    = @CARRIERCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 10).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@COUNTRYCODE", SqlDbType.NVarChar, 20).Value = countryCode
                .Add("@TRCLASS", SqlDbType.NVarChar, 20).Value = C_TRADER.CLASS.AGENT
                .Add("@CARRIERCODE", SqlDbType.NVarChar, 20).Value = carrierCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' タンク情報を設定
    ''' </summary>
    ''' <param name="tankNo">左リストで選択したコード</param>
    Private Sub SetDisplayTankNo(tankNo As String)

        Dim tankDt As New DataTable   'データテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT DP.DEPOTCODE   AS DEPOTCODE")
        sqlStat.AppendLine("      ,DP.NAMES       AS DEPOTNAME")
        sqlStat.AppendLine("      ,DP.LOCATION    AS LOCATION")
        sqlStat.AppendLine("      ,CASE OVIN.ACTUALDATE WHEN '1900/01/01' THEN '' ELSE FORMAT(OVIN.ACTUALDATE,'yyyy/MM/dd') END AS REPAIRDEPOINDATE")
        sqlStat.AppendLine("      ,OB.BRID        AS REPAIRBRID")
        sqlStat.AppendLine("      ,OB.PRODUCTCODE AS LASTPRODUCT")
        sqlStat.AppendLine("      ,ISNULL(PD.PRODUCTNAME,'') AS PRODUCTNAME")

        sqlStat.AppendLine("      ,ISNULL(OB2.PRODUCTCODE,'') AS TWOAGOPRODUCT")
        sqlStat.AppendLine("      ,ISNULL(PD2.PRODUCTNAME,'') AS TWOAGOPRODUCTNAME")

        sqlStat.AppendLine("      ,OB.ORDERNO     AS LASTORDERNO")
        sqlStat.AppendLine("      ,ISNULL(trim(TK.REPAIRSTAT),'') AS TANKUSAGE")
        sqlStat.AppendLine("  FROM GBM0006_TANK TK")

        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE OV")
        sqlStat.AppendLine("    ON OV.STYMD    <= @STYMD")
        sqlStat.AppendLine("    AND OV.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("    AND OV.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("    AND OV.TANKNO    = TK.TANKNO")
        sqlStat.AppendLine("    AND OV.ACTIONID  = 'DOUT'")
        sqlStat.AppendLine("    AND OV.ORDERNO   = (SELECT ORDERNO FROM GBT0005_ODR_VALUE OVMX ")
        sqlStat.AppendLine("                        WHERE @TANKNO          = OVMX.TANKNO")
        sqlStat.AppendLine("                          AND OVMX.ACTIONID   IN('DOUT')")
        sqlStat.AppendLine("                          AND OVMX.ACTUALDATE <> '1900/01/01'")
        sqlStat.AppendLine("                          AND OVMX.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("                          AND OVMX.INITYMD     =  ")
        sqlStat.AppendLine("                          (SELECT MAX(INITYMD) FROM GBT0005_ODR_VALUE OVMX2  ")
        sqlStat.AppendLine("                           WHERE @TANKNO           = OVMX2.TANKNO")
        sqlStat.AppendLine("                             AND OVMX2.ACTIONID   IN('DOUT')")
        sqlStat.AppendLine("                             AND OVMX2.ACTUALDATE <> '1900/01/01'")
        sqlStat.AppendLine("                             AND OVMX2.DELFLG     <> @DELFLG))")
        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OB")
        sqlStat.AppendLine("     ON OB.ORDERNO   = OV.ORDERNO")
        sqlStat.AppendLine("    AND OB.STYMD    <= @STYMD")
        sqlStat.AppendLine("    AND OB.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("    AND OB.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE OVIN")
        sqlStat.AppendLine("     ON OVIN.ORDERNO   = OV.ORDERNO")
        sqlStat.AppendLine("    AND OVIN.STYMD    <= @STYMD")
        sqlStat.AppendLine("    AND OVIN.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("    AND OVIN.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("    AND OVIN.ACTIONID IN('ETYD','ETYC')")
        sqlStat.AppendLine("  LEFT JOIN GBM0003_DEPOT DP")
        'sqlStat.AppendLine("     ON DP.DEPOTCODE = OV.CONTRACTORFIX")
        sqlStat.AppendLine("     ON DP.DEPOTCODE = OVIN.CONTRACTORFIX")
        sqlStat.AppendLine("    AND SUBSTRING(DP.ORGCODE,1,2) = @COUNTRY")
        sqlStat.AppendLine("    AND DP.STYMD    <= @STYMD")
        sqlStat.AppendLine("    AND DP.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("    AND DP.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD")
        sqlStat.AppendLine("     ON PD.PRODUCTCODE  = OB.PRODUCTCODE")
        sqlStat.AppendLine("    AND PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("    AND PD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("    AND PD.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE OVMX3")
        sqlStat.AppendLine("     ON OVMX3.INITYMD  = (SELECT MAX(INITYMD) FROM GBT0005_ODR_VALUE OVMX ")
        sqlStat.AppendLine("                          WHERE ")
        sqlStat.AppendLine("                               OVMX.INITYMD <> (SELECT MAX(INITYMD) FROM GBT0005_ODR_VALUE OVMX2")
        sqlStat.AppendLine("                               WHERE @TANKNO = OVMX2.TANKNO")
        sqlStat.AppendLine("                                 AND OVMX2.ACTIONID   IN('DOUT')")
        sqlStat.AppendLine("                                 AND OVMX2.ACTUALDATE <> '1900/01/01'")
        sqlStat.AppendLine("                                 AND OVMX2.DELFLG     <> @DELFLG) ")
        sqlStat.AppendLine("                           AND @TANKNO          = OVMX.TANKNO")
        sqlStat.AppendLine("                           AND OVMX.ACTIONID   IN('DOUT')")
        sqlStat.AppendLine("                           AND OVMX.ACTUALDATE <> '1900/01/01'")
        sqlStat.AppendLine("                           AND OVMX.DELFLG     <> @DELFLG) ")
        sqlStat.AppendLine("    AND OVMX3.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OB2")
        sqlStat.AppendLine("     ON OB2.ORDERNO   = OVMX3.ORDERNO")
        sqlStat.AppendLine("    AND OB2.STYMD    <= @STYMD")
        sqlStat.AppendLine("    AND OB2.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("    AND OB2.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD2")
        sqlStat.AppendLine("     ON PD2.PRODUCTCODE  = OB2.PRODUCTCODE")
        sqlStat.AppendLine("    AND PD2.STYMD       <= @STYMD")
        sqlStat.AppendLine("    AND PD2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("    AND PD2.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  WHERE TK.TANKNO    = @TANKNO")
        sqlStat.AppendLine("    AND TK.STYMD    <= @STYMD")
        sqlStat.AppendLine("    AND TK.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("    AND TK.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("ORDER BY OVIN.ACTUALDATE DESC")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@TANKNO", SqlDbType.NVarChar, 20).Value = tankNo
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@COUNTRY", SqlDbType.NVarChar, 2).Value = Me.hdnCountryOrg.Value
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(tankDt)

                If tankDt IsNot Nothing AndAlso tankDt.Rows.Count > 0 Then
                    Dim tankDr As DataRow = tankDt.Rows(0)
                    Me.txtDepoCode.Text = Convert.ToString(tankDr.Item("DEPOTCODE"))
                    Me.lblDepoCodeText.Text = Convert.ToString(tankDr.Item("DEPOTNAME"))
                    Me.txtLocation.Text = Convert.ToString(tankDr.Item("LOCATION"))
                    If Convert.ToString(tankDr.Item("REPAIRDEPOINDATE")) = "1900/01/01" OrElse Convert.ToString(tankDr.Item("REPAIRDEPOINDATE")) = "" Then
                        Me.txtDepoInDate.Text = ""
                    Else
                        Me.txtDepoInDate.Text = Date.Parse(Convert.ToString(tankDr.Item("REPAIRDEPOINDATE"))).ToString(GBA00003UserSetting.DATEFORMAT)
                    End If

                    Me.txtBreakerNo.Text = Convert.ToString(tankDr.Item("REPAIRBRID"))
                    Me.txtLastProduct.Text = Convert.ToString(tankDr.Item("LASTPRODUCT"))
                    Me.lblLastProductText.Text = Convert.ToString(tankDr.Item("PRODUCTNAME"))
                    Me.txtTwoAgoProduct.Text = Convert.ToString(tankDr.Item("TWOAGOPRODUCT"))
                    Me.lblTwoAgoProductText.Text = Convert.ToString(tankDr.Item("TWOAGOPRODUCTNAME"))
                    Me.txtLastOrderNo.Text = Convert.ToString(tankDr.Item("LASTORDERNO"))
                    Me.txtTankUsage.Text = Convert.ToString(tankDr.Item("TANKUSAGE"))

                Else

                    Me.txtDepoCode.Text = ""
                    Me.lblDepoCodeText.Text = ""
                    Me.txtLocation.Text = ""
                    Me.txtDepoInDate.Text = ""
                    Me.txtBreakerNo.Text = ""
                    Me.txtLastProduct.Text = ""
                    Me.lblLastProductText.Text = ""
                    Me.txtTwoAgoProduct.Text = ""
                    Me.lblTwoAgoProductText.Text = ""
                    Me.txtLastOrderNo.Text = ""
                    Me.txtTankUsage.Text = ""

                End If
                txtTankUsage_Change()

            End Using 'sqlDa
        End Using 'sqlCon,sqlCmd

    End Sub
    ''' <summary>
    ''' 右ボックスのコメント欄制御
    ''' </summary>
    ''' <param name="isOpen"></param>
    Private Sub DisplayCostRemarks(isOpen As Boolean)
        Dim tabObjects As New List(Of HtmlControl) From {Me.tabRepair}
        Dim costGroup As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Repair

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        Dim uniqueIndex As Integer = 0
        Integer.TryParse(Me.hdnCurrentUnieuqIndex.Value, uniqueIndex)
        Dim targetRemarkRow = (From allCostItem In allCostList
                               Where allCostItem.UniqueIndex = uniqueIndex).ToList
        If targetRemarkRow IsNot Nothing AndAlso targetRemarkRow.Count > 0 Then
            If isOpen = True Then
                Me.txtRemarkInput.Text = targetRemarkRow(0).Remarks
            Else
                targetRemarkRow(0).Remarks = Me.txtRemarkInput.Text
            End If
        End If
        ViewState(CONST_VS_NAME_COSTLIST) = allCostList
        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.ItemGroup = costGroup
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()
        FileUppEnabledControls()
    End Sub

    ''' <summary>
    ''' 右の出力帳票
    ''' </summary>
    ''' <param name="isOwner">オーナータブ</param>
    ''' <param name="currentTab">現在アクティブのタブ</param>
    Private Function RightboxInit(isOwner As Boolean, currentTab As COSTITEM.CostItemGroup) As String
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
        COA0016VARIget.VARI = "Default"
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
    ''' <summary>
    ''' 画面入力内容を収集しデータテーブルに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function CollectDisplayOrganizerInfo() As DataTable
        'データテーブルのガワを作成
        Dim retDt As DataTable = CreateOrganizerInfoTable()
        If ViewState(CONST_VS_NAME_INIT_ORGINFO) IsNot Nothing Then
            Dim initDt As DataTable = DirectCast(ViewState(CONST_VS_NAME_INIT_ORGINFO), DataTable)
            retDt.Rows(0).ItemArray = initDt.Rows(0).ItemArray
        End If
        Dim dr As DataRow = retDt.Rows(0)
        dr.Item("BRID") = Me.lblBrNo.Text

        dr.Item("REMARK") = HttpUtility.HtmlDecode(Me.lblBrRemarkText.Text)
        dr.Item("APPLYTEXT") = HttpUtility.HtmlDecode(Me.lblApplyRemarks.Text)
        dr.Item("APPROVEDTEXT") = HttpUtility.HtmlDecode(Me.lblAppJotRemarks.Text)

        Dim applyDate As Date = Nothing
        If Date.TryParseExact(Me.txtAppRequestYmd.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, applyDate) Then
            dr.Item("APPLYDATE") = applyDate
        Else
            dr.Item("APPLYDATE") = Me.txtAppRequestYmd.Text
        End If
        dr.Item("APPLICANTID") = Me.txtAppSalesPic.Text
        dr.Item("APPLICANTNAME") = HttpUtility.HtmlDecode(Me.lblAppSalesPicText.Text)

        Dim approveDate As Date = Nothing
        If Date.TryParseExact(Me.txtApprovedYmd.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, approveDate) Then
            dr.Item("APPROVEDATE") = approveDate
        Else
            dr.Item("APPROVEDATE") = Me.txtApprovedYmd.Text
        End If
        dr.Item("APPROVERID") = Me.txtAppJotPic.Text
        dr.Item("APPROVERNAME") = HttpUtility.HtmlDecode(Me.lblAppJotPicText.Text)

        'リペア
        dr.Item("TANKNO") = Me.txtTankNo.Text
        dr.Item("DEPOTCODE") = Me.txtDepoCode.Text
        dr.Item("DEPOTNAME") = Me.lblDepoCodeText.Text
        dr.Item("LOCATION") = Me.txtLocation.Text

        Dim depoinDate As Date = Nothing
        If Date.TryParseExact(Me.txtDepoInDate.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, depoinDate) Then
            dr.Item("REPAIRDEPOINDATE") = depoinDate
        Else
            dr.Item("REPAIRDEPOINDATE") = Me.txtDepoInDate.Text
        End If

        dr.Item("REPAIRBRID") = Me.txtBreakerNo.Text
        dr.Item("LASTPRODUCT") = Me.txtLastProduct.Text
        dr.Item("PRODUCTNAME") = Me.lblLastProductText.Text
        dr.Item("TWOAGOPRODUCT") = Me.txtTwoAgoProduct.Text
        dr.Item("TWOAGOPRODUCTNAME") = Me.lblTwoAgoProductText.Text
        dr.Item("LASTORDERNO") = Me.txtLastOrderNo.Text
        dr.Item("DELFLG") = Me.txtDeleteFlag.Text
        dr.Item("TANKUSAGE") = Me.txtTankUsage.Text
        dr.Item("AGENTORGANIZER") = Me.txtSettlementOffice.Text
        dr.Item("OFFICENAME") = Me.lblSettlementOfficeText.Text

        dr.Item("SPECIALINS") = HttpUtility.HtmlDecode(Me.lblRemarks.Text)

        'オーガナイザ国
        dr.Item("COUNTRYORGANIZER") = Me.hdnCountryOrganizer.Value

        If Me.chkLeaseCheck.Checked Then
            dr.Item("USINGLEASETANK") = "1"
        Else
            dr.Item("USINGLEASETANK") = "0"
        End If

        Return retDt
    End Function
    ''' <summary>
    ''' 入力チェック、データ登録、Excel出力時に使用するため画面情報をデータテーブルに格納
    ''' </summary>
    ''' <param name="currentTab"></param>
    ''' <returns></returns>
    Private Function CollectDisplayCostInfo(Optional currentTab As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Repair) As DataTable
        Dim retDt As DataTable = CreateCostInfoTable()

        Dim targetCostData As List(Of COSTITEM) = Nothing

        Dim costData As List(Of COSTITEM) = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        If currentTab <> COSTITEM.CostItemGroup.Repair Then
            targetCostData = costData 'TODOステータスに応じ動きを変える
        Else
            targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab).ToList
        End If

        For Each costItem In targetCostData
            Dim dtlPolPod As String = ""
            Dim agent As String = ""
            Dim contractor As String = ""
            dtlPolPod = "POL1"
            contractor = txtDepoCode.Text
            agent = txtSettlementOffice.Text

            Dim dr As DataRow = retDt.NewRow
            dr.Item("DTLPOLPOD") = dtlPolPod
            dr.Item("COSTCODE") = costItem.CostCode
            dr.Item("USD") = costItem.USD
            dr.Item("LOCAL") = costItem.Local
            dr.Item("CURRENCYCODE") = costItem.LocalCurrncy
            dr.Item("LOCALRATE") = costItem.LocalCurrncyRate
            dr.Item("ITEM1") = costItem.Item1
            dr.Item("REPAIRFLG") = costItem.RepairFlg
            dr.Item("APPROVEDUSD") = costItem.ApprovedUsd
            dr.Item("REMARK") = costItem.Remarks
            dr.Item("CLASS2") = costItem.Class2
            dr.Item("CLASS4") = costItem.Class4
            dr.Item("CLASS8") = costItem.Class8
            dr.Item("AGENT") = agent
            dr.Item("COUNTRYCODE") = costItem.CountryCode
            dr.Item("CONTRACTOR") = contractor
            dr.Item("INVOICEDBY") = agent
            retDt.Rows.Add(dr)
        Next
        Return retDt
    End Function
    ''' <summary>
    ''' 入力チェック関数
    ''' </summary>
    ''' <param name="ds">チェック対象のデータセット（オーナ情報、費用情報）</param>
    ''' <param name="isMinCheck">一時保存するための最低限チェックのみか(True:最低限チェックのみ,False:完全チェック)</param>
    ''' <param name="isCheckAllTabs">全タブのチェックを行うか(True:全タブ,False:選択しているタブのみ)</param>
    ''' <returns>True:正常,False:異常</returns>
    Private Function CheckInput(ds As DataSet, isMinCheck As Boolean, Optional isCheckAllTabs As Boolean = False) As Boolean

        Dim ownerDt As DataTable = ds.Tables("ORGANIZER_INFO")
        Dim costDt As DataTable = ds.Tables("COST_INFO")
        Dim rightBoxMessage As New Text.StringBuilder
        Dim message As New StringBuilder
        Dim dummyLabelObj As New Label
        Dim errMessage As String = ""
        Dim mapId As String = ""
        Dim hasError As Boolean = False
        If isMinCheck Then
            mapId = "GBT00012REPAIR"
        Else
            mapId = "GBT00012REPAIR"
        End If

        Dim fieldList As New List(Of String) From {"TANKNO", "DELFLG", "TANKUSAGE", "DEPOTCODE", "LOCATION", "REPAIRDEPOINDATE", "REPAIRBRID", "LASTORDERNO", "LASTPRODUCT", "TWOAGOPRODUCT"}
        If CheckSingle(mapId, ownerDt, fieldList, errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If

        'fieldList = New List(Of String) From {"COSTCODE", "ITEM1", "ITEM2", "USD", "LOCAL", "APPROVEDUSD", "REMARK"}
        fieldList = New List(Of String) From {"COSTCODE", "ITEM1", "USD", "LOCAL", "APPROVEDUSD", "REMARK"}
        Dim keyFields As New List(Of String) From {"COSTCODE"}
        If CheckSingle(mapId, costDt, fieldList, errMessage, keyFields) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If

        SetTankNoListItem(Me.chkLeaseCheck.Checked)
        If ChedckList(txtTankNo.Text, lbTankNo, "Tank No.", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If

        Dim dt As DataTable = GetDepot()
        With Me.lbDepot
            .DataSource = dt
            .DataTextField = "LISTBOXNAME"
            .DataValueField = "CODE"
            .DataBind()
            .Focus()
        End With
        If ChedckList(txtDepoCode.Text, lbDepot, "Depot Code", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If

        SetDelFlgListItem()
        If ChedckList(txtDeleteFlag.Text, lbDelFlg, "Delete", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If

        '一時保存のチェックの場合はここで終了
        If isMinCheck = True Then
            If hasError Then
                'フッターに左ボックスを見るようメッセージを設定
                Dim messageNo As String = C_MESSAGENO.RIGHTBIXOUT
                CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
                '左ボックスにエラーメッセージ表示
                Me.txtRightErrorMessage.Text = rightBoxMessage.ToString
            End If

        End If
        Return Not hasError
    End Function
    ''' <summary>
    ''' 単項目チェック処理
    ''' </summary>
    ''' <param name="mapId">IN:チェック条件のMAPID</param>
    ''' <param name="dt">IN:チェック対象のデータテーブル</param>
    ''' <param name="checkFileds">IN:チェックフィールド一覧</param>
    ''' <param name="errMessage">OUT：エラーメッセージ</param>
    ''' <param name="keyFields">IN(省略可):エラーメッセージ表示時に示すキーフィールドリスト、ここを指定した場合は「エラー内容」＋「当引数のフィールドと値」をメッセージに付与します
    ''' 省略時は付与しません</param>
    ''' <param name="keyValuePadLen">IN(省略可 省略時20):「--> [項目名] = [値]」を表示する際の項目名から=までにスペースを埋めるバイト数</param>
    ''' <returns>メッセージ番号:すべて正常時はC_MESSAGENO.NORMAL(00000) チェック異常時はC_MESSAGENO.RIGHTBIXOUT(10008)を返却</returns>
    Private Function CheckSingle(ByVal mapId As String, ByVal dt As DataTable, ByVal checkFileds As List(Of String), ByRef errMessage As String, Optional keyFields As List(Of String) = Nothing, Optional keyValuePadLen As Integer = 20) As String

        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim hasError As Boolean = False
        Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
        Dim retMessage As New StringBuilder
        'エラーメッセージ取得すら失敗した場合
        Dim getMessageErrorString As String = "エラーメッセージ({0})の取得に失敗しました。"
        If BASEDLL.COA0019Session.LANGDISP <> "JA" Then
            getMessageErrorString = "Failed To Get Error message ({0})."
        End If
        '******************************
        '引数チェック
        '******************************
        '検査対象のデータテーブルレコードが存在しない、チェックフィールドが存在しない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 OrElse checkFileds.Count = 0 Then
            Return retMessageNo
        End If
        'DataTableに本当に存在するフィールを取得
        Dim targetCheckFields As New List(Of String)
        For Each checkField As String In checkFileds
            If dt.Columns.Contains(checkField) Then
                targetCheckFields.Add(checkField)
            End If
        Next
        '検査すべきフィールドがない場合はそのまま終了
        If targetCheckFields.Count = 0 Then
            Return retMessageNo
        End If
        'DataTableに本当に存在するキーフィールドを取得
        Dim targetKeyFields As List(Of String) = Nothing
        If keyFields IsNot Nothing Then
            targetKeyFields = New List(Of String)
            For Each keyField As String In keyFields
                If dt.Columns.Contains(keyField) Then
                    targetKeyFields.Add(keyField)
                End If
            Next
            If targetKeyFields.Count = 0 Then
                targetKeyFields = Nothing
            End If
        End If

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck              '項目チェック

        'チェックごとに変わらないパラメータ設定
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = mapId

        '******************************
        'フィールド名ディクショナリ取得
        '******************************
        Dim fieldDic As New Dictionary(Of String, String)
        COA0026FieldCheck.FIELDDIC = fieldDic
        COA0026FieldCheck.COA0026getFieldList()
        fieldDic = COA0026FieldCheck.FIELDDIC
        '******************************
        '単項目チェック開始
        '******************************

        'データテーブルの行ループ開始
        For Each dr As DataRow In dt.Rows
            'チェックフィールドのループ開始

            For Each checkField In targetCheckFields
                COA0026FieldCheck.FIELD = checkField
                COA0026FieldCheck.VALUE = Convert.ToString(dr.Item(checkField))
                COA0026FieldCheck.COA0026FieldCheck()
                If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then
                    retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                    CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, dummyLabelObj)
                    retMessage.AppendFormat("・{0}：{1}", fieldDic(checkField), dummyLabelObj.Text).AppendLine()

                    If targetKeyFields IsNot Nothing Then
                        For Each keyField In targetKeyFields
                            retMessage.AppendFormat("--> {0} = {1}", padRight(fieldDic(keyField), keyValuePadLen), Convert.ToString(dr.Item(keyField))).AppendLine()
                        Next
                    End If 'END targetKeyFields IsNot Nothing 

                End If 'END  COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL
            Next

            Dim stringKeyInfo As String = ""


        Next 'END For Each dr As DataRow In dt.Rows
        errMessage = retMessage.ToString
        Return retMessageNo
    End Function
    ''' <summary>
    ''' 文字左スペース埋め
    ''' </summary>
    ''' <param name="st"></param>
    ''' <param name="len"></param>
    ''' <returns></returns>
    ''' <remarks>エラー一覧で項目名称が日本語英語まちまちなので調整</remarks>
    Function padRight(ByVal st As String, ByVal len As Integer) As String
        Dim padLength As Integer = len - (System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(st) - st.Length)
        '埋められない場合はそのまま返却
        If padLength <= 0 Then
            Return st
        End If
        Return st.PadRight(len, " "c)
    End Function
    ''' <summary>
    ''' 禁則文字置換
    ''' </summary>
    ''' <param name="targetObjects">対象オブジェクト（テキストボックスリスト or データテーブル)</param>
    ''' <param name="columnList">置換対象カラム一覧(データテーブル時のみ指定)</param>
    Private Sub ChangeInvalidChar(targetObjects As Object, Optional columnList As List(Of String) = Nothing)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get
        'テキストボックスの全置換
        If TypeOf targetObjects Is List(Of TextBox) Then
            Dim targetTextboxList As List(Of TextBox) = DirectCast(targetObjects, List(Of TextBox))
            For Each targetTextbox In targetTextboxList
                With COA0008InvalidChar
                    .CHARin = targetTextbox.Text
                    .COA0008RemoveInvalidChar()
                    If .CHARin <> .CHARout Then
                        targetTextbox.Text = .CHARout
                    End If
                End With
            Next
        End If
        'データテーブルの格納値置換
        If TypeOf targetObjects Is DataTable Then
            If columnList Is Nothing OrElse columnList.Count = 0 Then
                '引数置換対象のカラムがない場合はそのまま終了
                Return
            End If
            Dim dt As DataTable = DirectCast(targetObjects, DataTable)
            'データテーブルがないまたはレコードがない場合はそのまま終了
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                Return
            End If
            '引数カラムリストのうち引数データテーブルに存在するカラムに限定
            Dim changeValueColumnList As New List(Of String)
            For Each columnName As String In columnList
                If dt.Columns.Contains(columnName) Then
                    changeValueColumnList.Add(columnName)
                End If
            Next
            'データテーブルとのカラム名マッチングの結果,
            '置換対象のカラムが存在しない場合はそのまま終了
            If changeValueColumnList.Count = 0 Then
                Return
            End If
            'データ行のループ
            For Each dr As DataRow In dt.Rows
                'カラム名のループ
                For Each columnName As String In changeValueColumnList
                    With COA0008InvalidChar
                        .CHARin = Convert.ToString(dr.Item(columnName))
                        .COA0008RemoveInvalidChar()
                        If .CHARin <> .CHARout Then
                            dr.Item(columnName) = .CHARout
                        End If
                    End With
                Next 'カラム名のループEND
            Next 'データ行のループEND

        End If
    End Sub
    ''' <summary>
    ''' データ登録処理
    ''' </summary>
    ''' <param name="ds"></param>
    ''' <param name="currentTab"></param>
    ''' <param name="isEntryAllTabs"></param>
    Private Sub EntryData(ds As DataSet, currentTab As COSTITEM.CostItemGroup, ByRef errFlg As Boolean, Optional isEntryAllTabs As Boolean = False, Optional isSendConfirm As Boolean = False)
        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        Dim newEntry As Boolean = True
        Dim breakerId As String = ""
        Dim ownerDt As DataTable = ds.Tables("ORGANIZER_INFO")
        Dim costDt As DataTable = ds.Tables("COST_INFO")

        brInfo = DirectCast(ViewState(CONST_VS_NAME_DICBRINFO), Dictionary(Of String, BreakerInfo))
        If brInfo("INFO").BrId <> "" Then
            newEntry = False
        End If

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            '新規ブレーカーの場合ブレーカー番号を取得し紐づけデータの作成
            If newEntry Then
                '新規ブレーカーNo作成
                breakerId = GetNewBreakerNo(sqlCon)
                'ブレーカー紐づけ情報作成
                brInfo = SetBreakerInfo(breakerId, ownerDt)
                'brInfo("INFO").BrId = breakerId
                'DB登録処理実行
                EntryNewBreaker(brInfo, ownerDt, costDt, sqlCon, errFlg)
            Else

                '更新可能チェック(タイムスタンプ比較）
                If CanBreakerUpdate(brInfo, sqlCon) = False Then
                    Dim msgNo As String = C_MESSAGENO.CANNOTUPDATE
                    errFlg = False
                    CommonFunctions.ShowMessage(msgNo, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                                                messageParams:=New List(Of String) From {String.Format("CODE:{0}", msgNo)})
                    Return
                End If
                '更新処理実行
                UpdateBreaker(brInfo, ownerDt, costDt, sqlCon)
            End If

        End Using

        Dim apSt = GetStatus(Me.hdnBrId.Value)

        If Me.hdnStatus.Value = C_APP_STATUS.APPROVED AndAlso apSt = C_APP_STATUS.APPROVED Then
            isBeforeApploveFlg = False
        Else
            isBeforeApploveFlg = True
        End If

        Me.hdnBrId.Value = brInfo("INFO").BrId

        'File更新処理
        '添付ファイルを正式フォルダに転送
        CommonFunctions.SaveAttachmentFilesList(AfterRepairAttachment, brInfo("INFO").BrId, CONST_DIRNAME_REPAIR, isBeforeApploveFlg, After)
        'DoneFile更新処理
        CommonFunctions.SaveAttachmentFilesList(BeforeRepairAttachment, brInfo("INFO").BrId, CONST_DIRNAME_REPAIR, isBeforeApploveFlg, Before)

    End Sub
    ''' <summary>
    ''' 新規ブレーカー登録
    ''' </summary>
    ''' <remarks>無条件に全項目インサートする</remarks>
    Private Sub EntryNewBreaker(brInfoDt As Dictionary(Of String, BreakerInfo), ownerDt As DataTable, costDt As DataTable, Optional sqlCon As SqlConnection = Nothing, Optional isSendConfirm As Boolean = False)
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Dim tran As SqlTransaction = Nothing
        Dim procDateTime As DateTime = DateTime.Now
        Dim brId As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            brId = brInfoDt.Values(0).BrId
            tran = sqlCon.BeginTransaction() 'トランザクション開始

            '******************************
            ' 紐づけ情報インサート
            '******************************
            sqlStat.AppendLine("INSERT INTO GBT0001_BR_INFO (")
            sqlStat.AppendLine("            BRID ")
            sqlStat.AppendLine("           ,SUBID ")
            sqlStat.AppendLine("           ,TYPE ")
            sqlStat.AppendLine("           ,LINKID ")
            sqlStat.AppendLine("           ,STYMD ")
            sqlStat.AppendLine("           ,BRTYPE ")
            sqlStat.AppendLine("           ,APPLYID ")
            sqlStat.AppendLine("           ,LASTSTEP ")
            sqlStat.AppendLine("           ,USETYPE ")
            sqlStat.AppendLine("           ,REMARK ")
            sqlStat.AppendLine("           ,DELFLG ")
            sqlStat.AppendLine("           ,INITYMD ")
            sqlStat.AppendLine("           ,UPDYMD ")
            sqlStat.AppendLine("           ,UPDUSER ")
            sqlStat.AppendLine("           ,UPDTERMID ")
            sqlStat.AppendLine("           ,RECEIVEYMD ")
            sqlStat.AppendLine("   ) VALUES ( ")
            sqlStat.AppendLine("            @BRID ")
            sqlStat.AppendLine("           ,@SUBID ")
            sqlStat.AppendLine("           ,@TYPE ")
            sqlStat.AppendLine("           ,@LINKID ")
            sqlStat.AppendLine("           ,@STYMD ")
            sqlStat.AppendLine("           ,@BRTYPE ")
            sqlStat.AppendLine("           ,@APPLYID ")
            sqlStat.AppendLine("           ,@LASTSTEP ")
            sqlStat.AppendLine("           ,@USETYPE ")
            sqlStat.AppendLine("           ,@REMARK ")
            sqlStat.AppendLine("           ,@DELFLG ")
            sqlStat.AppendLine("           ,@INITYMD ")
            sqlStat.AppendLine("           ,@UPDYMD ")
            sqlStat.AppendLine("           ,@UPDUSER ")
            sqlStat.AppendLine("           ,@UPDTERMID ")
            sqlStat.AppendLine("           ,@RECEIVEYMD ")
            sqlStat.AppendLine(") ")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    '当インサート時の各行で変化がないパラメータ設定
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@SUBID", SqlDbType.NVarChar, 20).Value = brInfoDt.Values(0).SubId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@BRTYPE", SqlDbType.NVarChar, 20).Value = brInfoDt.Values(0).BrType
                    .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = brInfoDt.Values(0).ApplyId
                    .Add("@LASTSTEP", SqlDbType.NVarChar, 20).Value = brInfoDt.Values(0).LastStep
                    .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = brInfoDt.Values(0).UseType
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                '動的パラメータ
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)
                Dim paramType As SqlParameter = sqlCmd.Parameters.Add("@TYPE", SqlDbType.NVarChar, 20)
                Dim paramLinkID As SqlParameter = sqlCmd.Parameters.Add("@LINKID", SqlDbType.NVarChar, 20)
                'InfoデータをループしINFOテーブルの登録
                For Each brInfoItem As BreakerInfo In brInfoDt.Values
                    If brInfoItem.Type = "POD1" OrElse brInfoItem.Type = "POD2" Then
                        Continue For
                    End If

                    '各行のパラメータ設定
                    paramRemark.Value = brInfoItem.Remark
                    paramType.Value = brInfoItem.Type
                    paramLinkID.Value = brInfoItem.LinkId
                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                Next
            End Using

            '******************************
            ' organizer情報（ブレーカー基本）インサート
            '******************************
            sqlStat.Clear()
            sqlStat.AppendLine("INSERT INTO GBT0002_BR_BASE (")
            sqlStat.AppendLine("              BRID")
            sqlStat.AppendLine("             ,BRBASEID")
            sqlStat.AppendLine("             ,STYMD")
            sqlStat.AppendLine("             ,VALIDITYFROM")
            sqlStat.AppendLine("             ,VALIDITYTO")
            sqlStat.AppendLine("             ,TERMTYPE")
            sqlStat.AppendLine("             ,NOOFTANKS")
            sqlStat.AppendLine("             ,SHIPPER")
            sqlStat.AppendLine("             ,CONSIGNEE")
            sqlStat.AppendLine("             ,CARRIER1")
            sqlStat.AppendLine("             ,CARRIER2")
            sqlStat.AppendLine("             ,PRODUCTCODE")
            sqlStat.AppendLine("             ,PRODUCTWEIGHT")
            sqlStat.AppendLine("             ,RECIEPTCOUNTRY1")
            sqlStat.AppendLine("             ,RECIEPTPORT1")
            sqlStat.AppendLine("             ,RECIEPTCOUNTRY2")
            sqlStat.AppendLine("             ,RECIEPTPORT2")
            sqlStat.AppendLine("             ,LOADCOUNTRY1")
            sqlStat.AppendLine("             ,LOADPORT1")
            sqlStat.AppendLine("             ,LOADCOUNTRY2")
            sqlStat.AppendLine("             ,LOADPORT2")
            sqlStat.AppendLine("             ,DISCHARGECOUNTRY1")
            sqlStat.AppendLine("             ,DISCHARGEPORT1")
            sqlStat.AppendLine("             ,DISCHARGECOUNTRY2")
            sqlStat.AppendLine("             ,DISCHARGEPORT2")
            sqlStat.AppendLine("             ,DELIVERYCOUNTRY1")
            sqlStat.AppendLine("             ,DELIVERYPORT1")
            sqlStat.AppendLine("             ,DELIVERYCOUNTRY2")
            sqlStat.AppendLine("             ,DELIVERYPORT2")
            sqlStat.AppendLine("             ,VSL1")
            sqlStat.AppendLine("             ,VOY1")
            sqlStat.AppendLine("             ,ETD1")
            sqlStat.AppendLine("             ,ETA1")
            sqlStat.AppendLine("             ,VSL2")
            sqlStat.AppendLine("             ,VOY2")
            sqlStat.AppendLine("             ,ETD2")
            sqlStat.AppendLine("             ,ETA2")
            sqlStat.AppendLine("             ,INVOICEDBY")
            sqlStat.AppendLine("             ,LOADING")
            sqlStat.AppendLine("             ,STEAMING")
            sqlStat.AppendLine("             ,TIP")
            sqlStat.AppendLine("             ,EXTRA")
            sqlStat.AppendLine("             ,JOTHIREAGE")
            sqlStat.AppendLine("             ,COMMERCIALFACTOR")
            sqlStat.AppendLine("             ,AMTREQUEST")
            sqlStat.AppendLine("             ,AMTPRINCIPAL")
            sqlStat.AppendLine("             ,AMTDISCOUNT")
            sqlStat.AppendLine("             ,DEMURTO")
            sqlStat.AppendLine("             ,DEMURUSRATE1")
            sqlStat.AppendLine("             ,DEMURUSRATE2")
            sqlStat.AppendLine("             ,AGENTORGANIZER")
            sqlStat.AppendLine("             ,AGENTPOL1")
            sqlStat.AppendLine("             ,AGENTPOL2")
            sqlStat.AppendLine("             ,AGENTPOD1")
            sqlStat.AppendLine("             ,AGENTPOD2")
            sqlStat.AppendLine("             ,APPLYTEXT")
            sqlStat.AppendLine("             ,COUNTRYORGANIZER")
            sqlStat.AppendLine("             ,LASTORDERNO")
            sqlStat.AppendLine("             ,TANKNO")
            sqlStat.AppendLine("             ,DEPOTCODE")
            sqlStat.AppendLine("             ,TWOAGOPRODUCT")
            sqlStat.AppendLine("             ,FEE")
            sqlStat.AppendLine("             ,BILLINGCATEGORY")
            sqlStat.AppendLine("             ,USINGLEASETANK")
            sqlStat.AppendLine("             ,REPAIRDEPOINDATE")
            sqlStat.AppendLine("             ,REPAIRBRID")
            sqlStat.AppendLine("             ,REMARK")
            sqlStat.AppendLine("             ,DELFLG")
            sqlStat.AppendLine("             ,INITYMD ")
            sqlStat.AppendLine("             ,INITUSER ")
            sqlStat.AppendLine("             ,UPDYMD ")
            sqlStat.AppendLine("             ,UPDUSER ")
            sqlStat.AppendLine("             ,UPDTERMID ")
            sqlStat.AppendLine("             ,RECEIVEYMD ")
            sqlStat.AppendLine("   ) VALUES ( ")
            sqlStat.AppendLine("              @BRID")
            sqlStat.AppendLine("             ,@BRBASEID")
            sqlStat.AppendLine("             ,@STYMD")
            sqlStat.AppendLine("             ,@VALIDITYFROM")
            sqlStat.AppendLine("             ,@VALIDITYTO")
            sqlStat.AppendLine("             ,@TERMTYPE")
            sqlStat.AppendLine("             ,@NOOFTANKS")
            sqlStat.AppendLine("             ,@SHIPPER")
            sqlStat.AppendLine("             ,@CONSIGNEE")
            sqlStat.AppendLine("             ,@CARRIER1")
            sqlStat.AppendLine("             ,@CARRIER2")
            sqlStat.AppendLine("             ,@PRODUCTCODE")
            sqlStat.AppendLine("             ,@PRODUCTWEIGHT")
            sqlStat.AppendLine("             ,@RECIEPTCOUNTRY1")
            sqlStat.AppendLine("             ,@RECIEPTPORT1")
            sqlStat.AppendLine("             ,@RECIEPTCOUNTRY2")
            sqlStat.AppendLine("             ,@RECIEPTPORT2")
            sqlStat.AppendLine("             ,@LOADCOUNTRY1")
            sqlStat.AppendLine("             ,@LOADPORT1")
            sqlStat.AppendLine("             ,@LOADCOUNTRY2")
            sqlStat.AppendLine("             ,@LOADPORT2")
            sqlStat.AppendLine("             ,@DISCHARGECOUNTRY1")
            sqlStat.AppendLine("             ,@DISCHARGEPORT1")
            sqlStat.AppendLine("             ,@DISCHARGECOUNTRY2")
            sqlStat.AppendLine("             ,@DISCHARGEPORT2")
            sqlStat.AppendLine("             ,@DELIVERYCOUNTRY1")
            sqlStat.AppendLine("             ,@DELIVERYPORT1")
            sqlStat.AppendLine("             ,@DELIVERYCOUNTRY2")
            sqlStat.AppendLine("             ,@DELIVERYPORT2")
            sqlStat.AppendLine("             ,@VSL1")
            sqlStat.AppendLine("             ,@VOY1")
            sqlStat.AppendLine("             ,@ETD1")
            sqlStat.AppendLine("             ,@ETA1")
            sqlStat.AppendLine("             ,@VSL2")
            sqlStat.AppendLine("             ,@VOY2")
            sqlStat.AppendLine("             ,@ETD2")
            sqlStat.AppendLine("             ,@ETA2")
            sqlStat.AppendLine("             ,@INVOICEDBY")
            sqlStat.AppendLine("             ,@LOADING")
            sqlStat.AppendLine("             ,@STEAMING")
            sqlStat.AppendLine("             ,@TIP")
            sqlStat.AppendLine("             ,@EXTRA")
            sqlStat.AppendLine("             ,@JOTHIREAGE")
            sqlStat.AppendLine("             ,@COMMERCIALFACTOR")
            sqlStat.AppendLine("             ,@AMTREQUEST")
            sqlStat.AppendLine("             ,@AMTPRINCIPAL")
            sqlStat.AppendLine("             ,@AMTDISCOUNT")
            sqlStat.AppendLine("             ,@DEMURTO")
            sqlStat.AppendLine("             ,@DEMURUSRATE1")
            sqlStat.AppendLine("             ,@DEMURUSRATE2")
            sqlStat.AppendLine("             ,@AGENTORGANIZER")
            sqlStat.AppendLine("             ,@AGENTPOL1")
            sqlStat.AppendLine("             ,@AGENTPOL2")
            sqlStat.AppendLine("             ,@AGENTPOD1")
            sqlStat.AppendLine("             ,@AGENTPOD2")
            sqlStat.AppendLine("             ,@APPLYTEXT")
            sqlStat.AppendLine("             ,@COUNTRYORGANIZER")
            sqlStat.AppendLine("             ,@LASTORDERNO")
            sqlStat.AppendLine("             ,@TANKNO")
            sqlStat.AppendLine("             ,@DEPOTCODE")
            sqlStat.AppendLine("             ,@TWOAGOPRODUCT")
            sqlStat.AppendLine("             ,@FEE")
            sqlStat.AppendLine("             ,@BILLINGCATEGORY")
            sqlStat.AppendLine("             ,@USINGLEASETANK")
            sqlStat.AppendLine("             ,@REPAIRDEPOINDATE")
            sqlStat.AppendLine("             ,@REPAIRBRID")
            sqlStat.AppendLine("             ,@REMARK")
            sqlStat.AppendLine("             ,@DELFLG")
            sqlStat.AppendLine("             ,@INITYMD ")
            sqlStat.AppendLine("             ,@INITUSER ")
            sqlStat.AppendLine("             ,@UPDYMD ")
            sqlStat.AppendLine("             ,@UPDUSER ")
            sqlStat.AppendLine("             ,@UPDTERMID ")
            sqlStat.AppendLine("             ,@RECEIVEYMD ")
            sqlStat.AppendLine(") ")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    'パラメータ設定
                    Dim dr As DataRow = ownerDt.Rows(0)
                    Dim BrBaseId As String = brInfoDt("INFO").LinkId

                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@BRBASEID", SqlDbType.NVarChar, 20).Value = BrBaseId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@VALIDITYFROM", SqlDbType.Date).Value = procDateTime
                    .Add("@VALIDITYTO", SqlDbType.Date).Value = procDateTime
                    .Add("@TERMTYPE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TERMTYPE"))
                    .Add("@NOOFTANKS", SqlDbType.Int, 20).Value = IntStringToInt(Convert.ToString(dr.Item("NOOFTANKS")))
                    .Add("@SHIPPER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("SHIPPER"))
                    .Add("@CONSIGNEE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CONSIGNEE"))
                    .Add("@CARRIER1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CARRIER1"))
                    .Add("@CARRIER2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CARRIER2"))
                    .Add("@PRODUCTCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LASTPRODUCT"))
                    .Add("@PRODUCTWEIGHT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("PRODUCTWEIGHT")))
                    .Add("@RECIEPTCOUNTRY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("RECIEPTCOUNTRY1"))
                    .Add("@RECIEPTPORT1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("RECIEPTPORT1"))
                    .Add("@RECIEPTCOUNTRY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("RECIEPTCOUNTRY2"))
                    .Add("@RECIEPTPORT2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("RECIEPTPORT2"))
                    .Add("@LOADCOUNTRY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LOADCOUNTRY1"))
                    .Add("@LOADPORT1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LOADPORT1"))
                    .Add("@LOADCOUNTRY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LOADCOUNTRY2"))
                    .Add("@LOADPORT2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LOADPORT2"))
                    .Add("@DISCHARGECOUNTRY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DISCHARGECOUNTRY1"))
                    .Add("@DISCHARGEPORT1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DISCHARGEPORT1"))
                    .Add("@DISCHARGECOUNTRY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DISCHARGECOUNTRY2"))
                    .Add("@DISCHARGEPORT2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DISCHARGEPORT2"))
                    .Add("@DELIVERYCOUNTRY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DELIVERYCOUNTRY1"))
                    .Add("@DELIVERYPORT1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DELIVERYPORT1"))
                    .Add("@DELIVERYCOUNTRY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DELIVERYCOUNTRY2"))
                    .Add("@DELIVERYPORT2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DELIVERYPORT2"))
                    .Add("@VSL1", SqlDbType.NVarChar, 50).Value = Convert.ToString(dr.Item("VSL1"))
                    .Add("@VOY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("VOY1"))
                    .Add("@ETD1", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ETD1")))
                    .Add("@ETA1", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ETA1")))
                    .Add("@VSL2", SqlDbType.NVarChar, 50).Value = Convert.ToString(dr.Item("VSL2"))
                    .Add("@VOY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("VOY2"))
                    .Add("@ETD2", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ETD2")))
                    .Add("@ETA2", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ETA2")))
                    .Add("@INVOICEDBY", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTORGANIZER"))
                    .Add("@LOADING", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("LOADING")))
                    .Add("@STEAMING", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("STEAMING")))
                    .Add("@TIP", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("TIP")))
                    .Add("@EXTRA", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("EXTRA")))
                    .Add("@JOTHIREAGE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("JOTHIREAGE")))
                    .Add("@COMMERCIALFACTOR", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("COMMERCIALFACTOR")))
                    .Add("@AMTREQUEST", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMTREQUEST")))
                    .Add("@AMTPRINCIPAL", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMTPRINCIPAL")))
                    .Add("@AMTDISCOUNT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMTDISCOUNT")))
                    .Add("@DEMURTO", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("DEMURTO")))
                    .Add("@DEMURUSRATE1", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("DEMURUSRATE1")))
                    .Add("@DEMURUSRATE2", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("DEMURUSRATE2")))
                    .Add("@AGENTORGANIZER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTORGANIZER"))
                    .Add("@AGENTPOL1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTORGANIZER"))
                    .Add("@AGENTPOL2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOL2"))
                    .Add("@AGENTPOD1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOD1"))
                    .Add("@AGENTPOD2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOD2"))
                    .Add("@APPLYTEXT", SqlDbType.NVarChar, 1024).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                    .Add("@COUNTRYORGANIZER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("COUNTRYORGANIZER"))
                    .Add("@LASTORDERNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LASTORDERNO"))
                    .Add("@TANKNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TANKNO"))
                    .Add("@DEPOTCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DEPOTCODE"))
                    .Add("@TWOAGOPRODUCT", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TWOAGOPRODUCT"))
                    .Add("@FEE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("FEE")))
                    .Add("@BILLINGCATEGORY", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BILLINGCATEGORY"))
                    .Add("@USINGLEASETANK", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("USINGLEASETANK"))
                    .Add("@REPAIRDEPOINDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("REPAIRDEPOINDATE")))
                    .Add("@REPAIRBRID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REPAIRBRID"))
                    .Add("@REMARK", SqlDbType.NVarChar, 1024).Value = Convert.ToString(dr.Item("REMARK"))
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                'パラメータ変数定義
                sqlCmd.ExecuteNonQuery()

            End Using

            '******************************
            ' 費用情報インサート
            '******************************
            sqlStat.Clear()
            sqlStat.AppendLine("INSERT INTO GBT0003_BR_VALUE (")
            sqlStat.AppendLine("              BRID")
            sqlStat.AppendLine("             ,BRVALUEID")
            sqlStat.AppendLine("             ,STYMD")
            sqlStat.AppendLine("             ,DTLPOLPOD")
            sqlStat.AppendLine("             ,DTLOFFICE")
            sqlStat.AppendLine("             ,COSTCODE")
            sqlStat.AppendLine("             ,BASEON")
            sqlStat.AppendLine("             ,TAX")
            sqlStat.AppendLine("             ,USD")
            sqlStat.AppendLine("             ,LOCAL")
            sqlStat.AppendLine("             ,CONTRACTOR")
            sqlStat.AppendLine("             ,USDRATE")
            sqlStat.AppendLine("             ,LOCALRATE")
            sqlStat.AppendLine("             ,CURRENCYCODE")
            sqlStat.AppendLine("             ,AGENT")
            sqlStat.AppendLine("             ,CLASS2")
            sqlStat.AppendLine("             ,CLASS8")
            sqlStat.AppendLine("             ,COUNTRYCODE")
            sqlStat.AppendLine("             ,REPAIRFLG")
            sqlStat.AppendLine("             ,APPROVEDUSD")
            sqlStat.AppendLine("             ,INVOICEDBY")
            sqlStat.AppendLine("             ,REMARK")
            sqlStat.AppendLine("             ,DELFLG")
            sqlStat.AppendLine("             ,INITYMD ")
            sqlStat.AppendLine("             ,INITUSER ")
            sqlStat.AppendLine("             ,UPDYMD ")
            sqlStat.AppendLine("             ,UPDUSER ")
            sqlStat.AppendLine("             ,UPDTERMID ")
            sqlStat.AppendLine("             ,RECEIVEYMD ")
            sqlStat.AppendLine("   ) VALUES ( ")
            sqlStat.AppendLine("              @BRID")
            sqlStat.AppendLine("             ,@BRVALUEID")
            sqlStat.AppendLine("             ,@STYMD")
            sqlStat.AppendLine("             ,@DTLPOLPOD")
            sqlStat.AppendLine("             ,@DTLOFFICE")
            sqlStat.AppendLine("             ,@COSTCODE")
            sqlStat.AppendLine("             ,@BASEON")
            sqlStat.AppendLine("             ,@TAX")
            sqlStat.AppendLine("             ,@USD")
            sqlStat.AppendLine("             ,@LOCAL")
            sqlStat.AppendLine("             ,@CONTRACTOR")
            sqlStat.AppendLine("             ,@USDRATE")
            sqlStat.AppendLine("             ,@LOCALRATE")
            sqlStat.AppendLine("             ,@CURRENCYCODE")
            sqlStat.AppendLine("             ,@AGENT")
            sqlStat.AppendLine("             ,@CLASS2")
            sqlStat.AppendLine("             ,@CLASS8")
            sqlStat.AppendLine("             ,@COUNTRYCODE")
            sqlStat.AppendLine("             ,@REPAIRFLG")
            sqlStat.AppendLine("             ,@APPROVEDUSD")
            sqlStat.AppendLine("             ,@INVOICEDBY")
            sqlStat.AppendLine("             ,@REMARK")
            sqlStat.AppendLine("             ,@DELFLG")
            sqlStat.AppendLine("             ,@INITYMD ")
            sqlStat.AppendLine("             ,@INITUSER ")
            sqlStat.AppendLine("             ,@UPDYMD ")
            sqlStat.AppendLine("             ,@UPDUSER ")
            sqlStat.AppendLine("             ,@UPDTERMID ")
            sqlStat.AppendLine("             ,@RECEIVEYMD ")
            sqlStat.AppendLine(") ")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    '固定パラメータ
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@AGENT", SqlDbType.NVarChar, 20).Value = GBA00003UserSetting.OFFICECODE
                    .Add("@CLASS8", SqlDbType.NVarChar, 50).Value = "1"
                    .Add("@INVOICEDBY", SqlDbType.NVarChar, 20).Value = GBA00003UserSetting.OFFICECODE
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                '動的パラメータ定義
                Dim paramBrvalueid As SqlParameter = sqlCmd.Parameters.Add("@BRVALUEID", SqlDbType.NVarChar, 20)
                Dim paramDtlpolpod As SqlParameter = sqlCmd.Parameters.Add("@DTLPOLPOD", SqlDbType.NVarChar, 20)
                Dim paramDtloffice As SqlParameter = sqlCmd.Parameters.Add("@DTLOFFICE", SqlDbType.NVarChar, 20)
                Dim paramCostcode As SqlParameter = sqlCmd.Parameters.Add("@COSTCODE", SqlDbType.NVarChar, 20)
                Dim paramBaseon As SqlParameter = sqlCmd.Parameters.Add("@BASEON", SqlDbType.Float)
                Dim paramTax As SqlParameter = sqlCmd.Parameters.Add("@TAX", SqlDbType.Float)
                Dim paramUsd As SqlParameter = sqlCmd.Parameters.Add("@USD", SqlDbType.Float)
                Dim paramLocal As SqlParameter = sqlCmd.Parameters.Add("@LOCAL", SqlDbType.Float)
                Dim paramContractor As SqlParameter = sqlCmd.Parameters.Add("@CONTRACTOR", SqlDbType.NVarChar, 20)
                Dim paramUsdrate As SqlParameter = sqlCmd.Parameters.Add("@USDRATE", SqlDbType.Float)
                Dim paramLocalrate As SqlParameter = sqlCmd.Parameters.Add("@LOCALRATE", SqlDbType.Float)
                Dim paramCurrencycode As SqlParameter = sqlCmd.Parameters.Add("@CURRENCYCODE", SqlDbType.NVarChar)
                Dim paramClass2 As SqlParameter = sqlCmd.Parameters.Add("@CLASS2", SqlDbType.NVarChar, 50)
                Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
                Dim paramRepairFlg As SqlParameter = sqlCmd.Parameters.Add("@REPAIRFLG", SqlDbType.NVarChar, 1)
                Dim paramApprovedUsd As SqlParameter = sqlCmd.Parameters.Add("@APPROVEDUSD", SqlDbType.Float)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)

                '動的パラメータを設定し実行
                For Each dr As DataRow In costDt.Rows
                    Dim dtlPolPod As String = Convert.ToString(dr.Item("DTLPOLPOD"))
                    paramBrvalueid.Value = brInfoDt(dtlPolPod).LinkId
                    paramDtlpolpod.Value = dtlPolPod
                    paramDtloffice.Value = Convert.ToString(dr.Item("DTLOFFICE"))
                    paramCostcode.Value = Convert.ToString(dr.Item("COSTCODE"))
                    paramBaseon.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("BASEON")))
                    paramTax.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("TAX")))
                    paramUsd.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("USD")))
                    paramLocal.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCAL")))
                    paramContractor.Value = Convert.ToString(dr.Item("CONTRACTOR"))
                    paramUsdrate.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("USDRATE")))
                    paramLocalrate.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCALRATE")))

                    If DecimalStringToDecimal(Convert.ToString(dr.Item("LOCAL"))) <> 0 Then
                        paramCurrencycode.Value = Convert.ToString(dr.Item("CURRENCYCODE"))
                    Else
                        paramCurrencycode.Value = "USD"
                    End If


                    paramClass2.Value = Convert.ToString(dr.Item("CLASS2"))

                    paramCountryCode.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                    paramRepairFlg.Value = Convert.ToString(dr.Item("REPAIRFLG"))
                    paramApprovedUsd.Value = Convert.ToString(dr.Item("APPROVEDUSD"))

                    paramRemark.Value = Convert.ToString(dr.Item("REMARK"))

                    sqlCmd.ExecuteNonQuery()
                Next

            End Using
            Me.lblBrNo.Text = brId
            tran.Commit()
        Catch ex As Exception
            Throw
        Finally
            If tran IsNot Nothing Then
                tran.Dispose()
            End If
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try
    End Sub
    ''' <summary>
    ''' ブレーカー情報を取得し更新可能かチェック
    ''' </summary>
    ''' <param name="brInfoDt"></param>
    ''' <returns></returns>
    ''' <remarks>要タブ・権限に応じた制御</remarks>
    Private Function CanBreakerUpdate(brInfoDt As Dictionary(Of String, BreakerInfo), Optional sqlCon As SqlConnection = Nothing) As Boolean
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim brNo As String = brInfoDt.Values(0).BrId
            '更新直前のブレーカー紐づけテーブル取得
            Dim brInfoData = GetBreakerInfo(brNo, sqlCon)
            'タイムスタンプが一致していない場合は更新不可
            For Each brInfoKey As String In brInfoData.Keys
                If Not (brInfoDt(brInfoKey).UpdYmd = brInfoData(brInfoKey).UpdYmd AndAlso
                        brInfoDt(brInfoKey).UpdUser = brInfoData(brInfoKey).UpdUser AndAlso
                        brInfoDt(brInfoKey).UpdTermId = brInfoData(brInfoKey).UpdTermId) Then
                    Return False
                End If
            Next

            Return True

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
    ''' ブレーカー情報更新
    ''' </summary>
    ''' <param name="brInfoDt"></param>
    ''' <param name="ownerDt"></param>
    ''' <param name="costDt"></param>
    ''' <param name="sqlCon"></param>
    ''' <remarks>TODO：タブや権限による制御、現状一律更新</remarks>
    Private Sub UpdateBreaker(brInfoDt As Dictionary(Of String, BreakerInfo), ownerDt As DataTable, costDt As DataTable, Optional sqlCon As SqlConnection = Nothing, Optional isSendConfirm As Boolean = False)
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Dim tran As SqlTransaction = Nothing
        Dim procDateTime As DateTime = DateTime.Now

        Dim brId As String = brInfoDt.Values(0).BrId
        Dim InsBrInfo = New Dictionary(Of String, BreakerInfo)
        Dim commitedLinkId As String = ""
        Dim commitedSubId As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            tran = sqlCon.BeginTransaction() 'トランザクション開始

            '連番変更
            For Each brInfoItem In brInfoDt.Values
                Dim linkIdNoString As String = brInfoItem.LinkId.Replace(brInfoItem.Type & "-", "")
                linkIdNoString = brInfoItem.Type & "-" & Format(Integer.Parse(linkIdNoString) + 1, "00000")
                Dim insItem As New BreakerInfo
                insItem.BrId = brInfoItem.BrId
                insItem.BrType = brInfoItem.BrType
                insItem.LinkId = linkIdNoString
                insItem.Remark = brInfoItem.Remark
                insItem.SubId = brInfoItem.SubId
                insItem.Type = brInfoItem.Type
                insItem.UseType = brInfoItem.UseType
                insItem.ApplyId = brInfoItem.ApplyId
                insItem.LastStep = brInfoItem.LastStep
                InsBrInfo.Add(insItem.Type, insItem)
                '***************************************
                'それぞれ既存データに削除フラグを立てる
                '***************************************
                'PODの場合、対象のみ実施
                If brInfoItem.Type = "POD1" OrElse brInfoItem.Type = "POD2" Then
                    Continue For
                End If

                '紐付け情報
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
                sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD   = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE BRID    = @BRID")
                sqlStat.AppendLine("   AND SUBID   = @SUBID")
                sqlStat.AppendLine("   AND TYPE    = @TYPE")
                sqlStat.AppendLine("   AND LINKID  = @LINKID")
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    With sqlCmd.Parameters
                        'パラメータ設定
                        .Add("@BRID", SqlDbType.NVarChar, 20).Value = brInfoItem.BrId
                        .Add("@SUBID", SqlDbType.NVarChar, 20).Value = brInfoItem.SubId
                        .Add("@TYPE", SqlDbType.NVarChar, 20).Value = brInfoItem.Type
                        .Add("@LINKID", SqlDbType.NVarChar, 20).Value = brInfoItem.LinkId
                        .Add("@BRTYPE", SqlDbType.NVarChar, 20).Value = brInfoItem.BrType
                        .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = brInfoItem.UseType
                        .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                        .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using
                '基本情報
                sqlStat.Clear()
                If brInfoItem.Type = "INFO" Then
                    sqlStat.AppendLine("UPDATE GBT0002_BR_BASE")
                    sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                    sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                    sqlStat.AppendLine("      ,RECEIVEYMD   = @RECEIVEYMD ")
                    sqlStat.AppendLine(" WHERE BRID      = @BRID")
                    sqlStat.AppendLine("   AND BRBASEID  = @LINKID")
                    commitedLinkId = linkIdNoString  'コミット後に直近のINFOに紐づくLINKIDをHidden項目に設定するため退避
                    commitedSubId = brInfoItem.SubId 'コミット後に直近のINFOに紐づくSUBIDをHidden項目に設定するため退避
                Else
                    sqlStat.AppendLine("UPDATE GBT0003_BR_VALUE")
                    sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                    sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                    sqlStat.AppendLine("      ,RECEIVEYMD   = @RECEIVEYMD ")
                    sqlStat.AppendLine(" WHERE BRID      = @BRID")
                    sqlStat.AppendLine("   AND BRVALUEID = @LINKID")
                End If
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    With sqlCmd.Parameters
                        'パラメータ設定
                        .Add("@BRID", SqlDbType.NVarChar, 20).Value = brInfoItem.BrId
                        .Add("@LINKID", SqlDbType.NVarChar, 20).Value = brInfoItem.LinkId
                        .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                        .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using
                '承認コメント更新
                Dim dr As DataRow = ownerDt.Rows(0)
                If Convert.ToString(dr.Item("APPROVEDTEXT")) <> "" Then
                    sqlStat.Clear()
                    sqlStat.AppendLine("UPDATE COT0002_APPROVALHIST")
                    sqlStat.AppendLine("   SET APPROVEDTEXT = @APPROVEDTEXT ")
                    sqlStat.AppendLine("      ,UPDYMD       = @UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER      = @UPDUSER ")
                    sqlStat.AppendLine("      ,RECEIVEYMD   = @RECEIVEYMD ")
                    sqlStat.AppendLine(" WHERE COMPCODE     = @COMPCODE")
                    sqlStat.AppendLine("   AND APPLYID      = @APPLYID")
                    sqlStat.AppendLine("   AND STEP         = @STEP")
                    sqlStat.AppendLine("   AND DELFLG       = @DELFLG")
                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                        With sqlCmd.Parameters
                            'パラメータ設定
                            .Add("@APPROVEDTEXT", SqlDbType.NVarChar, 1024).Value = dr.Item("APPROVEDTEXT")
                            .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVCamp
                            .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = brInfoItem.ApplyId
                            .Add("@STEP", SqlDbType.NVarChar, 20).Value = brInfoItem.LastStep
                            .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                            .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        End With
                        sqlCmd.ExecuteNonQuery()
                    End Using
                End If
            Next brInfoItem

            '******************************
            ' 紐づけ情報インサート
            '******************************
            sqlStat.Clear()
            sqlStat.AppendLine("INSERT INTO GBT0001_BR_INFO (")
            sqlStat.AppendLine("            BRID ")
            sqlStat.AppendLine("           ,SUBID ")
            sqlStat.AppendLine("           ,TYPE ")
            sqlStat.AppendLine("           ,LINKID ")
            sqlStat.AppendLine("           ,STYMD ")
            sqlStat.AppendLine("           ,BRTYPE ")
            sqlStat.AppendLine("           ,APPLYID ")
            sqlStat.AppendLine("           ,LASTSTEP ")
            sqlStat.AppendLine("           ,USETYPE ")
            sqlStat.AppendLine("           ,REMARK ")
            sqlStat.AppendLine("           ,DELFLG ")
            sqlStat.AppendLine("           ,INITYMD ")
            sqlStat.AppendLine("           ,UPDYMD ")
            sqlStat.AppendLine("           ,UPDUSER ")
            sqlStat.AppendLine("           ,UPDTERMID ")
            sqlStat.AppendLine("           ,RECEIVEYMD ")
            sqlStat.AppendLine("   ) VALUES ( ")
            sqlStat.AppendLine("            @BRID ")
            sqlStat.AppendLine("           ,@SUBID ")
            sqlStat.AppendLine("           ,@TYPE ")
            sqlStat.AppendLine("           ,@LINKID ")
            sqlStat.AppendLine("           ,@STYMD ")
            sqlStat.AppendLine("           ,@BRTYPE ")
            sqlStat.AppendLine("           ,@APPLYID ")
            sqlStat.AppendLine("           ,@LASTSTEP ")
            sqlStat.AppendLine("           ,@USETYPE ")
            sqlStat.AppendLine("           ,@REMARK ")
            sqlStat.AppendLine("           ,@DELFLG ")
            sqlStat.AppendLine("           ,@INITYMD ")
            sqlStat.AppendLine("           ,@UPDYMD ")
            sqlStat.AppendLine("           ,@UPDUSER ")
            sqlStat.AppendLine("           ,@UPDTERMID ")
            sqlStat.AppendLine("           ,@RECEIVEYMD ")
            sqlStat.AppendLine(") ")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    '固定パラメータ設定
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@SUBID", SqlDbType.NVarChar, 20).Value = InsBrInfo.Values(0).SubId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@BRTYPE", SqlDbType.NVarChar, 20).Value = InsBrInfo.Values(0).BrType
                    .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = InsBrInfo.Values(0).UseType
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = Me.txtDeleteFlag.Text
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                '動的パラメータ定義
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)
                Dim paramType As SqlParameter = sqlCmd.Parameters.Add("@TYPE", SqlDbType.NVarChar, 20)
                Dim paramLinkID As SqlParameter = sqlCmd.Parameters.Add("@LINKID", SqlDbType.NVarChar, 20)
                Dim paramApplyId As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", SqlDbType.NVarChar, 20)
                Dim paramLastStep As SqlParameter = sqlCmd.Parameters.Add("@LASTSTEP", SqlDbType.NVarChar, 20)

                For Each brInfoItem As BreakerInfo In InsBrInfo.Values
                    If {"POD1", "POD2"}.Contains(brInfoItem.Type) Then
                        Continue For
                    End If

                    '各行のパラメータ設定
                    paramRemark.Value = brInfoItem.Remark
                    paramType.Value = brInfoItem.Type
                    paramLinkID.Value = brInfoItem.LinkId
                    paramApplyId.Value = brInfoItem.ApplyId
                    paramLastStep.Value = brInfoItem.LastStep

                    'SQL実行
                    sqlCmd.ExecuteNonQuery()
                Next
            End Using

            ''オーガナイザーの場合、実施
            'If Me.hdnRejBtn.Value = "1" OrElse saveTab = "INFO" Then

            '******************************
            ' organizer情報（ブレーカー基本）インサート
            '******************************
            sqlStat.Clear()
            sqlStat.AppendLine("INSERT INTO GBT0002_BR_BASE (")
            sqlStat.AppendLine("              BRID")
            sqlStat.AppendLine("             ,BRBASEID")
            sqlStat.AppendLine("             ,STYMD")
            sqlStat.AppendLine("             ,VALIDITYFROM")
            sqlStat.AppendLine("             ,VALIDITYTO")
            sqlStat.AppendLine("             ,TERMTYPE")
            sqlStat.AppendLine("             ,NOOFTANKS")
            sqlStat.AppendLine("             ,SHIPPER")
            sqlStat.AppendLine("             ,CONSIGNEE")
            sqlStat.AppendLine("             ,CARRIER1")
            sqlStat.AppendLine("             ,CARRIER2")
            sqlStat.AppendLine("             ,PRODUCTCODE")
            sqlStat.AppendLine("             ,PRODUCTWEIGHT")
            sqlStat.AppendLine("             ,RECIEPTCOUNTRY1")
            sqlStat.AppendLine("             ,RECIEPTPORT1")
            sqlStat.AppendLine("             ,RECIEPTCOUNTRY2")
            sqlStat.AppendLine("             ,RECIEPTPORT2")
            sqlStat.AppendLine("             ,LOADCOUNTRY1")
            sqlStat.AppendLine("             ,LOADPORT1")
            sqlStat.AppendLine("             ,LOADCOUNTRY2")
            sqlStat.AppendLine("             ,LOADPORT2")
            sqlStat.AppendLine("             ,DISCHARGECOUNTRY1")
            sqlStat.AppendLine("             ,DISCHARGEPORT1")
            sqlStat.AppendLine("             ,DISCHARGECOUNTRY2")
            sqlStat.AppendLine("             ,DISCHARGEPORT2")
            sqlStat.AppendLine("             ,DELIVERYCOUNTRY1")
            sqlStat.AppendLine("             ,DELIVERYPORT1")
            sqlStat.AppendLine("             ,DELIVERYCOUNTRY2")
            sqlStat.AppendLine("             ,DELIVERYPORT2")
            sqlStat.AppendLine("             ,VSL1")
            sqlStat.AppendLine("             ,VOY1")
            sqlStat.AppendLine("             ,ETD1")
            sqlStat.AppendLine("             ,ETA1")
            sqlStat.AppendLine("             ,VSL2")
            sqlStat.AppendLine("             ,VOY2")
            sqlStat.AppendLine("             ,ETD2")
            sqlStat.AppendLine("             ,ETA2")
            sqlStat.AppendLine("             ,INVOICEDBY")
            sqlStat.AppendLine("             ,LOADING")
            sqlStat.AppendLine("             ,STEAMING")
            sqlStat.AppendLine("             ,TIP")
            sqlStat.AppendLine("             ,EXTRA")
            sqlStat.AppendLine("             ,JOTHIREAGE")
            sqlStat.AppendLine("             ,COMMERCIALFACTOR")
            sqlStat.AppendLine("             ,AMTREQUEST")
            sqlStat.AppendLine("             ,AMTPRINCIPAL")
            sqlStat.AppendLine("             ,AMTDISCOUNT")
            sqlStat.AppendLine("             ,DEMURTO")
            sqlStat.AppendLine("             ,DEMURUSRATE1")
            sqlStat.AppendLine("             ,DEMURUSRATE2")
            sqlStat.AppendLine("             ,AGENTORGANIZER")
            sqlStat.AppendLine("             ,AGENTPOL1")
            sqlStat.AppendLine("             ,AGENTPOL2")
            sqlStat.AppendLine("             ,AGENTPOD1")
            sqlStat.AppendLine("             ,AGENTPOD2")
            sqlStat.AppendLine("             ,APPLYTEXT")
            sqlStat.AppendLine("             ,COUNTRYORGANIZER")
            sqlStat.AppendLine("             ,LASTORDERNO")
            sqlStat.AppendLine("             ,TANKNO")
            sqlStat.AppendLine("             ,DEPOTCODE")
            sqlStat.AppendLine("             ,TWOAGOPRODUCT")
            sqlStat.AppendLine("             ,FEE")
            sqlStat.AppendLine("             ,BILLINGCATEGORY")
            sqlStat.AppendLine("             ,USINGLEASETANK")
            sqlStat.AppendLine("             ,REPAIRDEPOINDATE")
            sqlStat.AppendLine("             ,REPAIRBRID")
            sqlStat.AppendLine("             ,REMARK")
            sqlStat.AppendLine("             ,DELFLG")
            sqlStat.AppendLine("             ,INITYMD ")
            sqlStat.AppendLine("             ,INITUSER ")
            sqlStat.AppendLine("             ,UPDYMD ")
            sqlStat.AppendLine("             ,UPDUSER ")
            sqlStat.AppendLine("             ,UPDTERMID ")
            sqlStat.AppendLine("             ,RECEIVEYMD ")
            sqlStat.AppendLine("   ) VALUES ( ")
            sqlStat.AppendLine("              @BRID")
            sqlStat.AppendLine("             ,@BRBASEID")
            sqlStat.AppendLine("             ,@STYMD")
            sqlStat.AppendLine("             ,@VALIDITYFROM")
            sqlStat.AppendLine("             ,@VALIDITYTO")
            sqlStat.AppendLine("             ,@TERMTYPE")
            sqlStat.AppendLine("             ,@NOOFTANKS")
            sqlStat.AppendLine("             ,@SHIPPER")
            sqlStat.AppendLine("             ,@CONSIGNEE")
            sqlStat.AppendLine("             ,@CARRIER1")
            sqlStat.AppendLine("             ,@CARRIER2")
            sqlStat.AppendLine("             ,@PRODUCTCODE")
            sqlStat.AppendLine("             ,@PRODUCTWEIGHT")
            sqlStat.AppendLine("             ,@RECIEPTCOUNTRY1")
            sqlStat.AppendLine("             ,@RECIEPTPORT1")
            sqlStat.AppendLine("             ,@RECIEPTCOUNTRY2")
            sqlStat.AppendLine("             ,@RECIEPTPORT2")
            sqlStat.AppendLine("             ,@LOADCOUNTRY1")
            sqlStat.AppendLine("             ,@LOADPORT1")
            sqlStat.AppendLine("             ,@LOADCOUNTRY2")
            sqlStat.AppendLine("             ,@LOADPORT2")
            sqlStat.AppendLine("             ,@DISCHARGECOUNTRY1")
            sqlStat.AppendLine("             ,@DISCHARGEPORT1")
            sqlStat.AppendLine("             ,@DISCHARGECOUNTRY2")
            sqlStat.AppendLine("             ,@DISCHARGEPORT2")
            sqlStat.AppendLine("             ,@DELIVERYCOUNTRY1")
            sqlStat.AppendLine("             ,@DELIVERYPORT1")
            sqlStat.AppendLine("             ,@DELIVERYCOUNTRY2")
            sqlStat.AppendLine("             ,@DELIVERYPORT2")
            sqlStat.AppendLine("             ,@VSL1")
            sqlStat.AppendLine("             ,@VOY1")
            sqlStat.AppendLine("             ,@ETD1")
            sqlStat.AppendLine("             ,@ETA1")
            sqlStat.AppendLine("             ,@VSL2")
            sqlStat.AppendLine("             ,@VOY2")
            sqlStat.AppendLine("             ,@ETD2")
            sqlStat.AppendLine("             ,@ETA2")
            sqlStat.AppendLine("             ,@INVOICEDBY")
            sqlStat.AppendLine("             ,@LOADING")
            sqlStat.AppendLine("             ,@STEAMING")
            sqlStat.AppendLine("             ,@TIP")
            sqlStat.AppendLine("             ,@EXTRA")
            sqlStat.AppendLine("             ,@JOTHIREAGE")
            sqlStat.AppendLine("             ,@COMMERCIALFACTOR")
            sqlStat.AppendLine("             ,@AMTREQUEST")
            sqlStat.AppendLine("             ,@AMTPRINCIPAL")
            sqlStat.AppendLine("             ,@AMTDISCOUNT")
            sqlStat.AppendLine("             ,@DEMURTO")
            sqlStat.AppendLine("             ,@DEMURUSRATE1")
            sqlStat.AppendLine("             ,@DEMURUSRATE2")
            sqlStat.AppendLine("             ,@AGENTORGANIZER")
            sqlStat.AppendLine("             ,@AGENTPOL1")
            sqlStat.AppendLine("             ,@AGENTPOL2")
            sqlStat.AppendLine("             ,@AGENTPOD1")
            sqlStat.AppendLine("             ,@AGENTPOD2")
            sqlStat.AppendLine("             ,@APPLYTEXT")
            sqlStat.AppendLine("             ,@COUNTRYORGANIZER")
            sqlStat.AppendLine("             ,@LASTORDERNO")
            sqlStat.AppendLine("             ,@TANKNO")
            sqlStat.AppendLine("             ,@DEPOTCODE")
            sqlStat.AppendLine("             ,@TWOAGOPRODUCT")
            sqlStat.AppendLine("             ,@FEE")
            sqlStat.AppendLine("             ,@BILLINGCATEGORY")
            sqlStat.AppendLine("             ,@USINGLEASETANK")
            sqlStat.AppendLine("             ,@REPAIRDEPOINDATE")
            sqlStat.AppendLine("             ,@REPAIRBRID")
            sqlStat.AppendLine("             ,@REMARK")
            sqlStat.AppendLine("             ,@DELFLG")
            sqlStat.AppendLine("             ,@INITYMD ")
            sqlStat.AppendLine("             ,@INITUSER ")
            sqlStat.AppendLine("             ,@UPDYMD ")
            sqlStat.AppendLine("             ,@UPDUSER ")
            sqlStat.AppendLine("             ,@UPDTERMID ")
            sqlStat.AppendLine("             ,@RECEIVEYMD ")
            sqlStat.AppendLine(") ")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    Dim dr As DataRow = ownerDt.Rows(0)
                    Dim BrBaseId As String = InsBrInfo("INFO").LinkId
                    'パラメータ設定
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@BRBASEID", SqlDbType.NVarChar, 20).Value = BrBaseId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@VALIDITYFROM", SqlDbType.Date).Value = procDateTime
                    .Add("@VALIDITYTO", SqlDbType.Date).Value = procDateTime
                    .Add("@TERMTYPE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TERMTYPE"))
                    .Add("@NOOFTANKS", SqlDbType.Int, 20).Value = IntStringToInt(Convert.ToString(dr.Item("NOOFTANKS")))
                    .Add("@SHIPPER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("SHIPPER"))
                    .Add("@CONSIGNEE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CONSIGNEE"))
                    .Add("@CARRIER1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CARRIER1"))
                    .Add("@CARRIER2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CARRIER2"))
                    .Add("@PRODUCTCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LASTPRODUCT"))
                    .Add("@PRODUCTWEIGHT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("PRODUCTWEIGHT")))
                    .Add("@RECIEPTCOUNTRY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("RECIEPTCOUNTRY1"))
                    .Add("@RECIEPTPORT1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("RECIEPTPORT1"))
                    .Add("@RECIEPTCOUNTRY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("RECIEPTCOUNTRY2"))
                    .Add("@RECIEPTPORT2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("RECIEPTPORT2"))
                    .Add("@LOADCOUNTRY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LOADCOUNTRY1"))
                    .Add("@LOADPORT1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LOADPORT1"))
                    .Add("@LOADCOUNTRY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LOADCOUNTRY2"))
                    .Add("@LOADPORT2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LOADPORT2"))
                    .Add("@DISCHARGECOUNTRY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DISCHARGECOUNTRY1"))
                    .Add("@DISCHARGEPORT1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DISCHARGEPORT1"))
                    .Add("@DISCHARGECOUNTRY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DISCHARGECOUNTRY2"))
                    .Add("@DISCHARGEPORT2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DISCHARGEPORT2"))
                    .Add("@DELIVERYCOUNTRY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DELIVERYCOUNTRY1"))
                    .Add("@DELIVERYPORT1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DELIVERYPORT1"))
                    .Add("@DELIVERYCOUNTRY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DELIVERYCOUNTRY2"))
                    .Add("@DELIVERYPORT2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DELIVERYPORT2"))
                    .Add("@VSL1", SqlDbType.NVarChar, 50).Value = Convert.ToString(dr.Item("VSL1"))
                    .Add("@VOY1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("VOY1"))
                    .Add("@ETD1", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ETD1")))
                    .Add("@ETA1", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ETA1")))
                    .Add("@VSL2", SqlDbType.NVarChar, 50).Value = Convert.ToString(dr.Item("VSL2"))
                    .Add("@VOY2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("VOY2"))
                    .Add("@ETD2", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ETD2")))
                    .Add("@ETA2", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ETA2")))
                    .Add("@INVOICEDBY", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTORGANIZER"))
                    .Add("@LOADING", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("LOADING")))
                    .Add("@STEAMING", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("STEAMING")))
                    .Add("@TIP", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("TIP")))
                    .Add("@EXTRA", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("EXTRA")))
                    .Add("@JOTHIREAGE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("JOTHIREAGE")))
                    .Add("@COMMERCIALFACTOR", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("COMMERCIALFACTOR")))
                    .Add("@AMTREQUEST", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMTREQUEST")))
                    .Add("@AMTPRINCIPAL", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMTPRINCIPAL")))
                    .Add("@AMTDISCOUNT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMTDISCOUNT")))
                    .Add("@DEMURTO", SqlDbType.Int).Value = IntStringToInt(Convert.ToString(dr.Item("DEMURTO")))
                    .Add("@DEMURUSRATE1", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("DEMURUSRATE1")))
                    .Add("@DEMURUSRATE2", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("DEMURUSRATE2")))
                    .Add("@AGENTORGANIZER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTORGANIZER"))
                    .Add("@AGENTPOL1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTORGANIZER"))
                    .Add("@AGENTPOL2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOL2"))
                    .Add("@AGENTPOD1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOD1"))
                    .Add("@AGENTPOD2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOD2"))
                    .Add("@APPLYTEXT", SqlDbType.NVarChar, 1024).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                    .Add("@COUNTRYORGANIZER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("COUNTRYORGANIZER"))
                    .Add("@LASTORDERNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LASTORDERNO"))
                    .Add("@TANKNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TANKNO"))
                    .Add("@DEPOTCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DEPOTCODE"))
                    .Add("@TWOAGOPRODUCT", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TWOAGOPRODUCT"))
                    .Add("@FEE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("FEE")))
                    .Add("@BILLINGCATEGORY", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BILLINGCATEGORY"))
                    .Add("@USINGLEASETANK", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("USINGLEASETANK"))
                    .Add("@REPAIRDEPOINDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("REPAIRDEPOINDATE")))
                    .Add("@REPAIRBRID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REPAIRBRID"))
                    .Add("@REMARK", SqlDbType.NVarChar, 1024).Value = Convert.ToString(dr.Item("REMARK"))
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = Me.txtDeleteFlag.Text
                    .Add("@INITYMD", SqlDbType.DateTime).Value = Me.hdnInitYmd.Value
                    .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = Me.hdnInitUser.Value
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                End With
                sqlCmd.ExecuteNonQuery()
            End Using

            '******************************
            ' 費用情報インサート
            '******************************
            sqlStat.Clear()
            sqlStat.AppendLine("INSERT INTO GBT0003_BR_VALUE (")
            sqlStat.AppendLine("              BRID")
            sqlStat.AppendLine("             ,BRVALUEID")
            sqlStat.AppendLine("             ,STYMD")
            sqlStat.AppendLine("             ,DTLPOLPOD")
            sqlStat.AppendLine("             ,DTLOFFICE")
            sqlStat.AppendLine("             ,COSTCODE")
            sqlStat.AppendLine("             ,BASEON")
            sqlStat.AppendLine("             ,TAX")
            sqlStat.AppendLine("             ,USD")
            sqlStat.AppendLine("             ,LOCAL")
            sqlStat.AppendLine("             ,CONTRACTOR")
            sqlStat.AppendLine("             ,USDRATE")
            sqlStat.AppendLine("             ,LOCALRATE")
            sqlStat.AppendLine("             ,CURRENCYCODE")
            sqlStat.AppendLine("             ,AGENT")
            sqlStat.AppendLine("             ,CLASS2")
            sqlStat.AppendLine("             ,CLASS8")
            sqlStat.AppendLine("             ,COUNTRYCODE")
            sqlStat.AppendLine("             ,REPAIRFLG")
            sqlStat.AppendLine("             ,APPROVEDUSD")
            sqlStat.AppendLine("             ,INVOICEDBY")
            sqlStat.AppendLine("             ,REMARK")
            sqlStat.AppendLine("             ,DELFLG")
            sqlStat.AppendLine("             ,INITYMD ")
            sqlStat.AppendLine("             ,INITUSER ")
            sqlStat.AppendLine("             ,UPDYMD ")
            sqlStat.AppendLine("             ,UPDUSER ")
            sqlStat.AppendLine("             ,UPDTERMID ")
            sqlStat.AppendLine("             ,RECEIVEYMD ")
            sqlStat.AppendLine("   ) VALUES ( ")
            sqlStat.AppendLine("              @BRID")
            sqlStat.AppendLine("             ,@BRVALUEID")
            sqlStat.AppendLine("             ,@STYMD")
            sqlStat.AppendLine("             ,@DTLPOLPOD")
            sqlStat.AppendLine("             ,@DTLOFFICE")
            sqlStat.AppendLine("             ,@COSTCODE")
            sqlStat.AppendLine("             ,@BASEON")
            sqlStat.AppendLine("             ,@TAX")
            sqlStat.AppendLine("             ,@USD")
            sqlStat.AppendLine("             ,@LOCAL")
            sqlStat.AppendLine("             ,@CONTRACTOR")
            sqlStat.AppendLine("             ,@USDRATE")
            sqlStat.AppendLine("             ,@LOCALRATE")
            sqlStat.AppendLine("             ,@CURRENCYCODE")
            sqlStat.AppendLine("             ,@AGENT")
            sqlStat.AppendLine("             ,@CLASS2")
            sqlStat.AppendLine("             ,@CLASS8")
            sqlStat.AppendLine("             ,@COUNTRYCODE")
            sqlStat.AppendLine("             ,@REPAIRFLG")
            sqlStat.AppendLine("             ,@APPROVEDUSD")
            sqlStat.AppendLine("             ,@INVOICEDBY")
            sqlStat.AppendLine("             ,@REMARK")
            sqlStat.AppendLine("             ,@DELFLG")
            sqlStat.AppendLine("             ,@INITYMD ")
            sqlStat.AppendLine("             ,@INITUSER ")
            sqlStat.AppendLine("             ,@UPDYMD ")
            sqlStat.AppendLine("             ,@UPDUSER ")
            sqlStat.AppendLine("             ,@UPDTERMID ")
            sqlStat.AppendLine("             ,@RECEIVEYMD ")
            sqlStat.AppendLine(") ")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@CLASS8", SqlDbType.NVarChar, 50).Value = "1"
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = Me.txtDeleteFlag.Text
                    .Add("@INITYMD", SqlDbType.DateTime).Value = Me.hdnInitYmd.Value
                    .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = Me.hdnInitUser.Value
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                '動的パラメータ定義
                Dim paramBrvalueid As SqlParameter = sqlCmd.Parameters.Add("@BRVALUEID", SqlDbType.NVarChar, 20)
                Dim paramDtlpolpod As SqlParameter = sqlCmd.Parameters.Add("@DTLPOLPOD", SqlDbType.NVarChar, 20)
                Dim paramDtloffice As SqlParameter = sqlCmd.Parameters.Add("@DTLOFFICE", SqlDbType.NVarChar, 20)
                Dim paramCostcode As SqlParameter = sqlCmd.Parameters.Add("@COSTCODE", SqlDbType.NVarChar, 20)
                Dim paramBaseon As SqlParameter = sqlCmd.Parameters.Add("@BASEON", SqlDbType.Float)
                Dim paramTax As SqlParameter = sqlCmd.Parameters.Add("@TAX", SqlDbType.Float)
                Dim paramUsd As SqlParameter = sqlCmd.Parameters.Add("@USD", SqlDbType.Float)
                Dim paramLocal As SqlParameter = sqlCmd.Parameters.Add("@LOCAL", SqlDbType.Float)
                Dim paramContractor As SqlParameter = sqlCmd.Parameters.Add("@CONTRACTOR", SqlDbType.NVarChar, 20)
                Dim paramUsdrate As SqlParameter = sqlCmd.Parameters.Add("@USDRATE", SqlDbType.Float)
                Dim paramLocalrate As SqlParameter = sqlCmd.Parameters.Add("@LOCALRATE", SqlDbType.Float)
                Dim paramCurrencycode As SqlParameter = sqlCmd.Parameters.Add("@CURRENCYCODE", SqlDbType.NVarChar)
                Dim paramAgent As SqlParameter = sqlCmd.Parameters.Add("@AGENT", SqlDbType.NVarChar, 20)
                Dim paramClass2 As SqlParameter = sqlCmd.Parameters.Add("@CLASS2", SqlDbType.NVarChar, 50)
                Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
                Dim paramRepairFlg As SqlParameter = sqlCmd.Parameters.Add("@REPAIRFLG", SqlDbType.NVarChar, 1)
                Dim paramApprovedUsd As SqlParameter = sqlCmd.Parameters.Add("@APPROVEDUSD", SqlDbType.Float)
                Dim paramInvoicedBy As SqlParameter = sqlCmd.Parameters.Add("@INVOICEDBY", SqlDbType.NVarChar, 20)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)

                For Each dr As DataRow In costDt.Rows
                    Dim dtlPolPod As String = Convert.ToString(dr.Item("DTLPOLPOD"))

                    If {"POD1", "POD2"}.Contains(dtlPolPod) Then
                        Continue For
                    End If

                    paramBrvalueid.Value = InsBrInfo(dtlPolPod).LinkId
                    paramDtlpolpod.Value = dtlPolPod
                    paramDtloffice.Value = Convert.ToString(dr.Item("DTLOFFICE"))
                    paramCostcode.Value = Convert.ToString(dr.Item("COSTCODE"))
                    paramBaseon.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("BASEON")))
                    paramTax.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("TAX")))
                    paramUsd.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("USD")))
                    paramLocal.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCAL")))
                    paramContractor.Value = Convert.ToString(dr.Item("CONTRACTOR"))
                    paramUsdrate.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("USDRATE")))
                    paramLocalrate.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCALRATE")))

                    If DecimalStringToDecimal(Convert.ToString(dr.Item("LOCAL"))) <> 0 Then
                        paramCurrencycode.Value = Convert.ToString(dr.Item("CURRENCYCODE"))
                    Else
                        paramCurrencycode.Value = "USD"
                    End If

                    paramAgent.Value = Convert.ToString(dr.Item("AGENT"))
                    paramClass2.Value = Convert.ToString(dr.Item("CLASS2"))

                    paramCountryCode.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                    paramRepairFlg.Value = Convert.ToString(dr.Item("REPAIRFLG"))
                    paramApprovedUsd.Value = Convert.ToString(dr.Item("APPROVEDUSD"))
                    paramInvoicedBy.Value = Convert.ToString(dr.Item("INVOICEDBY"))
                    paramRemark.Value = Convert.ToString(dr.Item("REMARK"))
                    sqlCmd.ExecuteNonQuery()
                Next

            End Using

            Me.lblBrNo.Text = brId

            tran.Commit() 'トランザクションコミット
            Me.hdnLinkId.Value = commitedLinkId '直近のLinkIdを画面隠し項目に設定
            Me.hdnSubId.Value = commitedSubId   '直近のhdnSubIdを画面隠し項目に設定
        Catch ex As Exception
            Throw
        Finally
            If tran IsNot Nothing Then
                tran.Dispose()
            End If
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 新規作成時はブレーカー情報を作りこむ
    ''' </summary>
    Private Function SetBreakerInfo(brId As String, ownerDt As DataTable) As Dictionary(Of String, BreakerInfo)
        Dim retDic As New Dictionary(Of String, BreakerInfo)
        Dim dr As DataRow = ownerDt.Rows(0)
        Dim typeList As New List(Of String) From {"INFO", "POL1", "POD1"}

        For Each type In typeList
            Dim item As New BreakerInfo
            item.BrId = brId
            item.SubId = "S00001"
            item.Type = type
            item.LinkId = type & "-" & "00001"

            item.BrType = C_BRTYPE.REPAIR
            item.UseType = Convert.ToString(dr.Item("USETYPE"))
            If type = "INFO" Then
                item.Remark = Convert.ToString(dr.Item("SPECIALINS"))
            Else
                item.Remark = ""
            End If

            retDic.Add(type, item)
        Next
        Return retDic
    End Function


    ''' <summary>
    ''' ブレーカー関連付け情報取得
    ''' </summary>
    ''' <param name="sqlCon">オプション 項目</param>
    ''' <returns>ディクショナリ キー：区分(POD1、POL1等) , 値：直近ブレーカー関連付け</returns>
    Private Function GetBreakerInfo(brId As String, Optional sqlCon As SqlConnection = Nothing) As Dictionary(Of String, BreakerInfo)
        Dim canCloseConnect As Boolean = False
        Dim retDic As New Dictionary(Of String, BreakerInfo)
        Dim sqlStat As New Text.StringBuilder
        '生きているブレーカーは基本情報＋発地着地(最大4)の5レコード想定
        sqlStat.AppendLine("Select BI.BRID ")
        sqlStat.AppendLine("      ,BI.SUBID ")
        sqlStat.AppendLine("      ,BI.TYPE ")
        sqlStat.AppendLine("      ,BI.LINKID ")
        sqlStat.AppendLine("      ,BI.STYMD ")
        sqlStat.AppendLine("      ,BI.BRTYPE ")
        sqlStat.AppendLine("      ,BI.APPLYID ")
        sqlStat.AppendLine("      ,BI.LASTSTEP ")
        sqlStat.AppendLine("      ,BI.USETYPE ")
        sqlStat.AppendLine("      ,BI.REMARK ")
        sqlStat.AppendLine("      ,CAST(BI.UPDTIMSTP As bigint) AS TIMSTP")
        sqlStat.AppendLine("      ,BI.UPDYMD")
        sqlStat.AppendLine("      ,BI.UPDUSER")
        sqlStat.AppendLine("      ,BI.UPDTERMID")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO AS BI")
        If Me.hdnDelFlg.Value = CONST_FLAG_YES Then '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
            sqlStat.AppendLine("   INNER JOIN")
            sqlStat.AppendLine("        (SELECT BIS2.BRID")
            sqlStat.AppendLine("               ,BIS2.SUBID")
            sqlStat.AppendLine("               ,BIS2.TYPE")
            sqlStat.AppendLine("               ,MAX(BIS2.LINKID) AS LINKID")
            sqlStat.AppendLine("         FROM GBT0001_BR_INFO BIS2")
            sqlStat.AppendLine("         WHERE BIS2.BRTYPE =  '" & C_BRTYPE.REPAIR & "'")
            sqlStat.AppendLine("         GROUP BY BIS2.BRID, BIS2.SUBID, BIS2.TYPE) AS BIS")
            sqlStat.AppendLine("    On BIS.BRID   = @BRID")
            sqlStat.AppendLine("   And BIS.SUBID  = @SUBID")
            sqlStat.AppendLine("   And BIS.LINKID = BI.LINKID")
        End If
        sqlStat.AppendLine(" WHERE BI.BRID         = @BRID")
        sqlStat.AppendLine("   And BI.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And BI.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And BI.BRTYPE       = '" & C_BRTYPE.REPAIR & "'")
        If Me.hdnDelFlg.Value = CONST_FLAG_YES Then '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
            sqlStat.AppendLine("   And BI.SUBID    = @SUBID")
        Else
            sqlStat.AppendLine("   And BI.DELFLG  <> @DELFLG")
        End If

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVCamp
                End With
                If Me.hdnDelFlg.Value = CONST_FLAG_YES Then '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
                    Dim paramSubId As SqlParameter = sqlCmd.Parameters.Add("@SUBID", SqlDbType.NVarChar, 20)
                    Dim paramLinkId As SqlParameter = sqlCmd.Parameters.Add("@LINKID", SqlDbType.NVarChar, 20)
                    paramSubId.Value = Me.hdnSubId.Value
                    paramLinkId.Value = Me.hdnLinkId.Value
                Else
                    Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                    paramDelFlg.Value = CONST_FLAG_YES
                End If

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt IsNot Nothing Then
                        For Each dr As DataRow In dt.Rows
                            Dim item As New BreakerInfo
                            item.BrId = Convert.ToString(dr("BRID"))
                            item.SubId = Convert.ToString(dr("SUBID"))
                            item.Type = Convert.ToString(dr("TYPE"))
                            item.LinkId = Convert.ToString(dr("LINKID"))
                            item.Stymd = Convert.ToString(dr("STYMD"))
                            item.BrType = Convert.ToString(dr("BRTYPE"))
                            item.ApplyId = Convert.ToString(dr("APPLYID"))
                            item.LastStep = Convert.ToString(dr("LASTSTEP"))
                            item.UseType = Convert.ToString(dr("USETYPE"))
                            item.Remark = Convert.ToString(dr("REMARK"))
                            item.TimeStamp = Convert.ToString(dr("TIMSTP"))
                            item.UpdYmd = Convert.ToString(dr("UPDYMD"))
                            item.UpdUser = Convert.ToString(dr("UPDUSER"))
                            item.UpdTermId = Convert.ToString(dr("UPDTERMID"))
                            retDic.Add(item.Type, item)
                        Next
                    End If
                End Using

            End Using
            Return retDic
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
    ''' ブレーカー基本情報取得処理
    ''' </summary>
    ''' <param name="dicBrInfo"></param>
    ''' <returns></returns>
    Private Function GetBreakerBase(dicBrInfo As Dictionary(Of String, BreakerInfo), Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = Nothing
        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("SELECT BS.BRID AS BRID")
        sqlStat.AppendLine("      ,BS.BRBASEID AS BRBASEID")
        sqlStat.AppendLine("      ,BS.STYMD AS STYMD")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYFROM WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYFROM,'yyyy/MM/dd') END AS VALIDITYFROM")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYTO  ,'yyyy/MM/dd') END AS VALIDITYTO")
        sqlStat.AppendLine("      ,BS.TERMTYPE AS TERMTYPE")
        sqlStat.AppendLine("      ,BS.NOOFTANKS AS NOOFTANKS")
        sqlStat.AppendLine("      ,BS.SHIPPER AS SHIPPER")
        sqlStat.AppendLine("      ,BS.CONSIGNEE AS CONSIGNEE")
        sqlStat.AppendLine("      ,BS.CARRIER1 AS CARRIER1")
        sqlStat.AppendLine("      ,BS.CARRIER2 AS CARRIER2")
        'sqlStat.AppendLine("      ,BS.PRODUCTCODE AS PRODUCTCODE")
        sqlStat.AppendLine("      ,BS.PRODUCTWEIGHT AS PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,BS.RECIEPTCOUNTRY1 AS RECIEPTCOUNTRY1")
        sqlStat.AppendLine("      ,BS.RECIEPTPORT1 AS RECIEPTPORT1")
        sqlStat.AppendLine("      ,BS.RECIEPTCOUNTRY2 AS RECIEPTCOUNTRY2")
        sqlStat.AppendLine("      ,BS.RECIEPTPORT2 AS RECIEPTPORT2")
        sqlStat.AppendLine("      ,BS.LOADCOUNTRY1 AS LOADCOUNTRY1")
        sqlStat.AppendLine("      ,BS.LOADPORT1 AS LOADPORT1")
        sqlStat.AppendLine("      ,BS.LOADCOUNTRY2 AS LOADCOUNTRY2")
        sqlStat.AppendLine("      ,BS.LOADPORT2 AS LOADPORT2")
        sqlStat.AppendLine("      ,BS.DISCHARGECOUNTRY1 AS DISCHARGECOUNTRY1")
        sqlStat.AppendLine("      ,BS.DISCHARGEPORT1 AS DISCHARGEPORT1")
        sqlStat.AppendLine("      ,BS.DISCHARGECOUNTRY2 AS DISCHARGECOUNTRY2")
        sqlStat.AppendLine("      ,BS.DISCHARGEPORT2 AS DISCHARGEPORT2")
        sqlStat.AppendLine("      ,BS.DELIVERYCOUNTRY1 AS DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,BS.DELIVERYPORT1 AS DELIVERYPORT1")
        sqlStat.AppendLine("      ,BS.DELIVERYCOUNTRY2 AS DELIVERYCOUNTRY2")
        sqlStat.AppendLine("      ,BS.DELIVERYPORT2 AS DELIVERYPORT2")
        sqlStat.AppendLine("      ,BS.VSL1 AS VSL1")
        sqlStat.AppendLine("      ,BS.VOY1 AS VOY1")
        sqlStat.AppendLine("      ,CASE BS.ETD1 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETD1,'yyyy/MM/dd') END AS ETD1")
        sqlStat.AppendLine("      ,CASE BS.ETA1 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETA1,'yyyy/MM/dd') END AS ETA1")
        sqlStat.AppendLine("      ,BS.VSL2 AS VSL2")
        sqlStat.AppendLine("      ,BS.VOY2 AS VOY2")
        sqlStat.AppendLine("      ,CASE BS.ETD2 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETD2,'yyyy/MM/dd') END AS ETD2")
        sqlStat.AppendLine("      ,CASE BS.ETA2 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETA2,'yyyy/MM/dd') END AS ETA2")
        sqlStat.AppendLine("      ,BS.INVOICEDBY AS INVOICEDBY")
        sqlStat.AppendLine("      ,BS.LOADING AS LOADING")
        sqlStat.AppendLine("      ,BS.STEAMING AS STEAMING")
        sqlStat.AppendLine("      ,BS.TIP AS TIP")
        sqlStat.AppendLine("      ,BS.EXTRA AS EXTRA")
        sqlStat.AppendLine("      ,BS.JOTHIREAGE AS JOTHIREAGE")
        sqlStat.AppendLine("      ,BS.COMMERCIALFACTOR AS COMMERCIALFACTOR")
        sqlStat.AppendLine("      ,BS.AMTREQUEST AS AMTREQUEST")
        sqlStat.AppendLine("      ,BS.AMTPRINCIPAL AS AMTPRINCIPAL")
        sqlStat.AppendLine("      ,BS.AMTDISCOUNT AS AMTDISCOUNT")
        sqlStat.AppendLine("      ,BS.DEMURTO AS DEMURTO")
        sqlStat.AppendLine("      ,BS.DEMURUSRATE1 AS DEMURUSRATE1")
        sqlStat.AppendLine("      ,BS.DEMURUSRATE2 AS DEMURUSRATE2")
        sqlStat.AppendLine("      ,BS.AGENTORGANIZER AS AGENTORGANIZER")
        sqlStat.AppendLine("      ,BS.AGENTPOL1 AS AGENTPOL1")
        sqlStat.AppendLine("      ,BS.AGENTPOL2 AS AGENTPOL2")
        sqlStat.AppendLine("      ,BS.AGENTPOD1 AS AGENTPOD1")
        sqlStat.AppendLine("      ,BS.AGENTPOD2 AS AGENTPOD2")
        sqlStat.AppendLine("      ,BS.APPLYTEXT AS APPLYTEXT")
        sqlStat.AppendLine("      ,BS.REMARK AS REMARK")
        sqlStat.AppendLine("      ,ISNULL(AH.APPROVEDTEXT,'') AS APPROVEDTEXT")
        sqlStat.AppendLine("      ,CASE CONVERT(NVARCHAR, AH.APPLYDATE , 111) WHEN '1900/01/01' THEN '' ELSE CONVERT(NVARCHAR, AH.APPLYDATE , 111) END AS APPLYDATE")
        sqlStat.AppendLine("      ,AH.APPLICANTID AS APPLICANTID")
        sqlStat.AppendLine("      ,ISNULL(US1.STAFFNAMES_EN,'') AS APPLICANTNAME")
        sqlStat.AppendLine("      ,CASE CONVERT(NVARCHAR, AH.APPROVEDATE , 111) WHEN '1900/01/01' THEN '' ELSE CONVERT(NVARCHAR, AH.APPROVEDATE , 111) END AS APPROVEDATE")
        sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
        sqlStat.AppendLine("      ,ISNULL(US2.STAFFNAMES_EN,'') AS APPROVERNAME")
        sqlStat.AppendLine("      ,BS.TANKNO AS TANKNO")
        sqlStat.AppendLine("      ,ISNULL(BS.DEPOTCODE,'') AS DEPOTCODE")
        sqlStat.AppendLine("      ,ISNULL(DP.NAMES,'')  AS DEPOTNAME")
        sqlStat.AppendLine("      ,ISNULL(DP.LOCATION,'')   AS LOCATION")
        sqlStat.AppendLine("      ,CASE BS.REPAIRDEPOINDATE WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.REPAIRDEPOINDATE,'yyyy/MM/dd') END AS REPAIRDEPOINDATE")
        sqlStat.AppendLine("      ,BS.REPAIRBRID       AS REPAIRBRID")
        sqlStat.AppendLine("      ,BS.PRODUCTCODE      AS LASTPRODUCT")

        sqlStat.AppendLine("      ,ISNULL(PD.PRODUCTNAME,'') AS PRODUCTNAME")

        sqlStat.AppendLine("      ,ISNULL(BS.TWOAGOPRODUCT,'') AS TWOAGOPRODUCT")
        sqlStat.AppendLine("      ,ISNULL(PD2.PRODUCTNAME,'') AS TWOAGOPRODUCTNAME")

        sqlStat.AppendLine("      ,BS.FEE AS FEE")
        sqlStat.AppendLine("      ,BS.BILLINGCATEGORY AS BILLINGCATEGORY")
        sqlStat.AppendLine("      ,BS.USINGLEASETANK  AS USINGLEASETANK")

        sqlStat.AppendLine("      ,BS.LASTORDERNO     AS LASTORDERNO")
        sqlStat.AppendLine("      ,BS.DELFLG AS DELFLG")
        sqlStat.AppendLine("      ,ISNULL(trim(TK.REPAIRSTAT),'') AS TANKUSAGE")
        sqlStat.AppendLine("      ,ISNULL(TR.NAMES,'') AS OFFICENAME")
        sqlStat.AppendLine("      ,BS.COUNTRYORGANIZER AS COUNTRYORGANIZER")
        sqlStat.AppendLine("      ,format(BS.INITYMD,'yyyy/MM/dd HH:mm:ss.fff') AS INITYMD")
        sqlStat.AppendLine("      ,BS.INITUSER AS INITUSER")
        sqlStat.AppendLine("  FROM GBT0002_BR_BASE BS ")
        sqlStat.AppendLine("  LEFT JOIN GBT0001_BR_INFO BI ")
        sqlStat.AppendLine("   ON BI.BRID          = BS.BRID")
        sqlStat.AppendLine("  And BI.TYPE          = 'INFO'")
        If Me.hdnDelFlg.Value = CONST_FLAG_YES Then '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
            sqlStat.AppendLine("  And BI.SUBID     = @SUBID")
            sqlStat.AppendLine("  And BI.LINKID    = @LINKID")
        Else
            sqlStat.AppendLine("  And BI.DELFLG   <> @DELFLG")
        End If
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH")
        sqlStat.AppendLine("    ON  AH.COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND  AH.APPLYID     = BI.APPLYID")
        sqlStat.AppendLine("   AND  AH.STEP        = BI.LASTSTEP")
        sqlStat.AppendLine("   AND  AH.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OB")
        sqlStat.AppendLine("    ON  OB.ORDERNO      = BS.LASTORDERNO")
        sqlStat.AppendLine("   AND  OB.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE OV")
        sqlStat.AppendLine("    ON  OV.ORDERNO      = BS.LASTORDERNO")
        sqlStat.AppendLine("   AND  OV.TANKNO       = BS.TANKNO")
        sqlStat.AppendLine("   AND  OV.ACTIONID    IN('ETYD','ETYC')")
        sqlStat.AppendLine("   AND  OV.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  0 < (SELECT COUNT(*) FROM GBT0005_ODR_VALUE OVCT ")
        sqlStat.AppendLine("             WHERE BS.LASTORDERNO   = OVCT.ORDERNO")
        sqlStat.AppendLine("               AND BS.TANKNO        = OVCT.TANKNO")
        sqlStat.AppendLine("               AND OVCT.ACTIONID   IN('DOUT')")
        sqlStat.AppendLine("               AND OVCT.ACTUALDATE <> '1900/01/01'")
        sqlStat.AppendLine("               AND OVCT.DELFLG     <> '" & CONST_FLAG_YES & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0003_DEPOT DP")
        sqlStat.AppendLine("    ON  DP.DEPOTCODE    = BS.DEPOTCODE")
        sqlStat.AppendLine("   AND  DP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0006_TANK TK")
        sqlStat.AppendLine("    ON  TK.TANKNO    = BS.TANKNO")
        sqlStat.AppendLine("   AND  TK.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD")
        sqlStat.AppendLine("    ON  PD.PRODUCTCODE  = BS.PRODUCTCODE")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR")
        sqlStat.AppendLine("    ON  TR.CARRIERCODE  = BS.AGENTORGANIZER")
        sqlStat.AppendLine("   AND  TR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  TR.CLASS        = '" & C_TRADER.CLASS.AGENT & "' ")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US1")
        sqlStat.AppendLine("    ON  US1.USERID      = AH.APPLICANTID")
        sqlStat.AppendLine("   AND  US1.STYMD      <= AH.APPLYDATE")
        sqlStat.AppendLine("   AND  US1.ENDYMD     >= AH.APPLYDATE")
        sqlStat.AppendLine("   AND  US1.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US2")
        sqlStat.AppendLine("    ON  US2.USERID      = AH.APPROVERID")
        sqlStat.AppendLine("   AND  US2.STYMD      <= AH.APPROVEDATE")
        sqlStat.AppendLine("   AND  US2.ENDYMD     >= AH.APPROVEDATE")
        sqlStat.AppendLine("   AND  US2.DELFLG     <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD2")
        sqlStat.AppendLine("     ON PD2.PRODUCTCODE  = BS.TWOAGOPRODUCT")
        sqlStat.AppendLine("    AND PD2.DELFLG      <> @DELFLG")

        sqlStat.AppendLine(" WHERE BS.BRID     = @BRID ")
        sqlStat.AppendLine("   AND BS.BRBASEID = @BRBASEID ")
        sqlStat.AppendLine(" ORDER BY OB.BRID , DP.DEPOTCODE DESC , OV.ACTUALDATE DESC ")

        Try
            Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                With sqlCmd.Parameters
                    'SQLパラメータ設定
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brInfoOrganizer.BrId
                    .Add("@BRBASEID", SqlDbType.NVarChar, 20).Value = brInfoOrganizer.LinkId
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVCamp
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With
                If Me.hdnDelFlg.Value = CONST_FLAG_YES Then '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
                    Dim paramSubId As SqlParameter = sqlCmd.Parameters.Add("@SUBID", SqlDbType.NVarChar, 20)
                    Dim paramLinkId As SqlParameter = sqlCmd.Parameters.Add("@LINKID", SqlDbType.NVarChar, 20)
                    paramSubId.Value = Me.hdnSubId.Value
                    paramLinkId.Value = Me.hdnLinkId.Value
                End If
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Breaker base info Error")
                    End If
                    retDt = CreateOrganizerInfoTable()
                    For Each col As DataColumn In dt.Columns
                        retDt.Rows(0)(col.ColumnName) = Convert.ToString(dt.Rows(0)(col.ColumnName))
                    Next

                End Using
                retDt.Rows(0).Item("USETYPE") = dicBrInfo.Values(0).UseType
            End Using
            Return retDt
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

        Return retDt
    End Function
    ''' <summary>
    ''' GBT0003_BR_VALUEテーブルより費用情報取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetBreakerValue(dicBrInfo As Dictionary(Of String, BreakerInfo), Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = Nothing
        Dim sqlStat As New Text.StringBuilder
        Dim useType As String = dicBrInfo.First.Value.UseType
        Dim brType As String = dicBrInfo.First.Value.BrType
        Dim nameField As String = "NAMESJP"
        If BASEDLL.COA0019Session.LANGDISP <> C_LANG.JA Then
            nameField = "NAMES"
        End If
        sqlStat.AppendLine("SELECT VL.BRID ")
        sqlStat.AppendLine("      ,VL.BRVALUEID ")
        sqlStat.AppendLine("      ,VL.STYMD ")
        sqlStat.AppendLine("      ,VL.DTLPOLPOD")
        sqlStat.AppendLine("      ,VL.CONTRACTOR AS CONTRACTOR")
        sqlStat.AppendLine("      ,VL.CLASS2     AS CLASS2")
        sqlStat.AppendLine("      ,VL.COSTCODE ")
        sqlStat.AppendFormat("      ,CC.{0} AS COSTNAME ", nameField).AppendLine()
        sqlStat.AppendLine("      ,CC.CLASS4 ")
        sqlStat.AppendLine("      ,CC.CLASS8 ")
        sqlStat.AppendLine("      ,VL.BASEON ")
        sqlStat.AppendLine("      ,VL.TAX ")
        sqlStat.AppendLine("      ,VL.USD ")
        sqlStat.AppendLine("      ,VL.LOCAL ")
        sqlStat.AppendLine("      ,VL.CURRENCYCODE ")
        sqlStat.AppendLine("      ,VL.LOCALRATE ")
        sqlStat.AppendLine("      ,VL.USDRATE ")
        sqlStat.AppendLine("      ,VL.REMARK ")
        sqlStat.AppendLine("      ,CASE WHEN PT.COSTCODE Is NULL THEN '1' ELSE '0' END AS CAN_DELETE")
        sqlStat.AppendFormat("      ,CC.{0} AS ITEM1 ", nameField).AppendLine()
        sqlStat.AppendLine("      ,VL.REPAIRFLG")
        sqlStat.AppendLine("      ,VL.APPROVEDUSD")
        sqlStat.AppendLine("      ,VL.COUNTRYCODE")
        sqlStat.AppendLine("      ,VL.INVOICEDBY AS INVOICEDBY")
        sqlStat.AppendLine("  FROM GBT0003_BR_VALUE VL ")
        sqlStat.AppendLine("      LEFT JOIN GBM0010_CHARGECODE CC")
        sqlStat.AppendLine("        ON  VL.COSTCODE     = CC.COSTCODE ")
        sqlStat.AppendLine("       AND  CC.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CC.LDKBN        = 'B'")
        sqlStat.AppendLine("       AND  CC.STYMD       <= @STYMD")
        sqlStat.AppendLine("       AND  CC.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("       AND  CC.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0009_TRPATTERN PT")
        sqlStat.AppendLine("        ON  VL.COSTCODE     = PT.COSTCODE ")
        sqlStat.AppendLine("       AND  VL.DTLPOLPOD    = PT.AGENTKBN ")
        sqlStat.AppendLine("       AND  PT.ORG          = 'GB_Default' ")
        sqlStat.AppendLine("       AND  PT.BRTYPE       = @BRTYPE ")
        sqlStat.AppendLine("       AND  PT.USETYPE      = @USETYPE ")
        sqlStat.AppendLine("       AND  PT.STYMD       <= @STYMD")
        sqlStat.AppendLine("       AND  PT.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("       AND  PT.DELFLG      <> @DELFLG")
        sqlStat.AppendLine(" WHERE BRID      = @BRID ")
        sqlStat.AppendLine("   AND BRVALUEID = @BRVALUEID ")
        sqlStat.AppendLine(" ORDER BY VL.DTLPOLPOD,CASE WHEN PT.COSTCODE Is NULL THEN '1' ELSE '0' END,CASE WHEN PT.CLASS2 Is NULL OR PT.CLASS2 = '' THEN 0 ELSE CONVERT(int,PT.CLASS2) END,VL.COSTCODE ")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                retDt = CreateCostInfoTable()
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = useType
                    .Add("@BRTYPE", SqlDbType.NVarChar, 20).Value = brType
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With

                Dim paramBrId As SqlParameter = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar, 20)
                Dim paramBrValueId As SqlParameter = sqlCmd.Parameters.Add("@BRVALUEID", SqlDbType.NVarChar, 20)

                For Each brInfoItem As BreakerInfo In dicBrInfo.Values
                    '基本情報の紐づけ情報はスキップ
                    If brInfoItem.Type <> "POL1" Then
                        Continue For
                    End If
                    'SQLパラメータ値セット
                    paramBrId.Value = brInfoItem.BrId
                    paramBrValueId.Value = brInfoItem.LinkId


                    Using sqlDa As New SqlDataAdapter(sqlCmd)
                        Dim dt As New DataTable
                        sqlDa.Fill(dt)
                        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                            'Throw New Exception("Get Breaker value info Error")
                        End If
                        Dim dicCandeleteCode As New Dictionary(Of String, String)

                        For Each dr As DataRow In dt.Rows
                            Dim writeDr As DataRow
                            writeDr = retDt.NewRow
                            For Each col As DataColumn In dt.Columns
                                writeDr.Item(col.ColumnName) = Convert.ToString(dr.Item(col.ColumnName))
                            Next
                            If Convert.ToString(writeDr.Item("CAN_DELETE")) = "0" AndAlso dicCandeleteCode.ContainsKey(Convert.ToString(writeDr.Item("COSTCODE"))) Then
                                writeDr.Item("CAN_DELETE") = "1"
                            ElseIf Convert.ToString(writeDr.Item("CAN_DELETE")) = "0" Then
                                dicCandeleteCode.Add(Convert.ToString(writeDr.Item("COSTCODE")), "")
                            End If
                            retDt.Rows.Add(writeDr)
                        Next

                    End Using

                Next
            End Using
            Return retDt
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

        Return retDt
    End Function
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
        sqlStat.AppendLine("Select  'BV' ")
        sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
        sqlStat.AppendLine("      + '-'")
        sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR " & C_SQLSEQ.BREAKER & ")),4)")
        sqlStat.AppendLine("      + '-'")
        sqlStat.AppendLine("      + (SELECT VALUE1")
        sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
        sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
        sqlStat.AppendLine("            AND KEYCODE = @KEYCODE")
        sqlStat.AppendLine("            AND STYMD  <= @STYMD")
        sqlStat.AppendLine("            AND ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("            AND DELFLG <> @DELFLG)")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = "SERVERSEQ"
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVname
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With

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
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    Protected Sub UploadExcel()
        Dim ds As New DataSet
        Dim currentTab = GetCurrentTab()
        Dim dtlPolPod As String = ""
        dtlPolPod = "POL1"

        Dim dt As DataTable = Nothing

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        Dim reportMapId As String = ""

        '承認時はエラー
        If Me.hdnApprovalFlg.Value <> "0" Then '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
            CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPLOADAPPLYING, Me.lblFooterMessage)
            Return
        End If

        '画面費用を取得しデータテーブルに格納
        dt = CollectDisplayCostInfo(currentTab)
        reportMapId = CONST_MAPID

        Dim COA0029XlsTable As New BASEDLL.COA0029XlsTable

        Dim returnCode As String = C_MESSAGENO.NORMAL


        ''UPLOAD_XLSデータ取得
        COA0029XlsTable.MAPID = reportMapId
        COA0029XlsTable.SHEETNAME = reportId + "Out" 'インポートは別シート
        COA0029XlsTable.COA0029XlsToTable()
        If COA0029XlsTable.ERR = C_MESSAGENO.NORMAL Then
            If COA0029XlsTable.TBLDATA.Rows.Count = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage)
                Return
            End If
        Else
            returnCode = COA0029XlsTable.ERR
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
            Return
        End If

        'コスト情報の場合
        Dim inportCostList As New List(Of COSTITEM)
        Dim maxUniqueIndex As Integer = 0
        For Each dr As DataRow In COA0029XlsTable.TBLDATA.Rows
            '費用コードが空白の場合はそのまま終了
            If Convert.ToString(dr.Item("COSTCODE")).Trim = "" Then
                Continue For
            End If

            Dim costitem As New COSTITEM
            costitem.ItemGroup = currentTab
            costitem.CostCode = Convert.ToString(dr.Item("COSTCODE")).Trim
            costitem.USD = Convert.ToString(dr.Item("USD")).Trim
            costitem.Local = Convert.ToString(dr.Item("LOCAL")).Trim

            costitem.RepairFlg = "0"
            costitem.ApprovedUsd = "0.00"
            costitem.LocalCurrncy = Me.txtLocalCurrencyRef.Text
            costitem.LocalCurrncyRate = Me.txtLocalRateRef.Text
            costitem.Remarks = ""
            costitem.IsAddedCost = "1"

            Dim tmpNum As Decimal
            Dim countryCode As String = Nothing
            countryCode = Me.hdnCountryOrganizer.Value
            costitem.CountryCode = countryCode
            If Decimal.TryParse(costitem.USD, tmpNum) Then
                costitem.USD = NumberFormat(tmpNum, countryCode, "", "", "1")
            End If
            If Decimal.TryParse(costitem.Local, tmpNum) Then
                costitem.Local = NumberFormat(tmpNum, countryCode)
            End If
            If Decimal.TryParse(costitem.ApprovedUsd, tmpNum) Then
                costitem.ApprovedUsd = NumberFormat(tmpNum, countryCode, "", "", "1")
            End If

            Dim costDt As DataTable = GetCost(Convert.ToString(dr.Item("COSTCODE")).Trim)
            If costDt IsNot Nothing AndAlso costDt.Rows.Count > 0 Then
                costitem.Item1 = Convert.ToString(costDt.Rows(0).Item("NAME"))
                costitem.Class4 = Convert.ToString(costDt.Rows(0).Item("CLASS4"))
                costitem.Class8 = Convert.ToString(costDt.Rows(0).Item("CLASS8"))
            Else
                Continue For 'マスタにない費用コードはスキップ
            End If

            costitem.UniqueIndex = maxUniqueIndex
            costitem.Class2 = Convert.ToString(maxUniqueIndex)
            costitem.SortOrder = Convert.ToString(maxUniqueIndex)
            maxUniqueIndex = maxUniqueIndex + 1

            inportCostList.Add(costitem)
        Next

        ViewState(CONST_VS_NAME_COSTLIST) = inportCostList
        Dim showCostList = (From allCostItem In inportCostList
                            Where allCostItem.ItemGroup = currentTab
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        If returnCode = C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALUPLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        End If

        CalcSummaryCostLocal()
        CalcSummaryCostUsd()
        CalcSummaryCostAppUsd()

        '費用項目非活性制御
        CostEnabledControls()
        FileUppEnabledControls()

    End Sub
    ''' <summary>
    ''' ファイルアップロード入力処理(添付ファイル)
    ''' </summary>
    Protected Sub UploadFile()
        'カレントタブ取得
        Dim currentTab = GetCurrentTab(Nothing)

        If currentTab = COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        Dim dtAttachment As DataTable = Nothing

        '修理前ファイル
        If currentTab = COSTITEM.CostItemGroup.FileUp Then
            dtAttachment = Me.BeforeRepairAttachment
        ElseIf currentTab = COSTITEM.CostItemGroup.DoneFileUp Then
            dtAttachment = Me.AfterRepairAttachment
        End If
        Dim chkMsgNo = CommonFunctions.CheckUploadAttachmentFile(dtAttachment)
        If chkMsgNo <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(chkMsgNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        If currentTab = COSTITEM.CostItemGroup.FileUp Then

            dtAttachment = CommonFunctions.UploadAttachmentFile(dtAttachment, Me.lblBrNo.Text, CONST_MAPID, Before)

            Me.BeforeRepairAttachment = dtAttachment

            'Repeaterバインド(空明細)
            dViewRep.DataSource = dtAttachment
            dViewRep.DataBind()

        ElseIf currentTab = COSTITEM.CostItemGroup.DoneFileUp Then

            dtAttachment = CommonFunctions.UploadAttachmentFile(dtAttachment, Me.lblBrNo.Text, CONST_MAPID, After)

            Me.AfterRepairAttachment = dtAttachment

            'Repeaterバインド(空明細)
            dDoneViewRep.DataSource = dtAttachment
            dDoneViewRep.DataBind()

        End If

        'メッセージ編集
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALIMPORT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

    End Sub

    ''' <summary>
    ''' DetailFile内容表示（DetailFileダブルクリック時（内容照会））
    ''' </summary>
    Protected Sub FileDisplay()

        'カレントタブ取得
        Dim currentTab = GetCurrentTab(Nothing)

        Dim fileName As String = Me.hdnFileDisplay.Value
        If fileName = "" Then
            Return
        End If

        Dim dtAttachment As DataTable = Nothing

        If currentTab = COSTITEM.CostItemGroup.FileUp Then

            dtAttachment = BeforeRepairAttachment

        ElseIf currentTab = COSTITEM.CostItemGroup.DoneFileUp Then

            dtAttachment = AfterRepairAttachment

        End If

        Dim dlUrl As String = CommonFunctions.GetAttachfileDownloadUrl(dtAttachment, fileName)
        Me.hdnPrintURL.Value = dlUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

    End Sub
    ''' <summary>
    ''' グリッド表示用のコストアイテムクラス
    ''' </summary>
    <Serializable>
    Public Class COSTITEM
        Public Enum CostItemGroup As Integer
            ''' <summary>
            ''' Repair
            ''' </summary>
            Repair = 0
            ''' <summary>
            ''' FileUp
            ''' </summary>
            FileUp = 1
            ''' <summary>
            ''' DoneFileUp
            ''' </summary>
            DoneFileUp = 2
        End Enum
        ''' <summary>
        ''' どのコストに属するか（リペア)
        ''' </summary>
        ''' <returns></returns>
        Public Property ItemGroup As CostItemGroup = 0
        ''' <summary>
        ''' 費用コード
        ''' </summary>
        ''' <returns></returns>
        Public Property CostCode As String = ""
        ''' <summary>
        ''' USD金額
        ''' </summary>
        ''' <returns></returns>
        Public Property USD As String = ""
        ''' <summary>
        ''' 現地金額
        ''' </summary>
        ''' <returns></returns>
        Public Property Local As String = ""
        ''' <summary>
        ''' 通貨コード
        ''' </summary>
        ''' <returns></returns>
        Public Property LocalCurrncy As String = ""
        ''' <summary>
        ''' 通貨レート
        ''' </summary>
        ''' <returns></returns>
        Public Property LocalCurrncyRate As String = ""
        ''' <summary>
        ''' 初見
        ''' </summary>
        ''' <returns></returns>
        Public Property Remarks As String = ""
        ''' <summary>
        ''' 発生区分
        ''' </summary>
        ''' <returns></returns>
        Public Property Class4 As String = ""
        ''' <summary>
        ''' US$入力
        ''' </summary>
        ''' <returns></returns>
        Public Property Class8 As String = ""
        ''' <summary>
        ''' マスタのソート順
        ''' </summary>
        ''' <returns></returns>
        Public Property SortOrder As String = ""
        ''' <summary>
        ''' 定形外の追加した費用か(0:定型,1:追加)
        ''' </summary>
        ''' <returns>削除ボタンの表示非表示に利用</returns>
        Public Property IsAddedCost As String = "0"
        ''' <summary>
        ''' 一意キーを格納（画面での削除を制御）
        ''' </summary>
        ''' <returns></returns>
        Public Property UniqueIndex As Integer = 0
        ''' <summary>
        ''' 輸送パターンソート順
        ''' </summary>
        ''' <returns></returns>
        Public Property Class2 As String = ""
        ''' <summary>
        ''' ITEM1
        ''' </summary>
        ''' <returns></returns>
        Public Property Item1 As String = ""
        ''' <summary>
        ''' ITEM2
        ''' </summary>
        ''' <returns></returns>
        Public Property Item2 As String = ""
        ''' <summary>
        ''' REPAIRFLG
        ''' </summary>
        ''' <returns></returns>
        Public Property RepairFlg As String = ""
        ''' <summary>
        ''' ApprovedUsd
        ''' </summary>
        ''' <returns></returns>
        Public Property ApprovedUsd As String = ""
        ''' <summary>
        ''' CountryCode
        ''' </summary>
        ''' <returns></returns>
        Public Property CountryCode As String = ""
        ''' <summary>
        ''' InvoicedBy
        ''' </summary>
        ''' <returns></returns>
        Public Property InvoicedBy As String = ""

        ''' <summary>
        ''' 全プロパティー値比較
        ''' </summary>
        ''' <param name="containsItem"></param>
        ''' <returns></returns>
        Public Function AllPropertyEquals(containsItem As COSTITEM) As Boolean
            Dim t As Type = GetType(COSTITEM)
            For Each prop In t.GetProperties()
                If Not prop.GetValue(containsItem).Equals(prop.GetValue(Me)) Then
                    Return False
                End If
            Next
            Return True
        End Function

    End Class
    ''' <summary>
    ''' グリッド表示用のコストアイテムクラス
    ''' </summary>
    <Serializable>
    Public Class BreakerInfo
        ''' <summary>
        ''' ブレーカーID
        ''' </summary>
        ''' <returns></returns>
        Public Property BrId As String = ""
        ''' <summary>
        ''' サブID(BRID-枝番)
        ''' </summary>
        ''' <returns></returns>
        Public Property SubId As String = ""
        ''' <summary>
        ''' 種別(POL1,POD1等)
        ''' </summary>
        ''' <returns></returns>
        Public Property Type As String = ""
        ''' <summary>
        ''' 個別ID
        ''' </summary>
        ''' <returns></returns>
        Public Property LinkId As String = ""
        ''' <summary>
        ''' 開始年月日
        ''' </summary>
        ''' <returns></returns>
        Public Property Stymd As String = ""
        ''' <summary>
        ''' ブレーカータイプ
        ''' </summary>
        ''' <returns></returns>
        Public Property BrType As String = ""
        ''' <summary>
        ''' 申請ID
        ''' </summary>
        ''' <returns></returns>
        Public Property ApplyId As String = ""
        ''' <summary>
        ''' 最終承認STEP
        ''' </summary>
        ''' <returns></returns>
        Public Property LastStep As String = ""
        ''' <summary>
        ''' 輸送パターン
        ''' </summary>
        ''' <returns></returns>
        Public Property UseType As String = ""
        ''' <summary>
        ''' 備考
        ''' </summary>
        ''' <returns></returns>
        Public Property Remark As String = ""
        ''' <summary>
        ''' タイムスタンプ
        ''' </summary>
        ''' <returns></returns>
        Public Property TimeStamp As String = ""
        ''' <summary>
        ''' 更新年月日
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdYmd As String = ""
        ''' <summary>
        ''' 更新ユーザID
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdUser As String = ""
        ''' <summary>
        ''' 更新端末
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdTermId As String = ""
    End Class

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
    ''' <summary>
    ''' Int文字列を数字に変換
    ''' </summary>
    ''' <param name="intString"></param>
    ''' <returns></returns>
    Private Function IntStringToInt(intString As String) As Integer
        Dim tmpInt As Integer = 0
        If Integer.TryParse(intString.Replace(",", ""), tmpInt) Then
            Return tmpInt
        Else
            Return 0
        End If
    End Function
    ''' <summary>
    ''' Decimal文字列を数字に変換
    ''' </summary>
    ''' <param name="dblString"></param>
    ''' <returns></returns>
    Private Function DecimalStringToDecimal(dblString As String) As Decimal
        Dim tmpDouble As Decimal = 0
        If Decimal.TryParse(dblString, tmpDouble) Then
            Return tmpDouble
        Else
            Return 0
        End If
    End Function

    ''' <summary>
    ''' 費用タブローカルコスト変更時
    ''' </summary>
    Public Sub CalcSummaryCostLocal(Optional initFlg As String = "")
        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab <> COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        Dim costData As List(Of COSTITEM) = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab AndAlso costItemRow.Local.Trim <> "" AndAlso IsNumeric(costItemRow.Local)).ToList

        'レート桁、端数制御取得
        Dim countryCode As String = Nothing
        Select Case currentTab
            Case COSTITEM.CostItemGroup.Repair
                countryCode = Me.hdnCountryOrg.Value
        End Select

        '桁数取得
        Dim dt As DataTable = Nothing
        Dim GBA00008Country As New GBA00008Country
        GBA00008Country.COUNTRYCODE = countryCode
        GBA00008Country.getCountryInfo()
        If GBA00008Country.ERR = C_MESSAGENO.NORMAL Then
            dt = GBA00008Country.COUNTRY_TABLE
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00008Country.ERR)})
            Return
        End If

        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)

        For Each item In targetCostData
            If item.Local <> "" AndAlso IsNumeric(item.Local) Then
                item.Local = NumberFormat(DecimalStringToDecimal(item.Local), countryCode)

                'USD計算
                If item.LocalCurrncyRate <> "" AndAlso IsNumeric(item.LocalCurrncyRate) Then

                    If CDec(item.Local) <> 0 Then
                        If CDec(item.LocalCurrncyRate) <> 0 Then
                            item.USD = Convert.ToString((Decimal.Parse(item.Local) / Decimal.Parse(item.LocalCurrncyRate)))
                        Else
                            item.USD = "0"
                        End If
                    End If

                    If item.USD = "" Then
                        item.USD = "0"
                    End If

                    Select Case Convert.ToString(dr.Item("ROUNDFLG"))
                        Case "U"
                            item.LocalCurrncyRate = Convert.ToString(RoundUp(Decimal.Parse(item.LocalCurrncyRate), CUInt(dr.Item("RATEDECIMALPLACES"))))
                        Case "D"
                            item.LocalCurrncyRate = Convert.ToString(RoundDown(Decimal.Parse(item.LocalCurrncyRate), CInt(dr.Item("RATEDECIMALPLACES"))))
                        Case "R"
                            item.LocalCurrncyRate = Convert.ToString(Round(Decimal.Parse(item.LocalCurrncyRate), CUInt(dr.Item("RATEDECIMALPLACES"))))
                    End Select
                    If IsNumeric(item.USD) = False Then
                        Continue For
                    End If
                    Dim decPlace As Integer = 0
                    Dim roundFlg As String = ""
                    If GetDecimalPlaces(decPlace, roundFlg) Then
                        Select Case roundFlg
                            Case "U"
                                item.USD = Convert.ToString(RoundUp(Decimal.Parse(item.USD), CUInt(decPlace)))
                            Case "D"
                                item.USD = Convert.ToString(RoundDown(Decimal.Parse(item.USD), decPlace))
                            Case "R"
                                item.USD = Convert.ToString(Round(Decimal.Parse(item.USD), CUInt(decPlace)))
                        End Select

                    End If

                    item.LocalCurrncyRate = NumberFormat(DecimalStringToDecimal(item.LocalCurrncyRate), countryCode, "", "1")

                    item.USD = NumberFormat(DecimalStringToDecimal(item.USD), countryCode, "", "", "1").ToString

                    Me.txtLocalRateRef.Text = item.LocalCurrncyRate

                End If
            End If
        Next
        '絞り込んだリストを集計
        Dim summaryUsd As Decimal = targetCostData.Sum(Function(item) Decimal.Parse(If(IsNumeric(item.USD), item.USD, "0")))

        '合計欄に値表示
        Me.iptEstimatedSummary.Value = NumberFormat(summaryUsd, countryCode, "", "", "1")

        Me.hdnCurrentUnieuqIndex.Value = ""

        ViewState(CONST_VS_NAME_COSTLIST) = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.ItemGroup = currentTab
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub
    ''' <summary>
    ''' USDコスト変更時
    ''' </summary>
    Public Sub CalcSummaryCostUsd(Optional initFlg As String = "")
        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab <> COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        'レート桁、端数制御取得
        Dim countryCode As String = Me.hdnCountryOrg.Value

        Dim costData As List(Of COSTITEM) = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab AndAlso costItemRow.USD.Trim <> "" AndAlso IsNumeric(costItemRow.USD)).ToList
        '数値のカンマ編集
        For Each item In targetCostData
            If item.USD <> "" AndAlso IsNumeric(item.USD) Then
                item.USD = NumberFormat(DecimalStringToDecimal(item.USD), countryCode, "", "", "1")
            End If
        Next
        '絞り込んだリストを集計
        Dim summary As Decimal = targetCostData.Sum(Function(item) Decimal.Parse(item.USD))
        '合計欄に値表示
        Me.iptEstimatedSummary.Value = NumberFormat(summary, countryCode, "", "", "1")
        Me.hdnCurrentUnieuqIndex.Value = ""

        ViewState(CONST_VS_NAME_COSTLIST) = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.ItemGroup = currentTab
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub
    ''' <summary>
    ''' 承認USDコスト変更時
    ''' </summary>
    Public Sub CalcSummaryCostAppUsd(Optional initFlg As String = "")
        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab <> COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        'レート桁、端数制御取得
        Dim countryCode As String = Me.hdnCountryOrg.Value

        Dim costData As List(Of COSTITEM) = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab AndAlso costItemRow.ApprovedUsd.Trim <> "" AndAlso IsNumeric(costItemRow.ApprovedUsd)).ToList
        '数値のカンマ編集
        For Each item In targetCostData
            If item.ApprovedUsd <> "" AndAlso IsNumeric(item.ApprovedUsd) Then
                item.ApprovedUsd = NumberFormat(DecimalStringToDecimal(item.ApprovedUsd), countryCode, "", "", "1")
            End If
        Next

        Dim targetSumCostData = (From costItemRow In costData
                                 Where costItemRow.ItemGroup = currentTab AndAlso costItemRow.ApprovedUsd.Trim <> "" AndAlso IsNumeric(costItemRow.ApprovedUsd) AndAlso costItemRow.RepairFlg = "1").ToList

        '絞り込んだリストを集計
        Dim summary As Decimal = targetSumCostData.Sum(Function(item) Decimal.Parse(item.ApprovedUsd))
        '合計欄に値表示
        Me.iptApprovedSummary.Value = NumberFormat(summary, countryCode, "", "", "1")
        Me.hdnCurrentUnieuqIndex.Value = ""

        ViewState(CONST_VS_NAME_COSTLIST) = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.ItemGroup = currentTab
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub
    ''' <summary>
    ''' 費用項目活性制御
    ''' </summary>
    Public Sub CostEnabledControls()
        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab <> COSTITEM.CostItemGroup.Repair Then
            Return
        End If
        '画面の入力値をクラスに配置
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab).ToList

        For i As Integer = 0 To targetCostData.Count - 1
            Dim costItem = targetCostData.Item(i)
            Dim gridRow = Me.gvDetailInfo.Rows(i)
            'Item1セル
            Dim item1Cell = gridRow.Cells(2)
            item1Cell.Attributes.Add("title", HttpUtility.HtmlDecode(item1Cell.Text))
            'LOCAL入力セル
            Dim localCostCell = gridRow.Cells(4)
            'USD入力セル
            Dim usdCostCell = gridRow.Cells(5)
            'RepairCheckBox 
            Dim repairChk As CheckBox = DirectCast(gridRow.Cells(6).Controls(1), CheckBox)

            'LOCAL入力制御
            If costItem.Class8 = "1" Then
                localCostCell.Enabled = False
            Else
                localCostCell.Enabled = True
            End If

            'USD制御
            If costItem.Local <> "" AndAlso IsNumeric(costItem.Local) AndAlso CDec(costItem.Local) <> 0 Then
                usdCostCell.Enabled = False
            Else
                usdCostCell.Enabled = True
            End If

            'チェックボックス制御
            If costItem.RepairFlg = "1" Then
                repairChk.Checked = True
            Else
                repairChk.Checked = False
            End If

            '申請
            If Me.hdnApprovalFlg.Value <> "0" Then '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
                '承認
                localCostCell.Enabled = False
                usdCostCell.Enabled = False
            End If

            '参照のみ
            If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
                localCostCell.Enabled = False
                usdCostCell.Enabled = False
                repairChk.Checked = False
                localCostCell.Enabled = False
                usdCostCell.Enabled = False
            End If

        Next

    End Sub

    ''' <summary>
    ''' ファイルアップロードタブ活性制御
    ''' </summary>
    Public Sub FileUppEnabledControls()

        'If Me.hdnAlreadyFlg.Value = "1" Then '承認済フラグ(承認画面の遷移時のみ設定 1:承認済)
        If Me.hdnAlreadyFlg.Value = "1" OrElse Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then '承認済フラグ(承認画面の遷移時のみ設定 1:承認済)

            For i As Integer = 0 To dViewRep.Items.Count - 1

                DirectCast(dViewRep.Items(i).FindControl("txtRepDelFlg"), System.Web.UI.WebControls.TextBox).Enabled = False

            Next

        End If

    End Sub
    ''' <summary>
    ''' 切り捨て関数
    ''' </summary>
    ''' <param name="value">値</param>
    ''' <param name="digits">IN：省略可能 省略時はセッション変数の対象桁数を取得</param>
    ''' <returns></returns>
    Private Function RoundDown(value As Decimal, Optional digits As Integer = Integer.MinValue) As Decimal

        If digits = Integer.MinValue Then
            Dim decPlace As Integer = 0
            Dim round As String = ""
            'digits = 2 'セッション変数の桁数
            If GetDecimalPlaces(decPlace, round) Then
                digits = decPlace
            Else
                digits = 0
            End If
        End If
        Dim coef As Decimal = Convert.ToDecimal(System.Math.Pow(10, digits))
        If value > 0 Then
            Return System.Math.Floor(value * coef) / coef
        Else
            Return System.Math.Ceiling(value * coef) / coef
        End If
    End Function
    ''' <summary>
    ''' 書式変更関数
    ''' </summary>
    ''' <param name="value">書式を変更</param>
    ''' <param name="formatString">個別の書式がある場合は指定、未指定の場合はセッション変数の有効桁に従い小数表示を生成</param>
    ''' <returns></returns>
    Private Function NumberFormat(value As Object, countryCode As String, Optional formatString As String = "", Optional rateDec As String = "", Optional usdFlg As String = "") As String
        Dim strValue As String = Convert.ToString(value)
        strValue = strValue.Trim
        '渡された項目がブランクの場合はブランクのまま返却
        If strValue = "" Then
            Return ""
        End If

        Dim decValue As Decimal
        '渡された項目が数字にならない場合は引数のまま返却
        If Decimal.TryParse(strValue, decValue) = False Then
            Return strValue
        End If
        '数値書式の生成
        Dim retFormatString As String = formatString
        If formatString = "" Then

            Dim digits As Integer = 0
            If usdFlg = "" Then

                '桁数取得
                Dim dt As DataTable = Nothing
                Dim GBA00008Country As New GBA00008Country
                GBA00008Country.COUNTRYCODE = countryCode
                GBA00008Country.getCountryInfo()
                If GBA00008Country.ERR = C_MESSAGENO.NORMAL Then
                    dt = GBA00008Country.COUNTRY_TABLE
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00008Country.ERR)})
                    Return ""
                End If

                If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                    Return ""
                End If
                Dim dr As DataRow = dt.Rows(0)

                If rateDec = "" Then
                    digits = CInt(dr.Item("DECIMALPLACES"))
                Else
                    digits = CInt(dr.Item("RATEDECIMALPLACES"))
                End If

            Else
                'USD桁数取得
                Dim decPlace As Integer = 0
                Dim round As String = ""
                If GetDecimalPlaces(decPlace, round) Then
                    digits = decPlace
                Else
                    digits = 0
                End If
            End If

            If digits <= 0 Then
                retFormatString = "#,##0"
            Else
                retFormatString = "#,##0." & New String("0"c, digits)
            End If
        End If
        Return decValue.ToString(retFormatString)
    End Function
    ''' <summary>
    ''' タンク番号変更時
    ''' </summary>
    Public Sub txtTankNo_Change()

        Try

            SetTankNoListItem(Me.chkLeaseCheck.Checked)
            SetDisplayTankNo(Me.txtTankNo.Text)

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try

    End Sub
    ''' <summary>
    ''' タンク使用法変更時
    ''' </summary>
    Public Sub txtTankUsage_Change()

        Try
            Me.lblTankUsageText.Text = ""

            SetTankUsageListItem()
            If Me.lbTankUsage.Items.Count > 0 Then
                Dim findListItem = Me.lbTankUsage.Items.FindByValue(Me.txtTankUsage.Text)
                If findListItem IsNot Nothing Then
                    Me.lblTankUsageText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbTankUsage.Items.FindByValue(Me.txtTankUsage.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblTankUsageText.Text = findListItemUpper.Text
                        Me.txtTankUsage.Text = findListItemUpper.Value
                    End If
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
    ''' 削除フラグ変更時
    ''' </summary>
    Public Sub txtDeleteFlag_Change()

        Try
            Me.lblDeleteFlagText.Text = ""

            SetDelFlgListItem()
            If Me.lbTankUsage.Items.Count > 0 Then
                Dim findListItem = Me.lbDelFlg.Items.FindByValue(Me.txtDeleteFlag.Text)
                If findListItem IsNot Nothing Then
                    Me.lblDeleteFlagText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbDelFlg.Items.FindByValue(Me.txtDeleteFlag.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblDeleteFlagText.Text = findListItemUpper.Text
                        Me.txtDeleteFlag.Text = findListItemUpper.Value
                    End If
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
    ''' デポコード変更時
    ''' </summary>
    Public Sub txtDepoCode_Change()

        Try
            Me.lblDepoCodeText.Text = ""

            Dim dt As DataTable = GetDepot()
            With Me.lbDepot
                .DataSource = dt
                .DataTextField = "LISTBOXNAME"
                .DataValueField = "CODE"
                .DataBind()
                .Focus()
            End With

            If Me.lbDepot.Items.Count > 0 Then
                Dim findListItem = Me.lbDepot.Items.FindByValue(Me.txtDepoCode.Text)
                If findListItem IsNot Nothing Then

                    If findListItem.Text.Contains(":") Then
                        Dim parts As String()
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblDepoCodeText.Text = parts(1)
                    Else
                        Me.lblDepoCodeText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbDepot.Items.FindByValue(Me.txtDepoCode.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then

                        If findListItemUpper.Text.Contains(":") Then
                            Dim parts As String()
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblDepoCodeText.Text = parts(1)
                        Else
                            Me.lblDepoCodeText.Text = findListItemUpper.Text
                        End If
                        Me.txtDepoCode.Text = findListItemUpper.Value

                    End If
                End If

                Dim findResult = (From item In dt
                                  Where Convert.ToString(item("CODE")) = Me.txtDepoCode.Text).FirstOrDefault

                If findResult IsNot Nothing Then
                    Me.txtLocation.Text = findResult.Item("LOCATION").ToString
                Else
                    Me.txtLocation.Text = ""
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
    ''' 最終積載品コード変更時
    ''' </summary>
    Public Sub txtLastProduct_Change()

        Try
            Me.lblLastProductText.Text = ""

            Dim dt As DataTable = GetProduct()
            With Me.lbProduct
                .DataSource = dt
                .DataTextField = "LISTBOXNAME"
                .DataValueField = "CODE"
                .DataBind()
                .Focus()
            End With

            If Me.lbProduct.Items.Count > 0 Then
                Dim findListItem = Me.lbProduct.Items.FindByValue(Me.txtLastProduct.Text)
                If findListItem IsNot Nothing Then

                    If findListItem.Text.Contains(":") Then
                        Dim parts As String()
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblLastProductText.Text = parts(1)
                    Else
                        Me.lblLastProductText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbProduct.Items.FindByValue(Me.txtLastProduct.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then

                        If findListItemUpper.Text.Contains(":") Then
                            Dim parts As String()
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblLastProductText.Text = parts(1)
                        Else
                            Me.lblLastProductText.Text = findListItemUpper.Text
                        End If
                        Me.txtLastProduct.Text = findListItemUpper.Value

                    End If
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
    ''' 前2積載品コード変更時
    ''' </summary>
    Public Sub txtTwoAgoProduct_Change()

        Try
            Me.lblTwoAgoProductText.Text = ""

            Dim dt As DataTable = GetProduct()
            With Me.lbProduct
                .DataSource = dt
                .DataTextField = "LISTBOXNAME"
                .DataValueField = "CODE"
                .DataBind()
                .Focus()
            End With

            If Me.lbProduct.Items.Count > 0 Then
                Dim findListItem = Me.lbProduct.Items.FindByValue(Me.txtTwoAgoProduct.Text)
                If findListItem IsNot Nothing Then

                    If findListItem.Text.Contains(":") Then
                        Dim parts As String()
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblTwoAgoProductText.Text = parts(1)
                    Else
                        Me.lblTwoAgoProductText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbProduct.Items.FindByValue(Me.txtTwoAgoProduct.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then

                        If findListItemUpper.Text.Contains(":") Then
                            Dim parts As String()
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblTwoAgoProductText.Text = parts(1)
                        Else
                            Me.lblTwoAgoProductText.Text = findListItemUpper.Text
                        End If
                        Me.txtTwoAgoProduct.Text = findListItemUpper.Value

                    End If
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
    ''' 切り上げ
    ''' </summary>
    ''' <param name="value">対象の数値</param>
    ''' <param name="decimalPlaces">有効小数桁数</param>
    ''' <returns>切り上げした数値</returns>
    Public Shared Function RoundUp(ByVal value As Decimal, ByVal decimalPlaces As UInt32) As Decimal
        Dim rate As Decimal = CDec(Math.Pow(10.0R, decimalPlaces))

        If value < 0 Then
            Return (Math.Ceiling(value * -1D * rate) / rate) * -1D
        Else
            Return Math.Ceiling(value * rate) / rate
        End If
    End Function

    ''' <summary>
    ''' 四捨五入
    ''' </summary>
    ''' <param name="value">対象の数値</param>
    ''' <param name="decimalPlaces">有効小数桁数</param>
    ''' <returns>四捨五入した数値</returns>
    Public Shared Function Round(ByVal value As Decimal, ByVal decimalPlaces As UInt32) As Decimal
        Return Math.Round(value, CInt(decimalPlaces), MidpointRounding.AwayFromZero)
    End Function
    ''' <summary>
    ''' Demurrage取得
    ''' </summary>
    Public Function GetDemurrageList() As List(Of String)
        Dim retList As List(Of String) = New List(Of String)

        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "BREAKEREXCLUSION"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            For i As Integer = 0 To COA0017FixValue.VALUE1.Items.Count - 1
                retList.Add(COA0017FixValue.VALUE1.Items.Item(i).ToString)
            Next
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If

        Return retList

    End Function

    ''' <summary>
    ''' USD桁数取得
    ''' </summary>
    Public Function GetDecimalPlaces(ByRef retDecPlace As Integer, ByRef retRound As String) As Boolean
        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DECIMALPLACES"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            retDecPlace = CInt(COA0017FixValue.VALUE1.Items(0).ToString)
            retRound = COA0017FixValue.VALUE2.Items(0).ToString
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
            Return False
        End If

        Return True

    End Function
    ''' <summary>
    ''' 削除フラグリストアイテムを設定
    ''' </summary>
    Private Sub SetDelFlgListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbDelFlg.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DELFLG"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbDelFlg
        Else
            COA0017FixValue.LISTBOX2 = Me.lbDelFlg
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbDelFlg = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbDelFlg = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

        Else

            Return
        End If

    End Sub
    ''' <summary>
    ''' タンク番号リストアイテムを設定
    ''' </summary>
    Private Sub SetTankNoListItem(ByVal leaseFlg As Boolean)
        Dim GBA00012TankInfo As New GBA00012TankInfo
        Dim dtDbResult As DataTable = Nothing

        Try

            'リストクリア
            Me.lbTankNo.Items.Clear()

            'Leaseの場合
            If leaseFlg Then

                GBA00012TankInfo.REPFLG = "1"
                GBA00012TankInfo.ISALLOCATEONLY = 3
                GBA00012TankInfo.GBA00012getTankStatusTable()
                If Not {C_MESSAGENO.NORMAL, C_MESSAGENO.NODATA}.Contains(GBA00012TankInfo.ERR) Then
                    'CommonFunctions.ShowMessage(GBA00012TankInfo.ERR, Me.lblFooterMessage)
                    Return
                End If
                dtDbResult = GBA00012TankInfo.TANKSTATUS_TABLE

                For Each dr As DataRow In dtDbResult.Rows
                    Me.lbTankNo.Items.Add(Convert.ToString(dr("TANKNO")))
                Next

            Else

                GBA00012TankInfo.LISTBOX_TANK = Me.lbTankNo
                GBA00012TankInfo.GBA00012getLeftListTank()
                If GBA00012TankInfo.ERR = C_MESSAGENO.NORMAL Then
                    Me.lbTankNo = DirectCast(GBA00012TankInfo.LISTBOX_TANK, ListBox)
                Else
                    Return
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
    ''' タンク使用法リストアイテムを設定
    ''' </summary>
    Private Sub SetTankUsageListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbTankUsage.Items.Clear()

        'タンク使用法ListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "USAGE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbTankUsage
        Else
            COA0017FixValue.LISTBOX2 = Me.lbTankUsage
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbTankUsage = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbTankUsage = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

        Else

            Return
        End If

    End Sub
    ''' <summary>
    ''' 一括チェック押下処理
    ''' </summary>
    Private Sub BulkCheck()

        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab <> COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        Dim costData As List(Of COSTITEM) = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab AndAlso costItemRow.Local.Trim <> "" AndAlso IsNumeric(costItemRow.Local)).ToList

        'レート桁、端数制御取得
        Dim countryCode As String = Me.hdnCountryOrg.Value

        For Each item In targetCostData
            item.RepairFlg = "1"
            item.ApprovedUsd = item.USD
        Next
        '絞り込んだリストを集計
        Dim summaryUsd As Decimal = targetCostData.Sum(Function(item) Decimal.Parse(item.USD))
        Dim summaryAppUsd As Decimal = targetCostData.Sum(Function(item) Decimal.Parse(item.ApprovedUsd))

        '合計欄に値表示
        Me.iptEstimatedSummary.Value = NumberFormat(summaryUsd, countryCode, "", "", "1")
        Me.iptApprovedSummary.Value = NumberFormat(summaryAppUsd, countryCode, "", "", "1")

        ViewState(CONST_VS_NAME_COSTLIST) = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.ItemGroup = currentTab
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub
    ''' <summary>
    ''' Leaseチェック押下処理
    ''' </summary>
    Private Sub LeaseCheck()

        'タンクリスト取得
        SetTankNoListItem(Me.chkLeaseCheck.Checked)
        '2019/07/02 ↓リースタンクではなくてもデポなどの項目を手入力可能としたため追加
        Dim enabledCont As Boolean = True
        If Me.hdnApprovalFlg.Value = "1" Then '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
            enabledCont = False
        End If
        ' 参照のみ
        If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
            enabledCont = False
        End If
        Me.txtDepoInDate.Enabled = enabledCont
        Me.txtDepoCode.Enabled = enabledCont
        'Me.txtLocation.Enabled = True
        Me.txtBreakerNo.Enabled = enabledCont
        Me.txtLastOrderNo.Enabled = enabledCont
        Me.txtLastProduct.Enabled = enabledCont
        Me.txtTwoAgoProduct.Enabled = enabledCont

        Me.lblDepoCode.CssClass = "requiredMark2"
        Me.lblDepoCode.Font.Underline = enabledCont
        Me.lblDepoInDate.Font.Underline = enabledCont
    End Sub
    ''' <summary>
    ''' 費用チェック押下処理
    ''' </summary>
    Private Sub CheckApp()

        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab <> COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        Dim costData As List(Of COSTITEM) = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))

        Dim uniqueIndex As Integer = 0
        Integer.TryParse(Me.hdnCheckUniqueNumber.Value, uniqueIndex)
        Dim targetCostRow = (From costItemRow In costData
                             Where costItemRow.ItemGroup = currentTab AndAlso costItemRow.UniqueIndex = uniqueIndex).ToList

        If targetCostRow IsNot Nothing AndAlso targetCostRow.Count > 0 Then
            If Me.hdnCheckAppChange.Value.ToUpper = "TRUE" Then
                targetCostRow(0).ApprovedUsd = targetCostRow(0).USD
                targetCostRow(0).RepairFlg = "1"
            Else
                targetCostRow(0).ApprovedUsd = "0.00"
                targetCostRow(0).RepairFlg = "0"
            End If
        End If

        Me.hdnCurrentUnieuqIndex.Value = ""

        ViewState(CONST_VS_NAME_COSTLIST) = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.ItemGroup = currentTab
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '合計算出
        CalcSummaryCostAppUsd()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub
    ''' <summary>
    ''' RepairFlag保持
    ''' </summary>
    Private Sub RepairFlagRetention(ByVal tab As COSTITEM.CostItemGroup)

        If tab <> COSTITEM.CostItemGroup.Repair Then
            Return
        End If

        Dim costData As List(Of COSTITEM) = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = tab AndAlso costItemRow.Local.Trim <> "" AndAlso IsNumeric(costItemRow.Local)).ToList

        ViewState(CONST_VS_NAME_COSTLIST) = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.ItemGroup = tab
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

    End Sub
    ''' <summary>
    ''' 申請ボタン押下時
    ''' </summary>
    Public Sub btnApply_Click()

        TextChangeCheck()
        hdnMsgboxAppChangeFlg.Value = "1"

        If hdnMsgboxShowFlg.Value = "0" Then
            '申請処理
            applyProc()
            hdnMsgboxAppChangeFlg.Value = ""
        End If

    End Sub
    ''' <summary>
    '''申請YESボタン押下時
    ''' </summary>
    Public Sub btnApplyMsgYes_Click()

        '保存処理
        saveProc()

        If Not hdnMsgId.Value = C_MESSAGENO.NORMALDBENTRY Then
            Return
        End If

        '申請処理
        applyProc()

    End Sub
    ''' <summary>
    ''' 申請NOボタン押下時
    ''' </summary>
    Public Sub btnApplyMsgNo_Click()

        '申請処理
        applyProc()

    End Sub

    ''' <summary>
    ''' 申請処理
    ''' </summary>
    Public Sub applyProc()
        Dim COA0021ListTable As New COA0021ListTable
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval

        Dim procDateTime As DateTime = DateTime.Now
        Dim applyId As String = Nothing
        Dim lastStep As String = Nothing

        Dim brInfoPrev As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        brInfoPrev = DirectCast(ViewState(CONST_VS_NAME_DICBRINFO), Dictionary(Of String, BreakerInfo))
        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing
        brInfo = GetBreakerInfo(brInfoPrev("INFO").BrId)
        '申請ID取得
        Dim GBA00002MasterApplyID As New GBA00002MasterApplyID
        GBA00002MasterApplyID.COMPCODE = COA0019Session.APSRVCamp
        GBA00002MasterApplyID.SYSCODE = C_SYSCODE_GB
        GBA00002MasterApplyID.KEYCODE = COA0019Session.APSRVname
        GBA00002MasterApplyID.MAPID = CONST_MAPID
        GBA00002MasterApplyID.EVENTCODE = C_BRREVENT.APPLY
        GBA00002MasterApplyID.SUBCODE = ""
        GBA00002MasterApplyID.COA0032getgApplyID()
        If GBA00002MasterApplyID.ERR = C_MESSAGENO.NORMAL Then
            applyId = GBA00002MasterApplyID.APPLYID
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00002MasterApplyID.ERR)})
            Return
        End If

        Dim subCode As String = Me.txtSettlementOffice.Text

        '申請登録
        COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
        COA0032Apploval.I_APPLYID = applyId
        COA0032Apploval.I_MAPID = CONST_MAPID
        COA0032Apploval.I_EVENTCODE = C_BRREVENT.APPLY
        COA0032Apploval.I_SUBCODE = subCode
        COA0032Apploval.COA0032setApply()
        If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
            lastStep = COA0032Apploval.O_LASTSTEP
        Else
            CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage)
            Return
        End If

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
                .Add("@BRID", SqlDbType.NVarChar, 20).Value = Me.lblBrNo.Text
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
        'GBA00009MailSendSet.MAILSUBCODE = subCode
        GBA00009MailSendSet.MAILSUBCODE = ""
        GBA00009MailSendSet.BRID = Me.lblBrNo.Text
        GBA00009MailSendSet.BRSUBID = brInfo("INFO").SubId
        GBA00009MailSendSet.BRBASEID = brInfo("INFO").LinkId
        GBA00009MailSendSet.BRROUND = ""
        GBA00009MailSendSet.APPLYID = applyId
        GBA00009MailSendSet.LASTSTEP = lastStep
        GBA00009MailSendSet.GBA00009setMailToRepBR()
        If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
            Return
        End If

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.APPLYSUCCESS

        Me.hdnStatus.Value = C_APP_STATUS.APPLYING

        Dim thisPageUrl As String = Request.Url.ToString
        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
        Server.Transfer(Request.Url.LocalPath)

    End Sub

    ''' <summary>
    ''' 承認ボタン押下時
    ''' </summary>
    Public Sub btnApproval_Click()
        '変更チェック
        TextChangeCheck()
        If Me.hdnMsgboxShowFlg.Value = "1" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.HASNOSAVEITEMS, Me.lblFooterMessage, naeiw:=C_NAEIW.INFORMATION, pageObject:=Me)
            Return
        End If
        '費用項目チェックボックスチェック済
        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))
        Dim qCheckedList = From item In allCostList Where item.RepairFlg = "1"
        If qCheckedList.Any = False Then
            '費用にチェックが無い場合
            CommonFunctions.ShowMessage("10029", Me.lblFooterMessage, naeiw:=C_NAEIW.INFORMATION, pageObject:=Me)
            Return
        End If
        btnApprovalConfirmOk_Click()
    End Sub
    ''' <summary>
    ''' 承認確認ボタンOK時イベント
    ''' </summary>
    Public Sub btnApprovalConfirmOk_Click()
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        brInfo = DirectCast(ViewState(CONST_VS_NAME_DICBRINFO), Dictionary(Of String, BreakerInfo))

        '承認登録
        COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
        COA0032Apploval.I_APPLYID = Me.hdnApplyId.Value
        COA0032Apploval.I_STEP = Me.hdnStep.Value
        COA0032Apploval.COA0032setApproval()
        If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage)
            Return
        End If

        If Me.hdnLastStep.Value = Me.hdnStep.Value Then

            Dim brId As String = ""
            'オーダー登録
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                brId = Me.hdnBrId.Value
                'ブレーカー費用項目取得
                Dim costDt As DataTable = GetBreakerCostData(brId, sqlCon)
                '新規オーダー番号生成（シーケンスより取得）
                Dim orderNo As String = GetOrderNo(sqlCon)
                Dim copyCnt As Integer = 1
                'DB登録実行
                Dim entDate As Date = Date.Now
                Dim tran As SqlTransaction = sqlCon.BeginTransaction() 'トランザクション開始
                InsertOrderBase(orderNo, brId, sqlCon, tran, entDate)
                InsertOrderValue(orderNo, costDt, copyCnt, brId, sqlCon, tran, entDate)
                InsertOrderValue2(orderNo, costDt, copyCnt, sqlCon, tran, entDate)
                tran.Commit()
                sqlCon.Close()
            End Using

            'メール
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = "BRR_Approved"
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = Me.lblBrNo.Text
            GBA00009MailSendSet.BRSUBID = brInfo("INFO").SubId
            GBA00009MailSendSet.BRBASEID = brInfo("INFO").LinkId
            GBA00009MailSendSet.BRROUND = ""
            GBA00009MailSendSet.APPLYID = Me.hdnApplyId.Value
            GBA00009MailSendSet.LASTSTEP = Me.hdnLastStep.Value
            GBA00009MailSendSet.GBA00009setMailToRepBR()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
                Return
            End If

            '正式フォルダに移行
            isBeforeApploveFlg = False

            'File更新処理
            '添付ファイルを正式フォルダに転送
            CommonFunctions.SaveAttachmentFilesList(AfterRepairAttachment, brInfo("INFO").BrId, CONST_DIRNAME_REPAIR, isBeforeApploveFlg, After, True)
            'DoneFile更新処理
            CommonFunctions.SaveAttachmentFilesList(BeforeRepairAttachment, brInfo("INFO").BrId, CONST_DIRNAME_REPAIR, isBeforeApploveFlg, Before, True)

        End If

        Me.hdnMsgId.Value = C_MESSAGENO.APPROVALSUCCESS
        Me.hdnAlreadyFlg.Value = "1" '承認済フラグ(承認画面の遷移時のみ設定 1:承認済)
        Me.hdnStatus.Value = C_APP_STATUS.APPROVED

        Dim thisPageUrl As String = Request.Url.ToString
        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
        Server.Transfer(Request.Url.LocalPath)

    End Sub
    ''' <summary>
    ''' 発着の費用情報を取得
    ''' </summary>
    ''' <param name="breakerId"></param>
    ''' <param name="sqlCon">省略可能：省略した場合は新規接続を生成</param>
    ''' <returns></returns>
    ''' <remarks>オーダー情報として利用するブレーカー費用項目取得</remarks>
    Private Function GetBreakerCostData(breakerId As String, Optional ByRef sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim dtDbResult As New DataTable
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            '発着側
            sqlStat.AppendLine("SELECT INF.BRID")
            sqlStat.AppendLine("     , ISNULL(INF.USETYPE,'')  AS USETYPE")
            sqlStat.AppendLine("     , VL.DTLPOLPOD            AS AGENTKBN")
            sqlStat.AppendLine("     , VL.COSTCODE             AS COSTCODE")
            sqlStat.AppendLine("     , VL.CURRENCYCODE         AS CURRENCYCODE")
            sqlStat.AppendLine("     , VL.TAXATION             AS TAXATION")
            sqlStat.AppendLine("     , VL.APPROVEDUSD          AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.LOCAL                AS LOCALBR")
            sqlStat.AppendLine("     , VL.LOCALRATE            AS LOCALRATE")
            sqlStat.AppendLine("     , VL.TAX                  AS TAXBR")
            sqlStat.AppendLine("     , VL.CONTRACTOR           AS CONTRACTORBR")
            sqlStat.AppendLine("     , VL.REMARK               AS REMARK")
            sqlStat.AppendLine("     , ISNULL((CASE VL.DTLPOLPOD WHEN 'POL1' THEN BS.AGENTPOL1 ")
            sqlStat.AppendLine("                                 WHEN 'POL2' THEN BS.AGENTPOL2 ")
            sqlStat.AppendLine("                                 WHEN 'POD1' THEN BS.AGENTPOD1 ")
            sqlStat.AppendLine("                                 WHEN 'POD2' THEN BS.AGENTPOD2 ")
            sqlStat.AppendLine("                                 ELSE '' END ")
            sqlStat.AppendLine("             ),'')             AS OFFICE ")
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTY")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS1,'')   AS WORKOSEQ")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS2,'')   AS DISPSEQ")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS3,'')   AS DATEFIELD")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS4,'')   AS DATEINTERVAL")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS5,'')   AS LASTACT")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS6,'')   AS REQUIREDACT")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS7,'')   AS ORIGINDESTINATION")

            sqlStat.AppendLine("     , ISNULL(BS.INVOICEDBY,'')     AS INVOICEDBY")
            sqlStat.AppendLine("     , ISNULL(BS.AGENTORGANIZER,'') AS AGENTORGANIZER")
            sqlStat.AppendLine("     , CONVERT([date],'1900/01/01') AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , VL.COUNTRYCODE               AS COUNTRYCODE")
            sqlStat.AppendLine("     , ISNULL(BS.TANKNO,'')         AS TANKNO")
            sqlStat.AppendLine("  FROM GBT0001_BR_INFO INF")
            sqlStat.AppendLine("  INNER JOIN  GBT0003_BR_VALUE VL")
            sqlStat.AppendLine("     ON VL.BRID       = @BRID ")
            sqlStat.AppendLine("    AND VL.BRID       = INF.BRID ")
            sqlStat.AppendLine("    AND VL.STYMD     <= INF.ENDYMD")
            sqlStat.AppendLine("    AND VL.ENDYMD    >= INF.STYMD")
            sqlStat.AppendLine("    AND VL.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  INNER JOIN  GBT0002_BR_BASE BS")
            sqlStat.AppendLine("     ON BS.BRID      = @BRID ")
            sqlStat.AppendLine("    AND BS.BRID      = INF.BRID ")
            sqlStat.AppendLine("    AND BS.DELFLG   <> @DELFLG")
            sqlStat.AppendLine(" WHERE INF.BRID      = @BRID")
            sqlStat.AppendLine("   AND INF.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("   AND INF.TYPE      = 'INFO' ")

            'DB接続
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dtDbResult)
                End Using
            End Using

            Return dtDbResult

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
    ''' オーダーNoをシーケンスより取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOrderNo(Optional ByRef sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim orderNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT  '' ")
            sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + (SELECT VALUE1")
            sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
            sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
            sqlStat.AppendLine("            AND KEYCODE = @KEYCODE")
            sqlStat.AppendLine("            AND STYMD  <= @STYMD")
            sqlStat.AppendLine("            AND ENDYMD >= @ENDYMD")
            sqlStat.AppendLine("            AND DELFLG <> @DELFLG)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR GBQ0003_ORDER)),4)")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = "SERVERSEQ"
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get new Order error")
                    End If

                    orderNo = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return orderNo
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
    ''' オーダー基本情報登録
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <param name="breakerId"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="entDate"></param>
    ''' <remarks>承認時に実行される</remarks>
    Private Sub InsertOrderBase(orderNo As String, breakerId As String, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If entDate = Date.Parse("1900/01/01") Then
            entDate = Date.Now
        End If
        '文言フィールド（開発中のためいったん固定
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If

        Dim COA0035Convert As New BASEDLL.COA0035Convert
        Dim cnvStr As String = Nothing
        COA0035Convert.I_CONVERT = Convert.ToString(1)
        COA0035Convert.I_CLASS = "CONVERT"
        COA0035Convert.COA0035convNumToEng()
        If COA0035Convert.O_ERR = C_MESSAGENO.NORMAL Then
            cnvStr = COA0035Convert.O_CONVERT1
        Else
            Throw New Exception("Fix value getError")
        End If

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0004_ODR_BASE (")
        sqlStat.AppendLine("       ORDERNO")
        sqlStat.AppendLine("      ,STYMD")
        sqlStat.AppendLine("      ,BRID")
        sqlStat.AppendLine("      ,BRTYPE")
        sqlStat.AppendLine("      ,VALIDITYFROM")
        sqlStat.AppendLine("      ,VALIDITYTO")
        sqlStat.AppendLine("      ,TERMTYPE")
        sqlStat.AppendLine("      ,NOOFTANKS")
        sqlStat.AppendLine("      ,SHIPPER")
        sqlStat.AppendLine("      ,CONSIGNEE")
        sqlStat.AppendLine("      ,CARRIER1")
        sqlStat.AppendLine("      ,CARRIER2")
        sqlStat.AppendLine("      ,PRODUCTCODE")
        sqlStat.AppendLine("      ,PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,RECIEPTCOUNTRY1")
        sqlStat.AppendLine("      ,RECIEPTPORT1")
        sqlStat.AppendLine("      ,RECIEPTCOUNTRY2")
        sqlStat.AppendLine("      ,RECIEPTPORT2")
        sqlStat.AppendLine("      ,LOADCOUNTRY1")
        sqlStat.AppendLine("      ,LOADPORT1")
        sqlStat.AppendLine("      ,LOADCOUNTRY2")
        sqlStat.AppendLine("      ,LOADPORT2")
        sqlStat.AppendLine("      ,DISCHARGECOUNTRY1")
        sqlStat.AppendLine("      ,DISCHARGEPORT1")
        sqlStat.AppendLine("      ,DISCHARGECOUNTRY2")
        sqlStat.AppendLine("      ,DISCHARGEPORT2")
        sqlStat.AppendLine("      ,DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,DELIVERYPORT1")
        sqlStat.AppendLine("      ,DELIVERYCOUNTRY2")
        sqlStat.AppendLine("      ,DELIVERYPORT2")
        sqlStat.AppendLine("      ,VSL1")
        sqlStat.AppendLine("      ,VOY1")
        sqlStat.AppendLine("      ,ETD1")
        sqlStat.AppendLine("      ,ETA1")
        sqlStat.AppendLine("      ,VSL2")
        sqlStat.AppendLine("      ,VOY2")
        sqlStat.AppendLine("      ,ETD2")
        sqlStat.AppendLine("      ,ETA2")
        sqlStat.AppendLine("      ,INVOICEDBY")
        sqlStat.AppendLine("      ,LOADING")
        sqlStat.AppendLine("      ,STEAMING")
        sqlStat.AppendLine("      ,TIP")
        sqlStat.AppendLine("      ,EXTRA")
        sqlStat.AppendLine("      ,DEMURTO")
        sqlStat.AppendLine("      ,DEMURUSRATE1")
        sqlStat.AppendLine("      ,DEMURUSRATE2")
        sqlStat.AppendLine("      ,SALESPIC")
        sqlStat.AppendLine("      ,AGENTORGANIZER")
        sqlStat.AppendLine("      ,AGENTPOL1")
        sqlStat.AppendLine("      ,AGENTPOL2")
        sqlStat.AppendLine("      ,AGENTPOD1")
        sqlStat.AppendLine("      ,AGENTPOD2")

        sqlStat.AppendLine("      ,SHIPPERTEXT")
        sqlStat.AppendLine("      ,CONSIGNEETEXT")
        sqlStat.AppendLine("      ,NOTIFYTEXT")
        sqlStat.AppendLine("      ,NOTIFYCONTTEXT1")
        sqlStat.AppendLine("      ,NOTIFYCONTTEXT2")

        sqlStat.AppendLine("      ,PREPAIDAT")
        sqlStat.AppendLine("      ,EXCHANGERATE")
        sqlStat.AppendLine("      ,LOCALCURRENCY")
        sqlStat.AppendLine("      ,PAYABLEAT")

        sqlStat.AppendLine("      ,FREIGHTANDCHARGES")
        sqlStat.AppendLine("      ,GOODSPKGS")
        sqlStat.AppendLine("      ,CONTAINERPKGS")
        sqlStat.AppendLine("      ,NOOFPACKAGE")

        sqlStat.AppendLine("      ,DELFLG")
        sqlStat.AppendLine("      ,INITYMD")
        sqlStat.AppendLine("      ,INITUSER")
        sqlStat.AppendLine("      ,UPDYMD")
        sqlStat.AppendLine("      ,UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD")
        sqlStat.AppendLine(" ) ")
        sqlStat.AppendLine("SELECT @ORDERNO")
        sqlStat.AppendLine("      ,@STYMD")
        sqlStat.AppendLine("      ,BI.BRID")
        sqlStat.AppendLine("      ,BI.BRTYPE")
        sqlStat.AppendLine("      ,BB.VALIDITYFROM")
        sqlStat.AppendLine("      ,BB.VALIDITYTO")
        sqlStat.AppendLine("      ,BB.TERMTYPE")
        sqlStat.AppendLine("      ,BB.NOOFTANKS")
        sqlStat.AppendLine("      ,BB.SHIPPER")
        sqlStat.AppendLine("      ,BB.CONSIGNEE")
        sqlStat.AppendLine("      ,BB.CARRIER1")
        sqlStat.AppendLine("      ,BB.CARRIER2")
        sqlStat.AppendLine("      ,BB.PRODUCTCODE")
        sqlStat.AppendLine("      ,BB.PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,BB.RECIEPTCOUNTRY1")
        sqlStat.AppendLine("      ,BB.RECIEPTPORT1")
        sqlStat.AppendLine("      ,BB.RECIEPTCOUNTRY2")
        sqlStat.AppendLine("      ,BB.RECIEPTPORT2")
        sqlStat.AppendLine("      ,BB.LOADCOUNTRY1")
        sqlStat.AppendLine("      ,BB.LOADPORT1")
        sqlStat.AppendLine("      ,BB.LOADCOUNTRY2")
        sqlStat.AppendLine("      ,BB.LOADPORT2")
        sqlStat.AppendLine("      ,BB.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("      ,BB.DISCHARGEPORT1")
        sqlStat.AppendLine("      ,BB.DISCHARGECOUNTRY2")
        sqlStat.AppendLine("      ,BB.DISCHARGEPORT2")
        sqlStat.AppendLine("      ,BB.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,BB.DELIVERYPORT1")
        sqlStat.AppendLine("      ,BB.DELIVERYCOUNTRY2")
        sqlStat.AppendLine("      ,BB.DELIVERYPORT2")
        sqlStat.AppendLine("      ,BB.VSL1")
        sqlStat.AppendLine("      ,BB.VOY1")
        sqlStat.AppendLine("      ,BB.ETD1")
        sqlStat.AppendLine("      ,BB.ETA1")
        sqlStat.AppendLine("      ,BB.VSL2")
        sqlStat.AppendLine("      ,BB.VOY2")
        sqlStat.AppendLine("      ,BB.ETD2")
        sqlStat.AppendLine("      ,BB.ETA2")
        sqlStat.AppendLine("      ,BB.INVOICEDBY")
        sqlStat.AppendLine("      ,BB.LOADING")
        sqlStat.AppendLine("      ,BB.STEAMING")
        sqlStat.AppendLine("      ,BB.TIP")
        sqlStat.AppendLine("      ,BB.EXTRA")
        sqlStat.AppendLine("      ,BB.DEMURTO")
        sqlStat.AppendLine("      ,BB.DEMURUSRATE1")
        sqlStat.AppendLine("      ,BB.DEMURUSRATE2")
        sqlStat.AppendLine("      ,@SALESPIC")
        sqlStat.AppendLine("      ,BB.AGENTORGANIZER")
        sqlStat.AppendLine("      ,BB.AGENTPOL1")
        sqlStat.AppendLine("      ,BB.AGENTPOL2")
        sqlStat.AppendLine("      ,BB.AGENTPOD1")
        sqlStat.AppendLine("      ,BB.AGENTPOD2")

        sqlStat.AppendFormat("      ,ISNULL(SP.{0} + CHAR(13) + CHAR(10) + SP.ADDR,'') AS SHIPPERTEXT ", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CS.{0} + CHAR(13) + CHAR(10) + CS.ADDR,'') AS CONSIGNEETEXT", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CS.{0} + CHAR(13) + CHAR(10) + CS.ADDR,'') AS NOTIFYTEXT", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,ISNULL(TR1.NAMEL + CHAR(13) + CHAR(10) + TR1.ADDR + CHAR(13) + CHAR(10) + CASE WHEN TR1.TEL = '' THEN '' ELSE 'TEL:' + TR1.TEL + ' ' END + CASE WHEN TR1.FAX = '' THEN '' ELSE 'FAX:' + TR1.FAX END ,'') AS NOTIFYCONTTEXT1")
        sqlStat.AppendLine("      ,ISNULL(TR2.NAMEL + CHAR(13) + CHAR(10) + TR2.ADDR + CHAR(13) + CHAR(10) + CASE WHEN TR2.TEL = '' THEN '' ELSE 'TEL:' + TR2.TEL + ' ' END + CASE WHEN TR2.FAX = '' THEN '' ELSE 'FAX:' + TR2.FAX END ,'') AS NOTIFYCONTTEXT2")

        sqlStat.AppendLine("      ,ISNULL(SP.CITY,'') AS PREPAIDAT")
        sqlStat.AppendLine("      ,ISNULL(ER.EXRATE,'') AS EXCHANGERATE")
        sqlStat.AppendLine("      ,ISNULL(ER.CURRENCYCODE,'') AS LOCALCURRENCY")
        sqlStat.AppendLine("      ,ISNULL(CS.CITY,'') AS PAYABLEAT")

        sqlStat.AppendLine("      ,CASE WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY1 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY1 THEN '""FREIGHT COLLECT"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY2 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY2 THEN '""FREIGHT COLLECT"" AS ARRANGED' ")
        sqlStat.AppendLine("      ELSE '' END AS FREIGHTANDCHARGES")
        sqlStat.AppendLine("      ,ISNULL(FV1.VALUE1 + CASE WHEN PD.PRODUCTNAME IS NULL THEN '' ELSE CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) + TRIM(PD.PRODUCTNAME) END + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) + CONVERT(nvarchar,BB.TIP) + @DAYSTEXT + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) ")
        sqlStat.AppendLine("      + CASE WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY1 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY1 THEN '""FREIGHT COLLECT"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY2 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY2 THEN '""FREIGHT COLLECT"" AS ARRANGED' ")
        sqlStat.AppendLine("      ELSE '' END ,'') AS GOODSPKGS")
        sqlStat.AppendLine("      ,@CONTAINERPKGS")
        sqlStat.AppendLine("      ,@NOOFPACKAGE")

        sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'") '削除フラグ(0固定)
        sqlStat.AppendLine("      ,@ENTDATE")
        sqlStat.AppendLine("      ,@UPDUSER")
        sqlStat.AppendLine("      ,@ENTDATE")
        sqlStat.AppendLine("      ,@UPDUSER")
        sqlStat.AppendLine("      ,@UPDTERMID")
        sqlStat.AppendLine("      ,@RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
        sqlStat.AppendLine("  LEFT JOIN GBT0002_BR_BASE BB")
        sqlStat.AppendLine("    ON BB.BRID     = @BRID")
        sqlStat.AppendLine("   AND BB.BRID     = BI.BRID")
        sqlStat.AppendLine("   AND BB.BRBASEID = BI.LINKID")
        sqlStat.AppendLine("   AND BB.DELFLG  <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'Shipper
        sqlStat.AppendLine("    ON SP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND SP.CUSTOMERCODE = BB.SHIPPER")
        sqlStat.AppendLine("   AND SP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND SP.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND SP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CS") 'Consinee
        sqlStat.AppendLine("    ON CS.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND CS.CUSTOMERCODE = BB.CONSIGNEE")
        sqlStat.AppendLine("   AND CS.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND CS.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND CS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND CS.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR1") 'Party to Contact1
        sqlStat.AppendLine("    ON TR1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TR1.CARRIERCODE  = BB.AGENTPOD1")
        sqlStat.AppendLine("   AND TR1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TR1.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TR1.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR2") 'Party to Contact2
        sqlStat.AppendLine("    ON TR2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TR2.CARRIERCODE  = BB.AGENTPOD2")
        sqlStat.AppendLine("   AND TR2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TR2.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TR2.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'Product
        sqlStat.AppendLine("    ON PD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND PD.PRODUCTCODE  = BB.PRODUCTCODE")
        sqlStat.AppendLine("   AND PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND PD.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND PD.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") 'FIXVAL
        sqlStat.AppendLine("    ON FV1.CLASS       = 'DESCGOODS'")
        sqlStat.AppendLine("   AND FV1.STYMD      <= getdate()")
        sqlStat.AppendLine("   AND FV1.ENDYMD     >= getdate()")
        sqlStat.AppendLine("   AND FV1.DELFLG     <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE ER") 'ExRate
        sqlStat.AppendLine("    ON ER.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND ER.COUNTRYCODE  = SP.COUNTRYCODE")
        sqlStat.AppendLine("   AND ER.TARGETYM     = (SELECT FORMAT(CONVERT(DATETIME,(LEFT(CONVERT(VARCHAR, GETDATE(), 112), 6)+'01')),'yyyy/MM/dd'))")
        sqlStat.AppendLine("   AND ER.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR3") 'COUNTRY
        sqlStat.AppendLine("    ON TR3.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TR3.CARRIERCODE  = BB.INVOICEDBY")
        sqlStat.AppendLine("   AND TR3.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TR3.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TR3.DELFLG      <> @DELFLG")

        sqlStat.AppendLine(" WHERE BI.BRID    = @BRID")
        sqlStat.AppendLine("   AND BI.TYPE    = @TYPE")
        sqlStat.AppendLine("   AND BI.DELFLG <> @DELFLG")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                'SQLパラメータの設定
                With sqlCmd.Parameters
                    .Add("@ORDERNO", SqlDbType.NVarChar, 20).Value = orderNo
                    .Add("@STYMD", SqlDbType.Date).Value = entDate
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                    .Add("@TYPE", SqlDbType.NVarChar, 20).Value = "INFO"
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVCamp
                    .Add("@DAYSTEXT", SqlDbType.NVarChar).Value = "DAYS DETENTION FREE AT DESTINATION"
                    .Add("@CONTAINERPKGS", SqlDbType.NVarChar).Value = cnvStr & "(" & Convert.ToString(1) & ")" & " TANK CONTAINER(S) ONLY"
                    .Add("@NOOFPACKAGE", SqlDbType.NVarChar).Value = Convert.ToString(1)
                    .Add("@SALESPIC", SqlDbType.NVarChar).Value = COA0019Session.USERID
                End With
                sqlCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try
    End Sub
    ''' <summary>
    ''' オーダー費用情報を更新
    ''' </summary>
    ''' <param name="orderNo">オーダーNo</param>
    ''' <param name="dt">費用データテーブル(コピー増幅なし)</param>
    ''' <param name="copyNum">コピー数</param>
    ''' <param name="sqlCon">[In(省略可)]SQL接続オブジェクト</param>
    ''' <param name="tran">[In(省略可)]SQLトランザクションオブジェクト</param>
    Private Sub InsertOrderValue(orderNo As String, dt As DataTable, copyNum As Integer, brId As String, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If entDate = Date.Parse("1900/01/01") Then
            entDate = Date.Now
        End If
        'この段階でありえないがコピー数が0の場合は終了
        If copyNum = 0 Then
            Return
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("        ORDERNO")
        sqlStat.AppendLine("       ,TANKSEQ")
        sqlStat.AppendLine("       ,DTLPOLPOD")
        sqlStat.AppendLine("       ,DTLOFFICE")
        sqlStat.AppendLine("       ,TANKNO")
        sqlStat.AppendLine("       ,COSTCODE")
        sqlStat.AppendLine("       ,ACTIONID")
        sqlStat.AppendLine("       ,DISPSEQ")
        sqlStat.AppendLine("       ,LASTACT")
        sqlStat.AppendLine("       ,REQUIREDACT")
        sqlStat.AppendLine("       ,ORIGINDESTINATION")
        sqlStat.AppendLine("       ,COUNTRYCODE")
        sqlStat.AppendLine("       ,CURRENCYCODE")
        sqlStat.AppendLine("       ,TAXATION")
        sqlStat.AppendLine("       ,AMOUNTBR")
        sqlStat.AppendLine("       ,AMOUNTORD")
        sqlStat.AppendLine("       ,AMOUNTFIX")
        sqlStat.AppendLine("       ,CONTRACTORBR")
        sqlStat.AppendLine("       ,CONTRACTORODR")
        sqlStat.AppendLine("       ,CONTRACTORFIX")
        sqlStat.AppendLine("       ,SCHEDELDATEBR")
        sqlStat.AppendLine("       ,SCHEDELDATE")
        sqlStat.AppendLine("       ,LOCALBR")
        sqlStat.AppendLine("       ,LOCALRATE")
        sqlStat.AppendLine("       ,TAXBR")
        sqlStat.AppendLine("       ,INVOICEDBY")
        sqlStat.AppendLine("       ,REMARK")
        sqlStat.AppendLine("       ,BRID")
        sqlStat.AppendLine("       ,BRCOST")
        sqlStat.AppendLine("       ,DATEFIELD")
        sqlStat.AppendLine("       ,DATEINTERVAL")
        sqlStat.AppendLine("       ,BRADDEDCOST")
        sqlStat.AppendLine("       ,AGENTORGANIZER")
        sqlStat.AppendLine("       ,DELFLG")
        sqlStat.AppendLine("       ,INITYMD")
        sqlStat.AppendLine("       ,INITUSER")
        sqlStat.AppendLine("       ,UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD")
        sqlStat.AppendLine(" ) VALUES ( ")
        sqlStat.AppendLine("        @ORDERNO")
        sqlStat.AppendLine("       ,@TANKSEQ")
        sqlStat.AppendLine("       ,@DTLPOLPOD")
        sqlStat.AppendLine("       ,@DTLOFFICE")
        sqlStat.AppendLine("       ,@TANKNO")
        sqlStat.AppendLine("       ,@COSTCODE")
        sqlStat.AppendLine("       ,@ACTIONID")
        sqlStat.AppendLine("       ,@DISPSEQ")
        sqlStat.AppendLine("       ,@LASTACT")
        sqlStat.AppendLine("       ,@REQUIREDACT")
        sqlStat.AppendLine("       ,@ORIGINDESTINATION")
        sqlStat.AppendLine("       ,@COUNTRYCODE")
        sqlStat.AppendLine("       ,@CURRENCYCODE")
        sqlStat.AppendLine("       ,@TAXATION")
        sqlStat.AppendLine("       ,@AMOUNTBR")
        sqlStat.AppendLine("       ,@AMOUNTBR")
        sqlStat.AppendLine("       ,@AMOUNTBR")
        sqlStat.AppendLine("       ,@CONTRACTORBR")
        sqlStat.AppendLine("       ,@CONTRACTORBR")
        sqlStat.AppendLine("       ,@CONTRACTORBR")
        sqlStat.AppendLine("       ,@SCHEDELDATEBR")
        sqlStat.AppendLine("       ,@SCHEDELDATEBR")
        sqlStat.AppendLine("       ,@LOCALBR")
        sqlStat.AppendLine("       ,@LOCALRATE")
        sqlStat.AppendLine("       ,@TAXBR")
        sqlStat.AppendLine("       ,@INVOICEDBY")
        sqlStat.AppendLine("       ,@REMARK")
        sqlStat.AppendLine("       ,@BRID")
        sqlStat.AppendLine("       ,@BRCOST")
        sqlStat.AppendLine("       ,@DATEFIELD")
        sqlStat.AppendLine("       ,@DATEINTERVAL")
        sqlStat.AppendLine("       ,@BRADDEDCOST")
        sqlStat.AppendLine("       ,@AGENTORGANIZER")
        sqlStat.AppendLine("       ,@DELFLG")
        sqlStat.AppendLine("       ,@ENTDATE")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@ENTDATE")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine(")")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                'SQLパラメータの設定
                Dim paramOrderno As SqlParameter = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 20)
                Dim paramTankSeq As SqlParameter = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar, 20)
                Dim paramDtlPolPod As SqlParameter = sqlCmd.Parameters.Add("@DTLPOLPOD", SqlDbType.NVarChar, 20)
                Dim paramDtlOffice As SqlParameter = sqlCmd.Parameters.Add("@DTLOFFICE", SqlDbType.NVarChar, 20)
                Dim paramTankNo As SqlParameter = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar, 20)
                Dim paramCostCode As SqlParameter = sqlCmd.Parameters.Add("@COSTCODE", SqlDbType.NVarChar, 20)
                Dim paramActionId As SqlParameter = sqlCmd.Parameters.Add("@ACTIONID", SqlDbType.NVarChar, 50)
                Dim paramDispSeq As SqlParameter = sqlCmd.Parameters.Add("@DISPSEQ", SqlDbType.NVarChar, 50)
                Dim paramLastAct As SqlParameter = sqlCmd.Parameters.Add("@LASTACT", SqlDbType.NVarChar, 50)
                Dim paramRequiredAct As SqlParameter = sqlCmd.Parameters.Add("@REQUIREDACT", SqlDbType.NVarChar, 50)
                Dim paramOriginDestination As SqlParameter = sqlCmd.Parameters.Add("@ORIGINDESTINATION", SqlDbType.NVarChar, 50)
                Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)

                Dim paramCurrencyCode As SqlParameter = sqlCmd.Parameters.Add("@CURRENCYCODE", SqlDbType.NVarChar, 20)
                Dim paramTaxation As SqlParameter = sqlCmd.Parameters.Add("@TAXATION", SqlDbType.NVarChar, 1)

                Dim paramAmountBr As SqlParameter = sqlCmd.Parameters.Add("@AMOUNTBR", SqlDbType.Float)
                Dim paramContractorBr As SqlParameter = sqlCmd.Parameters.Add("@CONTRACTORBR", SqlDbType.NVarChar, 20)
                Dim paramSchedelDateBr As SqlParameter = sqlCmd.Parameters.Add("@SCHEDELDATEBR", SqlDbType.Date, 20)
                Dim paramLocalBr As SqlParameter = sqlCmd.Parameters.Add("@LOCALBR", SqlDbType.Float, 20)
                Dim paramLocalRate As SqlParameter = sqlCmd.Parameters.Add("@LOCALRATE", SqlDbType.Float, 20)
                Dim paramTaxBr As SqlParameter = sqlCmd.Parameters.Add("@TAXBR", SqlDbType.Float, 20)
                Dim paramInvoicedBy As SqlParameter = sqlCmd.Parameters.Add("@INVOICEDBY", SqlDbType.NVarChar, 20)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)
                Dim paramBrid As SqlParameter = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar, 20)
                Dim paramBrCost As SqlParameter = sqlCmd.Parameters.Add("@BRCOST", SqlDbType.NVarChar, 20)

                Dim paramBrDateField As SqlParameter = sqlCmd.Parameters.Add("@DATEFIELD", SqlDbType.NVarChar, 50)
                Dim paramDateInterval As SqlParameter = sqlCmd.Parameters.Add("@DATEINTERVAL", SqlDbType.NVarChar, 50)
                Dim paramBrAddedCost As SqlParameter = sqlCmd.Parameters.Add("@BRADDEDCOST", SqlDbType.NVarChar, 50)

                Dim paramAgentOrganizer As SqlParameter = sqlCmd.Parameters.Add("@AGENTORGANIZER", SqlDbType.NVarChar, 20)
                Dim paramDelflg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                Dim paramEntDate As SqlParameter = sqlCmd.Parameters.Add("@ENTDATE", SqlDbType.DateTime)
                Dim paramUpduser As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)
                Dim paramUpdtermid As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 30)
                Dim paramReceiveymd As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)
                'コストデータに依存しない固定パラメータ値を設定
                paramOrderno.Value = orderNo
                paramBrid.Value = brId
                paramBrCost.Value = "1"
                paramDelflg.Value = CONST_FLAG_NO
                paramEntDate.Value = entDate
                paramUpduser.Value = COA0019Session.USERID
                paramUpdtermid.Value = HttpContext.Current.Session("APSRVname")
                paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD
                'コピー数分ループ(TANKSEQ)の0埋め前
                For i = 1 To copyNum
                    Dim tankSeq As String = i.ToString("000")
                    paramTankSeq.Value = tankSeq
                    'データテーブルループ
                    For Each dr As DataRow In dt.Rows
                        paramDtlPolPod.Value = Convert.ToString(dr.Item("AGENTKBN"))
                        paramDtlOffice.Value = Convert.ToString(dr.Item("OFFICE"))
                        paramTankNo.Value = Convert.ToString(dr.Item("TANKNO"))
                        paramCostCode.Value = Convert.ToString(dr.Item("COSTCODE"))
                        paramActionId.Value = Convert.ToString(dr.Item("ACTY"))
                        paramDispSeq.Value = Convert.ToString(dr.Item("DISPSEQ"))
                        paramLastAct.Value = Convert.ToString(dr.Item("LASTACT"))
                        paramRequiredAct.Value = Convert.ToString(dr.Item("REQUIREDACT"))
                        paramOriginDestination.Value = Convert.ToString(dr.Item("ORIGINDESTINATION"))
                        paramCountryCode.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                        '2019/10/10 START 日本のリペアであれば通貨コードは入力に従う(JPY or USD)
                        If dr.Item("COUNTRYCODE").Equals("JP") Then
                            '日本リペアの場合
                            paramCurrencyCode.Value = Convert.ToString(dr.Item("CURRENCYCODE"))
                        Else
                            'その他の場合USD固定
                            paramCurrencyCode.Value = GBC_CUR_USD
                        End If
                        '2019/10/10 END 日本のリペアであれば通貨コードは入力に従う(JPY or USD)
                        paramTaxation.Value = Convert.ToString(dr.Item("TAXATION"))
                        '2019/10/10 START 日本のリペア且つ通貨コードがJPYの場合はAmountに円価格を設定
                        If dr.Item("COUNTRYCODE").Equals("JP") AndAlso dr.Item("CURRENCYCODE").Equals("JPY") Then
                            paramAmountBr.Value = dr.Item("LOCALBR")
                        Else
                            paramAmountBr.Value = dr.Item("AMOUNTBR")
                        End If

                        paramContractorBr.Value = Convert.ToString(dr.Item("CONTRACTORBR"))
                        'paramSchedelDateBr.Value = dr.Item("SCHEDELDATEBR")
                        paramSchedelDateBr.Value = entDate
                        paramLocalBr.Value = dr.Item("LOCALBR")
                        paramLocalRate.Value = dr.Item("LOCALRATE")
                        paramTaxBr.Value = dr.Item("TAXBR")
                        paramInvoicedBy.Value = Convert.ToString(dr.Item("INVOICEDBY"))
                        paramRemark.Value = Convert.ToString(dr.Item("REMARK"))

                        paramBrDateField.Value = Convert.ToString(dr.Item("DATEFIELD"))
                        paramDateInterval.Value = Convert.ToString(dr.Item("DATEINTERVAL"))
                        paramBrAddedCost.Value = ""

                        paramAgentOrganizer.Value = Convert.ToString(dr.Item("AGENTORGANIZER"))

                        sqlCmd.ExecuteNonQuery()
                    Next 'End DataRow Loop
                Next 'End copyNum Loop
            End Using
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try
    End Sub
    ''' <summary>
    ''' オーダー2費用情報を更新
    ''' </summary>
    ''' <param name="orderNo">オーダーNo</param>
    ''' <param name="dt">費用データテーブル(コピー増幅なし)</param>
    ''' <param name="copyNum">コピー数</param>
    ''' <param name="sqlCon">[In(省略可)]SQL接続オブジェクト</param>
    ''' <param name="tran">[In(省略可)]SQLトランザクションオブジェクト</param>
    Private Sub InsertOrderValue2(orderNo As String, dt As DataTable, copyNum As Integer, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If entDate = Date.Parse("1900/01/01") Then
            entDate = Date.Now
        End If
        'この段階でありえないがコピー数が0の場合は終了
        If copyNum = 0 Then
            Return
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0007_ODR_VALUE2 (")
        sqlStat.AppendLine("        ORDERNO")
        sqlStat.AppendLine("       ,TANKSEQ")
        sqlStat.AppendLine("       ,TRILATERAL")
        sqlStat.AppendLine("       ,TANKTYPE")
        sqlStat.AppendLine("       ,NOOFPACKAGE")
        sqlStat.AppendLine("       ,DELFLG")
        sqlStat.AppendLine("       ,INITYMD")
        sqlStat.AppendLine("       ,UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD")
        sqlStat.AppendLine(" ) VALUES ( ")
        sqlStat.AppendLine("        @ORDERNO")
        sqlStat.AppendLine("       ,@TANKSEQ")
        sqlStat.AppendLine("       ,@TRILATERAL")
        sqlStat.AppendLine("       ,@TANKTYPE")
        sqlStat.AppendLine("       ,@NOOFPACKAGE")
        sqlStat.AppendLine("       ,@DELFLG")
        sqlStat.AppendLine("       ,@ENTDATE")
        sqlStat.AppendLine("       ,@ENTDATE")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine(")")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                'SQLパラメータの設定
                Dim paramOrderno As SqlParameter = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 20)
                Dim paramTankSeq As SqlParameter = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar, 20)
                Dim paramTrilateral As SqlParameter = sqlCmd.Parameters.Add("@TRILATERAL", SqlDbType.NVarChar, 1)
                Dim paramTankType As SqlParameter = sqlCmd.Parameters.Add("@TANKTYPE", SqlDbType.NVarChar, 20)
                Dim paramDelflg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                Dim paramEntDate As SqlParameter = sqlCmd.Parameters.Add("@ENTDATE", SqlDbType.DateTime)
                Dim paramUpduser As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)
                Dim paramUpdtermid As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 30)
                Dim paramReceiveymd As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)
                Dim paramNoOfPackage As SqlParameter = sqlCmd.Parameters.Add("@NOOFPACKAGE", SqlDbType.Float)
                'コストデータに依存しない固定パラメータ値を設定
                paramOrderno.Value = orderNo
                paramTankType.Value = "20TK"
                paramDelflg.Value = CONST_FLAG_NO
                paramEntDate.Value = entDate
                paramUpduser.Value = COA0019Session.USERID
                paramUpdtermid.Value = HttpContext.Current.Session("APSRVname")
                paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD
                paramNoOfPackage.Value = 1

                'コピー数分ループ(TANKSEQ)の0埋め前
                For i = 1 To copyNum
                    Dim tankSeq As String = i.ToString("000")
                    paramTankSeq.Value = tankSeq
                    Dim blDataList As New Dictionary(Of String, Object)
                    Dim oneFlg As Boolean = False
                    Dim twoFlg As Boolean = False

                    'データテーブルループ
                    For Each dr As DataRow In dt.Rows

                        Select Case Convert.ToString(dr.Item("AGENTKBN"))
                            Case "POL1", "POD1"
                                paramTrilateral.Value = "1"
                                If oneFlg Then
                                    Continue For
                                Else
                                    oneFlg = True
                                End If
                            Case "POL2", "POD2"
                                paramTrilateral.Value = "2"
                                If twoFlg Then
                                    Continue For
                                Else
                                    twoFlg = True
                                End If
                            Case Else
                                Continue For
                        End Select

                        sqlCmd.ExecuteNonQuery()
                    Next 'End DataRow Loop

                Next 'End copyNum Loop
            End Using
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 否認ボタン押下時処理
    ''' </summary>
    Public Sub btnReject_Click()
        TextChangeCheck()
        If Me.hdnMsgboxShowFlg.Value = "1" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.HASNOSAVEITEMS, Me.lblFooterMessage, naeiw:=C_NAEIW.INFORMATION, pageObject:=Me)
            Return
        End If

        btnRejectConfirmOk_Click()
    End Sub
    ''' <summary>
    ''' 否認確認ボタンOK時イベント
    ''' </summary>
    Public Sub btnRejectConfirmOk_Click()
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        brInfo = DirectCast(ViewState(CONST_VS_NAME_DICBRINFO), Dictionary(Of String, BreakerInfo))

        '否認登録
        COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
        COA0032Apploval.I_APPLYID = Me.hdnApplyId.Value
        COA0032Apploval.I_STEP = Me.hdnStep.Value
        COA0032Apploval.COA0032setDenial()
        If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage)
            Return
        End If

        'メール
        Dim GBA00009MailSendSet As New GBA00009MailSendSet
        GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
        GBA00009MailSendSet.EVENTCODE = "BRR_Rejected"
        GBA00009MailSendSet.MAILSUBCODE = ""
        GBA00009MailSendSet.BRID = Me.lblBrNo.Text
        GBA00009MailSendSet.BRSUBID = brInfo("INFO").SubId
        GBA00009MailSendSet.BRBASEID = brInfo("INFO").LinkId
        GBA00009MailSendSet.BRROUND = ""
        GBA00009MailSendSet.APPLYID = Me.hdnApplyId.Value
        GBA00009MailSendSet.LASTSTEP = Me.hdnLastStep.Value
        GBA00009MailSendSet.GBA00009setMailToRepBR()
        If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
            Return
        End If

        '繰り上げ
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            Dim brId As String = Me.hdnBrId.Value
            Dim subId As String = Me.hdnSubId.Value
            'DB登録実行
            Dim entDate As Date = Date.Now
            Dim tran As SqlTransaction = sqlCon.BeginTransaction() 'トランザクション開始
            InsertBreaker(subId, brId, sqlCon, tran, entDate)
            tran.Commit()
            sqlCon.Close()
        End Using

        ''メッセージ出力
        'CommonFunctions.ShowMessage(C_MESSAGENO.REJECTSUCCESS, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

        Me.hdnMsgId.Value = C_MESSAGENO.REJECTSUCCESS

        Me.hdnStatus.Value = C_APP_STATUS.REJECT

        Dim thisPageUrl As String = Request.Url.ToString
        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
        Server.Transfer(Request.Url.LocalPath)

    End Sub
    ''' <summary>
    ''' ブレーカー更新処理
    ''' </summary>
    ''' <param name="subId"></param>
    ''' <param name="breakerId"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="entDate"></param>
    Private Sub InsertBreaker(subId As String, breakerId As String, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If entDate = Date.Parse("1900/01/01") Then
            entDate = Date.Now
        End If

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Dim sqlStat As New StringBuilder
            Dim dt As New DataTable

            'LinkId取得
            sqlStat.Clear()
            sqlStat.AppendLine("Select TYPE , LINKID")
            sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
            sqlStat.AppendLine(" WHERE BRID    = @BRID")
            sqlStat.AppendLine("   And SUBID   = @SUBID")
            sqlStat.AppendLine("   And DELFLG <> @DELFLG")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                'SQLパラメータの設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                    .Add("@SUBID", SqlDbType.NVarChar, 20).Value = subId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using

            End Using

            '削除更新
            'BR_INFO
            sqlStat.Clear()
            sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
            sqlStat.AppendLine("   Set DELFLG    = @DELFLG ")
            sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE BRID    = @BRID")
            sqlStat.AppendLine("   And SUBID   = @SUBID")
            sqlStat.AppendLine("   And DELFLG <> @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                    .Add("@SUBID", SqlDbType.NVarChar, 20).Value = subId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                sqlCmd.ExecuteNonQuery()
            End Using

            'BR_BASE
            sqlStat.Clear()
            sqlStat.AppendLine("UPDATE GBT0002_BR_BASE")
            sqlStat.AppendLine("   Set DELFLG    = @DELFLG ")
            sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE BRID      = @BRID")
            sqlStat.AppendLine("   And DELFLG   <> @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                sqlCmd.ExecuteNonQuery()
            End Using

            'BR_VALUE
            sqlStat.Clear()
            sqlStat.AppendLine("UPDATE GBT0003_BR_VALUE")
            sqlStat.AppendLine("   Set DELFLG    = @DELFLG ")
            sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE BRID      = @BRID")
            sqlStat.AppendLine("   And DELFLG   <> @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                sqlCmd.ExecuteNonQuery()
            End Using

            For i As Integer = 0 To dt.Rows.Count - 1

                'BR_INFO
                sqlStat.Clear()
                sqlStat.AppendLine("INSERT INTO GBT0001_BR_INFO (")
                sqlStat.AppendLine("       BRID")
                sqlStat.AppendLine("      ,SUBID")
                sqlStat.AppendLine("      ,TYPE")
                sqlStat.AppendLine("      ,LINKID")
                sqlStat.AppendLine("      ,STYMD")
                sqlStat.AppendLine("      ,BRTYPE")
                sqlStat.AppendLine("      ,APPLYID")
                sqlStat.AppendLine("      ,LASTSTEP")
                sqlStat.AppendLine("      ,USETYPE")
                sqlStat.AppendLine("      ,REMARK")
                sqlStat.AppendLine("      ,DELFLG")
                sqlStat.AppendLine("      ,INITYMD")
                sqlStat.AppendLine("      ,UPDYMD")
                sqlStat.AppendLine("      ,UPDUSER")
                sqlStat.AppendLine("      ,UPDTERMID")
                sqlStat.AppendLine("      ,RECEIVEYMD")
                sqlStat.AppendLine(" ) ")
                sqlStat.AppendLine("Select BRID")
                sqlStat.AppendLine("      ,'S' + RIGHT('00000' + CONVERT(varchar ,  CONVERT(int ,right(SUBID,5))+1), 5) AS SUBID ")
                sqlStat.AppendLine("      ,TYPE")
                sqlStat.AppendLine("      ,TYPE + '-' + RIGHT('00000' + CONVERT(varchar ,  CONVERT(int ,right(LINKID,5))+1), 5) AS LINKID ")
                sqlStat.AppendLine("      ,@STYMD")
                sqlStat.AppendLine("      ,BRTYPE")
                sqlStat.AppendLine("      ,APPLYID")
                sqlStat.AppendLine("      ,LASTSTEP")
                sqlStat.AppendLine("      ,USETYPE")
                sqlStat.AppendLine("      ,REMARK")
                sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'     ") '削除フラグ(0固定)
                sqlStat.AppendLine("      ,@ENTDATE")
                sqlStat.AppendLine("      ,@ENTDATE")
                sqlStat.AppendLine("      ,@UPDUSER")
                sqlStat.AppendLine("      ,@UPDTERMID")
                sqlStat.AppendLine("      ,@RECEIVEYMD")
                sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
                sqlStat.AppendLine(" WHERE BRID    = @BRID")
                sqlStat.AppendLine("   AND SUBID   = @SUBID")
                sqlStat.AppendLine("   AND TYPE    = @TYPE")
                sqlStat.AppendLine("   AND LINKID  = @LINKID")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    If tran IsNot Nothing Then
                        sqlCmd.Transaction = tran
                    End If
                    'SQLパラメータの設定
                    With sqlCmd.Parameters
                        .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                        .Add("@SUBID", SqlDbType.NVarChar, 20).Value = subId
                        .Add("@TYPE", SqlDbType.NVarChar, 20).Value = dt.Rows(i).Item("TYPE")
                        .Add("@LINKID", SqlDbType.NVarChar, 20).Value = dt.Rows(i).Item("LINKID")
                        .Add("@STYMD", SqlDbType.Date).Value = entDate
                        .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                        .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using

                If Convert.ToString(dt.Rows(i).Item("TYPE")) = "INFO" Then

                    'BR_BASE
                    sqlStat.Clear()
                    sqlStat.AppendLine("INSERT INTO GBT0002_BR_BASE (")
                    sqlStat.AppendLine("       BRID")
                    sqlStat.AppendLine("      ,BRBASEID")
                    sqlStat.AppendLine("      ,STYMD")
                    sqlStat.AppendLine("      ,VALIDITYFROM")
                    sqlStat.AppendLine("      ,VALIDITYTO")
                    sqlStat.AppendLine("      ,TERMTYPE")
                    sqlStat.AppendLine("      ,NOOFTANKS")
                    sqlStat.AppendLine("      ,SHIPPER")
                    sqlStat.AppendLine("      ,CONSIGNEE")
                    sqlStat.AppendLine("      ,CARRIER1")
                    sqlStat.AppendLine("      ,CARRIER2")
                    sqlStat.AppendLine("      ,PRODUCTCODE")
                    sqlStat.AppendLine("      ,PRODUCTWEIGHT")
                    sqlStat.AppendLine("      ,RECIEPTCOUNTRY1")
                    sqlStat.AppendLine("      ,RECIEPTPORT1")
                    sqlStat.AppendLine("      ,RECIEPTCOUNTRY2")
                    sqlStat.AppendLine("      ,RECIEPTPORT2")
                    sqlStat.AppendLine("      ,LOADCOUNTRY1")
                    sqlStat.AppendLine("      ,LOADPORT1")
                    sqlStat.AppendLine("      ,LOADCOUNTRY2")
                    sqlStat.AppendLine("      ,LOADPORT2")
                    sqlStat.AppendLine("      ,DISCHARGECOUNTRY1")
                    sqlStat.AppendLine("      ,DISCHARGEPORT1")
                    sqlStat.AppendLine("      ,DISCHARGECOUNTRY2")
                    sqlStat.AppendLine("      ,DISCHARGEPORT2")
                    sqlStat.AppendLine("      ,DELIVERYCOUNTRY1")
                    sqlStat.AppendLine("      ,DELIVERYPORT1")
                    sqlStat.AppendLine("      ,DELIVERYCOUNTRY2")
                    sqlStat.AppendLine("      ,DELIVERYPORT2")
                    sqlStat.AppendLine("      ,VSL1")
                    sqlStat.AppendLine("      ,VOY1")
                    sqlStat.AppendLine("      ,ETD1")
                    sqlStat.AppendLine("      ,ETA1")
                    sqlStat.AppendLine("      ,VSL2")
                    sqlStat.AppendLine("      ,VOY2")
                    sqlStat.AppendLine("      ,ETD2")
                    sqlStat.AppendLine("      ,ETA2")
                    sqlStat.AppendLine("      ,INVOICEDBY")
                    sqlStat.AppendLine("      ,LOADING")
                    sqlStat.AppendLine("      ,STEAMING")
                    sqlStat.AppendLine("      ,TIP")
                    sqlStat.AppendLine("      ,EXTRA")
                    sqlStat.AppendLine("      ,JOTHIREAGE")
                    sqlStat.AppendLine("      ,COMMERCIALFACTOR")
                    sqlStat.AppendLine("      ,AMTREQUEST")
                    sqlStat.AppendLine("      ,AMTPRINCIPAL")
                    sqlStat.AppendLine("      ,AMTDISCOUNT")
                    sqlStat.AppendLine("      ,DEMURTO")
                    sqlStat.AppendLine("      ,DEMURUSRATE1")
                    sqlStat.AppendLine("      ,DEMURUSRATE2")
                    sqlStat.AppendLine("      ,AGENTORGANIZER")
                    sqlStat.AppendLine("      ,AGENTPOL1")
                    sqlStat.AppendLine("      ,AGENTPOL2")
                    sqlStat.AppendLine("      ,AGENTPOD1")
                    sqlStat.AppendLine("      ,AGENTPOD2")
                    sqlStat.AppendLine("      ,APPLYTEXT")
                    sqlStat.AppendLine("      ,COUNTRYORGANIZER")
                    sqlStat.AppendLine("      ,LASTORDERNO")
                    sqlStat.AppendLine("      ,TANKNO")
                    sqlStat.AppendLine("      ,DEPOTCODE")
                    sqlStat.AppendLine("      ,TWOAGOPRODUCT")
                    sqlStat.AppendLine("      ,FEE")
                    sqlStat.AppendLine("      ,BILLINGCATEGORY")
                    sqlStat.AppendLine("      ,USINGLEASETANK")
                    sqlStat.AppendLine("      ,REPAIRDEPOINDATE")
                    sqlStat.AppendLine("      ,REPAIRBRID")
                    sqlStat.AppendLine("      ,REMARK")
                    sqlStat.AppendLine("      ,DELFLG")
                    sqlStat.AppendLine("      ,INITYMD ")
                    sqlStat.AppendLine("      ,INITUSER ")
                    sqlStat.AppendLine("      ,UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER ")
                    sqlStat.AppendLine("      ,UPDTERMID ")
                    sqlStat.AppendLine("      ,RECEIVEYMD ")
                    sqlStat.AppendLine(" ) ")
                    sqlStat.AppendLine("SELECT BRID")
                    sqlStat.AppendLine("      ,LEFT(BRBASEID,5) + RIGHT('00000' + CONVERT(varchar ,  CONVERT(int ,right(BRBASEID,5))+1), 5) AS BRBASEID ")
                    sqlStat.AppendLine("      ,@STYMD")
                    sqlStat.AppendLine("      ,VALIDITYFROM")
                    sqlStat.AppendLine("      ,VALIDITYTO")
                    sqlStat.AppendLine("      ,TERMTYPE")
                    sqlStat.AppendLine("      ,NOOFTANKS")
                    sqlStat.AppendLine("      ,SHIPPER")
                    sqlStat.AppendLine("      ,CONSIGNEE")
                    sqlStat.AppendLine("      ,CARRIER1")
                    sqlStat.AppendLine("      ,CARRIER2")
                    sqlStat.AppendLine("      ,PRODUCTCODE")
                    sqlStat.AppendLine("      ,PRODUCTWEIGHT")
                    sqlStat.AppendLine("      ,RECIEPTCOUNTRY1")
                    sqlStat.AppendLine("      ,RECIEPTPORT1")
                    sqlStat.AppendLine("      ,RECIEPTCOUNTRY2")
                    sqlStat.AppendLine("      ,RECIEPTPORT2")
                    sqlStat.AppendLine("      ,LOADCOUNTRY1")
                    sqlStat.AppendLine("      ,LOADPORT1")
                    sqlStat.AppendLine("      ,LOADCOUNTRY2")
                    sqlStat.AppendLine("      ,LOADPORT2")
                    sqlStat.AppendLine("      ,DISCHARGECOUNTRY1")
                    sqlStat.AppendLine("      ,DISCHARGEPORT1")
                    sqlStat.AppendLine("      ,DISCHARGECOUNTRY2")
                    sqlStat.AppendLine("      ,DISCHARGEPORT2")
                    sqlStat.AppendLine("      ,DELIVERYCOUNTRY1")
                    sqlStat.AppendLine("      ,DELIVERYPORT1")
                    sqlStat.AppendLine("      ,DELIVERYCOUNTRY2")
                    sqlStat.AppendLine("      ,DELIVERYPORT2")
                    sqlStat.AppendLine("      ,VSL1")
                    sqlStat.AppendLine("      ,VOY1")
                    sqlStat.AppendLine("      ,ETD1")
                    sqlStat.AppendLine("      ,ETA1")
                    sqlStat.AppendLine("      ,VSL2")
                    sqlStat.AppendLine("      ,VOY2")
                    sqlStat.AppendLine("      ,ETD2")
                    sqlStat.AppendLine("      ,ETA2")
                    sqlStat.AppendLine("      ,INVOICEDBY")
                    sqlStat.AppendLine("      ,LOADING")
                    sqlStat.AppendLine("      ,STEAMING")
                    sqlStat.AppendLine("      ,TIP")
                    sqlStat.AppendLine("      ,EXTRA")
                    sqlStat.AppendLine("      ,JOTHIREAGE")
                    sqlStat.AppendLine("      ,COMMERCIALFACTOR")
                    sqlStat.AppendLine("      ,AMTREQUEST")
                    sqlStat.AppendLine("      ,AMTPRINCIPAL")
                    sqlStat.AppendLine("      ,AMTDISCOUNT")
                    sqlStat.AppendLine("      ,DEMURTO")
                    sqlStat.AppendLine("      ,DEMURUSRATE1")
                    sqlStat.AppendLine("      ,DEMURUSRATE2")
                    sqlStat.AppendLine("      ,AGENTORGANIZER")
                    sqlStat.AppendLine("      ,AGENTPOL1")
                    sqlStat.AppendLine("      ,AGENTPOL2")
                    sqlStat.AppendLine("      ,AGENTPOD1")
                    sqlStat.AppendLine("      ,AGENTPOD2")
                    sqlStat.AppendLine("      ,APPLYTEXT")
                    sqlStat.AppendLine("      ,COUNTRYORGANIZER")
                    sqlStat.AppendLine("      ,LASTORDERNO")
                    sqlStat.AppendLine("      ,TANKNO")
                    sqlStat.AppendLine("      ,DEPOTCODE")
                    sqlStat.AppendLine("      ,TWOAGOPRODUCT")
                    sqlStat.AppendLine("      ,FEE")
                    sqlStat.AppendLine("      ,BILLINGCATEGORY")
                    sqlStat.AppendLine("      ,USINGLEASETANK")
                    sqlStat.AppendLine("      ,REPAIRDEPOINDATE")
                    sqlStat.AppendLine("      ,REPAIRBRID")
                    sqlStat.AppendLine("      ,REMARK")
                    sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'     ") '削除フラグ(0固定)
                    sqlStat.AppendLine("      ,@INITYMD")
                    sqlStat.AppendLine("      ,@INITUSER")
                    sqlStat.AppendLine("      ,@ENTDATE")
                    sqlStat.AppendLine("      ,@UPDUSER")
                    sqlStat.AppendLine("      ,@UPDTERMID")
                    sqlStat.AppendLine("      ,@RECEIVEYMD")
                    sqlStat.AppendLine("  FROM GBT0002_BR_BASE ")
                    sqlStat.AppendLine(" WHERE BRID     = @BRID")
                    sqlStat.AppendLine("   AND BRBASEID = @BRBASEID")

                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                        If tran IsNot Nothing Then
                            sqlCmd.Transaction = tran
                        End If
                        'SQLパラメータの設定
                        With sqlCmd.Parameters
                            .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                            .Add("@BRBASEID", SqlDbType.NVarChar, 20).Value = dt.Rows(i).Item("LINKID")
                            .Add("@STYMD", SqlDbType.Date).Value = entDate
                            .Add("@INITYMD", SqlDbType.DateTime).Value = Me.hdnInitYmd.Value
                            .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = Me.hdnInitUser.Value
                            .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                            .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        End With
                        sqlCmd.ExecuteNonQuery()
                    End Using

                Else

                    'BR_VALUE
                    sqlStat.Clear()
                    sqlStat.AppendLine("INSERT INTO GBT0003_BR_VALUE (")
                    sqlStat.AppendLine("       BRID")
                    sqlStat.AppendLine("      ,BRVALUEID")
                    sqlStat.AppendLine("      ,STYMD")
                    sqlStat.AppendLine("      ,DTLPOLPOD")
                    sqlStat.AppendLine("      ,DTLOFFICE")
                    sqlStat.AppendLine("      ,COSTCODE")
                    sqlStat.AppendLine("      ,BASEON")
                    sqlStat.AppendLine("      ,TAX")
                    sqlStat.AppendLine("      ,USD")
                    sqlStat.AppendLine("      ,LOCAL")
                    sqlStat.AppendLine("      ,CONTRACTOR")
                    sqlStat.AppendLine("      ,LOCALRATE")
                    sqlStat.AppendLine("      ,USDRATE")
                    sqlStat.AppendLine("      ,CURRENCYCODE")
                    sqlStat.AppendLine("      ,AGENT")
                    sqlStat.AppendLine("      ,ACTIONID")
                    sqlStat.AppendLine("      ,CLASS1")
                    sqlStat.AppendLine("      ,CLASS2")
                    sqlStat.AppendLine("      ,CLASS3")
                    sqlStat.AppendLine("      ,CLASS4")
                    sqlStat.AppendLine("      ,CLASS5")
                    sqlStat.AppendLine("      ,CLASS6")
                    sqlStat.AppendLine("      ,CLASS7")
                    sqlStat.AppendLine("      ,CLASS8")
                    sqlStat.AppendLine("      ,COUNTRYCODE")
                    sqlStat.AppendLine("      ,REPAIRFLG")
                    sqlStat.AppendLine("      ,APPROVEDUSD")
                    sqlStat.AppendLine("      ,INVOICEDBY")
                    sqlStat.AppendLine("      ,REMARK")
                    sqlStat.AppendLine("      ,DELFLG")
                    sqlStat.AppendLine("      ,INITYMD ")
                    sqlStat.AppendLine("      ,INITUSER ")
                    sqlStat.AppendLine("      ,UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER ")
                    sqlStat.AppendLine("      ,UPDTERMID ")
                    sqlStat.AppendLine("      ,RECEIVEYMD ")
                    sqlStat.AppendLine(" ) ")
                    sqlStat.AppendLine("SELECT BRID")
                    sqlStat.AppendLine("      ,LEFT(BRVALUEID,5) + RIGHT('00000' + CONVERT(varchar ,  CONVERT(int ,right(BRVALUEID,5))+1), 5) AS BRVALUEID ")
                    sqlStat.AppendLine("      ,@STYMD")
                    sqlStat.AppendLine("      ,DTLPOLPOD")
                    sqlStat.AppendLine("      ,DTLOFFICE")
                    sqlStat.AppendLine("      ,COSTCODE")
                    sqlStat.AppendLine("      ,BASEON")
                    sqlStat.AppendLine("      ,TAX")
                    sqlStat.AppendLine("      ,USD")
                    sqlStat.AppendLine("      ,LOCAL")
                    sqlStat.AppendLine("      ,CONTRACTOR")
                    sqlStat.AppendLine("      ,LOCALRATE")
                    sqlStat.AppendLine("      ,USDRATE")
                    sqlStat.AppendLine("      ,CURRENCYCODE")
                    sqlStat.AppendLine("      ,AGENT")
                    sqlStat.AppendLine("      ,ACTIONID")
                    sqlStat.AppendLine("      ,CLASS1")
                    sqlStat.AppendLine("      ,CLASS2")
                    sqlStat.AppendLine("      ,CLASS3")
                    sqlStat.AppendLine("      ,CLASS4")
                    sqlStat.AppendLine("      ,CLASS5")
                    sqlStat.AppendLine("      ,CLASS6")
                    sqlStat.AppendLine("      ,CLASS7")
                    sqlStat.AppendLine("      ,CLASS8")
                    sqlStat.AppendLine("      ,COUNTRYCODE")
                    sqlStat.AppendLine("      ,REPAIRFLG")
                    sqlStat.AppendLine("      ,APPROVEDUSD")
                    sqlStat.AppendLine("      ,INVOICEDBY")
                    sqlStat.AppendLine("      ,REMARK")
                    sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'     ") '削除フラグ(0固定)
                    sqlStat.AppendLine("      ,@INITYMD")
                    sqlStat.AppendLine("      ,@INITUSER")
                    sqlStat.AppendLine("      ,@ENTDATE")
                    sqlStat.AppendLine("      ,@UPDUSER")
                    sqlStat.AppendLine("      ,@UPDTERMID")
                    sqlStat.AppendLine("      ,@RECEIVEYMD")
                    sqlStat.AppendLine("  FROM GBT0003_BR_VALUE ")
                    sqlStat.AppendLine(" WHERE BRID      = @BRID")
                    sqlStat.AppendLine("   AND BRVALUEID = @BRVALUEID")

                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                        If tran IsNot Nothing Then
                            sqlCmd.Transaction = tran
                        End If
                        'SQLパラメータの設定
                        With sqlCmd.Parameters
                            .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                            .Add("@BRVALUEID", SqlDbType.NVarChar, 20).Value = dt.Rows(i).Item("LINKID")
                            .Add("@STYMD", SqlDbType.Date).Value = entDate
                            .Add("@INITYMD", SqlDbType.DateTime).Value = Me.hdnInitYmd.Value
                            .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = Me.hdnInitUser.Value
                            .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                            .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        End With
                        sqlCmd.ExecuteNonQuery()
                    End Using

                End If
            Next

        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' タンク使用可否ダブルクリック処理
    ''' </summary>
    Public Sub txtTankUsage_DbClick()
        Const TANK_MAST_VARI As String = "GB_ShowDetail"
        If Me.txtTankNo.Text = "" Then
            Return
        End If
        Me.hdnMsgId.Value = ""
        Dim thisDisplayItems As GBT00012RITEMS = GetGbt00012items()
        Me.DisplayItems = thisDisplayItems

        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ 画面遷移先URL取得 ■■■
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = TANK_MAST_VARI
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        Dim url As String = COA0012DoUrl.URL
        HttpContext.Current.Session("MAPvariant") = TANK_MAST_VARI
        '画面遷移実行
        Server.Transfer(url)
    End Sub

    ''' <summary>
    ''' 当画面の保持必要情報を保持し退避用クラスを生成
    ''' </summary>
    ''' <returns></returns>
    Public Function GetGbt00012items() As GBT00012RITEMS
        Dim item As New GBT00012RITEMS

        item.StYMD = Me.hdnStYMD.Value
        item.EndYMD = Me.hdnEndYMD.Value
        item.Shipper = Me.hdnShipper.Value
        item.Consignee = Me.hdnConsignee.Value
        item.Port = Me.hdnPort.Value
        item.Approval = Me.hdnApproval.Value
        item.Office = Me.hdnOffice.Value
        item.MsgId = Me.hdnMsgId.Value
        item.ApprovalFlg = Me.hdnApprovalFlg.Value '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
        item.AppTranFlg = Me.hdnAppTranFlg.Value '承認済画面から来たフラグ(1:承認画面からの遷移)
        item.ApprovalObj = Me.hdnApprovalObj.Value
        item.Status = Me.hdnStatus.Value
        item.TankNo = Me.hdnTankNo.Value
        item.Depot = Me.hdnDepot.Value
        item.PrevViewID = Me.hdnPrevViewID.Value
        item.XMLsaveFileRet = Me.hdnXMLsaveFileRet.Value
        item.AlreadyFlg = Me.hdnAlreadyFlg.Value  '承認済フラグ(承認画面の遷移時のみ設定 1:承認済)
        item.DelFlg = Me.hdnDelFlg.Value '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
        item.BrId = Me.lblBrNo.Text
        item.SubId = Me.hdnSubId.Value
        item.LinkId = Me.hdnLinkId.Value
        item.ApplyId = Me.hdnApplyId.Value
        item.Cstep = Me.hdnStep.Value
        item.LastStep = Me.hdnLastStep.Value
        item.MapVariant = Me.hdnThisMapVariant.Value
        item.ReportVariant = Me.hdnReportVariant.Value
        item.GBT00012STankNo = Me.hdnGBT00012STankNo.Value
        item.Location = Me.hdnLocation.Value
        item.LastCargo = Me.hdnLastCargo.Value
        item.dicInitDatatables = DirectCast(ViewState("INITORGANIZERINFO"), DataTable)
        item.dicInitBrInfo = DirectCast(ViewState("INITDICBRINFO"), Dictionary(Of String, BreakerInfo))
        item.lstInitCostList = DirectCast(ViewState("INITCOSTLIST"), List(Of COSTITEM))

        item.dicCurrentDatatables = CollectDisplayOrganizerInfo()
        item.dicCurrentBrInfo = DirectCast(ViewState(CONST_VS_NAME_DICBRINFO), Dictionary(Of String, BreakerInfo))
        item.lstCurrentCostList = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))

        Return item
    End Function
    ''' <summary>
    ''' 退避情報を画面に戻す
    ''' </summary>
    ''' <param name="item"></param>
    Private Sub SetGbt00012items(item As GBT00012RITEMS)

        Me.DisplayItems = item
        Me.hdnStYMD.Value = item.StYMD
        Me.hdnEndYMD.Value = item.EndYMD
        Me.hdnShipper.Value = item.Shipper
        Me.hdnConsignee.Value = item.Consignee
        Me.hdnPort.Value = item.Port
        Me.hdnApproval.Value = item.Approval
        Me.hdnOffice.Value = item.Office
        Me.hdnMsgId.Value = item.MsgId
        Me.hdnApprovalFlg.Value = item.ApprovalFlg '承認フラグ(1:申請中・承認済み・申請画面or履歴画面から来た場合は無条件)
        Me.hdnAppTranFlg.Value = item.AppTranFlg '承認済画面から来たフラグ(1:承認画面からの遷移)
        Me.hdnApprovalObj.Value = item.ApprovalObj
        Me.hdnStatus.Value = item.Status
        Me.hdnTankNo.Value = item.TankNo
        Me.hdnDepot.Value = item.Depot
        Me.hdnPrevViewID.Value = item.PrevViewID
        Me.hdnXMLsaveFileRet.Value = item.XMLsaveFileRet
        Me.hdnAlreadyFlg.Value = item.AlreadyFlg  '承認済フラグ(承認画面の遷移時のみ設定 1:承認済)
        Me.hdnDelFlg.Value = item.DelFlg '削除済みフラグ（もはや体をなしていないので読み取りフラグとするY:読取,N:解放）
        Me.hdnBrId.Value = item.BrId
        Me.hdnSubId.Value = item.SubId
        Me.hdnLinkId.Value = item.LinkId
        Me.hdnApplyId.Value = item.ApplyId
        Me.hdnStep.Value = item.Cstep
        Me.hdnLastStep.Value = item.LastStep
        Me.hdnThisMapVariant.Value = item.MapVariant
        Me.hdnReportVariant.Value = item.ReportVariant
        Me.hdnGBT00012STankNo.Value = item.GBT00012STankNo
        Me.hdnLocation.Value = item.Location
        Me.hdnLastCargo.Value = item.LastCargo
        ViewState("INITORGANIZERINFO") = item.dicInitDatatables
        ViewState("INITDICBRINFO") = item.dicInitBrInfo
        ViewState("INITCOSTLIST") = item.lstInitCostList
        If item.dicCurrentDatatables IsNot Nothing AndAlso item.dicCurrentDatatables.Rows.Count > 0 Then
            With item.dicCurrentDatatables.Rows(0)
                Me.lblBrNo.Text = Convert.ToString(.Item("BRID"))
                Me.lblBrRemarkText.Text = HttpUtility.HtmlEncode(Convert.ToString(.Item("REMARK")))
                Me.lblApplyRemarks.Text = HttpUtility.HtmlEncode(Convert.ToString(.Item("APPLYTEXT")))
                Me.lblAppJotRemarks.Text = HttpUtility.HtmlEncode(Convert.ToString(.Item("APPROVEDTEXT")))
                If Convert.ToString(.Item("APPLYDATE")) <> "" Then
                    Me.txtAppRequestYmd.Text = Date.Parse(Convert.ToString(.Item("APPLYDATE"))).ToString(GBA00003UserSetting.DATEFORMAT)
                Else
                    Me.txtAppRequestYmd.Text = Convert.ToString(.Item("APPLYDATE")) 'Apply Date
                End If
                Me.txtAppSalesPic.Text = Convert.ToString(.Item("APPLICANTID"))
                Me.lblAppSalesPicText.Text = HttpUtility.HtmlEncode(Convert.ToString(.Item("APPLICANTNAME")))

                If Convert.ToString(.Item("APPROVEDATE")) <> "" Then
                    Me.txtApprovedYmd.Text = Date.Parse(Convert.ToString(.Item("APPROVEDATE"))).ToString(GBA00003UserSetting.DATEFORMAT)
                Else
                    Me.txtApprovedYmd.Text = Convert.ToString(.Item("APPROVEDATE")) 'Approved Date
                End If
                Me.txtAppJotPic.Text = Convert.ToString(.Item("APPROVERID"))
                Me.lblAppJotPicText.Text = HttpUtility.HtmlEncode(Convert.ToString(.Item("APPROVERNAME")))

                'リペア
                Me.txtTankNo.Text = Convert.ToString(.Item("TANKNO"))
                Me.txtDepoCode.Text = Convert.ToString(.Item("DEPOTCODE"))
                Me.lblDepoCodeText.Text = Convert.ToString(.Item("DEPOTNAME"))
                Me.txtLocation.Text = Convert.ToString(.Item("LOCATION"))
                If Convert.ToString(.Item("REPAIRDEPOINDATE")) <> "" Then
                    Me.txtDepoInDate.Text = Date.Parse(Convert.ToString(.Item("REPAIRDEPOINDATE"))).ToString(GBA00003UserSetting.DATEFORMAT)
                Else
                    Me.txtDepoInDate.Text = Convert.ToString(.Item("REPAIRDEPOINDATE")) 'Approved Date
                End If

                Me.txtBreakerNo.Text = Convert.ToString(.Item("REPAIRBRID"))
                Me.txtLastProduct.Text = Convert.ToString(.Item("LASTPRODUCT"))
                Me.lblLastProductText.Text = Convert.ToString(.Item("PRODUCTNAME"))
                Me.txtTwoAgoProduct.Text = Convert.ToString(.Item("TWOAGOPRODUCT"))
                Me.lblTwoAgoProductText.Text = Convert.ToString(.Item("TWOAGOPRODUCTNAME"))
                Me.txtLastOrderNo.Text = Convert.ToString(.Item("LASTORDERNO"))
                Me.txtDeleteFlag.Text = Convert.ToString(.Item("DELFLG"))
                Me.txtTankUsage.Text = Convert.ToString(.Item("TANKUSAGE"))
                Me.txtSettlementOffice.Text = Convert.ToString(.Item("AGENTORGANIZER"))
                Me.lblSettlementOfficeText.Text = Convert.ToString(.Item("OFFICENAME"))

                Me.lblRemarks.Text = HttpUtility.HtmlEncode(Convert.ToString(.Item("SPECIALINS")))

                'オーガナイザ国
                Me.hdnCountryOrganizer.Value = Convert.ToString(.Item("COUNTRYORGANIZER"))
                If .Item("USINGLEASETANK").Equals("1") Then
                    Me.chkLeaseCheck.Checked = True
                Else
                    Me.chkLeaseCheck.Checked = False
                End If

            End With
        End If
        ViewState(CONST_VS_NAME_DICBRINFO) = item.dicCurrentBrInfo
        ViewState(CONST_VS_NAME_COSTLIST) = item.lstCurrentCostList
    End Sub

    ''' <summary>
    ''' GBT000012画面情報退避用クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00012RITEMS
        Public Property StYMD As String
        Public Property EndYMD As String
        Public Property Shipper As String
        Public Property Consignee As String
        Public Property Port As String
        Public Property Approval As String
        Public Property Office As String
        Public Property IsViewOnlyPopup As String
        Public Property MsgId As String
        Public Property ApprovalFlg As String
        Public Property AppTranFlg As String
        Public Property ApprovalObj As String
        Public Property Status As String
        Public Property TankNo As String
        Public Property Depot As String
        Public Property PrevViewID As String
        Public Property XMLsaveFileRet As String
        Public Property AlreadyFlg As String
        Public Property DelFlg As String
        Public Property BrId As String
        Public Property LinkId As String
        Public Property SubId As String
        Public Property ApplyId As String
        Public Property Cstep As String
        Public Property LastStep As String
        Public Property MapVariant As String
        Public Property ReportVariant As String
        Public Property GBT00012STankNo As String
        Public Property LastCargo As String
        Public Property Location As String
        Public Property dicInitBrInfo As New Dictionary(Of String, BreakerInfo)
        Public Property dicInitDatatables As DataTable
        Public Property lstInitCostList As New List(Of COSTITEM)
        Public Property dicCurrentDatatables As DataTable
        Public Property dicCurrentBrInfo As New Dictionary(Of String, BreakerInfo)
        Public Property lstCurrentCostList As New List(Of COSTITEM)
    End Class

    ''' <summary>
    ''' テキスト変更チェック
    ''' </summary>
    Public Sub TextChangeCheck()

        hdnMsgboxShowFlg.Value = "0"

        Dim orgDt As DataTable = DirectCast(ViewState("INITORGANIZERINFO"), DataTable)
        Dim initBrInfo As Dictionary(Of String, BreakerInfo) = Nothing
        initBrInfo = DirectCast(ViewState("INITDICBRINFO"), Dictionary(Of String, BreakerInfo))

        Dim initAllCostList As List(Of COSTITEM)
        initAllCostList = DirectCast(ViewState("INITCOSTLIST"), List(Of COSTITEM))

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState(CONST_VS_NAME_COSTLIST), List(Of COSTITEM))

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing
        brInfo = DirectCast(ViewState(CONST_VS_NAME_DICBRINFO), Dictionary(Of String, BreakerInfo))

        If initBrInfo.ContainsKey("INFO") Then
            Dim initBrInfoItem = initBrInfo("INFO")
            Dim brInfoItem = brInfo("INFO")

            If brInfoItem.Remark <> initBrInfoItem.Remark Then
                hdnMsgboxShowFlg.Value = "1"
                Return
            End If
        End If

        'Organizer
        Dim dicChk As New Dictionary(Of String, String) From {
            {"TANKNO", Me.txtTankNo.Text}, {"DELFLG", Me.txtDeleteFlag.Text},
            {"TANKUSAGE", Me.txtTankUsage.Text}, {"REMARK", HttpUtility.HtmlDecode(Me.lblBrRemarkText.Text)},
            {"APPLYTEXT", HttpUtility.HtmlDecode(Me.lblApplyRemarks.Text)}, {"APPROVEDTEXT", HttpUtility.HtmlDecode(Me.lblAppJotRemarks.Text)},
            {"DEPOTCODE", Me.txtDepoCode.Text}, {"REPAIRDEPOINDATE", BaseDllCommon.FormatDateYMD(Me.txtDepoInDate.Text, GBA00003UserSetting.DATEFORMAT)},
            {"REPAIRBRID", Me.txtBreakerNo.Text}, {"LASTORDERNO", Me.txtLastOrderNo.Text},
            {"LASTPRODUCT", Me.txtLastProduct.Text}, {"TWOAGOPRODUCT", Me.txtTwoAgoProduct.Text}
        }
        For Each item As KeyValuePair(Of String, String) In dicChk
            If item.Value <> Convert.ToString(orgDt.Rows(0).Item(item.Key)) Then
                hdnMsgboxShowFlg.Value = "1"
                Return
            End If
        Next

        'Cost
        Dim showInitCostList As List(Of COSTITEM)
        showInitCostList = (From initAllCostItem In initAllCostList
                            Order By initAllCostItem.IsAddedCost, Convert.ToInt32(If(initAllCostItem.Class2 = "", "0", initAllCostItem.Class2))).ToList

        Dim showCostList As List(Of COSTITEM)
        showCostList = (From allCostItem In allCostList
                        Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList

        If showInitCostList.Count <> showCostList.Count Then
            hdnMsgboxShowFlg.Value = "1"
            Return
        End If

        For Each row In showInitCostList

            For Each sRow In showCostList
                sRow.LocalCurrncyRate = cnvInt(sRow.LocalCurrncyRate)
                sRow.Local = cnvInt(sRow.Local)
                sRow.USD = cnvInt(sRow.USD)
                sRow.ApprovedUsd = cnvInt(sRow.ApprovedUsd)
            Next
            Dim compRow = CommonFunctions.DeepCopy(row)
            compRow.LocalCurrncyRate = cnvInt(compRow.LocalCurrncyRate)
            compRow.Local = cnvInt(compRow.Local)
            compRow.USD = cnvInt(compRow.USD)
            compRow.ApprovedUsd = cnvInt(compRow.ApprovedUsd)
            Dim matchRec = From item In showCostList Where item.AllPropertyEquals(compRow)
            If matchRec.Any = False Then
                hdnMsgboxShowFlg.Value = "1"
                Return
            End If

        Next
        'データテーブルとチェックフィールド
        Dim dicModCheck As New Dictionary(Of List(Of DataTable), List(Of String))
        Dim prevBeforeAttachDt As DataTable = DirectCast(ViewState(CONST_VS_NAME_BEFORE_PREV_VAL), DataTable)
        dicModCheck.Add(New List(Of DataTable) From {Me.BeforeRepairAttachment, prevBeforeAttachDt},
                        New List(Of String) From {"FILENAME", "DELFLG", "ISMODIFIED"})

        '添付ファイルの個数判定
        For Each dicModItem In dicModCheck
            Dim dispAttachFileCnt As Integer = 0
            Dim prevAttachFileCnt As Integer = 0

            Dim dispAttachDt = dicModItem.Key(0)
            Dim prevAttachDt = dicModItem.Key(1)
            If dispAttachDt IsNot Nothing Then
                dispAttachFileCnt = dispAttachDt.Rows.Count
            End If
            If prevAttachDt IsNot Nothing Then
                prevAttachFileCnt = prevAttachDt.Rows.Count
            End If
            If prevAttachFileCnt <> dispAttachFileCnt Then
                '添付ファイルの数値が合わない場合は変更あり
                hdnMsgboxShowFlg.Value = "1"
                Return
            End If

            Dim maxRowIdx As Integer = dispAttachDt.Rows.Count - 1
            For rowIdx = 0 To maxRowIdx
                Dim dispDr As DataRow = dispAttachDt.Rows(rowIdx)
                Dim prevDr As DataRow = prevAttachDt.Rows(rowIdx)
                For Each fieldName In dicModItem.Value
                    If Not dispDr(fieldName).Equals(prevDr(fieldName)) Then
                        '対象フィールドの値に変更があった場合
                        hdnMsgboxShowFlg.Value = "1"
                        Return
                    End If
                Next fieldName 'フィールドループ
            Next 'データテーブル行ループ

        Next

        Dim dicAftModCheck As New Dictionary(Of List(Of DataTable), List(Of String))
        Dim prevAfterAttachDt As DataTable = DirectCast(ViewState(CONST_VS_NAME_AFTER_PREV_VAL), DataTable)
        dicAftModCheck.Add(New List(Of DataTable) From {Me.AfterRepairAttachment, prevAfterAttachDt},
                        New List(Of String) From {"FILENAME", "DELFLG", "ISMODIFIED"})

        '添付ファイルの個数判定
        For Each dicModItem In dicAftModCheck
            Dim dispAttachFileCnt As Integer = 0
            Dim prevAttachFileCnt As Integer = 0

            Dim dispAttachDt = dicModItem.Key(0)
            Dim prevAttachDt = dicModItem.Key(1)
            If dispAttachDt IsNot Nothing Then
                dispAttachFileCnt = dispAttachDt.Rows.Count
            End If
            If prevAttachDt IsNot Nothing Then
                prevAttachFileCnt = prevAttachDt.Rows.Count
            End If
            If prevAttachFileCnt <> dispAttachFileCnt Then
                '添付ファイルの数値が合わない場合は変更あり
                hdnMsgboxShowFlg.Value = "1"
                Return
            End If

            Dim maxRowIdx As Integer = dispAttachDt.Rows.Count - 1
            For rowIdx = 0 To maxRowIdx
                Dim dispDr As DataRow = dispAttachDt.Rows(rowIdx)
                Dim prevDr As DataRow = prevAttachDt.Rows(rowIdx)
                For Each fieldName In dicModItem.Value
                    If Not dispDr(fieldName).Equals(prevDr(fieldName)) Then
                        '対象フィールドの値に変更があった場合
                        hdnMsgboxShowFlg.Value = "1"
                        Return
                    End If
                Next fieldName 'フィールドループ
            Next 'データテーブル行ループ

        Next

        Return

    End Sub

    ''' <summary>
    ''' 数値変換
    ''' </summary>
    ''' <returns></returns>
    Private Function cnvInt(ByVal cnvTxt As String) As String

        Dim retVal As String = ""

        If IsNumeric(cnvTxt) Then
            retVal = Convert.ToString(Convert.ToDouble(cnvTxt))
        Else
            retVal = cnvTxt
        End If

        Return retVal
    End Function

    ''' <summary>
    ''' LIST登録チェック
    ''' </summary>
    ''' <param name="inText"></param>
    ''' <param name="inList"></param>
    Protected Function ChedckList(ByVal inText As String, ByVal inList As ListBox, ByVal textNm As String, ByRef errMessage As String) As String
        Dim flag As Boolean = False
        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
        Dim retMessage As New StringBuilder

        If inText <> "" Then

            For i As Integer = 0 To inList.Items.Count - 1
                If inList.Items(i).Value = inText Then
                    flag = True
                    Exit For
                End If
            Next

            If (flag = False) Then
                retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                CommonFunctions.ShowMessage(C_MESSAGENO.INVALIDINPUT, dummyLabelObj)
                retMessage.AppendFormat("・{0}：{1}", textNm, dummyLabelObj.Text).AppendLine()
            End If
        End If

        errMessage = retMessage.ToString
        Return retMessageNo
    End Function

    ''' <summary>
    ''' ステータス取得
    ''' </summary>
    ''' <param name="sqlCon">オプション 項目</param>
    ''' <returns>ステータス</returns>
    Private Function GetStatus(ByVal brid As String, Optional sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim retSt As String = ""

        If brid = "" Then
            Return retSt
        End If

        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("SELECT TRIM(AH.STATUS) as STATUS ")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
        sqlStat.AppendLine("  INNER JOIN COT0002_APPROVALHIST AH")
        sqlStat.AppendLine("    ON BI.APPLYID    = AH.APPLYID ")
        sqlStat.AppendLine("   AND BI.LASTSTEP   =  AH.STEP ")
        sqlStat.AppendLine(" WHERE BI.BRID    =  @BRID")
        sqlStat.AppendLine("   AND BI.DELFLG <> @DELFLG")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                With sqlCmd.Parameters
                    'SQLパラメータ設定
                    .Add("@BRID", SqlDbType.NVarChar).Value = brid
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                        retSt = Convert.ToString(dt.Rows(0).Item(0))
                    End If
                End Using

            End Using
            Return retSt
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
    ''' 画面上で選択されているタブを取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetCurrentTab(Optional defVal As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Repair) As COSTITEM.CostItemGroup
        Dim currentTab As COSTITEM.CostItemGroup = defVal
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)

        tabObjects.Add(COSTITEM.CostItemGroup.Repair, Me.tabRepair)
        tabObjects.Add(COSTITEM.CostItemGroup.FileUp, Me.tabFileUp)
        tabObjects.Add(COSTITEM.CostItemGroup.DoneFileUp, Me.tabDoneFileUp)

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected") Then
                currentTab = tabObject.Key
                Exit For
            End If
        Next
        Return currentTab
    End Function
End Class