Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' リース一覧
''' </summary>
Public Class GBT00028RESULT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00028R" '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00028L" '次画面一覧のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_INITDATE As String = "1900/01/01" 'DB日付初期値

    Private Const CONST_TBL_INVOICEINFO As String = "GBT0016_INVOICE_INFO"
    Private Const CONST_TBL_INVOICETANK As String = "GBT0017_INVOICE_TANKINFO"
    Private Const CONST_TBL_USER As String = "COS0005_USER"
    Private Const CONST_TBL_CUSTOMER As String = "GBM0004_CUSTOMER"
    Private Const CONST_TBL_TORI As String = "GBM0025_TORI"
    Private Const CONST_TBL_APPLY As String = "COS0022_APPROVAL"
    Private Const CONST_TBL_OV As String = "GBT0005_ODR_VALUE"
    Private Const CONST_TBL_OB As String = "GBT0004_ODR_BASE"
    Private Const CONST_TBL_LC As String = "GBT0010_LBR_CONTRACT"
    Private Const CONST_TBL_LA As String = "GBT0011_LBR_AGREEMENT"
    Private Const CONST_TBL_LT As String = "GBT0012_RESRVLEASETANK"

    '承認イベント
    Private Const CONST_EVENT_APPLY As String = "INVOICE_Apply"

    Private Const CONST_CHECK_ON As String = "1"
    Private Const CONST_CHECK_OFF As String = "0"

    '請求書タイプ
    Private Const CONST_REPTYPE_FVCLASS As String = "INV_REPORTCONF_MNG"
    Private Const CONST_REPTYPE_MNG As String = "InvoiceManagement"

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 前画面(検索条件保持用)
    ''' </summary>
    Public Property GBT00028SValues As GBT00028SELECT.GBT00028SValues
    ''' <summary>
    ''' 当画面情報保持
    ''' </summary>
    ''' <returns></returns>
    Public Property ThisScreenValues As GBT00028RESULT.GBT00028RValues
    ''' <summary>
    ''' ページロード時
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
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, Me.hdnThisMapVariant.Value)
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
                '****************************************
                '表示条件ラジオボタンの設定
                '****************************************
                '右ボックス帳票タブ
                Dim errMsg As String = ""
                'errMsg = Me.RightboxInit()
                '****************************************
                'テンプレート情報取得
                '****************************************
                Dim item As New List(Of String) From {"BreakerTotal", "Lease"}
                Me.repInvoiceNew.DataSource = item
                Me.repInvoiceNew.DataBind()
                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetInvoiceListDataTable()
                    'グリッド用データをファイルに退避
                    With Nothing
                        Dim COA0021ListTable As New COA0021ListTable
                        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
                        COA0021ListTable.TBLDATA = dt
                        COA0021ListTable.COA0021saveListTable()
                        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                            Return
                        End If
                    End With

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Dim listVari As String = Me.GBT00028SValues.ViewId
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = Me.hdnThisMapVariant.Value
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
                        .LEVENT = "ondblclick"
                        .LFUNC = "ListDbClick"
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .NOCOLUMNWIDTHOPT = 50
                        .TITLEOPT = True
                        .USERSORTOPT = 0
                    End With
                    COA0013TableObject.COA0013SetTableObject()

                End Using 'DataTable
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                Me.GBT00028SValues = DirectCast(ViewState("GBT00028SValues"), GBT00028SELECT.GBT00028SValues)
                '画面の入力情報を保持
                Dim messageNo As String = FileSaveDisplayInput()
                If messageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                                messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})
                    Return
                End If
                '**********************
                ' ボタンクリック判定
                '**********************
                'hdnButtonClickに文字列が設定されていたら実行する
                If Me.hdnButtonClick IsNot Nothing AndAlso Me.hdnButtonClick.Value <> "" Then
                    'ボタンID + "_Click"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)
                    Dim btnEventName As String = Me.hdnButtonClick.Value & "_Click"
                    Dim param() As Object = Nothing

                    If Me.hdnButtonClick.Value.StartsWith("btnInvoiceItem") Then
                        btnEventName = "btnInvoiceItem" & "_Click"
                        ReDim param(0)
                        param(0) = Me.hdnButtonClick.Value.Replace("btnInvoiceItem", "")
                    End If
                    Me.hdnButtonClick.Value = ""
                    CallByName(Me, btnEventName, CallType.Method, param)
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
                '**********************
                ' スクロール処理 
                '**********************
                ListScrole()
                hdnMouseWheel.Value = ""

            End If
            DisplayListObjEdit() '共通関数により描画された一覧の制御
            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        Catch ex As Threading.ThreadAbortException
            'キャンセルやServerTransferにて後続の処理が打ち切られた場合のエラーは発生させない
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.SYSTEMADM 'ここは適宜変えてください
            Dim NORMAL As String = ""
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = messageNo
            COA0003LogFile.COA0003WriteLog()

            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
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
                Case Else
            End Select
        End If

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
    ''' Excelダウンロードボタン押下時処理
    ''' </summary>
    Public Sub btnExcelDownload_Click()

        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue

        '印刷対象出力
        Dim PrintCnt As Integer = 0
        Dim RepMainCnt As Integer = 0
        Dim tmpFile As String = ""
        Dim outUrl As String = ""

        '出力件数取得
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = CONST_REPTYPE_FVCLASS
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Dim dicReportConf = COA0017FixValue.VALUEDIC
            If dicReportConf.ContainsKey(CONST_REPTYPE_MNG) Then
                RepMainCnt = Convert.ToInt32(dicReportConf(CONST_REPTYPE_MNG)(1))
            End If
        Else
            Throw New Exception("Fix value getError")
        End If

        '一覧表示データ復元 
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If

        'Dim dtTmp As DataTable = dt.Copy
        'PrintCnt = (CType(System.Math.Ceiling(dt.Rows.Count / RepMainCnt), Integer))
        Dim dvTBLview As DataView
        Dim dtTmp As DataTable
        dvTBLview = New DataView(dt)
        dvTBLview.RowFilter = "INVOICENO NOT LIKE '999999999%' AND ORIGINALOUTPUT > 0 "
        dtTmp = dvTBLview.ToTable()
        PrintCnt = (CType(System.Math.Ceiling(dtTmp.Rows.Count / RepMainCnt), Integer))

        '帳票出力
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            'Dim reportId As String = "InvoiceShip_Management" 'Me.hdnReportVariant.Value
            Dim reportId As String = CONST_REPTYPE_MNG 'Me.hdnReportVariant.Value
            Dim reportMapId As String = CONST_MAPID
            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = CONST_REPTYPE_MNG & "_Data"          'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = dtTmp                                    'PARAM04:データ参照tabledata
            COA0027ReportTable.ADDSHEET = "データ"      'PARAM05:追記シート（任意）
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                Return
            End If

            tmpFile = COA0027ReportTable.FILEpath
            outUrl = COA0027ReportTable.URL

            COA0027ReportTable.REPORTID = CONST_REPTYPE_MNG & "_Rep"         'PARAM02:帳票ID
            COA0027ReportTable.ADDSHEET = "REPORT"                               'PARAM05:追記シート（任意）
            COA0027ReportTable.FILETYPE = "XLSX"                                 'PARAM03:出力ファイル形式

            For i As Integer = 1 To PrintCnt
                dtTmp = dt.Clone
                COA0027ReportTable.TBLDATA = dtTmp                          'PARAM04:データ参照tabledata
                COA0027ReportTable.ADDFILE = tmpFile
                COA0027ReportTable.ADDSHEETNO = "PAGECNT" & i.ToString & "-" & PrintCnt    'PARAM05:追記シート（任意）
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

            'PDF出力
            dtTmp = dt.Clone
            COA0027ReportTable.TBLDATA = dtTmp                          'PARAM04:データ参照tabledata
            COA0027ReportTable.ADDFILE = tmpFile
            COA0027ReportTable.ADDSHEET = "印刷対象外"                      'PARAM05:追記シート（任意）
            COA0027ReportTable.ADDSHEETNO = Nothing                           'PARAM05:追記シート（任意）
            'COA0027ReportTable.FILETYPE = "PDF"                         'PARAM03:出力ファイル形式
            COA0027ReportTable.FILETYPE = "XLSX"                         'PARAM03:出力ファイル形式
            COA0027ReportTable.COA0027ReportTable()
            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If

            '別画面でExcelを表示
            hdnPrintURL.Value = COA0027ReportTable.URL
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

        End With
    End Sub

    ''' <summary>
    ''' 請求書新規作成ボタン押下時
    ''' </summary>
    ''' <param name="invoiceName">invoiceの種類</param>
    Public Sub btnInvoiceItem_Click(invoiceName As String)


        Me.ThisScreenValues = GetDispValue()
        Me.ThisScreenValues.NewInvoiceCreate = True
        Me.ThisScreenValues.InvoiceNo = ""
        Me.ThisScreenValues.ToriCode = ""
        Select Case invoiceName
            Case "BreakerTotal"
                Me.ThisScreenValues.InvoiceType = ""
            Case "Lease"
                Me.ThisScreenValues.InvoiceType = "L"
        End Select

        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl

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
        HttpContext.Current.Session("MAPurl") = COA0012DoUrl.URL
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub

    ''' <summary>
    ''' 保存ボタン押下時
    ''' </summary>
    Public Sub btnSave_Click()

        Dim COA0021ListTable As New COA0021ListTable
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        Dim dt As DataTable = CreateDataTable()
        Dim tran As SqlTransaction = Nothing

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        Dim procDateTime As DateTime = DateTime.Now
        'CHECKチェックボックスが変更済みの全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("CHECK_AP_DATE")) <> Convert.ToString(item("CHECK_AP_PRVDATE")) _
                    Or Convert.ToString(item("CHECK_S_DATE")) <> Convert.ToString(item("CHECK_S_PRVDATE")) _
                    Or Convert.ToString(item("CHECK_AC")) <> Convert.ToString(item("CHECK_AC_PRV")))
        Dim deleteDt As DataTable = Nothing
        If q.Any = True Then
            deleteDt = q.CopyToDataTable
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Try

            Dim sqlStat As New StringBuilder
            Using sqlCon = New SqlConnection(COA0019Session.DBcon),
                sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                tran = sqlCon.BeginTransaction() 'トランザクション開始

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
                sqlStat.AppendLine("          ,APPROVEUSER")
                sqlStat.AppendLine("          ,APPROVEDATE")
                sqlStat.AppendLine("          ,SENDUSER")
                sqlStat.AppendLine("          ,SENDDATE")
                sqlStat.AppendLine("          ,ACCUSER")
                sqlStat.AppendLine("          ,ACCDATE")
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
                sqlStat.AppendLine("          ,@APPROVEUSER")
                sqlStat.AppendLine("          ,@APPROVEDATE")
                sqlStat.AppendLine("          ,@SENDUSER")
                sqlStat.AppendLine("          ,@SENDDATE")
                sqlStat.AppendLine("          ,@ACCUSER")
                sqlStat.AppendLine("          ,@ACCDATE")
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
                sqlStat.AppendLine("      WHERE INCTORICODE  = @INCTORICODE")
                sqlStat.AppendLine("        AND INVOICEMONTH = @INVOICEMONTH")
                sqlStat.AppendLine("        AND INVOICENOSUB = @INVOICENOSUB")
                sqlStat.AppendLine("        AND DELFLG       = @DELFLG")
                sqlStat.AppendLine(";")

                sqlStat.AppendFormat("UPDATE {0} ", CONST_TBL_INVOICEINFO).AppendLine()
                sqlStat.AppendLine("  SET")
                sqlStat.AppendLine("   DELFLG            = @DELFLG_Y")
                sqlStat.AppendLine("  ,UPDYMD            = @UPDYMD")
                sqlStat.AppendLine("  ,UPDUSER           = @UPDUSER")
                sqlStat.AppendLine("  ,UPDTERMID         = @UPDTERMID")
                sqlStat.AppendLine("  ,RECEIVEYMD        = @RECEIVEYMD")
                sqlStat.AppendLine("  WHERE INCTORICODE  = @INCTORICODE")
                sqlStat.AppendLine("    AND INVOICEMONTH = @INVOICEMONTH")
                sqlStat.AppendLine("    AND INVOICENOSUB = @INVOICENOSUB")
                sqlStat.AppendLine("    AND UPDYMD      <> @UPDYMD")
                sqlStat.AppendLine("    AND DELFLG       = @DELFLG")
                sqlStat.AppendLine(";")

                '動的パラメータのみ変数化
                'Dim paramCustomer = sqlCmd.Parameters.Add("@CUSTOMERCODE", SqlDbType.NVarChar)
                Dim paramToriCode = sqlCmd.Parameters.Add("@INCTORICODE", SqlDbType.NVarChar)
                Dim paramInvoiceMonth = sqlCmd.Parameters.Add("@INVOICEMONTH", SqlDbType.NVarChar)
                Dim paramInvoiceMonthSub = sqlCmd.Parameters.Add("@INVOICENOSUB", SqlDbType.Int)
                Dim paramApproveU = sqlCmd.Parameters.Add("@APPROVEUSER", SqlDbType.NVarChar)
                Dim paramApproveD = sqlCmd.Parameters.Add("@APPROVEDATE", SqlDbType.DateTime)
                Dim paramSendU = sqlCmd.Parameters.Add("@SENDUSER", SqlDbType.NVarChar)
                Dim paramSendD = sqlCmd.Parameters.Add("@SENDDATE", SqlDbType.DateTime)
                Dim paramAccU = sqlCmd.Parameters.Add("@ACCUSER", SqlDbType.NVarChar)
                Dim paramAccD = sqlCmd.Parameters.Add("@ACCDATE", SqlDbType.DateTime)

                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG_Y", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVname
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.Transaction = tran
                For Each drDelete As DataRow In deleteDt.Rows

                    'paramCustomer.Value = Convert.ToString(drDelete.Item("CUSTOMERCODE"))
                    paramToriCode.Value = Convert.ToString(drDelete.Item("INCTORICODE"))
                    paramInvoiceMonth.Value = Convert.ToString(drDelete.Item("INVOICEMONTH"))
                    paramInvoiceMonthSub.Value = Convert.ToInt32(drDelete.Item("INVOICENOSUB"))

                    If Convert.ToString(drDelete.Item("CHECK_AP_DATE")) <> Convert.ToString(drDelete.Item("CHECK_AP_PRVDATE")) Then
                        If Convert.ToString(drDelete.Item("CHECK_AP")) = CONST_CHECK_ON Then
                            paramApproveU.Value = COA0019Session.USERID
                            paramApproveD.Value = procDateTime
                        Else
                            paramApproveU.Value = ""
                            paramApproveD.Value = CONST_INITDATE
                        End If
                    Else
                        paramApproveU.Value = drDelete.Item("APPROVEUSER")
                        paramApproveD.Value = drDelete.Item("CHECK_AP_PRVDATE")
                    End If
                    If Convert.ToString(drDelete.Item("CHECK_S_DATE")) <> Convert.ToString(drDelete.Item("CHECK_S_PRVDATE")) Then
                        If Convert.ToString(drDelete.Item("CHECK_S")) = CONST_CHECK_ON Then
                            paramSendU.Value = COA0019Session.USERID
                            paramSendD.Value = procDateTime
                        Else
                            paramSendU.Value = ""
                            paramSendD.Value = CONST_INITDATE
                        End If
                    Else
                        paramSendU.Value = drDelete.Item("SENDUSER")
                        paramSendD.Value = drDelete.Item("CHECK_S_PRVDATE")
                    End If
                    If Convert.ToString(drDelete.Item("CHECK_AC_DATE")) <> Convert.ToString(drDelete.Item("CHECK_AC_PRVDATE")) Then
                        If Convert.ToString(drDelete.Item("CHECK_AC")) = CONST_CHECK_ON Then
                            paramAccU.Value = COA0019Session.USERID
                            paramAccD.Value = procDateTime
                        Else
                            paramAccU.Value = ""
                            paramAccD.Value = CONST_INITDATE
                        End If
                    Else
                        paramAccU.Value = drDelete.Item("ACCUSER")
                        paramAccD.Value = drDelete.Item("CHECK_AC_PRVDATE")
                    End If

                    sqlCmd.CommandText = sqlStat.ToString
                    sqlCmd.ExecuteNonQuery()

                Next

                tran.Commit()

            End Using

        Catch ex As Exception
            Throw
        Finally
            If tran IsNot Nothing Then
                tran.Dispose()
            End If
        End Try

        'メッセージ出力
        'hdnMsgId.Value = C_MESSAGENO.APPLYSUCCESS

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub

    ''' <summary>
    ''' 削除ボタン押下時イベント
    ''' </summary>
    Public Sub btnDel_Click()
        CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMDELETE, pageObject:=Me, submitButtonId:="btnDelOK")
    End Sub

    ''' <summary>
    ''' 削除ボタン押下時
    ''' </summary>
    Public Sub btnDelOK_Click()

        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = CreateDataTable()
        Dim tran As SqlTransaction = Nothing

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        Dim procDateTime As DateTime = DateTime.Now
        'CHECKチェックボックスがチェック済の全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("DELCHK")) = CONST_CHECK_ON)
        Dim deleteDt As DataTable = Nothing
        If q.Any = True Then
            deleteDt = q.CopyToDataTable
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Try
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("  UPDATE {0} ", CONST_TBL_INVOICEINFO).AppendLine()
            sqlStat.AppendLine("   SET DELFLG     = @DELFLG_Y")
            sqlStat.AppendLine("      ,UPDYMD     = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE INVOICENO  = @INVOICENO")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG_Y;")
            sqlStat.AppendFormat("  UPDATE {0} ", CONST_TBL_INVOICETANK).AppendLine()
            sqlStat.AppendLine("   SET DELFLG     = @DELFLG_Y")
            sqlStat.AppendLine("      ,UPDYMD     = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE INVOICENO  = @INVOICENO")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG_Y;")

            Using sqlCon = New SqlConnection(COA0019Session.DBcon),
                sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                tran = sqlCon.BeginTransaction() 'トランザクション開始

                '動的パラメータのみ変数化
                Dim paramInvoiceNo = sqlCmd.Parameters.Add("@INVOICENO", SqlDbType.NVarChar)

                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG_Y", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.Transaction = tran
                For Each drDelete As DataRow In deleteDt.Rows
                    paramInvoiceNo.Value = drDelete.Item("INVOICENO")

                    sqlCmd.CommandText = sqlStat.ToString
                    sqlCmd.ExecuteNonQuery()

                Next

                tran.Commit()

            End Using

        Catch ex As Exception
            Throw
        Finally
            If tran IsNot Nothing Then
                tran.Dispose()
            End If
        End Try

        'メッセージ出力
        'hdnMsgId.Value = C_MESSAGENO.APPLYSUCCESS

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub

    ''' <summary>
    ''' 画面グリッドのデータを取得しファイルに保存する。
    ''' </summary>
    Private Function FileSaveDisplayInput() As String

        Dim COA0021ListTable As New COA0021ListTable
        Dim procDate As Date = Now

        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        dt = CreateDataTable()
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            Return COA0021ListTable.ERR
        End If

        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String)
        '入力値保持用のフィールド名設定
        fieldIdList.Add("DELCHK", objChkPrifix)
        fieldIdList.Add("CHECK_AP", objChkPrifix)
        fieldIdList.Add("CHECK_S", objChkPrifix)
        fieldIdList.Add("CHECK_AC", objChkPrifix)

        ' とりあえず右データエリアを対象
        For i = 1 To dt.Rows.Count
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                Dim linePos As String = i.ToString
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    '                    formToPost.Remove(dispObjId)
                End If

                'If displayValue <> "" Then
                '    Dim targetRow = (From rowItem In dt Where Convert.ToString(rowItem("LINECNT")) = displayValue)
                '    targetRow(0).Item(fieldId.Key) = CONST_CHECK_ON
                'End If
                Dim targetRow = (From rowItem In dt Where Convert.ToString(rowItem("LINECNT")) = linePos)
                If Convert.ToString(targetRow(0).Item(fieldId.Key & "_DISP")) = CONST_CHECK_ON Then
                    If (displayValue <> "" AndAlso Convert.ToString(targetRow(0).Item(fieldId.Key)) <> CONST_CHECK_ON) _
                    OrElse (displayValue = "" AndAlso Convert.ToString(targetRow(0).Item(fieldId.Key)) = CONST_CHECK_ON) Then
                        If displayValue <> "" Then
                            targetRow(0).Item(fieldId.Key) = CONST_CHECK_ON
                        Else
                            targetRow(0).Item(fieldId.Key) = CONST_CHECK_OFF
                        End If
                        targetRow(0).Item(fieldId.Key & "_DATE") = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    Else
                        If Not Convert.ToString(targetRow(0).Item(fieldId.Key & "_DATE")).Equals(Convert.ToString(targetRow(0).Item(fieldId.Key & "_PRVDATE"))) Then
                            targetRow(0).Item(fieldId.Key & "_DATE") = Convert.ToString(targetRow(0).Item(fieldId.Key & "_PRVDATE"))
                        End If
                    End If

                End If
            Next
        Next

        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()

        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0021ListTable.ERR
        End If
        Return C_MESSAGENO.NORMAL
    End Function

    ''' <summary>
    ''' 請求書情報取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetInvoiceListDataTable() As DataTable
        'ソート順取得
        Dim COA0020ProfViewSort As New COA0020ProfViewSort
        'Dim textCustomerTblField As String = "NAMES"
        Dim textCustomerTblField As String = "NAMESJP1"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            'textCustomerTblField = "NAMESEN"
            textCustomerTblField = "NAMES1"
        End If
        ' いったん固定
        textCustomerTblField = "NAMES1"

        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = Me.hdnThisMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder
        Dim retDt As New DataTable
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,WORK.* ")
        sqlStat.AppendLine("      FROM ( ")
        sqlStat.AppendLine("      SELECT ")
        sqlStat.AppendLine("       II.CUSTOMERCODE ")
        sqlStat.AppendLine("      ,II.INVOICEMONTH ")
        sqlStat.AppendLine("      ,II.INVOICENOSUB ")
        sqlStat.AppendLine("      ,II.STYMD ")
        sqlStat.AppendLine("      ,II.ENDYMD ")
        sqlStat.AppendLine("      ,II.INVOICENO ")
        sqlStat.AppendLine("      ,II.INCTORICODE ")
        sqlStat.AppendLine("      ,II.REMARK ")
        sqlStat.AppendLine("      ,II.OUTLANGUAGE ")
        sqlStat.AppendLine("      ,II.INVOICEDATE ")
        sqlStat.AppendLine("      ,II.DRAFTOUTPUT ")
        sqlStat.AppendLine("      ,II.ORIGINALOUTPUT ")
        sqlStat.AppendLine("      ,II.ACCCURRENCYSEGMENT ")
        sqlStat.AppendLine("      ,II.AMOUNT ")
        sqlStat.AppendLine("      ,II.INVOICEAMOUNT ")
        sqlStat.AppendLine("      ,II.TANK ")
        sqlStat.AppendLine("      ,CASE WHEN II.ACCCURRENCYSEGMENT = 'JPY' THEN FORMAT( round(II.INVOICEAMOUNT,0), 'C', 'ja-JP') ")
        sqlStat.AppendLine("            ELSE FORMAT( II.INVOICEAMOUNT, 'C', 'en-US') ")
        sqlStat.AppendLine("       END AS DISPAMOUNT ")
        sqlStat.AppendLine("      ,II.CREATEUSER ")
        sqlStat.AppendLine("      ,TRIM(ISNULL(USERC.STAFFNAMES,II.CREATEUSER)) as 'CREATEUSERNAME'")
        sqlStat.AppendLine("      ,II.CREATEDATE ")
        sqlStat.AppendLine("      ,II.APPROVEUSER ")
        sqlStat.AppendLine("      ,TRIM(ISNULL(USERA.STAFFNAMES,II.APPROVEUSER)) as 'APPROVEUSERNAME' ")
        sqlStat.AppendLine("      ,II.APPROVEDATE ")
        sqlStat.AppendLine("      ,CASE WHEN II.APPROVEDATE <> '1900/01/01' THEN '1' ELSE '0' END as 'CHECK_AP' ")
        sqlStat.AppendLine("      ,'1' as 'CHECK_AP_DISP' ")
        sqlStat.AppendLine("      ,II.APPROVEDATE as 'CHECK_AP_DATE'")
        sqlStat.AppendLine("      ,CASE WHEN II.APPROVEDATE <> '1900/01/01' THEN '1' ELSE '0' END as 'CHECK_AP_PRV' ")
        sqlStat.AppendLine("      ,II.APPROVEDATE as 'CHECK_AP_PRVDATE'")
        sqlStat.AppendLine("      ,II.SENDUSER ")
        sqlStat.AppendLine("      ,TRIM(ISNULL(USERS.STAFFNAMES,II.SENDUSER)) as 'SENDUSERNAME' ")
        sqlStat.AppendLine("      ,II.SENDDATE ")
        sqlStat.AppendLine("      ,CASE WHEN II.SENDDATE <> '1900/01/01' THEN '1' ELSE '0' END as 'CHECK_S' ")
        sqlStat.AppendLine("      ,'1' as 'CHECK_S_DISP' ")
        sqlStat.AppendLine("      ,II.SENDDATE as 'CHECK_S_DATE'")
        sqlStat.AppendLine("      ,CASE WHEN II.SENDDATE <> '1900/01/01' THEN '1' ELSE '0' END as 'CHECK_S_PRV' ")
        sqlStat.AppendLine("      ,II.SENDDATE as 'CHECK_S_PRVDATE'")
        sqlStat.AppendLine("      ,II.ACCUSER ")
        sqlStat.AppendLine("      ,TRIM(ISNULL(USERAC.STAFFNAMES,II.ACCUSER)) as 'ACCUSERNAME' ")
        sqlStat.AppendLine("      ,II.ACCDATE ")
        sqlStat.AppendLine("      ,CASE WHEN II.ACCDATE <> '1900/01/01' THEN '1' ELSE '0' END as 'CHECK_AC' ")
        sqlStat.AppendLine("      ,'1' as 'CHECK_AC_DISP' ")
        sqlStat.AppendLine("      ,II.ACCDATE as 'CHECK_AC_DATE'")
        sqlStat.AppendLine("      ,CASE WHEN II.ACCDATE <> '1900/01/01' THEN '1' ELSE '0' END as 'CHECK_AC_PRV' ")
        sqlStat.AppendLine("      ,II.ACCDATE as 'CHECK_AC_PRVDATE'")
        sqlStat.AppendLine("      ,II.DELFLG ")
        sqlStat.AppendLine("      ,II.INITYMD ")
        sqlStat.AppendLine("      ,II.UPDYMD ")
        sqlStat.AppendLine("      ,II.UPDUSER ")
        sqlStat.AppendLine("      ,II.UPDTERMID ")
        sqlStat.AppendLine("      ,CASE WHEN II.DRAFTOUTPUT > 0 THEN '済' ELSE '' END AS DRAFTDISP ")
        sqlStat.AppendLine("      ,CASE WHEN II.ORIGINALOUTPUT > 0 THEN '済' ELSE '' END AS ORIGINALDISP ")
        'sqlStat.AppendFormat("      ,MC.{0} AS 'CUSTOMERNAME'", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,MTORI.{0} AS 'CUSTOMERNAME'", textCustomerTblField).AppendLine()
        'sqlStat.AppendLine("      ,0 AS AMOUNT ")
        sqlStat.AppendLine("      ,'' AS DELCHK ")
        sqlStat.AppendLine("      ,'1' AS DELCHK_DISP ")
        sqlStat.AppendLine("      ,II.UPDYMD as 'DELCHK_DATE'")
        sqlStat.AppendLine("      ,'' as 'DELCHK_PRV'")
        sqlStat.AppendLine("      ,II.UPDYMD as 'DELCHK_PRVDATE'")
        sqlStat.AppendLine("      ,ISNULL(APPLY.APPROVALTYPE,'0') as 'APPROVALTYPE'")
        sqlStat.AppendLine("      ,II.WORK_C1 as 'INVOICETYPE'")
        sqlStat.AppendFormat("  FROM {0} II", CONST_TBL_INVOICEINFO).AppendLine()
        sqlStat.AppendFormat("    INNER JOIN {0} MTORI", CONST_TBL_TORI).AppendLine()
        sqlStat.AppendLine("      ON    MTORI.COMPCODE       = @COMPCODE")
        sqlStat.AppendLine("      AND   MTORI.TORIKBN        = 'I'")
        sqlStat.AppendLine("      AND   MTORI.TORICODE       = II.INCTORICODE")
        sqlStat.AppendLine("      AND   MTORI.STYMD         <= @NOWDATE")
        sqlStat.AppendLine("      AND   MTORI.ENDYMD        >= @NOWDATE")
        sqlStat.AppendLine("      AND   MTORI.DELFLG        <> @DELFLG")
        sqlStat.AppendFormat("    LEFT OUTER JOIN {0} APPLY", CONST_TBL_APPLY).AppendLine()
        sqlStat.AppendLine("      ON    APPLY.COMPCODE       = @COMPCODE")
        sqlStat.AppendLine("      AND   APPLY.MAPID          = @MAPID")
        sqlStat.AppendLine("      AND   APPLY.EVENTCODE      = @EVENTCODE")
        sqlStat.AppendLine("      AND   APPLY.SUBCODE        = 'Common'")
        sqlStat.AppendLine("      AND   APPLY.STEP           = '01'")
        sqlStat.AppendLine("      AND   APPLY.USERID         = @USERID")
        sqlStat.AppendLine("      AND   APPLY.STYMD         <= @NOWDATE")
        sqlStat.AppendLine("      AND   APPLY.ENDYMD        >= @NOWDATE")
        sqlStat.AppendLine("      AND   APPLY.DELFLG        <> @DELFLG")
        sqlStat.AppendFormat("    LEFT OUTER JOIN {0} USERC", CONST_TBL_USER).AppendLine()
        sqlStat.AppendLine("      ON    USERC.USERID         = II.CREATEUSER")
        sqlStat.AppendLine("      AND   USERC.STYMD         <= @NOWDATE")
        sqlStat.AppendLine("      AND   USERC.ENDYMD        >= @NOWDATE")
        sqlStat.AppendLine("      AND   USERC.DELFLG        <> @DELFLG")
        sqlStat.AppendFormat("    LEFT OUTER JOIN {0} USERA", CONST_TBL_USER).AppendLine()
        sqlStat.AppendLine("      ON    USERA.USERID         = II.APPROVEUSER")
        sqlStat.AppendLine("      AND   USERA.STYMD         <= @NOWDATE")
        sqlStat.AppendLine("      AND   USERA.ENDYMD        >= @NOWDATE")
        sqlStat.AppendLine("      AND   USERA.DELFLG        <> @DELFLG")
        sqlStat.AppendFormat("    LEFT OUTER JOIN {0} USERS", CONST_TBL_USER).AppendLine()
        sqlStat.AppendLine("      ON    USERS.USERID         = II.SENDUSER")
        sqlStat.AppendLine("      AND   USERS.STYMD         <= @NOWDATE")
        sqlStat.AppendLine("      AND   USERS.ENDYMD        >= @NOWDATE")
        sqlStat.AppendLine("      AND   USERS.DELFLG        <> @DELFLG")
        sqlStat.AppendFormat("    LEFT OUTER JOIN {0} USERAC", CONST_TBL_USER).AppendLine()
        sqlStat.AppendLine("      ON    USERAC.USERID        = II.ACCUSER")
        sqlStat.AppendLine("      AND   USERAC.STYMD        <= @NOWDATE")
        sqlStat.AppendLine("      AND   USERAC.ENDYMD       >= @NOWDATE")
        sqlStat.AppendLine("      AND   USERAC.DELFLG       <> @DELFLG")
        sqlStat.AppendLine(" WHERE II.INVOICEMONTH = @INVOICEMONTH")
        If Me.hdnThisMapVariant.Value <> "Management" Then
            'sqlStat.AppendLine("   AND II.CUSTOMERCODE = @CUSTOMERCODE")
            sqlStat.AppendLine("   AND II.INCTORICODE = @CUSTOMERCODE")
        Else
            '管理画面はオリジナルを出力したもののみ
            'sqlStat.AppendLine("   AND II.ORIGINALOUTPUT > 0 ")
        End If
        sqlStat.AppendLine("   AND II.DELFLG      <> @DELFLG")
        If Me.hdnThisMapVariant.Value = "Management" Then
            'BreakerTotal
#Region "<< SQL BreakerTotal >>"
            sqlStat.AppendLine("   UNION ALL")
            sqlStat.AppendLine("      SELECT ")
            sqlStat.AppendLine("       ''                        AS CUSTOMERCODE ")
            sqlStat.AppendLine("      ,''                        AS INVOICEMONTH ")
            sqlStat.AppendLine("      ,''                        AS INVOICENOSUB ")
            sqlStat.AppendLine("      ,''                        AS STYMD ")
            sqlStat.AppendLine("      ,''                        AS ENDYMD ")
            sqlStat.AppendLine("      ,'999999999-' + MTORI.TORICODE　   AS INVOICENO ")
            sqlStat.AppendLine("      ,MTORI.TORICODE            AS INCTORICODE ")
            sqlStat.AppendLine("      ,'－－未選択明細数(SHIP済)－－'    AS REMARK ")
            sqlStat.AppendLine("      ,''                        AS OUTLANGUAGE ")
            sqlStat.AppendLine("      ,''                        AS INVOICEDATE ")
            sqlStat.AppendLine("      ,0                         AS DRAFTOUTPUT ")
            sqlStat.AppendLine("      ,0                         AS ORIGINALOUTPUT ")
            sqlStat.AppendLine("      ,''               AS ACCCURRENCYSEGMENT ")
            sqlStat.AppendLine("      ,0                AS AMOUNT ")
            sqlStat.AppendLine("      ,0                AS INVOICEAMOUNT ")
            sqlStat.AppendLine("      ,COUNT(*)         AS TANK ")
            sqlStat.AppendLine("      ,''               AS DISPAMOUNT ")
            sqlStat.AppendLine("      ,''               AS CREATEUSER ")
            sqlStat.AppendLine("      ,''               AS CREATEUSERNAME ")
            sqlStat.AppendLine("      ,''               AS CREATEDATE ")
            sqlStat.AppendLine("      ,''               AS APPROVEUSER ")
            sqlStat.AppendLine("      ,''               AS APPROVEUSERNAME ")
            sqlStat.AppendLine("      ,''               AS APPROVEDATE ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AP' ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AP_DISP' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_AP_DATE'")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AP_PRV' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_AP_PRVDATE'")
            sqlStat.AppendLine("      ,''               AS SENDUSER ")
            sqlStat.AppendLine("      ,''               AS SENDUSERNAME ")
            sqlStat.AppendLine("      ,''               AS SENDDATE ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_S' ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_S_DISP' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_S_DATE'")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_S_PRV' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_S_PRVDATE'")
            sqlStat.AppendLine("      ,''               AS ACCUSER ")
            sqlStat.AppendLine("      ,''               AS ACCUSERNAME ")
            sqlStat.AppendLine("      ,''               AS ACCDATE ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AC' ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AC_DISP' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_AC_DATE'")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AC_PRV' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_AC_PRVDATE'")
            sqlStat.AppendLine("      ,''               AS DELFLG ")
            sqlStat.AppendLine("      ,''               AS INITYMD ")
            sqlStat.AppendLine("      ,''               AS UPDYMD ")
            sqlStat.AppendLine("      ,''               AS UPDUSER ")
            sqlStat.AppendLine("      ,''               AS UPDTERMID ")
            sqlStat.AppendLine("      ,''               AS DRAFTDISP ")
            sqlStat.AppendLine("      ,''               AS ORIGINALDISP ")
            sqlStat.AppendFormat("    ,MTORI.{0} AS 'CUSTOMERNAME'", textCustomerTblField).AppendLine()
            sqlStat.AppendLine("      ,''               AS DELCHK ")
            sqlStat.AppendLine("      ,''               AS DELCHK_DISP ")
            sqlStat.AppendLine("      ,''               AS DELCHK_DATE")
            sqlStat.AppendLine("      ,''               AS DELCHK_PRV")
            sqlStat.AppendLine("      ,''               AS 'DELCHK_PRVDATE'")
            sqlStat.AppendLine("      ,''               AS 'APPROVALTYPE'")
            sqlStat.AppendLine("      ,''               AS 'INVOICETYPE'")

            sqlStat.AppendFormat("  FROM {0} OV", CONST_TBL_OV)
            sqlStat.AppendFormat("  INNER JOIN {0} OB", CONST_TBL_OB)
            sqlStat.AppendLine("      ON  OB.DELFLG       <> @DELFLG ")
            sqlStat.AppendLine("     AND  OB.ORDERNO       = OV.ORDERNO ")
            sqlStat.AppendFormat("  INNER JOIN {0} MC", CONST_TBL_CUSTOMER).AppendLine()
            sqlStat.AppendLine("      ON MC.STYMD        <= @NOWDATE")
            sqlStat.AppendLine("     AND MC.ENDYMD       >= @NOWDATE")
            sqlStat.AppendLine("     AND MC.DELFLG       <> @DELFLG")
            sqlStat.AppendLine("     AND MC.CUSTOMERCODE  = OV.CONTRACTORFIX")

            sqlStat.AppendFormat("  INNER JOIN {0} MTORI", CONST_TBL_TORI).AppendLine()
            sqlStat.AppendLine("     ON    MTORI.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("     AND   MTORI.TORIKBN        = 'I'")
            sqlStat.AppendLine("     AND   MTORI.TORICODE       = MC.INCTORICODE")
            sqlStat.AppendLine("     AND   MTORI.STYMD         <= @NOWDATE")
            sqlStat.AppendLine("     AND   MTORI.ENDYMD        >= @NOWDATE")
            sqlStat.AppendLine("     AND   MTORI.DELFLG        <> @DELFLG")

            sqlStat.AppendFormat("  INNER JOIN {0} OVSHIP", CONST_TBL_OV)
            sqlStat.AppendLine("      ON  OVSHIP.ORDERNO   = OV.ORDERNO ")
            sqlStat.AppendLine("     AND  OVSHIP.TANKNO    = OV.TANKNO ")
            sqlStat.AppendLine("     AND  OVSHIP.DTLPOLPOD = 'POL1' ")
            sqlStat.AppendLine("     AND  OVSHIP.ACTIONID  = 'SHIP' ")
            sqlStat.AppendLine("     AND  (( OVSHIP.ACTUALDATE BETWEEN CONVERT(DATE, @INVOICEMONTH + '/01') AND  EOMONTH(CONVERT(DATE, @INVOICEMONTH + '/01')) ) ")
            sqlStat.AppendLine("           OR ( OVSHIP.ACTUALDATE = '1900/01/01' AND  OVSHIP.SCHEDELDATE  BETWEEN CONVERT(DATE, @INVOICEMONTH + '/01') AND  EOMONTH(CONVERT(DATE, @INVOICEMONTH + '/01')))) ")
            sqlStat.AppendLine("     AND  OVSHIP.DELFLG   <> @DELFLG")

            sqlStat.AppendLine("   WHERE OV.DELFLG        <> @DELFLG ")
            sqlStat.AppendLine("     AND OV.TANKNO        <> ''")
            sqlStat.AppendLine("     AND OV.INVOICEDBY     = 'JPA00001'")
            sqlStat.AppendLine("     AND OV.COSTCODE       = 'A0001-01'")
            sqlStat.AppendLine("     AND OV.SOAAPPDATE     = '1900/01/01'")
            sqlStat.AppendLine("     AND OV.BRID        LIKE 'BT%'")
            sqlStat.AppendLine("     AND NOT EXISTS (")
            sqlStat.AppendFormat("                      SELECT * FROM {0} ITW", CONST_TBL_INVOICETANK)
            sqlStat.AppendLine("                        WHERE ITW.ORDERNO = OV.ORDERNO ")
            sqlStat.AppendLine("                        AND   ITW.TANKNO  = OV.TANKNO ")
            sqlStat.AppendLine("                        AND   ITW.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("                    ) ")
            sqlStat.AppendFormat("     GROUP BY MTORI.TORICODE,MTORI.{0} ", textCustomerTblField).AppendLine()
#End Region

            'Lease
#Region "<< SQL Lease >>"
            sqlStat.AppendLine("   UNION ALL")
            sqlStat.AppendLine("      SELECT ")
            sqlStat.AppendLine("       ''                        AS CUSTOMERCODE ")
            sqlStat.AppendLine("      ,''                        AS INVOICEMONTH ")
            sqlStat.AppendLine("      ,''                        AS INVOICENOSUB ")
            sqlStat.AppendLine("      ,''                        AS STYMD ")
            sqlStat.AppendLine("      ,''                        AS ENDYMD ")
            sqlStat.AppendLine("      ,'999999999-' + MTORI.TORICODE   AS INVOICENO ")
            sqlStat.AppendLine("      ,MTORI.TORICODE            AS INCTORICODE ")
            sqlStat.AppendLine("      ,'－－未選択明細数(LEASE)－－'    AS REMARK ")
            sqlStat.AppendLine("      ,''                        AS OUTLANGUAGE ")
            sqlStat.AppendLine("      ,''                        AS INVOICEDATE ")
            sqlStat.AppendLine("      ,0                         AS DRAFTOUTPUT ")
            sqlStat.AppendLine("      ,0                         AS ORIGINALOUTPUT ")
            sqlStat.AppendLine("      ,''               AS ACCCURRENCYSEGMENT ")
            sqlStat.AppendLine("      ,0                AS AMOUNT ")
            sqlStat.AppendLine("      ,0                AS INVOICEAMOUNT ")
            sqlStat.AppendLine("      ,COUNT(*)         AS TANK ")
            sqlStat.AppendLine("      ,''               AS DISPAMOUNT ")
            sqlStat.AppendLine("      ,''               AS CREATEUSER ")
            sqlStat.AppendLine("      ,''               AS CREATEUSERNAME ")
            sqlStat.AppendLine("      ,''               AS CREATEDATE ")
            sqlStat.AppendLine("      ,''               AS APPROVEUSER ")
            sqlStat.AppendLine("      ,''               AS APPROVEUSERNAME ")
            sqlStat.AppendLine("      ,''               AS APPROVEDATE ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AP' ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AP_DISP' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_AP_DATE'")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AP_PRV' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_AP_PRVDATE'")
            sqlStat.AppendLine("      ,''               AS SENDUSER ")
            sqlStat.AppendLine("      ,''               AS SENDUSERNAME ")
            sqlStat.AppendLine("      ,''               AS SENDDATE ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_S' ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_S_DISP' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_S_DATE'")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_S_PRV' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_S_PRVDATE'")
            sqlStat.AppendLine("      ,''               AS ACCUSER ")
            sqlStat.AppendLine("      ,''               AS ACCUSERNAME ")
            sqlStat.AppendLine("      ,''               AS ACCDATE ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AC' ")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AC_DISP' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_AC_DATE'")
            sqlStat.AppendLine("      ,'0'              AS 'CHECK_AC_PRV' ")
            sqlStat.AppendLine("      ,''               AS 'CHECK_AC_PRVDATE'")
            sqlStat.AppendLine("      ,''               AS DELFLG ")
            sqlStat.AppendLine("      ,''               AS INITYMD ")
            sqlStat.AppendLine("      ,''               AS UPDYMD ")
            sqlStat.AppendLine("      ,''               AS UPDUSER ")
            sqlStat.AppendLine("      ,''               AS UPDTERMID ")
            sqlStat.AppendLine("      ,''               AS DRAFTDISP ")
            sqlStat.AppendLine("      ,''               AS ORIGINALDISP ")
            sqlStat.AppendFormat("    ,MTORI.{0} AS 'CUSTOMERNAME'", textCustomerTblField).AppendLine()
            sqlStat.AppendLine("      ,''               AS DELCHK ")
            sqlStat.AppendLine("      ,''               AS DELCHK_DISP ")
            sqlStat.AppendLine("      ,''               AS DELCHK_DATE")
            sqlStat.AppendLine("      ,''               AS DELCHK_PRV")
            sqlStat.AppendLine("      ,''               AS 'DELCHK_PRVDATE'")
            sqlStat.AppendLine("      ,''               AS 'APPROVALTYPE'")
            sqlStat.AppendFormat("    ,'{0}'              AS 'INVOICETYPE'", "L").AppendLine()

            sqlStat.AppendFormat("  FROM {0} OV", CONST_TBL_OV)
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

            sqlStat.AppendFormat("  INNER JOIN {0} MC", CONST_TBL_CUSTOMER).AppendLine()
            sqlStat.AppendLine("      ON MC.STYMD        <= @NOWDATE")
            sqlStat.AppendLine("     AND MC.ENDYMD       >= @NOWDATE")
            sqlStat.AppendLine("     AND MC.DELFLG       <> @DELFLG")
            sqlStat.AppendLine("     AND MC.CUSTOMERCODE  = OV.CONTRACTORFIX")

            sqlStat.AppendFormat("  INNER JOIN {0} MTORI", CONST_TBL_TORI).AppendLine()
            sqlStat.AppendLine("     ON    MTORI.COMPCODE       = @COMPCODE")
            sqlStat.AppendLine("     AND   MTORI.TORIKBN        = 'I'")
            sqlStat.AppendLine("     AND   MTORI.TORICODE       = MC.INCTORICODE")
            sqlStat.AppendLine("     AND   MTORI.STYMD         <= @NOWDATE")
            sqlStat.AppendLine("     AND   MTORI.ENDYMD        >= @NOWDATE")
            sqlStat.AppendLine("     AND   MTORI.DELFLG        <> @DELFLG")

            sqlStat.AppendLine("   WHERE OV.DELFLG        <> @DELFLG ")
            sqlStat.AppendLine("     AND OV.TANKNO        <> ''")
            sqlStat.AppendLine("     AND OV.INVOICEDBY     = 'JPA00001'")
            sqlStat.AppendLine("     AND OV.COSTCODE    LIKE 'S0103%'")
            sqlStat.AppendLine("     AND ( OV.ACTUALDATE BETWEEN CONVERT(DATE, @INVOICEMONTH + '/01') AND EOMONTH(CONVERT(DATE, @INVOICEMONTH + '/01')) )")
            sqlStat.AppendLine("     AND OV.SOAAPPDATE     = '1900/01/01'")
            sqlStat.AppendLine("     AND NOT EXISTS (")
            sqlStat.AppendFormat("                      SELECT * FROM {0} ITW", CONST_TBL_INVOICETANK)
            sqlStat.AppendLine("                        WHERE ITW.ORDERNO = OV.ORDERNO ")
            sqlStat.AppendLine("                        AND   ITW.TANKNO  = OV.TANKNO ")
            '当月分のリースタンク請求明細が存在しないタンクを対象
            sqlStat.AppendLine("                        AND   SUBSTRING(ITW.INVOICENO,CHARINDEX('-',ITW.INVOICENO)+1,4) = SUBSTRING(@INVOICEMONTH,3,2) + SUBSTRING(@INVOICEMONTH,6,2)")
            sqlStat.AppendLine("                        AND   ITW.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("                    ) ")
            sqlStat.AppendFormat("     GROUP BY MTORI.TORICODE,MTORI.{0} ", textCustomerTblField).AppendLine()
#End Region

        End If
        sqlStat.AppendLine("   ) WORK")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)

        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = Me.GBT00028SValues.InvoiceMonth
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = Me.GBT00028SValues.CustomerCode

                '承認関連
                .Add("@MAPID", SqlDbType.NVarChar).Value = CONST_MAPID
                .Add("@EVENTCODE", SqlDbType.NVarChar).Value = CONST_EVENT_APPLY
                .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID

            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return retDt
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

        'ラベル等やグリッドを除くの文言設定(適宜追加) リピーターの表ヘッダーもこの方式で可能ですので
        '作成者に聞いてください。
        AddLangSetting(dicDisplayText, Me.lblInvoiceDate, "請求月", "Invoice Date")
        AddLangSetting(dicDisplayText, Me.lblCustomerName, "顧客名", "Customer Name")

        'AddLangSetting(dicDisplayText, Me.btnExtract, "絞り込み", "Search")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "台帳出力", "List Download")
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")

        'AddLangSetting(dicDisplayText, Me.btnInvoiceNew, "請求書作成", "Invoice New")
        AddLangSetting(dicDisplayText, Me.lblInvoiceNew, "請求書作成", "Invoice New")
        AddLangSetting(dicDisplayText, Me.btnDel, "削除", "Delete")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub

    ''' <summary>
    ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateDataTable() As DataTable
        Dim retDt As New DataTable
        '共通項目
        retDt.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        retDt.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        retDt.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        retDt.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        retDt.Columns.Add("HIDDEN", GetType(Integer))
        '個別項目
        retDt.Columns.Add("INVOICENO", GetType(String))
        retDt.Columns.Add("CHECK", GetType(String))

        Return retDt
    End Function

    ''' <summary>
    ''' リスト行ダブルクリック時イベント
    ''' </summary>
    Private Sub ListRowDbClick()
        Dim rowIdString As String = Me.hdnListDBclick.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(rowIdString, rowId) = True Then
            rowId = rowId - 1
        Else
            Return
        End If

        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New COA0021ListTable

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})

            Return
        End If
        Dim selectedRow As DataRow = dt.Rows(rowId)
        If Left(Convert.ToString(selectedRow.Item("INVOICENO")), 9) <> "999999999" Then

            '未選択明細行　以外　詳細画面に遷移
            Me.ThisScreenValues = GetDispValue()
            Me.ThisScreenValues.NewInvoiceCreate = False
            Me.ThisScreenValues.InvoiceNo = Convert.ToString(selectedRow.Item("INVOICENO"))
            Me.ThisScreenValues.ToriCode = Convert.ToString(selectedRow.Item("INCTORICODE"))
            Me.ThisScreenValues.InvoiceType = Convert.ToString(selectedRow.Item("INVOICETYPE"))

            '■■■ 画面遷移先URL取得 ■■■
            Dim COA0012DoUrl As New COA0012DoUrl
            COA0012DoUrl.MAPIDP = CONST_MAPID
            COA0012DoUrl.VARIP = Me.hdnThisMapVariant.Value
            COA0012DoUrl.COA0012GetDoUrl()
            If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
            Else
                CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
                Return
            End If
            HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
            HttpContext.Current.Session("MAPurl") = COA0012DoUrl.URL

            '画面遷移実行
            Server.Transfer(COA0012DoUrl.URL)

        End If

    End Sub

    ''' <summary>
    ''' 一覧 マウスホイール時処理 (一覧スクロール)
    ''' </summary>
    Protected Sub ListScrole()
        'If hdnMouseWheel.Value = "" Then
        '    Return
        'End If
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル

        '表示データ件数取得
        Dim DataCnt As Integer = 0                  '(絞り込み後)有効Data数

        '一覧表示データ復元
        Dim dt As DataTable = CreateDataTable()

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To dt.Rows.Count - 1
            If Convert.ToString(dt.Rows(i)(4)) = "0" Then
                DataCnt = DataCnt + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                dt.Rows(i)("SELECT") = DataCnt
            End If
        Next

        '現在表示位置取得
        Dim ListPosition As Integer = 0
        If Me.hdnListPosition.Value = "" Then
            ListPosition = 1
        Else
            Try
                Integer.TryParse(Me.hdnListPosition.Value, ListPosition)
            Catch ex As Exception
                ListPosition = 1
            End Try
        End If

        Dim ScrollInt As Integer = CONST_SCROLLROWCOUNT
        '表示位置決定(次頁スクロール)
        If hdnMouseWheel.Value = "+" And
        (ListPosition + ScrollInt) < DataCnt Then
            ListPosition = ListPosition + ScrollInt
        End If

        '表示位置決定(前頁スクロール)
        If hdnMouseWheel.Value = "-" And
        (ListPosition - ScrollInt) >= 0 Then
            ListPosition = ListPosition - ScrollInt
        End If

        'ソート
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, ListPosition, hdnListPosition)
        '一覧作成
        COA0013TableObject.MAPID = CONST_MAPID
        COA0013TableObject.VARI = Me.GBT00028SValues.ViewId
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = Me.WF_LISTAREA
        COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.LEVENT = "ondblclick"
        COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.USERSORTOPT = 0
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

    End Sub

    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00028SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00028SELECT = DirectCast(Page.PreviousPage, GBT00028SELECT)
            Me.GBT00028SValues = prevObj.ThisScreenValues
            ViewState("GBT00028SValues") = Me.GBT00028SValues

        ElseIf TypeOf Page.PreviousPage Is GBT00028INVOICEEDIT Then
            '単票画面からの戻り
            Dim prevObj As GBT00028INVOICEEDIT = DirectCast(Page.PreviousPage, GBT00028INVOICEEDIT)
            Me.GBT00028SValues = prevObj.GBT00028RValues.GBT00028SValues
            ViewState("GBT00028SValues") = Me.GBT00028SValues

        ElseIf TypeOf Page.PreviousPage Is GBT00028RESULT Then
            '自身から遷移（削除時のリフレッシュのみの想定、それ以外の用途を追加する場合は注意）
            Dim prevObj As GBT00028RESULT = DirectCast(Page.PreviousPage, GBT00028RESULT)
            Me.GBT00028SValues = prevObj.GBT00028SValues
            ViewState("GBT00028SValues") = Me.GBT00028SValues

            Me.hdnThisMapVariant.Value = prevObj.hdnThisMapVariant.Value

            Dim prevLbRightObj As ListBox = DirectCast(prevObj.FindControl(Me.lbRightList.ID), ListBox)
            If prevLbRightObj IsNot Nothing Then
                Me.lbRightList.SelectedValue = prevLbRightObj.SelectedValue
            End If

            '正常メッセージをメッセージエリアに表示（削除正常時のみ自身をリフレッシュするため）
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
        End If

        ' 後で移動
        If Me.GBT00028SValues.CustomerCode = "" Then
            'Me.btnInvoiceNew.Visible = False
            Me.lblInvoiceNew.Visible = False
        Else
            'Me.btnInvoiceNew.Visible = True
            Me.lblInvoiceNew.Visible = True
        End If
        Me.txtInvoiceDate.Text = Me.GBT00028SValues.InvoiceMonth
        Me.txtCustomerName.Text = Me.GBT00028SValues.CustomerName

        'Me.WF_LISTAREA.CssClass = Me.hdnSearchType.Value
    End Sub

    ''' <summary>
    ''' 画面表示のテーブルを制御する
    ''' </summary>
    Private Sub DisplayListObjEdit()

        If Me.hdnThisMapVariant.Value = "Management" Then

            '請求書管理の場合、以下の項目を非表示
            Dim lstInputObjects As New List(Of Control) From {Me.lblCustomerName, Me.txtCustomerName}

            For Each obj As Control In lstInputObjects
                If TypeOf obj Is TextBox Then
                    Dim txtObj As TextBox = DirectCast(obj, TextBox)
                    txtObj.Visible = False
                End If
            Next

            Me.lblCustomerName.Text = ""
            Me.txtCustomerName.Text = ""

            'ボタン
            Me.btnDel.Visible = False
            'Me.btnInvoiceNew.Visible = False
            Me.lblInvoiceNew.Visible = False

            ' チェックボックス制御
            Dim COA0021ListTable As New COA0021ListTable
            Dim dt As DataTable = CreateDataTable()
            Dim tran As SqlTransaction = Nothing

            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                            messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
                Return
            End If

            Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
            Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
            'For i As Integer = 0 To dt.Rows.Count
            For Each dr As DataRow In dt.Rows

                If Left(Convert.ToString(dr.Item("INVOICENO")), 9) = "999999999" _
                    OrElse Convert.ToInt32(dr.Item("ORIGINALOUTPUT")) = 0 Then
                    ' 未選択明細数 行
                    ' 非表示
                    Dim chkId As String = "chkWF_LISTAREACHECK_AP" & Convert.ToString(dr.Item("LINECNT"))
                    Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
                    chk.Visible = False
                    dr.Item("CHECK_AP_DISP") = "0"

                    ' 非表示
                    chkId = "chkWF_LISTAREACHECK_S" & Convert.ToString(dr.Item("LINECNT"))
                    chk = DirectCast(tblCont.FindControl(chkId), CheckBox)
                    chk.Visible = False
                    dr.Item("CHECK_S_DISP") = "0"

                    ' 非表示
                    chkId = "chkWF_LISTAREACHECK_AC" & Convert.ToString(dr.Item("LINECNT"))
                    chk = DirectCast(tblCont.FindControl(chkId), CheckBox)
                    chk.Visible = False
                    dr.Item("CHECK_AC_DISP") = "0"

                Else
                    ' 承認
                    If Convert.ToString(dr.Item("APPROVALTYPE")) = "0" _
                    OrElse Date.Parse(Convert.ToString(dr.Item("APPROVEDATE"))).ToString("yyyy/MM/dd") <> "1900/01/01" Then
                        ' 承認権限が無ければ非活性
                        Dim chkId As String = "chkWF_LISTAREACHECK_AP" & Convert.ToString(dr.Item("LINECNT"))
                        Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
                        chk.Enabled = False
                        dr.Item("CHECK_AP_DISP") = "0"
                    End If

                    ' 発送
                    If Date.Parse(Convert.ToString(dr.Item("APPROVEDATE"))).ToString("yyyy/MM/dd") = "1900/01/01" _
                    OrElse Date.Parse(Convert.ToString(dr.Item("SENDDATE"))).ToString("yyyy/MM/dd") <> "1900/01/01" Then
                        ' 未承認であれば非活性
                        Dim chkId As String = "chkWF_LISTAREACHECK_S" & Convert.ToString(dr.Item("LINECNT"))
                        Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
                        chk.Enabled = False
                        dr.Item("CHECK_S_DISP") = "0"
                    End If

                    ' 計上

                End If

            Next

            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021saveListTable()

        Else

            'ボタン
            Me.btnSave.Visible = False
            Me.btnExcelDownload.Visible = False

        End If

    End Sub

    ''' <summary>
    ''' 当画面の情報を引き渡し用クラスに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDispValue() As GBT00028RValues
        Dim retVal As New GBT00028RValues
        retVal.GBT00028SValues = Me.GBT00028SValues
        Return retVal
    End Function

    ''' <summary>
    ''' 当画面情報保持クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00028RValues
        ''' <summary>
        ''' 新規請求書作成(True:新規作成,False:更新)
        ''' </summary>
        ''' <returns></returns>
        Public Property NewInvoiceCreate As Boolean = False
        ''' <summary>
        ''' 検索画面情報保持値
        ''' </summary>
        ''' <returns></returns>
        Public Property GBT00028SValues As GBT00028SELECT.GBT00028SValues
        ''' <summary>
        ''' 請求書No
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>選択した契約書No</remarks>
        Public Property InvoiceNo As String = ""
        ''' <summary>
        ''' 取引先コード
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>選択した取引先コード</remarks>
        Public Property ToriCode As String = ""
        ''' <summary>
        ''' 請求書種類(NULL:BreakerTotal,L:Lease)
        ''' </summary>
        ''' <returns></returns>
        Public Property InvoiceType As String = ""
    End Class

End Class