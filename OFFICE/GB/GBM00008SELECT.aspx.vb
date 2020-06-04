Imports System.Data.SqlClient
Imports System.Drawing
Imports BASEDLL
''' <summary>
''' 積載品マスタ検索画面クラス
''' </summary>
Public Class GBM00008SELECT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00008S" '自身のMAPID
    Private Const CONST_BASEID As String = "GBM00008"
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
            lblFooterMessage.ForeColor = Color.Black
            lblFooterMessage.Font.Bold = False

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
                DefaultValueSet()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                txtStYMD.Focus()

            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
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
        Catch ex As System.Threading.ThreadAbortException
            Return
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
                ''国コードビュー表示切替
                'Case Me.vLeftCountry.ID
                '    SetCountryListItem(Me.txtCountry.Text)
                ''顧客コードビュー表示切替
                'Case Me.vLeftShipper.ID
                '    SetShipperListItem(Me.txtShipper.Text)
                '積載品コードビュー表示切替
                Case Me.vLeftProduct.ID
                    SetProductListItem(Me.txtProduct.Text)
                '国連番号コードビュー表示切替
                Case Me.vLeftUNNO.ID
                    SetUNNOListItem(Me.txtUNNO.Text)
                '有効フラグビュー表示切替
                Case Me.vLeftEnabled.ID
                    SetEnabledListItem(Me.txtEnabled.Text)
                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        If Me.hdnTextDbClickField.Value = "txtEndYMD" Then
                            targetObject = FindControl("txtStYMD")
                            Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                            Me.hdnCalendarValue.Value = FormatDateYMD(txtobj.Text, GBA00003UserSetting.DATEFORMAT)

                            Me.mvLeft.Focus()
                        Else
                            Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                            Me.hdnCalendarValue.Value = FormatDateYMD(txtobj.Text, GBA00003UserSetting.DATEFORMAT)

                            Me.mvLeft.Focus()
                        End If
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

        '画面遷移先URL取得
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = Me.hdnThisMapVariant.Value
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value

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
                'Case Me.vLeftCountry.ID 'アクティブなビューが国コード
                '    '国コード選択時
                '    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                '    If targetObject IsNot Nothing Then
                '        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                '        If Me.lbCountry.SelectedItem IsNot Nothing Then
                '            txtobj.Text = Me.lbCountry.SelectedItem.Value
                '            Dim parts As String()
                '            parts = Split(Me.lbCountry.SelectedItem.Text, ":", -1, CompareMethod.Text)
                '            Me.lblCountryText.Text = parts(1)
                '            txtobj.Focus()
                '        Else
                '            txtobj.Text = ""
                '            Me.lblCountryText.Text = ""
                '            txtobj.Focus()
                '        End If
                '    End If
                'Case Me.vLeftShipper.ID 'アクティブなビューが顧客コード
                '    '顧客コード選択時
                '    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                '    If targetObject IsNot Nothing Then
                '        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                '        If Me.lbShipper.SelectedItem IsNot Nothing Then
                '            txtobj.Text = Me.lbShipper.SelectedItem.Value
                '            Dim parts As String()
                '            parts = Split(Me.lbShipper.SelectedItem.Text, ":", -1, CompareMethod.Text)
                '            Me.lblShipperText.Text = parts(1)
                '            txtobj.Focus()
                '        Else
                '            txtobj.Text = ""
                '            Me.lblShipperText.Text = ""
                '            txtobj.Focus()
                '        End If
                '    End If
                Case Me.vLeftProduct.ID 'アクティブなビューが積載品コード
                    '積載品コード選択時
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
                Case Me.vLeftUNNO.ID 'アクティブなビューが国連番号コード
                    '国連番号コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbUNNO.SelectedItem IsNot Nothing Then

                            Dim parts As String()
                            parts = Split(Me.lbUNNO.SelectedItem.Text, ",", -1, CompareMethod.Text)
                            txtobj.Text = parts(0)
                            Me.lblUNNOText.Text = Me.lbUNNO.SelectedItem.Attributes("data_names")

                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblUNNOText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftEnabled.ID 'アクティブなビューが国連番号コード
                    '有効フラグ選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbEnabled.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbEnabled.SelectedItem.Value
                            Me.lblEnabledText.Text = Me.lbEnabled.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblEnabledText.Text = ""
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
        AddLangSetting(dicDisplayText, Me.lblYMD1, "有効年月日", "Effective Date")
        AddLangSetting(dicDisplayText, Me.lblYMD2, "範囲指定", "From")
        AddLangSetting(dicDisplayText, Me.lblTilde, "～", "To")
        'AddLangSetting(dicDisplayText, Me.lblCountry, "国コード", "Country Code")
        'AddLangSetting(dicDisplayText, Me.lblShipper, "顧客コード", "Shipper Code")
        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品コード", "Product Code")
        AddLangSetting(dicDisplayText, Me.lblUNNO, "国連番号", "UNNO")
        AddLangSetting(dicDisplayText, Me.lblEnabled, "有効フラグ", "Validity Flag")
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

        Dim prevViewID As String = Nothing

        '選択画面の入力初期値設定
        'メニューから遷移/業務画面戻り判定
        If TypeOf Page.PreviousPage Is COM00002MENU Then

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        Else
            '実行画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            Dim prevSelectProductPage As GBM00008PRODUCT = DirectCast(Page.PreviousPage, GBM00008PRODUCT)

            '有効年月日
            txtStYMD.Text = FormatDateContrySettings(DirectCast(prevSelectProductPage.FindControl("hdnSelectedStYMD"), HiddenField).Value, GBA00003UserSetting.DATEFORMAT)
            txtEndYMD.Text = FormatDateContrySettings(DirectCast(prevSelectProductPage.FindControl("hdnSelectedEndYMD"), HiddenField).Value, GBA00003UserSetting.DATEFORMAT)
            ''国コード　
            'txtCountry.Text = DirectCast(prevSelectProductPage.FindControl("hdnSelectedCountryCode"), HiddenField).Value
            'txtCountry_Change()

            ''顧客コード　
            'txtShipper.Text = DirectCast(prevSelectProductPage.FindControl("hdnSelectedCustomerCode"), HiddenField).Value
            'txtShipper_Change()

            '積載品コード　
            txtProduct.Text = DirectCast(prevSelectProductPage.FindControl("hdnSelectedProductCode"), HiddenField).Value
            txtProduct_Change()

            '国連番号　
            txtUNNO.Text = DirectCast(prevSelectProductPage.FindControl("hdnUnNo"), HiddenField).Value
            txtUNNO_Change()

            '有効フラグ
            txtEnabled.Text = DirectCast(prevSelectProductPage.FindControl("hdnEnabled"), HiddenField).Value
            txtEnabled_Change()

            prevViewID = DirectCast(prevSelectProductPage.FindControl("hdnViewId"), HiddenField).Value

        End If

        'RightBox情報設定
        rightBoxSet()
        If prevViewID IsNot Nothing Then
            For i As Integer = 0 To lbRightList.Items.Count - 1
                If lbRightList.Items(i).Value = prevViewID Then
                    lbRightList.SelectedIndex = i
                End If
            Next
        End If
        If returnCode <> C_MESSAGENO.NORMAL Then
            Return
        End If

    End Sub
    ''' <summary>
    ''' 変数設定
    ''' </summary>
    Public Sub variableSet()
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget        '変数情報取

        '有効開始日
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "STYMD"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtStYMD.Text = FormatDateContrySettings(COA0016VARIget.VALUE, GBA00003UserSetting.DATEFORMAT)
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            Return
        End If

        '有効終了日
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "ENDYMD"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtEndYMD.Text = FormatDateContrySettings(COA0016VARIget.VALUE, GBA00003UserSetting.DATEFORMAT)
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            Return
        End If

        ''国コード
        'COA0016VARIget.MAPID = CONST_MAPID
        'COA0016VARIget.COMPCODE = ""
        'COA0016VARIget.VARI = HttpContext.Current.Session("MAPvariant")
        'COA0016VARIget.FIELD = "COUNTRY"
        'COA0016VARIget.COA0016VARIget()
        'If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
        '    txtCountry.Text = COA0016VARIget.VALUE
        'Else
        '    CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
        '    Return
        'End If

        ''顧客コード
        'COA0016VARIget.MAPID = CONST_MAPID
        'COA0016VARIget.COMPCODE = ""
        'COA0016VARIget.VARI = HttpContext.Current.Session("MAPvariant")
        'COA0016VARIget.FIELD = "SHIPPER"
        'COA0016VARIget.COA0016VARIget()
        'If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
        '    txtShipper.Text = COA0016VARIget.VALUE
        'Else
        '    CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
        '    Return
        'End If

        '積載品コード
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "PRODUCT"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtProduct.Text = COA0016VARIget.VALUE
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            Return
        End If

        '国連番号
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "UNNO"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtUNNO.Text = COA0016VARIget.VALUE
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            Return
        End If

        '有効フラグ
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "ENABLED"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtEnabled.Text = COA0016VARIget.VALUE
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
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
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

        ''国コード
        'COA0008InvalidChar.CHARin = txtCountry.Text
        'COA0008InvalidChar.COA0008RemoveInvalidChar()
        'If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        'Else
        '    txtCountry.Text = COA0008InvalidChar.CHARout
        'End If

        ''顧客コード
        'COA0008InvalidChar.CHARin = txtShipper.Text
        'COA0008InvalidChar.COA0008RemoveInvalidChar()
        'If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        'Else
        '    txtShipper.Text = COA0008InvalidChar.CHARout
        'End If

        '積載品コード
        COA0008InvalidChar.CHARin = txtProduct.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtProduct.Text = COA0008InvalidChar.CHARout
        End If

        '国連番号コード
        COA0008InvalidChar.CHARin = txtUNNO.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtUNNO.Text = COA0008InvalidChar.CHARout
        End If

        '有効フラグ
        COA0008InvalidChar.CHARin = txtEnabled.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtEnabled.Text = COA0008InvalidChar.CHARout
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

        ''国コード 単項目チェック
        'CheckSingle("COUNTRY", txtCountry.Text)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    txtCountry.Focus()
        '    Return
        'End If

        ''国コード List存在チェック
        'CheckList(txtCountry.Text, lbCountry, "")
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    txtCountry.Focus()
        '    Return
        'End If

        ''顧客コード 単項目チェック
        'CheckSingle("SHIPPER", txtShipper.Text)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    txtShipper.Focus()
        '    Return
        'End If

        ''顧客コード List存在チェック
        'CheckList(txtShipper.Text, lbShipper, "")
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    txtShipper.Focus()
        '    Return
        'End If

        '積載品コード 単項目チェック
        CheckSingle("PRODUCT", txtProduct.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtProduct.Focus()
            Return
        End If

        '積載品コード List存在チェック
        CheckList(txtProduct.Text, lbProduct, "")
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtProduct.Focus()
            Return
        End If

        '国連番号コード 単項目チェック
        CheckSingle("UNNO", txtUNNO.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtUNNO.Focus()
            Return
        End If

        '国連番号コード List存在チェック
        CheckList(txtUNNO.Text, lbUNNO, "UNNO")
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtUNNO.Focus()
            Return
        End If

        '有効フラグ 単項目チェック
        CheckSingle("ENABLED", txtEnabled.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtEnabled.Focus()
            Return
        End If

        '有効フラグ List存在チェック
        CheckList(txtEnabled.Text, lbEnabled, "")
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtEnabled.Focus()
            Return
        End If

    End Sub
    ''' <summary>
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="inColName"></param>
    ''' <param name="inText"></param>
    Protected Sub CheckSingle(ByVal inColName As String, ByVal inText As String)
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck              '項目チェック
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
    Protected Sub CheckList(ByVal inText As String, ByVal inList As ListBox, ByVal itm As String)
        Dim flag As Boolean = False

        If inText <> "" Then
            Select Case itm
                Case "UNNO"

                    For i As Integer = 0 To inList.Items.Count - 1
                        Dim parts As String()
                        parts = Split(inList.Items(i).Value, ",", -1, CompareMethod.Text)

                        If parts(0) = inText Then
                            flag = True
                            Exit For
                        End If
                    Next

                Case Else

                    For i As Integer = 0 To inList.Items.Count - 1
                        If inList.Items(i).Value = inText Then
                            flag = True
                            Exit For
                        End If
                    Next
            End Select

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
        Dim wkDateStart As Date = Nothing
        Dim wkDateEnd As Date = Nothing
        Date.TryParse(inStYMD, wkDateStart)
        Date.TryParse(inEndYMD, wkDateEnd)

        If wkDateStart > wkDateEnd Then
            returnCode = C_MESSAGENO.VALIDITYINPUT
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
        End If

    End Sub
    '''' <summary>
    '''' 国コードリストアイテムを設定
    '''' </summary>
    'Private Sub SetCountryListItem(selectedValue As String)
    '    Dim GBA00007OrganizationRelated As New GBA00007OrganizationRelated

    '    Try

    '        'リストクリア
    '        Me.lbCountry.Items.Clear()

    '        GBA00007OrganizationRelated.LISTBOX_COUNTRY = Me.lbCountry
    '        GBA00007OrganizationRelated.GBA00007getLeftListCountry()
    '        If GBA00007OrganizationRelated.ERR = C_MESSAGENO.NORMAL Then
    '            Me.lbCountry = DirectCast(GBA00007OrganizationRelated.LISTBOX_COUNTRY, ListBox)
    '        Else
    '            returnCode = GBA00007OrganizationRelated.ERR
    '            Return
    '        End If

    '        '一応現在入力しているテキストと一致するものを選択状態
    '        If Me.lbCountry.Items.Count > 0 Then
    '            Dim findListItem = Me.lbCountry.Items.FindByValue(selectedValue)
    '            If findListItem IsNot Nothing Then
    '                findListItem.Selected = True
    '            End If
    '        End If

    '        '正常
    '        returnCode = C_MESSAGENO.NORMAL

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    End Try
    'End Sub
    '''' <summary>
    '''' 顧客コードリストアイテムを設定
    '''' </summary>
    'Private Sub SetShipperListItem(selectedValue As String)
    '    Dim GBA00004CountryRelated As New GBA00004CountryRelated

    '    Try

    '        'リストクリア
    '        Me.lbShipper.Items.Clear()

    '        If Me.txtCountry.Text <> "" Then
    '            GBA00004CountryRelated.COUNTRYCODE = Me.txtCountry.Text
    '        End If
    '        GBA00004CountryRelated.LISTBOX_SHIPPER = Me.lbShipper
    '        GBA00004CountryRelated.GBA00004getLeftListShipper()
    '        If GBA00004CountryRelated.ERR = C_MESSAGENO.NORMAL Then
    '            Me.lbShipper = DirectCast(GBA00004CountryRelated.LISTBOX_SHIPPER, ListBox)
    '        Else
    '            returnCode = GBA00004CountryRelated.ERR
    '            Return
    '        End If

    '        '一応現在入力しているテキストと一致するものを選択状態
    '        If Me.lbShipper.Items.Count > 0 Then
    '            Dim findListItem = Me.lbShipper.Items.FindByValue(selectedValue)
    '            If findListItem IsNot Nothing Then
    '                findListItem.Selected = True
    '            End If
    '        End If

    '        '正常
    '        returnCode = C_MESSAGENO.NORMAL

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
    ''' 国連番号リストアイテムを設定
    ''' </summary>
    Private Sub SetUNNOListItem(selectedValue As String)

        Dim GBA00001UnNo As New GBA00001UnNo              '項目チェック
        'リストクリア
        Me.lbUNNO.Items.Clear()

        'リスト設定
        GBA00001UnNo.LISTBOX = Me.lbUNNO
        GBA00001UnNo.GBA00001getLeftListUnNo()
        If GBA00001UnNo.ERR = C_MESSAGENO.NORMAL Then
            Me.lbUNNO = GBA00001UnNo.LISTBOX

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbUNNO.Items.Count > 0 Then
                Dim findListItem = Me.lbUNNO.Items.FindByText(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL
        ElseIf GBA00001UnNo.ERR = C_MESSAGENO.NODATA Then
            'UNNOデータ未取得の場合は素通り
            returnCode = C_MESSAGENO.NODATA

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00001UnNo.ERR)})
        End If

    End Sub
    ''' <summary>
    ''' 有効フラグリストアイテムを設定
    ''' </summary>
    Private Sub SetEnabledListItem(selectedValue As String)

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

        'リストクリア
        Me.lbEnabled.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "ENABLED"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbEnabled
        Else
            COA0017FixValue.LISTBOX2 = Me.lbEnabled
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbEnabled = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbEnabled = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

        Else
            returnCode = COA0017FixValue.ERR
            Return
        End If

    End Sub
    '''' <summary>
    '''' 国名設定
    '''' </summary>
    'Public Sub txtCountry_Change()

    '    Try
    '        Me.lblCountryText.Text = ""

    '        SetCountryListItem(Me.txtCountry.Text)
    '        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbCountry.Items.Count > 0 Then
    '            Dim findListItem = Me.lbCountry.Items.FindByValue(Me.txtCountry.Text)
    '            If findListItem IsNot Nothing Then
    '                Dim parts As String()
    '                parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
    '                Me.lblCountryText.Text = parts(1)
    '            Else
    '                Dim findListItemUpper = Me.lbCountry.Items.FindByValue(Me.txtCountry.Text.ToUpper)
    '                If findListItemUpper IsNot Nothing Then
    '                    Dim parts As String()
    '                    parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
    '                    Me.lblCountryText.Text = parts(1)
    '                    Me.txtCountry.Text = parts(0)
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
    '''' <summary>
    '''' 顧客名設定
    '''' </summary>
    'Public Sub txtShipper_Change()

    '    Try
    '        Me.lblShipperText.Text = ""

    '        SetShipperListItem(Me.txtShipper.Text)
    '        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShipper.Items.Count > 0 Then
    '            Dim findListItem = Me.lbShipper.Items.FindByValue(Me.txtShipper.Text)
    '            If findListItem IsNot Nothing Then
    '                Dim parts As String()
    '                parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
    '                Me.lblShipperText.Text = parts(1)
    '            Else
    '                Dim findListItemUpper = Me.lbShipper.Items.FindByValue(Me.txtShipper.Text.ToUpper)
    '                If findListItemUpper IsNot Nothing Then
    '                    Dim parts As String()
    '                    parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
    '                    Me.lblShipperText.Text = parts(1)
    '                    Me.txtShipper.Text = parts(0)
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
    ''' 積載品名設定
    ''' </summary>
    Public Sub txtProduct_Change()

        Try
            Me.lblProductText.Text = ""

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
    ''' 国連番号名設定
    ''' </summary>
    Public Sub txtUNNO_Change()

        'Try
        '    Me.lblUNNOText.Text = ""

        '    SetUNNOListItem(Me.txtUNNO.Text)
        '    If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbUNNO.Items.Count > 0 Then
        '        Dim findListItem = Me.lbUNNO.Items.FindByValue(Me.txtUNNO.Text)
        '        If findListItem IsNot Nothing Then
        '            Me.lblUNNOText.Text = findListItem.Text
        '        Else
        '            Dim findListItemUpper = Me.lbUNNO.Items.FindByValue(Me.txtUNNO.Text.ToUpper)
        '            If findListItemUpper IsNot Nothing Then
        '                Me.lblUNNOText.Text = findListItemUpper.Text
        '                Me.txtUNNO.Text = findListItemUpper.Value
        '            End If
        '        End If
        '    End If

        'Catch ex As Exception
        '    returnCode = C_MESSAGENO.EXCEPTION
        '    COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
        '    COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
        '    COA0003LogFile.TEXT = ex.ToString()
        '    COA0003LogFile.MESSAGENO = returnCode
        '    COA0003LogFile.COA0003WriteLog()
        'End Try

    End Sub
    ''' <summary>
    ''' 有効フラグ名設定
    ''' </summary>
    Public Sub txtEnabled_Change()

        Try
            Me.lblEnabledText.Text = ""

            SetEnabledListItem(Me.txtEnabled.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbEnabled.Items.Count > 0 Then
                Dim findListItem = Me.lbEnabled.Items.FindByValue(Me.txtEnabled.Text)
                If findListItem IsNot Nothing Then
                    Me.lblEnabledText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbEnabled.Items.FindByValue(Me.txtEnabled.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblEnabledText.Text = findListItemUpper.Text
                        Me.txtEnabled.Text = findListItemUpper.Value
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