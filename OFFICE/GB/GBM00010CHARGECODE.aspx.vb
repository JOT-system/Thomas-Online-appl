Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net
Imports BASEDLL

''' <summary>
''' 費用項目マスタ画面クラス
''' </summary>
Public Class GBM00010CHARGECODE
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00010"    '自身のMAPID
    Private Const CONST_BASEDATATABLE = "GBM00010TBL"
    Private Const CONST_INPDATATABLE = "GBM00010INPTBL"
    Private Const CONST_UPDDATATABLE = "GBM00010UPDTBL"
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_TBLMASTER = "GBM0010_CHARGECODE"
    Private Const CONST_TBLAPPLY = "GBM0018_CHARGECODEAPPLY"
    Private Const CONST_EVENTCODE = "MasterApplyCharge"

    Dim errListAll As List(Of String)                   'インポート全体のエラー
    Dim errList As List(Of String)                      'インポート中の１セット分のエラー
    Private returnCode As String = String.Empty         'サブ用リターンコード
    Dim errDisp As String = Nothing                     'エラー用表示文言
    Dim updateDisp As String = Nothing                  '更新用表示文言

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
                    UploadExcel()
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
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
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
                & "       COSTCODE                           , " _
                & "       LDKBN                              , " _
                & "       STYMD                              , " _
                & "       ENDYMD                             , " _
                & "       CLASS1                             , " _
                & "       CLASS2                             , " _
                & "       CLASS3                             , " _
                & "       CLASS4                             , " _
                & "       CLASS5                             , " _
                & "       CLASS6                             , " _
                & "       CLASS7                             , " _
                & "       CLASS8                             , " _
                & "       CLASS9                             , " _
                & "       CLASS10                            , " _
                & "       SALESBR                            , " _
                & "       OPERATIONBR                        , " _
                & "       REPAIRBR                           , " _
                & "       SALES                              , " _
                & "       BL                                 , " _
                & "       TANKOPE                            , " _
                & "       NONBR                              , " _
                & "       SOA                                , " _
                & "       NAMES                              , " _
                & "       NAMEL                              , " _
                & "       NAMESJP                            , " _
                & "       NAMELJP                            , " _
                & "       SOACODE                            , " _
                & "       DATA                               , " _
                & "       JOTCODE                            , " _
                & "       ACCODE                             , " _
                & "       CRACCOUNT                          , " _
                & "       DBACCOUNT                          , " _
                & "       CRACCOUNTFORIGN                    , " _
                & "       DBACCOUNTFORIGN                    , " _
                & "       OFFCRACCOUNT                       , " _
                & "       OFFDBACCOUNT                       , " _
                & "       OFFCRACCOUNTFORIGN                 , " _
                & "       OFFDBACCOUNTFORIGN                 , " _
                & "       ACCAMPCODE                         , " _
                & "       ACTORICODE                         , " _
                & "       ACTORICODES                        , " _
                & "       CRGENERALPURPOSE                   , " _
                & "       DBGENERALPURPOSE                   , " _
                & "       CRSEGMENT1                         , " _
                & "       DBSEGMENT1                         , " _
                & "       REMARK                             , " _
                & "       DELFLG                             , " _
                & "       UPDYMD                             , " _
                & "       UPDUSER                            , " _
                & "       UPDTERMID                            " _
                & "  FROM (" _
                & "SELECT " _
                & "       '' as APPLYID , " _
                & "       isnull(rtrim(COMPCODE),'')                  as COMPCODE , " _
                & "       isnull(rtrim(COSTCODE),'')                  as COSTCODE , " _
                & "       isnull(rtrim(LDKBN),'')                     as LDKBN , " _
                & "       isnull(convert(nvarchar, STYMD , 111),'')   as STYMD , " _
                & "       isnull(convert(nvarchar, ENDYMD , 111),'')  as ENDYMD , " _
                & "       isnull(rtrim(CLASS1),'')                    as CLASS1 , " _
                & "       isnull(rtrim(CLASS2),'')                    as CLASS2 , " _
                & "       isnull(rtrim(CLASS3),'')                    as CLASS3 , " _
                & "       isnull(rtrim(CLASS4),'')                    as CLASS4 , " _
                & "       isnull(rtrim(CLASS5),'')                    as CLASS5 , " _
                & "       isnull(rtrim(CLASS6),'')                    as CLASS6 , " _
                & "       isnull(rtrim(CLASS7),'')                    as CLASS7 , " _
                & "       isnull(rtrim(CLASS8),'')                    as CLASS8 , " _
                & "       isnull(rtrim(CLASS9),'')                    as CLASS9 , " _
                & "       isnull(rtrim(CLASS10),'')                    as CLASS10 , " _
                & "       isnull(rtrim(SALESBR),'')                   as SALESBR , " _
                & "       isnull(rtrim(OPERATIONBR),'')               as OPERATIONBR , " _
                & "       isnull(rtrim(REPAIRBR),'')                  as REPAIRBR , " _
                & "       isnull(rtrim(SALES),'')                     as SALES , " _
                & "       isnull(rtrim(BL),'')                        as BL , " _
                & "       isnull(rtrim(TANKOPE),'')                   as TANKOPE , " _
                & "       isnull(rtrim(NONBR),'')                     as NONBR , " _
                & "       isnull(rtrim(SOA),'')                       as SOA , " _
                & "       isnull(rtrim(NAMES),'')                     as NAMES , " _
                & "       isnull(rtrim(NAMEL),'')                     as NAMEL , " _
                & "       isnull(rtrim(NAMESJP),'')                   as NAMESJP , " _
                & "       isnull(rtrim(NAMELJP),'')                   as NAMELJP , " _
                & "       isnull(rtrim(SOACODE),'')                   as SOACODE , " _
                & "       isnull(rtrim(DATA),'')                      as DATA , " _
                & "       isnull(rtrim(JOTCODE),'')                   as JOTCODE , " _
                & "       isnull(rtrim(ACCODE),'')                    as ACCODE , " _
                & "       isnull(rtrim(CRACCOUNT),'')                 as CRACCOUNT , " _
                & "       isnull(rtrim(DBACCOUNT),'')                 as DBACCOUNT , " _
                & "       isnull(rtrim(CRACCOUNTFORIGN),'')           as CRACCOUNTFORIGN , " _
                & "       isnull(rtrim(DBACCOUNTFORIGN),'')           as DBACCOUNTFORIGN , " _
                & "       isnull(rtrim(OFFCRACCOUNT),'')              as OFFCRACCOUNT , " _
                & "       isnull(rtrim(OFFDBACCOUNT),'')              as OFFDBACCOUNT , " _
                & "       isnull(rtrim(OFFCRACCOUNTFORIGN),'')        as OFFCRACCOUNTFORIGN , " _
                & "       isnull(rtrim(OFFDBACCOUNTFORIGN),'')        as OFFDBACCOUNTFORIGN , " _
                & "       isnull(rtrim(ACCAMPCODE),'')                as ACCAMPCODE , " _
                & "       isnull(rtrim(ACTORICODE),'')                as ACTORICODE , " _
                & "       isnull(rtrim(ACTORICODES),'')               as ACTORICODES , " _
                & "       isnull(rtrim(CRGENERALPURPOSE),'')          as CRGENERALPURPOSE , " _
                & "       isnull(rtrim(DBGENERALPURPOSE),'')          as DBGENERALPURPOSE , " _
                & "       isnull(rtrim(CRSEGMENT1),'')                as CRSEGMENT1 , " _
                & "       isnull(rtrim(DBSEGMENT1),'')                as DBSEGMENT1 , " _
                & "       isnull(rtrim(REMARK),'')                    as REMARK , " _
                & "       isnull(rtrim(DELFLG),'')                    as DELFLG , " _
                & "       isnull(convert(nvarchar, UPDYMD , 120),'')  as UPDYMD , " _
                & "       isnull(rtrim(UPDUSER),'')                   as UPDUSER , " _
                & "       isnull(rtrim(UPDTERMID),'')                 as UPDTERMID , " _
                & "       TIMSTP = cast(UPDTIMSTP                     as bigint) " _
                & " FROM " & CONST_TBLMASTER & " as tbl1 " _
                & " WHERE DELFLG    <> '" & CONST_FLAG_YES & "' " _
                & " AND   STYMD     <= @P1 " _
                & " AND   ENDYMD    >= @P2 " _
                & " AND   NOT EXISTS( "
            '承認画面から遷移の場合
            If  Page.PreviousPage Is Nothing Then
                SQLStr &= " SELECT * FROM " & CONST_TBLAPPLY & " as tbl2 " _
                    & " WHERE tbl2.APPLYID = @P3 "
            Else
                SQLStr &= " SELECT * FROM " & CONST_TBLAPPLY & " as tbl2 " _
                    & " WHERE tbl1.COMPCODE = tbl2.COMPCODE " _
                    & " AND   tbl1.LDKBN    = tbl2.LDKBN " _
                    & " AND   tbl1.COSTCODE = tbl2.COSTCODE " _
                    & " AND   tbl1.STYMD = tbl2.STYMD " _
                    & " AND   tbl1.DELFLG <> '" & CONST_FLAG_YES & "' " _
                    & " AND   tbl2.DELFLG <> '" & CONST_FLAG_YES & "' "
            End If
            SQLStr &= " )" _
                & " UNION ALL " _
                & "SELECT " _
                & "       isnull(rtrim(APPLYID),'')                   as APPLYID , " _
                & "       isnull(rtrim(COMPCODE),'')                  as COMPCODE , " _
                & "       isnull(rtrim(COSTCODE),'')                  as COSTCODE , " _
                & "       isnull(rtrim(LDKBN),'')                     as LDKBN , " _
                & "       isnull(convert(nvarchar, STYMD , 111),'')   as STYMD , " _
                & "       isnull(convert(nvarchar, ENDYMD , 111),'')  as ENDYMD , " _
                & "       isnull(rtrim(CLASS1),'')                    as CLASS1 , " _
                & "       isnull(rtrim(CLASS2),'')                    as CLASS2 , " _
                & "       isnull(rtrim(CLASS3),'')                    as CLASS3 , " _
                & "       isnull(rtrim(CLASS4),'')                    as CLASS4 , " _
                & "       isnull(rtrim(CLASS5),'')                    as CLASS5 , " _
                & "       isnull(rtrim(CLASS6),'')                    as CLASS6 , " _
                & "       isnull(rtrim(CLASS7),'')                    as CLASS7 , " _
                & "       isnull(rtrim(CLASS8),'')                    as CLASS8 , " _
                & "       isnull(rtrim(CLASS9),'')                    as CLASS9 , " _
                & "       isnull(rtrim(CLASS10),'')                    as CLASS10 , " _
                & "       isnull(rtrim(SALESBR),'')                   as SALESBR , " _
                & "       isnull(rtrim(OPERATIONBR),'')               as OPERATIONBR , " _
                & "       isnull(rtrim(REPAIRBR),'')                  as REPAIRBR , " _
                & "       isnull(rtrim(SALES),'')                     as SALES , " _
                & "       isnull(rtrim(BL),'')                        as BL , " _
                & "       isnull(rtrim(TANKOPE),'')                   as TANKOPE , " _
                & "       isnull(rtrim(NONBR),'')                     as NONBR , " _
                & "       isnull(rtrim(SOA),'')                       as SOA , " _
                & "       isnull(rtrim(NAMES),'')                     as NAMES , " _
                & "       isnull(rtrim(NAMEL),'')                     as NAMEL , " _
                & "       isnull(rtrim(NAMESJP),'')                   as NAMESJP , " _
                & "       isnull(rtrim(NAMELJP),'')                   as NAMELJP , " _
                & "       isnull(rtrim(SOACODE),'')                   as SOACODE , " _
                & "       isnull(rtrim(DATA),'')                      as DATA , " _
                & "       isnull(rtrim(JOTCODE),'')                   as JOTCODE , " _
                & "       isnull(rtrim(ACCODE),'')                    as ACCODE , " _
                & "       isnull(rtrim(CRACCOUNT),'')                 as CRACCOUNT , " _
                & "       isnull(rtrim(DBACCOUNT),'')                 as DBACCOUNT , " _
                & "       isnull(rtrim(CRACCOUNTFORIGN),'')           as CRACCOUNTFORIGN , " _
                & "       isnull(rtrim(DBACCOUNTFORIGN),'')           as DBACCOUNTFORIGN , " _
                & "       isnull(rtrim(OFFCRACCOUNT),'')              as OFFCRACCOUNT , " _
                & "       isnull(rtrim(OFFDBACCOUNT),'')              as OFFDBACCOUNT , " _
                & "       isnull(rtrim(OFFCRACCOUNTFORIGN),'')        as OFFCRACCOUNTFORIGN , " _
                & "       isnull(rtrim(OFFDBACCOUNTFORIGN),'')        as OFFDBACCOUNTFORIGN , " _
                & "       isnull(rtrim(ACCAMPCODE),'')                as ACCAMPCODE , " _
                & "       isnull(rtrim(ACTORICODE),'')                as ACTORICODE , " _
                & "       isnull(rtrim(ACTORICODES),'')               as ACTORICODES , " _
                & "       isnull(rtrim(CRGENERALPURPOSE),'')          as CRGENERALPURPOSE , " _
                & "       isnull(rtrim(DBGENERALPURPOSE),'')          as DBGENERALPURPOSE , " _
                & "       isnull(rtrim(CRSEGMENT1),'')                as CRSEGMENT1 , " _
                & "       isnull(rtrim(DBSEGMENT1),'')                as DBSEGMENT1 , " _
                & "       isnull(rtrim(REMARK),'')                    as REMARK , " _
                & "       isnull(rtrim(DELFLG),'')                    as DELFLG , " _
                & "       isnull(convert(nvarchar, UPDYMD , 120),'')  as UPDYMD , " _
                & "       isnull(rtrim(UPDUSER),'')                   as UPDUSER , " _
                & "       isnull(rtrim(UPDTERMID),'')                 as UPDTERMID , " _
                & "       TIMSTP = cast(UPDTIMSTP                     as bigint) " _
                & " FROM " & CONST_TBLAPPLY & " "
            '承認画面から遷移の場合
            If Page.PreviousPage Is Nothing Then
                SQLStr &= " WHERE APPLYID    = @P3 " _
                & " ) as tbl " _
                & " WHERE APPLYID    = @P3 "
            Else
                SQLStr &= " WHERE DELFLG    <> '" & CONST_FLAG_YES & "' " _
                & " AND   STYMD     <= @P1 " _
                & " AND   ENDYMD    >= @P2 " _
                & " ) as tbl " _
                & " WHERE DELFLG    <> '" & CONST_FLAG_YES & "' " _
                & " AND   STYMD     <= @P1 " _
                & " AND   ENDYMD    >= @P2 "
            End If

            ' 条件指定で指定されたものでＳＱＬで可能なものを追加する

            If Page.PreviousPage Is Nothing Then
            Else

                '費用コード
                If (String.IsNullOrEmpty(Me.hdnSelectedCostCode.Value) = False) Then
                    SQLStr &= String.Format(" AND COSTCODE = '{0}' ", Me.hdnSelectedCostCode.Value)
                End If

            End If

            SQLStr &= " ORDER BY " & COA0020ProfViewSort.SORTSTR

            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            Dim PARA1 As SqlParameter = SQLcmd.Parameters.Add("@P1", System.Data.SqlDbType.Date)
            Dim PARA2 As SqlParameter = SQLcmd.Parameters.Add("@P2", System.Data.SqlDbType.Date)
            Dim PARA3 As SqlParameter = SQLcmd.Parameters.Add("@P3", System.Data.SqlDbType.NVarChar)
            PARA1.Value = Me.hdnSelectedEndYMD.Value
            PARA2.Value = Me.hdnSelectedStYMD.Value
            If (String.IsNullOrEmpty(Me.hdnSelectedApplyID.Value) = False) Then
                PARA3.Value = Me.hdnSelectedApplyID.value
            Else
                PARA3.Value = ""
            End If
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
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString(GBA00003UserSetting.DATEFORMAT)
                        End If
                    End If
                    If workColumn = "ENDYMD" Then
                        If IsDate(INProwWork(workColumn)) Then
                            INProwWork(workColumn) = Date.Parse(Convert.ToString(INProwWork(workColumn))).ToString(GBA00003UserSetting.DATEFORMAT)
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
                Case Me.vLeftLdKbn.ID
                    SetLdKbnListItem()
                '分類１ビュー表示切替
                Case Me.vLeftClass1.ID
                    SetClass1ListItem()
                '分類２(売上内訳)ビュー表示切替
                Case Me.vLeftClass2.ID
                    SetClass2ListItem()
                '分類３(費用内訳)ビュー表示切替
                Case Me.vLeftClass3.ID
                    SetClass3ListItem()
                '分類４(発生区分)ビュー表示切替
                Case Me.vLeftClass4.ID
                    SetClass4ListItem()
                '分類５(手配要否)ビュー表示切替
                Case Me.vLeftClass5.ID
                    SetClass5ListItem()
                '分類６(税区分)ビュー表示切替
                Case Me.vLeftClass6.ID
                    SetClass6ListItem()
                ''分類７(発生ACTY)ビュー表示切替
                'Case Me.vLeftClass7.ID
                '    SetClass7ListItem()
                '分類８(US$入力)ビュー表示切替
                Case Me.vLeftClass8.ID
                    SetClass8ListItem()
                '分類９(per B/L)ビュー表示切替
                Case Me.vLeftClass9.ID
                    SetClass9ListItem()
                '分類１０(per B/L)ビュー表示切替
                Case Me.vLeftClass10.ID
                    SetClass10ListItem()
                '表示非表示ビュー表示切替
                Case Me.vLeftShowHide.ID
                    SetShowHideListItem()
                'SOAビュー表示切替
                Case Me.vLeftSoa.ID
                    SetSOAListItem()
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

            '費用名称 絞込判定
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtCostNameEx.Text <> "") Then
                Dim searchStr As String = ""
                '検索用文字列（部分一致）
                If (COA0019Session.LANGDISP = C_LANG.JA) Then
                    searchStr = Convert.ToString(BASEtbl.Rows(i)("NAMESJP")).ToUpper
                Else
                    searchStr = Convert.ToString(BASEtbl.Rows(i)("NAMES")).ToUpper
                End If

                If Not searchStr.Contains(txtCostNameEx.Text.ToUpper) Then
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
                             & "   and COSTCODE = @P02 " _
                             & "   and LDKBN = @P04 " _
                             & "   and STYMD = @P03 " _
                             & "   and DELFLG <> '" & CONST_FLAG_YES & "' ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARA01 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA02 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA03 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                        Dim PARA04 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

                        PARA01.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARA02.Value = BASEtbl.Rows(i)("COSTCODE")
                        PARA03.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARA04.Value = BASEtbl.Rows(i)("LDKBN")

                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("COSTCODE")) = Convert.ToString(BASEtbl.Rows(i)("COSTCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("LDKBN")) = Convert.ToString(BASEtbl.Rows(i)("LDKBN")) AndAlso
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
                             & "   and COSTCODE = @P02 " _
                             & "   and LDKBN = @P04 " _
                             & "   and STYMD = @P03 " _
                             & "   and DELFLG <> '" & CONST_FLAG_YES & "' ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        Dim PARAM1 As SqlParameter = SQLcmd.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARAM2 As SqlParameter = SQLcmd.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARAM3 As SqlParameter = SQLcmd.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                        Dim PARAM4 As SqlParameter = SQLcmd.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

                        PARAM1.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARAM2.Value = BASEtbl.Rows(i)("COSTCODE")
                        PARAM3.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARAM4.Value = BASEtbl.Rows(i)("LDKBN")
                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("COSTCODE")) = Convert.ToString(BASEtbl.Rows(i)("COSTCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("LDKBN")) = Convert.ToString(BASEtbl.Rows(i)("LDKBN")) AndAlso
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

                    If (Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = updateDisp OrElse Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = "★" & updateDisp) Then

                        '削除は更新しない
                        If Convert.ToString(BASEtbl.Rows(i)("DELFLG")) = CONST_FLAG_YES AndAlso Convert.ToString(BASEtbl.Rows(i)("TIMSTP")) = "0" Then
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
                                 & "    AND COSTCODE = @P03  " _
                                 & "    AND LDKBN = @P41  " _
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
                                 & "        CLASS1 = @P06 , " _
                                 & "        CLASS2 = @P07 , " _
                                 & "        CLASS3 = @P08 , " _
                                 & "        CLASS4 = @P09 , " _
                                 & "        CLASS5 = @P10 , " _
                                 & "        CLASS6 = @P11 , " _
                                 & "        CLASS7 = @P12 , " _
                                 & "        CLASS8 = @P13 , " _
                                 & "        CLASS9 = @P14 , " _
                                 & "        CLASS10 = @P40 , " _
                                 & "        SALESBR = @P15 , " _
                                 & "        OPERATIONBR = @P16 , " _
                                 & "        REPAIRBR = @P17 , " _
                                 & "        SALES = @P18 , " _
                                 & "        BL = @P19 , " _
                                 & "        TANKOPE = @P20 , " _
                                 & "        NONBR = @P21 , " _
                                 & "        SOA = @P22 , " _
                                 & "        NAMES = @P23 , " _
                                 & "        NAMEL = @P24 , " _
                                 & "        NAMESJP = @P25 , " _
                                 & "        NAMELJP = @P26 , " _
                                 & "        SOACODE = @P27 , " _
                                 & "        DATA = @P42 , " _
                                 & "        JOTCODE = @P43 , " _
                                 & "        ACCODE = @P44 , " _
                                 & "        CRACCOUNT = @P28 , " _
                                 & "        DBACCOUNT = @P29 , " _
                                 & "        CRACCOUNTFORIGN = @P45 , " _
                                 & "        DBACCOUNTFORIGN = @P46 , " _
                                 & "        OFFCRACCOUNT = @P47 , " _
                                 & "        OFFDBACCOUNT = @P48 , " _
                                 & "        OFFCRACCOUNTFORIGN = @P49 , " _
                                 & "        OFFDBACCOUNTFORIGN = @P50 , " _
                                 & "        ACCAMPCODE = @P30 , " _
                                 & "        ACTORICODE = @P31 , " _
                                 & "        ACTORICODES = @P32 , " _
                                 & "        CRGENERALPURPOSE = @P51 , " _
                                 & "        DBGENERALPURPOSE = @P52 , " _
                                 & "        CRSEGMENT1 = @P53 , " _
                                 & "        DBSEGMENT1 = @P54 , " _
                                 & "        REMARK = @P33 , " _
                                 & "        DELFLG = @P34 , " _
                                 & "        UPDYMD = @P36 , " _
                                 & "        UPDUSER = @P37 , " _
                                 & "        UPDTERMID = @P38 , " _
                                 & "        RECEIVEYMD = @P39  " _
                                 & "  WHERE COMPCODE = @P02 " _
                                 & "    AND COSTCODE = @P03 " _
                                 & "    AND LDKBN = @P41 " _
                                 & "    AND STYMD = @P04 ; " _
                                 & " IF ( @@FETCH_STATUS <> 0 ) " _
                                 & "  INSERT INTO " & updTable _
                                 & "       ("
                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID , "
                        End If
                        SQLStr = SQLStr & " COMPCODE , " _
                                 & "        COSTCODE , " _
                                 & "        LDKBN , " _
                                 & "        STYMD , " _
                                 & "        ENDYMD , " _
                                 & "        CLASS1 , " _
                                 & "        CLASS2 , " _
                                 & "        CLASS3 , " _
                                 & "        CLASS4 , " _
                                 & "        CLASS5 , " _
                                 & "        CLASS6 , " _
                                 & "        CLASS7 , " _
                                 & "        CLASS8 , " _
                                 & "        CLASS9 , " _
                                 & "        CLASS10 , " _
                                 & "        SALESBR , " _
                                 & "        OPERATIONBR , " _
                                 & "        REPAIRBR , " _
                                 & "        SALES , " _
                                 & "        BL , " _
                                 & "        TANKOPE , " _
                                 & "        NONBR , " _
                                 & "        SOA , " _
                                 & "        NAMES , " _
                                 & "        NAMEL , " _
                                 & "        NAMESJP , " _
                                 & "        NAMELJP , " _
                                 & "        SOACODE , " _
                                 & "        DATA , " _
                                 & "        JOTCODE , " _
                                 & "        ACCODE , " _
                                 & "        CRACCOUNT , " _
                                 & "        DBACCOUNT , " _
                                 & "        CRACCOUNTFORIGN , " _
                                 & "        DBACCOUNTFORIGN , " _
                                 & "        OFFCRACCOUNT , " _
                                 & "        OFFDBACCOUNT , " _
                                 & "        OFFCRACCOUNTFORIGN , " _
                                 & "        OFFDBACCOUNTFORIGN , " _
                                 & "        ACCAMPCODE , " _
                                 & "        ACTORICODE , " _
                                 & "        ACTORICODES , " _
                                 & "        CRGENERALPURPOSE , " _
                                 & "        DBGENERALPURPOSE , " _
                                 & "        CRSEGMENT1 , " _
                                 & "        DBSEGMENT1 , " _
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
                        SQLStr = SQLStr & "    @P02,@P03,@P41,@P04,@P05,@P06,@P07,@P08,@P09,@P10,@P11, " _
                                 & "           @P12,@P13,@P14,@P40,@P15,@P16,@P17,@P18,@P19,@P20, " _
                                 & "           @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P42,@P43,@P44,@P28," _
                                 & "           @P29,@P45,@P46,@P47,@P48,@P49,@P50,@P30," _
                                 & "           @P31,@P32,@P51,@P52,@P53,@P54,@P33,@P34,@P35,@P36,@P37,@P38,@P39); " _
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
                        Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.NVarChar)
                        Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.DateTime)
                        Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.DateTime)
                        Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar)
                        Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.NVarChar)
                        Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.DateTime)
                        Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.NVarChar)
                        Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", System.Data.SqlDbType.NVarChar)

                        Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", System.Data.SqlDbType.NVarChar)
                        Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", System.Data.SqlDbType.NVarChar)
                        Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", System.Data.SqlDbType.NVarChar)

                        Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", System.Data.SqlDbType.NVarChar)
                        Dim PARA46 As SqlParameter = SQLcmd.Parameters.Add("@P46", System.Data.SqlDbType.NVarChar)
                        Dim PARA47 As SqlParameter = SQLcmd.Parameters.Add("@P47", System.Data.SqlDbType.NVarChar)
                        Dim PARA48 As SqlParameter = SQLcmd.Parameters.Add("@P48", System.Data.SqlDbType.NVarChar)
                        Dim PARA49 As SqlParameter = SQLcmd.Parameters.Add("@P49", System.Data.SqlDbType.NVarChar)
                        Dim PARA50 As SqlParameter = SQLcmd.Parameters.Add("@P50", System.Data.SqlDbType.NVarChar)

                        Dim PARA51 As SqlParameter = SQLcmd.Parameters.Add("@P51", System.Data.SqlDbType.NVarChar)
                        Dim PARA52 As SqlParameter = SQLcmd.Parameters.Add("@P52", System.Data.SqlDbType.NVarChar)
                        Dim PARA53 As SqlParameter = SQLcmd.Parameters.Add("@P53", System.Data.SqlDbType.NVarChar)
                        Dim PARA54 As SqlParameter = SQLcmd.Parameters.Add("@P54", System.Data.SqlDbType.NVarChar)

                        PARA01.Value = BASEtbl.Rows(i)("APPLYID")
                        PARA02.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARA03.Value = BASEtbl.Rows(i)("COSTCODE")
                        PARA41.Value = BASEtbl.Rows(i)("LDKBN")
                        PARA04.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARA05.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("ENDYMD")))
                        PARA06.Value = BASEtbl.Rows(i)("CLASS1")
                        PARA07.Value = BASEtbl.Rows(i)("CLASS2")
                        PARA08.Value = BASEtbl.Rows(i)("CLASS3")
                        PARA09.Value = BASEtbl.Rows(i)("CLASS4")
                        PARA10.Value = BASEtbl.Rows(i)("CLASS5")
                        PARA11.Value = BASEtbl.Rows(i)("CLASS6")
                        PARA12.Value = BASEtbl.Rows(i)("CLASS7")
                        PARA13.Value = BASEtbl.Rows(i)("CLASS8")
                        PARA14.Value = BASEtbl.Rows(i)("CLASS9")
                        PARA40.Value = BASEtbl.Rows(i)("CLASS10")
                        PARA15.Value = BASEtbl.Rows(i)("SALESBR")
                        PARA16.Value = BASEtbl.Rows(i)("OPERATIONBR")
                        PARA17.Value = BASEtbl.Rows(i)("REPAIRBR")
                        PARA18.Value = BASEtbl.Rows(i)("SALES")
                        PARA19.Value = BASEtbl.Rows(i)("BL")
                        PARA20.Value = BASEtbl.Rows(i)("TANKOPE")
                        PARA21.Value = BASEtbl.Rows(i)("NONBR")
                        PARA22.Value = BASEtbl.Rows(i)("SOA")
                        PARA23.Value = BASEtbl.Rows(i)("NAMES")
                        PARA24.Value = BASEtbl.Rows(i)("NAMEL")
                        PARA25.Value = BASEtbl.Rows(i)("NAMESJP")
                        PARA26.Value = BASEtbl.Rows(i)("NAMELJP")
                        PARA27.Value = BASEtbl.Rows(i)("SOACODE")
                        PARA42.Value = BASEtbl.Rows(i)("DATA")
                        PARA43.Value = BASEtbl.Rows(i)("JOTCODE")
                        PARA44.Value = BASEtbl.Rows(i)("ACCODE")
                        PARA28.Value = BASEtbl.Rows(i)("CRACCOUNT")
                        PARA29.Value = BASEtbl.Rows(i)("DBACCOUNT")
                        PARA45.Value = BASEtbl.Rows(i)("CRACCOUNTFORIGN")
                        PARA46.Value = BASEtbl.Rows(i)("DBACCOUNTFORIGN")
                        PARA47.Value = BASEtbl.Rows(i)("OFFCRACCOUNT")
                        PARA48.Value = BASEtbl.Rows(i)("OFFDBACCOUNT")
                        PARA49.Value = BASEtbl.Rows(i)("OFFCRACCOUNTFORIGN")
                        PARA50.Value = BASEtbl.Rows(i)("OFFDBACCOUNTFORIGN")
                        PARA30.Value = BASEtbl.Rows(i)("ACCAMPCODE")
                        PARA31.Value = BASEtbl.Rows(i)("ACTORICODE")
                        PARA32.Value = BASEtbl.Rows(i)("ACTORICODES")
                        PARA51.Value = BASEtbl.Rows(i)("CRGENERALPURPOSE")
                        PARA52.Value = BASEtbl.Rows(i)("DBGENERALPURPOSE")
                        PARA53.Value = BASEtbl.Rows(i)("CRSEGMENT1")
                        PARA54.Value = BASEtbl.Rows(i)("DBSEGMENT1")
                        PARA33.Value = BASEtbl.Rows(i)("REMARK")
                        PARA34.Value = BASEtbl.Rows(i)("DELFLG")
                        PARA35.Value = nowDate
                        PARA36.Value = nowDate
                        PARA37.Value = COA0019Session.USERID
                        PARA38.Value = HttpContext.Current.Session("APSRVname")
                        PARA39.Value = CONST_DEFAULT_RECEIVEYMD

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

                        COA0030Journal.ROW = copyDataTable.Rows(0)
                        COA0030Journal.COA0030SaveJournal()
                        If COA0030Journal.ERR = C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
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
                                & "   And COSTCODE = @P02 " _
                                & "   And LDKBN = @P04 " _
                                & "   And STYMD = @P03 ;"

                        SQLcmd2 = New SqlCommand(SQLStr2, SQLcon)
                        Dim PARA1 As SqlParameter = SQLcmd2.Parameters.Add("@P01", System.Data.SqlDbType.NVarChar)
                        Dim PARA2 As SqlParameter = SQLcmd2.Parameters.Add("@P02", System.Data.SqlDbType.NVarChar)
                        Dim PARA3 As SqlParameter = SQLcmd2.Parameters.Add("@P03", System.Data.SqlDbType.Date)
                        Dim PARA4 As SqlParameter = SQLcmd2.Parameters.Add("@P04", System.Data.SqlDbType.NVarChar)

                        PARA1.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARA2.Value = BASEtbl.Rows(i)("COSTCODE")
                        PARA3.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARA4.Value = BASEtbl.Rows(i)("LDKBN")

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

        BASEtblUpdate()

        '一覧(BASEtbl)内で、新規追加（タイムスタンプ０）かつ削除の場合はレコード削除
        Dim DelCnt As Integer = 0
        Dim LineCnt As Integer = 1
        Dim LoopCnt As Integer = BASEtbl.Rows.Count - 1
        Dim i As Integer = 0

        Do While i <= LoopCnt - DelCnt
            If Convert.ToString(BASEtbl.Rows(i)("TIMSTP")) = "0" AndAlso Convert.ToString(BASEtbl.Rows(i)("DELFLG")) = CONST_FLAG_YES Then
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
        COA0013TableObject.VARI = Me.hdnThisMapVariant.Value
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
        For i As Integer = INPtbl.Rows.Count - 1 To 1 Step -1
            'KEY重複
            If Convert.ToString(INPtbl.Rows(i)("COMPCODE")) = Convert.ToString(INPtbl.Rows(i - 1)("COMPCODE")) AndAlso
               Convert.ToString(INPtbl.Rows(i)("COSTCODE")) = Convert.ToString(INPtbl.Rows(i - 1)("COSTCODE")) AndAlso
               Convert.ToString(INPtbl.Rows(i)("LDKBN")) = Convert.ToString(INPtbl.Rows(i - 1)("LDKBN")) AndAlso
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
                    If Convert.ToString(BASEtbl.Rows(j)("DELFLG")) <> CONST_FLAG_YES Then
                        '日付以外の項目が等しい
                        If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(workInpRow("COMPCODE")) AndAlso
                           Convert.ToString(BASEtbl.Rows(j)("COSTCODE")) = Convert.ToString(workInpRow("COSTCODE")) AndAlso
                           Convert.ToString(BASEtbl.Rows(j)("LDKBN")) = Convert.ToString(workInpRow("LDKBN")) Then

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
            rtc &= ControlChars.NewLine & "  --> COST CODE       =" & Convert.ToString(argRow("COSTCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> L and D         =" & Convert.ToString(argRow("LDKBN")) & " , "
            rtc &= ControlChars.NewLine & "  --> EFFECTIVE(FROM) =" & Convert.ToString(argRow("STYMD")) & " , "
            rtc &= ControlChars.NewLine & "  --> DELETE FLG      =" & Convert.ToString(argRow("DELFLG")) & " "
        Else
            rtc &= ControlChars.NewLine & "  --> 会社コード      =" & Convert.ToString(argRow("COMPCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> 費用コード      =" & Convert.ToString(argRow("COSTCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> 発着区分        =" & Convert.ToString(argRow("LDKBN")) & " , "
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
                Case Me.vLeftLdKbn.ID 'アクティブなビューが発着区分
                    '発着区分選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbLdKbn.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbLdKbn.SelectedItem.Value
                            Me.lblLdKbnText.Text = Me.lbLdKbn.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblLdKbnText.Text = ""
                            txtobj.Focus()
                        End If
                    End If
                Case Me.vLeftClass1.ID 'アクティブなビューが分類１
                    '分類１選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類１
                        If Me.lbClass1.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass1.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass1.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass2.ID 'アクティブなビューが分類２(売上内訳)
                    '分類２(売上内訳)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類２
                        If Me.lbClass2.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass2.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass2.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass3.ID 'アクティブなビューが分類３(費用内訳)
                    '分類３(費用内訳)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類３
                        If Me.lbClass3.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass3.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass3.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass4.ID 'アクティブなビューが分類４(発生区分)
                    '分類４(発生区分)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類４
                        If Me.lbClass4.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass4.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass4.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass5.ID 'アクティブなビューが分類５(手配要否)
                    '分類５(手配要否)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類５
                        If Me.lbClass5.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass5.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass5.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass6.ID 'アクティブなビューが分類６(税区分)
                    '分類６(税区分)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類６
                        If Me.lbClass6.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass6.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass6.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass7.ID 'アクティブなビューが分類７(発生ACTY)
                    '分類７(発生ACTY)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類７
                        If Me.lbClass7.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass7.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass7.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass8.ID 'アクティブなビューが分類８(US$入力)
                    '分類８(US$入力)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類８
                        If Me.lbClass8.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass8.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass8.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass9.ID 'アクティブなビューが分類９(per B/L)
                    '分類９(per B/L)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類９
                        If Me.lbClass9.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass9.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass9.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftClass10.ID 'アクティブなビューが分類９(per B/L)
                    '分類９(per B/L)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類９
                        If Me.lbClass10.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbClass10.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_2"),
                                System.Web.UI.WebControls.Label).Text = Me.lbClass10.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_2").Focus()
                        End If
                    End If
                Case Me.vLeftShowHide.ID 'アクティブなビューが表示非表示
                    '表示非表示選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター表示非表示
                        If Me.lbShowHide.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_3"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbShowHide.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_3"),
                                System.Web.UI.WebControls.Label).Text = Me.lbShowHide.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_3").Focus()
                        End If
                    End If
                Case Me.vLeftSoa.ID 'SOAチェック
                    '分類９(per B/L)選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター分類９
                        If Me.lbSoa.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_3"),
                                System.Web.UI.WebControls.TextBox).Text = Me.lbSoa.SelectedItem.Value
                            DirectCast(WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_TEXT_3"),
                                System.Web.UI.WebControls.Label).Text = Me.lbSoa.SelectedItem.Text
                            WF_DViewRep1.Items(CInt(Me.hdnTextDbClickField.Value)).FindControl("WF_Rep1_VALUE_3").Focus()
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
        AddLangSetting(dicDisplayText, Me.lblCostNameEx, "費用名称", "Cost Name")

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
        AddLangSetting(dicDisplayText, Me.lblCostCode, "費用コード", "Cost Code")
        AddLangSetting(dicDisplayText, Me.lblLdKbn, "発着区分", "Loading and Discharging")

        AddLangSetting(dicDisplayText, Me.lblYMD, "有効年月日", "Effective Date")
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
            Me.lblDtabCharge.Text = "Cost Info"
            Me.lblDtabAccount.Text = "Account Info"
        Else
            Me.lblDtabCharge.Text = "費用項目情報"
            Me.lblDtabAccount.Text = "経理情報"
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
        table.Columns.Add("COMPCODE", GetType(String))
        table.Columns.Add("COSTCODE", GetType(String))
        table.Columns.Add("LDKBN", GetType(String))
        table.Columns.Add("STYMD", GetType(String))
        table.Columns.Add("ENDYMD", GetType(String))
        table.Columns.Add("CLASS1", GetType(String))
        table.Columns.Add("CLASS2", GetType(String))
        table.Columns.Add("CLASS3", GetType(String))
        table.Columns.Add("CLASS4", GetType(String))
        table.Columns.Add("CLASS5", GetType(String))
        table.Columns.Add("CLASS6", GetType(String))
        table.Columns.Add("CLASS7", GetType(String))
        table.Columns.Add("CLASS8", GetType(String))
        table.Columns.Add("CLASS9", GetType(String))
        table.Columns.Add("CLASS10", GetType(String))
        table.Columns.Add("SALESBR", GetType(String))
        table.Columns.Add("OPERATIONBR", GetType(String))
        table.Columns.Add("REPAIRBR", GetType(String))
        table.Columns.Add("SALES", GetType(String))
        table.Columns.Add("BL", GetType(String))
        table.Columns.Add("TANKOPE", GetType(String))
        table.Columns.Add("NONBR", GetType(String))
        table.Columns.Add("SOA", GetType(String))
        table.Columns.Add("NAMES", GetType(String))
        table.Columns.Add("NAMEL", GetType(String))
        table.Columns.Add("NAMESJP", GetType(String))
        table.Columns.Add("NAMELJP", GetType(String))
        table.Columns.Add("SOACODE", GetType(String))
        table.Columns.Add("DATA", GetType(String))
        table.Columns.Add("JOTCODE", GetType(String))
        table.Columns.Add("ACCODE", GetType(String))
        table.Columns.Add("CRACCOUNT", GetType(String))
        table.Columns.Add("DBACCOUNT", GetType(String))
        table.Columns.Add("CRACCOUNTFORIGN", GetType(String))
        table.Columns.Add("DBACCOUNTFORIGN", GetType(String))
        table.Columns.Add("OFFCRACCOUNT", GetType(String))
        table.Columns.Add("OFFDBACCOUNT", GetType(String))
        table.Columns.Add("OFFCRACCOUNTFORIGN", GetType(String))
        table.Columns.Add("OFFDBACCOUNTFORIGN", GetType(String))
        table.Columns.Add("ACCAMPCODE", GetType(String))
        table.Columns.Add("ACTORICODE", GetType(String))
        table.Columns.Add("ACTORICODES", GetType(String))
        table.Columns.Add("CRGENERALPURPOSE", GetType(String))
        table.Columns.Add("DBGENERALPURPOSE", GetType(String))
        table.Columns.Add("CRSEGMENT1", GetType(String))
        table.Columns.Add("DBSEGMENT1", GetType(String))
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
        workRow("COMPCODE") = HttpContext.Current.Session("APSRVCamp")
        workRow("COSTCODE") = ""
        workRow("LDKBN") = ""
        workRow("STYMD") = ""
        workRow("ENDYMD") = ""
        workRow("CLASS1") = ""
        workRow("CLASS2") = ""
        workRow("CLASS3") = ""
        workRow("CLASS4") = ""
        workRow("CLASS5") = ""
        workRow("CLASS6") = ""
        workRow("CLASS7") = ""
        workRow("CLASS8") = ""
        workRow("CLASS9") = ""
        workRow("CLASS10") = ""
        workRow("SALESBR") = ""
        workRow("OPERATIONBR") = ""
        workRow("REPAIRBR") = ""
        workRow("SALES") = ""
        workRow("BL") = ""
        workRow("TANKOPE") = ""
        workRow("NONBR") = ""
        workRow("SOA") = ""
        workRow("NAMES") = ""
        workRow("NAMEL") = ""
        workRow("NAMESJP") = ""
        workRow("NAMELJP") = ""
        workRow("SOACODE") = ""
        workRow("DATA") = ""
        workRow("JOTCODE") = ""
        workRow("ACCODE") = ""
        workRow("CRACCOUNT") = ""
        workRow("DBACCOUNT") = ""
        workRow("CRACCOUNTFORIGN") = ""
        workRow("DBACCOUNTFORIGN") = ""
        workRow("OFFCRACCOUNT") = ""
        workRow("OFFDBACCOUNT") = ""
        workRow("OFFCRACCOUNTFORIGN") = ""
        workRow("OFFDBACCOUNTFORIGN") = ""
        workRow("ACCAMPCODE") = ""
        workRow("ACTORICODE") = ""
        workRow("ACTORICODES") = ""
        workRow("CRGENERALPURPOSE") = ""
        workRow("DBGENERALPURPOSE") = ""
        workRow("CRSEGMENT1") = ""
        workRow("DBSEGMENT1") = ""
        workRow("REMARK") = ""
        workRow("DELFLG") = ""
        workRow("UPDYMD") = ""
        workRow("UPDUSER") = ""
        workRow("UPDTERMID") = ""

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
        COA0015ProfViewD.TAB = "CHARGECODE"
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
            workRow("COMPCODE") = HttpContext.Current.Session("APSRVCamp")
            workRow("COSTCODE") = txtCostCode.Text
            workRow("LDKBN") = txtLdKbn.Text
            workRow("STYMD") = FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT)
            workRow("ENDYMD") = FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT)
            workRow("CLASS1") = ""
            workRow("CLASS2") = ""
            workRow("CLASS3") = ""
            workRow("CLASS4") = ""
            workRow("CLASS5") = ""
            workRow("CLASS6") = ""
            workRow("CLASS7") = ""
            workRow("CLASS8") = ""
            workRow("CLASS9") = ""
            workRow("CLASS10") = ""
            workRow("SALESBR") = ""
            workRow("OPERATIONBR") = ""
            workRow("REPAIRBR") = ""
            workRow("SALES") = ""
            workRow("BL") = ""
            workRow("TANKOPE") = ""
            workRow("NONBR") = ""
            workRow("SOA") = ""
            workRow("NAMES") = ""
            workRow("NAMEL") = ""
            workRow("NAMESJP") = ""
            workRow("NAMELJP") = ""
            workRow("SOACODE") = ""
            workRow("DATA") = ""
            workRow("JOTCODE") = ""
            workRow("ACCODE") = ""
            workRow("CRACCOUNT") = ""
            workRow("DBACCOUNT") = ""
            workRow("CRACCOUNTFORIGN") = ""
            workRow("DBACCOUNTFORIGN") = ""
            workRow("OFFCRACCOUNT") = ""
            workRow("OFFDBACCOUNT") = ""
            workRow("OFFCRACCOUNTFORIGN") = ""
            workRow("OFFDBACCOUNTFORIGN") = ""
            workRow("ACCAMPCODE") = ""
            workRow("ACTORICODE") = ""
            workRow("ACTORICODES") = ""
            workRow("CRGENERALPURPOSE") = ""
            workRow("DBGENERALPURPOSE") = ""
            workRow("CRSEGMENT1") = ""
            workRow("DBSEGMENT1") = ""
            workRow("REMARK") = ""
            workRow("DELFLG") = txtDelFlg.Text
            workRow("UPDYMD") = ""
            INPtbl.Rows.Add(workRow)
        Next

        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = Me.hdnViewId.Value
        COA0014DetailView.TABID = "CHARGECODE"
        COA0014DetailView.SRCDATA = INPtbl
        COA0014DetailView.REPEATER = WF_DViewRep1
        COA0014DetailView.COLPREFIX = "WF_Rep1_"
        COA0014DetailView.COA0014ReadDetailView()

        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = Me.hdnViewId.Value
        COA0014DetailView.TABID = "ACCOUNT"
        COA0014DetailView.SRCDATA = INPtbl
        COA0014DetailView.REPEATER = WF_DViewRep2
        COA0014DetailView.COLPREFIX = "WF_Rep2_"
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
        COA0014DetailView.TABID = "CHARGECODE"
        COA0014DetailView.SRCDATA = dataTable
        COA0014DetailView.REPEATER = WF_DViewRep1
        COA0014DetailView.COLPREFIX = "WF_Rep1_"
        COA0014DetailView.COA0014SetDetailView()

        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = Me.hdnViewId.Value
        COA0014DetailView.TABID = "ACCOUNT"
        COA0014DetailView.SRCDATA = dataTable
        COA0014DetailView.REPEATER = WF_DViewRep2
        COA0014DetailView.COLPREFIX = "WF_Rep2_"
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

            For i As Integer = 0 To WF_DViewRep2.Items.Count - 1
                If fieldList.IndexOf(DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_1"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName1 = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_1"), System.Web.UI.WebControls.Label)
                    If repName1.CssClass = "" Then
                        repName1.CssClass = "requiredMark2"
                    Else
                        repName1.CssClass = repName1.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_2"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName2 = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_2"), System.Web.UI.WebControls.Label)
                    If repName2.CssClass = "" Then
                        repName2.CssClass = "requiredMark2"
                    Else
                        repName2.CssClass = repName2.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_3"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName3 = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_3"), System.Web.UI.WebControls.Label)
                    If repName3.CssClass = "" Then
                        repName3.CssClass = "requiredMark2"
                    Else
                        repName3.CssClass = repName3.CssClass & " " & "requiredMark2"
                    End If
                End If
            Next
        End If

        WF_DetailMView.ActiveViewIndex = 0

        lblDtabCharge.Style.Remove("color")
        lblDtabCharge.Style.Add("color", "blue")
        lblDtabCharge.Style.Remove("background-color")
        lblDtabCharge.Style.Add("background-color", "rgb(220,230,240)")
        lblDtabCharge.Style.Remove("border")
        lblDtabCharge.Style.Add("border", "1px solid blue")
        lblDtabCharge.Style.Remove("font-weight")
        lblDtabCharge.Style.Add("font-weight", "bold")

        'Detail設定処理
        SetDetailDbClick()

        dataTable.Dispose()
        dataTable = Nothing

        '名称設定
        CLASS1_Change()
        CLASS2_Change()
        CLASS3_Change()
        CLASS4_Change()
        CLASS5_Change()
        CLASS6_Change()
        'CLASS7_Change()
        CLASS8_Change()
        CLASS9_Change()
        CLASS10_Change()
        SALESBR_Change()
        OPERATIONBR_Change()
        REPAIRBR_Change()
        SALES_Change()
        BL_Change()
        TANKOPE_Change()
        NONBR_Change()
        SOA_Change()

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

        For i As Integer = 0 To WF_DViewRep2.Items.Count - 1
            If fieldList.Count > 0 Then
                If fieldList.IndexOf(DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_1"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName1 = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_1"), System.Web.UI.WebControls.Label)
                    If repName1.CssClass = "" Then
                        repName1.CssClass = "requiredMark2"
                    Else
                        repName1.CssClass = repName1.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_2"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName2 = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_2"), System.Web.UI.WebControls.Label)
                    If repName2.CssClass = "" Then
                        repName2.CssClass = "requiredMark2"
                    Else
                        repName2.CssClass = repName2.CssClass & " " & "requiredMark2"
                    End If
                End If
                If fieldList.IndexOf(DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_3"), System.Web.UI.WebControls.Label).Text) <> -1 Then
                    repName3 = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_3"), System.Web.UI.WebControls.Label)
                    If repName3.CssClass = "" Then
                        repName3.CssClass = "requiredMark2"
                    Else
                        repName3.CssClass = repName3.CssClass & " " & "requiredMark2"
                    End If
                End If
            End If

            'ダブルクリック時コード検索イベント追加
            If DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_1"), System.Web.UI.WebControls.Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_1"), System.Web.UI.WebControls.Label)
                repValue = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_VALUE_1"), System.Web.UI.WebControls.TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_1"), System.Web.UI.WebControls.Label)
                    repName.Attributes.Remove("style")
                    repName.Attributes.Add("style", "text-decoration: underline;")
                End If
            End If

            If DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_2"), System.Web.UI.WebControls.Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_2"), System.Web.UI.WebControls.Label)
                repValue = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_VALUE_2"), System.Web.UI.WebControls.TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_2"), System.Web.UI.WebControls.Label)
                    repName.Attributes.Remove("style")
                    repName.Attributes.Add("style", "text-decoration: underline;")
                End If
            End If

            If DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_3"), System.Web.UI.WebControls.Label).Text <> "" Then
                repField = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELD_3"), System.Web.UI.WebControls.Label)
                repValue = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_VALUE_3"), System.Web.UI.WebControls.TextBox)
                GetAttributes(repField.Text, repAttr)
                If repAttr <> "" AndAlso repValue.ReadOnly = False Then
                    repValue.Attributes.Remove("ondblclick")
                    repValue.Attributes.Add("ondblclick", repAttr)
                    repName = DirectCast(WF_DViewRep2.Items(i).FindControl("WF_Rep2_FIELDNM_3"), System.Web.UI.WebControls.Label)
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
            Case "CLASS1"
                '分類１
                repAttr = "Field_DBclick('vLeftClass1', '0');"
            Case "CLASS2"
                '分類２(売上内訳)
                repAttr = "Field_DBclick('vLeftClass2', '1');"
            Case "CLASS3"
                '分類３(費用内訳)
                repAttr = "Field_DBclick('vLeftClass3', '2');"
            Case "CLASS4"
                '分類４(発生区分)
                repAttr = "Field_DBclick('vLeftClass4', '3');"
            Case "CLASS5"
                '分類５(手配要否)
                repAttr = "Field_DBclick('vLeftClass5', '4');"
            Case "CLASS6"
                '分類６(税区分)
                repAttr = "Field_DBclick('vLeftClass6', '5');"
            'Case "CLASS7"
            '    '分類７(発生ACTY)
            '    repAttr = "Field_DBclick('vLeftClass7', '6');"
            Case "CLASS8"
                '分類８(US$入力)
                repAttr = "Field_DBclick('vLeftClass8', '7');"
            Case "CLASS9"
                '分類９(per B/L)
                repAttr = "Field_DBclick('vLeftClass9', '8');"
            Case "CLASS10"
                '分類１０(デマレッジ終端費用コード)
                repAttr = "Field_DBclick('vLeftClass10', '9');"
            Case "SALESBR"
                'セールスBR
                repAttr = "Field_DBclick('vLeftShowHide', '0');"
            Case "OPERATIONBR"
                '移動BR
                repAttr = "Field_DBclick('vLeftShowHide', '1');"
            Case "REPAIRBR"
                '修理BR
                repAttr = "Field_DBclick('vLeftShowHide', '2');"
            Case "SALES"
                '受注
                repAttr = "Field_DBclick('vLeftShowHide', '3');"
            Case "BL"
                'BL
                repAttr = "Field_DBclick('vLeftShowHide', '4');"
            Case "TANKOPE"
                '手配
                repAttr = "Field_DBclick('vLeftShowHide', '5');"
            Case "NONBR"
                'その他経費
                repAttr = "Field_DBclick('vLeftShowHide', '6');"
            Case "SOA"
                '精算
                repAttr = "Field_DBclick('vLeftSoa', '7');"
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

        '入力項目チェック
        '①単項目チェック

        'カラム情報取得
        dicField = New Dictionary(Of String, String)
        CheckSingle(InpRow, dicField, escapeFlg)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '②存在チェック(LeftBoxチェック)
        '会社コード
        SetCompCodeListItem(Convert.ToString(InpRow("COMPCODE")))
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

        '発着区分
        SetLdKbnListItem()
        ChedckList(Convert.ToString(InpRow("LDKBN")), lbLdKbn, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("LDKBN") & ":" & Convert.ToString(InpRow("LDKBN")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '分類１
        SetClass1ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS1")), lbClass1, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS1") & ":" & Convert.ToString(InpRow("CLASS1")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '分類２
        SetClass2ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS2")), lbClass2, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS2") & ":" & Convert.ToString(InpRow("CLASS2")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '分類３
        SetClass3ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS3")), lbClass3, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS3") & ":" & Convert.ToString(InpRow("CLASS3")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '分類４
        SetClass4ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS4")), lbClass4, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS4") & ":" & Convert.ToString(InpRow("CLASS4")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '分類５
        SetClass5ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS5")), lbClass5, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS5") & ":" & Convert.ToString(InpRow("CLASS5")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '分類６
        SetClass6ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS6")), lbClass6, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS6") & ":" & Convert.ToString(InpRow("CLASS6")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        ''分類７
        'SetClass7ListItem()
        'ChedckList(Convert.ToString(InpRow("CLASS7")), lbClass7, refErrMessage)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    errMessageStr = Me.ErrItemSet(InpRow)
        '    If txtRightErrorMessage.Text <> "" Then
        '        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
        '    End If
        '    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
        '                                   & "・" & refErrMessage & "(" & dicField("CLASS7") & ":" & Convert.ToString(InpRow("CLASS7")) & ")" & errMessageStr
        '    errFlg = True
        '    returnCode = C_MESSAGENO.NORMAL
        'End If

        '分類８
        SetClass8ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS8")), lbClass8, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS8") & ":" & Convert.ToString(InpRow("CLASS8")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '分類９
        SetClass9ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS9")), lbClass9, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS9") & ":" & Convert.ToString(InpRow("CLASS9")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '分類１０
        SetClass10ListItem()
        ChedckList(Convert.ToString(InpRow("CLASS10")), lbClass10, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CLASS10") & ":" & Convert.ToString(InpRow("CLASS10")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'セールスBR
        SetShowHideListItem()
        ChedckList(Convert.ToString(InpRow("SALESBR")), lbShowHide, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("SALESBR") & ":" & Convert.ToString(InpRow("SALESBR")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '移動BR
        SetShowHideListItem()
        ChedckList(Convert.ToString(InpRow("OPERATIONBR")), lbShowHide, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("OPERATIONBR") & ":" & Convert.ToString(InpRow("OPERATIONBR")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '修理BR
        SetShowHideListItem()
        ChedckList(Convert.ToString(InpRow("REPAIRBR")), lbShowHide, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("REPAIRBR") & ":" & Convert.ToString(InpRow("REPAIRBR")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '受注
        SetShowHideListItem()
        ChedckList(Convert.ToString(InpRow("SALES")), lbShowHide, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("SALES") & ":" & Convert.ToString(InpRow("SALES")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'BL
        SetShowHideListItem()
        ChedckList(Convert.ToString(InpRow("BL")), lbShowHide, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("BL") & ":" & Convert.ToString(InpRow("BL")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '手配
        SetShowHideListItem()
        ChedckList(Convert.ToString(InpRow("TANKOPE")), lbShowHide, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("TANKOPE") & ":" & Convert.ToString(InpRow("TANKOPE")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        'その他経費
        SetShowHideListItem()
        ChedckList(Convert.ToString(InpRow("NONBR")), lbShowHide, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("NONBR") & ":" & Convert.ToString(InpRow("NONBR")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '精算
        SetSOAListItem()
        ChedckList(Convert.ToString(InpRow("SOA")), lbSoa, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("SOA") & ":" & Convert.ToString(InpRow("SOA")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '削除フラグ
        SetDelFlgListItem(Convert.ToString(InpRow("DELFLG")))
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

            '入力文字置き換え
            '画面PassWord内の使用禁止文字排除
            COA0008InvalidChar.CHARin = Convert.ToString(argRow(itm.Key))
            COA0008InvalidChar.COA0008RemoveInvalidChar()
            If COA0008InvalidChar.CHARin <> COA0008InvalidChar.CHARout Then
                argRow(itm.Key) = COA0008InvalidChar.CHARout
            End If

            '単項目チェック
            COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
            COA0026FieldCheck.MAPID = CONST_MAPID
            COA0026FieldCheck.FIELD = itm.Key
            COA0026FieldCheck.VALUE = Convert.ToString(argRow(itm.Key))
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
            PARA3.Value = CONST_FLAG_YES
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
    ''' 発着区分リストアイテムを設定
    ''' </summary>
    Private Sub SetLdKbnListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbLdKbn.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "LDKBN"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbLdKbn
        Else
            COA0017FixValue.LISTBOX2 = Me.lbLdKbn
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbLdKbn = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbLdKbn = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類１リストアイテムを設定
    ''' </summary>
    Private Sub SetClass1ListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbClass1.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS1"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass1
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass1
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass1 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass1 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類２(売上内訳)リストアイテムを設定
    ''' </summary>
    Private Sub SetClass2ListItem()
        Dim COA0017FixValue As New COA0017FixValue
        'リストクリア
        Me.lbClass2.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS2"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass2
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass2
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass2 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass2 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類３(費用内訳)リストアイテムを設定
    ''' </summary>
    Private Sub SetClass3ListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbClass3.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS3"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass3
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass3
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass3 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass3 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類４(発生区分)リストアイテムを設定
    ''' </summary>
    Private Sub SetClass4ListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbClass4.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS4"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass4
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass4
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass4 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass4 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類５(手配要否)リストアイテムを設定
    ''' </summary>
    Private Sub SetClass5ListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbClass5.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS5"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass5
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass5
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass5 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass5 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類６(税区分)リストアイテムを設定
    ''' </summary>
    Private Sub SetClass6ListItem()

        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbClass6.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS6"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass6
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass6
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass6 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass6 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    '''' 分類７(発生ACTY)リストアイテムを設定
    '''' </summary>
    'Private Sub SetClass7ListItem()

    '    Dim COA0017FixValue As New COA0017FixValue

    '    'リストクリア
    '    Me.lbClass7.Items.Clear()

    '    'リスト設定
    '    COA0017FixValue.COMPCODE = GBC_COMPCODE_D
    '    COA0017FixValue.CLAS = "CHARGECLASS7"
    '    If COA0019Session.LANGDISP = C_LANG.JA Then
    '        COA0017FixValue.LISTBOX1 = Me.lbClass7
    '    Else
    '        COA0017FixValue.LISTBOX2 = Me.lbClass7
    '    End If

    '    COA0017FixValue.COA0017getListFixValue()
    '    If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

    '        If COA0019Session.LANGDISP = C_LANG.JA Then
    '            Me.lbClass7 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
    '        Else
    '            Me.lbClass7 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類８(US$入力)リストアイテムを設定
    ''' </summary>
    Private Sub SetClass8ListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbClass8.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS8"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass8
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass8
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass8 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass8 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類９(per B/L)リストアイテムを設定
    ''' </summary>
    Private Sub SetClass9ListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbClass9.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS9"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass9
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass9
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass9 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass9 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 分類１０(デマレッジ終端費用コード)リストアイテムを設定
    ''' </summary>
    Private Sub SetClass10ListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbClass10.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CHARGECLASS10"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbClass10
        Else
            COA0017FixValue.LISTBOX2 = Me.lbClass10
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbClass10 = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbClass10 = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' 表示非表示リストアイテムを設定
    ''' </summary>
    Private Sub SetShowHideListItem()
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbShowHide.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "SHOWHIDE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbShowHide
        Else
            COA0017FixValue.LISTBOX2 = Me.lbShowHide
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbShowHide = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbShowHide = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
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
    ''' SOA)リストアイテムを設定
    ''' </summary>
    Private Sub SetSOAListItem()
        'DataBase接続文字
        Dim SQLcon As New SqlConnection(COA0019Session.DBcon)
        Dim SQLStr As String = Nothing
        Dim SQLcmd As New SqlCommand()
        Dim SQLdr As SqlDataReader = Nothing

        Try

            'リストクリア
            Me.lbSoa.Items.Clear()

            'DataBase接続(Open)
            SQLcon.Open()

            '検索SQL文(AGENTSOA)
            SQLStr =
                 "SELECT VALUE3 AS CODE, VALUE1 AS NAMES, VALUE2 AS NAMES_EN " _
               & " FROM  COS0017_FIXVALUE " _
               & " Where COMPCODE = @P1" _
               & "   and SYSCODE  = @P2" _
               & "   and CLASS    = @P3" _
               & "   And STYMD   <= @P4 " _
               & "   and ENDYMD  >= @P5 " _
               & "   and DELFLG  <> @P6 "
            SQLcmd = New SqlCommand(SQLStr, SQLcon)
            With SQLcmd.Parameters
                .Add("@P1", System.Data.SqlDbType.NVarChar).Value = GBC_COMPCODE_D
                .Add("@P2", System.Data.SqlDbType.NVarChar).Value = C_SYSCODE_GB
                .Add("@P3", System.Data.SqlDbType.NVarChar).Value = "AGENTSOA"
                .Add("@P4", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@P5", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@P6", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            SQLdr = SQLcmd.ExecuteReader()

            While SQLdr.Read
                'DBからアイテムを設定
                If COA0019Session.LANGDISP = C_LANG.JA Then
                    Me.lbSoa.Items.Add(New ListItem(Convert.ToString(SQLdr("NAMES")), Convert.ToString(SQLdr("CODE"))))
                Else
                    Me.lbSoa.Items.Add(New ListItem(Convert.ToString(SQLdr("NAMES_EN")), Convert.ToString(SQLdr("CODE"))))
                End If
            End While
            Me.lbSoa.Items.Add(New ListItem("None", "0"))
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
    ''' 会社コード名設定
    ''' </summary>
    Public Sub txtLdKbn_Change()

        Try
            Me.lblLdKbnText.Text = ""

            SetLdKbnListItem()
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbLdKbn.Items.Count > 0 Then
                Dim findListItem = Me.lbLdKbn.Items.FindByValue(Me.txtLdKbn.Text)
                If findListItem IsNot Nothing Then
                    Me.lblLdKbnText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbLdKbn.Items.FindByValue(Me.txtLdKbn.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblLdKbnText.Text = findListItemUpper.Text
                        Me.txtLdKbn.Text = findListItemUpper.Value
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
    ''' 分類１名設定
    ''' </summary>
    Public Sub CLASS1_Change()

        Try

            'リピーター分類１
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS1" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass1ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass1.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass1.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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
    ''' 分類２(売上内訳)名設定
    ''' </summary>
    Public Sub CLASS2_Change()

        Try
            'リピーター分類２
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS2" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass2ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass2.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass2.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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
    ''' 分類３(費用内訳)名設定
    ''' </summary>
    Public Sub CLASS3_Change()

        Try
            'リピーター分類３
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS3" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass3ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass3.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass3.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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
    ''' 分類４(発生区分)名設定
    ''' </summary>
    Public Sub CLASS4_Change()

        Try
            'リピーター分類４
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS4" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass4ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass4.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass4.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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
    ''' 分類５(手配要否)名設定
    ''' </summary>
    Public Sub CLASS5_Change()

        Try
            'リピーター分類５
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS5" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass5ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass5.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass5.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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
    ''' 分類６(税区分)名設定
    ''' </summary>
    Public Sub CLASS6_Change()

        Try
            'リピーター分類６
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS6" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass6ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass6.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass6.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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

    '''' <summary>
    '''' 分類７(発生ACTY)名設定
    '''' </summary>
    'Public Sub CLASS7_Change()

    '    Try
    '        'リピーター分類７
    '        For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

    '            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS7" Then
    '                '名称削除
    '                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

    '                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

    '                    SetClass7ListItem()
    '                    If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass7.Items.Count > 0 Then
    '                        Dim findListItem = Me.lbClass7.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
    '                                                                                                System.Web.UI.WebControls.TextBox).Text)
    '                        If findListItem IsNot Nothing Then
    '                            DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"),
    '                                    System.Web.UI.WebControls.Label).Text = findListItem.Text
    '                        End If
    '                    End If
    '                End If
    '            End If
    '        Next

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
    ''' 分類８(US$入力)名設定
    ''' </summary>
    Public Sub CLASS8_Change()

        Try
            'リピーター分類８
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS8" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass8ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass8.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass8.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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
    ''' 分類９(per B/L)名設定
    ''' </summary>
    Public Sub CLASS9_Change()

        Try
            'リピーター分類９
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS9" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass9ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass9.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass9.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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
    ''' 分類１０(デマレッジ終端費用コード)名設定
    ''' </summary>
    Public Sub CLASS10_Change()

        Try
            'リピーター分類１０
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_2"), System.Web.UI.WebControls.Label).Text = "CLASS10" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_2"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetClass10ListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbClass10.Items.Count > 0 Then
                            Dim findListItem = Me.lbClass10.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_2"),
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
    ''' セールスBR名設定
    ''' </summary>
    Public Sub SALESBR_Change()

        Try
            'リピーターセールスBR
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "SALESBR" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetShowHideListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShowHide.Items.Count > 0 Then
                            Dim findListItem = Me.lbShowHide.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
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
    ''' 移動BR名設定
    ''' </summary>
    Public Sub OPERATIONBR_Change()

        Try
            'リピーター移動BR
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "OPERATIONBR" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetShowHideListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShowHide.Items.Count > 0 Then
                            Dim findListItem = Me.lbShowHide.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
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
    ''' 修理BR名設定
    ''' </summary>
    Public Sub REPAIRBR_Change()

        Try
            'リピーター修理BR
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "REPAIRBR" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetShowHideListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShowHide.Items.Count > 0 Then
                            Dim findListItem = Me.lbShowHide.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
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
    ''' 受注名設定
    ''' </summary>
    Public Sub SALES_Change()

        Try
            'リピーター受注
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "SALES" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetShowHideListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShowHide.Items.Count > 0 Then
                            Dim findListItem = Me.lbShowHide.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
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
    ''' BL名設定
    ''' </summary>
    Public Sub BL_Change()

        Try
            'リピーターBL
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "BL" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetShowHideListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShowHide.Items.Count > 0 Then
                            Dim findListItem = Me.lbShowHide.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
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
    ''' 手配名設定
    ''' </summary>
    Public Sub TANKOPE_Change()

        Try
            'リピーター手配
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "TANKOPE" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetShowHideListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShowHide.Items.Count > 0 Then
                            Dim findListItem = Me.lbShowHide.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
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
    ''' その他経費名設定
    ''' </summary>
    Public Sub NONBR_Change()

        Try
            'リピーターその他経費
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "NONBR" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetShowHideListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbShowHide.Items.Count > 0 Then
                            Dim findListItem = Me.lbShowHide.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
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
    ''' 精算名設定
    ''' </summary>
    Public Sub SOA_Change()

        Try
            'リピーター精算
            For i As Integer = 0 To WF_DViewRep1.Items.Count - 1

                If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "SOA" Then
                    '名称削除
                    DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_TEXT_3"), System.Web.UI.WebControls.Label).Text = ""

                    If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text <> "" Then

                        SetSOAListItem()
                        If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbSoa.Items.Count > 0 Then
                            Dim findListItem = Me.lbSoa.Items.FindByValue(DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"),
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
        txtCompCode.Text = ""
        lblCompCodeText.Text = ""
        txtCostCode.Text = ""
        lblCostCodeText.Text = ""
        txtLdKbn.Text = ""
        lblLdKbnText.Text = ""
        txtStYMD.Text = ""
        txtEndYMD.Text = ""
        txtDelFlg.Text = ""
        lblDelFlgText.Text = ""

        'ボタン制御
        SetButtonControl()

        'Detail初期設定
        detailboxInit()

        'フォーカス設定
        txtStYMD.Focus()

        INPtbl.Clear()
        INPtbl.Dispose()

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

        Dim compareUpdTargetFieldList = CommonFunctions.CreateCompareFieldList({"COMPCODE", "COSTCODE", "LDKBN", "STYMD"})
        Dim compareModFieldList = CommonFunctions.CreateCompareFieldList({"ENDYMD", "CLASS1", "CLASS2", "CLASS3", "CLASS4", "CLASS5", "CLASS6",
                                                                                  "CLASS7", "CLASS8", "CLASS9", "CLASS10", "SALESBR", "OPERATIONBR",
                                                                                  "REPAIRBR", "SALES", "BL", "TANKOPE", "NONBR", "SOA", "NAMES", "NAMEL",
                                                                                  "NAMESJP", "NAMELJP", "SOACODE", "DATA", "JOTCODE", "ACCODE", "CRACCOUNT", "DBACCOUNT",
                                                                                  "CRACCOUNTFORIGN", "DBACCOUNTFORIGN", "OFFCRACCOUNT", "OFFDBACCOUNT", "OFFCRACCOUNTFORIGN", "OFFDBACCOUNTFORIGN", "ACCAMPCODE",
                                                                                  "ACTORICODE", "ACTORICODES", "CRGENERALPURPOSE", "DBGENERALPURPOSE", "CRSEGMENT1", "DBSEGMENT1", "REMARK", "DELFLG"})

        For i As Integer = 0 To INPtbl.Rows.Count - 1

            If Convert.ToString(INPtbl(i)("HIDDEN")) <> "1" Then ' "1" ・・・取り込み対象外エラー

                Dim workBasePos As Integer = -1
                newFlg = False
                '内部テーブル検索
                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                    Dim workBaseRow As DataRow
                    workBaseRow = BASEtbl.NewRow
                    workBaseRow.ItemArray = BASEtbl.Rows(j).ItemArray

                    ' 更新対象検索
                    If CommonFunctions.CompareDataFields(workBaseRow, INPtbl(i), compareUpdTargetFieldList) Then

                        ' 変更なし  
                        If Convert.ToString(INPtbl(i)("OPERATION")) <> errDisp AndAlso
                           CommonFunctions.CompareDataFields(workBaseRow, INPtbl(i), compareModFieldList) Then
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
                    newFlg = True

                End If

                ' 内部テーブル編集
                If workBasePos >= 0 Then

                    '内部テーブル検索
                    For k As Integer = 0 To BASEtbl.Rows.Count - 1

                        Dim workBaseRow2 As DataRow
                        workBaseRow2 = BASEtbl.NewRow
                        workBaseRow2.ItemArray = BASEtbl.Rows(k).ItemArray

                        If CommonFunctions.CompareDataFields(workBaseRow2, INPtbl(i), compareUpdTargetFieldList) Then

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
                                If txtRightErrorMessage.Text <> "" Then
                                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                                End If
                                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr
                                'endFlg = True

                                INPtbl(i)("OPERATION") = errDisp
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
                    If Convert.ToString(INPtbl(i)("OPERATION")) <> errDisp Then
                        workBaseRow("OPERATION") = updateDisp
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

                    'エラーの場合、値を更新しない
                    'エラーかつ新規の場合、値を設定する
                    If Convert.ToString(workBaseRow("OPERATION")) <> errDisp OrElse
                        (Convert.ToString(workBaseRow("OPERATION")) = errDisp AndAlso newFlg) Then
                        '個別項目
                        workBaseRow("COMPCODE") = INPtbl(i)("COMPCODE")
                        workBaseRow("COSTCODE") = INPtbl(i)("COSTCODE")
                        workBaseRow("LDKBN") = INPtbl(i)("LDKBN")
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
                        workBaseRow("CLASS1") = INPtbl(i)("CLASS1")
                        workBaseRow("CLASS2") = INPtbl(i)("CLASS2")
                        workBaseRow("CLASS3") = INPtbl(i)("CLASS3")
                        workBaseRow("CLASS4") = INPtbl(i)("CLASS4")
                        workBaseRow("CLASS5") = INPtbl(i)("CLASS5")
                        workBaseRow("CLASS6") = INPtbl(i)("CLASS6")
                        workBaseRow("CLASS7") = INPtbl(i)("CLASS7")
                        workBaseRow("CLASS8") = INPtbl(i)("CLASS8")
                        workBaseRow("CLASS9") = INPtbl(i)("CLASS9")
                        workBaseRow("CLASS10") = INPtbl(i)("CLASS10")
                        workBaseRow("SALESBR") = INPtbl(i)("SALESBR")
                        workBaseRow("OPERATIONBR") = INPtbl(i)("OPERATIONBR")
                        workBaseRow("REPAIRBR") = INPtbl(i)("REPAIRBR")
                        workBaseRow("SALES") = INPtbl(i)("SALES")
                        workBaseRow("BL") = INPtbl(i)("BL")
                        workBaseRow("TANKOPE") = INPtbl(i)("TANKOPE")
                        workBaseRow("NONBR") = INPtbl(i)("NONBR")
                        workBaseRow("SOA") = INPtbl(i)("SOA")
                        workBaseRow("NAMES") = INPtbl(i)("NAMES")
                        workBaseRow("NAMEL") = INPtbl(i)("NAMEL")
                        workBaseRow("NAMESJP") = INPtbl(i)("NAMESJP")
                        workBaseRow("NAMELJP") = INPtbl(i)("NAMELJP")
                        workBaseRow("SOACODE") = INPtbl(i)("SOACODE")
                        workBaseRow("DATA") = INPtbl(i)("DATA")
                        workBaseRow("JOTCODE") = INPtbl(i)("JOTCODE")
                        workBaseRow("ACCODE") = INPtbl(i)("ACCODE")
                        workBaseRow("CRACCOUNT") = INPtbl(i)("CRACCOUNT")
                        workBaseRow("DBACCOUNT") = INPtbl(i)("DBACCOUNT")
                        workBaseRow("CRACCOUNTFORIGN") = INPtbl(i)("CRACCOUNTFORIGN")
                        workBaseRow("DBACCOUNTFORIGN") = INPtbl(i)("DBACCOUNTFORIGN")
                        workBaseRow("OFFCRACCOUNT") = INPtbl(i)("OFFCRACCOUNT")
                        workBaseRow("OFFDBACCOUNT") = INPtbl(i)("OFFDBACCOUNT")
                        workBaseRow("OFFCRACCOUNTFORIGN") = INPtbl(i)("OFFCRACCOUNTFORIGN")
                        workBaseRow("OFFDBACCOUNTFORIGN") = INPtbl(i)("OFFDBACCOUNTFORIGN")
                        workBaseRow("ACCAMPCODE") = INPtbl(i)("ACCAMPCODE")
                        workBaseRow("ACTORICODE") = INPtbl(i)("ACTORICODE")
                        workBaseRow("ACTORICODES") = INPtbl(i)("ACTORICODES")
                        workBaseRow("CRGENERALPURPOSE") = INPtbl(i)("CRGENERALPURPOSE")
                        workBaseRow("DBGENERALPURPOSE") = INPtbl(i)("DBGENERALPURPOSE")
                        workBaseRow("CRSEGMENT1") = INPtbl(i)("CRSEGMENT1")
                        workBaseRow("DBSEGMENT1") = INPtbl(i)("DBSEGMENT1")
                        workBaseRow("REMARK") = INPtbl(i)("REMARK")
                        If Convert.ToString(INPtbl(i)("DELFLG")) = "" Then
                            workBaseRow("DELFLG") = CONST_FLAG_NO
                        Else
                            workBaseRow("DELFLG") = INPtbl(i)("DELFLG")
                        End If

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
        txtCostCode.Text = Convert.ToString(dataTable(0)("COSTCODE"))
        txtLdKbn.Text = Convert.ToString(dataTable(0)("LDKBN"))
        txtLdKbn_Change()

        txtStYMD.Text = FormatDateContrySettings(Convert.ToString(dataTable(0)("STYMD")), GBA00003UserSetting.DATEFORMAT)
        txtEndYMD.Text = FormatDateContrySettings(Convert.ToString(dataTable(0)("ENDYMD")), GBA00003UserSetting.DATEFORMAT)
        txtDelFlg.Text = Convert.ToString(dataTable(0)("DELFLG"))
        txtDelFlg_Change()

        'ボタン制御
        SetButtonControl()

        'ダブルクリック明細情報取得設定（Detailbox情報)
        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = Me.hdnViewId.Value
        COA0014DetailView.TABID = "CHARGECODE"
        COA0014DetailView.SRCDATA = dataTable
        COA0014DetailView.REPEATER = WF_DViewRep1
        COA0014DetailView.COLPREFIX = "WF_Rep1_"
        COA0014DetailView.COA0014SetDetailView()

        COA0014DetailView.MAPID = CONST_MAPID
        COA0014DetailView.VARI = Me.hdnViewId.Value
        COA0014DetailView.TABID = "ACCOUNT"
        COA0014DetailView.SRCDATA = dataTable
        COA0014DetailView.REPEATER = WF_DViewRep2
        COA0014DetailView.COLPREFIX = "WF_Rep2_"
        COA0014DetailView.COA0014SetDetailView()

        'Detail初期設定
        SetDetailDbClick()

        '名称設定
        CLASS1_Change()
        CLASS2_Change()
        CLASS3_Change()
        CLASS4_Change()
        CLASS5_Change()
        CLASS6_Change()
        'CLASS7_Change()
        CLASS8_Change()
        CLASS9_Change()
        CLASS10_Change()
        SALESBR_Change()
        OPERATIONBR_Change()
        REPAIRBR_Change()
        SALES_Change()
        BL_Change()
        TANKOPE_Change()
        NONBR_Change()
        SOA_Change()

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

        '費用項目情報
        lblDtabCharge.Style.Remove("color")
        lblDtabCharge.Style.Add("color", "black")
        lblDtabCharge.Style.Remove("background-color")
        lblDtabCharge.Style.Add("background-color", "rgb(255,255,253)")
        lblDtabCharge.Style.Remove("border")
        lblDtabCharge.Style.Add("border", "1px solid black")
        lblDtabCharge.Style.Remove("font-weight")
        lblDtabCharge.Style.Add("font-weight", "normal")

        '経理情報 
        lblDtabAccount.Style.Remove("color")
        lblDtabAccount.Style.Add("color", "black")
        lblDtabAccount.Style.Remove("background-color")
        lblDtabAccount.Style.Add("background-color", "rgb(255,255,253)")
        lblDtabAccount.Style.Remove("border")
        lblDtabAccount.Style.Add("border", "1px solid black")
        lblDtabAccount.Style.Remove("font-weight")
        lblDtabAccount.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '費用項目情報
                lblDtabCharge.Style.Remove("color")
                lblDtabCharge.Style.Add("color", "blue")
                lblDtabCharge.Style.Remove("background-color")
                lblDtabCharge.Style.Add("background-color", "rgb(220,230,240)")
                lblDtabCharge.Style.Remove("border")
                lblDtabCharge.Style.Add("border", "1px solid blue")
                lblDtabCharge.Style.Remove("font-weight")
                lblDtabCharge.Style.Add("font-weight", "bold")
            Case 1
                '経理情報 
                lblDtabAccount.Style.Remove("color")
                lblDtabAccount.Style.Add("color", "blue")
                lblDtabAccount.Style.Remove("background-color")
                lblDtabAccount.Style.Add("background-color", "rgb(220,230,240)")
                lblDtabAccount.Style.Remove("border")
                lblDtabAccount.Style.Add("border", "1px solid blue")
                lblDtabAccount.Style.Remove("font-weight")
                lblDtabAccount.Style.Add("font-weight", "bold")

        End Select

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
        If TypeOf Page.PreviousPage Is GBM00010SELECT Then
            '検索画面の場合
            Dim prevObj As GBM00010SELECT = DirectCast(Page.PreviousPage, GBM00010SELECT)

            Me.hdnSelectedStYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtStYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            Me.hdnSelectedEndYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtEndYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            If hdnSelectedEndYMD.Value = "" Then
                hdnSelectedEndYMD.Value = hdnSelectedStYMD.Value
            End If

            Me.hdnSelectedCostCode.Value = DirectCast(prevObj.FindControl("txtCostCode"), TextBox).Text

            Me.hdnViewId.Value = DirectCast(prevObj.FindControl("lbRightList"), ListBox).SelectedValue

        ElseIf Page.PreviousPage Is Nothing Then

            Dim prevObj As GBM00000APPROVAL = DirectCast(Page.PreviousPage, GBM00000APPROVAL)

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
End Class
