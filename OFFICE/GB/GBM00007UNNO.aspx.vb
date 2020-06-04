Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net
Imports BASEDLL
''' <summary>
''' 国連番号マスタ画面クラス
''' </summary>
Public Class GBM00007UNNO
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00007"    '自身のMAPID
    Private Const CONST_BASEDATATABLE = "GBM00007TBL"
    Private Const CONST_INPDATATABLE = "GBM00007INPTBL"
    Private Const CONST_UPDDATATABLE = "GBM00007UPDTBL"
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_TBLMASTER = "GBM0007_UNNO"
    Private Const CONST_TBLAPPLY = "GBM0012_UNNOAPPLY"
    Private Const CONST_EVENTCODE = "MasterApplyUnNo"

    Private errListAll As List(Of String)               'インポート全体のエラー
    Private errList As List(Of String)                  'インポート中の１セット分のエラー
    Private returnCode As String = String.Empty         'サブ用リターンコード
    Private PDFrow As DataRow
    'Private opeStrUpdate As String                      'オペレーション（更新）文字列
    'Private opeStrError As String                       'オペレーション（エラー）文字列

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
            Dim COA0007getCompanyInfo As New COA0007CompanyInfo             '会社情報取得
            Dim COA0021ListTable As New COA0021ListTable
            Dim COA0031ProfMap As New COA0031ProfMap

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
                hdnXMLsaveFile.Value = COA0019Session.XMLDir & "\" & Date.Now.ToString("yyyyMMdd") & "-" & COA0019Session.USERID & "-" & CONST_MAPID & "-" & Me.hdnThisMapVariant.Value & "-" & Date.Now.ToString("HHmmss") & ".txt"
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
                '前画面情報設定
                '****************************************
                getPrevInfo()
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
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", returnCode)})
                    Return
                End If

                '一覧表示項目取得
                GetListData()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", returnCode)})
                    Return
                End If

                '一覧表示データ保存
                COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = BASEtbl
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If

                '一覧表示データ編集（性能対策）
                Dim COA0013TableObject As New COA0013TableObject
                Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(BASEtbl, Me.pnlListArea, CONST_DSPROWCOUNT, 1, hdnListPosition)

                With COA0013TableObject
                    .MAPID = CONST_MAPID
                    .VARI = hdnPrevViewID.Value
                    .SRCDATA = listData
                    .TBLOBJ = pnlListArea
                    .SCROLLTYPE = "2"
                    .TITLEOPT = True
                    .LEVENT = "ondblclick"
                    .LFUNC = "ListDbClick"
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
                ' スクロール処理 
                '**********************
                ListScrole()
                hdnMouseWheel.Value = ""
            End If

            '****************************************
            'Close処理
            '****************************************
            BASEtbl.Dispose()
            BASEtbl = Nothing
            INPtbl.Dispose()
            INPtbl = Nothing
            UPDtbl.Dispose()
            UPDtbl = Nothing

            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定

        Catch ex As System.Threading.ThreadAbortException
            Return
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.EXCEPTION)})

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
        COA0016VARIget.VARI = Me.hdnPrevViewID.Value
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
            COA0020ProfViewSort.VARI = hdnPrevViewID.Value
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
            & "       UNNO                               , " _
            & "       HAZARDCLASS                        , " _
            & "       PACKINGGROUP                       , " _
            & "       STYMD                              , " _
            & "       ENDYMD                             , " _
            & "       PRODUCTNAME                        , " _
            & "       PRODUCTNAME_EN                     , " _
            & "       NAME                               , " _
            & "       NAME_EN                            , " _
            & "       COMPATIBILITYGROUP                 , " _
            & "       SUBSIDIARYRISK                     , " _
            & "       LIMITEDQUANTITIES                  , " _
            & "       EXCEPTETQUANTITIES                 , " _
            & "       PKINSTRUCTIONS                     , " _
            & "       PKPROVISIONS                       , " _
            & "       LPKINSTRUCTIONS                    , " _
            & "       LPKPROVISIONS                      , " _
            & "       IBCINSTRUCTIONS                    , " _
            & "       IBCPROVISIONS                      , " _
            & "       TANKINSTRUCTIONS                   , " _
            & "       TANKPROVISIONS                     , " _
            & "       FLEXIBLE                           , " _
            & "       SPPROVISIONS                       , " _
            & "       LOADINGMETHOD                      , " _
            & "       SEGREGATION                        , " _
            & "       REMARK                             , " _
            & "       ENABLED                            , " _
            & "       DELFLG                             , " _
            & "       UPDYMD                             , " _
            & "       UPDUSER                            , " _
            & "       UPDTERMID                            " _
            & "  FROM (" _
            & "    SELECT " _
            & "       '' as APPLYID , " _
            & "       isnull(rtrim(UNNO),'')                     as UNNO , " _
            & "       isnull(rtrim(HAZARDCLASS),'')              as HAZARDCLASS , " _
            & "       isnull(rtrim(PACKINGGROUP),'')             as PACKINGGROUP , " _
            & "       isnull(convert(nvarchar, STYMD , 111),'')  as STYMD , " _
            & "       isnull(convert(nvarchar, ENDYMD , 111),'') as ENDYMD , " _
            & "       isnull(rtrim(PRODUCTNAME),'')              as PRODUCTNAME , " _
            & "       isnull(rtrim(PRODUCTNAME_EN),'')           as PRODUCTNAME_EN , " _
            & "       isnull(rtrim(NAME),'')                     as NAME , " _
            & "       isnull(rtrim(NAME_EN),'')                  as NAME_EN , " _
            & "       isnull(rtrim(COMPATIBILITYGROUP),'')       as COMPATIBILITYGROUP , " _
            & "       isnull(rtrim(SUBSIDIARYRISK),'')           as SUBSIDIARYRISK , " _
            & "       isnull(rtrim(LIMITEDQUANTITIES),'')        as LIMITEDQUANTITIES , " _
            & "       isnull(rtrim(EXCEPTETQUANTITIES),'')       as EXCEPTETQUANTITIES , " _
            & "       isnull(rtrim(PKINSTRUCTIONS),'')           as PKINSTRUCTIONS , " _
            & "       isnull(rtrim(PKPROVISIONS),'')             as PKPROVISIONS , " _
            & "       isnull(rtrim(LPKINSTRUCTIONS),'')          as LPKINSTRUCTIONS , " _
            & "       isnull(rtrim(LPKPROVISIONS),'')            as LPKPROVISIONS , " _
            & "       isnull(rtrim(IBCINSTRUCTIONS),'')          as IBCINSTRUCTIONS , " _
            & "       isnull(rtrim(IBCPROVISIONS),'')            as IBCPROVISIONS , " _
            & "       isnull(rtrim(TANKINSTRUCTIONS),'')         as TANKINSTRUCTIONS , " _
            & "       isnull(rtrim(TANKPROVISIONS),'')           as TANKPROVISIONS , " _
            & "       isnull(rtrim(FLEXIBLE),'')                 as FLEXIBLE , " _
            & "       isnull(rtrim(SPPROVISIONS),'')             as SPPROVISIONS , " _
            & "       isnull(rtrim(LOADINGMETHOD),'')            as LOADINGMETHOD , " _
            & "       isnull(rtrim(SEGREGATION),'')              as SEGREGATION , " _
            & "       isnull(rtrim(REMARK),'')                   as REMARK , " _
            & "       isnull(rtrim(ENABLED),'')                  as ENABLED , " _
            & "       isnull(rtrim(DELFLG),'')                   as DELFLG , " _
            & "       isnull(convert(nvarchar, UPDYMD , 120),'') as UPDYMD , " _
            & "       isnull(rtrim(UPDUSER),'')                  as UPDUSER , " _
            & "       isnull(rtrim(UPDTERMID),'')                as UPDTERMID , " _
            & "       TIMSTP = cast(UPDTIMSTP                    as bigint) " _
            & "    FROM " & CONST_TBLMASTER & " as tbl1 " _
            & "    WHERE DELFLG    <> @P4 " _
            & "    AND   STYMD     <= @P1 " _
            & "    AND   ENDYMD    >= @P2 " _
            & "    AND   NOT EXISTS( "
            '承認画面から遷移の場合
            If Page.PreviousPage Is Nothing Then
                SQLStr &= " SELECT * FROM " & CONST_TBLAPPLY & " as tbl2 " _
                   & " WHERE tbl2.APPLYID = @P3 "
            Else
                SQLStr &= "                      SELECT * FROM " & CONST_TBLAPPLY & " as tbl2 " _
                    & "                      WHERE tbl1.UNNO = tbl2.UNNO " _
                    & "                      AND   tbl1.HAZARDCLASS = tbl2.HAZARDCLASS " _
                    & "                      AND   tbl1.PACKINGGROUP = tbl2.PACKINGGROUP " _
                    & "                      AND   tbl1.STYMD = tbl2.STYMD " _
                    & "                      AND   tbl1.DELFLG <> @P4 " _
                    & "                      AND   tbl2.DELFLG <> @P4 "
            End If
            SQLStr &= "                    )" _
            & "    UNION ALL " _
            & "    SELECT " _
            & "       isnull(rtrim(APPLYID),'')                  as APPLYID , " _
            & "       isnull(rtrim(UNNO),'')                     as UNNO , " _
            & "       isnull(rtrim(HAZARDCLASS),'')              as HAZARDCLASS , " _
            & "       isnull(rtrim(PACKINGGROUP),'')             as PACKINGGROUP , " _
            & "       isnull(convert(nvarchar, STYMD , 111),'')  as STYMD , " _
            & "       isnull(convert(nvarchar, ENDYMD , 111),'') as ENDYMD , " _
            & "       isnull(rtrim(PRODUCTNAME),'')              as PRODUCTNAME , " _
            & "       isnull(rtrim(PRODUCTNAME_EN),'')           as PRODUCTNAME_EN , " _
            & "       isnull(rtrim(NAME),'')                     as NAME , " _
            & "       isnull(rtrim(NAME_EN),'')                  as NAME_EN , " _
            & "       isnull(rtrim(COMPATIBILITYGROUP),'')       as COMPATIBILITYGROUP , " _
            & "       isnull(rtrim(SUBSIDIARYRISK),'')           as SUBSIDIARYRISK , " _
            & "       isnull(rtrim(LIMITEDQUANTITIES),'')        as LIMITEDQUANTITIES , " _
            & "       isnull(rtrim(EXCEPTETQUANTITIES),'')       as EXCEPTETQUANTITIES , " _
            & "       isnull(rtrim(PKINSTRUCTIONS),'')           as PKINSTRUCTIONS , " _
            & "       isnull(rtrim(PKPROVISIONS),'')             as PKPROVISIONS , " _
            & "       isnull(rtrim(LPKINSTRUCTIONS),'')          as LPKINSTRUCTIONS , " _
            & "       isnull(rtrim(LPKPROVISIONS),'')            as LPKPROVISIONS , " _
            & "       isnull(rtrim(IBCINSTRUCTIONS),'')          as IBCINSTRUCTIONS , " _
            & "       isnull(rtrim(IBCPROVISIONS),'')            as IBCPROVISIONS , " _
            & "       isnull(rtrim(TANKINSTRUCTIONS),'')         as TANKINSTRUCTIONS , " _
            & "       isnull(rtrim(TANKPROVISIONS),'')           as TANKPROVISIONS , " _
            & "       isnull(rtrim(FLEXIBLE),'')                 as FLEXIBLE , " _
            & "       isnull(rtrim(SPPROVISIONS),'')             as SPPROVISIONS , " _
            & "       isnull(rtrim(LOADINGMETHOD),'')            as LOADINGMETHOD , " _
            & "       isnull(rtrim(SEGREGATION),'')              as SEGREGATION , " _
            & "       isnull(rtrim(REMARK),'')                   as REMARK , " _
            & "       isnull(rtrim(ENABLED),'')                  as ENABLED , " _
            & "       isnull(rtrim(DELFLG),'')                   as DELFLG , " _
            & "       isnull(convert(nvarchar, UPDYMD , 120),'') as UPDYMD , " _
            & "       isnull(rtrim(UPDUSER),'')                  as UPDUSER , " _
            & "       isnull(rtrim(UPDTERMID),'')                as UPDTERMID , " _
            & "       TIMSTP = cast(UPDTIMSTP                    as bigint) " _
            & "    FROM  " & CONST_TBLAPPLY & " "
            '承認画面から遷移の場合
            If Page.PreviousPage Is Nothing Then
                SQLStr &= " WHERE APPLYID    = @P3 " _
                & " ) as tbl " _
                & " WHERE APPLYID    = @P3 "
            Else
                SQLStr &= "    WHERE DELFLG    <> @P4 " _
                & "    AND   STYMD     <= @P1 " _
                & "    AND   ENDYMD    >= @P2 " _
                & "  ) as tbl " _
                & "  WHERE DELFLG    <> @P4 " _
                & "  AND   STYMD     <= @P1 " _
                & "  AND   ENDYMD    >= @P2 "
            End If

            ' 条件指定で指定されたものでＳＱＬで可能なものを追加する

            If Page.PreviousPage Is Nothing Then
            Else

                '国連番号
                If (String.IsNullOrEmpty(hdnPrevCondUNNO.Value) = False) Then
                    SQLStr &= String.Format(" AND UNNO = '{0}' ", hdnPrevCondUNNO.Value)
                End If

                '等級
                If (String.IsNullOrEmpty(hdnPrevCondHazardClass.Value) = False) Then
                    SQLStr &= String.Format(" AND HAZARDCLASS = '{0}' ", hdnPrevCondHazardClass.Value)
                End If

                '容器等級
                If (String.IsNullOrEmpty(hdnPrevCondPackingGroup.Value) = False) Then
                    SQLStr &= String.Format(" AND PACKINGGROUP = '{0}' ", hdnPrevCondPackingGroup.Value)
                End If

                ''有効フラグ
                'If (String.IsNullOrEmpty(HttpContext.Current.Session(CONST_MAPID & "_VALIDFLG")) = False) Then
                '    SQLStr &= String.Format(" AND VALIDFLG = '{0}' ", HttpContext.Current.Session(CONST_MAPID & "_VALIDFLG"))
                'End If

            End If

            SQLStr &= " ORDER BY " & COA0020ProfViewSort.SORTSTR

            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
            Dim PARA4 As SqlParameter = SQLcmd.Parameters.Add("@P4", System.Data.SqlDbType.NVarChar)
            PARA1.Value = hdnPrevCondEndYMD.Value
            PARA2.Value = hdnPrevCondStYMD.Value
            'SQLcmd.CommandTimeout = 300
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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

            Dim findInt As Integer = 99999
            Dim stYMDStr As String = ""

            For j As Integer = 5 To INPtbl.Columns.Count - 1

                ' カラム名設定
                Dim workColumn = INPtbl.Columns.Item(j).ColumnName
                '　カラム未定義、値が未設定の場合は空文字を設定
                If Not (COA0029XlsTable.TBLDATA.Columns.Contains(workColumn)) OrElse
                   IsDBNull(COA0029XlsTable.TBLDATA.Rows(i)(workColumn)) Then
                    INProwWork(workColumn) = ""
                Else
                    INProwWork(workColumn) = COA0029XlsTable.TBLDATA.Rows(i)(workColumn)

                    'カラム毎の編集が必要な場合、個別に設定
                    If workColumn = "APPLYID" Then
                        '申請IDは空白
                        INProwWork(workColumn) = ""
                    End If
                    If workColumn = "STYMD" Then
                        If IsDate(INProwWork(workColumn)) Then
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString("yyyy/MM/dd")
                            'Else
                            '    INProwWork(workColumn) = ""
                        End If
                    End If
                    If workColumn = "ENDYMD" Then
                        If IsDate(INProwWork(workColumn)) Then
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString("yyyy/MM/dd")
                            'Else
                            '    INProwWork(workColumn) = ""
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
                ''国連番号ビュー表示切替
                'Case Me.vLeftUnNo.ID
                '    SetUnNoListItem(Me.txtUnNo.Text)
                '等級ビュー表示切替
                Case Me.vLeftHazardClass.ID
                    SetHazardClassListItem(Me.txtHazardClass.Text)
                '容器等級ビュー表示切替
                Case Me.vLeftPackingGroup.ID
                    SetPackingGroupListItem(Me.txtPackingGroup.Text)
                'Enableビュー表示切替
                Case Me.vLeftEnabled.ID
                    SetEnabledListItem(Me.txtEnabled.Text)
                '削除フラグビュー表示切替
                Case Me.vLeftDelFlg.ID
                    SetDelFlgListItem(Me.txtDelFlg.Text)
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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

            '国連番号　前方一致
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtUnNoEx.Text <> "") Then
                Dim searchStr As String = Convert.ToString(BASEtbl.Rows(i)("UNNO"))

                If Not searchStr.StartsWith(txtUnNoEx.Text) Then
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If

            End If

            '等級　前方一致
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtHazardClassEx.Text <> "") Then
                Dim searchStr As String = Convert.ToString(BASEtbl.Rows(i)("HAZARDCLASS"))
                If Not searchStr.StartsWith(txtHazardClassEx.Text) Then
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If
            End If

            '容器等級　完全一致
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtPackingGroupEx.Text <> "") Then
                If Convert.ToString(BASEtbl.Rows(i)("PACKINGGROUP")) <> txtPackingGroupEx.Text Then
                    BASEtbl.Rows(i)("HIDDEN") = 1
                End If
            End If

        Next

        '画面先頭を表示
        hdnListPosition.Value = "1"

        '一覧表示データ保存
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            BASEtbl = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Try

            txtRightErrorMessage.Text = ""

            SQLcon.Open() 'DataBase接続(Open)

            'DB更新前チェック
            '  ※同一Key全てのレコードが更新されていない事をチェックする
            For i As Integer = 0 To BASEtbl.Rows.Count - 1

                'If BASEtbl.Rows(i)("OPERATION") = "更新" And BASEtbl.Rows(i)("TIMSTP") <> "0" Then
                If Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = hdnOpeStrUpdate.Value AndAlso Convert.ToString(BASEtbl.Rows(i)("TIMSTP")) <> "0" Then
                    '※追加レコードは、BASEtbl.Rows(i)("TIMSTP") = "0"となっている

                    Try

                        '同一Keyレコードを抽出
                        SQLStr = ""
                        SQLStr =
                               " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP, " _
                             & "   convert(nvarchar, UPDYMD , 120) as UPDYMD, " _
                             & "   rtrim(UPDUSER) as UPDUSER , rtrim(UPDTERMID) as UPDTERMID " _
                             & " FROM  " & CONST_TBLMASTER _
                             & " WHERE UNNO = @P01  " _
                             & "   And HAZARDCLASS = @P02 " _
                             & "   And PACKINGGROUP = @P03 " _
                             & "   And STYMD = @P04 " _
                             & "   And DELFLG <> @P05 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)

                        PARA01.Value = BASEtbl.Rows(i)("UNNO")
                        PARA02.Value = BASEtbl.Rows(i)("HAZARDCLASS")
                        PARA03.Value = BASEtbl.Rows(i)("PACKINGGROUP")
                        PARA04.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARA05.Value = BaseDllCommon.CONST_FLAG_YES

                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            'If RTrim(BASEtbl.Rows(i)("TIMSTP")) = SQLdr("TIMSTP") Then
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("UNNO")) = Convert.ToString(BASEtbl.Rows(i)("UNNO")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("HAZARDCLASS")) = Convert.ToString(BASEtbl.Rows(i)("HAZARDCLASS")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("PACKINGGROUP")) = Convert.ToString(BASEtbl.Rows(i)("PACKINGGROUP")) AndAlso
                                       RTrim(Convert.ToString(BASEtbl.Rows(j)("STYMD"))) = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD"))) Then

                                        'BASEtbl.Rows(j)("OPERATION") = "エラー"
                                        BASEtbl.Rows(j)("OPERATION") = hdnOpeStrError.Value

                                    End If
                                Next

                                'エラーレポート編集
                                Dim errMessageStr As String = ""
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr

                                'メッセージ取得
                                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, dummyMsgBox)

                                errMessageStr = "・" & dummyMsgBox.Text
                                errMessageStr = errMessageStr & ControlChars.NewLine
                                errMessageStr = errMessageStr & Me.ErrItemSet(BASEtbl.Rows(i))
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
                             & " FROM  " & CONST_TBLAPPLY _
                             & " WHERE UNNO = @P01  " _
                             & "   And HAZARDCLASS = @P02 " _
                             & "   And PACKINGGROUP = @P03 " _
                             & "   And STYMD = @P04 " _
                             & "   And DELFLG <> @P05 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARAM1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARAM2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARAM3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                        Dim PARAM4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.Date)
                        Dim PARAM5 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.NVarChar)

                        PARAM1.Value = BASEtbl.Rows(i)("UNNO")
                        PARAM2.Value = BASEtbl.Rows(i)("HAZARDCLASS")
                        PARAM3.Value = BASEtbl.Rows(i)("PACKINGGROUP")
                        PARAM4.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARAM5.Value = BaseDllCommon.CONST_FLAG_YES

                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("UNNO")) = Convert.ToString(BASEtbl.Rows(i)("UNNO")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("HAZARDCLASS")) = Convert.ToString(BASEtbl.Rows(i)("HAZARDCLASS")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("PACKINGGROUP")) = Convert.ToString(BASEtbl.Rows(i)("PACKINGGROUP")) AndAlso
                                       RTrim(Convert.ToString(BASEtbl.Rows(j)("STYMD"))) = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD"))) Then

                                        BASEtbl.Rows(j)("OPERATION") = hdnOpeStrError.Value

                                    End If
                                Next

                                'エラーレポート編集
                                Dim errMessageStr As String = ""
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr

                                'メッセージ取得
                                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, dummyMsgBox)

                                errMessageStr = "・" & dummyMsgBox.Text
                                errMessageStr = errMessageStr & ControlChars.NewLine
                                errMessageStr = errMessageStr & Me.ErrItemSet(BASEtbl.Rows(i))
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

                    'If BASEtbl.Rows(i)("OPERATION") = "更新" Or BASEtbl.Rows(i)("OPERATION") = "★更新" Then
                    If Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = hdnOpeStrUpdate.Value OrElse Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = "★" & hdnOpeStrUpdate.Value Then

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
                                     & "  WHERE UNNO = @P02 " _
                                     & "    AND HAZARDCLASS = @P03  " _
                                     & "    AND PACKINGGROUP = @P04  " _
                                     & "    AND STYMD = @P05 ;  " _
                                     & " OPEN timestamp ;  " _
                                     & " FETCH NEXT FROM timestamp INTO @timestamp ;  " _
                                     & " IF ( @@FETCH_STATUS = 0 ) " _
                                     & "  UPDATE " & updTable _
                                     & "  SET "
                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID = @P01 , "
                        End If
                        SQLStr = SQLStr & " ENDYMD = @P06 , " _
                                     & "        PRODUCTNAME = @P32 , " _
                                     & "        PRODUCTNAME_EN = @P33 , " _
                                     & "        NAME = @P07 , " _
                                     & "        NAME_EN = @P08 , " _
                                     & "        COMPATIBILITYGROUP = @P09 , " _
                                     & "        SUBSIDIARYRISK = @P10 , " _
                                     & "        LIMITEDQUANTITIES = @P11 , " _
                                     & "        EXCEPTETQUANTITIES = @P12 , " _
                                     & "        PKINSTRUCTIONS = @P13 , " _
                                     & "        PKPROVISIONS = @P14 , " _
                                     & "        LPKINSTRUCTIONS = @P15 , " _
                                     & "        LPKPROVISIONS = @P16 , " _
                                     & "        IBCINSTRUCTIONS = @P17 , " _
                                     & "        IBCPROVISIONS = @P18 , " _
                                     & "        TANKINSTRUCTIONS = @P19 , " _
                                     & "        TANKPROVISIONS = @P20 , " _
                                     & "        FLEXIBLE = @P21 , " _
                                     & "        SPPROVISIONS = @P22 , " _
                                     & "        LOADINGMETHOD = @P23 , " _
                                     & "        SEGREGATION = @P24 , " _
                                     & "        REMARK = @P25 , " _
                                     & "        ENABLED = @P26 , " _
                                     & "        DELFLG = @P27 , " _
                                     & "        INITYMD = @P28 , " _
                                     & "        UPDYMD = @P28 , " _
                                     & "        UPDUSER = @P29 , " _
                                     & "        UPDTERMID = @P30 , " _
                                     & "        RECEIVEYMD = @P31  " _
                                     & "  WHERE UNNO = @P02 " _
                                     & "    AND HAZARDCLASS = @P03 " _
                                     & "    AND PACKINGGROUP = @P04 " _
                                     & "    AND STYMD = @P05 ; " _
                                     & " IF ( @@FETCH_STATUS <> 0 ) " _
                                     & "  INSERT INTO  " & updTable _
                                     & "       ( "
                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID , "
                        End If
                        SQLStr = SQLStr & " UNNO , " _
                                     & "        HAZARDCLASS , " _
                                     & "        PACKINGGROUP , " _
                                     & "        STYMD , " _
                                     & "        ENDYMD , " _
                                     & "        PRODUCTNAME , " _
                                     & "        PRODUCTNAME_EN , " _
                                     & "        NAME , " _
                                     & "        NAME_EN , " _
                                     & "        COMPATIBILITYGROUP , " _
                                     & "        SUBSIDIARYRISK , " _
                                     & "        LIMITEDQUANTITIES , " _
                                     & "        EXCEPTETQUANTITIES , " _
                                     & "        PKINSTRUCTIONS , " _
                                     & "        PKPROVISIONS , " _
                                     & "        LPKINSTRUCTIONS , " _
                                     & "        LPKPROVISIONS , " _
                                     & "        IBCINSTRUCTIONS , " _
                                     & "        IBCPROVISIONS , " _
                                     & "        TANKINSTRUCTIONS , " _
                                     & "        TANKPROVISIONS , " _
                                     & "        FLEXIBLE , " _
                                     & "        SPPROVISIONS , " _
                                     & "        LOADINGMETHOD , " _
                                     & "        SEGREGATION , " _
                                     & "        REMARK , " _
                                     & "        ENABLED , " _
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
                        SQLStr = SQLStr & "         @P02,@P03,@P04,@P05,@P06,@P32,@P33,@P07,@P08,@P09,@P10, " _
                                     & "           @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20, " _
                                     & "           @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P28,@P29, " _
                                     & "           @P30,@P31); " _
                                     & " CLOSE timestamp ; " _
                                     & " DEALLOCATE timestamp ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)
                        Dim PARA05 As SqlParameter = SQLcmd.Parameters.Add("@P05", System.Data.SqlDbType.Date)
                        Dim PARA06 As SqlParameter = SQLcmd.Parameters.Add("@P06", System.Data.SqlDbType.Date)
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
                        Dim PARA22 As SqlParameter = SQLcmd.Parameters.Add("@P22", System.Data.SqlDbType.NVarChar)
                        Dim PARA23 As SqlParameter = SQLcmd.Parameters.Add("@P23", System.Data.SqlDbType.NVarChar)
                        Dim PARA24 As SqlParameter = SQLcmd.Parameters.Add("@P24", System.Data.SqlDbType.NVarChar)
                        Dim PARA25 As SqlParameter = SQLcmd.Parameters.Add("@P25", System.Data.SqlDbType.NVarChar)
                        Dim PARA26 As SqlParameter = SQLcmd.Parameters.Add("@P26", System.Data.SqlDbType.NVarChar)
                        Dim PARA27 As SqlParameter = SQLcmd.Parameters.Add("@P27", System.Data.SqlDbType.NVarChar)
                        Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.DateTime)
                        Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.NVarChar)
                        Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.NVarChar)
                        Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.DateTime)
                        Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.NVarChar)
                        Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.NVarChar)

                        PARA01.Value = BASEtbl.Rows(i)("APPLYID")
                        PARA02.Value = BASEtbl.Rows(i)("UNNO")
                        PARA03.Value = BASEtbl.Rows(i)("HAZARDCLASS")
                        PARA04.Value = BASEtbl.Rows(i)("PACKINGGROUP")
                        PARA05.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARA06.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("ENDYMD")))
                        PARA07.Value = BASEtbl.Rows(i)("NAME")
                        PARA08.Value = BASEtbl.Rows(i)("NAME_EN")
                        PARA09.Value = BASEtbl.Rows(i)("COMPATIBILITYGROUP")
                        PARA10.Value = BASEtbl.Rows(i)("SUBSIDIARYRISK")
                        PARA11.Value = BASEtbl.Rows(i)("LIMITEDQUANTITIES")
                        PARA12.Value = BASEtbl.Rows(i)("EXCEPTETQUANTITIES")
                        PARA13.Value = BASEtbl.Rows(i)("PKINSTRUCTIONS")
                        PARA14.Value = BASEtbl.Rows(i)("PKPROVISIONS")
                        PARA15.Value = BASEtbl.Rows(i)("LPKINSTRUCTIONS")
                        PARA16.Value = BASEtbl.Rows(i)("LPKPROVISIONS")
                        PARA17.Value = BASEtbl.Rows(i)("IBCINSTRUCTIONS")
                        PARA18.Value = BASEtbl.Rows(i)("IBCPROVISIONS")
                        PARA19.Value = BASEtbl.Rows(i)("TANKINSTRUCTIONS")
                        PARA20.Value = BASEtbl.Rows(i)("TANKPROVISIONS")
                        PARA21.Value = BASEtbl.Rows(i)("FLEXIBLE")
                        PARA22.Value = BASEtbl.Rows(i)("SPPROVISIONS")
                        PARA23.Value = BASEtbl.Rows(i)("LOADINGMETHOD")
                        PARA24.Value = BASEtbl.Rows(i)("SEGREGATION")
                        PARA25.Value = BASEtbl.Rows(i)("REMARK")
                        PARA26.Value = BASEtbl.Rows(i)("ENABLED")
                        PARA27.Value = BASEtbl.Rows(i)("DELFLG")
                        PARA28.Value = nowDate
                        PARA29.Value = COA0019Session.USERID
                        PARA30.Value = COA0019Session.APSRVname
                        PARA31.Value = CONST_DEFAULT_RECEIVEYMD
                        PARA32.Value = BASEtbl.Rows(i)("PRODUCTNAME")
                        PARA33.Value = BASEtbl.Rows(i)("PRODUCTNAME_EN")

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
                        copyDataTable.Columns.Remove("Select")
                        copyDataTable.Columns.Remove("HIDDEN")
                        copyDataTable.Columns.Remove("TIMSTP")

                        COA0030Journal.ROW = copyDataTable.Rows(0)
                        COA0030Journal.COA0030SaveJournal()
                        If COA0030Journal.ERR = C_MESSAGENO.NORMAL Then
                        Else
                            CommonFunctions.ShowMessage(COA0030Journal.ERR, Me.lblFooterMessage, pageObject:=Me)
                            Return
                        End If

                        '更新結果(TIMSTP)再取得 …　連続処理を可能にする。
                        'SQLStr2 = " Select CAST(UPDTIMSTP As bigint) As TIMSTP " _
                        SQLStr2 = " SELECT CAST(UPDTIMSTP as bigint) as TIMSTP, " _
                                     & "   convert(nvarchar, UPDYMD , 120) as UPDYMD, " _
                                     & "   rtrim(UPDUSER) as UPDUSER , rtrim(UPDTERMID) as UPDTERMID " _
                                     & " FROM " & CONST_TBLMASTER _
                                     & " WHERE UNNO = @P01 " _
                                     & "   And HAZARDCLASS = @P02 " _
                                     & "   And PACKINGGROUP = @P03 " _
                                     & "   And STYMD = @P04 ;"

                        SQLcmd2 = New SqlCommand(SQLStr2, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd2.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA2 As SqlParameter = SQLcmd2.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA3 As SqlParameter = SQLcmd2.Parameters.Add("@P03", System.Data.SqlDbType.NVarChar)
                        Dim PARA4 As SqlParameter = SQLcmd2.Parameters.Add("@P04", System.Data.SqlDbType.Date)

                        PARA1.Value = BASEtbl.Rows(i)("UNNO")
                        PARA2.Value = BASEtbl.Rows(i)("HAZARDCLASS")
                        PARA3.Value = BASEtbl.Rows(i)("PACKINGGROUP")
                        PARA4.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))

                        SQLdr2 = SQLcmd2.ExecuteReader()

                        While SQLdr2.Read
                            BASEtbl.Rows(i)("UPDYMD") = SQLdr2("UPDYMD")
                            BASEtbl.Rows(i)("UPDUSER") = SQLdr2("UPDUSER")
                            BASEtbl.Rows(i)("UPDTERMID") = SQLdr2("UPDTERMID")
                            BASEtbl.Rows(i)("TIMSTP") = SQLdr2("TIMSTP")
                        End While

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
            CommonFunctions.ShowMessage(O_ERR, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", O_ERR)})

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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        Else
            CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0027ReportTable.ERR)})
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage)
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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

            If errList.Count = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMALLISTADDED, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR, pageObject:=Me)
            End If
        End If

        BASEtbl.Clear()
        INPtbl.Clear()
        UPDtbl.Clear()
        BASEtbl.Dispose()
        INPtbl.Dispose()
        UPDtbl.Dispose()

        'カーソル設定
        txtOperationEx.Focus()

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
        txtUnNo.Focus()

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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
        COA0013TableObject.VARI = hdnPrevViewID.Value
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = pnlListArea
        COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.LEVENT = "ondblclick"
        COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""
    End Sub
    ''' <summary>
    ''' 入力データチェック
    ''' </summary>
    Protected Sub INPtblCheck()

        Dim dummyMsgBox As Label = New Label

        'インターフェイス初期値設定
        returnCode = C_MESSAGENO.NORMAL

        '事前準備（キー重複レコード削除）
        For i As Integer = INPtbl.Rows.Count - 1 To 1 Step -1
            'KEY重複
            'If INPtbl.Rows(i)("APPLYID") = INPtbl.Rows(i - 1)("APPLYID") AndAlso
            If Convert.ToString(INPtbl.Rows(i)("UNNO")) = Convert.ToString(INPtbl.Rows(i - 1)("UNNO")) AndAlso
               Convert.ToString(INPtbl.Rows(i)("HAZARDCLASS")) = Convert.ToString(INPtbl.Rows(i - 1)("HAZARDCLASS")) AndAlso
               Convert.ToString(INPtbl.Rows(i)("PACKINGGROUP")) = Convert.ToString(INPtbl.Rows(i - 1)("PACKINGGROUP")) AndAlso
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
            'Date.TryParseExact(Convert.ToString(workInpRow("STYMD")), GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, inpDateStart)
            'Date.TryParseExact(Convert.ToString(workInpRow("ENDYMD")), GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, inpDateEnd)
            ' メッセージ取得
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
                Dim errMessageStr As String = ""
                errMessageStr = "・" & errorMessage
                ' レコード内容を展開する
                errMessageStr = errMessageStr & Me.ErrItemSet(workInpRow)
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr
            Else
                '主キー情報重複チェック(一覧表示内容とのチェック)
                For j As Integer = 0 To BASEtbl.Rows.Count - 1
                    If Convert.ToString(BASEtbl.Rows(j)("DELFLG")) <> BaseDllCommon.CONST_FLAG_YES Then
                        '日付以外の項目が等しい
                        'If BASEtbl.Rows(j)("APPLYID") = workInpRow("APPLYID") AndAlso
                        If Convert.ToString(BASEtbl.Rows(j)("UNNO")) = Convert.ToString(workInpRow("UNNO")) AndAlso
                           Convert.ToString(BASEtbl.Rows(j)("HAZARDCLASS")) = Convert.ToString(workInpRow("HAZARDCLASS")) AndAlso
                           Convert.ToString(BASEtbl.Rows(j)("PACKINGGROUP")) = Convert.ToString(workInpRow("PACKINGGROUP")) Then

                            'ENDYMDは変更扱い
                            If Convert.ToString(BASEtbl.Rows(j)("STYMD")) = Convert.ToString(workInpRow("STYMD")) Then
                                '同一レコード
                                Exit For
                            Else

                                Dim baseDateStart As Date
                                Dim baseDateEnd As Date
                                Date.TryParse(Convert.ToString(BASEtbl.Rows(j)("STYMD")), baseDateStart)
                                Date.TryParse(Convert.ToString(BASEtbl.Rows(j)("ENDYMD")), baseDateEnd)

                                ' 日付相関チェック
                                If inpDateStart <= baseDateStart AndAlso baseDateStart <= inpDateEnd OrElse
                                   inpDateStart <= baseDateEnd AndAlso baseDateEnd <= inpDateEnd OrElse
                                   baseDateStart <= inpDateStart AndAlso inpDateStart <= baseDateEnd OrElse
                                   baseDateStart <= inpDateEnd AndAlso inpDateEnd <= baseDateEnd Then
                                    If returnCode = C_MESSAGENO.NORMAL Then
                                        'KEY重複
                                        returnCode = C_MESSAGENO.RIGHTBIXOUT
                                    End If

                                    'エラーレポート編集
                                    Dim errMessageStr As String = ""
                                    errMessageStr = "・" & errorMessage
                                    ' レコード内容を展開する
                                    errMessageStr = errMessageStr & Me.ErrItemSet(workInpRow)
                                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr
                                    Exit For
                                End If
                            End If
                        End If
                    End If
                Next
            End If
            If returnCode <> C_MESSAGENO.NORMAL Then
                'workInpRow("OPERATION") = "エラー"
                workInpRow("OPERATION") = hdnOpeStrError.Value
                errListAll.Add(C_MESSAGENO.RIGHTBIXOUT)
                errList.Add(C_MESSAGENO.RIGHTBIXOUT)
                If returnCode = C_MESSAGENO.REQUIREDVALUE OrElse returnCode = C_MESSAGENO.HASAPPLYINGRECORD Then ' 一覧反映対象外
                    workInpRow("HIDDEN") = "1"
                    returnCode = C_MESSAGENO.RIGHTBIXOUT
                End If
            End If
            INPtbl.Rows(i).ItemArray = workInpRow.ItemArray
        Next

    End Sub
    ''' <summary>
    ''' エラーキー情報出力
    ''' </summary>
    ''' <param name="argRow"></param>
    ''' <returns></returns>
    Private Function ErrItemSet(ByVal argRow As DataRow) As String
        Dim rtc As String = String.Empty

        If (COA0019Session.LANGDISP <> C_LANG.JA) Then
            rtc &= ControlChars.NewLine & "  --> UN No.          =" & Convert.ToString(argRow("UNNO")) & " , "
            rtc &= ControlChars.NewLine & "  --> CLASS           =" & Convert.ToString(argRow("HAZARDCLASS")) & " , "
            rtc &= ControlChars.NewLine & "  --> PG              =" & Convert.ToString(argRow("PACKINGGROUP")) & " , "
            rtc &= ControlChars.NewLine & "  --> EFFECTIVE(FROM) =" & Convert.ToString(argRow("STYMD")) & " , "
            rtc &= ControlChars.NewLine & "  --> DELETE FLG      =" & Convert.ToString(argRow("DELFLG")) & " "
        Else
            rtc &= ControlChars.NewLine & "  --> 国連番号        =" & Convert.ToString(argRow("UNNO")) & " , "
            rtc &= ControlChars.NewLine & "  --> 等級            =" & Convert.ToString(argRow("HAZARDCLASS")) & " , "
            rtc &= ControlChars.NewLine & "  --> 容器等級        =" & Convert.ToString(argRow("PACKINGGROUP")) & " , "
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
                Case Me.vLeftUnNo.ID 'アクティブなビューが国連番号
                    '国連番号選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbUnNo.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbUnNo.SelectedItem.Value
                            Me.lblUnNoText.Text = Me.lbUnNo.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblUnNoText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftHazardClass.ID 'アクティブなビューが等級
                    '等級選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbHazardClass.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbHazardClass.SelectedItem.Value
                            Me.lblHazardClassText.Text = Me.lbHazardClass.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblHazardClassText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftPackingGroup.ID 'アクティブなビューが容器等級
                    '容器等級選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbPackingGroup.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbPackingGroup.SelectedItem.Value
                            Me.lblPackingGroupText.Text = Me.lbPackingGroup.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblPackingGroupText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftEnabled.ID 'アクティブなビューがENABLED
                    'ENABLED選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbEnabled.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbEnabled.SelectedItem.Value
                            Me.lblEnabledText.Text = Me.lbEnabled.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblEnabledText.Text = ""
                            txtobj.Focus()
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
                                '''''★★★★★　要確認　★★★★★★★★★★★★★★★
                                'DirectCast(WF_DViewRepPDF.Items(IntCnt).FindControl("WF_Rep_DELFLG"),
                                '          System.Web.UI.WebControls.TextBox).Text = Me.lbDelFlg.SelectedItem.Value
                                'WF_DViewRepPDF.Items(Integer.Parse(hdnTextDbClickField.Value)).FindControl("WF_Rep_DELFLG").Focus()
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
        AddLangSetting(dicDisplayText, Me.lblUnNoEx, "国連番号", "UN No.")
        AddLangSetting(dicDisplayText, Me.lblHazardClassEx, "等級", "Class")
        AddLangSetting(dicDisplayText, Me.lblPackingGroupEx, "容器等級", "PG")

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
        AddLangSetting(dicDisplayText, Me.lblUnNo, "国連番号", "UN No.")
        AddLangSetting(dicDisplayText, Me.lblHazardClass, "等級", "Class")
        AddLangSetting(dicDisplayText, Me.lblPackingGroup, "容器等級", "PG")
        AddLangSetting(dicDisplayText, Me.lblYMD, "有効年月日", "Effective Date")
        AddLangSetting(dicDisplayText, Me.lblEnabled, "有効", "Enabled")
        AddLangSetting(dicDisplayText, Me.lblDelFlg, "削除", "Delete")

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
            Me.lblDtabUnNo.Text = "UN No."
            Me.hdnOpeStrUpdate.Value = "UPDATE"
            Me.hdnOpeStrError.Value = "ERROR"
        Else
            Me.lblDtabUnNo.Text = "国連番号"
            Me.hdnOpeStrUpdate.Value = "更新"
            Me.hdnOpeStrError.Value = "エラー"
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

        '画面固有項目
        table.Columns.Add("APPLYID", GetType(String))
        table.Columns.Add("UNNO", GetType(String))
        table.Columns.Add("HAZARDCLASS", GetType(String))
        table.Columns.Add("PACKINGGROUP", GetType(String))
        table.Columns.Add("STYMD", GetType(String))
        table.Columns.Add("ENDYMD", GetType(String))
        table.Columns.Add("PRODUCTNAME", GetType(String))
        table.Columns.Add("PRODUCTNAME_EN", GetType(String))
        table.Columns.Add("NAME", GetType(String))
        table.Columns.Add("NAME_EN", GetType(String))
        table.Columns.Add("COMPATIBILITYGROUP", GetType(String))
        table.Columns.Add("SUBSIDIARYRISK", GetType(String))
        table.Columns.Add("LIMITEDQUANTITIES", GetType(String))
        table.Columns.Add("EXCEPTETQUANTITIES", GetType(String))
        table.Columns.Add("PKINSTRUCTIONS", GetType(String))
        table.Columns.Add("PKPROVISIONS", GetType(String))
        table.Columns.Add("LPKINSTRUCTIONS", GetType(String))
        table.Columns.Add("LPKPROVISIONS", GetType(String))
        table.Columns.Add("IBCINSTRUCTIONS", GetType(String))
        table.Columns.Add("IBCPROVISIONS", GetType(String))
        table.Columns.Add("TANKINSTRUCTIONS", GetType(String))
        table.Columns.Add("TANKPROVISIONS", GetType(String))
        table.Columns.Add("FLEXIBLE", GetType(String))
        table.Columns.Add("SPPROVISIONS", GetType(String))
        table.Columns.Add("LOADINGMETHOD", GetType(String))
        table.Columns.Add("SEGREGATION", GetType(String))
        table.Columns.Add("REMARK", GetType(String))
        table.Columns.Add("ENABLED", GetType(String))
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
        workRow("UNNO") = ""
        workRow("HAZARDCLASS") = ""
        workRow("PACKINGGROUP") = ""
        workRow("STYMD") = ""
        workRow("ENDYMD") = ""
        workRow("PRODUCTNAME") = ""
        workRow("PRODUCTNAME_EN") = ""
        workRow("NAME") = ""
        workRow("NAME_EN") = ""
        workRow("COMPATIBILITYGROUP") = ""
        workRow("SUBSIDIARYRISK") = ""
        workRow("LIMITEDQUANTITIES") = ""
        workRow("EXCEPTETQUANTITIES") = ""
        workRow("PKINSTRUCTIONS") = ""
        workRow("PKPROVISIONS") = ""
        workRow("LPKINSTRUCTIONS") = ""
        workRow("LPKPROVISIONS") = ""
        workRow("IBCINSTRUCTIONS") = ""
        workRow("IBCPROVISIONS") = ""
        workRow("TANKINSTRUCTIONS") = ""
        workRow("TANKPROVISIONS") = ""
        workRow("FLEXIBLE") = ""
        workRow("SPPROVISIONS") = ""
        workRow("LOADINGMETHOD") = ""
        workRow("SEGREGATION") = ""
        workRow("REMARK") = ""
        workRow("ENABLED") = ""
        workRow("DELFLG") = ""

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

        'Detail取り込み用テーブル作成
        COA0015ProfViewD.MAPID = CONST_MAPID
        COA0015ProfViewD.VARI = hdnPrevViewID.Value
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
            workRow("UNNO") = txtUnNo.Text
            workRow("HAZARDCLASS") = txtHazardClass.Text
            workRow("PACKINGGROUP") = txtPackingGroup.Text

            '日付変換用
            Dim stDate As Date = Nothing
            Dim endDate As Date = Nothing
            If Date.TryParseExact(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, stDate) Then
                workRow("STYMD") = stDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParseExact(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT.Replace("dd", "d").Replace("MM", "M"), Nothing, Nothing, stDate) Then
                workRow("STYMD") = stDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParse(txtStYMD.Text, stDate) Then
                workRow("STYMD") = stDate.ToString("yyyy/MM/dd")
            Else
                workRow("STYMD") = txtStYMD.Text
            End If

            If Date.TryParseExact(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, endDate) Then
                workRow("ENDYMD") = endDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParseExact(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT.Replace("dd", "d").Replace("MM", "M"), Nothing, Nothing, endDate) Then
                workRow("ENDYMD") = endDate.ToString("yyyy/MM/dd")
            ElseIf Date.TryParse(txtEndYMD.Text, endDate) Then
                workRow("ENDYMD") = endDate.ToString("yyyy/MM/dd")
            Else
                workRow("ENDYMD") = txtEndYMD.Text
            End If

            workRow("PRODUCTNAME") = ""
            workRow("PRODUCTNAME_EN") = ""
            workRow("NAME") = ""
            workRow("NAME_EN") = ""
            workRow("COMPATIBILITYGROUP") = ""
            workRow("SUBSIDIARYRISK") = ""
            workRow("LIMITEDQUANTITIES") = ""
            workRow("EXCEPTETQUANTITIES") = ""
            workRow("PKINSTRUCTIONS") = ""
            workRow("PKPROVISIONS") = ""
            workRow("LPKINSTRUCTIONS") = ""
            workRow("LPKPROVISIONS") = ""
            workRow("IBCINSTRUCTIONS") = ""
            workRow("IBCPROVISIONS") = ""
            workRow("TANKINSTRUCTIONS") = ""
            workRow("TANKPROVISIONS") = ""
            workRow("FLEXIBLE") = ""
            workRow("SPPROVISIONS") = ""
            workRow("LOADINGMETHOD") = ""
            workRow("SEGREGATION") = ""
            workRow("REMARK") = ""
            workRow("ENABLED") = txtEnabled.Text
            workRow("DELFLG") = txtDelFlg.Text
            INPtbl.Rows.Add(workRow)
        Next

        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = hdnPrevViewID.Value
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
        COA0014DetailView.VARI = hdnPrevViewID.Value
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
            Next
        End If

        WF_DetailMView.ActiveViewIndex = 0

        lblDtabUnNo.Style.Remove("color")
        lblDtabUnNo.Style.Add("color", "blue")
        lblDtabUnNo.Style.Remove("background-color")
        lblDtabUnNo.Style.Add("background-color", "rgb(220,230,240)")
        lblDtabUnNo.Style.Remove("border")
        lblDtabUnNo.Style.Add("border", "1px solid blue")
        lblDtabUnNo.Style.Remove("font-weight")
        lblDtabUnNo.Style.Add("font-weight", "bold")

        'Detail設定処理
        SetDetailDbClick()

        '初期値設定
        Dim endDt As Date = Date.Parse("2099/12/31")
        Me.txtStYMD.Text = Date.Now.ToString(GBA00003UserSetting.DATEFORMAT)
        Me.txtEndYMD.Text = endDt.ToString(GBA00003UserSetting.DATEFORMAT)

        txtEnabled.Text = BaseDllCommon.CONST_FLAG_NO
        txtEnabled_Change()
        txtDelFlg.Text = BaseDllCommon.CONST_FLAG_NO
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
            Case Else
                repAttr = ""

        End Select
    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    Public Sub ListUpdateCheck(ByVal InpRow As DataRow)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck                '項目チェック
        Dim escapeFlg As Boolean = False
        Dim errFlg As Boolean = False
        Dim errMessageStr As String = Nothing
        Dim refErrMessage As String = Nothing
        Dim dicField As Dictionary(Of String, String)
        returnCode = C_MESSAGENO.NORMAL

        '入力文字置き換え
        '画面PassWord内の使用禁止文字排除

        '国連番号
        COA0008InvalidChar.CHARin = Convert.ToString(InpRow("UNNO"))
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            InpRow("UNNO") = COA0008InvalidChar.CHARout
        End If

        '等級
        COA0008InvalidChar.CHARin = Convert.ToString(InpRow("HAZARDCLASS"))
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            InpRow("HAZARDCLASS") = COA0008InvalidChar.CHARout
        End If

        '容器等級
        COA0008InvalidChar.CHARin = Convert.ToString(InpRow("PACKINGGROUP"))
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            InpRow("PACKINGGROUP") = COA0008InvalidChar.CHARout
        End If

        '有効開始日
        COA0008InvalidChar.CHARin = Convert.ToString(InpRow("STYMD"))
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            InpRow("STYMD") = COA0008InvalidChar.CHARout
        End If

        '有効終了日
        COA0008InvalidChar.CHARin = Convert.ToString(InpRow("ENDYMD"))
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            InpRow("ENDYMD") = COA0008InvalidChar.CHARout
        End If

        '削除フラグ
        COA0008InvalidChar.CHARin = Convert.ToString(InpRow("DELFLG"))
        COA0008InvalidChar.COA0008RemoveInvalidChar()
        If COA0008InvalidChar.CHARin = COA0008InvalidChar.CHARout Then
        Else
            InpRow("DELFLG") = COA0008InvalidChar.CHARout
        End If

        'カラム情報取得
        dicField = New Dictionary(Of String, String)

        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELDDIC = dicField
        COA0026FieldCheck.COA0026getFieldList()
        dicField = COA0026FieldCheck.FIELDDIC

        '入力項目チェック
        '①単項目チェック
        checkSingle(dicField, InpRow, errFlg, escapeFlg)

        '②存在チェック(LeftBoxチェック)
        '等級
        SetHazardClassListItem(Convert.ToString(InpRow("HAZARDCLASS")))
        ChedckList(Convert.ToString(InpRow("HAZARDCLASS")), lbHazardClass, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine &
                                            "・" & refErrMessage & "(" & dicField("HAZARDCLASS") & ":" & Convert.ToString(InpRow("HAZARDCLASS")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '容器等級　※容器等級は未制定あり
        SetPackingGroupListItem(Convert.ToString(InpRow("PACKINGGROUP")))
        ChedckList(Convert.ToString(InpRow("PACKINGGROUP")), lbPackingGroup, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine &
                                        "・" & refErrMessage & "(" & dicField("PACKINGGROUP") & ":" & Convert.ToString(InpRow("PACKINGGROUP")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'Enabled
        SetEnabledListItem(Convert.ToString(InpRow("ENABLED")))
        ChedckList(Convert.ToString(InpRow("ENABLED")), lbEnabled, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine &
                                            "・" & refErrMessage & "(" & dicField("ENABLED") & ":" & Convert.ToString(InpRow("ENABLED")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '削除フラグ
        SetDelFlgListItem(Convert.ToString(InpRow("DELFLG")))
        ChedckList(Convert.ToString(InpRow("DELFLG")), lbDelFlg, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine &
                                            "・" & refErrMessage & "(" & dicField("DELFLG") & ":" & Convert.ToString(InpRow("DELFLG")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
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
    ''' <param name="argDic"></param>
    ''' <param name="argRow"></param>
    ''' <param name="argErrFlg"></param>
    Protected Sub checkSingle(ByVal argDic As Dictionary(Of String, String), ByVal argRow As DataRow, ByRef argErrFlg As Boolean, ByRef argEscFlg As Boolean)
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck                '項目チェック
        Dim errMessageStr As String = Nothing
        Dim errMessage As String = Nothing

        For Each itm As KeyValuePair(Of String, String) In argDic

            '単項目チェック
            COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
            COA0026FieldCheck.MAPID = CONST_MAPID
            COA0026FieldCheck.FIELD = itm.Key
            COA0026FieldCheck.VALUE = Convert.ToString(argRow(itm.Key))
            COA0026FieldCheck.COA0026FieldCheck()
            If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
            Else
                CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
                errMessage = Me.lblFooterMessage.Text & "(" & itm.Value & ":" & Convert.ToString(argRow(itm.Key)) & ")"

                errMessageStr = Me.ErrItemSet(argRow)

                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & "・" & errMessage & errMessageStr

                If COA0026FieldCheck.ERR = C_MESSAGENO.REQUIREDVALUE Then
                    argEscFlg = True
                End If
                argErrFlg = True

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
    ''' <summary>
    ''' 国連番号リストアイテムを設定
    ''' </summary>
    Private Sub SetUnNoListItem(selectedValue As String)
        Dim GBA00001UnNo As New GBA00001UnNo              '項目チェック
        'リストクリア
        Me.lbUnNo.Items.Clear()

        'リスト設定
        GBA00001UnNo.LISTBOX = Me.lbUnNo
        GBA00001UnNo.GBA00001getLeftListUnNo()
        If GBA00001UnNo.ERR = C_MESSAGENO.NORMAL Then
            Me.lbUnNo = GBA00001UnNo.LISTBOX
            ViewState("UNNOKEYVALUE") = GBA00001UnNo.UnNoKeyValue

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbUnNo.Items.Count > 0 Then
                Dim findListItem = Me.lbUnNo.Items.FindByValue(selectedValue)
                If findListItem IsNot Nothing Then
                    findListItem.Selected = True
                End If
            End If
            '正常
            returnCode = C_MESSAGENO.NORMAL

        ElseIf GBA00001UnNo.ERR = C_MESSAGENO.NODATA Then
            returnCode = C_MESSAGENO.NORMAL

        Else
            '異常
            returnCode = C_MESSAGENO.SYSTEMADM
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00001UnNo.ERR)})
        End If

    End Sub
    ''' <summary>
    ''' 等級リストアイテムを設定
    ''' </summary>
    Private Sub SetHazardClassListItem(selectedValue As String)
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

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbHazardClass.Items.Count > 0 Then
                Dim findListItem = Me.lbHazardClass.Items.FindByValue(selectedValue)
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
    ''' 容器等級リストアイテムを設定
    ''' </summary>
    Private Sub SetPackingGroupListItem(selectedValue As String)
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

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbPackingGroup.Items.Count > 0 Then
                Dim findListItem = Me.lbPackingGroup.Items.FindByValue(selectedValue)
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
    ''' Enabledリストアイテムを設定
    ''' </summary>
    Private Sub SetEnabledListItem(selectedValue As String)
        Dim COA0017FixValue As New COA0017FixValue
        'リストクリア
        Me.lbEnabled.Items.Clear()

        'リスト設定
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

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbEnabled.Items.Count > 0 Then
                Dim findListItem = Me.lbEnabled.Items.FindByValue(selectedValue)
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
    ''' 国連番号名設定
    ''' </summary>
    Public Sub txtUnNo_Change()

        Try
            Me.lblUnNoText.Text = ""

            SetUnNoListItem(Me.txtUnNo.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbUnNo.Items.Count > 0 Then
                Dim findListItem = Me.lbUnNo.Items.FindByValue(Me.txtUnNo.Text)
                If findListItem IsNot Nothing Then
                    Me.lblUnNoText.Text = findListItem.Text
                    'Me.lblUnNoText.Attributes.Add("title", findListItem.Text)
                Else
                    Dim findListItemUpper = Me.lbUnNo.Items.FindByValue(Me.txtUnNo.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblUnNoText.Text = findListItemUpper.Text
                        'Me.lblUnNoText.Attributes.Add("title", findListItemUpper.Text)
                        Me.txtUnNo.Text = findListItemUpper.Value
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
    ''' 等級設定
    ''' </summary>
    Public Sub txtHazardClass_Change()

        Try
            Me.lblHazardClassText.Text = ""

            SetHazardClassListItem(Me.txtHazardClass.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbHazardClass.Items.Count > 0 Then
                Dim findListItem = Me.lbHazardClass.Items.FindByValue(Me.txtHazardClass.Text)
                If findListItem IsNot Nothing Then
                    Me.lblHazardClassText.Text = findListItem.Text
                    'Me.lblHazardClassText.Attributes.Add("title", findListItem.Text)
                Else
                    Dim findListItemUpper = Me.lbHazardClass.Items.FindByValue(Me.txtHazardClass.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblHazardClassText.Text = findListItemUpper.Text
                        'Me.lblHazardClassText.Attributes.Add("title", findListItemUpper.Text)
                        Me.txtHazardClass.Text = findListItemUpper.Value
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
    ''' 容器等級設定
    ''' </summary>
    Public Sub txtPackingGroup_Change()

        Try
            Me.lblPackingGroupText.Text = ""

            GetPackingGroupCharConv()
            If returnCode = C_MESSAGENO.NORMAL AndAlso charConvList.Items.Count > 0 Then

                Dim charConvItem = charConvList.Items.FindByValue(Me.txtPackingGroup.Text)
                If charConvItem IsNot Nothing Then
                    Me.txtPackingGroup.Text = charConvItem.Text
                End If
            End If

            SetPackingGroupListItem(Me.txtPackingGroup.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbPackingGroup.Items.Count > 0 Then
                Dim findListItem = Me.lbPackingGroup.Items.FindByValue(Me.txtPackingGroup.Text)
                If findListItem IsNot Nothing Then
                    Me.lblPackingGroupText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbPackingGroup.Items.FindByValue(Me.txtPackingGroup.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblPackingGroupText.Text = findListItemUpper.Text
                        Me.txtPackingGroup.Text = findListItemUpper.Value
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
    ''' Enabled名設定
    ''' </summary>
    Public Sub txtEnabled_Change()

        Try
            Me.lblEnabledText.Text = ""

            SetEnabledListItem(Me.txtEnabled.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbEnabled.Items.Count > 0 Then
                Dim findListItem = Me.lbEnabled.Items.FindByValue(Me.txtEnabled.Text)
                If findListItem IsNot Nothing Then
                    Me.lblEnabledText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbDelFlg.Items.FindByValue(Me.txtEnabled.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblEnabledText.Text = findListItemUpper.Text
                        Me.txtEnabled.Text = findListItemUpper.Value
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
    ''' detailboxクリア
    ''' </summary>
    Protected Sub detailboxClear()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        returnCode = C_MESSAGENO.NORMAL

        '一覧表示データ復元
        BASEtblColumnsAdd(BASEtbl)
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
                'Case "★更新"
                Case "★" & hdnOpeStrUpdate.Value
                    'BASEtbl.Rows(i)(1) = "更新"
                    BASEtbl.Rows(i)(1) = hdnOpeStrUpdate.Value
                'Case "★エラー"
                Case "★" & hdnOpeStrError.Value
                    'BASEtbl.Rows(i)(1) = "エラー"
                    BASEtbl.Rows(i)(1) = hdnOpeStrError.Value
            End Select
        Next

        '一覧表示データ保存
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            returnCode = COA0021ListTable.ERR
            Return
        End If

        lblLineCntText.Text = ""
        lblApplyIDText.Text = ""
        txtUnNo.Text = ""
        lblUnNoText.Text = ""
        txtPackingGroup.Text = ""
        lblPackingGroupText.Text = ""
        txtHazardClass.Text = ""
        lblHazardClassText.Text = ""
        txtStYMD.Text = ""
        txtEndYMD.Text = ""
        txtEnabled.Text = ""
        lblEnabledText.Text = ""
        txtDelFlg.Text = ""
        lblDelFlgText.Text = ""

        'ボタン制御
        SetButtonControl()

        'Detail初期設定
        detailboxInit()

        'フォーカス設定
        txtOperationEx.Focus()

        INPtbl.Clear()
        INPtbl.Dispose()

    End Sub
    ''' <summary>
    ''' 内部テーブルデータ更新
    ''' </summary>
    Protected Sub BASEtblUpdate()
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim dummyMsgBox As Label = New Label
        Dim errorMessage As String = Nothing
        Dim errMessageStr As String = Nothing

        '操作表示クリア
        For i As Integer = 0 To BASEtbl.Rows.Count - 1
            Select Case Convert.ToString(BASEtbl.Rows(i)("OPERATION"))
                Case "&nbsp;"
                    BASEtbl.Rows(i)("OPERATION") = ""
                Case "★"
                    BASEtbl.Rows(i)("OPERATION") = ""
                'Case "★更新"
                Case "★" & hdnOpeStrUpdate.Value
                    'BASEtbl.Rows(i)("OPERATION") = "更新"
                    BASEtbl.Rows(i)("OPERATION") = hdnOpeStrUpdate.Value
                'Case "★エラー"
                Case "★" & hdnOpeStrError.Value
                    'BASEtbl.Rows(i)("OPERATION") = "エラー"
                    BASEtbl.Rows(i)("OPERATION") = hdnOpeStrError.Value
            End Select
        Next

        For i As Integer = 0 To INPtbl.Rows.Count - 1

            If Convert.ToString(INPtbl(i)("HIDDEN")) <> "1" Then ' "1" ・・・取り込み対象外エラー

                Dim workBasePos As Integer = -1

                '内部テーブル検索
                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                    Dim workBaseRow As DataRow
                    workBaseRow = BASEtbl.NewRow
                    workBaseRow.ItemArray = BASEtbl.Rows(j).ItemArray

                    ' 更新対象検索
                    'If workBaseRow("APPLYID") = INPtbl(i)("APPLYID") AndAlso
                    If Convert.ToString(workBaseRow("UNNO")) = Convert.ToString(INPtbl(i)("UNNO")) AndAlso
                       Convert.ToString(workBaseRow("HAZARDCLASS")) = Convert.ToString(INPtbl(i)("HAZARDCLASS")) AndAlso
                       Convert.ToString(workBaseRow("PACKINGGROUP")) = Convert.ToString(INPtbl(i)("PACKINGGROUP")) AndAlso
                       Convert.ToString(workBaseRow("STYMD")) = Convert.ToString(INPtbl(i)("STYMD")) Then

                        ' 変更なし  
                        'If INPtbl(i)("OPERATION") <> "エラー" AndAlso
                        If Convert.ToString(INPtbl(i)("OPERATION")) <> hdnOpeStrError.Value AndAlso
                           Convert.ToString(workBaseRow("ENDYMD")) = Convert.ToString(INPtbl(i)("ENDYMD")) AndAlso
                           Convert.ToString(workBaseRow("PRODUCTNAME")) = Convert.ToString(INPtbl(i)("PRODUCTNAME")) AndAlso
                           Convert.ToString(workBaseRow("PRODUCTNAME_EN")) = Convert.ToString(INPtbl(i)("PRODUCTNAME_EN")) AndAlso
                           Convert.ToString(workBaseRow("NAME")) = Convert.ToString(INPtbl(i)("NAME")) AndAlso
                           Convert.ToString(workBaseRow("NAME_EN")) = Convert.ToString(INPtbl(i)("NAME_EN")) AndAlso
                           Convert.ToString(workBaseRow("COMPATIBILITYGROUP")) = Convert.ToString(INPtbl(i)("COMPATIBILITYGROUP")) AndAlso
                           Convert.ToString(workBaseRow("SUBSIDIARYRISK")) = Convert.ToString(INPtbl(i)("SUBSIDIARYRISK")) AndAlso
                           Convert.ToString(workBaseRow("LIMITEDQUANTITIES")) = Convert.ToString(INPtbl(i)("LIMITEDQUANTITIES")) AndAlso
                           Convert.ToString(workBaseRow("EXCEPTETQUANTITIES")) = Convert.ToString(INPtbl(i)("EXCEPTETQUANTITIES")) AndAlso
                           Convert.ToString(workBaseRow("PKINSTRUCTIONS")) = Convert.ToString(INPtbl(i)("PKINSTRUCTIONS")) AndAlso
                           Convert.ToString(workBaseRow("PKPROVISIONS")) = Convert.ToString(INPtbl(i)("PKPROVISIONS")) AndAlso
                           Convert.ToString(workBaseRow("LPKINSTRUCTIONS")) = Convert.ToString(INPtbl(i)("LPKINSTRUCTIONS")) AndAlso
                           Convert.ToString(workBaseRow("LPKPROVISIONS")) = Convert.ToString(INPtbl(i)("LPKPROVISIONS")) AndAlso
                           Convert.ToString(workBaseRow("IBCINSTRUCTIONS")) = Convert.ToString(INPtbl(i)("IBCINSTRUCTIONS")) AndAlso
                           Convert.ToString(workBaseRow("IBCPROVISIONS")) = Convert.ToString(INPtbl(i)("IBCPROVISIONS")) AndAlso
                           Convert.ToString(workBaseRow("TANKINSTRUCTIONS")) = Convert.ToString(INPtbl(i)("TANKINSTRUCTIONS")) AndAlso
                           Convert.ToString(workBaseRow("TANKPROVISIONS")) = Convert.ToString(INPtbl(i)("TANKPROVISIONS")) AndAlso
                           Convert.ToString(workBaseRow("FLEXIBLE")) = Convert.ToString(INPtbl(i)("FLEXIBLE")) AndAlso
                           Convert.ToString(workBaseRow("SPPROVISIONS")) = Convert.ToString(INPtbl(i)("SPPROVISIONS")) AndAlso
                           Convert.ToString(workBaseRow("LOADINGMETHOD")) = Convert.ToString(INPtbl(i)("LOADINGMETHOD")) AndAlso
                           Convert.ToString(workBaseRow("SEGREGATION")) = Convert.ToString(INPtbl(i)("SEGREGATION")) AndAlso
                           Convert.ToString(workBaseRow("REMARK")) = Convert.ToString(INPtbl(i)("REMARK")) AndAlso
                           Convert.ToString(workBaseRow("ENABLED")) = Convert.ToString(INPtbl(i)("ENABLED")) AndAlso
                           Convert.ToString(workBaseRow("DELFLG")) = Convert.ToString(INPtbl(i)("DELFLG")) Then
                            workBasePos = -999    '-999 は登録対象外
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
                End If

                ' 内部テーブル編集
                If workBasePos >= 0 Then

                    '内部テーブル検索
                    For k As Integer = 0 To BASEtbl.Rows.Count - 1

                        Dim workBaseRow2 As DataRow
                        workBaseRow2 = BASEtbl.NewRow
                        workBaseRow2.ItemArray = BASEtbl.Rows(k).ItemArray

                        If Convert.ToString(workBaseRow2("UNNO")) = Convert.ToString(INPtbl(i)("UNNO")) AndAlso
                           Convert.ToString(workBaseRow2("HAZARDCLASS")) = Convert.ToString(INPtbl(i)("HAZARDCLASS")) AndAlso
                           Convert.ToString(workBaseRow2("PACKINGGROUP")) = Convert.ToString(INPtbl(i)("PACKINGGROUP")) AndAlso
                           Convert.ToString(workBaseRow2("STYMD")) = Convert.ToString(INPtbl(i)("STYMD")) Then

                            '申請中のものはエラー
                            If Convert.ToString(workBaseRow2("APPLYID")) <> "" Then
                                returnCode = C_MESSAGENO.HASAPPLYINGRECORD
                                CommonFunctions.ShowMessage(returnCode, dummyMsgBox)
                                errorMessage = dummyMsgBox.Text
                                'エラーレポート編集
                                errMessageStr = ""
                                errMessageStr = "・" & errorMessage
                                ' レコード内容を展開する
                                errMessageStr = errMessageStr & Me.ErrItemSet(INPtbl(i))
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr

                                INPtbl(i)("OPERATION") = hdnOpeStrError.Value
                            End If

                        End If

                    Next

                    '固定項目
                    Dim workBaseRow As DataRow
                    workBaseRow = BASEtbl.NewRow

                    If workBasePos < BASEtbl.Rows.Count Then
                        '更新対象を設定
                        workBaseRow.ItemArray = BASEtbl.Rows(workBasePos).ItemArray
                    End If

                    '固定項目
                    workBaseRow("LINECNT") = workBasePos + 1
                    'If INPtbl(i)("OPERATION") <> "エラー" Then
                    If Convert.ToString(INPtbl(i)("OPERATION")) <> hdnOpeStrError.Value Then
                        'workBaseRow("OPERATION") = "更新"
                        workBaseRow("OPERATION") = hdnOpeStrUpdate.Value
                    Else
                        workBaseRow("OPERATION") = INPtbl(i)("OPERATION")
                    End If
                    If workBasePos >= BASEtbl.Rows.Count Then
                        workBaseRow("TIMSTP") = "0"                                 ' 新規レコード
                    Else
                        workBaseRow("TIMSTP") = BASEtbl(workBasePos)("TIMSTP")      ' 更新レコード
                    End If
                    workBaseRow("SELECT") = 1
                    workBaseRow("HIDDEN") = 0

                    Dim stDate As Date = Nothing
                    Dim endDate As Date = Nothing

                    '個別項目
                    'workBaseRow("APPLYID") = INPtbl(i)("APPLYID")
                    workBaseRow("UNNO") = INPtbl(i)("UNNO")
                    workBaseRow("HAZARDCLASS") = INPtbl(i)("HAZARDCLASS")
                    workBaseRow("PACKINGGROUP") = INPtbl(i)("PACKINGGROUP")
                    If Date.TryParse(Convert.ToString(INPtbl(i)("STYMD")), stDate) Then
                        workBaseRow("STYMD") = stDate.ToString("yyyy/MM/dd")
                    Else
                        workBaseRow("STYMD") = INPtbl(i)("STYMD")
                    End If
                    If Date.TryParse(Convert.ToString(INPtbl(i)("ENDYMD")), endDate) Then
                        workBaseRow("ENDYMD") = endDate.ToString("yyyy/MM/dd")
                    Else
                        workBaseRow("ENDYMD") = INPtbl(i)("ENDYMD")
                    End If
                    workBaseRow("PRODUCTNAME") = INPtbl(i)("PRODUCTNAME")
                    workBaseRow("PRODUCTNAME_EN") = INPtbl(i)("PRODUCTNAME_EN")
                    workBaseRow("NAME") = INPtbl(i)("NAME")
                    workBaseRow("NAME_EN") = INPtbl(i)("NAME_EN")
                    workBaseRow("COMPATIBILITYGROUP") = INPtbl(i)("COMPATIBILITYGROUP")
                    workBaseRow("SUBSIDIARYRISK") = INPtbl(i)("SUBSIDIARYRISK")
                    workBaseRow("LIMITEDQUANTITIES") = INPtbl(i)("LIMITEDQUANTITIES")
                    workBaseRow("EXCEPTETQUANTITIES") = INPtbl(i)("EXCEPTETQUANTITIES")
                    workBaseRow("PKINSTRUCTIONS") = INPtbl(i)("PKINSTRUCTIONS")
                    workBaseRow("PKPROVISIONS") = INPtbl(i)("PKPROVISIONS")
                    workBaseRow("LPKINSTRUCTIONS") = INPtbl(i)("LPKINSTRUCTIONS")
                    workBaseRow("LPKPROVISIONS") = INPtbl(i)("LPKPROVISIONS")
                    workBaseRow("IBCINSTRUCTIONS") = INPtbl(i)("IBCINSTRUCTIONS")
                    workBaseRow("IBCPROVISIONS") = INPtbl(i)("IBCPROVISIONS")
                    workBaseRow("TANKINSTRUCTIONS") = INPtbl(i)("TANKINSTRUCTIONS")
                    workBaseRow("TANKPROVISIONS") = INPtbl(i)("TANKPROVISIONS")
                    workBaseRow("FLEXIBLE") = INPtbl(i)("FLEXIBLE")
                    workBaseRow("SPPROVISIONS") = INPtbl(i)("SPPROVISIONS")
                    workBaseRow("LOADINGMETHOD") = INPtbl(i)("LOADINGMETHOD")
                    workBaseRow("SEGREGATION") = INPtbl(i)("SEGREGATION")
                    workBaseRow("REMARK") = INPtbl(i)("REMARK")
                    workBaseRow("ENABLED") = INPtbl(i)("ENABLED")
                    If Convert.ToString(INPtbl(i)("DELFLG")) = "" Then
                        workBaseRow("DELFLG") = BaseDllCommon.CONST_FLAG_NO
                    Else
                        workBaseRow("DELFLG") = INPtbl(i)("DELFLG")
                    End If

                    If workBasePos < BASEtbl.Rows.Count Then
                        '更新対象を設定
                        BASEtbl.Rows(workBasePos).ItemArray = workBaseRow.ItemArray
                    Else
                        BASEtbl.Rows.Add(workBaseRow)
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
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
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
        txtUnNo.Text = Convert.ToString(dataTable(0)("UNNO"))
        'txtUnNo_Change()
        txtHazardClass.Text = Convert.ToString(dataTable(0)("HAZARDCLASS"))
        txtHazardClass_Change()
        txtPackingGroup.Text = Convert.ToString(dataTable(0)("PACKINGGROUP"))
        txtPackingGroup_Change()
        txtStYMD.Text = FormatDateContrySettings(Convert.ToString(dataTable(0)("STYMD")), GBA00003UserSetting.DATEFORMAT)
        txtEndYMD.Text = FormatDateContrySettings(Convert.ToString(dataTable(0)("ENDYMD")), GBA00003UserSetting.DATEFORMAT)
        txtEnabled.Text = Convert.ToString(dataTable(0)("ENABLED"))
        txtEnabled_Change()
        txtDelFlg.Text = Convert.ToString(dataTable(0)("DELFLG"))
        txtDelFlg_Change()

        'ボタン制御
        SetButtonControl()

        'ダブルクリック明細情報取得設定（Detailbox情報)
        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = hdnPrevViewID.Value
        COA0014DetailView.TABID = ""
        COA0014DetailView.SRCDATA = dataTable
        COA0014DetailView.REPEATER = WF_DViewRep1
        COA0014DetailView.COLPREFIX = "WF_Rep1_"
        COA0014DetailView.COA0014SetDetailView()

        'Detail初期設定
        SetDetailDbClick()

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
                'Case "★更新"
                Case "★" & hdnOpeStrUpdate.Value
                    'BASEtbl.Rows(i)(1) = "更新"
                    BASEtbl.Rows(i)(1) = hdnOpeStrUpdate.Value
                'Case "★エラー"
                Case "★" & hdnOpeStrError.Value
                    'BASEtbl.Rows(i)(1) = "エラー"
                    BASEtbl.Rows(i)(1) = hdnOpeStrError.Value
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
            'Case "更新"
            Case hdnOpeStrUpdate.Value
                'BASEtbl.Rows(lineCnt)(1) = "★更新"
                BASEtbl.Rows(lineCnt)(1) = "★" & hdnOpeStrUpdate.Value
            'Case "エラー"
            Case hdnOpeStrError.Value
                'BASEtbl.Rows(lineCnt)(1) = "★エラー"
                BASEtbl.Rows(lineCnt)(1) = "★" & hdnOpeStrError.Value
            Case Else
        End Select

        '画面表示データ保存
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = BASEtbl
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        BASEtbl.Clear()
        BASEtbl.Dispose()

        '画面編集
        txtUnNo.Focus()

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

        '国連番号情報
        'lblDtabUnNo.Style.Remove("color")
        'lblDtabUnNo.Style.Add("color", "black")
        'lblDtabUnNo.Style.Remove("background-color")
        'lblDtabUnNo.Style.Add("background-color", "rgb(255,255,253)")
        'lblDtabUnNo.Style.Remove("border")
        'lblDtabUnNo.Style.Add("border", "1px solid black")
        'lblDtabUnNo.Style.Remove("font-weight")
        'lblDtabUnNo.Style.Add("font-weight", "normal")

        'Select Case WF_DetailMView.ActiveViewIndex
        '    Case 0
        '国連番号情報
        lblDtabUnNo.Style.Remove("color")
        lblDtabUnNo.Style.Add("color", "blue")
        lblDtabUnNo.Style.Remove("background-color")
        lblDtabUnNo.Style.Add("background-color", "rgb(220,230,240)")
        lblDtabUnNo.Style.Remove("border")
        lblDtabUnNo.Style.Add("border", "1px solid blue")
        lblDtabUnNo.Style.Remove("font-weight")
        lblDtabUnNo.Style.Add("font-weight", "bold")

        'End Select

    End Sub

    ''' <summary>
    ''' 抽出条件退避
    ''' </summary>
    Protected Sub getPrevInfo()

        If TypeOf Page.PreviousPage Is GBM00007SELECT Then

            ' 選択画面オブジェクト
            Dim prevSelectUnNoPage As GBM00007SELECT = DirectCast(Page.PreviousPage, GBM00007SELECT)
            hdnPrevViewID.Value = DirectCast(prevSelectUnNoPage.FindControl("lbRightList"), ListBox).SelectedValue
            hdnPrevCondStYMD.Value = FormatDateYMD(DirectCast(prevSelectUnNoPage.FindControl("txtStYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)
            hdnPrevCondEndYMD.Value = FormatDateYMD(DirectCast(prevSelectUnNoPage.FindControl("txtEndYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)
            If hdnPrevCondEndYMD.Value = "" Then
                hdnPrevCondEndYMD.Value = hdnPrevCondStYMD.Value
            End If
            hdnPrevCondUNNO.Value = DirectCast(prevSelectUnNoPage.FindControl("txtUNNO"), TextBox).Text
            hdnPrevCondHazardClass.Value = DirectCast(prevSelectUnNoPage.FindControl("txtHazardClass"), TextBox).Text
            hdnPrevCondPackingGroup.Value = DirectCast(prevSelectUnNoPage.FindControl("txtPackingGroup"), TextBox).Text

        ElseIf Page.PreviousPage Is Nothing Then

            Dim prevObj As GBM00000APPROVAL = DirectCast(Page.PreviousPage, GBM00000APPROVAL)

            'Me.hdnPrevViewID.Value = Convert.ToString(Request.Form("hdnSelectedValue1"))
            'Me.hdnPrevCondStYMD.Value = Convert.ToString(Request.Form("hdnSelectedValue2"))
            'Me.hdnPrevCondEndYMD.Value = Convert.ToString(Request.Form("hdnSelectedValue3"))
            'Me.hdnPrevCondUNNO.Value = Convert.ToString(Request.Form("hdnSelectedValue4"))
            'Me.hdnPrevCondHazardClass.Value = Convert.ToString(Request.Form("hdnSelectedValue5"))
            'Me.hdnPrevCondPackingGroup.Value = Convert.ToString(Request.Form("hdnSelectedValue6"))

            Me.hdnSelectedApplyID.Value = Convert.ToString(Request.Form("hdnSelectedValue1"))
            Me.hdnPrevCondStYMD.Value = FormatDateYMD(Convert.ToString(Request.Form("hdnSelectedValue2")), GBA00003UserSetting.DATEFORMAT)
            Me.hdnPrevCondEndYMD.Value = FormatDateYMD(Convert.ToString(Request.Form("hdnSelectedValue3")), GBA00003UserSetting.DATEFORMAT)

            Me.hdnPrevViewID.Value = "Default"

        End If

    End Sub

    Protected Sub SetButtonControl()

        If lblApplyIDText.Text <> "" Then
            btnListUpdate.Disabled = True
        Else
            btnListUpdate.Disabled = False
        End If
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
    ''' 絞り込み容器等級設定
    ''' </summary>
    Public Sub txtPackingGroupEx_Change()

        Try

            GetPackingGroupCharConv()
            If returnCode = C_MESSAGENO.NORMAL AndAlso charConvList.Items.Count > 0 Then

                Dim charConvItem = charConvList.Items.FindByValue(Me.txtPackingGroupEx.Text)
                If charConvItem IsNot Nothing Then
                    Me.txtPackingGroupEx.Text = charConvItem.Text
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
    ''' 表示非表示制御
    ''' </summary>
    Private Sub VisibleControls()

        If Page.PreviousPage Is Nothing Then

            Me.btnListUpdate.Visible = False
            Me.btnDbUpdate.Visible = False

        End If

    End Sub
End Class