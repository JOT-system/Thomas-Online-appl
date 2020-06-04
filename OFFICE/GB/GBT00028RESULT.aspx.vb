Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' リース一覧
''' </summary>
Public Class GBT00028RESULT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00028R" '自身のMAPID
    Private Const CONST_BASEID As String = "GBT00028L" '次画面一覧のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分

    Private Const CONST_TBL_INVOICEINFO As String = "GBT0016_INVOICE_INFO"
    Private Const CONST_TBL_INVOICETANK As String = "GBT0017_INVOICE_TANKINFO"
    Private Const CONST_TBL_CUSTOMER As String = "GBM0004_CUSTOMER"

    Private Const CONST_CHECK_ON As String = "1"

    'Private Const CONST_TBL_AGREEMENT As String = "GBT0011_LBR_AGREEMENT"
    'Private Const CONST_TBL_TANK As String = "GBT0012_RESRVLEASETANK"
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 前画面(検索条件保持用)
    ''' </summary>
    Public Property GBT00028SValues As GBT00028SELECT.GBT00028SValues
    ''' <summary>
    ''' 当画面情報保持
    ''' </summary>
    ''' <returns></returns>
    Public Property ThisScreenValues As GBT00028RESULT.GBT00028RValues
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
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, Me.hdnThisMapVariant.Value)
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
                'errMsg = Me.RightboxInit()
                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetInvoiceListDataTable()
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
                    Dim listVari As String = Me.GBT00028SValues.ViewId
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = listVari
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "1"
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
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                Me.GBT00028SValues = DirectCast(ViewState("GBT00028SValues"), GBT00028SELECT.GBT00028SValues)
                '画面の入力情報を保持
                Dim messageNo As String = FileSaveDisplayInput()
                If messageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                                messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})
                    Return
                End If
                '**********************
                ' ボタンクリック判定
                '**********************
                'hdnButtonClickに文字列が設定されていたら実行する
                If Me.hdnButtonClick IsNot Nothing AndAlso Me.hdnButtonClick.Value <> "" Then
                    'ボタンID + "_Click"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)
                    Dim btnEventName As String = Me.hdnButtonClick.Value & "_Click"
                    Dim param() As Object = Nothing

                    If Me.hdnButtonClick.Value.StartsWith("btnTemplateItem") Then
                        btnEventName = "btnTemplateItem" & "_Click"
                        ReDim param(0)
                        param(0) = Me.hdnButtonClick.Value.Replace("btnTemplateItem", "")
                    End If
                    Me.hdnButtonClick.Value = ""
                    CallByName(Me, btnEventName, CallType.Method, param)
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
            DisplayListObjEdit() '共通関数により描画された一覧の制御
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
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    Public Sub btnExtract_Click()
        'Dim dt As DataTable = CreateDataTable()
        'Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        'Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        ''一覧表示データ復元 
        'COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        'COA0021ListTable.TBLDATA = dt
        'COA0021ListTable.COA0021recoverListTable()
        'If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
        '    dt = COA0021ListTable.OUTTBL
        'Else
        '    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
        '    Return
        'End If
        ''そもそも初期検索結果がない場合は絞り込まず終了
        'If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
        '    Return
        'End If

        ''フィルタでの絞り込みを利用するか確認
        'Dim isFillterOff As Boolean = True
        'If Me.txtShipper.Text.Trim <> "" OrElse Me.txtRemarkCont.Text.Trim <> "" Then
        '    isFillterOff = False
        'End If

        'For Each dr As DataRow In dt.Rows
        '    dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
        '    'フィルタ使用時の場合
        '    If isFillterOff = False Then
        '        '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
        '        If Not ((Me.txtShipper.Text.Trim = "" OrElse Convert.ToString(dr("SHIPPERNAME")).Contains(Me.txtShipper.Text.Trim)) _
        '                AndAlso (Me.txtRemarkCont.Text.Trim = "" OrElse Convert.ToString(dr("REMARK")).Contains(Me.txtRemarkCont.Text.Trim))) Then
        '            dr.Item("HIDDEN") = 1
        '        End If
        '    End If
        'Next
        ''画面先頭を表示
        'hdnListPosition.Value = "1"

        ''一覧表示データ保存
        'COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        'COA0021ListTable.TBLDATA = dt
        'COA0021ListTable.COA0021saveListTable()
        'If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
        '    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
        'Else
        '    'メッセージ表示
        '    CommonFunctions.ShowMessage(C_MESSAGENO.NORMALEXTRUCT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
        'End If

        ''カーソル設定
        'Me.txtShipper.Focus()

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
    ''' 請求書新規作成ボタン押下時
    ''' </summary>
    Public Sub btnInvoiceNew_Click()
        '何かと準備
        'TODO何か

        Me.ThisScreenValues = GetDispValue()
        Me.ThisScreenValues.NewInvoiceCreate = True
        Me.ThisScreenValues.InvoiceNo = ""
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl

        '画面遷移先URL取得
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = Me.hdnThisMapVariant.Value
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
        HttpContext.Current.Session("MAPurl") = COA0012DoUrl.URL
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub

    ''' <summary>
    ''' 削除ボタン押下時
    ''' </summary>
    Public Sub btnSave_Click()

        Dim COA0021ListTable As New COA0021ListTable
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        Dim dt As DataTable = CreateDataTable()
        Dim tran As SqlTransaction = Nothing

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
            Return
        End If

        Dim procDateTime As DateTime = DateTime.Now
        'CHECKチェックボックスがチェック済の全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("DELCHK")) = CONST_CHECK_ON)
        Dim deleteDt As DataTable = Nothing
        If q.Any = True Then
            deleteDt = q.CopyToDataTable
        Else
            CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Try
            Dim sqlStat As New StringBuilder
            sqlStat.AppendFormat("  UPDATE {0} ", CONST_TBL_INVOICEINFO).AppendLine()
            sqlStat.AppendLine("   SET DELFLG     = @DELFLG_Y")
            sqlStat.AppendLine("      ,UPDYMD     = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE INVOICENO  = @INVOICENO")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG_Y;")
            sqlStat.AppendFormat("  UPDATE {0} ", CONST_TBL_INVOICETANK).AppendLine()
            sqlStat.AppendLine("   SET DELFLG     = @DELFLG_Y")
            sqlStat.AppendLine("      ,UPDYMD     = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE INVOICENO  = @INVOICENO")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG_Y;")

            Using sqlCon = New SqlConnection(COA0019Session.DBcon),
                sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                tran = sqlCon.BeginTransaction() 'トランザクション開始

                '動的パラメータのみ変数化
                Dim paramInvoiceNo = sqlCmd.Parameters.Add("@INVOICENO", SqlDbType.NVarChar)

                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG_Y", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.Transaction = tran
                For Each drDelete As DataRow In deleteDt.Rows
                    paramInvoiceNo.Value = drDelete.Item("INVOICENO")

                    sqlCmd.CommandText = sqlStat.ToString
                    sqlCmd.ExecuteNonQuery()

                Next

                tran.Commit()

                '    'メール
                '    Dim GBA00009MailSendSet As New GBA00009MailSendSet
                '    GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                '    GBA00009MailSendSet.EVENTCODE = C_BRSEVENT.APPLY
                '    'GBA00009MailSendSet.MAILSUBCODE = subCode
                '    GBA00009MailSendSet.MAILSUBCODE = ""
                '    GBA00009MailSendSet.BRID = Convert.ToString(applyDr.Item("BRID"))
                '    GBA00009MailSendSet.BRSUBID = Convert.ToString(applyDr.Item("SUBID"))
                '    GBA00009MailSendSet.BRBASEID = Convert.ToString(applyDr.Item("BRBASEID"))
                '    GBA00009MailSendSet.BRROUND = ""
                '    GBA00009MailSendSet.APPLYID = applyId
                '    GBA00009MailSendSet.GBA00009setMailToBR()
                '    If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                '        CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                '        Return
                '    End If

            End Using

        Catch ex As Exception
            Throw
        Finally
            If tran IsNot Nothing Then
                tran.Dispose()
            End If
        End Try

        'メッセージ出力
        'hdnMsgId.Value = C_MESSAGENO.APPLYSUCCESS

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub

    ''' <summary>
    ''' 画面グリッドのデータを取得しファイルに保存する。
    ''' </summary>
    Private Function FileSaveDisplayInput() As String

        Dim COA0021ListTable As New COA0021ListTable

        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        dt = CreateDataTable()
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            Return COA0021ListTable.ERR
        End If

        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String)
        '入力値保持用のフィールド名設定
        fieldIdList.Add("DELCHK", objChkPrifix)

        ' とりあえず右データエリアを対象
        For i = 1 To dt.Rows.Count
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    '                    formToPost.Remove(dispObjId)
                End If

                If displayValue <> "" Then
                    Dim targetRow = (From rowItem In dt Where Convert.ToString(rowItem("LINECNT")) = displayValue)
                    targetRow(0).Item(fieldId.Key) = CONST_CHECK_ON
                End If
            Next
        Next

        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()

        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0021ListTable.ERR
        End If
        Return C_MESSAGENO.NORMAL
    End Function

    ''' <summary>
    ''' 請求書情報取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetInvoiceListDataTable() As DataTable
        'ソート順取得
        Dim COA0020ProfViewSort As New COA0020ProfViewSort
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If

        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = Me.hdnThisMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder
        Dim retDt As New DataTable
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,'' AS OPERATION")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,II.CUSTOMERCODE ")
        sqlStat.AppendLine("      ,II.INVOICEMONTH ")
        sqlStat.AppendLine("      ,II.INVOICENOSUB ")
        sqlStat.AppendLine("      ,II.STYMD ")
        sqlStat.AppendLine("      ,II.ENDYMD ")
        sqlStat.AppendLine("      ,II.INVOICENO ")
        sqlStat.AppendLine("      ,II.INCTORICODE ")
        sqlStat.AppendLine("      ,II.REMARK ")
        sqlStat.AppendLine("      ,II.OUTLANGUAGE ")
        sqlStat.AppendLine("      ,II.INVOICEDATE ")
        sqlStat.AppendLine("      ,II.DRAFTOUTPUT ")
        sqlStat.AppendLine("      ,II.ORIGINALOUTPUT ")
        sqlStat.AppendLine("      ,II.AMOUNT ")
        sqlStat.AppendLine("      ,II.FIXFLG ")
        sqlStat.AppendLine("      ,II.DELFLG ")
        sqlStat.AppendLine("      ,II.INITYMD ")
        sqlStat.AppendLine("      ,II.UPDYMD ")
        sqlStat.AppendLine("      ,II.UPDUSER ")
        sqlStat.AppendLine("      ,II.UPDTERMID ")
        sqlStat.AppendLine("      ,CASE WHEN II.DRAFTOUTPUT > 0 THEN '済' ELSE '' END AS DRAFTDISP ")
        sqlStat.AppendLine("      ,CASE WHEN II.ORIGINALOUTPUT > 0 THEN '済' ELSE '' END AS ORIGINALDISP ")
        sqlStat.AppendFormat("      ,MC.{0} AS 'CUSTOMERNAME'", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,0 AS AMOUNT ")
        sqlStat.AppendLine("      ,'' AS DELCHK ")
        sqlStat.AppendFormat("  FROM {0} II", CONST_TBL_INVOICEINFO).AppendLine()
        sqlStat.AppendFormat("    INNER JOIN {0} MC", CONST_TBL_CUSTOMER).AppendLine()
        sqlStat.AppendLine("      ON    MC.COMPCODE       = @COMPCODE")
        sqlStat.AppendLine("      AND   MC.CUSTOMERCODE   = II.CUSTOMERCODE")
        sqlStat.AppendLine("      AND   MC.STYMD         <= @NOWDATE")
        sqlStat.AppendLine("      AND   MC.ENDYMD        >= @NOWDATE")
        sqlStat.AppendLine("      AND   MC.DELFLG        <> @DELFLG")
        sqlStat.AppendLine(" WHERE II.CUSTOMERCODE = @CUSTOMERCODE")
        sqlStat.AppendLine("   AND II.INVOICEMONTH = @INVOICEMONTH")
        sqlStat.AppendLine("   AND II.DELFLG      <> @DELFLG")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)

        Using sqlCon = New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES

                .Add("@INVOICEMONTH", SqlDbType.NVarChar).Value = Me.GBT00028SValues.InvoiceMonth
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = Me.GBT00028SValues.CustomerCode
            End With
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
        AddLangSetting(dicDisplayText, Me.lblInvoiceDate, "請求月", "Invoice Date")
        AddLangSetting(dicDisplayText, Me.lblCustomerName, "顧客名", "Customer Name")

        'AddLangSetting(dicDisplayText, Me.btnExtract, "絞り込み", "Search")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnInvoiceNew, "請求書作成", "Invoice New")
        AddLangSetting(dicDisplayText, Me.btnSave, "削除", "Delete")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

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
        retDt.Columns.Add("INVOICENO", GetType(String))
        retDt.Columns.Add("CHECK", GetType(String))

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
        Me.ThisScreenValues = GetDispValue()
        Me.ThisScreenValues.NewInvoiceCreate = False
        Me.ThisScreenValues.InvoiceNo = Convert.ToString(selectedRow.Item("INVOICENO"))

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
        HttpContext.Current.Session("MAPvariant") = Me.hdnThisMapVariant.Value
        HttpContext.Current.Session("MAPurl") = COA0012DoUrl.URL

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
        COA0013TableObject.VARI = Me.GBT00028SValues.ViewId
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
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00028SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00028SELECT = DirectCast(Page.PreviousPage, GBT00028SELECT)
            Me.GBT00028SValues = prevObj.ThisScreenValues
            ViewState("GBT00028SValues") = Me.GBT00028SValues

        ElseIf TypeOf Page.PreviousPage Is GBT00028INVOICEEDIT Then
            '単票画面からの戻り
            Dim prevObj As GBT00028INVOICEEDIT = DirectCast(Page.PreviousPage, GBT00028INVOICEEDIT)
            Me.GBT00028SValues = prevObj.GBT00028RValues.GBT00028SValues
            ViewState("GBT00028SValues") = Me.GBT00028SValues

        ElseIf TypeOf Page.PreviousPage Is GBT00028RESULT Then
            '自身から遷移（削除時のリフレッシュのみの想定、それ以外の用途を追加する場合は注意）
            Dim prevObj As GBT00028RESULT = DirectCast(Page.PreviousPage, GBT00028RESULT)
            Me.GBT00028SValues = prevObj.GBT00028SValues
            ViewState("GBT00028SValues") = Me.GBT00028SValues

            Me.hdnThisMapVariant.Value = prevObj.hdnThisMapVariant.Value

            Dim prevLbRightObj As ListBox = DirectCast(prevObj.FindControl(Me.lbRightList.ID), ListBox)
            If prevLbRightObj IsNot Nothing Then
                Me.lbRightList.SelectedValue = prevLbRightObj.SelectedValue
            End If

            '正常メッセージをメッセージエリアに表示（削除正常時のみ自身をリフレッシュするため）
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
        End If

        ' 後で移動
        If Me.GBT00028SValues.CustomerCode = "" Then
            Me.btnInvoiceNew.Visible = False
        Else
            Me.btnInvoiceNew.Visible = True
        End If
        Me.txtInvoiceDate.Text = Me.GBT00028SValues.InvoiceMonth
        Me.txtCustomerName.Text = Me.GBT00028SValues.CustomerName

        'Me.WF_LISTAREA.CssClass = Me.hdnSearchType.Value
    End Sub
    ''' <summary>
    ''' 画面表示のテーブルを制御する
    ''' </summary>
    Private Sub DisplayListObjEdit()
        'Dim targetPanel As Panel = Me.WF_LISTAREA
        'Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
        'If rightDataDiv.HasControls = False _
        '   OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
        '   OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
        '    Return
        'End If
        'Dim rightHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HR"), Panel)
        'Dim rightHeaderTable As Table = DirectCast(rightHeaderDiv.Controls(0), Table)
        'Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"ROWTYPE", ""}, {"APPLY", ""},
        '                                                                 {"APPLYTEXT", ""}, {"CONTRACTNO", ""}, {"SHIPPERNAME", ""}, {"PRODUCTNAME", ""}, {"AGREEMENTNO", ""},
        '                                                                 {"ENABLED", ""}}
        'With Nothing '右ヘッダーの列名より対象の列番号を取得
        '    Dim headerTableRow = rightHeaderTable.Rows(0)
        '    If headerTableRow.Cells.Count = 0 Then
        '        Return 'ヘッダー列に列が存在しない場合は終了
        '    End If
        '    'セル名称より列番号を取得
        '    Dim maxCellIndex = headerTableRow.Cells.Count - 1
        '    For cellIndex = 0 To maxCellIndex
        '        Dim targetCell As TableCell = headerTableRow.Cells(cellIndex)
        '        If targetCell.Attributes("cellfiedlname") IsNot Nothing AndAlso
        '       dicColumnNameToNo.ContainsKey(targetCell.Attributes("cellfiedlname")) Then
        '            dicColumnNameToNo(targetCell.Attributes("cellfiedlname")) = cellIndex.ToString
        '        End If
        '    Next
        'End With '列番号取得完了

        'Dim leftHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HL"), Panel)
        'Dim leftHeaderTable As Table = DirectCast(leftHeaderDiv.Controls(0), Table)
        'Dim dicLeftColumnNameToNo As New Dictionary(Of String, String) From {{"ACTIONBTN", ""},
        '                                                                     {"ORDERNO", ""},
        '                                                                     {"TANKSEQ", ""},
        '                                                                     {"TANKNO", ""}, {"APPLY", ""}, {"APPLYTEXT", ""}, {"CONTRACTNO", ""}, {"SHIPPERNAME", ""}, {"PRODUCTNAME", ""}}

        'With Nothing '右ヘッダーの列名より対象の列番号を取得
        '    Dim headerTableRow = leftHeaderTable.Rows(0)
        '    If headerTableRow.Cells.Count = 0 Then
        '        Return 'ヘッダー列に列が存在しない場合は終了
        '    End If
        '    'セル名称より列番号を取得
        '    Dim maxCellIndex = headerTableRow.Cells.Count - 1
        '    For cellIndex = 0 To maxCellIndex
        '        Dim targetCell As TableCell = headerTableRow.Cells(cellIndex)
        '        If targetCell.Attributes("cellfiedlname") IsNot Nothing AndAlso
        '       dicLeftColumnNameToNo.ContainsKey(targetCell.Attributes("cellfiedlname")) Then
        '            dicLeftColumnNameToNo(targetCell.Attributes("cellfiedlname")) = cellIndex.ToString
        '        End If
        '    Next
        'End With '列番号取得完了
        'Dim rightDataTable As Table = DirectCast(rightDataDiv.Controls(0), Table)
        'Dim leftDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DL"), Panel)
        'Dim leftDataTable As Table = DirectCast(leftDataDiv.Controls(0), Table) '1列目LINECNT 、3列目のSHOW DELETEカラム取得用

        ''******************************
        ''レンダリング行のループ
        ''******************************
        'Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        'For i = 0 To rowCnt
        '    Dim tbrRight As TableRow = rightDataTable.Rows(i)

        '    Dim tbrLeft As TableRow = leftDataTable.Rows(i)
        '    Dim hideDelete As String = tbrLeft.Cells(2).Text '1削除負荷、それ以外は削除可能
        '    Dim lineCnt As String = tbrLeft.Cells(0).Text

        '    '業種別の追加
        '    If dicColumnNameToNo("ROWTYPE") <> "" Then
        '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ROWTYPE")))
        '            tbrRight.Attributes.Add("data-rowtype", .Text)
        '            tbrLeft.Attributes.Add("data-rowtype", .Text)
        '        End With
        '    End If
        '    'CONTRACTNOの取得
        '    Dim contractNo As String = ""
        '    If dicColumnNameToNo("CONTRACTNO") <> "" Then
        '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("CONTRACTNO")))
        '            contractNo = .Text
        '        End With
        '    End If
        '    If dicLeftColumnNameToNo("CONTRACTNO") <> "" Then
        '        With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("CONTRACTNO")))
        '            contractNo = .Text
        '        End With
        '    End If

        '    Dim agreementNo As String = ""
        '    If dicColumnNameToNo("AGREEMENTNO") <> "" Then
        '        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("AGREEMENTNO")))
        '            agreementNo = .Text
        '        End With
        '    End If

        '    '表示非表示を切り替えるフィールドのデータ行に印をつける
        '    For Each fieldName As String In {"CONTRACTNO", "SHIPPERNAME", "PRODUCTNAME"}
        '        If dicColumnNameToNo(fieldName) <> "" Then
        '            With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
        '                .Attributes.Add("data-fieldname", fieldName)
        '            End With
        '        End If
        '        If dicLeftColumnNameToNo(fieldName) <> "" Then
        '            With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo(fieldName)))
        '                .Attributes.Add("data-fieldname", fieldName)
        '            End With
        '        End If
        '    Next

        '    If tbrLeft.Attributes("data-rowtype") IsNot Nothing Then
        '        Dim btnName As String = ""
        '        If tbrLeft.Attributes("data-rowtype") = "CONTRACT" Then
        '            btnName = "Add Agreement"
        '        End If
        '        Dim addBtnEnabled = True
        '        If dicColumnNameToNo("ENABLED") <> "" AndAlso
        '           tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ENABLED"))).Text <> "Y" Then
        '            addBtnEnabled = False
        '        End If
        '        If dicLeftColumnNameToNo("ACTIONBTN") <> "" Then
        '            With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ACTIONBTN")))
        '                If .HasControls = True Then
        '                    Dim buttonItem As New HtmlInputButton
        '                    With .Controls(0)
        '                        buttonItem.ID = .ID
        '                        buttonItem.Name = .ID
        '                    End With
        '                    buttonItem.ViewStateMode = ViewStateMode.Disabled

        '                    .Controls.Clear()
        '                    If btnName <> "" Then
        '                        buttonItem.Attributes.Add("onclick", String.Format("addAgreement('{0}')", contractNo))
        '                        buttonItem.Value = btnName
        '                        buttonItem.Disabled = Not addBtnEnabled
        '                        .Controls.Add(buttonItem)
        '                    End If

        '                End If

        '            End With
        '        End If
        '    End If
        'Next

    End Sub
    ''' <summary>
    ''' 当画面の情報を引き渡し用クラスに格納
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDispValue() As GBT00028RValues
        Dim retVal As New GBT00028RValues
        retVal.GBT00028SValues = Me.GBT00028SValues
        Return retVal
    End Function

    ''' <summary>
    ''' 当画面情報保持クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00028RValues
        ''' <summary>
        ''' 新規請求書作成(True:新規作成,False:更新)
        ''' </summary>
        ''' <returns></returns>
        Public Property NewInvoiceCreate As Boolean = False
        ''' <summary>
        ''' 検索画面情報保持値
        ''' </summary>
        ''' <returns></returns>
        Public Property GBT00028SValues As GBT00028SELECT.GBT00028SValues
        ''' <summary>
        ''' 請求書No
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>選択した契約書No</remarks>
        Public Property InvoiceNo As String = ""
    End Class

End Class