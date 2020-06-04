Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' タンク動静検索結果画面クラス
''' </summary>
Public Class GBT00030RESULT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00030R" '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 25              'マウススクロール時の増分
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
                Dim retDt = Me.GetListDataTable()
                'サマリー一覧表データ取得
                Dim sumDt = SummaryDataTable(retDt)

                Using dt As DataTable = sumDt
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
                    Dim listVari As String = Me.hdnReportVariant.Value
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = listVari & "H"
                        .SRCDATA = CreateDataTable()
                        .TBLOBJ = WF_LISTAREA_H
                        .SCROLLTYPE = "0"
                        '.LEVENT = ""
                        '.LFUNC = ""
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .NOCOLUMNWIDTHOPT = -1
                        .TITLEOPT = True
                        .USERSORTOPT = 0
                    End With
                    COA0013TableObject.COA0013SetTableObject()

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = listVari
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
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

                '一覧ヘッダー部ALLボタン追加
                AddHeaderAllButton()

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
                If Me.hdnListDBclick.Value <> "" AndAlso Me.hdnListCellClick.Value = "" Then
                    ListRowDbClick()
                    Me.hdnListDBclick.Value = ""
                    'Return '単票ページにリダイレクトするため念のため処理は終わらせる
                End If
                '**********************
                ' 一覧表のCellクリック判定
                '**********************
                If Me.hdnListDBclick.Value <> "" AndAlso Me.hdnListCellClick.Value <> "" Then
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
                'タンク番号
                Case vLeftTankNo.ID
                    Dim dt As DataTable = GetTankNo()
                    With Me.lbTankNo
                        .DataSource = dt
                        .DataTextField = "TANKNO"
                        .DataValueField = "TANKNO"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtTankNo.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With

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
            'WF_TITLETEXT.Text = COA0011ReturnUrl.NAMES
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
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    Public Sub btnExtract_Click()
        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
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

        'フィルタでの絞り込みを利用するか確認
        Dim isFillterOff As Boolean = True
        If Me.txtTankNo.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtTankNo.Text.Trim = "" OrElse Convert.ToString(dr("TANKNO")).Equals(Me.txtTankNo.Text.Trim))) Then
                    dr.Item("HIDDEN") = 1
                End If

            End If
        Next
        '画面先頭を表示
        hdnListPosition.Value = "1"

        '一覧表示データ保存
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
        Else
            'メッセージ表示
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALEXTRUCT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        End If

        'カーソル設定
        Me.txtTankNo.Focus()

    End Sub
    ''' <summary>
    ''' Excelダウンロードボタン押下時処理
    ''' </summary>
    Public Sub btnExcelDownload_Click()
        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        'Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
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

        '帳票出力
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            Dim reportId As String = "Default"
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
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        Dim retDt As New DataTable
        Dim sb As New StringBuilder(2048)

        'ソート順取得
        COA0020ProfViewSort.MAPID = mapId
        COA0020ProfViewSort.VARI = Me.hdnReportVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        sb.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sb.AppendLine("      ,'' AS OPERATION")
        sb.AppendLine("      ,0 AS TIMSTP")
        sb.AppendLine("      ,'1' AS 'SELECT' ")
        sb.AppendLine("      ,'0' AS HIDDEN ")
        sb.AppendLine("      ,TBL.*")
        sb.AppendLine("  FROM (")

        sb.AppendLine("select ")
        sb.AppendLine("  AREANAME ")
        sb.AppendLine("  , BASEAREA ")
        sb.AppendLine("  , isnull([AETYC], 0) as AETYC ")
        sb.AppendLine("  , isnull([AETYD], 0) as AETYD ")
        sb.AppendLine("  , isnull([BTKAL], 0) as BTKAL ")
        sb.AppendLine("  , isnull([BDOUT], 0) as BDOUT ")
        sb.AppendLine("  , isnull([BLOAD], 0) as BLOAD ")
        sb.AppendLine("  , isnull([BCYIN], 0) as BCYIN ")
        sb.AppendLine("  , isnull([BSHIP], 0) as BSHIP ")
        sb.AppendLine("  , isnull([BTRAV1], 0) as BTRAV1 ")
        sb.AppendLine("  , isnull([BTRSH1], 0) as BTRSH1 ")
        sb.AppendLine("  , isnull([BTRAV2], 0) as BTRAV2 ")
        sb.AppendLine("  , isnull([BTRSH2], 0) as BTRSH2 ")
        sb.AppendLine("  , isnull([BARVD], 0) as BARVD ")
        sb.AppendLine("  , isnull([BDPIN], 0) as BDPIN ")
        sb.AppendLine("  , isnull([BDLRY], 0) as BDLRY ")
        sb.AppendLine("  , isnull([BETYD], 0) as BETYD ")
        sb.AppendLine("  , isnull([ATKAL], 0) as ATKAL ")
        sb.AppendLine("  , isnull([ACYIN], 0) as ACYIN ")
        sb.AppendLine("  , isnull([ASHIP], 0) as ASHIP ")
        sb.AppendLine("  , isnull([ATRAV1], 0) as ATRAV1 ")
        sb.AppendLine("  , isnull([ATRSH1], 0) as ATRSH1 ")
        sb.AppendLine("  , isnull([ATRAV2], 0) as ATRAV2 ")
        sb.AppendLine("  , isnull([ATRSH2], 0) as ATRSH2 ")
        sb.AppendLine("  , isnull([AARVD], 0) as AARVD ")
        sb.AppendLine("  , isnull([AETKAL], 0) as AETKAL ")
        sb.AppendLine("  , isnull([AESHIP], 0) as AESHIP ")
        sb.AppendLine("from ")
        sb.AppendLine("  ( ")
        sb.AppendLine("    select ")
        sb.AppendLine("      pm.AREANAME ")
        sb.AppendLine("      , work.BASEAREA ")
        sb.AppendLine("      , work.ROOT + work.ACTIONID as 'col' ")
        sb.AppendLine("      , count(*) as 'cnt' ")
        sb.AppendLine("    from ")
        sb.AppendLine("      ( ")
        sb.AppendLine("        select ")
        sb.AppendLine("          RANK() OVER ( PARTITION BY ov.TANKNO ORDER BY ov.TANKNO, ov.ACTUALDATE desc, ov.DISPSEQ desc ) AS ORDERSORT ")
        sb.AppendLine("          , case when ob.DISCHARGEPORT1 = 'JPSDJ' then ob.LOADPORT1 else ob.DISCHARGEPORT1 end as BASEAREA ")
        sb.AppendLine("          , case when ob.DISCHARGEPORT1 = 'JPSDJ' then 'B' else 'A' end as ROOT ")
        sb.AppendLine("          , ob.LOADPORT1 ")
        sb.AppendLine("          , ob.DISCHARGEPORT1 ")
        sb.AppendLine("          , ov.ACTIONID ")
        sb.AppendLine("          , ov.DISPSEQ ")
        sb.AppendLine("          , ov.TANKNO ")
        sb.AppendLine("          , ov.SCHEDELDATE ")
        sb.AppendLine("          , ov.ACTUALDATE ")
        sb.AppendLine("          , ov.CONTRACTORFIX ")
        sb.AppendLine("          , ov.ORDERNO ")
        sb.AppendLine("        from ")
        sb.AppendLine("          GBT0005_ODR_VALUE as ov with (nolock) ")
        sb.AppendLine("          inner join GBT0004_ODR_BASE ob on ob.ORDERNO = ov.ORDERNO and ob.DELFLG <> 'Y' ")
        sb.AppendLine("        where ")
        sb.AppendLine("          ov.DELFLG <> 'Y' ")
        sb.AppendLine("          and ov.ACTIONID <> '' ")
        sb.AppendLine("          and ov.TANKNO <> '' ")
        sb.AppendLine("          and ov.ORDERNO not in ( ")
        sb.AppendLine("            select ")
        sb.AppendLine("              ob.ORDERNO ")
        sb.AppendLine("            from ")
        sb.AppendLine("              GBT0004_ODR_BASE ob ")
        sb.AppendLine("              inner join GBT0001_BR_INFO bi ")
        sb.AppendLine("                on  bi.BRID = ob.BRID ")
        sb.AppendLine("                and bi.DELFLG <> 'Y' ")
        sb.AppendLine("                and bi.TYPE = 'INFO' ")
        sb.AppendLine("                and bi.USETYPE in ('P00005', 'P00006') ")
        sb.AppendLine("            where ")
        sb.AppendLine("              ob.DELFLG <> 'Y' ")
        sb.AppendLine("          ) ")
        sb.AppendLine("      ) work ")
        sb.AppendLine("      left outer join GBM0002_PORT pm ")
        sb.AppendLine("        on pm.PORTCODE = work.BASEAREA and pm.DELFLG <> 'Y' ")
        sb.AppendLine("    where ")
        sb.AppendLine("      work.ORDERSORT = '1' ")
        sb.AppendLine("    group by ")
        sb.AppendLine("      pm.AREANAME ")
        sb.AppendLine("      , work.BASEAREA ")
        sb.AppendLine("      , work.ROOT ")
        sb.AppendLine("      , work.ACTIONID ")
        sb.AppendLine("  ) work2 pivot( ")
        sb.AppendLine("    sum([cnt]) for [col] in ( ")
        sb.AppendLine("        [AETYC] ")
        sb.AppendLine("      , [AETYD] ")
        sb.AppendLine("      , [BTKAL] ")
        sb.AppendLine("      , [BDOUT] ")
        sb.AppendLine("      , [BLOAD] ")
        sb.AppendLine("      , [BCYIN] ")
        sb.AppendLine("      , [BSHIP] ")
        sb.AppendLine("      , [BTRAV1] ")
        sb.AppendLine("      , [BTRSH1] ")
        sb.AppendLine("      , [BTRAV2] ")
        sb.AppendLine("      , [BTRSH2] ")
        sb.AppendLine("      , [BARVD] ")
        sb.AppendLine("      , [BDPIN] ")
        sb.AppendLine("      , [BDLRY] ")
        sb.AppendLine("      , [BETYD] ")
        sb.AppendLine("      , [ATKAL] ")
        sb.AppendLine("      , [ACYIN] ")
        sb.AppendLine("      , [ASHIP] ")
        sb.AppendLine("      , [ATRAV1] ")
        sb.AppendLine("      , [ATRSH1] ")
        sb.AppendLine("      , [ATRAV2] ")
        sb.AppendLine("      , [ATRSH2] ")
        sb.AppendLine("      , [AARVD] ")
        sb.AppendLine("      , [AETKAL] ")
        sb.AppendLine("      , [AESHIP] ")
        sb.AppendLine("    ) ")
        sb.AppendLine("  ) as PV ")
        sb.AppendLine(") as TBL")

        sb.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sb.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                If Me.hdnTankNo.Value <> "" Then
                    .Add("@TANKNO", SqlDbType.NVarChar).Value = Me.hdnTankNo.Value
                End If

            End With
            'SQLパラメータ(動的変化あり)
            Dim paramValidityfrom As SqlParameter = Nothing
            Dim paramValidityto As SqlParameter = Nothing
            Dim paramShipper As SqlParameter = Nothing
            Dim paramConsignee As SqlParameter = Nothing
            Dim paramPort As SqlParameter = Nothing
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
                'ビューごとの処理はケースを追加で実現
                Case Me.vLeftTankNo.ID 'アクティブなビューがタンク番号
                    'タンク番号選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbTankNo.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbTankNo.SelectedItem.Value
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            txtobj.Focus()
                        End If
                    End If
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
        AddLangSetting(dicDisplayText, Me.btnExtract, "絞り込み", "Search")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblTankNo, "タンク番号", "Tank No")

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
        retDt.Columns.Add("AREANAME", GetType(String))
        retDt.Columns.Add("BASEAREA", GetType(String))

        retDt.Columns.Add("AETYD", GetType(String))
        retDt.Columns.Add("BORDE", GetType(String))
        retDt.Columns.Add("BTKAL", GetType(String))
        retDt.Columns.Add("BDOUT", GetType(String))
        retDt.Columns.Add("BCYIN", GetType(String))
        retDt.Columns.Add("BSHIP", GetType(String))
        retDt.Columns.Add("BTRAV1", GetType(String))
        retDt.Columns.Add("BTRSH1", GetType(String))
        retDt.Columns.Add("BTRAV2", GetType(String))
        retDt.Columns.Add("BTRSH2", GetType(String))
        retDt.Columns.Add("BARVD", GetType(String))
        retDt.Columns.Add("BDPIN", GetType(String))
        retDt.Columns.Add("BDLRY", GetType(String))

        retDt.Columns.Add("BETYD", GetType(String))
        retDt.Columns.Add("ATKAL", GetType(String))
        retDt.Columns.Add("ACYIN", GetType(String))
        retDt.Columns.Add("ASHIP", GetType(String))
        retDt.Columns.Add("ATRAV2", GetType(String))
        retDt.Columns.Add("ATRSH2", GetType(String))
        retDt.Columns.Add("ATRAV1", GetType(String))
        retDt.Columns.Add("ATRSH1", GetType(String))
        retDt.Columns.Add("AARVD", GetType(String))
        Return retDt
    End Function
    ''' <summary>
    ''' サマリー一覧編集
    ''' </summary>
    ''' <returns></returns>
    Private Function SummaryDataTable(ByRef dt As DataTable) As DataTable
        'サマリデータベーステーブル作成
        Dim retDt = CreateDataTable()

        For Each row In dt.AsEnumerable
            Dim newRow = retDt.NewRow
            newRow("LINECNT") = row("LINECNT")
            newRow("OPERATION") = row("OPERATION")
            newRow("TIMSTP") = row("TIMSTP")
            newRow("SELECT") = row("SELECT")
            newRow("HIDDEN") = row("HIDDEN")

            newRow("AREANAME") = row("AREANAME")
            newRow("BASEAREA") = row("BASEAREA")

            newRow("AETYD") = (Integer.Parse(row("AETYC").ToString) + Integer.Parse(row("AETYD").ToString)).ToString
            newRow("BORDE") = "0"
            newRow("BTKAL") = row("BTKAL")
            newRow("BDOUT") = (Integer.Parse(row("BDOUT").ToString) + Integer.Parse(row("BLOAD").ToString)).ToString
            newRow("BCYIN") = row("BCYIN")
            newRow("BSHIP") = row("BSHIP")
            newRow("BTRAV1") = row("BTRAV1")
            newRow("BTRSH1") = row("BTRSH1")
            newRow("BTRAV2") = row("BTRAV2")
            newRow("BTRSH2") = row("BTRSH2")
            newRow("BARVD") = row("BARVD")
            newRow("BDPIN") = row("BDPIN")
            newRow("BDLRY") = row("BDLRY")

            newRow("BETYD") = row("BETYD")
            newRow("ATKAL") = (Integer.Parse(row("ATKAL").ToString) + Integer.Parse(row("AETKAL").ToString)).ToString
            newRow("ACYIN") = row("ACYIN")
            newRow("ASHIP") = (Integer.Parse(row("ASHIP").ToString) + Integer.Parse(row("AESHIP").ToString)).ToString
            newRow("ATRAV2") = row("ATRAV2")
            newRow("ATRSH2") = row("ATRSH2")
            newRow("ATRAV1") = row("ATRAV1")
            newRow("ATRSH1") = row("ATRSH1")
            newRow("AARVD") = row("AARVD")
            retDt.Rows.Add(newRow)
        Next

        Return retDt
    End Function

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
        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnReportVariant.Value & "H"
            .SRCDATA = CreateDataTable()
            .TBLOBJ = Me.WF_LISTAREA_H
            .SCROLLTYPE = "0"
            '.LEVENT = ""
            '.LFUNC = ""
            .OPERATIONCOLUMNWIDTHOPT = -1
            .NOCOLUMNWIDTHOPT = -1
            .TITLEOPT = True
            .USERSORTOPT = 0
        End With
        COA0013TableObject.COA0013SetTableObject()

        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnReportVariant.Value
            .SRCDATA = listData
            .TBLOBJ = Me.WF_LISTAREA
            .SCROLLTYPE = "2"
            '.LEVENT = "ondblclick"
            '.LFUNC = "ListDbClick"
            .OPERATIONCOLUMNWIDTHOPT = -1
            .NOCOLUMNWIDTHOPT = -1
            .TITLEOPT = True
            .USERSORTOPT = 0
        End With
        COA0013TableObject.COA0013SetTableObject()

        '一覧ヘッダー部ALLボタン追加
        AddHeaderAllButton()

        hdnMouseWheel.Value = ""

    End Sub
    ''' <summary>
    ''' 右ボックス設定
    ''' </summary>
    Public Function RightboxInit() As String
        Dim COA0018ViewList As New BASEDLL.COA0018ViewList          '変数情報取
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget            '変数情報取
        Dim retVal As String = ""
        'RightBOX情報設定
        '画面レイアウト情報
        COA0018ViewList.MAPID = CONST_MAPID
        COA0018ViewList.FORWARDMATCHVARIANT = Me.hdnThisMapVariant.Value
        COA0018ViewList.VIEW = lbRightList
        COA0018ViewList.COA0018getViewList()
        If COA0018ViewList.ERR = C_MESSAGENO.NORMAL Then
            Try
                For i As Integer = 0 To DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items.Count - 1
                    lbRightList.Items.Add(New ListItem(DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items(i).Text, DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items(i).Value))
                Next
            Catch ex As Exception
            End Try
        Else
            CommonFunctions.ShowMessage(COA0018ViewList.ERR, Me.lblFooterMessage)
            retVal = COA0018ViewList.ERR
            Return retVal
        End If

        'ビューID変数検索
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value  '"Default" 'Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0016VARIget.FIELD = "VIEWID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            retVal = COA0016VARIget.ERR
            Return retVal
        End If

        'ListBox選択
        lbRightList.SelectedIndex = 0     '選択無しの場合、デフォルト
        For i As Integer = 0 To lbRightList.Items.Count - 1

            If lbRightList.Items(i).Value <> COA0018ViewList.FORWARDMATCHVARIANT Then
                lbRightList.Items(i).Text = lbRightList.Items(i).Text.Replace(":" & COA0016VARIget.VARI, ":")
            End If

            If lbRightList.Items(i).Value = COA0016VARIget.VALUE Then
                lbRightList.SelectedIndex = i
            End If
        Next
        retVal = C_MESSAGENO.NORMAL
        Return retVal
    End Function
    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00030SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00030SELECT = DirectCast(Page.PreviousPage, GBT00030SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtTankNo", Me.hdnTankNo},
                                                                        {"lbRightList", Me.hdnReportVariant}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is TextBox Then
                        Dim tmpText As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpText.Text
                    ElseIf TypeOf tmpCont Is RadioButtonList Then
                        Dim tmpRbl As RadioButtonList = DirectCast(tmpCont, RadioButtonList)
                        item.Value.Value = tmpRbl.SelectedValue
                    ElseIf TypeOf tmpCont Is ListBox Then
                        Dim tmplist As ListBox = DirectCast(tmpCont, ListBox)
                        item.Value.Value = tmplist.SelectedValue
                    End If

                End If
            Next

            'ElseIf TypeOf Page.PreviousPage Is GBT00030DETAILS Then
            '    '検索画面の場合
            '    Dim prevObj As GBT00030DETAILS = DirectCast(Page.PreviousPage, GBT00030DETAILS)
            '    Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnTankNo", Me.hdnTankNo},
            '                                                                    {"hdnReportVariant", Me.hdnReportVariant}}

            '    For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
            '        Dim tmpCont As Control = prevObj.FindControl(item.Key)

            '        If tmpCont IsNot Nothing Then
            '            If TypeOf tmpCont Is TextBox Then
            '                Dim tmpText As TextBox = DirectCast(tmpCont, TextBox)
            '                item.Value.Value = tmpText.Text
            '            ElseIf TypeOf tmpCont Is RadioButtonList Then
            '                Dim tmpRbl As RadioButtonList = DirectCast(tmpCont, RadioButtonList)
            '                item.Value.Value = tmpRbl.SelectedValue
            '            ElseIf TypeOf tmpCont Is ListBox Then
            '                Dim tmplist As ListBox = DirectCast(tmpCont, ListBox)
            '                item.Value.Value = tmplist.SelectedValue
            '            ElseIf TypeOf tmpCont Is HiddenField Then
            '                Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
            '                item.Value.Value = tmpHdn.Value
            '            End If

            '        End If
            '    Next

        End If
    End Sub

    ''' <summary>
    ''' タンク番号一覧を取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetTankNo() As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT rtrim(TANKNO) as TANKNO")
        sqlStat.AppendLine("  FROM GBM0006_TANK ")
        sqlStat.AppendLine(" WHERE  COMPCODE     = @COMPCODE ")
        sqlStat.AppendLine("   AND  STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  DELFLG      <> @DELFLG ")
        sqlStat.AppendLine(" ORDER BY TANKNO ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
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

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If

        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim brId As String = Convert.ToString(selectedRow.Item("BRID"))
        Me.hdnSelectedBrId.Value = brId

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = "GB_RepairHistory"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        Session("MAPmapid") = CONST_MAPID
        Session("MAPvariant") = "GB_RepairHistory"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub

    ''' <summary>
    ''' リストCellクリック時イベント
    ''' </summary>
    Private Sub ListCellClick()
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
        Dim detailInfo As Integer = 0
        Select Case colNm
            Case "AETYD"
                detailInfo = 1
            Case "BORDR"
            Case "BTKAL"
                detailInfo = 3
            Case "BDOUT",
                 "BCYIN",
                 "BSHIP",
                 "BTRAV1",
                 "BTRSH1",
                 "BTRAV2",
                 "BTRSH2",
                 "BARVD",
                 "BDPIN"
                detailInfo = 4
            Case "BDLRY",
                 "BETYD"
                detailInfo = 5
            Case "ATKAL",
                 "ACYIN",
                 "ASHIP",
                 "ATRAV2",
                 "ATRSH2",
                 "ATRAV1",
                 "ATRSH1",
                 "AARVD",
                 "AETKAL",
                 "AESHIP"
                detailInfo = 7
            Case Else
                Return
        End Select

        'データ復元
        Dim dt As DataTable = CreateDataTable()
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


        Dim selectedRow As DataRow = dt.Rows(rowId)

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = "Default"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        Session("MAPmapid") = CONST_MAPID
        Session("MAPvariant") = "Default"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub

    ''' <summary>
    ''' ALLボタン押下時処理
    ''' </summary>
    Public Sub btnAllAction_Click()
        'データ復元
        Dim dt As DataTable = CreateDataTable()
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

        Dim colNm As String = Me.hdnListCellclick.Value

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
        Session("MAPmapid") = CONST_MAPID
        Session("MAPvariant") = "GB_TankStatusList"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

    End Sub

    ''' <summary>
    ''' 一覧ヘッダー部ALLボタン追加
    ''' </summary>
    Private Sub AddHeaderAllButton()
        Dim rTblCtrl As Control = WF_LISTAREA.FindControl("WF_LISTAREA_HR").Controls(0)
        If IsNothing(rTblCtrl) Then Exit Sub
        Dim tblCtrl As Control = rTblCtrl.Controls(0)
        If IsNothing(tblCtrl) Then Exit Sub

        '対象列のみALLボタン追加
        For Each th As Control In tblCtrl.Controls
            Dim rTableCell As TableHeaderCell = CType(th, TableHeaderCell)
            '列名取得
            Dim cellName As String = rTableCell.Attributes("cellfiedlname")
            Select Case cellName
                Case "BTRAV1", "BTRSH1", "BTRAV2", "BTRSH2", "BARVD"
                    Dim lblAdd As Label = New Label With {
                        .Text = rTableCell.Text,
                        .ID = "lbl" & cellName
                    }
                    rTableCell.Controls.Add(lblAdd)

                    Dim btnAdd As HtmlButton = New HtmlButton With {
                        .ViewStateMode = UI.ViewStateMode.Disabled,
                        .InnerText = "ALL",
                        .ID = "btnAll" & cellName
                    }
                    btnAdd.Attributes.Add("cellfiedlname", cellName)
                    rTableCell.Controls.Add(btnAdd)
                Case Else
                    Continue For
            End Select
        Next

    End Sub

End Class