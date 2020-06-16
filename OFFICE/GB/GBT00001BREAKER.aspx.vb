Imports System.Data.SqlClient
Imports System.Net
Imports BASEDLL

''' <summary>
''' ブレーカー単票画面クラス
''' </summary>
Public Class GBT00001BREAKER
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00001" '自身のMAPID
    Private Const CONST_APP_MAPID As String = "GBT00005A" '承認画面のMAPID
    Private Const PRODUCT_NONDG As String = "NON-DG"
    Private Const PRODUCT_TP33 As String = "TP33"
    Private Const BEFORE_SAVE_MSG As String = "Do you want to execute it after saving?"
    Private Const CONST_VS_DISP_POLONLY As String = "DISP_POLONLY"
    Private Const CONST_VS_CHANGE_PORTCODE As String = "CHANGE_PORTCODE"
    Private Const CONST_VS_CHANGE_PORTTEXTID As String = "CHANGE_PORTTEXTID"
    Private PreProcType As String = ""
    'VIEWSTATE名
    Private Const CONST_VS_NAME_GBT00002RV As String = "GBT00002RValues"

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile

    ''' <summary>
    ''' ブレーカー検索結果画面情報
    ''' </summary>
    ''' <returns></returns>
    Public Property GBT00002RValues As GBT00002RESULT.GBT00002RValues
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
                Me.Form.Attributes.Add("data-profid", If(GBA00003UserSetting.IS_JPOPERATOR, "JpOperation", "default"))
                '****************************************
                '遷移元よりデータ取得
                '****************************************
                Dim ds As DataSet = New DataSet
                If Me.GetPrevDisplayInfo(ds) <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                '費用一覧を変更可能な一時リスト変数に可能
                Dim costList As List(Of COSTITEM) = Me.CreateTemporaryCostList(ds.Tables("COST_INFO"), ds.Tables("ORGANIZER_INFO"))
                'VIEWSTATEにコスト情報を保存
                ViewState("COSTLIST") = costList
                'POL表示のみかの判定をVIEWSTATEに追加
                Dim qPolOnly = From cItm In costList Where cItm.ItemGroup = COSTITEM.CostItemGroup.Inport1
                If qPolOnly.Any = True Then
                    ViewState(CONST_VS_DISP_POLONLY) = "0"
                Else
                    ViewState(CONST_VS_DISP_POLONLY) = "1"
                End If
                '初期情報保持
                ViewState("INITORGANIZERINFO") = ds.Tables("ORGANIZER_INFO")
                If ViewState("COPYORGANIZERINFO") IsNot Nothing Then
                    ViewState("INITORGANIZERINFO") = ViewState("COPYORGANIZERINFO")
                End If
                ViewState("INITDICBRINFO") = ViewState("DICBRINFO")
                ViewState("INITCOSTLIST") = ViewState("COSTLIST")
                'SetDisplayTotalCost(True)

                Me.hdnEnableControl.Value = GetEnableControl()
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                If Me.hdnBrType.Value = "1" Then
                    COA0031ProfMap.VARIANTP = "SalesBreaker"
                Else
                    COA0031ProfMap.VARIANTP = "OperationBreaker"
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
                '固定右ボックス選択肢
                '****************************************
                SetTermListItem()
                SetBillingCategoryListItem()
                '****************************************
                '取得データを画面展開
                '****************************************
                '保持項目設定
                SetInitData(ds.Tables("ORGANIZER_INFO"))
                'オーナー情報
                SetDisplayOrganizerInfo(ds.Tables("ORGANIZER_INFO"))
                SetDisplayTotalCost(True)
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'タブによる表示切替
                '****************************************
                'organizerタブを選択状態にする(権限によって使い分け想定)
                If Me.hdnCostSelectedTabId.Value <> "" Then
                    Me.tabOrganizer.Attributes.Add("class", "selected")
                    Me.hdnSelectedTabId.Value = Me.hdnCostSelectedTabId.Value
                    visibleControl(True, Me.hdnCostSelectedTabId.Value)
                    TabClick(Me.hdnSelectedTabId.Value)
                    SetCountryControl(Me.hdnSelectedTabId.Value)
                Else
                    Me.tabOrganizer.Attributes.Add("class", "selected")
                    Me.hdnSelectedTabId.Value = Me.tabOrganizer.ClientID
                    visibleControl(True, Me.tabOrganizer.ClientID)
                    SetCountryControl(Me.tabOrganizer.ClientID)
                    '右ボックス帳票タブ
                    Dim errMsg As String = ""
                    errMsg = Me.RightboxInit(True, COSTITEM.CostItemGroup.Organizer)
                    If errMsg <> "" Then
                        'Me.lblFooterMessage.Text = errMsg
                    End If
                End If
                Dim org As Boolean = True
                If Me.hdnSelectedTabId.Value <> Me.tabOrganizer.ClientID Then
                    org = False
                End If
                enabledControls(org)
                setChkInit()


                '初回自動計算
                CalcDemurrageDay()
                CalcTotalDays(True)
                CalcFillingRate()
                CalcSummaryCostLocal()
                CalcSummaryCostUsd()
                CostEnabledControls()
                CalcInvoiceTotal() '一旦InvoiceTotalは初回ロードじ計算

                'メッセージ設定
                If Me.hdnMsgId.Value <> "" Then
                    CommonFunctions.ShowMessage(hdnMsgId.Value, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                End If
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                Me.GBT00002RValues = DirectCast(ViewState(CONST_VS_NAME_GBT00002RV), GBT00002RESULT.GBT00002RValues)
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
                enabledControls(If(Me.hdnSelectedTabId.Value = Me.tabOrganizer.ID, True, False))
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
                    'DeleteCostItem(uniqueIndex)
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
                        Me.btnRemarkInputEdit.Visible = False
                    ElseIf Me.hdnRemarkboxField.Value = "lblRemarks" OrElse Me.hdnRemarkboxField.Value = "lblRemarks2" Then
                        If Me.hdnRemarkInitFlg.Value = "" Then
                            ViewState("DICBRINFO_REM") = ViewState("DICBRINFO")
                            Me.hdnRemarkInitFlg.Value = "1"
                        End If
                        If Me.hdnRemarkFlg.Value <> "1" Then
                            '結合Remark
                            Me.btnRemarkInputOk.Disabled = Not targetControl.Enabled
                            Me.btnRemarkInputEdit.Disabled = Not targetControl.Enabled
                            Me.txtRemarkInput.ReadOnly = True
                            Me.btnRemarkInputEdit.Visible = True
                            Me.hdnRemarkboxFieldName.Value = "SPECIAL INSTRUCTIONS"

                            Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
                            If ViewState("DICBRINFO_REM") IsNot Nothing Then
                                brInfo = DirectCast(ViewState("DICBRINFO_REM"), Dictionary(Of String, BreakerInfo))
                            End If

                            Dim combRem As String = Nothing
                            For Each keyString As String In {"INFO", "POL1", "POD1", "POL2", "POD2"}
                                If brInfo.ContainsKey(keyString) Then
                                    Dim brInfoItem = brInfo(keyString)
                                    brInfoItem.Remark.Replace(vbCrLf, vbCrLf & "  ")

                                    combRem = combRem & "【" & If(keyString = "INFO", "ORGANIZER", keyString) & "】" & vbCrLf
                                    combRem = combRem & "  " & brInfoItem.Remark.Replace(vbCrLf, vbCrLf & "  ")
                                    combRem = combRem & vbCrLf
                                End If

                            Next

                            Me.txtRemarkInput.Text = combRem
                        End If

                    Else
                        If targetControl.Enabled = False Then
                            Me.btnRemarkInputOk.Disabled = True
                            Me.txtRemarkInput.ReadOnly = True
                            Me.btnRemarkInputEdit.Visible = False
                        Else
                            Me.btnRemarkInputOk.Disabled = False
                            Me.txtRemarkInput.ReadOnly = False
                            Me.btnRemarkInputEdit.Visible = False
                        End If
                        Me.txtRemarkInput.Text = HttpUtility.HtmlDecode(targetControl.Text)
                    End If
                    'マルチライン入力ボックスの表示
                    Me.divRemarkInputBoxWrapper.Style("display") = "block"
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
                    End If

                    Me.hdnListUpload.Value = ""
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
            'JPY参考値表示処理
            SetHireageJpy()
        Catch ex As Threading.ThreadAbortException
            Return
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.SYSTEMADM)})

            Dim additonalErrorMessage As String = ControlChars.CrLf & "ボタン：{0}、遷移画面ID:{1}、ブレーカーID(現(2回目))：{2}({3})、初回ロード分岐：{4}"
            Dim prevPageObj = Me.PreviousPage
            Dim formId As String = ""
            Dim prevBrId As String = ""
            If prevPageObj IsNot Nothing Then
                formId = prevPageObj.Form.ID
                If TypeOf prevPageObj Is GBT00001BREAKER Then
                    prevBrId = DirectCast(prevPageObj, GBT00001BREAKER).lblBrNo.Text
                End If
            End If
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString() & String.Format(additonalErrorMessage, Me.hdnButtonClick.Value, formId, Me.lblBrNo.Text, prevBrId, Me.PreProcType)
            COA0003LogFile.MESSAGENO = C_MESSAGENO.SYSTEMADM
            COA0003LogFile.COA0003WriteLog()

            Return
        Finally
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        End Try
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
    ''' 左ビュー表示処理
    ''' </summary>
    Private Sub DisplayLeftView()

        Dim GBA00004CountryRelated As GBA00004CountryRelated = New GBA00004CountryRelated
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
                '国ビュー表示
                Case Me.vLeftCountry.ID
                    Dim dt As DataTable = GetCountry()
                    With Me.lbCountry
                        .DataSource = dt
                        .DataTextField = "NAME"
                        .DataValueField = "COUNTRYCODE"
                        .DataBind()
                        .Focus()
                    End With
                    '入力済のデータを選択状態にする
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    If dblClickField IsNot Nothing AndAlso lbCountry.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbCountry.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
                Case Me.vLeftCarrier.ID
                    Dim countryCode As String = ""
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    If dblClickField.ID = Me.txtCarrier1.ID Then
                        countryCode = Me.txtLoadCountry1.Text.Trim
                    Else
                        countryCode = Me.txtLoadCountry2.Text.Trim
                    End If
                    Dim dt As DataTable = GetCarrier(countryCode)
                    With Me.lbCarrier
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With
                    '入力済のデータを選択状態にする
                    If dblClickField IsNot Nothing AndAlso lbCarrier.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbCarrier.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
                Case vLeftConsignee.ID
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    Dim countryCode As String = Me.txtDeliveryCountry1.Text
                    If Me.hdnBrType.Value = "1" Then
                        'SALESの場合
                        Dim dt As DataTable = GetConsignee(countryCode)
                        With Me.lbConsignee
                            .DataSource = dt
                            .DataTextField = "LISTBOXNAME"
                            .DataValueField = "CUSTOMERCODE"
                            .DataBind()
                            .Focus()
                        End With
                    Else
                        'OPEの場合
                        Dim dt As DataTable = GetAgent(countryCode)
                        With Me.lbConsignee
                            .DataSource = dt
                            .DataTextField = "LISTBOXNAME"
                            .DataValueField = "CODE"
                            .DataBind()
                            .Focus()
                        End With
                    End If

                    '入力済のデータを選択状態にする
                    If dblClickField IsNot Nothing AndAlso lbConsignee.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbConsignee.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
                Case Me.vLeftProduct.ID
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    Dim dt As DataTable = GetProduct()
                    With Me.lbProduct
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With
                    '入力済のデータを選択状態にする
                    If dblClickField IsNot Nothing AndAlso lbProduct.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbProduct.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
                Case vLeftCost.ID
                    Dim dt As DataTable = GetCost(Me.hdnBrType.Value, "", Me.hdnSelectedTabId.Value) 'TODO一旦セールスのみなので用オペブレ時対応
                    With Me.lbCost
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With
                Case vLeftContractor.ID

                    Dim countryCode As String = ""
                    'カレントタブに応じ取得する国コードを取得
                    Dim tabObjects As New List(Of HtmlControl) From {Me.tabInport1, Me.tabInport2, Me.tabExport1, Me.tabExport2}
                    For Each tabObject In tabObjects
                        If tabObject.Attributes("class") IsNot Nothing AndAlso tabObject.Attributes("class").Contains("selected") Then
                            Select Case tabObject.ID
                                Case Me.tabExport1.ID
                                    countryCode = Me.txtRecieptCountry1.Text
                                Case Me.tabInport1.ID
                                    countryCode = Me.txtDischargeCountry1.Text
                                Case Me.tabExport2.ID
                                    countryCode = Me.txtRecieptCountry2.Text
                                Case Me.tabInport2.ID
                                    countryCode = Me.txtDischargeCountry2.Text
                            End Select
                        End If
                    Next

                    ' 発生区分
                    Dim allCostList As List(Of COSTITEM)
                    allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
                    Dim uniqueIndex As Integer = 0
                    Integer.TryParse(Me.hdnCurrentUnieuqIndex.Value, uniqueIndex)
                    Dim targetChargeClass4 = (From allCostItem In allCostList
                                              Where allCostItem.UniqueIndex = uniqueIndex Select allCostItem.ChargeClass4).FirstOrDefault
                    Dim dt As DataTable = New DataTable
                    lbContractor.Items.Clear()

                    Select Case targetChargeClass4
                        Case GBC_CHARGECLASS4.AGENT
                            GBA00004CountryRelated.COUNTRYCODE = countryCode
                            GBA00004CountryRelated.LISTBOX_OFFICE = lbContractor
                            GBA00004CountryRelated.GBA00004getLeftListOffice()
                        Case GBC_CHARGECLASS4.CURRIER
                            GBA00004CountryRelated.COUNTRYCODE = countryCode
                            GBA00004CountryRelated.LISTBOX_VENDER = lbContractor
                            GBA00004CountryRelated.GBA00004getLeftListVender()
                        Case GBC_CHARGECLASS4.FORWARDER
                            GBA00004CountryRelated.COUNTRYCODE = countryCode
                            GBA00004CountryRelated.LISTBOX_FORWARDER = lbContractor
                            GBA00004CountryRelated.GBA00004getLeftListForwarder()
                        Case GBC_CHARGECLASS4.DEPOT
                            GBA00004CountryRelated.COUNTRYCODE = countryCode
                            GBA00004CountryRelated.LISTBOX_DEPOT = lbContractor
                            GBA00004CountryRelated.GBA00004getLeftListDepot()
                        Case GBC_CHARGECLASS4.OTHER
                            GBA00004CountryRelated.COUNTRYCODE = countryCode
                            GBA00004CountryRelated.LISTBOX_OTHER = lbContractor
                            GBA00004CountryRelated.GBA00004getLeftListOther()
                    End Select
                Case Me.vLeftTerm.ID
                    Me.lbTerm.Focus()
                    '入力済のデータを選択状態にする
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)

                    If dblClickField IsNot Nothing AndAlso lbTerm.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbTerm.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
                Case Me.vLeftBillingCategory.ID
                    Me.lbBillingCategory.Focus()
                    '入力済のデータを選択状態にする
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)

                    If dblClickField IsNot Nothing AndAlso lbBillingCategory.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbBillingCategory.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If

                Case Me.vLeftAgent.ID
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)

                    If dblClickField IsNot Nothing AndAlso lbAgent.Items IsNot Nothing Then
                        Dim dblClickTextObj As TextBox = DirectCast(dblClickField, TextBox)
                        'ダブルクリックしたフィールドに応じ国コードを取得
                        Dim countryCode As String = ""
                        Select Case dblClickTextObj.ID
                            Case Me.txtAgentPol1.ID
                                countryCode = Me.txtLoadCountry1.Text.Trim
                            Case Me.txtAgentPod1.ID
                                countryCode = Me.txtDischargeCountry1.Text.Trim
                            Case Me.txtAgentPol2.ID
                                countryCode = Me.txtLoadCountry2.Text.Trim
                            Case Me.txtAgentPod2.ID
                                countryCode = Me.txtDischargeCountry2.Text.Trim
                            Case Me.txtInvoiced.ID
                                countryCode = ""
                        End Select
                        Dim dt As DataTable = GetAgent(countryCode)
                        With Me.lbAgent
                            .DataSource = dt
                            .DataTextField = "LISTBOXNAME"
                            .DataValueField = "CODE"
                            .DataBind()
                            .Focus()
                        End With
                    End If
                Case Me.vLeftMSDS.ID
                    SetMSDSItem()
                Case Me.vLeftPort.ID
                    'BRの発側は新規作時に選んだ港の国に応じた荷主を決定する、
                    '当画面では荷主の変更は許可しないため、国の縛りは含める
                    Dim countryCode As String = ""
                    If Me.hdnTextDbClickField.Value.Equals("txtRecieptPort1") Then
                        countryCode = Me.txtRecieptCountry1.Text
                    End If
                    Dim dt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(countryCode)
                    With Me.lbPort
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "PORTCODE"
                        .DataBind()
                        .Focus()
                    End With
                    '入力済のデータを選択状態にする
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)

                    If dblClickField IsNot Nothing AndAlso lbPort.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbPort.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
            End Select
        End If

    End Sub

    ''' <summary>
    ''' 申請ボタン押下時
    ''' </summary>
    Public Sub btnApply_Click()
        'セールスブレーカーかつフィリングレートチェックエラーの場合は申請させない
        If Me.hdnBrType.Value = "1" AndAlso {"", "ERROR"}.Contains(Me.txtTankFillingCheck.Text) Then
            CommonFunctions.ShowMessage("10028", Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

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
        Dim callerButton As String = "btnApplyMsgYes"
        saveProc(callerButton)

        If Not hdnMsgId.Value = C_MESSAGENO.NORMALENTRY Then
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

        Dim COA0004LableMessage As New BASEDLL.COA0004LableMessage    'メッセージ取得
        Dim COA0021ListTable As New COA0021ListTable
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval

        Dim brInfoPrev As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        brInfoPrev = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))
        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing
        brInfo = GetBreakerInfo(brInfoPrev("INFO").BrId)
        Dim procDateTime As DateTime = DateTime.Now
        Dim applyId As String = Nothing
        Dim lastStep As String = Nothing

        '申請ID取得
        Dim GBA00011ApplyID As New GBA00011ApplyID
        GBA00011ApplyID.COMPCODE = GBC_COMPCODE_D
        GBA00011ApplyID.SYSCODE = COA0019Session.SYSCODE
        GBA00011ApplyID.KEYCODE = COA0019Session.APSRVname
        GBA00011ApplyID.DIVISION = "B"
        GBA00011ApplyID.SEQOBJID = C_SQLSEQ.BREAKERWORK
        GBA00011ApplyID.SEQLEN = 6
        GBA00011ApplyID.GBA00011getApplyID()
        If GBA00011ApplyID.ERR = C_MESSAGENO.NORMAL Then
            applyId = GBA00011ApplyID.APPLYID
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                            messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00011ApplyID.ERR)})
            Return
        End If

        Dim subCode As String = Me.hdnAgentOrganizer.Value

        '申請登録
        COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
        COA0032Apploval.I_APPLYID = applyId
        COA0032Apploval.I_MAPID = CONST_MAPID
        COA0032Apploval.I_EVENTCODE = C_BRSEVENT.APPLY
        COA0032Apploval.I_SUBCODE = subCode
        COA0032Apploval.COA0032setApply()
        If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
            lastStep = COA0032Apploval.O_LASTSTEP
        Else
            CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage)
            Return
        End If

        'ブレーカー更新
        Dim GBA00016BreakerApplyProc As New GBA00016BreakerApplyProc _
              With {.brId = Me.lblBrNo.Text, .ApplyId = applyId,
                    .LastStep = lastStep, .AmtRequest = Me.txtAmtRequest.Text,
                    .ProcDateTime = procDateTime}

        GBA00016BreakerApplyProc.GBA00016BreakerDataApplyUpdate()
        Me.hdnStatus.Value = C_APP_STATUS.APPLYING
        'メール
        Dim GBA00009MailSendSet As New GBA00009MailSendSet
        GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
        GBA00009MailSendSet.EVENTCODE = C_BRSEVENT.APPLY
        GBA00009MailSendSet.MAILSUBCODE = ""
        GBA00009MailSendSet.BRID = Me.lblBrNo.Text
        GBA00009MailSendSet.BRSUBID = brInfo("INFO").SubId
        GBA00009MailSendSet.BRBASEID = brInfo("INFO").LinkId
        GBA00009MailSendSet.BRROUND = ""
        GBA00009MailSendSet.APPLYID = applyId
        GBA00009MailSendSet.GBA00009setMailToBR()
        If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
            Return
        End If

        'メッセージ出力
        Me.hdnMsgId.Value = C_MESSAGENO.APPLYSUCCESS

        '選択タブ保持
        Me.hdnCostSelectedTabId.Value = Me.hdnSelectedTabId.Value

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()

        Dim currentCostItemGroup As COSTITEM.CostItemGroup = Nothing
        Select Case Me.hdnSelectedTabId.Value
            Case Me.tabInport1.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Inport1
            Case Me.tabInport2.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Inport2
            Case Me.tabExport1.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Export1
            Case Me.tabExport2.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Export2
            Case Me.tabOrganizer.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Organizer
        End Select

        If currentCostItemGroup <> COSTITEM.CostItemGroup.Organizer Then
            '入力内容保持
            SaveGridItem(currentCostItemGroup)
        End If

        TextChangeCheck()
        If Me.hdnMsgboxShowFlg.Value = "1" Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, Me, submitButtonId:="btnExitMsgOk")
            Return
        End If

        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ 画面遷移先URL取得 ■■■
        If Me.hdnCallerMapId.Value = CONST_APP_MAPID Then
            COA0012DoUrl.MAPIDP = CONST_APP_MAPID
        Else
            COA0012DoUrl.MAPIDP = "GBT00002S"
        End If
        HttpContext.Current.Session("MAPvariant") = "GB_Default"
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If

        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 終了OKボタン押下時
    ''' </summary>
    Public Sub btnExitMsgOk_Click()

        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ 画面遷移先URL取得 ■■■
        If Me.hdnCallerMapId.Value = CONST_APP_MAPID Then
            COA0012DoUrl.MAPIDP = CONST_APP_MAPID
        Else
            COA0012DoUrl.MAPIDP = "GBT00002S"
        End If
        HttpContext.Current.Session("MAPvariant") = "GB_Default"
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If

        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 編集ボタン押下時
    ''' </summary>
    Public Sub btnReject_Click()
        'メッセージ表示
        CommonFunctions.ShowMessage(C_MESSAGENO.REJECTSUCCESS, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        Me.hdnStatus.Value = C_APP_STATUS.REVISE
        Dim isOrg As Boolean = False
        If GetCurrentTab() = COSTITEM.CostItemGroup.Organizer Then
            isOrg = True
        End If
        Dim selectedTab As String = hdnSelectedTabId.Value
        visibleControl(isOrg, selectedTab)
    End Sub
    ''' <summary>
    ''' Excel出力ボタン押下時
    ''' </summary>
    Public Sub btnOutputExcel_Click()
        Dim ds As New DataSet
        Dim currentTab As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Organizer
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)
        Dim isOrganizer As Boolean = False
        Dim isOutFull As Boolean = False
        Dim isOutFile As Boolean = False

        tabObjects.Add(COSTITEM.CostItemGroup.Organizer, Me.tabOrganizer)
        tabObjects.Add(COSTITEM.CostItemGroup.Export1, Me.tabExport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport1, Me.tabInport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Export2, Me.tabExport2)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport2, Me.tabInport2)


        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        If reportId = "GBT00001F" Then
            isOutFull = True
        End If

        '帳票出力
        Dim tmpFile As String = ""
        Dim outUrl As String = ""

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Visible = True Then

                If isOutFull = True OrElse (tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected")) Then

                    currentTab = tabObject.Key
                    If currentTab = COSTITEM.CostItemGroup.Organizer Then
                        isOrganizer = True
                    Else
                        isOrganizer = False
                    End If

                    Dim dt As DataTable = Nothing

                    Dim reportMapId As String = ""
                    If isOrganizer = True Then
                        '画面オーガナイザー情報を取得しデータテーブルに格納
                        dt = CollectDisplayOrganizerInfo()
                        reportMapId = "GBT00001_O"
                        'If isOutFull = True Then
                        '    reportId = "GBT00001O"
                        'End If
                    Else
                        If isOutFull = True Then
                            reportId = "GBT00001C"
                        Else
                            '一旦画面費用項目をviewstateに退避
                            SaveGridItem(currentTab)
                        End If
                        '画面費用を取得しデータテーブルに格納
                        dt = CollectDisplayCostInfo(currentTab)
                        reportMapId = "GBT00001_C"
                    End If

                    With Nothing
                        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
                        COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                        COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                        COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
                        COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                        If isOutFull = True Then
                            COA0027ReportTable.ADDSHEET = tabObject.Value.InnerText            'PARAM07:追記シート（任意）
                            If tmpFile <> "" Then
                                COA0027ReportTable.ADDFILE = tmpFile                           'PARAM06:追記ファイル（フルパス（O_FILEpath））
                            End If
                        End If
                        COA0027ReportTable.COA0027ReportTable()

                        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                        Else
                            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                            Return
                        End If

                        tmpFile = COA0027ReportTable.FILEpath
                        outUrl = COA0027ReportTable.URL

                    End With
                    isOutFile = True

                End If

                If isOutFull = False AndAlso isOutFile = True Then
                    Exit For
                End If

            End If
        Next

        '別画面でExcelを表示
        hdnPrintURL.Value = outUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

    End Sub

    ''' <summary>
    ''' PDF出力ボタン押下時
    ''' </summary>
    Public Sub btnPrint_Click()
        Dim ds As New DataSet
        Dim currentTab As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Organizer
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)
        Dim isOrganizer As Boolean = False

        tabObjects.Add(COSTITEM.CostItemGroup.Organizer, Me.tabOrganizer)
        tabObjects.Add(COSTITEM.CostItemGroup.Export1, Me.tabExport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport1, Me.tabInport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Export2, Me.tabExport2)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport2, Me.tabInport2)

        Dim reportId As String = ""
        Dim reportMapId As String = "GBT00001_P"
        If Me.hdnCountryOrganizer.Value.Equals("JP") AndAlso GBA00003UserSetting.IS_JOTUSER Then
            reportMapId = reportMapId & "J"
        End If
        '帳票出力
        Dim tmpFile As String = ""
        Dim outUrl As String = ""
        Dim dt As DataTable = Nothing
        Dim dtCost As DataTable = Nothing
        Dim dtMarge As DataTable = Nothing
        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Visible = True Then

                currentTab = tabObject.Key
                If currentTab = COSTITEM.CostItemGroup.Organizer Then
                    isOrganizer = True
                Else
                    isOrganizer = False
                End If

                If isOrganizer = True Then
                    '画面オーガナイザー情報を取得しデータテーブルに格納
                    dt = CollectDisplayOrganizerInfo()
                Else
                    '画面費用を取得しデータテーブルに格納
                    dtCost = CollectDisplayCostInfo(currentTab)
                    If dtMarge Is Nothing Then
                        dtMarge = dtCost.Clone
                    End If
                    dtMarge.Merge(dtCost)
                End If

            End If
        Next

        With Nothing

            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

            'オーガナイザー
            reportId = "GBT00001P"

            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
            COA0027ReportTable.ADDSHEET = Me.tabOrganizer.InnerText            'PARAM07:追記シート（任意）
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                Return
            End If

            tmpFile = COA0027ReportTable.FILEpath
            outUrl = COA0027ReportTable.URL

            '明細
            Dim pageCnt As Integer = 1                                         '出力明細ページ数
            'Dim dataSrt As Integer                                              '明細出力対象開始行
            Dim dataEnd As Integer                                             '明細出力対象終了行
            Const INFOBRLINE As Integer = 29                                   '明細１ページ目出力可能行数
            Const INFO2LINE As Integer = 69                                    '明細２ページ目以降出力可能行数
            'Const INFOBRLINE As Integer = 4                                   '明細１ページ目出力可能行数
            'Const INFO2LINE As Integer = 8                                    '明細２ページ目以降出力可能行数
            reportId = "Info"

            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID

            '1枚目の出力行数は29
            If dtMarge.Rows.Count > INFOBRLINE Then
                pageCnt = (CType(Math.Ceiling((dtMarge.Rows.Count - INFOBRLINE) / INFO2LINE), Integer) + 1)
            End If
            Dim j As Integer = 0
            For i As Integer = 1 To pageCnt

                Dim dtMargeTmp As DataTable = dtMarge.Clone
                If tmpFile <> "" Then
                    COA0027ReportTable.ADDFILE = tmpFile                       'PARAM06:追記ファイル（フルパス（O_FILEpath））
                End If

                If i <> pageCnt Then
                    COA0027ReportTable.FILETYPE = "XLSX"                       'PARAM03:出力ファイル形式
                Else
                    COA0027ReportTable.FILETYPE = "pdf"                        'PARAM03:出力ファイル形式
                End If
                If i = 1 Then
                    'dataSrt = 1
                    dataEnd = INFOBRLINE
                    COA0027ReportTable.ADDSHEET = "InfoBr"                     'PARAM07:追記シート（任意）
                    COA0027ReportTable.ADDSHEETNO = Nothing                    'PARAM08:追記シートNO（任意）

                    Do While (j < dtMarge.Rows.Count)

                        dtMargeTmp.ImportRow(dtMarge.Rows(j))
                        With dtMargeTmp.Rows(dtMargeTmp.Rows.Count - 1)
                            .Item("TAXATION") = If(.Item("TAXATION").Equals("0"), "N", "Y")
                        End With

                        j = j + 1

                    Loop
                    j = dataEnd

                Else
                    'dataSrt = dataEnd + 1
                    dataEnd = INFOBRLINE + ((i - 1) * INFO2LINE)
                    COA0027ReportTable.ADDSHEET = "Info"                       'PARAM07:追記シート（任意）
                    COA0027ReportTable.ADDSHEETNO = (i - 1).ToString           'PARAM08:追記シートNO（任意）

                    Do While (j < dtMarge.Rows.Count AndAlso j < dataEnd)

                        dtMargeTmp.ImportRow(dtMarge.Rows(j))
                        With dtMargeTmp.Rows(dtMargeTmp.Rows.Count - 1)
                            .Item("TAXATION") = If(.Item("TAXATION").Equals("0"), "N", "Y")
                        End With

                        j = j + 1

                    Loop

                End If

                'Do While (j < dtMarge.Rows.Count AndAlso j < dataEnd)

                '    dtMargeTmp.ImportRow(dtMarge.Rows(j))
                '    With dtMargeTmp.Rows(dtMargeTmp.Rows.Count - 1)
                '        .Item("TAXATION") = If(.Item("TAXATION").Equals("0"), "N", "Y")
                '    End With

                '    j = j + 1

                'Loop
                ''dtMargeTmp = (From item In dtMarge.Rows.Item(0) Where rows.item >= dataSrt AndAlso CInt(item("LINECNT")) <= dataEnd).CopyToDataTable
                COA0027ReportTable.TBLDATA = dtMargeTmp                        'PARAM04:データ参照tabledata
                COA0027ReportTable.COA0027ReportTable()
                If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                Else
                    CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                    Return
                End If
                tmpFile = COA0027ReportTable.FILEpath
                outUrl = COA0027ReportTable.URL
            Next
            tmpFile = COA0027ReportTable.FILEpath
            outUrl = COA0027ReportTable.URL

        End With

        '別画面でExcelを表示
        hdnPrintURL.Value = outUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint()", True)

    End Sub
    ''' <summary>
    ''' 保存ボタン押下時イベント
    ''' </summary>
    Public Sub btnSave_Click()

        '保存処理
        Dim callerButton As String = "btnSave"
        saveProc(callerButton)

        If Not Me.hdnMsgId.Value = C_MESSAGENO.NORMALENTRY Then
            Return
        End If

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub
    ''' <summary>
    ''' 保存処理
    ''' </summary>
    Public Sub saveProc(callerButton As String)

        Dim ds As New DataSet
        'オーナー基本情報のテキストボックス禁則文字変換
        Dim changeInvalidTextObjects As New List(Of TextBox) From
            {Me.txtBrStYmd, Me.txtBrEndYmd, Me.txtBrTerm, Me.txtNoOfTanks,
             Me.txtConsignee, Me.txtCarrier1, Me.txtCarrier2, Me.txtProduct,
             Me.txtRecieptCountry1, Me.txtRecieptPort1, Me.txtLoadCountry1, Me.txtLoadPort1,
             Me.txtDischargeCountry1, Me.txtDischargePort1, Me.txtDeliveryCountry1, Me.txtDeliveryPort1,
             Me.txtRecieptCountry2, Me.txtRecieptPort2, Me.txtLoadCountry2, Me.txtLoadPort2,
             Me.txtDischargeCountry2, Me.txtDischargePort2, Me.txtDeliveryCountry2, Me.txtDeliveryPort2,
             Me.txtAgentPol1, Me.txtAgentPol2, Me.txtAgentPod1, Me.txtAgentPod2,
             Me.txtVsl1, Me.txtVoy1, Me.txtEtd1, Me.txtEta1,
             Me.txtVsl2, Me.txtVoy2, Me.txtEtd2, Me.txtEta2,
             Me.txtInvoiced}
        ChangeInvalidChar(changeInvalidTextObjects)
        '画面情報をデータテーブルに格納
        Dim orgDt As DataTable = CollectDisplayOrganizerInfo()
        Dim costDt As DataTable = CollectDisplayCostInfo()
        '費用項目の禁則文字置換
        ChangeInvalidChar(costDt, New List(Of String) From {"COSTCODE", "COSTNAME", "CONTRACTOR", "CURRENCYCODE"})
        '各種データテーブルをデータセットに格納
        ds.Tables.AddRange({orgDt, costDt})
        '入力チェック
        If CheckInput(ds, True, True) = False Then
            hdnMsgId.Value = C_MESSAGENO.RIGHTBIXOUT
            Return
        End If
        '保存時のみ且つオーガナイザ且つ申請画面遷移ではない時の入力チェック
        '※既存データにマイナスがある為、JOTのメモ保存時に保存できなくなるのを回避
        If Me.hdnSelectedTabId.Value = Me.tabOrganizer.ClientID AndAlso
            Not (Me.hdnIsViewFromApprove.Value.Equals("1")) Then
            If IsNumeric(Me.txtJOTHireage.Text) AndAlso Decimal.Parse(Me.txtJOTHireage.Text) < 0 Then
                'メッセージ出力
                CommonFunctions.ShowMessage(C_MESSAGENO.HIREAGEISNAGATIVE, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
                hdnMsgId.Value = C_MESSAGENO.HIREAGEISNAGATIVE
                Return
            End If
        End If
        '港変更チェック
        Dim modPorts = GetModifiedPort()
        Dim isModifiedPort As Boolean = False
        If modPorts.Count > 0 AndAlso Me.hdnNewBreaker.Value <> "1" Then
            isModifiedPort = True
        ElseIf ViewState("COPYORGANIZERINFO") IsNot Nothing Then
            isModifiedPort = True
        End If
        'DB登録処理実行
        EntryData(ds, COSTITEM.CostItemGroup.Organizer, isModifiedPort:=isModifiedPort, callerButton:=callerButton)
        If hdnMsgId.Value <> C_MESSAGENO.NORMALENTRY Then
            Return
        End If
        '選択タブ保持
        Me.hdnCostSelectedTabId.Value = Me.hdnSelectedTabId.Value

        'インプットリクエストボタン制御
        If Me.hdnSelectedTabId.Value = Me.tabOrganizer.ID Then
            Me.hdnNewBreaker.Value = ""
        End If

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.NORMALENTRY

    End Sub
    ''' <summary>
    ''' メール送信OKボタン押下時
    ''' 一旦保存時と同じ動き
    ''' </summary>
    ''' <remarks>InputRequestボタン</remarks>
    Public Sub btnSelectMailOk_Click()
        Dim ds As New DataSet
        'オーナー基本情報のテキストボックス禁則文字変換
        Dim changeInvalidTextObjects As New List(Of TextBox) From
            {Me.txtBrStYmd, Me.txtBrEndYmd, Me.txtBrTerm, Me.txtNoOfTanks,
             Me.txtConsignee, Me.txtCarrier1, Me.txtCarrier2, Me.txtProduct,
             Me.txtRecieptCountry1, Me.txtRecieptPort1, Me.txtLoadCountry1, Me.txtLoadPort1,
             Me.txtDischargeCountry1, Me.txtDischargePort1, Me.txtDeliveryCountry1, Me.txtDeliveryPort1,
             Me.txtRecieptCountry2, Me.txtRecieptPort2, Me.txtLoadCountry2, Me.txtLoadPort2,
             Me.txtAgentPod1, Me.txtAgentPod2, Me.txtAgentPol1, Me.txtAgentPol2,
             Me.txtDischargeCountry2, Me.txtDischargePort2, Me.txtDeliveryCountry2, Me.txtDeliveryPort2,
             Me.txtVsl1, Me.txtVoy1, Me.txtEtd1, Me.txtEta1,
             Me.txtVsl2, Me.txtVoy2, Me.txtEtd2, Me.txtEta2,
             Me.txtInvoiced}
        ChangeInvalidChar(changeInvalidTextObjects)
        '画面情報をデータテーブルに格納
        Dim orgDt As DataTable = CollectDisplayOrganizerInfo()
        Dim costDt As DataTable = CollectDisplayCostInfo()
        '費用項目の禁則文字置換
        ChangeInvalidChar(costDt, New List(Of String) From {"COSTCODE", "COSTNAME", "CONTRACTOR", "CURRENCYCODE"})
        '各種データテーブルをデータセットに格納
        ds.Tables.AddRange({orgDt, costDt})
        '入力チェック
        If CheckInput(ds, True, True) = False Then
            Return
        End If

        orgDt = ds.Tables("ORGANIZER_INFO")
        costDt = ds.Tables("COST_INFO")

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        brInfo = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))

        Dim procDateTime As DateTime = DateTime.Now
        Dim applyId As String = Nothing
        Dim lastStep As String = Nothing

        Dim edit As String = ""
        Dim comp As String = ""
        If Not GetApprovalStat(C_APP_STATUS.EDITING, edit) Then
            Return
        End If
        If Not GetApprovalStat(C_APP_STATUS.COMPLETE, comp) Then
            Return
        End If
        'InputRequest画面情報を取得(対象の部分のみ※非表示は配列に入らない)
        Dim inputRequstInfoList = GetInputRequestInfo(brInfo, orgDt)
        For Each inputRequstInfo In inputRequstInfoList
            applyId = ""
            lastStep = ""

            If inputRequstInfo.RequestFlg AndAlso {"", comp}.Contains(inputRequstInfo.Status) Then
                '申請IDを取得し申請テーブルに登録
                Dim messageNo As String = InputRequestApplyEntry(inputRequstInfo.EventCode, inputRequstInfo.SubCode, applyId, lastStep)
                If messageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)
                    Return
                End If

                'ブレーカー更新
                Dim sqlStat As New StringBuilder
                Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                    sqlCon.Open() '接続オープン

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
                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                        With sqlCmd.Parameters
                            'パラメータ設定
                            Dim dr As DataRow = orgDt.Rows(0)
                            .Add("@BRID", SqlDbType.NVarChar, 20).Value = dr.Item("BRID")
                            .Add("@TYPE", SqlDbType.NVarChar, 20).Value = inputRequstInfo.Type
                            .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                            .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = applyId
                            .Add("@LASTSTEP", SqlDbType.NVarChar, 20).Value = lastStep
                            .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        End With
                        sqlCmd.ExecuteNonQuery()
                    End Using
                End Using

                '内部ステータス更新
                inputRequstInfo.HdnStatusObj.Value = edit
            ElseIf inputRequstInfo.Status = "" Then
                '申請IDを取得し申請テーブルに登録
                Dim messageNo As String = InputRequestApplyEntry(inputRequstInfo.EventCode, inputRequstInfo.SubCode, applyId, lastStep)
                If messageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)
                    Return
                End If
                'ブレーカー更新
                Dim sqlStat As New StringBuilder
                Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                    sqlCon.Open() '接続オープン

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
                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                        With sqlCmd.Parameters
                            'パラメータ設定
                            Dim dr As DataRow = orgDt.Rows(0)
                            .Add("@BRID", SqlDbType.NVarChar, 20).Value = dr.Item("BRID")
                            .Add("@TYPE", SqlDbType.NVarChar, 20).Value = inputRequstInfo.Type
                            .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                            .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = applyId
                            .Add("@LASTSTEP", SqlDbType.NVarChar, 20).Value = lastStep
                            .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        End With
                        sqlCmd.ExecuteNonQuery()
                    End Using
                End Using

                'ステータスをCompleteに更新
                Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                    sqlCon.Open() '接続オープン

                    sqlStat.Clear()
                    sqlStat.AppendLine("UPDATE COT0002_APPROVALHIST")
                    sqlStat.AppendLine("   SET APPROVEDATE = @APPROVEDATE")
                    sqlStat.AppendLine("      ,APPROVERID  = @APPROVERID")
                    sqlStat.AppendLine("      ,STATUS      = @STATUS")
                    sqlStat.AppendLine("      ,UPDYMD      = @UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER     = @UPDUSER ")
                    sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
                    sqlStat.AppendLine(" WHERE APPLYID     = ")
                    sqlStat.AppendLine("       (SELECT APPLYID FROM GBT0001_BR_INFO ")
                    sqlStat.AppendLine("         WHERE BRID    = @BRID ")
                    sqlStat.AppendLine("           AND TYPE    = @TYPE ")
                    sqlStat.AppendLine("           AND DELFLG <> @DELFLG) ")
                    sqlStat.AppendLine("   AND DELFLG     <> @DELFLG ")

                    'DB接続
                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                        With sqlCmd.Parameters
                            'パラメータ設定
                            Dim dr As DataRow = orgDt.Rows(0)
                            .Add("@APPROVEDATE", SqlDbType.DateTime).Value = procDateTime
                            .Add("@APPROVERID", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                            .Add("@STATUS", SqlDbType.NVarChar, 20).Value = C_APP_STATUS.COMPLETE
                            .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                            .Add("@BRID", SqlDbType.NVarChar, 20).Value = dr.Item("BRID")
                            .Add("@TYPE", SqlDbType.NVarChar, 20).Value = inputRequstInfo.Type
                            .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        End With
                        sqlCmd.ExecuteNonQuery()
                    End Using
                End Using

                '内部ステータス更新
                inputRequstInfo.HdnStatusObj.Value = comp
            End If

            If inputRequstInfo.MailFlg AndAlso (applyId <> "" OrElse inputRequstInfo.ApplyId <> "") Then
                If applyId = "" Then
                    applyId = inputRequstInfo.ApplyId
                End If
                'メール
                Dim GBA00009MailSendSet As New GBA00009MailSendSet
                GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                GBA00009MailSendSet.EVENTCODE = inputRequstInfo.EventCode
                'GBA00009MailSendSet.MAILSUBCODE = subCode
                GBA00009MailSendSet.MAILSUBCODE = ""
                GBA00009MailSendSet.BRID = Convert.ToString(orgDt.Rows(0).Item("BRID"))
                GBA00009MailSendSet.BRSUBID = brInfo("INFO").SubId
                GBA00009MailSendSet.BRBASEID = brInfo("INFO").LinkId
                'GBA00009MailSendSet.BRROUND = inputRequstInfo.EventCode
                GBA00009MailSendSet.BRROUND = inputRequstInfo.BrRound
                GBA00009MailSendSet.APPLYID = applyId
                GBA00009MailSendSet.GBA00009setMailToBR()
                If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
                    Return
                End If

            End If

        Next inputRequstInfo

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.NORMALENTRY

        '選択タブ保持
        Me.hdnCostSelectedTabId.Value = Me.hdnSelectedTabId.Value

        '
        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub
    ''' <summary>
    ''' InputRequstの情報を取得
    ''' </summary>
    ''' <param name="brInfo">ブレーカー基本情報</param>
    ''' <param name="orgDt">オーガナイザ情報</param>
    ''' <returns></returns>
    Private Function GetInputRequestInfo(brInfo As Dictionary(Of String, BreakerInfo), orgDt As DataTable) As List(Of InputRequestValue)
        Dim retObj As New List(Of InputRequestValue)
        Dim orgDr = orgDt.Rows(0)
        Dim itemSet = {New With {.type = "POL1", .chkInputRObj = Me.chkInputRequestExport1, .chkMailObj = Me.chkMailExport1,
                                .statObj = Me.hdnPol1Status, .eventCode = C_BRSEVENT.COSTIN_POL, .brRound = "1"},
                       New With {.type = "POD1", .chkInputRObj = Me.chkInputRequestImport1, .chkMailObj = Me.chkMailInport1,
                                 .statObj = Me.hdnPod1Status, .eventCode = C_BRSEVENT.COSTIN_POD, .brRound = "1"},
                       New With {.type = "POL2", .chkInputRObj = Me.chkInputRequestExport2, .chkMailObj = Me.chkMailExport2,
                                 .statObj = Me.hdnPol2Status, .eventCode = C_BRSEVENT.COSTIN_POL, .brRound = "2"},
                       New With {.type = "POD2", .chkInputRObj = Me.chkInputRequestImport2, .chkMailObj = Me.chkMailInport2,
                                 .statObj = Me.hdnPod2Status, .eventCode = C_BRSEVENT.COSTIN_POD, .brRound = "2"}}
        For Each item In itemSet
            If item.chkInputRObj.Visible = False Then
                Continue For
            End If
            Dim irv As New InputRequestValue
            irv.Type = item.type
            irv.RequestFlg = item.chkInputRObj.Checked
            irv.MailFlg = item.chkMailObj.Checked
            irv.Status = item.statObj.Value
            irv.HdnStatusObj = item.statObj
            irv.EventCode = item.eventCode
            irv.SubCode = Convert.ToString(orgDr("AGENT" & item.type))
            irv.BrRound = item.brRound
            irv.ApplyId = brInfo(item.type).ApplyId

            retObj.Add(irv)
        Next item
        Return retObj
    End Function
    ''' <summary>
    ''' InputRequestを申請テーブルに登録
    ''' </summary>
    ''' <param name="evntCode">[In]イベントコード</param>
    ''' <param name="subCode">[In]サブコード</param>
    ''' <param name="applyId">[Out]申請ID</param>
    ''' <param name="lastStep">[Out]最終承認ステップ</param>
    ''' <returns>メッセージNo</returns>
    Private Function InputRequestApplyEntry(evntCode As String, subCode As String, ByRef applyId As String, ByRef lastStep As String) As String
        '申請ID取得
        Dim GBA00011ApplyID As New GBA00011ApplyID
        GBA00011ApplyID.COMPCODE = GBC_COMPCODE_D
        GBA00011ApplyID.SYSCODE = COA0019Session.SYSCODE
        GBA00011ApplyID.KEYCODE = COA0019Session.APSRVname
        GBA00011ApplyID.DIVISION = "B"
        GBA00011ApplyID.SEQOBJID = C_SQLSEQ.BREAKERWORK
        GBA00011ApplyID.SEQLEN = 6
        GBA00011ApplyID.GBA00011getApplyID()
        If GBA00011ApplyID.ERR = C_MESSAGENO.NORMAL Then
            applyId = GBA00011ApplyID.APPLYID
        Else
            Return GBA00011ApplyID.ERR
        End If

        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        '申請登録
        COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
        COA0032Apploval.I_APPLYID = applyId
        COA0032Apploval.I_MAPID = CONST_MAPID
        COA0032Apploval.I_EVENTCODE = evntCode
        COA0032Apploval.I_SUBCODE = subCode
        COA0032Apploval.COA0032setApply()
        If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
            lastStep = COA0032Apploval.O_LASTSTEP
        Else
            Return COA0032Apploval.O_ERR
        End If
        'ここまで来た場合は正常
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' EntryCostメール送信OKボタン押下時
    ''' </summary>
    Public Sub btnEntryCostSelectMailOk_Click()

        'EntryCost処理
        entryCost()

    End Sub
    ''' <summary>
    ''' EntryCostメール送信YESボタン押下時
    ''' </summary>
    Public Sub btnEntryCostSelectMailYes_Click()

        '保存処理
        Dim callerButton As String = "btnEntryCostSelectMailYes"
        saveProc(callerButton)

        If Not hdnMsgId.Value = C_MESSAGENO.NORMALENTRY Then
            Return
        End If

        'EntryCost処理
        entryCost()

    End Sub
    ''' <summary>
    ''' EntryCostメール送信NOボタン押下時
    ''' </summary>
    Public Sub btnEntryCostSelectMailNo_Click()

        'EntryCost処理
        entryCost()

    End Sub

    ''' <summary>
    ''' EntryCost
    ''' </summary>
    Public Sub entryCost()

        Dim ds As New DataSet
        'オーナー基本情報のテキストボックス禁則文字変換
        Dim changeInvalidTextObjects As New List(Of TextBox) From
            {Me.txtBrStYmd, Me.txtBrEndYmd, Me.txtBrTerm, Me.txtNoOfTanks,
             Me.txtConsignee, Me.txtCarrier1, Me.txtCarrier2, Me.txtProduct,
             Me.txtRecieptCountry1, Me.txtRecieptPort1, Me.txtLoadCountry1, Me.txtLoadPort1,
             Me.txtDischargeCountry1, Me.txtDischargePort1, Me.txtDeliveryCountry1, Me.txtDeliveryPort1,
             Me.txtRecieptCountry2, Me.txtRecieptPort2, Me.txtLoadCountry2, Me.txtLoadPort2,
             Me.txtDischargeCountry2, Me.txtDischargePort2, Me.txtDeliveryCountry2, Me.txtDeliveryPort2,
             Me.txtAgentPol1, Me.txtAgentPol2, Me.txtAgentPod1, Me.txtAgentPod2,
             Me.txtVsl1, Me.txtVoy1, Me.txtEtd1, Me.txtEta1,
             Me.txtVsl2, Me.txtVoy2, Me.txtEtd2, Me.txtEta2,
             Me.txtInvoiced}
        ChangeInvalidChar(changeInvalidTextObjects)
        '画面情報をデータテーブルに格納
        Dim orgDt As DataTable = CollectDisplayOrganizerInfo()
        Dim costDt As DataTable = CollectDisplayCostInfo()
        '費用項目の禁則文字置換
        ChangeInvalidChar(costDt, New List(Of String) From {"COSTCODE", "COSTNAME", "CONTRACTOR", "CURRENCYCODE"})
        '各種データテーブルをデータセットに格納
        ds.Tables.AddRange({orgDt, costDt})
        '入力チェック
        If CheckInput(ds, True, True) = False Then
            Return
        End If

        orgDt = ds.Tables("ORGANIZER_INFO")
        costDt = ds.Tables("COST_INFO")

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        brInfo = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))

        Dim procDateTime As DateTime = DateTime.Now
        Dim applyId As String = Nothing

        Dim evntCode As String = Nothing
        Dim type As String = Nothing
        Dim lastStep As String = Nothing
        Dim subCode As String = Nothing
        Dim brRound As String = Nothing

        Dim comp As String = ""
        If Not GetApprovalStat(C_APP_STATUS.COMPLETE, comp) Then
            Return
        End If

        'Tab判定
        If Me.hdnSelectedTabId.Value = Me.tabExport1.ClientID Then
            evntCode = C_BRSEVENT.COSTFN_POL
            type = "POL1"
            subCode = Convert.ToString(orgDt.Rows(0).Item("AGENTPOL1"))
            brRound = "1"
        ElseIf Me.hdnSelectedTabId.Value = Me.tabInport1.ClientID Then
            evntCode = C_BRSEVENT.COSTFN_POD
            type = "POD1"
            subCode = Convert.ToString(orgDt.Rows(0).Item("AGENTPOD1"))
            brRound = "1"
        ElseIf Me.hdnSelectedTabId.Value = Me.tabExport2.ClientID Then
            evntCode = C_BRSEVENT.COSTFN_POL
            type = "POL2"
            subCode = Convert.ToString(orgDt.Rows(0).Item("AGENTPOL2"))
            brRound = "2"
        ElseIf Me.hdnSelectedTabId.Value = Me.tabInport2.ClientID Then
            evntCode = C_BRSEVENT.COSTFN_POD
            type = "POD2"
            subCode = Convert.ToString(orgDt.Rows(0).Item("AGENTPOD2"))
            brRound = "2"
        End If

        '申請ID取得
        Dim GBA00011ApplyID As New GBA00011ApplyID
        GBA00011ApplyID.COMPCODE = GBC_COMPCODE_D
        GBA00011ApplyID.SYSCODE = COA0019Session.SYSCODE
        GBA00011ApplyID.KEYCODE = COA0019Session.APSRVname
        GBA00011ApplyID.DIVISION = "B"
        GBA00011ApplyID.SEQOBJID = C_SQLSEQ.BREAKERWORK
        GBA00011ApplyID.SEQLEN = 6
        GBA00011ApplyID.GBA00011getApplyID()
        If GBA00011ApplyID.ERR = C_MESSAGENO.NORMAL Then
            applyId = GBA00011ApplyID.APPLYID
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00011ApplyID.ERR)})
            Return
        End If

        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        '申請登録
        COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
        COA0032Apploval.I_APPLYID = applyId
        COA0032Apploval.I_MAPID = CONST_MAPID
        COA0032Apploval.I_EVENTCODE = evntCode
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
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open() '接続オープン

            sqlStat.Clear()
            sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
            sqlStat.AppendLine("   SET APPLYID   = @APPLYID")
            sqlStat.AppendLine("      ,LASTSTEP  = @LASTSTEP")
            sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            sqlStat.AppendLine(" WHERE BRID      = @BRID")
            sqlStat.AppendLine("   AND TYPE      = @TYPE")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

            'DB接続
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                With sqlCmd.Parameters
                    'パラメータ設定
                    Dim dr As DataRow = orgDt.Rows(0)
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = dr.Item("BRID")
                    .Add("@TYPE", SqlDbType.NVarChar, 20).Value = type
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = applyId
                    .Add("@LASTSTEP", SqlDbType.NVarChar, 20).Value = lastStep
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                End With
                sqlCmd.ExecuteNonQuery()
            End Using

        End Using

        '内部ステータス更新
        Select Case type
            Case "POL1"
                Me.hdnPol1Status.Value = comp
            Case "POD1"
                Me.hdnPod1Status.Value = comp
            Case "POL2"
                Me.hdnPol2Status.Value = comp
            Case "POD2"
                Me.hdnPod2Status.Value = comp
        End Select

        If Me.chkMailSend.Checked Then
            Dim currentBrInfo = GetBreakerInfo(Convert.ToString(orgDt.Rows(0).Item("BRID")))
            'メール
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = evntCode
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = Convert.ToString(orgDt.Rows(0).Item("BRID"))
            GBA00009MailSendSet.BRSUBID = currentBrInfo("INFO").SubId
            GBA00009MailSendSet.BRBASEID = currentBrInfo("INFO").LinkId
            GBA00009MailSendSet.BRROUND = brRound
            GBA00009MailSendSet.APPLYID = applyId
            GBA00009MailSendSet.GBA00009setMailToBR()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
                Return
            End If

        End If

        'メッセージ出力
        Me.hdnMsgId.Value = C_MESSAGENO.NORMALENTRYCOST

        '選択タブ保持
        Me.hdnCostSelectedTabId.Value = Me.hdnSelectedTabId.Value

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)
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
                Case Me.vLeftCountry.ID
                    '国選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim countryCode As String = ""
                        If Me.lbCountry.SelectedItem IsNot Nothing Then
                            countryCode = Me.lbCountry.SelectedItem.Value
                        End If
                        SetDisplayCountry(targetTextBox, countryCode)
                    End If

                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If
                Case vLeftCarrier.ID
                    '船選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim carrierCode As String = ""
                        If Me.lbCarrier.SelectedItem IsNot Nothing Then
                            carrierCode = Me.lbCarrier.SelectedItem.Value
                        End If

                        SetDisplayCarrier(targetTextBox, carrierCode)
                    End If

                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If
                Case vLeftConsignee.ID
                    '荷受人選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim customerCode As String = ""
                        If Me.lbConsignee.SelectedItem IsNot Nothing Then
                            customerCode = Me.lbConsignee.SelectedItem.Value
                        End If
                        SetDisplayConsignee(targetTextBox, customerCode)
                    End If
                Case Me.vLeftProduct.ID
                    '積載品選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim productCode As String = ""
                        If Me.lbProduct.SelectedItem IsNot Nothing Then
                            productCode = Me.lbProduct.SelectedItem.Value
                        End If
                        SetDisplayProduct(targetTextBox, productCode)
                    End If
                Case vLeftCost.ID
                    '費用選択時
                    If Me.lbCost.SelectedItem IsNot Nothing Then
                        Dim costCode As String = Me.lbCost.SelectedItem.Value
                        AddNewCostItem(costCode)
                    End If
                Case vLeftContractor.ID
                    Dim carrierCode As String = ""
                    If Me.lbContractor.SelectedItem IsNot Nothing Then
                        carrierCode = Me.lbContractor.SelectedItem.Value

                        SetDisplayContractor(carrierCode)
                    End If

                Case vLeftTerm.ID
                    If Me.lbTerm.SelectedItem IsNot Nothing Then
                        Me.txtBrTerm.Text = Me.lbTerm.SelectedItem.Value
                        Me.lblBrTermText.Text = Me.lbTerm.SelectedItem.Text
                    Else
                        Me.lblBrTermText.Text = ""
                    End If
                Case vLeftBillingCategory.ID
                    If Me.lbBillingCategory.SelectedItem IsNot Nothing Then
                        Me.txtBillingCategory.Text = Me.lbBillingCategory.SelectedItem.Value
                        Me.lblBillingCategoryText.Text = Me.lbBillingCategory.SelectedItem.Text
                    Else
                        Me.lblBillingCategoryText.Text = ""
                    End If

                    If Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.CONSIGNEE Then
                        Me.lblConsignee.CssClass = "requiredMark2"
                    Else
                        Me.lblConsignee.CssClass = ""
                    End If

                Case vLeftAgent.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim carrierCode As String = ""
                        If Me.lbAgent.SelectedItem IsNot Nothing Then
                            carrierCode = Me.lbAgent.SelectedItem.Value
                        End If
                        SetDisplayAgent(targetTextBox, carrierCode)
                    End If

                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If

                    If txtInvoiced.Text <> "" Then
                        Dim country As String = GetInvoicedBy(txtInvoiced.Text)

                        If country = Me.txtRecieptCountry1.Text Then
                            Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.SHIPPER
                        ElseIf country = Me.txtDischargeCountry1.Text Then
                            Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.CONSIGNEE
                        End If
                        Me.txtBillingCategory_Change()
                    End If
                Case vLeftMSDS.ID
                    'MSDS選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim fileName As String = Nothing
                    Dim filePath As String = Nothing
                    Dim prtDir As String = COA0019Session.PRINTWORKDir & "\" & COA0019Session.USERID
                    If targetObject IsNot Nothing Then
                        If Me.lbMSDS.SelectedItem IsNot Nothing Then
                            fileName = Me.lbMSDS.SelectedItem.Text
                            filePath = Me.lbMSDS.SelectedItem.Value

                            'ディレクトリが存在しない場合、作成する
                            If Not System.IO.Directory.Exists(prtDir) Then
                                System.IO.Directory.CreateDirectory(prtDir)
                            End If

                            'ダウンロードファイル送信準備
                            System.IO.File.Copy(filePath, prtDir & "\" & fileName, True)

                            'ダウンロード処理へ遷移
                            hdnPrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & COA0019Session.PRINTROOTUrl & "/" & COA0019Session.USERID & "/" & Uri.EscapeUriString(fileName) 'TODO:固定でIP指定
                            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

                            Return
                        End If
                    End If
                Case Me.vLeftPort.ID
                    '港変更時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim selectedVal As String = ""
                    If Me.lbPort.SelectedItem IsNot Nothing Then
                        selectedVal = Me.lbPort.SelectedItem.Value
                    End If
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        ShowModifiedMessage(targetTextBox, selectedVal)
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

        ElseIf (Me.hdnRemarkboxField.Value = "lblRemarks" OrElse Me.hdnRemarkboxField.Value = "lblRemarks2") And
               Me.hdnRemarkFlg.Value = "1" Then

            Me.btnRemarkInputEdit.Disabled = False
            Me.txtRemarkInput.ReadOnly = True

            Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
            If ViewState("DICBRINFO_REM") IsNot Nothing Then
                brInfo = DirectCast(ViewState("DICBRINFO_REM"), Dictionary(Of String, BreakerInfo))

                '選択タブ判定
                If Me.hdnSelectedTabId.Value = Me.tabOrganizer.ClientID Then
                    'オーガナイザー
                    brInfo("INFO").Remark = Me.txtRemarkInput.Text
                ElseIf Me.hdnSelectedTabId.Value = Me.tabExport1.ClientID Then
                    'POL
                    brInfo("POL1").Remark = Me.txtRemarkInput.Text
                ElseIf Me.hdnSelectedTabId.Value = Me.tabExport2.ClientID Then
                    'POL
                    brInfo("POL2").Remark = Me.txtRemarkInput.Text
                ElseIf Me.hdnSelectedTabId.Value = Me.tabInport1.ClientID Then
                    'POD
                    brInfo("POD1").Remark = Me.txtRemarkInput.Text
                ElseIf Me.hdnSelectedTabId.Value = Me.tabInport2.ClientID Then
                    'POD
                    brInfo("POD2").Remark = Me.txtRemarkInput.Text
                End If

                ViewState("DICBRINFO_REM") = brInfo

            End If

            Me.hdnRemarkFlg.Value = "0"
            Return

        ElseIf (Me.hdnRemarkboxField.Value = "lblRemarks" OrElse Me.hdnRemarkboxField.Value = "lblRemarks2") And
               Me.hdnRemarkFlg.Value <> "1" Then

            Me.hdnRemarkInitFlg.Value = ""

            Dim targetControl As Label = DirectCast(Me.FindControl("lblRemarks"), Label)
            Dim targetControl2 As Label = DirectCast(Me.FindControl("lblRemarks2"), Label)
            Me.txtRemarkInput.Text = Me.txtRemarkInput.Text.Replace("【ORGANIZER】", "").Replace("【POL1】", "").Replace("【POD1】", "").Replace("【POL2】", "").Replace("【POD2】", "")
            targetControl.Text = HttpUtility.HtmlEncode(Me.txtRemarkInput.Text)
            If targetControl.Text.Replace(vbCrLf, "").Replace(" ", "") <> "" Then
                targetControl.CssClass = "hasRemark"
            Else
                targetControl.CssClass = ""
            End If
            targetControl2.Text = HttpUtility.HtmlEncode(Me.txtRemarkInput.Text)
            If targetControl2.Text.Replace(vbCrLf, "").Replace(" ", "") <> "" Then
                targetControl2.CssClass = "hasRemark"
            Else
                targetControl2.CssClass = ""
            End If

            ViewState("DICBRINFO") = ViewState("DICBRINFO_REM")
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

        If (Me.hdnRemarkboxField.Value = "lblRemarks" OrElse Me.hdnRemarkboxField.Value = "lblRemarks2") And
               Me.hdnRemarkFlg.Value = "1" Then

            Me.btnRemarkInputEdit.Disabled = False
            Me.hdnRemarkFlg.Value = "0"
            Return
        End If

        Me.hdnRemarkInitFlg.Value = ""

        Me.hdnRemarkboxOpen.Value = ""
        Me.hdnRemarkboxField.Value = ""
        Me.hdnCurrentUnieuqIndex.Value = ""
        'マルチライン入力ボックスの非表示
        Me.divRemarkInputBoxWrapper.Style("display") = "none"
    End Sub
    ''' <summary>
    ''' 備考入力ボックスのEDITボタン押下時イベント
    ''' </summary>
    Public Sub btnRemarkInputEdit_Click()

        Me.txtRemarkInput.ReadOnly = False
        Me.hdnRemarkFlg.Value = "1"
        Me.btnRemarkInputEdit.Disabled = True
        Me.btnRemarkInputOk.Disabled = False

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        If ViewState("DICBRINFO_REM") IsNot Nothing Then
            brInfo = DirectCast(ViewState("DICBRINFO_REM"), Dictionary(Of String, BreakerInfo))
        End If

        '選択タブ判定
        If Me.hdnSelectedTabId.Value = Me.tabOrganizer.ClientID Then
            'オーガナイザー
            Me.txtRemarkInput.Text = brInfo("INFO").Remark
            Me.hdnRemarkboxFieldName.Value = "SPECIAL INSTRUCTIONS(ORGANIZER)"
        ElseIf Me.hdnSelectedTabId.Value = Me.tabExport1.ClientID Then
            'POL
            Me.txtRemarkInput.Text = brInfo("POL1").Remark
            Me.hdnRemarkboxFieldName.Value = "SPECIAL INSTRUCTIONS(POL1)"
        ElseIf Me.hdnSelectedTabId.Value = Me.tabExport2.ClientID Then
            'POL
            Me.txtRemarkInput.Text = brInfo("POL2").Remark
            Me.hdnRemarkboxFieldName.Value = "SPECIAL INSTRUCTIONS(POL2)"
        ElseIf Me.hdnSelectedTabId.Value = Me.tabInport1.ClientID Then
            'POD
            Me.txtRemarkInput.Text = brInfo("POD1").Remark
            Me.hdnRemarkboxFieldName.Value = "SPECIAL INSTRUCTIONS(POD1)"
        ElseIf Me.hdnSelectedTabId.Value = Me.tabInport2.ClientID Then
            'POD
            Me.txtRemarkInput.Text = brInfo("POD2").Remark
            Me.hdnRemarkboxFieldName.Value = "SPECIAL INSTRUCTIONS(POD2)"
        End If

    End Sub
    ''' <summary>
    ''' 港変更確認メッセージOK押下時
    ''' </summary>
    Public Sub btnConfirmPortModifiedOk_Click()
        SetDisplayPort()
        '画面情報をデータテーブルに格納
        Dim orgDt As DataTable = CollectDisplayOrganizerInfo()
        Dim costDt As DataTable = CollectDisplayCostInfo()
        '費用項目の禁則文字置換
        ChangeInvalidChar(costDt, New List(Of String) From {"COSTCODE", "COSTNAME", "CONTRACTOR", "CURRENCYCODE"})
        '各種データテーブルをデータセットに格納
        Dim ds As New DataSet
        ds.Tables.AddRange({orgDt, costDt})
        'ここで履歴連番・港が変わったほうの申請情報を費用情報を初期化
        EditWhenModifiedPort(ds)
        Dim costList = Me.CreateTemporaryCostList(ds.Tables("COST_INFO"), ds.Tables("ORGANIZER_INFO"))
        ViewState("COSTLIST") = costList
        '費用が初期化されるため費用合計を再計算
        SetDisplayTotalCost()
    End Sub
    ''' <summary>
    ''' 変更した港情報データを変更
    ''' </summary>
    Public Sub EditWhenModifiedPort(ByRef ds As DataSet, Optional IsCopyNewFirstLoad As Boolean = False)
        '変更された発着(POL1,POD1等)を取得
        Dim modifiedPorts As List(Of String) = GetModifiedPort()
        Dim brInfo = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))
        Dim useType As String = brInfo("INFO").UseType
        Dim brId As String = brInfo("INFO").BrId '振り直しする前のBRID
        '新規作成した状態のbrInfoを取得
        Dim dtOrgInfo As DataTable = ds.Tables("ORGANIZER_INFO")
        Dim dtCost As DataTable = ds.Tables("COST_INFO")
        Dim brInfoDummyNew = SetBreakerInfo("", dtOrgInfo)
        '新規作成した状態の紐づけ連番(linkId,SubId)に置き換え
        For Each brInfoDummyNewItem In brInfoDummyNew
            If Not brInfo.ContainsKey(brInfoDummyNewItem.Key) Then
                Continue For
            End If
            With brInfo(brInfoDummyNewItem.Key)
                .LinkId = brInfoDummyNewItem.Value.LinkId
                .SubId = brInfoDummyNewItem.Value.SubId
                .ApplyId = ""  '申請初期化
                .LastStep = "" '最終承認ステップ初期化
            End With
        Next
        '変更があったPortにつき費用項目を新規作成状態に戻す
        Dim dtCostDummyNew = CreateCostData(dtOrgInfo) '輸送パターンを元に新規作成した空データ(全発着あり)
        If IsCopyNewFirstLoad = False AndAlso modifiedPorts.Contains("POL1") Then
            modifiedPorts.Remove("POL1")
        End If
        Dim qNotModCost = From costRow In dtCost Where Not modifiedPorts.Contains(Convert.ToString(costRow("DTLPOLPOD")))
        Dim qNewCost = From costRow In dtCostDummyNew Where modifiedPorts.Contains(Convert.ToString(costRow("DTLPOLPOD")))

        '変更した港の費用情報をクリアし新規挿入、変更無い港の費用情報の港を現状維持し生成
        Dim dtCostResult As DataTable = dtCostDummyNew.Clone '差し替えマージした費用データ（変更してない発着側を保持し変更したものについては新規状態にする）
        For Each qObj In {qNotModCost, qNewCost}
            If qObj.Any Then
                For Each qItem In qObj
                    Dim addRow As DataRow = dtCostResult.NewRow
                    addRow.ItemArray = qItem.ItemArray
                    dtCostResult.Rows.Add(addRow)
                Next '各費用をループし新規テーブル生成
            End If
        Next '変更港費用、現状維持費用ループ
        '返却データセットを差し替え
        ds.Tables.Remove("COST_INFO")
        ds.Tables.Add(dtCostResult)
    End Sub
    ''' <summary>
    ''' MSDS設定
    ''' </summary>
    Public Sub SetMSDSItem()

        Dim dir As String = Nothing
        Dim PdfFiles As String() = Nothing
        Dim fileName As String = Nothing

        'リストクリア
        Me.lbMSDS.Items.Clear()

        '未入力の場合空白
        If Me.txtProduct.Text = "" OrElse Me.txtShipper.Text = "" Then
            Return
        End If

        '正式ディレクトリ
        dir = COA0019Session.UPLOADFILESDir & "\MSDS\" & Me.txtProduct.Text

        'ファイル取得
        If System.IO.Directory.Exists(dir) Then
            PdfFiles = System.IO.Directory.GetFiles(dir)
            For Each tempFile As String In PdfFiles
                fileName = tempFile
                fileName = System.IO.Path.GetFileName(tempFile)
                'ファイル設定
                Me.lbMSDS.Items.Add(New ListItem(fileName, tempFile))
            Next
        Else
            Return
        End If

    End Sub
    ''' <summary>
    ''' 輸送形態変更時
    ''' </summary>
    Public Sub txtBrTerm_Change()
        Dim brTerm As String = Me.txtBrTerm.Text.Trim
        Me.txtBrTerm.Text = brTerm
        Dim findItem As ListItem = Me.lbTerm.Items.FindByValue(brTerm)
        Me.lblBrTermText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblBrTermText.Text = findItem.Text
        End If
    End Sub
    ''' <summary>
    ''' 荷受人変更時
    ''' </summary>
    Public Sub txtConsignee_Change()
        Dim consignee As String = Me.txtConsignee.Text.Trim
        Me.txtConsignee.Text = consignee
        Me.lblConsigneeText.Text = ""
        If consignee <> "" Then
            SetDisplayConsignee(Me.txtConsignee, consignee)
        End If
    End Sub
    ''' <summary>
    ''' 船会社１変更時
    ''' </summary>
    Public Sub txtCarrier1_Change()
        Dim carrierCode As String = Me.txtCarrier1.Text.Trim
        Me.txtCarrier1.Text = carrierCode
        Me.lblCarrier1Text.Text = ""
        If carrierCode <> "" Then
            SetDisplayCarrier(Me.txtCarrier1, carrierCode)
        End If
    End Sub
    ''' <summary>
    ''' 船会社２変更時
    ''' </summary>
    Public Sub txtCarrier2_Change()
        Dim carrierCode As String = Me.txtCarrier2.Text.Trim
        Me.txtCarrier2.Text = carrierCode
        Me.lblCarrier2Text.Text = ""
        If carrierCode <> "" Then
            SetDisplayCarrier(Me.txtCarrier2, carrierCode)
        End If
    End Sub
    ''' <summary>
    ''' 発１港変更時
    ''' </summary>
    Public Sub txtRecieptPort1_Change()
        Dim portCode As String = Me.txtRecieptPort1.Text.Trim
        Me.txtRecieptPort1.Text = portCode
        ShowModifiedMessage(Me.txtRecieptPort1, portCode)

    End Sub
    ''' <summary>
    ''' 発２港変更時
    ''' </summary>
    Public Sub txtRecieptPort2_Change()
        Dim portCode As String = Me.txtRecieptPort2.Text.Trim
        Me.txtRecieptPort2.Text = portCode
        ShowModifiedMessage(Me.txtRecieptPort2, portCode)
    End Sub
    ''' <summary>
    ''' 着１港変更時
    ''' </summary>
    Public Sub txtDischargePort1_Change()
        Dim portCode As String = Me.txtDischargePort1.Text.Trim
        Me.txtDischargePort1.Text = portCode
        ShowModifiedMessage(Me.txtDischargePort1, portCode)
    End Sub
    ''' <summary>
    ''' 着２港変更時
    ''' </summary>
    Public Sub txtDischargePort2_Change()
        Dim portCode As String = Me.txtDischargePort2.Text.Trim
        Me.txtDischargePort2.Text = portCode
        ShowModifiedMessage(Me.txtDischargePort2, portCode)
    End Sub
    ''' <summary>
    ''' 積載品変更時イベント
    ''' </summary>
    Public Sub txtProduct_Change()
        Dim productCode As String = Me.txtProduct.Text.Trim
        Me.txtProduct.Text = productCode
        Me.lblProductText.Text = ""
        If productCode <> "" Then
            SetDisplayProduct(Me.txtProduct, productCode)
        End If
    End Sub
    ''' <summary>
    ''' 船荷証券発行コード変更時
    ''' </summary>
    Public Sub txtInvoiced_Change()
        Dim invoicedBy As String = Me.txtInvoiced.Text.Trim
        Me.txtInvoiced.Text = invoicedBy
        Me.lblInvoicedText.Text = ""
        If invoicedBy <> "" Then
            SetDisplayAgent(Me.txtInvoiced, invoicedBy)
            If Me.txtInvoiced.Text = "" Then
                Me.txtInvoiced.Text = invoicedBy
            End If
        End If

        If txtInvoiced.Text <> "" Then
            Dim country As String = GetInvoicedBy(txtInvoiced.Text)

            If country = Me.txtRecieptCountry1.Text Then
                Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.SHIPPER
            ElseIf country = Me.txtDischargeCountry1.Text Then
                Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.CONSIGNEE
            End If
            Me.txtBillingCategory_Change()
        End If

    End Sub
    ''' <summary>
    ''' 請求先変更時
    ''' </summary>
    Public Sub txtBillingCategory_Change()
        Dim billingCategory As String = Me.txtBillingCategory.Text.Trim
        Me.txtBillingCategory.Text = billingCategory
        Dim findItem As ListItem = Me.lbBillingCategory.Items.FindByValue(billingCategory)
        Me.lblBillingCategoryText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblBillingCategoryText.Text = findItem.Text
        End If

        If Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.CONSIGNEE Then
            Me.lblConsignee.CssClass = "requiredMark2"
        Else
            Me.lblConsignee.CssClass = ""
        End If
    End Sub

    ''' <summary>
    ''' タブクリックイベント
    ''' </summary>
    ''' <param name="tabObjId">クリックしたタブオブジェクトのID</param>
    Protected Sub TabClick(tabObjId As String)
        Dim isOwner As Boolean = False
        '一旦選択されたタブがオーガナイザの場合はオーナーとする
        If tabObjId = Me.tabOrganizer.ClientID Then
            isOwner = True
        End If

        Dim beforeTab As String = ""
        Dim selectedTab As String = ""
        Dim tabObjects As New List(Of HtmlGenericControl)
        tabObjects.Add(Me.tabOrganizer)
        tabObjects.Add(Me.tabExport1)
        tabObjects.Add(Me.tabInport1)
        tabObjects.Add(Me.tabExport2)
        tabObjects.Add(Me.tabInport2)

        For Each tabObject In tabObjects
            If tabObject.Attributes("class") IsNot Nothing AndAlso tabObject.Attributes("class").Contains("selected") Then
                beforeTab = tabObject.ID
            End If
            tabObject.Attributes.Remove("class")
            If tabObjId = tabObject.ID Then
                tabObject.Attributes.Add("class", "selected")
                selectedTab = tabObject.ID

            End If

        Next

        SetCostGridItem(beforeTab, selectedTab)
        SetDisplayTotalCost()
        visibleControl(isOwner, selectedTab)
        RightboxInit(isOwner, COSTITEM.CostItemGroup.Export1)
        SetStatus(selectedTab)
        SetCountryControl(selectedTab)
        If selectedTab <> Me.tabOrganizer.ID Then
            CalcSummaryCostLocal()
            CalcSummaryCostUsd()
        End If
        '費用項目非活性制御
        CostEnabledControls()

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
        AddLangSetting(dicDisplayText, Me.lblRightInfo1, "ダブルクリックを行い入力を確定してください。", "Double click to confirm input.")
        AddLangSetting(dicDisplayText, Me.lblRightInfo2, "ダブルクリックを行い入力を確定してください。", "Double click to confirm input.")
        '****************************************
        ' 共通情報部分
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblBrInfoHeader, "BR-Info", "BR-Info")
        AddLangSetting(dicDisplayText, Me.lblBrType, "種類", "USE TYPE")
        AddLangSetting(dicDisplayText, Me.lblBrStYmd, "有効期限", "VALIDITY")
        AddLangSetting(dicDisplayText, Me.lblBrRemark, "BR注記", "NOTE")
        AddLangSetting(dicDisplayText, Me.lblBrTerm, "輸送形態", "TERM")
        AddLangSetting(dicDisplayText, Me.lblNoOfTanks, "Monthly Volume.", "Monthly Volume.")
        'AddLangSetting(dicDisplayText, Me.lblNoOfTanks, "タンク本数", "NO of Tanks")
        AddLangSetting(dicDisplayText, Me.lblInvoiced, "船荷証券発行者", "INVOICED BY")
        AddLangSetting(dicDisplayText, Me.lblApploveDate, "DATE", "DATE")
        AddLangSetting(dicDisplayText, Me.lblAgent, "AGENT", "AGENT")
        AddLangSetting(dicDisplayText, Me.lblPic, "PIC", "PIC")
        AddLangSetting(dicDisplayText, Me.lblAppRemarks, "REMARKS", "REMARKS")
        AddLangSetting(dicDisplayText, Me.lblApproval, "Apply", "Apply")
        AddLangSetting(dicDisplayText, Me.lblApproved, "Approved", "Approved")
        AddLangSetting(dicDisplayText, Me.lblShipperConsigneeinfoHeader, "Shipper/Consignee/Carrier-Info", "Shipper/Consignee/Carrier-Info")
        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主", "SHIPPER")
        AddLangSetting(dicDisplayText, Me.lblConsignee, "荷受人", "CONSIGNEE")
        AddLangSetting(dicDisplayText, Me.lblCarrier1, "船会社1", "CARRIER1")
        AddLangSetting(dicDisplayText, Me.lblCarrier2, "船会社2", "CARRIER2")
        AddLangSetting(dicDisplayText, Me.lblProductTankInfoHeader, "Product/Tank-Info", "Product/Tank-Info")
        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品", "PRODUCT")
        AddLangSetting(dicDisplayText, Me.lblImdg, "危険品等級", "IMDG")
        AddLangSetting(dicDisplayText, Me.lblUNNo, "国連番号", "UN No.")

        '****************************************
        ' オーナーのみ情報部分
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblPortPlaceInfoHeader, "Port/Place-Info", "Port/Place-Info")
        AddLangSetting(dicDisplayText, Me.lblCountry1, "COUNTRY", "COUNTRY")
        AddLangSetting(dicDisplayText, Me.lblPort1, "PORT", "PORT")
        AddLangSetting(dicDisplayText, Me.lblCountry2, "COUNTRY", "COUNTRY")
        AddLangSetting(dicDisplayText, Me.lblPort2, "PORT", "PORT")

        AddLangSetting(dicDisplayText, Me.lblExport1Row, "輸出1(Export)", "Export1")
        AddLangSetting(dicDisplayText, Me.lblInport1Row, "輸入1(Import)", "Import1")
        AddLangSetting(dicDisplayText, Me.lblExport2Row, "輸出2(Export)", "Export2")
        AddLangSetting(dicDisplayText, Me.lblInport2Row, "輸入2(Import)", "Import2")

        AddLangSetting(dicDisplayText, Me.lblCarrierInfoHeader, "Carrier-SubInfo", "Carrier-SubInfo")
        AddLangSetting(dicDisplayText, Me.lblVsl1, "船名1", "VSL1")
        AddLangSetting(dicDisplayText, Me.lblVoy1, "航海番号1", "VOY1")
        AddLangSetting(dicDisplayText, Me.lblEtd1, "出発日1", "ETD1")
        AddLangSetting(dicDisplayText, Me.lblEta1, "到着日1", "ETA1")
        AddLangSetting(dicDisplayText, Me.lblVsl2, "船名2", "VSL2")
        AddLangSetting(dicDisplayText, Me.lblVoy2, "航海番号2", "VOY2")
        AddLangSetting(dicDisplayText, Me.lblEtd2, "出発日2", "ETD2")
        AddLangSetting(dicDisplayText, Me.lblEta2, "到着日2", "ETA2")

        AddLangSetting(dicDisplayText, Me.lblProductTankSubinfoHeader, "Product/Tank-SubInfo", "Product/Tank-SubInfo")
        AddLangSetting(dicDisplayText, Me.lblMSDS, "[MSDS]", "[MSDS]")
        AddLangSetting(dicDisplayText, Me.lblWeight, "積載重量", "WEIGHT")
        AddLangSetting(dicDisplayText, Me.lblSGravity, "比重", "S.GRAVITY")
        AddLangSetting(dicDisplayText, Me.lblTankCapacity, "タンク容量", "CAPACITY")
        AddLangSetting(dicDisplayText, Me.lblTankFillingRate, "タンク積載％", "TANK FILLING RATE")
        AddLangSetting(dicDisplayText, Me.lblTankFillingCheck, "ﾁｪｯｸ結果", "CHECK")

        AddLangSetting(dicDisplayText, Me.lblHireageInfoHeader, "Hireage-Info", "Hireage-Info")
        AddLangSetting(dicDisplayText, Me.lblTotal, "期間合計", "TOTAL")
        AddLangSetting(dicDisplayText, Me.lblLoading, "発側期間", "LOADING")
        AddLangSetting(dicDisplayText, Me.lblSteaming, "船上期間", "STEAMING")
        AddLangSetting(dicDisplayText, Me.lblTip, "着側期間", "TIP")
        AddLangSetting(dicDisplayText, Me.lblExtra, "追加期間", "EXTRA")
        AddLangSetting(dicDisplayText, Me.lblJOTHireage, "JOT総額", "HIREAGE")
        AddLangSetting(dicDisplayText, Me.lblCommercialFactor, "調整", "ADJUSTMENT")
        AddLangSetting(dicDisplayText, Me.lblInvoicedTotal, "総額", "TOTAL INVOICED")
        AddLangSetting(dicDisplayText, Me.lblPerDay, "PerDay", "PerDay")
        AddLangSetting(dicDisplayText, Me.lblAmount, "総額変更", "AMOUNT")
        AddLangSetting(dicDisplayText, Me.lblAmtRequest, "要求", "Sp.REQUEST")
        AddLangSetting(dicDisplayText, Me.lblAmtPrincipal, "確認", "Sp.PRINCIPAL")
        AddLangSetting(dicDisplayText, Me.lblAmtDiscount, "差額", "Sp.DISCOUNT")

        AddLangSetting(dicDisplayText, Me.lblHireageJPYInfoHeader, "Hireage-Info(円)", "Hireage-Info(JPY)")
        AddLangSetting(dicDisplayText, Me.lblJOTHireageJPY, "JOT総額(円)", "HIREAGE(JPY)")
        AddLangSetting(dicDisplayText, Me.lblCommercialFactorJPY, "調整(円)", "ADJUSTMENT(JPY)")
        AddLangSetting(dicDisplayText, Me.lblInvoicedTotalJPY, "総額(円)", "TOTAL INVOICED(JPY)")
        AddLangSetting(dicDisplayText, Me.lblPerDayJPY, "PerDay(円)", "PerDay(JPY)")
        AddLangSetting(dicDisplayText, Me.lblAmountJPY, "総額変更", "AMOUNT")
        AddLangSetting(dicDisplayText, Me.lblAmtRequestJPY, "要求(円)", "Sp.REQUEST(JPY)")
        AddLangSetting(dicDisplayText, Me.lblAmtPrincipalJPY, "確認(円)", "Sp.PRINCIPAL(JPY)")
        AddLangSetting(dicDisplayText, Me.lblAmtDiscountJPY, "差額(円)", "Sp.DISCOUNT(JPY)")

        AddLangSetting(dicDisplayText, Me.lblCostInfoHeader, "Cost-Info", "Cost-Info")
        AddLangSetting(dicDisplayText, Me.lblFee, "手数料", "COMMISSION")
        AddLangSetting(dicDisplayText, Me.lblBillingCategory, "請求先", "BILLING")

        AddLangSetting(dicDisplayText, Me.lblDemurrageInfoHeader, "Demurrage-Info", "Demurrage-Info")
        AddLangSetting(dicDisplayText, Me.lblDemurday1, "一次期間", "DAYS")
        AddLangSetting(dicDisplayText, Me.lblDemurday2, "二次期間", "THEREAFTER")

        AddLangSetting(dicDisplayText, Me.lblDetailInfoHeadedr, "BRdetail-Info", "BRdetail-Info")
        '****************************************
        ' 発・着要素
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblAgencySummary, "各代理店合計", "Cost Summary")
        AddLangSetting(dicDisplayText, Me.lblLocalRateRef, "Loc.Cur Rate", "Loc.Cur Rate")
        'AddLangSetting(dicDisplayText, Me.lblUSDRateRef, "TTM Rate", "TTM Rate")
        '****************************************
        ' 各種ボタン
        '****************************************
        AddLangSetting(dicDisplayText, Me.btnAddCost, "費用追加", "Add Cost")
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnApply, "申請", "Apply")
        AddLangSetting(dicDisplayText, Me.btnInputRequest, "登録", "Input Request")
        AddLangSetting(dicDisplayText, Me.btnApproval, "承認", "Approval")
        AddLangSetting(dicDisplayText, Me.btnAppReject, "否認", "Reject")
        AddLangSetting(dicDisplayText, Me.btnReject, "編集", "Edit")
        AddLangSetting(dicDisplayText, Me.btnOutputExcel, "ﾃﾞｰﾀﾀﾞｳﾝﾛｰﾄﾞ", "Data Download")
        AddLangSetting(dicDisplayText, Me.btnPrint, "Print", "Print")
        AddLangSetting(dicDisplayText, Me.btnEntryCost, "費用登録", "Entry Done")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        '****************************************
        ' 送付先ポップアップ
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblSendTargetMessage, "送付先選択", "Select mail recipient")

        AddLangSetting(dicDisplayText, Me.btnSelectMailOk, "OK", "OK")
        AddLangSetting(dicDisplayText, Me.btnSelectMailCancel, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.chkMailExport1, "輸出1", "Export1")
        AddLangSetting(dicDisplayText, Me.chkMailInport1, "輸入1", "Import1")
        AddLangSetting(dicDisplayText, Me.chkMailExport2, "輸出2", "Export2")
        AddLangSetting(dicDisplayText, Me.chkMailInport2, "輸入2", "Import2")
        '****************************************
        ' 送信有無選択ポップアップ
        '****************************************
        'AddLangSetting(dicDisplayText, Me.lblEntryCostSendTargetMessage, "送信有無選択", "Select whether to send mail")

        AddLangSetting(dicDisplayText, Me.btnEntryCostSelectMailOk, "OK", "OK")
        AddLangSetting(dicDisplayText, Me.btnEntryCostSelectMailCancel, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.chkMailSend, "送信", "Send")

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
        AddLangSetting(dicDisplayText, Me.hdnDispLeftBoxCostName, "費用名称", "Cost Name")
        AddLangSetting(dicDisplayText, Me.hdnRemarkEmptyMessage, "DoubleClick to input", "DoubleClick to input")
        'ファイルアップロードメッセージ
        AddLangSetting(dicDisplayText, Me.hdnUploadMessage01, "ファイルアップロード開始", "Start uploading files")
        AddLangSetting(dicDisplayText, Me.hdnUploadError01, "ファイルアップロードが失敗しました。", "File upload failed.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError02, "通信を中止しました。", "Communication was canceled.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError03, "タイムアウトエラーが発生しました。", "A timeout error occurred.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError04, "更新権限がありません。", "do not have update permission.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError05, "対応外のファイル形式です。", "It is an incompatible file format.")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)

        '****************************************
        ' グリッドヘッダーの表示文言設定(GrivViewだけは個別制御が必要)
        '****************************************
        Dim dicGridDisplayText As New Dictionary(Of String, Dictionary(Of String, String))
        dicGridDisplayText.Add("CostCodeCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "コード"}, {C_LANG.EN, "Code"}})
        dicGridDisplayText.Add("CostNameCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "名称"}, {C_LANG.EN, "Cost Name"}})
        dicGridDisplayText.Add("BlCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "B/L"}, {C_LANG.EN, "B/L"}})
        dicGridDisplayText.Add("JOTCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "JOT"}, {C_LANG.EN, "JOT"}})
        dicGridDisplayText.Add("SCCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "BILL"}, {C_LANG.EN, "BILL"}})
        dicGridDisplayText.Add("TaxationCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "課税"}, {C_LANG.EN, "Taxation"}})
        dicGridDisplayText.Add("BaseOnCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "数量"}, {C_LANG.EN, "Based on"}})
        dicGridDisplayText.Add("LocalCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "現地金額"}, {C_LANG.EN, "LOCAL"}})
        dicGridDisplayText.Add("USDCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "USD金額"}, {C_LANG.EN, "USD"}})
        dicGridDisplayText.Add("ContractorCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "業者コード"}, {C_LANG.EN, "Vendor Code"}})
        dicGridDisplayText.Add("ContractorTextCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "業者名"}, {C_LANG.EN, "Vendor Name"}})
        dicGridDisplayText.Add("LocalRateCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "現地通貨換算RATE"}, {C_LANG.EN, "Loc.Cur Rate"}})
        dicGridDisplayText.Add("RemarksCell",
                           New Dictionary(Of String, String) From {{C_LANG.JA, "所見"}, {C_LANG.EN, "REMARK"}})

        If gvDetailInfo.Columns.Count > 0 Then
            '最大列数取得
            Dim colMaxIndex As Integer = gvDetailInfo.Columns.Count - 1
            '列のループ
            For i = 0 To colMaxIndex
                Dim fldObj As DataControlField = gvDetailInfo.Columns(i)
                '変換ディクショナリに対象カラム名を置換が設定されている場合文言変更
                If dicGridDisplayText.ContainsKey(fldObj.HeaderStyle.CssClass) = True Then
                    fldObj.HeaderText = dicGridDisplayText(fldObj.HeaderStyle.CssClass)(lang)
                End If
            Next
        End If
    End Sub
    ''' <summary>
    ''' 遷移元（前画面）の情報を取得
    ''' </summary>
    Private Function GetPrevDisplayInfo(ByRef retDataSet As DataSet) As String

        Dim GBA00006PortRelated As GBA00006PortRelated = New GBA00006PortRelated
        Dim dummyList As ListBox = New ListBox
        Dim retVal As String = C_MESSAGENO.NORMAL
        Me.hdnCallerMapId.Value = Convert.ToString(HttpContext.Current.Session("MAPmapid"))
        If TypeOf Page.PreviousPage Is GBT00001NEWBREAKER Then
            '新規ページからの遷移
            Dim dtBreakerBase As DataTable = Nothing
            Dim costDt As DataTable = Nothing
            Dim prevNewBreakerPage As GBT00001NEWBREAKER = DirectCast(Page.PreviousPage, GBT00001NEWBREAKER)
            Dim hdnPrevMapVari As HiddenField = DirectCast(prevNewBreakerPage.FindControl("hdnThisMapVariant"), HiddenField)
            Dim initDate As Date = New Date(Date.Now.Year, Date.Now.Month, 1)
            initDate = initDate.AddMonths(2).AddDays(-1)
            If hdnPrevMapVari.Value.EndsWith("New") Then
                dtBreakerBase = CreateOrganizerInfoTable()
                Dim dr As DataRow = dtBreakerBase.Rows(0)
                Dim hasError As Boolean = False
                '前ページの内容をデータテーブルに格納
                'initDate = initDate.AddMonths(2).AddDays(-1)
                With prevNewBreakerPage
                    dr.Item("BRTYPE") = DirectCast(.FindControl("txtBreakerType"), TextBox).Text
                    Me.hdnBrType.Value = DirectCast(.FindControl("txtBreakerType"), TextBox).Text
                    dr.Item("USETYPE") = DirectCast(.FindControl("txtTransferPattern"), TextBox).Text

                    '月末日の取得
                    dr.Item("VALIDITYFROM") = Date.Now.ToString("yyyy/MM/dd")
                    dr.Item("VALIDITYTO") = initDate.ToString("yyyy/MM/dd")

                    'ブレーカー使用不可フラグ
                    dr.Item("DISABLED") = CONST_FLAG_NO 'N(使用可)

                    dr.Item("TERMTYPE") = "CC" '一旦CY-CY
                    '三国間か
                    dr.Item("ISTRILATERAL") = DirectCast(.FindControl("hdnIsTrilateral"), HiddenField).Value
                    'リースタンク利用輸送か
                    dr.Item("USINGLEASETANK") = If(DirectCast(.FindControl("chkLeaseTankUse"), CheckBox).Checked, "1", "0")
                    '発地1
                    dr.Item("RECIEPTCOUNTRY1") = DirectCast(.FindControl("hdnPolCountry1"), HiddenField).Value
                    dr.Item("RECIEPTPORT1") = DirectCast(.FindControl("txtPolPort1"), TextBox).Text
                    dr.Item("LOADCOUNTRY1") = dr.Item("RECIEPTCOUNTRY1")
                    dr.Item("LOADPORT1") = dr.Item("RECIEPTPORT1")
                    '着地1
                    dr.Item("DISCHARGECOUNTRY1") = DirectCast(.FindControl("hdnPodCountry1"), HiddenField).Value
                    dr.Item("DISCHARGEPORT1") = DirectCast(.FindControl("txtPodPort1"), TextBox).Text
                    dr.Item("DELIVERYCOUNTRY1") = dr.Item("DISCHARGECOUNTRY1")
                    dr.Item("DELIVERYPORT1") = dr.Item("DISCHARGEPORT1")

                    If DirectCast(.FindControl("hdnInitProductCode"), HiddenField).Value <> "" Then
                        dr.Item("PRODUCTCODE") = DirectCast(.FindControl("hdnInitProductCode"), HiddenField).Value
                    Else
                        dr.Item("PRODUCTCODE") = ""
                    End If

                    'AgentPol1
                    'dr.Item("AGENTPOL1") = dr.Item("RECIEPTCOUNTRY1") & "A" & "00001"
                    If DirectCast(.FindControl("hdnInitAgentPol1"), HiddenField).Value <> "" Then
                        dr.Item("AGENTPOL1") = DirectCast(.FindControl("hdnInitAgentPol1"), HiddenField).Value
                    Else
                        GBA00006PortRelated.PORTCODE = Convert.ToString(dr.Item("LOADPORT1"))
                        GBA00006PortRelated.LISTBOX_OFFICE = dummyList
                        GBA00006PortRelated.GBA00006getLeftListOffice()
                        If GBA00006PortRelated.OfficeKeyValue.Count > 0 Then
                            dr.Item("AGENTPOL1") = GBA00006PortRelated.OfficeKeyValue.First.Key
                        End If
                    End If

                    'AgentPod1
                    'dr.Item("AGENTPOD1") = dr.Item("DISCHARGECOUNTRY1") & "A" & "00001"
                    If DirectCast(.FindControl("hdnInitAgentPod1"), HiddenField).Value <> "" Then
                        dr.Item("AGENTPOD1") = DirectCast(.FindControl("hdnInitAgentPod1"), HiddenField).Value
                    Else
                        GBA00006PortRelated.PORTCODE = Convert.ToString(dr.Item("DISCHARGEPORT1"))
                        GBA00006PortRelated.LISTBOX_OFFICE = dummyList
                        GBA00006PortRelated.GBA00006getLeftListOffice()
                        If GBA00006PortRelated.OfficeKeyValue.Count > 0 Then
                            dr.Item("AGENTPOD1") = GBA00006PortRelated.OfficeKeyValue.First.Key
                        End If
                    End If
                    'INVOICEDBY
                    If DirectCast(.FindControl("hdnInitInvoicedBy"), HiddenField).Value <> "" Then
                        dr.Item("INVOICEDBY") = DirectCast(.FindControl("hdnInitInvoicedBy"), HiddenField).Value
                    Else
                        dr.Item("INVOICEDBY") = GBA00003UserSetting.OFFICECODE
                    End If

                    '請求先
                    Dim country As String = GetInvoicedBy(dr.Item("INVOICEDBY").ToString)
                    'If country = dr.Item("DISCHARGECOUNTRY1").ToString Then
                    '    dr.Item("BILLINGCATEGORY") = GBC_DELIVERYCLASS.CONSIGNEE
                    '    'bilFlg = False
                    'Else
                    '    dr.Item("BILLINGCATEGORY") = GBC_DELIVERYCLASS.SHIPPER
                    '    'bilFlg = True
                    'End If
                    If DirectCast(.FindControl("hdnInitBillingCategory"), HiddenField).Value <> "" Then
                        dr.Item("BILLINGCATEGORY") = DirectCast(.FindControl("hdnInitBillingCategory"), HiddenField).Value
                    Else
                        dr.Item("BILLINGCATEGORY") = ""
                    End If

                    If Convert.ToString(dr.Item("ISTRILATERAL")) = "1" Then
                        '発地2
                        dr.Item("RECIEPTCOUNTRY2") = DirectCast(.FindControl("hdnPolCountry2"), HiddenField).Value
                        dr.Item("RECIEPTPORT2") = DirectCast(.FindControl("txtPolPort2"), TextBox).Text
                        dr.Item("LOADCOUNTRY2") = dr.Item("RECIEPTCOUNTRY2")
                        dr.Item("LOADPORT2") = dr.Item("RECIEPTPORT2")
                        '着地2
                        dr.Item("DISCHARGECOUNTRY2") = DirectCast(.FindControl("hdnPodCountry2"), HiddenField).Value
                        dr.Item("DISCHARGEPORT2") = DirectCast(.FindControl("txtPodPort2"), TextBox).Text
                        dr.Item("DELIVERYCOUNTRY2") = dr.Item("DISCHARGECOUNTRY2")
                        dr.Item("DELIVERYPORT2") = dr.Item("DISCHARGEPORT2")

                        'AgentPol2
                        'dr.Item("AGENTPOL2") = dr.Item("RECIEPTCOUNTRY2") & "A" & "00001"
                        If DirectCast(.FindControl("hdnInitAgentPol2"), HiddenField).Value <> "" Then
                            dr.Item("AGENTPOL2") = DirectCast(.FindControl("hdnInitAgentPol2"), HiddenField).Value
                        Else
                            GBA00006PortRelated.PORTCODE = Convert.ToString(dr.Item("LOADPORT2"))
                            GBA00006PortRelated.LISTBOX_OFFICE = dummyList
                            GBA00006PortRelated.GBA00006getLeftListOffice()
                            If GBA00006PortRelated.OfficeKeyValue.Count > 0 Then
                                dr.Item("AGENTPOL2") = GBA00006PortRelated.OfficeKeyValue.First.Key
                            End If
                        End If
                        'AgentPod2
                        'dr.Item("AGENTPOD2") = dr.Item("DISCHARGECOUNTRY2") & "A" & "00001"
                        If DirectCast(.FindControl("hdnInitAgentPod2"), HiddenField).Value <> "" Then
                            dr.Item("AGENTPOD2") = DirectCast(.FindControl("hdnInitAgentPod2"), HiddenField).Value
                        Else
                            GBA00006PortRelated.PORTCODE = Convert.ToString(dr.Item("DISCHARGEPORT2"))
                            GBA00006PortRelated.LISTBOX_OFFICE = dummyList
                            GBA00006PortRelated.GBA00006getLeftListOffice()
                            If GBA00006PortRelated.OfficeKeyValue.Count > 0 Then
                                dr.Item("AGENTPOD2") = GBA00006PortRelated.OfficeKeyValue.First.Key
                            End If
                        End If
                    End If
                    dr.Item("SHIPPER") = DirectCast(.FindControl("txtShipper"), TextBox).Text
                    If DirectCast(.FindControl("hdnInitConsignee"), HiddenField).Value <> "" Then
                        dr.Item("CONSIGNEE") = DirectCast(.FindControl("hdnInitConsignee"), HiddenField).Value
                    Else
                        dr.Item("CONSIGNEE") = ""
                    End If

                    dr.Item("COUNTRYORGANIZER") = GBA00003UserSetting.COUNTRYCODE
                    dr.Item("AGENTORGANIZER") = GBA00003UserSetting.OFFICECODE

                    dr.Item("CAPACITY") = GetCapacity()

                    GetOrgInfo()
                    dr.Item("DEMURTO") = Me.txtDemurdayT1.Text
                    dr.Item("DEMURUSRATE1") = Me.txtDemurUSRate1.Text
                    dr.Item("DEMURUSRATE2") = Me.txtDemurUSRate2.Text
                    dr.Item("TIP") = Me.txtTip.Text
                End With

                '費用情報取得
                costDt = CreateCostData(dtBreakerBase)
                Dim qPolOnly = From cItm As DataRow In costDt Where cItm("DTLPOLPOD").Equals("POD1")
                If qPolOnly.Any = True Then
                    ViewState(CONST_VS_DISP_POLONLY) = "0"
                Else
                    ViewState(CONST_VS_DISP_POLONLY) = "1"
                End If
                Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
                'ブレーカー紐づけ情報作成
                brInfo = SetBreakerInfo("", dtBreakerBase)
                ViewState("DICBRINFO") = brInfo
                'メイン情報取得
                retDataSet.Tables.Add(dtBreakerBase)
                retDataSet.Tables.Add(costDt)

            Else 'コピー時新規遷移

                Dim brNo As String = ""
                Dim hdnBrIdObj As HiddenField = Nothing
                hdnBrIdObj = DirectCast(prevNewBreakerPage.FindControl("hdnCopyBaseBrId"), HiddenField)

                brNo = hdnBrIdObj.Value
                Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo, withoutApplyInfo:=True)
                Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
                If brInfoOrganizer.BrType = C_BRTYPE.SALES Then
                    Me.hdnBrType.Value = "1"
                Else
                    Me.hdnBrType.Value = "2"
                End If

                'メイン情報取得
                dtBreakerBase = GetBreakerBase(dicBrInfo)
                'コピー新規時はAGENTORGANIZERを再設定
                dtBreakerBase.Rows(0).Item("AGENTORGANIZER") = GBA00003UserSetting.OFFICECODE
                ViewState("COPYORGANIZERINFO") = CommonFunctions.DeepCopy(dtBreakerBase)
                ViewState("INITORGANIZERINFO") = CommonFunctions.DeepCopy(dtBreakerBase)
                Dim dr As DataRow = dtBreakerBase.Rows(0)
                'コピー新規で前画面から書き換える項目
                With prevNewBreakerPage
                    dr.Item("BRTYPE") = DirectCast(.FindControl("txtBreakerType"), TextBox).Text
                    Me.hdnBrType.Value = DirectCast(.FindControl("txtBreakerType"), TextBox).Text

                    dr.Item("VALIDITYFROM") = Date.Now.ToString("yyyy/MM/dd")
                    dr.Item("VALIDITYTO") = initDate.ToString("yyyy/MM/dd")

                    'ブレーカー使用不可フラグ
                    dr.Item("DISABLED") = CONST_FLAG_NO 'N(使用可)

                    '三国間か
                    dr.Item("ISTRILATERAL") = DirectCast(.FindControl("hdnIsTrilateral"), HiddenField).Value

                    '発地1
                    dr.Item("RECIEPTCOUNTRY1") = DirectCast(.FindControl("hdnPolCountry1"), HiddenField).Value
                    dr.Item("RECIEPTPORT1") = DirectCast(.FindControl("txtPolPort1"), TextBox).Text
                    dr.Item("LOADCOUNTRY1") = dr.Item("RECIEPTCOUNTRY1")
                    dr.Item("LOADPORT1") = dr.Item("RECIEPTPORT1")
                    '着地1
                    dr.Item("DISCHARGECOUNTRY1") = DirectCast(.FindControl("hdnPodCountry1"), HiddenField).Value
                    dr.Item("DISCHARGEPORT1") = DirectCast(.FindControl("txtPodPort1"), TextBox).Text
                    dr.Item("DELIVERYCOUNTRY1") = dr.Item("DISCHARGECOUNTRY1")
                    dr.Item("DELIVERYPORT1") = dr.Item("DISCHARGEPORT1")

                    'AgentPol1
                    'dr.Item("AGENTPOL1") = dr.Item("RECIEPTCOUNTRY1") & "A" & "00001"
                    GBA00006PortRelated.PORTCODE = Convert.ToString(dr.Item("LOADPORT1"))
                    GBA00006PortRelated.LISTBOX_OFFICE = dummyList
                    GBA00006PortRelated.GBA00006getLeftListOffice()
                    dr.Item("AGENTPOL1") = GBA00006PortRelated.OfficeKeyValue.First.Key

                    'AgentPod1
                    'dr.Item("AGENTPOD1") = dr.Item("DISCHARGECOUNTRY1") & "A" & "00001"
                    GBA00006PortRelated.PORTCODE = Convert.ToString(dr.Item("DISCHARGEPORT1"))
                    GBA00006PortRelated.LISTBOX_OFFICE = dummyList
                    GBA00006PortRelated.GBA00006getLeftListOffice()
                    If GBA00006PortRelated.OfficeKeyValue.Count > 0 Then
                        dr.Item("AGENTPOD1") = GBA00006PortRelated.OfficeKeyValue.First.Key
                    End If
                    If Convert.ToString(dr.Item("ISTRILATERAL")) = "1" Then
                        '発地2
                        dr.Item("RECIEPTCOUNTRY2") = DirectCast(.FindControl("hdnPolCountry2"), HiddenField).Value
                        dr.Item("RECIEPTPORT2") = DirectCast(.FindControl("txtPolPort2"), TextBox).Text
                        dr.Item("LOADCOUNTRY2") = dr.Item("RECIEPTCOUNTRY2")
                        dr.Item("LOADPORT2") = dr.Item("RECIEPTPORT2")
                        '着地2
                        dr.Item("DISCHARGECOUNTRY2") = DirectCast(.FindControl("hdnPodCountry2"), HiddenField).Value
                        dr.Item("DISCHARGEPORT2") = DirectCast(.FindControl("txtPodPort2"), TextBox).Text
                        dr.Item("DELIVERYCOUNTRY2") = dr.Item("DISCHARGECOUNTRY2")
                        dr.Item("DELIVERYPORT2") = dr.Item("DISCHARGEPORT2")

                        'AgentPol2
                        'dr.Item("AGENTPOL2") = dr.Item("RECIEPTCOUNTRY2") & "A" & "00001"
                        GBA00006PortRelated.PORTCODE = Convert.ToString(dr.Item("LOADPORT2"))
                        GBA00006PortRelated.LISTBOX_OFFICE = dummyList
                        GBA00006PortRelated.GBA00006getLeftListOffice()
                        dr.Item("AGENTPOL2") = GBA00006PortRelated.OfficeKeyValue.First.Key
                        'AgentPod2
                        'dr.Item("AGENTPOD2") = dr.Item("DISCHARGECOUNTRY2") & "A" & "00001"
                        GBA00006PortRelated.PORTCODE = Convert.ToString(dr.Item("DISCHARGEPORT2"))
                        GBA00006PortRelated.LISTBOX_OFFICE = dummyList
                        GBA00006PortRelated.GBA00006getLeftListOffice()
                        dr.Item("AGENTPOD2") = GBA00006PortRelated.OfficeKeyValue.First.Key

                    End If
                    dr.Item("SHIPPER") = DirectCast(.FindControl("txtShipper"), TextBox).Text
                    '申請関連はクリア
                    For Each applyField In {"APPROVEDTEXT", "APPLYDATE", "APPLICANTID", "APPLICANTNAME", "APPROVEDATE", "APPROVERID", "APPROVERNAME"}
                        dr.Item(applyField) = ""
                    Next
                End With

                ViewState("DICBRINFO") = dicBrInfo
                '費用情報取得
                costDt = GetBreakerValue(dicBrInfo)
                ModifyExrateCopyBr(dtBreakerBase, costDt)
                Dim qPolOnly = From cItm As DataRow In costDt Where cItm("DTLPOLPOD").Equals("POD1")
                If qPolOnly.Any = True Then
                    ViewState(CONST_VS_DISP_POLONLY) = "0"
                Else
                    ViewState(CONST_VS_DISP_POLONLY) = "1"
                End If
                lblBrNo.Attributes.Add("CopyNew", "1")
                'メイン情報取得
                retDataSet.Tables.Add(dtBreakerBase)
                retDataSet.Tables.Add(costDt)
                '一旦画面展開
                SetDisplayOrganizerInfo(dtBreakerBase)
                'コピー新規の場合は連番をクリアしアイテムを新規状態にする
                EditWhenModifiedPort(retDataSet, True)
            End If

            '検索条件
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                               {"hdnEndYMD", Me.hdnEndYMD},
                                                               {"hdnShipper", Me.hdnShipper},
                                                               {"hdnConsignee", Me.hdnConsignee},
                                                               {"hdnPort", Me.hdnPort},
                                                               {"hdnApproval", Me.hdnApproval},
                                                               {"hdnOffice", Me.hdnOffice},
                                                               {"hdnSearchBreakerType", Me.hdnSearchBreakerType}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevNewBreakerPage.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            Me.hdnNewBreaker.Value = "1"
            Me.hdnPol1Status.Value = ""
            Me.hdnPol2Status.Value = ""
            Me.hdnPod1Status.Value = ""
            Me.hdnPod2Status.Value = ""

        ElseIf TypeOf Page.PreviousPage Is GBT00001BREAKER Then
            PreProcType = "自画面遷移"
            '自身からの遷移(Save時に反応)
            Dim brNo As String = ""
            Dim prevPage As GBT00001BREAKER = DirectCast(Page.PreviousPage, GBT00001BREAKER)
            brNo = prevPage.lblBrNo.Text
            Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo)
            ViewState("DICBRINFO") = dicBrInfo
            Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
            If brInfoOrganizer.BrType = C_BRTYPE.SALES Then
                Me.hdnBrType.Value = "1"
            Else
                Me.hdnBrType.Value = "2"
            End If
            'メイン情報取得
            Dim dt As DataTable = GetBreakerBase(dicBrInfo)
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
                                                                        {"hdnSearchBreakerType", Me.hdnSearchBreakerType},
                                                                        {"hdnPrevViewID", Me.hdnPrevViewID},
                                                                        {"hdnPol1Status", Me.hdnPol1Status},
                                                                        {"hdnPol2Status", Me.hdnPol2Status},
                                                                        {"hdnPod1Status", Me.hdnPod1Status},
                                                                        {"hdnPod2Status", Me.hdnPod2Status},
                                                                        {"hdnPOLPort", Me.hdnPOLPort},
                                                                        {"hdnPODPort", Me.hdnPODPort},
                                                                        {"hdnProduct", Me.hdnProduct},
                                                                        {"hdnBrId", Me.hdnBrId},
                                                                        {"hdnStatus", Me.hdnStatus},
                                                                        {"hdnIsViewOnlyPopup", Me.hdnIsViewOnlyPopup},
                                                                        {"hdnMsgId", Me.hdnMsgId},
                                                                        {"hdnCostSelectedTabId", Me.hdnCostSelectedTabId},
                                                                        {"hdnNewBreaker", Me.hdnNewBreaker},
                                                                        {"hdnIsViewFromApprove", Me.hdnIsViewFromApprove},
                                                                        {"hdnCallerMapId", Me.hdnCallerMapId},
                                                                        {"hdnXMLsaveFileRet", Me.hdnXMLsaveFileRet},
                                                                        {"hdnExtract", Me.hdnExtract},
                                                                        {"hdnStep", Me.hdnStep}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            'ステータスを直近の登録状態に変更
            Dim dicEntCostStats As New Dictionary(Of String, HiddenField) From {{"POL1", hdnPol1Status}, {"POD1", hdnPod1Status}, {"POL2", hdnPol2Status}, {"POD2", hdnPod2Status}}
            For Each entCost In dicEntCostStats
                If dicBrInfo.ContainsKey(entCost.Key) Then
                    entCost.Value.Value = dicBrInfo(entCost.Key).AppStatus
                End If
            Next

        ElseIf TypeOf Page.PreviousPage Is GBT00002RESULT Then
            '一覧からの遷移
            Dim brNo As String = ""
            Dim prevPage As GBT00002RESULT = DirectCast(Page.PreviousPage, GBT00002RESULT)
            Me.GBT00002RValues = prevPage.ThisScreenValues
            ViewState(CONST_VS_NAME_GBT00002RV) = prevPage.ThisScreenValues
            Dim hdnBrIdObj As HiddenField = Nothing
            hdnBrIdObj = DirectCast(prevPage.FindControl("hdnSelectedBrId"), HiddenField)

            brNo = hdnBrIdObj.Value
            Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo)
            Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
            If brInfoOrganizer.BrType = C_BRTYPE.SALES Then
                Me.hdnBrType.Value = "1"
            Else
                Me.hdnBrType.Value = "2"
            End If

            'メイン情報取得
            Dim dt As DataTable = GetBreakerBase(dicBrInfo)
            ViewState("DICBRINFO") = dicBrInfo
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
                                                                        {"hdnSearchBreakerType", Me.hdnSearchBreakerType},
                                                                        {"hdnPol1Status", Me.hdnPol1Status},
                                                                        {"hdnPol2Status", Me.hdnPol2Status},
                                                                        {"hdnPod1Status", Me.hdnPod1Status},
                                                                        {"hdnPod2Status", Me.hdnPod2Status},
                                                                        {"hdnXMLsaveFileRet", Me.hdnXMLsaveFileRet}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next
            '直近ステータスの取得
            Dim dtStat = GetStatus(brNo)
            If dtStat IsNot Nothing AndAlso dtStat.Rows.Count > 0 Then
                Dim infoRow = From dr As DataRow In dtStat Where dr("TYPE").Equals("INFO") Select Convert.ToString(dr.Item("STATUS"))
                If infoRow.Any Then
                    Me.hdnStatus.Value = infoRow(0)
                End If
            End If


        ElseIf TypeOf Page.PreviousPage Is GBT00005APPROVAL Then
            '承認画面からの遷移
            Me.hdnIsViewFromApprove.Value = "1" '承認画面遷移か(1:承認画面,0:その他)ここでしか1を立てない
            Dim brNo As String = ""
            Dim prevPage As GBT00005APPROVAL = DirectCast(Page.PreviousPage, GBT00005APPROVAL)
            Dim hdnBrIdObj As HiddenField = Nothing
            hdnBrIdObj = DirectCast(prevPage.FindControl("hdnSelectedBrId"), HiddenField)
            brNo = hdnBrIdObj.Value

            Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brNo)
            ViewState("DICBRINFO") = dicBrInfo
            Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
            If brInfoOrganizer.BrType = C_BRTYPE.SALES Then
                Me.hdnBrType.Value = "1"
            Else
                Me.hdnBrType.Value = "2"
            End If

            'メイン情報取得
            Dim dt As DataTable = GetBreakerBase(dicBrInfo)
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
                                                                        {"hdnPOLPort", Me.hdnPOLPort},
                                                                        {"hdnPODPort", Me.hdnPODPort},
                                                                        {"hdnProduct", Me.hdnProduct},
                                                                        {"hdnBrId", Me.hdnBrId},
                                                                        {"hdnSearchBreakerType", Me.hdnSearchBreakerType},
                                                                        {"hdnStatus", Me.hdnStatus},
                                                                        {"hdnXMLsaveFileRet", Me.hdnXMLsaveFileRet},
                                                                        {"hdnExtract", Me.hdnExtract},
                                                                        {"hdnDenial", Me.hdnDenial},
                                                                        {"hdnSelectedStep", Me.hdnStep},
                                                                        {"hdnPrevViewID", Me.hdnPrevViewID}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

        ElseIf Page.PreviousPage Is Nothing Then
            PreProcType = "遷移元なし"
            '単票直接呼出しパターン(JavaScriptよりPOSTした内容を取得し判定)
            'If Convert.ToString(Request.Form("hdnSender")) = "GBT00003R" Then
            If Convert.ToString(Request.Form("hdnSender")) = "GBT00003R" OrElse
                Convert.ToString(Request.Form("hdnSender")) = "GBT00026T" Then
                Me.hdnIsViewOnlyPopup.Value = "1" '編集不可能のポップアップであるフラグをON
                'オーダー一覧からの遷移
                Dim brId As String = Convert.ToString(Request.Form("hdnBrIdFromOrderList"))
                Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brId)
                ViewState("DICBRINFO") = dicBrInfo
                Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
                If brInfoOrganizer.BrType = C_BRTYPE.SALES Then
                    Me.hdnBrType.Value = "1"
                Else
                    Me.hdnBrType.Value = "2"
                End If

                'メイン情報取得
                Dim dt As DataTable = GetBreakerBase(dicBrInfo)
                'メイン情報格納
                retDataSet.Tables.Add(dt)
                '費用情報取得
                Dim costDt As DataTable = GetBreakerValue(dicBrInfo)
                retDataSet.Tables.Add(costDt)

            End If
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
        With retDt.Columns
            .Add("BRID", GetType(String))
            .Add("BRBASEID", GetType(String))
            .Add("STYMD", GetType(String))
            .Add("USETYPE", GetType(String))
            .Add("VALIDITYFROM", GetType(String))
            .Add("VALIDITYTO", GetType(String))
            .Add("DISABLED", GetType(String))
            .Add("TERMTYPE", GetType(String))
            '.Add("NOOFTANKS", GetType(String)).DefaultValue = "1"
            .Add("NOOFTANKS", GetType(String))
            .Add("SHIPPER", GetType(String))
            .Add("CONSIGNEE", GetType(String))
            .Add("CARRIER1", GetType(String))
            .Add("CARRIER2", GetType(String))
            .Add("PRODUCTCODE", GetType(String))
            .Add("IMDGCODE", GetType(String))
            .Add("UNNO", GetType(String))
            .Add("RECIEPTCOUNTRY1", GetType(String))
            .Add("RECIEPTPORT1", GetType(String))
            .Add("LOADCOUNTRY1", GetType(String))
            .Add("LOADPORT1", GetType(String))
            .Add("DISCHARGECOUNTRY1", GetType(String))
            .Add("DISCHARGEPORT1", GetType(String))
            .Add("DELIVERYCOUNTRY1", GetType(String))
            .Add("DELIVERYPORT1", GetType(String))
            .Add("RECIEPTCOUNTRY2", GetType(String))
            .Add("RECIEPTPORT2", GetType(String))
            .Add("LOADCOUNTRY2", GetType(String))
            .Add("LOADPORT2", GetType(String))
            .Add("DISCHARGECOUNTRY2", GetType(String))
            .Add("DISCHARGEPORT2", GetType(String))
            .Add("DELIVERYCOUNTRY2", GetType(String))
            .Add("DELIVERYPORT2", GetType(String))

            .Add("VSL1", GetType(String))
            .Add("VOY1", GetType(String))
            .Add("ETD1", GetType(String))
            .Add("ETA1", GetType(String))

            .Add("VSL2", GetType(String))
            .Add("VOY2", GetType(String))
            .Add("ETD2", GetType(String))
            .Add("ETA2", GetType(String))
            .Add("INVOICEDBY", GetType(String))
            .Add("PRODUCTWEIGHT", GetType(String)).DefaultValue = "0"
            .Add("CAPACITY", GetType(String))
            .Add("GRAVITY", GetType(String))
            .Add("LOADING", GetType(String)).DefaultValue = "0"
            .Add("STEAMING", GetType(String)).DefaultValue = "0"
            .Add("TIP", GetType(String)).DefaultValue = "0"
            .Add("EXTRA", GetType(String)).DefaultValue = "0"
            .Add("JOTHIREAGE", GetType(String))
            .Add("COMMERCIALFACTOR", GetType(String))
            .Add("AMTREQUEST", GetType(String)).DefaultValue = "0"
            .Add("AMTPRINCIPAL", GetType(String)).DefaultValue = "0"
            .Add("AMTDISCOUNT", GetType(String)).DefaultValue = "0"
            .Add("DEMURTO", GetType(String)).DefaultValue = "0"
            .Add("DEMURUSRATE1", GetType(String)).DefaultValue = "0"
            .Add("DEMURUSRATE2", GetType(String)).DefaultValue = "0"

            .Add("COUNTRYORGANIZER", GetType(String))
            .Add("FEE", GetType(String)).DefaultValue = "0"
            .Add("BILLINGCATEGORY", GetType(String))

            .Add("REMARK", GetType(String))
            .Add("APPLYTEXT", GetType(String))
            .Add("APPROVEDTEXT", GetType(String))
            .Add("BRTYPE", GetType(String)) 'ブレーカータイプ
            .Add("ISTRILATERAL", GetType(String)) '3国間輸送か "1.三国,その他.通常
            .Add("USINGLEASETANK", GetType(String)).DefaultValue = "0"
            .Add("DAYSTOTAL", GetType(String))
            .Add("PERDAY", GetType(String))
            .Add("TOTALINVOICED", GetType(String))
            .Add("LASTORDERNO", GetType(String))
            .Add("TANKNO", GetType(String))
            .Add("DEPOTCODE", GetType(String))
            .Add("TWOAGOPRODUCT", GetType(String))

            '承認情報
            .Add("APPLYDATE", GetType(String))
            .Add("APPLICANTID", GetType(String))
            .Add("APPLICANTNAME", GetType(String))
            .Add("APPROVEDATE", GetType(String))
            .Add("APPROVERID", GetType(String))
            .Add("APPROVERNAME", GetType(String))

            .Add("DUMMY", GetType(String))
            .Add("DUMMY2", GetType(String))
            'エージェント関係
            .Add("AGENTORGANIZER", GetType(String))
            .Add("AGENTPOL1", GetType(String))
            .Add("AGENTPOL2", GetType(String))
            .Add("AGENTPOD1", GetType(String))
            .Add("AGENTPOD2", GetType(String))

            '出力用名称
            .Add("BRTYPENAME", GetType(String))
            .Add("BRTERMNAME", GetType(String))
            .Add("APPSALESPICNAME", GetType(String))
            .Add("INVOICEDNAME", GetType(String))
            .Add("SHIPPERNAME", GetType(String))
            .Add("CONSIGNEENAME", GetType(String))
            .Add("CARRIER1NAME", GetType(String))
            .Add("CARRIER2NAME", GetType(String))
            .Add("PRODUCTNAME", GetType(String))
            .Add("RECIEPTPORT1NAME", GetType(String))
            .Add("LOADPORT1NAME", GetType(String))
            .Add("DISCHARGEPORT1NAME", GetType(String))
            .Add("DELIVERYPORT1NAME", GetType(String))
            .Add("RECIEPTPORT2NAME", GetType(String))
            .Add("LOADPORT2NAME", GetType(String))
            .Add("DISCHARGEPORT2NAME", GetType(String))
            .Add("DELIVERYPORT2NAME", GetType(String))
            .Add("SPECIALINSTRUCTIONS", GetType(String))
            .Add("AGENTNAME", GetType(String))

            .Add("INITYMD", GetType(String))
            .Add("INITUSER", GetType(String))
            .Add("INITUSERNAME", GetType(String))
            '↓コピー元BRID用
            .Add("ORIGINALCOPYBRID", GetType(String))
            '↓JPY(帳票用)
            .Add("TOTALCOST_JPY", GetType(String)).DefaultValue = "0"
            .Add("PERDAY_JPY", GetType(String)).DefaultValue = "0"
            .Add("TOTALINVOICED_JPY", GetType(String)).DefaultValue = "0"
            .Add("JOTHIREAGE_JPY", GetType(String)).DefaultValue = "0"
            .Add("COMMERCIALFACTOR_JPY", GetType(String)).DefaultValue = "0"
            .Add("AMTREQUEST_JPY", GetType(String)).DefaultValue = "0"
            .Add("AMTPRINCIPAL_JPY", GetType(String)).DefaultValue = "0"
            .Add("AMTDISCOUNT_JPY", GetType(String)).DefaultValue = "0"
            .Add("JPY_RATE", GetType(String)).DefaultValue = "0"
            '↑JPY(帳票用)
        End With

        Dim dr As DataRow = retDt.NewRow
        dr.Item("DUMMY") = "END"
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
        With retDt.Columns
            .Add("BRID", GetType(String))
            .Add("BRVALUEID", GetType(String))
            .Add("STYMD", GetType(String))
            .Add("DTLPOLPOD", GetType(String))
            .Add("DTLOFFICE", GetType(String))
            .Add("COSTCODE", GetType(String))
            .Add("COSTNAME", GetType(String))
            .Add("BASEON", GetType(String)).DefaultValue = "1"
            .Add("TAX", GetType(String)).DefaultValue = "0"
            .Add("USD", GetType(String))
            .Add("LOCAL", GetType(String)).DefaultValue = "0"
            .Add("CONTRACTOR", GetType(String))
            .Add("CONTRACTORNAME", GetType(String))
            .Add("CURRENCYCODE", GetType(String))
            .Add("LOCALRATE", GetType(String))
            .Add("USDRATE", GetType(String))
            .Add("REMARK", GetType(String))
            .Add("CHARGECLASS4", GetType(String))
            .Add("CHARGECLASS8", GetType(String))
            .Add("CAN_DELETE", GetType(String))
            .Add("SORT_ORDER", GetType(String))
            .Add("AGENT", GetType(String))

            .Add("ACTIONID", GetType(String))
            .Add("CLASS1", GetType(String))
            .Add("CLASS2", GetType(String))
            .Add("CLASS3", GetType(String))
            .Add("CLASS4", GetType(String))
            .Add("CLASS5", GetType(String))
            .Add("CLASS6", GetType(String))
            .Add("CLASS7", GetType(String))
            .Add("CLASS8", GetType(String))
            .Add("CLASS9", GetType(String))
            .Add("TAXATION", GetType(String)).DefaultValue = "0"
            .Add("CINVOICEDBY", GetType(String))
            .Add("COUNTRYCODE", GetType(String))
            .Add("REPAIRFLG", GetType(String)).DefaultValue = "0"
            .Add("APPROVEDUSD", GetType(String))
            .Add("LOCALCURRENCY", GetType(String))
            .Add("BILLING", GetType(String))
        End With
        Return retDt
    End Function
    ''' <summary>
    ''' 初回保持項目
    ''' </summary>
    Private Sub SetInitData(dt As DataTable)
        Dim dr As DataRow = dt.Rows(0)

        Me.hdnInitYmd.Value = Convert.ToString(dr.Item("INITYMD"))
        Me.hdnInitUser.Value = Convert.ToString(dr.Item("INITUSER"))
        Me.hdnInitUserName.Value = Convert.ToString(dr.Item("INITUSERNAME"))

    End Sub
    ''' <summary>
    ''' オーナー情報をデータテーブルより画面に貼り付け
    ''' </summary>
    ''' <param name="dt"></param>
    Private Sub SetDisplayOrganizerInfo(dt As DataTable, Optional isExcelInport As Boolean = False)
        Dim dr As DataRow = dt.Rows(0)
        Me.hdnIsTrilateral.Value = Convert.ToString(dr.Item("ISTRILATERAL"))
        Me.lblBrNo.Text = Convert.ToString(dr.Item("BRID"))
        Dim jprateBrId As String = Me.lblBrNo.Text
        If lblBrNo.Attributes.Keys.Cast(Of String).Contains("CopyNew") Then
            jprateBrId = ""
        End If
        ViewState("JPYEXR") = GetJpyExrate(jprateBrId)
        Me.txtBrType.Text = Convert.ToString(dr.Item("USETYPE"))
        If Me.txtBrType.Text <> "" Then
            Dim useTypeDt As DataTable = GetTransferPattern(Me.hdnBrType.Value, Me.txtBrType.Text)
            If useTypeDt IsNot Nothing AndAlso useTypeDt.Rows.Count > 0 Then
                Me.lblBrTypeText.Text = HttpUtility.HtmlEncode(Convert.ToString(useTypeDt.Rows(0).Item("NAMES")))
            End If
        End If

        If Convert.ToString(dr.Item("VALIDITYFROM")) <> "" Then
            Me.txtBrStYmd.Text = Date.Parse(Convert.ToString(dr.Item("VALIDITYFROM"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtBrStYmd.Text = Convert.ToString(dr.Item("VALIDITYFROM"))
        End If
        Me.hdnOriginalCopyBrid.Value = Convert.ToString(dr.Item("ORIGINALCOPYBRID"))
        If Convert.ToString(dr.Item("VALIDITYTO")) <> "" Then
            Me.txtBrEndYmd.Text = Date.Parse(Convert.ToString(dr.Item("VALIDITYTO"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtBrEndYmd.Text = Convert.ToString(dr.Item("VALIDITYTO"))
        End If

        If Convert.ToString(dr.Item("DISABLED")) = CONST_FLAG_YES Then
            Me.chkDisabled.Checked = True
        Else
            Me.chkDisabled.Checked = False
        End If

        Me.txtBrTerm.Text = Convert.ToString(dr.Item("TERMTYPE"))
        If Me.txtBrTerm.Text <> "" Then

            Me.lblBrTermText.Text = ""
            Dim brTypeItem As ListItem = Me.lbTerm.Items.FindByValue(Me.txtBrTerm.Text)
            If brTypeItem IsNot Nothing Then

                Me.lblBrTermText.Text = brTypeItem.Text
            End If
        End If
        Me.txtNoOfTanks.Text = Convert.ToString(dr.Item("NOOFTANKS"))
        'オーガナイザー情報保持用
        Me.hdnAgentOrganizer.Value = Convert.ToString(dr.Item("AGENTORGANIZER"))
        Me.hdnCountryOrganizer.Value = Convert.ToString(dr.Item("COUNTRYORGANIZER"))
        '承認関係項目
        If Convert.ToString(dr.Item("APPLYDATE")) <> "" Then
            Me.txtAppRequestYmd.Text = Date.Parse(Convert.ToString(dr.Item("APPLYDATE"))).ToString(GBA00003UserSetting.DATEFORMAT) 'Apply Date
            Me.txtAppOffice.Text = Convert.ToString(dr.Item("AGENTORGANIZER")) 'Apply Office
        Else
            Me.txtAppRequestYmd.Text = Convert.ToString(dr.Item("APPLYDATE")) 'Apply Date
            Me.txtAppOffice.Text = ""
        End If
        Me.txtAppSalesPic.Text = Convert.ToString(dr.Item("APPLICANTID")) 'Apply PIC
        Me.lblAppSalesPicText.Text = Convert.ToString(dr.Item("APPLICANTNAME")) 'Apply PIC NAME(ラベルなのでHTMLエンコード)
        Me.lblApplyRemarks.Text = Convert.ToString(dr.Item("APPLYTEXT")) 'Apply Remarks(ラベルなのでHTMLエンコード)

        If Convert.ToString(dr.Item("APPROVEDATE")) <> "" Then
            Me.txtApprovedYmd.Text = Date.Parse(Convert.ToString(dr.Item("APPROVEDATE"))).ToString(GBA00003UserSetting.DATEFORMAT) 'Approved Date
        Else
            Me.txtApprovedYmd.Text = Convert.ToString(dr.Item("APPROVEDATE")) 'Approved Date
        End If
        Me.txtAppJotPic.Text = Convert.ToString(dr.Item("APPROVERID"))   'Approved PIC
        Me.lblAppJotPicText.Text = Convert.ToString(dr.Item("APPROVERNAME")) 'Approved PIC NAME(ラベルなのでHTMLエンコード)
        Me.lblAppJotRemarks.Text = Convert.ToString(dr.Item("APPROVEDTEXT")) 'Approved Remarks(ラベルなのでHTMLエンコード)
        '国関係の情報展開
        Me.txtRecieptCountry1.Text = Convert.ToString(dr.Item("RECIEPTCOUNTRY1"))
        Me.txtRecieptPort1.Text = Convert.ToString(dr.Item("RECIEPTPORT1"))
        Me.lblRecieptPort1Text.Text = ""
        If Me.txtRecieptCountry1.Text <> "" AndAlso Me.txtRecieptPort1.Text <> "" Then
            Dim portDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(Me.txtRecieptCountry1.Text, Me.txtRecieptPort1.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblRecieptPort1Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtLoadCountry1.Text = Convert.ToString(dr.Item("LOADCOUNTRY1"))
        Me.txtLoadPort1.Text = Convert.ToString(dr.Item("LOADPORT1"))
        Me.lblLoadPort1Text.Text = ""
        If Me.txtLoadCountry1.Text <> "" AndAlso Me.txtLoadPort1.Text <> "" Then
            Dim portDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(Me.txtLoadCountry1.Text, Me.txtLoadPort1.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblLoadPort1Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtDischargeCountry1.Text = Convert.ToString(dr.Item("DISCHARGECOUNTRY1"))
        Me.txtDischargePort1.Text = Convert.ToString(dr.Item("DISCHARGEPORT1"))
        Me.lblDischargePort1Text.Text = ""
        If Me.txtDischargeCountry1.Text <> "" AndAlso Me.txtDischargePort1.Text <> "" Then
            Dim portDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(Me.txtDischargeCountry1.Text, Me.txtDischargePort1.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblDischargePort1Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtDeliveryCountry1.Text = Convert.ToString(dr.Item("DELIVERYCOUNTRY1"))
        Me.txtDeliveryPort1.Text = Convert.ToString(dr.Item("DELIVERYPORT1"))
        Me.lblDeliveryPort1Text.Text = ""
        If Me.txtDeliveryCountry1.Text <> "" AndAlso Me.txtDeliveryPort1.Text <> "" Then
            Dim portDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(Me.txtDeliveryCountry1.Text, Me.txtDeliveryPort1.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblDeliveryPort1Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtRecieptCountry2.Text = Convert.ToString(dr.Item("RECIEPTCOUNTRY2"))
        Me.txtRecieptPort2.Text = Convert.ToString(dr.Item("RECIEPTPORT2"))
        Me.lblRecieptPort2Text.Text = ""
        If Me.txtRecieptCountry2.Text <> "" AndAlso Me.txtRecieptPort2.Text <> "" Then
            Dim portDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(Me.txtRecieptCountry2.Text, Me.txtRecieptPort2.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblRecieptPort2Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtLoadCountry2.Text = Convert.ToString(dr.Item("LOADCOUNTRY2"))
        Me.txtLoadPort2.Text = Convert.ToString(dr.Item("LOADPORT2"))
        Me.lblLoadPort2Text.Text = ""
        If Me.txtLoadCountry2.Text <> "" AndAlso Me.txtLoadPort2.Text <> "" Then
            Dim portDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(Me.txtLoadCountry2.Text, Me.txtLoadPort2.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblLoadPort2Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If
        Me.txtDischargeCountry2.Text = Convert.ToString(dr.Item("DISCHARGECOUNTRY2"))
        Me.txtDischargePort2.Text = Convert.ToString(dr.Item("DISCHARGEPORT2"))
        Me.lblDischargePort2Text.Text = ""
        If Me.txtDischargeCountry2.Text <> "" AndAlso Me.txtDischargePort2.Text <> "" Then
            Dim portDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(Me.txtDischargeCountry2.Text, Me.txtDischargePort2.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblDischargePort2Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtDeliveryCountry2.Text = Convert.ToString(dr.Item("DELIVERYCOUNTRY2"))
        Me.txtDeliveryPort2.Text = Convert.ToString(dr.Item("DELIVERYPORT2"))
        Me.lblDeliveryPort2Text.Text = ""
        If Me.txtDeliveryCountry2.Text <> "" AndAlso Me.txtDeliveryPort2.Text <> "" Then
            Dim portDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(Me.txtDeliveryCountry2.Text, Me.txtDeliveryPort2.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblDeliveryPort2Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtShipper.Text = Convert.ToString(dr.Item("SHIPPER"))
        Me.lblShipperText.Text = ""
        If Me.txtShipper.Text <> "" AndAlso Me.txtLoadCountry1.Text <> "" Then

            Dim shipperDt As DataTable = New DataTable
            If Me.hdnBrType.Value = "1" Then
                shipperDt = GetShipper(Me.txtLoadCountry1.Text, Me.txtShipper.Text)
            Else
                shipperDt = GetAgent(Me.txtLoadCountry1.Text, Me.txtShipper.Text)
            End If

            If shipperDt IsNot Nothing AndAlso shipperDt.Rows.Count > 0 Then
                Me.lblShipperText.Text = HttpUtility.HtmlEncode(Convert.ToString(shipperDt.Rows(0).Item("NAME")))
            End If
        End If

        Me.txtConsignee.Text = Convert.ToString(dr.Item("CONSIGNEE"))
        Me.lblConsigneeText.Text = ""
        If Me.txtConsignee.Text <> "" Then
            SetDisplayConsignee(Me.txtConsignee, Me.txtConsignee.Text)
        End If
        Me.txtCarrier1.Text = Convert.ToString(dr.Item("CARRIER1"))
        Me.lblCarrier1Text.Text = ""
        If Me.txtCarrier1.Text <> "" Then
            SetDisplayCarrier(Me.txtCarrier1, Me.txtCarrier1.Text)
        End If
        Me.txtCarrier2.Text = Convert.ToString(dr.Item("CARRIER2"))
        Me.lblCarrier2Text.Text = ""
        If Me.txtCarrier2.Text <> "" Then
            SetDisplayCarrier(Me.txtCarrier2, Me.txtCarrier2.Text)
        End If
        Me.txtProduct.Text = Convert.ToString(dr.Item("PRODUCTCODE"))
        Me.lblProductText.Text = ""
        If Me.txtProduct.Text <> "" Then
            SetDisplayProduct(Me.txtProduct, Me.txtProduct.Text)
        End If
        'Me.txtImdg.Text = Convert.ToString(dr.Item("IMDGCODE"))

        'Me.txtUNNo.Text = Convert.ToString(dr.Item("UNNO"))

        Me.txtVsl1.Text = Convert.ToString(dr.Item("VSL1"))
        Me.txtVoy1.Text = Convert.ToString(dr.Item("VOY1"))

        If Convert.ToString(dr.Item("ETD1")) <> "" Then
            Me.txtEtd1.Text = Date.Parse(Convert.ToString(dr.Item("ETD1"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtEtd1.Text = Convert.ToString(dr.Item("ETD1"))
        End If

        If Convert.ToString(dr.Item("ETA1")) <> "" Then
            Me.txtEta1.Text = Date.Parse(Convert.ToString(dr.Item("ETA1"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtEta1.Text = Convert.ToString(dr.Item("ETA1"))
        End If

        Me.txtVsl2.Text = Convert.ToString(dr.Item("VSL2"))
        Me.txtVoy2.Text = Convert.ToString(dr.Item("VOY2"))

        If Convert.ToString(dr.Item("ETD2")) <> "" Then
            Me.txtEtd2.Text = Date.Parse(Convert.ToString(dr.Item("ETD2"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtEtd2.Text = Convert.ToString(dr.Item("ETD2"))
        End If

        If Convert.ToString(dr.Item("ETA2")) <> "" Then
            Me.txtEta2.Text = Date.Parse(Convert.ToString(dr.Item("ETA2"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtEta2.Text = Convert.ToString(dr.Item("ETA2"))
        End If

        Me.txtWeight.Text = Convert.ToString(dr.Item("PRODUCTWEIGHT"))
        'Me.txtSGravity.Text = Convert.ToString(dr.Item("GRAVITY"))
        'Me.txtTankCapacity.Text = "25,000"
        Me.txtTankCapacity.Text = Convert.ToString(dr.Item("CAPACITY"))
        Me.txtLoading.Text = Convert.ToString(dr.Item("LOADING"))
        Me.txtSteaming.Text = Convert.ToString(dr.Item("STEAMING"))
        Me.txtTip.Text = Convert.ToString(dr.Item("TIP"))
        Me.txtExtra.Text = Convert.ToString(dr.Item("EXTRA"))
        Me.txtJOTHireage.Text = Convert.ToString(dr.Item("JOTHIREAGE"))
        Me.txtCommercialFactor.Text = Convert.ToString(dr.Item("COMMERCIALFACTOR"))
        Me.txtInvoicedTotal.Text = Convert.ToString(dr.Item("TOTALINVOICED"))
        Me.txtAmtRequest.Text = Convert.ToString(dr.Item("AMTREQUEST"))
        Me.txtAmtPrincipal.Text = Convert.ToString(dr.Item("AMTPRINCIPAL"))
        Me.txtAmtDiscount.Text = Convert.ToString(dr.Item("AMTDISCOUNT"))
        Me.txtDemurdayT1.Text = Convert.ToString(dr.Item("DEMURTO"))
        Me.txtDemurUSRate1.Text = Convert.ToString(dr.Item("DEMURUSRATE1"))
        Me.txtDemurUSRate2.Text = Convert.ToString(dr.Item("DEMURUSRATE2"))
        Me.lblBrRemarkText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("REMARK")))

        'Me.txtLocalRate.Text = Convert.ToString(dr.Item("LOCALRATE"))
        'Me.txtUSDRate.Text = Convert.ToString(dr.Item("USDRATE"))
        'AGENT情報
        'POL1
        Me.txtAgentPol1.Text = Convert.ToString(dr.Item("AGENTPOL1"))
        Me.lblAgentPol1Text.Text = ""
        If Me.txtLoadCountry1.Text <> "" AndAlso Me.txtAgentPol1.Text <> "" Then
            Dim portDt As DataTable = Me.GetAgent(Me.txtLoadCountry1.Text, Me.txtAgentPol1.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblAgentPol1Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If
        'POL2
        Me.txtAgentPol2.Text = Convert.ToString(dr.Item("AGENTPOL2"))
        Me.lblAgentPol2Text.Text = ""
        If Me.txtLoadCountry2.Text <> "" AndAlso Me.txtAgentPol2.Text <> "" Then
            Dim portDt As DataTable = Me.GetAgent(Me.txtLoadCountry2.Text, Me.txtAgentPol2.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblAgentPol2Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If
        'POD1
        Me.txtAgentPod1.Text = Convert.ToString(dr.Item("AGENTPOD1"))
        Me.lblAgentPod1Text.Text = ""
        If Me.txtDischargeCountry1.Text <> "" AndAlso Me.txtAgentPod1.Text <> "" Then
            Dim portDt As DataTable = Me.GetAgent(Me.txtDischargeCountry1.Text, Me.txtAgentPod1.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblAgentPod1Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If
        'POD2
        Me.txtAgentPod2.Text = Convert.ToString(dr.Item("AGENTPOD2"))
        Me.lblAgentPod2Text.Text = ""
        If Me.txtDischargeCountry2.Text <> "" AndAlso Me.txtAgentPod2.Text <> "" Then
            Dim portDt As DataTable = Me.GetAgent(Me.txtDischargeCountry2.Text, Me.txtAgentPod2.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblAgentPod2Text.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtInvoiced.Text = Convert.ToString(dr.Item("INVOICEDBY"))
        Me.lblInvoicedText.Text = ""
        If Me.txtInvoiced.Text <> "" Then
            'Dim portDt As DataTable = Me.GetAgent(GBA00003UserSetting.COUNTRYCODE, Me.txtInvoiced.Text)
            Dim portDt As DataTable = Me.GetAgent("", Me.txtInvoiced.Text)
            If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
                Me.lblInvoicedText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            End If
        End If

        Me.txtBillingCategory.Text = Convert.ToString(dr.Item("BILLINGCATEGORY"))
        Me.lblBillingCategoryText.Text = ""
        txtBillingCategory_Change()

        If Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.CONSIGNEE Then
            Me.lblConsignee.CssClass = "requiredMark2"
        Else
            Me.lblConsignee.CssClass = ""
        End If

        Me.txtFee.Text = Convert.ToString(dr.Item("FEE"))

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        Dim combRem As String = Nothing
        If ViewState("DICBRINFO") IsNot Nothing Then
            brInfo = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))

            For Each keyString As String In {"INFO", "POL1", "POD1", "POL2", "POD2"}
                If brInfo.ContainsKey(keyString) Then
                    Dim brInfoItem = brInfo(keyString)

                    combRem = combRem & brInfoItem.Remark
                    combRem = combRem & vbCrLf
                End If

            Next

            Me.lblRemarks.Text = combRem
            Me.lblRemarks2.Text = combRem
            If combRem.Replace(vbCrLf, "").Replace(" ", "") <> "" Then
                Me.lblRemarks.CssClass = "hasRemark"
                Me.lblRemarks2.CssClass = "hasRemark"
            Else
                Me.lblRemarks.CssClass = ""
                Me.lblRemarks2.CssClass = ""
            End If
        End If

        'ステータス取得
        If Me.lblBrNo.Text <> "" Then
            Dim stDt As DataTable = GetStatus(Me.lblBrNo.Text)
            If stDt Is Nothing OrElse stDt.Rows.Count = 0 Then
                Return
            End If

            Dim appFlg As Boolean = True
            Dim inpFlg As Boolean = True
            For Each stDr As DataRow In stDt.Rows

                If Convert.ToString(stDr.Item("TYPE")) = "INFO" Then
                    If Not (Convert.ToString(stDr.Item("STATUS")) = "" OrElse
                            Convert.ToString(stDr.Item("STATUS")) = C_APP_STATUS.REJECT) Then
                        appFlg = False
                        inpFlg = False
                    End If

                    If {C_APP_STATUS.APPLYING,
                        C_APP_STATUS.APPROVED,
                        C_APP_STATUS.COMPLETE}.Contains(Convert.ToString(stDr.Item("STATUS"))) Then
                        Me.hdnDisableAll.Value = "1"
                    End If
                    'Me.hdnStatus.Value = Convert.ToString(stDr.Item("STATUS")) 'オーガナイザステータス取得
                Else
                    If Convert.ToString(stDr.Item("STATUS")) <> C_APP_STATUS.COMPLETE Then
                        appFlg = False
                    End If
                End If
            Next

            If appFlg Then
                Me.hdnApply.Value = "1"
            Else
                Me.hdnApply.Value = ""
            End If

            If Not inpFlg Then
                Me.hdnInputReq.Value = "1"
            End If
        Else
            Me.hdnApply.Value = ""
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
        'If BASEDLL.COA0019Session.LANGDISP = C_LANG.JA Then
        '    displayNameField = "NAMESJA"
        'End If
        sqlStat.AppendLine("SELECT TR.AGENTKBN")
        sqlStat.AppendLine("      , CH.COSTCODE As CODE")
        sqlStat.AppendFormat("     , CH.{0} AS NAMES", displayNameField).AppendLine()
        sqlStat.AppendLine("      , TR.ACTIONID As ACTIONID")
        sqlStat.AppendLine("      , TR.INITCONTRACTOR As INITCONTRACTOR")
        sqlStat.AppendLine("      , TR.CLASS1 As CLASS1")
        sqlStat.AppendLine("      , TR.CLASS2 As CLASS2")
        sqlStat.AppendLine("      , TR.CLASS3 As CLASS3")
        sqlStat.AppendLine("      , TR.CLASS4 As CLASS4")
        sqlStat.AppendLine("      , TR.CLASS5 As CLASS5")
        sqlStat.AppendLine("      , TR.CLASS6 As CLASS6")
        sqlStat.AppendLine("      , TR.CLASS7 As CLASS7")
        sqlStat.AppendLine("      , '0'       As CLASS8")
        sqlStat.AppendLine("      , CH.CLASS9 As CLASS9")
        sqlStat.AppendLine("      , CH.CLASS4 As CHARGECLASS4")
        sqlStat.AppendLine("      , CH.CLASS8 As CHARGECLASS8")
        sqlStat.AppendLine("  FROM GBM0009_TRPATTERN TR")
        sqlStat.AppendLine(" INNER JOIN GBM0010_CHARGECODE CH")
        sqlStat.AppendLine("    ON TR.COMPCODE = CH.COMPCODE")
        sqlStat.AppendLine("   AND TR.COSTCODE = CH.COSTCODE")
        sqlStat.AppendLine("   AND (CH.LDKBN = 'B' OR ")
        sqlStat.AppendLine("        CH.LDKBN = CASE WHEN TR.AGENTKBN LIKE 'POL%' THEN 'L' ")
        sqlStat.AppendLine("                        ELSE 'D' END)")
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
        sqlStat.AppendLine("   AND NOT EXISTS (SELECT 1 ")
        sqlStat.AppendLine("                     FROM COS0017_FIXVALUE FXVS")
        sqlStat.AppendLine("                    WHERE FXVS.COMPCODE = '" & GBC_COMPCODE_D & "'")
        sqlStat.AppendLine("                      AND FXVS.CLASS   = '" & C_FIXVALUECLAS.BREX & "'")
        sqlStat.AppendLine("                      AND FXVS.STYMD  <= @STYMD")
        sqlStat.AppendLine("                      AND FXVS.ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("                      AND FXVS.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                      AND FXVS.KEYCODE = TR.COSTCODE")
        sqlStat.AppendLine("                   )")
        sqlStat.AppendLine(" ORDER BY TR.AGENTKBN,TR.COSTCODE")
        '結果セット定義
        Dim sqlResultDt As New DataTable
        'ブレーカータイプ取得
        Dim dr As DataRow = dt.Rows(0)
        Dim breakerType As String = Convert.ToString(dr.Item("BRTYPE"))
        Dim breakerTypeDev As String = C_BRTYPE.SALES
        If breakerType <> "1" Then
            breakerTypeDev = C_BRTYPE.OPERATION
        End If
        Dim useType As String = Convert.ToString(dr.Item("USETYPE"))
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@ORG", SqlDbType.NVarChar, 20).Value = "GB_Default" '輸送パターンマスタ同フィールド名条件
                .Add("@BREAKERTYPE", SqlDbType.NVarChar, 20).Value = breakerTypeDev
                .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = useType
                .Add("@STYMD", SqlDbType.Date).Value = Date.Today
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Today
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(sqlResultDt)
            End Using
        End Using
        '取得結果を囲う
        retDt = CreateCostInfoTable()
        If sqlResultDt IsNot Nothing AndAlso sqlResultDt.Rows.Count > 0 Then
            retDt = CreateCostInfoTable()
            Dim sortOrder As New Dictionary(Of String, Integer) From {{"POL1", 0}, {"POL2", 0},
                                                                      {"POD1", 0}, {"POD2", 0}}
            Dim countryCodes As New Dictionary(Of String, String) From {{"POL1", Convert.ToString(dr.Item("RECIEPTCOUNTRY1"))},
                                                                        {"POL2", Convert.ToString(dr.Item("RECIEPTCOUNTRY2"))},
                                                                        {"POD1", Convert.ToString(dr.Item("DISCHARGECOUNTRY1"))},
                                                                        {"POD2", Convert.ToString(dr.Item("DISCHARGECOUNTRY2"))}}

            Dim agents As New Dictionary(Of String, String) From {{"POL1", Convert.ToString(dr.Item("AGENTPOL1"))},
                                                                  {"POL2", Convert.ToString(dr.Item("AGENTPOL2"))},
                                                                  {"POD1", Convert.ToString(dr.Item("AGENTPOD1"))},
                                                                  {"POD2", Convert.ToString(dr.Item("AGENTPOD2"))}}
            '発着国ごとのExRate
            Dim exRates As New Dictionary(Of String, String) 'Exレートを管理
            Dim curCodes As New Dictionary(Of String, String) '通貨コードを格納
            Dim GBA00010ExRate As New GBA00010ExRate
            '発着の国をループ
            For Each countryCode In countryCodes.Values
                '国コードが未指定、レート設定済みの場合はスキップ
                If countryCode = "" OrElse exRates.ContainsKey(countryCode) Then
                    Continue For
                End If
                'ExRate取得
                GBA00010ExRate.COUNTRYCODE = countryCode
                GBA00010ExRate.TARGETYM = Date.Now.ToString("yyyy/MM")
                GBA00010ExRate.getExRateInfo()
                If GBA00010ExRate.ERR = C_MESSAGENO.NORMAL Then
                    Dim exRtDt = GBA00010ExRate.EXRATE_TABLE
                    Dim exRtDr As DataRow = exRtDt.Rows(0)
                    exRates.Add(countryCode, Convert.ToString(exRtDr.Item("EXRATE")))
                Else
                    exRates.Add(countryCode, "0")
                End If
                '通貨コード取得
                Dim ctDt As DataTable = GetCountry(countryCode)
                If ctDt IsNot Nothing AndAlso ctDt.Rows.Count <> 0 Then
                    curCodes.Add(countryCode, Convert.ToString(ctDt.Rows(0).Item("CURRENCYCODE")))
                End If

            Next countryCode
            'BILLINGの設定
            Dim bliingFlg As String = "1"
            If dr.Item("BILLINGCATEGORY").Equals(GBC_DELIVERYCLASS.CONSIGNEE) Then
                bliingFlg = "0"
            End If

            For Each sqlResultDr As DataRow In sqlResultDt.Rows
                Dim writeDr As DataRow
                writeDr = retDt.NewRow
                Dim dtlPolPod As String = Convert.ToString(sqlResultDr.Item("AGENTKBN"))

                Dim countryCode As String = countryCodes(dtlPolPod)
                Dim rate As String = exRates(countryCode)

                writeDr.Item("DTLPOLPOD") = dtlPolPod
                writeDr.Item("COSTCODE") = sqlResultDr.Item("CODE")
                writeDr.Item("COSTNAME") = sqlResultDr.Item("NAMES")
                writeDr.Item("CAN_DELETE") = "0"
                writeDr.Item("SORT_ORDER") = sortOrder(dtlPolPod)
                writeDr.Item("ACTIONID") = sqlResultDr.Item("ACTIONID")
                writeDr.Item("CONTRACTOR") = sqlResultDr.Item("INITCONTRACTOR")
                writeDr.Item("CLASS1") = sqlResultDr.Item("CLASS1")
                writeDr.Item("CLASS2") = sqlResultDr.Item("CLASS2")
                writeDr.Item("CLASS3") = sqlResultDr.Item("CLASS3")
                writeDr.Item("CLASS4") = sqlResultDr.Item("CLASS4")
                writeDr.Item("CLASS5") = sqlResultDr.Item("CLASS5")
                writeDr.Item("CLASS6") = sqlResultDr.Item("CLASS6")
                writeDr.Item("CLASS7") = sqlResultDr.Item("CLASS7")
                writeDr.Item("CLASS8") = sqlResultDr.Item("CLASS8")
                writeDr.Item("CLASS9") = sqlResultDr.Item("CLASS9")
                writeDr.Item("CHARGECLASS4") = sqlResultDr.Item("CHARGECLASS4")
                writeDr.Item("CHARGECLASS8") = sqlResultDr.Item("CHARGECLASS8")

                writeDr.Item("AGENT") = agents(dtlPolPod)
                writeDr.Item("CINVOICEDBY") = agents(dtlPolPod)

                writeDr.Item("COUNTRYCODE") = countryCode
                writeDr.Item("TAXATION") = GetDefaultTaxation(countryCode)
                writeDr.Item("LOCALRATE") = rate
                writeDr.Item("CURRENCYCODE") = GBC_CUR_USD
                writeDr.Item("LOCALCURRENCY") = curCodes(countryCode)
                writeDr.Item("BILLING") = bliingFlg
                retDt.Rows.Add(writeDr)
                sortOrder(dtlPolPod) = sortOrder(dtlPolPod) + 1
            Next
        End If

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
            Dim sortOrder As New Dictionary(Of COSTITEM.CostItemGroup, Integer) From {{COSTITEM.CostItemGroup.Export1, 0},
                                                                                      {COSTITEM.CostItemGroup.Inport1, 0},
                                                                                      {COSTITEM.CostItemGroup.Export2, 0},
                                                                                      {COSTITEM.CostItemGroup.Inport2, 0}}
            Dim uniqueIndex As Integer = 0
            For Each costDr As DataRow In dt.Rows
                Dim countryCode As String = Nothing
                Dim item As New COSTITEM
                item.SortOrder = sortOrder(item.ItemGroup).ToString
                sortOrder(item.ItemGroup) = sortOrder(item.ItemGroup) + 1
                item.CostCode = Convert.ToString(costDr.Item("COSTCODE"))
                item.CostName = Convert.ToString(costDr.Item("COSTNAME"))
                item.BasedOn = Convert.ToString(costDr.Item("BASEON"))
                item.USD = Convert.ToString(costDr.Item("USD"))
                item.Local = Convert.ToString(costDr.Item("LOCAL"))
                item.LocalCurrncyRate = Convert.ToString(costDr.Item("LOCALRATE"))
                'item.USDRate = Convert.ToString(costDr.Item("USDRATE"))
                'item.Tax = Convert.ToString(costDr.Item("TAX"))
                item.Remarks = Convert.ToString(costDr.Item("REMARK"))
                item.ChargeClass4 = Convert.ToString(costDr.Item("CHARGECLASS4"))
                item.ChargeClass8 = Convert.ToString(costDr.Item("CHARGECLASS8"))
                item.IsAddedCost = Convert.ToString(costDr.Item("CAN_DELETE"))
                item.ConstractorCode = Convert.ToString(costDr.Item("CONTRACTOR"))
                item.Constractor = Convert.ToString(costDr.Item("CONTRACTORNAME"))
                item.SortOrder = Convert.ToString(costDr.Item("SORT_ORDER"))
                item.ActionId = Convert.ToString(costDr.Item("ACTIONID"))
                item.Class1 = Convert.ToString(costDr.Item("CLASS1"))
                item.Class2 = Convert.ToString(costDr.Item("CLASS2"))
                item.Class3 = Convert.ToString(costDr.Item("CLASS3"))
                item.Class4 = Convert.ToString(costDr.Item("CLASS4"))
                item.Class5 = Convert.ToString(costDr.Item("CLASS5"))
                item.Class6 = Convert.ToString(costDr.Item("CLASS6"))
                item.Class7 = Convert.ToString(costDr.Item("CLASS7"))
                item.Class8 = Convert.ToString(costDr.Item("CLASS8"))
                item.Class9 = Convert.ToString(costDr.Item("CLASS9"))
                If Convert.ToString(costDr.Item("CINVOICEDBY")) = C_JOT_AGENT Then
                    item.InvoicedBy = "1"
                Else
                    item.InvoicedBy = "0"
                End If
                item.Billing = Convert.ToString(costDr.Item("BILLING"))
                item.Taxation = Convert.ToString(costDr.Item("TAXATION"))
                item.CountryCode = Convert.ToString(costDr.Item("COUNTRYCODE"))
                Select Case Convert.ToString(costDr.Item("DTLPOLPOD"))
                    Case "POL1"
                        item.ItemGroup = COSTITEM.CostItemGroup.Export1
                        countryCode = Convert.ToString(orgDt.Rows(0).Item("RECIEPTCOUNTRY1"))
                    Case "POD1"
                        item.ItemGroup = COSTITEM.CostItemGroup.Inport1
                        countryCode = Convert.ToString(orgDt.Rows(0).Item("DISCHARGECOUNTRY1"))
                    Case "POL2"
                        item.ItemGroup = COSTITEM.CostItemGroup.Export2
                        countryCode = Convert.ToString(orgDt.Rows(0).Item("RECIEPTCOUNTRY2"))
                    Case "POD2"
                        item.ItemGroup = COSTITEM.CostItemGroup.Inport2
                        countryCode = Convert.ToString(orgDt.Rows(0).Item("DISCHARGECOUNTRY2"))
                End Select

                If demList.IndexOf(item.CostCode) <> -1 Then
                    Continue For
                End If

                retList.Add(item)

                item.UniqueIndex = uniqueIndex

                uniqueIndex = uniqueIndex + 1

                '初期値設定
                Me.txtLocalRateRef.Text = NumberFormat(Convert.ToString(costDr.Item("LOCALRATE")), countryCode, "", "1")
                'Me.txtUSDRateRef.Text = Convert.ToString(costDr.Item("USDRATE"))
            Next
        End If

        Return retList
    End Function

    ''' <summary>
    ''' 選択前の費用一覧の入力値を保持し、選択したタブに一致する費用情報を表示
    ''' </summary>
    ''' <param name="beforeTab">切替前のタブ</param>
    ''' <param name="selectedTab">切替後のタブ</param>
    Private Sub SetCostGridItem(ByVal beforeTab As String, ByVal selectedTab As String)
        If beforeTab <> Me.tabOrganizer.ClientID Then
            Dim beforeCostItemGroup As COSTITEM.CostItemGroup
            beforeCostItemGroup = COSTITEM.CostItemGroup.Export1
            Select Case beforeTab
                Case Me.tabExport1.ClientID
                    beforeCostItemGroup = COSTITEM.CostItemGroup.Export1
                Case Me.tabInport1.ClientID
                    beforeCostItemGroup = COSTITEM.CostItemGroup.Inport1
                Case Me.tabExport2.ClientID
                    beforeCostItemGroup = COSTITEM.CostItemGroup.Export2
                Case Me.tabInport2.ClientID
                    beforeCostItemGroup = COSTITEM.CostItemGroup.Inport2
            End Select

            SaveGridItem(beforeCostItemGroup)
        End If
        If selectedTab <> Me.tabOrganizer.ID Then
            Dim currentCostItemGroup = COSTITEM.CostItemGroup.Export1
            Dim countryCode As String = Nothing
            Select Case selectedTab
                Case Me.tabExport1.ClientID
                    currentCostItemGroup = COSTITEM.CostItemGroup.Export1
                    countryCode = txtRecieptCountry1.Text
                Case Me.tabInport1.ClientID
                    currentCostItemGroup = COSTITEM.CostItemGroup.Inport1
                    countryCode = txtDischargeCountry1.Text
                Case Me.tabExport2.ClientID
                    currentCostItemGroup = COSTITEM.CostItemGroup.Export2
                    countryCode = txtRecieptCountry2.Text
                Case Me.tabInport2.ClientID
                    currentCostItemGroup = COSTITEM.CostItemGroup.Inport2
                    countryCode = txtDischargeCountry2.Text
            End Select

            Dim allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
            Dim showCostList = (From allCostItem In allCostList
                                Where allCostItem.ItemGroup = currentCostItemGroup
                                Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
            Me.gvDetailInfo.DataSource = showCostList
            Me.gvDetailInfo.DataBind()

            If gvDetailInfo.Rows.Count > 0 Then

                Me.txtLocalRateRef.Text = NumberFormat(DirectCast(gvDetailInfo.Rows(0).FindControl("txtLocalRate"), System.Web.UI.WebControls.TextBox).Text, countryCode, "", "1")
                'Me.txtUSDRateRef.Text = DirectCast(gvDetailInfo.Rows(0).FindControl("txtUSDRate"), System.Web.UI.WebControls.TextBox).Text
            End If
        End If

    End Sub
    ''' <summary>
    ''' 費用項目一覧にコードを追加
    ''' </summary>
    ''' <param name="costCode"></param>
    Private Sub AddNewCostItem(costCode As String)
        Dim tabObjects As New List(Of HtmlControl) From {Me.tabInport1, Me.tabInport2, Me.tabExport1, Me.tabExport2}
        Dim costGroup As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Export1
        Dim countryCode As String = ""
        For Each tabObject In tabObjects
            If tabObject.Attributes("class") IsNot Nothing AndAlso tabObject.Attributes("class").Contains("selected") Then
                Select Case tabObject.ID
                    Case Me.tabInport1.ID
                        costGroup = COSTITEM.CostItemGroup.Inport1
                        countryCode = Me.txtDischargeCountry1.Text
                    Case Me.tabInport2.ID
                        costGroup = COSTITEM.CostItemGroup.Inport2
                        countryCode = Me.txtDischargeCountry2.Text
                    Case Me.tabExport1.ID
                        costGroup = COSTITEM.CostItemGroup.Export1
                        countryCode = Me.txtRecieptCountry1.Text
                    Case Me.tabExport2.ID
                        costGroup = COSTITEM.CostItemGroup.Export2
                        countryCode = Me.txtRecieptCountry2.Text
                End Select
            End If

        Next

        '左ボックス表示前の入力内容を一旦保持
        SaveGridItem(costGroup)
        '費用項目を取得
        Dim dt As DataTable = GetCost(Me.hdnBrType.Value, costCode, Me.hdnSelectedTabId.Value)
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        If allCostList Is Nothing Then
            allCostList = New List(Of COSTITEM)
        End If
        Dim item As New COSTITEM
        item.ItemGroup = costGroup
        item.CostCode = costCode
        item.CostName = Convert.ToString(dr.Item("NAME"))

        Dim addedCostList = (From allCostItem In allCostList
                             Where allCostItem.ItemGroup = costGroup _
                              And allCostItem.IsAddedCost = "1").ToList
        Dim maxSortNo As Integer = 0
        If addedCostList IsNot Nothing AndAlso addedCostList.Count > 0 Then
            maxSortNo = maxSortNo + addedCostList.Count + 1
        End If
        item.BasedOn = "1"
        'item.Tax = "0"
        item.USD = "0.00"
        item.Local = NumberFormat("0", countryCode)
        item.ConstractorCode = ""
        item.Constractor = ""
        'item.USDRate = Me.txtUSDRateRef.Text
        item.LocalCurrncyRate = Me.txtLocalRateRef.Text
        item.Class1 = ""
        item.Class3 = ""
        item.Class4 = ""
        item.Class5 = ""
        item.Class6 = ""
        item.Class7 = ""
        item.Class8 = "1"
        item.Class9 = Convert.ToString(dr.Item("CHARGECLASS9"))
        item.InvoicedBy = ""
        item.CountryCode = countryCode
        item.Remarks = ""
        item.ChargeClass4 = Convert.ToString(dr.Item("CHARGECLASS4"))
        item.ChargeClass8 = Convert.ToString(dr.Item("CHARGECLASS8"))
        item.SortOrder = Convert.ToString(maxSortNo)
        item.Taxation = GetDefaultTaxation(countryCode)
        item.IsAddedCost = "1"
        If Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.CONSIGNEE Then
            item.Billing = "0"
        Else
            item.Billing = "1"
        End If
        Dim maxUniqueIndex As Integer = 0
        Dim maxOrderdUniqueIndex = (From allCostItem In allCostList
                                    Order By allCostItem.UniqueIndex Descending).ToList
        Dim qMaxClass2 = (From allCostItem In allCostList
                          Where allCostItem.Class2 <> "" AndAlso IsNumeric(allCostItem.Class2)
                          Order By CInt(allCostItem.Class2) Descending)

        If maxOrderdUniqueIndex IsNot Nothing Then
            maxUniqueIndex = maxOrderdUniqueIndex(0).UniqueIndex + 1
        End If
        Dim maxClass2 As Integer = 0
        If qMaxClass2.Any Then
            maxClass2 = CInt(qMaxClass2.FirstOrDefault.Class2)
            maxClass2 = maxClass2 + 1
        End If
        If maxClass2 < maxUniqueIndex Then
            maxClass2 = maxUniqueIndex
        End If
        item.UniqueIndex = maxUniqueIndex
        item.Class2 = Convert.ToString(maxClass2)
        allCostList.Add(item)
        ViewState("COSTLIST") = allCostList

        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.ItemGroup = costGroup
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub
    ''' <summary>
    ''' 費用一覧のアイテムを削除
    ''' </summary>
    ''' <param name="uniqueIndex">内部保持しているuniqueインデックス</param>
    Private Sub DeleteCostItem(uniqueIndex As Integer)
        'Dim tabObjects As New List(Of HtmlControl) From {Me.tabInport1, Me.tabInport2, Me.tabExport1, Me.tabExport2}
        'Dim costGroup As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Export1
        'For Each tabObject In tabObjects
        '    If tabObject.Attributes("class") IsNot Nothing AndAlso tabObject.Attributes("class").Contains("selected") Then
        '        Select Case tabObject.ID
        '            Case Me.tabInport1.ID
        '                costGroup = COSTITEM.CostItemGroup.Inport1
        '            Case Me.tabInport2.ID
        '                costGroup = COSTITEM.CostItemGroup.Inport2
        '            Case Me.tabExport1.ID
        '                costGroup = COSTITEM.CostItemGroup.Export1
        '            Case Me.tabExport2.ID
        '                costGroup = COSTITEM.CostItemGroup.Export2
        '        End Select
        '    End If

        'Next
        Dim costGroup = GetCurrentTab(COSTITEM.CostItemGroup.Export1)
        '入力内容保持
        SaveGridItem(costGroup)

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))

        Dim removedCostList = (From allCostItem In allCostList
                               Where allCostItem.UniqueIndex <> uniqueIndex).ToList
        ViewState("COSTLIST") = removedCostList
        Dim showCostList = (From allCostItem In removedCostList
                            Where allCostItem.ItemGroup = costGroup
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()

        CalcSummaryCostUsd()

    End Sub
    ''' <summary>
    ''' 費目グリッドデータをViewStateに保存
    ''' </summary>
    ''' <param name="currentTab"></param>
    Private Sub SaveGridItem(currentTab As COSTITEM.CostItemGroup)
        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim retCostList = (From allCostItem In allCostList
                           Where allCostItem.ItemGroup <> currentTab).ToList

        Dim correctDispCostList As New List(Of COSTITEM)
        For Each gridItem As GridViewRow In Me.gvDetailInfo.Rows
            Dim item As New COSTITEM
            item.ItemGroup = currentTab
            item.CostCode = DirectCast(gridItem.FindControl("hdnCostCode"), HiddenField).Value
            item.CostName = DirectCast(gridItem.FindControl("hdnCostName"), HiddenField).Value
            item.BasedOn = DirectCast(gridItem.FindControl("txtBaseOn"), TextBox).Text
            'item.Tax = DirectCast(gridItem.FindControl("txtTax"), TextBox).Text
            item.USD = DirectCast(gridItem.FindControl("txtUsd"), TextBox).Text
            item.Local = DirectCast(gridItem.FindControl("txtLocal"), TextBox).Text
            item.ConstractorCode = DirectCast(gridItem.FindControl("txtContractor"), TextBox).Text
            item.Constractor = DirectCast(gridItem.FindControl("txtContractorText"), TextBox).Text
            'item.USDRate = DirectCast(gridItem.FindControl("txtUSDRate"), TextBox).Text
            item.LocalCurrncyRate = DirectCast(gridItem.FindControl("txtLocalRate"), TextBox).Text
            item.Remarks = DirectCast(gridItem.FindControl("hdnRemarks"), HiddenField).Value
            item.ChargeClass4 = DirectCast(gridItem.FindControl("hdnChargeClass4"), HiddenField).Value
            item.ChargeClass8 = DirectCast(gridItem.FindControl("hdnChargeClass8"), HiddenField).Value
            item.SortOrder = DirectCast(gridItem.FindControl("hdnSortOrder"), HiddenField).Value
            item.IsAddedCost = DirectCast(gridItem.FindControl("hdnIsAddedCost"), HiddenField).Value
            item.ActionId = DirectCast(gridItem.FindControl("hdnActionId"), HiddenField).Value
            item.Class1 = DirectCast(gridItem.FindControl("hdnClass1"), HiddenField).Value
            item.Class2 = DirectCast(gridItem.FindControl("hdnClass2"), HiddenField).Value
            item.Class3 = DirectCast(gridItem.FindControl("hdnClass3"), HiddenField).Value
            item.Class4 = DirectCast(gridItem.FindControl("hdnClass4"), HiddenField).Value
            item.Class5 = DirectCast(gridItem.FindControl("hdnClass5"), HiddenField).Value
            item.Class6 = DirectCast(gridItem.FindControl("hdnClass6"), HiddenField).Value
            item.Class7 = DirectCast(gridItem.FindControl("hdnClass7"), HiddenField).Value
            item.Class8 = DirectCast(gridItem.FindControl("hdnClass8"), HiddenField).Value
            If DirectCast(gridItem.FindControl("chkBl"), CheckBox).Checked Then
                item.Class9 = CONST_FLAG_YES
            Else
                item.Class9 = CONST_FLAG_NO
            End If
            If DirectCast(gridItem.FindControl("chkJOT"), CheckBox).Checked Then
                item.InvoicedBy = "1"
            Else
                item.InvoicedBy = "0"
            End If
            If DirectCast(gridItem.FindControl("chkSC"), CheckBox).Checked Then
                item.Billing = "1"
            Else
                item.Billing = "0"
            End If
            Dim chkTaxation As CheckBox = DirectCast(gridItem.FindControl("chkTaxation"), CheckBox)
            If chkTaxation IsNot Nothing Then
                If chkTaxation.Checked = True Then
                    item.Taxation = "1"
                ElseIf chkTaxation.Checked = False Then
                    item.Taxation = "0"
                End If
            End If
            item.CountryCode = DirectCast(gridItem.FindControl("hdnCountryCode"), HiddenField).Value
            Dim uniqueIndexString = DirectCast(gridItem.FindControl("hdnUniqueIndex"), HiddenField).Value
            Dim uniqueIndex = 0
            Integer.TryParse(uniqueIndexString, uniqueIndex)
            item.UniqueIndex = uniqueIndex

            retCostList.Add(item)
        Next
        ViewState("COSTLIST") = retCostList

    End Sub
    ''' <summary>
    ''' オーガナイザタブの費用合計を設定
    ''' </summary>
    Private Sub SetDisplayTotalCost(Optional ByVal isFirstLoad As Boolean = False)
        Me.txtTotalCost.Text = ""

        'カレント国コード取得
        Dim countryCode As String = getCountryCode()

        If ViewState("COSTLIST") Is Nothing Then
            Return
        End If
        Dim costList As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim costTotal As Integer = 0
        Dim summaryTarget = (From allCostItem In costList
                             Where allCostItem.USD <> "" AndAlso IsNumeric(allCostItem.USD)).ToList
        If summaryTarget IsNot Nothing Then
            Dim dimSummary As Decimal = summaryTarget.Sum(Function(item) Decimal.Parse(item.USD))
            Dim coms As Decimal = 0
            If Decimal.TryParse(Me.txtFee.Text, coms) Then
                Me.txtFee.Text = NumberFormat(Me.txtFee.Text, "", "", "", "1")
                dimSummary += coms
            End If

            Dim dislpaySummary As String = NumberFormat(dimSummary, countryCode, "", "", "1")

            If Me.hdnBrType.Value = "1" Then

                If Me.hdnPrevTotalInvoicedValue.Value <> dislpaySummary AndAlso isFirstLoad <> True Then
                    Me.hdnCanCalcHireageCommercialFactor.Value = "1"
                    CalcHireageCommercialfactor()
                ElseIf Me.hdnCanCalcHireageCommercialFactor.Value = "1" Then
                    Me.hdnCanCalcHireageCommercialFactor.Value = "1" '既に変更が立っていて発動していない場合
                Else
                    Me.hdnCanCalcHireageCommercialFactor.Value = ""
                End If
            Else

                Me.hdnCanCalcHireageCommercialFactor.Value = ""

                Me.txtInvoicedTotal.Text = dislpaySummary
            End If

            Me.txtTotalCost.Text = dislpaySummary
            Me.hdnPrevTotalInvoicedValue.Value = dislpaySummary
            If Me.hdnCanCalcHireageCommercialFactor.Value = "1" Then
                CalcHireageCommercialfactor()
                Me.hdnCanCalcHireageCommercialFactor.Value = ""
            End If
        End If
    End Sub
    ''' <summary>
    ''' 表示非表示制御(刷新前)
    ''' </summary>
    ''' <param name="isOwner">オーナータブが選択された場合</param>
    ''' <param name="selectedTab">選択されたタブ</param>
    Private Sub visibleControl(ByVal isOwner As Boolean, ByVal selectedTab As String)

        '一旦後続で表示/非表示を切り替える箇所をすべて非表示にする
        Dim allVisibleControls As New List(Of Control)
        allVisibleControls.AddRange({Me.trPortInfoRow1, Me.trPortInfoRow2,
                                     Me.trPortInfoRow3, Me.trPortInfoRow4,
                                     Me.trPortInfoRow5,
                                     Me.trCarrierSubInfoRow1, Me.trCarrierSubInfoRow2,
                                     Me.trCarrierSubInfoRow3,
                                     Me.trProductTankSubInfoRow1, Me.trProductTankSubInfoRow2,
                                     Me.trHireageInfoRow1, Me.trHireageInfoRow2,
                                     Me.trHireageInfoRow3, Me.trHireageInfoRow4,
                                     Me.trHireageJPYInfoRow1, Me.trHireageJPYInfoRow2,
                                     Me.trHireageJPYInfoRow3,
                                     Me.divDemurrage,
                                     Me.divBrDetailInfo,
                                     Me.btnInputRequest, Me.btnEntryCost, Me.btnReject, Me.btnApply, Me.btnPrint, Me.btnApproval, Me.btnAppReject,
                                     Me.trCostInfoRow1, Me.trCostInfoRow2,
                                     Me.lblCarrier2, Me.txtCarrier2, Me.lblCarrier2Text,
                                     Me.tabExport2, Me.tabInport2,
                                     Me.chkMailExport2, Me.chkMailInport2,
                                     Me.lblchkExport2, Me.chkInputRequestExport2,
                                     Me.lblchkImport2, Me.chkInputRequestImport2,
                                     Me.lblConsignee, Me.txtConsignee, Me.lblConsigneeText,
                                     Me.tabInport1,
                                     Me.chkMailInport1,
                                     Me.lblchkImport1, Me.chkInputRequestImport1,
                                     Me.lblRemarks2, Me.btnOutputExcel, Me.btnSave, Me.btnAddCost,
                                     Me.lblDisabled, Me.chkDisabled})
        For Each item In allVisibleControls
            item.Visible = False
        Next

        '以下表示するコントロールのみの制御を行うこと、さらなる表示→非表示の制御は行わない
        Dim visibleControls As New List(Of Control)
        '******************************
        'オーガナイザヘッダー(共通情報)部分
        '******************************
        'POLのみのブレーカー以外
        If Not ViewState(CONST_VS_DISP_POLONLY).Equals("1") Then
            visibleControls.AddRange({Me.lblConsignee, Me.txtConsignee, Me.lblConsigneeText, Me.tabInport1,
                                      Me.chkMailInport1, Me.lblchkImport1, Me.chkInputRequestImport1})
        End If
        '三国間の場合第二輸送部分の共通情報を表示
        If Me.hdnIsTrilateral.Value = "1" Then
            visibleControls.AddRange({Me.lblCarrier2, Me.txtCarrier2, Me.lblCarrier2Text,
                                      Me.tabExport2, Me.tabInport2,
                                      Me.chkMailExport2, Me.chkMailInport2,
                                      Me.lblchkExport2, Me.chkInputRequestExport2,
                                      Me.lblchkImport2, Me.chkInputRequestImport2})
        End If
        '******************************
        'フッター部分
        '******************************
        If isOwner = True Then 'オーナータブの場合
            visibleControls.AddRange({Me.trPortInfoRow1, Me.trPortInfoRow2,
                                      Me.trCarrierSubInfoRow1, Me.trCarrierSubInfoRow2,
                                      Me.trProductTankSubInfoRow1, Me.trProductTankSubInfoRow2,
                                      Me.trHireageInfoRow1, Me.trHireageInfoRow2,
                                      Me.trHireageInfoRow3, Me.trHireageInfoRow4,
                                      Me.trCostInfoRow1, Me.trCostInfoRow2,
                                      Me.divDemurrage})

            If Not ViewState(CONST_VS_DISP_POLONLY).Equals("1") Then
                visibleControls.Add(Me.trPortInfoRow3)
            End If

            If Me.hdnIsTrilateral.Value = "1" Then
                visibleControls.AddRange({Me.trPortInfoRow4, Me.trPortInfoRow5, Me.trCarrierSubInfoRow3})
            End If
            If Me.hdnCountryOrganizer.Value.Equals("JP") AndAlso GBA00003UserSetting.IS_JOTUSER Then
                'ブレーカーのオーガナイザ国が日本かつ、JOTユーザの場合は円表示解放
                visibleControls.AddRange({Me.trHireageJPYInfoRow1, Me.trHireageJPYInfoRow2, Me.trHireageJPYInfoRow3})
            End If
        Else '代理店の場合
            visibleControls.AddRange({Me.trPortInfoRow1, Me.divBrDetailInfo})
            If selectedTab.EndsWith("1") Then
                visibleControls.AddRange({Me.trPortInfoRow2})
                If Not ViewState(CONST_VS_DISP_POLONLY).Equals("1") Then
                    visibleControls.Add(Me.trPortInfoRow3)
                End If
            Else
                visibleControls.AddRange({Me.trPortInfoRow4, Me.trPortInfoRow5, Me.lblRemarks2})
            End If
        End If
        '******************************
        'ボタン表示
        '******************************
        If Me.hdnIsViewOnlyPopup.Value = "1" Then
            '完全リードオンリーパターン
            If isOwner Then
                visibleControls.Add(Me.btnPrint) '印刷ボタンのみ
            End If
        ElseIf Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
            '完全リードオンリーパターン
            If isOwner Then
                visibleControls.Add(Me.btnPrint) '印刷ボタンのみ
            End If
        ElseIf Me.hdnIsViewFromApprove.Value = "1" Then
            '承認画面からの遷移
            If isOwner Then
                visibleControls.Add(Me.btnPrint)
                visibleControls.Add(Me.btnSave)
                visibleControls.Add(Me.lblDisabled)
                visibleControls.Add(Me.chkDisabled)
            End If

            '否認編集時のみ別の見せ方をする
            Select Case Me.hdnStatus.Value
                Case C_APP_STATUS.REVISE '否認入力
                    visibleControls.AddRange({Me.btnAppReject, Me.btnOutputExcel, Me.btnAddCost})
                    If isOwner = False Then
                        visibleControls.Add(Me.btnSave)
                    End If
                Case Else '他のステータス
                    visibleControls.AddRange({Me.btnApproval, Me.btnAppReject, Me.btnReject})
            End Select
        Else
            '通常パターン
            visibleControls.Add(Me.btnSave)
            If isOwner Then
                visibleControls.AddRange({Me.btnOutputExcel, Me.btnApply, Me.btnInputRequest, Me.btnPrint})
            Else
                visibleControls.AddRange({Me.btnOutputExcel, Me.btnEntryCost, Me.btnAddCost})
            End If
        End If
        '******************************
        '設定したオブジェクトにつき表示
        '******************************
        For Each item In visibleControls
            item.Visible = True
        Next

    End Sub
    ''' <summary>
    ''' 使用可否制御
    ''' </summary>
    ''' <param name="isOwner">オーガナイザタブが選択された場合True</param>    
    Private Sub enabledControls(isOwner As Boolean)
        Dim orgHeaderInputs As New List(Of Control) _
                      From {Me.txtBrType, Me.txtBrStYmd, Me.txtBrEndYmd, Me.lblBrRemarkText,
                            Me.txtBrTerm, Me.txtNoOfTanks, Me.txtInvoiced, Me.txtBillingCategory,
                            Me.txtAppRequestYmd, Me.txtAppOffice, Me.txtAppSalesPic, Me.lblApplyRemarks,
                            Me.txtApprovedYmd, Me.txtAppJotPic, Me.lblAppJotRemarks,
                            Me.txtShipper, Me.txtConsignee, Me.txtCarrier1, Me.txtCarrier2,
                            Me.txtProduct, Me.txtImdg, Me.txtUNNo}

        Dim orgBottomInputs As New List(Of Control) _
                      From {Me.txtRecieptCountry1, Me.txtRecieptPort1, Me.txtLoadCountry1, Me.txtLoadPort1, Me.txtAgentPol1, Me.lblRemarks,
                            Me.txtDischargeCountry1, Me.txtDischargePort1, Me.txtDeliveryCountry1, Me.txtDeliveryPort1, Me.txtAgentPod1,
                            Me.txtRecieptCountry2, Me.txtRecieptPort2, Me.txtLoadCountry2, Me.txtLoadPort2, Me.txtAgentPol2, Me.lblRemarks2,
                            Me.txtDischargeCountry2, Me.txtDischargePort2, Me.txtDeliveryCountry2, Me.txtDeliveryPort2, Me.txtAgentPod2,
                            Me.txtVsl1, Me.txtVoy1, Me.txtEtd1, Me.txtEta1,
                            Me.txtVsl2, Me.txtVoy2, Me.txtEtd2, Me.txtEta2,
                            Me.txtWeight, Me.txtSGravity, Me.txtTankCapacity, Me.txtTankFillingRate, Me.txtTankFillingCheck,
                            Me.txtTotal, Me.txtLoading, Me.txtSteaming, Me.txtTip, Me.txtExtra,
                            Me.txtTotalCost, Me.txtJOTHireage, Me.txtCommercialFactor, Me.txtInvoicedTotal, Me.txtPerDay,
                            Me.txtAmtRequest, Me.txtAmtPrincipal, Me.txtAmtDiscount,
                            Me.txtFee,
                            Me.txtDemurdayF1, Me.txtDemurdayT1, Me.txtDemurUSRate1, Me.txtDemurday2, Me.txtDemurUSRate2}

        Dim costBottomInputs As New List(Of Control) _
                    From {Me.btnAddCost, Me.iptAgencySummaryUsd, Me.txtLocalRateRef,
                          Me.gvDetailInfo}

        Dim actionButtons As New List(Of Control) _
                    From {Me.btnApproval, Me.btnAppReject, Me.btnReject, Me.btnOutputExcel,
                          Me.btnSave, Me.btnApply, Me.btnInputRequest, Me.btnEntryCost,
                          Me.btnPrint}
        'Me.btnBackについていかなる状況でも利用可能な為不可にしない

        '一旦すべてのコントロールを使用不可にする
        EnabledChangeListObjects(False, orgHeaderInputs, orgBottomInputs, costBottomInputs, actionButtons)

        '対象タブに編集する権限なしor遷移元がオーダー一覧の場合は何もさせない
        If Me.hdnIsViewFromApprove.Value <> "1" AndAlso ((Me.hdnCountryControl.Value = "0" AndAlso Me.hdnEnableControl.Value = "1") OrElse
            Me.hdnIsViewOnlyPopup.Value = "1") Then
            '権限なしの為すべて不可で終了(ExcelとPrintのみ使用可能)
            Dim viewModeEnableObjects As New List(Of Control) From {Me.btnOutputExcel, Me.btnPrint}
            '使用可否制御修正まえで申請可能だと発着どちらだろうが申請出すこと可能(これはダメだと思うので、確認後コメント)
            If Me.hdnApply.Value = "1" Then
                viewModeEnableObjects.Add(Me.btnApply)
            End If
            'リスト内のオブジェクトを使用可能に変更
            EnabledChangeListObjects(True, viewModeEnableObjects)

            Return
        End If
        'これ以降は使用可能の制御のみを行うこと
        Dim enabledObjectList As New List(Of Control)
        '****************************************
        '承認画面からの遷移時制御
        '****************************************
        If Me.hdnIsViewFromApprove.Value = "1" Then
            '承認画面遷移時はNOTE及びPRINT,SAVEは何があっても利用可能
            enabledObjectList.AddRange({Me.lblBrRemarkText, Me.btnPrint, Me.btnSave})
            '承認画面遷移でのステータス事の制御
            Select Case Me.hdnStatus.Value
                Case C_APP_STATUS.APPLYING '【申請中】
                    '承認・否認・編集(EDIT)ボタン解放
                    'enabledObjectList.AddRange({Me.btnApproval, Me.btnAppReject, Me.btnReject})
                    enabledObjectList.AddRange({Me.lblAppJotRemarks, Me.btnApproval, Me.btnAppReject, Me.btnReject})
                Case C_APP_STATUS.REVISE   '【編集(EDIT)での解放時OR中】
                    '承認コメント,否認ボタン、費目追加,発着代理店費用項目グリッド
                    enabledObjectList.AddRange({Me.lblAppJotRemarks, Me.gvDetailInfo, Me.btnAddCost, Me.btnAppReject})
                    '共通項目入力項目
                    If isOwner Then 'オーガナイザタブ選択時
                        enabledObjectList.AddRange({Me.txtBrStYmd, Me.txtBrEndYmd,
                                                    Me.txtBrTerm, Me.txtNoOfTanks, Me.txtInvoiced, Me.txtBillingCategory,
                                                    Me.txtConsignee, Me.txtCarrier1, Me.txtCarrier2,
                                                    Me.txtProduct})
                    End If

                    'オーガナイザタブ入力項目解放
                    enabledObjectList.AddRange({Me.txtVsl1, Me.txtVoy1, Me.txtEtd1, Me.txtEta1,
                                                Me.txtVsl2, Me.txtVoy2, Me.txtEtd2, Me.txtEta2,
                                                Me.txtWeight, Me.txtTankCapacity,
                                                Me.txtLoading, Me.txtSteaming, Me.txtTip, Me.txtExtra,
                                                Me.txtJOTHireage, Me.txtCommercialFactor, Me.txtInvoicedTotal,
                                                Me.txtAmtPrincipal,
                                                Me.txtFee,
                                                Me.txtDemurdayT1, Me.txtDemurUSRate2})
            End Select
        End If
        '****************************************
        '通常画面からの遷移時制御
        '****************************************
        If Me.hdnIsViewFromApprove.Value <> "1" Then

            Select Case Me.hdnStatus.Value
                Case "", C_APP_STATUS.APPAGAIN, C_APP_STATUS.REJECT '新規入力、否認後
                    If isOwner Then 'オーガナイザタブ選択
                        'オーガナイザ情報（上部）
                        enabledObjectList.AddRange({Me.txtBrStYmd, Me.txtBrEndYmd, Me.txtBrTerm, Me.lblBrRemarkText,
                                                    Me.txtNoOfTanks, Me.txtInvoiced, Me.txtBillingCategory, Me.lblApplyRemarks,
                                                    Me.txtConsignee, Me.txtCarrier1, Me.txtCarrier2,
                                                    Me.txtProduct})
                        'オーガナイザ情報（下部）
                        enabledObjectList.AddRange({Me.txtAgentPol1, Me.txtAgentPod1,
                                                    Me.txtAgentPol2, Me.txtAgentPod2,
                                                    Me.lblRemarks,
                                                    Me.txtVsl1, Me.txtVoy1, Me.txtEtd1, Me.txtEta1,
                                                    Me.txtVsl2, Me.txtVoy2, Me.txtEtd2, Me.txtEta2,
                                                    Me.txtWeight, Me.txtTankCapacity,
                                                    Me.txtLoading, Me.txtSteaming, Me.txtTip, Me.txtExtra,
                                                    Me.txtJOTHireage, Me.txtCommercialFactor, Me.txtInvoicedTotal,
                                                    Me.txtAmtRequest,
                                                    Me.txtFee,
                                                    Me.txtDemurdayT1, Me.txtDemurUSRate1, Me.txtDemurUSRate2})
                        '港変更不可を設定
                        Dim editbleCost As Boolean = False '他タブが費用編集可能な状態か判定
                        If lblBrNo.Text <> "" Then
                            Dim dtStat = GetStatus(Me.lblBrNo.Text)
                            Dim qStat = From statItem In dtStat Where statItem("STATUS").Equals(C_APP_STATUS.EDITING)
                            If qStat.Any Then
                                editbleCost = True
                            End If
                        End If
                        'インプットリクエスト状態、費用入力状態の場合は港変更不可を維持
                        If Not (Me.hdnInputReq.Value = "1" OrElse editbleCost) Then
                            enabledObjectList.AddRange({Me.txtRecieptPort1, Me.txtRecieptPort2,
                                                        Me.txtDischargePort1, Me.txtDischargePort2})
                        End If
                        'オーガナイザ情報（ボタン制御）
                        'インプットリクエスト
                        If Not (Me.hdnNewBreaker.Value = "1" OrElse Me.hdnInputReq.Value = "1") Then
                            '新規作成 または インプットリクエスト不可能の場合以外使用化
                            enabledObjectList.Add(Me.btnInputRequest)
                        End If
                        '申請ボタン
                        If Me.hdnApply.Value = "1" Then
                            enabledObjectList.Add(Me.btnApply)
                        End If
                        'その他ボタン
                        enabledObjectList.AddRange({Me.btnOutputExcel, Me.btnSave, Me.btnPrint})
                    Else '費用タブ選択時
                        If Me.hdnEntryCost.Value <> "1" Then
                            enabledObjectList.AddRange({Me.btnAddCost, Me.gvDetailInfo, Me.lblRemarks, Me.lblRemarks2})
                            enabledObjectList.AddRange({Me.btnSave, Me.btnEntryCost})
                        End If
                        'その他ボタン
                        enabledObjectList.AddRange({Me.btnOutputExcel, Me.btnPrint})
                    End If

                Case C_APP_STATUS.APPLYING '申請中
                    enabledObjectList.AddRange({Me.btnOutputExcel, Me.btnPrint})
                Case C_APP_STATUS.REVISE '否認時 入力
                    '承認画面から来ない限り操作不可
                    enabledObjectList.AddRange({Me.btnOutputExcel, Me.btnPrint})
                Case C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE '承認後、自動承認後
                    enabledObjectList.AddRange({Me.btnOutputExcel, Me.btnPrint})
            End Select
        End If
        '一旦すべてのコントロールを使用不可にする
        EnabledChangeListObjects(True, enabledObjectList)

    End Sub
    ''' <summary>
    ''' 渡されたリストに付き一括でEnabledの変更を行う
    ''' </summary>
    ''' <param name="enabled">使用可否</param>
    ''' <param name="items">制御するコントロールリスト</param>
    Private Sub EnabledChangeListObjects(enabled As Boolean, ParamArray items() As List(Of Control))
        For Each listObj In items
            For Each controlObj In listObj
                If TypeOf controlObj Is WebControl Then
                    Dim txtObj As WebControl = DirectCast(controlObj, WebControl)
                    txtObj.Enabled = enabled
                ElseIf TypeOf controlObj Is HtmlControl Then
                    Dim btnObj As HtmlControl = DirectCast(controlObj, HtmlControl)
                    btnObj.Disabled = Not enabled
                ElseIf TypeOf controlObj Is HtmlGenericControl Then
                    Dim btnObj As HtmlGenericControl = DirectCast(controlObj, HtmlGenericControl)
                    btnObj.Disabled = Not enabled
                Else
                    Dim aaa As String = ""
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 左ボックスのリストデータをクリア
    ''' </summary>
    ''' <remarks>viewstateのデータ量軽減</remarks>
    Private Sub ClearLeftListData()
        Me.lbCarrier.Items.Clear()
        Me.lbConsignee.Items.Clear()
        Me.lbCountry.Items.Clear()
        Me.lbPort.Items.Clear()
        Me.lbProduct.Items.Clear()
        Me.lbShipper.Items.Clear()
        Me.lbCost.Items.Clear()
        Me.mvLeft.SetActiveView(Me.vLeftCal)
    End Sub
    ''' <summary>
    ''' 国データ取得
    ''' </summary>
    ''' <param name="countryCode">国コード(オプショナル)未指定時は全件</param>
    ''' <returns>国データテーブル</returns>
    ''' <remarks>GBM0001_COUNTRYより国データを取得</remarks>
    Private Function GetCountry(Optional ByVal countryCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim textField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMES"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT COUNTRYCODE")
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("     , CURRENCYCODE")
        sqlStat.AppendLine("     , TAXRATE")
        sqlStat.AppendLine("  FROM GBM0001_COUNTRY")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")

        If countryCode <> "" Then
            sqlStat.AppendLine("   And COUNTRYCODE = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY COUNTRYCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 10).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@COUNTRYCODE", SqlDbType.NVarChar, 10).Value = countryCode
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
    ''' 国を設定
    ''' </summary>
    ''' <param name="targetTextObject">国コードテキストボックス</param>
    ''' <param name="countryCode">国コード</param>
    Private Sub SetDisplayCountry(targetTextObject As TextBox, countryCode As String)
        Dim targetLabel As Label = Nothing
        '港情報を初期化
        Dim portSet = {New With {.tgtId = Me.txtRecieptCountry1.ID, .codeObj = Me.txtRecieptPort1, .textObj = Me.lblRecieptPort1Text},
                       New With {.tgtId = Me.txtLoadCountry1.ID, .codeObj = Me.txtLoadPort1, .textObj = Me.lblLoadPort1Text},
                       New With {.tgtId = Me.txtDischargeCountry1.ID, .codeObj = Me.txtDischargePort1, .textObj = Me.lblDischargePort1Text},
                       New With {.tgtId = Me.txtDeliveryCountry1.ID, .codeObj = Me.txtDeliveryPort1, .textObj = Me.lblDeliveryPort1Text},
                       New With {.tgtId = Me.txtRecieptCountry2.ID, .codeObj = Me.txtRecieptPort2, .textObj = Me.lblRecieptPort2Text},
                       New With {.tgtId = Me.txtLoadCountry2.ID, .codeObj = Me.txtLoadPort2, .textObj = Me.lblLoadPort2Text},
                       New With {.tgtId = Me.txtDischargeCountry2.ID, .codeObj = Me.txtDischargePort2, .textObj = Me.lblDischargePort2Text},
                       New With {.tgtId = Me.txtDeliveryCountry2.ID, .codeObj = Me.txtDeliveryPort2, .textObj = Me.lblDeliveryPort2Text}
                      }.ToDictionary(Function(obj) obj.tgtId, Function(obj) obj)

        If portSet.ContainsKey(targetTextObject.ID) Then
            With portSet(targetTextObject.ID)
                .codeObj.Text = ""
                .textObj.Text = ""
            End With
        End If

        targetTextObject.Text = countryCode.Trim
        '国コードが未入力の場合はDBアクセスせずに終了
        If countryCode.Trim = "" Then
            Return
        End If

        Dim dt As DataTable = GetCountry(countryCode.Trim)
        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If

    End Sub
    ''' <summary>
    ''' 積載品マスタより情報を取得
    ''' </summary>
    ''' <param name="targetTextObject">国コードテキストボックス</param>
    ''' <param name="productCode">積載品コード</param>
    Private Sub SetDisplayProduct(targetTextObject As TextBox, productCode As String)
        '積載品の付帯情報を一旦クリア
        Me.lblProductText.Text = ""
        targetTextObject.Text = productCode.Trim
        Me.txtImdg.Text = ""
        Me.txtUNNo.Text = ""
        Me.txtSGravity.Text = ""

        '積載品コードが未入力の場合はDBアクセスせずに終了
        If productCode.Trim = "" Then
            Return
        End If

        Dim dt As DataTable = GetProduct(productCode.Trim)
        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        '取得データを画面に展開
        Dim dr As DataRow = dt.Rows(0)
        Me.lblProductText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
        'Dim imdg = Convert.ToString(dr.Item("IMDGCODE"))
        'Me.txtImdg.Text = imdg
        Dim hcls = Convert.ToString(dr.Item("HAZARDCLASS"))
        If hcls = "" Then
            Me.hdnProductIsHazard.Value = "0"
            Me.txtImdg.Text = PRODUCT_NONDG
        Else
            Me.hdnProductIsHazard.Value = "1"
            Me.txtImdg.Text = hcls
        End If
        Dim unno As String = Convert.ToString(dr.Item("UNNO"))
        Me.txtUNNo.Text = unno
        If unno = "" Then
            '    Me.txtUNNo.Text = PRODUCT_NA
            Me.txtUNNo.Text = PRODUCT_NONDG
        End If

        Me.txtSGravity.Text = Convert.ToString(dr.Item("GRAVITY"))
        Me.hdnPrpvisions.Value = Convert.ToString(dr.Item("PRPVISIONS"))
        CalcFillingRate() '設定したGRAVITYをもとにFillingRate再計算
    End Sub
    ''' <summary>
    ''' 荷受人一覧取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <param name="customerCode">顧客コード</param>
    ''' <returns>荷受人一覧データテーブル</returns>
    ''' <remarks>GBM0004_CUSTOMERより荷受人情報を取得</remarks>
    Private Function GetConsignee(countryCode As String, Optional customerCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("Select CUSTOMERCODE")
        sqlStat.AppendFormat("      , {0} As NAME", textField).AppendLine()
        sqlStat.AppendFormat("      , CUSTOMERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If customerCode <> "" Then
            sqlStat.AppendLine("   AND CUSTOMERCODE    = @CUSTOMERCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("ORDER BY CUSTOMERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@COUNTRYCODE", SqlDbType.NVarChar, 20).Value = countryCode
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20).Value = customerCode
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
    ''' 荷主名称を画面に設定
    ''' </summary>
    ''' <param name="targetTextObject">対象テキスト</param>
    ''' <param name="customerCode">荷主コード（顧客コード）</param>
    Private Sub SetDisplayConsignee(targetTextObject As TextBox, customerCode As String)
        '一旦リセット
        targetTextObject.Text = customerCode.Trim
        Me.lblConsigneeText.Text = ""
        '荷主コード（顧客コード）が未入力の場合はDBアクセスせずに終了
        If customerCode.Trim = "" Then
            Return
        End If
        Dim countryCode As String = Me.txtDeliveryCountry1.Text

        Dim dt As DataTable = New DataTable
        If Me.hdnBrType.Value = "1" Then
            dt = GetConsignee(countryCode, customerCode.Trim)
        Else
            dt = GetAgent(countryCode, customerCode.Trim)
        End If

        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)
        Me.lblConsigneeText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
    End Sub
    ''' <summary>
    ''' 船会社情報取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <param name="carrierCode">船会社コード</param>
    ''' <returns>船会社情報一覧</returns>
    ''' <remarks>GBM0005_TRADERテーブルより船会社情報一覧を取得する</remarks>
    Private Function GetCarrier(countryCode As String, Optional carrierCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "NAMES"
        'If COA0019Session.LANGDISP <> C_LANG.JA Then
        '    textField = "NAMES"
        'End If
        'SQL文作成(TODO:ORGもキーだが今のところ未設定)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CARRIERCODE AS CODE")
        sqlStat.AppendFormat("      ,{0} AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,CARRIERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0005_TRADER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If carrierCode <> "" Then
            sqlStat.AppendLine("   AND CARRIERCODE    = @CARRIERCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        'sqlStat.AppendLine("   AND CLASS = 'FORWARDER'")
        sqlStat.AppendLine("   AND CLASS = '" & C_TRADER.CLASS.CARRIER & "'")
        sqlStat.AppendLine("ORDER BY CARRIERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@COUNTRYCODE", SqlDbType.NVarChar, 20).Value = countryCode
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
    ''' 船会社名称を画面に設定
    ''' </summary>
    ''' <param name="targetTextObject">対象テキスト</param>
    ''' <param name="carrierCode">船会社コード</param>
    Private Sub SetDisplayCarrier(targetTextObject As TextBox, carrierCode As String)
        '一旦リセット
        targetTextObject.Text = carrierCode.Trim
        Dim targetLabel As Label = Me.lblCarrier1Text
        If targetTextObject.ID = Me.txtCarrier2.ClientID Then
            targetLabel = Me.lblCarrier2Text
        End If
        targetLabel.Text = ""
        '荷主コード（顧客コード）が未入力の場合はDBアクセスせずに終了
        If carrierCode.Trim = "" Then
            Return
        End If
        Dim countryCode As String = Me.txtLoadCountry1.Text
        If targetTextObject.ID = Me.txtCarrier2.ID Then
            countryCode = Me.txtLoadCountry2.Text
        End If
        Dim dt As DataTable = GetCarrier(countryCode, carrierCode.Trim)
        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)
        targetLabel.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
    End Sub
    ''' <summary>
    ''' 積載品検索
    ''' </summary>
    ''' <param name="productCode">積載品コード（省略時は全件）</param>
    ''' <returns></returns>
    Private Function GetProduct(Optional productCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "PRODUCTNAME"

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT rtrim(PRODUCTCODE) AS CODE")
        sqlStat.AppendFormat("      ,rtrim({0}) AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,rtrim(PRODUCTCODE) + ':' + rtrim({0})  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("      ,rtrim(IMDGCODE) AS IMDGCODE")
        sqlStat.AppendLine("      ,rtrim(UNNO) AS UNNO")
        sqlStat.AppendLine("      ,rtrim(GRAVITY) AS GRAVITY")
        sqlStat.AppendLine("      ,rtrim(HAZARDCLASS) AS HAZARDCLASS")
        sqlStat.AppendLine("      ,rtrim(PRPVISIONS) AS PRPVISIONS")
        sqlStat.AppendLine("  FROM GBM0008_PRODUCT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")

        If productCode <> "" Then
            sqlStat.AppendLine("   AND PRODUCTCODE    = @PRODUCTCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND ENABLED      = @ENABLED")
        sqlStat.AppendLine("ORDER BY PRODUCTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
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
    ''' 輸送パターンデータテーブル取得
    ''' </summary>
    ''' <param name="breakerType">ブレーカー種類</param>
    ''' <returns>輸送パターンデータテーブル</returns>
    ''' <param name="useType">単一取得の場合はUseTypeを指定</param>
    ''' <remarks>GBM0009よりブレーカー一覧を取得</remarks>
    Private Function GetTransferPattern(breakerType As String, Optional ByVal useType As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT USETYPE")
        sqlStat.AppendLine("     , NAMES")
        sqlStat.AppendLine("     , SUM(CASE AGENTKBN WHEN 'POD1' THEN 1 ELSE 0 END) AS POD1COUNT ")
        sqlStat.AppendLine("     , SUM(CASE AGENTKBN WHEN 'POL1' THEN 1 ELSE 0 END) AS POL1COUNT ")
        sqlStat.AppendLine("     , SUM(CASE AGENTKBN WHEN 'POD2' THEN 1 ELSE 0 END) AS POD2COUNT ")
        sqlStat.AppendLine("     , SUM(CASE AGENTKBN WHEN 'POL2' THEN 1 ELSE 0 END) AS POL2COUNT ")
        sqlStat.AppendLine("  FROM GBM0009_TRPATTERN")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND ORG         = @ORG")
        sqlStat.AppendLine("   AND BRTYPE = @BREAKERTYPE")
        If useType <> "" Then
            sqlStat.AppendLine("   AND USETYPE     = @USETYPE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("GROUP BY USETYPE, NAMES")
        sqlStat.AppendLine("ORDER BY USETYPE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            Dim breakerTypeDev As String = C_BRTYPE.SALES
            If breakerType <> "1" Then
                breakerTypeDev = C_BRTYPE.OPERATION
            End If
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@ORG", SqlDbType.NVarChar, 20).Value = "GB_Default"
                .Add("@BREAKERTYPE", SqlDbType.NVarChar, 20).Value = breakerTypeDev
                .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = useType
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES

            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using 'End sqlCon,sqlCmd
        Return retDt
    End Function
    ''' <summary>
    ''' 荷主一覧取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <param name="customerCode">顧客コード(オプショナル)未指定時は国コードで絞りこんだ全件</param>
    ''' <returns>荷主一覧データテーブル</returns>
    ''' <remarks>GBM0004_CUSTOMERより荷主一覧を取得</remarks>
    Private Function GetShipper(countryCode As String, Optional customerCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CUSTOMERCODE")
        sqlStat.AppendFormat("      ,{0} AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,CUSTOMERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If customerCode <> "" Then
            sqlStat.AppendLine("   AND CUSTOMERCODE    = @CUSTOMERCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("ORDER BY CUSTOMERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@COUNTRYCODE", SqlDbType.NVarChar, 20).Value = countryCode
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20).Value = customerCode
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
    ''' 費用一覧取得
    ''' </summary>
    ''' <param name="breakerType">ブレーカー種類</param>
    ''' <param name="costCode">費用コード(未指定時は全件)</param>
    ''' <returns></returns>
    Private Function GetCost(breakerType As String, Optional costCode As String = "", Optional selectTab As String = "") As DataTable
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
        sqlStat.AppendLine("       , CLASS4 As CHARGECLASS4")
        sqlStat.AppendLine("       , CLASS8 As CHARGECLASS8")
        sqlStat.AppendLine("       , CLASS9 As CHARGECLASS9")
        sqlStat.AppendLine("  FROM GBM0010_CHARGECODE")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        Select Case breakerType
            Case "1"
                sqlStat.AppendLine("   And SALESBR     = '" & CONST_FLAG_YES & "'")
            Case "2"
                sqlStat.AppendLine("   And OPERATIONBR = '" & CONST_FLAG_YES & "'")
                sqlStat.AppendLine("   And CLASS2      = ''")
        End Select
        If costCode <> "" Then
            sqlStat.AppendLine("   And COSTCODE    = @COSTCODE")
        End If
        If selectTab = Me.tabExport1.ClientID OrElse selectTab = Me.tabExport2.ClientID Then
            sqlStat.AppendLine("   And LDKBN   IN  (" & "'B','L'" & ")")
        ElseIf selectTab = Me.tabInport1.ClientID OrElse selectTab = Me.tabInport2.ClientID Then
            sqlStat.AppendLine("   And LDKBN   IN  (" & "'B','D'" & ")")
        End If
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY COSTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                'SQLパラメータ値セット
                .Add("@COMPCODE", SqlDbType.NVarChar, 10).Value = HttpContext.Current.Session("APSRVCamp")
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
        sqlStat.AppendLine("SELECT CARRIERCODE AS CODE")
        sqlStat.AppendFormat("     , CARRIERCODE + ':' + {0} As LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0005_TRADER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If countryCode <> "" Then
            sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   AND CLASS       = '" & C_TRADER.CLASS.AGENT & "'")
        If carrierCode <> "" Then
            sqlStat.AppendLine("   And CARRIERCODE    = @CARRIERCODE")
        End If
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY CARRIERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 10).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@COUNTRYCODE", SqlDbType.NVarChar, 20).Value = countryCode
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
    ''' 対象のテキストボックス、ラベルに選択したエージェントを設定
    ''' </summary>
    ''' <param name="targetText">対象のテキストボックス</param>
    ''' <param name="carrierCode">左リストで選択したコード</param>
    Private Sub SetDisplayAgent(targetText As TextBox, carrierCode As String)
        Dim countryCode As String = ""
        Dim targetLabel As Label = Nothing
        Select Case targetText.ID
            Case Me.txtAgentPol1.ID
                countryCode = Me.txtLoadCountry1.Text
                targetLabel = Me.lblAgentPol1Text
            Case Me.txtAgentPod1.ID
                countryCode = Me.txtDischargeCountry1.Text
                targetLabel = Me.lblAgentPod1Text
            Case Me.txtAgentPol2.ID
                countryCode = Me.txtLoadCountry2.Text
                targetLabel = Me.lblAgentPol2Text
            Case Me.txtAgentPod2.ID
                countryCode = Me.txtDischargeCountry2.Text
                targetLabel = Me.lblAgentPod2Text
            Case Me.txtInvoiced.ID
                countryCode = ""
                targetLabel = Me.lblInvoicedText
        End Select
        targetText.Text = ""
        targetLabel.Text = ""
        Dim dt As DataTable = GetAgent(countryCode, carrierCode)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            targetText.Text = Convert.ToString(dr.Item("CODE"))
            targetLabel.Text = Convert.ToString(dr.Item("NAME"))
        End If
    End Sub
    ''' <summary>
    ''' 左リストTERMの選択肢作成
    ''' </summary>
    Private Sub SetTermListItem()
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Me.lbTerm.Items.Clear()
        'Term選択肢
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "TERM"
        COA0017FixValue.LISTBOX1 = Me.lbTerm
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbTerm = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            Throw New Exception("Fix value getError")
        End If

    End Sub
    ''' <summary>
    ''' 左リスト請求先の選択肢作成
    ''' </summary>
    Private Sub SetBillingCategoryListItem()
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Me.lbBillingCategory.Items.Clear()
        'Term選択肢
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DEMUACCT"
        COA0017FixValue.LISTBOX1 = Me.lbBillingCategory
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbBillingCategory = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            Throw New Exception("Fix value getError")
        End If

    End Sub
    ''' <summary>
    ''' 選択（入力）した業者コードに応じ業者名を画面に設定
    ''' </summary>
    ''' <param name="carrierCode">業者コード</param>
    Private Sub SetDisplayContractor(carrierCode As String)
        Dim tabObjects As New List(Of HtmlControl) From {Me.tabInport1, Me.tabInport2, Me.tabExport1, Me.tabExport2}
        Dim costGroup As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Export1
        For Each tabObject In tabObjects
            If tabObject.Attributes("class") IsNot Nothing AndAlso tabObject.Attributes("class").Contains("selected") Then
                Select Case tabObject.ID
                    Case Me.tabInport1.ID
                        costGroup = COSTITEM.CostItemGroup.Inport1
                    Case Me.tabInport2.ID
                        costGroup = COSTITEM.CostItemGroup.Inport2
                    Case Me.tabExport1.ID
                        costGroup = COSTITEM.CostItemGroup.Export1
                    Case Me.tabExport2.ID
                        costGroup = COSTITEM.CostItemGroup.Export2
                End Select
            End If

        Next
        '入力内容保持
        SaveGridItem(costGroup)

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))

        Dim uniqueIndex As Integer = 0
        Integer.TryParse(Me.hdnCurrentUnieuqIndex.Value, uniqueIndex)
        Dim changeCostList = (From allCostItem In allCostList
                              Where allCostItem.UniqueIndex = uniqueIndex).ToList

        If changeCostList IsNot Nothing AndAlso changeCostList.Count > 0 Then

            '一旦リセット
            changeCostList(0).ConstractorCode = carrierCode.Trim
            changeCostList(0).Constractor = ""
            Dim targetChargeClass4 As String = changeCostList(0).ChargeClass4
            Dim targetCountryCode As String = changeCostList(0).CountryCode
            Dim lbDummyObj As New ListBox
            Dim GBA00004CountryRelated As New GBA00004CountryRelated
            Select Case targetChargeClass4
                Case GBC_CHARGECLASS4.AGENT
                    GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                    GBA00004CountryRelated.LISTBOX_OFFICE = lbDummyObj
                    GBA00004CountryRelated.GBA00004getLeftListOffice()
                Case GBC_CHARGECLASS4.CURRIER
                    GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                    GBA00004CountryRelated.LISTBOX_VENDER = lbDummyObj
                    GBA00004CountryRelated.GBA00004getLeftListVender()
                Case GBC_CHARGECLASS4.FORWARDER
                    GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                    GBA00004CountryRelated.LISTBOX_FORWARDER = lbDummyObj
                    GBA00004CountryRelated.GBA00004getLeftListForwarder()
                Case GBC_CHARGECLASS4.DEPOT
                    GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                    GBA00004CountryRelated.LISTBOX_DEPOT = lbDummyObj
                    GBA00004CountryRelated.GBA00004getLeftListDepot()
                Case GBC_CHARGECLASS4.OTHER
                    GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                    GBA00004CountryRelated.LISTBOX_OTHER = lbDummyObj
                    GBA00004CountryRelated.GBA00004getLeftListOther()
            End Select

            Dim selectedContractorName As String = ""
            If lbDummyObj IsNot Nothing AndAlso lbDummyObj.Items.Count <> 0 AndAlso
               lbDummyObj.Items.FindByValue(carrierCode) IsNot Nothing Then
                Dim selItem = lbDummyObj.Items.FindByValue(carrierCode)
                If selItem.Text.Contains(":") Then
                    selectedContractorName = Split(selItem.Text, ":", 2)(1)
                Else
                    selectedContractorName = selItem.Text
                End If
                changeCostList(0).Constractor = selectedContractorName
            End If

            '選択されたベンダーを紐づく項目に設定
            For i As Integer = 0 To allCostList.Count - 1

                If allCostList.Item(i).ItemGroup = changeCostList(0).ItemGroup AndAlso
                    allCostList.Item(i).ChargeClass4 = changeCostList(0).ChargeClass4 Then

                    If allCostList.Item(i).ConstractorCode = "" Then
                        allCostList.Item(i).ConstractorCode = carrierCode.Trim
                        allCostList.Item(i).Constractor = changeCostList(0).Constractor
                    End If

                End If
            Next

        End If

        ViewState("COSTLIST") = allCostList
        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.ItemGroup = costGroup
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub
    ''' <summary>
    ''' テキストボックス変更時イベント
    ''' </summary>
    ''' <param name="targetTxtObj"></param>
    Private Sub ShowModifiedMessage(targetTxtObj As TextBox, portCode As String)
        '変更前の港コードを取得
        Dim prevPortCode As String = GetPrevPortCode(targetTxtObj)
        targetTxtObj.Text = prevPortCode
        '空白変更は行わせない
        If portCode = "" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.REQUIREDVALUE, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        'リストに存在しない港に変更させない
        Dim countryCode As String = ""
        If targetTxtObj.ID = "txtRecieptPort1" Then
            countryCode = Me.txtRecieptCountry1.Text
        End If
        Dim GBA00006PortRelated As New GBA00006PortRelated
        Dim chkDt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(countryCode, portCode)
        If chkDt Is Nothing OrElse chkDt.Rows.Count = 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.UNSELECTABLEERR, Me.lblFooterMessage, pageObject:=Me,
            messageParams:=New List(Of String) From {String.Format("VALUE:{0}", portCode)})
            Return
        End If
        '変更後の値及び変更対象コントロールIDをVIEWSTATEに退避
        ViewState(CONST_VS_CHANGE_PORTCODE) = portCode
        ViewState(CONST_VS_CHANGE_PORTTEXTID) = targetTxtObj.ID
        '変更確認メッセージを表示
        '確認メッセージ表示
        CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMPORTMODIFIED, Me, submitButtonId:="btnConfirmPortModifiedOk")
        hdnMsgId.Value = C_MESSAGENO.CONFIRMPORTMODIFIED
        Return
    End Sub
    ''' <summary>
    ''' 港コード変更時処理
    ''' </summary>
    Private Sub SetDisplayPort()
        Dim targetTxtObj As TextBox = DirectCast(Me.FindControl(Convert.ToString(ViewState(CONST_VS_CHANGE_PORTTEXTID))), TextBox)
        Dim portCode As String = Convert.ToString(ViewState(CONST_VS_CHANGE_PORTCODE))
        '変更対象の港に応じ連動項目を判定
        Dim wictchTrans As String = Right(targetTxtObj.ID, 1) '第一輸送OR第二輸送
        '発地・着地判定
        Dim portStr As String = "Discharge"
        Dim placeStr As String = "Delivery"
        Dim polPod As String = "Pod"
        If targetTxtObj.ID.Contains("RecieptPort") Then
            portStr = "Reciept"
            placeStr = "Load"
            polPod = "Pol"
        End If
        '発1は国コード縛り
        Dim countryCode As String = ""
        If targetTxtObj.ID.Equals("txtRecieptPort1") Then
            countryCode = Me.txtRecieptCountry1.Text
        End If
        '以下画面に確実に存在することが前提(各オブジェクトのIDを変更する場合は注意)
        Dim txtCountryObjcts As New List(Of TextBox) From {DirectCast(Me.FindControl(String.Format("txt{0}Country{1}", portStr, wictchTrans)), TextBox),
                                                           DirectCast(Me.FindControl(String.Format("txt{0}Country{1}", placeStr, wictchTrans)), TextBox)}


        Dim lblPortNameObjects As New List(Of Label) From {DirectCast(Me.FindControl(String.Format("lbl{0}Port{1}Text", portStr, wictchTrans)), Label),
                                                           DirectCast(Me.FindControl(String.Format("lbl{0}Port{1}Text", placeStr, wictchTrans)), Label)}

        Dim txtPlaceObj As TextBox = DirectCast(Me.FindControl(String.Format("txt{0}Port{1}", placeStr, wictchTrans)), TextBox)

        Dim txtAgentObj As TextBox = DirectCast(Me.FindControl(String.Format("txtAgent{0}{1}", polPod, wictchTrans)), TextBox)
        Dim lblAgentObj As Label = DirectCast(Me.FindControl(String.Format("lblAgent{0}{1}Text", polPod, wictchTrans)), Label)


        '一旦初期化
        targetTxtObj.Text = portCode.Trim
        txtCountryObjcts(0).Text = countryCode
        txtCountryObjcts(1).Text = ""
        lblPortNameObjects(0).Text = ""
        lblPortNameObjects(1).Text = ""
        txtPlaceObj.Text = ""
        txtAgentObj.Text = ""
        lblAgentObj.Text = ""
        '港未入力または対象が無い場合は終了
        If portCode.Trim = "" Then
            Return
        End If
        Dim GBA00006PortRelated As GBA00006PortRelated = New GBA00006PortRelated
        Dim dtPort As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(countryCode, portCode:=portCode.Trim)
        If dtPort Is Nothing OrElse dtPort.Rows.Count = 0 Then
            Return
        End If
        Dim drPort As DataRow = dtPort.Rows(0)
        txtPlaceObj.Text = portCode.Trim
        '港に紐づく国設定
        txtCountryObjcts(0).Text = Convert.ToString(drPort("COUNTRYCODE"))
        txtCountryObjcts(1).Text = Convert.ToString(drPort("COUNTRYCODE"))
        '港名設定
        lblPortNameObjects(0).Text = HttpUtility.HtmlEncode(Convert.ToString(drPort("NAME")))
        lblPortNameObjects(1).Text = HttpUtility.HtmlEncode(Convert.ToString(drPort("NAME")))
        'Agent取得
        Dim dummyList As New ListBox
        GBA00006PortRelated.PORTCODE = Convert.ToString(portCode.Trim)
        GBA00006PortRelated.LISTBOX_OFFICE = dummyList
        GBA00006PortRelated.GBA00006getLeftListOffice()
        If GBA00006PortRelated.OfficeKeyValue IsNot Nothing Then
            Dim agentItem = GBA00006PortRelated.OfficeKeyValue.First
            txtAgentObj.Text = agentItem.Key
            lblAgentObj.Text = agentItem.Value
        End If
    End Sub
    ''' <summary>
    ''' 右ボックスのコメント欄制御
    ''' </summary>
    ''' <param name="isOpen"></param>
    Private Sub DisplayCostRemarks(isOpen As Boolean)
        Dim tabObjects As New List(Of HtmlControl) From {Me.tabInport1, Me.tabInport2, Me.tabExport1, Me.tabExport2}
        Dim costGroup As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Export1
        For Each tabObject In tabObjects
            If tabObject.Attributes("class") IsNot Nothing AndAlso tabObject.Attributes("class").Contains("selected") Then
                Select Case tabObject.ID
                    Case Me.tabInport1.ID
                        costGroup = COSTITEM.CostItemGroup.Inport1
                    Case Me.tabInport2.ID
                        costGroup = COSTITEM.CostItemGroup.Inport2
                    Case Me.tabExport1.ID
                        costGroup = COSTITEM.CostItemGroup.Export1
                    Case Me.tabExport2.ID
                        costGroup = COSTITEM.CostItemGroup.Export2
                End Select
            End If

        Next
        '入力内容保持
        SaveGridItem(costGroup)

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
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
        ViewState("COSTLIST") = allCostList
        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.ItemGroup = costGroup
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '費用項目非活性制御
        CostEnabledControls()
    End Sub

    ''' <summary>
    ''' 左の出力帳票
    ''' </summary>
    ''' <param name="isOwner">オーナータブ</param>
    ''' <param name="currentTab">現在アクティブのタブ</param>
    Private Function RightboxInit(isOwner As Boolean, currentTab As COSTITEM.CostItemGroup) As String
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim excelMapId As String = "GBT00001_C"
        '選択タブによりMAPID特定
        If isOwner = True Then
            excelMapId = "GBT00001_O"
        End If

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
        Dim dr As DataRow = retDt.Rows(0)
        dr.Item("ISTRILATERAL") = Me.hdnIsTrilateral.Value
        dr.Item("BRID") = Me.lblBrNo.Text
        dr.Item("USETYPE") = Me.txtBrType.Text
        dr.Item("BRTYPE") = Me.hdnBrType.Value
        Dim stYmd As Date = Nothing
        If Date.TryParseExact(Me.txtBrStYmd.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, stYmd) Then
            dr.Item("VALIDITYFROM") = stYmd
        Else
            dr.Item("VALIDITYFROM") = Me.txtBrStYmd.Text
        End If
        Dim endYmd As Date = Nothing
        If Date.TryParseExact(Me.txtBrEndYmd.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, endYmd) Then
            dr.Item("VALIDITYTO") = endYmd
        Else
            dr.Item("VALIDITYTO") = Me.txtBrEndYmd.Text
        End If

        If Me.chkDisabled.Checked = True Then
            dr.Item("DISABLED") = CONST_FLAG_YES
        Else
            dr.Item("DISABLED") = CONST_FLAG_NO
        End If
        dr.Item("ORIGINALCOPYBRID") = Me.hdnOriginalCopyBrid.Value

        dr.Item("TERMTYPE") = Me.txtBrTerm.Text
        dr.Item("NOOFTANKS") = Me.txtNoOfTanks.Text
        'このあたりに承認情報
        dr.Item("SHIPPER") = Me.txtShipper.Text
        dr.Item("CONSIGNEE") = Me.txtConsignee.Text
        dr.Item("CARRIER1") = Me.txtCarrier1.Text
        dr.Item("CARRIER2") = Me.txtCarrier2.Text
        dr.Item("PRODUCTCODE") = Me.txtProduct.Text
        dr.Item("IMDGCODE") = Me.txtImdg.Text
        dr.Item("UNNO") = Me.txtUNNo.Text
        dr.Item("RECIEPTCOUNTRY1") = Me.txtRecieptCountry1.Text
        dr.Item("RECIEPTPORT1") = Me.txtRecieptPort1.Text
        dr.Item("LOADCOUNTRY1") = Me.txtLoadCountry1.Text
        dr.Item("LOADPORT1") = Me.txtLoadPort1.Text

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

        dr.Item("DISCHARGECOUNTRY1") = Me.txtDischargeCountry1.Text
        dr.Item("DISCHARGEPORT1") = Me.txtDischargePort1.Text
        dr.Item("DELIVERYCOUNTRY1") = Me.txtDeliveryCountry1.Text
        dr.Item("DELIVERYPORT1") = Me.txtDeliveryPort1.Text

        dr.Item("RECIEPTCOUNTRY2") = Me.txtRecieptCountry2.Text
        dr.Item("RECIEPTPORT2") = Me.txtRecieptPort2.Text
        dr.Item("LOADCOUNTRY2") = Me.txtLoadCountry2.Text
        dr.Item("LOADPORT2") = Me.txtLoadPort2.Text

        dr.Item("DISCHARGECOUNTRY2") = Me.txtDischargeCountry2.Text
        dr.Item("DISCHARGEPORT2") = Me.txtDischargePort2.Text
        dr.Item("DELIVERYCOUNTRY2") = Me.txtDeliveryCountry2.Text
        dr.Item("DELIVERYPORT2") = Me.txtDeliveryPort2.Text

        dr.Item("VSL1") = Me.txtVsl1.Text
        dr.Item("VOY1") = Me.txtVoy1.Text

        Dim etd1 As Date = Nothing
        If Date.TryParseExact(Me.txtEtd1.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etd1) Then
            dr.Item("ETD1") = etd1
        Else
            dr.Item("ETD1") = Me.txtEtd1.Text
        End If

        Dim eta1 As Date = Nothing
        If Date.TryParseExact(Me.txtEta1.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, eta1) Then
            dr.Item("ETA1") = eta1
        Else
            dr.Item("ETA1") = Me.txtEta1.Text
        End If

        dr.Item("VSL2") = Me.txtVsl2.Text
        dr.Item("VOY2") = Me.txtVoy2.Text

        Dim etd2 As Date = Nothing
        If Date.TryParseExact(Me.txtEtd2.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etd2) Then
            dr.Item("ETD2") = etd2
        Else
            dr.Item("ETD2") = Me.txtEtd2.Text
        End If

        Dim eta2 As Date = Nothing
        If Date.TryParseExact(Me.txtEta2.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, eta2) Then
            dr.Item("ETA2") = eta2
        Else
            dr.Item("ETA2") = Me.txtEta2.Text
        End If

        dr.Item("INVOICEDBY") = Me.txtInvoiced.Text
        dr.Item("FEE") = Me.txtFee.Text
        dr.Item("BILLINGCATEGORY") = Me.txtBillingCategory.Text

        If Me.txtWeight.Text <> "" Then
            Dim productWeight As String = Me.txtWeight.Text.Replace(",", "")
            Dim productWeightNum As Decimal = 0
            If Decimal.TryParse(productWeight, productWeightNum) Then
                dr.Item("PRODUCTWEIGHT") = productWeightNum
            End If
        Else
            dr.Item("PRODUCTWEIGHT") = ""
        End If

        dr.Item("GRAVITY") = Me.txtSGravity.Text

        If Me.txtTankCapacity.Text <> "" Then
            Dim capacity As String = Me.txtTankCapacity.Text.Replace(",", "")
            Dim capacityNum As Decimal = 0
            If Decimal.TryParse(capacity, capacityNum) Then
                dr.Item("CAPACITY") = capacityNum
            End If
        Else
            dr.Item("CAPACITY") = ""
        End If

        dr.Item("LOADING") = Me.txtLoading.Text
        dr.Item("STEAMING") = Me.txtSteaming.Text
        dr.Item("TIP") = Me.txtTip.Text
        dr.Item("EXTRA") = Me.txtExtra.Text
        dr.Item("JOTHIREAGE") = Me.txtJOTHireage.Text
        dr.Item("COMMERCIALFACTOR") = Me.txtCommercialFactor.Text
        dr.Item("AMTREQUEST") = Me.txtAmtRequest.Text
        dr.Item("AMTPRINCIPAL") = Me.txtAmtPrincipal.Text
        dr.Item("AMTDISCOUNT") = Me.txtAmtDiscount.Text
        dr.Item("DEMURTO") = Me.txtDemurdayT1.Text
        dr.Item("DEMURUSRATE1") = Me.txtDemurUSRate1.Text
        dr.Item("DEMURUSRATE2") = Me.txtDemurUSRate2.Text
        dr.Item("REMARK") = HttpUtility.HtmlDecode(Me.lblBrRemarkText.Text)
        dr.Item("APPLYTEXT") = HttpUtility.HtmlDecode(Me.lblApplyRemarks.Text)
        dr.Item("APPROVEDTEXT") = HttpUtility.HtmlDecode(Me.lblAppJotRemarks.Text)
        dr.Item("PERDAY") = Me.txtPerDay.Text
        dr.Item("TOTALINVOICED") = Me.txtInvoicedTotal.Text
        'オーガナイザ国
        dr.Item("COUNTRYORGANIZER") = Me.hdnCountryOrganizer.Value
        'エージェント関係
        dr.Item("AGENTORGANIZER") = Me.hdnAgentOrganizer.Value  ' GBA00003UserSetting.OFFICECODE
        dr.Item("AGENTPOL1") = Me.txtAgentPol1.Text
        dr.Item("AGENTPOL2") = Me.txtAgentPol2.Text
        dr.Item("AGENTPOD1") = Me.txtAgentPod1.Text
        dr.Item("AGENTPOD2") = Me.txtAgentPod2.Text

        '出力用名称
        dr.Item("BRTYPENAME") = Me.lblBrTypeText.Text
        dr.Item("BRTERMNAME") = Me.lblBrTermText.Text
        dr.Item("APPSALESPICNAME") = Me.lblAppSalesPicText.Text
        dr.Item("INVOICEDNAME") = Me.lblInvoicedText.Text
        dr.Item("SHIPPERNAME") = Me.lblShipperText.Text
        dr.Item("CONSIGNEENAME") = Me.lblConsigneeText.Text
        dr.Item("CARRIER1NAME") = Me.lblCarrier1Text.Text
        dr.Item("CARRIER2NAME") = Me.lblCarrier2Text.Text
        dr.Item("PRODUCTNAME") = Me.lblProductText.Text
        dr.Item("RECIEPTPORT1NAME") = Me.lblRecieptPort1Text.Text
        dr.Item("LOADPORT1NAME") = Me.lblLoadPort1Text.Text
        dr.Item("DISCHARGEPORT1NAME") = Me.lblDischargePort1Text.Text
        dr.Item("DELIVERYPORT1NAME") = Me.lblDeliveryPort1Text.Text
        dr.Item("RECIEPTPORT2NAME") = Me.lblRecieptPort2Text.Text
        dr.Item("LOADPORT2NAME") = Me.lblLoadPort2Text.Text
        dr.Item("DISCHARGEPORT2NAME") = Me.lblDischargePort2Text.Text
        dr.Item("DELIVERYPORT2NAME") = Me.lblDeliveryPort2Text.Text
        dr.Item("SPECIALINSTRUCTIONS") = Me.lblRemarks.Text

        dr.Item("INITUSERNAME") = Me.hdnInitUserName.Value

        Dim agentDt As DataTable = Me.GetAgent(Me.hdnCountryOrganizer.Value, Me.hdnAgentOrganizer.Value)
        If agentDt IsNot Nothing AndAlso agentDt.Rows.Count > 0 Then
            dr.Item("AGENTNAME") = Convert.ToString(agentDt.Rows(0).Item("NAME"))
        End If
        '↓JPY換算項目の設定
        SetHireageJpy()
        dr("TOTALCOST_JPY") = Me.txtTotalCostJPY.Text
        dr("PERDAY_JPY") = Me.txtPerDayJPY.Text
        dr("TOTALINVOICED_JPY") = Me.txtInvoicedTotalJPY.Text
        dr("JOTHIREAGE_JPY") = Me.txtJOTHireageJPY.Text
        dr("COMMERCIALFACTOR_JPY") = Me.txtCommercialFactorJPY.Text
        dr("AMTREQUEST_JPY") = Me.txtAmtRequestJPY.Text
        dr("AMTPRINCIPAL_JPY") = Me.txtAmtPrincipalJPY.Text
        dr("AMTDISCOUNT_JPY") = Me.txtAmtDiscountJPY.Text
        dr("JPY_RATE") = Convert.ToString(ViewState("JPYEXR"))
        '↑JPY換算項目の設定
        Return retDt
    End Function
    ''' <summary>
    ''' 入力チェック、データ登録、Excel出力時に使用するため画面情報をデータテーブルに格納
    ''' </summary>
    ''' <param name="currentTab"></param>
    ''' <returns></returns>
    Private Function CollectDisplayCostInfo(Optional currentTab As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Organizer) As DataTable
        Dim retDt As DataTable = CreateCostInfoTable()

        Dim targetCostData As List(Of COSTITEM) = Nothing
        Dim beforeCostItemGroup As COSTITEM.CostItemGroup
        With Nothing '暫定
            Dim current As String = ""
            For Each tabObject In {Me.tabExport1, Me.tabInport1, Me.tabExport2, Me.tabInport2, Me.tabOrganizer}
                If tabObject.Attributes("class") IsNot Nothing AndAlso tabObject.Attributes("class").Contains("selected") Then
                    current = tabObject.ID
                End If

            Next
            beforeCostItemGroup = COSTITEM.CostItemGroup.Organizer
            Select Case current
                Case Me.tabExport1.ClientID
                    beforeCostItemGroup = COSTITEM.CostItemGroup.Export1
                Case Me.tabInport1.ClientID
                    beforeCostItemGroup = COSTITEM.CostItemGroup.Inport1
                Case Me.tabExport2.ClientID
                    beforeCostItemGroup = COSTITEM.CostItemGroup.Export2
                Case Me.tabInport2.ClientID
                    beforeCostItemGroup = COSTITEM.CostItemGroup.Inport2
            End Select
            If beforeCostItemGroup <> COSTITEM.CostItemGroup.Organizer Then
                SaveGridItem(beforeCostItemGroup)
            End If

        End With
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        If currentTab = COSTITEM.CostItemGroup.Organizer Then
            targetCostData = costData 'TODOステータスに応じ動きを変える
        Else
            targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab).ToList
        End If
        For Each costItem In targetCostData
            Dim dtlPolPod As String = ""
            Dim agent As String = ""
            Dim countryCode As String = ""
            Select Case costItem.ItemGroup
                Case COSTITEM.CostItemGroup.Export1
                    dtlPolPod = "POL1"
                    agent = Me.txtAgentPol1.Text.Trim
                    countryCode = Me.txtRecieptCountry1.Text
                Case COSTITEM.CostItemGroup.Export2
                    dtlPolPod = "POL2"
                    agent = Me.txtAgentPol2.Text.Trim
                    countryCode = Me.txtRecieptCountry2.Text
                Case COSTITEM.CostItemGroup.Inport1
                    dtlPolPod = "POD1"
                    agent = Me.txtAgentPod1.Text.Trim
                    countryCode = Me.txtDischargeCountry1.Text
                Case COSTITEM.CostItemGroup.Inport2
                    dtlPolPod = "POD2"
                    agent = Me.txtAgentPod2.Text.Trim
                    countryCode = Me.txtDischargeCountry2.Text
            End Select

            Dim ctDt As DataTable = GetCountry(countryCode.Trim)
            Dim localCurrency As String = ""
            Dim currency = ctDt.Rows(0).Item("CURRENCYCODE")
            localCurrency = currency.ToString
            If costItem.Local = "" Then
                costItem.Local = "0"
            End If

            If IsNumeric(costItem.Local) AndAlso Convert.ToDouble(costItem.Local) = 0 Then
                currency = GBC_CUR_USD
            End If

            Dim dr As DataRow = retDt.NewRow
            dr.Item("DTLPOLPOD") = dtlPolPod
            dr.Item("COSTCODE") = costItem.CostCode
            dr.Item("COSTNAME") = costItem.CostName
            dr.Item("BASEON") = costItem.BasedOn
            'dr.Item("TAX") = costItem.Tax
            dr.Item("USD") = costItem.USD
            dr.Item("LOCAL") = costItem.Local
            dr.Item("LOCALRATE") = costItem.LocalCurrncyRate
            dr.Item("CONTRACTOR") = costItem.ConstractorCode
            dr.Item("REMARK") = costItem.Remarks
            dr.Item("CHARGECLASS4") = costItem.ChargeClass4
            dr.Item("CHARGECLASS8") = costItem.ChargeClass8
            dr.Item("AGENT") = agent
            dr.Item("ACTIONID") = costItem.ActionId
            dr.Item("CLASS1") = costItem.Class1
            dr.Item("CLASS2") = costItem.Class2
            dr.Item("CLASS3") = costItem.Class3
            dr.Item("CLASS4") = costItem.Class4
            dr.Item("CLASS5") = costItem.Class5
            dr.Item("CLASS6") = costItem.Class6
            dr.Item("CLASS7") = costItem.Class7
            dr.Item("CLASS8") = costItem.Class8
            dr.Item("CLASS9") = costItem.Class9
            dr.Item("TAXATION") = costItem.Taxation
            If costItem.InvoicedBy = "1" Then
                dr.Item("CINVOICEDBY") = C_JOT_AGENT
            Else
                dr.Item("CINVOICEDBY") = agent
            End If
            dr.Item("COUNTRYCODE") = countryCode
            dr.Item("CURRENCYCODE") = currency
            dr.Item("LOCALCURRENCY") = localCurrency
            dr.Item("BILLING") = costItem.Billing

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
        Dim errMessage As String = ""
        Dim mapId As String = ""
        Dim hasError As Boolean = False
        If isMinCheck Then
            mapId = "GBT00001SALESTEMP"
        Else
            mapId = "GBT00001SALES"
        End If

        '表示しているタブに応じ港のチェックを行う追加フィールドを加える
        Dim additionalFields As New List(Of String)
        If Me.tabExport1.Visible Then
            additionalFields.Add("RECIEPTPORT1")
            additionalFields.Add("AGENTPOL1")
        End If
        If Me.tabInport1.Visible Then
            additionalFields.Add("DISCHARGEPORT1")
            additionalFields.Add("AGENTPOD1")
        End If
        If Me.tabExport2.Visible Then
            additionalFields.Add("RECIEPTPORT2")
            additionalFields.Add("AGENTPOL2")
        End If
        If Me.tabInport2.Visible Then
            additionalFields.Add("DISCHARGEPORT2")
            additionalFields.Add("AGENTPOD2")
        End If

        Dim fieldList As New List(Of String) From {"VALIDITYFROM", "VALIDITYTO", "TERMTYPE",
                                                   "NOOFTANKS", "SHIPPER", "CONSIGNEE", "CARRIER1",
                                                   "CARRIER2", "PRODUCTCODE", "RECIEPTCOUNTRY1",
                                                    "LOADCOUNTRY1", "LOADPORT1", "DISCHARGECOUNTRY1",
                                                    "DELIVERYCOUNTRY1", "DELIVERYPORT1", "RECIEPTCOUNTRY2",
                                                    "LOADCOUNTRY2", "LOADPORT2", "DISCHARGECOUNTRY2",
                                                    "DELIVERYPORT2", "VSL1", "VOY1", "ETD1", "ETA1",
                                                   "VSL2", "VOY2", "ETD2", "ETA2", "INVOICEDBY", "PRODUCTWEIGHT", "CAPACITY",
                                                   "LOADING", "STEAMING", "TIP", "EXTRA", "JOTHIREAGE", "COMMERCIALFACTOR",
                                                   "AMTREQUEST", "AMTPRINCIPAL", "AMTDISCOUNT", "DEMURTO",
                                                   "DEMURUSRATE1", "DEMURUSRATE2", "REMARK",
                                                   "FEE", "BILLINGCATEGORY"}
        fieldList.AddRange(additionalFields)
        If CheckSingle(mapId, ownerDt, fieldList, errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If
        'Dim listFieldCheckObjects As New List(Of String) From {""}
        If Me.hdnBrType.Value = "1" Then
            'SALESの場合、必須
            If Me.txtProduct.Text = "" Then

                errMessage = "・PRODUCTCODE：  Required value not inputted"
                rightBoxMessage.AppendLine(errMessage)
                hasError = True
            End If

        End If

        '請求先がConsigneeの場合、必須
        If Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.CONSIGNEE Then
            If Me.txtConsignee.Text = "" Then

                errMessage = "・CONSIGNEE：  Required value not inputted"
                rightBoxMessage.AppendLine(errMessage)
                hasError = True

            End If
        End If



        'オーガナイザー部リスト存在チェック
        fieldList = New List(Of String) From {"TERMTYPE", "INVOICEDBY", "CONSIGNEE", "CARRIER1", "CARRIER2",
                                              "AGENTPOL1", "AGENTPOL2", "AGENTPOD1", "AGENTPOD2", "PRODUCTCODE", "BILLINGCATEGORY"}
        fieldList.AddRange(additionalFields)
        If CheckListData(ownerDt, fieldList, errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If
        fieldList = New List(Of String) From {"COSTCODE", "COSTNAME", "BASEON", "TAX", "USD", "LOCAL", "USDRATE", "LOCALRATE", "REMARK"}
        Dim costMapId As String = "GBT00001SALESCOST"
        Dim keyFields As New List(Of String) From {"COSTCODE"}
        If CheckSingle(costMapId, costDt, fieldList, errMessage, keyFields) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If
        '港変更チェック確認
        Dim modPorts = GetModifiedPort()
        Dim dtTargetCheckCost As DataTable = costDt
        If modPorts.Count > 0 Then
            dtTargetCheckCost = costDt.Clone
            '港変更の場合は初期状態に書き換える為チェック対象から外す
            Dim qTargetCheckCost = From costItem In costDt Where Not modPorts.Contains(Convert.ToString(costItem("DTLPOLPOD")))
            If qTargetCheckCost.Any Then
                dtTargetCheckCost = CommonFunctions.DeepCopy(qTargetCheckCost.CopyToDataTable)
            End If
        End If
        '費用部リスト存在チェック
        fieldList = New List(Of String) From {"CONTRACTOR"}
        If CheckListData(dtTargetCheckCost, fieldList, errMessage) <> C_MESSAGENO.NORMAL Then
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
        'Dim hasError As Boolean = False
        Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
        Dim retMessage As New StringBuilder
        'エラーメッセージ取得すら失敗した場合
        Dim getMessageErrorString As String = "エラーメッセージ({0})の取得に失敗しました。"
        If BASEDLL.COA0019Session.LANGDISP <> C_LANG.JA Then
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
                    CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, dummyLabelObj, naeiw:=C_NAEIW.ERROR)

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
    ''' リストチェック
    ''' </summary>
    ''' <param name="dt">IN:チェック対象のデータテーブル</param>
    ''' <param name="checkFileds">IN:チェックフィールド一覧</param>
    ''' <param name="errMessage">OUT：エラーメッセージ</param>
    ''' <param name="keyFields">IN(省略可):エラーメッセージ表示時に示すキーフィールドリスト、ここを指定した場合は「エラー内容」＋「当引数のフィールドと値」をメッセージに付与します
    ''' 省略時は付与しません</param>
    ''' <param name="keyValuePadLen">IN(省略可 省略時20):「--> [項目名] = [値]」を表示する際の項目名から=までにスペースを埋めるバイト数</param>
    ''' <returns>メッセージ番号:すべて正常時はC_MESSAGENO.NORMAL(00000) チェック異常時はC_MESSAGENO.RIGHTBIXOUT(10008)を返却</returns>
    Private Function CheckListData(ByVal dt As DataTable, ByVal checkFileds As List(Of String), ByRef errMessage As String, Optional keyFields As List(Of String) = Nothing, Optional keyValuePadLen As Integer = 20) As String
        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim hasError As Boolean = False
        Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
        Dim retMessage As New StringBuilder


        'エラーメッセージ取得すら失敗した場合
        Dim getMessageErrorString As String = "エラーメッセージ({0})の取得に失敗しました。"
        If BASEDLL.COA0019Session.LANGDISP <> C_LANG.JA Then
            getMessageErrorString = "Failed To Get Error message ({0})."
        End If

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
        Dim fieldDic As New Dictionary(Of String, String) _
                        From {{"TERMTYPE", "TERM"}, {"INVOICEDBY", "INVOICED BY"},
                              {"CONSIGNEE", "CONSIGNEE"}, {"CARRIER1", "CARRIER1"}, {"CARRIER2", "CARRIER2"},
                              {"AGENTPOL1", "Export1 AGENT"}, {"AGENTPOL2", "Export2 AGENT"}, {"AGENTPOD1", "Import1 AGENT"}, {"AGENTPOD2", "Import2 AGENT"},
                              {"PRODUCTCODE", "PRODUCTCODE"}, {"COSTCODE", "COSTCODE"}, {"CONTRACTOR", "VENDER"}, {"BILLINGCATEGORY", "BILLING"},
                              {"RECIEPTPORT1", "Export1 PORT"}, {"DISCHARGEPORT1", "Import1 PORT"}, {"RECIEPTPORT2", "Export2 PORT"}, {"DISCHARGEPORT2", "Import2 PORT"}}
        'データテーブルの行ループ開始
        Dim chkDt As DataTable = Nothing
        Dim ddlToTable As New DataTable
        ddlToTable.Columns.Add(New DataColumn("KEY", GetType(String)))
        ddlToTable.Columns.Add(New DataColumn("VALUE", GetType(String)))
        Dim chkfieldName As String = ""
        Dim GBA00004CountryRelated As New GBA00004CountryRelated
        For Each dr As DataRow In dt.Rows
            'チェックフィールドのループ開始
            For Each checkField In targetCheckFields
                Dim chkVal As String = Convert.ToString(dr.Item(checkField))
                If chkVal = "" Then
                    Continue For 'ブランクの場合はスキップ
                End If
                chkDt = Nothing
                Select Case checkField
                    Case "TERMTYPE"
                        SetTermListItem()
                        chkDt = ddlToTable.Clone()
                        Me.lbTerm.Items.Cast(Of ListItem).ToList _
                        .ForEach(Function(item) chkDt.Rows.Add({item.Value, item.Text}))
                        chkfieldName = "KEY"
                    Case "CONSIGNEE"
                        Dim countryCode As String = Me.txtDeliveryCountry1.Text
                        If Me.hdnBrType.Value = "1" Then
                            chkDt = GetConsignee(countryCode)
                            chkfieldName = "CUSTOMERCODE"
                        Else
                            chkDt = GetAgent(countryCode)
                            chkfieldName = "CODE"
                        End If
                    Case "CARRIER1", "CARRIER2"
                        Dim countryCode As String = Me.txtLoadCountry1.Text
                        If checkField = "CARRIER2" Then
                            countryCode = Me.txtLoadCountry2.Text
                        End If
                        chkDt = GetCarrier(countryCode)
                        chkfieldName = "CODE"
                    Case "INVOICEDBY", "AGENTPOL1", "AGENTPOL2", "AGENTPOD1", "AGENTPOD2"
                        Dim countryCode As String = ""
                        Select Case checkField
                            Case "AGENTPOL1"
                                countryCode = Me.txtLoadCountry1.Text
                            Case "AGENTPOL2"
                                countryCode = Me.txtLoadCountry2.Text
                            Case "AGENTPOD1"
                                countryCode = Me.txtDischargeCountry1.Text
                            Case "AGENTPOD2"
                                countryCode = Me.txtDischargeCountry2.Text
                        End Select
                        chkDt = GetAgent(countryCode)
                        chkfieldName = "CODE"
                    Case "PRODUCTCODE"
                        chkDt = GetProduct()
                        chkfieldName = "CODE"
                    Case "COSTCODE"
                        Dim selectedTabId As String = Me.tabExport1.ClientID
                        If Convert.ToString(dr("DTLPOLPOD")).StartsWith("POD") Then
                            selectedTabId = Me.tabInport1.ClientID
                        End If
                        chkDt = GetCost(Me.hdnBrType.Value, selectTab:=selectedTabId)
                        chkfieldName = "CODE"
                    Case "CONTRACTOR"
                        Dim chargeClass4 = Convert.ToString(dr("CHARGECLASS4"))
                        Dim countryCode As String = Convert.ToString(dr("COUNTRYCODE"))
                        GBA00004CountryRelated.COUNTRYCODE = countryCode
                        chkDt = ddlToTable.Clone()
                        Dim lbDummy As New ListBox

                        Select Case chargeClass4
                            Case GBC_CHARGECLASS4.AGENT
                                GBA00004CountryRelated.LISTBOX_OFFICE = lbDummy
                                GBA00004CountryRelated.GBA00004getLeftListOffice()
                            Case GBC_CHARGECLASS4.CURRIER
                                GBA00004CountryRelated.LISTBOX_VENDER = lbDummy
                                GBA00004CountryRelated.GBA00004getLeftListVender()
                            Case GBC_CHARGECLASS4.FORWARDER
                                GBA00004CountryRelated.LISTBOX_FORWARDER = lbDummy
                                GBA00004CountryRelated.GBA00004getLeftListForwarder()
                            Case GBC_CHARGECLASS4.DEPOT
                                GBA00004CountryRelated.LISTBOX_DEPOT = lbDummy
                                GBA00004CountryRelated.GBA00004getLeftListDepot()
                            Case GBC_CHARGECLASS4.OTHER
                                GBA00004CountryRelated.LISTBOX_OTHER = lbDummy
                                GBA00004CountryRelated.GBA00004getLeftListOther()
                        End Select
                        lbDummy.Items.Cast(Of ListItem).ToList _
                        .ForEach(Function(item) chkDt.Rows.Add({item.Value, item.Text}))
                        chkfieldName = "KEY"
                    Case "BILLINGCATEGORY"
                        SetBillingCategoryListItem()
                        chkDt = ddlToTable.Clone()
                        Me.lbBillingCategory.Items.Cast(Of ListItem).ToList _
                        .ForEach(Function(item) chkDt.Rows.Add({item.Value, item.Text}))
                        chkfieldName = "KEY"
                    Case "RECIEPTPORT1", "DISCHARGEPORT1", "RECIEPTPORT2", "DISCHARGEPORT2"
                        Dim portCode As String = ""
                        Dim countryCode As String = ""
                        Select Case checkField
                            Case "RECIEPTPORT1"
                                portCode = Me.txtRecieptPort1.Text
                                countryCode = Me.txtRecieptCountry1.Text
                            Case "RECIEPTPORT2"
                                portCode = Me.txtRecieptPort2.Text
                            Case "DISCHARGEPORT1"
                                portCode = Me.txtDischargePort1.Text
                            Case "DISCHARGEPORT2"
                                portCode = Me.txtDischargePort2.Text
                        End Select
                        chkDt = GBA00006PortRelated.GBA00006getPortCodeValue(countryCode, portCode)
                        chkfieldName = "PORTCODE"
                End Select

                Dim result = From item In chkDt Where Convert.ToString(item(chkfieldName)).Equals(chkVal)
                If result.Any = False Then
                    hasError = True
                    retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                    CommonFunctions.ShowMessage(C_MESSAGENO.UNSELECTABLEERR, dummyLabelObj, naeiw:=C_NAEIW.ERROR,
                        messageParams:=New List(Of String) From {String.Format("VALUE:{0}", chkVal)})

                    retMessage.AppendFormat("・{0}：{1}", fieldDic(checkField), dummyLabelObj.Text).AppendLine()

                    If targetKeyFields IsNot Nothing Then
                        For Each keyField In targetKeyFields
                            retMessage.AppendFormat("--> {0} = {1}", padRight(fieldDic(keyField), keyValuePadLen), Convert.ToString(dr.Item(keyField))).AppendLine()
                        Next
                    End If 'END targetKeyFields IsNot Nothing 

                End If
            Next
        Next
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
    Private Sub EntryData(ds As DataSet, currentTab As COSTITEM.CostItemGroup, Optional isEntryAllTabs As Boolean = False, Optional isSendConfirm As Boolean = False, Optional isModifiedPort As Boolean = False, Optional callerButton As String = "")
        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        Dim newEntry As Boolean = True
        Dim breakerId As String = ""
        Dim ownerDt As DataTable = ds.Tables("ORGANIZER_INFO")
        Dim costDt As DataTable = ds.Tables("COST_INFO")
        hdnMsgId.Value = C_MESSAGENO.NORMALENTRY
        brInfo = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))
        If brInfo("INFO").BrId <> "" Then
            newEntry = False
        End If

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            '新規ブレーカーまたは港変更の場合ブレーカー番号を取得し紐づけデータの作成
            If newEntry = True OrElse isModifiedPort = True Then
                '新規ブレーカーNo作成
                breakerId = GetNewBreakerNo(sqlCon)
                'ブレーカー紐づけ情報作成
                'brInfo = SetBreakerInfo(breakerId, ownerDt)
                Dim orgBrId As String = ""
                If isModifiedPort Then
                    orgBrId = brInfo("INFO").BrId
                End If
                brInfo("INFO").BrId = breakerId
                'DB登録処理実行
                EntryNewBreaker(brInfo, ownerDt, costDt, sqlCon, orgBrId:=orgBrId, isModifiedPort:=isModifiedPort)
                Return
            End If
            '港変更時（既存データを破棄し新たなブレーカーを登録）
            'TODO:処理
            If newEntry = False Then
                '更新可能チェック(タイムスタンプ比較）
                Dim saveTab As String = ""
                '保存タブ判定
                If Me.hdnSelectedTabId.Value = Me.tabOrganizer.ClientID Then
                    'オーガナイザー
                    saveTab = "INFO"
                ElseIf Me.hdnSelectedTabId.Value = Me.tabExport1.ClientID Then
                    'POL
                    saveTab = "POL1"
                ElseIf Me.hdnSelectedTabId.Value = Me.tabExport2.ClientID Then
                    'POL
                    saveTab = "POL2"
                ElseIf Me.hdnSelectedTabId.Value = Me.tabInport1.ClientID Then
                    'POD
                    saveTab = "POD1"
                ElseIf Me.hdnSelectedTabId.Value = Me.tabInport2.ClientID Then
                    'POD
                    saveTab = "POD2"
                End If
                If CanBreakerUpdate(brInfo, sqlCon, saveTab, callerButton:=callerButton) = False Then
                    Dim msgNo As String = C_MESSAGENO.CANNOTUPDATE
                    CommonFunctions.ShowMessage(msgNo, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", msgNo)})
                    hdnMsgId.Value = msgNo
                    Return
                End If
                '更新処理実行
                UpdateBreaker(brInfo, ownerDt, costDt, sqlCon, saveTab:=saveTab, callerButton:=callerButton)
            End If

        End Using

    End Sub
    ''' <summary>
    ''' 新規ブレーカー登録
    ''' </summary>
    ''' <remarks>無条件に全項目インサートする</remarks>
    Private Sub EntryNewBreaker(brInfoDt As Dictionary(Of String, BreakerInfo), ownerDt As DataTable, costDt As DataTable, Optional sqlCon As SqlConnection = Nothing, Optional isSendConfirm As Boolean = False, Optional orgBrId As String = "", Optional isModifiedPort As Boolean = False)
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Dim tran As SqlTransaction = Nothing
        Dim procDateTime As DateTime = DateTime.Now
        Dim brId As String = ""
        Dim AGENTPOL1 As String = ""
        Dim AGENTPOL2 As String = ""
        Dim AGENTPOD1 As String = ""
        Dim AGENTPOD2 As String = ""

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            'brId = brInfoDt.Values(0).BrId
            brId = brInfoDt("INFO").BrId
            tran = sqlCon.BeginTransaction() 'トランザクション開始
            '******************************
            ' ポート変更の場合は変更前のBRIDデータを論理削除
            '******************************
            If ViewState("COPYORGANIZERINFO") Is Nothing Then
                'コピー新規ではない場合元を削除
                DeleteBreaker(orgBrId, sqlCon, tran, procDateTime)
            End If
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
                '固定パラメータの設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@SUBID", SqlDbType.NVarChar, 20).Value = brInfoDt("INFO").SubId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@BRTYPE", SqlDbType.NVarChar, 20).Value = brInfoDt("INFO").BrType
                    .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = brInfoDt("INFO").ApplyId
                    .Add("@LASTSTEP", SqlDbType.NVarChar, 20).Value = brInfoDt("INFO").LastStep
                    .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = brInfoDt("INFO").UseType
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                End With
                '動的パラメータの設定
                'Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 5120)
                Dim paramType As SqlParameter = sqlCmd.Parameters.Add("@TYPE", SqlDbType.NVarChar, 20)
                Dim paramLinkID As SqlParameter = sqlCmd.Parameters.Add("@LINKID", SqlDbType.NVarChar, 20)

                For Each brInfoItem As BreakerInfo In brInfoDt.Values
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
            sqlStat.AppendLine("             ,CAPACITY")
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
            sqlStat.AppendLine("             ,REMARK")
            sqlStat.AppendLine("             ,ORIGINALCOPYBRID")
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
            sqlStat.AppendLine("             ,@CAPACITY")
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
            sqlStat.AppendLine("             ,@REMARK")
            sqlStat.AppendLine("             ,@ORIGINALCOPYBRID")
            sqlStat.AppendLine("             ,@DELFLG")
            sqlStat.AppendLine("             ,@INITYMD ")
            sqlStat.AppendLine("             ,@INITUSER ")
            sqlStat.AppendLine("             ,@UPDYMD ")
            sqlStat.AppendLine("             ,@UPDUSER ")
            sqlStat.AppendLine("             ,@UPDTERMID ")
            sqlStat.AppendLine("             ,@RECEIVEYMD ")
            sqlStat.AppendLine(") ")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                'パラメータ変数定義
                With sqlCmd.Parameters
                    'パラメータ設定
                    Dim dr As DataRow = ownerDt.Rows(0)
                    Dim BrBaseId As String = brInfoDt("INFO").LinkId
                    Dim usingLeaseTank As String = brInfoDt("INFO").UsingLeaseTank
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@BRBASEID", SqlDbType.NVarChar, 20).Value = BrBaseId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@VALIDITYFROM", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("VALIDITYFROM")))
                    .Add("@VALIDITYTO", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("VALIDITYTO")))
                    .Add("@TERMTYPE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TERMTYPE"))
                    .Add("@NOOFTANKS", SqlDbType.Int, 20).Value = IntStringToInt(Convert.ToString(dr.Item("NOOFTANKS")))
                    .Add("@SHIPPER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("SHIPPER"))
                    .Add("@CONSIGNEE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CONSIGNEE"))
                    .Add("@CARRIER1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CARRIER1"))
                    .Add("@CARRIER2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CARRIER2"))
                    .Add("@PRODUCTCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("PRODUCTCODE"))
                    .Add("@PRODUCTWEIGHT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("PRODUCTWEIGHT")))
                    .Add("@CAPACITY", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("CAPACITY")))
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
                    .Add("@INVOICEDBY", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("INVOICEDBY"))
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
                    .Add("@AGENTPOL1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOL1"))
                    .Add("@AGENTPOL2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOL2"))
                    .Add("@AGENTPOD1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOD1"))
                    .Add("@AGENTPOD2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOD2"))
                    '.Add("@APPLYTEXT", SqlDbType.NVarChar, 1024).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                    .Add("@APPLYTEXT", SqlDbType.NVarChar, 5120).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                    .Add("@COUNTRYORGANIZER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("COUNTRYORGANIZER"))
                    .Add("@LASTORDERNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LASTORDERNO"))
                    .Add("@TANKNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TANKNO"))
                    .Add("@DEPOTCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DEPOTCODE"))
                    .Add("@TWOAGOPRODUCT", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TWOAGOPRODUCT"))
                    .Add("@FEE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("FEE")))
                    .Add("@BILLINGCATEGORY", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("BILLINGCATEGORY"))
                    .Add("@USINGLEASETANK", SqlDbType.NVarChar).Value = usingLeaseTank
                    .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REMARK"))
                    .Add("@ORIGINALCOPYBRID", SqlDbType.NVarChar).Value = orgBrId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    AGENTPOL1 = Convert.ToString(dr.Item("AGENTPOL1"))
                    AGENTPOL2 = Convert.ToString(dr.Item("AGENTPOL2"))
                    AGENTPOD1 = Convert.ToString(dr.Item("AGENTPOD1"))
                    AGENTPOD2 = Convert.ToString(dr.Item("AGENTPOD2"))
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
            sqlStat.AppendLine("             ,ACTIONID")
            sqlStat.AppendLine("             ,CLASS1")
            sqlStat.AppendLine("             ,CLASS2")
            sqlStat.AppendLine("             ,CLASS3")
            sqlStat.AppendLine("             ,CLASS4")
            sqlStat.AppendLine("             ,CLASS5")
            sqlStat.AppendLine("             ,CLASS6")
            sqlStat.AppendLine("             ,CLASS7")
            sqlStat.AppendLine("             ,CLASS8")
            sqlStat.AppendLine("             ,CLASS9")
            sqlStat.AppendLine("             ,TAXATION")
            sqlStat.AppendLine("             ,COUNTRYCODE")
            sqlStat.AppendLine("             ,REPAIRFLG")
            sqlStat.AppendLine("             ,APPROVEDUSD")
            sqlStat.AppendLine("             ,INVOICEDBY")
            sqlStat.AppendLine("             ,BILLING")
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
            sqlStat.AppendLine("             ,@ACTIONID")
            sqlStat.AppendLine("             ,@CLASS1")
            sqlStat.AppendLine("             ,@CLASS2")
            sqlStat.AppendLine("             ,@CLASS3")
            sqlStat.AppendLine("             ,@CLASS4")
            sqlStat.AppendLine("             ,@CLASS5")
            sqlStat.AppendLine("             ,@CLASS6")
            sqlStat.AppendLine("             ,@CLASS7")
            sqlStat.AppendLine("             ,@CLASS8")
            sqlStat.AppendLine("             ,@CLASS9")
            sqlStat.AppendLine("             ,@TAXATION")
            sqlStat.AppendLine("             ,@COUNTRYCODE")
            sqlStat.AppendLine("             ,@REPAIRFLG")
            sqlStat.AppendLine("             ,@APPROVEDUSD")
            sqlStat.AppendLine("             ,@INVOICEDBY")
            sqlStat.AppendLine("             ,@BILLING")
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
                '固定パラメータの設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                '動的パラメータの設定
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
                Dim paramActionId As SqlParameter = sqlCmd.Parameters.Add("@ACTIONID", SqlDbType.NVarChar, 20)
                Dim paramClass1 As SqlParameter = sqlCmd.Parameters.Add("@CLASS1", SqlDbType.NVarChar, 50)
                Dim paramClass2 As SqlParameter = sqlCmd.Parameters.Add("@CLASS2", SqlDbType.NVarChar, 50)
                Dim paramClass3 As SqlParameter = sqlCmd.Parameters.Add("@CLASS3", SqlDbType.NVarChar, 50)
                Dim paramClass4 As SqlParameter = sqlCmd.Parameters.Add("@CLASS4", SqlDbType.NVarChar, 50)
                Dim paramClass5 As SqlParameter = sqlCmd.Parameters.Add("@CLASS5", SqlDbType.NVarChar, 50)
                Dim paramClass6 As SqlParameter = sqlCmd.Parameters.Add("@CLASS6", SqlDbType.NVarChar, 50)
                Dim paramClass7 As SqlParameter = sqlCmd.Parameters.Add("@CLASS7", SqlDbType.NVarChar, 50)
                Dim paramClass8 As SqlParameter = sqlCmd.Parameters.Add("@CLASS8", SqlDbType.NVarChar, 50)
                Dim paramClass9 As SqlParameter = sqlCmd.Parameters.Add("@CLASS9", SqlDbType.NVarChar, 50)
                Dim paramTaxation As SqlParameter = sqlCmd.Parameters.Add("@TAXATION", SqlDbType.NVarChar, 50)
                Dim paramCountry As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
                Dim paramRepairFlg As SqlParameter = sqlCmd.Parameters.Add("@REPAIRFLG", SqlDbType.NVarChar, 1)
                Dim paramApprovedUsd As SqlParameter = sqlCmd.Parameters.Add("@APPROVEDUSD", SqlDbType.Float)
                Dim paramInvoicedBy As SqlParameter = sqlCmd.Parameters.Add("@INVOICEDBY", SqlDbType.NVarChar, 20)
                Dim paramBilling As SqlParameter = sqlCmd.Parameters.Add("@BILLING", SqlDbType.NVarChar)
                'Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 5120)

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
                    paramCurrencycode.Value = Convert.ToString(dr.Item("CURRENCYCODE"))
                    paramAgent.Value = Convert.ToString(dr.Item("AGENT"))
                    paramActionId.Value = Convert.ToString(dr.Item("ACTIONID"))
                    paramClass1.Value = Convert.ToString(dr.Item("CLASS1"))
                    paramClass2.Value = Convert.ToString(dr.Item("CLASS2"))
                    paramClass3.Value = Convert.ToString(dr.Item("CLASS3"))
                    paramClass4.Value = Convert.ToString(dr.Item("CLASS4"))
                    paramClass5.Value = Convert.ToString(dr.Item("CLASS5"))
                    paramClass6.Value = Convert.ToString(dr.Item("CLASS6"))
                    paramClass7.Value = Convert.ToString(dr.Item("CLASS7"))
                    paramClass8.Value = Convert.ToString(dr.Item("CLASS8"))
                    paramClass9.Value = Convert.ToString(dr.Item("CLASS9"))
                    paramTaxation.Value = Convert.ToString(dr.Item("TAXATION"))
                    paramCountry.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                    paramRepairFlg.Value = Convert.ToString(dr.Item("REPAIRFLG"))
                    paramApprovedUsd.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("APPROVEDUSD")))

                    paramInvoicedBy.Value = Convert.ToString(dr.Item("CINVOICEDBY"))
                    paramBilling.Value = Convert.ToString(dr.Item("BILLING"))
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
    ''' ブレーカーの全情報を論理削除
    ''' </summary>
    ''' <remarks>港変更時にのみ実行</remarks>
    Private Sub DeleteBreaker(brId As String, ByRef sqlCon As SqlConnection, ByRef sqlTran As SqlTransaction, Optional procDate As Date = #1900/1/1#)
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        '引数で指定したBrIdにつき情報・基本・費用の全情報に削除フラグを立てる
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
        sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
        sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
        sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE BRID      = @BRID")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
        sqlStat.AppendLine(";")
        sqlStat.AppendLine("UPDATE GBT0002_BR_BASE")
        sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
        sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
        sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE BRID      = @BRID")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
        sqlStat.AppendLine(";")
        sqlStat.AppendLine("UPDATE GBT0003_BR_VALUE")
        sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
        sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
        sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE BRID      = @BRID")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
        sqlStat.AppendLine(";")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("@BRID", SqlDbType.NVarChar).Value = brId
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            End With
            sqlCmd.ExecuteNonQuery()
        End Using
    End Sub
    ''' <summary>
    ''' ブレーカー情報を取得し更新可能かチェック
    ''' </summary>
    ''' <param name="brInfoDt"></param>
    ''' <returns></returns>
    ''' <remarks>要タブ・権限に応じた制御</remarks>
    Private Function CanBreakerUpdate(brInfoDt As Dictionary(Of String, BreakerInfo), Optional sqlCon As SqlConnection = Nothing, Optional dtlPolPod As String = "", Optional callerButton As String = "") As Boolean
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim brNo As String = brInfoDt("INFO").BrId
            '更新直前のブレーカー紐づけテーブル取得
            Dim brInfoData = GetBreakerInfo(brNo, sqlCon)
            'タイムスタンプが一致していない場合は更新不可
            For Each brInfoKey As String In brInfoData.Keys
                If dtlPolPod <> "" AndAlso dtlPolPod <> brInfoKey Then
                    Continue For
                End If
                'If brInfoDt(brInfoKey).TimeStamp <> brInfoData(brInfoKey).TimeStamp Then
                '    Return False
                'End If
                If brInfoDt(brInfoKey).UpdYmd <> brInfoData(brInfoKey).UpdYmd OrElse
                   brInfoDt(brInfoKey).UpdUser <> brInfoData(brInfoKey).UpdUser OrElse
                   brInfoDt(brInfoKey).UpdTermId <> brInfoData(brInfoKey).UpdTermId Then
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
    Private Sub UpdateBreaker(brInfoDt As Dictionary(Of String, BreakerInfo), ownerDt As DataTable, costDt As DataTable, Optional sqlCon As SqlConnection = Nothing, Optional isSendConfirm As Boolean = False, Optional saveTab As String = "", Optional callerButton As String = "")
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Dim tran As SqlTransaction = Nothing
        Dim procDateTime As DateTime = DateTime.Now

        Dim brId As String = brInfoDt("INFO").BrId
        Dim usingLeaseTank As String = brInfoDt("INFO").UsingLeaseTank
        Dim InsBrInfo = New Dictionary(Of String, BreakerInfo)

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            '直近BrBaseステータス取得
            Dim dtStat As DataTable = GetStatus(brId)
            Dim qDrStat = From drItem In dtStat Where drItem("TYPE").Equals("INFO") Select Convert.ToString(drItem("STATUS"))
            Dim currentStat As String = ""
            If qDrStat.Any Then
                currentStat = qDrStat(0)
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
                'POL、PODの場合、対象のみ実施、否認時修正中の場合はすべて保存
                If Me.hdnStatus.Value <> C_APP_STATUS.REVISE AndAlso
                   (({"POL1", "POL2", "POD1", "POD2"}.Contains(saveTab)) OrElse
                    (callerButton = "btnSave" AndAlso saveTab = "INFO")) Then
                    If Not brInfoItem.Type.Contains(saveTab) Then
                        Continue For
                    End If
                End If
                '紐付け情報
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
                sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
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
                    sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
                    sqlStat.AppendLine(" WHERE BRID      = @BRID")
                    sqlStat.AppendLine("   AND BRBASEID  = @LINKID")
                Else
                    sqlStat.AppendLine("UPDATE GBT0003_BR_VALUE")
                    sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                    sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                    sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
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
                            '.Add("@APPROVEDTEXT", SqlDbType.NVarChar, 1024).Value = dr.Item("APPROVEDTEXT")
                            .Add("@APPROVEDTEXT", SqlDbType.NVarChar, 5120).Value = dr.Item("APPROVEDTEXT")
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
                Dim subIdParamValue As String = ""
                '承認画面遷移の場合
                If Me.hdnCallerMapId.Value = CONST_APP_MAPID Then
                    'ステータスが否認時修正中の初回保存以外
                    'If Me.hdnStatus.Value = C_APP_STATUS.REVISE AndAlso currentStat = C_APP_STATUS.REVISE Then
                    If (Me.hdnStatus.Value = C_APP_STATUS.REVISE AndAlso currentStat = C_APP_STATUS.REVISE) _
                        OrElse (Me.hdnStatus.Value = currentStat AndAlso callerButton = "btnSave") Then
                        subIdParamValue = InsBrInfo("INFO").SubId
                    Else
                        Dim reg As New Regex("[^\d]")
                        Dim strDes As String = reg.Replace(InsBrInfo("INFO").SubId, "")
                        Dim strSub As String = InsBrInfo("INFO").SubId.Replace(strDes, "")
                        Dim subNo As String = Format(Integer.Parse(strDes) + 1, "00000")
                        Dim subId As String = strSub & subNo
                        subIdParamValue = subId
                    End If
                Else
                    subIdParamValue = InsBrInfo("INFO").SubId
                End If
                '固定パラメータの設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@SUBID", SqlDbType.NVarChar, 20).Value = subIdParamValue
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@BRTYPE", SqlDbType.NVarChar, 20).Value = InsBrInfo("INFO").BrType
                    .Add("@USETYPE", SqlDbType.NVarChar, 20).Value = InsBrInfo("INFO").UseType
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                '動的パラメータの設定
                'Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 5120)
                Dim paramType As SqlParameter = sqlCmd.Parameters.Add("@TYPE", SqlDbType.NVarChar, 20)
                Dim paramLinkID As SqlParameter = sqlCmd.Parameters.Add("@LINKID", SqlDbType.NVarChar, 20)
                Dim paramApplyId As SqlParameter = sqlCmd.Parameters.Add("@APPLYID", SqlDbType.NVarChar, 20)
                Dim paramLastStep As SqlParameter = sqlCmd.Parameters.Add("@LASTSTEP", SqlDbType.NVarChar, 20)

                For Each brInfoItem As BreakerInfo In InsBrInfo.Values
                    '対象のみ実施
                    If (Not (Me.hdnStatus.Value = C_APP_STATUS.REVISE)) AndAlso (({"POL1", "POL2", "POD1", "POD2"}.Contains(saveTab)) OrElse
                        (callerButton = "btnSave" AndAlso saveTab = "INFO")) Then
                        If Not brInfoItem.Type.Contains(saveTab) Then
                            Continue For
                        End If
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

            'オーガナイザーの場合、実施
            If (Me.hdnStatus.Value = C_APP_STATUS.REVISE) OrElse saveTab = "INFO" Then

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
                sqlStat.AppendLine("             ,DISABLED")
                sqlStat.AppendLine("             ,TERMTYPE")
                sqlStat.AppendLine("             ,NOOFTANKS")
                sqlStat.AppendLine("             ,SHIPPER")
                sqlStat.AppendLine("             ,CONSIGNEE")
                sqlStat.AppendLine("             ,CARRIER1")
                sqlStat.AppendLine("             ,CARRIER2")
                sqlStat.AppendLine("             ,PRODUCTCODE")
                sqlStat.AppendLine("             ,PRODUCTWEIGHT")
                sqlStat.AppendLine("             ,CAPACITY")
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
                sqlStat.AppendLine("             ,REMARK")
                sqlStat.AppendLine("             ,ORIGINALCOPYBRID")
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
                sqlStat.AppendLine("             ,@DISABLED")
                sqlStat.AppendLine("             ,@TERMTYPE")
                sqlStat.AppendLine("             ,@NOOFTANKS")
                sqlStat.AppendLine("             ,@SHIPPER")
                sqlStat.AppendLine("             ,@CONSIGNEE")
                sqlStat.AppendLine("             ,@CARRIER1")
                sqlStat.AppendLine("             ,@CARRIER2")
                sqlStat.AppendLine("             ,@PRODUCTCODE")
                sqlStat.AppendLine("             ,@PRODUCTWEIGHT")
                sqlStat.AppendLine("             ,@CAPACITY")
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
                sqlStat.AppendLine("             ,@REMARK")
                sqlStat.AppendLine("             ,@ORIGINALCOPYBRID")
                sqlStat.AppendLine("             ,@DELFLG")
                sqlStat.AppendLine("             ,@INITYMD ")
                sqlStat.AppendLine("             ,@INITUSER ")
                sqlStat.AppendLine("             ,@UPDYMD ")
                sqlStat.AppendLine("             ,@UPDUSER ")
                sqlStat.AppendLine("             ,@UPDTERMID ")
                sqlStat.AppendLine("             ,@RECEIVEYMD ")
                sqlStat.AppendLine(") ")
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    'パラメータ変数定義
                    With sqlCmd.Parameters
                        Dim dr As DataRow = ownerDt.Rows(0)
                        Dim BrBaseId As String = InsBrInfo("INFO").LinkId

                        .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                        .Add("@BRBASEID", SqlDbType.NVarChar, 20).Value = BrBaseId
                        .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                        .Add("@VALIDITYFROM", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("VALIDITYFROM")))
                        .Add("@VALIDITYTO", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("VALIDITYTO")))
                        .Add("@DISABLED", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DISABLED"))
                        .Add("@TERMTYPE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TERMTYPE"))
                        .Add("@NOOFTANKS", SqlDbType.Int, 20).Value = IntStringToInt(Convert.ToString(dr.Item("NOOFTANKS")))
                        .Add("@SHIPPER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("SHIPPER"))
                        .Add("@CONSIGNEE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CONSIGNEE"))
                        .Add("@CARRIER1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CARRIER1"))
                        .Add("@CARRIER2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("CARRIER2"))
                        .Add("@PRODUCTCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("PRODUCTCODE"))
                        .Add("@PRODUCTWEIGHT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("PRODUCTWEIGHT")))
                        .Add("@CAPACITY", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("CAPACITY")))
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
                        .Add("@INVOICEDBY", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("INVOICEDBY"))
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
                        .Add("@AGENTPOL1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOL1"))
                        .Add("@AGENTPOL2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOL2"))
                        .Add("@AGENTPOD1", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOD1"))
                        .Add("@AGENTPOD2", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("AGENTPOD2"))
                        '.Add("@APPLYTEXT", SqlDbType.NVarChar, 1024).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                        .Add("@APPLYTEXT", SqlDbType.NVarChar, 5120).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                        .Add("@COUNTRYORGANIZER", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("COUNTRYORGANIZER"))
                        .Add("@LASTORDERNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("LASTORDERNO"))
                        .Add("@TANKNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TANKNO"))
                        .Add("@DEPOTCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("DEPOTCODE"))
                        .Add("@TWOAGOPRODUCT", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TWOAGOPRODUCT"))
                        .Add("@FEE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("FEE")))
                        .Add("@BILLINGCATEGORY", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("BILLINGCATEGORY"))
                        .Add("@USINGLEASETANK", SqlDbType.NVarChar, 20).Value = usingLeaseTank
                        .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REMARK"))
                        .Add("@ORIGINALCOPYBRID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ORIGINALCOPYBRID"))
                        .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                        .Add("@INITYMD", SqlDbType.DateTime).Value = Me.hdnInitYmd.Value
                        .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = Me.hdnInitUser.Value
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                        .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using
            End If

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
            sqlStat.AppendLine("             ,ACTIONID")
            sqlStat.AppendLine("             ,CLASS1")
            sqlStat.AppendLine("             ,CLASS2")
            sqlStat.AppendLine("             ,CLASS3")
            sqlStat.AppendLine("             ,CLASS4")
            sqlStat.AppendLine("             ,CLASS5")
            sqlStat.AppendLine("             ,CLASS6")
            sqlStat.AppendLine("             ,CLASS7")
            sqlStat.AppendLine("             ,CLASS8")
            sqlStat.AppendLine("             ,CLASS9")
            sqlStat.AppendLine("             ,TAXATION")
            sqlStat.AppendLine("             ,COUNTRYCODE")
            sqlStat.AppendLine("             ,REPAIRFLG")
            sqlStat.AppendLine("             ,APPROVEDUSD")
            sqlStat.AppendLine("             ,INVOICEDBY")
            sqlStat.AppendLine("             ,BILLING")
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
            sqlStat.AppendLine("             ,@ACTIONID")
            sqlStat.AppendLine("             ,@CLASS1")
            sqlStat.AppendLine("             ,@CLASS2")
            sqlStat.AppendLine("             ,@CLASS3")
            sqlStat.AppendLine("             ,@CLASS4")
            sqlStat.AppendLine("             ,@CLASS5")
            sqlStat.AppendLine("             ,@CLASS6")
            sqlStat.AppendLine("             ,@CLASS7")
            sqlStat.AppendLine("             ,@CLASS8")
            sqlStat.AppendLine("             ,@CLASS9")
            sqlStat.AppendLine("             ,@TAXATION")
            sqlStat.AppendLine("             ,@COUNTRYCODE")
            sqlStat.AppendLine("             ,@REPAIRFLG")
            sqlStat.AppendLine("             ,@APPROVEDUSD")
            sqlStat.AppendLine("             ,@INVOICEDBY")
            sqlStat.AppendLine("             ,@BILLING")
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
                '固定パラメータの設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@STYMD", SqlDbType.Date).Value = procDateTime
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = Me.hdnInitYmd.Value
                    .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = Me.hdnInitUser.Value
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                '動的パラメータの設定
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
                Dim paramActionId As SqlParameter = sqlCmd.Parameters.Add("@ACTIONID", SqlDbType.NVarChar, 20)
                Dim paramClass1 As SqlParameter = sqlCmd.Parameters.Add("@CLASS1", SqlDbType.NVarChar, 50)
                Dim paramClass2 As SqlParameter = sqlCmd.Parameters.Add("@CLASS2", SqlDbType.NVarChar, 50)
                Dim paramClass3 As SqlParameter = sqlCmd.Parameters.Add("@CLASS3", SqlDbType.NVarChar, 50)
                Dim paramClass4 As SqlParameter = sqlCmd.Parameters.Add("@CLASS4", SqlDbType.NVarChar, 50)
                Dim paramClass5 As SqlParameter = sqlCmd.Parameters.Add("@CLASS5", SqlDbType.NVarChar, 50)
                Dim paramClass6 As SqlParameter = sqlCmd.Parameters.Add("@CLASS6", SqlDbType.NVarChar, 50)
                Dim paramClass7 As SqlParameter = sqlCmd.Parameters.Add("@CLASS7", SqlDbType.NVarChar, 50)
                Dim paramClass8 As SqlParameter = sqlCmd.Parameters.Add("@CLASS8", SqlDbType.NVarChar, 50)
                Dim paramClass9 As SqlParameter = sqlCmd.Parameters.Add("@CLASS9", SqlDbType.NVarChar, 50)
                Dim paramTaxation As SqlParameter = sqlCmd.Parameters.Add("@TAXATION", SqlDbType.NVarChar, 50)
                Dim paramCountry As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
                Dim paramRepairFlg As SqlParameter = sqlCmd.Parameters.Add("@REPAIRFLG", SqlDbType.NVarChar, 1)
                Dim paramApprovedUsd As SqlParameter = sqlCmd.Parameters.Add("@APPROVEDUSD", SqlDbType.Float)
                Dim paramInvoicedBy As SqlParameter = sqlCmd.Parameters.Add("@INVOICEDBY", SqlDbType.NVarChar, 20)
                Dim paramBilling As SqlParameter = sqlCmd.Parameters.Add("@BILLING", SqlDbType.NVarChar)
                'Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 5120)

                For Each dr As DataRow In costDt.Rows
                    Dim dtlPolPod As String = Convert.ToString(dr.Item("DTLPOLPOD"))
                    If (Not Me.hdnStatus.Value = C_APP_STATUS.REVISE) AndAlso (({"POL1", "POL2", "POD1", "POD2"}.Contains(saveTab)) OrElse
                         (callerButton = "btnSave" AndAlso saveTab = "INFO")) Then
                        If Not dtlPolPod.Contains(saveTab) Then
                            Continue For
                        End If
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
                    paramCurrencycode.Value = Convert.ToString(dr.Item("CURRENCYCODE"))
                    paramAgent.Value = Convert.ToString(dr.Item("AGENT"))
                    paramActionId.Value = Convert.ToString(dr.Item("ACTIONID"))
                    paramClass1.Value = Convert.ToString(dr.Item("CLASS1"))
                    paramClass2.Value = Convert.ToString(dr.Item("CLASS2"))
                    paramClass3.Value = Convert.ToString(dr.Item("CLASS3"))
                    paramClass4.Value = Convert.ToString(dr.Item("CLASS4"))
                    paramClass5.Value = Convert.ToString(dr.Item("CLASS5"))
                    paramClass6.Value = Convert.ToString(dr.Item("CLASS6"))
                    paramClass7.Value = Convert.ToString(dr.Item("CLASS7"))
                    paramClass8.Value = Convert.ToString(dr.Item("CLASS8"))
                    paramClass9.Value = Convert.ToString(dr.Item("CLASS9"))
                    paramTaxation.Value = Convert.ToString(dr.Item("TAXATION"))
                    paramCountry.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                    paramRepairFlg.Value = Convert.ToString(dr.Item("REPAIRFLG"))
                    paramApprovedUsd.Value = DecimalStringToDecimal(Convert.ToString(dr.Item("APPROVEDUSD")))

                    paramInvoicedBy.Value = Convert.ToString(dr.Item("CINVOICEDBY"))
                    paramBilling.Value = Convert.ToString(dr.Item("BILLING"))
                    paramRemark.Value = Convert.ToString(dr.Item("REMARK"))

                    sqlCmd.ExecuteNonQuery()
                Next

            End Using

            Me.lblBrNo.Text = brId

            tran.Commit() 'トランザクションコミット

            '******************************
            ' 編集の場合、ステータス更新
            '******************************
            If currentStat = C_APP_STATUS.APPLYING AndAlso Me.hdnStatus.Value = C_APP_STATUS.REVISE Then
                Dim COA0032Apploval As New BASEDLL.COA0032Apploval

                '訂正中登録
                COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
                COA0032Apploval.I_APPLYID = InsBrInfo("INFO").ApplyId
                COA0032Apploval.I_STEP = Me.hdnStep.Value
                COA0032Apploval.COA0032setCorrection()
                If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage)
                    Return
                End If
            End If
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
        Dim typeList As New List(Of String) From {"INFO", "POL1"}
        If Not ViewState(CONST_VS_DISP_POLONLY).Equals("1") Then
            typeList.Add("POD1")
        End If
        If Convert.ToString(dr.Item("ISTRILATERAL")) = "1" Then
            typeList.AddRange({"POL2", "POD2"})
        End If
        For Each type In typeList
            Dim item As New BreakerInfo
            item.BrId = brId
            item.SubId = "S00001"
            item.Type = type
            item.LinkId = type & "-" & "00001"
            If Convert.ToString(dr.Item("BRTYPE")) = "1" Then
                item.BrType = C_BRTYPE.SALES
            Else
                item.BrType = C_BRTYPE.OPERATION
            End If
            item.UseType = Convert.ToString(dr.Item("USETYPE"))
            item.UsingLeaseTank = Convert.ToString(dr.Item("USINGLEASETANK"))
            item.Remark = Convert.ToString(dr.Item("REMARK"))

            retDic.Add(type, item)
        Next
        Return retDic
    End Function


    ''' <summary>
    ''' ブレーカー関連付け情報取得
    ''' </summary>
    ''' <param name="sqlCon">オプション 項目</param>
    ''' <returns>ディクショナリ キー：区分(POD1、POL1等) , 値：直近ブレーカー関連付け</returns>
    Private Function GetBreakerInfo(brId As String, Optional sqlCon As SqlConnection = Nothing, Optional withoutApplyInfo As Boolean = False) As Dictionary(Of String, BreakerInfo)
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
        sqlStat.AppendLine("      ,AH.STATUS ")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUSNAME")
        sqlStat.AppendLine("      ,BI.USETYPE ")
        sqlStat.AppendLine("      ,BI.REMARK ")
        sqlStat.AppendLine("      ,CAST(BI.UPDTIMSTP As bigint) AS TIMSTP")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, BI.UPDYMD , 120),'') AS UPDYMD")
        sqlStat.AppendLine("      ,isnull(rtrim(BI.UPDUSER),'')                  AS UPDUSER")
        sqlStat.AppendLine("      ,isnull(rtrim(BI.UPDTERMID),'')                AS UPDTERMID")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH") '承認履歴
        sqlStat.AppendLine("    ON  AH.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AH.APPLYID      = BI.APPLYID")
        sqlStat.AppendLine("   AND  AH.STEP         = BI.LASTSTEP")
        sqlStat.AppendLine("   AND  AH.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FV.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FV.KEYCODE      = AH.STATUS")
        sqlStat.AppendLine("   AND  FV.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  FV.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
        sqlStat.AppendLine(" WHERE BI.BRID         = @BRID")
        sqlStat.AppendLine("   And BI.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And BI.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And BI.DELFLG      <> @DELFLG")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim dt As New DataTable

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVCamp
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@LANGDISP", SqlDbType.NVarChar, 20).Value = COA0019Session.LANGDISP
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using
            End Using

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                For Each dr As DataRow In dt.Rows
                    Dim item As New BreakerInfo
                    item.BrId = Convert.ToString(dr("BRID"))
                    item.SubId = Convert.ToString(dr("SUBID"))
                    item.Type = Convert.ToString(dr("TYPE"))
                    item.LinkId = Convert.ToString(dr("LINKID"))
                    item.Stymd = Convert.ToString(dr("STYMD"))
                    item.BrType = Convert.ToString(dr("BRTYPE"))
                    If withoutApplyInfo Then
                        item.ApplyId = ""
                        item.LastStep = ""
                        item.AppStatus = ""
                    Else
                        item.ApplyId = Convert.ToString(dr("APPLYID"))
                        item.LastStep = Convert.ToString(dr("LASTSTEP"))
                        item.AppStatus = Convert.ToString(dr("STATUSNAME"))
                    End If
                    item.UseType = Convert.ToString(dr("USETYPE"))
                    item.Remark = Convert.ToString(dr("REMARK"))
                    item.TimeStamp = Convert.ToString(dr("TIMSTP"))
                    item.UpdYmd = Convert.ToString(dr("UPDYMD"))
                    item.UpdUser = Convert.ToString(dr("UPDUSER"))
                    item.UpdTermId = Convert.ToString(dr("UPDTERMID"))
                    retDic.Add(item.Type, item)
                Next dr
            End If

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
        sqlStat.AppendLine("      ,BS.DISABLED AS DISABLED")
        sqlStat.AppendLine("      ,BS.TERMTYPE AS TERMTYPE")
        sqlStat.AppendLine("      ,BS.NOOFTANKS AS NOOFTANKS")
        sqlStat.AppendLine("      ,BS.SHIPPER AS SHIPPER")
        sqlStat.AppendLine("      ,BS.CONSIGNEE AS CONSIGNEE")
        sqlStat.AppendLine("      ,BS.CARRIER1 AS CARRIER1")
        sqlStat.AppendLine("      ,BS.CARRIER2 AS CARRIER2")
        sqlStat.AppendLine("      ,BS.PRODUCTCODE AS PRODUCTCODE")
        sqlStat.AppendLine("      ,BS.PRODUCTWEIGHT AS PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,BS.CAPACITY AS CAPACITY")
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
        sqlStat.AppendLine("      ,BS.COUNTRYORGANIZER AS COUNTRYORGANIZER")
        sqlStat.AppendLine("      ,BS.LASTORDERNO AS LASTORDERNO")
        sqlStat.AppendLine("      ,BS.TANKNO AS TANKNO")
        sqlStat.AppendLine("      ,BS.DEPOTCODE AS DEPOTCODE")
        sqlStat.AppendLine("      ,BS.TWOAGOPRODUCT AS TWOAGOPRODUCT")
        sqlStat.AppendLine("      ,BS.FEE AS FEE")
        sqlStat.AppendLine("      ,BS.BILLINGCATEGORY AS BILLINGCATEGORY")
        sqlStat.AppendLine("      ,BS.USINGLEASETANK  AS USINGLEASETANK")
        sqlStat.AppendLine("      ,BS.REMARK AS REMARK")
        sqlStat.AppendLine("      ,BS.ORIGINALCOPYBRID AS ORIGINALCOPYBRID")
        sqlStat.AppendLine("      ,ISNULL(AH.APPROVEDTEXT,'') AS APPROVEDTEXT")
        sqlStat.AppendLine("      ,CASE CONVERT(NVARCHAR, AH.APPLYDATE , 111) WHEN '1900/01/01' THEN '' ELSE CONVERT(NVARCHAR, AH.APPLYDATE , 111) END AS APPLYDATE")
        sqlStat.AppendLine("      ,AH.APPLICANTID AS APPLICANTID")
        sqlStat.AppendLine("      ,ISNULL(US1.STAFFNAMES_EN,'') AS APPLICANTNAME")
        sqlStat.AppendLine("      ,CASE CONVERT(NVARCHAR, AH.APPROVEDATE , 111) WHEN '1900/01/01' THEN '' ELSE CONVERT(NVARCHAR, AH.APPROVEDATE , 111) END AS APPROVEDATE")
        sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
        sqlStat.AppendLine("      ,ISNULL(US2.STAFFNAMES_EN,'') AS APPROVERNAME")
        sqlStat.AppendLine("      ,format(BS.INITYMD,'yyyy/MM/dd HH:mm:ss.fff') AS INITYMD")
        sqlStat.AppendLine("      ,BS.INITUSER AS INITUSER")
        sqlStat.AppendLine("      ,ISNULL(US3.STAFFNAMES_EN,'') AS INITUSERNAME")
        sqlStat.AppendLine("  FROM GBT0002_BR_BASE BS ")
        sqlStat.AppendLine(" LEFT JOIN GBT0001_BR_INFO BI ")
        sqlStat.AppendLine("   ON BI.BRID          = BS.BRID")
        sqlStat.AppendLine("  And BI.TYPE          = 'INFO'")
        sqlStat.AppendLine("  And BI.DELFLG       <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH")
        sqlStat.AppendLine("    ON  AH.COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND  AH.APPLYID     = BI.APPLYID")
        sqlStat.AppendLine("   AND  AH.STEP        = BI.LASTSTEP")
        sqlStat.AppendLine("   AND  AH.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US1")
        sqlStat.AppendLine("    ON  US1.USERID      = AH.APPLICANTID")
        sqlStat.AppendLine("   AND  US1.STYMD      <= (CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO  END) ")
        sqlStat.AppendLine("   AND  US1.ENDYMD     >= (CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO  END) ")
        sqlStat.AppendLine("   AND  US1.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US2")
        sqlStat.AppendLine("    ON  US2.USERID      = AH.APPROVERID")
        sqlStat.AppendLine("   AND  US2.STYMD      <= (CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO  END) ")
        sqlStat.AppendLine("   AND  US2.ENDYMD     >= (CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO  END) ")
        sqlStat.AppendLine("   AND  US2.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US3")
        sqlStat.AppendLine("    ON  US3.USERID      = BS.INITUSER")
        sqlStat.AppendLine("   AND  US3.STYMD      <= (CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO  END) ")
        sqlStat.AppendLine("   AND  US3.ENDYMD     >= (CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO  END) ")
        sqlStat.AppendLine("   AND  US3.DELFLG     <> @DELFLG")
        sqlStat.AppendLine(" WHERE BS.BRID     = @BRID ")
        sqlStat.AppendLine("   AND BS.BRBASEID = @BRBASEID ")
        Try
            Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brInfoOrganizer.BrId
                    .Add("@BRBASEID", SqlDbType.NVarChar, 20).Value = brInfoOrganizer.LinkId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVCamp
                End With
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
                retDt.Rows(0).Item("USETYPE") = dicBrInfo("INFO").UseType
                'Br紐づけ情報が4件以上の場合は三国間扱い(INFO,PODx,POLx)
                If dicBrInfo.Count >= 4 Then
                    retDt.Rows(0).Item("ISTRILATERAL") = "1"
                Else
                    retDt.Rows(0).Item("ISTRILATERAL") = "0"
                End If
                brInfoOrganizer.UsingLeaseTank = Convert.ToString(retDt.Rows(0).Item("USINGLEASETANK"))
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
        sqlStat.AppendLine("      ,COALESCE(DP.NAMES,TR.NAMES) AS CONTRACTORNAME")
        'sqlStat.AppendFormat("    ,CASE WHEN CC.CLASS4 = '{0}' THEN DP.NAMES ELSE TR.NAMES END AS CONTRACTORNAME ", GBC_CHARGECLASS4.DEPOT).AppendLine()
        sqlStat.AppendLine("      ,VL.COSTCODE ")
        sqlStat.AppendFormat("    ,CC.{0} AS COSTNAME ", nameField).AppendLine()
        sqlStat.AppendLine("      ,CC.CLASS4 AS CHARGECLASS4 ")
        sqlStat.AppendLine("      ,CC.CLASS8 AS CHARGECLASS8 ")
        sqlStat.AppendLine("      ,VL.BASEON ")
        sqlStat.AppendLine("      ,VL.TAX ")
        sqlStat.AppendLine("      ,VL.USD ")
        sqlStat.AppendLine("      ,VL.LOCAL ")
        sqlStat.AppendLine("      ,VL.CURRENCYCODE ")
        sqlStat.AppendLine("      ,VL.LOCALRATE ")
        sqlStat.AppendLine("      ,VL.USDRATE ")
        sqlStat.AppendLine("      ,VL.ACTIONID AS ACTIONID")
        sqlStat.AppendLine("      ,VL.CLASS1 AS CLASS1")
        sqlStat.AppendLine("      ,VL.CLASS2 AS CLASS2 ")
        sqlStat.AppendLine("      ,VL.CLASS3 AS CLASS3 ")
        sqlStat.AppendLine("      ,VL.CLASS4 AS CLASS4 ")
        sqlStat.AppendLine("      ,VL.CLASS5 AS CLASS5 ")
        sqlStat.AppendLine("      ,VL.CLASS6 AS CLASS6 ")
        sqlStat.AppendLine("      ,VL.CLASS7 AS CLASS7 ")
        sqlStat.AppendLine("      ,VL.CLASS8 AS CLASS8 ")
        sqlStat.AppendLine("      ,VL.CLASS9 AS CLASS9 ")
        sqlStat.AppendLine("      ,VL.TAXATION AS TAXATION ")
        sqlStat.AppendLine("      ,VL.COUNTRYCODE AS COUNTRYCODE ")
        sqlStat.AppendLine("      ,VL.INVOICEDBY AS CINVOICEDBY ")
        sqlStat.AppendLine("      ,VL.BILLING AS BILLING ")
        sqlStat.AppendLine("      ,VL.REMARK ")
        sqlStat.AppendLine("      ,CASE WHEN PT.COSTCODE Is NULL THEN '1' ELSE '0' END AS CAN_DELETE")
        sqlStat.AppendLine("  FROM GBT0003_BR_VALUE VL ")
        sqlStat.AppendLine("      LEFT JOIN GBM0010_CHARGECODE CC")
        sqlStat.AppendLine("        ON  VL.COSTCODE     = CC.COSTCODE ")
        sqlStat.AppendLine("       AND  CC.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND (CC.LDKBN = 'B' OR ")
        sqlStat.AppendLine("            CC.LDKBN = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' THEN 'L' ")
        sqlStat.AppendLine("                            ELSE 'D' END)")
        sqlStat.AppendLine("       AND  CC.STYMD       <= @STYMD")
        sqlStat.AppendLine("       AND  CC.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("       AND  CC.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0009_TRPATTERN PT")
        sqlStat.AppendLine("        ON  VL.COSTCODE = PT.COSTCODE ")
        sqlStat.AppendLine("       AND  VL.DTLPOLPOD = PT.AGENTKBN ")
        sqlStat.AppendLine("       AND  PT.ORG      = 'GB_Default' ")
        sqlStat.AppendLine("       AND  PT.BRTYPE   = @BRTYPE ")
        sqlStat.AppendLine("       AND  PT.USETYPE  = @USETYPE ")
        sqlStat.AppendLine("       AND  PT.STYMD       <= @STYMD")
        sqlStat.AppendLine("       AND  PT.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("       AND  PT.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TR")
        sqlStat.AppendLine("        ON  VL.CONTRACTOR = TR.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        'sqlStat.AppendLine("       AND  TR.CLASS        = 'VENDER'")
        sqlStat.AppendLine("       AND  TR.STYMD       <= @STYMD")
        sqlStat.AppendLine("       AND  TR.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("       AND  TR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DP")
        sqlStat.AppendLine("        ON  VL.CONTRACTOR = DP.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DP.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DP.STYMD       <= @STYMD")
        sqlStat.AppendLine("       AND  DP.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("       AND  DP.DELFLG      <> @DELFLG")
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
                Dim paramBrId As SqlParameter = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar, 20)
                Dim paramBrValueId As SqlParameter = sqlCmd.Parameters.Add("@BRVALUEID", SqlDbType.NVarChar, 20)
                Dim paramUseType As SqlParameter = sqlCmd.Parameters.Add("@USETYPE", SqlDbType.NVarChar, 20)
                Dim paramBrType As SqlParameter = sqlCmd.Parameters.Add("@BRTYPE", SqlDbType.NVarChar, 20)
                Dim paramStYmd As SqlParameter = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
                Dim paramEndYmd As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
                Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)

                For Each brInfoItem As BreakerInfo In dicBrInfo.Values
                    '基本情報の紐づけ情報はスキップ
                    If brInfoItem.Type = "INFO" Then
                        Continue For
                    End If
                    'SQLパラメータ値セット
                    paramBrId.Value = brInfoItem.BrId
                    paramBrValueId.Value = brInfoItem.LinkId
                    paramUseType.Value = useType
                    paramBrType.Value = brType
                    paramStYmd.Value = Date.Now
                    paramEndYmd.Value = Date.Now
                    paramDelFlg.Value = CONST_FLAG_YES

                    Using sqlDa As New SqlDataAdapter(sqlCmd)
                        Dim dt As New DataTable
                        sqlDa.Fill(dt)
                        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                            Throw New Exception("Get Breaker value info Error")
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
        Dim brType As String = ""
        If Me.hdnBrType.Value = "1" Then
            brType = "BT"
        Else
            brType = "BE"
        End If
        '生きているブレーカーは基本情報＋発地着地(最大4)の5レコード想定
        sqlStat.AppendLine("Select  '" & brType & "' ")
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
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = C_SERVERSEQ
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVname
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With
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
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    Protected Sub UploadExcel()
        Dim ds As New DataSet
        Dim currentTab As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Organizer
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)
        Dim isOrganizer As Boolean = False
        Dim isOutFull As Boolean = False
        Dim outCnt As Integer = 0
        Dim tabId As String = Nothing

        tabObjects.Add(COSTITEM.CostItemGroup.Organizer, Me.tabOrganizer)
        tabObjects.Add(COSTITEM.CostItemGroup.Export1, Me.tabExport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport1, Me.tabInport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Export2, Me.tabExport2)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport2, Me.tabInport2)

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        If reportId = "GBT00001F" Then
            isOutFull = True
        End If

        '初期処理
        Dim returnCode As String = C_MESSAGENO.NORMAL

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects

            If tabObject.Value.Visible = True Then
                If isOutFull = True OrElse (tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected")) Then
                    currentTab = tabObject.Key
                    If currentTab = COSTITEM.CostItemGroup.Organizer Then
                        isOrganizer = True
                    Else
                        isOrganizer = False
                    End If

                    Dim dtlPolPod As String = ""
                    Select Case currentTab
                        Case COSTITEM.CostItemGroup.Export1
                            dtlPolPod = "POL1"
                            tabId = Me.tabExport1.ClientID
                        Case COSTITEM.CostItemGroup.Inport1
                            dtlPolPod = "POD1"
                            tabId = Me.tabInport1.ClientID
                        Case COSTITEM.CostItemGroup.Export2
                            dtlPolPod = "POL2"
                            tabId = Me.tabExport2.ClientID
                        Case COSTITEM.CostItemGroup.Inport2
                            dtlPolPod = "POD2"
                            tabId = Me.tabInport2.ClientID
                    End Select
                    Dim dt As DataTable = Nothing

                    Dim reportMapId As String = ""
                    If isOrganizer = True Then
                        '画面オーガナイザー情報を取得しデータテーブルに格納
                        dt = CollectDisplayOrganizerInfo()
                        reportMapId = "GBT00001_O"
                    Else
                        If isOutFull = True Then
                            reportId = "GBT00001C"
                        Else
                            '一旦画面費用項目をviewstateに退避
                            SaveGridItem(currentTab)
                        End If
                        '画面費用を取得しデータテーブルに格納
                        dt = CollectDisplayCostInfo(currentTab)
                        reportMapId = "GBT00001_C"
                    End If

                    'Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル
                    'Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
                    Dim COA0029XlsTable As New BASEDLL.COA0029XlsTable

                    ''初期処理
                    'errList = New List(Of String)
                    'errListAll = New List(Of String)
                    'Dim returnCode As String = C_MESSAGENO.NORMAL

                    ''UPLOAD_XLSデータ取得
                    COA0029XlsTable.MAPID = reportMapId
                    If isOutFull = True Then
                        COA0029XlsTable.SHEETNAME = tabObject.Value.InnerText & "O" '20101011 インポートは別シート
                    Else
                        Dim sheetId As String = reportMapId.Replace("_", "") & "O"  '20101011 インポートは別シート
                        COA0029XlsTable.SHEETNAME = sheetId
                    End If
                    COA0029XlsTable.COA0029XlsToTable()
                    'COA0029XlsTable.TBLDATA = dt
                    If COA0029XlsTable.ERR = C_MESSAGENO.NORMAL Then
                        outCnt = outCnt + COA0029XlsTable.TBLDATA.Rows.Count
                        'If COA0029XlsTable.TBLDATA.Rows.Count = 0 Then
                        If outCnt = 0 Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage)
                            Return
                        End If
                    Else
                        returnCode = COA0029XlsTable.ERR
                        CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                        Return
                    End If
                    If isOrganizer = True Then
                        'オーガナイザーの場合
                        'TODO organizerインポート処理
                        If COA0029XlsTable.TBLDATA.Rows.Count = 0 Then
                            Return
                        End If
                        Dim excelRetDr As DataRow = COA0029XlsTable.TBLDATA.Rows(0)
                        Dim baseDt As DataTable = CreateOrganizerInfoTable()
                        Dim writeDr As DataRow = baseDt.Rows(0)
                        '書き換え禁止の項目は画面情報を上書き
                        writeDr.Item("ISTRILATERAL") = Me.hdnIsTrilateral.Value
                        writeDr.Item("BRID") = Me.lblBrNo.Text
                        writeDr.Item("USETYPE") = Me.txtBrType.Text
                        writeDr.Item("RECIEPTCOUNTRY1") = Me.txtRecieptCountry1.Text
                        writeDr.Item("RECIEPTPORT1") = Me.txtRecieptPort1.Text
                        writeDr.Item("LOADCOUNTRY1") = Me.txtLoadCountry1.Text
                        writeDr.Item("LOADPORT1") = Me.txtLoadPort1.Text
                        writeDr.Item("DISCHARGECOUNTRY1") = Me.txtDischargeCountry1.Text
                        writeDr.Item("DISCHARGEPORT1") = Me.txtDischargePort1.Text
                        writeDr.Item("DELIVERYCOUNTRY1") = Me.txtDeliveryCountry1.Text
                        writeDr.Item("DELIVERYPORT1") = Me.txtDeliveryPort1.Text
                        writeDr.Item("RECIEPTCOUNTRY2") = Me.txtRecieptCountry2.Text
                        writeDr.Item("RECIEPTPORT2") = Me.txtRecieptPort2.Text
                        writeDr.Item("LOADCOUNTRY2") = Me.txtLoadCountry2.Text
                        writeDr.Item("LOADPORT2") = Me.txtLoadPort2.Text
                        writeDr.Item("DISCHARGECOUNTRY2") = Me.txtDischargeCountry2.Text
                        writeDr.Item("DISCHARGEPORT2") = Me.txtDischargePort2.Text
                        writeDr.Item("DELIVERYCOUNTRY2") = Me.txtDeliveryCountry2.Text
                        writeDr.Item("DELIVERYPORT2") = Me.txtDeliveryPort2.Text
                        writeDr.Item("SHIPPER") = Me.txtShipper.Text
                        writeDr.Item("AMTREQUEST") = Me.txtAmtRequest.Text
                        writeDr.Item("AMTPRINCIPAL") = Me.txtAmtRequest.Text
                        writeDr.Item("AMTDISCOUNT") = Me.txtAmtDiscount.Text
                        writeDr.Item("REMARK") = HttpUtility.HtmlDecode(Me.lblBrRemarkText.Text)
                        writeDr.Item("APPLYTEXT") = HttpUtility.HtmlDecode(Me.lblApplyRemarks.Text)
                        writeDr.Item("APPROVEDTEXT") = HttpUtility.HtmlDecode(Me.lblAppJotRemarks.Text)

                        writeDr.Item("COUNTRYORGANIZER") = Me.hdnCountryOrganizer.Value
                        writeDr.Item("AGENTORGANIZER") = Me.hdnAgentOrganizer.Value
                        writeDr.Item("APPLYDATE") = Me.txtAppRequestYmd.Text
                        writeDr.Item("APPLICANTID") = Me.txtAppSalesPic.Text
                        writeDr.Item("APPLICANTNAME") = HttpUtility.HtmlDecode(Me.lblAppSalesPicText.Text)
                        writeDr.Item("APPROVEDATE") = Me.txtApprovedYmd.Text
                        writeDr.Item("APPROVERID") = Me.txtAppJotPic.Text
                        writeDr.Item("APPROVERNAME") = HttpUtility.HtmlDecode(Me.lblAppJotPicText.Text)

                        'Excelより転送するフィールド名の設定
                        Dim excelCopyFields As New List(Of String) From {"TERMTYPE", "NOOFTANKS",
                                                             "CONSIGNEE", "CARRIER1", "PRODUCTCODE",
                                                             "VSL1", "VOY1", "PRODUCTWEIGHT",
                                                             "LOADING", "STEAMING", "TIP", "EXTRA", "JOTHIREAGE",
                                                             "COMMERCIALFACTOR", "TOTALINVOICED",
                                                             "DEMURTO", "DEMURUSRATE1", "DEMURUSRATE2", "AGENTPOL1", "AGENTPOD1",
                                                             "INVOICEDBY", "FEE", "BILLINGCATEGORY"}
                        Dim excelDateFields As New List(Of String) From {"VALIDITYFROM", "VALIDITYTO", "ETD1", "ETA1"}
                        '3国間の場合のみ3ご区間に関わる入力を取り込む
                        If Me.hdnIsTrilateral.Value = "1" Then
                            excelCopyFields.AddRange({"CARRIER2", "VSL2", "VOY2", "ETD2", "ETA2", "AGENTPOL2", "AGENTPOD2"})
                            excelDateFields.AddRange({"ETD2", "ETA2"})

                        End If
                        '設定したフィールド名をループし画面設定用のデータテーブルに転記
                        For Each excelCopyField As String In excelCopyFields
                            writeDr.Item(excelCopyField) = excelRetDr.Item(excelCopyField)
                        Next
                        For Each excelDateField As String In excelDateFields
                            Dim dateString As String = Convert.ToString(excelRetDr.Item(excelDateField))
                            dateString = FormatDateYMD(dateString, GBA00003UserSetting.DATEFORMAT)
                            Dim dtmTmp As Date
                            If dateString.Trim = "" OrElse Date.TryParse(dateString, dtmTmp) = False Then
                                Continue For
                            End If
                            writeDr.Item(excelDateField) = dtmTmp.ToString("yyyy/MM/dd")
                        Next

                        SetDisplayOrganizerInfo(baseDt, True)
                        CalcDemurrageDay()
                        CalcTotalDays(True)
                        CalcFillingRate()
                        If Convert.ToString(writeDr("TOTALINVOICED")).Trim = "" Then
                            CalcInvoiceTotal()
                        Else
                            CalcHireageCommercialfactor()
                        End If

                    Else
                        'コスト情報の場合
                        Dim inportCostList As New List(Of COSTITEM)
                        For Each dr As DataRow In COA0029XlsTable.TBLDATA.Rows
                            '費用コードが空白の場合はそのまま終了
                            If Convert.ToString(dr.Item("COSTCODE")).Trim = "" Then
                                Continue For
                            End If

                            Dim costitem As New COSTITEM
                            costitem.ItemGroup = currentTab
                            costitem.CostCode = Convert.ToString(dr.Item("COSTCODE")).Trim
                            costitem.BasedOn = Convert.ToString(dr.Item("BASEON")).Trim
                            'costitem.Tax = Convert.ToString(dr.Item("TAX")).Trim
                            costitem.USD = Convert.ToString(dr.Item("USD")).Trim
                            costitem.Local = Convert.ToString(dr.Item("LOCAL")).Trim
                            costitem.ConstractorCode = Convert.ToString(dr.Item("DTLOFFICE")).Trim
                            costitem.LocalCurrncyRate = Convert.ToString(dr.Item("LOCALRATE")).Trim
                            'costitem.USDRate = Convert.ToString(dr.Item("USDRATE")).Trim
                            costitem.IsAddedCost = "1"
                            Dim tmpNum As Decimal
                            Dim countryCode As String = Nothing
                            Select Case currentTab
                                Case COSTITEM.CostItemGroup.Export1
                                    countryCode = Me.txtLoadCountry1.Text.Trim
                                Case COSTITEM.CostItemGroup.Inport1
                                    countryCode = Me.txtDischargeCountry1.Text.Trim
                                Case COSTITEM.CostItemGroup.Export2
                                    countryCode = Me.txtLoadCountry2.Text.Trim
                                Case COSTITEM.CostItemGroup.Inport2
                                    countryCode = Me.txtDischargeCountry2.Text.Trim
                            End Select

                            If Decimal.TryParse(costitem.USD, tmpNum) Then
                                costitem.USD = NumberFormat(tmpNum, countryCode, "", "", "1")
                            End If
                            If Decimal.TryParse(costitem.Local, tmpNum) Then
                                costitem.Local = NumberFormat(tmpNum, countryCode)
                            End If
                            costitem.Class1 = ""
                            costitem.Class3 = ""
                            costitem.Class4 = ""
                            costitem.Class5 = ""
                            costitem.Class6 = ""
                            costitem.Class7 = ""
                            costitem.Class8 = "1"
                            costitem.CountryCode = countryCode
                            costitem.InvoicedBy = ""
                            costitem.Taxation = GetDefaultTaxation(countryCode)
                            '費用項目を取得
                            Dim costDt As DataTable = GetCost(Me.hdnBrType.Value, Convert.ToString(dr.Item("COSTCODE")).Trim, tabId)
                            If costDt Is Nothing OrElse costDt.Rows.Count = 0 Then
                                costitem.ChargeClass4 = ""
                                costitem.ChargeClass8 = CONST_FLAG_NO
                                costitem.Class9 = CONST_FLAG_NO
                            Else
                                Dim costDr As DataRow = costDt.Rows(0)
                                costitem.ChargeClass4 = Convert.ToString(costDr.Item("CHARGECLASS4"))
                                costitem.ChargeClass8 = Convert.ToString(costDr.Item("CHARGECLASS8"))
                                costitem.Class9 = Convert.ToString(costDr.Item("CHARGECLASS9"))
                            End If

                            If Me.txtBillingCategory.Text = GBC_DELIVERYCLASS.CONSIGNEE Then
                                costitem.Billing = "0"
                            Else
                                costitem.Billing = "1"
                            End If

                            inportCostList.Add(costitem)
                        Next
                        '必須費用項目の設定チェック(無い場合は取り込み不可）
                        If HasRequiredCostCode(inportCostList, currentTab) = False Then
                            'If COA0019Session.LANGDISP = C_LANG.JA Then
                            '    Me.lblFooterMessage.Text = "必須費用項目が削除されています。"
                            'Else
                            '    Me.lblFooterMessage.Text = "【英語】必須費用項目が削除されています。"
                            'End If
                            returnCode = C_MESSAGENO.DELETEREQUIREDCOST
                            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                            Return
                        End If
                        'グリッドに表示
                        Dim allCostList As List(Of COSTITEM)
                        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
                        If allCostList Is Nothing Then
                            allCostList = New List(Of COSTITEM)
                        End If
                        'カレントタブを除いた費用一覧に絞り込み最大のuniqueキーを作成
                        Dim costListWithOutCurrentTab As New List(Of COSTITEM)
                        If (From allCostItem In allCostList
                            Where allCostItem.ItemGroup <> currentTab).Any Then
                            costListWithOutCurrentTab = (From allCostItem In allCostList
                                                         Where allCostItem.ItemGroup <> currentTab).ToList
                        End If


                        Dim maxUniqueIndex As Integer = 0

                        Dim maxOrderdUniqueIndex = (From allCostItem In costListWithOutCurrentTab
                                                    Order By allCostItem.UniqueIndex Descending)
                        If maxOrderdUniqueIndex.Any Then
                            maxUniqueIndex = maxOrderdUniqueIndex(0).UniqueIndex + 1
                        End If


                        '費用名称及び、業者名称の設定およびuniqueインデックス付与
                        For Each item As COSTITEM In inportCostList
                            Dim costDt As DataTable = GetCost(Me.hdnBrType.Value, item.CostCode, tabId)
                            If costDt IsNot Nothing AndAlso costDt.Rows.Count > 0 Then
                                item.CostName = Convert.ToString(costDt.Rows(0).Item("NAME"))
                            Else
                                Continue For 'マスタにない費用コードはスキップ
                            End If
                            If item.ConstractorCode <> "" Then
                                Dim targetChargeClass4 As String = Convert.ToString(costDt.Rows(0)("CHARGECLASS4"))
                                Dim targetCountryCode As String = item.CountryCode
                                Dim lbDummyObj As New ListBox
                                Dim GBA00004CountryRelated As New GBA00004CountryRelated
                                Select Case targetChargeClass4
                                    Case GBC_CHARGECLASS4.AGENT
                                        GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                                        GBA00004CountryRelated.LISTBOX_OFFICE = lbDummyObj
                                        GBA00004CountryRelated.GBA00004getLeftListOffice()
                                    Case GBC_CHARGECLASS4.CURRIER
                                        GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                                        GBA00004CountryRelated.LISTBOX_VENDER = lbDummyObj
                                        GBA00004CountryRelated.GBA00004getLeftListVender()
                                    Case GBC_CHARGECLASS4.FORWARDER
                                        GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                                        GBA00004CountryRelated.LISTBOX_FORWARDER = lbDummyObj
                                        GBA00004CountryRelated.GBA00004getLeftListForwarder()
                                    Case GBC_CHARGECLASS4.DEPOT
                                        GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                                        GBA00004CountryRelated.LISTBOX_DEPOT = lbDummyObj
                                        GBA00004CountryRelated.GBA00004getLeftListDepot()
                                    Case GBC_CHARGECLASS4.OTHER
                                        GBA00004CountryRelated.COUNTRYCODE = targetCountryCode
                                        GBA00004CountryRelated.LISTBOX_OTHER = lbDummyObj
                                        GBA00004CountryRelated.GBA00004getLeftListOther()
                                End Select

                                Dim selectedContractorName As String = ""
                                If lbDummyObj IsNot Nothing AndAlso lbDummyObj.Items.Count <> 0 AndAlso
                                   lbDummyObj.Items.FindByValue(item.ConstractorCode) IsNot Nothing Then
                                    Dim selItem = lbDummyObj.Items.FindByValue(item.ConstractorCode)
                                    If selItem.Text.Contains(":") Then
                                        selectedContractorName = Split(selItem.Text, ":", 2)(1)
                                    Else
                                        selectedContractorName = selItem.Text
                                    End If
                                    item.Constractor = selectedContractorName
                                End If

                            End If
                            item.UniqueIndex = maxUniqueIndex
                            If item.IsAddedCost = "1" Then
                                Dim qMaxClass2Excel = (From allCostItem In inportCostList
                                                       Where allCostItem.Class2 <> "" AndAlso IsNumeric(allCostItem.Class2)
                                                       Order By CInt(allCostItem.Class2) Descending)

                                Dim qMaxClass2HdnDisp = (From allCostItem In costListWithOutCurrentTab
                                                         Where allCostItem.Class2 <> "" AndAlso IsNumeric(allCostItem.Class2)
                                                         Order By CInt(allCostItem.Class2) Descending)

                                Dim maxClass2Excel As Integer = 0
                                Dim maxClaas2HdnDisp As Integer = 0
                                Dim maxClass2 As Integer = 0
                                If qMaxClass2Excel.Any Then
                                    maxClass2Excel = CInt(qMaxClass2Excel(0).Class2) + 1
                                End If
                                If qMaxClass2HdnDisp.Any Then
                                    maxClaas2HdnDisp = CInt(qMaxClass2HdnDisp(0).Class2) + 1
                                End If
                                maxClass2 = maxClass2Excel
                                If maxClass2Excel < maxClaas2HdnDisp Then
                                    maxClass2 = maxClaas2HdnDisp
                                End If
                                item.Class2 = Convert.ToString(maxClass2)
                            End If

                            maxUniqueIndex = maxUniqueIndex + 1
                            costListWithOutCurrentTab.Add(item)
                            If item.SortOrder = "" Then
                                item.SortOrder = "0"
                            End If
                        Next
                        ViewState("COSTLIST") = costListWithOutCurrentTab
                        Dim showCostList = (From allCostItem In costListWithOutCurrentTab
                                            Where allCostItem.ItemGroup = currentTab
                                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
                        Me.gvDetailInfo.DataSource = showCostList
                        Me.gvDetailInfo.DataBind()
                    End If ' END isOrganizer
                    '費用項目非活性制御
                    CostEnabledControls()

                    If isOutFull = False Then
                        Exit For
                    End If
                End If
            End If
        Next

        If returnCode = C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALUPLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        End If

    End Sub

    ''' <summary>
    ''' 費用項目必須チェック
    ''' </summary>
    ''' <param name="costList"></param>
    ''' <param name="currentTab"></param>
    ''' <returns></returns>
    Private Function HasRequiredCostCode(costList As List(Of COSTITEM), currentTab As COSTITEM.CostItemGroup) As Boolean
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT COSTCODE")
        sqlStat.AppendLine("      ,ACTIONID")
        sqlStat.AppendLine("      ,CLASS1")
        sqlStat.AppendLine("      ,CLASS2")
        sqlStat.AppendLine("      ,CLASS3")
        sqlStat.AppendLine("      ,CLASS4")
        sqlStat.AppendLine("      ,CLASS5")
        sqlStat.AppendLine("      ,CLASS6")
        sqlStat.AppendLine("      ,CLASS7")
        sqlStat.AppendLine("  FROM GBM0009_TRPATTERN")
        sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE")
        sqlStat.AppendLine("   AND ORG      = @ORG")
        sqlStat.AppendLine("   AND BRTYPE   = @BRTYPE")
        sqlStat.AppendLine("   AND USETYPE  = @USETYPE")
        sqlStat.AppendLine("   AND AGENTKBN = @AGENTKBN")
        sqlStat.AppendLine("   AND STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
        Dim agentKbn As String = ""
        Select Case currentTab
            Case COSTITEM.CostItemGroup.Export1
                agentKbn = "POL1"
            Case COSTITEM.CostItemGroup.Inport1
                agentKbn = "POD1"
            Case COSTITEM.CostItemGroup.Export2
                agentKbn = "POL2"
            Case COSTITEM.CostItemGroup.Inport2
                agentKbn = "POD2"
        End Select
        'DB接続
        Dim trpatternDt As New DataTable
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            Dim brTypeVal As String = C_BRTYPE.OPERATION
            If Me.hdnBrType.Value = "1" Then
                brTypeVal = C_BRTYPE.SALES
            End If
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 10).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@ORG", SqlDbType.NVarChar, 10).Value = "GB_Default" '一旦GB_Default固定
                .Add("@BRTYPE", SqlDbType.NVarChar, 10).Value = brTypeVal
                .Add("@USETYPE", SqlDbType.NVarChar, 10).Value = txtBrType.Text
                .Add("@AGENTKBN", SqlDbType.NVarChar, 10).Value = agentKbn
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(trpatternDt)
            End Using
        End Using
        '取り込まれたExcelに必須費用コードが記載されているか確認
        Dim demList As List(Of String) = GetDemurrageList()
        Dim dicAddedCost As New Dictionary(Of String, String)
        Dim sortNum As Integer = 0
        For Each dr As DataRow In trpatternDt.Rows
            Dim expression As String = String.Format("COSTCODE='{0}'", Convert.ToString(dr.Item("COSTCODE")).Replace("'", "''"))
            Dim findCostCode As String = Convert.ToString(dr.Item("COSTCODE"))
            If demList.IndexOf(findCostCode) <> -1 Then
                Continue For
            End If
            Dim findResult As List(Of COSTITEM) = (From x In costList Where x.CostCode = findCostCode).ToList
            If findResult Is Nothing OrElse findResult.Count = 0 Then
                Return False
            End If
            '同一費用コードは最初に見つかった費用コードを定型費用コードとして判定
            If dicAddedCost.ContainsKey(Convert.ToString(dr.Item("COSTCODE"))) Then
                findResult(0).IsAddedCost = "1"
                findResult(0).ActionId = Convert.ToString(dr.Item("ACTIONID"))
                findResult(0).Class1 = Convert.ToString(dr.Item("CLASS1"))
                findResult(0).Class2 = Convert.ToString(dr.Item("CLASS2"))
                findResult(0).Class3 = Convert.ToString(dr.Item("CLASS3"))
                findResult(0).Class4 = Convert.ToString(dr.Item("CLASS4"))
                findResult(0).Class5 = Convert.ToString(dr.Item("CLASS5"))
                findResult(0).Class6 = Convert.ToString(dr.Item("CLASS6"))
                findResult(0).Class7 = Convert.ToString(dr.Item("CLASS7"))
                findResult(0).Class8 = "1"
                findResult(0).SortOrder = "0"
            Else
                dicAddedCost.Add(Convert.ToString(dr.Item("COSTCODE")), "")
                findResult(0).IsAddedCost = "0"
                findResult(0).ActionId = Convert.ToString(dr.Item("ACTIONID"))
                findResult(0).Class1 = Convert.ToString(dr.Item("CLASS1"))
                findResult(0).Class2 = Convert.ToString(dr.Item("CLASS2"))
                findResult(0).Class3 = Convert.ToString(dr.Item("CLASS3"))
                findResult(0).Class4 = Convert.ToString(dr.Item("CLASS4"))
                findResult(0).Class5 = Convert.ToString(dr.Item("CLASS5"))
                findResult(0).Class6 = Convert.ToString(dr.Item("CLASS6"))
                findResult(0).Class7 = Convert.ToString(dr.Item("CLASS7"))
                findResult(0).Class8 = "0"
                findResult(0).SortOrder = sortNum.ToString
                sortNum = sortNum + 1
            End If

        Next
        Return True
    End Function
    ''' <summary>
    ''' グリッド表示用のコストアイテムクラス
    ''' </summary>
    <Serializable>
    Public Class COSTITEM
        Public Enum CostItemGroup As Integer
            ''' <summary>
            ''' 輸出1(Export)
            ''' </summary>
            Export1 = 0
            ''' <summary>
            ''' 輸入1(Inport)
            ''' </summary>
            Inport1 = 1
            ''' <summary>
            ''' 輸出2(Export)
            ''' </summary>
            Export2 = 2
            ''' <summary>
            ''' 輸入2(Inport)
            ''' </summary>
            Inport2 = 3
            ''' <summary>
            ''' オーガナイザ(費用計算では利用しない)
            ''' </summary>
            Organizer = 9999
        End Enum
        ''' <summary>
        ''' どのコストに属するか（輸出1,輸入1,輸出2,輸入2)
        ''' </summary>
        ''' <returns></returns>
        Public Property ItemGroup As CostItemGroup = 0
        ''' <summary>
        ''' 費用コード
        ''' </summary>
        ''' <returns></returns>
        Public Property CostCode As String = ""
        ''' <summary>
        ''' 費用名称
        ''' </summary>
        ''' <returns></returns>
        Public Property CostName As String = ""
        ''' <summary>
        ''' 数量
        ''' </summary>
        ''' <returns></returns>
        Public Property BasedOn As String = ""
        '''' <summary>
        '''' 税
        '''' </summary>
        '''' <returns></returns>
        'Public Property Tax As String = ""
        ''' <summary>
        ''' 課税区分(1:課税,0:非課税)
        ''' </summary>
        ''' <returns></returns>
        Public Property Taxation As String = ""
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
        ''' 業者コード
        ''' </summary>
        ''' <returns></returns>
        Public Property ConstractorCode As String = ""
        ''' <summary>
        ''' 業者名
        ''' </summary>
        ''' <returns></returns>
        Public Property Constractor As String = ""
        '''' <summary>
        '''' US＄換算RATE
        '''' </summary>
        '''' <returns></returns>
        'Public Property USDRate As String = ""
        ''' <summary>
        ''' 現地通貨換算RATE
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
        Public Property ChargeClass4 As String = ""
        ''' <summary>
        ''' US$入力
        ''' </summary>
        ''' <returns></returns>
        Public Property ChargeClass8 As String = ""
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
        ''' アクションコード
        ''' </summary>
        ''' <returns></returns>
        Public Property ActionId As String = ""
        ''' <summary>
        ''' 遂行順序
        ''' </summary>
        ''' <returns></returns>
        Public Property Class1 As String = ""
        ''' <summary>
        ''' 予定日付参照
        ''' </summary>
        ''' <returns></returns>
        Public Property Class3 As String = ""
        ''' <summary>
        ''' 予定日付加減算日数
        ''' </summary>
        ''' <returns></returns>
        Public Property Class4 As String = ""
        ''' <summary>
        ''' 輸送完了作業
        ''' </summary>
        ''' <returns></returns>
        Public Property Class5 As String = ""
        ''' <summary>
        ''' 必須作業
        ''' </summary>
        ''' <returns></returns>
        Public Property Class6 As String = ""
        ''' <summary>
        ''' 起点終点
        ''' </summary>
        ''' <returns></returns>
        Public Property Class7 As String = ""
        ''' <summary>
        ''' コスト追加フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property Class8 As String = ""

        ''' <summary>
        ''' per B/L
        ''' </summary>
        ''' <returns></returns>
        Public Property Class9 As String = ""

        ''' <summary>
        ''' InvoicedBy
        ''' </summary>
        ''' <returns></returns>
        Public Property InvoicedBy As String = ""

        ''' <summary>
        ''' Billing
        ''' </summary>
        ''' <returns></returns>
        Public Property Billing As String = ""

        ''' <summary>
        ''' 国コード
        ''' </summary>
        ''' <returns></returns>
        Public Property CountryCode As String = ""
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
        ''' リースタンク利用
        ''' </summary>
        ''' <returns></returns>
        Public Property UsingLeaseTank As String = ""
        ''' <summary>
        ''' タイムスタンプ
        ''' </summary>
        ''' <returns></returns>
        Public Property TimeStamp As String = ""
        ''' <summary>
        ''' 更新日
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdYmd As String = ""
        ''' <summary>
        ''' 更新ユーザー
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdUser As String = ""
        ''' <summary>
        ''' 更新端末
        ''' </summary>
        ''' <returns></returns>
        Public Property UpdTermId As String = ""
        ''' <summary>
        ''' 現在の申請・承認ステータス
        ''' </summary>
        ''' <returns></returns>
        Public Property AppStatus As String = ""
    End Class
    ''' <summary>
    ''' InputRequestの各発着の情報を保持
    ''' </summary>
    Private Class InputRequestValue
        ''' <summary>
        ''' Type(POL1,POD1等)
        ''' </summary>
        ''' <returns></returns>
        Public Property [Type] As String = ""
        ''' <summary>
        ''' RequestFlg 
        ''' </summary>
        ''' <returns></returns>
        Public Property RequestFlg As Boolean = False
        ''' <summary>
        ''' メール送信フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property MailFlg As Boolean = False
        ''' <summary>
        ''' ステータス
        ''' </summary>
        ''' <returns></returns>
        Public Property Status As String = ""
        ''' <summary>
        ''' ステータス保持用Hidden項目オブジェクト
        ''' </summary>
        ''' <returns></returns>
        Public Property HdnStatusObj As HiddenField = Nothing
        ''' <summary>
        ''' イベントコード
        ''' </summary>
        ''' <returns></returns>
        Public Property EventCode As String = ""
        ''' <summary>
        ''' サブコード
        ''' </summary>
        ''' <returns></returns>
        Public Property SubCode As String = ""
        ''' <summary>
        ''' 第n輸送
        ''' </summary>
        ''' <returns></returns>
        Public Property BrRound As String = ""
        ''' <summary>
        ''' 申請ID
        ''' </summary>
        ''' <returns></returns>
        Public Property ApplyId As String = ""
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
    ''' FillingRate自動計算
    ''' </summary>
    Public Sub CalcFillingRate()
        Me.txtTankFillingRate.Text = ""
        Me.txtTankFillingCheck.Text = ""
        Me.txtTankFillingCheck.CssClass = "aspNetDisabled"

        'カレント国コード取得
        Dim countryCode As String = getCountryCode()

        Dim dummyDec As Decimal
        For Each txtObj As TextBox In {Me.txtWeight, Me.txtSGravity, Me.txtTankCapacity}
            If txtObj.Text.Trim = "" OrElse Decimal.TryParse(txtObj.Text.Trim, dummyDec) = False Then
                Return
            End If
        Next
        Dim prpvisions As String = Me.hdnPrpvisions.Value
        Dim isHazard As String = Me.hdnProductIsHazard.Value
        Dim weight As Decimal = RoundDown(DecimalStringToDecimal(Me.txtWeight.Text), 0)
        Me.txtWeight.Text = NumberFormat(weight, "", "#,##0")
        Dim gravity As Decimal = DecimalStringToDecimal(Me.txtSGravity.Text)
        Dim capacity As Decimal = 0

        capacity = RoundDown(DecimalStringToDecimal(Me.txtTankCapacity.Text), 0)
        Me.txtTankCapacity.Text = NumberFormat(capacity, "", "#,##0")

        If capacity = 0 OrElse gravity = 0 Then
            Return
        End If

        Dim fillingRate As Decimal = weight / (capacity * gravity) * 100
        'Me.txtTankFillingRate.Text = NumberFormat(fillingRate, countryCode) & "%"
        Me.txtTankFillingRate.Text = fillingRate.ToString("#,##0.00") & "%"
        Dim highValue As Decimal = 95
        Dim lowValue As Decimal = 70

        If prpvisions.Contains(PRODUCT_TP33) Then
            highValue = 100
            lowValue = 0.001D
        ElseIf isHazard = "1" Then
            highValue = 95
            lowValue = 80
        ElseIf gravity <= 0 Then
            highValue = 95
            lowValue = -0.001D
        End If
        If fillingRate < lowValue OrElse fillingRate > highValue Then
            Me.txtTankFillingCheck.Text = "ERROR"
            Me.txtTankFillingCheck.CssClass = "aspNetDisabled error"
        Else
            Me.txtTankFillingCheck.Text = "CLEAR!"
            Me.txtTankFillingCheck.CssClass = "aspNetDisabled clear"
        End If
    End Sub
    ''' <summary>
    ''' 日数計算処理
    ''' </summary>
    Public Sub CalcTotalDays(Optional isRelateCalc As Boolean = False)
        Dim totalDays As Decimal
        Dim loading As Decimal
        Dim steaming As Decimal
        Dim tip As Decimal
        Dim extra As Decimal
        Me.txtTotal.Text = ""
        Dim dummyDec As Decimal
        For Each txtObj As TextBox In {Me.txtLoading, Me.txtSteaming, Me.txtTip, Me.txtExtra}
            If txtObj.Text.Trim <> "" AndAlso Decimal.TryParse(txtObj.Text.Trim, dummyDec) = False Then
                Return
            End If
        Next

        loading = If(Me.txtLoading.Text.Trim = "", 0, RoundDown(DecimalStringToDecimal(Me.txtLoading.Text.Trim), 0))
        Me.txtLoading.Text = If(Me.txtLoading.Text.Trim = "", "", NumberFormat(loading, "", "#,##0"))
        steaming = If(Me.txtSteaming.Text.Trim = "", 0, RoundDown(DecimalStringToDecimal(Me.txtSteaming.Text.Trim), 0))
        Me.txtSteaming.Text = If(Me.txtSteaming.Text.Trim = "", "", NumberFormat(steaming, "", "#,##0"))
        tip = If(Me.txtTip.Text.Trim = "", 0, RoundDown(DecimalStringToDecimal(Me.txtTip.Text.Trim), 0))
        Me.txtTip.Text = If(Me.txtTip.Text.Trim = "", "", NumberFormat(tip, "", "#,##0"))
        extra = If(Me.txtExtra.Text.Trim = "", 0, RoundDown(DecimalStringToDecimal(Me.txtExtra.Text.Trim), 0))
        Me.txtExtra.Text = If(Me.txtExtra.Text.Trim = "", "", NumberFormat(extra, "", "#,##0"))

        totalDays = loading + steaming + tip + extra
        Me.txtTotal.Text = NumberFormat(totalDays, "", "#,##0")
        If isRelateCalc = False Then
            CalcHireageCommercialfactor()
        End If
    End Sub
    ''' <summary>
    ''' JOT売上情報・総額算出(JOTHIREAGE COMMERCIALFACTOR変更時に反映)
    ''' </summary>
    Public Sub CalcInvoiceTotal()

        'カレント国コード取得
        Dim countryCode As String = getCountryCode()

        Dim dummyDec As Decimal
        For Each txtObj As TextBox In {Me.txtTotalCost, Me.txtJOTHireage, Me.txtCommercialFactor, Me.txtTotal}
            If txtObj.Text.Trim <> "" AndAlso Decimal.TryParse(txtObj.Text.Trim, dummyDec) = False Then
                Return
            End If
        Next
        '計算項目の取得
        Dim totalCost As Decimal = DecimalStringToDecimal(Me.txtTotalCost.Text)
        Dim jotHireage As Decimal = DecimalStringToDecimal(Me.txtJOTHireage.Text)
        jotHireage = RoundDown(jotHireage)
        Me.txtJOTHireage.Text = NumberFormat(jotHireage, countryCode, "", "", "1")
        Dim commercialFactor As Decimal = DecimalStringToDecimal(Me.txtCommercialFactor.Text)
        commercialFactor = RoundDown(commercialFactor)
        Me.txtCommercialFactor.Text = NumberFormat(commercialFactor, countryCode, "", "", "1")

        Dim totalSpan As Decimal = DecimalStringToDecimal(Me.txtTotal.Text)
        'INVOICED TOTALを計算
        Dim invoiceTotal As Decimal
        invoiceTotal = jotHireage + commercialFactor + totalCost
        invoiceTotal = RoundDown(invoiceTotal)
        Me.txtInvoicedTotal.Text = NumberFormat(invoiceTotal, countryCode, "", "", "1")
        'PAR DAYを計算
        Dim parDay As Decimal
        If totalSpan = 0 Then
            Me.txtPerDay.Text = ""
            Return
        End If
        parDay = (invoiceTotal - totalCost) / totalSpan
        parDay = RoundDown(parDay)
        Me.txtPerDay.Text = NumberFormat(parDay, countryCode, "", "", "1")

        If txtAmtRequest.Text = "0" AndAlso txtAmtPrincipal.Text = "0" Then
            Me.txtAmtDiscount.Text = "0"
        Else
            If txtAmtRequest.Text <> "0" AndAlso txtAmtPrincipal.Text = "0" Then
                Dim amtReq As Decimal = Nothing
                If Decimal.TryParse(Me.txtAmtRequest.Text, amtReq) Then
                    Dim invTotal As Decimal = Nothing
                    If Decimal.TryParse(Me.txtInvoicedTotal.Text, invTotal) Then
                        Me.txtAmtDiscount.Text = (amtReq - invTotal).ToString
                    End If
                End If
            Else
                Dim amtPrin As Decimal = Nothing
                If Decimal.TryParse(Me.txtAmtPrincipal.Text, amtPrin) Then
                    Dim invTotal As Decimal = Nothing
                    If Decimal.TryParse(Me.txtInvoicedTotal.Text, invTotal) Then
                        Me.txtAmtDiscount.Text = (amtPrin - invTotal).ToString
                    End If
                End If
            End If
        End If

    End Sub
    ''' <summary>
    ''' 総額よりJOT総額、調整、総額 自動計算をする
    ''' 総額変更時に実行
    ''' </summary>
    Public Sub CalcHireageCommercialfactor()
        Me.txtJOTHireage.Text = ""
        Me.txtCommercialFactor.Text = ""
        Me.txtPerDay.Text = ""

        'カレント国コード取得
        Dim countryCode As String = getCountryCode()

        '必要項目に入力がない場合は終了
        Dim dummyDec As Decimal
        For Each txtObj As TextBox In {Me.txtTotalCost, Me.txtInvoicedTotal, Me.txtTotal}
            If txtObj.Text.Trim = "" OrElse Decimal.TryParse(txtObj.Text.Trim, dummyDec) = False Then
                Return
            End If
        Next
        Dim totalCost As Decimal = DecimalStringToDecimal(Me.txtTotalCost.Text)
        Dim invoiceTotal As Decimal = RoundDown(DecimalStringToDecimal(Me.txtInvoicedTotal.Text))
        Me.txtInvoicedTotal.Text = NumberFormat(invoiceTotal, countryCode, "", "", "1")
        Dim totalSpan As Decimal = DecimalStringToDecimal(Me.txtTotal.Text)
        If totalSpan = 0 Then
            Return
        End If
        Dim hireagePerDay As Decimal = RoundDown((invoiceTotal - totalCost) / totalSpan)
        Me.txtPerDay.Text = NumberFormat(hireagePerDay, countryCode, "", "", "1")
        Dim jotHireage As Decimal = RoundDown(totalSpan * hireagePerDay)
        Me.txtJOTHireage.Text = NumberFormat(jotHireage, countryCode, "", "", "1")
        Dim commercialFactor As Decimal = RoundDown(invoiceTotal - totalCost - jotHireage)
        Me.txtCommercialFactor.Text = NumberFormat(commercialFactor, countryCode, "", "", "1")

        If txtAmtRequest.Text = "0" AndAlso txtAmtPrincipal.Text = "0" Then
            Me.txtAmtDiscount.Text = "0"
        Else
            If txtAmtRequest.Text <> "0" AndAlso txtAmtPrincipal.Text = "0" Then
                Dim amtReq As Decimal = Nothing
                If Decimal.TryParse(Me.txtAmtRequest.Text, amtReq) Then
                    Dim invTotal As Decimal = Nothing
                    If Decimal.TryParse(Me.txtInvoicedTotal.Text, invTotal) Then
                        Me.txtAmtDiscount.Text = (amtReq - invTotal).ToString
                    End If
                End If
            Else
                Dim amtPrin As Decimal = Nothing
                If Decimal.TryParse(Me.txtAmtPrincipal.Text, amtPrin) Then
                    Dim invTotal As Decimal = Nothing
                    If Decimal.TryParse(Me.txtInvoicedTotal.Text, invTotal) Then
                        Me.txtAmtDiscount.Text = (amtPrin - invTotal).ToString
                    End If
                End If
            End If
        End If

    End Sub
    ''' <summary>
    ''' Demurrage翌日を自動計算
    ''' </summary>
    Public Sub CalcDemurrageDay()
        Me.txtDemurdayF1.Text = "1"
        Me.txtDemurday2.Text = ""
        '必要項目に入力がない場合は終了
        Dim dummyDec As Decimal
        For Each txtObj As TextBox In {Me.txtDemurdayT1}
            If txtObj.Text.Trim = "" OrElse Decimal.TryParse(txtObj.Text.Trim, dummyDec) = False Then
                Return
            End If
        Next
        Dim daysToNum As Decimal = 0
        daysToNum = RoundDown(DecimalStringToDecimal(Me.txtDemurdayT1.Text), 0)
        txtDemurdayT1.Text = NumberFormat(daysToNum, "", "#,##0")
        daysToNum = daysToNum + 1
        Me.txtDemurday2.Text = NumberFormat(daysToNum, "", "#,##0")
    End Sub
    ''' <summary>
    ''' 費用タブローカルコスト変更時
    ''' </summary>
    Public Sub CalcSummaryCostLocal()
        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab = COSTITEM.CostItemGroup.Organizer Then
            Return 'ありえないが念のため
        End If
        '画面の入力値をクラスに配置
        SaveGridItem(currentTab)
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        For Each cstItm In costData
            If cstItm.Local = "" Then
                cstItm.Local = "0"
            End If
        Next

        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab AndAlso costItemRow.Local.Trim <> "" AndAlso IsNumeric(costItemRow.Local)).ToList

        'レート桁、端数制御取得
        Dim countryCode As String = Nothing
        Select Case currentTab
            Case COSTITEM.CostItemGroup.Export1
                countryCode = Me.txtLoadCountry1.Text.Trim
            Case COSTITEM.CostItemGroup.Inport1
                countryCode = Me.txtDischargeCountry1.Text.Trim
            Case COSTITEM.CostItemGroup.Export2
                countryCode = Me.txtLoadCountry2.Text.Trim
            Case COSTITEM.CostItemGroup.Inport2
                countryCode = Me.txtDischargeCountry2.Text.Trim
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
            If item.Local = "" Then
                item.Local = "0"
            End If

            If IsNumeric(item.Local) Then
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
                        Case GBC_ROUNDFLG.UP
                            item.LocalCurrncyRate = Convert.ToString(RoundUp(Decimal.Parse(item.LocalCurrncyRate), CUInt(dr.Item("RATEDECIMALPLACES"))))
                        Case GBC_ROUNDFLG.DOWN
                            item.LocalCurrncyRate = Convert.ToString(RoundDown(Decimal.Parse(item.LocalCurrncyRate), CInt(dr.Item("RATEDECIMALPLACES"))))
                        Case GBC_ROUNDFLG.ROUND
                            item.LocalCurrncyRate = Convert.ToString(Round(Decimal.Parse(item.LocalCurrncyRate), CUInt(dr.Item("RATEDECIMALPLACES"))))
                    End Select

                    Dim decPlace As Integer = 0
                    Dim roundFlg As String = ""
                    If GetDecimalPlaces(decPlace, roundFlg) Then
                        Select Case roundFlg
                            Case GBC_ROUNDFLG.UP
                                item.USD = Convert.ToString(RoundUp(Decimal.Parse(item.USD), CUInt(decPlace)))
                            Case GBC_ROUNDFLG.DOWN
                                item.USD = Convert.ToString(RoundDown(Decimal.Parse(item.USD), decPlace))
                            Case GBC_ROUNDFLG.ROUND
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
        Dim summary As Decimal = targetCostData.Sum(Function(item) Decimal.Parse(item.USD))

        '合計欄に値表示
        Me.iptAgencySummaryUsd.Value = NumberFormat(summary, countryCode, "", "", "1")

        Me.hdnCurrentUnieuqIndex.Value = ""

        ViewState("COSTLIST") = costData
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
    Public Sub CalcSummaryCostUsd()
        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab = COSTITEM.CostItemGroup.Organizer Then
            Return 'ありえないが念のため
        End If

        'レート桁、端数制御取得
        Dim countryCode As String = Nothing
        Select Case currentTab
            Case COSTITEM.CostItemGroup.Export1
                countryCode = Me.txtLoadCountry1.Text.Trim
            Case COSTITEM.CostItemGroup.Inport1
                countryCode = Me.txtDischargeCountry1.Text.Trim
            Case COSTITEM.CostItemGroup.Export2
                countryCode = Me.txtLoadCountry2.Text.Trim
            Case COSTITEM.CostItemGroup.Inport2
                countryCode = Me.txtDischargeCountry2.Text.Trim
        End Select

        '画面の入力値をクラスに配置
        SaveGridItem(currentTab)
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        For Each cstItm In costData
            If cstItm.USD = "" Then
                cstItm.USD = "0"
            End If
        Next

        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab AndAlso costItemRow.USD.Trim <> "" AndAlso IsNumeric(costItemRow.USD)).ToList
        '数値のカンマ編集
        For Each item In targetCostData
            If item.USD = "" Then
                item.USD = "0"
            End If

            If IsNumeric(item.USD) Then
                item.USD = NumberFormat(DecimalStringToDecimal(item.USD), countryCode, "", "", "1")
            End If
        Next
        '絞り込んだリストを集計
        Dim summary As Decimal = targetCostData.Sum(Function(item) Decimal.Parse(item.USD))
        '合計欄に値表示
        Me.iptAgencySummaryUsd.Value = NumberFormat(summary, countryCode, "", "", "1")
        Me.hdnCurrentUnieuqIndex.Value = ""
        ViewState("COSTLIST") = costData
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
    ''' <remarks>ローカル⇔USDの使用可否制御</remarks>
    Public Sub CostEnabledControls()
        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        If currentTab = COSTITEM.CostItemGroup.Organizer Then
            Return 'ありえないが念のため
        End If
        '画面の入力値をクラスに配置
        SaveGridItem(currentTab)
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim targetCostData = (From costItemRow In costData
                              Where costItemRow.ItemGroup = currentTab).ToList

        Dim localColIdx As Integer = 0
        Dim usdColIdx As Integer = 0
        For colIdx As Integer = 0 To Me.gvDetailInfo.Columns.Count - 1
            Dim gvRow = Me.gvDetailInfo.Columns(colIdx)
            If gvRow.HeaderStyle.CssClass = "LocalCell" Then
                localColIdx = colIdx
            ElseIf gvRow.HeaderStyle.CssClass = "USDCell" Then
                usdColIdx = colIdx
            End If
        Next

        For i As Integer = 0 To targetCostData.Count - 1
            Dim costDataItem = targetCostData.Item(i)
            Dim gvRow = Me.gvDetailInfo.Rows(i)
            'LOCAL入力制御
            If CDec(txtLocalRateRef.Text) = 0 Then
                gvRow.Cells(localColIdx).Enabled = False
            Else
                gvRow.Cells(localColIdx).Enabled = True
            End If

            If costDataItem.ChargeClass8 = CONST_FLAG_YES Then
                gvRow.Cells(localColIdx).Enabled = False
            End If

            'USD制御
            If costDataItem.Local <> "" AndAlso IsNumeric(costDataItem.Local) AndAlso CDec(costDataItem.Local) <> 0 Then
                gvRow.Cells(usdColIdx).Enabled = False
            Else
                gvRow.Cells(usdColIdx).Enabled = True
            End If
        Next i
    End Sub
    ''' <summary>
    ''' 切り捨て関数
    ''' </summary>
    ''' <param name="value">値</param>
    ''' <param name="digits">IN：省略可能 省略時はセッション変数の対象桁数を取得</param>
    ''' <returns></returns>
    Private Function RoundDown(value As Decimal, Optional digits As Integer = Integer.MinValue) As Decimal

        If digits = Integer.MinValue Then
            'digits = 2 'セッション変数の桁数
            Dim decPlace As Integer = 0
            Dim round As String = ""
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
    ''' LOCALRATE変更時
    ''' </summary>
    Public Sub txtLocalRateRef_Change()

        'レート反映
        If Me.txtLocalRateRef.Text <> "" AndAlso IsNumeric(Me.txtLocalRateRef.Text) Then

            For i As Integer = 0 To gvDetailInfo.Rows.Count - 1

                DirectCast(gvDetailInfo.Rows(i).FindControl("txtLocalRate"), System.Web.UI.WebControls.TextBox).Text = Me.txtLocalRateRef.Text

            Next

        End If

        '費用タブコスト変更処理
        CalcSummaryCostLocal()

        '費用項目非活性制御
        CostEnabledControls()

    End Sub

    ''' <summary>
    ''' 値引申請額変更時イベント
    ''' </summary>
    Public Sub txtAmtRequest_Change()

        Dim amtReq As Decimal = Nothing

        If Me.txtAmtRequest.Text = "" Then
            Me.txtAmtRequest.Text = "0"
        End If

        If Me.txtAmtRequest.Text = "0" Then
            If Me.txtAmtPrincipal.Text = "0" Then
                Me.txtAmtDiscount.Text = "0"
            End If
            Return
        End If

        If Decimal.TryParse(Me.txtAmtRequest.Text, amtReq) Then

            Dim invTotal As Decimal = Nothing
            If Decimal.TryParse(Me.txtInvoicedTotal.Text, invTotal) Then
                Me.txtAmtDiscount.Text = (amtReq - invTotal).ToString
            End If
        End If

    End Sub
    ''' <summary>
    ''' 値引き確定額変更時イベント
    ''' </summary>
    Public Sub txtAmtPrincipal_Change()

        Dim amtPrin As Decimal = Nothing

        If Me.txtAmtPrincipal.Text = "" Then
            Me.txtAmtPrincipal.Text = "0"
        End If

        If Me.txtAmtPrincipal.Text = "0" Then
            If Me.txtAmtRequest.Text = "0" Then
                Me.txtAmtDiscount.Text = "0"
            End If
            Return
        End If

        If Decimal.TryParse(Me.txtAmtPrincipal.Text, amtPrin) Then

            Dim invTotal As Decimal = Nothing
            If Decimal.TryParse(Me.txtInvoicedTotal.Text, invTotal) Then
                Me.txtAmtDiscount.Text = (amtPrin - invTotal).ToString
            End If
        End If

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
    ''' カレントタブ国コード取得
    ''' </summary>
    ''' <returns>国コード</returns>
    Public Function getCountryCode() As String

        'カレントタブ取得
        Dim currentTab = GetCurrentTab()

        'レート桁、端数制御取得
        Dim countryCode As String = Nothing
        Select Case currentTab
            Case COSTITEM.CostItemGroup.Export1
                countryCode = Me.txtLoadCountry1.Text.Trim
            Case COSTITEM.CostItemGroup.Inport1
                countryCode = Me.txtDischargeCountry1.Text.Trim
            Case COSTITEM.CostItemGroup.Export2
                countryCode = Me.txtLoadCountry2.Text.Trim
            Case COSTITEM.CostItemGroup.Inport2
                countryCode = Me.txtDischargeCountry2.Text.Trim
            Case COSTITEM.CostItemGroup.Organizer
                countryCode = Me.hdnCountryOrganizer.Value
        End Select

        Return countryCode

    End Function

    ''' <summary>
    ''' Contractor変更時
    ''' </summary>
    Public Sub CalcContractor()

        Dim carrierCode As String = ""
        Dim currentTab = GetCurrentTab(COSTITEM.CostItemGroup.Export1)
        If currentTab = COSTITEM.CostItemGroup.Organizer Then
            currentTab = COSTITEM.CostItemGroup.Export1
        End If

        '入力内容保持
        SaveGridItem(currentTab)

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))

        Dim uniqueIndex As Integer = 0
        Integer.TryParse(Me.hdnCurrentUnieuqIndex.Value, uniqueIndex)
        Dim changeCostList = (From allCostItem In allCostList
                              Where allCostItem.UniqueIndex = uniqueIndex).ToList

        If changeCostList IsNot Nothing AndAlso changeCostList.Count > 0 Then

            changeCostList(0).Constractor = ""
            carrierCode = changeCostList(0).ConstractorCode

        End If

        ViewState("COSTLIST") = allCostList
        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.ItemGroup = currentTab
                            Order By allCostItem.IsAddedCost, Convert.ToInt32(If(allCostItem.Class2 = "", "0", allCostItem.Class2))).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        If carrierCode = "" Then
            Return
        End If

        SetDisplayContractor(carrierCode)

    End Sub

    ''' <summary>
    ''' Demurrage取得
    ''' </summary>
    Public Function GetDemurrageList() As List(Of String)
        Dim retList As List(Of String) = New List(Of String)

        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = C_FIXVALUECLAS.BREX
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
        COA0017FixValue.CLAS = C_FIXVALUECLAS.USD_DECIMALPLACES
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
    ''' グリッドビューDataBindイベント
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub gvDetailInfo_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles gvDetailInfo.RowDataBound
        If e.Row.RowType <> DataControlRowType.DataRow Then
            Return
        End If

        'B/L
        Dim chk As CheckBox = DirectCast(e.Row.FindControl("chkBl"), CheckBox)
        If chk IsNot Nothing Then
            If chk.Text = CONST_FLAG_YES Then
                chk.Checked = True
            Else
                chk.Checked = False
            End If
        End If

        For Each chkField In {"chkJOT", "chkSC", "chkTaxation"}
            Dim chkObj As CheckBox = DirectCast(e.Row.FindControl(chkField), CheckBox)
            If chkObj IsNot Nothing Then
                If chkObj.Text = "1" Then
                    chkObj.Checked = True
                Else
                    chkObj.Checked = False
                End If
            End If
        Next
    End Sub
    ''' <summary>
    ''' Capacity取得
    ''' </summary>
    Public Function GetCapacity() As String

        Dim COA0017FixValue As New COA0017FixValue
        Dim retVal As String = ""

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CAPACITY"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            retVal = COA0017FixValue.VALUE1.Items(0).ToString
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If

        Return retVal

    End Function
    ''' <summary>
    ''' 課税チェックボックスのデフォルト値取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>仮作成にて変動の可能性がある為、デフォルト値取得関数化</remarks>
    Private Function GetDefaultTaxation(countryCode As String) As String
        Return If(GBA00003UserSetting.IS_JPOPERATOR AndAlso countryCode = "JP", "1", "0")
    End Function

    ''' <summary>
    ''' ステータス値取得
    ''' </summary>
    Private Function GetStatus(ByVal brId As String) As DataTable

        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT BI.TYPE AS TYPE")
        sqlStat.AppendLine("     , BI.APPLYID AS APPLYID")
        sqlStat.AppendLine("     , ISNULL(TRIM(AH.STATUS),'') AS STATUS")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH")
        sqlStat.AppendLine("  ON AH.APPLYID = BI.APPLYID")
        sqlStat.AppendLine(" AND AH.STEP    = BI.LASTSTEP")
        sqlStat.AppendLine(" AND AH.DELFLG <> @DELFLG ")
        sqlStat.AppendLine(" WHERE BI.BRID      = @BRID")
        sqlStat.AppendLine("   AND BI.DELFLG   <> @DELFLG")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@BRID", SqlDbType.NVarChar).Value = brId
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        'コピー新規の場合はステータスを取らない
        If ViewState("COPYORGANIZERINFO") IsNot Nothing AndAlso retDt IsNot Nothing Then
            For Each dr As DataRow In retDt.Rows
                dr("APPLYID") = ""
                dr("STATUS") = ""
            Next

        End If
        Return retDt
    End Function

    ''' <summary>
    ''' Organizer-Info初期値取得
    ''' </summary>
    Public Sub GetOrgInfo()

        Dim COA0016VARIget As New BASEDLL.COA0016VARIget        '変数情報取

        Dim dicDefaultValueSettings As New Dictionary(Of String, TextBox) _
                        From {{"DEMURTO", Me.txtDemurdayT1},
                              {"DEMURUSRATE1", Me.txtDemurUSRate1},
                              {"DEMURUSRATE2", Me.txtDemurUSRate2},
                              {"TIP", Me.txtTip}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = "Default"
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                item.Value.Text = COA0016VARIget.VALUE
            Else
                CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0016VARIget.ERR)})
                Return
            End If
        Next

    End Sub

    ''' <summary>
    ''' ボタン制御用ステータス設定
    ''' </summary>
    ''' <param name="selectedTab">切替後のタブ</param>
    Private Sub SetStatus(ByVal selectedTab As String)

        Dim type As String = ""

        If selectedTab <> Me.tabOrganizer.ClientID Then

            If Me.lblBrNo.Text <> "" Then

                Select Case selectedTab
                    Case Me.tabExport1.ClientID
                        type = "POL1"
                    Case Me.tabInport1.ClientID
                        type = "POD1"
                    Case Me.tabExport2.ClientID
                        type = "POL2"
                    Case Me.tabInport2.ClientID
                        type = "POD2"
                End Select

                Dim stDt As DataTable = GetStatus(Me.lblBrNo.Text)
                If stDt Is Nothing OrElse stDt.Rows.Count = 0 Then
                    Return
                End If

                For Each stDr As DataRow In stDt.Rows
                    If Convert.ToString(stDr.Item("TYPE")) = type Then

                        If Convert.ToString(stDr.Item("STATUS")) <> C_APP_STATUS.EDITING Then
                            hdnEntryCost.Value = "1"
                        Else
                            hdnEntryCost.Value = ""
                        End If
                    End If
                Next
            Else
                hdnEntryCost.Value = "1"
            End If
        Else
            hdnEntryCost.Value = ""
        End If
    End Sub

    ''' <summary>
    ''' 国制御設定
    ''' </summary>
    Private Sub SetCountryControl(ByVal selectedTab As String)

        Dim countryCode As String = ""

        Select Case selectedTab
            Case Me.tabExport1.ClientID
                countryCode = Me.txtLoadCountry1.Text
            Case Me.tabInport1.ClientID
                countryCode = Me.txtDeliveryCountry1.Text
            Case Me.tabExport2.ClientID
                countryCode = Me.txtLoadCountry2.Text
            Case Me.tabInport2.ClientID
                countryCode = Me.txtDeliveryCountry2.Text
            Case Me.tabOrganizer.ClientID
                countryCode = Me.hdnCountryOrganizer.Value
        End Select

        If GBA00003UserSetting.IS_JOTUSER Then
            Me.hdnCountryControl.Value = "1"
        Else
            If countryCode = GBA00003UserSetting.COUNTRYCODE Then
                Me.hdnCountryControl.Value = "1"
            Else
                Me.hdnCountryControl.Value = "0"
            End If
        End If

    End Sub

    ''' <summary>
    ''' 承認ステータス名取得
    ''' </summary>
    ''' <param name="keycode">[IN]申請ステータスコード</param>
    ''' <param name="retStat">[OUT]引数で指定されたステータス名称</param>
    ''' <return>True:正常取得,False:異常</return>
    Public Function GetApprovalStat(ByVal keycode As String, ByRef retStat As String) As Boolean

        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "APPROVAL"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            For i As Integer = 0 To COA0017FixValue.VALUE1.Items.Count - 1
                If COA0017FixValue.VALUE1.Items(i).Value.ToString = keycode Then
                    If COA0019Session.LANGDISP = C_LANG.JA Then
                        retStat = COA0017FixValue.VALUE1.Items(i).ToString
                    Else
                        retStat = COA0017FixValue.VALUE2.Items(i).ToString
                    End If
                End If
            Next
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
            Return False
        End If

        Return True

    End Function

    ''' <summary>
    ''' チェックボックス初期設定
    ''' </summary>
    ''' <remarks>InputRequest時にポップアップする各発着の
    ''' InputRequestチェック及びメール送信有無のチェックボックス制御</remarks>
    Private Sub setChkInit()

        Dim edit As String = ""
        If Not GetApprovalStat(C_APP_STATUS.EDITING, edit) Then
            Return
        End If
        '各発着のステータス（編集中等),InputRequestチェックボックス,MailCheckBoxの配列
        Dim controlLists = {New With {.statusName = Me.hdnPol1Status.Value, .requestChk = Me.chkInputRequestExport1, .mailChk = Me.chkMailExport1},
                            New With {.statusName = Me.hdnPod1Status.Value, .requestChk = Me.chkInputRequestImport1, .mailChk = Me.chkMailInport1},
                            New With {.statusName = Me.hdnPol2Status.Value, .requestChk = Me.chkInputRequestExport2, .mailChk = Me.chkMailExport2},
                            New With {.statusName = Me.hdnPod2Status.Value, .requestChk = Me.chkInputRequestImport2, .mailChk = Me.chkMailInport2}}
        '各発着情報を元にチェックボックスを制御
        For Each controlItem In controlLists
            If controlItem.statusName = "" Then
                controlItem.requestChk.Checked = True
                controlItem.mailChk.Checked = True
            ElseIf controlItem.statusName = edit Then
                controlItem.requestChk.Enabled = False
                controlItem.mailChk.Enabled = True
            End If
            Dim onChangeScript As String = String.Format("inputRequestChk('{0}','{1}');", controlItem.requestChk.ClientID, controlItem.mailChk.ClientID)
            controlItem.requestChk.Attributes.Add("onchange", onChangeScript)
        Next controlItem

    End Sub

    Public Sub btnEntryCost_Click()

        TextChangeCheck()
        hdnMsgboxChangeFlg.Value = "1"

        If hdnMsgboxShowFlg.Value = "0" Then
            Me.hdnEntryCostFieldName.Value = "Select whether to send mail"
        Else
            Me.hdnEntryCostFieldName.Value = BEFORE_SAVE_MSG
        End If

    End Sub


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

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing
        brInfo = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))

        Select Case Me.hdnSelectedTabId.Value

            Case Me.tabInport1.ID
                If initBrInfo.ContainsKey("POD1") Then
                    Dim initBrInfoItem = initBrInfo("POD1")
                    Dim brInfoItem = brInfo("POD1")

                    If brInfoItem.Remark <> initBrInfoItem.Remark Then
                        hdnMsgboxShowFlg.Value = "1"
                        Return
                    End If
                End If
            Case Me.tabInport2.ID
                If initBrInfo.ContainsKey("POD2") Then
                    Dim initBrInfoItem = initBrInfo("POD2")
                    Dim brInfoItem = brInfo("POD2")

                    If brInfoItem.Remark <> initBrInfoItem.Remark Then
                        hdnMsgboxShowFlg.Value = "1"
                        Return
                    End If
                End If
            Case Me.tabExport1.ID
                If initBrInfo.ContainsKey("POL1") Then
                    Dim initBrInfoItem = initBrInfo("POL1")
                    Dim brInfoItem = brInfo("POL1")

                    If brInfoItem.Remark <> initBrInfoItem.Remark Then
                        hdnMsgboxShowFlg.Value = "1"
                        Return
                    End If
                End If
            Case Me.tabExport2.ID
                If initBrInfo.ContainsKey("POL2") Then
                    Dim initBrInfoItem = initBrInfo("POL2")
                    Dim brInfoItem = brInfo("POL2")

                    If brInfoItem.Remark <> initBrInfoItem.Remark Then
                        hdnMsgboxShowFlg.Value = "1"
                        Return
                    End If
                End If
            Case Me.tabOrganizer.ID
                If initBrInfo.ContainsKey("INFO") Then
                    Dim initBrInfoItem = initBrInfo("INFO")
                    Dim brInfoItem = brInfo("INFO")

                    If brInfoItem.Remark <> initBrInfoItem.Remark Then
                        hdnMsgboxShowFlg.Value = "1"
                        Return
                    End If
                End If
        End Select

        'Organizer
        '日付項目
        Dim dateDicChk As New Dictionary(Of String, String) From {{"VALIDITYFROM", Me.txtBrStYmd.Text}, {"VALIDITYTO", Me.txtBrEndYmd.Text},
            {"ETD1", Me.txtEtd1.Text}, {"ETA1", Me.txtEta1.Text},
            {"ETD2", Me.txtEtd2.Text}, {"ETA2", Me.txtEta2.Text}}
        For Each item As KeyValuePair(Of String, String) In dateDicChk

            Dim chgItem As String = ""
            Dim itmDate As Date
            If item.Value <> "" Then
                If Date.TryParseExact(item.Value, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, itmDate) Then
                    chgItem = itmDate.ToString("yyyy/MM/dd")
                Else
                    chgItem = item.Value
                End If
            End If

            If chgItem <> Convert.ToString(orgDt.Rows(0).Item(item.Key)) Then
                hdnMsgboxShowFlg.Value = "1"
                Return
            End If
        Next

        Dim dicChk As New Dictionary(Of String, String) From {
            {"REMARK", HttpUtility.HtmlDecode(Me.lblBrRemarkText.Text)}, {"TERMTYPE", Me.txtBrTerm.Text},
            {"NOOFTANKS", Me.txtNoOfTanks.Text}, {"INVOICEDBY", Me.txtInvoiced.Text},
            {"APPLYTEXT", HttpUtility.HtmlDecode(Me.lblApplyRemarks.Text)}, {"APPROVEDTEXT", HttpUtility.HtmlDecode(Me.lblAppJotRemarks.Text)},
            {"CONSIGNEE", Me.txtConsignee.Text}, {"CARRIER1", Me.txtCarrier1.Text},
            {"CARRIER2", Me.txtCarrier2.Text}, {"PRODUCTCODE", Me.txtProduct.Text},
            {"RECIEPTPORT1", Me.txtRecieptPort1.Text}, {"DISCHARGEPORT1", Me.txtDischargePort1.Text},
            {"RECIEPTPORT2", Me.txtRecieptPort2.Text}, {"DISCHARGEPORT2", Me.txtDischargePort2.Text},
            {"AGENTPOL1", Me.txtAgentPol1.Text}, {"AGENTPOD1", Me.txtAgentPod1.Text},
            {"AGENTPOL2", Me.txtAgentPol2.Text}, {"AGENTPOD2", Me.txtAgentPod2.Text},
            {"VSL1", Me.txtVsl1.Text}, {"VOY1", Me.txtVoy1.Text},
            {"VSL2", Me.txtVsl2.Text}, {"VOY2", Me.txtVoy2.Text},
            {"PRODUCTWEIGHT", cnvInt(Me.txtWeight.Text)}, {"CAPACITY", cnvInt(Me.txtTankCapacity.Text)},
            {"LOADING", cnvInt(Me.txtLoading.Text)}, {"STEAMING", cnvInt(Me.txtSteaming.Text)},
            {"TIP", cnvInt(Me.txtTip.Text)}, {"EXTRA", cnvInt(Me.txtExtra.Text)},
            {"JOTHIREAGE", cnvInt(Me.txtJOTHireage.Text)}, {"COMMERCIALFACTOR", cnvInt(Me.txtCommercialFactor.Text)},
            {"AMTREQUEST", cnvInt(Me.txtAmtRequest.Text)},
            {"DEMURTO", Me.txtDemurdayT1.Text}, {"DEMURUSRATE1", Me.txtDemurUSRate1.Text},
            {"DEMURUSRATE2", Me.txtDemurUSRate2.Text}, {"FEE", cnvInt(Me.txtFee.Text)}, {"BILLINGCATEGORY", Me.txtBillingCategory.Text},
            {"DISABLED", If(Me.chkDisabled.Checked, CONST_FLAG_YES, CONST_FLAG_NO)}}
        For Each item As KeyValuePair(Of String, String) In dicChk
            If item.Value <> Convert.ToString(orgDt.Rows(0).Item(item.Key)) Then
                hdnMsgboxShowFlg.Value = "1"
                Return
            End If
        Next

        'Cost
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)
        tabObjects.Add(COSTITEM.CostItemGroup.Export1, Me.tabExport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport1, Me.tabInport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Export2, Me.tabExport2)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport2, Me.tabInport2)

        'Dim currentCostItemGroup As COSTITEM.CostItemGroup
        For Each tabObject In tabObjects

            Dim allCostList As List(Of COSTITEM)
            allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))

            Dim showInitCostList As List(Of COSTITEM)
            showInitCostList = (From initAllCostItem In initAllCostList
                                Where initAllCostItem.ItemGroup = tabObject.Key
                                Order By initAllCostItem.IsAddedCost, Convert.ToInt32(If(initAllCostItem.Class2 = "", "0", initAllCostItem.Class2))).ToList

            Dim showCostList As List(Of COSTITEM)
            showCostList = (From allCostItem In allCostList
                            Where allCostItem.ItemGroup = tabObject.Key
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
                Next sRow

                Dim matchRec = From item In showCostList Where item.AllPropertyEquals(row)
                If matchRec.Any = False Then
                    hdnMsgboxShowFlg.Value = "1"
                    Return
                End If

            Next row
        Next tabObject

        Return

    End Sub
    ''' <summary>
    ''' 港が変更されたかチェック
    ''' </summary>
    ''' <returns>変更された発着情報("POL1","POD1","POL2","POD2"等)、変更が無い場合はCount 0 </returns>
    Private Function GetModifiedPort() As List(Of String)
        Dim retVal As New List(Of String)
        ''新規作成時は無意味なのでそのまま返却
        'If Me.hdnNewBreaker.Value = "1" Then
        '    Return retVal
        'End If
        '起動時データテーブル
        Dim orgDt As DataTable = DirectCast(ViewState("INITORGANIZERINFO"), DataTable)
        Dim orgDr As DataRow = orgDt.Rows(0)
        If Not orgDr("RECIEPTPORT1").Equals(Me.txtRecieptPort1.Text) Then
            retVal.Add("POL1")
        End If
        If Not orgDr("DISCHARGEPORT1").Equals(Me.txtDischargePort1.Text) Then
            retVal.Add("POD1")
        End If
        If Not orgDr("RECIEPTPORT2").Equals(Me.txtRecieptPort2.Text) Then
            retVal.Add("POL2")
        End If
        If Not orgDr("DISCHARGEPORT2").Equals(Me.txtDischargePort2.Text) Then
            retVal.Add("POD2")
        End If
        Return retVal
    End Function
    ''' <summary>
    ''' 変更前の港コードを取得
    ''' </summary>
    ''' <param name="targetTextObj"></param>
    ''' <returns></returns>
    Private Function GetPrevPortCode(targetTextObj As TextBox) As String
        '変更前の基本情報を取得
        Dim orgDt As DataTable = DirectCast(ViewState("INITORGANIZERINFO"), DataTable)
        Dim orgDr As DataRow = orgDt.Rows(0)
        Dim portFieldName As String = ""
        Select Case targetTextObj.ID
            Case "txtRecieptPort1"
                portFieldName = "RECIEPTPORT1"
            Case "txtDischargePort1"
                portFieldName = "DISCHARGEPORT1"
            Case "txtRecieptPort2"
                portFieldName = "RECIEPTPORT2"
            Case "txtDischargePort2"
                portFieldName = "DISCHARGEPORT2"
        End Select

        Dim prevPortCode As String = Convert.ToString(orgDr(portFieldName))
        Return prevPortCode
    End Function

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
    ''' 表示制御取得
    ''' </summary>
    Public Function GetEnableControl() As String

        Dim COA0017FixValue As New COA0017FixValue
        Dim retVal As String = ""

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CONTROL"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            retVal = COA0017FixValue.VALUE1.Items(0).ToString
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If

        Return retVal

    End Function

    ''' <summary>
    ''' 請求先判定取得
    ''' </summary>
    Public Function GetInvoicedBy(ByVal invoicedBy As String) As String

        Dim retVal As String = ""
        Dim dataT As New DataTable

        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT COUNTRYCODE")
        sqlStat.AppendLine("  FROM GBM0005_TRADER ")
        sqlStat.AppendLine(" WHERE CARRIERCODE  = @CARRIERCODE")
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCarrierCode As SqlParameter = sqlCmd.Parameters.Add("@CARRIERCODE", SqlDbType.NVarChar)
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCarrierCode.Value = invoicedBy
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dataT)
                If dataT.Rows.Count > 0 Then
                    retVal = dataT.Rows(0).Item(0).ToString()
                End If
            End Using
        End Using

        Return retVal

    End Function
    ''' <summary>
    ''' 承認ボタン押下時
    ''' </summary>
    Public Sub btnApproval_Click()

        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        brInfo = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))

        Dim COA0032Apploval As New BASEDLL.COA0032Apploval

        '承認登録
        COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
        COA0032Apploval.I_APPLYID = brInfo("INFO").ApplyId
        COA0032Apploval.I_STEP = Me.hdnStep.Value
        COA0032Apploval.COA0032setApproval()
        If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        Me.hdnStatus.Value = C_APP_STATUS.APPROVED
        If brInfo("INFO").LastStep = Me.hdnStep.Value Then
            'メール
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = "BRS_Approved"
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = Me.lblBrNo.Text
            GBA00009MailSendSet.BRSUBID = brInfo("INFO").SubId
            GBA00009MailSendSet.BRBASEID = brInfo("INFO").LinkId
            GBA00009MailSendSet.APPLYID = brInfo("INFO").ApplyId
            GBA00009MailSendSet.LASTSTEP = brInfo("INFO").LastStep
            GBA00009MailSendSet.GBA00009setMailToBR()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then

                CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                Me.hdnCostSelectedTabId.Value = Me.hdnSelectedTabId.Value
                Me.hdnMsgId.Value = GBA00009MailSendSet.ERR
                Server.Transfer(Request.Url.LocalPath)
                Return
            End If
        End If

        Me.hdnCostSelectedTabId.Value = Me.hdnSelectedTabId.Value
        Me.hdnMsgId.Value = C_MESSAGENO.APPROVALSUCCESS
        Server.Transfer(Request.Url.LocalPath)
        'メッセージ出力

    End Sub
    ''' <summary>
    ''' 否認ボタン押下時
    ''' </summary>
    Public Sub btnAppReject_Click()
        Dim currentCostItemGroup As COSTITEM.CostItemGroup = Nothing

        Select Case Me.hdnSelectedTabId.Value
            Case Me.tabInport1.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Inport1
            Case Me.tabInport2.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Inport2
            Case Me.tabExport1.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Export1
            Case Me.tabExport2.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Export2
            Case Me.tabOrganizer.ID
                currentCostItemGroup = COSTITEM.CostItemGroup.Organizer
        End Select

        If currentCostItemGroup <> COSTITEM.CostItemGroup.Organizer Then
            '入力内容保持
            SaveGridItem(currentCostItemGroup)
        End If

        TextChangeCheck()
        If Me.hdnMsgboxShowFlg.Value = "1" Then
            CommonFunctions.ShowConfirmMessage("00025", Me, submitButtonId:="btnAppRejectOk")
            Return
        End If

        btnAppRejectOk_Click()
    End Sub
    ''' <summary>
    ''' 否認確認メッセージ
    ''' </summary>
    Public Sub btnAppRejectOk_Click()
        Dim brInfo As Dictionary(Of String, BreakerInfo) = Nothing 'ブレーカー関連付け
        brInfo = DirectCast(ViewState("DICBRINFO"), Dictionary(Of String, BreakerInfo))

        Dim COA0032Apploval As New BASEDLL.COA0032Apploval

        '否認登録
        COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
        COA0032Apploval.I_APPLYID = brInfo("INFO").ApplyId
        COA0032Apploval.I_STEP = Me.hdnStep.Value
        COA0032Apploval.COA0032setDenial()
        If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'メール
        Dim GBA00009MailSendSet As New GBA00009MailSendSet
        GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
        GBA00009MailSendSet.EVENTCODE = "BRS_Rejected"
        GBA00009MailSendSet.MAILSUBCODE = ""
        GBA00009MailSendSet.BRID = Me.lblBrNo.Text
        GBA00009MailSendSet.BRSUBID = brInfo("INFO").SubId
        GBA00009MailSendSet.BRBASEID = brInfo("INFO").LinkId
        GBA00009MailSendSet.APPLYID = brInfo("INFO").ApplyId
        GBA00009MailSendSet.LASTSTEP = brInfo("INFO").LastStep
        GBA00009MailSendSet.GBA00009setMailToBR()
        If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        Me.hdnStatus.Value = C_APP_STATUS.REJECT
        Me.hdnCostSelectedTabId.Value = Me.hdnSelectedTabId.Value

        Me.hdnMsgId.Value = C_MESSAGENO.REJECTSUCCESS
        Server.Transfer(Request.Url.LocalPath)
        'メッセージ出力
        'CommonFunctions.ShowMessage(C_MESSAGENO.REJECTSUCCESS, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

    End Sub

    ''' <summary>
    ''' コピーブレーカーの費用データの為替レートを直近の為替レートに置換
    ''' </summary>
    ''' <param name="dtBrBase"></param>
    ''' <param name="dtBrValue"></param>
    Private Sub ModifyExrateCopyBr(ByRef dtBrBase As DataTable, ByRef dtBrValue As DataTable)
        Dim dr As DataRow = dtBrBase.Rows(0)
        Dim countryCodes As New Dictionary(Of String, String) From {{"POL1", Convert.ToString(dr.Item("RECIEPTCOUNTRY1"))},
                                                                    {"POL2", Convert.ToString(dr.Item("RECIEPTCOUNTRY2"))},
                                                                    {"POD1", Convert.ToString(dr.Item("DISCHARGECOUNTRY1"))},
                                                                    {"POD2", Convert.ToString(dr.Item("DISCHARGECOUNTRY2"))}}
        '発着国ごとのExRate
        Dim exRates As New Dictionary(Of String, String) 'Exレートを管理
        Dim GBA00010ExRate As New GBA00010ExRate
        '発着の国をループ
        For Each countryCode In countryCodes.Values
            '国コードが未指定、レート設定済みの場合はスキップ
            If countryCode = "" OrElse exRates.ContainsKey(countryCode) Then
                Continue For
            End If
            'ExRate取得
            GBA00010ExRate.COUNTRYCODE = countryCode
            GBA00010ExRate.TARGETYM = Date.Now.ToString("yyyy/MM")
            GBA00010ExRate.getExRateInfo()
            If GBA00010ExRate.ERR = C_MESSAGENO.NORMAL Then
                Dim exRtDt = GBA00010ExRate.EXRATE_TABLE
                Dim exRtDr As DataRow = exRtDt.Rows(0)
                exRates.Add(countryCode, Convert.ToString(exRtDr.Item("EXRATE")))
            Else
                exRates.Add(countryCode, "0")
            End If

        Next countryCode
        For Each drCost As DataRow In dtBrValue.Rows
            If exRates.ContainsKey(Convert.ToString(drCost("COUNTRYCODE"))) Then
                drCost.Item("LOCALRATE") = exRates(Convert.ToString(drCost("COUNTRYCODE")))
            End If
        Next
    End Sub
    ''' <summary>
    ''' 現在画面選択中のタブ取得
    ''' </summary>
    ''' <param name="defVal">取得できない場合のデフォルトタブ</param>
    ''' <returns>選択中のタブ</returns>
    Private Function GetCurrentTab(Optional defVal As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Organizer) As COSTITEM.CostItemGroup
        Dim currentTab As COSTITEM.CostItemGroup = COSTITEM.CostItemGroup.Organizer
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)

        tabObjects.Add(COSTITEM.CostItemGroup.Organizer, Me.tabOrganizer)
        tabObjects.Add(COSTITEM.CostItemGroup.Export1, Me.tabExport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport1, Me.tabInport1)
        tabObjects.Add(COSTITEM.CostItemGroup.Export2, Me.tabExport2)
        tabObjects.Add(COSTITEM.CostItemGroup.Inport2, Me.tabInport2)

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected") Then
                currentTab = tabObject.Key
                Exit For
            End If
        Next tabObject

        Return currentTab
    End Function
    ''' <summary>
    ''' JPYのレートを取得する
    ''' </summary>
    ''' <param name="brId"></param>
    ''' <returns></returns>
    Private Function GetJpyExrate(brId As String) As String
        Dim targetYm As String = Now.ToString("yyyy/MM")
        If brId <> "" Then
            '削除を含むブレーカーINITDATAの最も古い日付をRateを取得するレート年月とする
            '※(更新者がTACOS)の場合はシステム日付
            Dim sqlStat As New Text.StringBuilder
            sqlStat.AppendLine("SELECT MIN(BI.INITYMD) AS INITYMD ")
            sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
            sqlStat.AppendLine(" WHERE BI.BRID     = @BRID")
            sqlStat.AppendLine("   AND BI.TYPE     = @TYPE")
            sqlStat.AppendLine("   AND BI.UPDUSER <> @UPDUSER")
            Using sqlCon = New SqlConnection(COA0019Session.DBcon),
                  sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar).Value = brId
                    .Add("@TYPE", SqlDbType.NVarChar).Value = "INFO"
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = "TACOS"
                End With
                Dim obj = sqlCmd.ExecuteScalar
                If obj IsNot Nothing AndAlso IsDate(obj) AndAlso Convert.ToString(obj) <> "1900/01/01" Then
                    targetYm = CDate(obj).ToString("yyyy/MM")
                End If
            End Using
        End If
        Dim GBA00010ExRate As New GBA00010ExRate
        GBA00010ExRate.COUNTRYCODE = "JP"
        GBA00010ExRate.TARGETYM = targetYm
        GBA00010ExRate.getExRateInfo()
        If GBA00010ExRate.ERR = C_MESSAGENO.NORMAL Then
            Dim exRtDt = GBA00010ExRate.EXRATE_TABLE
            Dim exRtDr As DataRow = exRtDt.Rows(0)

            Return Convert.ToString(exRtDr.Item("EXRATE"))
        Else
            Return "0"
        End If

    End Function
    ''' <summary>
    ''' USDベースのHireageを元にJPYの参考値を画面に設定する
    ''' </summary>
    Private Sub SetHireageJpy()
        Dim jpyRate As Decimal = DecimalStringToDecimal(Convert.ToString(ViewState("JPYEXR")))
        '円フィールドと対になるドルのフィールドをリスト定義
        Dim jpyUsdObjects = {New With {.jpyObj = Me.txtTotalCostJPY, .usdObj = Me.txtTotalCost},
                             New With {.jpyObj = Me.txtJOTHireageJPY, .usdObj = Me.txtJOTHireage},
                             New With {.jpyObj = Me.txtCommercialFactorJPY, .usdObj = Me.txtCommercialFactor},
                             New With {.jpyObj = Me.txtInvoicedTotalJPY, .usdObj = Me.txtInvoicedTotal},
                             New With {.jpyObj = Me.txtPerDayJPY, .usdObj = Me.txtPerDay},
                             New With {.jpyObj = Me.txtAmtRequestJPY, .usdObj = Me.txtAmtRequest},
                             New With {.jpyObj = Me.txtAmtPrincipalJPY, .usdObj = Me.txtAmtPrincipal},
                             New With {.jpyObj = Me.txtAmtDiscountJPY, .usdObj = Me.txtAmtDiscount}}
        '対象のドルフィールドを円換算し円フィールドに格納
        For Each jpyUsdObj In jpyUsdObjects
            jpyUsdObj.jpyObj.Text = ""
            If jpyRate = 0 OrElse jpyUsdObj.usdObj.Text.Trim = "" Then
                Continue For
            End If
            Dim usd As Decimal = DecimalStringToDecimal(jpyUsdObj.usdObj.Text)
            Dim jpy As Decimal = Math.Round(usd * jpyRate, 0)
            jpyUsdObj.jpyObj.Text = NumberFormat(jpy, "JP")
        Next
    End Sub
    ''' <summary>
    ''' 手数料変更時
    ''' </summary>
    Public Sub txtFee_Change()
        SetDisplayTotalCost()
    End Sub
End Class