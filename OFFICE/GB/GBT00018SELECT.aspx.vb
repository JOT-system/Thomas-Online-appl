Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' SOA締め検索画面クラス
''' </summary>
Public Class GBT00018SELECT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00018S"     '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00018A"
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
                Me.txtBillingYmd.Focus()
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
                '請求月ビュー表示切替
                Case Me.vLeftBillingDate.ID
                    SetBillingDateListItem(Me.txtBillingYmd.Text)
                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Dim dtObj As Date = Nothing
                        Dim strObj As String = txtobj.Text & "/01"
                        If Date.TryParse(strObj, dtObj) Then
                            Me.hdnCalendarValue.Value = strObj
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
                Case Me.vLeftBillingDate.ID 'アクティブなビューが請求月
                    '請求月選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        txtobj.Text = Me.lbBillingDate.SelectedItem.Text
                        txtobj.Focus()
                    End If
                Case Me.vLeftCal.ID 'アクティブなビューがカレンダー
                    'カレンダー選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        txtobj.Text = CDate(Me.hdnCalendarValue.Value).ToString("yyyy/MM")
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

        AddLangSetting(dicDisplayText, Me.lblBillingYmd, "請求月", "Billing Month")

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

        If TypeOf Page.PreviousPage Is GBT00018APPROVAL Then
            Dim prevPage As GBT00018APPROVAL = DirectCast(Page.PreviousPage, GBT00018APPROVAL)
            '実行画面からの画面遷移
            '○画面項目設定（セッション変数より）処理
            Dim dicObjs As New Dictionary(Of String, TextBox) From {
                                                                    {"hdnBillingYmd", Me.txtBillingYmd}}

            '前画面の値を当画面のテキストボックスに展開
            For Each dicObj As KeyValuePair(Of String, TextBox) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(dicObj.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdnObj As HiddenField = DirectCast(tmpCont, HiddenField)
                    dicObj.Value.Text = FormatDateContrySettings(tmphdnObj.Value, GBA00003UserSetting.DATEYMFORMAT)
                End If
            Next
            '選択画面の入力初期値設定
            'メニューから遷移/業務画面戻り判定
        ElseIf Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00018APPROVAL Then
            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        End If

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
                        From {{"BILLINGYMD", Me.txtBillingYmd}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = Me.hdnMapVariant.Value
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                item.Value.Text = FormatDateContrySettings(COA0016VARIget.VALUE, GBA00003UserSetting.DATEYMFORMAT)
            Else
                CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
                Return
            End If
        Next

        '請求日初期値設定
        SetBillingDate()

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
        Dim checkObjList = {New With {.txtObj = Me.txtBillingYmd, .lstObj = DirectCast(Nothing, ListBox), .fieldName = "BILLINGYMD", .swapListValue = False}}
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
                CheckSingle(checkObj.fieldName, checkObj.txtObj.Text)
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
    ''' 請求日変更処理
    ''' </summary>
    Public Sub txtBillingYmd_Change()
        Try
            If Me.txtBillingYmd.Text.Trim = "" Then
                Return
            End If

            Dim bilDate As Date = Nothing

            If Date.TryParse(txtBillingYmd.Text, bilDate) Then
                txtBillingYmd.Text = bilDate.ToString(GBA00003UserSetting.DATEYMFORMAT)
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
    ''' 請求日初期値設定
    ''' </summary>
    Public Sub SetBillingDate()

        Try
            'SQL文の作成
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       MIN(REPORTMONTH) AS DATE")
            sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CL")
            sqlStat.AppendLine(" WHERE CL.DELFLG        = @DELFLG")
            sqlStat.AppendLine("   AND CL.APPLYID      <> '' ")

            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = CONST_FLAG_NO
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        'DBからアイテムを設定
                        Me.txtBillingYmd.Text = FormatDateContrySettings(SQLdr("DATE").ToString(), GBA00003UserSetting.DATEYMFORMAT)
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
    ''' 請求月リストアイテムを設定
    ''' </summary>
    Private Sub SetBillingDateListItem(selectedValue As String)
        Try
            'リストクリア
            Me.lbBillingDate.Items.Clear()

            'Dim month As Date = Date.Now.AddMonths(-1)

            'For i As Integer = 0 To 11

            '    Me.lbBillingDate.Items.Add(month.ToString("yyyy/MM"))

            '    month = month.AddMonths(1)
            'Next

            'SQL文の作成
            Dim GBA00003UserSetting As New GBA00003UserSetting With {.USERID = COA0019Session.USERID}
            GBA00003UserSetting.GBA00003GetUserSetting()

            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT TOP 1 CD.BILLINGYMD ")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,1,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH1")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,2,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH2")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,3,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH3")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,4,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH4")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,5,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH5")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,6,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH6")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,7,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH7")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,8,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH8")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,9,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH9")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,10,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH10")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,11,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH11")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,12,CD.BILLINGYMD),'" & GBA00003UserSetting.DATEYMFORMAT & "') AS MONTH12")
            sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CD")
            sqlStat.AppendLine(" WHERE CD.REPORTMONTH   = (SELECT TOP 1 MIN(REPORTMONTH) as REPORTMONTH FROM GBT0006_CLOSINGDAY group by COUNTRYCODE order by REPORTMONTH DESC)")
            sqlStat.AppendLine("   AND CD.DELFLG        = @DELFLG")

            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = CONST_FLAG_NO
                End With

                Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                    While SQLdr.Read
                        'DBからアイテムを設定
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH1")), "1"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH2")), "2"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH3")), "3"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH4")), "4"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH5")), "5"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH6")), "6"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH7")), "7"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH8")), "8"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH9")), "9"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH10")), "10"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH11")), "11"))
                        Me.lbBillingDate.Items.Add(New ListItem(String.Format("{0}", SQLdr("MONTH12")), "12"))
                        Exit While
                    End While
                End Using 'SQLdr
            End Using 'SQLcon SQLcmd

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbBillingDate.Items.Count > 0 Then
                Dim findListItem = Me.lbBillingDate.Items.FindByText(selectedValue)
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

End Class