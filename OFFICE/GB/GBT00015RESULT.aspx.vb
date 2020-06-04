Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' COSTUP検索結果画面クラス
''' </summary>
Public Class GBT00015RESULT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00015" '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00004" '次画面一覧のMAPID
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
                'POL
                Case vLeftPOL.ID
                    Dim dt As DataTable = GetPort()
                    With Me.lbPOL
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtPOL.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
                    'POD
                Case vLeftPOD.ID
                    Dim dt As DataTable = GetPort()
                    With Me.lbPOD
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtPOD.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
                    'Shipper
                Case vLeftShipper.ID
                    Dim dt As DataTable = GetShipper()
                    With Me.lbShipper
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtShipper.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
                    'Consignee
                Case vLeftConsignee.ID
                    Dim dt As DataTable = GetConsignee()
                    With Me.lbConsignee
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtConsignee.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
                    'Product
                Case vLeftProduct.ID
                    Dim dt As DataTable = GetProduct()
                    With Me.lbProduct
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtProduct.Text)
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
        COA0011ReturnUrl.MAPID = CONST_MAPID & "R" ' "GBT00003R"
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
        If Me.txtOrder.Text.Trim <> "" OrElse Me.txtPOL.Text.Trim <> "" OrElse
                Me.txtPOD.Text.Trim <> "" OrElse Me.txtProduct.Text.Trim <> "" OrElse
            Me.txtShipper.Text.Trim <> "" OrElse Me.txtConsignee.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtOrder.Text.Trim = "" OrElse Convert.ToString(dr("ODID")).StartsWith(Me.txtOrder.Text.Trim)) _
                  AndAlso (Me.txtPOL.Text.Trim = "" OrElse (Me.lblPOLText.Text.Trim <> "" AndAlso Convert.ToString(dr("POL")).Trim.Equals(Me.lblPOLText.Text.Trim))) _
                  AndAlso (Me.txtPOD.Text.Trim = "" OrElse (Me.lblPODText.Text.Trim <> "" AndAlso Convert.ToString(dr("POD")).Trim.Equals(Me.lblPODText.Text.Trim))) _
                  AndAlso (Me.txtShipper.Text.Trim = "" OrElse (Me.lblShipperText.Text.Trim <> "" AndAlso Convert.ToString(dr("SHIPPER")).Trim.Equals(Me.lblShipperText.Text.Trim))) _
                  AndAlso (Me.txtConsignee.Text.Trim = "" OrElse (Me.lblConsigneeText.Text.Trim <> "" AndAlso Convert.ToString(dr("CONSIGNEE")).Trim.Equals(Me.lblConsigneeText.Text.Trim))) _
                  AndAlso (Me.txtProduct.Text.Trim = "" OrElse (Me.lblProductText.Text.Trim <> "" AndAlso Convert.ToString(dr("PRODUCTCODE")).Trim.Equals(Me.lblProductText.Text.Trim)))
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
        If Me.hdnETDStYMD.Value <> "" OrElse Me.hdnETAStYMD.Value <> "" Then
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
        sqlStat.AppendLine("      ,'-'         AS APPLOVAL") '一旦1固定
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETD1", String.Format(etdDatefield, "1"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETA1", String.Format(etaDatefield, "1"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETD2", String.Format(etdDatefield, "2"))
        sqlStat.AppendFormat("      ,ISNULL(CASE {0}   WHEN '1900/01/01' THEN '' ELSE FORMAT({0},'yyyy/MM/dd')   END,'') AS ETA2", String.Format(etaDatefield, "2"))
        'sqlStat.AppendLine("      ,OBS.SHIPPER AS SHIPPER")
        sqlStat.AppendFormat("      ,ISNULL(SP.{0},'') AS SHIPPER", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(PD.{0},'') AS PRODUCTCODE", textProductTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CN.{0},'') AS CONSIGNEE", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,ISNULL(POL.AREANAME,'')  AS POL")
        sqlStat.AppendLine("      ,ISNULL(POD.AREANAME,'')  AS POD")
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
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  PD.CUSTOMERCODE = BS.SHIPPER")
        'sqlStat.AppendLine("   AND  PD.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PD.PRODUCTCODE  = OBS.PRODUCTCODE")
        sqlStat.AppendLine("   AND  PD.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  PD.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD.ENABLED      = @ENABLED")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POL") 'POL名称用JOIN
        sqlStat.AppendLine("    ON  POL.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  POL.COUNTRYCODE  = OBS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  POL.PORTCODE     = OBS.LOADPORT1")
        sqlStat.AppendLine("   AND  POL.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  POL.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  POL.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT POD") 'POD名称用JOIN
        sqlStat.AppendLine("    ON  POD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  POD.COUNTRYCODE  = OBS.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   AND  POD.PORTCODE     = OBS.DISCHARGEPORT1")
        sqlStat.AppendLine("   AND  POD.STYMD       <= OBS.ENDYMD")
        sqlStat.AppendLine("   AND  POD.ENDYMD      >= OBS.STYMD")
        sqlStat.AppendLine("   AND  POD.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN (SELECT OVS.ORDERNO,COUNT(DISTINCT OVS.TANKSEQ) AS NOOFORDER  ") 'オーダー数用JOIN
        sqlStat.AppendLine("               FROM GBT0005_ODR_VALUE OVS")
        sqlStat.AppendLine("              WHERE OVS.DELFLG <> @DELFLG")
        sqlStat.AppendLine("              GROUP BY OVS.ORDERNO) OVSCNT")
        sqlStat.AppendLine("    ON  OVSCNT.ORDERNO     = OBS.ORDERNO")
        sqlStat.AppendLine(" WHERE OBS.DELFLG        <> @DELFLG")
        sqlStat.AppendLine("   AND ( 1=1")
        If sqlEtdEtaOrderCondition.Length > 0 Then
            sqlStat.AppendLine(sqlEtdEtaOrderCondition.ToString)
        End If
        sqlStat.AppendLine("   OR OBS.BRTYPE = @LEASEBRTYPE)")
        If Me.hdnShipper.Value <> "" Then
            sqlStat.AppendLine(" And OBS.SHIPPER = @SHIPPER")
        End If
        If Me.hdnConsignee.Value <> "" Then
            sqlStat.AppendLine("   And OBS.CONSIGNEE     = @CONSIGNEE")
        End If
        If Me.hdnPortOfLoading.Value <> "" Then
            sqlStat.AppendLine("   And (   OBS.LOADPORT1     = @POL")
            sqlStat.AppendLine("        Or OBS.LOADPORT2     = @POL")
            sqlStat.AppendLine("       )")
        End If
        If Me.hdnPortOfDischarge.Value <> "" Then
            sqlStat.AppendLine("   And (   OBS.DISCHARGEPORT1 = @POD")
            sqlStat.AppendLine("        Or OBS.DISCHARGEPORT1 = @POD")
            sqlStat.AppendLine("       )")
        End If
        If Me.hdnOffice.Value <> "" Then
            'OFFICE
            sqlStat.AppendLine("   And (    OBS.AGENTORGANIZER = @OFFICECODE")
            sqlStat.AppendLine("         Or OBS.AGENTPOL1      = @OFFICECODE")
            sqlStat.AppendLine("         Or OBS.AGENTPOL2      = @OFFICECODE")
            sqlStat.AppendLine("         Or OBS.AGENTPOD1      = @OFFICECODE")
            sqlStat.AppendLine("         Or OBS.AGENTPOD2      = @OFFICECODE")
            sqlStat.AppendLine("       )")
        End If
        sqlStat.AppendLine(")")
        '共通関数は単一テーブル想定のため全体をサブクエリー化 
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        sqlStat.AppendLine("FROM (")
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
                .Add("LEASEBRTYPE", SqlDbType.NVarChar).Value = C_BRTYPE.LEASE
                If Me.hdnETDStYMD.Value <> "" Then
                    .Add("@ETDST", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETDStYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    If Me.hdnETDEndYMD.Value <> "" Then
                        .Add("@ETDEND", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETDEndYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    Else
                        .Add("@ETDEND", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETDStYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    End If
                End If
                If Me.hdnETAStYMD.Value <> "" Then
                    .Add("@ETAST", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETAStYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    If Me.hdnETAEndYMD.Value <> "" Then
                        .Add("@ETAEND", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETAEndYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    Else
                        .Add("@ETAEND", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETAStYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    End If
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
                Case vLeftPOL.ID
                    'POL選択時
                    Me.lblPOLText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbPOL.SelectedItem IsNot Nothing Then
                        Dim polCode As String = Me.lbPOL.SelectedItem.Value
                        Dim dt As DataTable = GetPort(polCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblPOLText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
                        End If
                    End If
                Case vLeftPOD.ID
                    'POD選択時
                    Me.lblPODText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbPOD.SelectedItem IsNot Nothing Then
                        Dim podCode As String = Me.lbPOD.SelectedItem.Value
                        Dim dt As DataTable = GetPort(podCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblPODText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
                        End If
                    End If
                Case vLeftShipper.ID
                    'Shipper選択時
                    Me.lblShipperText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbShipper.SelectedItem IsNot Nothing Then
                        Dim shipperCode As String = Me.lbShipper.SelectedItem.Value
                        Dim dt As DataTable = GetShipper(shipperCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblShipperText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
                        End If
                    End If
                Case vLeftConsignee.ID
                    'Consignee選択時
                    Me.lblConsigneeText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbConsignee.SelectedItem IsNot Nothing Then
                        Dim consigneeCode As String = Me.lbConsignee.SelectedItem.Value
                        Dim dt As DataTable = GetConsignee(consigneeCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblConsigneeText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
                        End If
                    End If
                Case vLeftProduct.ID
                    'PRODUCT選択時
                    Me.lblProductText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbProduct.SelectedItem IsNot Nothing Then
                        Dim productCode As String = Me.lbProduct.SelectedItem.Value
                        Dim dt As DataTable = GetProduct(productCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblProductText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
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
    '''' <summary>
    '''' 一覧表DELETEボタン押下時処理
    '''' </summary>
    'Public Sub btnListDelete_Click()
    '    Dim currentRownum As String = Me.hdnListCurrentRownum.Value
    '    Dim rowId As Integer = 0
    '    If Integer.TryParse(currentRownum, rowId) = True Then
    '        rowId = rowId - 1
    '    Else
    '        Return
    '    End If

    '    Dim dt As DataTable = CreateDataTable()
    '    Dim COA0021ListTable As New BASEDLL.COA0021ListTable
    '    Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
    '    '一覧表示データ復元 
    '    COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
    '    COA0021ListTable.TBLDATA = dt
    '    COA0021ListTable.COA0021recoverListTable()
    '    If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
    '        dt = COA0021ListTable.OUTTBL
    '    Else
    '        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
    '        Return
    '    End If
    '    'この段階でありえないが初期検索結果がない場合は終了
    '    If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
    '        Return
    '    End If
    '    '引き渡す情報を当画面のHidden項目に格納
    '    Dim selectedRow As DataRow = dt.Rows(rowId)
    '    'SQL接続生成
    '    Using sqlCon As New SqlConnection(Convert.ToString(HttpContext.Current.Session("DBcon")))
    '        sqlCon.Open()
    '        '論理削除可能かチェック
    '        If CheckCanDelete(selectedRow, sqlCon) = False Then
    '            CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, Me.lblFooterMessage)
    '            Return
    '        End If
    '        '各オーダーテーブルの論理削除
    '        Dim orderNo As String = Convert.ToString(selectedRow.Item("ODID"))
    '        DeleteOrderBaseValue(orderNo, sqlCon)
    '    End Using
    '    'ここまで来たら論理削除正常終了のため自身を再読み込みし一覧を再取得
    '    Server.Transfer(Request.Url.LocalPath)
    'End Sub
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

        AddLangSetting(dicDisplayText, Me.lblOrder, "オーダーID", "Order ID")
        AddLangSetting(dicDisplayText, Me.lblPOL, "POL", "POL")
        AddLangSetting(dicDisplayText, Me.lblPOD, "POD", "POD")
        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主", "Shipper")
        AddLangSetting(dicDisplayText, Me.lblConsignee, "荷受人", "Consignee")
        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品", "Product")

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
        If TypeOf Page.PreviousPage Is GBT00015SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00015SELECT = DirectCast(Page.PreviousPage, GBT00015SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtETDStYMD", Me.hdnETDStYMD},
                                                                        {"txtETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"txtETAStYMD", Me.hdnETAStYMD},
                                                                        {"txtETAEndYMD", Me.hdnETAEndYMD},
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

        ElseIf TypeOf Page.PreviousPage Is GBT00015RESULT Then
            '自身から遷移（削除時のリフレッシュのみの想定、それ以外の用途を追加する場合は注意）
            Dim prevObj As GBT00015RESULT = DirectCast(Page.PreviousPage, GBT00015RESULT)
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
    ''' POL一覧を取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetPort(Optional portCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT PORTCODE AS CODE")
        sqlStat.AppendLine("      ,AREANAME AS NAME")
        sqlStat.AppendLine("      ,PORTCODE + ':' + AREANAME AS LISTBOXNAME")
        sqlStat.AppendLine("  FROM GBM0002_PORT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        'sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If portCode <> "" Then
            sqlStat.AppendLine("   AND PORTCODE    = @PORTCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY PORTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            'Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            Dim paramPortCode As SqlParameter = Nothing
            If portCode <> "" Then
                paramPortCode = sqlCmd.Parameters.Add("@PORTCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            'paramCountryCode.Value = countryCode
            If portCode <> "" Then
                paramPortCode.Value = portCode
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt

    End Function
    ''' <summary>
    ''' 荷主一覧取得
    ''' </summary>
    ''' <param name="customerCode">顧客コード(オプショナル)未指定時は国コードで絞りこんだ全件</param>
    ''' <returns>荷主一覧データテーブル</returns>
    ''' <remarks>GBM0004_CUSTOMERより荷主一覧を取得</remarks>
    Private Function GetShipper(Optional customerCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CUSTOMERCODE AS CODE")
        sqlStat.AppendFormat("      ,{0} AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,CUSTOMERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        'sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If customerCode <> "" Then
            sqlStat.AppendLine("   AND CUSTOMERCODE    = @CUSTOMERCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("ORDER BY CUSTOMERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            'Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            Dim paramCustomerCode As SqlParameter = Nothing
            If customerCode <> "" Then
                paramCustomerCode = sqlCmd.Parameters.Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            'paramCountryCode.Value = countryCode
            If customerCode <> "" Then
                paramCustomerCode.Value = customerCode
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 荷受人一覧取得
    ''' </summary>
    ''' <param name="customerCode">顧客コード</param>
    ''' <returns>荷受人一覧データテーブル</returns>
    ''' <remarks>GBM0004_CUSTOMERより荷受人情報を取得</remarks>
    Private Function GetConsignee(Optional customerCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("Select CUSTOMERCODE AS CODE")
        sqlStat.AppendFormat("      , {0} As NAME", textField).AppendLine()
        sqlStat.AppendFormat("      , CUSTOMERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        'sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        If customerCode <> "" Then
            sqlStat.AppendLine("   AND CUSTOMERCODE    = @CUSTOMERCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("ORDER BY CUSTOMERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            'Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            Dim paramCustomerCode As SqlParameter = Nothing
            If customerCode <> "" Then
                paramCustomerCode = sqlCmd.Parameters.Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            'paramCountryCode.Value = countryCode
            If customerCode <> "" Then
                paramCustomerCode.Value = customerCode
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 積載品検索
    ''' </summary>
    ''' <param name="productCode">積載品コード（省略時は全件）</param>
    ''' <returns></returns>
    Private Function GetProduct(Optional productCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "PRODUCTNAME"

        'If COA0019Session.LANGDISP <> C_LANG.JA Then
        '    textField = "NAMES"
        'End If
        'SQL文作成(TODO:ORGもキーだが今のところ未設定)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT rtrim(PRODUCTCODE) AS CODE")
        sqlStat.AppendFormat("      ,rtrim({0}) AS NAME", textField).AppendLine()
        sqlStat.AppendFormat("      ,rtrim(PRODUCTCODE) + ':' + rtrim({0})  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("      ,rtrim(IMDGCODE) AS IMDGCODE")
        sqlStat.AppendLine("      ,rtrim(UNNO) AS UNNO")
        sqlStat.AppendLine("      ,rtrim(GRAVITY) AS GRAVITY")
        sqlStat.AppendLine("      ,rtrim(HAZARDCLASS) AS HAZARDCLASS")
        sqlStat.AppendLine("  FROM GBM0008_PRODUCT")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        'sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        'sqlStat.AppendLine("   AND CUSTOMERCODE = @CUSTOMERCODE")
        If productCode <> "" Then
            sqlStat.AppendLine("   AND PRODUCTCODE    = @PRODUCTCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND ENABLED      = @ENABLED")
        sqlStat.AppendLine("ORDER BY PRODUCTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            'Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            'Dim paramCustomerCode As SqlParameter = sqlCmd.Parameters.Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20)
            Dim paramProductCodeCode As SqlParameter = Nothing
            If productCode <> "" Then
                paramProductCodeCode = sqlCmd.Parameters.Add("@PRODUCTCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd As SqlParameter = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd As SqlParameter = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            Dim paramEnabled As SqlParameter = sqlCmd.Parameters.Add("@ENABLED", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            'paramCountryCode.Value = countryCode
            'paramCustomerCode.Value = shipperCode
            If productCode <> "" Then
                paramProductCodeCode.Value = productCode
            End If
            paramStYmd.Value = Date.Now
            paramEndYmd.Value = Date.Now
            paramDelFlg.Value = CONST_FLAG_YES
            paramEnabled.Value = CONST_FLAG_YES
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function

    ''' <summary>
    ''' [絞り込み条件]POLコード変更時イベント
    ''' </summary>
    Public Sub txtPOL_Change()
        Dim pol As String = Me.txtPOL.Text.Trim
        Me.lblPOLText.Text = ""
        If pol = "" Then
            Return
        End If

        Dim dt As DataTable = GetPort(pol)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Me.txtPOL.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblPOLText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
        End If
    End Sub

    ''' <summary>
    ''' [絞り込み条件]PODコード変更時イベント
    ''' </summary>
    Public Sub txtPOD_Change()
        Dim pod As String = Me.txtPOD.Text.Trim
        Me.lblPODText.Text = ""
        If pod = "" Then
            Return
        End If

        Dim dt As DataTable = GetPort(pod)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Me.txtPOD.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblPODText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
        End If
    End Sub

    ''' <summary>
    ''' [絞り込み条件]Shipperコード変更時イベント
    ''' </summary>
    Public Sub txtShipper_Change()
        Dim shipper As String = Me.txtShipper.Text.Trim
        Me.lblShipperText.Text = ""
        If shipper = "" Then
            Return
        End If

        Dim dt As DataTable = GetShipper(shipper)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Me.txtShipper.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblShipperText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
        End If
    End Sub

    ''' <summary>
    ''' [絞り込み条件]Consigneeコード変更時イベント
    ''' </summary>
    Public Sub txtConsignee_Change()
        Dim consignee As String = Me.txtConsignee.Text.Trim
        Me.lblConsigneeText.Text = ""
        If consignee = "" Then
            Return
        End If

        Dim dt As DataTable = GetConsignee(consignee)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Me.txtConsignee.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblConsigneeText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
        End If
    End Sub

    ''' <summary>
    ''' [絞り込み条件]Productコード変更時イベント
    ''' </summary>
    Public Sub txtProduct_Change()
        Dim product As String = Me.txtProduct.Text.Trim
        Me.lblProductText.Text = ""
        If product = "" Then
            Return
        End If

        Dim dt As DataTable = GetProduct(product)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Me.txtProduct.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblProductText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
        End If
    End Sub
End Class