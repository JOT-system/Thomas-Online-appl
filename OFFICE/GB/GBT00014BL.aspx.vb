Imports System.Data.SqlClient
Imports System.Net
Imports BASEDLL

''' <summary>
''' B/L情報編集画面クラス
''' </summary>
Public Class GBT00014BL
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00014" '自身のMAPID
    Private Const CONST_PREPAID As String = "PREPAID"
    Private Const CONST_COLLECT As String = "COLLECT"
    Private Const CONST_FILETYPE_EXCEL As String = "XLSX"
    Private Const CONST_FILETYPE_EXCEL_OLD As String = "XLS"
    Private Const CONST_FILETYPE_PDF As String = "PDF"
    Private Const CONST_HOUSEBLISSUE_TRUE As String = "YES"
    Private Const CONST_HOUSEBLISSUE_FALSE As String = "NO (=BCO)"
    Private Const CONST_SENDTYPE_CARRIER As String = "1 CARRIER（代行）"
    Private Const CONST_SENDTYPE_SELF As String = "2 SELF（自社）"

    Private Const CONST_BOOKING_ONE_SHEET As String = "DRY(RAD)"
    Private Class CONST_REPORT_ID
        Public Class ARRIVAL_NOTICE
            Public Const PREFIX As String = "JOTAN_"

            Public Const ID As String = "JOTAN_ArrivalNotice"
        End Class
        Public Class BOOKING
            Public Const PREFIX As String = "JOTBI_"

            Public Const ONE As String = "JOTBI_BookingONE"
            Public Const OOCL As String = "JOTBI_BookingOOCL"
        End Class
        Public Class SHIPPING_INSTRUCTION
            Public Const PREFIX As String = "JOTSI_"

            Public Const ID As String = "JOTSI_ShippingInstruction"
        End Class
        Public Class DOCK_RECEIPT
            Public Const PREFIX As String = "JOTDR_"

            Public Const ONE As String = "JOTDR_BLInstructionONE"
            Public Const OOCL As String = "JOTDR_DockReceiptOOCL"
        End Class
        Public Class GATE_IN_SLIP
            Public Const PREFIX As String = "JOTGI_"

            Public Const ID As String = "JOTGI_GateInSlip"
            Public Const ONE As String = "JOTGI_GateInSlipONE"
            Public Const OOCL As String = "JOTGI_GateInSlipOOCL"
        End Class
        Public Class FORWARDING_NOTICE
            Public Const PREFIX As String = "JOTFN_"

            Public Const ID As String = "JOTFN_ForwardingNotice"
        End Class
    End Class

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' ファイル用テーブル
    ''' </summary>
    Private FileTbl As DataTable

    Private FileRow As DataRow

    Private Const CONST_DIRECTORY As String = "GBT00014BL"
    Private Const CONST_DIRECTORY_SUB As String = "BL"
    Private PreProcType As String = ""
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
            '作業用データベース設定
            '****************************************
            FileTbl = New DataTable("FILEINFO")

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '遷移元よりデータ取得
                '****************************************
                Dim ds As DataSet = New DataSet
                If Me.GetPrevDisplayInfo(ds) <> C_MESSAGENO.NORMAL Then
                    Return
                End If

                '一覧を変更可能な一時リスト変数に可能
                Dim valList As List(Of COSTITEM) = Me.CreateTemporaryInfoList(ds.Tables("ORDER_VALUE"))
                'VIEWSTATEに情報を保存
                ViewState("COSTLIST") = valList
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = "BL"
                COA0031ProfMap.COA0031GetDisplayTitle()

                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})
                    Return
                End If
                '****************************************
                '固定左ボックス選択肢
                '****************************************
                SetTermListItem()
                '****************************************
                '取得データを画面展開
                '****************************************
                'BLNo更新
                If Not SetBlNo(ds) Then
                    Return
                End If
                'オーナー情報
                SetDisplayOrderBase(ds)
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'タブによる表示切替
                '****************************************
                Me.tabBL.Attributes.Add("class", "selected")
                Me.hdnSelectedTabId.Value = Me.tabBL.ClientID
                visibleControl(Me.tabBL.ClientID)
                enabledControls()

                '明細初期表示
                'SetCostGridItem(Me.tabBL.ClientID, Me.hdnSelectedTabId.Value)
                SetCostGridItem(True)
                '初回自動計算
                CalcSummaryNetWeight()
                CalcSummaryGrossWeight()
                CalcSummaryNoOfPackage()
                '名称設定
                'txtFreightCharges_Change()
                txtBlType_Change()
                txtCarBlType_Change()
                txtPaymentPlace_Change()
                txtBlIssuePlace_Change()
                txtAnIssuePlace_Change()
                'SetCarrierCode()
                txtCarrier_Change()
                SetDemAcctListItem()
                SetEorFListItem()
                '****************************************
                'Fileタブ初期処理
                '****************************************
                FileInitDel()
                FileInitRead(Me.tabFileUp.ClientID)
                '****************************************
                '初期状態のデータを退避
                '****************************************
                ds.Tables.Add(Me.FileTbl)
                ViewState("INIT_COSTDATASET") = ds

                'メッセージ設定
                If hdnMsgId.Value <> "" Then
                    CommonFunctions.ShowMessage(hdnMsgId.Value, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                End If
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
                ' タブクリック判定
                '**********************
                Dim clickedTabCont As Control = Me.FindControl(Me.hdnSelectedTabId.Value)
                Dim clickedTabObj As HtmlControls.HtmlControl = Nothing
                If clickedTabCont IsNot Nothing Then
                    clickedTabObj = DirectCast(clickedTabCont, HtmlControls.HtmlControl)
                End If
                If clickedTabObj IsNot Nothing AndAlso clickedTabObj.Attributes("class") <> "selected" Then
                    TabClick(Me.hdnSelectedTabId.Value)
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
                ' 自動計算処理
                '**********************
                If Me.hdnCalcFunctionName.Value <> "" Then
                    Dim funcName As String = Me.hdnCalcFunctionName.Value
                    Me.hdnCalcFunctionName.Value = ""
                    Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(funcName)
                    If mi IsNot Nothing Then
                        CallByName(Me, funcName, CallType.Method, Nothing)
                    End If
                End If
                '**********************
                ' ファイルアップロード処理
                '**********************
                If Me.hdnListUpload.Value IsNot Nothing AndAlso Me.hdnListUpload.Value <> "" Then
                    If Me.hdnListUpload.Value = "XLS_LOADED" Then
                        UploadExcel()
                    ElseIf Me.hdnListUpload.Value = "FILE_LOADED" Then
                        UploadFile()
                    End If

                    Me.hdnListUpload.Value = ""
                End If
                '**********************
                ' Detail File内容表示処理
                '**********************
                If Me.hdnFileDisplay.Value IsNot Nothing AndAlso Me.hdnFileDisplay.Value <> "" Then
                    FileDisplay()
                    hdnFileDisplay.Value = ""
                End If
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

        Catch ex As Threading.ThreadAbortException
            Return
        Catch ex As Exception
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.SYSTEMADM)})
            Dim additonalErrorMessage As String = ControlChars.CrLf & "ボタン：{0}、遷移画面ID:{1}、オーダーNo(現(2回目))：{2}({3})、初回ロード分岐：{4}"
            Dim prevPageObj = Me.PreviousPage
            Dim formId As String = ""
            Dim prevOrderNo As String = ""
            If prevPageObj IsNot Nothing Then
                formId = prevPageObj.Form.ID
                If TypeOf prevPageObj Is GBT00014BL Then
                    prevOrderNo = DirectCast(prevPageObj, GBT00014BL).lblOrderNo.Text
                End If

            End If

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString() & String.Format(additonalErrorMessage, Me.hdnButtonClick.Value, formId, Me.lblOrderNo.Text, prevOrderNo, Me.PreProcType)
            COA0003LogFile.MESSAGENO = C_MESSAGENO.SYSTEMADM
            COA0003LogFile.COA0003WriteLog()

            Return
        Finally
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定

            'Close処理
            FileTbl.Dispose()
            FileTbl = Nothing
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
                    'Freight and Charges表示切替
                Case Me.vLeftFrtAndCrg.ID
                    SetFrtAndCrgListItem()

                Case Me.vLeftCountry.ID
                    SetCountryListItem()

                Case Me.vLeftBlType.ID
                    SetBlTypeListItem()

                Case Me.vLeftCarBlType.ID
                    SetCarBlTypeListItem()

                Case Me.vLeftCarrier.ID
                    SetCarrierListItem()

                Case Me.vLeftDemAcct.ID
                    SetDemAcctListItem()

                Case Me.vLeftEorF.ID
                    SetEorFListItem()

                Case Me.vLeftDelFlg.ID
                    SetDelFlgListItem()

            End Select
        End If

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        '変更チェック
        Dim ds As New DataSet
        '画面情報をデータテーブルに格納
        Dim baseDt As DataTable = CollectDisplayOrderBase()
        Dim valueDt As DataTable = CollectDisplayOrderValue()

        '各種データテーブルをデータセットに格納
        ds.Tables.AddRange({baseDt, valueDt})
        If Me.HasModifiedData(ds) = True Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, pageObject:=Me, submitButtonId:="btnBackOk")
            Return
        End If
        '戻る処理実行
        btnBackOk_Click()
    End Sub
    ''' <summary>
    ''' 戻る確認メッセージOK押下時
    ''' </summary>
    Public Sub btnBackOk_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl
        '■■■ 画面遷移先URL取得 ■■■
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
        '画面遷移実行
        Server.Transfer(COA0011ReturnUrl.URL)

    End Sub
    ''' <summary>
    ''' Excel出力ボタン押下時
    ''' </summary>
    Public Sub btnOutputExcel_Click()

        Dim ds As New DataSet

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        Dim reportMapId As String = ""

        '一旦画面費用項目をviewstateに退避
        SaveGridItem()

        '帳票出力
        Dim outUrl As String = ""

        Dim dt As DataTable = Nothing

        '画面情報を取得しデータテーブルに格納
        dt = CollectDisplayReportInfo()
        reportMapId = "GBT00014"

        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL                 'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If

            outUrl = COA0027ReportTable.URL

        End With

        '別画面でExcelを表示
        hdnPrintURL.Value = outUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

    End Sub

    ''' <summary>
    ''' 印刷ボタン押下時
    ''' </summary>
    Public Sub btnPrint_Click()
        printProc(CONST_FILETYPE_EXCEL)
    End Sub

    ''' <summary>
    ''' PDF印刷ボタン押下時
    ''' </summary>
    Public Sub btnPDFPrint_Click()
        printProc(CONST_FILETYPE_PDF)
    End Sub

    ''' <summary>
    ''' 印刷処理
    ''' </summary>
    Public Sub printProc(ByVal filetype As String)
        '右ボックスの選択レポートIDを取得
        If Me.lbRightListPrint.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightListPrint.SelectedItem.Value

        Dim reportMapId As String = "GBT00014Print"

        '帳票出力
        Dim tmpFile As String = ""
        Dim outUrl As String = ""

        'データ取得
        Dim dt As DataTable = Nothing
        Dim pageCnt As Integer = 0
        Dim atchFlg As Boolean = False
        Dim breakPageFlg As Boolean = False
        Dim attMarksText As String = ""
        Dim attMarks As New List(Of String)
        Dim attachedText As String = "AS PER ATTACHED SHEET"
        Const ATTACHLINE As Integer = 60    'Attached Sheetの出力行数

        dt = CollectDisplayReportInfoPrint(Me.hdnOrderNo.Value, Me.hdnWhichTrans.Value)

        '帳票編集
        If reportId = "B/L" Then
#Region "<< B/L >>"
            'Dim colSet = {New With {Key .col = "SHIPPERTEXT", .chara = 45, .line = 5, .itemText = "[Shipper]"},
            '              New With {Key .col = "CONSIGNEETEXT", .chara = 45, .line = 5, .itemText = "[Consignee]"},
            '              New With {Key .col = "NOTIFYTEXT", .chara = 45, .line = 5, .itemText = "[Notify Party]"},
            '              New With {Key .col = "NOTIFYCONTTEXT", .chara = 45, .line = 4, .itemText = "[Party to contact for cargo release]"},
            '              New With {Key .col = "MARKS", .chara = 25, .line = 7, .itemText = "[Marks & Numbers]"},
            '              New With {Key .col = "GOODSPKGS", .chara = 40, .line = 18, .itemText = "[Description of Goods]"},
            '              New With {Key .col = "REVENUETONS", .chara = 11, .line = 9, .itemText = "[Revenue Tons]"},
            '              New With {Key .col = "RATE", .chara = 6, .line = 9, .itemText = "[Rate]"},
            '              New With {Key .col = "PER", .chara = 6, .line = 9, .itemText = "[Per]"},
            '              New With {Key .col = "PREPAID", .chara = 13, .line = 9, .itemText = "[Prepaid]"},
            '              New With {Key .col = "COLLECT", .chara = 15, .line = 9, .itemText = "[Collect]"}}
            Dim colSet = {
                    New With {Key .col = "SHIPPERTEXT", .chara = 45, .line = 5, .itemText = "[Shipper]"},
                    New With {Key .col = "CONSIGNEETEXT", .chara = 45, .line = 5, .itemText = "[Consignee]"},
                    New With {Key .col = "NOTIFYTEXT", .chara = 45, .line = 5, .itemText = "[Notify Party]"},
                    New With {Key .col = "MARKSANDNUMBERS", .chara = 25, .line = 7, .itemText = "[Marks & Numbers]"},
                    New With {Key .col = "TANKINFO", .chara = 50, .line = 9, .itemText = "[Container No.]"},
                    New With {Key .col = "GOODSPKGS", .chara = 40, .line = 18, .itemText = "[Description of Goods]"},
                    New With {Key .col = "REVENUETONS", .chara = 11, .line = 9, .itemText = "[Revenue Tons]"},
                    New With {Key .col = "RATE", .chara = 6, .line = 9, .itemText = "[Rate]"},
                    New With {Key .col = "PER", .chara = 6, .line = 9, .itemText = "[Per]"},
                    New With {Key .col = "PREPAID", .chara = 13, .line = 9, .itemText = "[Prepaid]"},
                    New With {Key .col = "COLLECT", .chara = 15, .line = 9, .itemText = "[Collect]"},
                    New With {Key .col = "FREIGHTANDCHARGES", .chara = 18, .line = 9, .itemText = "[Freight and Charges]"}}
            For Each col In colSet

                '改ページ有無の判定
                If indentionCheck(col.chara, col.line, Convert.ToString(dt.Rows(0).Item(col.col))) Then

                    If attMarksText <> "" Then
                        attMarksText = attMarksText & vbCrLf & vbCrLf
                    End If

                    attMarksText = attMarksText & col.itemText & vbCrLf & vbCrLf & Convert.ToString(dt.Rows(0).Item(col.col))

                    dt.Rows(0).Item(col.col) = attachedText

                    breakPageFlg = True

                End If
            Next

            '改ページ有
            If breakPageFlg Then

                Dim attMarksTexts As String() = Nothing
                Dim attPageText As String
                Dim atchLine As Integer

                attMarksTexts = Split(attMarksText, vbLf)
                attPageText = ""

                For Each LineText In attMarksTexts
                    atchLine = atchLine + 1
                    If atchLine > ATTACHLINE Then
                        attMarks.Add(attPageText)
                        pageCnt = pageCnt + 1
                        atchLine = 1
                        attPageText = LineText & vbLf
                    Else
                        attPageText = attPageText & LineText & vbLf
                    End If
                Next

                If atchLine <> 0 Then
                    attMarks.Add(attPageText)
                    atchLine = 0
                    pageCnt = pageCnt + 1
                End If

            End If

            For i As Integer = 0 To pageCnt

                If atchFlg Then
                    reportId = "Attached"

                    If dt.Rows(0).Item("BLID").ToString <> "" AndAlso i = 1 Then

                        dt.Rows(0).Item("BLID") = "B/L No. : " & Convert.ToString(dt.Rows(0).Item("BLID"))
                    End If

                    If dt.Rows(0).Item("VOY").ToString <> "" AndAlso i = 1 Then

                        dt.Rows(0).Item("VOY") = "Voyage No. : " & Convert.ToString(dt.Rows(0).Item("VOY"))
                    End If

                    If dt.Rows(0).Item("VSL").ToString <> "" AndAlso i = 1 Then

                        dt.Rows(0).Item("VSL") = "Vessel Name : " & Convert.ToString(dt.Rows(0).Item("VSL"))
                    End If

                    If attMarks(i - 1) <> "" Then

                        dt.Rows(0).Item("ATTMARKS") = attMarks(i - 1)

                    End If

                End If

                With Nothing
                    Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                    COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                    COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                    'If breakPageFlg = True AndAlso atchFlg = False Then
                    If breakPageFlg = True AndAlso i <> pageCnt Then
                        COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL             'PARAM03:出力ファイル形式
                    Else
                        COA0027ReportTable.FILETYPE = filetype                         'PARAM03:出力ファイル形式
                    End If
                    COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                    If atchFlg Then
                        COA0027ReportTable.ADDSHEET = "Attached Sheet"                 'PARAM07:追記シート（任意）
                        COA0027ReportTable.ADDSHEETNO = i.ToString                     'PARAM08:追記シートNO（任意）
                        If tmpFile <> "" Then
                            COA0027ReportTable.ADDFILE = tmpFile                       'PARAM06:追記ファイル（フルパス（O_FILEpath））
                        End If
                    End If

                    COA0027ReportTable.COA0027ReportTable()

                    dt.Columns.Remove("ROWKEY")
                    dt.Columns.Remove("CELLNO")
                    dt.Columns.Remove("ROWCNT")

                    If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                    Else
                        CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                        Return
                    End If

                    atchFlg = True
                    tmpFile = COA0027ReportTable.FILEpath
                    outUrl = COA0027ReportTable.URL

                End With

            Next
#End Region
        ElseIf reportId = CONST_REPORT_ID.DOCK_RECEIPT.OOCL Then
#Region "<< DockReceipt >>"

            If dt.Rows.Count > 0 Then
                If dt.Rows(0).Item("TANKINFO").ToString <> "" Then
                    'タンク情報編集
                    Dim tankDt As DataTable = GetOrderValue(Me.hdnOrderNo.Value)

                    Dim tankTexts As String = ""
                    Dim tankText As String = ""
                    Dim tankNo As String = ""
                    Dim tankType As String = ""
                    Dim sealNo As String = ""
                    Dim netWeight As String = ""
                    Dim tareWeight As String = ""
                    Dim grossWeight As String = ""

                    For Each tank As DataRow In tankDt.Rows

                        If tankDt.Rows.Count > 12 Then
                            If tankTexts.Length <> 0 Then
                                tankTexts += vbCrLf
                            End If

                            tankText = String.Format("{0,15} / {1,5} / {2,10} / {3,10} / {4,10} / {5,10} / {6,10} / {7,10} / {8,10}",
                                                 tank.Item("TANKNO").ToString(),
                                                 "20'TK",
                                                 tank.Item("SEALNO1").ToString(),
                                                 "",
                                                 Convert.ToDecimal(tank.Item("NETWEIGHT")).ToString("#,##0"),
                                                 Convert.ToDecimal(tank.Item("TAREWEIGHT")).ToString("#,##0"),
                                                 Convert.ToDecimal(tank.Item("GROSSWEIGHT")).ToString("#,##0"),
                                                 "",
                                                 "")
                            tankTexts &= tankText
                        Else
                            If tankNo.Length <> 0 Then

                                tankNo += vbCrLf
                                tankType += vbCrLf
                                sealNo += vbCrLf
                                netWeight += vbCrLf
                                tareWeight += vbCrLf
                                grossWeight += vbCrLf
                            End If

                            tankNo &= tank.Item("TANKNO").ToString()
                            tankType &= "20'TN"
                            sealNo &= tank.Item("SEALNO1").ToString()
                            netWeight &= Convert.ToDecimal(tank.Item("NETWEIGHT")).ToString("#,##0")
                            tareWeight &= Convert.ToDecimal(tank.Item("TAREWEIGHT")).ToString("#,##0")
                            grossWeight &= Convert.ToDecimal(tank.Item("GROSSWEIGHT")).ToString("#,##0")
                        End If

                    Next

                    '改ページ有無の判定
                    If tankDt.Rows.Count > 12 Then

                        dt.Rows(0).Item("TANKINFO") = "AS PER" & vbCrLf & " ATTACHED SHEET"
                        attMarksText = "[CONTAINER NO.]" & vbCrLf & vbCrLf & tankTexts

                        breakPageFlg = True
                    Else
                        dt.Columns.Add("TYPE")
                        dt.Columns.Add("SEALNO")
                        dt.Columns.Add("CARGOWT")
                        dt.Columns.Add("TAREWT")
                        dt.Columns.Add("GROSSWT")

                        dt.Rows(0).Item("TANKINFO") = tankNo
                        dt.Rows(0).Item("TYPE") = tankType
                        dt.Rows(0).Item("SEALNO") = sealNo
                        dt.Rows(0).Item("CARGOWT") = netWeight
                        dt.Rows(0).Item("TAREWT") = tareWeight
                        dt.Rows(0).Item("GROSSWT") = grossWeight
                    End If

                End If
                dt.Rows(0).Item("DRQUANTITYPACKAGES") = dt.Rows(0).Item("DRQUANTITYPACKAGES").ToString.Replace(" ONLY", "")


                '改ページ有
                If breakPageFlg Then

                    Dim attMarksTexts As String() = Nothing
                    Dim attPageText As String
                    Dim atchLine As Integer

                    attMarksTexts = Split(attMarksText, vbLf)
                    attPageText = ""

                    For Each LineText In attMarksTexts
                        atchLine = atchLine + 1
                        If atchLine > ATTACHLINE Then
                            attMarks.Add(attPageText)
                            pageCnt = pageCnt + 1
                            atchLine = 1
                            attPageText = LineText & vbLf
                        Else
                            attPageText = attPageText & LineText & vbLf
                        End If
                    Next

                    If atchLine <> 0 Then
                        attMarks.Add(attPageText)
                        atchLine = 0
                        pageCnt = pageCnt + 1
                    End If

                End If

                For i As Integer = 0 To pageCnt

                    If atchFlg Then
                        reportId = "Attached"

                        If dt.Rows(0).Item("BLID").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("BLID") = "B/L No. : " & Convert.ToString(dt.Rows(0).Item("BLID"))
                        End If

                        If dt.Rows(0).Item("VOY").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("VOY") = "Voyage No. : " & Convert.ToString(dt.Rows(0).Item("VOY"))
                        End If

                        If dt.Rows(0).Item("VSL").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("VSL") = "Vessel Name : " & Convert.ToString(dt.Rows(0).Item("VSL"))
                        End If

                        If attMarks(i - 1) <> "" Then

                            dt.Rows(0).Item("ATTMARKS") = attMarks(i - 1)

                        End If

                    End If


                    With Nothing
                        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                        COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                        COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID

                        If breakPageFlg = True AndAlso i <> pageCnt OrElse filetype = CONST_FILETYPE_EXCEL Then
                            COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL_OLD         'PARAM03:出力ファイル形式
                        Else
                            COA0027ReportTable.FILETYPE = filetype                         'PARAM03:出力ファイル形式
                        End If
                        COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                        If atchFlg Then
                            COA0027ReportTable.ADDSHEET = "Attached Sheet"                 'PARAM07:追記シート（任意）
                            COA0027ReportTable.ADDSHEETNO = i.ToString                     'PARAM08:追記シートNO（任意）
                            If tmpFile <> "" Then
                                COA0027ReportTable.ADDFILE = tmpFile                       'PARAM06:追記ファイル（フルパス（O_FILEpath））
                            End If
                        End If

                        COA0027ReportTable.COA0027ReportTable()

                        dt.Columns.Remove("ROWKEY")
                        dt.Columns.Remove("CELLNO")
                        dt.Columns.Remove("ROWCNT")

                        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                        Else
                            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                            Return
                        End If

                        atchFlg = True
                        tmpFile = COA0027ReportTable.FILEpath
                        outUrl = COA0027ReportTable.URL

                    End With

                Next
            End If

#End Region
        ElseIf reportId = CONST_REPORT_ID.DOCK_RECEIPT.ONE Then
#Region "<< B/L Instruction >>"
            Const CONTAINERNLINE = 8         '本紙 Sheetのコンテナ出力行数
            Const CLPLINE As Integer = 22    'CLP Sheetのコンテナ出力行数
            Const CLP_REPORT_ID = "JOTDR_BLInstructionONE_CLP"

            If dt.Rows.Count > 0 Then
                Dim dtRow = dt.Rows(0)

                'タンク情報取得
                Dim tankDt As DataTable = GetOrderValue(Me.hdnOrderNo.Value)
                '改ページ有無の判定
                If tankDt.Rows.Count > CONTAINERNLINE Then
                    pageCnt = Convert.ToInt16(Math.Ceiling(tankDt.Rows.Count / CLPLINE))
                    breakPageFlg = True
                End If

                Dim dtCnt As Integer = 0
                Dim addColCnt As Integer = 0
                For i As Integer = 0 To pageCnt

                    'タンク情報編集
                    If tankDt.Rows.Count > 0 Then
                        For rowNum As Integer = 1 To CLPLINE
                            If breakPageFlg = False AndAlso rowNum > CONTAINERNLINE Then
                                Exit For
                            End If
                            If rowNum > addColCnt Then
                                addColCnt += 1
                                dt.Columns.Add("CONTAINERNO" & addColCnt)
                                dt.Columns.Add("SEALNO" & addColCnt)
                                dt.Columns.Add("SIZE" & addColCnt)
                                dt.Columns.Add("TYPE" & addColCnt)
                                dt.Columns.Add("TAREWT" & addColCnt)
                            Else
                                dtRow.Item("CONTAINERNO" & rowNum) = ""
                                dtRow.Item("SEALNO" & rowNum) = ""
                                dtRow.Item("SIZE" & rowNum) = ""
                                dtRow.Item("TYPE" & rowNum) = ""
                                dtRow.Item("TAREWT" & rowNum) = ""
                            End If

                            If breakPageFlg = True AndAlso i = 0 Then
                                '本紙は8行しか項目ない為、超える場合は全件CLPへ編集、その場合は本紙1行目にその旨を記載
                                dtRow.Item("CONTAINERNO" & 1) = attachedText
                                Exit For
                            End If
                            If dtCnt < tankDt.Rows.Count Then
                                Dim tank As DataRow = tankDt.Rows(dtCnt)
                                dtRow.Item("CONTAINERNO" & rowNum) = tank.Item("TANKNO").ToString()
                                dtRow.Item("SEALNO" & rowNum) = tank.Item("SEALNO1").ToString()
                                dtRow.Item("SIZE" & rowNum) = tank.Item("TANKTYPE").ToString()
                                dtRow.Item("TYPE" & rowNum) = "TNK"
                                If Not String.IsNullOrEmpty(tank.Item("TAREWEIGHT").ToString()) Then
                                    dtRow.Item("TAREWT" & rowNum) = Convert.ToDecimal(tank.Item("TAREWEIGHT")).ToString("#,##0")
                                End If
                                dtCnt += 1
                            End If

                        Next

                    End If


                    If atchFlg Then
                        reportId = CLP_REPORT_ID
                    End If

                    With Nothing
                        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                        COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                        COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID

                        If breakPageFlg = True AndAlso i <> pageCnt Then
                            COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL             'PARAM03:出力ファイル形式
                        Else
                            COA0027ReportTable.FILETYPE = filetype                         'PARAM03:出力ファイル形式
                        End If
                        COA0027ReportTable.ADDSHEET = "BL INSTRUCTIONS"
                        COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                        If atchFlg Then
                            COA0027ReportTable.ADDSHEET = "SUPPLIMENTAL SHEET (CLP) "                 'PARAM07:追記シート（任意）
                            COA0027ReportTable.ADDSHEETNO = i.ToString                     'PARAM08:追記シートNO（任意）
                            If tmpFile <> "" Then
                                COA0027ReportTable.ADDFILE = tmpFile                       'PARAM06:追記ファイル（フルパス（O_FILEpath））
                            End If
                        End If

                        COA0027ReportTable.COA0027ReportTable()

                        dt.Columns.Remove("ROWKEY")
                        dt.Columns.Remove("CELLNO")
                        dt.Columns.Remove("ROWCNT")

                        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                        Else
                            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                            Return
                        End If

                        atchFlg = True
                        tmpFile = COA0027ReportTable.FILEpath
                        outUrl = COA0027ReportTable.URL

                    End With

                Next
            End If

#End Region
        ElseIf reportId = CONST_REPORT_ID.GATE_IN_SLIP.ID Then
#Region "<< CY搬入票 >>"
            Const sheetName = "塩竈港運送"
            If dt.Rows.Count > 0 Then
                Dim dtRow = dt.Rows(0)

                'タンク情報取得
                Dim tankDt As DataTable = GetOrderValue(Me.hdnOrderNo.Value)
                pageCnt = tankDt.Rows.Count

                Dim newDt As DataTable = New DataTable
                newDt.Columns.Add("VSL")
                newDt.Columns.Add("VOY")
                newDt.Columns.Add("BOOKINGNO")
                newDt.Columns.Add("CONTAINERNO")
                newDt.Columns.Add("SEALNO")
                newDt.Columns.Add("GROSSWEIGHT")
                newDt.Columns.Add("PORTOFDISCHARGE")
                newDt.Columns.Add("PRODUCT")
                newDt.Columns.Add("GITERM")
                newDt.Columns.Add("EMPTY")
                newDt.Columns.Add("CHKOOCL")
                newDt.Columns.Add("CHKKMTC")
                newDt.Columns.Add("CHKONE")

                newDt.Columns.Add("OUTPUT_YEAR1")
                newDt.Columns.Add("OUTPUT_YEAR2")
                newDt.Columns.Add("OUTPUT_YEAR3")
                newDt.Columns.Add("OUTPUT_YEAR4")
                newDt.Columns.Add("OUTPUT_MONTH1")
                newDt.Columns.Add("OUTPUT_MONTH2")
                newDt.Columns.Add("OUTPUT_DAY1")
                newDt.Columns.Add("OUTPUT_DAY2")

                Dim product As String = ""
                product &= dtRow.Item("PRODUCTNAME").ToString()
                product &= " / " & dtRow.Item("IMDGCODE").ToString()
                product &= " / " & dtRow.Item("UNNO").ToString()

                '船社チェック欄
                Dim chkOOCL As String = ""
                Dim chkKMTC As String = ""
                Dim chkONE As String = ""
                Select Case dtRow.Item("CARRIER1").ToString()
                    Case "JPT00223", "JPT00227"
                        chkOOCL = "✔"
                    Case "JPC00060"
                        chkKMTC = "✔"
                    Case "JPC00083"
                        chkONE = "✔"
                    Case Else
                End Select

                Dim outputDate As String = Today().ToString("yyyyMMdd")

                atchFlg = True
                Dim dtCnt As Integer = 0
                For i As Integer = 0 To pageCnt - 1
                    newDt.Clear()
                    Dim tank = tankDt.Rows(i)
                    Dim newRow = newDt.NewRow
                    '共通情報編集
                    newRow.Item("VSL") = dtRow.Item("VSL").ToString()
                    newRow.Item("VOY") = "Voy No. " & dtRow.Item("VOY").ToString()
                    newRow.Item("BOOKINGNO") = dtRow.Item("BOOKINGNO").ToString()
                    newRow.Item("PORTOFDISCHARGE") = dtRow.Item("PORTOFDISCHARGE").ToString()
                    newRow.Item("PRODUCT") = product
                    newRow.Item("GITERM") = dtRow.Item("GITERM").ToString()
                    newRow.Item("EMPTY") = ""
                    newRow.Item("CHKOOCL") = chkOOCL
                    newRow.Item("CHKKMTC") = chkKMTC
                    newRow.Item("CHKONE") = chkONE

                    newRow.Item("OUTPUT_YEAR1") = outputDate(0)
                    newRow.Item("OUTPUT_YEAR2") = outputDate(1)
                    newRow.Item("OUTPUT_YEAR3") = outputDate(2)
                    newRow.Item("OUTPUT_YEAR4") = outputDate(3)
                    newRow.Item("OUTPUT_MONTH1") = outputDate(4)
                    newRow.Item("OUTPUT_MONTH2") = outputDate(5)
                    newRow.Item("OUTPUT_DAY1") = outputDate(6)
                    newRow.Item("OUTPUT_DAY2") = outputDate(7)


                    'タンク情報編集
                    newRow.Item("CONTAINERNO") = tank.Item("TANKNO").ToString()
                    newRow.Item("SEALNO") = tank.Item("SEALNO1").ToString()
                    newRow.Item("GROSSWEIGHT") = Convert.ToDecimal(tank.Item("GROSSWEIGHT")).ToString("#,##0")
                    newDt.Rows.Add(newRow)

                    With Nothing
                        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                        COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                        COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID

                        If breakPageFlg = True AndAlso i <> pageCnt Then
                            COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL             'PARAM03:出力ファイル形式
                        Else
                            COA0027ReportTable.FILETYPE = filetype                         'PARAM03:出力ファイル形式
                        End If
                        COA0027ReportTable.TBLDATA = newDt                                    'PARAM04:データ参照tabledata
                        If atchFlg Then
                            COA0027ReportTable.ADDSHEET = sheetName                 'PARAM07:追記シート（任意）
                            COA0027ReportTable.ADDSHEETNO = (i + 1).ToString                     'PARAM08:追記シートNO（任意）
                            If tmpFile <> "" Then
                                COA0027ReportTable.ADDFILE = tmpFile                       'PARAM06:追記ファイル（フルパス（O_FILEpath））
                            End If
                        End If

                        COA0027ReportTable.COA0027ReportTable()

                        newDt.Columns.Remove("ROWKEY")
                        newDt.Columns.Remove("CELLNO")
                        newDt.Columns.Remove("ROWCNT")

                        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                        Else
                            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                            Return
                        End If

                        atchFlg = True
                        tmpFile = COA0027ReportTable.FILEpath
                        outUrl = COA0027ReportTable.URL

                    End With

                Next
            End If

#End Region
        ElseIf reportId = "ShippingAdvice" Then
#Region "<< ShippingAdvice >>"
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
                COA0027ReportTable.FILETYPE = filetype                             'PARAM03:出力ファイル形式
                COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                COA0027ReportTable.COA0027ReportTable()

                If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                Else
                    CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If

                tmpFile = COA0027ReportTable.FILEpath
                outUrl = COA0027ReportTable.URL

            End With

#End Region
        ElseIf reportId = "ArrivalNotice" Then
#Region "<< ArrivalNotice >>"

            If dt.Rows.Count > 0 Then

                Dim colSet = {
                            New With {Key .col = "TANKNO", .chara = 11, .line = 1, .itemText = "[TANK NOS ]"}
                    }
                'New With {Key .col = "TANKNO", .chara = 59, .line = 1, .itemText = "[TANK NOS ]"}

                For Each col In colSet

                    '改ページ有無の判定
                    If indentionCheck(col.chara, col.line, Convert.ToString(dt.Rows(0).Item(col.col))) Then

                        If attMarksText <> "" Then
                            attMarksText = attMarksText & vbCrLf & vbCrLf
                        End If

                        attMarksText = attMarksText & col.itemText & vbCrLf & vbCrLf & Convert.ToString(dt.Rows(0).Item(col.col))

                        dt.Rows(0).Item(col.col) = attachedText

                        breakPageFlg = True

                    End If
                Next

                ' 取得データ補正
                If reportId = "ArrivalNotice" Then
                    dt.Rows(0).Item("CONSIGNEENAME") = "To:  " & Convert.ToString(dt.Rows(0).Item("CONSIGNEENAME"))
                Else
                    dt.Rows(0).Item("GROSSWEIGHT") = Convert.ToString(dt.Rows(0).Item("GROSSWEIGHT")) & " KGS"
                    dt.Rows(0).Item("NETWEIGHT") = Convert.ToString(dt.Rows(0).Item("NETWEIGHT")) & " KGS"

                    dt.Rows(0).Item("DEMURUSRATE1") = "USD. " & NumberFormat(Convert.ToString(dt.Rows(0).Item("DEMURUSRATE1")), "", "#,##0.00")
                    dt.Rows(0).Item("DEMURUSRATE2") = "USD. " & NumberFormat(Convert.ToString(dt.Rows(0).Item("DEMURUSRATE2")), "", "#,##0.00")
                End If

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

                    If i > 3 Then
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

                Dim freAndChg As String = Nothing

                freAndChg = dt.Rows(0).Item("FREIGHTANDCHARGES").ToString

                If freAndChg.ToUpper.Contains(CONST_PREPAID) Then
                    dt.Rows(0).Item("FREIGHTANDCHARGES") = CONST_PREPAID
                ElseIf freAndChg.ToUpper.Contains(CONST_COLLECT) Then
                    dt.Rows(0).Item("FREIGHTANDCHARGES") = CONST_COLLECT
                Else
                    dt.Rows(0).Item("FREIGHTANDCHARGES") = ""
                End If

                '改ページ有
                If breakPageFlg Then

                    Dim attMarksTexts As String() = Nothing
                    Dim attPageText As String
                    Dim atchLine As Integer

                    attMarksTexts = Split(attMarksText, vbLf)
                    attPageText = ""

                    For Each LineText In attMarksTexts
                        atchLine = atchLine + 1
                        If atchLine > ATTACHLINE Then
                            attMarks.Add(attPageText)
                            pageCnt = pageCnt + 1
                            atchLine = 1
                            attPageText = LineText & vbLf
                        Else
                            attPageText = attPageText & LineText & vbLf
                        End If
                    Next

                    If atchLine <> 0 Then
                        attMarks.Add(attPageText)
                        atchLine = 0
                        pageCnt = pageCnt + 1
                    End If

                End If

                For i As Integer = 0 To pageCnt

                    If atchFlg Then
                        reportId = "Attached"

                        If dt.Rows(0).Item("BLID").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("BLID") = "B/L No. : " & Convert.ToString(dt.Rows(0).Item("BLID"))
                        End If

                        If dt.Rows(0).Item("VOY").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("VOY") = "Voyage No. : " & Convert.ToString(dt.Rows(0).Item("VOY"))
                        End If

                        If dt.Rows(0).Item("VSL").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("VSL") = "Vessel Name : " & Convert.ToString(dt.Rows(0).Item("VSL"))
                        End If

                        If attMarks(i - 1) <> "" Then

                            dt.Rows(0).Item("ATTMARKS") = Replace(attMarks(i - 1), ",", vbCrLf)

                        End If

                    End If

                    With Nothing
                        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                        COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                        COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                        'If breakPageFlg = True AndAlso atchFlg = False Then
                        If breakPageFlg = True AndAlso i <> pageCnt Then
                            COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL             'PARAM03:出力ファイル形式
                        Else
                            COA0027ReportTable.FILETYPE = filetype                         'PARAM03:出力ファイル形式
                        End If
                        COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                        If atchFlg Then
                            COA0027ReportTable.ADDSHEET = "Attached Sheet"                 'PARAM07:追記シート（任意）
                            COA0027ReportTable.ADDSHEETNO = i.ToString                     'PARAM08:追記シートNO（任意）
                            If tmpFile <> "" Then
                                COA0027ReportTable.ADDFILE = tmpFile                       'PARAM06:追記ファイル（フルパス（O_FILEpath））
                            End If
                        End If

                        COA0027ReportTable.COA0027ReportTable()

                        dt.Columns.Remove("ROWKEY")
                        dt.Columns.Remove("CELLNO")
                        dt.Columns.Remove("ROWCNT")

                        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                        Else
                            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                            Return
                        End If

                        atchFlg = True
                        tmpFile = COA0027ReportTable.FILEpath
                        outUrl = COA0027ReportTable.URL

                    End With

                Next

            End If
#End Region
        ElseIf reportId.StartsWith(CONST_REPORT_ID.ARRIVAL_NOTICE.ID) Then
#Region "<< JOT ArrivalNotice >>"

            If dt.Rows.Count > 0 Then

                Dim colSet = {
                            New With {Key .col = "TANKNO", .chara = 11, .line = 1, .itemText = "[TANK NOS ]"}
                    }
                'New With {Key .col = "TANKNO", .chara = 59, .line = 1, .itemText = "[TANK NOS ]"}

                For Each col In colSet

                    '改ページ有無の判定
                    If indentionCheck(col.chara, col.line, Convert.ToString(dt.Rows(0).Item(col.col))) Then

                        If attMarksText <> "" Then
                            attMarksText = attMarksText & vbCrLf & vbCrLf
                        End If

                        attMarksText = attMarksText & col.itemText & vbCrLf & vbCrLf & Convert.ToString(dt.Rows(0).Item(col.col))

                        dt.Rows(0).Item(col.col) = attachedText

                        breakPageFlg = True

                    End If
                Next

                ' 取得データ補正
                If reportId = "ArrivalNotice" Then
                    dt.Rows(0).Item("CONSIGNEENAME") = "To:  " & Convert.ToString(dt.Rows(0).Item("CONSIGNEENAME"))
                Else
                    dt.Rows(0).Item("GROSSWEIGHT") = Convert.ToString(dt.Rows(0).Item("GROSSWEIGHT")) & " KGS"
                    dt.Rows(0).Item("NETWEIGHT") = Convert.ToString(dt.Rows(0).Item("NETWEIGHT")) & " KGS"

                    dt.Rows(0).Item("DEMURUSRATE1") = "USD. " & NumberFormat(Convert.ToString(dt.Rows(0).Item("DEMURUSRATE1")), "", "#,##0.00")
                    dt.Rows(0).Item("DEMURUSRATE2") = "USD. " & NumberFormat(Convert.ToString(dt.Rows(0).Item("DEMURUSRATE2")), "", "#,##0.00")
                End If

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

                    If i > 3 Then
                        Exit For
                    End If

                    dt.Rows(0).Item("CONSIGNEETEXT" & (i + 1).ToString) = consText(i)

                Next

                Dim notfText As String() = Nothing

                notfText = Split(dt.Rows(0).Item("NOTIFYTEXT1").ToString, vbCrLf)

                For i As Integer = 0 To notfText.Count - 1

                    If i > 3 Then
                        Exit For
                    End If

                    dt.Rows(0).Item("NOTIFYTEXT" & (i + 1).ToString) = notfText(i)

                Next

                Dim freAndChg As String = Nothing

                freAndChg = dt.Rows(0).Item("FREIGHTANDCHARGES").ToString

                If freAndChg.ToUpper.Contains(CONST_PREPAID) Then
                    dt.Rows(0).Item("FREIGHTANDCHARGES") = CONST_PREPAID
                ElseIf freAndChg.ToUpper.Contains(CONST_COLLECT) Then
                    dt.Rows(0).Item("FREIGHTANDCHARGES") = CONST_COLLECT
                Else
                    dt.Rows(0).Item("FREIGHTANDCHARGES") = ""
                End If

                '改ページ有
                If breakPageFlg Then

                    Dim attMarksTexts As String() = Nothing
                    Dim attPageText As String
                    Dim atchLine As Integer

                    attMarksTexts = Split(attMarksText, vbLf)
                    attPageText = ""

                    For Each LineText In attMarksTexts
                        atchLine = atchLine + 1
                        If atchLine > ATTACHLINE Then
                            attMarks.Add(attPageText)
                            pageCnt = pageCnt + 1
                            atchLine = 1
                            attPageText = LineText & vbLf
                        Else
                            attPageText = attPageText & LineText & vbLf
                        End If
                    Next

                    If atchLine <> 0 Then
                        attMarks.Add(attPageText)
                        atchLine = 0
                        pageCnt = pageCnt + 1
                    End If

                End If

                For i As Integer = 0 To pageCnt

                    If atchFlg Then
                        reportId = "Attached"

                        If dt.Rows(0).Item("BLID").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("BLID") = "B/L No. : " & Convert.ToString(dt.Rows(0).Item("BLID"))
                        End If

                        If dt.Rows(0).Item("VOY").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("VOY") = "Voyage No. : " & Convert.ToString(dt.Rows(0).Item("VOY"))
                        End If

                        If dt.Rows(0).Item("VSL").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("VSL") = "Vessel Name : " & Convert.ToString(dt.Rows(0).Item("VSL"))
                        End If

                        If attMarks(i - 1) <> "" Then

                            dt.Rows(0).Item("ATTMARKS") = Replace(attMarks(i - 1), ",", vbCrLf)

                        End If

                    End If

                    With Nothing
                        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                        COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                        COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                        'If breakPageFlg = True AndAlso atchFlg = False Then
                        If breakPageFlg = True AndAlso i <> pageCnt Then
                            COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL             'PARAM03:出力ファイル形式
                        Else
                            COA0027ReportTable.FILETYPE = filetype                         'PARAM03:出力ファイル形式
                        End If
                        COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                        If atchFlg Then
                            COA0027ReportTable.ADDSHEET = "Attached Sheet"                 'PARAM07:追記シート（任意）
                            COA0027ReportTable.ADDSHEETNO = i.ToString                     'PARAM08:追記シートNO（任意）
                            If tmpFile <> "" Then
                                COA0027ReportTable.ADDFILE = tmpFile                       'PARAM06:追記ファイル（フルパス（O_FILEpath））
                            End If
                        End If

                        COA0027ReportTable.COA0027ReportTable()

                        dt.Columns.Remove("ROWKEY")
                        dt.Columns.Remove("CELLNO")
                        dt.Columns.Remove("ROWCNT")

                        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                        Else
                            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                            Return
                        End If

                        atchFlg = True
                        tmpFile = COA0027ReportTable.FILEpath
                        outUrl = COA0027ReportTable.URL

                    End With

                Next

            End If
#End Region
        ElseIf reportId = "ShippingInstruction" OrElse reportId.StartsWith(CONST_REPORT_ID.SHIPPING_INSTRUCTION.ID) Then
#Region "<< ShippingInstruction >>"
            If dt.Rows.Count > 0 Then

                Dim colSet = {
                            New With {Key .col = "TANKNO", .chara = 11, .line = 9, .itemText = "[TANK NOS ]"}
                    }
                For Each col In colSet

                    '改ページ有無の判定
                    If indentionCheck(col.chara, col.line, Convert.ToString(dt.Rows(0).Item(col.col))) Then

                        If attMarksText <> "" Then
                            attMarksText = attMarksText & vbCrLf & vbCrLf
                        End If

                        attMarksText = attMarksText & col.itemText & vbCrLf & vbCrLf & Convert.ToString(dt.Rows(0).Item(col.col))

                        dt.Rows(0).Item(col.col) = attachedText

                        breakPageFlg = True

                    End If
                Next

                ' 取得データ補正
                Dim shipText As String() = Nothing

                shipText = Split(dt.Rows(0).Item("AGENTPOLTEXT1").ToString, vbCrLf)

                For i As Integer = 0 To shipText.Count - 1

                    If i > 2 Then
                        Exit For
                    End If

                    dt.Rows(0).Item("AGENTPOLTEXT" & (i + 1).ToString) = shipText(i)

                Next

                Dim consText As String() = Nothing

                consText = Split(dt.Rows(0).Item("AGENTPODTEXT1").ToString, vbCrLf)

                For i As Integer = 0 To consText.Count - 1

                    If i > 4 Then
                        Exit For
                    End If

                    dt.Rows(0).Item("AGENTPODTEXT" & (i + 1).ToString) = consText(i)

                Next

                Dim notfText As String() = Nothing

                notfText = Split(dt.Rows(0).Item("AGENTNOTIFYTEXT1").ToString, vbCrLf)

                For i As Integer = 0 To notfText.Count - 1

                    If i > 4 Then
                        Exit For
                    End If

                    dt.Rows(0).Item("AGENTNOTIFYTEXT" & (i + 1).ToString) = notfText(i)

                Next

                'タンク
                If dt.Rows(0).Item("TANKNO").ToString <> "" Then

                    Dim tankText As String() = Nothing
                    Dim tankNo As String = ""
                    Dim tareWeight As String = ""
                    Dim capacity As String = ""

                    If dt.Rows(0).Item("TANKNO").ToString = attachedText Then
                        tankText = Split(Split(attMarksText, vbCrLf)(2), ",")
                    Else
                        tankText = Split(dt.Rows(0).Item("TANKNO").ToString, ",")
                    End If


                    For i As Integer = 0 To tankText.Count - 1

                        If i <> 0 Then
                            tankNo += vbCrLf
                            tareWeight += vbCrLf
                            capacity += vbCrLf
                        End If

                        Dim tankDt As DataTable = GetTank(tankText(i))

                        If dt.Rows(0).Item("TANKNO").ToString = attachedText Then
                            tankNo += tankText(i) + " " + Convert.ToDecimal(tankDt.Rows(0).Item("TAREWEIGHT")).ToString("#,##0") & " KGS"
                        Else
                            tankNo += tankText(i)
                            tareWeight += Convert.ToDecimal(tankDt.Rows(0).Item("TAREWEIGHT")).ToString("#,##0") & " KGS"
                            capacity += Convert.ToDecimal(tankDt.Rows(0).Item("TANKCAPACITY")).ToString("#,##0") & " LTR"
                        End If


                    Next

                    If dt.Rows(0).Item("TANKNO").ToString = attachedText Then
                        attMarksText = Split(attMarksText, vbCrLf)(0) + vbCrLf + vbCrLf + tankNo
                    Else
                        dt.Rows(0).Item("TANKNO") = tankNo
                        dt.Rows(0).Item("SHIPTAREWEIGHT") = tareWeight
                        dt.Rows(0).Item("TANKCAPACITY") = capacity
                    End If
                End If

                Dim freAndChg As String = Nothing

                freAndChg = dt.Rows(0).Item("FREIGHTANDCHARGES").ToString

                If freAndChg.ToUpper.Contains(CONST_PREPAID) Then
                    dt.Rows(0).Item("FREIGHT") = CONST_PREPAID
                ElseIf freAndChg.ToUpper.Contains(CONST_COLLECT) Then
                    dt.Rows(0).Item("FREIGHT") = CONST_COLLECT
                Else
                    dt.Rows(0).Item("FREIGHT") = ""
                End If
                '改ページ有
                If breakPageFlg Then

                    Dim attMarksTexts As String() = Nothing
                    Dim attPageText As String
                    Dim atchLine As Integer

                    attMarksTexts = Split(attMarksText, vbLf)
                    attPageText = ""

                    For Each LineText In attMarksTexts
                        atchLine = atchLine + 1
                        If atchLine > ATTACHLINE Then
                            attMarks.Add(attPageText)
                            pageCnt = pageCnt + 1
                            atchLine = 1
                            attPageText = LineText & vbLf
                        Else
                            attPageText = attPageText & LineText & vbLf
                        End If
                    Next

                    If atchLine <> 0 Then
                        attMarks.Add(attPageText)
                        atchLine = 0
                        pageCnt = pageCnt + 1
                    End If

                End If

                For i As Integer = 0 To pageCnt

                    If atchFlg Then
                        reportId = "Attached"

                        If dt.Rows(0).Item("BLID").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("BLID") = "B/L No. : " & Convert.ToString(dt.Rows(0).Item("BLID"))
                        End If

                        If dt.Rows(0).Item("VOY").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("VOY") = "Voyage No. : " & Convert.ToString(dt.Rows(0).Item("VOY"))
                        End If

                        If dt.Rows(0).Item("VSL").ToString <> "" AndAlso i = 1 Then

                            dt.Rows(0).Item("VSL") = "Vessel Name : " & Convert.ToString(dt.Rows(0).Item("VSL"))
                        End If

                        If attMarks(i - 1) <> "" Then

                            dt.Rows(0).Item("ATTMARKS") = attMarks(i - 1)

                        End If

                    End If

                    With Nothing
                        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                        COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                        COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                        'If breakPageFlg = True AndAlso atchFlg = False Then
                        If breakPageFlg = True AndAlso i <> pageCnt Then
                            COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL             'PARAM03:出力ファイル形式
                        Else
                            COA0027ReportTable.FILETYPE = filetype                         'PARAM03:出力ファイル形式
                        End If
                        COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                        If atchFlg Then
                            COA0027ReportTable.ADDSHEET = "Attached Sheet"                 'PARAM07:追記シート（任意）
                            COA0027ReportTable.ADDSHEETNO = i.ToString                     'PARAM08:追記シートNO（任意）
                            If tmpFile <> "" Then
                                COA0027ReportTable.ADDFILE = tmpFile                       'PARAM06:追記ファイル（フルパス（O_FILEpath））
                            End If
                        End If

                        COA0027ReportTable.COA0027ReportTable()

                        dt.Columns.Remove("ROWKEY")
                        dt.Columns.Remove("CELLNO")
                        dt.Columns.Remove("ROWCNT")

                        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                        Else
                            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                            Return
                        End If

                        atchFlg = True
                        tmpFile = COA0027ReportTable.FILEpath
                        outUrl = COA0027ReportTable.URL

                    End With

                Next

            End If
#End Region
        ElseIf reportId.StartsWith(CONST_REPORT_ID.BOOKING.PREFIX) Then
#Region "<< Booking >>"
            If dt.Rows.Count > 0 Then

                Dim shipText As String() = Nothing

                shipText = Split(dt.Rows(0).Item("AGENTPOLTEXT1").ToString, vbCrLf)

                For i As Integer = 0 To shipText.Count - 1

                    If i > 2 Then
                        Exit For
                    End If

                    dt.Rows(0).Item("AGENTPOLTEXT" & (i + 1).ToString) = shipText(i)

                Next

                Dim consText As String() = Nothing

                consText = Split(dt.Rows(0).Item("AGENTPODTEXT1").ToString, vbCrLf)

                For i As Integer = 0 To consText.Count - 1

                    If i > 4 Then
                        Exit For
                    End If

                    dt.Rows(0).Item("AGENTPODTEXT" & (i + 1).ToString) = consText(i)

                Next

                Dim notfText As String() = Nothing

                notfText = Split(dt.Rows(0).Item("AGENTNOTIFYTEXT1").ToString, vbCrLf)

                For i As Integer = 0 To notfText.Count - 1

                    If i > 4 Then
                        Exit For
                    End If

                    dt.Rows(0).Item("AGENTNOTIFYTEXT" & (i + 1).ToString) = notfText(i)

                Next

                Dim freAndChg As String = Nothing

                freAndChg = dt.Rows(0).Item("FREIGHTANDCHARGES").ToString

                If freAndChg.ToUpper.Contains(CONST_PREPAID) Then
                    dt.Rows(0).Item("FREIGHT") = CONST_PREPAID
                ElseIf freAndChg.ToUpper.Contains(CONST_COLLECT) Then
                    dt.Rows(0).Item("FREIGHT") = CONST_COLLECT
                Else
                    dt.Rows(0).Item("FREIGHT") = ""
                End If

                If reportId = CONST_REPORT_ID.BOOKING.OOCL Then
                    If dt.Rows(0).Item("FREIGHT").ToString.Contains(CONST_PREPAID) Then
                        dt.Rows(0).Item("FREIGHT") = CONST_PREPAID(0) + CONST_PREPAID.Substring(1).ToLower()
                    End If
                    If dt.Rows(0).Item("PORTOFDISCHARGE").ToString.Contains("PORT KELANG") Then
                        dt.Rows(0).Item("BIPODTERMINAL") = "PORT KELANG WEST PORT"
                    End If
                ElseIf reportId = CONST_REPORT_ID.BOOKING.ONE Then
                    dt.Rows(0).Item("BIHOUSEBLISSUE") = CONST_HOUSEBLISSUE_TRUE
                    dt.Rows(0).Item("BIAMSSENDTYPE") = CONST_SENDTYPE_SELF
                    dt.Rows(0).Item("BIACISENDTYPE") = CONST_SENDTYPE_CARRIER
                End If


                With Nothing
                    Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                    COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                    COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                    COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL                 'PARAM03:出力ファイル形式
                    COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                    If reportId = CONST_REPORT_ID.BOOKING.OOCL Then
                        COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL_OLD
                    ElseIf reportId = CONST_REPORT_ID.BOOKING.ONE Then
                        COA0027ReportTable.ADDSHEET = CONST_BOOKING_ONE_SHEET
                    End If

                    COA0027ReportTable.COA0027ReportTable()

                    If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                    Else
                        CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                        Return
                    End If

                    tmpFile = COA0027ReportTable.FILEpath
                    outUrl = COA0027ReportTable.URL

                End With
            End If
#End Region
        ElseIf reportId.StartsWith(CONST_REPORT_ID.FORWARDING_NOTICE.ID) Then
#Region "<< ForwardingNotice >>"

            If dt.Rows.Count > 0 Then
                Dim dtRow As DataRow = dt.Rows(0)

                Select Case dtRow.Item("CARRIER1").ToString()
                    'OOCL
                    Case "JPT00223", "JPT00227"
                        dtRow.Item("FACYTRUCKER") = "塩竃港運株式会社 "
                        dtRow.Item("FACYTRUCKERTELFAX") = "TEL: 022-254-0948 / FAX ; 022-254-2983"
                    'KMTC
                    Case "JPC00060"
                        dtRow.Item("FACYTRUCKER") = "塩竃港運株式会社 "
                        dtRow.Item("FACYTRUCKERTELFAX") = "TEL: 022-254-0948 / FAX ; 022-254-2983"
                    'ONE
                    Case "JPC00083"
                        dtRow.Item("FACYTRUCKER") = "三陸運輸株式会社"
                        dtRow.Item("FACYTRUCKERTELFAX") = "TEL: 022-254-2101 / FAX: 022-254-2005"
                    Case Else
                End Select

                'TankNoについて
                '出力先が15行及び3カラムしかないので
                '1行に最大4つのTankNoを編集して最大60TankNo出力
                Dim tankNos As String = dtRow.Item("TANKNO").ToString
                Dim tanks As String() = Split(tankNos, ",")
                Dim rowNo As Integer = 0
                Dim colTankCnt As Integer = 0
                Dim tankText As String = ""
                For Each tankNo In tanks
                    '行先頭ではなければ"/"で連結
                    If colTankCnt <> 0 Then
                        tankText &= " / "
                    End If
                    tankText &= tankNo
                    colTankCnt += 1

                    If colTankCnt = 4 OrElse tankNo = tanks.Last Then
                        rowNo += 1
                        dt.Columns.Add("CONTAINERNO" & rowNo)
                        dtRow.Item("CONTAINERNO" & rowNo) = tankText
                        tankText = ""
                        colTankCnt = 0
                    End If
                Next

                With Nothing
                    Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                    COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                    COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                    COA0027ReportTable.FILETYPE = CONST_FILETYPE_EXCEL                 'PARAM03:出力ファイル形式
                    COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                    COA0027ReportTable.COA0027ReportTable()

                    If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                    Else
                        CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                        Return
                    End If

                    tmpFile = COA0027ReportTable.FILEpath
                    outUrl = COA0027ReportTable.URL

                End With

            End If
#End Region
        ElseIf reportId <> "B/L" AndAlso reportId <> "Attached" Then
#Region "<< その他 >>"

            With Nothing
                Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

                COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                COA0027ReportTable.FILETYPE = filetype                             'PARAM03:出力ファイル形式
                COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
                COA0027ReportTable.COA0027ReportTable()

                If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                Else
                    CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If

                tmpFile = COA0027ReportTable.FILEpath
                outUrl = COA0027ReportTable.URL

            End With
#End Region
        End If


        '別画面でExcelを表示
        hdnPrintURL.Value = outUrl

        If filetype = CONST_FILETYPE_EXCEL Or filetype = CONST_FILETYPE_EXCEL_OLD Then
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)
        ElseIf filetype = CONST_FILETYPE_PDF Then
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint()", True)
        End If

    End Sub

    ''' <summary>
    ''' FileDownloadボタン押下時
    ''' </summary>
    Public Sub btnOutputFile_Click()

        Dim currentTab As COSTITEM.CostItemGroup = Nothing
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)

        tabObjects.Add(COSTITEM.CostItemGroup.BL, Me.tabBL)
        tabObjects.Add(COSTITEM.CostItemGroup.TANK, Me.tabTank)
        tabObjects.Add(COSTITEM.CostItemGroup.OTHER, Me.tabOther)
        tabObjects.Add(COSTITEM.CostItemGroup.FileUp, Me.tabFileUp)

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected") Then
                currentTab = tabObject.Key
                Exit For
            End If
        Next

        If currentTab <> COSTITEM.CostItemGroup.FileUp Then
            Return
        End If

        '初期設定
        Dim UpDir As String = Nothing

        'アップロードファイル名を取得　＆　移動
        UpDir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\" & CONST_DIRECTORY & "\" & CONST_DIRECTORY_SUB & "\"
        UpDir = UpDir & Me.hdnOrderNo.Value & "\" & Me.hdnWhichTrans.Value & "\Update"

        '帳票出力
        Dim outUrl As String = ""
        Dim ZipDir As String = ""

        ZipDir = COA0019Session.PRINTWORKDir & "\" & COA0019Session.USERID & "\" & "ZIP"

        If System.IO.Directory.Exists(UpDir) = False Then
            System.IO.Directory.CreateDirectory(UpDir)
        End If

        If System.IO.Directory.Exists(ZipDir) = False Then
            System.IO.Directory.CreateDirectory(ZipDir)
        Else
            System.IO.Directory.Delete(ZipDir, True)
            System.IO.Directory.CreateDirectory(ZipDir)
        End If

        Dim fileCnt() As String = System.IO.Directory.GetFiles(UpDir, "*.*")
        If fileCnt.Length = 0 Then
            'メッセージ編集
            CommonFunctions.ShowMessage(C_MESSAGENO.FILENOTEXISTS, Me.lblFooterMessage, pageObject:=Me)
            Return

        End If

        Dim zipName As String = Me.hdnOrderNo.Value & ".zip"

        '圧縮実行
        System.IO.Compression.ZipFile.CreateFromDirectory(UpDir, ZipDir & "\" & zipName, System.IO.Compression.CompressionLevel.Optimal, False, Text.Encoding.GetEncoding("shift_jis"))
        outUrl = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & COA0019Session.PRINTROOTUrl & "/" & COA0019Session.USERID & "/ZIP/" & Uri.EscapeUriString(zipName)


        '別画面でExcelを表示
        hdnZipURL.Value = outUrl
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_DownLoad()", True)

        'メッセージ編集
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDOWNLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

    End Sub

    ''' <summary>
    ''' オーダー情報取得処理(帳票出力用)
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <param name="tranCls"></param>
    ''' <returns></returns>
    Private Function CollectDisplayReportInfoPrint(ByVal orderNo As String, ByVal tranCls As String, Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False

        '文言フィールド（開発中のためいったん固定
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If

        Dim retDt As DataTable = New DataTable
        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("SELECT CASE WHEN @TRANCLS = '1' THEN OB.BLID1 ELSE OB.BLID2 END AS BLID")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.SHIPPERTEXT  ELSE OB.SHIPPERTEXT2 END AS SHIPPERTEXT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.CONSIGNEETEXT  ELSE OB.CONSIGNEETEXT2 END AS CONSIGNEETEXT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.NOTIFYTEXT ELSE OB.NOTIFYTEXT2 END AS NOTIFYTEXT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.NOTIFYCONTTEXT1 ELSE OB.NOTIFYCONTTEXT2 END AS NOTIFYCONTTEXT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.FINDESTINATIONTEXT ELSE OB.FINDESTINATIONTEXT2 END AS FINDESTINATIONTEXT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.PRECARRIAGETEXT ELSE OB.PRECARRIAGETEXT2 END AS PRECARRIAGETEXT")
        'sqlStat.AppendLine("      ,ISNULL(PT1.AREANAME,'') + ' ' + FV1.VALUE3 AS PLACEOFRECIEPT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.BLRECEIPT1 ELSE OB.BLRECEIPT2 END AS PLACEOFRECIEPT")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.VSL1 ELSE OB.VSL2 END AS VSL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.VOY1 ELSE OB.VOY2 END AS VOY")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.VSL1 + CASE WHEN OB.VSL1 = '' THEN '' ELSE ' ' END + OB.VOY1 ELSE OB.VSL2 + CASE WHEN OB.VSL2 = '' THEN '' ELSE ' ' END + OB.VOY2 END AS VSLVOY")
        'sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT2.AREANAME,'') = '' THEN '' ELSE PT2.AREANAME + ', ' END + ISNULL(CT2.NAMES,'') AS PORTOFLOADING")
        'sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT3.AREANAME,'') = '' THEN '' ELSE PT3.AREANAME + ', ' END + ISNULL(CT3.NAMES,'') AS PORTOFDISCHARGE")
        'sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT4.AREANAME,'') = '' THEN '' ELSE PT4.AREANAME + ', ' END + ISNULL(CT4.NAMES,'') AS PLACEOFDELIVERY")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.BLLOADING1 ELSE OB.BLLOADING2 END AS PORTOFLOADING")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.BLDISCHARGE1 ELSE OB.BLDISCHARGE2 END AS PORTOFDISCHARGE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.BLDELIVERY1 ELSE OB.BLDELIVERY2 END AS PLACEOFDELIVERY")

        sqlStat.AppendLine("      ,OB.MARKSANDNUMBERS AS MARKSANDNUMBERS")
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

        'sqlStat.AppendLine("      ,'M3)' + CHAR(13) + CHAR(10) + REPLACE(REPLACE(CONVERT(NVARCHAR ,CONVERT(money, CASE WHEN @TRANCLS = '1' THEN OB.MEASUREMENT ELSE OB.MEASUREMENT2 END),1), '.00', ''), '0', '') AS MEASUREMENT")
        'sqlStat.AppendLine("      ,'M3)' + CHAR(13) + CHAR(10) + REPLACE(CONVERT(money, CASE WHEN @TRANCLS = '1' THEN OB.MEASUREMENT ELSE OB.MEASUREMENT2 END,1), 0.00, '') AS MEASUREMENT")
        'sqlStat.AppendLine("      ,'M3)' + CHAR(13) + CHAR(10) + (CASE WHEN CONVERT(money, CASE WHEN @TRANCLS = '1' THEN OB.MEASUREMENT ELSE OB.MEASUREMENT2 END) <= 0 THEN '' ELSE ")
        'sqlStat.AppendLine("                                                FORMAT(CONVERT(decimal(16,6), CASE WHEN @TRANCLS = '1' THEN OB.MEASUREMENT ELSE OB.MEASUREMENT2 END),'###,###,##0.######') END) AS MEASUREMENT")
        sqlStat.AppendLine("      ,'M3)' + CHAR(13) + CHAR(10) + CASE WHEN @TRANCLS = '1' THEN OB.MEASUREMENT ELSE OB.MEASUREMENT2 END AS MEASUREMENT")
        sqlStat.AppendLine("      ,CASE WHEN REPLACE(CONVERT(NVARCHAR ,CONVERT(money, CASE WHEN @TRANCLS = '1' THEN OB.DECLAREDVALUE ELSE OB.DECLAREDVALUE2 END),1), '.00', '') = '0' THEN '' ELSE ")
        sqlStat.AppendLine("                 REPLACE(CONVERT(NVARCHAR ,CONVERT(money, CASE WHEN @TRANCLS = '1' THEN OB.DECLAREDVALUE ELSE OB.DECLAREDVALUE2 END),1), '.00', '') END AS DECLAREDVALUE")
        sqlStat.AppendLine("      ,OB.FREIGHTANDCHARGES AS FREIGHTANDCHARGES")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.REVENUETONS ELSE OB.REVENUETONS2 END AS REVENUETONS")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.RATE ELSE OB.RATE2 END AS RATE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.PER ELSE OB.PER2 END AS PER")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.PREPAID ELSE OB.PREPAID2 END AS PREPAID")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.COLLECT ELSE OB.COLLECT2 END AS COLLECT")
        sqlStat.AppendLine("      ,CONVERT(NVARCHAR ,CONVERT(money, OB.EXCHANGERATE),1) AS BLEXCHANGERATE")
        sqlStat.AppendLine("      ,OB.PREPAIDAT AS PREPAIDAT")
        sqlStat.AppendLine("      ,OB.PAYABLEAT AS PAYABLEAT")
        sqlStat.AppendLine("      ,OB.LOCALCURRENCY AS LOCALCURRENCY")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.NOOFBL ELSE OB.NOOFBL2 END AS NOOFBL")
        'sqlStat.AppendLine("      ,(OB.PREPAIDAT + CASE WHEN OB.PREPAIDAT = '' THEN '' ELSE ' : ' END + CASE WHEN @TRANCLS = '1' THEN CASE OB.ETD1 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.ETD1,'yyyy-MM-dd') END ELSE CASE OB.ETD2 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.ETD2,'yyyy-MM-dd') END END) AS ISSUEDATE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.BLPLACEDATEISSUE1 ELSE OB.BLPLACEDATEISSUE2 END AS ISSUEDATE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.LDNVSL1 ELSE OB.LDNVSL2 END AS LADENVSL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.LDNPOL1 ELSE OB.LDNPOL2 END AS LADENPOL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN CASE OB.LDNDATE1 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.LDNDATE1,'yyyy-MM-dd') END ELSE CASE OB.LDNDATE2 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.LDNDATE2,'yyyy-MM-dd') END END AS LADENDATE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.LDNBY1 ELSE OB.LDNBY2 END AS LADENBY")
        sqlStat.AppendLine("      ,(SELECT COUNT(TANKSEQ) FROM GBT0007_ODR_VALUE2 WHERE ORDERNO = @ORDERNO AND TRILATERAL = @TRANCLS AND DELFLG <> @DELFLG) AS TANKCNT")
        sqlStat.AppendLine("      ,'' AS ATTMARKS")

        sqlStat.AppendLine("      ,ISNULL(TD1.NAMES,'') AS AGENTPOD")
        sqlStat.AppendLine("      ,ISNULL(TD2.NAMES,'') AS AGENTPOL")
        sqlStat.AppendLine("      ,(SELECT REPLACE(TRIM((SELECT OV.TANKNO AS [data()]  FROM GBT0007_ODR_VALUE2 OV2 ")
        sqlStat.AppendLine("        LEFT JOIN GBT0005_ODR_VALUE OV ON OV.ORDERNO  = OV2.ORDERNO ")
        sqlStat.AppendLine("        Where TRILATERAL = @TRANCLS and OV2.ORDERNO = @ORDERNO ")
        sqlStat.AppendLine("        GROUP BY TANKNO ORDER BY TANKNO FOR XML PATH('') )),' ',',')) AS TANKNO")
        sqlStat.AppendLine("      ,trim(PD.PRODUCTNAME) AS PRODUCTNAME")
        sqlStat.AppendLine("      ,CASE WHEN trim(PD.UNNO) = '' THEN 'NON' ELSE trim(PD.UNNO) END AS UNNO")
        sqlStat.AppendLine("      ,CASE WHEN trim(PD.HAZARDCLASS) = '' THEN 'NON' ELSE trim(PD.HAZARDCLASS) END AS IMDGCODE")

        sqlStat.AppendLine("      ,ISNULL(TD3.NAMES,'') AS USETYPE")

        sqlStat.AppendLine("      ,REPLACE(REPLACE(FV1.VALUE1,'-','/'),' ','') AS TERMTYPE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.VSL1 + ' ' + OB.VOY1 ELSE OB.VSL2 + ' ' + OB.VOY2 END AS VSLNAME")

        sqlStat.AppendLine("      ,ISNULL(PT2.AREANAME,'') + ', ' + ISNULL(CT2.NAMES,'') + CASE WHEN ETD.ACTDATE = '1900/01/01' THEN '' ")
        sqlStat.AppendLine("              ELSE ' on ' + FORMAT(ETD.ACTDATE,'dd') + ' ' + FV2.VALUE1 + ', ' + FORMAT(ETD.ACTDATE,'yyyy') END AS SAPORTOFLOADING ")

        sqlStat.AppendLine("      ,ISNULL(PT3.AREANAME,'') + ', ' + ISNULL(CT3.NAMES,'') + CASE WHEN ETA.ACTDATE = '1900/01/01' THEN '' ")
        sqlStat.AppendLine("              ELSE ' on ' + FORMAT(ETA.ACTDATE,'dd') + ' ' + FV3.VALUE1 + ', ' + FORMAT(ETA.ACTDATE,'yyyy') END AS SAPORTOFDISCHARGE ")

        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT4.AREANAME,'') = '' THEN '' ELSE PT4.AREANAME + ' ' + ISNULL(FV1.VALUE4,'') END AS SIPLACEOFDELIVERY")

        sqlStat.AppendLine("      ,FV1.VALUE1 AS SHIPPINGTERM")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.BLTYPE ELSE OB.BLTYPE2 END AS BLTYPE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.CARRIERBLNO ELSE OB.CARRIERBLNO2 END AS CARRIERBLNO")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.CARRIERBLTYPE ELSE OB.CARRIERBLTYPE2 END AS CARRIERBLTYPE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.SHIPPERTEXT ELSE OB.SHIPPERTEXT2 END AS SHIPPERTEXT1")
        sqlStat.AppendLine("      ,'' AS SHIPPERTEXT2")
        sqlStat.AppendLine("      ,'' AS SHIPPERTEXT3")
        sqlStat.AppendLine("      ,'' AS SHIPPERTEXT4")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.CONSIGNEETEXT ELSE OB.CONSIGNEETEXT2 END AS CONSIGNEETEXT1")
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT2")
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT3")
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT4")
        sqlStat.AppendLine("      ,'' AS CONSIGNEETEXT5")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.NOTIFYTEXT ELSE OB.NOTIFYTEXT2 END AS NOTIFYTEXT1")
        sqlStat.AppendLine("      ,'' AS NOTIFYTEXT2")
        sqlStat.AppendLine("      ,'' AS NOTIFYTEXT3")
        sqlStat.AppendLine("      ,'' AS NOTIFYTEXT4")
        sqlStat.AppendLine("      ,'' AS NOTIFYTEXT5")
        sqlStat.AppendLine("      ,(SELECT CONVERT(NVARCHAR ,CONVERT(money, SUM(GROSSWEIGHT)), 1) ")
        sqlStat.AppendLine("      FROM GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("      WHERE ORDERNO    = @ORDERNO ")
        sqlStat.AppendLine("        AND TRILATERAL = @TRANCLS ")
        sqlStat.AppendLine("        AND DELFLG    <> @DELFLG ")
        sqlStat.AppendLine("      ) AS GROSSWEIGHT")
        sqlStat.AppendLine("      ,(SELECT CONVERT(NVARCHAR ,CONVERT(money, SUM(NETWEIGHT)), 1) ")
        sqlStat.AppendLine("      FROM GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("      WHERE ORDERNO    = @ORDERNO ")
        sqlStat.AppendLine("        AND TRILATERAL = @TRANCLS ")
        sqlStat.AppendLine("        AND DELFLG    <> @DELFLG ")
        sqlStat.AppendLine("      ) AS NETWEIGHT")
        sqlStat.AppendLine("      ,OB.TIP AS TIP")
        sqlStat.AppendLine("      ,'1 TO ' + CONVERT(NVARCHAR ,OB.DEMURTO) + 'DAYS AT' AS DEMURTO1")
        sqlStat.AppendLine("      ,OB.DEMURUSRATE1 AS DEMURUSRATE1")
        sqlStat.AppendLine("      ,CONVERT(NVARCHAR ,OB.DEMURTO + 1) + ' DAYS ONWARDS AT' AS DEMURTO2")
        sqlStat.AppendLine("      ,OB.DEMURUSRATE2 AS DEMURUSRATE2")
        sqlStat.AppendLine("      , 'EX.' + ISNULL(EX.CURRENCYCODE,'') AS SALOCALCURRENCY")
        sqlStat.AppendLine("      ,REPLACE(ISNULL((SELECT TOP 1 EXSHIPRATE FROM (SELECT * FROM GBT0007_ODR_VALUE2 ")
        sqlStat.AppendLine("        WHERE TRILATERAL = @TRANCLS AND ORDERNO = @ORDERNO AND DELFLG <> @DELFLG ) AS RATE),'0'),'0','') AS EXCHANGERATE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.DEMUFORACCT ELSE OB.DEMUFORACCT2 END As DEMUACCT")

        'sqlStat.AppendFormat("      ,'TO: ' + CASE WHEN @TRANCLS = '1' THEN ISNULL(CN.{0},'') ELSE ISNULL(SP.{0},'') END AS CONSIGNEENAME", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,CASE WHEN @TRANCLS = '1' THEN ISNULL(CN.{0},'') ELSE ISNULL(SP.{0},'') END AS CONSIGNEENAME", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,CASE WHEN ETD.ACTDATE = '1900/01/01' THEN '' ELSE FORMAT(ETD.ACTDATE,'dd') + ' ' + FV2.VALUE1 + ', ' + FORMAT(ETD.ACTDATE,'yyyy') END AS ETDACTDATE ")
        sqlStat.AppendLine("      ,CASE WHEN ETA.ACTDATE = '1900/01/01' THEN '' ELSE FORMAT(ETA.ACTDATE,'dd') + ' ' + FV3.VALUE1 + ', ' + FORMAT(ETA.ACTDATE,'yyyy') END AS ETAACTDATE ")
        sqlStat.AppendLine("      ,ISNULL(PT2.AREANAME,'') + ', ' + ISNULL(CT2.NAMES,'') AS PORTOFDESTINATION")

        sqlStat.AppendLine("      ,ISNULL(TD2.NAMEL + CHAR(13) + CHAR(10) + TD2.ADDR,'') AS AGENTPOLTEXT1")
        sqlStat.AppendLine("      ,'' AS AGENTPOLTEXT2")
        sqlStat.AppendLine("      ,'' AS AGENTPOLTEXT3")
        sqlStat.AppendLine("      ,ISNULL(TD1.NAMEL + CHAR(13) + CHAR(10) + TD1.ADDR,'') AS AGENTPODTEXT1")
        sqlStat.AppendLine("      ,'' AS AGENTPODTEXT2")
        sqlStat.AppendLine("      ,'' AS AGENTPODTEXT3")
        sqlStat.AppendLine("      ,'' AS AGENTPODTEXT4")
        sqlStat.AppendLine("      ,'' AS AGENTPODTEXT5")
        sqlStat.AppendLine("      ,ISNULL(TD1.NAMEL + CHAR(13) + CHAR(10) + TD1.ADDR,'') AS AGENTNOTIFYTEXT1")
        sqlStat.AppendLine("      ,'' AS AGENTNOTIFYTEXT2")
        sqlStat.AppendLine("      ,'' AS AGENTNOTIFYTEXT3")
        sqlStat.AppendLine("      ,'' AS AGENTNOTIFYTEXT4")
        sqlStat.AppendLine("      ,'' AS AGENTNOTIFYTEXT5")

        sqlStat.AppendFormat("      ,CASE WHEN @TRANCLS = '1' THEN ISNULL(SP.{0},'') ELSE ISNULL(CN.{0},'') END AS SHIPPERNAME", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN ISNULL(SP.CONTACTPERSON,'') ELSE ISNULL(CN.CONTACTPERSON,'') END AS SHIPCONTACTPERSON")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN ISNULL(SP.TEL,'') ELSE ISNULL(CN.TEL,'') END AS SHIPTEL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN ISNULL(SP.FAX,'') ELSE ISNULL(CN.FAX,'') END AS SHIPFAX")
        sqlStat.AppendLine("      ,ISNULL(TD2.NAMEL,'') AS AGENTPOLNAMEL")
        sqlStat.AppendLine("      ,ISNULL(TD2.TEL,'') AS AGENTPOLTEL")
        sqlStat.AppendLine("      ,ISNULL(TD2.FAX,'') AS AGENTPOLFAX")
        sqlStat.AppendLine("      ,@USERNAME AS AGENTPOLCONTACTPERSON")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN ISNULL(OB.BOOKINGNO,'') ELSE ISNULL(OB.BOOKINGNO2,'') END AS BOOKINGNO")
        sqlStat.AppendLine("      ,'(IN TANK)' AS INTANK")
        sqlStat.AppendLine("      ,'' AS SHIPTAREWEIGHT")
        sqlStat.AppendLine("      ,'' AS TANKCAPACITY")
        sqlStat.AppendLine("      ,'ON BOARD' AS BLDATE")
        sqlStat.AppendLine("      ,ISNULL(SP.CITY,'') + ', ' + ISNULL(CT2.NAMES,'') AS ISSUEAT")
        sqlStat.AppendLine("      ,'' AS FREIGHT")
        sqlStat.AppendLine("      ,FORMAT(GETDATE (),'dd') + ' ' + FV4.VALUE1 + ', ' + FORMAT(GETDATE (),'yyyy') AS OUTPUTDATE ")
        sqlStat.AppendLine("      ,CASE WHEN ETA.ACTDATE = '1900/01/01' THEN '' ELSE '(' + FORMAT(ETA.ACTDATE,'dd') + ' ' + FV3.VALUE1 + ', ' + FORMAT(ETA.ACTDATE,'yyyy') + ')' END AS SIETAACTDATE ")
        sqlStat.AppendLine("      ,CASE WHEN ETD.ACTDATE = '1900/01/01' THEN '' ELSE '(' + FORMAT(DATEADD(DAY, -1, ETD.ACTDATE),'dd') + ' ' + FV5.VALUE1 + ', ' + FORMAT(DATEADD(DAY, -1, ETD.ACTDATE),'yyyy') + ')' END AS CYCUT ")
        sqlStat.AppendLine("      ,CASE WHEN ETD.ACTDATE = '1900/01/01' THEN '' ELSE '(' + FORMAT(DATEADD(DAY, -1, ETD.ACTDATE),'dd') + ' ' + FV5.VALUE1 + ', ' + FORMAT(DATEADD(DAY, -1, ETD.ACTDATE),'yyyy') + ')' END AS DOCCUT ")
        sqlStat.AppendLine("      ,CASE WHEN trim(PD.HAZARDCLASS) = '' THEN '(NON-HAZ)' ELSE '(' + trim(PD.HAZARDCLASS) + ')' END AS SIIMDGCODE")
        sqlStat.AppendLine("      ,OB.NOOFPACKAGE + CASE WHEN OB.NOOFPACKAGE = '' THEN '' ELSE ' SOC TANK CONTAINERS' END AS SINOOFPACKAGE")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN CASE WHEN OB.CARRIERBLTYPE = '' THEN '' ELSE '""' + OB.CARRIERBLTYPE + '""' END ELSE CASE WHEN OB.CARRIERBLTYPE2 = '' THEN '' ELSE '""' + OB.CARRIERBLTYPE2 + '""' END END AS SICARRIERBLTYPE")

        '上記はB/L用に編集されているため個別で取得
        sqlStat.AppendLine("      ,PT2.AREANAME AS POLAREA")
        sqlStat.AppendLine("      ,PT3.AREANAME AS PODAREA")
        sqlStat.AppendLine("      ,FV1.VALUE3 AS POLTYPE")
        sqlStat.AppendLine("      ,FV1.VALUE4 AS PODTYPE")

        sqlStat.AppendLine("      ,ISNULL(TD2.CONTACTMAIL,'') AS AGENTPOLMAIL")
        sqlStat.AppendLine("      ,CASE WHEN ETD.ACTDATE = '1900/01/01' THEN '' ELSE FORMAT(ETD.ACTDATE,'MM/dd') END AS BIETDACTDATE ")

        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.TRANSIT1VSL1 ELSE OB.TRANSIT1VSL2 END AS TRANSIT1VSL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.TRANSIT1VOY1 ELSE OB.TRANSIT1VOY2 END AS TRANSIT1VOY")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.TRANSIT2VSL1 ELSE OB.TRANSIT2VSL2 END AS TRANSIT2VSL")
        sqlStat.AppendLine("      ,CASE WHEN @TRANCLS = '1' THEN OB.TRANSIT2VOY1 ELSE OB.TRANSIT2VOY2 END AS TRANSIT2VOY")
        sqlStat.AppendLine("      ,OB.CARRIER1 AS CARRIER1")
        sqlStat.AppendLine("      ,US.STAFFNAMES AS STAFFNAMES")
        sqlStat.AppendLine("      ,FV1.VALUE4 AS GITERM")
        sqlStat.AppendLine("      ,CASE WHEN OB.CONTAINERPKGS = '' THEN '' ELSE OB.CONTAINERPKGS END AS DRQUANTITYPACKAGES")
        sqlStat.AppendLine("      ,CASE WHEN ETD.ACTDATE = '1900/01/01' THEN '' ELSE FORMAT(ETD.ACTDATE,'yyyy/MM/dd') END AS FNETDACTDATE ")
        sqlStat.AppendLine("      ,CASE WHEN ETA.ACTDATE = '1900/01/01' THEN '' ELSE FORMAT(ETA.ACTDATE,'yyyy/MM/dd') END AS FNETAACTDATE ")
        sqlStat.AppendLine("      ,FORMAT(GETDATE (),'yyyy/MM/dd') AS FNOUTPUTDATE ")

        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE OB ")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT1 ")
        sqlStat.AppendLine("    ON PT1.PORTCODE  = (CASE WHEN @TRANCLS = '1' THEN OB.RECIEPTPORT1 ELSE OB.RECIEPTPORT2 END)")
        sqlStat.AppendLine("   AND PT1.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT1.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND PT1.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1 ")
        sqlStat.AppendLine("    ON FV1.KEYCODE   = OB.TERMTYPE")
        sqlStat.AppendLine("   AND FV1.CLASS     = 'TERM'")
        sqlStat.AppendLine("   AND FV1.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND FV1.ENDYMD   >= @ENDYMD")
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

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD ")
        sqlStat.AppendLine("    On PD.PRODUCTCODE  = OB.PRODUCTCODE")
        sqlStat.AppendLine("   And PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And PD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And PD.DELFLG      <> @DELFLG")

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

        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY EX ")
        sqlStat.AppendLine("    ON EX.COUNTRYCODE  = (Case When @TRANCLS = '1' THEN OB.RECIEPTCOUNTRY1 ELSE OB.RECIEPTCOUNTRY2 END)")
        sqlStat.AppendLine("   AND EX.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND EX.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND EX.DELFLG   <> @DELFLG")

        'sqlStat.AppendLine("  LEFT JOIN (Select TOP 1 ORDERNO, (Case When ACTUALDATE = '1900/01/01' THEN SCHEDELDATE ELSE ACTUALDATE END ) AS ACTDATE FROM GBT0005_ODR_VALUE WHERE ORDERNO = @ORDERNO AND DTLPOLPOD = (CASE WHEN @TRANCLS = '1' THEN 'POL1' ELSE 'POL2' END) AND ACTIONID in ('SHIP','RPEC','RPED','RPHC','RPHD') AND DELFLG <> @DELFLG) AS ETD")
        sqlStat.AppendLine("  LEFT JOIN (Select TOP 1 ORDERNO, SCHEDELDATE AS ACTDATE FROM GBT0005_ODR_VALUE WHERE ORDERNO = @ORDERNO AND DTLPOLPOD = (CASE WHEN @TRANCLS = '1' THEN 'POL1' ELSE 'POL2' END) AND DATEFIELD like 'ETD%' AND DELFLG <> @DELFLG) AS ETD")
        sqlStat.AppendLine("    ON ETD.ORDERNO = OB.ORDERNO")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2 ")
        sqlStat.AppendLine("    ON FV2.KEYCODE   = FORMAT(ETD.ACTDATE,'MM') ")
        sqlStat.AppendLine("   AND FV2.CLASS     = 'MONTH'")
        sqlStat.AppendLine("   AND FV2.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND FV2.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND FV2.DELFLG   <> @DELFLG")

        'sqlStat.AppendLine("  LEFT JOIN (Select TOP 1 ORDERNO, (Case When ACTUALDATE = '1900/01/01' THEN SCHEDELDATE ELSE ACTUALDATE END ) AS ACTDATE FROM GBT0005_ODR_VALUE WHERE ORDERNO = @ORDERNO AND DTLPOLPOD = (CASE WHEN @TRANCLS = '1' THEN 'POD1' ELSE 'POD2' END) AND ACTIONID in ('ARVD','DCEC','DCED','ETYC') AND DELFLG <> @DELFLG) AS ETA")
        sqlStat.AppendLine("  LEFT JOIN (Select TOP 1 ORDERNO, SCHEDELDATE AS ACTDATE FROM GBT0005_ODR_VALUE WHERE ORDERNO = @ORDERNO AND DTLPOLPOD = (CASE WHEN @TRANCLS = '1' THEN 'POD1' ELSE 'POD2' END) AND DATEFIELD like 'ETA%' AND DELFLG <> @DELFLG) AS ETA")
        sqlStat.AppendLine("    ON ETA.ORDERNO = OB.ORDERNO")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV3 ")
        sqlStat.AppendLine("    ON FV3.KEYCODE   = FORMAT(ETA.ACTDATE,'MM') ")
        sqlStat.AppendLine("   AND FV3.CLASS     = 'MONTH'")
        sqlStat.AppendLine("   AND FV3.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND FV3.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND FV3.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV4 ")
        sqlStat.AppendLine("    ON FV4.KEYCODE   = FORMAT(GETDATE (),'MM') ")
        sqlStat.AppendLine("   AND FV4.CLASS     = 'MONTH'")
        sqlStat.AppendLine("   AND FV4.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND FV4.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND FV4.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV5 ")
        sqlStat.AppendLine("    ON FV5.KEYCODE   = FORMAT(DATEADD(DAY, -1, ETD.ACTDATE),'MM') ")
        sqlStat.AppendLine("   AND FV5.CLASS     = 'MONTH'")
        sqlStat.AppendLine("   AND FV5.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CN ")
        sqlStat.AppendLine("    ON CN.CUSTOMERCODE   = OB.CONSIGNEE")
        sqlStat.AppendLine("   AND CN.STYMD         <= @STYMD")
        sqlStat.AppendLine("   AND CN.ENDYMD        >= @ENDYMD")
        sqlStat.AppendLine("   AND CN.DELFLG        <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP ")
        sqlStat.AppendLine("    ON SP.CUSTOMERCODE   = OB.SHIPPER")
        sqlStat.AppendLine("   AND SP.STYMD         <= @STYMD")
        sqlStat.AppendLine("   AND SP.ENDYMD        >= @ENDYMD")
        sqlStat.AppendLine("   AND SP.DELFLG        <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN (Select TOP 1 ORDERNO, CONTRACTORBR AS PORTCODE FROM GBT0005_ODR_VALUE WHERE ORDERNO = @ORDERNO AND DTLPOLPOD = (CASE WHEN @TRANCLS = '1' THEN 'POL1' ELSE 'POL2' END) AND ACTIONID = 'TRAV' AND DELFLG <> @DELFLG) AS TS")
        sqlStat.AppendLine("    ON TS.ORDERNO = OB.ORDERNO")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTTS ")
        sqlStat.AppendLine("    ON PTTS.PORTCODE   = TS.PORTCODE")
        sqlStat.AppendLine("   AND PTTS.STYMD         <= @STYMD")
        sqlStat.AppendLine("   AND PTTS.ENDYMD        >= @ENDYMD")
        sqlStat.AppendLine("   AND PTTS.DELFLG        <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US ")
        sqlStat.AppendLine("    ON US.USERID       = OB.UPDUSER")
        sqlStat.AppendLine("   AND US.STYMD         <= @STYMD")
        sqlStat.AppendLine("   AND US.ENDYMD        >= @ENDYMD")
        sqlStat.AppendLine("   AND US.DELFLG        <> @DELFLG")

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
                Dim paramUserName As SqlParameter = sqlCmd.Parameters.Add("@USERNAME", SqlDbType.NVarChar)
                'SQLパラメータ値セット
                paramOrderNo.Value = orderNo
                paramDelFlg.Value = CONST_FLAG_YES
                paramStYmd.Value = Date.Now
                paramEndYmd.Value = Date.Now
                paramTranCls.Value = tranCls
                paramUserName.Value = COA0019Session.USERNAME
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Order base info Error")
                    End If
                    retDt = CreateOrderInfoTable()
                    For Each col As DataColumn In dt.Columns
                        If retDt.Columns.Contains(col.ColumnName) Then
                            retDt.Rows(0)(col.ColumnName) = Convert.ToString(dt.Rows(0)(col.ColumnName))
                        End If
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
    Private Function CreateOrderInfoTable() As DataTable
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
        retDt.Columns.Add("VSLVOY", GetType(String))
        retDt.Columns.Add("PORTOFLOADING", GetType(String))
        retDt.Columns.Add("PORTOFDISCHARGE", GetType(String))
        retDt.Columns.Add("PLACEOFDELIVERY", GetType(String))
        retDt.Columns.Add("MARKSANDNUMBERS", GetType(String))
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
        retDt.Columns.Add("BLEXCHANGERATE", GetType(String))

        retDt.Columns.Add("AGENTPOD", GetType(String))
        retDt.Columns.Add("AGENTPOL", GetType(String))
        retDt.Columns.Add("TANKNO", GetType(String))
        retDt.Columns.Add("PRODUCTNAME", GetType(String))
        retDt.Columns.Add("UNNO", GetType(String))
        retDt.Columns.Add("IMDGCODE", GetType(String))
        retDt.Columns.Add("USETYPE", GetType(String))
        retDt.Columns.Add("TERMTYPE", GetType(String))
        retDt.Columns.Add("VSLNAME", GetType(String))
        retDt.Columns.Add("SHIPPINGTERM", GetType(String))
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
        retDt.Columns.Add("NOTIFYTEXT4", GetType(String))
        retDt.Columns.Add("NOTIFYTEXT5", GetType(String))
        retDt.Columns.Add("GROSSWEIGHT", GetType(String))
        retDt.Columns.Add("NETWEIGHT", GetType(String))
        retDt.Columns.Add("TIP", GetType(String))
        retDt.Columns.Add("DEMURTO1", GetType(String))
        retDt.Columns.Add("DEMURUSRATE1", GetType(String))
        retDt.Columns.Add("DEMURTO2", GetType(String))
        retDt.Columns.Add("DEMURUSRATE2", GetType(String))
        retDt.Columns.Add("DEMUACCT", GetType(String))
        retDt.Columns.Add("CONSIGNEENAME", GetType(String))
        retDt.Columns.Add("ETDACTDATE", GetType(String))
        retDt.Columns.Add("ETAACTDATE", GetType(String))
        retDt.Columns.Add("PORTOFDESTINATION", GetType(String))
        retDt.Columns.Add("BLTYPE", GetType(String))

        retDt.Columns.Add("SALOCALCURRENCY", GetType(String))
        retDt.Columns.Add("SAPORTOFLOADING", GetType(String))
        retDt.Columns.Add("SAPORTOFDISCHARGE", GetType(String))
        retDt.Columns.Add("SIPLACEOFDELIVERY", GetType(String))
        retDt.Columns.Add("SIETAACTDATE", GetType(String))
        retDt.Columns.Add("SIIMDGCODE", GetType(String))
        retDt.Columns.Add("SINOOFPACKAGE", GetType(String))
        retDt.Columns.Add("SICARRIERBLTYPE", GetType(String))

        retDt.Columns.Add("AGENTPOLTEXT1", GetType(String))
        retDt.Columns.Add("AGENTPOLTEXT2", GetType(String))
        retDt.Columns.Add("AGENTPOLTEXT3", GetType(String))
        retDt.Columns.Add("AGENTPODTEXT1", GetType(String))
        retDt.Columns.Add("AGENTPODTEXT2", GetType(String))
        retDt.Columns.Add("AGENTPODTEXT3", GetType(String))
        retDt.Columns.Add("AGENTPODTEXT4", GetType(String))
        retDt.Columns.Add("AGENTPODTEXT5", GetType(String))
        retDt.Columns.Add("AGENTNOTIFYTEXT1", GetType(String))
        retDt.Columns.Add("AGENTNOTIFYTEXT2", GetType(String))
        retDt.Columns.Add("AGENTNOTIFYTEXT3", GetType(String))
        retDt.Columns.Add("AGENTNOTIFYTEXT4", GetType(String))
        retDt.Columns.Add("AGENTNOTIFYTEXT5", GetType(String))
        retDt.Columns.Add("SHIPPERNAME", GetType(String))
        retDt.Columns.Add("SHIPCONTACTPERSON", GetType(String))
        retDt.Columns.Add("SHIPTEL", GetType(String))
        retDt.Columns.Add("SHIPFAX", GetType(String))
        retDt.Columns.Add("OUTPUTDATE", GetType(String))
        retDt.Columns.Add("AGENTPOLNAMEL", GetType(String))
        retDt.Columns.Add("AGENTPOLTEL", GetType(String))
        retDt.Columns.Add("AGENTPOLFAX", GetType(String))
        retDt.Columns.Add("AGENTPOLCONTACTPERSON", GetType(String))
        retDt.Columns.Add("BOOKINGNO", GetType(String))
        retDt.Columns.Add("SHIPTAREWEIGHT", GetType(String))
        retDt.Columns.Add("TANKCAPACITY", GetType(String))
        retDt.Columns.Add("INTANK", GetType(String))
        retDt.Columns.Add("BLDATE", GetType(String))
        retDt.Columns.Add("ISSUEAT", GetType(String))
        retDt.Columns.Add("FREIGHT", GetType(String))
        retDt.Columns.Add("CYCUT", GetType(String))
        retDt.Columns.Add("DOCCUT", GetType(String))

        '
        retDt.Columns.Add("POLAREA", GetType(String))
        retDt.Columns.Add("PODAREA", GetType(String))
        retDt.Columns.Add("POLTYPE", GetType(String))
        retDt.Columns.Add("PODTYPE", GetType(String))


        retDt.Columns.Add("AGENTPOLMAIL", GetType(String))
        retDt.Columns.Add("BIHOUSEBLISSUE", GetType(String))
        retDt.Columns.Add("BIAMSSENDTYPE", GetType(String))
        retDt.Columns.Add("BIACISENDTYPE", GetType(String))
        retDt.Columns.Add("BIETDACTDATE", GetType(String))
        retDt.Columns.Add("BIPODTERMINAL", GetType(String))

        retDt.Columns.Add("TRANSITPORT", GetType(String))
        retDt.Columns.Add("TRANSIT1VSL", GetType(String))
        retDt.Columns.Add("TRANSIT1VOY", GetType(String))
        retDt.Columns.Add("TRANSIT2VSL", GetType(String))
        retDt.Columns.Add("TRANSIT2VOY", GetType(String))

        retDt.Columns.Add("STAFFNAMES", GetType(String))
        retDt.Columns.Add("CARRIER1", GetType(String))
        retDt.Columns.Add("GITERM", GetType(String))

        retDt.Columns.Add("DRQUANTITYPACKAGES", GetType(String))

        retDt.Columns.Add("FACYTRUCKER", GetType(String))
        retDt.Columns.Add("FACYTRUCKERTELFAX", GetType(String))
        retDt.Columns.Add("FNETDACTDATE", GetType(String))
        retDt.Columns.Add("FNETAACTDATE", GetType(String))
        retDt.Columns.Add("FNOUTPUTDATE", GetType(String))
        '検討中
        retDt.Columns.Add("DUMMY", GetType(String))
        retDt.Columns.Add("DUMMY2", GetType(String))

        Dim dr As DataRow = retDt.NewRow
        dr.Item("DUMMY") = ""
        retDt.Rows.Add(dr)
        Return retDt
    End Function

    ''' <summary>
    ''' 保存ボタン押下時イベント
    ''' </summary>
    Public Sub btnSave_Click()
        Dim ds As New DataSet
        '画面情報をデータテーブルに格納
        Dim baseDt As DataTable = CollectDisplayOrderBase()
        Dim valueDt As DataTable = CollectDisplayOrderValue()

        '各種データテーブルをデータセットに格納
        ds.Tables.AddRange({baseDt, valueDt})
        '入力チェック
        If CheckInput(ds, True, True) = False Then
            Return
        End If

        'DB登録処理実行
        Dim errFlg As Boolean = True
        EntryData(ds, errFlg)
        If Not errFlg Then
            Return
        End If

        Me.hdnMsgId.Value = C_MESSAGENO.NORMALDBENTRY

        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
        Server.Transfer(Request.Url.LocalPath)

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

                'Case Me.vLeftFrtAndCrg.ID
                '    'Freight and Charges選択時
                '    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                '    If targetObject IsNot Nothing Then
                '        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                '        If Me.lbFrtAndCrg.SelectedItem IsNot Nothing Then
                '            txtobj.Text = Me.lbFrtAndCrg.SelectedItem.Value
                '            Me.lblFreightChargesText.Text = Me.lbFrtAndCrg.SelectedItem.Text
                '            txtobj.Focus()
                '        Else
                '            txtobj.Text = ""
                '            Me.lblFreightChargesText.Text = ""
                '            txtobj.Focus()
                '        End If
                '    End If

                Case Me.vLeftCountry.ID
                    '国選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Select Case targetObject.ID
                            Case "txtPaymentPlace"

                                If Me.lbCountry.SelectedItem IsNot Nothing Then
                                    txtobj.Text = Me.lbCountry.SelectedItem.Value
                                    Dim parts As String()
                                    parts = Split(Me.lbCountry.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                    Me.lblPaymentPlaceText.Text = parts(1)
                                    txtobj.Focus()
                                Else
                                    txtobj.Text = ""
                                    Me.lblPaymentPlaceText.Text = ""
                                    txtobj.Focus()
                                End If

                            Case "txtBlIssuePlace"

                                If Me.lbCountry.SelectedItem IsNot Nothing Then
                                    txtobj.Text = Me.lbCountry.SelectedItem.Value
                                    Dim parts As String()
                                    parts = Split(Me.lbCountry.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                    Me.lblBlIssuePlaceText.Text = parts(1)
                                    txtobj.Focus()
                                Else
                                    txtobj.Text = ""
                                    Me.lblBlIssuePlaceText.Text = ""
                                    txtobj.Focus()
                                End If

                            Case "txtAnIssuePlace"

                                If Me.lbCountry.SelectedItem IsNot Nothing Then
                                    txtobj.Text = Me.lbCountry.SelectedItem.Value
                                    Dim parts As String()
                                    parts = Split(Me.lbCountry.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                    Me.lblAnIssuePlaceText.Text = parts(1)
                                    txtobj.Focus()
                                Else
                                    txtobj.Text = ""
                                    Me.lblAnIssuePlaceText.Text = ""
                                    txtobj.Focus()
                                End If

                        End Select

                    End If

                Case Me.vLeftBlType.ID
                    'BlType選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbBlType.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbBlType.SelectedItem.Value
                            Me.lblBlTypeText.Text = Me.lbBlType.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblBlTypeText.Text = ""
                            txtobj.Focus()
                        End If
                    End If

                Case Me.vLeftCarBlType.ID
                    'CarBlType選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCarBlType.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbCarBlType.SelectedItem.Value
                            Me.lblCarBlTypeText.Text = Me.lbCarBlType.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblCarBlTypeText.Text = ""
                            txtobj.Focus()
                        End If
                    End If

                Case Me.vLeftCarrier.ID
                    '船選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim carrierCode As String = ""
                        If Me.lbCarrier.SelectedItem IsNot Nothing Then
                            carrierCode = Me.lbCarrier.SelectedItem.Value
                        End If

                        SetDisplayCarrier(targetTextBox, carrierCode)
                    End If

                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If

                Case Me.vLeftDemAcct.ID
                    'Demu for the acct of選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbDemAcct.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbDemAcct.SelectedItem.Value
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                    End If

                Case vLeftEorF.ID
                    Dim EmptyOrFull As String = ""
                    If Me.lbEorF.SelectedItem IsNot Nothing Then
                        EmptyOrFull = Me.lbEorF.SelectedItem.Text

                        SetDisplayEmptyOrFull(EmptyOrFull)
                    End If

                Case Me.vLeftDelFlg.ID 'アクティブなビューが削除フラグ
                    '削除フラグ選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then

                    Else
                        'リピーター削除フラグ
                        Dim IntCnt As Integer = Nothing
                        If Me.lbDelFlg.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            If Me.hdnSelectedTabId.Value = Me.tabFileUp.ClientID Then
                                If Integer.TryParse(Me.hdnTextDbClickField.Value, IntCnt) Then
                                    DirectCast(dViewRep.Items(IntCnt).FindControl("txtRepDelFlg"),
                                          System.Web.UI.WebControls.TextBox).Text = Me.lbDelFlg.SelectedItem.Value
                                    dViewRep.Items(Integer.Parse(hdnTextDbClickField.Value)).FindControl("txtRepDelFlg").Focus()
                                End If
                            End If
                        End If

                    End If
                Case Else
                    '何もしない
            End Select
        End If
        '○ 画面左サイドボックス非表示は、画面JavaScriptで実行
        Me.hdnTextDbClickField.Value = ""
        Me.hdnIsLeftBoxOpen.Value = ""
        'ClearLeftListData()
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
        'ClearLeftListData()
    End Sub
    ''' <summary>
    ''' タブクリックイベント
    ''' </summary>
    ''' <param name="tabObjId">クリックしたタブオブジェクトのID</param>
    Protected Sub TabClick(tabObjId As String)

        Dim beforeTab As String = ""
        Dim intBeforeTab As Integer = Nothing
        Dim selectedTab As String = ""
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)
        tabObjects.Add(COSTITEM.CostItemGroup.BL, Me.tabBL)
        tabObjects.Add(COSTITEM.CostItemGroup.TANK, Me.tabTank)
        tabObjects.Add(COSTITEM.CostItemGroup.OTHER, Me.tabOther)
        tabObjects.Add(COSTITEM.CostItemGroup.FileUp, Me.tabFileUp)

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected") Then
                beforeTab = tabObject.Value.ClientID
                intBeforeTab = tabObject.Key
            End If
            tabObject.Value.Attributes.Remove("class")
            If tabObjId = tabObject.Value.ClientID Then
                tabObject.Value.Attributes.Add("class", "selected")
                selectedTab = tabObject.Value.ClientID
            End If

        Next

        'リペアフラグを保持
        visibleControl(selectedTab)

        If selectedTab = Me.tabTank.ID Then

            'SetCostGridItem(beforeTab, selectedTab)
            SetCostGridItem()

            CalcSummaryNetWeight()
            CalcSummaryGrossWeight()
            CalcSummaryNoOfPackage()

            enabledControls()

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
        '****************************************
        '右ボックス
        '****************************************
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Remark")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")
        AddLangSetting(dicDisplayText, Me.lblRightInfo1, "ダブルクリックを行い入力を確定してください。", "Double click To confirm input.")
        AddLangSetting(dicDisplayText, Me.lblRightInfo2, "ダブルクリックを行い入力を確定してください。", "Double click To confirm input.")
        '****************************************
        ' 画面
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblBlInfoHeader, "BL-Info", "BL-Info")
        AddLangSetting(dicDisplayText, Me.lblOrderNoTitle, "Order ID：", "Order ID：")
        AddLangSetting(dicDisplayText, Me.lblBlNoTitle, "B/L No：", "B/L No：")
        AddLangSetting(dicDisplayText, Me.lblShipper, "Shipper", "Shipper")
        AddLangSetting(dicDisplayText, Me.lblConsignee, "Consignee", "Consignee")
        AddLangSetting(dicDisplayText, Me.lblNotifyParty, "NotifyParty", "Notify Party")
        'AddLangSetting(dicDisplayText, Me.lblCountry, "Country", "Country")
        'AddLangSetting(dicDisplayText, Me.lblPort, "Port", "Port")
        'AddLangSetting(dicDisplayText, Me.lblCountry2, "Country", "Country")
        'AddLangSetting(dicDisplayText, Me.lblPort2, "Port", "Port")
        'AddLangSetting(dicDisplayText, Me.lblExportRow, "Export", "Export")
        'AddLangSetting(dicDisplayText, Me.lblImportRow, "Import", "Import")
        'AddLangSetting(dicDisplayText, Me.lblExport2Row, "Export2", "Export2")
        'AddLangSetting(dicDisplayText, Me.lblImport2Row, "Import2", "Import2")
        AddLangSetting(dicDisplayText, Me.lblPreCarriageBy, "Pre-carriage by", "Pre-carriage by")
        AddLangSetting(dicDisplayText, Me.lblVessel, "Vessel", "Vessel")
        'AddLangSetting(dicDisplayText, Me.lblVessel2, "Vessel2", "Vessel2")
        AddLangSetting(dicDisplayText, Me.lblVoyNo, "Voy No", "Voy No")
        'AddLangSetting(dicDisplayText, Me.lblVoyNo2, "Voy No2", "Voy No2")
        AddLangSetting(dicDisplayText, Me.lblCargoRelease, "Contact for cargo release", "Contact for cargo release")
        AddLangSetting(dicDisplayText, Me.lblFnlDest, "Final Destination", "Final Destination")
        'AddLangSetting(dicDisplayText, Me.lblNoContainerPkg, "No of Containers or Pkgs", "No of Containers or Pkgs")
        'AddLangSetting(dicDisplayText, Me.lblDesGoods, "Description of Goods", "Description of Goods")
        AddLangSetting(dicDisplayText, Me.lblFreightCharges, "Freight and Charges", "Freight and Charges")
        'AddLangSetting(dicDisplayText, Me.lblSay, "SAY", "SAY")
        'AddLangSetting(dicDisplayText, Me.lblTotalNumCont, "Total number of Containers", "Total number of Containers")
        AddLangSetting(dicDisplayText, Me.lblMarksNumbers, "Marks And Numbers", "Marks And Numbers")
        AddLangSetting(dicDisplayText, Me.lblMerDecValue, "Merchant's Declared", "Merchant's Declared")
        AddLangSetting(dicDisplayText, Me.lblRevenueTons, "Revenue Tons", "Revenue Tons")
        AddLangSetting(dicDisplayText, Me.lblRate, "Rate", "Rate")
        AddLangSetting(dicDisplayText, Me.lblPer, "Per", "Per")
        AddLangSetting(dicDisplayText, Me.lblPrepaid, "Prepaid", "Prepaid")
        AddLangSetting(dicDisplayText, Me.lblCollect, "Collect", "Collect")
        'AddLangSetting(dicDisplayText, Me.lblShipLine, "Ship Line", "Ship Line")
        AddLangSetting(dicDisplayText, Me.lblCarrierBlNo, "Carrier B/L No", "Carrier B/L No")
        AddLangSetting(dicDisplayText, Me.lblBookingNo, "Booking No", "Booking No")
        'AddLangSetting(dicDisplayText, Me.lblTermType, "Term Type", "Term Type")
        AddLangSetting(dicDisplayText, Me.lblNoOfPackage, "No of Package", "No of Package")
        AddLangSetting(dicDisplayText, Me.lblShipRateEx, "Ex Ship Rate", "Ex Ship Rate")
        AddLangSetting(dicDisplayText, Me.lblShipRateIn, "In Ship Rate", "In Ship Rate")
        AddLangSetting(dicDisplayText, Me.lblNoOfBl, "No of B/L", "No of B/L")
        'AddLangSetting(dicDisplayText, Me.lblBlNo, "B/L No", "B/L No")
        AddLangSetting(dicDisplayText, Me.lblBlType, "B/L Type", "B/L Type")
        AddLangSetting(dicDisplayText, Me.lblPaymentPlace, "Payment Place", "Payment Place")
        AddLangSetting(dicDisplayText, Me.lblBlIssuePlace, "B/L Issue Place", "B/L Issue Place")
        AddLangSetting(dicDisplayText, Me.lblAnIssuePlace, "A/N Issue Place", "A/N Issue Place")

        AddLangSetting(dicDisplayText, Me.lblDemAcct, "Demu For The Acct Of", "Demu For The Acct Of")
        AddLangSetting(dicDisplayText, Me.lblCarBlType, "Carrier B/L Type", "Carrier B/L Type")

        AddLangSetting(dicDisplayText, Me.lblDecOfGd, "Description Of Goods", "Description Of Goods")
        AddLangSetting(dicDisplayText, Me.lblCarrier, "Carrier", "Carrier")

        AddLangSetting(dicDisplayText, Me.lblVsl2nd, "2nd Vessel", "2nd Vessel")
        AddLangSetting(dicDisplayText, Me.lblVoy2nd, "2nd Voyage", "2nd Voyage")
        AddLangSetting(dicDisplayText, Me.lblVsl3rd, "3rd Vessel", "3rd Vessel")
        AddLangSetting(dicDisplayText, Me.lblVoy3rd, "3rd Voyage", "3rd Voyage")

        'AddLangSetting(dicDisplayText, Me.lblExchangeRate, "Exchange Rate", "Exchange Rate")
        'AddLangSetting(dicDisplayText, Me.lblPrepaidAt, "Prepaid at", "Prepaid at")
        'AddLangSetting(dicDisplayText, Me.lblPayableAt, "Payable at", "Payable at")
        'AddLangSetting(dicDisplayText, Me.lblLocalCurrency, "Local Currency", "Local Currency")
        'AddLangSetting(dicDisplayText, Me.lblDateOfIssue, "Date Of Issue", "Date Of Issue")
        AddLangSetting(dicDisplayText, Me.lblGrossSummary, "総合計", "Gross Total")
        AddLangSetting(dicDisplayText, Me.lblNetSummary, "正味合計", "Net Total")
        AddLangSetting(dicDisplayText, Me.lblMeasurement, "Measurement", "Measurement")

        AddLangSetting(dicDisplayText, Me.lblLdnVessel, "Laden Vessel", "Laden Vessel")
        AddLangSetting(dicDisplayText, Me.lblLdnPol, "Laden Pol", "Laden Pol")
        AddLangSetting(dicDisplayText, Me.lblLdnDate, "Laden Date", "Laden Date")
        AddLangSetting(dicDisplayText, Me.lblLdnBy, "Laden By", "Laden By")

        '****************************************
        ' 各種ボタン
        '****************************************
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnOutputExcel, "ﾃﾞｰﾀﾀﾞｳﾝﾛｰﾄﾞ", "Data Download")
        AddLangSetting(dicDisplayText, Me.btnOutputFile, "ﾌｧｲﾙﾀﾞｳﾝﾛｰﾄﾞ", "File Download")
        AddLangSetting(dicDisplayText, Me.btnPrint, "帳票出力", "Excel Print")
        AddLangSetting(dicDisplayText, Me.btnPDFPrint, "PDF帳票出力", "PDF Print")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        '****************************************
        '左ボックス
        '****************************************
        AddLangSetting(dicDisplayText, Me.lblRightListDiscription, "Export/Import Settings", "Export/Import Settings")
        AddLangSetting(dicDisplayText, Me.lblRightListPrintDiscription, "Print Settings", "Print Settings")

        '****************************************
        ' 隠しフィールド
        '****************************************
        AddLangSetting(dicDisplayText, Me.hdnDispDeleteBtnText, "削除", "Delete")
        AddLangSetting(dicDisplayText, Me.hdnRemarkEmptyMessage, "DoubleClick to input", "DoubleClick to input")
        'ファイルアップロードメッセージ
        AddLangSetting(dicDisplayText, Me.hdnUploadMessage01, "ファイルアップロード開始", "Start uploading files")
        AddLangSetting(dicDisplayText, Me.hdnUploadError01, "ファイルアップロードが失敗しました。", "File upload failed.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError02, "通信を中止しました。", "Communication was canceled.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError03, "タイムアウトエラーが発生しました。", "A timeout error occurred.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError04, "更新権限がありません。", "do not have update permission.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError05, "対応外のファイル形式です。", "It is an incompatible file format.")

        SetDisplayLangObjects(dicDisplayText, lang)

        '****************************************
        ' グリッドヘッダーの表示文言設定(GrivViewだけは個別制御が必要)
        '****************************************
        Dim dicGridDisplayText As New Dictionary(Of Integer, Dictionary(Of String, String))
        dicGridDisplayText.Add(0,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "タンク番号"}, {C_LANG.EN, "TankNo"}})
        dicGridDisplayText.Add(1,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "表示順"}, {C_LANG.EN, "Seq"}})
        dicGridDisplayText.Add(2,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "タイプ"}, {C_LANG.EN, "TankType"}})
        dicGridDisplayText.Add(3,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "SealNo1"}, {C_LANG.EN, "SealNo1"}})
        dicGridDisplayText.Add(4,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "SealNo2"}, {C_LANG.EN, "SealNo2"}})
        dicGridDisplayText.Add(5,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "SealNo3"}, {C_LANG.EN, "SealNo3"}})
        dicGridDisplayText.Add(6,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "SealNo4"}, {C_LANG.EN, "SealNo4"}})
        dicGridDisplayText.Add(7,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "Gross Weight"}, {C_LANG.EN, "Gross Weight"}})
        dicGridDisplayText.Add(8,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "Net Weight"}, {C_LANG.EN, "Net Weight"}})
        dicGridDisplayText.Add(9,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "Empty Or Full"}, {C_LANG.EN, "Empty Or Full"}})
        dicGridDisplayText.Add(10,
                           New Dictionary(Of String, String) From {{C_LANG.JA, "No of Package"}, {C_LANG.EN, "No of Package"}})

        If gvDetailInfo.Columns.Count > 0 Then
            '最大列数取得
            Dim colMaxIndex As Integer = gvDetailInfo.Columns.Count - 1
            '列のループ
            For i = 0 To colMaxIndex
                Dim fldObj As DataControlField = gvDetailInfo.Columns(i)
                '変換ディクショナリに対象カラム名を置換が設定されている場合文言変更
                If dicGridDisplayText.ContainsKey(i) = True Then
                    fldObj.HeaderText = dicGridDisplayText(i)(lang)
                End If
            Next
        End If
    End Sub
    ''' <summary>
    ''' 遷移元（前画面）の情報を取得
    ''' </summary>
    Private Function GetPrevDisplayInfo(ByRef retDataSet As DataSet) As String

        Dim retVal As String = C_MESSAGENO.NORMAL

        '右ボックス帳票タブ
        Dim errMsg As String = ""
        errMsg = Me.RightboxInit(True)
        If errMsg <> "" Then
            retVal = errMsg
        End If

        If TypeOf Page.PreviousPage Is GBT00014BL Then
            Me.PreProcType = "自画面遷移"
            '自身からの遷移(Save時に反応)
            Dim prevPage As GBT00014BL = DirectCast(Page.PreviousPage, GBT00014BL)
            Me.hdnOrderNo.Value = prevPage.lblOrderNo.Text

            Dim hdnWhiTra As HiddenField = Nothing
            hdnWhiTra = DirectCast(prevPage.FindControl("hdnWhichTrans"), HiddenField)
            Me.hdnWhichTrans.Value = hdnWhiTra.Value

            'メイン情報取得
            Dim dt As DataTable = GetOrderBase(Me.hdnOrderNo.Value)
            'Me.hdnTmstmp.Value = Convert.ToString(dt.Rows(0).Item("TIMSTP"))
            Me.hdnUpdYmd.Value = Convert.ToString(dt.Rows(0).Item("UPDYMD"))
            Me.hdnUpdUser.Value = Convert.ToString(dt.Rows(0).Item("UPDUSER"))
            Me.hdnUpdTermId.Value = Convert.ToString(dt.Rows(0).Item("UPDTERMID"))
            'メイン情報格納
            retDataSet.Tables.Add(dt)
            '費用情報取得
            Dim costDt As DataTable = GetOrderValue(Me.hdnOrderNo.Value)
            retDataSet.Tables.Add(costDt)

            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSearchType", Me.hdnSearchType},
                                                                        {"hdnBlIssued", Me.hdnBlIssued},
                                                                        {"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPort", Me.hdnPort},
                                                                        {"hdnProduct", Me.hdnProduct},
                                                                        {"hdnCarrier", Me.hdnCarrier},
                                                                        {"hdnVsl", Me.hdnVsl},
                                                                        {"hdnCountry", Me.hdnCountry},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnMsgId", Me.hdnMsgId},
                                                                        {"hdnDepartureArrival", Me.hdnDepartureArrival},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"hdnReportVariant", Me.hdnReportVariant}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            Dim prevLbRightPrintObj As ListBox = DirectCast(prevPage.FindControl(Me.lbRightListPrint.ID), ListBox)
            If prevLbRightPrintObj IsNot Nothing Then
                Me.lbRightListPrint.SelectedValue = prevLbRightPrintObj.SelectedValue
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00013RESULT Then

            '一覧からの遷移
            Dim prevPage As GBT00013RESULT = DirectCast(Page.PreviousPage, GBT00013RESULT)
            Dim hdnOrderNoObj As HiddenField = Nothing
            hdnOrderNoObj = DirectCast(prevPage.FindControl("hdnSelectedOrderNo"), HiddenField)
            Me.hdnOrderNo.Value = hdnOrderNoObj.Value

            Dim hdnWhiTra As HiddenField = Nothing
            hdnWhiTra = DirectCast(prevPage.FindControl("hdnSelectedWhichTrans"), HiddenField)
            Me.hdnWhichTrans.Value = hdnWhiTra.Value

            'メイン情報取得
            Dim dt As DataTable = GetOrderBase(Me.hdnOrderNo.Value)
            'Me.hdnTmstmp.Value = Convert.ToString(dt.Rows(0).Item("TIMSTP"))
            Me.hdnUpdYmd.Value = Convert.ToString(dt.Rows(0).Item("UPDYMD"))
            Me.hdnUpdUser.Value = Convert.ToString(dt.Rows(0).Item("UPDUSER"))
            Me.hdnUpdTermId.Value = Convert.ToString(dt.Rows(0).Item("UPDTERMID"))

            'メイン情報格納
            retDataSet.Tables.Add(dt)
            '費用情報取得
            Dim costDt As DataTable = GetOrderValue(Me.hdnOrderNo.Value)
            retDataSet.Tables.Add(costDt)

            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSearchType", Me.hdnSearchType},
                                                                        {"hdnBlIssued", Me.hdnBlIssued},
                                                                        {"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPort", Me.hdnPort},
                                                                        {"hdnProduct", Me.hdnProduct},
                                                                        {"hdnCarrier", Me.hdnCarrier},
                                                                        {"hdnVsl", Me.hdnVsl},
                                                                        {"hdnCountry", Me.hdnCountry},
                                                                        {"hdnOffice", Me.hdnOffice}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

        ElseIf TypeOf Page.PreviousPage Is GBT00017RESULT Then

            '一覧からの遷移
            Dim prevPage As GBT00017RESULT = DirectCast(Page.PreviousPage, GBT00017RESULT)
            Dim hdnOrderNoObj As HiddenField = Nothing
            hdnOrderNoObj = DirectCast(prevPage.FindControl("hdnSelectedOdId"), HiddenField)
            Me.hdnOrderNo.Value = hdnOrderNoObj.Value

            Dim hdnWhiTra As HiddenField = Nothing
            hdnWhiTra = DirectCast(prevPage.FindControl("hdnSelectedTrans"), HiddenField)
            Me.hdnWhichTrans.Value = hdnWhiTra.Value

            'メイン情報取得
            Dim dt As DataTable = GetOrderBase(Me.hdnOrderNo.Value)
            'Me.hdnTmstmp.Value = Convert.ToString(dt.Rows(0).Item("TIMSTP"))
            Me.hdnUpdYmd.Value = Convert.ToString(dt.Rows(0).Item("UPDYMD"))
            Me.hdnUpdUser.Value = Convert.ToString(dt.Rows(0).Item("UPDUSER"))
            Me.hdnUpdTermId.Value = Convert.ToString(dt.Rows(0).Item("UPDTERMID"))

            'メイン情報格納
            retDataSet.Tables.Add(dt)
            '費用情報取得
            Dim costDt As DataTable = GetOrderValue(Me.hdnOrderNo.Value)
            retDataSet.Tables.Add(costDt)

            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSearchType", Me.hdnSearchType},
                                                                         {"hdnBlIssued", Me.hdnBlIssued},
                                                                         {"hdnETDStYMD", Me.hdnETDStYMD},
                                                                         {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                         {"hdnShipper", Me.hdnShipper},
                                                                         {"hdnConsignee", Me.hdnConsignee},
                                                                         {"hdnPort", Me.hdnPort},
                                                                         {"hdnProduct", Me.hdnProduct},
                                                                         {"hdnCarrier", Me.hdnCarrier},
                                                                         {"hdnVsl", Me.hdnVsl},
                                                                         {"hdnCountry", Me.hdnCountry},
                                                                         {"hdnOffice", Me.hdnOffice},
                                                                         {"hdnDepartureArrival", Me.hdnDepartureArrival},
                                                                         {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                         {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                         {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                         {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                         {"hdnReportVariant", Me.hdnReportVariant}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevPage.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next
        Else
            PreProcType = "遷移元なし"
        End If

        Return retVal
    End Function
    ''' <summary>
    ''' オーナー情報を格納する空のデータテーブルを作成する
    ''' </summary>
    ''' <returns>Organizer情報のデータテーブルを作成</returns>
    ''' <remarks>複数レコードはありえないので１レコード作り返却</remarks>
    Private Function CreateOrderBaseTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = "ORDER_BASE"
        retDt.Columns.Add("ORDERNO", GetType(String))
        retDt.Columns.Add("STYMD", GetType(String))
        retDt.Columns.Add("ENDYMD", GetType(String))
        retDt.Columns.Add("BRID", GetType(String))
        retDt.Columns.Add("BRTYPE", GetType(String))
        retDt.Columns.Add("VALIDITYFROM", GetType(String))
        retDt.Columns.Add("VALIDITYTO", GetType(String))
        retDt.Columns.Add("TERMTYPE", GetType(String))
        retDt.Columns.Add("NOOFTANKS", GetType(String))
        retDt.Columns.Add("SHIPPER", GetType(String))
        retDt.Columns.Add("CONSIGNEE", GetType(String))
        retDt.Columns.Add("CARRIER1", GetType(String))
        retDt.Columns.Add("CARRIER2", GetType(String))
        retDt.Columns.Add("PRODUCTCODE", GetType(String))
        retDt.Columns.Add("PRODUCTWEIGHT", GetType(String))
        retDt.Columns.Add("RECIEPTCOUNTRY1", GetType(String))
        retDt.Columns.Add("RECIEPTPORT1", GetType(String))
        retDt.Columns.Add("RECIEPTCOUNTRY2", GetType(String))
        retDt.Columns.Add("RECIEPTPORT2", GetType(String))
        retDt.Columns.Add("LOADCOUNTRY1", GetType(String))
        retDt.Columns.Add("LOADPORT1", GetType(String))
        retDt.Columns.Add("LOADCOUNTRY2", GetType(String))
        retDt.Columns.Add("LOADPORT2", GetType(String))
        retDt.Columns.Add("DISCHARGECOUNTRY1", GetType(String))
        retDt.Columns.Add("DISCHARGEPORT1", GetType(String))
        retDt.Columns.Add("DISCHARGECOUNTRY2", GetType(String))
        retDt.Columns.Add("DISCHARGEPORT2", GetType(String))
        retDt.Columns.Add("DELIVERYCOUNTRY1", GetType(String))
        retDt.Columns.Add("DELIVERYPORT1", GetType(String))
        retDt.Columns.Add("DELIVERYCOUNTRY2", GetType(String))
        retDt.Columns.Add("DELIVERYPORT2", GetType(String))
        retDt.Columns.Add("VSL1", GetType(String))
        retDt.Columns.Add("VOY1", GetType(String))
        retDt.Columns.Add("ETD1", GetType(String))
        retDt.Columns.Add("ETA1", GetType(String))
        retDt.Columns.Add("VSL2", GetType(String))
        retDt.Columns.Add("VOY2", GetType(String))
        retDt.Columns.Add("ETD2", GetType(String))
        retDt.Columns.Add("ETA2", GetType(String))
        retDt.Columns.Add("INVOICEDBY", GetType(String))
        retDt.Columns.Add("LOADING", GetType(String))
        retDt.Columns.Add("STEAMING", GetType(String))
        retDt.Columns.Add("TIP", GetType(String))
        retDt.Columns.Add("EXTRA", GetType(String))
        retDt.Columns.Add("DEMURTO", GetType(String))
        retDt.Columns.Add("DEMURUSRATE1", GetType(String))
        retDt.Columns.Add("DEMURUSRATE2", GetType(String))
        retDt.Columns.Add("SALESPIC", GetType(String))
        retDt.Columns.Add("AGENTORGANIZER", GetType(String))
        retDt.Columns.Add("AGENTPOL1", GetType(String))
        retDt.Columns.Add("AGENTPOL2", GetType(String))
        retDt.Columns.Add("AGENTPOD1", GetType(String))
        retDt.Columns.Add("AGENTPOD2", GetType(String))
        retDt.Columns.Add("BLID1", GetType(String))
        retDt.Columns.Add("BLAPPDATE1", GetType(String))
        retDt.Columns.Add("BLID2", GetType(String))
        retDt.Columns.Add("BLAPPDATE2", GetType(String))
        retDt.Columns.Add("SHIPPERNAME", GetType(String))
        retDt.Columns.Add("SHIPPERTEXT", GetType(String))
        retDt.Columns.Add("CONSIGNEENAME", GetType(String))
        retDt.Columns.Add("CONSIGNEETEXT", GetType(String))
        retDt.Columns.Add("IECCODE", GetType(String))
        retDt.Columns.Add("NOTIFYNAME", GetType(String))
        retDt.Columns.Add("NOTIFYTEXT", GetType(String))
        retDt.Columns.Add("NOTIFYCONT", GetType(String))
        retDt.Columns.Add("NOTIFYCONTNAME", GetType(String))
        retDt.Columns.Add("NOTIFYCONTTEXT1", GetType(String))
        retDt.Columns.Add("NOTIFYCONTTEXT2", GetType(String))
        retDt.Columns.Add("PRECARRIAGETEXT", GetType(String))
        retDt.Columns.Add("VSL", GetType(String))
        retDt.Columns.Add("VOY", GetType(String))
        retDt.Columns.Add("FINDESTINATIONNAME", GetType(String))
        retDt.Columns.Add("FINDESTINATIONTEXT", GetType(String))
        retDt.Columns.Add("PRODUCT", GetType(String))
        retDt.Columns.Add("PRODUCTPORDER", GetType(String))
        retDt.Columns.Add("PRODUCTTIP", GetType(String))
        retDt.Columns.Add("PRODUCTFREIGHT", GetType(String))
        retDt.Columns.Add("FREIGHTANDCHARGES", GetType(String))
        retDt.Columns.Add("PREPAIDAT", GetType(String))
        retDt.Columns.Add("GOODSPKGS", GetType(String))
        retDt.Columns.Add("CONTAINERPKGS", GetType(String))
        retDt.Columns.Add("BLNUM", GetType(String))
        retDt.Columns.Add("CONTAINERNO", GetType(String))
        retDt.Columns.Add("SEALNO", GetType(String))
        retDt.Columns.Add("NOOFCONTAINER", GetType(String))
        retDt.Columns.Add("DECLAREDVALUE", GetType(String))
        retDt.Columns.Add("REVENUETONS", GetType(String))
        retDt.Columns.Add("RATE", GetType(String))
        retDt.Columns.Add("PER", GetType(String))
        retDt.Columns.Add("PREPAID", GetType(String))
        retDt.Columns.Add("COLLECT", GetType(String))
        retDt.Columns.Add("EXCHANGERATE", GetType(String))
        retDt.Columns.Add("PAYABLEAT", GetType(String))
        retDt.Columns.Add("LOCALCURRENCY", GetType(String))
        retDt.Columns.Add("CARRIERBLNO", GetType(String))
        retDt.Columns.Add("BOOKINGNO", GetType(String))
        retDt.Columns.Add("BOOKINGNO2", GetType(String))
        retDt.Columns.Add("NOOFPACKAGE", GetType(String))
        retDt.Columns.Add("BLTYPE", GetType(String))
        retDt.Columns.Add("NOOFBL", GetType(String))
        retDt.Columns.Add("PAYMENTPLACE", GetType(String))
        retDt.Columns.Add("BLISSUEPLACE", GetType(String))
        retDt.Columns.Add("ANISSUEPLACE", GetType(String))
        retDt.Columns.Add("MEASUREMENT", GetType(String))
        retDt.Columns.Add("MARKSANDNUMBERS", GetType(String))

        retDt.Columns.Add("LDNVSL1", GetType(String))
        retDt.Columns.Add("LDNPOL1", GetType(String))
        retDt.Columns.Add("LDNDATE1", GetType(String))
        retDt.Columns.Add("LDNBY1", GetType(String))

        retDt.Columns.Add("LDNVSL2", GetType(String))
        retDt.Columns.Add("LDNPOL2", GetType(String))
        retDt.Columns.Add("LDNDATE2", GetType(String))
        retDt.Columns.Add("LDNBY2", GetType(String))

        retDt.Columns.Add("REMARK", GetType(String))
        retDt.Columns.Add("DELFLG", GetType(String))
        retDt.Columns.Add("TIMSTP", GetType(String))
        retDt.Columns.Add("PRODUCTNAME", GetType(String))

        retDt.Columns.Add("CARRIERBLTYPE", GetType(String))
        retDt.Columns.Add("DEMUFORACCT", GetType(String))

        retDt.Columns.Add("SHIPPERTEXT2", GetType(String))
        retDt.Columns.Add("CONSIGNEETEXT2", GetType(String))
        retDt.Columns.Add("NOTIFYTEXT2", GetType(String))
        retDt.Columns.Add("PRECARRIAGETEXT2", GetType(String))
        retDt.Columns.Add("FINDESTINATIONTEXT2", GetType(String))
        retDt.Columns.Add("NOOFBL2", GetType(String))
        retDt.Columns.Add("CARRIERBLTYPE2", GetType(String))
        retDt.Columns.Add("CARRIERBLNO2", GetType(String))
        retDt.Columns.Add("BLTYPE2", GetType(String))
        retDt.Columns.Add("DEMUFORACCT2", GetType(String))
        retDt.Columns.Add("MEASUREMENT2", GetType(String))
        retDt.Columns.Add("REVENUETONS2", GetType(String))
        retDt.Columns.Add("RATE2", GetType(String))
        retDt.Columns.Add("PER2", GetType(String))
        retDt.Columns.Add("PREPAID2", GetType(String))
        retDt.Columns.Add("COLLECT2", GetType(String))
        retDt.Columns.Add("DECLAREDVALUE2", GetType(String))
        retDt.Columns.Add("PAYMENTPLACE2", GetType(String))
        retDt.Columns.Add("BLISSUEPLACE2", GetType(String))
        retDt.Columns.Add("ANISSUEPLACE2", GetType(String))

        retDt.Columns.Add("BLRECEIPT1", GetType(String))
        retDt.Columns.Add("BLRECEIPT2", GetType(String))
        retDt.Columns.Add("BLLOADING1", GetType(String))
        retDt.Columns.Add("BLLOADING2", GetType(String))
        retDt.Columns.Add("BLDISCHARGE1", GetType(String))
        retDt.Columns.Add("BLDISCHARGE2", GetType(String))
        retDt.Columns.Add("BLDELIVERY1", GetType(String))
        retDt.Columns.Add("BLDELIVERY2", GetType(String))
        retDt.Columns.Add("BLPLACEDATEISSUE1", GetType(String))
        retDt.Columns.Add("BLPLACEDATEISSUE2", GetType(String))

        retDt.Columns.Add("UPDYMD", GetType(String))
        retDt.Columns.Add("UPDUSER", GetType(String))
        retDt.Columns.Add("UPDTERMID", GetType(String))

        retDt.Columns.Add("TRANSIT1VSL1", GetType(String))
        retDt.Columns.Add("TRANSIT1VOY1", GetType(String))
        retDt.Columns.Add("TRANSIT2VSL1", GetType(String))
        retDt.Columns.Add("TRANSIT2VOY1", GetType(String))
        retDt.Columns.Add("TRANSIT1VSL2", GetType(String))
        retDt.Columns.Add("TRANSIT1VOY2", GetType(String))
        retDt.Columns.Add("TRANSIT2VSL2", GetType(String))
        retDt.Columns.Add("TRANSIT2VOY2", GetType(String))

        '初期値設定
        retDt.Columns("MEASUREMENT").DefaultValue = "0"

        Dim dr As DataRow = retDt.NewRow
        retDt.Rows.Add(dr)
        Return retDt
    End Function
    ''' <summary>
    ''' 明細用のデータテーブル作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateOrderValueTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = "ORDER_VALUE"
        retDt.Columns.Add("ORDERNO", GetType(String))
        retDt.Columns.Add("TANKNO", GetType(String))
        retDt.Columns.Add("TANKSEQ", GetType(String))
        retDt.Columns.Add("TANKTYPE", GetType(String))
        retDt.Columns.Add("GROSSWEIGHT", GetType(String))
        retDt.Columns.Add("NETWEIGHT", GetType(String))
        retDt.Columns.Add("SEALNO1", GetType(String))
        retDt.Columns.Add("SEALNO2", GetType(String))
        retDt.Columns.Add("SEALNO3", GetType(String))
        retDt.Columns.Add("SEALNO4", GetType(String))
        retDt.Columns.Add("EMPTYORFULL", GetType(String))
        retDt.Columns.Add("NOOFPACKAGE", GetType(String))
        retDt.Columns.Add("EXSHIPRATE", GetType(String))
        retDt.Columns.Add("INSHIPRATE", GetType(String))
        retDt.Columns.Add("TAREWEIGHT", GetType(String))

        retDt.Columns.Add("DISPSEQ", GetType(String))
        retDt.Columns.Add("WORKC1", GetType(String))
        retDt.Columns.Add("WORKC2", GetType(String))
        retDt.Columns.Add("WORKC3", GetType(String))
        retDt.Columns.Add("WORKC4", GetType(String))
        retDt.Columns.Add("WORKC5", GetType(String))
        retDt.Columns.Add("WORKF1", GetType(String))
        retDt.Columns.Add("WORKF2", GetType(String))
        retDt.Columns.Add("WORKF3", GetType(String))
        retDt.Columns.Add("WORKF4", GetType(String))
        retDt.Columns.Add("WORKF5", GetType(String))

        Return retDt

    End Function
    ''' <summary>
    ''' オーダー情報をデータテーブルより画面に貼り付け
    ''' </summary>
    ''' <param name="ds"></param>
    Private Sub SetDisplayOrderBase(ds As DataSet, Optional isExcelInport As Boolean = False)
        Dim dt As DataTable = ds.Tables("ORDER_BASE")
        Dim dr As DataRow = dt.Rows(0)
        Me.lblOrderNo.Text = Convert.ToString(dr.Item("ORDERNO"))
        If hdnWhichTrans.Value = "1" Then
            'Me.hdnBLNo.Value = Convert.ToString(dr.Item("BLID1"))
            Me.lblBlNo.Text = Convert.ToString(dr.Item("BLID1"))
            Me.txtCarrier.Text = Convert.ToString(dr.Item("CARRIER1"))
            'Me.txtDateOfIssue.Text = Convert.ToString(dr.Item("BLAPPDATE1"))
            'Me.txtRecieptCountry.Text = Convert.ToString(dr.Item("RECIEPTCOUNTRY1"))
            'Me.txtRecieptPort.Text = Convert.ToString(dr.Item("RECIEPTPORT1"))
            'Me.lblRecieptPortText.Text = ""
            'If Me.txtRecieptCountry.Text <> "" AndAlso Me.txtRecieptPort.Text <> "" Then
            '    Dim portDt As DataTable = Me.GetPort(Me.txtRecieptCountry.Text, Me.txtRecieptPort.Text)
            '    If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
            '        Me.lblRecieptPortText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            '    End If
            'End If
            Me.hdnLoadCountry.Value = Convert.ToString(dr.Item("LOADCOUNTRY1"))
            'Me.txtLoadPort.Text = Convert.ToString(dr.Item("LOADPORT1"))
            'If Me.txtLoadCountry.Text <> "" AndAlso Me.txtLoadPort.Text <> "" Then
            '    Dim portDt As DataTable = Me.GetPort(Me.txtLoadCountry.Text, Me.txtLoadPort.Text)
            '    If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
            '        Me.lblLoadPortText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            '    End If
            'End If
            'Me.txtDischargeCountry.Text = Convert.ToString(dr.Item("DISCHARGECOUNTRY1"))
            'Me.txtDischargePort.Text = Convert.ToString(dr.Item("DISCHARGEPORT1"))
            'If Me.txtDischargeCountry.Text <> "" AndAlso Me.txtDischargePort.Text <> "" Then
            '    Dim portDt As DataTable = Me.GetPort(Me.txtDischargeCountry.Text, Me.txtDischargePort.Text)
            '    If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
            '        Me.lblDischargePortText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            '    End If
            'End If
            'Me.txtDeliveryCountry.Text = Convert.ToString(dr.Item("DELIVERYCOUNTRY1"))
            'Me.txtDeliveryPort.Text = Convert.ToString(dr.Item("DELIVERYPORT1"))
            'If Me.txtDeliveryCountry.Text <> "" AndAlso Me.txtDeliveryPort.Text <> "" Then
            '    Dim portDt As DataTable = Me.GetPort(Me.txtDeliveryCountry.Text, Me.txtDeliveryPort.Text)
            '    If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
            '        Me.lblDeliveryPortText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            '    End If
            'End If
            Me.txtVessel.Text = Convert.ToString(dr.Item("VSL1"))
            Me.txtVoyNo.Text = Convert.ToString(dr.Item("VOY1"))
            Me.txtCargoReleaseText.Text = Convert.ToString(dr.Item("NOTIFYCONTTEXT1"))

            Me.txtLdnVessel.Text = Convert.ToString(dr.Item("LDNVSL1"))
            Me.txtLdnPol.Text = Convert.ToString(dr.Item("LDNPOL1"))

            Dim ldnDate As Date = Nothing
            If Date.TryParse(Convert.ToString(dr.Item("LDNDATE1")), ldnDate) Then
                Me.txtLdnDate.Text = ldnDate.ToString(GBA00003UserSetting.DATEFORMAT)
            Else
                Me.txtLdnDate.Text = Convert.ToString(dr.Item("LDNDATE1"))
            End If

            Me.txtLdnBy.Text = Convert.ToString(dr.Item("LDNBY1"))
            Me.txtBookingNo.Text = Convert.ToString(dr.Item("BOOKINGNO"))

            Me.txtShipperText.Text = Convert.ToString(dr.Item("SHIPPERTEXT"))
            Me.txtConsigneeText.Text = Convert.ToString(dr.Item("CONSIGNEETEXT"))
            Me.txtNotifyPartyText.Text = Convert.ToString(dr.Item("NOTIFYTEXT"))
            Me.txtPreCarriageBy.Text = Convert.ToString(dr.Item("PRECARRIAGETEXT"))
            Me.txtFnlDest.Text = Convert.ToString(dr.Item("FINDESTINATIONTEXT"))
            Me.txtNoOfBl.Text = Convert.ToString(dr.Item("NOOFBL"))
            Me.txtBlType.Text = Convert.ToString(dr.Item("BLTYPE"))
            Me.txtCarrierBlNo.Text = Convert.ToString(dr.Item("CARRIERBLNO"))
            Me.txtCarBlType.Text = Convert.ToString(dr.Item("CARRIERBLTYPE"))
            Me.txtDemAcct.Text = Convert.ToString(dr.Item("DEMUFORACCT"))
            Me.txtMeasurement.Text = Convert.ToString(dr.Item("MEASUREMENT"))
            Me.txtMerDecValue.Text = Convert.ToString(dr.Item("DECLAREDVALUE"))
            Me.txtRevenueTons.Text = Convert.ToString(dr.Item("REVENUETONS"))
            Me.txtRate.Text = Convert.ToString(dr.Item("RATE"))
            Me.txtPer.Text = Convert.ToString(dr.Item("PER"))
            Me.txtPrepaid.Text = Convert.ToString(dr.Item("PREPAID"))
            Me.txtCollect.Text = Convert.ToString(dr.Item("COLLECT"))
            Me.txtPaymentPlace.Text = Convert.ToString(dr.Item("PAYMENTPLACE"))
            Me.txtBlIssuePlace.Text = Convert.ToString(dr.Item("BLISSUEPLACE"))
            Me.txtAnIssuePlace.Text = Convert.ToString(dr.Item("ANISSUEPLACE"))
            Me.txtDecOfGdText.Text = Convert.ToString(dr.Item("GOODSPKGS"))

            Me.txtPlaceOfReceipt.Text = Convert.ToString(dr.Item("BLRECEIPT1"))
            Me.txtPortOfLoading.Text = Convert.ToString(dr.Item("BLLOADING1"))
            Me.txtPortOfDischarge.Text = Convert.ToString(dr.Item("BLDISCHARGE1"))
            Me.txtPlaceOfDelivery.Text = Convert.ToString(dr.Item("BLDELIVERY1"))
            Me.txtBlPlaceDateIssue.Text = Convert.ToString(dr.Item("BLPLACEDATEISSUE1"))

            Me.txtVsl2nd.Text = Convert.ToString(dr.Item("TRANSIT1VSL1"))
            Me.txtVoy2nd.Text = Convert.ToString(dr.Item("TRANSIT1VOY1"))
            Me.txtVsl3rd.Text = Convert.ToString(dr.Item("TRANSIT2VSL1"))
            Me.txtVoy3rd.Text = Convert.ToString(dr.Item("TRANSIT2VOY1"))

        Else
            'Me.hdnBLNo.Value = Convert.ToString(dr.Item("BLID2"))
            Me.lblBlNo.Text = Convert.ToString(dr.Item("BLID2"))
            Me.txtCarrier.Text = Convert.ToString(dr.Item("CARRIER2"))
            'Me.txtDateOfIssue.Text = Convert.ToString(dr.Item("BLAPPDATE2"))
            'Me.txtRecieptCountry.Text = Convert.ToString(dr.Item("RECIEPTCOUNTRY2"))
            'Me.txtRecieptPort.Text = Convert.ToString(dr.Item("RECIEPTPORT2"))
            'If Me.txtRecieptCountry.Text <> "" AndAlso Me.txtRecieptPort.Text <> "" Then
            '    Dim portDt As DataTable = Me.GetPort(Me.txtRecieptCountry.Text, Me.txtRecieptPort.Text)
            '    If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
            '        Me.lblRecieptPortText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            '    End If
            'End If
            Me.hdnLoadCountry.Value = Convert.ToString(dr.Item("LOADCOUNTRY2"))
            'Me.txtLoadPort.Text = Convert.ToString(dr.Item("LOADPORT2"))
            'If Me.txtLoadCountry.Text <> "" AndAlso Me.txtLoadPort.Text <> "" Then
            '    Dim portDt As DataTable = Me.GetPort(Me.txtLoadCountry.Text, Me.txtLoadPort.Text)
            '    If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
            '        Me.lblLoadPortText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            '    End If
            'End If
            'Me.txtDischargeCountry.Text = Convert.ToString(dr.Item("DISCHARGECOUNTRY2"))
            'Me.txtDischargePort.Text = Convert.ToString(dr.Item("DISCHARGEPORT2"))
            'If Me.txtDischargeCountry.Text <> "" AndAlso Me.txtDischargePort.Text <> "" Then
            '    Dim portDt As DataTable = Me.GetPort(Me.txtDischargeCountry.Text, Me.txtDischargePort.Text)
            '    If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
            '        Me.lblDischargePortText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            '    End If
            'End If
            'Me.txtDeliveryCountry.Text = Convert.ToString(dr.Item("DELIVERYCOUNTRY2"))
            'Me.txtDeliveryPort.Text = Convert.ToString(dr.Item("DELIVERYPORT2"))
            'If Me.txtDeliveryCountry.Text <> "" AndAlso Me.txtDeliveryPort.Text <> "" Then
            '    Dim portDt As DataTable = Me.GetPort(Me.txtDeliveryCountry.Text, Me.txtDeliveryPort.Text)
            '    If portDt IsNot Nothing AndAlso portDt.Rows.Count > 0 Then
            '        Me.lblDeliveryPortText.Text = Convert.ToString(portDt.Rows(0).Item("NAME"))
            '    End If
            'End If
            Me.txtVessel.Text = Convert.ToString(dr.Item("VSL2"))
            Me.txtVoyNo.Text = Convert.ToString(dr.Item("VOY2"))
            Me.txtCargoReleaseText.Text = Convert.ToString(dr.Item("NOTIFYCONTTEXT2"))

            Me.txtLdnVessel.Text = Convert.ToString(dr.Item("LDNVSL2"))
            Me.txtLdnPol.Text = Convert.ToString(dr.Item("LDNPOL2"))
            Dim ldnDate As Date = Nothing
            If Date.TryParse(Convert.ToString(dr.Item("LDNDATE2")), ldnDate) Then
                Me.txtLdnDate.Text = ldnDate.ToString(GBA00003UserSetting.DATEFORMAT)
            Else
                Me.txtLdnDate.Text = Convert.ToString(dr.Item("LDNDATE2"))
            End If

            Me.txtLdnBy.Text = Convert.ToString(dr.Item("LDNBY2"))
            Me.txtBookingNo.Text = Convert.ToString(dr.Item("BOOKINGNO2"))

            Me.txtShipperText.Text = Convert.ToString(dr.Item("SHIPPERTEXT2"))
            Me.txtConsigneeText.Text = Convert.ToString(dr.Item("CONSIGNEETEXT2"))
            Me.txtNotifyPartyText.Text = Convert.ToString(dr.Item("NOTIFYTEXT2"))
            Me.txtPreCarriageBy.Text = Convert.ToString(dr.Item("PRECARRIAGETEXT2"))
            Me.txtFnlDest.Text = Convert.ToString(dr.Item("FINDESTINATIONTEXT2"))
            Me.txtNoOfBl.Text = Convert.ToString(dr.Item("NOOFBL2"))
            Me.txtBlType.Text = Convert.ToString(dr.Item("BLTYPE2"))
            Me.txtCarrierBlNo.Text = Convert.ToString(dr.Item("CARRIERBLNO2"))
            Me.txtCarBlType.Text = Convert.ToString(dr.Item("CARRIERBLTYPE2"))
            Me.txtDemAcct.Text = Convert.ToString(dr.Item("DEMUFORACCT2"))
            Me.txtMeasurement.Text = Convert.ToString(dr.Item("MEASUREMENT2"))
            Me.txtMerDecValue.Text = Convert.ToString(dr.Item("DECLAREDVALUE2"))
            Me.txtRevenueTons.Text = Convert.ToString(dr.Item("REVENUETONS2"))
            Me.txtRate.Text = Convert.ToString(dr.Item("RATE2"))
            Me.txtPer.Text = Convert.ToString(dr.Item("PER2"))
            Me.txtPrepaid.Text = Convert.ToString(dr.Item("PREPAID2"))
            Me.txtCollect.Text = Convert.ToString(dr.Item("COLLECT2"))
            Me.txtPaymentPlace.Text = Convert.ToString(dr.Item("PAYMENTPLACE2"))
            Me.txtBlIssuePlace.Text = Convert.ToString(dr.Item("BLISSUEPLACE2"))
            Me.txtAnIssuePlace.Text = Convert.ToString(dr.Item("ANISSUEPLACE2"))

            Me.txtPlaceOfReceipt.Text = Convert.ToString(dr.Item("BLRECEIPT2"))
            Me.txtPortOfLoading.Text = Convert.ToString(dr.Item("BLLOADING2"))
            Me.txtPortOfDischarge.Text = Convert.ToString(dr.Item("BLDISCHARGE2"))
            Me.txtPlaceOfDelivery.Text = Convert.ToString(dr.Item("BLDELIVERY2"))
            Me.txtBlPlaceDateIssue.Text = Convert.ToString(dr.Item("BLPLACEDATEISSUE2"))

            Me.txtVsl2nd.Text = Convert.ToString(dr.Item("TRANSIT1VSL2"))
            Me.txtVoy2nd.Text = Convert.ToString(dr.Item("TRANSIT1VOY2"))
            Me.txtVsl3rd.Text = Convert.ToString(dr.Item("TRANSIT2VSL2"))
            Me.txtVoy3rd.Text = Convert.ToString(dr.Item("TRANSIT2VOY2"))

        End If
        'Me.txtNoContainerPkg.Text = Convert.ToString(dr.Item("CONTAINERNO"))
        Me.txtFreightCharges.Text = Convert.ToString(dr.Item("FREIGHTANDCHARGES"))
        Me.txtMarksNumbers.Text = Convert.ToString(dr.Item("MARKSANDNUMBERS"))
        'Me.txtSay.Text = Convert.ToString(dr.Item("CONTAINERPKGS"))
        'Me.txtTotalNumCont.Text = Convert.ToString(dr.Item("NOOFCONTAINER"))
        'Me.txtBookingNo.Text = Convert.ToString(dr.Item("BOOKINGNO"))
        'Me.txtTermType.Text = Convert.ToString(dr.Item("TERMTYPE"))
        'If Me.txtTermType.Text <> "" Then

        '    Me.lblTermType.Text = ""
        '    Dim termTypeItem As ListItem = Me.lbTerm.Items.FindByValue(Me.txtTermType.Text)
        '    If termTypeItem IsNot Nothing Then

        '        Me.lblTermTypeText.Text = termTypeItem.Text
        '    End If
        'End If
        'Me.txtExchangeRate.Text = Convert.ToString(dr.Item("EXCHANGERATE"))
        'Me.txtPrepaidAt.Text = Convert.ToString(dr.Item("PREPAIDAT"))
        'Me.txtPayableAt.Text = Convert.ToString(dr.Item("PAYABLEAT"))
        'Me.txtLocalCurrency.Text = Convert.ToString(dr.Item("LOCALCURRENCY"))

        Me.hdnProductName.Value = Convert.ToString(dr.Item("PRODUCTNAME"))

        'Me.lblShipLineText.Text = ""
        'If Me.txtShipLine.Text <> "" Then
        '    SetDisplayShipLine(Me.txtShipLine, Me.txtShipLine.Text)
        'End If

    End Sub

    ''' <summary>
    ''' 明細用のリストデータ作成
    ''' </summary>
    ''' <param name="dt">データテーブル</param>
    ''' <returns>タブ切り替え等での途中入力を保持する</returns>
    Function CreateTemporaryInfoList(dt As DataTable) As List(Of COSTITEM)

        Dim retList As New List(Of COSTITEM)

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim uniqueIndex As Integer = 0

            For Each dr As DataRow In dt.Rows

                Dim item As New COSTITEM
                item.OrderNo = Convert.ToString(dr.Item("ORDERNO"))
                item.TankNo = Convert.ToString(dr.Item("TANKNO"))
                item.TankSeq = Convert.ToString(dr.Item("TANKSEQ"))
                item.TankType = Convert.ToString(dr.Item("TANKTYPE"))
                item.SealNo1 = Convert.ToString(dr.Item("SEALNO1"))
                item.SealNo2 = Convert.ToString(dr.Item("SEALNO2"))
                item.SealNo3 = Convert.ToString(dr.Item("SEALNO3"))
                item.SealNo4 = Convert.ToString(dr.Item("SEALNO4"))
                item.GrossWeight = Convert.ToString(dr.Item("GROSSWEIGHT"))
                item.NetWeight = Convert.ToString(dr.Item("NETWEIGHT"))
                item.EmptyOrFull = Convert.ToString(dr.Item("EMPTYORFULL"))
                item.NoOfPackage = Convert.ToString(dr.Item("NOOFPACKAGE"))
                item.ShipRateEx = Convert.ToString(dr.Item("EXSHIPRATE"))
                item.ShipRateIn = Convert.ToString(dr.Item("INSHIPRATE"))
                item.TareWeight = Convert.ToString(dr.Item("TAREWEIGHT"))

                item.DispSeq = Convert.ToString(dr.Item("DISPSEQ"))
                item.WorkC1 = Convert.ToString(dr.Item("WORKC1"))
                item.WorkC2 = Convert.ToString(dr.Item("WORKC2"))
                item.WorkC3 = Convert.ToString(dr.Item("WORKC3"))
                item.WorkC4 = Convert.ToString(dr.Item("WORKC4"))
                item.WorkC5 = Convert.ToString(dr.Item("WORKC5"))
                item.WorkF1 = Convert.ToString(dr.Item("WORKF1"))
                item.WorkF2 = Convert.ToString(dr.Item("WORKF2"))
                item.WorkF3 = Convert.ToString(dr.Item("WORKF3"))
                item.WorkF4 = Convert.ToString(dr.Item("WORKF4"))
                item.WorkF5 = Convert.ToString(dr.Item("WORKF5"))

                retList.Add(item)

                item.UniqueIndex = uniqueIndex

                uniqueIndex = uniqueIndex + 1

            Next

        End If

        Return retList
    End Function
    Private Sub SaveGridItem(Optional initflg As Boolean = False)
        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim retCostList = (From allCostItem In allCostList Where allCostItem.OrderNo <> "").ToList

        Dim correctDispCostList As New List(Of COSTITEM)
        For Each gridItem As GridViewRow In Me.gvDetailInfo.Rows
            Dim item As New COSTITEM
            item.OrderNo = DirectCast(gridItem.FindControl("hdnOrderNo"), HiddenField).Value
            item.TankNo = DirectCast(gridItem.FindControl("txtTankNo"), TextBox).Text
            item.TankSeq = DirectCast(gridItem.FindControl("hdnTankSeq"), HiddenField).Value
            item.TankType = DirectCast(gridItem.FindControl("txtTankType"), TextBox).Text
            item.SealNo1 = DirectCast(gridItem.FindControl("txtSealNo1"), TextBox).Text
            item.SealNo2 = DirectCast(gridItem.FindControl("txtSealNo2"), TextBox).Text
            item.SealNo3 = DirectCast(gridItem.FindControl("txtSealNo3"), TextBox).Text
            item.SealNo4 = DirectCast(gridItem.FindControl("txtSealNo4"), TextBox).Text
            item.GrossWeight = DirectCast(gridItem.FindControl("txtGrossWeight"), TextBox).Text
            item.NetWeight = DirectCast(gridItem.FindControl("txtNetWeight"), TextBox).Text
            item.EmptyOrFull = DirectCast(gridItem.FindControl("txtEmptyOrFull"), TextBox).Text
            item.NoOfPackage = DirectCast(gridItem.FindControl("txtNoOfPackage"), TextBox).Text
            item.ShipRateEx = txtShipRateEx.Text
            item.ShipRateIn = txtShipRateIn.Text
            item.TareWeight = DirectCast(gridItem.FindControl("hdnTareWeight"), HiddenField).Value

            item.DispSeq = DirectCast(gridItem.FindControl("txtDispSeq"), TextBox).Text
            'item.WorkC1 = ""
            'item.WorkC2 = ""
            'item.WorkC3 = ""
            'item.WorkC4 = ""
            'item.WorkC5 = ""
            'item.WorkF1 = ""
            'item.WorkF2 = ""
            'item.WorkF3 = ""
            'item.WorkF4 = ""
            'item.WorkF5 = ""

            Dim uniqueIndexString = DirectCast(gridItem.FindControl("hdnUniqueIndex"), HiddenField).Value
            Dim uniqueIndex = 0
            Integer.TryParse(uniqueIndexString, uniqueIndex)
            item.UniqueIndex = uniqueIndex

            correctDispCostList.Add(item)
        Next

        If initflg Then
            correctDispCostList = retCostList
            If retCostList.Count > 0 Then
                txtShipRateEx.Text = NumberFormat(retCostList(0).ShipRateEx, "", "#,##0.00")
                txtShipRateIn.Text = NumberFormat(retCostList(0).ShipRateIn, "", "#,##0.00")
            Else
                txtShipRateEx.Text = "0.00"
                txtShipRateIn.Text = "0.00"
            End If
        End If

        ViewState("COSTLIST") = correctDispCostList

    End Sub
    ''' <summary>
    ''' 表示非表示制御
    ''' </summary>
    ''' <param name="selectedTab"></param>
    Private Sub visibleControl(ByVal selectedTab As String)
        Dim disableAll As Boolean = False
        If Me.hdnIsViewOnlyPopup.Value = "1" Then
            disableAll = True
        End If

        '一旦入力項目の表示を非表示にする
        Dim allVisibleControls As New List(Of HtmlControl)
        allVisibleControls.AddRange({Me.divBlDetailInfo,
                                     Me.divTankDetailInfo,
                                     Me.divOtherDetailInfo,
                                     Me.btnOutputExcel,
                                     Me.btnOutputFile,
                                     Me.btnPrint,
                                     Me.btnPDFPrint,
                                     Me.btnSave,
                                     Me.divFileUpInfo})
        For Each item In allVisibleControls
            item.Visible = False
        Next
        Dim visibleControls As New List(Of HtmlControl)
        If selectedTab = Me.tabBL.ClientID Then

            visibleControls.AddRange({Me.divBlDetailInfo, Me.btnPrint, Me.btnPDFPrint, Me.btnSave})

        ElseIf selectedTab = Me.tabTank.ClientID Then

            visibleControls.AddRange({Me.divTankDetailInfo, Me.btnOutputExcel, Me.btnPrint, Me.btnPDFPrint, Me.btnSave})

        ElseIf selectedTab = Me.tabOther.ClientID Then

            visibleControls.AddRange({Me.divOtherDetailInfo, Me.btnPrint, Me.btnPDFPrint, Me.btnSave})

        ElseIf selectedTab = Me.tabFileUp.ClientID Then

            visibleControls.AddRange({Me.divFileUpInfo, Me.btnOutputFile, Me.btnSave})

        End If

        '対象のアイテムを表示
        For Each item In visibleControls
            item.Visible = True
        Next
        If disableAll = True Then
            Me.spnActButtonBox.Visible = False
        End If

    End Sub
    ''' <summary>
    ''' 非活性制御
    ''' </summary>
    Private Sub enabledControls()

        'Importの場合船社レートのみ編集可能
        If Me.hdnDepartureArrival.Value = "02IMPORT" Then

            Dim controlObjects As New List(Of TextBox) _
                             From {Me.txtShipperText, Me.txtConsigneeText,
                                   Me.txtNotifyPartyText, Me.txtCargoReleaseText,
                                   Me.txtPreCarriageBy, Me.txtVessel,
                                   Me.txtVoyNo, Me.txtFnlDest,
                                   Me.txtShipRateEx, Me.txtBookingNo,
                                   Me.txtNoOfBl, Me.txtCarBlType,
                                   Me.txtCarrierBlNo, Me.txtBlType,
                                   Me.txtDemAcct, Me.txtMeasurement,
                                   Me.txtRevenueTons, Me.txtRate,
                                   Me.txtPer, Me.txtPrepaid,
                                   Me.txtCollect, Me.txtMerDecValue,
                                   Me.txtPaymentPlace, Me.txtBlIssuePlace,
                                   Me.txtAnIssuePlace, Me.txtLdnVessel,
                                   Me.txtLdnPol, Me.txtLdnDate,
                                   Me.txtLdnBy, Me.txtCarrier, Me.txtFreightCharges, Me.txtMarksNumbers,
                                   Me.txtVsl2nd, Me.txtVoy2nd, Me.txtVsl3rd, Me.txtVoy3rd}
            For Each controlObj In controlObjects
                controlObj.Enabled = False
            Next

            Dim tankTypeIdx As Integer = 0
            Dim sealNo1Idx As Integer = 0
            Dim sealNo2Idx As Integer = 0
            Dim sealNo3Idx As Integer = 0
            Dim sealNo4Idx As Integer = 0
            Dim netWeightIdx As Integer = 0
            Dim emptyOrFullIdx As Integer = 0
            Dim noOfPackageIdx As Integer = 0
            Dim dispSeqIdx As Integer = 0
            For colIdx As Integer = 0 To Me.gvDetailInfo.Columns.Count - 1
                If Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "TankType" Then
                    tankTypeIdx = colIdx
                ElseIf Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "SealNo1" Then
                    sealNo1Idx = colIdx
                ElseIf Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "SealNo2" Then
                    sealNo2Idx = colIdx
                ElseIf Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "SealNo3" Then
                    sealNo3Idx = colIdx
                ElseIf Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "SealNo4" Then
                    sealNo4Idx = colIdx
                ElseIf Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "NetWeight" Then
                    netWeightIdx = colIdx
                ElseIf Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "EmptyOrFull" Then
                    emptyOrFullIdx = colIdx
                ElseIf Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "NoOfPackage" Then
                    noOfPackageIdx = colIdx
                ElseIf Me.gvDetailInfo.Columns(colIdx).HeaderStyle.CssClass = "DispSeq" Then
                    dispSeqIdx = colIdx
                End If
            Next

            For i As Integer = 0 To gvDetailInfo.Rows.Count - 1

                Me.gvDetailInfo.Rows(i).Cells(tankTypeIdx).Enabled = False
                Me.gvDetailInfo.Rows(i).Cells(sealNo1Idx).Enabled = False
                Me.gvDetailInfo.Rows(i).Cells(sealNo2Idx).Enabled = False
                Me.gvDetailInfo.Rows(i).Cells(sealNo3Idx).Enabled = False
                Me.gvDetailInfo.Rows(i).Cells(sealNo4Idx).Enabled = False
                Me.gvDetailInfo.Rows(i).Cells(netWeightIdx).Enabled = False
                Me.gvDetailInfo.Rows(i).Cells(emptyOrFullIdx).Enabled = False
                Me.gvDetailInfo.Rows(i).Cells(noOfPackageIdx).Enabled = False
                Me.gvDetailInfo.Rows(i).Cells(dispSeqIdx).Enabled = False

            Next

        Else
            'Export
            Me.txtShipRateIn.Enabled = False

        End If

        '第2輸送の場合
        If Me.hdnWhichTrans.Value = "2" Then
            Me.txtDecOfGdText.Enabled = False
        End If

    End Sub
    ''' <summary>
    ''' 右の出力帳票
    ''' </summary>
    ''' <param name="isOwner">オーナータブ</param>
    Private Function RightboxInit(isOwner As Boolean) As String
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim excelMapId As String = "GBT00014"

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
                    Me.lbRightList.Items.Add(listItem)
                Next
            Catch ex As Exception
            End Try
        Else
            retVal = COA0022ProfXls.ERR
            Return retVal
        End If

        'レポートID変数検索
        COA0016VARIget.MAPID = excelMapId
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = "Default"
        COA0016VARIget.FIELD = "REPORTID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR <> C_MESSAGENO.NORMAL Then
            retVal = COA0016VARIget.ERR
            Return retVal
        End If

        'ListBox選択
        Me.lbRightList.SelectedIndex = -1     '選択無しの場合、デフォルト
        Dim targetListItem = lbRightList.Items.FindByValue(COA0016VARIget.VALUE)
        If targetListItem IsNot Nothing Then
            targetListItem.Selected = True
        Else
            If Me.lbRightList.Items.Count > 0 Then
                Me.lbRightList.SelectedIndex = 0
            End If
        End If


        Dim printMapId As String = "GBT00014Print"

        'レポートID情報
        COA0022ProfXls.MAPID = printMapId
        COA0022ProfXls.COA0022getReportId()
        Me.lbRightListPrint.Items.Clear() '一旦選択肢をクリア
        If COA0022ProfXls.ERR = C_MESSAGENO.NORMAL Then
            Try
                Dim listBoxObj As ListBox = DirectCast(COA0022ProfXls.REPORTOBJ, ListBox)
                For Each listItem As ListItem In listBoxObj.Items
                    If listItem.Value <> "Attached" AndAlso listItem.Value <> "JOTDR_BLInstructionONE_CLP" Then
                        Me.lbRightListPrint.Items.Add(listItem)
                    End If
                Next
            Catch ex As Exception
            End Try
        Else
            retVal = COA0022ProfXls.ERR
            Return retVal
        End If

        'レポートID変数検索
        COA0016VARIget.MAPID = printMapId
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = "BLList"
        COA0016VARIget.FIELD = "REPORTID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR <> C_MESSAGENO.NORMAL Then
            retVal = COA0016VARIget.ERR
            Return retVal
        End If

        'ListBox選択
        lbRightListPrint.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To lbRightListPrint.Items.Count - 1
            If lbRightListPrint.Items(i).Value = COA0016VARIget.VALUE Then
                lbRightListPrint.SelectedIndex = i
            End If
        Next

        Return retVal
    End Function
    ''' <summary>
    ''' 画面入力内容を収集しデータテーブルに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function CollectDisplayOrderBase() As DataTable
        'データテーブルのガワを作成
        Dim retDt As DataTable = CreateOrderBaseTable()
        Dim dr As DataRow = retDt.Rows(0)
        dr.Item("ORDERNO") = Me.lblOrderNo.Text

        If hdnWhichTrans.Value = "1" Then
            'dr.Item("BLID1") = Me.hdnBLNo.Value
            dr.Item("BLID1") = Me.lblBlNo.Text
            dr.Item("CARRIER1") = Me.txtCarrier.Text
            'dr.Item("BLAPPDATE1") = Me.txtDateOfIssue.Text
            'dr.Item("RECIEPTCOUNTRY1") = Me.txtRecieptCountry.Text
            'dr.Item("RECIEPTPORT1") = Me.txtRecieptPort.Text
            'dr.Item("LOADCOUNTRY1") = Me.txtLoadCountry.Text
            'dr.Item("LOADPORT1") = Me.txtLoadPort.Text
            'dr.Item("DISCHARGECOUNTRY1") = Me.txtDischargeCountry.Text
            'dr.Item("DISCHARGEPORT1") = Me.txtDischargePort.Text
            'dr.Item("DELIVERYCOUNTRY1") = Me.txtDeliveryCountry.Text
            'dr.Item("DELIVERYPORT1") = Me.txtDeliveryPort.Text
            dr.Item("VSL1") = Me.txtVessel.Text
            dr.Item("VOY1") = Me.txtVoyNo.Text
            dr.Item("NOTIFYCONTTEXT1") = Me.txtCargoReleaseText.Text

            dr.Item("LDNVSL1") = Me.txtLdnVessel.Text
            dr.Item("LDNPOL1") = Me.txtLdnPol.Text
            Dim ldnDate1 As Date = Nothing
            If Date.TryParseExact(Me.txtLdnDate.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, ldnDate1) Then
                dr.Item("LDNDATE1") = ldnDate1.ToString("yyyy/MM/dd")
            Else
                dr.Item("LDNDATE1") = Me.txtLdnDate.Text
            End If
            dr.Item("LDNBY1") = Me.txtLdnBy.Text
            dr.Item("BOOKINGNO") = Me.txtBookingNo.Text

            dr.Item("SHIPPERTEXT") = Me.txtShipperText.Text
            dr.Item("CONSIGNEETEXT") = Me.txtConsigneeText.Text
            dr.Item("NOTIFYTEXT") = Me.txtNotifyPartyText.Text
            dr.Item("PRECARRIAGETEXT") = Me.txtPreCarriageBy.Text
            dr.Item("FINDESTINATIONTEXT") = Me.txtFnlDest.Text
            dr.Item("DECLAREDVALUE") = Me.txtMerDecValue.Text
            dr.Item("REVENUETONS") = Me.txtRevenueTons.Text
            dr.Item("RATE") = Me.txtRate.Text
            dr.Item("PER") = Me.txtPer.Text
            dr.Item("PREPAID") = Me.txtPrepaid.Text
            dr.Item("COLLECT") = Me.txtCollect.Text
            dr.Item("CARRIERBLNO") = Me.txtCarrierBlNo.Text
            dr.Item("NOOFBL") = Me.txtNoOfBl.Text
            dr.Item("BLTYPE") = Me.txtBlType.Text
            dr.Item("PAYMENTPLACE") = Me.txtPaymentPlace.Text
            dr.Item("BLISSUEPLACE") = Me.txtBlIssuePlace.Text
            dr.Item("ANISSUEPLACE") = Me.txtAnIssuePlace.Text
            dr.Item("MEASUREMENT") = Me.txtMeasurement.Text
            dr.Item("CARRIERBLTYPE") = Me.txtCarBlType.Text
            dr.Item("DEMUFORACCT") = Me.txtDemAcct.Text
            dr.Item("GOODSPKGS") = Me.txtDecOfGdText.Text

            dr.Item("BLRECEIPT1") = Me.txtPlaceOfReceipt.Text
            dr.Item("BLLOADING1") = Me.txtPortOfLoading.Text
            dr.Item("BLDISCHARGE1") = Me.txtPortOfDischarge.Text
            dr.Item("BLDELIVERY1") = Me.txtPlaceOfDelivery.Text
            dr.Item("BLPLACEDATEISSUE1") = Me.txtBlPlaceDateIssue.Text

            dr.Item("TRANSIT1VSL1") = Me.txtVsl2nd.Text
            dr.Item("TRANSIT1VOY1") = Me.txtVoy2nd.Text
            dr.Item("TRANSIT2VSL1") = Me.txtVsl3rd.Text
            dr.Item("TRANSIT2VOY1") = Me.txtVoy3rd.Text

        Else
            'dr.Item("BLID2") = Me.hdnBLNo.Value
            dr.Item("BLID2") = Me.lblBlNo.Text
            dr.Item("CARRIER2") = Me.txtCarrier.Text
            'dr.Item("BLAPPDATE2") = Me.txtDateOfIssue.Text
            'dr.Item("RECIEPTCOUNTRY2") = Me.txtRecieptCountry.Text
            'dr.Item("RECIEPTPORT2") = Me.txtRecieptPort.Text
            'dr.Item("LOADCOUNTRY2") = Me.txtLoadCountry.Text
            'dr.Item("LOADPORT2") = Me.txtLoadPort.Text
            'dr.Item("DISCHARGECOUNTRY2") = Me.txtDischargeCountry.Text
            'dr.Item("DISCHARGEPORT2") = Me.txtDischargePort.Text
            'dr.Item("DELIVERYCOUNTRY2") = Me.txtDeliveryCountry.Text
            'dr.Item("DELIVERYPORT2") = Me.txtDeliveryPort.Text
            dr.Item("VSL2") = Me.txtVessel.Text
            dr.Item("VOY2") = Me.txtVoyNo.Text
            dr.Item("NOTIFYCONTTEXT2") = Me.txtCargoReleaseText.Text

            dr.Item("LDNVSL2") = Me.txtLdnVessel.Text
            dr.Item("LDNPOL2") = Me.txtLdnPol.Text
            Dim ldnDate2 As Date = Nothing
            If Date.TryParseExact(Me.txtLdnDate.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, ldnDate2) Then
                dr.Item("LDNDATE2") = ldnDate2.ToString("yyyy/MM/dd")
            Else
                dr.Item("LDNDATE2") = Me.txtLdnDate.Text
            End If
            dr.Item("LDNBY2") = Me.txtLdnBy.Text
            dr.Item("BOOKINGNO2") = Me.txtBookingNo.Text

            dr.Item("SHIPPERTEXT2") = Me.txtShipperText.Text
            dr.Item("CONSIGNEETEXT2") = Me.txtConsigneeText.Text
            dr.Item("NOTIFYTEXT2") = Me.txtNotifyPartyText.Text
            dr.Item("PRECARRIAGETEXT2") = Me.txtPreCarriageBy.Text
            dr.Item("FINDESTINATIONTEXT2") = Me.txtFnlDest.Text
            dr.Item("DECLAREDVALUE2") = Me.txtMerDecValue.Text
            dr.Item("REVENUETONS2") = Me.txtRevenueTons.Text
            dr.Item("RATE2") = Me.txtRate.Text
            dr.Item("PER2") = Me.txtPer.Text
            dr.Item("PREPAID2") = Me.txtPrepaid.Text
            dr.Item("COLLECT2") = Me.txtCollect.Text
            dr.Item("CARRIERBLNO2") = Me.txtCarrierBlNo.Text
            dr.Item("NOOFBL2") = Me.txtNoOfBl.Text
            dr.Item("BLTYPE2") = Me.txtBlType.Text
            dr.Item("PAYMENTPLACE2") = Me.txtPaymentPlace.Text
            dr.Item("BLISSUEPLACE2") = Me.txtBlIssuePlace.Text
            dr.Item("ANISSUEPLACE2") = Me.txtAnIssuePlace.Text
            dr.Item("MEASUREMENT2") = Me.txtMeasurement.Text
            dr.Item("CARRIERBLTYPE2") = Me.txtCarBlType.Text
            dr.Item("DEMUFORACCT2") = Me.txtDemAcct.Text

            dr.Item("BLRECEIPT2") = Me.txtPlaceOfReceipt.Text
            dr.Item("BLLOADING2") = Me.txtPortOfLoading.Text
            dr.Item("BLDISCHARGE2") = Me.txtPortOfDischarge.Text
            dr.Item("BLDELIVERY2") = Me.txtPlaceOfDelivery.Text
            dr.Item("BLPLACEDATEISSUE2") = Me.txtBlPlaceDateIssue.Text

            dr.Item("TRANSIT1VSL2") = Me.txtVsl2nd.Text
            dr.Item("TRANSIT1VOY2") = Me.txtVoy2nd.Text
            dr.Item("TRANSIT2VSL2") = Me.txtVsl3rd.Text
            dr.Item("TRANSIT2VOY2") = Me.txtVoy3rd.Text

        End If

        'dr.Item("CONTAINERNO") = Me.txtNoContainerPkg.Text
        dr.Item("FREIGHTANDCHARGES") = Me.txtFreightCharges.Text
        dr.Item("MARKSANDNUMBERS") = Me.txtMarksNumbers.Text
        Dim cnvStr As String = GetConvEng()
        dr.Item("CONTAINERPKGS") = cnvStr & " TANK CONTAINER(S) ONLY"
        'dr.Item("NOOFCONTAINER") = Me.txtTotalNumCont.Text
        'dr.Item("BOOKINGNO") = Me.txtBookingNo.Text
        'dr.Item("TERMTYPE") = Me.txtTermType.Text
        dr.Item("NOOFPACKAGE") = Me.iptNoOfPackage.Value
        'dr.Item("EXCHANGERATE") = Me.txtExchangeRate.Text
        'dr.Item("PREPAIDAT") = Me.txtPrepaidAt.Text
        'dr.Item("PAYABLEAT") = Me.txtPayableAt.Text
        'dr.Item("LOCALCURRENCY") = Me.txtLocalCurrency.Text

        dr.Item("PRODUCTNAME") = Me.hdnProductName.Value

        Return retDt
    End Function
    ''' <summary>
    ''' 入力チェック、データ登録、Excel出力時に使用するため画面情報をデータテーブルに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function CollectDisplayOrderValue() As DataTable
        Dim retDt As DataTable = CreateOrderValueTable()

        Dim targetCostData As List(Of COSTITEM) = Nothing
        SaveGridItem()
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        targetCostData = (From costItemRow In costData Where costItemRow.OrderNo <> "").ToList

        For Each costItem In targetCostData
            Dim dr As DataRow = retDt.NewRow
            dr.Item("ORDERNO") = costItem.OrderNo
            dr.Item("TANKNO") = costItem.TankNo
            dr.Item("TANKSEQ") = costItem.TankSeq
            dr.Item("TANKTYPE") = costItem.TankType
            dr.Item("SEALNO1") = costItem.SealNo1
            dr.Item("SEALNO2") = costItem.SealNo2
            dr.Item("SEALNO3") = costItem.SealNo3
            dr.Item("SEALNO4") = costItem.SealNo4
            dr.Item("GROSSWEIGHT") = costItem.GrossWeight
            dr.Item("NETWEIGHT") = costItem.NetWeight
            dr.Item("EMPTYORFULL") = costItem.EmptyOrFull
            dr.Item("NOOFPACKAGE") = costItem.NoOfPackage
            dr.Item("EXSHIPRATE") = costItem.ShipRateEx
            dr.Item("INSHIPRATE") = costItem.ShipRateIn
            dr.Item("TAREWEIGHT") = costItem.TareWeight

            dr.Item("DISPSEQ") = costItem.DispSeq
            'dr.Item("WORKC1") = costItem.WorkC1
            'dr.Item("WORKC2") = costItem.WorkC2
            'dr.Item("WORKC3") = costItem.WorkC3
            'dr.Item("WORKC4") = costItem.WorkC4
            'dr.Item("WORKC5") = costItem.WorkC5
            'dr.Item("WORKF1") = costItem.WorkF1
            'dr.Item("WORKF2") = costItem.WorkF2
            'dr.Item("WORKF3") = costItem.WorkF3
            'dr.Item("WORKF4") = costItem.WorkF4
            'dr.Item("WORKF5") = costItem.WorkF5
            retDt.Rows.Add(dr)
        Next
        Return retDt
    End Function
    ''' <summary>
    ''' 帳票出力用のデータを収集しデータテーブルに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function CollectDisplayReportInfo() As DataTable
        'データテーブルのガワを作成
        Dim retDt As DataTable = CreateOrderValueTable()
        'Dim dr As DataRow = retDt.Rows(0)
        'dr.Item("ORDERNO") = Me.lblOrderNo.Text

        Dim targetData As List(Of COSTITEM) = Nothing
        SaveGridItem()
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        targetData = (From costItemRow In costData Where costItemRow.OrderNo <> "").ToList

        For Each dtItem In targetData
            Dim dtdr As DataRow = retDt.NewRow
            dtdr.Item("ORDERNO") = dtItem.OrderNo
            dtdr.Item("TANKNO") = dtItem.TankNo
            dtdr.Item("TANKSEQ") = dtItem.TankSeq
            dtdr.Item("TANKTYPE") = dtItem.TankType
            dtdr.Item("SEALNO1") = dtItem.SealNo1
            dtdr.Item("SEALNO2") = dtItem.SealNo2
            dtdr.Item("SEALNO3") = dtItem.SealNo3
            dtdr.Item("SEALNO4") = dtItem.SealNo4
            dtdr.Item("GROSSWEIGHT") = dtItem.GrossWeight
            dtdr.Item("NETWEIGHT") = dtItem.NetWeight
            dtdr.Item("EMPTYORFULL") = dtItem.EmptyOrFull
            dtdr.Item("NOOFPACKAGE") = dtItem.NoOfPackage
            dtdr.Item("EXSHIPRATE") = dtItem.ShipRateEx
            dtdr.Item("INSHIPRATE") = dtItem.ShipRateIn
            dtdr.Item("TAREWEIGHT") = dtItem.TareWeight
            dtdr.Item("DISPSEQ") = dtItem.DispSeq
            dtdr.Item("WORKC1") = dtItem.WorkC1
            dtdr.Item("WORKC2") = dtItem.WorkC2
            dtdr.Item("WORKC3") = dtItem.WorkC3
            dtdr.Item("WORKC4") = dtItem.WorkC4
            dtdr.Item("WORKC5") = dtItem.WorkC5
            dtdr.Item("WORKF1") = dtItem.WorkF1
            dtdr.Item("WORKF2") = dtItem.WorkF2
            dtdr.Item("WORKF3") = dtItem.WorkF3
            dtdr.Item("WORKF4") = dtItem.WorkF4
            dtdr.Item("WORKF5") = dtItem.WorkF5
            retDt.Rows.Add(dtdr)
        Next

        Return retDt
    End Function
    ''' <summary>
    ''' 入力チェック関数
    ''' </summary>
    ''' <param name="ds">チェック対象のデータセット（オーナ情報、費用情報）</param>
    ''' <param name="isMinCheck">一時保存するための最低限チェックのみか(True:最低限チェックのみ,False:完全チェック)</param>
    ''' <param name="isCheckAllTabs">全タブのチェックを行うか(True:全タブ,False:選択しているタブのみ)</param>
    ''' <returns>True:正常,False:異常</returns>
    Private Function CheckInput(ds As DataSet, isMinCheck As Boolean, Optional isCheckAllTabs As Boolean = False) As Boolean

        Dim ownerDt As DataTable = ds.Tables("ORDER_BASE")
        Dim costDt As DataTable = ds.Tables("ORDER_VALUE")
        Dim rightBoxMessage As New Text.StringBuilder
        Dim errMessage As String = ""
        Dim mapId As String = ""
        Dim hasError As Boolean = False
        Dim forcusFlg As Boolean = False
        If isMinCheck Then
            mapId = "GBT00014"
        Else
            mapId = "GBT00014"
        End If

        'Dim fieldList As New List(Of String) From {"ORDERNO", "BLID1", "CARRIER1", "BLAPPDATE1", "RECIEPTCOUNTRY1", "RECIEPTPORT1", "LOADCOUNTRY1", "LOADPORT1",
        '                                           "DISCHARGECOUNTRY1", "DISCHARGEPORT1", "DELIVERYCOUNTRY1", "DELIVERYPORT1", "VSL1", "VOY1", "BLID2", "CARRIER2",
        '                                           "BLAPPDATE2", "RECIEPTCOUNTRY2", "RECIEPTPORT2", "LOADCOUNTRY2", "LOADPORT2", "DISCHARGECOUNTRY2", "DISCHARGEPORT2",
        '                                           "DELIVERYCOUNTRY2", "DELIVERYPORT2", "VSL2", "VOY2", "SHIPPERTEXT", "CONSIGNEETEXT", "NOTIFYTEXT", "PRECARRIAGETEXT",
        '                                           "NOTIFYCONTTEXT1", "NOTIFYCONTTEXT2", "FINDESTINATIONTEXT", "CONTAINERNO", "GOODSPKGS", "FREIGHTANDCHARGES", "CONTAINERPKGS", "NOOFCONTAINER",
        '                                           "DECLAREDVALUE", "REVENUETONS", "RATE", "PER", "PREPAID", "COLLECT", "CARRIERBLNO", "BOOKINGNO", "TERMTYPE",
        '                                           "NOOFBL", "PAYMENTPLACE", "BLISSUEPLACE", "ANISSUEPLACE", "EXCHANGERATE", "PREPAIDAT", "PAYABLEAT", "LOCALCURRENCY", "MEASUREMENT", "EXSHIPRATE", "INSHIPRATE"}
        Dim fieldList As New List(Of String) From {"ORDERNO", "CARRIER1", "CARRIER2", "PRECARRIAGETEXT", "FINDESTINATIONTEXT",
                                                   "DECLAREDVALUE", "REVENUETONS", "RATE", "PER", "PREPAID", "COLLECT", "CARRIERBLNO", "BOOKINGNO", "BOOKINGNO2",
                                                   "NOOFBL", "BLTYPE", "PAYMENTPLACE", "BLISSUEPLACE", "ANISSUEPLACE", "MEASUREMENT", "EXSHIPRATE", "INSHIPRATE",
                                                   "SHIPPERTEXT", "CONSIGNEETEXT", "NOTIFYTEXT",
                                                   "VSL1", "VOY1", "NOTIFYCONTTEXT1", "LDNVSL1", "LDNPOL1", "LDNDATE1", "LDNBY1",
                                                   "VSL2", "VOY2", "NOTIFYCONTTEXT2", "LDNVSL2", "LDNPOL2", "LDNDATE2", "LDNBY2",
                                                   "SHIPPERTEXT2", "CONSIGNEETEXT2", "NOTIFYTEXT2", "PRECARRIAGETEXT2", "FINDESTINATIONTEXT2",
                                                   "NOOFBL2", "CARRIERBLTYPE2", "CARRIERBLNO2", "BLTYPE2", "DEMUFORACCT2", "MEASUREMENT2", "REVENUETONS2",
                                                   "RATE2", "PER2", "PREPAID2", "COLLECT2", "DECLAREDVALUE2", "PAYMENTPLACE2", "BLISSUEPLACE2", "ANISSUEPLACE2",
                                                   "CARRIERBLTYPE", "DEMUFORACCT", "GOODSPKGS", "FREIGHTANDCHARGES", "FREIGHTANDCHARGES",
                                                   "BLRECEIPT1", "BLRECEIPT2", "BLLOADING1", "BLLOADING2", "BLDISCHARGE1", "BLDISCHARGE2", "BLDELIVERY1", "BLDELIVERY2", "BLPLACEDATEISSUE1", "BLPLACEDATEISSUE2",
                                                   "TRANSIT1VSL1", "TRANSIT1VOY1", "TRANSIT2VSL1", "TRANSIT2VOY1", "TRANSIT1VSL2", "TRANSIT1VOY2", "TRANSIT2VSL2", "TRANSIT2VOY2"}
        If CheckSingle(mapId, ownerDt, fieldList, errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If

        'fieldList = New List(Of String) From {"TANKNO", "TANKTYPE", "SEALNO1", "SEALNO2", "SEALNO3", "SEALNO4", "GROSSWEIGHT", "NETWEIGHT", "EMPTYORFULL", "NOOFPACKAGE", "EXSHIPRATE"}
        fieldList = New List(Of String) From {"TANKNO", "TANKTYPE", "SEALNO1", "SEALNO2", "SEALNO3", "SEALNO4", "GROSSWEIGHT", "NETWEIGHT", "EMPTYORFULL", "NOOFPACKAGE", "EXSHIPRATE", "DISPSEQ"}
        If CheckSingle(mapId, costDt, fieldList, errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            hasError = True
        End If

        'リストチェック
        'If ChedckList(txtFreightCharges.Text, lbFrtAndCrg, "Freight and Charges", errMessage) <> C_MESSAGENO.NORMAL Then
        '    rightBoxMessage.Append(errMessage)
        '    hasError = True
        'End If

        If ChedckList(txtPaymentPlace.Text, lbCountry, "Payment Place", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            If Me.hdnSelectedTabId.Value = Me.tabOther.ClientID AndAlso forcusFlg = False Then
                txtPaymentPlace.Focus()
                forcusFlg = True
            End If
            hasError = True
        End If

        If ChedckList(txtBlIssuePlace.Text, lbCountry, "B/L Issue Place", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            If Me.hdnSelectedTabId.Value = Me.tabOther.ClientID AndAlso forcusFlg = False Then
                txtBlIssuePlace.Focus()
                forcusFlg = True
            End If
            hasError = True
        End If

        If ChedckList(txtAnIssuePlace.Text, lbCountry, "A/N Issue Place", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            If Me.hdnSelectedTabId.Value = Me.tabOther.ClientID AndAlso forcusFlg = False Then
                txtAnIssuePlace.Focus()
                forcusFlg = True
            End If
            hasError = True
        End If

        If ChedckList(txtBlType.Text, lbBlType, "B/L Type", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            If Me.hdnSelectedTabId.Value = Me.tabBL.ClientID AndAlso forcusFlg = False Then
                txtBlType.Focus()
                forcusFlg = True
            End If
            hasError = True
        End If

        If ChedckList(txtCarBlType.Text, lbCarBlType, "Carrier B/L Type", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            If Me.hdnSelectedTabId.Value = Me.tabBL.ClientID AndAlso forcusFlg = False Then
                txtCarBlType.Focus()
                forcusFlg = True
            End If
            hasError = True
        End If

        If ChedckList(txtCarrier.Text, lbCarrier, "Carrier", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            If Me.hdnSelectedTabId.Value = Me.tabBL.ClientID AndAlso forcusFlg = False Then
                txtCarrier.Focus()
                forcusFlg = True
            End If
            hasError = True
        End If

        If ChedckList(txtDemAcct.Text, lbDemAcct, "Demu For The Acct Of", errMessage) <> C_MESSAGENO.NORMAL Then
            rightBoxMessage.Append(errMessage)
            If Me.hdnSelectedTabId.Value = Me.tabBL.ClientID AndAlso forcusFlg = False Then
                txtDemAcct.Focus()
                forcusFlg = True
            End If
            hasError = True
        End If
        'Empty Or Full
        For i As Integer = 0 To costDt.Rows.Count - 1
            Dim dr As DataRow = costDt.Rows(i)
            If ChedckList(Convert.ToString(dr.Item("EMPTYORFULL")), lbEorF, "Empty Or Full", errMessage) <> C_MESSAGENO.NORMAL Then
                rightBoxMessage.Append(errMessage)
                If Me.hdnSelectedTabId.Value = Me.tabTank.ClientID AndAlso forcusFlg = False Then
                    DirectCast(Me.gvDetailInfo.Rows(i).FindControl("txtEmptyOrFull"), TextBox).Focus()
                    forcusFlg = True
                End If
                hasError = True
            End If
        Next
        '一時保存のチェックの場合はここで終了
        If isMinCheck = True Then
            If hasError Then
                'フッターに左ボックスを見るようメッセージを設定
                Dim messageNo As String = C_MESSAGENO.RIGHTBIXOUT
                CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
                '左ボックスにエラーメッセージ表示
                Me.txtRightErrorMessage.Text = rightBoxMessage.ToString
            End If

        End If
        Return Not hasError
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
        If BASEDLL.COA0019Session.LANGDISP <> "JA" Then
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
        Dim hasExShipRateError As Boolean = False
        'データテーブルの行ループ開始
        For Each dr As DataRow In dt.Rows
            'チェックフィールドのループ開始

            For Each checkField In targetCheckFields
                COA0026FieldCheck.FIELD = checkField
                COA0026FieldCheck.VALUE = Convert.ToString(dr.Item(checkField))
                COA0026FieldCheck.COA0026FieldCheck()
                If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then
                    retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                    If hasExShipRateError = True And checkField = "EXSHIPRATE" Then
                        Continue For
                    ElseIf checkField = "EXSHIPRATE" Then
                        hasExShipRateError = True
                    End If
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
    ''' LIST登録チェック
    ''' </summary>
    ''' <param name="inText"></param>
    ''' <param name="inList"></param>
    Protected Function ChedckList(ByVal inText As String, ByVal inList As ListBox, ByVal textNm As String, ByRef errMessage As String) As String
        Dim flag As Boolean = False
        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
        Dim retMessage As New StringBuilder

        If inText <> "" Then

            For i As Integer = 0 To inList.Items.Count - 1
                If inList.Items(i).Value = inText Then
                    flag = True
                    Exit For
                End If
            Next

            If (flag = False) Then
                retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                CommonFunctions.ShowMessage(C_MESSAGENO.INVALIDINPUT, dummyLabelObj)
                retMessage.AppendFormat("・{0}：{1}", textNm, dummyLabelObj.Text).AppendLine()
            End If
        End If

        errMessage = retMessage.ToString
        Return retMessageNo
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
    ''' データ登録処理
    ''' </summary>
    ''' <param name="ds"></param>
    Private Sub EntryData(ds As DataSet, ByRef errFlg As Boolean)
        Dim newEntry As Boolean = True
        Dim breakerId As String = ""
        Dim baseDt As DataTable = ds.Tables("ORDER_BASE")
        Dim valDt As DataTable = ds.Tables("ORDER_VALUE")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()

            '更新可能チェック(タイムスタンプ比較）
            If CanOrderUpdate(baseDt, sqlCon) = False Then
                Dim msgNo As String = C_MESSAGENO.CANNOTUPDATE
                errFlg = False
                CommonFunctions.ShowMessage(msgNo, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", msgNo)})
                Return
            End If
            '更新処理実行
            UpdateOrder(baseDt, valDt, sqlCon)

        End Using

        'File更新処理
        FileDBupdate(Me.hdnOrderNo.Value, Me.tabFileUp.ClientID, Me.hdnWhichTrans.Value)

    End Sub

    ''' <summary>
    ''' ブレーカー情報を取得し更新可能かチェック
    ''' </summary>
    ''' <param name="baseDt"></param>
    ''' <returns></returns>
    ''' <remarks>要タブ・権限に応じた制御</remarks>
    Private Function CanOrderUpdate(ByVal baseDt As DataTable, Optional sqlCon As SqlConnection = Nothing) As Boolean
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            Dim orderNo As String = Convert.ToString(baseDt.Rows(0).Item("ORDERNO"))
            'Dim tmstmp As String = Convert.ToString(baseDt.Rows(0).Item("TIMSTP"))
            'Dim tmstmp As String = Me.hdnTmstmp.Value
            Dim updYmd As String = Me.hdnUpdYmd.Value
            Dim updUser As String = Me.hdnUpdUser.Value
            Dim updTermId As String = Me.hdnUpdTermId.Value
            '更新直前のブレーカー紐づけテーブル取得
            Dim orderInfoData = GetOrderBase(orderNo, sqlCon)
            'タイムスタンプが一致していない場合は更新不可
            For Each dr As DataRow In orderInfoData.Rows
                If Not (updYmd = Convert.ToString(dr("UPDYMD")) AndAlso
                        updUser = Convert.ToString(dr("UPDUSER")) AndAlso
                        updTermId = Convert.ToString(dr("UPDTERMID"))) Then
                    Return False
                End If
            Next

            Return True

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
    ''' オーダー情報更新
    ''' </summary>
    ''' <param name="ownerDt"></param>
    ''' <param name="costDt"></param>
    ''' <param name="sqlCon"></param>
    ''' <remarks>TODO：タブや権限による制御、現状一律更新</remarks>
    Private Sub UpdateOrder(ownerDt As DataTable, costDt As DataTable, Optional sqlCon As SqlConnection = Nothing)
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Dim tran As SqlTransaction = Nothing
        Dim procDateTime As DateTime = DateTime.Now
        Dim dr As DataRow = ownerDt.Rows(0)
        Dim dt As String = "1900/01/01"
        Dim marksNumbers As String = ""
        Dim tankInfo As String = ""

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                canCloseConnect = True
            End If
            tran = sqlCon.BeginTransaction() 'トランザクション開始
            Dim trilateralVal As String = ""
            If Me.hdnWhichTrans.Value = "1" Then
                'sqlStat.AppendLine("   AND TRILATERAL = '1' ")
                trilateralVal = "1"
            ElseIf Me.hdnWhichTrans.Value = "2" Then
                'sqlStat.AppendLine("   AND TRILATERAL = '2' ")
                trilateralVal = "2"
            End If
            For Each vlDr As DataRow In (From costDr In costDt Order By costDr("DISPSEQ")) 'タンク単位のループ

                'オーダー明細2情報
                sqlStat.Clear()
                sqlStat.AppendLine("INSERT INTO GBT0007_ODR_VALUE2 ")
                sqlStat.AppendLine("(")
                sqlStat.AppendLine("    ORDERNO")
                sqlStat.AppendLine("   ,STYMD")
                sqlStat.AppendLine("   ,ENDYMD")
                sqlStat.AppendLine("   ,TANKSEQ")
                sqlStat.AppendLine("   ,TRILATERAL")
                sqlStat.AppendLine("   ,TANKTYPE")
                sqlStat.AppendLine("   ,GROSSWEIGHT")
                sqlStat.AppendLine("   ,NETWEIGHT")
                sqlStat.AppendLine("   ,SEALNO1")
                sqlStat.AppendLine("   ,SEALNO2")
                sqlStat.AppendLine("   ,SEALNO3")
                sqlStat.AppendLine("   ,SEALNO4")
                sqlStat.AppendLine("   ,EMPTYORFULL")
                sqlStat.AppendLine("   ,NOOFPACKAGE")
                sqlStat.AppendLine("   ,EXSHIPRATE")
                sqlStat.AppendLine("   ,INSHIPRATE")
                sqlStat.AppendLine("   ,APPLYID")
                sqlStat.AppendLine("   ,APPLYTEXT")
                sqlStat.AppendLine("   ,LASTSTEP")
                sqlStat.AppendLine("   ,DISPSEQ")
                sqlStat.AppendLine("   ,WORKC1")
                sqlStat.AppendLine("   ,WORKC2")
                sqlStat.AppendLine("   ,WORKC3")
                sqlStat.AppendLine("   ,WORKC4")
                sqlStat.AppendLine("   ,WORKC5")
                sqlStat.AppendLine("   ,WORKF1")
                sqlStat.AppendLine("   ,WORKF2")
                sqlStat.AppendLine("   ,WORKF3")
                sqlStat.AppendLine("   ,WORKF4")
                sqlStat.AppendLine("   ,WORKF5")
                sqlStat.AppendLine("   ,DELFLG")
                sqlStat.AppendLine("   ,INITYMD")
                sqlStat.AppendLine("   ,UPDYMD")
                sqlStat.AppendLine("   ,UPDUSER")
                sqlStat.AppendLine("   ,UPDTERMID")
                sqlStat.AppendLine("   ,RECEIVEYMD")
                sqlStat.AppendLine(") SELECT ")
                sqlStat.AppendLine("    ORDERNO")
                sqlStat.AppendLine("   ,STYMD")
                sqlStat.AppendLine("   ,ENDYMD")
                sqlStat.AppendLine("   ,TANKSEQ")
                sqlStat.AppendLine("   ,TRILATERAL")
                sqlStat.AppendLine("   ,TANKTYPE")
                sqlStat.AppendLine("   ,GROSSWEIGHT")
                sqlStat.AppendLine("   ,NETWEIGHT")
                sqlStat.AppendLine("   ,SEALNO1")
                sqlStat.AppendLine("   ,SEALNO2")
                sqlStat.AppendLine("   ,SEALNO3")
                sqlStat.AppendLine("   ,SEALNO4")
                sqlStat.AppendLine("   ,EMPTYORFULL")
                sqlStat.AppendLine("   ,NOOFPACKAGE")
                sqlStat.AppendLine("   ,EXSHIPRATE")
                sqlStat.AppendLine("   ,INSHIPRATE")
                sqlStat.AppendLine("   ,APPLYID")
                sqlStat.AppendLine("   ,APPLYTEXT")
                sqlStat.AppendLine("   ,LASTSTEP")
                sqlStat.AppendLine("   ,DISPSEQ")
                sqlStat.AppendLine("   ,WORKC1")
                sqlStat.AppendLine("   ,WORKC2")
                sqlStat.AppendLine("   ,WORKC3")
                sqlStat.AppendLine("   ,WORKC4")
                sqlStat.AppendLine("   ,WORKC5")
                sqlStat.AppendLine("   ,WORKF1")
                sqlStat.AppendLine("   ,WORKF2")
                sqlStat.AppendLine("   ,WORKF3")
                sqlStat.AppendLine("   ,WORKF4")
                sqlStat.AppendLine("   ,WORKF5")
                sqlStat.AppendLine("   ,@DELFLG")
                sqlStat.AppendLine("   ,@UPDYMD")
                sqlStat.AppendLine("   ,@UPDYMD")
                sqlStat.AppendLine("   ,@UPDUSER")
                sqlStat.AppendLine("   ,@UPDTERMID")
                sqlStat.AppendLine("   ,@RECEIVEYMD")
                sqlStat.AppendLine("FROM GBT0007_ODR_VALUE2")
                sqlStat.AppendLine("WHERE  ORDERNO      =  @ORDERNO")
                sqlStat.AppendLine("   AND DELFLG      <>  @DELFLG ")
                sqlStat.AppendLine("   AND TANKSEQ      =  @TANKSEQ ")
                sqlStat.AppendFormat("   AND TRILATERAL = '{0}' ", trilateralVal).AppendLine()
                sqlStat.AppendLine(";")
                sqlStat.AppendLine("UPDATE GBT0007_ODR_VALUE2")
                sqlStat.AppendLine("   SET TANKTYPE     =  @TANKTYPE ")
                sqlStat.AppendLine("      ,SEALNO1      =  @SEALNO1 ")
                sqlStat.AppendLine("      ,SEALNO2      =  @SEALNO2 ")
                sqlStat.AppendLine("      ,SEALNO3      =  @SEALNO3 ")
                sqlStat.AppendLine("      ,SEALNO4      =  @SEALNO4 ")
                sqlStat.AppendLine("      ,GROSSWEIGHT  =  @GROSSWEIGHT ")
                sqlStat.AppendLine("      ,NETWEIGHT    =  @NETWEIGHT ")
                sqlStat.AppendLine("      ,EMPTYORFULL  =  @EMPTYORFULL ")
                sqlStat.AppendLine("      ,NOOFPACKAGE  =  @NOOFPACKAGE ")
                sqlStat.AppendLine("      ,EXSHIPRATE   =  @EXSHIPRATE ")
                sqlStat.AppendLine("      ,INSHIPRATE   =  @INSHIPRATE ")
                sqlStat.AppendLine("      ,DISPSEQ      =  @DISPSEQ ")
                sqlStat.AppendLine("      ,WORKC1       =  @WORKC1 ")
                sqlStat.AppendLine("      ,WORKC2       =  @WORKC2 ")
                sqlStat.AppendLine("      ,WORKC3       =  @WORKC3 ")
                sqlStat.AppendLine("      ,WORKC4       =  @WORKC4 ")
                sqlStat.AppendLine("      ,WORKC5       =  @WORKC5 ")
                sqlStat.AppendLine("      ,WORKF1       =  @WORKF1 ")
                sqlStat.AppendLine("      ,WORKF2       =  @WORKF2 ")
                sqlStat.AppendLine("      ,WORKF3       =  @WORKF3 ")
                sqlStat.AppendLine("      ,WORKF4       =  @WORKF4 ")
                sqlStat.AppendLine("      ,WORKF5       =  @WORKF5 ")
                sqlStat.AppendLine("      ,UPDYMD       =  @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER      =  @UPDUSER ")
                sqlStat.AppendLine("      ,UPDTERMID    =  @UPDTERMID ")
                sqlStat.AppendLine("      ,RECEIVEYMD   =  @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE ORDERNO      =  @ORDERNO ")
                sqlStat.AppendLine("   AND DELFLG      <>  @DELFLG ")
                sqlStat.AppendLine("   AND TANKSEQ      =  @TANKSEQ ")
                sqlStat.AppendFormat("   AND TRILATERAL = '{0}' ", trilateralVal).AppendLine()
                sqlStat.AppendLine(";")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    With sqlCmd.Parameters
                        .Add("@ORDERNO", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("ORDERNO"))
                        .Add("@TANKSEQ", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("TANKSEQ"))
                        .Add("@TANKTYPE", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("TANKTYPE"))
                        .Add("@SEALNO1", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("SEALNO1"))
                        .Add("@SEALNO2", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("SEALNO2"))
                        .Add("@SEALNO3", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("SEALNO3"))
                        .Add("@SEALNO4", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("SEALNO4"))
                        .Add("@GROSSWEIGHT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(vlDr.Item("GROSSWEIGHT")))
                        .Add("@NETWEIGHT", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(vlDr.Item("NETWEIGHT")))
                        .Add("@EMPTYORFULL", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("EMPTYORFULL"))
                        .Add("@NOOFPACKAGE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(vlDr.Item("NOOFPACKAGE")))
                        .Add("@EXSHIPRATE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(vlDr.Item("EXSHIPRATE")))
                        .Add("@INSHIPRATE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(vlDr.Item("INSHIPRATE")))
                        .Add("@DISPSEQ", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("DISPSEQ"))
                        .Add("@WORKC1", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKC1"))
                        .Add("@WORKC2", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKC2"))
                        .Add("@WORKC3", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKC3"))
                        .Add("@WORKC4", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKC4"))
                        .Add("@WORKC5", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKC5"))
                        .Add("@WORKF1", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKF1"))
                        .Add("@WORKF2", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKF2"))
                        .Add("@WORKF3", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKF3"))
                        .Add("@WORKF4", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKF4"))
                        .Add("@WORKF5", SqlDbType.NVarChar).Value = Convert.ToString(vlDr.Item("WORKF5"))
                        .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With

                    sqlCmd.ExecuteNonQuery()
                End Using

                'タンク情報設定
                'If marksNumbers = "" Then
                '    marksNumbers = "<VAN SIDE MARK>" & vbCrLf
                'Else
                '    marksNumbers = marksNumbers & vbCrLf
                'End If
                'If Convert.ToString(dr.Item("PRODUCTNAME")) <> "" Then
                '    marksNumbers = marksNumbers & Convert.ToString(dr.Item("PRODUCTNAME")) & vbCrLf
                'End If
                'If Convert.ToString(vlDr.Item("NETWEIGHT")) <> "" Then
                '    marksNumbers = marksNumbers & "NET WT " & Convert.ToString(vlDr.Item("NETWEIGHT")) & " KGS" & vbCrLf
                'End If
                'If Convert.ToString(vlDr.Item("GROSSWEIGHT")) <> "" Then
                '    marksNumbers = marksNumbers & "GROSS WT " & Convert.ToString(vlDr.Item("GROSSWEIGHT")) & " KGS" & vbCrLf
                'End If

                If tankInfo = "" Then
                    tankInfo = Convert.ToString(vlDr.Item("TANKNO"))
                Else
                    tankInfo = tankInfo & vbCrLf & Convert.ToString(vlDr.Item("TANKNO"))
                End If
                If Convert.ToString(vlDr.Item("TANKTYPE")) <> "" Then
                    tankInfo = tankInfo & "/" & Convert.ToString(vlDr.Item("TANKTYPE"))
                End If
                If Convert.ToString(vlDr.Item("SEALNO1")) <> "" Then
                    tankInfo = tankInfo & "/" & Convert.ToString(vlDr.Item("SEALNO1"))
                End If
                If Convert.ToString(vlDr.Item("SEALNO2")) <> "" Then
                    tankInfo = tankInfo & "/" & Convert.ToString(vlDr.Item("SEALNO2"))
                End If
                If Convert.ToString(vlDr.Item("SEALNO3")) <> "" Then
                    tankInfo = tankInfo & "/" & Convert.ToString(vlDr.Item("SEALNO3"))
                End If
                If Convert.ToString(vlDr.Item("SEALNO4")) <> "" Then
                    tankInfo = tankInfo & "/" & Convert.ToString(vlDr.Item("SEALNO4"))
                End If

            Next

            'オーダー基本情報
            sqlStat.Clear()
#Region "更新前の最新レコードを削除フラグを立て保持する部分"
            sqlStat.AppendLine("INSERT INTO GBT0004_ODR_BASE")
            sqlStat.AppendLine("(")
            sqlStat.AppendLine("    ORDERNO")
            sqlStat.AppendLine("   ,STYMD")
            sqlStat.AppendLine("   ,ENDYMD")
            sqlStat.AppendLine("   ,BRID")
            sqlStat.AppendLine("   ,BRTYPE")
            sqlStat.AppendLine("   ,VALIDITYFROM")
            sqlStat.AppendLine("   ,VALIDITYTO")
            sqlStat.AppendLine("   ,TERMTYPE")
            sqlStat.AppendLine("   ,NOOFTANKS")
            sqlStat.AppendLine("   ,SHIPPER")
            sqlStat.AppendLine("   ,CONSIGNEE")
            sqlStat.AppendLine("   ,CARRIER1")
            sqlStat.AppendLine("   ,CARRIER2")
            sqlStat.AppendLine("   ,PRODUCTCODE")
            sqlStat.AppendLine("   ,PRODUCTWEIGHT")
            sqlStat.AppendLine("   ,RECIEPTCOUNTRY1")
            sqlStat.AppendLine("   ,RECIEPTPORT1")
            sqlStat.AppendLine("   ,RECIEPTCOUNTRY2")
            sqlStat.AppendLine("   ,RECIEPTPORT2")
            sqlStat.AppendLine("   ,LOADCOUNTRY1")
            sqlStat.AppendLine("   ,LOADPORT1")
            sqlStat.AppendLine("   ,LOADCOUNTRY2")
            sqlStat.AppendLine("   ,LOADPORT2")
            sqlStat.AppendLine("   ,DISCHARGECOUNTRY1")
            sqlStat.AppendLine("   ,DISCHARGEPORT1")
            sqlStat.AppendLine("   ,DISCHARGECOUNTRY2")
            sqlStat.AppendLine("   ,DISCHARGEPORT2")
            sqlStat.AppendLine("   ,DELIVERYCOUNTRY1")
            sqlStat.AppendLine("   ,DELIVERYPORT1")
            sqlStat.AppendLine("   ,DELIVERYCOUNTRY2")
            sqlStat.AppendLine("   ,DELIVERYPORT2")
            sqlStat.AppendLine("   ,VSL1")
            sqlStat.AppendLine("   ,VOY1")
            sqlStat.AppendLine("   ,ETD1")
            sqlStat.AppendLine("   ,ETA1")
            sqlStat.AppendLine("   ,VSL2")
            sqlStat.AppendLine("   ,VOY2")
            sqlStat.AppendLine("   ,ETD2")
            sqlStat.AppendLine("   ,ETA2")
            sqlStat.AppendLine("   ,INVOICEDBY")
            sqlStat.AppendLine("   ,LOADING")
            sqlStat.AppendLine("   ,STEAMING")
            sqlStat.AppendLine("   ,TIP")
            sqlStat.AppendLine("   ,EXTRA")
            sqlStat.AppendLine("   ,DEMURTO")
            sqlStat.AppendLine("   ,DEMURUSRATE1")
            sqlStat.AppendLine("   ,DEMURUSRATE2")
            sqlStat.AppendLine("   ,SALESPIC")
            sqlStat.AppendLine("   ,AGENTORGANIZER")
            sqlStat.AppendLine("   ,AGENTPOL1")
            sqlStat.AppendLine("   ,AGENTPOL2")
            sqlStat.AppendLine("   ,AGENTPOD1")
            sqlStat.AppendLine("   ,AGENTPOD2")
            sqlStat.AppendLine("   ,USINGLEASETANK")
            sqlStat.AppendLine("   ,BLID1")
            sqlStat.AppendLine("   ,BLAPPDATE1")
            sqlStat.AppendLine("   ,BLID2")
            sqlStat.AppendLine("   ,BLAPPDATE2")
            sqlStat.AppendLine("   ,SHIPPERNAME")
            sqlStat.AppendLine("   ,SHIPPERTEXT")
            sqlStat.AppendLine("   ,SHIPPERTEXT2")
            sqlStat.AppendLine("   ,CONSIGNEENAME")
            sqlStat.AppendLine("   ,CONSIGNEETEXT")
            sqlStat.AppendLine("   ,CONSIGNEETEXT2")
            sqlStat.AppendLine("   ,IECCODE")
            sqlStat.AppendLine("   ,NOTIFYNAME")
            sqlStat.AppendLine("   ,NOTIFYTEXT")
            sqlStat.AppendLine("   ,NOTIFYTEXT2")
            sqlStat.AppendLine("   ,NOTIFYCONT")
            sqlStat.AppendLine("   ,NOTIFYCONTNAME")
            sqlStat.AppendLine("   ,NOTIFYCONTTEXT1")
            sqlStat.AppendLine("   ,NOTIFYCONTTEXT2")
            sqlStat.AppendLine("   ,PRECARRIAGETEXT")
            sqlStat.AppendLine("   ,PRECARRIAGETEXT2")
            sqlStat.AppendLine("   ,VSL")
            sqlStat.AppendLine("   ,VOY")
            sqlStat.AppendLine("   ,FINDESTINATIONNAME")
            sqlStat.AppendLine("   ,FINDESTINATIONTEXT")
            sqlStat.AppendLine("   ,FINDESTINATIONTEXT2")
            sqlStat.AppendLine("   ,PRODUCT")
            sqlStat.AppendLine("   ,PRODUCTPORDER")
            sqlStat.AppendLine("   ,PRODUCTTIP")
            sqlStat.AppendLine("   ,PRODUCTFREIGHT")
            sqlStat.AppendLine("   ,FREIGHTANDCHARGES")
            sqlStat.AppendLine("   ,PREPAIDAT")
            sqlStat.AppendLine("   ,GOODSPKGS")
            sqlStat.AppendLine("   ,CONTAINERPKGS")
            sqlStat.AppendLine("   ,BLNUM")
            sqlStat.AppendLine("   ,CONTAINERNO")
            sqlStat.AppendLine("   ,SEALNO")
            sqlStat.AppendLine("   ,NOOFCONTAINER")
            sqlStat.AppendLine("   ,DECLAREDVALUE")
            sqlStat.AppendLine("   ,DECLAREDVALUE2")
            sqlStat.AppendLine("   ,REVENUETONS")
            sqlStat.AppendLine("   ,REVENUETONS2")
            sqlStat.AppendLine("   ,RATE")
            sqlStat.AppendLine("   ,RATE2")
            sqlStat.AppendLine("   ,PER")
            sqlStat.AppendLine("   ,PER2")
            sqlStat.AppendLine("   ,PREPAID")
            sqlStat.AppendLine("   ,PREPAID2")
            sqlStat.AppendLine("   ,COLLECT")
            sqlStat.AppendLine("   ,COLLECT2")
            sqlStat.AppendLine("   ,EXCHANGERATE")
            sqlStat.AppendLine("   ,PAYABLEAT")
            sqlStat.AppendLine("   ,LOCALCURRENCY")
            sqlStat.AppendLine("   ,CARRIERBLNO")
            sqlStat.AppendLine("   ,CARRIERBLNO2")
            sqlStat.AppendLine("   ,BOOKINGNO")
            sqlStat.AppendLine("   ,BOOKINGNO2")
            sqlStat.AppendLine("   ,NOOFPACKAGE")
            sqlStat.AppendLine("   ,BLTYPE")
            sqlStat.AppendLine("   ,BLTYPE2")
            sqlStat.AppendLine("   ,NOOFBL")
            sqlStat.AppendLine("   ,NOOFBL2")
            sqlStat.AppendLine("   ,PAYMENTPLACE")
            sqlStat.AppendLine("   ,PAYMENTPLACE2")
            sqlStat.AppendLine("   ,BLISSUEPLACE")
            sqlStat.AppendLine("   ,BLISSUEPLACE2")
            sqlStat.AppendLine("   ,ANISSUEPLACE")
            sqlStat.AppendLine("   ,ANISSUEPLACE2")
            sqlStat.AppendLine("   ,MEASUREMENT")
            sqlStat.AppendLine("   ,MEASUREMENT2")
            sqlStat.AppendLine("   ,MARKSANDNUMBERS")
            sqlStat.AppendLine("   ,TANKINFO")
            sqlStat.AppendLine("   ,LDNVSL1")
            sqlStat.AppendLine("   ,LDNPOL1")
            sqlStat.AppendLine("   ,LDNDATE1")
            sqlStat.AppendLine("   ,LDNBY1")
            sqlStat.AppendLine("   ,LDNVSL2")
            sqlStat.AppendLine("   ,LDNPOL2")
            sqlStat.AppendLine("   ,LDNDATE2")
            sqlStat.AppendLine("   ,LDNBY2")
            sqlStat.AppendLine("   ,CARRIERBLTYPE")
            sqlStat.AppendLine("   ,CARRIERBLTYPE2")
            sqlStat.AppendLine("   ,DEMUFORACCT")
            sqlStat.AppendLine("   ,DEMUFORACCT2")
            sqlStat.AppendLine("   ,BLRECEIPT1")
            sqlStat.AppendLine("   ,BLRECEIPT2")
            sqlStat.AppendLine("   ,BLLOADING1")
            sqlStat.AppendLine("   ,BLLOADING2")
            sqlStat.AppendLine("   ,BLDISCHARGE1")
            sqlStat.AppendLine("   ,BLDISCHARGE2")
            sqlStat.AppendLine("   ,BLDELIVERY1")
            sqlStat.AppendLine("   ,BLDELIVERY2")
            sqlStat.AppendLine("   ,BLPLACEDATEISSUE1")
            sqlStat.AppendLine("   ,BLPLACEDATEISSUE2")
            sqlStat.AppendLine("   ,TRANSIT1VSL1")
            sqlStat.AppendLine("   ,TRANSIT1VOY1")
            'sqlStat.AppendLine("   ,TRANSIT1ETD1")
            'sqlStat.AppendLine("   ,TRANSIT1ETA1")
            sqlStat.AppendLine("   ,TRANSIT2VSL1")
            sqlStat.AppendLine("   ,TRANSIT2VOY1")
            'sqlStat.AppendLine("   ,TRANSIT2ETD1")
            'sqlStat.AppendLine("   ,TRANSIT2ETA1")
            sqlStat.AppendLine("   ,TRANSIT1VSL2")
            sqlStat.AppendLine("   ,TRANSIT1VOY2")
            'sqlStat.AppendLine("   ,TRANSIT1ETD2")
            'sqlStat.AppendLine("   ,TRANSIT1ETA2")
            sqlStat.AppendLine("   ,TRANSIT2VSL2")
            sqlStat.AppendLine("   ,TRANSIT2VOY2")
            'sqlStat.AppendLine("   ,TRANSIT2ETD2")
            'sqlStat.AppendLine("   ,TRANSIT2ETA2")
            sqlStat.AppendLine("   ,REMARK")
            sqlStat.AppendLine("   ,DELFLG")
            sqlStat.AppendLine("   ,INITYMD")
            sqlStat.AppendLine("   ,INITUSER")
            sqlStat.AppendLine("   ,UPDYMD")
            sqlStat.AppendLine("   ,UPDUSER")
            sqlStat.AppendLine("   ,UPDTERMID")
            sqlStat.AppendLine("   ,RECEIVEYMD")
            sqlStat.AppendLine(")")
            sqlStat.AppendLine("SELECT ")
            sqlStat.AppendLine("    ORDERNO")
            sqlStat.AppendLine("   ,STYMD")
            sqlStat.AppendLine("   ,ENDYMD")
            sqlStat.AppendLine("   ,BRID")
            sqlStat.AppendLine("   ,BRTYPE")
            sqlStat.AppendLine("   ,VALIDITYFROM")
            sqlStat.AppendLine("   ,VALIDITYTO")
            sqlStat.AppendLine("   ,TERMTYPE")
            sqlStat.AppendLine("   ,NOOFTANKS")
            sqlStat.AppendLine("   ,SHIPPER")
            sqlStat.AppendLine("   ,CONSIGNEE")
            sqlStat.AppendLine("   ,CARRIER1")
            sqlStat.AppendLine("   ,CARRIER2")
            sqlStat.AppendLine("   ,PRODUCTCODE")
            sqlStat.AppendLine("   ,PRODUCTWEIGHT")
            sqlStat.AppendLine("   ,RECIEPTCOUNTRY1")
            sqlStat.AppendLine("   ,RECIEPTPORT1")
            sqlStat.AppendLine("   ,RECIEPTCOUNTRY2")
            sqlStat.AppendLine("   ,RECIEPTPORT2")
            sqlStat.AppendLine("   ,LOADCOUNTRY1")
            sqlStat.AppendLine("   ,LOADPORT1")
            sqlStat.AppendLine("   ,LOADCOUNTRY2")
            sqlStat.AppendLine("   ,LOADPORT2")
            sqlStat.AppendLine("   ,DISCHARGECOUNTRY1")
            sqlStat.AppendLine("   ,DISCHARGEPORT1")
            sqlStat.AppendLine("   ,DISCHARGECOUNTRY2")
            sqlStat.AppendLine("   ,DISCHARGEPORT2")
            sqlStat.AppendLine("   ,DELIVERYCOUNTRY1")
            sqlStat.AppendLine("   ,DELIVERYPORT1")
            sqlStat.AppendLine("   ,DELIVERYCOUNTRY2")
            sqlStat.AppendLine("   ,DELIVERYPORT2")
            sqlStat.AppendLine("   ,VSL1")
            sqlStat.AppendLine("   ,VOY1")
            sqlStat.AppendLine("   ,ETD1")
            sqlStat.AppendLine("   ,ETA1")
            sqlStat.AppendLine("   ,VSL2")
            sqlStat.AppendLine("   ,VOY2")
            sqlStat.AppendLine("   ,ETD2")
            sqlStat.AppendLine("   ,ETA2")
            sqlStat.AppendLine("   ,INVOICEDBY")
            sqlStat.AppendLine("   ,LOADING")
            sqlStat.AppendLine("   ,STEAMING")
            sqlStat.AppendLine("   ,TIP")
            sqlStat.AppendLine("   ,EXTRA")
            sqlStat.AppendLine("   ,DEMURTO")
            sqlStat.AppendLine("   ,DEMURUSRATE1")
            sqlStat.AppendLine("   ,DEMURUSRATE2")
            sqlStat.AppendLine("   ,SALESPIC")
            sqlStat.AppendLine("   ,AGENTORGANIZER")
            sqlStat.AppendLine("   ,AGENTPOL1")
            sqlStat.AppendLine("   ,AGENTPOL2")
            sqlStat.AppendLine("   ,AGENTPOD1")
            sqlStat.AppendLine("   ,AGENTPOD2")
            sqlStat.AppendLine("   ,USINGLEASETANK")
            sqlStat.AppendLine("   ,BLID1")
            sqlStat.AppendLine("   ,BLAPPDATE1")
            sqlStat.AppendLine("   ,BLID2")
            sqlStat.AppendLine("   ,BLAPPDATE2")
            sqlStat.AppendLine("   ,SHIPPERNAME")
            sqlStat.AppendLine("   ,SHIPPERTEXT")
            sqlStat.AppendLine("   ,SHIPPERTEXT2")
            sqlStat.AppendLine("   ,CONSIGNEENAME")
            sqlStat.AppendLine("   ,CONSIGNEETEXT")
            sqlStat.AppendLine("   ,CONSIGNEETEXT2")
            sqlStat.AppendLine("   ,IECCODE")
            sqlStat.AppendLine("   ,NOTIFYNAME")
            sqlStat.AppendLine("   ,NOTIFYTEXT")
            sqlStat.AppendLine("   ,NOTIFYTEXT2")
            sqlStat.AppendLine("   ,NOTIFYCONT")
            sqlStat.AppendLine("   ,NOTIFYCONTNAME")
            sqlStat.AppendLine("   ,NOTIFYCONTTEXT1")
            sqlStat.AppendLine("   ,NOTIFYCONTTEXT2")
            sqlStat.AppendLine("   ,PRECARRIAGETEXT")
            sqlStat.AppendLine("   ,PRECARRIAGETEXT2")
            sqlStat.AppendLine("   ,VSL")
            sqlStat.AppendLine("   ,VOY")
            sqlStat.AppendLine("   ,FINDESTINATIONNAME")
            sqlStat.AppendLine("   ,FINDESTINATIONTEXT")
            sqlStat.AppendLine("   ,FINDESTINATIONTEXT2")
            sqlStat.AppendLine("   ,PRODUCT")
            sqlStat.AppendLine("   ,PRODUCTPORDER")
            sqlStat.AppendLine("   ,PRODUCTTIP")
            sqlStat.AppendLine("   ,PRODUCTFREIGHT")
            sqlStat.AppendLine("   ,FREIGHTANDCHARGES")
            sqlStat.AppendLine("   ,PREPAIDAT")
            sqlStat.AppendLine("   ,GOODSPKGS")
            sqlStat.AppendLine("   ,CONTAINERPKGS")
            sqlStat.AppendLine("   ,BLNUM")
            sqlStat.AppendLine("   ,CONTAINERNO")
            sqlStat.AppendLine("   ,SEALNO")
            sqlStat.AppendLine("   ,NOOFCONTAINER")
            sqlStat.AppendLine("   ,DECLAREDVALUE")
            sqlStat.AppendLine("   ,DECLAREDVALUE2")
            sqlStat.AppendLine("   ,REVENUETONS")
            sqlStat.AppendLine("   ,REVENUETONS2")
            sqlStat.AppendLine("   ,RATE")
            sqlStat.AppendLine("   ,RATE2")
            sqlStat.AppendLine("   ,PER")
            sqlStat.AppendLine("   ,PER2")
            sqlStat.AppendLine("   ,PREPAID")
            sqlStat.AppendLine("   ,PREPAID2")
            sqlStat.AppendLine("   ,COLLECT")
            sqlStat.AppendLine("   ,COLLECT2")
            sqlStat.AppendLine("   ,EXCHANGERATE")
            sqlStat.AppendLine("   ,PAYABLEAT")
            sqlStat.AppendLine("   ,LOCALCURRENCY")
            sqlStat.AppendLine("   ,CARRIERBLNO")
            sqlStat.AppendLine("   ,CARRIERBLNO2")
            sqlStat.AppendLine("   ,BOOKINGNO")
            sqlStat.AppendLine("   ,BOOKINGNO2")
            sqlStat.AppendLine("   ,NOOFPACKAGE")
            sqlStat.AppendLine("   ,BLTYPE")
            sqlStat.AppendLine("   ,BLTYPE2")
            sqlStat.AppendLine("   ,NOOFBL")
            sqlStat.AppendLine("   ,NOOFBL2")
            sqlStat.AppendLine("   ,PAYMENTPLACE")
            sqlStat.AppendLine("   ,PAYMENTPLACE2")
            sqlStat.AppendLine("   ,BLISSUEPLACE")
            sqlStat.AppendLine("   ,BLISSUEPLACE2")
            sqlStat.AppendLine("   ,ANISSUEPLACE")
            sqlStat.AppendLine("   ,ANISSUEPLACE2")
            sqlStat.AppendLine("   ,MEASUREMENT")
            sqlStat.AppendLine("   ,MEASUREMENT2")
            sqlStat.AppendLine("   ,MARKSANDNUMBERS")
            sqlStat.AppendLine("   ,TANKINFO")
            sqlStat.AppendLine("   ,LDNVSL1")
            sqlStat.AppendLine("   ,LDNPOL1")
            sqlStat.AppendLine("   ,LDNDATE1")
            sqlStat.AppendLine("   ,LDNBY1")
            sqlStat.AppendLine("   ,LDNVSL2")
            sqlStat.AppendLine("   ,LDNPOL2")
            sqlStat.AppendLine("   ,LDNDATE2")
            sqlStat.AppendLine("   ,LDNBY2")
            sqlStat.AppendLine("   ,CARRIERBLTYPE")
            sqlStat.AppendLine("   ,CARRIERBLTYPE2")
            sqlStat.AppendLine("   ,DEMUFORACCT")
            sqlStat.AppendLine("   ,DEMUFORACCT2")
            sqlStat.AppendLine("   ,BLRECEIPT1")
            sqlStat.AppendLine("   ,BLRECEIPT2")
            sqlStat.AppendLine("   ,BLLOADING1")
            sqlStat.AppendLine("   ,BLLOADING2")
            sqlStat.AppendLine("   ,BLDISCHARGE1")
            sqlStat.AppendLine("   ,BLDISCHARGE2")
            sqlStat.AppendLine("   ,BLDELIVERY1")
            sqlStat.AppendLine("   ,BLDELIVERY2")
            sqlStat.AppendLine("   ,BLPLACEDATEISSUE1")
            sqlStat.AppendLine("   ,BLPLACEDATEISSUE2")
            sqlStat.AppendLine("   ,TRANSIT1VSL1")
            sqlStat.AppendLine("   ,TRANSIT1VOY1")
            'sqlStat.AppendLine("   ,TRANSIT1ETD1")
            'sqlStat.AppendLine("   ,TRANSIT1ETA1")
            sqlStat.AppendLine("   ,TRANSIT2VSL1")
            sqlStat.AppendLine("   ,TRANSIT2VOY1")
            'sqlStat.AppendLine("   ,TRANSIT2ETD1")
            'sqlStat.AppendLine("   ,TRANSIT2ETA1")
            sqlStat.AppendLine("   ,TRANSIT1VSL2")
            sqlStat.AppendLine("   ,TRANSIT1VOY2")
            'sqlStat.AppendLine("   ,TRANSIT1ETD2")
            'sqlStat.AppendLine("   ,TRANSIT1ETA2")
            sqlStat.AppendLine("   ,TRANSIT2VSL2")
            sqlStat.AppendLine("   ,TRANSIT2VOY2")
            'sqlStat.AppendLine("   ,TRANSIT2ETD2")
            'sqlStat.AppendLine("   ,TRANSIT2ETA2")
            sqlStat.AppendLine("   ,REMARK")
            sqlStat.AppendLine("   ,@DELFLG")
            sqlStat.AppendLine("   ,@UPDYMD")
            sqlStat.AppendLine("   ,@UPDUSER")
            sqlStat.AppendLine("   ,@UPDYMD")
            sqlStat.AppendLine("   ,@UPDUSER")
            sqlStat.AppendLine("   ,@UPDTERMID")
            sqlStat.AppendLine("   ,@RECEIVEYMD")
            sqlStat.AppendLine("  FROM GBT0004_ODR_BASE")
            sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO ")
            sqlStat.AppendLine("   AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine(";")
#End Region
            sqlStat.AppendLine("UPDATE GBT0004_ODR_BASE ")
            sqlStat.AppendLine("   SET ")
            If Me.hdnWhichTrans.Value = "1" Then
                sqlStat.AppendLine("       BLID1   = @BLID1 ")
                sqlStat.AppendLine("       ,CARRIER1   = @CARRIER1 ")
                'sqlStat.AppendLine("      ,BLAPPDATE1 = @BLAPPDATE1 ")
                'sqlStat.AppendLine("      ,RECIEPTCOUNTRY1 = @RECIEPTCOUNTRY1 ")
                'sqlStat.AppendLine("      ,RECIEPTPORT1 = @RECIEPTPORT1 ")
                'sqlStat.AppendLine("      ,LOADCOUNTRY1 = @LOADCOUNTRY1 ")
                'sqlStat.AppendLine("      ,LOADPORT1 = @LOADPORT1 ")
                'sqlStat.AppendLine("      ,DISCHARGECOUNTRY1 = @DISCHARGECOUNTRY1 ")
                'sqlStat.AppendLine("      ,DISCHARGEPORT1 = @DISCHARGEPORT1 ")
                'sqlStat.AppendLine("      ,DELIVERYCOUNTRY1 = @DELIVERYCOUNTRY1 ")
                'sqlStat.AppendLine("      ,DELIVERYPORT1 = @DELIVERYPORT1 ")
                sqlStat.AppendLine("      ,VSL1 = @VSL1 ")
                sqlStat.AppendLine("      ,VOY1 = @VOY1 ")
                sqlStat.AppendLine("      ,NOTIFYCONTTEXT1 = @NOTIFYCONTTEXT ")

                sqlStat.AppendLine("      ,LDNVSL1 = @LDNVSL1 ")
                sqlStat.AppendLine("      ,LDNPOL1 = @LDNPOL1 ")
                sqlStat.AppendLine("      ,LDNDATE1 = @LDNDATE1 ")
                sqlStat.AppendLine("      ,LDNBY1 = @LDNBY1 ")
                sqlStat.AppendLine("      ,BOOKINGNO = @BOOKINGNO ")

                sqlStat.AppendLine("      ,SHIPPERTEXT = @SHIPPERTEXT ")
                sqlStat.AppendLine("      ,CONSIGNEETEXT = @CONSIGNEETEXT ")
                sqlStat.AppendLine("      ,NOTIFYTEXT = @NOTIFYTEXT ")
                sqlStat.AppendLine("      ,PRECARRIAGETEXT = @PRECARRIAGETEXT ")
                sqlStat.AppendLine("      ,FINDESTINATIONTEXT = @FINDESTINATIONTEXT ")
                sqlStat.AppendLine("      ,DECLAREDVALUE = @DECLAREDVALUE ")
                sqlStat.AppendLine("      ,REVENUETONS = @REVENUETONS ")
                sqlStat.AppendLine("      ,RATE = @RATE ")
                sqlStat.AppendLine("      ,PER = @PER ")
                sqlStat.AppendLine("      ,PREPAID = @PREPAID ")
                sqlStat.AppendLine("      ,COLLECT = @COLLECT ")
                sqlStat.AppendLine("      ,CARRIERBLNO = @CARRIERBLNO ")
                sqlStat.AppendLine("      ,NOOFBL = @NOOFBL ")
                sqlStat.AppendLine("      ,BLTYPE = @BLTYPE ")
                sqlStat.AppendLine("      ,PAYMENTPLACE = @PAYMENTPLACE ")
                sqlStat.AppendLine("      ,BLISSUEPLACE = @BLISSUEPLACE ")
                sqlStat.AppendLine("      ,ANISSUEPLACE = @ANISSUEPLACE ")
                sqlStat.AppendLine("      ,MEASUREMENT = @MEASUREMENT ")
                sqlStat.AppendLine("      ,CARRIERBLTYPE = @CARRIERBLTYPE ")
                sqlStat.AppendLine("      ,DEMUFORACCT = @DEMUFORACCT ")
                sqlStat.AppendLine("      ,GOODSPKGS  = @GOODSPKGS ")

                sqlStat.AppendLine("      ,BLRECEIPT1 = @BLRECEIPT1 ")
                sqlStat.AppendLine("      ,BLLOADING1 = @BLLOADING1 ")
                sqlStat.AppendLine("      ,BLDISCHARGE1 = @BLDISCHARGE1 ")
                sqlStat.AppendLine("      ,BLDELIVERY1 = @BLDELIVERY1 ")
                sqlStat.AppendLine("      ,BLPLACEDATEISSUE1 = @BLPLACEDATEISSUE1 ")

                sqlStat.AppendLine("      ,TRANSIT1VSL1 = @TRANSIT1VSL1 ")
                sqlStat.AppendLine("      ,TRANSIT1VOY1 = @TRANSIT1VOY1 ")
                sqlStat.AppendLine("      ,TRANSIT2VSL1 = @TRANSIT2VSL1 ")
                sqlStat.AppendLine("      ,TRANSIT2VOY1 = @TRANSIT2VOY1 ")

            ElseIf Me.hdnWhichTrans.Value = "2" Then
                sqlStat.AppendLine("       BLID2   = @BLID2 ")
                sqlStat.AppendLine("       ,CARRIER2   = @CARRIER2 ")
                'sqlStat.AppendLine("      ,BLAPPDATE2 = @BLAPPDATE2 ")
                'sqlStat.AppendLine("      ,RECIEPTCOUNTRY2 = @RECIEPTCOUNTRY2 ")
                'sqlStat.AppendLine("      ,RECIEPTPORT2 = @RECIEPTPORT2 ")
                'sqlStat.AppendLine("      ,LOADCOUNTRY2 = @LOADCOUNTRY2 ")
                'sqlStat.AppendLine("      ,LOADPORT2 = @LOADPORT2 ")
                'sqlStat.AppendLine("      ,DISCHARGECOUNTRY2 = @DISCHARGECOUNTRY2 ")
                'sqlStat.AppendLine("      ,DISCHARGEPORT2 = @DISCHARGEPORT2 ")
                'sqlStat.AppendLine("      ,DELIVERYCOUNTRY2 = @DELIVERYCOUNTRY2 ")
                'sqlStat.AppendLine("      ,DELIVERYPORT2 = @DELIVERYPORT2 ")
                sqlStat.AppendLine("      ,VSL2 = @VSL2 ")
                sqlStat.AppendLine("      ,VOY2 = @VOY2 ")
                sqlStat.AppendLine("      ,NOTIFYCONTTEXT2 = @NOTIFYCONTTEXT ")

                sqlStat.AppendLine("      ,LDNVSL2 = @LDNVSL2 ")
                sqlStat.AppendLine("      ,LDNPOL2 = @LDNPOL2 ")
                sqlStat.AppendLine("      ,LDNDATE2 = @LDNDATE2 ")
                sqlStat.AppendLine("      ,LDNBY2 = @LDNBY2 ")
                sqlStat.AppendLine("      ,BOOKINGNO2 = @BOOKINGNO2 ")

                sqlStat.AppendLine("      ,SHIPPERTEXT2 = @SHIPPERTEXT2 ")
                sqlStat.AppendLine("      ,CONSIGNEETEXT2 = @CONSIGNEETEXT2 ")
                sqlStat.AppendLine("      ,NOTIFYTEXT2 = @NOTIFYTEXT2 ")
                sqlStat.AppendLine("      ,PRECARRIAGETEXT2 = @PRECARRIAGETEXT2 ")
                sqlStat.AppendLine("      ,FINDESTINATIONTEXT2 = @FINDESTINATIONTEXT2 ")
                sqlStat.AppendLine("      ,DECLAREDVALUE2 = @DECLAREDVALUE2 ")
                sqlStat.AppendLine("      ,REVENUETONS2 = @REVENUETONS2 ")
                sqlStat.AppendLine("      ,RATE2 = @RATE2 ")
                sqlStat.AppendLine("      ,PER2 = @PER2 ")
                sqlStat.AppendLine("      ,PREPAID2 = @PREPAID2 ")
                sqlStat.AppendLine("      ,COLLECT2 = @COLLECT2 ")
                sqlStat.AppendLine("      ,CARRIERBLNO2 = @CARRIERBLNO2 ")
                sqlStat.AppendLine("      ,NOOFBL2 = @NOOFBL2 ")
                sqlStat.AppendLine("      ,BLTYPE2 = @BLTYPE2 ")
                sqlStat.AppendLine("      ,PAYMENTPLACE2 = @PAYMENTPLACE2 ")
                sqlStat.AppendLine("      ,BLISSUEPLACE2 = @BLISSUEPLACE2 ")
                sqlStat.AppendLine("      ,ANISSUEPLACE2 = @ANISSUEPLACE2 ")
                sqlStat.AppendLine("      ,MEASUREMENT2 = @MEASUREMENT2 ")
                sqlStat.AppendLine("      ,CARRIERBLTYPE2 = @CARRIERBLTYPE2 ")
                sqlStat.AppendLine("      ,DEMUFORACCT2 = @DEMUFORACCT2 ")

                sqlStat.AppendLine("      ,BLRECEIPT2 = @BLRECEIPT2 ")
                sqlStat.AppendLine("      ,BLLOADING2 = @BLLOADING2 ")
                sqlStat.AppendLine("      ,BLDISCHARGE2 = @BLDISCHARGE2 ")
                sqlStat.AppendLine("      ,BLDELIVERY2 = @BLDELIVERY2 ")
                sqlStat.AppendLine("      ,BLPLACEDATEISSUE2 = @BLPLACEDATEISSUE2 ")

                sqlStat.AppendLine("      ,TRANSIT1VSL2 = @TRANSIT1VSL2 ")
                sqlStat.AppendLine("      ,TRANSIT1VOY2 = @TRANSIT1VOY2 ")
                sqlStat.AppendLine("      ,TRANSIT2VSL2 = @TRANSIT2VSL2 ")
                sqlStat.AppendLine("      ,TRANSIT2VOY2 = @TRANSIT2VOY2 ")

            End If
            'sqlStat.AppendLine("      ,CONTAINERNO = @CONTAINERNO ")
            sqlStat.AppendLine("      ,FREIGHTANDCHARGES = @FREIGHTANDCHARGES ")
            sqlStat.AppendLine("      ,CONTAINERPKGS = @CONTAINERPKGS ")
            'sqlStat.AppendLine("      ,NOOFCONTAINER = @NOOFCONTAINER ")
            'sqlStat.AppendLine("      ,BOOKINGNO = @BOOKINGNO ")
            'sqlStat.AppendLine("      ,TERMTYPE = @TERMTYPE ")
            sqlStat.AppendLine("      ,NOOFPACKAGE = @NOOFPACKAGE ")
            'sqlStat.AppendLine("      ,EXCHANGERATE = @EXCHANGERATE ")
            'sqlStat.AppendLine("      ,PREPAIDAT = @PREPAIDAT ")
            'sqlStat.AppendLine("      ,PAYABLEAT = @PAYABLEAT ")
            'sqlStat.AppendLine("      ,LOCALCURRENCY = @LOCALCURRENCY ")
            sqlStat.AppendLine("      ,MARKSANDNUMBERS = @MARKSANDNUMBERS ")
            sqlStat.AppendLine("      ,TANKINFO = @TANKINFO ")
            sqlStat.AppendLine("      ,UPDYMD  = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER = @UPDUSER ")
            sqlStat.AppendLine("      ,UPDTERMID = @UPDTERMID ")
            sqlStat.AppendLine("      ,RECEIVEYMD =  @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO ")
            'sqlStat.AppendLine("   AND STYMD   = @STYMD ")
            sqlStat.AppendLine("   AND DELFLG <> @DELFLG ")
            sqlStat.AppendLine(";")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                With sqlCmd.Parameters
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ORDERNO"))
                    '.Add("@STYMD", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("STYMD")))
                    If Me.hdnWhichTrans.Value = "1" Then
                        .Add("@BLID1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLID1"))
                        .Add("@CARRIER1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CARRIER1"))
                        'If Convert.ToString(dr.Item("BLAPPDATE1")) = "" Then
                        '    .Add("@BLAPPDATE1", SqlDbType.Date).Value = dt
                        'Else
                        '    .Add("@BLAPPDATE1", SqlDbType.Date).Value = Convert.ToString(dr.Item("BLAPPDATE1"))
                        'End If
                        '.Add("@RECIEPTCOUNTRY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("RECIEPTCOUNTRY1"))
                        '.Add("@RECIEPTPORT1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("RECIEPTPORT1"))
                        '.Add("@LOADCOUNTRY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LOADCOUNTRY1"))
                        '.Add("@LOADPORT1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LOADPORT1"))
                        '.Add("@DISCHARGECOUNTRY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DISCHARGECOUNTRY1"))
                        '.Add("@DISCHARGEPORT1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DISCHARGEPORT1"))
                        '.Add("@DELIVERYCOUNTRY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DELIVERYCOUNTRY1"))
                        '.Add("@DELIVERYPORT1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DELIVERYPORT1"))
                        .Add("@VSL1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("VSL1"))
                        .Add("@VOY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("VOY1"))
                        .Add("@NOTIFYCONTTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("NOTIFYCONTTEXT1"))

                        .Add("@LDNVSL1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LDNVSL1"))
                        .Add("@LDNPOL1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LDNPOL1"))
                        .Add("@LDNDATE1", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("LDNDATE1")))
                        .Add("@LDNBY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LDNBY1"))
                        .Add("@BOOKINGNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BOOKINGNO"))

                        .Add("@SHIPPERTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("SHIPPERTEXT"))
                        .Add("@CONSIGNEETEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CONSIGNEETEXT"))
                        .Add("@NOTIFYTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("NOTIFYTEXT"))
                        .Add("@PRECARRIAGETEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PRECARRIAGETEXT"))
                        .Add("@FINDESTINATIONTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("FINDESTINATIONTEXT"))
                        .Add("@DECLAREDVALUE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("DECLAREDVALUE")))
                        .Add("@REVENUETONS", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REVENUETONS"))
                        .Add("@RATE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("RATE"))
                        .Add("@PER", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PER"))
                        .Add("@PREPAID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PREPAID"))
                        .Add("@COLLECT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("COLLECT"))
                        .Add("@CARRIERBLNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CARRIERBLNO"))
                        .Add("@NOOFBL", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("NOOFBL"))
                        .Add("@BLTYPE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLTYPE"))
                        .Add("@PAYMENTPLACE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PAYMENTPLACE"))
                        .Add("@BLISSUEPLACE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLISSUEPLACE"))
                        .Add("@ANISSUEPLACE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ANISSUEPLACE"))
                        .Add("@MEASUREMENT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("MEASUREMENT"))
                        .Add("@CARRIERBLTYPE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CARRIERBLTYPE"))
                        .Add("@DEMUFORACCT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DEMUFORACCT"))
                        .Add("@GOODSPKGS", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("GOODSPKGS"))

                        .Add("@BLRECEIPT1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLRECEIPT1"))
                        .Add("@BLLOADING1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLLOADING1"))
                        .Add("@BLDISCHARGE1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLDISCHARGE1"))
                        .Add("@BLDELIVERY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLDELIVERY1"))
                        .Add("@BLPLACEDATEISSUE1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLPLACEDATEISSUE1"))

                        .Add("@TRANSIT1VSL1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TRANSIT1VSL1"))
                        .Add("@TRANSIT1VOY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TRANSIT1VOY1"))
                        .Add("@TRANSIT2VSL1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TRANSIT2VSL1"))
                        .Add("@TRANSIT2VOY1", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TRANSIT2VOY1"))

                    ElseIf Me.hdnWhichTrans.Value = "2" Then
                        .Add("@BLID2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLID2"))
                        .Add("@CARRIER2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CARRIER2"))
                        'If Convert.ToString(dr.Item("BLAPPDATE2")) = "" Then
                        '    .Add("@BLAPPDATE2", SqlDbType.Date).Value = dt
                        'Else
                        '    .Add("@BLAPPDATE2", SqlDbType.Date).Value = Convert.ToString(dr.Item("BLAPPDATE2"))
                        'End If
                        '.Add("@RECIEPTCOUNTRY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("RECIEPTCOUNTRY2"))
                        '.Add("@RECIEPTPORT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("RECIEPTPORT2"))
                        '.Add("@LOADCOUNTRY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LOADCOUNTRY2"))
                        '.Add("@LOADPORT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LOADPORT2"))
                        '.Add("@DISCHARGECOUNTRY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DISCHARGECOUNTRY2"))
                        '.Add("@DISCHARGEPORT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DISCHARGEPORT2"))
                        '.Add("@DELIVERYCOUNTRY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DELIVERYCOUNTRY2"))
                        '.Add("@DELIVERYPORT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DELIVERYPORT2"))
                        .Add("@VSL2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("VSL2"))
                        .Add("@VOY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("VOY2"))
                        .Add("@NOTIFYCONTTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("NOTIFYCONTTEXT2"))

                        .Add("@LDNVSL2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LDNVSL2"))
                        .Add("@LDNPOL2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LDNPOL2"))
                        .Add("@LDNDATE2", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("LDNDATE2")))
                        .Add("@LDNBY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LDNBY2"))
                        .Add("@BOOKINGNO2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BOOKINGNO2"))

                        .Add("@SHIPPERTEXT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("SHIPPERTEXT2"))
                        .Add("@CONSIGNEETEXT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CONSIGNEETEXT2"))
                        .Add("@NOTIFYTEXT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("NOTIFYTEXT2"))
                        .Add("@PRECARRIAGETEXT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PRECARRIAGETEXT2"))
                        .Add("@FINDESTINATIONTEXT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("FINDESTINATIONTEXT2"))
                        .Add("@DECLAREDVALUE2", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("DECLAREDVALUE2")))
                        .Add("@REVENUETONS2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REVENUETONS2"))
                        .Add("@RATE2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("RATE2"))
                        .Add("@PER2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PER2"))
                        .Add("@PREPAID2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PREPAID2"))
                        .Add("@COLLECT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("COLLECT2"))
                        .Add("@CARRIERBLNO2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CARRIERBLNO2"))
                        .Add("@NOOFBL2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("NOOFBL2"))
                        .Add("@BLTYPE2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLTYPE2"))
                        .Add("@PAYMENTPLACE2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PAYMENTPLACE2"))
                        .Add("@BLISSUEPLACE2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLISSUEPLACE2"))
                        .Add("@ANISSUEPLACE2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ANISSUEPLACE2"))
                        .Add("@MEASUREMENT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("MEASUREMENT2"))
                        .Add("@CARRIERBLTYPE2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CARRIERBLTYPE2"))
                        .Add("@DEMUFORACCT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DEMUFORACCT2"))

                        .Add("@BLRECEIPT2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLRECEIPT2"))
                        .Add("@BLLOADING2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLLOADING2"))
                        .Add("@BLDISCHARGE2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLDISCHARGE2"))
                        .Add("@BLDELIVERY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLDELIVERY2"))
                        .Add("@BLPLACEDATEISSUE2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BLPLACEDATEISSUE2"))

                        .Add("@TRANSIT1VSL2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TRANSIT1VSL2"))
                        .Add("@TRANSIT1VOY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TRANSIT1VOY2"))
                        .Add("@TRANSIT2VSL2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TRANSIT2VSL2"))
                        .Add("@TRANSIT2VOY2", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TRANSIT2VOY2"))

                    End If
                    '.Add("@CONTAINERNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CONTAINERNO"))
                    .Add("@FREIGHTANDCHARGES", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("FREIGHTANDCHARGES"))
                    .Add("@CONTAINERPKGS", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CONTAINERPKGS"))
                    '.Add("@NOOFCONTAINER", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("NOOFCONTAINER"))
                    '.Add("@BOOKINGNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BOOKINGNO"))
                    '.Add("@TERMTYPE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TERMTYPE"))
                    .Add("@NOOFPACKAGE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("NOOFPACKAGE"))
                    '.Add("@EXCHANGERATE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("EXCHANGERATE")))
                    '.Add("@PREPAIDAT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PREPAIDAT"))
                    '.Add("@PAYABLEAT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("PAYABLEAT"))
                    '.Add("@LOCALCURRENCY", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LOCALCURRENCY"))
                    '.Add("@MARKSANDNUMBERS", SqlDbType.NVarChar).Value = marksNumbers
                    .Add("@MARKSANDNUMBERS", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("MARKSANDNUMBERS"))
                    .Add("@TANKINFO", SqlDbType.NVarChar).Value = tankInfo

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.ExecuteNonQuery()
            End Using

            tran.Commit() 'トランザクションコミット

        Catch ex As Exception
            Throw
        Finally
            If tran IsNot Nothing Then
                tran.Dispose()
            End If
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try

    End Sub
    ''' <summary>
    ''' オーダー基本情報取得処理
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <returns></returns>
    Private Function GetOrderBase(ByVal orderNo As String, Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = Nothing
        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("SELECT OB.ORDERNO AS ORDERNO")
        sqlStat.AppendLine("      ,CASE OB.STYMD   WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.STYMD   ,'yyyy/MM/dd') END AS STYMD")
        sqlStat.AppendLine("      ,CASE OB.ENDYMD  WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.ENDYMD  ,'yyyy/MM/dd') END AS ENDYMD")
        sqlStat.AppendLine("      ,OB.BRID AS BRID")
        sqlStat.AppendLine("      ,OB.BRTYPE AS BRTYPE")
        sqlStat.AppendLine("      ,CASE OB.VALIDITYFROM   WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.VALIDITYFROM   ,'yyyy/MM/dd') END AS VALIDITYFROM")
        sqlStat.AppendLine("      ,CASE OB.VALIDITYTO  WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.VALIDITYTO  ,'yyyy/MM/dd') END AS VALIDITYTO")
        sqlStat.AppendLine("      ,OB.TERMTYPE AS TERMTYPE")
        sqlStat.AppendLine("      ,OB.NOOFTANKS AS NOOFTANKS")
        sqlStat.AppendLine("      ,OB.SHIPPER AS SHIPPER")
        sqlStat.AppendLine("      ,OB.CONSIGNEE AS CONSIGNEE")
        sqlStat.AppendLine("      ,OB.CARRIER1 AS CARRIER1")
        sqlStat.AppendLine("      ,OB.CARRIER2 AS CARRIER2")
        sqlStat.AppendLine("      ,OB.PRODUCTCODE AS PRODUCTCODE")
        sqlStat.AppendLine("      ,OB.PRODUCTWEIGHT AS PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,OB.RECIEPTCOUNTRY1 AS RECIEPTCOUNTRY1")
        sqlStat.AppendLine("      ,OB.RECIEPTPORT1 AS RECIEPTPORT1")
        sqlStat.AppendLine("      ,OB.RECIEPTCOUNTRY2 AS RECIEPTCOUNTRY2")
        sqlStat.AppendLine("      ,OB.RECIEPTPORT2 AS RECIEPTPORT2")
        sqlStat.AppendLine("      ,OB.LOADCOUNTRY1 AS LOADCOUNTRY1")
        sqlStat.AppendLine("      ,OB.LOADPORT1 AS LOADPORT1")
        sqlStat.AppendLine("      ,OB.LOADCOUNTRY2 AS LOADCOUNTRY2")
        sqlStat.AppendLine("      ,OB.LOADPORT2 AS LOADPORT2")
        sqlStat.AppendLine("      ,OB.DISCHARGECOUNTRY1 AS DISCHARGECOUNTRY1")
        sqlStat.AppendLine("      ,OB.DISCHARGEPORT1 AS DISCHARGEPORT1")
        sqlStat.AppendLine("      ,OB.DISCHARGECOUNTRY2 AS DISCHARGECOUNTRY2")
        sqlStat.AppendLine("      ,OB.DISCHARGEPORT2 AS DISCHARGEPORT2")
        sqlStat.AppendLine("      ,OB.DELIVERYCOUNTRY1 AS DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,OB.DELIVERYPORT1 AS DELIVERYPORT1")
        sqlStat.AppendLine("      ,OB.DELIVERYCOUNTRY2 AS DELIVERYCOUNTRY2")
        sqlStat.AppendLine("      ,OB.DELIVERYPORT2 AS DELIVERYPORT2")
        sqlStat.AppendLine("      ,OB.VSL1 AS VSL1")
        sqlStat.AppendLine("      ,OB.VOY1 AS VOY1")
        sqlStat.AppendLine("      ,CASE OB.ETD1 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.ETD1,'yyyy/MM/dd') END AS ETD1")
        sqlStat.AppendLine("      ,CASE OB.ETA1 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.ETA1,'yyyy/MM/dd') END AS ETA1")
        sqlStat.AppendLine("      ,OB.VSL2 AS VSL2")
        sqlStat.AppendLine("      ,OB.VOY2 AS VOY2")
        sqlStat.AppendLine("      ,CASE OB.ETD2 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.ETD2,'yyyy/MM/dd') END AS ETD2")
        sqlStat.AppendLine("      ,CASE OB.ETA2 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.ETA2,'yyyy/MM/dd') END AS ETA2")
        sqlStat.AppendLine("      ,OB.INVOICEDBY AS INVOICEDBY")
        sqlStat.AppendLine("      ,OB.LOADING AS LOADING")
        sqlStat.AppendLine("      ,OB.STEAMING AS STEAMING")
        sqlStat.AppendLine("      ,OB.TIP AS TIP")
        sqlStat.AppendLine("      ,OB.EXTRA AS EXTRA")
        sqlStat.AppendLine("      ,OB.DEMURTO AS DEMURTO")
        sqlStat.AppendLine("      ,OB.DEMURUSRATE1 AS DEMURUSRATE1")
        sqlStat.AppendLine("      ,OB.DEMURUSRATE2 AS DEMURUSRATE2")
        sqlStat.AppendLine("      ,OB.SALESPIC AS SALESPIC")
        sqlStat.AppendLine("      ,OB.AGENTORGANIZER AS AGENTORGANIZER")
        sqlStat.AppendLine("      ,OB.AGENTPOL1 AS AGENTPOL1")
        sqlStat.AppendLine("      ,OB.AGENTPOL2 AS AGENTPOL2")
        sqlStat.AppendLine("      ,OB.AGENTPOD1 AS AGENTPOD1")
        sqlStat.AppendLine("      ,OB.AGENTPOD2 AS AGENTPOD2")
        sqlStat.AppendLine("      ,OB.BLID1 AS BLID1")
        sqlStat.AppendLine("      ,CASE CONVERT(NVARCHAR, OB.BLAPPDATE1 , 111) WHEN '1900/01/01' THEN '' ELSE CONVERT(NVARCHAR, OB.BLAPPDATE1 , 111) END AS BLAPPDATE1")
        sqlStat.AppendLine("      ,OB.BLID2 AS BLID2")
        sqlStat.AppendLine("      ,CASE CONVERT(NVARCHAR, OB.BLAPPDATE2 , 111) WHEN '1900/01/01' THEN '' ELSE CONVERT(NVARCHAR, OB.BLAPPDATE2 , 111) END AS BLAPPDATE2")
        sqlStat.AppendLine("      ,OB.SHIPPERNAME AS SHIPPERNAME")
        sqlStat.AppendLine("      ,OB.SHIPPERTEXT AS SHIPPERTEXT")
        sqlStat.AppendLine("      ,OB.SHIPPERTEXT2 AS SHIPPERTEXT2")
        sqlStat.AppendLine("      ,OB.CONSIGNEENAME AS CONSIGNEENAME")
        sqlStat.AppendLine("      ,OB.CONSIGNEETEXT AS CONSIGNEETEXT")
        sqlStat.AppendLine("      ,OB.CONSIGNEETEXT2 AS CONSIGNEETEXT2")
        sqlStat.AppendLine("      ,OB.IECCODE AS IECCODE")
        sqlStat.AppendLine("      ,OB.NOTIFYNAME AS NOTIFYNAME")
        sqlStat.AppendLine("      ,OB.NOTIFYTEXT AS NOTIFYTEXT")
        sqlStat.AppendLine("      ,OB.NOTIFYTEXT2 AS NOTIFYTEXT2")
        sqlStat.AppendLine("      ,OB.NOTIFYCONT AS NOTIFYCONT")
        sqlStat.AppendLine("      ,OB.NOTIFYCONTNAME AS NOTIFYCONTNAME")
        sqlStat.AppendLine("      ,OB.NOTIFYCONTTEXT1 AS NOTIFYCONTTEXT1")
        sqlStat.AppendLine("      ,OB.NOTIFYCONTTEXT2 AS NOTIFYCONTTEXT2")
        sqlStat.AppendLine("      ,OB.PRECARRIAGETEXT AS PRECARRIAGETEXT")
        sqlStat.AppendLine("      ,OB.PRECARRIAGETEXT2 AS PRECARRIAGETEXT2")
        sqlStat.AppendLine("      ,OB.VSL AS VSL")
        sqlStat.AppendLine("      ,OB.VOY AS VOY")
        sqlStat.AppendLine("      ,OB.FINDESTINATIONNAME AS FINDESTINATIONNAME")
        sqlStat.AppendLine("      ,OB.FINDESTINATIONTEXT AS FINDESTINATIONTEXT")
        sqlStat.AppendLine("      ,OB.FINDESTINATIONTEXT2 AS FINDESTINATIONTEXT2")
        sqlStat.AppendLine("      ,OB.PRODUCT AS PRODUCT")
        sqlStat.AppendLine("      ,OB.PRODUCTPORDER AS PRODUCTPORDER")
        sqlStat.AppendLine("      ,OB.PRODUCTTIP AS PRODUCTTIP")
        sqlStat.AppendLine("      ,OB.PRODUCTFREIGHT AS PRODUCTFREIGHT")
        sqlStat.AppendLine("      ,OB.FREIGHTANDCHARGES AS FREIGHTANDCHARGES")
        sqlStat.AppendLine("      ,OB.PREPAIDAT AS PREPAIDAT")
        sqlStat.AppendLine("      ,OB.GOODSPKGS AS GOODSPKGS")
        sqlStat.AppendLine("      ,OB.CONTAINERPKGS AS CONTAINERPKGS")
        sqlStat.AppendLine("      ,OB.BLNUM AS BLNUM")
        sqlStat.AppendLine("      ,OB.CONTAINERNO AS CONTAINERNO")
        sqlStat.AppendLine("      ,OB.SEALNO AS SEALNO")
        sqlStat.AppendLine("      ,OB.NOOFCONTAINER AS NOOFCONTAINER")
        sqlStat.AppendLine("      ,OB.DECLAREDVALUE AS DECLAREDVALUE")
        sqlStat.AppendLine("      ,OB.DECLAREDVALUE2 AS DECLAREDVALUE2")
        sqlStat.AppendLine("      ,OB.REVENUETONS AS REVENUETONS")
        sqlStat.AppendLine("      ,OB.REVENUETONS2 AS REVENUETONS2")
        sqlStat.AppendLine("      ,OB.RATE AS RATE")
        sqlStat.AppendLine("      ,OB.RATE2 AS RATE2")
        sqlStat.AppendLine("      ,OB.PER AS PER")
        sqlStat.AppendLine("      ,OB.PER2 AS PER2")
        sqlStat.AppendLine("      ,OB.PREPAID AS PREPAID")
        sqlStat.AppendLine("      ,OB.PREPAID2 AS PREPAID2")
        sqlStat.AppendLine("      ,OB.COLLECT AS COLLECT")
        sqlStat.AppendLine("      ,OB.COLLECT2 AS COLLECT2")
        sqlStat.AppendLine("      ,OB.EXCHANGERATE AS EXCHANGERATE")
        sqlStat.AppendLine("      ,OB.PAYABLEAT AS PAYABLEAT")
        sqlStat.AppendLine("      ,OB.LOCALCURRENCY AS LOCALCURRENCY")
        sqlStat.AppendLine("      ,OB.CARRIERBLNO AS CARRIERBLNO")
        sqlStat.AppendLine("      ,OB.CARRIERBLNO2 AS CARRIERBLNO2")
        sqlStat.AppendLine("      ,OB.BOOKINGNO AS BOOKINGNO")
        sqlStat.AppendLine("      ,OB.BOOKINGNO2 AS BOOKINGNO2")
        'sqlStat.AppendLine("      ,OB.NOOFPACKAGE AS NOOFPACKAGE")
        sqlStat.AppendLine("      ,OB.BLTYPE AS BLTYPE")
        sqlStat.AppendLine("      ,OB.BLTYPE2 AS BLTYPE2")
        sqlStat.AppendLine("      ,OB.NOOFBL AS NOOFBL")
        sqlStat.AppendLine("      ,OB.NOOFBL2 AS NOOFBL2")
        sqlStat.AppendLine("      ,OB.PAYMENTPLACE AS PAYMENTPLACE")
        sqlStat.AppendLine("      ,OB.PAYMENTPLACE2 AS PAYMENTPLACE2")
        sqlStat.AppendLine("      ,OB.BLISSUEPLACE AS BLISSUEPLACE")
        sqlStat.AppendLine("      ,OB.BLISSUEPLACE2 AS BLISSUEPLACE2")
        sqlStat.AppendLine("      ,OB.ANISSUEPLACE AS ANISSUEPLACE")
        sqlStat.AppendLine("      ,OB.ANISSUEPLACE2 AS ANISSUEPLACE2")
        sqlStat.AppendLine("      ,OB.MEASUREMENT AS MEASUREMENT")
        sqlStat.AppendLine("      ,OB.MEASUREMENT2 AS MEASUREMENT2")
        sqlStat.AppendLine("      ,OB.MARKSANDNUMBERS AS MARKSANDNUMBERS")
        sqlStat.AppendLine("      ,OB.LDNVSL1 AS LDNVSL1")
        sqlStat.AppendLine("      ,OB.LDNPOL1 AS LDNPOL1")
        sqlStat.AppendLine("      ,CASE OB.LDNDATE1 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.LDNDATE1,'yyyy/MM/dd') END AS LDNDATE1")
        sqlStat.AppendLine("      ,OB.LDNBY1 AS LDNBY1")
        sqlStat.AppendLine("      ,OB.LDNVSL2 AS LDNVSL2")
        sqlStat.AppendLine("      ,OB.LDNPOL2 AS LDNPOL2")
        sqlStat.AppendLine("      ,CASE OB.LDNDATE2 WHEN '1900/01/01' THEN '' ELSE FORMAT(OB.LDNDATE2,'yyyy/MM/dd') END AS LDNDATE2")
        sqlStat.AppendLine("      ,OB.LDNBY2 AS LDNBY2")
        sqlStat.AppendLine("      ,OB.CARRIERBLTYPE AS CARRIERBLTYPE")
        sqlStat.AppendLine("      ,OB.CARRIERBLTYPE2 AS CARRIERBLTYPE2")
        sqlStat.AppendLine("      ,OB.DEMUFORACCT AS DEMUFORACCT")
        sqlStat.AppendLine("      ,OB.DEMUFORACCT2 AS DEMUFORACCT2")
        sqlStat.AppendLine("      ,OB.BLRECEIPT1 AS BLRECEIPT1")
        sqlStat.AppendLine("      ,OB.BLRECEIPT2 AS BLRECEIPT2")
        sqlStat.AppendLine("      ,OB.BLLOADING1 AS BLLOADING1")
        sqlStat.AppendLine("      ,OB.BLLOADING2 AS BLLOADING2")
        sqlStat.AppendLine("      ,OB.BLDISCHARGE1 AS BLDISCHARGE1")
        sqlStat.AppendLine("      ,OB.BLDISCHARGE2 AS BLDISCHARGE2")
        sqlStat.AppendLine("      ,OB.BLDELIVERY1 AS BLDELIVERY1")
        sqlStat.AppendLine("      ,OB.BLDELIVERY2 AS BLDELIVERY2")
        sqlStat.AppendLine("      ,OB.BLPLACEDATEISSUE1 AS BLPLACEDATEISSUE1")
        sqlStat.AppendLine("      ,OB.BLPLACEDATEISSUE2 AS BLPLACEDATEISSUE2")

        sqlStat.AppendLine("      ,OB.TRANSIT1VSL1 AS TRANSIT1VSL1")
        sqlStat.AppendLine("      ,OB.TRANSIT1VOY1 AS TRANSIT1VOY1")
        sqlStat.AppendLine("      ,OB.TRANSIT2VSL1 AS TRANSIT2VSL1")
        sqlStat.AppendLine("      ,OB.TRANSIT2VOY1 AS TRANSIT2VOY1")
        sqlStat.AppendLine("      ,OB.TRANSIT1VSL2 AS TRANSIT1VSL2")
        sqlStat.AppendLine("      ,OB.TRANSIT1VOY2 AS TRANSIT1VOY2")
        sqlStat.AppendLine("      ,OB.TRANSIT2VSL2 AS TRANSIT2VSL2")
        sqlStat.AppendLine("      ,OB.TRANSIT2VOY2 AS TRANSIT2VOY2")

        sqlStat.AppendLine("      ,OB.REMARK AS REMARK")
        sqlStat.AppendLine("      ,OB.DELFLG AS DELFLG")
        sqlStat.AppendLine("      ,CAST(OB.UPDTIMSTP As bigint) AS TIMSTP")
        sqlStat.AppendLine("      ,ISNULL(TRIM(PD.PRODUCTNAME),'') AS PRODUCTNAME")
        sqlStat.AppendLine("      ,OB.UPDYMD AS UPDYMD")
        sqlStat.AppendLine("      ,OB.UPDUSER AS UPDUSER")
        sqlStat.AppendLine("      ,OB.UPDTERMID AS UPDTERMID")
        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE OB ")
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD ")
        sqlStat.AppendLine("    ON PD.PRODUCTCODE  = OB.PRODUCTCODE")
        sqlStat.AppendLine("   AND PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND PD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine(" WHERE OB.ORDERNO      = @ORDERNO ")
        sqlStat.AppendLine("   AND OB.DELFLG      <> @DELFLG ")
        sqlStat.AppendLine("   And OB.STYMD       <= @STYMD")
        sqlStat.AppendLine("   And OB.ENDYMD      >= @ENDYMD")
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
                'SQLパラメータ値セット
                paramOrderNo.Value = orderNo
                paramDelFlg.Value = CONST_FLAG_YES
                paramStYmd.Value = Date.Now
                paramEndYmd.Value = Date.Now
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Order base info Error")
                    End If
                    retDt = CreateOrderBaseTable()
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
    ''' オーダー明細情報取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOrderValue(ByVal orderNo As String, Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = Nothing
        Dim sqlStat As New Text.StringBuilder
        Dim nameField As String = "NAMESJP"
        If BASEDLL.COA0019Session.LANGDISP <> C_LANG.JA Then
            nameField = "NAMES"
        End If
        sqlStat.AppendLine("SELECT OV2.ORDERNO AS ORDERNO")
        sqlStat.AppendLine("      ,OV.TANKNO AS TANKNO")
        sqlStat.AppendLine("      ,OV2.TANKSEQ AS TANKSEQ")
        sqlStat.AppendLine("      ,OV2.TANKTYPE AS TANKTYPE")
        sqlStat.AppendLine("      ,OV2.GROSSWEIGHT AS GROSSWEIGHT")
        sqlStat.AppendLine("      ,OV2.NETWEIGHT AS NETWEIGHT")
        sqlStat.AppendLine("      ,OV2.SEALNO1 AS SEALNO1")
        sqlStat.AppendLine("      ,OV2.SEALNO2 AS SEALNO2")
        sqlStat.AppendLine("      ,OV2.SEALNO3 AS SEALNO3")
        sqlStat.AppendLine("      ,OV2.SEALNO4 AS SEALNO4")
        sqlStat.AppendLine("      ,OV2.EMPTYORFULL AS EMPTYORFULL")
        sqlStat.AppendLine("      ,OV2.NOOFPACKAGE AS NOOFPACKAGE")
        sqlStat.AppendLine("      ,OV2.EXSHIPRATE AS EXSHIPRATE")
        sqlStat.AppendLine("      ,OV2.INSHIPRATE AS INSHIPRATE")
        sqlStat.AppendLine("      ,OV2.DISPSEQ AS DISPSEQ")
        sqlStat.AppendLine("      ,OV2.WORKC1 AS WORKC1")
        sqlStat.AppendLine("      ,OV2.WORKC2 AS WORKC2")
        sqlStat.AppendLine("      ,OV2.WORKC3 AS WORKC3")
        sqlStat.AppendLine("      ,OV2.WORKC4 AS WORKC4")
        sqlStat.AppendLine("      ,OV2.WORKC5 AS WORKC5")
        sqlStat.AppendLine("      ,OV2.WORKF1 AS WORKF1")
        sqlStat.AppendLine("      ,OV2.WORKF2 AS WORKF2")
        sqlStat.AppendLine("      ,OV2.WORKF3 AS WORKF3")
        sqlStat.AppendLine("      ,OV2.WORKF4 AS WORKF4")
        sqlStat.AppendLine("      ,OV2.WORKF5 AS WORKF5")
        sqlStat.AppendLine("      ,TK.NETWEIGHT AS TAREWEIGHT")
        sqlStat.AppendLine("  FROM GBT0007_ODR_VALUE2 OV2 ")
        sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE OV ")
        sqlStat.AppendLine("    ON OV.ORDERNO   = OV2.ORDERNO")
        sqlStat.AppendLine("   AND OV.TANKSEQ   = OV2.TANKSEQ")
        sqlStat.AppendLine("   AND OV.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND OV.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND OV.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("   And OV.TANKNO   <> ''")

        If Me.hdnWhichTrans.Value = "1" Then
            sqlStat.AppendLine("   AND OV.DTLPOLPOD IN ('POL1','POD1') ")
            sqlStat.AppendLine("   AND OV.ACTIONID IN ('SHIP','RPEC','RPED','RPHC','RPHD') ")
        ElseIf Me.hdnWhichTrans.Value = "2" Then
            sqlStat.AppendLine("   AND OV.DTLPOLPOD IN ('POL2','POD2') ")
            sqlStat.AppendLine("   AND OV.ACTIONID IN ('SHIP','RPEC','RPED','RPHC','RPHD') ")
        End If

        sqlStat.AppendLine("  LEFT JOIN GBM0006_TANK TK ")
        sqlStat.AppendLine("    ON TK.TANKNO    = OV.TANKNO")
        sqlStat.AppendLine("   AND TK.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND TK.ENDYMD   >= @ENDYMD")
        sqlStat.AppendLine("   AND TK.DELFLG   <> @DELFLG")
        sqlStat.AppendLine(" WHERE OV2.ORDERNO  = @ORDERNO ")
        sqlStat.AppendLine("   AND OV2.DELFLG  <> @DELFLG ")
        sqlStat.AppendLine("   And OV2.STYMD   <= @STYMD")
        sqlStat.AppendLine("   And OV2.ENDYMD  >= @ENDYMD")

        If Me.hdnWhichTrans.Value = "1" Then
            sqlStat.AppendLine("   And OV2.TRILATERAL = '1' ")
        ElseIf Me.hdnWhichTrans.Value = "2" Then
            sqlStat.AppendLine("   And OV2.TRILATERAL = '2' ")
        End If

        'sqlStat.AppendLine(" ORDER BY (CASE WHEN ISNULL(OV.TANKNO,'') = '' THEN '1' ELSE '0' END) , OV.TANKNO ")
        sqlStat.AppendLine(" ORDER BY (CASE WHEN ISNULL(OV.TANKNO,'') = '' THEN '1' ELSE '0' END) , OV2.DISPSEQ, OV.TANKNO ")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                retDt = CreateOrderValueTable()
                'SQLパラメータ設定
                Dim paramOrderNo As SqlParameter = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 20)
                Dim paramStYmd As SqlParameter = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
                Dim paramEndYmd As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
                Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)

                'SQLパラメータ値セット
                paramOrderNo.Value = orderNo
                paramStYmd.Value = Date.Now
                paramEndYmd.Value = Date.Now
                paramDelFlg.Value = CONST_FLAG_YES

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Order value info Error")
                    End If
                    Dim dicCandeleteCode As New Dictionary(Of String, String)

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
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    Protected Sub UploadExcel()
        Dim ds As New DataSet
        Dim dt As DataTable = Nothing

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        Dim reportMapId As String = ""

        '一旦画面費用項目をviewstateに退避
        SaveGridItem()
        '画面費用を取得しデータテーブルに格納
        dt = CollectDisplayOrderValue()
        reportMapId = "GBT00014"

        'Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル
        'Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        Dim COA0029XlsTable As New BASEDLL.COA0029XlsTable

        ''初期処理
        'errList = New List(Of String)
        'errListAll = New List(Of String)
        Dim returnCode As String = C_MESSAGENO.NORMAL

        ''UPLOAD_XLSデータ取得
        COA0029XlsTable.MAPID = reportMapId
        COA0029XlsTable.COA0029XlsToTable()
        'COA0029XlsTable.TBLDATA = dt
        If COA0029XlsTable.ERR = C_MESSAGENO.NORMAL Then
            If COA0029XlsTable.TBLDATA.Rows.Count = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        Else
            returnCode = COA0029XlsTable.ERR
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'コスト情報の場合
        Dim inportCostList As New List(Of COSTITEM)
        Dim targetCostData As List(Of COSTITEM) = Nothing
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim uniqueIndex = 0

        For Each dr As DataRow In COA0029XlsTable.TBLDATA.Rows
            'タンク番号が空白の場合はそのまま終了
            If Convert.ToString(dr.Item("TANKNO")).Trim = "" Then
                Continue For
            End If

            targetCostData = Nothing
            targetCostData = (From costItemRow In costData Where costItemRow.TankNo = Convert.ToString(dr.Item("TANKNO")).Trim).ToList

            '同じタンク番号がない場合はそのまま終了
            If targetCostData.Count = 0 Then
                Continue For
            End If

            Dim tankDt As DataTable = GetTank(targetCostData.Item(0).TankNo)
            Dim costitem As New COSTITEM
            costitem.OrderNo = Me.lblOrderNo.Text
            costitem.TankNo = targetCostData.Item(0).TankNo
            costitem.TankSeq = targetCostData.Item(0).TankSeq
            costitem.TankType = Convert.ToString(dr.Item("TANKTYPE")).Trim
            costitem.SealNo1 = Convert.ToString(dr.Item("SEALNO1")).Trim
            costitem.SealNo2 = Convert.ToString(dr.Item("SEALNO2")).Trim
            costitem.SealNo3 = Convert.ToString(dr.Item("SEALNO3")).Trim
            costitem.SealNo4 = Convert.ToString(dr.Item("SEALNO4")).Trim
            costitem.GrossWeight = "0"
            costitem.NetWeight = Convert.ToString(dr.Item("NETWEIGHT")).Trim
            costitem.EmptyOrFull = Convert.ToString(dr.Item("EMPTYORFULL")).Trim
            costitem.NoOfPackage = Convert.ToString(dr.Item("NOOFPACKAGE")).Trim
            costitem.ShipRateEx = Me.txtShipRateEx.Text
            costitem.ShipRateIn = Me.txtShipRateIn.Text
            costitem.TareWeight = Convert.ToString(tankDt.Rows(0).Item("TAREWEIGHT"))
            costitem.DispSeq = Convert.ToString(dr.Item("DISPSEQ")).Trim
            'costitem.WorkC1 = Convert.ToString(dr.Item("WORKC1")).Trim
            'costitem.WorkC2 = Convert.ToString(dr.Item("WORKC2")).Trim
            'costitem.WorkC3 = Convert.ToString(dr.Item("WORKC3")).Trim
            'costitem.WorkC4 = Convert.ToString(dr.Item("WORKC4")).Trim
            'costitem.WorkC5 = Convert.ToString(dr.Item("WORKC5")).Trim
            'costitem.WorkF1 = Convert.ToString(dr.Item("WORKF1")).Trim
            'costitem.WorkF2 = Convert.ToString(dr.Item("WORKF2")).Trim
            'costitem.WorkF3 = Convert.ToString(dr.Item("WORKF3")).Trim
            'costitem.WorkF4 = Convert.ToString(dr.Item("WORKF4")).Trim
            'costitem.WorkF5 = Convert.ToString(dr.Item("WORKF5")).Trim
            costitem.UniqueIndex = uniqueIndex
            uniqueIndex = uniqueIndex + 1
            Dim tmpNum As Decimal

            If Decimal.TryParse(costitem.GrossWeight, tmpNum) Then
                costitem.GrossWeight = NumberFormat(tmpNum, "", "#,##0.00")
            End If
            If Decimal.TryParse(costitem.NetWeight, tmpNum) Then
                costitem.NetWeight = NumberFormat(tmpNum, "", "#,##0.00")
            End If

            inportCostList.Add(costitem)
        Next

        If inportCostList.Count <> costData.Count Then

            targetCostData = (From costItemRow In costData).ToList
            For Each tagItem In targetCostData
                Dim exFlg As Boolean = False

                For Each inpItem In inportCostList
                    If tagItem.TankNo = inpItem.TankNo Then
                        exFlg = True
                    End If
                Next

                If exFlg Then
                    Continue For
                End If

                Dim tankDt As DataTable = GetTank(tagItem.TankNo)
                Dim costitem As New COSTITEM
                costitem.OrderNo = Me.lblOrderNo.Text
                costitem.TankNo = tagItem.TankNo
                costitem.TankSeq = tagItem.TankSeq
                costitem.TankType = tagItem.TankType
                costitem.SealNo1 = tagItem.SealNo1
                costitem.SealNo2 = tagItem.SealNo2
                costitem.SealNo3 = tagItem.SealNo3
                costitem.SealNo4 = tagItem.SealNo4
                costitem.GrossWeight = tagItem.GrossWeight
                costitem.NetWeight = tagItem.NetWeight
                costitem.EmptyOrFull = tagItem.EmptyOrFull
                costitem.NoOfPackage = tagItem.NoOfPackage
                costitem.ShipRateEx = Me.txtShipRateEx.Text
                costitem.ShipRateIn = Me.txtShipRateIn.Text
                costitem.TareWeight = Convert.ToString(tankDt.Rows(0).Item("TAREWEIGHT"))
                costitem.DispSeq = tagItem.DispSeq
                'costitem.WorkC1 = tagItem.WorkC1
                'costitem.WorkC2 = tagItem.WorkC2
                'costitem.WorkC3 = tagItem.WorkC3
                'costitem.WorkC4 = tagItem.WorkC4
                'costitem.WorkC5 = tagItem.WorkC5
                'costitem.WorkF1 = tagItem.WorkF1
                'costitem.WorkF2 = tagItem.WorkF2
                'costitem.WorkF3 = tagItem.WorkF3
                'costitem.WorkF4 = tagItem.WorkF4
                'costitem.WorkF5 = tagItem.WorkF5

                Dim tmpNum As Decimal

                If Decimal.TryParse(costitem.GrossWeight, tmpNum) Then
                    costitem.GrossWeight = NumberFormat(tmpNum, "", "#,##0.00")
                End If
                If Decimal.TryParse(costitem.NetWeight, tmpNum) Then
                    costitem.NetWeight = NumberFormat(tmpNum, "", "#,##0.00")
                End If

                inportCostList.Add(costitem)
            Next
        End If

        Dim sortInpList = (From allCostItem In inportCostList
                           Order By allCostItem.TankSeq).ToList

        ViewState("COSTLIST") = sortInpList

        Dim showCostList = (From allCostItem In inportCostList
                            Where allCostItem.OrderNo <> "" AndAlso allCostItem.TankNo <> ""
                            Order By allCostItem.TankSeq).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        '初回自動計算
        CalcSummaryNetWeight()
        CalcSummaryGrossWeight()
        CalcSummaryNoOfPackage()

        'メッセージ
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALUPLOAD, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

    End Sub
    ''' <summary>
    ''' ファイルアップロード入力処理(添付ファイル)
    ''' </summary>
    Protected Sub UploadFile()
        'カレントタブ取得
        Dim currentTab As COSTITEM.CostItemGroup = Nothing
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)

        tabObjects.Add(COSTITEM.CostItemGroup.BL, Me.tabBL)
        tabObjects.Add(COSTITEM.CostItemGroup.TANK, Me.tabTank)
        tabObjects.Add(COSTITEM.CostItemGroup.OTHER, Me.tabOther)
        tabObjects.Add(COSTITEM.CostItemGroup.FileUp, Me.tabFileUp)

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected") Then
                currentTab = tabObject.Key
                Exit For
            End If
        Next

        If currentTab <> COSTITEM.CostItemGroup.FileUp Then
            Return
        End If

        'アップロードチェック
        Dim retmsg As String = ""
        uploadChk(retmsg, currentTab)

        If retmsg <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(retmsg, Me.lblFooterMessage)
            Return
        End If

        '初期設定
        Dim UpDir As String = Nothing

        'アップロードファイル名を取得　＆　移動
        UpDir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\" & CONST_DIRECTORY & "\" & CONST_DIRECTORY_SUB & "\"
        UpDir = UpDir & Me.hdnOrderNo.Value & "\" & Me.hdnWhichTrans.Value & "\Update"

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(UpDir) = False Then
            System.IO.Directory.CreateDirectory(UpDir)
        End If

        For Each tempFile As String In System.IO.Directory.GetFiles(COA0019Session.UPLOADDir & "\" & COA0019Session.USERID, "*.*")
            'ディレクトリ付ファイル名より、ファイル名編集
            Dim DirFile As String = System.IO.Path.GetFileName(tempFile)
            '正式フォルダ内全ファイル→Updateフォルダへ上書コピー
            Try
                System.IO.File.Copy(tempFile, UpDir & "\" & DirFile, True)
                System.IO.File.Delete(tempFile)
            Catch ex As Exception
            End Try
        Next

        'Updateディレクトリ内ファイル(追加操作)
        Dim FilesDir As New List(Of String)
        Dim FilesName As New List(Of String)
        Dim FilesDel As New List(Of String)

        For Each tempFile As String In System.IO.Directory.GetFiles(UpDir, "*", System.IO.SearchOption.AllDirectories)
            Dim tFile As String = System.IO.Path.GetFileName(tempFile)
            If FilesName.IndexOf(tFile) = -1 Then
                'ファイルパス格納
                FilesDir.Add(tempFile)
                'ファイル名格納
                FilesName.Add(tFile)
                '削除フラグ格納
                FilesDel.Add(CONST_FLAG_NO)
            End If
        Next

        'Repeaterバインド準備
        FileTblColumnsAdd()

        For i As Integer = 0 To FilesDir.Count - 1
            FileRow = FileTbl.NewRow
            FileRow("FILENAME") = FilesName.Item(i)
            FileRow("DELFLG") = CONST_FLAG_NO
            FileRow("FILEPATH") = FilesDir.Item(i)
            FileTbl.Rows.Add(FileRow)
        Next

        '修理前ファイル

        'Repeaterバインド(空明細)
        dViewRep.DataSource = FileTbl
        dViewRep.DataBind()

        'Repeaterへデータをセット
        For i As Integer = 0 To FilesDir.Count - 1

            'ファイル記号名称
            DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Text = HttpUtility.HtmlEncode(FilesName.Item(i))
            '削除
            DirectCast(dViewRep.Items(i).FindControl("txtRepDelFlg"), System.Web.UI.WebControls.TextBox).Text = CONST_FLAG_NO
            'FILEPATH
            DirectCast(dViewRep.Items(i).FindControl("lblRepFilePath"), System.Web.UI.WebControls.Label).Text = FilesDir.Item(i)

        Next

        'イベント設定
        Dim attr As String = ""
        Dim ItemCnt As Integer = 0
        For i As Integer = 0 To dViewRep.Items.Count - 1
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            attr = "FileDisplay('" & DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Text & "')"
            DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Attributes.Remove("ondblclick")
            DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Attributes.Add("ondblclick", attr)

            ItemCnt = i
            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            attr = "Field_DBclick('vLeftDelFlg' "
            attr = attr & ", '" & ItemCnt.ToString & "'"
            attr = attr & " )"
            DirectCast(dViewRep.Items(i).FindControl("txtRepDelFlg"), System.Web.UI.WebControls.TextBox).Attributes.Remove("ondblclick")
            DirectCast(dViewRep.Items(i).FindControl("txtRepDelFlg"), System.Web.UI.WebControls.TextBox).Attributes.Add("ondblclick", attr)
        Next

        'メッセージ編集
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALIMPORT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
    End Sub
    ''' <summary>
    ''' ファイルカラム設定
    ''' </summary>
    Protected Sub FileTblColumnsAdd()

        If FileTbl.Columns.Count <> 0 Then
            FileTbl.Columns.Clear()
        End If

        'FileTblテンポラリDB項目作成
        FileTbl.Clear()

        FileTbl.Columns.Add("FILENAME", GetType(String))
        FileTbl.Columns.Add("DELFLG", GetType(String))
        FileTbl.Columns.Add("FILEPATH", GetType(String))

    End Sub
    ''' <summary>
    ''' FileDB更新処理
    ''' </summary>
    Protected Sub FileDBupdate(ByVal prmOrderNo As String, ByVal tabId As String, ByVal prmWhichTrans As String)
        '初期設定
        Dim dirSend As String = ""
        Dim dirTemp As String = ""
        Dim dirProd As String = ""
        Dim appFlg As String = ""

        For i As Integer = 0 To dViewRep.Items.Count - 1

            '画面・削除入力処理
            'Detail・表示Fileが、削除フラグONの場合、Updateフォルダ内該当Fileを直接削除
            If DirectCast(dViewRep.Items(i).FindControl("txtRepDelFlg"), System.Web.UI.WebControls.TextBox).Text = CONST_FLAG_YES Then
                Try
                    System.IO.File.Delete(DirectCast(dViewRep.Items(i).FindControl("lblRepFilePath"), System.Web.UI.WebControls.Label).Text)
                Catch ex As Exception
                End Try
            End If
        Next

        '○FTP格納ディレクトリ編集
        '正式ディレクトリ
        dirProd = COA0019Session.UPLOADFILESDir & "\" & CONST_DIRECTORY_SUB & "\" & prmOrderNo & "\" & prmWhichTrans
        appFlg = "2"

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(dirProd) = False Then
            System.IO.Directory.CreateDirectory(dirProd)
        End If

        'Tempフォルダーが存在したら処理する（EXCEL入力の場合、Tempができないため）
        dirTemp = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\" & CONST_DIRECTORY & "\"
        dirTemp = dirTemp & CONST_DIRECTORY_SUB & "\" & prmOrderNo & "\" & prmWhichTrans & "\" & "Update"

        If System.IO.Directory.Exists(dirTemp) Then

            'PDF正式格納フォルダクリア処理
            For Each tempFile As String In System.IO.Directory.GetFiles(dirProd, "*", System.IO.SearchOption.AllDirectories)
                'サブフォルダは対象外
                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next

            'Update_Hフォルダ内容をPDF正式格納フォルダへコピー
            For Each tempFile As String In System.IO.Directory.GetFiles(dirTemp, "*", System.IO.SearchOption.AllDirectories)
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim wkFile As String = System.IO.Path.GetFileName(tempFile)
                'Update_Hフォルダ内PDF→PDF正式格納フォルダへ上書コピー
                System.IO.File.Copy(tempFile, dirProd & "\" & wkFile, True)
            Next

            '集配信用フォルダ格納処理
            Dim COA00034SendDirectory As New COA00034SendDirectory
            Dim pgmDir As String = "\" & CONST_DIRECTORY_SUB & "\" & prmOrderNo & "\" & prmWhichTrans
            COA00034SendDirectory.SendDirectoryCopy(pgmDir, dirProd, appFlg)

        End If
    End Sub
    ''' <summary>
    ''' File Tempディレクトリ削除(PAGE_load時)
    ''' </summary>
    Protected Sub FileInitDel()
        Dim wkUPdirs As String()
        Dim wkUPfiles As String()

        'Temp納ディレクトリ編集
        'ファイル格納Dir作成
        Dim wkDir As String = ""
        wkDir = wkDir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\" & CONST_DIRECTORY & "\" & CONST_DIRECTORY_SUB

        Dim wkDirDel As New List(Of String)

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(wkDir) = False Then
            System.IO.Directory.CreateDirectory(wkDir)
        End If

        'PDF格納ディレクトリ＞MC0006_TODOKESAKI\Temp\ユーザIDフォルダ内のファイル取得
        wkUPdirs = System.IO.Directory.GetDirectories(wkDir, "*", System.IO.SearchOption.AllDirectories)
        For Each tempFile As String In wkUPdirs
            'Tempの自ユーザ内フォルダを取得
            wkDirDel.Add(tempFile)
        Next

        'Listを降順に並べる⇒下位ディレクトリが先頭となる
        wkDirDel.Reverse()

        For i As Integer = 0 To wkDirDel.Count - 1
            'フォルダー内ファイル削除
            wkUPfiles = System.IO.Directory.GetFiles(wkDirDel.Item(i), "*", System.IO.SearchOption.AllDirectories)
            'フォルダー内ファイル削除
            For Each tempFile As String In wkUPfiles
                'ファイル削除
                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                    '読み取り専用などは削除できない
                End Try
            Next

            Try
                'ファイル削除
                System.IO.Directory.Delete(wkDirDel.Item(i))
            Catch ex As Exception
                'ファイルが残っている場合、削除できない
            End Try
        Next

    End Sub
    ''' <summary>
    ''' File読み込み ＆ ディレクトリ作成
    ''' </summary>
    Protected Sub FileInitRead(ByVal tabName As String)
        Dim orderNo As String = Me.hdnOrderNo.Value
        Dim whichTrans As String = Me.hdnWhichTrans.Value

        If orderNo = "" Then
            Return
        End If

        If whichTrans = "" Then
            Return
        End If

        Dim UpFile As String() = Nothing

        '初期設定
        Dim wkDir As String = Nothing

        'フォルダ作成　＆　ファイルコピー
        'File格納Dir作成
        wkDir = ""
        wkDir = wkDir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\" & CONST_DIRECTORY

        '正式ディレクトリ作成＞リペアディレクトリ作成
        If System.IO.Directory.Exists(COA0019Session.UPLOADFILESDir & "\" & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans) Then
        Else
            System.IO.Directory.CreateDirectory(COA0019Session.UPLOADFILESDir & "\" & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans)
        End If

        '一時保存ディレクトリ作成
        If System.IO.Directory.Exists(wkDir & "\" & CONST_DIRECTORY_SUB) Then
        Else
            System.IO.Directory.CreateDirectory(wkDir & "\" & CONST_DIRECTORY_SUB)
        End If

        '一時保存ディレクトリ＞リペアディレクトリ作成
        If System.IO.Directory.Exists(wkDir & "\" & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans) Then
        Else
            System.IO.Directory.CreateDirectory(wkDir & "\" & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans)
        End If

        '一時保存ディレクトリ＞積載品ディレクトリ作成＞Update の処理
        If System.IO.Directory.Exists(wkDir & "\" & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans & "\Update") Then
            '連続処理の場合、前回処理を残す
        Else
            'ユーザIDディレクトリ＞リペアコードディレクトリ作成＞Update 作成
            System.IO.Directory.CreateDirectory(wkDir & "\" & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans & "\Update")

            '正式フォルダ内ファイル→一時保存ディレクトリ＞リペアディレクトリ作成＞Update へコピー

            UpFile = System.IO.Directory.GetFiles(COA0019Session.UPLOADFILESDir & "\" & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans, "*", System.IO.SearchOption.AllDirectories)

            For Each tempFile As String In UpFile
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim wkFile As String = System.IO.Path.GetFileName(tempFile)
                '正式フォルダ内全PDF→Updateフォルダへ上書コピー
                System.IO.File.Copy(tempFile, wkDir & "\" & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans & "\Update" & "\" & wkFile, True)
            Next
        End If

        '画面編集
        '格納ディレクトリ編集
        wkDir = ""
        wkDir = wkDir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\" & CONST_DIRECTORY & "\"
        wkDir = wkDir & CONST_DIRECTORY_SUB & "\" & orderNo & "\" & whichTrans & "\Update"

        'ディレクトリ内ファイル一覧
        Dim wkFilesDir As New List(Of String)
        Dim wkFilesName As New List(Of String)
        Dim wkFilesDel As New List(Of String)

        UpFile = System.IO.Directory.GetFiles(wkDir, "*", System.IO.SearchOption.AllDirectories)
        For Each tempFile As String In UpFile
            Dim wkTempFile As String = System.IO.Path.GetFileName(tempFile)
            If wkFilesName.IndexOf(wkTempFile) = -1 Then
                'ファイルパス格納
                wkFilesDir.Add(tempFile)
                'ファイル名格納
                wkFilesName.Add(wkTempFile)
                '削除フラグ格納
                wkFilesDel.Add(CONST_FLAG_NO)
            End If
        Next

        'Repeaterバインド準備
        FileTblColumnsAdd()

        For i As Integer = 0 To wkFilesDir.Count - 1
            FileRow = FileTbl.NewRow
            FileRow("FILENAME") = wkFilesName.Item(i)
            FileRow("DELFLG") = CONST_FLAG_NO
            FileRow("FILEPATH") = wkFilesDir.Item(i)
            FileTbl.Rows.Add(FileRow)

        Next

        'Repeaterバインド(空明細)
        dViewRep.DataSource = FileTbl
        dViewRep.DataBind()

        DirectCast(hdnListBox, ListBox).Items.Clear()

        'Repeaterへデータをセット
        For i As Integer = 0 To wkFilesDir.Count - 1

            'ファイル記号名称
            DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Text = wkFilesName.Item(i)
            '削除
            DirectCast(dViewRep.Items(i).FindControl("txtRepDelFlg"), System.Web.UI.WebControls.TextBox).Text = CONST_FLAG_NO
            'FILEPATH
            DirectCast(dViewRep.Items(i).FindControl("lblRepFilePath"), System.Web.UI.WebControls.Label).Text = wkFilesDir.Item(i)

            hdnListBox.Items.Add(New ListItem(wkFilesName.Item(i), "0"))
        Next

        'イベント設定
        Dim wkAttr As String = ""
        Dim ItemCnt As Integer = 0
        For i As Integer = 0 To dViewRep.Items.Count - 1
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            wkAttr = "FileDisplay('" & DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Text & "')"
            DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Attributes.Remove("ondblclick")
            DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Attributes.Add("ondblclick", wkAttr)

            ItemCnt = i
            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            wkAttr = "Field_DBclick('vLeftDelFlg' "
            wkAttr = wkAttr & ", '" & ItemCnt.ToString & "'"
            wkAttr = wkAttr & " )"
            DirectCast(dViewRep.Items(i).FindControl("txtRepDelFlg"), System.Web.UI.WebControls.TextBox).Attributes.Remove("ondblclick")
            DirectCast(dViewRep.Items(i).FindControl("txtRepDelFlg"), System.Web.UI.WebControls.TextBox).Attributes.Add("ondblclick", wkAttr)
        Next

    End Sub
    ''' <summary>
    ''' DetailFile内容表示（DetailFileダブルクリック時（内容照会））
    ''' </summary>
    Protected Sub FileDisplay()

        'カレントタブ取得
        Dim currentTab As COSTITEM.CostItemGroup = Nothing
        Dim tabObjects As New Dictionary(Of COSTITEM.CostItemGroup, HtmlGenericControl)

        tabObjects.Add(COSTITEM.CostItemGroup.BL, Me.tabBL)
        tabObjects.Add(COSTITEM.CostItemGroup.TANK, Me.tabTank)
        tabObjects.Add(COSTITEM.CostItemGroup.OTHER, Me.tabOther)
        tabObjects.Add(COSTITEM.CostItemGroup.FileUp, Me.tabFileUp)

        For Each tabObject As KeyValuePair(Of COSTITEM.CostItemGroup, HtmlGenericControl) In tabObjects
            If tabObject.Value.Attributes("class") IsNot Nothing AndAlso tabObject.Value.Attributes("class").Contains("selected") Then
                currentTab = tabObject.Key
                Exit For
            End If
        Next

        If currentTab = COSTITEM.CostItemGroup.FileUp Then

            Dim pwDir As String = COA0019Session.PRINTWORKDir & "\" & COA0019Session.USERID

            For i As Integer = 0 To dViewRep.Items.Count - 1
                'ダブルクリック時コード検索イベント追加
                If DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Text = hdnFileDisplay.Value Then
                    'ディレクトリが存在しない場合、作成する
                    If System.IO.Directory.Exists(pwDir) = False Then
                        System.IO.Directory.CreateDirectory(pwDir)
                    End If

                    'ダウンロードファイル送信準備
                    System.IO.File.Copy(DirectCast(dViewRep.Items(i).FindControl("lblRepFilePath"), System.Web.UI.WebControls.Label).Text,
                                    pwDir & "\" & DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Text, True)

                    'ダウンロード処理へ遷移
                    hdnPrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & COA0019Session.PRINTROOTUrl & "/" & COA0019Session.USERID & "/" &
                                         Uri.EscapeUriString(DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Text)
                    ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

                    Exit For
                End If
            Next

        End If

    End Sub
    ''' <summary>
    ''' グリッド表示用のコストアイテムクラス
    ''' </summary>
    <Serializable>
    Public Class COSTITEM
        Public Enum CostItemGroup As Integer
            ''' <summary>
            ''' BL
            ''' </summary>
            BL = 0
            ''' <summary>
            ''' TANK
            ''' </summary>
            TANK = 1
            ''' <summary>
            ''' OTHER
            ''' </summary>
            OTHER = 2
            ''' <summary>
            ''' FileUp
            ''' </summary>
            FileUp = 3
        End Enum
        ''' <summary>
        ''' オーダー番号
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderNo As String = ""
        ''' <summary>
        ''' タンク番号
        ''' </summary>
        ''' <returns></returns>
        Public Property TankNo As String = ""
        ''' <summary>
        ''' 作業番号(タンクSEQ)
        ''' </summary>
        ''' <returns></returns>
        Public Property TankSeq As String = ""
        ''' <summary>
        ''' タイプ
        ''' </summary>
        ''' <returns></returns>
        Public Property TankType As String = ""
        ''' <summary>
        ''' SealNo1
        ''' </summary>
        ''' <returns></returns>
        Public Property SealNo1 As String = ""
        ''' <summary>
        ''' SealNo2
        ''' </summary>
        ''' <returns></returns>
        Public Property SealNo2 As String = ""
        ''' <summary>
        ''' SealNo3
        ''' </summary>
        ''' <returns></returns>
        Public Property SealNo3 As String = ""
        ''' <summary>
        ''' SealNo4
        ''' </summary>
        ''' <returns></returns>
        Public Property SealNo4 As String = ""
        ''' <summary>
        ''' GrossWeight
        ''' </summary>
        ''' <returns></returns>
        Public Property GrossWeight As String = ""
        ''' <summary>
        ''' NetWeight
        ''' </summary>
        ''' <returns></returns>
        Public Property NetWeight As String = ""
        ''' <summary>
        ''' 空白or完全
        ''' </summary>
        ''' <returns></returns>
        Public Property EmptyOrFull As String = ""

        ''' <summary>
        ''' No Of Package
        ''' </summary>
        ''' <returns></returns>
        Public Property NoOfPackage As String = ""

        ''' <summary>
        ''' 船社レートEx
        ''' </summary>
        ''' <returns></returns>
        Public Property ShipRateEx As String = ""

        ''' <summary>
        ''' 船社レートIn
        ''' </summary>
        ''' <returns></returns>
        Public Property ShipRateIn As String = ""

        ''' <summary>
        ''' 風袋重量
        ''' </summary>
        ''' <returns></returns>
        Public Property TareWeight As String = ""
        ''' <summary>
        ''' 表示順
        ''' </summary>
        ''' <returns></returns>
        Public Property DispSeq As String = ""
        ''' <summary>
        ''' 予備Ｃ１
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkC1 As String = ""
        ''' <summary>
        ''' 予備Ｃ２
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkC2 As String = ""
        ''' <summary>
        ''' 予備Ｃ３
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkC3 As String = ""
        ''' <summary>
        ''' 予備Ｃ４
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkC4 As String = ""
        ''' <summary>
        ''' 予備Ｃ５
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkC5 As String = ""
        ''' <summary>
        ''' 予備Ｆ１
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkF1 As String = ""
        ''' <summary>
        ''' 予備Ｆ２
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkF2 As String = ""
        ''' <summary>
        ''' 予備Ｆ３
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkF3 As String = ""
        ''' <summary>
        ''' 予備Ｆ４
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkF4 As String = ""
        ''' <summary>
        ''' 予備Ｆ５
        ''' </summary>
        ''' <returns></returns>
        Public Property WorkF5 As String = ""

        ''' <summary>
        ''' 定形外の追加した費用か(0:定型,1:追加)
        ''' </summary>
        ''' <returns>削除ボタンの表示非表示に利用</returns>
        Public Property IsAddedCost As String = "0"
        ''' <summary>
        ''' 一意キーを格納（画面での削除を制御）
        ''' </summary>
        ''' <returns></returns>
        Public Property UniqueIndex As Integer = 0

    End Class

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
    ''' Int文字列を数字に変換
    ''' </summary>
    ''' <param name="intString"></param>
    ''' <returns></returns>
    Private Function IntStringToInt(intString As String) As Integer
        Dim tmpInt As Integer = 0
        If Integer.TryParse(intString.Replace(",", ""), tmpInt) Then
            Return tmpInt
        Else
            Return 0
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
    ''' GrossWeight変更時
    ''' </summary>
    Public Sub CalcSummaryGrossWeight()

        '画面の入力値をクラスに配置
        SaveGridItem()
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData Where costItemRow.OrderNo <> "").ToList
        '数値のカンマ編集
        For Each item In targetCostData
            If item.GrossWeight <> "" AndAlso IsNumeric(item.GrossWeight) Then
                item.GrossWeight = NumberFormat(DecimalStringToDecimal(item.GrossWeight), "", "#,##0.00")
            End If
        Next

        '絞り込んだリストを集計
        Dim summary As Decimal = targetCostData.Sum(Function(item) Decimal.Parse(item.GrossWeight))
        '合計欄に値表示
        Me.iptGrossSummary.Value = NumberFormat(summary, "", "#,##0.00")
        Me.hdnCurrentUnieuqIndex.Value = ""

        ViewState("COSTLIST") = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.OrderNo <> "" AndAlso allCostItem.TankNo <> "").ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

    End Sub
    ''' <summary>
    ''' NetWeight変更時
    ''' </summary>
    Public Sub CalcSummaryNetWeight()

        '画面の入力値をクラスに配置
        SaveGridItem()
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData Where costItemRow.OrderNo <> "").ToList

        '数値のカンマ編集
        For Each item In targetCostData
            If item.NetWeight <> "" AndAlso IsNumeric(item.NetWeight) Then

                item.NetWeight = NumberFormat(DecimalStringToDecimal(item.NetWeight), "", "#,##0.00")

                'Gross計算
                If item.TareWeight <> "" AndAlso IsNumeric(item.TareWeight) Then
                    item.GrossWeight = Convert.ToString((Decimal.Parse(item.NetWeight) + Decimal.Parse(item.TareWeight)))
                End If
            End If
        Next

        '絞り込んだリストを集計
        Dim summary As Decimal = targetCostData.Sum(Function(item) If(IsNumeric(item.NetWeight), Decimal.Parse(item.NetWeight), 0))
        '合計欄に値表示
        Me.iptNetSummary.Value = NumberFormat(summary, "", "#,##0.00")
        Me.hdnCurrentUnieuqIndex.Value = ""

        ViewState("COSTLIST") = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.OrderNo <> "" AndAlso allCostItem.TankNo <> "").ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        CalcSummaryGrossWeight()

    End Sub

    ''' <summary>
    ''' NoOfPackage変更時
    ''' </summary>
    Public Sub CalcSummaryNoOfPackage()

        '画面の入力値をクラスに配置
        SaveGridItem()
        Dim costData As List(Of COSTITEM) = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        'タブに合致し集計可能な費用情報のみに絞り込み
        Dim targetCostData = (From costItemRow In costData Where costItemRow.OrderNo <> "").ToList
        '数値のカンマ編集
        For Each item In targetCostData
            If item.NoOfPackage <> "" AndAlso IsNumeric(item.NoOfPackage) Then
                item.NoOfPackage = NumberFormat(DecimalStringToDecimal(item.NoOfPackage), "", "#,##0")
            End If
        Next

        '絞り込んだリストを集計
        Dim summary As Decimal = targetCostData.Sum(Function(item) If(IsNumeric(item.NoOfPackage), Decimal.Parse(item.NoOfPackage), 0))
        '合計欄に値表示
        Me.iptNoOfPackage.Value = NumberFormat(summary, "", "#,##0")
        Me.hdnCurrentUnieuqIndex.Value = ""

        ViewState("COSTLIST") = costData
        Dim showCostList = (From allCostItem In costData
                            Where allCostItem.OrderNo <> "" AndAlso allCostItem.TankNo <> "").ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

    End Sub

    ''' <summary>
    ''' 切り捨て関数
    ''' </summary>
    ''' <param name="value">値</param>
    ''' <param name="digits">IN：省略可能 省略時はセッション変数の対象桁数を取得</param>
    ''' <returns></returns>
    Private Function RoundDown(value As Decimal, Optional digits As Integer = Integer.MinValue) As Decimal

        If digits = Integer.MinValue Then
            Dim dec As String = ""
            'digits = 2 'セッション変数の桁数
            digits = GetDecimalPlaces(dec)
        End If
        Dim coef As Decimal = Convert.ToDecimal(System.Math.Pow(10, digits))
        If value > 0 Then
            Return System.Math.Floor(value * coef) / coef
        Else
            Return System.Math.Ceiling(value * coef) / coef
        End If
    End Function
    ''' <summary>
    ''' 書式変更関数
    ''' </summary>
    ''' <param name="value">書式を変更</param>
    ''' <param name="formatString">個別の書式がある場合は指定、未指定の場合はセッション変数の有効桁に従い小数表示を生成</param>
    ''' <returns></returns>
    Private Function NumberFormat(value As Object, countryCode As String, Optional formatString As String = "", Optional rateDec As String = "", Optional usdFlg As String = "") As String
        Dim strValue As String = Convert.ToString(value)
        strValue = strValue.Trim
        '渡された項目がブランクの場合はブランクのまま返却
        If strValue = "" Then
            Return ""
        End If

        Dim decValue As Decimal
        '渡された項目が数字にならない場合は引数のまま返却
        If Decimal.TryParse(strValue, decValue) = False Then
            Return strValue
        End If
        '数値書式の生成
        Dim retFormatString As String = formatString
        If formatString = "" Then

            Dim digits As Integer = 0
            If usdFlg = "" Then

                '桁数取得
                Dim dt As DataTable = Nothing
                Dim GBA00008Country As New GBA00008Country
                GBA00008Country.COUNTRYCODE = countryCode
                GBA00008Country.getCountryInfo()
                If GBA00008Country.ERR = C_MESSAGENO.NORMAL Then
                    dt = GBA00008Country.COUNTRY_TABLE
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00008Country.ERR)})
                    Return ""
                End If

                If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                    Return ""
                End If
                Dim dr As DataRow = dt.Rows(0)

                If rateDec = "" Then
                    digits = CInt(dr.Item("DECIMALPLACES"))
                Else
                    digits = CInt(dr.Item("RATEDECIMALPLACES"))
                End If

            Else
                Dim dec As String = ""
                'USD桁数取得
                digits = GetDecimalPlaces(dec)
                Select Case dec
                    Case "U"
                        decValue = RoundUp(decValue, CUInt(digits))
                    Case "D"
                        decValue = RoundDown(decValue, digits)
                    Case "R"
                        decValue = Round(decValue, CUInt(digits))
                    Case Else
                End Select
            End If

            If digits <= 0 Then
                retFormatString = "#,##0"
            Else
                retFormatString = "#,##0." & New String("0"c, digits)
            End If
        End If
        Return decValue.ToString(retFormatString)
    End Function
    '''' <summary>
    '''' 運賃諸費変更時
    '''' </summary>
    'Public Sub txtFreightCharges_Change()

    '    Try
    '        Me.lblFreightChargesText.Text = ""

    '        SetFrtAndCrgListItem()
    '        If Me.lbFrtAndCrg.Items.Count > 0 Then
    '            Dim findListItem = Me.lbFrtAndCrg.Items.FindByValue(Me.txtFreightCharges.Text)
    '            If findListItem IsNot Nothing Then
    '                Me.lblFreightChargesText.Text = findListItem.Text
    '            Else
    '                Dim findListItemUpper = Me.lbFrtAndCrg.Items.FindByValue(Me.txtFreightCharges.Text.ToUpper)
    '                If findListItemUpper IsNot Nothing Then
    '                    Me.lblFreightChargesText.Text = findListItemUpper.Text
    '                    Me.txtFreightCharges.Text = findListItemUpper.Value
    '                End If
    '            End If
    '        End If

    '    Catch ex As Exception
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.COA0003WriteLog()
    '    End Try
    'End Sub
    ''' <summary>
    ''' 発船社レート変更時
    ''' </summary>
    Public Sub txtShipRateEx_Change()

        If txtShipRateEx.Text <> "" Then
            If IsNumeric(txtShipRateEx.Text) Then
                txtShipRateEx.Text = NumberFormat(txtShipRateEx.Text, "", "#,##0.00")
            End If
        Else
            txtShipRateEx.Text = "0.00"
        End If

        SaveGridItem()
        Dim allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.OrderNo <> "" AndAlso allCostItem.TankNo <> "").ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        ViewState("COSTLIST") = showCostList

    End Sub
    ''' <summary>
    ''' 着船社レート変更時
    ''' </summary>
    Public Sub txtShipRateIn_Change()

        If txtShipRateIn.Text <> "" Then
            If IsNumeric(txtShipRateIn.Text) Then
                txtShipRateIn.Text = NumberFormat(txtShipRateIn.Text, "", "#,##0.00")
            End If
        Else
            txtShipRateIn.Text = "0.00"
        End If

        SaveGridItem()
        Dim allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.OrderNo <> "" AndAlso allCostItem.TankNo <> "").ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        ViewState("COSTLIST") = showCostList

    End Sub

    ''' <summary>
    ''' 切り上げ
    ''' </summary>
    ''' <param name="value">対象の数値</param>
    ''' <param name="decimalPlaces">有効小数桁数</param>
    ''' <returns>切り上げした数値</returns>
    Public Shared Function RoundUp(ByVal value As Decimal, ByVal decimalPlaces As UInt32) As Decimal
        Dim rate As Decimal = CDec(Math.Pow(10.0R, decimalPlaces))

        If value < 0 Then
            Return (Math.Ceiling(value * -1D * rate) / rate) * -1D
        Else
            Return Math.Ceiling(value * rate) / rate
        End If
    End Function

    ''' <summary>
    ''' 四捨五入
    ''' </summary>
    ''' <param name="value">対象の数値</param>
    ''' <param name="decimalPlaces">有効小数桁数</param>
    ''' <returns>四捨五入した数値</returns>
    Public Shared Function Round(ByVal value As Decimal, ByVal decimalPlaces As UInt32) As Decimal
        Return Math.Round(value, CInt(decimalPlaces), MidpointRounding.AwayFromZero)
    End Function

    ''' <summary>
    ''' USD桁数取得
    ''' </summary>
    Public Function GetDecimalPlaces(ByRef retDec As String) As Integer
        Dim retInt As Integer = 0

        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DECIMALPLACES"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            retInt = CInt(COA0017FixValue.VALUE1.Items(0).ToString)
            retDec = COA0017FixValue.VALUE2.Items(0).ToString
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If

        Return retInt

    End Function
    ''' <summary>
    ''' 運賃諸費リストアイテムを設定
    ''' </summary>
    Private Sub SetFrtAndCrgListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbFrtAndCrg.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "FREIGHT"
        COA0017FixValue.LISTBOX1 = Me.lbFrtAndCrg
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbFrtAndCrg = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            Return
        End If
    End Sub
    ''' <summary>
    ''' E or F リストアイテムを設定
    ''' </summary>
    Private Sub SetEorFListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbEorF.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "EORF"
        COA0017FixValue.LISTBOX1 = Me.lbEorF
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbEorF = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            Return
        End If
    End Sub
    ''' <summary>
    ''' 国リストアイテムを設定
    ''' </summary>
    Private Sub SetCountryListItem()

        Dim GBA00007OrganizationRelated As New GBA00007OrganizationRelated

        Try

            'リストクリア
            Me.lbCountry.Items.Clear()

            GBA00007OrganizationRelated.LISTBOX_COUNTRY = Me.lbCountry
            GBA00007OrganizationRelated.GBA00007getLeftListCountry()
            If GBA00007OrganizationRelated.ERR = C_MESSAGENO.NORMAL Then
                Me.lbCountry = DirectCast(GBA00007OrganizationRelated.LISTBOX_COUNTRY, ListBox)
            Else
                Return
            End If

        Catch ex As Exception
            Return
        End Try

    End Sub
    ''' <summary>
    ''' B/L Typeリストアイテムを設定
    ''' </summary>
    Private Sub SetBlTypeListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbBlType.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "BLTYPE"
        COA0017FixValue.LISTBOX1 = Me.lbBlType
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbBlType = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            Return
        End If
    End Sub

    ''' <summary>
    ''' Carrier B/L Typeリストアイテムを設定
    ''' </summary>
    Private Sub SetCarBlTypeListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbCarBlType.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "BLTYPE"
        COA0017FixValue.LISTBOX1 = Me.lbCarBlType
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbCarBlType = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            Return
        End If
    End Sub

    ''' <summary>
    ''' Carrierリストアイテムを設定
    ''' </summary>
    Private Sub SetCarrierListItem()

        Dim countryCode As String = ""
        Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
        countryCode = Me.hdnLoadCountry.Value
        Dim dt As DataTable = GetCarrier(countryCode)
        With Me.lbCarrier
            .DataSource = dt
            .DataTextField = "LISTBOXNAME"
            .DataValueField = "CODE"
            .DataBind()
            .Focus()
        End With
        '入力済のデータを選択状態にする
        If dblClickField IsNot Nothing AndAlso lbCarrier.Items IsNot Nothing Then
            Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
            Dim findLbValue As ListItem = lbCarrier.Items.FindByValue(dblClickFieldText.Text)
            If findLbValue IsNot Nothing Then
                findLbValue.Selected = True
            End If
        End If

    End Sub

    ''' <summary>
    ''' DEMU ACCTリストアイテムを設定
    ''' </summary>
    Private Sub SetDemAcctListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbDemAcct.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DEMUACCT"
        COA0017FixValue.LISTBOX1 = Me.lbDemAcct
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbDemAcct = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            Return
        End If
    End Sub

    ''' <summary>
    ''' 選択前の費用一覧の入力値を保持し、選択したタブに一致する費用情報を表示
    ''' </summary>
    Private Sub SetCostGridItem(Optional initFlg As Boolean = False)

        SaveGridItem(initFlg)
        Dim allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))
        Dim showCostList = (From allCostItem In allCostList
                            Where allCostItem.OrderNo <> "" AndAlso allCostItem.TankNo <> "").ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

        ViewState("COSTLIST") = showCostList

    End Sub

    ''' <summary>
    ''' 港検索
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <param name="portCode">港コード(オプショナル、未指定の場合は国に対する港全件)</param>
    ''' <returns>対象の港データテーブル</returns>
    ''' <remarks>GBM0002_PORTより引数条件に一致する港を検索、返却する</remarks>
    Private Function GetPort(countryCode As String, Optional portCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT PORTCODE")
        sqlStat.AppendLine("      ,AREANAME AS NAME")
        sqlStat.AppendLine("      ,PORTCODE + ':' + AREANAME AS LISTBOXNAME")
        sqlStat.AppendLine("  FROM GBM0002_PORT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If portCode <> "" Then
            sqlStat.AppendLine("   AND PORTCODE    = @PORTCODE")
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
            Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            Dim paramPortCode As SqlParameter = Nothing
            If portCode <> "" Then
                paramPortCode = sqlCmd.Parameters.Add("@PORTCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramCountryCode.Value = countryCode
            If portCode <> "" Then
                paramPortCode.Value = portCode
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
    ''' 左リストTERMの選択肢作成
    ''' </summary>
    Private Sub SetTermListItem()
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Me.lbTerm.Items.Clear()
        'Term選択肢
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "TERM"
        COA0017FixValue.LISTBOX1 = Me.lbTerm
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            Me.lbTerm = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
        Else
            Throw New Exception("Fix value getError")
        End If

    End Sub
    '''' <summary>
    '''' 船会社名称を画面に設定
    '''' </summary>
    '''' <param name="targetTextObject">対象テキスト</param>
    '''' <param name="carrierCode">船会社コード</param>
    'Private Sub SetDisplayShipLine(targetTextObject As TextBox, carrierCode As String)
    '    '一旦リセット
    '    targetTextObject.Text = carrierCode.Trim
    '    Dim targetLabel As Label = Me.lblShipLineText
    '    targetLabel.Text = ""
    '    '荷主コード（顧客コード）が未入力の場合はDBアクセスせずに終了
    '    If carrierCode.Trim = "" Then
    '        Return
    '    End If
    '    Dim countryCode As String = Me.txtLoadCountry.Text
    '    Dim dt As DataTable = GetCarrier(countryCode, carrierCode.Trim)
    '    'データが取れない場合はそのまま終了
    '    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
    '        Return
    '    End If
    '    Dim dr As DataRow = dt.Rows(0)
    '    targetLabel.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
    'End Sub
    ''' <summary>
    ''' 船会社情報取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <param name="carrierCode">船会社コード</param>
    ''' <returns>船会社情報一覧</returns>
    ''' <remarks>GBM0005_TRADERテーブルより船会社情報一覧を取得する</remarks>
    Private Function GetCarrier(countryCode As String, Optional carrierCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "NAMES"
        'If COA0019Session.LANGDISP <> C_LANG.JA Then
        '    textField = "NAMES"
        'End If
        'SQL文作成(TODO:ORGもキーだが今のところ未設定)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CARRIERCODE AS CODE")
        sqlStat.AppendFormat("      ,{0} AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,CARRIERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0005_TRADER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If carrierCode <> "" Then
            sqlStat.AppendLine("   AND CARRIERCODE    = @CARRIERCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND CLASS = '" & C_TRADER.CLASS.CARRIER & "'")
        sqlStat.AppendLine("ORDER BY CARRIERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            Dim paramCarrierCodeCode As SqlParameter = Nothing
            If carrierCode <> "" Then
                paramCarrierCodeCode = sqlCmd.Parameters.Add("@CARRIERCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramCountryCode.Value = countryCode
            If carrierCode <> "" Then
                paramCarrierCodeCode.Value = carrierCode
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
    ''' PaymentPlace名設定
    ''' </summary>
    Public Sub txtPaymentPlace_Change()

        Try
            Me.lblPaymentPlaceText.Text = ""

            SetCountryListItem()
            If Me.lbCountry.Items.Count > 0 Then
                Dim findListItem = Me.lbCountry.Items.FindByValue(Me.txtPaymentPlace.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblPaymentPlaceText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbCountry.Items.FindByValue(Me.txtPaymentPlace.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblPaymentPlaceText.Text = parts(1)
                        Me.txtPaymentPlace.Text = parts(0)
                    End If
                End If
            End If

        Catch ex As Exception
            Return
        End Try
    End Sub
    ''' <summary>
    ''' BlIssuePlace名設定
    ''' </summary>
    Public Sub txtBlIssuePlace_Change()

        Try
            Me.lblBlIssuePlaceText.Text = ""

            SetCountryListItem()
            If Me.lbCountry.Items.Count > 0 Then
                Dim findListItem = Me.lbCountry.Items.FindByValue(Me.txtBlIssuePlace.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblBlIssuePlaceText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbCountry.Items.FindByValue(Me.txtBlIssuePlace.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblBlIssuePlaceText.Text = parts(1)
                        Me.txtBlIssuePlace.Text = parts(0)
                    End If
                End If
            End If

        Catch ex As Exception
            Return
        End Try
    End Sub
    ''' <summary>
    ''' AnIssuePlace名設定
    ''' </summary>
    Public Sub txtAnIssuePlace_Change()

        Try
            Me.lblAnIssuePlaceText.Text = ""

            SetCountryListItem()
            If Me.lbCountry.Items.Count > 0 Then
                Dim findListItem = Me.lbCountry.Items.FindByValue(Me.txtAnIssuePlace.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblAnIssuePlaceText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbCountry.Items.FindByValue(Me.txtAnIssuePlace.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblAnIssuePlaceText.Text = parts(1)
                        Me.txtAnIssuePlace.Text = parts(0)
                    End If
                End If
            End If

        Catch ex As Exception
            Return
        End Try
    End Sub
    ''' <summary>
    ''' BL Type名設定
    ''' </summary>
    Public Sub txtBlType_Change()

        Try
            Me.lblBlTypeText.Text = ""

            SetBlTypeListItem()
            If Me.lbBlType.Items.Count > 0 Then
                Dim findListItem = Me.lbBlType.Items.FindByValue(Me.txtBlType.Text)
                If findListItem IsNot Nothing Then
                    Me.lblBlTypeText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbBlType.Items.FindByValue(Me.txtBlType.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblBlTypeText.Text = findListItemUpper.Text
                        Me.txtBlType.Text = findListItemUpper.Value
                    End If
                End If
            End If

        Catch ex As Exception
            Return
        End Try
    End Sub
    ''' <summary>
    ''' Carrier BL Type名設定
    ''' </summary>
    Public Sub txtCarBlType_Change()

        Try
            Me.lblCarBlTypeText.Text = ""

            SetCarBlTypeListItem()
            If Me.lbCarBlType.Items.Count > 0 Then
                Dim findListItem = Me.lbCarBlType.Items.FindByValue(Me.txtCarBlType.Text)
                If findListItem IsNot Nothing Then
                    Me.lblCarBlTypeText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbCarBlType.Items.FindByValue(Me.txtCarBlType.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblCarBlTypeText.Text = findListItemUpper.Text
                        Me.txtCarBlType.Text = findListItemUpper.Value
                    End If
                End If
            End If

        Catch ex As Exception
            Return
        End Try
    End Sub
    ''' <summary>
    ''' No of B/L英語変換
    ''' </summary>
    Public Sub txtNoOfBl_Change()

        Try
            If Me.txtNoOfBl.Text <> "" AndAlso IsNumeric(Me.txtNoOfBl.Text) Then

                Dim COA0035Convert As New BASEDLL.COA0035Convert
                Dim cnvStr As String = Nothing
                Dim numStr As String = Me.txtNoOfBl.Text
                COA0035Convert.I_CONVERT = Me.txtNoOfBl.Text
                COA0035Convert.I_CLASS = "CONVERT"
                COA0035Convert.COA0035convNumToEng()
                If COA0035Convert.O_ERR = C_MESSAGENO.NORMAL Then
                    cnvStr = COA0035Convert.O_CONVERT1
                    Me.txtNoOfBl.Text = cnvStr & " (" & numStr & ")"
                Else
                    Throw New Exception("Fix value getError")
                End If

            End If

        Catch ex As Exception
            Return
        End Try
    End Sub
    ''' <summary>
    ''' Carrier名設定
    ''' </summary>
    Public Sub txtCarrier_Change()

        SetCarrierListItem()
        Dim carrierCode As String = Me.txtCarrier.Text.Trim
        Me.txtCarrier.Text = carrierCode
        Me.lblCarrierText.Text = ""
        If carrierCode <> "" Then
            SetDisplayCarrier(Me.txtCarrier, carrierCode)
        End If
    End Sub
    ''' <summary>
    ''' 選択（入力）したEmptyOrFull区分を画面に設定
    ''' </summary>
    Private Sub SetDisplayEmptyOrFull(ByVal EmptyOrFull As String)

        '入力内容保持
        SaveGridItem()

        Dim allCostList As List(Of COSTITEM)
        allCostList = DirectCast(ViewState("COSTLIST"), List(Of COSTITEM))

        Dim uniqueIndex As Integer = 0
        Integer.TryParse(Me.hdnCurrentUnieuqIndex.Value, uniqueIndex)
        Dim changeCostList = (From allCostItem In allCostList
                              Where allCostItem.UniqueIndex = uniqueIndex).ToList

        If changeCostList IsNot Nothing AndAlso changeCostList.Count > 0 Then

            '一旦リセット
            changeCostList(0).EmptyOrFull = EmptyOrFull.Trim
        End If

        ViewState("COSTLIST") = allCostList
        Dim showCostList = (From allCostItem In allCostList).ToList
        Me.gvDetailInfo.DataSource = showCostList
        Me.gvDetailInfo.DataBind()

    End Sub
    ''' <summary>
    ''' 風袋重量取得
    ''' </summary>
    ''' <param name="tankNo">タンク番号</param>
    ''' <returns>タンク情報一覧</returns>
    Private Function GetTank(tankNo As String) As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT NETWEIGHT AS TAREWEIGHT")
        sqlStat.AppendLine("      ,TANKCAPACITY AS TANKCAPACITY")
        sqlStat.AppendLine("  FROM GBM0006_TANK")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND TANKNO      = @TANKNO")
        sqlStat.AppendLine("   AND STYMD      <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD     >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG     <> @DELFLG")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            Dim paramTankNo As SqlParameter = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar, 20)
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp")
            paramTankNo.Value = tankNo
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
    ''' No of Package英語変換
    ''' </summary>
    Public Function GetConvEng() As String
        Dim retVal As String = ""

        If Me.iptNoOfPackage.Value <> "" AndAlso IsNumeric(Me.iptNoOfPackage.Value) Then

            Dim COA0035Convert As New BASEDLL.COA0035Convert
            Dim cnvStr As String = Nothing
            Dim numStr As String = Me.iptNoOfPackage.Value
            COA0035Convert.I_CONVERT = Me.iptNoOfPackage.Value
            COA0035Convert.I_CLASS = "CONVERT"
            COA0035Convert.COA0035convNumToEng()
            If COA0035Convert.O_ERR = C_MESSAGENO.NORMAL Then
                cnvStr = COA0035Convert.O_CONVERT1
                retVal = cnvStr & " (" & numStr & ")"
            Else
                Throw New Exception("Fix value getError")
            End If

        End If

        Return retVal

    End Function

    ''' <summary>
    ''' アップロードチェック
    ''' </summary>
    Private Sub uploadChk(ByRef retMsg As String, ByVal currentTab As COSTITEM.CostItemGroup)

        retMsg = C_MESSAGENO.NORMAL

        '数量チェック
        Dim quantity As String = GetUploadList("QUANTITY")
        Dim sumQua As Integer = 0
        Dim mCnt As Integer = 0
        Dim files As String() = System.IO.Directory.GetFiles(COA0019Session.UPLOADDir & "\" & COA0019Session.USERID, "*.*")

        mCnt = 0

        If currentTab = COSTITEM.CostItemGroup.FileUp Then

            For Each tempFile As String In files

                For i As Integer = 0 To dViewRep.Items.Count - 1

                    If DirectCast(dViewRep.Items(i).FindControl("lblRepFileName"), System.Web.UI.WebControls.Label).Text = System.IO.Path.GetFileName(tempFile) Then
                        mCnt += 1
                    End If
                Next
            Next

            sumQua = dViewRep.Items.Count + files.Count - mCnt

        End If

        If sumQua > CInt(quantity) Then
            retMsg = C_MESSAGENO.TOOMANYUPLOADFILES
            Return
        End If

        '拡張子チェック
        Dim extension As String = GetUploadList("EXTENSION")
        Dim splExtens As String()
        splExtens = Split(extension, ",")

        For Each tempFile As String In files

            'ディレクトリ付ファイル名より、ファイル名編集
            Dim DirFile As String = tempFile
            Dim preFlg As Boolean = False
            Dim wkExt As String = ""
            For Each ext As String In splExtens
                wkExt = System.IO.Path.GetExtension(DirFile)
                If ext = wkExt OrElse ext.ToUpper = wkExt.ToUpper Then
                    preFlg = True
                End If
            Next

            If Not preFlg Then
                retMsg = C_MESSAGENO.INCORRECTFILETYPE
                Return
            End If
        Next

        Return

    End Sub

    ''' <summary>
    ''' アップロード情報取得
    ''' </summary>
    Public Function GetUploadList(ByVal itmKey As String) As String
        Dim retVal As String = ""

        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "UPLOAD"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            For i As Integer = 0 To COA0017FixValue.VALUE1.Items.Count - 1
                If itmKey = COA0017FixValue.VALUE1.Items(i).Value Then
                    retVal = COA0017FixValue.VALUE1.Items(i).Text
                End If
            Next
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
        End If

        Return retVal

    End Function

    ''' <summary>
    ''' 削除フラグリストアイテムを設定
    ''' </summary>
    Private Sub SetDelFlgListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get

        'リストクリア
        Me.lbDelFlg.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DELFLG"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbDelFlg
        Else
            COA0017FixValue.LISTBOX2 = Me.lbDelFlg
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbDelFlg = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbDelFlg = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

        Else

            Return
        End If

    End Sub

    ''' <summary>
    ''' 船会社名称を画面に設定
    ''' </summary>
    ''' <param name="targetTextObject">対象テキスト</param>
    ''' <param name="carrierCode">船会社コード</param>
    Private Sub SetDisplayCarrier(targetTextObject As TextBox, carrierCode As String)
        '一旦リセット
        targetTextObject.Text = carrierCode.Trim
        Dim targetLabel As Label = Me.lblCarrierText
        targetLabel.Text = ""
        '荷主コード（顧客コード）が未入力の場合はDBアクセスせずに終了
        If carrierCode.Trim = "" Then
            Return
        End If
        Dim countryCode As String = Me.hdnLoadCountry.Value
        Dim dt As DataTable = GetCarrier(countryCode, carrierCode.Trim)
        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)
        targetLabel.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
    End Sub

    '''' <summary>
    '''' Carrier初期値設定
    '''' </summary>
    'Private Sub SetCarrierCode()

    '    If Me.txtCarrier.Text = "" Then
    '        Dim crDt As DataTable = GetOrdValContractor(Me.lblOrderNo.Text)
    '        If crDt.Rows.Count > 0 Then
    '            Me.txtCarrier.Text = Convert.ToString(crDt.Rows(0).Item("CONTRACTOR"))
    '        End If
    '    End If

    'End Sub

    '''' <summary>
    '''' オーダー明細業者取得
    '''' </summary>
    '''' <param name="orderNo">受注番号</param>
    '''' <returns>オーダー明細業者一覧</returns>
    'Private Function GetOrdValContractor(orderNo As String) As DataTable
    '    Dim retDt As New DataTable   '戻り値用のデータテーブル
    '    'SQL文作成
    '    Dim sqlStat As New StringBuilder
    '    sqlStat.AppendLine("SELECT CONTRACTORFIX AS CONTRACTOR")
    '    sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
    '    sqlStat.AppendLine(" WHERE ORDERNO        = @ORDERNO")
    '    sqlStat.AppendLine("   AND ACTIONID      IN ('SHIP','RPEC','RPED','RPHC','RPHD')")
    '    sqlStat.AppendLine("   AND CONTRACTORFIX <> ''")
    '    sqlStat.AppendLine("   AND DELFLG        <> @DELFLG")
    '    sqlStat.AppendLine("   AND DELFLG        <> @DELFLG")
    '    sqlStat.AppendLine("   Order By ACTUALDATE")

    '    'DB接続
    '    Using sqlCon As New SqlConnection(COA0019Session.DBcon),
    '          sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

    '        sqlCon.Open() '接続オープン
    '        'SQLパラメータ設定
    '        Dim paramOrderNo As SqlParameter = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar)
    '        Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
    '        'SQLパラメータ値セット
    '        paramOrderNo.Value = orderNo
    '        paramDelFlg.Value = CONST_FLAG_YES
    '        Using sqlDa As New SqlDataAdapter(sqlCmd)
    '            sqlDa.Fill(retDt)
    '        End Using
    '    End Using
    '    Return retDt
    'End Function

    ''' <summary>
    ''' 初期表示時のデータと比較し変更データがあるか確認
    ''' </summary>
    ''' <param name="screenData">基本情報(OderBase)と詳細情報(OderValue2)のデータセット</param>
    ''' <returns>True:変更データあり,False:変更データなし</returns>
    Private Function HasModifiedData(screenData As DataSet) As Boolean
        Dim dsInit As DataSet = DirectCast(ViewState("INIT_COSTDATASET"), DataSet)

        If screenData Is Nothing OrElse dsInit Is Nothing Then
            Throw New Exception("B/L CheckDataSet not exists")
        End If

        'カラムリストの生成
        Dim dicColList As New Dictionary(Of String, List(Of String))
        'BASE部分のカラムリスト
        Dim colList As New List(Of String) From {"ORDERNO", "PRODUCTNAME", "FREIGHTANDCHARGES", "MARKSANDNUMBERS"}
        If Me.hdnWhichTrans.Value = "1" Then
            colList.AddRange({"CARRIER1", "VSL1", "VOY1", "NOTIFYCONTTEXT1",
                                 "LDNVSL1", "LDNPOL1", "LDNDATE1", "LDNBY1", "BOOKINGNO",
                                 "SHIPPERTEXT", "CONSIGNEETEXT", "NOTIFYTEXT", "PRECARRIAGETEXT",
                                 "FINDESTINATIONTEXT", "DECLAREDVALUE", "REVENUETONS", "RATE",
                                 "PER", "PREPAID", "COLLECT", "CARRIERBLNO", "NOOFBL", "BLTYPE",
                                 "PAYMENTPLACE", "BLISSUEPLACE", "ANISSUEPLACE", "MEASUREMENT",
                                 "CARRIERBLTYPE", "DEMUFORACCT", "GOODSPKGS",
                                 "BLRECEIPT1", "BLLOADING1", "BLDISCHARGE1", "BLDELIVERY1", "BLPLACEDATEISSUE1",
                                 "TRANSIT1VSL1", "TRANSIT1VOY1", "TRANSIT2VSL1", "TRANSIT2VOY1", "TRANSIT1VSL2", "TRANSIT1VOY2", "TRANSIT2VSL2", "TRANSIT2VOY2"})
        Else
            colList.AddRange({"CARRIER2", "VSL2", "VOY2", "NOTIFYCONTTEXT2",
                                 "LDNVSL2", "LDNPOL2", "LDNDATE2", "LDNBY2", "BOOKINGNO2",
                                 "SHIPPERTEXT2", "CONSIGNEETEXT2", "NOTIFYTEXT2", "PRECARRIAGETEXT2",
                                 "FINDESTINATIONTEXT2", "DECLAREDVALUE2", "REVENUETONS2", "RATE2",
                                 "PER2", "PREPAID2", "COLLECT2", "CARRIERBLNO2", "NOOFBL2", "BLTYPE2",
                                 "PAYMENTPLACE2", "BLISSUEPLACE2", "ANISSUEPLACE2", "MEASUREMENT2",
                                 "CARRIERBLTYPE2", "DEMUFORACCT2",
                                 "BLRECEIPT2", "BLLOADING2", "BLDISCHARGE2", "BLDELIVERY2", "BLPLACEDATEISSUE2"})
        End If
        dicColList.Add("ORDER_BASE", colList)
        'VALUE部分のカラムリスト
        colList = New List(Of String) From {"ORDERNO", "TANKNO", "TANKSEQ", "TANKTYPE",
                                               "SEALNO1", "SEALNO2", "SEALNO3", "SEALNO4",
                                               "NETWEIGHT", "EMPTYORFULL",
                                               "NOOFPACKAGE", "EXSHIPRATE", "INSHIPRATE", "TAREWEIGHT", "DISPSEQ"}
        dicColList.Add("ORDER_VALUE", colList)

        For Each tableName In {"ORDER_BASE", "ORDER_VALUE"}
            '初期状態または画面編集後の情報格納データテーブルが存在しない場合
            If Not dsInit.Tables.Contains(tableName) OrElse
               Not screenData.Tables.Contains(tableName) Then
                Throw New Exception(String.Format("B/L CheckDataTable not exists:TableName={0}", tableName))
            End If
            Dim dtInit As DataTable = Nothing
            If tableName = "ORDER_VALUE" AndAlso dsInit.Tables(tableName).Rows.Count > 0 Then
                dtInit = dsInit.Tables(tableName).Clone()
                Dim dtInitq = (From item As DataRow In dsInit.Tables(tableName) Where Convert.ToString(item("TANKNO")) <> "")
                If dtInitq.Any = True Then
                    dtInit = dtInitq.CopyToDataTable
                End If
            Else
                dtInit = dsInit.Tables(tableName)
            End If
            Dim dtDisp As DataTable = screenData.Tables(tableName)

            'レコード数不一致
            If dtInit.Rows.Count <> dtDisp.Rows.Count Then
                Return True
            End If
            '件数が0件の場合次のテーブルチェック
            If screenData.Tables(tableName).Rows.Count = 0 Then
                Continue For
            End If

            '列名をリストに格納
            Dim colNames As List(Of String) = (From col As DataColumn _
                                                 In dsInit.Tables(tableName).Columns.Cast(Of DataColumn)
                                               Select Convert.ToString(col.ColumnName)).ToList

            '行ループ
            Dim rowCnt As Integer = dtInit.Rows.Count - 1
            For rowIdx = 0 To rowCnt
                Dim drInit As DataRow = dtInit.Rows(rowIdx)
                Dim drDisp As DataRow = dtDisp.Rows(rowIdx)
                'カラム名をループし全項目比較
                For Each colName In dicColList(tableName)
                    If {"NETWEIGHT", "EXSHIPRATE", "INSHIPRATE"}.Contains(colName) AndAlso
                       IsNumeric(drDisp.Item(colName)) Then
                        drDisp.Item(colName) = Convert.ToString(Convert.ToDouble(drDisp.Item(colName)))
                    End If
                    'カラムに1件でも変更データがあれば変更ありで終了
                    Dim valInit As String = Convert.ToString(drInit.Item(colName)).Replace(ControlChars.CrLf, ControlChars.Lf).Replace(ControlChars.Cr, ControlChars.Lf)
                    Dim valDisp As String = Convert.ToString(drDisp.Item(colName)).Replace(ControlChars.CrLf, ControlChars.Lf).Replace(ControlChars.Cr, ControlChars.Lf)
                    If valInit.Equals(valDisp) = False Then
                        Return True
                    End If
                Next 'カラム名ループEnd
            Next 'rowIdx 行ループEnd
        Next 'テーブル名ループEnd
        'ファイル一覧の変更チェック
        Dim dtInitFile As DataTable = dsInit.Tables("FILEINFO")
        If dViewRep.Items.Count <> dtInitFile.Rows.Count Then
            Return True
        End If
        'データレコードのチェック
        If dtInitFile.Rows.Count > 0 Then
            For i As Integer = 0 To Me.dViewRep.Items.Count - 1
                Dim drInitFile As DataRow = dtInitFile.Rows(i)
                Dim repItem As RepeaterItem = Me.dViewRep.Items(i)
                Dim dispFileName As String = DirectCast(repItem.FindControl("lblRepFileName"), Label).Text
                Dim dispFilePath As String = DirectCast(repItem.FindControl("lblRepFilePath"), Label).Text
                Dim dispFileDeleteFlg As String = DirectCast(repItem.FindControl("txtRepDelFlg"), TextBox).Text
                If Not (drInitFile("FILENAME").Equals(dispFileName) AndAlso
                        drInitFile("FILEPATH").Equals(dispFilePath) AndAlso
                        drInitFile("DELFLG").Equals(dispFileDeleteFlg)) Then
                    Return True
                End If
            Next
        End If
        'ここまでくれば変更なし
        Return False

    End Function
    ''' <summary>
    ''' 改行判定処理
    ''' </summary>
    ''' <returns></returns>
    Private Function indentionCheck(ByVal chara As Integer, ByVal line As Integer, ByVal textVal As String) As Boolean

        Dim retBool As Boolean = False
        Dim retVal As Integer = 0
        Dim cnt As Integer = 0

        Dim consText As String() = Nothing

        consText = Split(textVal, vbCrLf)

        For Each cons In consText

            If cons = "" Then
                cnt = 1
            Else
                cnt = Convert.ToInt16(Math.Ceiling(cons.Count / chara))
            End If

            retVal += cnt

        Next

        If line < retVal Then
            retBool = True
        End If

        Return retBool
    End Function
    ''' <summary>
    ''' B/L番号をシーケンスより取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetBLNo(Optional ByRef sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim blNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT  'JOT' ")
            sqlStat.AppendLine("      + right(left(convert(char,getdate(),12),2),1)")
            sqlStat.AppendLine("      + 'A'")
            sqlStat.AppendLine("      + right('00000' + trim(convert(char,NEXT VALUE FOR " & C_SQLSEQ.BL & ")),5)")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get new B/L No. error")
                    End If

                    blNo = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return blNo
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Close()
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try

    End Function
    ''' <summary>
    ''' B/L No.更新
    ''' </summary>
    Private Function SetBlNo(ByRef ds As DataSet) As Boolean
        Dim dt As DataTable = ds.Tables("ORDER_BASE")
        Dim dr As DataRow = dt.Rows(0)

        If (hdnWhichTrans.Value = "1" AndAlso Convert.ToString(dr.Item("BLID1")) = "") OrElse
            (hdnWhichTrans.Value = "2" AndAlso Convert.ToString(dr.Item("BLID2")) = "") Then

            Dim blno As String = GetBLNo()

            If hdnWhichTrans.Value = "1" Then
                dr.Item("BLID1") = blno
            Else
                dr.Item("BLID2") = blno
            End If

            'DB登録処理実行
            Dim errFlg As Boolean = True
            EntryData(ds, errFlg)
            If Not errFlg Then
                Return False
            End If

            Dim dsNew As DataSet = New DataSet
            If GetPrevDisplayInfo(dsNew) <> C_MESSAGENO.NORMAL Then
                Return False
            End If

            ds = dsNew

            '一覧を変更可能な一時リスト変数に可能
            Dim valList As List(Of COSTITEM) = Me.CreateTemporaryInfoList(ds.Tables("ORDER_VALUE"))
            'VIEWSTATEに情報を保存
            ViewState("COSTLIST") = valList

        End If

        Return True
    End Function

End Class