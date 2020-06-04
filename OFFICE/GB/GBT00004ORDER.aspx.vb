Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' TANK ACTIVITY ENTRY１画面クラス
''' </summary>
Public Class GBT00004ORDER
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00004"    '自身のMAPID
    Private Const CONST_DSPROWCOUNT = 34                '指定数＋１が表示対象
    Private Const CONST_SCROLLROWCOUNT = 8              'マウススクロール時の増分
    ''' <summary>
    ''' FIXVALUEデマレッジ計算フィールド取得用
    ''' </summary>
    Private Const CONST_FIXCLAS_DEMUCALCFIELD As String = "DUMURRAGECALCFIELD"
    ''' <summary>
    ''' 登録方式 NONBR
    ''' </summary>
    Private Const CONST_ENTRYTYPE_NONBR As String = "NONBR"
    ''' <summary>
    ''' オーガナイザー費目を更新のトリガーとなるACTY
    ''' </summary>
    Private Const CONST_ACTY_ORGANIZER_COST_DATE As String = "SHIP"
    ''' <summary>
    ''' 4表のレンジ名(3行必須,1行目:PAYABLE 買掛金（ＪＯＴ）,2行目:RECEIVABLE 売掛金（ＪＯＴ）,3行目:NETSETTLEMENTDUE 未払金)
    ''' </summary>
    ''' <remarks>SOA締めで利用</remarks>
    Private Const CONST_AXISCHART_RANGENAME As String = "RNG_SUMMARYUSD"
    ''' <summary>
    ''' フィールド名：買掛金
    ''' </summary>
    ''' <remarks>SOA締めで利用</remarks>
    Private Const CONST_CLOSINDDAY_FILEDNAME1 As String = "PAYABLE"
    ''' <summary>
    ''' フィールド名：売掛金
    ''' </summary>
    ''' <remarks>SOA締めで利用</remarks>
    Private Const CONST_CLOSINDDAY_FILEDNAME2 As String = "RECEIVABLE"
    ''' <summary>
    ''' フィールド名：未払金
    ''' </summary>
    ''' <remarks>SOA締めで利用</remarks>
    Private Const CONST_CLOSINDDAY_FILEDNAME3 As String = "NETSETTLEMENTDUE"
    ''' <summary>
    ''' 変更を検知するPODに属するACTYリスト
    ''' </summary>
    Public Property ProcResult As ProcMessage = Nothing

    Public Property Gbt00006ROrderInfo As GBT00006RESULT.GBT00006ROrderInfo
    ''' <summary>
    ''' 申請時にApplyIdが必要なDataId
    ''' </summary>
    ''' <returns></returns>
    Public Property NeedsApplyTextDataId As New List(Of String)
    ''' <summary>
    ''' 処理返却用のメッセージ
    ''' </summary>
    Public Class ProcMessage
        Public Property MessageNo As String = C_MESSAGENO.NORMAL
        Public Property canNotEntryTankSeq As List(Of Hashtable)
        Public Property modOtherUsers As List(Of DataRow)
        Public Property dateSeqError As New List(Of DataRow)
        Public Property NonBrNo As String = ""
    End Class
    ''' <summary>
    ''' 修正パターン列挙型
    ''' </summary>
    <Flags()>
    Private Enum ModifyType As Integer
        ''' <summary>
        ''' 追加
        ''' </summary>
        ins = 1
        ''' <summary>
        ''' 追加（タンク更新を含んだ費用の追加）
        ''' </summary>
        insTank = 2
        ''' <summary>
        ''' 更新
        ''' </summary>
        upd = 4
        ''' <summary>
        ''' タンク更新(タンク単位で更新のためのこちらの更新はトランザクションする目的)
        ''' </summary>
        updTank = 8
        ''' <summary>
        ''' 論理削除
        ''' </summary>
        del = 16
        ''' <summary>
        ''' 論理削除（タンク更新を含んだ費用の削除）
        ''' </summary>
        delTank = 32
    End Enum

    Private SavedDt As DataTable = Nothing
    ''' <summary>
    ''' ポストバック時のデータテーブル内容
    ''' </summary>
    Private PrevDt As DataTable = Nothing
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

            Response.Cache.SetCacheability(HttpCacheability.NoCache)
            COA0003LogFile = New COA0003LogFile              'ログ出力

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'ユーザーオフィス、通貨保持
                '****************************************
                Dim GBA00003UserSetting As New GBA00003UserSetting With {.USERID = COA0019Session.USERID}
                GBA00003UserSetting.GBA00003GetUserSetting()
                Me.hdnUserOffice.Value = GBA00003UserSetting.OFFICECODE
                Me.hdnUserCountry.Value = GBA00003UserSetting.COUNTRYCODE
                Dim GBA00008Country As New GBA00008Country With {.COUNTRYCODE = GBA00003UserSetting.COUNTRYCODE}
                GBA00008Country.getCountryInfo()
                Dim dtCountry As DataTable = GBA00008Country.COUNTRY_TABLE
                Me.hdnUserCurrency.Value = ""
                Me.lbCurrencyCode.Items.Add(New ListItem(GBC_CUR_USD, GBC_CUR_USD)) 'ユーザーの国も抑えておく
                If dtCountry IsNot Nothing AndAlso dtCountry.Rows.Count > 0 Then
                    Me.hdnUserCurrency.Value = Convert.ToString(dtCountry.Rows(0).Item("CURRENCYCODE"))
                    Me.lbCurrencyCode.Items.Add(New ListItem(Me.hdnUserCurrency.Value, Me.hdnUserCurrency.Value))

                End If
                Me.hdnUsdDecimalPlaces.Value = GetDecimalPlaces()

                '****************************************
                '前画面情報の引継ぎ
                '****************************************
                If GBA00003UserSetting.IS_JOTUSER Then
                    Me.hdnCurrentCloseYm.Value = GetClosingDate(GBC_JOT_SOA_COUNTRY)
                Else
                    Me.hdnCurrentCloseYm.Value = GetClosingDate(GBA00003UserSetting.COUNTRYCODE)
                End If

                SetPrevDisplayValues()
                Me.lblClosingDate.Text = Me.hdnReportMonth.Value
                txtActy_Change()

                '****************************************
                '表示非表示制御
                '****************************************
                DisplayControl()
                '****************************************
                '画面タイトル取得
                '****************************************
                Dim COA0031ProfMap As New COA0031ProfMap
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnListMapVariant.Value
                COA0031ProfMap.COA0031GetDisplayTitle()

                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0031ProfMap.ERR)})
                    Return
                End If

                '****************************************
                '右ボックス帳票IDリストの生成
                '****************************************
                Dim retMessageNo As String = RightboxInit()
                If retMessageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(retMessageNo, Me.lblFooterMessage)
                    Return
                End If

            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                Me.lblFooterMessage.Text = ""
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
                    Dim btnEventName As String = ""
                    Dim param As Object = Nothing
                    If hdnOnchangeField.Value.StartsWith("txtWF_LISTAREASCHEDELDATE") OrElse
                       hdnOnchangeField.Value.StartsWith("txtWF_LISTAREAACTUALDATE") OrElse
                       hdnOnchangeField.Value.StartsWith("txtWF_LISTAREASOAAPPDATE") Then

                        '変更イベント受け渡し用のパラメータ
                        Dim paramVal As New Hashtable
                        paramVal.Add("SENDER", hdnOnchangeField.Value) '対象フィールド名
                        paramVal.Add("ROW", Me.hdnListCurrentRownum.Value) '変更した行
                        param = paramVal
                        '実行関数名の生成
                        btnEventName = "txtListDate_Change"
                        Me.hdnOnchangeField.Value = ""
                        '変更イベントが存在する場合は実行存在しない場合はスキップ
                        Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(btnEventName)
                        If mi IsNot Nothing Then
                            CallByName(Me, btnEventName, CallType.Method, param)
                        End If
                    ElseIf hdnOnchangeField.Value.StartsWith("txtWF_LISTAREACURRENCYCODE") Then
                        '変更イベント受け渡し用のパラメータ
                        Dim paramVal As New Hashtable
                        paramVal.Add("SENDER", hdnOnchangeField.Value) '対象フィールド名
                        paramVal.Add("ROW", Me.hdnListCurrentRownum.Value) '変更した行
                        param = paramVal
                        '実行関数名の生成
                        btnEventName = "txtListCurrency_Change"
                        Me.hdnOnchangeField.Value = ""
                        '変更イベントが存在する場合は実行存在しない場合はスキップ
                        Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(btnEventName)
                        If mi IsNot Nothing Then
                            CallByName(Me, btnEventName, CallType.Method, param)
                        End If
                    ElseIf hdnOnchangeField.Value.StartsWith("txtWF_LISTAREACONTRACTORODR") OrElse
                           hdnOnchangeField.Value.StartsWith("txtWF_LISTAREACONTRACTORFIX") Then
                        '変更イベント受け渡し用のパラメータ
                        Dim paramVal As New Hashtable
                        paramVal.Add("SENDER", hdnOnchangeField.Value) '対象フィールド名
                        paramVal.Add("ROW", Me.hdnListCurrentRownum.Value) '変更した行
                        param = paramVal
                        '実行関数名の生成

                        btnEventName = "txtListContractor_Change"
                        Me.hdnOnchangeField.Value = ""
                        '変更イベントが存在する場合は実行存在しない場合はスキップ
                        Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(btnEventName)
                        If mi IsNot Nothing Then
                            CallByName(Me, btnEventName, CallType.Method, param)
                        End If
                    Else 'このElseケースの前に業者テキストも付く想定
                        'テキストID + "_Change"というイベントを実行する。(この規則性ではない場合、個別の分岐をしてください)
                        btnEventName = Me.hdnOnchangeField.Value & "_Change"
                        Me.hdnOnchangeField.Value = ""
                        '変更イベントが存在する場合は実行存在しない場合はスキップ
                        Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(btnEventName)
                        If mi IsNot Nothing Then
                            CallByName(Me, btnEventName, CallType.Method)
                        End If
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
                ' オフィスコードダブルクリック
                '**********************
                If Me.hdnTextDbClickField IsNot Nothing AndAlso Me.hdnTextDbClickField.Value = "DTLOFFICE" Then
                    Me.hdnTextDbClickField.Value = ""
                    Dim swapRowNum As String = Me.hdnListCurrentRownum.Value
                    Me.hdnListCurrentRownum.Value = ""
                    '発⇔着を交互に入れ替える
                    Dim retMessageNo = SwapOffice(swapRowNum)
                    If retMessageNo <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(retMessageNo, Me.lblFooterMessage)
                        Return
                    End If
                End If
                '**********************
                ' デマレージ 10%の項目計算
                '**********************
                If Me.hdnTextDbClickField IsNot Nothing AndAlso Me.hdnTextDbClickField.Value = "AMOUNTFIX" Then
                    Me.hdnTextDbClickField.Value = ""
                    Dim calcRowNum As String = Me.hdnListCurrentRownum.Value
                    Me.hdnListCurrentRownum.Value = ""
                    '10%計算を行う
                    Dim retMessageNo = CalcDumCommAmount(calcRowNum)
                    If retMessageNo <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(retMessageNo, Me.lblFooterMessage)
                        Return
                    End If
                End If
                '**********************
                ' 一覧タンク部分ダブルクリックイベント判定
                '**********************
                If Me.hdnTankProc.Value <> "" Then
                    If Me.hdnTankProc.Value = "OPEN" Then
                        Dim retMessageNo As String = OpenTankList()
                        If retMessageNo <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(retMessageNo, Me.lblFooterMessage)
                        End If
                    ElseIf Me.hdnTankProc.Value = "DELETE" Then
                        If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                            UpdateDatatableTankNo(Me.hdnSelectedOrderId.Value, Me.hdnSelectedTankSeq.Value, "", False, dataid:=Me.hdnSelectedDataId.Value)
                        Else
                            UpdateDatatableTankNo(Me.hdnSelectedOrderId.Value, Me.hdnSelectedTankSeq.Value, "", False)
                        End If

                        Me.hdnSelectedOrderId.Value = ""
                        Me.hdnSelectedTankSeq.Value = ""

                    End If

                End If
                '**********************
                ' 申請理由入力ボックス表示
                '**********************
                If Me.hdnRemarkboxField.Value <> "" Then
                    If Me.hdnRemarkboxField.Value.Contains("txtWF_LISTAREAAPPLYTEXT") Then
                        DisplayApplyReason(True)
                    ElseIf Me.hdnRemarkboxField.Value.Contains("txtWF_LISTAREAREMARK") Then
                        DisplayRemark(True)
                    End If

                    Me.divRemarkInputBoxWrapper.Style("display") = "block"

                End If
                Me.hdnTankProc.Value = ""
                '**********************
                ' ファイルアップロード処理
                '**********************
                If Me.hdnListUpload.Value IsNot Nothing AndAlso Me.hdnListUpload.Value <> "" Then
                    If Me.hdnListUpload.Value = "XLS_LOADED" Then
                        UploadExcel()
                    End If

                    Me.hdnListUpload.Value = ""
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
            Me.WF_LISTAREA.CssClass = Me.hdnListMapVariant.Value

            DisplayListObjEdit() '共通関数により描画された一覧の制御

            '読み取り専用(検索条件精算済)
            If Me.hdnSettleType.Value = "02SETTLED" OrElse
                Me.Form.Attributes("data-disabled") = "1" Then
                'Me.WF_LISTAREA.Attributes("readonly") = "readonly"
                'Me.WF_LISTAREA.CssClass = Me.WF_LISTAREA.CssClass & " " & "disableAll"
                Me.WF_LISTAREA.Enabled = False
            End If
            'USD AMOUNT Summary
            lblUsdAmountSummary.Text = GetUsdAmountSummary()
        Catch ex As Threading.ThreadAbortException

            '何もしない
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

            Return
        End Try
    End Sub
    ''' <summary>
    ''' 左ビュー表示処理
    ''' </summary>
    Private Sub DisplayLeftView()
        Dim targetObject As Control = Nothing
        Me.btnLeftBoxButtonSel.Disabled = False
        'ビューの存在チェック
        Dim changeViewObj As View = DirectCast(Me.mvLeft.FindControl(Me.hdnLeftboxActiveViewId.Value), View)
        If changeViewObj IsNot Nothing Then
            Me.mvLeft.SetActiveView(changeViewObj)
            Select Case changeViewObj.ID
                '他のビューが存在する場合はViewIdでCaseを追加
                Case vLeftCost.ID
                    Dim brType As String = ""
                    If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                        brType = C_BRTYPE.NONBR
                    Else
                        brType = GetBrType(Me.hdnOrderNo.Value)
                    End If
                    Dim dt As DataTable = GetCostItem(brType) 'TODO一旦セールスのみなので用オペブレ時対応
                    Dim costCodeLists = dt.AsEnumerable.GroupBy(Function(p) p("CODE")) _
                                        .Select(Function(group) group.First())
                    If costCodeLists.Any = True Then
                        dt = costCodeLists.CopyToDataTable
                    End If
                    With Me.lbCost
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            .SelectedIndex = -1
                            Dim findListItem = .Items.FindByValue(Me.txtCostItem.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
                Case vLeftAddCost.ID
                    Dim brType As String = GetBrType(Me.hdnOrderNo.Value)
                    Dim polPod As String = ""
                    If Me.rblPolPod.SelectedItem IsNot Nothing Then
                        If Me.rblPolPod.SelectedItem.Value.StartsWith("POD") Then
                            polPod = "D"
                        Else
                            polPod = "L"
                        End If

                    End If
                    '追加可能な発着情報が無い場合は選択（OKボタン使用不可）
                    If Me.rblPolPod.Items Is Nothing OrElse Me.rblPolPod.Items.Count = 0 Then
                        Me.btnLeftBoxButtonSel.Disabled = True
                    End If
                    Dim dt As DataTable = GetCostItem(brType, polPod:=polPod)
                    With Me.lbAddCost
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With
                Case vLeftAddNbCost.ID
                    Dim dt As DataTable = GetCostItem(C_BRTYPE.NONBR) 'TODO一旦セールスのみなので用オペブレ時対応
                    With Me.lbAddNbCost
                        .DataSource = dt
                        .DataTextField = "LISTBOXNAME"
                        .DataValueField = "CODE"
                        .DataBind()
                        .Focus()
                    End With
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
                            .SelectedIndex = -1
                            Dim findListItem = .Items.FindByValue(Me.txtActy.Text)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
                Case Me.vLeftVender.ID


                    '絞り込みベンダー選択時
                    If Me.lbVender.Items.Count > 0 Then
                        Me.lbVender.SelectedIndex = -1
                        Dim findListItem = Me.lbVender.Items.FindByValue(Me.txtVender.Text)
                        If findListItem IsNot Nothing Then
                            findListItem.Selected = True
                        End If
                    End If
                    Me.lbVender.Focus()
                Case Me.vLeftCurrencyCode.ID
                    '通貨コード選択表示切替
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where item("LINECNT").Equals(Integer.Parse(lineCnt))).FirstOrDefault
                    Dim currentCur As String = ""
                    Dim country As String = ""
                    If selectedRow IsNot Nothing Then
                        currentCur = Convert.ToString(selectedRow.Item("CURRENCYCODE"))
                    End If
                    'If Me.hdnListMapVariant.Value <> "GB_NonBreaker" Then
                    If Me.hdnListMapVariant.Value <> "GB_NonBreaker" _
                        OrElse (Me.hdnListMapVariant.Value = "GB_NonBreaker" AndAlso GBA00003UserSetting.IS_JOTUSER AndAlso Me.hdnOffice.Value <> "") Then
                        'ノンブレーカー以外は発着の国に合わせるリストを都度作成
                        Dim dtCur As DataTable = Me.GetCurrency(selectedRow)
                        If dtCur IsNot Nothing Then
                            With Me.lbCurrencyCode
                                .Items.Clear()
                                .DataSource = dtCur
                                .DataTextField = "LISTBOXNAME"
                                .DataValueField = "CODE"
                                .DataBind()
                                .Items.Insert(0, New ListItem(GBC_CUR_USD, GBC_CUR_USD))
                            End With
                        End If

                    End If
                    With Me.lbCurrencyCode
                        .Focus()
                        .SelectedIndex = -1
                        '一応現在入力しているテキストと一致するものを選択状態
                        If .Items.Count > 0 Then
                            Dim findListItem = .Items.FindByValue(currentCur)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If
                    End With
                Case Me.vLeftContractor.ID
                    '業者選択表示切替
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where item("LINECNT").Equals(Integer.Parse(lineCnt))).FirstOrDefault
                    Dim country As String = ""
                    Dim chargeClass4 As String = ""
                    Dim currentContractor As String = ""
                    Dim brContractor As String = ""
                    Dim orgFlg As Boolean = False
                    Dim listCode As String = "CODE"

                    If selectedRow IsNot Nothing Then
                        If Not Left(Convert.ToString(selectedRow.Item("CHARGE_CLASS4")), Len(GBC_CHARGECLASS4.PORT_I)).Equals(GBC_CHARGECLASS4.PORT_I) Then
                            country = Convert.ToString(selectedRow.Item("COUNTRYCODE"))
                        Else
                            listCode = "PORTCODE"
                        End If
                        chargeClass4 = Convert.ToString(selectedRow.Item("CHARGE_CLASS4"))
                        If Me.hdnListMapVariant.Value <> "GB_Demurrage" Then
                            brContractor = Convert.ToString(selectedRow.Item("CONTRACTORBR"))
                        End If
                        If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREACONTRACTORODR") Then
                            currentContractor = Convert.ToString(selectedRow.Item("CONTRACTORODR"))
                        Else
                            currentContractor = Convert.ToString(selectedRow.Item("CONTRACTORFIX"))
                        End If
                        If Convert.ToString(selectedRow.Item("DTLPOLPOD")) = "Organizer" Then
                            orgFlg = True
                        End If

                        If orgFlg = False Then
                            Dim costCode As String = Convert.ToString(selectedRow.Item("COSTCODE"))
                            Dim dtCst = GetCostItem("", costCode:=costCode)

                            If dtCst IsNot Nothing AndAlso dtCst.Rows.Count > 0 Then
                                Dim drCst = dtCst.Rows(0)
                                If Convert.ToString(drCst("CLASS2")) <> "" Then
                                    orgFlg = True
                                End If
                            End If
                        End If
                    End If

                    'ノンブレーカー以外は発着の国に合わせるリストを都度作成
                    Dim dtCont As DataTable

                    If orgFlg AndAlso Me.hdnListMapVariant.Value <> "GB_NonBreaker" Then
                        dtCont = Me.GetCustomer(brContractor)
                    ElseIf orgFlg AndAlso Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                        dtCont = Me.GetCustomer(countryCode:=country)
                    Else
                        dtCont = Me.GetContractor(country, chargeClass4)
                    End If

                    If dtCont IsNot Nothing Then
                        With Me.lbContractor
                            .Items.Clear()
                            .DataSource = dtCont
                            .DataTextField = "LISTBOXNAME"
                            '.DataValueField = "CODE"
                            .DataValueField = listCode
                            .DataBind()
                            .Focus()
                            '一応現在入力しているテキストと一致するものを選択状態
                            If .Items.Count > 0 Then
                                .SelectedIndex = -1
                                Dim findListItem = .Items.FindByValue(currentContractor)
                                If findListItem IsNot Nothing Then
                                    findListItem.Selected = True
                                End If
                            End If

                        End With
                    End If
                Case Me.vLeftReportMonth.ID
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where item("LINECNT").Equals(Integer.Parse(lineCnt))).FirstOrDefault
                    Dim currentVal As String = ""
                    Dim currentBillingMonth As String = ""

                    Me.lbReportMonth.Items.Clear()

                    If selectedRow IsNot Nothing Then
                        currentVal = Convert.ToString(selectedRow.Item("DEMREPORTMONTH"))
                        currentBillingMonth = Convert.ToString(selectedRow.Item("CLOSINGMONTH"))
                        If currentBillingMonth <> "" Then
                            Dim currentBillingMonthDtm As DateTime = CDate(currentBillingMonth & "/01")
                            Dim monthString As String = currentBillingMonthDtm.ToString("yyyy/MM")
                            Me.lbReportMonth.Items.Add(New ListItem(monthString, monthString))
                            monthString = currentBillingMonthDtm.AddMonths(1).ToString("yyyy/MM")
                            Me.lbReportMonth.Items.Add(New ListItem(monthString, monthString))
                        End If


                        '一応現在入力しているテキストと一致するものを選択状態
                        If Me.lbReportMonth.Items.Count > 0 Then
                            Me.lbReportMonth.SelectedIndex = -1
                            Dim findListItem = Me.lbReportMonth.Items.FindByValue(currentVal)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If

                        Me.mvLeft.Focus()
                    End If
                Case Me.vLeftAccCurrencySegment.ID
                    '汎用補助区分ビュー(ノンブレのみの使用想定)
                    Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                    Dim selectedRow As DataRow = (From item In Me.SavedDt Where item("LINECNT").Equals(Integer.Parse(lineCnt))).FirstOrDefault
                    Dim currentVal As String = ""


                    Me.lbAccCurrencySegment.Items.Clear()
                    If selectedRow IsNot Nothing Then
                        currentVal = Convert.ToString(selectedRow.Item("ACCCURRENCYSEGMENT"))
                        Dim dt As DataTable = GetAccCurrencySegment()
                        Me.lbAccCurrencySegment.DataSource = dt
                        Me.lbAccCurrencySegment.DataTextField = "LISTBOXNAME"
                        Me.lbAccCurrencySegment.DataValueField = "CODE"
                        Me.lbAccCurrencySegment.DataBind()
                        '一応現在入力しているテキストと一致するものを選択状態
                        If Me.lbAccCurrencySegment.Items.Count > 0 Then
                            lbAccCurrencySegment.SelectedIndex = -1
                            Dim findListItem = Me.lbAccCurrencySegment.Items.FindByValue(currentVal)
                            If findListItem IsNot Nothing Then
                                findListItem.Selected = True
                            End If
                        End If

                        Me.mvLeft.Focus()
                    End If
                Case Me.vLeftCal.ID
                    Me.hdnBillingYmd.Value = ""
                    'カレンダビュー表示切替
                    If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREASCHEDELDATE") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREAACTUALDATE") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREASOAAPPDATE") Then
                        Dim rowitem = GetDatatableDate(Me.hdnTextDbClickField.Value, Me.hdnListCurrentRownum.Value)
                        Dim selectedDate As String = ""
                        Dim billingYmd As String = ""
                        If rowitem.Value IsNot Nothing Then
                            selectedDate = Convert.ToString(rowitem.Value(rowitem.Key))
                            billingYmd = Convert.ToString(rowitem.Value("BILLINGYMD"))
                            Try
                                billingYmd = Date.Parse(billingYmd).AddDays(-1).ToString("yyyy/MM/dd")
                            Catch ex As Exception
                            End Try
                        End If
                        Dim tmpDate As Date
                        If Date.TryParse(selectedDate, tmpDate) = False Then
                            selectedDate = ""
                        End If
                        Me.hdnCalendarValue.Value = selectedDate
                        If billingYmd <> "" Then
                            'Me.hdnBillingYmd.Value = billingYmd
                        End If
                        Me.mvLeft.Focus()
                    Else
                        targetObject = FindControl(Me.hdnTextDbClickField.Value)
                        If targetObject IsNot Nothing Then

                            Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                            Me.hdnCalendarValue.Value = txtobj.Text

                            Me.mvLeft.Focus()
                        End If
                    End If

            End Select
        End If

    End Sub
    ''' <summary>
    ''' 申請理由表示処理
    ''' </summary>
    ''' <param name="isOpen"></param>
    Private Sub DisplayApplyReason(isOpen As Boolean)
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        '一覧表示データ復元 
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If

        Dim uniqueIndex As String = Me.hdnCurrentUnieuqIndex.Value
        Dim targetRow = (From dr In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = uniqueIndex).FirstOrDefault

        If targetRow IsNot Nothing Then
            If isOpen = True Then
                Me.txtRemarkInput.Text = Convert.ToString(targetRow.Item("APPLYTEXT"))
                If Convert.ToString(targetRow.Item("STATUSCODE")).Trim <> C_APP_STATUS.APPLYING Then
                    btnRemarkInputOk.Visible = True
                Else
                    btnRemarkInputOk.Visible = False
                End If
                If Convert.ToString(targetRow.Item("CANROWEDIT")) = "0" Then
                    btnRemarkInputOk.Disabled = True
                Else
                    btnRemarkInputOk.Disabled = False
                End If
                Me.txtRemarkInput.Focus()
            Else
                targetRow.Item("APPLYTEXT") = Me.txtRemarkInput.Text
                '一覧表データの保存
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    Throw New Exception("Update Apply Text Failed")
                End If
                'Me.WF_LISTAREA.Focus() '強制スクロールされるので一旦コメント
            End If
        End If

    End Sub
    ''' <summary>
    ''' 備考表示処理
    ''' </summary>
    ''' <param name="isOpen"></param>
    Private Sub DisplayRemark(isOpen As Boolean)
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        '一覧表示データ復元 
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If

        Dim uniqueIndex As String = Me.hdnCurrentUnieuqIndex.Value
        Dim targetRow = (From dr In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = uniqueIndex).FirstOrDefault

        If targetRow IsNot Nothing Then
            If isOpen = True Then
                Me.txtRemarkInput.Text = Convert.ToString(targetRow.Item("REMARK"))
                If targetRow.Item("CANROWEDIT").Equals("1") Then
                    Me.btnRemarkInputOk.Disabled = False
                Else
                    Me.btnRemarkInputOk.Disabled = True
                End If
                Me.txtRemarkInput.Focus()
            Else
                targetRow.Item("REMARK") = Me.txtRemarkInput.Text
                '一覧表データの保存
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    Throw New Exception("Update Remark Failed")
                End If
            End If
        End If

    End Sub
    ''' <summary>
    ''' 戻るボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        '未保存の費用がある場合は申請不可
        'キー発着・フィールド
        Dim dicDemurrageCalcField As New Dictionary(Of String, List(Of String))
        With Nothing

            Dim COA0017FixValue As New COA0017FixValue
            COA0017FixValue.COMPCODE = GBC_COMPCODE_D
            COA0017FixValue.CLAS = CONST_FIXCLAS_DEMUCALCFIELD

            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                dicDemurrageCalcField = COA0017FixValue.VALUEDIC
            Else
                Throw New Exception("Fix value getError")
            End If

        End With
        Dim notSavedData = GetModifiedDataTable(dicDemurrageCalcField, False, True)
        If Not (notSavedData Is Nothing OrElse notSavedData.Count = 0) Then
            CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMCLOSE, pageObject:=Me, submitButtonId:="btnBackOk")
            Return
        End If
        '確認メッセージを表示しない場合は終了
        btnBackOk_Click()

    End Sub
    ''' <summary>
    ''' 戻る確定時処理(btnBack_Click時に更新データが無い場合も通る)
    ''' </summary>
    Public Sub btnBackOk_Click()
        Dim url As String = ""
        If Me.hdnListMapVariant.Value = "Default" Then
            Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
            '■■■ 画面遷移先URL取得 ■■■
            COA0012DoUrl.MAPIDP = "GBT00003S"
            HttpContext.Current.Session("MAPvariant") = "GB_Default"
            COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
            COA0012DoUrl.COA0012GetDoUrl()
            If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
            Else
                CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage)
                Return
            End If
            url = COA0012DoUrl.URL
        Else
            Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

            '画面戻先URL取得
            COA0011ReturnUrl.MAPID = CONST_MAPID
            COA0011ReturnUrl.VARI = hdnListMapVariant.Value
            COA0011ReturnUrl.COA0011GetReturnUrl()
            If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
                lblTitleText.Text = COA0011ReturnUrl.NAMES
            Else
                CommonFunctions.ShowMessage(COA0011ReturnUrl.ERR, Me.lblFooterMessage)
                Return
            End If

            '次画面の変数セット
            HttpContext.Current.Session("MAPvariant") = COA0011ReturnUrl.VARI_Return
            HttpContext.Current.Session("MAPurl") = COA0011ReturnUrl.URL

            url = COA0011ReturnUrl.URL
        End If

        '画面遷移実行
        Server.Transfer(url)
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
                    If Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREASCHEDELDATE") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREAACTUALDATE") OrElse
                       Me.hdnTextDbClickField.Value.StartsWith("txtWF_LISTAREASOAAPPDATE") Then
                        Dim val As String = ""
                        val = Me.hdnCalendarValue.Value
                        Dim tmpDate As Date
                        If val <> "" AndAlso Date.TryParseExact(val, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, tmpDate) = False Then
                            val = Me.hdnCalendarValue.Value
                        ElseIf val <> "" Then
                            val = tmpDate.ToString("yyyy/MM/dd") '一旦yyyy/MM/dd形式に変更（TODO：国ごとの日付フォーマット)
                        End If

                        'Me.hdnActiveElementAfterOnChange.Value = Me.hdnTextDbClickField.Value
                        Dim messageNo As String = UpdateDatatableDate(val, Me.hdnTextDbClickField.Value, Me.hdnListCurrentRownum.Value)
                        If messageNo <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)
                        End If
                    Else
                        targetObject = FindControl(Me.hdnTextDbClickField.Value)
                        If targetObject IsNot Nothing Then
                            Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                            txtobj.Text = Me.hdnCalendarValue.Value
                            txtobj.Focus()
                        End If
                    End If
                Case vLeftCost.ID
                    Me.lblCostItemText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    '費用選択時
                    If Me.lbCost.SelectedItem IsNot Nothing Then
                        Dim costCode As String = Me.lbCost.SelectedItem.Value
                        Dim brType As String = ""
                        If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                            brType = C_BRTYPE.NONBR
                        End If
                        Dim dt As DataTable = GetCostItem(brType, costCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblCostItemText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
                        End If

                    End If
                Case vLeftAddCost.ID
                    '費用選択時(一覧に費用を追加)
                    If Me.lbAddCost.SelectedItem IsNot Nothing Then
                        Dim costCode As String = Me.lbAddCost.SelectedItem.Value
                        Dim messageNo As String = AddNewCostItem(costCode)
                        If messageNo <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})

                        End If
                    End If
                Case vLeftAddNbCost.ID
                    'ノンブレーカー費用選択時(一覧に費用追加)
                    If Me.lbAddNbCost.SelectedItem IsNot Nothing Then
                        Dim costCode As String = Me.lbAddNbCost.SelectedItem.Value
                        Dim messageNo As String = AddNewNbCostItem(costCode)
                        If messageNo <> C_MESSAGENO.NORMAL Then
                            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})

                        End If
                    End If
                Case vLeftActy.ID
                    'ACTY選択時
                    Me.lblActyText.Text = ""
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim txtObject As TextBox = DirectCast(targetObject, TextBox)
                    If targetObject IsNot Nothing Then
                        txtObject.Text = ""
                    End If

                    If Me.lbActy.SelectedItem IsNot Nothing Then
                        Dim actyCode As String = Me.lbActy.SelectedItem.Value
                        Dim dt As DataTable = GetActy(actyCode)
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            Dim dr As DataRow = dt.Rows(0)
                            If targetObject IsNot Nothing Then
                                txtObject.Text = Convert.ToString(dr.Item("CODE"))
                            End If
                            Me.lblActyText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
                        End If

                    End If
                Case Me.vLeftVender.ID
                    '業者選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim venderLabelObj As Label = Nothing

                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Select Case txtobj.ID
                            Case Me.txtVender.ID
                                venderLabelObj = Me.lblVenderText
                            Case Me.txtBrVender.ID
                                venderLabelObj = Me.lblBrVenderText
                            Case Me.txtEstimatedVender.ID
                                venderLabelObj = Me.lblEstimatedVenderText
                        End Select
                        If Me.lbVender.SelectedItem IsNot Nothing Then
                            txtobj.Text = Me.lbVender.SelectedItem.Value
                            If Me.lbVender.SelectedItem.Text.Contains(":") Then
                                Dim parts As String()
                                parts = Split(Me.lbVender.SelectedItem.Text, ":", -1, CompareMethod.Text)
                                venderLabelObj.Text = parts(1)
                            Else
                                venderLabelObj.Text = Me.lbVender.SelectedItem.Text
                            End If
                            txtobj.Focus()
                        Else
                            txtobj.Text = ""
                            venderLabelObj.Text = ""
                            txtobj.Focus()
                        End If
                    End If

                Case Me.vLeftCurrencyCode.ID
                    '通貨コード選択時
                    If Me.lbCurrencyCode.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        UpdateDatatableCurrency(Me.lbCurrencyCode.SelectedValue, lineCnt)
                    End If

                Case Me.vLeftContractor.ID
                    '業者選択時
                    If Me.lbContractor.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        UpdateDatatableContractor(Me.lbContractor.SelectedValue, targetTextField, lineCnt)
                    End If
                Case Me.vLeftReportMonth.ID
                    '請求月選択時
                    If Me.lbReportMonth.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        UpdateDatatableReportMonth(Me.lbReportMonth.SelectedValue, lineCnt)
                    End If
                Case Me.vLeftAccCurrencySegment.ID
                    '業者選択時
                    If Me.lbAccCurrencySegment.SelectedItem IsNot Nothing Then
                        Dim lineCnt As String = Me.hdnListCurrentRownum.Value
                        Dim targetTextField As String = Me.hdnTextDbClickField.Value
                        UpdateDatatableAccCurrencySegment(Me.lbAccCurrencySegment.SelectedValue, lineCnt)
                    End If
                Case Else
                    '何もしない
            End Select
        End If
        '○ 画面左サイドボックス非表示は、画面JavaScriptで実行
        Me.mvLeft.SetActiveView(Me.vLeftCal)
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
        Me.mvLeft.SetActiveView(Me.vLeftCal)
        Me.hdnTextDbClickField.Value = ""
        Me.hdnIsLeftBoxOpen.Value = ""
    End Sub
    ''' <summary>
    ''' 絞り込みボタン押下時処理
    ''' </summary>
    Public Sub btnExtract_Click(Optional isButtonClick As Boolean = True)
        Dim pageObj As Page = Nothing

        If isButtonClick = True Then
            Me.hdnFilterCostItem.Value = Me.txtCostItem.Text.Trim
            Me.hdnFilterActy.Value = Me.txtActy.Text.Trim
            pageObj = Me
        End If

        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        '一覧表示データ復元 
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=pageObj)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If

        'フィルタでの絞り込みを利用するか確認
        Dim isFillterOff As Boolean = True
        If Me.hdnFilterCostItem.Value <> "" OrElse Me.hdnFilterActy.Value <> "" _
           OrElse Me.hdnListMapVariant.Value = "GB_TankActivity" _
           OrElse Me.hdnListMapVariant.Value = "GB_SOA" _
           OrElse Me.txtVender.Text <> "" _
            OrElse Me.txtVender.Text <> "" _
            OrElse Me.txtEstimatedVender.Text <> "" _
            OrElse Me.txtTankNo.Text <> "" _
            OrElse Me.txtOrderNo.Text <> "" _
            OrElse Me.txtBrVender.Text <> "" Then
            isFillterOff = False
        End If

        For Each dr As DataRow In dt.Rows
            dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
            'フィルタ使用時の場合
            If isFillterOff = False Then
                '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
                If Not ((Me.hdnFilterCostItem.Value = "" OrElse Convert.ToString(dr("COSTCODE")).Equals(Me.hdnFilterCostItem.Value)) _
                   AndAlso (Me.hdnFilterActy.Value = "" OrElse Convert.ToString(dr("ACTIONID")).Equals(Me.hdnFilterActy.Value)) _
                   AndAlso (Me.txtVender.Text = "" OrElse Convert.ToString(dr("CONTRACTORFIX")).Equals(Me.txtVender.Text)) _
                   AndAlso (Me.txtBrVender.Text = "" OrElse Convert.ToString(dr("CONTRACTORBR")).Equals(Me.txtBrVender.Text)) _
                   AndAlso (Me.txtEstimatedVender.Text = "" OrElse Convert.ToString(dr("CONTRACTORODR")).Equals(Me.txtEstimatedVender.Text)) _
                   AndAlso (Me.txtOrderNo.Text = "" OrElse Convert.ToString(dr("ORDERNO")).StartsWith(Me.txtOrderNo.Text)) _
                   AndAlso (Me.txtTankNo.Text = "" OrElse Convert.ToString(dr("TANKNO")).StartsWith(Me.txtTankNo.Text))) _
                   OrElse (Me.hdnListMapVariant.Value = "GB_TankActivity" AndAlso Convert.ToString(dr("ACTIONID")) = "") _
                   OrElse (Me.hdnListMapVariant.Value = "GB_SOA" AndAlso Me.chkHideNoAmount.Checked = True AndAlso (dr("UAG_USD").Equals("0"))) _
                   OrElse (Me.hdnListMapVariant.Value = "GB_SOA" AndAlso Me.ckhShowTotalInvoiceRelatedCost.Checked = False AndAlso (dr("BRADDEDCOST").Equals("2"))) Then
                    dr.Item("HIDDEN") = 1
                End If
            End If
        Next
        'Dim startIdx As Integer = 0
        'Dim endIdx As Integer = dt.Rows.Count - 1
        'Threading.Tasks.Parallel.For(startIdx,
        '                             endIdx,
        '    Sub(idx)
        '        Dim dr As DataRow = dt.Rows(idx)
        '        dr.Item("HIDDEN") = 0 '一旦表示 HIDDENフィールドに0
        '        'フィルタ使用時の場合
        '        If isFillterOff = False Then
        '            '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
        '            If Not ((Me.hdnFilterCostItem.Value = "" OrElse Convert.ToString(dr("COSTCODE")).Equals(Me.hdnFilterCostItem.Value)) _
        '            AndAlso (Me.hdnFilterActy.Value = "" OrElse Convert.ToString(dr("ACTIONID")).StartsWith(Me.hdnFilterActy.Value))) _
        '            OrElse (Me.hdnListMapVariant.Value = "GB_TankActivity" AndAlso Convert.ToString(dr("ACTIONID")) = "") Then
        '                dr.Item("HIDDEN") = 1
        '            End If
        '        End If

        '    End Sub)

        'Dim messagesLock As Object = New Object()
        'Threading.Tasks.Parallel.ForEach(dt.AsEnumerable(),
        '    Sub(dr)
        '        Dim hiddnVal As Integer = 0

        '        'フィルタ使用時の場合
        '        If isFillterOff = False Then
        '            '条件に合致しない場合は非表示 HIDDENフィールドに1を立てる
        '            If Not ((Me.hdnFilterCostItem.Value = "" OrElse Convert.ToString(dr("COSTCODE")).Equals(Me.hdnFilterCostItem.Value)) _
        '               AndAlso (Me.hdnFilterActy.Value = "" OrElse Convert.ToString(dr("ACTIONID")).StartsWith(Me.hdnFilterActy.Value))) _
        '               OrElse (Me.hdnListMapVariant.Value = "GB_TankActivity" AndAlso Convert.ToString(dr("ACTIONID")) = "") Then
        '                'dr.Item("HIDDEN") = 1
        '                hiddnVal = 1
        '            End If
        '        End If
        '        SyncLock messagesLock
        '            dr.Item("HIDDEN") = hiddnVal
        '        End SyncLock

        '    End Sub)

        '画面先頭を表示
        hdnListPosition.Value = "1"

        '一覧表示データ保存
        COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        Me.SavedDt = dt
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=pageObj)
        Else
            'メッセージ表示
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMALEXTRUCT, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=pageObj)
        End If
        'カーソル設定
        If isButtonClick = True Then
            Me.txtCostItem.Focus()
        Else
            Me.btnAddCost.Focus()
            Me.lblFooterMessage.Text = ""
        End If

    End Sub
    ''' <summary>
    ''' Excelダウンロードボタン押下時処理
    ''' </summary>
    Public Sub btnExcelDownload_Click()
        Dim dt As DataTable = Nothing
        Dim COA0021ListTable As New BASEDLL.COA0021ListTable
        'Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
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

        Else
            dt = Me.SavedDt
        End If
        'そもそも初期検索結果がない場合は絞り込まず終了
        If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
            Return
        End If
        Dim outputDt As DataTable


        'If Me.hdnListMapVariant.Value = "GB_TankActivity" Then
        '    outputDt = (From item In dt
        '                Where Convert.ToString(item.Item("ACTIONID")) <> "").CopyToDataTable
        'Else
        '    outputDt = dt
        'End If
        '現在表示しているもののみ
        Dim dispDispRow = (From item In dt Where Convert.ToString(item("HIDDEN")) = "0")
        If dispDispRow.Any = False Then
            Return
        End If
        outputDt = dispDispRow.CopyToDataTable
        '帳票出力
        With Nothing
            Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
            Dim reportId As String = Me.lbRightList.SelectedItem.Value
            Dim reportMapId As String = CONST_MAPID & hdnListMapVariant.Value
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
    ''' 申請ボタン押下時処理
    ''' </summary>
    Public Sub btnApply_Click()
        Dim dt As DataTable = Nothing
        Dim eventCode As String = C_ODREVENT.APPLY & Me.hdnListMapVariant.Value

        'EventCode
        Dim eventDEM = "ODR_ApplyGB_Demurrage"     'デマレージ
        Dim eventNON = "ODR_ApplyGB_NonBreaker"    'ノンブレーカー
        Dim eventCOS = "ODR_ApplyGB_CostUp"        'COSTUP
        Dim eventORD = "ODR_Apply"                 'ORDER

        '退避データ復元
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            Dim COA0021ListTable As COA0021ListTable = New COA0021ListTable
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If
        '入力チェック
        Dim errMessage As String = ""
        Dim fieldList As New List(Of String) From {"APPLYTEXT"}
        Dim keyFields As New List(Of String) From {"LINECNT"}

        '申請チェックボックスがOnのもののみ取得 TODO 費用変動などの条件が必要かも
        Dim applyCheckDt = (From dr In dt Where Convert.ToString(dr.Item("APPLY")).ToUpper = "ON")
        '申請対象がない場合はそのまま終了(TODOメッセージ)
        If applyCheckDt.Any = False OrElse applyCheckDt Is Nothing OrElse applyCheckDt.Count = 0 Then
            Return
        End If
        'チェックしたデータのうち申請テキストの入力があるデータを絞り込み
        Dim targetDt = (From dr In applyCheckDt Where Convert.ToString(dr.Item("APPLYTEXT")).Trim <> "")
        If targetDt.Any = False OrElse targetDt Is Nothing OrElse targetDt.Count = 0 Then
            '全件コメントが無い場合はそのまま処理終了
            CommonFunctions.ShowMessage(C_MESSAGENO.APPLYREASONNOINPUT, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", C_MESSAGENO.APPLYREASONNOINPUT)})

            Return
        End If
        Me.NeedsApplyTextDataId = (From dr In applyCheckDt Where Convert.ToString(dr.Item("APPLYTEXT")).Trim = "" Select Convert.ToString(dr.Item("DATAID"))).ToList

        '未保存の費用がある場合は申請不可
        'キー発着・フィールド
        Dim dicDemurrageCalcField As New Dictionary(Of String, List(Of String))
        With Nothing

            Dim COA0017FixValue As New COA0017FixValue
            COA0017FixValue.COMPCODE = GBC_COMPCODE_D
            COA0017FixValue.CLAS = CONST_FIXCLAS_DEMUCALCFIELD

            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                dicDemurrageCalcField = COA0017FixValue.VALUEDIC
            Else
                Throw New Exception("Fix value getError")
            End If

        End With
        Dim notSavedData = GetModifiedDataTable(dicDemurrageCalcField, False, True, True)
        If Not (notSavedData Is Nothing OrElse notSavedData.Count = 0) Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NOSAVECOSTITEM, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '単項目チェック
        Dim messageNo As String
        messageNo = CheckSingle(CONST_MAPID, targetDt.CopyToDataTable, fieldList, errMessage, keyFields:=keyFields)
        If messageNo <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
            '左ボックスにエラーメッセージ表示
            Me.txtRightErrorMessage.Text = errMessage
            Return
        End If
        '申請ID取得オブジェクトの生成
        Dim GBA00011ApplyID As New GBA00011ApplyID With {
                .COMPCODE = GBC_COMPCODE_D, 'COA0019Session.APSRVCamp,
                .SYSCODE = C_SYSCODE_GB,
                .KEYCODE = COA0019Session.APSRVname,
                .DIVISION = "O",
                .SEQOBJID = C_SQLSEQ.ORDERAPPLY,
                .SEQLEN = 6
                }
        '申請処理共通オブジェクトの生成
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval With {
            .I_COMPCODE = COA0019Session.APSRVCamp,
            .I_MAPID = CONST_MAPID,
            .I_EVENTCODE = eventCode
        }
        'オーダー(明細)の申請項目更新文の作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("      ORDERNO")
        sqlStat.AppendLine("     ,STYMD")
        sqlStat.AppendLine("     ,ENDYMD")
        sqlStat.AppendLine("     ,TANKSEQ")
        sqlStat.AppendLine("     ,DTLPOLPOD")
        sqlStat.AppendLine("     ,DTLOFFICE")
        sqlStat.AppendLine("     ,TANKNO")
        sqlStat.AppendLine("     ,COSTCODE")
        sqlStat.AppendLine("     ,ACTIONID")
        sqlStat.AppendLine("     ,DISPSEQ")
        sqlStat.AppendLine("     ,LASTACT")
        sqlStat.AppendLine("     ,REQUIREDACT")
        sqlStat.AppendLine("     ,ORIGINDESTINATION")
        sqlStat.AppendLine("     ,COUNTRYCODE")
        sqlStat.AppendLine("     ,CURRENCYCODE")
        sqlStat.AppendLine("     ,TAXATION")
        sqlStat.AppendLine("     ,AMOUNTBR")
        sqlStat.AppendLine("     ,AMOUNTORD")
        sqlStat.AppendLine("     ,AMOUNTFIX")
        sqlStat.AppendLine("     ,CONTRACTORBR")
        sqlStat.AppendLine("     ,CONTRACTORODR")
        sqlStat.AppendLine("     ,CONTRACTORFIX")
        sqlStat.AppendLine("     ,SCHEDELDATEBR")
        sqlStat.AppendLine("     ,SCHEDELDATE")
        sqlStat.AppendLine("     ,ACTUALDATE")
        sqlStat.AppendLine("     ,LOCALBR")
        sqlStat.AppendLine("     ,LOCALRATE")
        sqlStat.AppendLine("     ,TAXBR")
        sqlStat.AppendLine("     ,AMOUNTPAY")
        sqlStat.AppendLine("     ,LOCALPAY")
        sqlStat.AppendLine("     ,TAXPAY")
        sqlStat.AppendLine("     ,INVOICEDBY")
        sqlStat.AppendLine("     ,APPLYID")
        sqlStat.AppendLine("     ,APPLYTEXT")
        sqlStat.AppendLine("     ,LASTSTEP")
        sqlStat.AppendLine("     ,SOAAPPDATE")
        sqlStat.AppendLine("     ,REMARK")
        sqlStat.AppendLine("     ,BRID")
        sqlStat.AppendLine("     ,BRCOST")
        sqlStat.AppendLine("     ,DATEFIELD")
        sqlStat.AppendLine("     ,DATEINTERVAL")
        sqlStat.AppendLine("     ,BRADDEDCOST")
        sqlStat.AppendLine("     ,AGENTORGANIZER")
        sqlStat.AppendLine("     ,DELFLG")
        sqlStat.AppendLine("     ,INITYMD")
        sqlStat.AppendLine("     ,INITUSER")
        sqlStat.AppendLine("     ,UPDYMD")
        sqlStat.AppendLine("     ,UPDUSER")
        sqlStat.AppendLine("     ,UPDTERMID")
        sqlStat.AppendLine(" ) SELECT ORDERNO")
        sqlStat.AppendLine("         ,STYMD")
        sqlStat.AppendLine("         ,ENDYMD")
        sqlStat.AppendLine("         ,TANKSEQ")
        sqlStat.AppendLine("         ,DTLPOLPOD")
        sqlStat.AppendLine("         ,DTLOFFICE")
        sqlStat.AppendLine("         ,TANKNO")
        sqlStat.AppendLine("         ,COSTCODE")
        sqlStat.AppendLine("         ,ACTIONID")
        sqlStat.AppendLine("         ,DISPSEQ")
        sqlStat.AppendLine("         ,LASTACT")
        sqlStat.AppendLine("         ,REQUIREDACT")
        sqlStat.AppendLine("         ,ORIGINDESTINATION")
        sqlStat.AppendLine("         ,COUNTRYCODE")
        sqlStat.AppendLine("         ,CURRENCYCODE")
        sqlStat.AppendLine("         ,TAXATION")
        sqlStat.AppendLine("         ,AMOUNTBR")
        sqlStat.AppendLine("         ,AMOUNTORD")
        sqlStat.AppendLine("         ,AMOUNTFIX")
        sqlStat.AppendLine("         ,CONTRACTORBR")
        sqlStat.AppendLine("         ,CONTRACTORODR")
        sqlStat.AppendLine("         ,CONTRACTORFIX")
        sqlStat.AppendLine("         ,SCHEDELDATEBR")
        sqlStat.AppendLine("         ,SCHEDELDATE")
        sqlStat.AppendLine("         ,ACTUALDATE")
        sqlStat.AppendLine("         ,LOCALBR")
        sqlStat.AppendLine("         ,LOCALRATE")
        sqlStat.AppendLine("         ,TAXBR")
        sqlStat.AppendLine("         ,AMOUNTPAY")
        sqlStat.AppendLine("         ,LOCALPAY")
        sqlStat.AppendLine("         ,TAXPAY")
        sqlStat.AppendLine("         ,INVOICEDBY")
        sqlStat.AppendLine("         ,APPLYID")
        sqlStat.AppendLine("         ,APPLYTEXT")
        sqlStat.AppendLine("         ,LASTSTEP")
        sqlStat.AppendLine("         ,SOAAPPDATE")
        sqlStat.AppendLine("         ,REMARK")
        sqlStat.AppendLine("         ,BRID")
        sqlStat.AppendLine("         ,BRCOST")
        sqlStat.AppendLine("         ,DATEFIELD")
        sqlStat.AppendLine("         ,DATEINTERVAL")
        sqlStat.AppendLine("         ,BRADDEDCOST")
        sqlStat.AppendLine("         ,AGENTORGANIZER")
        sqlStat.AppendLine("         ,'" & CONST_FLAG_YES & "'             AS DELFLG")
        sqlStat.AppendLine("         ,INITYMD")
        sqlStat.AppendLine("         ,INITUSER")
        sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

        sqlStat.AppendLine(" UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("    SET APPLYID      = @APPLYID")
        sqlStat.AppendLine("       ,APPLYTEXT    = @APPLYTEXT")
        sqlStat.AppendLine("       ,LASTSTEP     = @LASTSTEP")
        sqlStat.AppendLine("       ,UPDYMD    = @UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER   = @UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID = @UPDTERMID")
        sqlStat.AppendLine(" WHERE DATAID = @DATAID;")
#Region "DATAID保持対応前"
        'sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        'sqlStat.AppendLine("      ORDERNO")
        'sqlStat.AppendLine("     ,STYMD")
        'sqlStat.AppendLine("     ,ENDYMD")
        'sqlStat.AppendLine("     ,TANKSEQ")
        'sqlStat.AppendLine("     ,DTLPOLPOD")
        'sqlStat.AppendLine("     ,DTLOFFICE")
        'sqlStat.AppendLine("     ,TANKNO")
        'sqlStat.AppendLine("     ,COSTCODE")
        'sqlStat.AppendLine("     ,ACTIONID")
        'sqlStat.AppendLine("     ,DISPSEQ")
        'sqlStat.AppendLine("     ,LASTACT")
        'sqlStat.AppendLine("     ,REQUIREDACT")
        'sqlStat.AppendLine("     ,ORIGINDESTINATION")
        'sqlStat.AppendLine("     ,COUNTRYCODE")
        'sqlStat.AppendLine("     ,CURRENCYCODE")
        'sqlStat.AppendLine("     ,TAXATION")
        'sqlStat.AppendLine("     ,AMOUNTBR")
        'sqlStat.AppendLine("     ,AMOUNTORD")
        'sqlStat.AppendLine("     ,AMOUNTFIX")
        'sqlStat.AppendLine("     ,CONTRACTORBR")
        'sqlStat.AppendLine("     ,CONTRACTORODR")
        'sqlStat.AppendLine("     ,CONTRACTORFIX")
        'sqlStat.AppendLine("     ,SCHEDELDATEBR")
        'sqlStat.AppendLine("     ,SCHEDELDATE")
        'sqlStat.AppendLine("     ,ACTUALDATE")
        'sqlStat.AppendLine("     ,LOCALBR")
        'sqlStat.AppendLine("     ,LOCALRATE")
        'sqlStat.AppendLine("     ,TAXBR")
        'sqlStat.AppendLine("     ,AMOUNTPAY")
        'sqlStat.AppendLine("     ,LOCALPAY")
        'sqlStat.AppendLine("     ,TAXPAY")
        'sqlStat.AppendLine("     ,INVOICEDBY")
        'sqlStat.AppendLine("     ,APPLYID")
        'sqlStat.AppendLine("     ,APPLYTEXT")
        'sqlStat.AppendLine("     ,LASTSTEP")
        'sqlStat.AppendLine("     ,SOAAPPDATE")
        'sqlStat.AppendLine("     ,REMARK")
        'sqlStat.AppendLine("     ,BRID")
        'sqlStat.AppendLine("     ,BRCOST")
        'sqlStat.AppendLine("     ,DATEFIELD")
        'sqlStat.AppendLine("     ,DATEINTERVAL")
        'sqlStat.AppendLine("     ,BRADDEDCOST")
        'sqlStat.AppendLine("     ,AGENTORGANIZER")
        'sqlStat.AppendLine("     ,DELFLG")
        'sqlStat.AppendLine("     ,INITYMD")
        'sqlStat.AppendLine("     ,UPDYMD")
        'sqlStat.AppendLine("     ,UPDUSER")
        'sqlStat.AppendLine("     ,UPDTERMID")
        'sqlStat.AppendLine(" ) SELECT ORDERNO")
        'sqlStat.AppendLine("         ,STYMD")
        'sqlStat.AppendLine("         ,ENDYMD")
        'sqlStat.AppendLine("         ,TANKSEQ")
        'sqlStat.AppendLine("         ,DTLPOLPOD")
        'sqlStat.AppendLine("         ,DTLOFFICE")
        'sqlStat.AppendLine("         ,TANKNO")
        'sqlStat.AppendLine("         ,COSTCODE")
        'sqlStat.AppendLine("         ,ACTIONID")
        'sqlStat.AppendLine("         ,DISPSEQ")
        'sqlStat.AppendLine("         ,LASTACT")
        'sqlStat.AppendLine("         ,REQUIREDACT")
        'sqlStat.AppendLine("         ,ORIGINDESTINATION")
        'sqlStat.AppendLine("         ,COUNTRYCODE")
        'sqlStat.AppendLine("         ,CURRENCYCODE")
        'sqlStat.AppendLine("         ,TAXATION")
        'sqlStat.AppendLine("         ,AMOUNTBR")
        'sqlStat.AppendLine("         ,AMOUNTORD")
        'sqlStat.AppendLine("         ,AMOUNTFIX")
        'sqlStat.AppendLine("         ,CONTRACTORBR")
        'sqlStat.AppendLine("         ,CONTRACTORODR")
        'sqlStat.AppendLine("         ,CONTRACTORFIX")
        'sqlStat.AppendLine("         ,SCHEDELDATEBR")
        'sqlStat.AppendLine("         ,SCHEDELDATE")
        'sqlStat.AppendLine("         ,ACTUALDATE")
        'sqlStat.AppendLine("         ,LOCALBR")
        'sqlStat.AppendLine("         ,LOCALRATE")
        'sqlStat.AppendLine("         ,TAXBR")
        'sqlStat.AppendLine("         ,AMOUNTPAY")
        'sqlStat.AppendLine("         ,LOCALPAY")
        'sqlStat.AppendLine("         ,TAXPAY")
        'sqlStat.AppendLine("         ,INVOICEDBY")
        'sqlStat.AppendLine("         ,@APPLYID       AS APPLYID")
        'sqlStat.AppendLine("         ,@APPLYTEXT     AS APPLYTEXT")
        'sqlStat.AppendLine("         ,@LASTSTEP      AS LASTSTEP")
        'sqlStat.AppendLine("         ,SOAAPPDATE")
        'sqlStat.AppendLine("         ,REMARK")
        'sqlStat.AppendLine("         ,BRID")
        'sqlStat.AppendLine("         ,BRCOST")
        'sqlStat.AppendLine("         ,DATEFIELD")
        'sqlStat.AppendLine("         ,DATEINTERVAL")
        'sqlStat.AppendLine("         ,BRADDEDCOST")
        'sqlStat.AppendLine("         ,AGENTORGANIZER")
        'sqlStat.AppendLine("         ,'" & CONST_FLAG_NO & "'             AS DELFLG")
        'sqlStat.AppendLine("         ,@UPDYMD         AS INITYMD")
        'sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        'sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        'sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        'sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        'sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

        'sqlStat.AppendLine(" UPDATE GBT0005_ODR_VALUE")
        'sqlStat.AppendLine("    SET DELFLG = '" & CONST_FLAG_YES & "'")
        'sqlStat.AppendLine("       ,UPDYMD    = @UPDYMD")
        'sqlStat.AppendLine("       ,UPDUSER   = @UPDUSER")
        'sqlStat.AppendLine("       ,UPDTERMID = @UPDTERMID")
        'sqlStat.AppendLine(" WHERE DATAID = @DATAID;")
#End Region

        'sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        'sqlStat.AppendLine("   SET APPLYID   = @APPLYID")
        'sqlStat.AppendLine("      ,APPLYTEXT = @APPLYTEXT")
        'sqlStat.AppendLine("      ,LASTSTEP  = @LASTSTEP")
        'sqlStat.AppendLine("      ,UPDYMD    = @UPDYMD")
        'sqlStat.AppendLine("      ,UPDUSER   = @UPDUSER")
        'sqlStat.AppendLine(" WHERE DATAID = @DATAID;")
        Dim procDate As Date = Date.Now

        '申請対象レコードのループ
        Dim applyId As String = ""
        Dim subCode As String = ""
        Dim lastStep As String = ""
        Dim skipApplyData As New List(Of DataRow) '他者更新により読み飛ばしたデータ
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            For Each dr In targetDt
                '他者更新チェック
                If Me.CheckUpdateOtherUsers(dr, sqlCon) = False Then
                    skipApplyData.Add(dr) 'スキップしたデータを後続のメッセージ表示のため退避
                    Continue For '他社に更新されていたらスキップする
                End If

                '申請IDの取得
                GBA00011ApplyID.GBA00011getApplyID()
                If GBA00011ApplyID.ERR = C_MESSAGENO.NORMAL Then
                    applyId = GBA00011ApplyID.APPLYID
                Else
                    CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", GBA00011ApplyID.ERR)})
                    Return
                End If

                If applyId = "" Then
                    Throw New Exception("GBA00011getApplyIDの取得したAPPLYIDが空白です。パラメータ・シーケンスの存在有無を確認ください。" &
                                         ControlChars.CrLf &
                                         String.Format("COMPCODE={0}" & ControlChars.CrLf &
                                                       "SYSCODE ={1}" & ControlChars.CrLf &
                                                       "KEYCODE ={2}" & ControlChars.CrLf &
                                                       "DIVISION={3}" & ControlChars.CrLf &
                                                       "SEQOBJID={4}" & ControlChars.CrLf &
                                                       "SEQLEN  ={5}",
                                                       GBA00011ApplyID.COMPCODE, GBA00011ApplyID.SYSCODE,
                                                       GBA00011ApplyID.KEYCODE, GBA00011ApplyID.DIVISION,
                                                       GBA00011ApplyID.SEQOBJID, GBA00011ApplyID.SEQLEN))
                End If

                '1費用項目(1レコード)単位の更新だがオーダー(明細)と申請テーブルを更新するためトランザクションを張る
                Using tran = sqlCon.BeginTransaction,
                      sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, tran)
                    '申請登録
                    subCode = "" 'Convert.ToString(dr.Item("AGENTORGANIZER"))
                    COA0032Apploval.I_APPLYID = applyId
                    COA0032Apploval.I_SUBCODE = subCode
                    COA0032Apploval.COA0032setApply()

                    If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
                        lastStep = COA0032Apploval.O_LASTSTEP
                    Else
                        tran.Rollback() '申請登録に失敗した場合はオーダー(明細)の更新をロールバック
                        CommonFunctions.ShowMessage(COA0032Apploval.O_ERR, Me.lblFooterMessage, pageObject:=Me)
                        Return
                    End If
                    'オーダー(明細)申請情報更新処理実行
                    With sqlCmd.Parameters
                        .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                        .Add("@APPLYTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                        .Add("@LASTSTEP", SqlDbType.NVarChar).Value = lastStep
                        .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                        .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                        .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                        .Add("@DATAID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DATAID"))
                    End With
                    sqlCmd.ExecuteNonQuery()

                    tran.Commit()
                End Using

                'SOAじゃない場合、メール送信
                If eventCode = eventDEM OrElse
                    eventCode = eventNON OrElse
                    eventCode = eventCOS Then

                    Dim mailEventCode As String = eventCode
                    If eventCode = eventCOS Then
                        mailEventCode = eventORD
                    End If

                    'DATAID取得
                    Dim dataId As String = ""
                    dataId = GetDATAID(applyId)

                    'メール
                    Dim GBA00009MailSendSet As New GBA00009MailSendSet
                    GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
                    GBA00009MailSendSet.EVENTCODE = mailEventCode
                    GBA00009MailSendSet.MAILSUBCODE = ""
                    GBA00009MailSendSet.ODRDATAID = dataId
                    GBA00009MailSendSet.APPLYID = applyId
                    GBA00009MailSendSet.APPLYSTEP = C_APP_FIRSTSTEP

                    If eventCode = eventNON Then
                        GBA00009MailSendSet.GBA00009setMailToNonBR()
                    Else
                        GBA00009MailSendSet.GBA00009setMailToOdr()
                    End If
                    If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(GBA00009MailSendSet.ERR, Me.lblFooterMessage, pageObject:=Me)
                        Return
                    End If
                End If
            Next 'End For Each dr In targetDt
        End Using
        'TODO 処理記載
        If Me.NeedsApplyTextDataId IsNot Nothing AndAlso Me.NeedsApplyTextDataId.Count > 0 Then
            Me.ProcResult = New ProcMessage
            Me.ProcResult.MessageNo = C_MESSAGENO.SKIPAPPLYITEM
        End If

        If skipApplyData.Count = 0 Then
            'TODO 問題なく成功パターン
            Me.hdnRefreshMessageNo.Value = C_MESSAGENO.APPLYSUCCESS
            Server.Transfer(Request.Url.LocalPath) '自身を再ロード
        Else
            '問題ありパターン
        End If
    End Sub
    ''' <summary>
    ''' 保存ボタン押下時処理
    ''' </summary>
    Public Sub btnSave_Click()
        Dim dt As DataTable = Nothing
        Dim messageNo As String
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            Dim COA0021ListTable As COA0021ListTable = New COA0021ListTable
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If
        'データテーブルの禁則文字置換
        ChangeInvalidChar(dt, New List(Of String) From {"AMOUNTORD", "AMOUNTFIX", "CONTRACTORODR", "CONTRACTORFIX", "SCHEDELDATE", "ACTUALDATE", "INVOICEDBY"})
        '入力チェック
        Dim errMessage As String = ""
        Dim fieldList As New List(Of String)
        Select Case Me.hdnListMapVariant.Value
            Case "GB_SOA"
                fieldList.AddRange({"APPLYTEXT", "AMOUNTFIX", "ACTUALDATE"})
            Case "GB_NonBreaker"
                fieldList.AddRange({"APPLYTEXT", "TANKNO", "CURRENCYCODE", "AMOUNTORD", "CONTRACTORFIX", "ACTUALDATE", "INVOICEDBY"})
            Case "GB_Demurrage"
                fieldList.AddRange({"APPLYTEXT", "AMOUNTORD"})
            Case "GB_TankActivity"
                fieldList.AddRange({"APPLYTEXT", "CONTRACTORFIX", "ACTUALDATE"})
            Case Else
                fieldList.AddRange({"APPLYTEXT", "CURRENCYCODE", "AMOUNTORD", "CONTRACTORODR", "SCHEDELDATE", "REMARK"})
        End Select


        Dim keyFields As New List(Of String) From {"LINECNT"}

        '登録対象のデータを取得
        Dim splitTankMod As Boolean = True
        Dim entryType As String = ""
        If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
            splitTankMod = False
            entryType = CONST_ENTRYTYPE_NONBR
        End If
        'キー発着・フィールド
        Dim dicDemurrageCalcField As New Dictionary(Of String, List(Of String))
        With Nothing

            Dim COA0017FixValue As New COA0017FixValue
            COA0017FixValue.COMPCODE = GBC_COMPCODE_D
            COA0017FixValue.CLAS = CONST_FIXCLAS_DEMUCALCFIELD

            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                dicDemurrageCalcField = COA0017FixValue.VALUEDIC
            Else
                Throw New Exception("Fix value getError")
            End If

        End With
        Dim targetData = GetModifiedDataTable(dicDemurrageCalcField, splitTankMod)
        '登録対象データが0件の場合は処理終了
        If targetData Is Nothing OrElse targetData.Count = 0 Then
            messageNo = C_MESSAGENO.NOENTRYDATA
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        Dim checkDt As DataTable = Me.CreateOrderListTable
        For Each targetDataItem In targetData.Keys
            '削除は単項目チェック対象外とする
            If {ModifyType.del, ModifyType.delTank}.Contains(targetDataItem) Then
                Continue For
            End If
            checkDt.Merge(targetData(targetDataItem).CopyToDataTable)
        Next
        '単項目チェック
        If checkDt.Rows.Count > 0 Then
            messageNo = CheckSingle(CONST_MAPID, checkDt, fieldList, errMessage, keyFields:=keyFields)
            If messageNo <> C_MESSAGENO.NORMAL Then
                CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)

                '左ボックスにエラーメッセージ表示
                Me.txtRightErrorMessage.Text = errMessage
                Return
            End If
            'デマレージ精算月チェック
            If Me.hdnListMapVariant.Value = "GB_Demurrage" Then
                Dim dmyDate As Date = Now
                Dim dateString As String = ""
                Dim retMsg As New StringBuilder
                For Each checkDr As DataRow In checkDt.Rows
                    dateString = Convert.ToString(checkDr("ACTUALDATE"))
                    If dateString = "" Then
                        Continue For
                    End If
                    If Date.TryParse(dateString, dmyDate) = False Then
                        retMsg.AppendFormat("・{0}：{1}", "REPORT MONTH", "Invalid Month.").AppendLine()
                        retMsg.AppendFormat("--> {0} = {1}", padRight("REPORT MONTH", 20), Convert.ToString(checkDr("DEMREPORTMONTH"))).AppendLine()
                        messageNo = C_MESSAGENO.RIGHTBIXOUT
                        Continue For
                    End If

                    Dim currentBillingMonth As String = Convert.ToString(checkDr("CLOSINGMONTH"))
                    Dim inputMonth As String = Convert.ToString(checkDr("DEMREPORTMONTH"))
                    If currentBillingMonth > inputMonth Then
                        retMsg.AppendFormat("・{0}：{1}", "REPORT MONTH", "Past Month.").AppendLine()
                        retMsg.AppendFormat("--> {0} = {1}", padRight("REPORT MONTH", 20), Convert.ToString(checkDr("DEMREPORTMONTH"))).AppendLine()
                        messageNo = C_MESSAGENO.RIGHTBIXOUT
                        Continue For
                    End If

                Next
                If messageNo <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)

                    '左ボックスにエラーメッセージ表示
                    Me.txtRightErrorMessage.Text = retMsg.ToString
                    Return
                End If
            End If
            'TODO ここにタンク整合性チェック(発着地に対象のタンクが存在するか？)
        End If
        Me.ProcResult = EntryOrderValue(targetData, entryType, dicDemurrageCalcField)
        If Me.ProcResult.MessageNo <> C_MESSAGENO.NORMALDBENTRY Then
            Dim naeiw As String = C_NAEIW.ABNORMAL
            CommonFunctions.ShowMessage(Me.ProcResult.MessageNo, Me.lblFooterMessage, naeiw:=naeiw, pageObject:=Me)
            '左ボックス表示する結果の場合はメッセージを生成
            If Me.ProcResult.MessageNo = C_MESSAGENO.RIGHTBIXOUT Then
                Dim message As New StringBuilder
                'タンク引当更新失敗
                If Me.ProcResult.canNotEntryTankSeq.Count >= 1 Then
                    message.AppendLine("TANKSETTING ERROR")
                End If
                For Each item In Me.ProcResult.canNotEntryTankSeq
                    message.AppendFormat("ORDERNO='{0}',TANKSEQ='{1}'", item("ORDERNO"), item("TANKSEQ")).AppendLine()
                Next
                '他ユーザー更新メッセージ
                If Me.ProcResult.modOtherUsers.Count >= 1 Then
                    Dim dummyLabel As New Label
                    Dim errCannotUpdate As String = ""
                    CommonFunctions.ShowMessage(C_MESSAGENO.CANNOTUPDATE, dummyLabel)
                    errCannotUpdate = dummyLabel.Text
                    message.AppendFormat(errCannotUpdate).AppendLine()
                    For Each item In Me.ProcResult.modOtherUsers
                        message.AppendFormat("--> {0} = {1}", "No.", Convert.ToString(item("LINECNT"))).AppendLine()
                    Next
                End If
                '日付整合性エラー
                If Me.ProcResult.dateSeqError.Count >= 1 Then
                    Server.Transfer(Request.Url.LocalPath) '自身を再ロード
                    'Dim dummyLabel As New Label
                    'Dim errCannotUpdate As String = ""
                    'CommonFunctions.ShowMessage(C_MESSAGENO.VALIDITYINPUT, dummyLabel)
                    'errCannotUpdate = dummyLabel.Text
                    'message.AppendFormat(errCannotUpdate).AppendLine()
                    'For Each item In Me.ProcResult.dateSeqError
                    '    message.AppendFormat("--> {0} = {1}", "No.", Convert.ToString(item("LINECNT"))).AppendLine()
                    'Next
                End If
                'prevObj.ProcResult.modOtherUsers '→他ユーザーに更新されたDATAIDのリスト(上部で取得したdtで必要メッセージを生成)
                Me.txtRightErrorMessage.Text = message.ToString
            End If

        Else
            Server.Transfer(Request.Url.LocalPath) '自身を再ロード
        End If

        'btnBack_Click()
    End Sub
    ''' <summary>
    ''' 精算〆ボタン押下時
    ''' </summary>
    Public Sub btnBliingClose_Click()
        '変更済みデータ有無チェック
        Dim dicDemurrageCalcField As New Dictionary(Of String, List(Of String))

        With Nothing

            Dim COA0017FixValue As New COA0017FixValue
            COA0017FixValue.COMPCODE = GBC_COMPCODE_D
            COA0017FixValue.CLAS = CONST_FIXCLAS_DEMUCALCFIELD

            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                dicDemurrageCalcField = COA0017FixValue.VALUEDIC
            Else
                Throw New Exception("Fix value getError")
            End If

        End With
        Dim targetData = GetModifiedDataTable(dicDemurrageCalcField, False)
        '変更データがある場合はセーブを促し処理終了
        If targetData IsNot Nothing AndAlso targetData.Count > 0 Then
            CommonFunctions.ShowMessage(C_MESSAGENO.NOSAVECOSTITEM, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        'JOTSOAテーブルに転記するOrderValueを取得
        Dim procDate As Date = Now
        Dim dt As DataTable = GetSOAListData(isBliingClose:=True)
        'EXCEL帳票を背面で生成し金額を算出
        Dim dicCloseUpdValues = GetSummaryReportValues(dt)
        If dicCloseUpdValues Is Nothing Then
            Return
        End If
        dt = GetSOAListData(isBliingClose:=True, isForRerpotAndDisp:=False)
        'JOTBASE対象のオーダーNoを取得
        Dim qOdrNoGrp = From item In dt Where item("BRTYPE").Equals(C_BRTYPE.SALES) Group By odNo = Convert.ToString(item("ORDERNO")) Into Group Select odNo
        Dim odrNoList As New List(Of String)
        If qOdrNoGrp.Any Then
            odrNoList = qOdrNoGrp.ToList
        End If
        '強制締めデータを取得
        Dim forceCloseList = (From item In dt Where Convert.ToString(item("ISAUTOCLOSELONG")) = "1" AndAlso {"", "1900/01/01"}.Contains(Convert.ToString(item("SOAAPPDATE"))) AndAlso Not item("COSTCODE").Equals(GBC_COSTCODE_DEMURRAGE))

        '締め月の設定
        Dim reportMonth As String = FormatDateContrySettings(FormatDateYMD(Me.lblClosingDate.Text, GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
        Dim applyId As String = ""
        Dim lastStep As String = ""
        'JOTSOAテーブル群への転記及び締め日付更新
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            Using tran = sqlCon.BeginTransaction
                '強制締めの場合はOrderValueを0化再取得ののち以下の処理を実行
                If forceCloseList.Any Then
                    Dim forceCloseDt As DataTable = forceCloseList.CopyToDataTable
                    UpdateForceCloseOrderValue(forceCloseDt, sqlCon, tran, procDate)
                    '締めデータ再取得
                    dt = GetSOAListData(isBliingClose:=True, isForRerpotAndDisp:=False)
                End If
                'JOT SOAテーブルへの転記
                Dim messageNo As String = EntryJotSoaValue(dt, sqlCon, tran, procDate)
                If messageNo <> C_MESSAGENO.NORMAL Then
                    tran.Rollback()
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If
                'JOTSOABASEへの登録
                messageNo = EntryJotSoaBase(odrNoList, reportMonth, sqlCon, tran, procDate)
                If messageNo <> C_MESSAGENO.NORMAL Then
                    tran.Rollback()
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If
                'BILLING CLOSEテーブルの更新
                messageNo = EntryBillingClose(sqlCon, dicCloseUpdValues, applyId, lastStep, tran, procDate)
                If messageNo <> C_MESSAGENO.NORMAL Then
                    tran.Rollback()
                    CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage, pageObject:=Me)
                    Return
                End If
                tran.Commit()
            End Using
        End Using
        '申請メール送信
        Dim mailSendMessage As String = C_MESSAGENO.NORMAL
        mailSendMessage = SendSoaClosingMail(applyId, lastStep)
        Me.ProcResult = New ProcMessage With {.MessageNo = mailSendMessage}
        'Server.Transfer(Request.Url.LocalPath) '自身を再ロード
        btnBack_Click()
    End Sub
    '''' <summary>
    '''' SOA FIXボタン押下時処理
    '''' </summary>
    '''' <remarks>デマレッジ確定時コミッションを増幅</remarks>
    'Public Sub btnFix_Click()
    '    '変更済みデータ有無チェック
    '    Dim dicDemurrageCalcField As New Dictionary(Of String, List(Of String))
    '    With Nothing

    '        Dim COA0017FixValue As New COA0017FixValue
    '        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
    '        COA0017FixValue.CLAS = CONST_FIXCLAS_DEMUCALCFIELD

    '        COA0017FixValue.COA0017getListFixValue()
    '        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
    '            dicDemurrageCalcField = COA0017FixValue.VALUEDIC
    '        Else
    '            Throw New Exception("Fix value getError")
    '        End If

    '    End With
    '    Dim targetData = GetModifiedDataTable(dicDemurrageCalcField, False)
    '    '変更データがある場合はセーブを促し処理終了
    '    If targetData IsNot Nothing AndAlso targetData.Count > 0 Then
    '        CommonFunctions.ShowMessage(C_MESSAGENO.NOSAVECOSTITEM, Me.lblFooterMessage)
    '        Return
    '    End If

    '    Dim targetDt = (From item As DataRow In Me.SavedDt
    '                    Where Convert.ToString(item("SOAAPPDATE")) <> ""
    '                    Select item)
    '    If targetDt.Any = False Then
    '        CommonFunctions.ShowMessage(C_MESSAGENO.NOENTRYDATA, Me.lblFooterMessage)
    '        Return
    '    End If
    '    Dim procDate As Date = Date.Now
    '    Using sqlCon As New SqlConnection(COA0019Session.DBcon)
    '        sqlCon.Open()
    '        For Each dr As DataRow In targetDt
    '            AddAgentCommRecord(dr, sqlCon, procDate:=procDate)
    '        Next
    '    End Using
    '    Me.ProcResult = New ProcMessage With {.MessageNo = C_MESSAGENO.NORMAL}
    '    Server.Transfer(Request.Url.LocalPath) '自身を再ロード
    'End Sub

    ''' <summary>
    ''' 備考入力ボックスのOKボタン押下時イベント
    ''' </summary>
    Public Sub btnRemarkInputOk_Click()
        If Me.hdnRemarkboxField.Value.Contains("txtWF_LISTAREAAPPLYTEXT") Then
            DisplayApplyReason(False)
        ElseIf Me.hdnRemarkboxField.Value.Contains("txtWF_LISTAREAREMARK") Then
            DisplayRemark(False)
        End If

        Me.hdnRemarkboxOpen.Value = ""
        Me.hdnRemarkboxField.Value = ""
        Me.hdnCurrentUnieuqIndex.Value = ""
        'マルチライン入力ボックスの非表示
        Me.divRemarkInputBoxWrapper.Style("display") = "none"
    End Sub
    ''' <summary>
    ''' 備考入力ボックスのキャンセルボタン押下時イベント
    ''' </summary>
    Public Sub btnRemarkInputCancel_Click()

        Me.hdnRemarkboxOpen.Value = ""
        Me.hdnRemarkboxField.Value = ""
        Me.hdnCurrentUnieuqIndex.Value = ""
        'マルチライン入力ボックスの非表示
        Me.divRemarkInputBoxWrapper.Style("display") = "none"
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
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return
            End If
        Else
            dt = Me.SavedDt
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
    ''' 削除ボタン押下時イベント
    ''' </summary>
    Public Sub btnListDelete_Click()
        CommonFunctions.ShowConfirmMessage(C_MESSAGENO.CONFIRMDELETE, pageObject:=Me, submitButtonId:="btnListDeleteOK")
    End Sub
    ''' <summary>
    ''' 削除ボタン押下時イベント
    ''' </summary>
    Public Sub btnListDeleteOK_Click()
        '削除対象のLINECNTを取得
        Dim deleteLineCnt As Integer = 0
        Integer.TryParse(Me.hdnListCurrentRownum.Value, deleteLineCnt)
        If deleteLineCnt = 0 Then
            'ありえないが押された行が取得できない場合はそのまま終了
            Return
        End If
        Dim COA0021ListTable As New COA0021ListTable
        '一覧表示データ復元 
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If

        '復元した情報をもとに削除する行データを取得
        '最大ACTYNOを取得
        Dim deleteRow As DataRow = (From dr As DataRow In dt
                                    Where dr.Item("LINECNT").Equals(deleteLineCnt)
                                    Select dr).FirstOrDefault()
        'ありえないがLINECNTでデータが取得できない場合はそのまま終了
        If deleteRow Is Nothing Then
            Return
        End If
        '削除対象のTANKSEQを保持
        Dim tankSeq As String = Convert.ToString(deleteRow.Item("TANKSEQ"))
        '削除後のデータテーブル
        Dim saveRowCollection = (From dr As DataRow In dt
                                 Where Not dr.Item("LINECNT").Equals(deleteLineCnt)
                                 Select dr)

        Dim saveDt As DataTable = Nothing
        If saveRowCollection Is Nothing OrElse saveRowCollection.Any = False Then
            saveDt = Me.CreateOrderListTable
        Else
            saveDt = saveRowCollection.CopyToDataTable
        End If
        Dim lineCnt As Integer = 1
        Dim reNumberActyNo As Integer = 1
        Dim isReNoActy As Boolean = True
        If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
            isReNoActy = False
        End If
        For Each saveDr As DataRow In saveDt.Rows
            saveDr.Item("LINECNT") = lineCnt
            lineCnt = lineCnt + 1
            'ACTYNO振り直し(追加対象のTANKSEQの場合)
            If isReNoActy = True AndAlso tankSeq = Convert.ToString(saveDr.Item("TANKSEQ")) Then
                saveDr.Item("ACTYNO") = reNumberActyNo.ToString("000")
                reNumberActyNo = reNumberActyNo + 1
            End If
        Next
        'ファイルを保存
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = saveDt
        COA0021ListTable.COA0021saveListTable()
        Me.SavedDt = saveDt
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me)
        Else
            'TODO 確認削除確認メッセージを出したうえでさらに正常終了ポップアップさせるか？
            CommonFunctions.ShowMessage(C_MESSAGENO.NORMAL, naeiw:=C_NAEIW.NORMAL, lblObject:=Me.lblFooterMessage)
        End If
    End Sub
    ''' <summary>
    ''' [絞り込み条件]費用コード変更時イベント
    ''' </summary>
    Public Sub txtCostItem_Change()
        Dim costCode As String = Me.txtCostItem.Text.Trim
        Me.lblCostItemText.Text = ""
        If costCode = "" Then
            Return
        End If
        Dim brType As String = ""
        If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
            brType = C_BRTYPE.NONBR
        End If
        Dim dt As DataTable = GetCostItem(brType, costCode)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            txtCostItem.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblCostItemText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
        End If
    End Sub
    ''' <summary>
    ''' [絞り込み条件]Actyコード変更時イベント
    ''' </summary>
    Public Sub txtActy_Change()
        Dim actyCode As String = Me.txtActy.Text.Trim
        Me.lblActyText.Text = ""
        If actyCode = "" Then
            Return
        End If

        Dim dt As DataTable = GetActy(actyCode)
        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            Dim dr As DataRow = dt.Rows(0)
            Me.txtActy.Text = Convert.ToString(dr.Item("CODE"))
            Me.lblActyText.Text = HttpUtility.HtmlEncode(dr.Item("NAME"))
        End If
    End Sub
    ''' <summary>
    ''' [絞り込み条件]ベンダー変更時イベント
    ''' </summary>
    Public Sub txtVender_Change()
        Try
            Me.lblVenderText.Text = ""
            If Me.txtVender.Text.Trim = "" Then
                Return
            End If

            If Me.lbVender.Items.Count > 0 Then
                Dim findListItem = Me.lbVender.Items.FindByValue(Me.txtVender.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblVenderText.Text = parts(1)
                    Else
                        Me.lblVenderText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbVender.Items.FindByValue(Me.txtVender.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblVenderText.Text = parts(1)
                            Me.txtVender.Text = parts(0)
                        Else
                            Me.lblVenderText.Text = findListItemUpper.Text
                            Me.txtVender.Text = findListItemUpper.Value
                        End If

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
    ''' [絞り込み条件]ベンダー変更時イベント
    ''' </summary>
    Public Sub txtBrVender_Change()
        Try
            Me.lblBrVenderText.Text = ""
            If Me.txtBrVender.Text.Trim = "" Then
                Return
            End If

            If Me.lbVender.Items.Count > 0 Then
                Dim findListItem = Me.lbVender.Items.FindByValue(Me.txtBrVender.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblBrVenderText.Text = parts(1)
                    Else
                        Me.lblBrVenderText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbVender.Items.FindByValue(Me.txtBrVender.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblBrVenderText.Text = parts(1)
                            Me.txtBrVender.Text = parts(0)
                        Else
                            Me.lblBrVenderText.Text = findListItemUpper.Text
                            Me.txtBrVender.Text = findListItemUpper.Value
                        End If

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
    ''' [絞り込み条件]ベンダー変更時イベント
    ''' </summary>
    Public Sub txtEstimatedVender_Change()
        Try

            Me.lblEstimatedVenderText.Text = ""
            If Me.txtEstimatedVender.Text.Trim = "" Then
                Return
            End If

            If Me.lbVender.Items.Count > 0 Then
                Dim findListItem = Me.lbVender.Items.FindByValue(Me.txtEstimatedVender.Text)
                If findListItem IsNot Nothing Then
                    Dim parts As String()
                    If findListItem.Text.Contains(":") Then
                        parts = Split(findListItem.Text, ":", -1, CompareMethod.Text)
                        Me.lblEstimatedVenderText.Text = parts(1)
                    Else
                        Me.lblEstimatedVenderText.Text = findListItem.Text
                    End If
                Else
                    Dim findListItemUpper = Me.lbVender.Items.FindByValue(Me.txtEstimatedVender.Text.ToUpper)
                    If findListItemUpper IsNot Nothing Then
                        Dim parts As String()
                        If findListItemUpper.Text.Contains(":") Then
                            parts = Split(findListItemUpper.Text, ":", -1, CompareMethod.Text)
                            Me.lblEstimatedVenderText.Text = parts(1)
                            Me.txtEstimatedVender.Text = parts(0)
                        Else
                            Me.lblEstimatedVenderText.Text = findListItemUpper.Text
                            Me.txtEstimatedVender.Text = findListItemUpper.Value
                        End If

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
    ''' 一覧表予定日（オーダー）変更時イベント
    ''' </summary>
    ''' <param name="param">キー:SENDER 値：変更したテキストボックスID</param>
    '''                     キー:ROW       値：対象の行
    Public Sub txtListDate_Change(param As Hashtable)
        Dim val As String = ""
        Dim targetObjId As String = Convert.ToString(param("SENDER"))
        Dim rowNum As String = Convert.ToString(param("ROW"))
        If Request.Form.AllKeys.Contains(targetObjId) = True Then
            val = Request.Form.Item(targetObjId)
            val = val.Trim
            Dim tmpDate As Date
            If val <> "" AndAlso Date.TryParseExact(val, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, tmpDate) = False Then
                Return '日付に変換できない場合はそのまま終了(他のACTYと連動させない）
            ElseIf val <> "" Then
                val = tmpDate.ToString("yyyy/MM/dd") '一旦yyyy/MM/dd形式に変更（TODO：国ごとの日付フォーマット)
            End If
        End If
        'カレンダーでの変更と同様のACTYIDでの連動を実行
        UpdateDatatableDate(val, targetObjId, rowNum)
    End Sub
    ''' <summary>
    ''' 一覧表通貨コード変更時イベント
    ''' </summary>
    ''' <param name="param">キー:SENDER 値：変更したテキストボックスID</param>
    '''                     キー:ROW       値：対象の行
    Public Sub txtListCurrency_Change(param As Hashtable)
        Dim val As String = ""
        Dim targetObjId As String = Convert.ToString(param("SENDER"))
        Dim rowNum As String = Convert.ToString(param("ROW"))
        If Request.Form.AllKeys.Contains(targetObjId) = True Then
            val = Request.Form.Item(targetObjId)
            val = val.Trim

        End If
        '通貨コードの変更
        UpdateDatatableCurrency(val, rowNum)
    End Sub
    ''' <summary>
    ''' 一覧表業者コード変更時イベント
    ''' </summary>
    ''' <param name="param">キー:SENDER 値：変更したテキストボックスID</param>
    '''                     キー:ROW       値：対象の行
    Public Sub txtListContractor_Change(param As Hashtable)
        Dim val As String = ""
        Dim targetObjId As String = Convert.ToString(param("SENDER"))
        Dim rowNum As String = Convert.ToString(param("ROW"))
        If Request.Form.AllKeys.Contains(targetObjId) = True Then
            val = Request.Form.Item(targetObjId)
            val = val.Trim
        End If
        '業者コード連動変更の実行
        UpdateDatatableContractor(val, targetObjId, rowNum)
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
        AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnExcelDownload, "Excel出力", "Excel Download")
        AddLangSetting(dicDisplayText, Me.btnApply, "申請", "Apply")
        'AddLangSetting(dicDisplayText, Me.btnFix, "確定", "SOA Fix")
        AddLangSetting(dicDisplayText, Me.btnBack, "戻る", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")
        AddLangSetting(dicDisplayText, Me.lblOrderNoLabel, "オーダーId", "Order ID")
        AddLangSetting(dicDisplayText, Me.lblCostItemLabel, "費用", "Cost Item")
        AddLangSetting(dicDisplayText, Me.lblActy, "アクションコード", "Acty")
        AddLangSetting(dicDisplayText, Me.lblVenderLabel, "業者", "Vendor")
        AddLangSetting(dicDisplayText, Me.lblBrVenderLabel, "業者 - BR", "Breaker Vendor")
        AddLangSetting(dicDisplayText, Me.lblEstimatedVenderLabel, "業者 - 予定", "Estimated Vendor")
        AddLangSetting(dicDisplayText, Me.lblUsdAmountSummaryLabel, "金額計($)", "Total Amount($)")
        AddLangSetting(dicDisplayText, Me.hdnListDeleteName, "削除", "Delete")
        AddLangSetting(dicDisplayText, Me.btnAddCost, "費用追加", "Add Cost")

        'ファイルアップロードメッセージ
        AddLangSetting(dicDisplayText, Me.hdnUploadMessage01, "ファイルアップロード開始", "Start uploading files")
        AddLangSetting(dicDisplayText, Me.hdnUploadError01, "ファイルアップロードが失敗しました。", "File upload failed.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError02, "通信を中止しました。", "Communication was canceled.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError03, "タイムアウトエラーが発生しました。", "A timeout error occurred.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError04, "更新権限がありません。", "do not have update permission.")
        AddLangSetting(dicDisplayText, Me.hdnUploadError05, "対応外のファイル形式です。", "It is an incompatible file format.")

        '（仮）
        AddLangSetting(dicDisplayText, Me.btnBliingClose, "精算締め", "Billing Close")
        AddLangSetting(dicDisplayText, Me.lblClosingDateLabel, "精算月", "Billing Month")
        '（仮）タンク番号条件
        AddLangSetting(dicDisplayText, Me.lblTankNoLabel, "Tank No.", "Tank No.")

        AddLangSetting(dicDisplayText, Me.lblAllocateTankCount, "引当数", "Allocate Count")
        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub


    ''' <summary>
    ''' オーダー関連の各種テーブルより情報を取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetOrderListData(Optional applyId As String = "") As DataTable

        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        Dim textCostTblField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCostTblField = "NAMES"
        End If
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If

        'ソート順取得
        COA0020ProfViewSort.MAPID = CONST_MAPID
        COA0020ProfViewSort.VARI = If(Me.hdnListMapVariant.Value = "GB_CostUp", Me.hdnListMapVariant.Value, "Default")
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder()
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,('SYS' + right('00000' + trim(convert(char,ROW_NUMBER() OVER(ORDER BY ORDERNO))), 5)) AS SYSKEY")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        sqlStat.AppendLine("FROM (")

        sqlStat.AppendLine("SELECT '' AS OPERATION")
        sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
        sqlStat.AppendLine("     , '1' AS 'SELECT' ")
        If Me.hdnListId.Value = "DefaultTankAllocate" AndAlso Me.txtActy.Text <> "" Then
            sqlStat.AppendLine("     , CASE VL.ACTIONID WHEN @ACTIONID THEN '0' ELSE '1' END AS HIDDEN ")
        Else
            sqlStat.AppendLine("     , '0' AS HIDDEN ")
        End If
        sqlStat.AppendLine("     , 'P00001'      AS USETYPE") '本当は親を保持しUSETYPE
        sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
        sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
        sqlStat.AppendLine("     , OBS.BRTYPE    AS BRTYPR")
        sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
        sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
        sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
        sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
        sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
        sqlStat.AppendFormat("     , ISNULL(CST.{0},'') AS COSTNAME", textCostTblField).AppendLine()
        sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTIONID")
        sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
        sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")

        'sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.LOADCOUNTRY1")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.LOADCOUNTRY2")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.DISCHARGECOUNTRY1")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.DISCHARGECOUNTRY2")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN ''")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN ''")
        'sqlStat.AppendLine("            ELSE '' END AS COUNTRYCODE")
        sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
        sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
        sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
        sqlStat.AppendLine("     , CNTY.TAXRATE     AS TAXRATE")

        sqlStat.AppendLine("     , VL.AMOUNTBR         AS AMOUNTBR")
        sqlStat.AppendLine("     , VL.AMOUNTORD        AS AMOUNTORD")
        sqlStat.AppendLine("     , VL.AMOUNTFIX        AS AMOUNTFIX")
        sqlStat.AppendLine("     , VL.AMOUNTPAY     AS AMOUNTPAY")
        sqlStat.AppendLine("     , VL.LOCALPAY     AS LOCALPAY")
        '業者コード
        sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
        sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
        sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")
        '業者名
        'sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTBR.{0} IS NOT NULL)  THEN ISNULL(CTBR.{0},'')  ELSE COALESCE(DPBR.NAMES,TRBR.NAMES,'')   END AS CONTRACTORNAMEBR ", textCustomerTblField).AppendLine()
        'sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTODR.{0} IS NOT NULL) THEN ISNULL(CTODR.{0},'') ELSE COALESCE(DPODR.NAMES,TRODR.NAMES,'') END AS CONTRACTORNAMEODR ", textCustomerTblField).AppendLine()
        'sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTFIX.{0} IS NOT NULL) THEN ISNULL(CTFIX.{0},'') ELSE COALESCE(DPFIX.NAMES,TRFIX.NAMES,'') END AS CONTRACTORNAMEFIX ", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTBR.{0} IS NOT NULL)  THEN ISNULL(CTBR.{0},'')  ELSE COALESCE(PTBR.AREANAME,DPBR.NAMES,TRBR.NAMES,'')   END AS CONTRACTORNAMEBR ", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTODR.{0} IS NOT NULL) THEN ISNULL(CTODR.{0},'') ELSE COALESCE(PTODR.AREANAME,DPODR.NAMES,TRODR.NAMES,'') END AS CONTRACTORNAMEODR ", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTFIX.{0} IS NOT NULL) THEN ISNULL(CTFIX.{0},'') ELSE COALESCE(PTFIX.AREANAME,DPFIX.NAMES,TRFIX.NAMES,'') END AS CONTRACTORNAMEFIX ", textCustomerTblField).AppendLine()

        sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
        sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
        sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
        sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
        sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
        sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
        sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
        sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
        sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")

        sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
        sqlStat.AppendLine("     , VL.BRID           AS BRID")
        sqlStat.AppendLine("     , VL.BRCOST         AS BRCOST")
        sqlStat.AppendLine("     , ''                AS ACTYNO")
        sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")
        sqlStat.AppendLine("     , RIGHT(VL.DTLPOLPOD,1) + REPLACE(REPLACE(VL.DTLPOLPOD,'POL','000'),'POD','001') AS AGENTKBNSORT")
        sqlStat.AppendLine("     , CASE WHEN ISNULL(VL.DISPSEQ,'') = '' THEN '1' ")
        sqlStat.AppendLine("            ELSE '0' END AS DISPSEQISEMPTY")

        sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.AGENTPOL1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.AGENTPOL2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.AGENTPOD1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.AGENTPOD2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN OBS.AGENTORGANIZER")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN VL.AGENTORGANIZER")
        sqlStat.AppendLine("            ELSE '' END AS AGENT")


        sqlStat.AppendLine("     , OBS.TIP          AS TIP")
        sqlStat.AppendLine("     , OBS.DEMURTO      AS DEMURTO")
        sqlStat.AppendLine("     , OBS.DEMURUSRATE1 AS DEMURUSRATE1")
        sqlStat.AppendLine("     , OBS.DEMURUSRATE2 AS DEMURUSRATE2")
        sqlStat.AppendLine("     , ISNULL(CST.CLASS1,'') AS CHARGE_CLASS1")
        sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'') AS CHARGE_CLASS4")
        sqlStat.AppendLine("     , CASE WHEN VL.INVOICEDBY = @JOTAGENT THEN 'on' ELSE '' END AS JOT")
        sqlStat.AppendLine("     , VL.DATEFIELD")
        sqlStat.AppendLine("     , AH.STATUS AS STATUSCODE")
        sqlStat.AppendLine("     , VL.BRCOST AS ACTION")
        sqlStat.AppendLine("     , VL.REMARK AS REMARK")
        sqlStat.AppendLine("     , VL.BRADDEDCOST AS BRADDEDCOST")
        'sqlStat.AppendLine("     , CASE WHEN JOTSOAVL.DATAIDODR IS NULL THEN '0' ELSE '1' END AS IS_BILLINGCLOSED")

        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OBS")
        sqlStat.AppendLine("    ON OBS.ORDERNO = VL.ORDERNO")
        sqlStat.AppendLine("   AND OBS.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST")
        sqlStat.AppendLine("    ON CST.COMPCODE  = @COMPCODE")
        sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
        sqlStat.AppendLine("   AND '1' = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' AND CST.LDKBN IN ('B','L') THEN '1' ")
        sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'POD%' AND CST.LDKBN IN ('B','D') THEN '1' ")
        sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
        sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'Organizer' AND CST.LDKBN IN ('D') THEN '' ")
        sqlStat.AppendLine("                  ELSE '1'")
        sqlStat.AppendLine("             END")
        sqlStat.AppendLine("   AND CST.STYMD     <= VL.STYMD")
        sqlStat.AppendLine("   AND CST.ENDYMD    >= VL.STYMD")
        sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH") '承認履歴
        sqlStat.AppendLine("    ON  AH.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AH.APPLYID      = VL.APPLYID")
        sqlStat.AppendLine("   AND  AH.STEP         = VL.LASTSTEP")
        sqlStat.AppendLine("   AND  AH.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FV.CLASS        = 'APPROVAL'")
        'sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN AH.STATUS IS NOT NULL THEN AH.STATUS ")
        'sqlStat.AppendLine("                               WHEN VL.AMOUNTFIX <> VL.AMOUNTORD THEN '" & C_APP_STATUS.APPAGAIN & "'")
        'sqlStat.AppendLine("                               ELSE NULL")
        'sqlStat.AppendLine("                           END")
        sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '' AND AH.STATUS <> '" & C_APP_STATUS.APPROVED & "') THEN AH.STATUS ")
        sqlStat.AppendLine("                               WHEN ((AH.STATUS IS NULL OR AH.STATUS = '' OR AH.STATUS = '" & C_APP_STATUS.APPROVED & "') AND VL.AMOUNTFIX <> VL.AMOUNTORD) THEN '" & C_APP_STATUS.APPAGAIN & "'")
        sqlStat.AppendLine("                               WHEN AH.STATUS IS NOT NULL THEN AH.STATUS ")
        sqlStat.AppendLine("                               ELSE NULL")
        sqlStat.AppendLine("                           END")
        sqlStat.AppendLine("   AND  FV.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CNTY")
        sqlStat.AppendLine("         ON CNTY.COUNTRYCODE      = VL.COUNTRYCODE")
        sqlStat.AppendLine("        AND CNTY.DELFLG           <> @DELFLG")
        sqlStat.AppendLine("        AND CNTY.STYMD            <= (case when VL.ACTUALDATE = '1900/01/01' then @NOWDATE else VL.ACTUALDATE end)")
        sqlStat.AppendLine("        AND CNTY.ENDYMD           >= (case when VL.ACTUALDATE = '1900/01/01' then @NOWDATE else VL.ACTUALDATE end)")

        '*BR_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0002_PORT PTBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = PTBR.PORTCODE ")
        sqlStat.AppendLine("       AND  PTBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  PTBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  PTBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  PTBR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CTBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = CTBR.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CTBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CTBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  CTBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  CTBR.DELFLG      <> @DELFLG")
        '*BR_CONTRACTOR名取得JOIN END

        '*ODR_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0002_PORT PTODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = PTODR.PORTCODE ")
        sqlStat.AppendLine("       AND  PTODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  PTODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  PTODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  PTODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CTODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = CTODR.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CTODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CTODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  CTODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  CTODR.DELFLG      <> @DELFLG")
        '*ODR_CONTRACTOR名取得JOIN END

        '*FIX_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0002_PORT PTFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = PTFIX.PORTCODE ")
        sqlStat.AppendLine("       AND  PTFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  PTFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  PTFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  PTFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CTFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = CTFIX.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CTFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CTFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  CTFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  CTFIX.DELFLG      <> @DELFLG")
        '*FIX_CONTRACTOR名取得JOIN END

        '*ブレーカー基本情報取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBT0002_BR_BASE BB")
        sqlStat.AppendLine("        ON  BB.BRID    = OBS.BRID ")
        sqlStat.AppendLine("       AND  BB.DELFLG <> @DELFLG")
        '*ブレーカー基本情報取得JOIN END
        If Me.hdnListMapVariant.Value = "GB_CostUp" Then
            ''SOACLOSE連動済確認JOIN START
            sqlStat.AppendLine("  LEFT JOIN (SELECT DISTINCT JOTSOAVLS.REPORTMONTH,JOTSOAVLS.DATAIDODR FROM GBT0008_JOTSOA_VALUE JOTSOAVLS  with(nolock)")
            sqlStat.AppendLine("        WHERE JOTSOAVLS.SOAAPPDATE   <> @INITSOAAPDATE")
            sqlStat.AppendLine("          AND JOTSOAVLS.CLOSINGMONTH  = JOTSOAVLS.REPORTMONTH")
            sqlStat.AppendLine("          AND JOTSOAVLS.DELFLG       <> @DELFLG")
            sqlStat.AppendLine("             ) JOTSOAVL")
            sqlStat.AppendLine("    ON JOTSOAVL.DATAIDODR   = VL.DATAID")

            ''SOACLOSE連動済確認JOIN END
        End If

        sqlStat.AppendLine(" WHERE VL.DELFLG    <> @DELFLG")
        'sqlStat.AppendLine("   AND VL.DTLPOLPOD <> @DTLPOLPOD")
        'sqlStat.AppendLine("   AND CST.CLASS1 IN ('" & GBC_CHARGECLASS1.ADMINISTRATION & "','" & GBC_CHARGECLASS1.OPEEXPENSES & "' )")
        sqlStat.AppendLine("   AND (   CST.CLASS3 <> ''")
        sqlStat.AppendLine("        OR EXISTS(")
        sqlStat.AppendLine("                   SELECT 1 ")
        sqlStat.AppendLine("                     FROM COS0017_FIXVALUE FXSL")
        sqlStat.AppendLine("                    WHERE FXSL.COMPCODE = '" & GBC_COMPCODE_D & "'")
        sqlStat.AppendLine("                      AND FXSL.SYSCODE  = '" & C_SYSCODE_GB & "'")
        sqlStat.AppendLine("                      AND FXSL.CLASS    = 'LEASEPAYMENT'")
        sqlStat.AppendLine("                      AND FXSL.VALUE3   = VL.COSTCODE")
        sqlStat.AppendLine("                      AND FXSL.STYMD   <= VL.STYMD")
        sqlStat.AppendLine("                      AND FXSL.ENDYMD  >= VL.STYMD")
        sqlStat.AppendLine("                      AND FXSL.DELFLG  <> @DELFLG")
        sqlStat.AppendLine("                 )")
        sqlStat.AppendLine("        )")
        If Me.hdnOrderNo.Value <> "" Then
            sqlStat.AppendLine("   AND VL.ORDERNO    = @ORDERNO")
        End If
        If applyId <> "" Then
            sqlStat.AppendLine("   AND VL.APPLYID    = @APPLYID")
        End If
        sqlStat.AppendLine("   AND VL.COSTCODE  <> @COSTCODE")
        sqlStat.AppendLine("   AND EXISTS(SELECT 1 ") '基本情報が削除されていたら対象外
        sqlStat.AppendLine("                FROM GBT0004_ODR_BASE OBSS")
        sqlStat.AppendLine("               WHERE OBSS.ORDERNO = VL.ORDERNO")
        sqlStat.AppendLine("                 AND OBSS.DELFLG    <> @DELFLG)")
        sqlStat.AppendLine("   AND NOT EXISTS (SELECT 1 ") 'デマレッジ終端アクションはタンク動静のみ表示
        sqlStat.AppendLine("                     FROM GBM0010_CHARGECODE CSTS")
        sqlStat.AppendLine("                    WHERE CSTS.COMPCODE = @COMPCODE")
        sqlStat.AppendLine("                      AND CSTS.COSTCODE = VL.COSTCODE")
        sqlStat.AppendLine("                      AND CSTS.CLASS10  = '" & CONST_FLAG_YES & "'")
        sqlStat.AppendLine("                      AND CSTS.STYMD   <= VL.STYMD")
        sqlStat.AppendLine("                      AND CSTS.ENDYMD  >= VL.STYMD")
        sqlStat.AppendLine("                      AND CSTS.DELFLG  <> @DELFLG")
        sqlStat.AppendLine("                  )")

        If Me.hdnListMapVariant.Value = "GB_CostUp" Then
            sqlStat.AppendLine("   AND JOTSOAVL.DATAIDODR IS NULL")
        End If

        sqlStat.AppendLine("   ) TBL")

        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        Dim dtDbResult As New DataTable
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                If Me.hdnOrderNo.Value <> "" Then
                    .Add("@ORDERNO", SqlDbType.NVarChar, 20).Value = Me.hdnOrderNo.Value
                End If
                If applyId <> "" Then
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                End If
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                '.Add("@DTLPOLPOD", SqlDbType.NVarChar, 20).Value = "Organizer"
                .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                .Add("@COSTCODE", SqlDbType.NVarChar).Value = GBC_COSTCODE_DEMURRAGE
                .Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT
                If Me.txtActy.Text <> "" Then
                    .Add("@ACTIONID", SqlDbType.NVarChar).Value = Me.txtActy.Text
                End If
                .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@INITSOAAPDATE", System.Data.SqlDbType.Date).Value = "1900/01/01"
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using

        Dim retDt As DataTable = CreateOrderListTable()
        Dim loopEnd As Integer = 1
        Integer.TryParse(Me.hdnCopy.Value, loopEnd)
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next

        '対象データ０件以外
        If dtDbResult.Rows.Count <> 0 Then

            Dim actyNo As Integer = 0
            Dim orderNo As String = Convert.ToString(dtDbResult.Rows(0).Item("ORDERNO"))
            Dim tankSeq As String = Convert.ToString(dtDbResult.Rows(0).Item("TANKSEQ"))
            For Each readDr As DataRow In dtDbResult.Rows
                '同一カラム名を単純転送
                Dim writeDr As DataRow = retDt.NewRow
                writeDr.BeginEdit()
                For Each colName In colNameList
                    If colName = "DISPSEQ" Then
                        writeDr.Item(colName) = Convert.ToString(readDr.Item(colName))
                    Else
                        writeDr.Item(colName) = readDr.Item(colName)
                    End If
                Next
                If Not (tankSeq.Equals(readDr.Item("TANKSEQ")) _
                        AndAlso orderNo.Equals(readDr.Item("ORDERNO"))) Then
                    actyNo = 0
                    orderNo = Convert.ToString(readDr.Item("ORDERNO"))
                    tankSeq = Convert.ToString(readDr.Item("TANKSEQ"))
                End If
                actyNo = actyNo + 1
                writeDr.Item("ACTYNO") = actyNo.ToString("000")
                writeDr.EndEdit()
                SetCanRowEdit(writeDr)
                retDt.Rows.Add(writeDr)
            Next
        End If

        Return retDt

    End Function
    ''' <summary>
    ''' オーダー(タンク動静)関連の各種テーブルより情報を取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetTankActivityListData(Optional applyId As String = "") As DataTable
        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        'ここの処理は本来オーダーテーブルより取得を行う
        'Dim brNo As String = Me.hdnBrId.Value
        Dim copy As String = Me.hdnCopy.Value
        Dim textCostTblField As String = "NAMESJP"
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCostTblField = "NAMES"
            textCustomerTblField = "NAMESEN"
        End If
        'ソート順取得
        COA0020ProfViewSort.MAPID = mapId
        COA0020ProfViewSort.VARI = Me.hdnListMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlEtdEtaOrderCondition As New StringBuilder
        Dim etaDatefield As String = ""
        Dim etaActy As String = "('ARVD','DCEC','DCED','ETYC')"

        If Me.hdnETAStYMD.Value <> "" Then
            sqlEtdEtaOrderCondition.AppendLine(" AND ")
            '予定パターン
            If Me.hdnSearchType.Value = "01SCHE" Then
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
                If Me.hdnETAStYMD.Value <> "" Then
                    With sqlEtdEtaOrderCondition
                        .AppendLine("(    (     OBS.ETA1 BETWEEN @ETAST AND @ETAEND") 'オーダー基本のETA ETDが収まっていること
                        .AppendLine("     )")
                        .AppendLine(" OR  (     OBS.ETA2 BETWEEN @ETAST AND @ETAEND")
                        .AppendLine("     )")
                        .AppendLine(" OR  (     ODVALETA_W.ORDERNO is not null )")
                        '.AppendLine(" OR  (     EXISTS(SELECT 1 ") 'オーダー明細SHIPがETDの範囲に存在するか
                        '.AppendLine("                   FROM GBT0005_ODR_VALUE ODVALETA ")
                        '.AppendLine("                  WHERE ODVALETA.ORDERNO   = OBS.ORDERNO ")
                        '.AppendLine("                    AND ODVALETA.ACTIONID in " & etaActy & " ")
                        '.AppendLine("                    AND ODVALETA.DELFLG   <> @DELFLG ")
                        '.AppendLine("                    AND CASE WHEN ODVALETA.SCHEDELDATE = '1900/01/01'")
                        '.AppendLine("                               THEN ODVALETA.SCHEDELDATEBR")
                        '.AppendLine("                             ELSE ODVALETA.SCHEDELDATE END BETWEEN @ETAST AND @ETAEND")
                        '.AppendLine("                 )") 'オーダー明細SHIP END
                        '.AppendLine("     )")
                        .AppendLine(")")
                    End With
                End If
            End If
            '実績パターン
            If Me.hdnSearchType.Value = "02FIX" Then
                etaDatefield = "(SELECT TOP 1 ODVALETD.ACTUALDATE AS ETA{0} " &
                               "   FROM GBT0005_ODR_VALUE ODVALETD " &
                               "  WHERE ODVALETD.ORDERNO = OBS.ORDERNO " &
                               "    AND ODVALETD.ACTIONID  in " & etaActy & " " &
                               "    AND ODVALETD.DTLPOLPOD  = 'POD{0}' " &
                               "    AND ODVALETD.DELFLG   <> @DELFLG" &
                               "  ORDER BY ODVALETD.DISPSEQ)"

                'オーダー明細
                If Me.hdnETAStYMD.Value <> "" Then
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

        Dim sqlStat As New StringBuilder()
        sqlStat.AppendLine("Select ROW_NUMBER() OVER(ORDER BY HIDDEN," & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        sqlStat.AppendLine("      ,('SYS' + right('00000' + trim(convert(char,ROW_NUMBER() OVER(ORDER BY HIDDEN," & COA0020ProfViewSort.SORTSTR & "))), 5)) AS SYSKEY")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT '' AS OPERATION")
        sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
        sqlStat.AppendLine("     , '1' AS 'SELECT' ")
        sqlStat.AppendLine("     , CASE VL.ACTIONID WHEN '' THEN '1' ELSE '0' END AS HIDDEN ")
        sqlStat.AppendLine("     , 'P00001'      AS USETYPE") '本当は親を保持しUSETYPE
        sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
        sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
        sqlStat.AppendLine("     , OBS.BRTYPE    AS BRTYPR")
        sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
        sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
        sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
        sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
        sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
        sqlStat.AppendFormat("     , ISNULL(CST.{0},'') AS COSTNAME", textCostTblField).AppendLine()
        sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTIONID")
        sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
        sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
        sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")
        'sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.LOADCOUNTRY1")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.LOADCOUNTRY2")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.DISCHARGECOUNTRY1")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.DISCHARGECOUNTRY2")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN ''")
        'sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN ''")
        'sqlStat.AppendLine("            ELSE '' END AS COUNTRYCODE")
        sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
        sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
        sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
        sqlStat.AppendLine("     , CNTY.TAXRATE   AS TAXRATE")
        sqlStat.AppendLine("     , VL.AMOUNTBR         AS AMOUNTBR")
        sqlStat.AppendLine("     , VL.AMOUNTORD        AS AMOUNTORD")
        sqlStat.AppendLine("     , VL.AMOUNTFIX        AS AMOUNTFIX")
        sqlStat.AppendLine("     , VL.AMOUNTPAY        AS AMOUNTPAY")
        sqlStat.AppendLine("     , VL.LOCALPAY         AS LOCALPAY")
        sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
        sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
        sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")
        '業者名
        sqlStat.AppendLine("     ,COALESCE(CMBR.NAMESEN,PTBR.AREANAME,DPBR.NAMES,TRBR.NAMES)   AS CONTRACTORNAMEBR ")
        sqlStat.AppendLine("     ,COALESCE(CMODR.NAMESEN,PTODR.AREANAME,DPODR.NAMES,TRODR.NAMES) AS CONTRACTORNAMEODR ")
        sqlStat.AppendLine("     ,COALESCE(CMFIX.NAMESEN,PTFIX.AREANAME,DPFIX.NAMES,TRFIX.NAMES) AS CONTRACTORNAMEFIX ")

        sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
        sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
        sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
        sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")
        sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
        sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
        sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
        sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
        sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
        sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
        sqlStat.AppendLine("     , VL.BRID           AS BRID")
        'sqlStat.AppendLine("     , VL.BRCOST         AS BRCOST")
        sqlStat.AppendLine("     , '1'         AS BRCOST") 'タンク動静の場合は削除させない
        sqlStat.AppendLine("     , ''                AS ACTYNO")
        sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")
        sqlStat.AppendLine("     , RIGHT(VL.DTLPOLPOD,1) + REPLACE(REPLACE(VL.DTLPOLPOD,'POL','000'),'POD','001') AS AGENTKBNSORT")
        sqlStat.AppendLine("     , CASE WHEN ISNULL(VL.DISPSEQ,'') = '' THEN '1' ")
        sqlStat.AppendLine("            ELSE '0' END AS DISPSEQISEMPTY")
        sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.AGENTPOL1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.AGENTPOL2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.AGENTPOD1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.AGENTPOD2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN OBS.AGENTORGANIZER")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN VL.AGENTORGANIZER")
        sqlStat.AppendLine("            ELSE '' END AS AGENT")

        sqlStat.AppendLine("     , OBS.TIP          AS TIP")
        sqlStat.AppendLine("     , OBS.DEMURTO      AS DEMURTO")
        sqlStat.AppendLine("     , OBS.DEMURUSRATE1 AS DEMURUSRATE1")
        sqlStat.AppendLine("     , OBS.DEMURUSRATE2 AS DEMURUSRATE2")
        sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'') AS CHARGE_CLASS4")
        sqlStat.AppendLine("     , CASE WHEN VL.INVOICEDBY = @JOTAGENT THEN 'on' ELSE '' END AS JOT")
        sqlStat.AppendLine("     , VL.DATEFIELD")
        sqlStat.AppendLine("     , AH.STATUS AS STATUSCODE")
        sqlStat.AppendLine("     , VL.BRADDEDCOST AS BRADDEDCOST")
        sqlStat.AppendLine("     , CASE WHEN APPLINGTANK.ORDERNO IS NULL THEN '1' ELSE '' END AS CAN_ENTRY_ACTUALDATE")
        sqlStat.AppendLine("     , ISNULL(TP.TANKFILLING,'') AS TANKFILLING")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OBS")
        sqlStat.AppendLine("    ON OBS.ORDERNO = VL.ORDERNO")
        sqlStat.AppendLine("   AND OBS.DELFLG    <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST")
        sqlStat.AppendLine("    ON CST.COMPCODE  = @COMPCODE")
        sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
        sqlStat.AppendLine("   AND '1' = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' AND CST.LDKBN IN ('B','L') THEN '1' ")
        sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'POD%' AND CST.LDKBN IN ('B','D') THEN '1' ")
        sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
        sqlStat.AppendLine("                  ELSE '1'")
        sqlStat.AppendLine("             END")
        sqlStat.AppendLine("   AND CST.STYMD     <= VL.STYMD")
        sqlStat.AppendLine("   AND CST.ENDYMD    >= VL.STYMD")
        sqlStat.AppendLine("   AND CST.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH") '承認履歴
        sqlStat.AppendLine("    ON  AH.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AH.APPLYID      = VL.APPLYID")
        sqlStat.AppendLine("   AND  AH.STEP         = VL.LASTSTEP")
        sqlStat.AppendLine("   AND  AH.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FV.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN AH.STATUS IS NOT NULL THEN AH.STATUS ")
        sqlStat.AppendLine("                               WHEN VL.AMOUNTFIX <> VL.AMOUNTORD THEN '" & C_APP_STATUS.APPAGAIN & "'")
        sqlStat.AppendLine("                               ELSE NULL")
        sqlStat.AppendLine("                           END")
        sqlStat.AppendLine("   AND  FV.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
        '*BR_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0002_PORT PTBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = PTBR.PORTCODE ")
        sqlStat.AppendLine("       AND  PTBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  PTBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  PTBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  PTBR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0001_COUNTRY CNTY")
        sqlStat.AppendLine("        ON CNTY.COUNTRYCODE   = VL.COUNTRYCODE")
        sqlStat.AppendLine("       AND CNTY.DELFLG       <> @DELFLG")
        sqlStat.AppendLine("       AND CNTY.STYMD        <= (case when VL.ACTUALDATE = '1900/01/01' then @NOWDATE else VL.ACTUALDATE end)")
        sqlStat.AppendLine("       AND CNTY.ENDYMD       >= (case when VL.ACTUALDATE = '1900/01/01' then @NOWDATE else VL.ACTUALDATE end)")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CMBR ")
        sqlStat.AppendLine("        ON CMBR.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND CMBR.CUSTOMERCODE  = VL.CONTRACTORBR ")
        sqlStat.AppendLine("       AND CST.CLASS4        >= '顧客' ")
        sqlStat.AppendLine("       AND CMBR.STYMD        <= VL.STYMD")
        sqlStat.AppendLine("       AND CMBR.ENDYMD       >= VL.STYMD")
        sqlStat.AppendLine("       AND CMBR.DELFLG       <> @DELFLG")
        '*BR_CONTRACTOR名取得JOIN END

        '*ODR_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0002_PORT PTODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = PTODR.PORTCODE ")
        sqlStat.AppendLine("       AND  PTODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  PTODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  PTODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  PTODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CMODR ")
        sqlStat.AppendLine("        ON CMODR.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND CMODR.CUSTOMERCODE  = VL.CONTRACTORODR ")
        sqlStat.AppendLine("       AND CST.CLASS4         >= '顧客' ")
        sqlStat.AppendLine("       AND CMODR.STYMD        <= VL.STYMD")
        sqlStat.AppendLine("       AND CMODR.ENDYMD       >= VL.STYMD")
        sqlStat.AppendLine("       AND CMODR.DELFLG       <> @DELFLG")
        '*ODR_CONTRACTOR名取得JOIN END

        '*FIX_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0002_PORT PTFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = PTFIX.PORTCODE ")
        sqlStat.AppendLine("       AND  PTFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  PTFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  PTFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  PTFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CMFIX ")
        sqlStat.AppendLine("        ON CMFIX.COMPCODE      = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND CMFIX.CUSTOMERCODE  = VL.CONTRACTORFIX ")
        sqlStat.AppendLine("       AND CST.CLASS4         >= '顧客' ")
        sqlStat.AppendLine("       AND CMFIX.STYMD        <= VL.STYMD")
        sqlStat.AppendLine("       AND CMFIX.ENDYMD       >= VL.STYMD")
        sqlStat.AppendLine("       AND CMFIX.DELFLG       <> @DELFLG")
        '*FIX_CONTRACTOR名取得JOIN END
        '*タンク利用申請中情報取得用(申請中・否認でタンクを変えていない日付をいじらせないレコードを返却）JOIN START
        sqlStat.AppendLine("      LEFT JOIN (SELECT DISTINCT OVL2.ORDERNO,OVL2.TANKSEQ")
        sqlStat.AppendLine("                   FROM GBT0007_ODR_VALUE2 OVL2")
        sqlStat.AppendLine("             INNER JOIN COT0002_APPROVALHIST AHTANK")
        sqlStat.AppendLine("                     ON OVL2.APPLYID     = AHTANK.APPLYID")
        sqlStat.AppendLine("                    AND OVL2.LASTSTEP    = AHTANK.STEP")
        sqlStat.AppendLine("                    AND AHTANK.COMPCODE  = @COMPCODE")
        sqlStat.AppendLine("                    AND AHTANK.STATUS   IN('" & C_APP_STATUS.APPLYING & "','" & C_APP_STATUS.REJECT & "')")
        sqlStat.AppendLine("                    AND AHTANK.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("                  WHERE OVL2.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                ) APPLINGTANK")
        sqlStat.AppendLine("             ON APPLINGTANK.ORDERNO = VL.ORDERNO")
        sqlStat.AppendLine("            AND APPLINGTANK.TANKSEQ = VL.TANKSEQ")
        '*タンク利用申請中情報取得用(申請中・否認でタンクを変えていない日付をいじらせないレコードを返却）JOIN END

        '*タンク充填状況及び例外的な顧客名取得 JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBT0001_BR_INFO BI")
        sqlStat.AppendLine("        ON  VL.BRID            = BI.BRID ")
        sqlStat.AppendLine("       AND  BI.SUBID          <> '' ")
        sqlStat.AppendLine("       AND  BI.TYPE            = 'INFO' ")
        sqlStat.AppendLine("       AND  BI.TYPE            = 'INFO' ")
        sqlStat.AppendLine("       AND  BI.DELFLG         <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0009_TRPATTERN TP")
        sqlStat.AppendLine("        ON  TP.COMPCODE        = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TP.ORG             = 'GB_Default' ")
        sqlStat.AppendLine("       AND  TP.BRTYPE          = BI.BRTYPE ")
        sqlStat.AppendLine("       AND  TP.USETYPE         = BI.USETYPE ")
        sqlStat.AppendLine("       AND  TP.ACTIONID        = VL.ACTIONID ")
        sqlStat.AppendLine("       AND  TP.CLASS2          = VL.DISPSEQ ")
        sqlStat.AppendLine("       AND  TP.DELFLG         <> @DELFLG")
        '*タンク充填状況及び例外的な顧客名取得 JOIN END

        '*速度改善
        sqlStat.AppendLine("      LEFT JOIN ( ")
        sqlStat.AppendLine("      SELECT DISTINCT ORDERNO ")
        sqlStat.AppendLine("      FROM   GBT0005_ODR_VALUE ODVALETA ")
        sqlStat.AppendLine("      WHERE ODVALETA.ACTIONID in " & etaActy & " ")
        sqlStat.AppendLine("       AND ODVALETA.DELFLG   <> @DELFLG ")
        sqlStat.AppendLine("       AND ((ODVALETA.SCHEDELDATE BETWEEN @ETAST AND @ETAEND) ")
        sqlStat.AppendLine("       OR   (ODVALETA.SCHEDELDATE = '1900/01/01' and ODVALETA.SCHEDELDATEBR BETWEEN @ETAST AND @ETAEND))")
        sqlStat.AppendLine("      ) ODVALETA_W ")
        sqlStat.AppendLine("      ON ODVALETA_W.ORDERNO   = VL.ORDERNO ")
        '

        sqlStat.AppendLine(" WHERE VL.DTLPOLPOD <> @DTLPOLPOD")
        sqlStat.AppendLine("   AND VL.COSTCODE  <> @COSTCODE")
        sqlStat.AppendLine("   AND VL.TANKNO    <> '' ")
        sqlStat.AppendLine("   AND VL.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("   AND EXISTS(SELECT 1 ") '基本情報が削除されていたら対象外
        sqlStat.AppendLine("                FROM GBT0004_ODR_BASE OBSS")
        sqlStat.AppendLine("               WHERE OBSS.ORDERNO = VL.ORDERNO")
        sqlStat.AppendLine("                 AND OBSS.DELFLG    <> @DELFLG)")
        If applyId <> "" Then
            sqlStat.AppendLine("   AND VL.APPLYID    = @APPLYID")
        End If

        '動的検索条件のSQL付与
        If sqlEtdEtaOrderCondition.Length > 0 Then
            sqlStat.AppendLine(sqlEtdEtaOrderCondition.ToString)
        End If

        If Me.hdnVender.Value <> "" Then
            'VENDER
            sqlStat.AppendLine("   AND (    (VL.CONTRACTORFIX   = @VENDER AND VL.ACTIONID <> '') ")
            sqlStat.AppendLine("         OR  VL.ACTIONID = '')")
        End If

        If Me.hdnTankNo.Value <> "" Then
            'TANK NO
            sqlStat.AppendLine("   AND VL.TANKNO   = @TANKNO")
        End If

        If Me.hdnActy.Value <> "" Then
            'ACTY
            sqlStat.AppendLine("   AND (    VL.ACTIONID   = @ACTY")
            sqlStat.AppendLine("         OR VL.ACTIONID   = '')")
        End If

        If Me.hdnOrderNo.Value <> "" Then
            'ORDER NO
            sqlStat.AppendLine("   AND VL.ORDERNO   = @ORDERNO")
        End If

        If Me.hdnOffice.Value <> "" Then
            'OFFICE
            'sqlStat.AppendLine("   AND (    OBS.AGENTORGANIZER = @OFFICECODE")
            'sqlStat.AppendLine("         OR OBS.AGENTPOL1      = @OFFICECODE")
            'sqlStat.AppendLine("         OR OBS.AGENTPOL2      = @OFFICECODE")
            'sqlStat.AppendLine("         OR OBS.AGENTPOD1      = @OFFICECODE")
            'sqlStat.AppendLine("         OR OBS.AGENTPOD2      = @OFFICECODE")
            'sqlStat.AppendLine("         OR VL.AGENTORGANIZER  = @OFFICECODE")
            'sqlStat.AppendLine("       )")
            sqlStat.AppendLine("   AND VL.DTLOFFICE  = @OFFICECODE")
        End If

        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY HIDDEN," & COA0020ProfViewSort.SORTSTR)
        Dim dtDbResult As New DataTable
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters

                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DTLPOLPOD", SqlDbType.NVarChar, 20).Value = "Organizer"
                .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                .Add("@COSTCODE", SqlDbType.NVarChar).Value = GBC_COSTCODE_DEMURRAGE
                If applyId <> "" Then
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                End If
                .Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT

                If Me.hdnETAStYMD.Value <> "" Then
                    .Add("@ETAST", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETAStYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    If Me.hdnETAEndYMD.Value <> "" Then
                        .Add("@ETAEND", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETAEndYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    Else
                        .Add("@ETAEND", SqlDbType.Date).Value = FormatDateYMD(Me.hdnETAStYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    End If
                End If
                If Me.hdnVender.Value <> "" Then
                    .Add("@VENDER", SqlDbType.NVarChar).Value = Me.hdnVender.Value
                End If
                If Me.hdnTankNo.Value <> "" Then
                    .Add("@TANKNO", SqlDbType.NVarChar).Value = Me.hdnTankNo.Value
                End If
                If Me.hdnActy.Value <> "" Then
                    .Add("@ACTY", SqlDbType.NVarChar).Value = Me.hdnActy.Value
                End If
                If Me.hdnOrderNo.Value <> "" Then
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = Me.hdnOrderNo.Value
                End If
                If Me.hdnOffice.Value <> "" Then
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.hdnOffice.Value
                End If
                .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using

        Dim retDt As DataTable = CreateOrderListTable()
        If dtDbResult Is Nothing OrElse dtDbResult.Rows Is Nothing OrElse dtDbResult.Rows.Count = 0 Then
            Return retDt
        End If
        Dim loopEnd As Integer = 1
        Integer.TryParse(Me.hdnCopy.Value, loopEnd)
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next
        Dim actyNo As Integer = 0
        Dim orderNo As String = Convert.ToString(dtDbResult.Rows(0).Item("ORDERNO"))
        Dim tankSeq As String = Convert.ToString(dtDbResult.Rows(0).Item("TANKSEQ"))
        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                If colName = "DISPSEQ" Then
                    writeDr.Item(colName) = Convert.ToString(readDr.Item(colName))
                Else
                    writeDr.Item(colName) = readDr.Item(colName)
                End If
            Next
            If Not (tankSeq.Equals(readDr.Item("TANKSEQ")) _
                    AndAlso orderNo.Equals(readDr.Item("ORDERNO"))) Then
                actyNo = 0
                orderNo = Convert.ToString(readDr.Item("ORDERNO"))
                tankSeq = Convert.ToString(readDr.Item("TANKSEQ"))
            End If
            actyNo = actyNo + 1
            writeDr.Item("ACTYNO") = actyNo.ToString("000")
            SetCanRowEdit(writeDr)
            retDt.Rows.Add(writeDr)
        Next

        Return retDt

    End Function
    ''' <summary>
    ''' オーダーNoタンク、タンクシーケンスをキーに表示順＋日付データを取得
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <param name="tankSeq"></param>
    ''' <param name="sqlCon"></param>
    ''' <returns></returns>
    ''' <remarks>日付項目が初期値のデータについては最大値に置換しているので注意</remarks>
    Private Function GetOrderValueDateSeq(orderNo As String, tankSeq As String, sqlCon As SqlConnection) As DataTable
        Dim dtDbResult As New DataTable
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT VL.DISPSEQ")
        sqlStat.AppendLine("      ,CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '2099/12/31' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
        sqlStat.AppendLine("      ,CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '2099/12/31' ELSE FORMAT(VL.SCHEDELDATE,'yyyy/MM/dd')   END AS SCHEDELDATE")
        sqlStat.AppendLine("      ,CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '2099/12/31' ELSE FORMAT(VL.ACTUALDATE,'yyyy/MM/dd')    END AS ACTUALDATE")
        sqlStat.AppendLine("      ,CONVERT(varchar(36),VL.DATAID)     AS DATAID")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlStat.AppendLine(" WHERE VL.ORDERNO = @ORDERNO")
        sqlStat.AppendLine("   AND VL.TANKSEQ = @TANKSEQ")
        sqlStat.AppendLine("   AND VL.BRADDEDCOST  = '0' ") '初期輸送パターン転送データのみ比較対象
        sqlStat.AppendLine("   AND VL.DTLPOLPOD    <> 'Organizer' ")
        sqlStat.AppendLine("   AND VL.DISPSEQ      <> '' ") '念の為DISPSEQが無いものは除外
        sqlStat.AppendLine("   AND VL.DELFLG  <> @DELFLG")
        sqlStat.AppendLine(" ORDER BY VL.DISPSEQ")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankSeq
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        Return dtDbResult
    End Function
    ''' <summary>
    ''' 引数のオーダー番号につき、Ship済のTANKNO,TANKSEQを返却
    ''' </summary>
    ''' <param name="orderNo">オーダーNo</param>
    ''' <returns>Ship済のタンクNo,タンクSeq</returns>
    Private Function GetOrderValueShipTanks(orderNo As String) As Dictionary(Of String, String)
        Dim retVal As New Dictionary(Of String, String)
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT TANKSEQ")
        sqlStat.AppendLine("     , TANKNO")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE ORDERNO      = @ORDERNO")
        sqlStat.AppendLine("   AND ACTIONID    IN ('SHIP','RPHC','RPHD')")
        sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND ACTUALDATE  <> @INITDATE")
        sqlStat.AppendLine(" GROUP BY TANKSEQ,TANKNO")

        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@INITDATE", SqlDbType.DateTime).Value = "1900/01/01"
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                Dim tmpDt As New DataTable
                sqlDa.Fill(tmpDt)
                If tmpDt IsNot Nothing AndAlso tmpDt.Rows.Count > 0 Then
                    retVal = tmpDt.AsEnumerable().ToDictionary(Of String, String)(Function(itm As DataRow) Convert.ToString(itm("TANKSEQ")), Function(itm As DataRow) Convert.ToString(itm("TANKNO")))
                End If
            End Using
        End Using
        Return retVal
    End Function
    ''' <summary>
    ''' オーダー(SOA)関連の各種テーブルより情報を取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>TODO:SQL差し替え</remarks>
    Private Function GetSOAListData(Optional applyId As String = "", Optional isBliingClose As Boolean = False, Optional isForRerpotAndDisp As Boolean = True) As DataTable
        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力

        Dim GBA00013SoaInfo As New GBA00013SoaInfo
        GBA00013SoaInfo.SORTMAPID = mapId
        GBA00013SoaInfo.SORTMAPVARIANT = Me.hdnListMapVariant.Value
        GBA00013SoaInfo.COUNTRYCODE = Me.hdnCountry.Value
        GBA00013SoaInfo.REPORTMONTH = FormatDateContrySettings(FormatDateYMD(Me.hdnReportMonth.Value, GBA00003UserSetting.DATEYMFORMAT), "yyyy/MM")
        GBA00013SoaInfo.ACTUALDATEFROM = FormatDateYMD(Me.hdnActualDateStYMD.Value, GBA00003UserSetting.DATEFORMAT)
        GBA00013SoaInfo.ACTUALDATETO = FormatDateYMD(Me.hdnActualDateEndYMD.Value, GBA00003UserSetting.DATEFORMAT)

        GBA00013SoaInfo.INVOICEDBYTYPE = Me.hdnInvoicedBy.Value
        If isBliingClose = False Then
            GBA00013SoaInfo.VENDER = Me.hdnVender.Value
            GBA00013SoaInfo.SOATYPE = Me.hdnAgentSoa.Value
            GBA00013SoaInfo.OFFICE = Me.hdnOffice.Value
        Else
            GBA00013SoaInfo.SHOULDGETALLCOST = "1"
        End If

        If isForRerpotAndDisp = False Then
            GBA00013SoaInfo.SOACLOSEPROC = "1"
        Else
            GBA00013SoaInfo.SOACLOSEPROC = ""
        End If

        GBA00013SoaInfo.GBA00013getSoaDataTable()
        If Not {C_MESSAGENO.NORMAL, C_MESSAGENO.NODATA}.Contains(GBA00013SoaInfo.ERR) Then
            Throw New Exception("GBA00013getSoaDataTable Error")
        End If
        Dim dtDbResult As DataTable = GBA00013SoaInfo.SOADATATABLE

        Dim retDt As DataTable = CreateOrderListTable()
        If dtDbResult Is Nothing OrElse dtDbResult.Rows Is Nothing OrElse dtDbResult.Rows.Count = 0 Then
            Return retDt
        End If
        Dim loopEnd As Integer = 1
        Integer.TryParse(Me.hdnCopy.Value, loopEnd)
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next
        Dim actyNo As Integer = 0
        Dim orderNo As String = Convert.ToString(dtDbResult.Rows(0).Item("ORDERNO"))
        Dim tankSeq As String = Convert.ToString(dtDbResult.Rows(0).Item("TANKSEQ"))
        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                If colName = "DISPSEQ" Then
                    writeDr.Item(colName) = Convert.ToString(readDr.Item(colName))
                Else
                    writeDr.Item(colName) = readDr.Item(colName)
                End If
            Next
            If Me.txtVender.Text <> "" AndAlso Not (readDr.Item("CONTRACTORFIX").Equals(Me.txtVender.Text)) Then
                writeDr.Item("HIDDEN") = "1"
            End If

            If Me.ckhShowTotalInvoiceRelatedCost.Checked = False AndAlso readDr.Item("BRADDEDCOST").Equals("2") Then
                writeDr.Item("HIDDEN") = "1"
            End If

            If Me.chkHideNoAmount.Checked = True AndAlso (readDr.Item("UAG_USD").ToString.Equals("0")) Then
                writeDr.Item("HIDDEN") = "1"
            End If

            If Not (tankSeq.Equals(readDr.Item("TANKSEQ")) _
                    AndAlso orderNo.Equals(readDr.Item("ORDERNO"))) Then
                actyNo = 0
                orderNo = Convert.ToString(readDr.Item("ORDERNO"))
                tankSeq = Convert.ToString(readDr.Item("TANKSEQ"))
            End If
            If Convert.ToString(readDr.Item("ISFUTUREMONTH")) = "1" Then
                writeDr.Item("ISAUTOCLOSE") = "-1"
            End If
            actyNo = actyNo + 1
            writeDr.Item("ACTYNO") = actyNo.ToString("000")
            SetCanRowEdit(writeDr)
            retDt.Rows.Add(writeDr)
        Next

        Return retDt

    End Function
    ''' <summary>
    ''' オーダー明細テーブルよりノンブレーカー一覧のデータを取得する
    ''' </summary>
    ''' <returns></returns>
    Private Function GetNonBrListData(Optional applyId As String = "") As DataTable
        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得

        'ここの処理は本来オーダーテーブルより取得を行う
        'Dim brNo As String = Me.hdnBrId.Value
        Dim copy As String = Me.hdnCopy.Value
        Dim textCostTblField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCostTblField = "NAMES"
        End If
        'ソート順取得
        COA0020ProfViewSort.MAPID = mapId
        COA0020ProfViewSort.VARI = Me.hdnListMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder()
        sqlStat.AppendLine("Select ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        sqlStat.AppendLine("      ,('SYS' + right('00000' + trim(convert(char,ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & "))), 5)) AS SYSKEY")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT '' AS OPERATION")
        sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
        sqlStat.AppendLine("     , '1' AS 'SELECT' ")
        sqlStat.AppendLine("     , '0' AS HIDDEN ")
        sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
        sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
        sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
        sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
        sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
        sqlStat.AppendFormat("     , ISNULL(CST.{0},'')   AS COSTNAME", textCostTblField).AppendLine()
        sqlStat.AppendLine("     , ISNULL(CST.CLASS1, '') AS CHARGE_CLASS1")
        sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
        sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
        sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")
        sqlStat.AppendLine("     , VL.COUNTRYCODE   AS COUNTRYCODE")
        sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
        sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
        sqlStat.AppendLine("     , CNTY.TAXRATE     AS TAXRATE")
        sqlStat.AppendLine("     , VL.AMOUNTORD     AS AMOUNTORD")
        sqlStat.AppendLine("     , VL.AMOUNTFIX     AS AMOUNTFIX")
        sqlStat.AppendLine("     , VL.AMOUNTPAY     AS AMOUNTPAY")
        sqlStat.AppendLine("     , VL.LOCALPAY      AS LOCALPAY")
        sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
        sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")

        '業者名
        sqlStat.AppendLine("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' THEN ISNULL(CUSODR.NAMESEN,'') ELSE COALESCE(DPODR.NAMES,TRODR.NAMES,'') END AS CONTRACTORNAMEODR ")
        sqlStat.AppendLine("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' THEN ISNULL(CUSFIX.NAMESEN,'') ELSE COALESCE(DPFIX.NAMES,TRFIX.NAMES,'') END AS CONTRACTORNAMEFIX ")

        sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
        sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
        sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")

        sqlStat.AppendLine("     , VL.LOCALRATE      AS LOCALRATE")
        sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
        sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
        sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
        sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
        sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
        sqlStat.AppendLine("     , VL.BRID           AS BRID")
        'sqlStat.AppendLine("     , VL.BRCOST         AS BRCOST")
        sqlStat.AppendLine("     , '0'         AS BRCOST") 'タンク動静の場合は削除させない
        sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")

        'sqlStat.AppendLine("     , CASE WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '') THEN AH.STATUS ")
        'sqlStat.AppendLine("                               WHEN CST.CLASS1 = '" & C_CHARGECODE.CLASS1.SALESCHAR & "' THEN '" & C_APP_STATUS.APPAGAIN & "'")
        'sqlStat.AppendLine("                               ELSE NULL")
        'sqlStat.AppendLine("                           END As TEST")
        sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENT")

        sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'') AS CHARGE_CLASS4")
        sqlStat.AppendLine("     , CASE WHEN VL.INVOICEDBY = @JOTAGENT THEN 'on' ELSE '' END AS JOT")
        sqlStat.AppendLine("     , VL.DATEFIELD")
        sqlStat.AppendLine("     , AH.STATUS AS STATUSCODE")
        sqlStat.AppendLine("     , VL.ACCCURRENCYSEGMENT")
        'sqlStat.AppendLine("     , CASE WHEN (CST.CRGENERALPURPOSE = '1' OR CST.DBGENERALPURPOSE = '1') THEN '1' ELSE '0' END AS ENABLEACCCURRENCYSEGMENT")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST")
        sqlStat.AppendLine("    On CST.COMPCODE  = @COMPCODE")
        sqlStat.AppendLine("   AND CST.NONBR     = '" & CONST_FLAG_YES & "'")
        sqlStat.AppendLine("   AND CST.COSTCODE  = VL.COSTCODE")
        sqlStat.AppendLine("   AND CST.STYMD     <= VL.STYMD")
        sqlStat.AppendLine("   AND CST.ENDYMD    >= VL.STYMD")
        sqlStat.AppendLine("   AND CST.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH") '承認履歴
        sqlStat.AppendLine("    On  AH.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   And  AH.APPLYID      = VL.APPLYID")
        sqlStat.AppendLine("   And  AH.STEP         = VL.LASTSTEP")
        sqlStat.AppendLine("   And  AH.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV") 'STATUS用JOIN
        sqlStat.AppendLine("    On  FV.CLASS        = 'APPROVAL'")
        'sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '') THEN AH.STATUS ")
        'sqlStat.AppendLine("                               WHEN CST.NONBR = '" & CONST_FLAG_YES & "' AND CST.CLASS2 <> '' THEN '" & C_APP_STATUS.APPAGAIN & "'")
        'sqlStat.AppendLine("                               ELSE NULL")
        'sqlStat.AppendLine("                           END")
        sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '' AND AH.STATUS <> '" & C_APP_STATUS.APPROVED & "') THEN AH.STATUS ")
        sqlStat.AppendLine("                               WHEN (AH.STATUS = '" & C_APP_STATUS.APPROVED & "' AND VL.AMOUNTFIX <> VL.AMOUNTORD) THEN '" & C_APP_STATUS.APPAGAIN & "'")
        sqlStat.AppendLine("                               WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '') THEN AH.STATUS ")
        sqlStat.AppendLine("                               WHEN CST.NONBR = '" & CONST_FLAG_YES & "' AND CST.CLASS2 <> '' THEN '" & C_APP_STATUS.APPAGAIN & "'")
        sqlStat.AppendLine("                               ELSE NULL")
        sqlStat.AppendLine("                           END")
        sqlStat.AppendLine("   AND  FV.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRD")
        sqlStat.AppendLine("    ON  TRD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  TRD.CARRIERCODE  = VL.DTLOFFICE")
        sqlStat.AppendLine("   AND  TRD.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("   AND  TRD.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("   AND  TRD.DELFLG      <> @DELFLG")

        '*ODR_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = CUSODR.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CUSODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CUSODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  CUSODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  CUSODR.DELFLG      <> @DELFLG")
        '*ODR_CONTRACTOR名取得JOIN END

        '*FIX_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CUSFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = CUSFIX.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CUSFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CUSFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  CUSFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  CUSFIX.DELFLG      <> @DELFLG")
        '*FIX_CONTRACTOR名取得JOIN END
        '*国マスタ JOIN START
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CNTY")
        sqlStat.AppendLine("         ON CNTY.COUNTRYCODE      = VL.COUNTRYCODE")
        sqlStat.AppendLine("        AND CNTY.DELFLG           <> @DELFLG")
        sqlStat.AppendLine("        AND CNTY.STYMD            <= (case when VL.ACTUALDATE = '1900/01/01' then @NOWDATE else VL.ACTUALDATE end)")
        sqlStat.AppendLine("        AND CNTY.ENDYMD           >= (case when VL.ACTUALDATE = '1900/01/01' then @NOWDATE else VL.ACTUALDATE end)")
        '*国マスタ JOIN END
        sqlStat.AppendLine("WHERE VL.DELFLG     <> @DELFLG ")
        sqlStat.AppendLine("  AND VL.ORDERNO  LIKE 'NB%' ")
        sqlStat.AppendLine("  AND VL.BRID        = '' ")
        If hdnDateTermStYMD.Value <> "" AndAlso hdnDateTermEndYMD.Value <> "" Then
            sqlStat.AppendLine("  AND VL.ACTUALDATE  BETWEEN @ACTUALDATEFROM AND @ACTUALDATETO")
        End If
        If hdnApproval.Value <> "" Then
            sqlStat.AppendLine("  AND FV.KEYCODE     = @APPROVAL ")
        End If
        If applyId <> "" Then
            sqlStat.AppendLine("  AND VL.APPLYID        = @APPLYID ")
        End If
        If Me.hdnOffice.Value <> "" Then
            'OFFICE
            sqlStat.AppendLine("  AND VL.AGENTORGANIZER = @OFFICECODE ")
        End If

        'TODO 精算・未清算条件

        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        Dim dtDbResult As New DataTable
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                If hdnDateTermStYMD.Value <> "" AndAlso hdnDateTermEndYMD.Value <> "" Then
                    .Add("@ACTUALDATEFROM", SqlDbType.Date).Value = FormatDateYMD(Me.hdnDateTermStYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    .Add("@ACTUALDATETO", SqlDbType.Date).Value = FormatDateYMD(Me.hdnDateTermEndYMD.Value, GBA00003UserSetting.DATEFORMAT)
                End If
                If hdnApproval.Value <> "" Then
                    .Add("@APPROVAL", SqlDbType.NVarChar).Value = Me.hdnApproval.Value
                End If
                If applyId <> "" Then
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                End If
                If Me.hdnOffice.Value <> "" Then
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.hdnOffice.Value
                End If
                .Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT
                .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using

        Dim retDt As DataTable = CreateOrderListTable()
        If dtDbResult Is Nothing OrElse dtDbResult.Rows Is Nothing OrElse dtDbResult.Rows.Count = 0 Then
            Return retDt
        End If
        Dim loopEnd As Integer = 1
        Integer.TryParse(Me.hdnCopy.Value, loopEnd)
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next

        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                If colName = "DISPSEQ" Then
                    writeDr.Item(colName) = Convert.ToString(readDr.Item(colName))
                Else
                    writeDr.Item(colName) = readDr.Item(colName)
                End If
            Next
            SetCanRowEdit(writeDr)
            retDt.Rows.Add(writeDr)
        Next

        Return retDt

    End Function
    ''' <summary>
    ''' オーダー明細テーブルよりディマレッジ対象一覧のデータを取得する
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDemurrageListData(Optional applyId As String = "") As DataTable

        Dim mapId As String = CONST_MAPID
        Dim COA0003LogFile As New BASEDLL.COA0003LogFile              'ログ出力
        Dim COA0020ProfViewSort As New BASEDLL.COA0020ProfViewSort    'テーブルソート文字列取得
        Dim textCustomerTblField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCustomerTblField = "NAMESEN"
        End If
        'ここの処理は本来オーダーテーブルより取得を行う
        'Dim brNo As String = Me.hdnBrId.Value
        Dim copy As String = Me.hdnCopy.Value
        Dim textCostTblField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textCostTblField = "NAMES"
        End If
        'ソート順取得
        COA0020ProfViewSort.MAPID = mapId
        COA0020ProfViewSort.VARI = Me.hdnListMapVariant.Value
        COA0020ProfViewSort.TAB = ""
        COA0020ProfViewSort.COA0020getProfViewSort()

        Dim sqlStat As New StringBuilder()
        sqlStat.AppendLine(" WITH W_JOTAGENT AS (") 'START 
        sqlStat.AppendLine("   SELECT TR.CARRIERCODE")
        sqlStat.AppendLine("     FROM GBM0005_TRADER TR")
        sqlStat.AppendLine("    WHERE TR.STYMD  <= @NOWDATE")
        sqlStat.AppendLine("      AND TR.ENDYMD >= @NOWDATE")
        sqlStat.AppendLine("      AND TR.DELFLG <> @DELFLG")
        sqlStat.AppendLine("      AND EXISTS (SELECT 1")
        sqlStat.AppendLine("                    FROM COS0017_FIXVALUE FXV")
        sqlStat.AppendLine("                   WHERE FXV.COMPCODE   = 'Default'")
        sqlStat.AppendLine("                     AND FXV.SYSCODE    = 'GB'")
        sqlStat.AppendLine("                     AND FXV.CLASS      = 'JOTCOUNTRYORG'")
        sqlStat.AppendLine("                     AND FXV.KEYCODE     = TR.MORG")
        sqlStat.AppendLine("                     AND FXV.STYMD     <= @NOWDATE")
        sqlStat.AppendLine("                     AND FXV.ENDYMD    >= @NOWDATE")
        sqlStat.AppendLine("                     AND FXV.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("                 )")
        sqlStat.AppendLine(")")
        sqlStat.AppendLine("SELECT ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & ") As LINECNT")
        sqlStat.AppendLine("      ,TBL.* ")
        sqlStat.AppendLine("      ,''  AS DELETEFLAG ")
        sqlStat.AppendLine("      ,('SYS' + right('00000' + trim(convert(char,ROW_NUMBER() OVER(ORDER BY " & COA0020ProfViewSort.SORTSTR & "))), 5)) AS SYSKEY")
        sqlStat.AppendLine("      ,CASE WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.DOWN & "' THEN FLOOR(TBL.REFAMOUNT_BS * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
        sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.UP & "' THEN CEILING(  TBL.REFAMOUNT_BS * POWER(10,TBL.DECIMALPLACES)) / POWER(10,TBL.DECIMALPLACES) ")
        sqlStat.AppendLine("            WHEN TBL.ROUNDFLG = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND(  TBL.REFAMOUNT_BS,TBL.DECIMALPLACES * 1) ")
        sqlStat.AppendLine("            ELSE TBL.REFAMOUNT_BS END AS REFAMOUNT ")
        sqlStat.AppendLine("      ,CASE WHEN TBL.AMOUNTORD <> TBL.AMOUNTFIX THEN '0' ELSE '1' END AS CANMODIFYREPORTMONTH") '請求月更新可能フラグ
        sqlStat.AppendLine("      ,DEMREPORTMONTH AS REPORTYMDORG")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("SELECT '' AS OPERATION")
        sqlStat.AppendLine("     , TIMSTP = cast(VL.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
        sqlStat.AppendLine("     , '1' AS 'SELECT' ")
        sqlStat.AppendLine("     , '0' AS HIDDEN ")
        sqlStat.AppendLine("     , CONVERT(varchar(36),VL.DATAID)     AS DATAID")
        sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
        sqlStat.AppendLine("     , VL.DTLOFFICE  AS DTLOFFICE ")
        sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
        sqlStat.AppendLine("     , VL.TANKSEQ     AS TANKSEQ ")
        sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
        sqlStat.AppendFormat("     , ISNULL(CST.{0},'')   AS COSTNAME", textCostTblField).AppendLine()
        sqlStat.AppendLine("     , ISNULL(CST.CLASS1, '') AS CHARGE_CLASS1")
        sqlStat.AppendLine("     , CASE WHEN VL.DISPSEQ = '' THEN null ELSE CONVERT(INT,VL.DISPSEQ) END      AS DISPSEQ")
        sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
        sqlStat.AppendLine("     , VL.ORIGINDESTINATION AS ORIGINDESTINATION")
        sqlStat.AppendLine("     , VL.AMOUNTBR     AS AMOUNTBR")
        sqlStat.AppendLine("     , VL.AMOUNTORD     AS AMOUNTORD")
        sqlStat.AppendLine("     , VL.AMOUNTFIX     AS AMOUNTFIX")
        sqlStat.AppendLine("     , VL.AMOUNTPAY     AS AMOUNTPAY")
        sqlStat.AppendLine("     , VL.LOCALPAY     AS LOCALPAY")

        '業者コード
        sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
        sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
        sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")
        '業者名
        sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTBR.{0} IS NOT NULL)  THEN ISNULL(CTBR.{0},'')  ELSE COALESCE(DPBR.NAMES,TRBR.NAMES,'')   END AS CONTRACTORNAMEBR ", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTODR.{0} IS NOT NULL) THEN ISNULL(CTODR.{0},'') ELSE COALESCE(DPODR.NAMES,TRODR.NAMES,'') END AS CONTRACTORNAMEODR ", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("    ,CASE WHEN ISNULL(CST.CLASS2,'') <> '' OR (VL.DTLPOLPOD = 'Organizer' AND CTFIX.{0} IS NOT NULL) THEN ISNULL(CTFIX.{0},'') ELSE COALESCE(DPFIX.NAMES,TRFIX.NAMES,'') END AS CONTRACTORNAMEFIX ", textCustomerTblField).AppendLine()

        sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
        sqlStat.AppendLine("     , VL.LOCALRATE  AS LOCALRATE")
        sqlStat.AppendLine("     , CASE WHEN VL.TAXATION = '1' THEN 'on' ELSE '' END AS TAXATION")
        sqlStat.AppendLine("     , CNTY.TAXRATE   AS TAXRATE")
        sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,  'yyyy/MM/dd') END AS SCHEDELDATEBR")
        sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
        sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
        sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")

        sqlStat.AppendLine("     , VL.INVOICEDBY     AS INVOICEDBY")
        sqlStat.AppendLine("     , VL.APPLYID        AS APPLYID")
        sqlStat.AppendLine("     , VL.APPLYTEXT      AS APPLYTEXT")
        sqlStat.AppendLine("     , VL.LASTSTEP       AS LASTSTEP")
        sqlStat.AppendLine("     , CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(FV.VALUE1,'') WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(FV.VALUE2,'') END AS STATUS")
        sqlStat.AppendLine("     , VL.BRID           AS BRID")
        'sqlStat.AppendLine("     , VL.BRCOST         AS BRCOST")
        sqlStat.AppendLine("     , '0'         AS BRCOST") 'タンク動静の場合は削除させない
        sqlStat.AppendLine("     , VL.AGENTORGANIZER AS AGENTORGANIZER")

        sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.AGENTPOL1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.AGENTPOL2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.AGENTPOD1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.AGENTPOD2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN OBS.AGENTORGANIZER")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN VL.AGENTORGANIZER")
        sqlStat.AppendLine("            ELSE '' END AS ORGOFFICE")

        sqlStat.AppendLine("     , CASE WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.AGENTPOD1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.AGENTPOD2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.AGENTPOL1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.AGENTPOL2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN OBS.AGENTORGANIZER")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN VL.AGENTORGANIZER")
        sqlStat.AppendLine("            ELSE '' END AS OTHEROFFICE")
        sqlStat.AppendLine("     , CASE WHEN VL.COUNTRYCODE <> ''       THEN VL.COUNTRYCODE")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL1'      THEN OBS.LOADCOUNTRY1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POL2'      THEN OBS.LOADCOUNTRY2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD1'      THEN OBS.DISCHARGECOUNTRY1")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'POD2'      THEN OBS.DISCHARGECOUNTRY2")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = 'Organizer' THEN ''")
        sqlStat.AppendLine("            WHEN VL.DTLPOLPOD = ''          THEN ''")
        sqlStat.AppendLine("            ELSE '' END      AS COUNTRYCODE")
        sqlStat.AppendLine("     , ISNULL(CST.CLASS4,'') AS CHARGE_CLASS4")
        sqlStat.AppendLine("      ,ISNULL(USREXR.EXRATE,'') AS EXRATE")
        sqlStat.AppendLine("      ,CASE WHEN USREXR.EXRATE IS NULL OR USREXR.EXRATE = 0 THEN '' ")
        sqlStat.AppendLine("            WHEN VL.CURRENCYCODE = '" & GBC_CUR_USD & "' THEN VL.AMOUNTFIX * USREXR.EXRATE") 'ドル換算の場合はローカル
        sqlStat.AppendLine("            ELSE VL.AMOUNTFIX / USREXR.EXRATE") 'ローカル換算の場合はドル
        sqlStat.AppendLine("        END AS REFAMOUNT_BS")
        sqlStat.AppendLine("     , (VL.AMOUNTFIX * 0.1)     AS COMMAMOUNT") 'TODO FIXVALUE
        sqlStat.AppendLine("     , CASE WHEN VL.INVOICEDBY = @JOTAGENT THEN 'on' ELSE '' END AS JOT")
        sqlStat.AppendLine("     , VL.DTLPOLPOD AS DTLPOLPOD")
        sqlStat.AppendLine("     , VL.DATEFIELD")
        sqlStat.AppendLine("     , CASE WHEN VL.ACTUALDATE = '1900/01/01' THEN '' ")
        sqlStat.AppendLine("            ELSE FORMAT(VL.ACTUALDATE,'yyyy/MM') END     AS DEMREPORTMONTH")
        'sqlStat.AppendLine("     , FORMAT(DATEADD(month,1,CLD.BILLINGYMD),'yyyy/MM') AS CLOSINGMONTH")
        sqlStat.AppendLine("     , FORMAT(DATEADD(month,1,DATEADD(day,-1,CLD.BILLINGYMD)),'yyyy/MM') AS CLOSINGMONTH")
        sqlStat.AppendLine("     , AH.STATUS AS STATUSCODE")
        sqlStat.AppendLine("     , CNTY.ROUNDFLG      AS ROUNDFLG")
        sqlStat.AppendLine("     , CNTY.DECIMALPLACES AS DECIMALPLACES")
        sqlStat.AppendLine("     , CASE WHEN JOTSOAVL.DATAIDODR IS NULL THEN '0' ELSE '1' END AS IS_BILLINGCLOSED")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")

        sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OBS")
        sqlStat.AppendLine("    ON OBS.ORDERNO = VL.ORDERNO")
        sqlStat.AppendLine("   AND OBS.DELFLG    <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CST")
        sqlStat.AppendLine("    On CST.COMPCODE  = @COMPCODE")
        sqlStat.AppendLine("   And CST.COSTCODE  = VL.COSTCODE")
        sqlStat.AppendLine("   AND '1' = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' AND CST.LDKBN IN ('B','L') THEN '1' ")
        sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'POD%' AND CST.LDKBN IN ('B','D') THEN '1' ")
        sqlStat.AppendLine("                  WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
        sqlStat.AppendLine("                  ELSE '1'")
        sqlStat.AppendLine("             END")
        sqlStat.AppendLine("   And CST.STYMD     <= VL.STYMD")
        sqlStat.AppendLine("   And CST.ENDYMD    >= VL.STYMD")
        sqlStat.AppendLine("   And CST.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH") '承認履歴
        sqlStat.AppendLine("    On  AH.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   And  AH.APPLYID      = VL.APPLYID")
        sqlStat.AppendLine("   And  AH.STEP         = VL.LASTSTEP")
        sqlStat.AppendLine("   And  AH.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV") 'STATUS用JOIN
        sqlStat.AppendLine("    On  FV.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN (AH.STATUS IS NOT NULL AND AH.STATUS <> '') THEN AH.STATUS ")
        sqlStat.AppendLine("                               WHEN VL.AMOUNTBR <> VL.AMOUNTORD THEN '" & C_APP_STATUS.APPAGAIN & "'")
        sqlStat.AppendLine("                               ELSE NULL")
        'sqlStat.AppendLine("                               ELSE '" & C_APP_STATUS.APPAGAIN & "'") 'DEMURRAGEの場合はすべて申請が必要な想定
        sqlStat.AppendLine("                           END")
        sqlStat.AppendLine("   AND  FV.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CNTY")
        sqlStat.AppendLine("         ON CNTY.COUNTRYCODE      = VL.COUNTRYCODE")
        sqlStat.AppendLine("        AND CNTY.DELFLG           <> @DELFLG")
        sqlStat.AppendLine("        AND CNTY.STYMD            <= (case when VL.ACTUALDATE = '1900/01/01' then @NOWDATE else VL.ACTUALDATE end)")
        sqlStat.AppendLine("        AND CNTY.ENDYMD           >= (case when VL.ACTUALDATE = '1900/01/01' then @NOWDATE else VL.ACTUALDATE end)")

        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE USREXR")
        sqlStat.AppendLine("         ON USREXR.COMPCODE      = @COMPCODE")
        sqlStat.AppendLine("        AND USREXR.CURRENCYCODE  = CNTY.CURRENCYCODE")
        sqlStat.AppendLine("        AND USREXR.TARGETYM      = CONVERT(date,DATEADD(DAY, 1-DATEPART(DAY,  getdate()),  getdate()))")
        sqlStat.AppendLine("        AND USREXR.DELFLG       <> @DELFLG")
        'InvoicedByとTraderをJOINしInvoicedByの国を取得 SOA締め日取得用の国 START
        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER INVTRA")
        sqlStat.AppendLine("         ON INVTRA.COMPCODE      = @COMPCODE")
        sqlStat.AppendLine("        AND INVTRA.CARRIERCODE   = VL.INVOICEDBY")
        sqlStat.AppendLine("        AND INVTRA.STYMD        <= VL.STYMD")
        sqlStat.AppendLine("        AND INVTRA.ENDYMD       >= VL.STYMD")
        sqlStat.AppendLine("        AND INVTRA.DELFLG       <> @DELFLG")
        'InvoicedByの国コードを取得 END

        'SOA締め日JOIN START
        sqlStat.AppendLine("  LEFT JOIN GBT0006_CLOSINGDAY CLD")
        'sqlStat.AppendLine("         ON CLD.COUNTRYCODE  = VL.COUNTRYCODE")
        sqlStat.AppendLine("         ON CLD.COUNTRYCODE  = CASE WHEN VL.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE INVTRA.COUNTRYCODE END")
        'CASE WHEN TBLSUB.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE @COUNTRY END
        sqlStat.AppendLine("        AND CLD.DELFLG       <> @DELFLG")
        sqlStat.AppendLine("        AND CLD.REPORTMONTH  = (SELECT MAX(CLDS.REPORTMONTH)")
        sqlStat.AppendLine("                                  FROM GBT0006_CLOSINGDAY CLDS")
        sqlStat.AppendLine("                                 WHERE CLDS.COUNTRYCODE = CASE WHEN VL.INVOICEDBY IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) THEN '" & GBC_JOT_SOA_COUNTRY & "' ELSE INVTRA.COUNTRYCODE END")
        sqlStat.AppendLine("                                   AND CLDS.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("                                )")
        'SOA締め日JOIN END
        '*BR_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = TRBR.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRBR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = DPBR.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPBR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CTBR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORBR = CTBR.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CTBR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CTBR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  CTBR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  CTBR.DELFLG      <> @DELFLG")
        '*BR_CONTRACTOR名取得JOIN END

        '*ODR_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = TRODR.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = DPODR.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPODR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CTODR")
        sqlStat.AppendLine("        ON  VL.CONTRACTORODR = CTODR.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CTODR.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CTODR.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  CTODR.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  CTODR.DELFLG      <> @DELFLG")
        '*ODR_CONTRACTOR名取得JOIN END

        '*FIX_CONTRACTOR名取得JOIN START
        sqlStat.AppendLine("      LEFT JOIN GBM0005_TRADER TRFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = TRFIX.CARRIERCODE ")
        sqlStat.AppendLine("       AND  TRFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  TRFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  TRFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0003_DEPOT DPFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = DPFIX.DEPOTCODE ")
        sqlStat.AppendLine("       AND  DPFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  DPFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  DPFIX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("      LEFT JOIN GBM0004_CUSTOMER CTFIX")
        sqlStat.AppendLine("        ON  VL.CONTRACTORFIX = CTFIX.CUSTOMERCODE ")
        sqlStat.AppendLine("       AND  CTFIX.COMPCODE     = '" & GBC_COMPCODE & "' ")
        sqlStat.AppendLine("       AND  CTFIX.STYMD       <= VL.STYMD")
        sqlStat.AppendLine("       AND  CTFIX.ENDYMD      >= VL.STYMD")
        sqlStat.AppendLine("       AND  CTFIX.DELFLG      <> @DELFLG")
        '*FIX_CONTRACTOR名取得JOIN END
        'SOACLOSE連動済確認JOIN START
        sqlStat.AppendLine("  LEFT JOIN (SELECT DISTINCT JOTSOAVLS.REPORTMONTH,JOTSOAVLS.DATAIDODR FROM GBT0008_JOTSOA_VALUE JOTSOAVLS  with(nolock)")
        sqlStat.AppendLine("        WHERE JOTSOAVLS.SOAAPPDATE   <> @INITSOAAPDATE")
        sqlStat.AppendLine("          AND JOTSOAVLS.COSTCODE      = @COSTCODE")
        sqlStat.AppendLine("          AND JOTSOAVLS.CLOSINGMONTH  = JOTSOAVLS.REPORTMONTH")
        sqlStat.AppendLine("          AND JOTSOAVLS.DELFLG       <> @DELFLG")
        sqlStat.AppendLine("             ) JOTSOAVL")
        sqlStat.AppendLine("    ON JOTSOAVL.DATAIDODR   = VL.DATAID")

        'SOACLOSE連動済確認JOIN END
        sqlStat.AppendLine("WHERE VL.DELFLG        <> @DELFLG ")
        sqlStat.AppendLine("  AND VL.BRID          <> '' ")
        'sqlStat.AppendLine("  AND VL.DTLPOLPOD      = @DTLPOLPOD")
        sqlStat.AppendLine("  AND VL.COSTCODE       = @COSTCODE")
        sqlStat.AppendLine("  AND VL.SCHEDELDATEBR  > @SCHEDELDATEBR") '一旦MINVALUEはイコールで含める(本来「以上」ではなく「超える」）
        sqlStat.AppendLine("  AND VL.AMOUNTBR  <> 0")
        'sqlStat.AppendLine("  AND VL.TANKNO        <> ''")
        '日付条件
        If Me.hdnDateTermStYMD.Value <> "" AndAlso Me.hdnDateTermEndYMD.Value <> "" Then
            sqlStat.AppendLine("  AND VL.ACTUALDATE  BETWEEN @ACTUALDATEFROM AND @ACTUALDATETO")
        End If
        '申請ステータス
        If Me.hdnApproval.Value <> "" Then
            sqlStat.AppendLine("  AND FV.KEYCODE     = @APPROVAL ")
        End If
        'Shipper
        If Me.hdnShipper.Value <> "" Then
            sqlStat.AppendLine("   AND OBS.SHIPPER = @SHIPPER")
        End If
        'Consignee
        If Me.hdnConsignee.Value <> "" Then
            sqlStat.AppendLine("   AND OBS.CONSIGNEE = @CONSIGNEE")
        End If
        'タンクNo
        If Me.hdnTankNo.Value <> "" Then
            sqlStat.AppendLine("  AND VL.TANKNO     = @TANKNO ")
        End If
        '申請ID
        If applyId <> "" Then
            sqlStat.AppendLine("  AND VL.APPLYID     = @APPLYID ")
        End If
        If Me.hdnOffice.Value <> "" Then
            'OFFICE
            sqlStat.AppendLine("   AND VL.INVOICEDBY  = @OFFICECODE")
        End If

        'TODO 精算・未清算条件

        sqlStat.AppendLine("   ) TBL")
        sqlStat.AppendLine(" ORDER BY " & COA0020ProfViewSort.SORTSTR)
        Dim dtDbResult As New DataTable
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                .Add("@SCHEDELDATEBR", SqlDbType.Date).Value = "1900/01/01"
                .Add("@INITSOAAPDATE", SqlDbType.Date).Value = "1900/01/01"
                If hdnDateTermStYMD.Value <> "" AndAlso hdnDateTermEndYMD.Value <> "" Then
                    .Add("@ACTUALDATEFROM", SqlDbType.Date).Value = FormatDateYMD(Me.hdnDateTermStYMD.Value, GBA00003UserSetting.DATEFORMAT)
                    .Add("@ACTUALDATETO", SqlDbType.Date).Value = FormatDateYMD(Me.hdnDateTermEndYMD.Value, GBA00003UserSetting.DATEFORMAT)
                End If
                If hdnApproval.Value <> "" Then
                    .Add("@APPROVAL", SqlDbType.NVarChar).Value = Me.hdnApproval.Value
                End If
                If Me.hdnTankNo.Value <> "" Then
                    .Add("@TANKNO", SqlDbType.NVarChar).Value = Me.hdnTankNo.Value
                End If
                If Me.hdnShipper.Value <> "" Then
                    .Add("@SHIPPER", SqlDbType.NVarChar).Value = Me.hdnShipper.Value
                End If
                If Me.hdnConsignee.Value <> "" Then
                    .Add("@CONSIGNEE", SqlDbType.NVarChar).Value = Me.hdnConsignee.Value
                End If
                ' .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = "Organizer"
                .Add("@COSTCODE", SqlDbType.NVarChar).Value = GBC_COSTCODE_DEMURRAGE
                If applyId <> "" Then
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                End If
                '.Add("@USERCURRENCY", SqlDbType.NVarChar).Value = Me.hdnUserCurrency.Value
                .Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT
                If Me.hdnOffice.Value <> "" Then
                    .Add("@OFFICECODE", SqlDbType.NVarChar).Value = Me.hdnOffice.Value
                End If
                .Add("@NOWDATE", SqlDbType.Date).Value = Now
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using

        Dim retDt As DataTable = CreateOrderListTable()
        If dtDbResult Is Nothing OrElse dtDbResult.Rows Is Nothing OrElse dtDbResult.Rows.Count = 0 Then
            Return retDt
        End If
        Dim loopEnd As Integer = 1
        Integer.TryParse(Me.hdnCopy.Value, loopEnd)
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next

        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                If colName = "DISPSEQ" Then
                    writeDr.Item(colName) = Convert.ToString(readDr.Item(colName))
                Else
                    writeDr.Item(colName) = readDr.Item(colName)
                End If
            Next
            SetCanRowEdit(writeDr)
            retDt.Rows.Add(writeDr)
        Next

        Return retDt

    End Function
    ''' <summary>
    ''' 月締め日を取得
    ''' </summary>
    ''' <param name="countryCode"></param>
    ''' <returns></returns>
    Private Function GetClosingDate(countryCode As String) As String
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT FORMAT(DATEADD(month,1,DATEADD(day,-1,CD.BILLINGYMD)),'yyyy/MM')")
        sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CD")
        sqlStat.AppendLine(" WHERE CD.COUNTRYCODE = @COUNTRYCODE ")
        sqlStat.AppendLine("   AND CD.DELFLG <> @DELFLG")
        sqlStat.AppendLine("   AND CD.REPORTMONTH = (SELECT MAX(CDS.REPORTMONTH)")
        sqlStat.AppendLine("                           FROM GBT0006_CLOSINGDAY CDS")
        sqlStat.AppendLine("                          WHERE CDS.COUNTRYCODE = @COUNTRYCODE")
        sqlStat.AppendLine("                            AND CDS.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("                        )")

        Dim dtDbResult As New DataTable
        Dim retStr As String = ""
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using 'sqlDa
        End Using 'sqlCon sqlCmd
        If dtDbResult IsNot Nothing AndAlso dtDbResult.Rows.Count > 0 Then
            With dtDbResult.Rows(0)
                retStr = Convert.ToString(.Item(0))
            End With
        End If
        Return retStr
    End Function
    ''' <summary>
    ''' ブレーカータイプ取得
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <returns></returns>
    Private Function GetBrType(orderNo As String) As String
        If orderNo = "" Then
            Return ""
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT TRIM(BRTYPE) AS BRTYPE")
        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO ")
        sqlStat.AppendLine("   AND DELFLG <> @DELFLG")

        Dim dtDbResult As New DataTable
        Dim retStr As String = ""
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using 'sqlDa
        End Using 'sqlCon sqlCmd
        If dtDbResult IsNot Nothing AndAlso dtDbResult.Rows.Count > 0 Then
            With dtDbResult.Rows(0)
                retStr = Convert.ToString(.Item(0))
            End With
        End If
        Return retStr
    End Function
    ''' <summary>
    ''' オーダーBase情報の取得
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <returns></returns>
    ''' <remarks>タンク引当（タンク一覧画面遷移時）に利用</remarks>
    Private Function GetOrderBaseDt(orderNo As String) As DataTable
        'ありえないがオーダーNoが空白
        If orderNo = "" Then
            Return Nothing
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT TRIM(BS.BRTYPE) AS BRTYPE")
        sqlStat.AppendLine("      ,BS.SHIPPER")
        sqlStat.AppendLine("      ,BS.PRODUCTCODE")
        sqlStat.AppendLine("      ,CASE WHEN EXISTS(SELECT 1")
        sqlStat.AppendLine("                          FROM GBT0005_ODR_VALUE SVL")
        sqlStat.AppendLine("                         WHERE SVL.ORDERNO   = BS.ORDERNO")
        sqlStat.AppendLine("                           AND SVL.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("                           AND SVL.ACTIONID  = @ACTLO")
        sqlStat.AppendLine("                       )")
        sqlStat.AppendLine("              THEN 'LEASEOUT'")
        sqlStat.AppendLine("            WHEN EXISTS(SELECT 1")
        sqlStat.AppendLine("                          FROM GBT0005_ODR_VALUE SVL")
        sqlStat.AppendLine("                         WHERE SVL.ORDERNO   = BS.ORDERNO")
        sqlStat.AppendLine("                           And SVL.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("                           And SVL.ACTIONID  = @ACTLI")
        sqlStat.AppendLine("                       )")
        sqlStat.AppendLine("             THEN 'LEASEIN'")
        sqlStat.AppendLine("            ELSE ''")
        sqlStat.AppendLine("        END AS LEASEIO")
        sqlStat.AppendLine("      ,BS.USINGLEASETANK")
        sqlStat.AppendLine("      ,BS.AGENTORGANIZER")
        sqlStat.AppendLine("      ,BS.RECIEPTCOUNTRY1")
        sqlStat.AppendLine("  FROM GBT0004_ODR_BASE BS")
        sqlStat.AppendLine(" WHERE BS.ORDERNO = @ORDERNO ")
        sqlStat.AppendLine("   AND BS.DELFLG <> @DELFLG")

        Dim dtDbResult As New DataTable
        Dim retStr As String = ""
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ACTLO", SqlDbType.NVarChar).Value = "LESD"
                .Add("@ACTLI", SqlDbType.NVarChar).Value = "LEIN"
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using 'sqlDa
        End Using 'sqlCon sqlCmd

        Return dtDbResult
    End Function
    ''' <summary>
    ''' 一覧表示用のデータテーブルを作成
    ''' </summary>
    ''' <returns>TODOまだイマジネーションのため揉む必要あり</returns>
    Private Function CreateOrderListTable() As DataTable
        Dim retDt As New DataTable
        With retDt.Columns
            '固定部分は追加しておく
            .Add("LINECNT", GetType(Integer))            'DBの固定フィールド
            .Add("OPERATION", GetType(String)).DefaultValue = ""           'DBの固定フィールド
            .Add("TIMSTP", GetType(String)).DefaultValue = ""              'DBの固定フィールド
            .Add("SELECT", GetType(Integer))             'DBの固定フィールド
            .Add("HIDDEN", GetType(Integer))
            .Add("DATAID", GetType(String)).DefaultValue = ""
            .Add("SYSKEY", GetType(String)).DefaultValue = ""
            Dim colList As New List(Of String) From {"ACTION", "ORDERNO", "BRTYPE", "TANKSEQ", "DTLPOLPOD", "DTLOFFICE", "TANKNO", "COSTCODE", "COSTNAME", "ACTIONID", "DISPSEQ", "LASTACT",
                                                 "AMOUNTBR", "AMOUNTORD", "AMOUNTFIX", "CONTRACTORBR", "CONTRACTORODR", "CONTRACTORFIX", "SCHEDELDATEBR", "SCHEDELDATE", "ACTUALDATE",
                                                 "APPLYID", "APPLYTEXT", "LASTSTEP", "STATUS", "BRID", "BRCOST", "ACTYNO", "AGENTKBNSORT", "USETYPE", "DISPSEQISEMPTY", "APPLY",
                                                 "INVOICEDBY", "AGENTORGANIZER", "DELFLG",
                                                 "IS_ODR_CHANGECOST", "IS_FIX_CHANGECOST",
                                                 "IS_CALC_DEMURRAGE", "TIP", "DEMURTO", "DEMURUSRATE1", "DEMURUSRATE2",
                                                 "CHARGE_CLASS1", "CHARGE_CLASS4", "LOCALRATE", "CURRENCYCODE",
                                                 "AGENT", "ORGOFFICE", "OTHEROFFICE", "COUNTRYCODE",
                                                 "EXRATE", "REFAMOUNT", "AMOUNTPAY", "LOCALPAY", "SOAAPPDATE",
                                                 "IS_UPDATE_SHIPDATE", "ORIGINDESTINATION", "COMMAMOUNT",
                                                 "CONTRACTORNAMEBR", "CONTRACTORNAMEODR", "CONTRACTORNAMEFIX",
                                                 "BILLINGYMD", "ISBILLINGCLOSED", "USDAMOUNT", "LOCALAMOUNT", "REPORTYMD",
                                                 "JOT", "ISAUTOCLOSE", "ISAUTOCLOSELONG", "DISPLAYCURRANCYCODE", "TAXATION", "TAXRATE",
                                                 "DATEFIELD", 'タンク引当でETDを識別するため
                                                 "SOACHECK", 'SOA日付更新チェックボックス
                                                 "REPORTYMDORG", '本来精算月
                                                 "DEMREPORTMONTH", 'デマレージ用精算月入力ボックス(ActualDateを年月変換し格納)
                                                 "CANMODIFYREPORTMONTH",
                                                 "CLOSINGMONTH",
                                                 "STATUSCODE",
                                                 "LOCALRATESOA", "AMOUNTPAYODR", "LOCALPAYODR", "SOACODE", "SOASHORTCODE", "JOTCODE", "ACCODE",
                                                 "UAG_USD", "UAG_LOCAL", "USD_USD", "USD_LOCAL", "LOCAL_USD", "LOCAL_LOCAL",
                                                 "UPDYMD", "UPDUSER", "UPDTERMID", "REMARK",
                                                 "CAN_ENTRY_ACTUALDATE", "BRADDEDCOST", "IS_UPDATE_TTLINVOICESOAAPPDATE", "SHIPDATE", "DOUTDATE",
                                                 "ACCCURRENCYSEGMENT", "ENABLEACCCURRENCYSEGMENT",
                                                 "CANROWEDIT", "IS_BILLINGCLOSED", "PREV_CONTRACTORFIX",
                                                 "DISPLOCALRATE",
                                                 "TANKFILLING" 'タンク充填状況
            }
            For Each colName As String In colList
                .Add(colName, GetType(String)).DefaultValue = ""
            Next
            .Item("CANROWEDIT").DefaultValue = "1" 'デフォルト行編集可能
        End With
        Return retDt
    End Function
    ''' <summary>
    ''' 4軸表出力用のデータテーブルを作成
    ''' </summary>
    ''' <returns></returns>
    Private Function CreateSummaryListTable() As DataTable
        Dim retDt As New DataTable
        With retDt.Columns
            '固定部分は追加しておく
            .Add("LINECNT", GetType(Integer))            'DBの固定フィールド
            .Add("OPERATION", GetType(String)).DefaultValue = ""           'DBの固定フィールド
            .Add("TIMSTP", GetType(String)).DefaultValue = ""              'DBの固定フィールド
            .Add("SELECT", GetType(Integer))             'DBの固定フィールド
            .Add("HIDDEN", GetType(Integer))
            .Add("DATAID", GetType(String)).DefaultValue = ""
            .Add("SYSKEY", GetType(String)).DefaultValue = ""
            .Add("REPORTMONTH", GetType(String)).DefaultValue = ""         '出力月
            .Add("SOACODE", GetType(String)).DefaultValue = ""             'SOAコード
            Dim colList As New List(Of String) From {"ORDERNO", "BRTYPE", "TANKSEQ", "DTLPOLPOD", "DTLOFFICE", "TANKNO", "COSTCODE", "COSTNAME", "ACTIONID", "DISPSEQ", "LASTACT",
                                                 "AMOUNTBR", "AMOUNTORD", "AMOUNTFIX", "CONTRACTORBR", "CONTRACTORODR", "CONTRACTORFIX", "SCHEDELDATEBR", "SCHEDELDATE", "ACTUALDATE",
                                                 "APPLYID", "APPLYTEXT", "LASTSTEP", "STATUS", "BRID", "BRCOST", "ACTYNO", "AGENTKBNSORT", "USETYPE", "DISPSEQISEMPTY", "APPLY",
                                                 "INVOICEDBY", "AGENTORGANIZER", "DELFLG",
                                                 "IS_ODR_CHANGECOST", "IS_FIX_CHANGECOST",
                                                 "IS_CALC_DEMURRAGE", "TIP", "DEMURTO", "DEMURUSRATE1", "DEMURUSRATE2",
                                                 "CHARGE_CLASS1", "CHARGE_CLASS4", "LOCALRATE", "CURRENCYCODE",
                                                 "AGENT", "ORGOFFICE", "OTHEROFFICE", "COUNTRYCODE",
                                                 "EXRATE", "REFAMOUNT", "AMOUNTPAY", "LOCALPAY", "SOAAPPDATE",
                                                 "IS_UPDATE_SHIPDATE", "ORIGINDESTINATION", "COMMAMOUNT",
                                                 "CONTRACTORNAMEBR", "CONTRACTORNAMEODR", "CONTRACTORNAMEFIX",
                                                 "BILLINGYMD", "ISBILLINGCLOSED", "USDAMOUNT", "LOCALAMOUNT", "REPORTYMD",
                                                 "JOT", "ISAUTOCLOSE", "ISAUTOCLOSELONG", "DISPLAYCURRANCYCODE", "TAXATION", "TAXRATE", "TAXRATE_L", "SOARATE", "EXSHIPRATE_1", "EXSHIPRATE_2",
                                                 "REPORTMONTHH", "COUNTRYNAMEH", "OFFICENAMEH", "APPLYUSERH", "CURRENCYCODEH", "LOCALRATEH",
                                                 "REPORTMONTHORG", "DATA", "JOTCODE", "ACCODE", "LOCALRATESOA", "AMOUNTPAYODR", "LOCALPAYODR",
                                                 "UAG_USD", "UAG_LOCAL", "USD_USD", "USD_LOCAL", "LOCAL_USD", "LOCAL_LOCAL",
                                                 "FINALREPORTNOH", "CLOSEDATEH", "PRINTDATEH", "REMARK"}

            For Each colName As String In colList
                .Add(colName, GetType(String)).DefaultValue = ""
            Next
        End With
        Return retDt
    End Function

    ''' <summary>
    ''' 一覧 マウスホイール時処理 (一覧スクロール)
    ''' </summary>
    Protected Sub ListScrole()
        'If hdnMouseWheel.Value = "" Then
        '    Return
        'End If

        '表示データ件数取得
        Dim DataCnt As Integer = 0                  '(絞り込み後)有効Data数

        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            Dim COA0021ListTable As New BASEDLL.COA0021ListTable            '内部テーブル
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return
            End If
        Else
            dt = Me.SavedDt
        End If


        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        For i As Integer = 0 To dt.Rows.Count - 1
            If Convert.ToString(dt.Rows(i)("HIDDEN")) = "0" Then
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
        If hdnMouseWheel.Value = "+" AndAlso
        (ListPosition + ScrollInt) < DataCnt Then
            ListPosition = ListPosition + ScrollInt
        End If

        '表示位置決定(前頁スクロール)
        If hdnMouseWheel.Value = "-" AndAlso
        (ListPosition - ScrollInt) >= 0 Then
            ListPosition = ListPosition - ScrollInt
        End If

        'ソート
        Dim COA0013TableObject As New BASEDLL.COA0013TableObject
        Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, ListPosition, hdnListPosition)
        '一覧作成
        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnListId.Value
            .SRCDATA = listData
            .TBLOBJ = Me.WF_LISTAREA
            .SCROLLTYPE = "2"
            .TITLEOPT = True
            .NOCOLUMNWIDTHOPT = 50
            .OPERATIONCOLUMNWIDTHOPT = -1
            .USERSORTOPT = 1
        End With
        COA0013TableObject.COA0013SetTableObject()
        hdnMouseWheel.Value = ""

        '1.現在表示しているLINECNTのリストをビューステートに保持
        '2.APPLYチェックがついているチェックボックスオ"DISPLAY_LINECNT_LIST"ブジェクトをチェック状態にする
        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList
            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
            For Each targetCheckBoxId As String In {"APPLY", "JOT", "TAXATION", "SOACHECK"}

                '申請チェックボックスの加工
                Dim targetCheckBoxLineCnt = (From dr As DataRow In listData
                                             Where Convert.ToString(dr.Item(targetCheckBoxId)) <> ""
                                             Select Convert.ToInt32(dr.Item("LINECNT")))
                For Each lineCnt As Integer In targetCheckBoxLineCnt
                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & targetCheckBoxId & lineCnt.ToString
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        With chkObj
                            .Checked = True
                            .Attributes.Add("data-listchkid", .ClientID)
                            .Attributes.Add("data-checkedval", "true")
                        End With
                    End If
                Next
                targetCheckBoxLineCnt = (From dr As DataRow In listData
                                         Where Convert.ToString(dr.Item(targetCheckBoxId)) = ""
                                         Select Convert.ToInt32(dr.Item("LINECNT")))
                For Each lineCnt As Integer In targetCheckBoxLineCnt
                    Dim chkObjId As String = "chk" & Me.WF_LISTAREA.ID & targetCheckBoxId & lineCnt.ToString
                    Dim tmpObj As Control = Me.WF_LISTAREA.FindControl(chkObjId)
                    If tmpObj IsNot Nothing Then
                        Dim chkObj As CheckBox = DirectCast(tmpObj, CheckBox)
                        With chkObj
                            .Attributes.Add("data-listchkid", .ClientID)
                            .Attributes.Add("data-checkedval", "false")
                        End With
                    End If
                Next
            Next
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If

        'Dim test As Control = Me.WF_LISTAREA.FindControl("txtWF_LISTAREASCHEDELDATE2")
        'If test IsNot Nothing Then
        '    Dim val As String = DirectCast(test, TextBox).Text
        '    Dim tbox As TextBox = DirectCast(test, TextBox)
        '    tbox.Attributes.Add("value", val)
        'End If
        hdnMouseWheel.Value = ""



    End Sub
    ''' <summary>
    ''' リスト行ダブルクリック時イベント
    ''' </summary>
    Private Sub ListRowDbClick()
        '一旦何もしない
    End Sub
    ''' <summary>
    ''' 前画面より各種情報を引き継ぎ
    ''' </summary>
    Private Sub SetPrevDisplayValues()
        Dim dt As DataTable = New DataTable
        Dim isScroll As Boolean = False
        Me.hdnListMapVariant.Value = "Default"
        If TypeOf Page.PreviousPage Is GBT00004NEWORDER Then
            '新規作成画面より遷移
            Dim prevObj As GBT00004NEWORDER = DirectCast(Page.PreviousPage, GBT00004NEWORDER)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSearchType", Me.hdnSearchType},
                                                                        {"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"txtFillingDate", Me.hdnFillingDate},
                                                                        {"txtEtd1", Me.hdnEtd1},
                                                                        {"txtEta1", Me.hdnEta1},
                                                                        {"txtEtd2", Me.hdnEtd2},
                                                                        {"txtEta2", Me.hdnEta2},
                                                                        {"hdnOrderNo", Me.hdnOrderNo},
                                                                        {"hdnReportVariant", Me.hdnReportVariant},
                                                                        {"hdnListId", Me.hdnListId}}


            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next

            '仮
            Dim tmpTxtCont As Control = prevObj.FindControl("txtTotalTanks")
            Dim tmpTxt As TextBox = DirectCast(tmpTxtCont, TextBox)
            Me.hdnCopy.Value = tmpTxt.Text
            Me.lblAllocateTankSelectedCount.Text = "0"
            Me.lblAllocateTankMaxCount.Text = tmpTxt.Text
            Me.hdnIsNewData.Value = "1"
            If Me.hdnListId.Value = "DefaultTankAllocate" Then
                Me.txtActy.Text = "TKAL"
            End If
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            'オーダー情報をテーブルより取得
            dt = Me.GetOrderListData()
            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage, pageObject:=Me,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
            End With
        ElseIf TypeOf Page.PreviousPage Is GBT00003RESULT Then
            '検索結果画面より遷移
            Dim prevObj As GBT00003RESULT = DirectCast(Page.PreviousPage, GBT00003RESULT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSearchType", Me.hdnSearchType},
                                                                        {"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSelectedOdId", Me.hdnOrderNo},
                                                                        {"hdnReportVariant", Me.hdnReportVariant}}
            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next

            Dim prevRightList As ListBox = DirectCast(prevObj.FindControl("lbRightList"), ListBox)
            If prevRightList IsNot Nothing Then
                Me.hdnListId.Value = prevRightList.SelectedValue
            End If
            If Me.hdnListId.Value = "DefaultTankAllocate" Then
                Me.txtActy.Text = "TKAL"
            End If
            Me.hdnIsNewData.Value = "0"
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            'オーダー情報をテーブルより取得
            dt = Me.GetOrderListData()
            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
            End With

            Dim selectedCount As Integer = (From dr In dt
                                            Where Convert.ToString(dr.Item("TANKNO")) <> ""
                                            Group By tankseq = Convert.ToString(dr.Item("TANKSEQ")) Into Group
                                            Select tankseq).Count
            Dim maxCount As Integer = (From dr In dt
                                       Group By tankseq = Convert.ToString(dr.Item("TANKSEQ")) Into Group
                                       Select tankseq).Count
            Me.lblAllocateTankSelectedCount.Text = Convert.ToString(selectedCount)
            Me.lblAllocateTankMaxCount.Text = Convert.ToString(maxCount)

        ElseIf TypeOf Page.PreviousPage Is GBT00015RESULT Then
            Me.hdnListMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
            'COSTUP検索結果画面より遷移
            Dim prevObj As GBT00015RESULT = DirectCast(Page.PreviousPage, GBT00015RESULT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSearchType", Me.hdnSearchType},
                                                                        {"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnSelectedOdId", Me.hdnOrderNo},
                                                                        {"hdnReportVariant", Me.hdnReportVariant}}
            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next

            Dim prevRightList As ListBox = DirectCast(prevObj.FindControl("lbRightList"), ListBox)
            If prevRightList IsNot Nothing Then
                Me.hdnListId.Value = prevRightList.SelectedValue
            End If

            Me.hdnIsNewData.Value = "0"
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            'オーダー情報をテーブルより取得
            dt = Me.GetOrderListData()
            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
            End With
        ElseIf TypeOf Page.PreviousPage Is GBT00006RESULT Then
            'タンク一覧画面より遷移
            Dim prevObj As GBT00006RESULT = DirectCast(Page.PreviousPage, GBT00006RESULT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSearchType", Me.hdnSearchType},
                                                                        {"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnFillingDate", Me.hdnFillingDate},
                                                                        {"hdnEtd1", Me.hdnEtd1},
                                                                        {"hdnEta1", Me.hdnEta1},
                                                                        {"hdnEtd2", Me.hdnEtd2},
                                                                        {"hdnEta2", Me.hdnEta2},
                                                                        {"hdnOrderNo", Me.hdnOrderNo},
                                                                        {"hdnOrderOrgXMLsaveFile", Me.hdnOrgXMLsaveFile},
                                                                        {"hdnOrderXMLsaveFile", Me.hdnXMLsaveFile},
                                                                        {"hdnIsNewData", Me.hdnIsNewData},
                                                                        {"hdnCopy", Me.hdnCopy},
                                                                        {"hdnSelectedOrderId", Me.hdnSelectedOrderId},
                                                                        {"hdnSelectedTankSeq", Me.hdnSelectedTankSeq},
                                                                        {"hdnSelectedTankId", Me.hdnSelectedTankId},
                                                                        {"hdnSelectedDataId", Me.hdnSelectedDataId},
                                                                        {"hdnOrderDispListPosition", Me.hdnListPosition},
                                                                        {"hdnListMapVariant", Me.hdnListMapVariant},
                                                                        {"hdnDateTermStYMD", Me.hdnDateTermStYMD},
                                                                        {"hdnDateTermEndYMD", Me.hdnDateTermEndYMD},
                                                                        {"hdnApproval", Me.hdnApproval},
                                                                        {"hdnReportVariant", Me.hdnReportVariant},
                                                                        {"hdnListId", Me.hdnListId}}


            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next
            Me.txtActy.Text = ""
            Dim hdnActObj As HiddenField = DirectCast(prevObj.FindControl("hdnActy"), HiddenField)
            If hdnActObj IsNot Nothing Then
                Me.txtActy.Text = hdnActObj.Value
            End If
            Me.hdnOrderNo.Value = Me.hdnSelectedOrderId.Value
            If prevObj.OrderInfo IsNot Nothing AndAlso prevObj.OrderInfo.IsAllocated = True Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NORMALDBENTRY, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL)

                Select Case Me.hdnListMapVariant.Value
                    Case "GB_TankActivity"
                        dt = Me.GetTankActivityListData()
                    Case "GB_SOA"
                        Dim tmpTxt As TextBox = DirectCast(prevObj.FindControl(Me.txtVender.ID), TextBox)
                        If tmpTxt.Text IsNot Nothing Then
                            Me.txtVender.Text = tmpTxt.Text
                            txtVender_Change()
                        End If
                        dt = Me.GetSOAListData()

                    Case "GB_NonBreaker"
                        dt = Me.GetNonBrListData()
                    Case "GB_Demurrage"
                        dt = Me.GetDemurrageListData()
                    Case Else
                        dt = Me.GetOrderListData()
                End Select

                With Nothing
                    Dim COA0021ListTable As New COA0021ListTable
                    COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()
                    Me.SavedDt = dt
                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                    '保存時比較用のデータを退避
                    COA0021ListTable = New COA0021ListTable
                    COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()
                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                End With

            End If
            '一覧表示データ復元
            If Me.SavedDt Is Nothing Then
                dt = CreateOrderListTable()
                Me.SavedDt = dt
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
            Else
                dt = Me.SavedDt
            End If

            Dim selectedCount As Integer = (From dr In dt
                                            Where Convert.ToString(dr.Item("TANKNO")) <> ""
                                            Group By tankseq = Convert.ToString(dr.Item("TANKSEQ")) Into Group
                                            Select tankseq).Count
            Dim maxCount As Integer = (From dr In dt
                                       Group By tankseq = Convert.ToString(dr.Item("TANKSEQ")) Into Group
                                       Select tankseq).Count
            Me.lblAllocateTankSelectedCount.Text = Convert.ToString(selectedCount)
            Me.lblAllocateTankMaxCount.Text = Convert.ToString(maxCount)

            If Me.hdnSelectedTankId.Value <> "" Then
                If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                    dt = UpdateDatatableTankNo(Me.hdnSelectedOrderId.Value, Me.hdnSelectedTankSeq.Value,
                                           Me.hdnSelectedTankId.Value, prevObj.NeedsTankUseApply, dt, Me.hdnSelectedDataId.Value)
                Else
                    'dt = UpdateDatatableTankNo(Me.hdnSelectedOrderId.Value, Me.hdnSelectedTankSeq.Value,
                    '                       Me.hdnSelectedTankId.Value, prevObj.NeedsTankUseApply, dt)
                End If

                Me.hdnSelectedOrderId.Value = ""
                Me.hdnSelectedTankSeq.Value = ""
                Me.hdnSelectedTankId.Value = ""
                Me.hdnSelectedDataId.Value = ""
                If dt Is Nothing Then
                    Return
                End If
            End If

            Dim listPosition As Integer = 1
            Integer.TryParse(Me.hdnListPosition.Value, listPosition)

        ElseIf TypeOf Page.PreviousPage Is GBT00007SELECT Then
            'ノンブレーカー検索画面より遷移
            'ノンブレーカー用のMAPVariantを保持
            Me.hdnListMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))

            Dim prevObj As GBT00007SELECT = DirectCast(Page.PreviousPage, GBT00007SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtDateTermStYMD", Me.hdnDateTermStYMD},
                                                                        {"txtDateTermEndYMD", Me.hdnDateTermEndYMD},
                                                                        {"txtOffice", Me.hdnOffice},
                                                                        {"txtApproval", Me.hdnApproval},
                                                                        {"rblSettleType", Me.hdnSettleType}}
            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    ElseIf TypeOf tmpCont Is RadioButtonList Then
                        Dim tmpRbl As RadioButtonList = DirectCast(tmpCont, RadioButtonList)
                        item.Value.Value = tmpRbl.SelectedValue
                    End If
                End If
            Next
            Me.hdnIsNewData.Value = "0"
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            Me.hdnListId.Value = Me.hdnListMapVariant.Value '一旦
            'オーダー情報をテーブルより取得
            dt = Me.GetNonBrListData()
            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
            End With
        ElseIf TypeOf Page.PreviousPage Is GBT00008SELECT Then
            'デマレージ検索条件画面からの遷移
            'ノンブレーカー検索画面より遷移
            'ノンブレーカー用のMAPVariantを保持
            Me.hdnListMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))

            Dim prevObj As GBT00008SELECT = DirectCast(Page.PreviousPage, GBT00008SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtDateTermStYMD", Me.hdnDateTermStYMD},
                                                                        {"txtDateTermEndYMD", Me.hdnDateTermEndYMD},
                                                                        {"txtShipper", Me.hdnShipper},
                                                                        {"txtConsignee", Me.hdnConsignee},
                                                                        {"txtOffice", Me.hdnOffice},
                                                                        {"txtTankNo", Me.hdnTankNo},
                                                                        {"txtApproval", Me.hdnApproval},
                                                                        {"rblSettleType", Me.hdnSettleType}}
            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    ElseIf TypeOf tmpCont Is RadioButtonList Then
                        Dim tmpRbl As RadioButtonList = DirectCast(tmpCont, RadioButtonList)
                        item.Value.Value = tmpRbl.SelectedValue
                    End If
                End If
            Next
            Me.hdnIsNewData.Value = "0"
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            Me.hdnListId.Value = Me.hdnListMapVariant.Value '一旦
            'オーダー情報をテーブルより取得
            dt = Me.GetDemurrageListData()
            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
            End With

        ElseIf TypeOf Page.PreviousPage Is GBT00004ORDER Then
            '自分自身のリロード（SAVE、APPLY時に発生想定）
            Dim prevObj As GBT00004ORDER = DirectCast(Page.PreviousPage, GBT00004ORDER)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"hdnSearchType", Me.hdnSearchType},
                                                                        {"hdnETDStYMD", Me.hdnETDStYMD},
                                                                        {"hdnETDEndYMD", Me.hdnETDEndYMD},
                                                                        {"hdnETAStYMD", Me.hdnETAStYMD},
                                                                        {"hdnETAEndYMD", Me.hdnETAEndYMD},
                                                                        {"hdnShipper", Me.hdnShipper},
                                                                        {"hdnConsignee", Me.hdnConsignee},
                                                                        {"hdnPortOfLoading", Me.hdnPortOfLoading},
                                                                        {"hdnPortOfDischarge", Me.hdnPortOfDischarge},
                                                                        {"hdnOffice", Me.hdnOffice},
                                                                        {"hdnFillingDate", Me.hdnFillingDate},
                                                                        {"hdnEtd1", Me.hdnEtd1},
                                                                        {"hdnEta1", Me.hdnEta1},
                                                                        {"hdnEtd2", Me.hdnEtd2},
                                                                        {"hdnEta2", Me.hdnEta2},
                                                                        {"hdnOrderNo", Me.hdnOrderNo},
                                                                        {"hdnIsNewData", Me.hdnIsNewData},
                                                                        {"hdnCopy", Me.hdnCopy},
                                                                        {"hdnSelectedOrderId", Me.hdnSelectedOrderId},
                                                                        {"hdnSelectedTankSeq", Me.hdnSelectedTankSeq},
                                                                        {"hdnSelectedTankId", Me.hdnSelectedTankId},
                                                                        {"hdnListMapVariant", Me.hdnListMapVariant},
                                                                        {"hdnApproval", Me.hdnApproval},
                                                                        {"hdnDateTermStYMD", Me.hdnDateTermStYMD},
                                                                        {"hdnDateTermEndYMD", Me.hdnDateTermEndYMD},
                                                                        {"hdnSettleType", Me.hdnSettleType},
                                                                        {"hdnInvoicedBy", Me.hdnInvoicedBy},
                                                                        {"hdnVender", Me.hdnVender},
                                                                        {"hdnAgentSoa", Me.hdnAgentSoa},
                                                                        {"hdnCountry", Me.hdnCountry},
                                                                        {"hdnActualDateStYMD", Me.hdnActualDateStYMD},
                                                                        {"hdnActualDateEndYMD", Me.hdnActualDateEndYMD},
                                                                        {"hdnReportMonth", Me.hdnReportMonth},
                                                                        {"hdnReportVariant", Me.hdnReportVariant},
                                                                        {"hdnListId", Me.hdnListId},
                                                                        {"hdnActy", Me.hdnActy},
                                                                        {"hdnTankNo", Me.hdnTankNo}}


            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next
            Me.hdnIsNewData.Value = "0"
            Me.txtActy.Text = ""
            Dim prevActy As TextBox = DirectCast(prevObj.FindControl("txtActy"), TextBox)
            If prevActy IsNot Nothing Then
                Me.txtActy.Text = prevActy.Text
            End If
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            'オーダー情報をテーブルより取得
            If prevObj.ProcResult IsNot Nothing AndAlso prevObj.ProcResult.NonBrNo <> "" Then
                'ノンブレーカーの初回保存後の遷移した場合Hidden項目にオーダー番号を設定
                Me.hdnOrderNo.Value = prevObj.ProcResult.NonBrNo
            End If
            Select Case Me.hdnListMapVariant.Value
                Case "GB_TankActivity"
                    dt = Me.GetTankActivityListData()
                Case "GB_SOA"
                    Dim tmpTxt As TextBox = DirectCast(prevObj.FindControl(Me.txtVender.ID), TextBox)
                    If tmpTxt.Text IsNot Nothing Then
                        Me.txtVender.Text = tmpTxt.Text
                        txtVender_Change()
                    End If
                    dt = Me.GetSOAListData()

                Case "GB_NonBreaker"
                    dt = Me.GetNonBrListData()
                Case "GB_Demurrage"
                    dt = Me.GetDemurrageListData()
                Case Else
                    dt = Me.GetOrderListData()
            End Select
            If prevObj.NeedsApplyTextDataId IsNot Nothing AndAlso prevObj.NeedsApplyTextDataId.Count > 0 Then
                Dim targetApplyCheckObj = (From dr In dt Where prevObj.NeedsApplyTextDataId.Contains(Convert.ToString(dr.Item("DATAID"))))
                If targetApplyCheckObj.Any = True Then
                    For Each item In targetApplyCheckObj
                        item("APPLY") = "on"
                    Next
                End If
            End If
            Dim dtPrevData As DataTable = CommonFunctions.DeepCopy(dt)
            If prevObj.ProcResult IsNot Nothing AndAlso prevObj.ProcResult.dateSeqError.Count >= 1 Then
                For Each item In prevObj.ProcResult.dateSeqError
                    Dim dataId As String = Convert.ToString(item("DATAID"))
                    Dim qReWriteData = From reWrite In dt Where dataId <> "" AndAlso reWrite("DATAID").Equals(dataId)
                    If qReWriteData.Any Then
                        Dim drRewrite = qReWriteData.FirstOrDefault
                        drRewrite.ItemArray = item.ItemArray
                    End If
                Next
            End If
            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dtPrevData
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
            End With

            Dim selectedCount As Integer = (From dr In dt
                                            Where Convert.ToString(dr.Item("TANKNO")) <> ""
                                            Group By tankseq = Convert.ToString(dr.Item("TANKSEQ")) Into Group
                                            Select tankseq).Count
            Dim maxCount As Integer = (From dr In dt
                                       Group By tankseq = Convert.ToString(dr.Item("TANKSEQ")) Into Group
                                       Select tankseq).Count
            Me.lblAllocateTankSelectedCount.Text = Convert.ToString(selectedCount)
            Me.lblAllocateTankMaxCount.Text = Convert.ToString(maxCount)

            'リフレッシュ前のメッセージを表示
            Dim cntRefreshMessageNo As Control = prevObj.FindControl("hdnRefreshMessageNo")
            Dim hdnRefreshMessageNoObj As HiddenField = DirectCast(cntRefreshMessageNo, HiddenField)
            If prevObj.ProcResult IsNot Nothing Then
                Dim naeiw As String = C_NAEIW.ABNORMAL
                If {C_MESSAGENO.NORMAL, C_MESSAGENO.NORMALDBENTRY, C_MESSAGENO.APPLYSUCCESS}.Contains(prevObj.ProcResult.MessageNo) Then
                    naeiw = C_NAEIW.NORMAL
                End If
                CommonFunctions.ShowMessage(prevObj.ProcResult.MessageNo, Me.lblFooterMessage, naeiw:=naeiw, pageObject:=Me)
                '左ボックス表示する結果の場合はメッセージを生成
                If prevObj.ProcResult.MessageNo = C_MESSAGENO.RIGHTBIXOUT Then
                    Dim message As New StringBuilder
                    '日付整合性エラー
                    If prevObj.ProcResult.dateSeqError.Count >= 1 Then
                        Dim dummyLabel As New Label
                        Dim errCannotUpdate As String = ""
                        CommonFunctions.ShowMessage(C_MESSAGENO.VALIDITYINPUT, dummyLabel)
                        errCannotUpdate = dummyLabel.Text
                        message.AppendFormat(errCannotUpdate).AppendLine()
                        For Each item In prevObj.ProcResult.dateSeqError
                            message.AppendFormat("--> {0} = {1}", "No.", Convert.ToString(item("LINECNT"))).AppendLine()
                        Next

                    End If
                    'prevObj.ProcResult.modOtherUsers '→他ユーザーに更新されたDATAIDのリスト(上部で取得したdtで必要メッセージを生成)
                    Me.txtRightErrorMessage.Text = message.ToString
                End If

            ElseIf hdnRefreshMessageNoObj IsNot Nothing AndAlso hdnRefreshMessageNoObj.Value <> "" Then
                'APPLYに自身をリダイレクト
                CommonFunctions.ShowMessage(hdnRefreshMessageNoObj.Value, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me,
                                            messageParams:=New List(Of String) From {String.Format("CODE:{0}", hdnRefreshMessageNoObj.Value)})
            End If
        ElseIf TypeOf Page.PreviousPage Is COM00002MENU Then

            '一覧取得・表示時のMAPVariant
            Me.hdnListMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
            'デモ用の仮分岐本来は条件入力画面が噛まされる
            'Dim prevObj As COM00002MENU = DirectCast(Page.PreviousPage, COM00002MENU) 'ここで保持する値はない、本来は入力条件を保持
            Me.hdnIsNewData.Value = "0"
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            'オーダー情報をテーブルより取得
            If Me.hdnListMapVariant.Value = "GB_TankActivity" Then
                dt = Me.GetTankActivityListData()
            Else
                dt = Me.GetSOAListData()
            End If

            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If

            End With
        ElseIf TypeOf Page.PreviousPage Is GBT00004SELECT Then

            '一覧取得・表示時のMAPVariant
            Me.hdnListMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
            'デモ用の仮分岐本来は条件入力画面が噛まされる
            'Dim prevObj As COM00002MENU = DirectCast(Page.PreviousPage, COM00002MENU) 'ここで保持する値はない、本来は入力条件を保持
            Me.hdnIsNewData.Value = "0"
            Dim prevObj As GBT00004SELECT = DirectCast(Page.PreviousPage, GBT00004SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtOffice", Me.hdnOffice},
                                                                        {"txtTankNo", Me.hdnTankNo},
                                                                        {"rblSearchType", Me.hdnSearchType},
                                                                        {"txtStYMD", Me.hdnETAStYMD},
                                                                        {"txtEndYMD", Me.hdnETAEndYMD},
                                                                        {"txtVender", Me.hdnVender},
                                                                        {"txtActy", Me.hdnActy},
                                                                        {"txtOrderNo", Me.hdnOrderNo}}
            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    ElseIf TypeOf tmpCont Is RadioButtonList Then
                        Dim tmpRbl As RadioButtonList = DirectCast(tmpCont, RadioButtonList)
                        item.Value.Value = tmpRbl.SelectedValue
                    End If
                End If
            Next


            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            Me.hdnListId.Value = Me.hdnListMapVariant.Value '一旦
            'オーダー情報をテーブルより取得
            dt = Me.GetTankActivityListData()

            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If

            End With
        ElseIf TypeOf Page.PreviousPage Is GBT00009SELECT Then
            'SOA検索条件画面より遷移
            Dim prevObj As GBT00009SELECT = DirectCast(Page.PreviousPage, GBT00009SELECT)
            Dim dicObjs As New Dictionary(Of String, HiddenField) From {{"txtOffice", Me.hdnOffice},
                                                                        {"txtInvoicedBy", Me.hdnInvoicedBy},
                                                                        {"txtVender", Me.hdnVender},
                                                                        {"txtAgentSoa", Me.hdnAgentSoa},
                                                                        {"txtCountry", Me.hdnCountry},
                                                                        {"txtActualDateStYMD", Me.hdnActualDateStYMD},
                                                                        {"txtActualDateEndYMD", Me.hdnActualDateEndYMD},
                                                                        {"txtReportMonth", Me.hdnReportMonth}
                                                                        }

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    ElseIf TypeOf tmpCont Is RadioButtonList Then
                        Dim tmpRbl As RadioButtonList = DirectCast(tmpCont, RadioButtonList)
                        item.Value.Value = tmpRbl.SelectedValue
                    End If
                End If
            Next
            '一覧取得・表示時のMAPVariant
            Me.hdnListMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
            'デモ用の仮分岐本来は条件入力画面が噛まされる
            'Dim prevObj As COM00002MENU = DirectCast(Page.PreviousPage, COM00002MENU) 'ここで保持する値はない、本来は入力条件を保持
            Me.hdnIsNewData.Value = "0"
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            Me.hdnListId.Value = Me.hdnListMapVariant.Value '一旦
            'オーダー情報をテーブルより取得
            dt = Me.GetSOAListData()
            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If

            End With

        ElseIf TypeOf Page.PreviousPage Is GBT00017RESULT Then
            Me.hdnListMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
            'COSTUP検索結果画面より遷移
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
                                                                        {"hdnSelectedOdId", Me.hdnOrderNo},
                                                                        {"hdnSelectedTrans", Me.hdnTrans},
                                                                        {"hdnReportVariant", Me.hdnReportVariant}}
            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    If TypeOf tmpCont Is HiddenField Then
                        Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                        item.Value.Value = tmpHdn.Value
                    ElseIf TypeOf tmpCont Is TextBox Then
                        Dim tmpTxtObj As TextBox = DirectCast(tmpCont, TextBox)
                        item.Value.Value = tmpTxtObj.Text
                    End If
                End If
            Next

            Me.hdnSettleType.Value = "02SETTLED"

            Dim prevRightList As ListBox = DirectCast(prevObj.FindControl("lbRightList"), ListBox)
            If prevRightList IsNot Nothing Then
                Me.hdnListId.Value = prevRightList.SelectedValue
            End If

            Me.hdnIsNewData.Value = "0"
            '一覧情報保存先のファイル名
            Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
            Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
            'オーダー情報をテーブルより取得
            dt = Me.GetOrderListData()
            '一覧表データ取得
            With Nothing
                Dim COA0021ListTable As New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
                '保存時比較用のデータを退避
                COA0021ListTable = New COA0021ListTable
                COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                    Return
                End If
            End With
        ElseIf Page.PreviousPage Is Nothing Then
            '承認画面より遷移
            If Convert.ToString(Request.Form("hdnSender")) = "GBT00010A" Then
                Dim applyId As String = Request.Form("hdnApplyID")
                Dim eventCode As String = Request.Form("hdnEventCode")
                Dim listVariant As String = eventCode.Replace(C_ODREVENT.APPLY, "")

                Me.hdnListMapVariant.Value = listVariant
                Me.hdnListId.Value = listVariant
                Select Case Me.hdnListMapVariant.Value
                    Case "GB_TankActivity"
                        dt = Me.GetTankActivityListData(applyId)
                    Case "GB_SOA"
                        dt = Me.GetSOAListData(applyId)
                    Case "GB_NonBreaker"
                        dt = Me.GetNonBrListData(applyId)
                    Case "GB_Demurrage"
                        dt = Me.GetDemurrageListData(applyId)
                    Case Else
                        dt = Me.GetOrderListData(applyId)
                End Select

                '一覧表データ取得
                With Nothing
                    '一覧情報保存先のファイル名
                    Me.hdnXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))
                    '初回ロード時のデータ保持用(保存ボタン押下時にて上記ファイルと比較し変更を判断)
                    Me.hdnOrgXMLsaveFile.Value = String.Format("{0}\{1:yyyyMMdd}-{2}-{3}-{4}-{1:HHmmss}_org.txt", COA0019Session.XMLDir, Date.Now, COA0019Session.USERID, CONST_MAPID, HttpContext.Current.Session("MAPvariant"))

                    Dim COA0021ListTable As New COA0021ListTable
                    COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()
                    Me.SavedDt = dt
                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                    '保存時比較用のデータを退避
                    COA0021ListTable = New COA0021ListTable
                    COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
                    COA0021ListTable.TBLDATA = dt
                    COA0021ListTable.COA0021saveListTable()
                    If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                        CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
                        Return
                    End If
                    Me.hdnSettleType.Value = "02SETTLED"
                End With
            End If
        End If
        '****************************************
        'オーダー情報取得（仮）
        '****************************************
        '■■■ 一覧表示データ編集（性能対策） ■■■
        '表示対象行カウント(絞り込み対象)
        '　※　絞込（Cells(4)： 0=表示対象 , 1=非表示対象)
        Dim DataCnt As Integer = 0
        For i As Integer = 0 To dt.Rows.Count - 1
            If Convert.ToString(dt.Rows(i)(4)) = "0" Then
                DataCnt = DataCnt + 1
                '行（ラインカウント）を再設定する。既存項目（SELECT）を利用
                dt.Rows(i)("SELECT") = DataCnt
            End If
            dt.Rows(i)("PREV_CONTRACTORFIX") = dt.Rows(i)("CONTRACTORFIX")
        Next
        Dim COA0013TableObject As New COA0013TableObject
        Dim listData As DataTable = COA0013TableObject.GetSortedDatatable(dt, Me.WF_LISTAREA, CONST_DSPROWCOUNT, 1, hdnListPosition)
        With COA0013TableObject
            .MAPID = CONST_MAPID
            .VARI = Me.hdnListId.Value 'Me.hdnListMapVariant.Value,
            .SRCDATA = listData
            .TBLOBJ = WF_LISTAREA
            .SCROLLTYPE = "2"
            .TITLEOPT = True
            .NOCOLUMNWIDTHOPT = 50
            .OPERATIONCOLUMNWIDTHOPT = -1
            .USERSORTOPT = 1
        End With
        COA0013TableObject.COA0013SetTableObject()

        If listData IsNot Nothing AndAlso listData.Rows.Count > 0 Then
            Dim displayLineCnt As List(Of Integer) = (From dr As DataRow In listData
                                                      Select Convert.ToInt32(dr.Item("LINECNT"))).ToList

            For Each targetCheckBoxId As String In {"APPLY", "JOT", "TAXATION", "SOACHECK"}

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

            ViewState("DISPLAY_LINECNT_LIST") = displayLineCnt
        Else
            ViewState("DISPLAY_LINECNT_LIST") = Nothing
        End If
        '左ボックス費用追加用の選択項目追加
        SetAddCostDdlRbnItem(dt)
        '条件ベンダーの追加
        SetVenderListItem("")

    End Sub
    ''' <summary>
    ''' 表示非表示制御
    ''' </summary>
    ''' <remarks>初回ロード時（非ポストバック時）に実行する想定</remarks>
    Private Sub DisplayControl()
        ''一旦デマレッジ以外は確定ボタンを非表示
        'Me.btnFix.Visible = False
        'If Me.hdnListMapVariant.Value = "GB_Demurrage" Then
        '    Me.btnFix.Visible = True
        'End If
        '(仮)精算ボタン表示制御
        Me.btnBliingClose.Visible = False
        Dim hiddenSearchFieldList As New List(Of Control) _
            From {Me.spnClosingDate, Me.spnVender, Me.spnTankNo,
                  Me.spnActy, Me.spnBrVender, Me.spnEstimatedVender,
                  Me.spnAlocTankInfo, Me.spnCostItem, Me.spnUsdAmountSummary}
        For Each hiddenItem In hiddenSearchFieldList
            hiddenItem.Visible = False
        Next
        Dim showSearchFieldList As New List(Of Control)
        '通常オーダー（タンク引当）
        If Me.hdnListMapVariant.Value = "Default" Then
            showSearchFieldList.AddRange({Me.spnCostItem, Me.spnAlocTankInfo, Me.spnTankNo, Me.spnActy, Me.spnBrVender})
            Me.btnApply.Visible = False
        End If
        'SOA
        If Me.hdnListMapVariant.Value = "GB_SOA" Then
            Me.btnBliingClose.Visible = True
            Me.btnApply.Visible = False
            Dim counrtyIsMatch As Boolean = False
            Dim targetCloseYm As String = FormatDateContrySettings(Me.hdnCurrentCloseYm.Value, GBA00003UserSetting.DATEYMFORMAT)
            Dim prevMonthIsApproved As Boolean = False
            If IsDate(Me.hdnReportMonth.Value & "/01") Then
                Dim prevMonth As String = CDate(Me.hdnReportMonth.Value & "/01").AddMonths(-1).ToString("yyyy/MM")
                Dim closingGroup As String = If(GBA00003UserSetting.IS_JOTUSER, GBC_JOT_SOA_COUNTRY, Me.hdnCountry.Value)
                Dim dtCd = GetPrintClosingDate(closingGroup, prevMonth)
                If dtCd IsNot Nothing AndAlso dtCd.Rows.Count > 0 Then
                    prevMonthIsApproved = True
                End If
            End If
            Dim disabledObj As New List(Of HtmlInputButton) From {Me.btnApply, Me.btnSave}
            If GBA00003UserSetting.IS_JOTUSER = False Then
                disabledObj.Add(Me.btnExcelDownload)
            End If

            If GBA00003UserSetting.IS_JOTUSER = False AndAlso GBA00003UserSetting.COUNTRYCODE = Me.hdnCountry.Value Then
                counrtyIsMatch = True
            ElseIf Me.hdnInvoicedBy.Value = "OJ" AndAlso GBA00003UserSetting.IS_JOTUSER AndAlso Me.hdnCountry.Value = "" Then
                counrtyIsMatch = True
            End If
            If counrtyIsMatch = False Then
                Me.Form.Attributes.Add("data-disabled", "1")
                For Each btnObj As HtmlInputButton In disabledObj
                    btnObj.Attributes("disabled") = "disabled"
                    btnObj.Attributes("class") = "aspNetDisabled"
                Next
            End If
            If Me.hdnReportMonth.Value <> targetCloseYm OrElse counrtyIsMatch = False OrElse (prevMonthIsApproved = False AndAlso Not {"2019/06", "06/2019"}.Contains(targetCloseYm)) Then
                Me.btnBliingClose.Attributes("disabled") = "disabled"
                Me.btnBliingClose.Attributes("class") = "aspNetDisabled"
            End If

            showSearchFieldList.AddRange({Me.spnTankNo, Me.spnCostItem, Me.spnClosingDate, Me.spnVender, Me.spnUsdAmountSummary, Me.spnHideNoAmount})
            '↓20190725 TOTALINVOICE含み額を表示させるチェックボックス条件
            If GBA00003UserSetting.IS_JOTUSER Then
                spnShowTotalInvoiceRelatedCost.Visible = True
            Else
                spnShowTotalInvoiceRelatedCost.Visible = False
            End If
            '↑20190725 TOTALINVOICE含み額を表示させるチェックボックス条件
        End If
        'タンク動静
        If Me.hdnListMapVariant.Value = "GB_TankActivity" Then
            showSearchFieldList.AddRange({Me.spnCostItem, Me.spnTankNo, Me.spnActy, Me.spnEstimatedVender})
        End If
        'デマレッジ
        If Me.hdnListMapVariant.Value = "GB_Demurrage" Then
            showSearchFieldList.AddRange({Me.spnTankNo})
        End If
        'COST UP
        If Me.hdnListMapVariant.Value = "GB_CostUp" Then
            showSearchFieldList.AddRange({Me.spnCostItem, Me.spnTankNo, Me.spnActy, Me.spnBrVender})
        End If
        'ノンブレーカー
        If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
            showSearchFieldList.AddRange({Me.spnCostItem, Me.spnTankNo})
        End If
        '検索条件フィールドの表示
        For Each showSearchFielditem In showSearchFieldList
            showSearchFielditem.Visible = True
        Next
        'コスト追加ボタンを非表示
        If {"GB_TankActivity", "GB_SOA", "GB_Demurrage", "GB_PRINT"}.Contains(hdnListMapVariant.Value) Then
            Me.btnAddCost.Visible = False
        End If
        '読み取り専用(検索条件精算済)
        If Me.hdnSettleType.Value = "02SETTLED" Then
            Me.btnAddCost.Attributes("disabled") = "disabled"
            Me.btnAddCost.Attributes("class") = "aspNetDisabled"
            Dim invisibleItems As New List(Of Control) From {Me.btnApply, Me.btnExcelDownload, Me.btnSave, Me.btnRemarkInputOk}
            For Each item In invisibleItems
                item.Visible = False
            Next

            Me.txtRemarkInput.Enabled = False

        End If
        'GB_PRINTのみの制御
        If hdnListMapVariant.Value = "GB_PRINT" Then
            Me.spnOrderNo.Visible = False
            Me.btnExtract.Visible = False
            Me.orderHeaderBox.Visible = False
        End If
    End Sub
    ''' <summary>
    ''' 左ボックス費用追加の「No.」項目選択肢及び「POL/POD」項目選択肢の生成
    ''' </summary>
    ''' <param name="dt"></param>
    Private Sub SetAddCostDdlRbnItem(dt As DataTable)
        Me.ddlNo.Items.Clear()
        Me.rblPolPod.Items.Clear()
        Dim allTanks As String = "ALL TANKS"
        If COA0019Session.LANGDISP = C_LANG.JA Then
            allTanks = "全タンク"
        End If
        '全タンクを追加
        ddlNo.Items.Add(New ListItem(allTanks, ""))
        ddlNo.Items(0).Selected = True
        '表示すべきデータがない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        'データテーブルよりNoをグループ化し取得ドロップダウンリストに設定
        Dim noList As List(Of String) = (From dr As DataRow In dt
                                         Group By nostr = Convert.ToString(dr.Item("TANKSEQ")) Into grp = Group
                                         Select nostr).ToList
        For Each noStr As String In noList
            ddlNo.Items.Add(New ListItem(noStr, noStr))
        Next
        'データテーブルよりPOL/PODをグループ化しチェックボックスリストに設定
        Dim targetCountry As String = ""
        If Not GBA00003UserSetting.IS_JOTUSER Then
            targetCountry = GBA00003UserSetting.COUNTRYCODE
        End If
        Dim polPodList As List(Of String) = (From dr As DataRow In dt
                                             Where targetCountry = "" OrElse dr("COUNTRYCODE").Equals(targetCountry)
                                             Group By agtKbn = Convert.ToString(dr.Item("DTLPOLPOD")) Into grp = Group
                                             Select agtKbn).ToList
        For Each polPodStr As String In polPodList
            rblPolPod.Items.Add(New ListItem(polPodStr, polPodStr))
        Next

        If polPodList IsNot Nothing AndAlso polPodList.Count > 0 Then
            rblPolPod.Items(0).Selected = True
        End If
    End Sub
    ''' <summary>
    ''' 費用項目一覧を取得
    ''' </summary>
    ''' <param name="brType"></param>
    ''' <returns></returns>
    Private Function GetCostItem(brType As String, Optional costCode As String = "", Optional polPod As String = "") As DataTable
        'If brType = "" Then
        '    brType = C_CHARGECODE.CLASS1.SALES '一旦セールス固定 '現時点で未使用ですが引数でSQLの条件を変える想定
        'End If
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim textField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMES"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT COSTCODE AS CODE")
        sqlStat.AppendFormat("     , COSTCODE + ':' + {0} As LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("       ,CLASS1")
        sqlStat.AppendLine("       ,CLASS2")
        sqlStat.AppendLine("       ,CLASS4")
        sqlStat.AppendLine("       ,CASE WHEN (CRGENERALPURPOSE = '1' OR DBGENERALPURPOSE = '1') THEN '1' ELSE '0' END AS ENABLEACCCURRENCYSEGMENT") 'ノンブレのACCCURRENCYSEGMENT入力可否判定用
        sqlStat.AppendLine("  FROM GBM0010_CHARGECODE")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        Select Case brType
            Case C_BRTYPE.SALES
                sqlStat.AppendLine("   And SALESBR     = '" & CONST_FLAG_YES & "'")
            Case C_BRTYPE.OPERATION
                sqlStat.AppendLine("   And OPERATIONBR = '" & CONST_FLAG_YES & "'")
            Case C_BRTYPE.NONBR
                sqlStat.AppendLine("   And NONBR = '" & CONST_FLAG_YES & "'")
            Case C_BRTYPE.REPAIR
                sqlStat.AppendLine("   And REPAIRBR = '" & CONST_FLAG_YES & "'")
            Case "SOA"
                sqlStat.AppendLine("   And SOA = '" & CONST_FLAG_YES & "'")
        End Select
        If costCode <> "" Then
            sqlStat.AppendLine("   And COSTCODE    = @COSTCODE")
        End If
        If polPod <> "" Then
            sqlStat.AppendLine("   And (LDKBN    = 'B' OR LDKBN    = @LDKBN)")
        End If
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY COSTCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            With sqlCmd.Parameters
                'SQLパラメータ設定
                .Add("@COMPCODE", SqlDbType.NVarChar, 10).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@COSTCODE", SqlDbType.NVarChar, 20).Value = costCode
                .Add("@LDKBN", SqlDbType.NVarChar).Value = polPod
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
    ''' 業者コード列挙したリストアイテムを設定
    ''' </summary>
    ''' <param name="selectedValue"></param>
    ''' <remarks>TRADERとDEPOテーブルのUNION</remarks>
    Private Sub SetVenderListItem(selectedValue As String)
        'リストクリア
        Me.lbVender.Items.Clear()
        'SQL文の作成
        Dim country As String = Nothing

        Dim dtCont As DataTable = Me.GetContractor(country, GBC_CHARGECLASS4.OTHER)
        If dtCont IsNot Nothing Then
            With Me.lbVender
                .Items.Clear()
                .DataSource = dtCont
                .DataTextField = "LISTBOXNAME"
                .DataValueField = "CODE"
                .DataBind()
                .Focus()
            End With
        End If
    End Sub
    ''' <summary>
    ''' 通貨コード一覧データテーブル取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>FIXVALUEよりシステム共通通貨コード、国マスタより
    ''' 発着に紐づく国の通貨コード（ノンブレ時は自身の国に紐づく通貨コード）
    ''' を取得しデータテーブルを返却</remarks>
    Private Function GetCurrency(selectedDr As DataRow, Optional curCode As String = "") As DataTable
        If selectedDr Is Nothing Then
            Return Nothing
        End If
        Dim retDt As New DataTable
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT DISTINCT CRY.CURRENCYCODE        AS CODE")
        sqlStat.AppendLine("      ,CRY.CURRENCYCODE        AS LISTBOXNAME") '通貨に関してはコード＋名称は意味がないので一旦コードのみ
        sqlStat.AppendLine("      ,ISNULL(EXR.EXRATE,'0')  AS EXRATE")
        sqlStat.AppendLine("  FROM GBM0001_COUNTRY CRY")
        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE EXR")
        sqlStat.AppendLine("    ON EXR.COMPCODE     = CRY.COMPCODE")
        sqlStat.AppendLine("   AND EXR.COUNTRYCODE  = CRY.COUNTRYCODE")
        sqlStat.AppendLine("   AND EXR.CURRENCYCODE = CRY.CURRENCYCODE")
        sqlStat.AppendLine("   AND EXR.TARGETYM     = DATEADD(DAY, 1-DATEPART(DAY, @TARGETYM), @TARGETYM)")
        sqlStat.AppendLine("   AND EXR.DELFLG      <> @DELFLG")
        sqlStat.AppendLine(" WHERE CRY.COMPCODE       = @COMPCODE")
        sqlStat.AppendLine("   AND CRY.COUNTRYCODE    = @COUNTRYCODE")
        If curCode <> "" Then
            sqlStat.AppendLine("   AND CRY.CURRENCYCODE    = @CURRENCYCODE")
        End If
        sqlStat.AppendLine("   AND CRY.DELFLG  <> @DELFLG")
        sqlStat.AppendLine("   AND CRY.STYMD   <= @NOWDATE")
        sqlStat.AppendLine("   AND CRY.ENDYMD  >= @NOWDATE")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim dateList As New List(Of String) From {Convert.ToString(selectedDr.Item("ACTUALDATE")),
                                                      Convert.ToString(selectedDr.Item("SCHEDELDATE")),
                                                      Convert.ToString(selectedDr.Item("SCHEDELDATEBR"))
                                                      }

            Dim targetym As String = Date.Now.ToString("yyyy/MM/dd")
            For Each dateItem In dateList
                If dateItem <> "" Then
                    targetym = dateItem
                    Exit For
                End If
            Next
            With sqlCmd.Parameters
                Dim cuntryCode As String = Convert.ToString(selectedDr.Item("COUNTRYCODE"))
                If Me.hdnListMapVariant.Value = "GB_Demurrage" Then
                    cuntryCode = Me.hdnUserCurrency.Value
                End If

                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@TARGETYM", SqlDbType.NVarChar).Value = targetym
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = cuntryCode
                If curCode <> "" Then
                    .Add("@CURRENCYCODE", SqlDbType.NVarChar).Value = curCode
                End If
                .Add("@NOWDATE", System.Data.SqlDbType.Date).Value = Date.Now
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' 業者のリストを取得する
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <param name="carrierCode">業者コード</param>
    ''' <returns></returns>
    Private Function GetContractor(countryCode As String, chargeClass4 As String, Optional carrierCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        With retDt.Columns
            .Add("CODE", GetType(String))
            .Add("LISTBOXNAME", GetType(String))
            .Add("NAME", GetType(String))
        End With
        Dim GBA00004CountryRelated As GBA00004CountryRelated = New GBA00004CountryRelated
        GBA00004CountryRelated.COUNTRYCODE = countryCode
        Dim listboxBummy As New ListBox
        Select Case chargeClass4
            Case GBC_CHARGECLASS4.AGENT
                GBA00004CountryRelated.LISTBOX_OFFICE = listboxBummy
                GBA00004CountryRelated.GBA00004getLeftListOffice()
            Case GBC_CHARGECLASS4.CURRIER
                GBA00004CountryRelated.LISTBOX_VENDER = listboxBummy
                GBA00004CountryRelated.GBA00004getLeftListVender()
            Case GBC_CHARGECLASS4.FORWARDER
                GBA00004CountryRelated.LISTBOX_FORWARDER = listboxBummy
                GBA00004CountryRelated.GBA00004getLeftListForwarder()
            Case GBC_CHARGECLASS4.PORT
                GBA00004CountryRelated.LISTBOX_PORT = listboxBummy
                GBA00004CountryRelated.GBA00004getLeftListPort()
            Case GBC_CHARGECLASS4.DEPOT
                GBA00004CountryRelated.LISTBOX_DEPOT = listboxBummy
                GBA00004CountryRelated.GBA00004getLeftListDepot()
            Case GBC_CHARGECLASS4.OTHER
                GBA00004CountryRelated.LISTBOX_OTHER = listboxBummy
                GBA00004CountryRelated.GBA00004getLeftListOther()
            Case GBC_CHARGECLASS4.CUSTOMER
                If carrierCode <> "" Then
                    retDt = GetCustomer(customerCode:=carrierCode)
                ElseIf countryCode <> "" Then
                    retDt = GetCustomer(countryCode:=countryCode)
                Else
                    retDt = GetCustomer()
                End If
            Case Else
                ' 内航船は国指定なし
                If Left(chargeClass4, Len(GBC_CHARGECLASS4.PORT_I)).Equals(GBC_CHARGECLASS4.PORT_I) Then
                    If carrierCode <> "" Then
                        retDt = GBA00006PortRelated.GBA00006getPortCodeValue(portCode:=carrierCode)
                    Else
                        retDt = GBA00006PortRelated.GBA00006getPortCodeValue()
                    End If
                End If

        End Select
        If listboxBummy.Items IsNot Nothing AndAlso listboxBummy.Items.Count > 0 Then
            Dim listItem = (From item As ListItem In listboxBummy.Items.Cast(Of ListItem)
                            Select retDt.Rows.Add(item.Value, item.Text, Split(item.Text, ":", 2)(1))).CopyToDataTable
            '単一絞りこみがある場合
            If carrierCode <> "" Then
                With (From item In retDt Where Convert.ToString(item("CODE")) = carrierCode)
                    If .Any Then
                        retDt = .CopyToDataTable
                    Else
                        retDt = Nothing
                    End If
                End With
            End If
        End If
        Return retDt
    End Function
    ''' <summary>
    ''' 汎用補助区分(ノンブレ時の一覧表同項目選択肢）
    ''' </summary>
    ''' <param name="accCurrencySegment"></param>
    ''' <returns></returns>
    Private Function GetAccCurrencySegment(Optional accCurrencySegment As String = "") As DataTable
        Dim COA0017FixValue As New COA0017FixValue
        Dim dt As New DataTable
        With dt.Columns
            .Add("NAME", GetType(String))
            .Add("LISTBOXNAME", GetType(String))
            .Add("CODE", GetType(String))
        End With

        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "AUXCLASS"
        Dim dummyList As New ListBox
        If COA0019Session.LANGDISP = C_LANG.JA Then
            '本当は1だが暫定的に2
            COA0017FixValue.LISTBOX1 = dummyList
            'COA0017FixValue.LISTBOX2 = dummyList
        Else
            COA0017FixValue.LISTBOX2 = dummyList
        End If
        COA0017FixValue.COA0017getListFixValue()
        For Each litem As ListItem In dummyList.Items
            Dim dr As DataRow = dt.NewRow
            dr.Item("NAME") = litem.Text
            dr.Item("LISTBOXNAME") = litem.Value & ":" & litem.Text
            dr.Item("CODE") = litem.Value

            If accCurrencySegment <> "" Then
                If accCurrencySegment = litem.Value Then
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
    ''' 顧客のリストを取得する
    ''' </summary>
    ''' <param name="customerCode">顧客コード</param>
    ''' <param name="countryCode">国コード</param>
    ''' <returns></returns>
    Private Function GetCustomer(Optional customerCode As String = "", Optional countryCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        With retDt.Columns
            .Add("CODE", GetType(String))
            .Add("LISTBOXNAME", GetType(String))
            .Add("NAME", GetType(String))
        End With

        'SQL文作成
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CUSTOMERCODE AS CODE")
        sqlStat.AppendFormat("     , CUSTOMERCODE + ':' + {0} As LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If customerCode <> "" Then
            sqlStat.AppendLine("   And CUSTOMERCODE    = @CUSTOMERCODE")
        End If
        If countryCode <> "" Then
            sqlStat.AppendLine("   And COUNTRYCODE     = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY CUSTOMERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
                .Add("@CUSTOMERCODE", SqlDbType.NVarChar).Value = customerCode
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
                .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES  '"1"
            End With
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        Return retDt
    End Function
    ''' <summary>
    ''' ACTY番号を取得
    ''' </summary>
    ''' <param name="actyCode">省略時は全件取得</param>
    ''' <returns></returns>
    Private Function GetActy(Optional actyCode As String = "") As DataTable
        Dim COA0017FixValue As New COA0017FixValue
        Dim dummyList As New ListBox
        Dim dt As New DataTable
        With dt.Columns
            .Add("NAME", GetType(String))
            .Add("LISTBOXNAME", GetType(String))
            .Add("CODE", GetType(String))
        End With
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
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
                'COA0021ListTable.COA0021recoverListTable()
                'Me.PrevDt = COA0021ListTable.OUTTBL
                Me.PrevDt = dt.Clone
                For Each cdr As DataRow In dt.Rows
                    Me.PrevDt.ImportRow(cdr)
                Next
                'Me.PrevDt = dt.Copy
                'Me.PrevDt = CommonFunctions.DeepCopy(dt) 'COA0021ListTable.OUTTBL
            Else
                Me.PrevDt = Nothing
                Return COA0021ListTable.ERR

            End If
        Else
            dt = Me.SavedDt
        End If

        'この段階でありえないがデータテーブルがない場合は終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        '読み取り専用(検索条件精算済)の場合はこのまま終了
        If Me.hdnSettleType.Value = "02SETTLED" OrElse
           Me.Form.Attributes("data-disabled") = "1" Then
            Me.SavedDt = dt
            Return C_MESSAGENO.NORMAL
        End If

        'サフィックス抜き（LISTID)抜きのオブジェクト名リスト
        Dim objTxtPrifix As String = "txt" & Me.WF_LISTAREA.ID
        Dim objChkPrifix As String = "chk" & Me.WF_LISTAREA.ID
        Dim fieldIdList As New Dictionary(Of String, String)
        If Me.hdnListMapVariant.Value = "GB_TankActivity" Then
            fieldIdList.Add("AMOUNTORD", objTxtPrifix)
            fieldIdList.Add("CONTRACTORFIX", objTxtPrifix)
            'fieldIdList.Add("APPLY", objChkPrifix)
            fieldIdList.Add("ACTUALDATE", objTxtPrifix)
            fieldIdList.Add("JOT", objChkPrifix)
        ElseIf Me.hdnListMapVariant.Value = "GB_SOA" Then
            'fieldIdList.Add("SOAAPPDATE", objTxtPrifix) '2018/12/10チェックボックス化の為廃止
            'fieldIdList.Add("LOCALPAY", objTxtPrifix)
            'fieldIdList.Add("AMOUNTPAY", objTxtPrifix)
            'fieldIdList.Add("AMOUNTFIX", objTxtPrifix)
            fieldIdList.Add("AMOUNTORD", objTxtPrifix)
            fieldIdList.Add("APPLY", objChkPrifix)
            fieldIdList.Add("JOT", objChkPrifix)
            fieldIdList.Add("SOACHECK", objChkPrifix)
            fieldIdList.Add("ACTUALDATE", objTxtPrifix)
        ElseIf Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
            fieldIdList.Add("AMOUNTORD", objTxtPrifix)
            fieldIdList.Add("CONTRACTORFIX", objTxtPrifix)
            fieldIdList.Add("APPLY", objChkPrifix)
            fieldIdList.Add("ACTUALDATE", objTxtPrifix)
            fieldIdList.Add("INVOICEDBY", objTxtPrifix)
            fieldIdList.Add("TANKNO", objTxtPrifix)
            fieldIdList.Add("CURRENCYCODE", objTxtPrifix)
            fieldIdList.Add("JOT", objChkPrifix)
            fieldIdList.Add("ACCCURRENCYSEGMENT", objTxtPrifix)
        ElseIf Me.hdnListMapVariant.Value = "GB_Demurrage" Then
            fieldIdList.Add("AMOUNTORD", objTxtPrifix)
            'fieldIdList.Add("AMOUNTFIX", objTxtPrifix)
            fieldIdList.Add("APPLY", objChkPrifix)
            'fieldIdList.Add("SOAAPPDATE", objTxtPrifix)
            fieldIdList.Add("DEMREPORTMONTH", objTxtPrifix)
            fieldIdList.Add("CONTRACTORFIX", objTxtPrifix)
            fieldIdList.Add("JOT", objChkPrifix)
        Else
            fieldIdList.Add("AMOUNTORD", objTxtPrifix)
            fieldIdList.Add("CONTRACTORODR", objTxtPrifix)
            fieldIdList.Add("APPLY", objChkPrifix)
            fieldIdList.Add("SCHEDELDATE", objTxtPrifix)
            fieldIdList.Add("CURRENCYCODE", objTxtPrifix)
            fieldIdList.Add("JOT", objChkPrifix)
        End If
        '課税フラグを画面より収集するか判定
        If GBA00003UserSetting.IS_JPOPERATOR AndAlso
           {"GB_SOA", "GB_NonBreaker", "GB_CostUp"}.Contains(Me.hdnListMapVariant.Value) Then
            fieldIdList.Add("TAXATION", objChkPrifix)
        End If


        'Dim formToPost = New NameValueCollection(Request.Form)
        For Each i In displayLineCnt
            Dim dr As DataRow = dt.Rows(i - 1)
            dr("PREV_CONTRACTORFIX") = dr("CONTRACTORFIX") '書き換え前の業者Fixを保持

            For Each fieldId As KeyValuePair(Of String, String) In fieldIdList
                Dim dispObjId As String = fieldId.Value & fieldId.Key & i
                Dim displayValue As String = ""
                If Request.Form.AllKeys.Contains(dispObjId) Then
                    displayValue = Request.Form(dispObjId)
                    '                    formToPost.Remove(dispObjId)
                End If

                Dim val As String = ""
                If {"ACTUALDATE", "SCHEDELDATE", "SCHEDELDATEBR", "SOAAPPDATE"}.Contains(fieldId.Key) Then

                    val = displayValue
                    val = val.Trim
                    Dim tmpDate As Date
                    If val <> "" AndAlso Date.TryParseExact(val, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, tmpDate) = False Then
                        val = displayValue
                    ElseIf val <> "" Then
                        val = tmpDate.ToString("yyyy/MM/dd") '一旦yyyy/MM/dd形式に変更（TODO：国ごとの日付フォーマット)
                    End If
                    displayValue = val
                End If
                If {"DEMREPORTMONTH"}.Contains(fieldId.Key) Then
                    val = displayValue
                    val = val.Trim
                    Dim tmpDate As Date
                    If val <> "" AndAlso Date.TryParseExact(val, GBA00003UserSetting.DATEYMFORMAT, Nothing, Nothing, tmpDate) = False Then
                        val = displayValue
                    ElseIf val <> "" Then
                        val = tmpDate.ToString("yyyy/MM") '一旦yyyy/MM/dd形式に変更（TODO：国ごとの日付フォーマット)
                    End If
                    displayValue = val
                End If
                dr.Item(fieldId.Key) = displayValue
                'ノンブレーカー申請チェックボックス制御有効化のためコメントアウト
                'If Me.hdnListMapVariant.Value = "GB_NonBreaker" AndAlso fieldId.Key.Equals("AMOUNTORD") AndAlso Convert.ToString(dr.Item("STATUS")).Equals("") Then
                '    dr.Item("AMOUNTFIX") = dr.Item(fieldId.Key)
                'End If
            Next

        Next

        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        Me.SavedDt = dt
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0021ListTable.ERR
        End If
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 一覧に選択した費用を追加
    ''' </summary>
    ''' <param name="costCode">費用コード</param>
    ''' <returns>メッセージNo</returns>
    Private Function AddNewCostItem(costCode As String, Optional ByVal procExcel As Boolean = False, Optional ByRef excelDr As DataRow = Nothing, Optional ByRef lastSysno As String = "") As String
        Dim COA0021ListTable As New COA0021ListTable
        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return COA0021ListTable.ERR
            End If
        Else
            dt = Me.SavedDt
        End If


        Dim addTankNoList As New List(Of String)
        Dim podPol As String = ""
        Dim curCode As String = ""
        If procExcel = False Then
            '画面より費目追加処理
            If Me.ddlNo.SelectedValue = "" Then
                For Each litem As ListItem In Me.ddlNo.Items
                    If litem.Value <> "" Then
                        addTankNoList.Add(litem.Value)
                    End If
                Next
            Else
                addTankNoList.Add(Me.ddlNo.SelectedValue)
            End If
            podPol = Me.rblPolPod.SelectedValue
        Else
            'excelDr.Item("")
            'Excelより費目追加処理
            addTankNoList.Add(Convert.ToString(excelDr.Item("TANKSEQ")))
            curCode = Convert.ToString(excelDr.Item("CURRENCYCODE"))
            podPol = Convert.ToString(excelDr.Item("DTLPOLPOD"))
        End If
        'ありえないがNoが無い場合はそのまま終了
        If addTankNoList.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If

        Dim ldKbn As String = If(podPol.StartsWith("POD"), "D", "L")
        Dim brType As String = GetBrType(Convert.ToString(dt.Rows(0)("ORDERNO")))

        'コピーするフィールドリスト
        Dim copyFields As New List(Of String) From {"BRID", "ORDERNO", "TANKNO", "DTLPOLPOD",
                                                    "TANKSEQ", "AGENTKBNSORT", "USETYPE", "DTLOFFICE", "AGENTORGANIZER", "AGENT",
                                                    "COUNTRYCODE", "INVOICEDBY", "LOCALRATE"}
        '費用名称を変数に格納
        Dim costName As String = ""
        Dim chargeClass4 As String = ""
        Using costNameDt As DataTable = GetCostItem(brType, costCode, polPod:=ldKbn)
            If costNameDt IsNot Nothing AndAlso costNameDt.Rows.Count > 0 Then
                costName = Convert.ToString(costNameDt.Rows(0).Item("NAME"))
                chargeClass4 = Convert.ToString(costNameDt.Rows(0).Item("CLASS4"))
            Else
                Return C_MESSAGENO.INVALIDINPUT
            End If
        End Using
        Dim maxSysNo As String = (From dr As DataRow In dt
                                  Order By dr.Item("SYSKEY") Descending
                                  Select Convert.ToString(dr.Item("SYSKEY"))).FirstOrDefault()
        Dim currentSysNo As Integer = 0
        If maxSysNo IsNot Nothing AndAlso maxSysNo.Trim <> "" Then
            If Integer.TryParse(maxSysNo.Replace("SYS", ""), currentSysNo) = False Then
                currentSysNo = 0
            End If
        End If
        '追加すべきタンク連番を取得
        Dim lastSysKey As String = ""
        For Each addTankNo In addTankNoList
            '最大ACTYNOを取得
            Dim maxActyNoDr As DataRow = (From dr As DataRow In dt
                                          Where dr.Item("TANKSEQ").Equals(addTankNo)
                                          Order By dr.Item("ACTYNO") Descending
                                          Select dr).FirstOrDefault()
            '発・着にTANKSEQ応じた共通情報部分を取得かつ現在の最大ACTYNOを取得
            Dim copyDr As DataRow = (From dr As DataRow In dt
                                     Where dr.Item("DTLPOLPOD").Equals(podPol) _
                                   AndAlso dr.Item("TANKSEQ").Equals(addTankNo)
                                     Order By If(Convert.ToString(dr.Item("JOT")) = "", 0, 1)
                                     Select dr).FirstOrDefault()
            Dim addDr As DataRow = dt.NewRow
            For Each copyField As String In copyFields
                addDr.Item(copyField) = copyDr.Item(copyField)
            Next
            'ACTYNOのインクリメント
            Dim actyNoString As String = Convert.ToString(maxActyNoDr.Item("ACTYNO"))
            Dim actyNo As Integer = 0
            Integer.TryParse(actyNoString, actyNo)
            actyNo = actyNo + 1
            actyNoString = actyNo.ToString("000")
            '固定項目部
            addDr.Item("LINECNT") = 0 'あとでソート後振り直し
            addDr.Item("OPERATION") = "0"
            addDr.Item("TIMSTP") = ""
            addDr.Item("HIDDEN") = "0"
            addDr.Item("SELECT") = "1"
            '
            addDr.Item("DATAID") = ""
            addDr.Item("COSTCODE") = costCode
            addDr.Item("COSTNAME") = costName
            addDr.Item("CHARGE_CLASS4") = chargeClass4
            addDr.Item("CURRENCYCODE") = curCode
            addDr.Item("ACTYNO") = actyNoString
            addDr.Item("BRCOST") = "0"
            addDr.Item("ACTION") = "0"
            addDr.Item("DISPSEQISEMPTY") = "1"
            addDr.Item("TAXATION") = GetDefaultTaxation(Convert.ToString(addDr.Item("COUNTRYCODE")))
            addDr.Item("DELFLG") = CONST_FLAG_NO

            currentSysNo = currentSysNo + 1
            lastSysKey = "SYS" & currentSysNo.ToString("00000")
            addDr.Item("SYSKEY") = lastSysKey
            UpdateStringDbNullToBlank(addDr)
            dt.Rows.Add(addDr)

        Next
        '一意ソートキーの振り直し
        Dim sortedDt As DataTable = (From dr As DataRow In dt
                                     Order By dr.Item("TANKSEQ"), dr.Item("AGENTKBNSORT"), dr.Item("ACTYNO")
                                     Select dr).CopyToDataTable
        Dim lineCnt As Integer = 1
        Dim currentTankSeq As String = Convert.ToString(sortedDt.Rows(0).Item("TANKSEQ"))
        Dim reNumberActyNo As Integer = 1
        For Each sortedDr As DataRow In sortedDt.Rows
            sortedDr.Item("LINECNT") = lineCnt
            lineCnt = lineCnt + 1
            'ACTYNO振り直し(追加対象のTANKSEQの場合)
            If addTankNoList.Contains(Convert.ToString(sortedDr.Item("TANKSEQ"))) Then
                If currentTankSeq <> Convert.ToString(sortedDr.Item("TANKSEQ")) Then
                    reNumberActyNo = 1
                    currentTankSeq = Convert.ToString(sortedDr.Item("TANKSEQ"))
                End If
                sortedDr.Item("ACTYNO") = reNumberActyNo.ToString("000")
                reNumberActyNo = reNumberActyNo + 1
            End If

        Next
        If procExcel = False Then
            'ファイルを保存
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = sortedDt
            COA0021ListTable.COA0021saveListTable()
            Me.SavedDt = sortedDt
            If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                Return COA0021ListTable.ERR
            End If
            btnExtract_Click(False)
            Return C_MESSAGENO.NORMAL
        Else

            Me.SavedDt = sortedDt
            'excelDr.Item("SYSKEY") = lastSysKey
            lastSysno = lastSysKey
            Return C_MESSAGENO.NORMAL
        End If

    End Function
    ''' <summary>
    ''' 一覧に選択したノンブレーカー費用を追加
    ''' </summary>
    ''' <param name="costCode">費用コード</param>
    ''' <returns>メッセージNo</returns>
    Private Function AddNewNbCostItem(costCode As String, Optional ByVal procExcel As Boolean = False, Optional ByRef sysNo As String = "") As String
        Dim COA0021ListTable As New COA0021ListTable
        '一覧表示データ復元
        Dim dt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return COA0021ListTable.ERR
            End If
        Else
            dt = Me.SavedDt
        End If


        '費用名称を変数に格納
        Dim costName As String = ""
        Dim class1Val As String = ""
        Dim class2Val As String = ""
        Dim dispStatus As String = ""
        Dim chargeClass4 As String = ""
        Dim enableAccCurrencySegment = "0"
        Using costNameDt As DataTable = GetCostItem(C_BRTYPE.NONBR, costCode)
            If costNameDt IsNot Nothing AndAlso costNameDt.Rows.Count > 0 Then
                costName = Convert.ToString(costNameDt.Rows(0).Item("NAME"))
                chargeClass4 = Convert.ToString(costNameDt.Rows(0).Item("CLASS4"))
                class1Val = Convert.ToString(costNameDt.Rows(0).Item("CLASS1"))
                class2Val = Convert.ToString(costNameDt.Rows(0).Item("CLASS2"))
                '費用項目が売上の場合は申請必須とする
                If class2Val <> "" Then
                    dispStatus = Me.GetFixValue("APPROVAL", C_APP_STATUS.APPAGAIN)
                End If
                'ACCCURRENCYSEGMENT変更可否フラグ
                If costNameDt.Rows(0).Item("ENABLEACCCURRENCYSEGMENT").Equals("1") Then
                    enableAccCurrencySegment = "1"
                End If
            Else
                Return C_MESSAGENO.INVALIDINPUT
            End If
        End Using
        '最大のSysNoの取得
        Dim maxSysNo As String = (From dr As DataRow In dt
                                  Order By dr.Item("SYSKEY") Descending
                                  Select Convert.ToString(dr.Item("SYSKEY"))).FirstOrDefault()
        Dim currentSysNo As Integer = 0
        If maxSysNo IsNot Nothing AndAlso maxSysNo.Trim <> "" Then
            If Integer.TryParse(maxSysNo.Replace("SYS", ""), currentSysNo) = False Then
                currentSysNo = 0
            End If
        End If
        '最大のDispSeqの取得
        Dim maxDispSeq As String = (From dr As DataRow In dt
                                    Order By dr.Item("DISPSEQ") Descending
                                    Select Convert.ToString(dr.Item("DISPSEQ"))).FirstOrDefault()
        Dim currentDispSeq As Integer = 0
        If maxDispSeq IsNot Nothing AndAlso maxDispSeq.Trim <> "" Then
            If Integer.TryParse(maxDispSeq, currentDispSeq) = False Then
                currentDispSeq = 0
            End If
        End If
        '最大のLineCntの取得
        Dim maxLineCnt As String = (From dr As DataRow In dt
                                    Order By dr.Item("LINECNT") Descending
                                    Select Convert.ToString(dr.Item("LINECNT"))).FirstOrDefault()
        Dim currentLineCnt As Integer = 0
        If maxLineCnt IsNot Nothing AndAlso maxLineCnt.Trim <> "" Then
            If Integer.TryParse(maxLineCnt, currentLineCnt) = False Then
                currentLineCnt = 0
            End If
        End If
        Dim addDr As DataRow = dt.NewRow
        currentLineCnt = currentLineCnt + 1
        addDr.Item("LINECNT") = currentLineCnt
        addDr.Item("OPERATION") = "0"
        addDr.Item("TIMSTP") = ""
        addDr.Item("HIDDEN") = "0"
        addDr.Item("SELECT") = "1"
        '
        addDr.Item("ORDERNO") = Me.hdnOrderNo.Value
        addDr.Item("COSTCODE") = costCode
        addDr.Item("COSTNAME") = costName
        addDr.Item("CHARGE_CLASS4") = chargeClass4

        addDr.Item("BRCOST") = "0"
        addDr.Item("DISPSEQISEMPTY") = "1"
        currentDispSeq = currentDispSeq + 1
        addDr.Item("DISPSEQ") = currentDispSeq.ToString("0")
        addDr.Item("DELFLG") = CONST_FLAG_NO
        currentSysNo = currentSysNo + 1
        sysNo = "SYS" & currentSysNo.ToString("00000")
        addDr.Item("SYSKEY") = sysNo
        addDr.Item("DATAID") = sysNo '新規レコードは暫定的にSYSKEY(タンク引当のキーとするため)
        addDr.Item("STATUS") = dispStatus
        addDr.Item("CHARGE_CLASS1") = class1Val
        'Dim agent As String = Me.hdnUserOffice.Value
        Dim agent As String
        If GBA00003UserSetting.IS_JOTUSER AndAlso Me.hdnOffice.Value <> "" Then
            agent = Me.hdnOffice.Value
            Dim GBA00005OfficeRelated As New GBA00005OfficeRelated
            GBA00005OfficeRelated.OFFICECODE = Me.hdnOffice.Value
            GBA00005OfficeRelated.GBA00005getCountry()
            addDr.Item("COUNTRYCODE") = GBA00005OfficeRelated.COUNTRYCODE
        Else
            agent = Me.hdnUserOffice.Value
            addDr.Item("COUNTRYCODE") = Me.hdnUserCountry.Value
        End If
        addDr.Item("AGENTORGANIZER") = agent
        addDr.Item("AGENT") = agent
        addDr.Item("DTLOFFICE") = agent
        'addDr.Item("COUNTRYCODE") = Me.hdnUserCountry.Value
        'addDr.Item("TAXATION") = "0"
        addDr.Item("TAXATION") = GetDefaultTaxation(Convert.ToString(addDr.Item("COUNTRYCODE")))
        addDr.Item("INVOICEDBY") = agent
        addDr.Item("ENABLEACCCURRENCYSEGMENT") = enableAccCurrencySegment
        If enableAccCurrencySegment = "1" Then
            addDr.Item("ACCCURRENCYSEGMENT") = "1"
        End If
        UpdateStringDbNullToBlank(addDr)
        dt.Rows.Add(addDr)

        If procExcel = False Then
            'ファイルを保存
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021saveListTable()
            If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                Return COA0021ListTable.ERR
            End If
            Me.SavedDt = dt

            btnExtract_Click(False)
            Return C_MESSAGENO.NORMAL
        Else
            Me.SavedDt = dt
            Return C_MESSAGENO.NORMAL
        End If

    End Function
    ''' <summary>
    ''' タンク一覧画面へ遷移する
    ''' </summary>
    ''' <returns></returns>
    Private Function OpenTankList() As String
        '選択された情報をGBT00006ROrderInfoに設定
        Me.Gbt00006ROrderInfo = New GBT00006RESULT.GBT00006ROrderInfo
        Me.Gbt00006ROrderInfo.OrderNo = Me.hdnSelectedOrderId.Value
        '**************************************************
        'オーダーベースより情報取得
        '**************************************************
        Dim dtOdrBase As DataTable = GetOrderBaseDt(Me.Gbt00006ROrderInfo.OrderNo)
        If dtOdrBase IsNot Nothing AndAlso
           dtOdrBase.Rows.Count > 0 Then
            Dim drOdrBase As DataRow = dtOdrBase.Rows(0)
            With Me.Gbt00006ROrderInfo
                .ShipperCode = Convert.ToString(drOdrBase("SHIPPER"))
                .ProductCode = Convert.ToString(drOdrBase("PRODUCTCODE"))
                .LeaseIO = Convert.ToString(drOdrBase("LEASEIO"))
                .UsingLeaseTank = Convert.ToString(drOdrBase("USINGLEASETANK"))
                .AgentOrganizer = Convert.ToString(drOdrBase("AGENTORGANIZER"))
                .CountryCode = Convert.ToString(drOdrBase("RECIEPTCOUNTRY1"))
            End With
        End If
        '**************************************************
        '選択したオーダーの最大ETDを取得
        '**************************************************
        Me.Gbt00006ROrderInfo.ETD = ""
        Dim maxEtd As String = (From item In Me.SavedDt
                                Where Convert.ToString(item("ORDERNO")) = Me.Gbt00006ROrderInfo.OrderNo _
                              AndAlso Convert.ToString(item("DATEFIELD")).StartsWith("ETD")
                                Order By item("SCHEDELDATE") Descending
                                Select Convert.ToString(item("SCHEDELDATE"))).FirstOrDefault
        Me.Gbt00006ROrderInfo.ETD = Convert.ToString(maxEtd)
        '**************************************************
        '選択されたオーダー番号につきタンク未引当のタンクシーケンス及び、タンクNoを取得(タンクシーケンスでunique)
        '**************************************************
        Dim tankSeqList = (From item In Me.SavedDt
                           Where Convert.ToString(item("ORDERNO")) = Me.Gbt00006ROrderInfo.OrderNo
                           Group By TankSeq = Convert.ToString(item.Item("TANKSEQ"))
                           Into TankNo = Max(Convert.ToString(item("TANKNO"))))
        Dim shippedTanks = GetOrderValueShipTanks(Me.Gbt00006ROrderInfo.OrderNo)

        If tankSeqList.Any = True Then
            Me.Gbt00006ROrderInfo.TankInfoList = New Dictionary(Of String, GBT00006RESULT.GBT00006RTankInfo)
            With Me.Gbt00006ROrderInfo.TankInfoList
                For Each tankSeqItem In tankSeqList
                    Dim isShipped As Boolean = False
                    isShipped = False
                    If shippedTanks.ContainsKey(tankSeqItem.TankSeq) Then
                        isShipped = True
                    End If
                    .Add(tankSeqItem.TankSeq, New GBT00006RESULT.GBT00006RTankInfo(tankSeqItem.TankSeq, tankSeqItem.TankNo, isShipped))
                Next
            End With
        Else
            Return C_MESSAGENO.NONALLOCATETANKEXISTS 'ありえないが対象のオーダーNoのタンクシーケンスなし
        End If
        '**************************************************
        '未保存の状態で引き当てようとしていないかチェック
        '(引き剥がし→未保存→ダブルクリックのパターン）
        '**************************************************
        ''画面起動時のデータを取得
        'Dim firstTimeDt = CreateOrderListTable()
        'Dim COA0021ListTable As New COA0021ListTable With {.FILEdir = Me.hdnOrgXMLsaveFile.Value,
        '                                                   .TBLDATA = firstTimeDt}
        'COA0021ListTable.COA0021recoverListTable()
        'If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
        '    firstTimeDt = COA0021ListTable.OUTTBL
        'Else
        '    Return COA0021ListTable.ERR
        'End If
        'Dim firstTimeTankInfo = (From item In firstTimeDt
        '                         Where Convert.ToString(item("ORDERNO")) = Me.Gbt00006ROrderInfo.OrderNo _
        '                       AndAlso Me.Gbt00006ROrderInfo.TankInfoList.ContainsKey(Convert.ToString(item("TANKSEQ"))) _
        '                       AndAlso Convert.ToString(item("TANKNO")) <> ""
        '                         Group By TankSeq = Convert.ToString(item.Item("TANKSEQ"))
        '                         Into TankNo = Max(Convert.ToString(item("TANKNO"))))
        ''未保存の引き剥がし情報あり
        'If firstTimeTankInfo.Any Then
        '    Return C_MESSAGENO.NOSAVEDEALLOCATETANK
        'End If
        '**************************************************
        'タンクステータス画面へ遷移
        '**************************************************
        Dim COA0012DoUrl As BASEDLL.COA0012DoUrl
        '■■■ 画面遷移先URL取得 ■■■
        COA0012DoUrl.MAPIDP = CONST_MAPID
        HttpContext.Current.Session("MAPvariant") = "GB_TankSelect"
        COA0012DoUrl.VARIP = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0012DoUrl.ERR
        End If

        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 日付項目更新
    ''' </summary>
    ''' <param name="txtBoxId"></param>
    ''' <param name="rowNum"></param>
    ''' <returns></returns>
    Private Function GetDatatableDate(txtBoxId As String, rowNum As String) As KeyValuePair(Of String, DataRow)
        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = Nothing
        '一覧表示データ復元
        If Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                Return New KeyValuePair(Of String, DataRow)
            End If
        Else
            dt = Me.SavedDt
        End If
        '書き換えるテキストフィールドを特定
        Dim targetDateField As String = "ACTUALDATE"
        If txtBoxId.StartsWith("txtWF_LISTAREASCHEDELDATE") Then
            targetDateField = "SCHEDELDATE"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREASOAAPPDATE") Then
            targetDateField = "SOAAPPDATE"
        End If
        '日付変更対象の行を取得
        Dim targetRows = From dr As DataRow In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = rowNum
        'ありえないが編集業が存在しない場合
        If targetRows Is Nothing Then
            Return New KeyValuePair(Of String, DataRow)
        End If
        Dim targetRow As DataRow = targetRows(0)
        'Dim retDateValue As String = Convert.ToString(targetRow.Item(targetDateField))
        'Return retDateValue
        Return New KeyValuePair(Of String, DataRow)(targetDateField, targetRow)
    End Function
    ''' <summary>
    ''' 対応する日付に関連するデータテーブルを更新
    ''' </summary>
    ''' <param name="dtValue"></param>
    ''' <param name="txtBoxId"></param>
    ''' <param name="rowNum"></param>
    ''' <returns></returns>
    Private Function UpdateDatatableDate(dtValue As String, txtBoxId As String, rowNum As String, Optional targetDt As DataTable = Nothing) As String
        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = Nothing
        '一覧表示データ復元
        If targetDt IsNot Nothing Then
            dt = targetDt
        ElseIf Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                Return COA0021ListTable.ERR
            End If
        Else
            dt = Me.SavedDt
        End If
        '書き換えるテキストフィールドを特定
        Dim targetDateField As String = ""
        If txtBoxId.StartsWith("txtWF_LISTAREASCHEDELDATE") Then
            targetDateField = "SCHEDELDATE"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREASOAAPPDATE") Then
            targetDateField = "SOAAPPDATE"
        ElseIf txtBoxId.StartsWith("txtWF_LISTAREAACTUALDATE") Then
            targetDateField = "ACTUALDATE"
        Else
            Return C_MESSAGENO.NORMAL
        End If
        '日付変更対象の行を取得
        Dim targetRows = From dr As DataRow In Me.PrevDt
                         Where Convert.ToString(dr.Item("LINECNT")) = rowNum
        Dim afterInputRows = From dr As DataRow In dt
                             Where Convert.ToString(dr.Item("LINECNT")) = rowNum

        'ありえないが編集業が存在しない場合
        If targetRows Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If
        '自身の対象行を取得
        Dim targetRow As DataRow = targetRows(0)
        Dim afterInputRow As DataRow = afterInputRows(0)
        '自身の行の設定日付を取得
        Dim prevDtValue As String = Convert.ToString(targetRow.Item(targetDateField))
        '自身の行の日付を編集
        afterInputRow.Item(targetDateField) = dtValue
        '自身のACTYを取得
        Dim actyNo As String = Convert.ToString(targetRow.Item("ACTIONID"))
        'ACTY無しまたは日付項目がSOAAPDATEの場合は更新しない。
        If actyNo <> "" AndAlso targetDateField <> "SOAAPPDATE" Then
            '日付入力したACTYをもとに他の日付を連鎖して更新
            Dim orderNo As String = Convert.ToString(targetRow.Item("ORDERNO"))
            Dim tankSeq As String = Convert.ToString(targetRow.Item("TANKSEQ"))
            Dim dtlPolPod As String = Convert.ToString(targetRow.Item("DTLPOLPOD"))
            Dim polPod As String = "POL"
            If dtlPolPod.StartsWith("POD") Then
                polPod = "POD"
            End If
            Dim trans As String = "1" '第一輸送
            If dtlPolPod.EndsWith("2") Then
                trans = "2"
            End If
            Dim targetCostCode = GetIntarlockCostCodeFromActy(actyNo, polPod)
            'Dim polPod As List(Of String) = Nothing
            'If dtlPolPod.EndsWith("1") Then
            '    polPod = New List(Of String) From {"POL1", "POD1"}
            'Else
            '    polPod = New List(Of String) From {"POL2", "POD2"}
            'End If
            If targetCostCode IsNot Nothing AndAlso targetCostCode.Count > 0 Then
                '画面表示一覧データより同一のオーダー、連動更新対象の費目のレコードを絞り込み
                'Dim updateRowItems = From dr As DataRow In dt
                '                     Where Convert.ToString(dr.Item("ORDERNO")) = orderNo _
                '                       And Convert.ToString(dr.Item("TANKSEQ")) = tankSeq _
                '                       And targetCostCode.Contains(Convert.ToString(dr.Item("COSTCODE"))) _
                '                       And {prevDtValue, ""}.Contains(Convert.ToString(dr.Item(targetDateField)))
                'Dim updateRowItems = From dr As DataRow In dt
                '                     Where Convert.ToString(dr.Item("ORDERNO")) = orderNo _
                '                       And Convert.ToString(dr.Item("TANKSEQ")) = tankSeq _
                '                       And polPod.Contains(Convert.ToString(dr.Item("DTLPOLPOD"))) _
                '                       And targetCostCode.Contains(Convert.ToString(dr.Item("COSTCODE"))) _
                '                       And {prevDtValue, ""}.Contains(Convert.ToString(dr.Item(targetDateField)))

                Dim updateRowItems = From dr As DataRow In dt
                                     Where Convert.ToString(dr.Item("ORDERNO")) = orderNo _
                                   AndAlso Convert.ToString(dr.Item("TANKSEQ")) = tankSeq _
                                   AndAlso Convert.ToString(dr.Item("DTLPOLPOD")).EndsWith(trans) _
                                   AndAlso {prevDtValue, ""}.Contains(Convert.ToString(dr.Item(targetDateField))) _
                                   AndAlso targetCostCode.Any(Function(cItem) cItem.CostCode = Convert.ToString(dr.Item("COSTCODE")) _
                                                            AndAlso ((Convert.ToString(dr.Item("DTLPOLPOD")).StartsWith("PO" & cItem.LdKbn)) _
                                                                OrElse cItem.LdKbn = "B"))

                '絞り込んだ日付項目を更新
                For Each updateRowItem In updateRowItems
                    updateRowItem.Item(targetDateField) = dtValue
                Next
            End If
        End If

        If targetDt Is Nothing Then
            'サーバーローカルに保存
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021saveListTable()
            Me.SavedDt = dt
            If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                Return COA0021ListTable.ERR
            End If

        End If
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 通貨コード更新
    ''' </summary>
    ''' <param name="curCode"></param>
    ''' <param name="lineCnt"></param>
    ''' <returns></returns>
    Private Function UpdateDatatableCurrency(curCode As String, lineCnt As String, Optional excelProc As Boolean = False, Optional ByRef excelDr As DataRow = Nothing) As String
        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = Nothing
        If curCode = "" Then
            Return C_MESSAGENO.NORMAL
        End If
        Dim targetRow As DataRow
        If excelProc = False Then
            '一覧表示データ復元
            If Me.SavedDt Is Nothing Then
                dt = CreateOrderListTable()
                COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021recoverListTable()
                If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                    dt = COA0021ListTable.OUTTBL
                Else
                    Return COA0021ListTable.ERR
                End If
            Else
                dt = Me.SavedDt
            End If
            '対象の行取得
            targetRow = (From dr As DataRow In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = lineCnt).FirstOrDefault
        Else
            targetRow = excelDr
        End If

        'ありえないが編集業がない場合はそのまま終了
        If targetRow Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If

        Dim exr As String = "0"
        Dim curCodeFix As String = curCode.Trim
        targetRow.Item("CURRENCYCODE") = curCodeFix
        If curCodeFix <> GBC_CUR_USD Then
            Dim dtCur As DataTable = Me.GetCurrency(targetRow, curCodeFix)
            If dtCur IsNot Nothing AndAlso dtCur.Rows.Count > 0 Then
                exr = Convert.ToString(dtCur.Rows(0).Item("EXRATE"))
            End If
        Else
            Dim dtCur As DataTable = Me.GetCurrency(targetRow, Me.hdnUserCurrency.Value)
            If dtCur IsNot Nothing AndAlso dtCur.Rows.Count > 0 Then
                exr = Convert.ToString(dtCur.Rows(0).Item("EXRATE"))
            End If
        End If

        targetRow.Item("LOCALRATE") = exr
        If excelProc = False Then
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021saveListTable()
        End If
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 対応する業者に関連するデータテーブルを更新
    ''' </summary>
    ''' <param name="contValue"></param>
    ''' <param name="txtBoxId"></param>
    ''' <param name="rowNum"></param>
    ''' <returns></returns>
    Private Function UpdateDatatableContractor(contValue As String, txtBoxId As String, rowNum As String, Optional targetDt As DataTable = Nothing) As String
        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = Nothing

        '一覧表示データ復元
        If targetDt IsNot Nothing Then
            dt = targetDt
        ElseIf Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                Return COA0021ListTable.ERR
            End If
        Else
            dt = Me.SavedDt
        End If
        '日付変更対象の行を取得
        Dim targetRows = From dr As DataRow In Me.PrevDt
                         Where Convert.ToString(dr.Item("LINECNT")) = rowNum
        Dim afterInputRows = From dr As DataRow In dt
                             Where Convert.ToString(dr.Item("LINECNT")) = rowNum
        '自身の対象行を取得
        Dim targetRow As DataRow = targetRows(0)
        Dim afterInputRow As DataRow = afterInputRows(0)
        If targetRow Is Nothing Then
            targetRow = dt.NewRow
        End If
        '書き換えるテキストフィールドを特定
        Dim targetContractorField As String = "CONTRACTORFIX"
        Dim targetContractorNameField As String = "CONTRACTORNAMEFIX"
        If txtBoxId.StartsWith("txtWF_LISTAREACONTRACTORODR") Then
            afterInputRow.Item("CONTRACTORFIX") = contValue
            afterInputRow.Item("CONTRACTORODR") = contValue
            With Nothing
                'サーバーローカルに保存
                COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021saveListTable()
                Me.SavedDt = dt
                If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                    Return COA0021ListTable.ERR
                End If
            End With
            Return C_MESSAGENO.NORMAL
            targetContractorField = "CONTRACTORODR"
            targetContractorNameField = "CONTRACTORNAMEODR"
        End If



        '自身の行の設定日付を取得
        Dim prevDtValue As String = Convert.ToString(targetRow.Item(targetContractorField))
        '自身の行の日付を編集
        afterInputRow.Item(targetContractorField) = contValue
        '費目Class4を取得
        Dim chargeClass4 As String = Convert.ToString(afterInputRow.Item("CHARGE_CLASS4"))
        'Dim brContractor As String = Convert.ToString(targetRow.Item("CONTRACTORBR"))
        Dim orderNo As String = Convert.ToString(afterInputRow.Item("ORDERNO"))
        Dim tankSeq As String = Convert.ToString(afterInputRow.Item("TANKSEQ"))
        Dim dtlPolPod As String = Convert.ToString(afterInputRow.Item("DTLPOLPOD"))
        Dim countryCode As String = Convert.ToString(afterInputRow("COUNTRYCODE"))
        Dim orgContractor As String = Convert.ToString(afterInputRow("PREV_CONTRACTORFIX"))
        Dim contractorName As String = ""
        Dim refCustomerFlag As Boolean = False
        If Convert.ToString(targetRow.Item("DTLPOLPOD")) = "Organizer" Then
            refCustomerFlag = True
        Else
            Dim costCode As String = Convert.ToString(afterInputRow("COSTCODE"))
            Dim dtCst = GetCostItem("", costCode:=costCode)

            If dtCst IsNot Nothing AndAlso dtCst.Rows.Count > 0 Then
                Dim drCst = dtCst.Rows(0)
                If Convert.ToString(drCst("CLASS2")) <> "" Then
                    refCustomerFlag = True
                End If
            End If
        End If
        If refCustomerFlag Then

            If contValue <> "" Then 'コード入力時は検索を行う
                Dim dtCont = GetCustomer(contValue)
                If dtCont IsNot Nothing AndAlso dtCont.Rows.Count > 0 Then
                    contractorName = Convert.ToString(dtCont.Rows(0).Item("NAME"))
                End If
            End If

            Dim updateRowItems = From dr As DataRow In dt
                                 Where Convert.ToString(dr.Item("ORDERNO")) = orderNo _
                                       AndAlso Convert.ToString(dr.Item("TANKSEQ")) = tankSeq _
                                       AndAlso Convert.ToString(dr.Item("DTLPOLPOD")) = dtlPolPod _
                                       AndAlso Convert.ToString(dr.Item("CHARGE_CLASS4")) = chargeClass4 _
                                       AndAlso {orgContractor, ""}.Contains(Convert.ToString(dr.Item("CONTRACTORFIX")))

            '絞り込んだ日付項目を更新
            For Each updateRowItem In updateRowItems
                updateRowItem.Item(targetContractorField) = contValue
                updateRowItem.Item(targetContractorNameField) = contractorName
            Next

        Else
            If chargeClass4 <> "" AndAlso chargeClass4 <> "－" Then
                '日付入力したACTYをもとに他の日付を連鎖して更新

                If contValue <> "" Then 'コード入力時は検索を行う
                    Dim dtCont = GetContractor(countryCode, chargeClass4, contValue)
                    If dtCont IsNot Nothing AndAlso dtCont.Rows.Count > 0 Then
                        contractorName = Convert.ToString(dtCont.Rows(0).Item("NAME"))
                    End If
                End If
                'Dim polPod As List(Of String) = Nothing
                'If dtlPolPod.EndsWith("1") Then
                '    polPod = New List(Of String) From {"POL1", "POD1"}
                'Else
                '    polPod = New List(Of String) From {"POL2", "POD2"}
                'End If

                '画面表示一覧データより同一のオーダー、連動更新対象の費目のレコードを絞り込み

                Dim updateRowItems = From dr As DataRow In dt
                                     Where Convert.ToString(dr.Item("ORDERNO")) = orderNo _
                                   AndAlso Convert.ToString(dr.Item("TANKSEQ")) = tankSeq _
                                   AndAlso Convert.ToString(dr.Item("DTLPOLPOD")) = dtlPolPod _
                                   AndAlso Convert.ToString(dr.Item("CHARGE_CLASS4")) = chargeClass4 _
                                   AndAlso {orgContractor, ""}.Contains(Convert.ToString(dr.Item("CONTRACTORFIX")))

                '絞り込んだ日付項目を更新
                For Each updateRowItem In updateRowItems
                    updateRowItem.Item(targetContractorField) = contValue
                    updateRowItem.Item(targetContractorNameField) = contractorName
                Next
            End If

        End If
        '自身の行の日付を編集
        afterInputRow.Item(targetContractorNameField) = contractorName
        If targetDt Is Nothing Then
            'サーバーローカルに保存
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021saveListTable()
            Me.SavedDt = dt
            If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                Return COA0021ListTable.ERR
            End If

        End If
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' データテーブルの対応する行について汎用補助区分を更新
    ''' </summary>
    ''' <returns></returns>
    Private Function UpdateDatatableAccCurrencySegment(accCurrencySegmentVal As String, lineCnt As String, Optional targetDt As DataTable = Nothing) As String
        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = Nothing

        '一覧表示データ復元
        If targetDt IsNot Nothing Then
            dt = targetDt
        ElseIf Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                Return COA0021ListTable.ERR
            End If
        Else
            dt = Me.SavedDt
        End If
        '日付変更対象の行を取得
        Dim targetRows = From dr As DataRow In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = lineCnt
        If targetRows.Any = False Then
            Return C_MESSAGENO.NORMAL
        End If

        targetRows(0)("ACCCURRENCYSEGMENT") = accCurrencySegmentVal

        If targetDt Is Nothing Then
            'サーバーローカルに保存
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021saveListTable()
            Me.SavedDt = dt
            If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                Return COA0021ListTable.ERR
            End If
        End If

        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' データテーブルの対応する行について精算月を更新
    ''' </summary>
    ''' <returns></returns>
    Private Function UpdateDatatableReportMonth(reportMonth As String, lineCnt As String, Optional targetDt As DataTable = Nothing) As String
        Dim COA0021ListTable As New COA0021ListTable
        Dim dt As DataTable = Nothing

        '一覧表示データ復元
        If targetDt IsNot Nothing Then
            dt = targetDt
        ElseIf Me.SavedDt Is Nothing Then
            dt = CreateOrderListTable()
            COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                dt = COA0021ListTable.OUTTBL
            Else
                Return COA0021ListTable.ERR
            End If
        Else
            dt = Me.SavedDt
        End If
        '日付変更対象の行を取得
        Dim targetRows = From dr As DataRow In dt
                         Where Convert.ToString(dr.Item("LINECNT")) = lineCnt
        If targetRows.Any = False Then
            Return C_MESSAGENO.NORMAL
        End If

        targetRows(0)("DEMREPORTMONTH") = reportMonth

        If targetDt Is Nothing Then
            'サーバーローカルに保存
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = dt
            COA0021ListTable.COA0021saveListTable()
            Me.SavedDt = dt
            If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
                Return COA0021ListTable.ERR
            End If
        End If

        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 費用項目マスタよりACTYNoをもとに連動して日付を更新する費用コード一覧を取得
    ''' </summary>
    ''' <param name="actyNo">ACTYNo</param>
    ''' <param name="polPod">POL or POD</param>
    ''' <param name="sqlCon">(省略可)SQL接続、既存の接続を使用する場合は指定</param>
    ''' <returns>連動する費用コード一覧</returns>
    ''' <remarks>UpdateDatatableDate関数から呼ばれる想定</remarks>
    Private Function GetIntarlockCostCodeFromActy(actyNo As String, polPod As String, Optional sqlCon As SqlConnection = Nothing) As List(Of CostActy)
        Dim canCloseConnect As Boolean = False
        Dim retList As List(Of CostActy)
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT COSTCODE")
            sqlStat.AppendLine("     , LDKBN")
            sqlStat.AppendLine("  FROM GBM0010_CHARGECODE")
            sqlStat.AppendLine(" WHERE COMPCODE  = @COMPCODE")
            sqlStat.AppendLine("   AND CLASS7 LIKE '%' + @CLASS7 + '%'")
            sqlStat.AppendLine("   AND (LDKBN = 'B' OR LDKBN = @LDKBN)")
            sqlStat.AppendLine("   AND STYMD    <= @STYMD")
            sqlStat.AppendLine("   AND ENDYMD   >= @ENDYMD")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
            Dim retDb As New DataTable
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@CLASS7", SqlDbType.NVarChar).Value = actyNo
                    .Add("@LDKBN", SqlDbType.NVarChar).Value = Right(polPod, 1)
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                End With
                '取得結果をDataTableに転送
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(retDb)
                End Using
                retList = (From item In retDb
                           Select New CostActy(Convert.ToString(item.Item("COSTCODE")) _
                                             , Convert.ToString(item.Item("LDKBN")))
                           ).ToList
            End Using
            Return retList
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
    ''' 内部データテーブルのタンクNoを更新
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <param name="tankSeq"></param>
    ''' <param name="tankNo"></param>
    ''' <param name="dt"></param>
    ''' <returns>更新後のデータテーブル</returns>
    Private Function UpdateDatatableTankNo(orderNo As String, tankSeq As String, tankNo As String, needsTankUseApply As Boolean, Optional dt As DataTable = Nothing, Optional dataid As String = "") As DataTable
        Dim COA0021ListTable As New COA0021ListTable
        '省略可能引数が未設定の場合はローカルファイルより取得
        If dt Is Nothing Then
            '一覧表示データ復元
            If Me.SavedDt Is Nothing Then
                dt = CreateOrderListTable()
                COA0021ListTable.FILEdir = hdnXMLsaveFile.Value
                COA0021ListTable.TBLDATA = dt
                COA0021ListTable.COA0021recoverListTable()
                If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                    dt = COA0021ListTable.OUTTBL
                Else
                    CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                    Return Nothing
                End If
            Else
                dt = Me.SavedDt
            End If
        End If
        Dim targetRows As EnumerableRowCollection(Of DataRow)

        If dataid <> "" Then
            targetRows = From dr As DataRow In dt
                         Where Convert.ToString(dr.Item("DATAID")) = dataid
        Else
            targetRows = From dr As DataRow In dt
                         Where Convert.ToString(dr.Item("TANKSEQ")) = tankSeq _
                       AndAlso Convert.ToString(dr.Item("ORDERNO")) = orderNo
        End If
        For Each targetRow In targetRows
            targetRow.Item("TANKNO") = tankNo
        Next
        Dim updDateString As String = ""
        If tankNo <> "" Then
            updDateString = Date.Now.ToString("yyyy/MM/dd")
        End If
        Dim tkalRow = (From dr As DataRow In dt
                       Where Convert.ToString(dr.Item("TANKSEQ")) = tankSeq _
                       AndAlso Convert.ToString(dr.Item("ORDERNO")) = orderNo _
                       AndAlso Convert.ToString(dr.Item("ACTIONID")) = "TKAL").FirstOrDefault
        If tkalRow IsNot Nothing Then
            tkalRow.Item("SCHEDELDATE") = updDateString
            If needsTankUseApply = False Then
                tkalRow.Item("ACTUALDATE") = updDateString
            End If
        End If
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = dt
        COA0021ListTable.COA0021saveListTable()
        Me.SavedDt = dt
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0021ListTable.ERR)})
            Return Nothing
        End If
        Return dt
    End Function
    ''' <summary>
    ''' DataRowのDBNullが入っているカラムを空白に置換
    ''' </summary>
    ''' <param name="dr"></param>
    Sub UpdateStringDbNullToBlank(dr As DataRow)
        If dr Is Nothing Then
            Return
        End If
        Dim dt As DataTable = dr.Table
        For Each col As DataColumn In dt.Columns
            If dr.Item(col.ColumnName) Is DBNull.Value Then
                dr.Item(col.ColumnName) = ""
            End If
        Next
    End Sub
    ''' <summary>
    ''' Excel4軸表を生成し（Payable to JOT、Receivable from JOT、Net settlement due to from ）を取得
    ''' </summary>
    ''' <param name="dt">SOA締め対象のORDERVALUEデータ</param>
    ''' <returns>各金額の値、NOTHINGの場合はエラー</returns>
    Private Function GetSummaryReportValues(dt As DataTable) As Dictionary(Of String, String)
        Dim colLists As New Dictionary(Of String, Integer) From {{CONST_CLOSINDDAY_FILEDNAME1, 0},
                                                                 {CONST_CLOSINDDAY_FILEDNAME2, 1},
                                                                 {CONST_CLOSINDDAY_FILEDNAME3, 2}}
        Dim retValue As Dictionary(Of String, String) = Nothing
        Dim outputDt As DataTable = CreateSummaryListTable()
        If dt Is Nothing OrElse dt.Rows Is Nothing OrElse dt.Rows.Count = 0 Then
            Dim writeDr As DataRow = outputDt.NewRow
            SetHeaderValue(writeDr, Me.lblClosingDate.Text)
            outputDt.Rows.Add(writeDr)
        Else
            Dim colNameList As New List(Of String)
            For Each colOb As DataColumn In dt.Columns
                If outputDt.Columns.Contains(colOb.ColumnName) Then
                    colNameList.Add(colOb.ColumnName)
                End If
            Next
            Dim actyNo As Integer = 0
            Dim orderNo As String = Convert.ToString(dt.Rows(0).Item("ORDERNO"))
            Dim tankSeq As String = Convert.ToString(dt.Rows(0).Item("TANKSEQ"))
            For Each readDr As DataRow In dt.Rows
                '同一カラム名を単純転送
                Dim writeDr As DataRow = outputDt.NewRow
                For Each colName In colNameList
                    If colName = "DISPSEQ" Then
                        writeDr.Item(colName) = Convert.ToString(readDr.Item(colName))
                    Else
                        writeDr.Item(colName) = readDr.Item(colName)
                    End If
                Next
                If Not (tankSeq.Equals(readDr.Item("TANKSEQ")) _
                    AndAlso orderNo.Equals(readDr.Item("ORDERNO"))) Then
                    actyNo = 0
                    orderNo = Convert.ToString(readDr.Item("ORDERNO"))
                    tankSeq = Convert.ToString(readDr.Item("TANKSEQ"))
                End If
                actyNo = actyNo + 1
                writeDr.Item("ACTYNO") = actyNo.ToString("000")
                writeDr.Item("REPORTMONTHH") = FormatDateContrySettings(FormatDateYMD(Me.lblClosingDate.Text, GBA00003UserSetting.DATEFORMAT), "yyyy/MM") & "/01"
                SetHeaderValue(writeDr, Me.lblClosingDate.Text)
                writeDr("REPORTMONTH") = readDr.Item("REPORTYMD")
                'If {"", "-"}.Contains(Convert.ToString(readDr.Item("REPORTYMDORG"))) Then
                '    writeDr("REPORTMONTHORG") = Convert.ToString(readDr.Item("REPORTYMD"))
                'Else
                '    writeDr("REPORTMONTHORG") = Convert.ToString(readDr.Item("REPORTYMDORG"))
                'End If
                outputDt.Rows.Add(writeDr)
            Next
        End If
        Dim outFilePath As String = ""
        Using outputDt
            '帳票出力
            With Nothing
                Dim COA0027ReportTable As New BASEDLL.COA0027ReportTable
                Dim reportId As String = "ReportList"
                Dim reportMapId As String = "GBT00018A"
                COA0027ReportTable.MAPID = reportMapId                             'PARAM01:画面ID
                COA0027ReportTable.REPORTID = reportId                             'PARAM02:帳票ID
                COA0027ReportTable.FILETYPE = "XLSX"                               'PARAM03:出力ファイル形式
                COA0027ReportTable.TBLDATA = outputDt                              'PARAM04:データ参照tabledata
                COA0027ReportTable.COA0027ReportTable()

                If Not COA0027ReportTable.ERR = C_MESSAGENO.NORMAL Then
                    CommonFunctions.ShowMessage(COA0027ReportTable.ERR, Me.lblFooterMessage, pageObject:=Me)
                    Return Nothing
                End If
                outFilePath = COA0027ReportTable.FILEpath
            End With

            Dim con As String = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=""{0}"";" &
                                              "Extended Properties = ""Excel 12.0 Xml;HDR=NO"";" _
                                              , outFilePath)
            Using sqlCon As New OleDb.OleDbConnection(con)
                sqlCon.Open()
                Dim sqlString As String = String.Format("select * from {0}", CONST_AXISCHART_RANGENAME)
                Using sqlAdp As New OleDb.OleDbDataAdapter(sqlString, sqlCon)
                    Dim retDt As New DataTable
                    sqlAdp.Fill(retDt)
                    retValue = New Dictionary(Of String, String)
                    For Each colItem As KeyValuePair(Of String, Integer) In colLists
                        Dim itemValue As String = "0"
                        If retDt.Rows.Count > colItem.Value Then
                            Dim drExcel As DataRow = retDt.Rows(colItem.Value)
                            itemValue = Convert.ToString(drExcel(0))
                        End If
                        retValue.Add(colItem.Key, itemValue)
                    Next

                End Using 'End OleDb.OleDbDataAdapter(sqlString, sqlCon)
            End Using 'End OleDb.OleDbConnection(con)
        End Using 'End outputDt
        Return retValue
    End Function
    ''' <summary>
    ''' ヘッダー設定
    ''' </summary>
    ''' <param name="writeDr"></param>
    ''' <param name="reportMonth"></param>
    Private Sub SetHeaderValue(ByRef writeDr As DataRow, reportMonth As String)

        Dim reportDate As String = FormatDateContrySettings(FormatDateYMD(reportMonth, GBA00003UserSetting.DATEFORMAT), "yyyy/MM") & "/01"
        Dim clData As DataTable = GetPrintClosingDate(Me.hdnCountry.Value, reportDate)
        If clData.Rows.Count > 0 Then
            With clData.Rows(0)
                writeDr.Item("OFFICENAMEH") = Convert.ToString(.Item("OFFICENAME"))
                writeDr.Item("APPLYUSERH") = Convert.ToString(.Item("APPLYUSER"))
                writeDr.Item("CURRENCYCODEH") = Convert.ToString(.Item("CURRENCYCODE"))
                writeDr.Item("LOCALRATEH") = Convert.ToString(.Item("LOCALRATE"))
                writeDr.Item("CLOSEDATEH") = Convert.ToString(.Item("CLOSEDATE"))
            End With
        End If
    End Sub

    ''' <summary>
    ''' BILLING CLOSEテーブル登録処理
    ''' </summary>
    ''' <param name="sqlCon"></param>
    ''' <param name="dicSummaryRep">SOA集計帳票に表示している集計値</param>
    ''' <param name="sqlTran"></param>
    ''' <returns></returns>
    Public Function EntryBillingClose(ByRef sqlCon As SqlConnection, dicSummaryRep As Dictionary(Of String, String), ByRef applyId As String, ByRef lastStep As String, Optional sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#) As String
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Dim eventCode As String = C_SCLOSEEVENT.APPLY

        '申請ID取得オブジェクトの生成
        Dim GBA00011ApplyID As New GBA00011ApplyID With {
                .COMPCODE = GBC_COMPCODE_D, 'COA0019Session.APSRVCamp,
                .SYSCODE = C_SYSCODE_GB,
                .KEYCODE = COA0019Session.APSRVname,
                .DIVISION = "S",
                .SEQOBJID = C_SQLSEQ.SCLOSEAPPLY,
                .SEQLEN = 6
                }
        '申請処理共通オブジェクトの生成
        Dim COA0032Apploval As New BASEDLL.COA0032Apploval With {
            .I_COMPCODE = COA0019Session.APSRVCamp,
            .I_MAPID = CONST_MAPID,
            .I_EVENTCODE = eventCode
        }
        '申請IDの取得
        GBA00011ApplyID.GBA00011getApplyID()
        If GBA00011ApplyID.ERR = C_MESSAGENO.NORMAL Then
            applyId = GBA00011ApplyID.APPLYID
        Else
            Return GBA00011ApplyID.ERR
        End If
        '申請登録(合わせてLastStep取得)
        Dim subCode = "" 'Convert.ToString(dr.Item("AGENTORGANIZER"))
        COA0032Apploval.I_APPLYID = applyId
        COA0032Apploval.I_SUBCODE = subCode
        COA0032Apploval.COA0032setApply()
        If COA0032Apploval.O_ERR = C_MESSAGENO.NORMAL Then
            lastStep = COA0032Apploval.O_LASTSTEP
        Else
            Return COA0032Apploval.O_ERR
        End If

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine(" UPDATE GBT0006_CLOSINGDAY ")
        sqlStat.AppendLine("    SET  DELFLG     = @DELFLG")
        sqlStat.AppendLine("       , UPDYMD     = @ENTYMD")
        sqlStat.AppendLine("       , UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("       , UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("    AND COUNTRYCODE = @COUNTRYCODE")
        sqlStat.AppendLine("    AND REPORTMONTH = @REPORTMONTH;")

        sqlStat.AppendLine("INSERT INTO GBT0006_CLOSINGDAY")
        sqlStat.AppendLine("     (")
        sqlStat.AppendLine("     COUNTRYCODE")
        sqlStat.AppendLine("   , STYMD")
        sqlStat.AppendLine("   , BILLINGYMD")
        sqlStat.AppendLine("   , REPORTMONTH")
        sqlStat.AppendLine("   , APPLYID")
        sqlStat.AppendLine("   , LASTSTEP")
        sqlStat.AppendLine("   , APPLYUSER")
        sqlStat.AppendLine("   , APPLYOFFICE")
        sqlStat.AppendLine("   , PAYABLE")
        sqlStat.AppendLine("   , RECEIVABLE")
        sqlStat.AppendLine("   , NETSETTLEMENTDUE")
        sqlStat.AppendLine("   , DELFLG")
        sqlStat.AppendLine("   , INITYMD")
        sqlStat.AppendLine("   , UPDYMD")
        sqlStat.AppendLine("   , UPDUSER")
        sqlStat.AppendLine("   , UPDTERMID")
        sqlStat.AppendLine("   , RECEIVEYMD")
        sqlStat.AppendLine("     )")
        sqlStat.AppendLine(" SELECT COUNTRYCODE")
        sqlStat.AppendLine("       ,@STYMD")
        sqlStat.AppendLine("       ,DATEADD(month,1,BILLINGYMD)")
        sqlStat.AppendLine("       ,@REPORTMONTH")
        sqlStat.AppendLine("       ,@APPLYID")
        sqlStat.AppendLine("       ,@LASTSTEP")
        sqlStat.AppendLine("       ,@APPLYUSER")
        sqlStat.AppendLine("       ,@APPLYOFFICE")
        'sqlStat.AppendLine("       ,@PAYABLE")
        'sqlStat.AppendLine("       ,@RECEIVABLE")
        'sqlStat.AppendLine("       ,@NETSETTLEMENTDUE")
        sqlStat.AppendLine("       ,CONVERT(DECIMAL(13,2),@PAYABLE)")
        sqlStat.AppendLine("       ,CONVERT(DECIMAL(13,2),@RECEIVABLE)")
        sqlStat.AppendLine("       ,CONVERT(DECIMAL(13,2),@NETSETTLEMENTDUE)")
        sqlStat.AppendLine("       ,'" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("       ,@ENTYMD")
        sqlStat.AppendLine("       ,@ENTYMD")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine("   FROM GBT0006_CLOSINGDAY")
        sqlStat.AppendLine("  WHERE DELFLG <> @DELFLG ")
        sqlStat.AppendLine("    AND COUNTRYCODE = @COUNTRYCODE ")
        sqlStat.AppendLine("    AND REPORTMONTH = (SELECT MAX(CDS.REPORTMONTH) ")
        sqlStat.AppendLine("                         FROM GBT0006_CLOSINGDAY CDS ")
        sqlStat.AppendLine("                        WHERE CDS.DELFLG <> @DELFLG")
        sqlStat.AppendLine("                          AND CDS.COUNTRYCODE = @COUNTRYCODE")
        sqlStat.AppendLine("                      )")


        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@STYMD", SqlDbType.Date).Value = procDate
                .Add("@ENTYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = FormatDateContrySettings(FormatDateYMD(Me.lblClosingDate.Text.Trim(), GBA00003UserSetting.DATEYMFORMAT), "yyyy/MM")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                If GBA00003UserSetting.IS_JOTUSER Then
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = GBC_JOT_SOA_COUNTRY
                Else
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Me.hdnCountry.Value 'Me.hdnUserCountry.Value
                End If
                .Add("@APPLYID", SqlDbType.NVarChar).Value = applyId
                .Add("@LASTSTEP", SqlDbType.NVarChar).Value = lastStep
                .Add("@APPLYUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@APPLYOFFICE", SqlDbType.NVarChar).Value = Me.hdnUserOffice.Value
                .Add("@PAYABLE", SqlDbType.NVarChar).Value = DecimalStringToDecimal(dicSummaryRep(CONST_CLOSINDDAY_FILEDNAME1))
                .Add("@RECEIVABLE", SqlDbType.NVarChar).Value = DecimalStringToDecimal(dicSummaryRep(CONST_CLOSINDDAY_FILEDNAME2))
                .Add("@NETSETTLEMENTDUE", SqlDbType.NVarChar).Value = DecimalStringToDecimal(dicSummaryRep(CONST_CLOSINDDAY_FILEDNAME3))
            End With
            sqlCmd.ExecuteNonQuery()
        End Using


        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 印刷時に締め日情報を再取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <returns></returns>
    Private Function GetPrintClosingDate(ByVal countryCode As String, ByVal reportDate As String) As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        Static retResult As Dictionary(Of String, DataTable)
        If retResult Is Nothing Then
            retResult = New Dictionary(Of String, DataTable)
        End If
        Dim keyString As String = countryCode & "@" & reportDate
        If retResult.ContainsKey(keyString) Then
            retDt = retResult(keyString)
            Return retDt
        End If
        'SQL文作成
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(TR.NAMELJP,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(TR.NAMEL,'') END As OFFICENAME")
        sqlStat.AppendLine("      ,CASE WHEN @LANGDISP = '" & C_LANG.JA & "' THEN ISNULL(USN.STAFFNAMES,'') ")
        sqlStat.AppendLine("            WHEN @LANGDISP = '" & C_LANG.EN & "' THEN ISNULL(USN.STAFFNAMES_EN,'') END As APPLYUSER")
        sqlStat.AppendLine("      ,ISNULL(EX.CURRENCYCODE,'') AS CURRENCYCODE")
        sqlStat.AppendLine("      ,ISNULL(EX.EXRATE,'') AS LOCALRATE")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(CL.APPLYID,'') = '' THEN '' ELSE FORMAT(CL.UPDYMD,'yyyy/MM/dd HH:mm') END AS CLOSEDATE")

        sqlStat.AppendLine("  FROM GBT0006_CLOSINGDAY CL")
        sqlStat.AppendLine("  INNER JOIN COT0002_APPROVALHIST AP")
        sqlStat.AppendLine("          ON CL.APPLYID  = AP.APPLYID ")
        sqlStat.AppendLine("         AND CL.LASTSTEP = AP.STEP")
        sqlStat.AppendLine("         AND AP.STATUS IN ('" & C_APP_STATUS.APPROVED & "','" & C_APP_STATUS.COMPLETE & "')")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR") '業者名称用JOIN
        sqlStat.AppendLine("    ON  TR.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  TR.CARRIERCODE  = CL.APPLYOFFICE")
        sqlStat.AppendLine("   AND  TR.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  TR.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  TR.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0020_EXRATE EX") '通貨用JOIN
        sqlStat.AppendLine("    ON  EX.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  EX.COUNTRYCODE  = CL.COUNTRYCODE")
        sqlStat.AppendLine("   AND  EX.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  EX.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  EX.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  EX.TARGETYM     = DateAdd(Day, 1 - DatePart(Day, @TARGETYM), @TARGETYM)")

        sqlStat.AppendLine("  LEFT JOIN COS0005_USER USN") 'ユーザー名用JOIN
        sqlStat.AppendLine("    ON  USN.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  USN.USERID       = CL.APPLYUSER")
        sqlStat.AppendLine("   AND  USN.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND  USN.ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   AND  USN.DELFLG      <> @DELFLG")

        sqlStat.AppendLine(" WHERE CL.COUNTRYCODE = @COUNTRYCODE")
        sqlStat.AppendLine("   AND CL.REPORTMONTH = @REPORTMONTH")
        sqlStat.AppendLine("   AND CL.DELFLG     <> @DELFLG")

        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = countryCode
                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = FormatDateContrySettings(FormatDateYMD(reportDate, GBA00003UserSetting.DATEFORMAT), "yyyy/MM")
                .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@LANGDISP", SqlDbType.NVarChar).Value = COA0019Session.LANGDISP
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@TARGETYM", SqlDbType.Date).Value = Date.Parse(reportDate)
            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(retDt)
            End Using
        End Using
        retResult.Add(keyString, retDt)
        Return retDt
    End Function
    ''' <summary>
    ''' JOTSOAテーブルにデータ登録
    ''' </summary>
    ''' <param name="dt">登録対象データテーブル</param>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト</param>
    ''' <returns></returns>
    Public Function EntryJotSoaValue(dt As DataTable, ByRef sqlCon As SqlConnection, Optional sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#) As String
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Dim procGroup As String = "JOT"
        If Me.hdnInvoicedBy.Value <> "OJ" Then
            procGroup = Me.hdnCountry.Value
        End If
        Dim closingDate As String = FormatDateContrySettings(FormatDateYMD(Me.lblClosingDate.Text.Trim(), GBA00003UserSetting.DATEYMFORMAT), "yyyy/MM") 'Me.hdnUserCountry.Value
        Dim sqlStatThisMonthClear As New StringBuilder
        'JOTのエージェントを取得(INVOICED BYで判定用)
        sqlStatThisMonthClear.AppendLine("WITH ")
        sqlStatThisMonthClear.AppendLine(" W_JOTAGENT AS (") 'START 
        sqlStatThisMonthClear.AppendLine("   SELECT TR.CARRIERCODE")
        sqlStatThisMonthClear.AppendLine("     FROM GBM0005_TRADER TR")
        sqlStatThisMonthClear.AppendLine("    WHERE TR.STYMD  <= @NOWDATE")
        sqlStatThisMonthClear.AppendLine("      AND TR.ENDYMD >= @NOWDATE")
        sqlStatThisMonthClear.AppendLine("      AND TR.DELFLG <> @DELFLG")
        sqlStatThisMonthClear.AppendLine("      AND EXISTS (SELECT 1")
        sqlStatThisMonthClear.AppendLine("                    FROM COS0017_FIXVALUE FXV")
        sqlStatThisMonthClear.AppendLine("                   WHERE FXV.COMPCODE   = 'Default'")
        sqlStatThisMonthClear.AppendLine("                     AND FXV.SYSCODE    = 'GB'")
        sqlStatThisMonthClear.AppendLine("                     AND FXV.CLASS      = 'JOTCOUNTRYORG'")
        sqlStatThisMonthClear.AppendLine("                     AND FXV.KEYCODE     = TR.MORG")
        sqlStatThisMonthClear.AppendLine("                     AND FXV.STYMD     <= @NOWDATE")
        sqlStatThisMonthClear.AppendLine("                     AND FXV.ENDYMD    >= @NOWDATE")
        sqlStatThisMonthClear.AppendLine("                     AND FXV.DELFLG    <> @DELFLG")
        sqlStatThisMonthClear.AppendLine("                 )")
        sqlStatThisMonthClear.AppendLine(")")

        sqlStatThisMonthClear.AppendLine("UPDATE GBT0008_JOTSOA_VALUE")
        sqlStatThisMonthClear.AppendLine("    SET  DELFLG     = @DELFLG")
        sqlStatThisMonthClear.AppendLine("       , UPDYMD     = @ENTYMD")
        sqlStatThisMonthClear.AppendLine("       , UPDUSER    = @UPDUSER")
        sqlStatThisMonthClear.AppendLine("       , UPDTERMID  = @UPDTERMID")
        sqlStatThisMonthClear.AppendLine("       , RECEIVEYMD = @RECEIVEYMD")
        sqlStatThisMonthClear.AppendLine("  WHERE DELFLG      = '" & CONST_FLAG_NO & "'")
        sqlStatThisMonthClear.AppendLine("    AND CLOSINGMONTH = @REPORTMONTH")
        sqlStatThisMonthClear.AppendLine("    AND CLOSINGGROUP = @CLOSINGGROUP")
        If Me.hdnInvoicedBy.Value <> "" Then
            'INVOICEDBYTYPE
            Select Case Me.hdnInvoicedBy.Value
                Case "OJ" 'JOTのみ
                    sqlStatThisMonthClear.AppendLine("  AND INVOICEDBY    IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) ")
                Case "IJ" 'JOT含む '無条件と同じ
                    'sqlStatThisMonthClear.AppendLine("    AND COUNTRYCODE = @COUNTRYCODE")
                    sqlStatThisMonthClear.AppendLine("  AND EXISTS ( SELECT 1 ")
                    sqlStatThisMonthClear.AppendLine("                 FROM GBM0005_TRADER TRINV")
                    sqlStatThisMonthClear.AppendLine("                WHERE TRINV.COMPCODE = @COMPCODE")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.COUNTRYCODE = @COUNTRYCODE")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.CARRIERCODE = INVOICEDBY")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.DELFLG <> @DELFLG")
                    sqlStatThisMonthClear.AppendLine("             )")
                Case "EJ" 'JOT含まない
                    'sqlStatThisMonthClear.AppendLine("    AND COUNTRYCODE = @COUNTRYCODE")
                    sqlStatThisMonthClear.AppendLine("  AND EXISTS ( SELECT 1 ")
                    sqlStatThisMonthClear.AppendLine("                 FROM GBM0005_TRADER TRINV")
                    sqlStatThisMonthClear.AppendLine("                WHERE TRINV.COMPCODE = @COMPCODE")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.COUNTRYCODE = @COUNTRYCODE")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.CARRIERCODE = INVOICEDBY")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.DELFLG <> @DELFLG")
                    sqlStatThisMonthClear.AppendLine("             )")
                    sqlStatThisMonthClear.AppendLine("  AND INVOICEDBY    NOT IN (SELECT JOTA.CARRIERCODE FROM W_JOTAGENT JOTA) ")
                Case Else
                    sqlStatThisMonthClear.AppendLine("  AND EXISTS ( SELECT 1 ")
                    sqlStatThisMonthClear.AppendLine("                 FROM GBM0005_TRADER TRINV")
                    sqlStatThisMonthClear.AppendLine("                WHERE TRINV.COMPCODE = @COMPCODE")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.COUNTRYCODE = @COUNTRYCODE")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.CARRIERCODE = INVOICEDBY")
                    sqlStatThisMonthClear.AppendLine("                  AND TRINV.DELFLG <> @DELFLG")
                    sqlStatThisMonthClear.AppendLine("             )")
            End Select
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("INSERT INTO GBT0008_JOTSOA_VALUE")
        sqlStat.AppendLine(" (")
        sqlStat.AppendLine("   ORDERNO")
        sqlStat.AppendLine("  ,STYMD")
        sqlStat.AppendLine("  ,ENDYMD")
        sqlStat.AppendLine("  ,TANKSEQ")
        sqlStat.AppendLine("  ,DTLPOLPOD")
        sqlStat.AppendLine("  ,DTLOFFICE")
        sqlStat.AppendLine("  ,TANKNO")
        sqlStat.AppendLine("  ,COSTCODE")
        sqlStat.AppendLine("  ,ACTIONID")
        sqlStat.AppendLine("  ,DISPSEQ")
        sqlStat.AppendLine("  ,LASTACT")
        sqlStat.AppendLine("  ,REQUIREDACT")
        sqlStat.AppendLine("  ,ORIGINDESTINATION")
        sqlStat.AppendLine("  ,COUNTRYCODE")
        sqlStat.AppendLine("  ,CURRENCYCODE")
        sqlStat.AppendLine("  ,TAXATION")
        sqlStat.AppendLine("  ,AMOUNTBR")
        sqlStat.AppendLine("  ,AMOUNTORD")
        sqlStat.AppendLine("  ,AMOUNTFIX")
        sqlStat.AppendLine("  ,CONTRACTORBR")
        sqlStat.AppendLine("  ,CONTRACTORODR")
        sqlStat.AppendLine("  ,CONTRACTORFIX")
        sqlStat.AppendLine("  ,SCHEDELDATEBR")
        sqlStat.AppendLine("  ,SCHEDELDATE")
        sqlStat.AppendLine("  ,ACTUALDATE")
        sqlStat.AppendLine("  ,LOCALBR")
        sqlStat.AppendLine("  ,LOCALRATE")
        sqlStat.AppendLine("  ,AMOUNTPAYODR")
        sqlStat.AppendLine("  ,LOCALPAYODR")
        sqlStat.AppendLine("  ,TAXBR")
        sqlStat.AppendLine("  ,LOCALRATESOA")
        sqlStat.AppendLine("  ,AMOUNTPAY")
        sqlStat.AppendLine("  ,LOCALPAY")
        sqlStat.AppendLine("  ,TAXPAY")

        sqlStat.AppendLine("  ,UAG_USD")
        sqlStat.AppendLine("  ,UAG_LOCAL")
        sqlStat.AppendLine("  ,USD_USD")
        sqlStat.AppendLine("  ,USD_LOCAL")
        sqlStat.AppendLine("  ,LOCAL_USD")
        sqlStat.AppendLine("  ,LOCAL_LOCAL")

        sqlStat.AppendLine("  ,INVOICEDBY")
        sqlStat.AppendLine("  ,APPLYID")
        sqlStat.AppendLine("  ,APPLYTEXT")
        sqlStat.AppendLine("  ,LASTSTEP")
        sqlStat.AppendLine("  ,SOAAPPDATE")
        sqlStat.AppendLine("  ,REMARK")
        sqlStat.AppendLine("  ,BRID")
        sqlStat.AppendLine("  ,BRCOST")
        sqlStat.AppendLine("  ,DATEFIELD")
        sqlStat.AppendLine("  ,DATEINTERVAL")
        sqlStat.AppendLine("  ,BRADDEDCOST")
        sqlStat.AppendLine("  ,AGENTORGANIZER")
        sqlStat.AppendLine("  ,SOACODE")
        sqlStat.AppendLine("  ,SOASHORTCODE")
        sqlStat.AppendLine("  ,REPORTMONTH")
        sqlStat.AppendLine("  ,REPORTMONTHORG")
        sqlStat.AppendLine("  ,CLOSINGMONTH")
        sqlStat.AppendLine("  ,CLOSINGGROUP")
        sqlStat.AppendLine("  ,SHIPDATE")
        sqlStat.AppendLine("  ,DOUTDATE")
        sqlStat.AppendLine("  ,DATAIDODR")
        sqlStat.AppendLine("  ,DELFLG")
        sqlStat.AppendLine("  ,INITYMD")
        sqlStat.AppendLine("  ,UPDYMD")
        sqlStat.AppendLine("  ,UPDUSER")
        sqlStat.AppendLine("  ,UPDTERMID")
        sqlStat.AppendLine("  ,RECEIVEYMD")
        sqlStat.AppendLine(" )")
        sqlStat.AppendLine(" SELECT ")
        sqlStat.AppendLine("   VL.ORDERNO")
        sqlStat.AppendLine("  ,VL.STYMD")
        sqlStat.AppendLine("  ,VL.ENDYMD")
        sqlStat.AppendLine("  ,VL.TANKSEQ")
        sqlStat.AppendLine("  ,VL.DTLPOLPOD")
        sqlStat.AppendLine("  ,VL.DTLOFFICE")
        sqlStat.AppendLine("  ,VL.TANKNO")
        sqlStat.AppendLine("  ,VL.COSTCODE")
        sqlStat.AppendLine("  ,VL.ACTIONID")
        sqlStat.AppendLine("  ,VL.DISPSEQ")
        sqlStat.AppendLine("  ,VL.LASTACT")
        sqlStat.AppendLine("  ,VL.REQUIREDACT")
        sqlStat.AppendLine("  ,VL.ORIGINDESTINATION")
        sqlStat.AppendLine("  ,VL.COUNTRYCODE")
        sqlStat.AppendLine("  ,VL.CURRENCYCODE")
        sqlStat.AppendLine("  ,VL.TAXATION")
        sqlStat.AppendLine("  ,VL.AMOUNTBR")
        sqlStat.AppendLine("  ,VL.AMOUNTORD")
        sqlStat.AppendLine("  ,VL.AMOUNTFIX")
        sqlStat.AppendLine("  ,VL.CONTRACTORBR")
        sqlStat.AppendLine("  ,VL.CONTRACTORODR")
        sqlStat.AppendLine("  ,VL.CONTRACTORFIX")
        sqlStat.AppendLine("  ,VL.SCHEDELDATEBR")
        sqlStat.AppendLine("  ,VL.SCHEDELDATE")
        sqlStat.AppendLine("  ,VL.ACTUALDATE")
        sqlStat.AppendLine("  ,VL.LOCALBR")
        sqlStat.AppendLine("  ,VL.LOCALRATE")
        sqlStat.AppendLine("  ,@AMOUNTPAYODR")
        sqlStat.AppendLine("  ,@LOCALPAYODR")
        sqlStat.AppendLine("  ,VL.TAXBR")
        sqlStat.AppendLine("  ,@LOCALRATESOA")
        sqlStat.AppendLine("  ,@AMOUNTPAY")
        sqlStat.AppendLine("  ,@LOCALPAY")
        sqlStat.AppendLine("  ,VL.TAXPAY")

        sqlStat.AppendLine("  ,@UAG_USD")
        sqlStat.AppendLine("  ,@UAG_LOCAL")
        sqlStat.AppendLine("  ,@USD_USD")
        sqlStat.AppendLine("  ,@USD_LOCAL")
        sqlStat.AppendLine("  ,@LOCAL_USD")
        sqlStat.AppendLine("  ,@LOCAL_LOCAL")

        sqlStat.AppendLine("  ,VL.INVOICEDBY")
        sqlStat.AppendLine("  ,''")    'APPLYID
        sqlStat.AppendLine("  ,''")    'APPLYTEXT
        sqlStat.AppendLine("  ,''")    'LASTSTEP
        sqlStat.AppendLine("  ,VL.SOAAPPDATE")
        sqlStat.AppendLine("  ,VL.REMARK")
        sqlStat.AppendLine("  ,VL.BRID")
        sqlStat.AppendLine("  ,VL.BRCOST")
        sqlStat.AppendLine("  ,VL.DATEFIELD")
        sqlStat.AppendLine("  ,VL.DATEINTERVAL")
        sqlStat.AppendLine("  ,VL.BRADDEDCOST")
        sqlStat.AppendLine("  ,VL.AGENTORGANIZER")
        sqlStat.AppendLine("  ,ISNULL(CC.SOACODE,'') AS SOACODE")
        sqlStat.AppendLine("  ,LEFT(ISNULL(CC.SOACODE,''),3) AS SOASHORTCODE")
        sqlStat.AppendLine("  ,@REPORTMONTH")
        sqlStat.AppendLine("  ,@REPORTMONTHORG")
        sqlStat.AppendLine("  ,@CLOSINGMONTH")
        sqlStat.AppendLine("  ,@CLOSINGGROUP")
        sqlStat.AppendLine("  ,@SHIPDATE")
        sqlStat.AppendLine("  ,@DOUTDATE")
        sqlStat.AppendLine("  ,VL.DATAID")
        sqlStat.AppendLine("  ,VL.DELFLG")
        sqlStat.AppendLine("  ,@ENTYMD") 'INITYMD
        sqlStat.AppendLine("  ,@ENTYMD")  'UPDYMD
        sqlStat.AppendLine("  ,@UPDUSER")
        sqlStat.AppendLine("  ,@UPDTERMID")
        sqlStat.AppendLine("  ,@RECEIVEYMD")
        sqlStat.AppendLine("   FROM GBT0005_ODR_VALUE VL")
        sqlStat.AppendLine("   LEFT JOIN GBM0010_CHARGECODE CC")
        sqlStat.AppendLine("     ON CC.COSTCODE = VL.COSTCODE")
        sqlStat.AppendLine("    AND '1' = CASE WHEN VL.ORDERNO LIKE 'NB%'    AND CC.NONBR = '" & CONST_FLAG_NO & "' THEN '0'")
        sqlStat.AppendLine("                   WHEN VL.DTLPOLPOD LIKE 'POL%' AND CC.LDKBN IN ('B','L') THEN '1' ")
        sqlStat.AppendLine("                   WHEN VL.DTLPOLPOD LIKE 'POD%' AND CC.LDKBN IN ('B','D') THEN '1' ")
        sqlStat.AppendLine("                   WHEN VL.DTLPOLPOD LIKE 'Organizer' AND CC.LDKBN IN ('D') THEN '' ")
        sqlStat.AppendLine("                   WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
        sqlStat.AppendLine("                   ELSE '1'")
        sqlStat.AppendLine("              END")
        sqlStat.AppendLine("    AND CC.STYMD  <= getdate() ")
        sqlStat.AppendLine("    AND CC.ENDYMD >= getdate() ")
        sqlStat.AppendLine("    AND CC.DELFLG <> @DELFLG")
        sqlStat.AppendLine("  WHERE VL.DATAID  = @DATAID ")
        sqlStat.AppendLine("    AND VL.DELFLG <> @DELFLG")
        Using sqlCmd As New SqlCommand(sqlStatThisMonthClear.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Me.hdnCountry.Value 'Me.hdnUserCountry.Value
                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = FormatDateContrySettings(FormatDateYMD(Me.lblClosingDate.Text.Trim(), GBA00003UserSetting.DATEYMFORMAT), "yyyy/MM") 'Me.hdnUserCountry.Value
                .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = procGroup
                .Add("@NOWDATE", SqlDbType.DateTime).Value = Now
            End With
            sqlCmd.ExecuteNonQuery()
        End Using
        '転記すべきレコードが無ければ前回分のみを除去
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        '転記登録
        For Each dr As DataRow In dt.Rows
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
                With sqlCmd.Parameters
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@DATAID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DATAID"))
                    .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REPORTYMD"))
                    If {"", "-"}.Contains(Convert.ToString(dr.Item("REPORTYMDORG"))) Then
                        .Add("@REPORTMONTHORG", SqlDbType.NVarChar).Value = "-" 'Convert.ToString(dr.Item("REPORTYMD"))
                    Else
                        .Add("@REPORTMONTHORG", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REPORTYMDORG"))
                    End If
                    .Add("@AMOUNTPAY", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("USDAMOUNT")))
                    .Add("@LOCALPAY", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCALAMOUNT")))

                    .Add("@AMOUNTPAYODR", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMOUNTPAYODR")))
                    .Add("@LOCALPAYODR", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCALPAYODR")))
                    .Add("@LOCALRATESOA", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCALRATESOA")))

                    .Add("@UAG_USD", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("UAG_USD")))
                    .Add("@UAG_LOCAL", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("UAG_LOCAL")))
                    .Add("@USD_USD", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("USD_USD")))
                    .Add("@USD_LOCAL", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("USD_LOCAL")))
                    .Add("@LOCAL_USD", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCAL_USD")))
                    .Add("@LOCAL_LOCAL", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCAL_LOCAL")))

                    .Add("@CLOSINGMONTH", SqlDbType.NVarChar).Value = closingDate
                    .Add("@CLOSINGGROUP", SqlDbType.NVarChar).Value = procGroup
                    .Add("@SHIPDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("SHIPDATE")))
                    .Add("@DOUTDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("DOUTDATE")))



                    .Add("@ENTYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                End With
                sqlCmd.ExecuteNonQuery()
            End Using
        Next
        Return C_MESSAGENO.NORMAL 'ここまでくれば正常
    End Function
    ''' <summary>
    ''' JOTSOABASEテーブルにデータ登録
    ''' </summary>
    ''' <param name="orderNoList">登録対象のOrderNoリスト</param>
    ''' <param name="sqlCon">接続オブジェクト</param>
    ''' <param name="sqlTran">トランザクションオブジェクト</param>
    ''' <returns></returns>
    Public Function EntryJotSoaBase(orderNoList As List(Of String), reportMonth As String, ByRef sqlCon As SqlConnection, Optional sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#) As String
        If orderNoList Is Nothing OrElse orderNoList.Count = 0 Then
            Return C_MESSAGENO.NORMAL '転記すべきセールスブレーカーのオーダーが無い場合終了
        End If

        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("UPDATE GBT0013_JOTSOA_BASE")
        sqlStat.AppendLine("   SET DELFLG = @DELFLG")
        sqlStat.AppendLine("      ,UPDYMD     = @ENTYMD")
        sqlStat.AppendLine("      ,UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine(" WHERE REPORTMONTH = @REPORTMONTH")
        sqlStat.AppendLine("   AND ORDERNO     = @ORDERNO")
        sqlStat.AppendLine("   AND DELFLG     <> @DELFLG")
        sqlStat.AppendLine(";")
        sqlStat.AppendLine("INSERT INTO GBT0013_JOTSOA_BASE")
        sqlStat.AppendLine(" (")
        sqlStat.AppendLine("   REPORTMONTH")
        sqlStat.AppendLine("  ,ORDERNO")
        sqlStat.AppendLine("  ,STYMD")
        sqlStat.AppendLine("  ,LOADING")
        sqlStat.AppendLine("  ,STEAMING")
        sqlStat.AppendLine("  ,TIP")
        sqlStat.AppendLine("  ,EXTRA")
        sqlStat.AppendLine("  ,EXSHIPRATE1")
        sqlStat.AppendLine("  ,INSHIPRATE1")
        sqlStat.AppendLine("  ,EXSHIPRATE2")
        sqlStat.AppendLine("  ,INSHIPRATE2")
        sqlStat.AppendLine("  ,DELFLG")
        sqlStat.AppendLine("  ,INITUSER")
        sqlStat.AppendLine("  ,INITYMD")
        sqlStat.AppendLine("  ,UPDYMD")
        sqlStat.AppendLine("  ,UPDUSER")
        sqlStat.AppendLine("  ,UPDTERMID")
        sqlStat.AppendLine("  ,RECEIVEYMD")
        sqlStat.AppendLine(" )")
        sqlStat.AppendLine(" SELECT ")
        sqlStat.AppendLine("        @REPORTMONTH AS REPORTMONTH")
        sqlStat.AppendLine("       ,BS.ORDERNO")
        sqlStat.AppendLine("       ,@ENTYMD AS STYMD")
        sqlStat.AppendLine("       ,BS.LOADING")
        sqlStat.AppendLine("       ,BS.STEAMING")
        sqlStat.AppendLine("       ,BS.TIP")
        sqlStat.AppendLine("       ,BS.EXTRA")
        sqlStat.AppendLine("       ,ISNULL(T1VAL.EXSHIPRATE,0)  AS EXSHIPRATE1")
        sqlStat.AppendLine("       ,ISNULL(T1VAL.INSHIPRATE,0)  AS INSHIPRATE1")
        sqlStat.AppendLine("       ,ISNULL(T2VAL.EXSHIPRATE,0)  AS EXSHIPRATE2")
        sqlStat.AppendLine("       ,ISNULL(T2VAL.INSHIPRATE,0)  AS INSHIPRATE2")
        sqlStat.AppendLine("       ,@DELFLG_NO        AS DELFLG")
        sqlStat.AppendLine("       ,@UPDUSER          AS INITUSER")
        sqlStat.AppendLine("       ,@ENTYMD           AS INITYMD")
        sqlStat.AppendLine("       ,@ENTYMD           AS UPDYMD")
        sqlStat.AppendLine("       ,@UPDUSER          AS UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendLine("   FROM GBT0004_ODR_BASE BS")
        '第一輸送分の船社レコード取得用JOIN
        sqlStat.AppendLine("   LEFT JOIN GBT0007_ODR_VALUE2 T1VAL")
        sqlStat.AppendLine("     ON T1VAL.ORDERNO     = BS.ORDERNO")
        sqlStat.AppendLine("    AND T1VAL.TANKSEQ     = '001'")
        sqlStat.AppendLine("    AND T1VAL.TRILATERAL  = '1'")
        sqlStat.AppendLine("    AND T1VAL.DELFLG     <> @DELFLG")
        '第二輸送分の船社レコード取得用JOIN
        sqlStat.AppendLine("   LEFT JOIN GBT0007_ODR_VALUE2 T2VAL")
        sqlStat.AppendLine("     ON T2VAL.ORDERNO     = BS.ORDERNO")
        sqlStat.AppendLine("    AND T2VAL.TANKSEQ     = '001'")
        sqlStat.AppendLine("    AND T2VAL.TRILATERAL  = '2'")
        sqlStat.AppendLine("    AND T2VAL.DELFLG     <> @DELFLG")
        sqlStat.AppendLine("  WHERE BS.ORDERNO = @ORDERNO")
        sqlStat.AppendLine("    AND BS.DELFLG <> @DELFLG")
        sqlStat.AppendLine(";")
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@ENTYMD", SqlDbType.DateTime).Value = procDate.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                .Add("@REPORTMONTH", SqlDbType.NVarChar).Value = reportMonth
                .Add("@DELFLG_NO", SqlDbType.NVarChar).Value = CONST_FLAG_NO
            End With
            Dim paramOrderNo = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar)

            For Each orderNo In orderNoList
                paramOrderNo.Value = orderNo
                sqlCmd.ExecuteNonQuery()
            Next

        End Using
        Return C_MESSAGENO.NORMAL 'ここまでくれば正常
    End Function
    ''' <summary>
    ''' 左の出力帳票選択肢設定
    ''' </summary>
    ''' <returns>メッセージNo</returns>
    Private Function RightboxInit() As String
        Dim retVal As String = C_MESSAGENO.NORMAL
        Dim excelMapId As String = CONST_MAPID

        'RightBOX情報設定
        Dim COA0016VARIget As New BASEDLL.COA0016VARIget
        Dim COA0022ProfXls As New BASEDLL.COA0022ProfXls
        retVal = C_MESSAGENO.NORMAL

        '初期化
        'Me.txtRightErrorMessage.Text = ""

        'レポートID情報
        COA0022ProfXls.MAPID = excelMapId & Me.hdnListMapVariant.Value
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
        COA0016VARIget.MAPID = excelMapId & Me.hdnListMapVariant.Value
        COA0016VARIget.COMPCODE = GBC_COMPCODE_D
        COA0016VARIget.VARI = "Default"
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
    ''' 禁則文字置換
    ''' </summary>
    ''' <param name="targetObjects">対象オブジェクト（テキストボックスリスト or データテーブル)</param>
    ''' <param name="columnList">置換対象カラム一覧(データテーブル時のみ指定)</param>
    Private Sub ChangeInvalidChar(targetObjects As Object, Optional columnList As List(Of String) = Nothing)
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get
        'テキストボックスの全置換
        If TypeOf targetObjects Is List(Of TextBox) Then
            Dim targetTextboxList As List(Of TextBox) = DirectCast(targetObjects, List(Of TextBox))
            For Each targetTextbox In targetTextboxList
                With COA0008InvalidChar
                    .CHARin = targetTextbox.Text
                    .COA0008RemoveInvalidChar()
                    If .CHARin <> .CHARout Then
                        targetTextbox.Text = .CHARout
                    End If
                End With
            Next
        End If
        'データテーブルの格納値置換
        If TypeOf targetObjects Is DataTable Then
            If columnList Is Nothing OrElse columnList.Count = 0 Then
                '引数置換対象のカラムがない場合はそのまま終了
                Return
            End If
            Dim dt As DataTable = DirectCast(targetObjects, DataTable)
            'データテーブルがないまたはレコードがない場合はそのまま終了
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                Return
            End If
            '引数カラムリストのうち引数データテーブルに存在するカラムに限定
            Dim changeValueColumnList As New List(Of String)
            For Each columnName As String In columnList
                If dt.Columns.Contains(columnName) Then
                    changeValueColumnList.Add(columnName)
                End If
            Next
            'データテーブルとのカラム名マッチングの結果,
            '置換対象のカラムが存在しない場合はそのまま終了
            If changeValueColumnList.Count = 0 Then
                Return
            End If
            'データ行のループ
            For Each dr As DataRow In dt.Rows
                'カラム名のループ
                For Each columnName As String In changeValueColumnList
                    With COA0008InvalidChar
                        .CHARin = Convert.ToString(dr.Item(columnName))
                        .COA0008RemoveInvalidChar()
                        If .CHARin <> .CHARout Then
                            dr.Item(columnName) = .CHARout
                        End If
                    End With
                Next 'カラム名のループEND
            Next 'データ行のループEND

        End If
    End Sub
    ''' <summary>
    ''' 変更検知処理
    ''' </summary>
    ''' <param name="splitTankMod">タンク引当の変更も加味して分離したデータテーブルを返却するかTrue(省略時):分離する,False:省略しない</param>
    ''' <returns>変更対象のデータテーブルを生成</returns>
    ''' <remarks>当処理の戻り値データテーブルが更新・追加・論理削除対象のデータとなる</remarks>
    Private Function GetModifiedDataTable(dicDemurrageCalcField As Dictionary(Of String, List(Of String)), Optional splitTankMod As Boolean = True, Optional withoutApply As Boolean = False, Optional withoutRemark As Boolean = False) As Dictionary(Of ModifyType, List(Of DataRow))
        Dim COA0021ListTable As New COA0021ListTable
        Dim retDt As DataTable = CreateOrderListTable()
        retDt.Columns.Add("MODIFIED", GetType(ModifyType))
        Dim currentDt As DataTable
        Dim firstTimeDt As DataTable
        '**************************************************
        'データテーブル復元
        '**************************************************
        '画面編集しているデータテーブル取得
        If Me.SavedDt Is Nothing Then
            currentDt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = currentDt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                currentDt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return Nothing
            End If

        Else
            currentDt = Me.SavedDt
        End If
        '画面ロード時に退避した編集前のデータテーブル取得
        With Nothing
            firstTimeDt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
            COA0021ListTable.TBLDATA = firstTimeDt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                firstTimeDt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return Nothing
            End If
        End With
        '**************************************************
        '各種動作を行うデータ一覧の生成
        '**************************************************
        '最終的に返却する形式を格納するデータ
        Dim correctDataLists As New Dictionary(Of ModifyType, List(Of DataRow))
        Dim modifiedTankSeqList As New HashSet(Of String) 'タンクNoが変更されたTankSeqを保持(Insert、Delete判定時も変更を加味するため) 後続のContainsの速度を考慮しHashSet
        '1.更新分（現在のデータテーブルのDATAIDと一致するロード時のデータテーブルにて他フィールドに差分が存在する場合)
        Dim updateTargetList = (From tgtDr In currentDt Where Convert.ToString(tgtDr.Item("DATAID")) <> "" AndAlso Not Convert.ToString(tgtDr.Item("DATAID")).StartsWith("SYS"))
        Dim compareFieldList As New List(Of String) From {"TANKNO", "COSTCODE", "AMOUNTORD", "AMOUNTFIX",
                                                          "CONTRACTORODR", "CONTRACTORFIX", "SCHEDELDATE", "ACTUALDATE",
                                                          "APPLYTEXT", "INVOICEDBY", "CURRENCYCODE", "DTLOFFICE", "AMOUNTPAY", "LOCALPAY", "SOAAPPDATE",
                                                          "JOT", "TAXATION", "SOACHECK", "DEMREPORTMONTH", "REMARK", "ACCCURRENCYSEGMENT"}
        If withoutApply AndAlso compareFieldList.Contains("APPLYTEXT") Then
            compareFieldList.Remove("APPLYTEXT")
        End If
        If withoutRemark AndAlso compareFieldList.Contains("REMARK") Then
            compareFieldList.Remove("REMARK")
        End If
        Dim hasUnmatch As Boolean = False
        Dim updRowList As New List(Of DataRow)
        Dim hasTankNoUnmatch As Boolean = False
        Dim updTankRowList As New List(Of DataRow)
        Dim demurrageDic As New Dictionary(Of Tuple(Of String, String, String), String)
        Dim soaCheckChanged As Boolean = False
        Dim demReportMonthChange As Boolean = False
        For Each tgtDr In updateTargetList
            Dim dataId As String = Convert.ToString(tgtDr.Item("DATAID"))
            Dim compareDr = (From fstDr In firstTimeDt Where Convert.ToString(fstDr.Item("DATAID")) = dataId).FirstOrDefault
            hasUnmatch = False
            hasTankNoUnmatch = False
            soaCheckChanged = False
            demReportMonthChange = False
            For Each fieldName As String In compareFieldList
                If compareDr Is Nothing OrElse Not tgtDr(fieldName).Equals(compareDr(fieldName)) Then
                    hasUnmatch = True
                    Exit For
                End If
            Next
            If compareDr Is Nothing OrElse Not tgtDr("SOACHECK").Equals(compareDr("SOACHECK")) Then
                soaCheckChanged = True
            End If
            If compareDr Is Nothing OrElse Not tgtDr("DEMREPORTMONTH").Equals(compareDr("DEMREPORTMONTH")) Then
                demReportMonthChange = True
            End If

            '業者(予定)に変更があった場合Fixを上書き
            If compareDr Is Nothing OrElse Not tgtDr("CONTRACTORODR").Equals(compareDr("CONTRACTORODR")) Then
                tgtDr("CONTRACTORFIX") = tgtDr("CONTRACTORODR")
            End If
            'タンクの割り当て変更はタンクを含む全行が更新スキップせずに対象となる為別枠
            If compareDr Is Nothing OrElse Not tgtDr("TANKNO").Equals(compareDr("TANKNO")) Then
                Dim modTankseq As String = Convert.ToString(tgtDr("TANKSEQ"))
                If modifiedTankSeqList.Contains(modTankseq) = False Then
                    modifiedTankSeqList.Add(modTankseq)
                End If
                hasTankNoUnmatch = True
            End If
            '対象行のディープコピーを生成
            Dim cloneTgtDr = currentDt.NewRow
            cloneTgtDr.ItemArray = DirectCast(tgtDr.ItemArray.Clone(), Object())
            If hasUnmatch = True Then
                If soaCheckChanged = True Then
                    If Convert.ToString(cloneTgtDr("SOACHECK")) = "on" Then
                        cloneTgtDr("SOAAPPDATE") = Now.ToString("yyyy/MM/dd")
                    Else
                        cloneTgtDr("SOAAPPDATE") = ""
                    End If
                    cloneTgtDr("IS_UPDATE_TTLINVOICESOAAPPDATE") = "1"
                End If
                If Me.hdnListMapVariant.Value = "GB_SOA" AndAlso compareDr Is Nothing OrElse Not cloneTgtDr("TAXATION").Equals(compareDr("TAXATION")) Then
                    cloneTgtDr("IS_UPDATE_TTLINVOICESOAAPPDATE") = "1"
                End If
                If Me.hdnListMapVariant.Value = "GB_SOA" AndAlso (compareDr Is Nothing OrElse Not cloneTgtDr("JOT").Equals(compareDr("JOT"))) Then
                    cloneTgtDr("IS_UPDATE_TTLINVOICESOAAPPDATE") = "1"
                End If

                If demReportMonthChange = True Then
                    If Convert.ToString(cloneTgtDr("DEMREPORTMONTH")).Trim = "" Then
                        cloneTgtDr("SOAAPPDATE") = ""
                        cloneTgtDr("ACTUALDATE") = ""
                    Else
                        cloneTgtDr("SOAAPPDATE") = Now.ToString("yyyy/MM/dd")
                        cloneTgtDr("ACTUALDATE") = Convert.ToString(cloneTgtDr("DEMREPORTMONTH")).Trim & "/25"
                    End If
                End If
                '金額変化のフラグを付ける(修正前後の予定額に変更があり)
                If compareDr Is Nothing OrElse Not cloneTgtDr("AMOUNTORD").Equals(compareDr("AMOUNTORD")) Then
                    cloneTgtDr("IS_ODR_CHANGECOST") = "1"
                End If
                '金額変化のフラグを付ける(修正前後の実績額に変更があり)
                If compareDr Is Nothing OrElse Not cloneTgtDr("AMOUNTFIX").Equals(compareDr("AMOUNTFIX")) Then
                    cloneTgtDr("IS_FIX_CHANGECOST") = "1"
                End If
                'デマレージ処理の起因レコードの変更有無(ノンブレ以外、最初の輸送,
                'ACTIONIDが指定した値と一致、確定日に変更があった場合）
                Dim actionId As String = Convert.ToString(cloneTgtDr("ACTIONID"))
                If Not cloneTgtDr("BRTYPE").Equals("OPERATION") AndAlso
                    compareDr IsNot Nothing AndAlso
                    (cloneTgtDr("ORIGINDESTINATION").Equals("2") OrElse cloneTgtDr("ORIGINDESTINATION").Equals("1")) AndAlso
                    dicDemurrageCalcField.ContainsKey(Convert.ToString(cloneTgtDr("DTLPOLPOD"))) AndAlso
                    Not cloneTgtDr("ACTUALDATE").Equals(compareDr("ACTUALDATE")) Then

                    '既に更新キーであるレコードが追加済の場合は、無視
                    Dim demurrageKey As Tuple(Of String, String, String) =
                        Tuple.Create(Convert.ToString(cloneTgtDr("ORDERNO")),
                                     Convert.ToString(cloneTgtDr("TANKSEQ")),
                                     Convert.ToString(cloneTgtDr("DTLPOLPOD")))

                    If Not demurrageDic.ContainsKey(demurrageKey) Then
                        cloneTgtDr("IS_CALC_DEMURRAGE") = "1" 'デマレッジの確認処理を走らせる必要ありと判定
                        demurrageDic.Add(demurrageKey, "")
                    End If
                End If
                'SHIP確定日に変動があった場合フラグを立てておく（後続処理にてオーガナイザレコードの日付を更新する）
                If Not cloneTgtDr("BRTYPE").Equals("OPERATION") AndAlso
                   compareDr IsNot Nothing AndAlso
                   actionId = "SHIP" AndAlso
                   Not cloneTgtDr("ACTUALDATE").Equals(compareDr("ACTUALDATE")) Then
                    cloneTgtDr("IS_UPDATE_SHIPDATE") = "1"
                End If
            End If

            If hasUnmatch = True AndAlso (hasTankNoUnmatch = False OrElse splitTankMod = False) Then
                updRowList.Add(cloneTgtDr)
            ElseIf hasUnmatch = True AndAlso hasTankNoUnmatch = True Then
                updTankRowList.Add(cloneTgtDr)
            End If

        Next
        '分離しないため変更タンク一覧をブランク
        If splitTankMod = False Then
            modifiedTankSeqList.Clear()
        End If

        If updRowList IsNot Nothing AndAlso updRowList.Count > 0 Then
            correctDataLists.Add(ModifyType.upd, updRowList)
        End If

        If updTankRowList IsNot Nothing AndAlso updTankRowList.Count > 0 Then
            correctDataLists.Add(ModifyType.updTank, updTankRowList)
        End If

        '2.新規追加分(現在のデータテーブルのDATAIDフィールドが空)
        'タンク割り当ては行っていない
        Dim insRowListWithoutTankmod = (From insDr In currentDt Where (Convert.ToString(insDr.Item("DATAID")) = "" OrElse Convert.ToString(insDr.Item("DATAID")).StartsWith("SYS")) AndAlso modifiedTankSeqList.Contains(Convert.ToString(insDr.Item("TANKSEQ"))) = False)
        If insRowListWithoutTankmod IsNot Nothing AndAlso insRowListWithoutTankmod.Count > 0 Then
            correctDataLists.Add(ModifyType.ins, insRowListWithoutTankmod.ToList)
        End If
        'タンク割り当ても同時に行っている
        Dim insRowListWithTankmod = (From insDr In currentDt Where (Convert.ToString(insDr.Item("DATAID")) = "" OrElse Convert.ToString(insDr.Item("DATAID")).StartsWith("SYS")) AndAlso modifiedTankSeqList.Contains(Convert.ToString(insDr.Item("TANKSEQ"))) = True)
        If insRowListWithTankmod IsNot Nothing AndAlso insRowListWithTankmod.Count > 0 Then
            correctDataLists.Add(ModifyType.insTank, insRowListWithTankmod.ToList)
        End If

        '3.論理削除分(ロード時のブレーカーコストフラグがないDATAIDが現在のデータテーブルのDATAIDに存在しない)
        'ブレーカーコストフラグがないDATAIDの取得
        Dim brCostList = (From brCostDr In firstTimeDt
                          Where Convert.ToString(brCostDr.Item("BRCOST")) <> "1")
        If brCostList IsNot Nothing AndAlso
           brCostList.Count > 0 Then
            Dim brCostDataIdList = (From brCostDataIdDr In currentDt
                                    Where Convert.ToString(brCostDataIdDr.Item("BRCOST")) <> "1" _
                                     AndAlso Convert.ToString(brCostDataIdDr.Item("DATAID")) <> "" AndAlso Not Convert.ToString(brCostDataIdDr.Item("DATAID")).StartsWith("SYS")
                                    Select Convert.ToString(brCostDataIdDr.Item("DATAID"))).ToList

            Dim delRowList = (From delDr In brCostList
                              Where Convert.ToString(delDr.Item("BRCOST")) <> "1" _
                            AndAlso Convert.ToString(delDr.Item("DATAID")) <> "" AndAlso Not Convert.ToString(delDr.Item("DATAID")).StartsWith("SYS") _
                            AndAlso Not (brCostDataIdList.Contains(Convert.ToString(delDr.Item("DATAID"))))).ToList
            If delRowList IsNot Nothing AndAlso delRowList.Count > 0 Then
                '削除の情報をタンク割り当て有無でさらに分割
                '割り当てなし
                Dim delRowListWithoutTankMod = (From delRow In delRowList Where modifiedTankSeqList.Contains(Convert.ToString(delRow.Item("TANKSEQ"))) = False)
                If delRowListWithoutTankMod IsNot Nothing AndAlso delRowListWithoutTankMod.Count > 0 Then
                    correctDataLists.Add(ModifyType.del, delRowListWithoutTankMod.ToList)
                End If
                '割り当てあり
                Dim delRowListWithTankMod = (From delRow In delRowList Where modifiedTankSeqList.Contains(Convert.ToString(delRow.Item("TANKSEQ"))) = True)
                If delRowListWithTankMod IsNot Nothing AndAlso delRowListWithTankMod.Count > 0 Then
                    correctDataLists.Add(ModifyType.delTank, delRowListWithTankMod.ToList)
                End If
            End If
        End If


        Return correctDataLists
    End Function
    ''' <summary>
    ''' オーダー明細にデータを登録
    ''' </summary>
    ''' <param name="targetData"></param>
    ''' <param name="entryType">登録種類</param>
    ''' <returns></returns>
    ''' <remarks>それぞれ追加・更新・タンク更新・削除の処理へ飛ばす</remarks>
    Private Function EntryOrderValue(targetData As Dictionary(Of ModifyType, List(Of DataRow)), entryType As String, dicDemurrageCalcField As Dictionary(Of String, List(Of String))) As ProcMessage
        Dim retMessage As New ProcMessage
        Dim modOtherUser As New List(Of DataRow) '他ユーザー更新により登録不可のレコードを保持
        Dim dateSeqError As New List(Of DataRow) '日付とDISPSEQの整合性が取れない更新対象レコード
        Dim canNotEntryTankSeq As New List(Of Hashtable) '登録不可のタンクSEQを保持
        Dim entryFailedDataList As New Dictionary(Of ModifyType, List(Of DataRow))

        Dim procDate As Date = Date.Now '更新日時保持用(1度の更新処理での時刻は合わせるため)
        Dim messageNo As String = C_MESSAGENO.RIGHTBIXOUT
        'ログファイル書き込み共通機能の変動しないプロパティを設定
        COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
        COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
        COA0003LogFile.MESSAGENO = messageNo
        'タンク引き当てしたデータは必ず更新レコードがある為
        'ModifyType.updTankが存在するかチェック
        Dim tankModNoList As IEnumerable(Of String) = Nothing
        If targetData.ContainsKey(ModifyType.updTank) Then
            tankModNoList = (From dr In targetData(ModifyType.updTank) Group By tankseq = Convert.ToString(dr.Item("TANKSEQ")) Into Group Select tankseq)
        End If
        'DB接続の生成
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            'タンク引当対象レコードの登録（トランザクションありで都度登録）
            If tankModNoList IsNot Nothing Then
                For Each tankSeq In tankModNoList
                    'UPDATE対象
                    Dim updTankModList As IEnumerable(Of DataRow) = (From dr In targetData(ModifyType.updTank) Where Convert.ToString(dr.Item("TANKSEQ")) = tankSeq)
                    '追加対象
                    Dim insTankModList As IEnumerable(Of DataRow) = Nothing
                    If targetData.ContainsKey(ModifyType.insTank) Then
                        insTankModList = (From dr In targetData(ModifyType.insTank) Where Convert.ToString(dr.Item("TANKSEQ")) = tankSeq)
                    End If
                    '論理削除対象
                    Dim delTankModList As IEnumerable(Of DataRow) = Nothing
                    If targetData.ContainsKey(ModifyType.delTank) Then
                        delTankModList = (From dr In targetData(ModifyType.delTank) Where Convert.ToString(dr.Item("TANKSEQ")) = tankSeq)
                    End If
                    'オーダーNOを変数に保持
                    Dim orderNo As String = Convert.ToString(updTankModList(0).Item("ORDERNO"))
                    Dim tankNo As String = Convert.ToString(updTankModList(0).Item("TANKNO"))
                    'タンク引当レコードにつき、他ユーザーにレコードが更新されていないかチェック
                    If CheckNewCostEntryOtherUser(tankSeq, orderNo, sqlCon) = False Then
                        'タンク引当対象が更新できない場合
                        canNotEntryTankSeq.Add(New Hashtable From {{"TANKSEQ", tankSeq}, {"ORDERNO", orderNo}})
                        Continue For
                    End If
                    'タンクSEQ単位にトランザクションを利用しタンク引き当て関連の登録を行う
                    Using tran As SqlTransaction = sqlCon.BeginTransaction
                        Try
                            Dim isSkipThisTank As Boolean = False
                            'タンク引当分更新
                            For Each item In updTankModList
                                If CheckUpdateOtherUsers(item, sqlCon, tran) = False Then
                                    'タンク引当対象が更新できない場合
                                    canNotEntryTankSeq.Add(New Hashtable From {{"TANKSEQ", tankSeq}, {"ORDERNO", orderNo}})
                                    isSkipThisTank = True
                                    Continue For
                                End If
                                '発着分更新
                                UpdateOrderValue(item, sqlCon, tran, procDate)
                                'デマレッジ計算
                                If item("IS_CALC_DEMURRAGE").Equals("1") Then
                                    EntryDemurrage(item, sqlCon, dicDemurrageCalcField, tran, procDate)
                                End If
                            Next

                            If isSkipThisTank = True Then
                                Continue For
                            End If

                            'タンク引当分追加
                            If insTankModList IsNot Nothing Then
                                For Each item In insTankModList
                                    InsertOrderValue(item, sqlCon, tran, procDate)
                                Next
                            End If
                            'タンク引当分論理削除（削除フラグのみ他を入れても無視）
                            If delTankModList IsNot Nothing Then
                                For Each item In delTankModList
                                    If CheckUpdateOtherUsers(item, sqlCon, tran) = False Then
                                        'タンク引当対象が更新できない場合
                                        canNotEntryTankSeq.Add(New Hashtable From {{"TANKSEQ", tankSeq}, {"ORDERNO", orderNo}})
                                        isSkipThisTank = True
                                        Continue For
                                    End If
                                    DeleteOrderValue(item, sqlCon, tran, procDate)
                                Next
                                If isSkipThisTank = True Then
                                    Continue For
                                End If
                            End If
                            Dim drShipUpdRecord As DataRow = (From item In updTankModList
                                                              Where item("IS_UPDATE_SHIPDATE").Equals("1") _
                                                               AndAlso item("ORDERNO").Equals(orderNo) _
                                                               AndAlso item("TANKSEQ").Equals(tankSeq)).FirstOrDefault
                            Dim shipDate As String = ""
                            Dim isUpdateShipChanged As String = ""
                            If drShipUpdRecord IsNot Nothing Then
                                shipDate = Convert.ToString(drShipUpdRecord.Item("ACTUALDATE"))
                                isUpdateShipChanged = "1"
                            End If
                            'オーガナイザ分費用レコード更新
                            UpdateOrderValueOrganizerTankNo(orderNo, tankSeq, tankNo, sqlCon, shipDate, isUpdateShipChanged, tran, procDate)

                            tran.Commit()
                        Catch ex As Exception
                            canNotEntryTankSeq.Add(New Hashtable From {{"TANKSEQ", tankSeq}, {"ORDERNO", orderNo}})
                            COA0003LogFile.TEXT = String.Format("タンク引当登録エラー:ORDERNO({0}),TANKSEQ({1}) " & ControlChars.CrLf & "{2}",
                                                            orderNo, tankSeq, ex.ToString())
                            COA0003LogFile.COA0003WriteLog()
                            Continue For
                        End Try
                    End Using 'tran As SqlTransaction
                Next ' tankSeq In tankModNoList
            End If
            '通常の費用、日付などの登録（トランザクションなしで都度登録）
            If targetData.ContainsKey(ModifyType.upd) Then
                '更新対象のオーダーについて日付のORDERNO,TANKSEQキーの登録可能な最大DISPSEQを取得
                Dim dicMaxDispSeq = GetUpdatableMaxDispSeq(targetData(ModifyType.upd), sqlCon)
                For Each item In targetData(ModifyType.upd)
                    '他ユーザー更新チェック
                    If CheckUpdateOtherUsers(item, sqlCon) = False Then
                        modOtherUser.Add(item)
                        Continue For
                    End If
                    '更新可能な最大DSPSEQかチェック
                    If Convert.ToString(item("DISPSEQ")) <> "" AndAlso Convert.ToString(item("BRADDEDCOST")) = "0" AndAlso
                        dicMaxDispSeq IsNot Nothing AndAlso
                       dicMaxDispSeq.ContainsKey(Convert.ToString(item("ORDERNO"))) AndAlso
                       dicMaxDispSeq(Convert.ToString(item("ORDERNO"))).ContainsKey(Convert.ToString(item("TANKSEQ"))) AndAlso
                       dicMaxDispSeq(Convert.ToString(item("ORDERNO")))(Convert.ToString(item("TANKSEQ"))) <= CInt(Convert.ToString(item("DISPSEQ"))) Then
                        '更新不可の為スキップ
                        dateSeqError.Add(item)
                        Continue For
                    End If

                    Try
                        UpdateOrderValue(item, sqlCon, procDate:=procDate)
                        'デマレッジ計算
                        If item("IS_CALC_DEMURRAGE").Equals("1") Then
                            EntryDemurrage(item, sqlCon, dicDemurrageCalcField, procDate:=procDate)
                        End If
                        'オーガナイザーレコードの日付更新
                        If item("IS_UPDATE_SHIPDATE").Equals("1") Then
                            UpdateOrderValueOrganizerDate(item, sqlCon, procDate:=procDate)
                        End If
                        'デマレッジレコードのAgentComm10%増幅レコードの処理を行う
                        If Me.hdnListMapVariant.Value = "GB_Demurrage" Then
                            AddAgentCommRecord(item, sqlCon, procDate:=procDate)
                        End If
                        'TOTALINVOICEの変更を元にオーガナイザーレコードのSOAAPDATEの反映
                        '20190627 SOAにつき現状オーガナイザのでまれっじを除く全費目を見えるようにしているため、他ユーザー更新となってしまう
                        '         一旦画面に見えているためコメントアウトとする
                        '        ↓ここから
                        '20190719 解放
                        If Me.hdnListMapVariant.Value = "GB_SOA" _
                            AndAlso item("IS_UPDATE_TTLINVOICESOAAPPDATE").Equals("1") _
                            AndAlso item("COSTCODE").Equals(GBC_COSTCODE_SALES) Then
                            UpdateOrderValueOrganizerSoaDate(item, sqlCon, procDate:=procDate)
                        End If
                        '20190627 ↑ここまで
                    Catch ex As Exception
                        COA0003LogFile.TEXT = String.Format("オーダー費用明細 更新時エラー:DATAID({0}" & ControlChars.CrLf & "{1}",
                                                            item("DATAID"),
                                                            ex.ToString())
                        COA0003LogFile.TEXT = ex.ToString()
                        COA0003LogFile.COA0003WriteLog()
                    End Try

                Next
            End If
            If targetData.ContainsKey(ModifyType.ins) Then
                For Each item In targetData(ModifyType.ins)
                    Try
                        'NONブレーカーの場合はオーダーNOが存在しないためこのタイミングで割り振る
                        If entryType = CONST_ENTRYTYPE_NONBR Then
                            Dim nonBrNo As String = GetNonBrNo(sqlCon)
                            item.Item("ORDERNO") = nonBrNo
                            item.Item("DISPSEQ") = ""
                        End If
                        InsertOrderValue(item, sqlCon, procDate:=procDate)
                    Catch ex As Exception
                        COA0003LogFile.TEXT = String.Format("オーダー費用明細 追加時エラー:ORDERNO({0}),TANKSEQ({1}),COSTCODE({2})" & ControlChars.CrLf & "{3}",
                                                            item("ORDERNO"),
                                                            item("TANKSEQ"),
                                                            item("COSTCODE"),
                                                            ex.ToString())
                        COA0003LogFile.TEXT = ex.ToString()
                        COA0003LogFile.COA0003WriteLog()
                    End Try

                Next
            End If
            If targetData.ContainsKey(ModifyType.del) Then
                For Each item In targetData(ModifyType.del)
                    If CheckUpdateOtherUsers(item, sqlCon) = False Then
                        modOtherUser.Add(item)
                        Continue For
                    End If
                    Try
                        DeleteOrderValue(item, sqlCon, procDate:=procDate)
                    Catch ex As Exception
                        COA0003LogFile.TEXT = String.Format("オーダー費用明細 削除時エラー:DATAID({0}" & ControlChars.CrLf & "{1}",
                                                            item("DATAID"),
                                                            ex.ToString())
                        COA0003LogFile.COA0003WriteLog()
                    End Try
                Next
            End If
        End Using
        '処理結果に応じ左ボックス用のメッセージを表示
        If modOtherUser.Count = 0 AndAlso canNotEntryTankSeq.Count = 0 AndAlso dateSeqError.Count = 0 Then
            '全て正常の場合
            retMessage.MessageNo = C_MESSAGENO.NORMALDBENTRY

        Else
            retMessage.MessageNo = C_MESSAGENO.RIGHTBIXOUT
            retMessage.modOtherUsers = modOtherUser
            retMessage.dateSeqError = dateSeqError
            retMessage.canNotEntryTankSeq = canNotEntryTankSeq
        End If
        Return retMessage
    End Function
    ''' <summary>
    ''' 他ユーザー更新チェック
    ''' </summary>
    ''' <param name="targetDr">これから登録を行うデータ行</param>
    ''' <param name="sqlConn">SQL接続</param>
    ''' <returns>True:他ユーザー更新なし,False:他ユーザー更新あり</returns>
    ''' <remarks>EntryOrderValueのみ呼び出される</remarks>
    Private Function CheckUpdateOtherUsers(targetDr As DataRow, ByRef sqlConn As SqlConnection, Optional sqlTran As SqlTransaction = Nothing) As Boolean
        Dim sqlStat As New StringBuilder

        'チェック対象フィールドがブランクの場合はそもそも新規レコードのためチェック対象外
        If Convert.ToString(targetDr.Item("TIMSTP")).Trim = "" Then
            Return True
        End If

        'TODO更新チェックは他のフィールドになる想定なので要変更
        sqlStat.AppendLine("SELECT TIMSTP = cast(VL.UPDTIMSTP as bigint)")
        sqlStat.AppendLine("      ,isnull(convert(nvarchar, VL.UPDYMD , 120),'') as UPDYMD")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDUSER),'')                  as UPDUSER")
        sqlStat.AppendLine("      ,isnull(rtrim(VL.UPDTERMID),'')                as UPDTERMID")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlStat.AppendLine(" WHERE VL.DATAID = @DATAID")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlConn, sqlTran)
            Dim dataId As String = Convert.ToString(targetDr.Item("DATAID"))
            Dim paramDataId As SqlParameter = sqlCmd.Parameters.Add("@DATAID", SqlDbType.NVarChar)
            paramDataId.Value = dataId
            Using sqlDr As SqlDataReader = sqlCmd.ExecuteReader()
                'この段階でありえないがDATAIDが存在しない場合は、物理削除
                'された恐れがある為、更新させない
                If sqlDr.HasRows = False Then
                    Return False
                End If
                While sqlDr.Read
                    If Convert.ToString(targetDr.Item("UPDYMD")).TrimEnd = Convert.ToString(sqlDr("UPDYMD")).TrimEnd _
               AndAlso Convert.ToString(targetDr.Item("UPDUSER")).TrimEnd = Convert.ToString(sqlDr("UPDUSER")).TrimEnd _
               AndAlso Convert.ToString(targetDr.Item("UPDTERMID")).TrimEnd = Convert.ToString(sqlDr("UPDTERMID")).TrimEnd Then
                        Return True
                    End If
                End While
            End Using
        End Using
        'ここまで来てReturnしていない場合は比較結果不一致のため他ユーザー更新
        Return False
    End Function
    ''' <summary>
    ''' 日付項目の連続性チェックを行うための最大更新可能DISPシーケンスを取得
    ''' </summary>
    ''' <param name="updTarget"></param>
    ''' <param name="sqlCon"></param>
    ''' <returns></returns>
    Private Function GetUpdatableMaxDispSeq(updTarget As List(Of DataRow), sqlCon As SqlConnection) As Dictionary(Of String, Dictionary(Of String, Integer))
        '第一キー:オーダーNo,第二キー:TANKSEQ,値変更許可の最大DISPSEQ
        Dim dicRet As New Dictionary(Of String, Dictionary(Of String, Integer))
        'そもそも日付変更のないMapVariの場合は何もしない
        If Not {"Default", "DefaultDetailed", "DefaultTankAllocate", "GB_TankActivity"}.Contains(Me.hdnListMapVariant.Value) Then
            Return dicRet
        End If
        For Each item In updTarget
            'itemにそもそもdispSeqが無い場合、追加費用の場合は対象外
            If Convert.ToString(item("DISPSEQ")) = "" OrElse Convert.ToString(item("BRADDEDCOST")) <> "0" Then
                Continue For
            End If
            '既に最大MAXDISPシーケンスを算出済なら処理当該キーは処理不要
            Dim orderNo As String = Convert.ToString(item("ORDERNO"))
            Dim tankSeq As String = Convert.ToString(item("TANKSEQ"))
            If dicRet.ContainsKey(orderNo) AndAlso
               dicRet(orderNo).ContainsKey(tankSeq) Then
                Continue For
            End If
            'オーダーNo タンクSEQを元にDISPSEQの設定がある対象のオーダーを取得
            Dim dtAllCost As DataTable = GetOrderValueDateSeq(orderNo, tankSeq, sqlCon)
            '更新対象のうち同一オーダーNo、タンクSEQのデータを取得
            Dim updTargetTank = From updItem As DataRow In updTarget Where updItem("ORDERNO").Equals(orderNo) _
                                                                     AndAlso updItem("TANKSEQ").Equals(tankSeq)
            '現在の全更新対象の日付に変更
            Dim changedDateField As String = ""
            Dim dateFields As New List(Of String) From {"SCHEDELDATE", "ACTUALDATE"}
            For Each updItem In updTargetTank
                Dim qDbData = From drAllCost In dtAllCost Where drAllCost("DATAID").Equals(updItem("DATAID"))
                If qDbData.Any Then
                    Dim drAllCost As DataRow = qDbData.FirstOrDefault
                    For Each dateField In dateFields
                        Dim dateString As String = Convert.ToString(updItem(dateField))
                        If {"", "1900/01/01"}.Contains(dateString) Then
                            dateString = "2099/12/31"
                        End If
                        If Not dateString.Equals(drAllCost(dateField)) Then
                            changedDateField = dateField
                        End If
                        drAllCost(dateField) = dateString
                    Next
                End If
            Next
            '日付項目に変化が無い場合は対象外とする
            If changedDateField = "" Then
                If dicRet.ContainsKey(orderNo) Then
                    dicRet(orderNo).Add(tankSeq, Integer.MaxValue)
                Else
                    dicRet.Add(orderNo, New Dictionary(Of String, Integer) From {{tankSeq, Integer.MaxValue}})
                End If
                Continue For
            End If
            '日付の連続性が無いデータの取得
            Dim qErrorRow = From drAllCost In dtAllCost
                            Where (From subdrAllCost In dtAllCost
                                   Where CInt(drAllCost("DISPSEQ")) < CInt(subdrAllCost("DISPSEQ")) _
                                 AndAlso Convert.ToString(drAllCost(changedDateField)) > Convert.ToString(subdrAllCost(changedDateField))).Any
                            Order By CInt(drAllCost("DISPSEQ"))

            '不整合がある最小DISPSEQを取得しそれ以降のDISPSEQは登録させないようFalseを返却
            If qErrorRow.Any = False Then
                '正常の場合DISPSEQはHIGHVALUEとして条件にかからないようにする
                If dicRet.ContainsKey(orderNo) Then
                    dicRet(orderNo).Add(tankSeq, Integer.MaxValue)
                Else
                    dicRet.Add(orderNo, New Dictionary(Of String, Integer) From {{tankSeq, Integer.MaxValue}})
                End If
            Else
                Dim dr As DataRow = qErrorRow.FirstOrDefault
                If dicRet.ContainsKey(orderNo) Then
                    dicRet(orderNo).Add(tankSeq, CInt(dr("DISPSEQ")))
                Else
                    dicRet.Add(orderNo, New Dictionary(Of String, Integer) From {{tankSeq, CInt(dr("DISPSEQ"))}})
                End If
            End If
        Next
        'OrderNo,TANKSEQをキーとした更新可能な最大DISPSEQオブジェクトを生成
        Return dicRet

    End Function
    ''' <summary>
    ''' 他ユーザーによって更新対象の同一オーダーNo、TANKSEQに新規コストが追加されていないかチェック
    ''' (追加/削除されている場合は更新不可にするため)
    ''' </summary>
    ''' <param name="tankSeq"></param>
    ''' <param name="orderNo"></param>
    ''' <param name="sqlCon"></param>
    ''' <returns>True:他ユーザー更新なし、False:他ユーザー更新あり</returns>
    Private Function CheckNewCostEntryOtherUser(tankSeq As String, orderNo As String, ByRef sqlCon As SqlConnection, Optional ByRef sqlTran As SqlTransaction = Nothing) As Boolean
        If tankSeq = "" OrElse orderNo = "" Then
            Return True
        End If
        '1回のリクエストで複数回呼ぶ可能性があるのでStatic定義
        Static firstTimeDt As DataTable
        '画面ロード時に退避した編集前のデータテーブル取得
        If firstTimeDt Is Nothing Then '2回目以降は設定されている為ファイルIOはなし
            firstTimeDt = CreateOrderListTable()
            Dim COA0021ListTable As COA0021ListTable = New COA0021ListTable

            COA0021ListTable.FILEdir = Me.hdnOrgXMLsaveFile.Value
            COA0021ListTable.TBLDATA = firstTimeDt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                firstTimeDt = COA0021ListTable.OUTTBL
            Else
                CommonFunctions.ShowMessage(COA0021ListTable.ERR, Me.lblFooterMessage)
                Return Nothing
            End If
        End If

        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("Select CONVERT(varchar(36),VL.DATAID)     As DATAID")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlStat.AppendLine(" WHERE VL.ORDERNO    = @ORDERNO")
        sqlStat.AppendLine("   And VL.TANKSEQ    = @TANKSEQ")
        sqlStat.AppendLine("   And VL.DTLPOLPOD <> @DTLPOLPOD")
        sqlStat.AppendLine("   And VL.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("   And VL.COSTCODE  <> @COSTCODE")
        If Me.hdnListMapVariant.Value <> "GB_TankActivity" Then
            sqlStat.AppendLine("   And Not EXISTS (Select 1 ") 'デマレッジ終端アクションはタンク動静のみ表示
            sqlStat.AppendLine("                     FROM GBM0010_CHARGECODE CSTS")
            sqlStat.AppendLine("                    WHERE CSTS.COMPCODE = @COMPCODE")
            sqlStat.AppendLine("                      And CSTS.COSTCODE = VL.COSTCODE")
            sqlStat.AppendLine("                      And '1' = CASE WHEN VL.DTLPOLPOD LIKE 'POL%' AND CST.LDKBN IN ('B','L') THEN '1' ")
            sqlStat.AppendLine("                                     WHEN VL.DTLPOLPOD LIKE 'POD%' AND CST.LDKBN IN ('B','D') THEN '1' ")
            sqlStat.AppendLine("                                     WHEN VL.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            sqlStat.AppendLine("                                     ELSE '1'")
            sqlStat.AppendLine("                                END")
            sqlStat.AppendLine("                      AND CSTS.CLASS10  = '" & CONST_FLAG_YES & "'")
            sqlStat.AppendLine("                      AND CSTS.STYMD   <= VL.STYMD")
            sqlStat.AppendLine("                      AND CSTS.ENDYMD  >= VL.STYMD")
            sqlStat.AppendLine("                      AND CSTS.DELFLG  <> @DELFLG")
            sqlStat.AppendLine("                  )")
        End If


        '画面初回ロード時のOrderNo,タンクSEQにおけるDataIdリストを取得（処理時間現在のDBと突き合わせ用）
        Dim dataIdList = (From firstTimeDr In firstTimeDt Where Convert.ToString(firstTimeDr.Item("ORDERNO")) = orderNo AndAlso Convert.ToString(firstTimeDr.Item("TANKSEQ")) = tankSeq Select Convert.ToString(firstTimeDr.Item("DATAID")))
        'SQLを実行しデータテーブルを取得
        Dim dtDbResult As New DataTable
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankSeq
                .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = "Organizer"
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@COSTCODE", SqlDbType.NVarChar).Value = GBC_COSTCODE_DEMURRAGE

                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")

            End With

            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        'DB取得結果のうち画面初回ロード時に存在しないDataIdを検索(他ユーザー費用追加)
        ''(存在した場合、他ユーザーによる追加レコードあり）
        'Dim findNewCostItemList = (From dr In dtDbResult Where dataIdList.Contains(Convert.ToString(dr.Item("DATAID"))) = False)
        ''画面初回ロード時に存在しないデータIDがなければ更新可能
        'If findNewCostItemList IsNot Nothing AndAlso findNewCostItemList.Count > 0 Then
        '    Return False
        'End If
        '画面初回ロードのデータのうちDB取得結果に存在しないDataIdを検索(他ユーザー費用削除)
        '(存在した場合、他ユーザーによる削除レコードあり）
        Dim dbDataIdList = (From dr In dtDbResult Select Convert.ToString(dr.Item("DATAID")))
        Dim findDeleteCostItemList = (From dataId In dataIdList Where dbDataIdList.Contains(dataId) = False)
        If findDeleteCostItemList IsNot Nothing AndAlso findDeleteCostItemList.Count > 0 Then
            Return False
        End If

        'ここまで来た場合他ユーザーに更新されていないため更新可
        Return True

    End Function
    ''' <summary>
    ''' オーダー(明細)追加処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>EntryOrderValueのみ呼び出される</remarks>
    Private Function InsertOrderValue(dr As DataRow, ByRef sqlCon As SqlConnection, Optional ByRef sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/1/1#) As String
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Static sqlStat As StringBuilder
        'SQL文作成
        If sqlStat Is Nothing Then
            sqlStat = New StringBuilder
            sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE")
            sqlStat.AppendLine("     (ORDERNO")
            sqlStat.AppendLine("     ,TANKSEQ")
            sqlStat.AppendLine("     ,DTLPOLPOD")
            sqlStat.AppendLine("     ,DTLOFFICE")
            sqlStat.AppendLine("     ,TANKNO")
            sqlStat.AppendLine("     ,COSTCODE")
            sqlStat.AppendLine("     ,ACTIONID")
            sqlStat.AppendLine("     ,DISPSEQ")
            sqlStat.AppendLine("     ,LASTACT")
            sqlStat.AppendLine("     ,COUNTRYCODE")
            sqlStat.AppendLine("     ,CURRENCYCODE")
            sqlStat.AppendLine("     ,TAXATION")
            sqlStat.AppendLine("     ,AMOUNTORD")
            sqlStat.AppendLine("     ,AMOUNTFIX")
            sqlStat.AppendLine("     ,CONTRACTORODR")
            sqlStat.AppendLine("     ,CONTRACTORFIX")
            sqlStat.AppendLine("     ,SCHEDELDATE")
            sqlStat.AppendLine("     ,ACTUALDATE")
            sqlStat.AppendLine("     ,LOCALRATE")
            sqlStat.AppendLine("     ,INVOICEDBY")
            sqlStat.AppendLine("     ,APPLYTEXT")
            sqlStat.AppendLine("     ,REMARK")
            sqlStat.AppendLine("     ,BRID")
            sqlStat.AppendLine("     ,BRCOST")
            sqlStat.AppendLine("     ,AGENTORGANIZER")
            If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                sqlStat.AppendLine("     ,ACCCURRENCYSEGMENT")
            End If
            sqlStat.AppendLine("     ,DELFLG")
            sqlStat.AppendLine("     ,INITYMD")
            sqlStat.AppendLine("     ,INITUSER")
            sqlStat.AppendLine("     ,UPDYMD")
            sqlStat.AppendLine("     ,UPDUSER")
            sqlStat.AppendLine("     ,UPDTERMID")
            sqlStat.AppendLine("     ,RECEIVEYMD")
            sqlStat.AppendLine("     ) VALUES (@ORDERNO")
            sqlStat.AppendLine("              ,@TANKSEQ")
            sqlStat.AppendLine("              ,@DTLPOLPOD")
            sqlStat.AppendLine("              ,@DTLOFFICE")
            sqlStat.AppendLine("              ,@TANKNO")
            sqlStat.AppendLine("              ,@COSTCODE")
            sqlStat.AppendLine("              ,@ACTIONID")
            sqlStat.AppendLine("              ,@DISPSEQ")
            sqlStat.AppendLine("              ,@LASTACT")
            sqlStat.AppendLine("              ,@COUNTRYCODE")
            sqlStat.AppendLine("              ,@CURRENCYCODE")
            sqlStat.AppendLine("              ,@TAXATION")
            sqlStat.AppendLine("              ,@AMOUNTORD")
            sqlStat.AppendLine("              ,@AMOUNTFIX")
            sqlStat.AppendLine("              ,@CONTRACTORODR")
            sqlStat.AppendLine("              ,@CONTRACTORFIX")
            sqlStat.AppendLine("              ,@SCHEDELDATE")
            sqlStat.AppendLine("              ,@ACTUALDATE")
            sqlStat.AppendLine("              ,@LOCALRATE")
            sqlStat.AppendLine("              ,@INVOICEDBY")
            sqlStat.AppendLine("              ,@APPLYTEXT")
            sqlStat.AppendLine("              ,@REMARK")
            sqlStat.AppendLine("              ,@BRID")
            sqlStat.AppendLine("              ,@BRCOST")
            sqlStat.AppendLine("              ,@AGENTORGANIZER")
            If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                sqlStat.AppendLine("     ,@ACCCURRENCYSEGMENT")
            End If
            sqlStat.AppendLine("              ,@DELFLG")
            sqlStat.AppendLine("              ,@INITYMD")
            sqlStat.AppendLine("              ,@INITUSER")
            sqlStat.AppendLine("              ,@UPDYMD")
            sqlStat.AppendLine("              ,@UPDUSER")
            sqlStat.AppendLine("              ,@UPDTERMID")
            sqlStat.AppendLine("              ,@RECEIVEYMD")
            sqlStat.AppendLine("     )")
        End If

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ORDERNO"))
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TANKSEQ"))
                .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DTLPOLPOD"))
                .Add("@DTLOFFICE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DTLOFFICE"))
                .Add("@TANKNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TANKNO"))
                .Add("@COSTCODE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("COSTCODE"))
                .Add("@ACTIONID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ACTIONID"))
                .Add("@DISPSEQ", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DISPSEQ"))
                .Add("@LASTACT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LASTACT"))
                .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("COUNTRYCODE"))
                .Add("@CURRENCYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CURRENCYCODE"))
                .Add("@TAXATION", SqlDbType.NVarChar).Value = If(Convert.ToString(dr.Item("TAXATION")) = "on", "1", "0")
                .Add("@AMOUNTORD", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMOUNTORD")))
                .Add("@AMOUNTFIX", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMOUNTFIX")))
                .Add("@CONTRACTORODR", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CONTRACTORODR"))
                .Add("@CONTRACTORFIX", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CONTRACTORFIX"))
                .Add("@SCHEDELDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("SCHEDELDATE")))
                .Add("@ACTUALDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ACTUALDATE")))
                .Add("@LOCALRATE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCALRATE")))
                If Convert.ToString(dr.Item("JOT")) = "on" Then
                    .Add("@INVOICEDBY", SqlDbType.NVarChar).Value = C_JOT_AGENT
                Else
                    .Add("@INVOICEDBY", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DTLOFFICE"))
                End If
                .Add("@APPLYTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REMARK"))
                .Add("@BRID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BRID"))
                .Add("@BRCOST", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("BRCOST"))
                .Add("@AGENTORGANIZER", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("AGENTORGANIZER"))
                If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                    .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ACCCURRENCYSEGMENT"))
                End If
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_NO
                .Add("@INITYMD", SqlDbType.DateTime).Value = procDate
                .Add("@INITUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            End With

            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function
    ''' <summary>
    ''' オーダー（明細）テーブル更新処理
    ''' </summary>
    ''' <param name="dr"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="procDate"></param>
    ''' <returns></returns>
    Private Function UpdateOrderValue(dr As DataRow, ByRef sqlCon As SqlConnection, Optional ByRef sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/1/1#) As String
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Static sqlStat As StringBuilder
        'SQL文作成
        If sqlStat Is Nothing Then
            sqlStat = New StringBuilder
            sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
            sqlStat.AppendLine("      ORDERNO")
            sqlStat.AppendLine("     ,STYMD")
            sqlStat.AppendLine("     ,ENDYMD")
            sqlStat.AppendLine("     ,TANKSEQ")
            sqlStat.AppendLine("     ,DTLPOLPOD")
            sqlStat.AppendLine("     ,DTLOFFICE")
            sqlStat.AppendLine("     ,TANKNO")
            sqlStat.AppendLine("     ,COSTCODE")
            sqlStat.AppendLine("     ,ACTIONID")
            sqlStat.AppendLine("     ,DISPSEQ")
            sqlStat.AppendLine("     ,LASTACT")
            sqlStat.AppendLine("     ,REQUIREDACT")
            sqlStat.AppendLine("     ,ORIGINDESTINATION")
            sqlStat.AppendLine("     ,COUNTRYCODE")
            sqlStat.AppendLine("     ,CURRENCYCODE")
            sqlStat.AppendLine("     ,TAXATION")
            sqlStat.AppendLine("     ,AMOUNTBR")
            sqlStat.AppendLine("     ,AMOUNTORD")
            sqlStat.AppendLine("     ,AMOUNTFIX")
            sqlStat.AppendLine("     ,CONTRACTORBR")
            sqlStat.AppendLine("     ,CONTRACTORODR")
            sqlStat.AppendLine("     ,CONTRACTORFIX")
            sqlStat.AppendLine("     ,SCHEDELDATEBR")
            sqlStat.AppendLine("     ,SCHEDELDATE")
            sqlStat.AppendLine("     ,ACTUALDATE")
            sqlStat.AppendLine("     ,LOCALBR")
            sqlStat.AppendLine("     ,LOCALRATE")
            sqlStat.AppendLine("     ,TAXBR")
            sqlStat.AppendLine("     ,AMOUNTPAY")
            sqlStat.AppendLine("     ,LOCALPAY")
            sqlStat.AppendLine("     ,TAXPAY")
            sqlStat.AppendLine("     ,INVOICEDBY")
            sqlStat.AppendLine("     ,APPLYID")
            sqlStat.AppendLine("     ,APPLYTEXT")
            sqlStat.AppendLine("     ,LASTSTEP")
            sqlStat.AppendLine("     ,SOAAPPDATE")
            sqlStat.AppendLine("     ,REMARK")
            sqlStat.AppendLine("     ,BRID")
            sqlStat.AppendLine("     ,BRCOST")
            sqlStat.AppendLine("     ,DATEFIELD")
            sqlStat.AppendLine("     ,DATEINTERVAL")
            sqlStat.AppendLine("     ,BRADDEDCOST")
            sqlStat.AppendLine("     ,AGENTORGANIZER")
            sqlStat.AppendLine("     ,ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("     ,DELFLG")
            sqlStat.AppendLine("     ,INITYMD")
            sqlStat.AppendLine("     ,INITUSER")
            sqlStat.AppendLine("     ,UPDYMD")
            sqlStat.AppendLine("     ,UPDUSER")
            sqlStat.AppendLine("     ,UPDTERMID")
            sqlStat.AppendLine("     ,RECEIVEYMD")
            sqlStat.AppendLine(" ) SELECT ORDERNO")
            sqlStat.AppendLine("         ,STYMD")
            sqlStat.AppendLine("         ,ENDYMD")
            sqlStat.AppendLine("         ,TANKSEQ")
            sqlStat.AppendLine("         ,DTLPOLPOD")
            sqlStat.AppendLine("         ,DTLOFFICE")
            sqlStat.AppendLine("         ,TANKNO")
            sqlStat.AppendLine("         ,COSTCODE")
            sqlStat.AppendLine("         ,ACTIONID")
            sqlStat.AppendLine("         ,DISPSEQ")
            sqlStat.AppendLine("         ,LASTACT")
            sqlStat.AppendLine("         ,REQUIREDACT")
            sqlStat.AppendLine("         ,ORIGINDESTINATION")
            sqlStat.AppendLine("         ,COUNTRYCODE")
            sqlStat.AppendLine("         ,CURRENCYCODE")
            sqlStat.AppendLine("         ,TAXATION")
            sqlStat.AppendLine("         ,AMOUNTBR")
            sqlStat.AppendLine("         ,AMOUNTORD")
            sqlStat.AppendLine("         ,AMOUNTFIX")
            sqlStat.AppendLine("         ,CONTRACTORBR")
            sqlStat.AppendLine("         ,CONTRACTORODR")
            sqlStat.AppendLine("         ,CONTRACTORFIX")
            sqlStat.AppendLine("         ,SCHEDELDATEBR")
            sqlStat.AppendLine("         ,SCHEDELDATE")
            sqlStat.AppendLine("         ,ACTUALDATE")
            sqlStat.AppendLine("         ,LOCALBR")
            sqlStat.AppendLine("         ,LOCALRATE")
            sqlStat.AppendLine("         ,TAXBR")
            sqlStat.AppendLine("         ,AMOUNTPAY")
            sqlStat.AppendLine("         ,LOCALPAY")
            sqlStat.AppendLine("         ,TAXPAY")
            sqlStat.AppendLine("         ,INVOICEDBY")
            sqlStat.AppendLine("         ,APPLYID       AS APPLYID")
            sqlStat.AppendLine("         ,APPLYTEXT     AS APPLYTEXT")
            sqlStat.AppendLine("         ,LASTSTEP      AS LASTSTEP")
            sqlStat.AppendLine("         ,SOAAPPDATE")
            sqlStat.AppendLine("         ,REMARK")
            sqlStat.AppendLine("         ,BRID")
            sqlStat.AppendLine("         ,BRCOST")
            sqlStat.AppendLine("         ,DATEFIELD")
            sqlStat.AppendLine("         ,DATEINTERVAL")
            sqlStat.AppendLine("         ,BRADDEDCOST")
            sqlStat.AppendLine("         ,AGENTORGANIZER")
            sqlStat.AppendLine("         ,ACCCURRENCYSEGMENT")
            sqlStat.AppendLine("         ,'" & CONST_FLAG_YES & "'             AS DELFLG")
            sqlStat.AppendLine("         ,INITYMD")
            sqlStat.AppendLine("         ,INITUSER")
            sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
            sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
            sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
            sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
            sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

            sqlStat.AppendLine(" UPDATE GBT0005_ODR_VALUE")
            sqlStat.AppendLine("    SET TANKNO        = @TANKNO")
            If Me.hdnListMapVariant.Value = "GB_Demurrage" Then
                sqlStat.AppendLine("       ,DTLOFFICE      = @DTLOFFICE")
            End If
            sqlStat.AppendLine("       ,COSTCODE      = @COSTCODE")
            sqlStat.AppendLine("       ,CURRENCYCODE  = @CURRENCYCODE")
            sqlStat.AppendLine("       ,TAXATION      = @TAXATION")
            sqlStat.AppendLine("       ,AMOUNTORD     = @AMOUNTORD")
            sqlStat.AppendLine("       ,AMOUNTFIX     = @AMOUNTFIX")
            sqlStat.AppendLine("       ,CONTRACTORODR = @CONTRACTORODR")
            If Me.hdnListMapVariant.Value = "GB_Demurrage" Then
                sqlStat.AppendLine("       ,CONTRACTORFIX = (SELECT CASE WHEN OB.AGENTPOL1 = @DTLOFFICE THEN OB.SHIPPER")
                sqlStat.AppendLine("                                     WHEN OB.AGENTPOD1 = @DTLOFFICE AND CONTRACTORFIX = OB.SHIPPER THEN OB.CONSIGNEE")
                sqlStat.AppendLine("                                     ELSE @CONTRACTORFIX END AS VENDER")
                sqlStat.AppendLine("                           FROM GBT0004_ODR_BASE OB")
                sqlStat.AppendLine("                          WHERE OB.ORDERNO = @ORDERNO")
                sqlStat.AppendLine("                            AND OB.DELFLG <> '" & CONST_FLAG_YES & "'")
                sqlStat.AppendLine("                        )")
            Else
                sqlStat.AppendLine("       ,CONTRACTORFIX = @CONTRACTORFIX")
            End If

            sqlStat.AppendLine("       ,SCHEDELDATE   = @SCHEDELDATE")
            sqlStat.AppendLine("       ,ACTUALDATE    = @ACTUALDATE")
            sqlStat.AppendLine("       ,LOCALRATE     = @LOCALRATE")
            sqlStat.AppendLine("       ,AMOUNTPAY     = @AMOUNTPAY")
            sqlStat.AppendLine("       ,LOCALPAY      = @LOCALPAY")
            sqlStat.AppendLine("       ,INVOICEDBY    = @INVOICEDBY")
            sqlStat.AppendLine("       ,APPLYID       = @APPLYID")
            sqlStat.AppendLine("       ,APPLYTEXT     = @APPLYTEXT")
            sqlStat.AppendLine("       ,LASTSTEP      = @LASTSTEP")
            sqlStat.AppendLine("       ,SOAAPPDATE    = @SOAAPPDATE")
            sqlStat.AppendLine("       ,REMARK        = @REMARK")
            If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                sqlStat.AppendLine("       ,ACCCURRENCYSEGMENT = @ACCCURRENCYSEGMENT")
            End If
            sqlStat.AppendLine("       ,DELFLG        = '" & CONST_FLAG_NO & "'")
            sqlStat.AppendLine("       ,UPDYMD        = @UPDYMD")
            sqlStat.AppendLine("       ,UPDUSER       = @UPDUSER")
            sqlStat.AppendLine("       ,UPDTERMID     = @UPDTERMID")
            sqlStat.AppendLine("       ,RECEIVEYMD    = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE DATAID = @DATAID;")
#Region "DATAID保持対策前"
            'sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
            'sqlStat.AppendLine("      ORDERNO")
            'sqlStat.AppendLine("     ,STYMD")
            'sqlStat.AppendLine("     ,ENDYMD")
            'sqlStat.AppendLine("     ,TANKSEQ")
            'sqlStat.AppendLine("     ,DTLPOLPOD")
            'sqlStat.AppendLine("     ,DTLOFFICE")
            'sqlStat.AppendLine("     ,TANKNO")
            'sqlStat.AppendLine("     ,COSTCODE")
            'sqlStat.AppendLine("     ,ACTIONID")
            'sqlStat.AppendLine("     ,DISPSEQ")
            'sqlStat.AppendLine("     ,LASTACT")
            'sqlStat.AppendLine("     ,REQUIREDACT")
            'sqlStat.AppendLine("     ,ORIGINDESTINATION")
            'sqlStat.AppendLine("     ,COUNTRYCODE")
            'sqlStat.AppendLine("     ,CURRENCYCODE")
            'sqlStat.AppendLine("     ,TAXATION")
            'sqlStat.AppendLine("     ,AMOUNTBR")
            'sqlStat.AppendLine("     ,AMOUNTORD")
            'sqlStat.AppendLine("     ,AMOUNTFIX")
            'sqlStat.AppendLine("     ,CONTRACTORBR")
            'sqlStat.AppendLine("     ,CONTRACTORODR")
            'sqlStat.AppendLine("     ,CONTRACTORFIX")
            'sqlStat.AppendLine("     ,SCHEDELDATEBR")
            'sqlStat.AppendLine("     ,SCHEDELDATE")
            'sqlStat.AppendLine("     ,ACTUALDATE")
            'sqlStat.AppendLine("     ,LOCALBR")
            'sqlStat.AppendLine("     ,LOCALRATE")
            'sqlStat.AppendLine("     ,TAXBR")
            'sqlStat.AppendLine("     ,AMOUNTPAY")
            'sqlStat.AppendLine("     ,LOCALPAY")
            'sqlStat.AppendLine("     ,TAXPAY")
            'sqlStat.AppendLine("     ,INVOICEDBY")
            'sqlStat.AppendLine("     ,APPLYID")
            'sqlStat.AppendLine("     ,APPLYTEXT")
            'sqlStat.AppendLine("     ,LASTSTEP")
            'sqlStat.AppendLine("     ,SOAAPPDATE")
            'sqlStat.AppendLine("     ,REMARK")
            'sqlStat.AppendLine("     ,BRID")
            'sqlStat.AppendLine("     ,BRCOST")
            'sqlStat.AppendLine("     ,DATEFIELD")
            'sqlStat.AppendLine("     ,DATEINTERVAL")
            'sqlStat.AppendLine("     ,BRADDEDCOST")
            'sqlStat.AppendLine("     ,AGENTORGANIZER")
            'sqlStat.AppendLine("     ,DELFLG")
            'sqlStat.AppendLine("     ,INITYMD")
            'sqlStat.AppendLine("     ,UPDYMD")
            'sqlStat.AppendLine("     ,UPDUSER")
            'sqlStat.AppendLine("     ,UPDTERMID")
            'sqlStat.AppendLine("     ,RECEIVEYMD")
            'sqlStat.AppendLine(" ) SELECT ORDERNO")
            'sqlStat.AppendLine("         ,STYMD")
            'sqlStat.AppendLine("         ,ENDYMD")
            'sqlStat.AppendLine("         ,TANKSEQ")
            'sqlStat.AppendLine("         ,DTLPOLPOD")
            'sqlStat.AppendLine("         ,DTLOFFICE")
            'sqlStat.AppendLine("         ,@TANKNO        AS TANKNO")
            'sqlStat.AppendLine("         ,@COSTCODE      AS COSTCODE")
            'sqlStat.AppendLine("         ,ACTIONID")
            'sqlStat.AppendLine("         ,DISPSEQ")
            'sqlStat.AppendLine("         ,LASTACT")
            'sqlStat.AppendLine("         ,REQUIREDACT")
            'sqlStat.AppendLine("         ,ORIGINDESTINATION")
            'sqlStat.AppendLine("         ,COUNTRYCODE")
            'sqlStat.AppendLine("         ,@CURRENCYCODE AS CURRENCYCODE")
            'sqlStat.AppendLine("         ,@TAXATION")
            'sqlStat.AppendLine("         ,AMOUNTBR")
            'sqlStat.AppendLine("         ,@AMOUNTORD     AS AMOUNTORD")
            'sqlStat.AppendLine("         ,@AMOUNTFIX        AS AMOUNTFIX")
            'sqlStat.AppendLine("         ,CONTRACTORBR")
            'sqlStat.AppendLine("         ,@CONTRACTORODR AS CONTRACTORODR")
            'sqlStat.AppendLine("         ,@CONTRACTORFIX AS CONTRACTORFIX")
            'sqlStat.AppendLine("         ,SCHEDELDATEBR")
            'sqlStat.AppendLine("         ,@SCHEDELDATE   AS SCHEDELDATE")
            'sqlStat.AppendLine("         ,@ACTUALDATE    AS ACTUALDATE")
            'sqlStat.AppendLine("         ,LOCALBR")
            'sqlStat.AppendLine("         ,@LOCALRATE     AS LOCALRATE")
            'sqlStat.AppendLine("         ,TAXBR")
            'sqlStat.AppendLine("         ,@AMOUNTPAY")
            'sqlStat.AppendLine("         ,@LOCALPAY")
            'sqlStat.AppendLine("         ,TAXPAY")
            'sqlStat.AppendLine("         ,@INVOICEDBY")
            'sqlStat.AppendLine("         ,@APPLYID       AS APPLYID")
            'sqlStat.AppendLine("         ,@APPLYTEXT     AS APPLYTEXT")
            'sqlStat.AppendLine("         ,@LASTSTEP      AS LASTSTEP")
            'sqlStat.AppendLine("         ,@SOAAPPDATE")
            'sqlStat.AppendLine("         ,REMARK")
            'sqlStat.AppendLine("         ,BRID")
            'sqlStat.AppendLine("         ,BRCOST")
            'sqlStat.AppendLine("         ,DATEFIELD")
            'sqlStat.AppendLine("         ,DATEINTERVAL")
            'sqlStat.AppendLine("         ,BRADDEDCOST")
            'sqlStat.AppendLine("         ,AGENTORGANIZER")
            'sqlStat.AppendLine("         ,'" & CONST_FLAG_NO & "'             AS DELFLG")
            'sqlStat.AppendLine("         ,@UPDYMD         AS INITYMD")
            'sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
            'sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
            'sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
            'sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
            'sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
            'sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

            'sqlStat.AppendLine(" UPDATE GBT0005_ODR_VALUE")
            'sqlStat.AppendLine("    SET DELFLG = '" & CONST_FLAG_YES & "'")
            'sqlStat.AppendLine("       ,UPDYMD    = @UPDYMD")
            'sqlStat.AppendLine("       ,UPDUSER   = @UPDUSER")
            'sqlStat.AppendLine("       ,UPDTERMID = @UPDTERMID")
            'sqlStat.AppendLine("       ,RECEIVEYMD = @RECEIVEYMD ")
            'sqlStat.AppendLine(" WHERE DATAID = @DATAID;")
#End Region

        End If
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@TANKNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("TANKNO"))
                .Add("@COSTCODE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("COSTCODE"))
                .Add("@CURRENCYCODE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CURRENCYCODE"))
                .Add("@TAXATION", SqlDbType.NVarChar).Value = If(Convert.ToString(dr.Item("TAXATION")) = "on", "1", "0")
                .Add("@AMOUNTORD", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMOUNTORD")))
                .Add("@AMOUNTFIX", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMOUNTFIX")))
                .Add("@CONTRACTORODR", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CONTRACTORODR"))
                .Add("@CONTRACTORFIX", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("CONTRACTORFIX"))
                .Add("@SCHEDELDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("SCHEDELDATE")))
                .Add("@ACTUALDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("ACTUALDATE")))
                .Add("@SOAAPPDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(dr.Item("SOAAPPDATE")))

                .Add("@LOCALRATE", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCALRATE")))
                .Add("@AMOUNTPAY", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("AMOUNTPAY")))
                .Add("@LOCALPAY", SqlDbType.Float).Value = DecimalStringToDecimal(Convert.ToString(dr.Item("LOCALPAY")))

                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                If Convert.ToString(dr.Item("JOT")) = "on" Then
                    .Add("@INVOICEDBY", SqlDbType.NVarChar).Value = C_JOT_AGENT
                Else
                    .Add("@INVOICEDBY", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DTLOFFICE"))
                End If
                If ({"Default", "GB_TankActivity"}.Contains(Me.hdnListMapVariant.Value) AndAlso Convert.ToString(dr.Item("IS_ODR_CHANGECOST")) = "1") _
                    OrElse (Me.hdnListMapVariant.Value = "GB_SOA" AndAlso Convert.ToString(dr.Item("IS_FIX_CHANGECOST")) = "1") Then
                    '予定金額・確定金額を変更し保存した場合は申請フィールドをクリア
                    .Add("@APPLYTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = ""
                    .Add("@LASTSTEP", SqlDbType.NVarChar).Value = ""
                Else
                    '上記の除き編集前情報を引継
                    .Add("@APPLYTEXT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("APPLYTEXT"))
                    .Add("@APPLYID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("APPLYID"))
                    .Add("@LASTSTEP", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("LASTSTEP"))
                End If

                .Add("@DATAID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DATAID"))
                .Add("@REMARK", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("REMARK"))
                If Me.hdnListMapVariant.Value = "GB_Demurrage" Then
                    .Add("@DTLOFFICE", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DTLOFFICE"))
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ORDERNO"))
                End If
                If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                    .Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("ACCCURRENCYSEGMENT"))
                End If
            End With

            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function
    ''' <summary>
    ''' オーガナイザのタンクNoを更新
    ''' </summary>
    ''' <param name="orderNo">オーダーNo：テーブル更新条件</param>
    ''' <param name="tankSeq">TANKSEQ：テーブル更新条件</param>
    ''' <param name="tankNo">TANKNO：テーブル更新値</param>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="sqlTran">省略した場合トランザクションを行わない</param>
    ''' <param name="procDate">処理日付</param>
    ''' <returns></returns>
    ''' <remarks>タンク引当に変動があった場合のみ実行する
    ''' オーガナイザの同一オーダー同一TANKシーケンスについて一括UPDATEするため、DATAIDは不要
    ''' 合わせてSHIP日の変更があった場合オーガナイザレコードのSHIP日も更新する
    ''' 
    ''' ※20181211 タンク一覧画面にて本更新を行うためデッドロジック
    ''' 復活させる場合は現在レコードに削除フラグ→新規レコードインサート
    ''' から削除フラグを立てたコピーデータ作成→現在レコードに更新になるよう変更必須！
    ''' </remarks>
    Private Function UpdateOrderValueOrganizerTankNo(orderNo As String, tankSeq As String, tankNo As String,
                                                     ByRef sqlCon As SqlConnection, ByVal shipDate As String, ByVal isShipChanged As String, Optional ByRef sqlTran As SqlTransaction = Nothing,
                                                     Optional procDate As Date = #1900/1/1#) As String
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        'SHIP日が空白の場合はデフォルト日付で更新
        If shipDate = "" Then
            shipDate = "1900/01/01"
        End If
        Dim sqlStat As New StringBuilder
        'SQL文作成

        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("      ORDERNO")
        sqlStat.AppendLine("     ,STYMD")
        sqlStat.AppendLine("     ,ENDYMD")
        sqlStat.AppendLine("     ,TANKSEQ")
        sqlStat.AppendLine("     ,DTLPOLPOD")
        sqlStat.AppendLine("     ,DTLOFFICE")
        sqlStat.AppendLine("     ,TANKNO")
        sqlStat.AppendLine("     ,COSTCODE")
        sqlStat.AppendLine("     ,ACTIONID")
        sqlStat.AppendLine("     ,DISPSEQ")
        sqlStat.AppendLine("     ,LASTACT")
        sqlStat.AppendLine("     ,REQUIREDACT")
        sqlStat.AppendLine("     ,ORIGINDESTINATION")
        sqlStat.AppendLine("     ,COUNTRYCODE")
        sqlStat.AppendLine("     ,CURRENCYCODE")
        sqlStat.AppendLine("     ,TAXATION")
        sqlStat.AppendLine("     ,AMOUNTBR")
        sqlStat.AppendLine("     ,AMOUNTORD")
        sqlStat.AppendLine("     ,AMOUNTFIX")
        sqlStat.AppendLine("     ,CONTRACTORBR")
        sqlStat.AppendLine("     ,CONTRACTORODR")
        sqlStat.AppendLine("     ,CONTRACTORFIX")
        sqlStat.AppendLine("     ,SCHEDELDATEBR")
        sqlStat.AppendLine("     ,SCHEDELDATE")
        sqlStat.AppendLine("     ,ACTUALDATE")
        sqlStat.AppendLine("     ,LOCALBR")
        sqlStat.AppendLine("     ,LOCALRATE")
        sqlStat.AppendLine("     ,TAXBR")
        sqlStat.AppendLine("     ,AMOUNTPAY")
        sqlStat.AppendLine("     ,LOCALPAY")
        sqlStat.AppendLine("     ,TAXPAY")
        sqlStat.AppendLine("     ,INVOICEDBY")
        sqlStat.AppendLine("     ,APPLYID")
        sqlStat.AppendLine("     ,APPLYTEXT")
        sqlStat.AppendLine("     ,LASTSTEP")
        sqlStat.AppendLine("     ,SOAAPPDATE")
        sqlStat.AppendLine("     ,REMARK")
        sqlStat.AppendLine("     ,BRID")
        sqlStat.AppendLine("     ,BRCOST")
        sqlStat.AppendLine("     ,DATEFIELD")
        sqlStat.AppendLine("     ,DATEINTERVAL")
        sqlStat.AppendLine("     ,BRADDEDCOST")
        sqlStat.AppendLine("     ,AGENTORGANIZER")
        sqlStat.AppendLine("     ,DELFLG")
        sqlStat.AppendLine("     ,INITYMD")
        sqlStat.AppendLine("     ,INITUSER")
        sqlStat.AppendLine("     ,UPDYMD")
        sqlStat.AppendLine("     ,UPDUSER")
        sqlStat.AppendLine("     ,UPDTERMID")
        sqlStat.AppendLine("     ,RECEIVEYMD")
        sqlStat.AppendLine(" ) SELECT ORDERNO")
        sqlStat.AppendLine("         ,STYMD")
        sqlStat.AppendLine("         ,ENDYMD")
        sqlStat.AppendLine("         ,TANKSEQ")
        sqlStat.AppendLine("         ,DTLPOLPOD")
        sqlStat.AppendLine("         ,DTLOFFICE")
        sqlStat.AppendLine("         ,@TANKNO        AS TANKNO")
        sqlStat.AppendLine("         ,COSTCODE")
        sqlStat.AppendLine("         ,ACTIONID")
        sqlStat.AppendLine("         ,DISPSEQ")
        sqlStat.AppendLine("         ,LASTACT")
        sqlStat.AppendLine("         ,REQUIREDACT")
        sqlStat.AppendLine("         ,ORIGINDESTINATION")
        sqlStat.AppendLine("         ,COUNTRYCODE")
        sqlStat.AppendLine("         ,CURRENCYCODE")
        sqlStat.AppendLine("         ,TAXATION")
        sqlStat.AppendLine("         ,AMOUNTBR")
        sqlStat.AppendLine("         ,AMOUNTORD")
        sqlStat.AppendLine("         ,AMOUNTFIX")
        sqlStat.AppendLine("         ,CONTRACTORBR")
        sqlStat.AppendLine("         ,CONTRACTORODR")
        sqlStat.AppendLine("         ,CONTRACTORFIX")
        sqlStat.AppendLine("         ,SCHEDELDATEBR")
        sqlStat.AppendLine("         ,SCHEDELDATE")
        If isShipChanged = "1" Then
            sqlStat.AppendLine("         ,CASE WHEN DTLPOLPOD='Organizer' THEN @SHIPDATE ")
            sqlStat.AppendLine("               ELSE ACTUALDATE END AS ACTUALDATE")
        Else
            sqlStat.AppendLine("         ,ACTUALDATE")
        End If

        sqlStat.AppendLine("         ,LOCALBR")
        sqlStat.AppendLine("         ,LOCALRATE")
        sqlStat.AppendLine("         ,TAXBR")
        sqlStat.AppendLine("         ,AMOUNTPAY")
        sqlStat.AppendLine("         ,LOCALPAY")
        sqlStat.AppendLine("         ,TAXPAY")
        sqlStat.AppendLine("         ,INVOICEDBY")
        sqlStat.AppendLine("         ,APPLYID")
        sqlStat.AppendLine("         ,APPLYTEXT")
        sqlStat.AppendLine("         ,LASTSTEP")
        sqlStat.AppendLine("         ,SOAAPPDATE")
        sqlStat.AppendLine("         ,REMARK")
        sqlStat.AppendLine("         ,BRID")
        sqlStat.AppendLine("         ,BRCOST")
        sqlStat.AppendLine("         ,DATEFIELD")
        sqlStat.AppendLine("         ,DATEINTERVAL")
        sqlStat.AppendLine("         ,BRADDEDCOST")
        sqlStat.AppendLine("         ,AGENTORGANIZER")
        sqlStat.AppendLine("         ,'" & CONST_FLAG_NO & "'             AS DELFLG")
        sqlStat.AppendLine("         ,INITYMD")
        sqlStat.AppendLine("         ,INITUSER")
        sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKNO   <> @TANKNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        'sqlStat.AppendLine("   AND DTLPOLPOD = @DTLPOLPOD")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

        sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("   SET UPDYMD         = @UPDYMD")
        sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
        sqlStat.AppendLine("      ,DELFLG         = @DELFLG")
        sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKNO   <> @TANKNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        'sqlStat.AppendLine("   AND DTLPOLPOD = @DTLPOLPOD")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@TANKNO", SqlDbType.NVarChar).Value = tankNo
                If isShipChanged = "1" Then
                    .Add("@SHIPDATE", SqlDbType.Date).Value = Date.Parse(shipDate)
                End If
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankSeq
                '.Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = "Organizer"
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
            End With
            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function
    ''' <summary>
    ''' オーガナイザレコードの日付項目を自動更新
    ''' </summary>
    ''' <param name="shipRecord"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="procDate"></param>
    ''' <returns></returns>
    ''' <remarks>ACTYがSHIPの日付が変更された場合、オーガナイザーのレコードの日付項目を連動し更新</remarks>
    Function UpdateOrderValueOrganizerDate(shipRecord As DataRow,
                                           ByRef sqlCon As SqlConnection, Optional ByRef sqlTran As SqlTransaction = Nothing,
                                           Optional procDate As Date = #1900/1/1#) As String
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Dim sqlStat As New StringBuilder
        'SQL文作成
        Dim shipDate As String = ""
        shipDate = Convert.ToString(shipRecord.Item("ACTUALDATE"))
        If shipDate = "" Then
            shipDate = "1900/01/01"
        End If
        Dim tankSeq As String = Convert.ToString(shipRecord.Item("TANKSEQ"))
        Dim orderNo As String = Convert.ToString(shipRecord.Item("ORDERNO"))
        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("      ORDERNO")
        sqlStat.AppendLine("     ,STYMD")
        sqlStat.AppendLine("     ,ENDYMD")
        sqlStat.AppendLine("     ,TANKSEQ")
        sqlStat.AppendLine("     ,DTLPOLPOD")
        sqlStat.AppendLine("     ,DTLOFFICE")
        sqlStat.AppendLine("     ,TANKNO")
        sqlStat.AppendLine("     ,COSTCODE")
        sqlStat.AppendLine("     ,ACTIONID")
        sqlStat.AppendLine("     ,DISPSEQ")
        sqlStat.AppendLine("     ,LASTACT")
        sqlStat.AppendLine("     ,REQUIREDACT")
        sqlStat.AppendLine("     ,ORIGINDESTINATION")
        sqlStat.AppendLine("     ,COUNTRYCODE")
        sqlStat.AppendLine("     ,CURRENCYCODE")
        sqlStat.AppendLine("     ,TAXATION")
        sqlStat.AppendLine("     ,AMOUNTBR")
        sqlStat.AppendLine("     ,AMOUNTORD")
        sqlStat.AppendLine("     ,AMOUNTFIX")
        sqlStat.AppendLine("     ,CONTRACTORBR")
        sqlStat.AppendLine("     ,CONTRACTORODR")
        sqlStat.AppendLine("     ,CONTRACTORFIX")
        sqlStat.AppendLine("     ,SCHEDELDATEBR")
        sqlStat.AppendLine("     ,SCHEDELDATE")
        sqlStat.AppendLine("     ,ACTUALDATE")
        sqlStat.AppendLine("     ,LOCALBR")
        sqlStat.AppendLine("     ,LOCALRATE")
        sqlStat.AppendLine("     ,TAXBR")
        sqlStat.AppendLine("     ,AMOUNTPAY")
        sqlStat.AppendLine("     ,LOCALPAY")
        sqlStat.AppendLine("     ,TAXPAY")
        sqlStat.AppendLine("     ,INVOICEDBY")
        sqlStat.AppendLine("     ,APPLYID")
        sqlStat.AppendLine("     ,APPLYTEXT")
        sqlStat.AppendLine("     ,LASTSTEP")
        sqlStat.AppendLine("     ,SOAAPPDATE")
        sqlStat.AppendLine("     ,REMARK")
        sqlStat.AppendLine("     ,BRID")
        sqlStat.AppendLine("     ,BRCOST")
        sqlStat.AppendLine("     ,DATEFIELD")
        sqlStat.AppendLine("     ,DATEINTERVAL")
        sqlStat.AppendLine("     ,BRADDEDCOST")
        sqlStat.AppendLine("     ,AGENTORGANIZER")
        sqlStat.AppendLine("     ,DELFLG")
        sqlStat.AppendLine("     ,INITYMD")
        sqlStat.AppendLine("     ,INITUSER")
        sqlStat.AppendLine("     ,UPDYMD")
        sqlStat.AppendLine("     ,UPDUSER")
        sqlStat.AppendLine("     ,UPDTERMID")
        sqlStat.AppendLine("     ,RECEIVEYMD")
        sqlStat.AppendLine(" ) SELECT ORDERNO")
        sqlStat.AppendLine("         ,STYMD")
        sqlStat.AppendLine("         ,ENDYMD")
        sqlStat.AppendLine("         ,TANKSEQ")
        sqlStat.AppendLine("         ,DTLPOLPOD")
        sqlStat.AppendLine("         ,DTLOFFICE")
        sqlStat.AppendLine("         ,TANKNO")
        sqlStat.AppendLine("         ,COSTCODE")
        sqlStat.AppendLine("         ,ACTIONID")
        sqlStat.AppendLine("         ,DISPSEQ")
        sqlStat.AppendLine("         ,LASTACT")
        sqlStat.AppendLine("         ,REQUIREDACT")
        sqlStat.AppendLine("         ,ORIGINDESTINATION")
        sqlStat.AppendLine("         ,COUNTRYCODE")
        sqlStat.AppendLine("         ,CURRENCYCODE")
        sqlStat.AppendLine("         ,TAXATION")
        sqlStat.AppendLine("         ,AMOUNTBR")
        sqlStat.AppendLine("         ,AMOUNTORD")
        sqlStat.AppendLine("         ,AMOUNTFIX")
        sqlStat.AppendLine("         ,CONTRACTORBR")
        sqlStat.AppendLine("         ,CONTRACTORODR")
        sqlStat.AppendLine("         ,CONTRACTORFIX")
        sqlStat.AppendLine("         ,SCHEDELDATEBR")
        sqlStat.AppendLine("         ,SCHEDELDATE")
        sqlStat.AppendLine("         ,ACTUALDATE")
        sqlStat.AppendLine("         ,LOCALBR")
        sqlStat.AppendLine("         ,LOCALRATE")
        sqlStat.AppendLine("         ,TAXBR")
        sqlStat.AppendLine("         ,AMOUNTPAY")
        sqlStat.AppendLine("         ,LOCALPAY")
        sqlStat.AppendLine("         ,TAXPAY")
        sqlStat.AppendLine("         ,INVOICEDBY")
        sqlStat.AppendLine("         ,APPLYID")
        sqlStat.AppendLine("         ,APPLYTEXT")
        sqlStat.AppendLine("         ,LASTSTEP")
        sqlStat.AppendLine("         ,SOAAPPDATE")
        sqlStat.AppendLine("         ,REMARK")
        sqlStat.AppendLine("         ,BRID")
        sqlStat.AppendLine("         ,BRCOST")
        sqlStat.AppendLine("         ,DATEFIELD")
        sqlStat.AppendLine("         ,DATEINTERVAL")
        sqlStat.AppendLine("         ,BRADDEDCOST")
        sqlStat.AppendLine("         ,AGENTORGANIZER")
        sqlStat.AppendLine("         ,'" & CONST_FLAG_YES & "'             AS DELFLG")
        sqlStat.AppendLine("         ,INITYMD")
        sqlStat.AppendLine("         ,INITUSER")
        sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE ORDERNO     = @ORDERNO")
        sqlStat.AppendLine("   AND TANKSEQ     = @TANKSEQ")
        sqlStat.AppendLine("   AND DTLPOLPOD   = @DTLPOLPOD")
        sqlStat.AppendLine("   AND ACTUALDATE <> @SHIPDATE")
        sqlStat.AppendLine("   AND DELFLG     <> @DELFLG")

        sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("   SET UPDYMD         = @UPDYMD")
        sqlStat.AppendLine("      ,ACTUALDATE     = @SHIPDATE")
        sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        sqlStat.AppendLine("   AND DTLPOLPOD = @DTLPOLPOD")
        sqlStat.AppendLine("   AND ACTUALDATE <> @SHIPDATE")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

#Region "DATAID保持対応前"
        'sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        'sqlStat.AppendLine("      ORDERNO")
        'sqlStat.AppendLine("     ,STYMD")
        'sqlStat.AppendLine("     ,ENDYMD")
        'sqlStat.AppendLine("     ,TANKSEQ")
        'sqlStat.AppendLine("     ,DTLPOLPOD")
        'sqlStat.AppendLine("     ,DTLOFFICE")
        'sqlStat.AppendLine("     ,TANKNO")
        'sqlStat.AppendLine("     ,COSTCODE")
        'sqlStat.AppendLine("     ,ACTIONID")
        'sqlStat.AppendLine("     ,DISPSEQ")
        'sqlStat.AppendLine("     ,LASTACT")
        'sqlStat.AppendLine("     ,REQUIREDACT")
        'sqlStat.AppendLine("     ,ORIGINDESTINATION")
        'sqlStat.AppendLine("     ,COUNTRYCODE")
        'sqlStat.AppendLine("     ,CURRENCYCODE")
        'sqlStat.AppendLine("     ,TAXATION")
        'sqlStat.AppendLine("     ,AMOUNTBR")
        'sqlStat.AppendLine("     ,AMOUNTORD")
        'sqlStat.AppendLine("     ,AMOUNTFIX")
        'sqlStat.AppendLine("     ,CONTRACTORBR")
        'sqlStat.AppendLine("     ,CONTRACTORODR")
        'sqlStat.AppendLine("     ,CONTRACTORFIX")
        'sqlStat.AppendLine("     ,SCHEDELDATEBR")
        'sqlStat.AppendLine("     ,SCHEDELDATE")
        'sqlStat.AppendLine("     ,ACTUALDATE")
        'sqlStat.AppendLine("     ,LOCALBR")
        'sqlStat.AppendLine("     ,LOCALRATE")
        'sqlStat.AppendLine("     ,TAXBR")
        'sqlStat.AppendLine("     ,AMOUNTPAY")
        'sqlStat.AppendLine("     ,LOCALPAY")
        'sqlStat.AppendLine("     ,TAXPAY")
        'sqlStat.AppendLine("     ,INVOICEDBY")
        'sqlStat.AppendLine("     ,APPLYID")
        'sqlStat.AppendLine("     ,APPLYTEXT")
        'sqlStat.AppendLine("     ,LASTSTEP")
        'sqlStat.AppendLine("     ,SOAAPPDATE")
        'sqlStat.AppendLine("     ,REMARK")
        'sqlStat.AppendLine("     ,BRID")
        'sqlStat.AppendLine("     ,BRCOST")
        'sqlStat.AppendLine("     ,DATEFIELD")
        'sqlStat.AppendLine("     ,DATEINTERVAL")
        'sqlStat.AppendLine("     ,BRADDEDCOST")
        'sqlStat.AppendLine("     ,AGENTORGANIZER")
        'sqlStat.AppendLine("     ,DELFLG")
        'sqlStat.AppendLine("     ,INITYMD")
        'sqlStat.AppendLine("     ,UPDYMD")
        'sqlStat.AppendLine("     ,UPDUSER")
        'sqlStat.AppendLine("     ,UPDTERMID")
        'sqlStat.AppendLine("     ,RECEIVEYMD")
        'sqlStat.AppendLine(" ) SELECT ORDERNO")
        'sqlStat.AppendLine("         ,STYMD")
        'sqlStat.AppendLine("         ,ENDYMD")
        'sqlStat.AppendLine("         ,TANKSEQ")
        'sqlStat.AppendLine("         ,DTLPOLPOD")
        'sqlStat.AppendLine("         ,DTLOFFICE")
        'sqlStat.AppendLine("         ,TANKNO")
        'sqlStat.AppendLine("         ,COSTCODE")
        'sqlStat.AppendLine("         ,ACTIONID")
        'sqlStat.AppendLine("         ,DISPSEQ")
        'sqlStat.AppendLine("         ,LASTACT")
        'sqlStat.AppendLine("         ,REQUIREDACT")
        'sqlStat.AppendLine("         ,ORIGINDESTINATION")
        'sqlStat.AppendLine("         ,COUNTRYCODE")
        'sqlStat.AppendLine("         ,CURRENCYCODE")
        'sqlStat.AppendLine("         ,TAXATION")
        'sqlStat.AppendLine("         ,AMOUNTBR")
        'sqlStat.AppendLine("         ,AMOUNTORD")
        'sqlStat.AppendLine("         ,AMOUNTFIX")
        'sqlStat.AppendLine("         ,CONTRACTORBR")
        'sqlStat.AppendLine("         ,CONTRACTORODR")
        'sqlStat.AppendLine("         ,CONTRACTORFIX")
        'sqlStat.AppendLine("         ,SCHEDELDATEBR")
        'sqlStat.AppendLine("         ,SCHEDELDATE")
        'sqlStat.AppendLine("         ,@SHIPDATE AS ACTUALDATE")
        'sqlStat.AppendLine("         ,LOCALBR")
        'sqlStat.AppendLine("         ,LOCALRATE")
        'sqlStat.AppendLine("         ,TAXBR")
        'sqlStat.AppendLine("         ,AMOUNTPAY")
        'sqlStat.AppendLine("         ,LOCALPAY")
        'sqlStat.AppendLine("         ,TAXPAY")
        'sqlStat.AppendLine("         ,INVOICEDBY")
        'sqlStat.AppendLine("         ,APPLYID")
        'sqlStat.AppendLine("         ,APPLYTEXT")
        'sqlStat.AppendLine("         ,LASTSTEP")
        'sqlStat.AppendLine("         ,SOAAPPDATE")
        'sqlStat.AppendLine("         ,REMARK")
        'sqlStat.AppendLine("         ,BRID")
        'sqlStat.AppendLine("         ,BRCOST")
        'sqlStat.AppendLine("         ,DATEFIELD")
        'sqlStat.AppendLine("         ,DATEINTERVAL")
        'sqlStat.AppendLine("         ,BRADDEDCOST")
        'sqlStat.AppendLine("         ,AGENTORGANIZER")
        'sqlStat.AppendLine("         ,'" & CONST_FLAG_NO & "'             AS DELFLG")
        'sqlStat.AppendLine("         ,@UPDYMD         AS INITYMD")
        'sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        'sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        'sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        'sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        'sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        'sqlStat.AppendLine(" WHERE ORDERNO     = @ORDERNO")
        'sqlStat.AppendLine("   AND TANKSEQ     = @TANKSEQ")
        'sqlStat.AppendLine("   AND DTLPOLPOD   = @DTLPOLPOD")
        'sqlStat.AppendLine("   AND ACTUALDATE <> @SHIPDATE")
        'sqlStat.AppendLine("   AND DELFLG     <> @DELFLG")

        'sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        'sqlStat.AppendLine("   SET UPDYMD         = @UPDYMD")
        'sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
        'sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
        'sqlStat.AppendLine("      ,DELFLG         = @DELFLG")
        'sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
        'sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        'sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        'sqlStat.AppendLine("   AND DTLPOLPOD = @DTLPOLPOD")
        'sqlStat.AppendLine("   AND ACTUALDATE <> @SHIPDATE")
        'sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
#End Region

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters

                .Add("@SHIPDATE", SqlDbType.Date).Value = Date.Parse(shipDate)
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")


                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankSeq
                .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = "Organizer"
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            End With
            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function
    ''' <summary>
    ''' オーガナイザレコードのSOAAPPDATE日付項目を自動更新
    ''' </summary>
    ''' <param name="totalInvoiceRecord"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="procDate"></param>
    ''' <returns></returns>
    ''' <remarks>ACTYがSHIPの日付が変更された場合、オーガナイザーのレコードの日付項目を連動し更新
    ''' 20190719→ordernewで自動的につくられたオーガナイザーレコード(BRADDEDCOST='2')のみに限定</remarks>
    Function UpdateOrderValueOrganizerSoaDate(totalInvoiceRecord As DataRow,
                                           ByRef sqlCon As SqlConnection, Optional ByRef sqlTran As SqlTransaction = Nothing,
                                           Optional procDate As Date = #1900/1/1#) As String
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Dim sqlStat As New StringBuilder
        'SQL文作成
        Dim soaApDate As String = ""
        soaApDate = Convert.ToString(totalInvoiceRecord.Item("SOAAPPDATE"))
        If soaApDate = "" Then
            soaApDate = "1900/01/01"
        End If
        Dim ttlInvoiceCost As String = Convert.ToString(totalInvoiceRecord.Item("COSTCODE"))
        Dim dataId As String = Convert.ToString(totalInvoiceRecord.Item("DATAID"))
        Dim tankSeq As String = Convert.ToString(totalInvoiceRecord.Item("TANKSEQ"))
        Dim orderNo As String = Convert.ToString(totalInvoiceRecord.Item("ORDERNO"))
        Dim taxation As String = If(Convert.ToString(totalInvoiceRecord.Item("TAXATION")) = "on", "1", "0") ' Convert.ToString(totalInvoiceRecord.Item("TAXATION"))
        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("      ORDERNO")
        sqlStat.AppendLine("     ,STYMD")
        sqlStat.AppendLine("     ,ENDYMD")
        sqlStat.AppendLine("     ,TANKSEQ")
        sqlStat.AppendLine("     ,DTLPOLPOD")
        sqlStat.AppendLine("     ,DTLOFFICE")
        sqlStat.AppendLine("     ,TANKNO")
        sqlStat.AppendLine("     ,COSTCODE")
        sqlStat.AppendLine("     ,ACTIONID")
        sqlStat.AppendLine("     ,DISPSEQ")
        sqlStat.AppendLine("     ,LASTACT")
        sqlStat.AppendLine("     ,REQUIREDACT")
        sqlStat.AppendLine("     ,ORIGINDESTINATION")
        sqlStat.AppendLine("     ,COUNTRYCODE")
        sqlStat.AppendLine("     ,CURRENCYCODE")
        sqlStat.AppendLine("     ,TAXATION")
        sqlStat.AppendLine("     ,AMOUNTBR")
        sqlStat.AppendLine("     ,AMOUNTORD")
        sqlStat.AppendLine("     ,AMOUNTFIX")
        sqlStat.AppendLine("     ,CONTRACTORBR")
        sqlStat.AppendLine("     ,CONTRACTORODR")
        sqlStat.AppendLine("     ,CONTRACTORFIX")
        sqlStat.AppendLine("     ,SCHEDELDATEBR")
        sqlStat.AppendLine("     ,SCHEDELDATE")
        sqlStat.AppendLine("     ,ACTUALDATE")
        sqlStat.AppendLine("     ,LOCALBR")
        sqlStat.AppendLine("     ,LOCALRATE")
        sqlStat.AppendLine("     ,TAXBR")
        sqlStat.AppendLine("     ,AMOUNTPAY")
        sqlStat.AppendLine("     ,LOCALPAY")
        sqlStat.AppendLine("     ,TAXPAY")
        sqlStat.AppendLine("     ,INVOICEDBY")
        sqlStat.AppendLine("     ,APPLYID")
        sqlStat.AppendLine("     ,APPLYTEXT")
        sqlStat.AppendLine("     ,LASTSTEP")
        sqlStat.AppendLine("     ,SOAAPPDATE")
        sqlStat.AppendLine("     ,REMARK")
        sqlStat.AppendLine("     ,BRID")
        sqlStat.AppendLine("     ,BRCOST")
        sqlStat.AppendLine("     ,DATEFIELD")
        sqlStat.AppendLine("     ,DATEINTERVAL")
        sqlStat.AppendLine("     ,BRADDEDCOST")
        sqlStat.AppendLine("     ,AGENTORGANIZER")
        sqlStat.AppendLine("     ,DELFLG")
        sqlStat.AppendLine("     ,INITYMD")
        sqlStat.AppendLine("     ,INITUSER")
        sqlStat.AppendLine("     ,UPDYMD")
        sqlStat.AppendLine("     ,UPDUSER")
        sqlStat.AppendLine("     ,UPDTERMID")
        sqlStat.AppendLine("     ,RECEIVEYMD")
        sqlStat.AppendLine(" ) SELECT ORDERNO")
        sqlStat.AppendLine("         ,STYMD")
        sqlStat.AppendLine("         ,ENDYMD")
        sqlStat.AppendLine("         ,TANKSEQ")
        sqlStat.AppendLine("         ,DTLPOLPOD")
        sqlStat.AppendLine("         ,DTLOFFICE")
        sqlStat.AppendLine("         ,TANKNO")
        sqlStat.AppendLine("         ,COSTCODE")
        sqlStat.AppendLine("         ,ACTIONID")
        sqlStat.AppendLine("         ,DISPSEQ")
        sqlStat.AppendLine("         ,LASTACT")
        sqlStat.AppendLine("         ,REQUIREDACT")
        sqlStat.AppendLine("         ,ORIGINDESTINATION")
        sqlStat.AppendLine("         ,COUNTRYCODE")
        sqlStat.AppendLine("         ,CURRENCYCODE")
        sqlStat.AppendLine("         ,TAXATION")
        sqlStat.AppendLine("         ,AMOUNTBR")
        sqlStat.AppendLine("         ,AMOUNTORD")
        sqlStat.AppendLine("         ,AMOUNTFIX")
        sqlStat.AppendLine("         ,CONTRACTORBR")
        sqlStat.AppendLine("         ,CONTRACTORODR")
        sqlStat.AppendLine("         ,CONTRACTORFIX")
        sqlStat.AppendLine("         ,SCHEDELDATEBR")
        sqlStat.AppendLine("         ,SCHEDELDATE")
        sqlStat.AppendLine("         ,ACTUALDATE")
        sqlStat.AppendLine("         ,LOCALBR")
        sqlStat.AppendLine("         ,LOCALRATE")
        sqlStat.AppendLine("         ,TAXBR")
        sqlStat.AppendLine("         ,AMOUNTPAY")
        sqlStat.AppendLine("         ,LOCALPAY")
        sqlStat.AppendLine("         ,TAXPAY")
        sqlStat.AppendLine("         ,INVOICEDBY")
        sqlStat.AppendLine("         ,APPLYID")
        sqlStat.AppendLine("         ,APPLYTEXT")
        sqlStat.AppendLine("         ,LASTSTEP")
        sqlStat.AppendLine("         ,SOAAPPDATE")
        sqlStat.AppendLine("         ,REMARK")
        sqlStat.AppendLine("         ,BRID")
        sqlStat.AppendLine("         ,BRCOST")
        sqlStat.AppendLine("         ,DATEFIELD")
        sqlStat.AppendLine("         ,DATEINTERVAL")
        sqlStat.AppendLine("         ,BRADDEDCOST")
        sqlStat.AppendLine("         ,AGENTORGANIZER")
        sqlStat.AppendLine("         ,'" & CONST_FLAG_YES & "'             AS DELFLG")
        sqlStat.AppendLine("         ,INITYMD")
        sqlStat.AppendLine("         ,INITUSER")
        sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE ORDERNO     = @ORDERNO")
        sqlStat.AppendLine("   AND TANKSEQ     = @TANKSEQ")
        sqlStat.AppendLine("   AND DTLPOLPOD   = @DTLPOLPOD")
        'sqlStat.AppendLine("   AND SOAAPPDATE  <> @SOAAPPDATE")
        sqlStat.AppendLine("   AND DELFLG       <> @DELFLG")
        sqlStat.AppendLine("   AND DATAID       <> @DATAID")
        sqlStat.AppendLine("   AND BRADDEDCOST   = @BRADDEDCOST")

        sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("   SET UPDYMD         = @UPDYMD")
        sqlStat.AppendLine("      ,TAXATION       = @TAXATION")
        sqlStat.AppendLine("      ,SOAAPPDATE     = @SOAAPPDATE")
        sqlStat.AppendLine("      ,INVOICEDBY     = (SELECT OVS.INVOICEDBY FROM GBT0005_ODR_VALUE OVS WHERE OVS.DATAID = @DATAID AND OVS.DELFLG <> @DELFLG)")
        sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
        sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
        sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        sqlStat.AppendLine("   AND DTLPOLPOD = @DTLPOLPOD")
        'sqlStat.AppendLine("   AND SOAAPPDATE <> @SOAAPPDATE")
        sqlStat.AppendLine("   AND DELFLG       <> @DELFLG")
        sqlStat.AppendLine("   AND DATAID       <> @DATAID")
        sqlStat.AppendLine("   AND BRADDEDCOST   = @BRADDEDCOST")

        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters

                .Add("@SOAAPPDATE", SqlDbType.Date).Value = Date.Parse(soaApDate)
                .Add("@TAXATION", SqlDbType.NVarChar).Value = taxation
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")


                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankSeq
                .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = "Organizer"
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                '.Add("@TTLINVOICECOST", SqlDbType.NVarChar).Value = ttlInvoiceCost
                .Add("@DATAID", SqlDbType.NVarChar).Value = dataId
                .Add("@BRADDEDCOST", SqlDbType.NVarChar).Value = "2"
            End With
            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function
    ''' <summary>
    ''' オーダー(明細)論理削除処理
    ''' </summary>
    ''' <param name="dr"></param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="procDate"></param>
    ''' <returns></returns>
    Private Function DeleteOrderValue(dr As DataRow, ByRef sqlCon As SqlConnection, Optional ByRef sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/1/1#) As String
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Static sqlStat As StringBuilder
        'SQL文作成
        If sqlStat Is Nothing Then
            sqlStat = New StringBuilder
            sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
            sqlStat.AppendLine("   SET DELFLG         = @DELFLG")
            sqlStat.AppendLine("      ,UPDYMD         = @UPDYMD")
            sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
            sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
            sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE DATAID = @DATAID")
        End If
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                .Add("@DATAID", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DATAID"))
            End With

            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function
    ''' <summary>
    ''' デマレッジレコードの登録処理
    ''' </summary>
    ''' <param name="dr">デマレッジの元となるトリガーレコード(From or To)</param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="procDate"></param>
    ''' <returns></returns>
    Private Function EntryDemurrage(dr As DataRow, ByRef sqlCon As SqlConnection, dicDemurrageCalcField As Dictionary(Of String, List(Of String)), Optional ByRef sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/1/1#) As String
        Dim drFrom As DataRow = Nothing
        Dim drTo As DataRow = Nothing
        Dim dtlPolPod As String = Convert.ToString(dr("DTLPOLPOD"))
        '変更された側のレコード判定("2"の場合To"1"の場合From) 
        Dim changedOrigindestination As String = Convert.ToString(dr("ORIGINDESTINATION"))
        If changedOrigindestination = "2" Then
            drTo = dr
            '******************************************
            '上記で設定されたもう片方のレコードを取得
            '******************************************
            '画面表示にFromに該当するレコードが存在していれば取得
            Dim tmpDr = (From savedDr In Me.SavedDt
                         Where savedDr.Item("ORDERNO").Equals(dr.Item("ORDERNO")) _
                        AndAlso savedDr.Item("TANKSEQ").Equals(dr.Item("TANKSEQ")) _
                        AndAlso savedDr.Item("DTLPOLPOD").Equals(dr.Item("DTLPOLPOD")) _
                        AndAlso savedDr.Item("ORIGINDESTINATION").Equals("1")).FirstOrDefault
            '画面表示がない場合はDBに接続し取得
            If tmpDr Is Nothing Then
                tmpDr = GetDumarrageOrderRow(dr, sqlCon, "1", True, sqlTran)
            End If
            drFrom = tmpDr
        Else
            drFrom = dr
            '******************************************
            '上記で設定されたもう片方のレコードを取得
            '******************************************
            '画面表示にFromに該当するレコードが存在していれば取得
            Dim tmpDr = (From savedDr In Me.SavedDt
                         Where savedDr.Item("ORDERNO").Equals(dr.Item("ORDERNO")) _
                        AndAlso savedDr.Item("TANKSEQ").Equals(dr.Item("TANKSEQ")) _
                        AndAlso savedDr.Item("DTLPOLPOD").Equals(dr.Item("DTLPOLPOD")) _
                        AndAlso savedDr.Item("ORIGINDESTINATION").Equals("2")).FirstOrDefault
            '画面表示がない場合はDBに接続し取得
            If tmpDr Is Nothing Then
                tmpDr = GetDumarrageOrderRow(dr, sqlCon, "2", True, sqlTran)
            End If
            drTo = tmpDr
        End If
        'デマのトリガーとなるFrom Toのレコードが取得できない場合はそのまま終了
        If drFrom Is Nothing OrElse drTo Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If

        'intervalの取得
        Dim interval As Decimal = Decimal.Parse(Convert.ToString(drTo.Item(dicDemurrageCalcField(dtlPolPod)(1))))


        '前回計算がある場合を考慮し変動があった場合は更新
        Dim demurrageDate As String = "1900/01/01" '計算に合致しない場合は当レコードが更新される
        Dim demurrageAmount As Decimal = 0         '計算に合致しない場合は当レコードが更新される

        '日付項目が両方埋まっているかチェック
        '埋まっていない場合は当終了
        Dim fromDateString As String = ""
        If drFrom IsNot Nothing Then
            fromDateString = Convert.ToString(drFrom.Item("ACTUALDATE"))
        End If
        Dim toDateString As String = Convert.ToString(drTo.Item("ACTUALDATE"))
        If toDateString <> "" Then
            demurrageDate = Date.Parse(toDateString).ToString("yyyy/MM/dd")
        End If

        Dim dateDiffVal As Decimal = 0
        'デマレージ計算判定を行い対象となる場合は演算を行い金額と日付を転送
        If fromDateString <> "" AndAlso toDateString <> "" Then
            Dim fromDate As Date = Date.Parse(fromDateString)
            Dim toDate As Date = Date.Parse(toDateString)
            dateDiffVal = DateDiff(DateInterval.Day, fromDate, toDate) + 1
            Dim overDateSpan As Decimal = 0
            Dim dmurto As Decimal = Decimal.Parse(Convert.ToString(drTo.Item("DEMURTO")))
            Dim demurusRate1 As Decimal = Decimal.Parse(Convert.ToString(drTo.Item("DEMURUSRATE1")))
            Dim demurusRate2 As Decimal = Decimal.Parse(Convert.ToString(drTo.Item("DEMURUSRATE2")))
            '超過判定日数と比較し超えていた場合
            If interval > 0 AndAlso interval < dateDiffVal Then
                overDateSpan = dateDiffVal - interval
                If overDateSpan <= dmurto Then
                    '一段階目のオーバーで収まっている場合
                    demurrageAmount = overDateSpan * demurusRate1
                Else
                    '二段階目までオーバーしていた場合
                    Dim overSpanSplit1 As Decimal = dmurto
                    Dim overSpanSplit2 As Decimal = overDateSpan - dmurto
                    demurrageAmount = (overSpanSplit1 * demurusRate1) + (overSpanSplit2 * demurusRate2)
                End If
            End If
        End If
        '更新対象のデマレージレコードを取得しDATAIDを特定する
        Dim dumaRow As DataRow = GetDumarrageOrderRow(dr, sqlCon, sqlTran:=sqlTran)

        '更新対象のレコードが存在していない場合はそのまま終了
        If dumaRow IsNot Nothing Then
            '更新処理の実行(計算しない場合も日付を埋める)
            Dim dataId As String = Convert.ToString(dumaRow("DATAID"))
            UpdateDumarrage(dataId, demurrageDate, demurrageAmount, sqlCon, sqlTran, procDate)
        End If
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' デマレージ計算のためのデータ行を取得する
    ''' </summary>
    ''' <param name="dr">デマレージ計算のToのデータ行</param>
    ''' <param name="fromRow">未指定時はデマレージの費用項目</param>
    ''' <returns></returns>
    Private Function GetDumarrageOrderRow(dr As DataRow, ByRef sqlCon As SqlConnection, Optional targetOrigindestination As String = "1", Optional fromRow As Boolean = False, Optional ByRef sqlTran As SqlTransaction = Nothing) As DataRow

        Dim sqlStat As New StringBuilder()
        sqlStat.AppendLine("SELECT ")
        sqlStat.AppendLine("       CONVERT(varchar(36),VL.DATAID)     AS DATAID")
        sqlStat.AppendLine("     , VL.ORDERNO    AS ORDERNO")
        sqlStat.AppendLine("     , VL.TANKSEQ    AS TANKSEQ ")
        sqlStat.AppendLine("     , VL.DTLPOLPOD  AS DTLPOLPOD")
        sqlStat.AppendLine("     , VL.TANKNO     AS TANKNO ")
        sqlStat.AppendLine("     , VL.COSTCODE   AS COSTCODE")
        sqlStat.AppendLine("     , VL.LASTACT       AS LASTACT")
        sqlStat.AppendLine("     , VL.CURRENCYCODE  AS CURRENCYCODE")
        sqlStat.AppendLine("     , VL.AMOUNTBR         AS AMOUNTBR")
        sqlStat.AppendLine("     , VL.AMOUNTORD        AS AMOUNTORD")
        sqlStat.AppendLine("     , VL.AMOUNTFIX        AS AMOUNTFIX")
        sqlStat.AppendLine("     , VL.AMOUNTPAY     AS AMOUNTPAY")
        sqlStat.AppendLine("     , VL.LOCALPAY     AS LOCALPAY")
        sqlStat.AppendLine("     , VL.CONTRACTORBR  AS CONTRACTORBR")
        sqlStat.AppendLine("     , VL.CONTRACTORODR AS CONTRACTORODR")
        sqlStat.AppendLine("     , VL.CONTRACTORFIX AS CONTRACTORFIX")
        sqlStat.AppendLine("     , CASE VL.SCHEDELDATEBR WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATEBR,'yyyy/MM/dd') END AS SCHEDELDATEBR")
        sqlStat.AppendLine("     , CASE VL.SCHEDELDATE   WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SCHEDELDATE,  'yyyy/MM/dd') END AS SCHEDELDATE")
        sqlStat.AppendLine("     , CASE VL.ACTUALDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.ACTUALDATE,   'yyyy/MM/dd') END AS ACTUALDATE")
        sqlStat.AppendLine("     , CASE VL.SOAAPPDATE    WHEN '1900/01/01' THEN '' ELSE FORMAT(VL.SOAAPPDATE,   'yyyy/MM/dd') END AS SOAAPPDATE")

        sqlStat.AppendLine("     , AH.STATUS        AS STATUS")
        sqlStat.AppendLine("     , CASE WHEN VL.INVOICEDBY = @JOTAGENT THEN 'on' ELSE '' END AS JOT")
        sqlStat.AppendLine("     , VL.DATEFIELD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE VL")
        sqlStat.AppendLine("  LEFT JOIN COT0002_APPROVALHIST AH") '承認履歴
        sqlStat.AppendLine("    ON  AH.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  AH.APPLYID      = VL.APPLYID")
        sqlStat.AppendLine("   AND  AH.STEP         = VL.LASTSTEP")
        sqlStat.AppendLine("   AND  AH.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV") 'STATUS用JOIN
        sqlStat.AppendLine("    ON  FV.CLASS        = 'APPROVAL'")
        sqlStat.AppendLine("   AND  FV.KEYCODE      = CASE WHEN AH.STATUS IS NOT NULL THEN AH.STATUS ")
        sqlStat.AppendLine("                               WHEN VL.AMOUNTBR <> VL.AMOUNTORD THEN '" & C_APP_STATUS.APPAGAIN & "'")
        sqlStat.AppendLine("                               ELSE NULL")
        sqlStat.AppendLine("                           END")
        sqlStat.AppendLine("   AND  FV.STYMD    <= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.ENDYMD   >= VL.STYMD")
        sqlStat.AppendLine("   AND  FV.DELFLG   <> @DELFLG")
        If fromRow = False Then
            'SOACLOSE連動済確認JOIN START
            sqlStat.AppendLine("  LEFT JOIN (SELECT DISTINCT JOTSOAVLS.REPORTMONTH,JOTSOAVLS.DATAIDODR FROM GBT0008_JOTSOA_VALUE JOTSOAVLS with(nolock) ")
            sqlStat.AppendLine("        WHERE JOTSOAVLS.SOAAPPDATE   <> @INITSOAAPDATE")
            sqlStat.AppendLine("          AND JOTSOAVLS.COSTCODE      = @COSTCODE")
            sqlStat.AppendLine("          AND JOTSOAVLS.CLOSINGMONTH  = JOTSOAVLS.REPORTMONTH")
            sqlStat.AppendLine("          AND JOTSOAVLS.DELFLG       <> @DELFLG")
            sqlStat.AppendLine("             ) JOTSOAVL")
            sqlStat.AppendLine("    ON JOTSOAVL.DATAIDODR   = VL.DATAID")
            'SOACLOSE連動済確認JOIN END
        End If
        sqlStat.AppendLine(" WHERE VL.ORDERNO    = @ORDERNO")
        sqlStat.AppendLine("   AND VL.TANKSEQ    = @TANKSEQ")
        sqlStat.AppendLine("   AND VL.DTLPOLPOD  = @DTLPOLPOD")
        sqlStat.AppendLine("   AND VL.DELFLG    <> @DELFLG")
        If fromRow = False Then
            sqlStat.AppendLine("   AND VL.COSTCODE   = @COSTCODE")
            'デマレコードの金額変更中Applyingはデマ再計算対象外（抽出させない）
            sqlStat.AppendLine("   AND (      AH.STATUS IS NULL ")
            sqlStat.AppendLine("         OR  (      AH.STATUS IS NOT NULL ")
            sqlStat.AppendFormat("              AND AH.STATUS <> '{0}'", C_APP_STATUS.APPLYING, C_APP_STATUS.APPROVED).AppendLine()
            sqlStat.AppendLine("             )")
            sqlStat.AppendLine("       )")
            'デマレコードがSOAJOTVALに連動されたら再計算対象外、SOA承認ではJOTSOAVALUEに登録されるため遅い
            sqlStat.AppendLine("   AND JOTSOAVL.DATAIDODR IS NULL")
        Else
            sqlStat.AppendLine("   AND VL.ORIGINDESTINATION   = @ORIGINDESTINATION")
        End If
        sqlStat.AppendLine("   AND EXISTS(SELECT 1 ") '基本情報が削除されていたら対象外
        sqlStat.AppendLine("                FROM GBT0004_ODR_BASE OBSS")
        sqlStat.AppendLine("               WHERE OBSS.ORDERNO = VL.ORDERNO")
        sqlStat.AppendLine("                 AND OBSS.DELFLG    <> @DELFLG)")

        Dim dtDbResult As New DataTable
        'DB接続
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)

            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("ORDERNO"))
                .Add("@TANKSEQ", SqlDbType.NVarChar, 20).Value = Convert.ToString(dr.Item("TANKSEQ"))
                .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = Convert.ToString(dr.Item("DTLPOLPOD"))
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")

                If fromRow = False Then
                    .Add("@COSTCODE", SqlDbType.NVarChar).Value = GBC_COSTCODE_DEMURRAGE
                    .Add("@INITSOAAPDATE", SqlDbType.Date).Value = "1900/01/01"
                Else
                    .Add("@ORIGINDESTINATION", SqlDbType.NVarChar).Value = targetOrigindestination
                End If
                .Add("@JOTAGENT", SqlDbType.NVarChar).Value = C_JOT_AGENT
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using

        Dim retDt As DataTable = CreateOrderListTable()
        Dim loopEnd As Integer = 1
        Integer.TryParse(Me.hdnCopy.Value, loopEnd)
        Dim colNameList As New List(Of String)
        For Each colOb As DataColumn In dtDbResult.Columns
            If retDt.Columns.Contains(colOb.ColumnName) Then
                colNameList.Add(colOb.ColumnName)
            End If
        Next
        For Each readDr As DataRow In dtDbResult.Rows
            '同一カラム名を単純転送
            Dim writeDr As DataRow = retDt.NewRow
            For Each colName In colNameList
                writeDr.Item(colName) = readDr.Item(colName)
            Next
            retDt.Rows.Add(writeDr)
        Next

        If retDt Is Nothing OrElse retDt.Rows.Count = 0 Then
            Return Nothing
        Else
            Return retDt.Rows(0)
        End If

    End Function
    ''' <summary>
    ''' デマレッジレコードの更新
    ''' </summary>
    ''' <param name="dataId">更新対象のデマレージレコードID</param>
    ''' <param name="targetDate">対象日付</param>
    ''' <param name="targetAmount">対象金額</param>
    ''' <param name="sqlCon"></param>
    ''' <param name="sqlTran"></param>
    ''' <param name="procDate"></param>
    ''' <returns></returns>
    Private Function UpdateDumarrage(dataId As String, targetDate As String, targetAmount As Decimal, ByRef sqlCon As SqlConnection, Optional ByRef sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/1/1#) As String
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Static sqlStat As StringBuilder '何度も作成ロジックを通さないためスタティックスコープ
        'SQL文作成
        If sqlStat Is Nothing Then
            sqlStat = New StringBuilder
            sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
            sqlStat.AppendLine("      ORDERNO")
            sqlStat.AppendLine("     ,STYMD")
            sqlStat.AppendLine("     ,ENDYMD")
            sqlStat.AppendLine("     ,TANKSEQ")
            sqlStat.AppendLine("     ,DTLPOLPOD")
            sqlStat.AppendLine("     ,DTLOFFICE")
            sqlStat.AppendLine("     ,TANKNO")
            sqlStat.AppendLine("     ,COSTCODE")
            sqlStat.AppendLine("     ,ACTIONID")
            sqlStat.AppendLine("     ,DISPSEQ")
            sqlStat.AppendLine("     ,LASTACT")
            sqlStat.AppendLine("     ,REQUIREDACT")
            sqlStat.AppendLine("     ,ORIGINDESTINATION")
            sqlStat.AppendLine("     ,COUNTRYCODE")
            sqlStat.AppendLine("     ,CURRENCYCODE")
            sqlStat.AppendLine("     ,TAXATION")
            sqlStat.AppendLine("     ,AMOUNTBR")
            sqlStat.AppendLine("     ,AMOUNTORD")
            sqlStat.AppendLine("     ,AMOUNTFIX")
            sqlStat.AppendLine("     ,CONTRACTORBR")
            sqlStat.AppendLine("     ,CONTRACTORODR")
            sqlStat.AppendLine("     ,CONTRACTORFIX")
            sqlStat.AppendLine("     ,SCHEDELDATEBR")
            sqlStat.AppendLine("     ,SCHEDELDATE")
            sqlStat.AppendLine("     ,ACTUALDATE")
            sqlStat.AppendLine("     ,LOCALBR")
            sqlStat.AppendLine("     ,LOCALRATE")
            sqlStat.AppendLine("     ,TAXBR")
            sqlStat.AppendLine("     ,AMOUNTPAY")
            sqlStat.AppendLine("     ,LOCALPAY")
            sqlStat.AppendLine("     ,TAXPAY")
            sqlStat.AppendLine("     ,INVOICEDBY")
            sqlStat.AppendLine("     ,APPLYID")
            sqlStat.AppendLine("     ,APPLYTEXT")
            sqlStat.AppendLine("     ,LASTSTEP")
            sqlStat.AppendLine("     ,SOAAPPDATE")
            sqlStat.AppendLine("     ,REMARK")
            sqlStat.AppendLine("     ,BRID")
            sqlStat.AppendLine("     ,BRCOST")
            sqlStat.AppendLine("     ,DATEFIELD")
            sqlStat.AppendLine("     ,DATEINTERVAL")
            sqlStat.AppendLine("     ,BRADDEDCOST")
            sqlStat.AppendLine("     ,AGENTORGANIZER")
            sqlStat.AppendLine("     ,DELFLG")
            sqlStat.AppendLine("     ,INITYMD")
            sqlStat.AppendLine("     ,INITUSER")
            sqlStat.AppendLine("     ,UPDYMD")
            sqlStat.AppendLine("     ,UPDUSER")
            sqlStat.AppendLine("     ,UPDTERMID")
            sqlStat.AppendLine("     ,RECEIVEYMD")
            sqlStat.AppendLine(" ) SELECT ORDERNO")
            sqlStat.AppendLine("         ,STYMD")
            sqlStat.AppendLine("         ,ENDYMD")
            sqlStat.AppendLine("         ,TANKSEQ")
            sqlStat.AppendLine("         ,DTLPOLPOD")
            sqlStat.AppendLine("         ,DTLOFFICE")
            sqlStat.AppendLine("         ,TANKNO")
            sqlStat.AppendLine("         ,COSTCODE")
            sqlStat.AppendLine("         ,ACTIONID")
            sqlStat.AppendLine("         ,DISPSEQ")
            sqlStat.AppendLine("         ,LASTACT")
            sqlStat.AppendLine("         ,REQUIREDACT")
            sqlStat.AppendLine("         ,ORIGINDESTINATION")
            sqlStat.AppendLine("         ,COUNTRYCODE")
            sqlStat.AppendLine("         ,CURRENCYCODE")
            sqlStat.AppendLine("         ,TAXATION")
            sqlStat.AppendLine("         ,AMOUNTBR")
            sqlStat.AppendLine("         ,AMOUNTORD")
            sqlStat.AppendLine("         ,AMOUNTFIX")
            sqlStat.AppendLine("         ,CONTRACTORBR")
            sqlStat.AppendLine("         ,CONTRACTORODR")
            sqlStat.AppendLine("         ,CONTRACTORFIX")
            sqlStat.AppendLine("         ,SCHEDELDATEBR")
            sqlStat.AppendLine("         ,SCHEDELDATE")
            sqlStat.AppendLine("         ,ACTUALDATE")
            sqlStat.AppendLine("         ,LOCALBR")
            sqlStat.AppendLine("         ,LOCALRATE")
            sqlStat.AppendLine("         ,TAXBR")
            sqlStat.AppendLine("         ,AMOUNTPAY")
            sqlStat.AppendLine("         ,LOCALPAY")
            sqlStat.AppendLine("         ,TAXPAY")
            sqlStat.AppendLine("         ,INVOICEDBY")
            sqlStat.AppendLine("         ,APPLYID")
            sqlStat.AppendLine("         ,APPLYTEXT")
            sqlStat.AppendLine("         ,LASTSTEP")
            sqlStat.AppendLine("         ,SOAAPPDATE")
            sqlStat.AppendLine("         ,REMARK")
            sqlStat.AppendLine("         ,BRID")
            sqlStat.AppendLine("         ,BRCOST")
            sqlStat.AppendLine("         ,DATEFIELD")
            sqlStat.AppendLine("         ,DATEINTERVAL")
            sqlStat.AppendLine("         ,BRADDEDCOST")
            sqlStat.AppendLine("         ,AGENTORGANIZER")
            sqlStat.AppendLine("         ,'" & CONST_FLAG_YES & "'             AS DELFLG")
            sqlStat.AppendLine("         ,INITYMD")
            sqlStat.AppendLine("         ,INITUSER")
            sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
            sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
            sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
            sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
            sqlStat.AppendLine(" WHERE DATAID   = @DATAID;")

            sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
            sqlStat.AppendLine("   SET TAXATION       = @TAXATION")
            sqlStat.AppendLine("      ,AMOUNTBR       = @AMOUNTBR")
            sqlStat.AppendLine("      ,AMOUNTORD      = @AMOUNTORD")
            sqlStat.AppendLine("      ,AMOUNTFIX      = @AMOUNTORD")
            sqlStat.AppendLine("      ,SCHEDELDATEBR  = @SCHEDELDATEBR")
            sqlStat.AppendLine("      ,SCHEDELDATE    = @SCHEDELDATE")
            sqlStat.AppendLine("      ,ACTUALDATE     = @ACTUALDATE")
            sqlStat.AppendLine("      ,UPDYMD         = @UPDYMD")
            sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
            sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
            sqlStat.AppendLine("      ,DELFLG         = '" & CONST_FLAG_NO & "'")
            sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
            sqlStat.AppendLine(" WHERE DATAID   = @DATAID;")
#Region "DATAID保持対策前"
            'sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
            'sqlStat.AppendLine("      ORDERNO")
            'sqlStat.AppendLine("     ,STYMD")
            'sqlStat.AppendLine("     ,ENDYMD")
            'sqlStat.AppendLine("     ,TANKSEQ")
            'sqlStat.AppendLine("     ,DTLPOLPOD")
            'sqlStat.AppendLine("     ,DTLOFFICE")
            'sqlStat.AppendLine("     ,TANKNO")
            'sqlStat.AppendLine("     ,COSTCODE")
            'sqlStat.AppendLine("     ,ACTIONID")
            'sqlStat.AppendLine("     ,DISPSEQ")
            'sqlStat.AppendLine("     ,LASTACT")
            'sqlStat.AppendLine("     ,REQUIREDACT")
            'sqlStat.AppendLine("     ,ORIGINDESTINATION")
            'sqlStat.AppendLine("     ,COUNTRYCODE")
            'sqlStat.AppendLine("     ,CURRENCYCODE")
            'sqlStat.AppendLine("     ,TAXATION")
            'sqlStat.AppendLine("     ,AMOUNTBR")
            'sqlStat.AppendLine("     ,AMOUNTORD")
            'sqlStat.AppendLine("     ,AMOUNTFIX")
            'sqlStat.AppendLine("     ,CONTRACTORBR")
            'sqlStat.AppendLine("     ,CONTRACTORODR")
            'sqlStat.AppendLine("     ,CONTRACTORFIX")
            'sqlStat.AppendLine("     ,SCHEDELDATEBR")
            'sqlStat.AppendLine("     ,SCHEDELDATE")
            'sqlStat.AppendLine("     ,ACTUALDATE")
            'sqlStat.AppendLine("     ,LOCALBR")
            'sqlStat.AppendLine("     ,LOCALRATE")
            'sqlStat.AppendLine("     ,TAXBR")
            'sqlStat.AppendLine("     ,AMOUNTPAY")
            'sqlStat.AppendLine("     ,LOCALPAY")
            'sqlStat.AppendLine("     ,TAXPAY")
            'sqlStat.AppendLine("     ,INVOICEDBY")
            'sqlStat.AppendLine("     ,APPLYID")
            'sqlStat.AppendLine("     ,APPLYTEXT")
            'sqlStat.AppendLine("     ,LASTSTEP")
            'sqlStat.AppendLine("     ,SOAAPPDATE")
            'sqlStat.AppendLine("     ,REMARK")
            'sqlStat.AppendLine("     ,BRID")
            'sqlStat.AppendLine("     ,BRCOST")
            'sqlStat.AppendLine("     ,DATEFIELD")
            'sqlStat.AppendLine("     ,DATEINTERVAL")
            'sqlStat.AppendLine("     ,BRADDEDCOST")
            'sqlStat.AppendLine("     ,AGENTORGANIZER")
            'sqlStat.AppendLine("     ,DELFLG")
            'sqlStat.AppendLine("     ,INITYMD")
            'sqlStat.AppendLine("     ,UPDYMD")
            'sqlStat.AppendLine("     ,UPDUSER")
            'sqlStat.AppendLine("     ,UPDTERMID")
            'sqlStat.AppendLine("     ,RECEIVEYMD")
            'sqlStat.AppendLine(" ) SELECT ORDERNO")
            'sqlStat.AppendLine("         ,STYMD")
            'sqlStat.AppendLine("         ,ENDYMD")
            'sqlStat.AppendLine("         ,TANKSEQ")
            'sqlStat.AppendLine("         ,DTLPOLPOD")
            'sqlStat.AppendLine("         ,DTLOFFICE")
            'sqlStat.AppendLine("         ,TANKNO")
            'sqlStat.AppendLine("         ,COSTCODE")
            'sqlStat.AppendLine("         ,ACTIONID")
            'sqlStat.AppendLine("         ,DISPSEQ")
            'sqlStat.AppendLine("         ,LASTACT")
            'sqlStat.AppendLine("         ,REQUIREDACT")
            'sqlStat.AppendLine("         ,ORIGINDESTINATION")
            'sqlStat.AppendLine("         ,COUNTRYCODE")
            'sqlStat.AppendLine("         ,CURRENCYCODE")
            'sqlStat.AppendLine("         ,@TAXATION")
            'sqlStat.AppendLine("         ,@AMOUNTBR")
            'sqlStat.AppendLine("         ,@AMOUNTORD")
            'sqlStat.AppendLine("         ,@AMOUNTORD")
            'sqlStat.AppendLine("         ,CONTRACTORBR")
            'sqlStat.AppendLine("         ,CONTRACTORODR")
            'sqlStat.AppendLine("         ,CONTRACTORFIX")
            'sqlStat.AppendLine("         ,@SCHEDELDATEBR")
            'sqlStat.AppendLine("         ,@SCHEDELDATE")
            'sqlStat.AppendLine("         ,@SCHEDELDATE")
            'sqlStat.AppendLine("         ,LOCALBR")
            'sqlStat.AppendLine("         ,LOCALRATE")
            'sqlStat.AppendLine("         ,TAXBR")
            'sqlStat.AppendLine("         ,AMOUNTPAY")
            'sqlStat.AppendLine("         ,LOCALPAY")
            'sqlStat.AppendLine("         ,TAXPAY")
            'sqlStat.AppendLine("         ,INVOICEDBY")
            'sqlStat.AppendLine("         ,APPLYID")
            'sqlStat.AppendLine("         ,APPLYTEXT")
            'sqlStat.AppendLine("         ,LASTSTEP")
            'sqlStat.AppendLine("         ,SOAAPPDATE")
            'sqlStat.AppendLine("         ,REMARK")
            'sqlStat.AppendLine("         ,BRID")
            'sqlStat.AppendLine("         ,BRCOST")
            'sqlStat.AppendLine("         ,DATEFIELD")
            'sqlStat.AppendLine("         ,DATEINTERVAL")
            'sqlStat.AppendLine("         ,BRADDEDCOST")
            'sqlStat.AppendLine("         ,AGENTORGANIZER")
            'sqlStat.AppendLine("         ,'0'             AS DELFLG")
            'sqlStat.AppendLine("         ,@UPDYMD         AS INITYMD")
            'sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
            'sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
            'sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
            'sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
            'sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
            'sqlStat.AppendLine(" WHERE DATAID   = @DATAID")

            'sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
            'sqlStat.AppendLine("   SET UPDYMD         = @UPDYMD")
            'sqlStat.AppendLine("      ,UPDUSER        = @UPDUSER")
            'sqlStat.AppendLine("      ,UPDTERMID      = @UPDTERMID")
            'sqlStat.AppendLine("      ,DELFLG         = @DELFLG")
            'sqlStat.AppendLine("      ,RECEIVEYMD     = @RECEIVEYMD ")
            'sqlStat.AppendLine(" WHERE DATAID   = @DATAID")
#End Region


        End If
        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                'デマレコードの課税フラグは'0'(無し)固定
                .Add("@TAXATION", SqlDbType.NVarChar).Value = "0"
                .Add("@DATAID", SqlDbType.NVarChar).Value = dataId
                '
                .Add("@AMOUNTBR", SqlDbType.Float).Value = targetAmount
                .Add("@AMOUNTORD", SqlDbType.Float).Value = targetAmount
                .Add("@SCHEDELDATEBR", SqlDbType.Date).Value = targetDate
                .Add("@SCHEDELDATE", SqlDbType.Date).Value = targetDate
                .Add("@ACTUALDATE", SqlDbType.Date).Value = "1900/01/01"
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
            End With

            sqlCmd.ExecuteNonQuery()
        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function
    ''' <summary>
    ''' 強制締め対象オーダー（明細）テーブル更新処理
    ''' </summary>
    ''' <param name="dt">強制締め対象のデータ</param>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="sqlTran">トランザクションデータ</param>
    ''' <param name="procDate">処理日</param>
    ''' <returns></returns>
    Private Function UpdateForceCloseOrderValue(dt As DataTable, ByRef sqlCon As SqlConnection, Optional ByRef sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/1/1#) As String
        'オプション引数が指定されていない場合は当日日付(呼出し側の引数を省略すれば自動で都度時刻になる)
        If procDate.ToString("yyyy/MM/dd") = "1900/01/01" Then
            procDate = Now
        End If
        Dim sqlStat As New StringBuilder
        'SQL文作成

        sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
        sqlStat.AppendLine("      ORDERNO")
        sqlStat.AppendLine("     ,STYMD")
        sqlStat.AppendLine("     ,ENDYMD")
        sqlStat.AppendLine("     ,TANKSEQ")
        sqlStat.AppendLine("     ,DTLPOLPOD")
        sqlStat.AppendLine("     ,DTLOFFICE")
        sqlStat.AppendLine("     ,TANKNO")
        sqlStat.AppendLine("     ,COSTCODE")
        sqlStat.AppendLine("     ,ACTIONID")
        sqlStat.AppendLine("     ,DISPSEQ")
        sqlStat.AppendLine("     ,LASTACT")
        sqlStat.AppendLine("     ,REQUIREDACT")
        sqlStat.AppendLine("     ,ORIGINDESTINATION")
        sqlStat.AppendLine("     ,COUNTRYCODE")
        sqlStat.AppendLine("     ,CURRENCYCODE")
        sqlStat.AppendLine("     ,TAXATION")
        sqlStat.AppendLine("     ,AMOUNTBR")
        sqlStat.AppendLine("     ,AMOUNTORD")
        sqlStat.AppendLine("     ,AMOUNTFIX")
        sqlStat.AppendLine("     ,CONTRACTORBR")
        sqlStat.AppendLine("     ,CONTRACTORODR")
        sqlStat.AppendLine("     ,CONTRACTORFIX")
        sqlStat.AppendLine("     ,SCHEDELDATEBR")
        sqlStat.AppendLine("     ,SCHEDELDATE")
        sqlStat.AppendLine("     ,ACTUALDATE")
        sqlStat.AppendLine("     ,LOCALBR")
        sqlStat.AppendLine("     ,LOCALRATE")
        sqlStat.AppendLine("     ,TAXBR")
        sqlStat.AppendLine("     ,AMOUNTPAY")
        sqlStat.AppendLine("     ,LOCALPAY")
        sqlStat.AppendLine("     ,TAXPAY")
        sqlStat.AppendLine("     ,INVOICEDBY")
        sqlStat.AppendLine("     ,APPLYID")
        sqlStat.AppendLine("     ,APPLYTEXT")
        sqlStat.AppendLine("     ,LASTSTEP")
        sqlStat.AppendLine("     ,SOAAPPDATE")
        sqlStat.AppendLine("     ,REMARK")
        sqlStat.AppendLine("     ,BRID")
        sqlStat.AppendLine("     ,BRCOST")
        sqlStat.AppendLine("     ,DATEFIELD")
        sqlStat.AppendLine("     ,DATEINTERVAL")
        sqlStat.AppendLine("     ,BRADDEDCOST")
        sqlStat.AppendLine("     ,AGENTORGANIZER")
        sqlStat.AppendLine("     ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("     ,DELFLG")
        sqlStat.AppendLine("     ,INITYMD")
        sqlStat.AppendLine("     ,INITUSER")
        sqlStat.AppendLine("     ,UPDYMD")
        sqlStat.AppendLine("     ,UPDUSER")
        sqlStat.AppendLine("     ,UPDTERMID")
        sqlStat.AppendLine("     ,RECEIVEYMD")
        sqlStat.AppendLine(" ) SELECT ORDERNO")
        sqlStat.AppendLine("         ,STYMD")
        sqlStat.AppendLine("         ,ENDYMD")
        sqlStat.AppendLine("         ,TANKSEQ")
        sqlStat.AppendLine("         ,DTLPOLPOD")
        sqlStat.AppendLine("         ,DTLOFFICE")
        sqlStat.AppendLine("         ,TANKNO")
        sqlStat.AppendLine("         ,COSTCODE")
        sqlStat.AppendLine("         ,ACTIONID")
        sqlStat.AppendLine("         ,DISPSEQ")
        sqlStat.AppendLine("         ,LASTACT")
        sqlStat.AppendLine("         ,REQUIREDACT")
        sqlStat.AppendLine("         ,ORIGINDESTINATION")
        sqlStat.AppendLine("         ,COUNTRYCODE")
        sqlStat.AppendLine("         ,CURRENCYCODE")
        sqlStat.AppendLine("         ,TAXATION")
        sqlStat.AppendLine("         ,AMOUNTBR")
        sqlStat.AppendLine("         ,AMOUNTORD")
        sqlStat.AppendLine("         ,AMOUNTFIX")
        sqlStat.AppendLine("         ,CONTRACTORBR")
        sqlStat.AppendLine("         ,CONTRACTORODR")
        sqlStat.AppendLine("         ,CONTRACTORFIX")
        sqlStat.AppendLine("         ,SCHEDELDATEBR")
        sqlStat.AppendLine("         ,SCHEDELDATE")
        sqlStat.AppendLine("         ,ACTUALDATE")
        sqlStat.AppendLine("         ,LOCALBR")
        sqlStat.AppendLine("         ,LOCALRATE")
        sqlStat.AppendLine("         ,TAXBR")
        sqlStat.AppendLine("         ,AMOUNTPAY")
        sqlStat.AppendLine("         ,LOCALPAY")
        sqlStat.AppendLine("         ,TAXPAY")
        sqlStat.AppendLine("         ,INVOICEDBY")
        sqlStat.AppendLine("         ,APPLYID       AS APPLYID")
        sqlStat.AppendLine("         ,APPLYTEXT     AS APPLYTEXT")
        sqlStat.AppendLine("         ,LASTSTEP      AS LASTSTEP")
        sqlStat.AppendLine("         ,SOAAPPDATE")
        sqlStat.AppendLine("         ,REMARK")
        sqlStat.AppendLine("         ,BRID")
        sqlStat.AppendLine("         ,BRCOST")
        sqlStat.AppendLine("         ,DATEFIELD")
        sqlStat.AppendLine("         ,DATEINTERVAL")
        sqlStat.AppendLine("         ,BRADDEDCOST")
        sqlStat.AppendLine("         ,AGENTORGANIZER")
        sqlStat.AppendLine("         ,ACCCURRENCYSEGMENT")
        sqlStat.AppendLine("         ,'" & CONST_FLAG_YES & "'             AS DELFLG")
        sqlStat.AppendLine("         ,INITYMD")
        sqlStat.AppendLine("         ,INITUSER")
        sqlStat.AppendLine("         ,@UPDYMD         AS UPDYMD")
        sqlStat.AppendLine("         ,@UPDUSER        AS UPDUSER")
        sqlStat.AppendLine("         ,@UPDTERMID      AS UPDTERMID")
        sqlStat.AppendLine("         ,@RECEIVEYMD     AS RECEIVEYMD")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
        sqlStat.AppendLine(" WHERE DATAID = @DATAID;")

        sqlStat.AppendLine(" UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("    SET SOAAPPDATE       = @SOAAPPDATE")
        sqlStat.AppendLine("       ,AMOUNTFIX     = @AMOUNTFIX")
        sqlStat.AppendLine("       ,ACCCREYEN     = @ACCCREYEN")
        sqlStat.AppendLine("       ,ACCCREFOREIGN = @ACCCREFOREIGN")

        sqlStat.AppendLine("       ,FORCECLOSED      = @FORCECLOSED")
        sqlStat.AppendLine("       ,AMOUNTFIXBFC     = AMOUNTFIX")
        sqlStat.AppendLine("       ,ACCCREYENBFC     = ACCCREYENBFC")
        sqlStat.AppendLine("       ,ACCCREFOREIGNBFC = ACCCREFOREIGNBFC")

        sqlStat.AppendLine("       ,DELFLG        = '" & CONST_FLAG_NO & "'")
        sqlStat.AppendLine("       ,UPDYMD        = @UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER       = @UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID     = @UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD    = @RECEIVEYMD ")
        sqlStat.AppendLine(" WHERE DATAID = @DATAID;")


        Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon, sqlTran)
            'SQLパラメータ設定
            Dim paramDataId = sqlCmd.Parameters.Add("@DATAID", SqlDbType.NVarChar)
            With sqlCmd.Parameters
                .Add("@SOAAPPDATE", SqlDbType.Date).Value = procDate
                .Add("@FORCECLOSED", SqlDbType.Float).Value = "1"
                .Add("@AMOUNTFIX", SqlDbType.Float).Value = 0
                .Add("@ACCCREYEN", SqlDbType.Float).Value = 0
                .Add("@ACCCREFOREIGN", SqlDbType.Float).Value = 0

                .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD

            End With
            For Each dr As DataRow In dt.Rows
                paramDataId.Value = Convert.ToString(dr("DATAID"))
                sqlCmd.ExecuteNonQuery()
            Next

        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function

    ''' <summary>
    ''' ノンブレーカーNo(オーダーNo)をシーケンスより取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetNonBrNo(Optional ByRef sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim orderNo As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT  'NB' ")
            sqlStat.AppendLine("      + left(convert(char,getdate(),12),4)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + (SELECT VALUE1")
            sqlStat.AppendLine("           FROM COS0017_FIXVALUE")
            sqlStat.AppendLine("          WHERE CLASS   = @CLASS")
            sqlStat.AppendLine("            AND KEYCODE = @KEYCODE")
            sqlStat.AppendLine("            AND STYMD  <= @STYMD")
            sqlStat.AppendLine("            AND ENDYMD >= @ENDYMD")
            sqlStat.AppendLine("            AND DELFLG <> @DELFLG)")
            sqlStat.AppendLine("      + '-'")
            sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR " & C_SQLSEQ.NONBREAKER & ")),4)")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = C_SERVERSEQ
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
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
    ''' EXCELファイルアップロード入力処理
    ''' </summary>
    Protected Sub UploadExcel()
        Dim COA0029XlsTable As New BASEDLL.COA0029XlsTable
        Dim reportId As String = Me.lbRightList.SelectedItem.Value
        Dim reportMapId As String = CONST_MAPID & hdnListMapVariant.Value
        ''初期処理
        'errList = New List(Of String)
        'errListAll = New List(Of String)
        Dim returnCode As String = C_MESSAGENO.NORMAL
        Dim sheetName As String = "GBT00004"
        'If Not {"GB_SOA", "GB_Demurrage"}.Contains(Me.hdnListMapVariant.Value) Then
        If Not {"GB_Demurrage"}.Contains(Me.hdnListMapVariant.Value) Then
            sheetName = sheetName & "O"
        End If
        'reportMapId = ""
        ''UPLOAD_XLSデータ取得
        COA0029XlsTable.MAPID = reportMapId
        COA0029XlsTable.SHEETNAME = sheetName
        COA0029XlsTable.COA0029XlsToTable()
        'COA0029XlsTable.TBLDATA = dt
        If COA0029XlsTable.ERR = C_MESSAGENO.NORMAL Then
            If COA0029XlsTable.TBLDATA.Rows.Count = 0 Then
                CommonFunctions.ShowMessage(C_MESSAGENO.NODATA, Me.lblFooterMessage, pageObject:=Me)
                Return
            End If
        Else
            '取得したExcelデータのレポートIDが現在の表示機能と一致しているか確認
            If Not Me.lbRightList.SelectedItem.Value.Equals(COA0029XlsTable.REPORTID) Then
                'TODOエラーメッセージ＋Return
            End If
            returnCode = COA0029XlsTable.ERR
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If



        Dim excelRetDr As DataRow = COA0029XlsTable.TBLDATA.Rows(0)
        Dim excelDt As DataTable = COA0029XlsTable.TBLDATA
        Dim errMsg As String = ""
        returnCode = UpdateDataTableFromExcelFile(excelDt, errMsg)
        Dim naeiw As String = C_NAEIW.ABNORMAL
        If returnCode = C_MESSAGENO.NORMAL Then
            naeiw = C_NAEIW.NORMAL
        End If
        CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=naeiw, pageObject:=Me)
    End Sub
    ''' <summary>
    ''' アップロードされたExcelデータテーブルをもとに内部データテーブルを更新する
    ''' </summary>
    ''' <param name="uploadedExcelDt">Excelで取得したデータテーブ</param>
    ''' <param name="errMsg">[OUT]左ボックス用メッセージ</param>
    ''' <returns>メッセージNo</returns>
    ''' <remarks>SYSnnnnnにマッチした当該項目を更新(日付の場合は費用項目連動も考慮（ただし同一SYSnnnnの場合はあと勝ち）</remarks>
    Private Function UpdateDataTableFromExcelFile(uploadedExcelDt As DataTable, ByRef errMsg As String) As String
        'この段階でレコード0件の場合は正常終了扱い
        If uploadedExcelDt IsNot Nothing AndAlso uploadedExcelDt.Rows.Count = 0 Then
            Return C_MESSAGENO.NORMAL
        End If
        '一覧表示データ復元 
        Dim COA0021ListTable As New COA0021ListTable
        Dim writeDt As DataTable = Nothing
        Dim noeditWriteDt As DataTable = Nothing
        If Me.SavedDt Is Nothing Then
            writeDt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = writeDt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                writeDt = COA0021ListTable.OUTTBL
            Else
                Return COA0021ListTable.ERR
            End If
        Else
            writeDt = Me.SavedDt
        End If
        noeditWriteDt = writeDt.Clone
        If writeDt IsNot Nothing AndAlso writeDt.Rows.Count > 0 Then
            For Each writeitem As DataRow In writeDt.Rows
                Dim nRow As DataRow = noeditWriteDt.NewRow
                nRow.ItemArray = writeitem.ItemArray
                noeditWriteDt.Rows.Add(nRow)
            Next

        End If
        'ACTYが空のデータを上に持っていき処理を行う
        Dim uploadedExcelDtSorted = ""
        Dim writeFieldNameList As New List(Of String) '更新対象フィールド一覧
        '遷移元のMAPVARIANTにより取り込むフィールドを選ぶ(タンクNoもあと勝ち)
        Select Case Me.hdnListMapVariant.Value
            Case "GB_TankActivity" 'タンク動静
                writeFieldNameList.AddRange({"CONTRACTORFIX", "ACTUALDATE"}) 'タンクNo、予定金額、実績業者、実績日
            Case "GB_SOA" 'SOA
                writeFieldNameList.AddRange({"JOT", "SOACHECK", "TAXATION", "ACTUALDATE"}) '実績金額のみ
            Case "GB_Demurrage" 'デマレッジ
                writeFieldNameList.AddRange({"AMOUNTORD", "DEMREPORTMONTH"})
            Case "GB_NonBreaker" 'ノンブレーカー
                writeFieldNameList.AddRange({"TANKNO", "CURRENCYCODE", "AMOUNTORD", "CONTRACTORFIX", "ACTUALDATE", "INVOICEDBY"})
            Case "GB_CostUp"
                writeFieldNameList.AddRange({"CURRENCYCODE", "AMOUNTORD", "CONTRACTORODR"}) '予定金額、予定業者、予定日
            Case Else 'オーダー一覧
                writeFieldNameList.AddRange({"CURRENCYCODE", "AMOUNTORD", "CONTRACTORODR", "SCHEDELDATE"}) '予定金額、予定業者、予定日
        End Select
        Dim sortedUploadExcelDt As DataTable = (From uploadedExcelDr In uploadedExcelDt).CopyToDataTable
        For Each dr As DataRow In sortedUploadExcelDt.Rows
            Dim sysKey As String = Convert.ToString(dr.Item("DATAID"))
            Dim isNewCost As Boolean = False
            If sysKey = "" Then
                If Not {"GB_NonBreaker", "Default", "GB_CostUp"}.Contains(Me.hdnListMapVariant.Value) Then
                    Continue For
                Else
                    isNewCost = True
                    'ノンブレーカーの費目追加
                    If Me.hdnListMapVariant.Value = "GB_NonBreaker" AndAlso
                        sortedUploadExcelDt.Columns.Contains("COSTCODE") Then
                        '追加したがコストコード未入力の場合はスキップ
                        Dim costCode As String = Convert.ToString(dr.Item("COSTCODE")).Trim
                        If costCode = "" Then
                            Continue For
                        End If
                        If AddNewNbCostItem(costCode, True, sysKey) <> C_MESSAGENO.NORMAL Then
                            Continue For
                        Else
                            writeDt = Me.SavedDt
                        End If
                    ElseIf sortedUploadExcelDt.Columns.Contains("COSTCODE") Then
                        '追加したがコストコード未入力の場合はスキップ
                        Dim costCode As String = Convert.ToString(dr.Item("COSTCODE")).Trim
                        If costCode = "" Then
                            Continue For
                        End If
                        Dim lastSydkey As String = ""
                        If AddNewCostItem(costCode, True, dr, lastSydkey) <> C_MESSAGENO.NORMAL Then
                            Continue For
                        Else
                            sysKey = lastSydkey 'Convert.ToString(dr.Item("SYSKEY"))
                            writeDt = Me.SavedDt
                        End If
                    End If 'ノンブレーカーの費目追加

                End If '費目追加判定
            End If

            'Excelのシステム採番とローカル保存採番をマッチングさせ書き込む行を特定
            Dim writeDr As DataRow = (From wdr In writeDt
                                      Where wdr.Item("DATAID").Equals(sysKey)).FirstOrDefault
            '入力不可状態のデータをはじく場合、このタイミングでCANEDIT判定しスキップをいれる

            If isNewCost Then
                writeDr = (From wdr In writeDt
                           Where wdr.Item("SYSKEY").Equals(sysKey)).FirstOrDefault
            End If
            '書き込み先が存在しない場合はつぎへスキップ
            If writeDr Is Nothing Then
                Continue For
            End If
            '特殊スキップ
            'SOA時のデマレッジ費目は更新させないためスキップ
            If Me.hdnListMapVariant.Value = "GB_SOA" AndAlso writeDr.Item("COSTCODE").Equals(GBC_COSTCODE_DEMURRAGE) Then
                Continue For
            End If
            '編集不可行の場合はスキップ
            If writeDr.Item("CANROWEDIT").Equals("0") Then
                Continue For
            End If
            '値展開フィールドに記載
            For Each fieldName As String In writeFieldNameList
                'そもそもフィールドがない場合はスキップ
                If Not sortedUploadExcelDt.Columns.Contains(fieldName) Then
                    Continue For
                End If
                '値に変化がなければスキップ（付帯処理は行わない）
                If writeDr.Item(fieldName).Equals(dr.Item(fieldName)) Then
                    Continue For
                End If

                '日付項目かつ入力したデータが日付型の場合
                If {"SCHEDELDATE", "ACTUALDATE", "BLAPPDATE", "SOAAPPDATE"}.Contains(fieldName) Then
                    Dim dateString As String = Convert.ToString(dr.Item(fieldName))
                    dateString = dateString.Trim
                    dateString = FormatDateYMD(dateString, GBA00003UserSetting.DATEFORMAT)
                    Dim dateBuff As Date
                    '日付項目が空白または日付に変換できない場合は次のフィールドにスキップ
                    If dateString = "" OrElse Date.TryParse(dateString, dateBuff) = False Then
                        Continue For
                    End If
                    If dateString <> "" Then
                        dateString = dateBuff.ToString("yyyy/MM/dd")
                    End If
                    ' 日付のクリア
                    If dateString = "1900/01/01" Then
                        dateString = ""
                    End If
                    '日付項目一括転送を行う
                    Dim actyNo As String = Convert.ToString(writeDr.Item("ACTIONID"))
                    'If actyNo <> "" Then
                    If actyNo <> "" AndAlso dateString <> "" Then
                        '日付入力したACTYをもとに他の日付を連鎖して更新
                        Dim rowNum As String = Convert.ToString(writeDr.Item("LINECNT"))
                        Dim txtBoxName As String = String.Format("txt{0}{1}Dummy", Me.WF_LISTAREA.ID, fieldName)
                        UpdateDatatableDate(dateString, txtBoxName, rowNum, writeDt)
                    End If
                    'Excelの日付項目を転送

                    If isNewCost = True Then
                        writeDr.Item(fieldName) = dateString
                    Else
                        Dim noEditRowItem = (From item In noeditWriteDt Where item("LINECNT").Equals(writeDr("LINECNT"))).FirstOrDefault

                        If noEditRowItem Is Nothing OrElse (noEditRowItem IsNot Nothing AndAlso writeDr.Item(fieldName).Equals(noEditRowItem.Item(fieldName))) Then
                            writeDr.Item(fieldName) = dateString
                        End If
                    End If

                    Continue For '日付処理は終了のため後続処理へ
                End If '日付項目処理
                ''タンク引当(TANKNOが異なるかつ空白以外の場合はタンク引当対象)
                'If fieldName = "TANKNO" AndAlso Convert.ToString(dr.Item(fieldName)) <> "" Then

                '    Dim tankNo As String = Convert.ToString(dr.Item(fieldName))
                '    Dim orderNo As String = Convert.ToString(writeDr.Item("ORDERNO"))
                '    Dim tankSeq As String = Convert.ToString(writeDr.Item("TANKSEQ"))
                '    If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
                '        writeDr.Item(fieldName) = dr.Item(fieldName)
                '        Continue For
                '    End If
                '    'ノンブレを除き引き当てさせない
                '    ''引当可能チェック
                '    'If IsFreeTank(tankNo) = "1" Then
                '    '    Dim modTank = From tmpDr In writeDt
                '    '                  Where Convert.ToString(tmpDr.Item("ORDERNO")) = orderNo _
                '    '                  AndAlso Convert.ToString(tmpDr.Item("TANKSEQ")) = tankSeq
                '    '    For Each item In modTank
                '    '        item("TANKNO") = tankNo

                '    '    Next
                '    'End If
                '    'Continue For
                'ElseIf fieldName = "TANKNO" Then
                '    Continue For
                'End If
                '業者連動更新
                If {"CONTRACTORODR", "CONTRACTORFIX"}.Contains(fieldName) Then
                    '未入力の場合は一旦スキップ
                    If Convert.ToString(dr.Item(fieldName)) = "" Then
                        Continue For
                    End If

                    Dim contractor As String = Convert.ToString(dr.Item(fieldName))
                    Dim orderNo As String = Convert.ToString(writeDr.Item("ORDERNO"))
                    Dim tankSeq As String = Convert.ToString(writeDr.Item("TANKSEQ"))

                    Dim rowNum As String = Convert.ToString(writeDr.Item("LINECNT"))
                    Dim txtBoxName As String = String.Format("txt{0}{1}Dummy", Me.WF_LISTAREA.ID, fieldName)
                    '業者連動更新実行(ノンブレーカー以外は実行)
                    UpdateDatatableContractor(contractor, txtBoxName, rowNum, writeDt)
                    'Excelの業者項目を転送
                    writeDr.Item(fieldName) = contractor
                    Continue For
                End If
                '通貨コード(オーダー追加またはノンブレーカー以外での変更は不可)
                If {"GB_NonBreaker", "Default", "GB_CostUp"}.Contains(Me.hdnListMapVariant.Value) _
                    AndAlso fieldName = "CURRENCYCODE" _
                    AndAlso writeDt.Columns.Contains("BRCOST") _
                    AndAlso Not writeDr.Item("BRCOST").Equals("1") Then
                    writeDr.Item(fieldName) = dr.Item(fieldName)
                    UpdateDatatableCurrency(Convert.ToString(dr.Item(fieldName)), "0", True, writeDr)
                    Continue For
                ElseIf fieldName = "CURRENCYCODE" Then
                    Continue For
                End If

                'Excelに入力した値をコピー
                writeDr.Item(fieldName) = dr.Item(fieldName)
            Next 'フィールド名ループ END

        Next 'Excel取得のデータテーブルループEND 
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = writeDt
        COA0021ListTable.COA0021saveListTable()
        Me.SavedDt = writeDt
        If COA0021ListTable.ERR <> C_MESSAGENO.NORMAL Then
            Return COA0021ListTable.ERR
        End If
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' タンクの引き当て可否を確認
    ''' </summary>
    ''' <param name="tankNo"></param>
    ''' <returns>1:引当可、0:引当不可、9:未存在タンク</returns>
    Private Function IsFreeTank(tankNo As String, Optional sqlCon As SqlConnection = Nothing) As String
        '引きはがしの場合はチェックの必要なし
        If tankNo = "" Then
            Return "1"
        End If
        '現状なんでもOK（タンクマスタの登録がないため）
        Return "1"
    End Function

    ''' <summary>
    ''' 単項目チェック処理
    ''' </summary>
    ''' <param name="mapId">IN:チェック条件のMAPID</param>
    ''' <param name="dt">IN:チェック対象のデータテーブル</param>
    ''' <param name="checkFileds">IN:チェックフィールド一覧</param>
    ''' <param name="errMessage">OUT：エラーメッセージ</param>
    ''' <param name="keyFields">IN(省略可):エラーメッセージ表示時に示すキーフィールドリスト、ここを指定した場合は「エラー内容」＋「当引数のフィールドと値」をメッセージに付与します
    ''' 省略時は付与しません</param>
    ''' <param name="keyValuePadLen">IN(省略可 省略時20):「--> [項目名] = [値]」を表示する際の項目名から=までにスペースを埋めるバイト数</param>
    ''' <returns>メッセージ番号:すべて正常時はC_MESSAGENO.NORMAL(00000) チェック異常時はC_MESSAGENO.RIGHTBIXOUT(10008)を返却</returns>
    Private Function CheckSingle(ByVal mapId As String, ByVal dt As DataTable, ByVal checkFileds As List(Of String), ByRef errMessage As String, Optional keyFields As List(Of String) = Nothing, Optional keyValuePadLen As Integer = 20) As String
        Dim checkMapId As String = mapId & Me.hdnListMapVariant.Value
        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim hasError As Boolean = False
        Dim dummyLabelObj As New Label '画面描画しないダミーのラベルオブジェクト
        Dim retMessage As New StringBuilder
        'エラーメッセージ取得すら失敗した場合
        Dim getMessageErrorString As String = "エラーメッセージ({0})の取得に失敗しました。"
        If BASEDLL.COA0019Session.LANGDISP <> C_LANG.JA Then
            getMessageErrorString = "Failed To Get Error message ({0})."
        End If
        '******************************
        '引数チェック
        '******************************
        '検査対象のデータテーブルレコードが存在しない、チェックフィールドが存在しない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 OrElse checkFileds.Count = 0 Then
            Return retMessageNo
        End If
        'DataTableに本当に存在するフィールを取得
        Dim targetCheckFields As New List(Of String)
        For Each checkField As String In checkFileds
            If dt.Columns.Contains(checkField) Then
                targetCheckFields.Add(checkField)
            End If
        Next
        '検査すべきフィールドがない場合はそのまま終了
        If targetCheckFields.Count = 0 Then
            Return retMessageNo
        End If
        'DataTableに本当に存在するキーフィールドを取得
        Dim targetKeyFields As List(Of String) = Nothing
        If keyFields IsNot Nothing Then
            targetKeyFields = New List(Of String)
            For Each keyField As String In keyFields
                If dt.Columns.Contains(keyField) Then
                    targetKeyFields.Add(keyField)
                End If
            Next
            If targetKeyFields.Count = 0 Then
                targetKeyFields = Nothing
            End If
        End If

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck              '項目チェック

        'チェックごとに変わらないパラメータ設定
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = checkMapId

        '******************************
        'フィールド名ディクショナリ取得
        '******************************
        Dim fieldDic As New Dictionary(Of String, String)
        COA0026FieldCheck.FIELDDIC = fieldDic
        COA0026FieldCheck.COA0026getFieldList()
        fieldDic = COA0026FieldCheck.FIELDDIC
        '******************************
        '単項目チェック開始
        '******************************

        'データテーブルの行ループ開始
        For Each dr As DataRow In dt.Rows
            If Me.hdnListMapVariant.Value = "GB_TankActivity" AndAlso Convert.ToString(dr.Item("ACTIONID")) = "" Then
                Continue For
            End If

            'チェックフィールドのループ開始
            For Each checkField In targetCheckFields
                COA0026FieldCheck.FIELD = checkField
                COA0026FieldCheck.VALUE = Convert.ToString(dr.Item(checkField))
                COA0026FieldCheck.COA0026FieldCheck()
                If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then
                    retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                    CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, dummyLabelObj)
                    retMessage.AppendFormat("・{0}：{1}", fieldDic(checkField), dummyLabelObj.Text).AppendLine()

                    If targetKeyFields IsNot Nothing Then
                        For Each keyField In targetKeyFields
                            retMessage.AppendFormat("--> {0} = {1}", padRight(fieldDic(keyField), keyValuePadLen), Convert.ToString(dr.Item(keyField))).AppendLine()
                        Next
                    End If 'END targetKeyFields IsNot Nothing 

                End If 'END  COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL
            Next
            'SOA締め日チェック
            If Me.hdnListMapVariant.Value = "GB_SOA" Then
                Try
                    Dim soaDate As Date = CDate(dr.Item("SOAAPPDATE"))
                    Dim billingYmd As Date = CDate(dr.Item("BILLINGYMD"))
                    If soaDate < billingYmd Then
                        retMessageNo = C_MESSAGENO.RIGHTBIXOUT
                        retMessage.AppendFormat("・{0}：{1}", "SOA DATE", "（仮）締め日より過去です！").AppendLine()
                        retMessage.AppendFormat("--> {0} = {1}", "NO", dr.Item("LINECNT")).AppendLine()
                    End If
                Catch ex As Exception
                End Try

            End If
            Dim stringKeyInfo As String = ""


        Next 'END For Each dr As DataRow In dt.Rows
        errMessage = retMessage.ToString
        Return retMessageNo
    End Function
    ''' <summary>
    ''' Decimal文字列を数字に変換
    ''' </summary>
    ''' <param name="decString"></param>
    ''' <returns></returns>
    Private Function DecimalStringToDecimal(decString As String) As Decimal
        Dim tmpDec As Decimal = 0
        If Decimal.TryParse(decString, tmpDec) Then
            Return tmpDec
        Else
            Return 0
        End If
    End Function
    ''' <summary>
    ''' 日付を変換
    ''' </summary>
    ''' <param name="dateString"></param>
    ''' <returns>変換できない場合はMinValue</returns>
    Private Function DateStringToDateTime(dateString As String) As DateTime
        Dim dateTimeDefault As DateTime = DateTime.Parse("1900/01/01 00:00:00")
        Dim tmpDateTime As DateTime
        If DateTime.TryParse(dateString, tmpDateTime) Then
            Return tmpDateTime
        Else
            Return dateTimeDefault
        End If
    End Function
    ''' <summary>
    ''' 文字左スペース埋め
    ''' </summary>
    ''' <param name="st"></param>
    ''' <param name="len"></param>
    ''' <returns></returns>
    ''' <remarks>エラー一覧で項目名称が日本語英語まちまちなので調整</remarks>
    Function padRight(ByVal st As String, ByVal len As Integer) As String
        Dim padLength As Integer = len - (System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(st) - st.Length)
        '埋められない場合はそのまま返却
        If padLength <= 0 Then
            Return st
        End If
        Return st.PadRight(len, " "c)
    End Function
    ''' <summary>
    ''' FixValueより検索値を取得
    ''' </summary>
    ''' <param name="classVal"></param>
    ''' <param name="keyCode"></param>
    ''' <returns></returns>
    Private Function GetFixValue(classVal As String, keyCode As String) As String
        Dim valueField As String = "VALUE1"
        Dim dtDbResult As New DataTable
        Dim retValue As String = ""
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            valueField = "VALUE2"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendFormat("SELECT rtrim({0}) AS VAL", valueField).AppendLine()
        sqlStat.AppendLine("  FROM COS0017_FIXVALUE")
        sqlStat.AppendLine(" WHERE COMPCODE = @COMPCODE")
        sqlStat.AppendLine("   AND CLASS    = @CLASS")
        sqlStat.AppendLine("   AND KEYCODE  = @KEYCODE")
        sqlStat.AppendLine("   AND STYMD   <= @STYMD")
        sqlStat.AppendLine("   AND ENDYMD  >= @ENDYMD")
        sqlStat.AppendLine("   AND DELFLG  <> @DELFLG")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            With sqlCmd.Parameters
                .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = GBC_COMPCODE_D 'HttpContext.Current.Session("APSRVCamp")
                .Add("@CLASS", SqlDbType.NVarChar).Value = classVal
                .Add("@KEYCODE", SqlDbType.NVarChar).Value = keyCode
                .Add("@STYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@ENDYMD", System.Data.SqlDbType.Date).Value = Date.Now
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        If dtDbResult IsNot Nothing AndAlso dtDbResult.Rows.Count <> 0 Then
            Dim dr As DataRow = dtDbResult.Rows(0)
            retValue = Convert.ToString(dr.Item("VAL"))
        End If
        Return retValue
    End Function
    ''' <summary>
    ''' 画面表示のテーブルを制御する
    ''' </summary>
    Private Sub DisplayListObjEdit()
        Dim targetPanel As Panel = Me.WF_LISTAREA
        Dim dicDisplayRows As New Dictionary(Of Integer, DataRow)
        Dim dispLineCnt As New List(Of Integer)
        If ViewState("DISPLAY_LINECNT_LIST") IsNot Nothing Then
            dispLineCnt = DirectCast(ViewState("DISPLAY_LINECNT_LIST"), List(Of Integer))
            dicDisplayRows = (From itemRow In Me.SavedDt Where dispLineCnt.Contains(CInt(itemRow("LINECNT"))) Select New KeyValuePair(Of Integer, DataRow)(CInt(itemRow("LINECNT")), itemRow)).ToDictionary(Function(x) x.Key, Function(x) x.Value)
        End If

        Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
        If rightDataDiv.HasControls = False _
           OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
           OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
            Return
        End If
        Dim rightHeaderDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_HR"), Panel)
        Dim rightHeaderTable As Table = DirectCast(rightHeaderDiv.Controls(0), Table)
        Dim dicColumnNameToNo As New Dictionary(Of String, String) From {{"CHARGE_CLASS1", ""}, {"APPLY", ""},
                                                                         {"APPLYTEXT", ""}, {"CURRENCYCODE", ""}, {"DTLOFFICE", ""}, {"ORGOFFICE", ""},
                                                                         {"AMOUNTBR", ""}, {"CONTRACTORBR", ""}, {"SCHEDELDATEBR", ""},
                                                                         {"AMOUNTORD", ""}, {"AMOUNTFIX", ""}, {"CONTRACTORFIX", ""}, {"ACTUALDATE", ""},
                                                                         {"AMOUNTPAY", ""}, {"SOAAPPDATE", ""}, {"COSTCODE", ""},
                                                                         {"LOCALPAY", ""},
                                                                         {"TANKNO", ""}, {"DATAID", ""},
                                                                         {"ISBILLINGCLOSED", ""}, {"ISAUTOCLOSE", ""}, {"ISAUTOCLOSELONG", ""},
                                                                         {"TAXATION", ""},
                                                                         {"COUNTRYCODE", ""}, {"STATUSCODE", ""},
                                                                         {"DEMREPORTMONTH", ""}, {"CLOSINGMONTH", ""}, {"REPORTYMDORG", ""},
                                                                         {"SOACHECK", ""}, {"CONTRACTORODR", ""}, {"SCHEDELDATE", ""},
                                                                         {"DISPLAYCURRANCYCODE", ""},
                                                                         {"UAG_USD", ""}, {"CAN_ENTRY_ACTUALDATE", ""},
                                                                         {"ACCCURRENCYSEGMENT", ""}, {"INVOICEDBY", ""},
                                                                         {"JOT", ""}}
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
        Dim dicLeftColumnNameToNo As New Dictionary(Of String, String) From {{"ACTION", ""},
                                                                             {"ORDERNO", ""},
                                                                             {"TANKSEQ", ""},
                                                                             {"TANKNO", ""}, {"APPLY", ""}, {"APPLYTEXT", ""},
                                                                             {"ACCCURRENCYSEGMENT", ""}, {"SOACHECK", ""}, {"JOT", ""},
                                                                             {"TAXATION", ""}}

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
        Dim dtNonBrCosts As New DataTable
        If Me.hdnListMapVariant.Value = "GB_NonBreaker" Then
            dtNonBrCosts = GetCostItem(C_BRTYPE.NONBR)
        End If
        '******************************
        'レンダリング行のループ
        '******************************
        Dim disableRow As Boolean = False
        Dim rowCnt As Integer = rightDataTable.Rows.Count - 1
        Dim displayRow As DataRow = Nothing
        For i = 0 To rowCnt
            disableRow = False
            Dim tbrRight As TableRow = rightDataTable.Rows(i)

            Dim tbrLeft As TableRow = leftDataTable.Rows(i)
            Dim hideDelete As String = tbrLeft.Cells(2).Text '1削除負荷、それ以外は削除可能
            Dim lineCnt As String = tbrLeft.Cells(0).Text
            displayRow = Nothing
            If dicDisplayRows.ContainsKey(CInt(lineCnt)) Then
                displayRow = dicDisplayRows(CInt(lineCnt))
            End If
            'disableRowの使用不可フラグを立てる
            If displayRow IsNot Nothing AndAlso displayRow.Item("CANROWEDIT").Equals("0") Then
                disableRow = True
            End If

            'ノンブレーカー区分ボックスの表示制御
            If Me.hdnListMapVariant.Value = "GB_NonBreaker" AndAlso
               (From nonBritem In dtNonBrCosts Where Convert.ToString(nonBritem("CODE")) = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("COSTCODE"))).Text).Any AndAlso
               (dicColumnNameToNo("ACCCURRENCYSEGMENT") <> "" OrElse dicLeftColumnNameToNo("ACCCURRENCYSEGMENT") <> "") Then
                Dim fieldName As String = "ACCCURRENCYSEGMENT"
                Dim canEntry As Boolean = False
                If GBA00003UserSetting.IS_JOTUSER AndAlso (From nonBritem In dtNonBrCosts Where Convert.ToString(nonBritem("CODE")) = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("COSTCODE"))).Text AndAlso Convert.ToString(nonBritem("ENABLEACCCURRENCYSEGMENT")).Equals("1")).Any Then
                    canEntry = True
                End If
                '対象の
                Dim targetCell As TableCell = Nothing
                If dicColumnNameToNo(fieldName) <> "" Then
                    targetCell = tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))

                End If
                If dicLeftColumnNameToNo(fieldName) <> "" Then
                    targetCell = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo(fieldName)))
                End If

                If targetCell IsNot Nothing Then
                    If disableRow = False AndAlso canEntry = False AndAlso Not targetCell.Text.Contains("readonly=") Then
                        targetCell.Text = targetCell.Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                        targetCell.Style.Add("pointer-events", "none")
                    End If

                End If
            End If
            ''ノンブレーカーの申請ボックス非表示
            'If Me.hdnListMapVariant.Value = "GB_NonBreaker" AndAlso
            '   (From nonBritem In dtNonBrCosts Where Convert.ToString(nonBritem("CLASS2")) = "" AndAlso Convert.ToString(nonBritem("CODE")) = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("COSTCODE"))).Text).Any AndAlso
            '   (dicColumnNameToNo("APPLY") <> "" OrElse dicLeftColumnNameToNo("APPLY") <> "") AndAlso
            '   (dicColumnNameToNo("APPLYTEXT") <> "" OrElse dicLeftColumnNameToNo("APPLYTEXT") <> "") Then
            '    Dim chargeClass1 As String = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("CHARGE_CLASS1"))).Text
            '    '申請対象費用ではない場合申請のチェック、テキストを消す
            '    For Each fieldName As String In {"APPLY", "APPLYTEXT"}
            '        If dicColumnNameToNo(fieldName) <> "" Then
            '            With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
            '                .Controls.Clear()
            '                .Text = ""
            '            End With
            '        End If
            '        If dicLeftColumnNameToNo(fieldName) <> "" Then
            '            With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo(fieldName)))
            '                .Controls.Clear()
            '                .Text = ""
            '            End With
            '        End If
            '    Next

            'End If

            '通貨コードテキストの制御（削除可能なオーダーで後付けした費用については変更可能とする）
            If dicColumnNameToNo("CURRENCYCODE") <> "" Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("CURRENCYCODE")))
                    If disableRow = False AndAlso hideDelete = "1" AndAlso .Text.StartsWith("<input id=""txtWF_LISTAREACURRENCYCODE") Then
                        .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                        .Style.Add("pointer-events", "none")
                    End If
                End With
            End If

            'デマレージにてJOTのみOFFICEの変更を許可する(TODO：JOTユーザー判定)
            If Me.hdnListMapVariant.Value = "GB_Demurrage" AndAlso
               dicColumnNameToNo("DTLOFFICE") <> "" AndAlso
               dicColumnNameToNo("ORGOFFICE") <> "" Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("DTLOFFICE")))
                    If disableRow = False Then
                        .Attributes.Add("ondblclick", String.Format("swapOffice('{0}');", lineCnt))
                    End If
                    If .Text <> tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ORGOFFICE"))).Text Then
                        .CssClass = "swappedOffice"
                    End If
                End With
            End If
            'デマレージにて申請中の場合は費用の入力不可
            If disableRow = False AndAlso Me.hdnListMapVariant.Value = "GB_Demurrage" AndAlso
               dicColumnNameToNo("AMOUNTORD") <> "" AndAlso
               dicColumnNameToNo("STATUSCODE") <> "" AndAlso
               tbrRight.Cells(Integer.Parse(dicColumnNameToNo("STATUSCODE"))).Text.Trim = C_APP_STATUS.APPLYING Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("AMOUNTORD")))
                    If Not .Text.Contains("readonly=") Then
                        .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                        .Style.Add("pointer-events", "none")
                    End If
                End With
            End If
            'デマレージのテキストボックスのイベント関連付け
            If disableRow = False AndAlso Me.hdnListMapVariant.Value = "GB_Demurrage" AndAlso
               dicColumnNameToNo("DEMREPORTMONTH") <> "" AndAlso
               dicColumnNameToNo("CLOSINGMONTH") <> "" AndAlso
               dicColumnNameToNo("REPORTYMDORG") <> "" Then
                '現在の精算月(GBT0006_CLOSINGDAY)
                Dim closingMonthValue As String = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("CLOSINGMONTH"))).Text.Trim
                '修正前の精算月(入力不可の場合に元に戻すため使用)
                Dim reportMonthValue As String = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("REPORTYMDORG"))).Text.Trim

                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("DEMREPORTMONTH")))
                    .Text = .Text.Replace(">", " placeholder=""DoubleClick to select"" data-closingmonth=""" & closingMonthValue & """ data-reportmonth=""" & reportMonthValue & """ />")
                    .Text = String.Format("<span id=""{4}"" ondblclick=""leftMonthViewOpen('{1}', '{2}', '{3}', '{5}');"">{0}</span>", .Text, lineCnt, closingMonthValue, reportMonthValue, "lblWF_LISTAREADEMREPORTMONTH" & lineCnt, "txtWF_LISTAREADEMREPORTMONTH" & lineCnt)
                End With
            End If

            'SOA時にGB_Demurrageの項目を使用不可に変更
            If disableRow = False AndAlso Me.hdnListMapVariant.Value = "GB_SOA" AndAlso
               ((dicColumnNameToNo("COSTCODE") <> "" AndAlso
               tbrRight.Cells(Integer.Parse(dicColumnNameToNo("COSTCODE"))).Text = GBC_COSTCODE_DEMURRAGE) OrElse
                (dicColumnNameToNo("ISBILLINGCLOSED") <> "" AndAlso
                tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ISBILLINGCLOSED"))).Text = "1")) Then
                For Each fieldName As String In {"AMOUNTFIX", "CONTRACTORFIX", "ACTUALDATE", "AMOUNTPAY", "SOAAPPDATE", "LOCALPAY"}
                    If dicColumnNameToNo(fieldName) <> "" Then
                        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                            If Not .Text.Contains("readonly=") Then
                                .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                                .Style.Add("pointer-events", "none")
                            End If
                        End With
                    End If
                Next
                'SOAでは申請すらさせない
                For Each fieldName As String In {"APPLY", "APPLYTEXT"}
                    If dicColumnNameToNo(fieldName) <> "" Then
                        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                            .Controls.Clear()
                            .Text = ""
                        End With
                    End If
                    If dicLeftColumnNameToNo(fieldName) <> "" Then
                        With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo(fieldName)))
                            .Controls.Clear()
                            .Text = ""
                        End With
                    End If
                Next
            ElseIf disableRow = False AndAlso {"GB_TankActivity", "GB_SOA"}.Contains(Me.hdnListMapVariant.Value) AndAlso
                 ((dicColumnNameToNo("CAN_ENTRY_ACTUALDATE") <> "" AndAlso
                   tbrRight.Cells(Integer.Parse(dicColumnNameToNo("CAN_ENTRY_ACTUALDATE"))).Text <> "1")) Then
                Dim fieldName As String = "ACTUALDATE"
                If dicColumnNameToNo(fieldName) <> "" Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                        If Not .Text.Contains("readonly=") Then
                            .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" data-everydisable=""1"" />")
                            .Style.Add("pointer-events", "none")
                        End If
                    End With
                End If
            End If
            'SOA時にGB_Demurrageの項目を使用不可に変更
            If Me.hdnListMapVariant.Value = "GB_SOA" AndAlso
               (dicColumnNameToNo("ISAUTOCLOSE") <> "" OrElse dicColumnNameToNo("ISAUTOCLOSELONG") <> "") Then
                If dicColumnNameToNo("ISAUTOCLOSE") <> "" Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ISAUTOCLOSE")))
                        If .Text = "-1" Then
                            tbrRight.Attributes.Add("data-isautoclose", "-1")
                            tbrLeft.Attributes.Add("data-isautoclose", "-1")
                        End If

                        If .Text = "1" Then
                            tbrRight.Attributes.Add("data-isautoclose", "1")
                            tbrLeft.Attributes.Add("data-isautoclose", "1")
                        End If
                    End With
                End If
                If dicColumnNameToNo("ISAUTOCLOSELONG") <> "" Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("ISAUTOCLOSELONG")))
                        If .Text = "1" Then
                            tbrRight.Attributes.Add("data-isautoclose", "2")
                            tbrLeft.Attributes.Add("data-isautoclose", "2")
                        End If
                    End With
                End If

            End If

            If Me.hdnListMapVariant.Value = "GB_SOA" AndAlso
               dicColumnNameToNo("DISPLAYCURRANCYCODE") <> "" AndAlso
               dicColumnNameToNo("AMOUNTFIX") <> "" Then
                Dim isSetInvisible As Boolean = False
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("DISPLAYCURRANCYCODE")))
                    If .Text.Length >= 4 Then
                        isSetInvisible = True
                    End If
                End With
                If isSetInvisible = True Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("AMOUNTFIX")))
                        .Attributes.Add("data-hidecell", "1")
                    End With
                End If
            End If
            'ブレーカー項目入力項目を使用不可に
            For Each fieldName As String In {"AMOUNTBR", "CONTRACTORBR", "SCHEDELDATEBR"}
                If disableRow = False AndAlso dicColumnNameToNo(fieldName) <> "" Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                        If .Text.StartsWith(String.Format("<input id=""txtWF_LISTAREA{0}", fieldName)) Then
                            .Text = .Text.Replace(">", " disabled=""disabled"" class=""aspNetDisabled"" />")
                        End If
                    End With
                End If
            Next

            'タンク関連処理
            'ノンブレ・SOA以外の場合は引当・引きはがしのタグを挿入する
            If (dicColumnNameToNo("TANKNO") <> "" OrElse dicLeftColumnNameToNo("TANKNO") <> "") AndAlso
               dicLeftColumnNameToNo("ORDERNO") <> "" AndAlso
               dicLeftColumnNameToNo("TANKSEQ") <> "" AndAlso
               Not {"GB_NonBreaker", "GB_SOA", "GB_CostUp", "GB_TankActivity", "GB_PRINT"}.Contains(Me.hdnListMapVariant.Value) Then
                Dim orderNo As String = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ORDERNO"))).Text
                Dim tankSeq As String = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKSEQ"))).Text

                Dim dataId As String = ""
                If dicColumnNameToNo("DATAID") <> "" Then
                    dataId = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("DATAID"))).Text
                End If
                If dicColumnNameToNo("TANKNO") <> "" Then
                    '右にTANKNOがある場合
                    Dim tankNo As String = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("TANKNO"))).Text

                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("TANKNO")))
                        Dim canDelete As Boolean = False
                        If tankNo = "" Then
                            .CssClass = "noTank"
                        Else
                            .CssClass = "hasTank"
                            canDelete = True
                        End If

                        .Text = String.Format("<span ondblclick=""browseTankList('{1}', '{2}', '{3}');"">{0}</span>", tankNo, orderNo, tankSeq, dataId)
                        If canDelete = True Then
                            .Text = .Text & String.Format("<span class=""deleteTank"" onclick=""deleteTankNo('{0}', '{1}', '{2}');""></span>", orderNo, tankSeq, dataId)
                        End If
                    End With
                ElseIf dicLeftColumnNameToNo("TANKNO") <> "" Then
                    '左にTANKNOがある場合
                    Dim tankNo As String = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO"))).Text

                    With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("TANKNO")))
                        Dim canDelete As Boolean = False
                        If tankNo = "" Then
                            .CssClass = "noTank"
                        Else
                            .CssClass = "hasTank"
                            canDelete = True
                        End If

                        .Text = String.Format("<span ondblclick=""browseTankList('{1}', '{2}', '{3}');"">{0}</span>", tankNo, orderNo, tankSeq, dataId)
                        If canDelete = True Then
                            .Text = .Text & String.Format("<span class=""deleteTank"" onclick=""deleteTankNo('{0}', '{1}', '{2}');""></span>", orderNo, tankSeq, dataId)
                        End If
                    End With
                End If
            End If
            '課税フラグの表示制御
            If dicColumnNameToNo("TAXATION") <> "" AndAlso dicColumnNameToNo("COUNTRYCODE") <> "" Then
                Dim rowCountryCode As String = tbrRight.Cells(Integer.Parse(dicColumnNameToNo("COUNTRYCODE"))).Text
                'If rowCountryCode <> "JP" Then
                If rowCountryCode <> "JP" AndAlso GBA00003UserSetting.IS_JOTUSER <> True Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("TAXATION")))
                        If .HasControls = True AndAlso TypeOf .Controls(0) Is CheckBox Then
                            Dim chkObj As CheckBox = DirectCast(.Controls(0), CheckBox)
                            chkObj.Style.Add("display", "none")
                        End If

                    End With
                End If
            End If
            '削除ボタンの表示非表示制御
            If dicLeftColumnNameToNo("ACTION") <> "" Then
                With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ACTION")))
                    If .HasControls = True AndAlso TypeOf .Controls(0) Is HtmlButton AndAlso
                        hideDelete = "1" Then

                        .Controls.RemoveAt(0)
                    ElseIf .HasControls = True AndAlso TypeOf .Controls(0) Is HtmlButton Then
                        Dim htmlbutton As HtmlButton = DirectCast(.Controls(0), HtmlButton)
                        Dim htmlInputButton As New HtmlInputButton
                        If htmlbutton.Attributes.Count > 0 Then
                            For Each attrKey As String In htmlbutton.Attributes.Keys
                                htmlInputButton.Attributes.Add(attrKey, htmlbutton.Attributes(attrKey))
                            Next
                        End If
                        htmlInputButton.ID = htmlbutton.ID
                        htmlInputButton.Style.Add(HtmlTextWriterStyle.Display, "inline-block")
                        htmlInputButton.Value = Me.hdnListDeleteName.Value
                        If disableRow Then
                            htmlInputButton.Disabled = True
                        End If
                        .Controls.RemoveAt(0)
                        .Controls.Add(htmlInputButton)
                    End If

                End With
            End If
            '申請中のアイテムはチェックボックス非表示
            If (dicColumnNameToNo("APPLY") <> "" OrElse dicLeftColumnNameToNo("APPLY") <> "") AndAlso
               (dicColumnNameToNo("STATUSCODE") <> "" AndAlso
               tbrRight.Cells(Integer.Parse(dicColumnNameToNo("STATUSCODE"))).Text.Trim = C_APP_STATUS.APPLYING) Then

                If dicColumnNameToNo("APPLY") <> "" Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("APPLY")))
                        .Controls.Clear()
                        .Text = ""
                    End With
                End If
                If dicLeftColumnNameToNo("APPLY") <> "" Then
                    With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("APPLY")))
                        .Controls.Clear()
                        .Text = ""
                    End With
                End If

                For Each fieldName As String In {"AMOUNTORD", "AMOUNTFIX", "CONTRACTORODR", "CONTRACTORFIX", "SCHEDELDATE", "ACTUALDATE", "AMOUNTPAY", "SOAAPPDATE", "LOCALPAY"}
                    If dicColumnNameToNo(fieldName) <> "" Then
                        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                            If disableRow = False AndAlso Not .Text.Contains("readonly=") AndAlso .Text.Contains("type=""text""") Then
                                .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                                .Style.Add("pointer-events", "none")
                            End If
                        End With
                    End If
                Next
            End If
            '申請チェックにイベントを紐づけ
            If (dicColumnNameToNo("APPLY") <> "" OrElse dicLeftColumnNameToNo("APPLY") <> "") AndAlso
               (dicColumnNameToNo("APPLYTEXT") <> "" OrElse dicLeftColumnNameToNo("APPLYTEXT") <> "") Then
                Dim checkObj As CheckBox = Nothing

                If dicColumnNameToNo("APPLY") <> "" Then
                    With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("APPLY")))
                        If .HasControls = True AndAlso TypeOf .Controls(0) Is CheckBox Then
                            checkObj = DirectCast(.Controls(0), CheckBox)
                        End If
                    End With
                End If
                If dicLeftColumnNameToNo("APPLY") <> "" Then
                    With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("APPLY")))
                        If .HasControls = True AndAlso TypeOf .Controls(0) Is CheckBox Then
                            checkObj = DirectCast(.Controls(0), CheckBox)
                        End If
                    End With
                End If
                If checkObj IsNot Nothing Then
                    'Dim spanObj As New Label With {.EnableViewState = False, .ID = "spn" + checkObj.ID}
                    'spanObj.Controls.Add(checkObj)
                    'checkObj.Parent.Controls.Add()
                    checkObj.Attributes.Add("onchange", "applyChange('" & checkObj.ID & "','txtWF_LISTAREAAPPLYTEXT" & lineCnt & "');")
                    If checkObj.Checked = True Then
                        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("APPLYTEXT")))
                            .Text = .Text.Replace(">", " class='needsInput' >")
                        End With
                    End If
                End If
            End If
            Dim usdAmount As String = "0"
            If dicColumnNameToNo("UAG_USD") <> "" Then
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("UAG_USD")))
                    If .Text <> "" Then
                        usdAmount = .Text
                    End If
                End With
            End If
            If dicColumnNameToNo("SOACHECK") <> "" Then
                Dim checkObj As CheckBox = Nothing
                With tbrRight.Cells(Integer.Parse(dicColumnNameToNo("SOACHECK")))
                    If .HasControls = True AndAlso TypeOf .Controls(0) Is CheckBox Then
                        checkObj = DirectCast(.Controls(0), CheckBox)
                    End If
                End With
                If checkObj IsNot Nothing Then
                    'Dim spanObj As New Label With {.EnableViewState = False, .ID = "spn" + checkObj.ID}
                    'spanObj.Controls.Add(checkObj)
                    'checkObj.Parent.Controls.Add()
                    'checkObj.Attributes.Add("onchange", "soaChange('" & checkObj.ID & "','" & lineCnt & "');doSubmit();")
                    checkObj.Attributes.Add("onchange", "calcSummaryAmount('" & checkObj.ID & "','" & usdAmount & "'); soaChange('" & checkObj.ID & "','" & lineCnt & "');")
                    If Me.hdnListMapVariant.Value = "GB_SOA" AndAlso dicColumnNameToNo("ACTUALDATE") <> "" Then
                        Dim qfindDateNull = From dr In Me.SavedDt Where Convert.ToString(dr("LINECNT")) = lineCnt AndAlso
                                                                        (dr("ACTUALDATE").Equals("") _
                                                                         OrElse (Convert.ToString(dr("ORDERNO")).StartsWith("NB") AndAlso
                                                                                Not ({C_APP_STATUS.APPROVED, C_APP_STATUS.COMPLETE}.Contains(Convert.ToString(dr("STATUSCODE")).Trim))))
                        If qfindDateNull.Any = True Then
                            checkObj.Enabled = False
                        End If
                    End If
                    ''ノンブレかつ承認済みでないレコードはSOAチェックさせない
                    'Dim orderNo As String
                    'orderNo = ""
                    'If dicLeftColumnNameToNo("ORDERNO") <> "" Then
                    '    orderNo = tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo("ORDERNO"))).Text
                    'End If
                    'Dim statusCode As String
                    'statusCode = ""
                    'If dicColumnNameToNo("STATUSCODE") <> "" Then

                    'End If
                End If
            End If

            'disableRowがTrueの場合、行すべて編集不可
            If disableRow Then
                For Each fieldName As String In {"AMOUNTORD", "AMOUNTFIX", "CONTRACTORODR", "CONTRACTORFIX", "SCHEDELDATE", "ACTUALDATE", "AMOUNTPAY", "SOAAPPDATE", "LOCALPAY", "TANKNO", "CURRENCYCODE", "INVOICEDBY", "DEMREPORTMONTH", "ACCCURRENCYSEGMENT"}

                    If dicColumnNameToNo.ContainsKey(fieldName) AndAlso dicColumnNameToNo(fieldName) <> "" Then
                        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                            If Not .Text.Contains("readonly=") AndAlso .Text.Contains("type=""text""") Then
                                .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                                '.Style.Add("pointer-events", "none")
                                Continue For
                            End If
                        End With
                    End If

                    If dicLeftColumnNameToNo.ContainsKey(fieldName) AndAlso dicLeftColumnNameToNo(fieldName) <> "" Then
                        With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo(fieldName)))
                            If Not .Text.Contains("readonly=") AndAlso .Text.Contains("type=""text""") Then
                                .Text = .Text.Replace(">", " readonly=""readonly"" class=""aspNetDisabled"" />")
                                .Style.Add("pointer-events", "none")
                            End If
                        End With
                    End If
                Next
                'チェックボックスの使用可否
                For Each fieldName As String In {"APPLY", "JOT", "SOACHECK", "TAXATION"}
                    If dicColumnNameToNo.ContainsKey(fieldName) AndAlso dicColumnNameToNo(fieldName) <> "" Then
                        With tbrRight.Cells(Integer.Parse(dicColumnNameToNo(fieldName)))
                            If .HasControls = True AndAlso TypeOf .Controls(0) Is CheckBox Then
                                Dim chkObj As CheckBox = DirectCast(.Controls(0), CheckBox)
                                chkObj.Enabled = False
                                Continue For
                            End If
                        End With
                    End If

                    If dicLeftColumnNameToNo.ContainsKey(fieldName) AndAlso dicLeftColumnNameToNo(fieldName) <> "" Then
                        With tbrLeft.Cells(Integer.Parse(dicLeftColumnNameToNo(fieldName)))
                            If .HasControls = True AndAlso TypeOf .Controls(0) Is CheckBox Then
                                Dim chkObj As CheckBox = DirectCast(.Controls(0), CheckBox)
                                chkObj.Enabled = False
                            End If
                        End With
                    End If
                Next

            End If
        Next 'END ROWCOUNT
    End Sub
    '''' <summary>
    '''' 通貨コードの入力をコントロール
    '''' </summary>
    'Private Sub DisplayListCurrencyCodeControl()
    '    Dim targetPanel As Panel = Me.WF_LISTAREA

    '    Dim rightDataDiv As Panel = DirectCast(targetPanel.FindControl(targetPanel.ID & "_DR"), Panel)
    '    If rightDataDiv.HasControls = False _
    '       OrElse Not (TypeOf rightDataDiv.Controls(0) Is Table) _
    '       OrElse DirectCast(rightDataDiv.Controls(0), Table).Rows.Count = 0 Then
    '        Return
    '    End If
    'End Sub
    ''' <summary>
    ''' USDAMOUNT合計取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetUsdAmountSummary() As String
        Dim retValue As Decimal = 0

        If Me.hdnListMapVariant.Value <> "GB_SOA" Then
            Return Convert.ToString(retValue)
        End If
        'USD小数表示位置を取得し数字フォーマットを生成(除算・乗算がなくSummaryの為切り捨て切り上げ処理は行わない)
        Dim decPlace As Integer = 2
        If IsNumeric(Me.hdnUsdDecimalPlaces.Value) Then
            decPlace = CInt(Me.hdnUsdDecimalPlaces.Value)
        End If

        Dim formatString As String = "#,##0"
        If decPlace > 0 Then
            formatString = formatString & "." & New String("0"c, decPlace)
        End If
        If Me.SavedDt Is Nothing OrElse Me.SavedDt.Rows.Count = 0 Then
            retValue = 0
            Return retValue.ToString(formatString)
        End If
        retValue = (From item In Me.SavedDt Where Convert.ToString(item("SOACHECK")) <> "" Select If(IsNumeric(Convert.ToString(item("UAG_USD"))), Decimal.Parse(Convert.ToString(item("UAG_USD"))), 0)).Sum

        Return retValue.ToString(formatString)

    End Function
    ''' <summary>
    ''' USD桁数取得
    ''' </summary>
    Private Function GetDecimalPlaces() As String
        Dim COA0017FixValue As New COA0017FixValue

        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = "DECIMALPLACES"
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
        Else
            Throw New Exception("COA0017FixValue Get DECIMALPLACES Error")
        End If
        Return COA0017FixValue.VALUE1.Items(0).ToString

    End Function
    ''' <summary>
    ''' エージェントコード取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>仮ロジック</remarks>
    Private Function GetAgentCode() As String
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT MORGCODE AS VAL")
        sqlStat.AppendLine("  FROM COS0021_ORG ORG")
        sqlStat.AppendLine(" INNER JOIN COS0005_USER USR")
        sqlStat.AppendLine("    ON USR.USERID   = @USERID")
        sqlStat.AppendLine("   AND USR.DELFLG  <> @DELFLG")
        sqlStat.AppendLine("   AND USR.COMPCODE = ORG.COMPCODE")
        sqlStat.AppendLine("   AND USR.ORG      = ORG.ORGCODE")
        sqlStat.AppendLine(" WHERE ORG.SYSCODE  = @SYSCODE")
        sqlStat.AppendLine("   AND ORG.STYMD  <= USR.STYMD")
        sqlStat.AppendLine("   AND ORG.ENDYMD >= USR.STYMD")
        sqlStat.AppendLine("   AND ORG.DELFLG <> @DELFLG")
        Dim dtDbResult As New DataTable
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            With sqlCmd.Parameters
                .Add("@USERID", SqlDbType.NVarChar).Value = COA0019Session.USERID
                .Add("@SYSCODE", SqlDbType.NVarChar).Value = C_SYSCODE_GB
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES

            End With
            '取得結果をDataTableに転送
            Using sqlDa As New SqlDataAdapter(sqlCmd)
                sqlDa.Fill(dtDbResult)
            End Using
        End Using
        Dim retValue As String = "" '取得できない場合は一旦"021303"
        If dtDbResult IsNot Nothing AndAlso dtDbResult.Rows.Count <> 0 Then
            Dim dr As DataRow = dtDbResult.Rows(0)
            retValue = Convert.ToString(dr.Item("VAL"))
        End If
        Return retValue
    End Function
    ''' <summary>
    ''' オフィス相互変更
    ''' </summary>
    ''' <param name="lineCnt"></param>
    ''' <returns></returns>
    ''' <remarks>デマレッジ選択画面から遷移のデマレッジのオフィスを発着相互に入れ替え</remarks>
    Private Function SwapOffice(lineCnt As String) As String
        Dim currentDt As DataTable
        Dim COA0021ListTable As New COA0021ListTable
        Dim retMessage As String = C_MESSAGENO.NORMAL
        If Me.SavedDt Is Nothing Then
            currentDt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = currentDt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                currentDt = COA0021ListTable.OUTTBL
            Else
                retMessage = COA0021ListTable.ERR
                Return retMessage
            End If
        Else
            currentDt = Me.SavedDt
        End If
        Dim targetDr As DataRow = (From item In currentDt Where item("LINECNT").Equals(Integer.Parse(lineCnt))).FirstOrDefault
        If targetDr Is Nothing Then
            Return retMessage
        End If
        If targetDr.Item("DTLOFFICE").Equals(targetDr.Item("ORGOFFICE")) Then
            targetDr.Item("DTLOFFICE") = targetDr.Item("OTHEROFFICE")
        Else
            targetDr.Item("DTLOFFICE") = targetDr.Item("ORGOFFICE")
        End If
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = currentDt
        COA0021ListTable.COA0021saveListTable()

        Return retMessage

    End Function
    ''' <summary>
    ''' デマレッジ変更時の手数料参考額の表示
    ''' </summary>
    ''' <param name="lineCnt"></param>
    ''' <returns>デマレッジ入力時の確定額入力時に反応</returns>
    Private Function CalcDumCommAmount(lineCnt As String) As String
        Dim currentDt As DataTable
        Dim COA0021ListTable As New COA0021ListTable
        Dim retMessage As String = C_MESSAGENO.NORMAL
        If Me.SavedDt Is Nothing Then
            currentDt = CreateOrderListTable()
            COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
            COA0021ListTable.TBLDATA = currentDt
            COA0021ListTable.COA0021recoverListTable()
            If COA0021ListTable.ERR = C_MESSAGENO.NORMAL Then
                currentDt = COA0021ListTable.OUTTBL
            Else
                retMessage = COA0021ListTable.ERR
                Return retMessage
            End If
        Else
            currentDt = Me.SavedDt
        End If
        Dim targetDr As DataRow = (From item In currentDt Where item("LINECNT").Equals(Integer.Parse(lineCnt))).FirstOrDefault
        If targetDr Is Nothing Then
            Return retMessage
        End If
        'ACTUAL AMOUNT * 0.1
        Dim amauntFixString As String = Convert.ToString(targetDr("AMOUNTFIX"))
        Dim amauntFix As Decimal = 0
        If amauntFixString.Trim = "" OrElse Decimal.TryParse(amauntFixString, amauntFix) = False Then
            amauntFix = 0
        Else
            amauntFix = Decimal.Parse(amauntFixString)
        End If
        Dim agentComm As Decimal = amauntFix * Decimal.Parse("0.1")
        targetDr.Item("COMMAMOUNT") = agentComm
        COA0021ListTable.FILEdir = Me.hdnXMLsaveFile.Value
        COA0021ListTable.TBLDATA = currentDt
        COA0021ListTable.COA0021saveListTable()

        Return retMessage
    End Function
    ''' <summary>
    ''' デマレッジ確定時にエージェントコミッションを同発着に追加する処理
    ''' </summary>
    ''' <param name="drFixDemurrage">対象デマレッジレコード</param>
    ''' <param name="sqlCon">SQL接続</param>
    ''' <param name="sqlTran">[オプション]トランザクション(未指定時はトランザクションなし)</param>
    ''' <returns></returns>
    Private Function AddAgentCommRecord(drFixDemurrage As DataRow, sqlCon As SqlConnection, Optional sqlTran As SqlTransaction = Nothing, Optional procDate As Date = #1900/01/01#) As String

        'ありえないがそもそもレコードがない場合は何もしない
        If drFixDemurrage Is Nothing Then
            Return C_MESSAGENO.NORMAL
        End If
        If procDate.Equals(#1900/01/01#) Then
            procDate = Now
        End If
        'デマレッジ確定後の増幅定義の取得(キー：発着、リスト(0:発着、1:費用コード、2:率、3:Remarks記載文言)
        Static dicFixDemurrage As Dictionary(Of String, List(Of String)) = Nothing
        If dicFixDemurrage Is Nothing Then
            Dim COA0017FixValue As New COA0017FixValue With {
                .COMPCODE = GBC_COMPCODE_D, .CLAS = "FIXDEMURRAGE"
                }
            COA0017FixValue.COA0017getListFixValue()
            If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
                dicFixDemurrage = COA0017FixValue.VALUEDIC
            Else
                Throw New Exception("Fix value getError")
            End If
        End If
        Dim sqlStat As New StringBuilder
        Dim amountFix As Decimal = If(Convert.ToString(drFixDemurrage.Item("AMOUNTFIX")) = "", 0, Decimal.Parse(Convert.ToString(drFixDemurrage.Item("AMOUNTFIX"))))
        Dim orderNo As String = Convert.ToString(drFixDemurrage.Item("ORDERNO"))
        Dim tankSeq As String = Convert.ToString(drFixDemurrage.Item("TANKSEQ"))
        Dim dtlPolPod As String = Convert.ToString(drFixDemurrage.Item("DTLPOLPOD"))
        Dim listFixDemurrage As List(Of String) = dicFixDemurrage(dtlPolPod)
        Dim costCode As String = listFixDemurrage(1)
        Dim commRate As Decimal = If(listFixDemurrage(2) = "", 0, Decimal.Parse(listFixDemurrage(2)))
        Dim commAmount As Decimal = amountFix * commRate
        Dim commRemark As String = listFixDemurrage(3)
        '既にあるデマレッジ増幅のAgentComレコードに削除フラグを立てる
        sqlStat.AppendLine("UPDATE GBT0005_ODR_VALUE")
        sqlStat.AppendLine("   SET DELFLG     = @DELFLG")
        sqlStat.AppendLine("     , UPDYMD     = @UPDYMD")
        sqlStat.AppendLine("     , UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("     , UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("     , RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine(" WHERE ORDERNO   = @ORDERNO")
        sqlStat.AppendLine("   AND DTLPOLPOD = @DTLPOLPOD")
        sqlStat.AppendLine("   AND TANKSEQ   = @TANKSEQ")
        sqlStat.AppendLine("   AND COSTCODE  = @COSTCODE")
        sqlStat.AppendLine("   AND REMARK    = @REMARK")
        sqlStat.AppendLine("   AND DELFLG   <> @DELFLG;")
        '↓JOTの場合作らない対応前
        'If Convert.ToString(drFixDemurrage.Item("SOAAPPDATE")).Trim <> "" Then
        '↓20190924 こっちをコメントアウトすればJOTの場合AgentComは作らない(No205対応)
        If Convert.ToString(drFixDemurrage.Item("SOAAPPDATE")).Trim <> "" _
           AndAlso Convert.ToString(drFixDemurrage.Item("JOT")) <> "on" Then
            sqlStat.AppendLine("INSERT INTO GBT0005_ODR_VALUE (")
            sqlStat.AppendLine("           ORDERNO ")
            sqlStat.AppendLine("         , STYMD ")
            sqlStat.AppendLine("         , ENDYMD")
            sqlStat.AppendLine("         , TANKSEQ")
            sqlStat.AppendLine("         , DTLPOLPOD")
            sqlStat.AppendLine("         , DTLOFFICE")
            sqlStat.AppendLine("         , TANKNO")
            sqlStat.AppendLine("         , COSTCODE")
            sqlStat.AppendLine("         , ACTIONID")
            sqlStat.AppendLine("         , DISPSEQ")
            sqlStat.AppendLine("         , LASTACT")
            sqlStat.AppendLine("         , REQUIREDACT")
            sqlStat.AppendLine("         , ORIGINDESTINATION")
            sqlStat.AppendLine("         , COUNTRYCODE")
            sqlStat.AppendLine("         , CURRENCYCODE")
            sqlStat.AppendLine("         , TAXATION")
            sqlStat.AppendLine("         , AMOUNTBR")
            sqlStat.AppendLine("         , AMOUNTORD")
            sqlStat.AppendLine("         , AMOUNTFIX")
            sqlStat.AppendLine("         , CONTRACTORBR")
            sqlStat.AppendLine("         , CONTRACTORODR")
            sqlStat.AppendLine("         , CONTRACTORFIX")
            sqlStat.AppendLine("         , SCHEDELDATEBR")
            sqlStat.AppendLine("         , SCHEDELDATE")
            sqlStat.AppendLine("         , ACTUALDATE")
            sqlStat.AppendLine("         , LOCALBR")
            sqlStat.AppendLine("         , LOCALRATE")
            sqlStat.AppendLine("         , TAXBR")
            sqlStat.AppendLine("         , AMOUNTPAY")
            sqlStat.AppendLine("         , LOCALPAY")
            sqlStat.AppendLine("         , TAXPAY")
            sqlStat.AppendLine("         , INVOICEDBY")
            sqlStat.AppendLine("         , APPLYID")
            sqlStat.AppendLine("         , APPLYTEXT")
            sqlStat.AppendLine("         , LASTSTEP")
            sqlStat.AppendLine("         , SOAAPPDATE")
            sqlStat.AppendLine("         , REMARK")
            sqlStat.AppendLine("         , BRID")
            sqlStat.AppendLine("         , BRCOST")
            sqlStat.AppendLine("         , DATEFIELD")
            sqlStat.AppendLine("         , DATEINTERVAL")
            sqlStat.AppendLine("         , BRADDEDCOST")
            sqlStat.AppendLine("         , AGENTORGANIZER")
            sqlStat.AppendLine("         , DELFLG")
            sqlStat.AppendLine("         , INITYMD ")
            sqlStat.AppendLine("         , INITUSER")
            sqlStat.AppendLine("         , UPDYMD")
            sqlStat.AppendLine("         , UPDUSER")
            sqlStat.AppendLine("         , UPDTERMID")
            sqlStat.AppendLine("         , RECEIVEYMD")
            sqlStat.AppendLine("   ) ")
            sqlStat.AppendLine("SELECT TOP 1")
            sqlStat.AppendLine("           OV.ORDERNO ")
            sqlStat.AppendLine("         , OV.STYMD ")
            sqlStat.AppendLine("         , OV.ENDYMD")
            sqlStat.AppendLine("         , OV.TANKSEQ")
            sqlStat.AppendLine("         , OV.DTLPOLPOD")
            sqlStat.AppendLine("         , OV.DTLOFFICE")
            sqlStat.AppendLine("         , OV.TANKNO")
            sqlStat.AppendLine("         , @COSTCODE")
            sqlStat.AppendLine("         , ''") 'ACTIONID
            sqlStat.AppendLine("         , ''") 'DISPSREQ
            sqlStat.AppendLine("         , ''") 'LASTACT
            sqlStat.AppendLine("         , ''") 'REQUIREDACT
            sqlStat.AppendLine("         , ''") 'ORIGINDESTINATION
            sqlStat.AppendLine("         , OV.COUNTRYCODE")
            sqlStat.AppendLine("         , OV.CURRENCYCODE")
            sqlStat.AppendLine("         , @TAXATION")
            sqlStat.AppendLine("         , @COMMAMOUNT")
            sqlStat.AppendLine("         , @COMMAMOUNT")
            sqlStat.AppendLine("         , @COMMAMOUNT")
            sqlStat.AppendLine("         , ''") 'CONTRACTORBR
            sqlStat.AppendLine("         , ''") 'CONTRACTORODR
            sqlStat.AppendLine("         , CASE WHEN OV.DTLPOLPOD = 'POL1' THEN OBS.AGENTPOL1")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POL2' THEN OBS.AGENTPOL2")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POD1' THEN OBS.AGENTPOD1")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POD2' THEN OBS.AGENTPOD2")
            sqlStat.AppendLine("                ELSE '1'")
            sqlStat.AppendLine("            END") 'CONTRACTORFIX
            sqlStat.AppendLine("         , @DEMDATE") 'SCHEDELDATEBR
            sqlStat.AppendLine("         , @DEMDATE") 'SCHEDELDATE
            sqlStat.AppendLine("         , @DEMDATE") 'ACTUALDATE
            sqlStat.AppendLine("         , 0") 'LOCALBR
            sqlStat.AppendLine("         , OV.LOCALRATE") 'LOCALRATE
            sqlStat.AppendLine("         , 0") 'TAXBR
            sqlStat.AppendLine("         , 0") 'AMOUNTPAY
            sqlStat.AppendLine("         , 0") 'LOCALPAY
            sqlStat.AppendLine("         , 0") 'TAXPAY

            sqlStat.AppendLine("         , CASE WHEN OV.DTLPOLPOD = 'POL1' THEN OBS.AGENTPOL1")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POL2' THEN OBS.AGENTPOL2")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POD1' THEN OBS.AGENTPOD1")
            sqlStat.AppendLine("                WHEN OV.DTLPOLPOD = 'POD2' THEN OBS.AGENTPOD2")
            sqlStat.AppendLine("                ELSE '1'")
            sqlStat.AppendLine("            END") 'INVOICEDBY
            'sqlStat.AppendLine("         , OV.INVOICEDBY") 'INVOICEDBY
            sqlStat.AppendLine("         , ''") 'APPLYID
            sqlStat.AppendLine("         , ''") 'APPLYTEXT
            sqlStat.AppendLine("         , ''") 'LASTSTEP
            sqlStat.AppendLine("         , @DEMSOADATE") 'SOAAPPDATE
            sqlStat.AppendLine("         , @REMARK")
            sqlStat.AppendLine("         , OV.BRID")
            sqlStat.AppendLine("         , OV.BRCOST")
            sqlStat.AppendLine("         , OV.DATEFIELD")
            sqlStat.AppendLine("         , OV.DATEINTERVAL")
            sqlStat.AppendLine("         , OV.BRADDEDCOST")
            sqlStat.AppendLine("         , OV.AGENTORGANIZER")
            sqlStat.AppendLine("         , '" & CONST_FLAG_NO & "'")
            sqlStat.AppendLine("         , OV.INITYMD")
            sqlStat.AppendLine("         , OV.INITUSER")
            sqlStat.AppendLine("         , @UPDYMD")
            sqlStat.AppendLine("         , @UPDUSER")
            sqlStat.AppendLine("         , @UPDTERMID")
            sqlStat.AppendLine("         , @RECEIVEYMD")
            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE OV")
            sqlStat.AppendLine("  LEFT JOIN GBT0005_ODR_VALUE OVS")
            sqlStat.AppendLine("         ON OVS.ORDERNO   = OV.ORDERNO")
            sqlStat.AppendLine("        AND OVS.TANKSEQ   = OV.TANKSEQ")
            sqlStat.AppendLine("        AND OVS.DTLPOLPOD = OV.DTLPOLPOD")
            sqlStat.AppendLine("        AND OVS.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("        AND OVS.CONTRACTORFIX <> ''")
            sqlStat.AppendLine("  LEFT JOIN GBT0004_ODR_BASE OBS")
            sqlStat.AppendLine("         ON OBS.ORDERNO   = OV.ORDERNO")
            sqlStat.AppendLine("        AND OBS.DELFLG   <> @DELFLG")
            'sqlStat.AppendLine("  LEFT JOIN GBM0010_CHARGECODE CC")
            'sqlStat.AppendLine("         ON CC.COSTCODE = OVS.COSTCODE")
            'sqlStat.AppendLine("        AND '1' = CASE WHEN OVS.DTLPOLPOD LIKE 'POL%' AND CC.LDKBN IN ('B','L') THEN '1' ")
            'sqlStat.AppendLine("                       WHEN OVS.DTLPOLPOD LIKE 'POD%' AND CC.LDKBN IN ('B','D') THEN '1' ")
            'sqlStat.AppendLine("                       WHEN OVS.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            'sqlStat.AppendLine("                       ELSE '1'")
            'sqlStat.AppendLine("                  END")
            'sqlStat.AppendLine("        AND CC.DELFLG  <> @DELFLG")
            'sqlStat.AppendLine("        AND CC.CLASS4 IN (SELECT CCS.CLASS4")
            'sqlStat.AppendLine("                            FROM GBM0010_CHARGECODE CCS")
            'sqlStat.AppendLine("                           WHERE CCS.COSTCODE = OV.COSTCODE")
            'sqlStat.AppendLine("                             AND '1' = CASE WHEN OV.DTLPOLPOD LIKE 'POL%' AND CCS.LDKBN IN ('B','L') THEN '1' ")
            'sqlStat.AppendLine("                                            WHEN OV.DTLPOLPOD LIKE 'POD%' AND CCS.LDKBN IN ('B','D') THEN '1' ")
            'sqlStat.AppendLine("                                            WHEN OV.DTLPOLPOD LIKE 'PO%'  THEN '' ")
            'sqlStat.AppendLine("                                            ELSE '1'")
            'sqlStat.AppendLine("                                       END")
            'sqlStat.AppendLine("                             AND CCS.DELFLG  <> @DELFLG)")
            sqlStat.AppendLine(" WHERE OV.DATAID=@DATAID;")
        End If
        Try

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@UPDYMD", SqlDbType.DateTime).Value = procDate
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@DATAID", SqlDbType.NVarChar).Value = Convert.ToString(drFixDemurrage.Item("DATAID"))
                    .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                    .Add("@TANKSEQ", SqlDbType.NVarChar).Value = tankSeq
                    .Add("@DTLPOLPOD", SqlDbType.NVarChar).Value = dtlPolPod
                    .Add("@COSTCODE", SqlDbType.NVarChar).Value = costCode 'FIXVALUEで取得したデマレッジ手数料費目
                    .Add("@REMARK", SqlDbType.NVarChar).Value = commRemark
                    .Add("@TAXATION", SqlDbType.NVarChar).Value = If(GetDefaultTaxation(Convert.ToString(drFixDemurrage.Item("COUNTRYCODE"))) = "on", "1", "0")
                    .Add("@COMMAMOUNT", SqlDbType.NVarChar).Value = commAmount
                    .Add("@DEMDATE", SqlDbType.Date).Value = DateStringToDateTime(Convert.ToString(drFixDemurrage.Item("ACTUALDATE")))
                    .Add("@DEMSOADATE", SqlDbType.Date).Value = procDate
                    .Add("@RECEIVEYMD", SqlDbType.NVarChar).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.ExecuteNonQuery()
            End Using
            Return orderNo
        Catch ex As Exception
            Throw
        End Try
    End Function
    ''' <summary>
    ''' 抽出結果につき編集
    ''' </summary>
    ''' <param name="dr"></param>
    Private Sub SetCanRowEdit(ByRef dr As DataRow)
        '********************
        'BliingClose済の判定があるなたこのあたりに
        '********************
        '一旦デマ画面のみ
        If {"GB_Demurrage", "GB_CostUp"}.Contains(Me.hdnListMapVariant.Value) Then
            'CLOSE済の場合入力項目は使用不可(これ以下の判定は行わずすべて使用不可)
            Dim isBillingClosed As String = Convert.ToString(dr("IS_BILLINGCLOSED"))
            If isBillingClosed = "1" Then
                dr("CANROWEDIT") = "0"
                Return
            End If
        End If

        '********************
        'JOTはフル許可　※計上済みは対象外
        '********************
        If GBA00003UserSetting.IS_JOTUSER Then
            dr("CANROWEDIT") = "1"
            Return
        End If

        '********************
        '国ごとの判定
        '********************
        'デマおよびSOAはINVOICEDBYの国で判定
        If {"GB_SOA", "GB_Demurrage", "GB_CostUp"}.Contains(Me.hdnListMapVariant.Value) Then
            Dim totalInvoice As String = Convert.ToString(dr("INVOICEDBY"))
            'デマおよびSOAはこれ以降の判定は行わない
            If Not IsInvoicedByCountryBelongUser(totalInvoice) Then
                dr("CANROWEDIT") = "0"
            Else
                dr("CANROWEDIT") = "1"
            End If
            Return
        End If

        Dim canRowEdit As Boolean = True
        'デマ、SOA以外はオーダー明細の国コードで判定
        If (Not {"GB_SOA", "GB_Demurrage"}.Contains(Me.hdnListMapVariant.Value)) AndAlso Not dr("COUNTRYCODE").Equals(GBA00003UserSetting.COUNTRYCODE) Then
            canRowEdit = False 'SOAではない入力パターンでレコードの国と不一致
        End If
        '********************
        '動静の状態判定
        '********************

        'ARVD済（DATEFIELD ETA,ETD双方のACTUALDATEが埋まっている場合）かつSOA,デマの以外のモードでは画面をいじらせない
        Dim orderNo As String = Convert.ToString(dr("ORDERNO"))
        Dim tankSeq As String = Convert.ToString(dr("TANKSEQ"))
        Dim dtlPolPod As String = Convert.ToString(dr("DTLPOLPOD"))
        If dtlPolPod = "POL1" AndAlso IsOrderTankArrived(orderNo, tankSeq) Then
            canRowEdit = False
        End If

        '********************
        '判定結果をDataRowに書き込み
        '********************
        If canRowEdit Then
            dr("CANROWEDIT") = "1"
        Else
            dr("CANROWEDIT") = "0"
        End If
    End Sub
    ''' <summary>
    ''' 発着双方のACTUAL日付が埋まっているかチェック
    ''' </summary>
    ''' <param name="orderNo"></param>
    ''' <param name="tankSeq"></param>
    ''' <returns>True埋まっている,False埋まっていない</returns>
    Private Function IsOrderTankArrived(orderNo As String, tankSeq As String) As Boolean
        Static dicIsArrivedData As Dictionary(Of String, Boolean)
        Dim keyString As String = orderNo & "\@\" & tankSeq
        If dicIsArrivedData Is Nothing Then
            dicIsArrivedData = New Dictionary(Of String, Boolean)
        End If
        'ノンブレーカー(オーダーNo先頭NB)は発着の概念が無いため埋まってない扱いで終了
        If orderNo.StartsWith("NB") Then
            Return False
        End If

        If dicIsArrivedData.ContainsKey(keyString) Then
            Return dicIsArrivedData(keyString)
        End If

        Dim sqlStat As New StringBuilder
        'DATEFIELDがEDT ETAのカウントが2件ならば発着日付が両方とも埋まっている扱い
        sqlStat.AppendLine("SELECT ORDERNO ")
        sqlStat.AppendLine("      ,TANKSEQ ")
        sqlStat.AppendLine("      ,SUM(CASE WHEN DATEFIELD IN ('ETD','ETA','ETD1','ETA1') THEN 1 ELSE 0 END)  AS ETDETAINPUTEDCOUNT")
        sqlStat.AppendLine("      ,SUM(CASE WHEN DATEFIELD IN ('ETD','ETD1') THEN 1 ELSE 0 END)               AS ETDINPUTEDCOUNT")
        sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE WITH (nolock) ")
        sqlStat.AppendLine(" WHERE ORDERNO = @ORDERNO")
        sqlStat.AppendLine("   AND ACTUALDATE <> @ACTUALDATE ")
        sqlStat.AppendLine("   AND DELFLG     <> @DELFLG ")
        sqlStat.AppendLine(" GROUP BY ORDERNO,TANKSEQ  ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
            sqlCon.Open()
            With sqlCmd.Parameters
                .Add("@ORDERNO", SqlDbType.NVarChar).Value = orderNo
                .Add("@ACTUALDATE", SqlDbType.Date).Value = "1900/01/01"
                .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
            End With
            Using sqlRr = sqlCmd.ExecuteReader
                If sqlRr.HasRows = False Then
                    Return False
                End If
                Dim key As String = ""
                Dim result As Boolean = False
                While sqlRr.Read
                    key = Convert.ToString(sqlRr("ORDERNO")) & "\@\" & Convert.ToString(sqlRr("TANKSEQ"))
                    result = False
                    If sqlRr("ETDETAINPUTEDCOUNT").Equals(2) Then
                        result = True
                    End If

                    If Not dicIsArrivedData.ContainsKey(key) Then
                        dicIsArrivedData.Add(key, result)
                    End If
                End While
            End Using
        End Using
        If dicIsArrivedData.ContainsKey(keyString) Then
            Return dicIsArrivedData(keyString)
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' Invoicedbyの国がログインユーザーの国と一致するか判定
    ''' </summary>
    ''' <param name="invoicedBy">請求者コード(顧客コード)</param>
    ''' <returns>True：InvoicedByの国コードがログインユーザーに属す,False:属さない</returns>
    Private Function IsInvoicedByCountryBelongUser(invoicedBy As String) As Boolean
        Static countryResult As Dictionary(Of String, String)
        If countryResult Is Nothing Then '一回のリクエスト実行時のみ当IF内は呼ばれる
            countryResult = New Dictionary(Of String, String)
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT CARRIERCODE AS INVOICEDBY")
            sqlStat.AppendLine("  FROM GBM0005_TRADER WITH (nolock) ")
            sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
            sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
            sqlStat.AppendLine("   AND CLASS       = @CLASS")
            sqlStat.AppendLine("   AND DELFLG      <> @DELFLG")
            'DB接続
            Using sqlCon As New SqlConnection(COA0019Session.DBcon),
                  sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@COUNTRYCODE", SqlDbType.NVarChar).Value = GBA00003UserSetting.COUNTRYCODE
                    .Add("@CLASS", SqlDbType.NVarChar).Value = C_TRADER.CLASS.AGENT
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With
                Using sqlRr = sqlCmd.ExecuteReader
                    If sqlRr.HasRows = False Then
                        Return False
                    End If
                    Dim key As String = ""
                    While sqlRr.Read
                        key = Convert.ToString(sqlRr("INVOICEDBY"))

                        If Not countryResult.ContainsKey(key) Then
                            countryResult.Add(key, key)
                        End If
                    End While
                End Using
            End Using
        End If
        Return countryResult.ContainsKey(invoicedBy)

    End Function

    ''' <summary>
    ''' 課税フラグのデフォルト値取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>仮作成にて変動の可能性がある為、デフォルト値取得関数化</remarks>
    Private Function GetDefaultTaxation(countryCode As String) As String
        Return If(GBA00003UserSetting.IS_JPOPERATOR AndAlso countryCode = "JP", "on", "")
    End Function

    ''' <summary>
    ''' 最新のDATAID取得
    ''' </summary>
    ''' <returns></returns>
    Private Function GetDATAID(ByVal applyId As String, Optional ByRef sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim dataID As String = ""
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Dim sqlStat As New StringBuilder
            sqlStat.AppendLine("SELECT  DATAID ")
            sqlStat.AppendLine("  FROM GBT0005_ODR_VALUE")
            sqlStat.AppendLine(" WHERE APPLYID   = @APPLYID")
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@APPLYID", SqlDbType.NVarChar, 20).Value = applyId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Order Value error")
                    End If

                    dataID = Convert.ToString(dt.Rows(0).Item(0))
                End Using
            End Using
            Return dataID
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
    ''' SOA締め申請メール送信
    ''' </summary>
    ''' <param name="applyId"></param>
    Private Function SendSoaClosingMail(applyId As String, lastStep As String) As String
        'メール
        Dim GBA00009MailSendSet As New GBA00009MailSendSet
        GBA00009MailSendSet.COMPCODE = COA0019Session.APSRVCamp
        GBA00009MailSendSet.EVENTCODE = C_SCLOSEEVENT.APPLY
        GBA00009MailSendSet.MAILSUBCODE = ""
        GBA00009MailSendSet.APPLYID = applyId
        GBA00009MailSendSet.APPLYSTEP = lastStep
        GBA00009MailSendSet.GBA00009setMailToBliingClose()
        If GBA00009MailSendSet.ERR <> C_MESSAGENO.NORMAL Then
            Return GBA00009MailSendSet.ERR
        End If
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' 費目マスタより取得したCostCode Ld区分退避クラス
    ''' </summary>
    Private Class CostActy
        Public Property CostCode As String = ""
        Public Property LdKbn As String = ""
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="costCode"></param>
        ''' <param name="ldKbn"></param>
        Public Sub New(costCode As String, ldKbn As String)
            Me.CostCode = costCode
            Me.LdKbn = ldKbn
        End Sub
    End Class
End Class