Imports System.Data.SqlClient
Imports System.Drawing
Imports BASEDLL
''' <summary>
''' ORDER検索画面クラス
''' </summary>
Public Class GBT00003SELECT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00003S"     '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00003"
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
                txtETDStYMD.Focus()
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
                    If Me.hdnTextDbClickField.Value = "txtPortOfLoading" Then
                        SetPortListItem(Me.txtPortOfLoading.Text)
                    Else
                        SetPortListItem(Me.txtPortOfDischarge.Text)
                    End If
                '代理店コードビュー表示切替
                Case Me.vLeftOffice.ID
                    SetOfficeListItem(Me.txtOffice.Text)
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
        If Me.txtETAStYMD.Text <> "" AndAlso Me.txtETAEndYMD.Text = "" Then
            Me.txtETAEndYMD.Text = Me.txtETAStYMD.Text
        End If

        If Me.txtETAEndYMD.Text <> "" AndAlso Me.txtETAStYMD.Text = "" Then
            Me.txtETAStYMD.Text = Me.txtETAEndYMD.Text
        End If

        If Me.txtETDStYMD.Text <> "" AndAlso Me.txtETDEndYMD.Text = "" Then
            Me.txtETDEndYMD.Text = Me.txtETDStYMD.Text
        End If

        If Me.txtETDEndYMD.Text <> "" AndAlso Me.txtETDStYMD.Text = "" Then
            Me.txtETDStYMD.Text = Me.txtETDEndYMD.Text
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
                        If txtobj.ClientID = "txtPortOfLoading" Then
                            If Me.lbPort.SelectedItem IsNot Nothing Then
                                txtobj.Text = Me.lbPort.SelectedItem.Value
                                Dim parts As String()
                                parts = Split(Me.lbPort.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblPortOfLoadingText.Text = parts(1)
                                txtobj.Focus()
                            Else
                                txtobj.Text = ""
                                Me.lblPortOfLoadingText.Text = ""
                                txtobj.Focus()
                            End If
                        Else
                            If Me.lbPort.SelectedItem IsNot Nothing Then
                                txtobj.Text = Me.lbPort.SelectedItem.Value
                                Dim parts As String()
                                parts = Split(Me.lbPort.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblPortOfDischargeText.Text = parts(1)
                                txtobj.Focus()
                            Else
                                txtobj.Text = ""
                                Me.lblPortOfDischargeText.Text = ""
                                txtobj.Focus()
                            End If

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
        AddLangSetting(dicDisplayText, Me.lblETD1, "出港予定日", "ETD")
        AddLangSetting(dicDisplayText, Me.lblETD2, "範囲指定", "From")
        AddLangSetting(dicDisplayText, Me.lblETDTilde, "～", "To")
        AddLangSetting(dicDisplayText, Me.lblETA1, "入港予定日", "ETA")
        AddLangSetting(dicDisplayText, Me.lblETA2, "範囲指定", "From")
        AddLangSetting(dicDisplayText, Me.lblETATilde, "～", "To")
        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主コード", "Shipper Code")
        AddLangSetting(dicDisplayText, Me.lblConsignee, "荷受人コード", "Consignee Code")
        AddLangSetting(dicDisplayText, Me.lblPortOfLoading, "出港コード", "Port Of Loading Code")
        AddLangSetting(dicDisplayText, Me.lblPortOfDischarge, "入港コード", "Port Of Discharge Code")
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
        Dim repId As String = ""
        '選択画面の入力初期値設定
        'メニューから遷移/業務画面戻り判定
        If Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00003RESULT Then

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00003RESULT Then
            Dim prevPage As GBT00003RESULT = DirectCast(Page.PreviousPage, GBT00003RESULT)
            '実行画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            '前画面と当画面のテキストボックス関連ディクショナリ
            Dim dicObjs As New Dictionary(Of String, TextBox) From {{"hdnETDStYMD", Me.txtETDStYMD},
                                                                    {"hdnETDEndYMD", Me.txtETDEndYMD},
                                                                    {"hdnETAStYMD", Me.txtETAStYMD},
                                                                    {"hdnETAEndYMD", Me.txtETAEndYMD},
                                                                    {"hdnShipper", Me.txtShipper},
                                                                    {"hdnConsignee", Me.txtConsignee},
                                                                    {"hdnPortOfLoading", Me.txtPortOfLoading},
                                                                    {"hdnPortOfDischarge", Me.txtPortOfDischarge},
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
            Dim tmpRepIdHedden As HiddenField = DirectCast(prevPage.FindControl("hdnReportVariant"), HiddenField)
            If tmpRepIdHedden IsNot Nothing AndAlso tmpRepIdHedden.Value <> "" Then
                repId = tmpRepIdHedden.Value
            End If
        End If
        'コードを元に名称を設定
        '荷主コード　
        txtShipper_Change()
        '荷受人コード　
        txtConsignee_Change()
        '出港コード　
        txtPortOfLoading_Change()
        '入港コード
        txtPortOfDischarge_Change()
        '代理店コード
        txtOffice_Change()
        'RightBox情報設定
        rightBoxSet()
        If returnCode <> C_MESSAGENO.NORMAL Then
            Return
        End If
        If repId <> "" Then
            Dim findResult As ListItem = Me.lbRightList.Items.FindByValue(repId)
            If findResult IsNot Nothing Then
                'findResult.Selected = True
                Me.lbRightList.SelectedValue = findResult.Value
            End If
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
                        From {{"ETDSTYMD", Me.txtETDStYMD}, {"ETDENDYMD", Me.txtETDEndYMD},
                              {"ETASTYMD", Me.txtETAStYMD}, {"ETAENDYMD", Me.txtETAEndYMD},
                              {"SHIPPER", Me.txtShipper}, {"CONSIGNEE", Me.txtConsignee},
                              {"PORTOFLOADING", Me.txtPortOfLoading}, {"PORTOFDISCHARGE", Me.txtPortOfDischarge},
                              {"OFFICE", Me.txtOffice}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                If item.Key = "ETDENDYMD" Then
                    If COA0016VARIget.VALUE <> "" Then
                        item.Value.Text = Date.Parse(COA0016VARIget.VALUE).AddMonths(1).ToString(GBA00003UserSetting.DATEFORMAT)
                    Else
                        item.Value.Text = COA0016VARIget.VALUE
                    End If
                ElseIf item.Key = "ETDSTYMD" OrElse item.Key = "ETASTYMD" OrElse item.Key = "ETAENDYMD" Then
                    If COA0016VARIget.VALUE <> "" Then
                        item.Value.Text = Date.Parse(COA0016VARIget.VALUE).ToString(GBA00003UserSetting.DATEFORMAT)
                    Else
                        item.Value.Text = COA0016VARIget.VALUE
                    End If
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
        COA0016VARIget.VARI = hdnThisMapVariant.Value
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

        'ETA開始日
        COA0008InvalidChar.CHARin = txtETAStYMD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtETAStYMD.Text = COA0008InvalidChar.CHARout
        End If

        'ETA終了日
        COA0008InvalidChar.CHARin = txtETAEndYMD.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtETAEndYMD.Text = COA0008InvalidChar.CHARout
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

        '出港コード
        COA0008InvalidChar.CHARin = txtPortOfLoading.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtPortOfLoading.Text = COA0008InvalidChar.CHARout
        End If

        '入港コード
        COA0008InvalidChar.CHARin = txtPortOfDischarge.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtPortOfDischarge.Text = COA0008InvalidChar.CHARout
        End If

        '代理店コード
        COA0008InvalidChar.CHARin = txtOffice.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtOffice.Text = COA0008InvalidChar.CHARout
        End If

        '入力項目チェック

        'ETD開始日 単項目チェック
        Dim etdStStr As String
        Dim etdStDate As Date
        If Date.TryParseExact(Me.txtETDStYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etdStDate) Then
            etdStStr = etdStDate.ToString("yyyy/MM/dd")
        Else
            etdStStr = Me.txtETDStYMD.Text
        End If

        CheckSingle("ETDSTYMD", etdStStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETDStYMD.Focus()
            Return
        End If

        'ETD終了日 単項目チェック
        Dim etdEndStr As String
        Dim etdEndDate As Date
        If Date.TryParseExact(Me.txtETDEndYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etdEndDate) Then
            etdEndStr = etdEndDate.ToString("yyyy/MM/dd")
        Else
            etdEndStr = Me.txtETDEndYMD.Text
        End If

        CheckSingle("ETDENDYMD", etdEndStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETDEndYMD.Focus()
            Return
        End If

        'ETD日付整合性チェック
        CheckDate(etdStStr, etdEndStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETDStYMD.Focus()
            Return
        End If

        'ETA開始日 単項目チェック
        Dim etaStStr As String
        Dim etaStDate As Date
        If Date.TryParseExact(Me.txtETAStYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etaStDate) Then
            etaStStr = etaStDate.ToString("yyyy/MM/dd")
        Else
            etaStStr = Me.txtETAStYMD.Text
        End If

        CheckSingle("ETASTYMD", etaStStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETAStYMD.Focus()
            Return
        End If

        'ETA終了日 単項目チェック
        Dim etaEndStr As String
        Dim etaEndDate As Date
        If Date.TryParseExact(Me.txtETAEndYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etaEndDate) Then
            etaEndStr = etaEndDate.ToString("yyyy/MM/dd")
        Else
            etaEndStr = Me.txtETAEndYMD.Text
        End If

        CheckSingle("ETAENDYMD", etaEndStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETAEndYMD.Focus()
            Return
        End If

        '日付整合性チェック
        CheckDate(etaStStr, etaEndStr)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETAStYMD.Focus()
            Return
        End If

        '出港入港必須チェック
        ETDETAMustCheck()
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtETDStYMD.Focus()
            Return
        End If

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
        CheckSingle("PORTOFLOADING", txtPortOfLoading.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPortOfLoading.Focus()
            Return
        End If

        '積荷港コード List存在チェック
        CheckList(txtPortOfLoading.Text, lbPort)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPortOfLoading.Focus()
            Return
        End If

        '荷揚港コード 単項目チェック
        CheckSingle("PORTOFDISCHARGE", txtPortOfDischarge.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPortOfDischarge.Focus()
            Return
        End If

        '荷揚港コード List存在チェック
        CheckList(txtPortOfDischarge.Text, lbPort)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPortOfDischarge.Focus()
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
    ''' 出港日入港日必須チェック
    ''' </summary>
    Protected Sub ETDETAMustCheck()

        If (txtETDStYMD.Text Is Nothing OrElse txtETDStYMD.Text = "") AndAlso
           (txtETAStYMD.Text Is Nothing OrElse txtETAStYMD.Text = "") Then
            returnCode = C_MESSAGENO.REQUIREDVALUE
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
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
    Public Sub txtPortOfLoading_Change()
        Try
            Me.lblPortOfLoadingText.Text = ""
            If Me.txtPortOfLoading.Text.Trim = "" Then
                Return
            End If
            SetPortListItem(Me.txtPortOfLoading.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPort.Items.FindByValue(Me.txtPortOfLoading.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblPortOfLoadingText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbPort.Items.FindByValue(Me.txtPortOfLoading.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblPortOfLoadingText.Text = parts(1)
                        Me.txtPortOfLoading.Text = parts(0)
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
    ''' 荷揚港名設定
    ''' </summary>
    Public Sub txtPortOfDischarge_Change()
        Try
            Me.lblPortOfDischargeText.Text = ""
            If Me.txtPortOfDischarge.Text.Trim = "" Then
                Return
            End If
            SetPortListItem(Me.txtPortOfDischarge.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPort.Items.Count > 0 Then
                Dim findListItem = Me.lbPort.Items.FindByValue(Me.txtPortOfDischarge.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblPortOfDischargeText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbPort.Items.FindByValue(Me.txtPortOfDischarge.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblPortOfDischargeText.Text = parts(1)
                        Me.txtPortOfDischarge.Text = parts(0)
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