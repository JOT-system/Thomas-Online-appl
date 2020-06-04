Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' TANK履歴検索結果画面クラス
''' </summary>
Public Class GBT00021RESULT
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00021R" '自身のMAPID
    'Private Const CONST_BASEID As String = "GBT00021R" '次画面一覧のMAPID
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
                '国一覧生成
                '****************************************
                SetCountryListItem("")
                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Dim retDt As New DataTable
                If Me.hdnSearchDataType.Value = "01THOMAS" Then
                    retDt = Me.GetOrderListDataTable()
                ElseIf Me.hdnSearchDataType.Value = "02TACOS" Then
                    retDt = Me.GetTACOSOrderListDataTable()
                End If

                Using dt As DataTable = retDt
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
                'If Me.hdnListDBclick.Value <> "" Then
                '    ListRowDbClick()
                '    Me.hdnListDBclick.Value = ""
                '    'Return '単票ページにリダイレクトするため念のため処理は終わらせる
                'End If
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
                'ACTY
                Case vLeftActy.ID
                    Dim dt As DataTable = GetActy()
                    With Me.lbActy
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtACTY.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With

                'タンク番号
                Case vLeftTankNo.ID
                    Dim dt As List(Of String) = GetTankNo()
                    With Me.lbTankNo
                        .DataSource = dt
                        '.DataTextField = "TANKNO"
                        '.DataValueField = "TANKNO"
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

                '国コード
                Case vLeftCountry.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCountry.Items.Count > 0 Then
                            Dim findResult As ListItem = Me.lbCountry.Items.FindByValue(txtobj.Text)
                            If findResult IsNot Nothing Then
                                findResult.Selected = True
                            End If
                        End If

                        Me.mvLeft.Focus()
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
        If Me.txtACTY.Text.Trim <> "" OrElse Me.txtTankNo.Text.Trim <> "" OrElse
                Me.txtCountry.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtTankNo.Text.Trim = "" OrElse Convert.ToString(dr("TANKNO")).Equals(Me.txtTankNo.Text.Trim)) _
                   AndAlso (Me.txtCountry.Text = "" OrElse Convert.ToString(dr("CURRENTCOUNTRY")).Equals(Me.txtCountry.Text))) Then
                    dr.Item("HIDDEN") = 1
                End If

                'ACTY
                If dr.Item("HIDDEN").ToString = "0" AndAlso Me.txtACTY.Text.Contains(",") Then

                    Dim splActy As String()
                    splActy = Split(Me.txtACTY.Text, ",")

                    For Each act As String In splActy
                        If Convert.ToString(dr("ACTIONID")).Equals(act) Then
                            dr.Item("HIDDEN") = 0
                            Exit For
                        Else
                            dr.Item("HIDDEN") = 1
                        End If
                    Next
                Else
                    If Not (Me.txtACTY.Text = "" OrElse Convert.ToString(dr("ACTIONID")).Equals(Me.txtACTY.Text)) Then
                        dr.Item("HIDDEN") = 1
                    End If
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
            Dim reportId As String = lbRightList.SelectedValue.ToString
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

        'ソート順取得
        COA0020ProfViewSort.MAPID = mapId
        COA0020ProfViewSort.VARI = Me.hdnReportVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.PREFIX = "OV"
        COA0020ProfViewSort.COA0020getProfViewSort()

        'オーダー(当明細が含まれるブレーカーも対象（削除除く）)
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(OB.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,ISNULL(OV.TANKNO,'')  AS TANKNO")
        sqlStat.AppendLine("      ,ISNULL(OV.ORDERNO,'') AS ORDERNO")
        sqlStat.AppendLine("      ,ISNULL(OB.BRID,'')    AS BRID")
        sqlStat.AppendLine("      ,ISNULL(OV.ACTIONID,'')  AS ACTIONID")
        sqlStat.AppendLine("      ,ISNULL(OV.DISPSEQ,'')  AS DISPSEQ")
        sqlStat.AppendLine("      ,ISNULL(OV.ACTUALDATE,'')  AS ACTUALDATE")
        sqlStat.AppendLine("      ,CASE WHEN OV.DTLPOLPOD LIKE 'POL1' THEN ISNULL(OB.RECIEPTPORT1,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POL2' THEN ISNULL(OB.RECIEPTPORT2,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD1' THEN ISNULL(OB.DISCHARGEPORT1,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD2' THEN ISNULL(OB.DISCHARGEPORT1,'') ")
        sqlStat.AppendLine("       END AS CURRENTPORTCODE")
        sqlStat.AppendLine("      ,CASE WHEN OV.DTLPOLPOD LIKE 'POL1' THEN ISNULL(PTL.AREANAME,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POL2' THEN ISNULL(PTL2.AREANAME,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD1' THEN ISNULL(PTD.AREANAME,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD2' THEN ISNULL(PTD2.AREANAME,'') ")
        sqlStat.AppendLine("       END AS CURRENTPORT")
        sqlStat.AppendLine("      ,CASE WHEN OV.DTLPOLPOD LIKE 'POL1' THEN ISNULL(OB.LOADCOUNTRY1,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POL2' THEN ISNULL(OB.LOADCOUNTRY2,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD1' THEN ISNULL(OB.DISCHARGECOUNTRY1,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD2' THEN ISNULL(OB.DISCHARGECOUNTRY2,'') ")
        sqlStat.AppendLine("       END AS CURRENTCOUNTRY")
        sqlStat.AppendLine("      ,CASE WHEN OV.DTLPOLPOD LIKE 'POL1' THEN ISNULL(CTL.NAMES,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POL2' THEN ISNULL(CTL2.NAMES,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD1' THEN ISNULL(CTD.NAMES,'') ")
        sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD2' THEN ISNULL(CTD2.NAMES,'') ")
        sqlStat.AppendLine("       END AS CURRENTCOUNTRYNAME")
        sqlStat.AppendLine("      ,ISNULL(OV.DTLPOLPOD,'')  AS DTLPOLPOD")
        sqlStat.AppendLine("      ,ISNULL(OB.LOADCOUNTRY1,'')  AS POLCOUNTRYCODE")
        sqlStat.AppendLine("      ,ISNULL(CTL.NAMES,'')  AS POLCOUNTRY")
        sqlStat.AppendLine("      ,ISNULL(OB.RECIEPTPORT1,'')  AS POLCODE")
        sqlStat.AppendLine("      ,ISNULL(PTL.AREANAME,'')  AS POL")
        sqlStat.AppendLine("      ,ISNULL(OB.LOADCOUNTRY2,'')  AS POLCOUNTRYCODE2")
        sqlStat.AppendLine("      ,ISNULL(CTL2.NAMES,'')  AS POLCOUNTRY2")
        sqlStat.AppendLine("      ,ISNULL(OB.RECIEPTPORT2,'')  AS POLCODE2")
        sqlStat.AppendLine("      ,ISNULL(PTL2.AREANAME,'')  AS POL2")
        sqlStat.AppendLine("      ,ISNULL(OB.DISCHARGECOUNTRY1,'')  AS PODCOUNTRYCODE")
        sqlStat.AppendLine("      ,ISNULL(CTD.NAMES,'')  AS PODCOUNTRY")
        sqlStat.AppendLine("      ,ISNULL(OB.DISCHARGEPORT1,'')  AS PODCODE")
        sqlStat.AppendLine("      ,ISNULL(PTD.AREANAME,'')  AS POD")
        sqlStat.AppendLine("      ,ISNULL(OB.DISCHARGECOUNTRY2,'')  AS PODCOUNTRYCODE2")
        sqlStat.AppendLine("      ,ISNULL(CTD2.NAMES,'')  AS PODCOUNTRY2")
        sqlStat.AppendLine("      ,ISNULL(OB.DISCHARGEPORT2,'')  AS PODCODE2")
        sqlStat.AppendLine("      ,ISNULL(PTD2.AREANAME,'')  AS POD2")
        sqlStat.AppendLine("      ,''  AS DEPOT")
        sqlStat.AppendLine("      ,ISNULL(OB.BRTYPE,'')  AS BRTYPE")
        sqlStat.AppendLine("      ,ISNULL(OB.TERMTYPE,'')  AS TERMTYPE")
        sqlStat.AppendLine("      ,ISNULL(OB.SHIPPER,'')  AS SHIPPER")
        sqlStat.AppendLine("      ,ISNULL(CMS.NAMESEN,ISNULL(TMS.NAMES,''))  AS NAMESEN_CMS")
        sqlStat.AppendLine("      ,ISNULL(CMS.NAMELEN,ISNULL(TMS.NAMEL,''))  AS NAMELEN_CMS")
        sqlStat.AppendLine("      ,ISNULL(OB.CONSIGNEE,'')  AS CONSIGNEE")
        sqlStat.AppendLine("      ,ISNULL(CMC.NAMESEN,ISNULL(TMC.NAMES,''))  AS NAMESEN_CMC")
        sqlStat.AppendLine("      ,ISNULL(CMC.NAMELEN,ISNULL(TMC.NAMEL,''))  AS NAMELEN_CMC")
        sqlStat.AppendLine("      ,ISNULL(OB.PRODUCTCODE,'')  AS PRODUCTCODE")
        sqlStat.AppendLine("      ,ISNULL(PD.PRODUCTNAME,'')  AS PRODUCT")
        sqlStat.AppendLine("      ,ISNULL(OB.INVOICEDBY,'')  AS INVOICEDBY")
        sqlStat.AppendLine("      ,ISNULL(TMI.NAMES,'')  AS NAMES_TMI")
        sqlStat.AppendLine("      ,ISNULL(TMI.NAMEL,'')  AS NAMEL_TMI")
        sqlStat.AppendLine("      ,ISNULL(OB.LOADING,'')  AS LOADING")
        sqlStat.AppendLine("      ,ISNULL(OB.STEAMING,'')  AS STEAMING")
        sqlStat.AppendLine("      ,ISNULL(OB.TIP,'')  AS TIP")
        sqlStat.AppendLine("      ,ISNULL(OB.EXTRA,'')  AS EXTRA")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE OV")
        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OB") 'オーダー基本JOIN
        sqlStat.AppendLine("    ON  OB.ORDERNO   = OV.ORDERNO")
        sqlStat.AppendLine("   AND  OB.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PD.PRODUCTCODE  = OB.PRODUCTCODE")
        'sqlStat.AppendLine("   AND  PD.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  PD.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  PD.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PD.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD.ENABLED      = @ENABLED")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTL") 'POL名称用JOIN
        sqlStat.AppendLine("    ON  PTL.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PTL.COUNTRYCODE  = OB.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PTL.PORTCODE     = OB.LOADPORT1")
        'sqlStat.AppendLine("   AND  PTL.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  PTL.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  PTL.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PTL.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PTL.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTD") 'POD名称用JOIN
        sqlStat.AppendLine("    ON  PTD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PTD.COUNTRYCODE  = OB.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   AND  PTD.PORTCODE     = OB.DISCHARGEPORT1")
        'sqlStat.AppendLine("   AND  PTD.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  PTD.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  PTD.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PTD.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PTD.DELFLG       <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CTL") '発国名称用JOIN
        sqlStat.AppendLine("    ON  CTL.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CTL.COUNTRYCODE  = OB.LOADCOUNTRY1")
        'sqlStat.AppendLine("   AND  CTL.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  CTL.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  CTL.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CTL.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CTL.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CTD") '着国名称用JOIN
        sqlStat.AppendLine("    ON  CTD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CTD.COUNTRYCODE  = OB.DISCHARGECOUNTRY1")
        'sqlStat.AppendLine("   AND  CTD.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  CTD.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  CTD.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CTD.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CTD.DELFLG       <> @DELFLG")
        '第２輸送
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTL2") 'POL名称用JOIN
        sqlStat.AppendLine("    ON  PTL2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PTL2.COUNTRYCODE  = OB.LOADCOUNTRY2")
        sqlStat.AppendLine("   AND  PTL2.PORTCODE     = OB.LOADPORT2")
        'sqlStat.AppendLine("   AND  PTL2.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  PTL2.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  PTL2.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PTL2.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PTL2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTD2") 'POD名称用JOIN
        sqlStat.AppendLine("    ON  PTD2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PTD2.COUNTRYCODE  = OB.DISCHARGECOUNTRY2")
        sqlStat.AppendLine("   AND  PTD2.PORTCODE     = OB.DISCHARGEPORT2")
        'sqlStat.AppendLine("   AND  PTD2.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  PTD2.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  PTD2.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PTD2.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  PTD2.DELFLG       <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CTL2") '発国名称用JOIN
        sqlStat.AppendLine("    ON  CTL2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CTL2.COUNTRYCODE  = OB.LOADCOUNTRY2")
        'sqlStat.AppendLine("   AND  CTL2.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  CTL2.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  CTL2.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CTL2.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CTL2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CTD2") '着国名称用JOIN
        sqlStat.AppendLine("    ON  CTD2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CTD2.COUNTRYCODE  = OB.DISCHARGECOUNTRY2")
        'sqlStat.AppendLine("   AND  CTD2.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  CTD2.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  CTD2.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CTD2.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CTD2.DELFLG       <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CMS") '荷主名称用JOIN
        sqlStat.AppendLine("    ON  CMS.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CMS.COUNTRYCODE  = OB.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  CMS.CUSTOMERCODE = OB.SHIPPER")
        'sqlStat.AppendLine("   AND  CMS.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  CMS.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  CMS.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CMS.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CMS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CMC") '荷受名称用JOIN
        sqlStat.AppendLine("    ON  CMC.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CMC.COUNTRYCODE  = OB.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   AND  CMC.CUSTOMERCODE = OB.CONSIGNEE")
        'sqlStat.AppendLine("   AND  CMC.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  CMC.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  CMC.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CMC.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  CMC.DELFLG      <> @DELFLG")
        'sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CMI") 'INVOICEDBY名称用JOIN
        'sqlStat.AppendLine("    ON  CMI.COMPCODE     = @COMPCODE")
        ''sqlStat.AppendLine("   AND  CMI.COUNTRYCODE  = OB.LOADCOUNTRY1")
        'sqlStat.AppendLine("   AND  CMI.CUSTOMERCODE = OB.INVOICEDBY")
        'sqlStat.AppendLine("   AND  CMI.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  CMI.ENDYMD      >= OB.STYMD")
        'sqlStat.AppendLine("   AND  CMI.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TMS") '荷主名称用JOIN
        sqlStat.AppendLine("    ON  TMS.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  TMS.COUNTRYCODE  = OB.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  TMS.CARRIERCODE = OB.SHIPPER")
        'sqlStat.AppendLine("   AND  TMS.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  TMS.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  TMS.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  TMS.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  TMS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TMC") '荷受名称用JOIN
        sqlStat.AppendLine("    ON  TMC.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  TMC.COUNTRYCODE  = OB.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   AND  TMC.CARRIERCODE = OB.CONSIGNEE")
        'sqlStat.AppendLine("   AND  TMC.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  TMC.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  TMC.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  TMC.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  TMC.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TMI") 'InvoicedBy名称用JOIN
        sqlStat.AppendLine("    ON  TMI.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  TMI.COUNTRYCODE  = OB.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  TMI.CARRIERCODE = OB.INVOICEDBY")
        'sqlStat.AppendLine("   AND  TMI.STYMD       <= OB.ENDYMD")
        'sqlStat.AppendLine("   AND  TMI.ENDYMD      >= OB.STYMD")
        sqlStat.AppendLine("   AND  TMI.STYMD       <= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  TMI.ENDYMD      >= OV.ACTUALDATE")
        sqlStat.AppendLine("   AND  TMI.DELFLG      <> @DELFLG")

        sqlStat.AppendLine(" WHERE  OV.DELFLG        <> @DELFLG")
        sqlStat.AppendLine("   AND  convert(nvarchar, OV.ACTUALDATE , 111) <> '1900-01-01'")
        sqlStat.AppendLine("   AND  OV.ACTIONID      <> ''")

        sqlStat.AppendLine("   AND ( 1=1")
        If Me.hdnTankNo.Value <> "" Then
            sqlStat.AppendLine(" And OV.TANKNO = @TANKNO")
        End If
        If Me.hdnActy.Value <> "" Then
            sqlStat.AppendLine("   And OV.ACTIONID = @ACTIONID")
        End If
        If Me.hdnCountry.Value <> "" Then
            sqlStat.AppendLine("   And CASE WHEN OV.DTLPOLPOD LIKE 'POL1' THEN OB.LOADCOUNTRY1 ")
            sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD1' THEN OB.DISCHARGECOUNTRY1 ")
            sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POL2' THEN OB.LOADCOUNTRY2 ")
            sqlStat.AppendLine("            WHEN OV.DTLPOLPOD LIKE 'POD2' THEN OB.DISCHARGECOUNTRY2 ")
            sqlStat.AppendLine("       END = @COUNTRY")
        End If

        If Me.hdnStActualDate.Value <> "" AndAlso Me.hdnEndActualDate.Value <> "" Then
            sqlStat.AppendLine("   AND (convert(nvarchar, OV.ACTUALDATE , 111)  BETWEEN  @STACTUALDATE  AND  @ENDACTUALDATE )")
        End If

        sqlStat.AppendLine(")")
        '共通関数は単一テーブル想定のため全体をサブクエリー化 
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                If Me.hdnTankNo.Value <> "" Then
                    .Add("@TANKNO", SqlDbType.NVarChar).Value = Me.hdnTankNo.Value
                End If
                If Me.hdnActy.Value <> "" Then
                    .Add("@ACTIONID", SqlDbType.NVarChar).Value = Me.hdnActy.Value
                End If
                If Me.hdnCountry.Value <> "" Then
                    .Add("@COUNTRY", SqlDbType.NVarChar).Value = Me.hdnCountry.Value
                End If
                If Me.hdnStActualDate.Value <> "" AndAlso Me.hdnEndActualDate.Value <> "" Then
                    .Add("@STACTUALDATE", SqlDbType.Date).Value = FormatDateYMD(Me.hdnStActualDate.Value, GBA00003UserSetting.DATEFORMAT)
                    .Add("@ENDACTUALDATE", SqlDbType.Date).Value = FormatDateYMD(Me.hdnEndActualDate.Value, GBA00003UserSetting.DATEFORMAT)
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
    ''' TACOSオーダー一覧より値取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>要UNION オーダー</remarks>
    Private Function GetTACOSOrderListDataTable() As DataTable
        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        Dim retDt As New DataTable
        Dim sqlStat As New StringBuilder

        'オーダー(当明細が含まれるブレーカーも対象（削除除く）)
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY t.f_CreateDate) As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,'' AS TIMSTP")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,ISNULL(t.f_TankNo,'')  AS TANKNO")
        sqlStat.AppendLine("      ,ISNULL(t.f_JobNo,'') AS ORDERNO")
        sqlStat.AppendLine("      ,''  AS BRID")
        sqlStat.AppendLine("      ,ISNULL(t.f_Acty,'')  AS ACTIONID")
        sqlStat.AppendLine("      ,''  AS DISPSEQ")
        sqlStat.AppendLine("      ,ISNULL(t.f_Date,'')  AS ACTUALDATE")
        sqlStat.AppendLine("      ,ISNULL(p.f_PortCode,'') AS CURRENTPORTCODE")
        sqlStat.AppendLine("      ,ISNULL(p.f_AreaName,'') AS CURRENTPORT")
        sqlStat.AppendLine("      ,ISNULL(p.f_CountryCode,'') AS CURRENTCOUNTRY")
        sqlStat.AppendLine("      ,ISNULL(p.f_CountryName,'') AS CURRENTCOUNTRYNAME")
        sqlStat.AppendLine("      ,''  AS DTLPOLPOD")
        sqlStat.AppendLine("      ,''  AS POLCOUNTRYCODE")
        sqlStat.AppendLine("      ,''  AS POLCOUNTRY")
        sqlStat.AppendLine("      ,''  AS POLCODE")
        sqlStat.AppendLine("      ,''  AS POL")
        sqlStat.AppendLine("      ,''  AS POLCOUNTRYCODE2")
        sqlStat.AppendLine("      ,''  AS POLCOUNTRY2")
        sqlStat.AppendLine("      ,''  AS POLCODE2")
        sqlStat.AppendLine("      ,''  AS POL2")
        sqlStat.AppendLine("      ,''  AS PODCOUNTRYCODE")
        sqlStat.AppendLine("      ,''  AS PODCOUNTRY")
        sqlStat.AppendLine("      ,''  AS PODCODE")
        sqlStat.AppendLine("      ,''  AS POD")
        sqlStat.AppendLine("      ,''  AS PODCOUNTRYCODE2")
        sqlStat.AppendLine("      ,''  AS PODCOUNTRY2")
        sqlStat.AppendLine("      ,''  AS PODCODE2")
        sqlStat.AppendLine("      ,''  AS POD2")
        sqlStat.AppendLine("      ,ISNULL(t.f_CurrentDepotName,'') AS DEPOT")
        sqlStat.AppendLine("      ,''  AS BRTYPE")
        sqlStat.AppendLine("      ,''  AS TERMTYPE")
        sqlStat.AppendLine("      ,''  AS SHIPPER")
        sqlStat.AppendLine("      ,''  AS NAMESEN_CMS")
        sqlStat.AppendLine("      ,''  AS NAMELEN_CMS")
        sqlStat.AppendLine("      ,''  AS CONSIGNEE")
        sqlStat.AppendLine("      ,''  AS NAMESEN_CMC")
        sqlStat.AppendLine("      ,''  AS NAMELEN_CMC")
        sqlStat.AppendLine("      ,''  AS PRODUCTCODE")
        sqlStat.AppendLine("      ,ISNULL(t.f_Product,'')  AS PRODUCT")
        sqlStat.AppendLine("      ,''  AS INVOICEDBY")
        sqlStat.AppendLine("      ,''  AS NAMES_TMI")
        sqlStat.AppendLine("      ,''  AS NAMEL_TMI")
        sqlStat.AppendLine("      ,''  AS LOADING")
        sqlStat.AppendLine("      ,''  AS STEAMING")
        sqlStat.AppendLine("      ,''  AS TIP")
        sqlStat.AppendLine("      ,''  AS EXTRA")
        sqlStat.AppendLine("  FROM GBO0001_TACOS_TANKHISTORY t")
        sqlStat.AppendLine("  INNER JOIN GBO0002_TACOS_PORTCPDE p")
        sqlStat.AppendLine("    ON  p.f_PortCode = t.f_CurrentPort")
        If Me.hdnCountry.Value <> "" Then
            sqlStat.AppendLine("   And p.f_CountryCode = @COUNTRY")
        End If
        sqlStat.AppendLine(" WHERE  t.f_DelFlag        = @DELFLG")

        sqlStat.AppendLine("   AND ( 1=1")
        If Me.hdnTankNo.Value <> "" Then
            sqlStat.AppendLine(" And t.f_TankNo = @TANKNO")
        End If
        If Me.hdnActy.Value <> "" Then
            sqlStat.AppendLine("   And t.f_Acty = @ACTIONID")
        End If

        If Me.hdnStActualDate.Value <> "" AndAlso Me.hdnEndActualDate.Value <> "" Then
            sqlStat.AppendLine("   AND (convert(nvarchar, t.f_Date , 111)  BETWEEN  @STACTUALDATE  AND  @ENDACTUALDATE )")
        End If

        sqlStat.AppendLine(")")
        '共通関数は単一テーブル想定のため全体をサブクエリー化 
        sqlStat.AppendLine(" ORDER BY t.f_TankNo, t.f_CreateDate")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = "FALSE"
                If Me.hdnTankNo.Value <> "" Then
                    .Add("@TANKNO", SqlDbType.NVarChar).Value = Me.hdnTankNo.Value
                End If
                If Me.hdnActy.Value <> "" Then
                    .Add("@ACTIONID", SqlDbType.NVarChar).Value = Me.hdnActy.Value
                End If
                If Me.hdnCountry.Value <> "" Then
                    .Add("@COUNTRY", SqlDbType.NVarChar).Value = Me.hdnCountry.Value
                End If
                If Me.hdnStActualDate.Value <> "" AndAlso Me.hdnEndActualDate.Value <> "" Then
                    .Add("@STACTUALDATE", SqlDbType.Date).Value = FormatDateYMD(Me.hdnStActualDate.Value, GBA00003UserSetting.DATEFORMAT)
                    .Add("@ENDACTUALDATE", SqlDbType.Date).Value = FormatDateYMD(Me.hdnEndActualDate.Value, GBA00003UserSetting.DATEFORMAT)
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
                Case Me.vLeftActy.ID 'アクティブなビューがActy
                    'ACTY選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbActy.Items IsNot Nothing Then
                        Dim selectedItemList = (From item As ListItem In Me.lbActy.Items.Cast(Of ListItem) Where item.Selected Select item.Value)
                        If selectedItemList.Any Then
                            txtObject.Text = String.Join(",", selectedItemList)
                        Else
                            txtObject.Text = ""
                        End If
                    End If
                Case Me.vLeftCountry.ID
                    '国選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbCountry.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbCountry.SelectedItem.Value
                            If Me.lbCountry.SelectedItem.Text.Contains(":") Then
                                Dim parts As String()
                                parts = Split(Me.lbCountry.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                Me.lblCountryText.Text = parts(1)
                            Else
                                Me.lblCountryText.Text = Me.lbCountry.SelectedItem.Text
                            End If
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblCountryText.Text = ""
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
        AddLangSetting(dicDisplayText, Me.lblACTY, "ACTY", "ACTY")
        AddLangSetting(dicDisplayText, Me.lblCountry, "Country", "Country")

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
        retDt.Columns.Add("TANKNO", GetType(String))
        retDt.Columns.Add("ORDERNO", GetType(String))
        retDt.Columns.Add("BRID", GetType(String))
        retDt.Columns.Add("ACTIONID", GetType(String))
        retDt.Columns.Add("DISPSEQ", GetType(String))
        retDt.Columns.Add("ACTUALDATE", GetType(String))
        retDt.Columns.Add("CURRENTPORTCODE", GetType(String))
        retDt.Columns.Add("CURRENTPORT", GetType(String))
        retDt.Columns.Add("CURRENTCOUNTRY", GetType(String))
        retDt.Columns.Add("CURRENTCOUNTRYNAME", GetType(String))
        retDt.Columns.Add("DTLPOLPOD", GetType(String))
        retDt.Columns.Add("POLCOUNTRYCODE", GetType(String))
        retDt.Columns.Add("POLCOUNTRY", GetType(String))
        retDt.Columns.Add("POLCODE", GetType(String))
        retDt.Columns.Add("POL", GetType(String))
        retDt.Columns.Add("POLCOUNTRYCODE2", GetType(String))
        retDt.Columns.Add("POLCOUNTRY2", GetType(String))
        retDt.Columns.Add("POLCODE2", GetType(String))
        retDt.Columns.Add("POL2", GetType(String))
        retDt.Columns.Add("PODCOUNTRYCODE", GetType(String))
        retDt.Columns.Add("PODCOUNTRY", GetType(String))
        retDt.Columns.Add("PODCODE", GetType(String))
        retDt.Columns.Add("POD", GetType(String))
        retDt.Columns.Add("PODCOUNTRYCODE2", GetType(String))
        retDt.Columns.Add("PODCOUNTRY2", GetType(String))
        retDt.Columns.Add("PODCODE2", GetType(String))
        retDt.Columns.Add("POD2", GetType(String))
        retDt.Columns.Add("DEPOT", GetType(String))
        retDt.Columns.Add("BRTYPE", GetType(String))
        retDt.Columns.Add("TERMTYPE", GetType(String))
        retDt.Columns.Add("SHIPPER", GetType(String))
        retDt.Columns.Add("NAMESEN_CMS", GetType(String))
        retDt.Columns.Add("NAMELEN_CMS", GetType(String))
        retDt.Columns.Add("CONSIGNEE", GetType(String))
        retDt.Columns.Add("NAMESEN_CMC", GetType(String))
        retDt.Columns.Add("NAMELEN_CMC", GetType(String))
        retDt.Columns.Add("PRODUCTCODE", GetType(String))
        retDt.Columns.Add("PRODUCT", GetType(String))
        retDt.Columns.Add("INVOICEDBY", GetType(String))
        retDt.Columns.Add("NAMES_TMI", GetType(String))
        retDt.Columns.Add("NAMEL_TMI", GetType(String))
        retDt.Columns.Add("LOADING", GetType(String))
        retDt.Columns.Add("STEAMING", GetType(String))
        retDt.Columns.Add("TIP", GetType(String))
        retDt.Columns.Add("EXTRA", GetType(String))
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
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget            '変数情報取
        Dim retVal As String = ""
        'RightBOX情報設定
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
            CommonFunctions.ShowMessage(COA0022ProfXls.ERR, Me.lblFooterMessage)
            retVal = COA0022ProfXls.ERR
            Return retVal
        End If

        '画面レイアウト情報
        'COA0018ViewList.MAPID = CONST_MAPID
        ''COA0018ViewList.FORWARDMATCHVARIANT = "Default"
        'COA0018ViewList.VIEW = lbRightList
        'COA0018ViewList.COA0018getViewList()
        'If COA0018ViewList.ERR = C_MESSAGENO.NORMAL Then
        '    Try
        '        For i As Integer = 0 To DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items.Count - 1
        '            lbRightList.Items.Add(New ListItem(DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items(i).Text, DirectCast(COA0018ViewList.VIEW, System.Web.UI.WebControls.ListBox).Items(i).Value))
        '        Next
        '    Catch ex As Exception
        '    End Try
        'Else
        '    CommonFunctions.ShowMessage(COA0018ViewList.ERR, Me.lblFooterMessage)
        '    retVal = COA0018ViewList.ERR
        '    Return retVal
        'End If

        'ビューID変数検索
        COA0016VARIget.MAPID = CONST_MAPID
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = Me.hdnThisMapVariant.Value  '"Default" 'Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0016VARIget.FIELD = "REPORTID"
        COA0016VARIget.COA0016VARIget()
        If COA0016VARIget.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0016VARIget.ERR, Me.lblFooterMessage)
            retVal = COA0016VARIget.ERR
            Return retVal
        End If

        'ListBox選択
        'lbRightList.SelectedIndex = 0     '選択無しの場合、デフォルト
        'For i As Integer = 0 To lbRightList.Items.Count - 1

        '    If lbRightList.Items(i).Value <> COA0018ViewList.FORWARDMATCHVARIANT Then
        '        lbRightList.Items(i).Text = lbRightList.Items(i).Text.Replace(":" & COA0016VARIget.VARI, ":")
        '    End If

        '    If lbRightList.Items(i).Value = COA0016VARIget.VALUE Then
        '        lbRightList.SelectedIndex = i
        '    End If
        'Next
        Me.lbRightList.SelectedIndex = -1     '選択無しの場合、デフォルト
        Dim targetListItem = lbRightList.Items.FindByValue(COA0016VARIget.VALUE)
        If targetListItem IsNot Nothing Then
            targetListItem.Selected = True
        Else
            If Me.lbRightList.Items.Count > 0 Then
                Me.lbRightList.SelectedIndex = 0
            End If
        End If


        retVal = C_MESSAGENO.NORMAL
        Return retVal
    End Function
    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00021SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00021SELECT = DirectCast(Page.PreviousPage, GBT00021SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtStActualDate", Me.hdnStActualDate},
                                                                        {"txtEndActualDate", Me.hdnEndActualDate},
                                                                        {"txtTankNo", Me.hdnTankNo},
                                                                        {"txtActy", Me.hdnActy},
                                                                        {"txtCountry", Me.hdnCountry},
                                                                        {"lbRightList", Me.hdnReportVariant},
                                                                        {"rblDataType", Me.hdnSearchDataType}}

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

        End If
    End Sub
    ''' <summary>
    ''' ACTY番号を取得
    ''' </summary>
    ''' <param name="actyCode">省略時は全件取得</param>
    ''' <returns></returns>
    Private Function GetActy(Optional actyCode As String = "") As DataTable
        Dim COA0017FixValue As New COA0017FixValue
        Dim dummyList As New ListBox
        Dim dt As New DataTable
        dt.Columns.Add("NAME", GetType(String))
        dt.Columns.Add("LISTBOXNAME", GetType(String))
        dt.Columns.Add("CODE", GetType(String))
        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "ACTIVITYCODE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            '本当は1だが暫定的に2
            'COA0017FixValue.LISTBOX1 = dummyList
            COA0017FixValue.LISTBOX2 = dummyList
        Else
            COA0017FixValue.LISTBOX2 = dummyList
        End If
        COA0017FixValue.COA0017getListFixValue()
        For Each litem As ListItem In dummyList.Items
            Dim dr As DataRow = dt.NewRow
            dr.Item("NAME") = litem.Text
            dr.Item("LISTBOXNAME") = litem.Value & ":" & litem.Text
            dr.Item("CODE") = litem.Value

            If actyCode <> "" Then
                If actyCode = litem.Value Then
                    dt.Rows.Add(dr)
                    Continue For '複数マッチは想定外なのでそのまま終了
                End If
            Else
                dt.Rows.Add(dr)
            End If
        Next
        Return dt
    End Function

    ''' <summary>
    ''' タンク番号一覧を取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetTankNo() As List(Of String)

        Dim retTanks As New List(Of String)   '戻り値用のデータテーブル
        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        '一覧表示データ復元 
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return retTanks
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return retTanks
        End If

        Return (From dr In dt
                Group By itemTankNo = Convert.ToString(dr("TANKNO")) Into Group
                Order By itemTankNo
                Select itemTankNo).ToList
        ''SQL文作成
        'Dim sqlStat As New StringBuilder
        'sqlStat.AppendLine("SELECT rtrim(TANKNO) as TANKNO")
        'sqlStat.AppendLine("  FROM GBM0006_TANK ")
        'sqlStat.AppendLine(" WHERE  COMPCODE     = @COMPCODE ")
        'sqlStat.AppendLine("   AND  STYMD       <= @STYMD")
        'sqlStat.AppendLine("   AND  ENDYMD      >= @ENDYMD")
        'sqlStat.AppendLine("   AND  DELFLG      <> @DELFLG ")
        'sqlStat.AppendLine(" ORDER BY TANKNO ")
        ''DB接続
        'Using sqlCon As New SqlConnection(COA0019Session.DBcon),
        '      sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

        '    sqlCon.Open() '接続オープン
        '    With sqlCmd.Parameters
        '        .Add("@COMPCODE", System.Data.SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
        '        .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
        '        .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
        '        .Add("@DELFLG", System.Data.SqlDbType.NVarChar).Value = CONST_FLAG_YES
        '    End With
        '    Using sqlDa As New SqlDataAdapter(sqlCmd)
        '        sqlDa.Fill(retDt)
        '    End Using
        'End Using
        'Return retDt

    End Function

    ''' <summary>
    ''' 国コードリストアイテムを設定
    ''' </summary>
    Private Sub SetCountryListItem(selectedValue As String)
        Dim GBA00008Country As New GBA00008Country
        GBA00008Country.COUNTRY_LISTBOX = Me.lbCountry
        GBA00008Country.getCountryList()
        If Not (GBA00008Country.ERR = C_MESSAGENO.NORMAL OrElse GBA00008Country.ERR = C_MESSAGENO.NODATA) Then
            Throw New Exception("Get CountryCode List Error")
            Return
        End If
    End Sub

    ''' <summary>
    ''' [絞り込み条件]Countryコード変更時イベント
    ''' </summary>
    Public Sub txtCountry_Change()
        Me.lblCountryText.Text = ""
        If Me.txtCountry.Text.Trim = "" Then
            Return
        End If

        If Me.lbCountry.Items.Count > 0 Then
            Dim findListItem = Me.lbCountry.Items.FindByValue(Me.txtCountry.Text)
            If findListItem IsNot Nothing Then
                Dim parts As String()
                If findListItem.Text.Contains(":") Then
                    parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                    Me.lblCountryText.Text = parts(1)
                Else
                    Me.lblCountryText.Text = findListItem.Text
                End If
            Else
                Dim findListItemUpper = Me.lbCountry.Items.FindByValue(Me.txtCountry.Text.ToUpper)
                If findListItemUpper IsNot Nothing Then
                    Dim parts As String()
                    If findListItemUpper.Text.Contains(":") Then
                        parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                        Me.lblCountryText.Text = parts(1)
                        Me.txtCountry.Text = parts(0)
                    Else
                        Me.lblCountryText.Text = findListItemUpper.Text
                        Me.txtCountry.Text = findListItemUpper.Value
                    End If
                End If
            End If
        End If
    End Sub
End Class