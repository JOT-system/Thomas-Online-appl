Imports System.Data.SqlClient
Imports System.Drawing
Imports BASEDLL
''' <summary>
''' 国連番号マスタ検索画面クラス
''' </summary>
Public Class GBM00007SELECT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00007S"       '自身のMAPID
    Private Const CONST_BASEID As String = "GBM00007"
    Private returnCode As String = String.Empty             'サブ用リターンコード
    Private charConvList As ListBox = Nothing

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
            'lblFooterMessage.ForeColor = Color.Black
            'lblFooterMessage.Font.Bold = False

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

                '****************************************
                'セッション設定
                '****************************************

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
                '国連番号ビュー表示切替
                Case Me.vLeftUnNo.ID
                    SetUnNoListItem(Me.txtUnNo.Text & "," & Me.txtHazardClass.Text & "," & Me.txtPackingGroup.Text)
                '等級ビュー表示切替
                Case Me.vLeftHazardClass.ID
                    SetHazardClassListItem(Me.txtHazardClass.Text)
                '容器等級ビュー表示切替
                Case Me.vLeftPackingGroup.ID
                    SetPackingGroupListItem(Me.txtPackingGroup.Text)
                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    '    Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                    '    Me.hdnCalendarValue.Value = txtEndYMD.Text
                    Me.hdnCalendarValue.Value = FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT) ' カレンダーは常にFROM基準で表示

                    Me.mvLeft.Focus()
                    'End If

            End Select
        End If

    End Sub
    ''' <summary>
    ''' 実行ボタン押下時
    ''' </summary>
    Public Sub btnEnter_Click()
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl

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
                Case Me.vLeftUnNo.ID 'アクティブなビューが国コード
                    '国連番号選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbUnNo.SelectedItem IsNot Nothing Then
                            Dim parts As String()
                            parts = Split(Me.lbUnNo.SelectedItem.Text, ",", -1, CompareMethod.Text)
                            txtobj.Text = parts(0)
                            Dim UnNoKeyValue As Dictionary(Of String, String) = DirectCast(ViewState("UNNOKEYVALUE"), Dictionary(Of String, String))
                            Me.lblUnNoText.Text = UnNoKeyValue(Me.lbUnNo.SelectedItem.Text)

                            '等級、容器等級も設定
                            Me.txtHazardClass.Text = parts(1)
                            txtHazardClass_Change()
                            Me.txtPackingGroup.Text = parts(2)
                            txtPackingGroup_Change()

                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblUnNoText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftHazardClass.ID 'アクティブなビューが等級コード
                    '等級選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbHazardClass.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbHazardClass.SelectedItem.Value
                            Me.lblHazardClassText.Text = Me.lbHazardClass.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblHazardClassText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftPackingGroup.ID 'アクティブなビューが容器等級コード
                    '容器等級選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbPackingGroup.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbPackingGroup.SelectedItem.Value
                            Me.lblPackingGroupText.Text = Me.lbPackingGroup.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblPackingGroupText.Text = ""
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
        AddLangSetting(dicDisplayText, Me.lblUnNo, "国連番号", "UN No.")
        AddLangSetting(dicDisplayText, Me.lblHazardClass, "等級", "Class")
        AddLangSetting(dicDisplayText, Me.lblPackingGroup, "容器等級", "PG")
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
        If TypeOf Page.PreviousPage Is COM00002MENU Then _

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        Else
            '実行画面からの画面遷移
            '○画面項目設定処理
            Dim prevSelectUnNoPage As GBM00007UNNO = DirectCast(Page.PreviousPage, GBM00007UNNO)

            '有効年月日
            txtStYMD.Text = FormatDateContrySettings(DirectCast(prevSelectUnNoPage.FindControl("hdnPrevCondStYMD"), HiddenField).Value, GBA00003UserSetting.DATEFORMAT)
            txtEndYMD.Text = FormatDateContrySettings(DirectCast(prevSelectUnNoPage.FindControl("hdnPrevCondEndYMD"), HiddenField).Value, GBA00003UserSetting.DATEFORMAT)
            '国連番号　
            txtUnNo.Text = DirectCast(prevSelectUnNoPage.FindControl("hdnPrevCondUNNO"), HiddenField).Value
            txtUnNo_Change()
            '等級　
            txtHazardClass.Text = DirectCast(prevSelectUnNoPage.FindControl("hdnPrevCondHazardClass"), HiddenField).Value
            txtHazardClass_Change()
            '容器等級　
            txtPackingGroup.Text = DirectCast(prevSelectUnNoPage.FindControl("hdnPrevCondPackingGroup"), HiddenField).Value
            txtPackingGroup_Change()

            prevViewID = DirectCast(prevSelectUnNoPage.FindControl("hdnPrevViewID"), HiddenField).Value

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
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
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
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "ENDYMD"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtEndYMD.Text = FormatDateContrySettings(COA0016VARIget.VALUE, GBA00003UserSetting.DATEFORMAT)
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            Return
        End If

        '国連番号
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "UNNO"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtUnNo.Text = COA0016VARIget.VALUE
            txtUnNo_Change()
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            Return
        End If

        '等級
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "HAZARDCLASS"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtHazardClass.Text = COA0016VARIget.VALUE
            txtHazardClass_Change()
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            Return
        End If

        '容器等級
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
        COA0016VARIget.FIELD = "PACKINGGROUP"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
            txtPackingGroup.Text = COA0016VARIget.VALUE
            txtPackingGroup_Change()
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

        '国連番号
        COA0008InvalidChar.CHARin = txtUnNo.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtUnNo.Text = COA0008InvalidChar.CHARout
        End If

        '等級
        COA0008InvalidChar.CHARin = txtHazardClass.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtHazardClass.Text = COA0008InvalidChar.CHARout
        End If

        '容器等級
        COA0008InvalidChar.CHARin = txtPackingGroup.Text
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            txtPackingGroup.Text = COA0008InvalidChar.CHARout
        End If

        '入力項目チェック
        '①単項目チェック
        '有効開始日
        CheckSingle("STYMD", FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT))
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtStYMD.Focus()
            Return
        End If

        '有効終了日
        CheckSingle("ENDYMD", FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT))
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtEndYMD.Focus()
            Return
        End If

        '国連番号
        CheckSingle("UNNO", txtUnNo.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtUnNo.Focus()
            Return
        End If

        '等級
        CheckSingle("HAZARDCLASS", txtHazardClass.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtHazardClass.Focus()
            Return
        End If

        '容器等級
        CheckSingle("PACKINGGROUP", txtPackingGroup.Text)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPackingGroup.Focus()
            Return
        End If

        '②存在チェック(LeftBoxチェック)
        '等級
        CheckList(txtHazardClass.Text, lbHazardClass)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtHazardClass.Focus()
            Return
        End If

        '容器等級
        CheckList(txtPackingGroup.Text, lbPackingGroup)
        If returnCode <> C_MESSAGENO.NORMAL Then
            txtPackingGroup.Focus()
            Return
        End If

        '③相関チェック
        '有効開始日<=有効終了日
        If txtStYMD.Text <> "" AndAlso txtEndYMD.Text <> "" Then
            If FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT) > FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT) Then
                returnCode = C_MESSAGENO.VALIDITYINPUT
                CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
                txtStYMD.Focus()
                Return
            End If
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
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, pageObject:=Me)
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
    ''' 国連番号リストアイテムを設定
    ''' </summary>
    Private Sub SetUnNoListItem(selectedValue As String)
        Dim GBA00001UnNo As New GBA00001UnNo              '項目チェック
        'リストクリア
        Me.lbUnNo.Items.Clear()

        'リスト設定
        GBA00001UnNo.LISTBOX = Me.lbUnNo
        GBA00001UnNo.GBA00001getLeftListUnNo()
        If GBA00001UnNo.ERR = C_MESSAGENO.NORMAL Then
            Me.lbUnNo = GBA00001UnNo.LISTBOX
            ViewState("UNNOKEYVALUE") = GBA00001UnNo.UnNoKeyValue

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbUnNo.Items.Count > 0 Then
                Dim findListItem = Me.lbUnNo.Items.FindByText(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        ElseIf GBA00001UnNo.ERR = C_MESSAGENO.NODATA Then
            returnCode = C_MESSAGENO.NORMAL
        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00001UnNo.ERR)})
        End If
    End Sub

    ''' <summary>
    ''' 等級リストアイテムを設定
    ''' </summary>
    Private Sub SetHazardClassListItem(selectedValue As String)
        Dim COA0017FixValue As New COA0017FixValue
        'リストクリア
        Me.lbHazardClass.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "HAZARDCLASS"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbHazardClass
        Else
            COA0017FixValue.LISTBOX2 = Me.lbHazardClass
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbHazardClass = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbHazardClass = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbHazardClass.Items.Count > 0 Then
                Dim findListItem = Me.lbHazardClass.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If

    End Sub

    ''' <summary>
    ''' 容器等級リストアイテムを設定
    ''' </summary>
    Private Sub SetPackingGroupListItem(selectedValue As String)
        Dim COA0017FixValue As New COA0017FixValue
        'リストクリア
        Me.lbPackingGroup.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "PACKINGGROUP"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbPackingGroup
        Else
            COA0017FixValue.LISTBOX2 = Me.lbPackingGroup
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbPackingGroup = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbPackingGroup = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPackingGroup.Items.Count > 0 Then
                Dim findListItem = Me.lbPackingGroup.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If

    End Sub

    ''' <summary>
    ''' 国連番号名設定
    ''' </summary>
    Public Sub txtUnNo_Change()

        Try
            Me.lblUnNoText.Text = ""

            Dim findKey As String = Me.txtUnNo.Text & "," & Me.txtHazardClass.Text & "," & Me.txtPackingGroup.Text
            SetUnNoListItem(findKey)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbUnNo.Items.Count > 0 Then
                Dim findListItem = Me.lbUnNo.Items.FindByText(findKey)
                If findListItem IsNot Nothing Then
                    'Me.lblUnNoText.Text = findListItem.Attributes("data_names")
                    Dim UnNoKeyValue As Dictionary(Of String, String) = DirectCast(ViewState("UNNOKEYVALUE"), Dictionary(Of String, String))
                    Me.lblUnNoText.Text = UnNoKeyValue(Me.lbUnNo.SelectedItem.Text)
                Else
                    Dim findListItemUpper = Me.lbUnNo.Items.FindByValue(findKey.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        'Me.lblUnNoText.Text = findListItemUpper.Attributes("data_names")
                        Dim UnNoKeyValue As Dictionary(Of String, String) = DirectCast(ViewState("UNNOKEYVALUE"), Dictionary(Of String, String))
                        Me.lblUnNoText.Text = UnNoKeyValue(Me.lbUnNo.SelectedItem.Text)
                        Me.txtUnNo.Text = findListItemUpper.Text
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
    ''' 等級名設定
    ''' </summary>
    Public Sub txtHazardClass_Change()

        Try
            Me.lblHazardClassText.Text = ""

            SetHazardClassListItem(Me.txtHazardClass.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbHazardClass.Items.Count > 0 Then
                Dim findListItem = Me.lbHazardClass.Items.FindByValue(Me.txtHazardClass.Text)
                If findListItem IsNot Nothing Then
                    Me.lblHazardClassText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbHazardClass.Items.FindByValue(Me.txtHazardClass.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblHazardClassText.Text = findListItemUpper.Text
                        Me.txtHazardClass.Text = findListItemUpper.Value
                    End If
                End If
            End If
            '等級が変更された場合、国連番号も変更
            txtUnNo_Change()

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
    ''' 容器等級名設定
    ''' </summary>
    Public Sub txtPackingGroup_Change()

        Try
            Me.lblPackingGroupText.Text = ""

            GetPackingGroupCharConv()
            If returnCode = C_MESSAGENO.NORMAL AndAlso charConvList.Items.Count > 0 Then

                Dim charConvItem = charConvList.Items.FindByValue(Me.txtPackingGroup.Text)
                If charConvItem IsNot Nothing Then
                    Me.txtPackingGroup.Text = charConvItem.Text
                End If
            End If

            SetPackingGroupListItem(Me.txtPackingGroup.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPackingGroup.Items.Count > 0 Then
                Dim findListItem = Me.lbPackingGroup.Items.FindByValue(Me.txtPackingGroup.Text)
                If findListItem IsNot Nothing Then
                    Me.lblPackingGroupText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbPackingGroup.Items.FindByValue(Me.txtPackingGroup.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblPackingGroupText.Text = findListItemUpper.Text
                        Me.txtPackingGroup.Text = findListItemUpper.Value
                    End If
                End If
            End If
            '等級が変更された場合、国連番号も変更
            txtUnNo_Change()

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
    ''' 変換文字取得
    ''' </summary>
    Private Sub GetPackingGroupCharConv()
        Dim COA0017FixValue As New COA0017FixValue
        'リストクリア
        If charConvList IsNot Nothing Then
            charConvList.Items.Clear()
        End If

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARCONV"
        COA0017FixValue.LISTBOX1 = charConvList
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            charConvList = DirectCast(COA0017FixValue.LISTBOX1, ListBox)

            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else

            '異常
            returnCode = COA0017FixValue.ERR
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
        End If

    End Sub
End Class