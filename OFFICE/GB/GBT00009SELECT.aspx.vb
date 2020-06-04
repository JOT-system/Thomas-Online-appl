Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' SOA検索画面クラス
''' </summary>
Public Class GBT00009SELECT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00009S"     '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00004"
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

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            HttpContext.Current.Session("MAPurl") = ""
            returnCode = C_MESSAGENO.NORMAL

            '****************************************
            'メッセージ初期化
            '****************************************
            lblFooterMessage.Text = ""

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
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
                'コントロールの表示非表示
                '****************************************
                If GBA00003UserSetting.IS_JOTUSER Then
                    Me.trInvoicedBy.Visible = True
                Else
                    Me.trInvoicedBy.Visible = False
                End If
                '****************************************
                '初期表示
                '****************************************
                '検索設定の選択肢を取得(動的変化のない項目のみ)
                SetOfficeListItem("")
                SetAgentSoaListItem("")
                SetVenderListItem("")
                SetInvoicedByListItem("")
                SetCountryListItem("")
                SetReportMonthListItem("")
                DefaultValueSet()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                Me.txtInvoicedBy.Focus()
                '****************************************
                'セッション設定
                '****************************************
                HttpContext.Current.Session(CONST_BASEID & "_START") = CONST_MAPID
                'ClientScript.RegisterStartupScript(Me.Parent.GetType, "forcePostBack", "document.form[0].submit();", True)
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
                Case Me.vLeftReportMonth.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbReportMonth.Items.Count > 0 Then
                            Dim findResult As ListItem = Me.lbReportMonth.Items.FindByText(txtobj.Text)
                            If findResult IsNot Nothing Then
                                findResult.Selected = True
                            End If
                        End If

                        Me.mvLeft.Focus()
                    End If
                Case Me.vLeftInvoicedBy.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbInvoicedBy.Items.Count > 0 Then
                            Dim findResult As ListItem = Me.lbInvoicedBy.Items.FindByValue(txtobj.Text)
                            If findResult IsNot Nothing Then
                                findResult.Selected = True
                            End If
                        End If

                        Me.mvLeft.Focus()
                    End If
                Case Me.vLeftVender.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbVender.Items.Count > 0 Then
                            Dim findResult As ListItem = Me.lbVender.Items.FindByValue(txtobj.Text)
                            If findResult IsNot Nothing Then
                                findResult.Selected = True
                            End If
                        End If

                        Me.mvLeft.Focus()
                    End If
                Case Me.vLeftCountry.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCountry.Items.Count > 0 Then
                            Dim findResult As ListItem = Me.lbCountry.Items.FindByValue(txtobj.Text)
                            If findResult IsNot Nothing Then
                                findResult.Selected = True
                            End If
                        End If

                        Me.mvLeft.Focus()
                    End If
                Case Me.vLeftAgentSoa.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbAgentSoa.Items.Count > 0 Then
                            Dim findResult As ListItem = Me.lbAgentSoa.Items.FindByValue(txtobj.Text)
                            If findResult IsNot Nothing Then
                                findResult.Selected = True
                            End If
                        End If

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
        If Me.txtActualDateStYMD.Text <> "" AndAlso Me.txtActualDateEndYMD.Text = "" Then
            Me.txtActualDateEndYMD.Text = Me.txtActualDateStYMD.Text
        End If

        If Me.txtActualDateEndYMD.Text <> "" AndAlso Me.txtActualDateStYMD.Text = "" Then
            Me.txtActualDateStYMD.Text = Me.txtActualDateEndYMD.Text
        End If

        '画面遷移先URL取得
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = Me.hdnMapVariant.Value
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
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
                        txtobj.Text = Me.hdnCalendarValue.Value ' Date.Parse(Me.hdnCalendarValue.Value).ToString(GBA00003UserSetting.DATEFORMAT) '
                        txtobj.Focus()
                    End If
                Case Me.vLeftInvoicedBy.ID
                    '請求先選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbInvoicedBy.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbInvoicedBy.SelectedItem.Value
                            If Me.lbInvoicedBy.SelectedItem.Text.Contains(":") Then
                                Dim parts As String()
                                parts = Split(Me.lbInvoicedBy.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblInvoicedByText.Text = parts(1)
                            Else
                                Me.lblInvoicedByText.Text = Me.lbInvoicedBy.SelectedItem.Text
                            End If
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblInvoicedByText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftReportMonth.ID
                    '計上月選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbReportMonth.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbReportMonth.SelectedItem.Text
                            If Me.lbReportMonth.SelectedItem.Text.Contains(":") Then
                                Dim parts As String()
                                parts = Split(Me.lbReportMonth.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblReportMonthText.Text = parts(1)
                            Else
                                Me.lblReportMonthText.Text = Me.lbReportMonth.SelectedItem.Text
                            End If
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblReportMonthText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftVender.ID
                    '業者選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbVender.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbVender.SelectedItem.Value
                            If Me.lbVender.SelectedItem.Text.Contains(":") Then
                                Dim parts As String()
                                parts = Split(Me.lbVender.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblVenderText.Text = parts(1)
                            Else
                                Me.lblVenderText.Text = Me.lbVender.SelectedItem.Text
                            End If
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblVenderText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftCountry.ID
                    '国選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCountry.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbCountry.SelectedItem.Value
                            If Me.lbCountry.SelectedItem.Text.Contains(":") Then
                                Dim parts As String()
                                parts = Split(Me.lbCountry.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblCountryText.Text = parts(1)
                            Else
                                Me.lblCountryText.Text = Me.lbCountry.SelectedItem.Text
                            End If
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblCountryText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftAgentSoa.ID
                    'SOA種別選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbAgentSoa.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbAgentSoa.SelectedItem.Value
                            If Me.lbAgentSoa.SelectedItem.Text.Contains(":") Then
                                Dim parts As String()
                                parts = Split(Me.lbAgentSoa.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblAgentSoaText.Text = parts(1)
                            Else
                                Me.lblAgentSoaText.Text = Me.lbAgentSoa.SelectedItem.Text
                            End If
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblAgentSoaText.Text = ""
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

        AddLangSetting(dicDisplayText, Me.lblInvoicedBy, "請求先", "Invoiced by")
        AddLangSetting(dicDisplayText, Me.lblVender, "業者コード", "Vendor Code")
        AddLangSetting(dicDisplayText, Me.lblOffice, "代理店コード", "Office Code")
        AddLangSetting(dicDisplayText, Me.lblAgentSoa, "SOA種類", "SOA TYPE")
        AddLangSetting(dicDisplayText, Me.lblCountry, "国コード", "Country Code")

        AddLangSetting(dicDisplayText, Me.lblReportMonth, "計上月", "Report Month")
        AddLangSetting(dicDisplayText, Me.lblActualDate, "実績日", "Actual Date")
        AddLangSetting(dicDisplayText, Me.lblActualDateF, "範囲指定", "From")
        AddLangSetting(dicDisplayText, Me.lblActualDateT, "～", "To")


        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub

    ''' <summary>
    ''' 初期表示
    ''' </summary>
    Public Sub DefaultValueSet()

        If TypeOf Page.PreviousPage Is GBT00004ORDER Then
            Dim prevPage As GBT00004ORDER = DirectCast(Page.PreviousPage, GBT00004ORDER)
            '実行画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            '前画面と当画面のテキストボックス関連ディクショナリ
            '{"hdnDateTermStYMD", Me.txtDateTermStYMD},
            '{"hdnDateTermEndYMD", Me.txtDateTermEndYMD},
            Dim dicObjs As New Dictionary(Of String, TextBox) From {
                                                                    {"hdnOffice", Me.txtOffice},
                                                                    {"hdnInvoicedBy", Me.txtInvoicedBy},
                                                                    {"hdnVender", Me.txtVender},
                                                                    {"hdnAgentSoa", Me.txtAgentSoa},
                                                                    {"hdnCountry", Me.txtCountry},
                                                                    {"hdnActualDateStYMD", Me.txtActualDateStYMD},
                                                                    {"hdnActualDateEndYMD", Me.txtActualDateEndYMD},
                                                                    {"hdnReportMonth", Me.txtReportMonth}
                                                                    }

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, TextBox) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    dicObj.Value.Text = tmphdnObj.Value
                End If
            Next
            If Me.txtReportMonth.Text <> "" AndAlso Me.txtReportMonth.Text <> "ALL" Then
                If Me.lbReportMonth.Items IsNot Nothing AndAlso Me.lbReportMonth.Items.Count > 0 AndAlso Me.lbReportMonth.Items.FindByText(Me.txtReportMonth.Text) Is Nothing Then
                    Me.txtReportMonth.Text = Me.lbReportMonth.Items(0).Text
                End If
            End If
            '選択画面の入力初期値設定
            If prevPage.ProcResult IsNot Nothing AndAlso Not {"", C_MESSAGENO.NORMAL}.Contains(prevPage.ProcResult.MessageNo) Then
                CommonFunctions.ShowMessage(prevPage.ProcResult.MessageNo, Me.lblFooterMessage, pageObject:=Me, messageParams:=New List(Of String) From {prevPage.ProcResult.MessageNo})
            End If
            'メニューから遷移/業務画面戻り判定
        ElseIf Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00004ORDER Then
            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

            'JOTユーザーの場合Invoiced byはJOT Onlyをデフォルトとする
            If GBA00003UserSetting.IS_JOTUSER Then
                Me.txtInvoicedBy.Text = "OJ"
            Else
                '国コードは自国を設定
                'Me.txtOffice.Text = GBA00003UserSetting.OFFICECODE '自オフィス表示
                Me.txtOffice.Text = "" '20190718 オフィスの初期表示なし
                Me.txtCountry.Text = GBA00003UserSetting.COUNTRYCODE
            End If

        End If
        'コードを元に名称を設定
        txtAgentSoa_Change()
        txtCountry_Change()
        txtVender_Change()
        txtInvoicedBy_Change()
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
        '{"DATETERMSTYMD", Me.txtDateTermStYMD}, {"DATETERMENDYMD", Me.txtDateTermEndYMD},
        Dim dicDefaultValueSettings As New Dictionary(Of String, TextBox) _
                        From {
                              {"OFFICE", Me.txtOffice}, {"VENDER", Me.txtVender},
                              {"SOATYPE", Me.txtAgentSoa}, {"COUNTRYCODE", Me.txtCountry},
                              {"INVOICEDBY", Me.txtInvoicedBy}, {"ACTUALSTARTYMD", Me.txtActualDateStYMD},
                              {"ACTUALENDYMD", Me.txtActualDateEndYMD}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnMapVariant.Value
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                If {"ACTUALSTARTYMD", "ACTUALENDYMD"}.Contains(item.Key) Then
                    item.Value.Text = FormatDateContrySettings(COA0016VARIget.VALUE, GBA00003UserSetting.DATEFORMAT)
                Else
                    item.Value.Text = COA0016VARIget.VALUE
                End If
            Else
                CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
                Return
            End If
        Next

        COA0016VARIget.FIELD = "REPORTMONTH"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            Dim lbItem = Me.lbReportMonth.Items.FindByValue(COA0016VARIget.VALUE)
            If lbItem IsNot Nothing Then
                Me.txtReportMonth.Text = lbItem.Text
            End If

        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            Return
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
        COA0018ViewList.FORWARDMATCHVARIANT = Me.hdnMapVariant.Value
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
        COA0016VARIget.COMPCODE = ""
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
        '禁則文字置き換え、単項目チェック、リスト一致の処理を行う配列
        Dim checkObjList = {New With {.txtObj = Me.txtOffice, .lstObj = lbOffice, .fieldName = "OFFICE", .swapListValue = False},
                            New With {.txtObj = Me.txtAgentSoa, .lstObj = lbAgentSoa, .fieldName = "SOATYPE", .swapListValue = False},
                            New With {.txtObj = Me.txtInvoicedBy, .lstObj = lbInvoicedBy, .fieldName = "INVOICEDBY", .swapListValue = False},
                            New With {.txtObj = Me.txtCountry, .lstObj = lbCountry, .fieldName = "COUNTRYCODE", .swapListValue = False},
                            New With {.txtObj = Me.txtVender, .lstObj = lbVender, .fieldName = "VENDER", .swapListValue = False},
                            New With {.txtObj = Me.txtReportMonth, .lstObj = lbReportMonth, .fieldName = "REPORTMONTH", .swapListValue = True},
                            New With {.txtObj = Me.txtActualDateStYMD, .lstObj = DirectCast(Nothing, ListBox), .fieldName = "ACTUALSTARTYMD", .swapListValue = False},
                            New With {.txtObj = Me.txtActualDateEndYMD, .lstObj = DirectCast(Nothing, ListBox), .fieldName = "ACTUALENDYMD", .swapListValue = False}}
        '上記で定義した配列を元に入力チェック
        For Each checkObj In checkObjList
            '入力文字置き換え
            COA0008InvalidChar.CHARin = checkObj.txtObj.Text
            COA0008InvalidChar.COA0008RemoveInvalidChar()
            If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
            Else
                checkObj.txtObj.Text = COA0008InvalidChar.CHARout
            End If

            '入力項目チェック
            '単項目チェック
            If checkObj.fieldName <> "" Then
                Dim chkVal As String = checkObj.txtObj.Text

                If {"ACTUALSTARTYMD", "ACTUALENDYMD"}.Contains(checkObj.fieldName) Then
                    chkVal = FormatDateYMD(chkVal, GBA00003UserSetting.DATEFORMAT)
                End If
                CheckSingle(checkObj.fieldName, chkVal)
                If returnCode <> C_MESSAGENO.NORMAL Then
                    checkObj.txtObj.Focus()
                    Return
                End If
            End If

                'List存在チェック
                If checkObj.lstObj IsNot Nothing Then
                CheckList(checkObj.txtObj.Text, checkObj.lstObj, checkObj.swapListValue)
                If returnCode <> C_MESSAGENO.NORMAL Then
                    checkObj.txtObj.Focus()
                    Return
                End If
            End If
        Next
        '日付前後関係チェック
        CheckDate(FormatDateYMD(Me.txtActualDateStYMD.Text, GBA00003UserSetting.DATEFORMAT), FormatDateYMD(Me.txtActualDateEndYMD.Text, GBA00003UserSetting.DATEFORMAT))
        If returnCode <> C_MESSAGENO.NORMAL Then
            Me.txtActualDateStYMD.Focus()
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
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            returnCode = COA0026FieldCheck.ERR
        End If

    End Sub
    ''' <summary>
    ''' LIST登録チェック
    ''' </summary>
    ''' <param name="inText"></param>
    ''' <param name="inList"></param>
    Protected Sub CheckList(ByVal inText As String, ByVal inList As ListBox, Optional swapKeyValue As Boolean = False)

        Dim flag As Boolean = False

        If inText <> "" Then

            For i As Integer = 0 To inList.Items.Count - 1
                If (swapKeyValue = False AndAlso inList.Items(i).Value = inText) _
                 OrElse (swapKeyValue = True AndAlso inList.Items(i).Text = inText) Then
                    flag = True
                    Exit For
                End If
            Next

            If (flag = False) Then
                returnCode = C_MESSAGENO.INVALIDINPUT
                CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            End If
        End If
    End Sub
    ''' <summary>
    ''' 日付整合性チェック
    ''' </summary>
    ''' <param name="inStYMD"></param>
    ''' <param name="inEndYMD"></param>
    Protected Sub CheckDate(ByVal inStYMD As String, ByVal inEndYMD As String)

        If inStYMD = "" AndAlso inEndYMD = "" Then
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
    ''' 請求先タイプ選択
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetInvoicedByListItem(selectedValue As String)
        Try
            Me.lbInvoicedBy.Items.Clear()
            Dim COA0017FixValue As New COA0017FixValue With {.COMPCODE = GBC_COMPCODE_D, .CLAS = "INVOICEDBYTYPE",
                                                             .LISTBOX2 = Me.lbInvoicedBy}
            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR <> C_MESSAGENO.NORMAL Then
                returnCode = COA0017FixValue.ERR
                Return
            End If

            Me.lbInvoicedBy = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbInvoicedBy.Items.Count > 0 Then
                Dim findListItem = Me.lbInvoicedBy.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
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
    ''' SOAデータ絞り込み種別
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetAgentSoaListItem(selectedValue As String)
        Try
            Me.lbAgentSoa.Items.Clear()
            Dim COA0017FixValue As New COA0017FixValue With {.COMPCODE = GBC_COMPCODE_D, .CLAS = "AGENTSOA",
                                                             .LISTBOX2 = Me.lbAgentSoa}
            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR <> "" Then
                returnCode = COA0017FixValue.ERR
                Return
            End If

            Me.lbAgentSoa = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbAgentSoa.Items.Count > 0 Then
                Dim findListItem = Me.lbAgentSoa.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
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
    ''' 計上月種別を取得
    ''' </summary>
    ''' <param name="selectedValue"></param>
    ''' <remarks>FIXVALUEより取得</remarks>
    Private Sub SetReportMonthListItem(selectedValue As String)
        Try
            'Me.lbReportMonth.Items.Clear()
            'Dim COA0017FixValue As New COA0017FixValue With {.COMPCODE = GBC_COMPCODE_D, .CLAS = "REPORTMONTH",
            '                                                 .LISTBOX2 = Me.lbReportMonth}
            'COA0017FixValue.COA0017getListFixValue()
            'If COA0017FixValue.ERR <> "" Then
            '    returnCode = COA0017FixValue.ERR
            '    Return
            'End If

            'Me.lbReportMonth = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            ''一応現在入力しているテキストと一致するものを選択状態
            'If Me.lbReportMonth.Items.Count > 0 Then
            '    Dim findListItem = Me.lbReportMonth.Items.FindByValue(selectedValue)
            '    If findListItem IsNot Nothing Then
            '        findListItem.Selected = True
            '    End If
            'End If
            'リストクリア
            Me.lbReportMonth.Items.Clear()
            'SQL文の作成
            Dim GBA00003UserSetting As New GBA00003UserSetting With {.USERID = COA0019Session.USERID}
            GBA00003UserSetting.GBA00003GetUserSetting()
            Dim countryCode = GBA00003UserSetting.COUNTRYCODE
            If GBA00003UserSetting.IS_JOTUSER Then
                countryCode = GBC_JOT_SOA_COUNTRY
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT CD.BILLINGYMD ")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,1,DATEADD(day,-1,CD.BILLINGYMD)),'{0}') AS THISMONTH")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,2,DATEADD(day,-1,CD.BILLINGYMD)),'{0}') AS NEXTMONTH")
            sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CD")
            sqlStat.AppendLine(" WHERE CD.COUNTRYCODE    = @COUNTRYCODE")
            sqlStat.AppendLine("   AND CD.DELFLG         = @DELFLG")
            sqlStat.AppendLine("   AND CD.REPORTMONTH = (SELECT MAX(CDS.REPORTMONTH)")
            sqlStat.AppendLine("                           FROM GBT0006_CLOSINGDAY CDS")
            sqlStat.AppendLine("                          WHERE CDS.COUNTRYCODE    = @COUNTRYCODE")
            sqlStat.AppendLine("                            AND CDS.DELFLG         = @DELFLG")
            sqlStat.AppendLine("                        )")

            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(String.Format(sqlStat.ToString, GBA00003UserSetting.DATEYMFORMAT), SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COUNTRYCODE", System.Data.SqlDbType.NVarChar).Value = countryCode
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = CONST_FLAG_NO
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        'DBからアイテムを設定
                        Me.lbReportMonth.Items.Add(New ListItem(String.Format("{0}", SQLdr("THISMONTH")), "1"))
                        Me.lbReportMonth.Items.Add(New ListItem(String.Format("{0}", SQLdr("NEXTMONTH")), "2"))
                        Me.lbReportMonth.Items.Add(New ListItem("ALL", "3"))
                        Exit While
                    End While
                End Using 'SQLdr
            End Using 'SQLcon SQLcmd

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
    ''' 業者コード列挙したリストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    ''' <remarks>TRADERとDEPOテーブルのUNION</remarks>
    Private Sub SetVenderListItem(selectedValue As String)
        Try
            'リストクリア
            Me.lbVender.Items.Clear()
            'SQL文の作成
            Dim nameCol As String = ""
            If COA0019Session.LANGDISP = C_LANG.JA Then
                nameCol = "NAMESJP"
            Else
                nameCol = "NAMES"
            End If
            Dim nameColCustomer As String = ""
            If COA0019Session.LANGDISP = C_LANG.JA Then
                nameColCustomer = "NAMES"
            Else
                nameColCustomer = "NAMESEN"
            End If

            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT TBL.* ")
            sqlStat.AppendLine("  FROM (")
            sqlStat.AppendLine("SELECT TR.CARRIERCODE AS CODE")
            sqlStat.AppendFormat("      ,TR.{0}       AS NAME", nameCol).AppendLine()
            sqlStat.AppendLine("  FROM GBM0005_TRADER TR")
            sqlStat.AppendLine(" WHERE TR.STYMD  <= @STYMD")
            sqlStat.AppendLine("   AND TR.ENDYMD >= @ENDYMD")
            sqlStat.AppendLine("   AND TR.DELFLG  = @DELFLG")
            sqlStat.AppendLine("UNION ALL ")
            sqlStat.AppendLine("SELECT DP.DEPOTCODE AS CODE")
            sqlStat.AppendFormat("      ,DP.{0}       AS NAME", nameCol).AppendLine()
            sqlStat.AppendLine("  FROM GBM0003_DEPOT DP")
            sqlStat.AppendLine(" WHERE DP.STYMD  <= @STYMD")
            sqlStat.AppendLine("   AND DP.ENDYMD >= @ENDYMD")
            sqlStat.AppendLine("   AND DP.DELFLG  = @DELFLG")
            sqlStat.AppendLine("UNION ALL ")
            sqlStat.AppendLine("SELECT CU.CUSTOMERCODE AS CODE")
            sqlStat.AppendFormat("      ,CU.{0}       AS NAME", nameColCustomer).AppendLine()
            sqlStat.AppendLine("  FROM GBM0004_CUSTOMER CU")
            sqlStat.AppendLine(" WHERE CU.STYMD  <= @STYMD")
            sqlStat.AppendLine("   AND CU.ENDYMD >= @ENDYMD")
            sqlStat.AppendLine("   AND CU.DELFLG  = @DELFLG")
            sqlStat.AppendLine("  ) TBL")
            sqlStat.AppendLine(" ORDER BY CODE")

            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = CONST_FLAG_NO
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        'DBからアイテムを設定
                        Me.lbVender.Items.Add(New ListItem(String.Format("{0}:{1}", SQLdr("CODE"), SQLdr("NAME")), Convert.ToString(SQLdr("CODE"))))
                    End While
                End Using 'SQLdr
            End Using 'SQLcon SQLcmd

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
            If GBA00007OrganizationRelated.ERR = C_MESSAGENO.NORMAL Then
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
    ''' 国コードリストアイテムを設定
    ''' </summary>
    Private Sub SetCountryListItem(selectedValue As String)
        Dim GBA00008Country As New GBA00008Country
        GBA00008Country.COUNTRY_LISTBOX = Me.lbCountry
        GBA00008Country.getCountryList()
        If GBA00008Country.ERR <> C_MESSAGENO.NORMAL Then
            returnCode = GBA00008Country.ERR
            Return
        End If
        '一覧先頭にALLを追加
        Me.lbCountry.Items.Insert(0, New ListItem("ALL", "ALL"))
        '正常
        returnCode = C_MESSAGENO.NORMAL
    End Sub

    ''' <summary>
    ''' InvoicedBy種類条件変更時イベント
    ''' </summary>
    Public Sub txtInvoicedBy_Change()
        Try
            Me.lblInvoicedByText.Text = ""
            If Me.txtInvoicedBy.Text.Trim = "" Then
                Return
            End If

            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbInvoicedBy.Items.Count > 0 Then
                Dim findListItem = Me.lbInvoicedBy.Items.FindByValue(Me.txtInvoicedBy.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblInvoicedByText.Text = parts(1)
                    Else
                        Me.lblInvoicedByText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbInvoicedBy.Items.FindByValue(Me.txtInvoicedBy.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblInvoicedByText.Text = parts(1)
                            Me.txtInvoicedBy.Text = parts(0)
                        Else
                            Me.lblInvoicedByText.Text = findListItemUpper.Text
                            Me.txtInvoicedBy.Text = findListItemUpper.Value
                        End If

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
    ''' 計上月条件変更時イベント
    ''' </summary>
    Public Sub txtReportMonth_Change()
        Try
            Me.lblReportMonthText.Text = ""
            If Me.txtReportMonth.Text.Trim = "" Then
                Return
            End If

            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbReportMonth.Items.Count > 0 Then
                Dim findListItem = Me.lbReportMonth.Items.FindByValue(Me.txtReportMonth.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblReportMonthText.Text = parts(1)
                    Else
                        Me.lblReportMonthText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbReportMonth.Items.FindByValue(Me.txtReportMonth.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblReportMonthText.Text = parts(1)
                            Me.txtReportMonth.Text = parts(0)
                        Else
                            Me.lblReportMonthText.Text = findListItemUpper.Text
                            Me.txtReportMonth.Text = findListItemUpper.Value
                        End If

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
    ''' 業者変更時イベント
    ''' </summary>
    Public Sub txtVender_Change()
        Try
            Me.lblVenderText.Text = ""
            If Me.txtVender.Text.Trim = "" Then
                Return
            End If

            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbVender.Items.Count > 0 Then
                Dim findListItem = Me.lbVender.Items.FindByValue(Me.txtVender.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblVenderText.Text = parts(1)
                    Else
                        Me.lblVenderText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbVender.Items.FindByValue(Me.txtVender.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblVenderText.Text = parts(1)
                            Me.txtVender.Text = parts(0)
                        Else
                            Me.lblVenderText.Text = findListItemUpper.Text
                            Me.txtVender.Text = findListItemUpper.Value
                        End If

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
    ''' 国変更時イベント
    ''' </summary>
    Public Sub txtCountry_Change()
        Try
            Me.lblCountryText.Text = ""
            If Me.txtCountry.Text.Trim = "" Then
                Return
            End If

            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbCountry.Items.Count > 0 Then
                Dim findListItem = Me.lbCountry.Items.FindByValue(Me.txtCountry.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblCountryText.Text = parts(1)
                    Else
                        Me.lblCountryText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbCountry.Items.FindByValue(Me.txtCountry.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblCountryText.Text = parts(1)
                            Me.txtCountry.Text = parts(0)
                        Else
                            Me.lblCountryText.Text = findListItemUpper.Text
                            Me.txtCountry.Text = findListItemUpper.Value
                        End If

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
    ''' SOA種別変更時イベント
    ''' </summary>
    Public Sub txtAgentSoa_Change()
        Try
            Me.lblAgentSoaText.Text = ""
            If Me.txtAgentSoa.Text.Trim = "" Then
                Return
            End If
            If Me.lbAgentSoa.Items.Count > 0 Then
                Dim finditem As ListItem = Me.lbAgentSoa.Items.FindByValue(Me.txtAgentSoa.Text)
                Me.lblAgentSoaText.Text = finditem.Text
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
End Class