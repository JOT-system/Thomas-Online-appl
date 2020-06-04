Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL
Imports Microsoft.Office.Interop
''' <summary>
''' 帳票出力画面クラス
''' </summary>
Public Class GBT00016FORMOUTPUT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00016"     '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00016"
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
                DefaultValueSet()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                Me.btnEnter.Focus()
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
                Case Me.vLeftDepartment.ID
                    SetDepartmentListItem(Me.txtDepartment.Text)

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
    ''' 実行ボタン押下時
    ''' </summary>
    Public Sub btnEnter_Click()
        'チェック処理
        checkProc()
        If returnCode <> C_MESSAGENO.NORMAL Then
            Return
        End If

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        Dim reportMapId As String = ""

        '帳票出力
        Dim tmpFile As String = ""
        Dim outUrl As String = ""

        'データ取得
        Dim dt As DataTable = Nothing
        Dim int As Integer = 0
        Dim atchFlg As Boolean = False
        Dim delFlg As Boolean = True
        reportMapId = "GBT00016"

        Select Case reportId
            Case "B/L"

                dt = CollectDisplayReportInfoBL(Me.txtOrderNo.Text, Me.txtTransClass.Text)

                If Convert.ToInt64(dt.Rows(0).Item("TANKCNT")) > 1 Then
                    int = 1
                    delFlg = False
                End If

                For i As Integer = 0 To int

                    If atchFlg Then
                        reportId = "Attached"

                        If dt.Rows(0).Item("BLID").ToString <> "" Then

                            dt.Rows(0).Item("BLID") = "B/L No. : " & dt.Rows(0).Item("BLID").ToString
                        End If

                        If dt.Rows(0).Item("VOY").ToString <> "" Then

                            dt.Rows(0).Item("VOY") = "Voyage No. : " & dt.Rows(0).Item("VOY").ToString
                        End If

                        If dt.Rows(0).Item("VSL").ToString <> "" Then

                            dt.Rows(0).Item("VSL") = "Vessel Name : " & dt.Rows(0).Item("VSL").ToString
                        End If

                        If dt.Rows(0).Item("ATTMARKS").ToString <> "" Then

                            dt.Rows(0).Item("ATTMARKS") = "[Marks & Numbers]" & vbCrLf & vbCrLf & dt.Rows(0).Item("ATTMARKS").ToString
                        End If
                    End If

                            With Nothing
                        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                        COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                        COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                        COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
                        COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                        If atchFlg Then
                            COA0027ReportTable.ADDSHEET = "Attached Sheet"                  'PARAM07:追記シート（任意）
                            If tmpFile <> "" Then
                                COA0027ReportTable.ADDFILE = tmpFile                        'PARAM06:追記ファイル（フルパス（O_FILEpath））
                            End If
                        End If

                        COA0027ReportTable.COA0027ReportTable()

                        dt.Columns.Remove("ROWKEY")
                        dt.Columns.Remove("CELLNO")
                        dt.Columns.Remove("ROWCNT")

                        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                        Else
                            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                            Return
                        End If

                        atchFlg = True
                        tmpFile = COA0027ReportTable.FILEpath
                        outUrl = COA0027ReportTable.URL

                    End With

                Next

            Case "ShippingAdvice"

                dt = CollectDisplayReportInfoSA(Me.txtOrderNo.Text, Me.txtTransClass.Text)

                If dt.Rows.Count > 0 Then

                    Dim shipText As String() = Nothing

                    shipText = Split(dt.Rows(0).Item("SHIPPERTEXT1").ToString, vbCrLf)

                    For i As Integer = 0 To shipText.Count - 1

                        If i > 3 Then
                            Exit For
                        End If

                        dt.Rows(0).Item("SHIPPERTEXT" & (i + 1).ToString) = shipText(i)

                    Next

                    Dim consText As String() = Nothing

                    consText = Split(dt.Rows(0).Item("CONSIGNEETEXT1").ToString, vbCrLf)

                    For i As Integer = 0 To consText.Count - 1

                        If i > 4 Then
                            Exit For
                        End If

                        dt.Rows(0).Item("CONSIGNEETEXT" & (i + 1).ToString) = consText(i)

                    Next

                    Dim notfText As String() = Nothing

                    notfText = Split(dt.Rows(0).Item("NOTIFYTEXT1").ToString, vbCrLf)

                    For i As Integer = 0 To notfText.Count - 1

                        If i > 2 Then
                            Exit For
                        End If

                        dt.Rows(0).Item("NOTIFYTEXT" & (i + 1).ToString) = notfText(i)

                    Next

                End If

                With Nothing
                    Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                    COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                    COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                    COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
                    COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                    COA0027ReportTable.COA0027ReportTable()

                    If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                    Else
                        CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                        Return
                    End If

                    tmpFile = COA0027ReportTable.FILEpath
                    outUrl = COA0027ReportTable.URL

                End With

            Case "AccountingCollaboration"

                dt = CollectDisplayReportInfoAC(Me.txtOrderNo.Text, Me.txtTransClass.Text, Me.txtDepartment.Text)

                If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage)
                    Return
                End If

                Dim targetDt = (From dr In dt
                                Where Convert.ToString(dr.Item("DATACRITERIA")) <> "").CopyToDataTable

                If targetDt Is Nothing OrElse targetDt.Rows.Count = 0 Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage)
                    Return
                End If

                Dim voucher As Integer = 1
                Dim ditail As Integer = 1
                Dim toriCode As String = Convert.ToString(targetDt.Rows(0).Item("TORICODE"))
                Dim revenue As String = Convert.ToString(targetDt.Rows(0).Item("REVENUE"))

                For Each dr As DataRow In targetDt.Rows

                    If Convert.ToString(dr.Item("BOTHCLASS")) = "B" Then
                        '両建
                        If Not (toriCode.Equals(dr.Item("TORICODE")) _
                            AndAlso revenue.Equals(dr.Item("REVENUE"))) Then

                            voucher += 1
                            ditail = 1
                            toriCode = Convert.ToString(dr.Item("TORICODE"))
                            revenue = Convert.ToString(dr.Item("REVENUE"))
                        End If
                    Else
                        '相殺
                        If Not (toriCode.Equals(dr.Item("TORICODE"))) Then

                            voucher += 1
                            ditail = 1
                            toriCode = Convert.ToString(dr.Item("TORICODE"))
                            revenue = Convert.ToString(dr.Item("REVENUE"))

                        End If
                    End If

                    '伝票番号
                    dr.Item("SLIPNUMBER") = voucher.ToString("00000000")
                    '伝票NO
                    dr.Item("SLIPNO") = voucher.ToString
                    '明細行番号
                    dr.Item("DETAILLINENO") = ditail.ToString("000")

                    '期日設定
                    dr.Item("DEADLINE") = GetPayDay(Convert.ToString(dr.Item("REPORTMONTH")), Convert.ToString(dr.Item("HOLIDAYFLG")), Convert.ToString(dr.Item("PAYDAY")))

                    ditail += 1

                    '借方汎用補助1
                    If Convert.ToString(dr.Item("DBGENERALPURPOSE")) = "1" Then

                        Select Case Convert.ToInt32(dr.Item("DEBAMOUNT"))

                            Case 0 To 99999
                                dr.Item("DEBGENPURPOSE") = "1"

                            Case 100000 To 199999
                                dr.Item("DEBGENPURPOSE") = "2"

                            Case Else
                                dr.Item("DEBGENPURPOSE") = "9"

                        End Select

                    Else
                        dr.Item("DEBGENPURPOSE") = "0"
                    End If

                    '貸方汎用補助1
                    If Convert.ToString(dr.Item("CRGENERALPURPOSE")) = "1" Then

                        Select Case Convert.ToInt32(dr.Item("CREAMOUNT"))

                            Case 0 To 99999
                                dr.Item("CREGENPURPOSE") = "1"

                            Case 100000 To 199999
                                dr.Item("CREGENPURPOSE") = "2"

                            Case Else
                                dr.Item("CREGENPURPOSE") = "9"

                        End Select

                    Else
                        dr.Item("CREGENPURPOSE") = "0"
                    End If

                Next

                With Nothing
                    Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                    COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                    COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                    COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
                    COA0027ReportTable.TBLDATA = targetDt                              'PARAM04:データ参照tabledata
                    COA0027ReportTable.COA0027ReportTable()

                    If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                    Else
                        CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                        Return
                    End If

                    tmpFile = COA0027ReportTable.FILEpath
                    outUrl = COA0027ReportTable.URL

                End With

        End Select

        '別画面でExcelを表示
        hdnPrintURL.Value = outUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

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
                Case Me.vLeftDepartment.ID 'アクティブなビューが承認
                    '部門選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbDepartment.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbDepartment.SelectedItem.Value
                            Me.lblDepartmentText.Text = Me.lbDepartment.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblDepartmentText.Text = ""
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
        AddLangSetting(dicDisplayText, Me.btnEnter, "実行", "Print")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblOrderNo, "", "ReportMonth")
        AddLangSetting(dicDisplayText, Me.lblTransClass, "", "CountryCode")

        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub

    ''' <summary>
    ''' 初期表示
    ''' </summary>
    Public Sub DefaultValueSet()
        'RightBox情報設定
        rightBoxSet()
        If returnCode <> C_MESSAGENO.NORMAL Then
            Return
        End If
        '選択画面の入力初期値設定
        'メニューから遷移/業務画面戻り判定
        If Page.PreviousPage Is Nothing OrElse TypeOf Page.PreviousPage IsNot GBT00016FORMOUTPUT Then

            'メニューからの画面遷移
            '○画面項目設定（変数より）処理
            variableSet()
            If returnCode <> C_MESSAGENO.NORMAL Then
                Return
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00016FORMOUTPUT Then
            Dim prevPage As GBT00016FORMOUTPUT = DirectCast(Page.PreviousPage, GBT00016FORMOUTPUT)
            ''一覧画面からの画面遷移
            ''絞り込みはなし
            'Dim tmpHdn As HiddenField = DirectCast(prevPage.FindControl("hdnPrevViewID"), HiddenField)
            'If tmpHdn IsNot Nothing AndAlso
            '    Me.lbRightList.Items.FindByValue(tmpHdn.Value) IsNot Nothing Then
            '    Me.lbRightList.SelectedValue = tmpHdn.Value
            'End If
        End If

    End Sub
    ''' <summary>
    ''' 変数設定
    ''' </summary>
    Public Sub variableSet()

        Dim COA0016VARIget As New BASEDLL.COA0016VARIget        '変数情報取
        '現状絞り込み条件は設定してない
        'TODO 絞り込み条件作成

    End Sub
    ''' <summary>
    ''' 右ボックス設定
    ''' </summary>
    Public Sub rightBoxSet()
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim excelMapId As String = "GBT00016"

        'RightBOX情報設定
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        retVal = C_MESSAGENO.NORMAL

        '初期化
        Me.txtRightErrorMessage.Text = ""

        'レポートID情報
        COA0022ProfXls.MAPID = excelMapId
        COA0022ProfXls.COA0022getReportId()
        Me.lbRightList.Items.Clear() '一旦選択肢をクリア
        If COA0022ProfXls.ERR = C_MESSAGENO.NORMAL Then
            Try
                Dim listBoxObj As ListBox = DirectCast(COA0022ProfXls.REPORTOBJ, ListBox)
                For Each listItem As ListItem In listBoxObj.Items
                    If listItem.Value <> "Attached" Then
                        Me.lbRightList.Items.Add(listItem)
                    End If
                Next
            Catch ex As Exception
            End Try
        Else
            Return
        End If

        'レポートID変数検索
        COA0016VARIget.MAPID = excelMapId
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = "BLList"
        COA0016VARIget.FIELD = "REPORTID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR <> C_MESSAGENO.NORMAL Then
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
        'とりあえず枠だけは残しています

        '必須チェック
        If txtOrderNo.Text = "" Then
            returnCode = C_MESSAGENO.REQUIREDVALUE
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
            txtOrderNo.Focus()
            Return
        End If

        'If txtTransClass.Text = "" Then
        '    returnCode = C_MESSAGENO.REQUIREDVALUE
        '    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
        '    txtTransClass.Focus()
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
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
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
                CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
            End If
        End If
    End Sub

    ''' <summary>
    ''' オーダー情報取得処理(B/L出力用)
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <param name="tranCls"></param>
    ''' <returns></returns>
    Private Function CollectDisplayReportInfoBL(ByVal orderNo As String, ByVal tranCls As String, Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = New DataTable
        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("SELECT CASE WHEN @TRANCLS = '1' THEN OB.BLID1 ELSE OB.BLID2 END AS BLID")
        sqlStat.AppendLine("      ,OB.SHIPPERTEXT AS SHIPPERTEXT")
        sqlStat.AppendLine("      ,OB.CONSIGNEETEXT AS CONSIGNEETEXT")
        sqlStat.AppendLine("      ,OB.NOTIFYTEXT AS NOTIFYTEXT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.NOTIFYCONTTEXT1 ELSE OB.NOTIFYCONTTEXT2 END AS NOTIFYCONTTEXT")
        sqlStat.AppendLine("      ,OB.FINDESTINATIONTEXT AS FINDESTINATIONTEXT")
        sqlStat.AppendLine("      ,OB.PRECARRIAGETEXT AS PRECARRIAGETEXT")
        sqlStat.AppendLine("      ,ISNULL(PT1.AREANAME,'') + ' ' + FV1.VALUE3 AS PLACEOFRECIEPT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.VSL1 ELSE OB.VSL2 END AS VSL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.VOY1 ELSE OB.VOY2 END AS VOY")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT2.AREANAME,'') = '' THEN '' ELSE PT2.AREANAME + '. ' END + ISNULL(CT2.NAMES,'') AS PORTOFLOADING")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT3.AREANAME,'') = '' THEN '' ELSE PT3.AREANAME + '. ' END + ISNULL(CT3.NAMES,'') AS PORTOFDISCHARGE")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT4.AREANAME,'') = '' THEN '' ELSE PT4.AREANAME + '. ' END + ISNULL(CT4.NAMES,'') AS PLACEOFDELIVERY")

        sqlStat.AppendLine("      ,CASE WHEN (SELECT COUNT(TANKSEQ) FROM GBT0007_ODR_VALUE2 WHERE ORDERNO = @ORDERNO AND TRILATERAL = @TRANCLS AND DELFLG <> @DELFLG) > '1' THEN 'AS PER ATTACHED SHEET' ELSE OB.MARKSANDNUMBERS END AS MARKS")
        sqlStat.AppendLine("      ,OB.TANKINFO AS TANKINFO")

        sqlStat.AppendLine("      ,CASE WHEN OB.NOOFPACKAGE = '' THEN '' ELSE OB.NOOFPACKAGE + 'ISOTANK(S' END AS NOOFPACKAGE")
        sqlStat.AppendLine("      ,OB.GOODSPKGS AS GOODSPKGS")
        sqlStat.AppendLine("      ,CASE WHEN OB.CONTAINERPKGS = '' THEN '' ELSE 'SAY:' + OB.CONTAINERPKGS + '.-' END AS CONTAINERPKGS")

        sqlStat.AppendLine("      ,'KGS)' + CHAR(13) + CHAR(10) + (SELECT CONVERT(NVARCHAR ,CONVERT(money, SUM(GROSSWEIGHT)), 1) ")
        sqlStat.AppendLine("      FROM GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("      WHERE ORDERNO    = @ORDERNO ")
        sqlStat.AppendLine("        AND TRILATERAL = @TRANCLS ")
        sqlStat.AppendLine("        AND DELFLG    <> @DELFLG ")
        sqlStat.AppendLine("      ) AS GROSSTOTAL")

        sqlStat.AppendLine("      ,'Net Weight' + CHAR(13) + CHAR(10) + '(KGS)' + CHAR(13) + CHAR(10) + (SELECT CONVERT(NVARCHAR ,CONVERT(money, SUM(NETWEIGHT)), 1) ")
        sqlStat.AppendLine("      FROM GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("      WHERE ORDERNO    = @ORDERNO ")
        sqlStat.AppendLine("        AND TRILATERAL = @TRANCLS ")
        sqlStat.AppendLine("        AND DELFLG    <> @DELFLG ")
        sqlStat.AppendLine("      ) AS NETTOTAL")

        sqlStat.AppendLine("      ,'M3)' + CHAR(13) + CHAR(10) + REPLACE(CONVERT(NVARCHAR ,CONVERT(money, OB.MEASUREMENT),1), '.00', '') AS MEASUREMENT")
        sqlStat.AppendLine("      ,REPLACE(CONVERT(NVARCHAR ,CONVERT(money, OB.DECLAREDVALUE),1), '.00', '') AS DECLAREDVALUE")
        sqlStat.AppendLine("      ,OB.FREIGHTANDCHARGES AS FREIGHTANDCHARGES")
        sqlStat.AppendLine("      ,OB.REVENUETONS AS REVENUETONS")
        sqlStat.AppendLine("      ,OB.RATE AS RATE")
        sqlStat.AppendLine("      ,OB.PER AS PER")
        sqlStat.AppendLine("      ,OB.PREPAID AS PREPAID")
        sqlStat.AppendLine("      ,OB.COLLECT AS COLLECT")
        sqlStat.AppendLine("      ,CONVERT(NVARCHAR ,CONVERT(money, OB.EXCHANGERATE),1) AS EXCHANGERATE")
        sqlStat.AppendLine("      ,OB.PREPAIDAT AS PREPAIDAT")
        sqlStat.AppendLine("      ,OB.PAYABLEAT AS PAYABLEAT")
        sqlStat.AppendLine("      ,OB.LOCALCURRENCY AS LOCALCURRENCY")
        sqlStat.AppendLine("      ,OB.NOOFBL AS NOOFBL")
        sqlStat.AppendLine("      ,(OB.PREPAIDAT + CASE WHEN OB.PREPAIDAT = '' THEN '' ELSE ' : ' END + CASE WHEN @TRANCLS = '1' THEN CASE OB.BLAPPDATE1 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.BLAPPDATE1,'yyyy-MM-dd') END ELSE CASE OB.BLAPPDATE2 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.BLAPPDATE2,'yyyy-MM-dd') END END) AS ISSUEDATE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.LDNVSL1 ELSE OB.LDNVSL2 END AS LADENVSL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.LDNPOL1 ELSE OB.LDNPOL2 END AS LADENPOL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN CASE OB.LDNDATE1 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.LDNDATE1,'yyyy-MM-dd') END ELSE CASE OB.LDNDATE2 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.LDNDATE2,'yyyy-MM-dd') END END AS LADENDATE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.LDNBY1 ELSE OB.LDNBY2 END AS LADENBY")
        sqlStat.AppendLine("      ,(SELECT COUNT(TANKSEQ) FROM GBT0007_ODR_VALUE2 WHERE ORDERNO = @ORDERNO AND TRILATERAL = @TRANCLS AND DELFLG <> @DELFLG) AS TANKCNT")
        sqlStat.AppendLine("      ,OB.MARKSANDNUMBERS AS ATTMARKS")
        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE OB ")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT1 ")
        sqlStat.AppendLine("    ON PT1.PORTCODE  = (CASE WHEN @TRANCLS = '1' THEN OB.RECIEPTPORT1 ELSE OB.RECIEPTPORT2 END)")
        sqlStat.AppendLine("   AND PT1.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT1.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT1.DELFLG   <> @DELFLG")
        'sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT1 ")
        'sqlStat.AppendLine("    ON CT1.COUNTRYCODE  = PT1.COUNTRYCODE")
        'sqlStat.AppendLine("   AND CT1.STYMD    <= @STYMD")
        'sqlStat.AppendLine("   AND CT1.ENDYMD   >= @ENDYMD")
        'sqlStat.AppendLine("   AND CT1.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1 ")
        sqlStat.AppendLine("    ON FV1.KEYCODE   = OB.TERMTYPE")
        sqlStat.AppendLine("   AND FV1.CLASS     = 'TERM'")
        sqlStat.AppendLine("   AND FV1.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT2 ")
        sqlStat.AppendLine("    ON PT2.PORTCODE  = (CASE WHEN @TRANCLS = '1' THEN OB.LOADPORT1 ELSE OB.LOADPORT2 END)")
        sqlStat.AppendLine("   AND PT2.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT2.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT2.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT2 ")
        sqlStat.AppendLine("    ON CT2.COUNTRYCODE  = PT2.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT2.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT2.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND CT2.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT3 ")
        sqlStat.AppendLine("    ON PT3.PORTCODE  = (CASE WHEN @TRANCLS = '1' THEN OB.DISCHARGEPORT1 ELSE OB.DISCHARGEPORT2 END)")
        sqlStat.AppendLine("   AND PT3.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT3.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT3.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT3 ")
        sqlStat.AppendLine("    ON CT3.COUNTRYCODE  = PT3.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT3.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT3.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND CT3.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT4 ")
        sqlStat.AppendLine("    ON PT4.PORTCODE  = (CASE WHEN @TRANCLS = '1' THEN OB.DELIVERYPORT1 ELSE OB.DELIVERYPORT2 END)")
        sqlStat.AppendLine("   AND PT4.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT4.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT4.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT4 ")
        sqlStat.AppendLine("    ON CT4.COUNTRYCODE  = PT4.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT4.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT4.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND CT4.DELFLG   <> @DELFLG")

        sqlStat.AppendLine(" WHERE OB.ORDERNO  = @ORDERNO ")
        sqlStat.AppendLine("   AND OB.DELFLG  <> @DELFLG ")
        sqlStat.AppendLine("   And OB.STYMD    <= @STYMD")
        sqlStat.AppendLine("   And OB.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine(" ORDER BY OB.ORDERNO ")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                Dim paramOrderNo As SqlParameter = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 20)
                Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                Dim paramStYmd As SqlParameter = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
                Dim paramEndYmd As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
                Dim paramTranCls As SqlParameter = sqlCmd.Parameters.Add("@TRANCLS", SqlDbType.NVarChar, 1)
                'SQLパラメータ値セット
                paramOrderNo.Value = orderNo
                paramDelFlg.Value = CONST_FLAG_YES
                paramStYmd.Value = Date.Now
                paramEndYmd.Value = Date.Now
                paramTranCls.Value = tranCls
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Order base info Error")
                    End If
                    retDt = CreateOrderInfoTableBL()
                    For Each col As DataColumn In dt.Columns
                        retDt.Rows(0)(col.ColumnName) = Convert.ToString(dt.Rows(0)(col.ColumnName))
                    Next

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
    ''' オーダー情報を格納する空のデータテーブルを作成する
    ''' </summary>
    Private Function CreateOrderInfoTableBL() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = "ORDER_INFO"
        retDt.Columns.Add("BLID", GetType(String))
        retDt.Columns.Add("SHIPPERTEXT", GetType(String))
        retDt.Columns.Add("CONSIGNEETEXT", GetType(String))
        retDt.Columns.Add("NOTIFYTEXT", GetType(String))
        retDt.Columns.Add("NOTIFYCONTTEXT", GetType(String))
        retDt.Columns.Add("FINDESTINATIONTEXT", GetType(String))
        retDt.Columns.Add("PRECARRIAGETEXT", GetType(String))
        retDt.Columns.Add("PLACEOFRECIEPT", GetType(String))
        retDt.Columns.Add("VSL", GetType(String))
        retDt.Columns.Add("VOY", GetType(String))
        retDt.Columns.Add("PORTOFLOADING", GetType(String))
        retDt.Columns.Add("PORTOFDISCHARGE", GetType(String))
        retDt.Columns.Add("PLACEOFDELIVERY", GetType(String))
        retDt.Columns.Add("MARKS", GetType(String))
        retDt.Columns.Add("TANKINFO", GetType(String))
        retDt.Columns.Add("NOOFPACKAGE", GetType(String))
        retDt.Columns.Add("GOODSPKGS", GetType(String))
        retDt.Columns.Add("CONTAINERPKGS", GetType(String))
        retDt.Columns.Add("GROSSTOTAL", GetType(String))
        retDt.Columns.Add("NETTOTAL", GetType(String))
        retDt.Columns.Add("MEASUREMENT", GetType(String))
        retDt.Columns.Add("DECLAREDVALUE", GetType(String))
        retDt.Columns.Add("FREIGHTANDCHARGES", GetType(String))
        retDt.Columns.Add("REVENUETONS", GetType(String))
        retDt.Columns.Add("RATE", GetType(String))
        retDt.Columns.Add("PER", GetType(String))
        retDt.Columns.Add("PREPAID", GetType(String))
        retDt.Columns.Add("COLLECT", GetType(String))
        retDt.Columns.Add("EXCHANGERATE", GetType(String))
        retDt.Columns.Add("PREPAIDAT", GetType(String))
        retDt.Columns.Add("PAYABLEAT", GetType(String))
        retDt.Columns.Add("LOCALCURRENCY", GetType(String))
        retDt.Columns.Add("NOOFBL", GetType(String))
        retDt.Columns.Add("ISSUEDATE", GetType(String))
        retDt.Columns.Add("LADENVSL", GetType(String))
        retDt.Columns.Add("LADENPOL", GetType(String))
        retDt.Columns.Add("LADENDATE", GetType(String))
        retDt.Columns.Add("LADENBY", GetType(String))
        retDt.Columns.Add("TANKCNT", GetType(String))
        retDt.Columns.Add("ATTMARKS", GetType(String))

        '検討中
        retDt.Columns.Add("DUMMY", GetType(String))
        retDt.Columns.Add("DUMMY2", GetType(String))

        Dim dr As DataRow = retDt.NewRow
        dr.Item("DUMMY") = ""
        retDt.Rows.Add(dr)
        Return retDt
    End Function

    ''' <summary>
    ''' オーダー情報取得処理(SHIPPINGADVICE出力用)
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <param name="tranCls"></param>
    ''' <returns></returns>
    Private Function CollectDisplayReportInfoSA(ByVal orderNo As String, ByVal tranCls As String, Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = New DataTable
        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("SELECT ISNULL(TD1.NAMES,'') AS AGENTPOD")
        sqlStat.AppendLine("      ,ISNULL(TD2.NAMES,'') AS AGENTPOL")
        sqlStat.AppendLine("      ,(SELECT REPLACE(TRIM((SELECT OV.TANKNO AS [data()]  FROM GBT0007_ODR_VALUE2 OV2 ")
        sqlStat.AppendLine("        LEFT JOIN GBT0005_ODR_VALUE OV ON OV.ORDERNO  = OV2.ORDERNO ")
        sqlStat.AppendLine("        Where TRILATERAL = @TRANCLS and OV2.ORDERNO = @ORDERNO ")
        sqlStat.AppendLine("        GROUP BY TANKNO FOR XML PATH(''))),' ',',')) AS TANKNO")
        sqlStat.AppendLine("      ,trim(PD.PRODUCTNAME) AS PRODUCTNAME")
        sqlStat.AppendLine("      ,CASE WHEN trim(PD.UNNO) = 'NON-DG' THEN 'NON' ELSE trim(PD.UNNO) END AS UNNO")
        sqlStat.AppendLine("      ,CASE WHEN trim(PD.HAZARDCLASS) = 'NON-DG' THEN 'NON' ELSE trim(PD.HAZARDCLASS) END AS IMDGCODE")

        sqlStat.AppendLine("      ,ISNULL(TD3.NAMES,'') AS USETYPE")

        sqlStat.AppendLine("      ,REPLACE(REPLACE(FV1.VALUE1,'-','/'),' ','') AS TERMTYPE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.VSL1 + ' ' + OB.VOY1 ELSE OB.VSL2 + ' ' + OB.VOY2 END AS VSLNAME")
        sqlStat.AppendLine("      ,ISNULL(PT1.AREANAME,'') + ' ' + FV1.VALUE3 AS PLACEOFRECIEPT")

        sqlStat.AppendLine("      ,ISNULL(PT2.AREANAME,'') + ', ' + ISNULL(CT2.NAMES,'') + CASE WHEN ETD.ACTDATE = '1900/01/01' THEN '' ")
        sqlStat.AppendLine("              ELSE ' on ' + FORMAT(ETD.ACTDATE,'dd') + ' ' + FV2.VALUE1 + ', ' + FORMAT(ETD.ACTDATE,'yyyy') END AS PORTOFLOADING ")

        sqlStat.AppendLine("      ,ISNULL(PT3.AREANAME,'') + ', ' + ISNULL(CT3.NAMES,'') + CASE WHEN ETA.ACTDATE = '1900/01/01' THEN '' ")
        sqlStat.AppendLine("              ELSE ' on ' + FORMAT(ETA.ACTDATE,'dd') + ' ' + FV3.VALUE1 + ', ' + FORMAT(ETA.ACTDATE,'yyyy') END AS PORTOFDISCHARGE ")


        'sqlStat.AppendLine("      ,ISNULL(PT3.AREANAME,'') + ', ' + ISNULL(CT3.NAMES,'') + CASE WHEN @TRANCLS = '1' THEN CASE WHEN OB.ETA1 = '1900/01/01' THEN '' ELSE ' on ' + FORMAT(OB.ETA1,'dd') + ' ' + FV3.VALUE1 + ', ' + FORMAT(OB.ETA1,'yyyy') END ELSE CASE WHEN OB.ETA2 = '1900/01/01' THEN '' ELSE ' on ' + FORMAT(OB.ETA2,'dd') + ' ' + FV3.VALUE1 + ', ' + FORMAT(OB.ETA2,'yyyy') END END AS PORTOFDISCHARGE")

        sqlStat.AppendLine("      ,ISNULL(PT4.AREANAME,'') + ', ' + ISNULL(CT4.NAMES,'') AS PLACEOFDELIVERY")
        sqlStat.AppendLine("      ,FV1.VALUE1 AS SHIPPINGTERM")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.BLID1 ELSE OB.BLID2 END AS BLID")
        sqlStat.AppendLine("      ,OB.BLTYPE AS BLTYPE")
        sqlStat.AppendLine("      ,OB.CARRIERBLNO AS CARRIERBLNO")
        sqlStat.AppendLine("      ,OB.CARRIERBLTYPE AS CARRIERBLTYPE")
        sqlStat.AppendLine("      ,OB.SHIPPERTEXT AS SHIPPERTEXT1")
        sqlStat.AppendLine("      ,'' AS SHIPPERTEXT2")
        sqlStat.AppendLine("      ,'' AS SHIPPERTEXT3")
        sqlStat.AppendLine("      ,'' AS SHIPPERTEXT4")
        sqlStat.AppendLine("      ,OB.CONSIGNEETEXT AS CONSIGNEETEXT1")
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT2")
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT3")
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT4")
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT5")
        sqlStat.AppendLine("      ,OB.NOTIFYTEXT AS NOTIFYTEXT1")
        sqlStat.AppendLine("      ,'' AS NOTIFYTEXT2")
        sqlStat.AppendLine("      ,'' AS NOTIFYTEXT3")
        sqlStat.AppendLine("      ,(SELECT CONVERT(NVARCHAR ,CONVERT(money, SUM(NETWEIGHT)), 1) ")
        sqlStat.AppendLine("      FROM GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("      WHERE ORDERNO    = @ORDERNO ")
        sqlStat.AppendLine("        AND TRILATERAL = @TRANCLS ")
        sqlStat.AppendLine("        AND DELFLG    <> @DELFLG ")
        sqlStat.AppendLine("      ) AS NETWEIGHT")
        sqlStat.AppendLine("      ,OB.PAYABLEAT AS PAYABLEAT")
        sqlStat.AppendLine("      ,OB.TIP AS TIP")
        sqlStat.AppendLine("      ,'1 TO ' + CONVERT(NVARCHAR ,OB.DEMURTO) + 'DAYS AT' AS DEMURTO1")
        sqlStat.AppendLine("      ,OB.DEMURUSRATE1 AS DEMURUSRATE1")
        sqlStat.AppendLine("      ,CONVERT(NVARCHAR ,OB.DEMURTO) + 'DAYS AT' AS DEMURTO2")
        sqlStat.AppendLine("      ,OB.DEMURUSRATE2 AS DEMURUSRATE2")
        sqlStat.AppendLine("      , 'EX.' + ISNULL(EX.CURRENCYCODE,'') AS LOCALCURRENCY")
        sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 EXSHIPRATE FROM (SELECT * FROM GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("        WHERE TRILATERAL = @TRANCLS AND ORDERNO = @ORDERNO AND DELFLG <> @DELFLG ) AS RATE),'0') AS EXCHANGERATE")
        sqlStat.AppendLine("      ,OB.DEMUFORACCT As DEMUACCT")

        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE OB ")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD ")
        sqlStat.AppendLine("    On PD.PRODUCTCODE  = OB.PRODUCTCODE")
        sqlStat.AppendLine("   And PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And PD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And PD.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT1 ")
        sqlStat.AppendLine("    On PT1.PORTCODE  = (Case When @TRANCLS = '1' THEN OB.RECIEPTPORT1 ELSE OB.RECIEPTPORT2 END)")
        sqlStat.AppendLine("   AND PT1.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT1.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT1.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1 ")
        sqlStat.AppendLine("    ON FV1.KEYCODE   = OB.TERMTYPE")
        sqlStat.AppendLine("   AND FV1.CLASS     = 'TERM'")
        sqlStat.AppendLine("   AND FV1.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT2 ")
        sqlStat.AppendLine("    ON PT2.PORTCODE  = (CASE WHEN @TRANCLS = '1' THEN OB.LOADPORT1 ELSE OB.LOADPORT2 END)")
        sqlStat.AppendLine("   AND PT2.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT2.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT2.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT2 ")
        sqlStat.AppendLine("    ON CT2.COUNTRYCODE  = PT2.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT2.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT2.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND CT2.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT3 ")
        sqlStat.AppendLine("    ON PT3.PORTCODE  = (CASE WHEN @TRANCLS = '1' THEN OB.DISCHARGEPORT1 ELSE OB.DISCHARGEPORT2 END)")
        sqlStat.AppendLine("   AND PT3.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT3.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT3.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT3 ")
        sqlStat.AppendLine("    ON CT3.COUNTRYCODE  = PT3.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT3.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT3.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND CT3.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT4 ")
        sqlStat.AppendLine("    ON PT4.PORTCODE  = (CASE WHEN @TRANCLS = '1' THEN OB.DELIVERYPORT1 ELSE OB.DELIVERYPORT2 END)")
        sqlStat.AppendLine("   AND PT4.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT4.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT4.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT4 ")
        sqlStat.AppendLine("    ON CT4.COUNTRYCODE  = PT4.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT4.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT4.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND CT4.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TD1 ")
        sqlStat.AppendLine("    ON TD1.CARRIERCODE = CASE WHEN @TRANCLS = '1' THEN OB.AGENTPOD1 ELSE OB.AGENTPOD2 END")
        sqlStat.AppendLine("   AND TD1.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND TD1.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND TD1.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TD2 ")
        sqlStat.AppendLine("    ON TD2.CARRIERCODE = CASE WHEN @TRANCLS = '1' THEN OB.AGENTPOL1 ELSE OB.AGENTPOL2 END")
        sqlStat.AppendLine("   AND TD2.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND TD2.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND TD2.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TD3 ")
        sqlStat.AppendLine("    ON TD3.CARRIERCODE = CASE WHEN @TRANCLS = '1' THEN OB.CARRIER1 ELSE OB.CARRIER2 END")
        sqlStat.AppendLine("   AND TD3.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND TD3.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND TD3.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE EX ")
        sqlStat.AppendLine("    ON EX.COUNTRYCODE  = (Case When @TRANCLS = '1' THEN OB.RECIEPTCOUNTRY1 ELSE OB.RECIEPTCOUNTRY2 END)")
        sqlStat.AppendLine("   AND EX.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND EX.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND EX.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN (Select TOP 1 ORDERNO, (Case When ACTUALDATE = '1900/01/01' THEN SCHEDELDATE ELSE ACTUALDATE END ) AS ACTDATE FROM GBT0005_ODR_VALUE WHERE ORDERNO = @ORDERNO AND DTLPOLPOD = (CASE WHEN @TRANCLS = '1' THEN 'POL1' ELSE 'POL2' END) AND ACTIONID in ('SHIP','RPEC','RPED','RPHC','RPHD') AND DELFLG <> @DELFLG) AS ETD")
        sqlStat.AppendLine("    ON ETD.ORDERNO = OB.ORDERNO")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2 ")
        sqlStat.AppendLine("    ON FV2.KEYCODE   = FORMAT(ETD.ACTDATE,'MM') ")
        sqlStat.AppendLine("   AND FV2.CLASS     = 'MONTH'")
        sqlStat.AppendLine("   AND FV2.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN (Select TOP 1 ORDERNO, (Case When ACTUALDATE = '1900/01/01' THEN SCHEDELDATE ELSE ACTUALDATE END ) AS ACTDATE FROM GBT0005_ODR_VALUE WHERE ORDERNO = @ORDERNO AND DTLPOLPOD = (CASE WHEN @TRANCLS = '1' THEN 'POD1' ELSE 'POD2' END) AND ACTIONID in ('ARVD','DCEC','DCED','ETYC') AND DELFLG <> @DELFLG) AS ETA")
        sqlStat.AppendLine("    ON ETA.ORDERNO = OB.ORDERNO")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV3 ")
        sqlStat.AppendLine("    ON FV3.KEYCODE   = FORMAT(ETA.ACTDATE,'MM') ")
        sqlStat.AppendLine("   AND FV3.CLASS     = 'MONTH'")
        sqlStat.AppendLine("   AND FV3.DELFLG   <> @DELFLG")


        sqlStat.AppendLine(" WHERE OB.ORDERNO  = @ORDERNO ")
        sqlStat.AppendLine("   AND OB.DELFLG  <> @DELFLG ")
        sqlStat.AppendLine("   And OB.STYMD    <= @STYMD")
        sqlStat.AppendLine("   And OB.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine(" ORDER BY OB.ORDERNO ")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                Dim paramOrderNo As SqlParameter = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 20)
                Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                Dim paramStYmd As SqlParameter = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
                Dim paramEndYmd As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
                Dim paramTranCls As SqlParameter = sqlCmd.Parameters.Add("@TRANCLS", SqlDbType.NVarChar, 1)
                'SQLパラメータ値セット
                paramOrderNo.Value = orderNo
                paramDelFlg.Value = CONST_FLAG_YES
                paramStYmd.Value = Date.Now
                paramEndYmd.Value = Date.Now
                paramTranCls.Value = tranCls
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Order base info Error")
                    End If
                    retDt = CreateOrderInfoTableSA()
                    For Each col As DataColumn In dt.Columns
                        retDt.Rows(0)(col.ColumnName) = Convert.ToString(dt.Rows(0)(col.ColumnName))
                    Next

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
    ''' オーダー情報を格納する空のデータテーブルを作成する
    ''' </summary>
    Private Function CreateOrderInfoTableSA() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = "ORDER_INFO"
        retDt.Columns.Add("AGENTPOD", GetType(String))
        retDt.Columns.Add("AGENTPOL", GetType(String))
        retDt.Columns.Add("TANKNO", GetType(String))
        retDt.Columns.Add("PRODUCTNAME", GetType(String))
        retDt.Columns.Add("UNNO", GetType(String))
        retDt.Columns.Add("IMDGCODE", GetType(String))
        retDt.Columns.Add("USETYPE", GetType(String))
        retDt.Columns.Add("TERMTYPE", GetType(String))
        retDt.Columns.Add("VSLNAME", GetType(String))
        retDt.Columns.Add("PLACEOFRECIEPT", GetType(String))
        retDt.Columns.Add("PORTOFLOADING", GetType(String))
        retDt.Columns.Add("PORTOFDISCHARGE", GetType(String))
        retDt.Columns.Add("PLACEOFDELIVERY", GetType(String))
        retDt.Columns.Add("SHIPPINGTERM", GetType(String))
        retDt.Columns.Add("BLID", GetType(String))
        retDt.Columns.Add("BLTYPE", GetType(String))
        retDt.Columns.Add("CARRIERBLNO", GetType(String))
        retDt.Columns.Add("CARRIERBLTYPE", GetType(String))
        retDt.Columns.Add("SHIPPERTEXT1", GetType(String))
        retDt.Columns.Add("SHIPPERTEXT2", GetType(String))
        retDt.Columns.Add("SHIPPERTEXT3", GetType(String))
        retDt.Columns.Add("SHIPPERTEXT4", GetType(String))
        retDt.Columns.Add("CONSIGNEETEXT1", GetType(String))
        retDt.Columns.Add("CONSIGNEETEXT2", GetType(String))
        retDt.Columns.Add("CONSIGNEETEXT3", GetType(String))
        retDt.Columns.Add("CONSIGNEETEXT4", GetType(String))
        retDt.Columns.Add("CONSIGNEETEXT5", GetType(String))
        retDt.Columns.Add("NOTIFYTEXT1", GetType(String))
        retDt.Columns.Add("NOTIFYTEXT2", GetType(String))
        retDt.Columns.Add("NOTIFYTEXT3", GetType(String))
        retDt.Columns.Add("NETWEIGHT", GetType(String))
        retDt.Columns.Add("PAYABLEAT", GetType(String))
        retDt.Columns.Add("TIP", GetType(String))
        retDt.Columns.Add("DEMURTO1", GetType(String))
        retDt.Columns.Add("DEMURUSRATE1", GetType(String))
        retDt.Columns.Add("DEMURTO2", GetType(String))
        retDt.Columns.Add("DEMURUSRATE2", GetType(String))
        retDt.Columns.Add("DEMUACCT", GetType(String))
        retDt.Columns.Add("LOCALCURRENCY", GetType(String))
        retDt.Columns.Add("EXCHANGERATE", GetType(String))

        '検討中
        retDt.Columns.Add("DUMMY", GetType(String))
        retDt.Columns.Add("DUMMY2", GetType(String))

        Dim dr As DataRow = retDt.NewRow
        dr.Item("DUMMY") = ""
        retDt.Rows.Add(dr)
        Return retDt
    End Function

    ''' <summary>
    ''' オーダー情報取得処理(経理連携出力用)
    ''' </summary>
    ''' <param name="reportMonth"></param>
    ''' <param name="countryCode"></param>
    ''' <returns></returns>
    Private Function CollectDisplayReportInfoAC(ByVal reportMonth As String, ByVal countryCode As String, ByVal department As String) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = New DataTable

        Try
            '************************************
            'SQL生成
            '************************************
            'ユーザーの言語に応じ日本語⇔英語フィールド設定
            Dim textTblField As String = "NAMESJP"
            If COA0019Session.LANGDISP <> C_LANG.JA Then
                textTblField = "NAMES"
            End If

            Dim sqlStat As New StringBuilder()

            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("  SUM(DEBAMOUNT) as DEBAMOUNT ")
            sqlStat.AppendLine(" ,SUM(DEBCONSTAXAMOUNT) as DEBCONSTAXAMOUNT")
            sqlStat.AppendLine(" ,ROUND(SUM(DEBFORCURAMOUNT), 6) as DEBFORCURAMOUNT")
            sqlStat.AppendLine(" ,SUM(CREAMOUNT) as CREAMOUNT")
            sqlStat.AppendLine(" ,SUM(CRECONSTAXAMOUNT) as CRECONSTAXAMOUNT")
            sqlStat.AppendLine(" ,ROUND(SUM(CREFORCURAMOUNT), 6) as CREFORCURAMOUNT")
            sqlStat.AppendLine(" ,MAX(DATACRITERIA) as DATACRITERIA")
            sqlStat.AppendLine(" ,MAX(JOURNALENTRY) as JOURNALENTRY")
            sqlStat.AppendLine(" ,MAX(INPUTSCREENNO) as INPUTSCREENNO")
            sqlStat.AppendLine(" ,MAX(DOCUMENTDATE) as DOCUMENTDATE")
            sqlStat.AppendLine(" ,MAX(SETTLEMONTHCLS) as SETTLEMONTHCLS")
            sqlStat.AppendLine(" ,MAX(PROOFNO) as PROOFNO")
            sqlStat.AppendLine(" ,MAX(DEBBANK) as DEBBANK")
            sqlStat.AppendLine(" ,MAX(DEBPARTNER) as DEBPARTNER")
            'sqlStat.AppendLine(" ,MAX(DEBGENPURPOSE) as DEBGENPURPOSE")
            sqlStat.AppendLine(" ,MAX(DEBSEGMENT3) as DEBSEGMENT3")
            sqlStat.AppendLine(" ,MAX(DEBNO1) as DEBNO1")
            sqlStat.AppendLine(" ,MAX(DEBNO2) as DEBNO2")
            sqlStat.AppendLine(" ,MAX(DEBCONTAXCLS) as DEBCONTAXCLS")
            sqlStat.AppendLine(" ,MAX(DEBCONTAXCODE) as DEBCONTAXCODE")
            sqlStat.AppendLine(" ,MAX(DEBCONTAXRTCLS) as DEBCONTAXRTCLS")
            sqlStat.AppendLine(" ,MAX(DEBSIMINPCLS) as DEBSIMINPCLS")
            sqlStat.AppendLine(" ,ROUND(MAX(DEBFORCURRATE), 6) as DEBFORCURRATE")
            sqlStat.AppendLine(" ,MAX(DEBFORCURTRDCLS) as DEBFORCURTRDCLS")
            sqlStat.AppendLine(" ,MAX(CREBANK) as CREBANK")
            sqlStat.AppendLine(" ,MAX(CREPARTNER) as CREPARTNER")
            'sqlStat.AppendLine(" ,MAX(CREGENPURPOSE) as CREGENPURPOSE")
            sqlStat.AppendLine(" ,MAX(CRESEGMENT3) as CRESEGMENT3")
            sqlStat.AppendLine(" ,MAX(CRENO1) as CRENO1")
            sqlStat.AppendLine(" ,MAX(CRENO2) as CRENO2")
            sqlStat.AppendLine(" ,MAX(CRECONTAXCLS) as CRECONTAXCLS")
            sqlStat.AppendLine(" ,MAX(CRECONTAXCODE) as CRECONTAXCODE")
            sqlStat.AppendLine(" ,MAX(CRECONTAXRTCLS) as CRECONTAXRTCLS")
            sqlStat.AppendLine(" ,MAX(CRESIMINPCLS) as CRESIMINPCLS")
            sqlStat.AppendLine(" ,ROUND(MAX(CREFORCURRATE), 6) as CREFORCURRATE")
            sqlStat.AppendLine(" ,MAX(CREFORCURTRDCLS) as CREFORCURTRDCLS")
            sqlStat.AppendLine(" ,MAX(SUMMARY) as SUMMARY")
            sqlStat.AppendLine(" ,MAX(SUMMARYCODE) as SUMMARYCODE")
            sqlStat.AppendLine(" ,MAX(CREATEDDATE) as CREATEDDATE")
            sqlStat.AppendLine(" ,MAX(CREATEDTIME) as CREATEDTIME")
            sqlStat.AppendLine(" ,MAX(AUTHOR) as AUTHOR")
            sqlStat.AppendLine(" ,MAX(REPORTMONTH) as REPORTMONTH")
            sqlStat.AppendLine(" ,MAX(COUNTRYCODE) as COUNTRYCODE")

            sqlStat.AppendLine(" ,MAX(PAYDAY) as PAYDAY")
            sqlStat.AppendLine(" ,MAX(HOLIDAYFLG) as HOLIDAYFLG")

            sqlStat.AppendLine(" ,MAX(DBGENERALPURPOSE) as DBGENERALPURPOSE")
            sqlStat.AppendLine(" ,MAX(CRGENERALPURPOSE) as CRGENERALPURPOSE")

            sqlStat.AppendLine(" ,BOTHCLASS as BOTHCLASS")
            sqlStat.AppendLine(" ,TORICODE as TORICODE")
            sqlStat.AppendLine(" ,REVENUE as REVENUE")
            sqlStat.AppendLine(" ,DEBSUBJECT as DEBSUBJECT")
            sqlStat.AppendLine(" ,DEBSECTION as DEBSECTION")
            sqlStat.AppendLine(" ,CRESUBJECT as CRESUBJECT")
            sqlStat.AppendLine(" ,CRESECTION as CRESECTION")
            sqlStat.AppendLine(" ,TAXATION as TAXATION")

            sqlStat.AppendLine(" ,DEBSEGMENT1 as DEBSEGMENT1")
            sqlStat.AppendLine(" ,DEBSEGMENT2 as DEBSEGMENT2")

            sqlStat.AppendLine(" ,CRESEGMENT1 as CRESEGMENT1")
            sqlStat.AppendLine(" ,CRESEGMENT2 as CRESEGMENT2")


            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       TBLALL.* ")

            '固定値
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='DATACRITERIA' AND KEYCODE='0' AND DELFLG <> @DELFLG ),'') AS DATACRITERIA ")         'データ基準
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='INPUTSCREENNO' AND KEYCODE='11' AND DELFLG <> @DELFLG ),'') AS INPUTSCREENNO ")      '入力画面番号
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SETTLEMONTHCLS' AND KEYCODE='0' AND DELFLG <> @DELFLG ),'') AS SETTLEMONTHCLS ")     '決算月区分
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SEGMENT3' AND KEYCODE='30' AND DELFLG <> @DELFLG ),'') AS DEBSEGMENT3 ")             '借方セグメント3
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SIMULTANEOUS' AND KEYCODE='0' AND DELFLG <> @DELFLG ),'') AS DEBSIMINPCLS ")         '借方外税同時入力区分
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='FOREIGNTRANS' AND KEYCODE='0' AND DELFLG <> @DELFLG ),'') AS DEBFORCURTRDCLS ")      '借方外貨取引区分
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SEGMENT3' AND KEYCODE='30' AND DELFLG <> @DELFLG ),'') AS CRESEGMENT3 ")             '貸方セグメント3
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SIMULTANEOUS' AND KEYCODE='0' AND DELFLG <> @DELFLG ),'') AS CRESIMINPCLS ")         '貸方外税同時入力区分
            sqlStat.AppendLine("      ,ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='FOREIGNTRANS' AND KEYCODE='0' AND DELFLG <> @DELFLG ),'') AS CREFORCURTRDCLS ")      '貸方外貨取引区分

            sqlStat.AppendLine("      ,''  AS DEBNO1 ")
            sqlStat.AppendLine("      ,''  AS DEBNO2 ")
            sqlStat.AppendLine("      ,''  AS CRENO1 ")
            sqlStat.AppendLine("      ,''  AS CRENO2 ")

            sqlStat.AppendLine("      ,''  AS SUMMARY ")    '摘要 TODO:何を設定するか未定の為、空白

            sqlStat.AppendLine("      ,CASE WHEN TBLALL.TAXATION = '" & GBC_TAXATION.FREE & "' THEN '0' ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='SALESTAX' AND KEYCODE=TBLALL.DEBTAXRATE AND DELFLG <> 'Y' ),'') END AS DEBCONTAXRTCLS ") '借方消費税率区分
            sqlStat.AppendLine("      ,TBLALL.LOCALAMOUNT AS DEBAMOUNT ") '借方金額
            sqlStat.AppendLine("      ,CASE WHEN TBLALL.TAXATION = '" & GBC_TAXATION.FREE & "' THEN '0' ELSE CASE WHEN TBLALL.ACCCURRENCYSEGMENT = 'Y' THEN ROUND((CONVERT(FLOAT,ISNULL(TBLALL.DEBTAXRATE,'0')) / 100) * CONVERT(float,TBLALL.LOCALAMOUNT),0) ELSE ROUND(((CONVERT(FLOAT,ISNULL(TBLALL.DEBTAXRATE,'0')) / 100) * CONVERT(float,TBLALL.LOCALAMOUNT)) / TBLALL.EXRATE ,0) END END AS DEBCONSTAXAMOUNT ") '借方消費税額
            sqlStat.AppendLine("      ,CASE WHEN TBLALL.ACCCURRENCYSEGMENT = 'Y' THEN '0' ELSE TBLALL.USDAMOUNT END AS DEBFORCURAMOUNT ") '借方外貨金額
            sqlStat.AppendLine("      ,CASE WHEN TBLALL.ACCCURRENCYSEGMENT = 'Y' THEN '0' ELSE TBLALL.EXRATE END AS DEBFORCURRATE ") '借方外貨レート

            sqlStat.AppendLine("      ,CASE WHEN TBLALL.TAXATION = '" & GBC_TAXATION.FREE & "' THEN '0' ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='SALESTAX' AND KEYCODE=TBLALL.CRETAXRATE AND DELFLG <> 'Y' ),'') END AS CRECONTAXRTCLS ") '貸方消費税率区分
            sqlStat.AppendLine("      ,TBLALL.LOCALAMOUNT AS CREAMOUNT ") '貸方金額
            sqlStat.AppendLine("      ,CASE WHEN TBLALL.TAXATION = '" & GBC_TAXATION.FREE & "' THEN '0' ELSE CASE WHEN TBLALL.ACCCURRENCYSEGMENT = 'Y' THEN ROUND((CONVERT(FLOAT,ISNULL(TBLALL.CRETAXRATE,'0')) / 100) * CONVERT(float,TBLALL.LOCALAMOUNT),0) ELSE ROUND(((CONVERT(FLOAT,ISNULL(TBLALL.CRETAXRATE,'0')) / 100) * CONVERT(float,TBLALL.LOCALAMOUNT)) / TBLALL.EXRATE ,0) END END AS CRECONSTAXAMOUNT ") '貸方消費税額
            sqlStat.AppendLine("      ,CASE WHEN TBLALL.ACCCURRENCYSEGMENT = 'Y' THEN '0' ELSE TBLALL.USDAMOUNT END AS CREFORCURAMOUNT ") '貸方外貨金額
            sqlStat.AppendLine("      ,CASE WHEN TBLALL.ACCCURRENCYSEGMENT = 'Y' THEN '0' ELSE TBLALL.EXRATE END AS CREFORCURRATE ") '貸方外貨レート

            sqlStat.AppendLine("      ,FORMAT(@DATE,'yyyyMMdd')  AS CREATEDDATE ")
            sqlStat.AppendLine("      ,FORMAT(@DATE,'HHmmss')  AS CREATEDTIME ")
            sqlStat.AppendLine("      ,@USER  AS AUTHOR ")

            sqlStat.AppendLine("FROM (")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("       TBL.* ")

            sqlStat.AppendLine("      ,CASE WHEN TBL.ACCCURRENCYSEGMENT = 'Y' THEN ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='JOURNALFORM' AND KEYCODE='Y' AND DELFLG <> 'Y' ),'') ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='JOURNALFORM' AND KEYCODE='F' AND DELFLG <> 'Y' ),'') END AS JOURNALENTRY")
            sqlStat.AppendLine("      ,CASE WHEN TBL.ACCCURRENCYSEGMENT = 'Y' THEN CASE WHEN TBL.CHARGE_CLASS1 = '" & GBC_CHARGECLASS1.REVENUE & "' THEN ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='PROOFNUMBER' AND KEYCODE='A9' AND DELFLG <> 'Y' ),'') ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='PROOFNUMBER' AND KEYCODE='C9' AND DELFLG <> 'Y' ),'') END ")
            sqlStat.AppendLine("            ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='PROOFNUMBER' AND KEYCODE='G9' AND DELFLG <> 'Y' ),'') END AS PROOFNO ")
            sqlStat.AppendLine("      ,CASE WHEN TBL.ACCCURRENCYSEGMENT = 'Y' THEN TBL.DBACCOUNT ELSE TBL.DBACCOUNTFORIGN END AS DEBSUBJECT ") '借方科目
            sqlStat.AppendLine("      ,CASE WHEN TRIM(TBL.PROPERTY) = '" & GBC_PROPERTY.DOMESTIC & "' AND TBL.COSTCODE <> '" & GBC_COSTCODE_LEASE & "' THEN ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='DEPARTMENT' AND KEYCODE='CHEMICAL' AND DELFLG <> 'Y' ),'') ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='DEPARTMENT' AND KEYCODE='GLOBAL' AND DELFLG <> 'Y' ),'') END AS DEBSECTION ") '借方部門
            sqlStat.AppendLine("      ,ISNULL(TBL.BANKCODE,'') AS DEBBANK ") '借方銀行1
            sqlStat.AppendLine("      ,CASE WHEN TBL.CHARGE_CLASS1 = '" & GBC_CHARGECLASS1.REVENUE & "' THEN  CASE WHEN ISNULL(TBL.TORICODE,'') = '' THEN '' ELSE TBL.TORICODE + '0' END  ELSE CASE WHEN ISNULL(TBL.TORICODE,'') = '' THEN '' ELSE TBL.TORICODE + '3' END END AS DEBPARTNER") '借方取引先
            sqlStat.AppendLine("      ,ISNULL(TBL.DBSEGMENT1,'') AS DEBSEGMENT1 ") '借方セグメント1
            sqlStat.AppendLine("      ,CASE WHEN TBL.CHARGE_CLASS1 = '" & GBC_CHARGECLASS1.REVENUE & "' THEN  ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='TAXCLASS' AND KEYCODE='1' AND DELFLG <> 'Y' ),'') ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='TAXCLASS' AND KEYCODE='2' AND DELFLG <> 'Y' ),'') END AS DEBCONTAXCLS") '借方消費税区分
            sqlStat.AppendLine("      ,CASE WHEN TBL.TAXATION = '" & GBC_TAXATION.FREE & "' THEN  ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='TAXATION' AND KEYCODE='0' AND DELFLG <> 'Y' ),'') ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='TAXATION' AND KEYCODE='1' AND DELFLG <> 'Y' ),'') END AS DEBCONTAXCODE") '借方消費税コード

            'sqlStat.AppendLine("      ,CASE WHEN TBL.ACCCURRENCYSEGMENT = 'Y' THEN TBL.CRACCOUNT ELSE TBL.CRACCOUNTFORIGN END AS CRESUBJECT ") '貸方科目
            sqlStat.AppendLine("      ,CASE WHEN TBL.ACCCURRENCYSEGMENT = 'Y' THEN TBL.OFFCRACCOUNT ELSE TBL.OFFCRACCOUNTFORIGN END AS CRESUBJECT ") '貸方科目
            sqlStat.AppendLine("      ,CASE WHEN TRIM(TBL.PROPERTY) = '" & GBC_PROPERTY.DOMESTIC & "' AND TBL.COSTCODE <> '" & GBC_COSTCODE_LEASE & "' THEN ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='DEPARTMENT' AND KEYCODE='CHEMICAL' AND DELFLG <> 'Y' ),'') ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='DEPARTMENT' AND KEYCODE='GLOBAL' AND DELFLG <> 'Y' ),'') END AS CRESECTION ") '貸方部門
            sqlStat.AppendLine("      ,ISNULL(TBL.BANKCODE,'') AS CREBANK ") '貸方銀行1
            sqlStat.AppendLine("      ,CASE WHEN TBL.CHARGE_CLASS1 = '" & GBC_CHARGECLASS1.REVENUE & "' THEN  CASE WHEN ISNULL(TBL.TORICODE,'') = '' THEN '' ELSE TBL.TORICODE + '0' END  ELSE CASE WHEN ISNULL(TBL.TORICODE,'') = '' THEN '' ELSE TBL.TORICODE + '3' END END AS CREPARTNER") '貸方取引先
            sqlStat.AppendLine("      ,ISNULL(TBL.CRSEGMENT1,'') AS CRESEGMENT1 ") '貸方セグメント1
            sqlStat.AppendLine("      ,CASE WHEN TBL.CHARGE_CLASS1 = '" & GBC_CHARGECLASS1.REVENUE & "' THEN  ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='TAXCLASS' AND KEYCODE='1' AND DELFLG <> 'Y' ),'') ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='TAXCLASS' AND KEYCODE='2' AND DELFLG <> 'Y' ),'') END AS CRECONTAXCLS") '貸方消費税区分
            sqlStat.AppendLine("      ,CASE WHEN TBL.TAXATION = '" & GBC_TAXATION.FREE & "' THEN  ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='TAXATION' AND KEYCODE='0' AND DELFLG <> 'Y' ),'') ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='TAXATION' AND KEYCODE='1' AND DELFLG <> 'Y' ),'') END AS CRECONTAXCODE") '貸方消費税コード

            sqlStat.AppendLine("      ,CASE WHEN TBL.CHARGE_CLASS1 = '" & GBC_CHARGECLASS1.REVENUE & "' THEN ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='SUMMARY' AND KEYCODE='REVENUE' AND DELFLG <> 'Y' ),'') ")
            sqlStat.AppendLine("            ELSE ISNULL((SELECT TOP 1 VALUE1 FROM COS0017_FIXVALUE WHERE COMPCODE='Default' AND SYSCODE='GB' AND CLASS='SUMMARY' AND KEYCODE='COST' AND DELFLG <> 'Y' ),'') END AS SUMMARYCODE ")

            sqlStat.AppendLine("      ,CASE WHEN TBL.REPORTYMD_BASE < CLOSINGMONTH THEN CLOSINGMONTH ELSE TBL.REPORTYMD_BASE END AS REPORTYMD")

            sqlStat.AppendLine("      ,TBL.REPORTYMD_BASE AS REPORTYMDORG")

            sqlStat.AppendLine("      ,CASE WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN CEILING(TBL.USDAMOUNT_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN FLOOR(  TBL.USDAMOUNT_BOFORE_ROUND * POWER(10,TBL.USDDECIMALPLACES)) / POWER(10,TBL.USDDECIMALPLACES) ")
            sqlStat.AppendLine("            WHEN TBL.USDROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.USDAMOUNT_BOFORE_ROUND,TBL.USDDECIMALPLACES * 1) ")
            sqlStat.AppendLine("            ELSE TBL.USDAMOUNT_BOFORE_ROUND END AS USDAMOUNT ")

            'sqlStat.AppendLine("      ,CASE WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN CEILING(TBL.LOCALAMOUNT_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            'sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN FLOOR(  TBL.LOCALAMOUNT_BOFORE_ROUND * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
            'sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.LOCALAMOUNT_BOFORE_ROUND,TBL.DECIMALPLACES * 1) ")
            'sqlStat.AppendLine("            ELSE TBL.LOCALAMOUNT_BOFORE_ROUND END AS LOCALAMOUNT ")
            sqlStat.AppendLine("      ,ROUND(TBL.LOCALAMOUNT_BOFORE_ROUND,0) AS LOCALAMOUNT ")

            sqlStat.AppendLine("      ,CASE WHEN TBL.REPORTMONTHH = '' THEN '' ELSE  Format(DateAdd(Day, -1, DateAdd(Month, 1, DateAdd(Day, 1 - DatePart(Day,TBL.REPORTMONTHH),TBL.REPORTMONTHH))),'yyyy/MM/dd') END AS DOCUMENTDATE")

            sqlStat.AppendLine("      ,CASE WHEN TBL.CHARGE_CLASS1 = '" & GBC_CHARGECLASS1.REVENUE & "' THEN '1' ELSE '2' END AS REVENUE ") '収入費用区分

            sqlStat.AppendLine("FROM (")

            sqlStat.AppendLine("SELECT TBLSUB.*")
            sqlStat.AppendLine("      ,ISNULL(USREXR.EXRATE,'') AS EXRATE")

            sqlStat.AppendLine("      ,CASE WHEN USREXR.EXRATE IS NULL OR USREXR.EXRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX") 'ドル換算の場合はそのまま
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX / USREXR.EXRATE") 'ローカル換算の場合はドル
            sqlStat.AppendLine("        END AS USDAMOUNT_BOFORE_ROUND")

            sqlStat.AppendLine("       ,CASE WHEN USREXR.EXRATE IS NULL OR USREXR.EXRATE = 0 THEN '' ")
            sqlStat.AppendLine("            WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.AMOUNTFIX * USREXR.EXRATE") 'ドル換算の場合はローカル
            sqlStat.AppendLine("            ELSE TBLSUB.AMOUNTFIX") 'ローカル換算の場合はそのまま
            sqlStat.AppendLine("        END AS LOCALAMOUNT_BOFORE_ROUND")

            sqlStat.AppendLine("      ,CNTY.DECIMALPLACES AS DECIMALPLACES")
            sqlStat.AppendLine("      ,CNTY.ROUNDFLG      AS ROUNDFLG")
            sqlStat.AppendLine("      ,CNTY.TAXRATE       AS TAXRATE")
            sqlStat.AppendLine("      ,USDDECIMAL.VALUE1      AS USDDECIMALPLACES")
            sqlStat.AppendLine("      ,USDDECIMAL.VALUE2      AS USDROUNDFLG")
            sqlStat.AppendLine("      ,CASE CLD.BILLINGYMD WHEN '1900/01/01' THEN '' ELSE FORMAT(CLD.BILLINGYMD,'yyyy/MM/dd') END AS BILLINGYMD")
            sqlStat.AppendLine("      ,FORMAT(DATEADD(month,1,BILLINGYMD),'yyyy/MM') AS CLOSINGMONTH")
            sqlStat.AppendLine("      ,CASE WHEN  TBLSUB.SOAAPPDATE = '' OR TBLSUB.SOAAPPDATE >= (CASE CLD.BILLINGYMD WHEN '1900/01/01' THEN '' ELSE FORMAT(CLD.BILLINGYMD,'yyyy/MM/dd') END) THEN '' ELSE '1' END AS ISBILLINGCLOSED")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.BRTYPE IN ('" & C_BRTYPE.REPAIR & "','" & C_BRTYPE.NONBR & "')  THEN CASE WHEN TBLSUB.ACTUALDATEDTM = '1900/01/01' OR TBLSUB.ACTUALDATEDTM IS NULL THEN '-' WHEN DAY(TBLSUB.ACTUALDATEDTM)>=26 THEN FORMAT(DATEADD(month,1,TBLSUB.ACTUALDATEDTM),'yyyy/MM') ELSE FORMAT(TBLSUB.ACTUALDATEDTM,'yyyy/MM') END ")
            sqlStat.AppendLine("            WHEN TBLSUB.DTLPOLPOD  IN ('POL1','Organizer') THEN CASE WHEN TBLSUB.RECOEDDATE    = '1900/01/01' OR TBLSUB.RECOEDDATE IS NULL THEN '-' WHEN DAY(TBLSUB.RECOEDDATE)>=26 THEN FORMAT(DATEADD(month,1,TBLSUB.RECOEDDATE),'yyyy/MM') ELSE FORMAT(TBLSUB.RECOEDDATE,'yyyy/MM') END ")
            sqlStat.AppendLine("            ELSE CASE WHEN TBLSUB.ACTUALDATEDTM = '1900/01/01' OR TBLSUB.ACTUALDATEDTM IS NULL THEN '-' WHEN DAY(TBLSUB.ACTUALDATEDTM)>=26 THEN FORMAT(DATEADD(month,1,TBLSUB.ACTUALDATEDTM),'yyyy/MM') ELSE FORMAT(TBLSUB.ACTUALDATEDTM,'yyyy/MM') END END AS REPORTYMD_BASE ")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.INVOICEDBY = @JOTAGENT THEN 'on' ELSE '' END AS JOT")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.ACTUALDATE <> '' AND TBLSUB.ACTUALDATE <= (SELECT TOP 1 FORMAT(CASE WHEN DAY(GETDATE())>=26 THEN DATEADD(month,(VALUE1 * -1) + 1,GETDATE()) ELSE DATEADD(month,VALUE1 * -1,GETDATE()) END,'yyyy/MM') + '/25' FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SOALOWERLIMITMONTH' AND KEYCODE='-' AND DELFLG <> @DELFLG) THEN '1' ELSE '0' END AS ISAUTOCLOSE")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.ACTUALDATE <> '' AND TBLSUB.ACTUALDATE <= (SELECT TOP 1 FORMAT(CASE WHEN DAY(GETDATE())>=26 THEN DATEADD(month,(VALUE2 * -1) + 1,GETDATE()) ELSE DATEADD(month,VALUE2 * -1,GETDATE()) END,'yyyy/MM') + '/25' FROM COS0017_FIXVALUE WHERE COMPCODE='" & GBC_COMPCODE_D & "' AND SYSCODE='" & C_SYSCODE_GB & "' AND CLASS='SOALOWERLIMITMONTH' AND KEYCODE='-' AND DELFLG <> @DELFLG) THEN '1' ELSE '0' END AS ISAUTOCLOSELONG")
            sqlStat.AppendLine("      ,CASE WHEN TBLSUB.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN TBLSUB.CURRENCYCODE + '(' + ISNULL(CNTY.CURRENCYCODE,'') + ')' ELSE TBLSUB.CURRENCYCODE END AS DISPLAYCURRANCYCODE ")
            sqlStat.AppendLine(" FROM(")

            sqlStat.AppendLine("SELECT '' AS OPERATION")
            sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("     , '1' AS 'SELECT' ")
            sqlStat.AppendLine("     , '0' AS HIDDEN ")
            sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
            sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
            sqlStat.AppendLine("     , OBS.BRTYPE    AS BRTYPR")
            sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
            sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
            sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
            sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
            sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
            sqlStat.AppendFormat("     , ISNULL(CST.{0},'') AS COSTNAME", textTblField).AppendLine()
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTIONID")
            sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
            sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
            sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")

            sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
            sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
            sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
            sqlStat.AppendLine("     , VL.AMOUNTBR      AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.AMOUNTORD     AS AMOUNTORD")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE  WHEN '1900/01/01' THEN VL.AMOUNTORD ELSE VL.AMOUNTFIX END AS AMOUNTFIX")
            sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
            sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
            sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")

            sqlStat.AppendLine("     , VL.REPORTMONTH AS REPORTMONTH")
            sqlStat.AppendLine("     , CASE WHEN VL.REPORTMONTH = '' THEN '' ELSE VL.REPORTMONTH + '/01' END AS REPORTMONTHH")

            sqlStat.AppendLine("     , VL.REPORTMONTHORG AS REPORTMONTHORG")

            '業者名
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS4 = '{0}' THEN DPBR.NAMES ELSE TRBR.NAMES END AS CONTRACTORNAMEBR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS4 = '{0}' THEN DPODR.NAMES ELSE TRODR.NAMES END AS CONTRACTORNAMEODR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS4 = '{0}' THEN DPFIX.NAMES ELSE TRFIX.NAMES END AS CONTRACTORNAMEFIX ", GBC_CHARGECLASS4.DEPOT).AppendLine()

            sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
            sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
            sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
            sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
            sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
            sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
            sqlStat.AppendLine("     , VL.AMOUNTPAY      AS AMOUNTPAY")
            sqlStat.AppendLine("     , VL.LOCALPAY       AS LOCALPAY")

            sqlStat.AppendLine("     , VL.UAG_USD        AS UAG_USD")
            sqlStat.AppendLine("     , VL.UAG_LOCAL      AS UAG_LOCAL")
            sqlStat.AppendLine("     , VL.USD_USD        AS USD_USD")
            sqlStat.AppendLine("     , VL.USD_LOCAL      AS USD_LOCAL")
            sqlStat.AppendLine("     , VL.LOCAL_USD      AS LOCAL_USD")
            sqlStat.AppendLine("     , VL.LOCAL_LOCAL    AS LOCAL_LOCAL")

            sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
            sqlStat.AppendLine("     , VL.BRID           AS BRID")
            sqlStat.AppendLine("     , '1'               AS BRCOST") 'SOAの場合は削除させない
            sqlStat.AppendLine("     , ''                AS ACTYNO")
            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")
            sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'Organizer' THEN '00000' ELSE RIGHT(VL.DTLPOLPOD,1) + REPLACE(REPLACE(VL.DTLPOLPOD,'POL','000'),'POD','001') END AS AGENTKBNSORT")
            sqlStat.AppendLine("     , CASE WHEN ISNULL(VL.DISPSEQ,'') = '' THEN '1' ")
            sqlStat.AppendLine("            ELSE '0' END AS DISPSEQISEMPTY")
            sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.AGENTPOL1")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.AGENTPOL2")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.AGENTPOD1")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.AGENTPOD2")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN OBS.AGENTORGANIZER")
            sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN VL.AGENTORGANIZER")
            sqlStat.AppendLine("            ELSE '' END AS AGENT")

            sqlStat.AppendLine("     , ISNULL(CST.CLASS1, '') AS CHARGE_CLASS1")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'') AS CHARGE_CLASS4")
            sqlStat.AppendLine("     , VL.SOACODE AS SOACODE")
            sqlStat.AppendLine("     , CASE SHIPREC.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE SHIPREC.ACTUALDATE END AS RECOEDDATE")
            sqlStat.AppendLine("     , OBS.BRTYPE AS BRTYPE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS ACTUALDATEDTM")

            sqlStat.AppendLine("      , JC.DATA             AS DATA ")
            sqlStat.AppendLine("      , JC.JOTCODE          AS JOTCODE ")
            sqlStat.AppendLine("      , JC.ACCODE           AS ACCODE ")
            sqlStat.AppendLine("      , VL.LOCALRATESOA     AS LOCALRATESOA ")
            sqlStat.AppendLine("      , VL.AMOUNTPAYODR     AS AMOUNTPAYODR ")
            sqlStat.AppendLine("      , VL.LOCALPAYODR      AS LOCALPAYODR ")
            sqlStat.AppendLine("      , CASE WHEN ISNULL(VL.TAXATION,'') <> '' AND ISNULL(CS.TORICODE,'') <> '' THEN ISNULL(CS.ACCCURRENCYSEGMENT,'') ELSE ISNULL(VL.ACCCURRENCYSEGMENT,'') END AS ACCCURRENCYSEGMENT ")
            sqlStat.AppendLine("      , ISNULL(TR.PAYDAY,'') AS PAYDAY ")
            sqlStat.AppendLine("      , ISNULL(TR.HOLIDAYFLG,'') AS HOLIDAYFLG ")
            sqlStat.AppendLine("      , ISNULL(CST.CRACCOUNT,'') AS CRACCOUNT ")
            sqlStat.AppendLine("      , ISNULL(CST.DBACCOUNT,'') AS DBACCOUNT ")
            sqlStat.AppendLine("      , ISNULL(CST.CRACCOUNTFORIGN,'') AS CRACCOUNTFORIGN ")
            sqlStat.AppendLine("      , ISNULL(CST.DBACCOUNTFORIGN,'') AS DBACCOUNTFORIGN ")
            sqlStat.AppendLine("      , ISNULL(CST.OFFCRACCOUNT,'') AS OFFCRACCOUNT ")
            sqlStat.AppendLine("      , ISNULL(CST.OFFDBACCOUNT,'') AS OFFDBACCOUNT ")
            sqlStat.AppendLine("      , ISNULL(CST.OFFCRACCOUNTFORIGN,'') AS OFFCRACCOUNTFORIGN ")
            sqlStat.AppendLine("      , ISNULL(CST.OFFDBACCOUNTFORIGN,'') AS OFFDBACCOUNTFORIGN ")
            sqlStat.AppendLine("      , ISNULL(CST.CRGENERALPURPOSE,'') AS CRGENERALPURPOSE ")
            sqlStat.AppendLine("      , ISNULL(CST.DBGENERALPURPOSE,'') AS DBGENERALPURPOSE ")
            sqlStat.AppendLine("      , ISNULL(CST.CRSEGMENT1,'') AS CRSEGMENT1 ")
            sqlStat.AppendLine("      , ISNULL(CST.DBSEGMENT1,'') AS DBSEGMENT1 ")
            sqlStat.AppendLine("      , ISNULL(TK.PROPERTY,'') AS PROPERTY ")
            sqlStat.AppendLine("      , CASE WHEN ISNULL(VL.TAXATION,'') <> '' AND ISNULL(CS.TORICODE,'') <> '' THEN ISNULL(CS.TORICODE,'') ELSE ISNULL(VL.TORICODE,'') END AS TORICODE ")
            sqlStat.AppendLine("      , ISNULL(TORI.BANKCODE,'') AS BANKCODE ")
            sqlStat.AppendLine("      , ISNULL(CT.DEBITSEGMENT,'') AS CRESEGMENT2 ")
            sqlStat.AppendLine("      , ISNULL(CT.DEBITSEGMENT,'') AS DEBSEGMENT2 ")
            sqlStat.AppendLine("      , CASE WHEN ISNULL(VL.TAXATION,'') <> '' AND ISNULL(CS.TORICODE,'') <> '' THEN ISNULL(CS.BOTHCLASS,'') ELSE ISNULL(VL.BOTHCLASS,'') END AS BOTHCLASS ")
            sqlStat.AppendLine("      , ISNULL(TR.COUNTRYCODE,'') AS TRCOUNTRYCODE ")
            sqlStat.AppendLine("      , ISNULL(CT.TAXRATE,'') AS CRETAXRATE ")
            sqlStat.AppendLine("      , ISNULL(CT.TAXRATE,'') AS DEBTAXRATE ")


            'sqlStat.AppendLine("  FROM GBT0008_JOTSOA_VALUE VL")
            sqlStat.AppendLine("  FROM TEST_GBT0008_JOTSOA_VALUE VL")

            sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OBS")
            sqlStat.AppendLine("    ON OBS.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("   AND OBS.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN (")
            sqlStat.AppendLine("             SELECT SHIPRECSUB.ORDERNO")
            sqlStat.AppendLine("                  , SHIPRECSUB.TANKSEQ")
            sqlStat.AppendLine("                  , MAX(SHIPRECSUB.ACTUALDATE) AS ACTUALDATE")
            'sqlStat.AppendLine("               FROM GBT0008_JOTSOA_VALUE SHIPRECSUB")
            sqlStat.AppendLine("               FROM TEST_GBT0008_JOTSOA_VALUE SHIPRECSUB")
            sqlStat.AppendLine("              WHERE SHIPRECSUB.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("                AND SHIPRECSUB.ACTIONID  IN ('SHIP','RPEC','RPED','RPHC','RPHD')")
            sqlStat.AppendLine("                AND SHIPRECSUB.DTLPOLPOD = 'POL1'")
            sqlStat.AppendLine("             GROUP BY SHIPRECSUB.ORDERNO,SHIPRECSUB.TANKSEQ")
            sqlStat.AppendLine("            ) SHIPREC")
            sqlStat.AppendLine("    ON SHIPREC.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("   AND SHIPREC.TANKSEQ = VL.TANKSEQ")
            'sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST")
            sqlStat.AppendLine("  LEFT JOIN TEST_GBM0010_CHARGECODE CST")
            sqlStat.AppendLine("    ON CST.COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
            sqlStat.AppendLine("   AND '1' = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' AND CST.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'POD%' AND CST.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("             ELSE '1'")
            sqlStat.AppendLine("             END")
            sqlStat.AppendLine("   AND CST.STYMD     <= VL.ENDYMD")
            sqlStat.AppendLine("   AND CST.ENDYMD    >= VL.STYMD")
            sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH") '承認履歴
            sqlStat.AppendLine("    ON  AH.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  AH.APPLYID      = VL.APPLYID")
            sqlStat.AppendLine("   AND  AH.STEP         = VL.LASTSTEP")
            sqlStat.AppendLine("   AND  AH.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV") 'STATUS用JOIN
            sqlStat.AppendLine("    ON  FV.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN AH.STATUS IS NOT NULL THEN AH.STATUS ")
            sqlStat.AppendLine("                               WHEN VL.AMOUNTORD <> VL.AMOUNTFIX THEN '" & C_APP_STATUS.APPAGAIN & "'")
            sqlStat.AppendLine("                               ELSE NULL")
            sqlStat.AppendLine("                           END")
            sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
            'sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRD")
            sqlStat.AppendLine("  LEFT JOIN TEST_GBM0005_TRADER TRD")
            sqlStat.AppendLine("    ON  TRD.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  TRD.CARRIERCODE  = VL.DTLOFFICE")
            sqlStat.AppendLine("   AND  TRD.DELFLG      <> @DELFLG")

            '*BR_CONTRACTOR名取得JOIN START
            'sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0005_TRADER TRBR")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRBR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPBR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN END

            '*ODR_CONTRACTOR名取得JOIN START
            'sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0005_TRADER TRODR")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRODR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPODR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
            '*ODR_CONTRACTOR名取得JOIN END

            '*FIX_CONTRACTOR名取得JOIN START
            'sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0005_TRADER TRFIX")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
            '*FIX_CONTRACTOR名取得JOIN END

            'sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TR")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0005_TRADER TR")
            sqlStat.AppendLine("        ON  VL.INVOICEDBY   = TR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TR.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0025_TORI TORI")
            sqlStat.AppendLine("        ON  VL.TORICODE     = TORI.TORICODE ")
            sqlStat.AppendLine("       AND  TORI.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TORI.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TORI.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TORI.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN GBM0001_COUNTRY CT")
            sqlStat.AppendLine("        ON  TR.COUNTRYCODE = CT.COUNTRYCODE ")
            sqlStat.AppendLine("       AND  CT.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CT.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CT.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CT.DELFLG      <> @DELFLG")

            'sqlStat.AppendLine("      LEFT JOIN GBT0009_JOTCODE JC")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBT0009_JOTCODE JC")
            sqlStat.AppendLine("        ON  VL.COSTCODE     = JC.COSTCODE ")
            sqlStat.AppendLine("       AND  JC.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  JC.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  JC.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN GBM0006_TANK TK")
            sqlStat.AppendLine("        ON  TK.TANKNO       = VL.TANKNO ")
            sqlStat.AppendLine("       AND  TK.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TK.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TK.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TK.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0004_CUSTOMER CS")
            sqlStat.AppendLine("        ON  CS.CUSTOMERCODE = VL.CONTRACTORFIX ")
            sqlStat.AppendLine("       AND  CS.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CS.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CS.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CS.DELFLG      <> @DELFLG")

            sqlStat.AppendLine(" WHERE VL.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("   AND EXISTS(SELECT 1 ") '基本情報が削除されていたら対象外
            sqlStat.AppendLine("                FROM GBT0004_ODR_BASE OBSS")
            sqlStat.AppendLine("               WHERE OBSS.ORDERNO = VL.ORDERNO")
            sqlStat.AppendLine("                 AND OBSS.DELFLG    <> @DELFLG)")
            sqlStat.AppendLine("   AND NOT EXISTS (SELECT 1 ") 'デマレッジ終端アクションはタンク動静のみ表示
            'sqlStat.AppendLine("                     FROM GBM0010_CHARGECODE CSTS")
            sqlStat.AppendLine("                     FROM TEST_GBM0010_CHARGECODE CSTS")
            sqlStat.AppendLine("                    WHERE CSTS.COMPCODE = @COMPCODE")
            sqlStat.AppendLine("                      AND CSTS.COSTCODE = VL.COSTCODE")
            sqlStat.AppendLine("                      AND CSTS.CLASS10  = '1'")
            sqlStat.AppendLine("                      AND CSTS.STYMD   <= VL.ENDYMD")
            sqlStat.AppendLine("                      AND CSTS.ENDYMD  >= VL.STYMD")
            sqlStat.AppendLine("                      AND CSTS.DELFLG  <> @DELFLG")
            sqlStat.AppendLine("                  )")
            sqlStat.AppendLine(" UNION ALL")
            'ノンブレーカー分
            sqlStat.AppendLine("SELECT '' AS OPERATION")
            sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
            sqlStat.AppendLine("     , '1' AS 'SELECT' ")
            sqlStat.AppendLine("     , '0' AS HIDDEN ")
            sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
            sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
            sqlStat.AppendLine("     , ''    AS BRTYPR") 'ノンブレーカーはBase情報なし
            sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
            sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
            sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
            sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
            sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
            sqlStat.AppendFormat("     , ISNULL(CST.{0},'')   AS COSTNAME", textTblField).AppendLine()
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTIONID")
            sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
            sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
            sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")
            sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
            sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
            sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
            sqlStat.AppendLine("     , VL.AMOUNTBR      AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.AMOUNTORD     AS AMOUNTORD")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE  WHEN '1900/01/01' THEN VL.AMOUNTORD ELSE VL.AMOUNTFIX END AS AMOUNTFIX")
            sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
            sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
            sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")

            sqlStat.AppendLine("     , VL.REPORTMONTH AS REPORTMONTH")
            sqlStat.AppendLine("     , CASE WHEN VL.REPORTMONTH = '' THEN '' ELSE VL.REPORTMONTH + '/01' END AS REPORTMONTHH")

            sqlStat.AppendLine("     , VL.REPORTMONTHORG AS REPORTMONTHORG")

            '業者名
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS4 = '{0}' THEN DPBR.NAMES ELSE TRBR.NAMES END AS CONTRACTORNAMEBR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS4 = '{0}' THEN DPODR.NAMES ELSE TRODR.NAMES END AS CONTRACTORNAMEODR ", GBC_CHARGECLASS4.DEPOT).AppendLine()
            sqlStat.AppendFormat("    ,CASE WHEN CST.CLASS4 = '{0}' THEN DPFIX.NAMES ELSE TRFIX.NAMES END AS CONTRACTORNAMEFIX ", GBC_CHARGECLASS4.DEPOT).AppendLine()

            sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
            sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
            sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
            sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
            sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
            sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
            sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
            sqlStat.AppendLine("     , VL.AMOUNTPAY      AS AMOUNTPAY")
            sqlStat.AppendLine("     , VL.LOCALPAY       AS LOCALPAY")

            sqlStat.AppendLine("     , VL.UAG_USD        AS UAG_USD")
            sqlStat.AppendLine("     , VL.UAG_LOCAL      AS UAG_LOCAL")
            sqlStat.AppendLine("     , VL.USD_USD        AS USD_USD")
            sqlStat.AppendLine("     , VL.USD_LOCAL      AS USD_LOCAL")
            sqlStat.AppendLine("     , VL.LOCAL_USD      AS LOCAL_USD")
            sqlStat.AppendLine("     , VL.LOCAL_LOCAL    AS LOCAL_LOCAL")

            sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
            sqlStat.AppendLine("     , VL.BRID           AS BRID")
            sqlStat.AppendLine("     , '1'               AS BRCOST") 'SOAの場合は削除させない
            sqlStat.AppendLine("     , ''                AS ACTYNO")
            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")
            sqlStat.AppendLine("     , '000000' AS AGENTKBNSORT")
            sqlStat.AppendLine("     , ''       AS DISPSEQISEMPTY")
            sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENT")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS1, '') AS CHARGE_CLASS1")
            sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'')  AS CHARGE_CLASS4")
            sqlStat.AppendLine("     , VL.SOACODE AS SOACODE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS RECOEDDATE")
            sqlStat.AppendLine("     , 'NONBREAKER' AS BRTYPE")
            sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN null ELSE VL.ACTUALDATE END AS ACTUALDATEDTM")

            sqlStat.AppendLine("      , JC.DATA             AS DATA ")
            sqlStat.AppendLine("      , JC.JOTCODE          AS JOTCODE ")
            sqlStat.AppendLine("      , JC.ACCODE           AS ACCODE ")
            sqlStat.AppendLine("      , VL.LOCALRATESOA     AS LOCALRATESOA ")
            sqlStat.AppendLine("      , VL.AMOUNTPAYODR     AS AMOUNTPAYODR ")
            sqlStat.AppendLine("      , VL.LOCALPAYODR      AS LOCALPAYODR ")
            sqlStat.AppendLine("      , CASE WHEN ISNULL(VL.TAXATION,'') <> '' AND ISNULL(CS.TORICODE,'') <> '' THEN ISNULL(CS.ACCCURRENCYSEGMENT,'') ELSE ISNULL(VL.ACCCURRENCYSEGMENT,'') END AS ACCCURRENCYSEGMENT ")
            sqlStat.AppendLine("      , ISNULL(TR.PAYDAY,'') AS PAYDAY ")
            sqlStat.AppendLine("      , ISNULL(TR.HOLIDAYFLG,'') AS HOLIDAYFLG ")
            sqlStat.AppendLine("      , ISNULL(CST.CRACCOUNT,'') AS CRACCOUNT ")
            sqlStat.AppendLine("      , ISNULL(CST.DBACCOUNT,'') AS DBACCOUNT ")
            sqlStat.AppendLine("      , ISNULL(CST.CRACCOUNTFORIGN,'') AS CRACCOUNTFORIGN ")
            sqlStat.AppendLine("      , ISNULL(CST.DBACCOUNTFORIGN,'') AS DBACCOUNTFORIGN ")
            sqlStat.AppendLine("      , ISNULL(CST.OFFCRACCOUNT,'') AS OFFCRACCOUNT ")
            sqlStat.AppendLine("      , ISNULL(CST.OFFDBACCOUNT,'') AS OFFDBACCOUNT ")
            sqlStat.AppendLine("      , ISNULL(CST.OFFCRACCOUNTFORIGN,'') AS OFFCRACCOUNTFORIGN ")
            sqlStat.AppendLine("      , ISNULL(CST.OFFDBACCOUNTFORIGN,'') AS OFFDBACCOUNTFORIGN ")
            sqlStat.AppendLine("      , ISNULL(CST.CRGENERALPURPOSE,'') AS CRGENERALPURPOSE ")
            sqlStat.AppendLine("      , ISNULL(CST.DBGENERALPURPOSE,'') AS DBGENERALPURPOSE ")
            sqlStat.AppendLine("      , ISNULL(CST.CRSEGMENT1,'') AS CRSEGMENT1 ")
            sqlStat.AppendLine("      , ISNULL(CST.DBSEGMENT1,'') AS DBSEGMENT1 ")
            sqlStat.AppendLine("      , ISNULL(TK.PROPERTY,'') AS PROPERTY ")
            sqlStat.AppendLine("      , CASE WHEN ISNULL(VL.TAXATION,'') <> '' AND ISNULL(CS.TORICODE,'') <> '' THEN ISNULL(CS.TORICODE,'') ELSE ISNULL(VL.TORICODE,'') END AS TORICODE ")
            sqlStat.AppendLine("      , ISNULL(TORI.BANKCODE,'') AS BANKCODE ")
            sqlStat.AppendLine("      , ISNULL(CT.DEBITSEGMENT,'') AS CRESEGMENT2 ")
            sqlStat.AppendLine("      , ISNULL(CT.DEBITSEGMENT,'') AS DEBSEGMENT2 ")
            sqlStat.AppendLine("      , CASE WHEN ISNULL(VL.TAXATION,'') <> '' AND ISNULL(CS.TORICODE,'') <> '' THEN ISNULL(CS.BOTHCLASS,'') ELSE ISNULL(VL.BOTHCLASS,'') END AS BOTHCLASS ")
            sqlStat.AppendLine("      , ISNULL(TR.COUNTRYCODE,'') AS TRCOUNTRYCODE ")
            sqlStat.AppendLine("      , ISNULL(CT.TAXRATE,'') AS CRETAXRATE ")
            sqlStat.AppendLine("      , ISNULL(CT.TAXRATE,'') AS DEBTAXRATE ")

            'sqlStat.AppendLine("  FROM GBT0008_JOTSOA_VALUE VL")
            sqlStat.AppendLine("  FROM TEST_GBT0008_JOTSOA_VALUE VL")

            'sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST")
            sqlStat.AppendLine("  LEFT JOIN TEST_GBM0010_CHARGECODE CST")
            sqlStat.AppendLine("    ON CST.COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
            sqlStat.AppendLine("   AND '1' = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' AND CST.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'POD%' AND CST.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("             ELSE '1'")
            sqlStat.AppendLine("             END")
            sqlStat.AppendLine("   AND CST.STYMD     <= VL.ENDYMD")
            sqlStat.AppendLine("   AND CST.ENDYMD    >= VL.STYMD")
            sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH") '承認履歴
            sqlStat.AppendLine("    On  AH.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   And  AH.APPLYID      = VL.APPLYID")
            sqlStat.AppendLine("   And  AH.STEP         = VL.LASTSTEP")
            sqlStat.AppendLine("   And  AH.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV") 'STATUS用JOIN
            sqlStat.AppendLine("    On  FV.CLASS        = 'APPROVAL'")
            sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '') THEN AH.STATUS ")
            sqlStat.AppendLine("                               WHEN CST.NONBR = '" & CONST_FLAG_YES & "' AND CST.CLASS2 <> '' THEN '" & C_APP_STATUS.APPAGAIN & "'")
            sqlStat.AppendLine("                               ELSE NULL")
            sqlStat.AppendLine("                           END")
            sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
            'sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRD")
            sqlStat.AppendLine("  LEFT JOIN TEST_GBM0005_TRADER TRD")
            sqlStat.AppendLine("    ON  TRD.COMPCODE     = @COMPCODE")
            sqlStat.AppendLine("   AND  TRD.CARRIERCODE  = VL.DTLOFFICE")
            sqlStat.AppendLine("   AND  TRD.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN START
            'sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0005_TRADER TRBR")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRBR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR")
            sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPBR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
            '*BR_CONTRACTOR名取得JOIN END

            '*ODR_CONTRACTOR名取得JOIN START
            'sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0005_TRADER TRODR")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRODR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR")
            sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPODR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
            '*ODR_CONTRACTOR名取得JOIN END

            '*FIX_CONTRACTOR名取得JOIN START
            'sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0005_TRADER TRFIX")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TRFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX")
            sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
            sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  DPFIX.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
            '*FIX_CONTRACTOR名取得JOIN END

            'sqlStat.AppendLine("      LEFT JOIN GBT0009_JOTCODE JC")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBT0009_JOTCODE JC")
            sqlStat.AppendLine("        ON  VL.COSTCODE     = JC.COSTCODE ")
            sqlStat.AppendLine("       AND  JC.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  JC.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  JC.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN GBM0006_TANK TK")
            sqlStat.AppendLine("        ON  TK.TANKNO       = VL.TANKNO ")
            sqlStat.AppendLine("       AND  TK.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TK.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TK.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TK.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0004_CUSTOMER CS")
            sqlStat.AppendLine("        ON  CS.CUSTOMERCODE = VL.CONTRACTORFIX ")
            sqlStat.AppendLine("       AND  CS.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CS.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CS.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CS.DELFLG      <> @DELFLG")

            'sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TR")
            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0005_TRADER TR")
            sqlStat.AppendLine("        ON  VL.INVOICEDBY   = TR.CARRIERCODE ")
            sqlStat.AppendLine("       AND  TR.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TR.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TR.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TR.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN TEST_GBM0025_TORI TORI")
            sqlStat.AppendLine("        ON  VL.TORICODE     = TORI.TORICODE ")
            sqlStat.AppendLine("       AND  TORI.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  TORI.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  TORI.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  TORI.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("      LEFT JOIN GBM0001_COUNTRY CT")
            sqlStat.AppendLine("        ON  TR.COUNTRYCODE = CT.COUNTRYCODE ")
            sqlStat.AppendLine("       AND  CT.COMPCODE     = '" & GBC_COMPCODE & "' ")
            sqlStat.AppendLine("       AND  CT.STYMD       <= VL.ENDYMD")
            sqlStat.AppendLine("       AND  CT.ENDYMD      >= VL.STYMD")
            sqlStat.AppendLine("       AND  CT.DELFLG      <> @DELFLG")

            sqlStat.AppendLine("WHERE VL.DELFLG     <> @DELFLG ")
            sqlStat.AppendLine("  AND VL.ORDERNO  LIKE 'NB%' ")
            sqlStat.AppendLine("  AND VL.BRID        = '' ")
            sqlStat.AppendLine("  ) TBLSUB")
            sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE USREXR")
            sqlStat.AppendLine("         ON USREXR.COMPCODE      = @COMPCODE")
            '
            sqlStat.AppendLine("        AND USREXR.CURRENCYCODE  = (SELECT CTRSUB.CURRENCYCODE ")
            sqlStat.AppendLine("                                      FROM GBM0001_COUNTRY CTRSUB ")
            'sqlStat.AppendLine("                                     WHERE CTRSUB.COUNTRYCODE = TBLSUB.COUNTRYCODE ")
            sqlStat.AppendLine("                                      WHERE CTRSUB.COUNTRYCODE = 'JP' ")
            sqlStat.AppendLine("                                       AND CTRSUB.STYMD      <= @TARGETYM ")
            sqlStat.AppendLine("                                       AND CTRSUB.ENDYMD     >= @TARGETYM ")
            sqlStat.AppendLine("                                       AND CTRSUB.DELFLG     <> @DELFLG )")
            sqlStat.AppendLine("        AND USREXR.TARGETYM      = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")
            sqlStat.AppendLine("        AND USREXR.DELFLG       <> @DELFLG")
            'SOA締め日JOIN START
            sqlStat.AppendLine("  LEFT JOIN GBT0006_CLOSINGDAY CLD")
            sqlStat.AppendLine("         ON CLD.COUNTRYCODE      = TBLSUB.COUNTRYCODE")
            sqlStat.AppendLine("        AND CLD.STYMD           <= @NOWDATE ")
            sqlStat.AppendLine("        AND CLD.ENDYMD          >= @NOWDATE ")
            sqlStat.AppendLine("        AND CLD.DELFLG          <> @DELFLG")
            'SOA締め日JOIN END
            '国ごとの表示桁数取得用JOIN START
            'USD以外
            sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CNTY")
            sqlStat.AppendLine("         ON CNTY.COUNTRYCODE       = TBLSUB.COUNTRYCODE")
            sqlStat.AppendLine("        AND CNTY.STYMD            <= @TARGETYM ")
            sqlStat.AppendLine("        AND CNTY.ENDYMD           >= @TARGETYM ")
            sqlStat.AppendLine("        AND CNTY.DELFLG           <> @DELFLG")
            'USD
            sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE USDDECIMAL")
            sqlStat.AppendLine("         ON USDDECIMAL.COMPCODE   = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.SYSCODE    = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.CLASS      = '" & C_FIXVALUECLAS.USD_DECIMALPLACES & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.KEYCODE    = '" & GBC_CUR_USD & "'")
            sqlStat.AppendLine("        AND USDDECIMAL.DELFLG    <> @DELFLG")
            '国ごとの表示桁数取得用JOIN END
            '******************************
            '検索画面条件の付与 START
            '******************************
            sqlStat.AppendLine("WHERE 1 = 1 ")

            'sqlStat.AppendLine("AND EXISTS ( SELECT 1 FROM GBM0010_CHARGECODE CSTSUB")
            sqlStat.AppendLine("AND EXISTS ( SELECT 1 FROM TEST_GBM0010_CHARGECODE CSTSUB")
            sqlStat.AppendLine("              WHERE CSTSUB.COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("                AND CSTSUB.COSTCODE  = TBLSUB.COSTCODE")
            sqlStat.AppendLine("                AND '1' = CASE WHEN TBLSUB.DTLPOLPOD LIKE 'POL%' AND CSTSUB.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'POD%' AND CSTSUB.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                               WHEN TBLSUB.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("                          ELSE '1'")
            sqlStat.AppendLine("                          END")
            sqlStat.AppendLine("                AND CSTSUB.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("                AND CSTSUB.SOA      <> (SELECT FVS.VALUE3   ")
            sqlStat.AppendLine("                                      FROM COS0017_FIXVALUE FVS ")
            sqlStat.AppendLine("                                     WHERE FVS.COMPCODE = '" & GBC_COMPCODE_D & "'")
            sqlStat.AppendLine("                                       AND FVS.SYSCODE  = '" & C_SYSCODE_GB & "'")
            sqlStat.AppendLine("                                       AND FVS.CLASS    = 'AGENTSOA'")
            sqlStat.AppendLine("                                       AND FVS.KEYCODE  = 'CF'")
            sqlStat.AppendLine("                                       AND FVS.DELFLG  <> @DELFLG)")
            'sqlStat.AppendLine("                AND CSTSUB.SOA  IN (SELECT FVS.VALUE3 ")
            'sqlStat.AppendLine("                                      FROM COS0017_FIXVALUE FVS ")
            'sqlStat.AppendLine("                                     WHERE FVS.COMPCODE = '" & GBC_COMPCODE_D & "'")
            'sqlStat.AppendLine("                                       AND FVS.SYSCODE  = '" & C_SYSCODE_GB & "'")
            'sqlStat.AppendLine("                                       AND FVS.CLASS    = 'AGENTSOA'")
            'sqlStat.AppendLine("                                       AND FVS.DELFLG  <> @DELFLG)")
            sqlStat.AppendLine("           )")

            '******************************
            '非表示費用コード END
            '******************************

            sqlStat.AppendLine("  ) TBL")
            sqlStat.AppendLine("  ) TBLALL")
            sqlStat.AppendLine("  ) TBLALLSUM")
            '******************************
            '計上月絞り込み条件START
            '******************************
            sqlStat.AppendLine(" WHERE 1=1")
            If reportMonth <> "" Then
                sqlStat.AppendLine("  AND @REPORTMONTH = TBLALLSUM.REPORTMONTH")
            End If

            If countryCode <> "" Then
                sqlStat.AppendLine("  AND @COUNTRYCODE = TBLALLSUM.COUNTRYCODE")
            End If

            If department <> "" AndAlso department <> "ALL" Then
                sqlStat.AppendLine("  AND (@DEPARTMENT = TBLALLSUM.CRESECTION")
                sqlStat.AppendLine("   OR  @DEPARTMENT = TBLALLSUM.DEBSECTION )")
            End If

            '******************************
            '計上月絞り込み条件END
            '******************************
            sqlStat.AppendLine(" GROUP BY TBLALLSUM.DEBSEGMENT2 ,TBLALLSUM.CRESEGMENT2 ,TBLALLSUM.TORICODE ,TBLALLSUM.DEBSUBJECT ,TBLALLSUM.DEBSECTION ,TBLALLSUM.CRESUBJECT ,TBLALLSUM.CRESECTION ,TBLALLSUM.DEBSEGMENT1 ,TBLALLSUM.CRESEGMENT1 ,TBLALLSUM.BOTHCLASS ,TBLALLSUM.REVENUE ,TBLALLSUM.TAXATION ")
            sqlStat.AppendLine(" ORDER BY TBLALLSUM.TORICODE ,TBLALLSUM.REVENUE ")

            Dim dtDbResult As New DataTable
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
          sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open() '接続オープン
                Dim soaAppDateFrom As Date
                Dim soaAppDateTo As Date
                If Date.Now.Day() > 25 Then
                    soaAppDateFrom = DateSerial(Now.Year, Now.Month, 26)
                    soaAppDateTo = DateSerial(Now.Year, Now.Month + 1, 25)
                Else
                    soaAppDateFrom = DateSerial(Now.Year, Now.Month - 1, 26)
                    soaAppDateTo = DateSerial(Now.Year, Now.Month, 25)
                End If
                'SQLパラメータ設定
                With sqlCmd.Parameters

                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                    .Add("@SOAAPPDATEFROM", SqlDbType.Date).Value = soaAppDateFrom
                    .Add("@SOAAPPDATETO", SqlDbType.Date).Value = soaAppDateTo
                    .Add("@TARGETYM", SqlDbType.Date).Value = Me.txtOrderNo.Text & "/" & "01"
                    .Add("@DATE", SqlDbType.DateTime).Value = DateTime.Now
                    .Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT
                    .Add("@USER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now

                    If reportMonth <> "" Then
                        .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = reportMonth
                    End If
                    If countryCode <> "" Then
                        .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
                    End If
                    If department <> "" Then
                        .Add("@DEPARTMENT", SqlDbType.NVarChar).Value = department
                    End If

                End With
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get AccountingCollaborationList Error")
                    End If
                    retDt = CreateOrderInfoTableAC()

                    For Each dr As DataRow In dt.Rows
                        Dim writeDr As DataRow
                        writeDr = retDt.NewRow
                        For Each col As DataColumn In dt.Columns
                            writeDr.Item(col.ColumnName) = Convert.ToString(dr.Item(col.ColumnName))
                        Next
                        retDt.Rows.Add(writeDr)
                    Next

                End Using
            End Using
            Return retDt
        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try

        Return retDt
    End Function
    ''' <summary>
    ''' オーダー情報を格納する空のデータテーブルを作成する
    ''' </summary>
    Private Function CreateOrderInfoTableAC() As DataTable
        Dim retDt As New DataTable
        '固定部分は追加しておく
        retDt.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        retDt.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        retDt.Columns("OPERATION").DefaultValue = ""
        retDt.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        retDt.Columns("TIMSTP").DefaultValue = ""
        retDt.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        retDt.Columns.Add("HIDDEN", GetType(Integer))
        retDt.Columns.Add("DATAID", GetType(String))
        retDt.Columns("DATAID").DefaultValue = ""
        retDt.Columns.Add("SYSKEY", GetType(String))
        retDt.Columns("SYSKEY").DefaultValue = ""
        retDt.Columns.Add("REPORTMONTH", GetType(String))         '出力月
        retDt.Columns("REPORTMONTH").DefaultValue = ""
        retDt.Columns.Add("SOACODE", GetType(String))             'SOAコード
        retDt.Columns("SOACODE").DefaultValue = ""
        Dim colList As New List(Of String) From {"ORDERNO", "BRTYPE", "TANKSEQ", "DTLPOLPOD", "DTLOFFICE", "TANKNO", "BRTYPR",
                                                 "COSTCODE", "COSTNAME", "ACTIONID", "DISPSEQ", "LASTACT", "AMOUNTBR",
                                                 "AMOUNTORD", "AMOUNTFIX", "CONTRACTORBR", "CONTRACTORODR", "CONTRACTORFIX",
                                                 "SCHEDELDATEBR", "SCHEDELDATE", "ACTUALDATE", "APPLYID", "APPLYTEXT",
                                                 "LASTSTEP", "STATUS", "BRID", "BRCOST", "ACTYNO", "AGENTKBNSORT",
                                                 "USETYPE", "DISPSEQISEMPTY", "APPLY", "INVOICEDBY", "AGENTORGANIZER",
                                                 "DELFLG", "IS_ODR_CHANGECOST", "IS_FIX_CHANGECOST", "IS_CALC_DEMURRAGE",
                                                 "TIP", "DEMURTO", "DEMURUSRATE1", "DEMURUSRATE2", "CHARGE_CLASS1",
                                                 "CHARGE_CLASS4", "LOCALRATE", "CURRENCYCODE", "AGENT", "ORGOFFICE",
                                                 "OTHEROFFICE", "COUNTRYCODE", "EXRATE", "REFAMOUNT", "AMOUNTPAY",
                                                 "LOCALPAY", "SOAAPPDATE", "IS_UPDATE_SHIPDATE", "ORIGINDESTINATION",
                                                 "COMMAMOUNT", "CONTRACTORNAMEBR", "CONTRACTORNAMEODR", "CONTRACTORNAMEFIX",
                                                 "BILLINGYMD", "ISBILLINGCLOSED", "USDAMOUNT", "LOCALAMOUNT", "REPORTYMD",
                                                 "JOT", "ISAUTOCLOSE", "ISAUTOCLOSELONG", "DISPLAYCURRANCYCODE", "TAXATION",
                                                 "TAXRATE", "REPORTMONTHH", "COUNTRYNAMEH", "OFFICENAMEH", "APPLYUSERH",
                                                 "CURRENCYCODEH", "LOCALRATEH", "REPORTMONTHORG", "DATA", "JOTCODE", "ACCODE",
                                                 "LOCALRATESOA", "AMOUNTPAYODR", "LOCALPAYODR", "UAG_USD", "UAG_LOCAL", "USD_USD",
                                                 "USD_LOCAL", "LOCAL_USD", "LOCAL_LOCAL", "FINALREPORTNOH", "CLOSEDATEH", "PRINTDATEH",
                                                 "DATACRITERIA", "JOURNALENTRY", "INPUTSCREENNO", "DOCUMENTDATE", "SETTLEMONTHCLS",
                                                 "PROOFNO", "SLIPNUMBER", "SLIPNO", "DETAILLINENO", "DEBSUBJECT", "DEBSECTION", "DEBBANK",
                                                 "DEBPARTNER", "DEBGENPURPOSE", "DEBSEGMENT1", "DEBSEGMENT2", "DEBSEGMENT3",
                                                 "DEBNO1", "DEBNO2", "DEBCONTAXCLS", "DEBCONTAXCODE", "DEBCONTAXRTCLS", "DEBSIMINPCLS",
                                                 "DEBAMOUNT", "DEBCONSTAXAMOUNT", "DEBFORCURAMOUNT", "DEBFORCURRATE", "DEBFORCURTRDCLS",
                                                 "CRESUBJECT", "CRESECTION", "CREBANK", "CREPARTNER", "CREGENPURPOSE",
                                                 "CRESEGMENT1", "CRESEGMENT2", "CRESEGMENT3", "CRENO1", "CRENO2",
                                                 "CRECONTAXCLS", "CRECONTAXCODE", "CRECONTAXRTCLS", "CRESIMINPCLS", "CREAMOUNT",
                                                 "CRECONSTAXAMOUNT", "CREFORCURAMOUNT", "CREFORCURRATE", "CREFORCURTRDCLS", "DEADLINE",
                                                 "SUMMARY", "SUMMARYCODE", "CREATEDDATE", "CREATEDTIME", "AUTHOR", "ACCCURRENCYSEGMENT",
                                                 "CRACCOUNT", "DBACCOUNT", "CRACCOUNTFORIGN", "DBACCOUNTFORIGN", "CRGENERALPURPOSE",
                                                 "DBGENERALPURPOSE", "CRSEGMENT1", "DBSEGMENT1", "PROPERTY", "BANKCODE",
                                                 "RECOEDDATE", "ACTUALDATEDTM", "TORICODE", "USDAMOUNT_BOFORE_ROUND", "LOCALAMOUNT_BOFORE_ROUND",
                                                 "DECIMALPLACES", "ROUNDFLG", "USDDECIMALPLACES", "USDROUNDFLG", "CLOSINGMONTH", "REPORTYMD_BASE",
                                                 "REPORTYMDORG", "REVENUE", "BOTHCLASS", "TRCOUNTRYCODE", "CRETAXRATE", "DEBTAXRATE", "HOLIDAYFLG",
                                                 "PAYDAY", "OFFCRACCOUNT", "OFFDBACCOUNT", "OFFCRACCOUNTFORIGN", "OFFDBACCOUNTFORIGN"}
        For Each colName As String In colList
            retDt.Columns.Add(colName, GetType(String))
            retDt.Columns(colName).DefaultValue = ""
        Next

        '検討中
        retDt.Columns.Add("DUMMY", GetType(String))
        retDt.Columns.Add("DUMMY2", GetType(String))

        Dim dr As DataRow = retDt.NewRow
        dr.Item("DUMMY") = ""
        retDt.Rows.Add(dr)
        Return retDt
    End Function
    ''' <summary>
    ''' 期日設定
    ''' </summary>
    Private Function GetPayDay(targetMonth As String, holidayFlg As String, payday As String) As String
        Dim retVal As String = ""   '戻り値用のString
        Dim retDt As New DataTable
        Dim targetday As String = ""
        Dim dt As Date

        If targetMonth = "" Then
            Return retVal
        End If

        If Not Date.TryParse(targetMonth & "/01", dt) Then
            Return retVal
        End If

        If payday = "LAST" Then
            dt = dt.AddMonths(1).AddDays(-1)
            targetday = dt.ToString("yyyy/MM/dd")
        Else
            targetday = targetMonth & "/" & payday
        End If

        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("WITH DateTable (MyDate, Part) ")
        sqlStat.AppendLine("  AS( ")
        sqlStat.AppendLine("      SELECT	(DATEADD(dd, 1, EOMONTH (@TargetDate , -2))),datepart(weekday,DATEADD(dd, 1, EOMONTH (@TargetDate , -2))) ")
        sqlStat.AppendLine("      UNION ALL")
        sqlStat.AppendLine("      SELECT	DATEADD(dd, 1, MyDate),datepart(weekday,DATEADD(dd, 1, MyDate)) ")
        sqlStat.AppendLine("      FROM   DateTable ")
        sqlStat.AppendLine("      WHERE  MyDate < EOMONTH(@TargetDate, 1) ")
        sqlStat.AppendLine("    ), WorkCalender as ( ")
        sqlStat.AppendLine("      SELECT   d.MyDate,d.Part,isnull(h.holyday_name,'') as holidayname, ")
        sqlStat.AppendLine("      case when d.Part = '1' or d.Part = '7' or isnull(h.holyday_name,'') <> '' then '1' ")
        sqlStat.AppendLine("      else '0' end as holydayflg ")
        sqlStat.AppendLine("      FROM     DateTable d ")
        sqlStat.AppendLine("      left join holydays h ")
        sqlStat.AppendLine("      on h.holyday_date = d.MyDate ")
        sqlStat.AppendLine("    ) ")

        '休日フラグ
        If holidayFlg = "B" Then
            '前
            sqlStat.AppendLine("  select format(max(MyDate),'yyyy/MM/dd') from WorkCalender w ")
            sqlStat.AppendLine("  where w.MyDate <= @TargetDate ")

        ElseIf holidayFlg = "A" Then
            '後
            sqlStat.AppendLine("  select format(min(MyDate),'yyyy/MM/dd') from WorkCalender w ")
            sqlStat.AppendLine("  where w.MyDate >= @TargetDate ")

        Else
            'そのまま
            Return targetday

        End If
        sqlStat.AppendLine("  and   w.holydayflg <> '1' ")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramTargetDate As SqlParameter = sqlCmd.Parameters.Add("@TargetDate", SqlDbType.NVarChar)
            'SQLパラメータ値セット
            paramTargetDate.Value = targetday
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)

                If retDt.Rows.Count > 0 Then
                    retVal = retDt.Rows(0).Item(0).ToString()
                End If

            End Using
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' 部門リストアイテムを設定
    ''' </summary>
    Private Sub SetDepartmentListItem(selectedValue As String)

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

        'リストクリア
        Me.lbDepartment.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DEPARTMENTLIST"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbDepartment
        Else
            COA0017FixValue.LISTBOX2 = Me.lbDepartment
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbDepartment = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbDepartment = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If
        Else
            returnCode = COA0017FixValue.ERR
            Return
        End If

        '一覧先頭にALLを追加
        Me.lbDepartment.Items.Insert(0, New ListItem("ALL", "ALL"))

    End Sub

    ''' <summary>
    ''' 部門名設定
    ''' </summary>
    Public Sub txtDepartment_Change()
        Try
            Me.lblDepartmentText.Text = ""
            If Me.txtDepartment.Text.Trim = "" Then
                Return
            End If
            SetDepartmentListItem(Me.txtDepartment.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbDepartment.Items.Count > 0 Then
                Dim findListItem = Me.lbDepartment.Items.FindByValue(Me.txtDepartment.Text)
                If findListItem IsNot Nothing Then
                    Me.lblDepartmentText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbDepartment.Items.FindByValue(Me.txtDepartment.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblDepartmentText.Text = findListItemUpper.Text
                        Me.txtDepartment.Text = findListItemUpper.Value
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