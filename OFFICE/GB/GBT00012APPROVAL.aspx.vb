Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' リペアブレーカー承認画面クラス
''' </summary>
Public Class GBT00012APPROVAL
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00012A"   '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_DIRNAME_REPAIR As String = "REPAIR"

    ''' <summary>
    ''' ポストバック時画面上の情報を保持
    ''' </summary>
    Private AfterRepairAttachment As DataTable
    Private BeforeRepairAttachment As DataTable

    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' ページロード時処理
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

                '一覧情報保存先のファイル名
                Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = "GB_Default"
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})
                    Return
                End If

                '****************************************
                '前画面情報取得
                '****************************************
                SetPrevDisplayValues()
                '****************************************
                '初回絞り込み設定
                '****************************************
                If Me.hdnApprovalObj.Value = "" Then
                    If COA0019Session.LANGDISP = C_LANG.JA Then
                        Me.txtApprovalObj.Text = "承認者"
                    Else
                        Me.txtApprovalObj.Text = "Approver"
                    End If
                Else
                    Me.txtApprovalObj.Text = Me.hdnApprovalObj.Value
                End If
                Me.txtApplicantObj.Text = Me.hdnApplicantObj.Value

                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetListDataTable()

                    'チェックボックス設定
                    If Me.hdnXMLsaveFileRet.Value <> "" Then

                        Dim chkdt As DataTable = CreateDataTable()
                        Dim COA0021ListTable As New COA0021ListTable

                        COA0021ListTable.FILEdir = Me.hdnXMLsaveFileRet.Value
                        COA0021ListTable.TBLDATA = chkdt
                        COA0021ListTable.COA0021recoverListTable()
                        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                            chkdt = COA0021ListTable.OUTTBL
                        Else
                            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                                        messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
                            Return
                        End If

                        For i As Integer = 0 To chkdt.Rows.Count - 1
                            For j As Integer = 0 To dt.Rows.Count - 1

                                If Convert.ToString(chkdt.Rows(i)("APPLYID")) = Convert.ToString(dt.Rows(j)("APPLYID")) AndAlso
                                   Convert.ToString(chkdt.Rows(i)("STEP")) = Convert.ToString(dt.Rows(j)("STEP")) Then

                                    dt.Rows(j)("CHECK") = chkdt.Rows(i)("CHECK")
                                End If
                            Next
                        Next

                    End If

                    For Each dr As DataRow In dt.Rows
                        'フィルタ使用時の場合
                        'If Not (Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All") Then
                        '    '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                        '    If Not Convert.ToString(dr.Item("APPROVALOBJECT")) = Me.txtApprovalObj.Text.Trim Then
                        '        dr.Item("HIDDEN") = 1
                        '    Else
                        '        dr.Item("HIDDEN") = 0
                        '    End If
                        'Else
                        '    dr.Item("HIDDEN") = 0
                        'End If
                        dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
                        '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                        If Not ((Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All" OrElse Me.txtApprovalObj.Text.Trim = "" OrElse Convert.ToString(dr("APPROVALOBJECT")).Trim.Equals(Me.txtApprovalObj.Text.Trim)) _
                            AndAlso (Me.txtApplicantObj.Text.Trim = "" OrElse Convert.ToString(dr("APPLICANTID")).Trim.Equals(Me.txtApplicantObj.Text.Trim))) Then
                            dr.Item("HIDDEN") = 1
                        End If

                        '履歴は非表示
                        If dr.Item("DELFLG").Equals(CONST_FLAG_YES) Then
                            dr.Item("HIDDEN") = 1
                        End If

                    Next

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
                    Dim COA0013TableObject As New COA0013TableObject
                    Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)

                    With COA0013TableObject
                        .MAPID = CONST_MAPID
                        .VARI = Me.hdnPrevViewID.Value
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        .SCROLLTYPE = "2"
                        .LEVENT = "ondblclick"
                        .LFUNC = "ListDbClick"
                        .TITLEOPT = True
                        .NOCOLUMNWIDTHOPT = 50
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .USERSORTOPT = 0 '行開閉がある為ソートさせない
                    End With
                    COA0013TableObject.COA0013SetTableObject()

                    If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
                        Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                                  Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
                        ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
                    Else
                        ViewState("DISPLAY_LINECNT_LIST") = Nothing
                    End If

                    Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
                    Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
                    Dim divDrContL As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DL")
                    Dim tblContL As Table = DirectCast(divDrContL.Controls(0), Table)
                    Dim checkedValue As Boolean
                    For i As Integer = 0 To listData.Rows.Count - 1
                        If Convert.ToString(listData.Rows(i).Item("CHECK")) = "on" Then
                            checkedValue = True
                        Else
                            checkedValue = False
                        End If
                        Dim chkId As String = "chkWF_LISTAREACHECK" & Convert.ToString(listData.Rows(i).Item("LINECNT"))
                        Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
                        If chk IsNot Nothing Then
                            chk.Checked = checkedValue
                        End If

                        If Not {C_APP_STATUS.APPLYING, C_APP_STATUS.REVISE}.Contains(Trim(Convert.ToString(listData.Rows(i).Item("STATUS")))) Then
                            chk.Enabled = False
                        Else
                            chk.Enabled = True
                        End If

                        Dim celCls As String = ""
                        If Convert.ToString(listData.Rows(i).Item("DELFLG")) = CONST_FLAG_YES Then
                            celCls = "minusDiscount"
                        Else
                            celCls = ""
                        End If

                        For j As Integer = 0 To tblCont.Rows(i).Cells.Count - 1
                            tblCont.Rows(i).Cells(j).CssClass = celCls
                        Next

                        For k As Integer = 0 To tblContL.Rows(i).Cells.Count - 1
                            tblContL.Rows(i).Cells(k).CssClass = celCls
                        Next

                    Next

                End Using 'DataTable

                'メッセージ設定
                If hdnMsgId.Value <> "" Then
                    CommonFunctions.ShowMessage(hdnMsgId.Value, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)
                End If

                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)

            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                '画面の入力情報を保持
                Dim messageNo As String = FileSaveDisplayInput()
                If messageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})
                    Return
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
                ' ボタンクリック判定
                '**********************
                'hdnButtonClickに文字列が設定されていたら実行する
                If Me.hdnButtonClick IsNot Nothing AndAlso Me.hdnButtonClick.Value <> "" Then
                    'ボタンID + "_Click"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)
                    Dim btnEventName As String = ""
                    If Me.hdnButtonClick.Value.StartsWith("lbl" & Me.WF_LISTAREA.ID & "SHOWTANK") Then
                        btnEventName = "lblListShowTank_Click"
                    Else
                        btnEventName = Me.hdnButtonClick.Value & "_Click"
                    End If
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
                ' 一覧表の行ダブルクリック判定
                '**********************
                If Me.hdnListDBclick.Value <> "" Then
                    ListRowDbClick()
                    Me.hdnListDBclick.Value = ""
                    Return '単票ページにリダイレクトするため念のため処理は終わらせる
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
            DisplayListObjEdit() '共通関数により描画された一覧の制御
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
                '他のビューが存在する場合はViewIdでCaseを追加

                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Me.hdnCalendarValue.Value = FormatDateYMD(txtobj.Text, GBA00003UserSetting.DATEFORMAT)

                        Me.mvLeft.Focus()
                    End If
                    '承認ビュー表示切替
                Case Me.vLeftApprovalObj.ID
                    SetApprovalObjListItem(Me.txtApprovalObj.Text)
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
        COA0011ReturnUrl.VARI = "Default"
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
    ''' Excelダウンロードボタン押下時
    ''' </summary>
    Public Sub btnExcelDownload_Click()

        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = CreateDataTable()

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
    ''' 承認ボタン押下時
    ''' </summary>
    Public Sub btnApproval_Click()
        Dim COA0021ListTable As New COA0021ListTable
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        Dim dt As DataTable = CreateDataTable()

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
        'ステータス否認かつチェック付のレコードを取得
        Dim rejectRow = From item As DataRow In dt
                        Where Convert.ToString(item("STATUS")).Trim = C_APP_STATUS.REVISE _
                      AndAlso Convert.ToString(item("CHECK")) = "on"
        If rejectRow.Any Then
            '否認レコードありの場合処理終了
            'メッセージ出力
            CommonFunctions.ShowMessage(C_MESSAGENO.REVISING, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Dim errNo As String = ""
        'CHECKチェックボックスがチェック済の全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("CHECK")) = "on")
        Dim checkedDt As DataTable = Nothing
        If q.Any = True Then
            checkedDt = q.CopyToDataTable
        Else
            checkedDt = dt.Clone
        End If
        For Each checkedDr As DataRow In checkedDt.Rows 'For i As Integer = 0 To dt.Rows.Count - 1

            '承認登録
            COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
            COA0032Apploval.I_APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            COA0032Apploval.I_STEP = Convert.ToString(checkedDr.Item("STEP"))
            COA0032Apploval.COA0032setApproval()
            If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
                'Return
                If errNo = "" Then
                    errNo = COA0032Apploval.O_ERR
                End If
            End If

            If Convert.ToString(checkedDr.Item("LASTSTEP")) = Convert.ToString(checkedDr.Item("STEP")) Then

                Dim brId As String = ""
                'オーダー登録
                Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                    sqlCon.Open()
                    brId = Convert.ToString(checkedDr.Item("BRID"))
                    'ブレーカー費用項目取得
                    Dim costDt As DataTable = GetBreakerCostData(brId, sqlCon)
                    '新規オーダー番号生成（シーケンスより取得）
                    Dim orderNo As String = GetOrderNo(sqlCon)
                    Dim copyCnt As Integer = 1
                    'DB登録実行
                    Dim entDate As Date = Date.Now
                    Dim tran As SqlTransaction = sqlCon.BeginTransaction() 'トランザクション開始
                    InsertOrderBase(orderNo, brId, sqlCon, tran, entDate)
                    InsertOrderValue(orderNo, costDt, copyCnt, brId, sqlCon, tran, entDate)
                    InsertOrderValue2(orderNo, costDt, copyCnt, sqlCon, tran, entDate)
                    tran.Commit()
                    sqlCon.Close()
                End Using

                Dim Before As String = "BeforeRepair"
                Dim After As String = "AfterRepair"

                '承認前ファイルを取得
                BeforeRepairAttachment = CommonFunctions.GetInitAttachmentFileList(brId, CONST_DIRNAME_REPAIR, CONST_MAPID, True, Before)
                AfterRepairAttachment = CommonFunctions.GetInitAttachmentFileList(brId, CONST_DIRNAME_REPAIR, CONST_MAPID, True, After)

                'File更新処理
                '添付ファイルを正式フォルダに転送
                CommonFunctions.SaveAttachmentFilesList(BeforeRepairAttachment, brId, CONST_DIRNAME_REPAIR, False, Before, True)
                'DoneFile更新処理
                CommonFunctions.SaveAttachmentFilesList(AfterRepairAttachment, brId, CONST_DIRNAME_REPAIR, False, After, True)

                'メール
                Dim GBA00009MailSendSet As New GBA00009MailSendSet
                GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                GBA00009MailSendSet.EVENTCODE = "BRR_Approved"
                GBA00009MailSendSet.MAILSUBCODE = ""
                GBA00009MailSendSet.BRID = Convert.ToString(checkedDr.Item("BRID"))
                GBA00009MailSendSet.BRSUBID = Convert.ToString(checkedDr.Item("SUBID"))
                GBA00009MailSendSet.BRBASEID = Convert.ToString(checkedDr.Item("BRBASEID"))
                GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
                GBA00009MailSendSet.LASTSTEP = Convert.ToString(checkedDr.Item("LASTSTEP"))
                GBA00009MailSendSet.GBA00009setMailToRepBR()
                If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                    'CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                    'Return
                    If errNo = "" Then
                        errNo = GBA00009MailSendSet.ERR
                    End If
                End If

            End If

        Next

        '絞り込み
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = "全て"
            Else
                Me.txtApprovalObj.Text = "All"
            End If
        End If

        Me.hdnApprovalObj.Value = Me.txtApprovalObj.Text
        Me.hdnApplicantObj.Value = Me.txtApplicantObj.Text

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.APPROVALSUCCESS

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        HttpContext.Current.Session("MAPmapid") = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "GB_Default"
        COA0012DoUrl.MAPIDP = Convert.ToString(HttpContext.Current.Session("MAPmapid"))
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

    End Sub
    ''' <summary>
    ''' オーダーNoをシーケンスより取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOrderNo(Optional ByRef sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim orderNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT  '' ")
            sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + (SELECT VALUE1")
            sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
            sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
            sqlStat.AppendLine("            AND KEYCODE = @KEYCODE)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR GBQ0003_ORDER)),4)")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar).Value = "SERVERSEQ"
                    .Add("@KEYCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get new Order error")
                    End If

                    orderNo = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return orderNo
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
    ''' <summary>
    ''' 発着の費用情報を取得
    ''' </summary>
    ''' <param name="breakerId"></param>
    ''' <param name="sqlCon">省略可能：省略した場合は新規接続を生成</param>
    ''' <returns></returns>
    ''' <remarks>オーダー情報として利用するブレーカー費用項目取得</remarks>
    Private Function GetBreakerCostData(breakerId As String, Optional ByRef sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim dtDbResult As New DataTable
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            '発着側
            sqlStat.AppendLine("SELECT INF.BRID")
            sqlStat.AppendLine("     , ISNULL(INF.USETYPE,'')  AS USETYPE")
            sqlStat.AppendLine("     , VL.DTLPOLPOD            AS AGENTKBN")
            sqlStat.AppendLine("     , VL.COSTCODE             AS COSTCODE")
            sqlStat.AppendLine("     , VL.CURRENCYCODE         AS CURRENCYCODE")
            sqlStat.AppendLine("     , VL.TAXATION             AS TAXATION")
            sqlStat.AppendLine("     , VL.APPROVEDUSD          AS AMOUNTBR")
            'sqlStat.AppendLine("     , VL.USD                  AS AMOUNTBR")
            sqlStat.AppendLine("     , VL.LOCAL                AS LOCALBR")
            sqlStat.AppendLine("     , VL.LOCALRATE            AS LOCALRATE")
            sqlStat.AppendLine("     , VL.TAX                  AS TAXBR")
            sqlStat.AppendLine("     , VL.CONTRACTOR           AS CONTRACTORBR")
            sqlStat.AppendLine("     , VL.REMARK               AS REMARK")
            sqlStat.AppendLine("     , ISNULL((CASE VL.DTLPOLPOD WHEN 'POL1' THEN BS.AGENTPOL1 ")
            sqlStat.AppendLine("                                 WHEN 'POL2' THEN BS.AGENTPOL2 ")
            sqlStat.AppendLine("                                 WHEN 'POD1' THEN BS.AGENTPOD1 ")
            sqlStat.AppendLine("                                 WHEN 'POD2' THEN BS.AGENTPOD2 ")
            sqlStat.AppendLine("                                 ELSE '' END ")
            sqlStat.AppendLine("             ),'')             AS OFFICE ")
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTY")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS1,'')   AS WORKOSEQ")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS2,'')   AS DISPSEQ")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS3,'')   AS DATEFIELD")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS4,'')   AS DATEINTERVAL")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS5,'')   AS LASTACT")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS6,'')   AS REQUIREDACT")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS7,'')   AS ORIGINDESTINATION")

            sqlStat.AppendLine("     , ISNULL(BS.INVOICEDBY,'')     AS INVOICEDBY")
            sqlStat.AppendLine("     , ISNULL(BS.AGENTORGANIZER,'') AS AGENTORGANIZER")
            sqlStat.AppendLine("     , CONVERT([date],'1900/01/01') AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , VL.COUNTRYCODE               AS COUNTRYCODE")
            sqlStat.AppendLine("     , ISNULL(BS.TANKNO,'')         AS TANKNO")
            sqlStat.AppendLine("  FROM GBT0001_BR_INFO INF")
            sqlStat.AppendLine("  INNER JOIN  GBT0003_BR_VALUE VL")
            sqlStat.AppendLine("     ON VL.BRID       = @BRID ")
            sqlStat.AppendLine("    AND VL.BRID       = INF.BRID ")
            sqlStat.AppendLine("    AND VL.STYMD     <= INF.ENDYMD")
            sqlStat.AppendLine("    AND VL.ENDYMD    >= INF.STYMD")
            sqlStat.AppendLine("    AND VL.DELFLG <> @DELFLG ")
            sqlStat.AppendLine("  INNER JOIN  GBT0002_BR_BASE BS")
            sqlStat.AppendLine("     ON BS.BRID      = @BRID ")
            sqlStat.AppendLine("    AND BS.BRID      = INF.BRID ")
            sqlStat.AppendLine("    AND BS.DELFLG   <> @DELFLG")
            sqlStat.AppendLine(" WHERE INF.BRID      = @BRID")
            sqlStat.AppendLine("   AND INF.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("   AND INF.TYPE      = 'INFO' ")
            '発着とオーガナイザーのユニオン
            sqlStat.AppendLine(" UNION ALL ")
            'オーガナイザー側
            sqlStat.AppendLine("SELECT INF.BRID")
            sqlStat.AppendLine("     , ISNULL(INF.USETYPE,'')       AS USETYPE")
            sqlStat.AppendLine("     , TRP.AGENTKBN                 AS AGENTKBN")
            sqlStat.AppendLine("     , TRP.COSTCODE                 AS COSTCODE")
            sqlStat.AppendLine("     , '" & GBC_CUR_USD & "'                        AS CURRENCYCODE")
            sqlStat.AppendLine("     , '0'                          AS TAXATION")
            sqlStat.AppendLine("     , 0                            AS AMOUNTBR")
            sqlStat.AppendLine("     , 0                            AS LOCALBR")
            sqlStat.AppendLine("     , 0                            AS LOCALRATE")
            sqlStat.AppendLine("     , 0                            AS TAXBR")
            sqlStat.AppendLine("     , ''                           AS CONTRACTORBR")
            sqlStat.AppendLine("     , ''                           AS REMARK")
            sqlStat.AppendLine("     , ISNULL(BS.INVOICEDBY,'') AS OFFICE")
            sqlStat.AppendLine("     , ISNULL(TRP.ACTIONID,'')      AS ACTY")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS1,'')        AS WORKOSEQ")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS2,'')        AS DISPSEQ")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS3,'')        AS DATEFIELD")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS4,'')        AS DATEINTERVAL")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS5,'')        AS LASTACT")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS6,'')        AS REQUIREDACT")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS7,'')        AS ORIGINDESTINATION")
            sqlStat.AppendLine("     , ISNULL(BS.INVOICEDBY,'')     AS INVOICEDBY")
            sqlStat.AppendLine("     , ISNULL(BS.AGENTORGANIZER,'') AS AGENTORGANIZER")
            sqlStat.AppendLine("     , CONVERT([date],'1900/01/01') AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , BS.COUNTRYORGANIZER          AS COUNTRYCODE")
            sqlStat.AppendLine("     , ISNULL(BS.TANKNO,'')         AS TANKNO")
            sqlStat.AppendLine("  FROM GBT0001_BR_INFO INF")
            sqlStat.AppendLine("  INNER JOIN  GBT0002_BR_BASE BS")
            sqlStat.AppendLine("     ON BS.BRID      = @BRID ")
            sqlStat.AppendLine("    AND BS.BRID      = INF.BRID ")
            sqlStat.AppendLine("    AND BS.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0009_TRPATTERN TRP")
            sqlStat.AppendLine("    ON TRP.COMPCODE   = @COMPCODE")
            sqlStat.AppendLine("   AND TRP.ORG        = @ORG")
            sqlStat.AppendLine("   AND TRP.BRTYPE     = INF.BRTYPE")
            sqlStat.AppendLine("   AND TRP.USETYPE    = INF.USETYPE")
            sqlStat.AppendLine("   AND TRP.AGENTKBN   = 'Organizer'")
            sqlStat.AppendLine("   AND TRP.STYMD     <= INF.ENDYMD")
            sqlStat.AppendLine("   AND TRP.ENDYMD    >= INF.STYMD")
            sqlStat.AppendLine("   AND TRP.DELFLG    <> @DELFLG")
            sqlStat.AppendLine(" WHERE INF.BRID      = @BRID")
            sqlStat.AppendLine("   AND INF.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("   AND INF.TYPE      = 'INFO' ")

            'デマレッジレコード
            sqlStat.AppendLine(" UNION ALL ")
            sqlStat.AppendLine("SELECT INF.BRID")
            sqlStat.AppendLine("     , ISNULL(INF.USETYPE,'')       AS USETYPE")
            sqlStat.AppendLine("     , TRP.AGENTKBN                 AS AGENTKBN")
            sqlStat.AppendLine("     , TRP.COSTCODE                 AS COSTCODE")
            sqlStat.AppendLine("     , '" & GBC_CUR_USD & "'                        AS CURRENCYCODE")
            sqlStat.AppendLine("     , '0'                          AS TAXATION")
            sqlStat.AppendLine("     , 0                            AS AMOUNTBR")
            sqlStat.AppendLine("     , 0                            AS LOCALBR")
            sqlStat.AppendLine("     , 0                            AS LOCALRATE")
            sqlStat.AppendLine("     , 0                            AS TAXBR")
            sqlStat.AppendLine("     , ''                           AS CONTRACTORBR")
            sqlStat.AppendLine("     , ''                           AS REMARK")
            sqlStat.AppendLine("     , ISNULL((CASE TRP.AGENTKBN WHEN 'POL1' THEN BS.AGENTPOL1 ")
            sqlStat.AppendLine("                                 WHEN 'POL2' THEN BS.AGENTPOL2 ")
            sqlStat.AppendLine("                                 WHEN 'POD1' THEN BS.AGENTPOD1 ")
            sqlStat.AppendLine("                                 WHEN 'POD2' THEN BS.AGENTPOD2 ")
            sqlStat.AppendLine("                                 ELSE '' END ")
            sqlStat.AppendLine("             ),'')             AS OFFICE ")
            sqlStat.AppendLine("     , ISNULL(TRP.ACTIONID,'')      AS ACTY")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS1,'')        AS WORKOSEQ")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS2,'')        AS DISPSEQ")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS3,'')        AS DATEFIELD")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS4,'')        AS DATEINTERVAL")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS5,'')        AS LASTACT")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS6,'')        AS REQUIREDACT")
            sqlStat.AppendLine("     , ISNULL(TRP.CLASS7,'')        AS ORIGINDESTINATION")
            sqlStat.AppendLine("     , ISNULL(BS.INVOICEDBY,'')     AS INVOICEDBY")
            sqlStat.AppendLine("     , ISNULL(BS.AGENTORGANIZER,'') AS AGENTORGANIZER")
            sqlStat.AppendLine("     , CONVERT([date],'1900/01/01') AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , ISNULL((CASE TRP.AGENTKBN WHEN 'POL1' THEN BS.LOADCOUNTRY1 ")
            sqlStat.AppendLine("                                 WHEN 'POL2' THEN BS.LOADCOUNTRY2 ")
            sqlStat.AppendLine("                                 WHEN 'POD1' THEN BS.DISCHARGECOUNTRY1 ")
            sqlStat.AppendLine("                                 WHEN 'POD2' THEN BS.DISCHARGECOUNTRY2 ")
            sqlStat.AppendLine("                                 ELSE '' END ")
            sqlStat.AppendLine("             ),'')                  AS COUNTRYCODE ")
            sqlStat.AppendLine("     , ISNULL(BS.TANKNO,'')         AS TANKNO")
            sqlStat.AppendLine("  FROM GBT0001_BR_INFO INF")
            sqlStat.AppendLine("  INNER JOIN  GBT0002_BR_BASE BS")
            sqlStat.AppendLine("     ON BS.BRID      = @BRID ")
            sqlStat.AppendLine("    AND BS.BRID      = INF.BRID ")
            sqlStat.AppendLine("    AND BS.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("  INNER JOIN GBM0009_TRPATTERN TRP")
            sqlStat.AppendLine("    ON TRP.COMPCODE   = @COMPCODE")
            sqlStat.AppendLine("   AND TRP.ORG        = @ORG")
            sqlStat.AppendLine("   AND TRP.BRTYPE     = INF.BRTYPE")
            sqlStat.AppendLine("   AND TRP.USETYPE    = INF.USETYPE")
            sqlStat.AppendLine("   AND TRP.AGENTKBN   IN ('POL1','POD1','POL2','POD2')")
            sqlStat.AppendLine("   AND TRP.AGENTKBN   = INF.TYPE")
            sqlStat.AppendLine("   AND TRP.STYMD     <= INF.ENDYMD")
            sqlStat.AppendLine("   AND TRP.ENDYMD    >= INF.STYMD")
            sqlStat.AppendLine("   AND TRP.DELFLG    <> @DELFLG")
            sqlStat.AppendLine("   AND EXISTS (SELECT 1 ")
            sqlStat.AppendLine("                 FROM COS0017_FIXVALUE")
            sqlStat.AppendLine("                WHERE COMPCODE = @FIXVALCOMPCODE")
            sqlStat.AppendLine("                  AND SYSCODE  = @FIXVALSYSCODE")
            sqlStat.AppendLine("                  AND CLASS    = @FIXVALCLASS")
            sqlStat.AppendLine("                  AND KEYCODE  = TRP.COSTCODE")
            sqlStat.AppendLine("       )")
            sqlStat.AppendLine(" WHERE INF.BRID      = @BRID")
            sqlStat.AppendLine("   AND INF.DELFLG   <> @DELFLG")
            'DB接続
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                    .Add("@ORG", SqlDbType.NVarChar).Value = "GB_Default"
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                    .Add("@FIXVALCOMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE_D
                    .Add("@FIXVALSYSCODE", SqlDbType.NVarChar).Value = C_SYSCODE_GB
                    .Add("@FIXVALCLASS", SqlDbType.NVarChar).Value = C_FIXVALUECLAS.BREX
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dtDbResult)
                End Using
            End Using
            'オーガナイザーレコードの数字項目を埋める
            Dim brData As BreakerData = DirectCast(ViewState("BRDATA"), BreakerData)
            If brData IsNot Nothing Then
                Dim brTotalInvoiced As String = brData.BrTotalInvoiced
                If Not {"", "0"}.Contains(brData.BrAmtPrincipal) Then
                    brTotalInvoiced = brData.BrAmtPrincipal
                End If
                Dim dicOrganizerCost As New Dictionary(Of String, String) From {{GBC_COSTCODE_SALES, brTotalInvoiced},
                                                                                {GBC_COSTCODE_JOTHIRAGE, brData.BrHireage},
                                                                                {GBC_COSTCODE_JOTHIRAGEA, brData.BrAdjustment}}

                For Each orgCost As KeyValuePair(Of String, String) In dicOrganizerCost
                    Dim findResult = (From item In dtDbResult
                                      Where Convert.ToString(item("AGENTKBN")) = "Organizer" _
                                      AndAlso Convert.ToString(item("COSTCODE")) = orgCost.Key).FirstOrDefault
                    If findResult IsNot Nothing Then
                        findResult.Item("AMOUNTBR") = orgCost.Value
                    End If
                Next
            End If

            '重複キー項目を取得
            Dim dupulicateKeys = (From drItem In dtDbResult
                                  Where Convert.ToString(drItem.Item("AGENTKBN")) <> "Organizer"
                                  Group By agentkbn = Convert.ToString(drItem.Item("AGENTKBN")), costcode = Convert.ToString(drItem.Item("COSTCODE"))
                                  Into cnt = Count()
                                  Where cnt > 1
                                  )

            Return dtDbResult

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
    Private Sub InsertOrderBase(orderNo As String, breakerId As String, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If entDate = Date.Parse("1900/01/01") Then
            entDate = Date.Now
        End If
        '文言フィールド（開発中のためいったん固定
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If

        Dim COA0035Convert As New BASEDLL.COA0035Convert
        Dim cnvStr As String = Nothing
        COA0035Convert.I_CONVERT = Convert.ToString(1)
        COA0035Convert.I_CLASS = C_FIXVALUECLAS.CONV_NUM_ENG
        COA0035Convert.COA0035convNumToEng()
        If COA0035Convert.O_ERR = C_MESSAGENO.NORMAL Then
            cnvStr = COA0035Convert.O_CONVERT1
        Else
            Throw New Exception("Fix value getError")
        End If

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0004_ODR_BASE (")
        sqlStat.AppendLine("       ORDERNO")
        sqlStat.AppendLine("      ,STYMD")
        sqlStat.AppendLine("      ,BRID")
        sqlStat.AppendLine("      ,BRTYPE")
        sqlStat.AppendLine("      ,VALIDITYFROM")
        sqlStat.AppendLine("      ,VALIDITYTO")
        sqlStat.AppendLine("      ,TERMTYPE")
        sqlStat.AppendLine("      ,NOOFTANKS")
        sqlStat.AppendLine("      ,SHIPPER")
        sqlStat.AppendLine("      ,CONSIGNEE")
        sqlStat.AppendLine("      ,CARRIER1")
        sqlStat.AppendLine("      ,CARRIER2")
        sqlStat.AppendLine("      ,PRODUCTCODE")
        sqlStat.AppendLine("      ,PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,RECIEPTCOUNTRY1")
        sqlStat.AppendLine("      ,RECIEPTPORT1")
        sqlStat.AppendLine("      ,RECIEPTCOUNTRY2")
        sqlStat.AppendLine("      ,RECIEPTPORT2")
        sqlStat.AppendLine("      ,LOADCOUNTRY1")
        sqlStat.AppendLine("      ,LOADPORT1")
        sqlStat.AppendLine("      ,LOADCOUNTRY2")
        sqlStat.AppendLine("      ,LOADPORT2")
        sqlStat.AppendLine("      ,DISCHARGECOUNTRY1")
        sqlStat.AppendLine("      ,DISCHARGEPORT1")
        sqlStat.AppendLine("      ,DISCHARGECOUNTRY2")
        sqlStat.AppendLine("      ,DISCHARGEPORT2")
        sqlStat.AppendLine("      ,DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,DELIVERYPORT1")
        sqlStat.AppendLine("      ,DELIVERYCOUNTRY2")
        sqlStat.AppendLine("      ,DELIVERYPORT2")
        sqlStat.AppendLine("      ,VSL1")
        sqlStat.AppendLine("      ,VOY1")
        sqlStat.AppendLine("      ,ETD1")
        sqlStat.AppendLine("      ,ETA1")
        sqlStat.AppendLine("      ,VSL2")
        sqlStat.AppendLine("      ,VOY2")
        sqlStat.AppendLine("      ,ETD2")
        sqlStat.AppendLine("      ,ETA2")
        sqlStat.AppendLine("      ,INVOICEDBY")
        sqlStat.AppendLine("      ,LOADING")
        sqlStat.AppendLine("      ,STEAMING")
        sqlStat.AppendLine("      ,TIP")
        sqlStat.AppendLine("      ,EXTRA")
        sqlStat.AppendLine("      ,DEMURTO")
        sqlStat.AppendLine("      ,DEMURUSRATE1")
        sqlStat.AppendLine("      ,DEMURUSRATE2")
        sqlStat.AppendLine("      ,SALESPIC")
        sqlStat.AppendLine("      ,AGENTORGANIZER")
        sqlStat.AppendLine("      ,AGENTPOL1")
        sqlStat.AppendLine("      ,AGENTPOL2")
        sqlStat.AppendLine("      ,AGENTPOD1")
        sqlStat.AppendLine("      ,AGENTPOD2")

        sqlStat.AppendLine("      ,SHIPPERTEXT")
        sqlStat.AppendLine("      ,CONSIGNEETEXT")
        sqlStat.AppendLine("      ,NOTIFYTEXT")
        sqlStat.AppendLine("      ,NOTIFYCONTTEXT1")
        sqlStat.AppendLine("      ,NOTIFYCONTTEXT2")

        sqlStat.AppendLine("      ,PREPAIDAT")
        sqlStat.AppendLine("      ,EXCHANGERATE")
        sqlStat.AppendLine("      ,LOCALCURRENCY")
        sqlStat.AppendLine("      ,PAYABLEAT")

        sqlStat.AppendLine("      ,FREIGHTANDCHARGES")
        sqlStat.AppendLine("      ,GOODSPKGS")
        sqlStat.AppendLine("      ,CONTAINERPKGS")
        sqlStat.AppendLine("      ,NOOFPACKAGE")

        sqlStat.AppendLine("      ,DELFLG")
        sqlStat.AppendLine("      ,INITYMD")
        sqlStat.AppendLine("      ,INITUSER")
        sqlStat.AppendLine("      ,UPDYMD")
        sqlStat.AppendLine("      ,UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD")
        sqlStat.AppendLine(" ) ")
        sqlStat.AppendLine("SELECT @ORDERNO")
        sqlStat.AppendLine("      ,@STYMD")
        sqlStat.AppendLine("      ,BI.BRID")
        sqlStat.AppendLine("      ,BI.BRTYPE")
        sqlStat.AppendLine("      ,BB.VALIDITYFROM")
        sqlStat.AppendLine("      ,BB.VALIDITYTO")
        sqlStat.AppendLine("      ,BB.TERMTYPE")
        sqlStat.AppendLine("      ,BB.NOOFTANKS")
        sqlStat.AppendLine("      ,BB.SHIPPER")
        sqlStat.AppendLine("      ,BB.CONSIGNEE")
        sqlStat.AppendLine("      ,BB.CARRIER1")
        sqlStat.AppendLine("      ,BB.CARRIER2")
        sqlStat.AppendLine("      ,BB.PRODUCTCODE")
        sqlStat.AppendLine("      ,BB.PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,BB.RECIEPTCOUNTRY1")
        sqlStat.AppendLine("      ,BB.RECIEPTPORT1")
        sqlStat.AppendLine("      ,BB.RECIEPTCOUNTRY2")
        sqlStat.AppendLine("      ,BB.RECIEPTPORT2")
        sqlStat.AppendLine("      ,BB.LOADCOUNTRY1")
        sqlStat.AppendLine("      ,BB.LOADPORT1")
        sqlStat.AppendLine("      ,BB.LOADCOUNTRY2")
        sqlStat.AppendLine("      ,BB.LOADPORT2")
        sqlStat.AppendLine("      ,BB.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("      ,BB.DISCHARGEPORT1")
        sqlStat.AppendLine("      ,BB.DISCHARGECOUNTRY2")
        sqlStat.AppendLine("      ,BB.DISCHARGEPORT2")
        sqlStat.AppendLine("      ,BB.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,BB.DELIVERYPORT1")
        sqlStat.AppendLine("      ,BB.DELIVERYCOUNTRY2")
        sqlStat.AppendLine("      ,BB.DELIVERYPORT2")
        sqlStat.AppendLine("      ,BB.VSL1")
        sqlStat.AppendLine("      ,BB.VOY1")
        sqlStat.AppendLine("      ,BB.ETD1")
        sqlStat.AppendLine("      ,BB.ETA1")
        sqlStat.AppendLine("      ,BB.VSL2")
        sqlStat.AppendLine("      ,BB.VOY2")
        sqlStat.AppendLine("      ,BB.ETD2")
        sqlStat.AppendLine("      ,BB.ETA2")
        sqlStat.AppendLine("      ,BB.INVOICEDBY")
        sqlStat.AppendLine("      ,BB.LOADING")
        sqlStat.AppendLine("      ,BB.STEAMING")
        sqlStat.AppendLine("      ,BB.TIP")
        sqlStat.AppendLine("      ,BB.EXTRA")
        sqlStat.AppendLine("      ,BB.DEMURTO")
        sqlStat.AppendLine("      ,BB.DEMURUSRATE1")
        sqlStat.AppendLine("      ,BB.DEMURUSRATE2")
        sqlStat.AppendLine("      ,@SALESPIC")
        sqlStat.AppendLine("      ,BB.AGENTORGANIZER")
        sqlStat.AppendLine("      ,BB.AGENTPOL1")
        sqlStat.AppendLine("      ,BB.AGENTPOL2")
        sqlStat.AppendLine("      ,BB.AGENTPOD1")
        sqlStat.AppendLine("      ,BB.AGENTPOD2")

        sqlStat.AppendFormat("      ,ISNULL(SP.{0} + CHAR(13) + CHAR(10) + SP.ADDR,'') AS SHIPPERTEXT ", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CS.{0} + CHAR(13) + CHAR(10) + CS.ADDR,'') AS CONSIGNEETEXT", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CS.{0} + CHAR(13) + CHAR(10) + CS.ADDR,'') AS NOTIFYTEXT", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,ISNULL(TR1.NAMEL + CHAR(13) + CHAR(10) + TR1.ADDR + CHAR(13) + CHAR(10) + CASE WHEN TR1.TEL = '' THEN '' ELSE 'TEL:' + TR1.TEL + ' ' END + CASE WHEN TR1.FAX = '' THEN '' ELSE 'FAX:' + TR1.FAX END ,'') AS NOTIFYCONTTEXT1")
        sqlStat.AppendLine("      ,ISNULL(TR2.NAMEL + CHAR(13) + CHAR(10) + TR2.ADDR + CHAR(13) + CHAR(10) + CASE WHEN TR2.TEL = '' THEN '' ELSE 'TEL:' + TR2.TEL + ' ' END + CASE WHEN TR2.FAX = '' THEN '' ELSE 'FAX:' + TR2.FAX END ,'') AS NOTIFYCONTTEXT2")

        sqlStat.AppendLine("      ,ISNULL(SP.CITY,'') AS PREPAIDAT")
        sqlStat.AppendLine("      ,ISNULL(ER.EXRATE,'') AS EXCHANGERATE")
        sqlStat.AppendLine("      ,ISNULL(ER.CURRENCYCODE,'') AS LOCALCURRENCY")
        sqlStat.AppendLine("      ,ISNULL(CS.CITY,'') AS PAYABLEAT")

        sqlStat.AppendLine("      ,CASE WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY1 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY1 THEN '""FREIGHT COLLECT"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY2 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY2 THEN '""FREIGHT COLLECT"" AS ARRANGED' ")
        sqlStat.AppendLine("      ELSE '' END AS FREIGHTANDCHARGES")
        sqlStat.AppendLine("      ,ISNULL(FV1.VALUE1 + CASE WHEN PD.PRODUCTNAME IS NULL THEN '' ELSE CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) + TRIM(PD.PRODUCTNAME) END + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) + CONVERT(nvarchar,BB.TIP) + @DAYSTEXT + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) ")
        sqlStat.AppendLine("      + CASE WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY1 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY1 THEN '""FREIGHT COLLECT"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY2 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY2 THEN '""FREIGHT COLLECT"" AS ARRANGED' ")
        sqlStat.AppendLine("      ELSE '' END ,'') AS GOODSPKGS")
        sqlStat.AppendLine("      ,@CONTAINERPKGS")
        sqlStat.AppendLine("      ,@NOOFPACKAGE")

        sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'") '削除フラグ(0固定)
        sqlStat.AppendLine("      ,@ENTDATE")
        sqlStat.AppendLine("      ,@UPDUSER")
        sqlStat.AppendLine("      ,@ENTDATE")
        sqlStat.AppendLine("      ,@UPDUSER")
        sqlStat.AppendLine("      ,@UPDTERMID")
        sqlStat.AppendLine("      ,@RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
        sqlStat.AppendLine("  LEFT JOIN GBT0002_BR_BASE BB")
        sqlStat.AppendLine("    ON BB.BRID     = @BRID")
        sqlStat.AppendLine("   AND BB.BRID     = BI.BRID")
        sqlStat.AppendLine("   AND BB.BRBASEID = BI.LINKID")
        sqlStat.AppendLine("   AND BB.DELFLG  <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'Shipper
        sqlStat.AppendLine("    ON SP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND SP.CUSTOMERCODE = BB.SHIPPER")
        sqlStat.AppendLine("   AND SP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND SP.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND SP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CS") 'Consinee
        sqlStat.AppendLine("    ON CS.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND CS.CUSTOMERCODE = BB.CONSIGNEE")
        sqlStat.AppendLine("   AND CS.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND CS.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND CS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND CS.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR1") 'Party to Contact1
        sqlStat.AppendLine("    ON TR1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TR1.CARRIERCODE  = BB.AGENTPOD1")
        sqlStat.AppendLine("   AND TR1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TR1.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TR1.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR2") 'Party to Contact2
        sqlStat.AppendLine("    ON TR2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TR2.CARRIERCODE  = BB.AGENTPOD2")
        sqlStat.AppendLine("   AND TR2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TR2.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TR2.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'Product
        sqlStat.AppendLine("    ON PD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND PD.PRODUCTCODE  = BB.PRODUCTCODE")
        sqlStat.AppendLine("   AND PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND PD.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND PD.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") 'FIXVAL
        sqlStat.AppendLine("    ON FV1.CLASS       = 'DESCGOODS'")
        sqlStat.AppendLine("   AND FV1.DELFLG     <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE ER") 'ExRate
        sqlStat.AppendLine("    ON ER.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND ER.COUNTRYCODE  = SP.COUNTRYCODE")
        sqlStat.AppendLine("   AND ER.TARGETYM     = (SELECT FORMAT(CONVERT(DATETIME,(LEFT(CONVERT(VARCHAR, GETDATE(), 112), 6)+'01')),'yyyy/MM/dd'))")
        sqlStat.AppendLine("   AND ER.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR3") 'COUNTRY
        sqlStat.AppendLine("    ON TR3.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TR3.CARRIERCODE  = BB.INVOICEDBY")
        sqlStat.AppendLine("   AND TR3.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TR3.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TR3.DELFLG      <> @DELFLG")

        sqlStat.AppendLine(" WHERE BI.BRID    = @BRID")
        sqlStat.AppendLine("   AND BI.TYPE    = @TYPE")
        sqlStat.AppendLine("   AND BI.DELFLG <> @DELFLG")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                'SQLパラメータの設定
                With sqlCmd.Parameters
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@STYMD", SqlDbType.Date).Value = entDate
                    .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                    .Add("@TYPE", SqlDbType.NVarChar).Value = "INFO"
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = COA0019Session.APSRVCamp
                    .Add("@DAYSTEXT", SqlDbType.NVarChar).Value = "DAYS DETENTION FREE AT DESTINATION"
                    .Add("@CONTAINERPKGS", SqlDbType.NVarChar).Value = cnvStr & "(" & Convert.ToString(1) & ")" & " TANK CONTAINER(S) ONLY"
                    .Add("@NOOFPACKAGE", SqlDbType.NVarChar).Value = Convert.ToString(1)
                    .Add("@SALESPIC", SqlDbType.NVarChar).Value = COA0019Session.USERID
                End With
                sqlCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try
    End Sub
    ''' <summary>
    ''' オーダー費用情報を更新
    ''' </summary>
    ''' <param name="orderNo">オーダーNo</param>
    ''' <param name="dt">費用データテーブル(コピー増幅なし)</param>
    ''' <param name="copyNum">コピー数</param>
    ''' <param name="sqlCon">[In(省略可)]SQL接続オブジェクト</param>
    ''' <param name="tran">[In(省略可)]SQLトランザクションオブジェクト</param>
    Private Sub InsertOrderValue(orderNo As String, dt As DataTable, copyNum As Integer, brId As String, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If entDate = Date.Parse("1900/01/01") Then
            entDate = Date.Now
        End If
        'この段階でありえないがコピー数が0の場合は終了
        If copyNum = 0 Then
            Return
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("        ORDERNO")
        sqlStat.AppendLine("       ,TANKSEQ")
        sqlStat.AppendLine("       ,DTLPOLPOD")
        sqlStat.AppendLine("       ,DTLOFFICE")
        sqlStat.AppendLine("       ,TANKNO")
        sqlStat.AppendLine("       ,COSTCODE")
        sqlStat.AppendLine("       ,ACTIONID")
        sqlStat.AppendLine("       ,DISPSEQ")
        sqlStat.AppendLine("       ,LASTACT")
        sqlStat.AppendLine("       ,REQUIREDACT")
        sqlStat.AppendLine("       ,ORIGINDESTINATION")
        sqlStat.AppendLine("       ,COUNTRYCODE")
        sqlStat.AppendLine("       ,CURRENCYCODE")
        sqlStat.AppendLine("       ,TAXATION")
        sqlStat.AppendLine("       ,AMOUNTBR")
        sqlStat.AppendLine("       ,AMOUNTORD")
        sqlStat.AppendLine("       ,AMOUNTFIX")
        sqlStat.AppendLine("       ,CONTRACTORBR")
        sqlStat.AppendLine("       ,CONTRACTORODR")
        sqlStat.AppendLine("       ,CONTRACTORFIX")
        sqlStat.AppendLine("       ,SCHEDELDATEBR")
        sqlStat.AppendLine("       ,SCHEDELDATE")
        sqlStat.AppendLine("       ,LOCALBR")
        sqlStat.AppendLine("       ,LOCALRATE")
        sqlStat.AppendLine("       ,TAXBR")
        sqlStat.AppendLine("       ,INVOICEDBY")
        sqlStat.AppendLine("       ,REMARK")
        sqlStat.AppendLine("       ,BRID")
        sqlStat.AppendLine("       ,BRCOST")
        sqlStat.AppendLine("       ,DATEFIELD")
        sqlStat.AppendLine("       ,DATEINTERVAL")
        sqlStat.AppendLine("       ,BRADDEDCOST")
        sqlStat.AppendLine("       ,AGENTORGANIZER")
        sqlStat.AppendLine("       ,DELFLG")
        sqlStat.AppendLine("       ,INITYMD")
        sqlStat.AppendLine("       ,INITUSER")
        sqlStat.AppendLine("       ,UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD")
        sqlStat.AppendLine(" ) VALUES ( ")
        sqlStat.AppendLine("        @ORDERNO")
        sqlStat.AppendLine("       ,@TANKSEQ")
        sqlStat.AppendLine("       ,@DTLPOLPOD")
        sqlStat.AppendLine("       ,@DTLOFFICE")
        sqlStat.AppendLine("       ,@TANKNO")
        sqlStat.AppendLine("       ,@COSTCODE")
        sqlStat.AppendLine("       ,@ACTIONID")
        sqlStat.AppendLine("       ,@DISPSEQ")
        sqlStat.AppendLine("       ,@LASTACT")
        sqlStat.AppendLine("       ,@REQUIREDACT")
        sqlStat.AppendLine("       ,@ORIGINDESTINATION")
        sqlStat.AppendLine("       ,@COUNTRYCODE")
        sqlStat.AppendLine("       ,@CURRENCYCODE")
        sqlStat.AppendLine("       ,@TAXATION")
        sqlStat.AppendLine("       ,@AMOUNTBR")
        sqlStat.AppendLine("       ,@AMOUNTBR")
        sqlStat.AppendLine("       ,@AMOUNTBR")
        sqlStat.AppendLine("       ,@CONTRACTORBR")
        sqlStat.AppendLine("       ,@CONTRACTORBR")
        sqlStat.AppendLine("       ,@CONTRACTORBR")
        sqlStat.AppendLine("       ,@SCHEDELDATEBR")
        sqlStat.AppendLine("       ,@SCHEDELDATEBR")
        sqlStat.AppendLine("       ,@LOCALBR")
        sqlStat.AppendLine("       ,@LOCALRATE")
        sqlStat.AppendLine("       ,@TAXBR")
        sqlStat.AppendLine("       ,@INVOICEDBY")
        sqlStat.AppendLine("       ,@REMARK")
        sqlStat.AppendLine("       ,@BRID")
        sqlStat.AppendLine("       ,@BRCOST")
        sqlStat.AppendLine("       ,@DATEFIELD")
        sqlStat.AppendLine("       ,@DATEINTERVAL")
        sqlStat.AppendLine("       ,@BRADDEDCOST")
        sqlStat.AppendLine("       ,@AGENTORGANIZER")
        sqlStat.AppendLine("       ,@DELFLG")
        sqlStat.AppendLine("       ,@ENTDATE")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@ENTDATE")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine(")")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                'SQLパラメータの設定
                With sqlCmd.Parameters
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@BRID", SqlDbType.NVarChar).Value = brId
                    .Add("@BRCOST", SqlDbType.NVarChar).Value = "1"

                    .Add("@CURRENCYCODE", SqlDbType.NVarChar).Value = GBC_CUR_USD 'USD固定に変更
                    .Add("@BRADDEDCOST", SqlDbType.NVarChar).Value = ""

                    .Add("@SCHEDELDATEBR", SqlDbType.Date).Value = entDate

                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                'コピー数分ループ(TANKSEQ)の0埋め前
                Dim paramTankSeq As SqlParameter = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar)
                Dim paramDtlPolPod As SqlParameter = sqlCmd.Parameters.Add("@DTLPOLPOD", SqlDbType.NVarChar)
                Dim paramDtlOffice As SqlParameter = sqlCmd.Parameters.Add("@DTLOFFICE", SqlDbType.NVarChar)
                Dim paramTankNo As SqlParameter = sqlCmd.Parameters.Add("@TANKNO", SqlDbType.NVarChar)
                Dim paramCostCode As SqlParameter = sqlCmd.Parameters.Add("@COSTCODE", SqlDbType.NVarChar)
                Dim paramActionId As SqlParameter = sqlCmd.Parameters.Add("@ACTIONID", SqlDbType.NVarChar)
                Dim paramDispSeq As SqlParameter = sqlCmd.Parameters.Add("@DISPSEQ", SqlDbType.NVarChar)
                Dim paramLastAct As SqlParameter = sqlCmd.Parameters.Add("@LASTACT", SqlDbType.NVarChar)
                Dim paramRequiredAct As SqlParameter = sqlCmd.Parameters.Add("@REQUIREDACT", SqlDbType.NVarChar)
                Dim paramOriginDestination As SqlParameter = sqlCmd.Parameters.Add("@ORIGINDESTINATION", SqlDbType.NVarChar)
                Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar)

                Dim paramTaxation As SqlParameter = sqlCmd.Parameters.Add("@TAXATION", SqlDbType.NVarChar)

                Dim paramAmountBr As SqlParameter = sqlCmd.Parameters.Add("@AMOUNTBR", SqlDbType.Float)
                Dim paramContractorBr As SqlParameter = sqlCmd.Parameters.Add("@CONTRACTORBR", SqlDbType.NVarChar)

                Dim paramLocalBr As SqlParameter = sqlCmd.Parameters.Add("@LOCALBR", SqlDbType.Float)
                Dim paramLocalRate As SqlParameter = sqlCmd.Parameters.Add("@LOCALRATE", SqlDbType.Float)
                Dim paramTaxBr As SqlParameter = sqlCmd.Parameters.Add("@TAXBR", SqlDbType.Float)
                Dim paramInvoicedBy As SqlParameter = sqlCmd.Parameters.Add("@INVOICEDBY", SqlDbType.NVarChar)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar)
                Dim paramBrDateField As SqlParameter = sqlCmd.Parameters.Add("@DATEFIELD", SqlDbType.NVarChar)
                Dim paramDateInterval As SqlParameter = sqlCmd.Parameters.Add("@DATEINTERVAL", SqlDbType.NVarChar)
                Dim paramAgentOrganizer As SqlParameter = sqlCmd.Parameters.Add("@AGENTORGANIZER", SqlDbType.NVarChar)

                For i = 1 To copyNum
                    Dim tankSeq As String = i.ToString("000")
                    paramTankSeq.Value = tankSeq
                    'データテーブルループ
                    For Each dr As DataRow In dt.Rows
                        paramDtlPolPod.Value = Convert.ToString(dr.Item("AGENTKBN"))
                        paramDtlOffice.Value = Convert.ToString(dr.Item("OFFICE"))
                        paramTankNo.Value = Convert.ToString(dr.Item("TANKNO"))
                        paramCostCode.Value = Convert.ToString(dr.Item("COSTCODE"))
                        paramActionId.Value = Convert.ToString(dr.Item("ACTY"))
                        paramDispSeq.Value = Convert.ToString(dr.Item("DISPSEQ"))
                        paramLastAct.Value = Convert.ToString(dr.Item("LASTACT"))
                        paramRequiredAct.Value = Convert.ToString(dr.Item("REQUIREDACT"))
                        paramOriginDestination.Value = Convert.ToString(dr.Item("ORIGINDESTINATION"))
                        paramCountryCode.Value = Convert.ToString(dr.Item("COUNTRYCODE"))

                        paramTaxation.Value = Convert.ToString(dr.Item("TAXATION"))

                        paramAmountBr.Value = dr.Item("AMOUNTBR")
                        paramContractorBr.Value = Convert.ToString(dr.Item("CONTRACTORBR"))
                        paramLocalBr.Value = dr.Item("LOCALBR")
                        paramLocalRate.Value = dr.Item("LOCALRATE")
                        paramTaxBr.Value = dr.Item("TAXBR")
                        paramInvoicedBy.Value = Convert.ToString(dr.Item("INVOICEDBY"))
                        paramRemark.Value = Convert.ToString(dr.Item("REMARK"))

                        paramBrDateField.Value = Convert.ToString(dr.Item("DATEFIELD"))
                        paramDateInterval.Value = Convert.ToString(dr.Item("DATEINTERVAL"))

                        paramAgentOrganizer.Value = Convert.ToString(dr.Item("AGENTORGANIZER"))

                        sqlCmd.ExecuteNonQuery()
                    Next 'End DataRow Loop
                Next 'End copyNum Loop
            End Using
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try
    End Sub
    ''' <summary>
    ''' オーダー2費用情報を更新
    ''' </summary>
    ''' <param name="orderNo">オーダーNo</param>
    ''' <param name="dt">費用データテーブル(コピー増幅なし)</param>
    ''' <param name="copyNum">コピー数</param>
    ''' <param name="sqlCon">[In(省略可)]SQL接続オブジェクト</param>
    ''' <param name="tran">[In(省略可)]SQLトランザクションオブジェクト</param>
    Private Sub InsertOrderValue2(orderNo As String, dt As DataTable, copyNum As Integer, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If entDate = Date.Parse("1900/01/01") Then
            entDate = Date.Now
        End If
        'この段階でありえないがコピー数が0の場合は終了
        If copyNum = 0 Then
            Return
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0007_ODR_VALUE2 (")
        sqlStat.AppendLine("        ORDERNO")
        sqlStat.AppendLine("       ,TANKSEQ")
        sqlStat.AppendLine("       ,TRILATERAL")
        sqlStat.AppendLine("       ,TANKTYPE")
        sqlStat.AppendLine("       ,NOOFPACKAGE")
        sqlStat.AppendLine("       ,DELFLG")
        sqlStat.AppendLine("       ,INITYMD")
        sqlStat.AppendLine("       ,UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD")
        sqlStat.AppendLine(" ) VALUES ( ")
        sqlStat.AppendLine("        @ORDERNO")
        sqlStat.AppendLine("       ,@TANKSEQ")
        sqlStat.AppendLine("       ,@TRILATERAL")
        sqlStat.AppendLine("       ,@TANKTYPE")
        sqlStat.AppendLine("       ,@NOOFPACKAGE")
        sqlStat.AppendLine("       ,@DELFLG")
        sqlStat.AppendLine("       ,@ENTDATE")
        sqlStat.AppendLine("       ,@ENTDATE")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine(")")

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                With sqlCmd.Parameters
                    'SQLパラメータの設定
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@TANKTYPE", SqlDbType.NVarChar).Value = "20TK"
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                    .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    .Add("@NOOFPACKAGE", SqlDbType.Float).Value = 1
                End With

                Dim paramTankSeq As SqlParameter = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar)
                Dim paramTrilateral As SqlParameter = sqlCmd.Parameters.Add("@TRILATERAL", SqlDbType.NVarChar)

                'コピー数分ループ(TANKSEQ)の0埋め前
                For i = 1 To copyNum
                    Dim tankSeq As String = i.ToString("000")
                    paramTankSeq.Value = tankSeq
                    Dim blDataList As New Dictionary(Of String, Object)
                    Dim oneFlg As Boolean = False
                    Dim twoFlg As Boolean = False

                    'データテーブルループ
                    For Each dr As DataRow In dt.Rows

                        Select Case Convert.ToString(dr.Item("AGENTKBN"))
                            Case "POL1", "POD1"
                                paramTrilateral.Value = "1"
                                If oneFlg Then
                                    Continue For
                                Else
                                    oneFlg = True
                                End If
                            Case "POL2", "POD2"
                                paramTrilateral.Value = "2"
                                If twoFlg Then
                                    Continue For
                                Else
                                    twoFlg = True
                                End If
                            Case Else
                                Continue For
                        End Select

                        sqlCmd.ExecuteNonQuery()
                    Next 'End DataRow Loop

                Next 'End copyNum Loop
            End Using
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try

    End Sub
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
                Case Me.vLeftCal.ID 'アクティブなビューがカレンダー
                    'カレンダー選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        txtobj.Text = Me.hdnCalendarValue.Value
                        txtobj.Focus()
                    End If
                Case Me.vLeftApprovalObj.ID 'アクティブなビューが承認対象
                    '承認対象選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbApprovalObj.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbApprovalObj.SelectedItem.Text
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
        AddLangSetting(dicDisplayText, Me.btnReject, "否認", "Reject")
        AddLangSetting(dicDisplayText, Me.btnApproval, "承認", "Approval")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblApprovalObjLabel, "種別", "Type")
        AddLangSetting(dicDisplayText, Me.lblApplicantObjLabel, "申請者", "Applicant")

        AddLangSetting(dicDisplayText, Me.hdnTextShow, "表示", "Show")
        AddLangSetting(dicDisplayText, Me.hdnTextHide, "非表示", "Hide")

        SetDisplayLangObjects(dicDisplayText, lang)

    End Sub
    ''' <summary>
    ''' 一覧表のデータテーブルを取得する関数
    ''' </summary>
    ''' <returns></returns>
    Private Function GetListDataTable() As DataTable
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        'ソート順取得
        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = "Default"
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim dt As New DataTable
        Dim sqlStat As New StringBuilder
        '承認情報取得
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(BS.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,BS.BRID")
        sqlStat.AppendLine("      ,BS.BRBASEID")
        sqlStat.AppendLine("      ,BI.SUBID")
        sqlStat.AppendLine("      ,BI.LINKID")
        sqlStat.AppendLine("      ,convert(nvarchar, BS.STYMD , 111) as STYMD")
        sqlStat.AppendLine("      ,convert(nvarchar, BS.ENDYMD , 111) as ENDYMD")
        sqlStat.AppendLine("      ,ISNULL(BS.AGENTORGANIZER,'') AS AGENTORGANIZER")
        sqlStat.AppendLine("      ,ISNULL(BS.AGENTPOL1,'')      AS AGENTPOL1")
        sqlStat.AppendLine("      ,ISNULL(BS.AGENTPOL2,'')      AS AGENTPOL2")
        sqlStat.AppendLine("      ,ISNULL(BS.AGENTPOD1,'')      AS AGENTPOD1")
        sqlStat.AppendLine("      ,ISNULL(BS.AGENTPOD2,'')      AS AGENTPOD2")
        sqlStat.AppendLine("      ,CASE WHEN BS.APPLYTEXT<>'' THEN '〇' ELSE '' END AS HASREMARK")
        sqlStat.AppendLine("      ,BS.DELFLG")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN CASE WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN ISNULL(FV1.VALUE1,'') + '+' ")
        sqlStat.AppendLine("                                            ELSE CASE WHEN AH.STEP > ISNULL(AH2.STEP,'" & C_APP_FIRSTSTEP & "') THEN ISNULL(FV1.VALUE1,'') + '*' ")
        sqlStat.AppendLine("                                            ELSE ISNULL(FV1.VALUE1,'') END END ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN CASE WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN ISNULL(FV1.VALUE2,'') + '+'  ")
        sqlStat.AppendLine("                                            ELSE CASE WHEN AH.STEP > ISNULL(AH2.STEP,'" & C_APP_FIRSTSTEP & "') THEN ISNULL(FV1.VALUE2,'') + '*' ")
        sqlStat.AppendLine("                                            ELSE ISNULL(FV1.VALUE2,'') END END END AS APPROVALOBJECT ")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV2.VALUE1,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV2.VALUE2,'') END AS APPROVALORREJECT")
        sqlStat.AppendLine("      ,'' AS ""CHECK""")
        sqlStat.AppendLine("      ,ISNULL(AH.APPLYID,'') AS APPLYID")
        sqlStat.AppendLine("      ,ISNULL(AH.APPLICANTID,'') AS APPLICANTID")
        sqlStat.AppendLine("      ,ISNULL(AH.STEP,'')    AS STEP")
        sqlStat.AppendLine("      ,ISNULL(AH.STATUS,'')  AS STATUS")
        sqlStat.AppendLine("      ,CASE WHEN (AH3.STEP = BI.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.APPROVED & "') THEN 'APPROVED' ") '承認
        sqlStat.AppendLine("            WHEN (AH3.STEP = BI.LASTSTEP AND AH4.STATUS = '" & C_APP_STATUS.REJECT & "') THEN 'REJECT' ") '否認
        sqlStat.AppendLine("            ELSE trim(convert(char,(convert(int,isnull(AH3.STEP,'00'))))) + '/' + trim(convert(char,convert(int,BI.LASTSTEP))) END as STEPSTATE")
        sqlStat.AppendLine("      ,CASE WHEN AH.STATUS = '" & C_APP_STATUS.APPROVED & "' THEN '--' ") '承認
        sqlStat.AppendLine("            WHEN AH.STATUS = '" & C_APP_STATUS.REJECT & "' THEN '--' ") '否認
        sqlStat.AppendLine("            ELSE isnull(AH2.STEP,'" & C_APP_FIRSTSTEP & "') END as CURSTEP")
        sqlStat.AppendLine("      ,ISNULL(AP.APPROVALTYPE,'') AS APPROVALTYPE")
        'sqlStat.AppendLine("      ,ISNULL(TRIM(AH4.APPROVERID) + '(' + ISNULL(AH4.STEP,'--') + ')','') AS APPROVERID")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(US.STAFFNAMES,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(US.STAFFNAMES_EN,'') END AS APPROVERID")
        'sqlStat.AppendLine("      ,TI.TOTALINVOICED AS TOTALINVOICED")
        sqlStat.AppendLine("      ,ISNULL(TI.TOTALCOST,'0.00')        AS TOTALCOST")
        sqlStat.AppendLine("      ,ISNULL(TI.TOTALAPPROVE,'0.00')     AS TOTALAPPROVE")

        sqlStat.AppendLine("      ,ISNULL(BS.COUNTRYORGANIZER,'') AS COUNTRYORGANIZER")
        sqlStat.AppendLine("      ,ISNULL(BS.LASTORDERNO,'')      AS ORDERNO")
        sqlStat.AppendLine("      ,ISNULL(BS.TANKNO,'')           AS TANKNO")
        sqlStat.AppendLine("      ,ISNULL(BS.DEPOTCODE,'')        AS DEPOTCODE")
        sqlStat.AppendLine("      ,ISNULL(DP.NAMES,'')            AS DEPOTNAME")
        sqlStat.AppendLine("      ,ISNULL(DP.LOCATION,'')         AS LOCATION")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV3.VALUE1,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV3.VALUE2,'') END AS TANKUSAGE")
        sqlStat.AppendLine("      ,'' AS SHOWTANK")
        sqlStat.AppendLine("      ,CASE WHEN BS.DELFLG = '" & CONST_FLAG_NO & "' THEN 'HIDE' ELSE 'SHOW' END AS SHOWHIDE")
        sqlStat.AppendLine("      ,CASE WHEN BS.DELFLG = '" & CONST_FLAG_NO & "' THEN '1' ELSE '0' END AS BASEVALUEFLG")
        sqlStat.AppendLine("      ,convert(nvarchar, AH.APPLYDATE , 111) as APPLYDATE")
        sqlStat.AppendLine("      ,BI.LASTSTEP as LASTSTEP")
        sqlStat.AppendLine("      ,ISNULL(PD2.PRODUCTNAME,'') as PRODUCTCODE")
        sqlStat.AppendLine("      ,ISNULL(PD3.PRODUCTNAME,'') as TWOAGOPRODUCT")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST AH ") '承認履歴
        sqlStat.AppendLine("  INNER JOIN COS0022_APPROVAL AP") '承認設定マスタ
        sqlStat.AppendLine("    ON  AP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AP.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AP.EVENTCODE    = AH.EVENTCODE")
        sqlStat.AppendLine("   AND  AP.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("   AND  AP.STEP         = AH.STEP")
        sqlStat.AppendLine("   AND  AP.USERID       = @USERID")
        sqlStat.AppendLine("   AND  AP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  AP.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  AP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  INNER JOIN ") 'ブレーカー(関連付け)
        sqlStat.AppendLine("     (SELECT BI2.BRID ")
        sqlStat.AppendLine("            ,BI2.SUBID ")
        sqlStat.AppendLine("            ,BI2.LINKID ")
        sqlStat.AppendLine("            ,(SELECT BI3.APPLYID ")
        sqlStat.AppendLine("                FROM GBT0001_BR_INFO BI3 ")
        sqlStat.AppendLine("               WHERE BI3.BRID   = BI2.BRID ")
        sqlStat.AppendLine("                 AND BI3.SUBID  = BI2.SUBID ")
        sqlStat.AppendLine("                 AND BI3.TYPE   = 'INFO' ")
        sqlStat.AppendLine("                 AND BI3.LINKID = BI2.LINKID ")
        sqlStat.AppendLine("             ) AS APPLYID ")
        sqlStat.AppendLine("            ,(SELECT BI4.LASTSTEP ")
        sqlStat.AppendLine("                FROM GBT0001_BR_INFO BI4 ")
        sqlStat.AppendLine("               WHERE BI4.BRID   = BI2.BRID ")
        sqlStat.AppendLine("                 AND BI4.SUBID  = BI2.SUBID ")
        sqlStat.AppendLine("                 AND BI4.TYPE   = 'INFO' ")
        sqlStat.AppendLine("                 AND BI4.LINKID = BI2.LINKID ")
        sqlStat.AppendLine("             ) AS LASTSTEP ")
        sqlStat.AppendLine("    FROM ")
        sqlStat.AppendLine("      (SELECT BISS.BRID")
        sqlStat.AppendLine("            , BISS.SUBID")
        sqlStat.AppendLine("            , MAX(BISS.LINKID) AS LINKID")
        sqlStat.AppendLine("        FROM GBT0001_BR_INFO BISS")
        sqlStat.AppendLine("       WHERE BISS.TYPE   = 'INFO'")
        sqlStat.AppendLine("         AND BISS.BRTYPE = '" & C_BRTYPE.REPAIR & "'")
        sqlStat.AppendLine("       GROUP BY BISS.BRID,BISS.SUBID")
        sqlStat.AppendLine("     ) BI2")
        sqlStat.AppendLine("     ) BI")
        sqlStat.AppendLine("    ON  BI.APPLYID      = AH.APPLYID")
        'sqlStat.AppendLine("   AND  BI.LASTSTEP     = AH.STEP")
        'sqlStat.AppendLine("   AND  BI.TYPE         = 'INFO'")
        'sqlStat.AppendLine("   AND  BI.STYMD       <= @STYMD")
        'sqlStat.AppendLine("   AND  BI.ENDYMD      >= @ENDYMD")
        'sqlStat.AppendLine("   AND  BI.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  INNER JOIN GBT0002_BR_BASE BS") 'ブレーカー(基本)
        sqlStat.AppendLine("    ON  BS.BRID         = BI.BRID")
        sqlStat.AppendLine("   AND  BS.BRBASEID     = BI.LINKID")
        sqlStat.AppendLine("   AND  BS.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  BS.ENDYMD      >= @ENDYMD")
        'sqlStat.AppendLine("   AND  BS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MIN(STEP) AS STEP")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS <= '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> '" & CONST_FLAG_YES & "' ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH2 ")
        sqlStat.AppendLine("    ON  AH2.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH2.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH2.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN ( ")
        sqlStat.AppendLine("  SELECT APPLYID,MAPID,SUBCODE,MAX(STEP) AS STEP ")
        sqlStat.AppendLine("  FROM COT0002_APPROVALHIST ")
        sqlStat.AppendLine("  WHERE STATUS  > '" & C_APP_STATUS.REVISE & "' ")
        sqlStat.AppendLine("    AND DELFLG <> '" & CONST_FLAG_YES & "' ")
        sqlStat.AppendLine("  GROUP BY APPLYID,MAPID,SUBCODE ) AS AH3 ")
        sqlStat.AppendLine("    ON  AH3.APPLYID      = AH.APPLYID")
        sqlStat.AppendLine("   AND  AH3.MAPID        = AH.MAPID")
        sqlStat.AppendLine("   AND  AH3.SUBCODE      = AH.SUBCODE")
        sqlStat.AppendLine("  LEFT JOIN  COT0002_APPROVALHIST AH4 ")
        sqlStat.AppendLine("    ON AH3.APPLYID = AH4.APPLYID ")
        sqlStat.AppendLine("   AND AH3.STEP    = AH4.STEP ")
        sqlStat.AppendLine("   AND AH4.DELFLG <> '" & CONST_FLAG_YES & "'")
        sqlStat.AppendLine("  LEFT JOIN COS0005_USER US") 'APPROVER名称用JOIN
        sqlStat.AppendLine("    ON  US.USERID       = AH4.APPROVERID")
        sqlStat.AppendLine("   AND  US.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  US.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  US.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'SHIPPER名称用JOIN
        sqlStat.AppendLine("    ON  SP.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  SP.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  SP.CUSTOMERCODE = BS.SHIPPER")
        sqlStat.AppendLine("   AND  SP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  SP.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  SP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CN") 'CONSIGNEE名称用JOIN
        sqlStat.AppendLine("    ON  CN.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  CN.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  CN.CUSTOMERCODE = BS.CONSIGNEE")
        sqlStat.AppendLine("   AND  CN.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  CN.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  CN.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  CN.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
        'sqlStat.AppendLine("   AND  PD.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PD.PRODUCTCODE  = BS.PRODUCTCODE")
        sqlStat.AppendLine("   AND  PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  PD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD.ENABLED      =  @ENABLED")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTL") 'PORT名称用JOIN
        sqlStat.AppendLine("    ON  PTL.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PTL.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PTL.PORTCODE     = BS.LOADPORT1")
        sqlStat.AppendLine("   AND  PTL.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  PTL.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  PTL.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PTD") 'PORT名称用JOIN
        sqlStat.AppendLine("    ON  PTD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PTD.COUNTRYCODE  = BS.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("   AND  PTD.PORTCODE     = BS.DISCHARGEPORT1")
        sqlStat.AppendLine("   AND  PTD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  PTD.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  PTD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") '承認対象名称用JOIN
        sqlStat.AppendLine("    ON  FV1.COMPCODE     = '" & GBC_COMPCODE_D & "'")
        sqlStat.AppendLine("   AND  FV1.SYSCODE      = '" & C_SYSCODE_GB & "'")
        sqlStat.AppendLine("   AND  FV1.CLASS        = 'APPROVALTYPE'")
        sqlStat.AppendLine("   AND  FV1.KEYCODE      = AP.APPROVALTYPE")
        sqlStat.AppendLine("   AND  FV1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  FV1.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  FV1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2") '承認否認名称用JOIN
        sqlStat.AppendLine("    ON  FV2.COMPCODE     = '" & GBC_COMPCODE_D & "'")
        sqlStat.AppendLine("   AND  FV2.SYSCODE      = '" & C_SYSCODE_GB & "'")
        sqlStat.AppendLine("   AND  FV2.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FV2.KEYCODE      = AH.STATUS")
        sqlStat.AppendLine("   AND  FV2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  FV2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  FV2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0003_DEPOT DP") 'DEPOT名称用JOIN
        sqlStat.AppendLine("    ON  DP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  DP.DEPOTCODE    = BS.DEPOTCODE")
        sqlStat.AppendLine("   AND  DP.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  DP.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  DP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0006_TANK TK") 'TANK用JOIN
        sqlStat.AppendLine("    ON  TK.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  TK.TANKNO       = BS.TANKNO")
        sqlStat.AppendLine("   AND  TK.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  TK.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  TK.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV3") 'TANKUSAGE名称用JOIN
        sqlStat.AppendLine("    ON  FV3.COMPCODE     = '" & GBC_COMPCODE_D & "'")
        sqlStat.AppendLine("   AND  FV3.SYSCODE      = '" & C_SYSCODE_GB & "'")
        sqlStat.AppendLine("   AND  FV3.CLASS        = 'USAGE'")
        sqlStat.AppendLine("   AND  FV3.KEYCODE      = TK.REPAIRSTAT")
        sqlStat.AppendLine("   AND  FV3.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  FV3.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  FV3.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OB")
        sqlStat.AppendLine("    ON  OB.ORDERNO      = BS.LASTORDERNO")
        sqlStat.AppendLine("   AND  OB.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  OB.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  OB.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBT0002_BR_BASE BB2")
        sqlStat.AppendLine("    ON  BB2.BRID         = OB.BRID")
        sqlStat.AppendLine("   AND  BB2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  BB2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  BB2.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD2") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PD2.PRODUCTCODE  = BB2.PRODUCTCODE")
        sqlStat.AppendLine("   AND  PD2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  PD2.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  PD2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD2.ENABLED      =  @ENABLED")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD3") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD3.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PD3.PRODUCTCODE  = BS.TWOAGOPRODUCT")
        sqlStat.AppendLine("   AND  PD3.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  PD3.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  PD3.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD3.ENABLED      =  @ENABLED")

        sqlStat.AppendLine("  LEFT JOIN ( ")

        sqlStat.AppendLine("SELECT BIS.BRID")
        sqlStat.AppendLine("      ,BIS.SUBID")
        sqlStat.AppendLine("      ,Convert(NVARCHAR, Convert(money, SUM(BVS.USD)), 1) AS TOTALCOST")
        sqlStat.AppendLine("      ,Convert(NVARCHAR, Convert(money, SUM(BVS.APPROVEDUSD)), 1) AS TOTALAPPROVE")

        sqlStat.AppendLine("  FROM ")
        sqlStat.AppendLine("     (SELECT BIS2.BRID")
        sqlStat.AppendLine("            ,BIS2.SUBID")
        sqlStat.AppendLine("            ,MAX(BIS2.LINKID) AS LINKID")
        sqlStat.AppendLine("        FROM GBT0001_BR_INFO BIS2")
        sqlStat.AppendLine("       WHERE BIS2.TYPE   <> 'INFO'")
        sqlStat.AppendLine("         AND BIS2.BRTYPE =  '" & C_BRTYPE.REPAIR & "'")
        sqlStat.AppendLine("       GROUP BY BIS2.BRID,BIS2.SUBID")
        sqlStat.AppendLine("     ) BIS")
        sqlStat.AppendLine("  LEFT JOIN GBT0003_BR_VALUE BVS")
        sqlStat.AppendLine("         ON BVS.BRID  = BIS.BRID")
        sqlStat.AppendLine("        AND BVS.BRVALUEID = BIS.LINKID")
        sqlStat.AppendLine("GROUP BY BIS.BRID,BIS.SUBID")
        sqlStat.AppendLine(") TI ")

        sqlStat.AppendLine("    ON  TI.BRID     = BS.BRID ")
        sqlStat.AppendLine("   AND  TI.SUBID    = BI.SUBID ")
        sqlStat.AppendLine(" WHERE AH.DELFLG        <> @DELFLG")
        sqlStat.AppendLine("   AND AH.COMPCODE       = @COMPCODE")
        sqlStat.AppendLine("   AND AH.MAPID          = @MAPID")
        sqlStat.AppendLine("   AND AH.EVENTCODE      = @EVENTCODE")

        If Me.hdnStYMD.Value <> "" AndAlso Me.hdnEndYMD.Value <> "" Then
            sqlStat.AppendLine("   AND (convert(nvarchar, AH.APPLYDATE , 111)  BETWEEN  @APSTYMD  AND  @APENDYMD )")
        End If

        If Me.hdnTankNo.Value <> "" Then
            'TankNo
            sqlStat.AppendLine("   AND BS.TANKNO       = @TANKNO")
        End If

        If Me.hdnLastCargo.Value <> "" Then
            'Product
            sqlStat.AppendLine("   AND  (BB2.PRODUCTCODE   = @LASTCARGO OR  BS.TWOAGOPRODUCT   = @LASTCARGO)")
        End If

        If Me.hdnLocation.Value <> "" Then
            'Location
            sqlStat.AppendLine("   AND  DP.LOCATION   = @LOCATION")
        End If

        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" WHERE(DELFLG = '" & CONST_FLAG_NO & "' OR (DELFLG = '" & CONST_FLAG_YES & "' AND STEP = '" & C_APP_FIRSTSTEP & "'))")
        sqlStat.AppendLine("   AND EXISTS (SELECT 1 ")
        sqlStat.AppendLine("                 FROM GBT0001_BR_INFO BIST ")
        sqlStat.AppendLine("                WHERE BIST.BRID    = TBL.BRID")
        sqlStat.AppendLine("                  AND BIST.TYPE    = 'INFO'")
        sqlStat.AppendLine("                  AND BIST.DELFLG  = '" & CONST_FLAG_NO & "')")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@MAPID", SqlDbType.NVarChar).Value = "GBT00012"
                .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@EVENTCODE", SqlDbType.NVarChar).Value = C_BRREVENT.APPLY

                If Me.hdnStYMD.Value <> "" AndAlso Me.hdnEndYMD.Value <> "" Then
                    .Add("@APSTYMD", System.Data.SqlDbType.Date).Value = Me.hdnStYMD.Value
                    .Add("@APENDYMD", System.Data.SqlDbType.Date).Value = Me.hdnEndYMD.Value
                End If

                If Me.hdnTankNo.Value <> "" Then
                    .Add("@TANKNO", SqlDbType.NVarChar).Value = Me.hdnTankNo.Value
                End If
                If Me.hdnLastCargo.Value <> "" Then
                    .Add("@LASTCARGO", SqlDbType.NVarChar).Value = Me.hdnLastCargo.Value
                End If
                If Me.hdnLocation.Value <> "" Then
                    .Add("@LOCATION", SqlDbType.NVarChar).Value = Me.hdnLocation.Value
                End If
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dt)
            End Using
        End Using

        Dim retDt As DataTable = CreateDataTable()
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim colNameList As New List(Of String)
            For Each colOb As DataColumn In dt.Columns
                If retDt.Columns.Contains(colOb.ColumnName) Then
                    colNameList.Add(colOb.ColumnName)
                End If
            Next
            For Each readDr As DataRow In dt.Rows
                '同一カラム名を単純転送
                Dim writeDr As DataRow = retDt.NewRow
                For Each colName In colNameList
                    writeDr.Item(colName) = readDr.Item(colName)
                Next
                '案分対象の費用項目を持つか判定
                If readDr.Item("BASEVALUEFLG").Equals("1") Then
                    Dim childRows = From item In dt Where item("BRID").Equals(readDr.Item("BRID")) _
                                                  AndAlso item("DELFLG").Equals(CONST_FLAG_YES)
                    If childRows.Any Then
                        writeDr.Item("HASCHILD") = "1"
                    End If
                End If
                retDt.Rows.Add(writeDr)
            Next
        End If
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
                dt.Rows(i)("Select") = DataCnt
            End If

        Next

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If

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
        COA0013TableObject.VARI = "Default"
        COA0013TableObject.SRCDATA = listData
        COA0013TableObject.TBLOBJ = Me.WF_LISTAREA
        COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.LEVENT = "ondblclick"
        COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.USERSORTOPT = 0 '行開閉がある為ソートさせない
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.APPLYチェックがついているチェックボックスオブジェクトをチェック状態にする
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            '申請チェックボックスの加工
            For Each targetCheckBoxId As String In {"CHECK"} '複数チェックボックスを配置している場合は配列に追加


                Dim targetCheckBoxLineCnt = (From dr As DataRow In listData
                                             Where Convert.ToString(dr.Item(targetCheckBoxId)) <> ""
                                             Select Convert.ToInt32(dr.Item("LINECNT")))
                For Each lineCnt As Integer In targetCheckBoxLineCnt
                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & targetCheckBoxId & lineCnt.ToString
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        chkObj.Checked = True
                    End If
                Next
            Next
            'チェックボックス使用可否制御
            Dim targetDisableCheck = (From dr As DataRow In listData
                                      Where Not {C_APP_STATUS.APPLYING, C_APP_STATUS.REVISE}.Contains(Convert.ToString(dr.Item("STATUS")).Trim)
                                      Select Convert.ToInt32(dr.Item("LINECNT")))
            If targetDisableCheck.Any Then
                For Each lineCnt As Integer In targetDisableCheck
                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & "CHECK" & lineCnt.ToString
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        chkObj.Enabled = False
                    End If
                Next
            End If

            'マイナス金額の強調表示
            Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
            Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
            Dim divDrContL As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DL")
            Dim tblContL As Table = DirectCast(divDrContL.Controls(0), Table)
            Dim minusAmountRows = (From dr As DataRow In listData
                                   Where Convert.ToString(dr.Item("DELFLG")) = CONST_FLAG_YES)
            If minusAmountRows.Any Then
                For Each minusAmountRow In minusAmountRows
                    Dim rowIndex As Integer = listData.Rows.IndexOf(minusAmountRow)
                    tblCont.Rows(rowIndex).CssClass = "minusDiscount"
                    tblContL.Rows(rowIndex).CssClass = "minusDiscount"
                Next
            End If

        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

    End Sub
    ''' <summary>
    ''' 退避した一覧テキスト保存のデータ受け渡し用データテーブル作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateDataTable() As DataTable
        Dim retDt As New DataTable
        With retDt.Columns
            '共通項目
            .Add("LINECNT", GetType(Integer))              'DBの固定フィールド
            .Add("OPERATION", GetType(String))             'DBの固定フィールド
            .Add("TIMSTP", GetType(String))                'DBの固定フィールド
            .Add("SELECT", GetType(Integer))               'DBの固定フィールド
            .Add("HIDDEN", GetType(Integer))
            '個別項目
            .Add("BRID", GetType(String))                  'ブレーカーID
            .Add("BRBASEID", GetType(String))              'ブレーカー基本情報ID
            .Add("SUBID", GetType(String))                 'サブID
            .Add("LINKID", GetType(String))                '個別ID
            .Add("STYMD", GetType(String))                 '有効開始日
            .Add("ENDYMD", GetType(String))                '有効終了日
            .Add("AGENTORGANIZER", GetType(String))        'オーガナイザーエージェント
            .Add("AGENTPOL1", GetType(String))             '発１エージェント
            .Add("AGENTPOL2", GetType(String))             '発２エージェント
            .Add("AGENTPOD1", GetType(String))             '着１エージェント
            .Add("AGENTPOD2", GetType(String))             '着２エージェント
            .Add("HASREMARK", GetType(String))             '備考
            .Add("DELFLG", GetType(String))                '削除フラグ
            .Add("APPROVALOBJECT", GetType(String))        '承認対象(通常、代行、SKIP)
            .Add("APPROVALORREJECT", GetType(String))      '承認or否認
            .Add("CHECK", GetType(String))                 'チェック
            .Add("APPLYID", GetType(String))               '申請ID
            .Add("APPLICANTID", GetType(String))           '申請者
            .Add("STEP", GetType(String))                  'ステップ
            .Add("STATUS", GetType(String))                'ステータス
            .Add("CURSTEP", GetType(String))               '承認ステップ
            .Add("STEPSTATE", GetType(String))             'ステップ状況
            .Add("APPROVALTYPE", GetType(String))          '承認区分

            .Add("APPROVERID", GetType(String))
            .Add("TOTALCOST", GetType(String))
            .Add("TOTALAPPROVE", GetType(String))

            .Add("COUNTRYORGANIZER", GetType(String))      'オーガナイザー国コード
            .Add("ORDERNO", GetType(String))               '受注番号
            .Add("TANKNO", GetType(String))                'タンク番号
            .Add("DEPOTCODE", GetType(String))             'デポコード
            .Add("DEPOTNAME", GetType(String))             'デポ名
            .Add("LOCATION", GetType(String))              'ロケーション
            .Add("TANKUSAGE", GetType(String))             'タンク使用

            .Add("SHOWTANK", GetType(String))              'Showボタン
            .Add("SHOWHIDE", GetType(String))              '表示非表示切り替え
            .Add("BASEVALUEFLG", GetType(String))          '親Flag
            .Add("HASCHILD", GetType(String)).DefaultValue = ""              '子Flag

            .Add("APPLYDATE", GetType(String))              '申請日

            .Add("LASTSTEP", GetType(String))              'ラストステップ

            .Add("PRODUCTCODE", GetType(String))           '積載品
            .Add("TWOAGOPRODUCT", GetType(String))         '前２積載品
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

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If

        Dim selectedRow As DataRow = dt.Rows(rowId)
        Me.hdnSelectedBrId.Value = Convert.ToString(selectedRow.Item("BRID"))
        Me.hdnSelectedStYMD.Value = Convert.ToString(selectedRow.Item("STYMD"))
        Me.hdnSelectedEndYMD.Value = Convert.ToString(selectedRow.Item("ENDYMD"))
        Me.hdnSelectedOrderNo.Value = Convert.ToString(selectedRow.Item("ORDERNO"))
        Me.hdnSelectedTankNo.Value = Convert.ToString(selectedRow.Item("TANKNO"))
        Me.hdnSelectedDepoCode.Value = Convert.ToString(selectedRow.Item("DEPOTCODE"))

        Me.hdnApplyId.Value = Convert.ToString(selectedRow.Item("APPLYID"))
        Me.hdnStep.Value = Convert.ToString(selectedRow.Item("STEP"))
        Me.hdnLastStep.Value = Convert.ToString(selectedRow.Item("LASTSTEP"))

        Me.hdnXMLsaveFileRet.Value = hdnXMLsaveFile.Value
        If Convert.ToString(Trim(Convert.ToString(selectedRow.Item("STATUS")))) = C_APP_STATUS.REVISE Then
            Me.hdnCorrection.Value = "1"
            Me.hdnDenial.Value = ""
        ElseIf Convert.ToString(Trim(Convert.ToString(selectedRow.Item("STATUS")))) = C_APP_STATUS.REJECT Then
            Me.hdnCorrection.Value = ""
            Me.hdnDenial.Value = "1"
        Else
            Me.hdnCorrection.Value = ""
            Me.hdnDenial.Value = ""
        End If
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = "全て"
            Else
                Me.txtApprovalObj.Text = "All"
            End If
        End If
        Me.hdnApprovalObj.Value = txtApprovalObj.Text
        Me.hdnApplicantObj.Value = txtApplicantObj.Text
        Me.hdnSelectedStep.Value = Convert.ToString(selectedRow.Item("STEP"))

        If Convert.ToString(Trim(Convert.ToString(selectedRow.Item("DELFLG")))) = CONST_FLAG_YES Then
            Me.hdnSelectedDelFlg.Value = CONST_FLAG_YES
        Else
            Me.hdnSelectedDelFlg.Value = ""
        End If

        Me.hdnStatus.Value = Convert.ToString(Trim(Convert.ToString(selectedRow.Item("STATUS"))))

        Me.hdnSubId.Value = Convert.ToString(Trim(Convert.ToString(selectedRow.Item("SUBID"))))
        Me.hdnLinkId.Value = Convert.ToString(Trim(Convert.ToString(selectedRow.Item("LINKID"))))

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = "GB_ShowRepairBrDetail"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        HttpContext.Current.Session("MAPmapid") = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "GB_Default"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
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
        If Me.txtApprovalObj.Text.Trim <> "" OrElse Me.txtApplicantObj.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            'If isFillterOff = False AndAlso Not (Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All") Then
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtApprovalObj.Text.Trim = "全て" OrElse Me.txtApprovalObj.Text.Trim = "All" OrElse Me.txtApprovalObj.Text.Trim = "" OrElse Convert.ToString(dr("APPROVALOBJECT")).Trim.Equals(Me.txtApprovalObj.Text.Trim)) _
                   AndAlso (Me.txtApplicantObj.Text.Trim = "" OrElse Convert.ToString(dr("APPLICANTID")).Trim.Equals(Me.txtApplicantObj.Text.Trim))
                   ) Then
                    dr.Item("HIDDEN") = 1
                End If

            End If

            '履歴は非表示
            If dr.Item("DELFLG").Equals(CONST_FLAG_YES) Then
                dr.Item("HIDDEN") = 1
            End If

            '現在開いているタンクは閉じる
            If dr.Item("SHOWHIDE").Equals("SHOW") Then
                dr.Item("SHOWHIDE") = "HIDE"
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
        Me.txtApprovalObj.Focus()

    End Sub
    ''' <summary>
    ''' 否認ボタン押下時処理
    ''' </summary>
    Public Sub btnReject_Click()
        Dim COA0021ListTable As New COA0021ListTable
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval
        Dim dt As DataTable = CreateDataTable()

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

        Dim errNo As String = ""
        'CHECKチェックボックスがチェック済の全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("CHECK")) = "on")
        Dim checkedDt As DataTable = Nothing
        If q.Any = True Then
            checkedDt = q.CopyToDataTable
        Else
            checkedDt = dt.Clone
        End If
        For Each checkedDr As DataRow In checkedDt.Rows 'For i As Integer = 0 To dt.Rows.Count - 1

            '否認登録
            COA0032Apploval.I_COMPCODE = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
            COA0032Apploval.I_APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            COA0032Apploval.I_STEP = Convert.ToString(checkedDr.Item("STEP"))
            COA0032Apploval.COA0032setDenial()
            If COA0032Apploval.O_ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
                'Return
                If errNo = "" Then
                    errNo = COA0032Apploval.O_ERR
                End If
            End If

            'メール
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = "BRR_Rejected"
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = Convert.ToString(checkedDr.Item("BRID"))
            GBA00009MailSendSet.BRSUBID = Convert.ToString(checkedDr.Item("SUBID"))
            GBA00009MailSendSet.BRBASEID = Convert.ToString(checkedDr.Item("BRBASEID"))
            GBA00009MailSendSet.APPLYID = Convert.ToString(checkedDr.Item("APPLYID"))
            GBA00009MailSendSet.LASTSTEP = Convert.ToString(checkedDr.Item("LASTSTEP"))
            GBA00009MailSendSet.GBA00009setMailToRepBR()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                'CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                'Return
                If errNo = "" Then
                    errNo = GBA00009MailSendSet.ERR
                End If
            End If

            '繰り上げ
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                Dim brId As String = Convert.ToString(checkedDr.Item("BRID"))
                Dim subId As String = Convert.ToString(checkedDr.Item("SUBID"))
                'DB登録実行
                Dim entDate As Date = Date.Now
                Dim tran As SqlTransaction = sqlCon.BeginTransaction() 'トランザクション開始
                InsertBreaker(subId, brId, sqlCon, tran, entDate)
                tran.Commit()
                sqlCon.Close()
            End Using

        Next

        '絞り込み
        If Me.txtApprovalObj.Text = "" Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.txtApprovalObj.Text = "全て"
            Else
                Me.txtApprovalObj.Text = "All"
            End If
        End If
        Me.hdnApprovalObj.Value = Me.txtApprovalObj.Text
        Me.hdnApplicantObj.Value = Me.txtApplicantObj.Text

        If errNo <> "" Then
            CommonFunctions.ShowMessage(errNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.REJECTSUCCESS

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        HttpContext.Current.Session("MAPmapid") = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "GB_Default"
        COA0012DoUrl.MAPIDP = Convert.ToString(HttpContext.Current.Session("MAPmapid"))
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

    End Sub
    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00012REPAIR Then
            '単票画面の場合
            Dim prevObj As GBT00012REPAIR = DirectCast(Page.PreviousPage, GBT00012REPAIR)
            Dim tmpCont As Control = prevObj.FindControl("hdnXMLsaveFileRet")

            If tmpCont IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpCont, HiddenField)
                Me.hdnXMLsaveFileRet.Value = tmphdn.Value
            End If

            Dim tmpStYmd As Control = prevObj.FindControl("hdnStYMD")

            If tmpStYmd IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpStYmd, HiddenField)
                Me.hdnStYMD.Value = FormatDateYMD(tmphdn.Value, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpEndYmd As Control = prevObj.FindControl("hdnEndYMD")

            If tmpEndYmd IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpEndYmd, HiddenField)
                Me.hdnEndYMD.Value = FormatDateYMD(tmphdn.Value, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpAppObj As Control = prevObj.FindControl("hdnApprovalObj")

            If tmpAppObj IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpAppObj, HiddenField)
                Me.hdnApprovalObj.Value = tmphdn.Value
            End If

            Dim tmpTankNo As Control = prevObj.FindControl("hdnGBT00012STankNo")

            If tmpTankNo IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpTankNo, HiddenField)
                Me.hdnTankNo.Value = tmphdn.Value
            End If

            Dim tmpLastCargo As Control = prevObj.FindControl("hdnLastCargo")

            If tmpLastCargo IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpLastCargo, HiddenField)
                Me.hdnLastCargo.Value = tmphdn.Value
            End If

            Dim tmpLocation As Control = prevObj.FindControl("hdnLocation")

            If tmpLocation IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpLocation, HiddenField)
                Me.hdnLocation.Value = tmphdn.Value
            End If

            '画面ビューID保持
            Dim tmpPrevViewIDObj As HiddenField = DirectCast(prevObj.FindControl("hdnPrevViewID"), HiddenField)
            If tmpPrevViewIDObj IsNot Nothing Then
                Me.hdnPrevViewID.Value = tmpPrevViewIDObj.Value
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00012APPROVAL Then

            Dim prevObj As GBT00012APPROVAL = DirectCast(Page.PreviousPage, GBT00012APPROVAL)
            Dim tmpCont As Control = prevObj.FindControl("hdnMsgId")

            If tmpCont IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpCont, HiddenField)
                Me.hdnMsgId.Value = tmphdn.Value
            End If

            Dim tmpAppObj As Control = prevObj.FindControl("hdnApprovalObj")

            If tmpAppObj IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpAppObj, HiddenField)
                Me.hdnApprovalObj.Value = tmphdn.Value
            End If

            Dim tmpStYmd As Control = prevObj.FindControl("hdnStYMD")

            If tmpStYmd IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpStYmd, HiddenField)
                Me.hdnStYMD.Value = FormatDateYMD(tmphdn.Value, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpEndYmd As Control = prevObj.FindControl("hdnEndYMD")

            If tmpEndYmd IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpEndYmd, HiddenField)
                Me.hdnEndYMD.Value = FormatDateYMD(tmphdn.Value, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpTankNo As Control = prevObj.FindControl("hdnTankNo")

            If tmpTankNo IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpTankNo, HiddenField)
                Me.hdnTankNo.Value = tmphdn.Value
            End If

            Dim tmpLastCargo As Control = prevObj.FindControl("hdnLastCargo")

            If tmpLastCargo IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpLastCargo, HiddenField)
                Me.hdnLastCargo.Value = tmphdn.Value
            End If

            Dim tmpLocation As Control = prevObj.FindControl("hdnLocation")

            If tmpLocation IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpLocation, HiddenField)
                Me.hdnLocation.Value = tmphdn.Value
            End If

            '画面ビューID保持
            Dim tmpPrevViewIDObj As HiddenField = DirectCast(prevObj.FindControl("hdnPrevViewID"), HiddenField)
            If tmpPrevViewIDObj IsNot Nothing Then
                Me.hdnPrevViewID.Value = tmpPrevViewIDObj.Value
            End If

        ElseIf TypeOf Page.PreviousPage Is GBT00012SELECT Then

            Dim prevObj As GBT00012SELECT = DirectCast(Page.PreviousPage, GBT00012SELECT)
            Dim tmpStYmd As Control = prevObj.FindControl("txtStYMD")

            If tmpStYmd IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpStYmd, TextBox)
                Me.hdnStYMD.Value = FormatDateYMD(tmphdn.Text, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpEndYmd As Control = prevObj.FindControl("txtEndYMD")

            If tmpEndYmd IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpEndYmd, TextBox)
                Me.hdnEndYMD.Value = FormatDateYMD(tmphdn.Text, GBA00003UserSetting.DATEFORMAT)
            End If

            Dim tmpTankNo As Control = prevObj.FindControl("txtTankNo")

            If tmpTankNo IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpTankNo, TextBox)
                Me.hdnTankNo.Value = tmphdn.Text
            End If

            Dim tmpLastCargo As Control = prevObj.FindControl("txtLastCargo")

            If tmpLastCargo IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpLastCargo, TextBox)
                Me.hdnLastCargo.Value = tmphdn.Text
            End If

            Dim tmpLocation As Control = prevObj.FindControl("txtLocation")

            If tmpLocation IsNot Nothing Then
                Dim tmphdn As TextBox = DirectCast(tmpLocation, TextBox)
                Me.hdnLocation.Value = tmphdn.Text
            End If

            Dim tmplst As ListBox = DirectCast(prevObj.FindControl("lbRightList"), ListBox)
            Dim selectedViewId As String = "Default"
            If tmplst IsNot Nothing Then
                If tmplst.SelectedItem Is Nothing AndAlso
                      tmplst.Items.Count > 0 Then
                    selectedViewId = tmplst.Items(0).Value
                ElseIf tmplst.SelectedItem IsNot Nothing Then
                    selectedViewId = tmplst.SelectedItem.Value
                End If
            End If
            Me.hdnPrevViewID.Value = selectedViewId '前画面より選択した画面レイアウト

        End If
    End Sub
    ''' <summary>
    ''' 承認リストアイテムを設定
    ''' </summary>
    Private Function SetApprovalObjListItem(selectedValue As String) As String
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Dim retCode As String = C_MESSAGENO.NORMAL

        'リストクリア
        Me.lbApprovalObj.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "APPROVALDISPTYPE"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbApprovalObj
        Else
            COA0017FixValue.LISTBOX2 = Me.lbApprovalObj
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbApprovalObj = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbApprovalObj = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If
        Else
            retCode = COA0017FixValue.ERR
        End If
        Return retCode
    End Function
    ''' <summary>
    ''' ブレーカー情報保持クラス
    ''' </summary>
    <Serializable>
    Private Class BreakerData
        ''' <summary>
        ''' ブレーカー番号
        ''' </summary>
        ''' <returns></returns>
        Public Property BrId As String = ""
        ''' <summary>
        ''' タンク数
        ''' </summary>
        ''' <returns></returns>
        Public Property NoOfTanks As String = ""
        ''' <summary>
        ''' 代理店名
        ''' </summary>
        ''' <returns></returns>
        Public Property Office As String = ""
        ''' <summary>
        ''' 代理店担当
        ''' </summary>
        ''' <returns></returns>
        Public Property SalesPic As String = ""
        ''' <summary>
        ''' BLNOST
        ''' </summary>
        ''' <returns></returns>
        Public Property JotBlNoSt As String = ""
        ''' <summary>
        ''' BLNO
        ''' </summary>
        ''' <returns></returns>
        Public Property JotBlNo As String = ""
        ''' <summary>
        ''' 総額(BRより求めた総額)※コピー数を掛けた結果ではない
        ''' </summary>
        ''' <returns></returns>
        Public Property BrTotalInvoiced As String = ""
        ''' <summary>
        ''' BR Hireage
        ''' </summary>
        ''' <returns></returns>
        Public Property BrHireage As String = ""
        ''' <summary>
        ''' BR ADUSTMENT
        ''' </summary>
        ''' <returns></returns>
        Public Property BrAdjustment As String = ""
        ''' <summary>
        ''' AMTPRINCIPAL(ここが0以外の場合はTotalInvoiceとして計算)
        ''' </summary>
        ''' <returns></returns>
        Public Property BrAmtPrincipal As String = ""
        ''' <summary>
        ''' ETD1(ブレーカー情報)
        ''' </summary>
        ''' <returns></returns>
        Public Property BrEtd1 As String = ""
        ''' <summary>
        ''' ETA1(ブレーカー情報)
        ''' </summary>
        ''' <returns></returns>
        Public Property BrEta1 As String = ""
        ''' <summary>
        ''' ETD2(ブレーカー情報)
        ''' </summary>
        ''' <returns></returns>
        Public Property BrEtd2 As String = ""
        ''' <summary>
        ''' ETA2(ブレーカー情報)
        ''' </summary>
        ''' <returns></returns>
        Public Property BrEta2 As String = ""
        ''' <summary>
        ''' 3国間輸送(True:3国間,False(Default):2国間)
        ''' </summary>
        ''' <returns></returns>
        Public Property IsTrilateral As Boolean = False
        ''' <summary>
        ''' ブレーカー種類
        ''' </summary>
        ''' <returns></returns>
        Public Property BrType As String = ""
    End Class
    ''' <summary>
    ''' ブレーカー更新処理
    ''' </summary>
    ''' <param name="subId"></param>
    ''' <param name="breakerId"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="tran"></param>
    ''' <param name="entDate"></param>
    Private Sub InsertBreaker(subId As String, breakerId As String, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
        Dim canCloseConnect As Boolean = False
        If entDate = Date.Parse("1900/01/01") Then
            entDate = Date.Now
        End If

        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Dim sqlStat As New StringBuilder
            Dim dt As New DataTable

            'LinkId取得
            sqlStat.Clear()
            sqlStat.AppendLine("SELECT TYPE , LINKID")
            sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
            sqlStat.AppendLine(" WHERE BRID    = @BRID")
            sqlStat.AppendLine("   AND SUBID   = @SUBID")
            sqlStat.AppendLine("   AND DELFLG <> @DELFLG")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                'SQLパラメータの設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                    .Add("@SUBID", SqlDbType.NVarChar).Value = subId
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dt)
                End Using

            End Using

            '削除更新
            'BR_INFO
            sqlStat.Clear()
            sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
            sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
            sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE BRID    = @BRID")
            sqlStat.AppendLine("   AND SUBID   = @SUBID")
            sqlStat.AppendLine("   AND DELFLG <> @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                    .Add("@SUBID", SqlDbType.NVarChar).Value = subId
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                sqlCmd.ExecuteNonQuery()
            End Using

            'BR_BASE
            sqlStat.Clear()
            sqlStat.AppendLine("UPDATE GBT0002_BR_BASE")
            sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
            sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE BRID      = @BRID")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                sqlCmd.ExecuteNonQuery()
            End Using

            'BR_VALUE
            sqlStat.Clear()
            sqlStat.AppendLine("UPDATE GBT0003_BR_VALUE")
            sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
            sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE BRID      = @BRID")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With
                sqlCmd.ExecuteNonQuery()
            End Using

            For i As Integer = 0 To dt.Rows.Count - 1

                'BR_INFO
                sqlStat.Clear()
                sqlStat.AppendLine("INSERT INTO GBT0001_BR_INFO (")
                sqlStat.AppendLine("       BRID")
                sqlStat.AppendLine("      ,SUBID")
                sqlStat.AppendLine("      ,TYPE")
                sqlStat.AppendLine("      ,LINKID")
                sqlStat.AppendLine("      ,STYMD")
                sqlStat.AppendLine("      ,BRTYPE")
                sqlStat.AppendLine("      ,APPLYID")
                sqlStat.AppendLine("      ,LASTSTEP")
                sqlStat.AppendLine("      ,USETYPE")
                sqlStat.AppendLine("      ,REMARK")
                sqlStat.AppendLine("      ,DELFLG")
                sqlStat.AppendLine("      ,INITYMD")
                sqlStat.AppendLine("      ,UPDYMD")
                sqlStat.AppendLine("      ,UPDUSER")
                sqlStat.AppendLine("      ,UPDTERMID")
                sqlStat.AppendLine("      ,RECEIVEYMD")
                sqlStat.AppendLine(" ) ")
                sqlStat.AppendLine("SELECT BRID")
                sqlStat.AppendLine("      ,'S' + RIGHT('00000' + CONVERT(varchar ,  CONVERT(int ,right(SUBID,5))+1), 5) AS SUBID ")
                sqlStat.AppendLine("      ,TYPE")
                sqlStat.AppendLine("      ,TYPE + '-' + RIGHT('00000' + CONVERT(varchar ,  CONVERT(int ,right(LINKID,5))+1), 5) AS LINKID ")
                sqlStat.AppendLine("      ,@STYMD")
                sqlStat.AppendLine("      ,BRTYPE")
                sqlStat.AppendLine("      ,APPLYID")
                sqlStat.AppendLine("      ,LASTSTEP")
                'sqlStat.AppendLine("      ,@LASTSTEP")
                sqlStat.AppendLine("      ,USETYPE")
                sqlStat.AppendLine("      ,REMARK")
                sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'     ") '削除フラグ(0固定)
                sqlStat.AppendLine("      ,@ENTDATE")
                sqlStat.AppendLine("      ,@ENTDATE")
                sqlStat.AppendLine("      ,@UPDUSER")
                sqlStat.AppendLine("      ,@UPDTERMID")
                sqlStat.AppendLine("      ,@RECEIVEYMD")
                sqlStat.AppendLine("  FROM GBT0001_BR_INFO BI")
                sqlStat.AppendLine(" WHERE BRID    = @BRID")
                sqlStat.AppendLine("   AND SUBID   = @SUBID")
                sqlStat.AppendLine("   AND TYPE    = @TYPE")
                sqlStat.AppendLine("   AND LINKID  = @LINKID")

                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                    If tran IsNot Nothing Then
                        sqlCmd.Transaction = tran
                    End If
                    'SQLパラメータの設定
                    With sqlCmd.Parameters
                        .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                        .Add("@SUBID", SqlDbType.NVarChar).Value = subId
                        .Add("@TYPE", SqlDbType.NVarChar).Value = dt.Rows(i).Item("TYPE")
                        .Add("@LINKID", SqlDbType.NVarChar).Value = dt.Rows(i).Item("LINKID")
                        .Add("@STYMD", SqlDbType.Date).Value = entDate
                        '.Add("@APPLYID", SqlDbType.NVarChar).Value = ""
                        '.Add("@LASTSTEP", SqlDbType.NVarChar).Value = C_APP_FIRSTSTEP
                        .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using

                If Convert.ToString(dt.Rows(i).Item("TYPE")) = "INFO" Then

                    'BR_BASE
                    sqlStat.Clear()
                    sqlStat.AppendLine("INSERT INTO GBT0002_BR_BASE (")
                    sqlStat.AppendLine("       BRID")
                    sqlStat.AppendLine("      ,BRBASEID")
                    sqlStat.AppendLine("      ,STYMD")
                    sqlStat.AppendLine("      ,VALIDITYFROM")
                    sqlStat.AppendLine("      ,VALIDITYTO")
                    sqlStat.AppendLine("      ,TERMTYPE")
                    sqlStat.AppendLine("      ,NOOFTANKS")
                    sqlStat.AppendLine("      ,SHIPPER")
                    sqlStat.AppendLine("      ,CONSIGNEE")
                    sqlStat.AppendLine("      ,CARRIER1")
                    sqlStat.AppendLine("      ,CARRIER2")
                    sqlStat.AppendLine("      ,PRODUCTCODE")
                    sqlStat.AppendLine("      ,PRODUCTWEIGHT")
                    sqlStat.AppendLine("      ,RECIEPTCOUNTRY1")
                    sqlStat.AppendLine("      ,RECIEPTPORT1")
                    sqlStat.AppendLine("      ,RECIEPTCOUNTRY2")
                    sqlStat.AppendLine("      ,RECIEPTPORT2")
                    sqlStat.AppendLine("      ,LOADCOUNTRY1")
                    sqlStat.AppendLine("      ,LOADPORT1")
                    sqlStat.AppendLine("      ,LOADCOUNTRY2")
                    sqlStat.AppendLine("      ,LOADPORT2")
                    sqlStat.AppendLine("      ,DISCHARGECOUNTRY1")
                    sqlStat.AppendLine("      ,DISCHARGEPORT1")
                    sqlStat.AppendLine("      ,DISCHARGECOUNTRY2")
                    sqlStat.AppendLine("      ,DISCHARGEPORT2")
                    sqlStat.AppendLine("      ,DELIVERYCOUNTRY1")
                    sqlStat.AppendLine("      ,DELIVERYPORT1")
                    sqlStat.AppendLine("      ,DELIVERYCOUNTRY2")
                    sqlStat.AppendLine("      ,DELIVERYPORT2")
                    sqlStat.AppendLine("      ,VSL1")
                    sqlStat.AppendLine("      ,VOY1")
                    sqlStat.AppendLine("      ,ETD1")
                    sqlStat.AppendLine("      ,ETA1")
                    sqlStat.AppendLine("      ,VSL2")
                    sqlStat.AppendLine("      ,VOY2")
                    sqlStat.AppendLine("      ,ETD2")
                    sqlStat.AppendLine("      ,ETA2")
                    sqlStat.AppendLine("      ,INVOICEDBY")
                    sqlStat.AppendLine("      ,LOADING")
                    sqlStat.AppendLine("      ,STEAMING")
                    sqlStat.AppendLine("      ,TIP")
                    sqlStat.AppendLine("      ,EXTRA")
                    sqlStat.AppendLine("      ,JOTHIREAGE")
                    sqlStat.AppendLine("      ,COMMERCIALFACTOR")
                    sqlStat.AppendLine("      ,AMTREQUEST")
                    sqlStat.AppendLine("      ,AMTPRINCIPAL")
                    sqlStat.AppendLine("      ,AMTDISCOUNT")
                    sqlStat.AppendLine("      ,DEMURTO")
                    sqlStat.AppendLine("      ,DEMURUSRATE1")
                    sqlStat.AppendLine("      ,DEMURUSRATE2")
                    sqlStat.AppendLine("      ,AGENTORGANIZER")
                    sqlStat.AppendLine("      ,AGENTPOL1")
                    sqlStat.AppendLine("      ,AGENTPOL2")
                    sqlStat.AppendLine("      ,AGENTPOD1")
                    sqlStat.AppendLine("      ,AGENTPOD2")
                    sqlStat.AppendLine("      ,APPLYTEXT")
                    sqlStat.AppendLine("      ,COUNTRYORGANIZER")
                    sqlStat.AppendLine("      ,LASTORDERNO")
                    sqlStat.AppendLine("      ,TANKNO")
                    sqlStat.AppendLine("      ,DEPOTCODE")
                    sqlStat.AppendLine("      ,TWOAGOPRODUCT")
                    sqlStat.AppendLine("      ,REMARK")
                    sqlStat.AppendLine("      ,DELFLG")
                    sqlStat.AppendLine("      ,INITYMD ")
                    sqlStat.AppendLine("      ,INITUSER ")
                    sqlStat.AppendLine("      ,UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER ")
                    sqlStat.AppendLine("      ,UPDTERMID ")
                    sqlStat.AppendLine("      ,RECEIVEYMD ")
                    sqlStat.AppendLine(" ) ")
                    sqlStat.AppendLine("SELECT BRID")
                    sqlStat.AppendLine("      ,LEFT(BRBASEID,5) + RIGHT('00000' + CONVERT(varchar ,  CONVERT(int ,right(BRBASEID,5))+1), 5) AS BRBASEID ")
                    sqlStat.AppendLine("      ,@STYMD")
                    sqlStat.AppendLine("      ,VALIDITYFROM")
                    sqlStat.AppendLine("      ,VALIDITYTO")
                    sqlStat.AppendLine("      ,TERMTYPE")
                    sqlStat.AppendLine("      ,NOOFTANKS")
                    sqlStat.AppendLine("      ,SHIPPER")
                    sqlStat.AppendLine("      ,CONSIGNEE")
                    sqlStat.AppendLine("      ,CARRIER1")
                    sqlStat.AppendLine("      ,CARRIER2")
                    sqlStat.AppendLine("      ,PRODUCTCODE")
                    sqlStat.AppendLine("      ,PRODUCTWEIGHT")
                    sqlStat.AppendLine("      ,RECIEPTCOUNTRY1")
                    sqlStat.AppendLine("      ,RECIEPTPORT1")
                    sqlStat.AppendLine("      ,RECIEPTCOUNTRY2")
                    sqlStat.AppendLine("      ,RECIEPTPORT2")
                    sqlStat.AppendLine("      ,LOADCOUNTRY1")
                    sqlStat.AppendLine("      ,LOADPORT1")
                    sqlStat.AppendLine("      ,LOADCOUNTRY2")
                    sqlStat.AppendLine("      ,LOADPORT2")
                    sqlStat.AppendLine("      ,DISCHARGECOUNTRY1")
                    sqlStat.AppendLine("      ,DISCHARGEPORT1")
                    sqlStat.AppendLine("      ,DISCHARGECOUNTRY2")
                    sqlStat.AppendLine("      ,DISCHARGEPORT2")
                    sqlStat.AppendLine("      ,DELIVERYCOUNTRY1")
                    sqlStat.AppendLine("      ,DELIVERYPORT1")
                    sqlStat.AppendLine("      ,DELIVERYCOUNTRY2")
                    sqlStat.AppendLine("      ,DELIVERYPORT2")
                    sqlStat.AppendLine("      ,VSL1")
                    sqlStat.AppendLine("      ,VOY1")
                    sqlStat.AppendLine("      ,ETD1")
                    sqlStat.AppendLine("      ,ETA1")
                    sqlStat.AppendLine("      ,VSL2")
                    sqlStat.AppendLine("      ,VOY2")
                    sqlStat.AppendLine("      ,ETD2")
                    sqlStat.AppendLine("      ,ETA2")
                    sqlStat.AppendLine("      ,INVOICEDBY")
                    sqlStat.AppendLine("      ,LOADING")
                    sqlStat.AppendLine("      ,STEAMING")
                    sqlStat.AppendLine("      ,TIP")
                    sqlStat.AppendLine("      ,EXTRA")
                    sqlStat.AppendLine("      ,JOTHIREAGE")
                    sqlStat.AppendLine("      ,COMMERCIALFACTOR")
                    sqlStat.AppendLine("      ,AMTREQUEST")
                    sqlStat.AppendLine("      ,AMTPRINCIPAL")
                    sqlStat.AppendLine("      ,AMTDISCOUNT")
                    sqlStat.AppendLine("      ,DEMURTO")
                    sqlStat.AppendLine("      ,DEMURUSRATE1")
                    sqlStat.AppendLine("      ,DEMURUSRATE2")
                    sqlStat.AppendLine("      ,AGENTORGANIZER")
                    sqlStat.AppendLine("      ,AGENTPOL1")
                    sqlStat.AppendLine("      ,AGENTPOL2")
                    sqlStat.AppendLine("      ,AGENTPOD1")
                    sqlStat.AppendLine("      ,AGENTPOD2")
                    sqlStat.AppendLine("      ,APPLYTEXT")
                    sqlStat.AppendLine("      ,COUNTRYORGANIZER")
                    sqlStat.AppendLine("      ,LASTORDERNO")
                    sqlStat.AppendLine("      ,TANKNO")
                    sqlStat.AppendLine("      ,DEPOTCODE")
                    sqlStat.AppendLine("      ,TWOAGOPRODUCT")
                    sqlStat.AppendLine("      ,REMARK")
                    sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'     ") '削除フラグ(0固定)
                    sqlStat.AppendLine("      ,INITYMD")
                    sqlStat.AppendLine("      ,INITUSER")
                    sqlStat.AppendLine("      ,@ENTDATE")
                    sqlStat.AppendLine("      ,@UPDUSER")
                    sqlStat.AppendLine("      ,@UPDTERMID")
                    sqlStat.AppendLine("      ,@RECEIVEYMD")
                    sqlStat.AppendLine("  FROM GBT0002_BR_BASE ")
                    sqlStat.AppendLine(" WHERE BRID     = @BRID")
                    sqlStat.AppendLine("   AND BRBASEID = @BRBASEID")

                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                        If tran IsNot Nothing Then
                            sqlCmd.Transaction = tran
                        End If
                        'SQLパラメータの設定
                        With sqlCmd.Parameters
                            .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                            .Add("@BRBASEID", SqlDbType.NVarChar).Value = dt.Rows(i).Item("LINKID")
                            .Add("@STYMD", SqlDbType.Date).Value = entDate
                            .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                            .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                            .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        End With
                        sqlCmd.ExecuteNonQuery()
                    End Using

                Else

                    'BR_VALUE
                    sqlStat.Clear()
                    sqlStat.AppendLine("INSERT INTO GBT0003_BR_VALUE (")
                    sqlStat.AppendLine("       BRID")
                    sqlStat.AppendLine("      ,BRVALUEID")
                    sqlStat.AppendLine("      ,STYMD")
                    sqlStat.AppendLine("      ,DTLPOLPOD")
                    sqlStat.AppendLine("      ,DTLOFFICE")
                    sqlStat.AppendLine("      ,COSTCODE")
                    sqlStat.AppendLine("      ,BASEON")
                    sqlStat.AppendLine("      ,TAX")
                    sqlStat.AppendLine("      ,USD")
                    sqlStat.AppendLine("      ,LOCAL")
                    sqlStat.AppendLine("      ,CONTRACTOR")
                    sqlStat.AppendLine("      ,LOCALRATE")
                    sqlStat.AppendLine("      ,USDRATE")
                    sqlStat.AppendLine("      ,CURRENCYCODE")
                    sqlStat.AppendLine("      ,AGENT")
                    sqlStat.AppendLine("      ,ACTIONID")
                    sqlStat.AppendLine("      ,CLASS1")
                    sqlStat.AppendLine("      ,CLASS2")
                    sqlStat.AppendLine("      ,CLASS3")
                    sqlStat.AppendLine("      ,CLASS4")
                    sqlStat.AppendLine("      ,CLASS5")
                    sqlStat.AppendLine("      ,CLASS6")
                    sqlStat.AppendLine("      ,CLASS7")
                    sqlStat.AppendLine("      ,CLASS8")
                    sqlStat.AppendLine("      ,COUNTRYCODE")
                    sqlStat.AppendLine("      ,REPAIRFLG")
                    sqlStat.AppendLine("      ,APPROVEDUSD")
                    sqlStat.AppendLine("      ,INVOICEDBY")
                    sqlStat.AppendLine("      ,REMARK")
                    sqlStat.AppendLine("      ,DELFLG")
                    sqlStat.AppendLine("      ,INITYMD ")
                    sqlStat.AppendLine("      ,INITUSER ")
                    sqlStat.AppendLine("      ,UPDYMD ")
                    sqlStat.AppendLine("      ,UPDUSER ")
                    sqlStat.AppendLine("      ,UPDTERMID ")
                    sqlStat.AppendLine("      ,RECEIVEYMD ")
                    sqlStat.AppendLine(" ) ")
                    sqlStat.AppendLine("SELECT BRID")
                    sqlStat.AppendLine("      ,LEFT(BRVALUEID,5) + RIGHT('00000' + CONVERT(varchar ,  CONVERT(int ,right(BRVALUEID,5))+1), 5) AS BRVALUEID ")
                    sqlStat.AppendLine("      ,@STYMD")
                    sqlStat.AppendLine("      ,DTLPOLPOD")
                    sqlStat.AppendLine("      ,DTLOFFICE")
                    sqlStat.AppendLine("      ,COSTCODE")
                    sqlStat.AppendLine("      ,BASEON")
                    sqlStat.AppendLine("      ,TAX")
                    sqlStat.AppendLine("      ,USD")
                    sqlStat.AppendLine("      ,LOCAL")
                    sqlStat.AppendLine("      ,CONTRACTOR")
                    sqlStat.AppendLine("      ,LOCALRATE")
                    sqlStat.AppendLine("      ,USDRATE")
                    sqlStat.AppendLine("      ,CURRENCYCODE")
                    sqlStat.AppendLine("      ,AGENT")
                    sqlStat.AppendLine("      ,ACTIONID")
                    sqlStat.AppendLine("      ,CLASS1")
                    sqlStat.AppendLine("      ,CLASS2")
                    sqlStat.AppendLine("      ,CLASS3")
                    sqlStat.AppendLine("      ,CLASS4")
                    sqlStat.AppendLine("      ,CLASS5")
                    sqlStat.AppendLine("      ,CLASS6")
                    sqlStat.AppendLine("      ,CLASS7")
                    sqlStat.AppendLine("      ,CLASS8")
                    sqlStat.AppendLine("      ,COUNTRYCODE")
                    sqlStat.AppendLine("      ,REPAIRFLG")
                    sqlStat.AppendLine("      ,APPROVEDUSD")
                    sqlStat.AppendLine("      ,INVOICEDBY")
                    sqlStat.AppendLine("      ,REMARK")
                    sqlStat.AppendLine("      ,'" & CONST_FLAG_NO & "'     ") '削除フラグ(0固定)
                    sqlStat.AppendLine("      ,INITYMD")
                    sqlStat.AppendLine("      ,INITUSER")
                    sqlStat.AppendLine("      ,@ENTDATE")
                    sqlStat.AppendLine("      ,@UPDUSER")
                    sqlStat.AppendLine("      ,@UPDTERMID")
                    sqlStat.AppendLine("      ,@RECEIVEYMD")
                    sqlStat.AppendLine("  FROM GBT0003_BR_VALUE ")
                    sqlStat.AppendLine(" WHERE BRID      = @BRID")
                    sqlStat.AppendLine("   AND BRVALUEID = @BRVALUEID")

                    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                        If tran IsNot Nothing Then
                            sqlCmd.Transaction = tran
                        End If
                        'SQLパラメータの設定
                        With sqlCmd.Parameters
                            .Add("@BRID", SqlDbType.NVarChar).Value = breakerId
                            .Add("@BRVALUEID", SqlDbType.NVarChar).Value = dt.Rows(i).Item("LINKID")
                            .Add("@STYMD", SqlDbType.Date).Value = entDate
                            .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                            .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                            .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                        End With
                        sqlCmd.ExecuteNonQuery()
                    End Using

                End If
            Next

        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True AndAlso sqlCon IsNot Nothing Then
                sqlCon.Dispose()
                sqlCon = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 一覧タンク表示押下時
    ''' </summary>
    Public Sub lblListShowTank_Click()
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
        Dim clickedRow As DataRow = (From item In dt Where Convert.ToString(item("LINECNT")) = currentRowNum).FirstOrDefault

        'ありえないが対象の行番号のデータがない場合
        If clickedRow Is Nothing Then
            Return 'そのまま終了
        End If

        '選択された行に紐づくタンクを表示
        Dim brId As String = Convert.ToString(clickedRow.Item("BRID"))
        Dim showHide As String = Convert.ToString(clickedRow.Item("SHOWHIDE"))

        '現在開いているタンクは閉じる
        'Dim currentShowDr = (From item In dt Where (Not Convert.ToString(item("LINECNT")) = currentRowNum) AndAlso item("SHOWHIDE").Equals("SHOW")).FirstOrDefault
        Dim currentShowDrList = (From item In dt Where (Not (item("BRID").Equals(brId) AndAlso item("DELFLG").Equals(CONST_FLAG_YES))) AndAlso item("SHOWHIDE").Equals("SHOW"))
        'If currentShowDr IsNot Nothing Then
        '    currentShowDr.Item("SHOWHIDE") = "HIDE"
        'End If
        If currentShowDrList.Any = True Then
            For Each currentShowDr In currentShowDrList
                currentShowDr.Item("SHOWHIDE") = "HIDE"
            Next
        End If
        Dim hideTankDrList = (From item In dt Where item("DELFLG").Equals(CONST_FLAG_YES) AndAlso item("HIDDEN").Equals(0))
        If hideTankDrList.Any = True Then
            For Each hideTankDr In hideTankDrList
                hideTankDr.Item("HIDDEN") = 1
            Next
        End If

        Dim hide As Integer = 0
        Dim showHideAfterProcValue As String = "SHOW"
        If showHide = "SHOW" Then
            hide = 1
            showHideAfterProcValue = "HIDE"
        End If

        'Dim tankDrList = (From item In dt Where (Not Convert.ToString(item("LINECNT")) = currentRowNum) AndAlso item("BRID").Equals(brId) _
        '                                         AndAlso item("DELFLG").Equals("1"))
        Dim tankDrList = (From item In dt Where item("BRID").Equals(brId) AndAlso item("DELFLG").Equals(CONST_FLAG_YES))

        If tankDrList.Any = True Then
            'タンクレコードを非表示
            For Each tankDr In tankDrList
                tankDr.Item("HIDDEN") = hide
            Next
        End If

        Dim brIdDrList = (From item In dt Where item("BRID").Equals(brId) _
                                                 AndAlso item("DELFLG").Equals(CONST_FLAG_NO))

        If brIdDrList.Any = True Then
            'SHOWHIDE項目の値を入れ替え
            For Each brIdDr In brIdDrList
                brIdDr.Item("SHOWHIDE") = showHideAfterProcValue
            Next
        End If

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return
        End If
    End Sub
    ''' <summary>
    ''' 画面表示のテーブルを制御する
    ''' </summary>
    Private Sub DisplayListObjEdit()

        Dim targetPanel As Panel = Me.WF_LISTAREA

        Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
        If rightDataDiv.HasControls = False _
           OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
           OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
            Return
        End If
        Dim rightHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HR"), Panel)
        Dim rightHeaderTable As Table = DirectCast(rightHeaderDiv.Controls(0), Table)
        Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"SHOWHIDE", ""}, {"BASEVALUEFLG", ""}, {"HASCHILD", ""}}
        With Nothing '右ヘッダーの列名より対象の列番号を取得
            Dim headerTableRow = rightHeaderTable.Rows(0)
            If headerTableRow.Cells.Count = 0 Then
                Return 'ヘッダー列に列が存在しない場合は終了
            End If
            'セル名称より列番号を取得
            Dim maxCellIndex = headerTableRow.Cells.Count - 1
            For cellIndex = 0 To maxCellIndex
                Dim targetCell As TableCell = headerTableRow.Cells(cellIndex)
                If targetCell.Attributes("cellfiedlname") IsNot Nothing AndAlso
               dicColumnNameToNo.ContainsKey(targetCell.Attributes("cellfiedlname")) Then
                    dicColumnNameToNo(targetCell.Attributes("cellfiedlname")) = cellIndex.ToString
                End If
            Next
        End With '列番号取得完了

        Dim leftHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HL"), Panel)
        Dim leftHeaderTable As Table = DirectCast(leftHeaderDiv.Controls(0), Table)
        Dim dicLeftColumnNameToNo As New Dictionary(Of String, String) From {{"SHOWTANK", ""},
                                                                             {"BRID", ""}}

        With Nothing '右ヘッダーの列名より対象の列番号を取得
            Dim headerTableRow = leftHeaderTable.Rows(0)
            If headerTableRow.Cells.Count = 0 Then
                Return 'ヘッダー列に列が存在しない場合は終了
            End If
            'セル名称より列番号を取得
            Dim maxCellIndex = headerTableRow.Cells.Count - 1
            For cellIndex = 0 To maxCellIndex
                Dim targetCell As TableCell = headerTableRow.Cells(cellIndex)
                If targetCell.Attributes("cellfiedlname") IsNot Nothing AndAlso
               dicLeftColumnNameToNo.ContainsKey(targetCell.Attributes("cellfiedlname")) Then
                    dicLeftColumnNameToNo(targetCell.Attributes("cellfiedlname")) = cellIndex.ToString
                End If
            Next
        End With '列番号取得完了

        Dim rightDataTable As Table = DirectCast(rightDataDiv.Controls(0), Table)
        Dim leftDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DL"), Panel)
        Dim leftDataTable As Table = DirectCast(leftDataDiv.Controls(0), Table) '1列目LINECNT 、3列目のSHOW DELETEカラム取得用

        '******************************
        'レンダリング行のループ
        '******************************
        Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        Dim dicButtonName As New Dictionary(Of String, String) From {{"SHOW", Me.hdnTextHide.Value}, {"HIDE", Me.hdnTextShow.Value}}

        For i = 0 To rowCnt
            Dim tbrRight As TableRow = rightDataTable.Rows(i)

            Dim tbrLeft As TableRow = leftDataTable.Rows(i)
            Dim lineCnt As String = tbrLeft.Cells(0).Text

            'ボタンの表示非表示制御
            Dim showBtn As Boolean = False
            If dicColumnNameToNo("BASEVALUEFLG") <> "" AndAlso
               tbrRight.Cells(Integer.Parse(dicColumnNameToNo("BASEVALUEFLG"))).Text = "1" Then
                showBtn = True

            End If
            Dim hasChild As Boolean = False
            If dicColumnNameToNo("HASCHILD") <> "" AndAlso
               tbrRight.Cells(Integer.Parse(dicColumnNameToNo("HASCHILD"))).Text = "1" Then
                hasChild = True
            End If

            'タンク表示非表示機能制御
            If showBtn = True AndAlso
               hasChild = True AndAlso
               dicLeftColumnNameToNo("SHOWTANK") <> "" AndAlso
               dicColumnNameToNo("SHOWHIDE") <> "" Then
                Dim showTankLabel As New WebControls.Label
                showTankLabel.ID = "lbl" & Me.WF_LISTAREA.ID & "SHOWTANK" & lineCnt
                showTankLabel.Attributes.Add("actType", "SHOWHIDE")
                showTankLabel.Attributes.Add("rownum", lineCnt)
                showTankLabel.Attributes.Add("onclick", "listButtonClick(this);false;")
                showTankLabel.Text = dicButtonName(tbrRight.Cells(Integer.Parse(dicColumnNameToNo("SHOWHIDE"))).Text)
                With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("SHOWTANK")))
                    .Controls.Add(showTankLabel)
                End With
            End If
        Next
    End Sub
    ''' <summary>
    ''' 画面グリッドのデータを取得しファイルに保存する。
    ''' </summary>
    Private Function FileSaveDisplayInput() As String
        'そもそも画面表示データがない状態の場合はそのまま終了
        If ViewState("DISPLAY_LINECNT_LIST") Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If
        Dim displayLineCnt = DirectCast(ViewState("DISPLAY_LINECNT_LIST"), List(Of Integer))
        Dim COA0021ListTable As New COA0021ListTable
        '一覧表示データ復元
        Dim dt As DataTable = CreateDataTable()

        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021recoverListTable()
        If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
            dt = COA0021ListTable.OUTTBL
        Else
            Return C_MESSAGENO.SYSTEMADM
        End If

        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If

        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String) From {{"CHECK", objChkPrifix}}

        Dim formToPost = New NameValueCollection(Request.Form)
        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    formToPost.Remove(dispObjId)
                End If
                Dim dr As DataRow = dt.Rows(i - 1)
                dr.Item(fieldId.Key) = displayValue
            Next
        Next

        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0021ListTable.ERR
        End If
        Return C_MESSAGENO.NORMAL
    End Function
End Class