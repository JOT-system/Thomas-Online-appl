Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net
Imports BASEDLL

''' <summary>
''' 積載品マスタ画面クラス
''' </summary>
Public Class GBM00008PRODUCT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00008"    '自身のMAPID
    Private Const CONST_BASEDATATABLE = "GBM00008TBL"
    Private Const CONST_INPDATATABLE = "GBM00008INPTBL"
    Private Const CONST_UPDDATATABLE = "GBM00008UPDTBL"
    Private Const CONST_PDFDATATABLE = "GBM00008PDFTBL"
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_TBLMASTER = "GBM0008_PRODUCT"
    Private Const CONST_TBLAPPLY = "GBM0011_PRODUCTAPPLY"
    Private Const CONST_EVENTCODE = "MasterApplyProduct"

    'Private Const NONDG = "NON-DG"

    Private Const TANKINIT = "T"
    Private Const PRPVINIT = "TP"

    Dim errListAll As List(Of String)                   'インポート全体のエラー
    Dim errList As List(Of String)                      'インポート中の１セット分のエラー
    Private returnCode As String = String.Empty         'サブ用リターンコード
    Private PDFrow As DataRow
    Dim errDisp As String = Nothing                     'エラー用表示文言
    Dim updateDisp As String = Nothing                  '更新用表示文言
    Dim warningDisp As String = Nothing                  '警告用表示文言

    Dim warningFlg As Boolean = False
    Private charConvList As ListBox = Nothing

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
    ''' 行のロウデータ
    ''' </summary>
    Private WORKrow As DataRow

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

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

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

            '表示用文言判定
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errDisp = "ERROR"
                updateDisp = "UPDATE"
                warningDisp = "WARNING"
            Else
                errDisp = "エラー"
                updateDisp = "更新"
                warningDisp = "警告"
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
                'タイトル設定
                '****************************************
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnThisMapVariant.Value
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                End If
                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '会社コード設定
                SetCompanyCode
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
                    .VARI = hdnViewId.Value
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
                SetPDFListBox()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                txtProductCodeEx.Focus()

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
                For i As Integer = 0 To DirectCast(COA0022ProfXls.REPORTOBJ, ListBox).Items.Count - 1
                    lbRightList.Items.Add(New ListItem(DirectCast(COA0022ProfXls.REPORTOBJ, ListBox).Items(i).Text, DirectCast(COA0022ProfXls.REPORTOBJ, ListBox).Items(i).Value))
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
                & "       PRODUCTCODE                        , " _
                & "       STYMD                              , " _
                & "       ENDYMD                             , " _
                & "       PRODUCTNAME                        , " _
                & "       CHEMICALNAME                       , " _
                & "       IMDGCODE                           , " _
                & "       UNNO                               , " _
                & "       HAZARDCLASS                        , " _
                & "       PACKINGGROUP                       , " _
                & "       FIRESERVICEACT                     , " _
                & "       PANDDCONTROLACT                    , " _
                & "       CASNO                              , " _
                & "       GRAVITY                            , " _
                & "       FLASHPOINT                         , " _
                & "       TANKGRADE                          , " _
                & "       PRPVISIONS                         , " _
                & "       ENABLED                            , " _
                & "       MANUFACTURE                        , " _
                & "       REMARK                             , " _
                & "       DELFLG                             , " _
                & "       UPDYMD                             , " _
                & "       UPDUSER                            , " _
                & "       UPDTERMID                            " _
                & "  FROM (" _
                & "SELECT " _
                & "       '' as APPLYID , " _
                & "       isnull(rtrim(COMPCODE),'')                 as COMPCODE , " _
                & "       isnull(rtrim(PRODUCTCODE),'')              as PRODUCTCODE , " _
                & "       isnull(convert(nvarchar, STYMD , 111),'')  as STYMD , " _
                & "       isnull(convert(nvarchar, ENDYMD , 111),'') as ENDYMD , " _
                & "       isnull(rtrim(PRODUCTNAME),'')              as PRODUCTNAME , " _
                & "       isnull(rtrim(CHEMICALNAME),'')             as CHEMICALNAME , " _
                & "       isnull(rtrim(IMDGCODE),'')                 as IMDGCODE , " _
                & "       isnull(rtrim(UNNO),'')                     as UNNO , " _
                & "       isnull(rtrim(HAZARDCLASS),'')              as HAZARDCLASS , " _
                & "       isnull(rtrim(PACKINGGROUP),'')             as PACKINGGROUP , " _
                & "       isnull(rtrim(FIRESERVICEACT),'')           as FIRESERVICEACT , " _
                & "       isnull(rtrim(PANDDCONTROLACT),'')          as PANDDCONTROLACT , " _
                & "       isnull(rtrim(CASNO),'')                    as CASNO , " _
                & "       isnull(rtrim(GRAVITY),'')                  as GRAVITY , " _
                & "       isnull(rtrim(FLASHPOINT),'')               as FLASHPOINT , " _
                & "       isnull(rtrim(TANKGRADE),'')                as TANKGRADE , " _
                & "       isnull(rtrim(PRPVISIONS),'')               as PRPVISIONS , " _
                & "       isnull(rtrim(ENABLED),'')                  as ENABLED , " _
                & "       isnull(rtrim(MANUFACTURE),'')              as MANUFACTURE , " _
                & "       isnull(rtrim(REMARK),'')                   as REMARK , " _
                & "       isnull(rtrim(DELFLG),'')                   as DELFLG , " _
                & "       isnull(convert(nvarchar, UPDYMD , 120),'') as UPDYMD , " _
                & "       isnull(rtrim(UPDUSER),'')                  as UPDUSER , " _
                & "       isnull(rtrim(UPDTERMID),'')                as UPDTERMID , " _
                & "       TIMSTP = cast(UPDTIMSTP                    as bigint) " _
                & " FROM GBM0008_PRODUCT as tbl1 " _
                & " WHERE DELFLG    <> @P4 " _
                & " AND   STYMD     <= @P1 " _
                & " AND   ENDYMD    >= @P2 " _
                & " AND   NOT EXISTS( "
            '承認画面から遷移の場合
            If  Page.PreviousPage Is Nothing Then
                SQLStr &= " SELECT * FROM GBM0011_PRODUCTAPPLY as tbl2 " _
                    & " WHERE tbl2.APPLYID = @P3 "
            Else
                SQLStr &= " SELECT * FROM GBM0011_PRODUCTAPPLY as tbl2 " _
                    & " WHERE tbl1.COMPCODE = tbl2.COMPCODE " _
                    & " AND   tbl1.PRODUCTCODE = tbl2.PRODUCTCODE " _
                    & " AND   tbl1.STYMD = tbl2.STYMD " _
                    & " AND   tbl1.DELFLG <> @P4 " _
                    & " AND   tbl2.DELFLG <> @P4 "
            End If
            SQLStr &= " )" _
                & " UNION ALL " _
                & "SELECT " _
                & "       isnull(rtrim(APPLYID),'')                  as APPLYID , " _
                & "       isnull(rtrim(COMPCODE),'')                 as COMPCODE , " _
                & "       isnull(rtrim(PRODUCTCODE),'')              as PRODUCTCODE , " _
                & "       isnull(convert(nvarchar, STYMD , 111),'')  as STYMD , " _
                & "       isnull(convert(nvarchar, ENDYMD , 111),'') as ENDYMD , " _
                & "       isnull(rtrim(PRODUCTNAME),'')              as PRODUCTNAME , " _
                & "       isnull(rtrim(CHEMICALNAME),'')             as CHEMICALNAME , " _
                & "       isnull(rtrim(IMDGCODE),'')                 as IMDGCODE , " _
                & "       isnull(rtrim(UNNO),'')                     as UNNO , " _
                & "       isnull(rtrim(HAZARDCLASS),'')              as HAZARDCLASS , " _
                & "       isnull(rtrim(PACKINGGROUP),'')             as PACKINGGROUP , " _
                & "       isnull(rtrim(FIRESERVICEACT),'')           as FIRESERVICEACT , " _
                & "       isnull(rtrim(PANDDCONTROLACT),'')          as PANDDCONTROLACT , " _
                & "       isnull(rtrim(CASNO),'')                    as CASNO , " _
                & "       isnull(rtrim(GRAVITY),'')                  as GRAVITY , " _
                & "       isnull(rtrim(FLASHPOINT),'')               as FLASHPOINT , " _
                & "       isnull(rtrim(TANKGRADE),'')                as TANKGRADE , " _
                & "       isnull(rtrim(PRPVISIONS),'')               as PRPVISIONS , " _
                & "       isnull(rtrim(ENABLED),'')                  as ENABLED , " _
                & "       isnull(rtrim(MANUFACTURE),'')              as MANUFACTURE , " _
                & "       isnull(rtrim(REMARK),'')                   as REMARK , " _
                & "       isnull(rtrim(DELFLG),'')                   as DELFLG , " _
                & "       isnull(convert(nvarchar, UPDYMD , 120),'') as UPDYMD , " _
                & "       isnull(rtrim(UPDUSER),'')                  as UPDUSER , " _
                & "       isnull(rtrim(UPDTERMID),'')                as UPDTERMID , " _
                & "       TIMSTP = cast(UPDTIMSTP                    as bigint) " _
                & " FROM GBM0011_PRODUCTAPPLY "
            '承認画面から遷移の場合
            If Page.PreviousPage Is Nothing Then
                SQLStr &= " WHERE APPLYID    = @P3 " _
                & " ) as tbl " _
                & " WHERE APPLYID    = @P3 "
            Else
                SQLStr &= " WHERE DELFLG    <> @P4 " _
                & " AND   STYMD     <= @P1 " _
                & " AND   ENDYMD    >= @P2 " _
                & " ) as tbl " _
                & " WHERE DELFLG    <> @P4 " _
                & " AND   STYMD     <= @P1 " _
                & " AND   ENDYMD    >= @P2 "
            End If

            ' 条件指定で指定されたものでＳＱＬで可能なものを追加する

            If Page.PreviousPage Is Nothing Then
            Else

                '積載品コード
                If (String.IsNullOrEmpty(Me.hdnSelectedProductCode.Value) = False) Then
                    SQLStr &= String.Format(" AND PRODUCTCODE = '{0}' ", Me.hdnSelectedProductCode.Value)
                End If

                '国連番号コード
                If (String.IsNullOrEmpty(Me.hdnUnNo.Value) = False) Then
                    SQLStr &= String.Format(" AND UNNO = '{0}' ", Me.hdnUnNo.Value)
                End If

                '有効フラグ
                If (String.IsNullOrEmpty(Me.hdnEnabled.Value) = False) Then
                    SQLStr &= String.Format(" AND ENABLED = '{0}' ", Me.hdnEnabled.Value)
                End If
            End If

            SQLStr &= " ORDER BY " & COA0020ProfViewSort.SORTSTR

            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
            PARA1.Value = Me.hdnSelectedEndYMD.Value
            PARA2.Value = Me.hdnSelectedStYMD.Value
            If (String.IsNullOrEmpty(Me.hdnSelectedApplyID.Value) = False) Then
                PARA3.Value = Me.hdnSelectedApplyID.value
            Else
                PARA3.Value = ""
            End If
            PARA4.Value = BaseDllCommon.CONST_FLAG_YES
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

        'INPtblカラム設定
        BASEtblColumnsAdd(INPtbl)

        'Excelデータ毎にチェック＆更新
        For i As Integer = 0 To COA0029XlsTable.TBLDATA.Rows.Count - 1

            'XLSTBL明細⇒INProw
            INProwWork = INPtbl.NewRow

            INProwWork("LINECNT") = 0
            INProwWork("OPERATION") = ""
            INProwWork("TIMSTP") = "0"
            INProwWork("SELECT") = 1
            INProwWork("HIDDEN") = 0
            INProwWork("COMPCODE") = hdnCompCode.Value

            Dim findInt As Integer = 99999
            Dim stYMDStr As String = ""

            For j As Integer = 6 To INPtbl.Columns.Count - 1

                ' カラム名設定
                Dim workColumn = INPtbl.Columns.Item(j).ColumnName
                '　カラム未定義、値が未設定の場合は空文字を設定
                If workColumn = "COMPCODE" Then
                    INProwWork(workColumn) = Me.hdnCompCode.Value
                ElseIf Not (COA0029XlsTable.TBLDATA.Columns.Contains(workColumn)) OrElse
                   IsDBNull(COA0029XlsTable.TBLDATA.Rows(i)(workColumn)) Then
                    INProwWork(workColumn) = ""
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

                End If
            Next
            INPtbl.Rows.Add(INProwWork)
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
            If warningFlg Then
                CommonFunctions.ShowMessage(C_MESSAGENO.UNMATCHMASTERUSE, Me.lblFooterMessage, naeiw:=C_NAEIW.WARNING, pageObject:=Me)
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            End If
        Else
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
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
        txtProductCodeEx.Focus()

        warningFlg = False

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
                'Case Me.vLeftCompCode.ID
                '    SetCompCodeListItem(Me.txtCompCode.Text)
                ''国コードビュー表示切替
                'Case Me.vLeftCountry.ID
                '    SetCountryListItem(Me.txtCountry.Text)
                ''顧客コードビュー表示切替
                'Case Me.vLeftShipper.ID
                '    SetShipperListItem(Me.txtShipper.Text)
                '国連番号コードビュー表示切替
                Case Me.vLeftUNNO.ID
                    SetUNNOListItem()
                '削除フラグビュー表示切替
                Case Me.vLeftDelFlg.ID
                    SetDelFlgListItem()
                '有効フラグビュー表示切替
                Case Me.vLeftEnabled.ID
                    SetEnabledListItem()
                '等級フラグビュー表示切替
                Case Me.vLeftHazardClass.ID
                    SetHazardClassListItem()
                '容器等級フラグビュー表示切替
                Case Me.vLeftPackingGroup.ID
                    SetPackingGroupListItem()
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
        Dim opeWarning As String = Nothing

        '比較文字設定
        If (COA0019Session.LANGDISP = C_LANG.JA) Then
            opeAll = "全て"
            opeUpdate = "更新"
            opeErr = "エラー"
            opeUpdErr = "更新エラー"
            blank = "空白"
            opeWarning = "警告"
        Else
            opeAll = "ALL"
            opeUpdate = "UPDATE"
            opeErr = "ERROR"
            opeUpdErr = "UPDATEERROR"
            blank = "BLANK"
            opeWarning = "WARNING"
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
                ElseIf txtOperationEx.Text = opeWarning Then
                    '警告を表示
                    If (searchStr <> opeWarning) Then
                        BASEtbl.Rows(i)("HIDDEN") = 1
                    End If
                Else
                    'その他非表示
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If
            End If

            '積載品コード 絞込判定
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtProductCodeEx.Text <> "") Then
                Dim searchStr As String = Convert.ToString(BASEtbl.Rows(i)("PRODUCTCODE")).ToUpper
                '検索用文字列（前方一致）
                If Not searchStr.StartsWith(txtProductCodeEx.Text.ToUpper) Then
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If
            End If

            '積載品名称 絞込判定
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtProductNameEx.Text <> "") Then
                Dim searchStr As String = ""
                '検索用文字列（部分一致）
                searchStr = Convert.ToString(BASEtbl.Rows(i)("PRODUCTNAME")).ToUpper

                If Not searchStr.Contains(txtProductNameEx.Text.ToUpper) Then
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If
            End If

            'CAS No.
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtProductCodeEx.Text <> "") Then
                Dim searchStr As String = Convert.ToString(BASEtbl.Rows(i)("PRODUCTCODE")).ToUpper
                '検索用文字列（前方一致）
                If Not searchStr.StartsWith(txtProductCodeEx.Text.ToUpper) Then
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If
            End If

            'CAS No.
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtCasNoEx.Text <> "") Then
                Dim searchStr As String = Convert.ToString(BASEtbl.Rows(i)("CASNO")).ToUpper
                '検索用文字列（前方一致）
                If Not searchStr.StartsWith(txtCasNoEx.Text.ToUpper) Then
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
        txtProductCodeEx.Focus()
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
                If (Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = updateDisp OrElse Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = warningDisp) AndAlso Convert.ToString(BASEtbl.Rows(i)("TIMSTP")) <> "0" Then
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
                             & "   and PRODUCTCODE = @P02 " _
                             & "   and STYMD = @P03 " _
                             & "   and DELFLG <> @P04 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

                        PARA01.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARA02.Value = BASEtbl.Rows(i)("PRODUCTCODE")
                        PARA03.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARA04.Value = BaseDllCommon.CONST_FLAG_YES

                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("PRODUCTCODE")) = Convert.ToString(BASEtbl.Rows(i)("PRODUCTCODE")) AndAlso
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
                             & "   and PRODUCTCODE = @P02 " _
                             & "   and STYMD = @P03 " _
                             & "   and DELFLG <> @P04 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARAM1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARAM2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARAM3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                        Dim PARAM4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

                        PARAM1.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARAM2.Value = BASEtbl.Rows(i)("PRODUCTCODE")
                        PARAM3.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARAM4.Value = BaseDllCommon.CONST_FLAG_YES

                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("PRODUCTCODE")) = Convert.ToString(BASEtbl.Rows(i)("PRODUCTCODE")) AndAlso
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

                    If (Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = updateDisp OrElse Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = "★" & updateDisp) OrElse
                        (Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = warningDisp OrElse Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = "★" & warningDisp) Then

                        '削除は更新しない
                        If Convert.ToString(BASEtbl.Rows(i)("DELFLG")) = BaseDllCommon.CONST_FLAG_YES AndAlso Convert.ToString(BASEtbl.Rows(i)("TIMSTP")) = "0" Then
                            BASEtbl.Rows(i)("OPERATION") = ""
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
                            BASEtbl.Rows(i)("APPLYID") = GBA00002MasterApplyID.APPLYID
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

                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then

                            '申請登録
                            COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
                            COA0032Apploval.I_APPLYID = Convert.ToString(BASEtbl.Rows(i)("APPLYID"))
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
                                 & "    AND PRODUCTCODE = @P03  " _
                                 & "    AND STYMD = @P04 ;  " _
                                 & " OPEN timestamp ;  " _
                                 & " FETCH NEXT FROM timestamp INTO @timestamp ;  " _
                                 & " IF ( @@FETCH_STATUS = 0 ) " _
                                 & "  UPDATE " & updTable _
                                 & "  SET "
                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID = @P01 , "
                        End If
                        SQLStr = SQLStr & " ENDYMD = @P05 , " _
                                 & "        PRODUCTNAME = @P06 , " _
                                 & "        CHEMICALNAME = @P07 , " _
                                 & "        IMDGCODE = @P08 , " _
                                 & "        UNNO = @P09 , " _
                                 & "        HAZARDCLASS = @P10 , " _
                                 & "        PACKINGGROUP = @P11 , " _
                                 & "        FIRESERVICEACT = @P12 , " _
                                 & "        PANDDCONTROLACT = @P13 , " _
                                 & "        CASNO = @P14 , " _
                                 & "        GRAVITY = @P15 , " _
                                 & "        FLASHPOINT = @P16 , " _
                                 & "        TANKGRADE = @P17 , " _
                                 & "        PRPVISIONS = @P18 , " _
                                 & "        ENABLED = @P19 , " _
                                 & "        MANUFACTURE = @P27 , " _
                                 & "        REMARK = @P20 , " _
                                 & "        DELFLG = @P21 , " _
                                 & "        INITYMD = @P22 , " _
                                 & "        UPDYMD = @P23 , " _
                                 & "        UPDUSER = @P24 , " _
                                 & "        UPDTERMID = @P25 , " _
                                 & "        RECEIVEYMD = @P26  " _
                                 & "  WHERE COMPCODE = @P02 " _
                                 & "    AND PRODUCTCODE = @P03 " _
                                 & "    AND STYMD = @P04 ; " _
                                 & " IF ( @@FETCH_STATUS <> 0 ) " _
                                 & "  INSERT INTO " & updTable _
                                 & "       ("
                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID , "
                        End If
                        SQLStr = SQLStr & " COMPCODE , " _
                                 & "        PRODUCTCODE , " _
                                 & "        STYMD , " _
                                 & "        ENDYMD , " _
                                 & "        PRODUCTNAME , " _
                                 & "        CHEMICALNAME , " _
                                 & "        IMDGCODE , " _
                                 & "        UNNO , " _
                                 & "        HAZARDCLASS , " _
                                 & "        PACKINGGROUP , " _
                                 & "        FIRESERVICEACT , " _
                                 & "        PANDDCONTROLACT , " _
                                 & "        CASNO , " _
                                 & "        GRAVITY , " _
                                 & "        FLASHPOINT , " _
                                 & "        TANKGRADE , " _
                                 & "        PRPVISIONS , " _
                                 & "        ENABLED , " _
                                 & "        MANUFACTURE , " _
                                 & "        REMARK , " _
                                 & "        DELFLG , " _
                                 & "        INITYMD , " _
                                 & "        UPDYMD , " _
                                 & "        UPDUSER , " _
                                 & "        UPDTERMID , " _
                                 & "        RECEIVEYMD ) " _
                                 & "  VALUES ( "
                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " @P01, "
                        End If
                        SQLStr = SQLStr & "          @P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10, " _
                                  & "           @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P27,@P20, " _
                                  & "           @P21,@P22,@P23,@P24,@P25,@P26); " _
                                  & " CLOSE timestamp ; " _
                                  & " DEALLOCATE timestamp ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.NVarChar)
                        Dim PARA07 As SqlParameter = SQLcmd.Parameters.Add("@P07", System.Data.SqlDbType.NVarChar)
                        Dim PARA08 As SqlParameter = SQLcmd.Parameters.Add("@P08", System.Data.SqlDbType.NVarChar)
                        Dim PARA09 As SqlParameter = SQLcmd.Parameters.Add("@P09", System.Data.SqlDbType.NVarChar)
                        Dim PARA10 As SqlParameter = SQLcmd.Parameters.Add("@P10", System.Data.SqlDbType.NVarChar)
                        Dim PARA11 As SqlParameter = SQLcmd.Parameters.Add("@P11", System.Data.SqlDbType.NVarChar)
                        Dim PARA12 As SqlParameter = SQLcmd.Parameters.Add("@P12", System.Data.SqlDbType.NVarChar)
                        Dim PARA13 As SqlParameter = SQLcmd.Parameters.Add("@P13", System.Data.SqlDbType.NVarChar)
                        Dim PARA14 As SqlParameter = SQLcmd.Parameters.Add("@P14", System.Data.SqlDbType.NVarChar)
                        Dim PARA15 As SqlParameter = SQLcmd.Parameters.Add("@P15", System.Data.SqlDbType.NVarChar)
                        Dim PARA16 As SqlParameter = SQLcmd.Parameters.Add("@P16", System.Data.SqlDbType.NVarChar)
                        Dim PARA17 As SqlParameter = SQLcmd.Parameters.Add("@P17", System.Data.SqlDbType.NVarChar)
                        Dim PARA18 As SqlParameter = SQLcmd.Parameters.Add("@P18", System.Data.SqlDbType.NVarChar)
                        Dim PARA19 As SqlParameter = SQLcmd.Parameters.Add("@P19", System.Data.SqlDbType.NVarChar)
                        Dim PARA20 As SqlParameter = SQLcmd.Parameters.Add("@P20", System.Data.SqlDbType.NVarChar)
                        Dim PARA21 As SqlParameter = SQLcmd.Parameters.Add("@P21", System.Data.SqlDbType.NVarChar)
                        Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.DateTime)
                        Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.DateTime)
                        Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar)
                        Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.NVarChar)
                        Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.DateTime)
                        Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar)

                        PARA01.Value = BASEtbl.Rows(i)("APPLYID")
                        PARA02.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARA03.Value = BASEtbl.Rows(i)("PRODUCTCODE")
                        PARA04.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARA05.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("ENDYMD")))
                        PARA06.Value = BASEtbl.Rows(i)("PRODUCTNAME")
                        PARA07.Value = BASEtbl.Rows(i)("CHEMICALNAME")
                        PARA08.Value = BASEtbl.Rows(i)("IMDGCODE")
                        PARA09.Value = BASEtbl.Rows(i)("UNNO")
                        PARA10.Value = BASEtbl.Rows(i)("HAZARDCLASS")
                        PARA11.Value = BASEtbl.Rows(i)("PACKINGGROUP")
                        PARA12.Value = BASEtbl.Rows(i)("FIRESERVICEACT")
                        PARA13.Value = BASEtbl.Rows(i)("PANDDCONTROLACT")
                        PARA14.Value = BASEtbl.Rows(i)("CASNO")
                        PARA15.Value = BASEtbl.Rows(i)("GRAVITY")
                        PARA16.Value = BASEtbl.Rows(i)("FLASHPOINT")
                        PARA17.Value = BASEtbl.Rows(i)("TANKGRADE")
                        PARA18.Value = BASEtbl.Rows(i)("PRPVISIONS")
                        PARA19.Value = BASEtbl.Rows(i)("ENABLED")
                        PARA27.Value = BASEtbl.Rows(i)("MANUFACTURE")
                        PARA20.Value = BASEtbl.Rows(i)("REMARK")
                        PARA21.Value = BASEtbl.Rows(i)("DELFLG")
                        PARA22.Value = nowDate
                        PARA23.Value = nowDate
                        PARA24.Value = COA0019Session.USERID
                        PARA25.Value = HttpContext.Current.Session("APSRVname")
                        PARA26.Value = CONST_DEFAULT_RECEIVEYMD

                        SQLcmd.ExecuteNonQuery()

                        '結果 --> テーブル反映
                        BASEtbl.Rows(i)("UPDYMD") = nowDate.ToString("yyyy-MM-dd HH:mm:ss")
                        BASEtbl.Rows(i)("OPERATION") = ""


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
                                & "   And PRODUCTCODE = @P02 " _
                                & "   And STYMD = @P03 ;"

                        SQLcmd2 = New SqlCommand(SQLStr2, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd2.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA2 As SqlParameter = SQLcmd2.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA3 As SqlParameter = SQLcmd2.Parameters.Add("@P03", System.Data.SqlDbType.Date)

                        PARA1.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARA2.Value = BASEtbl.Rows(i)("PRODUCTCODE")
                        PARA3.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))

                        SQLdr2 = SQLcmd2.ExecuteReader()

                        While SQLdr2.Read
                            BASEtbl.Rows(i)("UPDYMD") = SQLdr2("UPDYMD")
                            BASEtbl.Rows(i)("UPDUSER") = SQLdr2("UPDUSER")
                            BASEtbl.Rows(i)("UPDTERMID") = SQLdr2("UPDTERMID")
                            BASEtbl.Rows(i)("TIMSTP") = SQLdr2("TIMSTP")
                        End While

                        'PDF更新処理
                        PDFDBupdate(Convert.ToString(BASEtbl.Rows(i)("COMPCODE")), Convert.ToString(BASEtbl.Rows(i)("PRODUCTCODE")), Convert.ToString(BASEtbl.Rows(i)("APPLYID")))

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
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL                                   '
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
        txtProductCodeEx.Focus()
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
        COA0027ReportTable.TBLDATA = BASEtbl                               'PARAM04:データ参照tabledata
        COA0027ReportTable.COA0027ReportTable()

        If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
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
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
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
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
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
        COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value

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
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
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
                If warningFlg Then
                    CommonFunctions.ShowMessage(C_MESSAGENO.UNMATCHMASTERUSE, Me.lblFooterMessage, naeiw:=C_NAEIW.WARNING, pageObject:=Me)
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.NORMALLISTADDED, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                End If
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
        txtProduct.Focus()

        warningFlg = False

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
        txtProduct.Focus()

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
        Dim dvTBLview As DataView
        dvTBLview = New DataView(BASEtbl)
        dvTBLview.RowFilter = "HIDDEN= '0'"

        'ポジションを設定するのみ
        If dvTBLview.Count Mod CONST_SCROLLROWCOUNT = 0 Then
            hdnListPosition.Value = Convert.ToString(dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT))
        Else
            hdnListPosition.Value = Convert.ToString(dvTBLview.Count - (dvTBLview.Count Mod CONST_SCROLLROWCOUNT) + 1)
        End If

        dvTBLview.Dispose()
        dvTBLview = Nothing

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
        COA0013TableObject.VARI = hdnViewId.Value
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
        ' メッセージ取得
        Dim dummyMsgBox As Label = New Label
        Dim errMessageStr As String = ""

        'インターフェイス初期値設定
        returnCode = C_MESSAGENO.NORMAL

        '事前準備（キー重複レコード削除）
        For i As Integer = INPtbl.Rows.Count - 1 To 1 Step -1
            'KEY重複
            If Convert.ToString(INPtbl.Rows(i)("COMPCODE")) = Convert.ToString(INPtbl.Rows(i - 1)("COMPCODE")) AndAlso
               Convert.ToString(INPtbl.Rows(i)("PRODUCTCODE")) = Convert.ToString(INPtbl.Rows(i - 1)("PRODUCTCODE")) AndAlso
               Convert.ToString(INPtbl.Rows(i)("STYMD")) = Convert.ToString(INPtbl.Rows(i - 1)("STYMD")) Then
                INPtbl.Rows(i).Delete()
            End If
        Next

        'チェック ＆　更新用テーブル作成
        'ポジション　＆　行レコード取得

        'タイトル区分存在チェック(Iレコード)　…　Iレコードが無ければエラー
        'ヘッダーのみも存在するのでチェックしない

        'チェック実行　-->　OK時 UPDtbl作成　…　パラメータ数を担保する必要あり(Defaultを参照)
        For i As Integer = 0 To INPtbl.Rows.Count - 1

            Dim workInpRow As DataRow
            workInpRow = INPtbl.NewRow
            workInpRow.ItemArray = INPtbl.Rows(i).ItemArray

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
                For j As Integer = 0 To BASEtbl.Rows.Count - 1
                    If Convert.ToString(BASEtbl.Rows(j)("DELFLG")) <> BaseDllCommon.CONST_FLAG_YES Then
                        '日付以外の項目が等しい
                        If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(workInpRow("COMPCODE")) AndAlso
                           Convert.ToString(BASEtbl.Rows(j)("PRODUCTCODE")) = Convert.ToString(workInpRow("PRODUCTCODE")) Then

                            'ENDYMDは変更扱い
                            If Convert.ToString(BASEtbl.Rows(j)("STYMD")) = Convert.ToString(workInpRow("STYMD")) Then

                                '同一レコード
                                Exit For
                            Else

                                Dim baseDateStart As Date
                                Dim baseDateEnd As Date
                                Date.TryParse(Convert.ToString(BASEtbl.Rows(j)("STYMD")), baseDateStart)
                                Date.TryParse(Convert.ToString(BASEtbl.Rows(j)("ENDYMD")), baseDateEnd)

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
                If Convert.ToString(workInpRow("OPERATION")) <> warningDisp Then
                    workInpRow("OPERATION") = errDisp
                    errListAll.Add(C_MESSAGENO.RIGHTBIXOUT)
                    errList.Add(C_MESSAGENO.RIGHTBIXOUT)
                    If returnCode = C_MESSAGENO.REQUIREDVALUE OrElse returnCode = C_MESSAGENO.HASAPPLYINGRECORD Then ' 一覧反映対象外
                        workInpRow("HIDDEN") = "1"
                        returnCode = C_MESSAGENO.RIGHTBIXOUT
                    End If
                End If
            End If
            INPtbl.Rows(i).ItemArray = workInpRow.ItemArray
        Next

        Dim refErrMessage As String = Nothing
        If WF_DViewRepPDF.Items.Count > 0 Then
            Dim retCode As String = returnCode
            For j As Integer = 0 To WF_DViewRepPDF.Items.Count - 1

                Dim dltFlg As String = DirectCast(WF_DViewRepPDF.Items(j).FindControl("WF_Rep_DELFLG"), TextBox).Text
                Dim fileNm As String = DirectCast(WF_DViewRepPDF.Items(j).FindControl("WF_Rep_FILENAME"), Label).Text

                '削除フラグ
                SetDelFlgListItem()
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

            If retCode <> C_MESSAGENO.NORMAL Then
                returnCode = retCode
            End If
        End If
    End Sub
    ''' <summary>
    ''' 警告チェック
    ''' </summary>
    Private Function WarCheck(ByVal workInpRow As DataRow) As Boolean
        Dim dummyMsgBox As Label = New Label
        Dim errMessageStr As String = Nothing

        Dim EnabledFlg As String = ""
        Dim unnoVal As String = Convert.ToString(workInpRow("UNNO"))
        Dim splUnnoVal As String() = Split(unnoVal, ",", -1, CompareMethod.Text)
        Dim hazclsVal As String = Convert.ToString(workInpRow("HAZARDCLASS"))
        Dim splHazclsVal As String() = Split(hazclsVal, ",", -1, CompareMethod.Text)
        Dim pacgrpVal As String = Convert.ToString(workInpRow("PACKINGGROUP"))
        Dim splPacgrpVal As String() = Split(pacgrpVal, ",", -1, CompareMethod.Text)

        For i As Integer = 0 To splUnnoVal.Count - 1
            For j As Integer = 0 To splHazclsVal.Count - 1
                For k As Integer = 0 To splPacgrpVal.Count - 1

                    GetEnabled(splUnnoVal(i), splHazclsVal(j), splPacgrpVal(k), EnabledFlg)

                    If EnabledFlg = CONST_FLAG_NO Then
                        Exit For
                    End If
                Next
                If EnabledFlg = CONST_FLAG_NO Then
                    Exit For
                End If
            Next
            If EnabledFlg = CONST_FLAG_NO Then
                Exit For
            End If
        Next

        Dim errmsg As String = Nothing
        If returnCode = C_MESSAGENO.NORMAL Then
            If EnabledFlg <> "" AndAlso EnabledFlg <> Convert.ToString(workInpRow("ENABLED")) Then
                CommonFunctions.ShowMessage(C_MESSAGENO.UNMATCHMASTERUSE, dummyMsgBox)
                errmsg = dummyMsgBox.Text
                'エラーレポート編集
                errMessageStr = ""
                errMessageStr = "・" & errmsg
                ' レコード内容を展開する
                errMessageStr = errMessageStr & Me.ErrItemSet(workInpRow)
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr

                Return True

            Else
                Return False
            End If
        Else
            Return False
        End If

    End Function
    ''' <summary>
    ''' エラーキー情報出力
    ''' </summary>
    ''' <param name="argRow"></param>
    ''' <returns></returns>
    Private Function ErrItemSet(ByVal argRow As DataRow) As String
        Dim rtc As String = String.Empty

        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
            rtc &= ControlChars.NewLine & "  --> COMPANY CODE    =" & Convert.ToString(argRow("COMPCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> PRODUCT CODE    =" & Convert.ToString(argRow("PRODUCTCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> EFFECTIVE(FROM) =" & Convert.ToString(argRow("STYMD")) & " , "
            rtc &= ControlChars.NewLine & "  --> DELETE FLG      =" & Convert.ToString(argRow("DELFLG")) & " "
        Else
            rtc &= ControlChars.NewLine & "  --> 会社コード      =" & Convert.ToString(argRow("COMPCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> 積載品コード    =" & Convert.ToString(argRow("PRODUCTCODE")) & " , "
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
                Case Me.vLeftUNNO.ID 'アクティブなビューが国連番号コード
                    '国連番号コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター国連番号
                        If Me.lbUNNO.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then

                            Dim parts As String()
                            Dim unnoTxt As String = ""

                            'If Me.lbUNNO.SelectedItem.Text = NONDG Then

                            '    DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                            '    TextBox).Text = Me.lbUNNO.SelectedItem.Value
                            '    DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                            '        Label).Text = Me.lbUNNO.SelectedItem.Text
                            '    WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()

                            'Else

                            parts = Split(Me.lbUNNO.SelectedItem.Text, ",", -1, CompareMethod.Text)
                            Dim UnNoKeyValue As Dictionary(Of String, String) = DirectCast(ViewState("UNNOKEYVALUE"), Dictionary(Of String, String))
                            unnoTxt = UnNoKeyValue(Me.lbUNNO.SelectedItem.Text)

                            Dim unno As String = ""
                            Dim hzclass As String = ""
                            Dim pcgroup As String = ""

                            unno = parts(0)
                            hzclass = parts(1)
                            pcgroup = parts(2)

                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                    TextBox).Text = unno
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                    Label).Text = unnoTxt
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()

                            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                                Select Case DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text

                                    Case "HAZARDCLASS"
                                        '等級
                                        DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = hzclass
                                        HAZARDCLASS_Change()

                                    Case "PACKINGGROUP"
                                        '容器等級
                                        DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = pcgroup
                                        PACKINGGROUP_Change()
                                End Select
                            Next

                            '警告設定
                            JudUNNO(unno, hzclass, pcgroup)

                            'End If

                        End If
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
                                          TextBox).Text = Me.lbDelFlg.SelectedItem.Value
                                WF_DViewRepPDF.Items(Integer.Parse(hdnTextDbClickField.Value)).FindControl("WF_Rep_DELFLG").Focus()
                            End If
                        End If
                    End If
                Case Me.vLeftEnabled.ID 'アクティブなビューが有効フラグ
                    '有効フラグ選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター有効フラグ
                        If Me.lbEnabled.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                TextBox).Text = Me.lbEnabled.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                Label).Text = Me.lbEnabled.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftHazardClass.ID 'アクティブなビューが等級
                    '等級選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター等級
                        If Me.lbHazardClass.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                TextBox).Text = Me.lbHazardClass.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                Label).Text = Me.lbHazardClass.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                        HAZARDCLASS_Change()
                    End If
                Case Me.vLeftPackingGroup.ID 'アクティブなビューが容器等級
                    '容器等級選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター容器等級
                        If Me.lbPackingGroup.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                TextBox).Text = Me.lbPackingGroup.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                Label).Text = Me.lbPackingGroup.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                        PACKINGGROUP_Change()
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
        AddLangSetting(dicDisplayText, Me.lblProductCodeEx, "積載品コード", "Product Code")
        AddLangSetting(dicDisplayText, Me.lblProductNameEx, "積載品名称", "Product Name")
        AddLangSetting(dicDisplayText, Me.lblCasNoEx, "CAS No.", "CAS No.")

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
        'AddLangSetting(dicDisplayText, Me.lblCompCode, "会社コード", "Company Code")
        'AddLangSetting(dicDisplayText, Me.lblCountry, "国コード", "Country Code")
        'AddLangSetting(dicDisplayText, Me.lblShipper, "顧客コード", "Shipper Code")
        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品コード", "Product Code")
        'AddLangSetting(dicDisplayText, Me.lblUNNO, "国連番号", "UNNO")
        AddLangSetting(dicDisplayText, Me.lblYMD, "有効年月日", "Effective Date")
        AddLangSetting(dicDisplayText, Me.lblDelFlg, "削除", "Delete")

        AddLangSetting(dicDisplayText, Me.WF_Rep2_DispSelect, "表示選択", "Disp Select")
        AddLangSetting(dicDisplayText, Me.WF_Rep2_Desc, "添付書類を登録する場合は、ここにドロップすること", "To register attached documents, drop it here")
        AddLangSetting(dicDisplayText, Me.WF_Rep2_PDFfileName, "ファイル名", "File Name")
        AddLangSetting(dicDisplayText, Me.WF_Rep2_Delete, "削 除", "Delete")

        AddLangSetting(dicDisplayText, Me.hdnUploadMessage01, "ファイルアップロード開始", "Start uploading files")
        AddLangSetting(dicDisplayText, Me.hdnUploadError01, "ファイルアップロードが失敗しました。", "File upload failed.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError02, "通信を中止しました。", "Communication was canceled.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError03, "タイムアウトエラーが発生しました。", "A timeout error occurred.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError04, "更新権限がありません。", "You do not have update permission.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError05, "対応外のファイル形式です。", "It is an incompatible file format.")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)

        '****************************************
        ' グリッドヘッダーの表示文言設定(GrivViewだけは個別制御が必要)
        '****************************************
        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
            Me.lblDtabProduct.Text = "Product Info"
            Me.lblDtabDocument.Text = "Documents(PDF)"
        Else
            Me.lblDtabProduct.Text = "積載品情報"
            Me.lblDtabDocument.Text = "書類（PDF）"
        End If

    End Sub
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
        table.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        table.Columns("SELECT").DefaultValue = "0"
        table.Columns.Add("HIDDEN", GetType(Integer))             'DBの固定フィールド
        table.Columns("HIDDEN").DefaultValue = "0"
        table.Columns.Add("COMPCODE", GetType(String))
        table.Columns("COMPCODE").DefaultValue = hdnCompCode.Value

        '画面固有項目
        table.Columns.Add("APPLYID", GetType(String))
        'table.Columns.Add("COUNTRYCODE", GetType(String))
        'table.Columns.Add("CUSTOMERCODE", GetType(String))
        table.Columns.Add("PRODUCTCODE", GetType(String))
        table.Columns.Add("STYMD", GetType(String))
        table.Columns.Add("ENDYMD", GetType(String))
        table.Columns.Add("PRODUCTNAME", GetType(String))
        table.Columns.Add("CHEMICALNAME", GetType(String))
        table.Columns.Add("IMDGCODE", GetType(String))
        table.Columns.Add("UNNO", GetType(String))
        table.Columns.Add("HAZARDCLASS", GetType(String))
        table.Columns.Add("PACKINGGROUP", GetType(String))
        table.Columns.Add("FIRESERVICEACT", GetType(String))
        table.Columns.Add("PANDDCONTROLACT", GetType(String))
        table.Columns.Add("CASNO", GetType(String))
        table.Columns.Add("GRAVITY", GetType(String))
        table.Columns.Add("FLASHPOINT", GetType(String))
        table.Columns.Add("TANKGRADE", GetType(String))
        table.Columns.Add("PRPVISIONS", GetType(String))
        table.Columns.Add("ENABLED", GetType(String))
        table.Columns.Add("MANUFACTURE", GetType(String))
        table.Columns.Add("REMARK", GetType(String))
        table.Columns.Add("DELFLG", GetType(String))
        table.Columns.Add("UPDYMD", GetType(String))
        table.Columns.Add("UPDUSER", GetType(String))
        table.Columns.Add("UPDTERMID", GetType(String))

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
        workRow("SELECT") = "0"                                     'DBの固定フィールド
        workRow("HIDDEN") = "0"                                     'DBの固定フィールド

        workRow("APPLYID") = ""
        workRow("COMPCODE") = ""
        workRow("PRODUCTCODE") = ""
        workRow("STYMD") = ""
        workRow("ENDYMD") = ""
        workRow("PRODUCTNAME") = ""
        workRow("CHEMICALNAME") = ""
        workRow("IMDGCODE") = ""
        workRow("UNNO") = ""
        workRow("HAZARDCLASS") = ""
        workRow("PACKINGGROUP") = ""
        workRow("FIRESERVICEACT") = ""
        workRow("PANDDCONTROLACT") = ""
        workRow("CASNO") = ""
        workRow("GRAVITY") = ""
        workRow("FLASHPOINT") = ""
        workRow("TANKGRADE") = ""
        workRow("PRPVISIONS") = ""
        workRow("ENABLED") = ""
        workRow("MANUFACTURE") = ""
        workRow("REMARK") = ""
        workRow("DELFLG") = ""
        workRow("UPDYMD") = ""

        argTbl.Rows.Add(workRow)

    End Sub
    ''' <summary>
    ''' detailbox 編集内容→INPtbl 
    ''' </summary>
    Protected Sub DetailBoxToINPtbl()
        Dim COA0014DetailView As New BASEDLL.COA0014DetailView
        Dim COA0015ProfViewD As New BASEDLL.COA0015ProfViewD        'UPROFview・Detail取得
        Dim workRow As DataRow

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
            If (String.IsNullOrEmpty(lblLineCntText.Text)) Then
                workRow("LINECNT") = 0
            Else
                workRow("LINECNT") = CType(lblLineCntText.Text, Integer)
            End If
            workRow("OPERATION") = ""
            workRow("TIMSTP") = "0"
            workRow("SELECT") = 1
            workRow("HIDDEN") = 0
            workRow("APPLYID") = lblApplyIDText.Text
            workRow("COMPCODE") = hdnCompCode.Value
            workRow("PRODUCTCODE") = txtProduct.Text
            workRow("STYMD") = FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT)
            workRow("ENDYMD") = FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT)
            workRow("PRODUCTNAME") = ""
            workRow("CHEMICALNAME") = ""
            workRow("IMDGCODE") = ""
            workRow("UNNO") = ""
            workRow("HAZARDCLASS") = ""
            workRow("PACKINGGROUP") = ""
            workRow("FIRESERVICEACT") = ""
            workRow("PANDDCONTROLACT") = ""
            workRow("CASNO") = ""
            workRow("GRAVITY") = ""
            workRow("FLASHPOINT") = ""
            workRow("TANKGRADE") = ""
            workRow("PRPVISIONS") = ""
            workRow("ENABLED") = ""
            workRow("MANUFACTURE") = ""
            workRow("REMARK") = ""
            workRow("DELFLG") = txtDelFlg.Text
            workRow("UPDYMD") = ""
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
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck
        Dim fieldList As List(Of String) = Nothing
        Dim dicField As Dictionary(Of String, String) = Nothing

        Dim dataTable As DataTable = New DataTable
        Dim dataRow As DataRow
        Dim repName1 As Label = Nothing
        Dim repName2 As Label = Nothing
        Dim repName3 As Label = Nothing

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

        If fieldList.Count > 0 Then
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), Label).Text) <> -1 Then
                    repName1 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_1"), Label)
                    If repName1.CssClass = "" Then
                        repName1.CssClass = "requiredMark2"
                    Else
                        repName1.CssClass = repName1.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text) <> -1 Then
                    repName2 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_2"), Label)
                    If repName2.CssClass = "" Then
                        repName2.CssClass = "requiredMark2"
                    Else
                        repName2.CssClass = repName2.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), Label).Text) <> -1 Then
                    repName3 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_3"), Label)
                    If repName3.CssClass = "" Then
                        repName3.CssClass = "requiredMark2"
                    Else
                        repName3.CssClass = repName3.CssClass & " " & "requiredMark2"
                    End If
                End If
            Next
        End If

        WF_DetailMView.ActiveViewIndex = 0

        lblDtabProduct.Style.Remove("color")
        lblDtabProduct.Style.Add("color", "blue")
        lblDtabProduct.Style.Remove("background-color")
        lblDtabProduct.Style.Add("background-color", "rgb(220,230,240)")
        lblDtabProduct.Style.Remove("border")
        lblDtabProduct.Style.Add("border", "1px solid blue")
        lblDtabProduct.Style.Remove("font-weight")
        lblDtabProduct.Style.Add("font-weight", "bold")

        'Detail設定処理
        SetDetailDbClick()

        '初期値設定
        SetInitValue()

        dataTable.Dispose()
        dataTable = Nothing

        'Enable名称設定
        ENABLED_Change()

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
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), Label).Text) <> -1 Then
                    repName1 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_1"), Label)
                    If repName1.CssClass = "" Then
                        repName1.CssClass = "requiredMark2"
                    Else
                        repName1.CssClass = repName1.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text) <> -1 Then
                    repName2 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_2"), Label)
                    If repName2.CssClass = "" Then
                        repName2.CssClass = "requiredMark2"
                    Else
                        repName2.CssClass = repName2.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), Label).Text) <> -1 Then
                    repName3 = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_3"), Label)
                    If repName3.CssClass = "" Then
                        repName3.CssClass = "requiredMark2"
                    Else
                        repName3.CssClass = repName3.CssClass & " " & "requiredMark2"
                    End If
                End If

            End If

            'ダブルクリック時コード検索イベント追加
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_1"), Label)
                repValue = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_1"), TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_1"), Label)
                    repName.Attributes.Remove("style")
                    repName.Attributes.Add("style", "text-decoration: underline;")
                End If
            End If

            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label)
                repValue = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_2"), Label)
                    repName.Attributes.Remove("style")
                    repName.Attributes.Add("style", "text-decoration: underline;")
                End If
            End If

            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), Label)
                repValue = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_3"), Label)
                    repName.Attributes.Remove("style")
                    repName.Attributes.Add("style", "text-decoration: underline;")
                End If
            End If

        Next

    End Sub
    ''' <summary>
    ''' Detail初期値設定処理
    ''' </summary>
    Protected Sub SetInitValue()

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck
        Dim fieldList As List(Of String) = Nothing
        Dim dicField As Dictionary(Of String, String) = Nothing
        Dim repField As Object = Nothing
        Dim repValue As Object = Nothing
        Dim repName As Object = Nothing
        Dim repAttr As String = ""

        For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

            'ENABLED設定
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "ENABLED" Then
                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = BaseDllCommon.CONST_FLAG_YES
            End If

            'FIRESERVICEACT設定
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), Label).Text = "FIRESERVICEACT" Then
                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), TextBox).Text = BaseDllCommon.CONST_FLAG_NO
            End If

            'GRAVITY設定
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), Label).Text = "GRAVITY" Then
                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), TextBox).Text = "0"
            End If

            'FLASHPOINT設定
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), Label).Text = "FLASHPOINT" Then
                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), TextBox).Text = "0"
            End If
        Next

        ENABLED_Change()

        Dim endDt As Date = Date.Parse("2099/12/31")
        Me.txtStYMD.Text = Date.Now.ToString(GBA00003UserSetting.DATEFORMAT)
        Me.txtEndYMD.Text = endDt.ToString(GBA00003UserSetting.DATEFORMAT)

        Me.txtDelFlg.Text = BaseDllCommon.CONST_FLAG_NO
        txtDelFlg_Change()

    End Sub
    ''' <summary>
    ''' ダブルクリック処理追加
    ''' </summary>
    ''' <param name="repField"></param>
    ''' <param name="repAttr"></param>
    Protected Sub GetAttributes(ByVal repField As String, ByRef repAttr As String)

        Select Case repField
            Case "ENABLED"
                '有効フラグ
                repAttr = "Field_DBclick('vLeftEnabled', '5');"
            Case "HAZARDCLASS"
                '等級
                'repAttr = "Field_DBclick('vLeftHazardClass', '0');"
            Case "PACKINGGROUP"
                '容器等級
                repAttr = "Field_DBclick('vLeftPackingGroup', '1');"
            Case "UNNO"
                '国連番号
                'repAttr = "Field_DBclick('vLeftUNNO', '2');"
            Case Else
                repAttr = ""

        End Select
    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    Public Sub ListUpdateCheck(ByVal InpRow As DataRow)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get
        Dim escapeFlg As Boolean = False
        Dim errFlg = False
        Dim errMessageStr As String = Nothing
        Dim refErrMessage As String = Nothing
        Dim dicField As Dictionary(Of String, String) = Nothing
        returnCode = C_MESSAGENO.NORMAL
        'Dim ndg As String = NONDG
        Dim dummy As Label = Nothing

        '入力項目チェック
        '①単項目チェック

        'カラム情報取得
        dicField = New Dictionary(Of String, String)
        CheckSingle(InpRow, dicField, escapeFlg)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '国連番号数値チェック
        If Convert.ToString(InpRow("UNNO")) <> "" Then
            If Not IsNumeric(Convert.ToString(InpRow("UNNO")).Replace(",", "")) Then

                dummy = New Label
                CommonFunctions.ShowMessage(C_MESSAGENO.INPUTERROR, dummy)

                errMessageStr = Me.ErrItemSet(InpRow)
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & dummy.Text & "(" & dicField("UNNO") & ":" & Convert.ToString(InpRow("UNNO")) & ")" & errMessageStr
                errFlg = True
            End If
        End If

        '等級数値チェック
        If Convert.ToString(InpRow("HAZARDCLASS")) <> "" Then
            If Not IsNumeric(Convert.ToString(InpRow("HAZARDCLASS")).Replace(",", "")) Then

                dummy = New Label
                CommonFunctions.ShowMessage(C_MESSAGENO.INPUTERROR, dummy)

                errMessageStr = Me.ErrItemSet(InpRow)
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & dummy.Text & "(" & dicField("HAZARDCLASS") & ":" & Convert.ToString(InpRow("HAZARDCLASS")) & ")" & errMessageStr
                errFlg = True
            End If
        End If

        'CAS No.数値チェック
        If Convert.ToString(InpRow("CASNO")) <> "" Then
            If Not IsNumeric(Convert.ToString(InpRow("CASNO")).Replace("-", "")) Then

                dummy = New Label
                CommonFunctions.ShowMessage(C_MESSAGENO.INPUTERROR, dummy)

                errMessageStr = Me.ErrItemSet(InpRow)
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & dummy.Text & "(" & dicField("CASNO") & ":" & Convert.ToString(InpRow("CASNO")) & ")" & errMessageStr
                errFlg = True
            End If
        End If

        '②存在チェック(LeftBoxチェック)
        ''会社コード
        'SetCompCodeListItem(Convert.ToString(InpRow("COMPCODE")))
        'ChedckList(Convert.ToString(InpRow("COMPCODE")), lbCompCode, refErrMessage)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    errMessageStr = Me.ErrItemSet(InpRow)
        '    If txtRightErrorMessage.Text <> "" Then
        '        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
        '    End If
        '    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
        '                                   & "・" & refErrMessage & "(" & dicField("COMPCODE") & ":" & Convert.ToString(InpRow("COMPCODE")) & ")" & errMessageStr
        '    errFlg = True
        '    returnCode = C_MESSAGENO.NORMAL
        'End If

        ''顧客コード
        'SetShipperListItem(InpRow("CUSTOMERCODE"))
        'ChedckList(InpRow("CUSTOMERCODE"), lbShipper, refErrMessage)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    errMessageStr = Me.ErrItemSet(InpRow)
        '    If txtRightErrorMessage.Text <> "" Then
        '        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
        '    End If
        '    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
        '                                   & "・" & refErrMessage & "(" & dicField("CUSTOMERCODE") & ":" & InpRow("CUSTOMERCODE") & ")" & errMessageStr
        '    errFlg = True
        '    returnCode = C_MESSAGENO.NORMAL
        'End If

        ''国コード
        'SetCountryListItem(InpRow("COUNTRYCODE"))
        'ChedckList(InpRow("COUNTRYCODE"), lbCountry, refErrMessage)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    errMessageStr = Me.ErrItemSet(InpRow)
        '    If txtRightErrorMessage.Text <> "" Then
        '        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
        '    End If
        '    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
        '                                   & "・" & refErrMessage & "(" & dicField("COUNTRYCODE") & ":" & InpRow("COUNTRYCODE") & ")" & errMessageStr
        '    errFlg = True
        '    returnCode = C_MESSAGENO.NORMAL
        'End If

        ''国連番号
        'SetUNNOListItem(InpRow("UNNO"))
        'ChedckList(InpRow("UNNO"), lbUNNO, refErrMessage)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    errMessageStr = Me.ErrItemSet(InpRow)
        '    If txtRightErrorMessage.Text <> "" Then
        '        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
        '    End If
        '    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
        '                                   & "・" & refErrMessage & "(" & dicField("UNNO") & ":" & InpRow("UNNO") & ")" & errMessageStr
        '    errFlg = True
        '    returnCode = C_MESSAGENO.NORMAL
        'End If

        '削除フラグ
        SetDelFlgListItem()
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

        '有効フラグ
        SetEnabledListItem()
        ChedckList(Convert.ToString(InpRow("ENABLED")), lbEnabled, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("ENABLED") & ":" & Convert.ToString(InpRow("ENABLED")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        ''等級
        'If Convert.ToString(InpRow("HAZARDCLASS")) <> ndg Then
        '    SetHazardClassListItem()
        '    ChedckList(Convert.ToString(InpRow("HAZARDCLASS")), lbHazardClass, refErrMessage)
        '    If returnCode <> C_MESSAGENO.NORMAL Then
        '        errMessageStr = Me.ErrItemSet(InpRow)
        '        If txtRightErrorMessage.Text <> "" Then
        '            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
        '        End If
        '        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
        '                                   & "・" & refErrMessage & "(" & dicField("HAZARDCLASS") & ":" & Convert.ToString(InpRow("HAZARDCLASS")) & ")" & errMessageStr
        '        errFlg = True
        '        returnCode = C_MESSAGENO.NORMAL
        '    End If
        'End If

        '容器等級
        'If Convert.ToString(InpRow("PACKINGGROUP")) <> ndg Then
        SetPackingGroupListItem()
        ChedckList(Convert.ToString(InpRow("PACKINGGROUP")), lbPackingGroup, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                               & "・" & refErrMessage & "(" & dicField("PACKINGGROUP") & ":" & Convert.ToString(InpRow("PACKINGGROUP")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If
        'End If

        '③個別チェック
        Dim pnCnt As Integer = 0
        Dim cnCnt As Integer = 0

        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            '製品名同一チェック
            If Convert.ToString(InpRow("PRODUCTNAME")) <> "" Then
                If Convert.ToString(InpRow("PRODUCTNAME")).ToUpper = Convert.ToString(BASEtbl.Rows(i).Item("PRODUCTNAME")).ToUpper AndAlso
                   Convert.ToString(InpRow("PRODUCTCODE")) <> Convert.ToString(BASEtbl.Rows(i).Item("PRODUCTCODE")) Then
                    pnCnt += 1
                End If
            End If

            '化学名同一チェック
            If Convert.ToString(InpRow("CHEMICALNAME")) <> "" Then
                If Convert.ToString(InpRow("CHEMICALNAME")).ToUpper = Convert.ToString(BASEtbl.Rows(i).Item("CHEMICALNAME")).ToUpper AndAlso
                   Convert.ToString(InpRow("PRODUCTCODE")) <> Convert.ToString(BASEtbl.Rows(i).Item("PRODUCTCODE")) Then
                    cnCnt += 1
                End If
            End If
        Next

        If pnCnt > 0 Then
            dummy = New Label
            'メッセージ取得
            If Convert.ToString(InpRow("ENABLED")) = BaseDllCommon.CONST_FLAG_NO Then
                CommonFunctions.ShowMessage(C_MESSAGENO.PROHIBITCHAR, dummy, messageParams:=New List(Of String) From {"name"})
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.DUPLICATENAME, dummy)
            End If

            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & dummy.Text & "(" & dicField("PRODUCTNAME") & ":" & Convert.ToString(InpRow("PRODUCTNAME")) & ")" & errMessageStr
            errFlg = True
        End If

        If cnCnt > 0 Then
            'メッセージ取得
            If Convert.ToString(InpRow("ENABLED")) = BaseDllCommon.CONST_FLAG_NO Then
                dummy = New Label
                CommonFunctions.ShowMessage(C_MESSAGENO.PROHIBITCHAR, dummy, messageParams:=New List(Of String) From {"name"})

                errMessageStr = Me.ErrItemSet(InpRow)
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & dummy.Text & "(" & dicField("CHEMICALNAME") & ":" & Convert.ToString(InpRow("CHEMICALNAME")) & ")" & errMessageStr
                errFlg = True
            End If
        End If

        'UnNo個別チェック
        If Convert.ToString(InpRow("UNNO")) <> "" AndAlso
           Convert.ToString(InpRow("HAZARDCLASS")) <> "" AndAlso
           Convert.ToString(InpRow("PACKINGGROUP")) <> "" Then

            Dim chk As String = "Y"
            JudUNNO(Convert.ToString(InpRow("UNNO")), Convert.ToString(InpRow("HAZARDCLASS")), Convert.ToString(InpRow("PACKINGGROUP")), chk)

            'If chk = "N" Then
            '    dummy = New Label
            '    CommonFunctions.ShowMessage(C_MESSAGENO.PROHIBITCHAR, dummy, messageParams:=New List(Of String) From {"UN"})

            '    errMessageStr = Me.ErrItemSet(InpRow)
            '    If txtRightErrorMessage.Text <> "" Then
            '        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            '    End If
            '    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
            '                               & "・" & dummy.Text & "(" & dicField("UNNO") & ":" & Convert.ToString(InpRow("UNNO")) & ")" & errMessageStr
            '    errFlg = True
            'End If

        End If

        If Convert.ToString(InpRow("UNNO")) <> "" OrElse
            Convert.ToString(InpRow("HAZARDCLASS")) <> "" OrElse
            Convert.ToString(InpRow("PACKINGGROUP")) <> "" Then

            'タンクグレードチェック
            If Convert.ToString(InpRow("TANKGRADE")) = "" Then
                dummy = New Label
                CommonFunctions.ShowMessage(C_MESSAGENO.REQUIREDVALUE, dummy)

                errMessageStr = Me.ErrItemSet(InpRow)
                If txtRightErrorMessage.Text <> "" Then
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                End If
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                               & "・" & dummy.Text & "(" & dicField("TANKGRADE") & ":" & Convert.ToString(InpRow("TANKGRADE")) & ")" & errMessageStr
                errFlg = True
            End If
        End If

        'TANKGRADE個別チェック
        If Not TankGradeChk(Convert.ToString(InpRow("TANKGRADE"))) Then

            dummy = New Label
            CommonFunctions.ShowMessage(C_MESSAGENO.INPUTERROR, dummy)

            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                       & "・" & dummy.Text & "(" & dicField("TANKGRADE") & ":" & Convert.ToString(InpRow("TANKGRADE")) & ")" & errMessageStr
            errFlg = True

        End If

        'PRPVISIONS個別チェック

        If Not PrpvisionsChk(Convert.ToString(InpRow("PRPVISIONS"))) Then

            dummy = New Label
            CommonFunctions.ShowMessage(C_MESSAGENO.INPUTERROR, dummy)

            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                       & "・" & dummy.Text & "(" & dicField("PRPVISIONS") & ":" & Convert.ToString(InpRow("PRPVISIONS")) & ")" & errMessageStr
            errFlg = True

        End If

        'エラーコード設定
        If escapeFlg Then
            '表反映除外対象
            returnCode = C_MESSAGENO.REQUIREDVALUE
        ElseIf errFlg Then
            '更新出来ないレコードが発生しました(右Boxのエラー詳細を参照 )。
            returnCode = C_MESSAGENO.RIGHTBIXOUT
        End If

    End Sub
    ''' <summary>
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="argRow"></param>
    Protected Sub CheckSingle(ByVal argRow As DataRow, ByRef argDic As Dictionary(Of String, String), ByRef argEscFlg As Boolean)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar        '例外文字排除 String Get
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck          '項目チェック

        Dim errMessage As String = Nothing
        Dim errItemStr As String = Nothing

        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELDDIC = argDic
        COA0026FieldCheck.COA0026getFieldList()
        If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
            argDic = COA0026FieldCheck.FIELDDIC
        Else
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        For Each itm As KeyValuePair(Of String, String) In argDic

            If itm.Key = "UNNO" OrElse itm.Key = "HAZARDCLASS" OrElse itm.Key = "PRPVISIONS" Then
            Else
                '入力文字置き換え
                '画面PassWord内の使用禁止文字排除
                COA0008InvalidChar.CHARin = Convert.ToString(argRow(itm.Key))
                COA0008InvalidChar.COA0008RemoveInvalidChar()
                If COA0008InvalidChar.CHARin <> COA0008InvalidChar.CHARout Then
                    argRow(itm.Key) = COA0008InvalidChar.CHARout
                End If
            End If

            '単項目チェック
            COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
            COA0026FieldCheck.MAPID = CONST_MAPID
            COA0026FieldCheck.FIELD = itm.Key
            COA0026FieldCheck.VALUE = Convert.ToString(argRow(itm.Key))
            COA0026FieldCheck.COA0026FieldCheck()
            If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)

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
    '''' <summary>
    '''' 会社コードリストアイテムを設定
    '''' </summary>
    'Private Sub SetCompCodeListItem(selectedValue As String)
    '    'DataBase接続文字
    '    Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
    '    Dim SQLStr As String = Nothing
    '    Dim SQLcmd As New SqlCommand()
    '    Dim SQLdr As SqlDataReader = Nothing

    '    Try

    '        'リストクリア
    '        Me.lbCompCode.Items.Clear()

    '        'DataBase接続(Open)
    '        SQLcon.Open()

    '        '検索SQL文
    '        SQLStr =
    '             "SELECT COMPCODE, NAMES, NAMES_EN " _
    '           & " FROM  COS0004_COMP " _
    '           & " Where STYMD   <= @P1 " _
    '           & "   and ENDYMD  >= @P2 " _
    '           & "   and DELFLG  <> @P3 "
    '        SQLcmd = New SqlCommand(SQLStr, SQLcon)
    '        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
    '        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
    '        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
    '        PARA1.Value = Date.Now
    '        PARA2.Value = Date.Now
    '        PARA3.Value = "1"
    '        SQLdr = SQLcmd.ExecuteReader()

    '        While SQLdr.Read
    '            'DBからアイテムを設定
    '            If COA0019Session.LANGDISP = C_LANG.JA Then
    '                Me.lbCompCode.Items.Add(New ListItem(Convert.ToString(SQLdr("NAMES")), Convert.ToString(SQLdr("COMPCODE"))))
    '            Else
    '                Me.lbCompCode.Items.Add(New ListItem(Convert.ToString(SQLdr("NAMES_EN")), Convert.ToString(SQLdr("COMPCODE"))))
    '            End If
    '        End While

    '        '一応現在入力しているテキストと一致するものを選択状態
    '        If Me.lbCompCode.Items.Count > 0 Then
    '            Dim findListItem = Me.lbCompCode.Items.FindByValue(selectedValue)
    '            If findListItem IsNot Nothing Then
    '                findListItem.Selected = True
    '            End If
    '        End If

    '        '正常
    '        returnCode = C_MESSAGENO.NORMAL

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    Finally
    '        'CLOSE
    '        If Not SQLdr Is Nothing Then
    '            SQLdr.Close()
    '        End If
    '        If Not SQLcmd Is Nothing Then
    '            SQLcmd.Dispose()
    '            SQLcmd = Nothing
    '        End If
    '        If Not SQLcon Is Nothing Then
    '            SQLcon.Close()
    '            SQLcon.Dispose()
    '            SQLcon = Nothing
    '        End If
    '    End Try
    'End Sub
    '''' <summary>
    '''' 顧客コードリストアイテムを設定
    '''' </summary>
    'Private Sub SetShipperListItem(selectedValue As String)
    '    Dim GBA00004CountryRelated As New GBA00004CountryRelated

    '    Try

    '        'リストクリア
    '        Me.lbShipper.Items.Clear()

    '        If Me.txtCountry.Text <> "" Then
    '            GBA00004CountryRelated.COUNTRYCODE = Me.txtCountry.Text
    '        End If
    '        GBA00004CountryRelated.LISTBOX_SHIPPER = Me.lbShipper
    '            GBA00004CountryRelated.GBA00004getLeftListShipper()
    '        If GBA00004CountryRelated.ERR = C_MESSAGENO.NORMAL Then
    '            Me.lbShipper = DirectCast(GBA00004CountryRelated.LISTBOX_SHIPPER, ListBox)
    '        Else
    '            returnCode = GBA00004CountryRelated.ERR
    '            Return
    '        End If

    '        '一応現在入力しているテキストと一致するものを選択状態
    '        If Me.lbShipper.Items.Count > 0 Then
    '            Dim findListItem = Me.lbShipper.Items.FindByValue(selectedValue)
    '            If findListItem IsNot Nothing Then
    '                findListItem.Selected = True
    '            End If
    '        End If

    '        '正常
    '        returnCode = C_MESSAGENO.NORMAL

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    End Try
    'End Sub
    '''' <summary>
    '''' 国コードリストアイテムを設定
    '''' </summary>
    'Private Sub SetCountryListItem(selectedValue As String)
    '    Dim GBA00007OrganizationRelated As New GBA00007OrganizationRelated

    '    Try

    '        'リストクリア
    '        Me.lbCountry.Items.Clear()

    '        GBA00007OrganizationRelated.LISTBOX_COUNTRY = Me.lbCountry
    '        GBA00007OrganizationRelated.GBA00007getLeftListCountry()
    '        If GBA00007OrganizationRelated.ERR = C_MESSAGENO.NORMAL Then
    '            Me.lbCountry = DirectCast(GBA00007OrganizationRelated.LISTBOX_COUNTRY, ListBox)
    '        Else
    '            returnCode = GBA00007OrganizationRelated.ERR
    '            Return
    '        End If

    '        '一応現在入力しているテキストと一致するものを選択状態
    '        If Me.lbCountry.Items.Count > 0 Then
    '            Dim findListItem = Me.lbCountry.Items.FindByValue(selectedValue)
    '            If findListItem IsNot Nothing Then
    '                findListItem.Selected = True
    '            End If
    '        End If

    '        '正常
    '        returnCode = C_MESSAGENO.NORMAL

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    End Try
    'End Sub
    ''' <summary>
    ''' 国連番号リストアイテムを設定
    ''' </summary>
    Private Sub SetUNNOListItem()

        Dim GBA00001UnNo As New GBA00001UnNo              '項目チェック
        'Dim UnNoKeyValue As Dictionary(Of String, String) = New Dictionary(Of String, String)

        'リストクリア
        Me.lbUNNO.Items.Clear()

        'リスト設定
        GBA00001UnNo.LISTBOX = Me.lbUNNO
        GBA00001UnNo.GBA00001getLeftListUnNo()
        If GBA00001UnNo.ERR = C_MESSAGENO.NORMAL Then
            Me.lbUNNO = GBA00001UnNo.LISTBOX
            ViewState("UNNOKEYVALUE") = GBA00001UnNo.UnNoKeyValue

            'Me.lbUNNO.Items.Add(NONDG)
            '正常
            returnCode = C_MESSAGENO.NORMAL
        ElseIf GBA00001UnNo.ERR = C_MESSAGENO.NODATA Then
            'UNNOデータ未取得の場合は素通り
            returnCode = C_MESSAGENO.NODATA
        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00001UnNo.ERR)})
        End If

    End Sub
    ''' <summary>
    ''' 削除フラグリストアイテムを設定
    ''' </summary>
    Private Sub SetDelFlgListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

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
            returnCode = COA0017FixValue.ERR
            Return
        End If

    End Sub
    ''' <summary>
    ''' 有効フラグリストアイテムを設定
    ''' </summary>
    Private Sub SetEnabledListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        returnCode = C_MESSAGENO.NORMAL

        'リストクリア
        Me.lbEnabled.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "ENABLED"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbEnabled
        Else
            COA0017FixValue.LISTBOX2 = Me.lbEnabled
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbEnabled = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbEnabled = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

        Else
            returnCode = COA0017FixValue.ERR
            Return
        End If

    End Sub
    ''' <summary>
    ''' 等級リストアイテムを設定
    ''' </summary>
    Private Sub SetHazardClassListItem()
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

            'Me.lbHazardClass.Items.Add(NONDG)

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
    Private Sub SetPackingGroupListItem()
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

            'Me.lbPackingGroup.Items.Add(NONDG)

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
    '''' 会社名設定
    '''' </summary>
    'Public Sub txtCompCode_Change()

    '    Try
    '        Me.lblCompCodeText.Text = ""

    '        SetCompCodeListItem(Me.txtCompCode.Text)
    '        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbCompCode.Items.Count > 0 Then
    '            Dim findListItem = Me.lbCompCode.Items.FindByValue(Me.txtCompCode.Text)
    '            If findListItem IsNot Nothing Then
    '                Me.lblCompCodeText.Text = findListItem.Text
    '                'Me.lblCompCodeText.Attributes.Add("title", findListItem.Text)
    '            Else
    '                Dim findListItemUpper = Me.lbCompCode.Items.FindByValue(Me.txtCompCode.Text.ToUpper)
    '                If findListItemUpper IsNot Nothing Then
    '                    Me.lblCompCodeText.Text = findListItemUpper.Text
    '                    'Me.lblCompCodeText.Attributes.Add("title", findListItemUpper.Text)
    '                    Me.txtCompCode.Text = findListItemUpper.Value
    '                End If
    '            End If
    '        End If

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    End Try
    'End Sub
    '''' <summary>
    '''' 顧客名設定
    '''' </summary>
    'Public Sub txtShipper_Change()

    '    Try
    '        Me.lblShipperText.Text = ""

    '        SetShipperListItem(Me.txtShipper.Text)
    '        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShipper.Items.Count > 0 Then
    '            Dim findListItem = Me.lbShipper.Items.FindByValue(Me.txtShipper.Text)
    '            If findListItem IsNot Nothing Then
    '                Dim parts As String()
    '                parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
    '                Me.lblShipperText.Text = parts(1)
    '            Else
    '                Dim findListItemUpper = Me.lbShipper.Items.FindByValue(Me.txtShipper.Text.ToUpper)
    '                If findListItemUpper IsNot Nothing Then
    '                    Dim parts As String()
    '                    parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
    '                    Me.lblShipperText.Text = parts(1)
    '                    Me.txtShipper.Text = parts(0)
    '                End If
    '            End If
    '        End If

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    End Try
    'End Sub
    '''' <summary>
    '''' 国名設定
    '''' </summary>
    'Public Sub txtCountry_Change()

    '    Try
    '        Me.lblCountryText.Text = ""

    '        SetCountryListItem(Me.txtCountry.Text)
    '        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbCountry.Items.Count > 0 Then
    '            Dim findListItem = Me.lbCountry.Items.FindByValue(Me.txtCountry.Text)
    '            If findListItem IsNot Nothing Then
    '                Dim parts As String()
    '                parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
    '                Me.lblCountryText.Text = parts(1)
    '            Else
    '                Dim findListItemUpper = Me.lbCountry.Items.FindByValue(Me.txtCountry.Text.ToUpper)
    '                If findListItemUpper IsNot Nothing Then
    '                    Dim parts As String()
    '                    parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
    '                    Me.lblCountryText.Text = parts(1)
    '                    Me.txtCountry.Text = parts(0)
    '                End If
    '            End If
    '        End If

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    End Try
    'End Sub

    ''' <summary>
    ''' 国連番号名設定
    ''' </summary>
    Public Sub UNNO_Change()

        Dim HAZARDCLASS As String = ""
        Dim PACKINGGROUP As String = ""
        Dim UNNO As String = ""
        Dim ind As Integer = 0

        Try
            Dim findKey As String = Nothing

            'リピーター国連番号
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "UNNO" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = ""

                    UNNO = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                    If UNNO.Replace(",", "") = "" Then
                        UNNO = UNNO.Replace(",", "")
                        DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = UNNO

                    Else
                        For j As Integer = 0 To UNNO.Length

                            If UNNO.Contains(",,") Then
                                UNNO = UNNO.Replace(",,", ",")
                            End If
                            DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = UNNO
                        Next

                    End If

                    ind = i

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "HAZARDCLASS" Then

                    HAZARDCLASS = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "PACKINGGROUP" Then

                    PACKINGGROUP = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                End If
            Next

            findKey = UNNO & "," & HAZARDCLASS & "," & PACKINGGROUP
            'If UNNO = NONDG Then
            '    findKey = NONDG
            'End If

            SetUNNOListItem()
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbUNNO.Items.Count > 0 Then
                Dim findListItem = Me.lbUNNO.Items.FindByText(findKey)
                If findListItem IsNot Nothing Then
                    'If findListItem.Text = NONDG Then
                    '    DirectCast(WF_DViewRep1.Items(ind).FindControl("WF_Rep1_VALUE_TEXT_2"),
                    '                    Label).Text = findListItem.Text
                    'Else

                    Dim UnNoKeyValue As Dictionary(Of String, String) = DirectCast(ViewState("UNNOKEYVALUE"), Dictionary(Of String, String))
                    DirectCast(WF_DViewRep1.Items(ind).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                        Label).Text = UnNoKeyValue(findListItem.Text)
                    'End If

                Else
                    Dim findListItemUpper = Me.lbUNNO.Items.FindByValue(findKey.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim UnNoKeyValue As Dictionary(Of String, String) = DirectCast(ViewState("UNNOKEYVALUE"), Dictionary(Of String, String))
                        DirectCast(WF_DViewRep1.Items(ind).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                        Label).Text = UnNoKeyValue(findListItemUpper.Text)
                        DirectCast(WF_DViewRep1.Items(ind).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = findListItemUpper.Text
                    End If
                End If
            End If

            '警告設定
            JudUNNO(UNNO, HAZARDCLASS, PACKINGGROUP)

            'Tank*付与
            ChkTankAst(UNNO, HAZARDCLASS, PACKINGGROUP)

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

            SetDelFlgListItem()
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbDelFlg.Items.Count > 0 Then
                Dim findListItem = Me.lbDelFlg.Items.FindByValue(Me.txtDelFlg.Text)
                If findListItem IsNot Nothing Then
                    Me.lblDelFlgText.Text = findListItem.Text
                    'Me.lblDelFlgText.Attributes.Add("title", findListItem.Text)
                Else
                    Dim findListItemUpper = Me.lbDelFlg.Items.FindByValue(Me.txtDelFlg.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblDelFlgText.Text = findListItemUpper.Text
                        'Me.lblDelFlgText.Attributes.Add("title", findListItemUpper.Text)
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
    ''' 有効フラグ名設定
    ''' </summary>
    Public Sub ENABLED_Change()

        Try

            'リピーター有効フラグ
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "ENABLED" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text <> "" Then

                        SetEnabledListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbEnabled.Items.Count > 0 Then
                            Dim findListItem = Me.lbEnabled.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
                                                                                                    TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                        Label).Text = findListItem.Text
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
    '' <summary>
    '' 等級名設定
    '' </summary>
    Public Sub HAZARDCLASS_Change()
        Dim HAZARDCLASS As String = ""
        Dim PACKINGGROUP As String = ""
        Dim UNNO As String = ""

        Try
            'リピーター等級名
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "HAZARDCLASS" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = ""

                    HAZARDCLASS = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                    If HAZARDCLASS.Replace(",", "") <> "" Then

                        SetHazardClassListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbHazardClass.Items.Count > 0 Then
                            Dim findListItem = Me.lbHazardClass.Items.FindByValue(HAZARDCLASS)

                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                           Label).Text = findListItem.Text
                            End If
                        End If

                        For j As Integer = 0 To HAZARDCLASS.Length

                            If HAZARDCLASS.Contains(",,") Then
                                HAZARDCLASS = HAZARDCLASS.Replace(",,", ",")
                            End If
                            DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = HAZARDCLASS
                        Next

                    Else
                        DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = HAZARDCLASS.Replace(",", "")
                    End If

                    HAZARDCLASS = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "PACKINGGROUP" Then

                    PACKINGGROUP = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "UNNO" Then

                    UNNO = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                End If

            Next

            '等級が変更された場合、国連番号も変更
            UNNO_Change()
            ''警告判定
            'JudUNNO(Me.txtUNNO.Text, HAZARDCLASS, PACKINGGROUP)

            'Tank*付与
            ChkTankAst(UNNO, HAZARDCLASS, PACKINGGROUP)

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    '' <summary>
    '' 容器等級名設定
    '' </summary>
    Public Sub PACKINGGROUP_Change()
        Dim HAZARDCLASS As String = ""
        Dim PACKINGGROUP As String = ""
        Dim UNNO As String = ""

        Try
            'リピーター容器等級
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "PACKINGGROUP" Then

                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text <> "" Then

                        GetPackingGroupCharConv()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso charConvList.Items.Count > 0 Then

                            Dim charConvItem = charConvList.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text)
                            If charConvItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = charConvItem.Text
                            End If
                        End If

                        SetPackingGroupListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPackingGroup.Items.Count > 0 Then
                            Dim findListItem = Me.lbPackingGroup.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
                                                                                                    TextBox).Text)
                            If findListItem IsNot Nothing Then
                                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                        Label).Text = findListItem.Text
                            End If
                        End If
                    End If

                    PACKINGGROUP = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "HAZARDCLASS" Then

                    HAZARDCLASS = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                ElseIf DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "UNNO" Then

                    UNNO = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                End If
            Next

            '等級が変更された場合、国連番号も変更
            UNNO_Change()
            ''警告判定
            'JudUNNO(Me.txtUNNO.Text, HAZARDCLASS, PACKINGGROUP)

            'Tank*付与
            ChkTankAst(UNNO, HAZARDCLASS, PACKINGGROUP)

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
                Case "★" & warningDisp
                    BASEtbl.Rows(i)(1) = warningDisp
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
        'txtCompCode.Text = ""
        'lblCompCodeText.Text = ""
        'txtShipper.Text = ""
        'lblShipperText.Text = ""
        'txtCountry.Text = ""
        'lblCountryText.Text = ""
        'txtUNNO.Text = ""
        'lblUNNOText.Text = ""
        txtProduct.Text = ""
        lblProductText.Text = ""
        txtStYMD.Text = ""
        txtEndYMD.Text = ""
        txtDelFlg.Text = ""
        lblDelFlgText.Text = ""
        txtWarMsg.Text = ""

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
        txtProduct.Focus()

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
                Case "★" & warningDisp
                    BASEtbl.Rows(i)("OPERATION") = warningDisp
            End Select
        Next

        Dim compareUpdTargetFieldList = CommonFunctions.CreateCompareFieldList({"COMPCODE", "PRODUCTCODE", "STYMD"})
        Dim compareModFieldList = CommonFunctions.CreateCompareFieldList({"ENDYMD", "PRODUCTNAME", "CHEMICALNAME", "IMDGCODE", "UNNO",
                                                                                  "HAZARDCLASS", "PACKINGGROUP", "FIRESERVICEACT", "PANDDCONTROLACT",
                                                                                  "CASNO", "GRAVITY", "FLASHPOINT", "TANKGRADE", "PRPVISIONS",
                                                                                  "ENABLED", "MANUFACTURE", "REMARK", "DELFLG"})

        Dim drInput As DataRow = INPtbl.NewRow
        For i As Integer = 0 To INPtbl.Rows.Count - 1

            drInput.ItemArray = INPtbl(i).ItemArray
            If Convert.ToString(drInput("HIDDEN")) <> "1" Then ' "1" ・・・取り込み対象外エラー

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
                    If CommonFunctions.CompareDataFields(workBaseRow, drInput, compareUpdTargetFieldList) Then

                        ' 変更なし  
                        If Convert.ToString(drInput("OPERATION")) <> errDisp AndAlso
                           CommonFunctions.CompareDataFields(workBaseRow, drInput, compareModFieldList) Then
                            workBasePos = -999    '-999 は登録対象外
                            If WF_DViewRepPDF.Items.Count <> 0 Then
                                For k As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
                                    If hdnListBoxPDF.Items.Count <> 0 Then
                                        For l As Integer = 0 To hdnListBoxPDF.Items.Count - 1
                                            If (DirectCast(WF_DViewRepPDF.Items(k).FindControl("WF_Rep_FILENAME"), Label).Text = hdnListBoxPDF.Items(l).Text And
                                            DirectCast(WF_DViewRepPDF.Items(k).FindControl("WF_Rep_DELFLG"), TextBox).Text = hdnListBoxPDF.Items(l).Value) Then
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
                                        If DirectCast(WF_DViewRepPDF.Items(k).FindControl("WF_Rep_DELFLG"), TextBox).Text = BaseDllCommon.CONST_FLAG_YES Then
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

                    '警告チェック
                    If WarCheck(drInput) AndAlso Convert.ToString(drInput("OPERATION")) <> errDisp Then
                        drInput("OPERATION") = warningDisp
                        warningFlg = True
                    End If
                End If

                ' 内部テーブル編集
                If workBasePos >= 0 Then

                    '警告チェック
                    If WarCheck(drInput) AndAlso Convert.ToString(drInput("OPERATION")) <> errDisp Then
                        drInput("OPERATION") = warningDisp
                        warningFlg = True
                    End If

                    '内部テーブル検索
                    For k As Integer = 0 To BASEtbl.Rows.Count - 1

                        'Dim workBaseRow2 As DataRow
                        'workBaseRow2 = BASEtbl.NewRow
                        'workBaseRow2.ItemArray = BASEtbl.Rows(k).ItemArray
                        workBaseRow2 = BASEtbl.Rows(k)

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

                                drInput("OPERATION") = errDisp
                            End If
                        End If

                    Next

                    'If endFlg Then
                    '    Exit For
                    'End If

                    '固定項目
                    'Dim workBaseRow As DataRow
                    'workBaseRow = BASEtbl.NewRow
                    workBaseRow3 = BASEtbl.NewRow

                    If workBasePos < BASEtbl.Rows.Count Then
                        '更新対象を設定
                        workBaseRow3.ItemArray = BASEtbl.Rows(workBasePos).ItemArray
                    End If

                    '固定項目
                    workBaseRow3("LINECNT") = workBasePos + 1
                    If Convert.ToString(drInput("OPERATION")) <> errDisp AndAlso Convert.ToString(drInput("OPERATION")) <> warningDisp Then
                        workBaseRow3("OPERATION") = updateDisp
                    Else
                        workBaseRow3("OPERATION") = drInput("OPERATION")
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

                    'エラーの場合、値を更新しない
                    'エラーかつ新規の場合、値を設定する
                    If Convert.ToString(workBaseRow3("OPERATION")) <> errDisp OrElse
                        (Convert.ToString(workBaseRow3("OPERATION")) = errDisp AndAlso newFlg) Then
                        '個別項目
                        workBaseRow3("COMPCODE") = drInput("COMPCODE")
                        workBaseRow3("PRODUCTCODE") = drInput("PRODUCTCODE")
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
                        workBaseRow3("PRODUCTNAME") = Convert.ToString(drInput("PRODUCTNAME")).ToUpper
                        workBaseRow3("CHEMICALNAME") = Convert.ToString(drInput("CHEMICALNAME")).ToUpper
                        workBaseRow3("IMDGCODE") = drInput("IMDGCODE")
                        workBaseRow3("UNNO") = drInput("UNNO")
                        workBaseRow3("HAZARDCLASS") = drInput("HAZARDCLASS")
                        workBaseRow3("PACKINGGROUP") = drInput("PACKINGGROUP")
                        workBaseRow3("FIRESERVICEACT") = drInput("FIRESERVICEACT")
                        workBaseRow3("PANDDCONTROLACT") = drInput("PANDDCONTROLACT")
                        workBaseRow3("CASNO") = drInput("CASNO")
                        workBaseRow3("GRAVITY") = drInput("GRAVITY")
                        workBaseRow3("FLASHPOINT") = drInput("FLASHPOINT")
                        workBaseRow3("TANKGRADE") = drInput("TANKGRADE")
                        workBaseRow3("PRPVISIONS") = drInput("PRPVISIONS")
                        workBaseRow3("ENABLED") = drInput("ENABLED")
                        workBaseRow3("MANUFACTURE") = drInput("MANUFACTURE")
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
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
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
        'txtCompCode.Text = Convert.ToString(dataTable(0)("COMPCODE"))
        'txtCompCode_Change()
        'txtCountry.Text = dataTable(0)("COUNTRYCODE")
        'txtCountry_Change()
        'txtShipper.Text = dataTable(0)("CUSTOMERCODE")
        'txtShipper_Change()
        txtProduct.Text = Convert.ToString(dataTable(0)("PRODUCTCODE"))
        'txtUNNO.Text = Convert.ToString(dataTable(0)("UNNO"))
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

        '名称設定
        ENABLED_Change()
        HAZARDCLASS_Change()
        PACKINGGROUP_Change()
        UNNO_Change()

        'タブ別処理(書類（PDF）)
        PDFInitRead(hdnCompCode.Value, txtProduct.Text)

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
                Case "★" & warningDisp
                    BASEtbl.Rows(i)(1) = warningDisp
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
            Case warningDisp
                BASEtbl.Rows(lineCnt)(1) = "★" & warningDisp
            Case Else
        End Select

        '画面表示データ保存
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        BASEtbl.Clear()
        BASEtbl.Dispose()

        '画面編集
        txtProduct.Focus()

    End Sub
    ''' <summary>
    ''' Detail タブ切替処理
    ''' </summary>
    Protected Sub DetailTABChange()

        '■■■ セッション変数設定 ■■■
        '固定項目設定  ★必須処理
        Session("Class") = "DetailTABChange"
        Dim DTABChangeVal As Integer
        Try
            Integer.TryParse(hdnDTABChange.Value, DTABChangeVal)
        Catch ex As Exception
            DTABChangeVal = 0
        End Try

        WF_DetailMView.ActiveViewIndex = DTABChangeVal

        '初期値（書式）変更

        '積載品情報
        lblDtabProduct.Style.Remove("color")
        lblDtabProduct.Style.Add("color", "black")
        lblDtabProduct.Style.Remove("background-color")
        lblDtabProduct.Style.Add("background-color", "rgb(255,255,253)")
        lblDtabProduct.Style.Remove("border")
        lblDtabProduct.Style.Add("border", "1px solid black")
        lblDtabProduct.Style.Remove("font-weight")
        lblDtabProduct.Style.Add("font-weight", "normal")

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
                lblDtabProduct.Style.Remove("color")
                lblDtabProduct.Style.Add("color", "blue")
                lblDtabProduct.Style.Remove("background-color")
                lblDtabProduct.Style.Add("background-color", "rgb(220,230,240)")
                lblDtabProduct.Style.Remove("border")
                lblDtabProduct.Style.Add("border", "1px solid blue")
                lblDtabProduct.Style.Remove("font-weight")
                lblDtabProduct.Style.Add("font-weight", "bold")
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
    '''' <summary>
    '''' 有効フラグチェック
    '''' </summary>
    '''' <param name="UnNoVal"></param>
    '''' <returns>有効無効判定結果</returns>
    'Protected Function EnabledCheck(ByVal UnNoVal As String, ByVal HazClsVal As String, ByVal PacGrpVal As String) As String

    '    Dim EnabledFlg As String = ""

    '    'DataBase接続文字
    '    Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
    '    Dim SQLStr As String = Nothing
    '    Dim SQLcmd As New SqlCommand()
    '    Dim SQLdr As SqlDataReader = Nothing

    '    Try

    '        'DataBase接続(Open)
    '        SQLcon.Open()

    '        '検索SQL文
    '        SQLStr =
    '             "SELECT ENABLED " _
    '           & " FROM  GBM0007_UNNO " _
    '           & " Where UNNO          = @P1 " _
    '           & "   and HAZARDCLASS   = @P3 " _
    '           & "   and PACKINGGROUP  = @P4 " _
    '           & "   and DELFLG       <> @P2 "

    '        SQLcmd = New SqlCommand(SQLStr, SQLcon)
    '        Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.VarChar)
    '        Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.VarChar)
    '        Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.VarChar)
    '        Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.VarChar)
    '        PARA1.Value = UnNoVal
    '        PARA2.Value = BaseDllCommon.CONST_FLAG_YES
    '        PARA3.Value = HazClsVal
    '        PARA4.Value = PacGrpVal

    '        SQLdr = SQLcmd.ExecuteReader()

    '        While SQLdr.Read
    '            EnabledFlg = Convert.ToString(SQLdr("ENABLED"))
    '        End While

    '    Catch ex As Exception
    '        returnCode = C_MESSAGENO.EXCEPTION
    '        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
    '        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
    '        COA0003LogFile.TEXT = ex.ToString()
    '        COA0003LogFile.MESSAGENO = returnCode
    '        COA0003LogFile.COA0003WriteLog()
    '    Finally
    '        'CLOSE
    '        If Not SQLdr Is Nothing Then
    '            SQLdr.Close()
    '        End If
    '        If Not SQLcmd Is Nothing Then
    '            SQLcmd.Dispose()
    '            SQLcmd = Nothing
    '        End If
    '        If Not SQLcon Is Nothing Then
    '            SQLcon.Close()
    '            SQLcon.Dispose()
    '            SQLcon = Nothing
    '        End If
    '    End Try

    '    Return EnabledFlg

    'End Function
    ''' <summary>
    ''' PDFリスト設定
    ''' </summary>
    Private Sub SetPDFListBox()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue
        returnCode = C_MESSAGENO.NORMAL

        'リストクリア
        Me.WF_Rep2_PDFselect.Items.Clear()

        'PDF選択ListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "GBM00008_PDF"
        COA0017FixValue.LISTBOX1 = WF_Rep2_PDFselect
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            WF_Rep2_PDFselect = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            WF_Rep2_PDFselect.SelectedIndex = 0
        Else
            returnCode = COA0017FixValue.ERR
            Return
        End If
    End Sub
    ''' <summary>
    ''' PDFファイルアップロード入力処理(PDFドロップ時)
    ''' </summary>
    Protected Sub UploadPDF()
        'セッション変数設定
        '固定項目設定
        Session("Class") = "UploadPDF"

        '初期設定
        Dim UpDir As String = Nothing

        '事前確認
        '一覧に存在かチェック

        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            If hdnCompCode.Value = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) OrElse
                txtProduct.Text = Convert.ToString(BASEtbl.Rows(i)("PRODUCTCODE")) Then
                'txtUNNO.Text = Convert.ToString(BASEtbl.Rows(i)("UNNO")) Then
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
        UpDir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT\"
        UpDir = UpDir & "MSDS\" & txtProduct.Text & "\Update_D"

        'ディレクトリが存在しない場合、作成する
        If System.IO.Directory.Exists(UpDir) = False Then
            System.IO.Directory.CreateDirectory(UpDir)
        End If

        For Each tempFile As String In System.IO.Directory.GetFiles(COA0019Session.UPLOADDir & "\" & COA0019Session.USERID, "*.*")

            'ディレクトリ付ファイル名より、ファイル名編集
            Dim DirFile As String = tempFile
            DirFile = System.IO.Path.GetFileName(tempFile)
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
        UpDir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\" & "GBM00008PRODUCT" & "\"
        UpDir = UpDir & WF_Rep2_PDFselect.SelectedItem.Text & "\" & txtProduct.Text & "\Update_D"

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
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = WW_Files_name.Item(i)
            '削除
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = BaseDllCommon.CONST_FLAG_NO
            'FILEPATH
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = WW_Files_dir.Item(i)

        Next

        'イベント設定
        Dim WW_ATTR As String = ""
        Dim ItemCnt As Integer = 0
        For i As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)

            ItemCnt = i
            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "Field_DBclick('vLeftDelFlg' "
            'WW_ATTR = WW_ATTR & ", '" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text & "'"
            WW_ATTR = WW_ATTR & ", '" & ItemCnt.ToString & "'"
            WW_ATTR = WW_ATTR & " )"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
        Next

        'メッセージ編集
        CommonFunctions.ShowMessage(C_MESSAGENO.NORMALIMPORT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

    End Sub
    ''' <summary>
    ''' PDFカラム設定
    ''' </summary>
    Protected Sub PDFtblColumnsAdd()

        '■ セッション変数設定
        '固定項目設定
        Session("Class") = "PDFtblColumnsAdd"

        If PDFtbl.Columns.Count = 0 Then
        Else
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
    ''' <param name="prmProductCode"></param>
    Protected Sub PDFDBupdate(ByVal prmCompCode As String, ByVal prmProductCode As String, ByVal prmApplyId As String)
        'セッション変数設定
        '固定項目設定
        Session("Class") = "PDFDBupdate"

        '初期設定
        Dim WW_DirSend As String = ""
        Dim WW_DirH As String = ""
        Dim WW_DirD As String = ""
        Dim WW_DirHON As String = ""
        Dim appFlg As String = ""

        'DB更新ボタン押下時
        '　　　　・Update_Hフォルダ内容を正式フォルダにコピー
        '　　　　・Update_D・Update_Hをお掃除
        'DB反映処理

        For i As Integer = 1 To WF_Rep2_PDFselect.Items.Count
            '○FTP格納ディレクトリ編集

            If prmApplyId = "" Then
                '正式ディレクトリ
                WW_DirHON = COA0019Session.UPLOADFILESDir & "\MSDS\" & prmProductCode
                appFlg = "2"
            Else
                '承認前ディレクトリ
                WW_DirHON = COA0019Session.BEFOREAPPROVALDir & "\MSDS\" & prmProductCode
                appFlg = "1"
            End If

            'ディレクトリが存在しない場合、作成する
            If System.IO.Directory.Exists(WW_DirHON) = False Then
                System.IO.Directory.CreateDirectory(WW_DirHON)
            End If

            'Tempフォルダーが存在したら処理する（EXCEL入力の場合、Tempができないため）
            WW_DirH = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT\"
            WW_DirH = WW_DirH & "MSDS\" & prmProductCode & "\Update_H"
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
                WW_DirD = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT\"
                WW_DirD = WW_DirD & "MSDS\" & prmProductCode & "\Update_D"

                For Each tempFile As String In System.IO.Directory.GetFiles(WW_DirD, "*", System.IO.SearchOption.AllDirectories)
                    Try
                        System.IO.File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next

                '集配信用フォルダ格納処理
                Dim COA00034SendDirectory As New COA00034SendDirectory
                Dim pgmDir As String = "\MSDS\" & prmProductCode
                COA00034SendDirectory.SendDirectoryCopy(pgmDir, WW_DirHON, appFlg)

            End If
        Next
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
        WW_Dir = WW_Dir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT\MSDS"

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
    ''' <param name="prmProductCode"></param>
    Protected Sub PDFInitRead(ByVal prmCompCode As String, ByVal prmProductCode As String)
        Dim WW_UPfiles As String()

        'セッション変数設定
        '固定項目設定  ★必須処理
        Session("Class") = "PDFInitRead"

        '初期設定
        Dim WW_Dir As String

        '事前確認
        '一覧に存在するかチェック
        If prmCompCode = "" OrElse prmProductCode = "" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.PDFLISTEXISTS, Me.lblFooterMessage)
            Return
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If prmCompCode = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) OrElse
                   prmProductCode = Convert.ToString(BASEtbl.Rows(i)("PRODUCTCODE")) Then
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
        'PDF格納Dir作成
        For i As Integer = 1 To WF_Rep2_PDFselect.Items.Count
            'PDF格納ディレクトリ編集
            WW_Dir = ""
            WW_Dir = WW_Dir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT"

            '正式ディレクトリ作成＞積載品ディレクトリ作成
            If lblApplyIDText.Text = "" Then
                If System.IO.Directory.Exists(COA0019Session.UPLOADFILESDir & "\MSDS\" & prmProductCode) Then
                Else
                    System.IO.Directory.CreateDirectory(COA0019Session.UPLOADFILESDir & "\MSDS\" & prmProductCode)
                End If
            Else
                '承認前ディレクトリ
                If System.IO.Directory.Exists(COA0019Session.BEFOREAPPROVALDir & "\MSDS\" & prmProductCode) Then
                Else
                    System.IO.Directory.CreateDirectory(COA0019Session.BEFOREAPPROVALDir & "\MSDS\" & prmProductCode)
                End If
            End If

            '一時保存ディレクトリ作成
            If System.IO.Directory.Exists(WW_Dir & "\MSDS") Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir & "\MSDS")
            End If

            '一時保存ディレクトリ＞積載品ディレクトリ作成
            If System.IO.Directory.Exists(WW_Dir & "\MSDS\" & prmProductCode) Then
            Else
                System.IO.Directory.CreateDirectory(WW_Dir & "\MSDS\" & prmProductCode)
            End If

            '一時保存ディレクトリ＞積載品ディレクトリ作成＞Update_H の処理
            If System.IO.Directory.Exists(WW_Dir & "\MSDS\" & prmProductCode & "\Update_H") Then
                '連続処理の場合、前回処理を残す
            Else
                'ユーザIDディレクトリ＞積載品コードディレクトリ作成＞Update_H 作成
                System.IO.Directory.CreateDirectory(WW_Dir & "\MSDS\" & prmProductCode & "\Update_H")

                '正式フォルダ内ファイル→一時保存ディレクトリ＞積載品ディレクトリ作成＞Update_H へコピー
                If lblApplyIDText.Text = "" Then
                    WW_UPfiles = System.IO.Directory.GetFiles(COA0019Session.UPLOADFILESDir & "\MSDS\" & prmProductCode, "*", System.IO.SearchOption.AllDirectories)
                Else
                    '承認前
                    WW_UPfiles = System.IO.Directory.GetFiles(COA0019Session.BEFOREAPPROVALDir & "\MSDS\" & prmProductCode, "*", System.IO.SearchOption.AllDirectories)
                End If

                For Each tempFile As String In WW_UPfiles
                    'ディレクトリ付ファイル名より、ファイル名編集
                    Dim WW_File As String = System.IO.Path.GetFileName(tempFile)
                    '正式フォルダ内全PDF→Update_Hフォルダへ上書コピー
                    System.IO.File.Copy(tempFile, WW_Dir & "\MSDS\" & prmProductCode & "\Update_H\" & WW_File, True)
                Next
            End If

            '一時保存ディレクトリ＞ユーザIDディレクトリ作成＞積載品ディレクトリ作成＞Update_D 処理
            If System.IO.Directory.Exists(WW_Dir & "\MSDS\" & prmProductCode & "\Update_D") Then
                'Update_Dフォルダ内ファイル削除
                WW_UPfiles = System.IO.Directory.GetFiles(WW_Dir & "\MSDS\" & prmProductCode & "\Update_D", "*", System.IO.SearchOption.AllDirectories)
                For Each tempFile As String In WW_UPfiles
                    Try
                        System.IO.File.Delete(tempFile)
                    Catch ex As Exception
                    End Try
                Next
            Else
                'Update_Dが存在しない場合、Update_Dフォルダ作成
                System.IO.Directory.CreateDirectory(WW_Dir & "\MSDS\" & prmProductCode & "\Update_D")
            End If

            'Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
            WW_UPfiles = System.IO.Directory.GetFiles(WW_Dir & "\MSDS\" & prmProductCode & "\Update_H", "*", System.IO.SearchOption.AllDirectories)
            For Each tempFile As String In WW_UPfiles
                'ディレクトリ付ファイル名より、ファイル名編集
                Dim WW_File As String = System.IO.Path.GetFileName(tempFile)
                'Update_Hフォルダ内全PDF→Update_Dフォルダへコピー
                System.IO.File.Copy(tempFile, WW_Dir & "\MSDS\" & prmProductCode & "\Update_D\" & WW_File, True)
            Next
        Next

        '画面編集
        'PDF格納ディレクトリ編集
        WW_Dir = ""
        WW_Dir = WW_Dir & COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT\"
        WW_Dir = WW_Dir & "MSDS\" & prmProductCode & "\Update_D"

        '表更新前のUpdate_Dディレクトリ内ファイル一覧
        Dim WW_Files_dir As New List(Of String)
        Dim WW_Files_name As New List(Of String)
        Dim WW_Files_del As New List(Of String)

        WW_UPfiles = System.IO.Directory.GetFiles(WW_Dir, "*", System.IO.SearchOption.AllDirectories)
        For Each tempFile As String In WW_UPfiles
            'If Right(tempFile, 4).ToUpper = ".PDF" Then
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
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = WW_Files_name.Item(i)
            '削除
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = BaseDllCommon.CONST_FLAG_NO
            'FILEPATH
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text = WW_Files_dir.Item(i)

            hdnListBoxPDF.Items.Add(New ListItem(WW_Files_name.Item(i), BaseDllCommon.CONST_FLAG_NO))
        Next

        'イベント設定
        Dim WW_ATTR As String = ""
        Dim ItemCnt As Integer = 0
        For i As Integer = 0 To WF_DViewRepPDF.Items.Count - 1
            'ダブルクリック時コード検索イベント追加(ファイル名称用)
            WW_ATTR = "DtabPDFdisplay('" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text & "')"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Attributes.Add("ondblclick", WW_ATTR)

            ItemCnt = i
            'ダブルクリック時コード検索イベント追加(削除フラグ用)
            WW_ATTR = "Field_DBclick('vLeftDelFlg' "
            'WW_ATTR = WW_ATTR & ", '" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text & "'"
            WW_ATTR = WW_ATTR & ", '" & ItemCnt.ToString & "'"
            WW_ATTR = WW_ATTR & " )"
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Remove("ondblclick")
            DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Attributes.Add("ondblclick", WW_ATTR)
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
            If DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text = hdnDTABPDFEXCELdisplay.Value Then
                'ディレクトリが存在しない場合、作成する
                If System.IO.Directory.Exists(WW_Dir) = False Then
                    System.IO.Directory.CreateDirectory(WW_Dir)
                End If

                'ダウンロードファイル送信準備
                System.IO.File.Copy(DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text,
                                    WW_Dir & "\" & DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text, True)

                'ダウンロード処理へ遷移
                hdnPrintURL.Value = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host _
                                    & "/" & COA0019Session.PRINTROOTUrl & "/" & COA0019Session.USERID & "/" _
                                    & Uri.EscapeUriString(DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILENAME"), Label).Text)

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
        'セッション変数設定
        '固定項目設定
        Session("Class") = "PDFTabListUpdate"

        '初期設定
        Dim WW_Dir As String

        '事前確認
        '一覧に存在かチェック
        If hdnCompCode.Value = "" OrElse txtProduct.Text = "" Then
            CommonFunctions.ShowMessage(C_MESSAGENO.PDFLISTEXISTS, Me.lblFooterMessage)
            Return
        Else
            For i As Integer = 0 To BASEtbl.Rows.Count - 1
                If hdnCompCode.Value = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) OrElse
                    txtProduct.Text = Convert.ToString(BASEtbl.Rows(i)("PRODUCTCODE")) Then
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
            If DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_DELFLG"), TextBox).Text = BaseDllCommon.CONST_FLAG_YES Then
                Try
                    System.IO.File.Delete(DirectCast(WF_DViewRepPDF.Items(i).FindControl("WF_Rep_FILEPATH"), Label).Text)
                Catch ex As Exception
                End Try
            End If
        Next

        'ファイルコピー

        For i As Integer = 1 To WF_Rep2_PDFselect.Items.Count

            'Update_Hフォルダクリア処理
            WW_Dir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT\"
            WW_Dir = WW_Dir & "MSDS\" & txtProduct.Text & "\Update_H"

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
            WW_Dir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT\"
            WW_Dir = WW_Dir & "MSDS\" & txtProduct.Text

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
            WW_Dir = COA0019Session.USERTEMPDir & "\" & COA0019Session.USERID & "\GBM00008PRODUCT\"
            WW_Dir = WW_Dir & "MSDS\" & txtProduct.Text & "\Update_D"
            For Each tempFile As String In System.IO.Directory.GetFiles(WW_Dir, "*", System.IO.SearchOption.AllDirectories)
                Try
                    System.IO.File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next

        Next

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
    ''' 国連番号有効判定
    ''' </summary>
    Protected Sub JudUNNO(ByVal UNNO As String, ByVal HAZARDCLASS As String, ByVal PACKINGGROUP As String, Optional ByRef chk As String = "")
        Dim ENABLED As String = ""

        Try

            If UNNO <> "" AndAlso HAZARDCLASS <> "" AndAlso PACKINGGROUP <> "" Then

                Dim unnoVal As String = UNNO
                Dim splUnnoVal As String() = Split(unnoVal, ",", -1, CompareMethod.Text)
                Dim hazclsVal As String = HAZARDCLASS
                Dim splHazclsVal As String() = Split(hazclsVal, ",", -1, CompareMethod.Text)
                Dim pacgrpVal As String = PACKINGGROUP
                Dim splPacgrpVal As String() = Split(pacgrpVal, ",", -1, CompareMethod.Text)

                For i As Integer = 0 To splUnnoVal.Count - 1
                    For j As Integer = 0 To splHazclsVal.Count - 1
                        For k As Integer = 0 To splPacgrpVal.Count - 1

                            GetEnabled(splUnnoVal(i), splHazclsVal(j), splPacgrpVal(k), ENABLED)

                            If ENABLED = CONST_FLAG_NO Then
                                Exit For
                            End If
                        Next
                        If ENABLED = CONST_FLAG_NO Then
                            Exit For
                        End If
                    Next
                    If ENABLED = CONST_FLAG_NO Then
                        Exit For
                    End If
                Next

                If chk <> "" Then
                    chk = ENABLED
                Else

                    If ENABLED = BaseDllCommon.CONST_FLAG_NO Then

                        Dim dummyMsgBox As Label = New Label
                        'メッセージ取得
                        CommonFunctions.ShowMessage(C_MESSAGENO.INVALIDUNNO, dummyMsgBox)

                        txtWarMsg.Text = dummyMsgBox.Text.Trim

                    Else
                        txtWarMsg.Text = ""
                    End If

                    'Enable初期設定
                    For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                        'ENABLED設定
                        If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "ENABLED" Then

                            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = "" Then

                                If txtWarMsg.Text = "" Then

                                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = BaseDllCommon.CONST_FLAG_YES
                                Else

                                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = BaseDllCommon.CONST_FLAG_NO

                                End If
                                ENABLED_Change()
                            End If
                        End If
                    Next
                End If
            Else
                txtWarMsg.Text = ""
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
        If TypeOf Page.PreviousPage Is GBM00008SELECT Then
            '検索画面の場合
            Dim prevObj As GBM00008SELECT = DirectCast(Page.PreviousPage, GBM00008SELECT)

            Me.hdnSelectedStYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtStYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            Me.hdnSelectedEndYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtEndYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            If hdnSelectedEndYMD.Value = "" Then
                hdnSelectedEndYMD.Value = hdnSelectedStYMD.Value
            End If

            'Me.hdnSelectedCountryCode.Value = DirectCast(prevObj.FindControl("txtCountry"), TextBox).Text

            'Me.hdnSelectedCustomerCode.Value = DirectCast(prevObj.FindControl("txtShipper"), TextBox).Text

            Me.hdnSelectedProductCode.Value = DirectCast(prevObj.FindControl("txtProduct"), TextBox).Text

            Me.hdnUnNo.Value = DirectCast(prevObj.FindControl("txtUNNO"), TextBox).Text

            Me.hdnEnabled.Value = DirectCast(prevObj.FindControl("txtEnabled"), TextBox).Text

            Me.hdnViewId.Value = DirectCast(prevObj.FindControl("lbRightList"), ListBox).SelectedValue

        ElseIf Page.PreviousPage Is Nothing Then

            Dim prevObj As GBM00000APPROVAL = DirectCast(Page.PreviousPage, GBM00000APPROVAL)

            'Me.hdnSelectedCompCode.Value = Convert.ToString(Request.Form("hdnSelectedValue1"))
            'Me.hdnSelectedCountryCode.Value = Convert.ToString(Request.Form("hdnSelectedValue2"))
            'Me.hdnSelectedCustomerCode.Value = Convert.ToString(Request.Form("hdnSelectedValue3"))
            'Me.hdnSelectedProductCode.Value = Convert.ToString(Request.Form("hdnSelectedValue4"))
            'Me.hdnSelectedStYMD.Value = Convert.ToString(Request.Form("hdnSelectedValue5"))
            'Me.hdnSelectedEndYMD.Value = Convert.ToString(Request.Form("hdnSelectedValue6"))

            Me.hdnSelectedApplyID.Value = Convert.ToString(Request.Form("hdnSelectedValue1"))
            Me.hdnSelectedStYMD.Value = FormatDateYMD(Convert.ToString(Request.Form("hdnSelectedValue2")), GBA00003UserSetting.DATEFORMAT)
            Me.hdnSelectedEndYMD.Value = FormatDateYMD(Convert.ToString(Request.Form("hdnSelectedValue3")), GBA00003UserSetting.DATEFORMAT)

            Me.hdnViewId.Value = "Default"

            Me.hdnMAPpermitCode.Value = "TRUE"

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
    ''' タンク*付与処理
    ''' </summary>
    Private Sub ChkTankAst(ByVal UNNO As String, ByVal HAZARDCLASS As String, ByVal PACKINGGROUP As String)

        Dim repName As Label = Nothing

        For i As Integer = 0 To WF_DViewRep1.Items.Count - 1
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "TANKGRADE" Then
                repName = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELDNM_2"), Label)
            End If
        Next

        If Not repName Is Nothing Then

            'If (UNNO <> NONDG AndAlso UNNO <> "") OrElse (HAZARDCLASS <> NONDG AndAlso HAZARDCLASS <> "") OrElse (PACKINGGROUP <> NONDG AndAlso PACKINGGROUP <> "") Then
            If UNNO <> "" OrElse HAZARDCLASS <> "" OrElse PACKINGGROUP <> "" Then
                repName.CssClass = "requiredMark2"
            Else
                repName.CssClass = ""
            End If
        End If

    End Sub

    ''' <summary>
    ''' オペレーションリストアイテムを設定
    ''' </summary>
    Private Sub SetCompanyCode()
        Dim comp As ListBox = New ListBox

        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "COMPANY"
        COA0017FixValue.LISTBOX1 = comp
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            comp = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            hdnCompCode.Value = comp.Items(0).Text

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
    ''' 国連番号有効取得
    ''' </summary>
    Protected Sub GetEnabled(ByVal UNNO As String, ByVal HAZARDCLASS As String, ByVal PACKINGGROUP As String, ByRef enabled As String)

        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        Try
            'DataBase接続(Open)
            SQLcon.Open()

            '検索SQL文
            SQLStr =
                "SELECT ENABLED " _
            & " FROM  GBM0007_UNNO " _
            & " Where UNNO          = @P1 " _
            & "   and HAZARDCLASS   = @P2 " _
            & "   and PACKINGGROUP  = @P3 " _
            & "   and STYMD        <= @P4 " _
            & "   and ENDYMD       >= @P5 " _
            & "   and DELFLG       <> @P6 "

            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.VarChar)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.VarChar)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.VarChar)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.Date)
            Dim PARA5 As SqlParameter = SQLcmd.Parameters.Add("@P5", System.Data.SqlDbType.Date)
            Dim PARA6 As SqlParameter = SQLcmd.Parameters.Add("@P6", System.Data.SqlDbType.VarChar)

            PARA1.Value = UNNO
            PARA2.Value = HAZARDCLASS
            PARA3.Value = PACKINGGROUP
            PARA4.Value = Date.Now
            PARA5.Value = Date.Now
            PARA6.Value = BaseDllCommon.CONST_FLAG_YES

            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                enabled = Convert.ToString(SQLdr("ENABLED"))
            End While

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
    ''' 頭文字チェック
    ''' </summary>
    Private Function HeadCheck(ByVal chkVal As String, ByVal initialVal As String) As Boolean

        If chkVal <> "" AndAlso initialVal <> "" Then

            If chkVal.Length <= initialVal.Length Then
                Return False

            Else

                If Not chkVal.Substring(0, initialVal.Length) = initialVal Then

                    Return False

                End If

            End If

        End If

        Return True
    End Function

    ''' <summary>
    ''' TANKGRADE個別チェック
    ''' </summary>
    Private Function TankGradeChk(ByVal chkVal As String) As Boolean

        If chkVal <> "" Then

            If Not HeadCheck(chkVal, TANKINIT) Then
                Return False
            End If

            If Not IsNumeric(chkVal.Substring(TANKINIT.Length)) Then
                Return False
            End If
        End If

        Return True
    End Function

    ''' <summary>
    ''' PRPVISIONS個別チェック
    ''' </summary>
    Private Function PrpvisionsChk(ByVal chkVal As String) As Boolean

        If chkVal.Replace(",", "") <> "" Then

            Dim splChkVal As String() = Split(chkVal, ",", -1, CompareMethod.Text)

            For i As Integer = 0 To splChkVal.Count - 1

                If Not HeadCheck(splChkVal(i), PRPVINIT) Then
                    Return False
                End If

                If Not IsNumeric(splChkVal(i).Substring(PRPVINIT.Length)) Then
                    Return False
                End If
            Next

        End If

        Return True
    End Function

    ''' <summary>
    ''' 追加規定名設定
    ''' </summary>
    Public Sub PRPVISIONS_Change()

        Try

            Dim prv As String = ""

            'リピーター追加規定
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), Label).Text = "PRPVISIONS" Then

                    prv = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text

                    If prv.Replace(",", "") <> "" Then

                        For j As Integer = 0 To prv.Length

                            If prv.Contains(",,") Then
                                prv = prv.Replace(",,", ",")
                            End If
                            DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = prv
                        Next

                    Else
                        DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), TextBox).Text = prv.Replace(",", "")

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

End Class