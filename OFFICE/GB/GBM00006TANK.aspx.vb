Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net
Imports BASEDLL

''' <summary>
''' タンクマスタ画面クラス
''' </summary>
Public Class GBM00006TANK
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00006"    '自身のMAPID
    Private Const CONST_BASEDATATABLE = "GBM00006TBL"
    Private Const CONST_INPDATATABLE = "GBM00006INPTBL"
    Private Const CONST_UPDDATATABLE = "GBM00006UPDTBL"
    Private Const CONST_PDFDATATABLE = "GBM00006PDFTBL"
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_TBLMASTER = "GBM0006_TANK"
    Private Const CONST_TBLAPPLY = "GBM0022_TANKAPPLY"
    Private Const CONST_TBLORDERB = "GBT0004_ODR_BASE"
    Private Const CONST_TBLORDERV = "GBT0005_ODR_VALUE"
    Private Const CONST_EVENTCODE = "MasterApplyTank"

    Private Const CONST_NEWYMD = "2019/04/01"
    Private Const CONST_NEWUSER = "SYSTEM"
    Private Const CONST_NEWDISPSEQ = "4"
    Private Const CONST_NEWACTUAL = "2019/04/30"

    Dim errListAll As List(Of String)                   'インポート全体のエラー
    Dim errList As List(Of String)                      'インポート中の１セット分のエラー
    Private returnCode As String = String.Empty         'サブ用リターンコード
    Private PDFrow As DataRow
    Dim errDisp As String = Nothing                     'エラー用表示文言
    Dim updateDisp As String = Nothing                  '更新用表示文言
    Private dicField As Dictionary(Of String, String) = Nothing

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile

    ''' <summary>
    ''' 一覧格納用テーブル
    ''' </summary>
    Private BASEtbl As DataTable
    ''' <summary>
    ''' チェック用テーブル
    ''' </summary>
    Private INPtbl As DataTable
    ''' <summary>
    ''' デフォルト用テーブル
    ''' </summary>
    Private UPDtbl As DataTable
    ''' <summary>
    ''' PDF用テーブル
    ''' </summary>
    Private PDFtbl As DataTable
    ''' <summary>
    ''' カラム情報用テーブル
    ''' </summary>
    Private COLtbl As DataTable
    ''' <summary>
    ''' 行のロウデータ
    ''' </summary>
    Private WORKrow As DataRow
    Public Property Gbt00006items As GBT00006RESULT.GBT00006RITEMS
    Public Property Gbt00012items As GBT00012REPAIR.GBT00012RITEMS


    ''' <summary>
    ''' ページロード時処理
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            COA0003LogFile = New COA0003LogFile                             'ログ出力
            Dim COA0007getCompanyInfo As New BASEDLL.COA0007CompanyInfo     '会社情報取得
            Dim COA0021ListTable As New BASEDLL.COA0021ListTable
            Dim COA0031ProfMap As New BASEDLL.COA0031ProfMap

            '****************************************
            '初回ロード時・ポストバック両方で必要な処理
            '****************************************

            '画面モード（更新・参照）設定
            If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) = "2" Then
                hdnMAPpermitCode.Value = "TRUE"
            Else
                hdnMAPpermitCode.Value = "FALSE"
            End If

            'リターンコード設定
            returnCode = C_MESSAGENO.NORMAL

            '作業用データベース設定
            BASEtbl = New DataTable(CONST_BASEDATATABLE)
            INPtbl = New DataTable(CONST_INPDATATABLE)
            UPDtbl = New DataTable(CONST_UPDDATATABLE)
            PDFtbl = New DataTable(CONST_PDFDATATABLE)
            COLtbl = New DataTable

            '表示用文言判定
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errDisp = "ERROR"
                updateDisp = "UPDATE"
            Else
                errDisp = "エラー"
                updateDisp = "更新"
            End If

            '****************************************
            'メッセージ初期化
            '****************************************
            Me.lblFooterMessage.Text = ""
            Me.lblFooterMessage.ForeColor = Color.Black
            Me.lblFooterMessage.Font.Bold = False

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                Me.hdnThisMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = COA0019Session.XMLDir & "\" & Date.Now.ToString("yyyyMMdd") & "-" & COA0019Session.USERID & "-" & CONST_MAPID & "-" & Me.hdnThisMapVariant.Value & "-" & Date.Now.ToString("HHmmss") & ".txt"
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
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
                '表示非表示制御
                '****************************************
                VisibleControls()
                '****************************************
                '選択情報　設定処理
                '****************************************
                '右Boxへの値設定
                RightboxInit()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                    Return
                End If

                '一覧表示項目取得
                GetListData()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                    Return
                End If

                '一覧表示データ保存
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = BASEtbl
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                    Return
                End If

                '一覧表示データ編集（性能対策）
                Dim COA0013TableObject As New COA0013TableObject
                Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(BASEtbl, Me.pnlListArea, CONST_DSPROWCOUNT, 1, hdnListPosition)

                With COA0013TableObject
                    .MAPID = CONST_MAPID
                    .VARI = Me.hdnViewId.Value
                    .SRCDATA = listData
                    .TBLOBJ = pnlListArea
                    .SCROLLTYPE = "2"
                    .LEVENT = "ondblclick"
                    .LFUNC = "ListDbClick"
                    .TITLEOPT = True
                    .USERSORTOPT = 1
                End With
                COA0013TableObject.COA0013SetTableObject()

                '****************************************
                'Detail初期設定
                '****************************************
                detailboxInit()
                hdnDTABChange.Value = "0"
                DetailTABChange()
                hdnDTABChange.Value = ""
                '****************************************
                'PDFタブ初期処理
                '****************************************
                PDFInitDel()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                txtOperationEx.Focus()

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
                '**********************
                ' ファイルアップロード処理
                '**********************
                If Me.hdnListUpload.Value IsNot Nothing AndAlso Me.hdnListUpload.Value <> "" Then
                    If Me.hdnListUpload.Value = "XLS_LOADED" Then
                        UploadExcel()
                    ElseIf Me.hdnListUpload.Value = "PDF_LOADED" Then
                        UploadPDF()
                    End If

                    Me.hdnListUpload.Value = ""
                End If
                '**********************
                ' 一覧ダブルクリック処理
                '**********************
                If Me.hdnListDbClick.Value IsNot Nothing AndAlso Me.hdnListDbClick.Value <> "" Then
                    List_DBclick()
                    Me.hdnListDbClick.Value = ""
                End If
                '**********************
                ' 明細タブ切り替え
                '**********************
                If Me.hdnDTABChange.Value IsNot Nothing AndAlso Me.hdnDTABChange.Value <> "" Then
                    DetailTABChange()
                    Me.hdnDTABChange.Value = ""
                End If
                '**********************
                ' Detail PFD内容表示処理
                '**********************
                If Me.hdnDTABPDFEXCELdisplay.Value IsNot Nothing AndAlso Me.hdnDTABPDFEXCELdisplay.Value <> "" Then
                    DTABPDFdisplay()
                    hdnDTABPDFEXCELdisplay.Value = ""
                End If
                '**********************
                ' スクロール処理 
                '**********************
                ListScrole()
                hdnMouseWheel.Value = ""
            End If
            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定

            '****************************************
            'Close処理
            '****************************************
            BASEtbl.Dispose()
            BASEtbl = Nothing
            INPtbl.Dispose()
            INPtbl = Nothing
            UPDtbl.Dispose()
            UPDtbl = Nothing
            PDFtbl.Dispose()
            PDFtbl = Nothing
            COLtbl.Dispose()
            COLtbl = Nothing

        Catch ex As Threading.ThreadAbortException
            Return
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.EXCEPTION
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = messageNo
            COA0003LogFile.COA0003WriteLog()

            Return
        Finally
            '****************************************
            'Close処理
            '****************************************
            If BASEtbl IsNot Nothing Then
                BASEtbl.Dispose()
                BASEtbl = Nothing
            End If
            If INPtbl IsNot Nothing Then
                INPtbl.Dispose()
                INPtbl = Nothing
            End If
            If UPDtbl IsNot Nothing Then
                UPDtbl.Dispose()
                UPDtbl = Nothing
            End If
            If PDFtbl IsNot Nothing Then
                PDFtbl.Dispose()
                PDFtbl = Nothing
            End If
            If COLtbl IsNot Nothing Then
                COLtbl.Dispose()
                COLtbl = Nothing
            End If

        End Try
    End Sub
    ''' <summary>
    ''' Rightbox初期化
    ''' </summary>
    Private Sub RightboxInit()
        'RightBOX情報設定
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        returnCode = C_MESSAGENO.NORMAL

        '初期化
        Me.txtRightErrorMessage.Text = ""

        'レポートID情報
        COA0022ProfXls.MAPID = CONST_MAPID
        COA0022ProfXls.COA0022getReportId()
        If COA0022ProfXls.ERR = C_MESSAGENO.NORMAL Then
            Try
                For i As Integer = 0 To DirectCast(COA0022ProfXls.REPORTOBJ, System.Web.UI.WebControls.ListBox).Items.Count - 1
                    lbRightList.Items.Add(New ListItem(DirectCast(COA0022ProfXls.REPORTOBJ, System.Web.UI.WebControls.ListBox).Items(i).Text, DirectCast(COA0022ProfXls.REPORTOBJ, System.Web.UI.WebControls.ListBox).Items(i).Value))
                Next
            Catch ex As Exception
            End Try
        Else
            returnCode = COA0022ProfXls.ERR
            Return
        End If

        'レポートID変数検索
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnViewId.Value
        COA0016VARIget.FIELD = "REPORTID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR <> C_MESSAGENO.NORMAL Then
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
    ''' 一覧データ取得
    ''' </summary>
    Protected Sub GetListData()
        COA0003LogFile = New COA0003LogFile                         'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort  'テーブルソート文字列取得
        returnCode = C_MESSAGENO.NORMAL

        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        '画面表示用データ取得
        'ユーザマスタ（申請）内容検索
        Try
            'ソート順取得
            COA0020ProfViewSort.MAPID = CONST_MAPID
            COA0020ProfViewSort.VARI = Me.hdnViewId.Value
            COA0020ProfViewSort.TAB = ""
            COA0020ProfViewSort.COA0020getProfViewSort()

            'テーブル検索結果をテーブル退避
            BASEtblColumnsAdd(BASEtbl)

            'DataBase接続文字
            SQLcon = New SqlConnection(COA0019Session.DBcon)
            SQLcon.Open() 'DataBase接続(Open)

            '検索SQL文
            SQLStr =
                "SELECT " _
                & "ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") as LINECNT , " _
                & "       '' as OPERATION , " _
                & "       TIMSTP , " _
                & "       '1' as 'SELECT'                    , " _
                & "       '0' as HIDDEN                      , " _
                & "       APPLYID                            , " _
                & "       COMPCODE                           , " _
                & "       TANKNO                             , " _
                & "       STYMD                              , " _
                & "       ENDYMD                             , " _
                & "       PROPERTY                           , " _
                & "       LMOF                               , " _
                & "       LEASESTAT                          , " _
                & "       REPAIRSTAT                         , " _
                & "       CASE WHEN INSPECTDATE5 = '1900/01/01' THEN '' ELSE INSPECTDATE5 END AS INSPECTDATE5 , " _
                & "       CASE WHEN INSPECTDATE2P5 = '1900/01/01' THEN '' ELSE INSPECTDATE2P5 END AS INSPECTDATE2P5 , " _
                & "       CASE WHEN NEXTINSPECTDATE = '1900/01/01' THEN '' ELSE NEXTINSPECTDATE END AS NEXTINSPECTDATE , " _
                & "       NEXTINSPECTTYPE                    , " _
                & "       JAPFIREAPPROVED                    , " _
                & "       MANUFACTURER                       , " _
                & "       MANUFACTURESERIALNO                , " _
                & "       CASE WHEN DATEOFMANUFACTURE = '1900/01/01' THEN '' ELSE DATEOFMANUFACTURE END AS DATEOFMANUFACTURE , " _
                & "       MATERIAL                           , " _
                & "       STRUCT                             , " _
                & "       USDOTAPPROVED                      , " _
                & "       NOMINALCAPACITY                    , " _
                & "       TANKCAPACITY                       , " _
                & "       MAXGROSSWEIGHT                     , " _
                & "       NETWEIGHT                          , " _
                & "       FREAMDIMENSION_H                   , " _
                & "       FREAMDIMENSION_W                   , " _
                & "       FREAMDIMENSION_L                   , " _
                & "       HEATING                            , " _
                & "       HEATING_SUB                        , " _
                & "       DISCHARGE                          , " _
                & "       NOOFBOTTMCLOSURES                  , " _
                & "       IMCOCLASS                          , " _
                & "       FOOTVALUETYPE                      , " _
                & "       BACKVALUETYPE                      , " _
                & "       TOPDISVALUETYPE                    , " _
                & "       AIRINLETVALUE                      , " _
                & "       BAFFLES                            , " _
                & "       TYPEOFPREVACVALUE                  , " _
                & "       BURSTDISCFITTED                    , " _
                & "       TYPEOFTHERM                        , " _
                & "       TYPEOFMANLID_CENTER                , " _
                & "       TYPEOFMANLID_FRONT                 , " _
                & "       TYPEOFMLSEAL                       , " _
                & "       WORKINGPRESSURE                    , " _
                & "       TESTPRESSURE                       , " _
                & "       REMARK1                            , " _
                & "       REMARK2                            , " _
                & "       FAULTS                             , " _
                & "       BASERAGEYY                         , " _
                & "       BASERAGEMM                         , " _
                & "       BASERAGE                           , " _
                & "       BASELEASE                          , " _
                & "       MARUKANSEAL                        , " _
                & "       REMARK                             , " _
                & "       DELFLG                             , " _
                & "       UPDYMD                             , " _
                & "       UPDUSER                            , " _
                & "       UPDTERMID                          , " _
                & "       ORDERNO                            , " _
                & "       LOADCOUNTRY1 AS NEWTANKCOUNTRY     , " _
                & "       LOADPORT1 AS NEWTANKPORT           , " _
                & "       ACTIONID AS NEWTANKACTY            , " _
                & "       ACTYCNT AS ACTYCNT                   " _
                & "  FROM (" _
                & "SELECT " _
                & "       '' as APPLYID , " _
                & "       isnull(rtrim(tbl1.COMPCODE),'')                 as COMPCODE , " _
                & "       isnull(rtrim(tbl1.TANKNO),'')                   as TANKNO , " _
                & "       isnull(convert(nvarchar, tbl1.STYMD , 111),'')  as STYMD , " _
                & "       isnull(convert(nvarchar, tbl1.ENDYMD , 111),'') as ENDYMD , " _
                & "       isnull(rtrim(tbl1.PROPERTY),'')                 as PROPERTY , " _
                & "       isnull(rtrim(tbl1.LMOF),'')                     as LMOF , " _
                & "       isnull(rtrim(tbl1.LEASESTAT),'')                as LEASESTAT , " _
                & "       isnull(rtrim(tbl1.REPAIRSTAT),'')               as REPAIRSTAT , " _
                & "       isnull(convert(nvarchar, tbl1.INSPECTDATE5 , 111),'')  as INSPECTDATE5 , " _
                & "       isnull(convert(nvarchar, tbl1.INSPECTDATE2P5 , 111),'')  as INSPECTDATE2P5 , " _
                & "       isnull(convert(nvarchar, tbl1.NEXTINSPECTDATE , 111),'')  as NEXTINSPECTDATE , " _
                & "       isnull(rtrim(tbl1.NEXTINSPECTTYPE),'')          as NEXTINSPECTTYPE , " _
                & "       isnull(rtrim(tbl1.JAPFIREAPPROVED),'')          as JAPFIREAPPROVED , " _
                & "       isnull(rtrim(tbl1.MANUFACTURER),'')             as MANUFACTURER , " _
                & "       isnull(rtrim(tbl1.MANUFACTURESERIALNO),'')      as MANUFACTURESERIALNO , " _
                & "       isnull(convert(nvarchar, tbl1.DATEOFMANUFACTURE , 111),'')  as DATEOFMANUFACTURE , " _
                & "       isnull(rtrim(tbl1.MATERIAL),'')                 as MATERIAL , " _
                & "       isnull(rtrim(tbl1.STRUCT),'')                   as STRUCT , " _
                & "       isnull(rtrim(tbl1.USDOTAPPROVED),'')            as USDOTAPPROVED , " _
                & "       isnull(rtrim(tbl1.NOMINALCAPACITY),'')          as NOMINALCAPACITY , " _
                & "       isnull(rtrim(tbl1.TANKCAPACITY),'')             as TANKCAPACITY , " _
                & "       isnull(rtrim(tbl1.MAXGROSSWEIGHT),'')           as MAXGROSSWEIGHT , " _
                & "       isnull(rtrim(tbl1.NETWEIGHT),'')                as NETWEIGHT , " _
                & "       isnull(rtrim(tbl1.FREAMDIMENSION_H),'')         as FREAMDIMENSION_H , " _
                & "       isnull(rtrim(tbl1.FREAMDIMENSION_W),'')         as FREAMDIMENSION_W , " _
                & "       isnull(rtrim(tbl1.FREAMDIMENSION_L),'')         as FREAMDIMENSION_L , " _
                & "       isnull(rtrim(tbl1.HEATING),'')                  as HEATING , " _
                & "       isnull(rtrim(tbl1.HEATING_SUB),'')              as HEATING_SUB , " _
                & "       isnull(rtrim(tbl1.DISCHARGE),'')                as DISCHARGE , " _
                & "       isnull(rtrim(tbl1.NOOFBOTTMCLOSURES),'')        as NOOFBOTTMCLOSURES , " _
                & "       isnull(rtrim(tbl1.IMCOCLASS),'')                as IMCOCLASS , " _
                & "       isnull(rtrim(tbl1.FOOTVALUETYPE),'')            as FOOTVALUETYPE , " _
                & "       isnull(rtrim(tbl1.BACKVALUETYPE),'')            as BACKVALUETYPE , " _
                & "       isnull(rtrim(tbl1.TOPDISVALUETYPE),'')          as TOPDISVALUETYPE , " _
                & "       isnull(rtrim(tbl1.AIRINLETVALUE),'')            as AIRINLETVALUE , " _
                & "       isnull(rtrim(tbl1.BAFFLES),'')                  as BAFFLES , " _
                & "       isnull(rtrim(tbl1.TYPEOFPREVACVALUE),'')        as TYPEOFPREVACVALUE , " _
                & "       isnull(rtrim(tbl1.BURSTDISCFITTED),'')          as BURSTDISCFITTED , " _
                & "       isnull(rtrim(tbl1.TYPEOFTHERM),'')              as TYPEOFTHERM , " _
                & "       isnull(rtrim(tbl1.TYPEOFMANLID_CENTER),'')      as TYPEOFMANLID_CENTER , " _
                & "       isnull(rtrim(tbl1.TYPEOFMANLID_FRONT),'')       as TYPEOFMANLID_FRONT , " _
                & "       isnull(rtrim(tbl1.TYPEOFMLSEAL),'')             as TYPEOFMLSEAL , " _
                & "       isnull(rtrim(tbl1.WORKINGPRESSURE),'')          as WORKINGPRESSURE , " _
                & "       isnull(rtrim(tbl1.TESTPRESSURE),'')             as TESTPRESSURE , " _
                & "       isnull(rtrim(tbl1.REMARK1),'')                  as REMARK1 , " _
                & "       isnull(rtrim(tbl1.REMARK2),'')                  as REMARK2 , " _
                & "       isnull(rtrim(tbl1.FAULTS),'')                   as FAULTS , " _
                & "       isnull(rtrim(tbl1.BASERAGEYY),'')               as BASERAGEYY , " _
                & "       isnull(rtrim(tbl1.BASERAGEMM),'')               as BASERAGEMM , " _
                & "       isnull(rtrim(tbl1.BASERAGE),'')                 as BASERAGE , " _
                & "       isnull(rtrim(tbl1.BASELEASE),'')                as BASELEASE , " _
                & "       isnull(rtrim(tbl1.MARUKANSEAL),'')              as MARUKANSEAL , " _
                & "       isnull(rtrim(tbl1.REMARK),'')                   as REMARK , " _
                & "       isnull(rtrim(tbl1.DELFLG),'')                   as DELFLG , " _
                & "       isnull(convert(nvarchar, tbl1.UPDYMD , 120),'') as UPDYMD , " _
                & "       isnull(rtrim(tbl1.UPDUSER),'')                  as UPDUSER , " _
                & "       isnull(rtrim(tbl1.UPDTERMID),'')                as UPDTERMID , " _
                & "       isnull(rtrim(tblob.ORDERNO),'')                 as ORDERNO , " _
                & "       isnull(rtrim(tblob.LOADCOUNTRY1),'')            as LOADCOUNTRY1 , " _
                & "       isnull(rtrim(tblob.LOADPORT1),'')               as LOADPORT1 , " _
                & "       isnull(rtrim(tblo.ACTIONID),'')                 as ACTIONID , " _
                & "       isnull(rtrim(tblov2.ACTYCNT),0)                 as ACTYCNT , " _
                & "       TIMSTP = cast(tbl1.UPDTIMSTP                    as bigint) " _
                & " FROM " & CONST_TBLMASTER & " as tbl1 " _
                & " LEFT OUTER JOIN " & CONST_TBLORDERV & " as tblo " _
                & "   ON  tblo.TANKNO    = tbl1.TANKNO " _
                & "   AND tblo.ACTIONID  IN (SELECT KEYCODE FROM COS0017_FIXVALUE WHERE CLASS = 'NEWTANKACTY' AND DELFLG <> 'Y') " _
                & "   AND tblo.DELFLG    <> @P4 " _
                & "   AND tblo.DISPSEQ    = @NEWDISPSEQ " _
                & "   AND tblo.ACTUALDATE = @NEWACTUAL " _
                & "   AND tblo.INITYMD    = @NEWYMD " _
                & "   AND tblo.INITUSER   = @NEWUSER " _
                & " LEFT OUTER JOIN " & CONST_TBLORDERB & " as tblob " _
                & "   ON  tblob.ORDERNO    = tblo.ORDERNO " _
                & "   AND tblob.DELFLG    <> @P4 " _
                & " LEFT OUTER JOIN ( " _
                & "   SELECT TANKNO,COUNT(*) AS ACTYCNT FROM " & CONST_TBLORDERV & " " _
                & "   WHERE  ACTIONID    <> '' " _
                & "   AND    DELFLG      <> @P4 " _
                & "   GROUP BY TANKNO ) tblov2 " _
                & "   ON  tblov2.TANKNO   = tbl1.TANKNO " _
                & " WHERE tbl1.DELFLG    <> @P4 " _
                & " AND   tbl1.STYMD     <= @P1 " _
                & " AND   tbl1.ENDYMD    >= @P2 " _
                & " AND   NOT EXISTS( "
            '承認画面から遷移の場合
            If  Page.PreviousPage Is Nothing Then
                SQLStr &= " SELECT * FROM " & CONST_TBLAPPLY & " as tbl2 " _
                    & " WHERE tbl2.APPLYID = @P3 "
            Else
                SQLStr &= " SELECT * FROM " & CONST_TBLAPPLY & " as tbl2 " _
                    & " WHERE tbl1.COMPCODE = tbl2.COMPCODE " _
                    & " AND   tbl1.TANKNO = tbl2.TANKNO " _
                    & " AND   tbl1.STYMD = tbl2.STYMD " _
                    & " AND   tbl1.DELFLG <> @P4 " _
                    & " AND   tbl2.DELFLG <> @P4 "
            End If
            SQLStr &= " )" _
                & " UNION ALL " _
                & "SELECT " _
                & "       isnull(rtrim(tbla.APPLYID),'')                  as APPLYID , " _
                & "       isnull(rtrim(tbla.COMPCODE),'')                 as COMPCODE , " _
                & "       isnull(rtrim(tbla.TANKNO),'')                   as TANKNO , " _
                & "       isnull(convert(nvarchar, tbla.STYMD , 111),'')  as STYMD , " _
                & "       isnull(convert(nvarchar, tbla.ENDYMD , 111),'') as ENDYMD , " _
                & "       isnull(rtrim(tbla.PROPERTY),'')                 as PROPERTY , " _
                & "       isnull(rtrim(tbla.LMOF),'')                     as LMOF , " _
                & "       isnull(rtrim(tbla.LEASESTAT),'')                as LEASESTAT , " _
                & "       isnull(rtrim(tbla.REPAIRSTAT),'')               as REPAIRSTAT , " _
                & "       isnull(convert(nvarchar, tbla.INSPECTDATE5 , 111),'')  as INSPECTDATE5 , " _
                & "       isnull(convert(nvarchar, tbla.INSPECTDATE2P5 , 111),'')  as INSPECTDATE2P5 , " _
                & "       isnull(convert(nvarchar, tbla.NEXTINSPECTDATE , 111),'')  as NEXTINSPECTDATE , " _
                & "       isnull(rtrim(tbla.NEXTINSPECTTYPE),'')          as NEXTINSPECTTYPE , " _
                & "       isnull(rtrim(tbla.JAPFIREAPPROVED),'')          as JAPFIREAPPROVED , " _
                & "       isnull(rtrim(tbla.MANUFACTURER),'')             as MANUFACTURER , " _
                & "       isnull(rtrim(tbla.MANUFACTURESERIALNO),'')      as MANUFACTURESERIALNO , " _
                & "       isnull(convert(nvarchar, tbla.DATEOFMANUFACTURE , 111),'')  as DATEOFMANUFACTURE , " _
                & "       isnull(rtrim(tbla.MATERIAL),'')                 as MATERIAL , " _
                & "       isnull(rtrim(tbla.STRUCT),'')                   as STRUCT , " _
                & "       isnull(rtrim(tbla.USDOTAPPROVED),'')            as USDOTAPPROVED , " _
                & "       isnull(rtrim(tbla.NOMINALCAPACITY),'')          as NOMINALCAPACITY , " _
                & "       isnull(rtrim(tbla.TANKCAPACITY),'')             as TANKCAPACITY , " _
                & "       isnull(rtrim(tbla.MAXGROSSWEIGHT),'')           as MAXGROSSWEIGHT , " _
                & "       isnull(rtrim(tbla.NETWEIGHT),'')                as NETWEIGHT , " _
                & "       isnull(rtrim(tbla.FREAMDIMENSION_H),'')         as FREAMDIMENSION_H , " _
                & "       isnull(rtrim(tbla.FREAMDIMENSION_W),'')         as FREAMDIMENSION_W , " _
                & "       isnull(rtrim(tbla.FREAMDIMENSION_L),'')         as FREAMDIMENSION_L , " _
                & "       isnull(rtrim(tbla.HEATING),'')                  as HEATING , " _
                & "       isnull(rtrim(tbla.HEATING_SUB),'')              as HEATING_SUB , " _
                & "       isnull(rtrim(tbla.DISCHARGE),'')                as DISCHARGE , " _
                & "       isnull(rtrim(tbla.NOOFBOTTMCLOSURES),'')        as NOOFBOTTMCLOSURES , " _
                & "       isnull(rtrim(tbla.IMCOCLASS),'')                as IMCOCLASS , " _
                & "       isnull(rtrim(tbla.FOOTVALUETYPE),'')            as FOOTVALUETYPE , " _
                & "       isnull(rtrim(tbla.BACKVALUETYPE),'')            as BACKVALUETYPE , " _
                & "       isnull(rtrim(tbla.TOPDISVALUETYPE),'')          as TOPDISVALUETYPE , " _
                & "       isnull(rtrim(tbla.AIRINLETVALUE),'')            as AIRINLETVALUE , " _
                & "       isnull(rtrim(tbla.BAFFLES),'')                  as BAFFLES , " _
                & "       isnull(rtrim(tbla.TYPEOFPREVACVALUE),'')        as TYPEOFPREVACVALUE , " _
                & "       isnull(rtrim(tbla.BURSTDISCFITTED),'')          as BURSTDISCFITTED , " _
                & "       isnull(rtrim(tbla.TYPEOFTHERM),'')              as TYPEOFTHERM , " _
                & "       isnull(rtrim(tbla.TYPEOFMANLID_CENTER),'')      as TYPEOFMANLID_CENTER , " _
                & "       isnull(rtrim(tbla.TYPEOFMANLID_FRONT),'')       as TYPEOFMANLID_FRONT , " _
                & "       isnull(rtrim(tbla.TYPEOFMLSEAL),'')             as TYPEOFMLSEAL , " _
                & "       isnull(rtrim(tbla.WORKINGPRESSURE),'')          as WORKINGPRESSURE , " _
                & "       isnull(rtrim(tbla.TESTPRESSURE),'')             as TESTPRESSURE , " _
                & "       isnull(rtrim(tbla.REMARK1),'')                  as REMARK1 , " _
                & "       isnull(rtrim(tbla.REMARK2),'')                  as REMARK2 , " _
                & "       isnull(rtrim(tbla.FAULTS),'')                   as FAULTS , " _
                & "       isnull(rtrim(tbla.BASERAGEYY),'')               as BASERAGEYY , " _
                & "       isnull(rtrim(tbla.BASERAGEMM),'')               as BASERAGEMM , " _
                & "       isnull(rtrim(tbla.BASERAGE),'')                 as BASERAGE , " _
                & "       isnull(rtrim(tbla.BASELEASE),'')                as BASELEASE , " _
                & "       isnull(rtrim(tbla.MARUKANSEAL),'')              as MARUKANSEAL , " _
                & "       isnull(rtrim(tbla.REMARK),'')                   as REMARK , " _
                & "       isnull(rtrim(tbla.DELFLG),'')                   as DELFLG , " _
                & "       isnull(convert(nvarchar, tbla.UPDYMD , 120),'') as UPDYMD , " _
                & "       isnull(rtrim(tbla.UPDUSER),'')                  as UPDUSER , " _
                & "       isnull(rtrim(tbla.UPDTERMID),'')                as UPDTERMID , " _
                & "       isnull(rtrim(tblob.ORDERNO),'')                 as ORDERNO , " _
                & "       isnull(rtrim(tblob.LOADCOUNTRY1),'')            as LOADCOUNTRY1 , " _
                & "       isnull(rtrim(tblob.LOADPORT1),'')               as LOADPORT1 , " _
                & "       isnull(rtrim(tblo.ACTIONID),'')                 as ACTIONID , " _
                & "       isnull(rtrim(tblov2.ACTYCNT),0)                 as ACTYCNT , " _
                & "       TIMSTP = cast(tbla.UPDTIMSTP                    as bigint) " _
                & " FROM " & CONST_TBLAPPLY & " as tbla " _
                & " LEFT OUTER JOIN " & CONST_TBLORDERV & " as tblo " _
                & "   ON  tblo.TANKNO    = tbla.TANKNO " _
                & "   AND tblo.ACTIONID  IN (SELECT KEYCODE FROM COS0017_FIXVALUE WHERE CLASS = 'NEWTANKACTY' AND DELFLG <> 'Y') " _
                & "   AND tblo.DELFLG    <> @P4 " _
                & "   AND tblo.DISPSEQ    = @NEWDISPSEQ " _
                & "   AND tblo.ACTUALDATE = @NEWACTUAL " _
                & "   AND tblo.INITYMD    = @NEWYMD " _
                & "   AND tblo.INITUSER   = @NEWUSER " _
                & " LEFT OUTER JOIN " & CONST_TBLORDERB & " as tblob " _
                & "   ON  tblob.ORDERNO    = tblo.ORDERNO " _
                & "   AND tblob.DELFLG    <> @P4 " _
                & " LEFT OUTER JOIN ( " _
                & "   SELECT TANKNO,COUNT(*) AS ACTYCNT FROM " & CONST_TBLORDERV & " " _
                & "   WHERE  ACTIONID    <> '' " _
                & "   AND    DELFLG      <> @P4 " _
                & "   GROUP BY TANKNO ) tblov2 " _
                & "   ON  tblov2.TANKNO   = tbla.TANKNO " _
            '承認画面から遷移の場合
            If Page.PreviousPage Is Nothing Then
                SQLStr &= " WHERE tbla.APPLYID    = @P3 " _
                & " ) as tbl " _
                & " WHERE APPLYID    = @P3 "
            Else
                SQLStr &= " WHERE tbla.DELFLG    <> @P4 " _
                & " AND   tbla.STYMD     <= @P1 " _
                & " AND   tbla.ENDYMD    >= @P2 " _
                & " ) as tbl " _
                & " WHERE DELFLG    <> @P4 " _
                & " AND   STYMD     <= @P1 " _
                & " AND   ENDYMD    >= @P2 "
            End If

            ' 条件指定で指定されたものでＳＱＬで可能なものを追加する
            'タンク番号
            If (String.IsNullOrEmpty(Me.hdnSelectedTankNo.Value) = False AndAlso Me.hdnSelectedTankNo.Value <> "") Then
                SQLStr &= String.Format(" AND TANKNO = '{0}' ", Me.hdnSelectedTankNo.Value)
            End If

            SQLStr &= " ORDER BY " & COA0020ProfViewSort.SORTSTR

            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
            Dim NEWYMD As SqlParameter = SQLcmd.Parameters.Add("@NEWYMD", System.Data.SqlDbType.Date)
            Dim NEWUSER As SqlParameter = SQLcmd.Parameters.Add("@NEWUSER", System.Data.SqlDbType.NVarChar)
            Dim NEWDISPSEQ As SqlParameter = SQLcmd.Parameters.Add("@NEWDISPSEQ", System.Data.SqlDbType.NVarChar)
            Dim NEWACTUAL As SqlParameter = SQLcmd.Parameters.Add("@NEWACTUAL", System.Data.SqlDbType.Date)
            PARA1.Value = Me.hdnSelectedEndYMD.Value
            PARA2.Value = Me.hdnSelectedStYMD.Value
            If (String.IsNullOrEmpty(Me.hdnSelectedApplyID.Value) = False) Then
                PARA3.Value = Me.hdnSelectedApplyID.value
            Else
                PARA3.Value = ""
            End If
            PARA4.Value = BaseDllCommon.CONST_FLAG_YES
            NEWYMD.Value = CONST_NEWYMD
            NEWUSER.Value = CONST_NEWUSER
            NEWDISPSEQ.Value = CONST_NEWDISPSEQ
            NEWACTUAL.Value = CONST_NEWACTUAL
            SQLdr = SQLcmd.ExecuteReader()

            'BASEtbl値設定
            BASEtbl.Load(SQLdr)

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
            Return

        Finally
            'CLOSE
            If Not SQLdr Is Nothing Then
                SQLdr.Close()
            End If
            If Not SQLcmd Is Nothing Then
                SQLcmd.Dispose()
                SQLcmd = Nothing
            End If
            If Not SQLcon Is Nothing Then
                SQLcon.Close()
                SQLcon.Dispose()
                SQLcon = Nothing
            End If

        End Try

    End Sub
    ''' <summary>
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    Protected Sub UploadExcel()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        Dim COA0029XlsTable As New BASEDLL.COA0029XlsTable

        '初期処理
        errList = New List(Of String)
        errListAll = New List(Of String)
        returnCode = C_MESSAGENO.NORMAL

        '項目チェック準備
        '一覧表示データ復元
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            returnCode = COA0021ListTable.ERR
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'UPLOAD_XLSデータ取得
        COA0029XlsTable.MAPID = CONST_MAPID
        If Right(lbRightList.SelectedValue.ToString, 3) = "_IO" Then
            COA0029XlsTable.SHEETNAME = CONST_MAPID & "I"
        End If
        COA0029XlsTable.COA0029XlsToTable()
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

        'エラーレポート準備
        Dim INProwWork As DataRow

        '初期処理
        Me.txtRightErrorMessage.Text = ""
        Me.lblFooterMessage.Text = ""
        Me.lblFooterMessage.ForeColor = Color.Black
        Me.lblFooterMessage.Font.Bold = False

        Dim sameDr As EnumerableRowCollection(Of DataRow)

        'INPtblカラム設定
        BASEtblColumnsAdd(INPtbl)

        'Excelデータ毎にチェック＆更新
        Dim val As String = ""
        Dim type As String = ""
        Dim date5 As String = ""
        Dim date2_5 As String = ""
        Dim dateManu As String = ""

        For i As Integer = 0 To COA0029XlsTable.TBLDATA.Rows.Count - 1

            'XLSTBL明細⇒INProw
            INProwWork = INPtbl.NewRow

            Dim tankNo As String = COA0029XlsTable.TBLDATA.Rows(i)("TANKNO").ToString
            If tankNo = "" Then
                Exit For
            End If
            If COA0029XlsTable.TBLDATA.Columns.Contains("STYMD") Then
                Dim stYmd As String = COA0029XlsTable.TBLDATA.Rows(i)("STYMD").ToString
                sameDr = (From item In BASEtbl Where item("TANKNO").Equals(tankNo) AndAlso item("STYMD").Equals(stYmd))
            Else
                sameDr = (From item In BASEtbl Where item("TANKNO").Equals(tankNo))
            End If

            If sameDr.Any Then
                INProwWork.ItemArray = sameDr(0).ItemArray
            End If
            INProwWork("LINECNT") = 0
            INProwWork("OPERATION") = ""
            INProwWork("TIMSTP") = "0"
            INProwWork("SELECT") = 1
            INProwWork("HIDDEN") = 0

            Dim findInt As Integer = 99999
            Dim stYMDStr As String = ""

            For j As Integer = 5 To INPtbl.Columns.Count - 1

                ' カラム名設定
                Dim workColumn = INPtbl.Columns.Item(j).ColumnName
                If workColumn = "COMPCODE" Then
                    INProwWork(workColumn) = HttpContext.Current.Session("APSRVCamp")
                    '　カラム未定義、値が未設定の場合は空文字を設定
                ElseIf Not (COA0029XlsTable.TBLDATA.Columns.Contains(workColumn)) OrElse
                   IsDBNull(COA0029XlsTable.TBLDATA.Rows(i)(workColumn)) Then

                    If workColumn = "COMPCODE" Then
                        '申請IDは空白
                        INProwWork(workColumn) = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
                    ElseIf workColumn = "INSPECTDATE5" Then
                        date5 = INProwWork(workColumn).ToString
                    ElseIf workColumn = "INSPECTDATE2P5" Then
                        date2_5 = INProwWork(workColumn).ToString
                    ElseIf workColumn = "DATEOFMANUFACTURE" Then
                        dateManu = INProwWork(workColumn).ToString
                    Else
                        'INProwWork(workColumn) = ""
                    End If
                Else
                    INProwWork(workColumn) = COA0029XlsTable.TBLDATA.Rows(i)(workColumn)

                    'カラム毎の編集が必要な場合、個別に設定
                    If workColumn = "APPLYID" Then
                        '申請IDは空白
                        INProwWork(workColumn) = ""
                    End If

                    'カラム毎の編集が必要な場合、個別に設定
                    If workColumn = "STYMD" Then
                        If IsDate(INProwWork(workColumn)) Then
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString("yyyy/MM/dd")
                        End If
                    End If
                    If workColumn = "ENDYMD" Then
                        If IsDate(INProwWork(workColumn)) Then
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString("yyyy/MM/dd")
                        End If
                    End If
                    If workColumn = "INSPECTDATE5" Then
                        date5 = ""
                        If IsDate(INProwWork(workColumn)) Then
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString("yyyy/MM/dd")
                            date5 = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString(GBA00003UserSetting.DATEFORMAT)
                        End If
                    End If
                    If workColumn = "INSPECTDATE2P5" Then
                        date2_5 = ""
                        If IsDate(INProwWork(workColumn)) Then
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString("yyyy/MM/dd")
                            date2_5 = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString(GBA00003UserSetting.DATEFORMAT)
                        End If
                    End If
                    If workColumn = "DATEOFMANUFACTURE" Then
                        dateManu = ""
                        If IsDate(INProwWork(workColumn)) Then
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString("yyyy/MM/dd")
                            dateManu = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString(GBA00003UserSetting.DATEFORMAT)
                        End If
                    End If

                End If
            Next
            ' 次回点検自動算出 ここから
            SetNextInspect(date2_5, date5, dateManu, val, type)
            If val <> "" Then
                val = BASEDLL.FormatDateYMD(val, GBA00003UserSetting.DATEFORMAT)
            End If
            INProwWork("NEXTINSPECTDATE") = val
            INProwWork("NEXTINSPECTTYPE") = type
            ' 次回点検自動算出 ここまで
            INPtbl.Rows.Add(INProwWork)
            val = ""
            type = ""

        Next

        INPtblCheck()

        BASEtblUpdate()

        '画面編集
        '一覧表示データ保存
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'メッセージ表示
        If errListAll.Count > 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
        ElseIf returnCode = C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        Else
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
        End If

        '画面終了
        'Close
        BASEtbl.Clear()
        INPtbl.Clear()
        BASEtbl.Dispose()
        INPtbl.Dispose()

        COA0029XlsTable.TBLDATA.Dispose()
        COA0029XlsTable.TBLDATA.Clear()

        'カーソル設定
        txtOperationEx.Focus()

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
                Case Me.vLeftOperation.ID
                    SetOperationListItem()
                '会社コードビュー表示切替
                Case Me.vLeftCompCode.ID
                    SetCompCodeListItem(Me.txtCompCode.Text)
                '所属ビュー表示切替
                Case Me.vLeftProperty.ID
                    SetPropertyListItem()
                '所有形態（自社、リース他）ビュー表示切替
                Case Me.vLeftLMOF.ID
                    SetLMOFListItem()
                '削除フラグビュー表示切替
                Case Me.vLeftDelFlg.ID
                    SetDelFlgListItem(Me.txtDelFlg.Text)
                'リースビュー表示切替
                Case Me.vLeftLeaseStat.ID
                    SetLeaseStatListItem()
                'リペアビュー表示切替
                Case Me.vLeftRepairStat.ID
                    SetRepairStatListItem()
                'JP消防検査有無ビュー表示切替
                Case Me.vLeftJapFireApproved.ID
                    SetJapFireApprovedListItem()
                '製造メーカービュー表示切替
                Case Me.vLeftManufacture.ID
                    SetManufactureListItem()

                '追加構造ビュー表示切替
                Case Me.vLeftStruct.ID
                    SetStructListItem()

                '荷重試験実施の有無ビュー表示切替
                Case Me.vLeftUsDotApproved.ID
                    SetUsDotApprovedListItem()

                '液出し口の位置ビュー表示切替
                Case Me.vLeftDischarge.ID
                    SetDischargeListItem()

                'フート弁の仕様ビュー表示切替
                Case Me.vLeftFootValue.ID
                    SetFootValueListItem()

                '下部液出し口の仕様ビュー表示切替
                Case Me.vLeftBottomOutlet.ID
                    SetBottomOutletListItem()

                '上部積込口の仕様ビュー表示切替
                Case Me.vLeftTopOutlet.ID
                    SetTopOutletListItem()

                'エアラインのバルブの仕様ビュー表示切替
                Case Me.vLeftAirInlet.ID
                    SetAirInletListItem()

                '防波板の有無ビュー表示切替
                Case Me.vLeftBaffles.ID
                    SetBafflesListItem()

                '破裂板の有無ビュー表示切替
                Case Me.vLeftBurstDisc.ID
                    SetBurstDiscListItem()

                '温度計の種類ビュー表示切替
                Case Me.vLeftTherm.ID
                    SetThermListItem()

                'マンホールパッキンの種類ビュー表示切替
                Case Me.vLeftMlSeal.ID
                    SetMlSealListItem()

                'マル関ステッカー貼付ビュー表示切替
                Case Me.vLeftMarukanSticker.ID
                    SetMarukanStickerListItem()

                'New Tank Portビュー表示切替
                Case Me.vLeftNewTankPort.ID
                    SetNewTankPortListItem()

               'New Tank Actyビュー表示切替
                Case Me.vLeftNewTankActy.ID
                    SetNewTankActyListItem()

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
    ''' 絞り込みボタン押下時
    ''' </summary>
    Public Sub btnExtract_Click()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        Dim opeAll As String = Nothing
        Dim opeUpdate As String = Nothing
        Dim opeErr As String = Nothing
        Dim opeUpdErr As String = Nothing
        Dim blank As String = Nothing

        '比較文字設定
        If (COA0019Session.LANGDISP = C_LANG.JA) Then
            opeAll = "全て"
            opeUpdate = "更新"
            opeErr = "エラー"
            opeUpdErr = "更新エラー"
            blank = "空白"
        Else
            opeAll = "ALL"
            opeUpdate = "UPDATE"
            opeErr = "ERROR"
            opeUpdErr = "UPDATEERROR"
            blank = "BLANK"
        End If

        '一覧表示データ復元 
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '絞り込み操作（GridView明細Hidden設定）
        For i As Integer = 0 To BASEtbl.Rows.Count - 1

            BASEtbl.Rows(i)("HIDDEN") = 0

            'オペレーション　完全一致
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtOperationEx.Text <> "") Then
                Dim searchStr As String = Convert.ToString(BASEtbl.Rows(i)("OPERATION"))
                If searchStr.Contains("★") Then
                    searchStr = searchStr.Replace("★", "")
                End If

                If txtOperationEx.Text = opeAll Then
                    '全て表示する

                ElseIf txtOperationEx.Text = opeUpdate Then
                    '更新を表示
                    If (searchStr <> opeUpdate) Then
                        BASEtbl.Rows(i)("HIDDEN") = 1
                    End If
                ElseIf txtOperationEx.Text = opeErr Then
                    'エラーを表示
                    If (searchStr <> opeErr) Then
                        BASEtbl.Rows(i)("HIDDEN") = 1
                    End If
                ElseIf txtOperationEx.Text = opeUpdErr Then
                    '更新、エラーを表示
                    If (searchStr <> opeUpdate AndAlso searchStr <> opeErr) Then
                        BASEtbl.Rows(i)("HIDDEN") = 1
                    End If
                ElseIf txtOperationEx.Text = blank Then
                    '空白を表示
                    If searchStr <> "" Then
                        BASEtbl.Rows(i)("HIDDEN") = 1
                    End If
                Else
                    'その他非表示
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If
            End If

            'タンク番号 絞込判定
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtTankNoEx.Text <> "") Then
                Dim searchStr As String = Convert.ToString(BASEtbl.Rows(i)("TANKNO")).ToUpper
                '検索用文字列（前方一致）
                If Not searchStr.StartsWith(txtTankNoEx.Text.ToUpper) Then
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If
            End If
        Next

        '画面先頭を表示
        hdnListPosition.Value = "1"

        '一覧表示データ保存
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
        Else
            'メッセージ表示
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALEXTRUCT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        End If

        'カーソル設定
        txtOperationEx.Focus()
    End Sub
    ''' <summary>
    ''' DB更新ボタン押下時
    ''' </summary>
    Public Sub btnDbUpdate_Click()
        COA0003LogFile = New COA0003LogFile                         'ログ出力
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0030Journal As New BASEDLL.COA0030Journal            'Journal Out
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        Dim GBA00002MasterApplyID As New GBA00002MasterApplyID
        Dim copyDataTable As New DataTable
        Dim dummyMsgBox As Label = New Label
        Dim workBaseRow As DataRow = BASEtbl.NewRow

        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing
        Dim SQLStr2 As String = Nothing
        Dim SQLcmd2 As New SqlCommand()
        Dim SQLdr2 As SqlDataReader = Nothing

        'Gridview追加処理
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Try

            txtRightErrorMessage.Text = ""

            SQLcon.Open() 'DataBase接続(Open)

            'DB更新前チェック
            '  ※同一Key全てのレコードが更新されていない事をチェックする

            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = updateDisp AndAlso Convert.ToString(BASEtbl.Rows(i)("TIMSTP")) <> "0" Then
                    '※追加レコードは、BASEtbl.Rows(i)("TIMSTP") = "0"となっている

                    Try

                        '同一Keyレコードを抽出
                        SQLStr = ""
                        SQLStr =
                            " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP, " _
                             & "   convert(nvarchar, UPDYMD , 120) as UPDYMD, " _
                             & "   rtrim(UPDUSER) as UPDUSER , rtrim(UPDTERMID) as UPDTERMID " _
                             & " FROM " & CONST_TBLMASTER _
                             & " WHERE COMPCODE = @P01 " _
                             & "   and TANKNO = @P02 " _
                             & "   and STYMD = @P03 " _
                             & "   and DELFLG <> @P04 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        With SQLcmd.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TANKNO")
                            .Add("@P03", System.Data.SqlDbType.Date).Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                            .Add("@P04", System.Data.SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
                        End With
                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("TANKNO")) = Convert.ToString(BASEtbl.Rows(i)("TANKNO")) AndAlso
                                        RTrim(Convert.ToString(BASEtbl.Rows(j)("STYMD"))) = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD"))) Then

                                        BASEtbl.Rows(j)("OPERATION") = errDisp

                                    End If
                                Next

                                'エラーレポート編集
                                Dim errMessageStr As String = ""

                                'メッセージ取得
                                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, dummyMsgBox)

                                errMessageStr = "・" & dummyMsgBox.Text
                                errMessageStr = errMessageStr & ControlChars.NewLine
                                errMessageStr = errMessageStr & Me.ErrItemSet(BASEtbl.Rows(i))
                                If txtRightErrorMessage.Text <> "" Then
                                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                                End If
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & errMessageStr
                            End If

                        End While

                        SQLdr.Close()

                        '同一Keyレコードを抽出
                        SQLStr = ""
                        SQLStr =
                            " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP, " _
                             & "   convert(nvarchar, UPDYMD , 120) as UPDYMD, " _
                             & "   rtrim(UPDUSER) as UPDUSER , rtrim(UPDTERMID) as UPDTERMID " _
                             & " FROM " & CONST_TBLAPPLY _
                             & " WHERE COMPCODE = @P01 " _
                             & "   and TANKNO = @P02 " _
                             & "   and STYMD = @P03 " _
                             & "   and DELFLG <> @P04 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        With SQLcmd.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TANKNO")
                            .Add("@P03", System.Data.SqlDbType.Date).Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                            .Add("@P04", System.Data.SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
                        End With

                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("TANKNO")) = Convert.ToString(BASEtbl.Rows(i)("TANKNO")) AndAlso
                                        RTrim(Convert.ToString(BASEtbl.Rows(j)("STYMD"))) = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD"))) Then

                                        BASEtbl.Rows(j)("OPERATION") = errDisp

                                    End If
                                Next

                                'エラーレポート編集
                                Dim errMessageStr As String = ""

                                'メッセージ取得
                                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, dummyMsgBox)

                                errMessageStr = "・" & dummyMsgBox.Text
                                errMessageStr = errMessageStr & ControlChars.NewLine
                                errMessageStr = errMessageStr & Me.ErrItemSet(BASEtbl.Rows(i))
                                If txtRightErrorMessage.Text <> "" Then
                                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                                End If
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & errMessageStr
                            End If

                        End While

                    Finally
                        'CLOSE
                        If Not SQLdr Is Nothing Then
                            SQLdr.Close()
                        End If
                        If Not SQLcmd Is Nothing Then
                            SQLcmd.Dispose()
                            SQLcmd = Nothing
                        End If

                    End Try

                End If
            Next

            'ＤＢ更新
            '　※エラーは処理されない
            For i As Integer = 0 To BASEtbl.Rows.Count - 1

                Try
                    workBaseRow = BASEtbl.Rows(i)
                    If (Convert.ToString(workBaseRow("OPERATION")) = updateDisp OrElse Convert.ToString(workBaseRow("OPERATION")) = "★" & updateDisp) Then

                        '削除は更新しない
                        If Convert.ToString(workBaseRow("DELFLG")) = BaseDllCommon.CONST_FLAG_YES AndAlso Convert.ToString(workBaseRow("TIMSTP")) = "0" Then
                            workBaseRow("OPERATION") = ""
                            Continue For
                        End If

                        Dim updTable As String = CONST_TBLAPPLY
                        '申請ID取得
                        GBA00002MasterApplyID.COMPCODE = COA0019Session.APSRVCamp
                        GBA00002MasterApplyID.SYSCODE = C_SYSCODE_GB
                        GBA00002MasterApplyID.KEYCODE = COA0019Session.APSRVname
                        GBA00002MasterApplyID.MAPID = CONST_MAPID
                        GBA00002MasterApplyID.EVENTCODE = CONST_EVENTCODE
                        GBA00002MasterApplyID.SUBCODE = ""
                        GBA00002MasterApplyID.COA0032getgApplyID()
                        If GBA00002MasterApplyID.ERR = C_MESSAGENO.NORMAL Then
                            workBaseRow("APPLYID") = GBA00002MasterApplyID.APPLYID
                            If GBA00002MasterApplyID.APPLYID <> "" Then
                                '承認必要(申請テーブルに登録)
                                updTable = CONST_TBLAPPLY
                            Else
                                updTable = CONST_TBLMASTER
                            End If
                        Else
                            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00002MasterApplyID.ERR)})
                            Return
                        End If

                        If Convert.ToString(workBaseRow("APPLYID")) <> "" Then

                            '申請登録
                            COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
                            COA0032Apploval.I_APPLYID = Convert.ToString(workBaseRow("APPLYID"))
                            COA0032Apploval.I_MAPID = CONST_MAPID
                            COA0032Apploval.I_EVENTCODE = CONST_EVENTCODE
                            COA0032Apploval.I_SUBCODE = ""
                            COA0032Apploval.COA0032setApply()
                            If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
                            Else
                                CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
                                Return
                            End If

                        End If

                        '更新SQL文･･･マスタへ更新
                        Dim nowDate As DateTime = Date.Now
                        SQLStr = ""
                        SQLStr =
                                   " DECLARE @timestamp as bigint ; " _
                                 & " set @timestamp = 0 ; " _
                                 & " DECLARE timestamp CURSOR FOR  " _
                                 & "  SELECT CAST(UPDTIMSTP as bigint) as timestamp " _
                                 & "  FROM " & updTable _
                                 & "  WHERE COMPCODE = @P02  " _
                                 & "    AND TANKNO   = @P03  " _
                                 & "    AND STYMD    = @P04 ;  " _
                                 & " OPEN timestamp ;  " _
                                 & " FETCH NEXT FROM timestamp INTO @timestamp ;  " _
                                 & " IF ( @@FETCH_STATUS = 0 ) " _
                                 & "  UPDATE " & updTable _
                                 & "  SET "
                        If Convert.ToString(workBaseRow("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID = @P01 , "
                        End If
                        SQLStr = SQLStr & " ENDYMD = @P05 , " _
                                 & "        PROPERTY = @P55 , " _
                                 & "        LMOF = @P06 , " _
                                 & "        LEASESTAT = @P07 , " _
                                 & "        REPAIRSTAT = @P08 , " _
                                 & "        INSPECTDATE5 = @P09 , " _
                                 & "        INSPECTDATE2P5 = @P10 , " _
                                 & "        NEXTINSPECTDATE = @P51 , " _
                                 & "        NEXTINSPECTTYPE = @P52 , " _
                                 & "        JAPFIREAPPROVED = @P11 , " _
                                 & "        MANUFACTURER = @P12 , " _
                                 & "        MANUFACTURESERIALNO = @P13 , " _
                                 & "        DATEOFMANUFACTURE = @P14 , " _
                                 & "        MATERIAL = @P15 , " _
                                 & "        STRUCT = @P16 , " _
                                 & "        USDOTAPPROVED = @P17 , " _
                                 & "        NOMINALCAPACITY = @P53 , " _
                                 & "        TANKCAPACITY = @P18 , " _
                                 & "        MAXGROSSWEIGHT = @P60 , " _
                                 & "        NETWEIGHT = @P19 , " _
                                 & "        FREAMDIMENSION_H = @P56 , " _
                                 & "        FREAMDIMENSION_W = @P57 , " _
                                 & "        FREAMDIMENSION_L = @P58 , " _
                                 & "        HEATING = @P21 , " _
                                 & "        HEATING_SUB = @P20 , " _
                                 & "        DISCHARGE = @P22 , " _
                                 & "        NOOFBOTTMCLOSURES = @P23 , " _
                                 & "        IMCOCLASS = @P24 , " _
                                 & "        FOOTVALUETYPE = @P25 , " _
                                 & "        BACKVALUETYPE = @P26 , " _
                                 & "        TOPDISVALUETYPE = @P27 , " _
                                 & "        AIRINLETVALUE = @P28 , " _
                                 & "        BAFFLES = @P29 , " _
                                 & "        TYPEOFPREVACVALUE = @P30 , " _
                                 & "        BURSTDISCFITTED = @P31 , " _
                                 & "        TYPEOFTHERM = @P32 , " _
                                 & "        TYPEOFMANLID_CENTER = @P33 , " _
                                 & "        TYPEOFMANLID_FRONT = @P59 , " _
                                 & "        TYPEOFMLSEAL = @P34 , " _
                                 & "        WORKINGPRESSURE = @P35 , " _
                                 & "        TESTPRESSURE = @P36 , " _
                                 & "        REMARK1 = @P37 , " _
                                 & "        REMARK2 = @P38 , " _
                                 & "        FAULTS = @P39 , " _
                                 & "        BASERAGEYY = @P40 , " _
                                 & "        BASERAGEMM = @P41 , " _
                                 & "        BASERAGE = @P42 , " _
                                 & "        BASELEASE = @P43 , " _
                                 & "        MARUKANSEAL = @P54 , " _
                                 & "        REMARK = @P44 , " _
                                 & "        DELFLG = @P45 , " _
                                 & "        UPDYMD = @P47 , " _
                                 & "        UPDUSER = @P48 , " _
                                 & "        UPDTERMID = @P49 , " _
                                 & "        RECEIVEYMD = @P50  " _
                                 & "  WHERE COMPCODE = @P02 " _
                                 & "    AND TANKNO = @P03 " _
                                 & "    AND STYMD = @P04 ; " _
                                 & " IF ( @@FETCH_STATUS <> 0 ) " _
                                 & "  INSERT INTO " & updTable _
                                 & "       ("
                        If Convert.ToString(workBaseRow("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID , "
                        End If
                        SQLStr = SQLStr & " COMPCODE , " _
                                 & "        TANKNO , " _
                                 & "        STYMD , " _
                                 & "        ENDYMD , " _
                                 & "        PROPERTY , " _
                                 & "        LMOF , " _
                                 & "        LEASESTAT , " _
                                 & "        REPAIRSTAT , " _
                                 & "        INSPECTDATE5 , " _
                                 & "        INSPECTDATE2P5 , " _
                                 & "        NEXTINSPECTDATE , " _
                                 & "        NEXTINSPECTTYPE , " _
                                 & "        JAPFIREAPPROVED , " _
                                 & "        MANUFACTURER , " _
                                 & "        MANUFACTURESERIALNO , " _
                                 & "        DATEOFMANUFACTURE , " _
                                 & "        MATERIAL , " _
                                 & "        STRUCT , " _
                                 & "        USDOTAPPROVED , " _
                                 & "        NOMINALCAPACITY , " _
                                 & "        TANKCAPACITY , " _
                                 & "        MAXGROSSWEIGHT , " _
                                 & "        NETWEIGHT , " _
                                 & "        FREAMDIMENSION_H , " _
                                 & "        FREAMDIMENSION_W , " _
                                 & "        FREAMDIMENSION_L , " _
                                 & "        HEATING , " _
                                 & "        HEATING_SUB , " _
                                 & "        DISCHARGE , " _
                                 & "        NOOFBOTTMCLOSURES , " _
                                 & "        IMCOCLASS , " _
                                 & "        FOOTVALUETYPE , " _
                                 & "        BACKVALUETYPE , " _
                                 & "        TOPDISVALUETYPE , " _
                                 & "        AIRINLETVALUE , " _
                                 & "        BAFFLES , " _
                                 & "        TYPEOFPREVACVALUE , " _
                                 & "        BURSTDISCFITTED , " _
                                 & "        TYPEOFTHERM , " _
                                 & "        TYPEOFMANLID_CENTER , " _
                                 & "        TYPEOFMANLID_FRONT , " _
                                 & "        TYPEOFMLSEAL , " _
                                 & "        WORKINGPRESSURE , " _
                                 & "        TESTPRESSURE , " _
                                 & "        REMARK1 , " _
                                 & "        REMARK2 , " _
                                 & "        FAULTS , " _
                                 & "        BASERAGEYY , " _
                                 & "        BASERAGEMM , " _
                                 & "        BASERAGE , " _
                                 & "        BASELEASE , " _
                                 & "        MARUKANSEAL , " _
                                 & "        REMARK , " _
                                 & "        DELFLG , " _
                                 & "        INITYMD , " _
                                 & "        UPDYMD , " _
                                 & "        UPDUSER , " _
                                 & "        UPDTERMID , " _
                                 & "        RECEIVEYMD ) " _
                                 & "  VALUES ( "
                        If Convert.ToString(workBaseRow("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " @P01, "
                        End If
                        SQLStr = SQLStr & "         @P02,@P03,@P04,@P05,@P55,@P06,@P07,@P08,@P09,@P10,@P51,@P52, " _
                                 & "           @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P53,@P18,@P60,@P19,@P56,@P57,@P58, " _
                                 & "           @P21,@P20,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30, " _
                                 & "           @P31,@P32,@P33,@P59,@P34,@P35,@P36,@P37,@P38,@P39,@P40, " _
                                 & "           @P41,@P42,@P43,@P54,@P44,@P45,@P46,@P47,@P48,@P49,@P50); " _
                                 & " CLOSE timestamp ; " _
                                 & " DEALLOCATE timestamp ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                        Dim PARA55 As SqlParameter = SQLcmd.Parameters.Add("@P55", System.Data.SqlDbType.NVarChar)
                        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
                        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)
                        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
                        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.Date)
                        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.Date)
                        Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", System.Data.SqlDbType.Date)
                        Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", System.Data.SqlDbType.NVarChar)
                        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)
                        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
                        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
                        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.Date)
                        Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)
                        Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar)
                        Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.NVarChar)
                        Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", System.Data.SqlDbType.Int)
                        Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.Int)
                        Dim PARA60 As SqlParameter = SQLcmd.Parameters.Add("@P60", System.Data.SqlDbType.Int)
                        Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.Int)
                        Dim PARA56 As SqlParameter = SQLcmd.Parameters.Add("@P56", System.Data.SqlDbType.NVarChar)
                        Dim PARA57 As SqlParameter = SQLcmd.Parameters.Add("@P57", System.Data.SqlDbType.NVarChar)
                        Dim PARA58 As SqlParameter = SQLcmd.Parameters.Add("@P58", System.Data.SqlDbType.NVarChar)
                        Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar)
                        Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.NVarChar)
                        Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar)
                        Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar)
                        Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar)
                        Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.NVarChar)
                        Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.NVarChar)
                        Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar)
                        Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar)
                        Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar)
                        Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.NVarChar)
                        Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar)
                        Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.NVarChar)
                        Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.NVarChar)
                        Dim PARA59 As SqlParameter = SQLcmd.Parameters.Add("@P59", System.Data.SqlDbType.NVarChar)
                        Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.NVarChar)
                        Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.NVarChar)
                        Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.NVarChar)
                        Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar)
                        Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.NVarChar)
                        Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.NVarChar)
                        Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.NVarChar)
                        Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", System.Data.SqlDbType.NVarChar)
                        Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", System.Data.SqlDbType.NVarChar)
                        Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", System.Data.SqlDbType.NVarChar)
                        Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", System.Data.SqlDbType.NVarChar)
                        Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", System.Data.SqlDbType.NVarChar)
                        Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", System.Data.SqlDbType.NVarChar)
                        Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", System.Data.SqlDbType.DateTime)
                        Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", System.Data.SqlDbType.DateTime)
                        Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", System.Data.SqlDbType.NVarChar)
                        Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", System.Data.SqlDbType.NVarChar)
                        Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", System.Data.SqlDbType.DateTime)
                        '' PARA60定義済み

                        PARA01.Value = workBaseRow("APPLYID")
                        PARA02.Value = workBaseRow("COMPCODE")
                        PARA03.Value = workBaseRow("TANKNO")
                        PARA04.Value = RTrim(Convert.ToString(workBaseRow("STYMD")))
                        PARA05.Value = RTrim(Convert.ToString(workBaseRow("ENDYMD")))
                        PARA55.Value = workBaseRow("PROPERTY")
                        PARA06.Value = workBaseRow("LMOF")
                        PARA07.Value = workBaseRow("LEASESTAT")
                        PARA08.Value = workBaseRow("REPAIRSTAT")
                        If RTrim(Convert.ToString(workBaseRow("INSPECTDATE5"))) <> "" Then
                            PARA09.Value = RTrim(Convert.ToString(workBaseRow("INSPECTDATE5")))
                        Else
                            PARA09.Value = "1900/01/01"
                        End If
                        If RTrim(Convert.ToString(workBaseRow("INSPECTDATE2P5"))) <> "" Then
                            PARA10.Value = RTrim(Convert.ToString(workBaseRow("INSPECTDATE2P5")))
                        Else
                            PARA10.Value = "1900/01/01"
                        End If
                        If RTrim(Convert.ToString(workBaseRow("NEXTINSPECTDATE"))) <> "" Then
                            PARA51.Value = RTrim(Convert.ToString(workBaseRow("NEXTINSPECTDATE")))
                        Else
                            PARA51.Value = "1900/01/01"
                        End If
                        PARA52.Value = workBaseRow("NEXTINSPECTTYPE")
                        PARA11.Value = workBaseRow("JAPFIREAPPROVED")
                        PARA12.Value = workBaseRow("MANUFACTURER")
                        PARA13.Value = workBaseRow("MANUFACTURESERIALNO")
                        If RTrim(Convert.ToString(workBaseRow("DATEOFMANUFACTURE"))) <> "" Then
                            PARA14.Value = RTrim(Convert.ToString(workBaseRow("DATEOFMANUFACTURE")))
                        Else
                            PARA14.Value = "1900/01/01"
                        End If
                        PARA15.Value = workBaseRow("MATERIAL")
                        PARA16.Value = workBaseRow("STRUCT")
                        PARA17.Value = workBaseRow("USDOTAPPROVED")
                        PARA53.Value = workBaseRow("NOMINALCAPACITY")
                        PARA18.Value = workBaseRow("TANKCAPACITY")
                        PARA60.Value = workBaseRow("MAXGROSSWEIGHT")
                        PARA19.Value = workBaseRow("NETWEIGHT")
                        PARA56.Value = workBaseRow("FREAMDIMENSION_H")
                        PARA57.Value = workBaseRow("FREAMDIMENSION_W")
                        PARA58.Value = workBaseRow("FREAMDIMENSION_L")
                        PARA21.Value = workBaseRow("HEATING")
                        PARA20.Value = workBaseRow("HEATING_SUB")
                        PARA22.Value = workBaseRow("DISCHARGE")
                        PARA23.Value = workBaseRow("NOOFBOTTMCLOSURES")
                        PARA24.Value = workBaseRow("IMCOCLASS")
                        PARA25.Value = workBaseRow("FOOTVALUETYPE")
                        PARA26.Value = workBaseRow("BACKVALUETYPE")
                        PARA27.Value = workBaseRow("TOPDISVALUETYPE")
                        PARA28.Value = workBaseRow("AIRINLETVALUE")
                        PARA29.Value = workBaseRow("BAFFLES")
                        PARA30.Value = workBaseRow("TYPEOFPREVACVALUE")
                        PARA31.Value = workBaseRow("BURSTDISCFITTED")
                        PARA32.Value = workBaseRow("TYPEOFTHERM")
                        PARA33.Value = workBaseRow("TYPEOFMANLID_CENTER")
                        PARA59.Value = workBaseRow("TYPEOFMANLID_FRONT")
                        PARA34.Value = workBaseRow("TYPEOFMLSEAL")
                        PARA35.Value = workBaseRow("WORKINGPRESSURE")
                        PARA36.Value = workBaseRow("TESTPRESSURE")
                        PARA37.Value = workBaseRow("REMARK1")
                        PARA38.Value = workBaseRow("REMARK2")
                        PARA39.Value = workBaseRow("FAULTS")
                        PARA40.Value = workBaseRow("BASERAGEYY")
                        PARA41.Value = workBaseRow("BASERAGEMM")
                        PARA42.Value = workBaseRow("BASERAGE")
                        PARA43.Value = workBaseRow("BASELEASE")
                        PARA54.Value = workBaseRow("MARUKANSEAL")
                        PARA44.Value = workBaseRow("REMARK")
                        PARA45.Value = workBaseRow("DELFLG")
                        PARA46.Value = nowDate
                        PARA47.Value = nowDate
                        PARA48.Value = COA0019Session.USERID
                        PARA49.Value = HttpContext.Current.Session("APSRVname")
                        PARA50.Value = CONST_DEFAULT_RECEIVEYMD

                        SQLcmd.ExecuteNonQuery()

                        '結果 --> テーブル反映
                        workBaseRow("UPDYMD") = nowDate.ToString("yyyy-MM-dd HH:mm:ss")
                        workBaseRow("OPERATION") = ""


                        '更新ジャーナル追加
                        COA0030Journal.TABLENM = updTable
                        COA0030Journal.ACTION = "UPDATE_INSERT"

                        copyDataTable = BASEtbl.Copy
                        copyDataTable.Columns.Remove("LINECNT")
                        copyDataTable.Columns.Remove("OPERATION")
                        copyDataTable.Columns.Remove("SELECT")
                        copyDataTable.Columns.Remove("HIDDEN")
                        copyDataTable.Columns.Remove("TIMSTP")

                        COA0030Journal.ROW = copyDataTable.Rows(i)
                        COA0030Journal.COA0030SaveJournal()
                        If COA0030Journal.ERR = C_MESSAGENO.NORMAL Then
                        Else
                            CommonFunctions.ShowMessage(COA0030Journal.ERR, Me.lblFooterMessage, pageObject:=Me)
                            Return
                        End If

                        '更新結果(TIMSTP)再取得 …　連続処理を可能にする。
                        SQLStr2 = " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP, " _
                                & "   convert(nvarchar, UPDYMD , 120) as UPDYMD, " _
                                & "   rtrim(UPDUSER) as UPDUSER , rtrim(UPDTERMID) as UPDTERMID " _
                                & " FROM " & updTable _
                                & " WHERE COMPCODE = @P01 " _
                                & "   And TANKNO = @P02 " _
                                & "   And STYMD = @P03 " _
                                & " ;"

                        SQLcmd2 = New SqlCommand(SQLStr2, SQLcon)
                        With SQLcmd2.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = workBaseRow("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = workBaseRow("TANKNO")
                            .Add("@P03", System.Data.SqlDbType.Date).Value = RTrim(Convert.ToString(workBaseRow("STYMD")))
                        End With

                        SQLdr2 = SQLcmd2.ExecuteReader()

                        While SQLdr2.Read
                            workBaseRow("UPDYMD") = SQLdr2("UPDYMD")
                            workBaseRow("UPDUSER") = SQLdr2("UPDUSER")
                            workBaseRow("UPDTERMID") = SQLdr2("UPDTERMID")
                            workBaseRow("TIMSTP") = SQLdr2("TIMSTP")
                        End While

                        '初期所在登録
                        If Convert.ToInt64(workBaseRow("ACTYCNT")) <= 1 Then
                            InsertNewTankOrder(workBaseRow)
                        End If

                        'PDF更新処理
                        PDFDBupdate(Convert.ToString(workBaseRow("COMPCODE")), Convert.ToString(workBaseRow("TANKNO")), Convert.ToString(workBaseRow("APPLYID")))

                    End If

                Finally
                    'CLOSE
                    If Not SQLdr Is Nothing Then
                        SQLdr.Close()
                    End If
                    If Not SQLcmd Is Nothing Then
                        SQLcmd.Dispose()
                        SQLcmd = Nothing
                    End If
                    If Not SQLdr2 Is Nothing Then
                        SQLdr2.Close()
                    End If
                    If Not SQLcmd2 Is Nothing Then
                        SQLcmd2.Dispose()
                        SQLcmd2 = Nothing
                    End If

                End Try

            Next

        Catch ex As Exception

            Dim O_ERR As String = C_MESSAGENO.EXCEPTION
            CommonFunctions.ShowMessage(O_ERR, Me.lblFooterMessage, pageObject:=Me)

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = O_ERR
            COA0003LogFile.COA0003WriteLog()                             'ログ出力

            Return

        Finally
            'CLOSE
            If Not SQLcon Is Nothing Then
                SQLcon.Close()
                SQLcon.Dispose()
                SQLcon = Nothing
            End If

        End Try

        'GridViewデータをテーブルに保存
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'メッセージ表示
        If txtRightErrorMessage.Text = "" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
        End If

        'カーソル設定
        txtOperationEx.Focus()
    End Sub
    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞボタン押下時
    ''' </summary>
    Public Sub btnDownload_Click()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

        '■■■ 一覧表示データ復元 ■■■
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '帳票出力dll Interface
        COA0027ReportTable.MAPID = CONST_MAPID                             'PARAM01:画面ID
        COA0027ReportTable.REPORTID = lbRightList.SelectedValue.ToString   'PARAM02:帳票ID
        COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
        If Right(lbRightList.SelectedValue.ToString, 3) = "_IO" Then
            COA0027ReportTable.ADDSHEET = CONST_MAPID & "O"
        End If
        COA0027ReportTable.TBLDATA = BASEtbl                               'PARAM04:データ参照tabledata
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

    End Sub
    ''' <summary>
    ''' 一覧印刷ボタン押下時
    ''' </summary>
    Public Sub btnPrint_Click()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable        '内部テーブル
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable

        '一覧表示データ復元
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '帳票出力dll Interface
        COA0027ReportTable.MAPID = CONST_MAPID                             'PARAM01:画面ID
        COA0027ReportTable.REPORTID = lbRightList.SelectedValue.ToString   'PARAM02:帳票ID
        COA0027ReportTable.FILETYPE = "pdf"                                'PARAM03:出力ファイル形式
        COA0027ReportTable.TBLDATA = BASEtbl                               'PARAM04:データ参照tabledata
        COA0027ReportTable.COA0027ReportTable()

        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        Else
            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '別画面でPDFを表示
        hdnPrintURL.Value = COA0027ReportTable.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint()", True)

    End Sub
    ''' <summary>
    ''' 終了ボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable

        '一覧表示データ復元
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            returnCode = COA0021ListTable.ERR
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'オペレーションチェック
        Dim modDr = From item In BASEtbl Where Not {"★", ""}.Contains(Convert.ToString(item("OPERATION")))
        If modDr.Any Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, Me, submitButtonId:="btnRemarkInputOk")
            Return
        End If

        btnRemarkInputOk_Click()

    End Sub
    ''' <summary>
    ''' 終了OK押下時
    ''' </summary>
    Public Sub btnRemarkInputOk_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '自画面MAPIDより親MAP・URLを取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        If ViewState("GBM00006ITEM") Is Nothing Then
            COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
        ElseIf TypeOf ViewState("GBM00006ITEM") Is GBT00006RESULT.GBT00006RITEMS Then
            Me.Gbt00006items = DirectCast(ViewState("GBM00006ITEM"), GBT00006RESULT.GBT00006RITEMS)
            COA0011ReturnUrl.VARI = Me.Gbt00006items.Gbt00006MapVariant
        Else
            Me.Gbt00012items = DirectCast(ViewState("GBM00006ITEM"), GBT00012REPAIR.GBT00012RITEMS)
            COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
        End If

        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage)
            Return
        End If

        '次画面の変数セット
        HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
        HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL

        '画面遷移実行
        Server.Transfer(COA0011ReturnUrl.URL)

    End Sub
    ''' <summary>
    ''' 表更新ボタン押下時
    ''' </summary>
    Public Sub btnListUpdate_Click()
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable

        '初期処理
        Me.txtRightErrorMessage.Text = ""
        Me.lblFooterMessage.Text = ""
        errList = New List(Of String)
        errListAll = New List(Of String)

        '画面表示データ復元
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'DetailBoxをINPtblへ退避
        DetailBoxToINPtbl()

        'INPtbl内容 チェック
        '　※チェックOKデータをUPDtblへ格納する
        INPtblCheck()
        If returnCode <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'デフォルトをUPDtblへ退避
        BASEtblColumnsAdd(UPDtbl)
        DefaultToTBL(UPDtbl)

        'PDFタブ更新 
        PDFTabListUpdate()

        BASEtblUpdate()

        '一覧(BASEtbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        Dim DelCnt As Integer = 0
        Dim LineCnt As Integer = 1
        Dim LoopCnt As Integer = BASEtbl.Rows.Count - 1
        Dim i As Integer = 0

        Do While i <= LoopCnt - DelCnt
            If Convert.ToString(BASEtbl.Rows(i)("TIMSTP")) = "0" AndAlso Convert.ToString(BASEtbl.Rows(i)("DELFLG")) = BaseDllCommon.CONST_FLAG_YES Then
                BASEtbl.Rows(i).Delete()
                DelCnt = DelCnt + 1
            Else
                BASEtbl.Rows(i)("LINECNT") = LineCnt
                LineCnt = LineCnt + 1
                i = i + 1
            End If
        Loop

        '一覧表示データ保存
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        '画面詳細編集
        'Gridview表示書式設定

        If returnCode <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
        Else
            'detailboxクリア
            detailboxClear()
            hdnDTABChange.Value = "0"
            DetailTABChange()
            hdnDTABChange.Value = ""

            'PDF初期画面編集
            'Repeaterバインド準備
            PDFtblColumnsAdd()
            'Repeaterバインド(空明細)
            WF_DViewRepPDF.DataSource = PDFtbl
            WF_DViewRepPDF.DataBind()

            If errList.Count = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMALLISTADDED, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            End If
        End If

        BASEtbl.Clear()
        INPtbl.Clear()
        UPDtbl.Clear()
        PDFtbl.Clear()
        BASEtbl.Dispose()
        INPtbl.Dispose()
        UPDtbl.Dispose()
        PDFtbl.Dispose()

        'カーソル設定
        txtStYMD.Focus()

    End Sub
    ''' <summary>
    ''' クリアボタン押下時
    ''' </summary>
    Public Sub btnClear_Click()
        'detailboxクリア
        detailboxClear()
        hdnDTABChange.Value = "0"
        DetailTABChange()
        hdnDTABChange.Value = ""

        If returnCode <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
            Return
        Else
            'メッセージ表示
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALCLEAR, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        End If

        'カーソル設定
        txtStYMD.Focus()

    End Sub
    ''' <summary>
    ''' 先頭頁ボタン押下時
    ''' </summary>
    Public Sub btnFIRST_Click()

        'ポジションを設定するのみ
        hdnListPosition.Value = "1"

    End Sub
    ''' <summary>
    ''' 最終頁ボタン押下時
    ''' </summary>
    Public Sub btnLAST_Click()
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable

        '一覧表示データ復元 
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        'ソート
        Using dvTBLview As DataView = New DataView(BASEtbl)
            dvTBLview.RowFilter = "HIDDEN= '0'"

            'ポジションを設定するのみ
            If dvTBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
                hdnListPosition.Value = Convert.ToString(dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT))
            Else
                hdnListPosition.Value = Convert.ToString(dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT) + 1)
            End If
        End Using

    End Sub
    ''' <summary>
    ''' 一覧 マウスホイール時処理 (一覧スクロール)
    ''' </summary>
    Protected Sub ListScrole()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル

        '表示データ件数取得
        Dim DataCnt As Integer = 0                  '(絞り込み後)有効Data数

        '一覧表示データ復元
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            If Convert.ToString(BASEtbl.Rows(i)(4)) = "0" Then
                DataCnt = DataCnt + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                BASEtbl.Rows(i)("SELECT") = DataCnt
            End If
        Next

        '現在表示位置取得
        Dim ListPosition As Integer = 0
        If hdnListPosition.Value = "" Then
            ListPosition = 1
        Else
            Try
                Integer.TryParse(hdnListPosition.Value, ListPosition)
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
        Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(BASEtbl, Me.pnlListArea, CONST_DSPROWCOUNT, ListPosition, hdnListPosition)
        '一覧作成
        COA0013TableObject.MAPID = CONST_MAPID
        COA0013TableObject.VARI = Me.hdnViewId.Value
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = pnlListArea
        COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.LEVENT = "ondblclick"
        COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

    End Sub
    ''' <summary>
    ''' 入力データチェック
    ''' </summary>
    Protected Sub INPtblCheck()
        Dim dummyMsgBox As Label = New Label
        Dim errMessageStr As String = ""

        'インターフェイス初期値設定
        returnCode = C_MESSAGENO.NORMAL

        '事前準備（キー重複レコード削除）
        Dim dupCheckFields = CommonFunctions.CreateCompareFieldList({"COMPCODE", "TANKNO", "STYMD"})
        Dim drBefor As DataRow = INPtbl.NewRow
        Dim drCurrent As DataRow = INPtbl.NewRow
        For i As Integer = INPtbl.Rows.Count - 1 To 1 Step -1
            'KEY重複
            drBefor = INPtbl.Rows(i - 1)
            drCurrent = INPtbl.Rows(i)

            'If Convert.ToString(drCurrent("COMPCODE")) = Convert.ToString(drBefor("COMPCODE")) AndAlso
            '   Convert.ToString(drCurrent("TANKNO")) = Convert.ToString(drBefor("TANKNO")) AndAlso
            '   Convert.ToString(drCurrent("STYMD")) = Convert.ToString(drBefor("STYMD")) Then
            If CommonFunctions.CompareDataFields(drCurrent, drBefor, dupCheckFields) Then
                INPtbl.Rows(i).Delete()
            End If
        Next

        'チェック ＆　更新用テーブル作成
        'ポジション　＆　行レコード取得

        'タイトル区分存在チェック(Iレコード)　…　Iレコードが無ければエラー
        'ヘッダーのみも存在するのでチェックしない

        'チェック実行　-->　OK時 UPDtbl作成　…　パラメータ数を担保する必要あり(Defaultを参照)
        Dim dicField As Dictionary(Of String, String) = Nothing
        For i As Integer = 0 To INPtbl.Rows.Count - 1

            Dim workInpRow As DataRow
            workInpRow = INPtbl.NewRow
            'workInpRow.ItemArray = INPtbl.Rows(i).ItemArray
            workInpRow = INPtbl.Rows(i)

            '-----------------------------------------------------
            '   項目内整合性チェック　キー項目入力チェック
            '-----------------------------------------------------
            ListUpdateCheck(workInpRow)

            '-----------------------------------------------------
            '   日付整合性チェック
            '-----------------------------------------------------
            Dim inpDateStart As Date
            Dim inpDateEnd As Date
            Date.TryParse(Convert.ToString(workInpRow("STYMD")), inpDateStart)
            Date.TryParse(Convert.ToString(workInpRow("ENDYMD")), inpDateEnd)
            Dim errorCode As String = C_MESSAGENO.VALIDITYINPUT
            Dim errorMessage As String = ""
            CommonFunctions.ShowMessage(errorCode, dummyMsgBox)
            errorMessage = dummyMsgBox.Text

            If inpDateStart >= inpDateEnd Then

                If returnCode = C_MESSAGENO.NORMAL Then
                    'KEY重複
                    returnCode = C_MESSAGENO.RIGHTBIXOUT
                End If

                'エラーレポート編集
                errMessageStr = ""
                errMessageStr = "・" & errorMessage
                ' レコード内容を展開する
                errMessageStr = errMessageStr & Me.ErrItemSet(workInpRow)
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr
            Else
                '主キー情報重複チェック(一覧表示内容とのチェック)
                Dim compareFieldList = CommonFunctions.CreateCompareFieldList({"COMPCODE", "TANKNO"})
                Dim dr As DataRow = BASEtbl.NewRow
                For j As Integer = 0 To BASEtbl.Rows.Count - 1
                    dr.ItemArray = BASEtbl.Rows(j).ItemArray
                    'If Convert.ToString(BASEtbl.Rows(j)("DELFLG")) <> BaseDllCommon.CONST_FLAG_YES Then
                    If Convert.ToString(dr("DELFLG")) <> BaseDllCommon.CONST_FLAG_YES Then
                        '日付以外の項目が等しい
                        'If BASEtbl.Rows(j)("APPLYID") = workInpRow("APPLYID") AndAlso
                        'If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(workInpRow("COMPCODE")) AndAlso
                        '       Convert.ToString(BASEtbl.Rows(j)("TANKNO")) = Convert.ToString(workInpRow("TANKNO")) Then
                        If CommonFunctions.CompareDataFields(BASEtbl.Rows(j), workInpRow, compareFieldList) Then

                            'ENDYMDは変更扱い
                            'If Convert.ToString(BASEtbl.Rows(j)("STYMD")) = Convert.ToString(workInpRow("STYMD")) Then
                            If Convert.ToString(dr("STYMD")) = Convert.ToString(workInpRow("STYMD")) Then

                                '同一レコード
                                Exit For
                            Else

                                Dim baseDateStart As Date
                                Dim baseDateEnd As Date
                                'Date.TryParse(Convert.ToString(BASEtbl.Rows(j)("STYMD")), baseDateStart)
                                'Date.TryParse(Convert.ToString(BASEtbl.Rows(j)("ENDYMD")), baseDateEnd)
                                Date.TryParse(Convert.ToString(dr("STYMD")), baseDateStart)
                                Date.TryParse(Convert.ToString(dr("ENDYMD")), baseDateEnd)

                                ' 
                                If inpDateStart <= baseDateStart AndAlso baseDateStart <= inpDateEnd OrElse
                                           inpDateStart <= baseDateEnd AndAlso baseDateEnd <= inpDateEnd OrElse
                                           baseDateStart <= inpDateStart AndAlso inpDateStart <= baseDateEnd OrElse
                                           baseDateStart <= inpDateEnd AndAlso inpDateEnd <= baseDateEnd Then
                                    If returnCode = C_MESSAGENO.NORMAL Then
                                        'KEY重複
                                        returnCode = C_MESSAGENO.RIGHTBIXOUT
                                    End If

                                    'エラーレポート編集
                                    errMessageStr = ""
                                    errMessageStr = "・" & errorMessage
                                    ' レコード内容を展開する
                                    errMessageStr = errMessageStr & Me.ErrItemSet(workInpRow)
                                    If txtRightErrorMessage.Text <> "" Then
                                        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                                    End If
                                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr
                                    Exit For
                                End If

                            End If
                        End If
                    End If
                Next
            End If
            If returnCode <> C_MESSAGENO.NORMAL Then
                workInpRow("OPERATION") = errDisp
                errListAll.Add(C_MESSAGENO.RIGHTBIXOUT)
                errList.Add(C_MESSAGENO.RIGHTBIXOUT)
                If returnCode = C_MESSAGENO.REQUIREDVALUE OrElse returnCode = C_MESSAGENO.HASAPPLYINGRECORD Then ' 一覧反映対象外
                    workInpRow("HIDDEN") = "1"
                    returnCode = C_MESSAGENO.RIGHTBIXOUT
                End If
            End If
            INPtbl.Rows(i).ItemArray = workInpRow.ItemArray
        Next

        Dim refErrMessage As String = Nothing
        Dim rtCode As String = returnCode
        If WF_DViewRepPDF.Items.Count > 0 Then
            For j As Integer = 0 To WF_DViewRepPDF.Items.Count - 1

                Dim dltFlg As String = DirectCast(WF_DViewRepPDF.Items(j).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Text
                Dim fileNm As String = DirectCast(WF_DViewRepPDF.Items(j).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text

                '削除フラグ
                SetDelFlgListItem(dltFlg)
                ChedckList(dltFlg, lbDelFlg, refErrMessage)
                If returnCode <> C_MESSAGENO.NORMAL Then
                    If txtRightErrorMessage.Text <> "" Then
                        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                    End If
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & "Delete" & ":" & dltFlg & ")" & ControlChars.NewLine _
                                           & "  --> FILE NAME    =     " & fileNm

                    returnCode = C_MESSAGENO.RIGHTBIXOUT
                    errListAll.Add(C_MESSAGENO.RIGHTBIXOUT)
                    errList.Add(C_MESSAGENO.RIGHTBIXOUT)
                End If
            Next
            If rtCode <> C_MESSAGENO.NORMAL Then
                returnCode = rtCode
            End If
        End If

    End Sub
    ''' <summary>
    ''' エラーキー情報出力
    ''' </summary>
    ''' <param name="argRow"></param>
    ''' <returns></returns>
    Private Function ErrItemSet(ByVal argRow As DataRow) As String
        Dim rtc As String = String.Empty

        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
            rtc &= ControlChars.NewLine & "  --> COMPANY CODE    =" & Convert.ToString(argRow("COMPCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> TANK NO         =" & Convert.ToString(argRow("TANKNO")) & " , "
            rtc &= ControlChars.NewLine & "  --> EFFECTIVE(FROM) =" & Convert.ToString(argRow("STYMD")) & " , "
            rtc &= ControlChars.NewLine & "  --> DELETE FLG      =" & Convert.ToString(argRow("DELFLG")) & " "
        Else
            rtc &= ControlChars.NewLine & "  --> 会社コード      =" & Convert.ToString(argRow("COMPCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> タンク番号      =" & Convert.ToString(argRow("TANKNO")) & " , "
            rtc &= ControlChars.NewLine & "  --> 有効日(From)    =" & Convert.ToString(argRow("STYMD")) & " , "
            rtc &= ControlChars.NewLine & "  --> 削除FLG         =" & Convert.ToString(argRow("DELFLG")) & " "
        End If

        Return rtc

    End Function
    ''' <summary>
    ''' 左ボックス選択ボタン押下時
    ''' </summary>
    Public Sub btnLeftBoxButtonSel_Click()
        Dim targetObject As Control = Nothing

        Dim dicKey As String = ""
        Dim valRepText As String = ""
        Dim txtRepText As String = ""

        '現在表示している左ビューを取得
        Dim activeViewObj As View = Me.mvLeft.GetActiveView
        If activeViewObj IsNot Nothing Then
            Select Case activeViewObj.ID
                'ビューごとの処理はケースを追加で実現
                Case Me.vLeftOperation.ID 'アクティブなビューがオペレーション
                    'オペレーション選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbOperation.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbOperation.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftCompCode.ID 'アクティブなビューが会社コード
                    '会社コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCompCode.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbCompCode.SelectedItem.Value
                            Me.lblCompCodeText.Text = Me.lbCompCode.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblCompCodeText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftProperty.ID 'アクティブなビューが所属
                    '所属選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbProperty.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbProperty.SelectedItem.Value
                            Me.lblPropertyText.Text = Me.lbProperty.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblPropertyText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftLMOF.ID 'アクティブなビューが所有形態（自社、リース他）
                    '所有形態（自社、リース他）選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター有効フラグ
                    If Me.lbLMOF.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbLMOF.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_1"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbLMOF.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1").Focus()
                        'リピーター設定は関数末
                        dicKey = "LMOF"
                        valRepText = Me.lbLMOF.SelectedItem.Value
                        txtRepText = Me.lbLMOF.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftLeaseStat.ID 'アクティブなビューがリース
                    'リース選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーターリース
                    If Me.lbLeaseStat.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbLeaseStat.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_1"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbLeaseStat.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1").Focus()
                        'リピーター設定は関数末
                        dicKey = "LEASESTAT"
                        valRepText = Me.lbLeaseStat.SelectedItem.Value
                        txtRepText = Me.lbLeaseStat.SelectedItem.Text
                    End If
                    'End If
                Case Me.vLeftRepairStat.ID 'アクティブなビューがリペア
                    'リペア選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーターリペア
                    If Me.lbRepairStat.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbRepairStat.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_1"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbRepairStat.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1").Focus()
                        'リピーター設定は関数末
                        dicKey = "REPAIRSTAT"
                        valRepText = Me.lbRepairStat.SelectedItem.Value
                        txtRepText = Me.lbRepairStat.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftJapFireApproved.ID 'アクティブなビューがJP消防検査有無
                    'JP消防検査有無選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーターJP消防検査有無
                    If Me.lbJapFireApproved.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1"),
                        '    System.Web.UI.WebControls.TextBox).Text = Me.lbJapFireApproved.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_1"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbJapFireApproved.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1").Focus()
                        'リピーター設定は関数末
                        dicKey = "JAPFIREAPPROVED"
                        valRepText = Me.lbJapFireApproved.SelectedItem.Value
                        txtRepText = Me.lbJapFireApproved.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftManufacture.ID 'アクティブなビューが製造メーカー
                    '製造メーカー選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター製造メーカー
                    If Me.lbManufacture.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbManufacture.SelectedItem.Value
                        ''DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_1"),
                        ''    System.Web.UI.WebControls.Label).Text = Me.lbManufacture.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1").Focus()
                        'リピーター設定は関数末
                        dicKey = "MANUFACTURER"
                        valRepText = Me.lbManufacture.SelectedItem.Value
                        txtRepText = ""
                    End If
                    'End If

                Case Me.vLeftStruct.ID 'アクティブなビューが追加構造
                    '追加構造選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター追加構造
                    If Me.lbStruct.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbStruct.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_1"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbStruct.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1").Focus()
                        'リピーター設定は関数末
                        dicKey = "STRUCT"
                        valRepText = Me.lbStruct.SelectedItem.Value
                        txtRepText = Me.lbStruct.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftUsDotApproved.ID 'アクティブなビューが荷重試験実施の有無
                    '荷重試験実施の有無選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター荷重試験実施の有無
                    If Me.lbUsDotApproved.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbUsDotApproved.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_1"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbUsDotApproved.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1").Focus()
                        'リピーター設定は関数末
                        dicKey = "USDOTAPPROVED"
                        valRepText = Me.lbUsDotApproved.SelectedItem.Value
                        txtRepText = Me.lbUsDotApproved.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftDischarge.ID 'アクティブなビューが液出し口の位置
                    '液出し口の位置選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター液出し口の位置
                    If Me.lbDischarge.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbDischarge.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbDischarge.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        'リピーター設定は関数末
                        dicKey = "DISCHARGE"
                        valRepText = Me.lbDischarge.SelectedItem.Value
                        txtRepText = Me.lbDischarge.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftFootValue.ID 'アクティブなビューがフート弁の仕様
                    'フート弁の仕様選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーターフート弁の仕様
                    If Me.lbFootValue.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbFootValue.SelectedItem.Value
                        ''DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                        ''    System.Web.UI.WebControls.Label).Text = Me.lbFootValue.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        'リピーター設定は関数末
                        dicKey = "FOOTVALUETYPE"
                        valRepText = Me.lbFootValue.SelectedItem.Value
                        txtRepText = ""
                    End If
                    'End If

                Case Me.vLeftBottomOutlet.ID 'アクティブなビューが下部液出し口の仕様
                    '下部液出し口の仕様選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター下部液出し口の仕様
                    If Me.lbBottomOutlet.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbBottomOutlet.SelectedItem.Value
                        ''DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                        ''    System.Web.UI.WebControls.Label).Text = Me.lbBottomOutlet.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        'リピーター設定は関数末
                        dicKey = "BACKVALUETYPE"
                        valRepText = Me.lbBottomOutlet.SelectedItem.Value
                        txtRepText = ""
                    End If
                    'End If

                Case Me.vLeftTopOutlet.ID 'アクティブなビューが上部積込口の仕様
                    '上部積込口の仕様選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター上部積込口の仕様
                    If Me.lbTopOutlet.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbTopOutlet.SelectedItem.Value
                        ''DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                        ''    System.Web.UI.WebControls.Label).Text = Me.lbTopOutlet.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        'リピーター設定は関数末
                        dicKey = "TOPDISVALUETYPE"
                        valRepText = Me.lbTopOutlet.SelectedItem.Value
                        txtRepText = ""
                    End If
                    'End If

                Case Me.vLeftAirInlet.ID 'アクティブなビューがエアラインのバルブの仕様
                    'エアラインのバルブの仕様選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーターエアラインのバルブの仕様
                    If Me.lbAirInlet.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbAirInlet.SelectedItem.Value
                        ''DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                        ''    System.Web.UI.WebControls.Label).Text = Me.lbAirInlet.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        'リピーター設定は関数末
                        dicKey = "AIRINLETVALUE"
                        valRepText = Me.lbAirInlet.SelectedItem.Value
                        txtRepText = ""
                    End If
                    'End If

                Case Me.vLeftBaffles.ID 'アクティブなビューが防波板の有無
                    '防波板の有無選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター防波板の有無
                    If Me.lbBaffles.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbBaffles.SelectedItem.Value
                        DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbBaffles.SelectedItem.Text
                        WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        'リピーター設定は関数末
                        dicKey = "BAFFLES"
                        valRepText = Me.lbBaffles.SelectedItem.Value
                        txtRepText = Me.lbBaffles.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftBurstDisc.ID 'アクティブなビューが破裂板の有無
                    '破裂板の有無選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター破裂板の有無
                    If Me.lbBurstDisc.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbBurstDisc.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbBurstDisc.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        'リピーター設定は関数末
                        dicKey = "BURSTDISCFITTED"
                        valRepText = Me.lbBurstDisc.SelectedItem.Value
                        txtRepText = Me.lbBurstDisc.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftTherm.ID 'アクティブなビューが温度計の種類
                    '温度計の種類選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーター温度計の種類
                    If Me.lbTherm.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbTherm.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbTherm.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        'リピーター設定は関数末
                        dicKey = "TYPEOFTHERM"
                        valRepText = Me.lbTherm.SelectedItem.Value
                        txtRepText = Me.lbTherm.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftMlSeal.ID 'アクティブなビューがマンホールパッキンの種類
                    'マンホールパッキンの種類選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーターマンホールパッキンの種類
                    If Me.lbMlSeal.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_3"),
                        '        System.Web.UI.WebControls.TextBox).Text = Me.lbMlSeal.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_3"),
                        '        System.Web.UI.WebControls.Label).Text = Me.lbMlSeal.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_3").Focus()
                        'リピーター設定は関数末
                        dicKey = "TYPEOFMLSEAL"
                        valRepText = Me.lbMlSeal.SelectedItem.Value
                        txtRepText = Me.lbMlSeal.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftMarukanSticker.ID 'アクティブなビューがマル関ステッカー貼付
                    'マル関ステッカー貼付選択時
                    'targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    'If targetObject IsNot Nothing Then
                    'Else
                    'リピーターマル関ステッカー貼付
                    If Me.lbMarukanSticker.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_3"),
                        '    System.Web.UI.WebControls.TextBox).Text = Me.lbMarukanSticker.SelectedItem.Value
                        'DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_3"),
                        '    System.Web.UI.WebControls.Label).Text = Me.lbMarukanSticker.SelectedItem.Text
                        'WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_3").Focus()
                        'リピーター設定は関数末
                        dicKey = "MARUKANSEAL"
                        valRepText = Me.lbMarukanSticker.SelectedItem.Value
                        txtRepText = Me.lbMarukanSticker.SelectedItem.Text
                    End If
                    'End If

                Case Me.vLeftNewTankPort.ID 'アクティブなビューがNew Tank Port
                    'リピーターNew Tank Port
                    If Me.lbNewTankPort.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        dicKey = "NEWTANKPORT"
                        valRepText = Me.lbNewTankPort.SelectedItem.Value
                        txtRepText = Me.lbNewTankPort.SelectedItem.Text
                    End If

                Case Me.vLeftNewTankActy.ID 'アクティブなビューがNew Tank Acty
                    'リピーターNew Tank Acty
                    If Me.lbNewTankActy.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                        dicKey = "NEWTANKACTY"
                        valRepText = Me.lbNewTankActy.SelectedItem.Value
                        txtRepText = Me.lbNewTankActy.SelectedItem.Text
                    End If

                Case Me.vLeftDelFlg.ID 'アクティブなビューが削除フラグ
                    '削除フラグ選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbDelFlg.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbDelFlg.SelectedItem.Value
                            Me.lblDelFlgText.Text = Me.lbDelFlg.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblDelFlgText.Text = ""
                            txtobj.Focus()
                        End If
                    Else
                        'リピーター削除フラグ
                        Dim IntCnt As Integer = Nothing
                        If Me.lbDelFlg.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            If Integer.TryParse(Me.hdnTextDbClickField.Value, IntCnt) Then
                                DirectCast(WF_DViewRepPDF.Items(IntCnt).FindControl("WF_Rep_DELFLG"),
                                          System.Web.UI.WebControls.TextBox).Text = Me.lbDelFlg.SelectedItem.Value
                                WF_DViewRepPDF.Items(Integer.Parse(hdnTextDbClickField.Value)).FindControl("WF_Rep_DELFLG").Focus()
                            End If
                        End If
                    End If
                Case Me.vLeftCal.ID 'アクティブなビューがカレンダー
                    'カレンダー選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        txtobj.Text = Me.hdnCalendarValue.Value
                        txtobj.Focus()
                    Else
                        'リピーター
                        If Me.hdnCalendarValue.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1"),
                                System.Web.UI.WebControls.TextBox).Text = Me.hdnCalendarValue.Value
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_1").Focus()

                            If Me.hdnTextDbClickField.Value = "3" OrElse Me.hdnTextDbClickField.Value = "4" Then
                                INSPECTDATE2P5_Change()
                            End If

                        End If
                    End If
                Case Else
                    '何もしない
            End Select
        End If

        'リピータ設定
        If valReptext <> "" Then
            'Detail ポジション取得
            Dim dicDetailColMap As New Dictionary(Of String, List(Of String))
            Dim COA0015ProfViewD As New BASEDLL.COA0015ProfViewD        'UPROFview・Detail取得
            COA0015ProfViewD.MAPID = CONST_MAPID
            COA0015ProfViewD.VARI = Me.hdnViewId.Value
            COA0015ProfViewD.TAB = ""
            COA0015ProfViewD.COA0015ProfViewD()
            If COA0015ProfViewD.ERR = C_MESSAGENO.NORMAL Then
                dicDetailColMap = COA0015ProfViewD.DMAPDIC
            Else
                'エラー処理
                CommonFunctions.ShowMessage(COA0015ProfViewD.ERR, Me.lblFooterMessage)
                Return
            End If

            Dim posRepRow As Integer = (CInt(dicDetailColMap(dicKey)(2)) - 1)
            Dim posRepCol As String = dicDetailColMap(dicKey)(1)
            DirectCast(WF_DViewRep1.Items(posRepRow).FindControl("WF_Rep1_VALUE_" & posRepCol),
                                System.Web.UI.WebControls.TextBox).Text = valRepText
            DirectCast(WF_DViewRep1.Items(posRepRow).FindControl("WF_Rep1_VALUE_TEXT_" & posRepCol),
                                System.Web.UI.WebControls.Label).Text = txtRepText
            WF_DViewRep1.Items(posRepRow).FindControl("WF_Rep1_VALUE_" & posRepCol).Focus()

        End If

        '画面左サイドボックス非表示は、画面JavaScriptで実行
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
        '画面左サイドボックス非表示は、画面JavaScriptで実行
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
        AddLangSetting(dicDisplayText, Me.lblOperationEx, "操作", "Operation")
        AddLangSetting(dicDisplayText, Me.lblTankNoEx, "タンク番号", "Tank No")

        AddLangSetting(dicDisplayText, Me.btnExtract, "絞り込み", "Search")
        AddLangSetting(dicDisplayText, Me.btnDbUpdate, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnDownload, "ﾃﾞｰﾀﾀﾞｳﾝﾛｰﾄﾞ", "Data Download")
        AddLangSetting(dicDisplayText, Me.btnPrint, "一覧印刷", "Print")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnListUpdate, "表更新", "ListUpdate")
        AddLangSetting(dicDisplayText, Me.btnClear, "クリア", "Clear")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblLineCnt, "選択No", "Select No")
        AddLangSetting(dicDisplayText, Me.lblApplyID, "申請ID", "Apply ID")
        AddLangSetting(dicDisplayText, Me.lblCompCode, "会社コード", "Company Code")
        AddLangSetting(dicDisplayText, Me.lblProperty, "所属", "Property")
        AddLangSetting(dicDisplayText, Me.lblTankNo, "タンク番号", "Tank No")
        AddLangSetting(dicDisplayText, Me.lblYMD, "有効年月日", "Effective Date")
        AddLangSetting(dicDisplayText, Me.lblDelFlg, "削除", "Delete")

        AddLangSetting(dicDisplayText, Me.WF_Rep2_Desc, "添付書類を登録する場合は、ここにドロップすること", "To register attached documents, drop it here")
        AddLangSetting(dicDisplayText, Me.WF_Rep2_PDFfileName, "ファイル名", "File Name")
        AddLangSetting(dicDisplayText, Me.WF_Rep2_Delete, "削 除", "Delete")

        AddLangSetting(dicDisplayText, Me.hdnUploadMessage01, "ファイルアップロード開始", "Start uploading files")
        AddLangSetting(dicDisplayText, Me.hdnUploadError01, "ファイルアップロードが失敗しました。", "File upload failed.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError02, "通信を中止しました。", "Communication was canceled.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError03, "タイムアウトエラーが発生しました。", "A timeout Error occurred.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError04, "更新権限がありません。", "You Do Not have update permission.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError05, "対応外のファイル形式です。", "It Is an incompatible file format.")

        AddLangSetting(dicDisplayText, Me.lblDtabTank, "タンク情報", "Tank Info")
        AddLangSetting(dicDisplayText, Me.lblDtabDocument, "書類（PDF）", "Documents(PDF)")

        Me.SetDisplayLangObjects(dicDisplayText, lang)

    End Sub
    '''' <summary>
    '''' LangSetting関数で利用する文言設定ディクショナリ作成関数
    '''' </summary>
    '''' <param name="dicDisplayText">対象ディクショナリオブジェクト</param>
    '''' <param name="obj">オブジェクト</param>
    '''' <param name="jaText">日本語文言</param>
    '''' <param name="enText">英語文言</param>
    'Private Sub AddLangSetting(ByRef dicDisplayText As Dictionary(Of Control, Dictionary(Of String, String)),
    '                           ByVal obj As Control, ByVal jaText As String, enText As String)
    '    dicDisplayText.Add(obj,
    '                       New Dictionary(Of String, String) _
    '                       From {{C_LANG.JA, jaText}, {C_LANG.EN, enText}})
    'End Sub
    ''' <summary>
    ''' 内部テーブルカラム設定
    ''' </summary>
    ''' <param name="table">内部テーブル</param>
    Protected Sub BASEtblColumnsAdd(table As DataTable)

        'DB項目クリア
        If table.Columns.Count = 0 Then
        Else
            table.Columns.Clear()
        End If
        table.Clear()

        '共通項目
        table.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        table.Columns("LINECNT").DefaultValue = 0
        table.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        table.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        table.Columns("TIMSTP").DefaultValue = "0"
        table.Columns.Add("Select", GetType(Integer))             'DBの固定フィールド
        table.Columns("Select").DefaultValue = "0"
        table.Columns.Add("HIDDEN", GetType(Integer))             'DBの固定フィールド
        table.Columns("HIDDEN").DefaultValue = "0"

        '画面固有項目
        table.Columns.Add("APPLYID", GetType(String))
        table.Columns("APPLYID").DefaultValue = ""
        table.Columns.Add("COMPCODE", GetType(String))
        table.Columns("COMPCODE").DefaultValue = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
        table.Columns.Add("TANKNO", GetType(String))
        table.Columns("TANKNO").DefaultValue = ""
        table.Columns.Add("STYMD", GetType(String))
        table.Columns("STYMD").DefaultValue = ""
        table.Columns.Add("ENDYMD", GetType(String))
        table.Columns("ENDYMD").DefaultValue = ""
        table.Columns.Add("PROPERTY", GetType(String))
        table.Columns("PROPERTY").DefaultValue = ""
        table.Columns.Add("LMOF", GetType(String))
        table.Columns("LMOF").DefaultValue = ""
        table.Columns.Add("LEASESTAT", GetType(String))
        table.Columns("LEASESTAT").DefaultValue = ""
        table.Columns.Add("REPAIRSTAT", GetType(String))
        table.Columns("REPAIRSTAT").DefaultValue = ""
        table.Columns.Add("INSPECTDATE5", GetType(String))
        table.Columns("INSPECTDATE5").DefaultValue = ""
        table.Columns.Add("INSPECTDATE2P5", GetType(String))
        table.Columns("INSPECTDATE2P5").DefaultValue = ""
        table.Columns.Add("NEXTINSPECTDATE", GetType(String))
        table.Columns("NEXTINSPECTDATE").DefaultValue = ""
        table.Columns.Add("NEXTINSPECTTYPE", GetType(String))
        table.Columns("NEXTINSPECTTYPE").DefaultValue = ""
        table.Columns.Add("JAPFIREAPPROVED", GetType(String))
        table.Columns("JAPFIREAPPROVED").DefaultValue = ""
        table.Columns.Add("MANUFACTURER", GetType(String))
        table.Columns("MANUFACTURER").DefaultValue = ""
        table.Columns.Add("MANUFACTURESERIALNO", GetType(String))
        table.Columns("MANUFACTURESERIALNO").DefaultValue = ""
        table.Columns.Add("DATEOFMANUFACTURE", GetType(String))
        table.Columns("DATEOFMANUFACTURE").DefaultValue = ""
        table.Columns.Add("MATERIAL", GetType(String))
        table.Columns("MATERIAL").DefaultValue = ""
        table.Columns.Add("STRUCT", GetType(String))
        table.Columns("STRUCT").DefaultValue = "S"
        table.Columns.Add("USDOTAPPROVED", GetType(String))
        table.Columns("USDOTAPPROVED").DefaultValue = ""
        table.Columns.Add("NOMINALCAPACITY", GetType(String))
        table.Columns("NOMINALCAPACITY").DefaultValue = "0"
        table.Columns.Add("TANKCAPACITY", GetType(String))
        table.Columns("TANKCAPACITY").DefaultValue = "0"
        table.Columns.Add("MAXGROSSWEIGHT", GetType(String))
        table.Columns("MAXGROSSWEIGHT").DefaultValue = "0"
        table.Columns.Add("NETWEIGHT", GetType(String))
        table.Columns("NETWEIGHT").DefaultValue = "0"
        table.Columns.Add("MEASUREMENT", GetType(String))
        table.Columns("MEASUREMENT").DefaultValue = ""
        table.Columns.Add("FREAMDIMENSION_H", GetType(String))
        table.Columns("FREAMDIMENSION_H").DefaultValue = ""
        table.Columns.Add("FREAMDIMENSION_W", GetType(String))
        table.Columns("FREAMDIMENSION_W").DefaultValue = ""
        table.Columns.Add("FREAMDIMENSION_L", GetType(String))
        table.Columns("FREAMDIMENSION_L").DefaultValue = ""
        table.Columns.Add("HEATING", GetType(String))
        table.Columns("HEATING").DefaultValue = ""
        table.Columns.Add("HEATING_SUB", GetType(String))
        table.Columns("HEATING_SUB").DefaultValue = ""
        table.Columns.Add("DISCHARGE", GetType(String))
        table.Columns("DISCHARGE").DefaultValue = ""
        table.Columns.Add("NOOFBOTTMCLOSURES", GetType(String))
        table.Columns("NOOFBOTTMCLOSURES").DefaultValue = ""
        table.Columns.Add("IMCOCLASS", GetType(String))
        table.Columns("IMCOCLASS").DefaultValue = ""
        table.Columns.Add("FOOTVALUETYPE", GetType(String))
        table.Columns("FOOTVALUETYPE").DefaultValue = ""
        table.Columns.Add("BACKVALUETYPE", GetType(String))
        table.Columns("BACKVALUETYPE").DefaultValue = ""
        table.Columns.Add("TOPDISVALUETYPE", GetType(String))
        table.Columns("TOPDISVALUETYPE").DefaultValue = ""
        table.Columns.Add("AIRINLETVALUE", GetType(String))
        table.Columns("AIRINLETVALUE").DefaultValue = ""
        table.Columns.Add("BAFFLES", GetType(String))
        table.Columns("BAFFLES").DefaultValue = ""
        table.Columns.Add("TYPEOFPREVACVALUE", GetType(String))
        table.Columns("TYPEOFPREVACVALUE").DefaultValue = ""
        table.Columns.Add("BURSTDISCFITTED", GetType(String))
        table.Columns("BURSTDISCFITTED").DefaultValue = ""
        table.Columns.Add("TYPEOFTHERM", GetType(String))
        table.Columns("TYPEOFTHERM").DefaultValue = ""
        table.Columns.Add("TYPEOFMANLID", GetType(String))
        table.Columns("TYPEOFMANLID").DefaultValue = ""
        table.Columns.Add("TYPEOFMANLID_CENTER", GetType(String))
        table.Columns("TYPEOFMANLID_CENTER").DefaultValue = "500"
        table.Columns.Add("TYPEOFMANLID_FRONT", GetType(String))
        table.Columns("TYPEOFMANLID_FRONT").DefaultValue = ""
        table.Columns.Add("TYPEOFMLSEAL", GetType(String))
        table.Columns("TYPEOFMLSEAL").DefaultValue = "A"
        table.Columns.Add("WORKINGPRESSURE", GetType(String))
        table.Columns("WORKINGPRESSURE").DefaultValue = ""
        table.Columns.Add("TESTPRESSURE", GetType(String))
        table.Columns("TESTPRESSURE").DefaultValue = ""
        table.Columns.Add("REMARK1", GetType(String))
        table.Columns("REMARK1").DefaultValue = ""
        table.Columns.Add("REMARK2", GetType(String))
        table.Columns("REMARK2").DefaultValue = ""
        table.Columns.Add("FAULTS", GetType(String))
        table.Columns("FAULTS").DefaultValue = ""
        table.Columns.Add("BASERAGEYY", GetType(String))
        table.Columns("BASERAGEYY").DefaultValue = ""
        table.Columns.Add("BASERAGEMM", GetType(String))
        table.Columns("BASERAGEMM").DefaultValue = ""
        table.Columns.Add("BASERAGE", GetType(String))
        table.Columns("BASERAGE").DefaultValue = ""
        table.Columns.Add("BASELEASE", GetType(String))
        table.Columns("BASELEASE").DefaultValue = ""
        table.Columns.Add("MARUKANSEAL", GetType(String))
        table.Columns("MARUKANSEAL").DefaultValue = ""
        table.Columns.Add("REMARK", GetType(String))
        table.Columns("REMARK").DefaultValue = ""
        table.Columns.Add("DELFLG", GetType(String))
        table.Columns("DELFLG").DefaultValue = ""
        table.Columns.Add("UPDYMD", GetType(String))
        table.Columns("UPDYMD").DefaultValue = ""
        table.Columns.Add("UPDUSER", GetType(String))
        table.Columns("UPDUSER").DefaultValue = ""
        table.Columns.Add("UPDTERMID", GetType(String))
        table.Columns("UPDTERMID").DefaultValue = ""

        table.Columns.Add("ORDERNO", GetType(String))
        table.Columns("ORDERNO").DefaultValue = ""
        table.Columns.Add("NEWTANKCOUNTRY", GetType(String))
        table.Columns("NEWTANKCOUNTRY").DefaultValue = ""
        table.Columns.Add("NEWTANKPORT", GetType(String))
        table.Columns("NEWTANKPORT").DefaultValue = ""
        table.Columns.Add("NEWTANKACTY", GetType(String))
        table.Columns("NEWTANKACTY").DefaultValue = ""
        table.Columns.Add("ACTYCNT", GetType(String))
        table.Columns("ACTYCNT").DefaultValue = 0

        For Each col As DataColumn In table.Columns
            If col.DataType = GetType(String) AndAlso
                col.DefaultValue Is DBNull.Value Then

                col.DefaultValue = ""
            End If
        Next

    End Sub
    ''' <summary>
    ''' 内部テーブルRow初期値設定
    ''' </summary>
    ''' <param name="argTbl">内部テーブル</param>
    Protected Sub DefaultToTBL(argTbl As DataTable)

        Dim workRow As DataRow                                      'デフォルト用のロウデータ

        workRow = argTbl.NewRow
        workRow("LINECNT") = 0                                      'DBの固定フィールド
        workRow("OPERATION") = ""                                   'DBの固定フィールド
        workRow("TIMSTP") = "0"                                     'DBの固定フィールド
        workRow("Select") = "0"                                     'DBの固定フィールド
        workRow("HIDDEN") = "0"                                     'DBの固定フィールド

        workRow("APPLYID") = ""
        workRow("COMPCODE") = ""
        workRow("TANKNO") = ""
        workRow("STYMD") = ""
        workRow("ENDYMD") = ""
        workRow("PROPERTY") = ""
        workRow("LMOF") = ""
        workRow("LEASESTAT") = ""
        workRow("REPAIRSTAT") = ""
        workRow("INSPECTDATE5") = ""
        workRow("INSPECTDATE2P5") = ""
        workRow("NEXTINSPECTDATE") = ""
        workRow("NEXTINSPECTTYPE") = ""
        workRow("JAPFIREAPPROVED") = ""
        workRow("MANUFACTURER") = ""
        workRow("MANUFACTURESERIALNO") = ""
        workRow("DATEOFMANUFACTURE") = ""
        workRow("MATERIAL") = ""
        workRow("STRUCT") = ""
        workRow("USDOTAPPROVED") = ""
        workRow("NOMINALCAPACITY") = "0"
        workRow("TANKCAPACITY") = "0"
        workRow("MAXGROSSWEIGHT") = "0"
        workRow("NETWEIGHT") = "0"
        workRow("MEASUREMENT") = ""
        workRow("FREAMDIMENSION_H") = ""
        workRow("FREAMDIMENSION_W") = ""
        workRow("FREAMDIMENSION_L") = ""
        workRow("HEATING") = ""
        workRow("HEATING_SUB") = ""
        workRow("DISCHARGE") = ""
        workRow("NOOFBOTTMCLOSURES") = ""
        workRow("IMCOCLASS") = ""
        workRow("FOOTVALUETYPE") = ""
        workRow("BACKVALUETYPE") = ""
        workRow("TOPDISVALUETYPE") = ""
        workRow("AIRINLETVALUE") = ""
        workRow("BAFFLES") = ""
        workRow("TYPEOFPREVACVALUE") = ""
        workRow("BURSTDISCFITTED") = ""
        workRow("TYPEOFTHERM") = ""
        workRow("TYPEOFMANLID") = ""
        workRow("TYPEOFMANLID_CENTER") = ""
        workRow("TYPEOFMANLID_FRONT") = ""
        workRow("TYPEOFMLSEAL") = ""
        workRow("WORKINGPRESSURE") = ""
        workRow("TESTPRESSURE") = ""
        workRow("REMARK1") = ""
        workRow("REMARK2") = ""
        workRow("FAULTS") = ""
        workRow("BASERAGEYY") = ""
        workRow("BASERAGEMM") = ""
        workRow("BASERAGE") = ""
        workRow("BASELEASE") = ""
        workRow("MARUKANSEAL") = ""
        workRow("REMARK") = ""
        workRow("DELFLG") = ""
        workRow("UPDYMD") = ""

        workRow("ORDERNO") = ""
        workRow("NEWTANKCOUNTRY") = ""
        workRow("NEWTANKPORT") = ""
        workRow("NEWTANKACTY") = ""
        workRow("ACTYCNT") = 0
        argTbl.Rows.Add(workRow)

    End Sub
    ''' <summary>
    ''' detailbox 編集内容→INPtbl 
    ''' </summary>
    Protected Sub DetailBoxToINPtbl()
        Dim COA0014DetailView As New BASEDLL.COA0014DetailView
        Dim COA0015ProfViewD As New BASEDLL.COA0015ProfViewD        'UPROFview・Detail取得
        Dim workRow As DataRow
        Dim sameDr As EnumerableRowCollection(Of DataRow)

        'Detail変数設定
        BASEtblColumnsAdd(INPtbl)
        Dim WW_DetailMAX As Integer = 0

        '日付変換用
        Dim pDate As Date = Nothing

        'Detail取り込み用テーブル作成
        COA0015ProfViewD.MAPID = CONST_MAPID
        COA0015ProfViewD.VARI = Me.hdnViewId.Value
        COA0015ProfViewD.TAB = ""
        COA0015ProfViewD.COA0015ProfViewD()
        If COA0015ProfViewD.ERR = C_MESSAGENO.NORMAL Then
            WW_DetailMAX = WF_DViewRep1.Items.Count \ COA0015ProfViewD.SEQMAX
        Else
            'エラー処理
            CommonFunctions.ShowMessage(COA0015ProfViewD.ERR, Me.lblFooterMessage)
            Return
        End If

        For i As Integer = 0 To WW_DetailMAX - 1
            workRow = INPtbl.NewRow

            'Jap Fire 対応(全項目更新でない場合の編集対象外のデータ埋め)
            Dim tankNo As String = txtTankNo.Text
            Dim stYmd As String = txtStYMD.Text
            If Date.TryParseExact(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, pDate) Then
                stYmd = pDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParseExact(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT.Replace("dd", "d").Replace("MM", "M"), Nothing, Nothing, pDate) Then
                stYmd = pDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParse(txtStYMD.Text, pDate) Then
                stYmd = pDate.ToString("yyyy/MM/dd")
            Else
                stYmd = txtStYMD.Text
            End If
            sameDr = (From item In BASEtbl Where item("TANKNO").Equals(tankNo) AndAlso item("STYMD").Equals(stYmd))
            If sameDr.Any Then
                workRow.ItemArray = sameDr(0).ItemArray
            Else
                workRow("LMOF") = ""
                workRow("LEASESTAT") = ""
                workRow("REPAIRSTAT") = ""
                workRow("INSPECTDATE5") = ""
                workRow("INSPECTDATE2P5") = ""
                workRow("NEXTINSPECTDATE") = ""
                workRow("NEXTINSPECTTYPE") = ""
                workRow("JAPFIREAPPROVED") = ""
                workRow("MANUFACTURER") = ""
                workRow("MANUFACTURESERIALNO") = ""
                workRow("DATEOFMANUFACTURE") = ""
                workRow("MATERIAL") = ""
                workRow("STRUCT") = ""
                workRow("USDOTAPPROVED") = ""
                workRow("NOMINALCAPACITY") = "0"
                workRow("TANKCAPACITY") = "0"
                workRow("MAXGROSSWEIGHT") = "0"
                workRow("NETWEIGHT") = "0"
                workRow("MEASUREMENT") = ""
                workRow("FREAMDIMENSION_H") = ""
                workRow("FREAMDIMENSION_W") = ""
                workRow("FREAMDIMENSION_L") = ""
                workRow("HEATING") = ""
                workRow("HEATING_SUB") = ""
                workRow("DISCHARGE") = ""
                workRow("NOOFBOTTMCLOSURES") = ""
                workRow("IMCOCLASS") = ""
                workRow("FOOTVALUETYPE") = ""
                workRow("BACKVALUETYPE") = ""
                workRow("TOPDISVALUETYPE") = ""
                workRow("AIRINLETVALUE") = ""
                workRow("BAFFLES") = ""
                workRow("TYPEOFPREVACVALUE") = ""
                workRow("BURSTDISCFITTED") = ""
                workRow("TYPEOFTHERM") = ""
                workRow("TYPEOFMANLID") = ""
                workRow("TYPEOFMANLID_CENTER") = ""
                workRow("TYPEOFMANLID_FRONT") = ""
                workRow("TYPEOFMLSEAL") = ""
                workRow("WORKINGPRESSURE") = ""
                workRow("TESTPRESSURE") = ""
                workRow("REMARK1") = ""
                workRow("REMARK2") = ""
                workRow("FAULTS") = ""
                workRow("BASERAGEYY") = ""
                workRow("BASERAGEMM") = ""
                workRow("BASERAGE") = ""
                workRow("BASELEASE") = ""
                workRow("MARUKANSEAL") = ""
                workRow("REMARK") = ""
                workRow("UPDYMD") = ""
                workRow("ORDERNO") = ""
                workRow("NEWTANKCOUNTRY") = ""
                workRow("NEWTANKPORT") = ""
                workRow("NEWTANKACTY") = ""
                workRow("ACTYCNT") = 0
            End If
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            If (String.IsNullOrEmpty(lblLineCntText.Text)) Then
                workRow("LINECNT") = 0
            Else
                workRow("LINECNT") = CType(lblLineCntText.Text, Integer)
            End If
            workRow("OPERATION") = ""
            workRow("TIMSTP") = "0"
            workRow("Select") = 1
            workRow("HIDDEN") = 0
            workRow("APPLYID") = lblApplyIDText.Text
            workRow("COMPCODE") = txtCompCode.Text
            workRow("TANKNO") = txtTankNo.Text
            If Date.TryParseExact(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, pDate) Then
                workRow("STYMD") = pDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParseExact(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT.Replace("dd", "d").Replace("MM", "M"), Nothing, Nothing, pDate) Then
                workRow("STYMD") = pDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParse(txtStYMD.Text, pDate) Then
                workRow("STYMD") = pDate.ToString("yyyy/MM/dd")
            Else
                workRow("STYMD") = txtStYMD.Text
            End If
            If Date.TryParseExact(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, pDate) Then
                workRow("ENDYMD") = pDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParseExact(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT.Replace("dd", "d").Replace("MM", "M"), Nothing, Nothing, pDate) Then
                workRow("ENDYMD") = pDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParse(txtEndYMD.Text, pDate) Then
                workRow("ENDYMD") = pDate.ToString("yyyy/MM/dd")
            Else
                workRow("ENDYMD") = txtEndYMD.Text
            End If
            'If Date.TryParse(txtStYMD.Text, pDate) Then
            '    'workRow("STYMD") = pDate.ToString("yyyy/MM/dd")
            '    workRow("STYMD") = FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT)
            'Else
            '    workRow("STYMD") = txtStYMD.Text
            'End If
            'If Date.TryParse(txtEndYMD.Text, pDate) Then
            '    'workRow("ENDYMD") = pDate.ToString("yyyy/MM/dd")
            '    workRow("ENDYMD") = FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT)
            'Else
            '    workRow("ENDYMD") = txtEndYMD.Text
            'End If
            workRow("PROPERTY") = txtProperty.Text
            'workRow("LMOF") = ""
            'workRow("LEASESTAT") = ""
            'workRow("REPAIRSTAT") = ""
            'workRow("INSPECTDATE5") = ""
            'workRow("INSPECTDATE2P5") = ""
            'workRow("NEXTINSPECTDATE") = ""
            'workRow("NEXTINSPECTTYPE") = ""
            'workRow("JAPFIREAPPROVED") = ""
            'workRow("MANUFACTURER") = ""
            'workRow("MANUFACTURESERIALNO") = ""
            'workRow("DATEOFMANUFACTURE") = ""
            'workRow("MATERIAL") = ""
            'workRow("STRUCT") = ""
            'workRow("USDOTAPPROVED") = ""
            'workRow("NOMINALCAPACITY") = "0"
            'workRow("TANKCAPACITY") = "0"
            'workRow("MAXGROSSWEIGHT") = "0"
            'workRow("NETWEIGHT") = "0"
            'workRow("MEASUREMENT") = ""
            'workRow("FREAMDIMENSION_H") = ""
            'workRow("FREAMDIMENSION_W") = ""
            'workRow("FREAMDIMENSION_L") = ""
            'workRow("HEATING") = ""
            'workRow("HEATING_SUB") = ""
            'workRow("DISCHARGE") = ""
            'workRow("NOOFBOTTMCLOSURES") = ""
            'workRow("IMCOCLASS") = ""
            'workRow("FOOTVALUETYPE") = ""
            'workRow("BACKVALUETYPE") = ""
            'workRow("TOPDISVALUETYPE") = ""
            'workRow("AIRINLETVALUE") = ""
            'workRow("BAFFLES") = ""
            'workRow("TYPEOFPREVACVALUE") = ""
            'workRow("BURSTDISCFITTED") = ""
            'workRow("TYPEOFTHERM") = ""
            'workRow("TYPEOFMANLID") = ""
            'workRow("TYPEOFMANLID_CENTER") = ""
            'workRow("TYPEOFMANLID_FRONT") = ""
            'workRow("TYPEOFMLSEAL") = ""
            'workRow("WORKINGPRESSURE") = ""
            'workRow("TESTPRESSURE") = ""
            'workRow("REMARK1") = ""
            'workRow("REMARK2") = ""
            'workRow("FAULTS") = ""
            'workRow("BASERAGEYY") = ""
            'workRow("BASERAGEMM") = ""
            'workRow("BASERAGE") = ""
            'workRow("BASELEASE") = ""
            'workRow("MARUKANSEAL") = ""
            'workRow("REMARK") = ""
            workRow("DELFLG") = txtDelFlg.Text
            'workRow("UPDYMD") = ""
            INPtbl.Rows.Add(workRow)
        Next

        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = Me.hdnViewId.Value
        COA0014DetailView.TABID = ""
        COA0014DetailView.SRCDATA = INPtbl
        COA0014DetailView.REPEATER = WF_DViewRep1
        COA0014DetailView.COLPREFIX = "WF_Rep1_"
        COA0014DetailView.COA0014ReadDetailView()

    End Sub
    ''' <summary>
    ''' detailbox初期化
    ''' </summary>
    Protected Sub detailboxInit()

        Dim COA0014DetailView As New BASEDLL.COA0014DetailView

        Dim dataTable As DataTable = New DataTable
        Dim dataRow As DataRow

        ' BLAJ固有（JapFire行進画面はヘッダ項目非活性）
        If Me.hdnViewId.Value = "BLAJ_Tank" Then
            Me.txtStYMD.Enabled = False
            Me.txtEndYMD.Enabled = False
            Me.txtProperty.Enabled = False
            Me.txtTankNo.Enabled = False
            Me.txtDelFlg.Enabled = False
        End If

        BASEtblColumnsAdd(dataTable)
        dataRow = dataTable.NewRow
        dataTable.Rows.Add(dataRow)

        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = Me.hdnViewId.Value
        COA0014DetailView.TABID = ""
        COA0014DetailView.SRCDATA = dataTable
        COA0014DetailView.REPEATER = WF_DViewRep1
        COA0014DetailView.COLPREFIX = "WF_Rep1_"
        COA0014DetailView.COA0014SetDetailView()

        WF_DetailMView.ActiveViewIndex = 0

        lblDtabTank.Style.Remove("color")
        lblDtabTank.Style.Add("color", "blue")
        lblDtabTank.Style.Remove("background-color")
        lblDtabTank.Style.Add("background-color", "rgb(220,230,240)")
        lblDtabTank.Style.Remove("border")
        lblDtabTank.Style.Add("border", "1px solid blue")
        lblDtabTank.Style.Remove("font-weight")
        lblDtabTank.Style.Add("font-weight", "bold")

        'Detail設定処理
        SetDetailDbClick()

        INSPECTDATE5_Change()
        STRUCT_Change()
        TYPEOFMLSEAL_Change()
        NEXTINSPECTTYPE_Change()

        '初期値設定
        Me.txtCompCode.Text = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
        Dim endDt As Date = Date.Parse("2099/12/31")
        Me.txtStYMD.Text = Date.Now.ToString(GBA00003UserSetting.DATEFORMAT)
        Me.txtEndYMD.Text = endDt.ToString(GBA00003UserSetting.DATEFORMAT)

        Me.txtDelFlg.Text = BaseDllCommon.CONST_FLAG_NO
        txtDelFlg_Change()

        dataTable.Dispose()
        dataTable = Nothing

        WF_DViewRep1.Visible = True

    End Sub
    ''' <summary>
    ''' Detail設定処理
    ''' </summary>
    Protected Sub SetDetailDbClick()

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck
        Dim fieldList As List(Of String) = Nothing
        Dim dicField As Dictionary(Of String, String) = Nothing
        Dim repName1 As Label = Nothing
        Dim repName2 As Label = Nothing
        Dim repName3 As Label = Nothing
        Dim repField As Label = Nothing
        Dim repValue As TextBox = Nothing
        Dim repName As Label = Nothing
        Dim repAttr As String = ""

        '必須*付与
        fieldList = New List(Of String)
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.MUST = "Y"
        COA0026FieldCheck.FIELDDIC = dicField
        COA0026FieldCheck.FIELDLIST = fieldList
        COA0026FieldCheck.COA0026getFieldList()
        If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
            fieldList = COA0026FieldCheck.FIELDLIST
        End If

        For i As Integer = 0 To WF_DViewRep1.Items.Count - 1
            If fieldList.Count > 0 Then
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName1 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_1"), System.Web.UI.WebControls.Label)
                    If repName1.CssClass = "" Then
                        repName1.CssClass = "requiredMark2"
                    Else
                        repName1.CssClass = repName1.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName2 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_2"), System.Web.UI.WebControls.Label)
                    If repName2.CssClass = "" Then
                        repName2.CssClass = "requiredMark2"
                    Else
                        repName2.CssClass = repName2.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName3 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_3"), System.Web.UI.WebControls.Label)
                    If repName3.CssClass = "" Then
                        repName3.CssClass = "requiredMark2"
                    Else
                        repName3.CssClass = repName3.CssClass & " " & "requiredMark2"
                    End If
                End If
            End If

            '次回検査日
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "NEXTINSPECTDATE" OrElse
                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "NEXTINSPECTTYPE" Then
                repValue = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox)
                'repValue.Readonly = True
                repValue.Enabled = False
            End If

            'ダブルクリック時コード検索イベント追加
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label)
                repValue = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_1"), System.Web.UI.WebControls.Label)
                    repName.Attributes.Remove("style")
                    repName.Attributes.Add("style", "text-decoration: underline;")
                End If
            End If

            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label)
                repValue = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_2"), System.Web.UI.WebControls.Label)
                    repName.Attributes.Remove("style")
                    repName.Attributes.Add("style", "text-decoration: underline;")
                End If
            End If

            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label)
                repValue = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_3"), System.Web.UI.WebControls.Label)
                    repName.Attributes.Remove("style")
                    repName.Attributes.Add("style", "text-decoration: underline;")
                End If
            End If

        Next

    End Sub
    ''' <summary>
    ''' ダブルクリック処理追加
    ''' </summary>
    ''' <param name="repField"></param>
    ''' <param name="repAttr"></param>
    Protected Sub GetAttributes(ByVal repField As String, ByRef repAttr As String)

        Select Case repField

            '左
            Case "LMOF"
                '所有形態（自社、リース他）
                repAttr = "Field_DBclick('vLeftLMOF', '0');"
            Case "LEASESTAT"
                'リース
                repAttr = "Field_DBclick('vLeftLeaseStat', '1');"
            Case "REPAIRSTAT"
                'リペア
                repAttr = "Field_DBclick('vLeftRepairStat', '2');"
            Case "INSPECTDATE5"
                '検査日(５年)
                repAttr = "Field_DBclick('vLeftCal', '3');"
            Case "INSPECTDATE2P5"
                '検査日（２．５年）
                repAttr = "Field_DBclick('vLeftCal', '4');"
            Case "JAPFIREAPPROVED"
                'JP消防検査有無
                repAttr = "Field_DBclick('vLeftJapFireApproved', '7');"
            Case "MANUFACTURER"
                '製造メーカー
                repAttr = "Field_DBclick('vLeftManufacture', '8');"
            Case "DATEOFMANUFACTURE"
                '製造日
                repAttr = "Field_DBclick('vLeftCal', '10');"
            Case "STRUCT"
                '追加構造
                repAttr = "Field_DBclick('vLeftStruct', '12');"
            Case "USDOTAPPROVED"
                '荷重試験実施の有無
                repAttr = "Field_DBclick('vLeftUsDotApproved', '13');"

                '中央
            Case "DISCHARGE"
                '液出し口の位置
                repAttr = "Field_DBclick('vLeftDischarge', '2');"
            Case "FOOTVALUETYPE"
                'フート弁の仕様
                repAttr = "Field_DBclick('vLeftFootValue', '5');"
            Case "BACKVALUETYPE"
                '下部液出し口の仕様
                repAttr = "Field_DBclick('vLeftBottomOutlet', '6');"
            Case "TOPDISVALUETYPE"
                '上部積込口の仕様
                repAttr = "Field_DBclick('vLeftTopOutlet', '7');"
            Case "AIRINLETVALUE"
                'エアラインのバルブの仕様
                repAttr = "Field_DBclick('vLeftAirInlet', '8');"
            Case "BAFFLES"
                '防波板の有無
                repAttr = "Field_DBclick('vLeftBaffles', '9');"
            Case "BURSTDISCFITTED"
                '破裂板の有無
                repAttr = "Field_DBclick('vLeftBurstDisc', '11');"
            Case "TYPEOFTHERM"
                '温度計の種類
                repAttr = "Field_DBclick('vLeftTherm', '12');"

                '右
            Case "TYPEOFMLSEAL"
                'マンホールパッキンの種類
                repAttr = "Field_DBclick('vLeftMlSeal', '3');"
            Case "MARUKANSEAL"
                'マルカンシール
                repAttr = "Field_DBclick('vLeftMarukanSticker', '13');"
            Case "NEWTANKPORT"
                'New Tank Port
                repAttr = "Field_DBclick('vLeftNewTankPort', '15');"
            Case "NEWTANKACTY"
                'New Tank Acty
                repAttr = "Field_DBclick('vLeftNewTankActy', '16');"

            Case Else
                repAttr = ""

        End Select
    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    Public Sub ListUpdateCheck(ByVal InpRow As DataRow)
        'Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get
        Dim escapeFlg As Boolean = False
        Dim errFlg = False
        Dim errMessageStr As String = Nothing
        Dim refErrMessage As String = Nothing
        returnCode = C_MESSAGENO.NORMAL

        '入力項目チェック
        '①単項目チェック

        'カラム情報取得
        CheckSingle(InpRow, escapeFlg)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '②存在チェック(LeftBoxチェック)
        '会社コード
        If Me.lbCompCode.Items.Count <= 0 Then
            SetCompCodeListItem(Convert.ToString(InpRow("COMPCODE")))
        End If
        ChedckList(Convert.ToString(InpRow("COMPCODE")), lbCompCode, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("COMPCODE") & ":" & Convert.ToString(InpRow("COMPCODE")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '所属
        If Me.lbProperty.Items.Count <= 0 Then
            SetPropertyListItem()
        End If
        ChedckList(Convert.ToString(InpRow("PROPERTY")), lbProperty, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("PROPERTY") & ":" & Convert.ToString(InpRow("PROPERTY")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '削除フラグ
        If Me.lbDelFlg.Items.Count <= 0 Then
            SetDelFlgListItem(txtDelFlg.Text)
        End If
        ChedckList(Convert.ToString(InpRow("DELFLG")), lbDelFlg, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("DELFLG") & ":" & Convert.ToString(InpRow("DELFLG")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If


        '所有形態（自社、リース他）
        If Me.lbLMOF.Items.Count <= 0 Then
            SetLMOFListItem()
        End If
        ChedckList(Convert.ToString(InpRow("LMOF")), lbLMOF, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("LMOF") & ":" & Convert.ToString(InpRow("LMOF")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'リース
        If Me.lbLeaseStat.Items.Count <= 0 Then
            SetLeaseStatListItem()
        End If
        ChedckList(Convert.ToString(InpRow("LEASESTAT")), lbLeaseStat, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("LEASESTAT") & ":" & Convert.ToString(InpRow("LEASESTAT")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'リペア
        If Me.lbRepairStat.Items.Count <= 0 Then
            SetRepairStatListItem()
        End If
        ChedckList(Convert.ToString(InpRow("REPAIRSTAT")), lbRepairStat, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("REPAIRSTAT") & ":" & Convert.ToString(InpRow("REPAIRSTAT")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'JP消防検査有無
        If Me.lbJapFireApproved.Items.Count <= 0 Then
            SetJapFireApprovedListItem()
        End If
        ChedckList(Convert.ToString(InpRow("JAPFIREAPPROVED")), lbJapFireApproved, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("JAPFIREAPPROVED") & ":" & Convert.ToString(InpRow("JAPFIREAPPROVED")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '追加構造
        If Me.lbStruct.Items.Count <= 0 Then
            SetStructListItem()
        End If
        ChedckList(Convert.ToString(InpRow("STRUCT")), lbStruct, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("STRUCT") & ":" & Convert.ToString(InpRow("STRUCT")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '荷重試験実施の有無
        If Me.lbUsDotApproved.Items.Count <= 0 Then
            SetUsDotApprovedListItem()
        End If
        ChedckList(Convert.ToString(InpRow("USDOTAPPROVED")), lbUsDotApproved, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("USDOTAPPROVED") & ":" & Convert.ToString(InpRow("USDOTAPPROVED")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '液出し口の位置
        If Me.lbDischarge.Items.Count <= 0 Then
            SetDischargeListItem()
        End If
        ChedckList(Convert.ToString(InpRow("DISCHARGE")), lbDischarge, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("DISCHARGE") & ":" & Convert.ToString(InpRow("DISCHARGE")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '防波板の有無
        If Me.lbBaffles.Items.Count <= 0 Then
            SetBafflesListItem()
        End If
        ChedckList(Convert.ToString(InpRow("BAFFLES")), lbBaffles, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("BAFFLES") & ":" & Convert.ToString(InpRow("BAFFLES")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '破裂板の有無
        If Me.lbBurstDisc.Items.Count <= 0 Then
            SetBurstDiscListItem()
        End If
        ChedckList(Convert.ToString(InpRow("BURSTDISCFITTED")), lbBurstDisc, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("BURSTDISCFITTED") & ":" & Convert.ToString(InpRow("BURSTDISCFITTED")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '温度計の種類
        If Me.lbTherm.Items.Count <= 0 Then
            SetThermListItem()
        End If
        ChedckList(Convert.ToString(InpRow("TYPEOFTHERM")), lbTherm, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("TYPEOFTHERM") & ":" & Convert.ToString(InpRow("TYPEOFTHERM")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'マンホールパッキンの種類
        If Me.lbMlSeal.Items.Count <= 0 Then
            SetMlSealListItem()
        End If
        ChedckList(Convert.ToString(InpRow("TYPEOFMLSEAL")), lbMlSeal, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("TYPEOFMLSEAL") & ":" & Convert.ToString(InpRow("TYPEOFMLSEAL")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'マル関ステッカー貼付
        If Me.lbMarukanSticker.Items.Count <= 0 Then
            SetMarukanStickerListItem()
        End If
        ChedckList(Convert.ToString(InpRow("MARUKANSEAL")), lbMarukanSticker, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("MARUKANSEAL") & ":" & Convert.ToString(InpRow("MARUKANSEAL")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'New Tank Port
        If Me.lbNewTankPort.Items.Count <= 0 Then
            SetNewTankPortListItem()
        End If
        ChedckList(Convert.ToString(InpRow("NEWTANKPORT")), lbNewTankPort, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("NEWTANKPORT") & ":" & Convert.ToString(InpRow("NEWTANKPORT")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'New Tank Acty
        If Me.lbNewTankActy.Items.Count <= 0 Then
            SetNewTankActyListItem()
        End If
        ChedckList(Convert.ToString(InpRow("NEWTANKACTY")), lbNewTankActy, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("NEWTANKACTY") & ":" & Convert.ToString(InpRow("NEWTANKACTY")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '個別チェック
        'タンク番号
        '桁数チェック
        If Convert.ToString(InpRow("TANKNO")) <> "" AndAlso Convert.ToString(InpRow("TANKNO")).Length <> 11 Then
            errMessageStr = Me.ErrItemSet(InpRow)
            CommonFunctions.ShowMessage(C_MESSAGENO.INPUTERROR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
            refErrMessage = Me.lblFooterMessage.Text
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("TANKNO") & ":" & Convert.ToString(InpRow("TANKNO")) & ")" & errMessageStr
            errFlg = True
        End If
        '数値チェック
        If Convert.ToString(InpRow("TANKNO")).Length > 4 Then
            If Not IsNumeric(Mid(Convert.ToString(InpRow("TANKNO")), 5)) Then

                errMessageStr = Me.ErrItemSet(InpRow)
                CommonFunctions.ShowMessage(C_MESSAGENO.INPUTERROR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
                refErrMessage = Me.lblFooterMessage.Text
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("TANKNO") & ":" & Convert.ToString(InpRow("TANKNO")) & ")" & errMessageStr
                errFlg = True
            End If
        End If

        'エラーコード設定
        If escapeFlg Then
            '表反映除外対象
            returnCode = C_MESSAGENO.REQUIREDVALUE
        ElseIf errFlg Then
            '更新出来ないレコードが発生しました(右Boxのエラー詳細を参照 )。
            returnCode = C_MESSAGENO.RIGHTBIXOUT
        Else
            returnCode = C_MESSAGENO.NORMAL
        End If

    End Sub
    ''' <summary>
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="argRow"></param>
    Protected Sub CheckSingle(ByVal argRow As DataRow, ByRef argEscFlg As Boolean)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar        '例外文字排除 String Get
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck          '項目チェック
        'Dim slAdditionalList As New List(Of String)(New String() {"", ""})
        Dim slExcludeList As New List(Of String)(New String() {ControlChars.Quote})

        Dim errMessage As String = Nothing
        Dim errItemStr As String = Nothing

        If Me.dicField Is Nothing Then
            COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
            COA0026FieldCheck.MAPID = CONST_MAPID
            COA0026FieldCheck.FIELDDIC = Me.dicField
            COA0026FieldCheck.COA0026getFieldList()
            If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
                Me.dicField = COA0026FieldCheck.FIELDDIC
            Else
                CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage)
                Return
            End If
        End If

        'Dim dtCheckField As DataTable = New DataTable
        For Each itm As KeyValuePair(Of String, String) In Me.dicField

            '入力文字置き換え
            '画面PassWord内の使用禁止文字排除
            COA0008InvalidChar.CHARin = Convert.ToString(argRow(itm.Key))
            COA0008InvalidChar.EXCLUDELIST = slExcludeList
            COA0008InvalidChar.COA0008RemoveInvalidChar()
            If COA0008InvalidChar.CHARin <> COA0008InvalidChar.CHARout Then
                argRow(itm.Key) = COA0008InvalidChar.CHARout
            End If

            '単項目チェック
            COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
            COA0026FieldCheck.MAPID = CONST_MAPID
            COA0026FieldCheck.FIELD = itm.Key
            COA0026FieldCheck.VALUE = Convert.ToString(argRow(itm.Key))
            'COA0026FieldCheck.CHECKDT = dtCheckField
            COA0026FieldCheck.CHECKDT = Me.COLtbl
            COA0026FieldCheck.COA0026FieldCheck()
            If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
                errMessage = Me.lblFooterMessage.Text

                returnCode = COA0026FieldCheck.ERR

                If COA0026FieldCheck.ERR = C_MESSAGENO.REQUIREDVALUE Then
                    argEscFlg = True
                End If

                errItemStr = Me.ErrItemSet(argRow)
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & errMessage & "(" & itm.Value & ":" & Convert.ToString(argRow(itm.Key)) & ")" & errItemStr

            End If
            'dtCheckField = COA0026FieldCheck.CHECKDT
            Me.COLtbl = COA0026FieldCheck.CHECKDT
        Next

    End Sub
    ''' <summary>
    ''' LIST登録チェック
    ''' </summary>
    ''' <param name="inText"></param>
    ''' <param name="inList"></param>
    Protected Sub ChedckList(ByVal inText As String, ByVal inList As ListBox, ByRef errMessage As String)
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
                errMessage = Me.lblFooterMessage.Text
            End If
        End If
    End Sub
    ''' <summary>
    ''' 会社コードリストアイテムを設定
    ''' </summary>
    Private Sub SetCompCodeListItem(selectedValue As String)
        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        Try

            'リストクリア
            Me.lbCompCode.Items.Clear()

            'DataBase接続(Open)
            SQLcon.Open()

            '検索SQL文
            SQLStr =
                 "SELECT COMPCODE, NAMES, NAMES_EN " _
               & " FROM  COS0004_COMP " _
               & " Where STYMD   <= @P1 " _
               & "   and ENDYMD  >= @P2 " _
               & "   and DELFLG  <> @P3 "
            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
            PARA1.Value = Date.Now
            PARA2.Value = Date.Now
            PARA3.Value = BaseDllCommon.CONST_FLAG_YES
            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                'DBからアイテムを設定
                If COA0019Session.LANGDISP = C_LANG.JA Then
                    Me.lbCompCode.Items.Add(New ListItem(Convert.ToString(SQLdr("NAMES")), Convert.ToString(SQLdr("COMPCODE"))))
                Else
                    Me.lbCompCode.Items.Add(New ListItem(Convert.ToString(SQLdr("NAMES_EN")), Convert.ToString(SQLdr("COMPCODE"))))
                End If
            End While

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbCompCode.Items.Count > 0 Then
                Dim findListItem = Me.lbCompCode.Items.FindByValue(selectedValue)
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
        Finally
            'CLOSE
            If Not SQLdr Is Nothing Then
                SQLdr.Close()
            End If
            If Not SQLcmd Is Nothing Then
                SQLcmd.Dispose()
                SQLcmd = Nothing
            End If
            If Not SQLcon Is Nothing Then
                SQLcon.Close()
                SQLcon.Dispose()
                SQLcon = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 所有形態（自社、リース他）リストアイテムを設定
    ''' </summary>
    Private Sub SetLMOFListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbLMOF.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "LMOF"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbLMOF
        Else
            COA0017FixValue.LISTBOX2 = Me.lbLMOF
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbLMOF = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbLMOF = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' リースリストアイテムを設定
    ''' </summary>
    Private Sub SetLeaseStatListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbLeaseStat.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "LEASESTAT"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbLeaseStat
        Else
            COA0017FixValue.LISTBOX2 = Me.lbLeaseStat
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbLeaseStat = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbLeaseStat = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 所属リストアイテムを設定
    ''' </summary>
    Private Sub SetPropertyListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbProperty.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "PROPERTY"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbProperty
        Else
            COA0017FixValue.LISTBOX2 = Me.lbProperty
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbProperty = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbProperty = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' リペアリストアイテムを設定
    ''' </summary>
    Private Sub SetRepairStatListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbRepairStat.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "REPAIRSTAT"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbRepairStat
        Else
            COA0017FixValue.LISTBOX2 = Me.lbRepairStat
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbRepairStat = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbRepairStat = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' JP消防検査有無リストアイテムを設定
    ''' </summary>
    Private Sub SetJapFireApprovedListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbJapFireApproved.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "GENERALFLG"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbJapFireApproved
        Else
            COA0017FixValue.LISTBOX2 = Me.lbJapFireApproved
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbJapFireApproved = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbJapFireApproved = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 製造メーカーリストアイテムを設定
    ''' </summary>
    Private Sub SetManufactureListItem()
        Dim COA0036ItemList As New COA0036ItemList

        'リストクリア
        Me.lbManufacture.Items.Clear()

        'リスト設定
        COA0036ItemList.I_TABLE = "GBM0006_TANK"
        COA0036ItemList.I_ITEM = "MANUFACTURER"
        COA0036ItemList.O_LISTBOX = Me.lbManufacture
        COA0036ItemList.COA0036GetItemList()
        If COA0036ItemList.O_ERR = C_MESSAGENO.NORMAL Then

            For Each lsitem As ListItem In DirectCast(COA0036ItemList.O_LISTBOX, ListBox).Items
                Me.lbManufacture.Items.Add(lsitem)
            Next
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0036ItemList.O_ERR)})
        End If

    End Sub
    ''' <summary>
    ''' 追加構造リストアイテムを設定
    ''' </summary>
    Private Sub SetStructListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbStruct.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "STRUCT"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbStruct
        Else
            COA0017FixValue.LISTBOX2 = Me.lbStruct
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbStruct = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbStruct = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 荷重試験実施の有無リストアイテムを設定
    ''' </summary>
    Private Sub SetUsDotApprovedListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbUsDotApproved.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "GENERALFLG"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbUsDotApproved
        Else
            COA0017FixValue.LISTBOX2 = Me.lbUsDotApproved
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbUsDotApproved = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbUsDotApproved = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 液出し口の位置リストアイテムを設定
    ''' </summary>
    Private Sub SetDischargeListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbDischarge.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DISCHARGE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbDischarge
        Else
            COA0017FixValue.LISTBOX2 = Me.lbDischarge
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbDischarge = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbDischarge = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' フート弁の仕様リストアイテムを設定
    ''' </summary>
    Private Sub SetFootValueListItem()
        Dim COA0036ItemList As New COA0036ItemList

        'リストクリア
        Me.lbFootValue.Items.Clear()

        'リスト設定
        COA0036ItemList.I_TABLE = "GBM0006_TANK"
        COA0036ItemList.I_ITEM = "FOOTVALUETYPE"
        COA0036ItemList.O_LISTBOX = Me.lbFootValue
        COA0036ItemList.COA0036GetItemList()
        If COA0036ItemList.O_ERR = C_MESSAGENO.NORMAL Then

            For Each lsitem As ListItem In DirectCast(COA0036ItemList.O_LISTBOX, ListBox).Items
                Me.lbFootValue.Items.Add(lsitem)
            Next
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0036ItemList.O_ERR)})
        End If

    End Sub
    ''' <summary>
    ''' 下部液出し口の仕様リストアイテムを設定
    ''' </summary>
    Private Sub SetBottomOutletListItem()
        Dim COA0036ItemList As New COA0036ItemList

        'リストクリア
        Me.lbBottomOutlet.Items.Clear()

        'リスト設定
        COA0036ItemList.I_TABLE = "GBM0006_TANK"
        COA0036ItemList.I_ITEM = "BACKVALUETYPE"
        COA0036ItemList.O_LISTBOX = Me.lbBottomOutlet
        COA0036ItemList.COA0036GetItemList()
        If COA0036ItemList.O_ERR = C_MESSAGENO.NORMAL Then

            For Each lsitem As ListItem In DirectCast(COA0036ItemList.O_LISTBOX, ListBox).Items
                Me.lbBottomOutlet.Items.Add(lsitem)
            Next
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0036ItemList.O_ERR)})
        End If

    End Sub

    ''' <summary>
    ''' 上部積込口の仕様リストアイテムを設定
    ''' </summary>
    Private Sub SetTopOutletListItem()
        Dim COA0036ItemList As New COA0036ItemList

        'リストクリア
        Me.lbTopOutlet.Items.Clear()

        'リスト設定
        COA0036ItemList.I_TABLE = "GBM0006_TANK"
        COA0036ItemList.I_ITEM = "TOPDISVALUETYPE"
        COA0036ItemList.O_LISTBOX = Me.lbTopOutlet
        COA0036ItemList.COA0036GetItemList()
        If COA0036ItemList.O_ERR = C_MESSAGENO.NORMAL Then

            For Each lsitem As ListItem In DirectCast(COA0036ItemList.O_LISTBOX, ListBox).Items
                Me.lbTopOutlet.Items.Add(lsitem)
            Next
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0036ItemList.O_ERR)})
        End If

    End Sub
    ''' <summary>
    ''' エアラインのバルブの仕様リストアイテムを設定
    ''' </summary>
    Private Sub SetAirInletListItem()
        Dim COA0036ItemList As New COA0036ItemList

        'リストクリア
        Me.lbAirInlet.Items.Clear()

        'リスト設定
        COA0036ItemList.I_TABLE = "GBM0006_TANK"
        COA0036ItemList.I_ITEM = "AIRINLETVALUE"
        COA0036ItemList.O_LISTBOX = Me.lbAirInlet
        COA0036ItemList.COA0036GetItemList()
        If COA0036ItemList.O_ERR = C_MESSAGENO.NORMAL Then

            For Each lsitem As ListItem In DirectCast(COA0036ItemList.O_LISTBOX, ListBox).Items
                Me.lbAirInlet.Items.Add(lsitem)
            Next
            '正常
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0036ItemList.O_ERR)})
        End If

    End Sub
    ''' <summary>
    ''' 防波板の有無リストアイテムを設定
    ''' </summary>
    Private Sub SetBafflesListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbBaffles.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "GENERALFLG"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbBaffles
        Else
            COA0017FixValue.LISTBOX2 = Me.lbBaffles
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbBaffles = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbBaffles = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 破裂板の有無リストアイテムを設定
    ''' </summary>
    Private Sub SetBurstDiscListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbBurstDisc.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "GENERALFLG"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbBurstDisc
        Else
            COA0017FixValue.LISTBOX2 = Me.lbBurstDisc
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbBurstDisc = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbBurstDisc = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 温度計の種類リストアイテムを設定
    ''' </summary>
    Private Sub SetThermListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbTherm.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "THERM"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbTherm
        Else
            COA0017FixValue.LISTBOX2 = Me.lbTherm
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbTherm = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbTherm = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' マンホールパッキンの種類リストアイテムを設定
    ''' </summary>
    Private Sub SetMlSealListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbMlSeal.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "MLSEAL"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbMlSeal
        Else
            COA0017FixValue.LISTBOX2 = Me.lbMlSeal
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbMlSeal = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbMlSeal = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' マル関ステッカー貼付リストアイテムを設定
    ''' </summary>
    Private Sub SetMarukanStickerListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbMarukanSticker.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "MARUKANSTICKER"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbMarukanSticker
        Else
            COA0017FixValue.LISTBOX2 = Me.lbMarukanSticker
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbMarukanSticker = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbMarukanSticker = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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

    '''' <summary>
    '''' フラグ全般リストアイテムを設定
    '''' </summary>
    'Private Sub SetGenFlgListItem()
    '    Dim COA0017FixValue As New COA0017FixValue

    '    'リストクリア
    '    Me.lbGenFlg.Items.Clear()

    '    'リスト設定
    '    COA0017FixValue.COMPCODE = GBC_COMPCODE_D
    '    COA0017FixValue.CLAS = "GENERALFLG"
    '    If COA0019Session.LANGDISP = C_LANG.JA Then
    '        COA0017FixValue.LISTBOX1 = Me.lbGenFlg
    '    Else
    '        COA0017FixValue.LISTBOX2 = Me.lbGenFlg
    '    End If

    '    COA0017FixValue.COA0017getListFixValue()
    '    If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

    '        If COA0019Session.LANGDISP = C_LANG.JA Then
    '            Me.lbGenFlg = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
    '        Else
    '            Me.lbGenFlg = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
    '        End If

    '        '正常
    '        returnCode = C_MESSAGENO.NORMAL

    '    Else
    '        '異常
    '        returnCode = C_MESSAGENO.SYSTEMADM
    '        CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
    '                                    messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
    '    End If

    'End Sub

    ''' <summary>
    ''' 検査日種別リストアイテムを設定
    ''' </summary>
    Private Sub SetInspectTypeListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbInspectType.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "INSPECTTYPE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbInspectType
        Else
            COA0017FixValue.LISTBOX2 = Me.lbInspectType
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbInspectType = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbInspectType = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 削除フラグリストアイテムを設定
    ''' </summary>
    Private Sub SetDelFlgListItem(selectedValue As String)
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbDelFlg.Items.Clear()

        'リスト設定
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

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbDelFlg.Items.Count > 0 Then
                Dim findListItem = Me.lbDelFlg.Items.FindByValue(selectedValue)
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
    ''' New Tank Actyリストアイテムを設定
    ''' </summary>
    Private Sub SetNewTankActyListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbNewTankActy.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "NEWTANKACTY"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbNewTankActy
        Else
            COA0017FixValue.LISTBOX2 = Me.lbNewTankActy
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbNewTankActy = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbNewTankActy = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' New Tank Portリストアイテムを設定
    ''' </summary>
    Private Sub SetNewTankPortListItem()

        Dim countryCode As String = ""
        Dim dt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(countryCode)
        With Me.lbNewTankPort
            .DataSource = dt
            .DataTextField = "LISTBOXNAME"
            .DataValueField = "PORTCODE"
            .DataBind()
            .Focus()
        End With
        '入力済のデータを選択状態にする
        Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)

        If dblClickField IsNot Nothing AndAlso lbNewTankPort.Items IsNot Nothing Then
            Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
            Dim findLbValue As ListItem = lbNewTankPort.Items.FindByValue(dblClickFieldText.Text)
            If findLbValue IsNot Nothing Then
                findLbValue.Selected = True
            End If
        End If
        '正常
        returnCode = C_MESSAGENO.NORMAL

    End Sub


    ''' <summary>
    ''' 会社コード名設定
    ''' </summary>
    Public Sub txtCompCode_Change()

        Try
            Me.lblCompCodeText.Text = ""

            SetCompCodeListItem(Me.txtCompCode.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbCompCode.Items.Count > 0 Then
                Dim findListItem = Me.lbCompCode.Items.FindByValue(Me.txtCompCode.Text)
                If findListItem IsNot Nothing Then
                    Me.lblCompCodeText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbCompCode.Items.FindByValue(Me.txtCompCode.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblCompCodeText.Text = findListItemUpper.Text
                        Me.txtCompCode.Text = findListItemUpper.Value
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
    ''' 所属名設定
    ''' </summary>
    Public Sub txtProperty_Change()

        Try
            Me.lblPropertyText.Text = ""

            SetPropertyListItem()
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbProperty.Items.Count > 0 Then
                Dim findListItem = Me.lbProperty.Items.FindByValue(Me.txtProperty.Text)
                If findListItem IsNot Nothing Then
                    Me.lblPropertyText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbProperty.Items.FindByValue(Me.txtProperty.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblPropertyText.Text = findListItemUpper.Text
                        Me.txtProperty.Text = findListItemUpper.Value
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
    ''' 所有形態（自社、リース他）名設定
    ''' </summary>
    Public Sub LMOF_Change()

        Try

            'リピーター有効フラグ
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "LMOF" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetLMOFListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbLMOF.Items.Count > 0 Then
                            Dim findListItem = Me.lbLMOF.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' リース名設定
    ''' </summary>
    Public Sub LEASESTAT_Change()

        Try

            'リピーターリース
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "LEASESTAT" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetLeaseStatListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbLeaseStat.Items.Count > 0 Then
                            Dim findListItem = Me.lbLeaseStat.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' リペア名設定
    ''' </summary>
    Public Sub REPAIRSTAT_Change()

        Try

            'リピーターリペア
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "REPAIRSTAT" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetRepairStatListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbRepairStat.Items.Count > 0 Then
                            Dim findListItem = Me.lbRepairStat.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 次回検査種別名設定
    ''' </summary>
    Public Sub NEXTINSPECTTYPE_Change()

        Try

            'リピーター次回検査種別
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "NEXTINSPECTTYPE" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetInspectTypeListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbInspectType.Items.Count > 0 Then
                            Dim findListItem = Me.lbInspectType.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 検査日（２．５年）変更時
    ''' </summary>
    Public Sub INSPECTDATE2P5_Change()

        NextInspectConfig()

    End Sub

    ''' <summary>
    ''' 検査日(５年)変更時
    ''' </summary>
    Public Sub INSPECTDATE5_Change()

        NextInspectConfig()

    End Sub

    ''' <summary>
    ''' 製造日変更時
    ''' </summary>
    Public Sub DATEOFMANUFACTURE_Change()

        NextInspectConfig()

    End Sub

    ''' <summary>
    ''' 次回検査日設定名設定
    ''' </summary>
    Public Sub NextInspectConfig()

        Dim ins5 As String = ""
        Dim ins5Date As Date = Nothing
        Dim ins2h As String = ""
        Dim ins2hDate As Date = Nothing
        Dim manufac As String = ""
        Dim val As String = ""
        Dim type As String = ""

        Try

            'リピーター検査日(５年)
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "INSPECTDATE5" Then

                    ins5 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                    System.Web.UI.WebControls.TextBox).Text

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "INSPECTDATE2P5" Then

                    ins2h = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                    System.Web.UI.WebControls.TextBox).Text

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "DATEOFMANUFACTURE" Then

                    manufac = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                    System.Web.UI.WebControls.TextBox).Text

                End If
            Next

            SetNextInspect(ins2h, ins5, manufac, val, type)

            'リピーター次回検査日設定
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "NEXTINSPECTDATE" Then

                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                    System.Web.UI.WebControls.TextBox).Text = val

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "NEXTINSPECTTYPE" Then

                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                    System.Web.UI.WebControls.TextBox).Text = type
                End If
            Next

            NEXTINSPECTTYPE_Change()

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
    ''' JP消防検査有無名設定
    ''' </summary>
    Public Sub JAPFIREAPPROVED_Change()

        Try

            'リピーターJP消防検査有無
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "JAPFIREAPPROVED" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetJapFireApprovedListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbJapFireApproved.Items.Count > 0 Then
                            Dim findListItem = Me.lbJapFireApproved.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 追加構造名設定
    ''' </summary>
    Public Sub STRUCT_Change()

        Try

            'リピーター追加構造
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "STRUCT" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetStructListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbStruct.Items.Count > 0 Then
                            Dim findListItem = Me.lbStruct.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 荷重試験実施の有無名設定
    ''' </summary>
    Public Sub USDOTAPPROVED_Change()

        Try

            'リピーター荷重試験実施の有無
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), System.Web.UI.WebControls.Label).Text = "USDOTAPPROVED" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetUsDotApprovedListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbUsDotApproved.Items.Count > 0 Then
                            Dim findListItem = Me.lbUsDotApproved.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_1"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 液出し口の位置名設定
    ''' </summary>
    Public Sub DISCHARGE_Change()

        Try

            'リピーター液出し口の位置
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "DISCHARGE" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetDischargeListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbDischarge.Items.Count > 0 Then
                            Dim findListItem = Me.lbDischarge.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 防波板の有無名設定
    ''' </summary>
    Public Sub BAFFLES_Change()

        Try

            'リピーター防波板の有無
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "BAFFLES" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetBafflesListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbBaffles.Items.Count > 0 Then
                            Dim findListItem = Me.lbBaffles.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 破裂板の有無名設定
    ''' </summary>
    Public Sub BURSTDISCFITTED_Change()

        Try

            'リピーター破裂板の有無
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "BURSTDISCFITTED" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetBurstDiscListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbBurstDisc.Items.Count > 0 Then
                            Dim findListItem = Me.lbBurstDisc.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 温度計の種類名設定
    ''' </summary>
    Public Sub TYPEOFTHERM_Change()

        Try

            'リピーター温度計の種類
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "TYPEOFTHERM" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetThermListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbTherm.Items.Count > 0 Then
                            Dim findListItem = Me.lbTherm.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' マンホールパッキンの種類名設定
    ''' </summary>
    Public Sub TYPEOFMLSEAL_Change()

        Try

            'リピーターマンホールパッキンの種類
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "TYPEOFMLSEAL" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetMlSealListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbMlSeal.Items.Count > 0 Then
                            Dim findListItem = Me.lbMlSeal.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' マル関ステッカー貼付名設定
    ''' </summary>
    Public Sub MARUKANSEAL_Change()

        Try

            'リピーターマル関ステッカー貼付
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "MARUKANSEAL" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetMarukanStickerListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbMarukanSticker.Items.Count > 0 Then
                            Dim findListItem = Me.lbMarukanSticker.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' New Tank Port名設定
    ''' </summary>
    Public Sub NEWTANKPORT_Change()

        Try

            'リピーターマル関ステッカー貼付
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "NEWTANKPORT" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetNewTankPortListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbNewTankPort.Items.Count > 0 Then
                            Dim findListItem = Me.lbNewTankPort.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' New Tank Acty名設定
    ''' </summary>
    Public Sub NEWTANKACTY_Change()

        Try

            'リピーターマル関ステッカー貼付
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "NEWTANKACTY" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetNewTankActyListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbNewTankActy.Items.Count > 0 Then
                            Dim findListItem = Me.lbNewTankActy.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
                                                                                                    System.Web.UI.WebControls.TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"),
                                        System.Web.UI.WebControls.Label).Text = findListItem.Text
                            End If
                        End If
                    End If
                End If
            Next

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
    ''' 削除フラグ名設定
    ''' </summary>
    Public Sub txtDelFlg_Change()

        Try
            Me.lblDelFlgText.Text = ""

            SetDelFlgListItem(Me.txtDelFlg.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbDelFlg.Items.Count > 0 Then
                Dim findListItem = Me.lbDelFlg.Items.FindByValue(Me.txtDelFlg.Text)
                If findListItem IsNot Nothing Then
                    Me.lblDelFlgText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbDelFlg.Items.FindByValue(Me.txtDelFlg.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblDelFlgText.Text = findListItemUpper.Text
                        Me.txtDelFlg.Text = findListItemUpper.Value
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
    ''' detailboxクリア
    ''' </summary>
    Protected Sub detailboxClear()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        returnCode = C_MESSAGENO.NORMAL

        '一覧表示データ復元
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            returnCode = COA0021ListTable.ERR
            Return
        End If

        '画面WF_GRID状態設定
        '状態をクリア設定
        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            Select Case Convert.ToString(BASEtbl.Rows(i)(1))
                Case ""
                    BASEtbl.Rows(i)(1) = ""
                Case "&nbsp;"
                    BASEtbl.Rows(i)(1) = ""
                Case "★"
                    BASEtbl.Rows(i)(1) = ""
                Case "★" & updateDisp
                    BASEtbl.Rows(i)(1) = updateDisp
                Case "★" & errDisp
                    BASEtbl.Rows(i)(1) = errDisp
            End Select
        Next

        '一覧表示データ保存
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            returnCode = COA0021ListTable.ERR
            Return
        End If

        lblLineCntText.Text = ""
        lblApplyIDText.Text = ""
        txtCompCode.Text = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
        lblCompCodeText.Text = ""
        txtProperty.Text = ""
        lblPropertyText.Text = ""
        txtTankNo.Text = ""
        'lblTankNoText.Text = ""
        txtStYMD.Text = ""
        txtEndYMD.Text = ""
        txtDelFlg.Text = ""
        lblDelFlgText.Text = ""

        'ボタン制御
        SetButtonControl()

        'Repeaterバインド準備
        PDFtblColumnsAdd()

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

        'Detail初期設定
        detailboxInit()

        'フォーカス設定
        txtStYMD.Focus()

        INPtbl.Clear()
        INPtbl.Dispose()

    End Sub
    ''' <summary>
    ''' 内部テーブルデータ更新
    ''' </summary>
    Protected Sub BASEtblUpdate()
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim newFlg = False
        Dim dummyMsgBox As Label = New Label
        Dim errorMessage As String = Nothing
        Dim errMessageStr As String = Nothing
        Dim endFlg As Boolean = False
        'work
        Dim workBasePos As Integer = -1
        Dim workBaseRow As DataRow
        workBaseRow = BASEtbl.NewRow
        Dim workBaseRow2 As DataRow
        workBaseRow2 = BASEtbl.NewRow
        Dim workBaseRow3 As DataRow

        '操作表示クリア
        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            Select Case Convert.ToString(BASEtbl.Rows(i)("OPERATION"))
                Case "&nbsp;"
                    BASEtbl.Rows(i)("OPERATION") = ""
                Case "★"
                    BASEtbl.Rows(i)("OPERATION") = ""
                Case "★" & updateDisp
                    BASEtbl.Rows(i)("OPERATION") = updateDisp
                Case "★" & errDisp
                    BASEtbl.Rows(i)("OPERATION") = errDisp
            End Select
        Next

        Dim compareUpdTargetFieldList = CommonFunctions.CreateCompareFieldList({"COMPCODE", "TANKNO", "STYMD"})
        Dim compareModCheckFieldList = CommonFunctions.CreateCompareFieldList({"ENDYMD", "PROPERTY", "LMOF", "LEASESTAT", "REPAIRSTAT",
                                                                               "INSPECTDATE5", "INSPECTDATE2P5", "NEXTINSPECTDATE", "NEXTINSPECTTYPE", "JAPFIREAPPROVED",
                                                                               "MANUFACTURER", "MANUFACTURESERIALNO", "DATEOFMANUFACTURE", "MATERIAL", "STRUCT",
                                                                               "USDOTAPPROVED", "NOMINALCAPACITY", "TANKCAPACITY", "MAXGROSSWEIGHT", "NETWEIGHT",
                                                                               "FREAMDIMENSION_H", "FREAMDIMENSION_W", "FREAMDIMENSION_L", "HEATING", "HEATING_SUB",
                                                                               "DISCHARGE", "NOOFBOTTMCLOSURES", "IMCOCLASS", "FOOTVALUETYPE", "BACKVALUETYPE",
                                                                               "TOPDISVALUETYPE", "AIRINLETVALUE", "BAFFLES", "TYPEOFPREVACVALUE", "BURSTDISCFITTED",
                                                                               "TYPEOFTHERM", "TYPEOFMANLID_CENTER", "TYPEOFMANLID_FRONT", "TYPEOFMLSEAL", "WORKINGPRESSURE",
                                                                               "TESTPRESSURE", "REMARK1", "REMARK2", "FAULTS", "BASERAGEYY",
                                                                               "BASERAGEMM", "BASERAGE", "BASELEASE", "MARUKANSEAL", "NEWTANKPORT", "NEWTANKACTY",
                                                                               "REMARK", "DELFLG"})

        Dim drInput As DataRow = INPtbl.NewRow
        For i As Integer = 0 To INPtbl.Rows.Count - 1

            drInput.ItemArray = INPtbl(i).ItemArray
            If Convert.ToString(INPtbl(i)("HIDDEN")) <> "1" Then ' "1" ・・・取り込み対象外エラー

                'Dim workBasePos As Integer = -1
                workBasePos = -1
                newFlg = False

                '内部テーブル検索
                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                    'Dim workBaseRow As DataRow
                    'workBaseRow = BASEtbl.NewRow
                    'workBaseRow.ItemArray = BASEtbl.Rows(j).ItemArray
                    workBaseRow = BASEtbl.Rows(j)

                    ' 更新対象検索
                    'If Convert.ToString(workBaseRow("COMPCODE")) = Convert.ToString(INPtbl(i)("COMPCODE")) AndAlso
                    '   Convert.ToString(workBaseRow("TANKNO")) = Convert.ToString(INPtbl(i)("TANKNO")) AndAlso
                    '   Convert.ToString(workBaseRow("STYMD")) = Convert.ToString(INPtbl(i)("STYMD")) Then
                    If CommonFunctions.CompareDataFields(workBaseRow, drInput, compareUpdTargetFieldList) Then

                        ' 変更なし
                        If Convert.ToString(INPtbl(i)("OPERATION")) <> errDisp AndAlso
                           CommonFunctions.CompareDataFields(workBaseRow, drInput, compareModCheckFieldList) Then

                            'If Convert.ToString(INPtbl(i)("OPERATION")) <> errDisp AndAlso
                            '       Convert.ToString(workBaseRow("ENDYMD")) = Convert.ToString(INPtbl(i)("ENDYMD")) AndAlso
                            '       Convert.ToString(workBaseRow("PROPERTY")) = Convert.ToString(INPtbl(i)("PROPERTY")) AndAlso
                            '       Convert.ToString(workBaseRow("LMOF")) = Convert.ToString(INPtbl(i)("LMOF")) AndAlso
                            '       Convert.ToString(workBaseRow("LEASESTAT")) = Convert.ToString(INPtbl(i)("LEASESTAT")) AndAlso
                            '       Convert.ToString(workBaseRow("REPAIRSTAT")) = Convert.ToString(INPtbl(i)("REPAIRSTAT")) AndAlso
                            '       Convert.ToString(workBaseRow("INSPECTDATE5")) = Convert.ToString(INPtbl(i)("INSPECTDATE5")) AndAlso
                            '       Convert.ToString(workBaseRow("INSPECTDATE2P5")) = Convert.ToString(INPtbl(i)("INSPECTDATE2P5")) AndAlso
                            '       Convert.ToString(workBaseRow("NEXTINSPECTDATE")) = Convert.ToString(INPtbl(i)("NEXTINSPECTDATE")) AndAlso
                            '       Convert.ToString(workBaseRow("NEXTINSPECTTYPE")) = Convert.ToString(INPtbl(i)("NEXTINSPECTTYPE")) AndAlso
                            '       Convert.ToString(workBaseRow("JAPFIREAPPROVED")) = Convert.ToString(INPtbl(i)("JAPFIREAPPROVED")) AndAlso
                            '       Convert.ToString(workBaseRow("MANUFACTURER")) = Convert.ToString(INPtbl(i)("MANUFACTURER")) AndAlso
                            '       Convert.ToString(workBaseRow("MANUFACTURESERIALNO")) = Convert.ToString(INPtbl(i)("MANUFACTURESERIALNO")) AndAlso
                            '       Convert.ToString(workBaseRow("DATEOFMANUFACTURE")) = Convert.ToString(INPtbl(i)("DATEOFMANUFACTURE")) AndAlso
                            '       Convert.ToString(workBaseRow("MATERIAL")) = Convert.ToString(INPtbl(i)("MATERIAL")) AndAlso
                            '       Convert.ToString(workBaseRow("STRUCT")) = Convert.ToString(INPtbl(i)("STRUCT")) AndAlso
                            '       Convert.ToString(workBaseRow("USDOTAPPROVED")) = Convert.ToString(INPtbl(i)("USDOTAPPROVED")) AndAlso
                            '       Convert.ToString(workBaseRow("NOMINALCAPACITY")) = Convert.ToString(INPtbl(i)("NOMINALCAPACITY")) AndAlso
                            '       Convert.ToString(workBaseRow("TANKCAPACITY")) = Convert.ToString(INPtbl(i)("TANKCAPACITY")) AndAlso
                            '       Convert.ToString(workBaseRow("MAXGROSSWEIGHT")) = Convert.ToString(INPtbl(i)("MAXGROSSWEIGHT")) AndAlso
                            '       Convert.ToString(workBaseRow("NETWEIGHT")) = Convert.ToString(INPtbl(i)("NETWEIGHT")) AndAlso
                            '       Convert.ToString(workBaseRow("FREAMDIMENSION_H")) = Convert.ToString(INPtbl(i)("FREAMDIMENSION_H")) AndAlso
                            '       Convert.ToString(workBaseRow("FREAMDIMENSION_W")) = Convert.ToString(INPtbl(i)("FREAMDIMENSION_W")) AndAlso
                            '       Convert.ToString(workBaseRow("FREAMDIMENSION_L")) = Convert.ToString(INPtbl(i)("FREAMDIMENSION_L")) AndAlso
                            '       Convert.ToString(workBaseRow("HEATING")) = Convert.ToString(INPtbl(i)("HEATING")) AndAlso
                            '       Convert.ToString(workBaseRow("HEATING_SUB")) = Convert.ToString(INPtbl(i)("HEATING_SUB")) AndAlso
                            '       Convert.ToString(workBaseRow("DISCHARGE")) = Convert.ToString(INPtbl(i)("DISCHARGE")) AndAlso
                            '       Convert.ToString(workBaseRow("NOOFBOTTMCLOSURES")) = Convert.ToString(INPtbl(i)("NOOFBOTTMCLOSURES")) AndAlso
                            '       Convert.ToString(workBaseRow("IMCOCLASS")) = Convert.ToString(INPtbl(i)("IMCOCLASS")) AndAlso
                            '       Convert.ToString(workBaseRow("FOOTVALUETYPE")) = Convert.ToString(INPtbl(i)("FOOTVALUETYPE")) AndAlso
                            '       Convert.ToString(workBaseRow("BACKVALUETYPE")) = Convert.ToString(INPtbl(i)("BACKVALUETYPE")) AndAlso
                            '       Convert.ToString(workBaseRow("TOPDISVALUETYPE")) = Convert.ToString(INPtbl(i)("TOPDISVALUETYPE")) AndAlso
                            '       Convert.ToString(workBaseRow("AIRINLETVALUE")) = Convert.ToString(INPtbl(i)("AIRINLETVALUE")) AndAlso
                            '       Convert.ToString(workBaseRow("BAFFLES")) = Convert.ToString(INPtbl(i)("BAFFLES")) AndAlso
                            '       Convert.ToString(workBaseRow("TYPEOFPREVACVALUE")) = Convert.ToString(INPtbl(i)("TYPEOFPREVACVALUE")) AndAlso
                            '       Convert.ToString(workBaseRow("BURSTDISCFITTED")) = Convert.ToString(INPtbl(i)("BURSTDISCFITTED")) AndAlso
                            '       Convert.ToString(workBaseRow("TYPEOFTHERM")) = Convert.ToString(INPtbl(i)("TYPEOFTHERM")) AndAlso
                            '       Convert.ToString(workBaseRow("TYPEOFMANLID_CENTER")) = Convert.ToString(INPtbl(i)("TYPEOFMANLID_CENTER")) AndAlso
                            '       Convert.ToString(workBaseRow("TYPEOFMANLID_FRONT")) = Convert.ToString(INPtbl(i)("TYPEOFMANLID_FRONT")) AndAlso
                            '       Convert.ToString(workBaseRow("TYPEOFMLSEAL")) = Convert.ToString(INPtbl(i)("TYPEOFMLSEAL")) AndAlso
                            '       Convert.ToString(workBaseRow("WORKINGPRESSURE")) = Convert.ToString(INPtbl(i)("WORKINGPRESSURE")) AndAlso
                            '       Convert.ToString(workBaseRow("TESTPRESSURE")) = Convert.ToString(INPtbl(i)("TESTPRESSURE")) AndAlso
                            '       Convert.ToString(workBaseRow("REMARK1")) = Convert.ToString(INPtbl(i)("REMARK1")) AndAlso
                            '       Convert.ToString(workBaseRow("REMARK2")) = Convert.ToString(INPtbl(i)("REMARK2")) AndAlso
                            '       Convert.ToString(workBaseRow("FAULTS")) = Convert.ToString(INPtbl(i)("FAULTS")) AndAlso
                            '       Convert.ToString(workBaseRow("BASERAGEYY")) = Convert.ToString(INPtbl(i)("BASERAGEYY")) AndAlso
                            '       Convert.ToString(workBaseRow("BASERAGEMM")) = Convert.ToString(INPtbl(i)("BASERAGEMM")) AndAlso
                            '       Convert.ToString(workBaseRow("BASERAGE")) = Convert.ToString(INPtbl(i)("BASERAGE")) AndAlso
                            '       Convert.ToString(workBaseRow("BASELEASE")) = Convert.ToString(INPtbl(i)("BASELEASE")) AndAlso
                            '       Convert.ToString(workBaseRow("MARUKANSEAL")) = Convert.ToString(INPtbl(i)("MARUKANSEAL")) AndAlso
                            '       Convert.ToString(workBaseRow("REMARK")) = Convert.ToString(INPtbl(i)("REMARK")) AndAlso
                            '       Convert.ToString(workBaseRow("DELFLG")) = Convert.ToString(INPtbl(i)("DELFLG")) Then
                            workBasePos = -999    '-999 は登録対象外
                            If WF_DViewRepPDF.Items.Count <> 0 Then
                                For k As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
                                    If hdnListBoxPDF.Items.Count <> 0 Then
                                        For l As Integer = 0 To hdnListBoxPDF.Items.Count - 1
                                            If (DirectCast(WF_DViewRepPDF.Items(k).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text = hdnListBoxPDF.Items(l).Text And
                                                    DirectCast(WF_DViewRepPDF.Items(k).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Text = hdnListBoxPDF.Items(l).Value) Then
                                                workBasePos = -999
                                            Else
                                                workBasePos = j
                                                Exit For
                                            End If
                                        Next
                                        If workBasePos <> -999 Then
                                            Exit For
                                        End If
                                    Else
                                        If DirectCast(WF_DViewRepPDF.Items(k).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Text = BaseDllCommon.CONST_FLAG_YES Then
                                            workBasePos = -999
                                        Else
                                            workBasePos = j
                                            Exit For
                                        End If
                                    End If
                                Next
                            End If
                        Else
                            '更新
                            workBasePos = j
                        End If
                        Exit For
                    End If
                Next

                ' 新規レコードのポジション設定
                If workBasePos = -1 Then ' -1 は新規登録対象
                    workBasePos = (BASEtbl.Rows.Count - 1) + 1
                    newFlg = True

                End If

                ' 内部テーブル編集
                If workBasePos >= 0 Then

                    '内部テーブル検索
                    For k As Integer = 0 To BASEtbl.Rows.Count - 1

                        'Dim workBaseRow2 As DataRow
                        'workBaseRow2 = BASEtbl.NewRow
                        'workBaseRow2.ItemArray = BASEtbl.Rows(k).ItemArray
                        workBaseRow2 = BASEtbl.Rows(k)

                        'If Convert.ToString(workBaseRow2("COMPCODE")) = Convert.ToString(INPtbl(i)("COMPCODE")) AndAlso
                        '   Convert.ToString(workBaseRow2("TANKNO")) = Convert.ToString(INPtbl(i)("TANKNO")) AndAlso
                        '   Convert.ToString(workBaseRow2("STYMD")) = Convert.ToString(INPtbl(i)("STYMD")) Then
                        If CommonFunctions.CompareDataFields(workBaseRow2, drInput, compareUpdTargetFieldList) Then

                            '申請中のものはエラー
                            If Convert.ToString(workBaseRow2("APPLYID")) <> "" Then
                                returnCode = C_MESSAGENO.HASAPPLYINGRECORD
                                CommonFunctions.ShowMessage(returnCode, dummyMsgBox)
                                errorMessage = dummyMsgBox.Text
                                'エラーレポート編集
                                errMessageStr = ""
                                errMessageStr = "・" & errorMessage
                                ' レコード内容を展開する
                                errMessageStr = errMessageStr & Me.ErrItemSet(drInput)
                                If txtRightErrorMessage.Text <> "" Then
                                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                                End If
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr
                                'endFlg = True

                                INPtbl(i)("OPERATION") = errDisp
                            End If

                            '既に輸送開始されているタンクの初期所在地の変更は不可
                            If Convert.ToInt64(drInput("ACTYCNT")) > 1 AndAlso
                                (Convert.ToString(workBaseRow2("NEWTANKPORT")) <> Convert.ToString(drInput("NEWTANKPORT")) OrElse Convert.ToString(workBaseRow2("NEWTANKACTY")) <> Convert.ToString(drInput("NEWTANKACTY"))) Then

                                returnCode = C_MESSAGENO.INVALIDINPUT
                                CommonFunctions.ShowMessage(returnCode, dummyMsgBox)
                                errorMessage = dummyMsgBox.Text
                                'エラーレポート編集
                                errMessageStr = ""
                                errMessageStr = "・" & errorMessage
                                ' レコード内容を展開する
                                'errMessageStr = errMessageStr & Me.ErrItemSet(drInput)
                                If txtRightErrorMessage.Text <> "" Then
                                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                                End If
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr _
                                                            & "(" & dicField("NEWTANKPORT") & ":" & Convert.ToString(drInput("NEWTANKPORT")) & ")" _
                                                            & "(" & dicField("NEWTANKACTY") & ":" & Convert.ToString(drInput("NEWTANKACTY")) & ")" & Me.ErrItemSet(drInput)
                                returnCode = C_MESSAGENO.RIGHTBIXOUT
                                CommonFunctions.ShowMessage(returnCode, dummyMsgBox)
                                INPtbl(i)("OPERATION") = errDisp
                            End If

                            Exit For
                        End If
                    Next

                    '固定項目
                    'Dim workBaseRow As DataRow
                    workBaseRow3 = BASEtbl.NewRow

                    If workBasePos < BASEtbl.Rows.Count Then
                        '更新対象を設定
                        workBaseRow3.ItemArray = BASEtbl.Rows(workBasePos).ItemArray
                    End If

                    '固定項目
                    workBaseRow3("LINECNT") = workBasePos + 1
                    If Convert.ToString(INPtbl(i)("OPERATION")) <> errDisp Then
                        workBaseRow3("OPERATION") = updateDisp
                    Else
                        workBaseRow3("OPERATION") = INPtbl(i)("OPERATION")
                    End If
                    If workBasePos >= BASEtbl.Rows.Count Then
                        workBaseRow3("TIMSTP") = "0"                                 ' 新規レコード
                    Else
                        workBaseRow3("TIMSTP") = BASEtbl(workBasePos)("TIMSTP")      ' 更新レコード
                    End If
                    workBaseRow3("SELECT") = 1
                    workBaseRow3("HIDDEN") = 0

                    Dim stDate As Date = Nothing
                    Dim endDate As Date = Nothing
                    Dim ins5Date As Date = Nothing
                    Dim ins2P5Date As Date = Nothing
                    Dim nextInsDate As Date = Nothing
                    Dim manufactDate As Date = Nothing

                    'エラーの場合、値を更新しない
                    'エラーかつ新規の場合、値を設定する
                    'If Convert.ToString(workBaseRow3("OPERATION")) <> errDisp OrElse
                    '    (Convert.ToString(workBaseRow3("OPERATION")) = errDisp AndAlso newFlg) Then
                    '    '個別項目
                    '    workBaseRow3("COMPCODE") = INPtbl(i)("COMPCODE")
                    '    workBaseRow3("TANKNO") = INPtbl(i)("TANKNO")
                    '    If Date.TryParse(Convert.ToString(INPtbl(i)("STYMD")), stDate) Then
                    '        workBaseRow3("STYMD") = stDate.ToString("yyyy/MM/dd")
                    '    Else
                    '        workBaseRow3("STYMD") = INPtbl(i)("STYMD")
                    '    End If
                    '    If Date.TryParse(Convert.ToString(INPtbl(i)("ENDYMD")), endDate) Then
                    '        workBaseRow3("ENDYMD") = endDate.ToString("yyyy/MM/dd")
                    '    Else
                    '        workBaseRow3("ENDYMD") = INPtbl(i)("ENDYMD")
                    '    End If
                    '    workBaseRow3("PROPERTY") = INPtbl(i)("PROPERTY")
                    '    workBaseRow3("LMOF") = INPtbl(i)("LMOF")
                    '    workBaseRow3("LEASESTAT") = INPtbl(i)("LEASESTAT")
                    '    workBaseRow3("REPAIRSTAT") = INPtbl(i)("REPAIRSTAT")
                    '    If Date.TryParse(Convert.ToString(INPtbl(i)("INSPECTDATE5")), ins5Date) Then
                    '        workBaseRow3("INSPECTDATE5") = ins5Date.ToString("yyyy/MM/dd")
                    '    Else
                    '        workBaseRow3("INSPECTDATE5") = INPtbl(i)("INSPECTDATE5")
                    '    End If
                    '    If Date.TryParse(Convert.ToString(INPtbl(i)("INSPECTDATE2P5")), ins2P5Date) Then
                    '        workBaseRow3("INSPECTDATE2P5") = ins2P5Date.ToString("yyyy/MM/dd")
                    '    Else
                    '        workBaseRow3("INSPECTDATE2P5") = INPtbl(i)("INSPECTDATE2P5")
                    '    End If

                    '    If Date.TryParse(Convert.ToString(INPtbl(i)("NEXTINSPECTDATE")), nextInsDate) Then
                    '        workBaseRow3("NEXTINSPECTDATE") = nextInsDate.ToString("yyyy/MM/dd")
                    '    Else
                    '        workBaseRow3("NEXTINSPECTDATE") = INPtbl(i)("NEXTINSPECTDATE")
                    '    End If
                    '    workBaseRow3("NEXTINSPECTTYPE") = INPtbl(i)("NEXTINSPECTTYPE")

                    '    workBaseRow3("JAPFIREAPPROVED") = INPtbl(i)("JAPFIREAPPROVED")
                    '    workBaseRow3("MANUFACTURER") = INPtbl(i)("MANUFACTURER")
                    '    workBaseRow3("MANUFACTURESERIALNO") = INPtbl(i)("MANUFACTURESERIALNO")

                    '    If Date.TryParse(Convert.ToString(INPtbl(i)("DATEOFMANUFACTURE")), manufactDate) Then
                    '        workBaseRow3("DATEOFMANUFACTURE") = manufactDate.ToString("yyyy/MM/dd")
                    '    Else
                    '        workBaseRow3("DATEOFMANUFACTURE") = INPtbl(i)("DATEOFMANUFACTURE")
                    '    End If
                    '    workBaseRow3("MATERIAL") = INPtbl(i)("MATERIAL")
                    '    workBaseRow3("STRUCT") = INPtbl(i)("STRUCT")
                    '    workBaseRow3("USDOTAPPROVED") = INPtbl(i)("USDOTAPPROVED")
                    '    workBaseRow3("NOMINALCAPACITY") = INPtbl(i)("NOMINALCAPACITY")
                    '    workBaseRow3("TANKCAPACITY") = INPtbl(i)("TANKCAPACITY")
                    '    workBaseRow3("MAXGROSSWEIGHT") = INPtbl(i)("MAXGROSSWEIGHT")
                    '    workBaseRow3("NETWEIGHT") = INPtbl(i)("NETWEIGHT")
                    '    workBaseRow3("FREAMDIMENSION_H") = INPtbl(i)("FREAMDIMENSION_H")
                    '    workBaseRow3("FREAMDIMENSION_W") = INPtbl(i)("FREAMDIMENSION_W")
                    '    workBaseRow3("FREAMDIMENSION_L") = INPtbl(i)("FREAMDIMENSION_L")
                    '    workBaseRow3("HEATING") = INPtbl(i)("HEATING")
                    '    workBaseRow3("HEATING_SUB") = INPtbl(i)("HEATING_SUB")
                    '    workBaseRow3("DISCHARGE") = INPtbl(i)("DISCHARGE")
                    '    workBaseRow3("NOOFBOTTMCLOSURES") = INPtbl(i)("NOOFBOTTMCLOSURES")
                    '    workBaseRow3("IMCOCLASS") = INPtbl(i)("IMCOCLASS")
                    '    workBaseRow3("FOOTVALUETYPE") = INPtbl(i)("FOOTVALUETYPE")
                    '    workBaseRow3("BACKVALUETYPE") = INPtbl(i)("BACKVALUETYPE")
                    '    workBaseRow3("TOPDISVALUETYPE") = INPtbl(i)("TOPDISVALUETYPE")
                    '    workBaseRow3("AIRINLETVALUE") = INPtbl(i)("AIRINLETVALUE")
                    '    workBaseRow3("BAFFLES") = INPtbl(i)("BAFFLES")
                    '    workBaseRow3("TYPEOFPREVACVALUE") = INPtbl(i)("TYPEOFPREVACVALUE")
                    '    workBaseRow3("BURSTDISCFITTED") = INPtbl(i)("BURSTDISCFITTED")
                    '    workBaseRow3("TYPEOFTHERM") = INPtbl(i)("TYPEOFTHERM")
                    '    workBaseRow3("TYPEOFMANLID_CENTER") = INPtbl(i)("TYPEOFMANLID_CENTER")
                    '    workBaseRow3("TYPEOFMANLID_FRONT") = INPtbl(i)("TYPEOFMANLID_FRONT")
                    '    workBaseRow3("TYPEOFMLSEAL") = INPtbl(i)("TYPEOFMLSEAL")
                    '    workBaseRow3("WORKINGPRESSURE") = INPtbl(i)("WORKINGPRESSURE")
                    '    workBaseRow3("TESTPRESSURE") = INPtbl(i)("TESTPRESSURE")
                    '    workBaseRow3("REMARK1") = INPtbl(i)("REMARK1")
                    '    workBaseRow3("REMARK2") = INPtbl(i)("REMARK2")
                    '    workBaseRow3("FAULTS") = INPtbl(i)("FAULTS")
                    '    workBaseRow3("BASERAGEYY") = INPtbl(i)("BASERAGEYY")
                    '    workBaseRow3("BASERAGEMM") = INPtbl(i)("BASERAGEMM")
                    '    workBaseRow3("BASERAGE") = INPtbl(i)("BASERAGE")
                    '    workBaseRow3("BASELEASE") = INPtbl(i)("BASELEASE")
                    '    workBaseRow3("MARUKANSEAL") = INPtbl(i)("MARUKANSEAL")
                    '    workBaseRow3("REMARK") = INPtbl(i)("REMARK")
                    '    If Convert.ToString(INPtbl(i)("DELFLG")) = "" Then
                    '        workBaseRow3("DELFLG") = BaseDllCommon.CONST_FLAG_NO
                    '    Else
                    '        workBaseRow3("DELFLG") = INPtbl(i)("DELFLG")
                    '    End If

                    'End If
                    If Convert.ToString(workBaseRow3("OPERATION")) <> errDisp OrElse
                        (Convert.ToString(workBaseRow3("OPERATION")) = errDisp AndAlso newFlg) Then
                        '個別項目
                        workBaseRow3("COMPCODE") = drInput("COMPCODE")
                        workBaseRow3("TANKNO") = drInput("TANKNO")
                        If Date.TryParse(Convert.ToString(drInput("STYMD")), stDate) Then
                            workBaseRow3("STYMD") = stDate.ToString("yyyy/MM/dd")
                        Else
                            workBaseRow3("STYMD") = drInput("STYMD")
                        End If
                        If Date.TryParse(Convert.ToString(drInput("ENDYMD")), endDate) Then
                            workBaseRow3("ENDYMD") = endDate.ToString("yyyy/MM/dd")
                        Else
                            workBaseRow3("ENDYMD") = drInput("ENDYMD")
                        End If
                        workBaseRow3("PROPERTY") = drInput("PROPERTY")
                        workBaseRow3("LMOF") = drInput("LMOF")
                        workBaseRow3("LEASESTAT") = drInput("LEASESTAT")
                        workBaseRow3("REPAIRSTAT") = drInput("REPAIRSTAT")
                        If Date.TryParse(Convert.ToString(drInput("INSPECTDATE5")), ins5Date) Then
                            workBaseRow3("INSPECTDATE5") = ins5Date.ToString("yyyy/MM/dd")
                        Else
                            workBaseRow3("INSPECTDATE5") = drInput("INSPECTDATE5")
                        End If
                        If Date.TryParse(Convert.ToString(drInput("INSPECTDATE2P5")), ins2P5Date) Then
                            workBaseRow3("INSPECTDATE2P5") = ins2P5Date.ToString("yyyy/MM/dd")
                        Else
                            workBaseRow3("INSPECTDATE2P5") = drInput("INSPECTDATE2P5")
                        End If

                        If Date.TryParse(Convert.ToString(drInput("NEXTINSPECTDATE")), nextInsDate) Then
                            workBaseRow3("NEXTINSPECTDATE") = nextInsDate.ToString("yyyy/MM/dd")
                        Else
                            workBaseRow3("NEXTINSPECTDATE") = drInput("NEXTINSPECTDATE")
                        End If
                        workBaseRow3("NEXTINSPECTTYPE") = drInput("NEXTINSPECTTYPE")

                        workBaseRow3("JAPFIREAPPROVED") = drInput("JAPFIREAPPROVED")
                        workBaseRow3("MANUFACTURER") = drInput("MANUFACTURER")
                        workBaseRow3("MANUFACTURESERIALNO") = drInput("MANUFACTURESERIALNO")

                        If Date.TryParse(Convert.ToString(drInput("DATEOFMANUFACTURE")), manufactDate) Then
                            workBaseRow3("DATEOFMANUFACTURE") = manufactDate.ToString("yyyy/MM/dd")
                        Else
                            workBaseRow3("DATEOFMANUFACTURE") = drInput("DATEOFMANUFACTURE")
                        End If
                        workBaseRow3("MATERIAL") = drInput("MATERIAL")
                        workBaseRow3("STRUCT") = drInput("STRUCT")
                        workBaseRow3("USDOTAPPROVED") = drInput("USDOTAPPROVED")
                        workBaseRow3("NOMINALCAPACITY") = drInput("NOMINALCAPACITY")
                        workBaseRow3("TANKCAPACITY") = drInput("TANKCAPACITY")
                        workBaseRow3("MAXGROSSWEIGHT") = drInput("MAXGROSSWEIGHT")
                        workBaseRow3("NETWEIGHT") = drInput("NETWEIGHT")
                        workBaseRow3("FREAMDIMENSION_H") = drInput("FREAMDIMENSION_H")
                        workBaseRow3("FREAMDIMENSION_W") = drInput("FREAMDIMENSION_W")
                        workBaseRow3("FREAMDIMENSION_L") = drInput("FREAMDIMENSION_L")
                        workBaseRow3("HEATING") = drInput("HEATING")
                        workBaseRow3("HEATING_SUB") = drInput("HEATING_SUB")
                        workBaseRow3("DISCHARGE") = drInput("DISCHARGE")
                        workBaseRow3("NOOFBOTTMCLOSURES") = drInput("NOOFBOTTMCLOSURES")
                        workBaseRow3("IMCOCLASS") = drInput("IMCOCLASS")
                        workBaseRow3("FOOTVALUETYPE") = drInput("FOOTVALUETYPE")
                        workBaseRow3("BACKVALUETYPE") = drInput("BACKVALUETYPE")
                        workBaseRow3("TOPDISVALUETYPE") = drInput("TOPDISVALUETYPE")
                        workBaseRow3("AIRINLETVALUE") = drInput("AIRINLETVALUE")
                        workBaseRow3("BAFFLES") = drInput("BAFFLES")
                        workBaseRow3("TYPEOFPREVACVALUE") = drInput("TYPEOFPREVACVALUE")
                        workBaseRow3("BURSTDISCFITTED") = drInput("BURSTDISCFITTED")
                        workBaseRow3("TYPEOFTHERM") = drInput("TYPEOFTHERM")
                        workBaseRow3("TYPEOFMANLID_CENTER") = drInput("TYPEOFMANLID_CENTER")
                        workBaseRow3("TYPEOFMANLID_FRONT") = drInput("TYPEOFMANLID_FRONT")
                        workBaseRow3("TYPEOFMLSEAL") = drInput("TYPEOFMLSEAL")
                        workBaseRow3("WORKINGPRESSURE") = drInput("WORKINGPRESSURE")
                        workBaseRow3("TESTPRESSURE") = drInput("TESTPRESSURE")
                        workBaseRow3("REMARK1") = drInput("REMARK1")
                        workBaseRow3("REMARK2") = drInput("REMARK2")
                        workBaseRow3("FAULTS") = drInput("FAULTS")
                        workBaseRow3("BASERAGEYY") = drInput("BASERAGEYY")
                        workBaseRow3("BASERAGEMM") = drInput("BASERAGEMM")
                        workBaseRow3("BASERAGE") = drInput("BASERAGE")
                        workBaseRow3("BASELEASE") = drInput("BASELEASE")
                        workBaseRow3("MARUKANSEAL") = drInput("MARUKANSEAL")
                        workBaseRow3("NEWTANKPORT") = drInput("NEWTANKPORT")
                        workBaseRow3("NEWTANKACTY") = drInput("NEWTANKACTY")
                        workBaseRow3("ACTYCNT") = drInput("ACTYCNT")
                        workBaseRow3("REMARK") = drInput("REMARK")
                        If Convert.ToString(drInput("DELFLG")) = "" Then
                            workBaseRow3("DELFLG") = BaseDllCommon.CONST_FLAG_NO
                        Else
                            workBaseRow3("DELFLG") = drInput("DELFLG")
                        End If

                    End If

                    If workBasePos < BASEtbl.Rows.Count Then
                        '更新対象を設定
                        BASEtbl.Rows(workBasePos).ItemArray = workBaseRow3.ItemArray
                    Else
                        BASEtbl.Rows.Add(workBaseRow3)
                    End If
                End If
            End If
        Next

    End Sub
    ''' <summary>
    ''' 一覧 明細行ダブルクリック処理 (List ---> detailbox)  
    ''' </summary>
    Protected Sub List_DBclick()
        Dim COA0014DetailView As New BASEDLL.COA0014DetailView          'DetailView設定
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル
        Dim dataTable As DataTable = New DataTable

        Me.txtRightErrorMessage.Text = ""
        Me.lblFooterMessage.Text = ""

        '画面表示データ復元 
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        '画面detailboxへ表示
        '画面選択明細から画面detailboxへ表示

        '抽出条件(ヘッダーレコードより)定義
        Dim lineCnt As Integer

        'LINECNT
        Try
            Integer.TryParse(hdnListDbClick.Value, lineCnt)
            lineCnt = lineCnt - 1
        Catch ex As Exception
            Return
        End Try

        dataTable = BASEtbl.Clone
        dataTable.ImportRow(BASEtbl(lineCnt))

        'ダブルクリック明細情報取得設定（Detailboxヘッダー情報)
        ' 選択行
        lblLineCntText.Text = Convert.ToString(dataTable(0)("LINECNT"))
        lblApplyIDText.Text = Convert.ToString(dataTable(0)("APPLYID"))
        txtCompCode.Text = Convert.ToString(dataTable(0)("COMPCODE"))
        txtCompCode_Change()
        txtProperty.Text = Convert.ToString(dataTable(0)("PROPERTY"))
        txtProperty_Change()
        txtTankNo.Text = Convert.ToString(dataTable(0)("TANKNO"))
        txtStYMD.Text = FormatDateContrySettings(Convert.ToString(dataTable(0)("STYMD")), GBA00003UserSetting.DATEFORMAT)
        txtEndYMD.Text = FormatDateContrySettings(Convert.ToString(dataTable(0)("ENDYMD")), GBA00003UserSetting.DATEFORMAT)
        txtDelFlg.Text = Convert.ToString(dataTable(0)("DELFLG"))
        txtDelFlg_Change()

        'ボタン制御
        SetButtonControl()

        'ダブルクリック明細情報取得設定（Detailbox情報)
        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = Me.hdnViewId.Value
        COA0014DetailView.TABID = ""
        COA0014DetailView.SRCDATA = dataTable
        COA0014DetailView.REPEATER = WF_DViewRep1
        COA0014DetailView.COLPREFIX = "WF_Rep1_"
        COA0014DetailView.COA0014SetDetailView()

        'Detail初期設定
        SetDetailDbClick()

        INSPECTDATE5_Change()
        LMOF_Change()
        LEASESTAT_Change()
        REPAIRSTAT_Change()

        JAPFIREAPPROVED_Change()
        STRUCT_Change()
        USDOTAPPROVED_Change()
        DISCHARGE_Change()
        BAFFLES_Change()
        BURSTDISCFITTED_Change()
        TYPEOFTHERM_Change()
        TYPEOFMLSEAL_Change()
        MARUKANSEAL_Change()

        NEWTANKPORT_Change()
        NEWTANKACTY_Change()

        NEXTINSPECTTYPE_Change()

        'タブ別処理(書類（PDF）)
        PDFInitRead(txtCompCode.Text, txtTankNo.Text)

        '画面WF_GRID状態設定
        '状態をクリア設定
        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            Select Case Convert.ToString(BASEtbl.Rows(i)(1))
                Case ""
                    BASEtbl.Rows(i)(1) = ""
                Case "&nbsp;"
                    BASEtbl.Rows(i)(1) = ""
                Case "★"
                    BASEtbl.Rows(i)(1) = ""
                Case "★" & updateDisp
                    BASEtbl.Rows(i)(1) = updateDisp
                Case "★" & errDisp
                    BASEtbl.Rows(i)(1) = errDisp
            End Select
        Next

        '選択明細のOperation項目に状態を設定(更新・追加・削除は編集中を設定しない)
        Select Case Convert.ToString(BASEtbl.Rows(lineCnt)(1))
            Case ""
                BASEtbl.Rows(lineCnt)(1) = "★"
            Case "&nbsp;"
                BASEtbl.Rows(lineCnt)(1) = "★"
            Case "★"
                BASEtbl.Rows(lineCnt)(1) = "★"
            Case updateDisp
                BASEtbl.Rows(lineCnt)(1) = "★" & updateDisp
            Case errDisp
                BASEtbl.Rows(lineCnt)(1) = "★" & errDisp
            Case Else
        End Select

        '画面表示データ保存
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        BASEtbl.Clear()
        BASEtbl.Dispose()

        '画面編集
        txtStYMD.Focus()

    End Sub
    ''' <summary>
    ''' Detail タブ切替処理
    ''' </summary>
    Protected Sub DetailTABChange()

        Dim DTABChangeVal As Integer
        Try
            Integer.TryParse(hdnDTABChange.Value, DTABChangeVal)
        Catch ex As Exception
            DTABChangeVal = 0
        End Try

        WF_DetailMView.ActiveViewIndex = DTABChangeVal

        '初期値（書式）変更

        'タンク情報
        lblDtabTank.Style.Remove("color")
        lblDtabTank.Style.Add("color", "black")
        lblDtabTank.Style.Remove("background-color")
        lblDtabTank.Style.Add("background-color", "rgb(255,255,253)")
        lblDtabTank.Style.Remove("border")
        lblDtabTank.Style.Add("border", "1px solid black")
        lblDtabTank.Style.Remove("font-weight")
        lblDtabTank.Style.Add("font-weight", "normal")

        '書類（PDF） 
        lblDtabDocument.Style.Remove("color")
        lblDtabDocument.Style.Add("color", "black")
        lblDtabDocument.Style.Remove("background-color")
        lblDtabDocument.Style.Add("background-color", "rgb(255,255,253)")
        lblDtabDocument.Style.Remove("border")
        lblDtabDocument.Style.Add("border", "1px solid black")
        lblDtabDocument.Style.Remove("font-weight")
        lblDtabDocument.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '届先情報
                lblDtabTank.Style.Remove("color")
                lblDtabTank.Style.Add("color", "blue")
                lblDtabTank.Style.Remove("background-color")
                lblDtabTank.Style.Add("background-color", "rgb(220,230,240)")
                lblDtabTank.Style.Remove("border")
                lblDtabTank.Style.Add("border", "1px solid blue")
                lblDtabTank.Style.Remove("font-weight")
                lblDtabTank.Style.Add("font-weight", "bold")
            Case 1
                '書類（PDF） 
                lblDtabDocument.Style.Remove("color")
                lblDtabDocument.Style.Add("color", "blue")
                lblDtabDocument.Style.Remove("background-color")
                lblDtabDocument.Style.Add("background-color", "rgb(220,230,240)")
                lblDtabDocument.Style.Remove("border")
                lblDtabDocument.Style.Add("border", "1px solid blue")
                lblDtabDocument.Style.Remove("font-weight")
                lblDtabDocument.Style.Add("font-weight", "bold")

        End Select

    End Sub
    ''' <summary>
    ''' PDFファイルアップロード入力処理(PDFドロップ時)
    ''' </summary>
    Protected Sub UploadPDF()
        '初期設定
        Dim UpDir As String = Nothing

        '事前確認
        '一覧に存在かチェック

        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            If txtCompCode.Text = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) OrElse
                txtTankNo.Text = Convert.ToString(BASEtbl.Rows(i)("TANKNO")) Then
                Exit For
            Else
                If (i - 1) >= BASEtbl.Rows.Count Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage, pageObject:=Me)
                    Return
                Else
                End If
            End If
        Next

        'アップロードファイル名を取得　＆　移動
        UpDir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK\"
        UpDir = UpDir & "TANK\" & txtTankNo.Text & "\Update_D"

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(UpDir) = False Then
            System.IO.Directory.CreateDirectory(UpDir)
        End If

        For Each tempFile As String In System.IO.Directory.GetFiles(COA0019Session.UPLOADDir & "\" & COA0019Session.USERID, "*.*")

            'ディレクトリ付ファイル名より、ファイル名編集
            Dim DirFile As String = System.IO.Path.GetFileName(tempFile)
            '正式フォルダ内全PDF→Update_Hフォルダへ上書コピー
            Try
                System.IO.File.Copy(tempFile, UpDir & "\" & DirFile, True)
                System.IO.File.Delete(tempFile)
            Catch ex As Exception
            End Try

            Exit For
        Next

        '画面編集
        'PDF格納ディレクトリ編集
        UpDir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\" & "GBM00006TANK" & "\"
        UpDir = UpDir & "TANK" & "\" & txtTankNo.Text & "\Update_D"

        '表更新前のUpdate_Dディレクトリ内ファイル(追加操作)
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        For Each tempFile As String In System.IO.Directory.GetFiles(UpDir, "*", System.IO.SearchOption.AllDirectories)
            Dim WW_tempFile As String = System.IO.Path.GetFileName(tempFile)
            If WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                'ファイルパス格納
                WW_Files_dir.Add(tempFile)
                'ファイル名格納
                WW_Files_name.Add(WW_tempFile)
                '削除フラグ格納
                WW_Files_del.Add(BaseDllCommon.CONST_FLAG_NO)
            End If
        Next

        'Repeaterバインド準備
        PDFtblColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            PDFrow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = BaseDllCommon.CONST_FLAG_NO
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)
        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

        'Repeaterへデータをセット
        For i As Integer = 0 To WW_Files_dir.Count - 1

            'ファイル記号名称
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text = WW_Files_name.Item(i)
            '削除
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Text = BaseDllCommon.CONST_FLAG_NO
            'FILEPATH
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), System.Web.UI.WebControls.Label).Text = WW_Files_dir.Item(i)
        Next

        'イベント設定
        Dim WW_ATTR As String = ""
        Dim ItemCnt As Integer = 0
        For i As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text & "')"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Attributes.Add("ondblclick", WW_ATTR)

            ItemCnt = i
            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "Field_DBclick('vLeftDelFlg' "
            'WW_ATTR = WW_ATTR & ", '" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Text & "'"
            WW_ATTR = WW_ATTR & ", '" & ItemCnt.ToString & "'"
            WW_ATTR = WW_ATTR & " )"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

        'メッセージ編集
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALIMPORT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

    End Sub
    ''' <summary>
    ''' PDFカラム設定
    ''' </summary>
    Protected Sub PDFtblColumnsAdd()

        If PDFtbl.Columns.Count <> 0 Then
            PDFtbl.Columns.Clear()
        End If

        'PDFtblテンポラリDB項目作成
        PDFtbl.Clear()

        PDFtbl.Columns.Add("FILENAME", GetType(String))
        PDFtbl.Columns.Add("DELFLG", GetType(String))
        PDFtbl.Columns.Add("FILEPATH", GetType(String))

    End Sub
    ''' <summary>
    ''' PDFDB更新処理
    ''' </summary>
    ''' <param name="prmCompCode"></param>
    ''' <param name="prmTankNo"></param>
    Protected Sub PDFDBupdate(ByVal prmCompCode As String, ByVal prmTankNo As String, ByVal prmApplyId As String)
        '初期設定
        Dim WW_DirSend As String = ""
        Dim WW_DirH As String = ""
        Dim WW_DirD As String = ""
        Dim WW_DirHON As String = ""
        Dim appFlg As String = ""

        '○FTP格納ディレクトリ編集

        If prmApplyId = "" Then
            '正式ディレクトリ
            WW_DirHON = COA0019Session.UPLOADFILESDir & "\TANK\" & prmTankNo
            appFlg = "2"
        Else
            '承認前ディレクトリ
            WW_DirHON = COA0019Session.BEFOREAPPROVALDir & "\TANK\" & prmTankNo
            appFlg = "1"
        End If

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(WW_DirHON) = False Then
            System.IO.Directory.CreateDirectory(WW_DirHON)
        End If

        'Tempフォルダーが存在したら処理する（EXCEL入力の場合、Tempができないため）
        WW_DirH = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK\"
        WW_DirH = WW_DirH & "TANK\" & prmTankNo & "\Update_H"
        If System.IO.Directory.Exists(WW_DirH) Then

            'PDF正式格納フォルダクリア処理
            For Each tempFile As String In System.IO.Directory.GetFiles(WW_DirHON, "*", System.IO.SearchOption.AllDirectories)
                'サブフォルダは対象外
                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next

            'Update_Hフォルダ内容をPDF正式格納フォルダへコピー
            For Each tempFile As String In System.IO.Directory.GetFiles(WW_DirH, "*", System.IO.SearchOption.AllDirectories)
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = System.IO.Path.GetFileName(tempFile)
                'Update_Hフォルダ内PDF→PDF正式格納フォルダへ上書コピー
                System.IO.File.Copy(tempFile, WW_DirHON & "\" & WW_File, True)
            Next

            'Update_Dフォルダクリア　※Update_Hフォルダは、連続処理に備えてクリアーしない
            WW_DirD = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK\"
            WW_DirD = WW_DirD & "TANK\" & prmTankNo & "\Update_D"

            For Each tempFile As String In System.IO.Directory.GetFiles(WW_DirD, "*", System.IO.SearchOption.AllDirectories)
                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next

            '集配信用フォルダ格納処理
            Dim COA00034SendDirectory As New COA00034SendDirectory
            Dim pgmDir As String = "\TANK\" & prmTankNo
            COA00034SendDirectory.SendDirectoryCopy(pgmDir, WW_DirHON, appFlg)

        End If
    End Sub
    ''' <summary>
    ''' PDF Tempディレクトリ削除(PAGE_load時)
    ''' </summary>
    Protected Sub PDFInitDel()
        Dim WW_UPdirs As String()
        Dim WW_UPfiles As String()

        'Temp納ディレクトリ編集
        'PDF格納Dir作成

        Dim WW_Dir As String = ""
        WW_Dir = WW_Dir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK\TANK"

        Dim WW_Dir_del As New List(Of String)

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(WW_Dir) = False Then
            System.IO.Directory.CreateDirectory(WW_Dir)
        End If

        'PDF格納ディレクトリ＞MC0006_TODOKESAKI\Temp\ユーザIDフォルダ内のファイル取得
        WW_UPdirs = System.IO.Directory.GetDirectories(WW_Dir, "*", System.IO.SearchOption.AllDirectories)
        For Each tempFile As String In WW_UPdirs
            'Tempの自ユーザ内フォルダを取得
            WW_Dir_del.Add(tempFile)
        Next

        'Listを降順に並べる⇒下位ディレクトリが先頭となる
        WW_Dir_del.Reverse()

        For i As Integer = 0 To WW_Dir_del.Count - 1
            'フォルダー内ファイル削除
            WW_UPfiles = System.IO.Directory.GetFiles(WW_Dir_del.Item(i), "*", System.IO.SearchOption.AllDirectories)
            'フォルダー内ファイル削除
            For Each tempFile As String In WW_UPfiles
                'ファイル削除
                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                    '読み取り専用などは削除できない
                End Try
            Next

            Try
                'ファイル削除
                System.IO.Directory.Delete(WW_Dir_del.Item(i))
            Catch ex As Exception
                'ファイルが残っている場合、削除できない
            End Try
        Next

    End Sub
    ''' <summary>
    ''' PDF読み込み ＆ ディレクトリ作成(Header・一覧ダブルクリック時)
    ''' </summary>
    ''' <param name="prmCompCode"></param>
    ''' <param name="prmTankNo"></param>
    Protected Sub PDFInitRead(ByVal prmCompCode As String, ByVal prmTankNo As String)
        Dim WW_UPfiles As String()

        '初期設定
        Dim WW_Dir As String

        '事前確認
        '一覧に存在するかチェック
        If prmCompCode = "" OrElse prmTankNo = "" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.PDFLISTEXISTS, Me.lblFooterMessage)
            Return
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If prmCompCode = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) OrElse
                   prmTankNo = Convert.ToString(BASEtbl.Rows(i)("TANKNO")) Then
                    Exit For
                Else
                    If (i - 1) >= BASEtbl.Rows.Count Then
                        CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage)
                        Return
                    Else
                    End If
                End If
            Next
        End If

        'フォルダ作成　＆　ファイルコピー
        'PDF格納ディレクトリ編集
        WW_Dir = ""
        WW_Dir = WW_Dir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK"

        '正式ディレクトリ作成＞タンクディレクトリ作成
        If lblApplyIDText.Text = "" Then
            If System.IO.Directory.Exists(COA0019Session.UPLOADFILESDir & "\TANK\" & prmTankNo) Then
            Else
                System.IO.Directory.CreateDirectory(COA0019Session.UPLOADFILESDir & "\TANK\" & prmTankNo)
            End If
        Else
            '承認前ディレクトリ
            If System.IO.Directory.Exists(COA0019Session.BEFOREAPPROVALDir & "\TANK\" & prmTankNo) Then
            Else
                System.IO.Directory.CreateDirectory(COA0019Session.BEFOREAPPROVALDir & "\TANK\" & prmTankNo)
            End If
        End If

        '一時保存ディレクトリ作成
        If System.IO.Directory.Exists(WW_Dir & "\TANK") Then
        Else
            System.IO.Directory.CreateDirectory(WW_Dir & "\TANK")
        End If

        '一時保存ディレクトリ＞タンクディレクトリ作成
        If System.IO.Directory.Exists(WW_Dir & "\TANK\" & prmTankNo) Then
        Else
            System.IO.Directory.CreateDirectory(WW_Dir & "\TANK\" & prmTankNo)
        End If

        '一時保存ディレクトリ＞タンクディレクトリ作成＞Update_H の処理
        If System.IO.Directory.Exists(WW_Dir & "\TANK\" & prmTankNo & "\Update_H") Then
            '連続処理の場合、前回処理を残す
        Else
            'ユーザIDディレクトリ＞タンクコードディレクトリ作成＞Update_H 作成
            System.IO.Directory.CreateDirectory(WW_Dir & "\TANK\" & prmTankNo & "\Update_H")

            '正式フォルダ内ファイル→一時保存ディレクトリ＞タンクディレクトリ作成＞Update_H へコピー
            If lblApplyIDText.Text = "" Then
                WW_UPfiles = System.IO.Directory.GetFiles(COA0019Session.UPLOADFILESDir & "\TANK\" & prmTankNo, "*", System.IO.SearchOption.AllDirectories)
            Else
                '承認前
                WW_UPfiles = System.IO.Directory.GetFiles(COA0019Session.BEFOREAPPROVALDir & "\TANK\" & prmTankNo, "*", System.IO.SearchOption.AllDirectories)
            End If

            For Each tempFile As String In WW_UPfiles
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = System.IO.Path.GetFileName(tempFile)
                '正式フォルダ内全PDF→Update_Hフォルダへ上書コピー
                System.IO.File.Copy(tempFile, WW_Dir & "\TANK\" & prmTankNo & "\Update_H\" & WW_File, True)
            Next
        End If

        '一時保存ディレクトリ＞ユーザIDディレクトリ作成＞タンクディレクトリ作成＞Update_D 処理
        If System.IO.Directory.Exists(WW_Dir & "\TANK\" & prmTankNo & "\Update_D") Then
            'Update_Dフォルダ内ファイル削除
            WW_UPfiles = System.IO.Directory.GetFiles(WW_Dir & "\TANK\" & prmTankNo & "\Update_D", "*", System.IO.SearchOption.AllDirectories)
            For Each tempFile As String In WW_UPfiles
                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next
        Else
            'Update_Dが存在しない場合、Update_Dフォルダ作成
            System.IO.Directory.CreateDirectory(WW_Dir & "\TANK\" & prmTankNo & "\Update_D")
        End If

        'Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
        WW_UPfiles = System.IO.Directory.GetFiles(WW_Dir & "\TANK\" & prmTankNo & "\Update_H", "*", System.IO.SearchOption.AllDirectories)
        For Each tempFile As String In WW_UPfiles
            'ディレクトリ付ファイル名より、ファイル名編集
            Dim WW_File As String = System.IO.Path.GetFileName(tempFile)
            'Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
            System.IO.File.Copy(tempFile, WW_Dir & "\TANK\" & prmTankNo & "\Update_D\" & WW_File, True)
        Next

        '画面編集
        'PDF格納ディレクトリ編集
        WW_Dir = ""
        WW_Dir = WW_Dir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK\"
        WW_Dir = WW_Dir & "TANK\" & prmTankNo & "\Update_D"

        '表更新前のUpdate_Dディレクトリ内ファイル一覧
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        WW_UPfiles = System.IO.Directory.GetFiles(WW_Dir, "*", System.IO.SearchOption.AllDirectories)
        For Each tempFile As String In WW_UPfiles
            Dim WW_tempFile As String = System.IO.Path.GetFileName(tempFile)
            If WW_Files_name.IndexOf(WW_tempFile) = -1 Then
                'ファイルパス格納
                WW_Files_dir.Add(tempFile)
                'ファイル名格納
                WW_Files_name.Add(WW_tempFile)
                '削除フラグ格納
                WW_Files_del.Add(BaseDllCommon.CONST_FLAG_NO)
            End If
        Next

        'Repeaterバインド準備
        PDFtblColumnsAdd()

        For i As Integer = 0 To WW_Files_dir.Count - 1
            PDFrow = PDFtbl.NewRow
            PDFrow("FILENAME") = WW_Files_name.Item(i)
            PDFrow("DELFLG") = BaseDllCommon.CONST_FLAG_NO
            PDFrow("FILEPATH") = WW_Files_dir.Item(i)
            PDFtbl.Rows.Add(PDFrow)

        Next

        'Repeaterバインド(空明細)
        WF_DViewRepPDF.DataSource = PDFtbl
        WF_DViewRepPDF.DataBind()

        DirectCast(hdnListBoxPDF, ListBox).Items.Clear()

        'Repeaterへデータをセット
        For i As Integer = 0 To WW_Files_dir.Count - 1

            'ファイル記号名称
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text = WW_Files_name.Item(i)
            '削除
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Text = BaseDllCommon.CONST_FLAG_NO
            'FILEPATH
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), System.Web.UI.WebControls.Label).Text = WW_Files_dir.Item(i)

            hdnListBoxPDF.Items.Add(New ListItem(WW_Files_name.Item(i), BaseDllCommon.CONST_FLAG_NO))
        Next

        'イベント設定
        Dim WW_ATTR As String = ""
        Dim ItemCnt As Integer = 0
        For i As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text & "')"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Attributes.Add("ondblclick", WW_ATTR)

            ItemCnt = i
            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "Field_DBclick('vLeftDelFlg' "
            'WW_ATTR = WW_ATTR & ", '" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Text & "'"
            WW_ATTR = WW_ATTR & ", '" & ItemCnt.ToString & "'"
            WW_ATTR = WW_ATTR & " )"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

    End Sub
    ''' <summary>
    ''' DetailPDF内容表示（Detail・PDFダブルクリック時（内容照会））
    ''' </summary>
    Protected Sub DTABPDFdisplay()

        Dim WW_Dir As String = COA0019Session.PRINTWORKDir & "\" & COA0019Session.USERID

        'セッション変数設定
        '固定項目設定
        Session("Class") = "DTABPDFdisplay"

        For i As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
            'ダブルクリック時コード検索イベント追加
            If DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text = hdnDTABPDFEXCELdisplay.Value Then
                'ディレクトリが存在しない場合、作成する
                If System.IO.Directory.Exists(WW_Dir) = False Then
                    System.IO.Directory.CreateDirectory(WW_Dir)
                End If

                'ダウンロードファイル送信準備
                System.IO.File.Copy(DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), System.Web.UI.WebControls.Label).Text,
                                    WW_Dir & "\" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), System.Web.UI.WebControls.Label).Text, True)

                'ダウンロード処理へ遷移
                hdnPrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & COA0019Session.PRINTROOTUrl & "/" & COA0019Session.USERID & "/" &
                                    Uri.EscapeUriString(DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text)
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

                Exit For
            End If
        Next

    End Sub
    ''' <summary>
    ''' PDF表更新時処理（Detail・表更新ボタン押下時）
    ''' </summary>
    Protected Sub PDFTabListUpdate()
        '初期設定
        Dim WW_Dir As String

        '事前確認
        '一覧に存在かチェック
        If txtCompCode.Text = "" OrElse txtTankNo.Text = "" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.PDFLISTEXISTS, Me.lblFooterMessage)
            Return
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If txtCompCode.Text = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) OrElse
                    txtTankNo.Text = Convert.ToString(BASEtbl.Rows(i)("TANKNO")) Then
                    Exit For
                Else
                    If (i - 1) >= BASEtbl.Rows.Count Then
                        CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage)
                        Return
                    Else
                    End If
                End If
            Next
        End If

        '画面・削除入力処理
        'Detail・表示PDFが、削除フラグONの場合、Update_Dフォルダ内該当PDFを直接削除
        '　※WF_Rep_FILEPATHは、Update_Dフォルダ内該当PDFを示す。

        For i As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
            If DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), System.Web.UI.WebControls.TextBox).Text = BaseDllCommon.CONST_FLAG_YES Then
                Try
                    System.IO.File.Delete(DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), System.Web.UI.WebControls.Label).Text)
                Catch ex As Exception
                End Try
            End If
        Next

        'ファイルコピー

        'Update_Hフォルダクリア処理
        WW_Dir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK\"
        WW_Dir = WW_Dir & "TANK\" & txtTankNo.Text & "\Update_H"

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(WW_Dir) = False Then
            System.IO.Directory.CreateDirectory(WW_Dir)
        End If

        For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*", System.IO.SearchOption.AllDirectories)
            Try
                System.IO.File.Delete(tempFile)
            Catch ex As Exception
            End Try
        Next

        'Update_Dフォルダ内容をUpdate_Hフォルダへコピー
        WW_Dir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK\"
        WW_Dir = WW_Dir & "TANK\" & txtTankNo.Text

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(WW_Dir & "\Update_D") = False Then
            System.IO.Directory.CreateDirectory(WW_Dir & "\Update_D")
        End If

        For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir & "\Update_D", "*", System.IO.SearchOption.AllDirectories)
            'ディレクトリ付ファイル名より、ファイル名編集
            Dim WW_File As String = System.IO.Path.GetFileName(tempFile)
            'Update_Dフォルダ内PDF→Update_Hフォルダへ上書コピー
            System.IO.File.Copy(tempFile, WW_Dir & "\Update_H\" & WW_File, True)
        Next

        'Update_Dフォルダクリア
        WW_Dir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00006TANK\"
        WW_Dir = WW_Dir & "TANK\" & txtTankNo.Text & "\Update_D"
        For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*", System.IO.SearchOption.AllDirectories)
            Try
                System.IO.File.Delete(tempFile)
            Catch ex As Exception
            End Try
        Next

    End Sub
    ''' <summary>
    ''' オペレーションリストアイテムを設定
    ''' </summary>
    Private Sub SetOperationListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbOperation.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "OPERATION"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbOperation
        Else
            COA0017FixValue.LISTBOX2 = Me.lbOperation
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbOperation = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbOperation = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBM00006SELECT Then
            '検索画面の場合
            Dim prevObj As GBM00006SELECT = DirectCast(Page.PreviousPage, GBM00006SELECT)

            Me.hdnSelectedStYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtStYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            Me.hdnSelectedEndYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtEndYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            If hdnSelectedEndYMD.Value = "" Then
                hdnSelectedEndYMD.Value = hdnSelectedStYMD.Value
            End If

            Me.hdnSelectedTankNo.Value = DirectCast(prevObj.FindControl("txtTankNo"), TextBox).Text

            Me.hdnViewId.Value = DirectCast(prevObj.FindControl("lbRightList"), ListBox).SelectedValue
        ElseIf TypeOf Page.PreviousPage Is GBT00006RESULT Then
            'タンクステータス一覧からの遷移
            Dim prevObj As GBT00006RESULT = DirectCast(Page.PreviousPage, GBT00006RESULT)
            ViewState("GBM00006ITEM") = prevObj.DisplayItems
            Me.hdnSelectedTankNo.Value = prevObj.DisplayItems.Gbt00006SelectedTankNo
            Me.hdnSelectedStYMD.Value = Date.Now.ToString("yyyy/MM/dd")
            Me.hdnSelectedEndYMD.Value = Date.Now.ToString("yyyy/MM/dd")
            Me.hdnViewId.Value = "Default" '一旦Default（導線が無いためどうするか要検討）
            Me.hdnThisMapVariant.Value = "Default"
            If prevObj.Request.Form("hdnListSortValueGBT00006RWF_LISTAREA") IsNot Nothing Then
                Me.hdnListSortValueGBT00006RWF_LISTAREA.Value = prevObj.Request.Form("hdnListSortValueGBT00006RWF_LISTAREA")
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00012REPAIR Then
            'リペアブレーカーからの遷移
            Dim prevObj As GBT00012REPAIR = DirectCast(Page.PreviousPage, GBT00012REPAIR)
            ViewState("GBM00006ITEM") = prevObj.DisplayItems
            Me.hdnSelectedTankNo.Value = DirectCast(prevObj.FindControl("txtTankNo"), TextBox).Text
            Me.hdnSelectedStYMD.Value = Date.Now.ToString("yyyy/MM/dd")
            Me.hdnSelectedEndYMD.Value = Date.Now.ToString("yyyy/MM/dd")
            Me.hdnViewId.Value = "Default" '一旦Default（導線が無いためどうするか要検討）
            'Me.hdnThisMapVariant.Value = "Default"

        ElseIf TypeOf Page.PreviousPage Is GBT00019APPROVAL Then

            Me.hdnSelectedStYMD.Value = Date.Now.ToString("yyyy/MM/dd")

            Me.hdnSelectedEndYMD.Value = Date.Now.ToString("yyyy/MM/dd")

            Me.hdnViewId.Value = "Default"

            Me.hdnThisMapVariant.Value = "GB_ShowTankDetail"

            Dim prevObj As GBT00019APPROVAL = DirectCast(Page.PreviousPage, GBT00019APPROVAL)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnMsgId", Me.hdnMsgId},
                                                                        {"hdnExtractTankNo", Me.hdnExtractTankNo},
                                                                        {"hdnExtractApp", Me.hdnExtractApp},
                                                                        {"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnOrderNo", Me.hdnOrderNo},
                                                                        {"hdnTankNo", Me.hdnTankNo},
                                                                        {"hdnPrevViewID", Me.hdnPrevViewID},
                                                                        {"hdnSelectTankNo", Me.hdnSelectedTankNo}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next

        ElseIf Page.PreviousPage Is Nothing Then

            ''タンク引当承認画面より遷移
            'If Convert.ToString(Request.Form("hdnSender")) = "GBT00019A" Then

            '    Me.hdnSelectedStYMD.Value = Date.Now.ToString("yyyy/MM/dd")

            '    Me.hdnSelectedEndYMD.Value = Date.Now.ToString("yyyy/MM/dd")

            '    Me.hdnSelectedTankNo.Value = Convert.ToString(Request.Form("hdnSelectTankNo"))

            '    Me.hdnViewId.Value = "Default"

            'Else

            Dim prevObj As GBM00000APPROVAL = DirectCast(Page.PreviousPage, GBM00000APPROVAL)

            Me.hdnSelectedApplyID.Value = Convert.ToString(Request.Form("hdnSelectedValue1"))
            Me.hdnSelectedStYMD.Value = FormatDateYMD(Convert.ToString(Request.Form("hdnSelectedValue2")), GBA00003UserSetting.DATEFORMAT)
            Me.hdnSelectedEndYMD.Value = FormatDateYMD(Convert.ToString(Request.Form("hdnSelectedValue3")), GBA00003UserSetting.DATEFORMAT)

            Me.hdnViewId.Value = "Default"

                Me.hdnMAPpermitCode.Value = "TRUE"
            'End If

        End If
    End Sub
    ''' <summary>
    ''' 表示非表示制御
    ''' </summary>
    Private Sub VisibleControls()

        If Page.PreviousPage Is Nothing Then


            Me.btnListUpdate.Visible = False
            Me.btnDbUpdate.Visible = False

        End If

    End Sub
    ''' <summary>
    ''' ボタン制御
    ''' </summary>
    Protected Sub SetButtonControl()

        If lblApplyIDText.Text <> "" Then
            btnListUpdate.Disabled = True
        Else
            btnListUpdate.Disabled = False
        End If

    End Sub
    ''' <summary>
    ''' 次回点検日設定
    ''' </summary>
    ''' <param name="date2_5">[IN]2.5年点検日</param>
    ''' <param name="date5">[IN]5年点検日</param>
    ''' <param name="nextVal">[OUT]次回点検日</param>
    ''' <param name="nextType">[OUT次回点検種類]</param>
    Private Sub SetNextInspect(ByVal date2_5 As String, ByVal date5 As String, ByVal dateManu As String, ByRef nextVal As String, ByRef nextType As String)
        Dim ins5 As String = "1900/01/01"
        Dim ins2h As String = "1900/01/01"
        Dim manufact As String = "1900/01/01"
        Dim val As String = ""
        Dim type As String = ""
        If date5 <> "" Then ins5 = date5
        If date2_5 <> "" Then ins2h = date2_5
        If dateManu <> "" Then manufact = dateManu

        Dim ins5Date As Date
        Dim ins2hDate As Date
        Dim manufactDate As Date
        Dim cnv As String = GBA00003UserSetting.DATEFORMAT.Replace("dd", "01")
        'If Not (ins5 = "" OrElse ins2h = "") Then
        If Not (manufact = "1900/01/01" AndAlso ins5 = "1900/01/01") Then
            ins5 = FormatDateYMD(ins5, GBA00003UserSetting.DATEFORMAT)
            ins2h = FormatDateYMD(ins2h, GBA00003UserSetting.DATEFORMAT)
            manufact = FormatDateYMD(manufact, GBA00003UserSetting.DATEFORMAT)
            ' 基準日
            If Date.TryParse(ins5, ins5Date) AndAlso Date.TryParse(ins2h, ins2hDate) AndAlso Date.TryParse(manufact, manufactDate) Then
                If ins5Date < manufactDate Then ins5Date = manufactDate
                If ins5Date >= ins2hDate Then
                    '「２．５年検査実施日」の方が小さい
                    type = "2.5"
                    val = ins5Date.AddMonths(30).ToString(cnv)
                ElseIf ins5Date < ins2hDate Then
                    '「５年検査実施日」の方が小さい
                    type = "5"
                    'If ins5Date.AddMonths(27) >= ins2hDate Then
                    '    val = ins2hDate.AddMonths(30).ToString(cnv)
                    'ElseIf ins5Date.AddMonths(27) < ins2hDate Then
                    '    val = ins5Date.AddMonths(60).ToString(cnv)
                    'End If
                    val = ins5Date.AddMonths(60).ToString(cnv)
                End If
            End If
        End If
        nextVal = val
        nextType = type
    End Sub

    ''' <summary>
    ''' New Tank所在登録
    ''' </summary>
    ''' <param name="dr">オーダーNo</param>
    Private Sub InsertNewTankOrder(dr As DataRow)

        Dim canCloseConnect As Boolean = False
        Dim procDateTime As DateTime = DateTime.Now

        Dim sqlStat As New StringBuilder
        ' Order baseを物理削除
        sqlStat.AppendFormat("DELETE OB FROM {0} AS OB ", CONST_TBLORDERB).AppendLine()
        sqlStat.AppendFormat("  INNER JOIN {0} AS OV ", CONST_TBLORDERV).AppendLine()
        sqlStat.AppendLine("    ON OV.ACTIONID IN (SELECT KEYCODE FROM COS0017_FIXVALUE WHERE CLASS = 'NEWTANKACTY' AND DELFLG <> 'Y')")
        sqlStat.AppendLine("   AND OV.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("   AND OV.DISPSEQ    = @NEWDISPSEQ")
        sqlStat.AppendLine("   AND OV.ACTUALDATE = @NEWACTUAL")
        sqlStat.AppendLine("   AND OV.INITYMD    = @NEWYMD")
        sqlStat.AppendLine("   AND OV.INITUSER   = @NEWUSER")
        sqlStat.AppendLine("   AND OV.TANKNO     = @TANKNO")
        sqlStat.AppendLine("   AND OV.ORDERNO     = OB.ORDERNO;")

        ' Order baseを登録
        sqlStat.AppendFormat("INSERT INTO {0} (", CONST_TBLORDERB).AppendLine()
        sqlStat.AppendLine("        ORDERNO")
        sqlStat.AppendLine("       ,STYMD")
        sqlStat.AppendLine("       ,ENDYMD")
        sqlStat.AppendLine("       ,BRID")
        sqlStat.AppendLine("       ,BRTYPE")
        sqlStat.AppendLine("       ,VALIDITYFROM")
        sqlStat.AppendLine("       ,VALIDITYTO")
        sqlStat.AppendLine("       ,TERMTYPE")
        sqlStat.AppendLine("       ,NOOFTANKS")
        sqlStat.AppendLine("       ,SHIPPER")
        sqlStat.AppendLine("       ,CONSIGNEE")
        sqlStat.AppendLine("       ,CARRIER1")
        sqlStat.AppendLine("       ,CARRIER2")
        sqlStat.AppendLine("       ,PRODUCTCODE")
        sqlStat.AppendLine("       ,PRODUCTWEIGHT")
        sqlStat.AppendLine("       ,RECIEPTCOUNTRY1")
        sqlStat.AppendLine("       ,RECIEPTPORT1")
        sqlStat.AppendLine("       ,RECIEPTCOUNTRY2")
        sqlStat.AppendLine("       ,RECIEPTPORT2")
        sqlStat.AppendLine("       ,LOADCOUNTRY1")
        sqlStat.AppendLine("       ,LOADPORT1")
        sqlStat.AppendLine("       ,LOADCOUNTRY2")
        sqlStat.AppendLine("       ,LOADPORT2")
        sqlStat.AppendLine("       ,DISCHARGECOUNTRY1")
        sqlStat.AppendLine("       ,DISCHARGEPORT1")
        sqlStat.AppendLine("       ,DISCHARGECOUNTRY2")
        sqlStat.AppendLine("       ,DISCHARGEPORT2")
        sqlStat.AppendLine("       ,DELIVERYCOUNTRY1")
        sqlStat.AppendLine("       ,DELIVERYPORT1")
        sqlStat.AppendLine("       ,DELIVERYCOUNTRY2")
        sqlStat.AppendLine("       ,DELIVERYPORT2")
        sqlStat.AppendLine("       ,VSL1")
        sqlStat.AppendLine("       ,VOY1")
        sqlStat.AppendLine("       ,ETD1")
        sqlStat.AppendLine("       ,ETA1")
        sqlStat.AppendLine("       ,VSL2")
        sqlStat.AppendLine("       ,VOY2")
        sqlStat.AppendLine("       ,ETD2")
        sqlStat.AppendLine("       ,ETA2")
        sqlStat.AppendLine("       ,INVOICEDBY")
        sqlStat.AppendLine("       ,LOADING")
        sqlStat.AppendLine("       ,STEAMING")
        sqlStat.AppendLine("       ,TIP")
        sqlStat.AppendLine("       ,EXTRA")
        sqlStat.AppendLine("       ,DEMURTO")
        sqlStat.AppendLine("       ,DEMURUSRATE1")
        sqlStat.AppendLine("       ,DEMURUSRATE2")
        sqlStat.AppendLine("       ,SALESPIC")
        sqlStat.AppendLine("       ,AGENTORGANIZER")
        sqlStat.AppendLine("       ,AGENTPOL1")
        sqlStat.AppendLine("       ,AGENTPOL2")
        sqlStat.AppendLine("       ,AGENTPOD1")
        sqlStat.AppendLine("       ,AGENTPOD2")
        sqlStat.AppendLine("       ,USINGLEASETANK")
        sqlStat.AppendLine("       ,BLID1")
        sqlStat.AppendLine("       ,BLAPPDATE1")
        sqlStat.AppendLine("       ,BLID2")
        sqlStat.AppendLine("       ,BLAPPDATE2")
        sqlStat.AppendLine("       ,SHIPPERNAME")
        sqlStat.AppendLine("       ,SHIPPERTEXT")
        sqlStat.AppendLine("       ,SHIPPERTEXT2")
        sqlStat.AppendLine("       ,CONSIGNEENAME")
        sqlStat.AppendLine("       ,CONSIGNEETEXT")
        sqlStat.AppendLine("       ,CONSIGNEETEXT2")
        sqlStat.AppendLine("       ,IECCODE")
        sqlStat.AppendLine("       ,NOTIFYNAME")
        sqlStat.AppendLine("       ,NOTIFYTEXT")
        sqlStat.AppendLine("       ,NOTIFYTEXT2")
        sqlStat.AppendLine("       ,NOTIFYCONT")
        sqlStat.AppendLine("       ,NOTIFYCONTNAME")
        sqlStat.AppendLine("       ,NOTIFYCONTTEXT1")
        sqlStat.AppendLine("       ,NOTIFYCONTTEXT2")
        sqlStat.AppendLine("       ,PRECARRIAGETEXT")
        sqlStat.AppendLine("       ,PRECARRIAGETEXT2")
        sqlStat.AppendLine("       ,VSL")
        sqlStat.AppendLine("       ,VOY")
        sqlStat.AppendLine("       ,FINDESTINATIONNAME")
        sqlStat.AppendLine("       ,FINDESTINATIONTEXT")
        sqlStat.AppendLine("       ,FINDESTINATIONTEXT2")
        sqlStat.AppendLine("       ,PRODUCT")
        sqlStat.AppendLine("       ,PRODUCTPORDER")
        sqlStat.AppendLine("       ,PRODUCTTIP")
        sqlStat.AppendLine("       ,PRODUCTFREIGHT")
        sqlStat.AppendLine("       ,FREIGHTANDCHARGES")
        sqlStat.AppendLine("       ,PREPAIDAT")
        sqlStat.AppendLine("       ,GOODSPKGS")
        sqlStat.AppendLine("       ,CONTAINERPKGS")
        sqlStat.AppendLine("       ,BLNUM")
        sqlStat.AppendLine("       ,CONTAINERNO")
        sqlStat.AppendLine("       ,SEALNO")
        sqlStat.AppendLine("       ,NOOFCONTAINER")
        sqlStat.AppendLine("       ,DECLAREDVALUE")
        sqlStat.AppendLine("       ,DECLAREDVALUE2")
        sqlStat.AppendLine("       ,REVENUETONS")
        sqlStat.AppendLine("       ,REVENUETONS2")
        sqlStat.AppendLine("       ,RATE")
        sqlStat.AppendLine("       ,RATE2")
        sqlStat.AppendLine("       ,PER")
        sqlStat.AppendLine("       ,PER2")
        sqlStat.AppendLine("       ,PREPAID")
        sqlStat.AppendLine("       ,PREPAID2")
        sqlStat.AppendLine("       ,COLLECT")
        sqlStat.AppendLine("       ,COLLECT2")
        sqlStat.AppendLine("       ,EXCHANGERATE")
        sqlStat.AppendLine("       ,PAYABLEAT")
        sqlStat.AppendLine("       ,LOCALCURRENCY")
        sqlStat.AppendLine("       ,CARRIERBLNO")
        sqlStat.AppendLine("       ,CARRIERBLNO2")
        sqlStat.AppendLine("       ,BOOKINGNO")
        sqlStat.AppendLine("       ,BOOKINGNO2")
        sqlStat.AppendLine("       ,NOOFPACKAGE")
        sqlStat.AppendLine("       ,BLTYPE")
        sqlStat.AppendLine("       ,BLTYPE2")
        sqlStat.AppendLine("       ,NOOFBL")
        sqlStat.AppendLine("       ,NOOFBL2")
        sqlStat.AppendLine("       ,PAYMENTPLACE")
        sqlStat.AppendLine("       ,PAYMENTPLACE2")
        sqlStat.AppendLine("       ,BLISSUEPLACE")
        sqlStat.AppendLine("       ,BLISSUEPLACE2")
        sqlStat.AppendLine("       ,ANISSUEPLACE")
        sqlStat.AppendLine("       ,ANISSUEPLACE2")
        sqlStat.AppendLine("       ,MEASUREMENT")
        sqlStat.AppendLine("       ,MEASUREMENT2")
        sqlStat.AppendLine("       ,MARKSANDNUMBERS")
        sqlStat.AppendLine("       ,TANKINFO")
        sqlStat.AppendLine("       ,LDNVSL1")
        sqlStat.AppendLine("       ,LDNPOL1")
        sqlStat.AppendLine("       ,LDNDATE1")
        sqlStat.AppendLine("       ,LDNBY1")
        sqlStat.AppendLine("       ,LDNVSL2")
        sqlStat.AppendLine("       ,LDNPOL2")
        sqlStat.AppendLine("       ,LDNDATE2")
        sqlStat.AppendLine("       ,LDNBY2")
        sqlStat.AppendLine("       ,CARRIERBLTYPE")
        sqlStat.AppendLine("       ,CARRIERBLTYPE2")
        sqlStat.AppendLine("       ,DEMUFORACCT")
        sqlStat.AppendLine("       ,DEMUFORACCT2")
        sqlStat.AppendLine("       ,BLRECEIPT1")
        sqlStat.AppendLine("       ,BLRECEIPT2")
        sqlStat.AppendLine("       ,BLLOADING1")
        sqlStat.AppendLine("       ,BLLOADING2")
        sqlStat.AppendLine("       ,BLDISCHARGE1")
        sqlStat.AppendLine("       ,BLDISCHARGE2")
        sqlStat.AppendLine("       ,BLDELIVERY1")
        sqlStat.AppendLine("       ,BLDELIVERY2")
        sqlStat.AppendLine("       ,BLPLACEDATEISSUE1")
        sqlStat.AppendLine("       ,BLPLACEDATEISSUE2")
        sqlStat.AppendLine("       ,REMARK")
        sqlStat.AppendLine("       ,DELFLG")
        sqlStat.AppendLine("       ,INITYMD")
        sqlStat.AppendLine("       ,INITUSER")
        sqlStat.AppendLine("       ,UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD")
        sqlStat.AppendLine(" ) VALUES ( ")
        sqlStat.AppendLine("        @ORDERNO")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,@BRID")
        sqlStat.AppendLine("       ,'OPERATION'")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,'CC'")
        sqlStat.AppendLine("       ,1")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,@NEWTANKCOUNTRY")
        sqlStat.AppendLine("       ,@NEWTANKPORT")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,@NEWTANKCOUNTRY")
        sqlStat.AppendLine("       ,@NEWTANKPORT")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,@NEWTANKCOUNTRY")
        sqlStat.AppendLine("       ,@NEWTANKPORT")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,@NEWTANKCOUNTRY")
        sqlStat.AppendLine("       ,@NEWTANKPORT")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,@NEWUSER")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,1")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'N'")
        sqlStat.AppendLine("       ,@NEWYMD")
        sqlStat.AppendLine("       ,@NEWUSER")
        sqlStat.AppendLine("       ,@UPDYMD")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine(");")

        ' Order Valueを物理削除
        sqlStat.AppendFormat("DELETE OV FROM {0} AS OV ", CONST_TBLORDERV).AppendLine()
        sqlStat.AppendLine("   WHERE OV.ACTIONID IN (SELECT KEYCODE FROM COS0017_FIXVALUE WHERE CLASS = 'NEWTANKACTY' AND DELFLG <> 'Y')")
        sqlStat.AppendLine("   AND OV.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("   AND OV.DISPSEQ    = @NEWDISPSEQ")
        sqlStat.AppendLine("   AND OV.ACTUALDATE = @NEWACTUAL")
        sqlStat.AppendLine("   AND OV.INITYMD    = @NEWYMD")
        sqlStat.AppendLine("   AND OV.INITUSER   = @NEWUSER")
        sqlStat.AppendLine("   AND OV.TANKNO     = @TANKNO;")

        ' Order Valueを登録
        sqlStat.AppendFormat("INSERT INTO {0} (", CONST_TBLORDERV).AppendLine()
        sqlStat.AppendLine("        ORDERNO")
        sqlStat.AppendLine("       ,STYMD")
        sqlStat.AppendLine("       ,ENDYMD")
        sqlStat.AppendLine("       ,TANKSEQ")
        sqlStat.AppendLine("       ,DTLPOLPOD")
        sqlStat.AppendLine("       ,DTLOFFICE")
        sqlStat.AppendLine("       ,TANKNO")
        sqlStat.AppendLine("       ,COSTCODE")
        sqlStat.AppendLine("       ,ACTIONID")
        sqlStat.AppendLine("       ,DISPSEQ")
        sqlStat.AppendLine("       ,LASTACT")
        sqlStat.AppendLine("       ,REQUIREDACT")
        sqlStat.AppendLine("       ,ORIGINDESTINATION")
        sqlStat.AppendLine("       ,COUNTRYCODE")
        sqlStat.AppendLine("       ,CURRENCYCODE")
        sqlStat.AppendLine("       ,TAXATION")
        sqlStat.AppendLine("       ,AMOUNTBR")
        sqlStat.AppendLine("       ,AMOUNTORD")
        sqlStat.AppendLine("       ,AMOUNTFIX")
        sqlStat.AppendLine("       ,CONTRACTORBR")
        sqlStat.AppendLine("       ,CONTRACTORODR")
        sqlStat.AppendLine("       ,CONTRACTORFIX")
        sqlStat.AppendLine("       ,SCHEDELDATEBR")
        sqlStat.AppendLine("       ,SCHEDELDATE")
        sqlStat.AppendLine("       ,ACTUALDATE")
        sqlStat.AppendLine("       ,LOCALBR")
        sqlStat.AppendLine("       ,LOCALRATE")
        sqlStat.AppendLine("       ,TAXBR")
        sqlStat.AppendLine("       ,AMOUNTPAY")
        sqlStat.AppendLine("       ,LOCALPAY")
        sqlStat.AppendLine("       ,TAXPAY")
        sqlStat.AppendLine("       ,INVOICEDBY")
        sqlStat.AppendLine("       ,APPLYID")
        sqlStat.AppendLine("       ,APPLYTEXT")
        sqlStat.AppendLine("       ,LASTSTEP")
        sqlStat.AppendLine("       ,SOAAPPDATE")
        sqlStat.AppendLine("       ,REMARK")
        sqlStat.AppendLine("       ,BRID")
        sqlStat.AppendLine("       ,BRCOST")
        sqlStat.AppendLine("       ,DATEFIELD")
        sqlStat.AppendLine("       ,DATEINTERVAL")
        sqlStat.AppendLine("       ,BRADDEDCOST")
        sqlStat.AppendLine("       ,AGENTORGANIZER")
        sqlStat.AppendLine("       ,CURRENCYSEGMENT")
        sqlStat.AppendLine("       ,ACCCRERATE")
        sqlStat.AppendLine("       ,ACCCREYEN")
        sqlStat.AppendLine("       ,ACCCREFOREIGN")
        sqlStat.AppendLine("       ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("       ,FORCECLOSED")
        sqlStat.AppendLine("       ,AMOUNTFIXBFC")
        sqlStat.AppendLine("       ,ACCCREYENBFC")
        sqlStat.AppendLine("       ,ACCCREFOREIGNBFC")
        sqlStat.AppendLine("       ,DELFLG")
        sqlStat.AppendLine("       ,INITYMD")
        sqlStat.AppendLine("       ,INITUSER")
        sqlStat.AppendLine("       ,UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD")
        sqlStat.AppendLine(" ) VALUES ( ")
        sqlStat.AppendLine("        @ORDERNO")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,'001'")
        sqlStat.AppendLine("       ,'POL1'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,@TANKNO")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,@NEWTANKACTY")
        sqlStat.AppendLine("       ,@NEWDISPSEQ")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,@NEWTANKCOUNTRY")
        sqlStat.AppendLine("       ,'USD'")
        sqlStat.AppendLine("       ,'0'")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,@NEWACTUAL")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'1900/01/01'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'ETD'")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,0")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,''")
        sqlStat.AppendLine("       ,'N'")
        sqlStat.AppendLine("       ,@NEWYMD")
        sqlStat.AppendLine("       ,@NEWUSER")
        sqlStat.AppendLine("       ,@UPDYMD")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine(");")

        Dim sqlCon As System.Data.SqlClient.SqlConnection = Nothing
        Dim tran As System.Data.SqlClient.SqlTransaction = Nothing

        Try

            sqlCon = New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open() '接続オープン
            tran = sqlCon.BeginTransaction

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                sqlCmd.Transaction = tran

                'SQLパラメータの設定
                With sqlCmd.Parameters
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = "TANKSET-" & Convert.ToString(dr("TANKNO"))
                    .Add("@BRID", SqlDbType.NVarChar).Value = "TANKSET-" & Convert.ToString(dr("TANKNO"))
                    .Add("@TANKNO", SqlDbType.NVarChar).Value = Convert.ToString(dr("TANKNO"))
                    .Add("@NEWTANKACTY", SqlDbType.NVarChar).Value = Convert.ToString(dr("NEWTANKACTY"))
                    .Add("@NEWTANKCOUNTRY", SqlDbType.NVarChar).Value = Left(Convert.ToString(dr("NEWTANKPORT")), 2)
                    .Add("@NEWTANKPORT", SqlDbType.NVarChar).Value = Convert.ToString(dr("NEWTANKPORT"))
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
                    .Add("@NEWYMD", SqlDbType.DateTime).Value = CONST_NEWYMD
                    .Add("@NEWUSER", SqlDbType.NVarChar).Value = CONST_NEWUSER
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@NEWDISPSEQ", SqlDbType.NVarChar).Value = CONST_NEWDISPSEQ
                    .Add("@NEWACTUAL", SqlDbType.DateTime).Value = CONST_NEWACTUAL
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.ExecuteNonQuery()

            End Using
            tran.Commit()

        Catch ex As Exception
            tran.Rollback()
            Throw
        Finally
            If sqlCon IsNot Nothing Then
                tran.Dispose()
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try

    End Sub

End Class
