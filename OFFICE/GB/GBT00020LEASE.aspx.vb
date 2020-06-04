Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' リースブレーカー契約書入力
''' </summary>
Public Class GBT00020LEASE
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00020L" '自身のMAPID

    Private Const CONST_TBL_CONTRACT As String = "GBT0010_LBR_CONTRACT"
    Private Const CONST_TBL_AGREEMENT As String = "GBT0011_LBR_AGREEMENT"
    Private Const CONST_TBL_TANK As String = "GBT0012_RESRVLEASETANK"
    Private Const CONST_DT_NAME_CONTRACT As String = "CONTRACT"
    Private Const CONST_VS_NAME_PREV_VAL As String = "PREV_VAL"
    Private Const CONST_VS_NAME_CUR_VAL As String = "CUR_VAL"
    ''' <summary>
    ''' アップロードファイルのリース契約書ルート
    ''' </summary>
    Private Const CONST_DIRNAME_LEASE_CONTRACT As String = "LEASE\CONTRACT"
    ''' <summary>
    ''' リースブレーカー検索結果画面情報
    ''' </summary>
    ''' <returns></returns>
    Public Property GBT00020RValues As GBT00020RESULT.GBT00020RValues
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' ポストバック時画面上の情報を保持
    ''' </summary>
    Private DsDisDisplayValues As DataSet
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
                Me.Form.Attributes.Add("data-profid", COA0019Session.PROFID)
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '固定右ボックス選択肢
                '****************************************
                SetFixvalueListItem("LEASEPAYMENTMONTH", Me.lbLeasePaymentType)
                SetFixvalueListItem("LEASEPAYMENTKIND", Me.lbLeasePaymentKind)
                SetFixvalueListItem("LEASEACCOUNT", Me.lbLeaseAccount)
                SetFixvalueListItem("LEASETAX", Me.lbTax)
                SetFixvalueListItem("GENERALFLG", Me.lbYesNo)
                '****************************************
                '遷移元よりデータ取得
                '****************************************
                Dim ds As DataSet = New DataSet
                If Me.GetPrevDisplayInfo(ds) <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                ViewState(CONST_VS_NAME_PREV_VAL) = ds
                Me.DsDisDisplayValues = CommonFunctions.DeepCopy(ds)
                ViewState(CONST_VS_NAME_CUR_VAL) = Me.DsDisDisplayValues

                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnThisMapVariant.Value
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
                '取得したデータテーブルを画面に展開
                SetDisplayFromDt(Me.DsDisDisplayValues.Tables(CONST_DT_NAME_CONTRACT))
                '使用可否制御
                enabledControls()
                '添付ファイル展開
                Me.repAttachment.DataSource = Me.DsDisDisplayValues.Tables(C_DTNAME_ATTACHMENT)
                Me.repAttachment.DataBind()
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                Me.GBT00020RValues = DirectCast(ViewState("GBT00020RValues"), GBT00020RESULT.GBT00020RValues)
                Me.DsDisDisplayValues = CollectDisplay()
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
                ' ファイルアップロード処理
                '**********************
                If Me.hdnListUpload.Value IsNot Nothing AndAlso Me.hdnListUpload.Value <> "" Then
                    If Me.hdnListUpload.Value = "XLS_LOADED" Then
                        UploadExcel()
                    ElseIf Me.hdnListUpload.Value = "PDF_LOADED" Then
                        UploadAttachment()
                    End If

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
            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            ViewState(CONST_VS_NAME_CUR_VAL) = Me.DsDisDisplayValues
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
                        Me.hdnCalendarValue.Value = FormatDateYMD(txtobj.Text, GBA00003UserSetting.DATEFORMAT)

                        Me.mvLeft.Focus()
                    End If
                'SHIPPERビュー表示
                Case Me.vLeftShipper.ID
                    Dim dt As DataTable = GetShipper()
                    With Me.lbShipper
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CUSTOMERCODE"
                        .DataBind()
                        .Focus()
                    End With
                    '入力済のデータを選択状態にする
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    If dblClickField IsNot Nothing AndAlso lbShipper.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbShipper.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
                Case Else
                    Dim dicListId As New Dictionary(Of String, ListBox) _
                        From {{Me.vLeftLeasePaymentType.ID, Me.lbLeasePaymentType}, {Me.vLeftLeasePaymentKind.ID, Me.lbLeasePaymentKind},
                              {Me.vLeftLeaseAccount.ID, Me.lbLeaseAccount}, {Me.vLeftTax.ID, Me.lbTax},
                              {Me.vLeftYesNo.ID, Me.lbYesNo}}

                    If dicListId.ContainsKey(changeViewObj.ID) = False Then
                        Return
                    End If
                    Dim targetListObj = dicListId(changeViewObj.ID)
                    targetListObj.SelectedIndex = -1
                    targetListObj.Focus()
                    '入力済のデータを選択状態にする
                    If Me.hdnTextDbClickField.Value.StartsWith("repAttachment_txtDeleteFlg_") Then
                        Dim drIndex As Integer = CInt(Me.hdnTextDbClickField.Value.Replace("repAttachment_txtDeleteFlg_", ""))
                        Dim dtAttachment As DataTable = Me.DsDisDisplayValues.Tables(C_DTNAME_ATTACHMENT)
                        Dim drTargetAttachmentRow = dtAttachment.Rows(drIndex)
                        Dim findLbValue As ListItem = targetListObj.Items.FindByValue(Convert.ToString(drTargetAttachmentRow("DELFLG")))
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    Else
                        Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)

                        If dblClickField IsNot Nothing AndAlso targetListObj.Items IsNot Nothing Then
                            Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                            Dim findLbValue As ListItem = targetListObj.Items.FindByValue(dblClickFieldText.Text)
                            If findLbValue IsNot Nothing Then
                                findLbValue.Selected = True
                            End If
                        End If
                    End If

            End Select
        End If

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()

        If IsModifiedData() Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, pageObject:=Me, submitButtonId:="btnBackOk")
            Return
        End If

        btnBackOk_Click()
    End Sub
    ''' <summary>
    ''' 戻る確定時処理(btnBack_Click時に更新データが無い場合も通る)
    ''' </summary>
    Public Sub btnBackOk_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
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
        '画面遷移実行()
        Server.Transfer(COA0011ReturnUrl.URL)

    End Sub
    ''' <summary>
    ''' Excel出力ボタン押下時
    ''' </summary>
    Public Sub btnOutputExcel_Click()
        ''別画面でExcelを表示
        'hdnPrintURL.Value = outUrl
        'ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

    End Sub
    ''' <summary>
    ''' 保存ボタン押下時イベント
    ''' </summary>
    Public Sub btnSave_Click()
        '******************************
        '変更値チェック
        '******************************
        If IsModifiedData() = False Then
            '変更が全くない場合はメッセージを表示
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '画面入力情報の収集
        Dim ds As DataSet = Me.DsDisDisplayValues
        '******************************
        '禁則文字の置換
        '******************************
        Dim invChangeField As New List(Of String) From {"CONTRACTFROM", "CONTRACTTO", "SHIPPER", "LEASETERM",
                                                        "LEASEPAYMENT", "LEASEPAYMENTTYPE", "LEASEPAYMENTKIND",
                                                        "AUTOEXTEND", "ACCOUNT", "TAXKIND", "NOOFAGREEMENT", "ACCSEGMENT"}
        ChangeInvalidChar(ds.Tables(CONST_DT_NAME_CONTRACT), invChangeField)
        '******************************
        '単項目チェック
        '******************************
        Dim dicCheckField As New Dictionary(Of String, TextBox) From
        {{"CONTRACTFROM", Me.txtLeaseFrom}, {"SHIPPER", Me.txtShipper},
         {"LEASEPAYMENTTYPE", Me.txtLeasePaymentType}, {"AUTOEXTEND", Me.txtAutoExtend},
        {"ACCOUNT", Me.txtLeaseAccount}, {"TAXKIND", Me.txtTax}, {"NOOFAGREEMENT", Me.txtNoOfAgreement},
        {"REMARK", Me.txtRemarks}, {"LEASEPAYMENTKIND", Me.txtLeasePaymentKind}, {"ACCSEGMENT", Me.txtAccSegment}}
        Dim dr As DataRow = ds.Tables(CONST_DT_NAME_CONTRACT).Rows(0)
        For Each singleChkItem As KeyValuePair(Of String, TextBox) In dicCheckField
            Dim fieldName As String = singleChkItem.Key
            Dim chkVal As String = Convert.ToString(dr.Item(fieldName))
            If CheckSingle(fieldName, chkVal) <> C_MESSAGENO.NORMAL Then
                singleChkItem.Value.Focus()
                Return
            End If
        Next
        '******************************
        'リスト存在チェック
        '******************************
        Dim listCheck As New List(Of TextBox) From {Me.txtShipper, Me.txtAutoExtend,
                                                    Me.txtLeasePaymentType, Me.txtLeaseAccount, Me.txtTax, Me.txtLeasePaymentKind}
        For Each chkObj In listCheck
            '空白ならスキップ
            If chkObj.Text = "" Then
                Continue For
            End If
            Dim dicListItem As Dictionary(Of String, String) = New Dictionary(Of String, String)
            Select Case chkObj.ID
                Case "txtShipper"
                    Dim dtDdlValue As DataTable = Nothing
                    dtDdlValue = GetShipper()
                    dicListItem = (From item In dtDdlValue Select key = Convert.ToString(item("CUSTOMERCODE")), val = Convert.ToString(item("NAME"))).ToDictionary(Function(dv) dv.key, Function(dv) dv.val)

                Case "txtLeasePaymentType"
                    dicListItem = (From listItem In lbLeasePaymentType.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtLeaseAccount"
                    dicListItem = (From listItem In lbLeaseAccount.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtAutoExtend"
                    dicListItem = (From listItem In lbYesNo.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtTax"
                    dicListItem = (From listItem In lbTax.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtLeasePaymentKind"
                    dicListItem = (From listItem In lbLeasePaymentKind.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
            End Select
            If Not dicListItem.ContainsKey(chkObj.Text) Then
                CommonFunctions.ShowMessage(C_MESSAGENO.UNSELECTABLEERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                        messageParams:=New List(Of String) From {String.Format("VALUE:{0}", chkObj.Text)})
                chkObj.Focus()
                Return
            End If
        Next chkObj
        '****************************************
        '他ユーザー更新チェック
        '当画面遷移直後の情報と現在のDBの状態比較
        '****************************************
        If Me.GBT00020RValues.NewBrCreate = False Then
            '修正時のみチェック
            Dim dtLatestContract As DataTable = CreateDispDt()
            dtLatestContract = GetContractItem(dtLatestContract, Me.GBT00020RValues.ContractNo)
            '既にレコードが無い場合、他社による削除
            If dtLatestContract IsNot Nothing AndAlso dtLatestContract.Rows.Count = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
            Dim drDispContract = ds.Tables(CONST_DT_NAME_CONTRACT).Rows(0)
            Dim drLatestContract = dtLatestContract.Rows(0)
            If Not (Convert.ToString(drDispContract.Item("UPDYMD")).TrimEnd = Convert.ToString(drLatestContract("UPDYMD")).TrimEnd _
               AndAlso Convert.ToString(drDispContract.Item("UPDUSER")).TrimEnd = Convert.ToString(drLatestContract("UPDUSER")).TrimEnd _
               AndAlso Convert.ToString(drDispContract.Item("UPDTERMID")).TrimEnd = Convert.ToString(drLatestContract("UPDTERMID")).TrimEnd) Then
                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        End If

        EntryDispValues(ds)
        '戻るアクション
        btnBackOk_Click()

    End Sub
    ''' <summary>
    ''' ダウンロードボタン押下時
    ''' </summary>
    Public Sub btnDownloadFiles_Click()
        Dim dtAttachment As DataTable = Me.DsDisDisplayValues.Tables(C_DTNAME_ATTACHMENT)
        'ダウンロード対象有無
        If dtAttachment Is Nothing OrElse dtAttachment.Rows.Count = 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.FILENOTEXISTS, Me.lblFooterMessage, pageObject:=Me)
        End If
        Dim dlUrl As String = CommonFunctions.GetAttachmentCompressedFileUrl(dtAttachment, Me.GBT00020RValues.ContractNo)
        If dlUrl <> "" Then
            Me.hdnPrintURL.Value = dlUrl
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
        End If
        '終了メッセージ
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDOWNLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
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
                Case Me.vLeftShipper.ID
                    '荷主選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim customerCode As String = ""
                        If Me.lbShipper.SelectedItem IsNot Nothing Then
                            customerCode = Me.lbShipper.SelectedItem.Value
                        End If
                        SetDisplayShipper(targetTextBox, customerCode)
                    End If
                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If

                Case Else
                    If Me.hdnTextDbClickField.Value.StartsWith("repAttachment_txtDeleteFlg_") Then
                        Dim drIndex As Integer = CInt(Me.hdnTextDbClickField.Value.Replace("repAttachment_txtDeleteFlg_", ""))
                        Dim dtAttachment As DataTable = Me.DsDisDisplayValues.Tables(C_DTNAME_ATTACHMENT)
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
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject Is Nothing Then
                        Return
                    End If
                    Dim targetTextObject As TextBox = DirectCast(targetObject, TextBox)
                    Dim taxtLabelObjects As New Dictionary(Of String, Object) _
                        From {{Me.txtLeasePaymentType.ID, New With {.lbl = Me.lblLeasePaymentTypeText, .list = Me.lbLeasePaymentType}},
                              {Me.txtLeasePaymentKind.ID, New With {.lbl = Me.lblLeasePaymentKindText, .list = Me.lbLeasePaymentKind}},
                              {Me.txtAutoExtend.ID, New With {.lbl = Nothing, .list = Me.lbYesNo}},
                              {Me.txtLeaseAccount.ID, New With {.lbl = Me.lblLeaseAccountText, .list = Me.lbLeaseAccount}},
                              {Me.txtTax.ID, New With {.lbl = Me.lblTaxText, .list = Me.lbTax}}}
                    If taxtLabelObjects.ContainsKey(targetObject.ID) = False Then
                        Return
                    End If
                    Dim targetTextLabelObj As New With {.lbl = Nothing, .list = Nothing}
                    Dim typ = taxtLabelObjects(targetObject.ID).GetType
                    Dim targetLabelObj As Label = DirectCast(typ.GetProperty("lbl").GetValue(taxtLabelObjects(targetObject.ID), Nothing), Label)
                    Dim targetListboxObj As ListBox = DirectCast(typ.GetProperty("list").GetValue(taxtLabelObjects(targetObject.ID), Nothing), ListBox)
                    If targetListboxObj.SelectedItem IsNot Nothing Then
                        targetTextObject.Text = targetListboxObj.SelectedItem.Value
                        If targetLabelObj IsNot Nothing Then
                            targetLabelObj.Text = targetListboxObj.SelectedItem.Text
                        End If
                    Else
                        If targetLabelObj IsNot Nothing Then
                            targetLabelObj.Text = ""
                        End If
                    End If
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
    ''' 支払月変更時
    ''' </summary>
    Public Sub txtLeasePaymentType_Change()
        Dim leasePaymentType As String = Me.txtLeasePaymentType.Text.Trim
        Me.txtLeasePaymentType.Text = leasePaymentType
        Dim findItem As ListItem = Me.lbLeasePaymentType.Items.FindByValue(leasePaymentType)
        Me.lblLeasePaymentTypeText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblLeasePaymentTypeText.Text = findItem.Text
        End If
    End Sub
    ''' <summary>
    ''' 種別変更時
    ''' </summary>
    Public Sub txtLeasePaymentKind_Change()
        Dim leasePaymentKind As String = Me.txtLeasePaymentKind.Text.Trim
        Me.txtLeasePaymentKind.Text = leasePaymentKind
        Dim findItem As ListItem = Me.lbLeasePaymentKind.Items.FindByValue(leasePaymentKind)
        Me.lblLeasePaymentKindText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblLeasePaymentKindText.Text = findItem.Text
        End If
    End Sub
    ''' <summary>
    ''' 自動延長変更時
    ''' </summary>
    Public Sub txtAutoExtend_Change()
        Dim autoExtend As String = Me.txtAutoExtend.Text.Trim
        Me.txtAutoExtend.Text = autoExtend
        Dim findItem As ListItem = Me.lbYesNo.Items.FindByValue(autoExtend)
        Me.lblAutoExtendText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblAutoExtendText.Text = findItem.Text
        End If
    End Sub
    ''' <summary>
    ''' 自動延長変更時
    ''' </summary>
    Public Sub txtLeaseAccount_Change()
        Dim leaseAccount As String = Me.txtLeaseAccount.Text.Trim
        Me.txtLeaseAccount.Text = leaseAccount
        Dim findItem As ListItem = Me.lbLeaseAccount.Items.FindByValue(leaseAccount)
        Me.lblLeaseAccountText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblLeaseAccountText.Text = findItem.Text
        End If
    End Sub
    ''' <summary>
    ''' 自動延長変更時
    ''' </summary>
    Public Sub txtTax_Change()
        Dim tax As String = Me.txtTax.Text.Trim
        Me.txtTax.Text = tax
        Dim findItem As ListItem = Me.lbTax.Items.FindByValue(tax)
        Me.lblTaxText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblTaxText.Text = findItem.Text
        End If
    End Sub
    ''' <summary>
    ''' 画面上のデータを登録
    ''' </summary>
    ''' <param name="ds"></param>
    Private Sub EntryDispValues(ds As DataSet)
        Dim dtContract As DataTable = ds.Tables(CONST_DT_NAME_CONTRACT)
        Dim drContract As DataRow = dtContract.Rows(0)
        Dim dtAttachment As DataTable = ds.Tables(C_DTNAME_ATTACHMENT)
        Dim procDate As Date = Now
        Dim contractNo As String = ""
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            'トランザクション実行
            Using sqlTran As SqlTransaction = sqlCon.BeginTransaction
                If Me.GBT00020RValues.NewBrCreate = True Then
                    '契約書No取得
                    contractNo = GBA00015Lease.GetNewContractNo(sqlCon, sqlTran)
                    '契約書テーブル登録
                    InsertContract(contractNo, drContract, sqlCon, sqlTran, procDate)
                    'NoOfAgreement数の協定書を生成
                    Dim noOfAgreement As Integer = CInt(drContract.Item("NOOFAGREEMENT"))
                    If noOfAgreement > 0 Then
                        For i = 1 To noOfAgreement
                            GBA00015Lease.InsertAgreement(contractNo, dtContract, sqlCon, sqlTran, procDate)
                        Next i
                    End If
                Else
                    '契約書テーブルの更新
                    contractNo = Me.GBT00020RValues.ContractNo
                    UpdateContract(drContract, sqlCon, sqlTran, procDate)
                End If
                '添付ファイルを正式フォルダに転送
                CommonFunctions.SaveAttachmentFilesList(dtAttachment, contractNo, CONST_DIRNAME_LEASE_CONTRACT)
                sqlTran.Commit()
            End Using 'End Transaction
        End Using 'End Connection
    End Sub
    ''' <summary>
    ''' 契約書テーブル新規登録
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub InsertContract(contractNo As String, dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("INSERT INTO {0} (", CONST_TBL_CONTRACT).AppendLine()
            sqlStat.AppendLine("   CONTRACTNO ")
            sqlStat.AppendLine("  ,STYMD  ")
            sqlStat.AppendLine("  ,CONTRACTFROM")
            sqlStat.AppendLine("  ,ENABLED")
            sqlStat.AppendLine("  ,SHIPPER")
            sqlStat.AppendLine("  ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("  ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("  ,AUTOEXTEND")
            sqlStat.AppendLine("  ,ACCOUNT")
            sqlStat.AppendLine("  ,TAXKIND")
            sqlStat.AppendLine("  ,INITUSER")
            sqlStat.AppendLine("  ,ORGANIZER")
            sqlStat.AppendLine("  ,COUNRTYORGANIZER")
            sqlStat.AppendLine("  ,ACCSEGMENT")
            sqlStat.AppendLine("  ,REMARK")
            sqlStat.AppendLine("  ,DELFLG")
            sqlStat.AppendLine("  ,INITYMD")
            sqlStat.AppendLine("  ,UPDYMD")
            sqlStat.AppendLine("  ,UPDUSER")
            sqlStat.AppendLine("  ,UPDTERMID")
            sqlStat.AppendLine("  ,RECEIVEYMD")
            sqlStat.AppendLine(") VALUES (")
            sqlStat.AppendLine("   @CONTRACTNO ")
            sqlStat.AppendLine("  ,@STYMD  ")
            sqlStat.AppendLine("  ,@CONTRACTFROM")
            sqlStat.AppendLine("  ,@ENABLED")
            sqlStat.AppendLine("  ,@SHIPPER")
            sqlStat.AppendLine("  ,@LEASEPAYMENTTYPE")
            sqlStat.AppendLine("  ,@LEASEPAYMENTKIND")
            sqlStat.AppendLine("  ,@AUTOEXTEND")
            sqlStat.AppendLine("  ,@ACCOUNT")
            sqlStat.AppendLine("  ,@TAXKIND")
            sqlStat.AppendLine("  ,@INITUSER")
            sqlStat.AppendLine("  ,@ORGANIZER")
            sqlStat.AppendLine("  ,@COUNRTYORGANIZER")
            sqlStat.AppendLine("  ,@ACCSEGMENT")
            sqlStat.AppendLine("  ,@REMARK")
            sqlStat.AppendLine("  ,@DELFLG")
            sqlStat.AppendLine("  ,@INITYMD")
            sqlStat.AppendLine("  ,@UPDYMD")
            sqlStat.AppendLine("  ,@UPDUSER")
            sqlStat.AppendLine("  ,@UPDTERMID")
            sqlStat.AppendLine("  ,@RECEIVEYMD")
            sqlStat.AppendLine(")")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = contractNo
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@CONTRACTFROM", SqlDbType.Date).Value = dr.Item("CONTRACTFROM")
                    .Add("@ENABLED", SqlDbType.NVarChar).Value = dr.Item("ENABLED")
                    .Add("@SHIPPER", SqlDbType.NVarChar).Value = dr.Item("SHIPPER")
                    .Add("@LEASEPAYMENTTYPE", SqlDbType.NVarChar).Value = dr.Item("LEASEPAYMENTTYPE")
                    .Add("@LEASEPAYMENTKIND", SqlDbType.NVarChar).Value = dr.Item("LEASEPAYMENTKIND")
                    .Add("@AUTOEXTEND", SqlDbType.NVarChar).Value = dr.Item("AUTOEXTEND")
                    .Add("@ACCOUNT", SqlDbType.NVarChar).Value = dr.Item("ACCOUNT")
                    .Add("@TAXKIND", SqlDbType.NVarChar).Value = dr.Item("TAXKIND")
                    .Add("@INITUSER", SqlDbType.NVarChar).Value = dr.Item("INITUSER")
                    .Add("@ORGANIZER", SqlDbType.NVarChar).Value = dr.Item("ORGANIZER")
                    .Add("@COUNRTYORGANIZER", SqlDbType.NVarChar).Value = dr.Item("COUNRTYORGANIZER")
                    .Add("@ACCSEGMENT", SqlDbType.NVarChar).Value = dr.Item("ACCSEGMENT")
                    .Add("@REMARK", SqlDbType.NVarChar).Value = dr.Item("REMARK")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                End With
                sqlCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Throw ex
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 契約書テーブル更新
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub UpdateContract(dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
        '本当はDeleteFlg立てInsertすること、説明会向けの為UPDATE文のみ
        Dim canCloseConnect As Boolean = False

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("UPDATE {0} ", CONST_TBL_CONTRACT).AppendLine()
            sqlStat.AppendLine("   SET  CONTRACTFROM     = @CONTRACTFROM")
            sqlStat.AppendLine("       ,ENABLED          = @ENABLED")
            sqlStat.AppendLine("       ,SHIPPER          = @SHIPPER")
            sqlStat.AppendLine("       ,LEASEPAYMENTTYPE = @LEASEPAYMENTTYPE")
            sqlStat.AppendLine("       ,LEASEPAYMENTKIND = @LEASEPAYMENTKIND")
            sqlStat.AppendLine("       ,AUTOEXTEND       = @AUTOEXTEND")
            sqlStat.AppendLine("       ,ACCOUNT          = @ACCOUNT")
            sqlStat.AppendLine("       ,TAXKIND          = @TAXKIND")
            sqlStat.AppendLine("       ,INITUSER         = @INITUSER")
            sqlStat.AppendLine("       ,ORGANIZER        = @ORGANIZER")
            sqlStat.AppendLine("       ,COUNRTYORGANIZER = @COUNRTYORGANIZER")
            sqlStat.AppendLine("       ,ACCSEGMENT       = @ACCSEGMENT")
            sqlStat.AppendLine("       ,REMARK           = @REMARK")
            sqlStat.AppendLine("       ,UPDYMD           = @UPDYMD")
            sqlStat.AppendLine("       ,UPDUSER          = @UPDUSER")
            sqlStat.AppendLine("       ,UPDTERMID        = @UPDTERMID")
            sqlStat.AppendLine("       ,RECEIVEYMD       = @RECEIVEYMD")
            sqlStat.AppendLine(" WHERE CONTRACTNO = @CONTRACTNO ")
            sqlStat.AppendLine("   AND DELFLG    <> @DELFLG ")
            sqlStat.AppendLine("   AND STYMD     <= @DATENOW ")
            sqlStat.AppendLine("   AND ENDYMD    >= @DATENOW ")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = dr.Item("CONTRACTNO")
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@CONTRACTFROM", SqlDbType.Date).Value = dr.Item("CONTRACTFROM")
                    .Add("@ENABLED", SqlDbType.NVarChar).Value = dr.Item("ENABLED")
                    .Add("@SHIPPER", SqlDbType.NVarChar).Value = dr.Item("SHIPPER")
                    .Add("@LEASEPAYMENTTYPE", SqlDbType.NVarChar).Value = dr.Item("LEASEPAYMENTTYPE")
                    .Add("@LEASEPAYMENTKIND", SqlDbType.NVarChar).Value = dr.Item("LEASEPAYMENTKIND")
                    .Add("@AUTOEXTEND", SqlDbType.NVarChar).Value = dr.Item("AUTOEXTEND")
                    .Add("@ACCOUNT", SqlDbType.NVarChar).Value = dr.Item("ACCOUNT")
                    .Add("@TAXKIND", SqlDbType.NVarChar).Value = dr.Item("TAXKIND")
                    .Add("@INITUSER", SqlDbType.NVarChar).Value = dr.Item("INITUSER")
                    .Add("@ORGANIZER", SqlDbType.NVarChar).Value = dr.Item("ORGANIZER")
                    .Add("@COUNRTYORGANIZER", SqlDbType.NVarChar).Value = dr.Item("COUNRTYORGANIZER")
                    .Add("@ACCSEGMENT", SqlDbType.NVarChar).Value = dr.Item("ACCSEGMENT")
                    .Add("@REMARK", SqlDbType.NVarChar).Value = dr.Item("REMARK")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    .Add("@DATENOW", SqlDbType.Date).Value = Now
                End With
                sqlCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Throw ex
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try
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

        AddLangSetting(dicDisplayText, Me.lblContractPerson, "契約担当者", "Contract Person")
        AddLangSetting(dicDisplayText, Me.lblBrInfoHeader, "Lease-Info", "Lease-Info")

        AddLangSetting(dicDisplayText, Me.lblLeasePriod, "リース期間", "Lease Period")
        AddLangSetting(dicDisplayText, Me.lblEnabled, "使用可", "Enabled")
        AddLangSetting(dicDisplayText, Me.lblLeasePaymentType, "支払月", "Payment Month")
        AddLangSetting(dicDisplayText, Me.lblLeasePaymentKind, "種別", "Kind")
        AddLangSetting(dicDisplayText, Me.lblAutoExtend, "自動延長", "Auto Extend")
        AddLangSetting(dicDisplayText, Me.lblLeaseAccount, "振込口座", "Account")
        AddLangSetting(dicDisplayText, Me.lblTax, "税区分", "Tax Kind")
        AddLangSetting(dicDisplayText, Me.lblNoOfAgreement, "協定書枚数", "No Of Agreement")
        AddLangSetting(dicDisplayText, Me.lblRemarks, "備考", "Remarks")
        AddLangSetting(dicDisplayText, Me.lblAttachment, "添付", "Attachment")
        AddLangSetting(dicDisplayText, Me.btnDownloadFiles, "ファイルダウンロード", "File Download")

        AddLangSetting(dicDisplayText, Me.lblAppDate, "DATE", "DATE")
        AddLangSetting(dicDisplayText, Me.lblAppAgent, "AGENT", "AGENT")
        AddLangSetting(dicDisplayText, Me.lblAppPic, "PIC", "PIC")
        AddLangSetting(dicDisplayText, Me.lblAppRemarks, "REMARKS", "REMARKS")
        AddLangSetting(dicDisplayText, Me.lblApply, "Apply", "Apply")
        AddLangSetting(dicDisplayText, Me.lblApproved, "Approved", "Approved")

        AddLangSetting(dicDisplayText, Me.lblShipperTel, "Tel", "Tel")
        AddLangSetting(dicDisplayText, Me.lblShipperAddress, "Address", "Address")

        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主", "Shipper")

        AddLangSetting(dicDisplayText, Me.lblAccSegment, "Segment", "Segment")

        '****************************************
        ' 添付ファイルヘッダー部
        '****************************************
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderText, "添付書類を登録する場合は、ここにドロップすること", "To register attached documents, drop it here")
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderFileName, "ファイル名", "FileName")
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderDelete, "削 除", "Delete")
        '****************************************
        ' 各種ボタン
        '****************************************
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnOutputExcel, "Excel出力", "Output Excel")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        '****************************************
        '左ボックス
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblRightListDiscription, "印刷・インポート設定", "Print/Import Settings")

        '****************************************
        ' 隠しフィールド
        '****************************************
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

    End Sub

    ''' <summary>
    ''' 遷移元（前画面）の情報を取得
    ''' </summary>
    Private Function GetPrevDisplayInfo(ByRef retDataSet As DataSet) As String

        Dim GBA00006PortRelated As GBA00006PortRelated = New GBA00006PortRelated
        Dim dummyList As ListBox = New ListBox
        Dim retVal As String = C_MESSAGENO.NORMAL

        If TypeOf Page.PreviousPage Is GBT00020LEASE Then
            '自身からの遷移(Save時に反応)
            Dim brNo As String = ""
            Dim prevPage As GBT00020LEASE = DirectCast(Page.PreviousPage, GBT00020LEASE)
            ViewState("GBT00020RValues") = prevPage.GBT00020RValues
        ElseIf TypeOf Page.PreviousPage Is GBT00020RESULT Then
            '一覧からの遷移
            Dim brNo As String = ""
            Dim prevPage As GBT00020RESULT = DirectCast(Page.PreviousPage, GBT00020RESULT)
            ViewState("GBT00020RValues") = prevPage.ThisScreenValue
            Me.GBT00020RValues = prevPage.ThisScreenValue
            'TODO Me.hdnThisMapVariant .Value によるさらなる分岐
            Dim dtDisp As DataTable = CreateDispDt()

            If GBT00020RValues.NewBrCreate = True Then
                '新規作成時
                dtDisp = GetNewConstractValue(dtDisp)
                dtDisp.Rows(0).Item("INITUSER") = COA0019Session.USERID
                dtDisp.Rows(0).Item("ORGANIZER") = GBA00003UserSetting.OFFICECODE
                dtDisp.Rows(0).Item("COUNRTYORGANIZER") = GBA00003UserSetting.COUNTRYCODE
            Else
                '変更時
                dtDisp = GetContractItem(dtDisp, Me.GBT00020RValues.ContractNo)
            End If
            retDataSet.Tables.Add(dtDisp)
        ElseIf TypeOf Page.PreviousPage Is GBT00005APPROVAL Then
            '↑承認画面変更を

        ElseIf Page.PreviousPage Is Nothing Then
            '遷移元のページが無いためエラースロー(F5を押された場合等に継続させない)
            Throw New Exception("PreviousPage is none")
        End If
        CommonFunctions.CleanUserTempDirectory(CONST_MAPID)
        Dim dtAttachment As DataTable = CommonFunctions.GetInitAttachmentFileList(Me.GBT00020RValues.ContractNo, CONST_DIRNAME_LEASE_CONTRACT, CONST_MAPID)
        retDataSet.Tables.Add(dtAttachment)
        Return retVal
    End Function
    ''' <summary>
    ''' 画面上の初期値データテーブルを生成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateDispDt() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = CONST_DT_NAME_CONTRACT
        With retDt.Columns
            .Add("CONTRACTNO", GetType(String)).DefaultValue = ""
            .Add("CONTRACTFROM", GetType(String)).DefaultValue = ""
            .Add("ENABLED", GetType(String)).DefaultValue = ""
            .Add("SHIPPER", GetType(String)).DefaultValue = ""
            .Add("LEASEPAYMENTTYPE", GetType(String)).DefaultValue = ""
            .Add("LEASEPAYMENTKIND", GetType(String)).DefaultValue = ""
            .Add("AUTOEXTEND", GetType(String)).DefaultValue = ""
            .Add("ACCOUNT", GetType(String)).DefaultValue = ""
            .Add("TAXKIND", GetType(String)).DefaultValue = ""
            .Add("INITUSER", GetType(String)).DefaultValue = ""
            .Add("ORGANIZER", GetType(String)).DefaultValue = ""
            .Add("COUNRTYORGANIZER", GetType(String)).DefaultValue = ""
            .Add("ACCSEGMENT", GetType(String)).DefaultValue = ""
            .Add("REMARK", GetType(String)).DefaultValue = ""
            .Add("NOOFAGREEMENT", GetType(String)).DefaultValue = ""
            .Add("NOOFAGREEMENTAPPLOVED", GetType(String)).DefaultValue = ""
            .Add("NOOFTOTALTANKS", GetType(String)).DefaultValue = ""
            .Add("NOOFFINISHTANKS", GetType(String)).DefaultValue = ""
            .Add("INITYMD", GetType(String)).DefaultValue = ""
            .Add("UPDYMD", GetType(String)).DefaultValue = ""
            .Add("UPDUSER", GetType(String)).DefaultValue = ""
            .Add("UPDTERMID", GetType(String)).DefaultValue = ""
        End With
        Return retDt
    End Function

    ''' <summary>
    ''' 画面上に新規情報を追加
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <returns></returns>
    Private Function GetNewConstractValue(dt As DataTable) As DataTable
        Dim retDt As DataTable = dt.Clone 'フィールドのガワをコピー
        Dim COA0016VARIget As New COA0016VARIget
        Dim dr = retDt.NewRow
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        Dim targetFields As New List(Of String) From {"SHIPPER", "CONTRACTFROM", "ENABLED",
                                                      "LEASEPAYMENTTYPE",
                                                      "LEASEPAYMENTKIND", "AUTOEXTEND", "ACCOUNT",
                                                      "TAXKIND", "NOOFAGREEMENT", "REMARK"}

        For Each fieldName In targetFields
            COA0016VARIget.FIELD = fieldName
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                dr(fieldName) = COA0016VARIget.VALUE
            Else
                Throw New Exception("GetNewConstructValue Error")
            End If
        Next
        dr.Item("NOOFAGREEMENTAPPLOVED") = "0"
        retDt.Rows.Add(dr)
        Return retDt
    End Function
    ''' <summary>
    ''' テーブルより契約書データを取得
    ''' </summary>
    ''' <param name="dt">入力用データテーブル</param>
    ''' <param name="contractNo">契約書No</param>
    ''' <returns></returns>
    Private Function GetContractItem(dt As DataTable, contractNo As String) As DataTable
        Dim retDt As DataTable = dt.Clone
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT CTR.CONTRACTNO")
        sqlStat.AppendLine("     , CASE CTR.CONTRACTFROM   WHEN '1900/01/01' THEN '' ELSE FORMAT(CTR.CONTRACTFROM,  'yyyy/MM/dd') END AS CONTRACTFROM")
        sqlStat.AppendLine("     , CTR.ENABLED")
        sqlStat.AppendLine("     , CTR.SHIPPER")
        sqlStat.AppendLine("     , CTR.LEASEPAYMENTTYPE")
        sqlStat.AppendLine("     , CTR.LEASEPAYMENTKIND")
        sqlStat.AppendLine("     , CTR.AUTOEXTEND")
        sqlStat.AppendLine("     , CTR.ACCOUNT")
        sqlStat.AppendLine("     , CTR.TAXKIND")
        sqlStat.AppendLine("     , CTR.INITUSER")
        sqlStat.AppendLine("     , CTR.ORGANIZER")
        sqlStat.AppendLine("     , CTR.COUNRTYORGANIZER")
        sqlStat.AppendLine("     , CTR.ACCSEGMENT")
        sqlStat.AppendLine("     , CTR.REMARK")
        sqlStat.AppendLine("     , (SELECT COUNT(AGR.AGREEMENTNO) ")
        sqlStat.AppendFormat("          FROM {0} AGR", CONST_TBL_AGREEMENT).AppendLine()
        sqlStat.AppendLine("         WHERE AGR.CONTRACTNO = CTR.CONTRACTNO")
        sqlStat.AppendLine("           AND AGR.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("           AND AGR.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("           AND AGR.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("       ) AS NOOFAGREEMENT")

        sqlStat.AppendLine("     , (SELECT COUNT(AGR.AGREEMENTNO) ")
        sqlStat.AppendFormat("          FROM {0} AGR", CONST_TBL_AGREEMENT).AppendLine()
        sqlStat.AppendLine("       INNER JOIN COT0002_APPROVALHIST AH")
        sqlStat.AppendLine("              ON  AH.COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("             AND  AH.APPLYID     = AGR.APPLYID")
        sqlStat.AppendLine("             AND  AH.STEP        = AGR.LASTSTEP")
        sqlStat.AppendLine("             AND  AH.DELFLG     <> @DELFLG")
        sqlStat.AppendFormat("             AND  AH.STATUS      IN ('{0}','{1}') ", C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE).AppendLine()
        sqlStat.AppendLine("         WHERE AGR.CONTRACTNO = CTR.CONTRACTNO")
        sqlStat.AppendLine("           AND AGR.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("           AND AGR.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("           AND AGR.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("       ) AS NOOFAGREEMENTAPPLOVED")
        '契約書に紐づくタンク総数(タンク総数=終了タンク総数の場合のみEnabledを操作可能にする）
        sqlStat.AppendLine("     , (SELECT COUNT(TNK.TANKNO) ")
        sqlStat.AppendFormat("          FROM {0} TNK", CONST_TBL_TANK).AppendLine()
        sqlStat.AppendLine("         WHERE TNK.CONTRACTNO = CTR.CONTRACTNO")
        sqlStat.AppendLine("           AND TNK.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("           AND TNK.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("           AND TNK.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("       ) AS NOOFTOTALTANKS")
        '契約書に紐づく終了タンク総数(タンク総数=終了タンク総数の場合のみEnabledを操作可能にする）
        sqlStat.AppendLine("     , (SELECT COUNT(TNK.TANKNO) ")
        sqlStat.AppendFormat("          FROM {0} TNK", CONST_TBL_TANK).AppendLine()
        sqlStat.AppendLine("         WHERE TNK.CONTRACTNO = CTR.CONTRACTNO")
        sqlStat.AppendLine("           AND TNK.STYMD       <= @NOWDATE")
        sqlStat.AppendLine("           AND TNK.ENDYMD      >= @NOWDATE")
        sqlStat.AppendLine("           AND TNK.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("           AND TNK.LEASEENDYMD <> '1900/01/01'")
        sqlStat.AppendLine("       ) AS NOOFFINISHTANKS")

        sqlStat.AppendLine("     , ISNULL(CONVERT(nvarchar, CTR.INITYMD , 120),'') AS INITYMD")
        sqlStat.AppendLine("     , ISNULL(CONVERT(nvarchar, CTR.UPDYMD , 120),'') AS UPDYMD")
        sqlStat.AppendLine("     , ISNULL(RTRIM(CTR.UPDUSER),'')                  AS UPDUSER")
        sqlStat.AppendLine("     , ISNULL(RTRIM(CTR.UPDTERMID),'')                AS UPDTERMID")
        sqlStat.AppendFormat("        FROM {0} CTR", CONST_TBL_CONTRACT).AppendLine()

        'sqlStat.AppendLine("          LEFT JOIN GBM0004_CUSTOMER SP") 'SHIPPER名称用JOIN
        'sqlStat.AppendLine("            ON  SP.COMPCODE     = @COMPCODE")
        ''sqlStat.AppendLine("           AND  SP.COUNTRYCODE  = BS.LOADCOUNTRY1")
        'sqlStat.AppendLine("           AND  SP.CUSTOMERCODE = CTR.SHIPPER")
        'sqlStat.AppendLine("           AND  SP.STYMD       <= @NOWDATE")
        'sqlStat.AppendLine("           AND  SP.ENDYMD      >= @NOWDATE")
        'sqlStat.AppendLine("           AND  SP.DELFLG      <> @DELFLG")
        'sqlStat.AppendLine("           AND  SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("         WHERE CTR.CONTRACTNO  = @CONTRACTNO")
        sqlStat.AppendLine("           AND CTR.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("           AND CTR.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("           AND CTR.DELFLG     <> @DELFLG")
        Dim dtDbResult As New DataTable
        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = contractNo
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next

        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                writeDr.Item(colName) = readDr.Item(colName)
            Next
            retDt.Rows.Add(writeDr)
        Next

        Return retDt

    End Function
    ''' <summary>
    ''' 画面表示処理
    ''' </summary>
    ''' <param name="dt"></param>
    Private Sub SetDisplayFromDt(dt As DataTable)
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)
        Me.lblContractPersonName.Text = Convert.ToString(dr.Item("INITUSER"))
        Me.lblBrNo.Text = Convert.ToString(dr.Item("CONTRACTNO"))
        Me.txtShipper.Text = Convert.ToString(dr.Item("SHIPPER"))
        Me.txtLeaseFrom.Text = FormatDateContrySettings(Convert.ToString(dr.Item("CONTRACTFROM")), GBA00003UserSetting.DATEFORMAT)
        If dr.Item("ENABLED").Equals(CONST_FLAG_YES) Then
            Me.chkEnabled.Checked = True
        Else
            Me.chkEnabled.Checked = False
        End If
        'Me.txtLeaseTo.Text = FormatDateContrySettings(Convert.ToString(dr.Item("CONTRACTTO")), GBA00003UserSetting.DATEFORMAT)
        'Me.txtLeaseTerm.Text = Convert.ToString(dr.Item("LEASETERM"))
        'Me.txtLeasePayment.Text = Convert.ToString(dr.Item("LEASEPAYMENT"))
        Me.txtLeasePaymentType.Text = Convert.ToString(dr.Item("LEASEPAYMENTTYPE"))
        Me.txtLeasePaymentKind.Text = Convert.ToString(dr.Item("LEASEPAYMENTKIND"))
        Me.txtAutoExtend.Text = Convert.ToString(dr.Item("AUTOEXTEND"))
        Me.txtLeaseAccount.Text = Convert.ToString(dr.Item("ACCOUNT"))
        Me.txtTax.Text = Convert.ToString(dr.Item("TAXKIND"))
        Me.txtNoOfAgreement.Text = Convert.ToString(dr.Item("NOOFAGREEMENT"))
        Me.txtRemarks.Text = Convert.ToString(dr.Item("REMARK"))
        Me.txtAccSegment.Text = Convert.ToString(dr.Item("ACCSEGMENT"))

        '付帯文言を展開
        If GBT00020RValues.NewBrCreate = False Then
            txtShipper_Change()
            txtLeasePaymentType_Change()
            txtLeasePaymentKind_Change()
            txtAutoExtend_Change()
            txtLeaseAccount_Change()
            txtTax_Change()
        End If
    End Sub
    ''' <summary>
    ''' 画面入力情報を取得しデータセットに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function CollectDisplay() As DataSet
        Dim ds As DataSet = DirectCast(ViewState(CONST_VS_NAME_CUR_VAL), DataSet)
        Dim retDs As New DataSet
        Dim dt = ds.Tables(CONST_DT_NAME_CONTRACT)
        Dim dispDt As DataTable = dt.Clone
        Dim dispDr = dispDt.NewRow
        dispDr.ItemArray = dt.Rows(0).ItemArray '初回読み込みの全情報をコピー
        dispDr.Item("SHIPPER") = Me.txtShipper.Text
        dispDr.Item("CONTRACTFROM") = FormatDateYMD(Me.txtLeaseFrom.Text, GBA00003UserSetting.DATEFORMAT)
        If Me.chkEnabled.Checked Then
            dispDr.Item("ENABLED") = CONST_FLAG_YES
        Else
            dispDr.Item("ENABLED") = CONST_FLAG_NO
        End If
        'dispDr.Item("CONTRACTTO") = FormatDateYMD(Me.txtLeaseTo.Text, GBA00003UserSetting.DATEFORMAT)
        'dispDr.Item("LEASETERM") = Me.txtLeaseTerm.Text
        'dispDr.Item("LEASEPAYMENT") = Me.txtLeasePayment.Text
        dispDr.Item("LEASEPAYMENTTYPE") = Me.txtLeasePaymentType.Text
        dispDr.Item("LEASEPAYMENTKIND") = Me.txtLeasePaymentKind.Text
        dispDr.Item("AUTOEXTEND") = Me.txtAutoExtend.Text
        dispDr.Item("ACCOUNT") = Me.txtLeaseAccount.Text
        dispDr.Item("TAXKIND") = Me.txtTax.Text
        dispDr.Item("NOOFAGREEMENT") = Me.txtNoOfAgreement.Text
        dispDr.Item("REMARK") = Me.txtRemarks.Text
        dispDr.Item("ACCSEGMENT") = Me.txtAccSegment.Text
        dispDt.Rows.Add(dispDr)
        retDs.Tables.Add(dispDt)
        '添付ファイルの収集
        Dim dtAttachment As DataTable = CommonFunctions.DeepCopy(ds.Tables(C_DTNAME_ATTACHMENT))
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
        retDs.Tables.Add(dtAttachment)
        Return retDs
    End Function
    ''' <summary>
    ''' 添付ファイル欄の添付ファイル名ダブルクリック時処理
    ''' </summary>
    Private Sub AttachmentFileNameDblClick()
        Dim fileName As String = Me.hdnFileDisplay.Value
        If fileName = "" Then
            Return
        End If
        Dim dtAttachment As DataTable = Me.DsDisDisplayValues.Tables(C_DTNAME_ATTACHMENT)
        Dim dlUrl As String = CommonFunctions.GetAttachfileDownloadUrl(dtAttachment, fileName)
        Me.hdnPrintURL.Value = dlUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
    End Sub
    ''' <summary>
    ''' 使用可否制御
    ''' </summary>
    Private Sub enabledControls()
        Dim dtCont As DataTable = Me.DsDisDisplayValues.Tables(CONST_DT_NAME_CONTRACT)
        Dim drCont As DataRow = dtCont.Rows(0)
        Dim inputControls As New List(Of Control) From {Me.txtShipper, Me.txtLeaseFrom, Me.txtLeasePaymentType,
                                                        Me.txtLeasePaymentKind, Me.txtAutoExtend,
                                                        Me.txtLeaseAccount, Me.txtTax,
                                                        Me.txtRemarks, Me.txtAccSegment}
        Dim inputEnabled As Boolean = True


        'chkEnabledはタンクが紐づいていても終了日が未入力の場合使用不可(総タンク数=終了日入力済タンク数)
        If Not Convert.ToString(drCont.Item("NOOFTOTALTANKS")) = Convert.ToString(drCont.Item("NOOFFINISHTANKS")) Then
            Me.chkEnabled.Enabled = False
        End If
        '承認された協定書があればまたは契約書のENABLEDがFALSEの場合、入力不可
        If Convert.ToString(drCont.Item("NOOFAGREEMENTAPPLOVED")) <> "0" OrElse
           Convert.ToString(drCont.Item("ENABLED")) <> "Y" Then
            inputEnabled = False
        End If
        '承認済みデータを1件でも持っていた場合は編集不可
        '新規作成時のみ協定書枚数入力可能
        If Me.GBT00020RValues.NewBrCreate = False Then
            Me.txtNoOfAgreement.Enabled = False
        End If

        For Each inputControl In inputControls
            If TypeOf inputControl Is TextBox Then
                Dim txtObj As TextBox = DirectCast(inputControl, TextBox)
                txtObj.Enabled = inputEnabled
            End If
        Next
        If inputEnabled = False Then
            Me.hdnUpload.Enabled = False
        End If
    End Sub
    ''' <summary>
    ''' 左ボックスのリストデータをクリア
    ''' </summary>
    ''' <remarks>viewstateのデータ量軽減</remarks>
    Private Sub ClearLeftListData()
        Me.lbPort.Items.Clear()
        Me.lbShipper.Items.Clear()
    End Sub

    ''' <summary>
    ''' 荷主一覧取得
    ''' </summary>
    ''' <param name="customerCode">顧客コード(オプショナル)未指定時は国コードで絞りこんだ全件</param>
    ''' <returns>荷主一覧データテーブル</returns>
    ''' <remarks>GBM0004_CUSTOMERより荷主一覧を取得</remarks>
    Private Function GetShipper(Optional customerCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（いったん固定
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CUSTOMERCODE")
        sqlStat.AppendFormat("      ,{0} AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,CUSTOMERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("      ,ADDR  AS ADDR")
        sqlStat.AppendLine("      ,TEL   AS TEL")
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        'sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
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
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = customerCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' Fixvalueを元にリストボックスを作成
    ''' </summary>
    ''' <param name="className"></param>
    ''' <param name="targetList"></param>
    ''' <remarks>動的要素ではなくロード時に設定で済むもののみに絞ること</remarks>
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
    ''' 荷主名称を画面に設定
    ''' </summary>
    ''' <param name="targetTextObject">対象テキスト</param>
    ''' <param name="customerCode">荷主コード（顧客コード）</param>
    Private Sub SetDisplayShipper(targetTextObject As TextBox, customerCode As String)
        '一旦リセット
        targetTextObject.Text = customerCode.Trim
        Me.lblShipperText.Text = ""
        '荷主コード（顧客コード）が未入力の場合はDBアクセスせずに終了
        If customerCode.Trim = "" Then
            Return
        End If
        Dim dt As DataTable = GetShipper(customerCode.Trim)

        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)
        Me.lblShipperText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
        Me.lblShipperAddressText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("ADDR")))
        Me.lblShipperTelText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("TEL")))
    End Sub

    ''' <summary>
    ''' 左の出力帳票
    ''' </summary>
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
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="inColName"></param>
    ''' <param name="inText"></param>
    Protected Function CheckSingle(ByVal inColName As String, ByVal inText As String) As String

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck      '項目チェック

        '〇単項目チェック
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELD = inColName
        COA0026FieldCheck.VALUE = inText
        COA0026FieldCheck.COA0026FieldCheck()
        If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
            Return C_MESSAGENO.NORMAL
        Else
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            Return COA0026FieldCheck.ERR
        End If

    End Function
    ''' <summary>
    ''' 画面データの変更チェック
    ''' </summary>
    ''' <returns>True:変更あり、False:変更なし</returns>
    Public Function IsModifiedData() As Boolean
        '画面入力情報の収集
        Dim dispDs As DataSet = Me.DsDisDisplayValues
        Dim prevDs As DataSet = DirectCast(ViewState(CONST_VS_NAME_PREV_VAL), DataSet)
        '新規作成の場合は変更ありと判定
        If Me.GBT00020RValues.NewBrCreate = True Then
            Return True
        End If
        '添付ファイルの個数判定
        With Nothing
            Dim dispAttachFileCnt As Integer = 0
            Dim prevAttachFileCnt As Integer = 0
            Dim dispAttachDt = dispDs.Tables(C_DTNAME_ATTACHMENT)
            Dim prevAttachDt = prevDs.Tables(C_DTNAME_ATTACHMENT)
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
        '変更前後の入力値を比較し変更を判定
        'データテーブル名とチェックフィールド
        Dim dicModCheck As New Dictionary(Of String, List(Of String))
        dicModCheck.Add(CONST_DT_NAME_CONTRACT,
                        New List(Of String) From {"SHIPPER", "CONTRACTFROM",
                                                  "ENABLED",
                                                  "LEASEPAYMENTTYPE", "LEASEPAYMENTKIND",
                                                  "AUTOEXTEND", "ACCOUNT", "TAXKIND",
                                                  "ACCSEGMENT", "REMARK"})
        dicModCheck.Add(C_DTNAME_ATTACHMENT,
                        New List(Of String) From {"FILENAME", "DELFLG", "ISMODIFIED"})
        For Each modCheckItem In dicModCheck
            Dim dispDt As DataTable = dispDs.Tables(modCheckItem.Key)
            Dim prevDt As DataTable = prevDs.Tables(modCheckItem.Key)
            Dim maxRowIdx As Integer = dispDt.Rows.Count - 1
            For rowIdx = 0 To maxRowIdx
                Dim dispDr As DataRow = dispDt.Rows(rowIdx)
                Dim prevDr As DataRow = prevDt.Rows(rowIdx)
                For Each fieldName In modCheckItem.Value
                    If Not dispDr(fieldName).Equals(prevDr(fieldName)) Then
                        '対象フィールドの値に変更があった場合
                        Return True
                    End If
                Next fieldName 'フィールドループ
            Next 'データテーブル行ループ
        Next modCheckItem 'チェックデータテーブルループ

        'ここまでくれば変更なし
        Return False
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
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    Protected Sub UploadExcel()

        'If returnCode = C_MESSAGENO.NORMAL Then
        '    CommonFunctions.ShowMessage(C_MESSAGENO.NORMALUPLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
        'End If

    End Sub
    ''' <summary>
    ''' 添付ファイルアップロード処理
    ''' </summary>
    Private Sub UploadAttachment()
        Dim ds As DataSet = Me.DsDisDisplayValues
        Dim dtAttachment As DataTable = ds.Tables(C_DTNAME_ATTACHMENT)
        Dim chkMsgNo = CommonFunctions.CheckUploadAttachmentFile(dtAttachment)
        If chkMsgNo <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(chkMsgNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        dtAttachment = CommonFunctions.UploadAttachmentFile(dtAttachment, Me.GBT00020RValues.ContractNo, CONST_MAPID)
        ds.Tables.Remove(C_DTNAME_ATTACHMENT)
        ds.Tables.Add(dtAttachment)
        repAttachment.DataSource = dtAttachment
        repAttachment.DataBind()
        Me.DsDisDisplayValues = ds
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
    ''' 荷主変更時イベント
    ''' </summary>
    Public Sub txtShipper_Change()
        SetDisplayShipper(Me.txtShipper, Me.txtShipper.Text)
    End Sub

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

End Class