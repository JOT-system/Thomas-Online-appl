Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' リースブレーカー協定書入力
''' </summary>
Public Class GBT00020AGREEMENT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00020A" '自身のMAPID
    Private Const PRODUCT_NONDG As String = "NON-DG"
    'DBテーブル名
    Private Const CONST_TBL_CONTRACT As String = "GBT0010_LBR_CONTRACT"
    Private Const CONST_TBL_AGREEMENT As String = "GBT0011_LBR_AGREEMENT"
    Private Const CONST_TBL_TANK As String = "GBT0012_RESRVLEASETANK"
    '内部保持のデータテーブル名称
    Private Const CONST_DT_NAME_CONTRACT As String = "CONTRACT"
    Private Const CONST_DT_NAME_AGREEMENT As String = "AGREEMENT"
    Private Const CONST_DT_NAME_TANKINFO As String = "TANKINFO"

    Private Const CONST_DT_NAME_TANKINFO_TO_ORDER As String = "TANKINFO_TOORDER"
    Private Const CONST_DT_NAME_ORDERBASE As String = "ODERBASE"
    'VIEWSTATE名
    Private Const CONST_VS_NAME_PREV_VAL As String = "PREVVAL"
    Private Const CONST_VS_NAME_CURRENT_VAL As String = "CURRENTVAL"
    Private Const CONST_DIRNAME_LEASE_AGREEMENT As String = "LEASE\AGREEMENT"

    Private Const CONST_VS_NAME_GBT00020SV As String = "GBT00020RValues"
    Private Const CONST_VS_NAME_GBT00024AV As String = "GBT00024AValues"

    Private Const CONST_MAPVARI As String = "GB_AGREEMENT"
    ''' <summary>
    ''' リースブレーカー検索結果画面情報
    ''' </summary>
    ''' <returns></returns>
    Public Property GBT00020RValues As GBT00020RESULT.GBT00020RValues
    Public Property GBT00020AGREEMENTValues As GBT00020AGREEMENT.GBT0020AGREEMENTDispItem
    ''' <summary>
    ''' 申請画面情報保持クラス
    ''' </summary>
    ''' <returns></returns>
    Public Property GBT00024AValues As GBT00024APPROVAL.GBT00024RValues
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 自身をリロードする際に保持するメッセージNo
    ''' </summary>
    ''' <returns></returns>
    Public Property PrevMessageNo As String = ""
    Public Property PrevAgreementNo As String = ""
    ''' <summary>
    ''' ポストバック時画面上の情報を保持
    ''' </summary>
    Private DsDisDisplayValues As DataSet

    Public Sub New()

    End Sub

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
                '上部ドロップダウン選択肢の設定
                '****************************************
                Dim orderStartPointList = GetCreateOrderStartPoint()
                If orderStartPointList.Count > 0 Then
                    Me.ddlOrderStart.Items.AddRange(orderStartPointList.ToArray)
                    Me.ddlOrderStart.SelectedValue = "0"
                End If
                '****************************************
                '遷移元よりデータ取得
                '****************************************
                Dim ds As DataSet = New DataSet
                If Me.GetPrevDisplayInfo(ds) <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                Me.DsDisDisplayValues = CommonFunctions.DeepCopy(ds)
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = CONST_MAPVARI
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
                SetFixvalueListItem("LEASECURRENCY", Me.lbLeaseCurrency)
                SetFixvalueListItem("AUTOEXTENDKIND", Me.lbAutoExtendKind)
                SetFixvalueListItem("GENERALFLG", Me.lbYesNo)
                SetFixvalueListItem("LEASETERM", Me.lbLeaseTerm)
                SetFixvalueListItem("LEASEPAYMENT", Me.lbLeaseType)
                SetFixvalueListItem("LEASEPAYMENTMONTH", Me.lbPaymentMonth)
                SetFixvalueListItem("LEASEPAYMENTKIND", Me.lbLeasePaymentKind)
                SetFixvalueListItem("LEASETAX", Me.lbTax)
                SetFixvalueListItem("GENERALFLG", Me.lbYesNo)
                '****************************************
                '取得データを画面展開
                '****************************************
                SetDispValues(Me.DsDisDisplayValues)
                '****************************************
                '使用可否制御
                '****************************************
                enabledControls()
                '****************************************
                '添付ファイル画面展開
                '****************************************
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
                Me.GBT00020RValues = DirectCast(ViewState(CONST_VS_NAME_GBT00020SV), GBT00020RESULT.GBT00020RValues)
                Me.GBT00024AValues = DirectCast(ViewState(CONST_VS_NAME_GBT00024AV), GBT00024APPROVAL.GBT00024RValues)
                Me.DsDisDisplayValues = CollectDispValues()
                SaveDisplayTankList()
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
                ' 備考・初見入力ボックス表示
                '**********************
                If Me.hdnRemarkboxField.Value <> "" Then
                    'その他備考
                    Dim targetControl As Label = DirectCast(Me.FindControl(Me.hdnRemarkboxField.Value), Label)
                    If targetControl.Enabled = False Then
                        Me.btnRemarkInputOk.Disabled = True
                        Me.txtRemarkInput.ReadOnly = True
                    Else
                        Me.btnRemarkInputOk.Disabled = False
                        Me.txtRemarkInput.ReadOnly = False
                    End If
                    Me.txtRemarkInput.Text = HttpUtility.HtmlDecode(targetControl.Text)
                    'マルチライン入力ボックスの表示
                    Me.divRemarkInputBoxWrapper.Style("display") = "block"
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
            ' 一覧表の行ダブルクリック判定
            '**********************
            If Me.hdnListDBclick.Value <> "" Then
                ListRowDbClick()
                Me.hdnListDBclick.Value = ""
                'Return '単票ページにリダイレクトするため念のため処理は終わらせる
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
            ViewState(CONST_VS_NAME_CURRENT_VAL) = Me.DsDisDisplayValues
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
                Case vLeftDepot.ID
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    Dim dt As DataTable = GetDepot()
                    With Me.lbDepot
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With
                    '入力済のデータを選択状態にする
                    If dblClickField IsNot Nothing AndAlso lbDepot.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbDepot.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If

                Case Else
                    Dim dicListId As New Dictionary(Of String, ListBox) _
                         From {{Me.vLeftAutoExtendKind.ID, Me.lbAutoExtendKind},
                               {Me.vLeftLeaseCurrency.ID, Me.lbLeaseCurrency},
                               {Me.vLeftYesNo.ID, Me.lbYesNo},
                               {Me.vLeftLeaseTerm.ID, Me.lbLeaseTerm}, {Me.vLeftLeaseType.ID, Me.lbLeaseType},
                               {Me.vLeftPaymentMonth.ID, Me.lbPaymentMonth}, {Me.vLeftLeasePaymentKind.ID, Me.lbLeasePaymentKind},
                               {Me.vLeftTax.ID, Me.lbTax}}

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
    ''' ダウンロードボタン押下時
    ''' </summary>
    Public Sub btnDownloadFiles_Click()
        Dim dtAttachment As DataTable = Me.DsDisDisplayValues.Tables(C_DTNAME_ATTACHMENT)
        'ダウンロード対象有無
        If dtAttachment Is Nothing OrElse dtAttachment.Rows.Count = 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.FILENOTEXISTS, Me.lblFooterMessage, pageObject:=Me)
        End If
        Dim dlUrl As String = CommonFunctions.GetAttachmentCompressedFileUrl(dtAttachment, Me.GBT00020RValues.AgreementNo)
        If dlUrl <> "" Then
            Me.hdnPrintURL.Value = dlUrl
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
        End If
        '終了メッセージ
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDOWNLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
    End Sub

    ''' <summary>
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        If IsModifiedData() Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, pageObject:=Me, submitButtonId:="btnBackOk")
            Return
        End If
        '変更を検知しない場合はそのまま前画面へ
        btnBackOk_Click()
    End Sub
    ''' <summary>
    ''' 申請ボタン押下時処理
    ''' </summary>
    Public Sub btnApply_Click()
        If ViewState(CONST_VS_NAME_PREV_VAL) Is Nothing Then

            Return
        End If
        '変更がある場合セーブしてから申請する旨メッセージで表示
        If IsModifiedData() Then
            CommonFunctions.ShowMessage(C_MESSAGENO.HASNOSAVEITEMS, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '****************************************
        '変更前後のデータ取得
        '****************************************
        Dim prevds As DataSet = DirectCast(ViewState(CONST_VS_NAME_PREV_VAL), DataSet)
        Dim workDs As DataSet = CommonFunctions.DeepCopy(Me.DsDisDisplayValues)
        '****************************************
        '入力チェック
        '****************************************
        If CheckInput(workDs, prevds) = False Then
            Return
        End If
        '****************************************
        '申請情報書込
        '****************************************
        Dim dtAgreement As DataTable = workDs.Tables(CONST_DT_NAME_AGREEMENT)
        Dim drAgreement As DataRow = dtAgreement.Rows(0)
        Dim messageNo As String = ApplyProc(drAgreement)
        If messageNo <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowConfirmMessage(messageNo, pageObject:=Me, submitButtonId:="btnBackOk")
            Return
        End If
        'メール

        Me.PrevMessageNo = C_MESSAGENO.NORMALDBENTRY
        Me.PrevAgreementNo = Convert.ToString(drAgreement("AGREEMENTNO"))

        Server.Transfer(Request.Url.LocalPath) '自身を再ロード
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
        'HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        HttpContext.Current.Session("MAPvariant") = "GB_LEASE"
        HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL
        '画面遷移実行()
        Server.Transfer(COA0011ReturnUrl.URL)

    End Sub
    ''' <summary>
    ''' Excel出力ボタン押下時
    ''' </summary>
    Public Sub btnOutputExcel_Click()
    End Sub
    ''' <summary>
    ''' 保存ボタン押下時イベント
    ''' </summary>
    Public Sub btnSave_Click()
        '****************************************
        '変更データ有無チェック
        '****************************************
        If Not IsModifiedData() Then
            '変更が全くない場合はメッセージを表示
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '****************************************
        '変更前後のデータ取得
        '****************************************
        Dim prevds As DataSet = DirectCast(ViewState(CONST_VS_NAME_PREV_VAL), DataSet)
        Dim workDs As DataSet = CommonFunctions.DeepCopy(Me.DsDisDisplayValues)
        '****************************************
        '入力チェック
        '****************************************
        If CheckInput(workDs, prevds) = False Then
            Return
        End If
        '****************************************
        '登録処理
        '****************************************
        Dim agreementNo As String = EntryAgreement(workDs, prevds)
        Me.PrevMessageNo = C_MESSAGENO.NORMALDBENTRY
        Me.PrevAgreementNo = agreementNo
        Server.Transfer(Request.Url.LocalPath) '自身を再ロード
    End Sub
    ''' <summary>
    ''' 入力チェック
    ''' </summary>
    ''' <param name="workDs">画面情報データセット</param>
    ''' <param name="prevDs">遷移直後の画面情報データセット</param>
    ''' <returns></returns>
    Private Function CheckInput(workDs As DataSet, prevDs As DataSet) As Boolean
        '禁則文字の置換
        Dim invChangeAgreementField As New List(Of String) From {"LEASETERM", "LEASETYPE", "PRODUCTCODE", "LEASEPAYMENTS",
                                                                 "LEASEPAYMENTTYPE", "AUTOEXTEND", "AUTOEXTENDKIND",
                                                                 "LEASEPAYMENTKIND", "RELEASE", "CURRENCY", "TAXKIND", "TAXRATE"}

        Dim invChangeTankField As New List(Of String) From {"LEASESTYMD", "LEASEENDYMDSCR", "CANCELFLG", "LEASEENDYMD",
                                                            "DEPOTOUT", "DEPOTIN", "PAYSTDAILY"}

        ChangeInvalidChar(workDs.Tables(CONST_DT_NAME_AGREEMENT), invChangeAgreementField)
        ChangeInvalidChar(workDs.Tables(CONST_DT_NAME_TANKINFO), invChangeAgreementField)
        '******************************
        '単項目チェック
        '******************************
        '上部単票部分
        Dim dicCheckField As New Dictionary(Of String, TextBox) From
        {{"LEASETERM", Me.txtLeaseTerm}, {"LEASETYPE", Me.txtLeaseType},
         {"PRODUCTCODE", Me.txtProduct},
         {"LEASEPAYMENTTYPE", Me.txtPaymentMonth}, {"AUTOEXTEND", Me.txtAutoExtend}, {"AUTOEXTENDKIND", Me.txtAutoExtendKind},
         {"LEASEPAYMENTKIND", Me.txtLeasePaymentKind}, {"LEASEPAYMENTS", Me.txtLeasePayments}, {"RELEASE", Me.txtReLease}, {"CURRENCY", Me.txtReLease},
         {"TAXKIND", Me.txtTax}, {"TAXRATE", Me.txtTaxRate},
         {"REMARK", Me.txtRemarks}
         }

        Dim dr As DataRow = workDs.Tables(CONST_DT_NAME_AGREEMENT).Rows(0)
        For Each singleChkItem As KeyValuePair(Of String, TextBox) In dicCheckField
            Dim fieldName As String = singleChkItem.Key
            Dim chkVal As String = Convert.ToString(dr.Item(fieldName))
            If CheckSingle(fieldName, chkVal) <> C_MESSAGENO.NORMAL Then
                singleChkItem.Value.Focus()
                Return False
            End If
        Next
        '一覧表
        Dim rightMessage As String = ""
        Dim tankChkField As New List(Of String) From {"LEASESTYMD", "LEASEENDYMDSCR", "LEASEENDYMD", "REMARK"}
        Dim keyField As New List(Of String) From {"TANKNO"}
        Dim tankListChkMessage As String = CheckSingle(CONST_MAPID, workDs.Tables(CONST_DT_NAME_TANKINFO), tankChkField, rightMessage, keyField)
        If tankListChkMessage <> C_MESSAGENO.NORMAL Then
            Me.txtRightErrorMessage.Text = rightMessage
            CommonFunctions.ShowMessage(tankListChkMessage, Me.lblFooterMessage, pageObject:=Me)
            Return False
        End If
        '******************************
        '各タンクの日付前後チェック
        '******************************
        Dim tankDateChkMessage As New Text.StringBuilder
        Dim dummyDateErrorMessage As New Label
        Dim hasErrorRow As Boolean = False
        CommonFunctions.ShowMessage(C_MESSAGENO.VALIDITYINPUT, dummyDateErrorMessage)
        For Each drTankItem As DataRow In workDs.Tables(CONST_DT_NAME_TANKINFO).Rows
            Dim stDate As String = Convert.ToString(drTankItem("LEASESTYMD"))
            Dim endDateScr As String = Convert.ToString(drTankItem("LEASEENDYMDSCR"))
            Dim endDate As String = Convert.ToString(drTankItem("LEASEENDYMD"))
            hasErrorRow = False
            If stDate = "" Then
                Continue For
            End If
            Dim stDateDtm As Date = Date.Parse(stDate)
            If endDateScr <> "" Then
                '単項目チェックを行っているので日付型に変換できるのが前提
                Dim endDateScrDtm As Date = Date.Parse(endDateScr)
                If stDateDtm > endDateScrDtm Then
                    hasErrorRow = True
                    tankDateChkMessage.AppendFormat("・Start to End Date(Sche)：{0}", dummyDateErrorMessage.Text).AppendLine()
                End If
            End If
            If endDate <> "" Then
                '単項目チェックを行っているので日付型に変換できるのが前提
                Dim endDateDtm As Date = Date.Parse(endDate)
                If stDateDtm > endDateDtm Then
                    hasErrorRow = True
                    tankDateChkMessage.AppendFormat("・Start to End Date：{0}", dummyDateErrorMessage.Text).AppendLine()
                End If
            End If
            If hasErrorRow = True Then
                tankDateChkMessage.AppendFormat("--> {0} = {1}", padRight("Tank No", 20), Convert.ToString(drTankItem.Item("TANKNO"))).AppendLine()
            End If
        Next
        If tankDateChkMessage.Length > 0 Then
            Me.txtRightErrorMessage.Text = tankDateChkMessage.ToString
            CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage, pageObject:=Me)
            Return False
        End If
        '******************************
        'リスト存在チェック
        '******************************
        Dim listCheck As New List(Of TextBox) From {Me.txtLeaseTerm, Me.txtLeaseType, Me.txtProduct, Me.txtAutoExtendKind,
                                                    Me.txtCurrency, Me.txtPaymentMonth, Me.txtAutoExtend,
                                                    Me.txtLeasePaymentKind, Me.txtTax}
        For Each chkObj In listCheck
            '空白ならスキップ
            If chkObj.Text = "" Then
                Continue For
            End If
            Dim dicListItem As Dictionary(Of String, String) = New Dictionary(Of String, String)
            Select Case chkObj.ID
                Case "txtLeaseTerm"
                    dicListItem = (From listItem In lbLeaseTerm.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtLeaseType"
                    dicListItem = (From listItem In lbLeaseType.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtProduct"
                    Dim dtDdlValue As DataTable = Nothing
                    dtDdlValue = GetProduct()
                    dicListItem = (From item In dtDdlValue Select key = Convert.ToString(item("CODE")), val = Convert.ToString(item("NAME"))).ToDictionary(Function(dv) dv.key, Function(dv) dv.val)
                Case "txtPaymentMonth"
                    dicListItem = (From listItem In lbPaymentMonth.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtAutoExtend"
                    dicListItem = (From listItem In lbYesNo.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtAutoExtendKind"
                    dicListItem = (From listItem In lbAutoExtendKind.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtLeasePaymentKind"
                    dicListItem = (From listItem In lbLeasePaymentKind.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtTax"
                    dicListItem = (From listItem In lbTax.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
                Case "txtCurrency"
                    dicListItem = (From listItem In lbLeaseCurrency.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)

            End Select
            If Not dicListItem.ContainsKey(chkObj.Text) Then
                CommonFunctions.ShowMessage(C_MESSAGENO.UNSELECTABLEERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                        messageParams:=New List(Of String) From {String.Format("VALUE:{0}", chkObj.Text)})
                chkObj.Focus()
                Return False
            End If
        Next chkObj
        '****************************************
        '他ユーザー更新チェック
        '当画面遷移直後の情報と現在のDBの状態比較
        '****************************************
        '修正時
        If Me.GBT00020RValues.AddAgreement = False Then
            Dim dtLatestAgreement As DataTable = CreateAgreementTable()
            Dim dtLatestTankInfo As DataTable = CreateTankInfoTable()
            dtLatestAgreement = GetAgreement(dtLatestAgreement, Me.GBT00020RValues.ContractNo, Me.GBT00020RValues.AgreementNo)
            dtLatestTankInfo = GetTankListInfo(dtLatestTankInfo, Me.GBT00020RValues.ContractNo, Me.GBT00020RValues.AgreementNo)
            '協定書の更新日比較
            If dtLatestAgreement IsNot Nothing AndAlso dtLatestAgreement.Rows.Count = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage, pageObject:=Me)
                Return False
            End If
            Dim drDispAgreement = prevDs.Tables(CONST_DT_NAME_AGREEMENT).Rows(0)
            Dim drLatestAgreement = dtLatestAgreement.Rows(0)
            If Not (Convert.ToString(drDispAgreement.Item("UPDYMD")).TrimEnd = Convert.ToString(drLatestAgreement("UPDYMD")).TrimEnd _
               AndAlso Convert.ToString(drDispAgreement.Item("UPDUSER")).TrimEnd = Convert.ToString(drLatestAgreement("UPDUSER")).TrimEnd _
               AndAlso Convert.ToString(drDispAgreement.Item("UPDTERMID")).TrimEnd = Convert.ToString(drLatestAgreement("UPDTERMID")).TrimEnd) Then
                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage, pageObject:=Me)
                Return False
            End If
            'タンク情報の比較
            If Not (dtLatestTankInfo IsNot Nothing AndAlso prevDs.Tables(CONST_DT_NAME_TANKINFO) IsNot Nothing AndAlso
                   dtLatestTankInfo.Rows.Count = prevDs.Tables(CONST_DT_NAME_TANKINFO).Rows.Count) Then
                '更新前後で数が違っていたら他者に更新されている
                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage, pageObject:=Me)
                Return False
            End If
            For Each drDispTankInfo As DataRow In prevDs.Tables(CONST_DT_NAME_TANKINFO).Rows
                Dim tankNo As String = Convert.ToString(drDispTankInfo.Item("TANKNO"))
                Dim qLatestTank = From drLatestTank In dtLatestTankInfo Where drLatestTank("TANKNO").Equals(tankNo) _
                                                                        AndAlso drLatestTank("UPDYMD").Equals(drDispTankInfo("UPDYMD")) _
                                                                        AndAlso drLatestTank("UPDUSER").Equals(drDispTankInfo("UPDUSER")) _
                                                                        AndAlso drLatestTank("UPDTERMID").Equals(drDispTankInfo("UPDTERMID"))

                If qLatestTank.Any = False Then
                    '更新前と直近のDB情報のタンクNoにつき同一の更新日、ユーザー、端末ではない場合
                    '他者更新あり
                    CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage, pageObject:=Me)
                    Return False
                End If
            Next
        End If
        Return True
    End Function
    ''' <summary>
    ''' リースオーダー作成イベント
    ''' </summary>
    Public Sub btnCreateLeaseOrder_Click()
        Dim ds As DataSet = Me.DsDisDisplayValues
        ds = CommonFunctions.DeepCopy(ds)
        '契約情報取得
        Dim dtContract As DataTable = CreateContractDt()
        dtContract = GetContractItem(dtContract, Me.GBT00020RValues.ContractNo)
        If ds.Tables.Contains(CONST_DT_NAME_CONTRACT) Then
            ds.Tables.Remove(CONST_DT_NAME_CONTRACT)
        End If
        ds.Tables.Add(dtContract)
        'タンク情報
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        Dim qToOrderTank = (From item In dtTankInfo Where Convert.ToString(item("TOORDER")) = "1" _
                                                      AndAlso Convert.ToString(item("LEASESTYMD")) <> "" _
                                                      AndAlso Convert.ToString(item("LEASEENDYMDSCR")) <> "")
        If qToOrderTank.Any = False Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        Dim toOrderTank As DataTable = qToOrderTank.CopyToDataTable
        toOrderTank.TableName = CONST_DT_NAME_TANKINFO_TO_ORDER
        ds.Tables.Add(toOrderTank)
        '画面上部作成起点ドロップダウンよりいつからのデータを作るか取得
        Dim selectedVal As Integer = CInt(Me.ddlOrderStart.SelectedValue)
        'これ以上の年月をOrder投入対象とする
        Dim targetDate As Date = New Date(Now.Year, Now.Month, 1)
        targetDate = targetDate.AddMonths(selectedVal * -1)
        EntryLeaseOrder(ds, targetDate)
        Dim drAgreement As DataRow = ds.Tables(CONST_DT_NAME_AGREEMENT).Rows(0)
        Me.PrevMessageNo = C_MESSAGENO.NORMALDBENTRY
        Me.PrevAgreementNo = Convert.ToString(drAgreement.Item("AGREEMENTNO"))
        Server.Transfer(Request.Url.LocalPath) '自身を再ロード
    End Sub
    ''' <summary>
    ''' オーダー料生成
    ''' </summary>
    ''' <param name="ds"></param>
    ''' <param name="targetDate">オーダー投入開始日</param>
    Private Sub EntryLeaseOrder(ds As DataSet, targetDate As Date)
        '協定書情報
        Dim dtAgreement As DataTable = ds.Tables(CONST_DT_NAME_AGREEMENT)
        Dim drAgreement As DataRow = dtAgreement.Rows(0)
        'タンク情報
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO_TO_ORDER)
        Dim isFirstOrder As Boolean = False
        Dim relatedTankList As New Dictionary(Of String, String)
        Dim orderNo As String = ""
        Dim contractNo As String = Convert.ToString(drAgreement.Item("CONTRACTNO"))
        Dim agreementNo As String = Convert.ToString(drAgreement.Item("AGREEMENTNO"))
        '初回連動していない場合は協定書データのオーダーNoは空白
        If Convert.ToString(drAgreement.Item("RELATEDORDERNO")) = "" Then
            isFirstOrder = True
            orderNo = GetOrderNo() '新規作成の場合はオーダーNoを生成
        Else
            orderNo = Convert.ToString(drAgreement.Item("RELATEDORDERNO"))
            relatedTankList = GetRelatedTanks(orderNo)
        End If
        'オーダー展開すみ含む総タンク数
        Dim tankCnt As Integer = 0
        Dim dicTankNo As New Dictionary(Of String, String)(relatedTankList)
        For Each item As DataRow In dtTankInfo.Rows
            If Not dicTankNo.ContainsKey(Convert.ToString(item("TANKNO"))) Then
                Dim maxSeq As Integer = 1
                Dim maxSeqStr = (From keyval In dicTankNo Order By keyval.Value Descending Select keyval.Value).FirstOrDefault
                If maxSeqStr IsNot Nothing AndAlso IsNumeric(maxSeqStr) Then
                    maxSeq = CInt(maxSeqStr) + 1
                End If
                dicTankNo.Add(Convert.ToString(item("TANKNO")), maxSeq.ToString("000"))
            End If
            item("TANKSEQ") = dicTankNo(Convert.ToString(item("TANKNO")))
        Next
        tankCnt = dicTankNo.Count
        'OderValue登録対象データ生成
        Dim dtOrderValue = CreateOrderValueData(orderNo, ds, targetDate)
        '登録処理実施
        Try
            Dim procDate As Date = Now
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                Using sqlTran = sqlCon.BeginTransaction

                    InsertOrderBase(orderNo, contractNo, agreementNo, tankCnt.ToString, sqlCon, sqlTran, procDate)
                    InsertOrderValue(dtOrderValue, dicTankNo, orderNo, targetDate, sqlCon, sqlTran, procDate)
                    InsertOrderValue2(orderNo, dicTankNo, sqlCon, sqlTran, procDate)
                    UpdateAgreementOrderRelate(orderNo, drAgreement, sqlCon, sqlTran, procDate)
                    sqlTran.Commit()
                End Using
            End Using

        Catch ex As Exception
            Throw
        End Try
    End Sub
    ''' <summary>
    ''' 画面タンク情報・契約書・協定書を元にOrderValueデータを生成
    ''' </summary>
    ''' <param name="ds"></param>
    ''' <returns></returns>
    Private Function CreateOrderValueData(orderNo As String, ds As DataSet, targetDate As Date) As DataTable
        Dim retDt As New DataTable
        Dim orderValField As New List(Of String) From {
                    "ORDERNO", "TANKSEQ", "DTLPOLPOD", "DTLOFFICE", "TANKNO",
                    "COSTCODE", "COUNTRYCODE", "CURRENCYCODE", "TAXATION",
                    "AMOUNT", "SCHEDELDATE", "INVOICEDBY", "AGENTORGANIZER",
                    "LOCALBR", "LOCALRATE", "BRID", "BRCOST", "SHIPPER"}
        For Each colName As String In orderValField
            retDt.Columns.Add(colName, GetType(String))
            retDt.Columns(colName).DefaultValue = ""
        Next

        '契約書情報
        Dim dtContract As DataTable = ds.Tables(CONST_DT_NAME_CONTRACT)
        Dim drContract As DataRow = dtContract.Rows(0)
        '協定書情報
        Dim dtAgreement As DataTable = ds.Tables(CONST_DT_NAME_AGREEMENT)
        Dim drAgreement As DataRow = dtAgreement.Rows(0)
        '契約書の国を基づくオーダー作成時のExRateを取得
        Dim exRate As String = "0"
        Dim GBA00010ExRate As New GBA00010ExRate With {.COUNTRYCODE = Convert.ToString(drContract("COUNRTYORGANIZER")),
                                                       .TARGETYM = Now.ToString("yyyy/MM")}
        GBA00010ExRate.getExRateInfo()
        exRate = GBA00010ExRate.EXRATEFIRSTROW
        '売上費用コードをリースタイプより選別
        Dim costListBox As New ListBox
        Dim COA0017FixValue As New COA0017FixValue
        COA0017FixValue.CLAS = "LEASEPAYMENT"
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.LISTBOX3 = costListBox
        COA0017FixValue.COA0017getListFixValue()
        Dim dicLeasePayment = COA0017FixValue.VALUEDIC
        Dim costCode As String = ""
        If dicLeasePayment.ContainsKey(Convert.ToString(drAgreement("LEASETYPE"))) Then
            costCode = dicLeasePayment(Convert.ToString(drAgreement("LEASETYPE")))(3)
        End If
        'タンク情報ループ
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO_TO_ORDER)

        For Each drTankInfo As DataRow In dtTankInfo.Rows

            Dim dicLeaseAmount = CalcAmountToOrder(drTankInfo, drContract, drAgreement)
            For Each dayAmount As KeyValuePair(Of String, String) In dicLeaseAmount
                'リース種類により売上費用コードを動的に変更
                Dim writeDr As DataRow = retDt.NewRow
                writeDr.Item("ORDERNO") = orderNo
                writeDr.Item("TANKSEQ") = drTankInfo.Item("TANKSEQ")
                writeDr.Item("DTLPOLPOD") = "POL1"
                writeDr.Item("DTLOFFICE") = drContract("ORGANIZER")
                writeDr.Item("TANKNO") = drTankInfo("TANKNO")
                writeDr.Item("COSTCODE") = costCode
                writeDr.Item("COUNTRYCODE") = drContract("COUNRTYORGANIZER")
                writeDr.Item("CURRENCYCODE") = drAgreement("CURRENCY")
                writeDr.Item("TAXATION") = If(Convert.ToString(drContract("TAXKIND")) = "TX", "1", "0")
                writeDr.Item("AMOUNT") = dayAmount.Value
                writeDr.Item("SCHEDELDATE") = dayAmount.Key
                writeDr.Item("INVOICEDBY") = drContract("ORGANIZER")
                writeDr.Item("AGENTORGANIZER") = drContract("ORGANIZER")
                If drAgreement.Item("CURRENCY").Equals("JPY") AndAlso drContract("COUNRTYORGANIZER").Equals("JP") Then
                    writeDr.Item("LOCALBR") = dayAmount.Value
                Else
                    writeDr.Item("LOCALBR") = "0"
                End If
                writeDr.Item("LOCALRATE") = exRate
                writeDr.Item("BRID") = drAgreement("AGREEMENTNO")
                writeDr.Item("BRCOST") = "1"
                writeDr.Item("SHIPPER") = drContract("SHIPPER")
                If targetDate.ToString("yyyy/MM/dd") <= dayAmount.Key Then
                    '対象行以外はオーダーレコードを生成しない
                    retDt.Rows.Add(writeDr)
                End If

            Next '生成した年月、売上のループ
        Next 'タンクループ
        Return retDt
    End Function
    ''' <summary>
    ''' リース売上計算（延長ではない）
    ''' </summary>
    ''' <param name="drTankInfo">タンク1行分</param>
    ''' <param name="drContract">契約情報</param>
    ''' <param name="drAgreement">協定情報</param>
    ''' <returns></returns>
    Private Function CalcAmountToOrder(drTankInfo As DataRow,
                                       drContract As DataRow, drAgreement As DataRow) As Dictionary(Of String, String)
        Dim startDate As String = Convert.ToString(drTankInfo("LEASESTYMD"))
        Dim endDate As String = Convert.ToString(drTankInfo("LEASEENDYMDSCR"))
        If Convert.ToString(drTankInfo("LEASEENDYMD")) <> "" Then
            endDate = Convert.ToString(drTankInfo("LEASEENDYMD"))
        End If

        Dim startDateDtm As Date
        Dim endDateDtm As Date
        Dim retVal As New Dictionary(Of String, String)
        If Date.TryParse(startDate, startDateDtm) = False OrElse
           Date.TryParse(endDate, endDateDtm) = False OrElse
           startDateDtm > endDateDtm Then
            Return Nothing
        End If
        '開始日日割りチェックがついている場合
        If Not drTankInfo("PAYSTDAILY").Equals("1") Then
            startDateDtm = FirstDayOfMonth(startDateDtm)
        End If
        '終了日日割りチェックがついている場合
        If Not drTankInfo("PAYENDDAILY").Equals("1") Then
            endDateDtm = LastDayOfMonth(endDateDtm)
        End If

        Dim amount As Decimal = 0
        Dim amountSummary As Decimal = 0

        Decimal.TryParse(Convert.ToString(drAgreement("LEASEPAYMENTS")), amount)

        Dim loopStartDtm As Date = New Date(startDateDtm.Year, startDateDtm.Month, 1)
        Dim loopCurrentDtm As Date = loopStartDtm
        Dim loopCurrentAmount As Decimal = 0
        Dim isSplitDays As Boolean = False
        Dim dateSpan As Long = 0
        Do Until endDateDtm < loopCurrentDtm
            loopCurrentAmount = 0
            isSplitDays = False
            If startDateDtm.ToString("yyyyMM") = endDateDtm.ToString("yyyyMM") AndAlso
               (startDateDtm.Day <> 1 OrElse endDateDtm <> LastDayOfMonth(endDateDtm)) Then
                '単月のみで月初日開始日が月初日ではないまたは終了日が月末日ではない場合
                isSplitDays = True
                dateSpan = DateDiff("D", startDateDtm, endDateDtm) + 1
            ElseIf loopStartDtm = loopCurrentDtm AndAlso loopStartDtm <> startDateDtm Then
                '初月の日割り
                isSplitDays = True
                dateSpan = DateDiff("D", startDateDtm, LastDayOfMonth(startDateDtm)) + 1
            ElseIf endDateDtm.ToString("yyyyMM") = loopCurrentDtm.ToString("yyyyMM") AndAlso
                   endDateDtm <> LastDayOfMonth(endDateDtm) Then
                '最終月と不一致の日割り
                isSplitDays = True
                dateSpan = DateDiff("D", loopCurrentDtm, endDateDtm) + 1
            ElseIf drContract("LEASEPAYMENTKIND").Equals("PR") Then
                '通常の日割り
                isSplitDays = True
                dateSpan = DateDiff("D", loopCurrentDtm, LastDayOfMonth(loopCurrentDtm)) + 1
            End If

            If isSplitDays AndAlso drContract("LEASEPAYMENTKIND").Equals("MC") Then
                '月単位の金額で日割り計算が必要な場合
                Dim monthTotalDays = DateDiff("D", loopCurrentDtm, LastDayOfMonth(loopCurrentDtm)) + 1
                'Dim amountPerDay As Decimal = amount / monthTotalDays
                Dim amountPerDay As Decimal = Math.Round(amount * 12 / 365, 0)
                Dim decimals As Integer = 0
                If drAgreement.Item("CURRENCY").Equals("USD") Then
                    decimals = 2
                End If
                loopCurrentAmount = amountPerDay * dateSpan
                loopCurrentAmount = Math.Round(loopCurrentAmount, decimals)
            ElseIf isSplitDays Then
                '通常の日割り計算
                loopCurrentAmount = amount * dateSpan
            Else
                'その他
                loopCurrentAmount = amount
            End If

            If drContract("LEASEPAYMENTTYPE").Equals("MT") Then
                '毎月払いの場合は月毎のレコードを生成
                retVal.Add(FirstDayOfMonth(loopCurrentDtm).ToString("yyyy/MM/dd"), Convert.ToString(loopCurrentAmount))
            End If
            '開始、終了払い用の為金額を合計していく
            amountSummary = amountSummary + loopCurrentAmount
            loopCurrentDtm = loopCurrentDtm.AddMonths(1)
        Loop
        '開始日・終了日払いの場合は期間合算値
        If drContract("LEASEPAYMENTTYPE").Equals("AS") Then
            '開始払い
            retVal.Add(FirstDayOfMonth(startDateDtm).ToString("yyyy/MM/dd"), Convert.ToString(amountSummary))
        ElseIf drContract("LEASEPAYMENTTYPE").Equals("AE") Then
            '終了払い
            retVal.Add(FirstDayOfMonth(endDateDtm).ToString("yyyy/MM/dd"), Convert.ToString(amountSummary))
        End If
        Return retVal
    End Function
    ''' <summary>
    ''' 月末日算出
    ''' </summary>
    ''' <param name="sourceDate"></param>
    ''' <returns></returns>
    Private Function LastDayOfMonth(ByVal sourceDate As Date) As Date
        Dim lastDay As DateTime = New DateTime(sourceDate.Year, sourceDate.Month, 1)
        Return lastDay.AddMonths(1).AddDays(-1)
    End Function
    ''' <summary>
    ''' 月初日算出
    ''' </summary>
    ''' <param name="sourceDate"></param>
    ''' <returns></returns>
    Private Function FirstDayOfMonth(ByVal sourceDate As Date) As Date
        Dim firstDay As Date = New Date(sourceDate.Year, sourceDate.Month, 1)
        Return firstDay
    End Function
    ''' <summary>
    ''' オーダー基本情報追加
    ''' </summary>
    ''' <param name="contractNo"></param>
    ''' <param name="agreementNo"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="procDate"></param>
    Private Sub InsertOrderBase(orderNo As String, contractNo As String, agreementNo As String, tankCnt As String, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If procDate = Date.Parse("1900/01/01") Then
            procDate = Date.Now
        End If
        '文言フィールド（開発中のためいったん固定
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If

        Dim COA0035Convert As New BASEDLL.COA0035Convert
        Dim cnvStr As String = Nothing
        COA0035Convert.I_CONVERT = tankCnt
        COA0035Convert.I_CLASS = "CONVERT"
        COA0035Convert.COA0035convNumToEng()
        If COA0035Convert.O_ERR = C_MESSAGENO.NORMAL Then
            cnvStr = COA0035Convert.O_CONVERT1
        Else
            Throw New Exception("Fix value getError")
        End If

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("UPDATE GBT0004_ODR_BASE ")
        sqlStat.AppendLine("   SET DELFLG     = @DELFLG")
        sqlStat.AppendLine("      ,UPDYMD     = @ENTDATE")
        sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO")
        sqlStat.AppendLine("   AND DELFLG <> @DELFLG;")

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
        sqlStat.AppendLine("      ,AGR.AGREEMENTNO")
        sqlStat.AppendLine("      ,@BRTYPE")
        sqlStat.AppendLine("      ,@VALIDITYFROM")
        sqlStat.AppendLine("      ,@VALIDITYTO")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,@NOOFTANKS")
        sqlStat.AppendLine("      ,CNT.SHIPPER")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,AGR.PRODUCTCODE")
        sqlStat.AppendLine("      ,0")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,'1900/01/01'") 'ETD1
        sqlStat.AppendLine("      ,'1900/01/01'") 'ETA1
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,'1900/01/01'") 'ETD2
        sqlStat.AppendLine("      ,'1900/01/01'") 'ETA2
        sqlStat.AppendLine("      ,CNT.ORGANIZER")
        sqlStat.AppendLine("      ,0") 'LOADING
        sqlStat.AppendLine("      ,0") 'STEAMING
        sqlStat.AppendLine("      ,0") 'TIP
        sqlStat.AppendLine("      ,0") 'EXTRA
        sqlStat.AppendLine("      ,0") 'DEMURTO
        sqlStat.AppendLine("      ,0") 'DEMURUSRATE1
        sqlStat.AppendLine("      ,0") 'DEMURUSRATE2
        sqlStat.AppendLine("      ,CNT.INITUSER") 'SALESTPIC
        sqlStat.AppendLine("      ,CNT.ORGANIZER") 'AGENTORGANIZER
        sqlStat.AppendLine("      ,CNT.ORGANIZER")  'AGENTPOL1
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")
        sqlStat.AppendLine("      ,''")

        sqlStat.AppendFormat("      ,ISNULL(SP.{0} + CHAR(13) + CHAR(10) + SP.ADDR,'') AS SHIPPERTEXT ", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT")
        sqlStat.AppendLine("      ,'' AS NOTIFYTEXT")
        sqlStat.AppendLine("      ,'' AS NOTIFYCONTTEXT1")
        sqlStat.AppendLine("      ,'' AS NOTIFYCONTTEXT2")

        sqlStat.AppendLine("      ,ISNULL(SP.CITY,'')         AS PREPAIDAT")
        sqlStat.AppendLine("      ,ISNULL(ER.EXRATE,'')       AS EXCHANGERATE")
        sqlStat.AppendLine("      ,ISNULL(ER.CURRENCYCODE,'') AS LOCALCURRENCY")
        sqlStat.AppendLine("      ,'' AS PAYABLEAT")

        sqlStat.AppendLine("      ,''  AS FREIGHTANDCHARGES")
        sqlStat.AppendLine("      ,''  AS GOODSPKGS")
        sqlStat.AppendLine("      ,@CONTAINERPKGS")
        sqlStat.AppendLine("      ,@NOOFPACKAGE")

        sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'") '削除フラグ(0固定)
        sqlStat.AppendLine("      ,@ENTDATE") 'INITYMD
        sqlStat.AppendLine("      ,@UPDUSER") 'INITUSER
        sqlStat.AppendLine("      ,@ENTDATE")
        sqlStat.AppendLine("      ,@UPDUSER")
        sqlStat.AppendLine("      ,@UPDTERMID")
        sqlStat.AppendLine("      ,@RECEIVEYMD")
        sqlStat.AppendFormat("  FROM {0} CNT", CONST_TBL_CONTRACT).AppendLine()
        sqlStat.AppendFormat("  LEFT JOIN {0} AGR", CONST_TBL_AGREEMENT).AppendLine()
        sqlStat.AppendLine("    ON AGR.CONTRACTNO     = @CONTRACTNO")
        sqlStat.AppendLine("   AND AGR.CONTRACTNO     = CNT.CONTRACTNO")
        sqlStat.AppendLine("   AND AGR.AGREEMENTNO    = @AGREEMENTNO")
        sqlStat.AppendLine("   AND AGR.DELFLG        <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'Shipper
        sqlStat.AppendLine("    ON SP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND SP.CUSTOMERCODE = CNT.SHIPPER")
        sqlStat.AppendLine("   AND SP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND SP.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND SP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'Product
        sqlStat.AppendLine("    ON PD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND PD.PRODUCTCODE  = AGR.PRODUCTCODE")
        sqlStat.AppendLine("   AND PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND PD.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND PD.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") 'FIXVAL
        sqlStat.AppendLine("    ON FV1.CLASS       = 'DESCGOODS'")
        sqlStat.AppendLine("   AND FV1.DELFLG     <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE ER") 'ExRate
        sqlStat.AppendLine("    ON ER.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND ER.COUNTRYCODE  = SP.COUNTRYCODE")
        sqlStat.AppendLine("   AND ER.TARGETYM     = (SELECT FORMAT(CONVERT(DATETIME,(LEFT(CONVERT(VARCHAR, GETDATE(), 112), 6)+'01')),'yyyy/MM/dd'))")
        sqlStat.AppendLine("   AND ER.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR3") 'COUNTRY
        sqlStat.AppendLine("    ON TR3.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TR3.CARRIERCODE  = CNT.ORGANIZER")
        sqlStat.AppendLine("   AND TR3.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TR3.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TR3.DELFLG      <> @DELFLG")

        sqlStat.AppendLine(" WHERE CNT.CONTRACTNO    = @CONTRACTNO")
        sqlStat.AppendLine("   AND CNT.DELFLG <> @DELFLG;")

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
                    .Add("@BRTYPE", SqlDbType.NVarChar).Value = C_BRTYPE.LEASE
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = contractNo
                    .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = agreementNo

                    .Add("@VALIDITYFROM", SqlDbType.Date).Value = procDate
                    .Add("@VALIDITYTO", SqlDbType.Date).Value = procDate

                    .Add("@NOOFTANKS", SqlDbType.Int).Value = tankCnt

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@ENTDATE", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = COA0019Session.APSRVCamp
                    .Add("@DAYSTEXT", SqlDbType.NVarChar).Value = "DAYS DETENTION FREE AT DESTINATION"
                    .Add("@CONTAINERPKGS", SqlDbType.NVarChar).Value = cnvStr & "(" & tankCnt & ")" & " TANK CONTAINER(S) ONLY"
                    .Add("@NOOFPACKAGE", SqlDbType.NVarChar).Value = Convert.ToString(tankCnt)
                    .Add("@SALESPIC", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    '.Add("@INVOICEDBY", SqlDbType.NVarChar).Value = GBA00003UserSetting.OFFICECODE
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
    ''' オーダー基本情報追加
    ''' </summary>
    ''' <param name="dtOrderValue"></param>
    ''' <param name="dicTankNo"></param>
    ''' <param name="targetDate">取り込み対象タンクNo</param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="procDate"></param>
    Private Sub InsertOrderValue(dtOrderValue As DataTable, dicTankNo As Dictionary(Of String, String), orderNo As String, targetDate As Date, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If procDate = Date.Parse("1900/01/01") Then
            procDate = Date.Now
        End If

        Dim sqlStatRemove As New StringBuilder
        sqlStatRemove.AppendLine("UPDATE GBT0005_ODR_VALUE ")
        sqlStatRemove.AppendLine("   SET DELFLG     = @DELFLG_YES")
        sqlStatRemove.AppendLine("      ,UPDYMD     = @UPDYMD")
        sqlStatRemove.AppendLine("      ,UPDUSER    = @UPDUSER")
        sqlStatRemove.AppendLine("      ,UPDTERMID  = @UPDTERMID")
        sqlStatRemove.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD")
        sqlStatRemove.AppendLine(" WHERE ORDERNO = @ORDERNO")
        sqlStatRemove.AppendLine("   AND TANKNO  = @TANKNO")
        sqlStatRemove.AppendLine("   AND SCHEDELDATEBR >= @SCHEDELDATEBR")
        sqlStatRemove.AppendLine("   AND DELFLG <> @DELFLG_YES;")
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
        sqlStat.AppendLine("       ,ACTUALDATE")
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
        sqlStat.AppendLine("       ,@AMOUNTORD")
        sqlStat.AppendLine("       ,@AMOUNTFIX")
        sqlStat.AppendLine("       ,@CONTRACTORBR")
        sqlStat.AppendLine("       ,@CONTRACTORODR")
        sqlStat.AppendLine("       ,@CONTRACTORFIX")
        sqlStat.AppendLine("       ,@SCHEDELDATEBR")
        sqlStat.AppendLine("       ,@SCHEDELDATE")
        sqlStat.AppendLine("       ,@ACTUALDATE")
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
        sqlStat.AppendLine("       ,@INITYMD")
        sqlStat.AppendLine("       ,@INITUSER")
        sqlStat.AppendLine("       ,@UPDYMD")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine(");")


        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            '登録対象にある既登録分論理削除（なくても指定したタンクの先のデータは削除）
            Using sqlCmd As New SqlCommand(sqlStatRemove.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                With sqlCmd.Parameters
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@SCHEDELDATEBR", SqlDbType.Date).Value = targetDate.ToString("yyyy/MM/dd")
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    .Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With
                '再作成対象のタンクNo及び対象日付以降の既登録データ一旦削除
                '動的パラメータはタンクNoのみ
                Dim paramTankno = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar)
                '登録対象のタンクNoをループ
                For Each tankNo As String In dicTankNo.Keys
                    paramTankno.Value = tankNo
                    sqlCmd.ExecuteNonQuery()
                Next tankNo
            End Using

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If

                Dim paramOrderno = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar)
                Dim paramTankseq = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar)
                Dim paramDtlpolpod = sqlCmd.Parameters.Add("@DTLPOLPOD", SqlDbType.NVarChar)
                Dim paramDtloffice = sqlCmd.Parameters.Add("@DTLOFFICE", SqlDbType.NVarChar)
                Dim paramTankno = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar)
                Dim paramCostcode = sqlCmd.Parameters.Add("@COSTCODE", SqlDbType.NVarChar)
                Dim paramActionid = sqlCmd.Parameters.Add("@ACTIONID", SqlDbType.NVarChar)
                Dim paramDispseq = sqlCmd.Parameters.Add("@DISPSEQ", SqlDbType.NVarChar)
                Dim paramLastact = sqlCmd.Parameters.Add("@LASTACT", SqlDbType.NVarChar)
                Dim paramRequiredact = sqlCmd.Parameters.Add("@REQUIREDACT", SqlDbType.NVarChar)
                Dim paramOrigindestination = sqlCmd.Parameters.Add("@ORIGINDESTINATION", SqlDbType.NVarChar)
                Dim paramCountrycode = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar)
                Dim paramCurrencycode = sqlCmd.Parameters.Add("@CURRENCYCODE", SqlDbType.NVarChar)
                Dim paramTaxation = sqlCmd.Parameters.Add("@TAXATION", SqlDbType.NVarChar)
                Dim paramAmountbr = sqlCmd.Parameters.Add("@AMOUNTBR", SqlDbType.Float)
                Dim paramAmountord = sqlCmd.Parameters.Add("@AMOUNTORD", SqlDbType.Float)
                Dim paramAmountfix = sqlCmd.Parameters.Add("@AMOUNTFIX", SqlDbType.Float)
                Dim paramContractorbr = sqlCmd.Parameters.Add("@CONTRACTORBR", SqlDbType.NVarChar)
                Dim paramContractorodr = sqlCmd.Parameters.Add("@CONTRACTORODR", SqlDbType.NVarChar)
                Dim paramContractorfix = sqlCmd.Parameters.Add("@CONTRACTORFIX", SqlDbType.NVarChar)
                Dim paramSchedeldatebr = sqlCmd.Parameters.Add("@SCHEDELDATEBR", SqlDbType.Date)
                Dim paramSchedeldate = sqlCmd.Parameters.Add("@SCHEDELDATE", SqlDbType.Date)
                Dim paramActualdate = sqlCmd.Parameters.Add("@ACTUALDATE", SqlDbType.Date)
                Dim paramLocalbr = sqlCmd.Parameters.Add("@LOCALBR", SqlDbType.Float)
                Dim paramLocalrate = sqlCmd.Parameters.Add("@LOCALRATE", SqlDbType.Float)
                Dim paramTaxbr = sqlCmd.Parameters.Add("@TAXBR", SqlDbType.Float)
                Dim paramInvoicedby = sqlCmd.Parameters.Add("@INVOICEDBY", SqlDbType.NVarChar)
                Dim paramRemark = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar)
                Dim paramBrid = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar)
                Dim paramBrcost = sqlCmd.Parameters.Add("@BRCOST", SqlDbType.NVarChar)
                Dim paramDatefield = sqlCmd.Parameters.Add("@DATEFIELD", SqlDbType.NVarChar)
                Dim paramDateinterval = sqlCmd.Parameters.Add("@DATEINTERVAL", SqlDbType.NVarChar)
                Dim paramBraddedcost = sqlCmd.Parameters.Add("@BRADDEDCOST", SqlDbType.NVarChar)
                Dim paramAgentorganizer = sqlCmd.Parameters.Add("@AGENTORGANIZER", SqlDbType.NVarChar)
                Dim paramDelflg = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar)
                Dim paramInitymd = sqlCmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)
                Dim paramInituser = sqlCmd.Parameters.Add("@INITUSER", SqlDbType.NVarChar)
                Dim paramUpdymd = sqlCmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)
                Dim paramUpduser = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar)
                Dim paramUpdtermid = sqlCmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar)
                Dim paramReceiveymd = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)
                sqlCmd.Parameters.Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                For Each drOrderValue As DataRow In dtOrderValue.Rows
                    'SQLパラメータの設定
                    paramOrderno.Value = drOrderValue("ORDERNO")
                    paramTankseq.Value = drOrderValue("TANKSEQ")
                    paramDtlpolpod.Value = drOrderValue("DTLPOLPOD")
                    paramDtloffice.Value = drOrderValue("DTLOFFICE")
                    paramTankno.Value = drOrderValue("TANKNO")
                    paramCostcode.Value = drOrderValue("COSTCODE")
                    paramActionid.Value = ""
                    paramDispseq.Value = ""
                    paramLastact.Value = ""
                    paramRequiredact.Value = ""
                    paramOrigindestination.Value = ""
                    paramCountrycode.Value = drOrderValue("COUNTRYCODE")
                    paramCurrencycode.Value = drOrderValue("CURRENCYCODE")
                    paramTaxation.Value = drOrderValue("TAXATION")
                    paramAmountbr.Value = DecimalStringToDecimal(Convert.ToString(drOrderValue("AMOUNT")))
                    paramAmountord.Value = DecimalStringToDecimal(Convert.ToString(drOrderValue("AMOUNT")))
                    paramAmountfix.Value = DecimalStringToDecimal(Convert.ToString(drOrderValue("AMOUNT")))
                    paramContractorbr.Value = Convert.ToString(drOrderValue("SHIPPER"))
                    paramContractorodr.Value = Convert.ToString(drOrderValue("SHIPPER"))
                    paramContractorfix.Value = Convert.ToString(drOrderValue("SHIPPER"))
                    paramSchedeldatebr.Value = DateStringToDateTime(Convert.ToString(drOrderValue("SCHEDELDATE")))
                    paramSchedeldate.Value = DateStringToDateTime(Convert.ToString(drOrderValue("SCHEDELDATE")))
                    paramActualdate.Value = DateStringToDateTime(Convert.ToString(drOrderValue("SCHEDELDATE")))

                    paramLocalbr.Value = DecimalStringToDecimal(Convert.ToString(drOrderValue("LOCALBR")))
                    paramLocalrate.Value = DecimalStringToDecimal(Convert.ToString(drOrderValue("LOCALRATE")))
                    paramTaxbr.Value = "0"
                    paramInvoicedby.Value = drOrderValue("INVOICEDBY")
                    paramRemark.Value = ""
                    paramBrid.Value = drOrderValue("BRID")
                    paramBrcost.Value = drOrderValue("BRCOST")
                    paramDatefield.Value = ""
                    paramDateinterval.Value = ""
                    paramBraddedcost.Value = ""
                    paramAgentorganizer.Value = drOrderValue("AGENTORGANIZER")
                    paramDelflg.Value = CONST_FLAG_NO
                    paramInitymd.Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    paramInituser.Value = COA0019Session.USERID
                    paramUpdymd.Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    paramUpduser.Value = COA0019Session.USERID
                    paramUpdtermid.Value = HttpContext.Current.Session("APSRVname")
                    paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD

                    sqlCmd.ExecuteNonQuery()
                Next

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
    ''' <param name="dicTankNo">コピー数</param>
    ''' <param name="sqlCon">[In(省略可)]SQL接続オブジェクト</param>
    ''' <param name="tran">[In(省略可)]SQLトランザクションオブジェクト</param>
    Private Sub InsertOrderValue2(orderNo As String, dicTankNo As Dictionary(Of String, String), Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If procDate = Date.Parse("1900/01/01") Then
            procDate = Date.Now
        End If
        'この段階でありえないがコピー数が0の場合は終了
        If dicTankNo Is Nothing AndAlso dicTankNo.Count = 0 Then
            Return
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("UPDATE GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("   SET DELFLG     = @DELFLG_YES")
        sqlStat.AppendLine("      ,UPDYMD     = @ENTDATE")
        sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO")
        sqlStat.AppendLine("   AND TANKSEQ = @TANKSEQ")
        sqlStat.AppendLine("   AND DELFLG  = @DELFLG;")

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
        sqlStat.AppendLine(");")

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
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@TRILATERAL", SqlDbType.NVarChar).Value = "1"
                    .Add("@TANKTYPE", SqlDbType.NVarChar).Value = "20TK"
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@ENTDATE", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    .Add("@NOOFPACKAGE", SqlDbType.Float).Value = dicTankNo.Count
                    .Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With
                '動的パラメータ
                Dim paramTankSeq As SqlParameter = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar)

                'タンク数分ループ(TANKSEQ)の0埋め前
                For i = 1 To dicTankNo.Count
                    Dim tankSeq As String = i.ToString("000")
                    paramTankSeq.Value = dicTankNo.Values(i - 1)
                    sqlCmd.ExecuteNonQuery()
                Next 'End dicTankNo.Count
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
    ''' テーブルより契約書データを取得
    ''' </summary>
    ''' <param name="orderNo">オーダーNo</param>
    ''' <returns>連動済のタンク番号を取得</returns>
    Private Function GetRelatedTanks(orderNo As String) As Dictionary(Of String, String)
        Dim retVal As New Dictionary(Of String, String)
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT DISTINCT TANKNO")
        sqlStat.AppendLine("               ,TANKSEQ")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO")
        sqlStat.AppendLine("   AND STYMD      <= @NOWDATE")
        sqlStat.AppendLine("   AND ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("   AND DELFLG     <> @DELFLG")

        Dim dtDbResult As New DataTable
        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        If dtDbResult IsNot Nothing AndAlso dtDbResult.Rows.Count > 0 Then

            retVal = (From item As DataRow In dtDbResult).ToDictionary(Function(itm) Convert.ToString(itm("TANKNO")),
                                                                       Function(itm) Convert.ToString(itm("TANKSEQ")))
        End If
        Return retVal

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
            sqlStat.AppendLine("            AND KEYCODE = @KEYCODE)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR GBQ0003_ORDER)),4)")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = "SERVERSEQ"
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVname")
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
    ''' タンク引当ボタン押下時処理
    ''' </summary>
    Public Sub btnAddNewTank_Click()
        OpenTankList()
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
                Case Me.vLeftDepot.ID
                    'デポ選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim depotCode As String = ""
                        If Me.lbDepot.SelectedItem IsNot Nothing Then
                            depotCode = Me.lbDepot.SelectedItem.Value
                        End If
                        SetDisplayDepot(targetTextBox, depotCode)
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
                        From {{Me.txtAutoExtendKind.ID, New With {.lbl = Me.lblAutoExtendKindText, .list = Me.lbAutoExtendKind}},
                              {Me.txtCurrency.ID, New With {.lbl = Nothing, .list = Me.lbLeaseCurrency}},
                              {Me.txtLeaseTerm.ID, New With {.lbl = Me.lblLeaseTermText, .list = Me.lbLeaseTerm}},
                              {Me.txtLeaseType.ID, New With {.lbl = Me.lblLeaseTypeText, .list = Me.lbLeaseType}},
                              {Me.txtPaymentMonth.ID, New With {.lbl = Me.lblPaymentMonthText, .list = Me.lbPaymentMonth}},
                              {Me.txtLeasePaymentKind.ID, New With {.lbl = Me.lblLeasePaymentKindText, .list = Me.lbLeasePaymentKind}},
                              {Me.txtAutoExtend.ID, New With {.lbl = Me.lblAutoExtendText, .list = Me.lbYesNo}},
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
    ''' タンク入力ボックスOKボタン押下時イベント
    ''' </summary>
    Public Sub btnTankInputOk_Click()

        Dim ds As DataSet = DirectCast(ViewState(CONST_VS_NAME_CURRENT_VAL), DataSet)
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        Dim selectedRow As DataRow = (From item In dtTankInfo Where Convert.ToString(item("LINECNT")) = hdnPopUpLineCnt.Value).FirstOrDefault
        If selectedRow Is Nothing Then
            Return
        End If
        Me.hdnPopUpLineCnt.Value = ""

        selectedRow.Item("LEASESTYMD") = FormatDateYMD(Me.txtStartDate.Text, GBA00003UserSetting.DATEFORMAT)
        selectedRow.Item("LEASEENDYMDSCR") = FormatDateYMD(Me.txtEndDateSche.Text, GBA00003UserSetting.DATEFORMAT)
        selectedRow.Item("LEASEENDYMD") = FormatDateYMD(Me.txtEndDate.Text, GBA00003UserSetting.DATEFORMAT)
        selectedRow.Item("REMARK") = Me.txtTankRemarks.Text
        Dim dicChkItem As New Dictionary(Of String, CheckBox) From {{"CANCELFLG", Me.chkCancel},
                                                                    {"PAYSTDAILY", Me.chkPayStDaily},
                                                                    {"PAYENDDAILY", Me.chkPayEndDaily}}
        For Each chkitem In dicChkItem
            If chkitem.Value.Checked Then
                selectedRow.Item(chkitem.Key) = "1"
            Else
                selectedRow.Item(chkitem.Key) = ""
            End If
        Next chkitem

        selectedRow.Item("DEPOTIN") = Me.txtDepoIn.Text
        selectedRow.Item("DEPOTINNAME") = Me.lblDepoInText.Text

        Me.hdnTankInputAreaDisplay.Value = "none"
        ViewState(CONST_VS_NAME_CURRENT_VAL) = ds
        Me.repTankInfo.DataSource = dtTankInfo
        Me.repTankInfo.DataBind()
    End Sub
    ''' <summary>
    ''' 備考入力ボックスのOKボタン押下時イベント
    ''' </summary>
    Public Sub btnRemarkInputOk_Click()
        Dim targetControl As Label = DirectCast(Me.FindControl(Me.hdnRemarkboxField.Value), Label)
        targetControl.Text = HttpUtility.HtmlEncode(Me.txtRemarkInput.Text)
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
    ''' リスト行ダブルクリック時イベント
    ''' </summary>
    Private Sub ListRowDbClick()
        Dim rowIdString As String = Me.hdnListDBclick.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(rowIdString, rowId) = False Then
            Return
        End If
        Dim ds As DataSet = DirectCast(ViewState(CONST_VS_NAME_CURRENT_VAL), DataSet)
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        Dim selectedRow As DataRow = (From item In dtTankInfo Where Convert.ToString(item("LINECNT")) = rowIdString).FirstOrDefault
        If selectedRow Is Nothing Then
            Return
        End If
        Me.hdnPopUpLineCnt.Value = Convert.ToString(selectedRow.Item("LINECNT"))

        Me.txtTankNo.Text = Convert.ToString(selectedRow.Item("TANKNO"))
        Me.txtDepoOut.Text = Convert.ToString(selectedRow.Item("DEPOTOUT"))
        Me.lblDepoOutText.Text = Convert.ToString(selectedRow.Item("DEPOTOUTNAME"))
        Me.txtStartDate.Text = FormatDateContrySettings(Convert.ToString(selectedRow.Item("LEASESTYMD")), GBA00003UserSetting.DATEFORMAT)
        Me.txtEndDateSche.Text = FormatDateContrySettings(Convert.ToString(selectedRow.Item("LEASEENDYMDSCR")), GBA00003UserSetting.DATEFORMAT)
        Me.txtEndDate.Text = FormatDateContrySettings(Convert.ToString(selectedRow.Item("LEASEENDYMD")), GBA00003UserSetting.DATEFORMAT)
        Me.txtTankRemarks.Text = Convert.ToString(selectedRow.Item("REMARK"))
        Dim dicChkItem As New Dictionary(Of String, CheckBox) From {{"CANCELFLG", Me.chkCancel},
                                                                    {"PAYSTDAILY", Me.chkPayStDaily},
                                                                    {"PAYENDDAILY", Me.chkPayEndDaily}}
        For Each chkitem In dicChkItem
            Dim chkItemVal = Convert.ToString(selectedRow.Item(chkitem.Key))
            If chkItemVal = "1" Then
                chkitem.Value.Checked = True
            Else
                chkitem.Value.Checked = False
            End If
        Next chkitem

        Me.txtDepoIn.Text = Convert.ToString(selectedRow.Item("DEPOTIN"))
        Me.lblDepoInText.Text = Convert.ToString(selectedRow.Item("DEPOTINNAME"))

        Me.hdnTankInputAreaDisplay.Value = "block"

    End Sub
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
    ''' リースターム変更時
    ''' </summary>
    Public Sub txtLeaseTerm_Change()
        Dim leaseTerm As String = Me.txtLeaseTerm.Text.Trim
        Me.txtLeaseTerm.Text = leaseTerm
        Dim findItem As ListItem = Me.lbLeaseTerm.Items.FindByValue(leaseTerm)
        Me.lblLeaseTermText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblLeaseTermText.Text = findItem.Text
        End If
    End Sub
    ''' <summary>
    ''' リース種類変更時イベント
    ''' </summary>
    Public Sub txtLeaseType_Change()
        Dim leaseType As String = Me.txtLeaseType.Text.Trim
        Me.txtLeaseType.Text = leaseType
        Dim findItem As ListItem = Me.lbLeaseType.Items.FindByValue(leaseType)
        Me.lblLeaseTypeText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblLeaseTypeText.Text = findItem.Text
        End If
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
    ''' 支払月変更時
    ''' </summary>
    Public Sub txtPaymentMonth_Change()
        Dim paymentMonth As String = Me.txtPaymentMonth.Text.Trim
        Me.txtPaymentMonth.Text = paymentMonth
        Dim findItem As ListItem = Me.lbPaymentMonth.Items.FindByValue(paymentMonth)
        Me.lblPaymentMonthText.Text = ""
        If findItem IsNot Nothing Then
            Me.lblPaymentMonthText.Text = findItem.Text
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
    ''' 自動延長変更時イベント
    ''' </summary>
    Public Sub txtAutoExtendKind_Change()
        Me.lblAutoExtendKindText.Text = ""
        Dim autoExtendCode As String = txtAutoExtendKind.Text.Trim
        If autoExtendCode = "" OrElse Me.lbAutoExtendKind.Items Is Nothing OrElse Me.lbAutoExtendKind.Items.Count = 0 Then
            Return
        End If
        Dim item As ListItem = Me.lbAutoExtendKind.Items.FindByValue(autoExtendCode)
        If item IsNot Nothing Then
            Me.lblAutoExtendKindText.Text = item.Text
        End If
    End Sub
    ''' <summary>
    ''' 税区分変更時
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
    ''' デポイン変更時イベント
    ''' </summary>
    Public Sub txtDepoIn_Change()
        Dim depoCode As String = Me.txtDepoIn.Text.Trim
        Me.txtDepoIn.Text = depoCode
        Me.lblDepoInText.Text = ""
        If depoCode <> "" Then
            SetDisplayProduct(Me.txtDepoIn, depoCode)
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
        AddLangSetting(dicDisplayText, Me.lblRightInfo1, "ダブルクリックを行い入力を確定してください。", "Double click to confirm input.")
        AddLangSetting(dicDisplayText, Me.lblRightInfo2, "ダブルクリックを行い入力を確定してください。", "Double click to confirm input.")
        '****************************************
        ' 共通情報部分
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblBrInfoHeader, "Lease-Info", "Lease-Info")

        AddLangSetting(dicDisplayText, Me.lblAppDate, "Date", "Date")
        AddLangSetting(dicDisplayText, Me.lblAppAgent, "Agent", "Agent")
        AddLangSetting(dicDisplayText, Me.lblAppPic, "Pic", "Pic")
        AddLangSetting(dicDisplayText, Me.lblAppRemarksH, "Remarks", "Remarks")
        AddLangSetting(dicDisplayText, Me.lblApply, "Apply", "Apply")
        AddLangSetting(dicDisplayText, Me.lblApproved, "Approved", "Approved")

        AddLangSetting(dicDisplayText, Me.lblProductImdg, "危険品等級", "IMDG")
        AddLangSetting(dicDisplayText, Me.lblProductUnNo, "国連番号", "UN No.")

        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品", "Product")
        AddLangSetting(dicDisplayText, Me.lblAutoExtendKind, "自動延長種別", "Auto Extend Kind")
        AddLangSetting(dicDisplayText, Me.lblLeasePayments, "リース料", "Lease Payments")
        AddLangSetting(dicDisplayText, Me.lblReLease, "再リース料", "Re-Lease")
        AddLangSetting(dicDisplayText, Me.lblCurrency, "通貨", "Currency")

        AddLangSetting(dicDisplayText, Me.lblTaxRate, "税率", "Tax Rate")

        AddLangSetting(dicDisplayText, Me.lblRemarks, "備考", "Remarks")


        AddLangSetting(dicDisplayText, Me.btnDownloadFiles, "ファイルダウンロード", "File Download")
        AddLangSetting(dicDisplayText, Me.lblAttachment, "添付", "Attachment")

        AddLangSetting(dicDisplayText, Me.btnAddNewTank, "引当", "Allocate")
        AddLangSetting(dicDisplayText, Me.lblTankList, "タンク一覧", "TankList")

        AddLangSetting(dicDisplayText, Me.lblTankNo, "タンクNo", "Tank No")
        AddLangSetting(dicDisplayText, Me.lblDepoOut, "搬出デポ", "Depo Out")
        AddLangSetting(dicDisplayText, Me.lblPayStDaily, "日割(開始)", "Daily Payment(Start)")
        AddLangSetting(dicDisplayText, Me.lblPayEndDaily, "(終了)", "(End)")

        AddLangSetting(dicDisplayText, Me.lblStartDate, "開始日", "Start Date")
        AddLangSetting(dicDisplayText, Me.lblEndDateSche, "終了日", "End Date(Sche)")

        AddLangSetting(dicDisplayText, Me.lblTankRemarks, "備考", "Remarks")

        AddLangSetting(dicDisplayText, Me.lblCancelDate, "途中解約", "Cancel")

        AddLangSetting(dicDisplayText, Me.lblEndDate, "実終了日", "End Date")

        AddLangSetting(dicDisplayText, Me.lblDepoIn, "返却デポ", "Depo In")

        '一覧ヘッダー
        AddLangSetting(dicDisplayText, Me.hdnListHeaderTank, "オーダー", "Order")
        AddLangSetting(dicDisplayText, Me.hdnListHeaderDelButton, "削除", "Delete")
        AddLangSetting(dicDisplayText, Me.hdnListHeaderTankNo, "タンクNo", "Tank No")
        AddLangSetting(dicDisplayText, Me.hdnListHeaderStatus, "ステータス", "Status")
        AddLangSetting(dicDisplayText, Me.hdnListHeaderDepoOut, "搬出デポ", "Depo Out")

        AddLangSetting(dicDisplayText, Me.hdnListHeaderStartDate, "開始日", "Start Date")
        AddLangSetting(dicDisplayText, Me.hdnListHeaderEndDateScr, "終了日", "End Date(Sche)")

        AddLangSetting(dicDisplayText, Me.hdnListHeaderRemarks, "備考", "Remarks")

        AddLangSetting(dicDisplayText, Me.hdnListHeaderCancel, "途中解約", "Cancel")

        AddLangSetting(dicDisplayText, Me.hdnListHeaderEndDate, "実終了日", "End Date")

        AddLangSetting(dicDisplayText, Me.hdnListHeaderDepoIn, "返却デポ", "Depo In")
        '****************************************
        ' 添付ファイルヘッダー部
        '****************************************
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderText, "添付書類を登録する場合は、ここにドロップすること", "To register attached documents, drop it here")
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderFileName, "ファイル名", "FileName")
        AddLangSetting(dicDisplayText, Me.hdnAttachmentHeaderDelete, "削 除", "Delete")
        '****************************************
        ' 各種ボタン
        '****************************************
        'AddLangSetting(dicDisplayText, Me.btnAddCost, "追加", "Add")
        AddLangSetting(dicDisplayText, Me.lblOrderStart, "オーダー作成起点", "Order Start Point")

        AddLangSetting(dicDisplayText, Me.btnCreateLeaseOrder, "リースオーダー作成", "Create Lease Order")
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnInputRequest, "登録", "Input Request")
        AddLangSetting(dicDisplayText, Me.btnReject, "否認", "Reject")
        AddLangSetting(dicDisplayText, Me.btnOutputExcel, "Excel出力", "Output Excel")
        AddLangSetting(dicDisplayText, Me.btnEntryCost, "費用登録", "Entry Cost")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.btnApply, "申請", "Apply")

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
        Dim prevDs As DataSet = Nothing
        If TypeOf Page.PreviousPage Is GBT00020AGREEMENT Then
            '自身からの遷移(Save時に反応)
            Dim brNo As String = ""
            Dim prevPage As GBT00020AGREEMENT = DirectCast(Page.PreviousPage, GBT00020AGREEMENT)
            Me.GBT00020RValues = prevPage.GBT00020RValues
            GBT00020RValues.AddAgreement = False
            GBT00020RValues.AgreementNo = prevPage.PrevAgreementNo
            ViewState(CONST_VS_NAME_GBT00020SV) = prevPage.GBT00020RValues

            Dim dtAgreement As DataTable = CreateAgreementTable()
            Dim dtTankInfo As DataTable = CreateTankInfoTable()
            Dim dtContract As DataTable = CreateContractDt()
            '前画面のキー情報を元にデータをDBより取得
            dtContract = GetContractItem(dtContract, Me.GBT00020RValues.ContractNo)
            dtAgreement = GetAgreement(dtAgreement, GBT00020RValues.ContractNo, GBT00020RValues.AgreementNo)
            dtTankInfo = GetTankListInfo(dtTankInfo, GBT00020RValues.ContractNo, GBT00020RValues.AgreementNo)
            retDataSet.Tables.AddRange({dtContract, dtAgreement, dtTankInfo})
            CommonFunctions.CleanUserTempDirectory(CONST_MAPID)
            Dim dtAttachment As DataTable = CommonFunctions.GetInitAttachmentFileList(Me.GBT00020RValues.AgreementNo, CONST_DIRNAME_LEASE_AGREEMENT, CONST_MAPID)
            retDataSet.Tables.Add(dtAttachment)
            prevDs = retDataSet
            '保存時に自身をリダイレクト
            If prevPage.PrevMessageNo <> "" Then
                Dim naeiw As String = C_NAEIW.ABNORMAL
                If {C_MESSAGENO.NORMAL, C_MESSAGENO.NORMALDBENTRY, C_MESSAGENO.APPLYSUCCESS}.Contains(prevPage.PrevMessageNo) Then
                    naeiw = C_NAEIW.NORMAL
                End If
                CommonFunctions.ShowMessage(prevPage.PrevMessageNo, Me.lblFooterMessage, naeiw:=naeiw, pageObject:=Me)
            End If
            Me.hdnThisMapVariant.Value = CONST_MAPVARI
            Me.ddlOrderStart.SelectedIndex = prevPage.ddlOrderStart.SelectedIndex
        ElseIf TypeOf Page.PreviousPage Is GBT00020RESULT Then
            '一覧からの遷移
            Dim brNo As String = ""
            Dim prevPage As GBT00020RESULT = DirectCast(Page.PreviousPage, GBT00020RESULT)
            ViewState(CONST_VS_NAME_GBT00020SV) = prevPage.ThisScreenValue
            Me.GBT00020RValues = prevPage.ThisScreenValue
            'TODO Me.hdnThisMapVariant .Value によるさらなる分岐
            Dim dtAgreement As DataTable = CreateAgreementTable()
            Dim dtTankInfo As DataTable = CreateTankInfoTable()
            Dim dtContract As DataTable = CreateContractDt()

            If GBT00020RValues.AddAgreement = True Then
                '新規協定書追加時
                'オーガナイザー情報の生成
                dtAgreement = CreateAgreementTable()
                Dim drAgreement As DataRow = dtAgreement.NewRow
                '契約書の入力内容をデフォルト設定
                Dim contNo As String = ""
                dtContract = GetContractItem(dtContract, GBT00020RValues.ContractNo)
                Dim drContTmp As DataRow = dtContract.Rows(0)
                Dim copyFields As New List(Of String) From {"LEASEPAYMENTTYPE", "LEASEPAYMENTKIND", "AUTOEXTEND", "TAXKIND"}

                For Each copyField In copyFields
                    drAgreement(copyField) = drContTmp(copyField)
                Next
                dtAgreement.Rows.Add(drAgreement)
                dtTankInfo = CreateTankInfoTable()
            Else
                '変更時
                '前画面のキー情報を元にデータをDBより取得
                dtContract = GetContractItem(dtContract, GBT00020RValues.ContractNo)
                dtAgreement = GetAgreement(dtAgreement, GBT00020RValues.ContractNo, GBT00020RValues.AgreementNo)
                dtTankInfo = GetTankListInfo(dtTankInfo, GBT00020RValues.ContractNo, GBT00020RValues.AgreementNo)
            End If
            retDataSet.Tables.AddRange({dtContract, dtAgreement, dtTankInfo})
            CommonFunctions.CleanUserTempDirectory(CONST_MAPID)
            Dim dtAttachment As DataTable = CommonFunctions.GetInitAttachmentFileList(Me.GBT00020RValues.AgreementNo, CONST_DIRNAME_LEASE_AGREEMENT, CONST_MAPID)
            retDataSet.Tables.Add(dtAttachment)
            prevDs = retDataSet
        ElseIf TypeOf Page.PreviousPage Is GBT00024APPROVAL Then
            '↑承認画面変更を
            Dim prevPage As GBT00024APPROVAL = DirectCast(Page.PreviousPage, GBT00024APPROVAL)
            Me.GBT00024AValues = prevPage.ThisScreenValues
            ViewState(CONST_VS_NAME_GBT00024AV) = Me.GBT00024AValues
            '
            Me.GBT00020RValues = New GBT00020RESULT.GBT00020RValues
            Me.GBT00020RValues.AddAgreement = False
            Me.GBT00020RValues.ContractNo = Me.GBT00024AValues.ContractNo
            Me.GBT00020RValues.AgreementNo = Me.GBT00024AValues.AgreementNo
            ViewState(CONST_VS_NAME_GBT00020SV) = Me.GBT00020RValues
            Dim dtContract As DataTable = CreateContractDt()
            Dim dtAgreement As DataTable = CreateAgreementTable()
            Dim dtTankInfo As DataTable = CreateTankInfoTable()
            dtContract = GetContractItem(dtContract, GBT00020RValues.ContractNo)
            dtAgreement = GetAgreement(dtAgreement, GBT00020RValues.ContractNo, GBT00020RValues.AgreementNo)
            dtTankInfo = GetTankListInfo(dtTankInfo, GBT00020RValues.ContractNo, GBT00020RValues.AgreementNo)
            retDataSet.Tables.AddRange({dtContract, dtAgreement, dtTankInfo})
            CommonFunctions.CleanUserTempDirectory(CONST_MAPID)
            Dim dtAttachment As DataTable = CommonFunctions.GetInitAttachmentFileList(Me.GBT00020RValues.AgreementNo, CONST_DIRNAME_LEASE_AGREEMENT, CONST_MAPID)
            retDataSet.Tables.Add(dtAttachment)
            prevDs = retDataSet

        ElseIf TypeOf Page.PreviousPage Is GBT00006RESULT Then
            Dim prevPage As GBT00006RESULT = DirectCast(Page.PreviousPage, GBT00006RESULT)
            Dim previd As String = prevPage.ClientID
            ViewState(CONST_VS_NAME_GBT00020SV) = prevPage.GBT00020LEASEValues.PrevDispItem
            Me.GBT00020RValues = prevPage.GBT00020LEASEValues.PrevDispItem
            Me.ddlOrderStart.SelectedValue = prevPage.GBT00020LEASEValues.OrderStartPoint
            retDataSet = prevPage.GBT00020LEASEValues.DispDs
            '引き当て後戻りの場合
            If prevPage.IsAllocateLeaseTank = True Then
                UpdateAllocTank(retDataSet.Tables(CONST_DT_NAME_TANKINFO), prevPage.AllocateTankList)
            End If
            prevDs = prevPage.GBT00020LEASEValues.PrevDispDs
            Me.hdnThisMapVariant.Value = CONST_MAPVARI
        ElseIf Page.PreviousPage Is Nothing Then
            '単票直接呼出しパターン(JavaScriptよりPOSTした内容を取得し判定)
            Throw New Exception("No PreviousPage Error")
        End If

        ViewState(CONST_VS_NAME_PREV_VAL) = prevDs '保存前の情報
        ViewState(CONST_VS_NAME_CURRENT_VAL) = retDataSet '編集中の情報保持用
        Return retVal
    End Function
    ''' <summary>
    ''' タンク一覧より引き当てた情報を元に一覧データを編集
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <param name="allocTankDt"></param>
    Private Sub UpdateAllocTank(dt As DataTable, allocTankDt As DataTable)
        '追加(一覧になし、選択タンクあり)
        Dim dispTankNo As New List(Of String)
        If dt IsNot Nothing Then
            dispTankNo = (From item In dt Select Convert.ToString(item("TANKNO"))).ToList
        End If
        If allocTankDt IsNot Nothing AndAlso allocTankDt.Rows.Count > 0 Then
            Dim appendTankNo = (From rowItem In allocTankDt Where Not dispTankNo.Contains(Convert.ToString(rowItem("TANKNO"))))
            If appendTankNo.Any Then
                For Each tankNo In appendTankNo
                    Dim dr As DataRow = dt.NewRow
                    dr.Item("TANKNO") = tankNo("TANKNO")
                    dr.Item("DEPOTOUT") = tankNo("DEPO_DEPOTCODE")
                    dr.Item("DEPOTOUTNAME") = tankNo("DEPO_NAMES")
                    dt.Rows.Add(dr)
                Next
            End If
        End If
        '削除(一覧にあり、選択タンクなし）
        Dim allocTankListWk As New List(Of String)
        If allocTankDt IsNot Nothing AndAlso allocTankDt.Rows.Count > 0 Then
            allocTankListWk = (From rowItem In allocTankDt Select Convert.ToString(rowItem("TANKNO"))).ToList
        End If
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            For rowId As Integer = dt.Rows.Count - 1 To 0 Step -1
                Dim dr As DataRow = dt.Rows(rowId)
                Dim delTankNo As String = Convert.ToString(dr.Item("TANKNO"))
                If Not (allocTankListWk.Contains(delTankNo)) Then
                    dt.Rows.Remove(dr)
                End If
            Next
        End If
        'タンクNoを元にソート
        Dim sortList = From item In dt Order By item("TANKNO")
        If sortList.Any Then
            Dim lineCnt As Integer = 1
            For Each sdr In sortList
                sdr.Item("LINECNT") = lineCnt
                lineCnt = lineCnt + 1
            Next
            dt = sortList.CopyToDataTable
        End If
    End Sub
    ''' <summary>
    ''' 協定書内部テーブル生成
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>1レコード前提</remarks>
    Private Function CreateAgreementTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = CONST_DT_NAME_AGREEMENT
        With retDt.Columns
            .Add("CONTRACTNO", GetType(String)).DefaultValue = ""
            .Add("AGREEMENTNO", GetType(String)).DefaultValue = ""
            .Add("STYMD", GetType(String)).DefaultValue = ""
            .Add("ENDYMD", GetType(String)).DefaultValue = ""
            .Add("LEASETERM", GetType(String)).DefaultValue = ""
            .Add("LEASETYPE", GetType(String)).DefaultValue = ""
            .Add("PRODUCTCODE", GetType(String)).DefaultValue = ""
            .Add("LEASEPAYMENTTYPE", GetType(String)).DefaultValue = ""

            .Add("AUTOEXTEND", GetType(String)).DefaultValue = ""
            .Add("AUTOEXTENDKIND", GetType(String)).DefaultValue = ""

            .Add("LEASEPAYMENTKIND", GetType(String)).DefaultValue = ""

            .Add("LEASEPAYMENTS", GetType(String)).DefaultValue = ""
            .Add("RELEASE", GetType(String)).DefaultValue = ""
            .Add("CURRENCY", GetType(String)).DefaultValue = ""
            .Add("TAXKIND", GetType(String)).DefaultValue = ""
            .Add("TAXRATE", GetType(String)).DefaultValue = ""
            .Add("REMARK", GetType(String)).DefaultValue = ""
            .Add("APPLYID", GetType(String)).DefaultValue = ""
            .Add("APPLYTEXT", GetType(String)).DefaultValue = ""
            .Add("LASTSTEP", GetType(String)).DefaultValue = ""
            .Add("RELATEDORDERNO", GetType(String)).DefaultValue = ""
            .Add("DELFLG", GetType(String)).DefaultValue = ""
            .Add("INITYMD", GetType(String)).DefaultValue = ""
            .Add("UPDYMD", GetType(String)).DefaultValue = ""
            .Add("UPDUSER", GetType(String)).DefaultValue = ""
            .Add("UPDTERMID", GetType(String)).DefaultValue = ""
            .Add("RECEIVEYMD", GetType(String)).DefaultValue = ""
            .Add("UPDTIMSTP", GetType(String)).DefaultValue = ""
            .Add("APPROVEDTEXT", GetType(String)).DefaultValue = ""
            .Add("APPLYDATE", GetType(String)).DefaultValue = ""
            .Add("APPLICANTID", GetType(String)).DefaultValue = ""
            .Add("APPLICANTNAME", GetType(String)).DefaultValue = ""
            .Add("APPROVEDATE", GetType(String)).DefaultValue = ""
            .Add("APPROVERID", GetType(String)).DefaultValue = ""
            .Add("APPROVERNAME", GetType(String)).DefaultValue = ""
            .Add("STATUS", GetType(String)).DefaultValue = ""
        End With
        Return retDt
    End Function
    ''' <summary>
    ''' タンク情報内部テーブル生成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateTankInfoTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = CONST_DT_NAME_TANKINFO
        With retDt.Columns
            .Add("LINECNT", GetType(Integer)).DefaultValue = 0
            .Add("CONTRACTNO", GetType(String)).DefaultValue = ""
            .Add("AGREEMENTNO", GetType(String)).DefaultValue = ""
            .Add("TANKNO", GetType(String)).DefaultValue = ""
            .Add("STYMD", GetType(String)).DefaultValue = ""
            .Add("ENDYMD", GetType(String)).DefaultValue = ""
            .Add("LEASESTYMD", GetType(String)).DefaultValue = ""
            .Add("LEASEENDYMDSCR", GetType(String)).DefaultValue = ""
            .Add("CANCELFLG", GetType(String)).DefaultValue = ""
            .Add("LEASEENDYMD", GetType(String)).DefaultValue = ""
            .Add("DEPOTOUT", GetType(String)).DefaultValue = ""
            .Add("DEPOTOUTNAME", GetType(String)).DefaultValue = ""
            .Add("DEPOTIN", GetType(String)).DefaultValue = ""
            .Add("DEPOTINNAME", GetType(String)).DefaultValue = ""
            .Add("PAYSTDAILY", GetType(String)).DefaultValue = ""
            .Add("PAYENDDAILY", GetType(String)).DefaultValue = ""
            .Add("REMARK", GetType(String)).DefaultValue = ""
            .Add("DELFLG", GetType(String)).DefaultValue = ""
            .Add("INITYMD", GetType(String)).DefaultValue = ""
            .Add("UPDYMD", GetType(String)).DefaultValue = ""
            .Add("UPDUSER", GetType(String)).DefaultValue = ""
            .Add("UPDTERMID", GetType(String)).DefaultValue = ""
            .Add("RECEIVEYMD", GetType(String)).DefaultValue = ""
            .Add("UPDTIMSTP", GetType(String)).DefaultValue = ""
            ''付帯情報
            '.Add("STATUS", GetType(String)).DefaultValue = ""
            'オーダー作成チェックボックス用
            .Add("TOORDER", GetType(String)).DefaultValue = ""
            'オーダー作成用TANKSEQ設定
            .Add("TANKSEQ", GetType(String)).DefaultValue = ""
        End With
        Return retDt
    End Function
    ''' <summary>
    ''' 取得したデータセットを元に画面に展開
    ''' </summary>
    ''' <param name="ds"></param>
    Private Sub SetDispValues(ds As DataSet)
        Dim dtCont As DataTable = ds.Tables(CONST_DT_NAME_CONTRACT)
        Dim drCont As DataRow = dtCont.Rows(0)
        Dim dtAgreement As DataTable = ds.Tables(CONST_DT_NAME_AGREEMENT)
        Dim drAgreement As DataRow = dtAgreement.Rows(0)
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        Dim dtAttachment As DataTable = ds.Tables(C_DTNAME_ATTACHMENT)
        '申請関連
        Me.hdnApplyStatus.Value = Convert.ToString(drAgreement.Item("STATUS")).Trim
        If Convert.ToString(drAgreement.Item("APPLYDATE")) <> "" Then
            Me.txtApplyDate.Text = Date.Parse(Convert.ToString(drAgreement.Item("APPLYDATE"))).ToString(GBA00003UserSetting.DATEFORMAT)
        Else
            Me.txtApplyDate.Text = Convert.ToString(drAgreement.Item("APPLYDATE")) 'Apply Date
        End If

        If Me.txtApplyDate.Text <> "" Then
            Me.txtApplyAgent.Text = Convert.ToString(drCont.Item("ORGANIZER")) 'Apply Office
        Else
            Me.txtApplyAgent.Text = ""
        End If
        Me.txtApplyPic.Text = Convert.ToString(drAgreement.Item("APPLICANTID")) 'Apply PIC
        Me.lblApplyPicText.Text = HttpUtility.HtmlEncode(Convert.ToString(drAgreement.Item("APPLICANTNAME"))) 'Apply PIC NAME(ラベルなのでHTMLエンコード)
        Me.lblApplyRemarks.Text = HttpUtility.HtmlEncode(Convert.ToString(drAgreement.Item("APPLYTEXT"))) 'Apply Remarks(ラベルなのでHTMLエンコード)

        If Convert.ToString(drAgreement.Item("APPROVEDATE")) <> "" Then
            Me.txtApprovedDate.Text = Date.Parse(Convert.ToString(drAgreement.Item("APPROVEDATE"))).ToString(GBA00003UserSetting.DATEFORMAT) 'Approved Date
        Else
            Me.txtApprovedDate.Text = Convert.ToString(drAgreement.Item("APPROVEDATE")) 'Approved Date
        End If
        Me.txtApprovedPic.Text = Convert.ToString(drAgreement.Item("APPROVERID"))   'Approved PIC
        Me.lblApprovedPicText.Text = Convert.ToString(drAgreement.Item("APPROVERNAME")) 'Approved PIC NAME(ラベルなのでHTMLエンコード)
        Me.lblAppJotRemarks.Text = Convert.ToString(drAgreement.Item("APPROVEDTEXT")) 'Approved Remarks(ラベルなのでHTMLエンコード)
        'メイン画面上部

        Me.lblAgreementNo.Text = Convert.ToString(drAgreement.Item("AGREEMENTNO"))

        Me.txtLeaseTerm.Text = Convert.ToString(drAgreement.Item("LEASETERM"))
        Me.txtLeaseType.Text = Convert.ToString(drAgreement.Item("LEASETYPE"))

        Me.txtProduct.Text = Convert.ToString(drAgreement.Item("PRODUCTCODE"))

        Me.txtPaymentMonth.Text = Convert.ToString(drAgreement.Item("LEASEPAYMENTTYPE"))
        Me.txtAutoExtend.Text = Convert.ToString(drAgreement.Item("AUTOEXTEND"))
        Me.txtAutoExtendKind.Text = Convert.ToString(drAgreement.Item("AUTOEXTENDKIND"))

        Me.txtLeasePaymentKind.Text = Convert.ToString(drAgreement.Item("LEASEPAYMENTKIND"))
        Me.txtLeasePayments.Text = Convert.ToString(drAgreement.Item("LEASEPAYMENTS"))
        Me.txtReLease.Text = Convert.ToString(drAgreement.Item("RELEASE"))
        Me.txtCurrency.Text = Convert.ToString(drAgreement.Item("CURRENCY"))

        Me.txtTax.Text = Convert.ToString(drAgreement.Item("TAXKIND"))
        Me.txtTaxRate.Text = Convert.ToString(drAgreement.Item("TAXRATE"))

        Me.txtRemarks.Text = Convert.ToString(drAgreement.Item("REMARK"))

        'タンク一覧
        Me.repTankInfo.DataSource = dtTankInfo
        Me.repTankInfo.DataBind()

        '文言設定
        txtLeaseTerm_Change()
        txtLeaseType_Change()
        txtProduct_Change()
        txtPaymentMonth_Change()
        txtAutoExtend_Change()
        txtAutoExtendKind_Change()
        txtLeasePaymentKind_Change()
        'txtLeasePayments_Change()
        txtTax_Change()

    End Sub
    ''' <summary>
    ''' 画面上のデータを取得し設定
    ''' </summary>
    ''' <returns>画面情報より取得したDataSet</returns>
    Private Function CollectDispValues() As DataSet
        Dim ds As DataSet = DirectCast(ViewState(CONST_VS_NAME_CURRENT_VAL), DataSet)
        Dim retDs As New DataSet
        With ds.Tables(CONST_DT_NAME_AGREEMENT).Rows(0)
            .Item("LEASETERM") = Me.txtLeaseTerm.Text
            .Item("LEASETYPE") = Me.txtLeaseType.Text
            .Item("PRODUCTCODE") = Me.txtProduct.Text
            .Item("LEASEPAYMENTTYPE") = Me.txtPaymentMonth.Text
            .Item("AUTOEXTEND") = Me.txtAutoExtend.Text
            .Item("AUTOEXTENDKIND") = Me.txtAutoExtendKind.Text
            .Item("LEASEPAYMENTKIND") = Me.txtLeasePaymentKind.Text
            .Item("LEASEPAYMENTS") = Me.txtLeasePayments.Text
            .Item("TAXKIND") = Me.txtTax.Text
            .Item("RELEASE") = Me.txtReLease.Text
            .Item("CURRENCY") = Me.txtCurrency.Text
            .Item("TAXRATE") = Me.txtTaxRate.Text
            .Item("REMARK") = Me.txtRemarks.Text
            .Item("APPLYTEXT") = HttpUtility.HtmlDecode(Me.lblApplyRemarks.Text)
        End With
        '添付ファイルグリッド
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
        ds.Tables.Remove(C_DTNAME_ATTACHMENT)
        ds.Tables.Add(dtAttachment)
        Return ds
    End Function
    ''' <summary>
    ''' 使用可否コントロール
    ''' </summary>
    Private Sub enabledControls()
        Dim dtCont As DataTable = Me.DsDisDisplayValues.Tables(CONST_DT_NAME_CONTRACT)
        Dim drCont As DataRow = dtCont.Rows(0)
        Me.btnCreateLeaseOrder.Disabled = True
        Me.btnSave.Disabled = True
        Me.btnApply.Disabled = True
        Me.btnAddNewTank.Disabled = True
        Dim enableCont As Boolean = True
        If Convert.ToString(drCont("ENABLED")) <> "Y" OrElse hdnThisMapVariant.Value = "GB_ShowLsDetail" Then
            enableCont = False
            Me.hdnUpload.Enabled = False
            Me.lblApplyRemarks.Enabled = False
        End If
        Dim lstInputObjects As New List(Of Control) From {Me.txtLeaseTerm, Me.txtLeaseType, Me.txtProduct,
                                                          Me.txtPaymentMonth, Me.txtAutoExtend, Me.txtAutoExtendKind,
                                                          Me.txtLeasePaymentKind, Me.txtLeasePayments, Me.txtReLease, Me.txtCurrency,
                                                          Me.txtTax, Me.txtTaxRate, Me.txtRemarks}

        Dim inputObjectsEnabled As Boolean = False
        '承認済協定書のみオーダー作成可能
        If enableCont AndAlso {C_APP_STATUS.COMPLETE, C_APP_STATUS.APPROVED}.Contains(Me.hdnApplyStatus.Value) Then
            Me.btnCreateLeaseOrder.Disabled = False
            Me.lblApplyRemarks.Enabled = False
        End If
        '申請中はタンク追加,保存させない
        If enableCont AndAlso Not {C_APP_STATUS.APPLYING}.Contains(Me.hdnApplyStatus.Value) Then
            Me.btnSave.Disabled = False
            Me.btnAddNewTank.Disabled = False
            'Me.lblApplyRemarks.Enabled = False
        End If
        '協定書テキスト入力エリアは新規、否認時のみ入力可能
        If enableCont AndAlso {"", C_APP_STATUS.APPAGAIN, C_APP_STATUS.REJECT}.Contains(hdnApplyStatus.Value) Then
            inputObjectsEnabled = True
            Me.btnApply.Disabled = False
        End If

        For Each obj As Control In lstInputObjects
            If TypeOf obj Is TextBox Then
                Dim txtObj As TextBox = DirectCast(obj, TextBox)
                txtObj.Enabled = inputObjectsEnabled
            End If
        Next
    End Sub
    ''' <summary>
    ''' SQLを実行し協定書・タンク情報を登録
    ''' </summary>
    ''' <param name="ds">画面上データセット</param>
    ''' <param name="prevDs">画面変更前データセット</param>
    Private Function EntryAgreement(ds As DataSet, prevDs As DataSet) As String
        Dim dtAgreement As DataTable = ds.Tables(CONST_DT_NAME_AGREEMENT)
        Dim drAgreement As DataRow = dtAgreement.Rows(0)
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        Dim dtAttachment As DataTable = ds.Tables(C_DTNAME_ATTACHMENT)
        Dim prevDtTankInfo As DataTable = prevDs.Tables(CONST_DT_NAME_TANKINFO)
        Dim deleteTankNo As New List(Of String)
        Dim agreementNo As String = ""
        If prevDtTankInfo IsNot Nothing AndAlso prevDtTankInfo.Rows.Count <> 0 Then
            If dtTankInfo.Rows.Count = 0 Then
                deleteTankNo = (From item In prevDtTankInfo Select Convert.ToString(item("TANKNO"))).ToList
            Else
                Dim currentTankNoList As List(Of String) = (From item In dtTankInfo Select Convert.ToString(item("TANKNO"))).ToList
                deleteTankNo = (From item In prevDtTankInfo Where Not currentTankNoList.Contains(Convert.ToString(item("TANKNO"))) Select Convert.ToString(item("TANKNO"))).ToList
            End If
        End If
        Try
            Dim procDate As Date = Now
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                Dim contractNo As String = Me.GBT00020RValues.ContractNo


                Using sqlTran = sqlCon.BeginTransaction
                    If Me.GBT00020RValues.AddAgreement Then
                        agreementNo = GBA00015Lease.GetNewAgreementNo(sqlCon, sqlTran)
                        InsertAgreement(Me.GBT00020RValues.ContractNo, agreementNo, drAgreement,
                                        sqlCon, sqlTran, procDate)
                    Else
                        agreementNo = Convert.ToString(drAgreement.Item("AGREEMENTNO"))
                        UpdateAgreement(drAgreement, sqlCon, sqlTran, procDate)
                    End If

                    DeleteTankInfo(drAgreement, deleteTankNo, sqlCon, sqlTran, procDate)
                    EntryTankInfo(contractNo, agreementNo, dtTankInfo, sqlCon, sqlTran, procDate)
                    '添付ファイルを正式フォルダに転送
                    CommonFunctions.SaveAttachmentFilesList(dtAttachment, agreementNo, CONST_DIRNAME_LEASE_AGREEMENT)

                    sqlTran.Commit()
                End Using

            End Using
            Return agreementNo
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' 協定書テーブル追加
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub InsertAgreement(contractNo As String, agreementNo As String, dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)

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
            sqlStat.AppendFormat("INSERT INTO {0} ", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine("  (")
            sqlStat.AppendLine("   CONTRACTNO")
            sqlStat.AppendLine("  ,AGREEMENTNO")
            sqlStat.AppendLine("  ,STYMD")
            sqlStat.AppendLine("  ,LEASETERM")
            sqlStat.AppendLine("  ,LEASETYPE")
            sqlStat.AppendLine("  ,PRODUCTCODE")
            sqlStat.AppendLine("  ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("  ,AUTOEXTEND")
            sqlStat.AppendLine("  ,AUTOEXTENDKIND")
            sqlStat.AppendLine("  ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("  ,LEASEPAYMENTS")
            sqlStat.AppendLine("  ,RELEASE")
            sqlStat.AppendLine("  ,CURRENCY")
            sqlStat.AppendLine("  ,TAXKIND")
            sqlStat.AppendLine("  ,TAXRATE")
            sqlStat.AppendLine("  ,REMARK")
            sqlStat.AppendLine("  ,APPLYID")
            sqlStat.AppendLine("  ,APPLYTEXT")
            sqlStat.AppendLine("  ,LASTSTEP")
            sqlStat.AppendLine("  ,RELATEDORDERNO")
            sqlStat.AppendLine("  ,DELFLG")
            sqlStat.AppendLine("  ,INITYMD")
            sqlStat.AppendLine("  ,UPDYMD")
            sqlStat.AppendLine("  ,UPDUSER")
            sqlStat.AppendLine("  ,UPDTERMID")
            sqlStat.AppendLine("  ,RECEIVEYMD")
            sqlStat.AppendLine("  ) VALUES (")
            sqlStat.AppendLine("   @CONTRACTNO")
            sqlStat.AppendLine("  ,@AGREEMENTNO")
            sqlStat.AppendLine("  ,@STYMD")
            sqlStat.AppendLine("  ,@LEASETERM")
            sqlStat.AppendLine("  ,@LEASETYPE")
            sqlStat.AppendLine("  ,@PRODUCTCODE")
            sqlStat.AppendLine("  ,@LEASEPAYMENTTYPE")
            sqlStat.AppendLine("  ,@AUTOEXTEND")
            sqlStat.AppendLine("  ,@AUTOEXTENDKIND")
            sqlStat.AppendLine("  ,@LEASEPAYMENTKIND")
            sqlStat.AppendLine("  ,@LEASEPAYMENTS")
            sqlStat.AppendLine("  ,@RELEASE")
            sqlStat.AppendLine("  ,@CURRENCY")
            sqlStat.AppendLine("  ,@TAXKIND")
            sqlStat.AppendLine("  ,@TAXRATE")
            sqlStat.AppendLine("  ,@REMARK")
            sqlStat.AppendLine("  ,@APPLYID")
            sqlStat.AppendLine("  ,@APPLYTEXT")
            sqlStat.AppendLine("  ,@LASTSTEP")
            sqlStat.AppendLine("  ,@RELATEDORDERNO")
            sqlStat.AppendLine("  ,@DELFLG")
            sqlStat.AppendLine("  ,@INITYMD")
            sqlStat.AppendLine("  ,@UPDYMD")
            sqlStat.AppendLine("  ,@UPDUSER")
            sqlStat.AppendLine("  ,@UPDTERMID")
            sqlStat.AppendLine("  ,@RECEIVEYMD")
            sqlStat.AppendLine("  )")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = contractNo
                    .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = agreementNo
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@LEASETERM", SqlDbType.NVarChar).Value = dr.Item("LEASETERM")
                    .Add("@LEASETYPE", SqlDbType.NVarChar).Value = dr.Item("LEASETYPE")
                    .Add("@PRODUCTCODE", SqlDbType.NVarChar).Value = dr.Item("PRODUCTCODE")
                    .Add("@LEASEPAYMENTTYPE", SqlDbType.NVarChar).Value = dr.Item("LEASEPAYMENTTYPE")
                    .Add("@AUTOEXTEND", SqlDbType.NVarChar).Value = dr.Item("AUTOEXTEND")
                    .Add("@AUTOEXTENDKIND", SqlDbType.NVarChar).Value = dr.Item("AUTOEXTENDKIND")
                    .Add("@LEASEPAYMENTKIND", SqlDbType.NVarChar).Value = dr.Item("LEASEPAYMENTKIND")
                    .Add("@LEASEPAYMENTS", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LEASEPAYMENTS")))
                    .Add("@RELEASE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("RELEASE")))
                    .Add("@CURRENCY", SqlDbType.NVarChar).Value = dr.Item("CURRENCY")
                    .Add("@TAXKIND", SqlDbType.NVarChar).Value = dr.Item("TAXKIND")
                    .Add("@TAXRATE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("TAXRATE")))
                    .Add("@REMARK", SqlDbType.NVarChar).Value = dr.Item("REMARK")
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = dr.Item("APPLYID")
                    .Add("@APPLYTEXT", SqlDbType.NVarChar).Value = dr.Item("APPLYTEXT")
                    .Add("@LASTSTEP", SqlDbType.NVarChar).Value = dr.Item("LASTSTEP")
                    .Add("@RELATEDORDERNO", SqlDbType.NVarChar).Value = ""
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
            Throw
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
    ''' 協定書テーブル更新
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub UpdateAgreement(dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
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
            sqlStat.AppendFormat("INSERT INTO {0} ", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine(" (")
            sqlStat.AppendLine("           CONTRACTNO")
            sqlStat.AppendLine("          ,AGREEMENTNO")
            sqlStat.AppendLine("          ,STYMD")
            sqlStat.AppendLine("          ,ENDYMD")
            sqlStat.AppendLine("          ,LEASETERM")
            sqlStat.AppendLine("          ,LEASETYPE")
            sqlStat.AppendLine("          ,PRODUCTCODE")
            sqlStat.AppendLine("          ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("          ,AUTOEXTEND")
            sqlStat.AppendLine("          ,AUTOEXTENDKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTS")
            sqlStat.AppendLine("          ,RELEASE")
            sqlStat.AppendLine("          ,CURRENCY")
            sqlStat.AppendLine("          ,TAXKIND")
            sqlStat.AppendLine("          ,TAXRATE")
            sqlStat.AppendLine("          ,REMARK")
            sqlStat.AppendLine("          ,APPLYID")
            sqlStat.AppendLine("          ,APPLYTEXT")
            sqlStat.AppendLine("          ,LASTSTEP")
            sqlStat.AppendLine("          ,RELATEDORDERNO")
            sqlStat.AppendLine("          ,DELFLG")
            sqlStat.AppendLine("          ,INITYMD")
            sqlStat.AppendLine("          ,UPDYMD")
            sqlStat.AppendLine("          ,UPDUSER")
            sqlStat.AppendLine("          ,UPDTERMID")
            sqlStat.AppendLine("          ,RECEIVEYMD")
            sqlStat.AppendLine(" )   SELECT ")
            sqlStat.AppendLine("           CONTRACTNO")
            sqlStat.AppendLine("          ,AGREEMENTNO")
            sqlStat.AppendLine("          ,STYMD")
            sqlStat.AppendLine("          ,ENDYMD")
            sqlStat.AppendLine("          ,LEASETERM")
            sqlStat.AppendLine("          ,LEASETYPE")
            sqlStat.AppendLine("          ,PRODUCTCODE")
            sqlStat.AppendLine("          ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("          ,AUTOEXTEND")
            sqlStat.AppendLine("          ,AUTOEXTENDKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTS")
            sqlStat.AppendLine("          ,RELEASE")
            sqlStat.AppendLine("          ,CURRENCY")
            sqlStat.AppendLine("          ,TAXKIND")
            sqlStat.AppendLine("          ,TAXRATE")
            sqlStat.AppendLine("          ,REMARK")
            sqlStat.AppendLine("          ,APPLYID")
            sqlStat.AppendLine("          ,APPLYTEXT")
            sqlStat.AppendLine("          ,LASTSTEP")
            sqlStat.AppendLine("          ,RELATEDORDERNO")
            sqlStat.AppendLine("          ,@DELFLG_YES")
            sqlStat.AppendLine("          ,@UPDYMD")
            sqlStat.AppendLine("          ,@UPDYMD")
            sqlStat.AppendLine("          ,@UPDUSER")
            sqlStat.AppendLine("          ,@UPDTERMID")
            sqlStat.AppendLine("          ,@RECEIVEYMD")
            sqlStat.AppendFormat("     FROM {0}", CONST_TBL_AGREEMENT)
            sqlStat.AppendLine("      WHERE CONTRACTNO  = @CONTRACTNO")
            sqlStat.AppendLine("        AND AGREEMENTNO = @AGREEMENTNO")
            sqlStat.AppendLine("        AND DELFLG      = @DELFLG")
            sqlStat.AppendLine(";")

            sqlStat.AppendFormat("UPDATE {0} ", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine("  SET")
            sqlStat.AppendLine("   LEASETERM        = @LEASETERM")
            sqlStat.AppendLine("  ,LEASETYPE        = @LEASETYPE")
            sqlStat.AppendLine("  ,PRODUCTCODE      = @PRODUCTCODE")
            sqlStat.AppendLine("  ,LEASEPAYMENTTYPE = @LEASEPAYMENTTYPE")
            sqlStat.AppendLine("  ,LEASEPAYMENTS    = @LEASEPAYMENTS")
            sqlStat.AppendLine("  ,RELEASE          = @RELEASE")
            sqlStat.AppendLine("  ,CURRENCY         = @CURRENCY")
            sqlStat.AppendLine("  ,TAXKIND          = @TAXKIND")
            sqlStat.AppendLine("  ,TAXRATE          = @TAXRATE")
            sqlStat.AppendLine("  ,AUTOEXTEND       = @AUTOEXTEND")
            sqlStat.AppendLine("  ,AUTOEXTENDKIND   = @AUTOEXTENDKIND")
            sqlStat.AppendLine("  ,LEASEPAYMENTKIND = @LEASEPAYMENTKIND")
            sqlStat.AppendLine("  ,REMARK           = @REMARK")
            sqlStat.AppendLine("  ,APPLYID          = @APPLYID")
            sqlStat.AppendLine("  ,APPLYTEXT        = @APPLYTEXT")
            sqlStat.AppendLine("  ,LASTSTEP         = @LASTSTEP")
            sqlStat.AppendLine("  ,RELATEDORDERNO   = @RELATEDORDERNO")
            sqlStat.AppendLine("  ,UPDYMD           = @UPDYMD")
            sqlStat.AppendLine("  ,UPDUSER          = @UPDUSER")
            sqlStat.AppendLine("  ,UPDTERMID        = @UPDTERMID")
            sqlStat.AppendLine("  ,RECEIVEYMD       = @RECEIVEYMD")
            sqlStat.AppendLine("  WHERE CONTRACTNO  = @CONTRACTNO")
            sqlStat.AppendLine("    AND AGREEMENTNO = @AGREEMENTNO")
            sqlStat.AppendLine("    AND DELFLG      = @DELFLG")
            sqlStat.AppendLine(";")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = dr.Item("CONTRACTNO")
                    .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = dr.Item("AGREEMENTNO")
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@LEASETERM", SqlDbType.NVarChar).Value = dr.Item("LEASETERM")
                    .Add("@LEASETYPE", SqlDbType.NVarChar).Value = dr.Item("LEASETYPE")
                    .Add("@PRODUCTCODE", SqlDbType.NVarChar).Value = dr.Item("PRODUCTCODE")
                    .Add("@LEASEPAYMENTTYPE", SqlDbType.NVarChar).Value = dr.Item("LEASEPAYMENTTYPE")
                    .Add("@LEASEPAYMENTKIND", SqlDbType.NVarChar).Value = dr.Item("LEASEPAYMENTKIND")
                    .Add("@LEASEPAYMENTS", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LEASEPAYMENTS")))
                    .Add("@RELEASE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("RELEASE")))
                    .Add("@CURRENCY", SqlDbType.NVarChar).Value = dr.Item("CURRENCY")
                    .Add("@TAXKIND", SqlDbType.NVarChar).Value = dr.Item("TAXKIND")
                    .Add("@TAXRATE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("TAXRATE")))
                    .Add("@AUTOEXTEND", SqlDbType.NVarChar).Value = dr.Item("AUTOEXTEND")
                    .Add("@AUTOEXTENDKIND", SqlDbType.NVarChar).Value = dr.Item("AUTOEXTENDKIND")
                    .Add("@REMARK", SqlDbType.NVarChar).Value = dr.Item("REMARK")
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = dr.Item("APPLYID")
                    .Add("@APPLYTEXT", SqlDbType.NVarChar).Value = dr.Item("APPLYTEXT")
                    .Add("@LASTSTEP", SqlDbType.NVarChar).Value = dr.Item("LASTSTEP")
                    .Add("@RELATEDORDERNO", SqlDbType.NVarChar).Value = dr.Item("RELATEDORDERNO")

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO

                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                    .Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With
                sqlCmd.ExecuteNonQuery()
            End Using
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
    End Sub
    ''' <summary>
    ''' 協定書テーブルのオーダー連動更新
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub UpdateAgreementOrderRelate(orderNo As String, dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
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
            sqlStat.AppendFormat("INSERT INTO {0} ", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine(" (")
            sqlStat.AppendLine("           CONTRACTNO")
            sqlStat.AppendLine("          ,AGREEMENTNO")
            sqlStat.AppendLine("          ,STYMD")
            sqlStat.AppendLine("          ,ENDYMD")
            sqlStat.AppendLine("          ,LEASETERM")
            sqlStat.AppendLine("          ,LEASETYPE")
            sqlStat.AppendLine("          ,PRODUCTCODE")
            sqlStat.AppendLine("          ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("          ,AUTOEXTEND")
            sqlStat.AppendLine("          ,AUTOEXTENDKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTS")
            sqlStat.AppendLine("          ,RELEASE")
            sqlStat.AppendLine("          ,CURRENCY")
            sqlStat.AppendLine("          ,TAXKIND")
            sqlStat.AppendLine("          ,TAXRATE")
            sqlStat.AppendLine("          ,REMARK")
            sqlStat.AppendLine("          ,APPLYID")
            sqlStat.AppendLine("          ,APPLYTEXT")
            sqlStat.AppendLine("          ,LASTSTEP")
            sqlStat.AppendLine("          ,RELATEDORDERNO")
            sqlStat.AppendLine("          ,DELFLG")
            sqlStat.AppendLine("          ,INITYMD")
            sqlStat.AppendLine("          ,UPDYMD")
            sqlStat.AppendLine("          ,UPDUSER")
            sqlStat.AppendLine("          ,UPDTERMID")
            sqlStat.AppendLine("          ,RECEIVEYMD")
            sqlStat.AppendLine(" )   SELECT ")
            sqlStat.AppendLine("           CONTRACTNO")
            sqlStat.AppendLine("          ,AGREEMENTNO")
            sqlStat.AppendLine("          ,STYMD")
            sqlStat.AppendLine("          ,ENDYMD")
            sqlStat.AppendLine("          ,LEASETERM")
            sqlStat.AppendLine("          ,LEASETYPE")
            sqlStat.AppendLine("          ,PRODUCTCODE")
            sqlStat.AppendLine("          ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("          ,AUTOEXTEND")
            sqlStat.AppendLine("          ,AUTOEXTENDKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTS")
            sqlStat.AppendLine("          ,RELEASE")
            sqlStat.AppendLine("          ,CURRENCY")
            sqlStat.AppendLine("          ,TAXKIND")
            sqlStat.AppendLine("          ,TAXRATE")
            sqlStat.AppendLine("          ,REMARK")
            sqlStat.AppendLine("          ,APPLYID")
            sqlStat.AppendLine("          ,APPLYTEXT")
            sqlStat.AppendLine("          ,LASTSTEP")
            sqlStat.AppendLine("          ,RELATEDORDERNO")
            sqlStat.AppendLine("          ,@DELFLG_YES")
            sqlStat.AppendLine("          ,@UPDYMD")
            sqlStat.AppendLine("          ,@UPDYMD")
            sqlStat.AppendLine("          ,@UPDUSER")
            sqlStat.AppendLine("          ,@UPDTERMID")
            sqlStat.AppendLine("          ,@RECEIVEYMD")
            sqlStat.AppendFormat("     FROM {0}", CONST_TBL_AGREEMENT)
            sqlStat.AppendLine("      WHERE CONTRACTNO  = @CONTRACTNO")
            sqlStat.AppendLine("        AND AGREEMENTNO = @AGREEMENTNO")
            sqlStat.AppendLine("        AND DELFLG      = @DELFLG")
            sqlStat.AppendLine(";")

            sqlStat.AppendFormat("UPDATE {0} ", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine("  SET")
            sqlStat.AppendLine("   RELATEDORDERNO  = @RELATEDORDERNO")
            sqlStat.AppendLine("  ,UPDYMD          = @UPDYMD")
            sqlStat.AppendLine("  ,UPDUSER         = @UPDUSER")
            sqlStat.AppendLine("  ,UPDTERMID       = @UPDTERMID")
            sqlStat.AppendLine("  ,RECEIVEYMD      = @RECEIVEYMD")
            sqlStat.AppendLine("  WHERE CONTRACTNO  = @CONTRACTNO")
            sqlStat.AppendLine("    AND AGREEMENTNO = @AGREEMENTNO")
            sqlStat.AppendLine("    AND DELFLG      = @DELFLG")
            sqlStat.AppendLine(";")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = dr.Item("CONTRACTNO")
                    .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = dr.Item("AGREEMENTNO")
                    .Add("@RELATEDORDERNO", SqlDbType.NVarChar).Value = orderNo

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO

                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                    .Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With
                sqlCmd.ExecuteNonQuery()
            End Using
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
    End Sub
    ''' <summary>
    ''' タンク情報登録
    ''' </summary>
    ''' <param name="contractNo"></param>
    ''' <param name="agreementNo"></param>
    ''' <param name="dtTankInfo">タンク情報データテーブル</param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="procDate"></param>
    Private Sub EntryTankInfo(contractNo As String, agreementNo As String, dtTankInfo As DataTable, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
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
            Dim sqlStatTankSel As New StringBuilder
            sqlStatTankSel.AppendLine("SELECT ")
            sqlStatTankSel.AppendLine("    CASE TGT.LEASESTYMD     WHEN '1900/01/01' THEN '' ELSE FORMAT(TGT.LEASESTYMD,    'yyyy/MM/dd')  END AS LEASESTYMD")
            sqlStatTankSel.AppendLine("   ,CASE TGT.LEASEENDYMDSCR WHEN '1900/01/01' THEN '' ELSE FORMAT(TGT.LEASEENDYMDSCR,'yyyy/MM/dd')  END AS LEASEENDYMDSCR")
            sqlStatTankSel.AppendLine("   ,TGT.CANCELFLG")
            sqlStatTankSel.AppendLine("   ,CASE TGT.LEASEENDYMD    WHEN '1900/01/01' THEN '' ELSE FORMAT(TGT.LEASEENDYMD,   'yyyy/MM/dd')  END AS LEASEENDYMD")
            sqlStatTankSel.AppendLine("   ,TGT.DEPOTOUT")
            sqlStatTankSel.AppendLine("   ,TGT.DEPOTIN")
            sqlStatTankSel.AppendLine("   ,TGT.PAYSTDAILY")
            sqlStatTankSel.AppendLine("   ,TGT.PAYENDDAILY")
            sqlStatTankSel.AppendLine("   ,TGT.REMARK")
            sqlStatTankSel.AppendFormat("FROM {0} TGT ", CONST_TBL_TANK).AppendLine()
            sqlStatTankSel.AppendLine(" WHERE TGT.CONTRACTNO  = @CONTRACTNO")
            sqlStatTankSel.AppendLine("   AND TGT.AGREEMENTNO = @AGREEMENTNO")
            sqlStatTankSel.AppendLine("   AND TGT.TANKNO      = @TANKNO")
            sqlStatTankSel.AppendLine("   AND TGT.DELFLG      = @DELFLG")

            Dim sqlStatUpdate As New StringBuilder
            sqlStatUpdate.AppendFormat("  INSERT INTO {0} ( ", CONST_TBL_TANK).AppendLine()
            sqlStatUpdate.AppendLine("   CONTRACTNO")
            sqlStatUpdate.AppendLine("  ,AGREEMENTNO")
            sqlStatUpdate.AppendLine("  ,TANKNO")
            sqlStatUpdate.AppendLine("  ,STYMD")
            sqlStatUpdate.AppendLine("  ,ENDYMD")
            sqlStatUpdate.AppendLine("  ,LEASESTYMD")
            sqlStatUpdate.AppendLine("  ,LEASEENDYMDSCR")
            sqlStatUpdate.AppendLine("  ,CANCELFLG")
            sqlStatUpdate.AppendLine("  ,LEASEENDYMD")
            sqlStatUpdate.AppendLine("  ,DEPOTOUT")
            sqlStatUpdate.AppendLine("  ,DEPOTIN")
            sqlStatUpdate.AppendLine("  ,PAYSTDAILY")
            sqlStatUpdate.AppendLine("  ,PAYENDDAILY")
            sqlStatUpdate.AppendLine("  ,REMARK")
            sqlStatUpdate.AppendLine("  ,DELFLG")
            sqlStatUpdate.AppendLine("  ,INITYMD")
            sqlStatUpdate.AppendLine("  ,UPDYMD")
            sqlStatUpdate.AppendLine("  ,UPDUSER")
            sqlStatUpdate.AppendLine("  ,UPDTERMID")
            sqlStatUpdate.AppendLine("  ,RECEIVEYMD")
            'sqlStatUpdate.AppendLine("  ,STATUS")
            sqlStatUpdate.AppendLine("  ) SELECT ")
            sqlStatUpdate.AppendLine("      TGT.CONTRACTNO")
            sqlStatUpdate.AppendLine("     ,TGT.AGREEMENTNO")
            sqlStatUpdate.AppendLine("     ,TGT.TANKNO")
            sqlStatUpdate.AppendLine("     ,TGT.STYMD")
            sqlStatUpdate.AppendLine("     ,@ENDYMD AS ENDYMD")
            sqlStatUpdate.AppendLine("     ,TGT.LEASESTYMD")
            sqlStatUpdate.AppendLine("     ,TGT.LEASEENDYMDSCR")
            sqlStatUpdate.AppendLine("     ,TGT.CANCELFLG")
            sqlStatUpdate.AppendLine("     ,TGT.LEASEENDYMD")
            sqlStatUpdate.AppendLine("     ,TGT.DEPOTOUT")
            sqlStatUpdate.AppendLine("     ,TGT.DEPOTIN")
            sqlStatUpdate.AppendLine("     ,TGT.PAYSTDAILY")
            sqlStatUpdate.AppendLine("     ,TGT.PAYENDDAILY")
            sqlStatUpdate.AppendLine("     ,TGT.REMARK")
            sqlStatUpdate.AppendLine("     ,@DELFLG_YES AS DELFLG")
            sqlStatUpdate.AppendLine("     ,@INITYMD    AS INITYMD")
            sqlStatUpdate.AppendLine("     ,@UPDYMD     AS UPDYMD")
            sqlStatUpdate.AppendLine("     ,@UPDUSER    AS UPDUSER")
            sqlStatUpdate.AppendLine("     ,@UPDTERMID  AS UPDTERMID")
            sqlStatUpdate.AppendLine("     ,@RECEIVEYMD AS RECEIVEYMD")
            'sqlStatUpdate.AppendLine("     ,TGT.STATUS")
            sqlStatUpdate.AppendFormat(" FROM {0} TGT", CONST_TBL_TANK).AppendLine()
            sqlStatUpdate.AppendLine(" WHERE TGT.CONTRACTNO  = @CONTRACTNO")
            sqlStatUpdate.AppendLine("   AND TGT.AGREEMENTNO = @AGREEMENTNO")
            sqlStatUpdate.AppendLine("   AND TGT.TANKNO      = @TANKNO")
            sqlStatUpdate.AppendLine("   AND TGT.DELFLG      = @DELFLG;")

            sqlStatUpdate.AppendFormat("  UPDATE {0} ", CONST_TBL_TANK).AppendLine()
            sqlStatUpdate.AppendLine("  SET LEASESTYMD     = @LEASESTYMD")
            sqlStatUpdate.AppendLine("     ,LEASEENDYMDSCR = @LEASEENDYMDSCR")
            sqlStatUpdate.AppendLine("     ,CANCELFLG      = @CANCELFLG")
            sqlStatUpdate.AppendLine("     ,LEASEENDYMD    = @LEASEENDYMD")
            sqlStatUpdate.AppendLine("     ,DEPOTOUT       = @DEPOTOUT")
            sqlStatUpdate.AppendLine("     ,DEPOTIN        = @DEPOTIN")
            sqlStatUpdate.AppendLine("     ,PAYSTDAILY     = @PAYSTDAILY")
            sqlStatUpdate.AppendLine("     ,PAYENDDAILY    = @PAYENDDAILY")
            sqlStatUpdate.AppendLine("     ,REMARK         = @REMARK")
            sqlStatUpdate.AppendLine("     ,UPDYMD         = @UPDYMD")
            sqlStatUpdate.AppendLine("     ,UPDUSER        = @UPDUSER")
            sqlStatUpdate.AppendLine("     ,UPDTERMID      = @UPDTERMID")
            sqlStatUpdate.AppendLine("     ,RECEIVEYMD     = @RECEIVEYMD")
            sqlStatUpdate.AppendLine(" WHERE CONTRACTNO  = @CONTRACTNO")
            sqlStatUpdate.AppendLine("   AND AGREEMENTNO = @AGREEMENTNO")
            sqlStatUpdate.AppendLine("   AND TANKNO      = @TANKNO")
            sqlStatUpdate.AppendLine("   AND DELFLG      = @DELFLG;")

            Dim sqlStatInsert As New StringBuilder
            sqlStatInsert.AppendFormat("  INSERT INTO {0} ( ", CONST_TBL_TANK).AppendLine()
            sqlStatInsert.AppendLine("   CONTRACTNO")
            sqlStatInsert.AppendLine("  ,AGREEMENTNO")
            sqlStatInsert.AppendLine("  ,TANKNO")
            sqlStatInsert.AppendLine("  ,STYMD")
            sqlStatInsert.AppendLine("  ,LEASESTYMD")
            sqlStatInsert.AppendLine("  ,LEASEENDYMDSCR")
            sqlStatInsert.AppendLine("  ,CANCELFLG")
            sqlStatInsert.AppendLine("  ,LEASEENDYMD")
            sqlStatInsert.AppendLine("  ,DEPOTOUT")
            sqlStatInsert.AppendLine("  ,DEPOTIN")
            sqlStatInsert.AppendLine("  ,PAYSTDAILY")
            sqlStatInsert.AppendLine("  ,PAYENDDAILY")
            sqlStatInsert.AppendLine("  ,REMARK")
            sqlStatInsert.AppendLine("  ,DELFLG")
            sqlStatInsert.AppendLine("  ,INITYMD")
            sqlStatInsert.AppendLine("  ,UPDYMD")
            sqlStatInsert.AppendLine("  ,UPDUSER")
            sqlStatInsert.AppendLine("  ,UPDTERMID")
            sqlStatInsert.AppendLine("  ,RECEIVEYMD")
            'sqlStatInsert.AppendLine("  ,STATUS")
            sqlStatInsert.AppendLine("  ) VALUES (")
            sqlStatInsert.AppendLine("      @CONTRACTNO")
            sqlStatInsert.AppendLine("     ,@AGREEMENTNO")
            sqlStatInsert.AppendLine("     ,@TANKNO")
            sqlStatInsert.AppendLine("     ,@STYMD")
            sqlStatInsert.AppendLine("     ,@LEASESTYMD")
            sqlStatInsert.AppendLine("     ,@LEASEENDYMDSCR")
            sqlStatInsert.AppendLine("     ,@CANCELFLG")
            sqlStatInsert.AppendLine("     ,@LEASEENDYMD")
            sqlStatInsert.AppendLine("     ,@DEPOTOUT")
            sqlStatInsert.AppendLine("     ,@DEPOTIN")
            sqlStatInsert.AppendLine("     ,@PAYSTDAILY")
            sqlStatInsert.AppendLine("     ,@PAYENDDAILY")
            sqlStatInsert.AppendLine("     ,@REMARK")
            sqlStatInsert.AppendLine("     ,@DELFLG")
            sqlStatInsert.AppendLine("     ,@INITYMD")
            sqlStatInsert.AppendLine("     ,@UPDYMD")
            sqlStatInsert.AppendLine("     ,@UPDUSER")
            sqlStatInsert.AppendLine("     ,@UPDTERMID")
            sqlStatInsert.AppendLine("     ,@RECEIVEYMD")
            'sqlStatInsert.AppendLine("     ,''")
            sqlStatInsert.AppendLine("  ); ")

            Using sqlCmd As New SqlCommand()
                sqlCmd.Connection = sqlCon
                sqlCmd.Transaction = tran

                '動的パラメータのみ変数化
                Dim paramTankNo = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar)
                Dim paramLeaseYmd = sqlCmd.Parameters.Add("@LEASESTYMD", SqlDbType.Date)
                Dim paramLeaseYmdScr = sqlCmd.Parameters.Add("@LEASEENDYMDSCR", SqlDbType.Date)
                Dim paramCancelFlg = sqlCmd.Parameters.Add("@CANCELFLG", SqlDbType.NVarChar)
                Dim paramLeaseEndYmd = sqlCmd.Parameters.Add("@LEASEENDYMD", SqlDbType.Date)
                Dim paramDepotOut = sqlCmd.Parameters.Add("@DEPOTOUT", SqlDbType.NVarChar)
                Dim paramDepotIn = sqlCmd.Parameters.Add("@DEPOTIN", SqlDbType.NVarChar)

                Dim paramPayStDaily = sqlCmd.Parameters.Add("@PAYSTDAILY", SqlDbType.NVarChar)
                Dim paramPayEndDaily = sqlCmd.Parameters.Add("@PAYENDDAILY", SqlDbType.NVarChar)

                Dim paramRemark = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar)
                With sqlCmd.Parameters
                    '固定パラメータ
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = contractNo
                    .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = agreementNo
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@ENDYMD", SqlDbType.Date).Value = procDate.AddDays(-1)
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                    .Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                End With

                For Each drTankInfo As DataRow In dtTankInfo.Rows
                    paramTankNo.Value = drTankInfo.Item("TANKNO")
                    paramLeaseYmd.Value = DateStringToDateTime(Convert.ToString(drTankInfo.Item("LEASESTYMD")))
                    paramLeaseYmdScr.Value = DateStringToDateTime(Convert.ToString(drTankInfo.Item("LEASEENDYMDSCR")))
                    paramCancelFlg.Value = drTankInfo.Item("CANCELFLG")
                    paramLeaseEndYmd.Value = DateStringToDateTime(Convert.ToString(drTankInfo.Item("LEASEENDYMD")))
                    paramDepotOut.Value = drTankInfo.Item("DEPOTOUT")
                    paramDepotIn.Value = drTankInfo.Item("DEPOTIN")
                    paramPayStDaily.Value = drTankInfo.Item("PAYSTDAILY")
                    paramPayEndDaily.Value = drTankInfo.Item("PAYENDDAILY")
                    paramRemark.Value = drTankInfo.Item("REMARK")
                    '同一タンクNoの検索
                    sqlCmd.CommandText = sqlStatTankSel.ToString
                    Dim retVal As DataTable = New DataTable
                    Using sqlDa As New SqlDataAdapter(sqlCmd)
                        sqlDa.Fill(retVal)
                    End Using
                    '検索結果が無い場合は新規登録
                    If retVal Is Nothing OrElse retVal.Rows.Count = 0 Then
                        sqlCmd.CommandText = sqlStatInsert.ToString
                        sqlCmd.ExecuteNonQuery()
                        Continue For
                    End If
                    '検索結果がある場合は値の変化を検索し変化があるタンクNoにつき更新
                    Dim isModTankInfo As Boolean = False
                    Dim drRetVal As DataRow = retVal.Rows(0)
                    For Each colObj As DataColumn In retVal.Columns
                        If Not drRetVal(colObj.ColumnName).Equals(colObj.ColumnName) Then
                            isModTankInfo = True
                            Exit For
                        End If
                    Next '変更チェック用For
                    If isModTankInfo Then
                        sqlCmd.CommandText = sqlStatUpdate.ToString
                        sqlCmd.ExecuteNonQuery()
                    End If

                Next 'タンク情報ループEnd
            End Using
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
    End Sub
    ''' <summary>
    ''' タンク情報より論理削除
    ''' </summary>
    ''' <param name="tankNoList"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="procDate"></param>
    Private Sub DeleteTankInfo(drAgreement As DataRow, tankNoList As List(Of String), Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)
        '本当はDeleteFlg立てInsertすること、説明会向けの為UPDATE文のみ
        Dim canCloseConnect As Boolean = False

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Try
            If tankNoList Is Nothing OrElse tankNoList.Count = 0 Then
                Return
            End If

            Dim targetTankNo As String = ""
            For Each tankNoItem In tankNoList
                If targetTankNo = "" Then
                    targetTankNo = "'" & tankNoItem & "'"
                Else
                    targetTankNo = targetTankNo & ",'" & tankNoItem & "'"
                End If
            Next


            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("UPDATE {0} ", CONST_TBL_TANK).AppendLine()
            sqlStat.AppendLine("  SET")
            sqlStat.AppendLine("   DELFLG          = @DELFLG_YES")
            sqlStat.AppendLine("  ,UPDYMD          = @UPDYMD")
            sqlStat.AppendLine("  ,UPDUSER         = @UPDUSER")
            sqlStat.AppendLine("  ,UPDTERMID       = @UPDTERMID")
            sqlStat.AppendLine("  ,RECEIVEYMD      = @RECEIVEYMD")
            sqlStat.AppendLine("  WHERE CONTRACTNO  = @CONTRACTNO")
            sqlStat.AppendLine("    AND AGREEMENTNO = @AGREEMENTNO")
            sqlStat.AppendLine("    AND DELFLG      = @DELFLG")
            sqlStat.AppendFormat("    AND TANKNO   IN({0})", targetTankNo).AppendLine()

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = drAgreement.Item("CONTRACTNO")
                    .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = drAgreement.Item("AGREEMENTNO")

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                End With
                sqlCmd.ExecuteNonQuery()
            End Using
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
    End Sub

    ''' <summary>
    ''' 左ボックスのリストデータをクリア
    ''' </summary>
    ''' <remarks>viewstateのデータ量軽減</remarks>
    Private Sub ClearLeftListData()
        Me.lbProduct.Items.Clear()
    End Sub
    ''' <summary>
    ''' デポマスタより情報を取得
    ''' </summary>
    ''' <param name="targetTextObject">国コードテキストボックス</param>
    ''' <param name="depotCode">積載品コード</param>
    Private Sub SetDisplayDepot(targetTextObject As TextBox, depotCode As String)
        'デポの付帯情報を一旦クリア
        targetTextObject.Text = depotCode.Trim
        Dim targetLabel As Label = Me.lblDepoInText
        targetLabel.Text = ""

        'デポコードが未入力の場合はDBアクセスせずに終了
        If depotCode.Trim = "" Then
            Return
        End If

        Dim dt As DataTable = GetDepot(depotCode.Trim)
        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        '取得データを画面に展開
        Dim dr As DataRow = dt.Rows(0)
        targetLabel.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))

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
        Me.txtUnNo.Text = ""
        'Me.txtSGravity.Text = ""

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
        Me.txtUnNo.Text = unno
        If unno = "" Then
            '    Me.txtUNNo.Text = PRODUCT_NA
            Me.txtUnNo.Text = PRODUCT_NONDG
        End If

    End Sub
    ''' <summary>
    ''' オーダー作成起点ドロップダウンリストの選択肢取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetCreateOrderStartPoint() As List(Of ListItem)
        Dim lbEngVal As New ListBox
        Dim lbJpVal As New ListBox
        Dim COA0017FixValue As New COA0017FixValue With
            {.COMPCODE = GBC_COMPCODE_D,
              .CLAS = "LEASEORDERFROM",
              .LISTBOX1 = lbJpVal,
              .LISTBOX2 = lbEngVal
            }
        COA0017FixValue.COA0017getListFixValue()
        Dim lbTarget As ListBox
        If COA0019Session.LANGDISP = C_LANG.JA Then
            lbTarget = lbJpVal
        Else
            lbTarget = lbEngVal
        End If
        Dim retVal As New List(Of ListItem)
        For Each item As ListItem In lbTarget.Items
            retVal.Add(New ListItem(item.Text, item.Value))
        Next
        Return retVal
    End Function
    ''' <summary>
    ''' 積載品検索
    ''' </summary>
    ''' <param name="productCode">積載品コード（省略時は全件）</param>
    ''' <returns></returns>
    Private Function GetProduct(Optional productCode As String = "") As DataTable
        Dim retDt = GBA00014Product.GBA00014getProductCodeValue(productCode, CONST_FLAG_YES)
        Return retDt
    End Function
    ''' <summary>
    ''' 協定書テーブルよりデータを取得
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <param name="contractNo"></param>
    ''' <param name="agreementNo"></param>
    ''' <returns></returns>
    Private Function GetAgreement(dt As DataTable, contractNo As String, agreementNo As String) As DataTable
        Dim retDt As DataTable = dt.Clone
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT AGR.CONTRACTNO")
        sqlStat.AppendLine("      ,AGR.AGREEMENTNO")
        sqlStat.AppendLine("      ,AGR.STYMD")
        sqlStat.AppendLine("      ,AGR.ENDYMD")
        sqlStat.AppendLine("      ,AGR.LEASETERM")
        sqlStat.AppendLine("      ,AGR.LEASETYPE")
        sqlStat.AppendLine("      ,AGR.PRODUCTCODE")
        sqlStat.AppendLine("      ,AGR.LEASEPAYMENTTYPE")

        sqlStat.AppendLine("      ,AGR.AUTOEXTEND")
        sqlStat.AppendLine("      ,AGR.AUTOEXTENDKIND")

        sqlStat.AppendLine("      ,AGR.LEASEPAYMENTKIND")
        sqlStat.AppendLine("      ,AGR.LEASEPAYMENTS")
        sqlStat.AppendLine("      ,AGR.RELEASE")
        sqlStat.AppendLine("      ,AGR.CURRENCY")

        sqlStat.AppendLine("      ,AGR.TAXKIND")
        sqlStat.AppendLine("      ,AGR.TAXRATE")

        sqlStat.AppendLine("      ,AGR.REMARK")
        sqlStat.AppendLine("      ,AGR.APPLYID")
        sqlStat.AppendLine("      ,AGR.APPLYTEXT")
        sqlStat.AppendLine("      ,AGR.LASTSTEP")
        sqlStat.AppendLine("      ,AGR.RELATEDORDERNO")
        sqlStat.AppendLine("      ,AGR.DELFLG")
        sqlStat.AppendLine("     , ISNULL(CONVERT(nvarchar, AGR.INITYMD , 120),'') AS INITYMD")
        sqlStat.AppendLine("     , ISNULL(CONVERT(nvarchar, AGR.UPDYMD , 120),'')  AS UPDYMD")
        sqlStat.AppendLine("     , ISNULL(RTRIM(AGR.UPDUSER),'')                   AS UPDUSER")
        sqlStat.AppendLine("     , ISNULL(RTRIM(AGR.UPDTERMID),'')                 AS UPDTERMID")
        '申請情報
        sqlStat.AppendLine("      ,ISNULL(AH.APPROVEDTEXT,'') AS APPROVEDTEXT")
        sqlStat.AppendLine("      ,CASE CONVERT(NVARCHAR, AH.APPLYDATE , 111) WHEN '1900/01/01' THEN '' ELSE CONVERT(NVARCHAR, AH.APPLYDATE , 111) END AS APPLYDATE")
        sqlStat.AppendLine("      ,AH.APPLICANTID AS APPLICANTID")
        sqlStat.AppendLine("      ,ISNULL(US1.STAFFNAMES_EN,'') AS APPLICANTNAME")
        sqlStat.AppendLine("      ,CASE CONVERT(NVARCHAR, AH.APPROVEDATE , 111) WHEN '1900/01/01' THEN '' ELSE CONVERT(NVARCHAR, AH.APPROVEDATE , 111) END AS APPROVEDATE")
        sqlStat.AppendLine("      ,AH.APPROVERID AS APPROVERID")
        sqlStat.AppendLine("      ,AH.STATUS AS STATUS")
        sqlStat.AppendLine("      ,ISNULL(US2.STAFFNAMES_EN,'') AS APPROVERNAME")
        sqlStat.AppendFormat("        FROM {0} AGR", CONST_TBL_AGREEMENT).AppendLine()
        '申請情報JOIN
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH")
        sqlStat.AppendLine("    ON  AH.COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND  AH.APPLYID     = AGR.APPLYID")
        sqlStat.AppendLine("   AND  AH.STEP        = AGR.LASTSTEP")
        sqlStat.AppendLine("   AND  AH.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US1")
        sqlStat.AppendLine("    ON  US1.USERID      = AH.APPLICANTID")
        sqlStat.AppendLine("   AND  US1.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US2")
        sqlStat.AppendLine("    ON  US2.USERID      = AH.APPROVERID")
        sqlStat.AppendLine("   AND  US2.DELFLG     <> @DELFLG")
        'sqlStat.AppendLine("  LEFT JOIN COS0005_USER US3")
        'sqlStat.AppendLine("    ON  US3.USERID      = BS.INITUSER")
        'sqlStat.AppendLine("   AND  US3.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("         WHERE AGR.CONTRACTNO  = @CONTRACTNO")
        sqlStat.AppendLine("           AND AGR.AGREEMENTNO = @AGREEMENTNO")
        sqlStat.AppendLine("           AND AGR.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("           AND AGR.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("           AND AGR.DELFLG     <> @DELFLG")

        Dim dtDbResult As New DataTable
        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = contractNo
                .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = agreementNo
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
    ''' 協定書に紐づいたタンク取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>暫定</remarks>
    Private Function GetTankListInfo(dt As DataTable, contractNo As String, agreementNo As String, Optional tankNo As String = "") As DataTable

        Dim retDt As DataTable = dt.Clone
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY TANKNO) As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,TBL.*")
        sqlStat.AppendLine("  FROM (")
        sqlStat.AppendLine("      SELECT ")
        sqlStat.AppendLine("      TKI.CONTRACTNO")
        sqlStat.AppendLine("     ,TKI.AGREEMENTNO")
        sqlStat.AppendLine("     ,TKI.TANKNO")
        sqlStat.AppendLine("     ,TKI.STYMD")
        sqlStat.AppendLine("     ,TKI.ENDYMD")
        sqlStat.AppendLine("     ,CASE TKI.LEASESTYMD   WHEN '1900/01/01' THEN '' ELSE FORMAT(TKI.LEASESTYMD,  'yyyy/MM/dd')  END AS LEASESTYMD")
        sqlStat.AppendLine("     ,CASE TKI.LEASEENDYMDSCR   WHEN '1900/01/01' THEN '' ELSE FORMAT(TKI.LEASEENDYMDSCR,  'yyyy/MM/dd')  END AS LEASEENDYMDSCR")
        sqlStat.AppendLine("     ,TKI.CANCELFLG")
        sqlStat.AppendLine("     ,CASE TKI.LEASEENDYMD   WHEN '1900/01/01' THEN '' ELSE FORMAT(TKI.LEASEENDYMD,  'yyyy/MM/dd') END AS LEASEENDYMD")
        sqlStat.AppendLine("     ,TKI.DEPOTOUT")
        sqlStat.AppendLine("     ,ISNULL(DPO.NAMES,'') AS DEPOTOUTNAME")
        sqlStat.AppendLine("     ,TKI.DEPOTIN")
        sqlStat.AppendLine("     ,ISNULL(DPI.NAMES,'') AS DEPOTINNAME")
        sqlStat.AppendLine("     ,TKI.PAYSTDAILY")
        sqlStat.AppendLine("     ,TKI.PAYENDDAILY")
        sqlStat.AppendLine("     ,TKI.REMARK")
        sqlStat.AppendLine("     ,TKI.DELFLG")
        sqlStat.AppendLine("     ,ISNULL(CONVERT(nvarchar, TKI.INITYMD , 120),'') AS INITYMD")
        sqlStat.AppendLine("     ,ISNULL(CONVERT(nvarchar, TKI.UPDYMD , 120),'')  AS UPDYMD")
        sqlStat.AppendLine("     ,ISNULL(RTRIM(TKI.UPDUSER),'')                   AS UPDUSER")
        sqlStat.AppendLine("     ,ISNULL(RTRIM(TKI.UPDTERMID),'')                 AS UPDTERMID")
        'sqlStat.AppendLine("     ,TKI.STATUS                                      AS STATUS")
        sqlStat.AppendFormat("  FROM {0} TKI", CONST_TBL_TANK)
        sqlStat.AppendLine("    LEFT JOIN GBM0003_DEPOT DPO")
        sqlStat.AppendLine("      ON  TKI.DEPOTOUT = DPO.DEPOTCODE ")
        sqlStat.AppendLine("     AND  DPO.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  DPO.STYMD       <= @NOWDATE")
        sqlStat.AppendLine("     AND  DPO.ENDYMD      >= @NOWDATE")
        sqlStat.AppendLine("     AND  DPO.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("    LEFT JOIN GBM0003_DEPOT DPI")
        sqlStat.AppendLine("      ON  TKI.DEPOTIN = DPI.DEPOTCODE ")
        sqlStat.AppendLine("     AND  DPI.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  DPI.STYMD       <= @NOWDATE")
        sqlStat.AppendLine("     AND  DPI.ENDYMD      >= @NOWDATE")
        sqlStat.AppendLine("     AND  DPI.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   WHERE TKI.CONTRACTNO  = @CONTRACTNO")
        sqlStat.AppendLine("     AND TKI.AGREEMENTNO = @AGREEMENTNO")
        sqlStat.AppendLine("     AND TKI.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("     AND TKI.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("     AND TKI.DELFLG     <> @DELFLG")
        If tankNo <> "" Then
            sqlStat.AppendLine("     AND TKI.TANKNO = @TANKNO")
        End If
        sqlStat.AppendLine(" ) TBL")
        sqlStat.AppendLine(" ORDER BY TBL.TANKNO")
        Dim dtDbResult As New DataTable
        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = contractNo
                .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = agreementNo
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                .Add("@TANKNO", SqlDbType.NVarChar).Value = tankNo
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
    ''' デポ一覧を取得
    ''' </summary>
    ''' <param name="depotCode">デポコード</param>
    ''' <returns>取得結果のデータテーブル</returns>
    ''' <remarks>GBM0003_DEPOTテーブルよりデポ一覧を取得</remarks>
    Private Function GetDepot(Optional depotCode As String = "") As DataTable
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
        sqlStat.AppendLine("  FROM GBM0003_DEPOT")
        sqlStat.AppendLine(" WHERE COMPCODE     = @COMPCODE")
        If depotCode <> "" Then
            sqlStat.AppendLine("   And DEPOTCODE    = @DEPOTCODE")
        End If
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY DEPOTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DEPOTCODE", SqlDbType.NVarChar).Value = depotCode
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
        Dim dispDs As DataSet = CommonFunctions.DeepCopy(Me.DsDisDisplayValues)
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
        dicModCheck.Add(CONST_DT_NAME_AGREEMENT,
                        New List(Of String) From {"LEASETERM", "LEASETYPE", "PRODUCTCODE",
                                                  "LEASEPAYMENTTYPE", "AUTOEXTENDKIND",
                                                  "LEASEPAYMENTS", "RELEASE", "CURRENCY",
                                                  "AUTOEXTEND", "AUTOEXTENDKIND",
                                                  "LEASEPAYMENTKIND", "TAXKIND",
                                                  "TAXRATE", "REMARK"})
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
        'タンク情報の比較(純粋な行位置で判定できないためTANKNOでマッチングさせ比較
        With Nothing
            Dim dispTankRowCnt As Integer = 0
            Dim prevTankDtRowCnt As Integer = 0
            Dim dispTankDt As DataTable = dispDs.Tables(CONST_DT_NAME_TANKINFO)
            Dim prevTankDt As DataTable = prevDs.Tables(CONST_DT_NAME_TANKINFO)

            If dispTankDt IsNot Nothing Then
                dispTankRowCnt = dispTankDt.Rows.Count
            End If
            If prevTankDt IsNot Nothing Then
                prevTankDtRowCnt = prevTankDt.Rows.Count
            End If
            If prevTankDtRowCnt <> dispTankRowCnt Then
                '行数が合わない場合は追加・削除での増減あり
                Return True
            End If

            For Each dispDr As DataRow In dispTankDt.Rows
                Dim tankNo As String = Convert.ToString(dispDr("TANKNO"))
                Dim qPrevDr = From item In prevTankDt Where item("TANKNO").Equals(tankNo)

                If qPrevDr.Any = False Then
                    '片側に存在しない場合は変更を行ったため変化あり判定
                    Return True
                End If
                Dim prevDr = qPrevDr.FirstOrDefault
                '設定値の比較
                For Each fieldName As String In {"LEASESTYMD", "LEASEENDYMDSCR", "CANCELFLG",
                                                 "LEASEENDYMD", "PAYSTDAILY", "PAYENDDAILY", "REMARK",
                                                 "DEPOTIN"}
                    If Not dispDr(fieldName).Equals(prevDr(fieldName)) Then
                        Return True
                    End If
                Next fieldName
            Next dispDr
        End With

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
        dtAttachment = CommonFunctions.UploadAttachmentFile(dtAttachment, Me.GBT00020RValues.AgreementNo, CONST_MAPID)
        ds.Tables.Remove(C_DTNAME_ATTACHMENT)
        ds.Tables.Add(dtAttachment)
        repAttachment.DataSource = dtAttachment
        repAttachment.DataBind()
        Me.DsDisDisplayValues = ds
    End Sub
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
    ''' 画面チェックボックス情報保持
    ''' </summary>
    Private Sub SaveDisplayTankList()
        Dim ds As DataSet = DirectCast(ViewState(CONST_VS_NAME_CURRENT_VAL), DataSet)
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        If dtTankInfo.Rows.Count = 0 OrElse repTankInfo.Items Is Nothing OrElse repTankInfo.Items.Count = 0 Then
            Return
        End If
        For Each repItem As RepeaterItem In repTankInfo.Items
            Dim lblLineCntObj As Label = DirectCast(repItem.FindControl("lblLineCnt"), Label)
            Dim chkToOrder As CheckBox = DirectCast(repItem.FindControl("chkToOrder"), CheckBox)

            Dim targetRow = (From rowItem In dtTankInfo Where Convert.ToString(rowItem("LINECNT")) = lblLineCntObj.Text)
            If targetRow.Any = True Then
                With targetRow(0)
                    If chkToOrder.Checked Then
                        .Item("TOORDER") = "1"
                    Else
                        .Item("TOORDER") = ""
                    End If
                End With
            End If
        Next
        ViewState(CONST_VS_NAME_CURRENT_VAL) = ds

    End Sub
    ''' <summary>
    ''' タンク一覧画面へ遷移する
    ''' </summary>
    ''' <returns></returns>
    Private Function OpenTankList() As String
        Me.GBT00020AGREEMENTValues = New GBT00020AGREEMENT.GBT0020AGREEMENTDispItem
        Dim ds As DataSet = Me.DsDisDisplayValues
        Dim prevDs As DataSet = DirectCast(ViewState(CONST_VS_NAME_PREV_VAL), DataSet)
        Me.GBT00020AGREEMENTValues.DispDs = ds
        Me.GBT00020AGREEMENTValues.PrevDispDs = prevDs
        Me.GBT00020AGREEMENTValues.PrevDispItem = Me.GBT00020RValues
        Me.GBT00020AGREEMENTValues.MapVari = Me.hdnThisMapVariant.Value
        Me.GBT00020AGREEMENTValues.OrderStartPoint = Me.ddlOrderStart.SelectedValue
        '**************************************************
        'タンクステータス画面へ遷移
        '**************************************************
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ 画面遷移先URL取得 ■■■
        COA0012DoUrl.MAPIDP = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "GB_LTankSelect"
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0012DoUrl.ERR
        End If

        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
        Return C_MESSAGENO.NORMAL
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
    ''' 画面上の初期値データテーブルを生成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateContractDt() As DataTable
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
            .Add("REMARK", GetType(String)).DefaultValue = ""
            .Add("NOOFAGREEMENT", GetType(String)).DefaultValue = ""
            .Add("INITYMD", GetType(String)).DefaultValue = ""
            .Add("UPDYMD", GetType(String)).DefaultValue = ""
            .Add("UPDUSER", GetType(String)).DefaultValue = ""
            .Add("UPDTERMID", GetType(String)).DefaultValue = ""
        End With
        Return retDt
    End Function
    ''' <summary>
    ''' 申請処理
    ''' </summary>
    ''' <returns>メッセージNo</returns>
    Private Function ApplyProc(dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#) As String
        Dim canCloseConnect As Boolean = False

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Dim applyId As String = ""
        Dim lastStep As String = ""
        '申請ID取得
        Dim GBA00011ApplyID As New GBA00011ApplyID
        GBA00011ApplyID.COMPCODE = GBC_COMPCODE_D
        GBA00011ApplyID.SYSCODE = COA0019Session.SYSCODE
        GBA00011ApplyID.KEYCODE = COA0019Session.APSRVname
        GBA00011ApplyID.DIVISION = "L"
        GBA00011ApplyID.SEQOBJID = C_SQLSEQ.LEASEAGREEMENTAPPLY
        GBA00011ApplyID.SEQLEN = 6
        GBA00011ApplyID.GBA00011getApplyID()
        If GBA00011ApplyID.ERR = C_MESSAGENO.NORMAL Then
            applyId = GBA00011ApplyID.APPLYID
        Else
            Return GBA00011ApplyID.ERR
        End If

        '申請登録
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
        COA0032Apploval.I_APPLYID = applyId
        COA0032Apploval.I_MAPID = CONST_MAPID
        COA0032Apploval.I_EVENTCODE = C_LEASEEVENT.APPLY
        COA0032Apploval.I_SUBCODE = ""
        COA0032Apploval.COA0032setApply()
        If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
            lastStep = COA0032Apploval.O_LASTSTEP
        Else
            Return COA0032Apploval.O_ERR
        End If
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            '申請情報更新SQL
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("INSERT INTO {0} ", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine(" (")
            sqlStat.AppendLine("           CONTRACTNO")
            sqlStat.AppendLine("          ,AGREEMENTNO")
            sqlStat.AppendLine("          ,STYMD")
            sqlStat.AppendLine("          ,ENDYMD")
            sqlStat.AppendLine("          ,LEASETERM")
            sqlStat.AppendLine("          ,LEASETYPE")
            sqlStat.AppendLine("          ,PRODUCTCODE")
            sqlStat.AppendLine("          ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("          ,AUTOEXTEND")
            sqlStat.AppendLine("          ,AUTOEXTENDKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTS")
            sqlStat.AppendLine("          ,RELEASE")
            sqlStat.AppendLine("          ,CURRENCY")
            sqlStat.AppendLine("          ,TAXKIND")
            sqlStat.AppendLine("          ,TAXRATE")
            sqlStat.AppendLine("          ,REMARK")
            sqlStat.AppendLine("          ,APPLYID")
            sqlStat.AppendLine("          ,APPLYTEXT")
            sqlStat.AppendLine("          ,LASTSTEP")
            sqlStat.AppendLine("          ,RELATEDORDERNO")
            sqlStat.AppendLine("          ,DELFLG")
            sqlStat.AppendLine("          ,INITYMD")
            sqlStat.AppendLine("          ,UPDYMD")
            sqlStat.AppendLine("          ,UPDUSER")
            sqlStat.AppendLine("          ,UPDTERMID")
            sqlStat.AppendLine("          ,RECEIVEYMD")
            sqlStat.AppendLine(" )   SELECT ")
            sqlStat.AppendLine("           CONTRACTNO")
            sqlStat.AppendLine("          ,AGREEMENTNO")
            sqlStat.AppendLine("          ,STYMD")
            sqlStat.AppendLine("          ,ENDYMD")
            sqlStat.AppendLine("          ,LEASETERM")
            sqlStat.AppendLine("          ,LEASETYPE")
            sqlStat.AppendLine("          ,PRODUCTCODE")
            sqlStat.AppendLine("          ,LEASEPAYMENTTYPE")
            sqlStat.AppendLine("          ,AUTOEXTEND")
            sqlStat.AppendLine("          ,AUTOEXTENDKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTKIND")
            sqlStat.AppendLine("          ,LEASEPAYMENTS")
            sqlStat.AppendLine("          ,RELEASE")
            sqlStat.AppendLine("          ,CURRENCY")
            sqlStat.AppendLine("          ,TAXKIND")
            sqlStat.AppendLine("          ,TAXRATE")
            sqlStat.AppendLine("          ,REMARK")
            sqlStat.AppendLine("          ,APPLYID")
            sqlStat.AppendLine("          ,APPLYTEXT")
            sqlStat.AppendLine("          ,LASTSTEP")
            sqlStat.AppendLine("          ,RELATEDORDERNO")
            sqlStat.AppendLine("          ,@DELFLG_YES")
            sqlStat.AppendLine("          ,@UPDYMD")
            sqlStat.AppendLine("          ,@UPDYMD")
            sqlStat.AppendLine("          ,@UPDUSER")
            sqlStat.AppendLine("          ,@UPDTERMID")
            sqlStat.AppendLine("          ,@RECEIVEYMD")
            sqlStat.AppendFormat("     FROM {0}", CONST_TBL_AGREEMENT)
            sqlStat.AppendLine("      WHERE CONTRACTNO  = @CONTRACTNO")
            sqlStat.AppendLine("        AND AGREEMENTNO = @AGREEMENTNO")
            sqlStat.AppendLine("        AND DELFLG      = @DELFLG")
            sqlStat.AppendLine(";")
            sqlStat.AppendFormat("UPDATE {0} ", CONST_TBL_AGREEMENT).AppendLine()
            sqlStat.AppendLine("   SET APPLYID    = @APPLYID")
            sqlStat.AppendLine("      ,APPLYTEXT  = @APPLYTEXT")
            sqlStat.AppendLine("      ,LASTSTEP   = @LASTSTEP")
            sqlStat.AppendLine("      ,UPDYMD     = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine("  WHERE CONTRACTNO  = @CONTRACTNO")
            sqlStat.AppendLine("    AND AGREEMENTNO = @AGREEMENTNO")
            sqlStat.AppendLine("    AND DELFLG      = @DELFLG")
            sqlStat.AppendLine(";")
            'DB接続
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CONTRACTNO", SqlDbType.NVarChar).Value = dr.Item("CONTRACTNO")
                    .Add("@AGREEMENTNO", SqlDbType.NVarChar).Value = dr.Item("AGREEMENTNO")
                    .Add("@STYMD", SqlDbType.Date).Value = procDate

                    .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                    .Add("@APPLYTEXT", SqlDbType.NVarChar).Value = dr.Item("APPLYTEXT")
                    .Add("@LASTSTEP", SqlDbType.NVarChar).Value = lastStep

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO

                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                    .Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With
                sqlCmd.ExecuteNonQuery()
            End Using
            'ここまでくれば正常終了
            Return C_MESSAGENO.NORMAL
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
        sqlStat.AppendLine("     , CTR.REMARK")
        sqlStat.AppendLine("     , (SELECT COUNT(AGR.AGREEMENTNO) ")
        sqlStat.AppendFormat("          FROM {0} AGR", CONST_TBL_AGREEMENT).AppendLine()
        sqlStat.AppendLine("         WHERE AGR.CONTRACTNO = CTR.CONTRACTNO")
        sqlStat.AppendLine("           AND AGR.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("           AND AGR.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("           AND AGR.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("       ) AS NOOFAGREEMENT")
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
    ''' 当画面の値引き渡しクラス
    ''' </summary>
    <Serializable>
    Public Class GBT0020AGREEMENTDispItem
        Public PrevDispItem As GBT00020RESULT.GBT00020RValues = Nothing
        Public DispDs As DataSet = Nothing
        Public PrevDispDs As DataSet = Nothing
        Public MapVari As String = ""
        Public OrderStartPoint As String = ""
        ''' <summary>
        ''' タンクステータスにて選択されたタンク一覧を保持
        ''' </summary>
        Public SelectedTankNo As List(Of String) = New List(Of String)
    End Class
End Class

