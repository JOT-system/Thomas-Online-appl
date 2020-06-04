Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' 帳票出力用検索結果画面クラス
''' </summary>
Public Class GBT00017RESULT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00017"    '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00004"   '次画面一覧のMAPID
    Private Const CONST_BLMAPID As String = "GBT00014"  'BL画面のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
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
                COA0031ProfMap.MAPIDP = CONST_MAPID & "R"
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
                Using dt As DataTable = Me.GetOrderListDataTable()
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

                    '■■■ 一覧表示データ編集（性能対策） ■■■
                    Dim listVari As String = Me.hdnReportVariant.Value
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = listVari
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
                        .LEVENT = "ondblclick"
                        .LFUNC = "ListDbClick"
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .NOCOLUMNWIDTHOPT = 50
                        .TITLEOPT = True
                        .USERSORTOPT = 1
                    End With
                    COA0013TableObject.COA0013SetTableObject()
                    For Each dr As DataRow In listData.Rows
                        Dim btnEdit1 = WF_LISTAREA.FindControl("btn" & Me.WF_LISTAREA.ID & "EDIT1" + dr.Item("LINECNT").ToString)
                        If dr.Item("SHOWEDIT1").ToString = "1" Then
                            btnEdit1.Visible = True
                        Else
                            btnEdit1.Visible = False
                        End If

                        Dim btnEdit2 = WF_LISTAREA.FindControl("btn" & Me.WF_LISTAREA.ID & "EDIT2" + dr.Item("LINECNT").ToString)
                        If dr.Item("SHOWEDIT2").ToString = "1" Then
                            btnEdit2.Visible = True
                        Else
                            btnEdit2.Visible = False
                        End If
                    Next
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

                '**********************
                ' ボタンクリック判定
                '**********************
                'hdnButtonClickに文字列が設定されていたら実行する
                If Me.hdnButtonClick IsNot Nothing AndAlso Me.hdnButtonClick.Value <> "" Then
                    'ボタンID + "_Click"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)

                    Dim btnEventName As String = ""
                    btnEventName = Me.hdnButtonClick.Value & "_Click"

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
                If Me.hdnListDBclick.Value <> "" Then
                    ListRowDbClick()
                    Me.hdnListDBclick.Value = ""
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
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID & "R"
        Me.hdnThisMapVariant.Value = "GB_PRINT"
        COA0011ReturnUrl.VARI = Me.hdnThisMapVariant.Value
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
            'WF_TITLETEXT.Text = COA0011ReturnUrl.NAMES
        Else
            CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '次画面の変数セット
        HttpContext.Current.Session("MAPvariant") = "GB_BL"
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
        If Me.txtShipper.Text.Trim <> "" OrElse Me.txtConsignee.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtShipper.Text.Trim = "" OrElse Convert.ToString(dr("SHIPPER")).Contains(Me.txtShipper.Text.Trim)) _
                  AndAlso (Me.txtConsignee.Text.Trim = "" OrElse Convert.ToString(dr("CONSIGNEE")).Contains(Me.txtConsignee.Text.Trim))
                  ) Then
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
        Me.txtShipper.Focus()

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
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If
        '帳票出力
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            Dim reportId As String = "FullColumnList" 'Me.hdnReportVariant.Value
            Dim reportMapId As String = CONST_MAPID
            COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
            COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
            COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
            COA0027ReportTable.TBLDATA = dt                                    'PARAM04:データ参照tabledata
            COA0027ReportTable.COA0027ReportTable()

            If COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
            Else
                CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage)
                Return
            End If

            '別画面でExcelを表示
            hdnPrintURL.Value = COA0027ReportTable.URL
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint()", True)

        End With
    End Sub
    ''' <summary>
    ''' オーダー一覧より値取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>要UNION オーダー</remarks>
    Private Function GetOrderListDataTable() As DataTable
        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        Dim retDt As New DataTable
        Dim sqlStat As New StringBuilder



        '文言フィールド（開発中のためいったん固定
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If
        Dim textProductTblField As String = "PRODUCTNAME"
        'If COA0019Session.LANGDISP <> C_LANG.JA Then
        '    textProductTblField = "NAMES"
        'End If
        Dim textTraderTblField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textTraderTblField = "NAMES"
        End If
        'ソート順取得
        COA0020ProfViewSort.MAPID = mapId
        COA0020ProfViewSort.VARI = Me.hdnReportVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()
        '個別入力条件の設定
        'Dim sqlEtdEtaBreakerCondition As New StringBuilder
        Dim sqlEtdEtaOrderCondition As New StringBuilder
        Dim etdDatefield As String = ""
        Dim etaDatefield As String = ""
        Dim etdActy As String = "('SHIP','RPEC','RPED','RPHC','RPHD')"
        Dim etaActy As String = "('ARVD','DCEC','DCED','ETYC')"

        '予定パターン
        If Me.hdnSearchType.Value = "01SCHE" Then
            etdDatefield = "(SELECT TOP 1 (CASE WHEN ODVALETD.SCHEDELDATE = '1900/01/01'" &
                           "              THEN ODVALETD.SCHEDELDATEBR" &
                           "              ELSE ODVALETD.SCHEDELDATE END) AS ETD{0} " &
                           "   FROM GBT0005_ODR_VALUE ODVALETD " &
                           "  WHERE ODVALETD.ORDERNO = OBS.ORDERNO " &
                           "    AND ODVALETD.ACTIONID  in " & etdActy & " " &
                           "    AND ODVALETD.DTLPOLPOD  = 'POL{0}' " &
                           "    AND ODVALETD.DELFLG   <> @DELFLG" &
                           "  ORDER BY ODVALETD.DISPSEQ DESC)"
            etaDatefield = "(SELECT TOP 1 (CASE WHEN ODVALETD.SCHEDELDATE = '1900/01/01'" &
                           "              THEN ODVALETD.SCHEDELDATEBR" &
                           "              ELSE ODVALETD.SCHEDELDATE END) AS ETA{0} " &
                           "   FROM GBT0005_ODR_VALUE ODVALETD " &
                           "  WHERE ODVALETD.ORDERNO = OBS.ORDERNO " &
                           "    AND ODVALETD.ACTIONID  in " & etaActy & "  " &
                           "    AND ODVALETD.DTLPOLPOD  = 'POD{0}' " &
                           "    AND ODVALETD.DELFLG   <> @DELFLG" &
                           "  ORDER BY ODVALETD.DISPSEQ)"
            '実績パターン
        ElseIf Me.hdnSearchType.Value = "02FIX" Then
        etdDatefield = "(SELECT TOP 1 ODVALETD.ACTUALDATE AS ETD{0} " &
                           "   FROM GBT0005_ODR_VALUE ODVALETD " &
                           "  WHERE ODVALETD.ORDERNO = OBS.ORDERNO " &
                           "    AND ODVALETD.ACTIONID  in " & etdActy & " " &
                           "    AND ODVALETD.DTLPOLPOD  = 'POL{0}' " &
                           "    AND ODVALETD.DELFLG   <> @DELFLG" &
                           "  ORDER BY ODVALETD.DISPSEQ DESC)"
            etaDatefield = "(SELECT TOP 1 ODVALETD.ACTUALDATE AS ETA{0} " &
                           "   FROM GBT0005_ODR_VALUE ODVALETD " &
                           "  WHERE ODVALETD.ORDERNO = OBS.ORDERNO " &
                           "    AND ODVALETD.ACTIONID  in " & etaActy & " " &
                           "    AND ODVALETD.DTLPOLPOD  = 'POD{0}' " &
                           "    AND ODVALETD.DELFLG   <> @DELFLG" &
                           "  ORDER BY ODVALETD.DISPSEQ)"
        End If

        If Me.hdnETDStYMD.Value <> "" OrElse Me.hdnETAStYMD.Value <> "" Then
            sqlEtdEtaOrderCondition.AppendLine(" AND ")
            'TODO冗長なので考える
            '予定パターン
            If Me.hdnSearchType.Value = "01SCHE" Then

                    'オーダー明細
                    If Me.hdnETDStYMD.Value <> "" And Me.hdnETAStYMD.Value <> "" Then
                    With sqlEtdEtaOrderCondition
                        .AppendLine("(    (     OBS.ETD1 BETWEEN @ETDST And @ETDEND") 'オーダー基本のETA ETDが収まっていること
                        .AppendLine("      And  OBS.ETA1 BETWEEN @ETAST And @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(" Or  (     OBS.ETD2 BETWEEN @ETDST And @ETDEND")
                        .AppendLine("      And  OBS.ETA2 BETWEEN @ETAST And @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(" Or  (     EXISTS(Select 1 ") 'オーダー明細SHIPがETDの範囲に存在するか
                        .AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETD ")
                        .AppendLine("                  WHERE ODVALETD.ORDERNO   = OBS.ORDERNO ")
                        .AppendLine("                    And ODVALETD.ACTIONID in " & etdActy & " ")
                        .AppendLine("                    AND ODVALETD.DELFLG   <> @DELFLG ")
                        .AppendLine("                    AND CASE WHEN ODVALETD.SCHEDELDATE = '1900/01/01'")
                        .AppendLine("                               THEN ODVALETD.SCHEDELDATEBR")
                        .AppendLine("                             ELSE ODVALETD.SCHEDELDATE END BETWEEN @ETDST AND @ETDEND")
                        .AppendLine("                 )") 'オーダー明細SHIP END
                        .AppendLine("      AND  EXISTS(SELECT 1 ") 'オーダー明細ARVDがETAの範囲に存在するか
                        .AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETA ")
                        .AppendLine("                  WHERE ODVALETA.ORDERNO   = OBS.ORDERNO ")
                        .AppendLine("                    AND ODVALETA.ACTIONID in " & etaActy & " ")
                        .AppendLine("                    AND ODVALETA.DELFLG   <> @DELFLG ")
                        .AppendLine("                    AND CASE WHEN ODVALETA.SCHEDELDATE = '1900/01/01'")
                        .AppendLine("                               THEN ODVALETA.SCHEDELDATEBR")
                        .AppendLine("                             ELSE ODVALETA.SCHEDELDATE END BETWEEN @ETAST AND @ETAEND")
                        .AppendLine("                 )")
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With
                End If
                If Me.hdnETDStYMD.Value <> "" And Me.hdnETAStYMD.Value = "" Then
                    With sqlEtdEtaOrderCondition
                        .AppendLine("(    (     OBS.ETD1 BETWEEN @ETDST AND @ETDEND") 'オーダー基本のETA ETDが収まっていること
                        .AppendLine("     )")
                        .AppendLine(" OR  (     OBS.ETD2 BETWEEN @ETDST AND @ETDEND")
                        .AppendLine("     )")
                        .AppendLine(" OR  (     EXISTS(SELECT 1 ") 'オーダー明細SHIPがETDの範囲に存在するか
                        .AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETD ")
                        .AppendLine("                  WHERE ODVALETD.ORDERNO   = OBS.ORDERNO ")
                        .AppendLine("                    AND ODVALETD.ACTIONID in " & etdActy & " ")
                        .AppendLine("                    AND ODVALETD.DELFLG   <> @DELFLG ")
                        .AppendLine("                    AND CASE WHEN ODVALETD.SCHEDELDATE = '1900/01/01'")
                        .AppendLine("                               THEN ODVALETD.SCHEDELDATEBR")
                        .AppendLine("                             ELSE ODVALETD.SCHEDELDATE END BETWEEN @ETDST AND @ETDEND")
                        .AppendLine("                 )") 'オーダー明細SHIP END
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With
                End If
                If Me.hdnETDStYMD.Value = "" And Me.hdnETAStYMD.Value <> "" Then
                    With sqlEtdEtaOrderCondition
                        .AppendLine("(    (     OBS.ETA1 BETWEEN @ETAST AND @ETAEND") 'オーダー基本のETA ETDが収まっていること
                        .AppendLine("     )")
                        .AppendLine(" OR  (     OBS.ETA2 BETWEEN @ETAST AND @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(" OR  (     EXISTS(SELECT 1 ") 'オーダー明細SHIPがETDの範囲に存在するか
                        .AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETA ")
                        .AppendLine("                  WHERE ODVALETA.ORDERNO   = OBS.ORDERNO ")
                        .AppendLine("                    AND ODVALETA.ACTIONID in " & etaActy & " ")
                        .AppendLine("                    AND ODVALETA.DELFLG   <> @DELFLG ")
                        .AppendLine("                    AND CASE WHEN ODVALETA.SCHEDELDATE = '1900/01/01'")
                        .AppendLine("                               THEN ODVALETA.SCHEDELDATEBR")
                        .AppendLine("                             ELSE ODVALETA.SCHEDELDATE END BETWEEN @ETAST AND @ETAEND")
                        .AppendLine("                 )") 'オーダー明細SHIP END
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With
                End If
            End If
            '実績パターン
            If Me.hdnSearchType.Value = "02FIX" Then

                'オーダー明細
                If Me.hdnETDStYMD.Value <> "" And Me.hdnETAStYMD.Value <> "" Then
                    With sqlEtdEtaOrderCondition
                        .AppendLine("(    (     EXISTS(SELECT 1 ") 'オーダー明細SHIPがETDの範囲に存在するか
                        .AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETD ")
                        .AppendLine("                  WHERE ODVALETD.ORDERNO    = OBS.ORDERNO ")
                        .AppendLine("                    AND ODVALETD.ACTIONID  in " & etdActy & " ")
                        .AppendLine("                    AND ODVALETD.DELFLG    <> @DELFLG ")
                        .AppendLine("                    AND ODVALETD.ACTUALDATE BETWEEN @ETDST AND @ETDEND")
                        .AppendLine("                 )") 'オーダー明細SHIP END
                        .AppendLine("      AND  EXISTS(SELECT 1 ") 'オーダー明細ARVDがETAの範囲に存在するか
                        .AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETA ")
                        .AppendLine("                  WHERE ODVALETA.ORDERNO    = OBS.ORDERNO ")
                        .AppendLine("                    AND ODVALETA.ACTIONID  in " & etaActy & " ")
                        .AppendLine("                    AND ODVALETA.DELFLG    <> @DELFLG ")
                        .AppendLine("                    AND ODVALETA.ACTUALDATE BETWEEN @ETAST AND @ETAEND")
                        .AppendLine("                 )")
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With
                End If
                If Me.hdnETDStYMD.Value <> "" And Me.hdnETAStYMD.Value = "" Then
                    With sqlEtdEtaOrderCondition
                        .AppendLine("(    (     EXISTS(SELECT 1 ") 'オーダー明細SHIPがETDの範囲に存在するか
                        .AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETD ")
                        .AppendLine("                  WHERE ODVALETD.ORDERNO    = OBS.ORDERNO ")
                        .AppendLine("                    AND ODVALETD.ACTIONID  in " & etdActy & " ")
                        .AppendLine("                    AND ODVALETD.DELFLG    <> @DELFLG ")
                        .AppendLine("                    AND ODVALETD.ACTUALDATE BETWEEN @ETDST AND @ETDEND")
                        .AppendLine("                 )") 'オーダー明細SHIP END
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With
                End If
                If Me.hdnETDStYMD.Value = "" And Me.hdnETAStYMD.Value <> "" Then
                    With sqlEtdEtaOrderCondition
                        .AppendLine("(    (     EXISTS(SELECT 1 ") 'オーダー明細ARVDがETAの範囲に存在するか
                        .AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETA ")
                        .AppendLine("                  WHERE ODVALETA.ORDERNO    = OBS.ORDERNO ")
                        .AppendLine("                    AND ODVALETA.ACTIONID  in " & etaActy & " ")
                        .AppendLine("                    AND ODVALETA.DELFLG    <> @DELFLG ")
                        .AppendLine("                    AND ODVALETA.ACTUALDATE BETWEEN @ETAST AND @ETAEND")
                        .AppendLine("                 )")
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With
                End If

            End If
        End If


        'オーダー明細のWidth句(当明細が含まれるブレーカーも対象（削除除く）)
        sqlStat.AppendLine("With W_ORDERLIST As (")
        sqlStat.AppendLine("SELECT '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(OBS.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,OBS.BRID")
        sqlStat.AppendLine("      ,OBS.BLID1")
        sqlStat.AppendLine("      ,OBS.BLID2")
        sqlStat.AppendLine("      ,'-'         AS APPLOVAL") '一旦1固定
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETD1", String.Format(etdDatefield, "1"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETA1", String.Format(etaDatefield, "1"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETD2", String.Format(etdDatefield, "2"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETA2", String.Format(etaDatefield, "2"))
        'sqlStat.AppendLine("      ,OBS.SHIPPER AS SHIPPER")
        sqlStat.AppendFormat("      ,ISNULL(SP.{0}, ISNULL(AGS.{1},'')) AS SHIPPER", textCustomerTblField, textTraderTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(PD.{0},'') AS PRODUCTCODE", textProductTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CN.{0}, ISNULL(AGC.{1},'')) AS CONSIGNEE", textCustomerTblField, textTraderTblField).AppendLine()
        sqlStat.AppendLine("      ,OBS.BRTYPE AS BRTYPE")
        sqlStat.AppendLine("      ,ISNULL(POL1.COUNTRYCODE,'')  AS POLCOUNTRY1")
        sqlStat.AppendLine("      ,ISNULL(POL1.AREANAME,'')  AS POLPORT1")
        sqlStat.AppendLine("      ,ISNULL(POD1.COUNTRYCODE,'')  AS PODCOUNTRY1")
        sqlStat.AppendLine("      ,ISNULL(POD1.AREANAME,'')  AS PODPORT1")
        sqlStat.AppendLine("      ,ISNULL(POL2.COUNTRYCODE,'')  AS POLCOUNTRY2")
        sqlStat.AppendLine("      ,ISNULL(POL2.AREANAME,'')  AS POLPORT2")
        sqlStat.AppendLine("      ,ISNULL(POD2.COUNTRYCODE,'')  AS PODCOUNTRY2")
        sqlStat.AppendLine("      ,ISNULL(POD2.AREANAME,'')  AS PODPORT2")
        sqlStat.AppendLine("      ,OVSCNT.NOOFORDER AS NOOFTANKS")
        sqlStat.AppendLine("      ,OBS.ORDERNO AS ODID")
        sqlStat.AppendLine("      ,'-'         AS BLISSUE")
        sqlStat.AppendLine("      ,CASE WHEN EXISTS(SELECT 1 ") 'SHIPの実績日が含まれてるレコード用
        sqlStat.AppendLine("                          FROM GBT0005_ODR_VALUE OVAL1")
        sqlStat.AppendLine("                         WHERE OVAL1.ORDERNO     = OBS.ORDERNO ")
        sqlStat.AppendLine("                           AND OVAL1.ACTIONID    = 'SHIP'")
        sqlStat.AppendLine("                           AND OVAL1.ACTUALDATE <> '1900/01/01'") '初期値以外な実績日を入力したと判定
        sqlStat.AppendLine("                           AND OVAL1.DELFLG     <> @DELFLG)")
        sqlStat.AppendLine("              OR EXISTS(SELECT 1 ") '申請中のオーダーレコード用
        sqlStat.AppendLine("                          FROM      GBT0005_ODR_VALUE OVAL2")
        sqlStat.AppendLine("                         INNER JOIN COT0002_APPROVALHIST APH")
        sqlStat.AppendLine("                            ON APH.APPLYID  = OVAL2.APPLYID")
        sqlStat.AppendLine("                           AND APH.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("                           AND APH.STEP     = OVAL2.LASTSTEP")
        sqlStat.AppendLine("                           AND APH.STATUS   = '" & C_APP_STATUS.APPLYING & "'") '承認中レコード
        sqlStat.AppendLine("                           AND APH.DELFLG  <> @DELFLG")
        sqlStat.AppendLine("                         WHERE OVAL2.ORDERNO = OBS.ORDERNO ")
        sqlStat.AppendLine("                           AND OVAL2.APPLYID > ''") '申請ID在りのレコードに限定
        sqlStat.AppendLine("                           AND OVAL2.DELFLG <> @DELFLG)")
        sqlStat.AppendLine("            THEN '0' ") '上記サブクエリがレコードを返したら削除不可
        sqlStat.AppendLine("            ELSE '1' ") '上記サブクエリーがレコードを返さなければ削除可
        sqlStat.AppendLine("        END AS CANDELETEORDER") 'オーダー削除可能判定('0':削除不可 '1':削除可)
        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE OBS")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'SHIPPER名称用JOIN
        sqlStat.AppendLine("    ON  SP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  SP.COUNTRYCODE  = OBS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  SP.CUSTOMERCODE = OBS.SHIPPER")
        sqlStat.AppendLine("   AND  SP.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  SP.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  SP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CN") 'CONSIGNEE名称用JOIN
        sqlStat.AppendLine("    ON  CN.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CN.COUNTRYCODE  = OBS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  CN.CUSTOMERCODE = OBS.CONSIGNEE")
        sqlStat.AppendLine("   AND  CN.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  CN.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  CN.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  CN.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER AGS") 'SHIPPER名称用JOIN
        sqlStat.AppendLine("    ON  AGS.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AGS.COUNTRYCODE  = OBS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  AGS.CARRIERCODE  = OBS.SHIPPER")
        sqlStat.AppendLine("   AND  AGS.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  AGS.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  AGS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  AGS.CLASS        = '" & C_TRADER.CLASS.AGENT & "'")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER AGC") 'CONSIGNEE名称用JOIN
        sqlStat.AppendLine("    ON  AGC.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AGC.COUNTRYCODE  = OBS.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("   AND  AGC.CARRIERCODE  = OBS.CONSIGNEE")
        sqlStat.AppendLine("   AND  AGC.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  AGC.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  AGC.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  AGC.CLASS        = '" & C_TRADER.CLASS.AGENT & "'")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  PD.CUSTOMERCODE = BS.SHIPPER")
        'sqlStat.AppendLine("   AND  PD.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PD.PRODUCTCODE  = OBS.PRODUCTCODE")
        sqlStat.AppendLine("   AND  PD.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  PD.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD.ENABLED      = @ENABLED")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POL1") 'POL名称用JOIN
        sqlStat.AppendLine("    ON  POL1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  POL1.COUNTRYCODE  = OBS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  POL1.PORTCODE     = OBS.LOADPORT1")
        sqlStat.AppendLine("   AND  POL1.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  POL1.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  POL1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POD1") 'POD名称用JOIN
        sqlStat.AppendLine("    ON  POD1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  POD1.COUNTRYCODE  = OBS.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   AND  POD1.PORTCODE     = OBS.DISCHARGEPORT1")
        sqlStat.AppendLine("   AND  POD1.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  POD1.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  POD1.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POL2") 'POL名称用JOIN
        sqlStat.AppendLine("    ON  POL2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  POL2.COUNTRYCODE  = OBS.LOADCOUNTRY2")
        sqlStat.AppendLine("   AND  POL2.PORTCODE     = OBS.LOADPORT2")
        sqlStat.AppendLine("   AND  POL2.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  POL2.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  POL2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POD2") 'POD名称用JOIN
        sqlStat.AppendLine("    ON  POD2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  POD2.COUNTRYCODE  = OBS.DISCHARGECOUNTRY2")
        sqlStat.AppendLine("   AND  POD2.PORTCODE     = OBS.DISCHARGEPORT2")
        sqlStat.AppendLine("   AND  POD2.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  POD2.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  POD2.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN (SELECT OVS.ORDERNO,COUNT(DISTINCT OVS.TANKSEQ) AS NOOFORDER  ") 'オーダー数用JOIN
        sqlStat.AppendLine("               FROM GBT0005_ODR_VALUE OVS")
        sqlStat.AppendLine("              WHERE OVS.DELFLG <> @DELFLG")
        sqlStat.AppendLine("              GROUP BY OVS.ORDERNO) OVSCNT")
        sqlStat.AppendLine("    ON  OVSCNT.ORDERNO     = OBS.ORDERNO")
        sqlStat.AppendLine(" WHERE OBS.DELFLG        <> @DELFLG")
        If sqlEtdEtaOrderCondition.Length > 0 Then
            sqlStat.AppendLine(sqlEtdEtaOrderCondition.ToString)
        End If
        If Me.hdnShipper.Value <> "" Then
            sqlStat.AppendLine("   AND OBS.SHIPPER       = @SHIPPER")
        End If
        If Me.hdnConsignee.Value <> "" Then
            sqlStat.AppendLine("   AND OBS.CONSIGNEE     = @CONSIGNEE")
        End If
        'If Me.hdnPortOfLoading.Value <> "" Then
        '    sqlStat.AppendLine("   AND (   OBS.LOADPORT1     = @POL")
        '    sqlStat.AppendLine("        OR OBS.LOADPORT2     = @POL")
        '    sqlStat.AppendLine("       )")
        'End If
        'If Me.hdnPortOfDischarge.Value <> "" Then
        '    sqlStat.AppendLine("   AND (   OBS.DISCHARGEPORT1 = @POD")
        '    sqlStat.AppendLine("        OR OBS.DISCHARGEPORT2 = @POD")
        '    sqlStat.AppendLine("       )")
        'End If

        If Me.hdnDepartureArrival.Value = "01EXPORT" Then

            If Me.hdnCountry.Value <> "" AndAlso Me.hdnCountry.Value <> "ALL" Then
                sqlStat.AppendLine("   AND (   OBS.LOADCOUNTRY1 = @COUNTRY")
                sqlStat.AppendLine("        OR OBS.LOADCOUNTRY2 = @COUNTRY")
                sqlStat.AppendLine("       )")
            End If

        ElseIf Me.hdnDepartureArrival.Value = "02IMPORT" Then

            If Me.hdnCountry.Value <> "" AndAlso Me.hdnCountry.Value <> "ALL" Then
                sqlStat.AppendLine("   AND (   OBS.DISCHARGECOUNTRY1 = @COUNTRY")
                sqlStat.AppendLine("        OR OBS.DISCHARGECOUNTRY2 = @COUNTRY")
                sqlStat.AppendLine("       )")
            End If

        End If

            If Me.hdnPort.Value <> "" Then
            sqlStat.AppendLine("   AND (   OBS.LOADPORT1 = @PORT")
            sqlStat.AppendLine("        OR OBS.LOADPORT2 = @PORT")
            sqlStat.AppendLine("        OR OBS.DISCHARGEPORT1 = @PORT")
            sqlStat.AppendLine("        OR OBS.DISCHARGEPORT2 = @PORT")
            sqlStat.AppendLine("       )")
        End If

        If Me.hdnProduct.Value <> "" Then
            sqlStat.AppendLine("   AND OBS.PRODUCTCODE     = @PRODUCT")
        End If

        If Me.hdnCarrier.Value <> "" Then
            sqlStat.AppendLine("   AND (   OBS.CARRIER1 = @CARRIER")
            sqlStat.AppendLine("        OR OBS.CARRIER2 = @CARRIER")
            sqlStat.AppendLine("       )")
        End If

        If Me.hdnVsl.Value <> "" Then
            sqlStat.AppendLine("   AND (   OBS.VSL1 = @VSL")
            sqlStat.AppendLine("        OR OBS.VSL2 = @VSL")
            sqlStat.AppendLine("       )")
        End If

        If Me.hdnOffice.Value <> "" Then
            'OFFICE
            sqlStat.AppendLine("   AND (    OBS.AGENTORGANIZER = @OFFICECODE")
            sqlStat.AppendLine("         OR OBS.AGENTPOL1      = @OFFICECODE")
            sqlStat.AppendLine("         OR OBS.AGENTPOL2      = @OFFICECODE")
            sqlStat.AppendLine("         OR OBS.AGENTPOD1      = @OFFICECODE")
            sqlStat.AppendLine("         OR OBS.AGENTPOD2      = @OFFICECODE")
            sqlStat.AppendLine("       )")
        End If

        '一旦保留(BL発行有無)
        If Me.hdnBlIssued.Value = "Y" Then
            sqlStat.AppendLine("   AND (   OBS.BLID1 <> ''")
            sqlStat.AppendLine("        OR OBS.BLID2 <> ''")
            sqlStat.AppendLine("       )")
        End If
        If Me.hdnBlIssued.Value = "N" Then
            sqlStat.AppendLine("   AND (   OBS.BLID1 = ''")
            sqlStat.AppendLine("        OR OBS.BLID2 = ''")
            sqlStat.AppendLine("       )")
        End If

        sqlStat.AppendLine(")")
        '共通関数は単一テーブル想定のため全体をサブクエリー化 
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        If GBA00003UserSetting.IS_JOTUSER Then
            sqlStat.AppendLine("      ,CASE WHEN POLCOUNTRY1 <> '' OR PODCOUNTRY1 <> '' THEN '1' ELSE '' END  AS SHOWEDIT1 ")
            sqlStat.AppendLine("      ,CASE WHEN POLCOUNTRY2 <> '' OR PODCOUNTRY2 <> '' THEN '1' ELSE '' END  AS SHOWEDIT2 ")
        Else
            sqlStat.AppendLine("      ,CASE WHEN POLCOUNTRY1 = @COUNTRY OR PODCOUNTRY1 = @COUNTRY THEN '1' ELSE '' END  AS SHOWEDIT1 ")
            sqlStat.AppendLine("      ,CASE WHEN POLCOUNTRY2 = @COUNTRY OR PODCOUNTRY2 = @COUNTRY THEN '1' ELSE '' END  AS SHOWEDIT2 ")
        End If
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine(" SELECT * FROM W_ORDERLIST) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)

        'DB接続
        Using sqlCon As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon"))),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                If Me.hdnETDStYMD.Value <> "" Then
                    .Add("@ETDST", SqlDbType.Date).Value = Date.ParseExact(Me.hdnETDStYMD.Value, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
                    .Add("@ETDEND", SqlDbType.Date).Value = Date.ParseExact(Me.hdnETDEndYMD.Value, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
                End If
                If Me.hdnETAStYMD.Value <> "" Then
                    .Add("@ETAST", SqlDbType.Date).Value = Date.ParseExact(Me.hdnETAStYMD.Value, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
                    .Add("@ETAEND", SqlDbType.Date).Value = Date.ParseExact(Me.hdnETAEndYMD.Value, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
                End If
                'If Me.hdnPortOfLoading.Value <> "" Then
                '    .Add("@POL", SqlDbType.NVarChar).Value = Me.hdnPortOfLoading.Value
                'End If
                'If Me.hdnPortOfDischarge.Value <> "" Then
                '    .Add("@POD", SqlDbType.NVarChar).Value = Me.hdnPortOfDischarge.Value
                'End If
                If Me.hdnDepartureArrival.Value <> "" AndAlso Me.hdnCountry.Value <> "" AndAlso Me.hdnCountry.Value <> "ALL" Then
                    .Add("@COUNTRY", SqlDbType.NVarChar).Value = Me.hdnCountry.Value
                Else
                    .Add("@COUNTRY", SqlDbType.NVarChar).Value = GBA00003UserSetting.COUNTRYCODE
                End If

                If Me.hdnShipper.Value <> "" Then
                    .Add("@SHIPPER", SqlDbType.NVarChar).Value = Me.hdnShipper.Value
                End If

                If Me.hdnConsignee.Value <> "" Then
                    .Add("@CONSIGNEE", SqlDbType.NVarChar).Value = Me.hdnConsignee.Value
                End If

                If Me.hdnPort.Value <> "" Then
                    .Add("@PORT", SqlDbType.NVarChar).Value = Me.hdnPort.Value
                End If

                If Me.hdnProduct.Value <> "" Then
                    .Add("@PRODUCT", SqlDbType.NVarChar).Value = Me.hdnProduct.Value
                End If

                If Me.hdnCarrier.Value <> "" Then
                    .Add("@CARRIER", SqlDbType.NVarChar).Value = Me.hdnCarrier.Value
                End If

                If Me.hdnVsl.Value <> "" Then
                    .Add("@VSL", SqlDbType.NVarChar).Value = Me.hdnVsl.Value
                End If

                If Me.hdnOffice.Value <> "" Then
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.hdnOffice.Value
                End If

            End With
            'SQLパラメータ(動的変化あり)
            Dim paramValidityfrom As SqlParameter = Nothing
            Dim paramValidityto As SqlParameter = Nothing
            'Dim paramShipper As SqlParameter = Nothing
            'Dim paramConsignee As SqlParameter = Nothing
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
    ''' 一覧表★ボタン押下時処理
    ''' </summary>
    Public Sub btnListAction_Click()
        Dim currentRownum As String = Me.hdnListCurrentRownum.Value
        Dim rowId As Integer = 0
        If Integer.TryParse(currentRownum, rowId) = True Then
            rowId = rowId - 1
        Else
            Return
        End If

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
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If
        'この段階でありえないが初期検索結果がない場合は終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If
        '引き渡す情報を当画面のHidden項目に格納
        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim brId As String = Convert.ToString(selectedRow.Item("BRID"))
        Dim odId As String = Convert.ToString(selectedRow.Item("ODID"))
        Me.hdnSelectedBrId.Value = brId
        Me.hdnSelectedOdId.Value = odId

        '■■■ 画面遷移先URL取得 ■■■
        Dim mapIdp As String = CONST_MAPID & "R"
        Dim varP As String = "GB_OrderNew"

        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = mapIdp
        COA0012DoUrl.VARIP = varP
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        Session("MAPmapid") = mapIdp
        Session("MAPvariant") = varP
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
        ''JavaScriptにて別タブ表示を実行するフラグを立てる
        'Me.hdnBreakerViewOpen.Value = "1"
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

        AddLangSetting(dicDisplayText, Me.lblShipperLabel, "荷主", "SHIPPER")
        AddLangSetting(dicDisplayText, Me.lblConsigneeLabel, "荷受人", "CONSIGNEE")

        AddLangSetting(dicDisplayText, Me.hdnConfirmTitle, "削除しますよろしいですか？", "Are you sure you want to delete?")
        AddLangSetting(dicDisplayText, Me.lblConfirmOrderNoName, "オーダーID", "Order ID")

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
        retDt.Columns.Add("ACTION", GetType(String))
        retDt.Columns.Add("BRID", GetType(String))
        retDt.Columns.Add("BLID1", GetType(String))
        retDt.Columns.Add("BLID2", GetType(String))
        retDt.Columns.Add("APPLOVAL", GetType(String))

        retDt.Columns.Add("ETD1", GetType(String))
        retDt.Columns.Add("ETA1", GetType(String))
        retDt.Columns.Add("ETD2", GetType(String))
        retDt.Columns.Add("ETA2", GetType(String))
        retDt.Columns.Add("SHIPPER", GetType(String))
        retDt.Columns.Add("PRODUCTCODE", GetType(String))
        retDt.Columns.Add("CONSIGNEE", GetType(String))
        retDt.Columns.Add("POL", GetType(String))
        retDt.Columns.Add("POD", GetType(String))
        retDt.Columns.Add("NOOFTANKS", GetType(String))
        retDt.Columns.Add("ODID", GetType(String))
        retDt.Columns.Add("DELETEFLAG", GetType(String))

        retDt.Columns.Add("SHOWEDIT1", GetType(String))
        retDt.Columns.Add("SHOWEDIT2", GetType(String))

        retDt.Columns.Add("BRTYPE", GetType(String))

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
        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim brId As String = Convert.ToString(selectedRow.Item("BRID"))
        Dim odId As String = Convert.ToString(selectedRow.Item("ODID"))
        Dim mapIdp As String = CONST_MAPID & "R"
        Me.hdnThisMapVariant.Value = "GB_PRINT"
        Dim varP As String = Me.hdnThisMapVariant.Value

        Me.hdnSelectedBrId.Value = brId
        Me.hdnSelectedOdId.Value = odId

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = mapIdp
        COA0012DoUrl.VARIP = varP
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        Session("MAPmapid") = mapIdp
        Session("MAPvariant") = varP
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
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
        COA0013TableObject.MAPID = CONST_MAPID
        COA0013TableObject.VARI = Me.hdnReportVariant.Value
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = Me.WF_LISTAREA
        COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.LEVENT = "ondblclick"
        COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

        For Each dr As DataRow In listData.Rows
            Dim btnEdit1 = WF_LISTAREA.FindControl("btn" & Me.WF_LISTAREA.ID & "EDIT1" + dr.Item("LINECNT").ToString)
            If dr.Item("SHOWEDIT1").ToString = "1" Then
                btnEdit1.Visible = True
            Else
                btnEdit1.Visible = False
            End If

            Dim btnEdit2 = WF_LISTAREA.FindControl("btn" & Me.WF_LISTAREA.ID & "EDIT2" + dr.Item("LINECNT").ToString)
            If dr.Item("SHOWEDIT2").ToString = "1" Then
                btnEdit2.Visible = True
            Else
                btnEdit2.Visible = False
            End If
        Next

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
        COA0018ViewList.MAPID = CONST_BASEID
        Me.hdnThisMapVariant.Value = "GB_PRINT"
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
        If TypeOf Page.PreviousPage Is GBT00013SELECT Then
            Me.hdnMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))

            '検索画面の場合
            Dim prevObj As GBT00013SELECT = DirectCast(Page.PreviousPage, GBT00013SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"rblSearchType", Me.hdnSearchType},
                                                                        {"txtBlIssued", Me.hdnBlIssued},
                                                                        {"txtETDStYMD", Me.hdnETDStYMD},
                                                                        {"txtETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"txtShipper", Me.hdnShipper},
                                                                        {"txtConsignee", Me.hdnConsignee},
                                                                        {"txtPort", Me.hdnPort},
                                                                        {"txtProduct", Me.hdnProduct},
                                                                        {"txtCarrier", Me.hdnCarrier},
                                                                        {"txtVsl", Me.hdnVsl},
                                                                        {"txtCountry", Me.hdnCountry},
                                                                        {"txtOffice", Me.hdnOffice},
                                                                        {"rblDepartureArrival", Me.hdnDepartureArrival}
                                                                        }

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

            hdnReportVariant.Value = "Default"

        ElseIf TypeOf Page.PreviousPage Is GBT00004ORDER Then
            'オーダー入力画面からの遷移
            Dim prevObj As GBT00004ORDER = DirectCast(Page.PreviousPage, GBT00004ORDER)
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
                                                                        {"hdnOrderNo", Me.hdnSelectedOdId},
                                                                        {"hdnTrans", Me.hdnSelectedTrans},
                                                                        {"hdnReportVariant", Me.hdnReportVariant}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            Dim hdnObj As HiddenField = DirectCast(prevObj.FindControl("hdnListId"), HiddenField)
            If hdnObj IsNot Nothing Then
                If Me.lbRightList.Items.FindByValue(hdnObj.Value) IsNot Nothing Then
                    Me.lbRightList.SelectedValue = hdnObj.Value
                End If
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00014BL Then
            'オーダー入力画面からの遷移
            Dim prevObj As GBT00014BL = DirectCast(Page.PreviousPage, GBT00014BL)
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
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            Dim hdnObj As HiddenField = DirectCast(prevObj.FindControl("hdnListId"), HiddenField)
            If hdnObj IsNot Nothing Then
                If Me.lbRightList.Items.FindByValue(hdnObj.Value) IsNot Nothing Then
                    Me.lbRightList.SelectedValue = hdnObj.Value
                End If
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00017RESULT Then
            '自身から遷移（削除時のリフレッシュのみの想定、それ以外の用途を追加する場合は注意）
            Dim prevObj As GBT00017RESULT = DirectCast(Page.PreviousPage, GBT00017RESULT)
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
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next

            Dim prevLbRightObj As ListBox = DirectCast(prevObj.FindControl(Me.lbRightList.ID), ListBox)
            If prevLbRightObj IsNot Nothing Then
                Me.lbRightList.SelectedValue = prevLbRightObj.SelectedValue
            End If

            HttpContext.Current.Session("MAPvariant") = "GB_PRINT"

            '正常メッセージをメッセージエリアに表示（削除正常時のみ自身をリフレッシュするため）
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
        End If
        Me.WF_LISTAREA.CssClass = Me.hdnSearchType.Value
    End Sub

    ''' <summary>
    ''' 一覧 Edit1 押下時
    ''' </summary>
    Public Sub btnListEdit1_Click()

        BLEditTrans("1")

    End Sub

    ''' <summary>
    ''' 一覧 Edit2 押下時
    ''' </summary>
    Public Sub btnListEdit2_Click()

        BLEditTrans("2")


    End Sub

    ''' <summary>
    ''' B/L編集遷移処理
    ''' </summary>
    Public Sub BLEditTrans(ByVal trans As String)
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

        Dim currentRowNum As String = Me.hdnListCurrentRownum.Value
        Dim clickedRow As DataRow = dt.Rows((CInt(currentRowNum) - 1))
        'ありえないが対象の行番号のデータがない場合
        If clickedRow Is Nothing Then
            Return 'そのまま終了
        End If
        '選択レコード情報設定
        Me.hdnSelectedOdId.Value = Convert.ToString(clickedRow.Item("ODID"))
        Me.hdnSelectedTrans.Value = trans

        Dim mapIdp As String = "GBT00017R"
        Dim varP As String = "GB_EDIT"

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = mapIdp
        COA0012DoUrl.VARIP = varP
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        Session("MAPmapid") = mapIdp
        Session("MAPvariant") = varP
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

    End Sub

End Class