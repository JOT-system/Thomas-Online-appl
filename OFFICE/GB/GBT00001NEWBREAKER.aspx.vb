Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' 新規ブレーカー作成画面クラス
''' </summary>
Public Class GBT00001NEWBREAKER
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00001N" '自身のMAPID
    ''' <summary>
    ''' VIEWSTATE名 選択輸送パターンの発着情報を保持
    ''' </summary>
    Private Const CONST_VS_POLPODCNT As String = "POLPODCNT"
    Private Const CONST_VS_NAME_GBT00002RV As String = "GBT00002RValues"
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' ブレーカー検索結果画面情報
    ''' </summary>
    ''' <returns></returns>
    Public Property GBT00002RValues As GBT00002RESULT.GBT00002RValues
    ''' <summary>
    ''' ページロード時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            COA0003LogFile = New COA0003LogFile              'ログ出力

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                Me.hdnThisMapVariant.Value = Convert.ToString(Session("MAPvariant"))
                '****************************************
                'ヘッダータイトル取得
                '****************************************
                Dim COA0031ProfMap As New BASEDLL.COA0031ProfMap 'タイトル文言取得
                Dim titleVari As String = "NewBreaker"
                If Me.hdnThisMapVariant.Value.EndsWith("Copy") Then
                    titleVari = "CopyBreaker"
                End If
                With COA0031ProfMap
                    .MAPIDP = CONST_MAPID
                    .VARIANTP = titleVari
                    .COA0031GetDisplayTitle()
                    Me.lblTitleText.Text = .NAMES
                End With

                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                '前々画面（前画面で保持している検索条件）をHiddenに記録
                '****************************************
                SetPrevDisplayInfo()
                '初期設定
                InitSetteing()
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                Me.GBT00002RValues = DirectCast(ViewState(CONST_VS_NAME_GBT00002RV), GBT00002RESULT.GBT00002RValues)
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

        Catch ex As System.Threading.ThreadAbortException
            Return
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.EXCEPTION 'ここは適宜変えてください
            Dim NORMAL As String = ""
            CommonFunctions.ShowMessage(C_MESSAGENO.EXCEPTION, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = messageNo
            COA0003LogFile.COA0003WriteLog()

            Return
        Finally
            '項目の入力可否制御
            disabledControls()
            Me.hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
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
                Case Me.vLeftBreakerType.ID
                    SetBreakerTypeListItem(Me.txtBreakerType.Text)
                '輸送パターンビュー表示
                Case Me.vLeftTransferPattern.ID
                    Dim dt As DataTable = GetTransferPattern(Me.txtBreakerType.Text)
                    With Me.lbTransferPattern
                        .DataSource = dt
                        .DataTextField = "NAMES"
                        .DataValueField = "USETYPE"
                        .DataBind()
                        '入力済のデータを選択状態にする
                        If .Items IsNot Nothing Then
                            Dim findLbValue As ListItem = .Items.FindByValue(Me.txtTransferPattern.Text)
                            If findLbValue IsNot Nothing Then
                                findLbValue.Selected = True
                            End If
                        End If
                        .Focus()
                    End With
                '港ビュー表示
                Case Me.vLeftPort.ID
                    Dim dt As DataTable = GetPort()
                    With lbPort
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "PORTCODE"
                        .DataBind()
                        .Focus()
                    End With
                    '入力済のデータを選択状態にする
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    If dblClickField IsNot Nothing AndAlso lbPort.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbPort.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
                'SHIPPERビュー表示
                Case Me.vLeftShipper.ID
                    Dim countryCode As String = Me.hdnPolCountry1.Value
                    If Me.txtBreakerType.Text = "1" Then
                        'SALESの場合
                        Dim dt As DataTable = GetShipper(countryCode)
                        With Me.lbShipper
                            .DataSource = dt
                            .DataTextField = "LISTBOXNAME"
                            .DataValueField = "CUSTOMERCODE"
                            .DataBind()
                            .Focus()
                        End With
                    Else
                        'OPEの場合
                        Dim dt As DataTable = GetAgent(countryCode)
                        With Me.lbShipper
                            .DataSource = dt
                            .DataTextField = "LISTBOXNAME"
                            .DataValueField = "CODE"
                            .DataBind()
                            .Focus()
                        End With
                    End If

                    '入力済のデータを選択状態にする
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    If dblClickField IsNot Nothing AndAlso lbShipper.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbShipper.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Me.hdnCalendarValue.Value = txtobj.Text
                        Me.mvLeft.Focus()
                    End If



            End Select
        End If

    End Sub
    ''' <summary>
    ''' 作成ボタン押下時処理
    ''' </summary>
    Public Sub btnCreate_Click()
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '必須入力チェック
        Dim requiredChkTexts As New List(Of TextBox) From {Me.txtBreakerType,
                                                          Me.txtTransferPattern,
                                                          Me.txtPolPort1,
                                                          Me.txtShipper
                                                          }
        Dim listCheckTexts As New List(Of TextBox) From {Me.txtTransferPattern,
                                                         Me.txtPolPort1,
                                                         Me.txtShipper
                                                         }
        If trPod1.Visible = True Then
            requiredChkTexts.Add(Me.txtPodPort1)
            listCheckTexts.Add(Me.txtPodPort1)
        End If

        If trPol2.Visible = True Then
            requiredChkTexts.AddRange({Me.txtPolPort2,
                                        Me.txtPodPort2})
            listCheckTexts.AddRange({Me.txtPolPort2, Me.txtPodPort2})
        End If

        For Each requiredChkText In requiredChkTexts
            If requiredChkText.Text = "" Then
                CommonFunctions.ShowMessage(C_MESSAGENO.REQUIREDVALUE, Me.lblFooterMessage, pageObject:=Me)
                requiredChkText.Focus()
                Return
            End If
        Next requiredChkText
        Dim chkDt As DataTable = Nothing
        Dim fieldName As String = ""
        For Each listCheckText In listCheckTexts
            Dim chkVal As String = listCheckText.Text
            chkDt = Nothing
            Select Case listCheckText.ID
                Case "txtTransferPattern"
                    chkDt = GetTransferPattern(Me.txtBreakerType.Text)
                    fieldName = "USETYPE"
                Case "txtPolPort1", "txtPodPort1", "txtPolPort2", "txtPodPort2"
                    chkDt = GetPort()
                    fieldName = "PORTCODE"
                Case "txtShipper"
                    Dim countryCode As String = Me.hdnPolCountry1.Value
                    If Me.txtBreakerType.Text = "1" Then
                        chkDt = GetShipper(countryCode)
                        fieldName = "CUSTOMERCODE"
                    Else
                        chkDt = GetAgent(countryCode)
                        fieldName = "CODE"
                    End If
            End Select
            Dim result = From item In chkDt Where Convert.ToString(item(fieldName)).ToUpper.Equals(chkVal.ToUpper())
            If result.Any = False Then
                CommonFunctions.ShowMessage(C_MESSAGENO.UNSELECTABLEERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                        messageParams:=New List(Of String) From {String.Format("VALUE:{0}", chkVal)})
                listCheckText.Focus()
                Return
            Else
                listCheckText.Text = Convert.ToString(result(0).Item(fieldName))
            End If
        Next
        '■■■ 画面遷移先URL取得 ■■■
        Session("MAPmapid") = CONST_MAPID
        Session("MAPvariant") = Me.hdnThisMapVariant.Value

        COA0012DoUrl.MAPIDP = Convert.ToString(Session("MAPmapid"))
        COA0012DoUrl.VARIP = Convert.ToString(Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value

        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
            Me.lblFooterMessage.Text = COA0011ReturnUrl.NAMES
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
                        txtobj.Text = Me.hdnCalendarValue.Value
                        txtobj.Focus()
                    End If
                Case Me.vLeftBreakerType.ID
                    'ブレーカー種類を明示的に変更したため輸送パターンはクリア
                    Me.txtTransferPattern.Text = ""
                    Me.lblTransferPatternText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbBreakerType.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbBreakerType.SelectedItem.Value
                            Me.lblBreakerTypeText.Text = HttpUtility.HtmlEncode(Me.lbBreakerType.SelectedItem.Text)
                        Else
                            txtobj.Text = ""
                            Me.lblBreakerTypeText.Text = ""
                        End If

                        txtobj.Focus()
                    End If
                Case Me.vLeftTransferPattern.ID
                    '輸送パターン選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim useType As String = ""

                    If lbTransferPattern.SelectedItem IsNot Nothing Then
                        useType = lbTransferPattern.SelectedItem.Value
                    End If
                    SetDisplayTransferPattern(Me.txtBreakerType.Text, useType)
                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If
                Case Me.vLeftPort.ID
                    '港選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim portCode As String = ""
                        If Me.lbPort.SelectedItem IsNot Nothing Then
                            portCode = Me.lbPort.SelectedItem.Value
                        End If
                        SetDisplayPort(targetTextBox, portCode)
                    End If
                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If
                Case Me.vLeftShipper.ID
                    '荷主選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim customerCode As String = ""
                        If Me.lbShipper.SelectedItem IsNot Nothing Then
                            customerCode = Me.lbShipper.SelectedItem.Value
                        End If
                        SetDisplayShipper(targetTextBox, customerCode)
                    End If
                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
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
    ''' ブレーカーラベル変更時処理
    ''' </summary>
    Public Sub txtBreakerType_Change()
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue
        'ブレーカー種類を明示的に変更したため輸送パターンはクリア
        Me.txtTransferPattern.Text = ""
        Me.lblTransferPatternText.Text = ""

        If Me.txtBreakerType.Text = "" Then
            Me.lblBreakerTypeText.Text = ""
            Return
        End If
        Dim keyValue As String = Me.txtBreakerType.Text.Trim
        With COA0017FixValue

            .COMPCODE = GBC_COMPCODE_D
            .CLAS = "GBT00001BRTYPE"
            .LISTBOX1 = Me.lbBreakerType
            .COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                Me.lbBreakerType = DirectCast(.LISTBOX1, ListBox)
                'テキストボックスに入力した内容と合致する場合は一覧を選択状態
                If Me.lbBreakerType.Items IsNot Nothing AndAlso
                   Me.lbBreakerType.Items.Count > 0 Then
                    Dim findListItem = Me.lbBreakerType.Items.FindByValue(keyValue)
                    If findListItem IsNot Nothing Then
                        Me.txtBreakerType.Text = keyValue
                        Me.lblBreakerTypeText.Text = HttpUtility.HtmlEncode(findListItem.Text)
                    Else
                        Me.lblBreakerTypeText.Text = ""
                    End If
                End If
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
            End If
        End With
    End Sub
    ''' <summary>
    ''' 輸送パターンテキストボックス変更時イベント
    ''' </summary>
    Public Sub txtTransferPattern_Change()
        SetDisplayTransferPattern(Me.txtBreakerType.Text, Me.txtTransferPattern.Text)
    End Sub
    ''' <summary>
    ''' 発地1港変更時イベント
    ''' </summary>
    Public Sub txtPolPort1_Change()
        SetDisplayPort(Me.txtPolPort1, Me.txtPolPort1.Text)
    End Sub
    ''' <summary>
    ''' 着地1港変更時イベント
    ''' </summary>
    Public Sub txtPodPort1_Change()
        SetDisplayPort(Me.txtPodPort1, Me.txtPodPort1.Text)
    End Sub
    ''' <summary>
    ''' 発地2港変更時イベント
    ''' </summary>
    Public Sub txtPolPort2_Change()
        SetDisplayPort(Me.txtPolPort2, Me.txtPolPort2.Text)
    End Sub
    ''' <summary>
    ''' 着地2港変更時イベント
    ''' </summary>
    Public Sub txtPodPort2_Change()
        SetDisplayPort(Me.txtPodPort2, Me.txtPodPort2.Text)
    End Sub
    ''' <summary>
    ''' 荷主変更時イベント
    ''' </summary>
    Public Sub txtShipper_Change()
        SetDisplayShipper(Me.txtShipper, Me.txtShipper.Text)
    End Sub
    ''' <summary>
    ''' 入力状態に応じテキストの入力可否制御を行う
    ''' </summary>
    Private Sub disabledControls()
        'ブレーカー名称が空白の場合
        If Me.lblBreakerTypeText.Text = "" Then
            '輸送パターンをクリア
            Me.txtTransferPattern.Text = ""
            Me.txtTransferPattern.Enabled = False '輸送パターンを入力不可
            Me.lblTransferPatternText.Text = ""
            '三国間フラグをなし
            Me.hdnIsTrilateral.Value = ""
        ElseIf Me.hdnCopyBaseBrId.Value <> "" Then
            Me.txtTransferPattern.Enabled = False
        Else
            'ブレーカー入力ありの場合は輸送パターンの入力許可
            Me.txtTransferPattern.Enabled = True
        End If
        'リースタンク利用チェックボックス
        If Me.hdnCopyBaseBrId.Value <> "" Then
            Me.chkLeaseTankUse.Enabled = False
        End If
        '輸送パターンが空白の場合
        If Me.lblTransferPatternText.Text = "" Then
            '三国間フラグをなしに
            Me.hdnIsTrilateral.Value = ""
        End If
        'POL1の国未指定の場合
        If Me.lblPolPort1Text.Text = "" Then
            'SHIPPERをクリア
            Me.txtShipper.Text = ""
            Me.lblShipperText.Text = ""
            Me.txtShipper.Enabled = False
        Else
            Me.txtShipper.Enabled = True
        End If
        '発のみパターンの場合POD1を非表示
        Dim dicPolPodCnt As Dictionary(Of String, Integer) = DirectCast(ViewState(CONST_VS_POLPODCNT), Dictionary(Of String, Integer))
        If dicPolPodCnt IsNot Nothing AndAlso dicPolPodCnt("POD1COUNT") = 0 Then
            Me.txtPodPort1.Text = ""
            Me.lblPodPort1Text.Text = ""
            Me.trPod1.Visible = False
        Else
            Me.trPod1.Visible = True
        End If
        '三国間フラグなない場合、出2、入2の行を非表示
        If hdnIsTrilateral.Value = "" Then
            Me.txtPodPort2.Text = ""
            Me.lblPodPort2Text.Text = ""

            Me.trPol2.Visible = False
            Me.trPod2.Visible = False

        Else
            Me.trPol2.Visible = True
            Me.trPod2.Visible = True
        End If
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

        'ラベル等やグリッドを除くの文言設定(適宜追加) リピーターの表ヘッダーもこの方式で可能ですので
        '作成者に聞いてください。
        AddLangSetting(dicDisplayText, Me.lblBreakerType, "ブレーカータイプ", "BREAKER TYPE")
        AddLangSetting(dicDisplayText, Me.lblLeaseTankUse, "リースタンク使用", "USING LEASE TANK")
        AddLangSetting(dicDisplayText, Me.lblTransferPattern, "輸送パターン", "TRANSFER PATTERN")
        AddLangSetting(dicDisplayText, Me.lblPol1, "発地1(港)", "POL1(PORT)")
        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主", "SHIPPER")
        AddLangSetting(dicDisplayText, Me.lblPod1, "着地1(港)", "POD1(PORT)")
        AddLangSetting(dicDisplayText, Me.lblPol2, "発地2(港)", "POL2(PORT)")
        AddLangSetting(dicDisplayText, Me.lblPod2, "着地2(港)", "POD2(PORT)")
        AddLangSetting(dicDisplayText, Me.btnCreate, "作成", "Create")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub
    ''' <summary>
    ''' ブレーカーリストアイテムを設定
    ''' </summary>
    Private Sub SetBreakerTypeListItem(selectedValue As String)
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue
        With COA0017FixValue

            .COMPCODE = GBC_COMPCODE_D
            .CLAS = "GBT00001BRTYPE"
            .LISTBOX1 = Me.lbBreakerType
            .COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                Me.lbBreakerType = DirectCast(.LISTBOX1, ListBox)
                'テキストボックスに入力した内容と合致する場合は一覧を選択状態
                If Me.lbBreakerType.Items IsNot Nothing AndAlso
                   Me.lbBreakerType.Items.Count > 0 Then
                    Dim findListItem = Me.lbBreakerType.Items.FindByValue(selectedValue)
                    If findListItem IsNot Nothing Then
                        findListItem.Selected = True
                    End If
                End If
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
            End If
        End With

    End Sub
    ''' <summary>
    ''' 輸送パターンデータテーブル取得
    ''' </summary>
    ''' <param name="breakerType">ブレーカー種類</param>
    ''' <returns>輸送パターンデータテーブル</returns>
    ''' <param name="useType">単一取得の場合はUseTypeを指定</param>
    ''' <remarks>GBM0009よりブレーカー一覧を取得</remarks>
    Private Function GetTransferPattern(breakerType As String, Optional ByVal useType As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT MAIN.USETYPE")
        sqlStat.AppendLine("     , MAIN.NAMES")
        sqlStat.AppendLine("     , SUM(CASE MAIN.AGENTKBN WHEN 'POD1' THEN 1 ELSE 0 END) AS POD1COUNT ")
        sqlStat.AppendLine("     , SUM(CASE MAIN.AGENTKBN WHEN 'POL1' THEN 1 ELSE 0 END) AS POL1COUNT ")
        sqlStat.AppendLine("     , SUM(CASE MAIN.AGENTKBN WHEN 'POD2' THEN 1 ELSE 0 END) AS POD2COUNT ")
        sqlStat.AppendLine("     , SUM(CASE MAIN.AGENTKBN WHEN 'POL2' THEN 1 ELSE 0 END) AS POL2COUNT ")
        sqlStat.AppendLine("     , ISNULL(SUB.INVOICEDBY,'') AS INVOICEDBY")
        sqlStat.AppendLine("     , ISNULL(SUB.BILLINGCATEGORY,'') AS BILLINGCATEGORY")
        sqlStat.AppendLine("     , ISNULL(SUB.LOADPORT1,'') AS LOADPORT1")
        sqlStat.AppendLine("     , ISNULL(SUB.DISCHARGEPORT1,'') AS DISCHARGEPORT1")
        sqlStat.AppendLine("     , ISNULL(SUB.LOADPORT2,'') AS LOADPORT2")
        sqlStat.AppendLine("     , ISNULL(SUB.DISCHARGEPORT2,'') AS DISCHARGEPORT2")
        sqlStat.AppendLine("     , ISNULL(SUB.SHIPPER,'') AS SHIPPER")
        sqlStat.AppendLine("     , ISNULL(SUB.CONSIGNEE,'') AS CONSIGNEE")
        sqlStat.AppendLine("     , ISNULL(SUB.PRODUCTCODE,'') AS PRODUCTCODE")
        sqlStat.AppendLine("     , ISNULL(SUB.AGENTPOL1,'') AS AGENTPOL1")
        sqlStat.AppendLine("     , ISNULL(SUB.AGENTPOD1,'') AS AGENTPOD1")
        sqlStat.AppendLine("     , ISNULL(SUB.AGENTPOL2,'') AS AGENTPOL2")
        sqlStat.AppendLine("     , ISNULL(SUB.AGENTPOD2,'') AS AGENTPOD2")
        sqlStat.AppendLine("  FROM GBM0009_TRPATTERN MAIN ")
        sqlStat.AppendLine("      LEFT OUTER JOIN GBM0029_TRPATTERNSUB SUB")
        sqlStat.AppendLine("        ON  SUB.COMPCODE = MAIN.COMPCODE")
        sqlStat.AppendLine("        AND SUB.ORG      = MAIN.ORG")
        sqlStat.AppendLine("        AND SUB.USETYPE  = MAIN.USETYPE")
        sqlStat.AppendLine("        AND SUB.STYMD   <= @STYMD")
        sqlStat.AppendLine("        AND SUB.ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("        AND SUB.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" WHERE MAIN.COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND MAIN.ORG         = @ORG")
        sqlStat.AppendLine("   AND MAIN.BRTYPE = @BREAKERTYPE")
        If useType <> "" Then
            sqlStat.AppendLine("   AND MAIN.USETYPE     = @USETYPE")
        End If
        sqlStat.AppendLine("   AND MAIN.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND MAIN.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND MAIN.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("GROUP BY MAIN.USETYPE, MAIN.NAMES")
        sqlStat.AppendLine(" ,SUB.LOADPORT1 ,SUB.DISCHARGEPORT1 ,SUB.LOADPORT2 ,SUB.DISCHARGEPORT2 ,SUB.SHIPPER ")
        sqlStat.AppendLine(" ,SUB.INVOICEDBY ,SUB.BILLINGCATEGORY ,SUB.CONSIGNEE ,SUB.PRODUCTCODE ,SUB.AGENTPOL1 ,SUB.AGENTPOD1 ,SUB.AGENTPOL2 ,SUB.AGENTPOD2 ")
        sqlStat.AppendLine("ORDER BY MAIN.USETYPE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            Dim paramOrg As SqlParameter = sqlCmd.Parameters.Add("@ORG", SqlDbType.NVarChar, 20)
            Dim paramBreakerType As SqlParameter = sqlCmd.Parameters.Add("@BREAKERTYPE", SqlDbType.NVarChar, 20)
            Dim paramUseType As SqlParameter = Nothing
            If useType <> "" Then
                paramUseType = sqlCmd.Parameters.Add("@USETYPE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp")
            paramOrg.Value = "GB_Default" '一旦GB_Default固定
            '開発用
            Dim breakerTypeDev As String = C_BRTYPE.SALES
            If breakerType <> "1" Then
                breakerTypeDev = C_BRTYPE.OPERATION
            End If
            paramBreakerType.Value = breakerTypeDev '輸送パターンのブレーカータイプはコードではなく文字で設定されている！いったん開発の仮うち
            If useType <> "" Then
                paramUseType.Value = useType
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 国データ取得
    ''' </summary>
    ''' <param name="countryCode">国コード(オプショナル)未指定時は全件</param>
    ''' <returns>国データテーブル</returns>
    ''' <remarks>GBM0001_COUNTRYより国データを取得</remarks>
    Private Function GetCountry(Optional ByVal countryCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim textField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMES"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT COUNTRYCODE")
        sqlStat.AppendFormat("     , {0} AS NAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0001_COUNTRY")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")

        If countryCode <> "" Then
            sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY COUNTRYCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 10)
            Dim paramCountryCode As SqlParameter = Nothing
            If countryCode <> "" Then
                paramCountryCode = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 10)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            If countryCode <> "" Then
                paramCountryCode.Value = countryCode
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 港検索
    ''' </summary>
    ''' <param name="portCode">港コード(オプショナル、未指定の場合は国に対する港全件)</param>
    ''' <returns>対象の港データテーブル</returns>
    ''' <remarks>GBM0002_PORTより引数条件に一致する港を検索、返却する</remarks>
    Private Function GetPort(Optional portCode As String = "", Optional countryCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT PORTCODE")
        sqlStat.AppendLine("      ,AREANAME AS NAME")
        sqlStat.AppendLine("      ,PORTCODE + ':' + AREANAME AS LISTBOXNAME")
        sqlStat.AppendLine("      ,COUNTRYCODE AS COUNTRYCODE")
        sqlStat.AppendLine("      ,AREACODE AS AREACODE")
        sqlStat.AppendLine("  FROM GBM0002_PORT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If portCode <> "" Then
            sqlStat.AppendLine("   AND PORTCODE    = @PORTCODE")
        End If
        If countryCode <> "" Then
            sqlStat.AppendLine("   AND COUNTRYCODE    = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY PORTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            Dim paramPortCode As SqlParameter = Nothing
            Dim paramCountryCode As SqlParameter = Nothing
            If portCode <> "" Then
                paramPortCode = sqlCmd.Parameters.Add("@PORTCODE", SqlDbType.NVarChar, 20)
            End If
            If countryCode <> "" Then
                paramCountryCode = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            If portCode <> "" Then
                paramPortCode.Value = portCode
            End If
            If countryCode <> "" Then
                paramCountryCode.Value = countryCode
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 荷主一覧取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <param name="customerCode">顧客コード(オプショナル)未指定時は国コードで絞りこんだ全件</param>
    ''' <returns>荷主一覧データテーブル</returns>
    ''' <remarks>GBM0004_CUSTOMERより荷主一覧を取得</remarks>
    Private Function GetShipper(countryCode As String, Optional customerCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CUSTOMERCODE")
        sqlStat.AppendFormat("      ,{0} AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,CUSTOMERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If customerCode <> "" Then
            sqlStat.AppendLine("   AND CUSTOMERCODE    = @CUSTOMERCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("ORDER BY CUSTOMERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            Dim paramCustomerCode As SqlParameter = Nothing
            If customerCode <> "" Then
                paramCustomerCode = sqlCmd.Parameters.Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramCountryCode.Value = countryCode
            If customerCode <> "" Then
                paramCustomerCode.Value = customerCode
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 対象Agent一覧を取得
    ''' </summary>
    ''' <param name="countryCode"></param>
    ''' <param name="carrierCode">業者コード</param>
    ''' <returns></returns>
    ''' <remarks>GBM0005_TRADERより引数国コードをもとにCLASS='AGENT'の一覧を取得</remarks>
    Private Function GetAgent(countryCode As String, Optional carrierCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim textField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMES"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CARRIERCODE AS CODE")
        sqlStat.AppendFormat("     , CARRIERCODE + ':' + {0} As LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0005_TRADER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If countryCode <> "" Then
            sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   AND CLASS       = '" & C_TRADER.CLASS.AGENT & "'")
        If carrierCode <> "" Then
            sqlStat.AppendLine("   And CARRIERCODE    = @CARRIERCODE")
        End If
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY CARRIERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 10)
            Dim paramCountryCode As SqlParameter = Nothing
            If countryCode <> "" Then
                paramCountryCode = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramcarrierCode As SqlParameter = Nothing
            If carrierCode <> "" Then
                paramcarrierCode = sqlCmd.Parameters.Add("@CARRIERCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            If countryCode <> "" Then
                paramCountryCode.Value = countryCode
            End If
            If carrierCode <> "" Then
                paramcarrierCode.Value = carrierCode
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' ブレーカー基本情報取得処理
    ''' </summary>
    ''' <param name="brNo">ブレーカーNo</param>
    ''' <returns></returns>
    ''' <remarks>コピー用機能より遷移した場合に既存ブレーカーの輸送パターン・港情報等を取得</remarks>
    Private Function GetBreakerBase(brNo As String, Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = Nothing
        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("SELECT BS.BRID     AS BRID")
        sqlStat.AppendLine("      ,BI.USETYPE  AS USETYPE")
        sqlStat.AppendLine("      ,BI.BRTYPE   AS BRTYPE")
        sqlStat.AppendLine("      ,BS.SHIPPER AS SHIPPER")
        sqlStat.AppendLine("      ,BS.CONSIGNEE AS CONSIGNEE")
        sqlStat.AppendLine("      ,BS.RECIEPTCOUNTRY1 AS RECIEPTCOUNTRY1")
        sqlStat.AppendLine("      ,BS.RECIEPTPORT1 AS RECIEPTPORT1")
        sqlStat.AppendLine("      ,BS.RECIEPTCOUNTRY2 AS RECIEPTCOUNTRY2")
        sqlStat.AppendLine("      ,BS.RECIEPTPORT2 AS RECIEPTPORT2")
        sqlStat.AppendLine("      ,BS.LOADCOUNTRY1 AS LOADCOUNTRY1")
        sqlStat.AppendLine("      ,BS.LOADPORT1 AS LOADPORT1")
        sqlStat.AppendLine("      ,BS.LOADCOUNTRY2 AS LOADCOUNTRY2")
        sqlStat.AppendLine("      ,BS.LOADPORT2 AS LOADPORT2")
        sqlStat.AppendLine("      ,BS.DISCHARGECOUNTRY1 AS DISCHARGECOUNTRY1")
        sqlStat.AppendLine("      ,BS.DISCHARGEPORT1 AS DISCHARGEPORT1")
        sqlStat.AppendLine("      ,BS.DISCHARGECOUNTRY2 AS DISCHARGECOUNTRY2")
        sqlStat.AppendLine("      ,BS.DISCHARGEPORT2 AS DISCHARGEPORT2")
        sqlStat.AppendLine("      ,BS.DELIVERYCOUNTRY1 AS DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,BS.DELIVERYPORT1 AS DELIVERYPORT1")
        sqlStat.AppendLine("      ,BS.DELIVERYCOUNTRY2 AS DELIVERYCOUNTRY2")
        sqlStat.AppendLine("      ,BS.DELIVERYPORT2 AS DELIVERYPORT2")
        sqlStat.AppendLine("      ,CASE BS.ETD1 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETD1,'yyyy/MM/dd') END AS ETD1")
        sqlStat.AppendLine("      ,CASE BS.ETA1 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETA1,'yyyy/MM/dd') END AS ETA1")
        sqlStat.AppendLine("      ,CASE BS.ETD2 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETD2,'yyyy/MM/dd') END AS ETD2")
        sqlStat.AppendLine("      ,CASE BS.ETA2 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETA2,'yyyy/MM/dd') END AS ETA2")
        sqlStat.AppendLine("      ,BS.AGENTORGANIZER AS AGENTORGANIZER")
        sqlStat.AppendLine("      ,BS.AGENTPOL1 AS AGENTPOL1")
        sqlStat.AppendLine("      ,BS.AGENTPOL2 AS AGENTPOL2")
        sqlStat.AppendLine("      ,BS.AGENTPOD1 AS AGENTPOD1")
        sqlStat.AppendLine("      ,BS.AGENTPOD2 AS AGENTPOD2")
        sqlStat.AppendLine("      ,BS.APPLYTEXT AS APPLYTEXT")
        sqlStat.AppendLine("      ,BS.COUNTRYORGANIZER AS COUNTRYORGANIZER")
        sqlStat.AppendLine("      ,BS.LASTORDERNO AS LASTORDERNO")
        sqlStat.AppendLine("      ,BS.TANKNO AS TANKNO")
        sqlStat.AppendLine("      ,BS.DEPOTCODE AS DEPOTCODE")
        sqlStat.AppendLine("      ,BS.TWOAGOPRODUCT AS TWOAGOPRODUCT")
        sqlStat.AppendLine("      ,BS.FEE AS FEE")
        sqlStat.AppendLine("      ,BS.BILLINGCATEGORY AS BILLINGCATEGORY")
        sqlStat.AppendLine("      ,BS.USINGLEASETANK  AS USINGLEASETANK")
        sqlStat.AppendLine("  FROM       GBT0002_BR_BASE BS")
        sqlStat.AppendLine("  INNER JOIN GBT0001_BR_INFO BI")
        sqlStat.AppendLine("          ON BI.BRID     = BS.BRID")
        sqlStat.AppendLine("         AND BI.LINKID   = BS.BRBASEID")
        sqlStat.AppendLine("         AND BI.TYPE     = 'INFO'")
        sqlStat.AppendLine("         AND BI.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" WHERE BS.BRID     = @BRID ")
        sqlStat.AppendLine("   AND BS.DELFLG  <> @DELFLG ")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar).Value = brNo
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    retDt = CommonFunctions.DeepCopy(dt)
                End Using
            End Using

            Return retDt
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try

        Return retDt
    End Function
    ''' <summary>
    ''' ラベル、3国間の情報を画面に設定する。
    ''' </summary>
    Private Sub SetDisplayTransferPattern(breakerType As String, useType As String)
        '一旦保持情報をクリア
        Me.lblTransferPatternText.Text = ""
        Me.hdnIsTrilateral.Value = ""
        ViewState(CONST_VS_POLPODCNT) = Nothing
        '引数がNull
        If breakerType = "" OrElse useType = "" Then
            Return
        End If
        '輸送パターンを検索取得できない場合はそのまま終了
        Dim dt As DataTable = GetTransferPattern(breakerType, useType.Trim)
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        '取得結果を画面に設定
        Me.txtTransferPattern.Text = useType.Trim
        Me.lblTransferPatternText.Text = HttpUtility.HtmlEncode(Convert.ToString(dt.Rows(0).Item("NAMES")))
        If Convert.ToString(dt.Rows(0).Item("POD2COUNT")) <> "0" Then
            Me.hdnIsTrilateral.Value = "1"
        End If
        Dim dicPolPodCnt As New Dictionary(Of String, Integer)
        For Each colObj As DataColumn In dt.Columns
            'カラム名末尾が"COUNT"とつくデータを保持（発１のみ判定用※本来はPOD1の数さえ押さえれば良いが念のためすべて保持）
            If colObj.ColumnName.EndsWith("COUNT") Then
                Dim costItemCnt As Integer = CInt(dt.Rows(0).Item(colObj.ColumnName))
                dicPolPodCnt.Add(colObj.ColumnName, costItemCnt)
            End If
        Next

        ' 初期値設定　※サブテーブルに情報がある場合
        If Convert.ToString(dt.Rows(0).Item("LOADPORT1")) <> "" Then
            Me.txtPolPort1.Text = HttpUtility.HtmlEncode(Convert.ToString(dt.Rows(0).Item("LOADPORT1")))
            txtPolPort1_Change()
        End If
        If Convert.ToString(dt.Rows(0).Item("DISCHARGEPORT1")) <> "" Then
            Me.txtPodPort1.Text = HttpUtility.HtmlEncode(Convert.ToString(dt.Rows(0).Item("DISCHARGEPORT1")))
            txtPodPort1_Change()
        End If
        If Convert.ToString(dt.Rows(0).Item("LOADPORT2")) <> "" Then
            Me.txtPolPort2.Text = HttpUtility.HtmlEncode(Convert.ToString(dt.Rows(0).Item("LOADPORT2")))
            txtPolPort2_Change()
        End If
        If Convert.ToString(dt.Rows(0).Item("DISCHARGEPORT2")) <> "" Then
            Me.txtPodPort2.Text = HttpUtility.HtmlEncode(Convert.ToString(dt.Rows(0).Item("DISCHARGEPORT2")))
            txtPodPort2_Change()
        End If
        If Convert.ToString(dt.Rows(0).Item("SHIPPER")) <> "" Then
            Me.txtShipper.Text = HttpUtility.HtmlEncode(Convert.ToString(dt.Rows(0).Item("SHIPPER")))
            txtShipper_Change()
        End If
        Me.hdnInitInvoicedBy.Value = Convert.ToString(dt.Rows(0).Item("INVOICEDBY"))
        Me.hdnInitBillingCategory.Value = Convert.ToString(dt.Rows(0).Item("BILLINGCATEGORY"))
        Me.hdnInitConsignee.Value = Convert.ToString(dt.Rows(0).Item("CONSIGNEE"))
        Me.hdnInitProductCode.Value = Convert.ToString(dt.Rows(0).Item("PRODUCTCODE"))
        Me.hdnInitAgentPol1.Value = Convert.ToString(dt.Rows(0).Item("AGENTPOL1"))
        Me.hdnInitAgentPod1.Value = Convert.ToString(dt.Rows(0).Item("AGENTPOD1"))
        Me.hdnInitAgentPol2.Value = Convert.ToString(dt.Rows(0).Item("AGENTPOL2"))
        Me.hdnInitAgentPod2.Value = Convert.ToString(dt.Rows(0).Item("AGENTPOD2"))

        ViewState(CONST_VS_POLPODCNT) = dicPolPodCnt
    End Sub
    ''' <summary>
    ''' 港名称を画面に設定
    ''' </summary>
    ''' <param name="targetTextObject">コード入力する対象のテキストボックス</param>
    ''' <param name="portCode">港コード</param>
    Private Sub SetDisplayPort(targetTextObject As TextBox, portCode As String)
        Dim targetLabel As Label = Nothing
        Select Case targetTextObject.ID
            Case Me.txtPolPort1.ID
                targetLabel = Me.lblPolPort1Text
                Me.txtShipper.Text = ""
                Me.lblShipperText.Text = ""
            Case Me.txtPodPort1.ID
                targetLabel = Me.lblPodPort1Text
            Case Me.txtPolPort2.ID
                targetLabel = Me.lblPolPort2Text
            Case Me.txtPodPort2.ID
                targetLabel = Me.lblPodPort2Text
        End Select
        '一旦リセット
        targetTextObject.Text = portCode.Trim
        targetLabel.Text = ""
        '港コードが未入力の場合はDBアクセスせずに終了
        If portCode.Trim = "" Then
            Return
        End If

        Dim dt As DataTable = GetPort(portCode.Trim)

        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)

        Select Case targetTextObject.ID
            Case Me.txtPolPort1.ID
                Me.hdnPolCountry1.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                Me.hdnPolPort1.Value = Convert.ToString(dr.Item("AREACODE"))
            Case Me.txtPodPort1.ID
                Me.hdnPodCountry1.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                Me.hdnPodPort1.Value = Convert.ToString(dr.Item("AREACODE"))
            Case Me.txtPolPort2.ID
                Me.hdnPolCountry2.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                Me.hdnPolPort2.Value = Convert.ToString(dr.Item("AREACODE"))
            Case Me.txtPodPort2.ID
                Me.hdnPodCountry2.Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                Me.hdnPodPort2.Value = Convert.ToString(dr.Item("AREACODE"))
        End Select
        targetLabel.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
    End Sub
    ''' <summary>
    ''' 荷主名称を画面に設定
    ''' </summary>
    ''' <param name="targetTextObject">対象テキスト</param>
    ''' <param name="customerCode">荷主コード（顧客コード）</param>
    Private Sub SetDisplayShipper(targetTextObject As TextBox, customerCode As String)
        '一旦リセット
        targetTextObject.Text = customerCode.Trim
        Me.lblShipperText.Text = ""
        '荷主コード（顧客コード）が未入力の場合はDBアクセスせずに終了
        If customerCode.Trim = "" Then
            Return
        End If
        Dim countryCode As String = Me.hdnPolCountry1.Value

        Dim dt As DataTable = New DataTable
        If Me.txtBreakerType.Text = "1" Then
            dt = GetShipper(countryCode, customerCode.Trim)
        Else
            dt = GetAgent(countryCode, customerCode.Trim)
        End If

        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)
        Me.lblShipperText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
    End Sub
    ''' <summary>
    ''' 前画面情報保持
    ''' </summary>
    Private Sub SetPrevDisplayInfo()
        If TypeOf Page.PreviousPage Is GBT00002RESULT Then
            '検索画面の場合
            Dim prevObj As GBT00002RESULT = DirectCast(Page.PreviousPage, GBT00002RESULT)
            Me.GBT00002RValues = prevObj.ThisScreenValues
            ViewState(CONST_VS_NAME_GBT00002RV) = prevObj.ThisScreenValues
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPort", Me.hdnPort},
                                                                        {"hdnApproval", Me.hdnApproval},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchBreakerType", Me.hdnSearchBreakerType}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            Dim tmpBkTp As Control = prevObj.FindControl("hdnBreakerType")
            If tmpBkTp IsNot Nothing Then
                Dim tmpHdn As HiddenField = DirectCast(tmpBkTp, HiddenField)
                Me.hdnBreakerType.Value = tmpHdn.Value
                Me.txtBreakerType.Text = tmpHdn.Value
                txtBreakerType_Change()
            End If
            Me.hdnCopyBaseBrId.Value = prevObj.CopyBrId
        End If
    End Sub
    ''' <summary>
    ''' 初期設定
    ''' </summary>
    Private Sub InitSetteing()
        '新規作成時の場合
        If Me.hdnThisMapVariant.Value.EndsWith("New") Then
            '輸送パターンの初期値取得
            Dim COA0016VARIget As New BASEDLL.COA0016VARIget        '変数情報取
            COA0016VARIget.MAPID = CONST_MAPID
            COA0016VARIget.COMPCODE = ""
            COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
            If Me.hdnBreakerType.Value = "1" Then
                COA0016VARIget.FIELD = "TRANSPATSALES"
            Else
                COA0016VARIget.FIELD = "TRANSPATOPE"
            End If
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                Me.txtTransferPattern.Text = COA0016VARIget.VALUE
                txtTransferPattern_Change()
            Else
                CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
                Return
            End If

            '港の初期値取得
            Dim dt As DataTable = GetPort("", GBA00003UserSetting.COUNTRYCODE)

            'データが取れない場合はそのまま終了
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                Return
            End If
            Dim dr As DataRow = dt.Rows(0)

            Me.txtPolPort1.Text = Convert.ToString(dr.Item("PORTCODE"))
            txtPolPort1_Change()
        End If
        'コピー時の遷移
        If Me.hdnThisMapVariant.Value.EndsWith("Copy") Then
            Dim dt As DataTable = GetBreakerBase(Me.hdnCopyBaseBrId.Value)
            '画面にコピー元の情報を設定
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then 'ありえないがデータが取れない場合(例外スロー)
                Throw New Exception(String.Format("BrId is not Found.{0}", Me.hdnCopyBaseBrId.Value))
            End If
            '画面にブレーカー情報を展開
            Dim dr As DataRow = dt.Rows(0)
            '輸送パターン
            Me.txtTransferPattern.Text = Convert.ToString(dr.Item("USETYPE"))
            txtTransferPattern_Change()
            '発港1
            Me.txtPolPort1.Text = Convert.ToString(dr.Item("RECIEPTPORT1"))
            txtPolPort1_Change()
            '荷主
            Me.txtShipper.Text = Convert.ToString(dr.Item("SHIPPER"))
            txtShipper_Change()
            '着港1
            Me.txtPodPort1.Text = Convert.ToString(dr.Item("DISCHARGEPORT1"))
            txtPodPort1_Change()
            '発港2
            Me.txtPolPort2.Text = Convert.ToString(dr.Item("RECIEPTPORT2"))
            txtPolPort2_Change()
            '着港2
            Me.txtPodPort2.Text = Convert.ToString(dr.Item("DISCHARGEPORT2"))
            txtPodPort2_Change()
            'リースタンク利用チェックボックス
            If dr.Item("USINGLEASETANK").Equals("1") Then
                Me.chkLeaseTankUse.Checked = True
            End If
        End If

    End Sub
End Class
