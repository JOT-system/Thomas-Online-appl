Imports System.Data.SqlClient
Imports System.Drawing
Imports BASEDLL
''' <summary>
''' TANK ACTIVITY検索画面クラス
''' </summary>
Public Class GBT00004SELECT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00004S"     '自身のMAPID
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
                '他のビューが存在する場合はViewIdでCaseを追加
                'ベンダーコードビュー表示切替
                Case Me.vLeftVender.ID
                    SetVenderListItem(Me.txtVender.Text)
                'タンク番号ビュー表示切替
                Case Me.vLeftTankNo.ID
                    SetTankNoListItem(Me.txtTankNo.Text)
                'ACTYビュー表示切替
                Case Me.vLeftACTY.ID
                    SetACTYListItem(Me.txtACTY.Text)
                'ORDERビュー表示切替
                Case Me.vLeftOrderNo.ID
                    SetOrderNoListItem(Me.txtOrderNo.Text)
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
                Case Me.vLeftVender.ID 'アクティブなビューがベンダーコード
                    'ベンダーコード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbVender.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbVender.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbVender.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblVenderText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblVenderText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftTankNo.ID 'アクティブなビューがタンク番号
                    'タンク番号選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbTankNo.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbTankNo.SelectedItem.Value
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftACTY.ID 'アクティブなビューがACTY
                    'ACTY選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbACTY.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbACTY.SelectedItem.Value
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftOrderNo.ID 'アクティブなビューがオーダー
                    'オーダー番号選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbOrderNo.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbOrderNo.SelectedItem.Value
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
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
        AddLangSetting(dicDisplayText, Me.lblSearchType, "検索種類", "Search Type")
        AddLangSetting(dicDisplayText, Me.lblYMD1, "日付", "ETA")
        AddLangSetting(dicDisplayText, Me.lblYMD2, "範囲指定", "From")
        AddLangSetting(dicDisplayText, Me.lblTilde, "～", "To")
        AddLangSetting(dicDisplayText, Me.lblVender, "ベンダーコード", "Vendor Code")
        AddLangSetting(dicDisplayText, Me.lblTankNo, "タンク番号", "Tank No")
        AddLangSetting(dicDisplayText, Me.lblACTY, "ACTY", "ACTY")
        AddLangSetting(dicDisplayText, Me.lblOrderNo, "オーダーID", "Order ID")
        AddLangSetting(dicDisplayText, Me.lblOffice, "代理店コード", "Office Code")
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

        '選択画面の入力初期値設定
        'メニューから遷移/業務画面戻り判定
        If Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00004ORDER Then

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00004ORDER Then
            Dim prevPage As GBT00004ORDER = DirectCast(Page.PreviousPage, GBT00004ORDER)
            '実行画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            '前画面と当画面のテキストボックス関連ディクショナリ
            Dim dicObjs As New Dictionary(Of String, TextBox) From {{"hdnETAStYMD", Me.txtStYMD},
                                                                    {"hdnETAEndYMD", Me.txtEndYMD},
                                                                    {"hdnVender", Me.txtVender},
                                                                    {"hdnTankNo", Me.txtTankNo},
                                                                    {"hdnACTY", Me.txtACTY},
                                                                    {"hdnOffice", Me.txtOffice},
                                                                    {"hdnOrderNo", Me.txtOrderNo}}

            '{"hdnOrderNo", Me.txtOrderNo},
            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, TextBox) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    If dicObj.Key = "hdnETAStYMD" OrElse dicObj.Key = "hdnETAEndYMD" Then
                        dicObj.Value.Text = FormatDateContrySettings(tmphdnObj.Value, GBA00003UserSetting.DATEFORMAT)
                    Else
                        dicObj.Value.Text = tmphdnObj.Value
                    End If

                End If
            Next
            Dim objSearchType As Control = prevPage.FindControl("hdnSearchType")
            If objSearchType IsNot Nothing Then
                Dim hdnSearchType As HiddenField = DirectCast(objSearchType, HiddenField)
                If Me.rblSearchType.Items.FindByValue(hdnSearchType.Value) IsNot Nothing Then
                    Me.rblSearchType.SelectedValue = hdnSearchType.Value
                End If
            End If

        End If
        'コードを元に名称を設定
        'ベンダーコード　
        txtVender_Change()
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
                              {"VENDER", Me.txtVender}, {"TANKNO", Me.txtTankNo},
                              {"ACTY", Me.txtACTY}, {"ORDERNO", Me.txtOrderNo},
                              {"OFFICE", Me.txtOffice}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                If item.Key = "STYMD" Then
                    item.Value.Text = FormatDateContrySettings(COA0016VARIget.VALUE, GBA00003UserSetting.DATEFORMAT)
                ElseIf item.Key = "ENDYMD" Then
                    item.Value.Text = FormatDateContrySettings(Date.Parse(COA0016VARIget.VALUE).AddMonths(1).ToString("yyyy/MM/dd"), GBA00003UserSetting.DATEFORMAT)
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
        COA0018ViewList.FORWARDMATCHVARIANT = Me.hdnThisMapVariant.Value
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

        '開始日
        COA0008InvalidChar.CHARin = txtStYMD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtStYMD.Text = COA0008InvalidChar.CHARout
        End If

        '終了日
        COA0008InvalidChar.CHARin = txtEndYMD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtEndYMD.Text = COA0008InvalidChar.CHARout
        End If

        'ベンダーコード
        COA0008InvalidChar.CHARin = txtVender.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtVender.Text = COA0008InvalidChar.CHARout
        End If

        'タンク番号
        COA0008InvalidChar.CHARin = txtTankNo.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtTankNo.Text = COA0008InvalidChar.CHARout
        End If

        'ACTY
        COA0008InvalidChar.CHARin = txtACTY.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtACTY.Text = COA0008InvalidChar.CHARout
        End If

        'ORDER No.
        COA0008InvalidChar.CHARin = txtOrderNo.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtOrderNo.Text = COA0008InvalidChar.CHARout
        End If

        '代理店コード
        COA0008InvalidChar.CHARin = txtOffice.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtOffice.Text = COA0008InvalidChar.CHARout
        End If

        '入力項目チェック

        '開始日 単項目チェック
        CheckSingle("STYMD", FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT))
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtStYMD.Focus()
            Return
        End If

        '終了日 単項目チェック
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

        'ベンダーコード 単項目チェック
        CheckSingle("VENDER", txtVender.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtVender.Focus()
            Return
        End If

        'ベンダーコード List存在チェック
        SetVenderListItem(txtVender.Text)
        CheckList(txtVender.Text, lbVender)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtVender.Focus()
            Return
        End If

        'タンク番号 単項目チェック
        CheckSingle("TANKNO", txtTankNo.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtTankNo.Focus()
            Return
        End If

        'タンク番号 List存在チェック
        SetTankNoListItem(txtTankNo.Text)
        CheckList(txtTankNo.Text, lbTankNo)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtTankNo.Focus()
            Return
        End If

        'ACTYコード 単項目チェック
        CheckSingle("ACTY", txtACTY.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtACTY.Focus()
            Return
        End If

        'ACTYコード List存在チェック
        SetACTYListItem(txtACTY.Text)
        CheckList(txtACTY.Text, lbACTY)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtACTY.Focus()
            Return
        End If

        'ORDER No 単項目チェック
        CheckSingle("ORDERNO", txtOrderNo.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtOrderNo.Focus()
            Return
        End If

        'ORDER No List存在チェック
        SetOrderNoListItem(txtOrderNo.Text)
        CheckList(txtOrderNo.Text, lbOrderNo)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtOrderNo.Focus()
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
    ''' ベンダーコードリストアイテムを設定
    ''' </summary>
    Private Sub SetVenderListItem(selectedValue As String)

        Dim GBA00004CountryRelated As GBA00004CountryRelated = New GBA00004CountryRelated

        Try
            'リストクリア
            Me.lbVender.Items.Clear()
            GBA00004CountryRelated.LISTBOX_OTHER = lbVender
            GBA00004CountryRelated.GBA00004getLeftListOther()
            If GBA00004CountryRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00004CountryRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbVender = DirectCast(GBA00004CountryRelated.LISTBOX_OTHER, ListBox)
            Else
                returnCode = GBA00004CountryRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbVender.Items.Count > 0 Then
                Dim findListItem = Me.lbVender.Items.FindByValue(selectedValue)
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
    ''' タンク番号リストアイテムを設定
    ''' </summary>
    Private Sub SetTankNoListItem(selectedValue As String)
        Try
            'リストクリア
            Me.lbTankNo.Items.Clear()

            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT rtrim(TANKNO) as TANKNO")
            sqlStat.AppendLine("  FROM GBM0006_TANK ")
            sqlStat.AppendLine(" WHERE  COMPCODE     = @COMPCODE ")
            sqlStat.AppendLine("   AND STYMD        <= @STYMD")
            sqlStat.AppendLine("   AND ENDYMD       >= @ENDYMD")
            sqlStat.AppendLine("   AND  DELFLG      <> @DELFLG ")
            sqlStat.AppendLine(" ORDER BY TANKNO ")
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
                        Me.lbTankNo.Items.Add(Convert.ToString(SQLdr("TANKNO")))
                    End While
                End Using
            End Using

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbTankNo.Items.Count > 0 Then
                Dim findListItem = Me.lbTankNo.Items.FindByValue(selectedValue)
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
    ''' ACTYリストアイテムを設定
    ''' </summary>
    Private Sub SetACTYListItem(selectedValue As String)
        Try
            'リストクリア
            Me.lbACTY.Items.Clear()
            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT DISTINCT rtrim(ACTIONID) as ACTIONID")
            sqlStat.AppendLine("  FROM GBM0009_TRPATTERN")
            sqlStat.AppendLine(" WHERE DELFLG        <> @P1")
            sqlStat.AppendLine(" AND   ACTIONID      <> '' ")
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Char, 1)
                PARA1.Value = CONST_FLAG_YES
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        Me.lbACTY.Items.Add(Convert.ToString(SQLdr("ACTIONID")))
                    End While
                End Using
            End Using
            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbACTY.Items.Count > 0 Then
                Dim findListItem = Me.lbACTY.Items.FindByValue(selectedValue)
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
    ''' ORDER No.リストアイテムを設定
    ''' </summary>
    Private Sub SetOrderNoListItem(selectedValue As String)
        Try
            'リストクリア
            Me.lbOrderNo.Items.Clear()
            '検索SQL文
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT rtrim(ORDERNO) as ORDERNO")
            sqlStat.AppendLine("  FROM GBT0004_ODR_BASE")
            sqlStat.AppendLine(" WHERE STYMD          <= @P1")
            sqlStat.AppendLine("   AND ENDYMD         >= @P2")
            sqlStat.AppendLine("   AND DELFLG         <> @P3")
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
                Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
                Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.Char, 1)
                PARA1.Value = Date.Now
                PARA2.Value = Date.Now
                PARA3.Value = CONST_FLAG_YES
                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        Me.lbOrderNo.Items.Add(Convert.ToString(SQLdr("ORDERNO")))
                    End While
                End Using
            End Using
            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbOrderNo.Items.Count > 0 Then
                Dim findListItem = Me.lbOrderNo.Items.FindByValue(selectedValue)
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
    ''' ベンダー名設定
    ''' </summary>
    Public Sub txtVender_Change()
        Try
            Me.lblVenderText.Text = ""
            If Me.txtVender.Text.Trim = "" Then
                Return
            End If
            SetVenderListItem(Me.txtVender.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbVender.Items.Count > 0 Then
                Dim findListItem = Me.lbVender.Items.FindByValue(Me.txtVender.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblVenderText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbVender.Items.FindByValue(Me.txtVender.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblVenderText.Text = parts(1)
                        Me.txtVender.Text = parts(0)
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
    Private Sub SetSearchTypeListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

        'リストクリア
        Me.rblSearchType.Items.Clear()
        Dim tmpListBoxObj As New ListBox
        'ユーザＩＤListBox設定
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
End Class