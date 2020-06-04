Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' ORDER検索画面クラス
''' </summary>
Public Class GBT00013SELECT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00013S"     '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00013"
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
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'タイトル設定
                '****************************************
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnThisMapVariant.Value
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                End If

                '****************************************
                '初期表示
                '****************************************
                '検索設定の選択肢を取得
                SetSearchTypeListItem()
                'B/L発行有無の選択肢をFIXVALUEより取得し左のリストボックスに展開
                SetBlIssuedListItem()
                '発着区分の選択肢を取得
                SetDepartureArrivalListItem()
                SetCountryListItem("")
                DefaultValueSet()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                rblSearchType.Focus()
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
                '他のビューが存在する場合はViewIdでCaseを追加
                '荷主コードビュー表示切替
                Case Me.vLeftShipper.ID
                    SetShipperListItem(Me.txtShipper.Text)
                '荷受人コードビュー表示切替
                Case Me.vLeftConsignee.ID
                    SetConsigneeListItem(Me.txtConsignee.Text)
                '港コードビュー表示切替
                Case Me.vLeftPort.ID
                    SetPortListItem(Me.txtPort.Text)
                '積載品コードビュー表示切替
                Case Me.vLeftProduct.ID
                    SetProductListItem(Me.txtProduct.Text)
                '船会社ビュー切り替え
                Case Me.vLeftCarrier.ID
                    SetCarrierListItem(Me.txtCarrier.Text)
                '代理店コードビュー表示切替
                Case Me.vLeftOffice.ID
                    SetOfficeListItem(Me.txtOffice.Text)
                ''発着区分ビュー表示切替
                'Case Me.vLeftDepartureArrival.ID
                '    SetDepartureArrivalListItem(Me.txtDepartureArrival.Text)
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
        If Me.txtETDStYMD.Text <> "" AndAlso Me.txtETDEndYMD.Text = "" Then
            Me.txtETDEndYMD.Text = Me.txtETDStYMD.Text
        End If

        If Me.txtETDEndYMD.Text <> "" AndAlso Me.txtETDStYMD.Text = "" Then
            Me.txtETDStYMD.Text = Me.txtETDEndYMD.Text
        End If

        '画面遷移先URL取得
        COA0012DoUrl.MAPIDP = CONST_MAPID

        'If txtDepartureArrival.Text <> "" Then
        HttpContext.Current.Session("MAPvariant") = "GB_PRINT"
        'Else
        'HttpContext.Current.Session("MAPvariant") = "GB_BL"
        'End If
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
    ''' 終了ボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '画面戻先URL取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
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
                Case Me.vLeftBlIssued.ID
                    'B/L発行有無
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbBlIssued.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbBlIssued.SelectedItem.Value
                        Else
                            txtobj.Text = ""
                        End If
                        txtBlIssued_Change()
                        txtobj.Focus()
                    End If
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
                Case Me.vLeftPort.ID 'アクティブなビューが港コード
                    '港コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)

                        If Me.lbPort.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbPort.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbPort.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblPortText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblPortText.Text = ""
                            txtobj.Focus()
                        End If

                    End If
                Case Me.vLeftProduct.ID
                    '積載品コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbProduct.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbProduct.SelectedItem.Value
                        Else
                            txtobj.Text = ""
                        End If
                        txtProduct_Change()
                        txtobj.Focus()
                    End If
                Case Me.vLeftCarrier.ID
                    '船会社コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCarrier.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbCarrier.SelectedItem.Value
                        Else
                            txtobj.Text = ""
                        End If
                        txtCarrier_Change()
                        txtobj.Focus()
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
                Case Me.vLeftDepartureArrival.ID 'アクティブなビューが発着区分
                    '発着区分選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbDepartureArrival.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbDepartureArrival.SelectedItem.Value
                        Else
                            txtobj.Text = ""
                        End If
                        'txtDepartureArrival_Change()
                        txtobj.Focus()
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
        AddLangSetting(dicDisplayText, Me.lblSearchType, "検索種類", "Search Type")
        AddLangSetting(dicDisplayText, Me.lblBlIssued, "B/L発行有無", "B/L ISSUED")
        AddLangSetting(dicDisplayText, Me.lblETD1, "出港予定日", "ETD")
        AddLangSetting(dicDisplayText, Me.lblETD2, "範囲指定", "From")
        AddLangSetting(dicDisplayText, Me.lblETDTilde, "～", "To")
        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主コード", "Shipper Code")
        AddLangSetting(dicDisplayText, Me.lblConsignee, "荷受人コード", "Consignee Code")
        AddLangSetting(dicDisplayText, Me.lblPort, "港コード", "Port Code")
        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品コード", "Product Code")
        AddLangSetting(dicDisplayText, Me.lblCarrier, "船社コード", "Carrier Code")
        AddLangSetting(dicDisplayText, Me.lblVsl, "船名", "VESSEL")
        AddLangSetting(dicDisplayText, Me.lblOffice, "代理店コード", "Office Code")
        AddLangSetting(dicDisplayText, Me.lblCountry, "国コード", "Country Code")
        AddLangSetting(dicDisplayText, Me.lblDepartureArrival, "発着区分", "Departure Arrival")
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

        '選択画面の入力初期値設定
        'メニューから遷移/業務画面戻り判定
        If Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage Is COM00002MENU Then

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00013RESULT Then
            Dim prevPage As GBT00013RESULT = DirectCast(Page.PreviousPage, GBT00013RESULT)
            '実行画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            '前画面と当画面のテキストボックス関連ディクショナリ
            Dim dicObjs As New Dictionary(Of String, TextBox) From {{"hdnBlIssued", Me.txtBlIssued},
                                                                    {"hdnETDStYMD", Me.txtETDStYMD},
                                                                    {"hdnETDEndYMD", Me.txtETDEndYMD},
                                                                    {"hdnShipper", Me.txtShipper},
                                                                    {"hdnConsignee", Me.txtConsignee},
                                                                    {"hdnPort", Me.txtPort},
                                                                    {"hdnProduct", Me.txtProduct},
                                                                    {"hdnCarrier", Me.txtCarrier},
                                                                    {"hdnVsl", Me.txtVsl},
                                                                    {"hdnCountry", Me.txtCountry},
                                                                    {"hdnOffice", Me.txtOffice}}

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, TextBox) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    dicObj.Value.Text = tmphdnObj.Value
                End If
            Next
            Dim objSearchType As Control = prevPage.FindControl("hdnSearchType")
            If objSearchType IsNot Nothing Then
                Dim hdnSearchType As HiddenField = DirectCast(objSearchType, HiddenField)
                If Me.rblSearchType.Items.FindByValue(hdnSearchType.Value) IsNot Nothing Then
                    Me.rblSearchType.SelectedValue = hdnSearchType.Value
                End If
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00017RESULT Then
            Dim prevPage As GBT00017RESULT = DirectCast(Page.PreviousPage, GBT00017RESULT)
            '実行画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            '前画面と当画面のテキストボックス関連ディクショナリ
            Dim dicObjs As New Dictionary(Of String, TextBox) From {{"hdnBlIssued", Me.txtBlIssued},
                                                                    {"hdnETDStYMD", Me.txtETDStYMD},
                                                                    {"hdnETDEndYMD", Me.txtETDEndYMD},
                                                                    {"hdnShipper", Me.txtShipper},
                                                                    {"hdnConsignee", Me.txtConsignee},
                                                                    {"hdnPort", Me.txtPort},
                                                                    {"hdnProduct", Me.txtProduct},
                                                                    {"hdnCarrier", Me.txtCarrier},
                                                                    {"hdnVsl", Me.txtVsl},
                                                                    {"hdnCountry", Me.txtCountry},
                                                                    {"hdnOffice", Me.txtOffice}
                                                                    }

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, TextBox) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    dicObj.Value.Text = tmphdnObj.Value
                End If
            Next
            Dim objSearchType As Control = prevPage.FindControl("hdnSearchType")
            If objSearchType IsNot Nothing Then
                Dim hdnSearchType As HiddenField = DirectCast(objSearchType, HiddenField)
                If Me.rblSearchType.Items.FindByValue(hdnSearchType.Value) IsNot Nothing Then
                    Me.rblSearchType.SelectedValue = hdnSearchType.Value
                End If
            End If

            Dim objDepartureArrival As Control = prevPage.FindControl("hdnDepartureArrival")
            If objDepartureArrival IsNot Nothing Then
                Dim hdnDepartureArrival As HiddenField = DirectCast(objDepartureArrival, HiddenField)
                If Me.rblDepartureArrival.Items.FindByValue(hdnDepartureArrival.Value) IsNot Nothing Then
                    Me.rblDepartureArrival.SelectedValue = hdnDepartureArrival.Value
                End If
            End If

            HttpContext.Current.Session("MAPvariant") = "GB_BL"
            Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))

        End If
        'コードを元に名称を設定
        'B/L発行有無
        txtBlIssued_Change()
        '荷主コード　
        txtShipper_Change()
        '荷受人コード　
        txtConsignee_Change()
        '出港コード　
        txtPort_Change()
        '積載品コード
        txtProduct_Change()
        '船社コード
        txtCarrier_Change()
        '代理店コード
        txtOffice_Change()
        '発着区分
        'txtDepartureArrival_Change()
        '国コード
        txtCountry_Change()
        ''RightBox情報設定
        'rightBoxSet()
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
                        From {{"BLISSUED", Me.txtBlIssued},
                              {"ETDSTYMD", Me.txtETDStYMD}, {"ETDENDYMD", Me.txtETDEndYMD},
                              {"SHIPPER", Me.txtShipper}, {"CONSIGNEE", Me.txtConsignee},
                              {"PORT", Me.txtPort}, {"PRODUCT", Me.txtProduct},
                              {"CARRIER", Me.txtProduct}, {"VSL", Me.txtVsl},
                              {"COUNTRYCODE", Me.txtCountry}, {"OFFICE", Me.txtOffice}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                If {"ETDSTYMD", "ETDENDYMD"}.Contains(item.Key) Then
                    item.Value.Text = BASEDLL.FormatDateContrySettings(COA0016VARIget.VALUE, GBA00003UserSetting.DATEFORMAT)
                Else
                    item.Value.Text = COA0016VARIget.VALUE
                End If
            Else
                CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
                Return
            End If
        Next
        'ラジオボタンの初期値設定
        COA0016VARIget.FIELD = "SEARCHTYPE"
        COA0016VARIget.COA0016VARIget()
        If Me.rblSearchType.Items.FindByValue(COA0016VARIget.VALUE) IsNot Nothing Then
            Me.rblSearchType.SelectedValue = COA0016VARIget.VALUE
        End If

        COA0016VARIget.FIELD = "DEPARTUREARRIVAL"
        COA0016VARIget.COA0016VARIget()
        If Me.rblDepartureArrival.Items.FindByValue(COA0016VARIget.VALUE) IsNot Nothing Then
            Me.rblDepartureArrival.SelectedValue = COA0016VARIget.VALUE
        End If

        'COUNTRYは動的の為個別セット
        SetCountryListItem("")
        If Me.lbCountry.Items.FindByValue(GBA00003UserSetting.COUNTRYCODE) IsNot Nothing Then
            Me.txtCountry.Text = GBA00003UserSetting.COUNTRYCODE
        Else
            Me.txtCountry.Text = ""
        End If

        'OFFICEは動的の為個別セット
        SetOfficeListItem("")
        If Me.lbOffice.Items.FindByValue(GBA00003UserSetting.OFFICECODE) IsNot Nothing AndAlso GBA00003UserSetting.IS_JOTUSER = False Then
            Me.txtOffice.Text = GBA00003UserSetting.OFFICECODE
        Else
            Me.txtOffice.Text = ""
        End If
    End Sub
    '''' <summary>
    '''' 右ボックス設定
    '''' </summary>
    'Public Sub rightBoxSet()
    '    Dim COA0018ViewList As New BASEDLL.COA0018ViewList          '変数情報取
    '    Dim COA0016VARIget As New BASEDLL.COA0016VARIget            '変数情報取

    '    'RightBOX情報設定
    '    '画面レイアウト情報
    '    COA0018ViewList.MAPID = CONST_BASEID
    '    COA0018ViewList.VIEW = lbRightList
    '    COA0018ViewList.COA0018getViewList()
    '    If COA0018ViewList.ERR = C_MESSAGENO.NORMAL Then
    '        Try
    '            For i As Integer = 0 To DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items.Count - 1
    '                lbRightList.Items.Add(New ListItem(DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items(i).Text, DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items(i).Value))
    '            Next
    '        Catch ex As Exception
    '        End Try
    '    Else
    '        CommonFunctions.ShowMessage(COA0018ViewList.ERR,Me.lblFooterMessage)
    '        returnCode = COA0018ViewList.ERR
    '        Return
    '    End If

    '    'ビューID変数検索
    '    COA0016VARIget.MAPID = CONST_MAPID
    '    COA0016VARIget.COMPCODE = ""
    '    COA0016VARIget.VARI = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
    '    COA0016VARIget.FIELD = "VIEWID"
    '    COA0016VARIget.COA0016VARIget()
    '    If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
    '    Else
    '        CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
    '        returnCode = COA0016VARIget.ERR
    '        Return
    '    End If

    '    'ListBox選択
    '    lbRightList.SelectedIndex = 0     '選択無しの場合、デフォルト
    '    For i As Integer = 0 To lbRightList.Items.Count - 1
    '        If lbRightList.Items(i).Value = COA0016VARIget.VALUE Then
    '            lbRightList.SelectedIndex = i
    '        End If
    '    Next

    'End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    Public Sub checkProc()
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get

        '入力文字置き換え
        '画面PassWord内の使用禁止文字排除

        'B/L ISSUED
        COA0008InvalidChar.CHARin = txtBlIssued.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtBlIssued.Text = COA0008InvalidChar.CHARout
        End If

        'ETD開始日
        COA0008InvalidChar.CHARin = txtETDStYMD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtETDStYMD.Text = COA0008InvalidChar.CHARout
        End If

        'ETD終了日
        COA0008InvalidChar.CHARin = txtETDEndYMD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtETDEndYMD.Text = COA0008InvalidChar.CHARout
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

        '港コード
        COA0008InvalidChar.CHARin = txtPort.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtPort.Text = COA0008InvalidChar.CHARout
        End If

        '積載品コード
        COA0008InvalidChar.CHARin = txtProduct.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtProduct.Text = COA0008InvalidChar.CHARout
        End If

        '船社コード
        COA0008InvalidChar.CHARin = txtCarrier.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtCarrier.Text = COA0008InvalidChar.CHARout
        End If

        '代理店コード
        COA0008InvalidChar.CHARin = txtOffice.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtOffice.Text = COA0008InvalidChar.CHARout
        End If

        ''発着区分
        'COA0008InvalidChar.CHARin = txtDepartureArrival.Text
        'COA0008InvalidChar.COA0008RemoveInvalidChar()
        'If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        'Else
        '    txtDepartureArrival.Text = COA0008InvalidChar.CHARout
        'End If

        '国コード
        COA0008InvalidChar.CHARin = txtCountry.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtCountry.Text = COA0008InvalidChar.CHARout
        End If

        '入力項目チェック

        'B/L ISSUED 単項目チェック
        CheckSingle("BLISSUED", txtBlIssued.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtBlIssued.Focus()
            Return
        End If

        'B/L ISSUED List存在チェック
        CheckList(txtBlIssued.Text, lbBlIssued)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtBlIssued.Focus()
            Return
        End If

        'ETD開始日 単項目チェック
        Dim stStr As String
        Dim stDate As Date
        If Date.TryParseExact(Me.txtETDStYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, stDate) Then
            stStr = stDate.ToString("yyyy/MM/dd")
        Else
            stStr = Me.txtETDStYMD.Text
        End If

        CheckSingle("ETDSTYMD", stStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETDStYMD.Focus()
            Return
        End If

        'ETD終了日 単項目チェック
        Dim endStr As String
        Dim endDate As Date
        If Date.TryParseExact(Me.txtETDEndYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, endDate) Then
            endStr = stDate.ToString("yyyy/MM/dd")
        Else
            endStr = Me.txtETDEndYMD.Text
        End If

        CheckSingle("ETDENDYMD", endStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETDEndYMD.Focus()
            Return
        End If

        'ETD日付整合性チェック
        CheckDate(stStr, endStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETDStYMD.Focus()
            Return
        End If

        ''出港入港必須チェック
        'ETDETAMustCheck()
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    txtETDStYMD.Focus()
        '    Return
        'End If

        '荷主コード 単項目チェック
        CheckSingle("SHIPPER", txtShipper.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtShipper.Focus()
            Return
        End If

        '荷主コード List存在チェック
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
        CheckList(txtConsignee.Text, lbConsignee)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtConsignee.Focus()
            Return
        End If

        '積荷港コード 単項目チェック
        CheckSingle("PORT", txtPort.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPort.Focus()
            Return
        End If

        '積荷港コード List存在チェック
        CheckList(txtPort.Text, lbPort)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPort.Focus()
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

        '船社コード 単項目チェック
        CheckSingle("CARRIER", txtCarrier.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtCarrier.Focus()
            Return
        End If

        '船社コード List存在チェック
        CheckList(txtCarrier.Text, lbCarrier)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtCarrier.Focus()
            Return
        End If

        '国コード 単項目チェック
        CheckSingle("COUNTRYCODE", txtCountry.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtCountry.Focus()
            Return
        End If

        '国コード List存在チェック
        CheckList(txtCountry.Text, lbCountry)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtCountry.Focus()
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

        ''発着区分 単項目チェック
        'CheckSingle("DEPARTUREARRIVAL", txtDepartureArrival.Text)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    txtDepartureArrival.Focus()
        '    Return
        'End If

        ''発着区分 List存在チェック
        'CheckList(txtDepartureArrival.Text, lbDepartureArrival)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    txtDepartureArrival.Focus()
        '    Return
        'End If

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
    ''' 出港日入港日必須チェック
    ''' </summary>
    Protected Sub ETDETAMustCheck()

        If (txtETDStYMD.Text Is Nothing OrElse txtETDStYMD.Text = "") Then
            returnCode = C_MESSAGENO.REQUIREDVALUE
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
        End If

    End Sub
    ''' <summary>
    ''' 荷主コードリストアイテムを設定
    ''' </summary>
    Private Sub SetShipperListItem(selectedValue As String)
        Try

            'リストクリア
            Me.lbShipper.Items.Clear()
            Dim GBA00004CountryRelated As New GBA00004CountryRelated
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
    ''' 積載品コードリストアイテムを設定
    ''' </summary>
    Private Sub SetProductListItem(selectedValue As String)
        Try
            Me.lbProduct.Items.Clear() 'リストクリア

            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT PRODUCTCODE")
            sqlStat.AppendLine("      ,PRODUCTNAME AS NAMESJP")
            sqlStat.AppendLine("      ,PRODUCTNAME AS NAMES")
            sqlStat.AppendLine("  FROM  GBM0008_PRODUCT")
            sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE")
            sqlStat.AppendLine("   AND STYMD   <= @STYMD")
            sqlStat.AppendLine("   AND ENDYMD  >= @ENDYMD")
            sqlStat.AppendLine("   AND DELFLG  <> @DELFLG")
            sqlStat.AppendLine("ORDER BY PRODUCTCODE")
            'DataBase接続文字
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = GBC_COMPCODE
                    .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", System.Data.SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES

                End With
                Using SQLdr = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        'DBからアイテムを設定
                        If COA0019Session.LANGDISP = C_LANG.JA Then
                            Me.lbProduct.Items.Add(New ListItem(String.Format("{0}:{1}", SQLdr("PRODUCTCODE"), SQLdr("NAMESJP")), Convert.ToString(SQLdr("PRODUCTCODE"))))
                        Else
                            Me.lbProduct.Items.Add(New ListItem(String.Format("{0}:{1}", SQLdr("PRODUCTCODE"), SQLdr("NAMES")), Convert.ToString(SQLdr("PRODUCTCODE"))))
                        End If

                    End While
                End Using
            End Using

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
        End Try
    End Sub
    ''' <summary>
    ''' 荷受人コードリストアイテムを設定
    ''' </summary>
    Private Sub SetConsigneeListItem(selectedValue As String)
        Try
            'リストクリア
            Me.lbConsignee.Items.Clear()
            Dim GBA00004CountryRelated As New GBA00004CountryRelated
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
    ''' 港コードリストアイテムを設定
    ''' </summary>
    Private Sub SetPortListItem(selectedValue As String)
        Try
            'リストクリア
            Me.lbPort.Items.Clear()
            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT PORTCODE")
            sqlStat.AppendLine("      ,AREANAME")
            sqlStat.AppendLine("  FROM GBM0002_PORT")
            sqlStat.AppendLine(" WHERE COMPCODE        = @P1")
            sqlStat.AppendLine("   AND STYMD          <= @P2")
            sqlStat.AppendLine("   AND ENDYMD         >= @P3")
            sqlStat.AppendLine("   AND DELFLG         <> @P4")
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 20)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Date)
                Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Char, 1)
                PARA1.Value = HttpContext.Current.Session("APSRVCamp")
                PARA2.Value = Date.Now
                PARA3.Value = Date.Now
                PARA4.Value = CONST_FLAG_YES
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        'DBからアイテムを設定
                        If COA0019Session.LANGDISP = C_LANG.JA Then
                            Me.lbPort.Items.Add(New ListItem(String.Format("{0}:{1}", SQLdr("PORTCODE"), SQLdr("AREANAME")), Convert.ToString(SQLdr("PORTCODE"))))
                        Else
                            Me.lbPort.Items.Add(New ListItem(String.Format("{0}:{1}", SQLdr("PORTCODE"), SQLdr("AREANAME")), Convert.ToString(SQLdr("PORTCODE"))))
                        End If
                    End While
                End Using
            End Using
            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPort.Items.FindByValue(selectedValue)
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
    ''' 港コードリストアイテムを設定
    ''' </summary>
    Private Sub SetCarrierListItem(selectedValue As String)
        Try
            'リストクリア
            Me.lbCarrier.Items.Clear()
            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT CARRIERCODE")
            sqlStat.AppendLine("     , NAMESJP")
            sqlStat.AppendLine("     , NAMES")
            sqlStat.AppendLine("  FROM GBM0005_TRADER")
            sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
            'sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
            sqlStat.AppendLine("   AND STYMD       <= @STYMD")
            sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
            sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
            'sqlStat.AppendLine("   AND CLASS = 'FORWARDER'")
            sqlStat.AppendLine("   AND CLASS = '" & C_TRADER.CLASS.CARRIER & "'")
            sqlStat.AppendLine("ORDER BY CARRIERCODE ")

            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.Char, 20).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = CONST_FLAG_YES

                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        'DBからアイテムを設定
                        If COA0019Session.LANGDISP = C_LANG.JA Then
                            Me.lbCarrier.Items.Add(New ListItem(String.Format("{0}:{1}", SQLdr("CARRIERCODE"), SQLdr("NAMESJP")), Convert.ToString(SQLdr("CARRIERCODE"))))
                        Else
                            Me.lbCarrier.Items.Add(New ListItem(String.Format("{0}:{1}", SQLdr("CARRIERCODE"), SQLdr("NAMES")), Convert.ToString(SQLdr("CARRIERCODE"))))
                        End If
                    End While
                End Using
            End Using
            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPort.Items.FindByValue(selectedValue)
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
    ''' 発着区分リストアイテムを設定
    ''' </summary>
    Private Sub SetDepartureArrivalListItem(selectedValue As String)
        Try
            Dim COA0017FixValue As New COA0017FixValue

            'リストクリア
            Me.lbDepartureArrival.Items.Clear()

            'リスト設定
            COA0017FixValue.COMPCODE = GBC_COMPCODE_D
            COA0017FixValue.CLAS = "DEPARTUREARRIVAL"
            COA0017FixValue.LISTBOX1 = Me.lbDepartureArrival
            COA0017FixValue.COA0017getListFixValue()

            Me.lbDepartureArrival = DirectCast(COA0017FixValue.LISTBOX1, ListBox)

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbDepartureArrival.Items.Count > 0 Then
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
        If Not (GBA00008Country.ERR = C_MESSAGENO.NORMAL OrElse GBA00008Country.ERR = C_MESSAGENO.NODATA) Then
            returnCode = GBA00008Country.ERR
            Return
        End If
        '一覧先頭にALLを追加
        Me.lbCountry.Items.Insert(0, New ListItem("ALL", "ALL"))
        '正常
        returnCode = C_MESSAGENO.NORMAL
    End Sub
    ''' <summary>
    ''' B/L発行有無変更時イベント
    ''' </summary>
    Public Sub txtBlIssued_Change()
        Me.lblBlIssuedText.Text = ""
        If Me.txtBlIssued.Text.Trim = "" Then
            Return
        End If
        Dim findListItem = Me.lbBlIssued.Items.FindByValue(Me.txtBlIssued.Text)
        If findListItem IsNot Nothing Then
            Me.lblBlIssuedText.Text = findListItem.Text
        Else
            Dim findListItemUpper = Me.lbBlIssued.Items.FindByValue(Me.txtBlIssued.Text.ToUpper)
            If findListItemUpper IsNot Nothing Then
                Me.lblBlIssuedText.Text = findListItemUpper.Text
                Me.txtBlIssued.Text = findListItemUpper.Value
            End If
        End If

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
            SetShipperListItem(Me.txtShipper.Text)
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
            SetConsigneeListItem(Me.txtConsignee.Text)
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
    ''' 積荷港名設定
    ''' </summary>
    Public Sub txtPort_Change()
        Try
            Me.lblPortText.Text = ""
            If Me.txtPort.Text.Trim = "" Then
                Return
            End If
            SetPortListItem(Me.txtPort.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPort.Items.FindByValue(Me.txtPort.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblPortText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbPort.Items.FindByValue(Me.txtPort.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblPortText.Text = parts(1)
                        Me.txtPort.Text = parts(0)
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

            SetProductListItem(Me.txtProduct.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbProduct.Items.Count > 0 Then
                Dim findListItem = Me.lbProduct.Items.FindByValue(Me.txtProduct.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblProductText.Text = parts(1)

                Else
                    Dim findListItemUpper = Me.lbProduct.Items.FindByValue(Me.txtProduct.Text.ToUpper)
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
    ''' 船会社設定
    ''' </summary>
    Public Sub txtCarrier_Change()
        Try
            Me.lblCarrierText.Text = ""
            If Me.txtCarrier.Text.Trim = "" Then
                Return
            End If
            SetCarrierListItem(Me.txtCarrier.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbCarrier.Items.Count > 0 Then
                Dim findListItem = Me.lbCarrier.Items.FindByValue(Me.txtCarrier.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblCarrierText.Text = parts(1)

                Else
                    Dim findListItemUpper = Me.lbCarrier.Items.FindByValue(Me.txtCarrier.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblCarrierText.Text = parts(1)
                        Me.txtCarrier.Text = parts(0)
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
    '''' <summary>
    '''' 発着区分名設定
    '''' </summary>
    'Public Sub txtDepartureArrival_Change()
    '    Try
    '        Me.lblDepartureArrivalText.Text = ""
    '        If Me.txtDepartureArrival.Text.Trim = "" Then
    '            Return
    '        End If

    '        SetDepartureArrivalListItem(Me.txtDepartureArrival.Text)
    '        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbDepartureArrival.Items.Count > 0 Then
    '            Dim findListItem = Me.lbDepartureArrival.Items.FindByValue(Me.txtDepartureArrival.Text)
    '            If findListItem IsNot Nothing Then
    '                Me.lblDepartureArrivalText.Text = findListItem.Text
    '            Else
    '                Dim findListItemUpper = Me.lbDepartureArrival.Items.FindByValue(Me.txtDepartureArrival.Text.ToUpper)
    '                If findListItemUpper IsNot Nothing Then
    '                    Me.lblDepartureArrivalText.Text = findListItemUpper.Text
    '                    Me.txtDepartureArrival.Text = findListItemUpper.Value
    '                End If
    '            End If
    '        End If

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    End Try
    'End Sub
    ''' <summary>
    ''' 固定値マスタよりラジオボタン選択肢を取得
    ''' </summary>
    Private Sub SetSearchTypeListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

        'リストクリア
        Me.rblSearchType.Items.Clear()
        Dim tmpListBoxObj As New ListBox
        'ListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "ORDERSEARCHTYPE"
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
            Me.rblSearchType.Items.Add(item)
        Next

    End Sub
    ''' <summary>
    ''' BL/ISSUEDの選択肢をFIXVALUEより取得し左リストボックスに設定
    ''' </summary>
    Private Sub SetBlIssuedListItem()
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Me.lbBlIssued.Items.Clear()
        'ListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "BLISSUED"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbBlIssued
        Else
            COA0017FixValue.LISTBOX2 = Me.lbBlIssued
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbBlIssued = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbBlIssued = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If
        Else
            returnCode = COA0017FixValue.ERR
            Return
        End If

    End Sub
    ''' <summary>
    ''' 発着区分選択肢を取得
    ''' </summary>
    Private Sub SetDepartureArrivalListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

        'リストクリア
        Me.rblDepartureArrival.Items.Clear()
        Dim tmpListBoxObj As New ListBox
        '発着区分ListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DEPARTUREARRIVAL"
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
            Me.rblDepartureArrival.Items.Add(item)
        Next

    End Sub
End Class