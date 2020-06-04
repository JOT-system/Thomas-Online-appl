Imports System.Data.SqlClient
Imports BASEDLL
''' <summary>
''' LEASEブレーカー検索クラス
''' </summary>
Public Class GBT00026BLNOTRANSFER
    Inherits GbPageCommon

    Private Const CONST_MAPID As String = "GBT00026T"     '自身のMAPID
    Private Const CONST_TBL_ODRB As String = "GBT0004_ODR_BASE"     'Order Base
    Private Const CONST_TBL_BRB As String = "GBT0002_BR_BASE"       'Breaker Base

    Private returnCode As String = String.Empty           'サブ用リターンコード
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
            Dim COA0031ProfMap As New BASEDLL.COA0031ProfMap
            Dim COA0007getCompanyInfo As New BASEDLL.COA0007CompanyInfo

            '共通ロード処理のエラー確認
            If Convert.ToString(Session(CONST_SESSION_COM_LOAD_MESSAGENO)) <> C_MESSAGENO.NORMAL Then
                Return
            End If

            HttpContext.Current.Session("MAPurl") = ""
            returnCode = C_MESSAGENO.NORMAL

            '****************************************
            'メッセージ初期化
            '****************************************
            lblFooterMessage.Text = ""

            '****************************************
            '初回ロード時
            '****************************************
            If IsPostBack = False Then
                Me.hdnMapVariant.Value = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
                '****************************************
                '日本語/英語 文言切替
                '****************************************
                LangSetting(COA0019Session.LANGDISP)
                '****************************************
                'タイトル設定
                '****************************************
                COA0031ProfMap.MAPIDP = CONST_MAPID
                COA0031ProfMap.VARIANTP = Me.hdnMapVariant.Value
                COA0031ProfMap.COA0031GetDisplayTitle()
                If COA0031ProfMap.ERR = C_MESSAGENO.NORMAL Then
                    Me.lblTitleText.Text = COA0031ProfMap.NAMES
                End If

                '****************************************
                '初期表示
                '****************************************
                '検索設定の選択肢を取得(動的変化のない項目のみ)
                DefaultValueSet()
                If returnCode <> C_MESSAGENO.NORMAL Then
                    Return
                End If
                '****************************************
                'フォーカス設定
                '****************************************
                Me.txtTransferer.Focus()
                '****************************************
                'セッション設定
                '****************************************
                'HttpContext.Current.Session(CONST_BASEID & "_START") = CONST_MAPID
                '****************************************
                '戻りURL設定
                '****************************************
                Me.hdnBreakerViewUrl.Value = GetBreakerUrl()

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
                ' Help表示
                '**********************
                If Me.hdnHelpChange.Value IsNot Nothing AndAlso Me.hdnHelpChange.Value <> "" Then
                    DivShowHelp_DoubleClick(CONST_MAPID)
                    Me.hdnHelpChange.Value = ""
                End If

                ' メッセージ
                Select Case Left(returnCode, 1)
                    Case "0"
                        '0は個別で出力
                    Case "1"
                        CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.INFORMATION, pageObject:=Me)
                    Case Else
                        CommonFunctions.ShowMessage(C_MESSAGENO.EXCEPTION, Me.lblFooterMessage, naeiw:=C_NAEIW.ABNORMAL, pageObject:=Me)
                End Select

            End If

            ' ボタン活性制御
            DispSet()

            '****************************************
            '何も問題なく最後まで到達した処理
            '****************************************
            hdnSubmit.Value = "FALSE" 'サブミット可能にするためFalseを設定
        Catch ex As Threading.ThreadAbortException

        Catch ex As Exception
            Dim messageNo As String = C_MESSAGENO.EXCEPTION
            CommonFunctions.ShowMessage(messageNo, Me.lblFooterMessage)

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

            End Select
        End If

    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    Private Sub DispSet()

        ' 付け替えボタンの活性化制御
        If Me.hdnChkTransferee.Value <> "" AndAlso Me.hdnChkTransferer.Value <> "" Then
            Me.btnEnter.Disabled = False
        Else
            Me.btnEnter.Disabled = True
        End If

    End Sub

    ''' <summary>
    ''' チェックボタン押下時
    ''' </summary>
    Public Sub btnCheck_Click()

        'チェック処理
        checkProc()

    End Sub

    ''' <summary>
    ''' 実行ボタン押下時
    ''' </summary>
    Public Sub btnEnter_Click()

        'チェック処理
        checkProc()
        If Me.hdnChkTransferee.Value = "" OrElse Me.hdnChkTransferer.Value = "" Then
            returnCode = C_MESSAGENO.NOENTRYDATA
            Return
        End If

        ' データ更新
        updateOrder()

    End Sub
    ''' <summary>
    ''' 終了ボタン押下時
    ''' </summary>
    Public Sub btnBack_Click()
        Dim COA0011ReturnUrl As New BASEDLL.COA0011ReturnUrl

        '画面戻先URL取得
        COA0011ReturnUrl.MAPID = CONST_MAPID
        COA0011ReturnUrl.VARI = Convert.ToString(HttpContext.Current.Session("MAPvariant"))
        COA0011ReturnUrl.COA0011GetReturnUrl()
        If COA0011ReturnUrl.ERR = C_MESSAGENO.NORMAL Then
            lblTitleText.Text = COA0011ReturnUrl.NAMES
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

        'ラベル等やグリッドを除く文言設定(適宜追加) リピーターの表ヘッダーもこの方式で可能ですので
        '作成者に聞いてください。
        AddLangSetting(dicDisplayText, Me.btnCheck, "チェック", "Check")
        AddLangSetting(dicDisplayText, Me.btnEnter, "付け替え", "Transfer")
        AddLangSetting(dicDisplayText, Me.btnBack, "終了", "Exit")
        AddLangSetting(dicDisplayText, Me.lblTransferer, "移行元 B/L No.", "Transferer")
        AddLangSetting(dicDisplayText, Me.lblTransferee, "移行先 B/L No.", "Transferee")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonSel, "　選　択　", "Select")
        AddLangSetting(dicDisplayText, Me.btnLeftBoxButtonCan, "キャンセル", "Cancel")
        AddLangSetting(dicDisplayText, Me.rbShowMemo, "メモ", "Memo")
        AddLangSetting(dicDisplayText, Me.rbShowError, "エラー詳細", "Error Information")

        '上記で設定したオブジェクトの文言を変更
        SetDisplayLangObjects(dicDisplayText, lang)
    End Sub

    ''' <summary>
    ''' 初期表示
    ''' </summary>
    Public Sub DefaultValueSet()

        'RightBox情報設定
        rightBoxSet()
        If returnCode <> C_MESSAGENO.NORMAL Then
            Return
        End If

    End Sub

    ''' <summary>
    ''' 右ボックス設定
    ''' </summary>
    Public Sub rightBoxSet()

        '特になし

    End Sub
    ''' <summary>
    ''' チェック処理
    ''' </summary>
    Public Sub CheckProc()
        Dim COA0008InvalidChar As New BASEDLL.COA0008InvalidChar              '例外文字排除 String Get

        '単項目チェック
        Dim singleCheckTextObjList As New Dictionary(Of String, TextBox) From {
                                                                               {"TRANSFERER", txtTransferer},
                                                                               {"TRANSFEREE", txtTransferee}
                                                                              }
        For Each txtObj As KeyValuePair(Of String, TextBox) In singleCheckTextObjList
            Dim chkVal As String = txtObj.Value.Text
            CheckSingle(txtObj.Key, chkVal)
            If returnCode <> C_MESSAGENO.NORMAL Then
                txtObj.Value.Focus()
                '   チェック結果初期化
                Me.hdnChkTransferer.Value = ""
                Me.hdnChkTransferee.Value = ""
                Me.lblTransfererOdrText.Text = "none"
                Me.lblTransfererBrText.Text = "none"
                Me.lblTransfereeOdrText.Text = "none"
                Me.lblTransfereeBrText.Text = "none"

                Return
            End If
        Next

        '移行元チェック
        CheckTransferer(Me.txtTransferer.Text)

        '移行先チェック
        CheckTransferee(Me.txtTransferee.Text)

    End Sub
    ''' <summary>
    ''' 単項目チェック
    ''' </summary>
    ''' <param name="inColName"></param>
    ''' <param name="inText"></param>
    Protected Sub CheckSingle(ByVal inColName As String, ByVal inText As String)

        Dim COA0026FieldCheck As New BASEDLL.COA0026FieldCheck      '項目チェック

        '〇単項目チェック
        COA0026FieldCheck.COMPCODE = GBC_COMPCODE_D
        COA0026FieldCheck.MAPID = CONST_MAPID
        COA0026FieldCheck.FIELD = inColName
        COA0026FieldCheck.VALUE = inText
        COA0026FieldCheck.COA0026FieldCheck()
        If COA0026FieldCheck.ERR = C_MESSAGENO.NORMAL Then
        Else
            returnCode = COA0026FieldCheck.ERR
        End If

    End Sub

    ''' <summary>
    ''' B/L関連情報取得
    ''' </summary>
    ''' <param name="blid"></param>
    ''' <returns></returns>
    Private Function GetBLInfo(blid As String) As DataTable

        'SQL文の作成
        Dim sqlStat As New StringBuilder
        sqlStat.AppendLine("SELECT tr.BLID1,tr.ORDERNO,tr.DELFLG,tr.OBRID,tr.UPDYMD,tr.BBRID ")
        sqlStat.AppendLine("FROM (")
        sqlStat.AppendLine("  SELECT")
        sqlStat.AppendLine("       ROW_NUMBER() OVER(ORDER BY ob.DELFLG, ob.UPDYMD DESC) AS LINECNT,")
        sqlStat.AppendLine("       ob.BLID1,ob.ORDERNO,ob.DELFLG,ob.BRID AS OBRID,ob.UPDYMD,isnull(bb.BRID,'N') AS BBRID")
        sqlStat.AppendFormat("  FROM {0} ob", CONST_TBL_ODRB).AppendLine()
        sqlStat.AppendFormat("  LEFT OUTER JOIN {0} bb", CONST_TBL_BRB).AppendLine()
        sqlStat.AppendLine("    ON  bb.DELFLG = @DELFLG")
        sqlStat.AppendLine("    AND bb.BRID = ob.BRID")
        sqlStat.AppendLine("  WHERE ob.BLID1 = @BLID")
        sqlStat.AppendLine("  AND   ob.BLID1 <> ''")
        sqlStat.AppendLine(") tr")
        sqlStat.AppendLine("WHERE   tr.LINECNT = 1")

        Dim retDt As New DataTable
        Try
            Using SQLcon As New SqlConnection(COA0019Session.DBcon),
                  SQLcmd = New SqlCommand(sqlStat.ToString, SQLcon)
                'DataBase接続(Open)
                SQLcon.Open()
                With SQLcmd.Parameters
                    .Add("@BLID", System.Data.SqlDbType.NVarChar).Value = blid
                    .Add("@DELFLG", System.Data.SqlDbType.Char, 1).Value = CONST_FLAG_NO
                End With

                Using sqlDa As New SqlDataAdapter(SQLcmd)
                    sqlDa.Fill(retDt)
                End Using
            End Using 'SQLcon SQLcmd

        Catch ex As Exception
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
        End Try

        Return retDt
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="blid"></param>
    Private Sub CheckTransferer(blid As String)

        Me.hdnChkTransferer.Value = ""
        Me.lblTransfererOdrText.Text = ""
        Me.lblTransfererBrText.Text = ""
        If blid = "" Then
            Return
        End If

        Dim ChkTransferer As String = blid
        Using dt As DataTable = Me.GetBLInfo(blid)

            ' 例外 データ未取得
            If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
                Me.lblTransfererOdrText.Text = "none"
                Me.lblTransfererOdrText.CssClass = "colorRed"
                Me.lblTransfererBrText.Text = "none"
                Me.lblTransfererBrText.CssClass = "colorRed"
                Return
            End If

            Dim selectedDr As DataRow = dt.Rows(0)
            Me.lblTransfererOdrText.Text = Convert.ToString(selectedDr("ORDERNO"))
            If Convert.ToString(selectedDr("DELFLG")) = "Y" Then
                Me.lblTransfererOdrText.Text = Me.lblTransfererOdrText.Text & "(Deleted)"
                Me.lblTransfererOdrText.CssClass = ""
            Else
                Me.lblTransfererOdrText.Text = Me.lblTransfererOdrText.Text & "(To Be Deleted)"
                Me.lblTransfererOdrText.CssClass = "colorRed"
                ChkTransferer = ""
            End If

            Me.lblTransfererBrText.Text = Convert.ToString(selectedDr("OBRID"))
            If Convert.ToString(selectedDr("BBRID")) = "N" Then
                Me.lblTransfererBrText.Text = Me.lblTransfererBrText.Text & "(Deleted)"
                Me.lblTransfererBrText.CssClass = "colorRed"
                ChkTransferer = ""
            Else
                Me.lblTransfererBrText.Text = Me.lblTransfererBrText.Text
                Me.lblTransfererBrText.CssClass = ""
            End If

        End Using
        Me.hdnChkTransferer.Value = ChkTransferer

    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="blid"></param>
    Private Sub CheckTransferee(blid As String)

        ' 初期化
        Me.hdnChkTransferee.Value = ""
        Me.lblTransfereeOdrText.Text = ""
        Me.lblTransfereeBrText.Text = ""
        If blid = "" Then
            Return
        End If

        Dim ChkTransferee As String = blid
        Using dt As DataTable = Me.GetBLInfo(blid)

            ' 例外 データ未取得
            If dt IsNot Nothing AndAlso dt.Rows.Count = 0 Then
                Me.lblTransfereeOdrText.Text = "none"
                Me.lblTransfereeOdrText.CssClass = "colorRed"
                Me.lblTransfereeBrText.Text = "none"
                Me.lblTransfereeBrText.CssClass = "colorRed"
                Return
            End If

            Dim selectedDr As DataRow = dt.Rows(0)
            Me.lblTransfereeOdrText.Text = Convert.ToString(selectedDr("ORDERNO"))
            If Convert.ToString(selectedDr("DELFLG")) = "Y" Then
                Me.lblTransfereeOdrText.Text = Me.lblTransfereeOdrText.Text & "(Deleted)"
                Me.lblTransfereeOdrText.CssClass = "colorRed"
                ChkTransferee = ""
            Else
                Me.lblTransfereeOdrText.Text = Me.lblTransfereeOdrText.Text
                Me.lblTransfereeOdrText.CssClass = ""
            End If

            Me.lblTransfereeBrText.Text = Convert.ToString(selectedDr("OBRID"))
            If Convert.ToString(selectedDr("BBRID")) = "N" Then
                Me.lblTransfereeBrText.Text = Me.lblTransfereeBrText.Text & "(Deleted)"
                Me.lblTransfereeBrText.CssClass = "colorRed"
                ChkTransferee = ""
            Else
                Me.lblTransfereeBrText.Text = Me.lblTransfereeBrText.Text
                Me.lblTransfereeBrText.CssClass = ""
            End If

        End Using
        Me.hdnChkTransferee.Value = ChkTransferee

    End Sub

    '' <summary>
    '' 移行元 B/L No.設定
    '' </summary>
    Public Sub txtTransferer_Change()

        '移行元チェック
        CheckTransferer(Me.txtTransferer.Text)

    End Sub

    '' <summary>
    '' 移行先 B/L No.設定
    '' </summary>
    Public Sub txtTransferee_Change()

        '移行先チェック
        CheckTransferee(Me.txtTransferee.Text)

    End Sub

    ''' <summary>
    ''' B/L No.付け替え
    ''' </summary>
    Private Sub updateOrder()

        Dim procDateTime As DateTime = DateTime.Now
        Dim sqlStat As New StringBuilder

        ' Order baseを登録
        sqlStat.AppendFormat("INSERT INTO {0} (", CONST_TBL_ODRB).AppendLine()
        sqlStat.AppendLine("        ORDERNO")
        sqlStat.AppendLine("       ,STYMD")
        sqlStat.AppendLine("       ,ENDYMD")
        sqlStat.AppendLine("       ,BRID")
        sqlStat.AppendLine("       ,BRTYPE")
        sqlStat.AppendLine("       ,VALIDITYFROM")
        sqlStat.AppendLine("       ,VALIDITYTO")
        sqlStat.AppendLine("       ,TERMTYPE")
        sqlStat.AppendLine("       ,NOOFTANKS")
        sqlStat.AppendLine("       ,SHIPPER")
        sqlStat.AppendLine("       ,CONSIGNEE")
        sqlStat.AppendLine("       ,CARRIER1")
        sqlStat.AppendLine("       ,CARRIER2")
        sqlStat.AppendLine("       ,PRODUCTCODE")
        sqlStat.AppendLine("       ,PRODUCTWEIGHT")
        sqlStat.AppendLine("       ,RECIEPTCOUNTRY1")
        sqlStat.AppendLine("       ,RECIEPTPORT1")
        sqlStat.AppendLine("       ,RECIEPTCOUNTRY2")
        sqlStat.AppendLine("       ,RECIEPTPORT2")
        sqlStat.AppendLine("       ,LOADCOUNTRY1")
        sqlStat.AppendLine("       ,LOADPORT1")
        sqlStat.AppendLine("       ,LOADCOUNTRY2")
        sqlStat.AppendLine("       ,LOADPORT2")
        sqlStat.AppendLine("       ,DISCHARGECOUNTRY1")
        sqlStat.AppendLine("       ,DISCHARGEPORT1")
        sqlStat.AppendLine("       ,DISCHARGECOUNTRY2")
        sqlStat.AppendLine("       ,DISCHARGEPORT2")
        sqlStat.AppendLine("       ,DELIVERYCOUNTRY1")
        sqlStat.AppendLine("       ,DELIVERYPORT1")
        sqlStat.AppendLine("       ,DELIVERYCOUNTRY2")
        sqlStat.AppendLine("       ,DELIVERYPORT2")
        sqlStat.AppendLine("       ,VSL1")
        sqlStat.AppendLine("       ,VOY1")
        sqlStat.AppendLine("       ,ETD1")
        sqlStat.AppendLine("       ,ETA1")
        sqlStat.AppendLine("       ,VSL2")
        sqlStat.AppendLine("       ,VOY2")
        sqlStat.AppendLine("       ,ETD2")
        sqlStat.AppendLine("       ,ETA2")
        sqlStat.AppendLine("       ,INVOICEDBY")
        sqlStat.AppendLine("       ,LOADING")
        sqlStat.AppendLine("       ,STEAMING")
        sqlStat.AppendLine("       ,TIP")
        sqlStat.AppendLine("       ,EXTRA")
        sqlStat.AppendLine("       ,DEMURTO")
        sqlStat.AppendLine("       ,DEMURUSRATE1")
        sqlStat.AppendLine("       ,DEMURUSRATE2")
        sqlStat.AppendLine("       ,SALESPIC")
        sqlStat.AppendLine("       ,AGENTORGANIZER")
        sqlStat.AppendLine("       ,AGENTPOL1")
        sqlStat.AppendLine("       ,AGENTPOL2")
        sqlStat.AppendLine("       ,AGENTPOD1")
        sqlStat.AppendLine("       ,AGENTPOD2")
        sqlStat.AppendLine("       ,USINGLEASETANK")
        sqlStat.AppendLine("       ,BLID1")
        sqlStat.AppendLine("       ,BLAPPDATE1")
        sqlStat.AppendLine("       ,BLID2")
        sqlStat.AppendLine("       ,BLAPPDATE2")
        sqlStat.AppendLine("       ,SHIPPERNAME")
        sqlStat.AppendLine("       ,SHIPPERTEXT")
        sqlStat.AppendLine("       ,SHIPPERTEXT2")
        sqlStat.AppendLine("       ,CONSIGNEENAME")
        sqlStat.AppendLine("       ,CONSIGNEETEXT")
        sqlStat.AppendLine("       ,CONSIGNEETEXT2")
        sqlStat.AppendLine("       ,IECCODE")
        sqlStat.AppendLine("       ,NOTIFYNAME")
        sqlStat.AppendLine("       ,NOTIFYTEXT")
        sqlStat.AppendLine("       ,NOTIFYTEXT2")
        sqlStat.AppendLine("       ,NOTIFYCONT")
        sqlStat.AppendLine("       ,NOTIFYCONTNAME")
        sqlStat.AppendLine("       ,NOTIFYCONTTEXT1")
        sqlStat.AppendLine("       ,NOTIFYCONTTEXT2")
        sqlStat.AppendLine("       ,PRECARRIAGETEXT")
        sqlStat.AppendLine("       ,PRECARRIAGETEXT2")
        sqlStat.AppendLine("       ,VSL")
        sqlStat.AppendLine("       ,VOY")
        sqlStat.AppendLine("       ,FINDESTINATIONNAME")
        sqlStat.AppendLine("       ,FINDESTINATIONTEXT")
        sqlStat.AppendLine("       ,FINDESTINATIONTEXT2")
        sqlStat.AppendLine("       ,PRODUCT")
        sqlStat.AppendLine("       ,PRODUCTPORDER")
        sqlStat.AppendLine("       ,PRODUCTTIP")
        sqlStat.AppendLine("       ,PRODUCTFREIGHT")
        sqlStat.AppendLine("       ,FREIGHTANDCHARGES")
        sqlStat.AppendLine("       ,PREPAIDAT")
        sqlStat.AppendLine("       ,GOODSPKGS")
        sqlStat.AppendLine("       ,CONTAINERPKGS")
        sqlStat.AppendLine("       ,BLNUM")
        sqlStat.AppendLine("       ,CONTAINERNO")
        sqlStat.AppendLine("       ,SEALNO")
        sqlStat.AppendLine("       ,NOOFCONTAINER")
        sqlStat.AppendLine("       ,DECLAREDVALUE")
        sqlStat.AppendLine("       ,DECLAREDVALUE2")
        sqlStat.AppendLine("       ,REVENUETONS")
        sqlStat.AppendLine("       ,REVENUETONS2")
        sqlStat.AppendLine("       ,RATE")
        sqlStat.AppendLine("       ,RATE2")
        sqlStat.AppendLine("       ,PER")
        sqlStat.AppendLine("       ,PER2")
        sqlStat.AppendLine("       ,PREPAID")
        sqlStat.AppendLine("       ,PREPAID2")
        sqlStat.AppendLine("       ,COLLECT")
        sqlStat.AppendLine("       ,COLLECT2")
        sqlStat.AppendLine("       ,EXCHANGERATE")
        sqlStat.AppendLine("       ,PAYABLEAT")
        sqlStat.AppendLine("       ,LOCALCURRENCY")
        sqlStat.AppendLine("       ,CARRIERBLNO")
        sqlStat.AppendLine("       ,CARRIERBLNO2")
        sqlStat.AppendLine("       ,BOOKINGNO")
        sqlStat.AppendLine("       ,BOOKINGNO2")
        sqlStat.AppendLine("       ,NOOFPACKAGE")
        sqlStat.AppendLine("       ,BLTYPE")
        sqlStat.AppendLine("       ,BLTYPE2")
        sqlStat.AppendLine("       ,NOOFBL")
        sqlStat.AppendLine("       ,NOOFBL2")
        sqlStat.AppendLine("       ,PAYMENTPLACE")
        sqlStat.AppendLine("       ,PAYMENTPLACE2")
        sqlStat.AppendLine("       ,BLISSUEPLACE")
        sqlStat.AppendLine("       ,BLISSUEPLACE2")
        sqlStat.AppendLine("       ,ANISSUEPLACE")
        sqlStat.AppendLine("       ,ANISSUEPLACE2")
        sqlStat.AppendLine("       ,MEASUREMENT")
        sqlStat.AppendLine("       ,MEASUREMENT2")
        sqlStat.AppendLine("       ,MARKSANDNUMBERS")
        sqlStat.AppendLine("       ,TANKINFO")
        sqlStat.AppendLine("       ,LDNVSL1")
        sqlStat.AppendLine("       ,LDNPOL1")
        sqlStat.AppendLine("       ,LDNDATE1")
        sqlStat.AppendLine("       ,LDNBY1")
        sqlStat.AppendLine("       ,LDNVSL2")
        sqlStat.AppendLine("       ,LDNPOL2")
        sqlStat.AppendLine("       ,LDNDATE2")
        sqlStat.AppendLine("       ,LDNBY2")
        sqlStat.AppendLine("       ,CARRIERBLTYPE")
        sqlStat.AppendLine("       ,CARRIERBLTYPE2")
        sqlStat.AppendLine("       ,DEMUFORACCT")
        sqlStat.AppendLine("       ,DEMUFORACCT2")
        sqlStat.AppendLine("       ,BLRECEIPT1")
        sqlStat.AppendLine("       ,BLRECEIPT2")
        sqlStat.AppendLine("       ,BLLOADING1")
        sqlStat.AppendLine("       ,BLLOADING2")
        sqlStat.AppendLine("       ,BLDISCHARGE1")
        sqlStat.AppendLine("       ,BLDISCHARGE2")
        sqlStat.AppendLine("       ,BLDELIVERY1")
        sqlStat.AppendLine("       ,BLDELIVERY2")
        sqlStat.AppendLine("       ,BLPLACEDATEISSUE1")
        sqlStat.AppendLine("       ,BLPLACEDATEISSUE2")
        sqlStat.AppendLine("       ,REMARK")
        sqlStat.AppendLine("       ,DELFLG")
        sqlStat.AppendLine("       ,INITYMD")
        sqlStat.AppendLine("       ,INITUSER")
        sqlStat.AppendLine("       ,UPDYMD")
        sqlStat.AppendLine("       ,UPDUSER")
        sqlStat.AppendLine("       ,UPDTERMID")
        sqlStat.AppendLine("       ,RECEIVEYMD")
        sqlStat.AppendLine(" )")
        sqlStat.AppendLine(" SELECT")
        sqlStat.AppendLine("        ORDERNO")
        sqlStat.AppendLine("       ,STYMD")
        sqlStat.AppendLine("       ,ENDYMD")
        sqlStat.AppendLine("       ,BRID")
        sqlStat.AppendLine("       ,BRTYPE")
        sqlStat.AppendLine("       ,VALIDITYFROM")
        sqlStat.AppendLine("       ,VALIDITYTO")
        sqlStat.AppendLine("       ,TERMTYPE")
        sqlStat.AppendLine("       ,NOOFTANKS")
        sqlStat.AppendLine("       ,SHIPPER")
        sqlStat.AppendLine("       ,CONSIGNEE")
        sqlStat.AppendLine("       ,CARRIER1")
        sqlStat.AppendLine("       ,CARRIER2")
        sqlStat.AppendLine("       ,PRODUCTCODE")
        sqlStat.AppendLine("       ,PRODUCTWEIGHT")
        sqlStat.AppendLine("       ,RECIEPTCOUNTRY1")
        sqlStat.AppendLine("       ,RECIEPTPORT1")
        sqlStat.AppendLine("       ,RECIEPTCOUNTRY2")
        sqlStat.AppendLine("       ,RECIEPTPORT2")
        sqlStat.AppendLine("       ,LOADCOUNTRY1")
        sqlStat.AppendLine("       ,LOADPORT1")
        sqlStat.AppendLine("       ,LOADCOUNTRY2")
        sqlStat.AppendLine("       ,LOADPORT2")
        sqlStat.AppendLine("       ,DISCHARGECOUNTRY1")
        sqlStat.AppendLine("       ,DISCHARGEPORT1")
        sqlStat.AppendLine("       ,DISCHARGECOUNTRY2")
        sqlStat.AppendLine("       ,DISCHARGEPORT2")
        sqlStat.AppendLine("       ,DELIVERYCOUNTRY1")
        sqlStat.AppendLine("       ,DELIVERYPORT1")
        sqlStat.AppendLine("       ,DELIVERYCOUNTRY2")
        sqlStat.AppendLine("       ,DELIVERYPORT2")
        sqlStat.AppendLine("       ,VSL1")
        sqlStat.AppendLine("       ,VOY1")
        sqlStat.AppendLine("       ,ETD1")
        sqlStat.AppendLine("       ,ETA1")
        sqlStat.AppendLine("       ,VSL2")
        sqlStat.AppendLine("       ,VOY2")
        sqlStat.AppendLine("       ,ETD2")
        sqlStat.AppendLine("       ,ETA2")
        sqlStat.AppendLine("       ,INVOICEDBY")
        sqlStat.AppendLine("       ,LOADING")
        sqlStat.AppendLine("       ,STEAMING")
        sqlStat.AppendLine("       ,TIP")
        sqlStat.AppendLine("       ,EXTRA")
        sqlStat.AppendLine("       ,DEMURTO")
        sqlStat.AppendLine("       ,DEMURUSRATE1")
        sqlStat.AppendLine("       ,DEMURUSRATE2")
        sqlStat.AppendLine("       ,SALESPIC")
        sqlStat.AppendLine("       ,AGENTORGANIZER")
        sqlStat.AppendLine("       ,AGENTPOL1")
        sqlStat.AppendLine("       ,AGENTPOL2")
        sqlStat.AppendLine("       ,AGENTPOD1")
        sqlStat.AppendLine("       ,AGENTPOD2")
        sqlStat.AppendLine("       ,USINGLEASETANK")
        sqlStat.AppendLine("       ,@OLDBLID")          '移行元B/LNo.
        sqlStat.AppendLine("       ,BLAPPDATE1")
        sqlStat.AppendLine("       ,BLID2")
        sqlStat.AppendLine("       ,BLAPPDATE2")
        sqlStat.AppendLine("       ,SHIPPERNAME")
        sqlStat.AppendLine("       ,SHIPPERTEXT")
        sqlStat.AppendLine("       ,SHIPPERTEXT2")
        sqlStat.AppendLine("       ,CONSIGNEENAME")
        sqlStat.AppendLine("       ,CONSIGNEETEXT")
        sqlStat.AppendLine("       ,CONSIGNEETEXT2")
        sqlStat.AppendLine("       ,IECCODE")
        sqlStat.AppendLine("       ,NOTIFYNAME")
        sqlStat.AppendLine("       ,NOTIFYTEXT")
        sqlStat.AppendLine("       ,NOTIFYTEXT2")
        sqlStat.AppendLine("       ,NOTIFYCONT")
        sqlStat.AppendLine("       ,NOTIFYCONTNAME")
        sqlStat.AppendLine("       ,NOTIFYCONTTEXT1")
        sqlStat.AppendLine("       ,NOTIFYCONTTEXT2")
        sqlStat.AppendLine("       ,PRECARRIAGETEXT")
        sqlStat.AppendLine("       ,PRECARRIAGETEXT2")
        sqlStat.AppendLine("       ,VSL")
        sqlStat.AppendLine("       ,VOY")
        sqlStat.AppendLine("       ,FINDESTINATIONNAME")
        sqlStat.AppendLine("       ,FINDESTINATIONTEXT")
        sqlStat.AppendLine("       ,FINDESTINATIONTEXT2")
        sqlStat.AppendLine("       ,PRODUCT")
        sqlStat.AppendLine("       ,PRODUCTPORDER")
        sqlStat.AppendLine("       ,PRODUCTTIP")
        sqlStat.AppendLine("       ,PRODUCTFREIGHT")
        sqlStat.AppendLine("       ,FREIGHTANDCHARGES")
        sqlStat.AppendLine("       ,PREPAIDAT")
        sqlStat.AppendLine("       ,GOODSPKGS")
        sqlStat.AppendLine("       ,CONTAINERPKGS")
        sqlStat.AppendLine("       ,BLNUM")
        sqlStat.AppendLine("       ,CONTAINERNO")
        sqlStat.AppendLine("       ,SEALNO")
        sqlStat.AppendLine("       ,NOOFCONTAINER")
        sqlStat.AppendLine("       ,DECLAREDVALUE")
        sqlStat.AppendLine("       ,DECLAREDVALUE2")
        sqlStat.AppendLine("       ,REVENUETONS")
        sqlStat.AppendLine("       ,REVENUETONS2")
        sqlStat.AppendLine("       ,RATE")
        sqlStat.AppendLine("       ,RATE2")
        sqlStat.AppendLine("       ,PER")
        sqlStat.AppendLine("       ,PER2")
        sqlStat.AppendLine("       ,PREPAID")
        sqlStat.AppendLine("       ,PREPAID2")
        sqlStat.AppendLine("       ,COLLECT")
        sqlStat.AppendLine("       ,COLLECT2")
        sqlStat.AppendLine("       ,EXCHANGERATE")
        sqlStat.AppendLine("       ,PAYABLEAT")
        sqlStat.AppendLine("       ,LOCALCURRENCY")
        sqlStat.AppendLine("       ,CARRIERBLNO")
        sqlStat.AppendLine("       ,CARRIERBLNO2")
        sqlStat.AppendLine("       ,BOOKINGNO")
        sqlStat.AppendLine("       ,BOOKINGNO2")
        sqlStat.AppendLine("       ,NOOFPACKAGE")
        sqlStat.AppendLine("       ,BLTYPE")
        sqlStat.AppendLine("       ,BLTYPE2")
        sqlStat.AppendLine("       ,NOOFBL")
        sqlStat.AppendLine("       ,NOOFBL2")
        sqlStat.AppendLine("       ,PAYMENTPLACE")
        sqlStat.AppendLine("       ,PAYMENTPLACE2")
        sqlStat.AppendLine("       ,BLISSUEPLACE")
        sqlStat.AppendLine("       ,BLISSUEPLACE2")
        sqlStat.AppendLine("       ,ANISSUEPLACE")
        sqlStat.AppendLine("       ,ANISSUEPLACE2")
        sqlStat.AppendLine("       ,MEASUREMENT")
        sqlStat.AppendLine("       ,MEASUREMENT2")
        sqlStat.AppendLine("       ,MARKSANDNUMBERS")
        sqlStat.AppendLine("       ,TANKINFO")
        sqlStat.AppendLine("       ,LDNVSL1")
        sqlStat.AppendLine("       ,LDNPOL1")
        sqlStat.AppendLine("       ,LDNDATE1")
        sqlStat.AppendLine("       ,LDNBY1")
        sqlStat.AppendLine("       ,LDNVSL2")
        sqlStat.AppendLine("       ,LDNPOL2")
        sqlStat.AppendLine("       ,LDNDATE2")
        sqlStat.AppendLine("       ,LDNBY2")
        sqlStat.AppendLine("       ,CARRIERBLTYPE")
        sqlStat.AppendLine("       ,CARRIERBLTYPE2")
        sqlStat.AppendLine("       ,DEMUFORACCT")
        sqlStat.AppendLine("       ,DEMUFORACCT2")
        sqlStat.AppendLine("       ,BLRECEIPT1")
        sqlStat.AppendLine("       ,BLRECEIPT2")
        sqlStat.AppendLine("       ,BLLOADING1")
        sqlStat.AppendLine("       ,BLLOADING2")
        sqlStat.AppendLine("       ,BLDISCHARGE1")
        sqlStat.AppendLine("       ,BLDISCHARGE2")
        sqlStat.AppendLine("       ,BLDELIVERY1")
        sqlStat.AppendLine("       ,BLDELIVERY2")
        sqlStat.AppendLine("       ,BLPLACEDATEISSUE1")
        sqlStat.AppendLine("       ,BLPLACEDATEISSUE2")
        sqlStat.AppendLine("       ,REMARK")
        sqlStat.AppendLine("       ,@DELFLG_N")
        sqlStat.AppendLine("       ,@UPDYMD")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDYMD")
        sqlStat.AppendLine("       ,@UPDUSER")
        sqlStat.AppendLine("       ,@UPDTERMID")
        sqlStat.AppendLine("       ,@RECEIVEYMD")
        sqlStat.AppendFormat("FROM {0} ", CONST_TBL_ODRB).AppendLine()
        sqlStat.AppendLine(" WHERE BLID1       = @NEWBLID")
        sqlStat.AppendLine("  AND  DELFLG      = @DELFLG_N;")
        ' Order base(旧B/L No.)を論理削除
        sqlStat.AppendFormat("UPDATE {0} ", CONST_TBL_ODRB).AppendLine()
        sqlStat.AppendLine("  SET    DELFLG     = @DELFLG_Y")
        sqlStat.AppendLine("        ,UPDYMD     = @UPDYMD")
        sqlStat.AppendLine("        ,UPDUSER    = @UPDUSER")
        sqlStat.AppendLine("        ,UPDTERMID  = @UPDTERMID")
        sqlStat.AppendLine("        ,RECEIVEYMD = @RECEIVEYMD")
        sqlStat.AppendLine("  WHERE BLID1       = @NEWBLID")
        sqlStat.AppendLine("   AND  DELFLG      = @DELFLG_N;")

        Dim sqlCon As System.Data.SqlClient.SqlConnection = Nothing
        Dim tran As System.Data.SqlClient.SqlTransaction = Nothing
        Try

            sqlCon = New SqlConnection(COA0019Session.DBcon)
            sqlCon.Open() '接続オープン
            tran = sqlCon.BeginTransaction

            Using sqlCmd As New SqlCommand(sqlStat.ToString, sqlCon)

                sqlCmd.Transaction = tran

                'SQLパラメータの設定
                With sqlCmd.Parameters
                    .Add("@OLDBLID", SqlDbType.NVarChar).Value = Me.hdnChkTransferer.Value  '移行元
                    .Add("@NEWBLID", SqlDbType.NVarChar).Value = Me.hdnChkTransferee.Value  '移行先
                    .Add("@DELFLG_Y", SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_YES
                    .Add("@DELFLG_N", SqlDbType.NVarChar).Value = BaseDllCommon.CONST_FLAG_NO
                    .Add("@UPDYMD", SqlDbType.NVarChar).Value = procDateTime.ToString("yyyy/MM/dd HH:mm:ss.FFF")
                    .Add("@UPDUSER", SqlDbType.NVarChar).Value = COA0019Session.USERID
                    .Add("@UPDTERMID", SqlDbType.NVarChar).Value = HttpContext.Current.Session("APSRVname")
                    .Add("@RECEIVEYMD", SqlDbType.DateTime).Value = CONST_DEFAULT_RECEIVEYMD
                End With

                sqlCmd.ExecuteNonQuery()

            End Using
            tran.Commit()
            returnCode = C_MESSAGENO.NORMALDBENTRY
            CommonFunctions.ShowMessage(returnCode, Me.lblFooterMessage, naeiw:=C_NAEIW.NORMAL, pageObject:=Me)

        Catch ex As Exception
            tran.Rollback()
            returnCode = C_MESSAGENO.EXCEPTION
            COA0003LogFile.RUNKBN = C_RUNKBN.ONLINE
            COA0003LogFile.NIWEA = C_NAEIW.ABNORMAL
            COA0003LogFile.TEXT = ex.ToString()
            COA0003LogFile.MESSAGENO = returnCode
            COA0003LogFile.COA0003WriteLog()
            Throw
        Finally
            If sqlCon IsNot Nothing Then
                tran.Dispose()
                sqlCon.Dispose()
                sqlCon = Nothing
            End If

        End Try

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
        '画面遷移実行
        brUrl = COA0012DoUrl.URL
        brUrl = VirtualPathUtility.ToAbsolute(brUrl) 'チルダURLから相対URLに変換
        Dim brUriObj As New Uri(Request.Url, brUrl) 'アプリルートURL+相対URL
        Return brUriObj.AbsoluteUri 'フルURLを返却(相対URLだとCHROMEではワークしない)
    End Function

End Class