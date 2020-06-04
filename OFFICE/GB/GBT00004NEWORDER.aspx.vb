Option Strict On
Imports System.Data.SqlClient
Imports BASEDLL

Public Class GBT00004NEWORDER
    Inherits GbPageCommon
    Private Const CONST_MAPID As String = "GBT00004N" '自身のMAPID
    Private Const CONST_ORGANIZER As String = "Organizer"
    Private Const CONST_SHIPPERCLASS As String = "S"
    Private Const CONSIGNEECLASS As String = "C"

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
                '****************************************
                'ヘッダータイトル取得
                '****************************************
                Dim COA0031ProfMap As New BASEDLL.COA0031ProfMap 'タイトル文言取得
                With COA0031ProfMap
                    .MAPIDP = CONST_MAPID
                    .VARIANTP = "NewOrder"
                    .COA0031GetDisplayTitle()
                    Me.lblTitleText.Text = .NAMES
                End With
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                '前々画面（前画面で保持している検索条件）をHiddenに記録
                '****************************************
                SetPrevDisplayInfo()
                '****************************************
                'Breaker情報を各テーブルより取得
                '****************************************
                Dim brId As String = Me.hdnBrId.Value
                Dim bd As BreakerData = Me.GetBreakerdata(brId)
                ViewState("BRDATA") = bd '画面表示、計算元のなる値をビューステートに退避

                '****************************************
                '取得したデータを画面展開
                '****************************************
                SetDisplayBreakerData(bd)
                '****************************************
                '画面の表示/非表示制御
                '****************************************
                VisibleControls(bd)
            End If
            '**********************************************
            'ポストバック時
            '**********************************************
            If IsPostBack Then
                'DO SOMETHING!
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
                ' 自動計算処理
                '**********************
                If Me.hdnCalcFunctionName.Value <> "" Then
                    Dim funcName As String = Me.hdnCalcFunctionName.Value
                    Me.hdnCalcFunctionName.Value = ""
                    Dim mi As Reflection.MethodInfo = Me.GetType.GetMethod(funcName)
                    If mi IsNot Nothing Then
                        CallByName(Me, funcName, CallType.Method, Nothing)
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
                ' Help表示
                '**********************
                If Me.hdnHelpChange.Value IsNot Nothing AndAlso Me.hdnHelpChange.Value <> "" Then
                    DivShowHelp_DoubleClick(CONST_MAPID)
                    Me.hdnHelpChange.Value = ""
                End If
            End If
            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
        Catch ex As Threading.ThreadAbortException
            '中断時エラーは無視
        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.SYSTEMADM 'ここは適宜変えてください
            Dim NORMAL As String = ""
            CommonFunctions.ShowMessage(C_MESSAGENO.SYSTEMADM, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", messageNo)})

            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = messageNo
            COA0003LogFile.COA0003WriteLog()

            Return
        Finally
            '項目の入力可否制御
            'disabledControls()
            Me.hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
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
                'カレンダビュー表示切替
                Case Me.vLeftCal.ID
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    If targetObject IsNot Nothing Then
                        Dim txtobj As TextBox = DirectCast(targetObject, TextBox)
                        Me.hdnCalendarValue.Value = txtobj.Text
                        Me.mvLeft.Focus()
                    End If
                Case vLeftConsignee.ID
                    Dim dblClickField As Control = Me.FindControl(Me.hdnTextDbClickField.Value)
                    Dim countryCode As String = Me.hdnDeliveryCountry1.Value
                    If Me.hdnBrType.Value = C_BRTYPE.SALES Then
                        'SALESの場合
                        Dim dt As DataTable = GetConsignee(countryCode)
                        With Me.lbConsignee
                            .DataSource = dt
                            .DataTextField = "LISTBOXNAME"
                            .DataValueField = "CUSTOMERCODE"
                            .DataBind()
                            .Focus()
                        End With
                    Else
                        'OPEの場合
                        Dim dt As DataTable = GetAgent(countryCode)
                        With Me.lbConsignee
                            .DataSource = dt
                            .DataTextField = "LISTBOXNAME"
                            .DataValueField = "CODE"
                            .DataBind()
                            .Focus()
                        End With
                    End If

                    '入力済のデータを選択状態にする
                    If dblClickField IsNot Nothing AndAlso lbConsignee.Items IsNot Nothing Then
                        Dim dblClickFieldText As TextBox = DirectCast(dblClickField, TextBox)
                        Dim findLbValue As ListItem = lbConsignee.Items.FindByValue(dblClickFieldText.Text)
                        If findLbValue IsNot Nothing Then
                            findLbValue.Selected = True
                        End If
                    End If
            End Select
        End If
    End Sub
    ''' <summary>
    ''' 保存ボタン押下時イベント
    ''' </summary>
    Public Sub btnSave_Click()
        '******************************
        ' 入力チェック
        '******************************
        Dim retMessageNo As String = ""
        retMessageNo = CheckInput()
        If retMessageNo <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(retMessageNo, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        '******************************
        ' オーダーテーブルにブレーカーコピー作成
        '******************************
        EntryNewOrder()
        '******************************
        ' 次画面遷移
        '******************************
        '■■■ 画面遷移先URL取得 ■■■
        Dim COA0012DoUrl As New COA0012DoUrl
        COA0012DoUrl.MAPIDP = CONST_MAPID
        COA0012DoUrl.VARIP = "GB_OrderNew"
        COA0012DoUrl.COA0012GetDoUrl()
        If COA0012DoUrl.ERR = C_MESSAGENO.NORMAL Then
        Else
            CommonFunctions.ShowMessage(COA0012DoUrl.ERR, Me.lblFooterMessage, pageObject:=Me)
            Return
        End If
        Session("MAPmapid") = CONST_MAPID
        Session("MAPvariant") = "GB_OrderNew"
        '画面遷移実行
        Server.Transfer(COA0012DoUrl.URL)
    End Sub
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '■■■ 画面戻先URL取得 ■■■
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = "GB_OrderNew"
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
            Me.lblFooterMessage.Text = COA0011ReturnUrl.NAMES
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
                Case vLeftConsignee.ID
                    '荷受人選択時
                    targetObject = FindControl(Me.hdnTextDbClickField.Value)
                    Dim targetTextBox As TextBox = Nothing
                    If targetObject IsNot Nothing Then
                        targetTextBox = DirectCast(targetObject, TextBox)
                        Dim customerCode As String = ""
                        If Me.lbConsignee.SelectedItem IsNot Nothing Then
                            customerCode = Me.lbConsignee.SelectedItem.Value
                        End If
                        SetDisplayConsignee(targetTextBox, customerCode)
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
    ''' 画面 オーダーの総額を計算
    ''' </summary>
    Public Sub CalcOrderTotalInvoiced()
        Dim brData As BreakerData = DirectCast(ViewState("BRDATA"), BreakerData)
        If brData Is Nothing Then
            Return
        End If
        'JAVASCRIPT側で制御しているが念のため
        If Me.txtCopy.Text.Trim = "" Then
            Return
        End If
        Dim decCopy As Decimal = DecimalStringToDecimal(Me.txtCopy.Text.Trim)
        decCopy = RoundDown(decCopy, 0)
        Me.txtCopy.Text = NumberFormat(decCopy, "#,##0")
        Dim noOfTanks As Decimal = DecimalStringToDecimal(brData.NoOfTanks)
        Dim brTotalInvoiced As Decimal = DecimalStringToDecimal(brData.BrTotalInvoiced)
        If Not {"", "0"}.Contains(brData.BrAmtPrincipal) Then
            brTotalInvoiced = DecimalStringToDecimal(brData.BrAmtPrincipal)
        End If
        Dim totalTanks As Decimal = decCopy * noOfTanks

        Me.txtTotalTanks.Text = NumberFormat(totalTanks, "#,##0")

        Dim costTotalInvoiced As Decimal = decCopy * brTotalInvoiced
        costTotalInvoiced = RoundDown(costTotalInvoiced)
        Me.txtTotalInvoiced.Text = NumberFormat(costTotalInvoiced)

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
        AddLangSetting(dicDisplayText, Me.lblBrId, "ブレーカーNO", "BREAKER No")
        AddLangSetting(dicDisplayText, Me.lblOffice, "代理店", "OFFICE")
        AddLangSetting(dicDisplayText, Me.lblSalesPic, "代理店担当", "SALES.PIC")
        AddLangSetting(dicDisplayText, Me.lblNoOfTanks, "タンク本数", "NO of Tanks")
        AddLangSetting(dicDisplayText, Me.lblCopy, "Volume", "Volume")
        AddLangSetting(dicDisplayText, Me.lblTotalTanks, "タンク総数", "Total Tanks")
        AddLangSetting(dicDisplayText, Me.lblFillingDate, "充填日", "FillingDate")
        AddLangSetting(dicDisplayText, Me.lblEtd1, "出港日1", "ETD1")
        AddLangSetting(dicDisplayText, Me.lblEta1, "着港日1", "ETA1")
        AddLangSetting(dicDisplayText, Me.lblEtd2, "出港日2", "ETD2")
        AddLangSetting(dicDisplayText, Me.lblEta2, "着港日2", "ETA2")
        AddLangSetting(dicDisplayText, Me.lblTotalInvoiced, "総額", "TOTAL INVOICED")

        ' AddLangSetting(dicDisplayText, Me.btnSave, "保存", "Save")
        AddLangSetting(dicDisplayText, Me.btnSave, "作成", "Create")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub

    ''' <summary>
    ''' 前画面情報保持
    ''' </summary>
    Private Sub SetPrevDisplayInfo()
        If TypeOf Page.PreviousPage Is GBT00003RESULT Then
            '検索結果画面の場合
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
                                                                        {"hdnSelectedBrId", Me.hdnBrId},
                                                                        {"hdnReportVariant", Me.hdnReportVariant}}

            For Each item As KeyValuePair(Of String, HiddenField) In dicObjs
                Dim tmpCont As Control = prevObj.FindControl(item.Key)
                If tmpCont IsNot Nothing Then
                    Dim tmpHdn As HiddenField = DirectCast(tmpCont, HiddenField)
                    item.Value.Value = tmpHdn.Value
                End If
            Next
            Dim prevRightList As ListBox = DirectCast(prevObj.FindControl("lbRightList"), ListBox)
            If prevRightList IsNot Nothing Then
                Me.hdnListId.Value = prevRightList.SelectedValue
            End If

        End If
    End Sub
    ''' <summary>
    ''' ブレーカー番号をもとに各ブレーカー関連テーブルよりオーダーに必要な情報を取得
    ''' </summary>
    ''' <param name="brId"></param>
    ''' <returns></returns>
    Private Function GetBreakerdata(brId As String) As BreakerData
        Dim retBrData As New BreakerData
        Dim ds As New DataSet
        Dim brDt As New DataTable
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            '各種テーブルよりデータ取得
            Dim dicBrInfo As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(brId, sqlCon)
            ViewState("BRINFO") = (dicBrInfo) '画面表示時のブレーカー情報をVIEWSTATEに退避
            Dim dtBrBase As DataTable = GetBreakerBase(dicBrInfo, sqlCon)
            Dim totalCost As String = GetBreakerValue(dicBrInfo, "", "", sqlCon)
            Dim totalShipperCost As String = GetBreakerValue(dicBrInfo, "1", "", sqlCon)
            Dim totalConsigneeCost As String = GetBreakerValue(dicBrInfo, "0", "", sqlCon)
            Dim totalProvisionalCost As String = GetBreakerValue(dicBrInfo, "", "P", sqlCon) '仮計上コスト総額

            '取得値を戻り値に設定
            Dim drBrBase As DataRow = dtBrBase.Rows(0)
            '取得した情報よりtotalInvoicedを計算

            retBrData.BrId = brId
            retBrData.NoOfTanks = Convert.ToString(drBrBase("NOOFTANKS"))

            Dim dt As DataTable = New DataTable
            If Convert.ToString(drBrBase("AGENTORGANIZER")) <> "" Then
                dt = GetAgent("", Convert.ToString(drBrBase("AGENTORGANIZER")))
            End If

            'データが取れない場合はそのまま終了
            If dt Is Nothing OrElse dt.Rows.Count > 0 Then
                Dim dr As DataRow = dt.Rows(0)
                retBrData.Office = Convert.ToString(dr.Item("NAME"))
            Else
                retBrData.Office = ""
            End If

            Me.hdnAgentOrganizer.Value = Convert.ToString(drBrBase("AGENTORGANIZER"))
            retBrData.SalesPic = COA0019Session.USERNAME
            Me.hdnSalesPic.value = COA0019Session.USERID
            retBrData.JotBlNoSt = ""　'TODO BL取得
            retBrData.JotBlNo = ""    'TODO BL取得
            retBrData.BrTotalInvoiced = CalcBrInvoiceTotal(drBrBase, totalCost, True)

            Dim sOrgFlg As Boolean = False
            Dim cOrgFlg As Boolean = False
            If Convert.ToString(drBrBase("BILLINGCATEGORY")) = GBC_DELIVERYCLASS.SHIPPER Then
                sOrgFlg = True
            ElseIf Convert.ToString(drBrBase("BILLINGCATEGORY")) = GBC_DELIVERYCLASS.CONSIGNEE Then
                cOrgFlg = True
            End If

            retBrData.BrShipperTotalInvoiced = CalcBrInvoiceTotal(drBrBase, totalShipperCost, sOrgFlg)
            retBrData.BrConsigneeTotalInvoiced = CalcBrInvoiceTotal(drBrBase, totalConsigneeCost, cOrgFlg)

            retBrData.BrShipperCostTotal = totalShipperCost
            retBrData.BrConsigneeCostTotal = totalConsigneeCost

            retBrData.BrHireage = Convert.ToString(drBrBase.Item("JOTHIREAGE"))
            retBrData.BrAdjustment = Convert.ToString(drBrBase.Item("COMMERCIALFACTOR"))
            retBrData.BrAmtPrincipal = Convert.ToString(drBrBase.Item("AMTPRINCIPAL"))
            retBrData.BrAmtDiscount = Convert.ToString(drBrBase.Item("AMTDISCOUNT"))
            retBrData.BrEtd1 = Convert.ToString(drBrBase("ETD1"))
            retBrData.BrEta1 = Convert.ToString(drBrBase("ETA1"))
            retBrData.BrEtd2 = Convert.ToString(drBrBase("ETD2"))
            retBrData.BrEta2 = Convert.ToString(drBrBase("ETA2"))
            retBrData.BrType = Convert.ToString(drBrBase("BRTYPE"))

            retBrData.VesselName = Convert.ToString(drBrBase("VSL1"))
            retBrData.VoyageNo = Convert.ToString(drBrBase("VOY1"))
            retBrData.Consignee = Convert.ToString(drBrBase("CONSIGNEE"))
            retBrData.DeliveryCountry1 = Convert.ToString(drBrBase("DELIVERYCOUNTRY1"))

            If Convert.ToString(drBrBase.Item("ISTRILATERAL")) = "1" Then
                retBrData.IsTrilateral = True
            Else
                retBrData.IsTrilateral = False
            End If
            retBrData.BrCommission = Convert.ToString(drBrBase("FEE"))
            retBrData.BillingCategory = Convert.ToString(drBrBase("BILLINGCATEGORY"))
            retBrData.BrValidityFrom = Convert.ToString(drBrBase("VALIDITYFROM"))
            retBrData.BrValidityTo = Convert.ToString(drBrBase("VALIDITYTO"))
            retBrData.BrTotalCost = totalCost
            retBrData.BrTotalProvisionalCost = totalProvisionalCost

            sqlCon.Close()
        End Using
        Return retBrData
    End Function
    ''' <summary>
    ''' ブレーカー関連付け情報データを取得
    ''' </summary>
    ''' <param name="sqlCon">オプション 項目</param>
    ''' <returns>ディクショナリ キー：区分(POD1、POL1等) , 値：直近ブレーカー関連付け</returns>
    ''' <remarks>COPY実施時に直近を再抽出しAPPLYIDに変化がないか突き合わせをする</remarks>
    Private Function GetBreakerInfo(brId As String, Optional sqlCon As SqlConnection = Nothing) As Dictionary(Of String, BreakerInfo)
        Dim canCloseConnect As Boolean = False
        Dim retDic As New Dictionary(Of String, BreakerInfo)
        Dim sqlStat As New Text.StringBuilder
        '生きているブレーカーは基本情報＋発地着地(最大4)の5レコード想定
        sqlStat.AppendLine("Select BRID")
        sqlStat.AppendLine("      ,BRTYPE")
        sqlStat.AppendLine("      ,SUBID")
        sqlStat.AppendLine("      ,TYPE")
        sqlStat.AppendLine("      ,LINKID")
        sqlStat.AppendLine("      ,STYMD")
        sqlStat.AppendLine("      ,APPLYID")
        sqlStat.AppendLine("      ,CAST(UPDTIMSTP As bigint) AS TIMSTP")
        sqlStat.AppendLine("  FROM GBT0001_BR_INFO")
        sqlStat.AppendLine(" WHERE BRID         = @BRID")
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brId
                    .Add("@STYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@ENDYMD", SqlDbType.Date).Value = Date.Now
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES

                End With
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt IsNot Nothing Then
                        For Each dr As DataRow In dt.Rows
                            Dim item As New BreakerInfo
                            item.BrId = Convert.ToString(dr("BRID"))
                            item.BrType = Convert.ToString(dr("BRTYPE"))
                            item.SubId = Convert.ToString(dr("SUBID"))
                            item.Type = Convert.ToString(dr("TYPE"))
                            item.LinkId = Convert.ToString(dr("LINKID"))
                            item.Stymd = Convert.ToString(dr("STYMD"))
                            item.ApplyId = Convert.ToString(dr("APPLYID"))
                            item.TimeStamp = Convert.ToString(dr("TIMSTP"))
                            retDic.Add(item.Type, item)
                        Next
                    End If
                End Using

            End Using
            Return retDic
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
    ''' ブレーカー基本情報取得処理
    ''' </summary>
    ''' <param name="dicBrInfo"></param>
    ''' <returns></returns>
    Private Function GetBreakerBase(dicBrInfo As Dictionary(Of String, BreakerInfo), Optional sqlCon As SqlConnection = Nothing) As DataTable
        Dim canCloseConnect As Boolean = False
        Dim retDt As DataTable = Nothing
        Dim sqlStat As New Text.StringBuilder
        sqlStat.AppendLine("SELECT BS.BRID ")
        sqlStat.AppendLine("      ,BS.BRBASEID ")
        sqlStat.AppendLine("      ,BS.STYMD ")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYFROM WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYFROM,'yyyy/MM/dd') END AS VALIDITYFROM")
        sqlStat.AppendLine("      ,CASE BS.VALIDITYTO   WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.VALIDITYTO  ,'yyyy/MM/dd') END AS VALIDITYTO")
        sqlStat.AppendLine("      ,BS.NOOFTANKS ")
        sqlStat.AppendLine("      ,BS.PRODUCTWEIGHT ")
        sqlStat.AppendLine("      ,PD.GRAVITY AS GRAVITY")
        sqlStat.AppendLine("      ,BS.LOADING ")
        sqlStat.AppendLine("      ,BS.STEAMING ")
        sqlStat.AppendLine("      ,BS.TIP ")
        sqlStat.AppendLine("      ,BS.EXTRA ")
        sqlStat.AppendLine("      ,BS.JOTHIREAGE ")
        sqlStat.AppendLine("      ,BS.COMMERCIALFACTOR ")
        sqlStat.AppendLine("      ,BS.AMTREQUEST ")
        sqlStat.AppendLine("      ,BS.AMTPRINCIPAL ")
        sqlStat.AppendLine("      ,BS.AMTDISCOUNT ")
        sqlStat.AppendLine("      ,BS.AGENTORGANIZER ")
        sqlStat.AppendLine("      ,BS.AGENTPOL1 ")
        sqlStat.AppendLine("      ,BS.AGENTPOL2 ")
        sqlStat.AppendLine("      ,BS.AGENTPOD1 ")
        sqlStat.AppendLine("      ,BS.AGENTPOD2 ")
        sqlStat.AppendLine("      ,CASE BS.ETD1 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETD1,'yyyy/MM/dd') END AS ETD1")
        sqlStat.AppendLine("      ,CASE BS.ETA1 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETA1,'yyyy/MM/dd') END AS ETA1")
        sqlStat.AppendLine("      ,CASE BS.ETD2 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETD2,'yyyy/MM/dd') END AS ETD2")
        sqlStat.AppendLine("      ,CASE BS.ETA2 WHEN '1900/01/01' THEN '' ELSE FORMAT(BS.ETA2,'yyyy/MM/dd') END AS ETA2")
        sqlStat.AppendLine("      ,BS.VSL1")
        sqlStat.AppendLine("      ,BS.VOY1")
        sqlStat.AppendLine("      ,BS.SHIPPER")
        sqlStat.AppendLine("      ,BS.CONSIGNEE")
        sqlStat.AppendLine("      ,BI.BRTYPE")
        sqlStat.AppendLine("      ,BS.DELIVERYCOUNTRY1")
        sqlStat.AppendLine("      ,BS.FEE")
        sqlStat.AppendLine("      ,BS.BILLINGCATEGORY")
        sqlStat.AppendLine("      ,BS.USINGLEASETANK")
        sqlStat.AppendLine("  FROM GBT0002_BR_BASE BS ")
        sqlStat.AppendLine("  LEFT JOIN GBT0001_BR_INFO BI") 'BRTYPE
        sqlStat.AppendLine("    ON  BI.BRID     = BS.BRID")
        sqlStat.AppendLine("   AND  BI.LINKID   = BS.BRBASEID")
        sqlStat.AppendLine("   AND  BI.DELFLG  <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'PRODUCT名称用JOIN
        sqlStat.AppendLine("    ON  PD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND  PD.STYMD       <= BS.ENDYMD")
        sqlStat.AppendLine("   AND  PD.ENDYMD      >= BS.STYMD")
        sqlStat.AppendLine("   AND  PD.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("   AND  PD.ENABLED      = @ENABLED")
        sqlStat.AppendLine(" WHERE BS.BRID     = @BRID ")
        sqlStat.AppendLine("   AND BS.BRBASEID = @BRBASEID ")
        Try
            Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brInfoOrganizer.BrId
                    .Add("@BRBASEID", SqlDbType.NVarChar, 20).Value = brInfoOrganizer.LinkId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@ENABLED", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                End With
                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Breaker base info Error")
                    End If
                    retDt = CreateOrganizerInfoTable()
                    For Each col As DataColumn In dt.Columns
                        retDt.Rows(0)(col.ColumnName) = Convert.ToString(dt.Rows(0)(col.ColumnName))
                    Next

                End Using
                'Br紐づけ情報が4件以上の場合は三国間扱い(INFO,PODx,POLx)
                If dicBrInfo.Count >= 4 Then
                    retDt.Rows(0).Item("ISTRILATERAL") = "1"
                Else
                    retDt.Rows(0).Item("ISTRILATERAL") = "0"
                End If
            End Using
            Return retDt
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

        Return retDt
    End Function
    ''' <summary>
    ''' BreakerValueよりCost合計を取得
    ''' </summary>
    ''' <param name="dicBrInfo"></param>
    ''' <returns></returns>
    Private Function GetBreakerValue(dicBrInfo As Dictionary(Of String, BreakerInfo), billing As String, Provisional As String, Optional sqlCon As SqlConnection = Nothing) As String
        Dim canCloseConnect As Boolean = False
        Dim sqlStat As New Text.StringBuilder
        Dim retValue As String = ""
        sqlStat.AppendLine("SELECT SUM(VL.USD) AS TOTALCOST ")
        'sqlStat.AppendLine("      ,SUM(CASE WHEN VL.LOCAL <> 0 AND EX.EXRATE IS NULL THEN 0")
        'sqlStat.AppendLine("                WHEN VL.LOCAL <> 0 AND USDDECIMAL.VALUE2 = '" & GBC_ROUNDFLG.DOWN & "' THEN CEILING((VL.LOCAL / EX.EXRATE) * POWER(10,USDDECIMAL.VALUE1)) / POWER(10,USDDECIMAL.VALUE1) ")
        'sqlStat.AppendLine("                WHEN VL.LOCAL <> 0 AND USDDECIMAL.VALUE2 = '" & GBC_ROUNDFLG.UP & "' THEN FLOOR((VL.LOCAL / EX.EXRATE) * POWER(10,USDDECIMAL.VALUE1)) / POWER(10,USDDECIMAL.VALUE1) ")
        'sqlStat.AppendLine("                WHEN VL.LOCAL <> 0 AND USDDECIMAL.VALUE2 = '" & GBC_ROUNDFLG.ROUND & "' THEN ROUND((VL.LOCAL / EX.EXRATE),USDDECIMAL.VALUE1 * 1) ")
        'sqlStat.AppendLine("                ELSE VL.USD END) AS USDAMOUNT ")
        sqlStat.AppendLine("  FROM GBT0003_BR_VALUE VL ")
        'sqlStat.AppendLine("LEFT JOIN GBM0020_EXRATE EX ")
        'sqlStat.AppendLine("       ON EX.COMPCODE     = @COMPCODE ")
        'sqlStat.AppendLine("      AND EX.CURRENCYCODE = VL.CURRENCYCODE ")
        'sqlStat.AppendLine("      AND EX.TARGETYM     = @TARGETYM ")
        'sqlStat.AppendLine("      AND EX.DELFLG      <> @DELFLG")
        ''USD(小数桁数)
        'sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE USDDECIMAL")
        'sqlStat.AppendLine("         ON USDDECIMAL.COMPCODE   = '" & GBC_COMPCODE_D & "'")
        'sqlStat.AppendLine("        AND USDDECIMAL.SYSCODE    = '" & C_SYSCODE_GB & "'")
        'sqlStat.AppendLine("        AND USDDECIMAL.CLASS      = '" & C_FIXVALUECLAS.USD_DECIMALPLACES & "'")
        'sqlStat.AppendLine("        AND USDDECIMAL.KEYCODE    = '" & GBC_CUR_USD & "'")
        'sqlStat.AppendLine("        AND USDDECIMAL.DELFLG    <> @DELFLG")

        sqlStat.AppendLine(" WHERE VL.BRID      = @BRID ")
        sqlStat.AppendLine("   AND VL.DELFLG   <> @DELFLG")
        If billing <> "" Then
            sqlStat.AppendLine("   AND VL.BILLING   = @BILLING")
        End If
        '費用のみをサマリーするための条件
        sqlStat.AppendLine("   AND EXISTS (SELECT 1")
        sqlStat.AppendLine("                 FROM GBM0010_CHARGECODE CST")
        sqlStat.AppendLine("                WHERE CST.DELFLG    <> @DELFLG")
        sqlStat.AppendLine("                  AND CST.COSTCODE   = VL.COSTCODE")
        If Provisional <> "" Then
            ' 仮計上項目を取得
            sqlStat.AppendLine("                  AND ( CST.CLASS3    <> '' AND CST.CLASS3    = 'P' )")
        Else
            ' 全費用項目を抽出
            sqlStat.AppendLine("                  AND CST.CLASS3    <> ''")
        End If
        sqlStat.AppendLine("                  AND CST.COMPCODE   = @COMPCODE")
        sqlStat.AppendLine("               )")
        Try
            If sqlCon Is Nothing Then
                sqlCon = New SqlConnection(COA0019Session.DBcon)
                sqlCon.Open()
                canCloseConnect = True
            End If
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                Dim brInfoOrganizer As BreakerInfo = dicBrInfo("INFO")
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = brInfoOrganizer.BrId
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    If billing <> "" Then
                        .Add("@BILLING", SqlDbType.NVarChar).Value = billing
                    End If
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE
                    '.Add("@TARGETYM", SqlDbType.Date).Value = Now.ToString("yyyy/MM") & "/01"
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    Dim dt As New DataTable
                    sqlDa.Fill(dt)
                    If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                        Throw New Exception("Get Breaker value Error")
                    End If
                    retValue = Convert.ToString(dt.Rows(0).Item("TOTALCOST"))

                End Using
            End Using
            Return retValue
        Catch
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
    ''' オーナー情報を格納する空のデータテーブルを作成する
    ''' </summary>
    ''' <returns>Organizer情報のデータテーブルを作成</returns>
    ''' <remarks>複数レコードはありえないので１レコード作り返却</remarks>
    Private Function CreateOrganizerInfoTable() As DataTable
        Dim retDt As New DataTable
        retDt.TableName = "ORGANIZER_INFO"
        retDt.Columns.Add("BRID", GetType(String))
        retDt.Columns.Add("BRBASEID", GetType(String))
        retDt.Columns.Add("STYMD", GetType(String))
        retDt.Columns.Add("USETYPE", GetType(String))
        retDt.Columns.Add("VALIDITYFROM", GetType(String))
        retDt.Columns.Add("VALIDITYTO", GetType(String))
        retDt.Columns.Add("TERMTYPE", GetType(String))
        retDt.Columns.Add("NOOFTANKS", GetType(String))
        retDt.Columns.Add("SHIPPER", GetType(String))
        retDt.Columns.Add("CONSIGNEE", GetType(String))
        retDt.Columns.Add("CARRIER1", GetType(String))
        retDt.Columns.Add("CARRIER2", GetType(String))
        retDt.Columns.Add("PRODUCTCODE", GetType(String))
        retDt.Columns.Add("IMDGCODE", GetType(String))
        retDt.Columns.Add("UNNO", GetType(String))
        retDt.Columns.Add("RECIEPTCOUNTRY1", GetType(String))
        retDt.Columns.Add("RECIEPTPORT1", GetType(String))
        retDt.Columns.Add("LOADCOUNTRY1", GetType(String))
        retDt.Columns.Add("LOADPORT1", GetType(String))
        retDt.Columns.Add("DISCHARGECOUNTRY1", GetType(String))
        retDt.Columns.Add("DISCHARGEPORT1", GetType(String))
        retDt.Columns.Add("DELIVERYCOUNTRY1", GetType(String))
        retDt.Columns.Add("DELIVERYPORT1", GetType(String))

        retDt.Columns.Add("RECIEPTCOUNTRY2", GetType(String))
        retDt.Columns.Add("RECIEPTPORT2", GetType(String))
        retDt.Columns.Add("LOADCOUNTRY2", GetType(String))
        retDt.Columns.Add("LOADPORT2", GetType(String))
        retDt.Columns.Add("DISCHARGECOUNTRY2", GetType(String))
        retDt.Columns.Add("DISCHARGEPORT2", GetType(String))
        retDt.Columns.Add("DELIVERYCOUNTRY2", GetType(String))
        retDt.Columns.Add("DELIVERYPORT2", GetType(String))

        retDt.Columns.Add("VSL1", GetType(String))
        retDt.Columns.Add("VOY1", GetType(String))
        retDt.Columns.Add("ETD1", GetType(String))
        retDt.Columns.Add("ETA1", GetType(String))

        retDt.Columns.Add("VSL2", GetType(String))
        retDt.Columns.Add("VOY2", GetType(String))
        retDt.Columns.Add("ETD2", GetType(String))
        retDt.Columns.Add("ETA2", GetType(String))

        retDt.Columns.Add("PRODUCTWEIGHT", GetType(String))
        retDt.Columns.Add("GRAVITY", GetType(String))
        retDt.Columns.Add("LOADING", GetType(String))
        retDt.Columns.Add("STEAMING", GetType(String))
        retDt.Columns.Add("TIP", GetType(String))
        retDt.Columns.Add("EXTRA", GetType(String))
        retDt.Columns.Add("JOTHIREAGE", GetType(String))
        retDt.Columns.Add("COMMERCIALFACTOR", GetType(String))
        retDt.Columns.Add("AMTREQUEST", GetType(String))
        retDt.Columns.Add("AMTPRINCIPAL", GetType(String))
        retDt.Columns.Add("AMTDISCOUNT", GetType(String))
        retDt.Columns.Add("DEMURTO", GetType(String))
        retDt.Columns.Add("DEMURUSRATE1", GetType(String))
        retDt.Columns.Add("DEMURUSRATE2", GetType(String))
        '念のため
        retDt.Columns.Add("REMARK", GetType(String))
        retDt.Columns.Add("BRTYPE", GetType(String)) 'ブレーカータイプ
        retDt.Columns.Add("ISTRILATERAL", GetType(String)) '3国間輸送か "1.三国,その他.通常
        retDt.Columns.Add("TANKCAPACITY", GetType(String))
        retDt.Columns.Add("DAYSTOTAL", GetType(String))
        retDt.Columns.Add("PERDAY", GetType(String))
        retDt.Columns.Add("TOTALINVOICED", GetType(String))
        '検討中
        retDt.Columns.Add("INVOICED1BY", GetType(String))
        retDt.Columns.Add("INVOICED2BY", GetType(String))
        retDt.Columns.Add("DUMMY", GetType(String))
        retDt.Columns.Add("DUMMY2", GetType(String))
        'エージェント関係
        retDt.Columns.Add("AGENTORGANIZER", GetType(String))
        retDt.Columns.Add("AGENTPOL1", GetType(String))
        retDt.Columns.Add("AGENTPOL2", GetType(String))
        retDt.Columns.Add("AGENTPOD1", GetType(String))
        retDt.Columns.Add("AGENTPOD2", GetType(String))

        retDt.Columns.Add("FEE", GetType(String)) '手数料
        retDt.Columns.Add("BILLINGCATEGORY", GetType(String)) 'SHIPPER,CONSIGNEE判定項目
        retDt.Columns.Add("TRPBILLING", GetType(String)) 'SHIPPER,CONSIGNEE判定項目

        retDt.Columns.Add("USINGLEASETANK", GetType(String))

        Dim dr As DataRow = retDt.NewRow
        dr.Item("DUMMY") = "　"
        retDt.Rows.Add(dr)
        Return retDt
    End Function
    ''' <summary>
    ''' ブレーカー単体のInvoicedTotalを算出
    ''' </summary>
    Private Function CalcBrInvoiceTotal(drBrBase As DataRow, totalCostString As String, orgFlg As Boolean) As String

        Dim totalCost As Decimal = DecimalStringToDecimal(totalCostString)
        Dim jotHireage As Decimal = DecimalStringToDecimal(Convert.ToString(drBrBase.Item("JOTHIREAGE")))
        Dim commercialFactor As Decimal = DecimalStringToDecimal(Convert.ToString(drBrBase.Item("COMMERCIALFACTOR")))
        Dim commission As Decimal = DecimalStringToDecimal(Convert.ToString(drBrBase.Item("FEE")))

        'INVOICED TOTALを計算
        Dim invoiceTotal As Decimal
        If orgFlg Then
            invoiceTotal = jotHireage + commercialFactor + totalCost + commission
        Else
            invoiceTotal = totalCost
        End If
        invoiceTotal = RoundDown(invoiceTotal)
        Return Convert.ToString(invoiceTotal)
    End Function
    ''' <summary>
    ''' ブレーカー情報を画面に設定
    ''' </summary>
    ''' <param name="brData"></param>
    Private Sub SetDisplayBreakerData(brData As BreakerData)
        Me.txtBrId.Text = brData.BrId
        Me.txtNoOfTanks.Text = NumberFormat(brData.NoOfTanks, "0")
        Me.txtOffice.Text = brData.Office
        Me.txtSalesPic.Text = brData.SalesPic
        Me.txtFillingDate.Text = ""
        Me.txtEtd1.Text = FormatDateContrySettings(brData.BrEtd1, GBA00003UserSetting.DATEFORMAT)
        Me.txtEta1.Text = FormatDateContrySettings(brData.BrEta1, GBA00003UserSetting.DATEFORMAT)
        Me.txtEtd2.Text = FormatDateContrySettings(brData.BrEtd2, GBA00003UserSetting.DATEFORMAT)
        Me.txtEta2.Text = FormatDateContrySettings(brData.BrEta2, GBA00003UserSetting.DATEFORMAT)
        Me.txtTotalTanks.Text = ""
        Me.txtTotalInvoiced.Text = ""
        Me.txtVesselName.Text = brData.VesselName
        Me.txtVoyageNo.Text = brData.VoyageNo
        Me.txtConsignee.Text = brData.Consignee
        Me.hdnBrType.Value = brData.BrType
        Me.hdnDeliveryCountry1.Value = brData.DeliveryCountry1
        txtConsignee_Change()

        'ブレーカーで入力されていた場合、非活性
        If Me.txtConsignee.Text <> "" Then
            Me.txtConsignee.Enabled = False
        End If

    End Sub
    ''' <summary>
    ''' 画面オブジェクトの表示非表示制御
    ''' </summary>
    ''' <param name="brData"></param>
    Private Sub VisibleControls(brData As BreakerData)
        '三国間輸送の場合は表示ETA2,ETD2を表示
        Me.trEta2.Visible = brData.IsTrilateral
        Me.trEtd2.Visible = brData.IsTrilateral
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
    ''' 切り捨て関数
    ''' </summary>
    ''' <param name="value">値</param>
    ''' <param name="digits">IN：省略可能 省略時はセッション変数の対象桁数を取得</param>
    ''' <returns></returns>
    Private Function RoundDown(value As Decimal, Optional digits As Integer = Integer.MinValue) As Decimal

        If digits = Integer.MinValue Then
            digits = 2 'セッション変数の桁数
        End If
        Dim coef As Decimal = Convert.ToDecimal(System.Math.Pow(10, digits))
        If value > 0 Then
            Return System.Math.Floor(value * coef) / coef
        Else
            Return System.Math.Ceiling(value * coef) / coef
        End If
    End Function
    ''' <summary>
    ''' 書式変更関数
    ''' </summary>
    ''' <param name="value">書式を変更</param>
    ''' <param name="formatString">個別の書式がある場合は指定、未指定の場合はセッション変数の有効桁に従い小数表示を生成</param>
    ''' <returns></returns>
    Private Function NumberFormat(value As Object, Optional formatString As String = "", Optional decPlace As Integer = 0) As String
        Dim strValue As String = Convert.ToString(value)
        strValue = strValue.Trim
        '渡された項目がブランクの場合はブランクのまま返却
        If strValue = "" Then
            Return ""
        End If

        Dim decValue As Decimal
        '渡された項目が数字にならない場合は引数のまま返却
        If Decimal.TryParse(strValue, decValue) = False Then
            Return strValue
        End If
        '数値書式の生成
        Dim retFormatString As String = formatString
        If formatString = "" Then
            Dim digits As Integer = 2 'ここは本来セッション変数の桁数
            If decPlace <> 0 Then
                digits = decPlace
            End If
            If digits <= 0 Then
                retFormatString = "#,##0"
            Else
                retFormatString = "#,##0." & New String("0"c, digits)
            End If
        End If
        Return decValue.ToString(retFormatString)
    End Function
    ''' <summary>
    ''' 項目チェック処理
    ''' </summary>
    ''' <returns>メッセージ番号(正常時 C_MESSAGENO.NORMAL,異常時はそれ以外)</returns>
    Private Function CheckInput() As String
        Dim retValue As String = C_MESSAGENO.NORMAL
        '******************************
        '禁則文字置換
        '******************************
        Dim targetObjects As New List(Of TextBox) From {Me.txtCopy, Me.txtEtd1, Me.txtEta1, Me.txtEtd2, Me.txtEta2, Me.txtBookingNo, Me.txtVesselName, Me.txtVoyageNo, Me.txtConsignee}
        ChangeInvalidChar(targetObjects)
        '******************************
        '単項目チェック
        '******************************
        Dim checkSingleObjects As New Dictionary(Of String, TextBox) From {{"COPY", Me.txtCopy}, {"FILLINGDATE", Me.txtFillingDate},
                                                                     {"ETD1", Me.txtEtd1}, {"ETA1", Me.txtEta1},
                                                                     {"BOOKINGNO", Me.txtBookingNo}, {"VSL1", Me.txtVesselName},
                                                                     {"VOY1", Me.txtVoyageNo}, {"CONSIGNEE", Me.txtConsignee}}
        '三国間の場合はETD2,ETA2をチェック項目に追加
        If Me.trEtd2.Visible = True Then
            checkSingleObjects.Add("ETD2", Me.txtEtd2)
            checkSingleObjects.Add("ETA2", Me.txtEta2)
        End If
        For Each checkObj As KeyValuePair(Of String, TextBox) In checkSingleObjects
            Dim checkValue As String = ""
            If checkObj.Key = "FILLINGDATE" OrElse checkObj.Key = "ETD1" OrElse checkObj.Key = "ETA1" OrElse checkObj.Key = "ETD2" OrElse checkObj.Key = "ETA2" Then
                checkValue = FormatDateYMD(checkObj.Value.Text, GBA00003UserSetting.DATEFORMAT)
            Else
                checkValue = checkObj.Value.Text
            End If
            Dim chkSingleReturnNo As String = CheckSingle(checkObj.Key, checkValue)
            If chkSingleReturnNo <> C_MESSAGENO.NORMAL Then
                checkObj.Value.Focus()
                Return chkSingleReturnNo
            End If
        Next
        '******************************
        'コピー数0チェック
        '******************************
        If CInt(Me.txtCopy.Text) = 0 Then
            Me.txtCopy.Focus()
            Return C_MESSAGENO.INVALIDINPUT
        End If
        '******************************
        '日付前後関係チェック
        '******************************
        'リスト先頭から順に大きくなければエラーとする（歯抜けを含む空白は許容）
        Dim checkDateSpanObjects As New List(Of TextBox) From {Me.txtFillingDate, Me.txtEtd1, Me.txtEta1}
        '三国間の場合はETD2,ETA2をチェック項目に追加
        If Me.trEtd2.Visible = True Then
            checkDateSpanObjects.AddRange({Me.txtEtd2, Me.txtEta2})
        End If
        retValue = CheckDateSpan(checkDateSpanObjects)
        If retValue <> C_MESSAGENO.NORMAL Then
            Return retValue
        End If
        '******************************
        'ブレーカー有効期限vsETD1チェック
        '******************************
        Dim etd1DtmStr As String = Date.ParseExact(Me.txtEtd1.Text, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
        Dim brData As BreakerData = DirectCast(ViewState("BRDATA"), BreakerData)
        '発日付がValidityの範囲外の場合エラー
        If etd1DtmStr < brData.BrValidityFrom OrElse etd1DtmStr > brData.BrValidityTo Then
            Me.txtEtd1.Focus()
            Return C_MESSAGENO.VALIDITYINPUT
        End If
        '******************************
        '更新可能チェック
        '更新直前の申請NOに変化がないかチェック
        '******************************
        retValue = CheckUpdatable()
        Return retValue
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
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="inColName"></param>
    ''' <param name="inText"></param>
    Private Function CheckSingle(ByVal inColName As String, ByVal inText As String) As String
        Dim retValue As String = C_MESSAGENO.NORMAL
        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck      '項目チェック

        '〇単項目チェック
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELD = inColName
        COA0026FieldCheck.VALUE = inText
        COA0026FieldCheck.COA0026FieldCheck()
        If COA0026FieldCheck.ERR <> C_MESSAGENO.NORMAL Then
            CommonFunctions.ShowMessage(COA0026FieldCheck.ERR, Me.lblFooterMessage, naeiw:=C_NAEIW.ERROR)
            retValue = COA0026FieldCheck.ERR
        End If
        Return retValue
    End Function
    ''' <summary>
    ''' 日付間隔チェック
    ''' </summary>
    ''' <param name="dateObj"></param>
    ''' <returns>メッセージNo</returns>
    Private Function CheckDateSpan(dateObj As List(Of TextBox)) As String

        Dim retMessageNo As String = C_MESSAGENO.NORMAL
        Dim hasValue As Boolean = False
        Dim prevFieldtDate As Date
        Dim currentDate As Date
        For Each txtObj As TextBox In dateObj
            '空白の場合はスキップ
            If txtObj.Text.Trim = "" Then
                Continue For
            End If
            Dim dateString = Date.ParseExact(txtObj.Text, GBA00003UserSetting.DATEFORMAT, Nothing).ToString("yyyy/MM/dd")
            If hasValue = False Then
                Date.TryParse(dateString, prevFieldtDate)
                hasValue = True
                Continue For
            End If
            Date.TryParse(dateString, currentDate)

            If currentDate < prevFieldtDate Then
                retMessageNo = C_MESSAGENO.VALIDITYINPUT
                txtObj.Focus()
                Return retMessageNo
            Else
                prevFieldtDate = currentDate
            End If
        Next
        Return retMessageNo
    End Function
    ''' <summary>
    ''' 更新可能チェック
    ''' </summary>
    ''' <returns>メッセージNo</returns>
    ''' <remarks>ブレーカー紐づけ情報(GBT0001_BR_INFO)の申請IDにつき
    ''' 当画面のオープン時と直近の状態を比較、変化があれば更新不可能とする</remarks>
    Private Function CheckUpdatable() As String
        Dim brInfoWhenOpen As Dictionary(Of String, BreakerInfo) = DirectCast(ViewState("BRINFO"), Dictionary(Of String, BreakerInfo))
        Dim brInfoCurrent As Dictionary(Of String, BreakerInfo) = GetBreakerInfo(Me.txtBrId.Text)
        '論理削除し直近のレコードが取れない場合は変更有
        If brInfoCurrent Is Nothing OrElse brInfoCurrent.Count = 0 Then
            Return C_MESSAGENO.CANNOTUPDATE
        End If

        'それぞれ申請IDを比較
        For Each key As String In brInfoWhenOpen.Keys
            If brInfoCurrent.ContainsKey(key) = False Then
                'ありえないがオーダー、発・着地に違いがある場合
                Return C_MESSAGENO.CANNOTUPDATE
            End If
            If brInfoWhenOpen(key).ApplyId <> brInfoCurrent(key).ApplyId Then
                Return C_MESSAGENO.CANNOTUPDATE
            End If
        Next
        Return C_MESSAGENO.NORMAL
    End Function
    ''' <summary>
    ''' ブレーカー情報をもとにオーダーデータを作成
    ''' </summary>
    ''' <returns></returns>
    Private Function EntryNewOrder() As String

        Dim brId As String = Me.txtBrId.Text
        Using sqlCon As New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open()
            Dim brData As BreakerData = DirectCast(ViewState("BRDATA"), BreakerData)
            'ブレーカー費用項目取得(含むオーガナイザ)
            Dim costDt As DataTable = GetBreakerCostData(brId, brData, sqlCon)
            '日付項目の更新
            EditOrderCostDateFields(costDt, sqlCon)
            '新規オーダー番号生成（シーケンスより取得）
            Dim orderNo As String = GetOrderNo(sqlCon)
            '増幅数を取得
            Dim copyCnt As Integer = 1
            Integer.TryParse(Me.txtCopy.Text, copyCnt)

            Dim noOfTanks As Decimal = DecimalStringToDecimal(brData.NoOfTanks)
            If noOfTanks = 0 Then
                noOfTanks = 1
            End If
            Dim totalTanks As Decimal = copyCnt '* noOfTanks
            copyCnt = CInt(totalTanks)
            'DB登録実行
            Dim entDate As Date = Date.Now
            Dim tran As SqlTransaction = sqlCon.BeginTransaction() 'トランザクション開始
            InsertOrderBase(orderNo, brId, copyCnt, sqlCon, tran, entDate)
            InsertOrderValue(orderNo, costDt, copyCnt, brData.BrType, sqlCon, tran, entDate)
            InsertOrderValue2(orderNo, costDt, copyCnt, sqlCon, tran, entDate)
            tran.Commit()
            sqlCon.Close()
            Me.hdnOrderNo.Value = orderNo
        End Using
        Return C_MESSAGENO.NORMALDBENTRY
    End Function
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
            sqlStat.AppendLine("      + right('0000' + trim(convert(char,NEXT VALUE FOR " & C_SQLSEQ.ORDER & ")),4)")
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@CLASS", SqlDbType.NVarChar, 20).Value = C_SERVERSEQ
                    .Add("@KEYCODE", SqlDbType.NVarChar, 20).Value = HttpContext.Current.Session("APSRVname")
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
    Private Function GetBreakerCostData(breakerId As String, brData As BreakerData, Optional ByRef sqlCon As SqlConnection = Nothing) As DataTable
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
            sqlStat.AppendLine("     , VL.USD                  AS AMOUNTBR")
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
            'sqlStat.AppendLine("     , ISNULL(TRP.ACTIONID,'') AS ACTY")
            'sqlStat.AppendLine("     , ISNULL(TRP.CLASS1,'')   AS WORKOSEQ")
            'sqlStat.AppendLine("     , ISNULL(TRP.CLASS2,'')   AS DISPSEQ")
            'sqlStat.AppendLine("     , ISNULL(TRP.CLASS3,'')   AS DATEFIELD")
            'sqlStat.AppendLine("     , ISNULL(TRP.CLASS4,'')   AS DATEINTERVAL")
            'sqlStat.AppendLine("     , ISNULL(TRP.CLASS5,'')   AS LASTACT")
            sqlStat.AppendLine("     , ISNULL(VL.ACTIONID,'') AS ACTY")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS1,'')   AS WORKOSEQ")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS2,'')   AS DISPSEQ")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS3,'')   AS DATEFIELD")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS4,'')   AS DATEINTERVAL")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS5,'')   AS LASTACT")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS6,'')   AS REQUIREDACT")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS7,'')   AS ORIGINDESTINATION")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS8,'')   AS BRADDEDCOST")
            sqlStat.AppendLine("     , ISNULL(VL.CLASS9,'')   AS PERBL")
            sqlStat.AppendLine("     , ISNULL(VL.TAXATION,'') AS TAXATION")
            sqlStat.AppendLine("     , ISNULL(VL.INVOICEDBY,'')     AS INVOICEDBY")
            sqlStat.AppendLine("     , ISNULL(BS.AGENTORGANIZER,'') AS AGENTORGANIZER")
            sqlStat.AppendLine("     , CONVERT([date],'1900/01/01') AS SCHEDELDATEBR")
            'sqlStat.AppendLine("     , REPLACE(REPLACE(VL.DTLPOLPOD,'POL','001')")
            'sqlStat.AppendLine("                                   ,'POD','002') AS AGENTKBNSORT")
            sqlStat.AppendLine("     , VL.COUNTRYCODE AS COUNTRYCODE")
            sqlStat.AppendLine("     , VL.BILLING AS BILLING")
            sqlStat.AppendLine("     , ISNULL(BS.SHIPPER,'') AS SHIPPER")
            sqlStat.AppendLine("     , ISNULL(BS.CONSIGNEE,'') AS CONSIGNEE")
            sqlStat.AppendLine("     , ISNULL(BS.BILLINGCATEGORY,'') AS BILLINGCATEGORY")
            sqlStat.AppendLine("     , '' AS TRPBILLING")
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
            'sqlStat.AppendLine("  LEFT JOIN GBM0009_TRPATTERN TRP")
            'sqlStat.AppendLine("    ON TRP.COMPCODE   = @COMPCODE")
            'sqlStat.AppendLine("   AND TRP.ORG        = @ORG")
            'sqlStat.AppendLine("   AND TRP.BRTYPE     = INF.BRTYPE")
            'sqlStat.AppendLine("   AND TRP.USETYPE    = INF.USETYPE")
            'sqlStat.AppendLine("   AND TRP.AGENTKBN   = VL.DTLPOLPOD")
            'sqlStat.AppendLine("   AND TRP.COSTCODE   = VL.COSTCODE")
            'sqlStat.AppendLine("   AND TRP.STYMD     <= INF.ENDYMD")
            'sqlStat.AppendLine("   AND TRP.ENDYMD    >= INF.STYMD")
            'sqlStat.AppendLine("   AND TRP.DELFLG    <> @DELFLG")
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
            sqlStat.AppendLine("     , '" & GBC_CUR_USD & "'        AS CURRENCYCODE")
            sqlStat.AppendLine("     , 0                            AS AMOUNTBR")
            sqlStat.AppendLine("     , 0                            AS LOCALBR")
            sqlStat.AppendLine("     , ISNULL(EX.EXRATE,0)          AS LOCALRATE")
            sqlStat.AppendLine("     , 0                            AS TAXBR")
            'sqlStat.AppendLine("     , ''                           AS CONTRACTORBR")
            sqlStat.AppendLine("     , ISNULL(TRP.INITCONTRACTOR,'') AS CONTRACTORBR")
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
            sqlStat.AppendLine("     , '2'                          AS BRADDEDCOST")
            sqlStat.AppendLine("     , ''                           AS PERBL")
            sqlStat.AppendLine("     , '0'                          AS TAXATION") '課税フラグオーガナイザー一旦は0固定
            sqlStat.AppendLine("     , ISNULL(BS.INVOICEDBY,'')     AS INVOICEDBY")
            sqlStat.AppendLine("     , ISNULL(BS.AGENTORGANIZER,'') AS AGENTORGANIZER")
            'sqlStat.AppendLine("     , '0000'                       AS AGENTKBNSORT")
            sqlStat.AppendLine("     , CONVERT([date],'1900/01/01') AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , BS.COUNTRYORGANIZER          AS COUNTRYCODE")
            sqlStat.AppendLine("     ,''                            AS BILLING ")
            sqlStat.AppendLine("     , ISNULL(BS.SHIPPER,'') AS SHIPPER")
            sqlStat.AppendLine("     , ISNULL(BS.CONSIGNEE,'') AS CONSIGNEE")
            sqlStat.AppendLine("     , ISNULL(BS.BILLINGCATEGORY,'') AS BILLINGCATEGORY")
            sqlStat.AppendLine("     , '' AS TRPBILLING")
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
            sqlStat.AppendLine("LEFT JOIN GBM0020_EXRATE EX ")
            sqlStat.AppendLine("       ON EX.COMPCODE     = @COMPCODE ")
            sqlStat.AppendLine("      AND EX.COUNTRYCODE  = BS.COUNTRYORGANIZER ")
            sqlStat.AppendLine("      AND EX.TARGETYM     = @TARGETYM ")
            sqlStat.AppendLine("      AND EX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE INF.BRID      = @BRID")
            sqlStat.AppendLine("   AND INF.DELFLG   <> @DELFLG")
            sqlStat.AppendLine("   AND INF.TYPE      = 'INFO' ")

            'デマレッジレコード
            sqlStat.AppendLine(" UNION ALL ")
            sqlStat.AppendLine("SELECT INF.BRID")
            sqlStat.AppendLine("     , ISNULL(INF.USETYPE,'')       AS USETYPE")
            sqlStat.AppendLine("     , TRP.AGENTKBN                 AS AGENTKBN")
            sqlStat.AppendLine("     , TRP.COSTCODE                 AS COSTCODE")
            sqlStat.AppendLine("     , '" & GBC_CUR_USD & "'        AS CURRENCYCODE")
            sqlStat.AppendLine("     , 0                            AS AMOUNTBR")
            sqlStat.AppendLine("     , 0                            AS LOCALBR")
            sqlStat.AppendLine("     , ISNULL(EX.EXRATE,0)          AS LOCALRATE")
            sqlStat.AppendLine("     , 0                            AS TAXBR")
            'sqlStat.AppendLine("     , ''                           AS CONTRACTORBR")
            sqlStat.AppendLine("     , ISNULL(TRP.INITCONTRACTOR,'') AS CONTRACTORBR")
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
            sqlStat.AppendLine("     , ''                           AS BRADDEDCOST")
            sqlStat.AppendLine("     , ''                           AS PERBL")
            sqlStat.AppendLine("     , '0'                          AS TAXATION") '課税フラグ デマレコード一旦は0固定
            'sqlStat.AppendLine("     , ISNULL(BS.INVOICEDBY,'')     AS INVOICEDBY")
            sqlStat.AppendLine("     , ISNULL((CASE TRP.AGENTKBN WHEN 'POL1' THEN BS.AGENTPOL1 ")
            sqlStat.AppendLine("                                 WHEN 'POL2' THEN BS.AGENTPOL2 ")
            sqlStat.AppendLine("                                 WHEN 'POD1' THEN BS.AGENTPOD1 ")
            sqlStat.AppendLine("                                 WHEN 'POD2' THEN BS.AGENTPOD2 ")
            sqlStat.AppendLine("                                 ELSE '' END ")
            sqlStat.AppendLine("             ),'')             AS INVOICEDBY ")
            sqlStat.AppendLine("     , ISNULL(BS.AGENTORGANIZER,'') AS AGENTORGANIZER")
            'sqlStat.AppendLine("     , '0000'                       AS AGENTKBNSORT")
            sqlStat.AppendLine("     , CONVERT([date],'1900/01/01') AS SCHEDELDATEBR")
            sqlStat.AppendLine("     , ISNULL((CASE TRP.AGENTKBN WHEN 'POL1' THEN BS.LOADCOUNTRY1 ")
            sqlStat.AppendLine("                                 WHEN 'POL2' THEN BS.LOADCOUNTRY2 ")
            sqlStat.AppendLine("                                 WHEN 'POD1' THEN BS.DISCHARGECOUNTRY1 ")
            sqlStat.AppendLine("                                 WHEN 'POD2' THEN BS.DISCHARGECOUNTRY2 ")
            sqlStat.AppendLine("                                 ELSE '' END ")
            sqlStat.AppendLine("             ),'')                  AS COUNTRYCODE ")
            sqlStat.AppendLine("     ,''                            AS BILLING ")
            sqlStat.AppendLine("     , ISNULL(BS.SHIPPER,'') AS SHIPPER")
            sqlStat.AppendLine("     , ISNULL(BS.CONSIGNEE,'') AS CONSIGNEE")
            sqlStat.AppendLine("     , ISNULL(BS.BILLINGCATEGORY,'') AS BILLINGCATEGORY")
            sqlStat.AppendLine("     , '' AS TRPBILLING")
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
            sqlStat.AppendLine("LEFT JOIN GBM0020_EXRATE EX ")
            sqlStat.AppendLine("       ON EX.COMPCODE     = @COMPCODE ")
            sqlStat.AppendLine("      AND EX.COUNTRYCODE  = (CASE TRP.AGENTKBN WHEN 'POL1' THEN BS.LOADCOUNTRY1 ")
            sqlStat.AppendLine("                                               WHEN 'POL2' THEN BS.LOADCOUNTRY2 ")
            sqlStat.AppendLine("                                               WHEN 'POD1' THEN BS.DISCHARGECOUNTRY1 ")
            sqlStat.AppendLine("                                               WHEN 'POD2' THEN BS.DISCHARGECOUNTRY2 ")
            sqlStat.AppendLine("                                               ELSE '' END)")
            sqlStat.AppendLine("      AND EX.TARGETYM     = @TARGETYM ")
            sqlStat.AppendLine("      AND EX.DELFLG      <> @DELFLG")
            sqlStat.AppendLine(" WHERE INF.BRID      = @BRID")
            sqlStat.AppendLine("   AND INF.DELFLG   <> @DELFLG")
            'DB接続
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                'SQLパラメータ設定
                With sqlCmd.Parameters
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                    .Add("@ORG", SqlDbType.NVarChar, 20).Value = "GB_Default"
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = GBC_COMPCODE
                    .Add("@FIXVALCOMPCODE", SqlDbType.NVarChar).Value = GBC_COMPCODE_D
                    .Add("@FIXVALSYSCODE", SqlDbType.NVarChar).Value = C_SYSCODE_GB
                    .Add("@FIXVALCLASS", SqlDbType.NVarChar).Value = C_FIXVALUECLAS.BREX
                    .Add("@TARGETYM", SqlDbType.Date).Value = Now.ToString("yyyy/MM") & "/01"
                End With

                Using sqlDa As New SqlDataAdapter(sqlCmd)
                    sqlDa.Fill(dtDbResult)
                End Using
            End Using

            '着発区分設定
            Dim findRow As DataRow = Nothing
            findRow = (From item In dtDbResult
                       Where Convert.ToString(item("AGENTKBN")) = CONST_ORGANIZER _
                        AndAlso Convert.ToString(item("COSTCODE")) = GBC_COSTCODE_SALES).FirstOrDefault

            If findRow IsNot Nothing Then
                findRow.Item("TRPBILLING") = CONST_SHIPPERCLASS
            End If

            '売上項目追加
            Dim qcopyDt = (From dr As DataRow In dtDbResult
                           Where Convert.ToString(dr.Item("AGENTKBN")) = CONST_ORGANIZER _
                         AndAlso Convert.ToString(dr.Item("COSTCODE")) = GBC_COSTCODE_SALES
                           Select dr)

            Dim copyDt As DataTable = Nothing
            If qcopyDt.Any Then
                copyDt = qcopyDt.CopyToDataTable
            End If
            Dim findRow2 As DataRow = Nothing
            If copyDt IsNot Nothing AndAlso copyDt.Rows.Count > 0 Then
                'TOTAL INVOICE(CONSIGNEE側増幅)
                Dim dtRow As DataRow = dtDbResult.NewRow
                dtRow.ItemArray = copyDt.Rows(0).ItemArray
                dtRow.Item("TRPBILLING") = CONSIGNEECLASS
                dtDbResult.Rows.Add(dtRow)
                '元受け輸送収入のレコード増幅
                Dim drM As DataRow = dtDbResult.NewRow
                drM.ItemArray = copyDt.Rows(0).ItemArray
                drM("COSTCODE") = GBC_COSTCODE_FREIGHT_REVENUE
                drM("AMOUNTBR") = "0"
                drM("TRPBILLING") = ""
                'オーガナイザーレコードの数字項目を埋める
                If brData IsNot Nothing Then
                    'drM("AMOUNTBR") = DecimalStringToDecimal(brData.BrTotalCost) + DecimalStringToDecimal(brData.BrCommission)
                    '仮計上分を考慮
                    drM("AMOUNTBR") = DecimalStringToDecimal(brData.BrTotalCost) + DecimalStringToDecimal(brData.BrCommission) - DecimalStringToDecimal(brData.BrTotalProvisionalCost)
                End If
                dtDbResult.Rows.Add(drM)
                '元受け輸送収入のレコード増幅(仮計上分)
                If DecimalStringToDecimal(brData.BrTotalProvisionalCost) <> 0 Then
                    Dim drMp As DataRow = dtDbResult.NewRow
                    drMp.ItemArray = copyDt.Rows(0).ItemArray
                    drMp("COSTCODE") = GBC_COSTCODE_PROVISIONAL
                    drMp("AMOUNTBR") = "0"
                    drMp("TRPBILLING") = ""
                    drMp("REMARK") = "Provisional Cost"
                    'オーガナイザーレコードの数字項目を埋める
                    If brData IsNot Nothing Then
                        drMp("AMOUNTBR") = DecimalStringToDecimal(brData.BrTotalProvisionalCost)
                    End If
                    dtDbResult.Rows.Add(drMp)
                End If
                '↓DISCOUNTRATEのレコード増幅(S0101-02) 20190712
                If brData.BrAmtDiscount <> "" AndAlso IsNumeric(brData.BrAmtDiscount) AndAlso brData.BrAmtDiscount.Trim <> "0" Then
                    Dim drDiscount As DataRow = dtDbResult.NewRow
                    drDiscount.ItemArray = copyDt.Rows(0).ItemArray
                    drDiscount("COSTCODE") = GBC_COSTCODE_JOTHIRAGEA
                    drDiscount("AMOUNTBR") = brData.BrAmtDiscount
                    drDiscount("TRPBILLING") = ""
                    drDiscount("REMARK") = "Amount Discount"
                    dtDbResult.Rows.Add(drDiscount)
                End If
                '↑DISCOUNTRATEのレコード増幅(S0101-02) 20190712
            End If

            'オーガナイザーレコードの数字項目を埋める
            If brData IsNot Nothing Then
                Dim dicOrganizerCost As Dictionary(Of String, String) = Nothing

                If Not {"", "0"}.Contains(brData.BrAmtPrincipal) Then

                    Dim shipperCostTotal As Decimal = DecimalStringToDecimal(brData.BrShipperCostTotal)
                    Dim consigneeCostTotal As Decimal = DecimalStringToDecimal(brData.BrConsigneeCostTotal)
                    Dim amtPrincipal As Decimal = DecimalStringToDecimal(brData.BrAmtPrincipal)
                    Dim shipperInvoiceTotal As Decimal
                    Dim consigneeInvoiceTotal As Decimal

                    shipperInvoiceTotal = amtPrincipal - consigneeCostTotal
                    shipperInvoiceTotal = RoundDown(shipperInvoiceTotal)

                    consigneeInvoiceTotal = amtPrincipal - shipperCostTotal
                    consigneeInvoiceTotal = RoundDown(consigneeInvoiceTotal)

                    'Principal
                    dicOrganizerCost = New Dictionary(Of String, String) From {{GBC_DELIVERYCLASS.SHIPPER, Convert.ToString(shipperInvoiceTotal)},
                                                                                {GBC_DELIVERYCLASS.CONSIGNEE, Convert.ToString(consigneeCostTotal)},
                                                                                {GBC_COSTCODE_JOTHIRAGE, brData.BrHireage},
                                                                                {GBC_COSTCODE_JOTHIRAGEA, brData.BrAdjustment},
                                                                                {GBC_COSTCODE_AGENTCOM, brData.BrCommission}}

                    For Each orgCost As KeyValuePair(Of String, String) In dicOrganizerCost

                        Dim findResult As DataRow = Nothing

                        If orgCost.Key = GBC_DELIVERYCLASS.SHIPPER Then

                            findResult = (From item In dtDbResult
                                          Where Convert.ToString(item("AGENTKBN")) = CONST_ORGANIZER _
                                      AndAlso Convert.ToString(item("COSTCODE")) = GBC_COSTCODE_SALES _
                                      AndAlso Convert.ToString(item("TRPBILLING")) = CONST_SHIPPERCLASS).FirstOrDefault

                        ElseIf orgCost.Key = GBC_DELIVERYCLASS.CONSIGNEE Then

                            findResult = (From item In dtDbResult
                                          Where Convert.ToString(item("AGENTKBN")) = CONST_ORGANIZER _
                                      AndAlso Convert.ToString(item("COSTCODE")) = GBC_COSTCODE_SALES _
                                      AndAlso Convert.ToString(item("TRPBILLING")) = CONSIGNEECLASS).FirstOrDefault

                        Else

                            findResult = (From item In dtDbResult
                                          Where Convert.ToString(item("AGENTKBN")) = CONST_ORGANIZER _
                                      AndAlso Convert.ToString(item("COSTCODE")) = orgCost.Key).FirstOrDefault

                        End If

                        If findResult IsNot Nothing Then
                            findResult.Item("AMOUNTBR") = orgCost.Value
                        End If
                    Next

                Else
                    'Shipper,Consignee分割
                    dicOrganizerCost = New Dictionary(Of String, String) From {{GBC_DELIVERYCLASS.SHIPPER, brData.BrShipperTotalInvoiced},
                                                                                {GBC_DELIVERYCLASS.CONSIGNEE, brData.BrConsigneeTotalInvoiced},
                                                                                {GBC_COSTCODE_JOTHIRAGE, brData.BrHireage},
                                                                                {GBC_COSTCODE_JOTHIRAGEA, brData.BrAdjustment},
                                                                                {GBC_COSTCODE_AGENTCOM, brData.BrCommission}}

                    For Each orgCost As KeyValuePair(Of String, String) In dicOrganizerCost

                        Dim findResult As DataRow = Nothing

                        If orgCost.Key = GBC_DELIVERYCLASS.SHIPPER Then

                            findResult = (From item In dtDbResult
                                          Where Convert.ToString(item("AGENTKBN")) = CONST_ORGANIZER _
                                      AndAlso Convert.ToString(item("COSTCODE")) = GBC_COSTCODE_SALES _
                                      AndAlso Convert.ToString(item("TRPBILLING")) = CONST_SHIPPERCLASS).FirstOrDefault

                        ElseIf orgCost.Key = GBC_DELIVERYCLASS.CONSIGNEE Then

                            findResult = (From item In dtDbResult
                                          Where Convert.ToString(item("AGENTKBN")) = CONST_ORGANIZER _
                                      AndAlso Convert.ToString(item("COSTCODE")) = GBC_COSTCODE_SALES _
                                      AndAlso Convert.ToString(item("TRPBILLING")) = CONSIGNEECLASS).FirstOrDefault

                        Else

                            findResult = (From item In dtDbResult
                                          Where Convert.ToString(item("AGENTKBN")) = CONST_ORGANIZER _
                                      AndAlso Convert.ToString(item("COSTCODE")) = orgCost.Key).FirstOrDefault

                        End If

                        If findResult IsNot Nothing Then
                            findResult.Item("AMOUNTBR") = orgCost.Value
                        End If
                    Next

                End If

            End If

            '増幅した結果の費用コード（売上総額:TotalInvoice）が0の場合はレコード作らない（データテーブルより削除）
            Dim qTotalInvoiceAmount0 = From item In dtDbResult
                                       Where Convert.ToString(item("AGENTKBN")) = CONST_ORGANIZER _
                                      AndAlso Convert.ToString(item("COSTCODE")) = GBC_COSTCODE_SALES _
                                      AndAlso Convert.ToString(item("AMOUNTBR")) = "0"
                                       Order By dtDbResult.Rows.IndexOf(item) Descending
                                       Select dtDbResult.Rows.IndexOf(item)

            If qTotalInvoiceAmount0.Any Then

                For Each item In qTotalInvoiceAmount0
                    dtDbResult.Rows.RemoveAt(item)
                Next
            End If
            '輸送パターン重複考慮
            '重複した費用コードの輸送パターン付帯情報を除去

            '重複キー項目を取得
            'Dim dupulicateKeys = (From drItem In dtDbResult
            '                      Where Convert.ToString(drItem.Item("AGENTKBN")) <> CONST_ORGANIZER
            '                      Group By agentkbn = Convert.ToString(drItem.Item("AGENTKBN")), costcode = Convert.ToString(drItem.Item("COSTCODE"))
            '                      Into cnt = Count()
            '                      Where cnt > 1
            '                      )
            ''重複キーをループ
            'For Each dupulicateKey In dupulicateKeys
            '    '重複キーのデータ行を取得
            '    Dim dupulicateDrArray = (From drItem As DataRow In dtDbResult
            '                             Where Convert.ToString(drItem.Item("AGENTKBN")) = dupulicateKey.agentkbn _
            '                            AndAlso Convert.ToString(drItem.Item("COSTCODE")) = dupulicateKey.costcode
            '                             )

            '    '先頭行の除く輸送パターン情報をクリア(添え字(index 0は先頭のため1からスタート)
            '    For i As Integer = 1 To dupulicateDrArray.Count - 1
            '        dupulicateDrArray(i).Item("ACTY") = ""
            '        dupulicateDrArray(i).Item("WORKOSEQ") = ""
            '        dupulicateDrArray(i).Item("DISPSEQ") = ""
            '        dupulicateDrArray(i).Item("DATEFIELD") = ""
            '        dupulicateDrArray(i).Item("LASTACT") = ""
            '    Next
            'Next

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
    ''' <summary>
    ''' ブレーカー費用一覧の日付情報を画面の入力日付で埋める
    ''' </summary>
    ''' <param name="dt">[IN/OUT]費用データテーブル</param>
    ''' <remarks></remarks>
    Private Sub EditOrderCostDateFields(ByRef dt As DataTable, sqlCon As SqlConnection)
        '費用項目の（輸送パターン 予定日付参照[CLASS3]）が設定されているフィールドにつき
        '当画面で入力した各日付を展開
        Dim updateRows = (From drItem In dt
                          Where Convert.ToString(drItem.Item("DATEFIELD")) <> ""
                           )

        '日付項目転送対象行を設定
        Dim fillingDate As Date
        Dim eta1 As Date, etd1 As Date
        Dim eta2 As Date, etd2 As Date
        If Date.TryParseExact(Me.txtFillingDate.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, fillingDate) = False Then
            fillingDate = Date.Parse("1900/01/01")
        End If
        If Date.TryParseExact(Me.txtEta1.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, eta1) = False Then
            eta1 = Date.Parse("1900/01/01")
        End If
        If Date.TryParseExact(Me.txtEtd1.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etd1) = False Then
            etd1 = Date.Parse("1900/01/01")
        End If
        If Date.TryParseExact(Me.txtEta2.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, eta2) = False Then
            eta2 = Date.Parse("1900/01/01")
        End If
        If Date.TryParseExact(Me.txtEtd2.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etd2) = False Then
            etd2 = Date.Parse("1900/01/01")
        End If

        Dim updateDateVal As DateTime
        Dim dateInterVal As Integer
        For Each updateRow As DataRow In updateRows
            updateDateVal = DateTime.Parse("1900/01/01")
            dateInterVal = 0
            If Convert.ToString(updateRow.Item("DATEINTERVAL")) = "" _
               OrElse Integer.TryParse(Convert.ToString(updateRow.Item("DATEINTERVAL")), dateInterVal) = False Then
                Continue For
            End If
            Select Case Convert.ToString(updateRow.Item("DATEFIELD"))
                Case "FillingDate"
                    updateDateVal = fillingDate
                Case "ETA1"
                    updateDateVal = eta1
                Case "ETD1"
                    updateDateVal = etd1
                Case "ETA2"
                    updateDateVal = eta2
                Case "ETD2"
                    updateDateVal = etd2
                '↓本来このケースはいらない
                Case "ETA"
                    updateDateVal = eta1
                Case "ETD"
                    updateDateVal = etd1
            End Select

            If updateDateVal.ToString("yyyy/MM/dd") = "1900/01/01" Then
                Continue For
            End If

            updateRow.Item("SCHEDELDATEBR") = updateDateVal.AddDays(dateInterVal)
        Next
        'ACTYに対応した費目の日付項目を自動展開
        Dim actyRows = (From drItem In dt
                        Where Convert.ToString(drItem.Item("ACTY")) <> "" _
                     AndAlso Convert.ToString(drItem.Item("AGENTKBN")) <> CONST_ORGANIZER _
                     AndAlso Convert.ToString(drItem.Item("SCHEDELDATEBR")) <> "" _
                     AndAlso CDate(Convert.ToString(drItem.Item("SCHEDELDATEBR"))).ToString("yyyy/MM/dd") <> "1900/01/01")

        For Each actyRow In actyRows
            Dim actyNo As String = Convert.ToString(actyRow.Item("ACTY"))
            Dim polPodString As String = Convert.ToString(actyRow.Item("AGENTKBN"))
            Dim polPod As String = "POL"
            If polPodString.StartsWith("POD") Then
                polPod = "POD"
            End If
            Dim trans As String = "1" '第一輸送
            If polPodString.EndsWith("2") Then
                trans = "2"
            End If

            Dim targetCostCode As List(Of CostActy) = GetIntarlockCostCodeFromActy(actyNo, polPod, sqlCon)
            If targetCostCode Is Nothing OrElse targetCostCode.Count = 0 Then
                Continue For
            End If
            'Dim targetRows = (From drItem In dt
            '                  Where polPod.Contains(Convert.ToString(drItem.Item("AGENTKBN"))) _
            '                     AndAlso targetCostCode.Contains(Convert.ToString(drItem.Item("COSTCODE")))
            '                )
            Dim targetRows = (From drItem In dt
                              Where Convert.ToString(drItem.Item("AGENTKBN")).EndsWith(trans) _
                                 AndAlso targetCostCode.Any(Function(cItem) cItem.CostCode = Convert.ToString(drItem.Item("COSTCODE")) _
                                                            AndAlso ((Convert.ToString(drItem.Item("AGENTKBN")).StartsWith("PO" & cItem.LdKbn)) _
                                                                OrElse cItem.LdKbn = "B"))
                             )
            For Each targetItem In targetRows
                targetItem.Item("SCHEDELDATEBR") = actyRow.Item("SCHEDELDATEBR")
            Next
        Next
    End Sub
    Private Sub InsertOrderBase(orderNo As String, breakerId As String, copyNum As Integer, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
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
        COA0035Convert.I_CONVERT = Convert.ToString(copyNum)
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

        sqlStat.AppendLine("      ,USINGLEASETANK")

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
        sqlStat.AppendLine("      ,BOOKINGNO")
        sqlStat.AppendLine("      ,NOOFPACKAGE")

        sqlStat.AppendLine("      ,SHIPPERTEXT2")
        sqlStat.AppendLine("      ,CONSIGNEETEXT2")
        sqlStat.AppendLine("      ,NOTIFYTEXT2")

        sqlStat.AppendLine("      ,LDNVSL1")
        sqlStat.AppendLine("      ,LDNPOL1")
        sqlStat.AppendLine("      ,LDNDATE1")
        sqlStat.AppendLine("      ,LDNBY1")
        sqlStat.AppendLine("      ,LDNVSL2")
        sqlStat.AppendLine("      ,LDNPOL2")
        sqlStat.AppendLine("      ,LDNDATE2")
        sqlStat.AppendLine("      ,LDNBY2")

        sqlStat.AppendLine("      ,BLRECEIPT1")
        sqlStat.AppendLine("      ,BLRECEIPT2")
        sqlStat.AppendLine("      ,BLLOADING1")
        sqlStat.AppendLine("      ,BLLOADING2")
        sqlStat.AppendLine("      ,BLDISCHARGE1")
        sqlStat.AppendLine("      ,BLDISCHARGE2")
        sqlStat.AppendLine("      ,BLDELIVERY1")
        sqlStat.AppendLine("      ,BLDELIVERY2")
        sqlStat.AppendLine("      ,BLPLACEDATEISSUE1")
        sqlStat.AppendLine("      ,BLPLACEDATEISSUE2")

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
        sqlStat.AppendLine("      ,@CONSIGNEE")
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
        'sqlStat.AppendLine("      ,BB.VSL1")
        'sqlStat.AppendLine("      ,BB.VOY1")
        sqlStat.AppendLine("      ,@VSL1")
        sqlStat.AppendLine("      ,@VOY1")
        sqlStat.AppendLine("      ,CASE WHEN BB.ETD1=@INITDATE THEN @ETD1 ELSE BB.ETD1 END")
        sqlStat.AppendLine("      ,CASE WHEN BB.ETA1=@INITDATE THEN @ETA1 ELSE BB.ETA1 END")
        sqlStat.AppendLine("      ,BB.VSL2")
        sqlStat.AppendLine("      ,BB.VOY2")
        sqlStat.AppendLine("      ,CASE WHEN BB.ETD2=@INITDATE THEN @ETD2 ELSE BB.ETD2 END")
        sqlStat.AppendLine("      ,CASE WHEN BB.ETA2=@INITDATE THEN @ETA2 ELSE BB.ETA2 END")
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

        sqlStat.AppendLine("      ,BB.USINGLEASETANK")

        sqlStat.AppendFormat("      ,ISNULL(SP.{0} + CHAR(13) + CHAR(10) + SP.ADDR,'') AS SHIPPERTEXT ", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CS.{0} + CHAR(13) + CHAR(10) + CS.ADDR,'') AS CONSIGNEETEXT", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(CS.{0} + CHAR(13) + CHAR(10) + CS.ADDR,'') AS NOTIFYTEXT", textCustomerTblField).AppendLine()
        sqlStat.AppendLine("      ,ISNULL(TR1.NAMEL + CHAR(13) + CHAR(10) + TR1.ADDR + CHAR(13) + CHAR(10) + CASE WHEN TR1.TEL = '' THEN '' ELSE 'TEL:' + TR1.TEL + ' ' END + CASE WHEN TR1.FAX = '' THEN '' ELSE 'FAX:' + TR1.FAX END ,'') AS NOTIFYCONTTEXT1")
        sqlStat.AppendLine("      ,ISNULL(TR2.NAMEL + CHAR(13) + CHAR(10) + TR2.ADDR + CHAR(13) + CHAR(10) + CASE WHEN TR2.TEL = '' THEN '' ELSE 'TEL:' + TR2.TEL + ' ' END + CASE WHEN TR2.FAX = '' THEN '' ELSE 'FAX:' + TR2.FAX END ,'') AS NOTIFYCONTTEXT2")

        'sqlStat.AppendLine("      ,ISNULL(SP.CITY,'') AS PREPAIDAT")
        sqlStat.AppendLine("      ,CASE WHEN BB.BILLINGCATEGORY = 'SHIPPER' THEN ISNULL(ORGL1.NAMEL,'') ELSE '' END AS PREPAIDAT")
        sqlStat.AppendLine("      ,ISNULL(ER.EXRATE,'') AS EXCHANGERATE")
        sqlStat.AppendLine("      ,ISNULL(ER.CURRENCYCODE,'') AS LOCALCURRENCY")
        'sqlStat.AppendLine("      ,ISNULL(CS.CITY,'') AS PAYABLEAT")
        sqlStat.AppendLine("      ,CASE WHEN BB.BILLINGCATEGORY = 'SHIPPER' THEN '' ELSE ISNULL(ORGD1.NAMEL,'') END AS PAYABLEAT")

        sqlStat.AppendLine("      ,CASE WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY1 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY1 THEN '""FREIGHT COLLECT""' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY2 THEN '""FREIGHT PREPAID"" AS ARRANGED' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY2 THEN '""FREIGHT COLLECT""' ")
        sqlStat.AppendLine("      ELSE '' END AS FREIGHTANDCHARGES")
        'sqlStat.AppendLine("      ,ISNULL(FV1.VALUE1 + CASE WHEN PD.PRODUCTNAME IS NULL THEN '' ELSE CHAR(10) + CHAR(10) + TRIM(PD.PRODUCTNAME) + CHAR(10) + 'PO# 1013/' + CONVERT(nvarchar,YEAR(GETDATE())) END + CHAR(10) + CHAR(10) + CONVERT(nvarchar,BB.TIP) + @DAYSTEXT + CHAR(10) + CHAR(10) ")
        sqlStat.AppendLine("      ,ISNULL(FV1.VALUE1 + CASE WHEN PD.PRODUCTNAME IS NULL THEN '' ELSE CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) + TRIM(PD.PRODUCTNAME) END + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) + CONVERT(nvarchar,BB.TIP) + @DAYSTEXT + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10) ")
        sqlStat.AppendLine("      + CASE WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY1 THEN '""FREIGHT PREPAID""' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY1 THEN '""FREIGHT COLLECT""' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.RECIEPTCOUNTRY2 THEN '""FREIGHT PREPAID""' ")
        sqlStat.AppendLine("            WHEN TR3.COUNTRYCODE = BB.DISCHARGECOUNTRY2 THEN '""FREIGHT COLLECT""' ")
        sqlStat.AppendLine("      ELSE '' END ,'') AS GOODSPKGS")
        sqlStat.AppendLine("      ,@CONTAINERPKGS")
        sqlStat.AppendLine("      ,@BOOKINGNO")
        sqlStat.AppendLine("      ,@NOOFPACKAGE")

        sqlStat.AppendFormat("      ,ISNULL(CS.{0} + CHAR(13) + CHAR(10) + CS.ADDR,'') AS SHIPPERTEXT2", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(SP.{0} + CHAR(13) + CHAR(10) + SP.ADDR,'') AS CONSIGNEETEXT2", textCustomerTblField).AppendLine()
        sqlStat.AppendFormat("      ,ISNULL(SP.{0} + CHAR(13) + CHAR(10) + SP.ADDR,'') AS NOTIFYTEXT2", textCustomerTblField).AppendLine()

        sqlStat.AppendLine("      ,@VSL1")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT1.AREANAME,'') = '' THEN '' ELSE PT1.AREANAME + ', ' END + ISNULL(CT1.NAMES,'') ")
        sqlStat.AppendLine("      ,CASE WHEN BB.ETD1=@INITDATE THEN @ETD1 ELSE BB.ETD1 END")
        sqlStat.AppendFormat("      ,ISNULL(SP.{0},'')", textCustomerTblField).AppendLine()

        sqlStat.AppendLine("      ,BB.VSL2")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT2.AREANAME,'') = '' THEN '' ELSE PT2.AREANAME + ', ' END + ISNULL(CT2.NAMES,'') ")
        sqlStat.AppendLine("      ,CASE WHEN BB.ETD2=@INITDATE THEN @ETD2 ELSE BB.ETD2 END")
        sqlStat.AppendFormat("      ,ISNULL(CS.{0},'')", textCustomerTblField).AppendLine()

        sqlStat.AppendLine("      ,ISNULL(PT3.AREANAME,'') + ' ' + FV2.VALUE3 AS BLRECEIPT1")
        sqlStat.AppendLine("      ,ISNULL(PT4.AREANAME,'') + ' ' + FV2.VALUE3 AS BLRECEIPT2")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT1.AREANAME,'') = '' THEN '' ELSE ISNULL(PT1.AREANAME,'') + ', ' + ISNULL(CT1.NAMES,'') END AS BLLOADING1")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT2.AREANAME,'') = '' THEN '' ELSE ISNULL(PT2.AREANAME,'') + ', ' + ISNULL(CT2.NAMES,'') END AS BLLOADING2")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT5.AREANAME,'') = '' THEN '' ELSE ISNULL(PT5.AREANAME,'') + ', ' + ISNULL(CT5.NAMES,'') END AS BLDISCHARGE1")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT6.AREANAME,'') = '' THEN '' ELSE ISNULL(PT6.AREANAME,'') + ', ' + ISNULL(CT6.NAMES,'') END AS BLDISCHARGE2")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT7.AREANAME,'') = '' THEN '' ELSE ISNULL(PT7.AREANAME,'') + ', ' + ISNULL(CT7.NAMES,'') END AS BLDELIVERY1")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(PT8.AREANAME,'') = '' THEN '' ELSE ISNULL(PT8.AREANAME,'') + ', ' + ISNULL(CT8.NAMES,'') END AS BLDELIVERY2")
        'sqlStat.AppendLine("      ,CASE WHEN ISNULL(SP.CITY,'') = '' THEN '' ELSE ISNULL(SP.CITY,'') + ' : ' + FORMAT(BB.ETD1,'yyyy-MM-dd') END AS BLPLACEDATEISSUE1")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(ORGL1.NAMEL,'') = '' THEN '' ELSE ISNULL(ORGL1.NAMEL,'') + ' : ' + FORMAT(@ETD1,'yyyy-MM-dd') END AS BLPLACEDATEISSUE1")
        'sqlStat.AppendLine("      ,CASE WHEN ISNULL(SP.CITY,'') = '' THEN '' ELSE ISNULL(SP.CITY,'') + ' : ' + FORMAT(BB.ETD2,'yyyy-MM-dd') END AS BLPLACEDATEISSUE2")
        sqlStat.AppendLine("      ,CASE WHEN ISNULL(ORGL2.NAMEL,'') = '' THEN '' ELSE ISNULL(ORGL2.NAMEL,'') + ' : ' + FORMAT(@ETD2,'yyyy-MM-dd') END AS BLPLACEDATEISSUE2")

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
        sqlStat.AppendLine("   AND CS.CUSTOMERCODE = @CONSIGNEE")
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
        sqlStat.AppendLine("  LEFT JOIN COS0021_ORG ORGD1") 'POD1 NAMEL
        sqlStat.AppendLine("    ON ORGD1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND ORGD1.ORGCODE      = TR1.MORG")
        sqlStat.AppendLine("   AND ORGD1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ORGD1.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND ORGD1.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TR2") 'Party to Contact2
        sqlStat.AppendLine("    ON TR2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TR2.CARRIERCODE  = BB.AGENTPOD2")
        sqlStat.AppendLine("   AND TR2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TR2.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TR2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0021_ORG ORGD2") 'POD2 NAMEL
        sqlStat.AppendLine("    ON ORGD2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND ORGD2.ORGCODE      = TR2.MORG")
        sqlStat.AppendLine("   AND ORGD2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ORGD2.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND ORGD2.DELFLG      <> @DELFLG")

        'POL1
        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRL1") 'Party to Contact1
        sqlStat.AppendLine("    ON TRL1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TRL1.CARRIERCODE  = BB.AGENTPOL1")
        sqlStat.AppendLine("   AND TRL1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TRL1.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TRL1.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0021_ORG ORGL1") 'POL1 NAMEL
        sqlStat.AppendLine("    ON ORGL1.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND ORGL1.ORGCODE      = TRL1.MORG")
        sqlStat.AppendLine("   AND ORGL1.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ORGL1.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND ORGL1.DELFLG      <> @DELFLG")

        'POL2
        sqlStat.AppendLine("  LEFT JOIN GBM0005_TRADER TRL2") 'Party to Contact1
        sqlStat.AppendLine("    ON TRL2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND TRL2.CARRIERCODE  = BB.AGENTPOL2")
        sqlStat.AppendLine("   AND TRL2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND TRL2.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND TRL2.DELFLG      <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN COS0021_ORG ORGL2") 'POL2 NAMEL
        sqlStat.AppendLine("    ON ORGL2.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND ORGL2.ORGCODE      = TRL2.MORG")
        sqlStat.AppendLine("   AND ORGL2.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND ORGL2.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND ORGL2.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0008_PRODUCT PD") 'Product
        sqlStat.AppendLine("    ON PD.COMPCODE     = @COMPCODE")
        sqlStat.AppendLine("   AND PD.PRODUCTCODE  = BB.PRODUCTCODE")
        sqlStat.AppendLine("   AND PD.STYMD       <= @STYMD")
        sqlStat.AppendLine("   AND PD.ENDYMD      >= @ENTDATE")
        sqlStat.AppendLine("   AND PD.DELFLG      <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV1") 'FIXVAL
        sqlStat.AppendLine("    ON FV1.CLASS       = 'REPORTTEXT'")
        sqlStat.AppendLine("   AND FV1.KEYCODE     = 'BL_DESCGOODS'")
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

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT1 ")
        sqlStat.AppendLine("    ON PT1.PORTCODE  = BB.LOADPORT1 ")
        sqlStat.AppendLine("   AND PT1.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT1.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND PT1.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT1 ")
        sqlStat.AppendLine("    ON CT1.COUNTRYCODE  = PT1.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT1.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT1.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND CT1.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT2 ")
        sqlStat.AppendLine("    ON PT2.PORTCODE  = BB.LOADPORT2 ")
        sqlStat.AppendLine("   AND PT2.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT2.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND PT2.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT2 ")
        sqlStat.AppendLine("    ON CT2.COUNTRYCODE  = PT2.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT2.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT2.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND CT2.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT3 ")
        sqlStat.AppendLine("    ON PT3.PORTCODE  = BB.RECIEPTPORT1")
        sqlStat.AppendLine("   AND PT3.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT3.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND PT3.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT4 ")
        sqlStat.AppendLine("    ON PT4.PORTCODE  = BB.RECIEPTPORT2")
        sqlStat.AppendLine("   AND PT4.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT4.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND PT4.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT5 ")
        sqlStat.AppendLine("    ON PT5.PORTCODE  = BB.DISCHARGEPORT1")
        sqlStat.AppendLine("   AND PT5.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT5.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND PT5.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT5 ")
        sqlStat.AppendLine("    ON CT5.COUNTRYCODE  = PT5.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT5.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT5.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND CT5.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT6 ")
        sqlStat.AppendLine("    ON PT6.PORTCODE  = BB.DISCHARGEPORT2")
        sqlStat.AppendLine("   AND PT6.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT6.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND PT6.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT6 ")
        sqlStat.AppendLine("    ON CT6.COUNTRYCODE  = PT6.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT6.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT6.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND CT6.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT7 ")
        sqlStat.AppendLine("    ON PT7.PORTCODE  = BB.DELIVERYPORT1")
        sqlStat.AppendLine("   AND PT7.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT7.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND PT7.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT7 ")
        sqlStat.AppendLine("    ON CT7.COUNTRYCODE  = PT7.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT7.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT7.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND CT7.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN GBM0002_PORT PT8 ")
        sqlStat.AppendLine("    ON PT8.PORTCODE  = BB.DELIVERYPORT2")
        sqlStat.AppendLine("   AND PT8.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND PT8.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND PT8.DELFLG   <> @DELFLG")
        sqlStat.AppendLine("  LEFT JOIN GBM0001_COUNTRY CT8 ")
        sqlStat.AppendLine("    ON CT8.COUNTRYCODE  = PT8.COUNTRYCODE")
        sqlStat.AppendLine("   AND CT8.STYMD    <= @STYMD")
        sqlStat.AppendLine("   AND CT8.ENDYMD   >= @ENTDATE")
        sqlStat.AppendLine("   AND CT8.DELFLG   <> @DELFLG")

        sqlStat.AppendLine("  LEFT JOIN COS0017_FIXVALUE FV2 ")
        sqlStat.AppendLine("    ON FV2.KEYCODE   = BB.TERMTYPE")
        sqlStat.AppendLine("   AND FV2.CLASS     = 'TERM'")
        sqlStat.AppendLine("   AND FV2.DELFLG   <> @DELFLG")

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
                    .Add("@ORDERNO", SqlDbType.NVarChar, 20).Value = orderNo
                    .Add("@STYMD", SqlDbType.Date).Value = entDate
                    .Add("@BRID", SqlDbType.NVarChar, 20).Value = breakerId
                    .Add("@TYPE", SqlDbType.NVarChar, 20).Value = "INFO"
                    .Add("@SALESPIC", SqlDbType.NVarChar, 20).Value = Me.hdnSalesPic.Value
                    .Add("@DELFLG", SqlDbType.NVarChar, 1).Value = CONST_FLAG_YES
                    .Add("@ENTDATE", SqlDbType.DateTime).Value = entDate
                    .Add("@UPDUSER", SqlDbType.NVarChar, 20).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar, 30).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                    .Add("@COMPCODE", SqlDbType.NVarChar, 20).Value = COA0019Session.APSRVCamp
                    .Add("@DAYSTEXT", SqlDbType.NVarChar).Value = "DAYS DETENTION FREE AT DESTINATION"
                    .Add("@CONTAINERPKGS", SqlDbType.NVarChar).Value = cnvStr & "(" & Convert.ToString(copyNum) & ")" & " TANK CONTAINER(S) ONLY"
                    .Add("@BOOKINGNO", SqlDbType.NVarChar).Value = Me.txtBookingNo.Text
                    .Add("@NOOFPACKAGE", SqlDbType.NVarChar).Value = Convert.ToString(copyNum)
                    .Add("@VSL1", SqlDbType.NVarChar).Value = Me.txtVesselName.Text
                    .Add("@VOY1", SqlDbType.NVarChar).Value = Me.txtVoyageNo.Text
                    .Add("@CONSIGNEE", SqlDbType.NVarChar).Value = Me.txtConsignee.Text
                    'ETD1 ETA1関連処理
                    .Add("@INITDATE", SqlDbType.Date).Value = "1900/01/01"
                    Dim eta1 As Date
                    Dim etd1 As Date
                    If Date.TryParseExact(Me.txtEta1.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, eta1) = False Then
                        eta1 = Date.Parse("1900/01/01")
                    End If
                    If Date.TryParseExact(Me.txtEtd1.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etd1) = False Then
                        etd1 = Date.Parse("1900/01/01")
                    End If
                    .Add("@ETA1", SqlDbType.Date).Value = eta1
                    .Add("@ETD1", SqlDbType.Date).Value = etd1

                    Dim eta2 As Date
                    Dim etd2 As Date
                    If Date.TryParseExact(Me.txtEta1.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, eta2) = False Then
                        eta2 = Date.Parse("1900/01/01")
                    End If
                    If Date.TryParseExact(Me.txtEtd1.Text, GBA00003UserSetting.DATEFORMAT, Nothing, Nothing, etd2) = False Then
                        etd2 = Date.Parse("1900/01/01")
                    End If
                    .Add("@ETA2", SqlDbType.Date).Value = eta2
                    .Add("@ETD2", SqlDbType.Date).Value = etd2

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
            sqlStat.AppendLine("   AND DELFLG   <> @DELFLG")
            Dim retDb As New DataTable
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                With sqlCmd.Parameters
                    .Add("@COMPCODE", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVCamp")
                    .Add("@DELFLG", SqlDbType.NVarChar).Value = CONST_FLAG_YES
                    .Add("@CLASS7", SqlDbType.NVarChar).Value = actyNo
                    .Add("@LDKBN", SqlDbType.NVarChar).Value = Right(polPod, 1)
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
    ''' オーダー費用情報を更新
    ''' </summary>
    ''' <param name="orderNo">オーダーNo</param>
    ''' <param name="dt">費用データテーブル(コピー増幅なし)</param>
    ''' <param name="copyNum">コピー数</param>
    ''' <param name="sqlCon">[In(省略可)]SQL接続オブジェクト</param>
    ''' <param name="tran">[In(省略可)]SQLトランザクションオブジェクト</param>
    Private Sub InsertOrderValue(orderNo As String, dt As DataTable, copyNum As Integer, brType As String, Optional ByRef sqlCon As SqlConnection = Nothing, Optional ByRef tran As SqlTransaction = Nothing, Optional entDate As Date = #1900/01/01#)
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
        sqlStat.AppendLine("       ,CURRENCYSEGMENT")
        sqlStat.AppendLine("       ,ACCCRERATE")
        sqlStat.AppendLine("       ,ACCCREYEN")
        sqlStat.AppendLine("       ,ACCCREFOREIGN")
        sqlStat.AppendLine("       ,ACCCURRENCYSEGMENT")
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
        sqlStat.AppendLine("       ,@COSTCODE")
        sqlStat.AppendLine("       ,@ACTIONID")
        sqlStat.AppendLine("       ,@DISPSEQ")
        sqlStat.AppendLine("       ,@LASTACT")
        sqlStat.AppendLine("       ,@REQUIREDACT")
        sqlStat.AppendLine("       ,@ORIGINDESTINATION")
        sqlStat.AppendLine("       ,@COUNTRYCODE")
        sqlStat.AppendLine("       ,@CURRENCYCODE")
        sqlStat.AppendLine("       ,@TAXATION")
        sqlStat.AppendLine("       ,@AMOUNTORD")
        'sqlStat.AppendLine("       ,@AMOUNTBR")
        sqlStat.AppendLine("       ,@AMOUNTORD")
        sqlStat.AppendLine("       ,@AMOUNTORD")
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
        sqlStat.AppendLine("       ,@CURRENCYSEGMENT")
        sqlStat.AppendLine("       ,@ACCCRERATE")
        sqlStat.AppendLine("       ,@ACCCREYEN")
        sqlStat.AppendLine("       ,@ACCCREFOREIGN")
        sqlStat.AppendLine("       ,@ACCCURRENCYSEGMENT")
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
            'ドル円レートを取得(経理資料用の円貨算出用
            Dim GBA00010ExRate As New GBA00010ExRate
            GBA00010ExRate.COUNTRYCODE = Convert.ToString("JP")
            GBA00010ExRate.TARGETYM = Date.Now.ToString("yyyy/MM")
            GBA00010ExRate.getExRateInfo()
            Dim jpUsdRateDt As New DataTable
            Dim jpUsdRate As String = ""
            If GBA00010ExRate.ERR = C_MESSAGENO.NORMAL Then
                jpUsdRateDt = GBA00010ExRate.EXRATE_TABLE
                Dim exRtDr As DataRow = jpUsdRateDt.Rows(0)
                jpUsdRate = Convert.ToString(exRtDr.Item("EXRATE"))
            Else
                jpUsdRate = "0"
            End If
            'JPYの桁数取得
            Dim GBA00008Country As New GBA00008Country
            Dim dtCont As New DataTable
            GBA00008Country.COUNTRYCODE = Convert.ToString("JP")
            GBA00008Country.COUNTRY_TABLE = dtCont
            GBA00008Country.getCountryInfo()
            dtCont = GBA00008Country.COUNTRY_TABLE
            Dim jpyDecPlace As String = Convert.ToString(dtCont.Rows(0).Item("DECIMALPLACES"))
            Dim jpyRoundFlg As String = Convert.ToString(dtCont.Rows(0).Item("ROUNDFLG"))
            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)
                If tran IsNot Nothing Then
                    sqlCmd.Transaction = tran
                End If
                'SQLパラメータの設定
                Dim paramOrderno As SqlParameter = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 20)
                Dim paramTankSeq As SqlParameter = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar, 20)
                Dim paramDtlPolPod As SqlParameter = sqlCmd.Parameters.Add("@DTLPOLPOD", SqlDbType.NVarChar, 20)
                Dim paramDtlOffice As SqlParameter = sqlCmd.Parameters.Add("@DTLOFFICE", SqlDbType.NVarChar, 20)
                Dim paramCostCode As SqlParameter = sqlCmd.Parameters.Add("@COSTCODE", SqlDbType.NVarChar, 20)
                Dim paramActionId As SqlParameter = sqlCmd.Parameters.Add("@ACTIONID", SqlDbType.NVarChar, 50)
                Dim paramDispSeq As SqlParameter = sqlCmd.Parameters.Add("@DISPSEQ", SqlDbType.NVarChar, 50)
                Dim paramLastAct As SqlParameter = sqlCmd.Parameters.Add("@LASTACT", SqlDbType.NVarChar, 50)
                Dim paramRequiredAct As SqlParameter = sqlCmd.Parameters.Add("@REQUIREDACT", SqlDbType.NVarChar, 50)
                Dim paramOriginDestination As SqlParameter = sqlCmd.Parameters.Add("@ORIGINDESTINATION", SqlDbType.NVarChar, 50)
                Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)

                Dim paramCurrencyCode As SqlParameter = sqlCmd.Parameters.Add("@CURRENCYCODE", SqlDbType.NVarChar, 20)
                Dim paramTaxation As SqlParameter = sqlCmd.Parameters.Add("@TAXATION", SqlDbType.NVarChar, 1)

                Dim paramAmountBr As SqlParameter = sqlCmd.Parameters.Add("@AMOUNTBR", SqlDbType.Float)
                Dim paramAmountOrd As SqlParameter = sqlCmd.Parameters.Add("@AMOUNTORD", SqlDbType.Float)
                Dim paramContractorBr As SqlParameter = sqlCmd.Parameters.Add("@CONTRACTORBR", SqlDbType.NVarChar, 20)
                Dim paramSchedelDateBr As SqlParameter = sqlCmd.Parameters.Add("@SCHEDELDATEBR", SqlDbType.Date, 20)
                Dim paramLocalBr As SqlParameter = sqlCmd.Parameters.Add("@LOCALBR", SqlDbType.Float, 20)
                Dim paramLocalRate As SqlParameter = sqlCmd.Parameters.Add("@LOCALRATE", SqlDbType.Float, 20)
                Dim paramTaxBr As SqlParameter = sqlCmd.Parameters.Add("@TAXBR", SqlDbType.Float, 20)
                Dim paramInvoicedBy As SqlParameter = sqlCmd.Parameters.Add("@INVOICEDBY", SqlDbType.NVarChar, 20)
                Dim paramRemark As SqlParameter = sqlCmd.Parameters.Add("@REMARK", SqlDbType.NVarChar, 200)
                Dim paramBrid As SqlParameter = sqlCmd.Parameters.Add("@BRID", SqlDbType.NVarChar, 20)
                Dim paramBrCost As SqlParameter = sqlCmd.Parameters.Add("@BRCOST", SqlDbType.NVarChar, 20)

                Dim paramBrDateField As SqlParameter = sqlCmd.Parameters.Add("@DATEFIELD", SqlDbType.NVarChar, 50)
                Dim paramDateInterval As SqlParameter = sqlCmd.Parameters.Add("@DATEINTERVAL", SqlDbType.NVarChar, 50)
                Dim paramBrAddedCost As SqlParameter = sqlCmd.Parameters.Add("@BRADDEDCOST", SqlDbType.NVarChar, 50)

                Dim paramAgentOrganizer As SqlParameter = sqlCmd.Parameters.Add("@AGENTORGANIZER", SqlDbType.NVarChar, 20)

                Dim paramCurrencySegment As SqlParameter = sqlCmd.Parameters.Add("@CURRENCYSEGMENT", SqlDbType.NVarChar)
                Dim paramAccCreRate As SqlParameter = sqlCmd.Parameters.Add("@ACCCRERATE", SqlDbType.Float)
                Dim paramAccCreYen As SqlParameter = sqlCmd.Parameters.Add("@ACCCREYEN", SqlDbType.Float)
                Dim paramAccCreForeign As SqlParameter = sqlCmd.Parameters.Add("@ACCCREFOREIGN", SqlDbType.Float)
                Dim paramAccCurrencySegment As SqlParameter = sqlCmd.Parameters.Add("@ACCCURRENCYSEGMENT", SqlDbType.NVarChar)

                Dim paramDelflg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                Dim paramEntDate As SqlParameter = sqlCmd.Parameters.Add("@ENTDATE", SqlDbType.DateTime)
                Dim paramUpduser As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)
                Dim paramUpdtermid As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 30)
                Dim paramReceiveymd As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)
                'コストデータに依存しない固定パラメータ値を設定
                paramOrderno.Value = orderNo
                paramBrid.Value = Me.txtBrId.Text
                paramBrCost.Value = "1"
                paramDelflg.Value = CONST_FLAG_NO
                paramEntDate.Value = entDate
                paramUpduser.Value = COA0019Session.USERID
                paramUpdtermid.Value = HttpContext.Current.Session("APSRVname")
                paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD

                Dim isFirst As Boolean = True
                'コピー数分ループ(TANKSEQ)の0埋め前
                For i = 1 To copyNum
                    Dim tankSeq As String = i.ToString("000")
                    paramTankSeq.Value = tankSeq
                    Dim blDataList As New Dictionary(Of String, Object)

                    Dim blSumD1 As Decimal = 0
                    Dim blSumD2 As Decimal = 0
                    Dim blSumL1 As Decimal = 0
                    Dim blSumL2 As Decimal = 0
                    Dim blSum As Decimal = 0

                    Dim blUsd As Object = 0

                    Dim OrgVender As String = ""
                    Dim OrgInvoicedBy As String = ""
                    Dim OrgDtlOffice As String = ""
                    Dim OrgCountryCode As String = ""
                    Dim OrgRate As String = ""
                    'Dim blRateD1 As Object = ""
                    'Dim blCurD1 As String = ""
                    'Dim blRateD2 As Object = ""
                    'Dim blCurD2 As String = ""
                    'Dim blRateL1 As Object = ""
                    'Dim blCurL1 As String = ""
                    'Dim blRateL2 As Object = ""
                    'Dim blCurL2 As String = ""

                    Dim useFlg As Boolean = False
                    Dim rate As String = Nothing
                    Dim blFlg As Boolean = False

                    'データテーブルループ
                    For Each dr As DataRow In dt.Rows
                        paramDtlPolPod.Value = Convert.ToString(dr.Item("AGENTKBN"))
                        paramDtlOffice.Value = Convert.ToString(dr.Item("OFFICE"))
                        paramCostCode.Value = Convert.ToString(dr.Item("COSTCODE"))
                        paramActionId.Value = Convert.ToString(dr.Item("ACTY"))
                        paramDispSeq.Value = Convert.ToString(dr.Item("DISPSEQ"))
                        paramLastAct.Value = Convert.ToString(dr.Item("LASTACT"))
                        paramRequiredAct.Value = Convert.ToString(dr.Item("REQUIREDACT"))
                        paramOriginDestination.Value = Convert.ToString(dr.Item("ORIGINDESTINATION"))
                        paramCountryCode.Value = Convert.ToString(dr.Item("COUNTRYCODE"))

                        paramCurrencyCode.Value = Convert.ToString(dr.Item("CURRENCYCODE"))
                        paramTaxation.Value = Convert.ToString(dr.Item("TAXATION"))

                        '為替レート取得
                        Dim exRtDt As DataTable = Nothing
                        GBA00010ExRate = New GBA00010ExRate
                        GBA00010ExRate.COUNTRYCODE = Convert.ToString(dr.Item("COUNTRYCODE"))
                        GBA00010ExRate.TARGETYM = Date.Now.ToString("yyyy/MM")
                        GBA00010ExRate.getExRateInfo()
                        If GBA00010ExRate.ERR = C_MESSAGENO.NORMAL Then
                            exRtDt = GBA00010ExRate.EXRATE_TABLE
                            Dim exRtDr As DataRow = exRtDt.Rows(0)
                            rate = Convert.ToString(exRtDr.Item("EXRATE"))
                        Else
                            rate = "0"
                        End If
                        paramLocalRate.Value = DecimalStringToDecimal(rate)

                        Dim amt As Object = Nothing
                        Dim usdAmt As Object = Nothing
                        If Convert.ToDecimal(dr.Item("LOCALBR")) <> 0 Then

                            If DecimalStringToDecimal(rate) <> 0 Then
                                usdAmt = Convert.ToString((Decimal.Parse(dr.Item("LOCALBR").ToString) / Decimal.Parse(rate)))
                            Else
                                usdAmt = "0"
                            End If

                            Dim decPlace As Integer = 0
                            Dim roundFlg As String = ""
                            If GetDecimalPlaces(decPlace, roundFlg) Then
                                Select Case roundFlg
                                    Case GBC_ROUNDFLG.UP
                                        usdAmt = Convert.ToString(RoundUp(Decimal.Parse(usdAmt.ToString), CUInt(decPlace)))
                                    Case GBC_ROUNDFLG.DOWN
                                        usdAmt = Convert.ToString(RoundDown(Decimal.Parse(usdAmt.ToString), decPlace))
                                    Case GBC_ROUNDFLG.ROUND
                                        usdAmt = Convert.ToString(Round(Decimal.Parse(usdAmt.ToString), CUInt(decPlace)))
                                End Select

                            End If

                            blUsd = NumberFormat(DecimalStringToDecimal(usdAmt.ToString), "", decPlace).ToString

                            amt = dr.Item("LOCALBR")

                        Else
                            blUsd = dr.Item("AMOUNTBR")

                            amt = dr.Item("AMOUNTBR")
                        End If

                        paramAmountBr.Value = amt

                        Dim amtOrd As Decimal = 0
                        Dim isBlTargetRow As Boolean
                        isBlTargetRow = False
                        Dim remarks As String = Convert.ToString(dr.Item("REMARK"))
                        'If isFirst = True Then

                        '    amtOrd = Convert.ToDecimal(amt)

                        If isFirst = False AndAlso Convert.ToString(dr.Item("PERBL")) = CONST_FLAG_YES Then

                            If blDataList.ContainsKey(Convert.ToString(dr.Item("AGENTKBN"))) = False Then
                                blDataList.Add(Convert.ToString(dr.Item("AGENTKBN")),
                                               New With {.country = Convert.ToString(dr.Item("COUNTRYCODE")),
                                                         .office = Convert.ToString(dr.Item("OFFICE"))})
                            End If

                            Select Case Convert.ToString(dr.Item("AGENTKBN"))
                                Case "POD1"
                                    blSumD1 += Convert.ToDecimal(blUsd)

                                Case "POD2"
                                    blSumD2 += Convert.ToDecimal(blUsd)
                                    useFlg = True

                                Case "POL1"
                                    blSumL1 += Convert.ToDecimal(blUsd)

                                Case "POL2"
                                    blSumL2 += Convert.ToDecimal(blUsd)
                                    useFlg = True

                            End Select

                            blFlg = True
                            isBlTargetRow = True
                            remarks = "Per B/L"

                            'ElseIf isFirst = False AndAlso Convert.ToString(dr.Item("PERBL")) = CONST_FLAG_NO Then
                        Else

                            amtOrd = Convert.ToDecimal(amt)

                        End If

                        paramAmountOrd.Value = amtOrd
                        paramContractorBr.Value = Convert.ToString(dr.Item("CONTRACTORBR"))
                        Dim brAddedCost As String = ""
                        brAddedCost = Convert.ToString(dr.Item("BRADDEDCOST"))
                        If Convert.ToString(dr.Item("AGENTKBN")) = CONST_ORGANIZER AndAlso Convert.ToString(dr.Item("COSTCODE")) = GBC_COSTCODE_SALES Then
                            brAddedCost = "" 'TOTALINVOICEはSOAに表示
                        End If

                        If {GBC_COSTCODE_DEMURRAGE}.Contains(Convert.ToString(dr.Item("COSTCODE"))) Then

                            If Convert.ToString(dr.Item("AGENTKBN")).StartsWith("POL") Then
                                paramContractorBr.Value = Convert.ToString(dr.Item("SHIPPER"))
                            Else
                                paramContractorBr.Value = Me.txtConsignee.Text
                            End If
                        ElseIf Convert.ToString(dr.Item("AGENTKBN")) = CONST_ORGANIZER Then
                            OrgInvoicedBy = Convert.ToString(dr.Item("INVOICEDBY"))
                            If Convert.ToString(dr.Item("TRPBILLING")) = CONST_SHIPPERCLASS Then
                                'SHIPPER
                                paramContractorBr.Value = Convert.ToString(dr.Item("SHIPPER"))
                                OrgVender = Convert.ToString(dr.Item("SHIPPER"))
                            ElseIf Convert.ToString(dr.Item("TRPBILLING")) = CONSIGNEECLASS Then
                                'CONSIGNEE
                                paramContractorBr.Value = Me.txtConsignee.Text
                                OrgVender = Me.txtConsignee.Text
                            Else
                                If Convert.ToString(dr.Item("BILLINGCATEGORY")) = GBC_DELIVERYCLASS.SHIPPER Then
                                    paramContractorBr.Value = Convert.ToString(dr.Item("SHIPPER"))
                                    OrgVender = Convert.ToString(dr.Item("SHIPPER"))
                                ElseIf Convert.ToString(dr.Item("BILLINGCATEGORY")) = GBC_DELIVERYCLASS.CONSIGNEE Then
                                    paramContractorBr.Value = Me.txtConsignee.Text
                                    OrgVender = Me.txtConsignee.Text
                                End If
                            End If

                            OrgDtlOffice = Convert.ToString(dr.Item("OFFICE"))
                            OrgCountryCode = Convert.ToString(dr.Item("COUNTRYCODE"))
                            OrgRate = rate
                            If {GBC_COSTCODE_AGENTCOM}.Contains(Convert.ToString(dr.Item("COSTCODE"))) Then
                                If remarks <> "" Then
                                    remarks = remarks & " "
                                End If
                                remarks = remarks & "Organizer Agent Comm"
                                '費用のInvoicedはエージェントオーガナイザーに変更
                                'dr.Item("INVOICEDBY") = dr.Item("AGENTORGANIZER")
                                brAddedCost = "" 'オーガナイザー手数料はSOAに表示
                            End If
                        End If
                        paramSchedelDateBr.Value = dr.Item("SCHEDELDATEBR")
                        paramLocalBr.Value = dr.Item("LOCALBR")
                        'paramLocalRate.Value = dr.Item("LOCALRATE")

                        paramTaxBr.Value = dr.Item("TAXBR")
                        paramInvoicedBy.Value = Convert.ToString(dr.Item("INVOICEDBY"))
                        paramRemark.Value = remarks

                        paramBrDateField.Value = Convert.ToString(dr.Item("DATEFIELD"))
                        paramDateInterval.Value = Convert.ToString(dr.Item("DATEINTERVAL"))
                        paramBrAddedCost.Value = brAddedCost

                        paramAgentOrganizer.Value = Convert.ToString(dr.Item("AGENTORGANIZER"))

                        paramCurrencySegment.Value = ""
                        paramAccCreRate.Value = DecimalStringToDecimal(rate)
                        Dim amountBr As String = Convert.ToString(dr.Item("AMOUNTBR"))
                        If isBlTargetRow Then
                            amountBr = "0"
                        End If
                        Dim amountJpy = Convert.ToString((Decimal.Parse(amountBr) * Decimal.Parse(jpUsdRate)))

                        Select Case jpyRoundFlg
                            Case GBC_ROUNDFLG.UP
                                amountJpy = Convert.ToString(RoundUp(Decimal.Parse(amountJpy.ToString), CUInt(jpyDecPlace)))
                            Case GBC_ROUNDFLG.DOWN
                                amountJpy = Convert.ToString(RoundDown(Decimal.Parse(amountJpy.ToString), CInt(jpyDecPlace)))
                            Case GBC_ROUNDFLG.ROUND
                                amountJpy = Convert.ToString(Round(Decimal.Parse(amountJpy.ToString), CUInt(jpyDecPlace)))
                        End Select
                        paramAccCreForeign.Value = amountBr
                        paramAccCreYen.Value = amountJpy

                        paramAccCurrencySegment.Value = ""

                        sqlCmd.ExecuteNonQuery()
                    Next 'End DataRow Loop

                    If isFirst = False AndAlso blFlg AndAlso brType = C_BRTYPE.SALES Then

                        'Dim cnt As Integer = 2
                        'If useFlg Then
                        '    cnt = 4
                        'End If

                        'For j As Integer = 1 To cnt

                        paramCostCode.Value = GBC_COSTCODE_HIRAGEOTHER
                        paramActionId.Value = ""
                        paramDispSeq.Value = ""
                        paramLastAct.Value = ""
                        paramRequiredAct.Value = ""
                        paramOriginDestination.Value = ""
                        paramLocalBr.Value = "0"
                        paramLocalRate.Value = OrgRate
                        paramCurrencyCode.Value = GBC_CUR_USD
                        paramRemark.Value = "Per B/L"

                        'Select Case j
                        '    Case 1
                        '        paramDtlPolPod.Value = "POD1"
                        '        paramAmountBr.Value = Convert.ToString(blSumD1)
                        '        paramAmountOrd.Value = Convert.ToString(blSumD1)
                        '    Case 2
                        '        paramDtlPolPod.Value = "POL1"
                        '        paramAmountBr.Value = Convert.ToString(blSumL1)
                        '        paramAmountOrd.Value = Convert.ToString(blSumL1)
                        '    Case 3
                        '        paramDtlPolPod.Value = "POD2"
                        '        paramAmountBr.Value = Convert.ToString(blSumD2)
                        '        paramAmountOrd.Value = Convert.ToString(blSumD2)
                        '    Case 4
                        '        paramDtlPolPod.Value = "POL2"
                        '        paramAmountBr.Value = Convert.ToString(blSumL2)
                        '        paramAmountOrd.Value = Convert.ToString(blSumL2)
                        'End Select

                        paramDtlPolPod.Value = CONST_ORGANIZER
                        blSum = blSumD1 + blSumL1 + blSumD2 + blSumL2
                        paramAmountBr.Value = Convert.ToString(blSum)
                        paramAmountOrd.Value = Convert.ToString(blSum)

                        'If blDataList.ContainsKey(Convert.ToString(paramDtlPolPod.Value)) Then
                        '    Dim blData = blDataList(Convert.ToString(paramDtlPolPod.Value))
                        '    paramInvoicedBy.Value = CallByName(blData, "office", CallType.Get)
                        '    paramDtlOffice.Value = CallByName(blData, "office", CallType.Get)
                        '    paramCountryCode.Value = CallByName(blData, "country", CallType.Get)
                        'End If
                        paramInvoicedBy.Value = OrgInvoicedBy
                        paramDtlOffice.Value = OrgDtlOffice
                        paramCountryCode.Value = OrgCountryCode
                        paramContractorBr.Value = OrgVender
                        paramTaxation.Value = GetDefaultTaxation(Convert.ToString(paramCountryCode.Value))
                        paramCurrencySegment.Value = ""
                        paramAccCreRate.Value = OrgRate
                        Dim amountBr As String = Convert.ToString(blSum)
                        Dim amountJpy = Convert.ToString((Decimal.Parse(amountBr) * Decimal.Parse(jpUsdRate)))

                        Select Case jpyRoundFlg
                            Case GBC_ROUNDFLG.UP
                                amountJpy = Convert.ToString(RoundUp(Decimal.Parse(amountJpy.ToString), CUInt(jpyDecPlace)))
                            Case GBC_ROUNDFLG.DOWN
                                amountJpy = Convert.ToString(RoundDown(Decimal.Parse(amountJpy.ToString), CInt(jpyDecPlace)))
                            Case GBC_ROUNDFLG.ROUND
                                amountJpy = Convert.ToString(Round(Decimal.Parse(amountJpy.ToString), CUInt(jpyDecPlace)))
                        End Select
                        paramAccCreForeign.Value = amountBr
                        paramAccCreYen.Value = amountJpy

                        paramAccCurrencySegment.Value = ""

                        paramTaxation.Value = "0" 'Per B/L分の課税フラグはOFF
                        paramBrAddedCost.Value = "2" 'Per B/L分のオーガナイザレコードもSOAに表示させないため2
                        sqlCmd.ExecuteNonQuery()

                        '↓20190712 S101-02に対してのA0100-01のマイナス金額レコードを生成
                        '以下のパラメータを除いてはS0101-02と同一
                        paramCostCode.Value = GBC_COSTCODE_FREIGHT_REVENUE
                        paramAmountBr.Value = Convert.ToString(blSum * -1)
                        paramAmountOrd.Value = Convert.ToString(blSum * -1)
                        paramAccCreForeign.Value = Convert.ToString(Decimal.Parse(amountBr) * -1)
                        paramAccCreYen.Value = Convert.ToString(Decimal.Parse(amountJpy) * -1)
                        sqlCmd.ExecuteNonQuery()
                        '↑20190712 S101-02に対してのA0100-01のマイナス金額レコードを生成
                        'Next

                    End If
                    isFirst = False
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
                'SQLパラメータの設定
                Dim paramOrderno As SqlParameter = sqlCmd.Parameters.Add("@ORDERNO", SqlDbType.NVarChar, 20)
                Dim paramTankSeq As SqlParameter = sqlCmd.Parameters.Add("@TANKSEQ", SqlDbType.NVarChar, 20)
                Dim paramTrilateral As SqlParameter = sqlCmd.Parameters.Add("@TRILATERAL", SqlDbType.NVarChar, 1)
                Dim paramTankType As SqlParameter = sqlCmd.Parameters.Add("@TANKTYPE", SqlDbType.NVarChar, 20)
                Dim paramDelflg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
                Dim paramEntDate As SqlParameter = sqlCmd.Parameters.Add("@ENTDATE", SqlDbType.DateTime)
                Dim paramUpduser As SqlParameter = sqlCmd.Parameters.Add("@UPDUSER", SqlDbType.NVarChar, 20)
                Dim paramUpdtermid As SqlParameter = sqlCmd.Parameters.Add("@UPDTERMID", SqlDbType.NVarChar, 30)
                Dim paramReceiveymd As SqlParameter = sqlCmd.Parameters.Add("@RECEIVEYMD", SqlDbType.DateTime)
                Dim paramNoOfPackage As SqlParameter = sqlCmd.Parameters.Add("@NOOFPACKAGE", SqlDbType.Float)
                'コストデータに依存しない固定パラメータ値を設定
                paramOrderno.Value = orderNo
                paramTankType.Value = "20TK"
                paramDelflg.Value = CONST_FLAG_NO
                paramEntDate.Value = entDate
                paramUpduser.Value = COA0019Session.USERID
                paramUpdtermid.Value = HttpContext.Current.Session("APSRVname")
                paramReceiveymd.Value = CONST_DEFAULT_RECEIVEYMD
                paramNoOfPackage.Value = 1

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
    ''' 課税フラグのデフォルト値取得
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>仮作成にて変動の可能性がある為、デフォルト値取得関数化</remarks>
    Private Function GetDefaultTaxation(countryCode As String) As String
        Return If(GBA00003UserSetting.IS_JPOPERATOR AndAlso countryCode = "JP", "1", "0")
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
        ''' <summary>
        ''' 船名
        ''' </summary>
        ''' <returns></returns>
        Public Property VesselName As String = ""
        ''' <summary>
        ''' 航海番号
        ''' </summary>
        ''' <returns></returns>
        Public Property VoyageNo As String = ""
        ''' <summary>
        ''' 荷受人
        ''' </summary>
        ''' <returns></returns>
        Public Property Consignee As String = ""
        ''' <summary>
        ''' 荷渡国コード1
        ''' </summary>
        ''' <returns></returns>
        Public Property DeliveryCountry1 As String = ""
        ''' <summary>
        ''' リースタンク利用("0":未使用,"1":使用)ブレーカーBaseから引き継ぎ項目
        ''' </summary>
        ''' <returns></returns>
        Public Property UsingLeaseTank As String = "0"
        ''' <summary>
        ''' BR COMMISSION
        ''' </summary>
        ''' <returns></returns>
        Public Property BrCommission As String = ""
        ''' <summary>
        ''' Shipperの総額
        ''' </summary>
        ''' <returns></returns>
        Public Property BrShipperTotalInvoiced As String = ""
        ''' <summary>
        ''' Consigneeの総額
        ''' </summary>
        ''' <returns></returns>
        Public Property BrConsigneeTotalInvoiced As String = ""
        ''' <summary>
        ''' 請求先判定
        ''' </summary>
        ''' <returns></returns>
        Public Property BillingCategory As String = ""

        ''' <summary>
        ''' Shipperの費用合計
        ''' </summary>
        ''' <returns></returns>
        Public Property BrShipperCostTotal As String = ""
        ''' <summary>
        ''' Consigneeの費用合計
        ''' </summary>
        ''' <returns></returns>
        Public Property BrConsigneeCostTotal As String = ""
        ''' <summary>
        ''' ブレーカー有効期限From
        ''' </summary>
        ''' <returns></returns>
        Public Property BrValidityFrom As String = ""
        ''' <summary>
        ''' ブレーカー有効期限To
        ''' </summary>
        ''' <returns></returns>
        Public Property BrValidityTo As String = ""
        ''' <summary>
        ''' BR時点費用合計
        ''' </summary>
        ''' <returns></returns>
        Public Property BrTotalCost As String = ""
        ''' <summary>
        ''' 値引き
        ''' </summary>
        ''' <returns></returns>
        Public Property BrAmtDiscount As String = ""
        ''' <summary>
        ''' BR時点仮計上費用合計
        ''' </summary>
        ''' <returns></returns>
        Public Property BrTotalProvisionalCost As String = ""
    End Class
    ''' <summary>
    ''' ブレーカー情報保持
    ''' </summary>
    <Serializable>
    Public Class BreakerInfo
        ''' <summary>
        ''' ブレーカーID
        ''' </summary>
        ''' <returns></returns>
        Public Property BrId As String = ""
        ''' <summary>
        ''' サブID(BRID-枝番)
        ''' </summary>
        ''' <returns></returns>
        Public Property SubId As String = ""
        ''' <summary>
        ''' ブレーカー種類
        ''' </summary>
        ''' <returns></returns>
        Public Property BrType As String = ""
        ''' <summary>
        ''' 種別(POL1,POD1等)
        ''' </summary>
        ''' <returns></returns>
        Public Property Type As String = ""
        ''' <summary>
        ''' 個別ID
        ''' </summary>
        ''' <returns></returns>
        Public Property LinkId As String = ""
        ''' <summary>
        ''' 開始年月日
        ''' </summary>
        ''' <returns></returns>
        Public Property Stymd As String = ""
        ''' <summary>
        ''' 申請ID
        ''' </summary>
        ''' <returns></returns>
        Public Property ApplyId As String = ""
        ''' <summary>
        ''' タイムスタンプ
        ''' </summary>
        ''' <returns></returns>
        Public Property TimeStamp As String = ""
    End Class

    ''' <summary>
    ''' USD桁数取得
    ''' </summary>
    Public Function GetDecimalPlaces(ByRef retDecPlace As Integer, ByRef retRound As String) As Boolean

        Dim COA0017FixValue As New COA0017FixValue
        'リスト設定
        COA0017FixValue.COMPCODE = GBC_COMPCODE_D
        COA0017FixValue.CLAS = C_FIXVALUECLAS.USD_DECIMALPLACES
        COA0017FixValue.COA0017getListFixValue()
        If COA0017FixValue.ERR = C_MESSAGENO.NORMAL Then
            retDecPlace = CInt(COA0017FixValue.VALUE1.Items(0).ToString)
            retRound = COA0017FixValue.VALUE2.Items(0).ToString
        Else
            '異常
            CommonFunctions.ShowMessage(COA0017FixValue.ERR, Me.lblFooterMessage,
                                        messageParams:=New List(Of String) From {String.Format("CODE:{0}", COA0017FixValue.ERR)})
            Return False
        End If

        Return True

    End Function

    ''' <summary>
    ''' 切り上げ
    ''' </summary>
    ''' <param name="value">対象の数値</param>
    ''' <param name="decimalPlaces">有効小数桁数</param>
    ''' <returns>切り上げした数値</returns>
    Public Shared Function RoundUp(ByVal value As Decimal, ByVal decimalPlaces As UInt32) As Decimal
        Dim rate As Decimal = CDec(Math.Pow(10.0R, decimalPlaces))

        If value < 0 Then
            Return (Math.Ceiling(value * -1D * rate) / rate) * -1D
        Else
            Return Math.Ceiling(value * rate) / rate
        End If
    End Function

    ''' <summary>
    ''' 四捨五入
    ''' </summary>
    ''' <param name="value">対象の数値</param>
    ''' <param name="decimalPlaces">有効小数桁数</param>
    ''' <returns>四捨五入した数値</returns>
    Public Shared Function Round(ByVal value As Decimal, ByVal decimalPlaces As UInt32) As Decimal
        Return Math.Round(value, CInt(decimalPlaces), MidpointRounding.AwayFromZero)
    End Function

    ''' <summary>
    ''' 荷受人一覧取得
    ''' </summary>
    ''' <param name="countryCode">国コード</param>
    ''' <param name="customerCode">顧客コード</param>
    ''' <returns>荷受人一覧データテーブル</returns>
    ''' <remarks>GBM0004_CUSTOMERより荷受人情報を取得</remarks>
    Private Function GetConsignee(countryCode As String, Optional customerCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        '文言フィールド（開発中のためいったん固定
        Dim textField As String = "NAMES"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMESEN"
        End If
        'SQL文作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("Select CUSTOMERCODE")
        sqlStat.AppendFormat("      , {0} As NAME", textField).AppendLine()
        sqlStat.AppendFormat("      , CUSTOMERCODE + ':' + {0}  AS LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0004_CUSTOMER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
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
            Dim paramCountryCode As SqlParameter = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            Dim paramCustomerCode As SqlParameter = Nothing
            If customerCode <> "" Then
                paramCustomerCode = sqlCmd.Parameters.Add("@CUSTOMERCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            paramCountryCode.Value = countryCode
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
    ''' 対象Agent一覧を取得
    ''' </summary>
    ''' <param name="countryCode"></param>
    ''' <param name="carrierCode">業者コード</param>
    ''' <returns></returns>
    ''' <remarks>GBM0005_TRADERより引数国コードをもとにCLASS='AGENT'の一覧を取得</remarks>
    Private Function GetAgent(countryCode As String, Optional carrierCode As String = "") As DataTable
        Dim retDt As New DataTable   '戻り値用のデータテーブル
        'SQL文作成
        Dim textField As String = "NAMESJP"
        If COA0019Session.LANGDISP <> C_LANG.JA Then
            textField = "NAMES"
        End If
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT CARRIERCODE AS CODE")
        sqlStat.AppendFormat("     , CARRIERCODE + ':' + {0} As LISTBOXNAME", textField).AppendLine()
        sqlStat.AppendFormat("     , {0} As NAME", textField).AppendLine()
        sqlStat.AppendLine("  FROM GBM0005_TRADER")
        sqlStat.AppendLine(" WHERE COMPCODE    = @COMPCODE")
        If countryCode <> "" Then
            sqlStat.AppendLine("   AND COUNTRYCODE = @COUNTRYCODE")
        End If
        sqlStat.AppendLine("   AND CLASS       = '" & C_TRADER.CLASS.AGENT & "'")
        If carrierCode <> "" Then
            sqlStat.AppendLine("   And CARRIERCODE    = @CARRIERCODE")
        End If
        sqlStat.AppendLine("   And STYMD       <= @STYMD")
        sqlStat.AppendLine("   And ENDYMD      >= @ENDYMD")
        sqlStat.AppendLine("   And DELFLG      <> @DELFLG")
        sqlStat.AppendLine("ORDER BY CARRIERCODE ")
        'DB接続
        Using sqlCon As New SqlConnection(COA0019Session.DBcon),
              sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

            sqlCon.Open() '接続オープン
            'SQLパラメータ設定
            Dim paramCompCode As SqlParameter = sqlCmd.Parameters.Add("@COMPCODE", SqlDbType.NVarChar, 10)
            Dim paramCountryCode As SqlParameter = Nothing
            If countryCode <> "" Then
                paramCountryCode = sqlCmd.Parameters.Add("@COUNTRYCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramcarrierCode As SqlParameter = Nothing
            If carrierCode <> "" Then
                paramcarrierCode = sqlCmd.Parameters.Add("@CARRIERCODE", SqlDbType.NVarChar, 20)
            End If
            Dim paramStYmd = sqlCmd.Parameters.Add("@STYMD", SqlDbType.Date)
            Dim paramEndYmd = sqlCmd.Parameters.Add("@ENDYMD", SqlDbType.Date)
            Dim paramDelFlg As SqlParameter = sqlCmd.Parameters.Add("@DELFLG", SqlDbType.NVarChar, 1)
            'SQLパラメータ値セット
            paramCompCode.Value = HttpContext.Current.Session("APSRVCamp") '本来はセッション変数をラッピングした構造体で取得
            If countryCode <> "" Then
                paramCountryCode.Value = countryCode
            End If
            If carrierCode <> "" Then
                paramcarrierCode.Value = carrierCode
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
    ''' 荷主名称を画面に設定
    ''' </summary>
    ''' <param name="targetTextObject">対象テキスト</param>
    ''' <param name="customerCode">荷主コード（顧客コード）</param>
    Private Sub SetDisplayConsignee(targetTextObject As TextBox, customerCode As String)
        '一旦リセット
        targetTextObject.Text = customerCode.Trim
        Me.lblConsigneeText.Text = ""
        '荷主コード（顧客コード）が未入力の場合はDBアクセスせずに終了
        If customerCode.Trim = "" Then
            Return
        End If
        Dim countryCode As String = Me.hdnDeliveryCountry1.Value

        Dim dt As DataTable = New DataTable
        If Me.hdnBrType.Value = C_BRTYPE.SALES Then
            dt = GetConsignee(countryCode, customerCode.Trim)
        Else
            dt = GetAgent(countryCode, customerCode.Trim)
        End If

        'データが取れない場合はそのまま終了
        If dt Is Nothing OrElse dt.Rows.Count = 0 Then
            Return
        End If
        Dim dr As DataRow = dt.Rows(0)
        Me.lblConsigneeText.Text = HttpUtility.HtmlEncode(Convert.ToString(dr.Item("NAME")))
    End Sub
    ''' <summary>
    ''' 荷受人変更時
    ''' </summary>
    Public Sub txtConsignee_Change()
        Dim consignee As String = Me.txtConsignee.Text.Trim
        Me.txtConsignee.Text = consignee
        Me.lblConsigneeText.Text = ""
        If consignee <> "" Then
            SetDisplayConsignee(Me.txtConsignee, consignee)
        End If
    End Sub
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