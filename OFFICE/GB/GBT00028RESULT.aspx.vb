﻿Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' リース一覧
''' </summary>
Public Class GBT00028RESULT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00028R" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_INITDATE As String = "1900/01/01" 'DB日付初期値

    Private Const CONST_TBL_INVOICEINFO As String = "GBT0016_INVOICE_INFO"
    Private Const CONST_TBL_INVOICETANK As String = "GBT0017_INVOICE_TANKINFO"
    Private Const CONST_TBL_USER As String = "COS0005_USER"
    Private Const CONST_TBL_FV As String = "COS0017_FIXVALUE"
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

    '請求書タイプ
    Private Const CONST_INVOICETYPE_NORMAL As String = ""
    Private Const CONST_INVOICETYPE_LEASE As String = "L"
    Private Const CONST_INVOICETYPE_NOTDONE As String = "S"
    Private Const CONST_INVOICETYPE_OTHER As String = "O"

    Private SavedDt As DataTable = Nothing
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
                Dim item As New List(Of String)
                If Me.hdnThisMapVariant.Value <> "Management" Then
                    item.Add("BreakerTotal")
                    item.Add("Lease")
                Else
                    item.Add("Other(JPY)")
                    item.Add("Other(USD)")
                End If
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
                    Me.SavedDt = dt

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

                    If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
                        Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                                  Select Convert.ToInt32(dr.Item("LINECNT"))).ToList

                        'For Each targetCheckBoxId As String In {"DELCHK", "CHECK_AP", "CHECK_S", "CHECK_AC"}

                        '    '申請チェックボックスの加工
                        '    Dim targetCheckBoxLineCnt = (From dr As DataRow In listData
                        '                                 Where Convert.ToString(dr.Item(targetCheckBoxId)) <> ""
                        '                                 Select Convert.ToInt32(dr.Item("LINECNT")))
                        '    For Each lineCnt As Integer In targetCheckBoxLineCnt
                        '        Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & targetCheckBoxId & lineCnt.ToString
                        '        Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                        '        If tmpObj IsNot Nothing Then
                        '            Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        '            chkObj.Checked = True
                        '        End If
                        '    Next

                        'Next

                        ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
                    Else
                        ViewState("DISPLAY_LINECNT_LIST") = Nothing
                    End If

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
                    'Me.hdnListDBclick.Value = ""
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
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim notSavedData = GetModifiedDataTable()
        If Not (notSavedData Is Nothing OrElse notSavedData.Rows.Count = 0) Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, pageObject:=Me, submitButtonId:="btnBackOk")
            Return
        End If
        '確認メッセージを表示しない場合は終了
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
        dvTBLview.RowFilter = "INVOICETYPE <> '" & CONST_INVOICETYPE_NOTDONE & "' AND ORIGINALOUTPUT > 0 "
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
                Me.ThisScreenValues.InvoiceType = CONST_INVOICETYPE_NORMAL
            Case "Lease"
                Me.ThisScreenValues.InvoiceType = CONST_INVOICETYPE_LEASE
            Case "Other(JPY)"
                AddOhterInvoice("JPY")
                Exit Sub
            Case "Other(USD)"
                AddOhterInvoice("USD")
                Exit Sub
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
    ''' THOMAS外請求書行追加
    ''' </summary>
    Public Sub AddOhterInvoice(ByVal invoiceCurrency As String)

        Dim COA0021ListTable As New COA0021ListTable
        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        dt = CreateDataTable()
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
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

        Dim addLineCnt As Integer = 1
        '追加した行以降をずらす
        For Each row As DataRow In dt.AsEnumerable
            Dim lineCnt As Integer = Convert.ToInt32(row.Item("LINECNT"))
            If Convert.ToString(row.Item("INVOICETYPE")) = CONST_INVOICETYPE_NOTDONE Then
                If addLineCnt = 1 Then
                    addLineCnt = lineCnt
                End If
                lineCnt += 1
                row.Item("LINECNT") = lineCnt.ToString
            End If
        Next

        Dim newRow = dt.NewRow()
        newRow("LINECNT") = addLineCnt.ToString
        newRow("OPERATION") = ""
        newRow("SELECT") = "1"
        newRow("HIDDEN") = "0"
        newRow("ACTION") = "NEW"
        newRow("CUSTOMERCODE") = ""
        newRow("INVOICEMONTH") = Me.GBT00028SValues.InvoiceMonth
        newRow("INVOICENOSUB") = 0
        newRow("STYMD") = procDateTime
        newRow("ENDYMD") = "2099/12/31"
        newRow("INVOICENO") = ""
        newRow("INCTORICODE") = "9999999999"
        newRow("REMARK") = ""
        newRow("OUTLANGUAGE") = ""
        newRow("INVOICEDATE") = "1900/01/01"
        newRow("DRAFTOUTPUT") = "0"
        newRow("ORIGINALOUTPUT") = "1"
        newRow("ACCCURRENCYSEGMENT") = invoiceCurrency
        newRow("AMOUNT") = 0
        newRow("INVOICEAMOUNT") = 0
        newRow("TAXAMT") = 0
        newRow("NONTAXAMT") = 0
        newRow("TANK") = 0
        newRow("CREATEUSER") = COA0019Session.USERID
        newRow("CREATEUSERNAME") = COA0019Session.USERNAME
        newRow("CREATEDATE") = "1900/01/01"
        newRow("APPROVEDATE") = "1900/01/01"
        newRow("CHECK_AP") = "0"
        newRow("CHECK_AP_DISP") = "1"
        newRow("CHECK_AP_PRV") = "0"
        newRow("CHECK_AP_DATE") = "1900/01/01"
        newRow("CHECK_AP_PRVDATE") = "1900/01/01"
        newRow("SENDDATE") = "1900/01/01"
        newRow("CHECK_S") = "0"
        newRow("CHECK_S_DISP") = "1"
        newRow("CHECK_S_PRV") = "0"
        newRow("CHECK_S_DATE") = "1900/01/01"
        newRow("CHECK_S_PRVDATE") = "1900/01/01"
        newRow("ACCDATE") = "1900/01/01"
        newRow("CHECK_AC") = "0"
        newRow("CHECK_AC_DISP") = "1"
        newRow("CHECK_AC_PRV") = "0"
        newRow("CHECK_AC_DATE") = "1900/01/01"
        newRow("CHECK_AC_PRVDATE") = "1900/01/01"
        newRow("DELCHK") = "0"
        newRow("DELCHK_DISP") = "1"
        newRow("DELCHK_DATE") = "1900/01/01"
        newRow("DELCHK_PRV") = ""
        newRow("DELCHK_PRVDATE") = "1900/01/01"
        newRow("DELFLG") = CONST_FLAG_NO
        newRow("DRAFTDISP") = ""
        'newRow("ORIGINALDISP") = "済"
        newRow("APPROVALTYPE") = "0"

        newRow("INVOICETYPE") = CONST_INVOICETYPE_OTHER
        newRow("INVOICENO_PRV") = ""
        newRow("CUSTOMERNAME_PRV") = ""
        newRow("REMARK_PRV") = ""
        newRow("INVOICEAMOUNT_PRV") = -1 '新規時は必ず変更発生
        newRow("TAXAMT_PRV") = 0
        newRow("NONTAXAMT_PRV") = 0
        dt.Rows.Add(newRow)

        Me.SavedDt = dt
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return
        End If

    End Sub

    ''' <summary>
    ''' 保存ボタン押下時
    ''' </summary>
    Public Sub btnSave_Click()

        Dim COA0032Apploval As New BASEDLL.COA0032Apploval

        Dim dt As DataTable = Nothing
        Dim messageNo As String
        If Me.SavedDt Is Nothing Then
            dt = CreateDataTable()
            Dim COA0021ListTable As COA0021ListTable = New COA0021ListTable
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If

        'データテーブルの禁則文字置換
        ChangeInvalidChar(dt, New List(Of String) From
                          {"INVOICENO", "CUSTOMERNAME", "REMARK", "INVOICEAMOUNT", "TAXAMT", "NONTAXAMT"})


        Dim targetData = GetModifiedDataTable()
        '登録対象データが0件の場合は処理終了
        If targetData Is Nothing OrElse targetData.Rows.Count = 0 Then
            messageNo = C_MESSAGENO.NOENTRYDATA
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Dim checkDt As DataTable = Nothing
        '削除は単項目チェック対象外とする
        Dim q = (From item In targetData
                 Where Convert.ToString(item("DELCHK")) <> "1")
        If q.Any Then
            checkDt = q.CopyToDataTable
        End If
        '単項目チェック
        Dim fieldList As New List(Of String)
        fieldList.AddRange({"INVOICENO", "CUSTOMERNAME", "REMARK", "INVOICEAMOUNT", "TAXAMT", "NONTAXAMT"})
        Dim keyFields As New List(Of String) From {"LINECNT"}
        Dim errMessage As String = ""

        If checkDt.Rows.Count > 0 Then
            messageNo = CheckSingle(CONST_MAPID, checkDt, fieldList, errMessage, keyFields:=keyFields)
            If messageNo <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)

                '左ボックスにエラーメッセージ表示
                Me.txtRightErrorMessage.Text = errMessage
                Return
            End If

            '請求金額チェック
            Dim retMsg As New StringBuilder
            For Each row As DataRow In checkDt.Rows
                Dim tmpAmount = DecimalStringToDecimal(row.Item("INVOICEAMOUNT").ToString)
                Dim tmpTaxAmt = DecimalStringToDecimal(row.Item("TAXAMT").ToString)
                Dim tmpNonTaxAmt = DecimalStringToDecimal(row.Item("NONTAXAMT").ToString)
                If tmpAmount <> (Convert.ToDecimal(tmpTaxAmt * 1.1) + tmpNonTaxAmt) Then
                    retMsg.AppendFormat("・{0}[{2}]：{1}", "LINECNT", "Totals don't match.", row("LINECNT")).AppendLine()
                    retMsg.AppendFormat("--> {0} = {1}", padRight("INVOICEAMOUNT", 20), tmpAmount).AppendLine()
                    messageNo = C_MESSAGENO.RIGHTBIXOUT
                End If
            Next
            If messageNo <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)

                '右ボックスにエラーメッセージ表示
                Me.txtRightErrorMessage.Text = retMsg.ToString
                Return
            End If
        End If

        Dim procDateTime As DateTime = DateTime.Now
        Try
            Using sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                Using sqlTran = sqlCon.BeginTransaction
                    For Each dr As DataRow In targetData.Rows
                        If dr.Item("ACTION").Equals("NEW") Then
                            Dim invoiceNoSub = GetNewInvoice(sqlCon, sqlTran, dr.Item("INCTORICODE").ToString, Me.GBT00028SValues.InvoiceMonth)
                            dr.Item("INVOICENOSUB") = invoiceNoSub
                            InsertInvoice(dr, sqlCon, sqlTran, procDateTime)
                        Else
                            UpdateInvoiceInfo(dr, sqlCon, sqlTran, procDateTime)
                        End If
                    Next
                    sqlTran.Commit()
                End Using
            End Using

        Catch ex As Exception
            Throw
        Finally
        End Try

        'メッセージ出力
        'hdnMsgId.Value = C_MESSAGENO.APPLYSUCCESS

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub

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
    ''' 変更検知処理
    ''' </summary>
    ''' <returns>変更対象のデータテーブルを生成</returns>
    ''' <remarks>当処理の戻り値データテーブルが更新・追加・論理削除対象のデータとなる</remarks>
    Private Function GetModifiedDataTable() As DataTable
        Dim COA0021ListTable As New COA0021ListTable
        Dim retDt As DataTable = CreateDataTable()
        Dim currentDt As DataTable
        '**************************************************
        'データテーブル復元
        '**************************************************
        '画面編集しているデータテーブル取得
        If Me.SavedDt Is Nothing Then
            currentDt = CreateDataTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = currentDt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                currentDt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return Nothing
            End If

        Else
            currentDt = Me.SavedDt
        End If

        'CHECKチェックボックスが変更済みの全データを取得
        'Otherの変更データを取得
        Dim q = (From item In currentDt
                 Where Convert.ToString(item("INVOICETYPE")) <> CONST_INVOICETYPE_NOTDONE _
                  And (Convert.ToString(item("CHECK_AP_DATE")) <> Convert.ToString(item("CHECK_AP_PRVDATE")) _
                    Or Convert.ToString(item("CHECK_S_DATE")) <> Convert.ToString(item("CHECK_S_PRVDATE")) _
                    Or Convert.ToString(item("CHECK_AC")) <> Convert.ToString(item("CHECK_AC_PRV")) _
                    Or Convert.ToString(item("INVOICENO")) <> Convert.ToString(item("INVOICENO_PRV")) _
                    Or Convert.ToString(item("CUSTOMERNAME")) <> Convert.ToString(item("CUSTOMERNAME_PRV")) _
                    Or Convert.ToString(item("REMARK")) <> Convert.ToString(item("REMARK_PRV")) _
                    Or Convert.ToString(item("INVOICEAMOUNT")) <> Convert.ToString(item("INVOICEAMOUNT_PRV")) _
                    Or Convert.ToString(item("TAXAMT")) <> Convert.ToString(item("TAXAMT_PRV")) _
                    Or Convert.ToString(item("NONTAXAMT")) <> Convert.ToString(item("NONTAXAMT_PRV"))
                    ))
        If q.Any Then
            retDt = q.CopyToDataTable
        End If

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
        Dim checkMapId As String = mapId
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
        COA0026FieldCheck.MAPID = checkMapId

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
                    CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, dummyLabelObj)
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
    ''' 請求書テーブル追加
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub InsertInvoice(dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#)

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
            sqlStat.AppendLine("  ,INVOICEAMOUNT")
            sqlStat.AppendLine("  ,TANK")
            sqlStat.AppendLine("  ,CREATEUSER")
            sqlStat.AppendLine("  ,CREATEDATE")
            sqlStat.AppendLine("  ,INVOICETYPE")
            sqlStat.AppendLine("  ,CUSTOMERNAME")
            'sqlStat.AppendLine("  ,WORK_C3")
            sqlStat.AppendLine("  ,TAXAMT")
            sqlStat.AppendLine("  ,NONTAXAMT")
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
            sqlStat.AppendLine("  ,@INVOICEAMOUNT")
            sqlStat.AppendLine("  ,@TANK")
            sqlStat.AppendLine("  ,@UPDUSER")
            sqlStat.AppendLine("  ,@INITYMD")
            sqlStat.AppendLine("  ,@INVOICETYPE")
            sqlStat.AppendLine("  ,@CUSTOMERNAME")
            'sqlStat.AppendLine("  ,@WORK_C3")
            sqlStat.AppendLine("  ,@TAXAMT")
            sqlStat.AppendLine("  ,@NONTAXAMT")
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
                    .Add("@INVOICENOSUB", SqlDbType.Int).Value = dr.Item("INVOICENOSUB")
                    .Add("@STYMD", SqlDbType.Date).Value = procDate
                    .Add("@INVOICENO", SqlDbType.NVarChar).Value = dr.Item("INVOICENO")
                    .Add("@INCTORICODE", SqlDbType.NVarChar).Value = dr.Item("INCTORICODE")
                    .Add("@REMARK", SqlDbType.NVarChar).Value = dr.Item("REMARK")
                    .Add("@OUTLANGUAGE", SqlDbType.NVarChar).Value = dr.Item("OUTLANGUAGE")
                    .Add("@INVOICEDATE", SqlDbType.NVarChar).Value = procDate.ToString("yyyy/MM/dd")
                    .Add("@DRAFTOUTPUT", SqlDbType.Int).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("DRAFTOUTPUT")))
                    .Add("@ORIGINALOUTPUT", SqlDbType.Int).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("ORIGINALOUTPUT")))
                    .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = dr.Item("ACCCURRENCYSEGMENT")
                    .Add("@AMOUNT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMOUNT")))
                    .Add("@INVOICEAMOUNT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("INVOICEAMOUNT")))
                    .Add("@TANK", SqlDbType.Int).Value = 0
                    .Add("@INVOICETYPE", SqlDbType.NVarChar).Value = dr.Item("INVOICETYPE")
                    .Add("@CUSTOMERNAME", SqlDbType.NVarChar).Value = dr.Item("CUSTOMERNAME")
                    '.Add("@WORK_C3", SqlDbType.NVarChar).Value = dr.Item("WORK_C3")
                    .Add("@TAXAMT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("TAXAMT")))
                    .Add("@NONTAXAMT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("NONTAXAMT")))
                    '.Add("@WORK_F3", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("WORK_F3")))
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
    Private Sub UpdateInvoiceInfo(dr As DataRow, Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#, Optional totalInvoice As Double = 0.0)

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
            sqlStat.AppendLine("          ,APPROVEUSER")
            sqlStat.AppendLine("          ,APPROVEDATE")
            sqlStat.AppendLine("          ,SENDUSER")
            sqlStat.AppendLine("          ,SENDDATE")
            sqlStat.AppendLine("          ,ACCUSER")
            sqlStat.AppendLine("          ,ACCDATE")
            sqlStat.AppendLine("          ,INVOICETYPE")
            sqlStat.AppendLine("          ,CUSTOMERNAME")
            sqlStat.AppendLine("          ,WORK_C3")
            sqlStat.AppendLine("          ,TAXAMT")
            sqlStat.AppendLine("          ,NONTAXAMT")
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
            sqlStat.AppendLine("          ,@INVOICENO")
            sqlStat.AppendLine("          ,CUSTOMERCODE")
            sqlStat.AppendLine("          ,@REMARK")
            sqlStat.AppendLine("          ,OUTLANGUAGE")
            sqlStat.AppendLine("          ,INVOICEDATE")
            sqlStat.AppendLine("          ,DRAFTOUTPUT")
            sqlStat.AppendLine("          ,ORIGINALOUTPUT")
            sqlStat.AppendLine("          ,ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("          ,AMOUNT")
            sqlStat.AppendLine("          ,@INVOICEAMOUNT")
            sqlStat.AppendLine("          ,TANK")
            sqlStat.AppendLine("          ,CREATEUSER")
            sqlStat.AppendLine("          ,CREATEDATE")
            sqlStat.AppendLine("          ,@APPROVEUSER")
            sqlStat.AppendLine("          ,@APPROVEDATE")
            sqlStat.AppendLine("          ,@SENDUSER")
            sqlStat.AppendLine("          ,@SENDDATE")
            sqlStat.AppendLine("          ,@ACCUSER")
            sqlStat.AppendLine("          ,@ACCDATE")
            sqlStat.AppendLine("          ,INVOICETYPE")
            sqlStat.AppendLine("          ,@CUSTOMERNAME")
            sqlStat.AppendLine("          ,WORK_C3")
            sqlStat.AppendLine("          ,@TAXAMT")
            sqlStat.AppendLine("          ,@NONTAXAMT")
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

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)

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
                Dim paramInvoiceNo = sqlCmd.Parameters.Add("@INVOICENO", SqlDbType.NVarChar)
                Dim paramCustomerName = sqlCmd.Parameters.Add("@CUSTOMERNAME", SqlDbType.NVarChar)
                Dim paramRemark = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar)
                Dim paramInvoiceAmount = sqlCmd.Parameters.Add("@INVOICEAMOUNT", SqlDbType.NVarChar)
                Dim paramTaxAmt = sqlCmd.Parameters.Add("@TAXAMT", SqlDbType.NVarChar)
                Dim paramNonTaxAmt = sqlCmd.Parameters.Add("@NONTAXAMT", SqlDbType.NVarChar)

                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG_Y", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVname
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                'paramCustomer.Value = Convert.ToString(dr.Item("CUSTOMERCODE"))
                paramToriCode.Value = Convert.ToString(dr.Item("INCTORICODE"))
                paramInvoiceMonth.Value = Convert.ToString(dr.Item("INVOICEMONTH"))
                paramInvoiceMonthSub.Value = Convert.ToInt32(dr.Item("INVOICENOSUB"))

                If Convert.ToString(dr.Item("CHECK_AP_DATE")) <> Convert.ToString(dr.Item("CHECK_AP_PRVDATE")) Then
                    If Convert.ToString(dr.Item("CHECK_AP")) = CONST_CHECK_ON Then
                        paramApproveU.Value = COA0019Session.USERID
                        paramApproveD.Value = procDate
                    Else
                        paramApproveU.Value = ""
                        paramApproveD.Value = CONST_INITDATE
                    End If
                Else
                    paramApproveU.Value = dr.Item("APPROVEUSER")
                    paramApproveD.Value = dr.Item("CHECK_AP_PRVDATE")
                End If
                If Convert.ToString(dr.Item("CHECK_S_DATE")) <> Convert.ToString(dr.Item("CHECK_S_PRVDATE")) Then
                    If Convert.ToString(dr.Item("CHECK_S")) = CONST_CHECK_ON Then
                        paramSendU.Value = COA0019Session.USERID
                        paramSendD.Value = procDate
                    Else
                        paramSendU.Value = ""
                        paramSendD.Value = CONST_INITDATE
                    End If
                Else
                    paramSendU.Value = dr.Item("SENDUSER")
                    paramSendD.Value = dr.Item("CHECK_S_PRVDATE")
                End If
                If Convert.ToString(dr.Item("CHECK_AC_DATE")) <> Convert.ToString(dr.Item("CHECK_AC_PRVDATE")) Then
                    If Convert.ToString(dr.Item("CHECK_AC")) = CONST_CHECK_ON Then
                        paramAccU.Value = COA0019Session.USERID
                        paramAccD.Value = procDate
                    Else
                        paramAccU.Value = ""
                        paramAccD.Value = CONST_INITDATE
                    End If
                Else
                    paramAccU.Value = dr.Item("ACCUSER")
                    paramAccD.Value = dr.Item("CHECK_AC_PRVDATE")
                End If

                If Convert.ToString(dr.Item("INVOICENO")) <> Convert.ToString(dr.Item("INVOICENO_PRV")) Then
                    paramInvoiceNo.Value = Convert.ToString(dr.Item("INVOICENO"))
                Else
                    paramInvoiceNo.Value = Convert.ToString(dr.Item("INVOICENO_PRV"))
                End If
                If Convert.ToString(dr.Item("CUSTOMERNAME")) <> Convert.ToString(dr.Item("CUSTOMERNAME_PRV")) Then
                    paramCustomerName.Value = Convert.ToString(dr.Item("CUSTOMERNAME"))
                Else
                    paramCustomerName.Value = Convert.ToString(dr.Item("CUSTOMERNAME_PRV"))
                End If
                If Convert.ToString(dr.Item("REMARK")) <> Convert.ToString(dr.Item("REMARK_PRV")) Then
                    paramRemark.Value = Convert.ToString(dr.Item("REMARK"))
                Else
                    paramRemark.Value = Convert.ToString(dr.Item("REMARK_PRV"))
                End If
                If Convert.ToString(dr.Item("INVOICEAMOUNT")) <> Convert.ToString(dr.Item("INVOICEAMOUNT_PRV")) Then
                    paramInvoiceAmount.Value = Convert.ToDouble(dr.Item("INVOICEAMOUNT"))
                Else
                    paramInvoiceAmount.Value = Convert.ToDouble(dr.Item("INVOICEAMOUNT_PRV"))
                End If
                If Convert.ToString(dr.Item("TAXAMT")) <> Convert.ToString(dr.Item("TAXAMT_PRV")) Then
                    paramTaxAmt.Value = Convert.ToDouble(dr.Item("TAXAMT"))
                Else
                    paramTaxAmt.Value = Convert.ToDouble(dr.Item("TAXAMT_PRV"))
                End If
                If Convert.ToString(dr.Item("NONTAXAMT")) <> Convert.ToString(dr.Item("NONTAXAMT_PRV")) Then
                    paramNonTaxAmt.Value = Convert.ToDouble(dr.Item("NONTAXAMT"))
                Else
                    paramNonTaxAmt.Value = Convert.ToDouble(dr.Item("NONTAXAMT_PRV"))
                End If

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
        'そもそも画面表示データがない状態の場合はそのまま終了
        If ViewState("DISPLAY_LINECNT_LIST") Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If
        Dim displayLineCnt = DirectCast(ViewState("DISPLAY_LINECNT_LIST"), List(Of Integer))
        Dim COA0021ListTable As New COA0021ListTable
        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateDataTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                Return COA0021ListTable.ERR

            End If
        Else
            dt = Me.SavedDt
        End If

        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String)
        Dim txtFieldIdList As New Dictionary(Of String, String)
        '入力値保持用のフィールド名設定
        fieldIdList.Add("DELCHK", objChkPrifix)
        fieldIdList.Add("CHECK_AP", objChkPrifix)
        fieldIdList.Add("CHECK_S", objChkPrifix)
        fieldIdList.Add("CHECK_AC", objChkPrifix)
        If Me.hdnThisMapVariant.Value = "Management" Then
            txtFieldIdList.Add("INVOICENO", objTxtPrifix)
            txtFieldIdList.Add("CUSTOMERNAME", objTxtPrifix)
            txtFieldIdList.Add("REMARK", objTxtPrifix)
            txtFieldIdList.Add("INVOICEAMOUNT", objTxtPrifix)
            txtFieldIdList.Add("TAXAMT", objTxtPrifix)
            txtFieldIdList.Add("NONTAXAMT", objTxtPrifix)
        End If
        Dim procDate As Date = Now

        ' とりあえず右データエリアを対象
        For Each i In displayLineCnt
            Dim targetRow = (From rowItem In dt Where Convert.ToString(rowItem("LINECNT")) = i.ToString)
            'Dim dr As DataRow = dt.Rows(i - 1)
            '対象行の通貨取得
            Dim currency As String = targetRow(0).Item("ACCCURRENCYSEGMENT").ToString
            Dim ci As System.Globalization.CultureInfo
            If currency = "USD" Then
                ci = New System.Globalization.CultureInfo("en-US")
            Else
                ci = New System.Globalization.CultureInfo("ja-JP")
            End If

            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                Dim linePos As String = i.ToString
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                End If

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

                'targetRow(0).Item(fieldId.Key) = displayValue
            Next
            For Each fieldId As KeyValuePair(Of String, String) In txtFieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                Dim linePos As String = i.ToString
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                End If

                If {"INVOICEAMOUNT", "TAXAMT", "NONTAXAMT"}.Contains(fieldId.Key) Then
                    Dim val As String = displayValue
                    val = StrConv(val.Trim, VbStrConv.Narrow)
                    Dim tmpVal As Decimal
                    If val <> "" Then
                        'スタイルとカルチャを変更して変換する
                        If Decimal.TryParse(val, System.Globalization.NumberStyles.Any, ci, tmpVal) = True Then
                            val = tmpVal.ToString
                        Else
                            val = "0"
                        End If
                    Else
                        val = "0"
                    End If
                    displayValue = val
                End If

                targetRow(0).Item(fieldId.Key) = displayValue
            Next
        Next

        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0021ListTable.ERR
        End If
        Me.SavedDt = dt

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
        sqlStat.AppendLine("      ,II.TAXAMT ")
        sqlStat.AppendLine("      ,II.NONTAXAMT ")
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
        sqlStat.AppendFormat("      ,ISNULL(MTORI.{0},II.CUSTOMERNAME) AS 'CUSTOMERNAME'", textCustomerTblField).AppendLine()
        'sqlStat.AppendLine("      ,0 AS AMOUNT ")
        sqlStat.AppendLine("      ,'' AS DELCHK ")
        sqlStat.AppendLine("      ,'1' AS DELCHK_DISP ")
        sqlStat.AppendLine("      ,II.UPDYMD as 'DELCHK_DATE'")
        sqlStat.AppendLine("      ,'' as 'DELCHK_PRV'")
        sqlStat.AppendLine("      ,II.UPDYMD as 'DELCHK_PRVDATE'")
        sqlStat.AppendLine("      ,ISNULL(APPLY.APPROVALTYPE,'0') as 'APPROVALTYPE'")
        sqlStat.AppendLine("      ,II.INVOICETYPE as 'INVOICETYPE'")
        sqlStat.AppendLine("      ,II.INVOICENO as 'INVOICENO_PRV' ")
        sqlStat.AppendFormat("      ,ISNULL(MTORI.{0},II.CUSTOMERNAME) AS 'CUSTOMERNAME_PRV'", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,II.REMARK as 'REMARK_PRV' ")
        sqlStat.AppendLine("      ,II.INVOICEAMOUNT as 'INVOICEAMOUNT_PRV' ")
        sqlStat.AppendLine("      ,II.TAXAMT as 'TAXAMT_PRV' ")
        sqlStat.AppendLine("      ,II.NONTAXAMT as 'NONTAXAMT_PRV' ")
        sqlStat.AppendFormat("  FROM {0} II", CONST_TBL_INVOICEINFO).AppendLine()
        sqlStat.AppendFormat("    LEFT JOIN {0} MTORI", CONST_TBL_TORI).AppendLine()
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
            'THOMAS外は対象外
            sqlStat.AppendFormat("   AND II.INVOICETYPE <> '{0}'", CONST_INVOICETYPE_OTHER).AppendLine()
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
            sqlStat.AppendLine("      ,'－－未選択明細数(SHIP済)－－   ' + CONVERT(varchar,COUNT(*))    AS REMARK ")
            sqlStat.AppendLine("      ,''                        AS OUTLANGUAGE ")
            sqlStat.AppendLine("      ,''                        AS INVOICEDATE ")
            sqlStat.AppendLine("      ,0                         AS DRAFTOUTPUT ")
            sqlStat.AppendLine("      ,0                         AS ORIGINALOUTPUT ")
            sqlStat.AppendLine("      ,''               AS ACCCURRENCYSEGMENT ")
            sqlStat.AppendLine("      ,0                AS AMOUNT ")
            sqlStat.AppendLine("      ,0                AS INVOICEAMOUNT ")
            sqlStat.AppendLine("      ,0                AS TAXAMT ")
            sqlStat.AppendLine("      ,0                AS NONTAXAMT ")
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
            sqlStat.AppendFormat("      ,'{0}'            AS 'INVOICETYPE'", CONST_INVOICETYPE_NOTDONE).AppendLine()
            sqlStat.AppendLine("      ,''               AS INVOICENO_PRV ")
            sqlStat.AppendLine("      ,''               AS CUSTOMERNAME_PRV")
            sqlStat.AppendLine("      ,''               AS REMARK_PRV ")
            sqlStat.AppendLine("      ,0                AS INVOICEAMOUNT_PRV ")
            sqlStat.AppendLine("      ,0                AS TAXAMT_PRV ")
            sqlStat.AppendLine("      ,0                AS NONTAXAMT_PRV ")

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
            sqlStat.AppendLine("      ,'－－未選択明細数(LEASE)－－   ' + CONVERT(varchar,COUNT(*))    AS REMARK ")
            sqlStat.AppendLine("      ,''                        AS OUTLANGUAGE ")
            sqlStat.AppendLine("      ,''                        AS INVOICEDATE ")
            sqlStat.AppendLine("      ,0                         AS DRAFTOUTPUT ")
            sqlStat.AppendLine("      ,0                         AS ORIGINALOUTPUT ")
            sqlStat.AppendLine("      ,''               AS ACCCURRENCYSEGMENT ")
            sqlStat.AppendLine("      ,0                AS AMOUNT ")
            sqlStat.AppendLine("      ,0                AS INVOICEAMOUNT ")
            sqlStat.AppendLine("      ,0                AS TAXAMT ")
            sqlStat.AppendLine("      ,0                AS NONTAXAMT ")
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
            sqlStat.AppendFormat("      ,'{0}'              AS 'INVOICETYPE'", CONST_INVOICETYPE_NOTDONE).AppendLine()
            sqlStat.AppendLine("      ,''               AS INVOICENO_PRV ")
            sqlStat.AppendLine("      ,''               AS CUSTOMERNAME_PRV")
            sqlStat.AppendLine("      ,''               AS REMARK_PRV ")
            sqlStat.AppendLine("      ,0                AS INVOICEAMOUNT_PRV ")
            sqlStat.AppendLine("      ,0                AS TAXAMT_PRV ")
            sqlStat.AppendLine("      ,0                AS NONTAXAMT_PRV ")

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
    Public Sub ListRowDbClick()
        Dim notSavedData = GetModifiedDataTable()
        If Not (notSavedData Is Nothing OrElse notSavedData.Rows.Count = 0) Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, pageObject:=Me, submitButtonId:="btnShowDetailsOk")
            Return
        End If
        '確認メッセージを表示しない場合は終了
        btnShowDetailsOk_Click()

    End Sub
    ''' <summary>
    ''' リスト行ダブルクリック時イベント後OK
    ''' </summary>
    Public Sub btnShowDetailsOk_Click()
        Dim rowIdString As String = Me.hdnListDBclick.Value
        Me.hdnListDBclick.Value = ""
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
        If Convert.ToString(selectedRow.Item("INVOICETYPE")) <> CONST_INVOICETYPE_NOTDONE AndAlso
            Convert.ToString(selectedRow.Item("INVOICETYPE")) <> CONST_INVOICETYPE_OTHER Then

            '未選択明細行　又は　THOMAS外明細
            '以外詳細画面に遷移

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
        '表示データ件数取得
        Dim DataCnt As Integer = 0                  '(絞り込み後)有効Data数
        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル
            dt = CreateDataTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return
            End If
            Me.SavedDt = dt
        Else
            dt = Me.SavedDt
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

        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList

            'For Each targetCheckBoxId As String In {"DELCHK", "CHECK_AP", "CHECK_S", "CHECK_AC"}

            '    '申請チェックボックスの加工
            '    Dim targetCheckBoxLineCnt = (From dr As DataRow In listData
            '                                 Where Convert.ToString(dr.Item(targetCheckBoxId)) <> ""
            '                                 Select Convert.ToInt32(dr.Item("LINECNT")))
            '    For Each lineCnt As Integer In targetCheckBoxLineCnt
            '        Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & targetCheckBoxId & lineCnt.ToString
            '        Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
            '        If tmpObj IsNot Nothing Then
            '            Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
            '            chkObj.Checked = True
            '        End If
            '    Next

            'Next

            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If
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
            'Me.lblInvoiceNew.Visible = False
            Me.lblInvoiceNew.Visible = True
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
        Dim targetPanel As Panel = Me.WF_LISTAREA
        Dim dicDisplayRows As New Dictionary(Of Integer, DataRow)
        Dim dispLineCnt As New List(Of Integer)
        If ViewState("DISPLAY_LINECNT_LIST") IsNot Nothing Then
            dispLineCnt = DirectCast(ViewState("DISPLAY_LINECNT_LIST"), List(Of Integer))
            dicDisplayRows = (From itemRow In Me.SavedDt Where dispLineCnt.Contains(CInt(itemRow("LINECNT"))) Select New KeyValuePair(Of Integer, DataRow)(CInt(itemRow("LINECNT")), itemRow)).ToDictionary(Function(x) x.Key, Function(x) x.Value)
        End If

        Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
        If rightDataDiv.HasControls = False _
           OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
           OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
            Return
        End If
        Dim rightHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HR"), Panel)
        Dim rightHeaderTable As Table = DirectCast(rightHeaderDiv.Controls(0), Table)
        Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"DELCHK", ""}, {"INVOICENO", ""},
                                                                         {"CUSTOMERNAME", ""}, {"REMARK", ""}, {"ACCCURRENCYSEGMENT", ""},
                                                                         {"INVOICEAMOUNT", ""}, {"TAXAMT", ""}, {"NONTAXAMT", ""}}
        With Nothing '右ヘッダーの列名より対象の列番号を取得
            Dim headerTableRow = rightHeaderTable.Rows(0)
            If headerTableRow.Cells.Count = 0 Then
                Return 'ヘッダー列に列が存在しない場合は終了
            End If
            'セル名称より列番号を取得
            Dim maxCellIndex = headerTableRow.Cells.Count - 1
            For cellIndex = 0 To maxCellIndex
                Dim targetCell As TableCell = headerTableRow.Cells(cellIndex)
                If targetCell.Attributes("cellfiedlname") IsNot Nothing AndAlso
                    dicColumnNameToNo.ContainsKey(targetCell.Attributes("cellfiedlname")) Then
                    dicColumnNameToNo(targetCell.Attributes("cellfiedlname")) = cellIndex.ToString
                End If
            Next
        End With '列番号取得完了

        Dim leftHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HL"), Panel)
        Dim leftHeaderTable As Table = DirectCast(leftHeaderDiv.Controls(0), Table)
        Dim dicLeftColumnNameToNo As New Dictionary(Of String, String) From {{"ACTION", ""}}

        With Nothing '右ヘッダーの列名より対象の列番号を取得
            Dim headerTableRow = leftHeaderTable.Rows(0)
            If headerTableRow.Cells.Count = 0 Then
                Return 'ヘッダー列に列が存在しない場合は終了
            End If
            'セル名称より列番号を取得
            Dim maxCellIndex = headerTableRow.Cells.Count - 1
            For cellIndex = 0 To maxCellIndex
                Dim targetCell As TableCell = headerTableRow.Cells(cellIndex)
                If targetCell.Attributes("cellfiedlname") IsNot Nothing AndAlso
               dicLeftColumnNameToNo.ContainsKey(targetCell.Attributes("cellfiedlname")) Then
                    dicLeftColumnNameToNo(targetCell.Attributes("cellfiedlname")) = cellIndex.ToString
                End If
            Next
        End With '列番号取得完了

        Dim rightDataTable As Table = DirectCast(rightDataDiv.Controls(0), Table)
        Dim leftDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DL"), Panel)
        Dim leftDataTable As Table = DirectCast(leftDataDiv.Controls(0), Table) '1列目LINECNT 、3列目のSHOW DELETEカラム取得用

        '******************************
        'レンダリング行のループ
        '******************************
        Dim disableRow As Boolean = False
        Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        Dim displayRow As DataRow = Nothing
        For i = 0 To rowCnt
            disableRow = False
            Dim tbrRight As TableRow = rightDataTable.Rows(i)

            Dim tbrLeft As TableRow = leftDataTable.Rows(i)
            Dim lineCnt As String = tbrLeft.Cells(0).Text
            displayRow = Nothing
            If dicDisplayRows.ContainsKey(CInt(lineCnt)) Then
                displayRow = dicDisplayRows(CInt(lineCnt))
            End If
            If Me.hdnThisMapVariant.Value = "Management" Then
                If Convert.ToString(displayRow.Item("INVOICETYPE")).Equals(CONST_INVOICETYPE_NOTDONE) Then

                    For Each fieldName As String In {"INVOICENO", "CUSTOMERNAME", "REMARK", "INVOICEAMOUNT", "TAXAMT", "NONTAXAMT"}
                        If dicColumnNameToNo(fieldName) <> "" Then
                            With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                                If Not .Text.Contains("readonly=") Then
                                    .Text = .Text.Replace(">", " readonly=""readonly"" />")
                                End If
                            End With
                        End If
                    Next
                    tbrRight.CssClass = "InvoiceNotDone"
                ElseIf Convert.ToString(displayRow.Item("INVOICETYPE")).Equals(CONST_INVOICETYPE_OTHER) Then
                    'For Each fieldName As String In {"INVOICENO", "CUSTOMERNAME", "REMARK", "INVOICEAMOUNT", "TAXAMT", "NONTAXAMT"}
                    '    If dicColumnNameToNo(fieldName) <> "" Then
                    '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                    '            If Not .Text.Contains("readonly=") Then
                    '                .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                    '                .Style.Add("pointer-events", "none")
                    '            End If
                    '        End With
                    '    End If
                    'Next
                    tbrRight.CssClass = "InvoiceOther"
                Else
                    For Each fieldName As String In {"INVOICENO", "CUSTOMERNAME", "REMARK"}
                        If dicColumnNameToNo(fieldName) <> "" Then
                            With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                                If Not .Text.Contains("readonly=") Then
                                    .Text = .Text.Replace(">", " readonly=""readonly"" />")
                                End If
                            End With
                        End If
                    Next
                    tbrRight.CssClass = "InvoiceNormal"
                End If

                '承認済の時は無効（但し、既に入力不可でない場合）
                If Date.Parse(Convert.ToString(displayRow.Item("APPROVEDATE"))).ToString("yyyy/MM/dd") <> "1900/01/01" Then
                    For Each fieldName As String In {"INVOICENO", "CUSTOMERNAME", "REMARK", "INVOICEAMOUNT", "TAXAMT", "NONTAXAMT"}
                        If dicColumnNameToNo(fieldName) <> "" Then
                            With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                                If Not .Text.Contains("readonly=") Then
                                    .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                                    .Style.Add("pointer-events", "none")
                                End If
                            End With
                        End If
                    Next
                End If
            Else

            End If
        Next

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
            'Me.btnDel.Visible = False
            Me.btnDel.Visible = True
            'Me.btnInvoiceNew.Visible = False
            'Me.lblInvoiceNew.Visible = False
            Me.lblInvoiceNew.Visible = True

            ' チェックボックス制御
            Dim COA0021ListTable As New COA0021ListTable
            Dim dt As DataTable = Me.SavedDt

            Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
            Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
            For Each dr As DataRow In dt.Rows

                If Convert.ToString(dr.Item("INVOICETYPE")) = CONST_INVOICETYPE_NOTDONE _
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

                If Convert.ToString(dr.Item("INVOICETYPE")) <> CONST_INVOICETYPE_OTHER Then
                    'THOMAS外 以外 
                    ' 削除は非表示
                    Dim chkId As String = "chkWF_LISTAREADELCHK" & Convert.ToString(dr.Item("LINECNT"))
                    Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
                    chk.Visible = False
                    dr.Item("DELCHK_DISP") = "0"
                End If
            Next

            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021saveListTable()
            Me.SavedDt = dt
        Else

            'ボタン
            Me.btnSave.Visible = False
            Me.btnExcelDownload.Visible = False

        End If



    End Sub

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
    ''' <summary>
    ''' 請求書No取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetNewInvoice(Optional sqlCon As SqlConnection = Nothing, Optional tran As SqlTransaction = Nothing,
                                            Optional toriCode As String = "", Optional invoiceMonth As String = "") As String
        Dim canCloseConnect As Boolean = False
        Dim invoiceNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If

            Dim sqlStat As New Text.StringBuilder
            sqlStat.AppendLine("SELECT ISNULL(MAX(II.INVOICENOSUB), 0) + 1 AS INVOICENOSUB ")
            sqlStat.AppendFormat("FROM {0} II ", CONST_TBL_INVOICEINFO).AppendLine()
            sqlStat.AppendLine("WHERE II.INCTORICODE   = @INCTORICODE")
            sqlStat.AppendLine("AND   II.INVOICEMONTH  = @INVOICEMONTH")
            'sqlStat.AppendLine("AND   II.STYMD         <= @NOWDATE")
            'sqlStat.AppendLine("AND   II.ENDYMD        >= @NOWDATE")
            sqlStat.AppendLine("AND   II.DELFLG        <> @DELFLG")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    '.Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                    '.Add("@NOWDATE", SqlDbType.Date).Value = Now
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                    .Add("@INCTORICODE", SqlDbType.NVarChar).Value = toriCode
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

End Class