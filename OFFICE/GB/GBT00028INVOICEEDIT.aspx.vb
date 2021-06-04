Imports System.Data.SqlClient
Imports System.Reflection
Imports BASEDLL

''' <summary>
''' 請求書入力
''' </summary>
Public Class GBT00028INVOICEEDIT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00028L" '自身のMAPID
    'DBテーブル名
    Private Const CONST_TBL_OV As String = "GBT0005_ODR_VALUE"
    Private Const CONST_TBL_OB As String = "GBT0004_ODR_BASE"
    Private Const CONST_TBL_PM As String = "GBM0008_PRODUCT"
    Private Const CONST_TBL_TK As String = "GBM0006_TANK"
    Private Const CONST_TBL_FV As String = "COS0017_FIXVALUE"
    Private Const CONST_TBL_OV2 As String = "GBT0007_ODR_VALUE2"

    Private Const CONST_TBL_COUNTRY As String = "GBM0001_COUNTRY"
    Private Const CONST_TBL_PORT As String = "GBM0002_PORT"
    Private Const CONST_TBL_CUSTOMER As String = "GBM0004_CUSTOMER"
    Private Const CONST_TBL_EXRATE As String = "GBM0020_EXRATE"
    Private Const CONST_TBL_TORI As String = "GBM0025_TORI"
    Private Const CONST_TBL_CLOSINGDAY As String = "GBT0006_CLOSINGDAY"
    Private Const CONST_TBL_BANK As String = "GBM0024_BANK"
    Private Const CONST_TBL_INVOICEINFO As String = "GBT0016_INVOICE_INFO"
    Private Const CONST_TBL_INVOICETANK As String = "GBT0017_INVOICE_TANKINFO"

    Private Const CONST_TBL_LC As String = "GBT0010_LBR_CONTRACT"
    Private Const CONST_TBL_LA As String = "GBT0011_LBR_AGREEMENT"
    Private Const CONST_TBL_LT As String = "GBT0012_RESRVLEASETANK"

    '内部保持のデータテーブル名称
    Private Const CONST_DT_NAME_CUSTOMERINFO As String = "CUSTOMERINFO"
    Private Const CONST_DT_NAME_TANKINFO As String = "TANKINFO"

    'VIEWSTATE名
    Private Const CONST_VS_NAME_GBT00028RV As String = "GBT00028RValues"
    Private Const CONST_VS_NAME_CURRENT_VAL As String = "CURRENTVAL"

    Private Const CONST_VS_NAME_PREV_VAL As String = "PREVVAL"

    '請求書タイプ
    Private Const CONST_VIEWID_MNG As String = "Management"
    '請求書タイプ
    Private Const CONST_REPTYPE_SHIP As String = "InvoiceShip"
    '請求書タイプ
    Private Const CONST_REPTYPE_LEASE As String = "InvoiceLease"
    ''' <summary>
    ''' 請求書のレンジ名()
    ''' </summary>
    ''' <remarks>請求書別金額</remarks>
    Private Const CONST_AXISCHART_RANGENAME As String = "RNG_TOTALINVOICE"

    ''' <summary>
    ''' 請求書検索結果画面情報
    ''' </summary>
    ''' <returns></returns>
    Public Property GBT00028RValues As GBT00028RESULT.GBT00028RValues
    Public Property GBT00028INVOICEEDITValues As GBT00028INVOICEEDIT.GBT00028INVOICEEDITDispItem
    ''' <summary>
    ''' 申請画面情報保持クラス
    ''' </summary>
    ''' <returns></returns>
    Public Property GBT00024AValues As GBT00024APPROVAL.GBT00024RValues
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 自身をリロードする際に保持するメッセージNo
    ''' </summary>
    ''' <returns></returns>
    Public Property PrevMessageNo As String = ""
    Public Property PrevInvoiceNo As String = ""
    ''' <summary>
    ''' ポストバック時画面上の情報を保持
    ''' </summary>
    Private DsDisDisplayValues As DataSet

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
                Me.Form.Attributes.Add("data-profid", COA0019Session.PROFID)
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '遷移元よりデータ取得
                '****************************************
                Dim ds As DataSet = New DataSet
                If Me.GetPrevDisplayInfo(ds) <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                Me.DsDisDisplayValues = CommonFunctions.DeepCopy(ds)
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnThisMapVariant.Value
                COA0031ProfMap.COA0031GetDisplayTitle()

                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})
                    Return
                End If
                ' Invoice New 押下時補正
                If GBT00028RValues.NewInvoiceCreate = True Then
                    Me.lblTitleText.Text = "Create New Invoice"
                End If
                If Me.GBT00028RValues.InvoiceType = "L" Then
                    Me.lblTitleText.Text += " (Lease)"
                End If
                '****************************************
                'Fixvalueを元にリストボックスを作成
                '****************************************
                SetFixvalueListItem("INV_LANGUAGE", Me.lbLanguage)

                '****************************************
                '取得データを画面展開
                '****************************************
                SetDispValues(Me.DsDisDisplayValues)
                '****************************************
                '使用可否制御
                '****************************************
                enabledControls()
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)

            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                Me.GBT00028RValues = DirectCast(ViewState(CONST_VS_NAME_GBT00028RV), GBT00028RESULT.GBT00028RValues)
                Me.DsDisDisplayValues = CollectDispValues()
                SaveDisplayTankList()
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
            End If
            '**********************
            ' 一覧表の行ダブルクリック判定
            '**********************
            If Me.hdnListDBclick.Value <> "" Then
                ListRowDbClick()
                Me.hdnListDBclick.Value = ""
                'Return '単票ページにリダイレクトするため念のため処理は終わらせる
            End If
            '**********************
            ' Help表示
            '**********************
            If Me.hdnHelpChange.Value IsNot Nothing AndAlso Me.hdnHelpChange.Value <> "" Then
                DivShowHelp_DoubleClick(CONST_MAPID)
                Me.hdnHelpChange.Value = ""
            End If
            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            ViewState(CONST_VS_NAME_CURRENT_VAL) = Me.DsDisDisplayValues

        Catch ex As Threading.ThreadAbortException
            Return
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.SYSTEMADM)})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.SYSTEMADM
            COA0003LogFile.COA0003WriteLog()

            Return
        Finally
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        End Try

    End Sub

    ''' <summary>
    ''' 左ビュー表示処理
    ''' </summary>
    Private Sub DisplayLeftView()

        Dim GBA00004CountryRelated As GBA00004CountryRelated = New GBA00004CountryRelated
        Dim targetObject As Control = Nothing
        'ビューの存在チェック
        Dim changeViewObj As View = DirectCast(Me.mvLeft.FindControl(Me.hdnLeftboxActiveViewId.Value), View)
        If changeViewObj IsNot Nothing Then
            Me.mvLeft.SetActiveView(changeViewObj)
            Select Case changeViewObj.ID
                '他のビューが存在する場合はViewIdでCaseを追加

                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Me.hdnCalendarValue.Value = FormatDateYMD(txtobj.Text, GBA00003UserSetting.DATEFORMAT)

                        Me.mvLeft.Focus()
                    End If
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
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        If IsModifiedData() Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, pageObject:=Me, submitButtonId:="btnBackOk")
            Return
        End If
        '変更を検知しない場合はそのまま前画面へ
        btnBackOk_Click()
    End Sub

    ''' <summary>
    ''' 戻る確定時処理(btnBack_Click時に更新データが無い場合も通る)
    ''' </summary>
    Public Sub btnBackOk_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
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
    ''' 絞り込みボタン押下時処理 ※仮
    ''' </summary>
    Public Sub btnExtract_Click()

    End Sub

    ''' <summary>
    ''' Excel出力ボタン押下時 ※仮対応
    ''' </summary>
    Public Sub btnOutputExcel_Click()
        Me.hdnPrintType.Value = "Excel"
        If IsModifiedData() Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMOUTPUT, pageObject:=Me, submitButtonId:="btnOutputOK")
            Return
        End If
        '変更を検知しない場合はそのまま前画面へ
        btnOutputOK_Click()

    End Sub

    ''' <summary>
    ''' PDF出力ボタン押下時
    ''' </summary>
    Public Sub btnOutput_Click()
        Me.hdnPrintType.Value = "PDF"
        If IsModifiedData() Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMOUTPUT, pageObject:=Me, submitButtonId:="btnOutputOK")
            Return
        End If
        '変更を検知しない場合はそのまま前画面へ
        btnOutputOK_Click()

    End Sub

    ''' <summary>
    ''' Excel出力ボタン押下時
    ''' </summary>
    Public Sub btnOutputOK_Click()

        Dim OutInvoiceDt As DataTable
        Dim repType As String
        If Me.GBT00028RValues.InvoiceType <> "L" Then
            OutInvoiceDt = GetOutPutInfo(GBT00028RValues.InvoiceNo, GBT00028RValues.GBT00028SValues.InvoiceMonth)
            repType = CONST_REPTYPE_SHIP
        Else
            OutInvoiceDt = GetOutPutInfoLease(GBT00028RValues.InvoiceNo, GBT00028RValues.GBT00028SValues.InvoiceMonth)
            repType = CONST_REPTYPE_LEASE
        End If

        Dim dsCustomer As DataSet = DirectCast(ViewState(CONST_VS_NAME_CURRENT_VAL), DataSet)
        Dim dtCustomer As DataTable = dsCustomer.Tables(CONST_DT_NAME_CUSTOMERINFO)
        Dim drCustomer As DataRow = dtCustomer.Rows(0)

        '帳票出力
        With Nothing

            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            Dim COA0017FixValue As New BASEDLL.COA0017FixValue
            Dim dtTmp As DataTable = OutInvoiceDt.Copy
            Dim tmpFile As String = ""
            Dim outUrl As String = ""

            '印刷対象出力
            Dim dicPrint As New Dictionary(Of String, Integer)
            Dim PrintCnt As Integer = 0
            Dim RepMainCnt As Integer = 0
            Dim RepSubCnt As Integer = 0

            '出力件数取得
            COA0017FixValue.COMPCODE = GBC_COMPCODE_D
            COA0017FixValue.CLAS = "INV_REPORTCONF"
            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                Dim dicReportConf = COA0017FixValue.VALUEDIC
                If dicReportConf.ContainsKey(repType) Then
                    RepMainCnt = Convert.ToInt32(dicReportConf(repType)(1))
                    RepSubCnt = Convert.ToInt32(dicReportConf(repType)(2))
                End If
            Else
                Throw New Exception("Fix value getError")
            End If

            PrintCnt = (CType(System.Math.Ceiling(OutInvoiceDt.Rows.Count / RepSubCnt), Integer))

            'データ出力
            COA0027ReportTable.MAPID = CONST_MAPID                      'PARAM01:画面ID
            'PARAM02:帳票ID
            'COA0027ReportTable.REPORTID = CONST_REPTYPE_SHIP & "_" & Convert.ToString(drCustomer.Item("CURRENCY")) & "_" & Convert.ToString(drCustomer.Item("OUTLANGUAGE"))
            COA0027ReportTable.REPORTID = repType & "_" & Convert.ToString(drCustomer.Item("ACCCURRENCYSEGMENT")) & "_" & Convert.ToString(drCustomer.Item("OUTLANGUAGE"))
            'COA0027ReportTable.FILETYPE = "PDF"                         'PARAM03:出力ファイル形式
            COA0027ReportTable.FILETYPE = "XLSX"                         'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = dtTmp                    'PARAM04:データ参照tabledata
            COA0027ReportTable.ADDSHEET = "データ"      'PARAM05:追記シート（任意）
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If

            tmpFile = COA0027ReportTable.FILEpath
            outUrl = COA0027ReportTable.URL

            '本紙・ドラフト
            If Me.rblInvoiceTyp.SelectedValue = "ORIGINAL" Then
                If OutInvoiceDt.Rows.Count <= RepMainCnt Then
                    ' 本紙
                    dicPrint = New Dictionary(Of String, Integer) From {{"ORIGINAL_0A", 1}, {"COPY_0A", 1}}
                Else
                    dicPrint = New Dictionary(Of String, Integer) From {{"ORIGINAL_1A", 1}, {"DETAILB", PrintCnt}, {"COPY_1C", 1}, {"DETAILD", PrintCnt}}
                End If
            Else
                If OutInvoiceDt.Rows.Count <= RepMainCnt Then
                    'ドラフト
                    dicPrint = New Dictionary(Of String, Integer) From {{"DRAFT_0A", 1}}
                Else
                    dicPrint = New Dictionary(Of String, Integer) From {{"DRAFT_1A", 1}, {"DETAILA", PrintCnt}}
                End If
            End If

            Dim dicCnt As Integer = 1 ' 
            For Each item As KeyValuePair(Of String, Integer) In dicPrint

                Dim sheetId As String = Left(item.Key, Len(item.Key) - 1)
                COA0027ReportTable.REPORTID = repType & "_" & sheetId         'PARAM02:帳票ID
                COA0027ReportTable.ADDSHEET = sheetId                               'PARAM05:追記シート（任意）
                COA0027ReportTable.FILETYPE = "XLSX"                                 'PARAM03:出力ファイル形式

                For i As Integer = 1 To item.Value
                    dtTmp = OutInvoiceDt.Clone
                    COA0027ReportTable.TBLDATA = dtTmp                          'PARAM04:データ参照tabledata
                    COA0027ReportTable.ADDFILE = tmpFile
                    COA0027ReportTable.ADDSHEETNO = dicCnt & "_PAGECNT" & i.ToString & "-" & item.Value    'PARAM05:追記シート（任意）
                    COA0027ReportTable.COA0027ReportTable()
                    If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                    Else
                        CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                        Return
                    End If
                    tmpFile = COA0027ReportTable.FILEpath
                    outUrl = COA0027ReportTable.URL

                Next
                dicCnt = dicCnt + 1

            Next

            Dim totalInvoice As Double = 0.0
            Dim con As String = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=""{0}"";" &
                                              "Extended Properties = ""Excel 12.0 Xml;HDR=NO"";" _
                                              , tmpFile)
            Using sqlCon As New OleDb.OleDbConnection(con)
                sqlCon.Open()
                Dim sqlString As String = String.Format("select * from {0}", CONST_AXISCHART_RANGENAME)
                Using sqlAdp As New OleDb.OleDbDataAdapter(sqlString, sqlCon)
                    Dim retDt As New DataTable
                    sqlAdp.Fill(retDt)
                    totalInvoice = DecimalStringToDecimal(Convert.ToString(retDt.Rows(0).Item(0)))
                End Using 'End OleDb.OleDbDataAdapter(sqlString, sqlCon)
            End Using 'End OleDb.OleDbConnection(con)

            'PDF出力
            dtTmp = OutInvoiceDt.Clone
            COA0027ReportTable.TBLDATA = dtTmp                          'PARAM04:データ参照tabledata
            COA0027ReportTable.ADDFILE = tmpFile
            COA0027ReportTable.ADDSHEET = "印刷対象外２"                      'PARAM05:追記シート（任意）
            COA0027ReportTable.ADDSHEETNO = Nothing                           'PARAM05:追記シート（任意）
            If Me.hdnPrintType.Value.ToUpper = "PDF" Then
                COA0027ReportTable.FILETYPE = "PDF"                         'PARAM03:出力ファイル形式
            Else
                COA0027ReportTable.FILETYPE = "XLSX"                         'PARAM03:出力ファイル形式
            End If
            COA0027ReportTable.COA0027ReportTable()
            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If

            '出力件数更新
            If Me.rblInvoiceTyp.SelectedValue = "ORIGINAL" Then
                Me.txtOutCntOriginal.Text = Convert.ToString(DecimalStringToDecimal(Me.txtOutCntOriginal.Text) + 1)
                drCustomer.Item("ORIGINALOUTPUT") = Me.txtOutCntOriginal.Text
            Else
                Me.txtOutCntDraft.Text = Convert.ToString(DecimalStringToDecimal(Me.txtOutCntDraft.Text) + 1)
                drCustomer.Item("DRAFTOUTPUT") = Me.txtOutCntDraft.Text
            End If

            Dim invoiceNo As String = EntryInvoice(dsCustomer, Nothing, Nothing, totalInvoice)

            '別画面でExcelを表示
            hdnPrintURL.Value = COA0027ReportTable.URL
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
        End With

    End Sub

    ''' <summary>
    ''' 保存ボタン押下時イベント
    ''' </summary>
    Public Sub btnSave_Click()

        '****************************************
        '請求対象有無チェック
        '****************************************
        Dim ds As DataSet = Me.DsDisDisplayValues
        ds = CommonFunctions.DeepCopy(ds)
        Dim saveDt As DataTable = Nothing

        'タンク情報
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        Dim qToOrderTank = (From item In dtTankInfo Where Convert.ToString(item("TOINVOICE")) = "1")
        If qToOrderTank.Any = False Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        Else
            saveDt = qToOrderTank.CopyToDataTable
        End If

        '****************************************
        '変更データ有無チェック
        '****************************************
        If Not IsModifiedData() Then
            '変更が全くない場合はメッセージを表示
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '****************************************
        '変更前後のデータ取得
        '****************************************
        Dim prevds As DataSet = DirectCast(ViewState(CONST_VS_NAME_PREV_VAL), DataSet)
        Dim workDs As DataSet = CommonFunctions.DeepCopy(Me.DsDisDisplayValues)
        '****************************************
        '入力チェック
        '****************************************
        If CheckInput(workDs, prevds) = False Then
            Return
        End If
        ''****************************************
        '登録処理
        '****************************************
        Dim invoiceNo As String = EntryInvoice(workDs, prevds, saveDt, Nothing)
        Me.PrevMessageNo = C_MESSAGENO.NORMALDBENTRY
        Me.PrevInvoiceNo = invoiceNo
        Server.Transfer(Request.Url.LocalPath) '自身を再ロード
    End Sub

    ''' <summary>
    ''' 入力チェック
    ''' </summary>
    ''' <param name="workDs">画面情報データセット</param>
    ''' <param name="prevDs">遷移直後の画面情報データセット</param>
    ''' <returns></returns>
    Private Function CheckInput(workDs As DataSet, prevDs As DataSet) As Boolean
        '禁則文字の置換
        Dim invChangeCustomerField As New List(Of String) From {"REMARK"}

        ChangeInvalidChar(workDs.Tables(CONST_DT_NAME_CUSTOMERINFO), invChangeCustomerField)
        '******************************
        '単項目チェック
        '******************************
        '上部単票部分
        Dim dicCheckField As New Dictionary(Of String, TextBox) From
        {{"OUTLANGUAGE", Me.txtlang}, {"INVOICEDATE", Me.txtIssueDate}, {"REMARK", Me.txtRemarks}
         }

        Dim dr As DataRow = workDs.Tables(CONST_DT_NAME_CUSTOMERINFO).Rows(0)
        For Each singleChkItem As KeyValuePair(Of String, TextBox) In dicCheckField
            Dim fieldName As String = singleChkItem.Key
            Dim chkVal As String = Convert.ToString(dr.Item(fieldName))
            If CheckSingle(fieldName, chkVal) <> C_MESSAGENO.NORMAL Then
                singleChkItem.Value.Focus()
                Return False
            End If
        Next
        '******************************
        'リスト存在チェック
        '******************************
        Dim listCheck As New List(Of TextBox) From {Me.txtlang}
        For Each chkObj In listCheck
            '空白ならスキップ
            If chkObj.Text = "" Then
                Continue For
            End If
            Dim dicListItem As Dictionary(Of String, String) = New Dictionary(Of String, String)
            Select Case chkObj.ID
                Case "txtlang"
                    dicListItem = (From listItem In lbLanguage.Items.Cast(Of ListItem)).ToDictionary(Function(dv) dv.Value, Function(dv) dv.Text)
            End Select
            If Not dicListItem.ContainsKey(chkObj.Text) Then
                CommonFunctions.ShowMessage(C_MESSAGENO.UNSELECTABLEERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                        messageParams:=New List(Of String) From {String.Format("VALUE:{0}", chkObj.Text)})
                chkObj.Focus()
                Return False
            End If
        Next chkObj
        Return True
    End Function

    ''' <summary>
    ''' 月末日算出
    ''' </summary>
    ''' <param name="sourceDate"></param>
    ''' <returns></returns>
    Private Function LastDayOfMonth(ByVal sourceDate As Date) As Date
        Dim lastDay As DateTime = New DateTime(sourceDate.Year, sourceDate.Month, 1)
        Return lastDay.AddMonths(1).AddDays(-1)
    End Function

    ''' <summary>
    ''' 月初日算出
    ''' </summary>
    ''' <param name="sourceDate"></param>
    ''' <returns></returns>
    Private Function FirstDayOfMonth(ByVal sourceDate As Date) As Date
        Dim firstDay As Date = New Date(sourceDate.Year, sourceDate.Month, 1)
        Return firstDay
    End Function

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
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject Is Nothing Then
                        Return
                    End If
                    Dim targetTextObject As TextBox = DirectCast(targetObject, TextBox)

                    Dim taxtLabelObjects As New Dictionary(Of String, Object) _
                        From {{Me.txtlang.ID, New With {.lbl = Me.lbllangText, .list = Me.lbLanguage}}}

                    If taxtLabelObjects.ContainsKey(targetObject.ID) = False Then
                        Return
                    End If
                    Dim targetTextLabelObj As New With {.lbl = Nothing, .list = Nothing}
                    Dim typ = taxtLabelObjects(targetObject.ID).GetType
                    Dim targetLabelObj As Label = DirectCast(typ.GetProperty("lbl").GetValue(taxtLabelObjects(targetObject.ID), Nothing), Label)
                    Dim targetListboxObj As ListBox = DirectCast(typ.GetProperty("list").GetValue(taxtLabelObjects(targetObject.ID), Nothing), ListBox)
                    If targetListboxObj.SelectedItem IsNot Nothing Then
                        targetTextObject.Text = targetListboxObj.SelectedItem.Value
                        If targetLabelObj IsNot Nothing Then
                            targetLabelObj.Text = targetListboxObj.SelectedItem.Text
                        End If
                    Else
                        If targetLabelObj IsNot Nothing Then
                            targetLabelObj.Text = ""
                        End If
                    End If
            End Select
        End If
        '○ 画面左サイドボックス非表示は、画面JavaScriptで実行
        Me.hdnTextDbClickField.Value = ""
        Me.hdnIsLeftBoxOpen.Value = ""
        ClearLeftListData()
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
        ClearLeftListData()
    End Sub

    ''' <summary>
    ''' リスト行ダブルクリック時イベント
    ''' </summary>
    Private Sub ListRowDbClick()
        '※チェックボックス制御は　SaveDisplayTankList

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
        '****************************************
        '右ボックス
        '****************************************
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Remark")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")
        AddLangSetting(dicDisplayText, Me.lblRightInfo1, "ダブルクリックを行い入力を確定してください。", "Double click to confirm input.")
        AddLangSetting(dicDisplayText, Me.lblRightInfo2, "ダブルクリックを行い入力を確定してください。", "Double click to confirm input.")
        '****************************************
        ' 共通情報部分
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblInvoiceNo, "請求書番号", "Invoice No.")
        AddLangSetting(dicDisplayText, Me.lblConditionsInfo, "お支払い条件", "Payment")
        AddLangSetting(dicDisplayText, Me.lblPaymentDate, "お支払い日", "Date")
        AddLangSetting(dicDisplayText, Me.lbllang, "言語", "Language")
        AddLangSetting(dicDisplayText, Me.lblInvoicePostNo, "請求先郵便番号", "Postal Code")
        AddLangSetting(dicDisplayText, Me.lblPaymentType, "お支払方法", "Way")
        AddLangSetting(dicDisplayText, Me.lblIssueDate, "請求書発行年月日", "Invoice Date")
        AddLangSetting(dicDisplayText, Me.lblInvoiceAddress, "請求先住所", "Address")
        AddLangSetting(dicDisplayText, Me.lblBank, "振込銀行", "Bank")
        AddLangSetting(dicDisplayText, Me.lblInvoiceType, "出力タイプ", "Print")
        AddLangSetting(dicDisplayText, Me.lblDepositItem, "預金種目", "Deposit Type")
        AddLangSetting(dicDisplayText, Me.lblOutCntDraft, "ドラフト版出力数", "DRAFT OutPut")
        AddLangSetting(dicDisplayText, Me.lblAccountNo, "口座番号", "Account No.")
        AddLangSetting(dicDisplayText, Me.lblOutCntOriginal, "本紙版出力数", "ORIGINAL OutPut")
        AddLangSetting(dicDisplayText, Me.lblInvoiceName, "請求先名称", "Name")
        AddLangSetting(dicDisplayText, Me.lblAccountName, "請求先名称", "Account Name")
        AddLangSetting(dicDisplayText, Me.lblCurrency, "通貨", "Currency")
        AddLangSetting(dicDisplayText, Me.lblRemarks, "Remark", "Remark")
        AddLangSetting(dicDisplayText, Me.lblTotal, "請求額", "Amount")

        '一覧ヘッダー
        If GBT00028RValues.NewInvoiceCreate = True Then
            AddLangSetting(dicDisplayText, Me.hdnListHeaderCheck, "発行有無", "Issue")
        Else
            AddLangSetting(dicDisplayText, Me.hdnListHeaderCheck, "発行対象", "Issued")
        End If
        If GBT00028RValues.InvoiceType <> "L" Then
            AddLangSetting(dicDisplayText, Me.hdnListHeaderNo, "No.", "No.")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderOrder, "オーダー No.", "Order No.")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderTankNo, "タンク No.", "Tank No.")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderBlId, "B/L ID", "B/L ID")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderTermType, "TERM TYPE", "Term Type")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderPOL, "POL", "POL")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderPOD, "POD", "POD")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderProduct, "積載品", "Product")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderLoadDate, "LOAD", "Load")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderETD, "ETD", "ETD")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderETA, "ETA", "ETA")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderShipDate, "SHIP", "Ship")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderArvdDate, "ARVD", "Arvd")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderAmount, "AMOUNT", "Amount")
        Else
            AddLangSetting(dicDisplayText, Me.hdnListHeaderNo, "No.", "No.")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderOrder, "オーダー No.", "Order No.")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderTankNo, "タンク No.", "Tank No.")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderTankCapacity, "屯数", "Tank Capacity")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderProduct, "積載品", "Product")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderLeaseST, "自", "LeaseStart")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderLeaseEND, "至", "LeaseEnd")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderLeaseDAYS, "日数", "Days")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderUnitPrice, "単価", "Unit Price")
            AddLangSetting(dicDisplayText, Me.hdnListHeaderAmount, "金額", "Amount")
        End If

        '****************************************
        ' 各種ボタン
        '****************************************
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnExtract, "絞り込み", "Search")
        AddLangSetting(dicDisplayText, Me.btnOutputExcel, "Excel出力", "Output Excel")
        AddLangSetting(dicDisplayText, Me.btnOutput, "PDF出力", "Output PDF")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        '****************************************
        '左ボックス
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblRightListDiscription, "印刷・インポート設定", "Print/Import Settings")

        '****************************************
        ' 隠しフィールド
        '****************************************
        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub

    ''' <summary>
    ''' 遷移元（前画面）の情報を取得
    ''' </summary>
    Private Function GetPrevDisplayInfo(ByRef retDataSet As DataSet) As String

        Dim dummyList As ListBox = New ListBox
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim prevDs As DataSet = Nothing
        If TypeOf Page.PreviousPage Is GBT00028INVOICEEDIT Then
            ''自身からの遷移(Save時に反応)
            'Dim brNo As String = ""
            Dim prevPage As GBT00028INVOICEEDIT = DirectCast(Page.PreviousPage, GBT00028INVOICEEDIT)
            Me.GBT00028RValues = prevPage.GBT00028RValues
            GBT00028RValues.NewInvoiceCreate = False
            GBT00028RValues.InvoiceNo = prevPage.PrevInvoiceNo
            ViewState(CONST_VS_NAME_GBT00028RV) = prevPage.GBT00028RValues

            Dim dtCustomerInfo As DataTable = CreateCustomerInfoTable()
            Dim dtTankInfo As DataTable = CreateTankInfoTable()
            ''前画面のキー情報を元にデータをDBより取得
            dtCustomerInfo = GetCustomerInfo(dtCustomerInfo, GBT00028RValues.GBT00028SValues.CustomerCode, GBT00028RValues.GBT00028SValues.InvoiceMonth)
            If Me.GBT00028RValues.InvoiceType <> "L" Then
                dtTankInfo = GetTankListInfo(dtTankInfo, GBT00028RValues)
            Else
                dtTankInfo = GetLeaseTankListInfo(dtTankInfo, GBT00028RValues)
            End If
            retDataSet.Tables.AddRange({dtCustomerInfo, dtTankInfo})
            prevDs = retDataSet

            '保存時に自身をリダイレクト
            If prevPage.PrevMessageNo <> "" Then
                Dim naeiw As String = C_NAEIW.ABNORMAL
                If {C_MESSAGENO.NORMAL, C_MESSAGENO.NORMALDBENTRY, C_MESSAGENO.APPLYSUCCESS}.Contains(prevPage.PrevMessageNo) Then
                    naeiw = C_NAEIW.NORMAL
                End If
                CommonFunctions.ShowMessage(prevPage.PrevMessageNo, Me.lblFooterMessage, naeiw:=naeiw, pageObject:=Me)
            End If
        ElseIf TypeOf Page.PreviousPage Is GBT00028RESULT Then
            ''一覧からの遷移
            Dim prevObj As GBT00028RESULT = DirectCast(Page.PreviousPage, GBT00028RESULT)
            Me.GBT00028RValues = prevObj.ThisScreenValues
            ViewState(CONST_VS_NAME_GBT00028RV) = Me.GBT00028RValues

            Dim dtCustomerInfo As DataTable = CreateCustomerInfoTable()
            Dim dtTankInfo As DataTable = CreateTankInfoTable()
            ' 管理画面からの場合、選択行の取引先を指定
            Dim getToriCode As String = GBT00028RValues.GBT00028SValues.CustomerCode
            If GBT00028RValues.GBT00028SValues.ViewId = CONST_VIEWID_MNG Then
                getToriCode = GBT00028RValues.ToriCode
            End If

            'dtCustomerInfo = GetCustomerInfo(dtCustomerInfo, GBT00028RValues.GBT00028SValues.CustomerCode, GBT00028RValues.GBT00028SValues.InvoiceMonth)
            dtCustomerInfo = GetCustomerInfo(dtCustomerInfo, getToriCode, GBT00028RValues.GBT00028SValues.InvoiceMonth)
            If Me.GBT00028RValues.InvoiceType <> "L" Then
                dtTankInfo = GetTankListInfo(dtTankInfo, GBT00028RValues)
            Else
                dtTankInfo = GetLeaseTankListInfo(dtTankInfo, GBT00028RValues)
            End If
            retDataSet.Tables.AddRange({dtCustomerInfo, dtTankInfo})
            prevDs = retDataSet
        ElseIf Page.PreviousPage Is Nothing Then
            '単票直接呼出しパターン(JavaScriptよりPOSTした内容を取得し判定)
            Throw New Exception("No PreviousPage Error")
        End If

        ViewState(CONST_VS_NAME_PREV_VAL) = prevDs '保存前の情報
        ViewState(CONST_VS_NAME_CURRENT_VAL) = retDataSet '編集中の情報保持用
        Return retVal
    End Function

    ''' <summary>
    ''' 顧客(取引先)情報内部テーブル生成
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>1レコード前提</remarks>
    Private Function CreateCustomerInfoTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = CONST_DT_NAME_CUSTOMERINFO
        With retDt.Columns
            .Add("CUSTOMERCODE", GetType(String)).DefaultValue = ""
            .Add("INVOICEMONTH", GetType(String)).DefaultValue = ""
            .Add("INVOICENO", GetType(String)).DefaultValue = ""
            .Add("INVOICENOSUB", GetType(String)).DefaultValue = ""
            .Add("INCTORICODE", GetType(String)).DefaultValue = ""
            .Add("POSTNUM", GetType(String)).DefaultValue = ""
            .Add("ADDRESS1", GetType(String)).DefaultValue = ""
            .Add("ADDRESS2", GetType(String)).DefaultValue = ""
            .Add("ADDRESS3", GetType(String)).DefaultValue = ""
            .Add("NAMES1", GetType(String)).DefaultValue = ""
            .Add("NAMES2", GetType(String)).DefaultValue = ""
            .Add("NAMES3", GetType(String)).DefaultValue = ""
            .Add("NAMEL1", GetType(String)).DefaultValue = ""
            .Add("NAMEL2", GetType(String)).DefaultValue = ""
            .Add("NAMEL3", GetType(String)).DefaultValue = ""
            .Add("ACCCURRENCYSEGMENT", GetType(String)).DefaultValue = ""
            .Add("PAYMENTDATE", GetType(String)).DefaultValue = ""
            .Add("PAYMENTWAY", GetType(String)).DefaultValue = ""
            .Add("BANK", GetType(String)).DefaultValue = ""
            .Add("DEPOSITTYPE", GetType(String)).DefaultValue = ""
            .Add("ACCOUNTNO", GetType(String)).DefaultValue = ""
            .Add("ACCOUNTNAME", GetType(String)).DefaultValue = ""
            .Add("CURRENCY", GetType(String)).DefaultValue = ""
            .Add("OUTLANGUAGE", GetType(String)).DefaultValue = ""
            .Add("INVOICEDATE", GetType(String)).DefaultValue = ""
            .Add("DRAFTOUTPUT", GetType(String)).DefaultValue = ""
            .Add("ORIGINALOUTPUT", GetType(String)).DefaultValue = ""
            .Add("REMARK", GetType(String)).DefaultValue = ""
            .Add("AMOUNT", GetType(String)).DefaultValue = ""
            .Add("HOLIDAYFLG", GetType(String)).DefaultValue = ""
            '.Add("FIXFLG", GetType(String)).DefaultValue = ""
            .Add("WORK_C1", GetType(String)).DefaultValue = ""
        End With
        Return retDt
    End Function

    ''' <summary>
    ''' タンク情報内部テーブル生成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateTankInfoTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = CONST_DT_NAME_TANKINFO
        With retDt.Columns
            .Add("LINECNT", GetType(Integer)).DefaultValue = 0
            .Add("ORDERNO", GetType(String)).DefaultValue = ""
            .Add("TANKNO", GetType(String)).DefaultValue = ""
            .Add("BLID", GetType(String)).DefaultValue = ""
            .Add("TERMTYPE", GetType(String)).DefaultValue = ""
            .Add("POL", GetType(String)).DefaultValue = ""
            .Add("POD", GetType(String)).DefaultValue = ""
            .Add("PRODUCTNAME", GetType(String)).DefaultValue = ""
            .Add("LOADDATE", GetType(String)).DefaultValue = ""
            .Add("ETD", GetType(String)).DefaultValue = ""
            .Add("ETA", GetType(String)).DefaultValue = ""
            .Add("SHIPDATE", GetType(String)).DefaultValue = ""
            .Add("ARVDDATE", GetType(String)).DefaultValue = ""
            .Add("AMOUNT", GetType(String)).DefaultValue = ""
            .Add("EXSHIPRATE", GetType(String)).DefaultValue = ""
            .Add("EXRATE", GetType(String)).DefaultValue = ""
            '.Add("REMARK", GetType(String)).DefaultValue = ""
            '.Add("DELFLG", GetType(String)).DefaultValue = ""
            '.Add("INITYMD", GetType(String)).DefaultValue = ""
            '.Add("UPDYMD", GetType(String)).DefaultValue = ""
            '.Add("UPDUSER", GetType(String)).DefaultValue = ""
            '.Add("UPDTERMID", GetType(String)).DefaultValue = ""
            '.Add("RECEIVEYMD", GetType(String)).DefaultValue = ""
            '.Add("UPDTIMSTP", GetType(String)).DefaultValue = ""
            ''付帯情報
            .Add("BRID", GetType(String)).DefaultValue = ""
            .Add("CUSTOMER", GetType(String)).DefaultValue = ""
            '.Add("STATUS", GetType(String)).DefaultValue = ""
            'チェックボックス用
            .Add("TOINVOICE", GetType(String)).DefaultValue = ""
            'チェックボックス用
            .Add("INVOICENO", GetType(String)).DefaultValue = ""
            ''オーダー作成用TANKSEQ設定
            '.Add("TANKSEQ", GetType(String)).DefaultValue = ""
            ''LEASE情報
            .Add("TANKCAPACITY", GetType(String)).DefaultValue = ""
            .Add("LEASEST", GetType(String)).DefaultValue = ""
            .Add("LEASEEND", GetType(String)).DefaultValue = ""
            .Add("LEASEDAYS", GetType(String)).DefaultValue = ""
            .Add("UNITPRICE", GetType(String)).DefaultValue = ""
            .Add("TAXATION", GetType(String)).DefaultValue = ""
        End With
        Return retDt
    End Function
    ''' <summary>
    ''' 取得したデータセットを元に画面に展開
    ''' </summary>
    ''' <param name="ds"></param>
    Private Sub SetDispValues(ds As DataSet)
        Dim dtCustomer As DataTable = ds.Tables(CONST_DT_NAME_CUSTOMERINFO)
        Dim drCustomer As DataRow = dtCustomer.Rows(0)
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        'メイン画面上部
        '※※※※※　期日は休日補正が必要
        If Me.GBT00028RValues.InvoiceNo <> "" Then
            Me.txtInvoiceNo.Text = "No." & Mid(Me.GBT00028RValues.InvoiceNo, 1, Len(Me.GBT00028RValues.InvoiceNo) - 2)
            Me.txtInvoiceNoSub.Text = Right(Me.GBT00028RValues.InvoiceNo, 2)
        End If

        Me.txtPaymentDate.Text = GetPayDay(Convert.ToString(drCustomer.Item("PAYMENTDATE")), Convert.ToString(drCustomer.Item("HOLIDAYFLG")))
        If Convert.ToString(drCustomer.Item("OUTLANGUAGE")) <> "" Then
            Me.txtlang.Text = Convert.ToString(drCustomer.Item("OUTLANGUAGE"))
        Else
            SetVari("LANGUAGE"， Me.txtlang)
        End If
        Me.txtInvoicePostNo.Text = Convert.ToString(drCustomer.Item("POSTNUM"))
        Me.txtPaymentType.Text = Convert.ToString(drCustomer.Item("PAYMENTWAY"))
        If GBT00028RValues.NewInvoiceCreate = True Then
            Dim dt As Date
            dt = Convert.ToDateTime(GBT00028RValues.GBT00028SValues.InvoiceMonth & "/01")
            dt = dt.AddMonths(1).AddDays(-1)
            Me.txtIssueDate.Text = dt.ToString("yyyy/MM/dd")
        Else
            Me.txtIssueDate.Text = Convert.ToString(drCustomer.Item("INVOICEDATE"))
        End If
        Me.txtInvoiceAddress1.Text = Convert.ToString(drCustomer.Item("ADDRESS1"))
        Me.txtBank.Text = Convert.ToString(drCustomer.Item("BANK"))
        Me.txtInvoiceAddress2.Text = Convert.ToString(drCustomer.Item("ADDRESS2"))
        Me.txtDepositItem.Text = Convert.ToString(drCustomer.Item("DEPOSITTYPE"))
        Me.txtOutCntDraft.Text = Convert.ToString(drCustomer.Item("DRAFTOUTPUT"))
        'Me.txtInvoiceAddress3.Text = Convert.ToString(drCustomer.Item("ADDRESS3"))
        Me.txtAccountNo.Text = Convert.ToString(drCustomer.Item("ACCOUNTNO"))
        Me.txtOutCntOriginal.Text = Convert.ToString(drCustomer.Item("ORIGINALOUTPUT"))
        Me.txtInvoiceName1.Text = Convert.ToString(drCustomer.Item("NAMEL1"))
        Me.txtAccountName.Text = Convert.ToString(drCustomer.Item("ACCOUNTNAME"))
        Me.txtInvoiceName2.Text = Convert.ToString(drCustomer.Item("NAMEL2"))
        Me.txtInvoiceName3.Text = Convert.ToString(drCustomer.Item("NAMEL3"))
        'Me.txtCurrency.Text = Convert.ToString(drCustomer.Item("CURRENCY"))
        If Convert.ToString(drCustomer.Item("ACCCURRENCYSEGMENT")) = "Y" Then
            Me.txtCurrency.Text = "JPY"
        Else
            Me.txtCurrency.Text = "USD"
        End If
        Me.txtRemarks.Text = Convert.ToString(drCustomer.Item("REMARK"))
        Me.txtTotal.Text = Convert.ToString(drCustomer.Item("AMOUNT"))

        '絞り込み条件
        If GBT00028RValues.NewInvoiceCreate = True Then
            If Me.GBT00028RValues.GBT00028SValues.POL <> "" Then
                Me.txtPOL.Text = Me.GBT00028RValues.GBT00028SValues.POL
                Me.txtPOL.Enabled = False
            Else
                Me.txtPOL.Enabled = True

            End If
            If Me.GBT00028RValues.GBT00028SValues.POD <> "" Then
                Me.txtPOD.Text = Me.GBT00028RValues.GBT00028SValues.POD
                Me.txtPOD.Enabled = False
            Else
                Me.txtPOD.Enabled = True

            End If
            If Me.GBT00028RValues.GBT00028SValues.ProductCode <> "" Then
                Me.txtProduct.Text = Me.GBT00028RValues.GBT00028SValues.ProductCode
                Me.txtProduct.Enabled = False
            Else
                Me.txtProduct.Enabled = True

            End If


        End If

        If Me.GBT00028RValues.InvoiceType = "L" Then
            Dim objSearch = Me.commonInfo.FindControl("trSearch")
            If Not IsNothing(objSearch) Then
                objSearch.Visible = False
            End If
            drCustomer.Item("WORK_C1") = "L"
        End If

        '帳票種類
        If Convert.ToInt32(drCustomer.Item("ORIGINALOUTPUT")) > 0 Then
            Me.rblInvoiceTyp.SelectedValue = "ORIGINAL"
        Else
            Me.rblInvoiceTyp.SelectedValue = "DRAFT"
        End If

        'タンク一覧
        If Me.GBT00028RValues.InvoiceType <> "L" Then
            Me.repTankInfo.DataSource = dtTankInfo
            Me.repTankInfo.DataBind()
            Me.repLeaseTankInfo.Visible = False
        Else
            Me.repLeaseTankInfo.DataSource = dtTankInfo
            Me.repLeaseTankInfo.DataBind()
            Me.repTankInfo.Visible = False

            Me.txtlang.Enabled = False
        End If

        '文言設定
        txtPOL_Change()
        txtPOD_Change()
        txtProduct_Change()
        txtLangage_Change()

    End Sub

    ''' <summary>
    ''' 画面上のデータを取得し設定
    ''' </summary>
    ''' <returns>画面情報より取得したDataSet</returns>
    Private Function CollectDispValues() As DataSet
        Dim ds As DataSet = DirectCast(ViewState(CONST_VS_NAME_CURRENT_VAL), DataSet)
        With ds.Tables(CONST_DT_NAME_CUSTOMERINFO).Rows(0)
            .Item("INVOICENO") = Me.txtInvoiceNo.Text
            .Item("INVOICENOSUB") = Me.txtInvoiceNoSub.Text
            .Item("PAYMENTDATE") = Me.txtPaymentDate.Text
            .Item("OUTLANGUAGE") = Me.txtlang.Text
            .Item("POSTNUM") = Me.txtInvoicePostNo.Text
            .Item("PAYMENTWAY") = Me.txtPaymentType.Text
            .Item("INVOICEDATE") = Me.txtIssueDate.Text
            .Item("ADDRESS1") = Me.txtInvoiceAddress1.Text
            .Item("BANK") = Me.txtBank.Text
            .Item("ADDRESS2") = Me.txtInvoiceAddress2.Text
            .Item("DEPOSITTYPE") = Me.txtDepositItem.Text
            .Item("DRAFTOUTPUT") = Me.txtOutCntDraft.Text
            '.Item("ADDRESS3") = Me.txtInvoiceAddress3.Text
            .Item("ACCOUNTNO") = Me.txtAccountNo.Text
            .Item("ORIGINALOUTPUT") = Me.txtOutCntOriginal.Text
            .Item("NAMEL1") = Me.txtInvoiceName1.Text
            .Item("ACCOUNTNAME") = Me.txtAccountName.Text
            .Item("NAMEL2") = Me.txtInvoiceName2.Text
            .Item("NAMEL3") = Me.txtInvoiceName3.Text
            '.Item("CURRENCY") = Me.txtCurrency.Text
            .Item("ACCCURRENCYSEGMENT") = Me.txtCurrency.Text
            .Item("REMARK") = Me.txtRemarks.Text
            .Item("AMOUNT") = Me.txtTotal.Text
        End With
        Return ds
    End Function
    ''' <summary>
    ''' 使用可否コントロール
    ''' </summary>
    Private Sub enabledControls()

        ' 管理画面からの場合、選択行の取引先を指定
        If GBT00028RValues.GBT00028SValues.ViewId = CONST_VIEWID_MNG Then
            '使用不可制御
            Me.btnExtract.Visible = False
            Me.btnSave.Visible = False
            Me.btnOutputExcel.Visible = False
            Me.btnOutput.Visible = False
            Me.txtlang.Enabled = False
            Me.txtRemarks.Enabled = False
            Me.txtIssueDate.Enabled = False
            Me.txtPOL.Enabled = False
            Me.txtPOD.Enabled = False
            Me.txtProduct.Enabled = False
            Me.rblInvoiceTyp.Enabled = False

        End If

    End Sub

    ''' <summary>
    ''' SQLを実行し請求書情報を登録
    ''' </summary>
    ''' <param name="ds">画面上データセット</param>
    ''' <param name="prevDs">画面変更前データセット</param>
    Private Function EntryInvoice(ds As DataSet, prevDs As DataSet, saveDt As DataTable, totalInvoice As Double) As String
        Dim dtCustomer As DataTable = ds.Tables(CONST_DT_NAME_CUSTOMERINFO)
        Dim drCustomer As DataRow = dtCustomer.Rows(0)
        Dim invoiceNo As String = ""
        Try
            Dim procDate As Date = Now
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                Using sqlTran = sqlCon.BeginTransaction

                    If Me.GBT00028RValues.NewInvoiceCreate = True Then
                        invoiceNo = GetNewInvoiceNo(sqlCon, sqlTran, Convert.ToString(drCustomer.Item("INCTORICODE")), Convert.ToString(drCustomer.Item("INVOICEMONTH")))
                        InsertInvoice(invoiceNo, drCustomer, saveDt.Rows.Count, sqlCon, sqlTran, procDate)
                        EntryTankInfo(invoiceNo, saveDt, sqlCon, sqlTran, procDate)
                    Else
                        invoiceNo = GBT00028RValues.InvoiceNo
                        UpdateInvoiceInfo(invoiceNo, drCustomer, sqlCon, sqlTran, procDate, totalInvoice)
                    End If

                    sqlTran.Commit()
                End Using

            End Using
            Return invoiceNo
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' 請求書テーブル追加
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub InsertInvoice(invoiceNo As String, dr As DataRow, tankCount As Integer, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)

        Dim canCloseConnect As Boolean = False

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("INSERT INTO {0} ", CONST_TBL_INVOICEINFO).AppendLine()
            sqlStat.AppendLine("  (")
            sqlStat.AppendLine("   CUSTOMERCODE")
            sqlStat.AppendLine("  ,INVOICEMONTH")
            sqlStat.AppendLine("  ,INVOICENOSUB")
            sqlStat.AppendLine("  ,STYMD")
            sqlStat.AppendLine("  ,INVOICENO")
            sqlStat.AppendLine("  ,INCTORICODE")
            sqlStat.AppendLine("  ,REMARK")
            sqlStat.AppendLine("  ,OUTLANGUAGE")
            sqlStat.AppendLine("  ,INVOICEDATE")
            sqlStat.AppendLine("  ,DRAFTOUTPUT")
            sqlStat.AppendLine("  ,ORIGINALOUTPUT")
            sqlStat.AppendLine("  ,ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("  ,AMOUNT")
            sqlStat.AppendLine("  ,TANK")
            sqlStat.AppendLine("  ,CREATEUSER")
            sqlStat.AppendLine("  ,CREATEDATE")
            sqlStat.AppendLine("  ,WORK_C1")
            'sqlStat.AppendLine("  ,WORK_C2")
            'sqlStat.AppendLine("  ,WORK_C3")
            'sqlStat.AppendLine("  ,WORK_F1")
            'sqlStat.AppendLine("  ,WORK_F2")
            'sqlStat.AppendLine("  ,WORK_F3")
            sqlStat.AppendLine("  ,DELFLG")
            sqlStat.AppendLine("  ,INITYMD")
            sqlStat.AppendLine("  ,UPDYMD")
            sqlStat.AppendLine("  ,UPDUSER")
            sqlStat.AppendLine("  ,UPDTERMID")
            sqlStat.AppendLine("  ,RECEIVEYMD")
            sqlStat.AppendLine("  ) VALUES (")
            sqlStat.AppendLine("   @CUSTOMERCODE")
            sqlStat.AppendLine("  ,@INVOICEMONTH")
            sqlStat.AppendLine("  ,@INVOICENOSUB")
            sqlStat.AppendLine("  ,@STYMD")
            sqlStat.AppendLine("  ,@INVOICENO")
            sqlStat.AppendLine("  ,@INCTORICODE")
            sqlStat.AppendLine("  ,@REMARK")
            sqlStat.AppendLine("  ,@OUTLANGUAGE")
            sqlStat.AppendLine("  ,@INVOICEDATE")
            sqlStat.AppendLine("  ,@DRAFTOUTPUT")
            sqlStat.AppendLine("  ,@ORIGINALOUTPUT")
            sqlStat.AppendLine("  ,@ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("  ,@AMOUNT")
            sqlStat.AppendLine("  ,@TANK")
            sqlStat.AppendLine("  ,@UPDUSER")
            sqlStat.AppendLine("  ,@INITYMD")
            sqlStat.AppendLine("  ,@WORK_C1")
            'sqlStat.AppendLine("  ,@WORK_C2")
            'sqlStat.AppendLine("  ,@WORK_C3")
            'sqlStat.AppendLine("  ,@WORK_F1")
            'sqlStat.AppendLine("  ,@WORK_F2")
            'sqlStat.AppendLine("  ,@WORK_F3")
            sqlStat.AppendLine("  ,@DELFLG")
            sqlStat.AppendLine("  ,@INITYMD")
            sqlStat.AppendLine("  ,@UPDYMD")
            sqlStat.AppendLine("  ,@UPDUSER")
            sqlStat.AppendLine("  ,@UPDTERMID")
            sqlStat.AppendLine("  ,@RECEIVEYMD")
            sqlStat.AppendLine("  )")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = dr.Item("CUSTOMERCODE")
                    .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = dr.Item("INVOICEMONTH")
                    .Add("@INVOICENOSUB", SqlDbType.Int).Value = Convert.ToInt32(Right(invoiceNo, 2))
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@INVOICENO", SqlDbType.NVarChar).Value = invoiceNo
                    .Add("@INCTORICODE", SqlDbType.NVarChar).Value = dr.Item("INCTORICODE")
                    .Add("@REMARK", SqlDbType.NVarChar).Value = dr.Item("REMARK")
                    .Add("@OUTLANGUAGE", SqlDbType.NVarChar).Value = dr.Item("OUTLANGUAGE")
                    .Add("@INVOICEDATE", SqlDbType.NVarChar).Value = dr.Item("INVOICEDATE")
                    .Add("@DRAFTOUTPUT", SqlDbType.Int).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("DRAFTOUTPUT")))
                    .Add("@ORIGINALOUTPUT", SqlDbType.Int).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("ORIGINALOUTPUT")))
                    .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = dr.Item("ACCCURRENCYSEGMENT")
                    .Add("@AMOUNT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMOUNT")))
                    .Add("@TANK", SqlDbType.Int).Value = tankCount
                    .Add("@WORK_C1", SqlDbType.NVarChar).Value = dr.Item("WORK_C1")
                    '.Add("@WORK_C2", SqlDbType.NVarChar).Value = dr.Item("WORK_C2")
                    '.Add("@WORK_C3", SqlDbType.NVarChar).Value = dr.Item("WORK_C3")
                    '.Add("@WORK_F1", SqlDbType.NVarChar).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("WORK_F1")))
                    '.Add("@WORK_F2", SqlDbType.NVarChar).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("WORK_F2")))
                    '.Add("@WORK_F3", SqlDbType.NVarChar).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("WORK_F3")))
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                End With
                sqlCmd.ExecuteNonQuery()
            End Using
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
    End Sub

    ''' <summary>
    ''' 請求書テーブル更新
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub UpdateInvoiceInfo(invoiceNo As String, dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#, Optional totalInvoice As Double = 0.0)

        Dim canCloseConnect As Boolean = False

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("INSERT INTO {0} ", CONST_TBL_INVOICEINFO).AppendLine()
            sqlStat.AppendLine(" (")
            sqlStat.AppendLine("           INCTORICODE")
            sqlStat.AppendLine("          ,INVOICEMONTH")
            sqlStat.AppendLine("          ,INVOICENOSUB")
            sqlStat.AppendLine("          ,STYMD")
            sqlStat.AppendLine("          ,ENDYMD")
            sqlStat.AppendLine("          ,INVOICENO")
            sqlStat.AppendLine("          ,CUSTOMERCODE")
            sqlStat.AppendLine("          ,REMARK")
            sqlStat.AppendLine("          ,OUTLANGUAGE")
            sqlStat.AppendLine("          ,INVOICEDATE")
            sqlStat.AppendLine("          ,DRAFTOUTPUT")
            sqlStat.AppendLine("          ,ORIGINALOUTPUT")
            sqlStat.AppendLine("          ,ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("          ,AMOUNT")
            sqlStat.AppendLine("          ,INVOICEAMOUNT")
            sqlStat.AppendLine("          ,TANK")
            sqlStat.AppendLine("          ,CREATEUSER")
            sqlStat.AppendLine("          ,CREATEDATE")
            sqlStat.AppendLine("          ,WORK_C1")
            sqlStat.AppendLine("          ,WORK_C2")
            sqlStat.AppendLine("          ,WORK_C3")
            sqlStat.AppendLine("          ,WORK_F1")
            sqlStat.AppendLine("          ,WORK_F2")
            sqlStat.AppendLine("          ,WORK_F3")
            sqlStat.AppendLine("          ,DELFLG")
            sqlStat.AppendLine("          ,INITYMD")
            sqlStat.AppendLine("          ,UPDYMD")
            sqlStat.AppendLine("          ,UPDUSER")
            sqlStat.AppendLine("          ,UPDTERMID")
            sqlStat.AppendLine("          ,RECEIVEYMD")
            sqlStat.AppendLine(" )   SELECT ")
            sqlStat.AppendLine("           INCTORICODE")
            sqlStat.AppendLine("          ,INVOICEMONTH")
            sqlStat.AppendLine("          ,INVOICENOSUB")
            sqlStat.AppendLine("          ,STYMD")
            sqlStat.AppendLine("          ,ENDYMD")
            sqlStat.AppendLine("          ,INVOICENO")
            sqlStat.AppendLine("          ,CUSTOMERCODE")
            sqlStat.AppendLine("          ,@REMARK")
            sqlStat.AppendLine("          ,@OUTLANGUAGE")
            sqlStat.AppendLine("          ,@INVOICEDATE")
            sqlStat.AppendLine("          ,@DRAFTOUTPUT")
            sqlStat.AppendLine("          ,@ORIGINALOUTPUT")
            sqlStat.AppendLine("          ,ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("          ,@AMOUNT")
            If totalInvoice <> 0.0 Then
                sqlStat.AppendLine("          ,@INVOICEAMOUNT")
            Else
                sqlStat.AppendLine("          ,INVOICEAMOUNT")
            End If
            sqlStat.AppendLine("          ,TANK")
            sqlStat.AppendLine("          ,CREATEUSER")
            sqlStat.AppendLine("          ,CREATEDATE")
            sqlStat.AppendLine("          ,WORK_C1")
            sqlStat.AppendLine("          ,WORK_C2")
            sqlStat.AppendLine("          ,WORK_C3")
            sqlStat.AppendLine("          ,WORK_F1")
            sqlStat.AppendLine("          ,WORK_F2")
            sqlStat.AppendLine("          ,WORK_F3")
            sqlStat.AppendLine("          ,DELFLG")
            sqlStat.AppendLine("          ,INITYMD")
            sqlStat.AppendLine("          ,@UPDYMD")
            sqlStat.AppendLine("          ,@UPDUSER")
            sqlStat.AppendLine("          ,@UPDTERMID")
            sqlStat.AppendLine("          ,@RECEIVEYMD")
            sqlStat.AppendFormat("     FROM {0}", CONST_TBL_INVOICEINFO)
            sqlStat.AppendLine("      WHERE INCTORICODE = @INCTORICODE")
            sqlStat.AppendLine("        AND INVOICEMONTH = @INVOICEMONTH")
            sqlStat.AppendLine("        AND INVOICENOSUB = @INVOICENOSUB")
            sqlStat.AppendLine("        AND DELFLG       = @DELFLG")
            sqlStat.AppendLine(";")

            sqlStat.AppendFormat("UPDATE {0} ", CONST_TBL_INVOICEINFO).AppendLine()
            sqlStat.AppendLine("  SET")
            sqlStat.AppendLine("   DELFLG           = @DELFLG_YES")
            sqlStat.AppendLine("  ,UPDYMD           = @UPDYMD")
            sqlStat.AppendLine("  ,UPDUSER          = @UPDUSER")
            sqlStat.AppendLine("  ,UPDTERMID        = @UPDTERMID")
            sqlStat.AppendLine("  ,RECEIVEYMD       = @RECEIVEYMD")
            sqlStat.AppendLine("  WHERE INCTORICODE = @INCTORICODE")
            sqlStat.AppendLine("    AND INVOICEMONTH = @INVOICEMONTH")
            sqlStat.AppendLine("    AND INVOICENOSUB = @INVOICENOSUB")
            sqlStat.AppendLine("    AND UPDYMD      <> @UPDYMD")
            sqlStat.AppendLine("    AND DELFLG       = @DELFLG")
            sqlStat.AppendLine(";")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters

                    '.Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = dr.Item("CUSTOMERCODE")
                    .Add("@INCTORICODE", SqlDbType.NVarChar).Value = dr.Item("INCTORICODE")
                    .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = dr.Item("INVOICEMONTH")
                    .Add("@INVOICENOSUB", SqlDbType.Int).Value = Convert.ToInt32(Right(invoiceNo, 2))

                    .Add("@REMARK", SqlDbType.NVarChar).Value = dr.Item("REMARK")
                    .Add("@OUTLANGUAGE", SqlDbType.NVarChar).Value = dr.Item("OUTLANGUAGE")
                    .Add("@INVOICEDATE", SqlDbType.NVarChar).Value = dr.Item("INVOICEDATE")
                    .Add("@DRAFTOUTPUT", SqlDbType.NVarChar).Value = dr.Item("DRAFTOUTPUT")
                    .Add("@ORIGINALOUTPUT", SqlDbType.NVarChar).Value = dr.Item("ORIGINALOUTPUT")
                    .Add("@AMOUNT", SqlDbType.NVarChar).Value = dr.Item("AMOUNT")
                    .Add("@INVOICEAMOUNT", SqlDbType.NVarChar).Value = totalInvoice

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO

                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                    .Add("@DELFLG_YES", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With
                sqlCmd.ExecuteNonQuery()
            End Using
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

    End Sub

    ''' <summary>
    ''' タンク情報登録
    ''' </summary>
    ''' <param name="invoiceNo"></param>
    ''' <param name="dtTankInfo">タンク情報データテーブル</param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="procDate"></param>
    Private Sub EntryTankInfo(invoiceNo As String, dtTankInfo As DataTable, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)

        Dim canCloseConnect As Boolean = False

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If

            Dim sqlStatInsert As New StringBuilder
            sqlStatInsert.AppendFormat("  INSERT INTO {0} ( ", CONST_TBL_INVOICETANK).AppendLine()
            sqlStatInsert.AppendLine("   INVOICENO")
            sqlStatInsert.AppendLine("  ,INVOICENOSUB")
            sqlStatInsert.AppendLine("  ,ORDERNO")
            sqlStatInsert.AppendLine("  ,TANKNO")
            sqlStatInsert.AppendLine("  ,STYMD")
            sqlStatInsert.AppendLine("  ,AMOUNT")
            sqlStatInsert.AppendLine("  ,EXRATE")
            sqlStatInsert.AppendLine("  ,EXSHIPRATE")
            'sqlStatInsert.AppendLine("  ,WORK_C1")
            'sqlStatInsert.AppendLine("  ,WORK_C2")
            'sqlStatInsert.AppendLine("  ,WORK_C3")
            'sqlStatInsert.AppendLine("  ,WORK_F1")
            'sqlStatInsert.AppendLine("  ,WORK_F2")
            'sqlStatInsert.AppendLine("  ,WORK_F3")
            sqlStatInsert.AppendLine("  ,DELFLG")
            sqlStatInsert.AppendLine("  ,INITYMD")
            sqlStatInsert.AppendLine("  ,UPDYMD")
            sqlStatInsert.AppendLine("  ,UPDUSER")
            sqlStatInsert.AppendLine("  ,UPDTERMID")
            sqlStatInsert.AppendLine("  ,RECEIVEYMD")
            sqlStatInsert.AppendLine("  ) VALUES (")
            sqlStatInsert.AppendLine("      @INVOICENO")
            sqlStatInsert.AppendLine("     ,@INVOICENOSUB")
            sqlStatInsert.AppendLine("     ,@ORDERNO")
            sqlStatInsert.AppendLine("     ,@TANKNO")
            sqlStatInsert.AppendLine("     ,@STYMD")
            sqlStatInsert.AppendLine("     ,@AMOUNT")
            sqlStatInsert.AppendLine("     ,@EXRATE")
            sqlStatInsert.AppendLine("     ,@EXSHIPRATE")
            'sqlStatInsert.AppendLine("     ,@WORK_C1")
            'sqlStatInsert.AppendLine("     ,@WORK_C2")
            'sqlStatInsert.AppendLine("     ,@WORK_C3")
            'sqlStatInsert.AppendLine("     ,@WORK_F1")
            'sqlStatInsert.AppendLine("     ,@WORK_F2")
            'sqlStatInsert.AppendLine("     ,@WORK_F3")
            sqlStatInsert.AppendLine("     ,@DELFLG")
            sqlStatInsert.AppendLine("     ,@INITYMD")
            sqlStatInsert.AppendLine("     ,@UPDYMD")
            sqlStatInsert.AppendLine("     ,@UPDUSER")
            sqlStatInsert.AppendLine("     ,@UPDTERMID")
            sqlStatInsert.AppendLine("     ,@RECEIVEYMD")
            sqlStatInsert.AppendLine("  ); ")

            Using sqlCmd As New SqlCommand()
                sqlCmd.Connection = sqlCon
                sqlCmd.Transaction = tran

                '動的パラメータのみ変数化
                Dim paramOrderNo = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar)
                Dim paramTankNo = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar)
                Dim paramAmount = sqlCmd.Parameters.Add("@AMOUNT", SqlDbType.NVarChar)
                Dim paramExRate = sqlCmd.Parameters.Add("@EXRATE", SqlDbType.NVarChar)
                Dim paramExShipRate = sqlCmd.Parameters.Add("@EXSHIPRATE", SqlDbType.NVarChar)

                With sqlCmd.Parameters
                    '固定パラメータ
                    .Add("@INVOICENO", SqlDbType.NVarChar).Value = invoiceNo
                    .Add("@INVOICENOSUB", SqlDbType.NVarChar).Value = Right(invoiceNo, 2)
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@INITYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                End With

                For Each drTankInfo As DataRow In dtTankInfo.Rows
                    '登録するのはチェックされているもの
                    'If Convert.ToString(drTankInfo.Item("TOINVOICE")) = "1" Then
                    paramOrderNo.Value = drTankInfo.Item("ORDERNO")
                    paramTankNo.Value = drTankInfo.Item("TANKNO")
                    paramAmount.Value = drTankInfo.Item("AMOUNT")
                    paramExRAte.Value = drTankInfo.Item("EXRATE")
                    paramEXSHIPRATE.Value = drTankInfo.Item("EXSHIPRATE")

                    sqlCmd.CommandText = sqlStatInsert.ToString
                    sqlCmd.ExecuteNonQuery()
                    'End If
                Next 'タンク情報ループEnd
            End Using
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
    End Sub

    ''' <summary>
    ''' 左ボックスのリストデータをクリア
    ''' </summary>
    ''' <remarks>viewstateのデータ量軽減</remarks>
    Private Sub ClearLeftListData()
        'Me.lbProduct.Items.Clear()
    End Sub

    ''' <summary>
    ''' 顧客マスタ、取引先マスタテーブルよりデータを取得
    ''' </summary>
    ''' <param name="dt"></param>
    ''' <param name="customerCode"></param>
    ''' <returns></returns>
    Private Function GetCustomerInfo(dt As DataTable, customerCode As String, invoiceMonth As String) As DataTable
        Dim retDt As DataTable = dt.Clone
        Dim sqlStat As New StringBuilder

        Dim searchStr As String = ""
        If (COA0019Session.LANGDISP = C_LANG.JA) Then
            searchStr = "      ,MT.NAMESJP1 as 'NAMES1', MT.NAMESJP2 as 'NAMES2',  MT.NAMESJP3 as 'NAMES3',MT.NAMELJP1 as 'NAMEL1', MT.NAMELJP2 as 'NAMEL2', MT.NAMELJP3 as 'NAMEL3'"
            searchStr = searchStr & "      ,MT.ADDRJP1 as 'ADDRESS1', MT.ADDRJP2 as 'ADDRESS2',MT.ADDRJP3 as 'ADDRESS3'"
        Else
            searchStr = "      ,MT.NAMES1 as 'NAMES1', MT.NAMES2 as 'NAMES2', MT.NAMES3 as 'NAMES3', MT.NAMEL1 as 'NAMEL1', MT.NAMEL2 as 'NAMEL2', MT.NAMEL3 as 'NAMEL3'"
            searchStr = searchStr & "      ,MT.ADDR1 as 'ADDRESS1', MT.ADDR2 as 'ADDRESS2',MT.ADDR3 as 'ADDRESS3'"
        End If

        sqlStat.AppendLine("SELECT MC.CUSTOMERCODE,@INVOICEMONTH as INVOICEMONTH,MC.INCTORICODE")
        sqlStat.AppendLine("      ,MT.POSTNUM as 'POSTNUM'")
        sqlStat.AppendFormat("      {0}", searchStr).AppendLine()
        sqlStat.AppendLine("      ,CASE WHEN MC.DEPOSITDAY = 'LAST'")
        sqlStat.AppendLine("         THEN DATEADD(d,-1,DATEADD(m,1,DATEADD(m,MC.DEPOSITADDMM + 1,CDW.BFDATE)))")
        sqlStat.AppendLine("         ELSE CONVERT(CHAR(8),CDW.BFDATE,111) + RIGHT('00' + MC.DEPOSITDAY,2)")
        sqlStat.AppendLine("       END as 'PAYMENTDATE'")
        sqlStat.AppendLine("      ,MC.HOLIDAYFLG")
        sqlStat.AppendLine("      ,MC.ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("      ,'銀行振込' as 'PAYMENTWAY'")
        sqlStat.AppendLine("      ,MB.NAMEJP + ' ' + MB.BRANCHNAMEJP as 'BANK',MB.TYPEOFACCOUNT as 'DEPOSITTYPE'")
        sqlStat.AppendLine("      ,MB.ACCOUNTNO as 'ACCOUNTNO',MB.ACCOUNTHOLDERK as 'ACCOUNTNAME'")
        sqlStat.AppendLine("      ,MB.CURRENCYCODE as 'CURRENCY'")
        sqlStat.AppendLine("      ,ISNULL(II.OUTLANGUAGE,'') as 'OUTLANGUAGE', ISNULL(convert(char(10),II.INVOICEDATE,111),'') as 'INVOICEDATE',ISNULL(II.REMARK,'') as 'REMARK'")
        sqlStat.AppendLine("      ,ISNULL(II.DRAFTOUTPUT, '0') as 'DRAFTOUTPUT', ISNULL(II.ORIGINALOUTPUT, '0') as 'ORIGINALOUTPUT', ISNULL(II.AMOUNT, '0') as 'AMOUNT'")
        sqlStat.AppendLine("      ,ISNULL(II.WORK_C1,'') as 'WORK_C1'")
        sqlStat.AppendFormat("FROM {0} MC", CONST_TBL_CUSTOMER).AppendLine()
        sqlStat.AppendFormat("  INNER JOIN {0} MT", CONST_TBL_TORI).AppendLine()
        sqlStat.AppendLine("    ON  MT.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("   AND  MT.COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND  MT.TORIKBN     = 'I'")
        sqlStat.AppendLine("   AND  MT.TORICODE    = MC.INCTORICODE")
        sqlStat.AppendLine("   AND  MT.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("   AND  MT.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("  INNER JOIN (")
        'sqlStat.AppendLine("    SELECT @CUSTOMER as 'CUSTOMERCODE', DATEADD(M,1,CONVERT(date,MAX(CD.REPORTMONTH) + '/01')) as 'BFDATE'")
        sqlStat.AppendLine("    SELECT @CUSTOMER as 'INCTORICODE', DATEADD(M,1,CONVERT(date,MAX(CD.REPORTMONTH) + '/01')) as 'BFDATE'")
        sqlStat.AppendFormat("    FROM {0} CD", CONST_TBL_CLOSINGDAY).AppendLine()
        sqlStat.AppendLine("      WHERE  CD.COUNTRYCODE = 'JOT'")
        sqlStat.AppendLine("      AND    CD.DELFLG     <> @DELFLG ) CDW")
        'sqlStat.AppendLine("   ON  CDW.CUSTOMERCODE     = MC.CUSTOMERCODE")
        sqlStat.AppendLine("   ON  CDW.INCTORICODE      = MC.INCTORICODE")
        sqlStat.AppendFormat("          INNER JOIN {0} MB", CONST_TBL_BANK).AppendLine()
        sqlStat.AppendLine("    ON  MB.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("   AND  MB.JOTBANKCODE = MT.BANKCODE")
        sqlStat.AppendLine("   AND  MB.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("   AND  MB.ENDYMD     >= @NOWDATE")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} II", CONST_TBL_INVOICEINFO)
        sqlStat.AppendLine("      ON  II.CUSTOMERCODE  = MC.CUSTOMERCODE ")
        sqlStat.AppendLine("     AND  II.INVOICEMONTH  = @INVOICEMONTH ")
        sqlStat.AppendLine("     AND  II.INVOICENO     = @INVOICENO ")
        sqlStat.AppendLine("     AND  II.DELFLG       <> @DELFLG ")
        'sqlStat.AppendLine("WHERE MC.CUSTOMERCODE  = @CUSTOMER")
        sqlStat.AppendLine("WHERE MC.INCTORICODE   = @CUSTOMER")
        sqlStat.AppendLine("    AND MC.STYMD      <= @NOWDATE")
        sqlStat.AppendLine("    AND MC.ENDYMD     >= @NOWDATE")
        sqlStat.AppendLine("    AND MC.DELFLG     <> @DELFLG")

        Dim dtDbResult As New DataTable
        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@CUSTOMER", SqlDbType.NVarChar).Value = customerCode
                .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = invoiceMonth
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@INVOICENO", SqlDbType.NVarChar).Value = GBT00028RValues.InvoiceNo
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next

        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                writeDr.Item(colName) = readDr.Item(colName)
            Next
            retDt.Rows.Add(writeDr)
        Next

        Return retDt
    End Function

    ''' <summary>
    ''' 請求書対象タンク取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>暫定</remarks>
    Private Function GetTankListInfo(dt As DataTable, GBT00028RVALUE As GBT00028RESULT.GBT00028RValues) As DataTable

        Dim retDt As DataTable = dt.Clone
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY ORDERNO,TANKNO) As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,TBL.*")
        sqlStat.AppendLine("  FROM (")
        sqlStat.AppendLine("      SELECT ")
        sqlStat.AppendLine("      OB.ORDERNO, OV.TANKNO, OB.BLID1, ISNULL(FV.VALUE1,'') as 'TERMTYPE', OB.LOADPORT1 as 'POL', OB.DISCHARGEPORT1 as 'POD', PM.PRODUCTNAME")
        sqlStat.AppendLine("     ,ISNULL(CONVERT(CHAR(10),OVLOAD.ACTUALDATE,111),'') as 'LOADDATE', ISNULL(CONVERT(CHAR(10),OVETD.SCHEDELDATE,111),'') as 'ETD'")
        sqlStat.AppendLine("     ,ISNULL(CONVERT(CHAR(10),OVETA.SCHEDELDATE,111),'') as 'ETA'")
        sqlStat.AppendLine("     ,ISNULL(CONVERT(CHAR(10),OVSHIP.ACTUALDATE,111),'') as 'SHIPDATE', ISNULL(CONVERT(CHAR(10),OVARVD.ACTUALDATE,111),'') as 'ARVDDATE'")
        sqlStat.AppendLine("     ,OV.AMOUNTFIX as 'AMOUNT'")
        sqlStat.AppendLine("     ,ISNULL(OV2.EXSHIPRATE,0.0) as 'EXSHIPRATE'")
        sqlStat.AppendLine("     ,ISNULL(RM.EXRATE,0.0) as 'EXRATE'")
        sqlStat.AppendLine("     ,OB.BRID as 'BRID'")
        sqlStat.AppendLine("     ,OV.CONTRACTORFIX as 'CUSTOMER'")
        sqlStat.AppendLine("     ,@TOINVOICE as 'TOINVOICE'")
        sqlStat.AppendLine("     ,@INVOICENO as 'INVOICENO'")
        sqlStat.AppendFormat("  FROM {0} OV", CONST_TBL_OV)
        If GBT00028RVALUE.NewInvoiceCreate = False Then
            sqlStat.AppendFormat("  INNER JOIN {0} IT", CONST_TBL_INVOICETANK)
            sqlStat.AppendLine("      ON  IT.INVOICENO     = @INVOICENO ")
            sqlStat.AppendLine("     AND  IT.INVOICENOSUB  = @INVOICENOSUB ")
            sqlStat.AppendLine("     AND  IT.ORDERNO       = OV.ORDERNO  ")
            sqlStat.AppendLine("     AND  IT.TANKNO        = OV.TANKNO  ")
            sqlStat.AppendLine("     AND  IT.DELFLG       <> @DELFLG ")

        End If
        sqlStat.AppendFormat("  INNER JOIN {0} OB", CONST_TBL_OB)
        sqlStat.AppendLine("      ON  OB.DELFLG       <> @DELFLG ")
        sqlStat.AppendLine("     AND  OB.ORDERNO       = OV.ORDERNO ")
        If GBT00028RVALUE.NewInvoiceCreate = True Then
            If GBT00028RVALUE.GBT00028SValues.POL <> "" Then
                sqlStat.AppendLine("     AND OB.LOADPORT1 = @POL")
            End If
            If GBT00028RVALUE.GBT00028SValues.POD <> "" Then
                sqlStat.AppendLine("     AND OB.DISCHARGEPORT1 = @POD")
            End If
            If GBT00028RVALUE.GBT00028SValues.ProductCode <> "" Then
                sqlStat.AppendLine("     AND OB.PRODUCTCODE = @PRODUCT")
            End If
        End If
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} PM", CONST_TBL_PM)
        sqlStat.AppendLine("      ON  PM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  PM.PRODUCTCODE   = OB.PRODUCTCODE ")
        sqlStat.AppendLine("     AND  PM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  PM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  PM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} FV", CONST_TBL_FV)
        sqlStat.AppendLine("      ON  FV.COMPCODE      = 'Default' ")
        sqlStat.AppendLine("     AND  FV.SYSCODE       = 'GB' ")
        sqlStat.AppendLine("     AND  FV.CLASS         = 'TERM' ")
        sqlStat.AppendLine("     AND  FV.KEYCODE       = OB.TERMTYPE ")
        sqlStat.AppendLine("     AND  FV.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  FV.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  FV.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OVLOAD", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OVLOAD.ORDERNO   = OV.ORDERNO ")
        sqlStat.AppendLine("     AND  OVLOAD.TANKNO    = OV.TANKNO ")
        sqlStat.AppendLine("     AND  OVLOAD.DTLPOLPOD = 'POL1' ")
        sqlStat.AppendLine("     AND  OVLOAD.ACTIONID  = 'LOAD' ")
        sqlStat.AppendLine("     AND  OVLOAD.DELFLG   <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OVETD", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OVETD.ORDERNO    = OV.ORDERNO ")
        sqlStat.AppendLine("     AND  OVETD.TANKNO     = OV.TANKNO ")
        sqlStat.AppendLine("     AND  OVETD.DTLPOLPOD  = 'POL1' ")
        sqlStat.AppendLine("     AND  OVETD.DATEFIELD LIKE 'ETD%' ")
        sqlStat.AppendLine("     AND  OVETD.DELFLG    <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OVETA", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OVETA.ORDERNO    = OV.ORDERNO ")
        sqlStat.AppendLine("     AND  OVETA.TANKNO     = OV.TANKNO ")
        sqlStat.AppendLine("     AND  OVETA.DTLPOLPOD  = 'POD1' ")
        sqlStat.AppendLine("     AND  OVETA.DATEFIELD LIKE 'ETA%' ")
        sqlStat.AppendLine("     AND  OVETA.DELFLG    <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OVSHIP", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OVSHIP.ORDERNO   = OV.ORDERNO ")
        sqlStat.AppendLine("     AND  OVSHIP.TANKNO    = OV.TANKNO ")
        sqlStat.AppendLine("     AND  OVSHIP.DTLPOLPOD = 'POL1' ")
        sqlStat.AppendLine("     AND  OVSHIP.ACTIONID  = 'SHIP' ")
        sqlStat.AppendLine("     AND  OVSHIP.DELFLG   <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OVARVD", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OVARVD.ORDERNO   = OV.ORDERNO ")
        sqlStat.AppendLine("     AND  OVARVD.TANKNO    = OV.TANKNO ")
        sqlStat.AppendLine("     AND  OVARVD.DTLPOLPOD = 'POL1' ")
        sqlStat.AppendLine("     AND  OVARVD.ACTIONID  = 'ARVD' ")
        sqlStat.AppendLine("     AND  OVARVD.DELFLG   <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OV2", CONST_TBL_OV2)
        sqlStat.AppendLine("      ON  OV2.ORDERNO      = OV.ORDERNO  ")
        sqlStat.AppendLine("     AND  OV2.TANKSEQ      = OV.TANKSEQ  ")
        sqlStat.AppendLine("     AND  OV2.TRILATERAL   = '1' ")
        sqlStat.AppendLine("     AND  OV2.DELFLG      <> @DELFLG ")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} RM", CONST_TBL_EXRATE)
        sqlStat.AppendLine("      ON  RM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  RM.COUNTRYCODE   = 'JP' ")
        sqlStat.AppendLine("     AND  RM.CURRENCYCODE  = 'JPY' ")
        sqlStat.AppendLine("     AND  RM.TARGETYM      = @INVOICEMONTH ")
        sqlStat.AppendLine("     AND  RM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  RM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  RM.DELFLG       <> @DELFLG")
        If GBT00028RVALUE.GBT00028SValues.CustomerCode <> "" Then
            sqlStat.AppendFormat("INNER JOIN {0} MC", CONST_TBL_CUSTOMER).AppendLine()
            sqlStat.AppendLine("     ON MC.INCTORICODE   = @CUSTOMER")
            sqlStat.AppendLine("    AND MC.STYMD        <= @NOWDATE")
            sqlStat.AppendLine("    AND MC.ENDYMD       >= @NOWDATE")
            sqlStat.AppendLine("    AND MC.DELFLG       <> @DELFLG")
            sqlStat.AppendLine("    AND MC.CUSTOMERCODE  = OV.CONTRACTORFIX")
        End If
        sqlStat.AppendLine("   WHERE OV.DELFLG        <> @DELFLG ")
        sqlStat.AppendLine("     AND OV.TANKNO        <> ''")
        sqlStat.AppendLine("     AND OV.INVOICEDBY     = 'JPA00001'")
        sqlStat.AppendLine("     AND OV.COSTCODE       = 'A0001-01'")
        sqlStat.AppendLine("     AND OV.SOAAPPDATE     = '1900/01/01'")
        sqlStat.AppendLine("     AND OV.BRID        LIKE 'BT%'")
        'If GBT00028RVALUE.GBT00028SValues.CustomerCode <> "" Then
        '    sqlStat.AppendLine("     AND OV.CONTRACTORFIX = @CUSTOMER")
        'End If
        If GBT00028RVALUE.NewInvoiceCreate = True Then
            sqlStat.AppendLine("     AND NOT EXISTS (")
            sqlStat.AppendFormat("                      SELECT * FROM {0} ITW", CONST_TBL_INVOICETANK)
            sqlStat.AppendLine("                        WHERE ITW.ORDERNO = OV.ORDERNO ")
            sqlStat.AppendLine("                        AND   ITW.TANKNO  = OV.TANKNO ")
            sqlStat.AppendLine("                        AND   ITW.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("                    ) ")
        End If
        sqlStat.AppendLine(" ) TBL")
        sqlStat.AppendLine(" ORDER BY TBL.ORDERNO,TBL.TANKNO")
        Dim dtDbResult As New DataTable
        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = GBT00028RValues.GBT00028SValues.InvoiceMonth & "/01"
                .Add("@CUSTOMER", SqlDbType.NVarChar).Value = GBT00028RVALUE.GBT00028SValues.CustomerCode
                .Add("@POL", SqlDbType.NVarChar).Value = GBT00028RVALUE.GBT00028SValues.POL
                .Add("@POD", SqlDbType.NVarChar).Value = GBT00028RVALUE.GBT00028SValues.POD
                .Add("@PRODUCT", SqlDbType.NVarChar).Value = GBT00028RVALUE.GBT00028SValues.ProductCode
                If GBT00028RVALUE.NewInvoiceCreate = True Then
                    .Add("@TOINVOICE", SqlDbType.NVarChar).Value = "0"
                    .Add("@INVOICENO", SqlDbType.NVarChar).Value = ""
                Else
                    .Add("@TOINVOICE", SqlDbType.NVarChar).Value = "1"
                    .Add("@INVOICENO", SqlDbType.NVarChar).Value = GBT00028RVALUE.InvoiceNo
                    .Add("@INVOICENOSUB", SqlDbType.NVarChar).Value = Convert.ToInt32(Right(GBT00028RVALUE.InvoiceNo, 2))
                End If

            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next

        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                writeDr.Item(colName) = readDr.Item(colName)
            Next
            retDt.Rows.Add(writeDr)
        Next

        Return retDt

    End Function
    ''' <summary>
    ''' 請求書対象リースタンク取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>暫定</remarks>
    Private Function GetLeaseTankListInfo(dt As DataTable, GBT00028RVALUE As GBT00028RESULT.GBT00028RValues) As DataTable

        Dim retDt As DataTable = dt.Clone
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY ORDERNO,TANKNO) As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,TBL.*")
        sqlStat.AppendLine("  FROM (")
        sqlStat.AppendLine("      SELECT ")
        sqlStat.AppendLine("      OB.ORDERNO, OV.TANKNO, OB.BLID1, '' as 'TERMTYPE', '' as 'POL', '' as 'POD', PM.PRODUCTNAME")
        sqlStat.AppendLine("     ,'' as 'LOADDATE','' as 'ETD','' as 'ETA'")
        sqlStat.AppendLine("     ,'' as 'SHIPDATE','' as 'ARVDDATE'")
        sqlStat.AppendLine("     ,OV.AMOUNTFIX as 'AMOUNT'")
        sqlStat.AppendLine("     ,0.0 as 'EXSHIPRATE'")
        sqlStat.AppendLine("     ,0.0 as 'EXRATE'")
        sqlStat.AppendLine("     ,OB.BRID as 'BRID'")
        sqlStat.AppendLine("     ,OV.CONTRACTORFIX as 'CUSTOMER'")
        sqlStat.AppendLine("     ,CONVERT(CHAR(10),case when (LT.LEASESTYMD  > @INVOICEMONTH and LT.LEASESTYMD  < EOMONTH(@INVOICEMONTH)) then LT.LEASESTYMD  else  @INVOICEMONTH end,111) as 'LEASEST'")
        sqlStat.AppendLine("     ,CONVERT(CHAR(10),case when (LT.LEASEENDYMD > @INVOICEMONTH and LT.LEASEENDYMD < EOMONTH(@INVOICEMONTH)) then LT.LEASEENDYMD else  EOMONTH(@INVOICEMONTH) end,111) as 'LEASEEND'")
        sqlStat.AppendLine("     ,datediff(Day,case when (LT.LEASESTYMD > @INVOICEMONTH And LT.LEASESTYMD < EOMONTH(@INVOICEMONTH)) then LT.LEASESTYMD else @INVOICEMONTH end, case when LT.LEASEENDYMD between @INVOICEMONTH And EOMONTH(@INVOICEMONTH) Then LT.LEASEENDYMD else EOMONTH(@INVOICEMONTH) end) + 1 as 'LEASEDAYS'")
        sqlStat.AppendLine("     ,case when ((LT.LEASESTYMD > @INVOICEMONTH And LT.LEASESTYMD < EOMONTH(@INVOICEMONTH)) Or (LT.LEASEENDYMD > @INVOICEMONTH And LT.LEASEENDYMD < EOMONTH(@INVOICEMONTH))) then convert(decimal(16,0),round((LA.LEASEPAYMENTS * 12.0 / 365.0),0)) else LA.LEASEPAYMENTS end as 'UNITPRICE'")
        sqlStat.AppendLine("     ,OV.TAXATION as 'TAXATION'")
        sqlStat.AppendLine("     ,convert(decimal(16,1),ISNULL(TK.NOMINALCAPACITY, 0) / 1000) as 'TANKCAPACITY'")
        sqlStat.AppendLine("     ,@TOINVOICE as 'TOINVOICE'")
        sqlStat.AppendLine("     ,@INVOICENO as 'INVOICENO'")
        sqlStat.AppendFormat("  FROM {0} OV", CONST_TBL_OV)
        If GBT00028RVALUE.NewInvoiceCreate = False Then
            sqlStat.AppendFormat("  INNER JOIN {0} IT", CONST_TBL_INVOICETANK)
            sqlStat.AppendLine("      ON  IT.INVOICENO     = @INVOICENO ")
            sqlStat.AppendLine("     AND  IT.INVOICENOSUB  = @INVOICENOSUB ")
            sqlStat.AppendLine("     AND  IT.ORDERNO       = OV.ORDERNO  ")
            sqlStat.AppendLine("     AND  IT.TANKNO        = OV.TANKNO  ")
            sqlStat.AppendLine("     AND  IT.DELFLG       <> @DELFLG ")

        End If
        sqlStat.AppendFormat("  INNER JOIN {0} OB", CONST_TBL_OB)
        sqlStat.AppendLine("      ON  OB.DELFLG       <> @DELFLG ")
        sqlStat.AppendLine("     AND  OB.ORDERNO       = OV.ORDERNO ")
        sqlStat.AppendFormat("  INNER JOIN {0} LA", CONST_TBL_LA)
        sqlStat.AppendLine("      ON  LA.DELFLG       <> @DELFLG ")
        sqlStat.AppendLine("     AND  LA.AGREEMENTNO   = OV.BRID ")
        sqlStat.AppendFormat("  INNER JOIN {0} LC", CONST_TBL_LC)
        sqlStat.AppendLine("      ON  LC.DELFLG       <> @DELFLG ")
        sqlStat.AppendLine("     AND  LC.CONTRACTNO    = LA.CONTRACTNO ")
        sqlStat.AppendFormat("  INNER JOIN {0} LT", CONST_TBL_LT)
        sqlStat.AppendLine("      ON  LT.DELFLG       <> @DELFLG ")
        sqlStat.AppendLine("     AND  LT.CONTRACTNO    = LC.CONTRACTNO ")
        sqlStat.AppendLine("     AND  LT.AGREEMENTNO   = LA.AGREEMENTNO ")
        sqlStat.AppendLine("     AND  LT.TANKNO        = OV.TANKNO ")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} PM", CONST_TBL_PM)
        sqlStat.AppendLine("      ON  PM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  PM.PRODUCTCODE   = OB.PRODUCTCODE ")
        sqlStat.AppendLine("     AND  PM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  PM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  PM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} TK", CONST_TBL_TK)
        sqlStat.AppendLine("      ON  TK.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  TK.TANKNO   = OV.TANKNO ")
        sqlStat.AppendLine("     AND  TK.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  TK.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  TK.DELFLG       <> @DELFLG")
        If GBT00028RVALUE.GBT00028SValues.CustomerCode <> "" Then
            sqlStat.AppendFormat("INNER JOIN {0} MC", CONST_TBL_CUSTOMER).AppendLine()
            sqlStat.AppendLine("     ON MC.INCTORICODE   = @CUSTOMER")
            sqlStat.AppendLine("    AND MC.STYMD        <= @NOWDATE")
            sqlStat.AppendLine("    AND MC.ENDYMD       >= @NOWDATE")
            sqlStat.AppendLine("    AND MC.DELFLG       <> @DELFLG")
            sqlStat.AppendLine("    AND MC.CUSTOMERCODE  = OV.CONTRACTORFIX")
        End If
        sqlStat.AppendLine("   WHERE OV.DELFLG        <> @DELFLG ")
        sqlStat.AppendLine("     AND OV.TANKNO        <> ''")
        sqlStat.AppendLine("     AND OV.INVOICEDBY     = 'JPA00001'")
        sqlStat.AppendLine("     AND OV.COSTCODE    LIKE 'S0103%'")
        sqlStat.AppendLine("     AND ( OV.ACTUALDATE BETWEEN @INVOICEMONTH AND EOMONTH(@INVOICEMONTH) )")
        sqlStat.AppendLine("     AND OV.SOAAPPDATE     = '1900/01/01'")
        If GBT00028RVALUE.NewInvoiceCreate = True Then
            sqlStat.AppendLine("     AND NOT EXISTS (")
            sqlStat.AppendFormat("                      SELECT * FROM {0} ITW", CONST_TBL_INVOICETANK)
            sqlStat.AppendLine("                        WHERE ITW.ORDERNO = OV.ORDERNO ")
            sqlStat.AppendLine("                        AND   ITW.TANKNO  = OV.TANKNO ")
            sqlStat.AppendLine("                        AND   ITW.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("                    ) ")
        End If
        sqlStat.AppendLine(" ) TBL")
        sqlStat.AppendLine(" ORDER BY TBL.ORDERNO,TBL.TANKNO")
        Dim dtDbResult As New DataTable
        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = GBT00028RValues.GBT00028SValues.InvoiceMonth & "/01"
                .Add("@CUSTOMER", SqlDbType.NVarChar).Value = GBT00028RVALUE.GBT00028SValues.CustomerCode
                .Add("@PRODUCT", SqlDbType.NVarChar).Value = GBT00028RVALUE.GBT00028SValues.ProductCode
                If GBT00028RVALUE.NewInvoiceCreate = True Then
                    .Add("@TOINVOICE", SqlDbType.NVarChar).Value = "0"
                    .Add("@INVOICENO", SqlDbType.NVarChar).Value = ""
                Else
                    .Add("@TOINVOICE", SqlDbType.NVarChar).Value = "1"
                    .Add("@INVOICENO", SqlDbType.NVarChar).Value = GBT00028RVALUE.InvoiceNo
                    .Add("@INVOICENOSUB", SqlDbType.NVarChar).Value = Convert.ToInt32(Right(GBT00028RVALUE.InvoiceNo, 2))
                End If

            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next

        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                writeDr.Item(colName) = readDr.Item(colName)
            Next
            retDt.Rows.Add(writeDr)
        Next

        Return retDt

    End Function

    ''' <summary>
    ''' 請求書出力情報取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>暫定</remarks>
    Private Function GetOutPutInfo(invoiceNo As String, invoiceMonth As String) As DataTable

        Dim sqlStat As New StringBuilder

        Dim AddColStr As String = ""
        If (Me.txtlang.Text.ToUpper = "JP") Then
            AddColStr = "JP"
        End If

        Dim retDt As New DataTable
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY OV.ORDERNO, OV.TANKNO) As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,OV.ORDERNO ")
        sqlStat.AppendLine("      ,OV.TANKNO, OB.BLID1 as 'BLID' ")
        sqlStat.AppendLine("      ,OB.TERMTYPE ")
        sqlStat.AppendLine("      ,OB.PRODUCTCODE ")
        sqlStat.AppendLine("      ,CONVERT(CHAR(10),isnull(OVL.ACTUALDATE,''),111) as 'LOAD' ")
        sqlStat.AppendLine("      ,CONVERT(CHAR(10),isnull(OVS.SCHEDELDATE,''),111) as 'ETD' ")
        sqlStat.AppendLine("      ,CONVERT(CHAR(10),isnull(OVA.SCHEDELDATE,''),111) as 'ETA' ")
        sqlStat.AppendLine("      ,OV.AMOUNTFIX ")
        sqlStat.AppendLine("      ,OV.TAXATION ")
        sqlStat.AppendLine("      ,OV.CURRENCYCODE ")
        sqlStat.AppendLine("      ,CM.ACCCURRENCYSEGMENT ")
        sqlStat.AppendFormat("    ,TM.NAMES{0}1 as 'NAMES1' ", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMES{0}2 as 'NAMES2'", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMES{0}3 as 'NAMES3'", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMEL{0}1 as 'NAMEL1'", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMEL{0}2 as 'NAMEL2'", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMEL{0}3 as 'NAMEL3'", AddColStr)
        sqlStat.AppendFormat("    ,TM.POSTNUM{0} as 'POSTNUM'", AddColStr)
        sqlStat.AppendFormat("    ,TM.ADDR{0}1 as 'ADDR1'", AddColStr)
        sqlStat.AppendFormat("    ,TM.ADDR{0}2 as 'ADDR2'", AddColStr)
        sqlStat.AppendFormat("    ,TM.ADDR{0}3 as 'ADDR3'", AddColStr)
        sqlStat.AppendLine("      ,BM.BANKCODE ")
        sqlStat.AppendLine("      ,BM.SWIFTCODE ")
        sqlStat.AppendLine("      ,BM.NAME ")
        sqlStat.AppendLine("      ,BM.NAMEJP ")
        sqlStat.AppendLine("      ,BM.NAMEK ")
        sqlStat.AppendLine("      ,BM.BRANCHCODE ")
        sqlStat.AppendLine("      ,BM.BRANCHNAME ")
        sqlStat.AppendLine("      ,BM.BRANCHNAMEJP ")
        sqlStat.AppendLine("      ,BM.BRANCHNAMEK ")
        sqlStat.AppendLine("      ,BM.ZIPCODE ")
        sqlStat.AppendLine("      ,BM.ADDR as 'ADDR_BM' ")
        sqlStat.AppendLine("      ,BM.ADDRJP ")
        sqlStat.AppendLine("      ,BM.TEL ")
        sqlStat.AppendLine("      ,BM.TYPEOFACCOUNT ")
        sqlStat.AppendLine("      ,BM.ACCOUNTNO ")
        sqlStat.AppendLine("      ,BM.ACCOUNTHOLDER ")
        sqlStat.AppendLine("      ,BM.ACCOUNTHOLDERK ")
        sqlStat.AppendLine("      ,BM.CURRENCYCODE as 'CURRENCYCODE_BM' ")
        sqlStat.AppendLine("      ,PM.PRODUCTNAME ")
        sqlStat.AppendLine("      ,CML.NAMES as 'NAMES_POL' ")
        sqlStat.AppendLine("      ,PML.AREANAME as 'AREANAME_POL' ")
        sqlStat.AppendLine("      ,CMD.NAMES as 'NAMES_POD' ")
        sqlStat.AppendLine("      ,PMD.AREANAME as 'AREANAME_POD' ")
        sqlStat.AppendLine("      ,CM.DEPOSITDAY ")
        sqlStat.AppendLine("      ,CM.DEPOSITADDMM ")
        sqlStat.AppendLine("      ,ISNULL(EM.EXRATE,0.0) as 'EXRATE' ")
        sqlStat.AppendLine("      ,ISNULL(OV2.EXSHIPRATE,0.0) as 'EXSHIPRATE' ")
        sqlStat.AppendLine("      ,II.INVOICENO ")
        sqlStat.AppendLine("      ,II.INVOICEMONTH ")
        sqlStat.AppendLine("      ,II.REMARK ")
        sqlStat.AppendLine("      ,CONVERT(CHAR(10),II.INVOICEDATE,111) as 'INVOICEDATE' ")
        sqlStat.AppendFormat("  FROM {0} II", CONST_TBL_INVOICEINFO)
        sqlStat.AppendFormat("  INNER JOIN {0} IT", CONST_TBL_INVOICETANK)
        sqlStat.AppendLine("      ON  IT.INVOICENO     = II.INVOICENO  ")
        sqlStat.AppendLine("     AND  IT.DELFLG       <> @DELFLG ")
        sqlStat.AppendFormat("  INNER JOIN {0} OV", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OV.ORDERNO       = IT.ORDERNO  ")
        sqlStat.AppendLine("     AND  OV.TANKNO        = IT.TANKNO ")
        sqlStat.AppendLine("     AND  OV.COSTCODE      = 'A0001-01' ")
        sqlStat.AppendLine("     AND  OV.DELFLG       <> @DELFLG ")
        sqlStat.AppendFormat("  INNER JOIN {0} OB", CONST_TBL_OB)
        sqlStat.AppendLine("      ON  OB.ORDERNO       = OV.ORDERNO  ")
        sqlStat.AppendLine("     AND  OB.DELFLG       <> @DELFLG ")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OVL", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OVL.ORDERNO      = OV.ORDERNO  ")
        sqlStat.AppendLine("     AND  OVL.TANKNO       = OV.TANKNO  ")
        sqlStat.AppendLine("     AND  OVL.DTLPOLPOD    = 'POL1' ")
        'sqlStat.AppendLine("     AND  OVL.DATEFIELD    = 'FillingDate'  ")
        sqlStat.AppendLine("     AND  OVL.ACTIONID     = 'LOAD' ")
        sqlStat.AppendLine("     AND  OVL.DELFLG      <> @DELFLG ")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OVS", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OVS.ORDERNO      = OV.ORDERNO  ")
        sqlStat.AppendLine("     AND  OVS.TANKNO       = OV.TANKNO  ")
        'sqlStat.AppendLine("     AND  OVS.DATEFIELD    = 'ETD' ")
        sqlStat.AppendLine("     AND  OVS.DTLPOLPOD    = 'POL1' ")
        sqlStat.AppendLine("     AND  OVS.DATEFIELD LIKE 'ETD%' ")
        sqlStat.AppendLine("     AND  OVS.DELFLG      <> @DELFLG ")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OVA", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OVA.ORDERNO      = OV.ORDERNO  ")
        sqlStat.AppendLine("     AND  OVA.TANKNO       = OV.TANKNO  ")
        'sqlStat.AppendLine("     AND  OVA.DATEFIELD    = 'ETA' ")
        sqlStat.AppendLine("     AND  OVA.DTLPOLPOD    = 'POD1' ")
        sqlStat.AppendLine("     AND  OVA.DATEFIELD LIKE 'ETA%' ")
        sqlStat.AppendLine("     AND  OVA.DELFLG      <> @DELFLG ")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} OV2", CONST_TBL_OV2)
        sqlStat.AppendLine("      ON  OV2.ORDERNO      = OV.ORDERNO  ")
        sqlStat.AppendLine("     AND  OV2.TANKSEQ      = OV.TANKSEQ  ")
        sqlStat.AppendLine("     AND  OV2.TRILATERAL   = '1' ")
        sqlStat.AppendLine("     AND  OV2.DELFLG      <> @DELFLG ")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} CM", CONST_TBL_CUSTOMER)
        sqlStat.AppendLine("      ON  CM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  CM.CUSTOMERCODE  = OV.CONTRACTORFIX ")
        sqlStat.AppendLine("     AND  CM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  CM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  CM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} TM", CONST_TBL_TORI)
        sqlStat.AppendLine("      ON  TM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  TM.TORIKBN       = 'I' ")
        sqlStat.AppendLine("     AND  TM.TORICODE      = CM.INCTORICODE ")
        sqlStat.AppendLine("     AND  TM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  TM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  TM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} BM", CONST_TBL_BANK)
        sqlStat.AppendLine("      ON  BM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  BM.JOTBANKCODE   = TM.BANKCODE ")
        sqlStat.AppendLine("     AND  BM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  BM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  BM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} PM", CONST_TBL_PM)
        sqlStat.AppendLine("      ON  PM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  PM.PRODUCTCODE   = OB.PRODUCTCODE ")
        sqlStat.AppendLine("     AND  PM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  PM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  PM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} CML", CONST_TBL_COUNTRY)
        sqlStat.AppendLine("      ON  CML.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  CML.COUNTRYCODE  = OB.LOADCOUNTRY1 ")
        sqlStat.AppendLine("     AND  CML.STYMD       <= @NOWDATE")
        sqlStat.AppendLine("     AND  CML.ENDYMD      >= @NOWDATE")
        sqlStat.AppendLine("     AND  CML.DELFLG      <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} CMD", CONST_TBL_COUNTRY)
        sqlStat.AppendLine("      ON  CMD.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  CMD.COUNTRYCODE  = OB.DISCHARGECOUNTRY1 ")
        sqlStat.AppendLine("     AND  CMD.STYMD       <= @NOWDATE")
        sqlStat.AppendLine("     AND  CMD.ENDYMD      >= @NOWDATE")
        sqlStat.AppendLine("     AND  CMD.DELFLG      <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} PML", CONST_TBL_PORT)
        sqlStat.AppendLine("      ON  PML.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  PML.COUNTRYCODE  = OB.LOADCOUNTRY1 ")
        sqlStat.AppendLine("     AND  PML.PORTCODE     = OB.LOADPORT1 ")
        sqlStat.AppendLine("     AND  PML.STYMD       <= @NOWDATE")
        sqlStat.AppendLine("     AND  PML.ENDYMD      >= @NOWDATE")
        sqlStat.AppendLine("     AND  PML.DELFLG      <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} PMD", CONST_TBL_PORT)
        sqlStat.AppendLine("      ON  PMD.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  PMD.COUNTRYCODE  = OB.DISCHARGECOUNTRY1 ")
        sqlStat.AppendLine("     AND  PMD.PORTCODE     = OB.DISCHARGEPORT1 ")
        sqlStat.AppendLine("     AND  PMD.STYMD       <= @NOWDATE")
        sqlStat.AppendLine("     AND  PMD.ENDYMD      >= @NOWDATE")
        sqlStat.AppendLine("     AND  PMD.DELFLG      <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} EM", CONST_TBL_EXRATE)
        sqlStat.AppendLine("      ON  EM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  EM.COUNTRYCODE   = 'JP' ")
        sqlStat.AppendLine("     AND  EM.CURRENCYCODE  = 'JPY' ")
        sqlStat.AppendLine("     AND  EM.TARGETYM      = @INVOICEMONTH ")
        sqlStat.AppendLine("     AND  EM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  EM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  EM.DELFLG       <> @DELFLG")
        sqlStat.AppendLine("   WHERE II.INVOICENO      = @INVOICENO ")
        sqlStat.AppendLine("     AND II.DELFLG        <> @DELFLG")
        sqlStat.AppendLine(" ORDER BY OV.ORDERNO,OV.TANKNO")

        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@INVOICENO", SqlDbType.NVarChar).Value = invoiceNo
                .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = invoiceMonth & "/01"

            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt

    End Function

    ''' <summary>
    ''' 請求書出力情報取得（リース）
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>暫定</remarks>
    Private Function GetOutPutInfoLease(invoiceNo As String, invoiceMonth As String) As DataTable

        Dim sqlStat As New StringBuilder

        Dim AddColStr As String = ""
        If (Me.txtlang.Text.ToUpper = "JP") Then
            AddColStr = "JP"
        End If

        Dim retDt As New DataTable
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY OV.ORDERNO, OV.TANKNO) As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,OV.ORDERNO ")
        sqlStat.AppendLine("      ,OV.TANKNO")
        sqlStat.AppendLine("      ,convert(decimal(16,1),ISNULL(TK.NOMINALCAPACITY, 0))/1000 as 'TANKCAPACITY'")
        sqlStat.AppendLine("      ,OB.PRODUCTCODE ")
        sqlStat.AppendLine("      ,PM.PRODUCTNAME ")
        sqlStat.AppendLine("      ,CONVERT(CHAR(10),case when (LT.LEASESTYMD  > @INVOICEMONTH and LT.LEASESTYMD  < EOMONTH(@INVOICEMONTH)) then LT.LEASESTYMD  else  @INVOICEMONTH end,111) as 'LEASEST'")
        sqlStat.AppendLine("      ,CONVERT(CHAR(10),case when (LT.LEASEENDYMD > @INVOICEMONTH and LT.LEASEENDYMD < EOMONTH(@INVOICEMONTH)) then LT.LEASEENDYMD else  EOMONTH(@INVOICEMONTH) end,111) as 'LEASEEND'")
        sqlStat.AppendLine("      ,datediff(Day,case when (LT.LEASESTYMD > @INVOICEMONTH And LT.LEASESTYMD < EOMONTH(@INVOICEMONTH)) then LT.LEASESTYMD else @INVOICEMONTH end, case when LT.LEASEENDYMD between @INVOICEMONTH And EOMONTH(@INVOICEMONTH) Then LT.LEASEENDYMD else EOMONTH(@INVOICEMONTH) end) + 1 as 'LEASEDAYS'")
        sqlStat.AppendLine("      ,case when ((LT.LEASESTYMD > @INVOICEMONTH And LT.LEASESTYMD < EOMONTH(@INVOICEMONTH)) Or (LT.LEASEENDYMD > @INVOICEMONTH And LT.LEASEENDYMD < EOMONTH(@INVOICEMONTH))) then convert(decimal(16,0),round((LA.LEASEPAYMENTS * 12.0 / 365.0),0)) else LA.LEASEPAYMENTS end as 'UNITPRICE'")
        sqlStat.AppendLine("      ,OV.AMOUNTFIX ")
        sqlStat.AppendLine("      ,OV.TAXATION ")
        sqlStat.AppendLine("      ,OV.CURRENCYCODE ")
        sqlStat.AppendLine("      ,CM.ACCCURRENCYSEGMENT ")
        sqlStat.AppendFormat("    ,TM.NAMES{0}1 as 'NAMES1' ", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMES{0}2 as 'NAMES2'", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMES{0}3 as 'NAMES3'", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMEL{0}1 as 'NAMEL1'", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMEL{0}2 as 'NAMEL2'", AddColStr)
        sqlStat.AppendFormat("    ,TM.NAMEL{0}3 as 'NAMEL3'", AddColStr)
        sqlStat.AppendFormat("    ,TM.POSTNUM{0} as 'POSTNUM'", AddColStr)
        sqlStat.AppendFormat("    ,TM.ADDR{0}1 as 'ADDR1'", AddColStr)
        sqlStat.AppendFormat("    ,TM.ADDR{0}2 as 'ADDR2'", AddColStr)
        sqlStat.AppendFormat("    ,TM.ADDR{0}3 as 'ADDR3'", AddColStr)
        sqlStat.AppendLine("      ,BM.BANKCODE ")
        sqlStat.AppendLine("      ,BM.SWIFTCODE ")
        sqlStat.AppendLine("      ,BM.NAME ")
        sqlStat.AppendLine("      ,BM.NAMEJP ")
        sqlStat.AppendLine("      ,BM.NAMEK ")
        sqlStat.AppendLine("      ,BM.BRANCHCODE ")
        sqlStat.AppendLine("      ,BM.BRANCHNAME ")
        sqlStat.AppendLine("      ,BM.BRANCHNAMEJP ")
        sqlStat.AppendLine("      ,BM.BRANCHNAMEK ")
        sqlStat.AppendLine("      ,BM.ZIPCODE ")
        sqlStat.AppendLine("      ,BM.ADDR as 'ADDR_BM' ")
        sqlStat.AppendLine("      ,BM.ADDRJP ")
        sqlStat.AppendLine("      ,BM.TEL ")
        sqlStat.AppendLine("      ,BM.TYPEOFACCOUNT ")
        sqlStat.AppendLine("      ,BM.ACCOUNTNO ")
        sqlStat.AppendLine("      ,BM.ACCOUNTHOLDER ")
        sqlStat.AppendLine("      ,BM.ACCOUNTHOLDERK ")
        sqlStat.AppendLine("      ,BM.CURRENCYCODE as 'CURRENCYCODE_BM' ")
        sqlStat.AppendLine("      ,CM.DEPOSITDAY ")
        sqlStat.AppendLine("      ,CM.DEPOSITADDMM ")
        sqlStat.AppendLine("      ,II.INVOICENO ")
        sqlStat.AppendLine("      ,II.INVOICEMONTH ")
        sqlStat.AppendLine("      ,II.REMARK ")
        sqlStat.AppendLine("      ,CONVERT(CHAR(10),II.INVOICEDATE,111) as 'INVOICEDATE' ")
        sqlStat.AppendFormat("  FROM {0} II", CONST_TBL_INVOICEINFO)
        sqlStat.AppendFormat("  INNER JOIN {0} IT", CONST_TBL_INVOICETANK)
        sqlStat.AppendLine("      ON  IT.INVOICENO     = II.INVOICENO  ")
        sqlStat.AppendLine("     AND  IT.DELFLG       <> @DELFLG ")
        sqlStat.AppendFormat("  INNER JOIN {0} OV", CONST_TBL_OV)
        sqlStat.AppendLine("      ON  OV.ORDERNO       = IT.ORDERNO  ")
        sqlStat.AppendLine("     AND  OV.TANKNO        = IT.TANKNO ")
        sqlStat.AppendLine("     AND  OV.COSTCODE   LIKE 'S0103%' ")
        sqlStat.AppendLine("     AND  ( OV.ACTUALDATE BETWEEN CONVERT(DATE, @INVOICEMONTH) AND EOMONTH(CONVERT(DATE, @INVOICEMONTH)) )")
        sqlStat.AppendLine("     AND  OV.DELFLG       <> @DELFLG ")
        sqlStat.AppendFormat("  INNER JOIN {0} OB", CONST_TBL_OB)
        sqlStat.AppendLine("      ON  OB.ORDERNO       = OV.ORDERNO  ")
        sqlStat.AppendLine("     AND  OB.DELFLG       <> @DELFLG ")
        sqlStat.AppendFormat("  INNER JOIN {0} LA", CONST_TBL_LA)
        sqlStat.AppendLine("      ON  LA.DELFLG       <> @DELFLG ")
        sqlStat.AppendLine("     AND  LA.AGREEMENTNO   = OV.BRID ")
        sqlStat.AppendFormat("  INNER JOIN {0} LC", CONST_TBL_LC)
        sqlStat.AppendLine("      ON  LC.DELFLG       <> @DELFLG ")
        sqlStat.AppendLine("     AND  LC.CONTRACTNO    = LA.CONTRACTNO ")
        sqlStat.AppendFormat("  INNER JOIN {0} LT", CONST_TBL_LT)
        sqlStat.AppendLine("      ON  LT.DELFLG       <> @DELFLG ")
        sqlStat.AppendLine("     AND  LT.CONTRACTNO    = LC.CONTRACTNO ")
        sqlStat.AppendLine("     AND  LT.AGREEMENTNO   = LA.AGREEMENTNO ")
        sqlStat.AppendLine("     AND  LT.TANKNO        = OV.TANKNO ")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} CM", CONST_TBL_CUSTOMER)
        sqlStat.AppendLine("      ON  CM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  CM.CUSTOMERCODE  = OV.CONTRACTORFIX ")
        sqlStat.AppendLine("     AND  CM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  CM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  CM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} TM", CONST_TBL_TORI)
        sqlStat.AppendLine("      ON  TM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  TM.TORIKBN       = 'I' ")
        sqlStat.AppendLine("     AND  TM.TORICODE      = CM.INCTORICODE ")
        sqlStat.AppendLine("     AND  TM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  TM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  TM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} BM", CONST_TBL_BANK)
        sqlStat.AppendLine("      ON  BM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  BM.JOTBANKCODE   = TM.BANKCODE ")
        sqlStat.AppendLine("     AND  BM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  BM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  BM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} PM", CONST_TBL_PM)
        sqlStat.AppendLine("      ON  PM.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  PM.PRODUCTCODE   = OB.PRODUCTCODE ")
        sqlStat.AppendLine("     AND  PM.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  PM.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  PM.DELFLG       <> @DELFLG")
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} TK", CONST_TBL_TK)
        sqlStat.AppendLine("      ON  TK.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("     AND  TK.TANKNO        = OV.TANKNO ")
        sqlStat.AppendLine("     AND  TK.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("     AND  TK.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("     AND  TK.DELFLG       <> @DELFLG")
        sqlStat.AppendLine("   WHERE II.INVOICENO      = @INVOICENO ")
        sqlStat.AppendLine("     AND II.DELFLG        <> @DELFLG")
        sqlStat.AppendLine(" ORDER BY OV.ORDERNO,OV.TANKNO")

        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@INVOICENO", SqlDbType.NVarChar).Value = invoiceNo
                .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = invoiceMonth & "/01"

            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt

    End Function

    ''' <summary>
    ''' 単項目チェック処理
    ''' </summary>
    ''' <param name="mapId">IN:チェック条件のMAPID</param>
    ''' <param name="dt">IN:チェック対象のデータテーブル</param>
    ''' <param name="checkFileds">IN:チェックフィールド一覧</param>
    ''' <param name="errMessage">OUT：エラーメッセージ</param>
    ''' <param name="keyFields">IN(省略可):エラーメッセージ表示時に示すキーフィールドリスト、ここを指定した場合は「エラー内容」＋「当引数のフィールドと値」をメッセージに付与します
    ''' 省略時は付与しません</param>
    ''' <param name="keyValuePadLen">IN(省略可 省略時20):「--> [項目名] = [値]」を表示する際の項目名から=までにスペースを埋めるバイト数</param>
    ''' <returns>メッセージ番号:すべて正常時はC_MESSAGENO.NORMAL(00000) チェック異常時はC_MESSAGENO.RIGHTBIXOUT(10008)を返却</returns>
    Private Function CheckSingle(ByVal mapId As String, ByVal dt As DataTable, ByVal checkFileds As List(Of String), ByRef errMessage As String, Optional keyFields As List(Of String) = Nothing, Optional keyValuePadLen As Integer = 20) As String

        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim hasError As Boolean = False
        Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
        Dim retMessage As New StringBuilder
        'エラーメッセージ取得すら失敗した場合
        Dim getMessageErrorString As String = "エラーメッセージ({0})の取得に失敗しました。"
        If BASEDLL.COA0019Session.LANGDISP <> C_LANG.JA Then
            getMessageErrorString = "Failed To Get Error message ({0})."
        End If
        '******************************
        '引数チェック
        '******************************
        '検査対象のデータテーブルレコードが存在しない、チェックフィールドが存在しない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 OrElse checkFileds.Count = 0 Then
            Return retMessageNo
        End If
        'DataTableに本当に存在するフィールを取得
        Dim targetCheckFields As New List(Of String)
        For Each checkField As String In checkFileds
            If dt.Columns.Contains(checkField) Then
                targetCheckFields.Add(checkField)
            End If
        Next
        '検査すべきフィールドがない場合はそのまま終了
        If targetCheckFields.Count = 0 Then
            Return retMessageNo
        End If
        'DataTableに本当に存在するキーフィールドを取得
        Dim targetKeyFields As List(Of String) = Nothing
        If keyFields IsNot Nothing Then
            targetKeyFields = New List(Of String)
            For Each keyField As String In keyFields
                If dt.Columns.Contains(keyField) Then
                    targetKeyFields.Add(keyField)
                End If
            Next
            If targetKeyFields.Count = 0 Then
                targetKeyFields = Nothing
            End If
        End If

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck              '項目チェック

        'チェックごとに変わらないパラメータ設定
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = mapId

        '******************************
        'フィールド名ディクショナリ取得
        '******************************
        Dim fieldDic As New Dictionary(Of String, String)
        COA0026FieldCheck.FIELDDIC = fieldDic
        COA0026FieldCheck.COA0026getFieldList()
        fieldDic = COA0026FieldCheck.FIELDDIC
        '******************************
        '単項目チェック開始
        '******************************

        'データテーブルの行ループ開始
        For Each dr As DataRow In dt.Rows
            'チェックフィールドのループ開始

            For Each checkField In targetCheckFields
                COA0026FieldCheck.FIELD = checkField
                COA0026FieldCheck.VALUE = Convert.ToString(dr.Item(checkField))
                COA0026FieldCheck.COA0026FieldCheck()
                If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then
                    retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                    CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, dummyLabelObj, naeiw:=C_NAEIW.ERROR)

                    retMessage.AppendFormat("・{0}：{1}", fieldDic(checkField), dummyLabelObj.Text).AppendLine()

                    If targetKeyFields IsNot Nothing Then
                        For Each keyField In targetKeyFields
                            retMessage.AppendFormat("--> {0} = {1}", padRight(fieldDic(keyField), keyValuePadLen), Convert.ToString(dr.Item(keyField))).AppendLine()
                        Next
                    End If 'END targetKeyFields IsNot Nothing 

                End If 'END  COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL
            Next

            Dim stringKeyInfo As String = ""


        Next 'END For Each dr As DataRow In dt.Rows
        errMessage = retMessage.ToString
        Return retMessageNo
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
        If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
            Return C_MESSAGENO.NORMAL
        Else
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            Return COA0026FieldCheck.ERR
        End If

    End Function

    ''' <summary>
    ''' 画面データの変更チェック
    ''' </summary>
    ''' <returns>True:変更あり、False:変更なし</returns>
    Public Function IsModifiedData() As Boolean
        '画面入力情報の収集
        Dim dispDs As DataSet = CommonFunctions.DeepCopy(Me.DsDisDisplayValues)
        Dim prevDs As DataSet = DirectCast(ViewState(CONST_VS_NAME_PREV_VAL), DataSet)

        '新規作成の場合は変更ありと判定
        If Me.GBT00028RValues.InvoiceNo = "" Then
            Return True
        End If

        '変更前後の入力値を比較し変更を判定
        'データテーブル名とチェックフィールド
        Dim dicModCheck As New Dictionary(Of String, List(Of String))
        dicModCheck.Add(CONST_DT_NAME_CUSTOMERINFO,
                        New List(Of String) From {"OUTLANGUAGE", "INVOICEDATE", "REMARK"})
        For Each modCheckItem In dicModCheck
            Dim dispDt As DataTable = dispDs.Tables(modCheckItem.Key)
            Dim prevDt As DataTable = prevDs.Tables(modCheckItem.Key)
            Dim maxRowIdx As Integer = dispDt.Rows.Count - 1
            For rowIdx = 0 To maxRowIdx
                Dim dispDr As DataRow = dispDt.Rows(rowIdx)
                Dim prevDr As DataRow = prevDt.Rows(rowIdx)
                For Each fieldName In modCheckItem.Value
                    If Not dispDr(fieldName).Equals(prevDr(fieldName)) Then
                        '対象フィールドの値に変更があった場合
                        Return True
                    End If
                Next fieldName 'フィールドループ
            Next 'データテーブル行ループ
        Next modCheckItem 'チェックデータテーブルループ

        'ここまでくれば変更なし
        Return False
    End Function

    ''' <summary>
    ''' 文字左スペース埋め
    ''' </summary>
    ''' <param name="st"></param>
    ''' <param name="len"></param>
    ''' <returns></returns>
    ''' <remarks>エラー一覧で項目名称が日本語英語まちまちなので調整</remarks>
    Function padRight(ByVal st As String, ByVal len As Integer) As String
        Dim padLength As Integer = len - (System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(st) - st.Length)
        '埋められない場合はそのまま返却
        If padLength <= 0 Then
            Return st
        End If
        Return st.PadRight(len, " "c)
    End Function

    ''' <summary>
    ''' 禁則文字置換
    ''' </summary>
    ''' <param name="targetObjects">対象オブジェクト（テキストボックスリスト or データテーブル)</param>
    ''' <param name="columnList">置換対象カラム一覧(データテーブル時のみ指定)</param>
    Private Sub ChangeInvalidChar(targetObjects As Object, Optional columnList As List(Of String) = Nothing)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get
        'テキストボックスの全置換
        If TypeOf targetObjects Is List(Of TextBox) Then
            Dim targetTextboxList As List(Of TextBox) = DirectCast(targetObjects, List(Of TextBox))
            For Each targetTextbox In targetTextboxList
                With COA0008InvalidChar
                    .CHARin = targetTextbox.Text
                    .COA0008RemoveInvalidChar()
                    If .CHARin <> .CHARout Then
                        targetTextbox.Text = .CHARout
                    End If
                End With
            Next
        End If
        'データテーブルの格納値置換
        If TypeOf targetObjects Is DataTable Then
            If columnList Is Nothing OrElse columnList.Count = 0 Then
                '引数置換対象のカラムがない場合はそのまま終了
                Return
            End If
            Dim dt As DataTable = DirectCast(targetObjects, DataTable)
            'データテーブルがないまたはレコードがない場合はそのまま終了
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                Return
            End If
            '引数カラムリストのうち引数データテーブルに存在するカラムに限定
            Dim changeValueColumnList As New List(Of String)
            For Each columnName As String In columnList
                If dt.Columns.Contains(columnName) Then
                    changeValueColumnList.Add(columnName)
                End If
            Next
            'データテーブルとのカラム名マッチングの結果,
            '置換対象のカラムが存在しない場合はそのまま終了
            If changeValueColumnList.Count = 0 Then
                Return
            End If
            'データ行のループ
            For Each dr As DataRow In dt.Rows
                'カラム名のループ
                For Each columnName As String In changeValueColumnList
                    With COA0008InvalidChar
                        .CHARin = Convert.ToString(dr.Item(columnName))
                        .COA0008RemoveInvalidChar()
                        If .CHARin <> .CHARout Then
                            dr.Item(columnName) = .CHARout
                        End If
                    End With
                Next 'カラム名のループEND
            Next 'データ行のループEND

        End If
    End Sub

    ''' <summary>
    ''' 日付を変換
    ''' </summary>
    ''' <param name="dateString"></param>
    ''' <returns>変換できない場合はMinValue</returns>
    Private Function DateStringToDateTime(dateString As String) As DateTime
        Dim dateTimeDefault As DateTime = DateTime.Parse("1900/01/01 00:00:00")
        Dim tmpDateTime As DateTime
        If DateTime.TryParse(dateString, tmpDateTime) Then
            Return tmpDateTime
        Else
            Return dateTimeDefault
        End If
    End Function

    ''' <summary>
    ''' Decimal文字列を数字に変換
    ''' </summary>
    ''' <param name="dblString"></param>
    ''' <returns></returns>
    Private Function DecimalStringToDecimal(dblString As String) As Decimal
        Dim tmpDouble As Decimal = 0
        If Decimal.TryParse(dblString, tmpDouble) Then
            Return tmpDouble
        Else
            Return 0
        End If
    End Function

    ''' <summary>
    ''' USD桁数取得
    ''' </summary>
    Public Function GetDecimalPlaces(ByRef retDecPlace As Integer, ByRef retRound As String) As Boolean

        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = C_FIXVALUECLAS.USD_DECIMALPLACES
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            retDecPlace = CInt(COA0017FixValue.VALUE1.Items(0).ToString)
            retRound = COA0017FixValue.VALUE2.Items(0).ToString
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
            Return False
        End If

        Return True

    End Function

    ''' <summary>
    ''' 画面チェックボックス情報保持
    ''' </summary>
    Private Sub SaveDisplayTankList()

        Dim dsc As DataSet = Me.DsDisDisplayValues
        Dim dtCustomer As DataTable = dsc.Tables(CONST_DT_NAME_CUSTOMERINFO)
        Dim drCustomer As DataRow = dtCustomer.Rows(0)
        Dim amountTotal As Double = 0.0
        Dim ds As DataSet = DirectCast(ViewState(CONST_VS_NAME_CURRENT_VAL), DataSet)
        Dim dtTankInfo As DataTable = ds.Tables(CONST_DT_NAME_TANKINFO)
        Dim repTmpTankInfo As Repeater
        If Me.GBT00028RValues.InvoiceType <> "L" Then
            repTmpTankInfo = repTankInfo
        Else
            repTmpTankInfo = repLeaseTankInfo
        End If
        If dtTankInfo.Rows.Count = 0 OrElse repTmpTankInfo.Items Is Nothing OrElse repTmpTankInfo.Items.Count = 0 Then
            Return
        End If
        For Each repItem As RepeaterItem In repTmpTankInfo.Items
            Dim lblLineCntObj As Label = DirectCast(repItem.FindControl("lblLineCnt"), Label)
            Dim chkToInvoice As CheckBox = DirectCast(repItem.FindControl("chkToInvoice"), CheckBox)

            Dim targetRow = (From rowItem In dtTankInfo Where Convert.ToString(rowItem("LINECNT")) = lblLineCntObj.Text)

            'End If
            If targetRow.Any = True Then
                With targetRow(0)
                    ' 全チェック
                    If Me.hdnAllSelectCheckChange.Value = "TRUE" Then
                        If Me.hdnAllSelectCheckValue.Value.ToUpper = "TRUE" Then
                            chkToInvoice.Checked = True
                        Else
                            chkToInvoice.Checked = False
                        End If
                    End If
                    ' オーダー指定
                    If Me.hdnListDBclick.Value <> "" Then
                        If .Item("ORDERNO").ToString.Equals(Me.hdnListDBclick.Value) Then
                            chkToInvoice.Checked = True
                        End If
                    End If
                    If chkToInvoice.Checked Then
                        .Item("TOINVOICE") = "1"
                        amountTotal = amountTotal + Convert.ToDecimal(.Item("AMOUNT"))
                    Else
                        .Item("TOINVOICE") = ""
                    End If
                End With
            End If
        Next
        drCustomer.Item("AMOUNT") = amountTotal
        Me.txtTotal.Text = amountTotal.ToString("#,##0.00")
        Me.hdnAllSelectCheckChange.Value = "FALSE"
        ViewState(CONST_VS_NAME_CURRENT_VAL) = ds

    End Sub

    ''' <summary>
    ''' Fixvalueを元にリストボックスを作成
    ''' </summary>
    ''' <param name="className"></param>
    ''' <param name="targetList"></param>
    ''' <remarks>動的要素ではなくロード時に設定で済むもののみに絞ること</remarks>
    Private Sub SetFixvalueListItem(className As String, targetList As ListBox)
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Dim jpList As New ListBox
        Dim engList As New ListBox
        targetList.Items.Clear()
        'Term選択肢
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = className
        COA0017FixValue.LISTBOX1 = jpList
        COA0017FixValue.LISTBOX2 = engList
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                targetList.Items.AddRange(jpList.Items.Cast(Of ListItem).ToArray)
            Else
                targetList.Items.AddRange(engList.Items.Cast(Of ListItem).ToArray)
            End If
        Else
            Throw New Exception("Fix value getError")
        End If
    End Sub

    ''' <summary>
    ''' COA0016VARIgetを使用し初期値を取得設定
    ''' </summary>
    ''' <param name="KeyString"></param>
    ''' <param name="textField"></param>
    Private Sub SetVari(KeyString As String, textField As TextBox)
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget        '変数情報取

        Dim dicDefaultValueSettings As New Dictionary(Of String, TextBox) _
                        From {{KeyString, textField}}
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = "Default"
        '上記初期設定を行うディクショナリのループ
        For Each item As KeyValuePair(Of String, TextBox) In dicDefaultValueSettings

            COA0016VARIget.FIELD = item.Key
            COA0016VARIget.COA0016VARIget()
            If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
                item.Value.Text = COA0016VARIget.VALUE
            Else
                CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0016VARIget.ERR)})
                Return
            End If
        Next

    End Sub

    ''' <summary>
    ''' 期日設定
    ''' </summary>
    Private Function GetPayDay(targetDay As String, holidayFlg As String) As String
        Dim retVal As String = ""   '戻り値用のString
        Dim retDt As New DataTable

        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("WITH DateTable (MyDate, Part) ")
        sqlStat.AppendLine("  AS( ")
        sqlStat.AppendLine("      SELECT	(DATEADD(dd, 1, EOMONTH (@TargetDate , -2))),datepart(weekday,DATEADD(dd, 1, EOMONTH (@TargetDate , -2))) ")
        sqlStat.AppendLine("      UNION ALL")
        sqlStat.AppendLine("      SELECT	DATEADD(dd, 1, MyDate),datepart(weekday,DATEADD(dd, 1, MyDate)) ")
        sqlStat.AppendLine("      FROM   DateTable ")
        sqlStat.AppendLine("      WHERE  MyDate <EOMONTH(@TargetDate, 1) ")
        sqlStat.AppendLine("    ), WorkCalender as ( ")
        sqlStat.AppendLine("      SELECT   d.MyDate,d.Part,isnull(h.HOLYDAY_NAME,'') as holidayname, ")
        sqlStat.AppendLine("      case when d.Part = '1' or d.Part = '7' or isnull(h.HOLYDAY_NAME,'') <> '' then '1' ")
        sqlStat.AppendLine("      else '0' end as holydayflg ")
        sqlStat.AppendLine("      FROM     DateTable d ")
        sqlStat.AppendLine("      left join GBM0026_HOLYDAYS h ")
        sqlStat.AppendLine("      on h.HOLYDAY_DATE = d.MyDate ")
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
            Return targetDay

        End If
        sqlStat.AppendLine("  and   w.holydayflg <> '1' ")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramTargetDate As SqlParameter = sqlCmd.Parameters.Add("@TargetDate", SqlDbType.NVarChar)
            'SQLパラメータ値セット
            paramTargetDate.Value = targetDay
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
    ''' 当画面の値引き渡しクラス
    ''' </summary>
    <Serializable>
    Public Class GBT00028INVOICEEDITDispItem
        Public PrevDispItem As GBT00028RESULT.GBT00028RValues = Nothing
        Public DispDs As DataSet = Nothing
        Public PrevDispDs As DataSet = Nothing
        Public MapVari As String = ""
        Public OrderStartPoint As String = ""
        ''' <summary>
        ''' タンクステータスにて選択されたタンク一覧を保持
        ''' </summary>
        Public SelectedTankNo As List(Of String) = New List(Of String)
    End Class

    ''' <summary>
    ''' 請求書No取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetNewInvoiceNo(Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing,
                                            Optional toriCode As String = "", Optional invoiceMonth As String = "") As String
        Dim canCloseConnect As Boolean = False
        Dim invoiceNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If

            Dim sqlStat As New Text.StringBuilder
            sqlStat.AppendLine("SELECT SUBSTRING(TRIM(MC.TORICODE),1,LEN(TRIM(MC.TORICODE))-1) + '-' ")
            sqlStat.AppendLine("      + RIGHT(REPLACE(@INVOICEMONTH,'/',''),4) + '-' + TRIM(FV.VALUE1) ")
            sqlStat.AppendLine("      + CASE WHEN ISNULL(II0.INVOICENOSUB,9999) = 9999 THEN '00' ELSE RIGHT('00' + TRIM(convert(char,ISNULL(INS.INVOCENOSUB,0))),2) END ")
            'sqlStat.AppendFormat("FROM {0} MC", CONST_TBL_CUSTOMER)
            sqlStat.AppendLine("FROM (SELECT @TORICODE AS TORICODE) MC")
            sqlStat.AppendFormat("  INNER JOIN {0} FV", CONST_TBL_FV)
            sqlStat.AppendLine("      ON  FV.COMPCODE      = 'Default' ")
            sqlStat.AppendLine("     AND  FV.SYSCODE       = 'GB' ")
            sqlStat.AppendLine("     AND  FV.CLASS         = 'INV_NO' ")
            sqlStat.AppendLine("     AND  FV.KEYCODE       = 'CODE' ")
            sqlStat.AppendLine("     AND  FV.STYMD        <= @NOWDATE")
            sqlStat.AppendLine("     AND  FV.ENDYMD       >= @NOWDATE")
            sqlStat.AppendLine("     AND  FV.DELFLG       <> @DELFLG")
            sqlStat.AppendLine("  LEFT OUTER JOIN ( ")
            sqlStat.AppendLine("                    SELECT @TORICODE as TORICODE, MIN( II.INVOICENOSUB + 1 ) AS INVOCENOSUB ")
            sqlStat.AppendFormat("                      FROM {0} II", CONST_TBL_INVOICEINFO)
            sqlStat.AppendLine("                      WHERE II.INCTORICODE = @TORICODE ")
            sqlStat.AppendLine("                      AND   II.INVOICEMONTH = @INVOICEMONTH ")
            sqlStat.AppendLine("                      AND  ( II.INVOICENOSUB + 1 ) NOT IN ( ")
            sqlStat.AppendFormat("                                                          SELECT IIW.INVOICENOSUB FROM {0} IIW", CONST_TBL_INVOICEINFO)
            sqlStat.AppendLine("                                                              WHERE IIW.INCTORICODE = @TORICODE ")
            sqlStat.AppendLine("                                                              AND   IIW.INVOICEMONTH = @INVOICEMONTH ")
            sqlStat.AppendLine("                                                              AND   IIW.STYMD       <= @NOWDATE ")
            sqlStat.AppendLine("                                                              AND   IIW.ENDYMD      >= @NOWDATE ")
            sqlStat.AppendLine("                                                              AND   IIW.DELFLG      <> @DELFLG ")
            sqlStat.AppendLine("                                                          ) ")
            sqlStat.AppendLine("                      AND   II.STYMD       <= @NOWDATE ")
            sqlStat.AppendLine("                      AND   II.ENDYMD      >= @NOWDATE ")
            sqlStat.AppendLine("                      AND   II.DELFLG      <> @DELFLG ")
            sqlStat.AppendLine("                  ) INS ")
            sqlStat.AppendLine("    ON INS.TORICODE = MC.TORICODE ")
            sqlStat.AppendFormat("  LEFT OUTER JOIN {0} II0", CONST_TBL_INVOICEINFO)
            sqlStat.AppendLine("     ON    II0.INCTORICODE = @TORICODE ")
            sqlStat.AppendLine("     AND   II0.INVOICEMONTH = @INVOICEMONTH ")
            sqlStat.AppendLine("     AND   II0.INVOICENOSUB = 0 ")
            sqlStat.AppendLine("     AND   II0.STYMD       <= @NOWDATE ")
            sqlStat.AppendLine("     AND   II0.ENDYMD      >= @NOWDATE ")
            sqlStat.AppendLine("     AND   II0.DELFLG      <> @DELFLG ")
            'sqlStat.AppendLine("WHERE MC.COMPCODE       = @COMPCODE")
            'sqlStat.AppendLine("AND   MC.CUSTOMERCODE   = @CUSTOMERCODE")
            'sqlStat.AppendLine("AND   MC.STYMD         <= @NOWDATE")
            'sqlStat.AppendLine("AND   MC.ENDYMD        >= @NOWDATE")
            'sqlStat.AppendLine("AND   MC.DELFLG        <> @DELFLG")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                    .Add("@NOWDATE", SqlDbType.Date).Value = Now
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                    '.Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = customerCode
                    .Add("@TORICODE", SqlDbType.NVarChar).Value = toriCode
                    .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = invoiceMonth
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get GetNewInvoiceNo error")
                    End If

                    invoiceNo = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return invoiceNo
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
    End Function

    ''' <summary>
    ''' POLリストアイテムを設定
    ''' </summary>
    Private Function SetPOLListItem(selectedValue As String) As String

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
            sqlStat.AppendLine("  where ov.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  and   ov.INVOICEDBY = 'JPA00001' ")
            sqlStat.AppendLine("  and   ov.COSTCODE = 'A0001-01' ")
            sqlStat.AppendLine("  and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("  and   ov.BRID like 'BT%' ")

            ' 指定済み
            If GBT00028RValues.GBT00028SValues.CustomerCode <> "" Then
                sqlStat.AppendLine("  and   ov.CONTRACTORFIX = @CUSTOMER ")
            End If
            If GBT00028RValues.GBT00028SValues.POL <> "" Then
                sqlStat.AppendLine("  and   ob.LOADPORT1 = @POL ")
            End If
            If Me.txtPOD.Text <> "" Then
                sqlStat.AppendLine("  and   ob.DISCHARGEPORT1 = @POD ")
            End If
            If Me.txtProduct.Text <> "" Then
                sqlStat.AppendLine("  and   ob.PRODUCTCODE = @PRODUCT ")
            End If
            If GBT00028RValues.NewInvoiceCreate = True Then
                sqlStat.AppendLine("     AND NOT EXISTS (")
                sqlStat.AppendFormat("                      SELECT * FROM {0} ITW", CONST_TBL_INVOICETANK)
                sqlStat.AppendLine("                        WHERE ITW.ORDERNO = ov.ORDERNO ")
                sqlStat.AppendLine("                        AND   ITW.TANKNO  = ov.TANKNO ")
                sqlStat.AppendLine("                        AND   ITW.DELFLG <> @DELFLG ")
                sqlStat.AppendLine("                    ) ")
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
                    If GBT00028RValues.GBT00028SValues.CustomerCode <> "" Then
                        .Add("@CUSTOMER", System.Data.SqlDbType.NVarChar).Value = GBT00028RValues.GBT00028SValues.CustomerCode
                    End If
                    If GBT00028RValues.GBT00028SValues.POL <> "" Then
                        .Add("@POL", System.Data.SqlDbType.NVarChar).Value = GBT00028RValues.GBT00028SValues.POL
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
                'Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '    While SQLdr.Read
                '        Me.lbPOL.Items.Add(Convert.ToString(SQLdr("POL")))
                '    End While
                'End Using
                If retDt IsNot Nothing Then
                    'Me.ERR = C_MESSAGENO.NODATA
                    'Return
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
            Return C_MESSAGENO.NORMAL

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
            Return C_MESSAGENO.EXCEPTION
        End Try

    End Function

    ''' <summary>
    ''' POL名設定
    ''' </summary>
    Public Sub txtPOL_Change()

        Try
            Me.lblPOLText.Text = ""
            Dim returnCode As String = String.Empty

            returnCode = SetPOLListItem(Me.txtPOL.Text)
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
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try

    End Sub

    ''' <summary>
    ''' PODリストアイテムを設定
    ''' </summary>
    Private Function SetPODListItem(selectedValue As String) As String

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
            sqlStat.AppendLine("  where ov.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  and   ov.INVOICEDBY = 'JPA00001' ")
            sqlStat.AppendLine("  and   ov.COSTCODE = 'A0001-01' ")
            sqlStat.AppendLine("  and   ov.SOAAPPDATE = '1900/01/01' ")
            sqlStat.AppendLine("  and   ov.BRID like 'BT%' ")

            ' 指定済み
            If GBT00028RValues.GBT00028SValues.CustomerCode <> "" Then
                sqlStat.AppendLine("  and   ov.CONTRACTORFIX = @CUSTOMER ")
            End If
            If Me.txtPOL.Text <> "" Then
                sqlStat.AppendLine("  and   ob.LOADPORT1 = @POL ")
            End If
            If GBT00028RValues.GBT00028SValues.POD <> "" Then
                sqlStat.AppendLine("  and   ob.DISCHARGEPORT1 = @POD ")
            End If
            If Me.txtProduct.Text <> "" Then
                sqlStat.AppendLine("  and   ob.PRODUCTCODE = @PRODUCT ")
            End If
            If GBT00028RValues.NewInvoiceCreate = True Then
                sqlStat.AppendLine("     AND NOT EXISTS (")
                sqlStat.AppendFormat("                      SELECT * FROM {0} ITW", CONST_TBL_INVOICETANK)
                sqlStat.AppendLine("                        WHERE ITW.ORDERNO = ov.ORDERNO ")
                sqlStat.AppendLine("                        AND   ITW.TANKNO  = ov.TANKNO ")
                sqlStat.AppendLine("                        AND   ITW.DELFLG <> @DELFLG ")
                sqlStat.AppendLine("                    ) ")
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
                    If GBT00028RValues.GBT00028SValues.CustomerCode <> "" Then
                        .Add("@CUSTOMER", System.Data.SqlDbType.NVarChar).Value = GBT00028RValues.GBT00028SValues.CustomerCode
                    End If
                    If Me.txtPOL.Text <> "" Then
                        .Add("@POL", System.Data.SqlDbType.NVarChar).Value = Me.txtPOL.Text
                    End If
                    If GBT00028RValues.GBT00028SValues.POD <> "" Then
                        .Add("@POD", System.Data.SqlDbType.NVarChar).Value = GBT00028RValues.GBT00028SValues.POD
                    End If
                    If Me.txtProduct.Text <> "" Then
                        .Add("@PRODUCT", System.Data.SqlDbType.NVarChar).Value = Me.txtProduct.Text
                    End If
                End With

                Using sqlDa As New SqlDataAdapter(SQLcmd)
                    sqlDa.Fill(retDt)
                End Using 'sqlDa
                'Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '    While SQLdr.Read
                '        Me.lbPOD.Items.Add(Convert.ToString(SQLdr("POD")))
                '    End While
                'End Using
                If retDt IsNot Nothing Then
                    'Me.ERR = C_MESSAGENO.NODATA
                    'Return
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
            Return C_MESSAGENO.NORMAL

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
            Return C_MESSAGENO.EXCEPTION
        End Try

    End Function

    ''' <summary>
    ''' POD名設定
    ''' </summary>
    Public Sub txtPOD_Change()

        Try
            Me.lblPODText.Text = ""
            Dim returnCode As String = String.Empty

            returnCode = SetPODListItem(Me.txtPOD.Text)
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
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try

    End Sub

    ''' <summary>
    ''' Productリストアイテムを設定
    ''' </summary>
    Private Function SetProductListItem(selectedValue As String) As String

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
            sqlStat.AppendLine("  where ov.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  and   ov.INVOICEDBY = 'JPA00001' ")
            If Me.GBT00028RValues.InvoiceType <> "L" Then
                sqlStat.AppendLine("  and   ov.COSTCODE = 'A0001-01' ")
                sqlStat.AppendLine("  and   ov.SOAAPPDATE = '1900/01/01' ")
                sqlStat.AppendLine("  and   ov.BRID like 'BT%' ")
            Else
                sqlStat.AppendLine("  and   ov.COSTCODE LIKE 'S0103%' ")
                sqlStat.AppendLine("  and   ov.SOAAPPDATE = '1900/01/01' ")
            End If

            ' 指定済み
            If GBT00028RValues.GBT00028SValues.CustomerCode <> "" Then
                '               sqlStat.AppendLine("  and   ov.CONTRACTORFIX = @CUSTOMER ")
            End If
            If Me.txtPOL.Text <> "" Then
                sqlStat.AppendLine("  and   ob.LOADPORT1 = @POL ")
            End If
            If Me.txtPOD.Text <> "" Then
                sqlStat.AppendLine("  and   ob.DISCHARGEPORT1 = @POD ")
            End If
            If GBT00028RValues.GBT00028SValues.ProductCode <> "" Then
                sqlStat.AppendLine("  and   ob.PRODUCTCODE = @PRODUCT ")
            End If
            If GBT00028RValues.NewInvoiceCreate = True Then
                sqlStat.AppendLine("     AND NOT EXISTS (")
                sqlStat.AppendFormat("                      SELECT * FROM {0} ITW", CONST_TBL_INVOICETANK)
                sqlStat.AppendLine("                        WHERE ITW.ORDERNO = ov.ORDERNO ")
                sqlStat.AppendLine("                        AND   ITW.TANKNO  = ov.TANKNO ")
                sqlStat.AppendLine("                        AND   ITW.DELFLG <> @DELFLG ")
                sqlStat.AppendLine("                    ) ")
            End If

            sqlStat.AppendLine(" ORDER BY ob.PRODUCTCODE, pm.PRODUCTNAME, ob.PRODUCTCODE + ':' + pm.PRODUCTNAME ")
            Dim retDt As New DataTable
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    If GBT00028RValues.GBT00028SValues.CustomerCode <> "" Then
                        .Add("@CUSTOMER", System.Data.SqlDbType.NVarChar).Value = GBT00028RValues.GBT00028SValues.CustomerCode
                    End If
                    If Me.txtPOL.Text <> "" Then
                        .Add("@POL", System.Data.SqlDbType.NVarChar).Value = Me.txtPOL.Text
                    End If
                    If Me.txtPOD.Text <> "" Then
                        .Add("@POD", System.Data.SqlDbType.NVarChar).Value = Me.txtPOD.Text
                    End If
                    If GBT00028RValues.GBT00028SValues.ProductCode <> "" Then
                        .Add("@PRODUCT", System.Data.SqlDbType.NVarChar).Value = GBT00028RValues.GBT00028SValues.ProductCode
                    End If
                End With

                Using sqlDa As New SqlDataAdapter(SQLcmd)
                    sqlDa.Fill(retDt)
                End Using 'sqlDa
                'Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                '    While SQLdr.Read
                '        Me.lbProduct.Items.Add(Convert.ToString(SQLdr("PRODUCT")))
                '    End While
                'End Using
                If retDt IsNot Nothing Then
                    'Me.ERR = C_MESSAGENO.NODATA
                    'Return
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
            Return C_MESSAGENO.NORMAL

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
            Return C_MESSAGENO.EXCEPTION
        End Try

    End Function

    ''' <summary>
    ''' Product名設定
    ''' </summary>
    Public Sub txtProduct_Change()

        Try
            Me.lblProductText.Text = ""
            Dim returnCode As String = String.Empty

            returnCode = SetProductListItem(Me.txtProduct.Text)
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
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try

    End Sub

    ''' <summary>
    ''' Langage名設定
    ''' </summary>
    Public Sub txtLangage_Change()

        Try
            Me.lbllangText.Text = ""
            'Dim returnCode As String = String.Empty

            'returnCode = SetProductListItem(Me.txtlang.Text)
            'If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbLanguage.Items.Count > 0 Then
            If Me.lbLanguage.Items.Count > 0 Then
                Dim findListItem = Me.lbLanguage.Items.FindByValue(Me.txtlang.Text)
                If findListItem IsNot Nothing Then
                    Me.lbllangText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbLanguage.Items.FindByValue(Me.txtlang.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lbllangText.Text = findListItemUpper.Text
                        Me.txtlang.Text = findListItemUpper.Value
                    End If
                End If
            End If

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try

    End Sub

End Class

