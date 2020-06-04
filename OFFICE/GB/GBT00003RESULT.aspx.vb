Imports System.Data.SqlClient
Imports BASEDLL
Public Class GBT00003RESULT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00003" '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00004" '次画面一覧のMAPID
    Private Const CONST_MAPVARI As String = "GB_Default"
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
                Me.hdnThisMapVariant.Value = CONST_MAPVARI 'どこから遷移されてもVariantは固定
                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, Me.hdnThisMapVariant.Value)
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID & "R"
                COA0031ProfMap.VARIANTP = "OrderBrowser"
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
                SetListViewTypeListItem()
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
                        .USERSORTOPT = 0
                    End With
                    COA0013TableObject.COA0013SetTableObject()

                End Using 'DataTable
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                '戻りURL設定
                '****************************************
                Me.hdnBreakerViewUrl.Value = GetBreakerUrl()

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
            DisplayListObjEdit()
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
                Case Me.vLeftProduct.ID
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    Dim dt As DataTable = GBA00014Product.GBA00014getProductCodeValue(enabled:=CONST_FLAG_YES)
                    With Me.lbProduct
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With

                    '入力済のデータを選択状態にする
                    If dblClickField IsNot Nothing AndAlso lbProduct.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbProduct.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If

                Case Me.vLeftPort.ID
                    '港ビュー表示
                    Dim dt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue()
                    With lbPort
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "PORTCODE"
                        .DataBind()
                        .Focus()
                    End With
                    '入力済のデータを選択状態にする
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    If dblClickField IsNot Nothing AndAlso lbPort.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbPort.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If

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
        If Me.txtShipper.Text.Trim <> "" OrElse Me.txtConsignee.Text.Trim <> "" _
            OrElse Me.rblListViewType.SelectedValue <> "ALL" _
            OrElse Me.txtProduct.Text.Trim <> "" OrElse Me.txtPol.Text.Trim <> "" _
            OrElse Me.txtPod.Text.Trim <> "" OrElse Me.txtBreaker.Text.Trim <> "" _
            OrElse Me.txtOrderId.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtShipper.Text.Trim = "" OrElse Convert.ToString(dr("SHIPPER")).Contains(Me.txtShipper.Text.Trim)) _
                  AndAlso (Me.txtConsignee.Text.Trim = "" OrElse Convert.ToString(dr("CONSIGNEE")).Contains(Me.txtConsignee.Text.Trim)) _
                  AndAlso (Me.txtProduct.Text.Trim = "" OrElse Convert.ToString(dr("PRODUCT")).Equals(Me.txtProduct.Text.Trim)) _
                  AndAlso (Me.txtPol.Text.Trim = "" OrElse Convert.ToString(dr("POL1CODE")).Equals(Me.txtPol.Text.Trim)) _
                  AndAlso (Me.txtPod.Text.Trim = "" OrElse Convert.ToString(dr("POD1CODE")).Equals(Me.txtPod.Text.Trim)) _
                  AndAlso (Me.txtBreaker.Text.Trim = "" OrElse Convert.ToString(dr("BRID")).StartsWith(Me.txtBreaker.Text.Trim)) _
                  AndAlso (Me.txtOrderId.Text.Trim = "" OrElse Convert.ToString(dr("ODID")).StartsWith(Me.txtOrderId.Text.Trim)) _
                  AndAlso (Me.rblListViewType.SelectedValue = "ALL" OrElse Me.rblListViewType.SelectedValue = "BRONLY" AndAlso Convert.ToString(dr("BRODFLG")) = "1")) Then
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
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
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
        'ソート順取得
        COA0020ProfViewSort.MAPID = mapId
        COA0020ProfViewSort.VARI = "Default"
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()
        '個別入力条件の設定
        Dim sqlEtdEtaBreakerCondition As New StringBuilder
        Dim sqlEtdEtaOrderCondition As New StringBuilder
        Dim etdDatefield As String = ""
        Dim etaDatefield As String = ""
        Dim etdActy As String = "('SHIP','RPEC','RPED','RPHC','RPHD')"
        Dim etaActy As String = "('ARVD','DCEC','DCED','ETYC')"
        If Me.hdnETDStYMD.Value <> "" OrElse Me.hdnETAStYMD.Value <> "" Then
            sqlEtdEtaBreakerCondition.AppendLine(" AND ")
            sqlEtdEtaOrderCondition.AppendLine(" AND ")
            'TODO冗長なので考える
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
                'オーダー明細
                If Me.hdnETDStYMD.Value <> "" AndAlso Me.hdnETAStYMD.Value <> "" Then
                    With sqlEtdEtaBreakerCondition
                        .AppendLine("((BS.ETD1 BETWEEN @ETDST And @ETDEND") '
                        .AppendLine("      And  BS.ETA1 BETWEEN @ETAST And @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(" Or  (     BS.ETD2 BETWEEN @ETDST And @ETDEND")
                        .AppendLine("      And  BS.ETA2 BETWEEN @ETAST And @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(" Or  (     BS.VALIDITYTO   >= @ETDST And BS.VALIDITYFROM <= @ETDEND")
                        .AppendLine("      And  BS.VALIDITYFROM <= @ETAST And BS.VALIDITYTO >= @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With

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
                If Me.hdnETDStYMD.Value <> "" AndAlso Me.hdnETAStYMD.Value = "" Then

                    With sqlEtdEtaBreakerCondition
                        .AppendLine("(    (     BS.ETD1 BETWEEN @ETDST AND @ETDEND") '
                        .AppendLine("     )")
                        .AppendLine(" OR  (     BS.ETD2 BETWEEN @ETDST AND @ETDEND")
                        .AppendLine("     )")
                        .AppendLine(" OR  (      BS.VALIDITYTO   >= @ETDST AND BS.VALIDITYFROM <= @ETDEND")
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With
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
                If Me.hdnETDStYMD.Value = "" AndAlso Me.hdnETAStYMD.Value <> "" Then
                    With sqlEtdEtaBreakerCondition
                        .AppendLine("(    (     BS.ETA1 BETWEEN @ETAST AND @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(" OR  (     BS.ETA2 BETWEEN @ETAST AND @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(" OR  (     BS.VALIDITYFROM <= @ETAST AND BS.VALIDITYTO >= @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(")")
                    End With

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

                sqlEtdEtaBreakerCondition.AppendLine("(1=2)") '実績はブレーカーを出さないため打ち消す（後続でオーダーにかかる実績は出す）
                'オーダー明細
                If Me.hdnETDStYMD.Value <> "" AndAlso Me.hdnETAStYMD.Value <> "" Then
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
                If Me.hdnETDStYMD.Value <> "" AndAlso Me.hdnETAStYMD.Value = "" Then
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
                If Me.hdnETDStYMD.Value = "" AndAlso Me.hdnETAStYMD.Value <> "" Then
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
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,OBS.BRID")
        sqlStat.AppendLine("      ,'2'         AS BRODFLG")  'ブレーカーオーダーフラグ(ブレーカー)
        sqlStat.AppendLine("      ,'-'         AS APPLOVAL") '一旦1固定
        sqlStat.AppendLine("      ,''          AS VALIDITYFROM")
        sqlStat.AppendLine("      ,''          AS VALIDITYTO")
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETD1", String.Format(etdDatefield, "1"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETA1", String.Format(etaDatefield, "1"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETD2", String.Format(etdDatefield, "2"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETA2", String.Format(etaDatefield, "2"))
        'sqlStat.AppendLine("      ,OBS.SHIPPER AS SHIPPER")
        sqlStat.AppendFormat("      ,ISNULL(SP.{0},'') AS SHIPPER", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,''              AS PRODUCTCODE")
        sqlStat.AppendLine("      ,OBS.PRODUCTCODE AS PRODUCT")
        'sqlStat.AppendLine("      ,OBS.CONSIGNEE AS CONSIGNEE")
        sqlStat.AppendFormat("      ,ISNULL(CN.{0},'') AS CONSIGNEE", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,''          AS POL")
        sqlStat.AppendLine("      ,''          AS POD")
        sqlStat.AppendLine("      ,ISNULL(OBS.LOADPORT1,'')       AS POL1CODE")
        sqlStat.AppendLine("      ,ISNULL(OBS.DISCHARGEPORT1,'')  AS POD1CODE")
        sqlStat.AppendLine("      ,ISNULL(OBS.LOADPORT2,'')       AS POL2CODE")
        sqlStat.AppendLine("      ,ISNULL(OBS.DISCHARGEPORT2,'')  AS POD2CODE")
        sqlStat.AppendLine("      ,OVSCNT.NOOFORDER AS NOOFTANKS")
        sqlStat.AppendLine("      ,OBS.ORDERNO AS ODID")
        sqlStat.AppendLine("      ,'-'         AS BLISSUE")
        sqlStat.AppendLine("      ,CASE WHEN EXISTS(SELECT 1 ") 'SHIPの実績日が含まれてるレコード用
        sqlStat.AppendLine("                          FROM GBT0005_ODR_VALUE OVAL1")
        sqlStat.AppendLine("                         WHERE OVAL1.ORDERNO     = OBS.ORDERNO ")
        sqlStat.AppendLine("                           AND OVAL1.ACTIONID   IN ('SHIP','RPEC','RPED','RPHC','RPHD')")
        sqlStat.AppendLine("                           AND OVAL1.ACTUALDATE <> '1900/01/01'") '初期値以外な実績日を入力したと判定
        sqlStat.AppendLine("                           AND OVAL1.DELFLG     <> @DELFLG)")
        'sqlStat.AppendLine("              OR EXISTS(SELECT 1 ") '申請中のオーダーレコード用
        'sqlStat.AppendLine("                          FROM      GBT0005_ODR_VALUE OVAL2")
        'sqlStat.AppendLine("                         INNER JOIN COT0002_APPROVALHIST APH")
        'sqlStat.AppendLine("                            ON APH.APPLYID  = OVAL2.APPLYID")
        'sqlStat.AppendLine("                           AND APH.COMPCODE = @COMPCODE")
        'sqlStat.AppendLine("                           AND APH.STEP     = OVAL2.LASTSTEP")
        'sqlStat.AppendLine("                           AND APH.STATUS   = '" & C_APP_STATUS.APPLYING & "'") '承認中レコード
        'sqlStat.AppendLine("                           AND APH.DELFLG  <> @DELFLG")
        'sqlStat.AppendLine("                         WHERE OVAL2.ORDERNO = OBS.ORDERNO ")
        'sqlStat.AppendLine("                           AND OVAL2.APPLYID > ''") '申請ID在りのレコードに限定
        'sqlStat.AppendLine("                           AND OVAL2.DELFLG <> @DELFLG)")
        sqlStat.AppendLine("            THEN '0' ") '上記サブクエリがレコードを返したら削除不可
        sqlStat.AppendLine("            ELSE '1' ") '上記サブクエリーがレコードを返さなければ削除可
        sqlStat.AppendLine("        END AS CANDELETEORDER") 'オーダー削除可能判定('0':削除不可 '1':削除可)
        sqlStat.AppendLine("      ,OBS.BOOKINGNO AS BOOKINGNO")
        sqlStat.AppendLine("      ,OBS.VSL1 AS VSL1")
        sqlStat.AppendLine("      ,OBS.VOY1 AS VOY1")
        sqlStat.AppendLine("      ,'' AS DISABLED")
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
        sqlStat.AppendLine("   AND  CN.COUNTRYCODE  = OBS.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("   AND  CN.CUSTOMERCODE = OBS.CONSIGNEE")
        sqlStat.AppendLine("   AND  CN.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  CN.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  CN.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  CN.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")
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
        If Me.hdnPortOfLoading.Value <> "" Then
            sqlStat.AppendLine("   AND (   OBS.LOADPORT1     = @POL")
            sqlStat.AppendLine("        OR OBS.LOADPORT2     = @POL")
            sqlStat.AppendLine("       )")
        End If
        If Me.hdnPortOfDischarge.Value <> "" Then
            sqlStat.AppendLine("   AND (   OBS.DISCHARGEPORT1 = @POD")
            sqlStat.AppendLine("        OR OBS.DISCHARGEPORT2 = @POD")
            sqlStat.AppendLine("       )")
        End If
        If Me.hdnOffice.Value <> "" Then
            sqlStat.AppendLine("   AND (    OBS.AGENTORGANIZER = @OFFICECODE")
            sqlStat.AppendLine("         OR OBS.AGENTPOL1      = @OFFICECODE")
            sqlStat.AppendLine("         OR OBS.AGENTPOL2      = @OFFICECODE")
            sqlStat.AppendLine("         OR OBS.AGENTPOD1      = @OFFICECODE")
            sqlStat.AppendLine("         OR OBS.AGENTPOD2      = @OFFICECODE")
            sqlStat.AppendLine("       )")
        End If
        sqlStat.AppendLine(")")
        '共通関数は単一テーブル想定のため全体をサブクエリー化 
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        sqlStat.AppendLine("FROM (")

        sqlStat.AppendLine("SELECT '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(BS.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,BS.BRID")
        sqlStat.AppendLine("      ,'1' AS BRODFLG")  'ブレーカーオーダーフラグ(ブレーカー)
        sqlStat.AppendLine("      ,'1' AS APPLOVAL") '一旦1固定
        sqlStat.AppendLine("      ,CASE BS.VALIDITYFROM WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYFROM,'yyyy/MM/dd') END AS VALIDITYFROM")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYTO,'yyyy/MM/dd')   END AS VALIDITYTO")
        sqlStat.AppendLine("      ,CASE BS.ETD1   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETD1,'yyyy/MM/dd')   END AS ETD1")
        sqlStat.AppendLine("      ,CASE BS.ETA1   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETA1,'yyyy/MM/dd')   END AS ETA1")
        sqlStat.AppendLine("      ,CASE BS.ETD2   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETD2,'yyyy/MM/dd')   END AS ETD2")
        sqlStat.AppendLine("      ,CASE BS.ETA2   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETA2,'yyyy/MM/dd')   END AS ETA2")
        sqlStat.AppendFormat("      ,ISNULL(SP.{0},'') AS SHIPPER", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(PD.{0},'') AS PRODUCTCODE", textProductTblField).AppendLine()
        sqlStat.AppendLine("      ,BS.PRODUCTCODE    AS PRODUCT")
        sqlStat.AppendFormat("      ,ISNULL(CN.{0},'') AS CONSIGNEE", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,ISNULL(POL.AREANAME,'')  AS POL")
        sqlStat.AppendLine("      ,ISNULL(POD.AREANAME,'')  AS POD")
        sqlStat.AppendLine("      ,ISNULL(BS.LOADPORT1,'')       AS POL1CODE")
        sqlStat.AppendLine("      ,ISNULL(BS.DISCHARGEPORT1,'')  AS POD1CODE")
        sqlStat.AppendLine("      ,ISNULL(BS.LOADPORT2,'')       AS POL2CODE")
        sqlStat.AppendLine("      ,ISNULL(BS.DISCHARGEPORT2,'')  AS POD2CODE")
        sqlStat.AppendLine("      ,BS.NOOFTANKS")
        sqlStat.AppendLine("      ,'-' AS ODID")
        sqlStat.AppendLine("      ,'-' AS BLISSUE")
        sqlStat.AppendLine("      ,'' AS CANDELETEORDER")
        sqlStat.AppendLine("      ,'' AS BOOKINGNO")
        sqlStat.AppendLine("      ,'' AS VSL1")
        sqlStat.AppendLine("      ,'' AS VOY1")
        sqlStat.AppendLine("      ,BS.DISABLED AS DISABLED")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI ")
        sqlStat.AppendLine(" INNER JOIN GBT0002_BR_BASE BS")
        sqlStat.AppendLine("    ON  BS.BRID     = BI.BRID")
        sqlStat.AppendLine("   AND  BS.BRBASEID = BI.LINKID")
        sqlStat.AppendLine("   AND  BS.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'SHIPPER名称用JOIN
        sqlStat.AppendLine("    ON  SP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  SP.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  SP.CUSTOMERCODE = BS.SHIPPER")
        sqlStat.AppendLine("   AND  SP.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  SP.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  SP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CN") 'CONSIGNEE名称用JOIN
        sqlStat.AppendLine("    ON  CN.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CN.COUNTRYCODE  = BS.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("   AND  CN.CUSTOMERCODE = BS.CONSIGNEE")
        sqlStat.AppendLine("   AND  CN.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  CN.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  CN.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  CN.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  PD.CUSTOMERCODE = BS.SHIPPER")
        'sqlStat.AppendLine("   AND  PD.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PD.PRODUCTCODE  = BS.PRODUCTCODE")
        sqlStat.AppendLine("   AND  PD.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  PD.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD.ENABLED      = @ENABLED")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POL") 'POL名称用JOIN
        sqlStat.AppendLine("    ON  POL.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  POL.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  POL.PORTCODE     = BS.LOADPORT1")
        sqlStat.AppendLine("   AND  POL.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  POL.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  POL.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POD") 'POD名称用JOIN
        sqlStat.AppendLine("    ON  POD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  POD.COUNTRYCODE  = BS.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   AND  POD.PORTCODE     = BS.DISCHARGEPORT1")
        sqlStat.AppendLine("   AND  POD.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  POD.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  POD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine(" WHERE BI.TYPE           = 'INFO'")
        sqlStat.AppendLine("   AND BI.BRTYPE        <> '" & C_BRTYPE.REPAIR & "'") 'リペアブレーカーは一覧より除く
        sqlStat.AppendLine("   AND BI.DELFLG        <> @DELFLG")
        If Me.hdnShipper.Value <> "" Then
            sqlStat.AppendLine("   AND BS.SHIPPER       = @SHIPPER")
        End If
        If Me.hdnConsignee.Value <> "" Then
            sqlStat.AppendLine("   AND BS.CONSIGNEE     = @CONSIGNEE")
        End If
        If Me.hdnPortOfLoading.Value <> "" Then
            sqlStat.AppendLine("   AND (   BS.LOADPORT1     = @POL")
            sqlStat.AppendLine("        OR BS.LOADPORT2     = @POL")
            sqlStat.AppendLine("       )")
        End If
        If Me.hdnPortOfDischarge.Value <> "" Then
            sqlStat.AppendLine("   AND (   BS.DISCHARGEPORT1 = @POD")
            sqlStat.AppendLine("        OR BS.DISCHARGEPORT2 = @POD")
            sqlStat.AppendLine("       )")
        End If
        If Me.hdnOffice.Value <> "" Then
            sqlStat.AppendLine("   AND (    BS.AGENTORGANIZER = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOL1      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOL2      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOD1      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOD2      = @OFFICECODE")
            sqlStat.AppendLine("       )")
        End If
        sqlStat.AppendLine("   AND ((")
        sqlStat.AppendLine("       BS.VALIDITYFROM <> '1900/01/01'") '一旦一時保存は出すが未入力禁止
        sqlStat.AppendLine("   AND EXISTS (SELECT 1 ") '承認済条件（環境が整ったらコメント解除）
        sqlStat.AppendLine("                 FROM COT0002_APPROVALHIST APHB ")
        sqlStat.AppendLine("                WHERE APHB.APPLYID  = BI.APPLYID")
        sqlStat.AppendLine("                  AND APHB.STEP     = BI.LASTSTEP")
        sqlStat.AppendLine("                  AND APHB.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("                  AND APHB.STATUS   in ('" & C_APP_STATUS.APPROVED & "','11') ") '承認済レコード
        sqlStat.AppendLine("                  AND APHB.DELFLG  <> @DELFLG")
        sqlStat.AppendLine("               )") 'EXISTS END
        If sqlEtdEtaBreakerCondition.Length > 0 Then
            sqlStat.AppendLine(sqlEtdEtaBreakerCondition.ToString)
        End If
        sqlStat.AppendLine("        )")
        sqlStat.AppendLine("    OR EXISTS (SELECT 1 ")   'オーダーと紐づくブレーカーは削除ではない限り出す
        sqlStat.AppendLine("                 FROM W_ORDERLIST ")
        sqlStat.AppendLine("                WHERE W_ORDERLIST.BRID = BS.BRID)")
        sqlStat.AppendLine("       )")

        'ここにオーダーのユニオン
        sqlStat.AppendLine("UNION ALL ")
        sqlStat.AppendLine(" SELECT * FROM W_ORDERLIST) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
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
                If Me.hdnShipper.Value <> "" Then
                    .Add("@SHIPPER", SqlDbType.NVarChar).Value = Me.hdnShipper.Value
                End If
                If Me.hdnConsignee.Value <> "" Then
                    .Add("@CONSIGNEE", SqlDbType.NVarChar).Value = Me.hdnConsignee.Value
                End If
                If Me.hdnPortOfLoading.Value <> "" Then
                    .Add("@POL", SqlDbType.NVarChar).Value = Me.hdnPortOfLoading.Value
                End If
                If Me.hdnPortOfDischarge.Value <> "" Then
                    .Add("@POD", SqlDbType.NVarChar).Value = Me.hdnPortOfDischarge.Value
                End If
                If Me.hdnOffice.Value <> "" Then
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.hdnOffice.Value
                End If
            End With
            'SQLパラメータ(動的変化あり)
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
                Case Me.vLeftPort.ID
                    '港選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)

                    Dim targetTextBox As TextBox = Nothing

                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim portCode As String = ""
                        If Me.lbPort.SelectedItem IsNot Nothing Then
                            portCode = Me.lbPort.SelectedItem.Value
                        End If
                        SetDisplayPort(targetTextBox, portCode)
                    End If

                    If targetObject IsNot Nothing Then
                        targetObject.Focus()
                    End If

                Case Me.vLeftProduct.ID
                    '積載品選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim productCode As String = ""
                        If Me.lbProduct.SelectedItem IsNot Nothing Then
                            productCode = Me.lbProduct.SelectedItem.Value
                        End If
                        SetDisplayProduct(targetTextBox, productCode)
                    End If
                Case Else
                    '何もしない
            End Select
        End If
        '○ 画面左サイドボックス非表示は、画面JavaScriptで実行
        Me.hdnTextDbClickField.Value = ""
        Me.hdnIsLeftBoxOpen.Value = ""

        Me.lbProduct.DataSource = Nothing
        Me.lbProduct.DataBind()
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

        Me.lbProduct.DataSource = Nothing
        Me.lbProduct.DataBind()
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
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
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
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
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
    ''' 一覧表DELETEボタン押下時処理
    ''' </summary>
    Public Sub btnListDelete_Click()
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
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        'この段階でありえないが初期検索結果がない場合は終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If
        '引き渡す情報を当画面のHidden項目に格納
        Dim selectedRow As DataRow = dt.Rows(rowId)
        'SQL接続生成
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            '論理削除可能かチェック
            If CheckCanDelete(selectedRow, sqlCon) = False Then
                CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
            '各オーダーテーブルの論理削除
            Dim orderNo As String = Convert.ToString(selectedRow.Item("ODID"))
            DeleteOrderBaseValue(orderNo, sqlCon)
        End Using
        'ここまで来たら論理削除正常終了のため自身を再読み込みし一覧を再取得
        Server.Transfer(Request.Url.LocalPath)
    End Sub
    ''' <summary>
    ''' プロダクト変更時イベント
    ''' </summary>
    Public Sub txtProduct_Change()
        SetDisplayProduct(Me.txtProduct, Me.txtProduct.Text)
    End Sub
    ''' <summary>
    ''' 発港変更時イベント
    ''' </summary>
    Public Sub txtPol_Change()
        SetDisplayPort(Me.txtPol, Me.txtPol.Text)
    End Sub
    ''' <summary>
    ''' 着港変更時イベント
    ''' </summary>
    Public Sub txtPod_Change()
        SetDisplayPort(Me.txtPod, Me.txtPod.Text)
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

        AddLangSetting(dicDisplayText, Me.lblShipperLabel, "荷主", "Shipper")
        AddLangSetting(dicDisplayText, Me.lblConsigneeLabel, "荷受人", "Consignee")

        AddLangSetting(dicDisplayText, Me.lblProductLabel, "積載品", "Product")

        AddLangSetting(dicDisplayText, Me.lblPolLabel, "発港", "POL")
        AddLangSetting(dicDisplayText, Me.lblPodLabel, "着港", "POD")

        AddLangSetting(dicDisplayText, Me.lblBreakerIdLabel, "ブレーカーID", "Breaker ID")
        AddLangSetting(dicDisplayText, Me.lblOrderIdLabel, "オーダーID", "Order ID")

        AddLangSetting(dicDisplayText, Me.lblListViewTypeLabel, "種類", "View")

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
        With retDt.Columns
            '共通項目
            .Add("LINECNT", GetType(Integer))            'DBの固定フィールド
            .Add("OPERATION", GetType(String))           'DBの固定フィールド
            .Add("TIMSTP", GetType(String))              'DBの固定フィールド
            .Add("SELECT", GetType(Integer))             'DBの固定フィールド
            .Add("HIDDEN", GetType(Integer))
            '個別項目
            .Add("ACTION", GetType(String))
            .Add("BRID", GetType(String))
            .Add("BRODFLG", GetType(String))
            .Add("APPLOVAL", GetType(String))

            .Add("VALIDITYFROM", GetType(String))
            .Add("VALIDITYTO", GetType(String))
            .Add("ETD1", GetType(String))
            .Add("ETA1", GetType(String))
            .Add("ETD2", GetType(String))
            .Add("ETA2", GetType(String))
            .Add("SHIPPER", GetType(String))
            .Add("PRODUCTCODE", GetType(String))
            .Add("CONSIGNEE", GetType(String))
            .Add("POL", GetType(String))
            .Add("POD", GetType(String))
            .Add("NOOFTANKS", GetType(String))
            .Add("ODID", GetType(String))
            .Add("BLISSUE", GetType(String))
            .Add("DELETEFLAG", GetType(String))

            .Add("BOOKINGNO", GetType(String))
            .Add("VSL1", GetType(String))
            .Add("VOY1", GetType(String))
            .Add("DISABLED", GetType(String))
        End With

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
        Dim brOdFlg As String = Convert.ToString(selectedRow.Item("BRODFLG"))  'ダブルクリックされた行判定("1"ブレーカーレコード、"2"オーダーレコード)
        Dim mapIdp As String = CONST_MAPID & "R"
        Dim varP As String = "GB_OrderNew"
        If brOdFlg = "2" Then
            mapIdp = CONST_MAPID & "R"
            varP = "GB_ShowDetail"
        End If

        Me.hdnSelectedBrId.Value = brId
        Me.hdnSelectedOdId.Value = odId

        If brOdFlg = "1" Then '20181027 ダブルクリックはブレーカー単票表示
            'JavaScriptにて別タブ表示を実行するフラグを立てる
            Me.hdnBreakerViewOpen.Value = "1"
            Return
        End If

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
        COA0013TableObject.USERSORTOPT = 0
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

    End Sub
    ''' <summary>
    ''' ブレーカーURL取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetBreakerUrl() As String
        Dim brUrl As String = ""
        '■■■ 画面遷移先URL取得 ■■■]
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = "GBT00001N"
        COA0012DoUrl.VARIP = "GB_SelesNew"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return ""
        End If
        Session("MAPmapid") = "GBT00003R"
        Session("MAPvariant") = "GB_ShowBrDetail"
        '画面遷移実行
        brUrl = COA0012DoUrl.URL
        brUrl = VirtualPathUtility.ToAbsolute(brUrl) 'チルダURLから相対URLに変換
        Dim brUriObj As New Uri(Request.Url, brUrl) 'アプリルートURL+相対URL
        Return brUriObj.AbsoluteUri 'フルURLを返却(相対URLだとCHROMEではワークしない)
    End Function
    ''' <summary>
    ''' 削除可否チェック
    ''' </summary>
    ''' <param name="tr">削除対象の画面表示データテーブル行</param>
    ''' <param name="sqlCon">SQLServer接続</param>
    ''' <returns></returns>
    Private Function CheckCanDelete(tr As DataRow, sqlCon As SqlConnection) As Boolean
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT TIMSTP = cast(OBS.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE OBS")
        sqlStat.AppendLine(" WHERE OBS.ORDERNO  = @ORDERNO")
        sqlStat.AppendLine("   AND OBS.DELFLG  <> @DELFLG")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            'SQLパラメータの設定
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = Convert.ToString(tr.Item("ODID"))
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            'データを取得しタイムスタンプを比較
            Dim retDt As New DataTable
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
            If retDt IsNot Nothing AndAlso retDt.Rows.Count > 0 Then
                Dim retDr As DataRow = retDt.Rows(0)
                If retDr.Item("TIMSTP").Equals(tr.Item("TIMSTP")) Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False 'レコードが存在しない場合は削除想定のため更新不可
            End If
        End Using
    End Function
    ''' <summary>
    ''' オーダーの基本情報、および明細情報をオーダーNoより論理削除する
    ''' <param name="orderNo">論理削除対象のオーダーNo</param>
    ''' <param name="sqlCon">SQLServer接続</param>
    ''' </summary>
    Private Sub DeleteOrderBaseValue(orderNo As String, sqlCon As SqlConnection)
        '2テーブルの論理削除時間をそろえるため実施前に変数に格納
        Dim deleteTime As Date = Date.Now
        'SQL生成
        Dim sqlStatBase As New StringBuilder
        sqlStatBase.AppendLine("UPDATE GBT0004_ODR_BASE ")
        sqlStatBase.AppendLine("   SET DELFLG    = @DELFLG")
        sqlStatBase.AppendLine("      ,UPDYMD    = @UPDYMD")
        sqlStatBase.AppendLine("      ,UPDUSER   = @UPDUSER")
        sqlStatBase.AppendLine("      ,UPDTERMID = @UPDTERMID")
        sqlStatBase.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD")
        sqlStatBase.AppendLine("  WHERE ORDERNO  = @ORDERNO")
        sqlStatBase.AppendLine("    AND DELFLG  <> @DELFLG")
        Dim sqlStatValue As New StringBuilder
        sqlStatValue.AppendLine("UPDATE GBT0005_ODR_VALUE ")
        sqlStatValue.AppendLine("   SET DELFLG    = @DELFLG")
        sqlStatValue.AppendLine("      ,UPDYMD    = @UPDYMD")
        sqlStatValue.AppendLine("      ,UPDUSER   = @UPDUSER")
        sqlStatValue.AppendLine("      ,UPDTERMID = @UPDTERMID")
        sqlStatValue.AppendLine("       ,RECEIVEYMD = @RECEIVEYMD")
        sqlStatValue.AppendLine("  WHERE ORDERNO  = @ORDERNO")
        sqlStatValue.AppendLine("    AND DELFLG  <> @DELFLG")
        Dim sqlStatValue2 As New StringBuilder
        sqlStatValue2.AppendLine("UPDATE GBT0007_ODR_VALUE2 ")
        sqlStatValue2.AppendLine("   SET DELFLG    = @DELFLG")
        sqlStatValue2.AppendLine("      ,UPDYMD    = @UPDYMD")
        sqlStatValue2.AppendLine("      ,UPDUSER   = @UPDUSER")
        sqlStatValue2.AppendLine("      ,UPDTERMID = @UPDTERMID")
        sqlStatValue2.AppendLine("       ,RECEIVEYMD = @RECEIVEYMD")
        sqlStatValue2.AppendLine("  WHERE ORDERNO  = @ORDERNO")
        sqlStatValue2.AppendLine("    AND DELFLG  <> @DELFLG")
        'SQLコマンド実行
        Using sqlCmd As New SqlCommand() With {.Connection = sqlCon}
            'パラメータ設定
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@UPDYMD", SqlDbType.DateTime).Value = deleteTime
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            End With
            '2テーブル更新のためトランザクション利用
            Using tran = sqlCon.BeginTransaction
                sqlCmd.Transaction = tran
                '基本情報の論理削除
                sqlCmd.CommandText = sqlStatBase.ToString
                sqlCmd.ExecuteNonQuery()
                '明細情報の論理削除
                sqlCmd.CommandText = sqlStatValue.ToString
                sqlCmd.ExecuteNonQuery()
                '明細情報2の論理削除
                sqlCmd.CommandText = sqlStatValue2.ToString
                sqlCmd.ExecuteNonQuery()
                tran.Commit()
            End Using
        End Using

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
        COA0018ViewList.FORWARDMATCHVARIANT = "Default"
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
        COA0016VARIget.COMPCODE = ""
        COA0016VARIget.VARI = "Default" 'Convert.ToString(HttpContext.Current.Session("MAPvariant"))
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
        If TypeOf Page.PreviousPage Is GBT00003SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00003SELECT = DirectCast(Page.PreviousPage, GBT00003SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtETDStYMD", Me.hdnETDStYMD},
                                                                        {"txtETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"txtETAStYMD", Me.hdnETAStYMD},
                                                                        {"txtETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"txtShipper", Me.hdnShipper},
                                                                        {"txtConsignee", Me.hdnConsignee},
                                                                        {"txtPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"txtPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"txtOffice", Me.hdnOffice},
                                                                        {"rblSearchType", Me.hdnSearchType},
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
        ElseIf TypeOf Page.PreviousPage Is GBT00004NEWORDER Then
            'オーダー新規作成画面より戻り
            Dim prevObj As GBT00004NEWORDER = DirectCast(Page.PreviousPage, GBT00004NEWORDER)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchType", Me.hdnSearchType},
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

        ElseIf TypeOf Page.PreviousPage Is GBT00004ORDER Then
            'オーダー入力画面からの遷移
            Dim prevObj As GBT00004ORDER = DirectCast(Page.PreviousPage, GBT00004ORDER)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchType", Me.hdnSearchType},
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

        ElseIf TypeOf Page.PreviousPage Is GBT00003RESULT Then
            '自身から遷移（削除時のリフレッシュのみの想定、それ以外の用途を追加する場合は注意）
            Dim prevObj As GBT00003RESULT = DirectCast(Page.PreviousPage, GBT00003RESULT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchType", Me.hdnSearchType},
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

            '正常メッセージをメッセージエリアに表示（削除正常時のみ自身をリフレッシュするため）
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
        End If
        Me.WF_LISTAREA.CssClass = Me.hdnSearchType.Value
    End Sub
    ''' <summary>
    ''' 固定値マスタよりラジオボタン選択肢を取得
    ''' </summary>
    Private Sub SetListViewTypeListItem()

        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        'リストクリア
        Me.rblListViewType.Items.Clear()
        Dim tmpListBoxObj As New ListBox
        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "ORDERLISTVIEWTYPE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = tmpListBoxObj
        Else
            COA0017FixValue.LISTBOX2 = tmpListBoxObj
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                tmpListBoxObj = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                tmpListBoxObj = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If
        Else
            Return
        End If

        For Each item As ListItem In tmpListBoxObj.Items
            Me.rblListViewType.Items.Add(item)
        Next
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget With {
                .MAPID = CONST_MAPID,
                .COMPCODE = "",
                .VARI = Me.hdnThisMapVariant.Value,
                .FIELD = "ORDERLISTVIEWTYPE"
            }
        COA0016VARIget.COA0016VARIget()
        If Me.rblListViewType.Items.FindByValue(COA0016VARIget.VALUE) IsNot Nothing Then
            Me.rblListViewType.SelectedValue = COA0016VARIget.VALUE
        End If
    End Sub
    ''' <summary>
    ''' 積載品マスタより情報を取得
    ''' </summary>
    ''' <param name="targetTextObject">国コードテキストボックス</param>
    ''' <param name="productCode">積載品コード</param>
    Private Sub SetDisplayProduct(targetTextObject As TextBox, productCode As String)
        '積載品の付帯情報を一旦クリア
        Me.lblProductText.Text = ""
        targetTextObject.Text = productCode.Trim

        '積載品コードが未入力の場合はDBアクセスせずに終了
        If productCode.Trim = "" Then
            Return
        End If

        Dim dt As DataTable = GBA00014Product.GBA00014getProductCodeValue(productCode.Trim, enabled:=CONST_FLAG_YES)
        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        '取得データを画面に展開
        Dim dr As DataRow = dt.Rows(0)
        Me.lblProductText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))

    End Sub
    ''' <summary>
    ''' 港名称を画面に設定
    ''' </summary>
    ''' <param name="targetTextObject">コード入力する対象のテキストボックス</param>
    ''' <param name="portCode">港コード</param>
    Private Sub SetDisplayPort(targetTextObject As TextBox, portCode As String)
        Dim targetLabel As Label = Nothing
        Select Case targetTextObject.ID
            Case Me.txtPol.ID
                targetLabel = Me.lblPolText
            Case Me.txtPod.ID
                targetLabel = Me.lblPodText
        End Select
        '一旦リセット
        targetTextObject.Text = portCode.Trim
        targetLabel.Text = ""
        '港コードが未入力の場合はDBアクセスせずに終了
        If portCode.Trim = "" Then
            Return
        End If

        Dim dt As DataTable = GBA00006PortRelated.GBA00006getPortCodeValue(portCode:=portCode.Trim)

        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)

        targetLabel.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
    End Sub
    ''' <summary>
    ''' 画面表示のテーブルを制御する
    ''' </summary>
    Private Sub DisplayListObjEdit()
        Dim targetPanel As Panel = Me.WF_LISTAREA

        '一覧表示データ復元
        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New COA0021ListTable
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
            Return
        End If
        'レコードを保持していない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        ''削除ボタンを消す対象のリストを取得
        'Dim qcannotDelTarget = From dtitem In dt Where Convert.ToString(dtitem("BRODFLG")) = "2" _
        '                                    AndAlso Convert.ToString(dtitem("CANDELETEORDER")) = "0"
        '                       Select Convert.ToString(dtitem("LINECNT"))
        'Dim cannotDelTarget As New List(Of String)
        'If qcannotDelTarget.Any Then
        '    cannotDelTarget = qcannotDelTarget.ToList
        'End If
        '削除ボタン使用不可のオーダーレコード、BR使用不可（新規オーダー作成不可）のリストを取得
        Dim qButtonEffectList = From dtitem In dt Where (Convert.ToString(dtitem("BRODFLG")) = "2" _
                                                        AndAlso Convert.ToString(dtitem("CANDELETEORDER")) = "0") _
                                                  OrElse (Convert.ToString(dtitem("BRODFLG")) = "1" _
                                                        AndAlso Convert.ToString(dtitem("DISABLED")).Equals(CONST_FLAG_YES))
                                Select New With {.LineCnt = Convert.ToString(dtitem("LINECNT")), .BrOdrFlg = Convert.ToString(dtitem("BRODFLG"))}

        If qButtonEffectList.Any = False Then
            '対象が無い場合は一覧制御なし
            Return
        End If

        Dim buttonEffectList = qButtonEffectList.ToDictionary(Function(x) x.LineCnt, Function(x) x)


        Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
        If rightDataDiv.HasControls = False _
           OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
           OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
            Return
        End If

        Dim rightHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HR"), Panel)
        Dim rightHeaderTable As Table = DirectCast(rightHeaderDiv.Controls(0), Table)

        Dim leftHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HL"), Panel)
        Dim leftHeaderTable As Table = DirectCast(leftHeaderDiv.Controls(0), Table)

        Dim rightDataTable As Table = DirectCast(rightDataDiv.Controls(0), Table)
        Dim leftDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DL"), Panel)
        Dim leftDataTable As Table = DirectCast(leftDataDiv.Controls(0), Table) '1列目LINECNT 、3列目のSHOW DELETEカラム取得用
        Dim dtNonBrCosts As New DataTable

        '******************************
        'レンダリング行のループ
        '******************************
        Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        Dim listActionButton As HtmlButton = Nothing
        For i = 0 To rowCnt
            Dim tbrRight As TableRow = rightDataTable.Rows(i)
            Dim tbrLeft As TableRow = leftDataTable.Rows(i)

            Dim lineCnt As String = tbrLeft.Cells(0).Text

            If buttonEffectList.ContainsKey(lineCnt) AndAlso buttonEffectList(lineCnt).BrOdrFlg = "2" Then
                'オーダー行の削除ボタン非表示
                tbrRight.Attributes.Add("data-cannotdelete", "1")
                tbrLeft.Attributes.Add("data-cannotdelete", "1")
            ElseIf buttonEffectList.ContainsKey(lineCnt) AndAlso buttonEffectList(lineCnt).BrOdrFlg = "1" Then
                'ブレーカー行の「★」（新規オーダー作成）ボタン使用不可
                listActionButton = DirectCast(tbrRight.FindControl("btnWF_LISTAREAACTION" & lineCnt), HtmlButton)
                If listActionButton Is Nothing Then
                    listActionButton = DirectCast(tbrLeft.FindControl("btnWF_LISTAREAACTION" & lineCnt), HtmlButton)
                End If
                '固定・可変いづれかにボタンオブジェクトが存在した場合使用不可にする
                If listActionButton IsNot Nothing Then
                    listActionButton.Disabled = True
                    listActionButton.Attributes.Add("Title", "Breaker has been disabled.")
                End If
                listActionButton = Nothing
            End If

        Next 'END ROWCOUNT
    End Sub
End Class