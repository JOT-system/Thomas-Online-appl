Imports System.Data.SqlClient
Imports System.Drawing
Imports BASEDLL
''' <summary>
''' 取引先検索画面クラス
''' </summary>
Public Class GBT00028SELECT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00028S"       '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00028"
    Private returnCode As String = String.Empty             'サブ用リターンコード

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 検索画面設定値保持プロパティ
    ''' </summary>
    ''' <returns></returns>
    Public Property ThisScreenValues As GBT00028SValues

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
                '使用可否制御
                '****************************************
                enabledControls()
                '****************************************
                'フォーカス設定
                '****************************************
                txtInvoiceMonth.Focus()
                '****************************************
                'セッション設定
                '****************************************

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
        Catch ex As System.Threading.ThreadAbortException
            Return
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.SYSTEMADM
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
                'InvoiceMonthビュー表示切替
                Case Me.vLeftInvoiceMonth.ID
                    SetInvoiceMonthListItem(Me.txtInvoiceMonth.Text)
                'Customerビュー表示切替
                Case Me.vLeftCustomer.ID
                    SetCustomerListItem(Me.txtCustomer.Text)
                'POLビュー表示切替
                Case Me.vLeftPOL.ID
                    SetPOLListItem(Me.txtPOL.Text)
                'PODビュー表示切替
                Case Me.vLeftPOD.ID
                    SetPODListItem(Me.txtPOD.Text)
                'Productビュー表示切替
                Case Me.vLeftProduct.ID
                    SetProductListItem(Me.txtProduct.Text)

            End Select
        End If

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時
    ''' </summary>
    Public Sub btnEnter_Click()
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl

        'チェック処理
        If Me.hdnThisMapVariant.Value = "Management" Then
            checkProc2()
        Else
            checkProc()
        End If
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
        Me.ThisScreenValues = GetDispValue()
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
                Case Me.vLeftInvoiceMonth.ID 'アクティブなビューがCustomer
                    'InvoiceMonth選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbInvoiceMonth.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbInvoiceMonth.SelectedItem.Value
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftCustomer.ID 'アクティブなビューがCustomer
                    'Customer選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCustomer.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbCustomer.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbCustomer.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblCustomerText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblCustomerText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftPOL.ID 'アクティブなビューがPOL
                    'POL選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbPOL.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbPOL.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbPOL.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblPOLText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblPOLText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftPOD.ID 'アクティブなビューがPOD
                    'POD選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbPOD.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbPOD.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbPOD.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblPODText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblPODText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftProduct.ID 'アクティブなビューがProduct
                    'Product選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbProduct.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbProduct.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbProduct.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblProductText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblProductText.Text = ""
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
        AddLangSetting(dicDisplayText, Me.lblInvoiceMonth, "請求月", "Invoice Month")
        AddLangSetting(dicDisplayText, Me.lblCustomer, "顧客コード", "Customer Code")
        AddLangSetting(dicDisplayText, Me.lblPOL, "ＰＯＬ", "POL")
        AddLangSetting(dicDisplayText, Me.lblPOD, "ＰＯＤ", "POD")
        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品コード", "Product Code")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")
        AddLangSetting(dicDisplayText, Me.lblRightListDiscription, "画面レイアウト設定", "Screen Layout")

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
        If Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00028RESULT Then

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            SetInvoiceMonthListItem(Me.txtInvoiceMonth.Text)
            'If Me.hdnThisMapVariant.Value = "Management" Then
            '    Me.txtInvoiceMonth.Text = lbInvoiceMonth.Items(lbInvoiceMonth.Items.Count - 3).Value
            'Else
            Me.txtInvoiceMonth.Text = lbInvoiceMonth.Items(lbInvoiceMonth.Items.Count - 2).Value
            'End If

        ElseIf TypeOf Page.PreviousPage Is GBT00028RESULT Then
            Dim prevPage As GBT00028RESULT = DirectCast(Page.PreviousPage, GBT00028RESULT)
            '実行画面からの画面遷移
            Me.SetDispValue(prevPage.GBT00028SValues)

            'prevViewID = DirectCast(prevPage.FindControl("hdnViewId"), HiddenField).Value

        End If

        'コードを元に名称を設定
        'Invoice Month
        SetInvoiceMonthListItem(Me.txtInvoiceMonth.Text)
        'Customer
        txtCustomer_Change()
        'POL
        txtPOL_Change()
        'POD
        txtPOD_Change()
        'Product
        txtProduct_Change()

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
    ''' 使用可否コントロール
    ''' </summary>
    Private Sub enabledControls()

        'メニューから遷移/業務画面戻り判定
        If Me.hdnThisMapVariant.Value = "Management" Then

            '請求書管理画面であれば下記抽出条件非表示
            Me.lblCustomer.Text = ""
            Me.lblPOL.Text = ""
            Me.lblPOD.Text = ""
            Me.lblProduct.Text = ""

            '請求書管理の場合、以下の項目を非表示
            Dim lstInputObjects As New List(Of Control) From {Me.txtCustomer, Me.txtPOL, Me.txtPOD, Me.txtProduct}

            For Each obj As Control In lstInputObjects
                If TypeOf obj Is TextBox Then
                    Dim txtObj As TextBox = DirectCast(obj, TextBox)
                    txtObj.Visible = False
                End If
            Next
        Else
            '請求書作成画面であれば顧客コード必須
            Me.lblCustomer.CssClass = "requiredMark"
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
                        From {
                              }
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
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

        'Custmer(取引先)
        COA0008InvalidChar.CHARin = txtCustomer.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtCustomer.Text = COA0008InvalidChar.CHARout
        End If

        'POL
        COA0008InvalidChar.CHARin = txtPOL.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtPOL.Text = COA0008InvalidChar.CHARout
        End If

        'POD
        COA0008InvalidChar.CHARin = txtPOD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtPOD.Text = COA0008InvalidChar.CHARout
        End If

        'Product
        COA0008InvalidChar.CHARin = txtProduct.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtProduct.Text = COA0008InvalidChar.CHARout
        End If

        '入力項目チェック
        '①単項目チェック
        'Customer
        CheckSingle("CUSTOMERCODE", txtCustomer.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtCustomer.Focus()
            Return
        End If

        'POL
        CheckSingle("POL", txtPOL.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPOL.Focus()
            Return
        End If

        'POD
        CheckSingle("POD", txtPOD.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPOD.Focus()
            Return
        End If

        'Product
        CheckSingle("PRODUCTCODE", txtProduct.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtProduct.Focus()
            Return
        End If

        '②存在チェック(LeftBoxチェック)
        'Invoice Month
        CheckList(txtInvoiceMonth.Text, lbInvoiceMonth)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtInvoiceMonth.Focus()
            Return
        End If

        'Customer
        CheckList(txtCustomer.Text, lbCustomer)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtCustomer.Focus()
            Return
        End If

        'POL
        CheckList(txtPOL.Text, lbPOL)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPOL.Focus()
            Return
        End If

        'POD
        CheckList(txtPOD.Text, lbPOD)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPOD.Focus()
            Return
        End If

        'Product
        CheckList(txtProduct.Text, lbProduct)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtProduct.Focus()
            Return
        End If

    End Sub

    ''' <summary>
    ''' チェック処理(請求月チェックのみ)
    ''' </summary>
    Public Sub checkProc2()

        '②存在チェック(LeftBoxチェック)
        'Invoice Month
        CheckList(txtInvoiceMonth.Text, lbInvoiceMonth)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtInvoiceMonth.Focus()
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
                returnCode = C_MESSAGENO.UNSELECTABLEERR
                CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                                            messageParams:=New List(Of String) From {String.Format("VALUE:{0}", inText)})
            End If
        End If
    End Sub

    ''' <summary>
    ''' InvoiceMonthリストアイテムを設定
    ''' </summary>
    Private Sub SetInvoiceMonthListItem(selectedValue As String)

        'リストクリア
        Me.lbInvoiceMonth.Items.Clear()
        Try

            '検索SQL文
            Dim initMonth As String = ""
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("DECLARE @STARTDATE nvarchar(10);")
            sqlStat.AppendLine("DECLARE @ENDDATE nvarchar(10);")
            sqlStat.AppendLine("SELECT @STARTDATE =  DATEADD(M,1,CONVERT(date,MAX(CD.REPORTMONTH) + '/01'))")
            sqlStat.AppendLine("FROM GBT0006_CLOSINGDAY CD")
            sqlStat.AppendLine("WHERE  CD.COUNTRYCODE = 'JOT'")
            sqlStat.AppendLine("AND    CD.DELFLG     <> @DELFLG;")
            sqlStat.AppendLine("SELECT @ENDDATE =  DATEADD(m,1,CONVERT(DATETIME, @STARTDATE));")
            If Me.hdnThisMapVariant.Value = "Management" Then
                ' 管理の場合は、システム導入時から
                sqlStat.AppendLine("SELECT @STARTDATE =  DATEADD(M,1,CONVERT(date,MIN(CD.REPORTMONTH) + '/01'))")
                sqlStat.AppendLine("FROM GBT0006_CLOSINGDAY CD")
                sqlStat.AppendLine("WHERE  CD.COUNTRYCODE = 'JOT'")
                sqlStat.AppendLine("AND    CD.DELFLG     <> @DELFLG;")
                'sqlStat.AppendLine("SELECT @INVOICEDATE =  '2020/01/01';")
                'sqlStat =
            End If
            sqlStat.AppendLine("WITH DateTable(MyDate) AS ( ")
            sqlStat.AppendLine("  SELECT")
            sqlStat.AppendLine("    CONVERT(DATETIME, @STARTDATE)")
            sqlStat.AppendLine("  UNION ALL ")
            sqlStat.AppendLine("  SELECT")
            sqlStat.AppendLine("    DATEADD(m, 1, MyDate) ")
            sqlStat.AppendLine("  FROM")
            sqlStat.AppendLine("    DateTable ")
            sqlStat.AppendLine("  WHERE")
            sqlStat.AppendLine("    MyDate < @ENDDATE")
            sqlStat.AppendLine(") ")
            sqlStat.AppendLine("SELECT CONVERT(CHAR(7),MyDate,111) as 'CODE', CONVERT(CHAR(7),MyDate,111) as 'NAME',")
            sqlStat.AppendLine("       CONVERT(CHAR(7),MyDate,111) as 'DISPLAYNAME' FROM DateTable")
            Dim retDt As New DataTable
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(SQLcmd)
                    sqlDa.Fill(retDt)
                End Using 'sqlDa
                If retDt IsNot Nothing Then
                    With Me.lbInvoiceMonth
                        .DataValueField = "CODE"
                        .DataTextField = "DISPLAYNAME"
                        .DataSource = retDt
                        .DataBind()
                    End With
                End If
            End Using

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbInvoiceMonth.Items.Count > 0 Then
                Dim findListItem = Me.lbInvoiceMonth.Items.FindByValue(selectedValue)
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
    ''' Customer(取引先)リストアイテムを設定
    ''' </summary>
    Private Sub SetCustomerListItem(selectedValue As String)

        'リストクリア
        Me.lbCustomer.Items.Clear()
        Try
            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("select distinct tm.TORICODE as 'CODE',")
            sqlStat.AppendLine("                tm.NAMES1      as 'NAME',")
            sqlStat.AppendLine("                tm.TORICODE + ':' + tm.NAMES1 as 'DISPLAYNAME'")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE ov ")
            sqlStat.AppendLine("    inner join GBT0004_ODR_BASE ob ")
            sqlStat.AppendLine("      on ob.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and ob.ORDERNO = ov.ORDERNO")
            sqlStat.AppendLine("    inner join GBM0004_CUSTOMER cm ")
            sqlStat.AppendLine("      on cm.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and cm.CUSTOMERCODE = ov.CONTRACTORFIX")
            sqlStat.AppendLine("      and cm.STYMD <= getdate()")
            sqlStat.AppendLine("      and cm.ENDYMD >= getdate()")
            sqlStat.AppendLine("    inner join GBM0025_TORI tm ")
            sqlStat.AppendLine("      on tm.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and tm.COMPCODE = '01'")
            sqlStat.AppendLine("      and tm.TORIKBN = 'I'")
            sqlStat.AppendLine("      and tm.TORICODE = cm.INCTORICODE")
            sqlStat.AppendLine("      and tm.STYMD <= getdate()")
            sqlStat.AppendLine("      and tm.ENDYMD >= getdate()")
            sqlStat.AppendLine("  where ov.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  and   ov.INVOICEDBY = 'JPA00001' ")
            sqlStat.AppendLine("  and   ov.TANKNO    <> '' ")
            sqlStat.AppendLine("  and   ")
            sqlStat.AppendLine("  ( ")
            sqlStat.AppendLine("    (       ov.COSTCODE = 'A0001-01' ")
            sqlStat.AppendLine("      and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("      and   ov.BRID like 'BT%' ")
            sqlStat.AppendLine("    ) ")
            ' リース追加
            sqlStat.AppendLine("    or ")
            sqlStat.AppendLine("    (       ov.COSTCODE like 'S0103%' ")
            sqlStat.AppendLine("      and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("    ) ")
            sqlStat.AppendLine("  ) ")
            ' 指定済み
            'If selectedValue <> "" Then
            '    sqlStat.AppendLine("  and   ov.CONTRACTORFIX = @CUSTOMER ")
            'End If
            If Me.txtPOL.Text <> "" Then
                sqlStat.AppendLine("  and   ob.LOADPORT1 = @POL ")
            End If
            If Me.txtPOD.Text <> "" Then
                sqlStat.AppendLine("  and   ob.DISCHARGEPORT1 = @POD ")
            End If
            If Me.txtProduct.Text <> "" Then
                sqlStat.AppendLine("  and   ob.PRODUCTCODE = @PRODUCT ")
            End If

            sqlStat.AppendLine(" ORDER BY tm.TORICODE, tm.NAMES1, tm.TORICODE + ':' + tm.NAMES1 ")
            Dim retDt As New DataTable
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    If selectedValue <> "" Then
                        .Add("@CUSTOMER", System.Data.SqlDbType.NVarChar).Value = selectedValue
                    End If
                    If Me.txtPOL.Text <> "" Then
                        .Add("@POL", System.Data.SqlDbType.NVarChar).Value = Me.txtPOL.Text
                    End If
                    If Me.txtPOD.Text <> "" Then
                        .Add("@POD", System.Data.SqlDbType.NVarChar).Value = Me.txtPOD.Text
                    End If
                    If Me.txtProduct.Text <> "" Then
                        .Add("@PRODUCT", System.Data.SqlDbType.NVarChar).Value = Me.txtProduct.Text
                    End If
                End With

                Using sqlDa As New SqlDataAdapter(SQLcmd)
                    sqlDa.Fill(retDt)
                End Using 'sqlDa
                If retDt IsNot Nothing Then
                    With Me.lbCustomer
                        .DataValueField = "CODE"
                        .DataTextField = "DISPLAYNAME"
                        .DataSource = retDt
                        .DataBind()
                    End With
                End If
            End Using

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbCustomer.Items.Count > 0 Then
                Dim findListItem = Me.lbCustomer.Items.FindByValue(selectedValue)
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
    ''' Customer(取引先)名設定
    ''' </summary>
    Public Sub txtCustomer_Change()

        Try
            Me.lblCustomerText.Text = ""

            SetCustomerListItem(Me.txtCustomer.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbCustomer.Items.Count > 0 Then
                Dim findListItem = Me.lbCustomer.Items.FindByValue(Me.txtCustomer.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblCustomerText.Text = parts(1)
                    Else
                        Me.lblCustomerText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbCustomer.Items.FindByValue(Me.txtCustomer.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblCustomerText.Text = parts(1)
                            Me.txtCustomer.Text = parts(0)
                        Else
                            Me.lblCustomerText.Text = findListItemUpper.Text
                            Me.txtCustomer.Text = findListItemUpper.Value
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
    ''' POLリストアイテムを設定
    ''' </summary>
    Private Sub SetPOLListItem(selectedValue As String)

        'リストクリア
        Me.lbPOL.Items.Clear()
        Try
            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("select distinct ob.LOADPORT1 as 'CODE',")
            sqlStat.AppendLine("                pm.AREANAME as 'NAME',")
            sqlStat.AppendLine("                ob.LOADPORT1 + ':' + pm.AREANAME as 'DISPLAYNAME'")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE ov ")
            sqlStat.AppendLine("    inner join GBT0004_ODR_BASE ob ")
            sqlStat.AppendLine("      on ob.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and ob.ORDERNO = ov.ORDERNO")
            sqlStat.AppendLine("    inner join GBM0002_PORT pm ")
            sqlStat.AppendLine("      on pm.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and pm.COUNTRYCODE = ob.LOADCOUNTRY1")
            sqlStat.AppendLine("      and pm.PORTCODE = ob.LOADPORT1")
            sqlStat.AppendLine("      and pm.STYMD <= getdate()")
            sqlStat.AppendLine("      and pm.ENDYMD >= getdate()")

            If Me.txtCustomer.Text <> "" Then
                sqlStat.AppendLine("    inner join GBM0004_CUSTOMER cm ")
                sqlStat.AppendLine("      on cm.DELFLG <> @DELFLG")
                sqlStat.AppendLine("      and cm.CUSTOMERCODE = ov.CONTRACTORFIX")
                sqlStat.AppendLine("      and cm.INCTORICODE = @INCTORICODE")
                sqlStat.AppendLine("      and cm.STYMD <= getdate()")
                sqlStat.AppendLine("      and cm.ENDYMD >= getdate()")
            End If

            sqlStat.AppendLine("  where ov.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  and   ov.INVOICEDBY = 'JPA00001' ")
            sqlStat.AppendLine("  and   ")
            sqlStat.AppendLine("  ( ")
            sqlStat.AppendLine("    (       ov.COSTCODE = 'A0001-01' ")
            sqlStat.AppendLine("      and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("      and   ov.BRID like 'BT%' ")
            sqlStat.AppendLine("    ) ")
            ' リース追加
            sqlStat.AppendLine("    or ")
            sqlStat.AppendLine("    (       ov.COSTCODE like 'S0103%' ")
            sqlStat.AppendLine("      and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("    ) ")
            sqlStat.AppendLine("  ) ")

            'If Me.txtCustomer.Text <> "" Then
            '    sqlStat.AppendLine("  and   ov.CONTRACTORFIX = @CUSTOMER ")
            'End If
            ' 指定済み
            'If selectedValue <> "" Then
            '    sqlStat.AppendLine("  and   ob.LOADPORT1 = @POL ")
            'End If
            If Me.txtPOD.Text <> "" Then
                sqlStat.AppendLine("  and   ob.DISCHARGEPORT1 = @POD ")
            End If
            If Me.txtProduct.Text <> "" Then
                sqlStat.AppendLine("  and   ob.PRODUCTCODE = @PRODUCT ")
            End If

            sqlStat.AppendLine(" ORDER BY ob.LOADPORT1, pm.AREANAME, ob.LOADPORT1 + ':' + pm.AREANAME ")
            Dim retDt As New DataTable
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    If Me.txtCustomer.Text <> "" Then
                        '.Add("@CUSTOMER", System.Data.SqlDbType.NVarChar).Value = Me.txtCustomer.Text
                        .Add("@INCTORICODE", System.Data.SqlDbType.NVarChar).Value = Me.txtCustomer.Text
                    End If
                    If selectedValue <> "" Then
                        .Add("@POL", System.Data.SqlDbType.NVarChar).Value = selectedValue
                    End If
                    If Me.txtPOD.Text <> "" Then
                        .Add("@POD", System.Data.SqlDbType.NVarChar).Value = Me.txtPOD.Text
                    End If
                    If Me.txtProduct.Text <> "" Then
                        .Add("@PRODUCT", System.Data.SqlDbType.NVarChar).Value = Me.txtProduct.Text
                    End If
                End With

                Using sqlDa As New SqlDataAdapter(SQLcmd)
                    sqlDa.Fill(retDt)
                End Using 'sqlDa
                If retDt IsNot Nothing Then
                    With Me.lbPOL
                        .DataValueField = "CODE"
                        .DataTextField = "DISPLAYNAME"
                        .DataSource = retDt
                        .DataBind()
                    End With
                End If

            End Using

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPOL.Items.Count > 0 Then
                Dim findListItem = Me.lbPOL.Items.FindByValue(selectedValue)
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
    ''' POL名設定
    ''' </summary>
    Public Sub txtPOL_Change()

        Try
            Me.lblPOLText.Text = ""

            SetPOLListItem(Me.txtPOL.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPOL.Items.Count > 0 Then
                Dim findListItem = Me.lbPOL.Items.FindByValue(Me.txtPOL.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblPOLText.Text = parts(1)
                    Else
                        Me.lblPOLText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbPOL.Items.FindByValue(Me.txtPOL.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblPOLText.Text = parts(1)
                            Me.txtPOL.Text = parts(0)
                        Else
                            Me.lblPOLText.Text = findListItemUpper.Text
                            Me.txtPOL.Text = findListItemUpper.Value
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
    ''' PODリストアイテムを設定
    ''' </summary>
    Private Sub SetPODListItem(selectedValue As String)

        'リストクリア
        Me.lbPOD.Items.Clear()
        Try
            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("select distinct ob.DISCHARGEPORT1 as 'CODE',")
            sqlStat.AppendLine("                pm.AREANAME as 'NAME',")
            sqlStat.AppendLine("                ob.DISCHARGEPORT1 + ':' + pm.AREANAME as 'DISPLAYNAME'")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE ov ")
            sqlStat.AppendLine("    inner join GBT0004_ODR_BASE ob ")
            sqlStat.AppendLine("      on ob.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and ob.ORDERNO = ov.ORDERNO")
            sqlStat.AppendLine("    inner join GBM0002_PORT pm ")
            sqlStat.AppendLine("      on pm.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and pm.COUNTRYCODE = ob.DISCHARGECOUNTRY1")
            sqlStat.AppendLine("      and pm.PORTCODE = ob.DISCHARGEPORT1")
            sqlStat.AppendLine("      and pm.STYMD <= getdate()")
            sqlStat.AppendLine("      and pm.ENDYMD >= getdate()")

            If Me.txtCustomer.Text <> "" Then
                sqlStat.AppendLine("    inner join GBM0004_CUSTOMER cm ")
                sqlStat.AppendLine("      on cm.DELFLG <> @DELFLG")
                sqlStat.AppendLine("      and cm.CUSTOMERCODE = ov.CONTRACTORFIX")
                sqlStat.AppendLine("      and cm.INCTORICODE = @INCTORICODE")
                sqlStat.AppendLine("      and cm.STYMD <= getdate()")
                sqlStat.AppendLine("      and cm.ENDYMD >= getdate()")
            End If

            sqlStat.AppendLine("  where ov.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  and   ov.INVOICEDBY = 'JPA00001' ")
            sqlStat.AppendLine("  and   ")
            sqlStat.AppendLine("  ( ")
            sqlStat.AppendLine("    (       ov.COSTCODE = 'A0001-01' ")
            sqlStat.AppendLine("      and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("      and   ov.BRID like 'BT%' ")
            sqlStat.AppendLine("    ) ")
            ' リース追加
            sqlStat.AppendLine("    or ")
            sqlStat.AppendLine("    (       ov.COSTCODE like 'S0103%' ")
            sqlStat.AppendLine("      and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("    ) ")
            sqlStat.AppendLine("  ) ")

            'If Me.txtCustomer.Text <> "" Then
            '    sqlStat.AppendLine("  and   ov.CONTRACTORFIX = @CUSTOMER ")
            'End If
            If Me.txtPOL.Text <> "" Then
                sqlStat.AppendLine("  and   ob.LOADPORT1 = @POL ")
            End If
            ' 指定済み
            'If selectedValue <> "" Then
            '    sqlStat.AppendLine("  and   ob.DISCHARGEPORT1 = @POD ")
            'End If
            If Me.txtProduct.Text <> "" Then
                sqlStat.AppendLine("  and   ob.PRODUCTCODE = @PRODUCT ")
            End If

            sqlStat.AppendLine(" ORDER BY ob.DISCHARGEPORT1, pm.AREANAME, ob.DISCHARGEPORT1 + ':' + pm.AREANAME ")
            Dim retDt As New DataTable
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    If Me.txtCustomer.Text <> "" Then
                        '.Add("@CUSTOMER", System.Data.SqlDbType.NVarChar).Value = Me.txtCustomer.Text
                        .Add("@INCTORICODE", System.Data.SqlDbType.NVarChar).Value = Me.txtCustomer.Text
                    End If
                    If Me.txtPOL.Text <> "" Then
                        .Add("@POL", System.Data.SqlDbType.NVarChar).Value = Me.txtPOL.Text
                    End If
                    If selectedValue <> "" Then
                        .Add("@POD", System.Data.SqlDbType.NVarChar).Value = selectedValue
                    End If
                    If Me.txtProduct.Text <> "" Then
                        .Add("@PRODUCT", System.Data.SqlDbType.NVarChar).Value = Me.txtProduct.Text
                    End If
                End With

                Using sqlDa As New SqlDataAdapter(SQLcmd)
                    sqlDa.Fill(retDt)
                End Using 'sqlDa
                If retDt IsNot Nothing Then
                    With Me.lbPOD
                        .DataValueField = "CODE"
                        .DataTextField = "DISPLAYNAME"
                        .DataSource = retDt
                        .DataBind()
                    End With
                End If
            End Using

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPOD.Items.Count > 0 Then
                Dim findListItem = Me.lbPOD.Items.FindByValue(selectedValue)
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
    ''' POD名設定
    ''' </summary>
    Public Sub txtPOD_Change()

        Try
            Me.lblPODText.Text = ""

            SetPODListItem(Me.txtPOD.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPOD.Items.Count > 0 Then
                Dim findListItem = Me.lbPOD.Items.FindByValue(Me.txtPOD.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblPODText.Text = parts(1)
                    Else
                        Me.lblPODText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbPOD.Items.FindByValue(Me.txtPOD.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblPODText.Text = parts(1)
                            Me.txtPOD.Text = parts(0)
                        Else
                            Me.lblPODText.Text = findListItemUpper.Text
                            Me.txtPOD.Text = findListItemUpper.Value
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
    ''' Productリストアイテムを設定
    ''' </summary>
    Private Sub SetProductListItem(selectedValue As String)

        'リストクリア
        Me.lbProduct.Items.Clear()
        Try
            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("select distinct ob.PRODUCTCODE as 'CODE',")
            sqlStat.AppendLine("                pm.PRODUCTNAME as 'NAME',")
            sqlStat.AppendLine("                ob.PRODUCTCODE + ':' + pm.PRODUCTNAME as 'DISPLAYNAME'")
            sqlStat.AppendLine("  from GBT0005_ODR_VALUE ov ")
            sqlStat.AppendLine("    inner join GBT0004_ODR_BASE ob ")
            sqlStat.AppendLine("      on ob.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and ob.ORDERNO = ov.ORDERNO")
            sqlStat.AppendLine("    inner join GBM0008_PRODUCT pm ")
            sqlStat.AppendLine("      on pm.DELFLG <> @DELFLG")
            sqlStat.AppendLine("      and pm.COMPCODE = @COMPCODE")
            sqlStat.AppendLine("      and pm.PRODUCTCODE = ob.PRODUCTCODE")
            sqlStat.AppendLine("      and pm.STYMD <= getdate()")
            sqlStat.AppendLine("      and pm.ENDYMD >= getdate()")

            If Me.txtCustomer.Text <> "" Then
                sqlStat.AppendLine("    inner join GBM0004_CUSTOMER cm ")
                sqlStat.AppendLine("      on cm.DELFLG <> @DELFLG")
                sqlStat.AppendLine("      and cm.CUSTOMERCODE = ov.CONTRACTORFIX")
                sqlStat.AppendLine("      and cm.INCTORICODE = @INCTORICODE")
                sqlStat.AppendLine("      and cm.STYMD <= getdate()")
                sqlStat.AppendLine("      and cm.ENDYMD >= getdate()")
            End If

            sqlStat.AppendLine("  where ov.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  and   ov.INVOICEDBY = 'JPA00001' ")
            sqlStat.AppendLine("  and   ")
            sqlStat.AppendLine("  ( ")
            sqlStat.AppendLine("    (       ov.COSTCODE = 'A0001-01' ")
            sqlStat.AppendLine("      and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("      and   ov.BRID like 'BT%' ")
            sqlStat.AppendLine("    ) ")
            ' リース追加
            sqlStat.AppendLine("    or ")
            sqlStat.AppendLine("    (       ov.COSTCODE like 'S0103%' ")
            sqlStat.AppendLine("      and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("    ) ")
            sqlStat.AppendLine("  ) ")

            'If Me.txtCustomer.Text <> "" Then
            '    sqlStat.AppendLine("  and   ov.CONTRACTORFIX = @CUSTOMER ")
            'End If
            If Me.txtPOL.Text <> "" Then
                sqlStat.AppendLine("  and   ob.LOADPORT1 = @POL ")
            End If
            If Me.txtPOD.Text <> "" Then
                sqlStat.AppendLine("  and   ob.DISCHARGEPORT1 = @POD ")
            End If
            ' 指定済み
            'If selectedValue <> "" Then
            '    sqlStat.AppendLine("  and   ob.PRODUCTCODE = @PRODUCT ")
            'End If

            sqlStat.AppendLine(" ORDER BY ob.PRODUCTCODE, pm.PRODUCTNAME, ob.PRODUCTCODE + ':' + pm.PRODUCTNAME ")
            Dim retDt As New DataTable
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    If Me.txtCustomer.Text <> "" Then
                        '.Add("@CUSTOMER", System.Data.SqlDbType.NVarChar).Value = Me.txtCustomer.Text
                        .Add("@INCTORICODE", System.Data.SqlDbType.NVarChar).Value = Me.txtCustomer.Text
                    End If
                    If Me.txtPOL.Text <> "" Then
                        .Add("@POL", System.Data.SqlDbType.NVarChar).Value = Me.txtPOL.Text
                    End If
                    If Me.txtPOD.Text <> "" Then
                        .Add("@POD", System.Data.SqlDbType.NVarChar).Value = Me.txtPOD.Text
                    End If
                    If selectedValue <> "" Then
                        .Add("@PRODUCT", System.Data.SqlDbType.NVarChar).Value = selectedValue
                    End If
                End With

                Using sqlDa As New SqlDataAdapter(SQLcmd)
                    sqlDa.Fill(retDt)
                End Using 'sqlDa
                If retDt IsNot Nothing Then
                    With Me.lbProduct
                        .DataValueField = "CODE"
                        .DataTextField = "DISPLAYNAME"
                        .DataSource = retDt
                        .DataBind()
                    End With
                End If
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
    ''' Product名設定
    ''' </summary>
    Public Sub txtProduct_Change()

        Try
            Me.lblProductText.Text = ""

            SetProductListItem(Me.txtProduct.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbProduct.Items.Count > 0 Then
                Dim findListItem = Me.lbProduct.Items.FindByValue(Me.txtProduct.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblProductText.Text = parts(1)
                    Else
                        Me.lblProductText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbProduct.Items.FindByValue(Me.txtProduct.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblProductText.Text = parts(1)
                            Me.txtProduct.Text = parts(0)
                        Else
                            Me.lblProductText.Text = findListItemUpper.Text
                            Me.txtProduct.Text = findListItemUpper.Value
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
    ''' GBT00028S(請求書検索条件)保持用クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00028SValues
        ''' <summary>
        ''' (検索条件)請求月
        ''' </summary>
        ''' <returns></returns>
        Public Property InvoiceMonth As String
        ''' <summary>
        ''' (検索条件)顧客コード
        ''' </summary>
        ''' <returns></returns>
        Public Property CustomerCode As String
        ''' <summary>
        ''' (検索条件)顧客名(取引先)
        ''' </summary>
        ''' <returns></returns>
        Public Property CustomerName As String
        ''' <summary>
        ''' (検索条件)POL
        ''' </summary>
        ''' <returns></returns>
        Public Property POL As String
        ''' <summary>
        ''' (検索条件)POD
        ''' </summary>
        ''' <returns></returns>
        Public Property POD As String
        ''' <summary>
        ''' (検索条件)積載品
        ''' </summary>
        ''' <returns></returns>
        Public Property ProductCode As String
        ''' <summary>
        ''' (右ボックス)ビューID
        ''' </summary>
        ''' <returns></returns>
        Public Property ViewId As String
    End Class

    ''' <summary>
    ''' 当画面の情報を引き渡し用クラスに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDispValue() As GBT00028SValues
        Dim retVal As New GBT00028SValues
        retVal.InvoiceMonth = Me.txtInvoiceMonth.Text
        retVal.CustomerCode = Me.txtCustomer.Text
        retVal.CustomerName = Me.lblCustomerText.Text
        retVal.POL = Me.txtPOL.Text
        retVal.POD = Me.txtPOD.Text
        retVal.ProductCode = Me.txtProduct.Text
        retVal.ViewId = Me.hdnThisMapVariant.Value
        'If Me.lbRightList.SelectedItem IsNot Nothing Then
        '    retVal.ViewId = Me.lbRightList.SelectedItem.Value
        'End If
        Return retVal
    End Function

    ''' <summary>
    ''' 当画面に戻ってきた際に引き渡された情報を展開
    ''' </summary>
    ''' <param name="valClass"></param>
    Private Sub SetDispValue(valClass As GBT00028SValues)
        Me.txtInvoiceMonth.Text = valClass.InvoiceMonth
        Me.txtCustomer.Text = valClass.CustomerCode
        Me.txtPOL.Text = valClass.POL
        Me.txtPOD.Text = valClass.POD
        Me.txtProduct.Text = valClass.ProductCode
        If Me.lbRightList.FindControl(valClass.ViewId) IsNot Nothing Then
            Me.lbRightList.SelectedValue = valClass.ViewId
        End If
    End Sub

End Class
