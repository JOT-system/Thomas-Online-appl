Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' ブレーカー承認検索画面クラス
''' </summary>
Public Class GBT00005SELECT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00005S"     '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00005A"
    Private returnCode As String = String.Empty           'サブ用リターンコード

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
            Dim COA0031ProfMap As New BASEDLL.COA0031ProfMap
            Dim COA0007getCompanyInfo As New BASEDLL.COA0007CompanyInfo

            HttpContext.Current.Session("MAPurl") = ""
            returnCode = C_MESSAGENO.NORMAL
            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            '****************************************
            'メッセージ初期化
            '****************************************
            lblFooterMessage.Text = ""

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                'セッション変数のMapVariantを退避
                Me.hdnMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'タイトル設定
                '****************************************
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnMapVariant.Value
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                End If

                '****************************************
                '初期表示
                '****************************************
                '検索設定の選択肢を取得
                SetBreakerTypeListItem()

                DefaultValueSet()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                txtStYMD.Focus()
                '****************************************
                'セッション設定
                '****************************************
                HttpContext.Current.Session(CONST_BASEID & "_START") = CONST_MAPID

            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then

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
                ' Help表示
                '**********************
                If Me.hdnHelpChange.Value IsNot Nothing AndAlso Me.hdnHelpChange.Value <> "" Then
                    DivShowHelp_DoubleClick(CONST_MAPID)
                    Me.hdnHelpChange.Value = ""
                End If
            End If

            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        Catch ex As Threading.ThreadAbortException

        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.EXCEPTION
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)

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
                '荷主コードビュー表示切替
                Case Me.vLeftShipper.ID
                    If Me.rblBreakerType.SelectedValue = "01SALES" Then
                        SetShipperListItem(Me.txtShipper.Text)
                    Else
                        SetAgentShipperListItem(Me.txtShipper.Text)
                    End If

                '荷受人コードビュー表示切替
                Case Me.vLeftConsignee.ID
                    If Me.rblBreakerType.SelectedValue = "01SALES" Then
                        SetConsigneeListItem(Me.txtConsignee.Text)
                    Else
                        SetAgentConsigneeListItem(Me.txtConsignee.Text)
                    End If

                'POL港コードビュー表示切替
                Case Me.vLeftPOLPort.ID
                    SetPOLPortListItem(Me.txtPOLPort.Text)

                'POD港コードビュー表示切替
                Case Me.vLeftPODPort.ID
                    SetPODPortListItem(Me.txtPODPort.Text)

                '積載品コードビュー表示切替
                Case Me.vLeftProduct.ID
                    SetProductListItem(Me.txtProduct.Text)

                'ブレーカーIDビュー表示切替
                Case Me.vLeftBreakerId.ID
                    If Me.rblBreakerType.SelectedValue = "01SALES" Then
                        SetBreakerIdListItem(C_BRTYPE.SALES, Me.txtBrId.Text)
                    Else
                        SetBreakerIdListItem(C_BRTYPE.OPERATION, Me.txtBrId.Text)
                    End If

                '承認ビュー表示切替
                Case Me.vLeftApproval.ID
                    SetApprovalListItem(Me.txtApproval.Text)

                '代理店コードビュー表示切替
                Case Me.vLeftOffice.ID
                    SetOfficeListItem(Me.txtOffice.Text)

                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Me.hdnCalendarValue.Value = FormatDateYMD(txtobj.Text, GBA00003UserSetting.DATEFORMAT)

                        Me.mvLeft.Focus()
                    End If

            End Select
        End If

    End Sub
    ''' <summary>
    ''' 実行ボタン押下時
    ''' </summary>
    Public Sub btnEnter_Click()
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl

        returnCode = C_MESSAGENO.NORMAL

        'チェック処理
        checkProc()
        If returnCode <> C_MESSAGENO.NORMAL Then
            Return
        End If

        '日付設定
        If Me.txtStYMD.Text <> "" AndAlso Me.txtEndYMD.Text = "" Then
            Me.txtEndYMD.Text = Me.txtStYMD.Text
        End If

        If Me.txtEndYMD.Text <> "" AndAlso Me.txtStYMD.Text = "" Then
            Me.txtStYMD.Text = Me.txtEndYMD.Text
        End If

        '画面遷移先URL取得
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = Me.hdnMapVariant.Value
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If

        HttpContext.Current.Session("MAPvariant") = Me.hdnMapVariant.Value

        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

    End Sub
    ''' <summary>
    ''' 終了ボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '画面戻先URL取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
            lblTitleText.Text = COA0011ReturnUrl.NAMES
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
    ''' 左ボックス選択ボタン押下時
    ''' </summary>
    Public Sub btnLeftBoxButtonSel_Click()
        Dim targetObject As Control = Nothing
        '現在表示している左ビューを取得
        Dim activeViewObj As View = Me.mvLeft.GetActiveView
        If activeViewObj IsNot Nothing Then
            Select Case activeViewObj.ID
                'ビューごとの処理はケースを追加で実現
                Case Me.vLeftShipper.ID 'アクティブなビューが荷主コード
                    '荷主コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbShipper.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbShipper.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbShipper.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblShipperText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblShipperText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftConsignee.ID 'アクティブなビューが荷受人コード
                    '荷受人コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbConsignee.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbConsignee.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbConsignee.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblConsigneeText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblConsigneeText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftPOLPort.ID 'アクティブなビューがPOL港コード
                    'POL港コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbPOLPort.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbPOLPort.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbPOLPort.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblPOLPortText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblPOLPortText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftPODPort.ID 'アクティブなビューがPOD港コード
                    'POD港コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbPODPort.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbPODPort.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbPODPort.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblPODPortText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblPODPortText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftProduct.ID 'アクティブなビューが積載品
                    '積載品選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbProduct.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbProduct.SelectedItem.Value
                            Me.lblProductText.Text = Me.lbProduct.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblProductText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftBreakerId.ID 'アクティブなビューがBreakerId
                    'BreakerId選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbBreakerId.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbBreakerId.SelectedItem.Value
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
                            txtobj.Text = Me.lbApproval.SelectedItem.Value
                            Me.lblApprovalText.Text = Me.lbApproval.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblApprovalText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftOffice.ID 'アクティブなビューが代理店コード
                    '代理店コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbOffice.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbOffice.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbOffice.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblOfficeText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblOfficeText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftCal.ID 'アクティブなビューがカレンダー
                    'カレンダー選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        txtobj.Text = Me.hdnCalendarValue.Value
                        txtobj.Focus()
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

        'ラベル等やグリッドを除く文言設定(適宜追加) リピーターの表ヘッダーもこの方式で可能ですので
        '作成者に聞いてください。
        AddLangSetting(dicDisplayText, Me.btnEnter, "実行", "Search")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.lblYMD1, "ブレーカー有効期限", "VALIDITY")
        AddLangSetting(dicDisplayText, Me.lblYMD2, "範囲指定", "From")
        AddLangSetting(dicDisplayText, Me.lblTilde, "～", "To")
        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主コード", "Shipper Code")
        AddLangSetting(dicDisplayText, Me.lblConsignee, "荷受人コード", "Consignee Code")
        AddLangSetting(dicDisplayText, Me.lblPOLPort, "POL港コード", "POL Port")
        AddLangSetting(dicDisplayText, Me.lblPODPort, "POD港コード", "POD Port")
        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品コード", "Product Code")
        AddLangSetting(dicDisplayText, Me.lblBrId, "ブレーカーID", "Breaker ID")
        AddLangSetting(dicDisplayText, Me.lblApproval, "承認", "Approval")
        AddLangSetting(dicDisplayText, Me.lblOffice, "代理店コード", "Office Code")
        AddLangSetting(dicDisplayText, Me.lblBreakerType, "ブレーカータイプ", "Breaker Type")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub

    ''' <summary>
    ''' 初期表示
    ''' </summary>
    Public Sub DefaultValueSet()
        'RightBox情報設定
        rightBoxSet()
        If returnCode <> C_MESSAGENO.NORMAL Then
            Return
        End If
        '選択画面の入力初期値設定
        'メニューから遷移/業務画面戻り判定
        If Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00005APPROVAL Then

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00005APPROVAL Then
            Dim prevPage As GBT00005APPROVAL = DirectCast(Page.PreviousPage, GBT00005APPROVAL)
            '一覧画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            '前画面と当画面のテキストボックス関連ディクショナリ
            Dim dicObjs As New Dictionary(Of String, TextBox) From {{"hdnStYMD", Me.txtStYMD},
                                                                    {"hdnEndYMD", Me.txtEndYMD},
                                                                    {"hdnShipper", Me.txtShipper},
                                                                    {"hdnConsignee", Me.txtConsignee},
                                                                    {"hdnPOLPort", Me.txtPOLPort},
                                                                    {"hdnPODPort", Me.txtPODPort},
                                                                    {"hdnProduct", Me.txtProduct},
                                                                    {"hdnBrId", Me.txtBrId},
                                                                    {"hdnApproval", Me.txtApproval},
                                                                    {"hdnOffice", Me.txtOffice}}

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, TextBox) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    If dicObj.Key = "hdnStYMD" OrElse dicObj.Key = "hdnEndYMD" Then
                        dicObj.Value.Text = FormatDateContrySettings(tmphdnObj.Value, GBA00003UserSetting.DATEFORMAT)
                    Else
                        dicObj.Value.Text = tmphdnObj.Value
                    End If
                End If
            Next
            Dim objSearchType As Control = prevPage.FindControl("hdnSearchBreakerType")
            If objSearchType IsNot Nothing Then
                Dim hdnBreakerType As HiddenField = DirectCast(objSearchType, HiddenField)
                If Me.rblBreakerType.Items.FindByValue(hdnBreakerType.Value) IsNot Nothing Then
                    Me.rblBreakerType.SelectedValue = hdnBreakerType.Value
                End If
            End If

            Dim tmpHdn As HiddenField = DirectCast(prevPage.FindControl("hdnPrevViewID"), HiddenField)
            If tmpHdn IsNot Nothing AndAlso
                Me.lbRightList.Items.FindByValue(tmpHdn.Value) IsNot Nothing Then
                Me.lbRightList.SelectedValue = tmpHdn.Value
            End If
        End If
        'コードを元に名称を設定
        '荷主コード　
        txtShipper_Change()
        '荷受人コード　
        txtConsignee_Change()
        'POL港コード　
        txtPOLPort_Change()
        'POD港コード　
        txtPODPort_Change()
        '積載品コード
        txtProduct_Change()
        '承認
        txtApproval_Change()
        '代理店コード
        txtOffice_Change()
        'RightBox情報設定
        rightBoxSet()
        If returnCode <> C_MESSAGENO.NORMAL Then
            Return
        End If

    End Sub
    ''' <summary>
    ''' 変数設定
    ''' </summary>
    Public Sub variableSet()

        Dim COA0016VARIget As New BASEDLL.COA0016VARIget        '変数情報取
        '初期値を設定するディクショナリ後続のループで使用
        'KEY：COS0014_PROFVARIのFIELDで引き当てるキー、VALUE:初期値を設定するテキストボックスオブジェクト
        Dim dicDefaultValueSettings As New Dictionary(Of String, TextBox) _
                        From {{"STYMD", Me.txtStYMD}, {"ENDYMD", Me.txtEndYMD},
                              {"SHIPPER", Me.txtShipper}, {"CONSIGNEE", Me.txtConsignee},
                              {"POLPORT", Me.txtPOLPort}, {"PODPORT", Me.txtPODPort},
                              {"PRODUCT", Me.txtProduct}, {"BRID", Me.txtBrId},
                              {"APPROVAL", Me.txtApproval}, {"OFFICE", Me.txtOffice}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                If item.Key = "STYMD" OrElse item.Key = "ENDYMD" Then
                    item.Value.Text = FormatDateContrySettings(COA0016VARIget.VALUE, GBA00003UserSetting.DATEFORMAT)
                Else
                    item.Value.Text = COA0016VARIget.VALUE
                End If
            Else
                CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
                Return
            End If
        Next

        'ラジオボタンの初期値設定
        COA0016VARIget.FIELD = "BREAKERTYPE"
        COA0016VARIget.COA0016VARIget()
        If Me.rblBreakerType.Items.FindByValue(COA0016VARIget.VALUE) IsNot Nothing Then
            Me.rblBreakerType.SelectedValue = COA0016VARIget.VALUE
        End If
        'OFFICEは動的の為個別セット
        SetOfficeListItem("")
        If Me.lbOffice.Items.FindByValue(GBA00003UserSetting.OFFICECODE) IsNot Nothing AndAlso GBA00003UserSetting.IS_JOTUSER = False Then
            Me.txtOffice.Text = GBA00003UserSetting.OFFICECODE
        Else
            Me.txtOffice.Text = ""
        End If

    End Sub
    ''' <summary>
    ''' 右ボックス設定
    ''' </summary>
    Public Sub rightBoxSet()
        Dim COA0018ViewList As New BASEDLL.COA0018ViewList          '変数情報取
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget            '変数情報取

        'RightBOX情報設定
        '画面レイアウト情報
        COA0018ViewList.MAPID = CONST_BASEID
        COA0018ViewList.VIEW = lbRightList
        COA0018ViewList.COA0018getViewList()
        If COA0018ViewList.ERR = C_MESSAGENO.NORMAL Then
            Try
                For i As Integer = 0 To DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items.Count - 1
                    lbRightList.Items.Add(New ListItem(DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items(i).Text, DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items(i).Value))
                Next
            Catch ex As Exception
            End Try
        Else
            CommonFunctions.ShowMessage(COA0018ViewList.ERR, Me.lblFooterMessage)
            returnCode = COA0018ViewList.ERR
            Return
        End If

        'ビューID変数検索
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnMapVariant.Value
        COA0016VARIget.FIELD = "VIEWID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            returnCode = COA0016VARIget.ERR
            Return
        End If

        'ListBox選択
        lbRightList.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To lbRightList.Items.Count - 1
            If lbRightList.Items(i).Value = COA0016VARIget.VALUE Then
                lbRightList.SelectedIndex = i
            End If
        Next

    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    Public Sub checkProc()

        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get
        '入力文字置き換え
        '画面PassWord内の使用禁止文字排除

        '有効開始日
        COA0008InvalidChar.CHARin = txtStYMD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtStYMD.Text = COA0008InvalidChar.CHARout
        End If

        '有効終了日
        COA0008InvalidChar.CHARin = txtEndYMD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtEndYMD.Text = COA0008InvalidChar.CHARout
        End If

        '荷主コード
        COA0008InvalidChar.CHARin = txtShipper.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtShipper.Text = COA0008InvalidChar.CHARout
        End If

        '荷受人コード
        COA0008InvalidChar.CHARin = txtConsignee.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtConsignee.Text = COA0008InvalidChar.CHARout
        End If

        '積載品コード
        COA0008InvalidChar.CHARin = txtProduct.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtProduct.Text = COA0008InvalidChar.CHARout
        End If

        'POL港コード
        COA0008InvalidChar.CHARin = txtPOLPort.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtPOLPort.Text = COA0008InvalidChar.CHARout
        End If

        'POL港コード
        COA0008InvalidChar.CHARin = txtPODPort.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtPODPort.Text = COA0008InvalidChar.CHARout
        End If

        'ブレーカーID
        COA0008InvalidChar.CHARin = txtBrId.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtBrId.Text = COA0008InvalidChar.CHARout
        End If

        '承認
        COA0008InvalidChar.CHARin = txtApproval.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtApproval.Text = COA0008InvalidChar.CHARout
        End If

        '代理店コード
        COA0008InvalidChar.CHARin = txtOffice.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtOffice.Text = COA0008InvalidChar.CHARout
        End If

        '入力項目チェック

        '有効開始日 単項目チェック
        CheckSingle("STYMD", FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT))
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtStYMD.Focus()
            Return
        End If

        '有効終了日 単項目チェック
        CheckSingle("ENDYMD", FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT))
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtEndYMD.Focus()
            Return
        End If

        '日付整合性チェック
        CheckDate(FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT), FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT))
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtStYMD.Focus()
            Return
        End If

        '荷主コード 単項目チェック
        CheckSingle("SHIPPER", txtShipper.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtShipper.Focus()
            Return
        End If

        '荷主コード List存在チェック
        If Me.rblBreakerType.SelectedValue = "01SALES" Then
            SetShipperListItem(Me.txtShipper.Text)
        Else
            SetAgentShipperListItem(Me.txtShipper.Text)
        End If
        CheckList(txtShipper.Text, lbShipper)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtShipper.Focus()
            Return
        End If

        '荷受人コード 単項目チェック
        CheckSingle("CONSIGNEE", txtConsignee.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtConsignee.Focus()
            Return
        End If

        '荷受人コード List存在チェック
        If Me.rblBreakerType.SelectedValue = "01SALES" Then
            SetConsigneeListItem(Me.txtConsignee.Text)
        Else
            SetAgentConsigneeListItem(Me.txtConsignee.Text)
        End If
        CheckList(txtConsignee.Text, lbConsignee)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtConsignee.Focus()
            Return
        End If

        '積載品コード 単項目チェック
        CheckSingle("PRODUCT", txtProduct.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtProduct.Focus()
            Return
        End If

        '積載品コード List存在チェック
        CheckList(txtProduct.Text, lbProduct)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtProduct.Focus()
            Return
        End If

        'POL港コード 単項目チェック
        CheckSingle("POLPORT", txtPOLPort.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPOLPort.Focus()
            Return
        End If

        'POL港コード List存在チェック
        CheckList(txtPOLPort.Text, lbPOLPort)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPOLPort.Focus()
            Return
        End If

        'POD港コード 単項目チェック
        CheckSingle("PODPORT", txtPODPort.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPODPort.Focus()
            Return
        End If

        'POD港コード List存在チェック
        CheckList(txtPODPort.Text, lbPODPort)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPODPort.Focus()
            Return
        End If

        'ブレーカーID 単項目チェック
        CheckSingle("BRID", txtBrId.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtBrId.Focus()
            Return
        End If

        'ブレーカーID List存在チェック
        If Me.rblBreakerType.SelectedValue = "01SALES" Then
            SetBreakerIdListItem(C_BRTYPE.SALES, Me.txtBrId.Text)
        Else
            SetBreakerIdListItem(C_BRTYPE.OPERATION, Me.txtBrId.Text)
        End If
        CheckList(txtBrId.Text, lbBreakerId)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtBrId.Focus()
            Return
        End If

        '承認 単項目チェック
        CheckSingle("APPROVAL", txtApproval.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtApproval.Focus()
            Return
        End If

        '承認 List存在チェック
        CheckList(txtApproval.Text, lbApproval)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtApproval.Focus()
            Return
        End If

        '代理店コード 単項目チェック
        CheckSingle("OFFICE", txtOffice.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtOffice.Focus()
            Return
        End If

        '代理店コード List存在チェック
        CheckList(txtOffice.Text, lbOffice)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtOffice.Focus()
            Return
        End If

    End Sub
    ''' <summary>
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="inColName"></param>
    ''' <param name="inText"></param>
    Protected Sub CheckSingle(ByVal inColName As String, ByVal inText As String)

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck      '項目チェック

        '〇単項目チェック
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELD = inColName
        COA0026FieldCheck.VALUE = inText
        COA0026FieldCheck.COA0026FieldCheck()
        If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
            returnCode = COA0026FieldCheck.ERR
        End If

    End Sub
    ''' <summary>
    ''' LIST登録チェック
    ''' </summary>
    ''' <param name="inText"></param>
    ''' <param name="inList"></param>
    Protected Sub CheckList(ByVal inText As String, ByVal inList As ListBox)

        Dim flag As Boolean = False

        If inText <> "" Then

            For i As Integer = 0 To inList.Items.Count - 1
                If inList.Items(i).Value = inText Then
                    flag = True
                    Exit For
                End If
            Next

            If (flag = False) Then
                returnCode = C_MESSAGENO.INVALIDINPUT
                CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
            End If
        End If
    End Sub

    ''' <summary>
    ''' 日付整合性チェック
    ''' </summary>
    ''' <param name="inStYMD"></param>
    ''' <param name="inEndYMD"></param>
    Protected Sub CheckDate(ByVal inStYMD As String, ByVal inEndYMD As String)

        If inStYMD = "" OrElse inEndYMD = "" Then
            Return
        End If
        Dim wkDateStart As Date = Nothing
        Dim wkDateEnd As Date = Nothing
        Date.TryParse(inStYMD, wkDateStart)
        Date.TryParse(inEndYMD, wkDateEnd)

        If wkDateStart > wkDateEnd Then

            returnCode = C_MESSAGENO.VALIDITYINPUT
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)

        End If

    End Sub
    ''' <summary>
    ''' 荷主コードリストアイテムを設定
    ''' </summary>
    Private Sub SetShipperListItem(selectedValue As String)

        Dim GBA00004CountryRelated As New GBA00004CountryRelated

        Try
            'リストクリア
            Me.lbShipper.Items.Clear()

            GBA00004CountryRelated.LISTBOX_SHIPPER = Me.lbShipper
            GBA00004CountryRelated.GBA00004getLeftListShipper()
            If GBA00004CountryRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00004CountryRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbShipper = DirectCast(GBA00004CountryRelated.LISTBOX_SHIPPER, ListBox)
            Else
                returnCode = GBA00004CountryRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbShipper.Items.Count > 0 Then
                Dim findListItem = Me.lbShipper.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' AgentShipperリストアイテムを設定
    ''' </summary>
    Private Sub SetAgentShipperListItem(selectedValue As String)

        Dim GBA00004CountryRelated As New GBA00004CountryRelated

        Try
            'リストクリア
            Me.lbShipper.Items.Clear()

            GBA00004CountryRelated.LISTBOX_AGENT = Me.lbShipper
            GBA00004CountryRelated.GBA00004getLeftListAgent()
            If GBA00004CountryRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00004CountryRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbShipper = DirectCast(GBA00004CountryRelated.LISTBOX_AGENT, ListBox)
            Else
                returnCode = GBA00004CountryRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbShipper.Items.Count > 0 Then
                Dim findListItem = Me.lbShipper.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub

    ''' <summary>
    ''' AgentConsigneeリストアイテムを設定
    ''' </summary>
    Private Sub SetAgentConsigneeListItem(selectedValue As String)

        Dim GBA00004CountryRelated As New GBA00004CountryRelated

        Try
            'リストクリア
            Me.lbConsignee.Items.Clear()

            GBA00004CountryRelated.LISTBOX_AGENT = Me.lbConsignee
            GBA00004CountryRelated.GBA00004getLeftListAgent()
            If GBA00004CountryRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00004CountryRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbConsignee = DirectCast(GBA00004CountryRelated.LISTBOX_AGENT, ListBox)
            Else
                returnCode = GBA00004CountryRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbConsignee.Items.Count > 0 Then
                Dim findListItem = Me.lbConsignee.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 荷受人コードリストアイテムを設定
    ''' </summary>
    Private Sub SetConsigneeListItem(selectedValue As String)

        Dim GBA00004CountryRelated As New GBA00004CountryRelated

        Try
            'リストクリア
            Me.lbConsignee.Items.Clear()

            GBA00004CountryRelated.LISTBOX_CONSIGNEE = Me.lbConsignee
            GBA00004CountryRelated.GBA00004getLeftListConsignee()
            If GBA00004CountryRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00004CountryRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbConsignee = DirectCast(GBA00004CountryRelated.LISTBOX_CONSIGNEE, ListBox)
            Else
                returnCode = GBA00004CountryRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbConsignee.Items.Count > 0 Then
                Dim findListItem = Me.lbConsignee.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub

    ''' <summary>
    ''' 積載品コードリストアイテムを設定
    ''' </summary>
    Private Sub SetProductListItem(selectedValue As String)
        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        Try

            'リストクリア
            Me.lbProduct.Items.Clear()

            'DataBase接続(Open)
            SQLcon.Open()

            '検索SQL文
            SQLStr =
                 "SELECT PRODUCTCODE , PRODUCTNAME  " _
               & " FROM  GBM0008_PRODUCT " _
               & " Where COMPCODE = @P1 " _
               & "   and STYMD   <= @P2 " _
               & "   and ENDYMD  >= @P3 " _
               & "   and DELFLG  <> @P4 "
            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
            PARA1.Value = GBC_COMPCODE
            PARA2.Value = Date.Now
            PARA3.Value = Date.Now
            PARA4.Value = BaseDllCommon.CONST_FLAG_YES
            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                'DBからアイテムを設定
                Me.lbProduct.Items.Add(New ListItem(Convert.ToString(SQLdr("PRODUCTNAME")), Convert.ToString(SQLdr("PRODUCTCODE"))))
            End While

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbProduct.Items.Count > 0 Then
                Dim findListItem = Me.lbProduct.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        Finally
            'CLOSE
            If Not SQLdr Is Nothing Then
                SQLdr.Close()
            End If
            If Not SQLcmd Is Nothing Then
                SQLcmd.Dispose()
                SQLcmd = Nothing
            End If
            If Not SQLcon Is Nothing Then
                SQLcon.Close()
                SQLcon.Dispose()
                SQLcon = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' POL港コードリストアイテムを設定
    ''' </summary>
    Private Sub SetPOLPortListItem(selectedValue As String)

        Dim GBA00007OrganizationRelated As New GBA00007OrganizationRelated

        Try
            'リストクリア
            Me.lbPOLPort.Items.Clear()

            GBA00007OrganizationRelated.LISTBOX_PORT = Me.lbPOLPort
            GBA00007OrganizationRelated.GBA00007getLeftListPort()
            If GBA00007OrganizationRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00007OrganizationRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbPOLPort = DirectCast(GBA00007OrganizationRelated.LISTBOX_PORT, ListBox)
            Else
                returnCode = GBA00007OrganizationRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPOLPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPOLPort.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub

    ''' <summary>
    ''' POD港コードリストアイテムを設定
    ''' </summary>
    Private Sub SetPODPortListItem(selectedValue As String)

        Dim GBA00007OrganizationRelated As New GBA00007OrganizationRelated

        Try
            'リストクリア
            Me.lbPODPort.Items.Clear()

            GBA00007OrganizationRelated.LISTBOX_PORT = Me.lbPODPort
            GBA00007OrganizationRelated.GBA00007getLeftListPort()
            If GBA00007OrganizationRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00007OrganizationRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbPODPort = DirectCast(GBA00007OrganizationRelated.LISTBOX_PORT, ListBox)
            Else
                returnCode = GBA00007OrganizationRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPODPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPODPort.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' ブレーカーID
    ''' </summary>
    Private Sub SetBreakerIdListItem(ByVal brType As String, ByVal selectedValue As String)
        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        Try

            'リストクリア
            Me.lbBreakerId.Items.Clear()

            'DataBase接続(Open)
            SQLcon.Open()

            '検索SQL文
            SQLStr =
                 "SELECT BRID  " _
               & " FROM  GBT0001_BR_INFO " _
               & " Where BRTYPE   = @P1 " _
               & "   and STYMD   <= @P2 " _
               & "   and ENDYMD  >= @P3 " _
               & "   and DELFLG  <> @P4 " _
               & " Order By BRID DESC "
            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.NVarChar)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
            PARA1.Value = brType
            PARA2.Value = Date.Now
            PARA3.Value = Date.Now
            PARA4.Value = BaseDllCommon.CONST_FLAG_YES
            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                'DBからアイテムを設定
                Me.lbBreakerId.Items.Add(New ListItem(Convert.ToString(SQLdr("BRID"))))
            End While

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbBreakerId.Items.Count > 0 Then
                Dim findListItem = Me.lbBreakerId.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        Finally
            'CLOSE
            If Not SQLdr Is Nothing Then
                SQLdr.Close()
            End If
            If Not SQLcmd Is Nothing Then
                SQLcmd.Dispose()
                SQLcmd = Nothing
            End If
            If Not SQLcon Is Nothing Then
                SQLcon.Close()
                SQLcon.Dispose()
                SQLcon = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 承認リストアイテムを設定
    ''' </summary>
    Private Sub SetApprovalListItem(selectedValue As String)

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

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
            returnCode = COA0017FixValue.ERR
            Return
        End If

    End Sub
    ''' <summary>
    ''' 代理店コードリストアイテムを設定
    ''' </summary>
    Private Sub SetOfficeListItem(selectedValue As String)

        Dim GBA00007OrganizationRelated As New GBA00007OrganizationRelated

        Try
            'リストクリア
            Me.lbOffice.Items.Clear()
            GBA00007OrganizationRelated.USERORG = GBA00003UserSetting.USERORG
            'GBA00007OrganizationRelated.OPTJOTEXCLUSION = "1"
            GBA00007OrganizationRelated.LISTBOX_OFFICE = Me.lbOffice
            GBA00007OrganizationRelated.GBA00007getLeftListOffice()
            If GBA00007OrganizationRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00007OrganizationRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbOffice = DirectCast(GBA00007OrganizationRelated.LISTBOX_OFFICE, ListBox)
            Else
                returnCode = GBA00007OrganizationRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbOffice.Items.Count > 0 Then
                Dim findListItem = Me.lbOffice.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 荷主名設定
    ''' </summary>
    Public Sub txtShipper_Change()
        Try
            Me.lblShipperText.Text = ""
            If Me.txtShipper.Text.Trim = "" Then
                Return
            End If
            If Me.rblBreakerType.SelectedValue = "01SALES" Then
                SetShipperListItem(Me.txtShipper.Text)
            Else
                SetAgentShipperListItem(Me.txtShipper.Text)
            End If

            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShipper.Items.Count > 0 Then
                Dim findListItem = Me.lbShipper.Items.FindByValue(Me.txtShipper.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblShipperText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbShipper.Items.FindByValue(Me.txtShipper.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblShipperText.Text = parts(1)
                        Me.txtShipper.Text = parts(0)
                    End If
                End If
            End If

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 荷受人名設定
    ''' </summary>
    Public Sub txtConsignee_Change()
        Try
            Me.lblConsigneeText.Text = ""
            If Me.txtConsignee.Text.Trim = "" Then
                Return
            End If

            If Me.rblBreakerType.SelectedValue = "01SALES" Then
                SetConsigneeListItem(Me.txtConsignee.Text)
            Else
                SetAgentConsigneeListItem(Me.txtConsignee.Text)
            End If
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbConsignee.Items.Count > 0 Then
                Dim findListItem = Me.lbConsignee.Items.FindByValue(Me.txtConsignee.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblConsigneeText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbConsignee.Items.FindByValue(Me.txtConsignee.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblConsigneeText.Text = parts(1)
                        Me.txtConsignee.Text = parts(0)
                    End If
                End If
            End If

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' POL港名設定
    ''' </summary>
    Public Sub txtPOLPort_Change()
        Try
            Me.lblPOLPortText.Text = ""
            If Me.txtPOLPort.Text.Trim = "" Then
                Return
            End If
            SetPOLPortListItem(Me.txtPOLPort.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPOLPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPOLPort.Items.FindByValue(Me.txtPOLPort.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblPOLPortText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbPOLPort.Items.FindByValue(Me.txtPOLPort.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblPOLPortText.Text = parts(1)
                        Me.txtPOLPort.Text = parts(0)
                    End If
                End If
            End If

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' POD港名設定
    ''' </summary>
    Public Sub txtPODPort_Change()
        Try
            Me.lblPODPortText.Text = ""
            If Me.txtPODPort.Text.Trim = "" Then
                Return
            End If
            SetPODPortListItem(Me.txtPODPort.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPODPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPODPort.Items.FindByValue(Me.txtPODPort.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblPODPortText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbPODPort.Items.FindByValue(Me.txtPODPort.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblPODPortText.Text = parts(1)
                        Me.txtPODPort.Text = parts(0)
                    End If
                End If
            End If

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 積載品名設定
    ''' </summary>
    Public Sub txtProduct_Change()
        Try
            Me.lblProductText.Text = ""
            If Me.txtProduct.Text.Trim = "" Then
                Return
            End If
            SetProductListItem(Me.txtProduct.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbProduct.Items.Count > 0 Then
                Dim findListItem = Me.lbProduct.Items.FindByValue(Me.txtProduct.Text)
                If findListItem IsNot Nothing Then
                    Me.lblProductText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbProduct.Items.FindByValue(Me.txtProduct.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblProductText.Text = findListItemUpper.Text
                        Me.txtProduct.Text = findListItemUpper.Value
                    End If
                End If
            End If

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 承認名設定
    ''' </summary>
    Public Sub txtApproval_Change()
        Try
            Me.lblApprovalText.Text = ""
            If Me.txtApproval.Text.Trim = "" Then
                Return
            End If
            SetApprovalListItem(Me.txtApproval.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbApproval.Items.Count > 0 Then
                Dim findListItem = Me.lbApproval.Items.FindByValue(Me.txtApproval.Text)
                If findListItem IsNot Nothing Then
                    Me.lblApprovalText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbApproval.Items.FindByValue(Me.txtApproval.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblApprovalText.Text = findListItemUpper.Text
                        Me.txtApproval.Text = findListItemUpper.Value
                    End If
                End If
            End If

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 代理店名設定
    ''' </summary>
    Public Sub txtOffice_Change()
        Try
            Me.lblOfficeText.Text = ""
            If Me.txtOffice.Text.Trim = "" Then
                Return
            End If

            SetOfficeListItem(Me.txtOffice.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbOffice.Items.Count > 0 Then
                Dim findListItem = Me.lbOffice.Items.FindByValue(Me.txtOffice.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblOfficeText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbOffice.Items.FindByValue(Me.txtOffice.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblOfficeText.Text = parts(1)
                        Me.txtOffice.Text = parts(0)
                    End If
                End If
            End If

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 固定値マスタよりラジオボタン選択肢を取得
    ''' </summary>
    Private Sub SetBreakerTypeListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

        'リストクリア
        Me.rblBreakerType.Items.Clear()
        Dim tmpListBoxObj As New ListBox
        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "BREAKERTYPE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = tmpListBoxObj
        Else
            COA0017FixValue.LISTBOX2 = tmpListBoxObj
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                tmpListBoxObj = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                tmpListBoxObj = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If
        Else
            returnCode = COA0017FixValue.ERR
            Return
        End If

        For Each item As ListItem In tmpListBoxObj.Items
            Me.rblBreakerType.Items.Add(item)
        Next

    End Sub

End Class