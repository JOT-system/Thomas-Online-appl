Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' ブレーカー一覧画面クラス
''' </summary>
Public Class GBT00002RESULT
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00002" '自身のMAPID
    'Private Const CONST_DSPROWCOUNT = 44                '指定数＋１が表示対象
    'Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    Private Const CONST_DSPROWCOUNT = 99                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 100              'マウススクロール時の増分
    Private Const C_HYPHEN = "-"
    ''' <summary>
    ''' コピー対象のBRNO
    ''' </summary>
    ''' <returns></returns>
    Public Property CopyBrId As String = ""
    ''' <summary>
    ''' ログ出力(クラススコープ ロード時にNewします)
    ''' </summary>
    Private COA0003LogFile As COA0003LogFile
    ''' <summary>
    ''' 当画面情報保持
    ''' </summary>
    ''' <returns></returns>
    Public Property ThisScreenValues As GBT00002RESULT.GBT00002RValues

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
                COA0031ProfMap.VARIANTP = "BreakerSearch"
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

                '非活性制御
                SetEnableItem()

                'Complete設定
                SetComplete()

                '****************************************
                '一覧表作成
                '****************************************
                '一覧表データ取得
                Using dt As DataTable = Me.GetListDataTable()
                    'フィリングレートチェック結果を更新
                    UpdateFillingRate(dt)
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

                                If Convert.ToString(chkdt.Rows(i)("BRID")) = Convert.ToString(dt.Rows(j)("BRID")) Then

                                    dt.Rows(j)("CHECK") = chkdt.Rows(i)("CHECK")
                                End If
                            Next
                        Next

                    End If

                    '■■■ 絞り込み ■■■
                    SetnExtractDt(dt)
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
                        .VARI = "Default"
                        .SRCDATA = listData
                        .TBLOBJ = WF_LISTAREA
                        '.SCROLLTYPE = "2"
                        .SCROLLTYPE = "3"
                        .LEVENT = "ondblclick"
                        .LFUNC = "ListDbClick"
                        .TITLEOPT = True
                        .NOCOLUMNWIDTHOPT = 50
                        .OPERATIONCOLUMNWIDTHOPT = -1
                        .USERSORTOPT = 1
                    End With
                    COA0013TableObject.COA0013SetTableObject()

                    Dim divDrCont As Control = WF_LISTAREA.FindControl("WF_LISTAREA_DR")
                    Dim tblCont As Table = DirectCast(divDrCont.Controls(0), Table)
                    Dim checkedValue As Boolean
                    For Each dr As DataRow In listData.Rows
                        If Convert.ToString(dr.Item("CHECK")) = "on" Then
                            checkedValue = True
                        Else
                            checkedValue = False
                        End If
                        Dim chkId As String = "chkWF_LISTAREACHECK" & Convert.ToString(dr.Item("LINECNT"))
                        Dim chk As CheckBox = DirectCast(tblCont.FindControl(chkId), CheckBox)
                        If chk IsNot Nothing Then
                            chk.Checked = checkedValue
                        End If

                        If {C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE, C_APP_STATUS.APPLYING}.Contains(Trim(Convert.ToString(dr.Item("STATUSIF")))) Then
                            chk.Enabled = False
                        Else
                            If Convert.ToString(dr.Item("FILLINGRATECHECK")).Equals("") AndAlso
                                (Convert.ToString(dr.Item("POL1")) = Me.hdnComplete.Value AndAlso Convert.ToString(dr.Item("POD1")) = Me.hdnComplete.Value) AndAlso
                                ((Convert.ToString(dr.Item("POL2")) = Me.hdnComplete.Value AndAlso Convert.ToString(dr.Item("POD2")) = Me.hdnComplete.Value) Or
                                (Convert.ToString(dr.Item("POL2")) = C_HYPHEN AndAlso Convert.ToString(dr.Item("POD2")) = C_HYPHEN)) Then
                                chk.Enabled = True
                            Else
                                chk.Enabled = False
                            End If
                        End If

                        Dim btnCopy = WF_LISTAREA.FindControl("btn" & Me.WF_LISTAREA.ID & "ACTION" + dr.Item("LINECNT").ToString)
                        If dr.Item("SHOWBUTTON").ToString = "1" AndAlso Convert.ToString(dr.Item("DISABLED")) = CONST_FLAG_NO Then
                            btnCopy.Visible = True
                        Else
                            btnCopy.Visible = False
                        End If

                        Dim btnDelete = WF_LISTAREA.FindControl("btn" & Me.WF_LISTAREA.ID & "DELETEBTN" + dr.Item("LINECNT").ToString)
                        If dr.Item("SHOWBUTTON").ToString = "1" AndAlso Not {C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE}.Contains(dr.Item("STATUSIF").ToString.Trim) Then
                            btnDelete.Visible = True
                        Else
                            btnDelete.Visible = False
                        End If

                        If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
                            chk.Enabled = False
                            btnCopy.Visible = False
                            btnDelete.Visible = False
                        End If

                    Next

                    '画面表示しているLineCntを保持
                    If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
                        Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                                  Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
                        ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
                    Else
                        ViewState("DISPLAY_LINECNT_LIST") = Nothing
                    End If
                End Using 'DataTable

                'メッセージ設定
                If hdnMsgId.Value <> "" Then
                    CommonFunctions.ShowMessage(hdnMsgId.Value, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)
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
                        Me.hdnCalendarValue.Value = txtobj.Text

                        Me.mvLeft.Focus()
                    End If
                'POLCountry
                Case vLeftPOLCountry.ID
                    Dim dt As DataTable = GetCountry()
                    With Me.lbPOLCountry
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtPOLCountry.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
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
                'PODCountry
                Case vLeftPODCountry.ID
                    Dim dt As DataTable = GetCountry()
                    With Me.lbPODCountry
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(Me.txtPODCountry.Text)
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

                '承認ビュー表示切替
                Case Me.vLeftApproval.ID
                    SetApprovalListItem(Me.txtApproval.Text)
            End Select
        End If

    End Sub
    ''' <summary>
    ''' 都度生成するリストボックスの選択肢データをクリア
    ''' </summary>
    Private Sub ClearLeftListData()
        Me.lbShipper.Items.Clear()
        Me.lbConsignee.Items.Clear()
        Me.lbProduct.Items.Clear()
        Me.lbPOLCountry.Items.Clear()
        Me.lbPOL.Items.Clear()
        Me.lbPODCountry.Items.Clear()
        Me.lbPOD.Items.Clear()
        Me.mvLeft.SetActiveView(Me.vLeftCal)
    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID & "R"
        COA0011ReturnUrl.VARI = "GB_Default"
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
    ''' セールス新規作成ボタン押下時
    ''' </summary>
    Public Sub btnCreateSales_Click()
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ チェック処理 ■■■
        'とりあえずMOCでは」入力チェックしない
        '■■■ 画面遷移先URL取得 ■■■
        COA0012DoUrl.MAPIDP = "GBT00002R"
        COA0012DoUrl.VARIP = "GB_SelesNew"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Me.hdnBreakerType.Value = "1"
        Me.hdnTransferPattern.Value = ""

        Session("MAPmapid") = "GBT00002R"
        Session("MAPvariant") = "GB_SelesNew"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' オペ新規作成ボタン押下時
    ''' </summary>
    Public Sub btnCreateOperation_Click()
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ チェック処理 ■■■
        'とりあえずMOCでは」入力チェックしない
        '■■■ 画面遷移先URL取得 ■■■
        COA0012DoUrl.MAPIDP = "GBT00002R"
        COA0012DoUrl.VARIP = "GB_OpeNew"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Me.hdnBreakerType.Value = "2"
        Me.hdnTransferPattern.Value = ""

        Session("MAPmapid") = "GBT00002R"
        Session("MAPvariant") = "GB_OpeNew"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 申請ボタン押下時
    ''' </summary>
    Public Sub btnApply_Click()
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

        Dim procDateTime As DateTime = DateTime.Now
        Dim applyId As String = Nothing
        Dim lastStep As String = Nothing
        'CHECKチェックボックスがチェック済の全データを取得
        Dim q = (From item In dt
                 Where Convert.ToString(item("CHECK")) = "on")
        Dim applyDt As DataTable = Nothing
        If q.Any = True Then
            applyDt = q.CopyToDataTable
        Else
            applyDt = dt.Clone
        End If
        For Each applyDr As DataRow In applyDt.Rows ' For i As Integer = 0 To dt.Rows.Count - 1

            '申請ID取得
            'Dim GBA00002MasterApplyID As New GBA00002MasterApplyID
            'GBA00002MasterApplyID.COMPCODE = COA0019Session.APSRVCamp
            'GBA00002MasterApplyID.SYSCODE = sysCode
            'GBA00002MasterApplyID.KEYCODE = COA0019Session.APSRVname
            'GBA00002MasterApplyID.MAPID = "GBT00001"
            'GBA00002MasterApplyID.EVENTCODE = C_BRSEVENT.APPLY
            'GBA00002MasterApplyID.SUBCODE = ""
            'GBA00002MasterApplyID.COA0032getgApplyID()
            'If GBA00002MasterApplyID.ERR = C_MESSAGENO.NORMAL Then
            '    applyId = GBA00002MasterApplyID.APPLYID
            'Else
            '    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
            '                                messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00002MasterApplyID.ERR)})
            '    Return
            'End If
            Dim GBA00011ApplyID As New GBA00011ApplyID
            GBA00011ApplyID.COMPCODE = GBC_COMPCODE_D
            GBA00011ApplyID.SYSCODE = COA0019Session.SYSCODE
            GBA00011ApplyID.KEYCODE = COA0019Session.APSRVname
            GBA00011ApplyID.DIVISION = "B"
            GBA00011ApplyID.SEQOBJID = C_SQLSEQ.BREAKERWORK
            GBA00011ApplyID.SEQLEN = 6
            GBA00011ApplyID.GBA00011getApplyID()
            If GBA00011ApplyID.ERR = C_MESSAGENO.NORMAL Then
                applyId = GBA00011ApplyID.APPLYID
            Else
                CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                            messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00011ApplyID.ERR)})
                Return
            End If
            Dim subCode As String = Convert.ToString(applyDr.Item("AGENTORGANIZER"))

            '申請登録
            COA0032Apploval.I_COMPCODE = COA0019Session.APSRVCamp
            COA0032Apploval.I_APPLYID = applyId
            COA0032Apploval.I_MAPID = "GBT00001"
            COA0032Apploval.I_EVENTCODE = C_BRSEVENT.APPLY
            COA0032Apploval.I_SUBCODE = subCode
            COA0032Apploval.COA0032setApply()
            If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
                lastStep = COA0032Apploval.O_LASTSTEP
            Else
                CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If

            Dim userEmail As String = ""

            'ブレーカー更新
            Dim GBA00016BreakerApplyProc As New GBA00016BreakerApplyProc _
               With {.brId = Convert.ToString(applyDr.Item("BRID")), .ApplyId = applyId,
                     .LastStep = lastStep, .AmtRequest = Convert.ToString(applyDr.Item("AMTREQUEST")),
                     .ProcDateTime = procDateTime}

            GBA00016BreakerApplyProc.GBA00016BreakerDataApplyUpdate()

            ' GBA00016BreakerApplyProc.AmtDiscount = Me.txtAmtDiscount.Text
            ' GBA00016BreakerApplyProc.ProcDateTime = procDateTime
            'Dim sqlStat As New StringBuilder
            'Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            '    sqlCon.Open() '接続オープン

            '    sqlStat.Clear()
            '    sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
            '    sqlStat.AppendLine("   SET APPLYID   = @APPLYID")
            '    sqlStat.AppendLine("      ,LASTSTEP  = @LASTSTEP")
            '    sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
            '    sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
            '    sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
            '    sqlStat.AppendLine(" WHERE BRID      = @BRID")
            '    sqlStat.AppendLine("   AND TYPE      = @TYPE")
            '    sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

            '    'DB接続
            '    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            '        With sqlCmd.Parameters
            '            'パラメータ設定
            '            .Add("@BRID", SqlDbType.NVarChar, 20).Value = Convert.ToString(applyDr.Item("BRID"))
            '            .Add("@TYPE", SqlDbType.NVarChar, 20).Value = "INFO"
            '            .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            '            .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = applyId
            '            .Add("@LASTSTEP", SqlDbType.NVarChar, 20).Value = lastStep
            '            .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
            '            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
            '            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            '        End With
            '        sqlCmd.ExecuteNonQuery()
            '    End Using

            '    'AMTPRINCIPAL設定処理
            '    sqlStat.Clear()
            '    sqlStat.AppendLine("UPDATE GBT0002_BR_BASE")
            '    sqlStat.AppendLine("   SET AMTPRINCIPAL = @AMTREQUEST ")
            '    sqlStat.AppendLine("      ,UPDYMD       = @UPDYMD ")
            '    sqlStat.AppendLine("      ,UPDUSER      = @UPDUSER ")
            '    sqlStat.AppendLine("      ,RECEIVEYMD   = @RECEIVEYMD ")
            '    sqlStat.AppendLine(" WHERE BRID         = @BRID")
            '    sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")

            '    'DB接続
            '    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            '        With sqlCmd.Parameters
            '            'パラメータ設定
            '            .Add("@AMTREQUEST", SqlDbType.Float).Value = Convert.ToString(applyDr.Item("AMTREQUEST"))
            '            .Add("@BRID", SqlDbType.NVarChar, 20).Value = Convert.ToString(applyDr.Item("BRID"))
            '            .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            '            .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
            '            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
            '            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            '        End With
            '        sqlCmd.ExecuteNonQuery()
            '    End Using
            'End Using

            'メール
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = C_BRSEVENT.APPLY
            'GBA00009MailSendSet.MAILSUBCODE = subCode
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = Convert.ToString(applyDr.Item("BRID"))
            GBA00009MailSendSet.BRSUBID = Convert.ToString(applyDr.Item("SUBID"))
            GBA00009MailSendSet.BRBASEID = Convert.ToString(applyDr.Item("BRBASEID"))
            GBA00009MailSendSet.BRROUND = ""
            GBA00009MailSendSet.APPLYID = applyId
            GBA00009MailSendSet.GBA00009setMailToBR()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If

        Next

        'メッセージ出力
        hdnMsgId.Value = C_MESSAGENO.APPLYSUCCESS

        Dim thisPageUrl As String = Request.Url.ToString
        Server.Transfer(Request.Url.LocalPath)

    End Sub
    '''' <summary>
    '''' 承認取り消しボタン押下時
    '''' </summary>
    'Public Sub btnApplyCancel_Click()

    'End Sub
    ''' <summary>
    ''' Excelダウンロードボタン押下時
    ''' </summary>
    Public Sub btnExcelDownload_Click()
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
                Case vLeftPOLCountry.ID
                    'POLCountry選択時
                    Me.lblPOLCountryText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbPOLCountry.SelectedItem IsNot Nothing Then
                        Dim countryCode As String = Me.lbPOLCountry.SelectedItem.Value
                        Dim dt As DataTable = GetCountry(countryCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblPOLCountryText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
                        End If
                    End If
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
                Case vLeftPODCountry.ID
                    'POD選択時
                    Me.lblPODCountryText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbPODCountry.SelectedItem IsNot Nothing Then
                        Dim countryCode As String = Me.lbPODCountry.SelectedItem.Value
                        Dim dt As DataTable = GetCountry(countryCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblPODCountryText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
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
                Case Me.vLeftApproval.ID 'アクティブなビューが承認
                    '承認選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        If Me.lbApproval.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbApproval.SelectedItem.Value
                            Me.lblApprovalText.Text = Me.lbApproval.SelectedItem.Text
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            Me.lblApprovalText.Text = ""
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
        ClearLeftListData()
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
        ClearLeftListData()
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
    ''' 前頁ボタン押下時
    ''' </summary>
    Public Sub btnPREV_Click()

        'ポジションを設定するのみ
        hdnMouseWheel.Value = "-"

    End Sub
    ''' <summary>
    ''' 次頁ボタン押下時
    ''' </summary>
    Public Sub btnNEXT_Click()

        'ポジションを設定するのみ
        hdnMouseWheel.Value = "+"

    End Sub
    ''' <summary>
    ''' 一覧コピーボタン押下時
    ''' </summary>
    Public Sub btnListAction_Click()
        '****************************************
        'コピー元のBrIdを取得
        '****************************************
        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New COA0021ListTable
        Dim sqlStat As New Text.StringBuilder

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

        Dim dr As DataRow = dt.Rows((CInt(hdnListCurrentRownum.Value) - 1))
        Dim brId As String = Convert.ToString(dr.Item("BRID"))
        Me.CopyBrId = brId
        'SALES or OPEの確認
        Dim profVari As String = "GB_OpeCopy"
        Me.hdnBreakerType.Value = "2"
        If Me.hdnSearchBreakerType.Value = "01SALES" Then
            profVari = "GB_SelesCopy"
            Me.hdnBreakerType.Value = "1"
        End If

        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ 画面遷移先URL取得 ■■■
        COA0012DoUrl.MAPIDP = "GBT00002R"
        COA0012DoUrl.VARIP = profVari
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If

        Me.hdnTransferPattern.Value = ""

        Session("MAPmapid") = "GBT00002R"
        Session("MAPvariant") = profVari
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)

#Region "コピーで港・荷主変更前(2019/4/10 ～ しばらくコメントアウトの上で残しますが問題なければ削除)"
        'Dim dt As DataTable = CreateDataTable()
        'Dim COA0021ListTable As New COA0021ListTable
        'Dim sqlStat As New Text.StringBuilder

        'COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        'COA0021ListTable.TBLDATA = dt
        'COA0021ListTable.COA0021recoverListTable()
        'If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
        '    dt = COA0021ListTable.OUTTBL
        'Else
        '    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
        '                                messageParams:=New List(Of String) From {"CODE:" & COA0021ListTable.ERR & ""})
        '    Return
        'End If

        'Dim dr As DataRow = dt.Rows((CInt(hdnListCurrentRownum.Value) - 1))
        'Dim brId = dr.Item("BRID")
        'Dim NewbrId As String = ""

        ''コピー処理
        ''DB接続
        'Using sqlCon As New SqlConnection(COA0019Session.DBcon)
        '    sqlCon.Open()

        '    'ブレーカーNo作成
        '    NewbrId = GetNewBreakerNo(sqlCon)

        '    '******************************
        '    ' 紐づけ情報インサート
        '    '******************************
        '    sqlStat.Clear()
        '    sqlStat.AppendLine("INSERT INTO GBT0001_BR_INFO (")
        '    sqlStat.AppendLine("            BRID ")
        '    sqlStat.AppendLine("           ,SUBID ")
        '    sqlStat.AppendLine("           ,TYPE ")
        '    sqlStat.AppendLine("           ,LINKID ")
        '    sqlStat.AppendLine("           ,STYMD ")
        '    sqlStat.AppendLine("           ,BRTYPE ")
        '    sqlStat.AppendLine("           ,APPLYID ")
        '    sqlStat.AppendLine("           ,LASTSTEP ")
        '    sqlStat.AppendLine("           ,USETYPE ")
        '    sqlStat.AppendLine("           ,REMARK ")
        '    sqlStat.AppendLine("           ,DELFLG ")
        '    sqlStat.AppendLine("           ,INITYMD ")
        '    sqlStat.AppendLine("           ,UPDYMD ")
        '    sqlStat.AppendLine("           ,UPDUSER ")
        '    sqlStat.AppendLine("           ,UPDTERMID ")
        '    sqlStat.AppendLine("           ,RECEIVEYMD ")
        '    sqlStat.AppendLine("   ) SELECT ")
        '    sqlStat.AppendLine("            @NEWBRID AS BRID")
        '    sqlStat.AppendLine("           ,left(SUBID,CHARINDEX( '0', SUBID)-1) + trim(right('0000000001',(len(SUBID)-CHARINDEX( '0', SUBID)+1))) as SUBID ")
        '    sqlStat.AppendLine("           ,TYPE ")
        '    sqlStat.AppendLine("           ,left(LINKID,CHARINDEX( '-', LINKID)) +trim(right('0000000001',(len(LINKID)-CHARINDEX( '-', LINKID)))) as LINKID ")
        '    sqlStat.AppendLine("           ,@STYMD ")
        '    sqlStat.AppendLine("           ,BRTYPE ")
        '    sqlStat.AppendLine("           ,'' AS APPLYID ")
        '    sqlStat.AppendLine("           ,'' AS LASTSTEP ")
        '    sqlStat.AppendLine("           ,USETYPE ")
        '    sqlStat.AppendLine("           ,REMARK ")
        '    sqlStat.AppendLine("           ,DELFLG ")
        '    sqlStat.AppendLine("           ,@INITYMD ")
        '    sqlStat.AppendLine("           ,@UPDYMD ")
        '    sqlStat.AppendLine("           ,@UPDUSER ")
        '    sqlStat.AppendLine("           ,@UPDTERMID ")
        '    sqlStat.AppendLine("           ,@RECEIVEYMD ")
        '    sqlStat.AppendLine("      FROM  GBT0001_BR_INFO    ")
        '    sqlStat.AppendLine("     WHERE  BRID    =  @BRID   ")
        '    sqlStat.AppendLine("       AND  DELFLG  =  @DELFLG ")
        '    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
        '        Dim paramNewBrId As SqlParameter = sqlCmd.Parameters.Add("@NEWBRID", SqlDbType.NVarChar, 20)
        '        Dim paramBrId As SqlParameter = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar, 20)
        '        Dim paramDelflg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
        '        Dim paramStYmd As SqlParameter = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
        '        Dim paramInitymd As SqlParameter = sqlCmd.Parameters.Add("@INITYMD", SqlDbType.DateTime)
        '        Dim paramUpdymd As SqlParameter = sqlCmd.Parameters.Add("@UPDYMD", SqlDbType.DateTime)
        '        Dim paramUpduser As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)
        '        Dim paramUpdtermid As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 30)
        '        Dim paramReceiveymd As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)

        '        'パラメータ設定
        '        paramNewBrId.Value = NewbrId
        '        paramBrId.Value = brId
        '        paramDelflg.Value = CONST_FLAG_NO
        '        paramStYmd.Value = Date.Now.ToString("yyyy/MM/dd")
        '        paramInitymd.Value = DateTime.Now
        '        paramUpdymd.Value = DateTime.Now
        '        paramUpduser.Value = COA0019Session.USERID
        '        paramUpdtermid.Value = HttpContext.Current.Session("APSRVname")
        '        paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD
        '        'SQL実行
        '        sqlCmd.ExecuteNonQuery()
        '    End Using

        '    '******************************
        '    ' organizer情報（ブレーカー基本）インサート
        '    '******************************
        '    sqlStat.Clear()
        '    sqlStat.AppendLine("INSERT INTO GBT0002_BR_BASE (")
        '    sqlStat.AppendLine("              BRID")
        '    sqlStat.AppendLine("             ,BRBASEID")
        '    sqlStat.AppendLine("             ,STYMD")
        '    sqlStat.AppendLine("             ,VALIDITYFROM")
        '    sqlStat.AppendLine("             ,VALIDITYTO")
        '    sqlStat.AppendLine("             ,TERMTYPE")
        '    sqlStat.AppendLine("             ,NOOFTANKS")
        '    sqlStat.AppendLine("             ,SHIPPER")
        '    sqlStat.AppendLine("             ,CONSIGNEE")
        '    sqlStat.AppendLine("             ,CARRIER1")
        '    sqlStat.AppendLine("             ,CARRIER2")
        '    sqlStat.AppendLine("             ,PRODUCTCODE")
        '    sqlStat.AppendLine("             ,PRODUCTWEIGHT")
        '    sqlStat.AppendLine("             ,RECIEPTCOUNTRY1")
        '    sqlStat.AppendLine("             ,RECIEPTPORT1")
        '    sqlStat.AppendLine("             ,RECIEPTCOUNTRY2")
        '    sqlStat.AppendLine("             ,RECIEPTPORT2")
        '    sqlStat.AppendLine("             ,LOADCOUNTRY1")
        '    sqlStat.AppendLine("             ,LOADPORT1")
        '    sqlStat.AppendLine("             ,LOADCOUNTRY2")
        '    sqlStat.AppendLine("             ,LOADPORT2")
        '    sqlStat.AppendLine("             ,DISCHARGECOUNTRY1")
        '    sqlStat.AppendLine("             ,DISCHARGEPORT1")
        '    sqlStat.AppendLine("             ,DISCHARGECOUNTRY2")
        '    sqlStat.AppendLine("             ,DISCHARGEPORT2")
        '    sqlStat.AppendLine("             ,DELIVERYCOUNTRY1")
        '    sqlStat.AppendLine("             ,DELIVERYPORT1")
        '    sqlStat.AppendLine("             ,DELIVERYCOUNTRY2")
        '    sqlStat.AppendLine("             ,DELIVERYPORT2")
        '    sqlStat.AppendLine("             ,VSL1")
        '    sqlStat.AppendLine("             ,VOY1")
        '    sqlStat.AppendLine("             ,ETD1")
        '    sqlStat.AppendLine("             ,ETA1")
        '    sqlStat.AppendLine("             ,VSL2")
        '    sqlStat.AppendLine("             ,VOY2")
        '    sqlStat.AppendLine("             ,ETD2")
        '    sqlStat.AppendLine("             ,ETA2")
        '    sqlStat.AppendLine("             ,INVOICEDBY")
        '    sqlStat.AppendLine("             ,LOADING")
        '    sqlStat.AppendLine("             ,STEAMING")
        '    sqlStat.AppendLine("             ,TIP")
        '    sqlStat.AppendLine("             ,EXTRA")
        '    sqlStat.AppendLine("             ,JOTHIREAGE")
        '    sqlStat.AppendLine("             ,COMMERCIALFACTOR")
        '    sqlStat.AppendLine("             ,AMTREQUEST")
        '    sqlStat.AppendLine("             ,AMTPRINCIPAL")
        '    sqlStat.AppendLine("             ,AMTDISCOUNT")
        '    sqlStat.AppendLine("             ,DEMURTO")
        '    sqlStat.AppendLine("             ,DEMURUSRATE1")
        '    sqlStat.AppendLine("             ,DEMURUSRATE2")
        '    sqlStat.AppendLine("             ,AGENTORGANIZER")
        '    sqlStat.AppendLine("             ,AGENTPOL1")
        '    sqlStat.AppendLine("             ,AGENTPOL2")
        '    sqlStat.AppendLine("             ,AGENTPOD1")
        '    sqlStat.AppendLine("             ,AGENTPOD2")
        '    sqlStat.AppendLine("             ,APPLYTEXT")
        '    sqlStat.AppendLine("             ,COUNTRYORGANIZER")
        '    sqlStat.AppendLine("             ,LASTORDERNO")
        '    sqlStat.AppendLine("             ,TANKNO")
        '    sqlStat.AppendLine("             ,DEPOTCODE")
        '    sqlStat.AppendLine("             ,TWOAGOPRODUCT")
        '    sqlStat.AppendLine("             ,FEE")
        '    sqlStat.AppendLine("             ,BILLINGCATEGORY")
        '    sqlStat.AppendLine("             ,USINGLEASETANK")
        '    sqlStat.AppendLine("             ,REMARK")
        '    sqlStat.AppendLine("             ,DELFLG")
        '    sqlStat.AppendLine("             ,INITYMD ")
        '    sqlStat.AppendLine("             ,INITUSER ")
        '    sqlStat.AppendLine("             ,UPDYMD ")
        '    sqlStat.AppendLine("             ,UPDUSER ")
        '    sqlStat.AppendLine("             ,UPDTERMID ")
        '    sqlStat.AppendLine("             ,RECEIVEYMD ")
        '    sqlStat.AppendLine("   ) SELECT ")
        '    sqlStat.AppendLine("              @NEWBRID AS BRID")
        '    sqlStat.AppendLine("             ,left(BRBASEID,CHARINDEX( '-', BRBASEID)) +trim(right('0000000001',(len(BRBASEID)-CHARINDEX( '-', BRBASEID)))) as BRBASEID ")
        '    sqlStat.AppendLine("             ,@STYMD")
        '    sqlStat.AppendLine("             ,@VALIDITYFROM")
        '    sqlStat.AppendLine("             ,@VALIDITYTO")
        '    sqlStat.AppendLine("             ,TERMTYPE")
        '    sqlStat.AppendLine("             ,NOOFTANKS")
        '    sqlStat.AppendLine("             ,SHIPPER")
        '    sqlStat.AppendLine("             ,CONSIGNEE")
        '    sqlStat.AppendLine("             ,CARRIER1")
        '    sqlStat.AppendLine("             ,CARRIER2")
        '    sqlStat.AppendLine("             ,PRODUCTCODE")
        '    sqlStat.AppendLine("             ,PRODUCTWEIGHT")
        '    sqlStat.AppendLine("             ,RECIEPTCOUNTRY1")
        '    sqlStat.AppendLine("             ,RECIEPTPORT1")
        '    sqlStat.AppendLine("             ,RECIEPTCOUNTRY2")
        '    sqlStat.AppendLine("             ,RECIEPTPORT2")
        '    sqlStat.AppendLine("             ,LOADCOUNTRY1")
        '    sqlStat.AppendLine("             ,LOADPORT1")
        '    sqlStat.AppendLine("             ,LOADCOUNTRY2")
        '    sqlStat.AppendLine("             ,LOADPORT2")
        '    sqlStat.AppendLine("             ,DISCHARGECOUNTRY1")
        '    sqlStat.AppendLine("             ,DISCHARGEPORT1")
        '    sqlStat.AppendLine("             ,DISCHARGECOUNTRY2")
        '    sqlStat.AppendLine("             ,DISCHARGEPORT2")
        '    sqlStat.AppendLine("             ,DELIVERYCOUNTRY1")
        '    sqlStat.AppendLine("             ,DELIVERYPORT1")
        '    sqlStat.AppendLine("             ,DELIVERYCOUNTRY2")
        '    sqlStat.AppendLine("             ,DELIVERYPORT2")
        '    sqlStat.AppendLine("             ,VSL1")
        '    sqlStat.AppendLine("             ,VOY1")
        '    sqlStat.AppendLine("             ,ETD1")
        '    sqlStat.AppendLine("             ,ETA1")
        '    sqlStat.AppendLine("             ,VSL2")
        '    sqlStat.AppendLine("             ,VOY2")
        '    sqlStat.AppendLine("             ,ETD2")
        '    sqlStat.AppendLine("             ,ETA2")
        '    sqlStat.AppendLine("             ,INVOICEDBY")
        '    sqlStat.AppendLine("             ,LOADING")
        '    sqlStat.AppendLine("             ,STEAMING")
        '    sqlStat.AppendLine("             ,TIP")
        '    sqlStat.AppendLine("             ,EXTRA")
        '    sqlStat.AppendLine("             ,JOTHIREAGE")
        '    sqlStat.AppendLine("             ,COMMERCIALFACTOR")
        '    sqlStat.AppendLine("             ,AMTREQUEST")
        '    sqlStat.AppendLine("             ,AMTPRINCIPAL")
        '    sqlStat.AppendLine("             ,AMTDISCOUNT")
        '    sqlStat.AppendLine("             ,DEMURTO")
        '    sqlStat.AppendLine("             ,DEMURUSRATE1")
        '    sqlStat.AppendLine("             ,DEMURUSRATE2")
        '    sqlStat.AppendLine("             ,AGENTORGANIZER")
        '    sqlStat.AppendLine("             ,AGENTPOL1")
        '    sqlStat.AppendLine("             ,AGENTPOL2")
        '    sqlStat.AppendLine("             ,AGENTPOD1")
        '    sqlStat.AppendLine("             ,AGENTPOD2")
        '    sqlStat.AppendLine("             ,APPLYTEXT")
        '    sqlStat.AppendLine("             ,COUNTRYORGANIZER")
        '    sqlStat.AppendLine("             ,LASTORDERNO")
        '    sqlStat.AppendLine("             ,TANKNO")
        '    sqlStat.AppendLine("             ,DEPOTCODE")
        '    sqlStat.AppendLine("             ,TWOAGOPRODUCT")
        '    sqlStat.AppendLine("             ,FEE")
        '    sqlStat.AppendLine("             ,BILLINGCATEGORY")
        '    sqlStat.AppendLine("             ,USINGLEASETANK")
        '    sqlStat.AppendLine("             ,REMARK")
        '    sqlStat.AppendLine("             ,DELFLG")
        '    sqlStat.AppendLine("             ,@INITYMD ")
        '    sqlStat.AppendLine("             ,@INITUSER ")
        '    sqlStat.AppendLine("             ,@UPDYMD ")
        '    sqlStat.AppendLine("             ,@UPDUSER ")
        '    sqlStat.AppendLine("             ,@UPDTERMID ")
        '    sqlStat.AppendLine("             ,@RECEIVEYMD ")
        '    sqlStat.AppendLine("      FROM  GBT0002_BR_BASE    ")
        '    sqlStat.AppendLine("     WHERE  BRID    =  @BRID   ")
        '    sqlStat.AppendLine("       AND  DELFLG  =  @DELFLG ")
        '    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
        '        With sqlCmd.Parameters
        '            '月末日の取得
        '            Dim initDate As Date = New Date(Date.Now.Year, Date.Now.Month, 1)
        '            initDate = initDate.AddMonths(2).AddDays(-1)
        '            'パラメータ設定
        '            .Add("@NEWBRID", SqlDbType.NVarChar, 20).Value = NewbrId
        '            .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
        '            .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
        '            .Add("@STYMD", SqlDbType.Date).Value = Date.Now.ToString("yyyy/MM/dd")
        '            .Add("@VALIDITYFROM", SqlDbType.Date).Value = Date.Now.ToString("yyyy/MM/dd")
        '            .Add("@VALIDITYTO", SqlDbType.Date).Value = initDate.ToString("yyyy/MM/dd")
        '            .Add("@INITYMD", SqlDbType.DateTime).Value = DateTime.Now
        '            .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
        '            .Add("@UPDYMD", SqlDbType.DateTime).Value = DateTime.Now
        '            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
        '            .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
        '            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
        '        End With
        '        'SQL実行
        '        sqlCmd.ExecuteNonQuery()
        '    End Using

        '    '******************************
        '    ' 費用情報インサート
        '    '******************************
        '    sqlStat.Clear()
        '    sqlStat.AppendLine("INSERT INTO GBT0003_BR_VALUE (")
        '    sqlStat.AppendLine("              BRID")
        '    sqlStat.AppendLine("             ,BRVALUEID")
        '    sqlStat.AppendLine("             ,STYMD")
        '    sqlStat.AppendLine("             ,DTLPOLPOD")
        '    sqlStat.AppendLine("             ,DTLOFFICE")
        '    sqlStat.AppendLine("             ,COSTCODE")
        '    sqlStat.AppendLine("             ,BASEON")
        '    sqlStat.AppendLine("             ,TAX")
        '    sqlStat.AppendLine("             ,USD")
        '    sqlStat.AppendLine("             ,LOCAL")
        '    sqlStat.AppendLine("             ,CONTRACTOR")
        '    sqlStat.AppendLine("             ,LOCALRATE")
        '    sqlStat.AppendLine("             ,USDRATE")
        '    sqlStat.AppendLine("             ,CURRENCYCODE")
        '    sqlStat.AppendLine("             ,AGENT")
        '    sqlStat.AppendLine("             ,ACTIONID")
        '    sqlStat.AppendLine("             ,CLASS1")
        '    sqlStat.AppendLine("             ,CLASS2")
        '    sqlStat.AppendLine("             ,CLASS3")
        '    sqlStat.AppendLine("             ,CLASS4")
        '    sqlStat.AppendLine("             ,CLASS5")
        '    sqlStat.AppendLine("             ,CLASS6")
        '    sqlStat.AppendLine("             ,CLASS7")
        '    sqlStat.AppendLine("             ,CLASS8")
        '    sqlStat.AppendLine("             ,CLASS9")
        '    sqlStat.AppendLine("             ,COUNTRYCODE")
        '    sqlStat.AppendLine("             ,REPAIRFLG")
        '    sqlStat.AppendLine("             ,APPROVEDUSD")
        '    sqlStat.AppendLine("             ,INVOICEDBY")
        '    sqlStat.AppendLine("             ,BILLING")
        '    sqlStat.AppendLine("             ,REMARK")
        '    sqlStat.AppendLine("             ,DELFLG")
        '    sqlStat.AppendLine("             ,INITYMD ")
        '    sqlStat.AppendLine("             ,INITUSER ")
        '    sqlStat.AppendLine("             ,UPDYMD ")
        '    sqlStat.AppendLine("             ,UPDUSER ")
        '    sqlStat.AppendLine("             ,UPDTERMID ")
        '    sqlStat.AppendLine("             ,RECEIVEYMD ")
        '    sqlStat.AppendLine("   ) SELECT ")
        '    sqlStat.AppendLine("              @NEWBRID AS BRID")
        '    sqlStat.AppendLine("             ,left(BV.BRVALUEID,CHARINDEX( '-', BV.BRVALUEID)) +trim(right('0000000001',(len(BV.BRVALUEID)-CHARINDEX( '-', BV.BRVALUEID)))) as BRVALUEID ")
        '    sqlStat.AppendLine("             ,@STYMD")
        '    sqlStat.AppendLine("             ,BV.DTLPOLPOD")
        '    sqlStat.AppendLine("             ,BV.DTLOFFICE")
        '    sqlStat.AppendLine("             ,BV.COSTCODE")
        '    sqlStat.AppendLine("             ,BV.BASEON")
        '    sqlStat.AppendLine("             ,BV.TAX")
        '    sqlStat.AppendLine("             ,BV.USD")
        '    sqlStat.AppendLine("             ,BV.LOCAL")
        '    sqlStat.AppendLine("             ,BV.CONTRACTOR")
        '    sqlStat.AppendLine("             ,isnull(ER.EXRATE,0)")
        '    sqlStat.AppendLine("             ,BV.USDRATE")
        '    sqlStat.AppendLine("             ,BV.CURRENCYCODE")
        '    sqlStat.AppendLine("             ,BV.AGENT")
        '    sqlStat.AppendLine("             ,BV.ACTIONID")
        '    sqlStat.AppendLine("             ,BV.CLASS1")
        '    sqlStat.AppendLine("             ,BV.CLASS2")
        '    sqlStat.AppendLine("             ,BV.CLASS3")
        '    sqlStat.AppendLine("             ,BV.CLASS4")
        '    sqlStat.AppendLine("             ,BV.CLASS5")
        '    sqlStat.AppendLine("             ,BV.CLASS6")
        '    sqlStat.AppendLine("             ,BV.CLASS7")
        '    sqlStat.AppendLine("             ,BV.CLASS8")
        '    sqlStat.AppendLine("             ,BV.CLASS9")
        '    sqlStat.AppendLine("             ,BV.COUNTRYCODE")
        '    sqlStat.AppendLine("             ,BV.REPAIRFLG")
        '    sqlStat.AppendLine("             ,BV.APPROVEDUSD")
        '    sqlStat.AppendLine("             ,BV.INVOICEDBY")
        '    sqlStat.AppendLine("             ,BV.BILLING")
        '    sqlStat.AppendLine("             ,BV.REMARK")
        '    sqlStat.AppendLine("             ,BV.DELFLG")
        '    sqlStat.AppendLine("             ,@INITYMD ")
        '    sqlStat.AppendLine("             ,@INITUSER ")
        '    sqlStat.AppendLine("             ,@UPDYMD ")
        '    sqlStat.AppendLine("             ,@UPDUSER ")
        '    sqlStat.AppendLine("             ,@UPDTERMID ")
        '    sqlStat.AppendLine("             ,@RECEIVEYMD ")
        '    sqlStat.AppendLine("      FROM  GBT0003_BR_VALUE BV  ")
        '    sqlStat.AppendLine("      LEFT OUTER JOIN GBM0020_EXRATE ER ")
        '    sqlStat.AppendLine("        ON   ER.COMPCODE     = @COMPCODE ")
        '    sqlStat.AppendLine("        AND  ER.COUNTRYCODE  = BV.COUNTRYCODE ")
        '    sqlStat.AppendLine("        AND  ER.STYMD    <= @STYMD ")
        '    sqlStat.AppendLine("        AND  ER.ENDYMD   >= @STYMD ")
        '    sqlStat.AppendLine("        AND  ER.DELFLG   =  @DELFLG ")
        '    sqlStat.AppendLine("        AND  ER.TARGETYM = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")
        '    sqlStat.AppendLine("     WHERE  BV.BRID    =  @BRID   ")
        '    sqlStat.AppendLine("       AND  BV.DELFLG  =  @DELFLG ")
        '    Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
        '        With sqlCmd.Parameters
        '            'パラメータ設定
        '            .Add("@NEWBRID", SqlDbType.NVarChar, 20).Value = NewbrId
        '            .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
        '            .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_NO
        '            .Add("@STYMD", SqlDbType.Date).Value = Date.Now.ToString("yyyy/MM/dd")
        '            .Add("@INITYMD", SqlDbType.DateTime).Value = DateTime.Now
        '            .Add("@INITUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
        '            .Add("@UPDYMD", SqlDbType.DateTime).Value = DateTime.Now
        '            .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
        '            .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
        '            .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
        '            .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = Convert.ToString(HttpContext.Current.Session("APSRVCamp"))
        '            .Add("@TARGETYM", SqlDbType.Date).Value = Date.Now
        '        End With
        '        'SQL実行
        '        sqlCmd.ExecuteNonQuery()
        '    End Using

        'End Using

        ''メッセージ出力
        'hdnMsgId.Value = C_MESSAGENO.NORMALCOPY

        'Dim thisPageUrl As String = Request.Url.ToString
        'Server.Transfer(Request.Url.LocalPath)
#End Region
    End Sub
    ''' <summary>
    ''' 一覧削除ボタン押下時
    ''' </summary>
    Public Sub btnListDelete_Click()

        Dim dt As DataTable = CreateDataTable()
        Dim COA0021ListTable As New COA0021ListTable
        Dim sqlStat As New Text.StringBuilder
        Dim tran As SqlTransaction = Nothing
        Dim procDateTime As DateTime = DateTime.Now

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

        Dim dr As DataRow = dt.Rows((CInt(hdnListCurrentRownum.Value) - 1))
        Dim brId As String = Convert.ToString(dr.Item("BRID"))



        'ステータスチェック
        If {C_APP_STATUS.APPLYING, C_APP_STATUS.REVISE, C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE}.Contains(Trim(Convert.ToString(dr.Item("STATUSIF")))) Then
            Dim MsgBrId As String = ""
            If COA0019Session.LANGDISP = C_LANG.JA Then
                MsgBrId = "ブレーカーID"
            Else
                MsgBrId = "Breaker ID"
            End If
            CommonFunctions.ShowMessage(C_MESSAGENO.UNSELECTABLEERR, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {MsgBrId & "：" & brId & ""})
            Return
        End If

        Dim obj As String() = Nothing
        Dim brRound As String = Nothing
        Dim eventCode As String = Nothing
        If Convert.ToString(dr.Item("POL2")) = "-" AndAlso Convert.ToString(dr.Item("POD2")) = "-" Then
            obj = {"POL1", "POD1"}
            brRound = "1"
        Else
            obj = {"POL1", "POD1", "POL2", "POD2"}
            brRound = "2"
        End If

        For Each mailObj In obj

            Select Case mailObj
                Case "POL1", "POL2"
                    eventCode = C_BRSEVENT.DELETE_POL
                Case "POD1", "POD2"
                    eventCode = C_BRSEVENT.DELETE_POD
            End Select

            'メール
            Dim GBA00009MailSendSet As New GBA00009MailSendSet
            GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
            GBA00009MailSendSet.EVENTCODE = eventCode
            GBA00009MailSendSet.MAILSUBCODE = ""
            GBA00009MailSendSet.BRID = brId
            GBA00009MailSendSet.BRSUBID = Convert.ToString(dr.Item("SUBID"))
            GBA00009MailSendSet.BRBASEID = Convert.ToString(dr.Item("BRBASEID"))
            GBA00009MailSendSet.BRROUND = brRound
            GBA00009MailSendSet.APPLYID = ""
            GBA00009MailSendSet.GBA00009setMailToBR()
            If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage)
                Return
            End If
        Next

        Try
            '削除処理
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()

                tran = sqlCon.BeginTransaction() 'トランザクション開始

                '******************************
                ' 紐づけ情報削除
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBT0001_BR_INFO")
                sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE BRID      = @BRID")
                sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    With sqlCmd.Parameters
                        'パラメータ設定
                        .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                        .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                        .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using

                '******************************
                ' organizer情報（ブレーカー基本）削除
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBT0002_BR_BASE")
                sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE BRID      = @BRID")
                sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    With sqlCmd.Parameters
                        'パラメータ設定
                        .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                        .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                        .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using

                '******************************
                ' 費用情報削除
                '******************************
                sqlStat.Clear()
                sqlStat.AppendLine("UPDATE GBT0003_BR_VALUE")
                sqlStat.AppendLine("   SET DELFLG    = @DELFLG ")
                sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD ")
                sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER ")
                sqlStat.AppendLine("      ,RECEIVEYMD = @RECEIVEYMD ")
                sqlStat.AppendLine(" WHERE BRID      = @BRID")
                sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
                Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    With sqlCmd.Parameters
                        'パラメータ設定
                        .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                        .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDateTime
                        .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                        .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    End With
                    sqlCmd.ExecuteNonQuery()
                End Using

                tran.Commit()

            End Using

            'メッセージ出力
            hdnMsgId.Value = C_MESSAGENO.NORMALENTRY

            Dim thisPageUrl As String = Request.Url.ToString
            Server.Transfer(Request.Url.LocalPath)

        Catch ex As Exception
            Throw
        Finally
            If tran IsNot Nothing Then
                tran.Dispose()
            End If
        End Try

    End Sub
    ''' <summary>
    ''' ブレーカー番号取得
    ''' </summary>
    ''' <param name="sqlCon">オプション 項目</param>
    ''' <returns>新規ブレーカー番号</returns>
    Private Function GetNewBreakerNo(Optional sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim brNo As String = ""
        Dim sqlStat As New Text.StringBuilder
        Dim brType As String = ""
        If Me.hdnSearchBreakerType.Value = "01SALES" Then
            brType = "BT"
        Else
            brType = "BE"
        End If
        '生きているブレーカーは基本情報＋発地着地(最大4)の5レコード想定
        sqlStat.AppendLine("Select  '" & brType & "' ")
        sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
        sqlStat.AppendLine("      + '-'")
        sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR " & C_SQLSEQ.BREAKER & ")),4)")
        sqlStat.AppendLine("      + '-'")
        sqlStat.AppendLine("      + (SELECT VALUE1")
        sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
        sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
        sqlStat.AppendLine("            AND KEYCODE = @KEYCODE")
        sqlStat.AppendLine("            AND STYMD  <= @STYMD")
        sqlStat.AppendLine("            AND ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("            AND DELFLG <> @DELFLG)")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                With sqlCmd.Parameters
                    'SQLパラメータ設定
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = C_SERVERSEQ
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVname
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get new BreakerNo error")
                    End If

                    brNo = Convert.ToString(dt.Rows(0).Item(0))
                End Using

            End Using
            Return brNo
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try
    End Function
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
        'Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String)
        '入力値保持用のフィールド名設定
        fieldIdList.Add("CHECK", objChkPrifix)

        For Each i In displayLineCnt
            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    '                    formToPost.Remove(dispObjId)
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

        '絞り込み条件保持
        Me.ThisScreenValues = New GBT00002RValues
        Me.ThisScreenValues.SearchShipper = Me.txtShipper.Text
        Me.ThisScreenValues.SearchConsignee = Me.txtConsignee.Text
        Me.ThisScreenValues.SearchProduct = Me.txtProduct.Text
        Me.ThisScreenValues.SearchPOLCountry = Me.txtPOLCountry.Text
        Me.ThisScreenValues.SearchPOL = Me.txtPOL.Text
        Me.ThisScreenValues.SearchPODCountry = Me.txtPODCountry.Text
        Me.ThisScreenValues.SearchPOD = Me.txtPOD.Text
        Me.ThisScreenValues.SearchBreakerID = Me.txtBreaker.Text
        Me.ThisScreenValues.SearchStatus = Me.txtApproval.Text

        Return C_MESSAGENO.NORMAL
    End Function
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
        AddLangSetting(dicDisplayText, Me.btnCreateSales, "セールス新規作成", "Sales New")
        AddLangSetting(dicDisplayText, Me.btnCreateOperation, "オペ新規作成", "Ope New")
        AddLangSetting(dicDisplayText, Me.btnApply, "申請", "Apply")
        'AddLangSetting(dicDisplayText, Me.btnApplyCancel, "申請取消", "APPLY CANCEL")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excelダウンロード", "Excel Download")

        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")

        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        AddLangSetting(dicDisplayText, Me.lblShipper, "荷主", "Shipper")
        AddLangSetting(dicDisplayText, Me.lblConsignee, "荷受人", "Consignee")
        AddLangSetting(dicDisplayText, Me.lblProduct, "積載品", "Product")
        AddLangSetting(dicDisplayText, Me.lblPOLCountry, "POL Country", "POL Country")
        AddLangSetting(dicDisplayText, Me.lblPOL, "POL", "POL")
        AddLangSetting(dicDisplayText, Me.lblPODCountry, "POD Country", "POD Country")
        AddLangSetting(dicDisplayText, Me.lblPOD, "POD", "POD")
        AddLangSetting(dicDisplayText, Me.lblBreaker, "ブレーカーID", "Breaker ID")
        AddLangSetting(dicDisplayText, Me.lblApproval, "承認", "Status")

        AddLangSetting(dicDisplayText, Me.hdnConfirmTitle, "削除しますよろしいですか？", "Are you sure you want to delete?")
        AddLangSetting(dicDisplayText, Me.lblConfirmBrNoName, "BR NO", "BR NO")
        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub


    ''' <summary>
    ''' 一覧表のデータテーブルを取得する関数
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>TODO:引き継いだ条件を引数に格納し抽出を行う(味見は全件)</remarks>
    Private Function GetListDataTable() As DataTable
        Dim mapId As String = "GBT00002"
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得
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
        COA0020ProfViewSort.VARI = "Default"
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder
        Dim retDt As New DataTable
        '一旦生きているブレーカはすべて取得
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       '' AS OPERATION")
        sqlStat.AppendLine("      ,TIMSTP = cast(BS.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,'1' AS 'SELECT' ")
        sqlStat.AppendLine("      ,'0' AS HIDDEN ")
        sqlStat.AppendLine("      ,''  AS ACTION ")
        sqlStat.AppendLine("      ,BS.BRID AS BRID")
        sqlStat.AppendLine("      ,BS.BRBASEID AS BRBASEID")
        sqlStat.AppendLine("      ,BIIF.SUBID AS SUBID")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYFROM WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYFROM,'yyyy/MM/dd') END AS VALIDITYFROM")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYTO,'yyyy/MM/dd')   END AS VALIDITYTO")
        sqlStat.AppendLine("      ,DISABLED AS DISABLED")
        sqlStat.AppendFormat("      ,ISNULL(SP.{0}, ISNULL(AGS.{1},'')) AS SHIPPER", textCustomerTblField, textTraderTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(PD.{0},'') AS PRODUCTCODE", textProductTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CN.{0}, ISNULL(AGC.{1},'')) AS CONSIGNEE", textCustomerTblField, textTraderTblField).AppendLine()
        sqlStat.AppendLine("      ,BS.LOADCOUNTRY1 AS POLCOUNTRY")
        sqlStat.AppendLine("      ,BS.LOADPORT1 AS POL")
        sqlStat.AppendLine("      ,ISNULL(PT.AREANAME,'')  AS POLNAME")
        sqlStat.AppendLine("      ,BS.DELIVERYCOUNTRY1 AS PODCOUNTRY")
        sqlStat.AppendLine("      ,BS.DELIVERYPORT1 AS POD")
        sqlStat.AppendLine("      ,ISNULL(PT2.AREANAME,'')  AS PODNAME")
        sqlStat.AppendLine("      ,CASE WHEN BS.REMARK<>'' THEN '〇' ELSE '' END AS HASREMARK")
        sqlStat.AppendLine("      ,BS.NOOFTANKS")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FVL1.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FVL1.VALUE2,'') END AS POL1")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FVD1.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FVD1.VALUE2,'') END AS POD1")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(BIL2.BRID,'') = '' THEN '-' ELSE CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FVL2.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FVL2.VALUE2,'') END END AS POL2")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(BID2.BRID,'') = '' THEN '-' ELSE CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FVD2.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FVD2.VALUE2,'') END END AS POD2")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FVIF.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FVIF.VALUE2,'') END AS APPLYORDENY")
        sqlStat.AppendLine("      ,'' AS ""CHECK""")
        sqlStat.AppendLine("      ,'' AS NUMORDEROF")
        sqlStat.AppendLine("      ,'' AS DELETEFLAG")
        sqlStat.AppendLine("      ,BS.AGENTORGANIZER")
        sqlStat.AppendLine("      ,BIIF.APPLYID AS APPLYIDINFO")
        sqlStat.AppendLine("      ,BIIF.LASTSTEP AS STEPINFO")
        sqlStat.AppendLine("      ,BIL1.APPLYID AS APPLYIDPOL1")
        sqlStat.AppendLine("      ,BIL1.LASTSTEP AS STEPPOL1")
        sqlStat.AppendLine("      ,BID1.APPLYID AS APPLYIDPOD1")
        sqlStat.AppendLine("      ,BID1.LASTSTEP AS STEPPOD1")
        sqlStat.AppendLine("      ,BIL2.APPLYID AS APPLYIDPOL2")
        sqlStat.AppendLine("      ,BIL2.LASTSTEP AS STEPPOL2")
        sqlStat.AppendLine("      ,BID2.APPLYID AS APPLYIDPOD2")
        sqlStat.AppendLine("      ,BID2.LASTSTEP AS STEPPOD2")
        sqlStat.AppendLine("      ,ISNULL(AHL1.STATUS,'') AS STATUSL1")
        sqlStat.AppendLine("      ,ISNULL(AHD1.STATUS,'') AS STATUSD1")
        sqlStat.AppendLine("      ,ISNULL(AHL2.STATUS,'') AS STATUSL2")
        sqlStat.AppendLine("      ,ISNULL(AHD2.STATUS,'') AS STATUSD2")
        sqlStat.AppendLine("      ,ISNULL(AHIF.STATUS,'') AS STATUSIF")
        sqlStat.AppendLine("      ,ISNULL(AHL1.APPLICANTID,'') AS APPLICANTIDL1")
        sqlStat.AppendLine("      ,ISNULL(AHD1.APPLICANTID,'') AS APPLICANTIDD1")
        sqlStat.AppendLine("      ,ISNULL(AHL2.APPLICANTID,'') AS APPLICANTIDL2")
        sqlStat.AppendLine("      ,ISNULL(AHD2.APPLICANTID,'') AS APPLICANTIDD2")
        sqlStat.AppendLine("      ,ISNULL(AHIF.APPLICANTID,'') AS APPLICANTIDIF")
        sqlStat.AppendLine("      ,BS.JOTHIREAGE AS JOTHIREAGE")
        sqlStat.AppendLine("      ,BS.COMMERCIALFACTOR AS COMMERCIALFACTOR")

        sqlStat.AppendLine("      ,ISNULL((SELECT SUM(WBVL1.USD)")
        sqlStat.AppendLine("             FROM   GBT0003_BR_VALUE WBVL1")
        sqlStat.AppendLine("             WHERE    WBVL1.BRID        = BS.BRID")
        sqlStat.AppendLine("             AND      WBVL1.BRVALUEID   = BIL1.LINKID")
        sqlStat.AppendLine("             AND      WBVL1.DELFLG     <> 'Y'")
        sqlStat.AppendLine("             GROUP BY WBVL1.BRID, WBVL1.BRVALUEID ),0.0) --AS POL1COST")
        sqlStat.AppendLine("       + ISNULL((SELECT SUM(WBVD1.USD) ")
        sqlStat.AppendLine("             FROM   GBT0003_BR_VALUE WBVD1")
        sqlStat.AppendLine("             WHERE    WBVD1.BRID        = BS.BRID")
        sqlStat.AppendLine("             AND      WBVD1.BRVALUEID   = BID1.LINKID")
        sqlStat.AppendLine("             AND      WBVD1.DELFLG     <> 'Y'")
        sqlStat.AppendLine("             GROUP BY WBVD1.BRID, WBVD1.BRVALUEID ),0.0) --AS POD1COST")
        sqlStat.AppendLine("       + ISNULL((SELECT SUM(WBVL2.USD) ")
        sqlStat.AppendLine("             FROM   GBT0003_BR_VALUE WBVL2")
        sqlStat.AppendLine("             WHERE    WBVL2.BRID        = BS.BRID")
        sqlStat.AppendLine("             AND      WBVL2.BRVALUEID   = BIL2.LINKID")
        sqlStat.AppendLine("             AND      WBVL2.DELFLG     <> 'Y'")
        sqlStat.AppendLine("             GROUP BY WBVL2.BRID, WBVL2.BRVALUEID ),0.0) --AS POL2COST")
        sqlStat.AppendLine("       + ISNULL((SELECT SUM(WBVD2.USD) ")
        sqlStat.AppendLine("             FROM   GBT0003_BR_VALUE WBVD2")
        sqlStat.AppendLine("             WHERE    WBVD2.BRID        = BS.BRID")
        sqlStat.AppendLine("             AND      WBVD2.BRVALUEID   = BID2.LINKID")
        sqlStat.AppendLine("             AND      WBVD2.DELFLG     <> 'Y'")
        sqlStat.AppendLine("             GROUP BY WBVD2.BRID, WBVD2.BRVALUEID ),0.0) --AS POD2COST")
        sqlStat.AppendLine("       + BS.JOTHIREAGE + BS.COMMERCIALFACTOR + BS.FEE AS TOTALINVOICED")
        sqlStat.AppendLine("      ,(BS.LOADING + BS.STEAMING + BS.TIP + BS.EXTRA) AS ROOTDAYS")
        sqlStat.AppendLine("      ,CASE WHEN (BS.LOADING + BS.STEAMING + BS.TIP + BS.EXTRA) > 0 THEN")
        sqlStat.AppendLine("              CONVERT(DECIMAL(16,2),ROUND(CONVERT(DECIMAL(16,6),(BS.JOTHIREAGE / (BS.LOADING + BS.STEAMING + BS.TIP + BS.EXTRA))),2,1)) ")
        sqlStat.AppendLine("            ELSE 0.0 END AS PERDAY ")

        sqlStat.AppendLine("      ,BS.AMTREQUEST AS AMTREQUEST")
        sqlStat.AppendLine("      ,''  AS DELETEBTN")
        sqlStat.AppendLine("      ,ISNULL(OB.COUNT, '0')  AS ORDERQUANTITY")
        sqlStat.AppendLine("      ,BIL1.BRTYPE AS BRTYPE")
        '2019/10/28 コピー・削除可能対象をOFFICEから国に変更 START
        'sqlStat.AppendLine("      ,CASE WHEN BS.AGENTORGANIZER = '" & GBA00003UserSetting.OFFICECODE & "' THEN '1' ELSE '' END  AS SHOWBUTTON")
        sqlStat.AppendLine("      ,CASE WHEN BS.COUNTRYORGANIZER = '" & GBA00003UserSetting.COUNTRYCODE & "' THEN '1' ELSE '' END  AS SHOWBUTTON")
        '2019/10/28 コピー・削除可能対象をOFFICEから国に変更 END
        sqlStat.AppendLine("      ,'' AS FILLINGRATECHECK")
        sqlStat.AppendLine("      ,BS.PRODUCTWEIGHT AS PRODUCTWEIGHT")
        sqlStat.AppendLine("      ,BS.CAPACITY AS CAPACITY")
        sqlStat.AppendLine("      ,ISNULL(PD.GRAVITY,'') AS GRAVITY")
        sqlStat.AppendLine("      ,ISNULL(PD.HAZARDCLASS,'') AS HAZARDCLASS")
        sqlStat.AppendLine("      ,BS.ORIGINALCOPYBRID AS ORIGINALCOPYBRID")
        sqlStat.AppendLine("      ,BS.INITUSER AS INITUSER")
        sqlStat.AppendLine("  FROM GBT0002_BR_BASE BS ")
        sqlStat.AppendLine("  LEFT JOIN GBT0001_BR_INFO BIL1") 'ブレーカー(関連付け)
        sqlStat.AppendLine("    ON  BIL1.BRID         = BS.BRID")
        sqlStat.AppendLine("   AND  BIL1.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  BIL1.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  BIL1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  BIL1.TYPE         = 'POL1'")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AHL1") '承認履歴
        sqlStat.AppendLine("    ON  AHL1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AHL1.APPLYID      = BIL1.APPLYID")
        sqlStat.AppendLine("   AND  AHL1.STEP         = BIL1.LASTSTEP")
        sqlStat.AppendLine("   AND  AHL1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FVL1") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FVL1.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FVL1.KEYCODE      = AHL1.STATUS")
        sqlStat.AppendLine("   AND  FVL1.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVL1.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVL1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBT0001_BR_INFO BID1") 'ブレーカー(関連付け)
        sqlStat.AppendLine("    ON  BID1.BRID         = BS.BRID")
        sqlStat.AppendLine("   AND  BID1.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  BID1.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  BID1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  BID1.TYPE         = 'POD1'")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AHD1") '承認履歴
        sqlStat.AppendLine("    ON  AHD1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AHD1.APPLYID      = BID1.APPLYID")
        sqlStat.AppendLine("   AND  AHD1.STEP         = BID1.LASTSTEP")
        sqlStat.AppendLine("   AND  AHD1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FVD1") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FVD1.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FVD1.KEYCODE      = AHD1.STATUS")
        sqlStat.AppendLine("   AND  FVD1.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVD1.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVD1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBT0001_BR_INFO BIL2") 'ブレーカー(関連付け)
        sqlStat.AppendLine("    ON  BIL2.BRID         = BS.BRID")
        sqlStat.AppendLine("   AND  BIL2.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  BIL2.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  BIL2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  BIL2.TYPE         = 'POL2'")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AHL2") '承認履歴
        sqlStat.AppendLine("    ON  AHL2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AHL2.APPLYID      = BIL2.APPLYID")
        sqlStat.AppendLine("   AND  AHL2.STEP         = BIL2.LASTSTEP")
        sqlStat.AppendLine("   AND  AHL2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FVL2") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FVL2.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FVL2.KEYCODE      = AHL2.STATUS")
        sqlStat.AppendLine("   AND  FVL2.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVL2.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVL2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBT0001_BR_INFO BID2") 'ブレーカー(関連付け)
        sqlStat.AppendLine("    ON  BID2.BRID         = BS.BRID")
        sqlStat.AppendLine("   AND  BID2.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  BID2.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  BID2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  BID2.TYPE         = 'POD2'")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AHD2") '承認履歴
        sqlStat.AppendLine("    ON  AHD2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AHD2.APPLYID      = BID2.APPLYID")
        sqlStat.AppendLine("   AND  AHD2.STEP         = BID2.LASTSTEP")
        sqlStat.AppendLine("   AND  AHD2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FVD2") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FVD2.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FVD2.KEYCODE      = AHD2.STATUS")
        sqlStat.AppendLine("   AND  FVD2.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVD2.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVD2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBT0001_BR_INFO BIIF") 'ブレーカー(関連付け)
        sqlStat.AppendLine("    ON  BIIF.BRID         = BS.BRID")
        sqlStat.AppendLine("   AND  BIIF.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  BIIF.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  BIIF.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  BIIF.TYPE         = 'INFO'")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AHIF") '承認履歴
        sqlStat.AppendLine("    ON  AHIF.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AHIF.APPLYID      = BIIF.APPLYID")
        sqlStat.AppendLine("   AND  AHIF.STEP         = BIIF.LASTSTEP")
        sqlStat.AppendLine("   AND  AHIF.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FVIF") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FVIF.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FVIF.KEYCODE      = AHIF.STATUS")
        sqlStat.AppendLine("   AND  FVIF.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVIF.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  FVIF.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER SP") 'SHIPPER名称用JOIN
        sqlStat.AppendLine("    ON  SP.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  SP.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  SP.CUSTOMERCODE = BS.SHIPPER")
        sqlStat.AppendLine("   AND  SP.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  SP.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  SP.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  SP.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.SHIPPER & "','" & C_CUSTOMERTYPE.COMMON & "')")
        sqlStat.AppendLine("  LEFT JOIN GBM0004_CUSTOMER CN") 'CONSIGNEE名称用JOIN
        sqlStat.AppendLine("    ON  CN.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  CN.COUNTRYCODE  = BS.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("   AND  CN.CUSTOMERCODE = BS.CONSIGNEE")
        sqlStat.AppendLine("   AND  CN.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  CN.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  CN.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  CN.CUSTOMERTYPE IN('" & C_CUSTOMERTYPE.CONSIGNEE & "','" & C_CUSTOMERTYPE.COMMON & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER AGS") 'SHIPPER名称用JOIN
        sqlStat.AppendLine("    ON  AGS.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AGS.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  AGS.CARRIERCODE  = BS.SHIPPER")
        sqlStat.AppendLine("   AND  AGS.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  AGS.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  AGS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  AGS.CLASS        = '" & C_TRADER.CLASS.AGENT & "'")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER AGC") 'CONSIGNEE名称用JOIN
        sqlStat.AppendLine("    ON  AGC.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AGC.COUNTRYCODE  = BS.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("   AND  AGC.CARRIERCODE  = BS.CONSIGNEE")
        sqlStat.AppendLine("   AND  AGC.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  AGC.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  AGC.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  AGC.CLASS        = '" & C_TRADER.CLASS.AGENT & "'")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PD.PRODUCTCODE  = BS.PRODUCTCODE")
        sqlStat.AppendLine("   AND  PD.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  PD.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD.ENABLED      = @ENABLED")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT") 'PORT名称用JOIN
        sqlStat.AppendLine("    ON  PT.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PT.COUNTRYCODE  = BS.LOADCOUNTRY1")
        sqlStat.AppendLine("   AND  PT.PORTCODE     = BS.LOADPORT1")
        sqlStat.AppendLine("   AND  PT.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  PT.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  PT.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT2") 'PORT名称用JOIN
        sqlStat.AppendLine("    ON  PT2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PT2.COUNTRYCODE  = BS.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("   AND  PT2.PORTCODE     = BS.DELIVERYPORT1")
        sqlStat.AppendLine("   AND  PT2.STYMD       <= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  PT2.ENDYMD      >= (CASE BS.VALIDITYTO WHEN '1900/01/01' THEN getdate() ELSE BS.VALIDITYTO END)")
        sqlStat.AppendLine("   AND  PT2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN ") '紐付ORDER数取得用JOIN
        sqlStat.AppendLine("  (SELECT COUNT(*) AS COUNT,BRID FROM GBT0004_ODR_BASE")
        sqlStat.AppendLine("  WHERE DELFLG = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("  GROUP BY BRID ) AS OB")
        sqlStat.AppendLine("    ON  OB.BRID  = BS.BRID")
        sqlStat.AppendLine(" WHERE BS.DELFLG  <> @DELFLG")
        sqlStat.AppendLine("   AND NOT EXISTS (SELECT 1 ") 'リペアブレーカーは一覧対象外
        sqlStat.AppendLine("                     FROM GBT0001_BR_INFO REPCHK")
        sqlStat.AppendLine("                    WHERE REPCHK.BRTYPE    = '" & C_BRTYPE.REPAIR & "'")
        sqlStat.AppendLine("                      AND REPCHK.BRID      = BS.BRID")
        sqlStat.AppendLine("                      AND REPCHK.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("                      AND REPCHK.TYPE      = 'INFO')")

        '動的検索条件のSQL付与
        If Me.hdnEndYMD.Value <> "" Then
            'VALIDITY FROM
            sqlStat.AppendLine("   AND BS.VALIDITYFROM   <= @VALIDITYFROM")
        End If

        If Me.hdnStYMD.Value <> "" Then
            'VALIDITY TO
            sqlStat.AppendLine("   AND BS.VALIDITYTO   >= @VALIDITYTO")
        End If

        If Me.hdnShipper.Value <> "" Then
            'SHIPPER
            sqlStat.AppendLine("   AND BS.SHIPPER   = @SHIPPER")
        End If

        If Me.hdnConsignee.Value <> "" Then
            'CONSIGNEE
            sqlStat.AppendLine("   AND BS.CONSIGNEE   = @CONSIGNEE")
        End If

        If Me.hdnSearchBreakerType.Value <> "" Then
            'ブレーカータイプ
            sqlStat.AppendLine("   AND BIL1.BRTYPE   = @BREAKERTYPE")
        End If

        If Me.hdnApproval.Value <> "" Then
            'ステータス
            sqlStat.AppendLine("   AND AHIF.STATUS   = @APPROVAL")
        End If
        Dim countryCode As String = ""
        Dim officeCode As String = ""
        If Me.hdnOffice.Value <> "" Then
            officeCode = Me.hdnOffice.Value
            sqlStat.AppendLine("   AND (    BS.AGENTORGANIZER = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOL1      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOL2      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOD1      = @OFFICECODE")
            sqlStat.AppendLine("         OR BS.AGENTPOD2      = @OFFICECODE")
            sqlStat.AppendLine("       )")
        End If
        'APPLOVAL , OFFICEは保留
        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            sqlCmd.CommandTimeout = 60
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 20)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            Dim paramEnabled As SqlParameter = sqlCmd.Parameters.Add("@ENABLED", SqlDbType.NVarChar, 1)
            Dim paramLangDisp As SqlParameter = sqlCmd.Parameters.Add("@LANGDISP", SqlDbType.NVarChar, 20)
            ' Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            Dim paramOfficeCode As SqlParameter = sqlCmd.Parameters.Add("@OFFICECODE", SqlDbType.NVarChar, 20)
            'SQLパラメータ(動的変化あり)
            Dim paramValidityfrom As SqlParameter = Nothing
            Dim paramValidityto As SqlParameter = Nothing
            Dim paramShipper As SqlParameter = Nothing
            Dim paramConsignee As SqlParameter = Nothing
            Dim paramPort As SqlParameter = Nothing
            Dim paramBreakerType As SqlParameter = Nothing
            Dim paramApproval As SqlParameter = Nothing
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramDelFlg.Value = CONST_FLAG_YES
            paramEnabled.Value = CONST_FLAG_YES
            paramLangDisp.Value = COA0019Session.LANGDISP
            'paramCountryCode.Value = countryCode
            paramOfficeCode.Value = officeCode
            If Me.hdnEndYMD.Value <> "" Then '検索条件のTOをFROMと突き合わせ
                'VALIDITY FROM
                paramValidityfrom = sqlCmd.Parameters.Add("@VALIDITYFROM", SqlDbType.Date)
                'paramValidityfrom.Value = Date.ParseExact(Me.hdnEndYMD.Value, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
                paramValidityfrom.Value = FormatDateYMD(Me.hdnEndYMD.Value, GBA00003UserSetting.DATEFORMAT)
            End If

            If Me.hdnStYMD.Value <> "" Then '検索条件のFROMをTOと突き合わせ
                'VALIDITY TO
                paramValidityto = sqlCmd.Parameters.Add("@VALIDITYTO", SqlDbType.Date)
                'paramValidityto.Value = Date.ParseExact(Me.hdnStYMD.Value, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
                paramValidityto.Value = FormatDateYMD(Me.hdnStYMD.Value, GBA00003UserSetting.DATEFORMAT)
            End If

            If Me.hdnShipper.Value <> "" Then
                'SHIPPER
                paramShipper = sqlCmd.Parameters.Add("@SHIPPER", SqlDbType.NVarChar, 20)
                paramShipper.Value = Me.hdnShipper.Value
            End If

            If Me.hdnConsignee.Value <> "" Then
                'CONSIGNEE
                paramConsignee = sqlCmd.Parameters.Add("@CONSIGNEE", SqlDbType.NVarChar, 20)
                paramConsignee.Value = Me.hdnConsignee.Value
            End If

            If Me.hdnSearchBreakerType.Value <> "" Then
                Dim brType As String = ""
                'ブレーカータイプ
                paramBreakerType = sqlCmd.Parameters.Add("@BREAKERTYPE", SqlDbType.NVarChar, 20)
                Select Case Me.hdnSearchBreakerType.Value
                    Case "01SALES"
                        brType = C_BRTYPE.SALES

                    Case "02OPE"
                        brType = C_BRTYPE.OPERATION

                End Select

                paramBreakerType.Value = brType
            End If

            If Me.hdnApproval.Value <> "" Then
                'Approval
                paramApproval = sqlCmd.Parameters.Add("@APPROVAL", SqlDbType.NVarChar, 20)
                paramApproval.Value = Me.hdnApproval.Value
            End If

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
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
        'COA0013TableObject.SCROLLTYPE = "2"
        COA0013TableObject.SCROLLTYPE = "3"
        COA0013TableObject.LEVENT = "ondblclick"
        COA0013TableObject.LFUNC = "ListDbClick"
        COA0013TableObject.TITLEOPT = True
        COA0013TableObject.NOCOLUMNWIDTHOPT = 50
        COA0013TableObject.OPERATIONCOLUMNWIDTHOPT = -1
        COA0013TableObject.USERSORTOPT = 1
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.APPLYチェックがついているチェックボックスオブジェクトをチェック状態にする
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            For Each targetCheckBoxId As String In {"CHECK"} '複数チェックボックスを配置している場合は配列に追加

                '申請チェックボックスの加工
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
            '申請チェックの使用可否制御
            For Each dr As DataRow In listData.Rows

                If Convert.ToString(dr.Item("FILLINGRATECHECK")).Equals("ERROR") OrElse
                    {C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE, C_APP_STATUS.APPLYING}.Contains(Trim(Convert.ToString(dr.Item("STATUSIF")))) OrElse
                    Not ((Convert.ToString(dr.Item("POL1")) = Me.hdnComplete.Value AndAlso Convert.ToString(dr.Item("POD1")) = Me.hdnComplete.Value) AndAlso
                    ((Convert.ToString(dr.Item("POL2")) = Me.hdnComplete.Value AndAlso Convert.ToString(dr.Item("POD2")) = Me.hdnComplete.Value) Or
                    (Convert.ToString(dr.Item("POL2")) = C_HYPHEN AndAlso Convert.ToString(dr.Item("POD2")) = C_HYPHEN))) Then

                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & "CHECK" & Convert.ToInt32(dr.Item("LINECNT"))
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        chkObj.Enabled = False
                    End If

                End If

                Dim btnCopy = WF_LISTAREA.FindControl("btn" & Me.WF_LISTAREA.ID & "ACTION" + dr.Item("LINECNT").ToString)
                If dr.Item("SHOWBUTTON").ToString = "1" AndAlso Convert.ToString(dr.Item("DISABLED")) = CONST_FLAG_NO Then
                    btnCopy.Visible = True
                Else
                    btnCopy.Visible = False
                End If

                Dim btnDelete = WF_LISTAREA.FindControl("btn" & Me.WF_LISTAREA.ID & "DELETEBTN" + dr.Item("LINECNT").ToString)
                If dr.Item("SHOWBUTTON").ToString = "1" AndAlso Not {C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE}.Contains(dr.Item("STATUSIF").ToString.Trim) Then
                    btnDelete.Visible = True
                Else
                    btnDelete.Visible = False
                End If

                If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & "CHECK" & Convert.ToInt32(dr.Item("LINECNT"))
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        chkObj.Enabled = False
                    End If

                    btnCopy.Visible = False
                    btnDelete.Visible = False
                End If

            Next
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
            .Add("LINECNT", GetType(Integer))            'DBの固定フィールド
            .Add("OPERATION", GetType(String))           'DBの固定フィールド
            .Add("TIMSTP", GetType(String))              'DBの固定フィールド
            .Add("SELECT", GetType(Integer))             'DBの固定フィールド
            .Add("HIDDEN", GetType(Integer))
            '個別項目
            .Add("ACTION", GetType(String))
            .Add("BRID", GetType(String))
            .Add("BRBASEID", GetType(String))
            .Add("SUBID", GetType(String))
            .Add("VALIDITYFROM", GetType(String))
            .Add("VALIDITYTO", GetType(String))
            .Add("SHIPPER", GetType(String))
            .Add("PRODUCTCODE", GetType(String))
            .Add("CONSIGNEE", GetType(String))
            .Add("POLCOUNTRY", GetType(String))
            .Add("POL", GetType(String))
            .Add("POLNAME", GetType(String))
            .Add("PODCOUNTRY", GetType(String))
            .Add("POD", GetType(String))
            .Add("PODNAME", GetType(String))
            .Add("HASREMARK", GetType(String))
            .Add("NOOFTANKS", GetType(String))
            .Add("POL1", GetType(String))
            .Add("POD1", GetType(String))
            .Add("POL2", GetType(String))
            .Add("POD2", GetType(String))
            .Add("APPLYORDENY", GetType(String))
            .Add("CHECK", GetType(String))
            .Add("DELETEFLAG", GetType(String))
            .Add("NUMORDEROF", GetType(String))
            .Add("AGENTORGANIZER", GetType(String))

            .Add("APPLYIDINFO", GetType(String))
            .Add("STEPINFO", GetType(String))
            .Add("APPLYIDPOL1", GetType(String))
            .Add("STEPPOL1", GetType(String))
            .Add("APPLYIDPOD1", GetType(String))
            .Add("STEPPOD1", GetType(String))
            .Add("APPLYIDPOL2", GetType(String))
            .Add("STEPPOL2", GetType(String))
            .Add("APPLYIDPOD2", GetType(String))
            .Add("STEPPOD2", GetType(String))

            .Add("STATUSL1", GetType(String))
            .Add("STATUSD1", GetType(String))
            .Add("STATUSL2", GetType(String))
            .Add("STATUSD2", GetType(String))
            .Add("STATUSIF", GetType(String))

            .Add("APPLICANTIDL1", GetType(String))
            .Add("APPLICANTIDD1", GetType(String))
            .Add("APPLICANTIDL2", GetType(String))
            .Add("APPLICANTIDD2", GetType(String))
            .Add("APPLICANTIDIF", GetType(String))

            .Add("JOTHIREAGE", GetType(String))
            .Add("COMMERCIALFACTOR", GetType(String))
            .Add("TOTALINVOICE", GetType(String))
            .Add("ROOTDAYS", GetType(String))
            .Add("PERDAY", GetType(String))

            .Add("AMTREQUEST", GetType(String))

            '削除
            .Add("DELETEBTN", GetType(String))
            .Add("ORDERQUANTITY", GetType(String))

            'BREAKERTYPE
            .Add("BRTYPE", GetType(String))

            .Add("SHOWBUTTON", GetType(String))
            .Add("DISABLED", GetType(String))
            'FillingRateチェック関連
            .Add("FILLINGRATECHECK", GetType(String))
            .Add("PRODUCTWEIGHT", GetType(String))
            .Add("CAPACITY", GetType(String))
            .Add("HAZARDCLASS", GetType(String))
            .Add("GRAVITY", GetType(String))
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
        Me.hdnXMLsaveFileRet.Value = hdnXMLsaveFile.Value

        Dim selectedRow As DataRow = dt.Rows(rowId)
        Dim brId As String = Convert.ToString(selectedRow.Item("BRID"))
        Me.hdnSelectedBrId.Value = brId

        Me.hdnPol1Status.Value = Convert.ToString(selectedRow.Item("POL1"))
        Me.hdnPol2Status.Value = Convert.ToString(selectedRow.Item("POL2"))
        Me.hdnPod1Status.Value = Convert.ToString(selectedRow.Item("POD1"))
        Me.hdnPod2Status.Value = Convert.ToString(selectedRow.Item("POD2"))

        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = "GBT00002R"
        COA0012DoUrl.VARIP = "GB_ShowDetail"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
            Return
        End If
        Session("MAPmapid") = "GBT00002R"
        Session("MAPvariant") = "GB_ShowDetail"
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

        '絞り込み処理を外だし
        SetnExtractDt(dt)
        ''フィルタでの絞り込みを利用するか確認
        'Dim isFillterOff As Boolean = True
        'If Me.txtBreaker.Text.Trim <> "" OrElse Me.txtPOL.Text.Trim <> "" OrElse
        '        Me.txtPOD.Text.Trim <> "" OrElse Me.txtProduct.Text.Trim <> "" OrElse
        '        Me.txtPOLCountry.Text.Trim <> "" OrElse Me.txtPODCountry.Text.Trim <> "" OrElse
        '        Me.txtShipper.Text.Trim <> "" OrElse Me.txtConsignee.Text.Trim <> "" OrElse
        '        Me.txtApproval.Text.Trim <> "" Then
        '    isFillterOff = False
        'End If

        'For Each dr As DataRow In dt.Rows
        '    dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
        '    'フィルタ使用時の場合
        '    If isFillterOff = False Then
        '        '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
        '        If Not ((Me.txtBreaker.Text.Trim = "" OrElse Convert.ToString(dr("BRID")).ToUpper.StartsWith(Me.txtBreaker.Text.Trim.ToUpper)) _
        '          AndAlso (Me.txtPOLCountry.Text.Trim = "" OrElse Convert.ToString(dr("POLCOUNTRY")).Trim.ToUpper.Equals(Me.txtPOLCountry.Text.Trim.ToUpper)) _
        '          AndAlso (Me.txtPOL.Text.Trim = "" OrElse Convert.ToString(dr("POL")).Trim.ToUpper.Equals(Me.txtPOL.Text.Trim.ToUpper)) _
        '          AndAlso (Me.txtPODCountry.Text.Trim = "" OrElse Convert.ToString(dr("PODCOUNTRY")).Trim.ToUpper.Equals(Me.txtPODCountry.Text.Trim.ToUpper)) _
        '          AndAlso (Me.txtPOD.Text.Trim = "" OrElse Convert.ToString(dr("POD")).Trim.ToUpper.Equals(Me.txtPOD.Text.Trim.ToUpper)) _
        '          AndAlso (Me.txtShipper.Text.Trim = "" OrElse (Me.lblShipperText.Text.Trim <> "" AndAlso Convert.ToString(dr("SHIPPER")).Trim.ToUpper.Equals(Me.lblShipperText.Text.Trim.ToUpper))) _
        '          AndAlso (Me.txtConsignee.Text.Trim = "" OrElse (Me.lblConsigneeText.Text.Trim <> "" AndAlso Convert.ToString(dr("CONSIGNEE")).Trim.ToUpper.Equals(Me.lblConsigneeText.Text.Trim.ToUpper))) _
        '          AndAlso (Me.txtProduct.Text.Trim = "" OrElse (Me.lblProductText.Text.Trim <> "" AndAlso Convert.ToString(dr("PRODUCTCODE")).Trim.ToUpper.Equals(Me.lblProductText.Text.Trim.ToUpper))) _
        '          AndAlso (Me.txtApproval.Text.Trim = "" OrElse Convert.ToString(dr("STATUSIF")).Trim.ToUpper.Equals(Me.txtApproval.Text.Trim.ToUpper))
        '        ) Then
        '            dr.Item("HIDDEN") = 1
        '        End If
        '    End If
        'Next
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
    ''' 絞り込みDataTable更新
    ''' </summary>
    Private Sub SetnExtractDt(dt As DataTable)

        'フィルタでの絞り込みを利用するか確認
        Dim isFillterOff As Boolean = True
        If Me.txtBreaker.Text.Trim <> "" OrElse Me.txtPOL.Text.Trim <> "" OrElse
                Me.txtPOD.Text.Trim <> "" OrElse Me.txtProduct.Text.Trim <> "" OrElse
                Me.txtPOLCountry.Text.Trim <> "" OrElse Me.txtPODCountry.Text.Trim <> "" OrElse
                Me.txtShipper.Text.Trim <> "" OrElse Me.txtConsignee.Text.Trim <> "" OrElse
                Me.txtApproval.Text.Trim <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.txtBreaker.Text.Trim = "" OrElse Convert.ToString(dr("BRID")).ToUpper.StartsWith(Me.txtBreaker.Text.Trim.ToUpper)) _
                  AndAlso (Me.txtPOLCountry.Text.Trim = "" OrElse Convert.ToString(dr("POLCOUNTRY")).Trim.ToUpper.Equals(Me.txtPOLCountry.Text.Trim.ToUpper)) _
                  AndAlso (Me.txtPOL.Text.Trim = "" OrElse Convert.ToString(dr("POL")).Trim.ToUpper.Equals(Me.txtPOL.Text.Trim.ToUpper)) _
                  AndAlso (Me.txtPODCountry.Text.Trim = "" OrElse Convert.ToString(dr("PODCOUNTRY")).Trim.ToUpper.Equals(Me.txtPODCountry.Text.Trim.ToUpper)) _
                  AndAlso (Me.txtPOD.Text.Trim = "" OrElse Convert.ToString(dr("POD")).Trim.ToUpper.Equals(Me.txtPOD.Text.Trim.ToUpper)) _
                  AndAlso (Me.txtShipper.Text.Trim = "" OrElse (Me.lblShipperText.Text.Trim <> "" AndAlso Convert.ToString(dr("SHIPPER")).Trim.ToUpper.Equals(Me.lblShipperText.Text.Trim.ToUpper))) _
                  AndAlso (Me.txtConsignee.Text.Trim = "" OrElse (Me.lblConsigneeText.Text.Trim <> "" AndAlso Convert.ToString(dr("CONSIGNEE")).Trim.ToUpper.Equals(Me.lblConsigneeText.Text.Trim.ToUpper))) _
                  AndAlso (Me.txtProduct.Text.Trim = "" OrElse (Me.lblProductText.Text.Trim <> "" AndAlso Convert.ToString(dr("PRODUCTCODE")).Trim.ToUpper.Equals(Me.lblProductText.Text.Trim.ToUpper))) _
                  AndAlso (Me.txtApproval.Text.Trim = "" OrElse Convert.ToString(dr("STATUSIF")).Trim.ToUpper.Equals(Me.txtApproval.Text.Trim.ToUpper))
                ) Then
                    dr.Item("HIDDEN") = 1
                End If
            End If
        Next

    End Sub
    ''' <summary>
    ''' 当画面のHiddenエリアに前画面の検索条件を格納
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        If TypeOf Page.PreviousPage Is GBT00002SELECT Then
            '検索画面の場合
            Dim prevObj As GBT00002SELECT = DirectCast(Page.PreviousPage, GBT00002SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtStYMD", Me.hdnStYMD},
                                                                           {"txtEndYMD", Me.hdnEndYMD},
                                                                           {"txtShipper", Me.hdnShipper},
                                                                           {"txtConsignee", Me.hdnConsignee},
                                                                           {"txtApproval", Me.hdnApproval},
                                                                           {"txtOffice", Me.hdnOffice},
                                                                           {"rblBreakerType", Me.hdnSearchBreakerType}}

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
        ElseIf TypeOf Page.PreviousPage Is GBT00001NEWBREAKER Then
            '新規作成画面の場合
            Dim prevObj As GBT00001NEWBREAKER = DirectCast(Page.PreviousPage, GBT00001NEWBREAKER)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPort", Me.hdnPort},
                                                                        {"hdnApproval", Me.hdnApproval},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchBreakerType", Me.hdnSearchBreakerType}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmphdn.Value
                End If
            Next
            '絞り込み条件
            Me.txtShipper.Text = prevObj.GBT00002RValues.SearchShipper
            Me.txtConsignee.Text = prevObj.GBT00002RValues.SearchConsignee
            Me.txtProduct.Text = prevObj.GBT00002RValues.SearchProduct
            Me.txtPOLCountry.Text = prevObj.GBT00002RValues.SearchPOLCountry
            Me.txtPOL.Text = prevObj.GBT00002RValues.SearchPOL
            Me.txtPODCountry.Text = prevObj.GBT00002RValues.SearchPODCountry
            Me.txtPOD.Text = prevObj.GBT00002RValues.SearchPOD
            Me.txtBreaker.Text = prevObj.GBT00002RValues.SearchBreakerID
            Me.txtApproval.Text = prevObj.GBT00002RValues.SearchStatus

        ElseIf TypeOf Page.PreviousPage Is GBT00001BREAKER Then
            '単票画面の場合
            Dim prevObj As GBT00001BREAKER = DirectCast(Page.PreviousPage, GBT00001BREAKER)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPort", Me.hdnPort},
                                                                        {"hdnApproval", Me.hdnApproval},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchBreakerType", Me.hdnSearchBreakerType}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmphdn.Value
                End If
            Next

            Dim tmpXMLFile As Control = prevObj.FindControl("hdnXMLsaveFileRet")

            If tmpXMLFile IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpXMLFile, HiddenField)
                Me.hdnXMLsaveFileRet.Value = tmphdn.Value
            End If
            '絞り込み条件
            Me.txtShipper.Text = prevObj.GBT00002RValues.SearchShipper
            Me.txtConsignee.Text = prevObj.GBT00002RValues.SearchConsignee
            Me.txtProduct.Text = prevObj.GBT00002RValues.SearchProduct
            Me.txtPOLCountry.Text = prevObj.GBT00002RValues.SearchPOLCountry
            Me.txtPOL.Text = prevObj.GBT00002RValues.SearchPOL
            Me.txtPODCountry.Text = prevObj.GBT00002RValues.SearchPODCountry
            Me.txtPOD.Text = prevObj.GBT00002RValues.SearchPOD
            Me.txtBreaker.Text = prevObj.GBT00002RValues.SearchBreakerID
            Me.txtApproval.Text = prevObj.GBT00002RValues.SearchStatus

        ElseIf TypeOf Page.PreviousPage Is GBT00002RESULT Then
            '同画面の場合
            Dim prevObj As GBT00002RESULT = DirectCast(Page.PreviousPage, GBT00002RESULT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnStYMD", Me.hdnStYMD},
                                                                        {"hdnEndYMD", Me.hdnEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPort", Me.hdnPort},
                                                                        {"hdnApproval", Me.hdnApproval},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSearchBreakerType", Me.hdnSearchBreakerType}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)

                If tmpCont IsNot Nothing Then
                    Dim tmphdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmphdn.Value
                End If
            Next

            Dim tmpMsg As Control = prevObj.FindControl("hdnMsgId")

            If tmpMsg IsNot Nothing Then
                Dim tmphdn As HiddenField = DirectCast(tmpMsg, HiddenField)
                Me.hdnMsgId.Value = tmphdn.Value
            End If

            '絞り込み条件
            Me.txtShipper.Text = prevObj.ThisScreenValues.SearchShipper
            Me.txtConsignee.Text = prevObj.ThisScreenValues.SearchConsignee
            Me.txtProduct.Text = prevObj.ThisScreenValues.SearchProduct
            Me.txtPOLCountry.Text = prevObj.ThisScreenValues.SearchPOLCountry
            Me.txtPOL.Text = prevObj.ThisScreenValues.SearchPOL
            Me.txtPODCountry.Text = prevObj.ThisScreenValues.SearchPODCountry
            Me.txtPOD.Text = prevObj.ThisScreenValues.SearchPOD
            Me.txtBreaker.Text = prevObj.ThisScreenValues.SearchBreakerID
            Me.txtApproval.Text = prevObj.ThisScreenValues.SearchStatus

        End If
        '絞り込み条件名称更新
        txtShipper_Change()
        txtConsignee_Change()
        txtProduct_Change()
        txtPOLCountry_Change()
        txtPOL_Change()
        txtPODCountry_Change()
        txtPOD_Change()
        txtApproval_Change()

    End Sub

    ''' <summary>
    ''' Country一覧を取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetCountry(Optional CountryCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT COUNTRYCODE AS CODE")
        sqlStat.AppendLine("      ,NAMES AS NAME")
        sqlStat.AppendLine("      ,COUNTRYCODE + ':' + NAMES AS LISTBOXNAME")
        sqlStat.AppendLine("  FROM GBM0001_COUNTRY")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If CountryCode <> "" Then
            sqlStat.AppendLine("   AND COUNTRYCODE    = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   AND STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY COUNTRYCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@COUNTRYCODE", SqlDbType.NVarChar, 20).Value = CountryCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt

    End Function

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
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@PORTCODE", SqlDbType.NVarChar, 20).Value = portCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
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

            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20).Value = customerCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
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
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20).Value = customerCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
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
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@PRODUCTCODE", SqlDbType.NVarChar, 20).Value = productCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@ENABLED", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function

    ''' <summary>
    ''' [絞り込み条件]POLCountryコード変更時イベント
    ''' </summary>
    Public Sub txtPOLCountry_Change()
        Dim polcountry As String = Me.txtPOLCountry.Text.Trim
        Me.lblPOLCountryText.Text = ""
        If polcountry = "" Then
            Return
        End If

        Dim dt As DataTable = GetCountry(polcountry)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Me.txtPOLCountry.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblPOLCountryText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
        End If
    End Sub

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
    ''' [絞り込み条件]PODCountryコード変更時イベント
    ''' </summary>
    Public Sub txtPODCountry_Change()
        Dim podcountry As String = Me.txtPODCountry.Text.Trim
        Me.lblPODCountryText.Text = ""
        If podcountry = "" Then
            Return
        End If

        Dim dt As DataTable = GetCountry(podcountry)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Me.txtPODCountry.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblPODCountryText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
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
        Dim shipper As String = Me.txtShipper.Text.Trim.ToUpper
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
        Dim consignee As String = Me.txtConsignee.Text.Trim.ToUpper
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
        Dim product As String = Me.txtProduct.Text.Trim.ToUpper
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
    ''' <summary>
    ''' 承認リストアイテムを設定
    ''' </summary>
    Private Function SetApprovalListItem(selectedValue As String) As String
        Dim COA0017FixValue As New BASEDLL.COA0017FixValue                 'FIXVALUE Get
        Dim retCode As String = C_MESSAGENO.NORMAL

        'リストクリア
        Me.lbApproval.Items.Clear()

        'ユーザＩＤListBox設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "APPROVAL"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            COA0017FixValue.LISTBOX1 = Me.lbApproval
        Else
            COA0017FixValue.LISTBOX2 = Me.lbApproval
        End If
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            If COA0019Session.LANGDISP = C_LANG.JA Then
                Me.lbApproval = DirectCast(COA0017FixValue.LISTBOX1, ListBox)
            Else
                Me.lbApproval = DirectCast(COA0017FixValue.LISTBOX2, ListBox)
            End If
        Else
            retCode = COA0017FixValue.ERR
        End If
        Return retCode
    End Function

    ''' <summary>
    ''' 承認名設定
    ''' </summary>
    Public Sub txtApproval_Change()
        Try
            Me.lblApprovalText.Text = ""
            If Me.txtApproval.Text.Trim = "" Then
                Return
            End If
            If SetApprovalListItem(Me.txtApproval.Text) = C_MESSAGENO.NORMAL AndAlso Me.lbApproval.Items.Count > 0 Then
                Dim findListItem = Me.lbApproval.Items.FindByValue(Me.txtApproval.Text)
                If findListItem IsNot Nothing Then
                    Me.lblApprovalText.Text = findListItem.Text
                Else
                    Dim findListItemUpper = Me.lbApproval.Items.FindByValue(Me.txtApproval.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Me.lblApprovalText.Text = findListItemUpper.Text
                        Me.txtApproval.Text = findListItemUpper.Value
                    End If
                End If
            End If

        Catch ex As Exception
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = C_MESSAGENO.EXCEPTION
            COA0003LogFile.COA0003WriteLog()
        End Try
    End Sub
    ''' <summary>
    ''' 項目非活性制御
    ''' </summary>
    Public Sub SetEnableItem()

        If Me.hdnSearchBreakerType.Value <> "" Then

            Select Case Me.hdnSearchBreakerType.Value
                Case "01SALES"
                    Me.btnCreateSales.Visible = True
                    Me.btnCreateOperation.Visible = False
                Case "02OPE"
                    Me.btnCreateSales.Visible = False
                    Me.btnCreateOperation.Visible = True
            End Select
        End If

        '画面モード（更新・参照）設定
        If Convert.ToString(HttpContext.Current.Session("MAPpermitcode")) <> "2" Then
            Me.btnApply.Visible = False
            Me.btnCreateSales.Visible = False
            Me.btnCreateOperation.Visible = False
        End If

    End Sub

    ''' <summary>
    ''' Complete設定
    ''' </summary>
    Public Sub SetComplete(Optional sqlCon As SqlConnection = Nothing)

        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("Select  VALUE1,VALUE2 ")
        sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
        sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
        sqlStat.AppendLine("            AND KEYCODE = @KEYCODE")
        sqlStat.AppendLine("            AND STYMD  <= @STYMD")
        sqlStat.AppendLine("            AND ENDYMD >= @ENDYMD")
        sqlStat.AppendLine("            AND DELFLG <> @DELFLG")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = "APPROVAL"
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = C_APP_STATUS.COMPLETE
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get FixValue error")
                    End If

                    If COA0019Session.LANGDISP = C_LANG.JA Then
                        Me.hdnComplete.Value = Convert.ToString(dt.Rows(0).Item("VALUE1"))
                    Else
                        Me.hdnComplete.Value = Convert.ToString(dt.Rows(0).Item("VALUE2"))
                    End If
                End Using

            End Using
        Catch ex As Exception
            Throw
        Finally
            If canCloseConnect = True Then
                If sqlCon IsNot Nothing Then
                    sqlCon.Close()
                    sqlCon.Dispose()
                End If
            End If
        End Try
    End Sub
    ''' <summary>
    ''' フィリングレートチェック結果をFILLINGRATECHECKフィールドに格納
    ''' </summary>
    ''' <param name="dt"></param>
    Private Sub UpdateFillingRate(dt As DataTable)
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If

        For Each dr As DataRow In dt.Rows
            'オペブレはフィリングレートチェック除外
            If dr("BRTYPE").Equals(C_BRTYPE.OPERATION) Then
                Continue For
            End If
            '
            dr("FILLINGRATECHECK") = "ERROR"
            Dim dummyDec As Decimal
            For Each txtObj As String In {dr("PRODUCTWEIGHT"), dr("GRAVITY"), dr("CAPACITY")}
                If txtObj.Trim = "" OrElse Decimal.TryParse(txtObj.Trim, dummyDec) = False Then
                    Continue For
                End If
            Next
            Dim hcls = Convert.ToString(dr("HAZARDCLASS"))
            Dim isHazard As String = ""
            If hcls = "" Then
                isHazard = "0"
            Else
                isHazard = "1"
            End If

            Dim weight As Decimal = DecimalStringToDecimal(Convert.ToString(dr("PRODUCTWEIGHT")))
            Dim gravity As Decimal = DecimalStringToDecimal(Convert.ToString(dr("GRAVITY")))
            Dim capacity As Decimal = 0

            capacity = DecimalStringToDecimal(Convert.ToString(dr("CAPACITY")))
            If capacity = 0 OrElse gravity = 0 Then
                '計算できないのでエラー
                Continue For
            End If

            Dim fillingRate As Decimal = weight / (capacity * gravity) * 100
            'Me.txtTankFillingRate.Text = NumberFormat(fillingRate, countryCode) & "%"
            'Me.txtTankFillingRate.Text = fillingRate.ToString("#,##0.00") & "%"
            Dim highValue As Decimal = 95
            Dim lowValue As Decimal = 70

            If isHazard = "1" Then
                highValue = 95
                lowValue = 80
            End If
            If fillingRate < lowValue OrElse fillingRate > highValue Then
                'チェックエラー
                Continue For
            Else
                '積載量閾値問題なし
                dr("FILLINGRATECHECK") = ""
            End If
        Next
    End Sub
    ''' <summary>
    ''' Decimal文字列を数字に変換
    ''' </summary>
    ''' <param name="dblString"></param>
    ''' <returns></returns>
    Private Function DecimalStringToDecimal(dblString As String) As Decimal
        Dim tmpDouble As Decimal = 0
        If Decimal.TryParse(dblString, tmpDouble) Then
            Return tmpDouble
        Else
            Return 0
        End If
    End Function

    ''' <summary>
    ''' 当画面情報保持クラス
    ''' </summary>
    <Serializable>
    Public Class GBT00002RValues
        ''' <summary>
        ''' 絞り込み条件(Shipper)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchShipper As String = ""
        ''' <summary>
        ''' 絞り込み条件(Consignee)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchConsignee As String = ""
        ''' <summary>
        ''' 絞り込み条件(Consignee)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchProduct As String = ""
        ''' <summary>
        ''' 絞り込み条件(POLCountry)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchPOLCountry As String = ""
        ''' <summary>
        ''' 絞り込み条件(POL)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchPOL As String = ""
        ''' <summary>
        ''' 絞り込み条件(PODCountry)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchPODCountry As String = ""
        ''' <summary>
        ''' 絞り込み条件(POD)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchPOD As String = ""
        ''' <summary>
        ''' 絞り込み条件(Breaker ID)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchBreakerID As String = ""
        ''' <summary>
        ''' 絞り込み条件(Status)
        ''' </summary>
        ''' <returns></returns>
        Public Property SearchStatus As String = ""

    End Class
End Class