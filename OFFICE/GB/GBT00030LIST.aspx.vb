Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' タンク動静管理画面クラス
''' </summary>
Public Class GBT00030LIST
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00030L" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 25              'マウススクロール時の増分

    Public Class SelectedMode
        Public Const ImportEmptyTank As String = "1"
        Public Const ImportBeforeTransport As String = "2"
        Public Const ImportInTransit As String = "3"
        Public Const ExportEmptyTank As String = "4"
        Public Const ExportBeforeTransport As String = "5"
        Public Const ExportInTransit As String = "6"
        Public Const StockTank As String = "9"
    End Class

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
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
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
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
                errMsg = Me.RightboxInit()
                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetListDataTable()
                    'グリッド用データをファイルに退避
                    With Nothing
                        Dim COA0021ListTable As New COA0021ListTable With {
                            .FILEdir = hdnXMLsaveFile.Value,
                            .TBLDATA = dt
                        }
                        COA0021ListTable.COA0021saveListTable()
                        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                            Return
                        End If
                    End With

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Dim listVari As String = Me.hdnThisMapVariant.Value
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = listVari & "H1"
                        .SRCDATA = CreateListDataTable()
                        .TBLOBJ = WF_LISTAREA_H1
                        .SCROLLTYPE = "0"
                        '.LEVENT = ""
                        '.LFUNC = ""
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .NOCOLUMNWIDTHOPT = -1
                        .TITLEOPT = False
                        .USERSORTOPT = 0
                    End With
                    COA0013TableObject.COA0013SetTableObject()
                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = listVari & "H"
                        .SRCDATA = CreateListDataTable()
                        .TBLOBJ = WF_LISTAREA_H
                        .SCROLLTYPE = "0"
                        '.LEVENT = ""
                        '.LFUNC = ""
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .NOCOLUMNWIDTHOPT = -1
                        .TITLEOPT = False
                        .USERSORTOPT = 0
                    End With
                    COA0013TableObject.COA0013SetTableObject()

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = listVari
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = ""
                        '.LEVENT = "ondblclick"
                        '.LFUNC = "ListDbClick"
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .NOCOLUMNWIDTHOPT = -1
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
                'DO SOMETHING!

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
                If Me.hdnListDBclick.Value <> "" AndAlso Me.hdnListCellclick.Value = "" Then
                    Me.hdnListDBclick.Value = ""
                    'Return '単票ページにリダイレクトするため念のため処理は終わらせる
                End If
                '**********************
                ' 一覧表のCellクリック判定
                '**********************
                If Me.hdnListDBclick.Value <> "" AndAlso Me.hdnListCellclick.Value <> "" Then
                    ListCellClick()
                    Me.hdnListDBclick.Value = ""
                    Me.hdnListCellclick.Value = ""
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

            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            'Me.Page.Form.Attributes.Add("data-mapvari", Me.hdnThisMapVariant.Value)
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        Catch ex As Threading.ThreadAbortException
            'キャンセルやServerTransferにて後続の処理が打ち切られた場合のエラーは発生させない
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.EXCEPTION 'ここは適宜変えてください
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
        Dim dt As DataTable = CreateListDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
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

        Dim outputDt As DataTable
        Dim dispDispRow = (From item In dt Where Convert.ToString(item("HIDDEN")) = "0")
        If dispDispRow.Any = False Then
            Return
        End If
        outputDt = dispDispRow.CopyToDataTable

        '右ボックスの選択レポートIDを取得
        If Me.lbRightList.SelectedItem Is Nothing Then
            '未選択の場合はそのまま終了
            Return
        End If
        Dim reportId As String = Me.lbRightList.SelectedItem.Value

        '帳票出力
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            Dim reportMapId As String = CONST_MAPID
            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = outputDt                              'PARAM04:データ参照tabledata
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
    ''' DBより一覧用データ取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetListDataTable() As DataTable
        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力

        Dim retDt As New DataTable
        Dim sb As New StringBuilder(2048)

        sb.AppendLine("-- リースタンク一覧 ")
        sb.AppendLine("select B.TANKNO ")
        sb.AppendLine(" , isnull(LESD.ACTUALDATE, '1900/01/01') as LESD ")
        sb.AppendLine(" , isnull(LEIN.ACTUALDATE, '1900/01/01') as LEIN ")
        sb.AppendLine(" , ST.ACTIONID as ACTIVITYCODE ")
        sb.AppendLine(" , ST.ORDERNO ")
        sb.AppendLine(" , ST.BASEAREA ")
        sb.AppendLine(" , ST.ROOT ")
        sb.AppendLine(" , PORT.AREANAME ")
        sb.AppendLine("from ( ")
        sb.AppendLine("   select * ")
        sb.AppendLine("   from GBV0002_LEASETANK ")
        sb.AppendLine("   where SHIPPER= @SHIPPER ")
        sb.AppendLine("     and PRODUCTCODE=@PRODUCTCODE ")
        sb.AppendLine(") as B ")
        sb.AppendLine("-- リース登録 ")
        sb.AppendLine("left outer join ( ")
        sb.AppendLine("  select OVLESD.TANKNO, max(OVLESD.ACTUALDATE) as ACTUALDATE ")
        sb.AppendLine("  from GBT0005_ODR_VALUE as OVLESD with(nolock) ")
        sb.AppendLine("  where OVLESD.ACTIONID = 'LESD' ")
        sb.AppendLine("  and   OVLESD.ACTUALDATE <> @INITDATE ")
        sb.AppendLine("  and   OVLESD.DELFLG <> @DELFLG ")
        sb.AppendLine("  group by OVLESD.TANKNO ")
        sb.AppendLine("  ) as LESD ")
        sb.AppendLine("on LESD.TANKNO = B.TANKNO ")
        sb.AppendLine("-- リースイン ")
        sb.AppendLine("left outer join ( ")
        sb.AppendLine("  select OVLEIN.TANKNO, max(OVLEIN.ACTUALDATE) as ACTUALDATE ")
        sb.AppendLine("  from GBT0005_ODR_VALUE as OVLEIN with(nolock) ")
        sb.AppendLine("  where OVLEIN.ACTIONID = 'LEIN' ")
        sb.AppendLine("  and   OVLEIN.ACTUALDATE <> @INITDATE ")
        sb.AppendLine("  and   OVLEIN.DELFLG <> @DELFLG ")
        sb.AppendLine("  group by OVLEIN.TANKNO ")
        sb.AppendLine("  ) as LEIN ")
        sb.AppendLine("on LEIN.TANKNO = B.TANKNO ")
        sb.AppendLine("-- 直近ステータス ")
        sb.AppendLine("left outer join ( ")
        sb.AppendLine("    select ")
        sb.AppendLine("        vt.TANKNO ")
        sb.AppendLine("      , vt.ACTIONID ")
        sb.AppendLine("      , vt.ORDERNO ")
        sb.AppendLine("      , case when vt.ACTIONID='LESD' then ob.LOADPORT1 else case when ob.DISCHARGEPORT1 = 'JPSDJ' then ob.LOADPORT1 else ob.DISCHARGEPORT1 end end as BASEAREA ")
        sb.AppendLine("      , case when vt.ACTIONID='LESD' then '' else case when ob.DISCHARGEPORT1 = 'JPSDJ' then 'I' else 'E' end end as ROOT ")
        sb.AppendLine("    from GBV0001_TANKSTATUS as vt ")
        sb.AppendLine("    inner join GBT0004_ODR_BASE ob on ob.ORDERNO = vt.ORDERNO and ob.DELFLG <> 'Y' ")
        sb.AppendLine("    where vt.RECENT=1 ")
        sb.AppendLine(") as ST ")
        sb.AppendLine("on  ST.TANKNO = B.TANKNO ")
        sb.AppendLine("-- 港マスタ ")
        sb.AppendLine("left outer join ( ")
        sb.AppendLine("  select ")
        sb.AppendLine("    pm.PORTCODE ")
        sb.AppendLine("  , pm.AREANAME ")
        sb.AppendLine("  from GBM0002_PORT as pm with(nolock) ")
        sb.AppendLine("  where pm.COMPCODE = '01' ")
        sb.AppendLine("  and   pm.STYMD <= @STYMD ")
        sb.AppendLine("  and   pm.ENDYMD >= @ENDYMD ")
        sb.AppendLine("   and pm.DELFLG <> @DELFLG ")
        sb.AppendLine(") AS PORT ")
        sb.AppendLine("on PORT.PORTCODE = ST.BASEAREA ")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
            sqlCmd As New SqlCommand(sb.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@INITDATE", SqlDbType.Date).Value = "1900/01/01"
                .Add("@STYMD", SqlDbType.Date).Value = Now()
                .Add("@ENDYMD", SqlDbType.Date).Value = Now()
                .Add("@SHIPPER", SqlDbType.NVarChar, 20).Value = "JPC01082"
                .Add("@PRODUCTCODE", SqlDbType.NVarChar, 20).Value = "000662"

            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using

        Return SummaryDataTable(retDt)
    End Function

    ''' <summary>
    ''' 左ボックス選択ボタン押下時
    ''' </summary>
    Public Sub btnLeftBoxButtonSel_Click()
        Dim targetObject As Control = Nothing
        '現在表示している左ビューを取得

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
        Dim dt As DataTable = CreateListDataTable()

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If

        'ソート
        Dim dvTBLview As DataView
        dvTBLview = New DataView(dt)
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
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")


        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub

    ''' <summary>
    ''' 一覧表用のデータテーブルを作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateListDataTable() As DataTable
        Dim retDt As New DataTable
        '固定部分は追加しておく
        retDt.Columns.Add("LINECNT", GetType(Integer))            'DBの固定フィールド
        retDt.Columns.Add("OPERATION", GetType(String))           'DBの固定フィールド
        retDt.Columns.Add("TIMSTP", GetType(String))              'DBの固定フィールド
        retDt.Columns.Add("SELECT", GetType(Integer))             'DBの固定フィールド
        retDt.Columns.Add("HIDDEN", GetType(Integer))
        Dim colList As New List(Of String) From {"AREANAME", "BASEAREA",
                                                "EETYD", "TKAL", "DOUT", "LOAD", "CYIN", "SHIP", "TRAV", "TRSH", "ARVD",
                                                "ETYD", "ETKAL", "EDOUT", "ECYIN", "ESHIP", "ETRAV", "ETRSH", "EARVD",
                                                "STOK", "TOTAL",
                                                "LEASETANK", "LEASEOUT", "LEASETOTAL",
                                                "OUTPUTDATE"}

        For Each colName As String In colList
            retDt.Columns.Add(colName, GetType(String))
            retDt.Columns(colName).DefaultValue = ""
        Next
        Return retDt
    End Function

    ''' <summary>
    ''' サマリー用動静別編集クラス
    ''' </summary>
    Class TableRowItem
        ''' <summary>
        ''' Activity
        ''' </summary>
        Public act As String
        ''' <summary>
        ''' 輸出入区分
        ''' </summary>
        ''' <remarks>I:Import/E:Export</remarks>
        Public root As String
        ''' <summary>
        ''' オーダー別タンク数
        ''' </summary>
        ''' <remarks>I:Import/E:Export</remarks>
        Public orderNo As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
    End Class

    ''' <summary>
    ''' サマリー一覧編集
    ''' </summary>
    ''' <returns></returns>
    Private Function SummaryDataTable(ByRef dt As DataTable) As DataTable
        Dim leaseTankNum As Integer = 0
        Dim leaseOutNum As Integer = 0
        Dim leaseTotal As Integer = 0
        Dim dicTable = New Dictionary(Of String, Dictionary(Of String, TableRowItem))

        For Each row As DataRow In dt.AsEnumerable

            'リースタンク合計
            leaseTotal += 1
            If row("LESD").ToString > row("LEIN").ToString Then
                'リースアウト済
                leaseOutNum += 1
            Else
                'リースアウト未済orリースイン済
                leaseTankNum += 1
                Continue For
            End If

            Dim area As String = row("AREANAME").ToString
            Dim act As String = row("ACTIVITYCODE").ToString
            Dim root As String = row("ROOT").ToString
            Dim orderNo As String = row("ORDERNO").ToString
            'EXPORT(empty)（回送）時はACTの頭に「E」
            If root = "E" Then
                act = root & act
            End If

            '地域（港）レコード
            Dim tRow As Dictionary(Of String, TableRowItem)
            If dicTable.ContainsKey(area) Then
                tRow = dicTable(area)
            Else
                tRow = New Dictionary(Of String, TableRowItem)
                dicTable(area) = tRow
            End If

            'ACT項目
            Dim tCol As TableRowItem
            If tRow.ContainsKey(act) Then
                tCol = tRow(act)
            Else
                tCol = New TableRowItem With {
                .act = act,
                .root = root
            }
                tRow(act) = tCol
            End If

            'Order・Tankカウント
            If tCol.orderNo.ContainsKey(orderNo) Then
                tCol.orderNo(orderNo) += 1
            Else
                tCol.orderNo(orderNo) = 1
            End If

        Next

        'サマリー設定
        Dim summarySet = {
        {"EETYD", "EETYC,EETYD,LESD"},
        {"TKAL", "TKAL"},
        {"DOUT", "DOUT"},
        {"LOAD", "LOAD"},
        {"CYIN", "CYIN"},
        {"SHIP", "SHIP"},
        {"TRAV", "TRAV,TRAV1,TRAV2"},
        {"TRSH", "TRSH,TRSH1,TRSH2"},
        {"ARVD", "ARVD,DPIN,DLRY"},
        {"ETYD", "ETYC,ETYD"},
        {"ETKAL", "ETKAL"},
        {"EDOUT", "EDOUT"},
        {"ECYIN", "ECYIN"},
        {"ESHIP", "ESHIP"},
        {"ETRAV", "ETRAV,ETRAV1,ETRAV2"},
        {"ETRSH", "ETRSH,ETRSH1,ETRSH2"},
        {"EARVD", "EARVD"},
        {"STOK", "STOK"}
        }

        'サマリデータベーステーブル作成
        Dim retDt = CreateListDataTable()
        Dim lineCnt As Integer = 0
        For Each tRow In dicTable
            lineCnt += 1
            Dim newRow = retDt.NewRow
            newRow("LINECNT") = lineCnt
            newRow("OPERATION") = ""
            newRow("TIMSTP") = 0
            newRow("SELECT") = "1"
            newRow("HIDDEN") = "0"

            newRow("AREANAME") = tRow.Key
            newRow("BASEAREA") = ""

            Dim totalTank = 0
            For i = 0 To CInt(summarySet.Length / summarySet.Rank) - 1
                Dim orderCnt = 0
                Dim tankCnt = 0
                For Each tmp In Split(summarySet(i, 1), ",")
                    If tRow.Value.ContainsKey(tmp) Then
                        orderCnt += tRow.Value(tmp).orderNo.Count
                        tankCnt += tRow.Value(tmp).orderNo.Sum(Function(d) d.Value)
                    End If
                Next
                Dim setup = summarySet(i, 0)
                'ETYDはタンク本数のみ
                If setup = "ETYD" OrElse setup = "EETYD" Then
                    newRow(setup) = "(" & tankCnt & ")"
                Else
                    newRow(setup) = orderCnt & "(" & tankCnt & ")"
                End If
                totalTank += tankCnt

            Next
            newRow("TOTAL") = "(" & totalTank & ")"

            retDt.Rows.Add(newRow)
        Next

        'リースブレーカー登録本数
        lblLeaseTank.Text = leaseTankNum.ToString
        'リースアウト本数
        lblLeaseOut.Text = leaseOutNum.ToString
        'リースタンク合計
        lblLeaseTotal.Text = leaseTotal.ToString
        '帳票用にDataTableにも設定
        For Each tRow As DataRow In retDt.Rows
            tRow("LEASETANK") = leaseTankNum
            tRow("LEASEOUT") = leaseOutNum
            tRow("LEASETOTAL") = leaseTotal
            tRow("OUTPUTDATE") = Now()
        Next

        Return retDt
    End Function

    ''' <summary>
    ''' 一覧 マウスホイール時処理 (一覧スクロール)
    ''' </summary>
    Protected Sub ListScrole()

        Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル

        '表示データ件数取得
        Dim DataCnt As Integer = 0                  '(絞り込み後)有効Data数

        '一覧表示データ復元
        Dim dt As DataTable = CreateListDataTable()

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
        ElseIf Integer.TryParse(Me.hdnListPosition.Value, ListPosition) = False Then
            ListPosition = 1
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
        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnThisMapVariant.Value & "H1"
            .SRCDATA = CreateListDataTable()
            .TBLOBJ = Me.WF_LISTAREA_H1
            .SCROLLTYPE = ""
            .OPERATIONCOLUMNWIDTHOPT = -1
            .NOCOLUMNWIDTHOPT = -1
            .TITLEOPT = False
            .USERSORTOPT = 0
        End With
        COA0013TableObject.COA0013SetTableObject()
        '一覧作成
        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnThisMapVariant.Value & "H"
            .SRCDATA = CreateListDataTable()
            .TBLOBJ = Me.WF_LISTAREA_H
            .SCROLLTYPE = ""
            .OPERATIONCOLUMNWIDTHOPT = -1
            .NOCOLUMNWIDTHOPT = -1
            .TITLEOPT = False
            .USERSORTOPT = 0
        End With
        COA0013TableObject.COA0013SetTableObject()

        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnThisMapVariant.Value
            .SRCDATA = listData
            .TBLOBJ = Me.WF_LISTAREA
            .SCROLLTYPE = ""
            '.LEVENT = "ondblclick"
            '.LFUNC = "ListDbClick"
            .OPERATIONCOLUMNWIDTHOPT = -1
            .NOCOLUMNWIDTHOPT = -1
            .TITLEOPT = True
            .USERSORTOPT = 0
        End With
        COA0013TableObject.COA0013SetTableObject()

        hdnMouseWheel.Value = ""

    End Sub
    ''' <summary>
    ''' 右ボックス設定
    ''' </summary>
    Public Function RightboxInit() As String
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim excelMapId As String = CONST_MAPID

        'RightBOX情報設定
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        retVal = C_MESSAGENO.NORMAL
        '初期化
        'Me.txtRightErrorMessage.Text = ""

        'レポートID情報
        COA0022ProfXls.MAPID = CONST_MAPID
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
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value
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

        Return retVal
    End Function
    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is COM00002MENU Then
            'メニュー画面の場合
        ElseIf TypeOf Page.PreviousPage Is GBT00030ORDERLIST Then
            'オーダー一覧画面の場合
        ElseIf TypeOf Page.PreviousPage Is GBT00030TANKLIST Then
            'タンク一覧画面の場合
        End If
    End Sub

    ''' <summary>
    ''' リストCellクリック時イベント
    ''' </summary>
    Private Sub ListCellClick()
        'データ復元
        Dim dt As DataTable = CreateListDataTable()
        Dim COA0021ListTable As New COA0021ListTable With {
            .FILEdir = hdnXMLsaveFile.Value,
            .TBLDATA = dt
        }
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                    messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        '選択行特定
        Dim rowIdString As String = Me.hdnListDBclick.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(rowIdString, rowId) = True Then
            rowId = rowId - 1
        Else
            Return
        End If

        '選択列特定
        Dim colNm As String = Me.hdnListCellclick.Value
        Dim detailInfo As String = ""
        Select Case colNm
            Case "EETYD"
                'ETYD（MY）
                detailInfo = SelectedMode.ImportEmptyTank
            Case "TKAL",
                "DOUT",
                "LOAD",
                "CYIN"
                'MY側　TKAL～CYIN
                detailInfo = SelectedMode.ImportBeforeTransport
            Case "SHIP",
                "TRAV",
                "TRSH",
                "ARVD"
                '輸送中（輸入）
                detailInfo = SelectedMode.ImportInTransit
            Case "ETYD"
                'ETYD（JP）
                detailInfo = SelectedMode.ExportEmptyTank
            Case "ETKAL",
                "EDOUT",
                "ECYIN"
                'JP側　(E)TKAL～(E)CYIN
                detailInfo = SelectedMode.ExportBeforeTransport
            Case "ESHIP",
                "ETRAV",
                "ETRSH",
                "EARVD"
                '輸送中（回送）
                detailInfo = SelectedMode.ExportInTransit
            Case "STOK"
                detailInfo = SelectedMode.StockTank
            Case "TOTAL"
            Case Else
                Return
        End Select

        Dim vari As String = Me.hdnThisMapVariant.Value
        'ETYD時はタンク一覧に遷移（それ以外はオーダー一覧）
        If detailInfo = SelectedMode.ImportEmptyTank OrElse
            detailInfo = SelectedMode.ExportEmptyTank OrElse
            detailInfo = SelectedMode.StockTank Then
            vari &= "_ETYD"
        End If

        '次画面引継ぎ項目
        Me.hdnSelectedPort.Value = dt.Rows(rowId).Item("AREANAME").ToString
        Me.hdnSelectedMode.Value = detailInfo
        Me.hdnSelectedActy.Value = colNm

        Dim selectedRow As DataRow = dt.Rows(rowId)

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = vari
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
    End Sub

    ''' <summary>
    ''' タンク情報クラス(オーダータンク引当情報)
    ''' </summary>
    <Serializable>
    Public Class GBT00030OrderInfo
        ''' <summary>
        ''' オーダーNo
        ''' </summary>
        ''' <returns></returns>
        Public Property OrderNo As String
        ''' <summary>
        ''' ETD
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ETD As String
        '''' <summary>
        '''' ETA
        '''' </summary>
        '''' <returns></returns>
        Public Property ETA As String
        '''' <summary>
        '''' VSLVOY
        '''' </summary>
        '''' <returns></returns>
        Public Property VSLVOL As String
        ''' <summary>
        ''' TSETD
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property TSETD As String
        '''' <summary>
        '''' TSETA
        '''' </summary>
        '''' <returns></returns>
        Public Property TSETA As String
        '''' <summary>
        '''' TSVSLVOY
        '''' </summary>
        '''' <returns></returns>
        Public Property TSVSLVOY As String
        ''' <summary>
        ''' タンクSEQごとの情報
        ''' </summary>
        ''' <returns></returns>
        Public Property TankInfoList As Dictionary(Of String, GBT00030TankInfo)
        ''' <summary>
        ''' 荷主
        ''' </summary>
        ''' <returns></returns>
        Public Property ShipperCode As String = ""
        ''' <summary>
        ''' 積載品
        ''' </summary>
        ''' <returns></returns>
        Public Property ProductCode As String = ""
        ''' <summary>
        ''' Import OR Export(True:IMPORT（輸入）、False：EXPORT（輸出（回送））)
        ''' </summary>
        ''' <returns></returns>
        Public Property IsImport As Boolean = False
        ''' <summary>
        ''' Ship済か(True:シップ済、False：未シップ)
        ''' </summary>
        ''' <returns></returns>
        Public Property IsShepped As Boolean = False
    End Class
    ''' <summary>
    ''' タンク単位情報(オーダータンク引当情報)
    ''' </summary>
    <Serializable>
    Public Class GBT00030TankInfo
        ''' <summary>
        ''' TANKSEQ
        ''' </summary>
        ''' <returns></returns>
        Public Property TankSeq As String
        ''' <summary>
        ''' タンク番号
        ''' </summary>
        ''' <returns></returns>
        Public Property TankNo As String
        ''' <summary>
        ''' 次回検査内容
        ''' </summary>
        ''' <returns></returns>
        Public Property NextInspectType As String
        ''' <summary>
        ''' 次回検査日
        ''' </summary>
        ''' <returns></returns>
        Public Property NextInspectDate As String
        ''' <summary>
        ''' シップ済か(True:シップ済,False:シップ未)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>ACTYが発のACTUALDATEが初期値以外でTrue</remarks>
        Public Property IsShipped As Boolean = False
        ''' <summary>
        ''' 前回輸送オーダー番号
        ''' </summary>
        ''' <returns></returns>
        Public Property LastImportOrderNo As String
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="tankSeq"></param>
        ''' <param name="tankNo"></param>
        Public Sub New(tankSeq As String, tankNo As String, nextInspectType As String, nextInspectDate As String, isShipped As Boolean, lastImportOrderNo As String)
            Me.TankSeq = tankSeq
            Me.TankNo = tankNo
            Me.NextInspectType = nextInspectType
            Me.NextInspectDate = nextInspectDate
            Me.IsShipped = isShipped
            Me.LastImportOrderNo = lastImportOrderNo
        End Sub
    End Class
End Class