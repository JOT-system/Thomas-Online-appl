Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net
Imports BASEDLL

''' <summary>
''' 取引先マスタ画面クラス
''' </summary>
Public Class GBM00021CLIENT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00021"    '自身のMAPID
    Private Const CONST_BASEDATATABLE = "GBM00021TBL"
    Private Const CONST_INPDATATABLE = "GBM00021INPTBL"
    Private Const CONST_UPDDATATABLE = "GBM00021UPDTBL"
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_TBLMASTER = "GBM0025_TORI"
    Private Const CONST_TBLAPPLY = "GBM0028_TORIAPPLY"
    Private Const CONST_EVENTCODE = "MasterApplyClient"

    Dim errListAll As List(Of String)                   'インポート全体のエラー
    Dim errList As List(Of String)                      'インポート中の１セット分のエラー
    Private returnCode As String = String.Empty         'サブ用リターンコード
    Dim errDisp As String = Nothing                     'エラー用表示文言
    Dim updateDisp As String = Nothing                  '更新用表示文言
    Dim newDisp As String = Nothing                     '新規用表示文言
    Dim deleteDisp As String = Nothing                  '削除用表示文言
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
    ''' カラム情報用テーブル
    ''' </summary>
    Private COLtbl As DataTable
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
            COLtbl = New DataTable

            '表示用文言判定
            If (COA0019Session.LANGDISP <> C_LANG.JA) Then
                errDisp = "ERROR"
                updateDisp = "UPDATE"
                newDisp = "NEW"
                deleteDisp = "DELETE"
            Else
                errDisp = "エラー"
                updateDisp = "更新"
                newDisp = "新規"
                deleteDisp = "削除"
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
                & "       TORIKBN                            , " _
                & "       TORICODE                           , " _
                & "       STYMD                              , " _
                & "       ENDYMD                             , " _
                & "       BANKCODE                           , " _
                & "       NAMES1                             , " _
                & "       NAMES2                             , " _
                & "       NAMES3                             , " _
                & "       NAMEL1                             , " _
                & "       NAMEL2                             , " _
                & "       NAMEL3                             , " _
                & "       POSTNUM                            , " _
                & "       ADDR1                              , " _
                & "       ADDR2                              , " _
                & "       ADDR3                              , " _
                & "       NAMESJP1                           , " _
                & "       NAMESJP2                           , " _
                & "       NAMESJP3                           , " _
                & "       NAMELJP1                           , " _
                & "       NAMELJP2                           , " _
                & "       NAMELJP3                           , " _
                & "       POSTNUMJP                          , " _
                & "       ADDRJP1                            , " _
                & "       ADDRJP2                            , " _
                & "       ADDRJP3                            , " _
                & "       REMARK                             , " _
                & "       DELFLG                             , " _
                & "       UPDYMD                             , " _
                & "       UPDUSER                            , " _
                & "       UPDTERMID                            " _
                & "  FROM (" _
                & "SELECT " _
                & "       '' as APPLYID , " _
                & "       isnull(rtrim(COMPCODE),'')                 as COMPCODE , " _
                & "       isnull(rtrim(TORIKBN),'')                  as TORIKBN , " _
                & "       isnull(rtrim(TORICODE),'')                 as TORICODE , " _
                & "       isnull(convert(nvarchar, STYMD , 111),'')  as STYMD , " _
                & "       isnull(convert(nvarchar, ENDYMD , 111),'') as ENDYMD , " _
                & "       isnull(rtrim(BANKCODE),'')                 as BANKCODE , " _
                & "       isnull(rtrim(NAMES1),'')                   as NAMES1 , " _
                & "       isnull(rtrim(NAMES2),'')                   as NAMES2 , " _
                & "       isnull(rtrim(NAMES3),'')                   as NAMES3 , " _
                & "       isnull(rtrim(NAMEL1),'')                   as NAMEL1 , " _
                & "       isnull(rtrim(NAMEL2),'')                   as NAMEL2 , " _
                & "       isnull(rtrim(NAMEL3),'')                   as NAMEL3 , " _
                & "       isnull(rtrim(POSTNUM),'')                  as POSTNUM , " _
                & "       isnull(rtrim(ADDR1),'')                    as ADDR1 , " _
                & "       isnull(rtrim(ADDR2),'')                    as ADDR2 , " _
                & "       isnull(rtrim(ADDR3),'')                    as ADDR3 , " _
                & "       isnull(rtrim(NAMESJP1),'')                 as NAMESJP1 , " _
                & "       isnull(rtrim(NAMESJP2),'')                 as NAMESJP2 , " _
                & "       isnull(rtrim(NAMESJP3),'')                 as NAMESJP3 , " _
                & "       isnull(rtrim(NAMELJP1),'')                 as NAMELJP1 , " _
                & "       isnull(rtrim(NAMELJP2),'')                 as NAMELJP2 , " _
                & "       isnull(rtrim(NAMELJP3),'')                 as NAMELJP3 , " _
                & "       isnull(rtrim(POSTNUMJP),'')                as POSTNUMJP , " _
                & "       isnull(rtrim(ADDRJP1),'')                  as ADDRJP1 , " _
                & "       isnull(rtrim(ADDRJP2),'')                  as ADDRJP2 , " _
                & "       isnull(rtrim(ADDRJP3),'')                  as ADDRJP3 , " _
                & "       isnull(rtrim(REMARK),'')                   as REMARK , " _
                & "       isnull(rtrim(DELFLG),'')                   as DELFLG , " _
                & "       isnull(convert(nvarchar, UPDYMD , 120),'') as UPDYMD , " _
                & "       isnull(rtrim(UPDUSER),'')                  as UPDUSER , " _
                & "       isnull(rtrim(UPDTERMID),'')                as UPDTERMID , " _
                & "       TIMSTP = cast(UPDTIMSTP                    as bigint) " _
                & " FROM " & CONST_TBLMASTER & " as tbl1 " _
                & " WHERE DELFLG    <> @P4 " _
                & " AND   STYMD     <= @P1 " _
                & " AND   ENDYMD    >= @P2 " _
                & " AND   NOT EXISTS( "
            '承認画面から遷移の場合
            If Page.PreviousPage Is Nothing Then
                SQLStr &= " SELECT * FROM " & CONST_TBLAPPLY & " as tbl2 " _
                    & " WHERE tbl2.APPLYID = @P3 "
            Else
                SQLStr &= " SELECT * FROM " & CONST_TBLAPPLY & " as tbl2 " _
                    & " WHERE tbl1.COMPCODE = tbl2.COMPCODE " _
                    & " AND   tbl1.TORIKBN = tbl2.TORIKBN " _
                    & " AND   tbl1.TORICODE = tbl2.TORICODE " _
                    & " AND   tbl1.STYMD = tbl2.STYMD " _
                    & " AND   tbl1.DELFLG <> @P4 " _
                    & " AND   tbl2.DELFLG <> @P4 "
            End If
            SQLStr &= " )" _
                & " UNION ALL " _
                & "SELECT " _
                & "       isnull(rtrim(APPLYID),'')                  as APPLYID , " _
                & "       isnull(rtrim(COMPCODE),'')                 as COMPCODE , " _
                & "       isnull(rtrim(TORIKBN),'')                  as TORIKBN , " _
                & "       isnull(rtrim(TORICODE),'')                 as TORICODE , " _
                & "       isnull(convert(nvarchar, STYMD , 111),'')  as STYMD , " _
                & "       isnull(convert(nvarchar, ENDYMD , 111),'') as ENDYMD , " _
                & "       isnull(rtrim(BANKCODE),'')                 as BANKCODE , " _
                & "       isnull(rtrim(NAMES1),'')                   as NAMES1 , " _
                & "       isnull(rtrim(NAMES2),'')                   as NAMES2 , " _
                & "       isnull(rtrim(NAMES3),'')                   as NAMES3 , " _
                & "       isnull(rtrim(NAMEL1),'')                   as NAMEL1 , " _
                & "       isnull(rtrim(NAMEL2),'')                   as NAMEL2 , " _
                & "       isnull(rtrim(NAMEL3),'')                   as NAMEL3 , " _
                & "       isnull(rtrim(POSTNUM),'')                  as POSTNUM , " _
                & "       isnull(rtrim(ADDR1),'')                    as ADDR1 , " _
                & "       isnull(rtrim(ADDR2),'')                    as ADDR2 , " _
                & "       isnull(rtrim(ADDR3),'')                    as ADDR3 , " _
                & "       isnull(rtrim(NAMESJP1),'')                 as NAMESJP1 , " _
                & "       isnull(rtrim(NAMESJP2),'')                 as NAMESJP2 , " _
                & "       isnull(rtrim(NAMESJP3),'')                 as NAMESJP3 , " _
                & "       isnull(rtrim(NAMELJP1),'')                 as NAMELJP1 , " _
                & "       isnull(rtrim(NAMELJP2),'')                 as NAMELJP2 , " _
                & "       isnull(rtrim(NAMELJP3),'')                 as NAMELJP3 , " _
                & "       isnull(rtrim(POSTNUMJP),'')                as POSTNUMJP , " _
                & "       isnull(rtrim(ADDRJP1),'')                  as ADDRJP1 , " _
                & "       isnull(rtrim(ADDRJP2),'')                  as ADDRJP2 , " _
                & "       isnull(rtrim(ADDRJP3),'')                  as ADDRJP3 , " _
                & "       isnull(rtrim(REMARK),'')                   as REMARK , " _
                & "       isnull(rtrim(DELFLG),'')                   as DELFLG , " _
                & "       isnull(convert(nvarchar, UPDYMD , 120),'') as UPDYMD , " _
                & "       isnull(rtrim(UPDUSER),'')                  as UPDUSER , " _
                & "       isnull(rtrim(UPDTERMID),'')                as UPDTERMID , " _
                & "       TIMSTP = cast(UPDTIMSTP                    as bigint) " _
                & " FROM " & CONST_TBLAPPLY & " "
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

                '取引先区分
                If (String.IsNullOrEmpty(Me.hdnSelectedToriKbn.Value) = False) Then
                    SQLStr &= String.Format(" AND TORIKBN = '{0}' ", Me.hdnSelectedToriKbn.Value)
                End If

                '取引先コード
                If (String.IsNullOrEmpty(Me.hdnSelectedToriCode.Value) = False) Then
                    SQLStr &= String.Format(" AND TORICODE like '{0}%' ", Me.hdnSelectedToriCode.Value)
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
                PARA3.Value = Me.hdnSelectedApplyID.Value
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
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage)
            Return
        End If

        'UPLOAD_XLSデータ取得
        COA0029XlsTable.MAPID = CONST_MAPID
        COA0029XlsTable.COA0029XlsToTable()
        If COA0029XlsTable.ERR = C_MESSAGENO.NORMAL Then
            If COA0029XlsTable.TBLDATA.Rows.Count = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage)
                Return
            End If
        Else
            returnCode = COA0029XlsTable.ERR
            CommonFunctions.ShowMessage(COA0029XlsTable.ERR, Me.lblFooterMessage)
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
        For i As Integer = 0 To COA0029XlsTable.TBLDATA.Rows.Count - 1

            'XLSTBL明細⇒INProw
            INProwWork = INPtbl.NewRow

            Dim ToriKbn As String = COA0029XlsTable.TBLDATA.Rows(i)("TORIKBN").ToString
            Dim ToriCode As String = COA0029XlsTable.TBLDATA.Rows(i)("TORICODE").ToString
            If COA0029XlsTable.TBLDATA.Columns.Contains("STYMD") Then
                Dim stYmd As String = COA0029XlsTable.TBLDATA.Rows(i)("STYMD").ToString
                sameDr = (From item In BASEtbl Where item("TORIKBN").Equals(ToriKbn) AndAlso item("TORICODE").Equals(ToriCode) AndAlso item("STYMD").Equals(stYmd))
            Else
                sameDr = (From item In BASEtbl Where item("TORIKBN").Equals(ToriKbn) AndAlso item("TORICODE").Equals(ToriCode))
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
                    'INProwWork(workColumn) = ""
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
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        'メッセージ表示
        If errListAll.Count > 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.RIGHTBIXOUT, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
        ElseIf returnCode = C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
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
                '取引先区分ビュー表示切替
                Case Me.vLeftToriKbn.ID
                    SetToriKbnListItem(Me.txtToriKbn.Text)
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

            '取引先名称 絞込判定
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtNamesEx.Text <> "") Then
                Dim searchStr As String = ""
                '検索用文字列（部分一致）
                If (COA0019Session.LANGDISP = C_LANG.JA) Then
                    searchStr = Convert.ToString(BASEtbl.Rows(i)("NAMESJP1")).ToUpper
                Else
                    searchStr = Convert.ToString(BASEtbl.Rows(i)("NAMES1")).ToUpper
                End If

                If Not searchStr.Contains(txtNamesEx.Text.ToUpper) Then
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
                If Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = updateDisp OrElse Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = deleteDisp Then
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
                             & "   and TORIKBN = @P02 " _
                             & "   and TORICODE = @P03 " _
                             & "   and STYMD = @P04 " _
                             & "   and DELFLG <> @P05 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        With SQLcmd.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TORIKBN")
                            .Add("@P03", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TORICODE")
                            .Add("@P04", System.Data.SqlDbType.Date).Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                            .Add("@P05", System.Data.SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
                        End With

                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("TORIKBN")) = Convert.ToString(BASEtbl.Rows(i)("TORIKBN")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("TORICODE")) = Convert.ToString(BASEtbl.Rows(i)("TORICODE")) AndAlso
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
                             & "   and TORIKBN = @P02 " _
                             & "   and TORICODE = @P03 " _
                             & "   and STYMD = @P04 " _
                             & "   and DELFLG <> @P05 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        With SQLcmd.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TORIKBN")
                            .Add("@P03", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TORICODE")
                            .Add("@P04", System.Data.SqlDbType.Date).Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                            .Add("@P05", System.Data.SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
                        End With

                        SQLdr = SQLcmd.ExecuteReader()

                        While SQLdr.Read
                            If RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDYMD"))) = Convert.ToString(SQLdr("UPDYMD")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDUSER"))) = Convert.ToString(SQLdr("UPDUSER")) AndAlso
                               RTrim(Convert.ToString(BASEtbl.Rows(i)("UPDTERMID"))) = Convert.ToString(SQLdr("UPDTERMID")) Then
                            Else
                                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                                    If Convert.ToString(BASEtbl.Rows(j)("COMPCODE")) = Convert.ToString(BASEtbl.Rows(i)("COMPCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("TORIKBN")) = Convert.ToString(BASEtbl.Rows(i)("TORIKBN")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("TORICODE")) = Convert.ToString(BASEtbl.Rows(i)("TORICODE")) AndAlso
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

                    If Not (Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = errDisp OrElse Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = "★" & errDisp OrElse Convert.ToString(BASEtbl.Rows(i)("OPERATION")) = "") Then

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

                            ''メール
                            'Dim GBA00009MailSendSet As New GBA00009MailSendSet
                            'GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                            'GBA00009MailSendSet.EVENTCODE = C_TRAMSTEVENT.APPLY
                            'GBA00009MailSendSet.STATUS = Convert.ToString(BASEtbl.Rows(i)("SAVESTATUS"))
                            'GBA00009MailSendSet.MAILSUBCODE = ""
                            'GBA00009MailSendSet.APPLYID = Convert.ToString(BASEtbl.Rows(i)("APPLYID"))
                            'GBA00009MailSendSet.GBA00009setMailToUserM()
                            'If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                            '    CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                            '    Return
                            'End If

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
                                 & "    AND TORIKBN = @P03  " _
                                 & "    AND TORICODE = @P04  " _
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
                                 & "        BANKCODE = @P07 , " _
                                 & "        NAMES1 = @P08 , " _
                                 & "        NAMES2 = @P09 , " _
                                 & "        NAMES3 = @P10 , " _
                                 & "        NAMEL1 = @P11 , " _
                                 & "        NAMEL2 = @P12 , " _
                                 & "        NAMEL3 = @P13 , " _
                                 & "        POSTNUM = @P14 , " _
                                 & "        ADDR1 = @P15 , " _
                                 & "        ADDR2 = @P16 , " _
                                 & "        ADDR3 = @P17 , " _
                                 & "        NAMESJP1 = @P18 , " _
                                 & "        NAMESJP2 = @P19 , " _
                                 & "        NAMESJP3 = @P20 , " _
                                 & "        NAMELJP1 = @P21 , " _
                                 & "        NAMELJP2 = @P22 , " _
                                 & "        NAMELJP3 = @P23 , " _
                                 & "        POSTNUMJP = @P24 , " _
                                 & "        ADDRJP1 = @P25 , " _
                                 & "        ADDRJP2 = @P26 , " _
                                 & "        ADDRJP3 = @P27 , " _
                                 & "        REMARK = @P28 , " _
                                 & "        DELFLG = @P29 , " _
                                 & "        INITYMD = @P30 , " _
                                 & "        UPDYMD = @P31 , " _
                                 & "        UPDUSER = @P32 , " _
                                 & "        UPDTERMID = @P33 , " _
                                 & "        RECEIVEYMD = @P34  " _
                                 & "  WHERE COMPCODE = @P02 " _
                                 & "    AND TORIKBN = @P03 " _
                                 & "    AND TORICODE = @P04 " _
                                 & "    AND STYMD = @P05 ; " _
                                 & " IF ( @@FETCH_STATUS <> 0 ) " _
                                 & "  INSERT INTO " & updTable _
                                 & "       ("
                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID , "
                        End If
                        SQLStr = SQLStr & " COMPCODE , " _
                                 & "        TORIKBN , " _
                                 & "        TORICODE , " _
                                 & "        STYMD , " _
                                 & "        ENDYMD , " _
                                 & "        BANKCODE , " _
                                 & "        NAMES1 , " _
                                 & "        NAMES2 , " _
                                 & "        NAMES3 , " _
                                 & "        NAMEL1 , " _
                                 & "        NAMEL2 , " _
                                 & "        NAMEL3 , " _
                                 & "        POSTNUM , " _
                                 & "        ADDR1 , " _
                                 & "        ADDR2 , " _
                                 & "        ADDR3 , " _
                                 & "        NAMESJP1 , " _
                                 & "        NAMESJP2 , " _
                                 & "        NAMESJP3 , " _
                                 & "        NAMELJP1 , " _
                                 & "        NAMELJP2 , " _
                                 & "        NAMELJP3 , " _
                                 & "        POSTNUMJP , " _
                                 & "        ADDRJP1 , " _
                                 & "        ADDRJP2 , " _
                                 & "        ADDRJP3 , " _
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
                        SQLStr = SQLStr & "         @P02,@P03,@P04,@P05,@P06,@P07,@P08,@P09,@P10, " _
                                 & "           @P11,@P12,@P13,@P14,@P15,@P16,@P17,@P18,@P19,@P20, " _
                                 & "           @P21,@P22,@P23,@P24,@P25,@P26,@P27,@P28,@P29,@P30, " _
                                 & "           @P31,@P32,@P33,@P34); " _
                                 & " CLOSE timestamp ; " _
                                 & " DEALLOCATE timestamp ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        With SQLcmd.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("APPLYID")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P03", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TORIKBN")
                            .Add("@P04", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TORICODE")
                            .Add("@P05", System.Data.SqlDbType.Date).Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                            .Add("@P06", System.Data.SqlDbType.Date).Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("ENDYMD")))
                            .Add("@P07", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("BANKCODE")
                            .Add("@P08", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMES1")
                            .Add("@P09", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMES2")
                            .Add("@P10", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMES3")
                            .Add("@P11", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMEL1")
                            .Add("@P12", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMEL2")
                            .Add("@P13", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMEL3")
                            .Add("@P14", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("POSTNUM")
                            .Add("@P15", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("ADDR1")
                            .Add("@P16", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("ADDR2")
                            .Add("@P17", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("ADDR3")
                            .Add("@P18", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMESJP1")
                            .Add("@P19", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMESJP2")
                            .Add("@P20", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMESJP3")
                            .Add("@P21", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMELJP1")
                            .Add("@P22", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMELJP2")
                            .Add("@P23", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("NAMELJP3")
                            .Add("@P24", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("POSTNUMJP")
                            .Add("@P25", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("ADDRJP1")
                            .Add("@P26", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("ADDRJP2")
                            .Add("@P27", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("ADDRJP3")
                            .Add("@P28", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("REMARK")
                            .Add("@P29", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("DELFLG")
                            .Add("@P30", System.Data.SqlDbType.DateTime).Value = nowDate
                            .Add("@P31", System.Data.SqlDbType.DateTime).Value = nowDate
                            .Add("@P32", System.Data.SqlDbType.NVarChar).Value = COA0019Session.USERID
                            .Add("@P33", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                            .Add("@P34", System.Data.SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                        End With
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
                                & "   And TORIKBN = @P02 " _
                                & "   And TORICODE = @P03 " _
                                & "   And STYMD = @P04 " _
                                & " ;"

                        SQLcmd2 = New SqlCommand(SQLStr2, SQLcon)
                        With SQLcmd2.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TORIKBN")
                            .Add("@P03", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("TORICODE")
                            .Add("@P04", System.Data.SqlDbType.Date).Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        End With

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
        Dim dupCheckFields = CommonFunctions.CreateCompareFieldList({"COMPCODE", "TORIKBN", "TORICODE", "STYMD"})
        Dim drBefor As DataRow = INPtbl.NewRow
        Dim drCurrent As DataRow = INPtbl.NewRow
        For i As Integer = INPtbl.Rows.Count - 1 To 1 Step -1
            'KEY重複
            drBefor = INPtbl.Rows(i - 1)
            drCurrent = INPtbl.Rows(i)
            If CommonFunctions.CompareDataFields(drCurrent, drBefor, dupCheckFields) Then
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
                Dim compareFieldList = CommonFunctions.CreateCompareFieldList({"COMPCODE", "TORIKBN", "TORICODE"})
                Dim dr As DataRow = BASEtbl.NewRow
                For j As Integer = 0 To BASEtbl.Rows.Count - 1
                    dr.ItemArray = BASEtbl.Rows(j).ItemArray
                    If Convert.ToString(dr("DELFLG")) <> BaseDllCommon.CONST_FLAG_YES Then
                        '日付以外の項目が等しい
                        'If BASEtbl.Rows(j)("APPLYID") = workInpRow("APPLYID") AndAlso
                        If CommonFunctions.CompareDataFields(dr, workInpRow, compareFieldList) Then

                            'ENDYMDは変更扱い
                            If Convert.ToString(dr("STYMD")) = Convert.ToString(workInpRow("STYMD")) Then

                                '同一レコード
                                Exit For
                            Else

                                Dim baseDateStart As Date
                                Dim baseDateEnd As Date
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
            rtc &= ControlChars.NewLine & "  --> CLIENT CLASS    =" & Convert.ToString(argRow("TORIKBN")) & " , "
            rtc &= ControlChars.NewLine & "  --> CLIENT CODE   =" & Convert.ToString(argRow("TORICODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> EFFECTIVE(FROM) =" & Convert.ToString(argRow("STYMD")) & " , "
            rtc &= ControlChars.NewLine & "  --> DELETE FLG      =" & Convert.ToString(argRow("DELFLG")) & " "
        Else
            rtc &= ControlChars.NewLine & "  --> 会社コード      =" & Convert.ToString(argRow("COMPCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> 取引先区分        =" & Convert.ToString(argRow("TORIKBN")) & " , "
            rtc &= ControlChars.NewLine & "  --> 取引先コード      =" & Convert.ToString(argRow("TORICODE")) & " , "
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
                Case Me.vLeftToriKbn.ID 'アクティブなビューが国コード
                    '取引先区分選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbToriKbn.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbToriKbn.SelectedItem.Value
                            Me.lblToriKbnText.Text = Me.lbToriKbn.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblToriKbnText.Text = ""
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
        AddLangSetting(dicDisplayText, Me.lblNamesEx, "名称(短)", "Name (short)")

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
        AddLangSetting(dicDisplayText, Me.lblToriKbn, "取引先区分", "Client Class")
        AddLangSetting(dicDisplayText, Me.lblToriCode, "取引先コード", "Client Code")
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
            Me.lblDtabTrader.Text = "Client Info"
        Else
            Me.lblDtabTrader.Text = "取引先情報"
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
        table.Columns("APPLYID").DefaultValue = ""
        table.Columns.Add("COMPCODE", GetType(String))
        table.Columns("COMPCODE").DefaultValue = ""
        table.Columns.Add("TORIKBN", GetType(String))
        table.Columns("TORIKBN").DefaultValue = ""
        table.Columns.Add("TORICODE", GetType(String))
        table.Columns("TORICODE").DefaultValue = ""
        table.Columns.Add("STYMD", GetType(String))
        table.Columns("STYMD").DefaultValue = ""
        table.Columns.Add("ENDYMD", GetType(String))
        table.Columns("ENDYMD").DefaultValue = ""
        table.Columns.Add("BANKCODE", GetType(String))
        table.Columns("BANKCODE").DefaultValue = ""
        table.Columns.Add("NAMES1", GetType(String))
        table.Columns("NAMES1").DefaultValue = ""
        table.Columns.Add("NAMES2", GetType(String))
        table.Columns("NAMES2").DefaultValue = ""
        table.Columns.Add("NAMES3", GetType(String))
        table.Columns("NAMES3").DefaultValue = ""
        table.Columns.Add("NAMEL1", GetType(String))
        table.Columns("NAMEL1").DefaultValue = ""
        table.Columns.Add("NAMEL2", GetType(String))
        table.Columns("NAMEL2").DefaultValue = ""
        table.Columns.Add("NAMEL3", GetType(String))
        table.Columns("NAMEL3").DefaultValue = ""
        table.Columns.Add("POSTNUM", GetType(String))
        table.Columns("POSTNUM").DefaultValue = ""
        table.Columns.Add("ADDR1", GetType(String))
        table.Columns("ADDR1").DefaultValue = ""
        table.Columns.Add("ADDR2", GetType(String))
        table.Columns("ADDR2").DefaultValue = ""
        table.Columns.Add("ADDR3", GetType(String))
        table.Columns("ADDR3").DefaultValue = ""
        table.Columns.Add("NAMESJP1", GetType(String))
        table.Columns("NAMESJP1").DefaultValue = ""
        table.Columns.Add("NAMESJP2", GetType(String))
        table.Columns("NAMESJP2").DefaultValue = ""
        table.Columns.Add("NAMESJP3", GetType(String))
        table.Columns("NAMESJP3").DefaultValue = ""
        table.Columns.Add("NAMELJP1", GetType(String))
        table.Columns("NAMELJP1").DefaultValue = ""
        table.Columns.Add("NAMELJP2", GetType(String))
        table.Columns("NAMELJP2").DefaultValue = ""
        table.Columns.Add("NAMELJP3", GetType(String))
        table.Columns("NAMELJP3").DefaultValue = ""
        table.Columns.Add("POSTNUMJP", GetType(String))
        table.Columns("POSTNUMJP").DefaultValue = ""
        table.Columns.Add("ADDRJP1", GetType(String))
        table.Columns("ADDRJP1").DefaultValue = ""
        table.Columns.Add("ADDRJP2", GetType(String))
        table.Columns("ADDRJP2").DefaultValue = ""
        table.Columns.Add("ADDRJP3", GetType(String))
        table.Columns("ADDRJP3").DefaultValue = ""

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

        table.Columns.Add("SAVESTATUS", GetType(String))

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
        workRow("TORIKBN") = ""
        workRow("TORICODE") = ""
        workRow("STYMD") = ""
        workRow("ENDYMD") = ""
        workRow("BANKCODE") = ""
        workRow("NAMES1") = ""
        workRow("NAMES2") = ""
        workRow("NAMES3") = ""
        workRow("NAMEL1") = ""
        workRow("NAMEL2") = ""
        workRow("NAMEL3") = ""
        workRow("POSTNUM") = ""
        workRow("ADDR1") = ""
        workRow("ADDR2") = ""
        workRow("ADDR3") = ""
        workRow("NAMESJP1") = ""
        workRow("NAMESJP2") = ""
        workRow("NAMESJP3") = ""
        workRow("NAMELJP1") = ""
        workRow("NAMELJP2") = ""
        workRow("NAMELJP3") = ""
        workRow("POSTNUMJP") = ""
        workRow("ADDRJP1") = ""
        workRow("ADDRJP2") = ""
        workRow("ADDRJP3") = ""
        workRow("REMARK") = ""
        workRow("DELFLG") = ""
        workRow("UPDYMD") = ""
        workRow("UPDUSER") = ""
        workRow("UPDTERMID") = ""

        workRow("SAVESTATUS") = ""

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
            workRow("COMPCODE") = HttpContext.Current.Session("APSRVCamp")
            workRow("TORIKBN") = txtToriKbn.Text
            workRow("TORICODE") = txtToriCode.Text
            workRow("STYMD") = FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT)
            workRow("ENDYMD") = FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT)
            workRow("BANKCODE") = ""
            workRow("NAMES1") = ""
            workRow("NAMES2") = ""
            workRow("NAMES3") = ""
            workRow("NAMEL1") = ""
            workRow("NAMEL2") = ""
            workRow("NAMEL3") = ""
            workRow("POSTNUM") = ""
            workRow("ADDR1") = ""
            workRow("ADDR2") = ""
            workRow("ADDR3") = ""
            workRow("NAMESJP1") = ""
            workRow("NAMESJP2") = ""
            workRow("NAMESJP3") = ""
            workRow("NAMELJP1") = ""
            workRow("NAMELJP2") = ""
            workRow("NAMELJP3") = ""
            workRow("POSTNUMJP") = ""
            workRow("ADDRJP1") = ""
            workRow("ADDRJP2") = ""
            workRow("ADDRJP3") = ""
            workRow("REMARK") = ""
            workRow("DELFLG") = txtDelFlg.Text
            workRow("UPDYMD") = ""
            workRow("UPDUSER") = ""
            workRow("UPDTERMID") = ""
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

        lblDtabTrader.Style.Remove("color")
        lblDtabTrader.Style.Add("color", "blue")
        lblDtabTrader.Style.Remove("background-color")
        lblDtabTrader.Style.Add("background-color", "rgb(220,230,240)")
        lblDtabTrader.Style.Remove("border")
        lblDtabTrader.Style.Add("border", "1px solid blue")
        lblDtabTrader.Style.Remove("font-weight")
        lblDtabTrader.Style.Add("font-weight", "bold")

        'Detail設定処理
        SetDetailDbClick()

        '初期値設定
        SetInitValue()

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
                GetAttributes(repField.Text, repAttr, i)
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
                GetAttributes(repField.Text, repAttr, i)
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
                GetAttributes(repField.Text, repAttr, i)
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

            'MORG設定
            If DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label).Text = "MORG" Then
                'repField = DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_FIELD_3"), System.Web.UI.WebControls.Label)
                DirectCast(WF_DViewRep1.Items(i).FindControl("WF_Rep1_VALUE_3"), System.Web.UI.WebControls.TextBox).Text = GBA00003UserSetting.OFFICECODE
            End If
        Next

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
    Protected Sub GetAttributes(ByVal repField As String, ByRef repAttr As String, Optional rowIdx As Integer = 0)

        Select Case repField
            'Case "ACCCURRENCYSEGMENT"
            '    '経理円貨外貨区分
            '    repAttr = String.Format("Field_DBclick('vLeftAccCurrencySegment', '{0}');", rowIdx)
            'Case "BOTHCLASS"
            '    '両建区分
            '    repAttr = String.Format("Field_DBclick('vLeftBothClass', '{0}');", rowIdx)
            ''Case "TORICODE"
            ''    '取引先コード
            ''    repAttr = String.Format("Field_DBclick('vLeftToriCode', '{0}');", rowIdx)
            ''Case "PAYDAY"
            ''    '期日
            ''    repAttr = String.Format("Field_DBclick('vLeftPayDay', '{0}');", rowIdx)
            'Case "HOLIDAYFLG"
            '    '休日フラグ
            '    repAttr = String.Format("Field_DBclick('vLeftHolidayFlg', '{0}');", rowIdx)
            'Case Else
            '    repAttr = ""

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
        'Dim dicField As Dictionary(Of String, String) = Nothing
        returnCode = C_MESSAGENO.NORMAL

        '入力項目チェック
        '①単項目チェック

        'カラム情報取得
        'dicField = New Dictionary(Of String, String)
        'CheckSingle(InpRow, dicField, escapeFlg)
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

        '取引先区分
        If Me.lbToriKbn.Items.Count <= 0 Then
            SetToriKbnListItem(Convert.ToString(InpRow("TORIKBN")))
        End If
        ChedckList(Convert.ToString(InpRow("TORIKBN")), lbToriKbn, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("ToriKbn") & ":" & Convert.ToString(InpRow("ToriKbn")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '取引先コード
        'If Me.lbToriCode.Items.Count <= 0 Then
        '    SetToriCodeListItem(Convert.ToString(InpRow("TORICODE")))
        'End If
        'ChedckList(Convert.ToString(InpRow("TORICODE")), lbToriCode, refErrMessage)
        'If returnCode <> C_MESSAGENO.NORMAL Then
        '    errMessageStr = Me.ErrItemSet(InpRow)
        '    If txtRightErrorMessage.Text <> "" Then
        '        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
        '    End If
        '    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
        '                                   & "・" & refErrMessage & "(" & dicField("TORICODE") & ":" & Convert.ToString(InpRow("TORICODE")) & ")" & errMessageStr
        '    errFlg = True
        '    returnCode = C_MESSAGENO.NORMAL
        'End If

        '削除フラグ
        If Me.lbDelFlg.Items.Count <= 0 Then
            SetDelFlgListItem(Convert.ToString(InpRow("DELFLG")))
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
    Protected Sub CheckSingle(ByVal argRow As DataRow, ByRef argEscFlg As Boolean)
        'Protected Sub CheckSingle(ByVal argRow As DataRow, ByRef argDic As Dictionary(Of String, String), ByRef argEscFlg As Boolean)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar        '例外文字排除 String Get
        Dim skipInvCharFields As New List(Of String) From {""}
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck          '項目チェック
        Dim slExcludeList As New List(Of String)(New String() {"－", "&"})

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

        'For Each itm As KeyValuePair(Of String, String) In argDic
        For Each itm As KeyValuePair(Of String, String) In Me.dicField

            '入力文字置き換え
            '画面PassWord内の使用禁止文字排除
            If skipInvCharFields.Contains(itm.Key) = False Then
                COA0008InvalidChar.CHARin = Convert.ToString(argRow(itm.Key))
                COA0008InvalidChar.EXCLUDELIST = slExcludeList
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
            With SQLcmd.Parameters
                .Add("@P1", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@P2", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@P3", System.Data.SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
            End With
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
    ''' 取引先区分リストアイテムを設定
    ''' </summary>
    Private Sub SetToriKbnListItem(selectedValue As String)
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbToriKbn.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "TORIKBN"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbToriKbn
        Else
            COA0017FixValue.LISTBOX2 = Me.lbToriKbn
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbToriKbn = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbToriKbn = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbToriKbn.Items.Count > 0 Then
                Dim findListItem = Me.lbToriKbn.Items.FindByValue(selectedValue)
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
    ''' 取引先コードリストアイテムを設定
    ''' </summary>
    'Private Sub SetToriCodeListItem(Optional selectedValue As String = "")
    '    Dim COA0017FixValue As New COA0017FixValue

    '    'リストクリア
    '    Me.lbToriCode.Items.Clear()

    '    'リスト設定
    '    Dim sqlStat As New StringBuilder
    '    sqlStat.AppendLine("SELECT TORICODE AS [CODE]")
    '    sqlStat.AppendLine("      ,TORICODE AS [NAME]")
    '    sqlStat.AppendLine("  FROM GBM0025_TORI")
    '    sqlStat.AppendLine(" WHERE DELFLG <> @DELFLG ")

    '    Using SQLcon As New SqlConnection(COA0019Session.DBcon),
    '          SQLcmd As New SqlCommand(sqlStat.ToString, SQLcon)
    '        SQLcon.Open()

    '        With SQLcmd.Parameters
    '            .Add("@DELFLG", SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
    '        End With
    '        Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
    '            While SQLdr.Read
    '                'DBからアイテムを設定
    '                Me.lbToriCode.Items.Add(New ListItem(Convert.ToString(SQLdr("CODE"))))
    '            End While
    '        End Using

    '    End Using

    '    '一応現在入力しているテキストと一致するものを選択状態
    '    If Me.lbToriCode.Items.Count > 0 Then
    '        Dim findListItem = Me.lbToriCode.Items.FindByValue(selectedValue)
    '        If findListItem IsNot Nothing Then
    '            findListItem.Selected = True
    '        End If
    '    End If
    '    '正常
    '    returnCode = C_MESSAGENO.NORMAL


    'End Sub
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
    ''' 取引先区分名設定
    ''' </summary>
    Public Sub txtToriKbn_Change()

        Try
            Me.lblToriKbnText.Text = ""

            SetToriKbnListItem(Me.txtToriKbn.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbToriKbn.Items.Count > 0 Then
                Dim findListItem = Me.lbToriKbn.Items.FindByValue(Me.txtToriKbn.Text)
                If findListItem IsNot Nothing Then
                    Me.lblToriKbnText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbToriKbn.Items.FindByValue(Me.txtToriKbn.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblToriKbnText.Text = findListItemUpper.Text
                        Me.txtToriKbn.Text = findListItemUpper.Value
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
                Case "★" & newDisp
                    BASEtbl.Rows(i)(1) = newDisp
                Case "★" & deleteDisp
                    BASEtbl.Rows(i)(1) = deleteDisp
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
        txtToriKbn.Text = ""
        lblToriKbnText.Text = ""
        txtToriCode.Text = ""
        lblToriCodeText.Text = ""
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
                Case "★" & newDisp
                    BASEtbl.Rows(i)("OPERATION") = newDisp
                Case "★" & deleteDisp
                    BASEtbl.Rows(i)("OPERATION") = deleteDisp
            End Select
        Next
        Dim compareUpdTargetFieldList = CommonFunctions.CreateCompareFieldList({"COMPCODE", "TORIKBN", "TORICODE", "STYMD"})
        Dim compareModCheckFieldList = CommonFunctions.CreateCompareFieldList({"ENDYMD", "BANKCODE", "NAMES1", "NAMES2", "NAMES3", "NAMEL1", "NAMEL2", "NAMEL3",
                                                                               "POSTNUM", "ADDR1", "ADDR2", "ADDR2",
                                                                               "NAMESJP1", "NAMESJP2", "NAMESJP3", "NAMELJP1", "NAMELJP2", "NAMELJP3",
                                                                               "POSTNUMJP", "ADDRJP1", "ADDRJP2", "ADDRJP3",
                                                                               "REMARK", "DELFLG"})

        Dim drInput As DataRow = INPtbl.NewRow
        For i As Integer = 0 To INPtbl.Rows.Count - 1

            drInput.ItemArray = INPtbl(i).ItemArray
            If Convert.ToString(drInput("HIDDEN")) <> "1" Then ' "1" ・・・取り込み対象外エラー

                Dim workBasePos As Integer = -1
                newFlg = False

                '内部テーブル検索
                For j As Integer = 0 To BASEtbl.Rows.Count - 1

                    Dim workBaseRow As DataRow
                    workBaseRow = BASEtbl.NewRow
                    workBaseRow.ItemArray = BASEtbl.Rows(j).ItemArray

                    ' 更新対象検索
                    If CommonFunctions.CompareDataFields(workBaseRow, drInput, compareUpdTargetFieldList) Then

                        ' 変更なし  
                        If Convert.ToString(drInput("OPERATION")) <> errDisp AndAlso
                           CommonFunctions.CompareDataFields(workBaseRow, drInput, compareModCheckFieldList) Then
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

                    '固定項目
                    Dim workBaseRow As DataRow
                    workBaseRow = BASEtbl.NewRow

                    If workBasePos < BASEtbl.Rows.Count Then
                        '更新対象を設定
                        workBaseRow.ItemArray = BASEtbl.Rows(workBasePos).ItemArray
                    End If

                    '固定項目
                    workBaseRow("LINECNT") = workBasePos + 1
                    If workBasePos >= BASEtbl.Rows.Count Then
                        workBaseRow("TIMSTP") = "0"                                 ' 新規レコード
                    Else
                        workBaseRow("TIMSTP") = BASEtbl(workBasePos)("TIMSTP")      ' 更新レコード
                    End If

                    If Convert.ToString(drInput("OPERATION")) <> errDisp Then
                        If newFlg Then
                            '新規
                            workBaseRow("OPERATION") = newDisp
                            workBaseRow("SAVESTATUS") = "1"
                        Else
                            If Convert.ToString(drInput("DELFLG")) <> BaseDllCommon.CONST_FLAG_YES Then
                                '更新
                                workBaseRow("OPERATION") = updateDisp
                                workBaseRow("SAVESTATUS") = "2"
                            Else
                                '削除
                                workBaseRow("OPERATION") = deleteDisp
                                workBaseRow("SAVESTATUS") = "3"
                            End If
                        End If
                    Else
                        workBaseRow("OPERATION") = drInput("OPERATION")
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
                        workBaseRow("COMPCODE") = drInput("COMPCODE")
                        workBaseRow("TORIKBN") = drInput("TORIKBN")
                        workBaseRow("TORICODE") = drInput("TORICODE")
                        If Date.TryParse(Convert.ToString(drInput("STYMD")), stDate) Then
                            workBaseRow("STYMD") = stDate.ToString("yyyy/MM/dd")
                        Else
                            workBaseRow("STYMD") = drInput("STYMD")
                        End If
                        If Date.TryParse(Convert.ToString(drInput("ENDYMD")), endDate) Then
                            workBaseRow("ENDYMD") = endDate.ToString("yyyy/MM/dd")
                        Else
                            workBaseRow("ENDYMD") = drInput("ENDYMD")
                        End If
                        workBaseRow("BANKCODE") = drInput("BANKCODE")
                        workBaseRow("NAMES1") = drInput("NAMES1")
                        workBaseRow("NAMES2") = drInput("NAMES2")
                        workBaseRow("NAMES3") = drInput("NAMES3")
                        workBaseRow("NAMEL1") = drInput("NAMEL1")
                        workBaseRow("NAMEL2") = drInput("NAMEL2")
                        workBaseRow("NAMEL3") = drInput("NAMEL3")
                        workBaseRow("POSTNUM") = drInput("POSTNUM")
                        workBaseRow("ADDR1") = drInput("ADDR1")
                        workBaseRow("ADDR2") = drInput("ADDR2")
                        workBaseRow("ADDR3") = drInput("ADDR3")
                        workBaseRow("NAMESJP1") = drInput("NAMESJP1")
                        workBaseRow("NAMESJP2") = drInput("NAMESJP2")
                        workBaseRow("NAMESJP3") = drInput("NAMESJP3")
                        workBaseRow("NAMELJP1") = drInput("NAMELJP1")
                        workBaseRow("NAMELJP2") = drInput("NAMELJP2")
                        workBaseRow("NAMELJP3") = drInput("NAMELJP3")
                        workBaseRow("POSTNUMJP") = drInput("POSTNUMJP")
                        workBaseRow("ADDRJP1") = drInput("ADDRJP1")
                        workBaseRow("ADDRJP2") = drInput("ADDRJP2")
                        workBaseRow("ADDRJP3") = drInput("ADDRJP3")
                        workBaseRow("REMARK") = drInput("REMARK")
                        If Convert.ToString(drInput("DELFLG")) = "" Then
                            workBaseRow("DELFLG") = BaseDllCommon.CONST_FLAG_NO
                        Else
                            workBaseRow("DELFLG") = drInput("DELFLG")
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
        txtToriKbn.Text = Convert.ToString(dataTable(0)("TORIKBN"))
        txtToriKbn_Change()
        txtToriCode.Text = Convert.ToString(dataTable(0)("TORICODE"))
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
                Case "★" & newDisp
                    BASEtbl.Rows(i)(1) = newDisp
                Case "★" & deleteDisp
                    BASEtbl.Rows(i)(1) = deleteDisp
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
            Case newDisp
                BASEtbl.Rows(lineCnt)(1) = "★" & newDisp
            Case deleteDisp
                BASEtbl.Rows(lineCnt)(1) = "★" & deleteDisp
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
        If TypeOf Page.PreviousPage Is GBM00021SELECT Then
            '検索画面の場合
            Dim prevObj As GBM00021SELECT = DirectCast(Page.PreviousPage, GBM00021SELECT)

            Me.hdnSelectedStYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtStYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            Me.hdnSelectedEndYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtEndYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            If hdnSelectedEndYMD.Value = "" Then
                hdnSelectedEndYMD.Value = hdnSelectedStYMD.Value
            End If

            Me.hdnSelectedToriKbn.Value = DirectCast(prevObj.FindControl("txtToriKbn"), TextBox).Text

            Me.hdnSelectedToriCode.Value = DirectCast(prevObj.FindControl("txtToriCode"), TextBox).Text

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
    ''' <summary>
    ''' リピーターのフィールド名を元にテキストボックス及び、文言ラベルを取得する
    ''' </summary>
    ''' <param name="repObj"></param>
    ''' <param name="fieldName"></param>
    ''' <returns>テキストボックス、ラベルオブジェクト（添え字0:テキストボックス、添え字1:ラベル)</returns>
    Private Function GetRepObjects(repObj As Repeater, fieldName As String) As List(Of Control)
        Dim retItem As New List(Of Control)
        '返すコントロールが無い場合そのまま終了
        If repObj.HasControls = False OrElse repObj.Items.Count = 0 Then
            Return Nothing
        End If
        Dim targetFieldNameLabel As Label = Nothing
        Dim repRowObj As RepeaterItem = Nothing
        For Each repItem In repObj.Items.Cast(Of RepeaterItem)
            '対象のラベルのうちフィールド名と完全一致する列を算出
            Dim qFindControl = From ctr In repItem.Controls.Cast(Of Control) Where TypeOf ctr Is Label _
                                                                             AndAlso DirectCast(ctr, Label).Text = fieldName _
                                                                             AndAlso ctr.ID.Contains("_FIELD_")
            '対象のIDのコントロールが無い場合
            If qFindControl.Any = True Then
                targetFieldNameLabel = DirectCast(qFindControl.FirstOrDefault, Label)
                repRowObj = DirectCast(targetFieldNameLabel.Parent, RepeaterItem)
                Exit For
            End If

        Next repItem
        '対象フィールドのコントロールが存在しない場合はそのまま終了
        If targetFieldNameLabel Is Nothing Then
            Return Nothing
        End If

        Dim txtId As String = targetFieldNameLabel.ID.Replace("FIELD", "VALUE")
        Dim lblId As String = targetFieldNameLabel.ID.Replace("FIELD", "VALUE_TEXT")
        Dim txtObj As TextBox = DirectCast(repRowObj.FindControl(txtId), TextBox)
        Dim lblObj As Label = DirectCast(repRowObj.FindControl(lblId), Label)
        'テキストボックス及びラベルが存在しない場合は終了
        If txtObj Is Nothing OrElse lblObj Is Nothing Then
            Return Nothing
        End If
        'フィールド名を元に発見した対象のラベル、テキストボックスを返却
        retItem.AddRange({txtObj, lblObj})
        Return retItem
    End Function
End Class
