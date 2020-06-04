Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' LEASEブレーカー検索クラス
''' </summary>
Public Class GBT00020SELECT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00020S"     '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00020R"
    Private returnCode As String = String.Empty           'サブ用リターンコード
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 検索画面設定値保持プロパティ
    ''' </summary>
    ''' <returns></returns>
    Public Property ThisScreenValues As GBT00020SValues
    ''' <summary>
    ''' ページロード時
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
                '初期表示
                '****************************************
                '検索設定の選択肢を取得(動的変化のない項目のみ)
                DefaultValueSet()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                Me.txtStYMD.Focus()
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

        '画面遷移先URL取得
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = Me.hdnMapVariant.Value
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        Me.ThisScreenValues = GetDispValue()
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
        COA0011ReturnUrl.VARI = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
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
        AddLangSetting(dicDisplayText, Me.lblYMD1, "リース契約", "Lease")
        AddLangSetting(dicDisplayText, Me.lblYMD2, "範囲指定", "From")
        AddLangSetting(dicDisplayText, Me.lblTilde, "～", "To")
        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主コード", "Shipper Code")
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
        If Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00020RESULT Then

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00020RESULT Then
            Dim prevPage As GBT00020RESULT = DirectCast(Page.PreviousPage, GBT00020RESULT)
            '実行画面からの画面遷移
            Me.SetDispValue(prevPage.GBT00020SValues)
        End If
        'コードを元に名称を設定
        '荷主コード　
        txtShipper_Change()
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
                              {"SHIPPER", Me.txtShipper},
                              {"OFFICE", Me.txtOffice}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnMapVariant.Value
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                item.Value.Text = COA0016VARIget.VALUE
                If {"STYMD", "ENDYMD"}.Contains(item.Key) Then
                    item.Value.Text = FormatDateContrySettings(item.Value.Text, GBA00003UserSetting.DATEFORMAT)
                End If
            Else
                CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
                Return
            End If
        Next

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
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
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
        Dim invCheckTextObjList As New List(Of TextBox) From {txtStYMD, txtEndYMD, txtShipper, txtOffice}
        For Each txtObj In invCheckTextObjList
            COA0008InvalidChar.CHARin = txtObj.Text
            COA0008InvalidChar.COA0008RemoveInvalidChar()
            If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
            Else
                txtObj.Text = COA0008InvalidChar.CHARout
            End If
        Next
        '単項目チェック
        Dim singleCheckTextObjList As New Dictionary(Of String, TextBox) From {{"STYMD", txtStYMD},
                                                                               {"ENDYMD", txtEndYMD},
                                                                               {"SHIPPER", txtShipper},
                                                                               {"OFFICE", txtOffice}}
        For Each txtObj As KeyValuePair(Of String, TextBox) In singleCheckTextObjList
            Dim chkVal As String = txtObj.Value.Text
            If {"STYMD", "ENDYMD"}.Contains(txtObj.Key) Then
                chkVal = FormatDateYMD(chkVal, GBA00003UserSetting.DATEFORMAT)
            End If
            CheckSingle(txtObj.Key, chkVal)
            If returnCode <> C_MESSAGENO.NORMAL Then
                txtObj.Value.Focus()
                Return
            End If
        Next

        '日付整合性チェック
        CheckDate(FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT), FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT))
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtStYMD.Focus()
            Return
        End If

        '荷主コード List存在チェック
        SetShipperListItem(Me.txtShipper.Text)
        CheckList(txtShipper.Text, lbShipper)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtShipper.Focus()
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
    ''' 荷主コードリストアイテムを設定
    ''' </summary>
    Private Sub SetShipperListItem(selectedValue As String)

        Dim GBA00004CountryRelated As New GBA00004CountryRelated

        Try
            'リストクリア
            Me.lbShipper.Items.Clear()

            GBA00004CountryRelated.LISTBOX_SHIPPER = Me.lbShipper
            GBA00004CountryRelated.GBA00004getLeftListShipper()
            If GBA00004CountryRelated.ERR = C_MESSAGENO.NORMAL Then
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
    ''' 当画面の情報を引き渡し用クラスに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDispValue() As GBT00020SValues
        Dim retVal As New GBT00020SValues
        retVal.StYmd = FormatDateYMD(Me.txtStYMD.Text, GBA00003UserSetting.DATEFORMAT)
        retVal.EndYmd = FormatDateYMD(Me.txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT)
        retVal.Shipper = Me.txtShipper.Text
        retVal.Office = Me.txtOffice.Text
        retVal.ViewId = ""
        If Me.lbRightList.SelectedItem IsNot Nothing Then
            retVal.ViewId = Me.lbRightList.SelectedItem.Value
        End If
        Return retVal
    End Function
    ''' <summary>
    ''' 当画面に戻ってきた際に引き渡された情報を展開
    ''' </summary>
    ''' <param name="valClass"></param>
    Private Sub SetDispValue(valClass As GBT00020SValues)
        Me.txtStYMD.Text = FormatDateContrySettings(valClass.StYmd, GBA00003UserSetting.DATEFORMAT)
        Me.txtEndYMD.Text = FormatDateContrySettings(valClass.EndYmd, GBA00003UserSetting.DATEFORMAT)
        Me.txtShipper.Text = valClass.Shipper
        Me.txtOffice.Text = valClass.Office
        If Me.lbRightList.FindControl(valClass.ViewId) IsNot Nothing Then
            Me.lbRightList.SelectedValue = valClass.ViewId
        End If
    End Sub
    ''' <summary>
    ''' GBT00020S(リースブレーカー検索条件)保持用クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00020SValues
        ''' <summary>
        ''' (検索条件)開始日
        ''' </summary>
        ''' <returns></returns>
        Public Property StYmd As String
        ''' <summary>
        ''' (検索条件)終了日
        ''' </summary>
        ''' <returns></returns>
        Public Property EndYmd As String
        ''' <summary>
        ''' (検索条件)荷主
        ''' </summary>
        ''' <returns></returns>
        Public Property Shipper As String
        ''' <summary>
        ''' (検索条件)オフィス
        ''' </summary>
        ''' <returns></returns>
        Public Property Office As String
        ''' <summary>
        ''' (右ボックス)ビューID
        ''' </summary>
        ''' <returns></returns>
        Public Property ViewId As String
    End Class
End Class