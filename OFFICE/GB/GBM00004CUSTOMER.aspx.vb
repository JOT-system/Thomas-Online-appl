﻿Imports System.Data.SqlClient
Imports System.Drawing
Imports System.Net
Imports BASEDLL

''' <summary>
''' 顧客マスタ画面クラス
''' </summary>
Public Class GBM00004CUSTOMER
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBM00004"    '自身のMAPID
    Private Const CONST_BASEDATATABLE = "GBM00004TBL"
    Private Const CONST_INPDATATABLE = "GBM00004INPTBL"
    Private Const CONST_UPDDATATABLE = "GBM00004UPDTBL"
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_TBLMASTER = "GBM0004_CUSTOMER"
    Private Const CONST_TBLAPPLY = "GBM0023_CUSTOMERAPPLY"
    Private Const CONST_EVENTCODE = "MasterApplyCustomer"

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
                hdnDTABChange.Value = "0"
                DetailTABChange()
                hdnDTABChange.Value = ""
                '****************************************
                'フォーカス設定
                '****************************************
                txtOperationEx.Focus()
                txtCustomerCode.Enabled = False

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
                & "       '1' as 'SELECT'                   , " _
                & "       '0' as HIDDEN                     , " _
                & "       APPLYID                           , " _
                & "       COMPCODE                          , " _
                & "       COUNTRYCODE                       , " _
                & "       CUSTOMERCODE                      , " _
                & "       STYMD                             , " _
                & "       ENDYMD                            , " _
                & "       NAMES                             , " _
                & "       NAMEL                             , " _
                & "       NAMESEN                           , " _
                & "       NAMELEN                           , " _
                & "       CUSTOMERTYPE                      , " _
                & "       POSTNUM                           , " _
                & "       ADDR                              , " _
                & "       ADDRJP                            , " _
                & "       CITY                              , " _
                & "       CITYJP                            , " _
                & "       ADDRBL                            , " _
                & "       BLNAME                            , " _
                & "       CONSIGNEEIEC                      , " _
                & "       TEL                               , " _
                & "       FAX                               , " _
                & "       CONTACTORG                        , " _
                & "       CONTACTPERSON                     , " _
                & "       CONTACTMAIL                       , " _
                & "       MORG                              , " _
                & "       ACCAMPCODE                        , " _
                & "       ACTORICODE                        , " _
                & "       ACTORICODES                       , " _
                & "       ACCCURRENCYSEGMENT                , " _
                & "       BOTHCLASS                         , " _
                & "       TORICOMP                          , " _
                & "       INCTORICODE                       , " _
                & "       EXPTORICODE                       , " _
                & "       DEPOSITDAY                        , " _
                & "       DEPOSITADDMM                      , " _
                & "       OVERDRAWDAY                       , " _
                & "       OVERDRAWADDMM                     , " _
                & "       HOLIDAYFLG                        , " _
                & "       REMARK                            , " _
                & "       DELFLG                            , " _
                & "       UPDYMD                            , " _
                & "       UPDUSER                           , " _
                & "       UPDTERMID                           " _
                & "  FROM (" _
                & "SELECT " _
                & "       '' as APPLYID , " _
                & "       isnull(rtrim(COMPCODE),'')                    as COMPCODE , " _
                & "       isnull(rtrim(COUNTRYCODE),'')                 as COUNTRYCODE , " _
                & "       isnull(rtrim(CUSTOMERCODE),'')                as CUSTOMERCODE , " _
                & "       isnull(convert(nvarchar, STYMD , 111),'')     as STYMD , " _
                & "       isnull(convert(nvarchar, ENDYMD , 111),'')    as ENDYMD , " _
                & "       isnull(rtrim(NAMES),'')                       as NAMES , " _
                & "       isnull(rtrim(NAMEL),'')                       as NAMEL , " _
                & "       isnull(rtrim(NAMESEN),'')                     as NAMESEN , " _
                & "       isnull(rtrim(NAMELEN),'')                     as NAMELEN , " _
                & "       isnull(rtrim(CUSTOMERTYPE),'')                as CUSTOMERTYPE , " _
                & "       isnull(rtrim(POSTNUM),'')                     as POSTNUM , " _
                & "       isnull(rtrim(ADDR),'')                        as ADDR , " _
                & "       isnull(rtrim(ADDRJP),'')                      as ADDRJP , " _
                & "       isnull(rtrim(CITY),'')                        as CITY , " _
                & "       isnull(rtrim(CITYJP),'')                      as CITYJP , " _
                & "       isnull(rtrim(ADDRBL),'')                      as ADDRBL , " _
                & "       isnull(rtrim(BLNAME),'')                      as BLNAME , " _
                & "       isnull(rtrim(CONSIGNEEIEC),'')                as CONSIGNEEIEC , " _
                & "       isnull(rtrim(TEL),'')                         as TEL , " _
                & "       isnull(rtrim(FAX),'')                         as FAX , " _
                & "       isnull(rtrim(CONTACTORG),'')                  as CONTACTORG , " _
                & "       isnull(rtrim(CONTACTPERSON),'')               as CONTACTPERSON , " _
                & "       isnull(rtrim(CONTACTMAIL),'')                 as CONTACTMAIL , " _
                & "       isnull(rtrim(MORG),'')                        as MORG , " _
                & "       isnull(rtrim(ACCAMPCODE),'')                  as ACCAMPCODE , " _
                & "       isnull(rtrim(ACTORICODE),'')                  as ACTORICODE , " _
                & "       isnull(rtrim(ACTORICODES),'')                 as ACTORICODES , " _
                & "       isnull(rtrim(ACCCURRENCYSEGMENT),'')          as ACCCURRENCYSEGMENT , " _
                & "       isnull(rtrim(BOTHCLASS),'')                   as BOTHCLASS , " _
                & "       isnull(rtrim(TORICOMP),'')                    as TORICOMP , " _
                & "       isnull(rtrim(INCTORICODE),'')                 as INCTORICODE , " _
                & "       isnull(rtrim(EXPTORICODE),'')                 as EXPTORICODE , " _
                & "       isnull(rtrim(DEPOSITDAY),'')                  as DEPOSITDAY , " _
                & "       isnull(rtrim(DEPOSITADDMM),'')                as DEPOSITADDMM , " _
                & "       isnull(rtrim(OVERDRAWDAY),'')                 as OVERDRAWDAY , " _
                & "       isnull(rtrim(OVERDRAWADDMM),'')               as OVERDRAWADDMM , " _
                & "       isnull(rtrim(HOLIDAYFLG),'')                  as HOLIDAYFLG , " _
                & "       isnull(rtrim(REMARK),'')                      as REMARK , " _
                & "       isnull(rtrim(DELFLG),'')                      as DELFLG , " _
                & "       isnull(convert(nvarchar, UPDYMD , 120),'')    as UPDYMD , " _
                & "       isnull(rtrim(UPDUSER),'')                     as UPDUSER , " _
                & "       isnull(rtrim(UPDTERMID),'')                   as UPDTERMID , " _
                & "       TIMSTP = cast(UPDTIMSTP                       as bigint) " _
                & " FROM " & CONST_TBLMASTER & " as tbl1 " _
                & " WHERE DELFLG    <> @P4 " _
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
                    & " AND   tbl1.COUNTRYCODE = tbl2.COUNTRYCODE " _
                    & " AND   tbl1.CUSTOMERCODE = tbl2.CUSTOMERCODE " _
                    & " AND   tbl1.STYMD = tbl2.STYMD " _
                    & " AND   tbl1.DELFLG <> @P4 " _
                    & " AND   tbl2.DELFLG <> @P4 "
            End If
            SQLStr &= " )" _
                & " UNION ALL " _
                & "SELECT " _
                & "       isnull(rtrim(APPLYID),'')                     as APPLYID , " _
                & "       isnull(rtrim(COMPCODE),'')                    as COMPCODE , " _
                & "       isnull(rtrim(COUNTRYCODE),'')                 as COUNTRYCODE , " _
                & "       isnull(rtrim(CUSTOMERCODE),'')                as CUSTOMERCODE , " _
                & "       isnull(convert(nvarchar, STYMD , 111),'')     as STYMD , " _
                & "       isnull(convert(nvarchar, ENDYMD , 111),'')    as ENDYMD , " _
                & "       isnull(rtrim(NAMES),'')                       as NAMES , " _
                & "       isnull(rtrim(NAMEL),'')                       as NAMEL , " _
                & "       isnull(rtrim(NAMESEN),'')                     as NAMESEN , " _
                & "       isnull(rtrim(NAMELEN),'')                     as NAMELEN , " _
                & "       isnull(rtrim(CUSTOMERTYPE),'')                as CUSTOMERTYPE , " _
                & "       isnull(rtrim(POSTNUM),'')                     as POSTNUM , " _
                & "       isnull(rtrim(ADDR),'')                        as ADDR , " _
                & "       isnull(rtrim(ADDRJP),'')                      as ADDRJP , " _
                & "       isnull(rtrim(CITY),'')                        as CITY , " _
                & "       isnull(rtrim(CITYJP),'')                      as CITYJP , " _
                & "       isnull(rtrim(ADDRBL),'')                      as ADDRBL , " _
                & "       isnull(rtrim(BLNAME),'')                      as BLNAME , " _
                & "       isnull(rtrim(CONSIGNEEIEC),'')                as CONSIGNEEIEC , " _
                & "       isnull(rtrim(TEL),'')                         as TEL , " _
                & "       isnull(rtrim(FAX),'')                         as FAX , " _
                & "       isnull(rtrim(CONTACTORG),'')                  as CONTACTORG , " _
                & "       isnull(rtrim(CONTACTPERSON),'')               as CONTACTPERSON , " _
                & "       isnull(rtrim(CONTACTMAIL),'')                 as CONTACTMAIL , " _
                & "       isnull(rtrim(MORG),'')                        as MORG , " _
                & "       isnull(rtrim(ACCAMPCODE),'')                  as ACCAMPCODE , " _
                & "       isnull(rtrim(ACTORICODE),'')                  as ACTORICODE , " _
                & "       isnull(rtrim(ACTORICODES),'')                 as ACTORICODES , " _
                & "       isnull(rtrim(ACCCURRENCYSEGMENT),'')          as ACCCURRENCYSEGMENT , " _
                & "       isnull(rtrim(BOTHCLASS),'')                   as BOTHCLASS , " _
                & "       isnull(rtrim(TORICOMP),'')                    as TORICOMP , " _
                & "       isnull(rtrim(INCTORICODE),'')                 as INCTORICODE , " _
                & "       isnull(rtrim(EXPTORICODE),'')                 as EXPTORICODE , " _
                & "       isnull(rtrim(DEPOSITDAY),'')                  as DEPOSITDAY , " _
                & "       isnull(rtrim(DEPOSITADDMM),'')                as DEPOSITADDMM , " _
                & "       isnull(rtrim(OVERDRAWDAY),'')                 as OVERDRAWDAY , " _
                & "       isnull(rtrim(OVERDRAWADDMM),'')               as OVERDRAWADDMM , " _
                & "       isnull(rtrim(HOLIDAYFLG),'')                  as HOLIDAYFLG , " _
                & "       isnull(rtrim(REMARK),'')                      as REMARK , " _
                & "       isnull(rtrim(DELFLG),'')                      as DELFLG , " _
                & "       isnull(convert(nvarchar, UPDYMD , 120),'')    as UPDYMD , " _
                & "       isnull(rtrim(UPDUSER),'')                     as UPDUSER , " _
                & "       isnull(rtrim(UPDTERMID),'')                   as UPDTERMID , " _
                & "       TIMSTP = cast(UPDTIMSTP                       as bigint) " _
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

                '国コード
                If (String.IsNullOrEmpty(Me.hdnSelectedCountryCode.Value) = False) Then
                    SQLStr &= String.Format(" AND COUNTRYCODE = '{0}' ", Me.hdnSelectedCountryCode.Value)
                End If

                '顧客コード
                If (String.IsNullOrEmpty(Me.hdnSelectedCustomerCode.Value) = False) Then
                    SQLStr &= String.Format(" AND CUSTOMERCODE = '{0}' ", Me.hdnSelectedCustomerCode.Value)
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

        Dim sameDr As EnumerableRowCollection(Of DataRow)

        'INPtblカラム設定
        BASEtblColumnsAdd(INPtbl)

        'Excelデータ毎にチェック＆更新
        For i As Integer = 0 To COA0029XlsTable.TBLDATA.Rows.Count - 1

            'XLSTBL明細⇒INProw
            INProwWork = INPtbl.NewRow

            Dim countryCode As String = COA0029XlsTable.TBLDATA.Rows(i)("COUNTRYCODE").ToString
            Dim customerCode As String = COA0029XlsTable.TBLDATA.Rows(i)("CUSTOMERCODE").ToString
            If COA0029XlsTable.TBLDATA.Columns.Contains("STYMD") Then
                Dim stYmd As String = COA0029XlsTable.TBLDATA.Rows(i)("STYMD").ToString
                sameDr = (From item In BASEtbl Where item("COUNTRYCODE").Equals(countryCode) AndAlso item("CUSTOMERCODE").Equals(customerCode) AndAlso item("STYMD").Equals(stYmd))
            Else
                sameDr = (From item In BASEtbl Where item("COUNTRYCODE").Equals(countryCode) AndAlso item("CUSTOMERCODE").Equals(customerCode))
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

        'INPtbl新規データ仮採番
        INPtblSetNewSeq()

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
                '国コードビュー表示切替
                Case Me.vLeftCountryCode.ID
                    SetCountryCodeListItem(Me.txtCountryCode.Text)
                '削除フラグビュー表示切替
                Case Me.vLeftDelFlg.ID
                    SetDelFlgListItem(Me.txtDelFlg.Text)
                '顧客種類ビュー表示切替
                Case Me.vLeftCustomerType.ID
                    SetCustomerTypeListItem()
                '経理円貨外貨区分
                Case Me.vLeftAccCurrencySegment.ID
                    SetAccCurrencySegmentListItem()
                '両建区分
                Case Me.vLeftBothClass.ID
                    SetBothClassListItem()
                '取引先コード
                Case Me.vLeftToriCode.ID
                    SetToriCodeListItem()
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
        Dim opeNew As String = Nothing
        Dim opeUpdate As String = Nothing
        Dim opeDelete As String = Nothing
        Dim opeErr As String = Nothing
        Dim opeUpdErr As String = Nothing
        Dim blank As String = Nothing

        '比較文字設定
        If (COA0019Session.LANGDISP = C_LANG.JA) Then
            opeAll = "全て"
            opeNew = "新規"
            opeUpdate = "更新"
            opeDelete = "削除"
            opeErr = "エラー"
            opeUpdErr = "更新エラー"
            blank = "空白"
        Else
            opeAll = "ALL"
            opeNew = "NEW"
            opeUpdate = "UPDATE"
            opeDelete = "DELETE"
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
                ElseIf txtOperationEx.Text = opeNew Then
                    '新規を表示
                    If (searchStr <> opeNew) Then
                        BASEtbl.Rows(i)("HIDDEN") = 1
                    End If
                ElseIf txtOperationEx.Text = opeUpdate Then
                    '更新を表示
                    If (searchStr <> opeUpdate) Then
                        BASEtbl.Rows(i)("HIDDEN") = 1
                    End If
                ElseIf txtOperationEx.Text = opeDelete Then
                    '削除を表示
                    If (searchStr <> opeDelete) Then
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

            '顧客名称 絞込判定
            If (Convert.ToString(BASEtbl.Rows(i)("HIDDEN")) = "0") AndAlso (txtNamesenEx.Text <> "") Then
                Dim searchStr As String = ""
                '検索用文字列（部分一致）
                If (COA0019Session.LANGDISP = C_LANG.JA) Then
                    searchStr = Convert.ToString(BASEtbl.Rows(i)("NAMES")).ToUpper
                Else
                    searchStr = Convert.ToString(BASEtbl.Rows(i)("NAMESEN")).ToUpper
                End If

                If Not searchStr.Contains(txtNamesenEx.Text.ToUpper) Then
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
                'If BASEtbl.Rows(i)("OPERATION") = updateDisp AndAlso BASEtbl.Rows(i)("TIMSTP") <> "0" Then
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
                             & "   and COUNTRYCODE = @P02 " _
                             & "   and CUSTOMERCODE = @P03 " _
                             & "   and STYMD = @P04 " _
                             & "   and DELFLG <> @P05 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        With SQLcmd.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COUNTRYCODE")
                            .Add("@P03", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("CUSTOMERCODE")
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
                                       Convert.ToString(BASEtbl.Rows(j)("COUNTRYCODE")) = Convert.ToString(BASEtbl.Rows(i)("COUNTRYCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("CUSTOMERCODE")) = Convert.ToString(BASEtbl.Rows(i)("CUSTOMERCODE")) AndAlso
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
                             & "   and COUNTRYCODE = @P02 " _
                             & "   and CUSTOMERCODE = @P03 " _
                             & "   and STYMD = @P04 " _
                             & "   and DELFLG <> @P05 ; "

                        SQLcmd = New SqlCommand(SQLStr, SQLcon)
                        With SQLcmd.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COUNTRYCODE")
                            .Add("@P03", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("CUSTOMERCODE")
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
                                       Convert.ToString(BASEtbl.Rows(j)("COUNTRYCODE")) = Convert.ToString(BASEtbl.Rows(i)("COUNTRYCODE")) AndAlso
                                       Convert.ToString(BASEtbl.Rows(j)("CUSTOMERCODE")) = Convert.ToString(BASEtbl.Rows(i)("CUSTOMERCODE")) AndAlso
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

                    'If (BASEtbl.Rows(i)("OPERATION") = updateDisp OrElse BASEtbl.Rows(i)("OPERATION") = "★" & updateDisp) Then
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
                            'GBA00009MailSendSet.EVENTCODE = C_CSTMSTEVENT.APPLY
                            'GBA00009MailSendSet.STATUS = Convert.ToString(BASEtbl.Rows(i)("SAVESTATUS"))
                            'GBA00009MailSendSet.MAILSUBCODE = ""
                            'GBA00009MailSendSet.APPLYID = Convert.ToString(BASEtbl.Rows(i)("APPLYID"))
                            'GBA00009MailSendSet.GBA00009setMailToUserM()
                            'If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                            '    CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                            '    Return
                            'End If

                        End If

                        '新規レコード
                        If Convert.ToString(BASEtbl.Rows(i)("TIMSTP")) = "0" Then
                            '新規オーダー番号生成（シーケンスより取得）
                            BASEtbl.Rows(i)("CUSTOMERCODE") = GetNewSeq(Convert.ToString(BASEtbl.Rows(i)("COUNTRYCODE")), SQLcon)
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
                                 & "    AND COUNTRYCODE = @P03  " _
                                 & "    AND CUSTOMERCODE = @P04  " _
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
                                 & "        NAMES = @P07 , " _
                                 & "        NAMEL = @P08 , " _
                                 & "        NAMESEN = @P09 , " _
                                 & "        NAMELEN = @P10 , " _
                                 & "        CUSTOMERTYPE = @P11 , " _
                                 & "        POSTNUM = @P12 , " _
                                 & "        ADDR = @P13 , " _
                                 & "        ADDRJP = @P14 , " _
                                 & "        CITY = @P34 , " _
                                 & "        CITYJP = @P35 , " _
                                 & "        ADDRBL = @P15 , " _
                                 & "        BLNAME = @P16 , " _
                                 & "        CONSIGNEEIEC = @P17 , " _
                                 & "        TEL = @P18 , " _
                                 & "        FAX = @P19 , " _
                                 & "        CONTACTORG = @P20 , " _
                                 & "        CONTACTPERSON = @P21 , " _
                                 & "        CONTACTMAIL = @P22 , " _
                                 & "        MORG = @P23 , " _
                                 & "        ACCAMPCODE = @P24 , " _
                                 & "        ACTORICODE = @P25 , " _
                                 & "        ACTORICODES = @P26 , " _
                                 & "        ACCCURRENCYSEGMENT = @P36 , " _
                                 & "        BOTHCLASS = @P37 , " _
                                 & "        TORICOMP = @P38 , " _
                                 & "        INCTORICODE = @P39 , " _
                                 & "        EXPTORICODE = @P40 , " _
                                 & "        DEPOSITDAY = @P41 , " _
                                 & "        DEPOSITADDMM = @P42 , " _
                                 & "        OVERDRAWDAY = @P43 , " _
                                 & "        OVERDRAWADDMM = @P44 , " _
                                 & "        HOLIDAYFLG = @P45 , " _
                                 & "        REMARK = @P27 , " _
                                 & "        DELFLG = @P28 , " _
                                 & "        UPDYMD = @P30 , " _
                                 & "        UPDUSER = @P31 , " _
                                 & "        UPDTERMID = @P32 , " _
                                 & "        RECEIVEYMD = @P33  " _
                                 & "  WHERE COMPCODE = @P02 " _
                                 & "    AND COUNTRYCODE = @P03 " _
                                 & "    AND CUSTOMERCODE = @P04 " _
                                 & "    AND STYMD = @P05 ; " _
                                 & " IF ( @@FETCH_STATUS <> 0 ) " _
                                 & "  INSERT INTO " & updTable _
                                 & "       ("
                        If Convert.ToString(BASEtbl.Rows(i)("APPLYID")) <> "" Then
                            SQLStr = SQLStr & " APPLYID , "
                        End If
                        SQLStr = SQLStr & " COMPCODE , " _
                                 & "        COUNTRYCODE , " _
                                 & "        CUSTOMERCODE , " _
                                 & "        STYMD , " _
                                 & "        ENDYMD , " _
                                 & "        NAMES , " _
                                 & "        NAMEL , " _
                                 & "        NAMESEN , " _
                                 & "        NAMELEN , " _
                                 & "        CUSTOMERTYPE , " _
                                 & "        POSTNUM , " _
                                 & "        ADDR , " _
                                 & "        ADDRJP , " _
                                 & "        CITY , " _
                                 & "        CITYJP , " _
                                 & "        ADDRBL , " _
                                 & "        BLNAME , " _
                                 & "        CONSIGNEEIEC , " _
                                 & "        TEL , " _
                                 & "        FAX , " _
                                 & "        CONTACTORG , " _
                                 & "        CONTACTPERSON , " _
                                 & "        CONTACTMAIL , " _
                                 & "        MORG , " _
                                 & "        ACCAMPCODE , " _
                                 & "        ACTORICODE , " _
                                 & "        ACTORICODES , " _
                                 & "        ACCCURRENCYSEGMENT , " _
                                 & "        BOTHCLASS , " _
                                 & "        TORICOMP , " _
                                 & "        INCTORICODE , " _
                                 & "        EXPTORICODE , " _
                                 & "        DEPOSITDAY , " _
                                 & "        DEPOSITADDMM , " _
                                 & "        OVERDRAWDAY , " _
                                 & "        OVERDRAWADDMM , " _
                                 & "        HOLIDAYFLG , " _
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
                                 & "           @P11,@P12,@P13,@P14,@P34,@P35,@P15,@P16,@P17,@P18,@P19,@P20, " _
                                 & "           @P21,@P22,@P23,@P24,@P25,@P26,@P36,@P37,@P38,@P39,@P40,@P41, " _
                                 & "           @P42,@P43,@P44,@P45,@P27,@P28,@P29,@P30,@P31,@P32,@P33); " _
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
                        Dim PARA28 As SqlParameter = SQLcmd.Parameters.Add("@P28", System.Data.SqlDbType.NVarChar)
                        Dim PARA29 As SqlParameter = SQLcmd.Parameters.Add("@P29", System.Data.SqlDbType.DateTime)
                        Dim PARA30 As SqlParameter = SQLcmd.Parameters.Add("@P30", System.Data.SqlDbType.DateTime)
                        Dim PARA31 As SqlParameter = SQLcmd.Parameters.Add("@P31", System.Data.SqlDbType.NVarChar)
                        Dim PARA32 As SqlParameter = SQLcmd.Parameters.Add("@P32", System.Data.SqlDbType.NVarChar)
                        Dim PARA33 As SqlParameter = SQLcmd.Parameters.Add("@P33", System.Data.SqlDbType.DateTime)

                        Dim PARA34 As SqlParameter = SQLcmd.Parameters.Add("@P34", System.Data.SqlDbType.NVarChar)
                        Dim PARA35 As SqlParameter = SQLcmd.Parameters.Add("@P35", System.Data.SqlDbType.NVarChar)

                        Dim PARA36 As SqlParameter = SQLcmd.Parameters.Add("@P36", System.Data.SqlDbType.NVarChar)
                        Dim PARA37 As SqlParameter = SQLcmd.Parameters.Add("@P37", System.Data.SqlDbType.NVarChar)
                        Dim PARA38 As SqlParameter = SQLcmd.Parameters.Add("@P38", System.Data.SqlDbType.NVarChar)
                        Dim PARA39 As SqlParameter = SQLcmd.Parameters.Add("@P39", System.Data.SqlDbType.NVarChar)
                        Dim PARA40 As SqlParameter = SQLcmd.Parameters.Add("@P40", System.Data.SqlDbType.NVarChar)
                        Dim PARA41 As SqlParameter = SQLcmd.Parameters.Add("@P41", System.Data.SqlDbType.NVarChar)
                        Dim PARA42 As SqlParameter = SQLcmd.Parameters.Add("@P42", System.Data.SqlDbType.Int)
                        Dim PARA43 As SqlParameter = SQLcmd.Parameters.Add("@P43", System.Data.SqlDbType.NVarChar)
                        Dim PARA44 As SqlParameter = SQLcmd.Parameters.Add("@P44", System.Data.SqlDbType.Int)
                        Dim PARA45 As SqlParameter = SQLcmd.Parameters.Add("@P45", System.Data.SqlDbType.NVarChar)

                        PARA01.Value = BASEtbl.Rows(i)("APPLYID")
                        PARA02.Value = BASEtbl.Rows(i)("COMPCODE")
                        PARA03.Value = BASEtbl.Rows(i)("COUNTRYCODE")
                        PARA04.Value = BASEtbl.Rows(i)("CUSTOMERCODE")
                        PARA05.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("STYMD")))
                        PARA06.Value = RTrim(Convert.ToString(BASEtbl.Rows(i)("ENDYMD")))
                        PARA07.Value = BASEtbl.Rows(i)("NAMES")
                        PARA08.Value = BASEtbl.Rows(i)("NAMEL")
                        PARA09.Value = BASEtbl.Rows(i)("NAMESEN")
                        PARA10.Value = BASEtbl.Rows(i)("NAMELEN")
                        PARA11.Value = BASEtbl.Rows(i)("CUSTOMERTYPE")
                        PARA12.Value = BASEtbl.Rows(i)("POSTNUM")
                        PARA13.Value = BASEtbl.Rows(i)("ADDR")
                        PARA14.Value = BASEtbl.Rows(i)("ADDRJP")
                        PARA15.Value = BASEtbl.Rows(i)("ADDRBL")
                        PARA16.Value = BASEtbl.Rows(i)("BLNAME")
                        PARA17.Value = BASEtbl.Rows(i)("CONSIGNEEIEC")
                        PARA18.Value = BASEtbl.Rows(i)("TEL")
                        PARA19.Value = BASEtbl.Rows(i)("FAX")
                        PARA20.Value = BASEtbl.Rows(i)("CONTACTORG")
                        PARA21.Value = BASEtbl.Rows(i)("CONTACTPERSON")
                        PARA22.Value = BASEtbl.Rows(i)("CONTACTMAIL")
                        PARA23.Value = BASEtbl.Rows(i)("MORG")
                        PARA24.Value = BASEtbl.Rows(i)("ACCAMPCODE")
                        PARA25.Value = BASEtbl.Rows(i)("ACTORICODE")
                        PARA26.Value = BASEtbl.Rows(i)("ACTORICODES")
                        PARA27.Value = BASEtbl.Rows(i)("REMARK")
                        PARA28.Value = BASEtbl.Rows(i)("DELFLG")
                        PARA29.Value = nowDate
                        PARA30.Value = nowDate
                        PARA31.Value = COA0019Session.USERID
                        PARA32.Value = HttpContext.Current.Session("APSRVname")
                        PARA33.Value = CONST_DEFAULT_RECEIVEYMD

                        PARA34.Value = BASEtbl.Rows(i)("CITY")
                        PARA35.Value = BASEtbl.Rows(i)("CITYJP")
                        PARA36.Value = BASEtbl.Rows(i)("ACCCURRENCYSEGMENT")
                        PARA37.Value = BASEtbl.Rows(i)("BOTHCLASS")
                        PARA38.Value = BASEtbl.Rows(i)("TORICOMP")
                        PARA39.Value = BASEtbl.Rows(i)("INCTORICODE")
                        PARA40.Value = BASEtbl.Rows(i)("EXPTORICODE")
                        PARA41.Value = BASEtbl.Rows(i)("DEPOSITDAY")
                        PARA42.Value = BASEtbl.Rows(i)("DEPOSITADDMM")
                        PARA43.Value = BASEtbl.Rows(i)("OVERDRAWDAY")
                        PARA44.Value = BASEtbl.Rows(i)("OVERDRAWADDMM")
                        PARA45.Value = BASEtbl.Rows(i)("HOLIDAYFLG")

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
                                & "   And COUNTRYCODE = @P02 " _
                                & "   And CUSTOMERCODE = @P03 " _
                                & "   And STYMD = @P04 " _
                                & " ;"

                        SQLcmd2 = New SqlCommand(SQLStr2, SQLcon)
                        With SQLcmd2.Parameters
                            .Add("@P01", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COMPCODE")
                            .Add("@P02", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("COUNTRYCODE")
                            .Add("@P03", System.Data.SqlDbType.NVarChar).Value = BASEtbl.Rows(i)("CUSTOMERCODE")
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

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE                             '
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

        'INPtbl新規データ仮採番
        INPtblSetNewSeq()

        ''新規作成用にCustomerCode補正
        'If INPtbl.Rows(0).Item("CUSTOMERCODE").ToString = "" Then
        '    Dim maxCode As String = BASEtbl.Rows.Cast(Of DataRow) _
        '    .Where(Function(row) Left(row("CUSTOMERCODE").ToString, 4) = INPtbl.Rows(0).Item("COUNTRYCODE").ToString & "C0") _
        '    .Select(Function(row) row("CUSTOMERCODE").ToString).Max()
        '    Dim codeNoString As String = maxCode.Replace(INPtbl.Rows(0).Item("COUNTRYCODE").ToString & "C", "")
        '    Dim newCode As String = INPtbl.Rows(0).Item("COUNTRYCODE").ToString & "C" & Format(Integer.Parse(codeNoString) + 1, "00000")
        '    INPtbl.Rows(0).Item("CUSTOMERCODE") = newCode
        'End If

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
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
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
        Dim dupCheckFields = CommonFunctions.CreateCompareFieldList({"COMPCODE", "COUNTRYCODE", "CUSTOMERCODE", "STYMD"})
        Dim drBefor As DataRow = INPtbl.NewRow
        Dim drCurrent As DataRow = INPtbl.NewRow
        For i As Integer = INPtbl.Rows.Count - 1 To 1 Step -1
            'KEY重複
            drBefor = INPtbl.Rows(i - 1)
            drCurrent = INPtbl.Rows(i)
            'If Convert.ToString(drCurrent("COMPCODE")) = Convert.ToString(drBefor("COMPCODE")) AndAlso
            '   Convert.ToString(drCurrent("COUNTRYCODE")) = Convert.ToString(drBefor("COUNTRYCODE")) AndAlso
            '   Convert.ToString(drCurrent("CUSTOMERCODE")) = Convert.ToString(drBefor("CUSTOMERCODE")) AndAlso
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
            Dim errorCodeNotFound As String = C_MESSAGENO.NOTFOUND
            Dim errorMessageNotFound As String = ""
            CommonFunctions.ShowMessage(errorCodeNotFound, dummyMsgBox)
            errorMessageNotFound = dummyMsgBox.Text
            Dim keyFind As Boolean = False

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
                Dim compareFieldList = CommonFunctions.CreateCompareFieldList({"COMPCODE", "COUNTRYCODE", "CUSTOMERCODE"})
                Dim dr As DataRow = BASEtbl.NewRow
                For j As Integer = 0 To BASEtbl.Rows.Count - 1
                    dr.ItemArray = BASEtbl.Rows(j).ItemArray
                    If Convert.ToString(dr("DELFLG")) <> BaseDllCommon.CONST_FLAG_YES Then
                        '日付以外の項目が等しい
                        If CommonFunctions.CompareDataFields(BASEtbl.Rows(j), workInpRow, compareFieldList) Then

                            '更新対象レコードあり
                            keyFind = True

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
                If Convert.ToString(workInpRow("TEMPCODE")) = "" AndAlso keyFind = False Then
                    If returnCode = C_MESSAGENO.NORMAL Then
                        'KEY重複
                        returnCode = C_MESSAGENO.NOTFOUND
                    End If
                    'エラーレポート編集
                    errMessageStr = ""
                    errMessageStr = "・" & errorMessageNotFound
                    ' レコード内容を展開する
                    errMessageStr = errMessageStr & Me.ErrItemSet(workInpRow)
                    If txtRightErrorMessage.Text <> "" Then
                        txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
                    End If
                    txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine & errMessageStr
                End If
            End If
            If returnCode <> C_MESSAGENO.NORMAL Then
                workInpRow("OPERATION") = errDisp
                errListAll.Add(C_MESSAGENO.RIGHTBIXOUT)
                errList.Add(C_MESSAGENO.RIGHTBIXOUT)
                If returnCode = C_MESSAGENO.REQUIREDVALUE OrElse
                    returnCode = C_MESSAGENO.HASAPPLYINGRECORD OrElse
                    returnCode = C_MESSAGENO.NOTFOUND Then ' 一覧反映対象外
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
            rtc &= ControlChars.NewLine & "  --> COUNTRY CODE    =" & Convert.ToString(argRow("COUNTRYCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> CUSTOMER CODE   =" & Convert.ToString(argRow("CUSTOMERCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> EFFECTIVE(FROM) =" & Convert.ToString(argRow("STYMD")) & " , "
            rtc &= ControlChars.NewLine & "  --> DELETE FLG      =" & Convert.ToString(argRow("DELFLG")) & " "
        Else
            rtc &= ControlChars.NewLine & "  --> 会社コード      =" & Convert.ToString(argRow("COMPCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> 国コード        =" & Convert.ToString(argRow("COUNTRYCODE")) & " , "
            rtc &= ControlChars.NewLine & "  --> 顧客コード      =" & Convert.ToString(argRow("CUSTOMERCODE")) & " , "
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
                Case Me.vLeftCountryCode.ID 'アクティブなビューが国コード
                    '国コード選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCountryCode.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbCountryCode.SelectedItem.Value
                            Dim parts As String()
                            parts = Split(Me.lbCountryCode.SelectedItem.Text, ":", -1, CompareMethod.Text)
                            Me.lblCountryCodeText.Text = parts(1)
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblCountryCodeText.Text = ""
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
                Case Me.vLeftCustomerType.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター 顧客種類
                        If Me.lbCustomerType.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            Dim lstCtr = GetRepObjects(WF_DViewRep1, "CUSTOMERTYPE")
                            If lstCtr IsNot Nothing Then
                                DirectCast(lstCtr(0), TextBox).Text = Me.lbCustomerType.SelectedItem.Value
                                DirectCast(lstCtr(1), Label).Text = Me.lbCustomerType.SelectedItem.Text
                                DirectCast(lstCtr(0), TextBox).Focus()
                            End If
                        End If
                    End If
                Case Me.vLeftAccCurrencySegment.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター 経理円貨外貨区分
                        If Me.lbAccCurrencySegment.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            Dim lstCtr = GetRepObjects(WF_DViewRep1, "ACCCURRENCYSEGMENT")
                            If lstCtr IsNot Nothing Then
                                DirectCast(lstCtr(0), TextBox).Text = Me.lbAccCurrencySegment.SelectedItem.Value
                                DirectCast(lstCtr(1), Label).Text = Me.lbAccCurrencySegment.SelectedItem.Text
                                DirectCast(lstCtr(0), TextBox).Focus()
                            End If
                        End If
                    End If
                Case Me.vLeftBothClass.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                    Else
                        'リピーター 両建区分
                        If Me.lbBothClass.SelectedItem IsNot Nothing AndAlso
                            Me.hdnTextDbClickField.Value IsNot Nothing Then
                            Dim lstCtr = GetRepObjects(WF_DViewRep1, "BOTHCLASS")
                            If lstCtr IsNot Nothing Then
                                DirectCast(lstCtr(0), TextBox).Text = Me.lbBothClass.SelectedItem.Value
                                DirectCast(lstCtr(1), Label).Text = Me.lbBothClass.SelectedItem.Text
                                DirectCast(lstCtr(0), TextBox).Focus()
                            End If
                        End If
                    End If
                'Case Me.vLeftToriCode.ID
                '    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                '    If targetObject IsNot Nothing Then
                '    Else
                '        'リピーター 取引先コード
                '        If Me.lbToriCode.SelectedItem IsNot Nothing AndAlso
                '            Me.hdnTextDbClickField.Value IsNot Nothing Then
                '            Dim lstCtr = GetRepObjects(WF_DViewRep1, "TORICODE")
                '            If lstCtr IsNot Nothing Then
                '                DirectCast(lstCtr(0), TextBox).Text = Me.lbToriCode.SelectedItem.Value
                '                'DirectCast(lstCtr(1), Label).Text = Me.lbToriCode.SelectedItem.Text
                '                DirectCast(lstCtr(0), TextBox).Focus()
                '            End If
                '        End If
                '    End If
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
        AddLangSetting(dicDisplayText, Me.lblNamesenEx, "顧客名称", "Customer Name")

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
        AddLangSetting(dicDisplayText, Me.lblCountryCode, "国コード", "Country Code")
        AddLangSetting(dicDisplayText, Me.lblCustomerCode, "顧客コード", "Customer Code")
        AddLangSetting(dicDisplayText, Me.lblYMD, "有効年月日", "Effective Date")
        AddLangSetting(dicDisplayText, Me.lblDelFlg, "削除", "Delete")

        AddLangSetting(dicDisplayText, Me.hdnUploadMessage01, "ファイルアップロード開始", "Start uploading files")
        AddLangSetting(dicDisplayText, Me.hdnUploadError01, "ファイルアップロードが失敗しました。", "File upload failed.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError02, "通信を中止しました。", "Communication was canceled.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError03, "タイムアウトエラーが発生しました。", "A timeout error occurred.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError04, "更新権限がありません。", "You do not have update permission.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError05, "対応外のファイル形式です。", "It is an incompatible file format.")

        AddLangSetting(dicDisplayText, Me.hdnMsgboxFieldName, "編集中のレコードが存在します。終了してもよろしいですか？", "There is a record being edited. Are you sure you want to quit?")

        AddLangSetting(dicDisplayText, Me.lblDtabCustomer, "顧客情報", "Customer Info")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
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
        table.Columns.Add("COUNTRYCODE", GetType(String))
        table.Columns("COUNTRYCODE").DefaultValue = ""
        table.Columns.Add("CUSTOMERCODE", GetType(String))
        table.Columns("CUSTOMERCODE").DefaultValue = ""
        table.Columns.Add("STYMD", GetType(String))
        table.Columns("STYMD").DefaultValue = ""
        table.Columns.Add("ENDYMD", GetType(String))
        table.Columns("ENDYMD").DefaultValue = ""
        table.Columns.Add("NAMES", GetType(String))
        table.Columns("NAMES").DefaultValue = ""
        table.Columns.Add("NAMEL", GetType(String))
        table.Columns("NAMEL").DefaultValue = ""
        table.Columns.Add("NAMESEN", GetType(String))
        table.Columns("NAMESEN").DefaultValue = ""
        table.Columns.Add("NAMELEN", GetType(String))
        table.Columns("NAMELEN").DefaultValue = ""
        table.Columns.Add("CUSTOMERTYPE", GetType(String))
        table.Columns("CUSTOMERTYPE").DefaultValue = ""
        table.Columns.Add("POSTNUM", GetType(String))
        table.Columns("POSTNUM").DefaultValue = ""
        table.Columns.Add("ADDR", GetType(String))
        table.Columns("ADDR").DefaultValue = ""
        table.Columns.Add("ADDRJP", GetType(String))
        table.Columns("ADDRJP").DefaultValue = ""
        table.Columns.Add("CITY", GetType(String))
        table.Columns("CITY").DefaultValue = ""
        table.Columns.Add("CITYJP", GetType(String))
        table.Columns("CITYJP").DefaultValue = ""
        table.Columns.Add("ADDRBL", GetType(String))
        table.Columns("ADDRBL").DefaultValue = ""
        table.Columns.Add("BLNAME", GetType(String))
        table.Columns("BLNAME").DefaultValue = ""
        table.Columns.Add("CONSIGNEEIEC", GetType(String))
        table.Columns("CONSIGNEEIEC").DefaultValue = ""
        table.Columns.Add("TEL", GetType(String))
        table.Columns("TEL").DefaultValue = ""
        table.Columns.Add("FAX", GetType(String))
        table.Columns("FAX").DefaultValue = ""
        table.Columns.Add("CONTACTORG", GetType(String))
        table.Columns("CONTACTORG").DefaultValue = ""
        table.Columns.Add("CONTACTPERSON", GetType(String))
        table.Columns("CONTACTPERSON").DefaultValue = ""
        table.Columns.Add("CONTACTMAIL", GetType(String))
        table.Columns("CONTACTMAIL").DefaultValue = ""
        table.Columns.Add("MORG", GetType(String))
        table.Columns("MORG").DefaultValue = ""
        table.Columns.Add("ACCAMPCODE", GetType(String))
        table.Columns("ACCAMPCODE").DefaultValue = ""
        table.Columns.Add("ACTORICODE", GetType(String))
        table.Columns("ACTORICODE").DefaultValue = ""
        table.Columns.Add("ACTORICODES", GetType(String))
        table.Columns("ACTORICODES").DefaultValue = ""
        table.Columns.Add("ACCCURRENCYSEGMENT", GetType(String))
        table.Columns("ACCCURRENCYSEGMENT").DefaultValue = ""
        table.Columns.Add("BOTHCLASS", GetType(String))
        table.Columns("BOTHCLASS").DefaultValue = ""
        table.Columns.Add("TORICOMP", GetType(String))
        table.Columns("TORICOMP").DefaultValue = ""
        table.Columns.Add("INCTORICODE", GetType(String))
        table.Columns("INCTORICODE").DefaultValue = ""
        table.Columns.Add("EXPTORICODE", GetType(String))
        table.Columns("EXPTORICODE").DefaultValue = ""
        table.Columns.Add("DEPOSITDAY", GetType(String))
        table.Columns("DEPOSITDAY").DefaultValue = ""
        table.Columns.Add("DEPOSITADDMM", GetType(String))
        table.Columns("DEPOSITADDMM").DefaultValue = ""
        table.Columns.Add("OVERDRAWDAY", GetType(String))
        table.Columns("OVERDRAWDAY").DefaultValue = ""
        table.Columns.Add("OVERDRAWADDMM", GetType(String))
        table.Columns("OVERDRAWADDMM").DefaultValue = ""
        table.Columns.Add("HOLIDAYFLG", GetType(String))
        table.Columns("HOLIDAYFLG").DefaultValue = ""
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
        table.Columns.Add("TEMPCODE", GetType(String))

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
        workRow("COUNTRYCODE") = ""
        workRow("CUSTOMERCODE") = ""
        workRow("STYMD") = ""
        workRow("ENDYMD") = ""
        workRow("NAMES") = ""
        workRow("NAMEL") = ""
        workRow("NAMESEN") = ""
        workRow("NAMELEN") = ""
        workRow("CUSTOMERTYPE") = ""
        workRow("POSTNUM") = ""
        workRow("ADDR") = ""
        workRow("ADDRJP") = ""
        workRow("CITY") = ""
        workRow("CITYJP") = ""
        workRow("ADDRBL") = ""
        workRow("BLNAME") = ""
        workRow("CONSIGNEEIEC") = ""
        workRow("TEL") = ""
        workRow("FAX") = ""
        workRow("CONTACTORG") = ""
        workRow("CONTACTPERSON") = ""
        workRow("CONTACTMAIL") = ""
        workRow("MORG") = ""
        workRow("ACCAMPCODE") = ""
        workRow("ACTORICODE") = ""
        workRow("ACTORICODES") = ""
        workRow("ACCCURRENCYSEGMENT") = ""
        workRow("BOTHCLASS") = ""
        workRow("TORICOMP") = ""
        workRow("INCTORICODE") = ""
        workRow("EXPTORICODE") = ""
        workRow("DEPOSITDAY") = ""
        workRow("DEPOSITADDMM") = "0"
        workRow("OVERDRAWDAY") = ""
        workRow("OVERDRAWADDMM") = "0"
        workRow("HOLIDAYFLG") = ""
        workRow("REMARK") = ""
        workRow("DELFLG") = ""
        workRow("UPDYMD") = ""

        workRow("SAVESTATUS") = ""
        workRow("TEMPCODE") = ""

        argTbl.Rows.Add(workRow)

    End Sub
    ''' <summary>
    ''' INPtbl新規データ仮採番 
    ''' </summary>
    Protected Sub INPtblSetNewSeq()

        If String.IsNullOrEmpty(hdnNewRecordSeq.Value) Then
            hdnNewRecordSeq.Value = "0"
        End If
        Dim newSeq As Integer = Convert.ToInt32(hdnNewRecordSeq.Value)

        For Each INProw As DataRow In INPtbl.Rows
            '新規レコード仮連番設定
            If Convert.ToString(INProw("COMPCODE")) <> "" AndAlso
               Convert.ToString(INProw("COUNTRYCODE")) <> "" AndAlso
               Convert.ToString(INProw("CUSTOMERCODE")) = "" AndAlso
               Convert.ToString(INProw("STYMD")) <> "" Then
                newSeq += 1
                INProw("TEMPCODE") = "NEW" & newSeq.ToString("00000")
                INProw("CUSTOMERCODE") = INProw("TEMPCODE")
            End If
        Next

        hdnNewRecordSeq.Value = newSeq.ToString

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
            workRow("COUNTRYCODE") = txtCountryCode.Text
            workRow("CUSTOMERCODE") = txtCustomerCode.Text
            workRow("STYMD") = FormatDateYMD(txtStYMD.Text, GBA00003UserSetting.DATEFORMAT)
            workRow("ENDYMD") = FormatDateYMD(txtEndYMD.Text, GBA00003UserSetting.DATEFORMAT)
            workRow("NAMES") = ""
            workRow("NAMEL") = ""
            workRow("NAMESEN") = ""
            workRow("NAMELEN") = ""
            workRow("CUSTOMERTYPE") = ""
            workRow("POSTNUM") = ""
            workRow("ADDR") = ""
            workRow("ADDRJP") = ""
            workRow("CITY") = ""
            workRow("CITYJP") = ""
            workRow("ADDRBL") = ""
            workRow("BLNAME") = ""
            workRow("CONSIGNEEIEC") = ""
            workRow("TEL") = ""
            workRow("FAX") = ""
            workRow("CONTACTORG") = ""
            workRow("CONTACTPERSON") = ""
            workRow("CONTACTMAIL") = ""
            workRow("MORG") = ""
            workRow("ACCAMPCODE") = ""
            workRow("ACTORICODE") = ""
            workRow("ACTORICODES") = ""
            workRow("ACCCURRENCYSEGMENT") = ""
            workRow("BOTHCLASS") = ""
            workRow("TORICOMP") = ""
            workRow("INCTORICODE") = ""
            workRow("EXPTORICODE") = ""
            workRow("DEPOSITDAY") = ""
            workRow("DEPOSITADDMM") = "0"
            workRow("OVERDRAWDAY") = ""
            workRow("OVERDRAWADDMM") = "0"
            workRow("HOLIDAYFLG") = ""
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

        lblDtabCustomer.Style.Remove("color")
        lblDtabCustomer.Style.Add("color", "blue")
        lblDtabCustomer.Style.Remove("background-color")
        lblDtabCustomer.Style.Add("background-color", "rgb(220,230,240)")
        lblDtabCustomer.Style.Remove("border")
        lblDtabCustomer.Style.Add("border", "1px solid blue")
        lblDtabCustomer.Style.Remove("font-weight")
        lblDtabCustomer.Style.Add("font-weight", "bold")

        'Detail設定処理
        SetDetailDbClick()

        '初期値設定
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
    ''' ダブルクリック処理追加
    ''' </summary>
    ''' <param name="repField"></param>
    ''' <param name="repAttr"></param>
    Protected Sub GetAttributes(ByVal repField As String, ByRef repAttr As String, Optional rowIdx As Integer = 0)

        Select Case repField
            Case "CUSTOMERTYPE"
                '顧客種類
                repAttr = String.Format("Field_DBclick('vLeftCustomerType', '{0}');", rowIdx)
            Case "ACCCURRENCYSEGMENT"
                '経理円貨外貨区分
                repAttr = String.Format("Field_DBclick('vLeftAccCurrencySegment', '{0}');", rowIdx)
            Case "BOTHCLASS"
                '両建区分
                repAttr = String.Format("Field_DBclick('vLeftBothClass', '{0}');", rowIdx)
                'Case "TORICODE"
                '    '取引先コード
                '    repAttr = String.Format("Field_DBclick('vLeftToriCode', '{0}');", rowIdx)
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

        '国コード
        If Me.lbCountryCode.Items.Count <= 0 Then
            SetCountryCodeListItem(Convert.ToString(InpRow("COUNTRYCODE")))
        End If
        ChedckList(Convert.ToString(InpRow("COUNTRYCODE")), lbCountryCode, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("COUNTRYCODE") & ":" & Convert.ToString(InpRow("COUNTRYCODE")) & ")" & errMessageStr
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

        '顧客種類
        If Me.lbCustomerType.Items.Count <= 0 Then
            SetCustomerTypeListItem()
        End If
        ChedckList(Convert.ToString(InpRow("CUSTOMERTYPE")), lbCustomerType, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("CUSTOMERTYPE") & ":" & Convert.ToString(InpRow("CUSTOMERTYPE")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '経理円貨外貨区分
        If Me.lbAccCurrencySegment.Items.Count <= 0 Then
            SetAccCurrencySegmentListItem()
        End If
        ChedckList(Convert.ToString(InpRow("ACCCURRENCYSEGMENT")), lbAccCurrencySegment, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("ACCCURRENCYSEGMENT") & ":" & Convert.ToString(InpRow("ACCCURRENCYSEGMENT")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If

        '両建区分
        If Me.lbBothClass.Items.Count <= 0 Then
            SetBothClassListItem()
        End If
        ChedckList(Convert.ToString(InpRow("BOTHCLASS")), lbBothClass, refErrMessage)
        If returnCode <> C_MESSAGENO.NORMAL Then
            errMessageStr = Me.ErrItemSet(InpRow)
            If txtRightErrorMessage.Text <> "" Then
                txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine
            End If
            txtRightErrorMessage.Text = txtRightErrorMessage.Text & ControlChars.NewLine _
                                           & "・" & refErrMessage & "(" & dicField("BOTHCLASS") & ":" & Convert.ToString(InpRow("BOTHCLASS")) & ")" & errMessageStr
            errFlg = True
            returnCode = C_MESSAGENO.NORMAL
        End If
        ''取引先コード
        'SetToriCodeListItem()
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
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck          '項目チェック
        Dim slExcludeList As New List(Of String)(New String() {",", "&", "'", "|", ";", ControlChars.Quote})

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
            COA0026FieldCheck.CHECKDT = Me.COLtbl
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
    ''' 国コードリストアイテムを設定
    ''' </summary>
    Private Sub SetCountryCodeListItem(selectedValue As String)

        Dim GBA00007OrganizationRelated As New GBA00007OrganizationRelated

        Try

            'リストクリア
            Me.lbCountryCode.Items.Clear()

            GBA00007OrganizationRelated.LISTBOX_COUNTRY = Me.lbCountryCode
            GBA00007OrganizationRelated.GBA00007getLeftListCountry()
            If GBA00007OrganizationRelated.ERR = C_MESSAGENO.NORMAL OrElse GBA00007OrganizationRelated.ERR = C_MESSAGENO.NODATA Then
                Me.lbCountryCode = DirectCast(GBA00007OrganizationRelated.LISTBOX_COUNTRY, ListBox)
            Else
                returnCode = GBA00007OrganizationRelated.ERR
                Return
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbCountryCode.Items.Count > 0 Then
                Dim findListItem = Me.lbCountryCode.Items.FindByValue(selectedValue)
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
    ''' 顧客種類リストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetCustomerTypeListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbCustomerType.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "CUSTOMERTYPE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbCustomerType
        Else
            COA0017FixValue.LISTBOX2 = Me.lbCustomerType
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbCustomerType = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbCustomerType = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbCustomerType.Items.Count > 0 Then
                Dim findListItem = Me.lbCustomerType.Items.FindByValue(selectedValue)
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
    ''' 経理円貨外貨区分フラグリストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetAccCurrencySegmentListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbAccCurrencySegment.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "ACCCURRENCYSEGMENT"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbAccCurrencySegment
        Else
            COA0017FixValue.LISTBOX2 = Me.lbAccCurrencySegment
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbAccCurrencySegment = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbAccCurrencySegment = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbAccCurrencySegment.Items.Count > 0 Then
                Dim findListItem = Me.lbAccCurrencySegment.Items.FindByValue(selectedValue)
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
    ''' 両建区分フラグリストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    Private Sub SetBothClassListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbBothClass.Items.Clear()

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "BOTHCLASS"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbBothClass
        Else
            COA0017FixValue.LISTBOX2 = Me.lbBothClass
        End If

        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then

            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbBothClass = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbBothClass = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If

            '一応現在入力しているテキストと一致するものを選択状態
            If Me.lbBothClass.Items.Count > 0 Then
                Dim findListItem = Me.lbBothClass.Items.FindByValue(selectedValue)
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
    ''' <param name="selectedValue"></param>
    Private Sub SetToriCodeListItem(Optional selectedValue As String = "")
        Dim COA0017FixValue As New COA0017FixValue

        'リストクリア
        Me.lbToriCode.Items.Clear()

        'リスト設定
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT TORICODE AS [CODE]")
        sqlStat.AppendLine("      ,TORICODE AS [NAME]")
        sqlStat.AppendLine("  FROM GBM0025_TORI")
        sqlStat.AppendLine(" WHERE DELFLG <> @DELFLG ")

        Using SQLcon As New SqlConnection(COA0019Session.DBcon),
              SQLcmd As New SqlCommand(sqlStat.ToString, SQLcon)
            SQLcon.Open()

            With SQLcmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
            End With
            Using SQLdr As SqlDataReader = SQLcmd.ExecuteReader()
                While SQLdr.Read
                    'DBからアイテムを設定
                    Me.lbToriCode.Items.Add(New ListItem(Convert.ToString(SQLdr("CODE"))))
                End While
            End Using

        End Using

        '一応現在入力しているテキストと一致するものを選択状態
        If Me.lbToriCode.Items.Count > 0 Then
            Dim findListItem = Me.lbToriCode.Items.FindByValue(selectedValue)
            If findListItem IsNot Nothing Then
                findListItem.Selected = True
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
    ''' 国コード名設定
    ''' </summary>
    Public Sub txtCountryCode_Change()

        Try
            Me.lblCountryCodeText.Text = ""

            SetCountryCodeListItem(Me.txtCountryCode.Text)
            If returnCode = C_MESSAGENO.NORMAL AndAlso Me.lbCountryCode.Items.Count > 0 Then
                Dim findListItem = Me.lbCountryCode.Items.FindByValue(Me.txtCountryCode.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblCountryCodeText.Text = parts(1)
                Else
                    Dim findListItemUpper = Me.lbCountryCode.Items.FindByValue(Me.txtCountryCode.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblCountryCodeText.Text = parts(1)
                        Me.txtCountryCode.Text = parts(0)
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
    ''' リピーター 顧客種類変更時イベント
    ''' </summary>
    Public Sub CUSTOMERTYPE_Change()
        SetCustomerTypeListItem()
        Dim ctrList = GetRepObjects(Me.WF_DViewRep1, "CUSTOMERTYPE")
        If ctrList IsNot Nothing AndAlso Me.lbCustomerType.Items.Count > 0 Then
            Dim findListItem = Me.lbCustomerType.Items.FindByValue(DirectCast(ctrList(0), TextBox).Text)
            If findListItem IsNot Nothing Then
                DirectCast(ctrList(1), Label).Text = findListItem.Text
            End If
        End If
    End Sub
    ''' <summary>
    ''' リピーター 経理円貨外貨区分変更時イベント
    ''' </summary>
    Public Sub ACCCURRENCYSEGMENT_Change()
        SetAccCurrencySegmentListItem()
        Dim ctrList = GetRepObjects(Me.WF_DViewRep1, "ACCCURRENCYSEGMENT")
        If ctrList IsNot Nothing AndAlso Me.lbAccCurrencySegment.Items.Count > 0 Then
            Dim findListItem = Me.lbAccCurrencySegment.Items.FindByValue(DirectCast(ctrList(0), TextBox).Text)
            If findListItem IsNot Nothing Then
                DirectCast(ctrList(1), Label).Text = findListItem.Text
            End If
        End If
    End Sub
    ''' <summary>
    ''' リピーター 両建区分変更時イベント
    ''' </summary>
    Public Sub BOTHCLASS_Change()
        SetBothClassListItem()
        Dim ctrList = GetRepObjects(Me.WF_DViewRep1, "BOTHCLASS")
        If ctrList IsNot Nothing AndAlso Me.lbBothClass.Items.Count > 0 Then
            Dim findListItem = Me.lbBothClass.Items.FindByValue(DirectCast(ctrList(0), TextBox).Text)
            If findListItem IsNot Nothing Then
                DirectCast(ctrList(1), Label).Text = findListItem.Text
            End If
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
        txtCustomerCode.Text = ""
        lblCustomerCodeText.Text = ""
        txtCountryCode.Text = ""
        lblCountryCodeText.Text = ""
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
                    If CommonFunctions.CompareDataFields(workBaseRow, drInput,
                                                         CommonFunctions.CreateCompareFieldList({"COMPCODE", "COUNTRYCODE", "CUSTOMERCODE", "STYMD"})) Then
                        Dim compareFieldList = CommonFunctions.CreateCompareFieldList({"ENDYMD", "NAMES", "NAMEL", "NAMESEN", "NAMELEN",
                                                                                       "CUSTOMERTYPE", "POSTNUM", "ADDR", "ADDRJP", "CITY", "CITYJP", "ADDRBL",
                                                                                       "BLNAME", "CONSIGNEEIEC", "TEL", "FAX", "CONTACTORG",
                                                                                       "CONTACTPERSON", "CONTACTMAIL", "MORG", "ACCAMPCODE",
                                                                                       "ACTORICODE", "ACTORICODES", "ACCCURRENCYSEGMENT", "BOTHCLASS", "TORICOMP",
                                                                                       "INCTORICODE", "EXPTORICODE", "DEPOSITDAY", "DEPOSITADDMM", "OVERDRAWDAY",
                                                                                       "OVERDRAWADDMM", "HOLIDAYFLG", "REMARK", "DELFLG"})
                        ' 変更なし  
                        If Convert.ToString(drInput("OPERATION")) <> errDisp AndAlso
                           CommonFunctions.CompareDataFields(workBaseRow, drInput, compareFieldList) Then
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
                    Dim compareFieldList = CommonFunctions.CreateCompareFieldList({"COMPCODE", "COUNTRYCODE", "CUSTOMERCODE", "STYMD"})
                    '内部テーブル検索
                    For k As Integer = 0 To BASEtbl.Rows.Count - 1

                        Dim workBaseRow2 As DataRow
                        workBaseRow2 = BASEtbl.NewRow
                        workBaseRow2.ItemArray = BASEtbl.Rows(k).ItemArray

                        If CommonFunctions.CompareDataFields(workBaseRow2, drInput, compareFieldList) Then

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
                        workBaseRow("COUNTRYCODE") = drInput("COUNTRYCODE")
                        workBaseRow("CUSTOMERCODE") = drInput("CUSTOMERCODE")
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
                        workBaseRow("NAMES") = drInput("NAMES")
                        workBaseRow("NAMEL") = drInput("NAMEL")
                        workBaseRow("NAMESEN") = drInput("NAMESEN")
                        workBaseRow("NAMELEN") = drInput("NAMELEN")
                        workBaseRow("CUSTOMERTYPE") = drInput("CUSTOMERTYPE")
                        workBaseRow("POSTNUM") = drInput("POSTNUM")
                        workBaseRow("ADDR") = drInput("ADDR")
                        workBaseRow("ADDRJP") = drInput("ADDRJP")
                        workBaseRow("CITY") = drInput("CITY")
                        workBaseRow("CITYJP") = drInput("CITYJP")
                        workBaseRow("ADDRBL") = drInput("ADDRBL")
                        workBaseRow("BLNAME") = drInput("BLNAME")
                        workBaseRow("CONSIGNEEIEC") = drInput("CONSIGNEEIEC")
                        workBaseRow("TEL") = drInput("TEL")
                        workBaseRow("FAX") = drInput("FAX")
                        workBaseRow("CONTACTORG") = drInput("CONTACTORG")
                        workBaseRow("CONTACTPERSON") = drInput("CONTACTPERSON")
                        workBaseRow("CONTACTMAIL") = drInput("CONTACTMAIL")
                        workBaseRow("MORG") = drInput("MORG")
                        workBaseRow("ACCAMPCODE") = drInput("ACCAMPCODE")
                        workBaseRow("ACTORICODE") = drInput("ACTORICODE")
                        workBaseRow("ACTORICODES") = drInput("ACTORICODES")
                        workBaseRow("ACCCURRENCYSEGMENT") = drInput("ACCCURRENCYSEGMENT")
                        workBaseRow("BOTHCLASS") = drInput("BOTHCLASS")
                        workBaseRow("TORICOMP") = drInput("TORICOMP")
                        workBaseRow("INCTORICODE") = drInput("INCTORICODE")
                        workBaseRow("EXPTORICODE") = drInput("EXPTORICODE")
                        workBaseRow("DEPOSITDAY") = drInput("DEPOSITDAY")
                        workBaseRow("DEPOSITADDMM") = drInput("DEPOSITADDMM")
                        workBaseRow("OVERDRAWDAY") = drInput("OVERDRAWDAY")
                        workBaseRow("OVERDRAWADDMM") = drInput("OVERDRAWADDMM")
                        workBaseRow("HOLIDAYFLG") = drInput("HOLIDAYFLG")
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
        txtCustomerCode.Text = Convert.ToString(dataTable(0)("CUSTOMERCODE"))
        txtCountryCode.Text = Convert.ToString(dataTable(0)("COUNTRYCODE"))
        txtCountryCode_Change()
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

        CUSTOMERTYPE_Change()
        ACCCURRENCYSEGMENT_Change()
        BOTHCLASS_Change()

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
        lblDtabCustomer.Style.Remove("color")
        lblDtabCustomer.Style.Add("color", "black")
        lblDtabCustomer.Style.Remove("background-color")
        lblDtabCustomer.Style.Add("background-color", "rgb(255,255,253)")
        lblDtabCustomer.Style.Remove("border")
        lblDtabCustomer.Style.Add("border", "1px solid black")
        lblDtabCustomer.Style.Remove("font-weight")
        lblDtabCustomer.Style.Add("font-weight", "normal")

        Select Case WF_DetailMView.ActiveViewIndex
            Case 0
                '届先情報
                lblDtabCustomer.Style.Remove("color")
                lblDtabCustomer.Style.Add("color", "blue")
                lblDtabCustomer.Style.Remove("background-color")
                lblDtabCustomer.Style.Add("background-color", "rgb(220,230,240)")
                lblDtabCustomer.Style.Remove("border")
                lblDtabCustomer.Style.Add("border", "1px solid blue")
                lblDtabCustomer.Style.Remove("font-weight")
                lblDtabCustomer.Style.Add("font-weight", "bold")

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
        If TypeOf Page.PreviousPage Is GBM00004SELECT Then
            '検索画面の場合
            Dim prevObj As GBM00004SELECT = DirectCast(Page.PreviousPage, GBM00004SELECT)

            Me.hdnSelectedStYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtStYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            Me.hdnSelectedEndYMD.Value = FormatDateYMD(DirectCast(prevObj.FindControl("txtEndYMD"), TextBox).Text, GBA00003UserSetting.DATEFORMAT)

            If hdnSelectedEndYMD.Value = "" Then
                hdnSelectedEndYMD.Value = hdnSelectedStYMD.Value
            End If

            Me.hdnSelectedCountryCode.Value = DirectCast(prevObj.FindControl("txtCountryCode"), TextBox).Text

            Me.hdnSelectedCustomerCode.Value = DirectCast(prevObj.FindControl("txtCustomerCode"), TextBox).Text

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
    ''' <summary>
    ''' 新規連番をシーケンスより取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetNewSeq(ByRef countryCode As String, Optional ByRef sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim newSeq As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT '" & countryCode & "' + 'C' + right('00000' + trim(convert(char,NEXT VALUE FOR " & C_SQLSEQ.CUSTOMER & "_" & countryCode & ")),5)")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get new Seq error")
                    End If

                    newSeq = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return newSeq
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
End Class
