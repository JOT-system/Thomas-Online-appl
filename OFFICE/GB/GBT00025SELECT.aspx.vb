Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' リースブレーカー(協定書)承認検索画面クラス
''' </summary>
Public Class GBT00025SELECT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00025S"     '自身のMAPID
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 検索画面設定値保持プロパティ
    ''' </summary>
    ''' <returns></returns>
    Public Property ThisScreenValues As GBT00025SValues
    '**********************************
    ' リストIDテーブル関連
    '**********************************
    'キー情報のPrefix
    Private Const CONST_KEY_PREFIX As String = "ARG"

    ''' <summary>
    ''' リストID取得テーブル名
    ''' </summary>
    Private Const CONST_TBL_LISTS As String = "COS00XX_CURRENTLIST"
    ''' <summary>
    ''' リストID格納フィールド名
    ''' </summary>
    Private Const CONST_LIST_ID_FILED As String = "LISTID"
    ''' <summary>
    ''' リスト名格納フィールド名(英語)
    ''' </summary>
    Private Const CONST_LIST_ID_FILEDNAME_E As String = "LISTNAME"
    ''' <summary>
    ''' リスト名格納フィールド名(日本語)
    ''' </summary>
    Private Const CONST_LIST_ID_FILEDNAME_J As String = "LISTNAME"
    ''' <summary>
    ''' 条件フィールド名文言(連番抜き)
    ''' </summary>
    Private Const CONST_LIST_CONDITIONS_FILEDNAME_PREFIX_E As String = "ARG"
    ''' <summary>
    ''' 条件フィールド名(連番抜き)日本語(前方一致で条件フィールド名文言選定するのでARG(英語) ARG_JP(日本語）
    ''' とすると破綻しますのでご注意JP_ARG等としてください
    ''' </summary>
    Private Const CONST_LIST_CONDITIONS_FILEDNAME_PREFIX_J As String = "ARG"
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
            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If


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
                DefaultValueSet() '一覧表から戻った場合のみ発動
                '****************************************
                'フォーカス設定
                '****************************************
                'txtStYMD.Focus()


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
                '帳票一覧IDビュー表示切替
                Case Me.vLeftListId.ID
                    'リスト一覧表を取得
                    Dim dtList As DataTable = GetCurrentList()
                    '取得結果を貼付
                    lbList.Items.Clear() 'リストをクリア
                    lbList.DataValueField = CONST_LIST_ID_FILED
                    lbList.DataTextField = "LISTITEMNAME"
                    lbList.DataSource = dtList
                    lbList.DataBind()
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        '現在入力している値を選択状態とする
                        If txtobj.Text <> "" AndAlso lbList.Items.FindByValue(txtobj.Text) IsNot Nothing Then
                            lbList.SelectedValue = txtobj.Text
                        End If
                        Me.mvLeft.Focus()
                    End If

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

        '入力チェック
        Dim additionalMessage As String = ""
        Dim chkMessage As String = checkProc(additionalMessage)
        If chkMessage <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(chkMessage, Me.lblFooterMessage, pageObject:=Me, messageParams:=New List(Of String) From {additionalMessage})
            Return
        End If
        '画面設定値取得
        Me.ThisScreenValues = GetDispValue()
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
                Case Me.vLeftListId.ID 'アクティブなビューが荷主コード
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Me.lblListText.Text = ""
                    Dim condItem As Dictionary(Of String, GBT00025SValues.ConditionItem) = Nothing
                    If targetObject IsNot Nothing AndAlso
                       lbList.SelectedItem IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        txtobj.Text = lbList.SelectedItem.Value
                        Dim dt As DataTable = GetCurrentList(lbList.SelectedItem.Value)
                        If dt IsNot Nothing AndAlso dt.Rows.Count <> 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            Dim text As String = Convert.ToString(dr("TARGETLISTNAME"))
                            Me.lblListText.Text = text
                        End If
                        condItem = GetConditionList(txtobj.Text)
                        txtobj.Focus()
                    End If
                    repListConditions.DataSource = condItem.Values
                    repListConditions.DataBind()
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
        ListClear()
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
        ListClear()
    End Sub
    ''' <summary>
    ''' ListId変更時イベント
    ''' </summary>
    Public Sub txtList_Change()
        Me.lblListText.Text = ""
        repListConditions.DataSource = Nothing
        If txtList.Text = "" Then
            repListConditions.DataBind()
            Return
        End If
        Dim condItem As Dictionary(Of String, GBT00025SValues.ConditionItem) = Nothing

        Dim txtobj As TextBox = txtList
        Dim dt As DataTable = GetCurrentList(txtobj.Text)
        If dt IsNot Nothing AndAlso dt.Rows.Count <> 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Dim text As String = Convert.ToString(dr("TARGETLISTNAME"))
            Me.lblListText.Text = text
        End If
        condItem = GetConditionList(txtobj.Text)
        txtobj.Focus()

        repListConditions.DataSource = condItem.Values
        repListConditions.DataBind()

    End Sub
    ''' <summary>
    ''' リストを初期化
    ''' </summary>
    Private Sub ListClear()
        Me.lbList.Items.Clear()
        Me.mvLeft.SetActiveView(Me.vLeftCal)
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
        AddLangSetting(dicDisplayText, Me.lblList, "リストID", "List ID")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub
    ''' <summary>
    ''' リスト定義テーブルよりデータを取得
    ''' </summary>
    ''' <param name="listId"></param>
    ''' <returns></returns>
    Private Function GetCurrentList(Optional listId As String = "") As DataTable
        Dim retDt As New DataTable
        Dim sqlStat As New StringBuilder
        '動的条件入力フィールド名がある為 * 抜きしています
        sqlStat.AppendLine("SELECT LS.*")
        '↓条件入力テキストの右側に表示する文言
        sqlStat.AppendFormat("      ,LS.{0} AS TARGETLISTNAME", If(COA0019Session.LANGDISP <> C_LANG.JA, CONST_LIST_ID_FILEDNAME_E, CONST_LIST_ID_FILEDNAME_J)).AppendLine()
        '↓左リストボックス表示用文言
        sqlStat.AppendFormat("      ,LS.{0} + ':' + LS.{1} AS LISTITEMNAME", CONST_LIST_ID_FILED, If(COA0019Session.LANGDISP <> C_LANG.JA, CONST_LIST_ID_FILEDNAME_E, CONST_LIST_ID_FILEDNAME_J)).AppendLine()
        sqlStat.AppendFormat("  FROM {0} LS", CONST_TBL_LISTS).AppendLine()
        sqlStat.AppendLine(" WHERE PROFID IN (@PRODID_D,@PROFID_U)")
        sqlStat.AppendLine("   AND DELFLG <> @DELFLG")
        If listId <> "" Then
            sqlStat.AppendFormat("   AND {0} = @LISTID", CONST_LIST_ID_FILED).AppendLine()
        End If
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@PRODID_D", SqlDbType.NVarChar).Value = "Default"
                .Add("@PROFID_U", SqlDbType.NVarChar).Value = COA0019Session.PROFID 'ログインUserのプロフID
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@LISTID", SqlDbType.NVarChar).Value = listId
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return retDt
    End Function
    ''' <summary>
    ''' リストIDを元にアイテムを生成
    ''' </summary>
    ''' <param name="listId"></param>
    ''' <returns></returns>
    Public Function GetConditionList(listId As String) As Dictionary(Of String, GBT00025SValues.ConditionItem)
        Dim dt As DataTable = GetCurrentList(listId)
        Dim retCondList As New Dictionary(Of String, GBT00025SValues.ConditionItem)
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return retCondList
        End If
        Dim targetFieldList As New Dictionary(Of String, String)
        Dim fieldNamePrefix As String = If(COA0019Session.LANGDISP <> C_LANG.JA, CONST_LIST_CONDITIONS_FILEDNAME_PREFIX_E, CONST_LIST_CONDITIONS_FILEDNAME_PREFIX_J)
        'フィールド名をループし対象の番号を取得
        For Each colObj As DataColumn In dt.Columns
            '条件フィールド名の前方一致が無い場合はスキップ
            If Not colObj.ColumnName.StartsWith(fieldNamePrefix) Then
                Continue For
            End If
            '前方一致部分を除いた箇所が数値以外ならスキップ
            Dim colNumArea As String = colObj.ColumnName.Replace(fieldNamePrefix, "")
            If Not IsNumeric(colNumArea) Then
                Continue For
            End If
            targetFieldList.Add(colObj.ColumnName, colNumArea)
        Next colObj
        Dim dr As DataRow = dt.Rows(0)
        '対象の文言フィールドが確定戻り値生成
        For Each targetField In targetFieldList
            '文言が未設定の場合は条件フィールドとして認めない
            Dim fieldName As String = Convert.ToString(dr(targetField.Key))
            If fieldName = "" Then
                Continue For
            End If
            Dim keyString = CONST_KEY_PREFIX & targetField.Value
            Dim valSettingColumnName As String = "VAL" & targetField.Value
            Dim valSetting = Convert.ToString(dr(valSettingColumnName))
            Dim condItem As New GBT00025SValues.ConditionItem(keyString, fieldName, "", valSetting)
            retCondList.Add(keyString, condItem)
        Next targetField

        Return retCondList
    End Function
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    Public Function checkProc(Optional ByRef addtionalMessage As String = "") As String

        '少なくともリスト選択は必須としとく
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get
        '入力文字置き換え
        '画面PassWord内の使用禁止文字排除
        If Me.txtList.Text.Trim = "" Then
            Return C_MESSAGENO.REQUIREDVALUE
        End If
        Dim listId As String = Me.txtList.Text
        Dim dt As DataTable = GetCurrentList(listId)
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            addtionalMessage = Me.txtList.Text
            Return C_MESSAGENO.UNSELECTABLEERR
        End If
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="inColName"></param>
    ''' <param name="inText"></param>
    Protected Function CheckSingle(ByVal inColName As String, ByVal inText As String) As String

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck      '項目チェック

        '〇単項目チェック
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELD = inColName
        COA0026FieldCheck.VALUE = inText
        COA0026FieldCheck.COA0026FieldCheck()
        Return COA0026FieldCheck.ERR

    End Function
    ''' <summary>
    ''' 初期表示
    ''' </summary>
    Public Sub DefaultValueSet()
        If TypeOf Page.PreviousPage Is GBT00025LIST Then
            Dim prevPage As GBT00025LIST = DirectCast(Page.PreviousPage, GBT00025LIST)
            '実行画面からの画面遷移
            Me.SetDispValue(prevPage.GBT00025SValues)
        End If
    End Sub

    ''' <summary>
    ''' 当画面の情報を引き渡し用クラスに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDispValue() As GBT00025SValues
        Dim retVal As New GBT00025SValues
        Dim listId As String = Me.txtList.Text
        Dim dt As DataTable = GetCurrentList(listId)
        'この段階でレコードなしはありえない想定
        Dim dr As DataRow = dt.Rows(0)
        Dim conditions As New Dictionary(Of String, GBT00025SValues.ConditionItem)
        For Each repItem As RepeaterItem In Me.repListConditions.Items
            Dim hdnKey As HiddenField = DirectCast(repItem.FindControl("hdnCOnditionKey"), HiddenField)
            Dim hdnFieldSetting As HiddenField = DirectCast(repItem.FindControl("hdnFieldSetting"), HiddenField)
            Dim lblFieldName As Label = DirectCast(repItem.FindControl("lblCondition"), Label)
            Dim txtValue As TextBox = DirectCast(repItem.FindControl("txtCondition"), TextBox)
            Dim lblText As Label = DirectCast(repItem.FindControl("lblConditionText"), Label)
            Dim fieldSettingVal As GBT00025SValues.FieldSettings = DirectCast([Enum].Parse(GetType(GBT00025SValues.FieldSettings), hdnFieldSetting.Value), GBT00025SValues.FieldSettings)
            conditions.Add(hdnKey.Value, New GBT00025SValues.ConditionItem(hdnKey.Value, lblFieldName.Text, txtValue.Text, lblText.Text, "", fieldSettingVal, True))
        Next repItem
        retVal.ListId = listId
        retVal.ListIdName = Me.lblListText.Text
        retVal.SelectedListDt = dt
        retVal.Conditions = conditions
        Return retVal
    End Function
    ''' <summary>
    ''' 当画面に戻ってきた際に引き渡された情報を展開
    ''' </summary>
    ''' <param name="valClass"></param>
    Private Sub SetDispValue(valClass As GBT00025SValues)
        Me.txtList.Text = valClass.ListId
        Me.lblListText.Text = valClass.ListIdName
        Me.repListConditions.DataSource = valClass.Conditions.Values
        Me.repListConditions.DataBind()
    End Sub

    ''' <summary>
    ''' 検索条件保持用クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00025SValues
        ''' <summary>
        ''' フィールド文言に応じ入力項目の設定を保持
        ''' </summary>
        Public Enum FieldSettings
            ''' <summary>
            ''' 標準
            ''' </summary>
            Normal = 0
            ''' <summary>
            ''' 入力行は見せない(次画面には連動)
            ''' </summary>
            Hidden = 1
            ''' <summary>
            ''' 入力不可(次画面には連動)
            ''' </summary>
            Fix = 2
        End Enum
        ''' <summary>
        ''' リストID
        ''' </summary>
        ''' <returns></returns>
        Public Property ListId As String = ""
        ''' <summary>
        ''' リストID名称
        ''' </summary>
        ''' <returns></returns>
        Public Property ListIdName As String = ""
        ''' <summary>
        ''' 選択した行のリスト情報
        ''' </summary>
        ''' <returns></returns>
        Public Property SelectedListDt As DataTable
        ''' <summary>
        ''' 入力した条件
        ''' </summary>
        ''' <returns>Key="ARGn"（名称未設定の場合歯抜けあり) ,Value=画面入力したキーに対応する値</returns>
        Public Property Conditions As New Dictionary(Of String, ConditionItem)
        ''' <summary>
        ''' 入力条件保持用サブクラス
        ''' </summary>
        <Serializable>
        Public Class ConditionItem
            Public Sub New(key As String, fieldName As String, value As String, valSetting As String)
                Me.New(key, fieldName, value, "", valSetting, FieldSettings.Normal)
            End Sub
            ''' <summary>
            ''' コンストラクタ(フル)
            ''' </summary>
            Public Sub New(key As String, fieldName As String, value As String, text As String, valSetting As String, fieldSetting As FieldSettings, Optional setParamFieldSetting As Boolean = False)
                Dim fieldNameWk As String = fieldName
                Dim fSetting As FieldSettings = FieldSettings.Normal
                If setParamFieldSetting = True Then
                    fSetting = fieldSetting
                ElseIf fieldName.StartsWith("_hdn_") Then
                    fSetting = FieldSettings.Hidden
                    fieldNameWk = fieldNameWk.Replace("_hdn_", "")
                ElseIf fieldName.StartsWith("_fix_") Then
                    fSetting = FieldSettings.Fix
                    fieldNameWk = fieldNameWk.Replace("_fix_", "")
                End If
                Me.Key = key
                Me.FieldName = fieldNameWk
                Me.Text = text
                Me.FieldSetting = fSetting
                Me.ValSetting = valSetting

                If value = "" AndAlso valSetting <> "" Then
                    Dim valTemp As String = ""
                    Try
                        valTemp = GetValueFromSetting(valSetting)
                    Catch ex As Exception
                        valTemp = ""
                    End Try
                    Me.Value = valTemp
                Else
                    Me.Value = value
                End If

            End Sub
            ''' <summary>
            ''' キーARG1等
            ''' </summary>
            ''' <returns></returns>
            Public Property Key As String = ""
            ''' <summary>
            ''' フィールド文言
            ''' </summary>
            ''' <returns></returns>
            Public Property FieldName As String = ""
            ''' <summary>
            ''' 画面条件入力値
            ''' </summary>
            ''' <returns></returns>
            Public Property Value As String = ""
            ''' <summary>
            ''' 入力値に基づく文言(未使用)
            ''' </summary>
            ''' <returns></returns>
            Public Property Text As String = ""
            ''' <summary>
            ''' フィールド設定プロパティ
            ''' </summary>
            ''' <returns></returns>
            Public Property FieldSetting As FieldSettings = FieldSettings.Normal
            ''' <summary>
            ''' Val[nn]の値
            ''' </summary>
            ''' <returns></returns>
            Public Property ValSetting As String
            ''' <summary>
            ''' 文字列装飾より変数の内容を取得
            ''' </summary>
            ''' <param name="valueSetting"></param>
            ''' <returns></returns>
            ''' <remarks>フル装飾且つ参照できる変数のみ利用してください
            ''' 取得できない場合はブランク</remarks>
            Private Function GetValueFromSetting(valueSetting As String) As String
                If valueSetting.Trim = "" Then
                    Return ""
                End If
                '名前空間からクラスまでの情報を取得
                Dim valSettingSplit = valueSetting.Split("."c)
                Dim typeString As String = ""
                Dim resType As Type = Nothing
                Dim baseDllString As String = "{0}, BASEDLL, Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null"
                Dim isBaseDll As Boolean = False
                Dim getTypeString As String = ""
                If valSettingSplit IsNot Nothing AndAlso valSettingSplit.Count > 0 AndAlso valSettingSplit(0).Equals("BASEDLL") Then
                    isBaseDll = True
                End If
                For Each valSettingItem In valSettingSplit

                    If typeString = "" Then
                        typeString = valSettingItem
                    Else
                        typeString = typeString & "." & valSettingItem
                    End If

                    If isBaseDll Then
                        getTypeString = String.Format(baseDllString, typeString)
                    Else
                        getTypeString = typeString
                    End If

                    If Type.GetType(getTypeString) IsNot Nothing Then
                        resType = Type.GetType(getTypeString)
                    End If
                Next
                If resType Is Nothing Then
                    Return valueSetting
                End If
                '対象クラスのメンバ存在チェック
                Dim memberName As String = valueSetting.Replace(resType.FullName & ".", "")

                If resType.GetMember(memberName) Is Nothing Then
                    Return valueSetting
                End If
                Dim member = resType.GetMember(memberName).First
                Select Case member.MemberType
                    Case Reflection.MemberTypes.Property
                        Dim retVal As String = Convert.ToString(resType.InvokeMember(memberName, Reflection.BindingFlags.GetProperty, Nothing, resType, Nothing))
                        Return Convert.ToString(retVal)
                    Case Else
                        Return valueSetting
                End Select
            End Function
        End Class
    End Class

End Class